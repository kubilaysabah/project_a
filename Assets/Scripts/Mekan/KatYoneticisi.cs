// KatYoneticisi.cs
// KatSayaci (saf mantık) ile sahne (Unity) arasındaki KÖPRÜ.
//
// İş bölümü şöyle:
//   KatSayaci      -> kuralı bilir, Unity'yi bilmez. Test edilebilir.
//   AnomaliSecici  -> hangi katta hangi anomalinin olacağını bilir. Test edilebilir.
//   CopPlani       -> hangi katta kaç poşet olacağını bilir. Test edilebilir.
//   KatYoneticisi  -> sahneyi bilir, kuralı bilmez. Oyuncunun ne yaptığını
//                     izler, ötekilere sorar, gelen cevabı sahnede uygular.

using System.Collections;          // IEnumerator (coroutine) için
using UnityEngine;

// ---------- ÇALIŞMA SIRASI ----------
// Unity aynı karede birçok scriptin LateUpdate'ini çağırır; sırayı normalde
// kendisi seçer. DefaultExecutionOrder bu sırayı sabitler: sayı büyüdükçe
// script daha SONRA çalışır. SonsuzMerdiven varsayılan sırada (0) çalışıyor;
// bu script 100 ile ondan SONRA çalışıyor.
//
// Neden şart? Oyuncu üst kata vardığı karede SonsuzMerdiven onu A'ya taşır,
// hemen ardından burada karar verilir ve sahanlık yeni katın görünümüne geçer.
// İkisi aynı karede olduğu için ekrana çizilen ilk görüntü zaten yenilenmiş
// sahanlıktır. Sıra ters olsaydı, oyuncu bir kare boyunca ESKİ katın sahanlığını
// görür ve sonra her şey birden değişirdi.
[DefaultExecutionOrder(100)]
public class KatYoneticisi : MonoBehaviour
{
    [Header("Bağlantılar")]
    [Tooltip("Oyuncudaki döngü scripti. Boş bırakılırsa sahnede aranır.")]
    [SerializeField] private SonsuzMerdiven merdiven;

    [Tooltip("Oyuncunun kendisi. Boş bırakılırsa döngü scriptinin sahibi kullanılır.")]
    [SerializeField] private Transform oyuncu;

    [Header("Eski kural (#59'a kadar)")]
    [Tooltip("Eski oyunun kuralı: anomalili katta geri dön, normal katta devam et; yanlış karar ya da " +
             "toplanmamış çöp 1. kata ceza. Yeni ilerlemede (kilitli kapılar, anahtarlar, #50) KAPALI " +
             "kalır; #59 kodunu tamamen kaldıracak. Açarsanız eski oyun aynen oynanır.")]
    [SerializeField] private bool eskiKural = false;

    [Header("Anomali")]
    [Tooltip("Bir katın anomalili olma olasılığı (yalnızca eski kural açıkken).")]
    [Range(0f, 1f)]
    [SerializeField] private float anomaliOlasiligi = 0.5f;

    [Tooltip("1. kat her zaman normal olsun mu? Oyuncunun önce NORMALİN neye " +
             "benzediğini görmesi gerekir; kapatmanız önerilmez.")]
    [SerializeField] private bool ilkKatHepNormal = true;

    [Tooltip("Rastgelelik tohumu. 0 = her oyunda farklı tur. Sıfırdan farklı bir " +
             "sayı verirseniz aynı tur birebir tekrar oynanır — hata ayıklarken işe yarar.")]
    [SerializeField] private int tohum = 0;

    [Header("Açılış (#16)")]
    [Tooltip("Oyun zemin kattaki kapıcı dairesinde, yönetici notuyla başlasın mı? " +
             "Kapatırsanız doğrudan 1. katta başlar; anomali test ederken işe yarar.")]
    [SerializeField] private bool acilisIleBasla = true;

    [Header("Test")]
    [Tooltip("Belirli bir kartı denemek için: buraya bir kart sürüklenirse 2. kattan " +
             "itibaren HER kat bu kartla gelir (42 kart arasından rastgele denk gelmesini " +
             "beklemeye gerek kalmaz). Boş bırakıldığında oyun normal çalışır. " +
             "Test bitince boşaltmayı unutmayın.")]
    [SerializeField] private AnomaliKarti testKarti;

    [Tooltip("Ekranın köşesinde kat numarasını ve anomali durumunu gösterir. " +
             "Sadece test içindir; gerçek oyunda kapalı olacak.")]
    [SerializeField] private bool testGostergesi = true;

    [Header("Gecikmeli ceza")]
    [Tooltip("Bütün ampullerin sönmesinin toplam süresi (saniye). Ampul sayısından " +
             "bağımsız: sahnede kaç ampul olursa olsun karanlık bu sürede gelir.")]
    [SerializeField] private float sonmeSuresi = 1.75f;

    [Tooltip("Tam karanlıkta geçen süre. Oyunun en korkunç anı burasıdır.")]
    [SerializeField] private float karanlikSuresi = 1.4f;

    // ---------- İÇ DURUM ----------

    private KatSayaci sayac = new KatSayaci();

    private Vector3 baslangicKonumu;      // ceza sonrası dönülecek yer
    private Quaternion baslangicDonusu;   // ve bakılacak yön
    private bool cezaBekliyor;            // yanlış karar verildi, ışıklar sönmeyi bekliyor
    private bool cezaCalisiyor;           // ceza gösterisi şu an oynuyor

    private CharacterController govde;

    // Kararı ışınlanmadan değil, gerçekten kat edilen dikey mesafeden okuyan takipçi.
    // Oyuncu yere İLK BASTIĞI anda kurulur (bkz. LateUpdate), çünkü oyuncu sahneye
    // yerden birkaç santim yukarıda konur ve ilk karelerde düşer; o anki
    // yükseklikten ölçmeye başlasaydık bütün kararlar birkaç santim kayardı.
    private KatIlerlemeTakipcisi ilerleme;

    // Takipçinin kurulabilmesi için oyuncunun yere OTURMUŞ olması gerekiyor.
    // Bunu anlamak için bir önceki karedeki yüksekliği tutuyoruz (bkz. LateUpdate).
    private float oncekiY = float.NaN;

    // Son kararın verildiği andaki net döngü sayısı. A sahanlığının karar
    // noktasına göre kaç kat yukarıda/aşağıda durduğunu bununla hesaplıyoruz.
    private int referansDongu;

    private Light[] ampuller;             // sahnedeki bütün lamba ışıkları (spot + dolgu)
    private Color[] ozgunRenkler;

    // Ceza sesleri (#25): lamba sönerken "tık", son lamba titrerken cızırtı.
    private AudioClip lambaSonme, lambaCizirti;

    // Lambaların ANA ışıkları (aşağı bakan spotlar). Her lambanın dolgu ışığı
    // ve parlayan camı bu ışığın çocuğu; ceza sırasında lamba bütünüyle söner.
    private Light[] lambalar;

    // ---- SAHNE PARÇALARI ----
    private Transform sahanlikA, sahanlikB;
    private GameObject ustKatlar;         // B'nin üstündeki sahte katlar
    private GameObject catiBolumu;        // 8. katta B'nin yerine geçen çatı

    // Her sahanlıkta dört kapı numarası ve kapı başına üç poşet.
    // [,] iki boyutlu dizi demek: posetler[kapı, sıra]. Satranç tahtası gibi;
    // bir sayı satırı, öbürü sütunu seçer.
    private TextMesh[] aNumaralar, bNumaralar;
    private CopPoseti[,] aPosetler, bPosetler;

    // ---- ANOMALİ ----
    private AnomaliSecici secici;
    private AnomaliKarti[] kartlar;

    // İki sahanlık ayrı ayrı bozuluyor, o yüzden HER BİRİNİN kendi uygulayıcısı
    // ve kendi geri alma defteri var.
    private AnomaliUygulayici uygulayiciA = new AnomaliUygulayici();
    private AnomaliUygulayici uygulayiciB = new AnomaliUygulayici();

    // ---- ÇÖP ----
    private CopPlani copPlani;
    private CopToplayici toplayici;

    // ---- AYAKKABILAR (#44) ----
    // Her dairenin 5 raflı ayakkabılığı ve kapı önündeki 0-2 dağınık çift. Plan, katta
    // kaç çiftin nerede olduğunu ve hangisinin rafa konduğunu bilir (çöp planı gibi).
    private AyakkabiPlani ayakkabiPlani;
    private GameObject[,] aRaflar, bRaflar;              // [daire, raf] raftaki çiftler
    private DaginikAyakkabi[,] aDaginik, bDaginik;       // [daire, sıra] kapı önündeki çiftler

    // ---- KAT KAPISI VE ANAHTAR (#52) ----
    // Her çekirdeğin (A ve B) ara sahanlığında kilitli demir kapı, her gerçek sahanlıkta
    // katın anahtarı. Hangi katların kapısının açıldığı burada tutulur; kayıt (Kayit)
    // yalnızca ulaşılan katı saklar, alt katların kapıları yüklemede açık sayılır.
    private KatKapisi kapiA, kapiB;
    private KatAnahtari anahtarA, anahtarB;
    private readonly System.Collections.Generic.HashSet<int> acilanKatlar = new System.Collections.Generic.HashSet<int>();

