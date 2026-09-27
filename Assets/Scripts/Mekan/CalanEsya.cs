// CalanEsya.cs
// Kendiliğinden çalmaya başlayan bir eşya (#62): 2. katta yaşlı Nuri'nin radyosu.
//
// Katın imza mekaniği "sesle bulma": oyuncu daireye girdikten birkaç saniye sonra eşya
// kendiliğinden çalar; oyuncu hangi odada olduğunu gözle değil kulakla bulur.
// Kulağın yön bulabilmesi için iki şey gerekir:
//   1. Ses 3B'dir: sağdan geliyorsa sağ kulakta, uzaktaysa kısık duyulur (Unity hesaplar).
//   2. Kapalı bir kapı sesi BOĞAR: tizleri yutar, sesi kısar. Unity duvarları ve kapıları
//      bilmez; ses her yere düz bir çizgide gider. Bu yüzden boğmayı kendimiz yapıyoruz:
//      oyuncu eşyanın odasında değilse ve o odanın kapısı kapalıysa, sesi bir alçak geçiren
//      süzgeçten (AudioLowPassFilter) geçirip kısıyoruz. Kapıyı açınca ses bir anda netleşir;
//      oyuncu "burası" der.
//
// Eşya dairenin kalıcı bir mobilyası (her dairede bir radyo var); onu "çalan eşya" yapan bu
// bileşeni kat dosyası ekler (KatZinciriVarligi.sesler, ZincirUygulayici.DekorUygula) ve kat
// değişince söker. Böylece başka bir katta aynı radyo sessiz bir mobilyadır ve yeni bir
// "kendiliğinden çalan şey" (telefon, müzik kutusu) yalnızca veriyle eklenir.
//
// IEtkilesimli: çalarken "KAPAT (E)", susarken "AÇ (E)". Kapatmak zorunlu değil; radyonun
// yerini bulan oyuncu kadrandaki yazıyı okuyabilir.

using UnityEngine;

public class CalanEsya : MonoBehaviour, IEtkilesimli
{
    // Kapalı kapının arkasından duyulan ses: 700 Hz üstü törpülenir, ses %45'e iner.
    // Açık kapıda süzgeç 22 kHz'e çıkar (kulağın duyabildiği her şey geçer: süzgeç yok gibi).
    private const float BogukKesim = 700f, NetKesim = 22000f, BogukSes = 0.45f;
    // Boğukluk kapı açılınca bir anda değil, çeyrek saniyede kalkar (kulakta "tık" olmasın).
    private const float GecisHizi = 4f;

    private AudioSource kaynak;
    private AudioLowPassFilter suzgec;
    private Light isik;                 // kadranın sıcak ışığı: karanlık odada eşyayı gösterir
    private IcKapi odaKapisi;           // eşyanın odasının kapısı (koridora açılan)
    private string oda;                 // eşyanın odası (DairePlani adı)
    private DaireEksenleri eksen;       // oyuncunun hangi odada olduğunu bulmak için
    private Transform oyuncu;
    private float gecikme;              // oyuncu daireye girdikten kaç saniye sonra çalar
    private float ses;                  // tam (boğulmamış) ses düzeyi
    private float iceridekiSure;        // oyuncunun dairede geçirdiği süre
    private bool kendiAcildi;           // kendiliğinden açılma bir kez olur
    private float bogukluk = 1f;        // 1: tam boğuk, 0: net (yumuşak geçiş için)
    private AudioClip dugmeSesi;

    /// <summary>Çalıyor mu?</summary>
    public bool Caliyor { get { return kaynak != null && kaynak.isPlaying; } }

