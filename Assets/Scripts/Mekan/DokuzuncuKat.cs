// DokuzuncuKat.cs
// Dokuzuncu kat: oyunun zorunlu, bulmacasız final bölümü (#108, Docs/hikaye.md §16).
//
// NEDEN
// Oyunun her parçası dokuzuncu kata işaret ediyordu (ruhsatta olmayan kat, gizli plan, "9"dan inen
// asansör, finaldeki yanan pencere) ama oyuncu orayı hiç görmüyordu. Bu bölüm o borcu öder: 8. katın
// asansörü gelir, kabin BOŞTUR; oyuncu binince asansör onu 1974'e, dokuzuncu kata çıkarır. Orada
// Cemal'in lambasını boş çiviye asar, salon açılır, sofrada Gölge oturur, çatı anahtarı oyuncunun
// tabağındadır. Anahtar alınınca Gölge kalkar ve kovalar; kabine kaçılır, kabin 8'e iner, çatıya çıkılır.
//
// NASIL (akış)
//   1. AsansorPanosu (8. kat) oyuncuyu kabinde görünce Yolculuk()'u başlatır: kapılar kapanır, uğultu,
//      gösterge 8→9, oyuncu kapılar kapalıyken dokuzuncu katın kabinine IŞINLANIR (iki kabin birebir
//      aynı; oyuncu aynı yerel konumda durur), orada kapılar açılır.
//   2. Holde CiviAskisi: lamba asılınca Asildi olayı → salon kapısı açılır.
//   3. Sofradaki anahtar (Esya, KatAnahtari, 8. kat) alınınca: anahtarlık şıkırdar, salonun ışığı söner,
//      oturan Gölge kalkar; KacisAni (7. katın kovalamacası, HazirGolge ile) izi sürer.
//   4. Oyuncu kabine girince Donus(): kapılar kapanır, Gölge kapıya vurur, gösterge 9→8, oyuncu 8. katın
//      kabinine ışınlanır, kapılar açılır. Yasandi = true: asansör bir daha çıkarmaz.
//   5. Yakalanırsa Yakalandi(): kararma, bölüm kabinin açılışından yeniden (anahtar tabağa, Gölge
//      sandalyeye, lamba oyuncuya). KAYIT DEĞİŞMEZ: dokuzuncu kat kayda girmez (#84: oyun burada
//      kapatılırsa 8. katın başından açılır).
//
// Dokuzuncu kat sahnede ayrı bir settir (Dokuzuncu_Kat kökü, x = -400; dış cephe gibi) ve GrayboxKurucu
// kurar: 1974 sahanlığı (33-36 numaralı kapılar, tuğlayla örülü merdiven), Daire_9 ve içindekiler.
// Bu bileşen o kökte durur; bağlantıları kurucu bağlar. Icinde (static) açıkken gerilim kuralı, takılma
// ipucu, Gezen Gölge ve ayna korkusu çalışmaz (Varlik.Etkin, TakilmaIpucu.Uygun, KatYoneticisi.GolgeSayabilir).

using System.Collections;
using UnityEngine;

public class DokuzuncuKat : MonoBehaviour
{
    [Header("Dokuzuncu katın parçaları (GrayboxKurucu bağlar)")]
    [SerializeField] private AsansorPanosu pano;          // Sahanlik_9'un asansör panosu
    [SerializeField] private Transform daire;             // Daire_9
    [SerializeField] private CiviAskisi civi;             // holdeki boş çivi
    [SerializeField] private IcKapi salonKapisi;          // lamba asılınca açılan kapı
    [SerializeField] private DaireLambalari lambalar;     // dairenin tavan lambaları (yalnızca salon yanar)
    [SerializeField] private DaireElektrigi elektrik;
    [SerializeField] private GameObject golge;            // sofrada oturan Gölge (Kacis_Golgesi kopyası)
    [SerializeField] private Transform anahtarYeri;       // oyuncunun tabağı (Sofra/Yer_2)
    [SerializeField] private GameObject anahtarSablonu;   // Esya_KatAnahtari prefabı
    [SerializeField] private AudioSource radyo;           // Oda A'daki radyo (Nuri'nin istasyonu)
    [SerializeField] private AudioClip carpmaSesi;        // Gölge'nin kabin kapısına vuruşu
    [SerializeField] private Light asiliLambaIsigi;       // çivideki lambanın ışığı (anahtar alınınca titrer)