    // ---- DAİRELER (#53) ----
    // A sahanlığındaki dört 3+1 daire (Daire_1..4, dizinin 0. yeri boş). Her katta yalnızca
    // biri açık (DairePlani.AcikDaire): o dairenin sahanlıktaki kapısı ardına kadar açılır,
    // kapının arkasındaki karanlık hol (Daire_Ici) kalkar, gerçek daire görünür.
    private readonly GameObject[] daireler = new GameObject[MekanOlculeri.DaireSayisi + 1];

    // ---- KİLİT ZİNCİRİ (#54) ----
    // Katların zincirleri: [kat - 1]. Boş olan katta daire kilitsizdir ve katın anahtarı
    // sahanlıktaki eski yerindedir. #55 sekiz katı dolduracak; şimdilik 1. katta test zinciri.
    [SerializeField] private KatZinciriVarligi[] katZincirleri;
    // Eşya şablonları (prefab): zincir eşyaları bunlardan kopyalanır. ZincirKurucu bağlar.
    [SerializeField] private GameObject odaAnahtariSablonu, katAnahtariSablonu, sigortaSablonu, tornavidaSablonu;
    [SerializeField] private GameObject cizimParcasiSablonu;   // yırtık çizimin yere serilen parçası (#63)
    [SerializeField] private GameObject levyeSablonu, kaydiriciSablonu;   // aletler (#65)
    [SerializeField] private GameObject anahtarParcasiSablonu;              // kırık anahtar parçası (#65)
    [SerializeField] private GameObject tabakSablonu;                       // sofraya konacak tabak (#66)
    [SerializeField] private GameObject asmaKilitSablonu, mentesePlakasiSablonu, camKirikSablonu, notSablonu;
    private ZincirUygulayici zincirUygulayici;

    // Her katın kaplama malzemeleri: [(kat - 1) * 4 + yüzey]. DokuKurucu üretir ve buraya bağlar.
    // Yüzey sırası KatKaplamalari.Yuzey ile aynı: oda duvarı, parke, ıslak zemin, ıslak duvar.
    [SerializeField] private Material[] kaplamalar;

    // Bir dairenin kaplanan parçaları: hangi çizicinin (MeshRenderer) hangi malzeme yuvası
    // hangi yüzey. Başta bir kez, editördeki malzeme adlarından çıkarılır; sonra her katta
    // yalnızca bu yuvalara o katın malzemesi takılır.
    private class KaplamaYeri
    {
        public MeshRenderer cizici;
        public int[] yuzeyler;   // her malzeme yuvası için yüzey numarası; -1 = dokunma (tavan)
    }
    private readonly System.Collections.Generic.List<KaplamaYeri>[] kaplamaYerleri =
        new System.Collections.Generic.List<KaplamaYeri>[MekanOlculeri.DaireSayisi + 1];
    private const float GirisAcikAci = 80f;   // açık dairenin sahanlık kapısı: içeri doğru 80°

    // ---- KAT OLAYLARI (#35) ----
    // Her katta bir iki kez çalan tek bir olay sesi (bebek, uzak kapı, üst kat adımları,
    // kalorifer, araba). Plan hangisinin ve ne zaman çalacağını bilir; çalıcı yalnızca
    // alt sahanlıkta (A) çalar, çünkü oyuncu hep A'nın gösterdiği kattadır.
    private KatOlayPlani olayPlani;
    private KatOlayCalici olayCaliciA;

    // ---- KATA ÖZGÜ AN (#39) ----
    // TV ve boru sesleri her katta kaydın farklı bir anını çalar. Her sahanlığın bu iki
    // kaynağı; sahanlık yeni bir katı gösterince o katın kaydırmasını alırlar.
    private OrtamSesi[] aKatSesleri, bKatSesleri;

    // Sahanlıkların şu an hangi katı gösterdiği. -1 = henüz hiçbiri.
    private int gosterilenA = -1, gosterilenB = -1;

    // Yeni ilerlemede (eski kural kapalı) oyuncunun bulunduğu kattan kaç kat AŞAĞI indiği.
    // Aşağı inmek ceza da ilerleme de değildir; oyuncu bir şey almak için inebilir. Tek
    // sahanlık döngüsünde aşağıdaki katlar da bu katın görüntüsünü gösterir (aynı kat gibi);
    // oyuncu yeniden çıkınca sayaç azalır ve kata geri döner. Böylece inip çıkan oyuncu kat
    // kapısını açmadan bir üst kata geçemez. Gerçek alt katlar #59'un işi.
    private int asagiInilen;
    private bool catiModu;

    // ---- AÇILIŞ ----
    private GameObject zeminKat;       // kapıcı dairesi olan zemin kat (başta kapalı)
    private GameObject sahteAlt1;      // 1. katın altındaki sahte kat; açılışta zemin kata yer bırakır
    private bool acilisModu;

    // ---------- KURULUM ----------

    private void Awake()
    {
        if (merdiven == null) merdiven = FindAnyObjectByType<SonsuzMerdiven>();
        if (oyuncu == null && merdiven != null) oyuncu = merdiven.transform;

        if (oyuncu != null)
        {
            baslangicKonumu = oyuncu.position;
            baslangicDonusu = oyuncu.rotation;
            govde = oyuncu.GetComponent<CharacterController>();
        }

        // Sahnedeki lamba ışıklarını bir kez topla; her karede aramak pahalı olurdu.
        // Spot ve nokta ışıklar lambalara ait; yönlü ışık (ay) dahil değil.
        var liste = new System.Collections.Generic.List<Light>();
        var anaIsiklar = new System.Collections.Generic.List<Light>();
        foreach (var l in FindObjectsByType<Light>())
        {
            // Kafa lambası (#51) oyuncunun; ceza onu söndürmez, kartlar hedeflemez.
            if (l.GetComponentInParent<KafaLambasi>() != null) continue;
            if (l.type == LightType.Point || l.type == LightType.Spot) liste.Add(l);
            if (l.type == LightType.Spot) anaIsiklar.Add(l);
        }
        ampuller = liste.ToArray();
        lambalar = anaIsiklar.ToArray();

        ozgunRenkler = new Color[ampuller.Length];
        for (int i = 0; i < ampuller.Length; i++) ozgunRenkler[i] = ampuller[i].color;

        // ---- SAHNE PARÇALARI ----
        sahanlikA = KokBul("Sahanlik_A")?.transform;
        sahanlikB = KokBul("Sahanlik_B")?.transform;
        catiBolumu = KokBul("CatiBolumu");

        GameObject sahte = KokBul("Sahte_Katlar");
        Transform ust = sahte != null ? sahte.transform.Find("Ust") : null;
        ustKatlar = ust != null ? ust.gameObject : null;

        zeminKat = KokBul("Zemin_Kat");
        Transform alt1 = sahte != null ? sahte.transform.Find("Alt/Sahanlik_Alt1") : null;
        sahteAlt1 = alt1 != null ? alt1.gameObject : null;

        aNumaralar = NumaralariBul(sahanlikA);
        bNumaralar = NumaralariBul(sahanlikB);
        aPosetler = PosetleriBul(sahanlikA);
        bPosetler = PosetleriBul(sahanlikB);
        olayCaliciA = new KatOlayCalici(sahanlikA);
        KapilariBul();
        DaireleriBul();
        zincirUygulayici = new ZincirUygulayici();
        zincirUygulayici.SablonEkle(EsyaTuru.OdaAnahtari, odaAnahtariSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.KatAnahtari, katAnahtariSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Sigorta, sigortaSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Tornavida, tornavidaSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Parca, cizimParcasiSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Levye, levyeSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Kaydirici, kaydiriciSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.AnahtarParcasi, anahtarParcasiSablonu);
        zincirUygulayici.SablonEkle(EsyaTuru.Tabak, tabakSablonu);
        zincirUygulayici.AsmaKilitSablonu = asmaKilitSablonu;
        zincirUygulayici.MentesePlakasiSablonu = mentesePlakasiSablonu;
        zincirUygulayici.CamKirikSablonu = camKirikSablonu;
        zincirUygulayici.NotSablonu = notSablonu;
        zincirUygulayici.YaziOrnegi = aNumaralar != null && aNumaralar.Length > 0 ? aNumaralar[0] : null;
        AyakkabilariBul(sahanlikA, out aRaflar, out aDaginik);
        AyakkabilariBul(sahanlikB, out bRaflar, out bDaginik);
        aKatSesleri = KatSesleriniBul(sahanlikA);
        bKatSesleri = KatSesleriniBul(sahanlikB);

        toplayici = FindAnyObjectByType<CopToplayici>();

        // Resources.LoadAll: "Resources" klasörlerindeki bütün kartları yükler.
        // Yeni kart eklemek için hiçbir listeye kaydetmek gerekmiyor.
        kartlar = Resources.LoadAll<AnomaliKarti>("Anomaliler");

        lambaSonme = Resources.Load<AudioClip>("Sesler/Lamba_Sonme");
        lambaCizirti = Resources.Load<AudioClip>("Sesler/Lamba_Cizirti");

        if (kartlar.Length == 0)
        {
            Debug.LogWarning("[ANOMALİ] Assets/Resources/Anomaliler klasöründe hiç kart yok. " +
                             "Bütün katlar normal geçecek.");
        }
    }