    /// <summary>
    /// Kat dosyasından kurulur. klip: döngüde çalan ses. oda: eşyanın bulunduğu oda;
    /// kapi: o odanın kapısı (yoksa boğma yapılmaz).
    /// </summary>
    public void Hazirla(AudioClip klip, float gecikmeSn, float sesDuzeyi, string odaAdi, IcKapi kapi, DaireEksenleri eksenler)
    {
        gecikme = gecikmeSn;
        ses = sesDuzeyi;
        oda = odaAdi;
        odaKapisi = kapi;
        eksen = eksenler;

        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.clip = klip;
        kaynak.loop = true;
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 1f;                          // tamamen 3B
        kaynak.rolloffMode = AudioRolloffMode.Logarithmic; // gerçek dünyadaki gibi: yakında hızlı azalır
        kaynak.minDistance = 1.0f;                         // 1 m'ye kadar tam ses
        kaynak.maxDistance = 16f;                          // dairenin öbür ucundan da duyulsun
        kaynak.dopplerLevel = 0f;                          // yürürken perde kaymasın
        kaynak.volume = ses * BogukSes;

        // AudioLowPassFilter aynı nesnedeki AudioSource'un sesini süzer; sırası önemli değil.
        suzgec = gameObject.AddComponent<AudioLowPassFilter>();
        suzgec.cutoffFrequency = BogukKesim;

        // Kadran ışığı: küçük ve sıcak, yalnızca çalarken yanar. Radyonun 35 cm önünde ve biraz
        // üstünde: 12 cm önündeyken radyonun yüzünü beyaza patlatıyordu (oynatma modu görüntüsü);
        // uzaktan ve kısık, radyoyu ve komodini hafifçe boyar, odaya "bir şey açık" duygusu verir.
        GameObject g = new GameObject("CalanEsya_Isik");
        g.transform.SetParent(transform, false);
        g.transform.localPosition = new Vector3(-0.085f, 0.22f, 0.35f);
        isik = g.AddComponent<Light>();
        isik.type = LightType.Point;
        isik.color = new Color(1f, 0.72f, 0.38f);
        isik.intensity = 0.12f;
        isik.range = 1.3f;
        isik.shadows = LightShadows.None;                  // tek bir küçük ışık: gölgeye gerek yok
        isik.enabled = false;

        dugmeSesi = Resources.Load<AudioClip>("Sesler/Radyo_Dugme");
        BirinciSahisKontrolcu k = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (k != null) oyuncu = k.transform;
    }

    /// <summary>Kat değişirken: susar, eklediklerini söker (bileşenin kendisini ZincirUygulayici siler).</summary>
    public void Sifirla()
    {
        if (kaynak != null) Destroy(kaynak);
        if (suzgec != null) Destroy(suzgec);
        if (isik != null) Destroy(isik.gameObject);
    }

    private void Update()
    {
        if (kaynak == null || oyuncu == null || eksen == null) return;

        // Oyuncu nerede? Plan eksenlerinde (u, v) ve hangi odada.
        eksen.PlanNoktasi(oyuncu.position, out float u, out float v);
        string oyuncununOdasi = DairePlani.NoktaninOdasi(u, v);

        // 1. Kendiliğinden açılma: oyuncu dairenin içindeyken süre işler.
        if (!kendiAcildi)
        {
            if (oyuncununOdasi != null) iceridekiSure += Time.deltaTime;
            if (iceridekiSure >= gecikme)
            {
                kendiAcildi = true;
                Ac(true);
                Debug.Log($"[SES] {name} kendiliğinden çalmaya başladı ({gecikme:0.0} sn).");
            }
        }

        // 2. Boğma: oyuncu eşyanın odasında değilse ve odanın kapısı kapalıysa boğuk.
        bool boguk = oyuncununOdasi != oda && odaKapisi != null && !odaKapisi.Acik;
        // MoveTowards: değeri hedefe doğru en fazla verilen adım kadar yaklaştırır.
        bogukluk = Mathf.MoveTowards(bogukluk, boguk ? 1f : 0f, GecisHizi * Time.deltaTime);
        // Kesim frekansı logaritmik ölçekte geçer: kulak frekansları oranla duyar
        // (700'den 1400'e çıkmak, 11000'den 22000'e çıkmak kadar fark edilir).
        suzgec.cutoffFrequency = Mathf.Exp(Mathf.Lerp(Mathf.Log(NetKesim), Mathf.Log(BogukKesim), bogukluk));
        kaynak.volume = ses * Mathf.Lerp(1f, BogukSes, bogukluk);
    }

    private void Ac(bool ac)
    {
        if (ac) kaynak.Play(); else kaynak.Stop();
        isik.enabled = ac;
        if (dugmeSesi != null) AudioSource.PlayClipAtPoint(dugmeSesi, transform.position, 0.6f);
    }

    // ---------- IEtkilesimli ----------
    public string Ipucu { get { return Dil.Getir(Caliyor ? "ipucu.kapat" : "ipucu.ac"); } }

    public bool EtkilesimeAcik { get { return kaynak != null; } }

    public void Etkilesim(GameObject oyuncuNesnesi)
    {
        // Oyuncu radyoyu kendisi açarsa kendiliğinden açılma artık olmaz.
        kendiAcildi = true;
        Ac(!Caliyor);
    }
}
