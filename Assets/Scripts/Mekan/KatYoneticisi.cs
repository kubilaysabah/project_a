// KatYoneticisi.cs
// KatSayaci (saf mantık) ile sahne (Unity) arasındaki KÖPRÜ.
//
// İş bölümü şöyle:
//   KatSayaci      -> kuralı bilir, Unity'yi bilmez. Test edilebilir.
//   KatYoneticisi  -> sahneyi bilir, kuralı bilmez. Oyuncunun ne yaptığını
//                     izler, KatSayaci'ya sorar, gelen cevabı sahnede uygular.
//
// Bu ayrımın pratik faydası: kuralı değiştirmek istediğimizde Unity'yi hiç açmayız,
// sahneyi değiştirmek istediğimizde kurala hiç dokunmayız.

using System.Collections;          // IEnumerator (coroutine) için
using UnityEngine;

public class KatYoneticisi : MonoBehaviour
{
    [Header("Bağlantılar")]
    [Tooltip("Oyuncudaki döngü scripti. Boş bırakılırsa sahnede aranır.")]
    [SerializeField] private SonsuzMerdiven merdiven;

    [Tooltip("Oyuncunun kendisi. Boş bırakılırsa 'Oyuncu' adlı nesne aranır.")]
    [SerializeField] private Transform oyuncu;

    [Header("Anomali")]
    [Tooltip("Bir katın anomalili olma olasılığı.")]
    [Range(0f, 1f)]
    [SerializeField] private float anomaliOlasiligi = 0.5f;

    [Tooltip("1. kat her zaman normal olsun mu? Oyuncunun önce NORMALİN neye " +
             "benzediğini görmesi gerekir; kapatmanız önerilmez.")]
    [SerializeField] private bool ilkKatHepNormal = true;

    [Tooltip("Rastgelelik tohumu. 0 = her oyunda farklı tur. Sıfırdan farklı bir " +
             "sayı verirseniz aynı tur birebir tekrar oynanır — hata ayıklarken işe yarar.")]
    [SerializeField] private int tohum = 0;

    [Header("Test")]
    [Tooltip("Ekranın köşesinde kat numarasını ve anomali durumunu gösterir. " +
             "Sadece test içindir; gerçek oyunda kapalı olacak.")]
    [SerializeField] private bool testGostergesi = true;

    [Header("Gecikmeli ceza")]
    [Tooltip("İki ışığın sönmesi arasındaki bekleme (saniye).")]
    [SerializeField] private float ampulAraligi = 0.35f;

    [Tooltip("Tam karanlıkta geçen süre. Oyunun en korkunç anı burasıdır.")]
    [SerializeField] private float karanlikSuresi = 1.4f;

    // ---------- İÇ DURUM ----------

    // Saf mantık nesnesi. Bütün kural burada.
    private KatSayaci sayac = new KatSayaci();

    private Vector3 baslangicKonumu;      // ceza sonrası dönülecek yer
    private bool cezaBekliyor;            // yanlış karar verildi, ışıklar sönmeyi bekliyor
    private bool cezaCalisiyor;           // ceza gösterisi şu an oynuyor

    // Kararı ışınlanmadan değil, gerçekten kat edilen dikey mesafeden okuyan takipçi.
    private KatIlerlemeTakipcisi ilerleme;

    private Light[] ampuller;             // sahnedeki bütün nokta ışıklar
    private Color[] ozgunRenkler;         // anomali/ceza öncesi renkleri

    // Kapı numarası yazıları. A = oyuncunun bulunduğu sahanlık,
    // B = merdivenin tepesindeki sahanlık (yani BİR ÜST kat).
    private TextMesh aSolNumara, aSagNumara, bSolNumara, bSagNumara;

    // ---------- ANOMALİ SİSTEMİ ----------
    private AnomaliSecici secici;

    // İki sahanlık ayrı ayrı bozuluyor, o yüzden HER BİRİNİN kendi uygulayıcısı
    // ve kendi geri alma defteri var. Tek uygulayıcı olsaydı A'yı geri alırken
    // B'nin kaydı silinir ve B'deki anomali kalıcı olurdu.
    private AnomaliUygulayici uygulayiciA;
    private AnomaliUygulayici uygulayiciB;