    // Sahnenin kök nesneleri arasında isimle arar. GameObject.Find'dan farkı,
    // KAPALI nesneleri de bulması: çatı bölümü oyun başında kapalı duruyor.
    private GameObject KokBul(string ad)
    {
        foreach (GameObject g in gameObject.scene.GetRootGameObjects())
        {
            if (g.name == ad) return g;
        }
        return null;
    }

    // Kata göre kaydırılan ortam sesleri: TV ve boru (#39). Uğultu ve rüzgâr dokudur,
    // hangi anda oldukları duyulmaz; onlar bütün katlarda aynı kalır.
    private OrtamSesi[] KatSesleriniBul(Transform sahanlik)
    {
        var liste = new System.Collections.Generic.List<OrtamSesi>();
        if (sahanlik == null) return liste.ToArray();
        foreach (string yol in new[] { "Kapi_2/Ses_TV", "Ses_Boru" })
        {
            Transform t = sahanlik.Find(yol);
            OrtamSesi o = t != null ? t.GetComponent<OrtamSesi>() : null;
            if (o != null) liste.Add(o);
        }
        return liste.ToArray();
    }

    private static void KatSesleriniKaydir(OrtamSesi[] sesler, int kat)
    {
        foreach (OrtamSesi o in sesler)
        {
            AudioSource k = o.GetComponent<AudioSource>();
            if (k != null && k.clip != null) o.Kaydirma = KatSesKaydirmasi.Saniye(kat, k.clip.length);
        }
    }

    private TextMesh[] NumaralariBul(Transform sahanlik)
    {
        var sonuc = new TextMesh[MekanOlculeri.DaireSayisi];
        if (sahanlik == null) return sonuc;

        for (int d = 0; d < sonuc.Length; d++)
        {
            Transform t = sahanlik.Find($"Kapi_{d + 1}/Mentese/Numara");
            if (t != null) sonuc[d] = t.GetComponent<TextMesh>();
        }
        return sonuc;
    }

    private CopPoseti[,] PosetleriBul(Transform sahanlik)
    {
        var sonuc = new CopPoseti[MekanOlculeri.DaireSayisi, CopPlani.EnFazlaPoset];
        if (sahanlik == null) return sonuc;

        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
        {
            for (int s = 0; s < CopPlani.EnFazlaPoset; s++)
            {
                Transform t = sahanlik.Find($"Kapi_{d + 1}/Copler/Poset_{s + 1}");
                if (t != null) sonuc[d, s] = t.GetComponent<CopPoseti>();
            }
        }
        return sonuc;
    }

    private void Start()
    {
        // Çözülen her kilit kaydedilsin (#54). Kilit.Cozuldu "static" bir olay: bütün kilitler
        // aynı olayı tetikler, burada bir kez abone oluyoruz (OnDestroy'da bırakıyoruz).
        Kilit.Cozuldu += Kayit.KilitCozuldu;
        // Tohum 0 ise her oyun farklı olsun diye saatin o anki değerini kullanıyoruz.
        int gercekTohum = tohum != 0 ? tohum : System.Environment.TickCount;

        // Test kartı verildiyse seçici yalnızca onu tanır ve her katta onu seçer
        // (olasılık 1). 1. ve 8. kat kuralları yine geçerli.
        if (testKarti != null)
        {
            Debug.LogWarning($"[ANOMALİ] TEST KARTI AÇIK: her kat '{testKarti.Kimlik}' ile gelecek. " +
                             "Normal oyun için KatYoneticisi → Test Kartı alanını boşaltın.");
            secici = new AnomaliSecici(new IAnomaliKart[] { testKarti }, gercekTohum, 1f, ilkKatHepNormal);
        }
        else
        {
            // Eski kural kapalıyken (yeni ilerleme) olasılık 0: hiçbir kat anomalili gelmez.
            // Kartlar ileride senaryolu olay kütüphanesi olacak (#60); rastgele seçim yok.
            secici = new AnomaliSecici(kartlar, gercekTohum, eskiKural ? anomaliOlasiligi : 0f, ilkKatHepNormal);
        }

        // Çöp planı AYRI bir tohumla kuruluyor. Aynı tohumu verseydik iki üretici
        // aynı sayı dizisini üretir ve poşet sayıları anomalilerle gizlice
        // bağlantılı olurdu (örneğin "3 poşetli kat hep anomalili" gibi).
        copPlani = new CopPlani(MekanOlculeri.DaireSayisi, gercekTohum + 7919);

        // Ayakkabılar da kendi tohumuyla.
        ayakkabiPlani = new AyakkabiPlani(MekanOlculeri.DaireSayisi, gercekTohum + 15485863);

        // Kat olayları da kendi tohumuyla (aynı sebep: planlar birbirine gizlice bağlanmasın).
        olayPlani = new KatOlayPlani(gercekTohum + 104729);

        // Kayıt varsa (#52) oyun kaldığı kattan sürer: açılış atlanır, oyuncu sahnedeki yerinde
        // (1. kat sahanlığının başlangıcı) o katın sahanlığında uyanır.
        if (KayitYuvasi.Ek != "")
            Debug.LogWarning($"[KAYIT] TEST YUVASI AÇIK ('{KayitYuvasi.Ek}'): oyun gerçek kayda değil test kaydına yazıyor. " +
                             $"Normal oyun için EditorPrefs '{KayitYuvasi.EditorAnahtari}' silinmeli.");
        bool kayittanDevam = Kayit.Var && testKarti == null;
        if (kayittanDevam)
        {
            sayac.Yukle(Kayit.Kat);
            Debug.Log($"[KAYIT] Kaldığı yerden: {sayac.MevcutKat}. kat");
        }

        referansDongu = merdiven != null ? merdiven.NetDongu : 0;
        SahanliklariGuncelle(zorla: true);

        if (acilisIleBasla && zeminKat != null && !kayittanDevam) AcilisiBaslat();
    }

    // ---------- AÇILIŞ (#16) ----------
    // 8. kattaki çatı modunun aynası: çatı B'nin yerini alıyordu, zemin kat da
    // 1. katın altındaki sahte katın yerini alıyor. Oyuncu kapıcı odasında uyanır,
    // merdivenden 1. kata çıkar; 1. kata yerleştiği anda zemin kat eski sahte kata
    // döner ve döngü normal çalışır. O anda oyuncu 1. katın sahanlığında, zemin kat
    // ise altında ve görüş alanı dışında: değişim görünmez.
    private void AcilisiBaslat()
    {
        acilisModu = true;
        zeminKat.SetActive(true);
        if (sahteAlt1 != null) sahteAlt1.SetActive(false);

        // Zemin kattan aşağı ışınlanma olmasın: zemin kat binanın dibi.
        merdiven.AsagiDonguyuAyarla(false);

        // Oyuncuyu kapıcı odasına al, yüzü kapıya dönük (+x). Cezada dönülecek yer
        // (baslangicKonumu) Awake'te kaydedildi ve 1. kat olarak kalıyor.
        if (govde != null) govde.enabled = false;
        oyuncu.position = new Vector3(MekanOlculeri.OdaBaslangicX, MekanOlculeri.ZeminKot + 0.2f,
                                      MekanOlculeri.OdaBaslangicZ);
        oyuncu.rotation = Quaternion.Euler(0f, 90f, 0f);
        if (govde != null) govde.enabled = true;
        Physics.SyncTransforms();
    }

    private void AcilisiBitir()
    {
        acilisModu = false;
        zeminKat.SetActive(false);
        if (sahteAlt1 != null) sahteAlt1.SetActive(true);
        merdiven.AsagiDonguyuAyarla(true);
        Debug.Log("[KAT] Açılış bitti: 1. kata varıldı, döngü başladı.");
    }

    // ---------- SANAL KOT ----------
    // Oyuncunun KURGUDAKİ yüksekliği: gerçek yüksekliği artı, şimdiye kadar
    // yapılan ışınlanmaların telafisi. Işınlanma bu sayıyı hiç etkilemez;
    // karar mantığı işte buna bakar, çünkü ışınlanma bir görüntü hilesidir.
    private float SanalKot()
    {
        if (oyuncu == null) return 0f;

        int netDongu = merdiven != null ? merdiven.NetDongu : 0;
        return oyuncu.position.y + netDongu * MekanOlculeri.DikeyOfset;
    }

    // ---------- HER KARE ----------

