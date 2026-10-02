// Varlik.cs
// Binadaki VARLIK (#86): gerilim kuralını (GerilimKurali) her kare sürer, kuralın istediği tepkiyi
// sahnede gösterir ve 3. kademede AVI yönetir.
//
// Şahan'ın isteği (2026-10-01): 1-5. katlarda tehdit yoktu; şimdi her katta oyuncunun gürültüsü
// ve oyalanması binayı kızdırır. Kademeler:
//   1. huzursuz: kafa lambası ve dairenin lambaları titrer, müzik kabarır; sonra üst kattan adımlar
//                ya da duvara odada olmayan birinin gölgesi düşer (GolgeIzi, #93).
//   2. yakın:    Gezen Gölge belirir (#71'in bileşeni); sonra kapı tıklatılır, lamba söner.
//   3. AV:       Gölge oyuncunun arkasında, görmediği bir yerde belirir ve oyuncunun geçtiği yoldan
//                (iz) gelir. Yakalarsa oyuncu katın başında uyanır (kilitler kalır, elindeki düşer:
//                KatYoneticisi.AvdaYakalandi). Oyuncu çömelip lambayı kapatır ve Gölge onu 2,5 sn
//                göremezse izi kaybeder; 18 saniyede yakalayamazsa da vazgeçer.
//
// Neden izi sürüyor da doğrudan gelmiyor? Dairede yol bulma (navmesh) yok; oyuncunun geçtiği
// noktalar zaten kapılardan geçen bir yoldur (7. katın kaçışı da böyle, KacisAni).
//
// Bileşen KatYoneticisi'nin nesnesinde durur (GezenGolge gibi): daire yeniden kurulsa da yok olmaz.
// Gürültü kaynakları (kavanoz, yanlış kapı, devrilen kapı, kol) Varlik.Gurultu(miktar) der; ne
// olduğunu bilmek zorunda değildir (IEtkilesimli ilkesi: nesne kendini anlatır, Varlık dinler).
//
// Kapali (static): oyun botu ve kat denetimi kuralı kapatır; ışınlanarak gezen bir test oyuncusu
// gerçek bir oyuncu gibi ısınmamalı.
//
// Gezen Gölge'nin kendi saati yok (Şahan, 2026-10-01): Gölge yalnızca buradan (2. kademe) ve yanlış
// kapı çalınınca (SahanlikKapisi) gelir.
//
// DOLAPTA SAKLANAN OYUNCU (#97, SaklanmaYeri): oyuncu bir gardıroba girince
//   - gerilim kuralı onu tam gizlenmiş sayar (dairede, lamba kapalı, eğik, kıpırtısız): gerilim en
//     yavaş hızla (SessizSogma) soğur;
//   - iz bırakmaz: Gölge izin sonuna, yani oyuncunun dolaba girdiği yere kadar gelir;
//   - görülmez (Goruyor false) ve yakalanmaz: Gölge dolabın önüne 0,8 m'ye kadar gelse de yakalama
//     sınaması atlanır. Gölge orada durur, bakınır ve GizlenmeSuresi dolunca "izi kaybetti" der.
// Oyuncunun kontrolcüsü saklanırken kapalıdır; Etkin() buna rağmen çalışır, yoksa Gölge dolabın
// önünde donup kalır, av hiç bitmezdi.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Varlik : MonoBehaviour
{
    [SerializeField] private GameObject sablon;          // Kacis_Golgesi şablonu
    [SerializeField] private AudioClip haberci;          // üst kattan ağır adımlar (Olay_UstKatAdimlari)
    [SerializeField] private AudioClip tiklatma;         // kapı tıklatma (Kapi_Tiklatma)

    /// <summary>Testler için: true iken kural işlemez (OyunBotu, KatDenetimi).</summary>
    public static bool Kapali;

    /// <summary>Sahnedeki tek Varlık (gürültü kaynakları buradan ulaşır).</summary>
    public static Varlik Ornek { get; private set; }

    private const float IzAraligi = 0.25f;               // oyuncunun yolu bu sıklıkla kaydedilir
    private const float IzSuresi = 12f;                  // bu kadar saniyelik iz tutulur
    private const float YakalamaMesafesi = 0.8f;
    private const float AdimAraligi = 0.5f;
    private const float BasYuksekligi = 1.85f;
    private const float GormeMesafesi = 9f;
    private const float DaireKontrolAraligi = 0.25f;

    private GerilimKurali kural;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private CharacterController govde;
    private KafaLambasi lamba;
    private Transform oyuncu;
    private Camera kamera;
    private int kat = -1;

    private readonly List<Vector3> izler = new List<Vector3>();
    private readonly List<float> izZamanlari = new List<float>();
    private float izSayaci;

    private float daireSayaci;
    private bool dairede;

    private GameObject golge;                            // av sürerken sahnedeki Gölge
    private static AudioClip adim;

    /// <summary>Av şu an sürüyor mu (GezenGolge bu sırada belirmez)?</summary>
    public bool AvSuruyor { get { return kural != null && kural.AvSuruyor; } }
    /// <summary>Gerilim 0-1 (test göstergesi için).</summary>
    public float Gerilim { get { return kural != null ? kural.Deger : 0f; } }
    public int Kademe { get { return kural != null ? kural.Kademe : 0; } }
    /// <summary>Av sürerken sahnedeki Gölge (yoksa null). SaklanmaYeri: Gölge yakındayken dolaba girilmez.</summary>
    public Transform AvGolgesi { get { return golge != null ? golge.transform : null; } }

    private void Awake() { Ornek = this; }
    private void OnDestroy() { if (Ornek == this) Ornek = null; }

    private void Start()
    {
        kural = new GerilimKurali(System.Environment.TickCount);
        yonetici = GetComponent<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrol != null)
        {
            oyuncu = kontrol.transform;
            govde = kontrol.GetComponent<CharacterController>();
            kamera = kontrol.GetComponentInChildren<Camera>();
        }
        lamba = FindAnyObjectByType<KafaLambasi>();
        if (adim == null) adim = Resources.Load<AudioClip>("Sesler/Adim_1");
    }

    /// <summary>Gürültü oldu: gerilim bu kadar yükselir (0,02 çekmece, 0,25 kavanoz, 0,3 yanlış kapı).</summary>
    public static void Gurultu(float miktar)
    {
        if (Ornek == null || Kapali || Ornek.kural == null) return;
        Ornek.kural.Gurultu(miktar);
    }

    private void Update()
    {
        if (kural == null || yonetici == null || oyuncu == null) return;

        // Yeni kat: kural baştan kurulur, iz silinir.
        if (yonetici.MevcutKat != kat)
        {
            kat = yonetici.MevcutKat;
            kural.KatAyarla(kat);
            AvIptal();
            izler.Clear(); izZamanlari.Clear();
        }

        if (!Etkin()) return;
        IziKaydet();
        DairedeMi();

        // Dolapta saklanan oyuncu (#97) tam gizlenmiş sayılır: gerilim en yavaş hızla soğur.
        bool saklaniyor = SaklanmaYeri.Saklaniyor;
        bool hareketli = !saklaniyor && govde != null && new Vector3(govde.velocity.x, 0f, govde.velocity.z).magnitude > 0.1f;
        bool lambaAcik = !saklaniyor && lamba != null && lamba.IsikYaniyor;
        bool egik = saklaniyor || (kontrol != null && kontrol.Egik);
        GerilimTepkisi tepki = kural.Ilerlet(Time.deltaTime, saklaniyor || dairede, lambaAcik, egik, hareketli);
        if (tepki != GerilimTepkisi.Yok) Goster(tepki);
    }

    // Kural bu karede işlesin mi? GezenGolge.SayabilirMi ile aynı kapılar + senaryolu anlar.
    private bool Etkin()
    {
        if (Kapali || sablon == null) return false;
        if (!yonetici.enabled || !yonetici.GolgeSayabilir) return false;   // açılış, kapı açık, çatı, final
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        // Menü, not, şifre ekranı, karartma: kontrolcü kapalı. Dolapta saklanırken de kapalıdır ama kural
        // işlemeye devam eder (#97): gerilim soğur, Gölge dolabın önünde bakınıp vazgeçer.
        if (kontrol == null || (!kontrol.enabled && !SaklanmaYeri.Saklaniyor)) return false;
        if (Time.timeScale <= 0f) return false;
        foreach (KacisAni k in FindObjectsByType<KacisAni>()) if (k.Basladi) return false;       // 7. kat
        foreach (SaklanmaAni s in FindObjectsByType<SaklanmaAni>()) if (s.Suruyor) return false; // 6. kat
        return true;
    }

    // Oyuncunun yolu: 0,25 sn'de bir nokta; 12 sn'den eskisi atılır. Av sürerken GİZLENEN oyuncu
    // (çömelmiş, lambası kapalı) iz bırakmaz: Gölge son gördüğü yere gelir, oyuncuyu görmezse vazgeçer.
    private void IziKaydet()
    {
        izSayaci += Time.deltaTime;
        if (izSayaci < IzAraligi) return;
        izSayaci = 0f;
        if (SaklanmaYeri.Saklaniyor) return;   // dolaptaki oyuncu hiç iz bırakmaz (#97)
        if (kural.AvSuruyor && Gizli()) return;
        izler.Add(oyuncu.position);
        izZamanlari.Add(Time.time);
        // Av sürerken iz KIRPILMAZ (#103): Gölge izi sıra numarasıyla (hedefIz) yürür; baştan bir nokta
        // atılınca bütün noktalar bir sıra kayar ve Gölge'nin hedefi bir nokta ileri atlardı. Kırpma her
        // 0,25 sn'de olduğu için Gölge saniyede dört nokta atlıyor, kapı ağızlarında duvarlardan geçiyordu.
        // Av en çok 18 sn sürer; iz o kadar uzar, av bitince eski noktalar bir sonraki kayıtta atılır.
        if (kural.AvSuruyor) return;
        while (izZamanlari.Count > 0 && Time.time - izZamanlari[0] > IzSuresi) { izler.RemoveAt(0); izZamanlari.RemoveAt(0); }
    }

    // Oyuncu dairenin içinde mi? Ayağının altındaki zemin bir dairenin (DaireEksenleri) parçasıysa
    // evet (AyakSesi.DaireZemininde ile aynı yol). Her karede değil, 0,25 sn'de bir bakılır.
    private void DairedeMi()
    {
        daireSayaci += Time.deltaTime;
        if (daireSayaci < DaireKontrolAraligi) return;
        daireSayaci = 0f;
        RaycastHit enYakin = default; float enKisa = float.MaxValue; bool var = false;
        foreach (RaycastHit h in Physics.RaycastAll(oyuncu.position + Vector3.up * 0.3f, Vector3.down, 0.8f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(oyuncu)) continue;
            if (h.distance < enKisa) { enKisa = h.distance; enYakin = h; var = true; }
        }
        dairede = var && enYakin.transform.GetComponentInParent<DaireEksenleri>() != null;
    }

    // ---------- TEPKİLER ----------
    private void Goster(GerilimTepkisi tepki)
    {
        Debug.Log($"[VARLIK] {kat}. kat: gerilim {kural.Deger:0.00}, kademe {kural.Kademe}: {tepki}.");
        switch (tepki)
        {
            case GerilimTepkisi.Titreme:
                if (lamba != null) lamba.Titret(0.9f);
                foreach (DaireLambalari d in FindObjectsByType<DaireLambalari>()) d.Titret();
                ArkaMuzik.Ornek?.Kabar();
                break;
            case GerilimTepkisi.UstKatAdimlari:
                if (haberci != null) AudioSource.PlayClipAtPoint(haberci, oyuncu.position + Vector3.up * 3f, 0.8f);
                break;
            case GerilimTepkisi.Golge:
                // Önce üst kattan adımlar, sesin ortasında Gölge (#74 kuralı). Yer yoksa en azından kapı.
                GezenGolge gezen = GetComponent<GezenGolge>();
                if (gezen == null || !gezen.HaberciyleBelir()) KapiTiklat();
                break;
            case GerilimTepkisi.Tiklatma:
                KapiTiklat();
                break;
            case GerilimTepkisi.LambaSonme:
                if (lamba != null) lamba.Sondur(1.3f);
                break;
            case GerilimTepkisi.GolgeIzi:
                // Duvara odada olmayan birinin gölgesi düşer (#93). Lamba kapalıysa, gölgesizse ya da
                // önünde uygun bir duvar yoksa en azından lamba titrer.
                if (!GolgeIzi.Goster(sablon, lamba, oyuncu) && lamba != null) lamba.Titret(0.9f);
                break;
            case GerilimTepkisi.Av:
                StartCoroutine(Av());
                break;
        }
    }

    // Oyuncuya en yakın KAPALI oda kapısı tıklatılır: kapının arkasında biri var gibi.
    private void KapiTiklat()
    {
        if (tiklatma == null) return;
        IcKapi secilen = null; float enKisa = float.MaxValue;
        foreach (IcKapi k in FindObjectsByType<IcKapi>())
        {
            if (!k.gameObject.activeInHierarchy || k.Acik) continue;
            float m = Vector3.Distance(k.transform.position, oyuncu.position);
            if (m < enKisa) { enKisa = m; secilen = k; }
        }
        Vector3 yer = secilen != null ? secilen.transform.position + Vector3.up * 1.2f : oyuncu.position + oyuncu.forward * 3f + Vector3.up;
        AudioSource.PlayClipAtPoint(tiklatma, yer, 0.9f);
        if (lamba != null) lamba.Titret(0.6f);
    }

    // ---------- AV ----------
    private IEnumerator Av()
    {
        // İmza (#105): anahtarlık arkadan (Gölge izin gerisinden gelir), sonra üst kattan adımlar + lamba titrer.
        GolgeImzasi.Cal(oyuncu.position - oyuncu.forward * 4f + Vector3.up * 1.2f);
        if (haberci != null) AudioSource.PlayClipAtPoint(haberci, oyuncu.position + Vector3.up * 3f, 0.9f);
        if (lamba != null) lamba.Titret(2f);
        ArkaMuzik.Ornek?.Kabar();
        yield return new WaitForSeconds(2.5f);

        // Gölge, izin oyuncunun GÖRMEDİĞİ en eski noktasında belirir (arkasında). Oyuncu uzun süre
        // yerinde durduysa iz tek bir yığındır; o zaman arkasında boş bir yer aranır. O da yoksa
        // (bir köşeye sıkışmış) av bu seferlik olmaz ve kural sıfırlanır (takılı kalmasın).
        int bas = BaslangicIzi();
        Vector3 yer;
        if (bas >= 0) yer = izler[bas];
        else if (!ArkadaYerBul(out yer)) { Debug.Log("[VARLIK] av: Gölge'ye yer yok, vazgeçti."); AvIptal(); kural.AvBitti(); yield break; }
        if (!Etkin()) { AvIptal(); kural.AvBitti(); yield break; }
        if (bas < 0) { izler.Clear(); izZamanlari.Clear(); izler.Add(yer); izZamanlari.Add(Time.time); bas = 0; }
        Vector3 bakis = oyuncu.position - yer; bakis.y = 0f;
        golge = Instantiate(sablon, yer, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);
        golge.name = "Av_Golgesi";
        Debug.Log($"[VARLIK] {kat}. kat: AV başladı ({Vector3.Distance(yer, oyuncu.position):0.0} m arkadan).");

        float hiz = GerilimKurali.AvHizi * kural.KatCarpani;
        float adimSayaci = 0f, gormemeSayaci = 0f, baslangic = Time.time;
        int hedefIz = bas + 1;
        IcKapi[] kapilar = FindObjectsByType<IcKapi>();   // bir kez; her kare aramak pahalı
        while (golge != null)
        {
            // Kat kapısı açıldı: kaçtı. Menü/not açıkken Gölge bekler (zaman da durur).
            if (yonetici.KatKapisiAcikMi(kat)) { Debug.Log("[VARLIK] av: kat kapısı açıldı, kaçtı."); break; }
            if (!Etkin()) { yield return null; continue; }
            if (Time.time - baslangic > GerilimKurali.AvSuresi) { Debug.Log("[VARLIK] av: süre doldu, Gölge vazgeçti."); break; }

            // İzi sür; iz bitince doğrudan oyuncuya.
            float kalan = hiz * Time.deltaTime;
            while (kalan > 0f && hedefIz < izler.Count)
            {
                Vector3 fark = izler[hedefIz] - golge.transform.position;
                float m = fark.magnitude;
                if (m <= kalan) { golge.transform.position = izler[hedefIz]; kalan -= m; hedefIz++; }
                else { golge.transform.position += fark / m * kalan; kalan = 0f; }
                Yonel(fark);
            }
            YoldakiKapiyiAc(kapilar);
            bool goruyor = Goruyor();
            bool izBitti = hedefIz >= izler.Count;
            // İz bitti (oyuncunun son görüldüğü yer): oyuncuyu görüyorsa doğrudan üstüne yürür,
            // görmüyorsa durup bakınır (sayaç işler).
            if (izBitti && goruyor)
            {
                Vector3 fark = oyuncu.position - golge.transform.position;
                if (fark.magnitude > 0.01f) { golge.transform.position += fark.normalized * Mathf.Min(fark.magnitude, kalan); Yonel(fark); }
            }

            adimSayaci += Time.deltaTime;
            if (adimSayaci >= AdimAraligi && adim != null && !(izBitti && !goruyor)) { adimSayaci = 0f; AdimCal(golge.transform.position); }

            // Görmeme sayacı: Gölge izin sonuna (oyuncunun son görüldüğü yere) varmış ve onu
            // görmüyorsa işler; izi sürerken köşe arkasında kalması sayılmaz. Dolunca vazgeçer.
            // Gizlenen oyuncu iz bırakmadığı için (IziKaydet) Gölge saklandığı yere değil, saklanmaya
            // başladığı yere gelir ve orada bakınır.
            bool sayilir = izBitti && !goruyor;
            gormemeSayaci = sayilir ? gormemeSayaci + Time.deltaTime : 0f;
            if (gormemeSayaci >= GerilimKurali.GizlenmeSuresi) { Debug.Log("[VARLIK] av: Gölge izi kaybetti (saklandı)."); break; }

            // Yakalama. Dolapta saklanan oyuncu yakalanmaz (#97): Gölge dolabın önüne kadar gelse de
            // bu sınama atlanır; yukarıdaki görmeme sayacı onu vazgeçirir.
            Vector3 arada = oyuncu.position - golge.transform.position; arada.y = 0f;
            if (!SaklanmaYeri.Saklaniyor && arada.magnitude < YakalamaMesafesi && Mathf.Abs(oyuncu.position.y - golge.transform.position.y) < 1.2f)
            {
                Yakalandi();
                yield break;
            }
            yield return null;
        }
        // Vazgeçti ya da izi kaybetti: lamba titrer, ışık dönünce Gölge yok.
        if (lamba != null) lamba.Titret(0.8f);
        yield return new WaitForSeconds(0.15f);
        AvIptal();
        kural.AvBitti();
    }

    // İzin en eski noktasından başlayıp oyuncunun ekranında olmayan, en az 2,5 m uzaktaki ilk nokta.
    private int BaslangicIzi()
    {
        if (izler.Count == 0) return -1;
        for (int i = 0; i < izler.Count; i++)
        {
            if (Vector3.Distance(izler[i], oyuncu.position) < 2.5f) continue;
            if (!EkrandaMi(izler[i])) return i;
        }
        return Vector3.Distance(izler[0], oyuncu.position) >= 1.5f ? 0 : -1;
    }

    // Oyuncunun EKRANINDA OLMAYAN, 1,8-4 m uzakta, zemini olan, arada duvar olmayan bir nokta.
    // Önce tam arkası (±30°) denenir, sonra her yön: dar bir koridorda arkası duvar olabilir.
    private bool ArkadaYerBul(out Vector3 yer)
    {
        yer = Vector3.zero;
        Vector3 geri = kamera != null ? -kamera.transform.forward : -oyuncu.forward;
        geri.y = 0f;
        if (geri.sqrMagnitude < 0.01f) geri = -oyuncu.forward;
        geri.Normalize();
        float ayak = oyuncu.position.y;
        for (int deneme = 0; deneme < 48; deneme++)
        {
            float aci = deneme < 16 ? Random.Range(-30f, 30f) : Random.Range(-180f, 180f);
            Vector3 aday = oyuncu.position + Quaternion.Euler(0f, aci, 0f) * geri * Random.Range(1.8f, 4f);
            if (!Physics.Raycast(aday + Vector3.up * 1f, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Mathf.Abs(zemin.point.y - ayak) > 0.3f) continue;
            aday.y = zemin.point.y;
            if (EkrandaMi(aday)) continue;
            if (Physics.CheckCapsule(aday + Vector3.up * 0.35f, aday + Vector3.up * 1.6f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) continue;
            // Duvarın ardına düşmesin: oyuncuyla aday arasında engel olmasın (ışın oyuncunun
            // kapsülünün içinden başlar; Linecast içinden başladığı çarpıştırıcıyı görmez).
            if (Physics.Linecast(oyuncu.position + Vector3.up * 1.2f, aday + Vector3.up * 1.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
            yer = aday;
            return true;
        }
        return false;
    }

    // Oyuncu gizleniyor mu: dolapta saklanıyor (#97) ya da çömelmiş ve kafa lambası kapalı.
    private bool Gizli()
    {
        if (SaklanmaYeri.Saklaniyor) return true;
        return kontrol != null && kontrol.Egik && lamba != null && !lamba.IsikYaniyor;
    }

    // Gölge'nin yolundaki kapalı oda kapısı açılır (kilitli değilse): kapalı kapının içinden geçmesin,
    // "kapıyı açtı" korkusu da bedava gelsin. Oyuncu kapıyı arkasından kapatınca Gölge yine girer;
    // saklanmanın kuralı görüş çizgisi ve sessizliktir, kapı değil.
    private void YoldakiKapiyiAc(IcKapi[] kapilar)
    {
        if (golge == null) return;
        foreach (IcKapi k in kapilar)
        {
            if (k == null || !k.gameObject.activeInHierarchy || k.Acik || k.Kilitli || k.Devrik) continue;
            Vector3 fark = k.transform.position - golge.transform.position; fark.y = 0f;
            if (fark.magnitude < 0.9f) k.Ayarla(true);
        }
    }

    private bool EkrandaMi(Vector3 yer)
    {
        if (kamera == null) return false;
        Vector3 e = kamera.WorldToViewportPoint(yer + Vector3.up * 1.2f);
        return e.z > 0f && e.x >= 0f && e.x <= 1f && e.y >= 0f && e.y <= 1f;
    }

    // Gölge'nin başından oyuncunun gözüne açık bir çizgi var mı (SaklanmaAni.Bak ile aynı)?
    private bool Goruyor()
    {
        if (golge == null || kamera == null) return false;
        if (SaklanmaYeri.Saklaniyor) return false;   // dolaptaki oyuncu görülmez (#97)
        Vector3 bas = golge.transform.position + Vector3.up * BasYuksekligi;
        Vector3 goz = kamera.transform.position;
        if (Vector3.Distance(bas, goz) > GormeMesafesi) return false;
        foreach (RaycastHit h in Physics.RaycastAll(bas, goz - bas, (goz - bas).magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (t.IsChildOf(oyuncu) || t.IsChildOf(golge.transform)) continue;
            return false;
        }
        return true;
    }

    private void Yonel(Vector3 fark)
    {
        Vector3 yatay = new Vector3(fark.x, 0f, fark.z);
        if (yatay.sqrMagnitude > 0.0001f) golge.transform.rotation = Quaternion.LookRotation(yatay);
    }

    private void Yakalandi()
    {
        Debug.Log($"[VARLIK] {kat}. kat: avda yakalandı; kat başında uyanılacak (kilitler kalır).");
        AvIptal();
        izler.Clear(); izZamanlari.Clear();
        kural.AvBitti();
        yonetici.AvdaYakalandi(kat);
    }

    private void AvIptal()
    {
        if (golge != null) Destroy(golge);
        golge = null;
    }

    private void AdimCal(Vector3 yer)
    {
        // Bastığı zeminin adımı (#99, ZeminSesi): merdivende beton, mutfakta fayans, halıda boğuk.
        AudioClip klip = golge != null ? ZeminSesi.AdimKlibi(yer, golge.transform) : null;
        if (klip == null) klip = adim;
        if (klip == null) return;
        GameObject g = new GameObject("Av_Adim");
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip; a.spatialBlend = 1f; a.pitch = 0.62f; a.volume = 1f; a.maxDistance = 16f;
        a.Play();
        Destroy(g, klip.length / a.pitch + 0.1f);
    }

    private void OnDisable() { AvIptal(); }
}