    private Transform sahanlikA, sahanlikB;
    private AnomaliKarti[] kartlar;

    // ---------- KURULUM ----------

    private void Awake()
    {
        if (merdiven == null) merdiven = FindAnyObjectByType<SonsuzMerdiven>();

        if (oyuncu == null && merdiven != null) oyuncu = merdiven.transform;

        if (oyuncu != null) baslangicKonumu = oyuncu.position;

        // Sahnedeki nokta ışıkları topla. FindObjectsByType, sahnedeki
        // belirli türdeki bütün bileşenleri bulur. Bir kez yapılır; her karede
        // yapmak çok pahalı olurdu.
        var hepsi = FindObjectsByType<Light>();

        var liste = new System.Collections.Generic.List<Light>();
        foreach (var l in hepsi)
        {
            if (l.type == LightType.Point) liste.Add(l);   // güneş ışığını dahil etme
        }
        ampuller = liste.ToArray();

        ozgunRenkler = new Color[ampuller.Length];
        for (int i = 0; i < ampuller.Length; i++) ozgunRenkler[i] = ampuller[i].color;

        // ---- ANOMALİ SİSTEMİ ----
        GameObject a = GameObject.Find("Sahanlik_A");
        GameObject b = GameObject.Find("Sahanlik_B");
        sahanlikA = a != null ? a.transform : null;
        sahanlikB = b != null ? b.transform : null;

        uygulayiciA = new AnomaliUygulayici();
        uygulayiciB = new AnomaliUygulayici();

        // Resources.LoadAll: "Resources" adlı klasörlerin içindeki her şeyi yükler.
        // Kartları oraya koymamızın sebebi şu: yeni bir kart eklemek için hiçbir
        // yere sürüklemek, hiçbir listeye kaydetmek gerekmiyor. Dosyayı klasöre
        // koyuyorsunuz, oyun onu kendiliğinden buluyor. "Kırk yedinci anomali
        // sıfır kod gerektirmeli" kuralı ancak böyle tam olarak sağlanıyor.
        kartlar = Resources.LoadAll<AnomaliKarti>("Anomaliler");

        if (kartlar.Length == 0)
        {
            Debug.LogWarning("[ANOMALİ] Assets/Resources/Anomaliler klasöründe hiç kart yok. " +
                             "Bütün katlar normal geçecek.");
        }

        // Kapı numarası yazılarını bul.
        aSolNumara = NumaraBul("Sahanlik_A/Kapi_Sol/Mentese/Numara");
        aSagNumara = NumaraBul("Sahanlik_A/Kapi_Sag/Mentese/Numara");
        bSolNumara = NumaraBul("Sahanlik_B/Kapi_Sol/Mentese/Numara");
        bSagNumara = NumaraBul("Sahanlik_B/Kapi_Sag/Mentese/Numara");
    }

    private TextMesh NumaraBul(string yol)
    {
        GameObject g = GameObject.Find(yol);
        return g != null ? g.GetComponent<TextMesh>() : null;
    }

    private void Start()
    {
        ilerleme = new KatIlerlemeTakipcisi(MekanOlculeri.DikeyOfset, SanalKot());

        // Tohum 0 ise her oyun farklı olsun diye saatin o anki değerini kullanıyoruz.
        int gercekTohum = tohum != 0 ? tohum : System.Environment.TickCount;

        secici = new AnomaliSecici(kartlar, gercekTohum, anomaliOlasiligi, ilkKatHepNormal);

        YeniKatHazirla();
    }