    private void LateUpdate()
    {
        // FPS ölçümü (test göstergesi için, #26). Tek karenin süresi çok oynak olduğu
        // için yumuşatılmış bir ortalama tutuyoruz: her karede farkın %5'i kadar yaklaş.
        // unscaledDeltaTime: Esc menüsü oyun saatini durdursa da gerçek kare süresi.
        ortalamaKareSuresi += (Time.unscaledDeltaTime - ortalamaKareSuresi) * 0.05f;

        if (merdiven == null || oyuncu == null) return;

        // Ceza gösterisi oynarken ve tur bittikten sonra karar dinlemeyiz.
        if (cezaCalisiyor || sayac.TurBitti) return;

        // Takipçiyi oyuncu yere OTURDUĞU anda kur.
        //
        // "Yerde mi?" (isGrounded) tek başına yetmiyor. Bu bilgi SON hareketten
        // kalmadır: ceza bitince oyuncuyu başlangıç noktasına 20 cm havada
        // koyuyoruz ve isGrounded bir kare boyunca hâlâ "evet" diyor. Takipçi o
        // anda kurulursa ölçüme 18 cm yukarıdan başlar ve bütün kararlar bir
        // basamak geç gelir: ışınlanma olur ama karar o karede verilmez.
        // Bu hata testte tam olarak böyle göründü. Çözüm: oyuncu yerde OLMALI ve
        // iki kare arasında yüksekliği artık değişmiyor olmalı; yani düşüşü bitmiş.
        if (ilerleme == null)
        {
            float y = oyuncu.position.y;
            bool oturdu = govde != null && govde.isGrounded && Mathf.Abs(y - oncekiY) < 0.0005f;
            oncekiY = y;

            if (!oturdu) return;

            // Açılışta oyuncu zemin kattayken karar sayılmaz: ölçüm 1. katta başlar.
            // Zemin kattan 1. kata çıkmak bir "kat kararı" değil, işe başlamaktır.
            if (acilisModu)
            {
                if (y < -0.5f) return;
                AcilisiBitir();
            }

            ilerleme = new KatIlerlemeTakipcisi(MekanOlculeri.DikeyOfset, SanalKot());
            referansDongu = merdiven.NetDongu;
        }

        float kot = SanalKot();

        // Ceza bekliyorsa, oyuncunun yarım kat yol almasını bekle. Yön bakmıyoruz.
        if (cezaBekliyor)
        {
            if (ilerleme.SonKarardanBeriMesafe(kot) >= MekanOlculeri.DikeyOfset / 2f)
            {
                cezaBekliyor = false;
                StartCoroutine(CezaGosterisi());
            }
            return;
        }

        // Oyuncu bir tam kat yüksekliği kadar yol aldıysa karar oluşur.
        if (ilerleme.KararOlustuMu(kot, out Karar karar))
        {
            referansDongu = merdiven.NetDongu;   // yeni karar noktası burası
            KarariIsle(karar);
        }

        // Ceza bekliyorsa sahanlıklara dokunmuyoruz: ışıklar zaten sönmek üzere.
        if (!cezaBekliyor && !sayac.TurBitti) SahanliklariGuncelle(zorla: false);

        // Senaryolu anomali anları (#13): kartın zamanlı etkileri yalnızca oyuncunun
        // BULUNDUĞU sahanlıkta işler. A mevcut katı gösteriyorsa oyuncu oradadır;
        // aşağı inerken A bir sonraki katı gösterir ve saat durur. Üst sahanlığın (B)
        // saati hiç işlemez: merdivenden bakarken bir şey kendiliğinden olmamalı.
        // Olaylı bir anomali anı gerçekleştiyse (kapı çarptı, zil çaldı, ışık göz kırptı)
        // arka müzik kısa süre kabarır (#40). Sessiz anomalilerde zamanlı etki olmadığı için
        // müzik değişmez; yani müzik cevabı vermez, yalnızca olmuş bir korkuyu vurgular.
        if (gosterilenA == sayac.MevcutKat && uygulayiciA.Ilerlet(Time.deltaTime) > 0)
            ArkaMuzik.Ornek?.Kabar();

        // Kat olayının saati de aynı kuralla işler (#35). Ceza yoldayken işlemez:
        // lambaların sönme sesine başka bir ses karışmasın.
        if (gosterilenA == sayac.MevcutKat && !cezaBekliyor) olayCaliciA.Ilerlet(Time.deltaTime);
    }

    // ---------- KARARIN SONUCU ----------

    private void KarariIsle(Karar karar)
    {
        // YENİ İLERLEME (#50): ceza yok. Aşağı inmek sayılır ama katı değiştirmez; aşağıdan
        // geri çıkmak da yalnızca sayacı azaltır. Bir üst kata ancak bulunduğu kattan, kat
        // kapısını açıp çıkarak geçilir (kapı kilitli olduğu için başka yol yok).
        if (!eskiKural)
        {
            if (karar == Karar.Geri) { asagiInilen++; Debug.Log($"[KAT] Aşağı inildi ({asagiInilen}); kat {sayac.MevcutKat} kalıyor."); return; }
            if (asagiInilen > 0) { asagiInilen--; Debug.Log($"[KAT] Geri çıkıldı ({asagiInilen}); kat {sayac.MevcutKat}."); return; }
            // Test kartı açıksa katta anomali olabilir; yeni ilerlemede anomali bir olaydır,
            // yukarı çıkmak yanlış karar değil. Sayaç "yanlış" demesin diye bayrak sıfırlanır.
            if (sayac.AnomaliVar) sayac.KatiHazirla(false);
        }

        // Katta anomali varsa görev aranmaz: anomaliyi görüp geri dönmek zaten
        // doğru davranıştır.
        // Toplanmamış çöp cezası yalnızca eski kuralda (yeni ilerlemede sahanlık işleri #59'da yeniden bağlanacak).
        bool gorevEksik = eskiKural && !sayac.AnomaliVar && ToplanmamisGorevVar(sayac.MevcutKat);

        int eskiKat = sayac.MevcutKat;
        // Anomali defteri (#45) için: karardan ÖNCE bu katın anomalisini hatırla; KararVer
        // mevcut katı ilerletiyor.
        bool anomaliliKat = sayac.AnomaliVar;
        IAnomaliKart katinKarti = KartSec(eskiKat);
        KararSonucu sonuc = sayac.KararVer(karar, gorevEksik);

        // Anomalili katta doğru biçimde geri döndü: anomaliyi fark ettiğini kanıtladı,
        // deftere işlenir (Şahan'ın kuralı). Test kartıyla oynarken işlenmez.
        if (sonuc == KararSonucu.Ilerle && karar == Karar.Geri && anomaliliKat && katinKarti != null && testKarti == null)
            Defter.Isle(katinKarti.Kimlik);

        switch (sonuc)
        {
            case KararSonucu.Ilerle:
                // Sahanlık burada yenilenmiyor; hemen ardından SahanliklariGuncelle
                // karar noktasının değiştiğini görüp yeniliyor.
                Debug.Log($"[KAT] Doğru karar ({karar}). {eskiKat} -> {sayac.MevcutKat}");
                // Otomatik kayıt (#52): yeni bir kata ulaşıldı.
                if (sayac.MevcutKat > Kayit.Kat) Kayit.Kat = sayac.MevcutKat;
                break;

            case KararSonucu.Sifirla:
                // Ceza ANINDA gelmez. Oyuncu yürümeye devam eder; korku o beklemede birikir.
                string sebep = gorevEksik ? "çöp alınmadı" : $"yanlış karar ({karar})";
                Debug.Log($"[KAT] CEZA: {sebep}. Toplam hata: {sayac.ToplamHata}");
                cezaBekliyor = true;
                break;

            case KararSonucu.CatiyaUlasildi:
                Debug.Log("[KAT] ÇATIYA ULAŞILDI. Döngü kapatıldı, çatı kapısı açıldı.");
                merdiven.DonguyuAyarla(false);
                CatiModunuAyarla(true);
                CatiKapisiniAc();
                break;
        }
    }

    // ---------- SAHANLIKLARIN GÖSTERDİĞİ KAT ----------
    // Sahnede iki gerçek sahanlık var ve her biri bir KATI temsil ediyor.
    // Hangi katı temsil ettiği, karar noktasına göre nerede durduğuna bağlı:
    //
    //   Karar noktasındaki sahanlık       -> mevcut kat (N).
    //   Bir üstündeki VEYA bir altındaki  -> bir sonraki kat (N+1).
    //
    // Neden ikisi de N+1? Çünkü bu oyunda doğru kararla yukarı çıkmak da aşağı
    // inmek de İLERLEMEK sayılıyor. Oyuncu hangi yöne giderse gitsin, vardığı
    // sahanlık bir sonraki kattır.
    //
    // Bunu her karede hesaplamamızın sebebi şu: oyuncu aşağı inip ışınlanınca
    // A sahanlığı artık "altındaki kat" olur ve N+1'i göstermelidir. Oyuncu o anda
    // merdivenin içinde, sahanlığa sırtı dönük; değişim görünmez. Vazgeçip geri
    // çıkarsa A yine N'yi gösterir. Böylece hiçbir ışınlanma ekranda bir
    // sahanlığın aniden değişmesine yol açmaz.
    private int GosterilecekKat(int karardanUzaklik)
    {
        // Yeni ilerleme: karar noktasının altındaki sahanlık (aşağı inen oyuncunun vardığı) ve
        // oyuncu aşağıdayken üstündeki sahanlık da bu katı gösterir; yalnızca bulunduğu katın
        // üstü bir sonraki kattır. Böylece inerken ya da geri çıkarken hiçbir sahanlık gözün
        // önünde değişmez.
        if (!eskiKural && (karardanUzaklik < 0 || asagiInilen > 0)) return sayac.MevcutKat;
        return karardanUzaklik == 0 ? sayac.MevcutKat : sayac.MevcutKat + 1;
    }