    private const float KacisBeklemesi = 2.5f;   // anahtar alındıktan kaç sn sonra Gölge yürümeye başlar
    private const float KacisHizi = 2.0f;         // oyuncu 1,8 m/sn
    private const float OturmaYuksekligi = 0.47f; // sandalye minderinin üstü

    /// <summary>Sahnedeki tek dokuzuncu kat (AsansorPanosu buradan ulaşır).</summary>
    public static DokuzuncuKat Ornek { get; private set; }
    /// <summary>Oyuncu dokuzuncu katta (yolculuk dâhil): gerilim kuralı, ipucu ve Gezen Gölge kapalı.</summary>
    public static bool Icinde { get; private set; }
    /// <summary>Bölüm bu oturumda yaşandı: 8. katın asansörü bir daha çıkarmaz. Kayda girmez.</summary>
    public static bool Yasandi { get; private set; }

    private Transform oyuncu;
    private BirinciSahisKontrolcu kontrol;
    private CharacterController govde;
    private KatYoneticisi yonetici;
    private AsansorPanosu pano8;               // oyuncuyu getiren 8. kat panosu (dönüşte oraya bırakır)
    private KacisAni kacis;
    private GameObject anahtar;
    private Vector3 golgeOturmaYeri;           // Gölge'nin sandalyedeki konumu (yakalanınca geri döner)
    private Quaternion golgeOturmaDonusu;
    private bool yolculukSuruyor, anahtarAlindi, donusBasladi;
    private float karartma;                    // 0-1: yakalanma kararması

    // Static alanlar sahne yeniden yüklenince (bitiş, Katlar sayfası) kendiliğinden sıfırlanmaz (#103).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SahneOlayinaBaglan()
    {
        Icinde = false; Yasandi = false;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SahneYuklendi;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SahneYuklendi;
    }
    private static void SahneYuklendi(UnityEngine.SceneManagement.Scene sahne, UnityEngine.SceneManagement.LoadSceneMode kip)
    { Icinde = false; Yasandi = false; }

    private void Awake()
    {
        Ornek = this;
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>(FindObjectsInactive.Include);
        oyuncu = kontrol != null ? kontrol.transform : null;
        govde = oyuncu != null ? oyuncu.GetComponent<CharacterController>() : null;
        yonetici = FindAnyObjectByType<KatYoneticisi>(FindObjectsInactive.Include);
        if (golge != null)
        {
            golgeOturmaYeri = golge.transform.position;
            golgeOturmaDonusu = golge.transform.rotation;
        }
    }

    private void OnDestroy() { if (Ornek == this) Ornek = null; }

    private void Start()
    {
        // 1974 dairesinin kaplamaları: 6. katın (1974'te donup kalmış evin) duvar kâğıdı ve parkesi.
        // Leke yok: bina o yıl yenidir. Kaplamalar KatYoneticisi'nde bağlı, ondan istenir.
        if (yonetici != null && daire != null) yonetici.KaplamayiGiydir(daire, 6);
        if (civi != null) civi.Asildi += LambaAsildi;
        BolumuKur();
    }

    /// <summary>8. katın panosu sorar: asansör oyuncuyu çıkarsın mı? Bir kez, yolculuk sürmüyorken.</summary>
    public bool Baslayabilir { get { return !Yasandi && !Icinde && !yolculukSuruyor; } }

    // ---------- 1. ÇIKIŞ YOLCULUĞU ----------

