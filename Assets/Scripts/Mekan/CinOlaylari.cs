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
//   #137 (Şahan'ın test turu, 7/9/10/11): AyakIzi (dairede: arkadan adımlar ıslak izler bırakır, izler kapalı
//   bir odanın kapısında biter), KapiGozu (sahanlıkta: bir kapının gözetleme deliği yanar, bakınca söner),
//   Zil (sahanlıkta: görüş dışı bir kapının zili, koşuda en çok 2), EsyaDonmesi (dairede: sandalye/manken
//   oyuncuya döner, çerçeve yüzünü duvara çevirir). Hiçbiri yakalamaz; zil ve kapı gözü imza çalmaz (varlık değil,
//   "içerideki biri" hissi).
//   #138: AsansorGecis (sahanlıkta, 2-7. katlar, katta bir kez): ARIZALI asansör kendi kendine çalışır, kabin
//   durmadan geçer (uğultu, gösterge, cam yarıklarında süzülen ışık, kapı önüne vuran ışık).
//   #135 (Şahan'ın test turu): geçiş 2,5 m/sn (4'te 0,5 sn'de bitiyordu), belirme 2-5 m (fenerin menzili),
//   belirmelerin yarısı 3. kattan itibaren KARARTMALI: figür dururken kafa lambası 1,3 sn ölür, karanlıkta
//   yalnız gözleri (GozParlamasi) görünür ve 1-1,5 m YAKLAŞIR, lamba yanınca titrer ve figür yok olur. Her
//   görünür olaydan sonra SESSİZLİK: uğultu, TV ve müzik 1 sn'de susar, 15-20 sn sessiz kalır, 3 sn'de döner.
//
// KURALLAR:
//   - Olaylar Varlik.Gurultu ÇAĞIRMAZ (oyuncunun gürültüsü değil, cinin işi). GolgeImzasi yalnız bir figür
//     GÖRÜNMEDEN hemen önce çalar (Belirme, Gecis, Kacis; CLAUDE.md: "varlığın her görünüşü imzayla başlar");
//     sesli olaylar, kapı gözü, zil ve asansör imza çalmaz (varlık değil, "içerideki biri").
//   - Saklanırken, av sürerken, 6. katın saklanma anı ve 7. katın kaçışı sırasında, menüde, açılışta ve
//     dokuzuncu katta olay yok. Saat sahanlıkta da işler (#133): daire olayları dairede; sahanlık olayları
//     (kapı gözü, zil, asansör) yalnız oyuncu A sahanlığının zeminindeyken (#139: üst sahanlıkta, merdivende,
//     zemin katta değil). 1. kat yalnız sesli olay alır, zemin katta olay yok (#141, CinOlayKurali.KattaIzinli).
//   - SEÇİM TEMBEL (#141): kural torbadaki sırayla türleri tek tek sorar (Hazirla); yalnız sorulan türün yeri
//     aranır ve bulunan yer olaya verilir. Olay yerini ikinci kez aramaz (#139: ikinci arama rastgele başarısız
//     olunca olay sessizce yutuluyor, figür torbadan boşa çıkıyordu).
//   - Kat yerinde baştan kurulunca (yakalanma) KatYoneticisi Sifirla()'yı çağırır (#139).
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
    [SerializeField] private Material[] ayakIziMalzemeleri;   // #137: ıslak ayak izi çıkartmaları (0 sol, 1 sağ); LekeKurucu üretir
    [SerializeField] private AudioClip zilSesi;               // #137: kapı zili (Zil_DingDong, S45)

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
    // #141 (B4): sessizlik süresi kata göre CinOlayKurali.SessizlikAraligi'nden gelir (1. kat 15-20 sn, 8. kat 8-12 sn).
    private const float KatDegisinceCikis = 0.8f;                     // #141: kat değişince sessizlik bu sürede biter (pat diye değil)
    private const float SessizlikInis = 1f, SessizlikCikis = 3f;      // #135: susma ve geri gelme süreleri
    private Coroutine sessizlik;                                      // #135: süren sessizlik (yoksa null)
    // #137
    private const float IzAdimi = 0.5f, IzYanKayma = 0.1f;            // ayak izleri: adım aralığı ve sağ-sol kayma (metre)
    private const int IzEnAz = 5, IzEnCok = 10;                       // bir yoldaki iz sayısı
    private const float IzBaslangicGeri = 2.5f;                       // izler oyuncunun bu kadar arkasında başlar
    private static readonly Vector3 IzBoyutu = new Vector3(0.13f, 0.30f, 0.12f);   // en (x), boy (y), derinlik (z)
    private const float KapiGozuSiddeti = 0.28f;                      // olay sırasında gözetleme deliğinin ışığı (kart 0,15)
    private const float KapiGozuBakma = 0.35f, KapiGozuZamanAsimi = 25f;   // bakınca sönmesi için bakış süresi; en çok açık kalma
    private const int ZilEnCokKosu = 2;                               // koşuda en çok zil
    private static int zilSayisi;                                     // koşudaki zil sayısı (sahne yüklenince sıfır)
    private int zilKati = -1;                                         // bu katta zil çaldı mı
    private readonly List<(Transform nesne, Quaternion donus)> dondurulenler = new List<(Transform, Quaternion)>();   // eşya dönmesi: kat değişince geri
    // #138
    private const int AsansorEnAzKat = 2;                             // 1. katta olmaz (oyuncu binayı tanısın)
    private const float AsansorGecisSuresi = 2.6f;                    // kabinin kattan geçme süresi (ışık alttan üste)
    private const float AsansorCamParlakligi = 2.2f;                  // cam yarıklarındaki ışık (HDR, Unlit)
    private const float AsansorOnIsik = 0.45f, AsansorOnMenzil = 1.8f; // kapının önüne vuran ışık (1,1 kapıyı lamba gibi parlatıyordu)
    private int asansorKati = -1;                                     // bu katta asansör geçti mi
    [SerializeField] private Material camIsigiMalzemesi;              // #138: Kat_Isik_Kaynagi.mat (URP Unlit); GrayboxKurucu_Zincir bağlar

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SahneYuklenince()
    {
        zilSayisi = 0;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Yuklendi;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += Yuklendi;
    }
    private static void Yuklendi(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m) { zilSayisi = 0; }
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

    // #139 (A6) / #141 (B3): olay seçilirken bulunan yer burada saklanır ve olay onu kullanır.
    private IcKapi hazirKapi;              // çarpılacak / açılacak / vurulacak kapı
    private AcilirParca hazirParca;        // açılacak çekmece / kapak
    private Vector3 hazirBas, hazirSon;    // kırılma noktası, belirme yeri, geçiş / kaçış / iz yolunun iki ucu
    private Transform hazirNesne;          // kapı gözünün kapısı, zil, dönecek eşya
    private RadyoAnonsu hazirCihaz;        // cızırdayacak radyo / TV
    private AsansorPanosu hazirPano;       // kendi kendine geçecek asansörün panosu
    private AsansorPanosu gecenPano;       // #139 (A2): asansör geçişi sürüyorsa panosu (yarıda kalırsa Temizle susturur)
    // #141 (B3): Etkin() her kare sahneyi taramasın; kovalamaca / saklanma anı bileşenleri saniyede bir bulunur.
    private KacisAni[] kacislar = new KacisAni[0];
    private SaklanmaAni[] saklanmalar = new SaklanmaAni[0];
    private float anTaramaSayaci = 99f;
    // #139 (A4): dönecek eşya bir oda kapısına bundan yakınsa ya da dönünce kapladığı yer bu kadar büyüyorsa dönmez.
    private const float KapiyaEnYakin = 1f, IzinliBuyume = 0.12f;

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
    private void OnDestroy()
    {
        if (Ornek == this) Ornek = null;
        Ayarlar.Degisti -= IzleriAyarla;   // #139 (A8)
    }

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
        // #139 (A8): kalite değişince konmuş ayak izleri de açılıp kapansın. "+=" bir olaya (event) abone olmak:
        // Ayarlar.Degisti tetiklenince IzleriAyarla çağrılır. Bileşen yok olurken "-=" ile abonelik bırakılır.
        Ayarlar.Degisti += IzleriAyarla;
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

        // #142 (A3): av, 7. kat kovalamacası ya da 6. kat saklanma anı başladıysa sahnedeki cin figürü gider:
        // aynı anda iki yaratık görünmesin. (Menü/not açıkken figür olduğu yerde bekler, kesilmez.)
        if (golge != null && TehditSuruyor()) OlayiKes("başka bir tehdit başladı");

        bool uygun = Etkin();
        if (uygun) DairedeMi();
        // #133: saat sahanlıkta da işler (görünür olaylar için); daire içi olaylar aşağıda daire ister.
        uygun = uygun && suren == null;
        if (!kural.Ilerlet(Time.deltaTime, uygun)) return;
        if (!uygun) return;   // sayaç dolu ama şu an uygun değil: uygun olunca seçilir

        // #141 (B3): kural türleri torbadaki sırayla tek tek sorar; Hazirla yalnız sorulan türün yerini arar,
        // bulursa hazir* alanlarına yazar. "kural.Sec(Hazirla)": Hazirla metodunun ADINI veriyoruz, çağırmıyoruz
        // (parantez yok); kural onu ne zaman ve kaç kez çağıracağına kendisi karar verir.
        CinOlayi olay = kural.Sec(Hazirla);
        if (olay == CinOlayi.Yok) return;
        Debug.Log($"[CIN] {kat}. kat: {olay}");
        switch (olay)
        {
            case CinOlayi.KapiCarpma: suren = StartCoroutine(KapiyiCarp(hazirKapi)); break;
            case CinOlayi.Kirilma: Kir(hazirBas); break;
            case CinOlayi.BakmayincaDegisim: Degistir(hazirKapi, hazirParca); break;
            case CinOlayi.IceridenVurus: suren = StartCoroutine(IceridenVur(hazirKapi)); break;
            case CinOlayi.ArkadanAdim: suren = StartCoroutine(ArkadanYuru()); break;
            case CinOlayi.Cihaz: if (hazirCihaz != null) hazirCihaz.KisaParazit(); break;
            case CinOlayi.Belirme: suren = StartCoroutine(Belir(hazirBas)); break;
            case CinOlayi.Gecis: suren = StartCoroutine(Gec(hazirBas, hazirSon)); break;
            case CinOlayi.Kacis: suren = StartCoroutine(Kac(hazirBas, hazirSon)); break;
            case CinOlayi.AyakIzi: suren = StartCoroutine(AyakIzleri(hazirBas, hazirSon)); break;
            case CinOlayi.KapiGozu: suren = StartCoroutine(KapiGozuYan(hazirNesne)); break;
            case CinOlayi.Zil: ZilCal(hazirNesne); break;
            case CinOlayi.EsyaDonmesi: EsyaDondur(hazirNesne); break;
            case CinOlayi.AsansorGecis: suren = StartCoroutine(AsansorGec(hazirPano)); break;
        }
    }

    // #141 (B3): kuralın sorduğu TEK türün şu an yapılıp yapılamayacağına bakar; yapılabiliyorsa yerini hazir*
    // alanlarına yazar. Her çağrıda alanlar önce boşaltılır: kural "evet" alınca hemen durduğu için alanlarda
    // kalan, seçilen türün yeridir.
    private bool Hazirla(CinOlayi olay)
    {
        bool dairede = daire != null;
        bool sahanlikta = !dairede && SahanliktaMi();   // #139 (A3)
        hazirKapi = null; hazirParca = null; hazirNesne = null; hazirCihaz = null; hazirPano = null;
        // #142 (B1): 2. kattaki ilk karşılaşma dersi (Varlik, kayıtta "ilkav") oynanmadan 2. kat da 1. kat gibi
        // yalnız sesli olay alır. Ders "karanlık seni avdan saklar" der; ondan önce cin karanlıkta yaklaşırsa
        // oyuncu iki kuralı karıştırır (Docs/hikaye.md §17).
        if (kat == 2 && !CinOlayKurali.KattaIzinli(olay, 1) && !Kayit.KilitCozukMu(2, Varlik.OgretiKimligi)) return false;
        switch (olay)
        {
            case CinOlayi.KapiCarpma: return dairede && (hazirKapi = CarpilacakKapi()) != null;
            case CinOlayi.Kirilma: return dairede && KirilmaNoktasi(out hazirBas);
            case CinOlayi.BakmayincaDegisim:
                if (!dairede) return false;
                // #142 (B2): önce yazı tura (kapı mı, çekmece mi), yalnız seçilen aranır; bulunamazsa öteki.
                // Eskiden ikisi de aranıp biri atılıyordu: çekmece araması dairedeki bütün parçalara ışın atar.
                if (Random.value < 0.5f) { hazirKapi = DegisecekKapi(); if (hazirKapi == null) hazirParca = DegisecekParca(); }
                else { hazirParca = DegisecekParca(); if (hazirParca == null) hazirKapi = DegisecekKapi(); }
                return hazirKapi != null || hazirParca != null;
            case CinOlayi.IceridenVurus: return dairede && vurusSesi != null && (hazirKapi = VurulacakKapi()) != null;
            case CinOlayi.ArkadanAdim: return dairede;
            case CinOlayi.Cihaz: return dairede && (hazirCihaz = Cihaz()) != null;
            // #142 (A3): sahnede başka bir figür (gezen gölge, duvar gölgesi, ayna...) varken figür gösterilmez.
            case CinOlayi.Belirme: return Sablon() != null && !FigurYurumesi.SahnedeBaskasiVar(null) && BelirmeYeri(out hazirBas);
            case CinOlayi.Gecis: return Sablon() != null && !FigurYurumesi.SahnedeBaskasiVar(null) && GecisYolu(out hazirBas, out hazirSon);
            case CinOlayi.Kacis: return Sablon() != null && !FigurYurumesi.SahnedeBaskasiVar(null) && KacisYolu(out hazirBas, out hazirSon);
            case CinOlayi.AyakIzi: return dairede && IzMalzemesiVar() && AyakIziYolu(out hazirBas, out hazirSon);
            case CinOlayi.KapiGozu: return sahanlikta && (hazirNesne = KapiGozuAdayi()) != null;
            case CinOlayi.Zil: return sahanlikta && zilSesi != null && zilSayisi < ZilEnCokKosu && zilKati != kat && (hazirNesne = ZilAdayi()) != null;
            case CinOlayi.EsyaDonmesi: return dairede && (hazirNesne = DonecekEsya()) != null;
            case CinOlayi.AsansorGecis: return sahanlikta && (hazirPano = AsansorAdayi()) != null;
        }
        return false;
    }

    // #139 (A3): oyuncu A sahanlığının (şu anki katı gösteren sahanlığın) zemininde mi? Üst sahanlık (B, +3 m),
    // ara sahanlık (+1,5 m), merdiven ve zemin kat (-3 m) değil. Sahanlık olayları (kapı gözü, zil, asansör) A'da
    // olur; oyuncu başka yerdeyken olurlarsa görmediği yerde ışık yanar, koşunun iki zilinden biri boşa gider.
    private bool SahanliktaMi()
    {
        if (kat < 1) return false;
        Transform s = SahanlikA();
        if (s == null) return false;
        if (Mathf.Abs(oyuncu.position.y - s.position.y) > 0.5f) return false;
        return yonetici.SahanlikKati(s) == kat;
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
        return !TehditSuruyor();
    }

    // #142 (A3): binada şu an cin dışında bir tehdit var mı: av, 7. kat kovalamacası, 6. kat saklanma anı?
    // Varsa cin saati durur ve sahnedeki cin figürü kaldırılır (Update).
    private bool TehditSuruyor()
    {
        Varlik v = Varlik.Ornek;
        if (v != null && v.AvSuruyor) return true;
        // #141 (B3): kovalamaca / saklanma anı bileşenleri kat kurulurken eklenir; saniyede bir taranır (eskiden
        // her kare bütün sahne aranıyor, her kare yeni bir dizi yaratılıyordu).
        anTaramaSayaci += Time.deltaTime;
        if (anTaramaSayaci >= 1f) { anTaramaSayaci = 0f; kacislar = FindObjectsByType<KacisAni>(); saklanmalar = FindObjectsByType<SaklanmaAni>(); }
        foreach (KacisAni k in kacislar) if (k != null && k.Basladi) return true;
        foreach (SaklanmaAni sa in saklanmalar) if (sa != null && sa.Suruyor) return true;
        return false;
    }

    /// <summary>
    /// #142 (A3): süren görünür cin olayını keser: eşyordam durur, figür silinir. Av başlarken Varlik çağırır
    /// (avın başındaki lamba titremesi figürün kayboluşunu örter); kovalamaca ve saklanma anında Update çağırır.
    /// İzler, kırıklar ve dönen eşyalar kalır: onlar olmuş şeylerin izi, yaratık değil.
    /// </summary>
    public void OlayiKes(string neden)
    {
        if (golge == null) return;
        if (suren != null) { StopCoroutine(suren); suren = null; }
        Destroy(golge);
        golge = null;
        Debug.Log($"[CIN] görünür olay kesildi: {neden}.");
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
        // #142 (A5): Unity bileşen kapanınca eşyordamları kendisi durdurur ama "suren" kaydı dolu kalırdı;
        // bileşen yeniden açılınca Update "bir olay sürüyor" sanıp saati bir daha işletmezdi.
        suren = null;
        if (golge != null) Destroy(golge); golge = null;
        AsansoruSustur();
        SessizligiBitir(false);
    }

    /// <summary>
    /// #139 (A7): kat YERİNDE baştan kurulunca (av / kovalamaca / saklanma yakalaması) KatYoneticisi çağırır:
    /// süren olay durur; izler, kırıklar, figür silinir; dönen eşyalar yerine döner; katın saati baştan kurulur
    /// (ilk olay yine geç gelir). Kat numarası değişmediği için Update bunu kendisi fark edemez.
    /// </summary>
    public void Sifirla()
    {
        Temizle();
        if (kural != null && kat >= 0) kural.KatAyarla(kat);
    }

    private void Temizle()
    {
        if (suren != null) { StopCoroutine(suren); suren = null; }
        AsansoruSustur();          // #139 (A2): yarıda kalan asansör geçişinin uğultusu ve göstergesi
        SessizligiBitir(true);     // #141 (B4): ses pat diye değil, 0,8 sn'de geri gelir
        anTaramaSayaci = 99f;      // yeni katın kovalamaca / saklanma bileşenleri hemen taransın
        if (golge != null) { Destroy(golge); golge = null; }
        foreach (GameObject g in konanlar) if (g != null) Destroy(g);
        konanlar.Clear();
        // #137: döndürülen eşyalar eski duruşuna (daireler kapanınca da saklanır; yeni katta temiz dursun).
        foreach (var d in dondurulenler) if (d.nesne != null) d.nesne.localRotation = d.donus;
        dondurulenler.Clear();
        daire = null;
    }

    // #139 (A2): asansör geçişi yarıda kaldıysa kuyu uğultusu döngüde kalmasın, gösterge eski bir kat yazmasın.
    private void AsansoruSustur()
    {
        if (gecenPano == null) return;
        // #142 (A6): yalnız pano hâlâ "ölü" (arızalı) ise. 7. kattan 8'e çıkılırken geçiş yarıda kalırsa bu
        // satır 8. katın zinciri panoyu hazırladıktan SONRAKİ karede çalışır; hazırlanmış panoya dokunmamalı.
        if (gecenPano.Hal == AsansorPanosu.Durum.Olu)
        {
            gecenPano.Ugultu(false);
            gecenPano.GostergeYaz("-");
        }
        gecenPano = null;
    }

    // #139 (A8): kalite oyun ortasında Düşük'e alınınca konmuş ayak izleri de söner, geri alınınca yanar
    // (DaireLekeleri'nin duvar lekeleri için yaptığının aynısı).
    private void IzleriAyarla()
    {
        bool acik = Ayarlar.Kalite != AyarKurallari.KaliteDusuk;
        foreach (GameObject g in konanlar)
        {
            if (g == null) continue;
            var p = g.GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
            if (p != null) p.enabled = acik;
        }
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

    /// <summary>
    /// #140: varlığın öbür görünüşleri (av, 7. kat kovalamacası, gezen gölge, saklanma anı, ayna, duvar gölgesi)
    /// de aynı torbadan figür çeker (VarlikGorunusu); figür yoksa eski Gölge şablonu döner.
    /// </summary>
    public GameObject FigurAl() { return FigurSec(); }

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
    private IEnumerator Kac(Vector3 bas, Vector3 son)
    {
        // #139 (A6): yol seçimde bulundu (Hazirla), burada yeniden aranmaz. Aşağıdaki süslü parantez yalnız bir
        // "blok": değişkenlerin geçerli olduğu alanı sınırlar, koşul değildir.
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
                    // #140: öne eğilme, sıçrama ve yalpa artık figürün kendi FigurYurumesi bileşeninde (hızdan
                    // ölçülür; avda ve kovalamacada da aynı yürüyüş/koşu). Burada yalnız yol ve yön.
                    golge.transform.position = taban;
                    golge.transform.rotation = kosuDonusu;
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
        CinOlayKurali.SessizlikAraligi(kat, out float enAz, out float enCok);   // #141 (B4): üst katlarda kısa
        sessizlik = StartCoroutine(Sessizlik(Random.Range(enAz, enCok)));
    }

    // Sessizliği bitirir. yumusak: kat değişince (#141 B4) ses 0,8 sn'de geri gelir (merdivende "pat" diye
    // açılan uğultu kulağı tırmalıyordu); bileşen kapanırken (sahne değişimi, final) hemen.
    private void SessizligiBitir(bool yumusak)
    {
        if (sessizlik != null) { StopCoroutine(sessizlik); sessizlik = null; }
        if (yumusak && isActiveAndEnabled && (OrtamSesi.Kisma < 1f || ArkaMuzik.Kisma < 1f))
        {
            sessizlik = StartCoroutine(SessizliktenCik(KatDegisinceCikis));
            return;
        }
        Kis(1f);
    }

    private IEnumerator SessizliktenCik(float sure)
    {
        float bas = Mathf.Min(OrtamSesi.Kisma, ArkaMuzik.Kisma);
        for (float t = 0f; t < sure; t += Time.deltaTime) { Kis(Mathf.Lerp(bas, 1f, t / sure)); yield return null; }
        Kis(1f);
        sessizlik = null;
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

    // ---------- #137: ISLAK AYAK İZLERİ ----------
    // "Arkadan adım"ın gözle görülen sürümü: izler oyuncunun 2,5 m arkasında başlar ve dairede kapalı bir
    // odanın kapısına doğru gider (birisi arkadan geçip o odaya girmiş gibi). Her iz bir DecalProjector
    // (duvar lekeleriyle aynı yöntem, #87): yere bakar, y ekseni yürüme yönü; sol-sağ sırayla, 50 cm arayla,
    // 10 cm yana kaymış. Her iz konurken o noktadan zeminin adım sesi gelir (0,45 sn arayla). İzler kat
    // değişince silinir (konanlar). Düşük kalitede çıkartma çizilmez (DaireLekeleri kuralı): olay seçilmez.
    private bool IzMalzemesiVar()
    {
        if (Ayarlar.Kalite == AyarKurallari.KaliteDusuk) return false;
        return ayakIziMalzemeleri != null && ayakIziMalzemeleri.Length >= 2 && ayakIziMalzemeleri[0] != null && ayakIziMalzemeleri[1] != null;
    }

    // Yol: başlangıç oyuncunun arkasında (dairenin zemininde, görünmez), bitiş kapalı bir oda kapısının önü.
    // #139 (A5): başlangıç masa / kanepe üstü değil ZEMİN olmalı ve yol diz hizasında (30 cm) engelsiz olmalı
    // (eskiden duvarın öbür yanındaki kapıya düz çizgi çekiliyor, izler duvarın öte yanına düşüyordu). Bunun
    // için görünmeyen kapalı kapılar karıştırılır ve her biri için birkaç başlangıç noktası denenir.
    private bool AyakIziYolu(out Vector3 bas, out Vector3 son)
    {
        bas = son = Vector3.zero;
        if (daire == null) return false;
        var kapilar = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (k.Acik) continue;
            if (Vector3.Distance(k.transform.position, oyuncu.position) > VurusMesafesi) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            kapilar.Add(k);
        }
        for (int i = kapilar.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); IcKapi g = kapilar[i]; kapilar[i] = kapilar[j]; kapilar[j] = g; }
        foreach (IcKapi hedef in kapilar)
        {
            for (int deneme = 0; deneme < 4; deneme++)
            {
                Vector3 geri = -oyuncu.forward * IzBaslangicGeri + oyuncu.right * Random.Range(-0.8f, 0.8f);
                Vector3 ust = oyuncu.position + geri + Vector3.up * 1.2f;
                if (!Physics.Raycast(ust, Vector3.down, out RaycastHit h, 1.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (h.collider.transform.IsChildOf(oyuncu)) continue;
                DaireEksenleri e = h.collider.GetComponentInParent<DaireEksenleri>();
                if (e == null || e.transform != daire) continue;
                if (!Gorus.Arkada(kamera, h.point)) continue;
                if (Mathf.Abs(h.point.y - oyuncu.position.y) > 0.1f) continue;   // zemin; masa / kanepe üstü değil
                Vector3 b = h.point;
                // Kapının önü: kapıdan başlangıca doğru 45 cm.
                Vector3 kapi = hedef.transform.position; kapi.y = b.y;
                Vector3 yon = b - kapi; yon.y = 0f;
                if (yon.magnitude < 1.2f) continue;
                Vector3 s2 = kapi + yon.normalized * 0.45f;
                if (YolTikali(b, s2, hedef.transform)) continue;
                bas = b; son = s2;
                return true;
            }
        }
        return false;
    }

    // #139 (A5): a'dan b'ye diz hizasında (30 cm) düz bir çizgi bir duvara / mobilyaya çarpıyor mu? Oyuncunun
    // kendisi ve hedef kapı sayılmaz.
    private bool YolTikali(Vector3 a, Vector3 b, Transform hedefKapi)
    {
        Vector3 y = Vector3.up * 0.3f;
        Vector3 fark = b - a;
        if (fark.sqrMagnitude < 0.0001f) return false;
        foreach (RaycastHit h in Physics.RaycastAll(a + y, fark.normalized, fark.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (t.IsChildOf(oyuncu) || (hedefKapi != null && t.IsChildOf(hedefKapi))) continue;
            return true;
        }
        return false;
    }

    private IEnumerator AyakIzleri(Vector3 bas, Vector3 son)
    {
        // #139 (A6): yol seçimde bulundu (Hazirla).
        {
            Vector3 yon = son - bas; yon.y = 0f;
            int sayi = Mathf.Clamp(Mathf.RoundToInt(yon.magnitude / IzAdimi), IzEnAz, IzEnCok);
            Vector3 ileri = yon.normalized, yan = Vector3.Cross(Vector3.up, ileri).normalized;
            Debug.Log($"[CIN] ayak izleri: {sayi} iz, {yon.magnitude:0.0} m, {bas:F1} → kapı önü.");
            for (int i = 0; i < sayi; i++)
            {
                bool sol = i % 2 == 0;
                Vector3 nokta = Vector3.Lerp(bas, son, (i + 0.5f) / sayi) + yan * (sol ? -IzYanKayma : IzYanKayma);
                // Zemin: noktanın üstünden aşağı; zemin yoksa (mobilya içi) iz konmaz ama ses gelir.
                // #139 (A5): yalnız başlangıçla aynı zemin hizasına (±5 cm) iz konur; halı kenarı, masa, kanepe üstüne değil.
                if (Physics.Raycast(nokta + Vector3.up * 1f, Vector3.down, out RaycastHit h, 1.5f, ~0, QueryTriggerInteraction.Ignore)
                    && !h.collider.transform.IsChildOf(oyuncu) && Mathf.Abs(h.point.y - bas.y) < 0.05f)
                {
                    GameObject g = new GameObject(sol ? "Cin_Iz_Sol" : "Cin_Iz_Sag");
                    g.transform.SetParent(daire, true);
                    g.transform.position = h.point + Vector3.up * 0.02f;
                    g.transform.rotation = Quaternion.LookRotation(Vector3.down, ileri);   // z yere, y yürüme yönü
                    var p = g.AddComponent<UnityEngine.Rendering.Universal.DecalProjector>();
                    p.material = ayakIziMalzemeleri[sol ? 0 : 1];
                    p.size = IzBoyutu;
                    p.pivot = Vector3.zero;
                    p.drawDistance = 25f;
                    p.fadeScale = 0.6f;
                    p.enabled = Ayarlar.Kalite != AyarKurallari.KaliteDusuk;   // #139 (A8)
                    konanlar.Add(g);
                    nokta = h.point;
                }
                AudioClip klip = ZeminSesi.AdimKlibi(nokta, oyuncu);
                if (klip != null) Cal(klip, nokta + Vector3.up * 0.05f, 0.75f);
                yield return new WaitForSeconds(AdimAraligi);
            }
        }
        suren = null;
    }

    // ---------- #137: KAPI GÖZÜ IŞIĞI (sahanlık) ----------
    // Sahanlıktaki kapalı bir dairenin gözetleme deliği yanar (içeride biri ışığı açtı ve bakıyor); oyuncu
    // o kapıya dönüp 0,35 sn bakınca söner (bakıldığını anlayıp ışığı kapattı). 25 sn bakılmazsa kendiliğinden
    // söner. 7. katta (SahanlikKapisi'li kapılar: gerçek kapının ışığı ipucudur) ve açık dairenin kapısında olmaz.
    private Transform SahanlikA()
    {
        GameObject g = GameObject.Find("Sahanlik_A");
        return g != null ? g.transform : null;
    }

    private IEnumerable<Transform> SahanlikKapilari()
    {
        Transform s = SahanlikA();
        if (s == null) yield break;
        for (int i = 1; i <= 4; i++)
        {
            Transform k = s.Find("Kapi_" + i);
            if (k == null || !k.gameObject.activeInHierarchy) continue;
            if (k.GetComponent<SahanlikKapisi>() != null) yield break;   // 7. kat: gözlem katı, karışmayız
            Transform m = k.Find("Mentese");
            if (m == null || Mathf.Abs(Mathf.DeltaAngle(0f, m.localEulerAngles.y)) > 3f) continue;   // açık daire kapısı
            yield return k;
        }
    }

    private Transform KapiGozuAdayi()
    {
        var adaylar = new List<Transform>();
        foreach (Transform k in SahanlikKapilari())
        {
            Transform isik = k.Find("Mentese/Kapi_Gozu_Isik");
            if (isik == null || isik.gameObject.activeSelf) continue;
            if (Vector3.Distance(isik.position, oyuncu.position) > 9f) continue;
            // #142 (A4): oyuncunun baktığı kapı aday değil: ışık gözünün önünde yanıp hemen sönüyordu (olay
            // boşa gidiyordu). Işık görüş dışında yanar; oyuncu dönünce bir kapının gözünün yandığını görür.
            if (Gorunur(isik.position, k)) continue;
            adaylar.Add(k);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // Oyuncu bu noktaya bakıyor mu: ekranın ortadaki %50'sinde ve göz ile nokta arasında engel yok.
    private bool Bakiyor(Vector3 nokta)
    {
        if (kamera == null) return false;
        Vector3 e = kamera.WorldToViewportPoint(nokta);
        if (e.z <= 0f || e.x < 0.25f || e.x > 0.75f || e.y < 0.25f || e.y > 0.75f) return false;
        Vector3 goz = kamera.transform.position;
        foreach (RaycastHit h in Physics.RaycastAll(goz, nokta - goz, Vector3.Distance(goz, nokta) - 0.15f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.collider.transform.IsChildOf(oyuncu)) return false;
        return true;
    }

    // #139 (A1): sahnedeki Kapi_Gozu_Isik'e dokunulmaz. 7. katın kartı onu gerçek kapının ipucu olarak açar; olay
    // sürerken kat değişirse ışık yanık kalıyor, kartın geri alma defteri de onu "aslı yanık" diye tutuyordu.
    // Olay onun bir KOPYASINI yakar: kopya konanlar listesindedir, kat değişince / sıfırlanınca Temizle siler.
    private IEnumerator KapiGozuYan(Transform kapi)
    {
        Transform asil = kapi != null ? kapi.Find("Mentese/Kapi_Gozu_Isik") : null;
        if (asil != null)
        {
            GameObject kopya = Instantiate(asil.gameObject, asil.parent);
            kopya.name = "Cin_Kapi_Gozu";
            kopya.transform.localPosition = asil.localPosition;
            kopya.transform.localRotation = asil.localRotation;
            Light l = kopya.GetComponent<Light>();
            if (l != null) l.intensity = KapiGozuSiddeti;
            kopya.SetActive(true);
            konanlar.Add(kopya);
            Debug.Log($"[CIN] kapı gözü: {kapi.name} yandı.");
            float bakis = 0f, gecen = 0f;
            while (gecen < KapiGozuZamanAsimi && daire == null && kopya != null)
            {
                gecen += Time.deltaTime;
                bakis = Bakiyor(kopya.transform.position) ? bakis + Time.deltaTime : 0f;
                if (bakis >= KapiGozuBakma) break;
                yield return null;
            }
            if (kopya != null) { konanlar.Remove(kopya); Destroy(kopya); }
            Debug.Log($"[CIN] kapı gözü: {(bakis >= KapiGozuBakma ? "bakılınca söndü" : "söndü")} ({gecen:0.0} sn).");
        }
        suren = null;
    }

    // ---------- #137: ZİL (sahanlık) ----------
    // Görüş dışındaki bir kapının zili kendi kendine çalar; koşuda en çok 2 kez, katta bir kez. Zil nesnesi
    // kapının çocuğudur (Kapi_N/Zil); ses oradan gelir (kapı kökü sahanlığın ortasındadır, CLAUDE.md).
    private Transform ZilAdayi()
    {
        var adaylar = new List<Transform>();
        foreach (Transform k in SahanlikKapilari())
        {
            Transform zil = k.Find("Zil");
            if (zil == null || Gorunur(zil.position, k)) continue;
            adaylar.Add(zil);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    private void ZilCal(Transform zil)
    {
        if (zil == null || zilSesi == null) return;
        zilSayisi++; zilKati = kat;
        Cal(zilSesi, zil.position, 0.9f);
        Titresim.Vur(0.2f, 0.5f, 0.25f);
        Debug.Log($"[CIN] zil: {zil.parent.name} çaldı ({zilSayisi}/{ZilEnCokKosu}).");
    }

    // ---------- #137: BAKMAYINCA DÖNEN EŞYA ----------
    // Görüş dışındaki bir sandalye ya da manken oyuncuya döner (oda kapısından içeri bakınca "bana bakıyor"),
    // ya da bir aile fotoğrafı çerçevesi yüzünü duvara çevirir (ayaklı çerçeve 180° döner, duvar çerçevesi
    // baş aşağı). 6. katın kendi anı olan Manken_Hol_Sabri'ye dokunulmaz. Dönüşler kat değişince geri alınır.
    private Transform DonecekEsya()
    {
        if (daire == null) return null;
        var adaylar = new List<Transform>();
        KatAni[] anlar = FindObjectsByType<KatAni>();              // #139 (A4): anların nesneleri
        IcKapi[] kapilar = daire.GetComponentsInChildren<IcKapi>(); // #139 (A4): kapı ağızları
        foreach (Transform t in daire.GetComponentsInChildren<Transform>())
        {
            if (!t.gameObject.activeInHierarchy) continue;
            string ad = t.name;
            bool sandalye = ad.Contains("Sandalye") && !ad.Contains("Bekleme");
            bool manken = ad.StartsWith("Manken") && ad != "Manken_Hol_Sabri";
            bool foto = t.GetComponent<AileFotografi>() != null;
            if (!sandalye && !manken && !foto) continue;
            if (t.parent != null && (t.parent.name.Contains("Sandalye") || t.parent.name.StartsWith("Manken"))) continue;   // parça değil, kök
            bool donmus = false; foreach (var d in dondurulenler) if (d.nesne == t) donmus = true;
            if (donmus) continue;
            if (AninNesnesi(t, anlar)) continue;                    // #139 (A4): sallanan sandalye gibi bir anın nesnesi
            if (!foto && !DonmeyeUygun(t, kapilar)) continue;       // #139 (A4): dönünce yolu / kapıyı kapatmasın
            Renderer r = t.GetComponentInChildren<Renderer>();
            Vector3 nokta = r != null ? r.bounds.center : t.position + Vector3.up * 0.5f;
            float m = Vector3.Distance(nokta, oyuncu.position);
            if (m < 1.5f || m > 12f || Gorunur(nokta, t)) continue;
            adaylar.Add(t);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // #139 (A4): t bir katın anının (KatAni) nesnesi mi ya da onun içinde / dışında mı? 2. katın sallanan
    // sandalyesi cin tarafından döndürülürse, an başlayınca kaydettiği duruşa zıplıyordu.
    private static bool AninNesnesi(Transform t, KatAni[] anlar)
    {
        foreach (KatAni a in anlar)
        {
            if (a == null || a.Hedef == null) continue;
            if (a.Hedef == t || a.Hedef.IsChildOf(t) || t.IsChildOf(a.Hedef)) return true;
        }
        return false;
    }

    // #139 (A4): sandalye / manken oyuncuya dönerse yol kapanır mı? Bir oda kapısının 1 m yakınındaysa dönmez;
    // değilse dönüş DENENİR, kapladığı yer (görünür kutusunun yatay ölçüsü) ölçülür ve dönüş geri alınır.
    // Kutu bir eksende 12 cm'den çok büyüyorsa (uzun bir manken yana dönünce koridoru kapatır) dönmez.
    private bool DonmeyeUygun(Transform t, IcKapi[] kapilar)
    {
        foreach (IcKapi k in kapilar)
        {
            Vector3 f = k.transform.position - t.position; f.y = 0f;
            if (f.magnitude < KapiyaEnYakin) return false;
        }
        Vector3 bakis = oyuncu.position - t.position; bakis.y = 0f;
        if (bakis.sqrMagnitude < 0.001f) return false;
        Bounds once = GorunurKutu(t);
        Quaternion eski = t.rotation;
        t.rotation = Quaternion.LookRotation(bakis.normalized);
        Bounds sonra = GorunurKutu(t);
        t.rotation = eski;
        return sonra.size.x - once.size.x < IzinliBuyume && sonra.size.z - once.size.z < IzinliBuyume;
    }

    // Bir nesnenin bütün çizicilerini saran dünya kutusu (Renderer.bounds dönüşten hemen sonra güncellenir).
    private static Bounds GorunurKutu(Transform t)
    {
        Renderer[] rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    private void EsyaDondur(Transform t)
    {
        if (t == null) return;
        dondurulenler.Add((t, t.localRotation));
        if (t.GetComponent<AileFotografi>() != null)
        {
            // Duvar çerçevesi (yüksekte) baş aşağı; ayaklı çerçeve (masada) yüzünü duvara.
            bool duvarda = t.position.y - oyuncu.position.y > 1.2f;
            t.rotation = duvarda ? t.rotation * Quaternion.Euler(0f, 0f, 180f) : t.rotation * Quaternion.Euler(0f, 180f, 0f);
            Debug.Log($"[CIN] eşya dönmesi: {t.name} {(duvarda ? "baş aşağı" : "yüzünü duvara")} döndü.");
        }
        else
        {
            Vector3 bakis = oyuncu.position - t.position; bakis.y = 0f;
            if (bakis.sqrMagnitude > 0.001f) t.rotation = Quaternion.LookRotation(bakis.normalized);
            Physics.SyncTransforms();
            Debug.Log($"[CIN] eşya dönmesi: {t.name} oyuncuya döndü.");
        }
    }

    // ---------- #138: ASANSÖR KENDİ KENDİNE GEÇER (sahanlık) ----------
    // "ARIZALI" asansör (oyuncu notu okudu, elektriği yok) birden çalışır: kuyudan motor uğultusu yükselir,
    // gösterge alttaki katı, sonra bu katı, sonra üsttekini yazar; kabin geçerken kapı kanatlarının cam
    // yarıkları ışıkla dolar (alttan üste süzülen bir parıltı) ve kapının önüne, yere sıcak bir ışık vurur;
    // kabin DURMAZ, gösterge "-"ye döner, uğultu söner. Kimse çağırmadı, kimse inmedi.
    // 8. katta (asansör gerçekten çalışır, zincirin parçası) ve 1. katta olmaz; katta bir kez. Pano
    // bileşeni yalnız gösterge ve uğultu için kullanılır (GostergeYaz, Ugultu); hâlini değiştirmeyiz.
    // Cam yarıkları: Blender modelinde (asansor.py) her kanatta kanatların birleşme çizgisine 12 cm uzaklıkta,
    // 1,45 m yükseklikte 9 × 62 cm koyu cam. Cam opak (kuyunun karanlığını gösterir); ışık, camın 3 mm
    // önüne konan iki Unlit dörtgenle ve önüne konan gölgesiz bir nokta ışıkla "sızar".
    private AsansorPanosu AsansorAdayi()
    {
        if (kat < AsansorEnAzKat || kat == KatSayaci.CatiKati || asansorKati == kat) return null;
        Transform s = SahanlikA();
        Transform a = s != null ? s.Find("Asansor") : null;
        if (a == null) return null;
        AsansorPanosu p = a.GetComponentInChildren<AsansorPanosu>();
        if (p == null || p.Hal != AsansorPanosu.Durum.Olu) return null;
        if (Vector3.Distance(a.position, oyuncu.position) > 12f) return null;
        return p;
    }

    private IEnumerator AsansorGec(AsansorPanosu pano)
    {
        if (pano != null)
        {
            asansorKati = kat;
            gecenPano = pano;   // #139 (A2): yarıda kalırsa Temizle uğultuyu durdurur, göstergeyi "-" yapar
            Transform a = pano.transform.parent;   // Asansor kökü
            Debug.Log($"[CIN] asansör: {kat}. katta kabin geçiyor.");
            Titresim.Vur(0.25f, 0.15f, 1.2f);
            pano.Ugultu(true);
            pano.GostergeYaz((kat - 1).ToString());
            yield return new WaitForSeconds(1.6f);
            // Kabin geçiyor: gösterge bu kat; camlar ve kapı önü alttan üste aydınlanır.
            pano.GostergeYaz(kat.ToString());
            Transform kanat = a.Find("Kapi_Sol");
            Renderer kr = kanat != null ? kanat.GetComponent<Renderer>() : null;
            float onYuz = kr != null ? kr.bounds.max.z : a.TransformPoint(new Vector3(0f, 0f, -MekanOlculeri.SahanlikDerin)).z;
            var camlar = new List<(Renderer r, MaterialPropertyBlock b)>();
            if (camIsigiMalzemesi != null)
            {
                foreach (float x in new[] { -0.12f, 0.12f })
                {
                    GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    q.name = "Cin_Asansor_Cam";
                    Destroy(q.GetComponent<Collider>());
                    q.transform.SetParent(a, false);
                    Vector3 yer = a.InverseTransformPoint(new Vector3(a.position.x + x, a.position.y + 1.45f, onYuz + 0.003f));
                    q.transform.localPosition = yer;
                    q.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // Unity'nin Quad'ı -z'ye bakar; sahanlık +z'de: 180° çevrilir
                    q.transform.localScale = new Vector3(0.09f, 0.62f, 1f);
                    var r = q.GetComponent<MeshRenderer>();
                    r.sharedMaterial = camIsigiMalzemesi;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    camlar.Add((r, new MaterialPropertyBlock()));
                    konanlar.Add(q);
                }
            }
            GameObject isikNesnesi = new GameObject("Cin_Asansor_Isigi");
            isikNesnesi.transform.SetParent(a, false);
            Light on = isikNesnesi.AddComponent<Light>();
            on.type = LightType.Point; on.color = new Color(1f, 0.84f, 0.6f); on.range = AsansorOnMenzil; on.shadows = LightShadows.None; on.intensity = 0f;
            konanlar.Add(isikNesnesi);
            for (float t = 0f; t < AsansorGecisSuresi; t += Time.deltaTime)
            {
                float u = t / AsansorGecisSuresi;                       // 0 → 1: kabin alttan üste
                float parlak = Mathf.Sin(u * Mathf.PI);                 // ortada en parlak
                foreach (var c in camlar)
                {
                    c.b.SetColor("_BaseColor", new Color(1f, 0.85f, 0.6f) * (AsansorCamParlakligi * parlak));
                    c.r.SetPropertyBlock(c.b);
                }
                isikNesnesi.transform.position = new Vector3(a.position.x, a.position.y + Mathf.Lerp(0.2f, 2.3f, u), onYuz + 0.35f);
                on.intensity = AsansorOnIsik * parlak;
                yield return null;
            }
            foreach (var c in camlar) if (c.r != null) Destroy(c.r.gameObject);
            Destroy(isikNesnesi);
            pano.GostergeYaz((kat + 1).ToString());
            yield return new WaitForSeconds(1.4f);
            pano.Ugultu(false);
            yield return new WaitForSeconds(0.8f);
            pano.GostergeYaz("-");
            gecenPano = null;
            Debug.Log("[CIN] asansör: geçti, gösterge \"-\".");
        }
        suren = null;
    }

    // ---------- GÖRÜNÜR OLAYLAR (#133) ----------
    // Belirme: imza, figür görüşte belirir, oyuncuya dönük durur, lamba titrer, kaybolur. Yakalama yok.
    // #135: 3. kattan itibaren belirmelerin yarısı KARARTMALI: lamba ölür (KafaLambasi.Sondur; oyuncunun
    // düğmesi işlemez), karanlıkta yalnız gözler parlar (GozParlamasi) ve figür 1-1,5 m yaklaşır (ayakta
    // durabileceği bir nokta varsa; oyuncuya 1,2 m'den yakına asla gelmez), lamba kendiliğinden yanınca
    // titrer ve figür yok olur. Dokunmaz.
    private IEnumerator Belir(Vector3 yer)
    {
        GameObject belirmeSablonu = FigurSec();
        if (belirmeSablonu != null)
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
                        // #142 (A1): bu bir adım değil, karanlıkta bir anda yer değiştirme: yürüyüş saymasın.
                        FigurYurumesi yuruyus = golge.GetComponent<FigurYurumesi>();
                        if (yuruyus != null) yuruyus.Isinlandi();
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
    private IEnumerator Gec(Vector3 bas, Vector3 son)
    {
        GameObject gecisSablonu = FigurSec();
        if (gecisSablonu != null)
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
    private IEnumerator KapiyiCarp(IcKapi kapi)
    {
        if (kapi != null)
        {
            Titresim.Vur(0.6f, 0.45f, 0.35f);
            if (lamba != null) lamba.Titret(0.5f);
            yield return kapi.Carp(0.16f, carpmaSesi);
        }
        suren = null;
    }

    private void Kir(Vector3 nokta)
    {
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
    private void Degistir(IcKapi k, AcilirParca p)
    {
        if (k != null) k.Ayarla(true);
        else if (p != null) p.Ayarla(true);
    }

    private IEnumerator IceridenVur(IcKapi kapi)
    {
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