    private void SahanliklariGuncelle(bool zorla)
    {
        int netDongu = merdiven != null ? merdiven.NetDongu : 0;

        // A'nın karar noktasına göre kaç kat yukarıda durduğu (0 veya -1).
        int aUzaklik = netDongu - referansDongu;

        int aKat = GosterilecekKat(aUzaklik);
        int bKat = GosterilecekKat(aUzaklik + 1);

        if (!zorla && aKat == gosterilenA && bKat == gosterilenB) return;

        SahanliklariKur(aKat, bKat);
    }

    private void SahanliklariKur(int aKat, int bKat)
    {
        // SIRA ÖNEMLİ:
        // 1) Sahanlıktan ayrılan katın toplanan poşetleri kaydedilir.
        // 2) Eski anomaliler geri alınır. Sahanlık tertemiz olur.
        // 3) Poşetler ve kapı numaraları yeni kata göre yazılır.
        // 4) En son kart uygulanır.
        // Kart numaralardan önce uygulansaydı, numara yazılırken kartın
        // değiştirdiği numara ezilirdi. Geri alma numaralardan sonra yapılsaydı
        // uygulayıcı eski katın numarasını geri yazardı.
        DurumuKaydet(aPosetler, gosterilenA);
        DurumuKaydet(bPosetler, gosterilenB);
        AyakkabiDurumunuKaydet(aDaginik, gosterilenA);
        AyakkabiDurumunuKaydet(bDaginik, gosterilenB);

        uygulayiciA.GeriAl();
        uygulayiciB.GeriAl();

        // Üst sahanlık çatı katını gösteriyorsa (8. kattayız), o bir sahanlık değil
        // çatı kapısıdır.
        bool cati = bKat > KatSayaci.CatiKati;
        CatiModunuAyarla(cati);

        PosetleriYerlestir(aPosetler, aKat);
        if (!cati) PosetleriYerlestir(bPosetler, bKat);
        AyakkabilariYerlestir(aRaflar, aDaginik, aKat);
        if (!cati) AyakkabilariYerlestir(bRaflar, bDaginik, bKat);

        NumaralariYaz(aNumaralar, aKat);
        NumaralariYaz(bNumaralar, bKat);

        // Açık daire (#53): kart uygulanmadan ÖNCE. Kart bir kapıyı döndürüyorsa (aralık kapı
        // gibi) önce bizim verdiğimiz açıyı kaydeder, geri alırken ona döner; sıra ters olsaydı
        // kartın açtığı kapıyı biz ezerdik.
        DaireleriAyarla(aKat, cati ? 0 : bKat);

        IAnomaliKart aKart = KartSec(aKat);
        IAnomaliKart bKart = cati ? null : KartSec(bKat);

        if (sahanlikA != null) uygulayiciA.Uygula(aKart as AnomaliKarti, sahanlikA);
        if (sahanlikB != null && !cati) uygulayiciB.Uygula(bKart as AnomaliKarti, sahanlikB);

        // Kat kapıları ve anahtarlar (#52).
        KapilariAyarla(aKat, bKat, cati);

        // A'nın kat olayı (#35). Kartın kendi sesi varsa (kapı çarpması, zil, tersten
        // ağlama...) o katta olay çalmaz: Şahan'ın kuralı "her katta bunlardan biri",
        // o katın sesi zaten anomalinin sesi. Olay ile anomali sesi üst üste binmez.
        bool olayVar = aKat >= KatSayaci.BaslangicKati && aKat <= KatSayaci.CatiKati && !SesliKart(aKart);
        olayCaliciA.Hazirla(olayVar, olayPlani.KatIcin(aKat), olayPlani.ZamanlarKatIcin(aKat));

        // TV ve boru o katın kendi anını çalsın (#39).
        KatSesleriniKaydir(aKatSesleri, aKat);
        KatSesleriniKaydir(bKatSesleri, bKat);

        gosterilenA = aKat;
        gosterilenB = bKat;

        // Karar kuralı, sahanlığın ne gösterdiğine değil MEVCUT KATA bakar.
        sayac.KatiHazirla(KartSec(sayac.MevcutKat) != null);

        string durum = aKart != null ? $"VAR ({aKart.Kimlik})" : "yok";
        Debug.Log($"[KAT] A = {aKat}. kat (anomali: {durum}), B = {(cati ? "çatı" : bKat + ". kat")}");
    }

    // ---------- BAŞKA BİR SES ÇALIYOR MU? (#37) ----------
    // Dış sesler (DisSesleri) çalmadan önce bunu sorar: kat olayı, anomali sesi ya da
    // ceza sürüyorsa araya girmez. Sesli bir kartın geldiği katta ilk 10 saniye de meşgul
    // sayılır: kartın zamanlı sesleri (zil, kapı tıklatma...) 2,5-8 sn'de çalıyor; dış ses
    // tam o sırada başlarsa üst üste binerdi.
    public bool SesMesgul
    {
        get
        {
            if (cezaBekliyor || cezaCalisiyor) return true;
            if (olayCaliciA != null && olayCaliciA.CaliyorMu) return true;
            if (gosterilenA == sayac.MevcutKat && SesliKart(KartSec(sayac.MevcutKat)) &&
                olayCaliciA != null && olayCaliciA.Saat < 10f) return true;
            if (sahanlikA != null)
                foreach (AudioSource k in sahanlikA.GetComponentsInChildren<AudioSource>())
                    if (k.name == "Anomali_Sesi" && k.isPlaying) return true;
            return false;
        }
    }

    /// <summary>Tur bitti mi (çatıya ulaşıldı)? Dış sesler ve müzik buna bakar.</summary>
    public bool TurBitti { get { return sayac != null && sayac.TurBitti; } }

    // ---------- KAT KAPISI VE ANAHTAR (#52) ----------
    private void KapilariBul()
    {
        // A'nın çekirdeği sahnenin kökünde; B'ninki ise sahte üst katların altında
        // (Sahte_Katlar/Ust/Cekirdek_B). Kökte aranırsa bulunamaz ve B'nin kapısı hiç
        // yönetilmez: merdiven gözünden yukarı bakınca yanlış hâlde görünür.
        GameObject cA = KokBul("Cekirdek_A"), sahte = KokBul("Sahte_Katlar");
        Transform kA = cA != null ? cA.transform.Find("Kat_Kapisi") : null;
        Transform kB = sahte != null ? sahte.transform.Find("Ust/Cekirdek_B/Kat_Kapisi") : null;
        kapiA = kA != null ? kA.GetComponent<KatKapisi>() : null;
        kapiB = kB != null ? kB.GetComponent<KatKapisi>() : null;
        if (kapiA != null) kapiA.Acildi += KapiAcildi;
        if (kapiB != null) kapiB.Acildi += KapiAcildi;
        Transform aA = sahanlikA != null ? sahanlikA.Find("Kat_Anahtari") : null;
        Transform aB = sahanlikB != null ? sahanlikB.Find("Kat_Anahtari") : null;
        anahtarA = aA != null ? aA.GetComponent<KatAnahtari>() : null;
        anahtarB = aB != null ? aB.GetComponent<KatAnahtari>() : null;
        if (kapiA == null) Debug.LogWarning("[KAPI] Cekirdek_A/Kat_Kapisi bulunamadı; kat kapısı yok.");
        if (kapiB == null) Debug.LogWarning("[KAPI] Sahte_Katlar/Ust/Cekirdek_B/Kat_Kapisi bulunamadı.");
    }

    // Bir katın kapısı açık mı: bu turda açıldıysa ya da kat zaten geride kaldıysa (kayıttan
    // yüklemede alt katların kapıları açılmış sayılır). 8. katta kapı yok gibi: yukarısı çatı,
    // çatı kapısı ayrı kilit (KatYoneticisi.CatiKapisiniAc).
    private bool KapiAcikMi(int kat)
    {
        return kat >= KatSayaci.CatiKati || kat < sayac.MevcutKat || acilanKatlar.Contains(kat);
    }

