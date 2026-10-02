// AsansorPanosu.cs
// Asansörün sigorta panosu ve asansörün kendisi (#68, 8. kat: imza mekaniği "asansör").
//
// Bina boyunca asansör arızalıdır: kapısında "ARIZALI" yazar, göstergesi "-" gösterir. Her
// sahanlıkta asansörün solunda küçük bir pano durur; üç sigortasından biri eksiktir. 8. katta
// kilit zincirinin son kilidi bu panodadır ("Asansor" kilidi):
//
//   1. Oyuncu Yönetim dairesinin dosya dolabında asansörün sigortasını bulur.
//   2. Eli sigortayı taşırken panoya bakınca "TAK (E)". Takınca panonun lambası kırmızıdan
//      yeşile döner: asansörde elektrik var.
//   3. Çağrı düğmesine basar ("ÇAĞIR", AsansorDugmesi). Kuyudan bir uğultu gelir; gösterge
//      önce "9" yazar ve bir süre öyle kalır (binada sekiz kat var), sonra "8". "Ding".
//   4. Kapılar iki yana kayar; ışıklı kabinin yerinde ÇATI ANAHTARI durur. Kilit çözülür.
//   5. Oyuncu anahtarı alıp uzaklaşınca kapılar kapanır, gösterge yeniden "9" der: asansör
//      geldiği yere döner. (Finalde dış cephede dokuzuncu katın penceresi yanacak.)
//
// Bileşen sahnede kalıcıdır: graybox kurucusu her gerçek sahanlığın panosuna ekler ve sesleri
// bağlar (build'e girsinler diye adla aranmaz). Zincir uygulayıcısı 8. katta Hazirla ile
// kurar, kat değişince Sifirla ile her şeyi eski hâline döndürür. Hazırlanmamış pano hiçbir
// şey yapmaz: başka katlarda oyuncu yalnızca bir sigortası eksik, ölü bir pano görür.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsansorPanosu : MonoBehaviour, IEtkilesimli
{
    // ---------- GRAYBOX KURUCUSUNUN BAĞLADIKLARI ----------
    [SerializeField] private GameObject eksikSigorta;   // panodaki üçüncü sigorta: takılınca görünür
    [SerializeField] private Renderer lamba;            // panonun durum lambası
    [SerializeField] private Transform kapiSol, kapiSag;
    // Kanada bantlı "ARIZALI" kâğıdı ve yazısı kanadın çocuğu değil (anomali kartları onları ayrı
    // ayrı hedefliyor); kanat kayınca onlar da aynı yolu kaymalı, yoksa boşlukta asılı kalırlar.
    // (Alanın adı ilk sürümden kaldı; kâğıt aslında oyuncunun sağındaki, dünyada -x'teki kanatta.)
    [SerializeField] private Transform[] solKanattakiler;
    [SerializeField] private GameObject kabin;          // ışıklı kabin (kapılar açılınca görünür)
    [SerializeField] private GameObject kuyu;           // kapıların ardındaki simsiyah kuyu
    [SerializeField] private TextMesh gosterge;         // kapının üstündeki kat göstergesi
    [SerializeField] private AudioClip dingSesi;
    [SerializeField] private AudioClip ugultuSesi;

    // Durumlar sırayla: sigorta yok -> elektrik var -> geliyor -> geldi (kapılar açık) -> gitti.
    public enum Durum { Olu, Elektrikli, Geliyor, Geldi, Gitti }
    public Durum Hal { get; private set; } = Durum.Olu;

    private const float KayisMesafesi = 0.52f;   // kapı kanadının duvarın içine kayacağı yol
    private const float KapiSuresi = 1.6f;       // kapıların açılma ya da kapanma süresi

    private bool hazir;                  // bu katta pano zincirin parçası mı?
    private string gerekliSigorta;       // takılacak sigortanın kimliği
    private int kat;
    private readonly List<Kilit> kilitler = new List<Kilit>();
    private Vector3 solKapali, sagKapali;         // kapıların kapalı konumu (Hazirla'da okunur)
    private Vector3[] ekKapali;                   // sol kanattakilerin kapalı konumu
    // Her kanadın kayacağı yön (+1: +x, -1: -x). Adından değil DURDUĞU YERDEN bulunur: modelin
    // "Kapi_Sol" parçası Blender -> Unity aktarımında x ekseni ters döndüğü için sahnenin +x
    // tarafında duruyor. İlk sürüm adına güvenip iki kanadı içe kaydırdı; kanatlar yer değiştirdi,
    // açıklık kapalı kaldı.
    private float solYon = -1f, sagYon = 1f;
    private float[] ekYon;
    private AudioSource ugultu;
    private Coroutine hareket;
    private static AudioClip sigortaSesi, dugmeSesi, varisSesi, kalkisSesi;

    // Lambanın iki rengi. Lamba ışıktan etkilenmeyen (Unlit) bir malzeme; 1'in üstündeki yeşil
    // (HDR) bloom ile hafifçe parlar. MaterialPropertyBlock: malzemenin kopyasını üretmeden
    // yalnızca bu çizicinin rengini değiştirir (anomali kartlarındaki ders).
    private static readonly Color KirmiziSonuk = new Color(0.22f, 0.02f, 0.02f);
    private static readonly Color YesilYanan = new Color(0.25f, 2.2f, 0.45f);
    private static readonly int TabanRenk = Shader.PropertyToID("_BaseColor");

    /// <summary>8. katın zinciri panoyu kurar: bu sigorta takılınca asansör çağrılabilir.</summary>
    public void Hazirla(string sigortaKimligi, int katNo)
    {
        Sifirla();
        hazir = true;
        gerekliSigorta = sigortaKimligi;
        kat = katNo;
    }

    public void KilitEkle(Kilit k) { if (k != null && !kilitler.Contains(k)) kilitler.Add(k); }

    /// <summary>Kat değişince: pano ölü, kapılar kapalı, kabin gizli, gösterge "-".</summary>
    public void Sifirla()
    {
        if (hareket != null) { StopCoroutine(hareket); hareket = null; }
        // Kapılar kapalı konumlarına döner. Konumlar ilk kez okunuyorsa kapılar zaten kapalıdır
        // (sahne kapalı kapılarla kaydedilir), okunan değer doğrudur.
        KapalilariOku();
        KapilariKoy(0f);
        hazir = false;
        Hal = Durum.Olu;
        kilitler.Clear();
        if (eksikSigorta != null) eksikSigorta.SetActive(false);
        if (kabin != null) kabin.SetActive(false);
        if (kuyu != null) kuyu.SetActive(true);
        if (gosterge != null) gosterge.text = "-";
        LambayiBoya(KirmiziSonuk);
        UgultuyuDurdur();
    }

    // Kapıların kapalı konumu bir kez okunur ve saklanır; sonra hep ona dönülür.
    private bool okundu;
    private void KapalilariOku()
    {
        if (okundu) return;
        okundu = true;
        if (kapiSol != null) solKapali = kapiSol.localPosition;
        if (kapiSag != null) sagKapali = kapiSag.localPosition;
        solYon = Taraf(kapiSol, -1f);
        sagYon = Taraf(kapiSag, 1f);
        int n = solKanattakiler != null ? solKanattakiler.Length : 0;
        ekKapali = new Vector3[n];
        ekYon = new float[n];
        for (int i = 0; i < n; i++)
        {
            if (solKanattakiler[i] == null) continue;
            ekKapali[i] = solKanattakiler[i].localPosition;
            // Kâğıt hangi taraftaysa o taraftaki kanatla birlikte kayar.
            ekYon[i] = solKanattakiler[i].localPosition.x < 0f ? -1f : 1f;
        }
    }

    // Kanadın asansörün hangi yanında durduğu: çizicisinin (görünen parçasının) ortasının
    // asansör kökünün ekseninde x'i eksi mi artı mı? Çizici yoksa verilen varsayılan.
    private static float Taraf(Transform kanat, float varsayilan)
    {
        if (kanat == null) return varsayilan;
        Renderer r = kanat.GetComponent<Renderer>();
        if (r == null || kanat.parent == null) return varsayilan;
        float x = kanat.parent.InverseTransformPoint(r.bounds.center).x;
        return x < 0f ? -1f : 1f;
    }

    private void Awake() { KapalilariOku(); LambayiBoya(KirmiziSonuk); }

    // ---------- PANO: SİGORTAYI TAK ----------
    public string Ipucu { get { return Dil.Getir("ipucu.tak"); } }

    // İstem yalnızca eli doğru sigortayı taşırken çıkar: öbür zamanlarda pano "bir sigortası
    // eksik" görünür, oyuncu ne araması gerektiğini gözüyle anlar (dairedeki sigorta kutusu gibi).
    public bool EtkilesimeAcik { get { return hazir && Hal == Durum.Olu && Cep.KimligiTasiyor(gerekliSigorta); } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik || !Cep.KimlikleKullan(gerekliSigorta)) return;
        if (sigortaSesi == null) sigortaSesi = Resources.Load<AudioClip>("Sesler/Sigorta_Takma");
        if (sigortaSesi != null) AudioSource.PlayClipAtPoint(sigortaSesi, transform.position, 0.7f);
        ElektrikVer();
        Debug.Log($"[ASANSOR] {kat}. kat: sigorta takıldı, asansörde elektrik var.");
    }

    private void ElektrikVer()
    {
        Hal = Durum.Elektrikli;
        if (eksikSigorta != null) eksikSigorta.SetActive(true);
        LambayiBoya(YesilYanan);
    }

    // ---------- DÜĞME: ASANSÖRÜ ÇAĞIR ----------
    /// <summary>Çağrı düğmesinde istem çıksın mı? Bu katta pano zincirin parçasıysa, asansör gelmeden önce.</summary>
    public bool DugmeAcik { get { return hazir && (Hal == Durum.Olu || Hal == Durum.Elektrikli); } }

    /// <summary>Düğmeye basıldı (AsansorDugmesi çağırır).</summary>
    public void DugmeyeBasildi()
    {
        // Bakalit çağır düğmesi (#99; eskiden radyo düğmesinin sesiydi).
        if (dugmeSesi == null) dugmeSesi = Resources.Load<AudioClip>("Sesler/Dugme_Bakalit");
        if (dugmeSesi == null) dugmeSesi = Resources.Load<AudioClip>("Sesler/Radyo_Dugme");
        if (dugmeSesi != null) AudioSource.PlayClipAtPoint(dugmeSesi, DugmeNoktasi(), 0.6f);
        // Elektrik yoksa düğme ölü: yalnızca tık. Oyuncu panoya bakıp eksik sigortayı görür.
        if (Hal != Durum.Elektrikli) return;
        hareket = StartCoroutine(Gel());
    }

    private Vector3 DugmeNoktasi() { return kapiSag != null ? kapiSag.position + Vector3.up * 1.1f : transform.position; }

    // Asansör dokuzuncu kattan iner.
    private IEnumerator Gel()
    {
        Hal = Durum.Geliyor;
        Debug.Log($"[ASANSOR] {kat}. kat: asansör çağrıldı.");
        yield return new WaitForSeconds(0.8f);
        // Kuyudan uğultu yükselir; gösterge "9" yazar ve bir süre öyle kalır.
        UgultuyuBaslat();
        if (gosterge != null) gosterge.text = "9";
        yield return new WaitForSeconds(4.5f);
        if (gosterge != null) gosterge.text = "8";
        yield return new WaitForSeconds(1.2f);
        UgultuyuDurdur();
        // Gerçek kayıt (#74, BigSoundBank #1712): kabin durur, kapılar açılır; hemen ardından zil.
        if (varisSesi == null) varisSesi = Resources.Load<AudioClip>("Sesler/Asansor_Varis");
        if (varisSesi != null) AudioSource.PlayClipAtPoint(varisSesi, KuyuNoktasi(), 0.9f);
        yield return new WaitForSeconds(0.4f);
        if (dingSesi != null) AudioSource.PlayClipAtPoint(dingSesi, GostergeNoktasi(), 0.9f);
        yield return new WaitForSeconds(0.5f);
        // Kabin kapıların ardında belirir (kuyu gizlenir), sonra kapılar kayarak açılır.
        if (kuyu != null) kuyu.SetActive(false);
        if (kabin != null) kabin.SetActive(true);
        yield return KapilariKaydir(0f, 1f);
        Hal = Durum.Geldi;
        foreach (Kilit k in kilitler) if (k != null && !k.Cozuk) k.Coz(acilsin: false);
        Debug.Log($"[ASANSOR] {kat}. kat: asansör geldi, kapılar açık.");
        hareket = null;
    }

    // Anahtar alınıp oyuncu kapılardan uzaklaşınca asansör geri döner.
    private void Update()
    {
        if (!hazir || Hal != Durum.Geldi || hareket != null) return;
        // 8. katta kabin boştur (#108): oyuncu içine girince asansör onu dokuzuncu kata çıkarır. Yolculuğu
        // DokuzuncuKat yönetir; biz yalnızca "oyuncu kabinde" deyip onun akışını başlatırız.
        if (kat == KatSayaci.CatiKati && DokuzuncuKat.Ornek != null && DokuzuncuKat.Ornek.Baslayabilir && OyuncuKabinde())   // yalnız 8. katın panosu (9'unki değil)
        {
            hareket = StartCoroutine(DokuzaGotur());
            return;
        }
        if (!Cep.Tasiyor(EsyaTuru.KatAnahtari, kat)) return;
        // Oyuncu kabinin içindeyken ya da kapıların dibindeyken kapılar kapanmaz: içeride
        // kapalı kalmak istemeyiz. Asansörün kendi ekseninde oyuncu kapıdan 1,2 m uzakta olmalı.
        Camera kamera = Camera.main;
        if (kamera == null) return;
        Vector3 yerel = transform.parent.InverseTransformPoint(kamera.transform.position);
        if (yerel.z < -MekanOlculeri.SahanlikDerin + 1.2f) return;
        hareket = StartCoroutine(Git());
    }

    private IEnumerator Git()
    {
        yield return new WaitForSeconds(2.5f);
        // Kapılar kapanır ve motor kalkar (gerçek kayıt, #74).
        if (kalkisSesi == null) kalkisSesi = Resources.Load<AudioClip>("Sesler/Asansor_Kalkis");
        if (kalkisSesi != null) AudioSource.PlayClipAtPoint(kalkisSesi, KuyuNoktasi(), 0.9f);
        yield return KapilariKaydir(1f, 0f);
        if (kabin != null) kabin.SetActive(false);
        if (kuyu != null) kuyu.SetActive(true);
        UgultuyuBaslat();
        yield return new WaitForSeconds(1.5f);
        if (gosterge != null) gosterge.text = "9";
        yield return new WaitForSeconds(3f);
        UgultuyuDurdur();
        Hal = Durum.Gitti;
        Debug.Log($"[ASANSOR] {kat}. kat: asansör dokuzuncu kata döndü.");
        hareket = null;
    }

    // Kapılar: 0 kapalı, 1 açık. Yumuşak başlayıp yumuşak biten bir kayış (kosinüs eğrisi).
    private IEnumerator KapilariKaydir(float bas, float son)
    {
        for (float t = 0f; t < KapiSuresi; t += Time.deltaTime)
        {
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t / KapiSuresi);
            KapilariKoy(Mathf.Lerp(bas, son, e));
            yield return null;
        }
        KapilariKoy(son);
    }

    // Sol kanat -x'e, sağ kanat +x'e, duvarın içine kayar.
    // Her kanat kendi tarafına, duvarın içine kayar.
    private void KapilariKoy(float aciklik)
    {
        float yol = KayisMesafesi * aciklik;
        if (kapiSol != null) kapiSol.localPosition = solKapali + Vector3.right * (solYon * yol);
        if (kapiSag != null) kapiSag.localPosition = sagKapali + Vector3.right * (sagYon * yol);
        if (solKanattakiler != null && ekKapali != null && ekYon != null)
            for (int i = 0; i < solKanattakiler.Length && i < ekKapali.Length; i++)
                if (solKanattakiler[i] != null) solKanattakiler[i].localPosition = ekKapali[i] + Vector3.right * (ekYon[i] * yol);
        // Kanat duvarın içine girerken kâğıdın yazısı duvar yüzeyiyle aynı düzleme geliyor ve
        // duvarın üstünde görünüyordu: kapılar yarıdan çok açıkken kâğıt gizlenir. Yalnızca bizim
        // gizlediğimizi geri açarız (kâğıdı gizleyen bir anomali kartının işine karışmamak için).
        bool gizle = aciklik > 0.5f;
        if (gizle != ekGizli && solKanattakiler != null)
        {
            foreach (Transform t in solKanattakiler) if (t != null) t.gameObject.SetActive(!gizle);
            ekGizli = gizle;
        }
    }

    private bool ekGizli;

    /// <summary>Kayıttan: asansör daha önce gelmişti (kilit çözük). Sigorta takılı, kapılar açık.</summary>
    public void GelmisBaslat()
    {
        if (!hazir) return;
        ElektrikVer();
        if (kuyu != null) kuyu.SetActive(false);
        if (kabin != null) kabin.SetActive(true);
        KapilariKoy(1f);
        Hal = Durum.Geldi;
    }

    // ---------- YARDIMCILAR ----------
    // ---------- DOKUZUNCU KAT (#108) ----------
    // Aşağıdaki parçalar DokuzuncuKat'ın yolculuk akışı için dışarıya açılmıştır: kapıları oynatmak,
    // göstergeyi yazmak, uğultuyu açıp kapamak, kabini göstermek, sesleri çalmak. Pano kendi hâlini korur.

    /// <summary>Oyuncunun kamerası kabinin İÇİNDE mi? (Kapı düzleminin ardında, kabin genişliğinde.)</summary>
    public bool OyuncuKabinde()
    {
        Camera kamera = Camera.main;
        if (kamera == null || transform.parent == null) return false;
        Vector3 yerel = transform.parent.InverseTransformPoint(kamera.transform.position);
        return yerel.z < -MekanOlculeri.SahanlikDerin - MekanOlculeri.DuvarKalin - 0.05f && Mathf.Abs(yerel.x) < 0.7f;
    }

    private IEnumerator DokuzaGotur()
    {
        yield return DokuzuncuKat.Ornek.Yolculuk(this);
        hareket = null;
    }

    /// <summary>Kabinin kökü (oyuncu kabinler arasında aynı yerel konumla taşınır).</summary>
    public Transform Kabin { get { return kabin != null ? kabin.transform : null; } }

    /// <summary>Kapıları verilen açıklıktan verilen açıklığa kaydırır (0 kapalı, 1 açık), 1,6 sn.</summary>
    public IEnumerator KapilariOynat(float bas, float son) { yield return KapilariKaydir(bas, son); }

    /// <summary>Kapıları bir anda verilen açıklığa koyar.</summary>
    public void KapilariKoyDisaridan(float aciklik) { KapalilariOku(); KapilariKoy(aciklik); }

    public void GostergeYaz(string metin) { if (gosterge != null) gosterge.text = metin; }
    public void Ugultu(bool acik) { if (acik) UgultuyuBaslat(); else UgultuyuDurdur(); }
    public void VarisSesiCal()
    {
        if (varisSesi == null) varisSesi = Resources.Load<AudioClip>("Sesler/Asansor_Varis");
        if (varisSesi != null) AudioSource.PlayClipAtPoint(varisSesi, KuyuNoktasi(), 0.9f);
    }
    public void KalkisSesiCal()
    {
        if (kalkisSesi == null) kalkisSesi = Resources.Load<AudioClip>("Sesler/Asansor_Kalkis");
        if (kalkisSesi != null) AudioSource.PlayClipAtPoint(kalkisSesi, KuyuNoktasi(), 0.9f);
    }
    public void DingCal() { if (dingSesi != null) AudioSource.PlayClipAtPoint(dingSesi, GostergeNoktasi(), 0.9f); }

    /// <summary>Kabin bu kattan ayrıldı (oyuncu içindeyken başka kata taşındı): kuyu görünür, kabin yok.</summary>
    public void KabinAyrildi(string gostergeMetni)
    {
        if (kabin != null) kabin.SetActive(false);
        if (kuyu != null) kuyu.SetActive(true);
        UgultuyuDurdur();
        GostergeYaz(gostergeMetni);
        Hal = Durum.Gitti;
    }

    /// <summary>
    /// Kabin bu kata geldi, kapılar KAPALI (oyuncu içinde): dokuzuncu katın panosu yolculuğun sonunda, 8. katın
    /// panosu dönüşte böyle başlar. Elektrik var sayılır (lamba yeşil); kapıları sonra KapilariOynat açar.
    /// </summary>
    public void KabinGeldiKapali(int katNo, string gostergeMetni)
    {
        KapalilariOku();
        hazir = true;
        kat = katNo;
        ElektrikVer();
        if (kuyu != null) kuyu.SetActive(false);
        if (kabin != null) kabin.SetActive(true);
        KapilariKoy(0f);
        GostergeYaz(gostergeMetni);
        Hal = Durum.Geldi;
    }

    private Vector3 GostergeNoktasi() { return gosterge != null ? gosterge.transform.position : transform.position; }

    // Kuyunun ağzı: kapıların ardında, bel hizası (kabinin ve motorun sesi buradan gelir).
    private Vector3 KuyuNoktasi() { return transform.parent.TransformPoint(new Vector3(0f, 1.2f, -MekanOlculeri.SahanlikDerin - 0.6f)); }

    private void UgultuyuBaslat()
    {
        if (ugultuSesi == null || ugultu != null) return;
        // Ses kuyudan gelir: kapıların ardında, bel hizasında bir kaynak.
        GameObject g = new GameObject("Asansor_Ugultusu");
        g.transform.SetParent(transform.parent, false);
        g.transform.localPosition = new Vector3(0f, 1.2f, -MekanOlculeri.SahanlikDerin - 0.8f);
        ugultu = g.AddComponent<AudioSource>();
        ugultu.clip = ugultuSesi;
        ugultu.loop = true;
        ugultu.spatialBlend = 1f;
        ugultu.minDistance = 1.5f;
        ugultu.maxDistance = 16f;
        ugultu.volume = 0.75f;
        ugultu.Play();
    }

    private void UgultuyuDurdur()
    {
        if (ugultu == null) return;
        Destroy(ugultu.gameObject);
        ugultu = null;
    }

    private void LambayiBoya(Color renk)
    {
        if (lamba == null) return;
        var blok = new MaterialPropertyBlock();
        lamba.GetPropertyBlock(blok);
        blok.SetColor(TabanRenk, renk);
        lamba.SetPropertyBlock(blok);
    }
}