    /// <summary>8. katın panosu başlatır (coroutine panoda koşar). Kapılar kapanır, oyuncu dokuza taşınır.</summary>
    public IEnumerator Yolculuk(AsansorPanosu kat8Panosu)
    {
        pano8 = kat8Panosu;
        yolculukSuruyor = true;
        Icinde = true;
        if (kontrol != null) kontrol.enabled = false;   // kapılar kapanırken kapıya sıkışmasın, kabinde dursun
        Debug.Log("[DOKUZ] Oyuncu kabinde: asansör dokuzuncu kata çıkıyor.");
        yield return pano8.KapilariOynat(1f, 0f);
        pano8.KalkisSesiCal();
        KameraHareketi.Sars(0.10f, 0.7f);
        pano8.Ugultu(true);
        if (radyo != null && radyo.clip != null) radyo.Play();   // yukarıda Nuri'nin radyosu çalıyor
        yield return new WaitForSeconds(1.4f);
        pano8.GostergeYaz("9");
        KameraHareketi.Sars(0.04f, 2.0f);                         // kabin yolda hafifçe titrer
        yield return new WaitForSeconds(2.2f);
        // Işınlanma: kapılar kapalı, oyuncu iki kabinde de aynı yerel noktada; göz hiçbir şey fark etmez.
        OyuncuyuKabineTasi(pano8.Kabin, pano != null ? pano.Kabin : null);
        pano8.KabinAyrildi("9");
        if (pano != null)
        {
            pano.KabinGeldiKapali(KatSayaci.CatiKati + 1, "9");
            pano.Ugultu(true);
            yield return new WaitForSeconds(0.9f);
            pano.Ugultu(false);
            KameraHareketi.Sars(0.12f, 0.5f);                     // varış sarsıntısı
            pano.VarisSesiCal();
            yield return new WaitForSeconds(0.4f);
            pano.DingCal();
            yield return new WaitForSeconds(0.5f);
            yield return pano.KapilariOynat(0f, 1f);
        }
        if (kontrol != null) kontrol.enabled = true;
        yolculukSuruyor = false;
        Debug.Log("[DOKUZ] Kapılar 1974'te açıldı.");
    }

    // Oyuncuyu bir kabinden öbürüne, aynı yerel konum ve bakışla taşır. CharacterController açıkken konum
    // elle değiştirilemez (kendi konumunu önbelleğe alır ve geri yazar): kapat-taşı-aç (SonsuzMerdiven gibi).
    private void OyuncuyuKabineTasi(Transform kaynak, Transform hedef)
    {
        if (oyuncu == null || kaynak == null || hedef == null) return;
        Vector3 yerel = kaynak.InverseTransformPoint(oyuncu.position);
        yerel.x = Mathf.Clamp(yerel.x, -0.4f, 0.4f);              // kabinin içinde kalsın
        if (govde != null) govde.enabled = false;
        oyuncu.position = hedef.TransformPoint(yerel);
        if (govde != null) govde.enabled = true;
        Physics.SyncTransforms();
    }

    // ---------- 2. ÇİVİ VE SALON KAPISI ----------

    private void LambaAsildi() { StartCoroutine(SalonuAc()); }

    private IEnumerator SalonuAc()
    {
        yield return new WaitForSeconds(0.9f);
        if (salonKapisi == null) yield break;
        salonKapisi.Kilitle(false);
        if (!salonKapisi.Acik) salonKapisi.Etkilesim(null);   // gıcırtısıyla, kendiliğinden
        Debug.Log("[DOKUZ] Salon kapısı kendiliğinden açıldı.");
    }

    // ---------- 3. ANAHTAR VE KAÇIŞ ----------

    private void Update()
    {
        if (!Icinde || yolculukSuruyor) return;
        // Anahtar alındı mı? Esya anahtarı cebe koyar; biz cebi izleriz (nesneye kod bağlamadan, #104 ruhu).
        if (!anahtarAlindi && Cep.Tasiyor(EsyaTuru.KatAnahtari, KatSayaci.CatiKati)) AnahtarAlindi();
        // Kaçış: anahtar elde, oyuncu kabinde → kapılar kapanır, iniş.
        if (anahtarAlindi && !donusBasladi && pano != null && pano.OyuncuKabinde()) StartCoroutine(Donus());
    }