    private void KapilariAyarla(int aKat, int bKat, bool cati)
    {
        if (kapiA != null) kapiA.Ayarla(aKat, KapiAcikMi(aKat));
        if (kapiB != null && !cati) kapiB.Ayarla(bKat, KapiAcikMi(bKat));
        // Anahtar: kapısı henüz açılmamış ve anahtarı cepte olmayan katlarda görünür.
        // Zinciri olan katta katın anahtarı dairenin içindedir (zincirin sonunda); sahanlıktaki gizlenir.
        if (anahtarA != null) anahtarA.Ayarla(aKat, ZincirBul(aKat) == null && !KapiAcikMi(aKat) && !Cep.Tasiyor(EsyaTuru.KatAnahtari, aKat));
        if (anahtarB != null) anahtarB.Ayarla(bKat, !cati && ZincirBul(bKat) == null && !KapiAcikMi(bKat) && !Cep.Tasiyor(EsyaTuru.KatAnahtari, bKat));
    }

    private void KapiAcildi(int kat)
    {
        acilanKatlar.Add(kat);
        // Aynı katı gösteren öbür kapı da açılsın (B sahanlığı aynı katı gösteriyorsa).
        if (kapiA != null && kapiA.Kat == kat && !kapiA.Acik) kapiA.Ayarla(kat, true);
        if (kapiB != null && kapiB.Kat == kat && !kapiB.Acik) kapiB.Ayarla(kat, true);
    }

    // ---------- DAİRELER (#53) ----------
    private void DaireleriBul()
    {
        for (int n = 1; n <= MekanOlculeri.DaireSayisi; n++)
        {
            Transform d = sahanlikA != null ? sahanlikA.Find($"Daire_{n}") : null;
            daireler[n] = d != null ? d.gameObject : null;
            kaplamaYerleri[n] = d != null ? KaplamaYerleriniBul(d) : null;
        }
        if (daireler[1] == null) Debug.LogWarning("[DAIRE] Sahanlik_A/Daire_1 bulunamadı; graybox yeniden kurulmalı.");
    }

    // A'da o katın dairesi açılır, öbür üçü kapanır. B'de yalnızca sahanlık kapısı açılır:
    // oyuncu B'ye hiç girmiyor, açık kapının ardında karanlık hol (Daire_Ici) durur. B'ye
    // varınca oyuncu A'ya ışınlanır ve A aynı daireyi gerçek hâliyle gösterir.
    private void DaireleriAyarla(int aKat, int bKat)
    {
        int aAcik = DairePlani.AcikDaire(aKat);
        int bAcik = DairePlani.AcikDaire(bKat);
        // Çift daire (#70): kat dosyası ikinci bir açık daire tanımlayabilir.
        KatZinciriVarligi varlik = VarlikBul(aKat);
        int ikinci = varlik != null && varlik.ikinciDaire >= 1 && varlik.ikinciDaire <= MekanOlculeri.DaireSayisi
                     && varlik.ikinciDaire != aAcik && aAcik > 0 ? varlik.ikinciDaire : 0;
        for (int n = 1; n <= MekanOlculeri.DaireSayisi; n++)
        {
            bool acik = n == aAcik || n == ikinci;
            if (daireler[n] != null)
            {
                daireler[n].SetActive(acik);
                // Yeni katın dairesi baştan: bütün oda kapıları kapalı. (Hangi odanın kilitli
                // olacağını #55'in kat içerikleri söyleyecek; balkon kapısı zaten hep kilitli.)
                if (acik)
                {
                    foreach (IcKapi k in daireler[n].GetComponentsInChildren<IcKapi>(true))
                        k.Ayarla(false);
                    // Eşyaların çekmece ve kapakları da kapalı başlar.
                    foreach (AcilirParca p in daireler[n].GetComponentsInChildren<AcilirParca>(true))
                        p.Ayarla(false);
                    // İkinci daire başka bir katın kaplamasını giyer: iki daire birbirine benzemesin.
                    KaplamayiGiydir(n, n == ikinci && varlik.ikinciKaplamaKati > 0 ? varlik.ikinciKaplamaKati : aKat);
                }
            }
            SahanlikKapisiniAyarla(sahanlikA, n, acik, gercekDaire: true);
            SahanlikKapisiniAyarla(sahanlikB, n, n == bAcik, gercekDaire: false);
        }

        // O katın kilit zinciri (yoksa daire kilitsiz): iki daire de açıldıktan SONRA kurulur,
        // çünkü zincir iki daireye yayılabilir (#70).
        if (aAcik > 0 && daireler[aAcik] != null && zincirUygulayici != null)
        {
            zincirUygulayici.IkinciDaire = ikinci > 0 && daireler[ikinci] != null ? daireler[ikinci].transform : null;
            zincirUygulayici.Uygula(daireler[aAcik].transform, ZincirBul(aKat), aKat);
            // Katın notları ve dekoru (koliler...), #55.
            zincirUygulayici.DekorUygula(daireler[aAcik].transform, varlik, aKat);
        }
    }

    /// <summary>Bu katın içerik dosyası; tanımlı değilse null.</summary>
    private KatZinciriVarligi VarlikBul(int kat)
    {
        if (katZincirleri == null || kat < 1 || kat > katZincirleri.Length) return null;
        return katZincirleri[kat - 1];
    }

    /// <summary>Bu katın kilit zinciri; tanımlı değilse null.</summary>
    private KilitZinciri ZincirBul(int kat)
    {
        KatZinciriVarligi v = VarlikBul(kat);
        return v != null ? v.zincir : null;
    }

    // Editördeki malzeme adı -> yüzey numarası. DokuKurucu'nun dairelere verdiği adlar.
    private static int YuzeyNumarasi(Material m)
    {
        if (m == null) return -1;
        switch (m.name)
        {
            case "Doku_DaireDuvar": return (int)KatKaplamalari.Yuzey.OdaDuvari;
            case "Doku_Parke": return (int)KatKaplamalari.Yuzey.Parke;
            case "Doku_Fayans": return (int)KatKaplamalari.Yuzey.IslakZemin;
            case "Doku_FayansDuvar": return (int)KatKaplamalari.Yuzey.IslakDuvar;
            default: return -1;
        }
    }

    private static System.Collections.Generic.List<KaplamaYeri> KaplamaYerleriniBul(Transform daire)
    {
        var sonuc = new System.Collections.Generic.List<KaplamaYeri>();
        foreach (MeshRenderer r in daire.GetComponentsInChildren<MeshRenderer>(true))
        {
            Material[] mats = r.sharedMaterials;
            var yuzeyler = new int[mats.Length];
            bool kaplanacak = false;
            for (int i = 0; i < mats.Length; i++)
            {
                yuzeyler[i] = YuzeyNumarasi(mats[i]);
                if (yuzeyler[i] >= 0) kaplanacak = true;
            }
            if (kaplanacak) sonuc.Add(new KaplamaYeri { cizici = r, yuzeyler = yuzeyler });
        }
        return sonuc;
    }

    // Dairenin kaplanan parçalarına bu katın malzemelerini takar.
    private void KaplamayiGiydir(int daire, int kat)
    {
        if (kaplamalar == null || kaplamalar.Length < KatKaplamalari.KatSayisi * KatKaplamalari.YuzeySayisi) return;
        if (kaplamaYerleri[daire] == null) return;
        int k = Mathf.Clamp(kat, 1, KatKaplamalari.KatSayisi) - 1;
        foreach (KaplamaYeri yer in kaplamaYerleri[daire])
        {
            // sharedMaterials bir KOPYA dizi döndürür: değiştirip geri atamak gerekir.
            Material[] mats = yer.cizici.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                if (yer.yuzeyler[i] >= 0) mats[i] = kaplamalar[k * KatKaplamalari.YuzeySayisi + yer.yuzeyler[i]];
            yer.cizici.sharedMaterials = mats;
        }
    }