    // ---------- SANAL KOT ----------
    // Oyuncunun KURGUDAKİ yüksekliği: gerçek yüksekliği artı, şimdiye kadar
    // yapılan ışınlanmaların telafisi.
    //
    // Işınlanma oyuncuyu bir kat aşağı taşır ama oyuncu aslında bir kat ÇIKMIŞTIR.
    // Bu yüzden her yukarı döngüde bir kat ekliyor, her aşağı döngüde bir kat
    // çıkarıyoruz. Sonuç, oyuncu tırmandıkça kesintisiz artan bir sayıdır;
    // ışınlanmanın bu sayıya hiç etkisi olmaz. Karar mantığı işte buna bakar,
    // çünkü ışınlanma bir görüntü hilesidir, oyuncunun kat ettiği yol değildir.
    private float SanalKot()
    {
        if (oyuncu == null) return 0f;

        int netDongu = (merdiven != null)
            ? merdiven.yukariDonguSayisi - merdiven.asagiDonguSayisi
            : 0;

        return oyuncu.position.y + netDongu * MekanOlculeri.DikeyOfset;
    }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (merdiven == null || oyuncu == null) return;

        // Ceza gösterisi oynarken karar dinlemeyiz.
        if (cezaCalisiyor) return;

        if (ilerleme == null) return;

        // Tur bittiyse artık karar dinlemiyoruz. Bu satır olmadan, çatıya
        // ulaşan oyuncu çatıya doğru yürümeye devam ettikçe bir kat daha
        // yol alıyor, karar mantığı yeniden tetikleniyor ve zaten açılmış
        // olan çatı kapısını ikinci kez açmaya çalışıp konsola uyarı basıyordu.
        if (sayac.TurBitti) return;

        float kot = SanalKot();

        // Ceza bekliyorsa, oyuncunun yarım kat yol almasını bekle.
        // Yön bakmıyoruz: yukarı da inse aşağı da, yarım kat yol aldığında
        // ışıklar sönmeye başlar. Mesafe mutlak değer olarak ölçülüyor.
        if (cezaBekliyor)
        {
            if (ilerleme.SonKarardanBeriMesafe(kot) >= MekanOlculeri.DikeyOfset / 2f)
            {
                cezaBekliyor = false;
                // StartCoroutine: aşağıdaki fonksiyonu başlatır ama BİTMESİNİ BEKLEMEZ.
                StartCoroutine(CezaGosterisi());
            }
            return;
        }