    private void AnahtarAlindi()
    {
        anahtarAlindi = true;
        Debug.Log("[DOKUZ] Çatı anahtarı tabaktan alındı: Gölge kalkıyor.");
        // Anahtarlık şıkırdar (#105), salonun ışığı söner, Gölge ayağa kalkar.
        if (golge != null)
        {
            GolgeImzasi.Cal(golge.transform.position + Vector3.up * 1.2f, 1f);
            GolgeAnimasyonu anim = golge.GetComponent<GolgeAnimasyonu>();
            if (anim != null) anim.Oturuyor = false;
            Vector3 p = golge.transform.position;
            golge.transform.position = new Vector3(p.x, golgeOturmaYeri.y - OturmaYuksekligi + KalcaYuksekligi(), p.z);
        }
        if (elektrik != null) elektrik.Ayarla(false);
        StartCoroutine(CividekiLambayiTitret());
        ArkaMuzik.Ornek?.Kabar();
    }

    // Gölge'nin kalça yüksekliği: bacak parçasının pivotu kalçadadır (golge.py). Otururken kök bu kadar
    // alçaltılır ki kalça minderin üstüne gelsin; kalkınca kök yere (y = taban) döner.
    private float KalcaYuksekligi()
    {
        Transform bacak = golge != null ? golge.transform.Find("Govde/Bacak_Sol") : null;
        return bacak != null ? bacak.localPosition.y : 0.95f;
    }

    private IEnumerator CividekiLambayiTitret()
    {
        if (asiliLambaIsigi == null) yield break;
        float temel = asiliLambaIsigi.intensity;
        for (float t = 0f; t < 1.6f; t += Time.deltaTime)
        {
            asiliLambaIsigi.intensity = temel * (Random.value < 0.35f ? Random.Range(0.1f, 0.5f) : 1f);
            yield return null;
        }
        asiliLambaIsigi.intensity = temel;
    }

    private IEnumerator Donus()
    {
        donusBasladi = true;
        if (kontrol != null) kontrol.enabled = false;
        if (kacis != null) kacis.Kacti();
        Debug.Log("[DOKUZ] Oyuncu kabinde: kapılar kapanıyor, iniş.");
        // Kapılar kapanırken Gölge yetişir ve kapıya vurur (iki darbe); kapılar kapanınca Gölge görünmez.
        StartCoroutine(KapiyaVur());
        yield return pano.KapilariOynat(1f, 0f);
        if (golge != null) golge.SetActive(false);
        pano.KalkisSesiCal();
        pano.Ugultu(true);
        KameraHareketi.Sars(0.10f, 0.7f);
        yield return new WaitForSeconds(1.4f);
        pano.GostergeYaz("8");
        yield return new WaitForSeconds(2.0f);
        if (radyo != null) radyo.Stop();
        OyuncuyuKabineTasi(pano.Kabin, pano8 != null ? pano8.Kabin : null);
        pano.KabinAyrildi("8");
        Icinde = false;
        Yasandi = true;
        if (pano8 != null)
        {
            pano8.KabinGeldiKapali(KatSayaci.CatiKati, "8");
            pano8.Ugultu(true);
            yield return new WaitForSeconds(0.8f);
            pano8.Ugultu(false);
            pano8.VarisSesiCal();
            yield return new WaitForSeconds(0.4f);
            pano8.DingCal();
            yield return new WaitForSeconds(0.5f);
            yield return pano8.KapilariOynat(0f, 1f);
        }
        if (kontrol != null) kontrol.enabled = true;
        Debug.Log("[DOKUZ] 8. kata dönüldü; çatı anahtarı elde. Bölüm bitti.");
    }

    private IEnumerator KapiyaVur()
    {
        if (pano == null) yield break;
        Vector3 kapi = pano.transform.parent.TransformPoint(new Vector3(0f, 1.2f, -MekanOlculeri.SahanlikDerin));
        yield return new WaitForSeconds(1.0f);
        if (carpmaSesi != null) AudioSource.PlayClipAtPoint(carpmaSesi, kapi, 1f);
        KameraHareketi.Sars(0.18f, 0.3f);
        yield return new WaitForSeconds(0.7f);
        if (carpmaSesi != null) AudioSource.PlayClipAtPoint(carpmaSesi, kapi, 0.9f);
        KameraHareketi.Sars(0.14f, 0.3f);
    }

    // ---------- 4. YAKALANMA: BÖLÜM BAŞA ----------

