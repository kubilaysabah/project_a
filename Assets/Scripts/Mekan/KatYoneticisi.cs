// KatYoneticisi.cs
// KatSayaci (saf mantık) ile sahne (Unity) arasındaki KÖPRÜ.
//
// İş bölümü şöyle:
//   KatSayaci      -> bulunulan katı bilir, Unity'yi bilmez. Test edilebilir.
//   CopPlani       -> hangi katta kaç poşet olacağını bilir. Test edilebilir.
//   KatYoneticisi  -> sahneyi bilir. Oyuncunun merdivende ne yaptığını izler,
//                     sahanlıkları ve daireyi bulunulan kata göre kurar.
//
// #59 (Şahan, 2026-09-27): eski oyunun "anomali varsa geri dön; yanlış karar 1. kata ceza"
// kuralı, rastgele anomali seçimi, gecikmeli ceza ve anomali defteri tamamen kaldırıldı.
// Bir üst kata yalnızca kat kapısı açılıp merdivenden çıkılarak geçilir.

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

    /// <summary>Oyuncunun bulunduğu kat (sayaç henüz kurulmadıysa 0).</summary>
    public int MevcutKat { get { return sayac != null ? sayac.MevcutKat : 0; } }

    /// <summary>
    /// Gezen Gölge'nin (#71) saati işlesin mi? Oyuncu bir katta, o katın kapısını açıp çıkamadığı
    /// sürece evet. Açılışta (zemin kat), bulunulan katın kapısı açıldıktan sonra ve çatı kapısı
    /// açılınca (final) hayır.
    /// </summary>
    public bool GolgeSayabilir
    {
        get { return sayac != null && !acilisModu && !catiAcildi && !acilanKatlar.Contains(sayac.MevcutKat); }
    }

    // Çatı kapısı açıldı mı (final başladı)?
    private bool catiAcildi;

    // 8. katta oyuncu çatı odasına çıktı mı (#68)? Yeni ilerlemede çatı odasına çıkmak bir
    // karar değildir: kapı kilitlidir, oyuncu geri inip dairedeki işi bitirebilir.
    private bool catiOdasinda;

    [Header("Rastgelelik")]
    [Tooltip("Rastgelelik tohumu (çöp, ayakkabı ve kat olayı planları). 0 = her oyunda farklı. " +
             "Sıfırdan farklı bir sayı verirseniz aynı tur birebir tekrar oynanır — hata ayıklarken işe yarar.")]
    [SerializeField] private int tohum = 0;

    [Header("Açılış (#16)")]
    [Tooltip("Oyun zemin kattaki kapıcı dairesinde, yönetici notuyla başlasın mı? " +
             "Kapatırsanız doğrudan 1. katta başlar.")]
    [SerializeField] private bool acilisIleBasla = true;

    [Header("Test")]
    [Tooltip("Ekranın köşesinde kat numarasını gösterir. " +
             "Sadece test içindir; gerçek oyunda kapalı olacak.")]
    [SerializeField] private bool testGostergesi = true;

    // ---------- İÇ DURUM ----------

    private KatSayaci sayac = new KatSayaci();

    private Vector3 baslangicKonumu;      // kaçışta yakalanınca dönülecek yer (katın sahanlığı)
    private Quaternion baslangicDonusu;   // ve bakılacak yön

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

    // ---- SAHNE PARÇALARI ----
    private Transform sahanlikA, sahanlikB;
    private GameObject ustKatlar;         // B'nin üstündeki sahte katlar
    private GameObject catiBolumu;        // 8. katta B'nin yerine geçen çatı

    // Her sahanlıkta dört kapı numarası ve kapı başına üç poşet.
    // [,] iki boyutlu dizi demek: posetler[kapı, sıra]. Satranç tahtası gibi;
    // bir sayı satırı, öbürü sütunu seçer.
    private TextMesh[] aNumaralar, bNumaralar;
    private CopPoseti[,] aPosetler, bPosetler;

    // ---- SENARYOLU SAHANLIK KARTI (#67) ----
    // Rastgele anomali kalmadı; yalnızca kat dosyası isterse (7. kat: gerçek kapının
    // izleri) alt sahanlığa senaryolu bir kart uygulanır. Uygulayıcı her değişikliği
    // kaydeder ve kat değişince geri alır.
    private AnomaliUygulayici uygulayiciA = new AnomaliUygulayici();

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

        // Her plan tohuma farklı bir sayı ekleyerek kuruluyor. Aynı tohumu verseydik
        // üreticiler aynı sayı dizisini üretir ve planlar birbirine gizlice bağlanırdı
        // (örneğin "3 poşetli katta hep bebek sesi" gibi).
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
        bool kayittanDevam = Kayit.Var;
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

        // Oyuncuyu kapıcı odasına al, yüzü kapıya dönük (+x). Kaçışta yakalanınca dönülecek
        // yer (baslangicKonumu) Awake'te kaydedildi ve katın sahanlığı olarak kalıyor.
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

        // Çatı kapısı açıldıktan (final) sonra kat dinlemeyiz.
        if (catiAcildi) return;

        // Takipçiyi oyuncu yere OTURDUĞU anda kur.
        //
        // "Yerde mi?" (isGrounded) tek başına yetmiyor. Bu bilgi SON hareketten
        // kalmadır: kaçışta yakalanınca oyuncuyu başlangıç noktasına 20 cm havada
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

        // Oyuncu merdivende bir tam kat yüksekliği kadar yol aldıysa bir "kat geçişi" oluşur.
        if (ilerleme.KararOlustuMu(kot, out Karar yon))
        {
            referansDongu = merdiven.NetDongu;   // yeni ölçüm noktası burası
            KatGecisiniIsle(yon);
        }

        SahanliklariGuncelle(zorla: false);

        // Senaryolu sahanlık kartının (#67) zamanlı etkileri, yalnızca oyuncunun BULUNDUĞU
        // sahanlıkta işler: A mevcut katı gösteriyorsa oyuncu oradadır.
        if (gosterilenA == sayac.MevcutKat) uygulayiciA.Ilerlet(Time.deltaTime);

        // Kat olayının saati de aynı kuralla işler (#35).
        if (gosterilenA == sayac.MevcutKat) olayCaliciA.Ilerlet(Time.deltaTime);
    }

    // ---------- KAT GEÇİŞİ ----------
    // Ceza yok. Aşağı inmek sayılır ama katı değiştirmez; aşağıdan geri çıkmak da yalnızca
    // sayacı azaltır. Bir üst kata ancak bulunduğu kattan, kat kapısını açıp çıkarak geçilir
    // (kapı kilitli olduğu için başka yol yok).
    private void KatGecisiniIsle(Karar yon)
    {
        // Çatı odasından 8. kata geri inmek (#68): yalnızca bayrak iner, kat 8 kalır.
        if (yon == Karar.Geri && catiOdasinda) { catiOdasinda = false; Debug.Log("[KAT] Çatı odasından 8. kata inildi."); return; }
        if (yon == Karar.Geri) { asagiInilen++; Debug.Log($"[KAT] Aşağı inildi ({asagiInilen}); kat {sayac.MevcutKat} kalıyor."); return; }
        if (asagiInilen > 0) { asagiInilen--; Debug.Log($"[KAT] Geri çıkıldı ({asagiInilen}); kat {sayac.MevcutKat}."); return; }

        // 8. kattan yukarı: çatı odasına çıkıldı. Kapı kendiliğinden AÇILMAZ; çatı anahtarı
        // gerekir (CatiKapisi -> CatiKapisiAnahtarlaAcildi).
        if (sayac.MevcutKat >= KatSayaci.CatiKati) { catiOdasinda = true; Debug.Log("[KAT] Çatı odasına çıkıldı; kapı çatı anahtarını bekliyor."); return; }

        int eskiKat = sayac.MevcutKat;
        if (!sayac.Ilerle()) return;

        // Sahanlık burada yenilenmiyor; hemen ardından SahanliklariGuncelle ölçüm noktasının
        // değiştiğini görüp yeniliyor.
        Debug.Log($"[KAT] Yukarı çıkıldı: {eskiKat} -> {sayac.MevcutKat}");
        // Otomatik kayıt (#52): yeni bir kata ulaşıldı.
        if (sayac.MevcutKat > Kayit.Kat) Kayit.Kat = sayac.MevcutKat;
    }

    // ---------- SAHANLIKLARIN GÖSTERDİĞİ KAT ----------
    // Sahnede iki gerçek sahanlık var ve her biri bir KATI temsil ediyor.
    // Hangi katı temsil ettiği, ölçüm noktasına göre nerede durduğuna bağlı:
    //
    //   Ölçüm noktasındaki sahanlık   -> mevcut kat (N).
    //   Bir üstündeki                 -> bir sonraki kat (N+1).
    //   Bir altındaki                 -> yine N.
    //
    // Neden alttaki de N? Aşağı inmek katı değiştirmez; tek sahanlık döngüsünde aşağıdaki
    // katlar da bu katın görüntüsünü gösterir (oyuncu bir şey almak için inebilir). Oyuncu
    // aşağıdayken üstündeki sahanlık da N'dir: geri çıkınca yine kendi katına varır.
    //
    // Bunu her karede hesaplamamızın sebebi şu: ışınlanma sahanlıkların yerini değiştirir,
    // ama hesap ışınlanmadan değil kat edilen yoldan yapılır; böylece hiçbir ışınlanma
    // ekranda bir sahanlığın aniden değişmesine yol açmaz.
    private int GosterilecekKat(int karardanUzaklik)
    {
        if (karardanUzaklik < 0 || asagiInilen > 0) return sayac.MevcutKat;
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
        // 2) Eski katın senaryolu kartı geri alınır. Sahanlık tertemiz olur.
        // 3) Poşetler ve kapı numaraları yeni kata göre yazılır.
        // 4) En son kart (varsa) uygulanır.
        // Kart numaralardan önce uygulansaydı, numara yazılırken kartın
        // değiştirdiği numara ezilirdi. Geri alma numaralardan sonra yapılsaydı
        // uygulayıcı eski katın numarasını geri yazardı.
        DurumuKaydet(aPosetler, gosterilenA);
        DurumuKaydet(bPosetler, gosterilenB);
        AyakkabiDurumunuKaydet(aDaginik, gosterilenA);
        AyakkabiDurumunuKaydet(bDaginik, gosterilenB);

        uygulayiciA.GeriAl();

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
        // Kat dosyası bütün kapılara tek bir numara verebilir (#67: 7. katta dördü de "13").
        OzelNumaraYaz(aNumaralar, aKat);
        OzelNumaraYaz(bNumaralar, bKat);

        // Açık daire (#53): kart uygulanmadan ÖNCE. Kart bir kapıyı döndürüyorsa (aralık kapı
        // gibi) önce bizim verdiğimiz açıyı kaydeder, geri alırken ona döner; sıra ters olsaydı
        // kartın açtığı kapıyı biz ezerdik.
        DaireleriAyarla(aKat, cati ? 0 : bKat);

        // Katın senaryolu sahanlık kartı (#67) yalnızca alt sahanlığa uygulanır: oyuncu hep
        // A'da durur, B yalnızca merdivenden görülür.
        KatZinciriVarligi aVarlik = VarlikBul(aKat);
        AnomaliKarti aKart = aVarlik != null ? aVarlik.sahanlikKarti : null;
        if (sahanlikA != null) uygulayiciA.Uygula(aKart, sahanlikA);

        // Kat kapıları ve anahtarlar (#52).
        KapilariAyarla(aKat, bKat, cati);

        // A'nın kat olayı (#35).
        bool olayVar = aKat >= KatSayaci.BaslangicKati && aKat <= KatSayaci.CatiKati;
        olayCaliciA.Hazirla(olayVar, olayPlani.KatIcin(aKat), olayPlani.ZamanlarKatIcin(aKat));

        // TV ve boru o katın kendi anını çalsın (#39).
        KatSesleriniKaydir(aKatSesleri, aKat);
        KatSesleriniKaydir(bKatSesleri, bKat);

        gosterilenA = aKat;
        gosterilenB = bKat;

        string kart = aKart != null ? $" (sahanlık kartı: {aKart.Kimlik})" : "";
        Debug.Log($"[KAT] A = {aKat}. kat{kart}, B = {(cati ? "çatı" : bKat + ". kat")}");
    }

    // ---------- BAŞKA BİR SES ÇALIYOR MU? (#37) ----------
    // Dış sesler (DisSesleri) çalmadan önce bunu sorar: kat olayı ya da senaryolu kartın
    // sesi çalıyorsa araya girmez.
    public bool SesMesgul
    {
        get
        {
            if (olayCaliciA != null && olayCaliciA.CaliyorMu) return true;
            if (sahanlikA != null)
                foreach (AudioSource k in sahanlikA.GetComponentsInChildren<AudioSource>())
                    if (k.name == "Anomali_Sesi" && k.isPlaying) return true;
            return false;
        }
    }

    /// <summary>Tur bitti mi (çatı kapısı açıldı, final başladı)? Dış sesler buna bakar.</summary>
    public bool TurBitti { get { return catiAcildi; } }

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

        GirisKapilariniAyarla(aAcik, aKat, varlik);

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

    /// <summary>Çatı kapısı çatı anahtarıyla açıldı (#68): döngü kapanır, final başlar.</summary>
    public void CatiKapisiAnahtarlaAcildi()
    {
        Debug.Log("[KAT] ÇATI KAPISI ÇATI ANAHTARIYLA AÇILDI.");
        if (merdiven != null) merdiven.DonguyuAyarla(false);
        CatiModunuAyarla(true);
        CatiKapisiniAc();
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
        catiAcildi = true;   // final: Gezen Gölge artık çıkmaz (#71)

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

    private void OzelNumaraYaz(TextMesh[] numaralar, int kat)
    {
        KatZinciriVarligi v = VarlikBul(kat);
        if (v == null || string.IsNullOrEmpty(v.kapiNumarasi)) return;
        foreach (TextMesh t in numaralar) if (t != null) t.text = v.kapiNumarasi;
    }

    // ---------- SAHANLIK KAPILARI (#67) ----------
    // "Giriş kapalı" katta dört kapıya SahanlikKapisi eklenir: gerçek olan (açık daireninki)
    // çalınınca açılır, öbürleri bir olay tetikler. Başka katlarda bileşenler sökülür.
    private void GirisKapilariniAyarla(int acikDaire, int kat, KatZinciriVarligi varlik)
    {
        if (sahanlikA == null) return;
        bool kapali = varlik != null && varlik.girisKapali && acikDaire > 0;
        for (int n = 1; n <= MekanOlculeri.DaireSayisi; n++)
        {
            Transform kapi = sahanlikA.Find($"Kapi_{n}");
            if (kapi == null) continue;
            SahanlikKapisi sk = kapi.GetComponent<SahanlikKapisi>();
            if (!kapali)
            {
                if (sk != null) DestroyImmediate(sk);
                continue;
            }
            if (sk == null) sk = kapi.gameObject.AddComponent<SahanlikKapisi>();
            bool gercek = n == acikDaire;
            sk.Hazirla(gercek, kat, "giris", varlik.tiklatmaSesi, varlik.yanlisKapiSesleri);
            if (!gercek) continue;
            // Gerçek kapı: daha önce açıldıysa (kayıt) açık, değilse kapalı başlar.
            if (Kayit.KilitCozukMu(kat, "giris")) sk.AcikBaslat();
            else
            {
                Transform mentese = kapi.Find("Mentese");
                if (mentese != null) mentese.localRotation = Quaternion.identity;
            }
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

    // ---------- TEST GÖSTERGESİ ----------
    // Ekranın köşesine kat numarasını yazar. Oyunun arayüzü
    // DEĞİLDİR; sadece elle test yapabilmek içindir.
    //
    // Yalnızca Editor'de ve geliştirme build'inde çizilir (#26). Debug.isDebugBuild
    // Editor'de ve "Development Build" işaretli build'de true, oyuncuya giden yayın
    // build'inde false. Böylece sahnedeki testGostergesi kutusu açık unutulsa bile
    // oyuncu bu yazıyı hiçbir zaman görmez.
    private float ortalamaKareSuresi = 1f / 60f;

    private void OnGUI()
    {
        // Kaçışta yakalanınca kararan ekran (#58). Test göstergesinden önce ve ondan bağımsız:
        // yayın build'inde de çizilmeli.
        if (yakalanmaKarartmasi > 0f)
        {
            ArayuzOlcegi.Uygula();
            GUI.color = new Color(0f, 0f, 0f, yakalanmaKarartmasi);
            GUI.DrawTexture(new Rect(0, 0, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (!testGostergesi || !Debug.isDebugBuild || copPlani == null) return;
        if (DuraklatmaMenusu.BaslikEkraninda) return;   // başlık ekranının üstüne çizilmesin (#32)
        ArayuzOlcegi.Uygula();   // gösterge de her ekranda aynı oranda (#34)

        var stil = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        stil.normal.textColor = Color.white;

        string durum = catiAcildi
            // Test göstergesi metin tablosuna girmez (#15): yalnızca geliştirici görüyor,
            // satılan oyunda gösterge yok. Bu yüzden yazıları burada Türkçe duruyor.
            ? "ÇATI — tur bitti"
            : $"Kat {sayac.MevcutKat} / {KatSayaci.CatiKati}" +
              (asagiInilen > 0 ? $"   ·   {asagiInilen} kat aşağıda" : "") +
              (acilanKatlar.Contains(sayac.MevcutKat) ? "   ·   kat kapısı açık" : "");

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

        // Bu katın olay sesi (#35).
        string olay = olayPlani == null || catiAcildi ? ""
            : "   ·   olay: " + olayPlani.KatIcin(sayac.MevcutKat);

        string alt = $"kayıt: {Kayit.Kat}. kat" + poset + kartAdi + olay + fps +
                     (catiModu ? "   ·   üstte çatı" : "") +
                     (catiOdasinda ? "   ·   çatı odasında" : "") +
                     (acilisModu ? "   ·   AÇILIŞ (zemin kat)" : "");

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(14, 12, 720, 62), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(24, 16, 710, 28), durum, stil);

        stil.fontSize = 14;
        stil.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(24, 44, 710, 24), alt, stil);
    }

    private void OnDestroy()
    {
        // Static bir olaya abone olan yok olurken aboneliği bırakmalı; yoksa olay yok olmuş
        // bir nesneye ulaşmaya çalışır (sahne yeniden yüklenince ikinci kez abone de olurduk).
        Kilit.Cozuldu -= Kayit.KilitCozuldu;
    }

    /// <summary>Kaçış anında yakalanan oyuncu katın başında (sahanlığın başında) uyanır (#58).</summary>
    public void OyuncuyuKatBasinaAl() { OyuncuyuBasaAl(); }

    // ---------- KAÇIŞTA YAKALANMA (#58) ----------
    // Şahan'ın kararı (2026-09-27): Gölge yakalarsa oyun değil, yalnızca O KAT baştan başlar.
    // Ekran kararır; karanlıkta elindeki eşya düşer, katın kaydı silinir, oyuncu sahanlığın
    // başına alınır ve daire sıfırdan kurulur (kilitler kapalı, anahtarlar yerinde, kaçış yeniden
    // hazır). Sonra ekran açılır.
    //
    // Bu iş neden KacisAni'de değil de burada? Daire yeniden kurulurken zincir uygulayıcısı
    // dairedeki KacisAni bileşenini söker. Karartma KacisAni'nin içinde çalışsaydı bileşen
    // sökülünce yarıda kalırdı: ekran açılmaz, oyuncunun kontrolü geri verilmezdi. Kat
    // yöneticisi hep sahnede durur; işi o bitirir.
    private float yakalanmaKarartmasi;       // 0-1: ekranın karalığı

    /// <summary>Kaçışta yakalanıldı: katı baştan kurar (KacisAni çağırır).</summary>
    public void KacistaYakalandi(int kat) { StartCoroutine(KatiBastanKur(kat)); }

    private IEnumerator KatiBastanKur(int kat)
    {
        // Karanlıkta oyuncu yürüyemesin.
        BirinciSahisKontrolcu kontrol = oyuncu != null ? oyuncu.GetComponent<BirinciSahisKontrolcu>() : null;
        if (kontrol != null) kontrol.enabled = false;

        // 0,35 sn'de ekran kararır.
        for (float t = 0f; t < 0.35f; t += Time.deltaTime) { yakalanmaKarartmasi = t / 0.35f; yield return null; }
        yakalanmaKarartmasi = 1f;

        // Karanlıkta: elindeki (katın anahtarı) düşer, katın kaydı silinir, oyuncu başa alınır.
        Cep.Bosalt();
        Kayit.KatiSifirla(kat);
        OyuncuyuBasaAl();
        // Sahanlıklar ve daire zorla yeniden kurulur: zincir uygulayıcısı kayda bakar, kayıtta
        // artık bu katın hiçbir kilidi yok, her şey baştan yerine konur. Eski KacisAni sökülürken
        // Gölge'yi de yok eder; yerine takılan yeni KacisAni anahtarı yine bekler.
        SahanliklariGuncelle(true);
        Debug.Log($"[KACIS] {kat}. kat baştan kuruldu.");

        yield return new WaitForSeconds(1.4f);
        if (kontrol != null) kontrol.enabled = true;
        // 1,2 sn'de ekran açılır.
        for (float t = 1f; t > 0f; t -= Time.deltaTime / 1.2f) { yakalanmaKarartmasi = t; yield return null; }
        yakalanmaKarartmasi = 0f;
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
        // Ölçüm noktası da burası: karartma sırasında kat edilen yol sayılmaz.
        // NaN ("sayı değil") hiçbir sayıya eşit değildir; böylece bir sonraki
        // kare kesinlikle "oturdu" sayılmaz ve oyuncunun düşmesi beklenir.
        ilerleme = null;
        oncekiY = float.NaN;
        referansDongu = merdiven.NetDongu;
        catiOdasinda = false;   // başa alınan oyuncu çatı odasında değil (#68)
        asagiInilen = 0;        // ve kendi katının sahanlığında duruyor
    }
}