        // Oyuncu bir tam kat yüksekliği kadar yol aldıysa karar oluşur.
        if (ilerleme.KararOlustuMu(kot, out Karar karar))
        {
            KarariIsle(karar);
        }
    }

    // ---------- KARARIN SONUCU ----------

    private void KarariIsle(Karar karar)
    {
        KararSonucu sonuc = sayac.KararVer(karar);

        switch (sonuc)
        {
            case KararSonucu.Ilerle:
                Debug.Log($"[KAT] Doğru karar ({karar}). Yeni kat: {sayac.MevcutKat}");
                YeniKatHazirla();
                break;

            case KararSonucu.Sifirla:
                // Ceza ANINDA gelmez. Oyuncu yürümeye devam eder, hiçbir şey
                // olmamış gibi görünür; korku o beklemede birikir.
                Debug.Log($"[KAT] YANLIŞ karar ({karar}). Ceza yolda... " +
                          $"Toplam hata: {sayac.ToplamHata}");
                cezaBekliyor = true;
                break;

            case KararSonucu.CatiyaUlasildi:
                Debug.Log("[KAT] ÇATIYA ULAŞILDI. Döngü kapatıldı, çatı kapısı açıldı.");
                // Döngü kapatılmazsa oyuncu çatıya hiç varamaz, sonsuza kadar döner.
                merdiven.DonguyuAyarla(false);
                CatiKapisiniAc();
                break;
        }
    }

    // ---------- TEST GÖSTERGESİ ----------
    // Ekranın köşesine kat numarasını ve anomali durumunu yazar.
    // Bu, oyunun UI'ı DEĞİLDİR; sadece elle test yapabilmek içindir.
    // Gerçek oyunda ekranda hiçbir yazı olmayacak — kural oyunun kendisidir.
    //
    // OnGUI, Unity'nin eski ve basit çizim yöntemidir. Yeni bir arayüz sistemi
    // kurmadan ekrana yazı basmanın en kısa yolu olduğu için burada tercih edildi.
    // Her karede birkaç kez çağrılır, o yüzden gerçek oyun arayüzünde kullanılmaz.
    private void OnGUI()
    {
        if (!testGostergesi) return;

        var stil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        stil.normal.textColor = Color.white;

        string durum = sayac.TurBitti
            ? "ÇATI — tur bitti"
            : $"Kat {sayac.MevcutKat} / {KatSayaci.CatiKati}   ·   " +
              (sayac.AnomaliVar ? "ANOMALİ VAR (geri dön)" : "normal (yukarı devam)");

        // Hangi kartın uygulandığını da yazıyoruz: anomaliyi göremezsek
        // kartın yanlış mı yazıldığını yoksa gerçekten ince mi olduğunu
        // ancak böyle ayırt edebiliriz.
        string kartAdi = (uygulayiciA != null && uygulayiciA.UygulananKart != null)
            ? "   ·   kart: " + uygulayiciA.UygulananKart.Kimlik
            : "";

        string alt = $"hata: {sayac.ToplamHata}" + kartAdi +
                     (cezaBekliyor ? "   ·   ceza yolda…" : "") +
                     (cezaCalisiyor ? "   ·   CEZA" : "");

        // Arka plan kutusu, yazı beyaz duvarda kaybolmasın.
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(14, 12, 460, 62), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(24, 16, 450, 28), durum, stil);

        stil.fontSize = 14;
        stil.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(24, 44, 450, 24), alt, stil);
    }

    // ---------- ÇATI KAPISI ----------
    // Kapı normalde kapalıdır ve merdiven boşluğunun tepesini örterek
    // gökyüzünün görünmesini de engeller. Açmak = nesneyi devre dışı bırakmak.
    // Açıldığı anda çatı düzlüğü ve gökyüzü görünür olur.
    private void CatiKapisiniAc()
    {
        GameObject kapi = GameObject.Find("CatiBolumu/CatiKapisi");

        if (kapi == null)
        {
            Debug.LogWarning("[KAT] Çatı kapısı bulunamadı. " +
                             "Graybox'ı 'Huzur → Graybox Sahanlığı Yeniden Kur' ile yenileyin.");
            return;
        }

        kapi.SetActive(false);
    }

    // ---------- YENİ KAT ----------

    private void YeniKatHazirla()
    {
        int kat = sayac.MevcutKat;

        // SIRA ÖNEMLİ, üç adım kesinlikle bu düzende olmalı:
        //
        // 1) Önce eski anomaliler geri alınır. Sahanlık tertemiz olur.
        // 2) Sonra kapı numaraları yazılır. Artık sahanlık o katın DOĞRU hâlinde.
        // 3) En son kart uygulanır.
        //
        // Neden? Uygulayıcı "eski hâl" olarak gördüğü şeyi kaydeder. Numaraları
        // karttan SONRA yazsaydık, kartın değiştirdiği numarayı ezerdik.
        // Geri almayı numaralardan SONRA yapsaydık, uygulayıcı eski katın
        // numarasını geri yazardı ve numara bir kat geride kalırdı.

        uygulayiciA.GeriAl();
        uygulayiciB.GeriAl();

        KapiNumaralariniYaz();

        // Alt sahanlık bu katı, üst sahanlık BİR ÜST katı gösterir.
        // Seçici aynı kata her zaman aynı cevabı verdiği için, oyuncu
        // yukarı çıkıp ışınlandığında sahanlık değişmez.
        IAnomaliKart aKart = secici.KatIcinSec(kat);
        IAnomaliKart bKart = secici.KatIcinSec(kat + 1);

        // Seçici arayüz üzerinden çalışıyor; sahnedeki gerçek tip AnomaliKarti.
        if (sahanlikA != null) uygulayiciA.Uygula(aKart as AnomaliKarti, sahanlikA);
        if (sahanlikB != null) uygulayiciB.Uygula(bKart as AnomaliKarti, sahanlikB);

        // Karar kuralı yalnızca oyuncunun BULUNDUĞU sahanlığa bakar.
        sayac.KatiHazirla(aKart != null);

        string durum = aKart != null ? $"VAR ({aKart.Kimlik})" : "yok";
        Debug.Log($"[KAT] {kat}. kat hazır — anomali: {durum}");
    }

    // ---------- KAPI NUMARALARI ----------
    // Her katta iki daire var, numaralar baştan sona sıralı gider:
    // 1. katta 1-2, 2. katta 3-4, ... 8. katta 15-16.
    //
    // Oyuncunun bulunduğu sahanlık (A) mevcut katı gösterir.
    // Merdivenin tepesindeki sahanlık (B) BİR ÜST katı gösterir; oyuncu
    // merdiveni çıkarken oraya baktığında numaraların arttığını görür.
    // Işınlanma anında A, B'nin numaralarını devralır ve döngü sürer.
    private void KapiNumaralariniYaz()
    {
        Yaz(aSolNumara, aSagNumara, sayac.MevcutKat);
        Yaz(bSolNumara, bSagNumara, sayac.MevcutKat + 1);
    }

    private void Yaz(TextMesh sol, TextMesh sag, int kat)
    {
        // Kat 1 -> 1 ve 2. Kat 2 -> 3 ve 4. Formül: 2*kat-1 ve 2*kat.
        if (sol != null) sol.text = (kat * 2 - 1).ToString();
        if (sag != null) sag.text = (kat * 2).ToString();
    }

    // renk Color.clear verilirse özgün renge dönülür.
    private void AmpulleriBoya(Color renk)
    {
        for (int i = 0; i < ampuller.Length; i++)
        {
            ampuller[i].color = (renk == Color.clear) ? ozgunRenkler[i] : renk;
        }
    }

    // ---------- GECİKMELİ CEZA ----------
    // Bu bir "coroutine"dir. Normal bir fonksiyon baştan sona tek karede biter.
    // Coroutine ise "yield return" satırında DURUR, belirtilen süre geçince
    // kaldığı yerden devam eder. Zamana yayılan sahneler için Unity'nin yoludur.
    // Geri dönüş tipi IEnumerator olmak zorundadır; Unity bunu böyle tanır.
    private IEnumerator CezaGosterisi()
    {
        cezaCalisiyor = true;

        // Döngüyü durdur: karanlıkta ışınlanma olmasın.
        merdiven.DonguyuAyarla(false);

        // Işıkları oyuncudan UZAKTAKİNDEN başlayarak söndür.
        // Karanlığın uzaktan yaklaşması, yakından başlamasından daha ürkütücüdür.
        var sirali = new System.Collections.Generic.List<Light>(ampuller);
        sirali.Sort((a, b) =>
            Vector3.Distance(b.transform.position, oyuncu.position)
            .CompareTo(Vector3.Distance(a.transform.position, oyuncu.position)));

        foreach (var ampul in sirali)
        {
            ampul.enabled = false;
            yield return new WaitForSeconds(ampulAraligi);   // burada bekler
        }

        // Tam karanlık. Oyunun en korkunç anı burasıdır: hiçbir şey olmaz.
        yield return new WaitForSeconds(karanlikSuresi);

        // Oyuncuyu başa al.
        OyuncuyuBasaAl();

        // Işıkları geri yak.
        foreach (var ampul in ampuller) ampul.enabled = true;
        AmpulleriBoya(Color.clear);

        merdiven.DonguyuAyarla(true);
        cezaCalisiyor = false;

        // Kat kararları silinir: aynı tur bir daha aynı sırayla gelmesin,
        // oyuncu ezberleyerek ilerleyemesin.
        secici.Sifirla();

        Debug.Log("[KAT] 1. kata dönüldü.");
        YeniKatHazirla();
    }

    private void OyuncuyuBasaAl()
    {
        // CharacterController açıkken konum elle değiştirilemez; kapat-taşı-aç.
        var cc = oyuncu.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        oyuncu.position = baslangicKonumu;

        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();

        // Oyuncu başa alındı; ilerleme sayacı da yeni kotuna sıfırlanmalı.
        // Yoksa ceza sırasında kat edilen yol bir sonraki karara sayılır.
        ilerleme.Sifirla(SanalKot());
    }
}