    private void Yakalandi(int kat)
    {
        if (!Icinde || donusBasladi) return;   // iniş başlamışsa Gölge kapıda kalır; bölüm başa dönmez
        StartCoroutine(BastanBasla());
    }

    private IEnumerator BastanBasla()
    {
        if (kontrol != null) kontrol.enabled = false;
        for (float t = 0f; t < 0.35f; t += Time.deltaTime) { karartma = t / 0.35f; yield return null; }
        karartma = 1f;
        Cep.Bosalt();
        BolumuKur();
        // Oyuncu kabinin ortasında, yüzü sahanlığa dönük (kabin kapısı +z'ye bakar); kapılar açık.
        if (pano != null && pano.Kabin != null && oyuncu != null)
        {
            if (govde != null) govde.enabled = false;
            oyuncu.position = pano.Kabin.TransformPoint(new Vector3(0f, 0.02f, -MekanOlculeri.SahanlikDerin - MekanOlculeri.DuvarKalin - 0.6f));
            oyuncu.rotation = Quaternion.identity;
            if (govde != null) govde.enabled = true;
            Physics.SyncTransforms();
            pano.KabinGeldiKapali(KatSayaci.CatiKati + 1, "9");
            pano.KapilariKoyDisaridan(1f);
        }
        Debug.Log("[DOKUZ] Yakalandı: bölüm kabinin açılışından yeniden başlıyor (kayıt değişmedi).");
        yield return new WaitForSeconds(1.2f);
        if (kontrol != null) kontrol.enabled = true;
        for (float t = 1f; t > 0f; t -= Time.deltaTime / 1.2f) { karartma = t; yield return null; }
        karartma = 0f;
    }

    // Bölümün başlangıç hâli: anahtar tabakta, Gölge sandalyede, salon kapalı ve kilitli, ışığı yanık,
    // çivi boş (lamba oyuncuda), kaçış anahtarı bekliyor. Hem Start'ta hem yakalanınca.
    private void BolumuKur()
    {
        anahtarAlindi = false;
        donusBasladi = false;
        if (anahtar != null) Destroy(anahtar);
        if (anahtarSablonu != null && anahtarYeri != null)
        {
            anahtar = Instantiate(anahtarSablonu, anahtarYeri, false);
            anahtar.name = "Cati_Anahtari";
            anahtar.transform.localPosition = new Vector3(0.02f, 0.012f, -0.01f);
            anahtar.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            Esya e = anahtar.GetComponent<Esya>();
            if (e != null) e.Ayarla(EsyaTuru.KatAnahtari, "cati_anahtari", KatSayaci.CatiKati);
        }
        if (golge != null)
        {
            golge.SetActive(true);
            GolgeAnimasyonu anim = golge.GetComponent<GolgeAnimasyonu>();
            if (anim != null) anim.Oturuyor = true;
            golge.transform.position = golgeOturmaYeri;
            golge.transform.rotation = golgeOturmaDonusu;
        }
        if (salonKapisi != null) { salonKapisi.Ayarla(false); salonKapisi.Kilitle(true); }
        if (elektrik != null) elektrik.Ayarla(true);
        if (lambalar != null) lambalar.Ayarla(new[] { "Salon" });
        if (civi != null) civi.Sifirla();
        // Kaçış anı (7. katla aynı bileşen): şablon yerine oturan Gölge, kat kapısı yerine kabin kapısı,
        // yakalanınca kat yerine bu bölüm başa döner.
        if (kacis == null)
        {
            kacis = gameObject.AddComponent<KacisAni>();
            kacis.HazirGolge = golge;
            kacis.YakalanmaIsleyici = Yakalandi;
            kacis.KapidanKacis = false;
            kacis.Hazirla(KatSayaci.CatiKati, golge, KacisBeklemesi, KacisHizi, null, null);
        }
        else kacis.YenidenBekle();
    }

    // Yakalanma kararması: KatYoneticisi'ninki gibi, ekran boyutuyla ölçeklenen tam ekran siyah.
    private void OnGUI()
    {
        if (karartma <= 0f) return;
        ArayuzOlcegi.Uygula();
        GUI.color = new Color(0f, 0f, 0f, karartma);
        GUI.DrawTexture(new Rect(0f, 0f, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
