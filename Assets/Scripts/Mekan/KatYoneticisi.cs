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

    [Header("Anomali — GEÇİCİ")]
    [Tooltip("GEÇİCİ: gerçek anomali sistemi #5'te gelecek. Şimdilik her kat " +
             "bu olasılıkla anomalili sayılıyor ve ampul yeşile dönüyor.")]
    [Range(0f, 1f)]
    [SerializeField] private float anomaliOlasiligi = 0.4f;

    [Header("Gecikmeli ceza")]
    [Tooltip("İki ışığın sönmesi arasındaki bekleme (saniye).")]
    [SerializeField] private float ampulAraligi = 0.35f;

    [Tooltip("Tam karanlıkta geçen süre. Oyunun en korkunç anı burasıdır.")]
    [SerializeField] private float karanlikSuresi = 1.4f;

    // ---------- İÇ DURUM ----------

    // Saf mantık nesnesi. Bütün kural burada.
    private KatSayaci sayac = new KatSayaci();

    private Vector3 baslangicKonumu;      // ceza sonrası dönülecek yer
    private int sonYukariSayisi;          // döngü sayaçlarının en son görülen hali
    private int sonAsagiSayisi;
    private bool cezaBekliyor;            // yanlış karar verildi, ışıklar sönmeyi bekliyor
    private bool cezaCalisiyor;           // ceza gösterisi şu an oynuyor
    private Karar cezayaYolAcanKarar;     // ceza hangi yönde yürürken tetiklenecek

    private Light[] ampuller;             // sahnedeki bütün nokta ışıklar
    private Color[] ozgunRenkler;         // anomali/ceza öncesi renkleri

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
    }

    private void Start()
    {
        YeniKatHazirla();
    }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (merdiven == null || oyuncu == null) return;

        // Ceza gösterisi oynarken karar dinlemeyiz.
        if (cezaCalisiyor) return;

        // Ceza bekliyorsa, oyuncunun yarım kat yol almasını bekle.
        if (cezaBekliyor)
        {
            // Yön önemli. Yukarı giderken oyuncu merdivenin ortasına ALTTAN yaklaşır,
            // aşağı inerken ÜSTTEN. Tek bir ">=" koyarsak, aşağı inen oyuncu zaten
            // ortanın üstünde olduğu için ceza anında patlar ve gecikme kaybolur.
            float z = oyuncu.position.z;
            bool yarimKatGecildi = (cezayaYolAcanKarar == Karar.Yukari)
                ? (z >= MekanOlculeri.MerdivenOrtasiZ)
                : (z <= MekanOlculeri.MerdivenOrtasiZ);

            if (yarimKatGecildi)
            {
                cezaBekliyor = false;
                // StartCoroutine: aşağıdaki fonksiyonu başlatır ama BİTMESİNİ BEKLEMEZ.
                StartCoroutine(CezaGosterisi());
            }
            return;
        }

        KararlariDinle();
    }

    // Döngü sayaçlarındaki artışı karara çevirir.
    private void KararlariDinle()
    {
        if (merdiven.yukariDonguSayisi > sonYukariSayisi)
        {
            sonYukariSayisi = merdiven.yukariDonguSayisi;
            KarariIsle(Karar.Yukari);
        }
        else if (merdiven.asagiDonguSayisi > sonAsagiSayisi)
        {
            sonAsagiSayisi = merdiven.asagiDonguSayisi;
            KarariIsle(Karar.Geri);
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
                cezayaYolAcanKarar = karar;
                break;

            case KararSonucu.CatiyaUlasildi:
                Debug.Log("[KAT] ÇATIYA ULAŞILDI. Döngü kapatıldı, çatı kapısı açıldı.");
                // Döngü kapatılmazsa oyuncu çatıya hiç varamaz, sonsuza kadar döner.
                merdiven.DonguyuAyarla(false);
                CatiKapisiniAc();
                break;
        }
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
        // GEÇİCİ: gerçek anomali kartları #5'te gelecek.
        bool anomali = Random.value < anomaliOlasiligi;
        sayac.KatiHazirla(anomali);

        // GEÇİCİ GÖRSEL İPUCU: anomalili katta ampul yeşile döner.
        // Elle test edebilmek için şart; #5'te veri güdümlü sistem bunun yerini alacak.
        AmpulleriBoya(anomali ? new Color(0.45f, 1f, 0.5f) : Color.clear);

        Debug.Log($"[KAT] {sayac.MevcutKat}. kat hazır — anomali: {(anomali ? "VAR" : "yok")}");
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

        // Döngü sayaçları değişmiş olabilir; köprünün yanlış karar okumaması için eşitle.
        sonYukariSayisi = merdiven.yukariDonguSayisi;
        sonAsagiSayisi = merdiven.asagiDonguSayisi;
    }
}