    private static void SahanlikKapisiniAyarla(Transform sahanlik, int n, bool acik, bool gercekDaire)
    {
        if (sahanlik == null) return;
        Transform kapi = sahanlik.Find($"Kapi_{n}");
        if (kapi == null) return;
        // Pozitif açı kapıyı daireye doğru (içeri) açar; menteşeler bunun için aynalı kuruldu.
        Transform mentese = kapi.Find("Mentese");
        if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, acik ? GirisAcikAci : 0f, 0f);
        // Karanlık hol yalnızca gerçek dairenin önünden kalkar; kapalı kapılarda ve B'de durur.
        Transform hol = kapi.Find("Daire_Ici");
        if (hol != null) hol.gameObject.SetActive(!(acik && gercekDaire));
    }

    // Kartın etkilerinden biri ses çalıyor mu? (Yalnızca susturan ya da taşıyan kartlar,
    // örneğin susan_tv, "sesli" sayılmaz: onlar yeni bir ses çıkarmıyor.)
    private static bool SesliKart(IAnomaliKart kart)
    {
        var k = kart as AnomaliKarti;
        if (k == null || k.Etkiler == null) return false;
        foreach (AnomaliEtkisi e in k.Etkiler)
            if (e != null && e.tur == EtkiTuru.Ses) return true;
        return false;
    }

    // Çatının üstünde kat yoktur; seçiciye 9. katı sormuyoruz.
    private IAnomaliKart KartSec(int kat)
    {
        return kat > KatSayaci.CatiKati ? null : secici.KatIcinSec(kat);
    }

    // ---------- ÇATI MODU ----------
    // 8. katta üst sahanlık ve üstündeki bütün katlar kapanır, yerine çatı bölümü
    // açılır. Yukarı ışınlanma da kapanır: oyuncu yukarı çıktığında bir sahanlığa
    // değil çatı kapısına varmalı. Aşağı kaçış yolu açık kalır.
    private void CatiModunuAyarla(bool mod)
    {
        catiModu = mod;

        if (sahanlikB != null) sahanlikB.gameObject.SetActive(!mod);
        if (ustKatlar != null) ustKatlar.SetActive(!mod);
        if (catiBolumu != null) catiBolumu.SetActive(mod);

        if (merdiven != null) merdiven.YukariDonguyuAyarla(!mod);
    }

    private void CatiKapisiniAc()
    {
        Transform kapi = catiBolumu != null ? catiBolumu.transform.Find("CatiKapisi") : null;

        if (kapi == null)
        {
            Debug.LogWarning("[KAT] Çatı kapısı bulunamadı. " +
                             "Graybox'ı 'Huzur → Graybox Sahanlığı Yeniden Kur' ile yenileyin.");
            return;
        }

        kapi.gameObject.SetActive(false);

        // Kapı açılınca sabahın sesi içeri dolar (#25): uzak şehir, kuşlar, rüzgâr.
        // Ses kaynağı çatı bölümünde kapalı bekliyor; açmak çalmak demek.
        Transform sabah = catiBolumu.transform.Find("Ses_Sabah");
        if (sabah != null) sabah.gameObject.SetActive(true);

        // Final (#16): şafak ışığı, güvercinler ve oyuncu çatıya çıkınca dış görüntü.
        FinalSahnesi final = FindAnyObjectByType<FinalSahnesi>();
        if (final != null) final.CatiAcildi(oyuncu);
    }

    // ---------- KAPI NUMARALARI ----------
    // Her katta dört daire, numaralar baştan sona sıralı:
    // 1. katta 1-4, 2. katta 5-8, ... 8. katta 29-32. Formül: 4N-3 ile 4N arası.
    // Numaralar KAT SAYACINA bağlıdır (CLAUDE.md'deki karar); geri dönüş de
    // ilerleme sayıldığı için oyuncu aşağı inerken numaralar artar.
    private void NumaralariYaz(TextMesh[] numaralar, int kat)
    {
        for (int d = 0; d < numaralar.Length; d++)
        {
            if (numaralar[d] != null) numaralar[d].text = (4 * kat - 3 + d).ToString();
        }
    }

    // ---------- ÇÖP POŞETLERİ ----------

    // Sahanlığı bir katın poşetleriyle doldurur: plan kaç poşet diyorsa o kadarı
    // görünür, bu turda toplanmış olanlar görünmez.
    private void PosetleriYerlestir(CopPoseti[,] posetler, int kat)
    {
        int[] adetler = copPlani.KatIcin(kat);

        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
        {
            for (int s = 0; s < CopPlani.EnFazlaPoset; s++)
            {
                CopPoseti p = posetler[d, s];
                if (p == null) continue;

                // Poşetler baştan sayılır: 2 poşet varsa 1. ve 2. görünür.
                bool mevcut = s < adetler[d];
                p.Ayarla(mevcut, copPlani.ToplandiMi(kat, d, s));
            }
        }
    }

    // Sahanlık başka bir katı göstermeye geçmeden önce, oyuncunun o katta
    // topladığı poşetleri plana yazar. Yoksa oyuncu aynı kata döndüğünde
    // topladığı poşetler yerine gelirdi.
    // ---------- AYAKKABILAR (#44) ----------
    private void AyakkabilariBul(Transform sahanlik, out GameObject[,] raflar, out DaginikAyakkabi[,] daginik)
    {
        raflar = new GameObject[MekanOlculeri.DaireSayisi, AyakkabiPlani.RafSayisi];
        daginik = new DaginikAyakkabi[MekanOlculeri.DaireSayisi, AyakkabiPlani.DisaridaEnCok];
        if (sahanlik == null) return;
        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
        {
            for (int r = 0; r < AyakkabiPlani.RafSayisi; r++)
            {
                Transform t = sahanlik.Find($"Kapi_{d + 1}/Ayakkabilik/Raf_{r + 1}/Cift");
                if (t != null) raflar[d, r] = t.gameObject;
            }
            for (int j = 0; j < AyakkabiPlani.DisaridaEnCok; j++)
            {
                Transform t = sahanlik.Find($"Kapi_{d + 1}/Ayakkabilar/Cift_{j + 1}");
                if (t != null) daginik[d, j] = t.GetComponent<DaginikAyakkabi>();
            }
        }
    }

    // Katın ayakkabılarını plana göre dizer: rafta "dolu raf" kadar çift (başlangıçtakiler +
    // bu turda rafa konanlar), kapı önünde henüz rafa konmamış dağınık çiftler.
    private void AyakkabilariYerlestir(GameObject[,] raflar, DaginikAyakkabi[,] daginik, int kat)
    {
        if (ayakkabiPlani == null || kat < KatSayaci.BaslangicKati) return;
        int[] disarida = ayakkabiPlani.Disaridakiler(kat);
        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
        {
            int dolu = ayakkabiPlani.DoluRaf(kat, d);
            for (int r = 0; r < AyakkabiPlani.RafSayisi; r++)
                if (raflar[d, r] != null) raflar[d, r].SetActive(r < dolu);
            for (int j = 0; j < AyakkabiPlani.DisaridaEnCok; j++)
                if (daginik[d, j] != null) daginik[d, j].Ayarla(j < disarida[d], ayakkabiPlani.RafaKonduMu(kat, d, j));
        }
    }

    // Sahanlık başka bir kata geçmeden önce: bu katta rafa konan çiftler plana işlenir,
    // yoksa oyuncu kata geri döndüğünde çiftler yine kapı önünde dururdu.
    private void AyakkabiDurumunuKaydet(DaginikAyakkabi[,] daginik, int kat)
    {
        if (ayakkabiPlani == null || kat < KatSayaci.BaslangicKati) return;
        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
            for (int j = 0; j < AyakkabiPlani.DisaridaEnCok; j++)
                if (daginik[d, j] != null && daginik[d, j].RafaKondu) ayakkabiPlani.RafaKonduOlarakIsaretle(kat, d, j);
    }

    private void DurumuKaydet(CopPoseti[,] posetler, int kat)
    {
        if (kat < KatSayaci.BaslangicKati || copPlani == null) return;

        for (int d = 0; d < MekanOlculeri.DaireSayisi; d++)
        {
            for (int s = 0; s < CopPlani.EnFazlaPoset; s++)
            {
                CopPoseti p = posetler[d, s];
                if (p != null && p.Alindi) copPlani.ToplandiOlarakIsaretle(kat, d, s);
            }
        }
    }

    // ---------- EKSİK GÖREV ----------
    // Verilen katta yapılmamış bir iş kaldı mı? Şu an tek iş çöp toplamak.
    //
    // Sahneye değil PLANA bakıyoruz: sahanlık o anda hangi katı gösteriyor olursa
    // olsun, soru "o katın bütün poşetleri toplandı mı" sorusudur. Plan kaç poşet
    // çektiyse o kadar poşet aranır; olmayan poşet eksik sayılmaz.
    private bool ToplanmamisGorevVar(int kat)
    {
        DurumuKaydet(aPosetler, gosterilenA);   // son toplananları plana işle
        DurumuKaydet(bPosetler, gosterilenB);

        int[] adetler = copPlani.KatIcin(kat);

        for (int d = 0; d < adetler.Length; d++)
        {
            for (int s = 0; s < adetler[d]; s++)
            {
                if (!copPlani.ToplandiMi(kat, d, s)) return true;
            }
        }
        return false;
    }

    // ---------- TEST GÖSTERGESİ ----------
    // Ekranın köşesine kat numarasını ve anomali durumunu yazar. Oyunun arayüzü
    // DEĞİLDİR; sadece elle test yapabilmek içindir.
    //
    // Yalnızca Editor'de ve geliştirme build'inde çizilir (#26). Debug.isDebugBuild
    // Editor'de ve "Development Build" işaretli build'de true, oyuncuya giden yayın
    // build'inde false. Böylece sahnedeki testGostergesi kutusu açık unutulsa bile
    // oyuncu bu yazıyı hiçbir zaman görmez.
    private float ortalamaKareSuresi = 1f / 60f;

    private void OnGUI()
    {
        if (!testGostergesi || !Debug.isDebugBuild || copPlani == null) return;
        if (DuraklatmaMenusu.BaslikEkraninda) return;   // başlık ekranının üstüne çizilmesin (#32)
        ArayuzOlcegi.Uygula();   // gösterge de her ekranda aynı oranda (#34)

        var stil = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        stil.normal.textColor = Color.white;

        string durum = sayac.TurBitti
            // Test göstergesi metin tablosuna girmez (#15): yalnızca geliştirici görüyor,
            // satılan oyunda gösterge yok. Bu yüzden yazıları burada Türkçe duruyor.
            ? "ÇATI — tur bitti"
            : $"Kat {sayac.MevcutKat} / {KatSayaci.CatiKati}   ·   " +
              (sayac.AnomaliVar ? "ANOMALİ VAR (geri dön)" : "normal (yukarı devam)");

        string kartAdi = uygulayiciA.UygulananKart != null
            ? "   ·   kart: " + uygulayiciA.UygulananKart.Kimlik
            : "";

        string poset = toplayici != null
            ? $"   ·   poşet: {toplayici.TasinanPoset} (bu katta {copPlani.KattakiToplam(sayac.MevcutKat)})"
            : "";

        // FPS: saniyedeki kare sayısı = 1 / bir karenin süresi. ms: bir kare kaç milisaniye.
        // Editor'de bu sayı güvenilmez (Editor kendi pencerelerini de çiziyor); gerçek
        // değer geliştirme build'inde okunur.
        string fps = $"   ·   FPS {1f / ortalamaKareSuresi:0} ({ortalamaKareSuresi * 1000f:0.0} ms)";

        // Bu katın olay sesi; kartın kendi sesi varsa olay çalmıyor (#35).
        string olay = olayPlani == null || sayac.TurBitti ? ""
            : SesliKart(KartSec(sayac.MevcutKat)) ? "   ·   olay: yok (kartın sesi var)"
            : "   ·   olay: " + olayPlani.KatIcin(sayac.MevcutKat);

        string alt = $"hata: {sayac.ToplamHata}" + poset + kartAdi + olay + fps +
                     (catiModu ? "   ·   üstte çatı" : "") +
                     (acilisModu ? "   ·   AÇILIŞ (zemin kat)" : "") +
                     (testKarti != null ? "   ·   TEST KARTI AÇIK" : "") +
                     (cezaBekliyor ? "   ·   ceza yolda…" : "") +
                     (cezaCalisiyor ? "   ·   CEZA" : "");

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(14, 12, 720, 62), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(24, 16, 710, 28), durum, stil);

        stil.fontSize = 14;
        stil.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(24, 44, 710, 24), alt, stil);
    }

    // ---------- GECİKMELİ CEZA ----------
    // Bir "coroutine": "yield return" satırında DURUR, süre geçince kaldığı
    // yerden devam eder. Zamana yayılan sahneler için Unity'nin yoludur.
    private IEnumerator CezaGosterisi()
    {
        cezaCalisiyor = true;

        // Döngüyü durdur: karanlıkta ışınlanma olmasın.
        merdiven.DonguyuAyarla(false);

        // Yalnızca şu an AÇIK olan lambalar söner; kapalı katlardakiler zaten görünmüyor.
        var sirali = new System.Collections.Generic.List<Light>();
        foreach (var l in lambalar) if (l.isActiveAndEnabled) sirali.Add(l);

        // Oyuncudan UZAKTAKİNDEN başlayarak söndür. Karanlığın uzaktan
        // yaklaşması, yakından başlamasından daha ürkütücüdür.
        sirali.Sort((a, b) =>
            Vector3.Distance(b.transform.position, oyuncu.position)
            .CompareTo(Vector3.Distance(a.transform.position, oyuncu.position)));

        // İki sönme arasındaki bekleme, toplam süre sabit kalacak şekilde hesaplanıyor.
        float aralik = sirali.Count > 0 ? sonmeSuresi / sirali.Count : 0f;

        for (int i = 0; i < sirali.Count; i++)
        {
            // En son sönen lamba oyuncuya en yakın olanı. O hemen sönmez: önce
            // birkaç kez titrer. Oyuncu artık neyin geldiğini anlar ama durduramaz.
            if (i == sirali.Count - 1) yield return StartCoroutine(Titret(sirali[i]));

            LambayiAyarla(sirali[i], false);

            // Lamba kendi yerinden "tık" diye söner: karanlık uzaktan yaklaşırken
            // ses de uzaktan yaklaşır, son tık oyuncunun tepesinde.
            if (lambaSonme != null) AudioSource.PlayClipAtPoint(lambaSonme, sirali[i].transform.position, 0.9f);

            yield return new WaitForSeconds(aralik);
        }

        // Tam karanlık ve TAM SESSİZLİK (#14). Binanın uğultusu, TV'ler, borular...
        // hepsi aynı anda susar. AudioListener.pause oyundaki bütün sesleri
        // duraklatır; oyuncu sesin yokluğunu, ışığın yokluğundan bile önce duyar.
        // Hiçbir şey olmaz; oyunun en korkunç anı burasıdır.
        AudioListener.pause = true;
        yield return new WaitForSeconds(karanlikSuresi);

        OyuncuyuBasaAl();

        foreach (var lamba in lambalar) LambayiAyarla(lamba, true);

        // Işıkla birlikte ses de geri gelir. Duraklatılan sesler kaldıkları yerden
        // devam eder; hepsi birlikte durduğu için saat kilitleri bozulmaz.
        AudioListener.pause = false;
        for (int i = 0; i < ampuller.Length; i++) ampuller[i].color = ozgunRenkler[i];

        merdiven.DonguyuAyarla(true);

        // Yeni tur: anomali kararları ve poşet sayıları yeniden belirlenir,
        // oyuncu bir önceki denemeyi ezberleyerek ilerleyemez.
        secici.Sifirla();
        copPlani.Sifirla();
        ayakkabiPlani.Sifirla();
        // #52: ceza 1. kata döndürüyor (bu kural #59 ile kalkacak); kayıt ve kapılar da başa.
        acilanKatlar.Clear();
        Cep.Bosalt();
        Kayit.Sil();
        olayPlani.Sifirla();
        if (toplayici != null) toplayici.Sifirla();

        // Sahanlıkların "gösterdiği kat" bilgisi de silinir. Silinmeseydi,
        // bir sonraki kurulumda eski turun toplanmış poşetleri YENİ turun
        // planına kaydedilirdi.
        gosterilenA = gosterilenB = -1;

        cezaCalisiyor = false;
        Debug.Log("[KAT] 1. kata dönüldü.");
        SahanliklariGuncelle(zorla: true);
    }

    // Bir lambayı bütün parçalarıyla açar ya da kapatır: ana spot ışık, dolgu
    // ışığı ve parlayan cam. Yalnızca ışığı kapatsaydık cam karanlıkta parlamaya
    // devam eder, "söndü" hissi hiç oluşmazdı.
    private void LambayiAyarla(Light lamba, bool acik)
    {
        foreach (Light l in lamba.GetComponentsInChildren<Light>(true)) l.enabled = acik;
        foreach (Renderer r in lamba.GetComponentsInChildren<Renderer>(true))
        {
            if (r.name == "Cam") r.enabled = acik;   // kablo zaten koyu, dokunmuyoruz
        }
    }

    // Arızalı bir ampul gibi titreme: kısa sönme, kısa yanma, düzensiz aralıklarla.
    // Aralıklar rastgele; düzenli bir yanıp sönme makine gibi durur, düzensizi arıza gibi.
    private IEnumerator Titret(Light lamba)
    {
        // Titreme boyunca ampulden elektrik cızırtısı.
        if (lambaCizirti != null) AudioSource.PlayClipAtPoint(lambaCizirti, lamba.transform.position, 0.8f);

        for (int i = 0; i < 3; i++)
        {
            LambayiAyarla(lamba, false);
            yield return new WaitForSeconds(Random.Range(0.04f, 0.10f));
            LambayiAyarla(lamba, true);
            yield return new WaitForSeconds(Random.Range(0.06f, 0.22f));
        }
    }

    // Oyun ceza sırasında durdurulursa ses duraklatılmış kalmasın.
    private void OnDisable()
    {
        AudioListener.pause = false;
    }

    private void OnDestroy()
    {
        // Static bir olaya abone olan yok olurken aboneliği bırakmalı; yoksa olay yok olmuş
        // bir nesneye ulaşmaya çalışır (sahne yeniden yüklenince ikinci kez abone de olurduk).
        Kilit.Cozuldu -= Kayit.KilitCozuldu;
    }

    private void OyuncuyuBasaAl()
    {
        // CharacterController açıkken konum elle değiştirilemez; kapat-taşı-aç.
        if (govde != null) govde.enabled = false;

        oyuncu.position = baslangicKonumu;
        oyuncu.rotation = baslangicDonusu;

        if (govde != null) govde.enabled = true;
        Physics.SyncTransforms();

        // Takipçi, oyuncu yere yeniden oturduğunda baştan kurulacak.
        // Karar noktası da burası: ceza sırasında kat edilen yol sayılmaz.
        // NaN ("sayı değil") hiçbir sayıya eşit değildir; böylece bir sonraki
        // kare kesinlikle "oturdu" sayılmaz ve oyuncunun düşmesi beklenir.
        ilerleme = null;
        oncekiY = float.NaN;
        referansDongu = merdiven.NetDongu;
    }
}
