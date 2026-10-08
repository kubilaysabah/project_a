// CinOlaylari.cs
// Cin olayları (#131): oyuncu dairedeyken, görüş dışında, aralıklı küçük dehşetler. Zamanlama ve seçim
// CinOlayKurali'nde (saf C#, testli); bu bileşen yöneticinin nesnesinde durur (Varlik gibi) ve seçilen olayı
// sahneye uygular.
//
// OLAYLAR (hepsi oyuncunun GÖRÜŞ DIŞINDA; Gorus.Gorunur ile denetlenir):
//   KapiCarpma        başka bir odanın açık kapısı sert kapanır (gerçek çarpma kaydı); kapalı kalır.
//   Kirilma           oyuncunun 2-4 m arkasında tabak / bardak / vazo kırılır; yerde cam kırıkları kalır.
//   BakmayincaDegisim kapalı bir oda kapısı ya da çekmece/kapak görüş dışındayken açılmış bulunur (sessiz).
//   IceridenVurus     kapalı (varsa kilitli) bir odanın kapısına içeriden üç vuruş.
//   ArkadanAdim       oyuncunun arkasından 3-4 adım (zeminin kendi sesi); oyuncu dönünce kesilir.
//   Cihaz             katın radyosu/TV'si (RadyoAnonsu) bir anlığına cızırtıyla açılıp kapanır.
//   Belirme (#133)    Gölge GÖRÜŞTE belirir: fener huzmesinin içinde, 4-9 m, oyuncuya dönük 1,4-2,2 sn durur
//                     (nefes alır, kafasıyla oyuncuyu izler), lamba titrer, kaybolur. Yaklaşmaz, dokunmaz.
//   Gecis (#133)      Gölge görüşün bir kenarından girip öbür kenarına 4 m/sn ile geçer, çıkınca silinir.
//   Görünür iki olay sahanlıkta da olur (Şahan: "katta gözüksün"); öbürleri yalnızca dairede.
//   #135 (Şahan'ın test turu): geçiş 2,5 m/sn (4'te 0,5 sn'de bitiyordu), belirme 2-5 m (fenerin menzili),
//   belirmelerin yarısı 3. kattan itibaren KARARTMALI: figür dururken kafa lambası 1,3 sn ölür, karanlıkta
//   yalnız gözleri (GozParlamasi) görünür ve 1-1,5 m YAKLAŞIR, lamba yanınca titrer ve figür yok olur. Her
//   görünür olaydan sonra SESSİZLİK: uğultu, TV ve müzik 1 sn'de susar, 15-20 sn sessiz kalır, 3 sn'de döner.
//
// KURALLAR:
//   - Olaylar Varlik.Gurultu ÇAĞIRMAZ (oyuncunun gürültüsü değil, cinin işi) ve GolgeImzasi ÇALMAZ (imza
//     varlığın görünmesinin habercisi kalsın).
//   - Saklanırken, av sürerken, 6. katın saklanma anı ve 7. katın kaçışı sırasında, menüde, açılışta ve
//     dokuzuncu katta olay yok. Sahanlıkta sayaç durur (olaylar dairenin içindir).
//   - Varlik.Kapali (bot, kat denetimi) bunu da kapatır.
//   - Kat değişince kırık parçalar silinir, süren olay durur, kural baştan kurulur.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinOlaylari : MonoBehaviour
{
    [SerializeField] private AudioClip carpmaSesi;       // Kapi_Carpma (kayıt, S43)
    [SerializeField] private AudioClip vurusSesi;        // Kapi_Tiklatma (kayıt, S42)
    [SerializeField] private GameObject kirikSablonu;    // Cam_Kiriklari (kavanozla aynı şablon)
    [SerializeField] private GameObject golgeSablonu;    // Kacis_Golgesi (#133: görünür olaylar; Varlik'le aynı şablon)
    [SerializeField] private GameObject[] figurler;      // #134: altı üç harfli figürü (FigurKurucu bağlar); boşsa Gölge şablonu

    public static CinOlaylari Ornek { get; private set; }

    private const float DaireKontrolAraligi = 0.25f;
    private const float KirilmaUzaklikEnAz = 2f, KirilmaUzaklikEnCok = 4f;
    private const float VurusMesafesi = 9f;
    private const int AdimSayisiEnAz = 3, AdimSayisiEnCok = 4;
    private const float AdimAraligi = 0.45f;
    private const float BelirmeEnYakin = 2f, BelirmeEnUzak = 5f;     // #133: belirme uzaklığı; #135: 8 → 5 m (fenerin okunur menzili)
    private const float GecisHizi = 2.5f;                             // #133: geçiş hızı (m/sn; oyuncu 1,8); #135: 4 → 2,5 (0,5 sn'lik geçiş görülmüyordu)
    private const int KarartmaEnAzKat = 3;                            // #135: karartmalı belirme bu kattan itibaren (1-2 ders katları)
    private const float KarartmaOlasiligi = 0.5f;                     // #135: belirmelerin yarısı karartmalı
    private const float KarartmaSuresi = 1.3f;                        // #135: lambanın ölü kaldığı süre
    private const float YaklasmaEnAz = 1.0f, YaklasmaEnCok = 1.5f;    // #135: karanlıkta yaklaşılan mesafe
    private const float YaklasmaEnYakin = 1.2f;                       // #135: oyuncuya bundan yakına gelmez
    private const float SessizlikEnAz = 15f, SessizlikEnCok = 20f;    // #135: korkudan sonra sessizlik süresi
    private const float SessizlikInis = 1f, SessizlikCikis = 3f;      // #135: susma ve geri gelme süreleri
    private Coroutine sessizlik;                                      // #135: süren sessizlik (yoksa null)
    private const float GecisEnYakin = 1.5f, GecisEnUzak = 6f;       // #133: geçiş uçlarının uzaklığı (odalar küçük)
    private const int YerDenemesi = 80;                               // #133: oda dar, kapı dar: çok dene
    private GameObject golge;                                         // #133: sahnedeki görünür Gölge (yoksa null)
    private const float KacisHizi = 4.5f;                             // #134: kaçış hızı (m/sn)
    private const float KacisAdimAraligi = 0.3f;                      // #134: koşu adımları arası
    private const float KacisBakma = 0.5f, KacisBakmaEnCok = 0.9f;    // #134: kaçmadan önce oyuncuya bakış süresi
    private readonly List<GameObject> torba = new List<GameObject>(); // #134: figür torbası (hepsi çıkınca yeniden dolar)
    private GameObject sonFigur;

    private CinOlayKurali kural;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private Transform oyuncu;
    private Camera kamera;
    private KafaLambasi lamba;
    private int kat = -1;
    private float daireSayaci;
    private Transform daire;                       // oyuncunun içinde olduğu daire (DaireEksenleri'nin nesnesi), yoksa null
    private Coroutine suren;
    private readonly List<GameObject> konanlar = new List<GameObject>();   // kırık parçalar: kat değişince silinir
    private AudioClip[] kirilmaSesleri;

    /// <summary>Test göstergesi: son olay ve bir sonrakine kalan süre.</summary>
    public string Durum
    {
        get
        {
            if (kural == null) return "";
            return $"{kural.Son} {(daire != null ? "" : "(sahanlık) ")}{kural.Kalan:0}s";
        }
    }

    private void Awake() { Ornek = this; }
    private void OnDestroy() { if (Ornek == this) Ornek = null; }

    private void Start()
    {
        kural = new CinOlayKurali(System.Environment.TickCount);
        yonetici = GetComponent<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrol != null)
        {
            oyuncu = kontrol.transform;
            kamera = kontrol.GetComponentInChildren<Camera>();
        }
        lamba = FindAnyObjectByType<KafaLambasi>();
        // Kırılma sesleri (S79-S81): esya_sesleri_uret.py → kirilma_tabak / kirilma_bardak / kirilma_vazo.
        var liste = new List<AudioClip>();
        foreach (string ad in new[] { "Kirilma_Tabak", "Kirilma_Bardak", "Kirilma_Vazo" })
        {
            AudioClip c = Resources.Load<AudioClip>("Sesler/" + ad);
            if (c != null) liste.Add(c);
        }
        if (liste.Count == 0) { AudioClip c = Resources.Load<AudioClip>("Sesler/Cam_Kirilma"); if (c != null) liste.Add(c); }
        kirilmaSesleri = liste.ToArray();
    }

    private void Update()
    {
        if (kural == null || yonetici == null || oyuncu == null) return;

        if (yonetici.BulunulanKat != kat)
        {
            kat = yonetici.BulunulanKat;
            kural.KatAyarla(kat);
            Temizle();
        }

        bool uygun = Etkin();
        if (uygun) DairedeMi();
        // #133: saat sahanlıkta da işler (görünür olaylar için); daire içi olaylar aşağıda daire ister.
        uygun = uygun && suren == null;
        if (!kural.Ilerlet(Time.deltaTime, uygun)) return;
        if (!uygun) return;   // sayaç dolu ama şu an uygun değil: uygun olunca seçilir

        bool dairede = daire != null;
        bool[] mevcut = new bool[CinOlayKurali.OlaySayisi];
        mevcut[(int)CinOlayi.KapiCarpma] = dairede && CarpilacakKapi() != null;
        mevcut[(int)CinOlayi.Kirilma] = dairede && KirilmaNoktasi(out _);
        mevcut[(int)CinOlayi.BakmayincaDegisim] = dairede && (DegisecekKapi() != null || DegisecekParca() != null);
        mevcut[(int)CinOlayi.IceridenVurus] = dairede && VurulacakKapi() != null;
        mevcut[(int)CinOlayi.ArkadanAdim] = dairede;
        mevcut[(int)CinOlayi.Cihaz] = dairede && Cihaz() != null;
        mevcut[(int)CinOlayi.Belirme] = Sablon() != null && BelirmeYeri(out _);
        mevcut[(int)CinOlayi.Gecis] = Sablon() != null && GecisYolu(out _, out _);
        mevcut[(int)CinOlayi.Kacis] = Sablon() != null && KacisYolu(out _, out _);
        CinOlayi olay = kural.Sec(mevcut);
        if (olay == CinOlayi.Yok) return;
        Debug.Log($"[CIN] {kat}. kat: {olay}");
        switch (olay)
        {
            case CinOlayi.KapiCarpma: suren = StartCoroutine(KapiyiCarp()); break;
            case CinOlayi.Kirilma: Kir(); break;
            case CinOlayi.BakmayincaDegisim: Degistir(); break;
            case CinOlayi.IceridenVurus: suren = StartCoroutine(IceridenVur()); break;
            case CinOlayi.ArkadanAdim: suren = StartCoroutine(ArkadanYuru()); break;
            case CinOlayi.Cihaz: Cihaz()?.KisaParazit(); break;
            case CinOlayi.Belirme: suren = StartCoroutine(Belir()); break;
            case CinOlayi.Gecis: suren = StartCoroutine(Gec()); break;
            case CinOlayi.Kacis: suren = StartCoroutine(Kac()); break;
        }
    }

    // Kural bu karede işlesin mi? Varlik.Etkin ile aynı kapılar; saklanırken ve av sürerken de kapalı.
    private bool Etkin()
    {
        if (Varlik.Kapali) return false;
        if (DokuzuncuKat.Icinde) return false;
        if (!yonetici.enabled || !yonetici.GolgeSayabilir) return false;   // açılış, kapı açık, çatı, final
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        if (kontrol == null || !kontrol.enabled) return false;              // menü, not, şifre ekranı, saklanma
        if (SaklanmaYeri.Saklaniyor) return false;
        if (Time.timeScale <= 0f) return false;
        Varlik v = Varlik.Ornek;
        if (v != null && v.AvSuruyor) return false;
        foreach (KacisAni k in FindObjectsByType<KacisAni>()) if (k.Basladi) return false;
        foreach (SaklanmaAni s in FindObjectsByType<SaklanmaAni>()) if (s.Suruyor) return false;
        return true;
    }

    // Oyuncunun altındaki zemin bir dairenin parçası mı? 0,25 sn'de bir bakılır (Varlik.DairedeMi gibi).
    private void DairedeMi()
    {
        daireSayaci += Time.deltaTime;
        if (daireSayaci < DaireKontrolAraligi) return;
        daireSayaci = 0f;
        daire = null;
        foreach (RaycastHit h in Physics.RaycastAll(oyuncu.position + Vector3.up * 0.5f, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(oyuncu)) continue;
            DaireEksenleri e = h.collider.GetComponentInParent<DaireEksenleri>();
            if (e != null && !DokuzuncuKat.SetinParcasi(e)) { daire = e.transform; break; }
        }
    }

    private void OnDisable()
    {
        if (golge != null) Destroy(golge); golge = null;
        SessizligiBitir();
    }

    private void Temizle()
    {
        if (suren != null) { StopCoroutine(suren); suren = null; }
        SessizligiBitir();
        if (golge != null) { Destroy(golge); golge = null; }
        foreach (GameObject g in konanlar) if (g != null) Destroy(g);
        konanlar.Clear();
        daire = null;
    }

    private bool Gorunur(Vector3 nokta, Transform haric) { return Gorus.Gorunur(kamera, oyuncu, nokta, haric); }

    // ---------- ADAYLAR ----------
    private IEnumerable<IcKapi> DaireKapilari()
    {
        if (daire == null) yield break;
        foreach (IcKapi k in daire.GetComponentsInChildren<IcKapi>())
            if (k.gameObject.activeInHierarchy && !k.Devrik) yield return k;
    }

    // Çarpılacak kapı: açık, oyuncunun yolunda değil, kanadı görünmüyor, 1,5-12 m arası.
    private IcKapi CarpilacakKapi()
    {
        var adaylar = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (!k.Acik || k.OyuncuYolunda || k.EtkilesimeAcik == false) continue;
            float m = Vector3.Distance(k.transform.position, oyuncu.position);
            if (m < 1.5f || m > 12f) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            adaylar.Add(k);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // Bakmayınca açılacak kapı: kapalı, kilitsiz, görünmüyor.
    private IcKapi DegisecekKapi()
    {
        var adaylar = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (k.Acik || k.Kilitli || !k.EtkilesimeAcik || k.OyuncuYolunda) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            adaylar.Add(k);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // Bakmayınca açılacak çekmece / kapak: kapalı, kilitsiz, görünmüyor.
    private AcilirParca DegisecekParca()
    {
        if (daire == null) return null;
        var adaylar = new List<AcilirParca>();
        foreach (AcilirParca p in daire.GetComponentsInChildren<AcilirParca>())
        {
            if (!p.gameObject.activeInHierarchy || p.Acik || p.Kilitli || !p.EtkilesimeAcik) continue;
            Renderer r = p.GetComponentInChildren<Renderer>();
            Vector3 nokta = r != null ? r.bounds.center : p.transform.position;
            if (Gorunur(nokta, p.transform)) continue;
            adaylar.Add(p);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // İçeriden vurulacak kapı: kapalı, görünmüyor, 9 m içinde; kilitli olanlar tercih edilir (içeride "kimse" yok).
    private IcKapi VurulacakKapi()
    {
        var kilitliler = new List<IcKapi>(); var digerleri = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (k.Acik) continue;
            if (Vector3.Distance(k.transform.position, oyuncu.position) > VurusMesafesi) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            (k.Kilitli ? kilitliler : digerleri).Add(k);
        }
        var liste = kilitliler.Count > 0 ? kilitliler : digerleri;
        return liste.Count == 0 ? null : liste[Random.Range(0, liste.Count)];
    }

    // Kırılma noktası: oyuncunun 2-4 m arkasında, dairenin zemininde, görünmeyen bir yer.
    private bool KirilmaNoktasi(out Vector3 nokta)
    {
        nokta = Vector3.zero;
        if (daire == null) return false;
        for (int deneme = 0; deneme < 8; deneme++)
        {
            float uzaklik = Random.Range(KirilmaUzaklikEnAz, KirilmaUzaklikEnCok);
            Vector3 geri = -oyuncu.forward * uzaklik + oyuncu.right * Random.Range(-1f, 1f);
            Vector3 ust = oyuncu.position + geri + Vector3.up * 1.2f;
            if (!Physics.Raycast(ust, Vector3.down, out RaycastHit h, 1.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (h.collider.transform.IsChildOf(oyuncu)) continue;
            DaireEksenleri e = h.collider.GetComponentInParent<DaireEksenleri>();
            if (e == null || e.transform != daire) continue;
            if (!Gorus.Arkada(kamera, h.point) || Gorunur(h.point + Vector3.up * 0.1f, null)) continue;
            nokta = h.point;
            return true;
        }
        return false;
    }

    private RadyoAnonsu Cihaz()
    {
        if (daire == null) return null;
        foreach (RadyoAnonsu r in daire.GetComponentsInChildren<RadyoAnonsu>())
            if (r.gameObject.activeInHierarchy && !r.Yayinda) return r;
        return null;
    }

    // ---------- FİGÜR HAVUZU (#134) ----------
    // Şahan: "altı figürü farklı farklı sırayla ekle." Torba kuralı: altı figür karıştırılıp torbaya konur, her
    // görünür olay torbadan bir tane çeker; torba boşalınca yeniden dolar; yeni torbanın ilk figürü bir önceki
    // olayın figürüyle aynı olamaz. Prefab yoksa (henüz indirilmedi) eski Gölge şablonu kullanılır.
    private GameObject Sablon() { return (figurler != null && figurler.Length > 0 && figurler[0] != null) ? figurler[0] : golgeSablonu; }

    private GameObject FigurSec()
    {
        if (figurler == null || figurler.Length == 0) return golgeSablonu;
        if (torba.Count == 0)
        {
            foreach (GameObject f in figurler) if (f != null) torba.Add(f);
            for (int i = torba.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); GameObject g = torba[i]; torba[i] = torba[j]; torba[j] = g; }
            if (torba.Count > 1 && torba[torba.Count - 1] == sonFigur) { GameObject g = torba[0]; torba[0] = torba[torba.Count - 1]; torba[torba.Count - 1] = g; }
        }
        if (torba.Count == 0) return golgeSablonu;
        GameObject secilen = torba[torba.Count - 1];
        torba.RemoveAt(torba.Count - 1);
        sonFigur = secilen;
        return secilen;
    }

    // ---------- GÖRÜNÜR OLAYLARIN YERLERİ (#133) ----------
    // Bir noktada Gölge durabilir mi: altında oyuncunun ayak hizasında zemin var, bir insan sığar (kapsül boş).
    private bool Durulur(ref Vector3 aday)
    {
        float ayak = oyuncu.position.y;
        if (!Physics.Raycast(aday + Vector3.up * 1f, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) return false;
        if (zemin.collider.transform.IsChildOf(oyuncu)) return false;
        if (Mathf.Abs(zemin.point.y - ayak) > 0.3f) return false;
        aday.y = zemin.point.y;
        return !Physics.CheckCapsule(aday + Vector3.up * 0.35f, aday + Vector3.up * 1.6f, 0.25f, ~0, QueryTriggerInteraction.Ignore);
    }

    // Gölge'nin göğsü ekranın ortadaki %70'inde ve gözle göğsü/dizi arasında engel yok (GezenGolge kuralı).
    private bool GozOnunde(Vector3 yer)
    {
        if (kamera == null) return false;
        Vector3 e = kamera.WorldToViewportPoint(yer + Vector3.up * 1.2f);
        if (e.z <= 0f || e.x < 0.15f || e.x > 0.85f || e.y < 0.15f || e.y > 0.85f) return false;
        Vector3 goz = kamera.transform.position;
        if (Physics.Linecast(goz, yer + Vector3.up * 1.3f, ~0, QueryTriggerInteraction.Ignore)) return false;
        return !Physics.Linecast(goz, yer + Vector3.up * 0.5f, ~0, QueryTriggerInteraction.Ignore);
    }

    private Vector3 Ileri()
    {
        Vector3 ileri = kamera != null ? kamera.transform.forward : oyuncu.forward;
        ileri.y = 0f;
        if (ileri.sqrMagnitude < 0.01f) ileri = oyuncu.forward;
        return ileri.normalized;
    }

    // Belirme yeri: bakış yönünde ±20°, 4-9 m, zeminde, görüşte.
    private bool BelirmeYeri(out Vector3 yer)
    {
        yer = Vector3.zero;
        if (oyuncu == null) return false;
        Vector3 ileri = Ileri();
        for (int deneme = 0; deneme < YerDenemesi; deneme++)
        {
            Vector3 aday = oyuncu.position + Quaternion.Euler(0f, Random.Range(-30f, 30f), 0f) * ileri * Random.Range(BelirmeEnYakin, BelirmeEnUzak);
            if (!Durulur(ref aday) || !GozOnunde(aday)) continue;
            yer = aday;
            return true;
        }
        return false;
    }

    // Geçiş yolu (#133): önce görüşte, zeminde boş bir nokta bulunur (±35°, 1,5-6 m); sonra bakışa DİK yönde
    // (sağ-sol) o noktadan iki yana, duvara ya da döşemenin bittiği yere kadar (en çok 2,5 m) gidilir. İki ucun
    // toplamı 1,6 m'den kısaysa geçiş olmaz. İlk sürüm oyuncunun iki yanında rastgele uç çifti arıyordu; dairede
    // 300 denemede 1 geçerli çıkıyordu (uçlar duvarın, mobilyanın içine düşüyor). Bu yol odanın darlığına uyar.
    private bool GecisYolu(out Vector3 bas, out Vector3 son)
    {
        bas = son = Vector3.zero;
        if (oyuncu == null) return false;
        Vector3 ileri = Ileri();
        Vector3 sag = Vector3.Cross(Vector3.up, ileri).normalized;
        for (int deneme = 0; deneme < YerDenemesi; deneme++)
        {
            Vector3 p = oyuncu.position + Quaternion.Euler(0f, Random.Range(-35f, 35f), 0f) * ileri * Random.Range(GecisEnYakin, GecisEnUzak);
            if (!Durulur(ref p) || !GozOnunde(p)) continue;
            float sol = SerbestMesafe(p, -sag, 2.5f), sagMesafe = SerbestMesafe(p, sag, 2.5f);
            if (sol + sagMesafe < 1.6f) continue;
            bool soldan = Random.value < 0.5f;
            bas = p + (soldan ? -sag * sol : sag * sagMesafe);
            son = p + (soldan ? sag * sagMesafe : -sag * sol);
            return true;
        }
        return false;
    }

    // p'den yon yönünde bir gövde ne kadar yürüyebilir: 25 cm adımlarla ilerlenir; her adımda altında oyuncunun
    // ayak hizasında zemin olmalı (merdiven boşluğuna, basamaklara taşmasın) ve kapsül boş olmalı.
    private float SerbestMesafe(Vector3 p, Vector3 yon, float enCok)
    {
        float ayak = oyuncu.position.y;
        float mesafe = 0f;
        for (float m = 0.25f; m <= enCok + 0.001f; m += 0.25f)
        {
            Vector3 n = p + yon * m;
            if (!Physics.Raycast(n + Vector3.up * 1f, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) break;
            if (zemin.collider.transform.IsChildOf(oyuncu) || Mathf.Abs(zemin.point.y - ayak) > 0.3f) break;
            if (Physics.CheckCapsule(n + Vector3.up * 0.35f, n + Vector3.up * 1.6f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) break;
            mesafe = m;
        }
        return mesafe;
    }

    // Kaçış yolu (#134): görüşte bir belirme noktası P; oradan OYUNCUDAN UZAĞA (uzaklaşma yönü ±60°) 2,5-6 m
    // boş bir koşu yolu. Yol oyuncuya yaklaşmaz; sonu görüş dışına düşerse daha iyi ama şart değil (figür
    // yolun sonunda silinir).
    private bool KacisYolu(out Vector3 bas, out Vector3 son)
    {
        bas = son = Vector3.zero;
        if (oyuncu == null) return false;
        // Odalar dar: tek bir belirme noktasından 2,5 m'lik düz koşu yolu çoğu kez çıkmaz; birkaç nokta denenir,
        // yön oyuncudan uzağa ±90° (yana kaçış da olur), en az 1,8 m. Yol asla oyuncuya doğru gitmez.
        for (int nokta = 0; nokta < 6; nokta++)
        {
            if (!BelirmeYeri(out bas)) continue;
            Vector3 uzak = bas - oyuncu.position; uzak.y = 0f;
            if (uzak.sqrMagnitude < 0.01f) continue;
            uzak.Normalize();
            float enIyi = 0f; Vector3 enIyiSon = Vector3.zero;
            for (int deneme = 0; deneme < 12; deneme++)
            {
                Vector3 yon = Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f) * uzak;
                float m = SerbestMesafe(bas, yon, 6f);
                if (m > enIyi) { enIyi = m; enIyiSon = bas + yon * m; }
            }
            if (enIyi < 1.8f) continue;
            son = enIyiSon;
            return true;
        }
        return false;
    }

    // Kaçış (#134): imza, figür görüşte belirir ve oyuncuya bakar; 0,5-0,9 sn sonra öne eğilip sıçramalı
    // adımlarla ("koşar gibi": iskeleti yok, gövde eğik, 4 Hz'de 7 cm zıplar) uzağa koşar; her adımda zeminin
    // ayak sesi YÜKSEK çalar (Şahan: "yüksek ayak sesi çıkararak"); yolun sonunda silinir. Dokunmaz, yakalamaz.
    private IEnumerator Kac()
    {
        if (KacisYolu(out Vector3 bas, out Vector3 son))
        {
            GameObject sablon = FigurSec();
            if (sablon != null)
            {
                GolgeImzasi.Cal(bas + Vector3.up * 1.2f);
                Vector3 bakis = oyuncu.position - bas; bakis.y = 0f;
                golge = Instantiate(sablon, bas, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);
                golge.name = "Cin_Golgesi";
                Titresim.Vur(0.3f, 0.5f, 0.3f);
                Debug.Log($"[CIN] kaçış: {sablon.name}, {Vector3.Distance(bas, oyuncu.position):0.0} m önde belirdi, {Vector3.Distance(bas, son):0.0} m kaçacak.");
                yield return new WaitForSeconds(Random.Range(KacisBakma, KacisBakmaEnCok));
                Vector3 yon = son - bas; yon.y = 0f;
                Quaternion kosuDonusu = Quaternion.LookRotation(yon.normalized);
                float sure = yon.magnitude / KacisHizi, adimSayaci = 0f;
                for (float t = 0f; t < sure && golge != null; t += Time.deltaTime)
                {
                    Vector3 taban = Vector3.Lerp(bas, son, t / sure);
                    float zip = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f)) * 0.07f;             // sıçrama
                    golge.transform.position = taban + Vector3.up * zip;
                    golge.transform.rotation = kosuDonusu * Quaternion.Euler(14f, 0f, Mathf.Sin(t * Mathf.PI * 8f) * 4f);   // öne eğik, hafif yalpa
                    adimSayaci += Time.deltaTime;
                    if (adimSayaci >= KacisAdimAraligi)
                    {
                        adimSayaci = 0f;
                        AudioClip klip = ZeminSesi.AdimKlibi(taban, golge.transform);
                        if (klip != null) Cal(klip, taban + Vector3.up * 0.05f, 1.0f);
                    }
                    yield return null;
                }
                if (golge != null) Destroy(golge);
                golge = null;
                Debug.Log("[CIN] kaçış bitti: figür silindi.");
                SessizlikBaslat();
            }
        }
        suren = null;
    }

    // ---------- KORKUDAN SONRA SESSİZLİK (#135) ----------
    // Görünür bir olay bitince bina susar: uğultu + TV (OrtamSesi.Kisma) ve müzik (ArkaMuzik.Kisma) 1 sn'de
    // 0'a iner, 15-20 sn öyle kalır, 3 sn'de geri gelir. Sessizlik sesin kendisinden daha korkutucudur;
    // oyuncu "bir şey oldu" duygusunu olay bittikten sonra da taşır. Kat değişince ve bileşen kapanınca
    // çarpanlar hemen 1'e döner. Oyun saatiyle yürür (menüde durur).
    private void SessizlikBaslat()
    {
        if (sessizlik != null) StopCoroutine(sessizlik);
        sessizlik = StartCoroutine(Sessizlik(Random.Range(SessizlikEnAz, SessizlikEnCok)));
    }

    private void SessizligiBitir()
    {
        if (sessizlik != null) { StopCoroutine(sessizlik); sessizlik = null; }
        OrtamSesi.Kisma = 1f;
        ArkaMuzik.Kisma = 1f;
    }

    private IEnumerator Sessizlik(float sure)
    {
        Debug.Log($"[CIN] sessizlik: {sure:0} sn.");
        for (float t = 0f; t < SessizlikInis; t += Time.deltaTime) { Kis(1f - t / SessizlikInis); yield return null; }
        Kis(0f);
        yield return new WaitForSeconds(sure);
        for (float t = 0f; t < SessizlikCikis; t += Time.deltaTime) { Kis(t / SessizlikCikis); yield return null; }
        Kis(1f);
        sessizlik = null;
    }

    private static void Kis(float oran) { OrtamSesi.Kisma = oran; ArkaMuzik.Kisma = oran; }

    // ---------- GÖRÜNÜR OLAYLAR (#133) ----------
    // Belirme: imza, figür görüşte belirir, oyuncuya dönük durur, lamba titrer, kaybolur. Yakalama yok.
    // #135: 3. kattan itibaren belirmelerin yarısı KARARTMALI: lamba ölür (KafaLambasi.Sondur; oyuncunun
    // düğmesi işlemez), karanlıkta yalnız gözler parlar (GozParlamasi) ve figür 1-1,5 m yaklaşır (ayakta
    // durabileceği bir nokta varsa; oyuncuya 1,2 m'den yakına asla gelmez), lamba kendiliğinden yanınca
    // titrer ve figür yok olur. Dokunmaz.
    private IEnumerator Belir()
    {
        GameObject belirmeSablonu = FigurSec();
        if (BelirmeYeri(out Vector3 yer) && belirmeSablonu != null)
        {
            GolgeImzasi.Cal(yer + Vector3.up * 1.2f);
            Vector3 bakis = oyuncu.position - yer; bakis.y = 0f;
            golge = Instantiate(belirmeSablonu, yer, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);
            golge.name = "Cin_Golgesi";
            Titresim.Vur(0.3f, 0.5f, 0.3f);
            bool karartmali = kat >= KarartmaEnAzKat && lamba != null && lamba.IsikYaniyor && Random.value < KarartmaOlasiligi;
            Debug.Log($"[CIN] belirme: {belirmeSablonu.name}, {Vector3.Distance(yer, oyuncu.position):0.0} m{(karartmali ? ", karartmalı" : "")}.");
            yield return new WaitForSeconds(Random.Range(1.4f, 2.2f));
            if (karartmali && golge != null)
            {
                lamba.Sondur(KarartmaSuresi);
                yield return new WaitForSeconds(0.45f);   // karanlık yerleşsin, gözler görülsün
                // Yaklaşma: oyuncuya doğru 1-1,5 m; durulacak yer yoksa (mobilya) yerinde kalır.
                Vector3 oyuncuya = oyuncu.position - yer; oyuncuya.y = 0f;
                float uzaklik = oyuncuya.magnitude;
                float adim = Mathf.Min(Random.Range(YaklasmaEnAz, YaklasmaEnCok), uzaklik - YaklasmaEnYakin);
                if (adim > 0.2f && golge != null)
                {
                    Vector3 yeni = yer + oyuncuya.normalized * adim;
                    if (Durulur(ref yeni))
                    {
                        golge.transform.position = yeni;
                        Vector3 yeniBakis = oyuncu.position - yeni; yeniBakis.y = 0f;
                        if (yeniBakis.sqrMagnitude > 0.001f) golge.transform.rotation = Quaternion.LookRotation(yeniBakis);
                        Debug.Log($"[CIN] belirme: karanlıkta {adim:0.0} m yaklaştı ({Vector3.Distance(yeni, oyuncu.position):0.0} m).");
                    }
                    else Debug.Log("[CIN] belirme: karanlıkta yaklaşacak yer yok, yerinde kaldı.");
                }
                // Lamba kendiliğinden yanana kadar bekle (Sondur bitişi uzatılmış olabilir), sonra kısa bir an dur.
                float bekleme = 0f;   // güvenlik: lamba 4 sn içinde yanmazsa (saklanma vb.) beklemeyi bırak
                while (lamba != null && !lamba.IsikYaniyor && bekleme < KarartmaSuresi + 3f) { bekleme += Time.deltaTime; yield return null; }
                yield return new WaitForSeconds(0.55f);
            }
            if (lamba != null) lamba.Titret(0.5f);
            yield return new WaitForSeconds(0.15f);
            if (golge != null) Destroy(golge);
            golge = null;
            SessizlikBaslat();
        }
        suren = null;
    }

    // Geçiş: imza, Gölge bir uçtan öbürüne 4 m/sn ile yürür (GolgeAnimasyonu bacakları hareketten sallar), silinir.
    private IEnumerator Gec()
    {
        GameObject gecisSablonu = FigurSec();
        if (GecisYolu(out Vector3 bas, out Vector3 son) && gecisSablonu != null)
        {
            GolgeImzasi.Cal(bas + Vector3.up * 1.2f);
            Vector3 yon = son - bas; yon.y = 0f;
            golge = Instantiate(gecisSablonu, bas, Quaternion.LookRotation(yon.normalized));
            golge.name = "Cin_Golgesi";
            Titresim.Vur(0.25f, 0.45f, 0.25f);
            Debug.Log($"[CIN] geçiş: {gecisSablonu.name}, {yon.magnitude:0.0} m.");
            float sure = yon.magnitude / GecisHizi;
            for (float t = 0f; t < sure && golge != null; t += Time.deltaTime)
            {
                golge.transform.position = Vector3.Lerp(bas, son, t / sure);
                yield return null;
            }
            if (golge != null) Destroy(golge);
            golge = null;
            SessizlikBaslat();
        }
        suren = null;
    }

    // ---------- OLAYLAR ----------
    private IEnumerator KapiyiCarp()
    {
        IcKapi kapi = CarpilacakKapi();
        if (kapi != null)
        {
            Titresim.Vur(0.6f, 0.45f, 0.35f);
            if (lamba != null) lamba.Titret(0.5f);
            yield return kapi.Carp(0.16f, carpmaSesi);
        }
        suren = null;
    }

    private void Kir()
    {
        if (!KirilmaNoktasi(out Vector3 nokta)) return;
        if (kirilmaSesleri != null && kirilmaSesleri.Length > 0)
            Cal(kirilmaSesleri[Random.Range(0, kirilmaSesleri.Length)], nokta + Vector3.up * 0.15f, 0.9f);
        if (kirikSablonu != null)
        {
            GameObject g = Instantiate(kirikSablonu, nokta + Vector3.up * 0.005f, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), daire);
            g.name = "Cin_Kiriklari";
            foreach (Collider c in g.GetComponentsInChildren<Collider>()) Destroy(c);   // yerdeki parçaya takılma yok
            konanlar.Add(g);
        }
        Titresim.Vur(0.25f, 0.5f, 0.2f);
    }

    // Sessiz: oyuncu dönünce kapattığı kapıyı açık, kapalı çekmeceyi çekilmiş bulur.
    private void Degistir()
    {
        IcKapi k = DegisecekKapi();
        AcilirParca p = DegisecekParca();
        if (k != null && (p == null || Random.value < 0.5f)) k.Ayarla(true);
        else if (p != null) p.Ayarla(true);
    }

    private IEnumerator IceridenVur()
    {
        IcKapi kapi = VurulacakKapi();
        if (kapi != null && vurusSesi != null)
        {
            Vector3 yer = kapi.transform.position + Vector3.up * 1.2f;
            Titresim.Vur(0.2f, 0.6f, 0.25f);
            Cal(vurusSesi, yer, 0.85f);
            if (lamba != null) lamba.Titret(0.6f);
        }
        yield return null;
        suren = null;
    }

    // Arkadan 3-4 adım; oyuncu dönüp bakarsa (nokta görüşe girerse) kesilir.
    private IEnumerator ArkadanYuru()
    {
        int sayi = Random.Range(AdimSayisiEnAz, AdimSayisiEnCok + 1);
        for (int i = 0; i < sayi; i++)
        {
            Vector3 nokta = oyuncu.position - oyuncu.forward * (3.0f - i * 0.5f) + Vector3.up * 0.05f;
            if (!Gorus.Arkada(kamera, nokta)) break;
            AudioClip klip = ZeminSesi.AdimKlibi(nokta, oyuncu);
            if (klip != null) Cal(klip, nokta, 0.7f);
            yield return new WaitForSeconds(AdimAraligi);
        }
        suren = null;
    }

    // Kısa ömürlü 3B ses (KatAni.Cal gibi: gerçekçi sönüm, dairenin öbür ucundan da duyulur).
    private static void Cal(AudioClip klip, Vector3 yer, float ses)
    {
        if (klip == null) return;
        GameObject g = new GameObject("Cin_Sesi");
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip;
        a.volume = ses;
        a.spatialBlend = 1f;
        a.minDistance = 1.5f;
        a.maxDistance = 20f;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();
        Destroy(g, klip.length + 0.1f);
    }
}
