// KameraHareketi.cs
// Kameranın "bedenli" hareketleri (#91): yürürken kafanın sallanması, sarsıntı ve kafa lambası
// huzmesinin dönüşlerde hafifçe geride kalması.
//
// Şahan'ın No70 / Phasmophobia karşılaştırması (2026-10-01): kamera bir direğin ucundaki
// göz gibi kayıyordu; yürürken hiç sallanmıyor, fener kafayla kusursuz aynı anda dönüyordu.
// Gerçek bir insanın başı her adımda biraz iner kalkar, ağırlık bir ayaktan öbürüne geçtikçe
// sağa sola kayar; kafaya bağlı bir fener de baş dönünce bir an geride kalır.
//
// ÜÇ PARÇA
//   1. KAFA SALLANMASI: oyuncunun yatay hızı okunur; bir "faz" (döngüde nerede olduğumuz, açı
//      cinsinden) yürünen MESAFEYLE ilerler: iki adımda (1,2 m) bir tam tur. Mesafeye bağlı
//      olduğu için yavaş yürüyen de hızlı yürüyen de adım başına aynı sallanmayı yaşar.
//        dikey  : adım başına bir inip kalkma (iki adımda iki kez), ~1,6 cm
//        yanal  : iki adımda bir sağa bir sola, ~0,8 cm
//        yatma  : yanal kaymayla birlikte çok küçük bir yan yatma (roll), ~0,4°
//      Eğikken yarısı; havadayken ya da kontrolcü kapalıyken (menü, yakın bakış) sıfır.
//      Genlik bir anda değil, yumuşakça açılıp kapanır: durunca kafa "yerine oturur".
//   2. SARSINTI: Sars(guc, sure) ile bir anlık sarsıntı (ör. bir kapı devrilince) ve
//      SurekliSarsinti ile sürekli titreme (GerilimEtkisi gerilime göre ayarlar). İkisi de
//      Perlin gürültüsüyle döndürür (aşağıda tanımı var).
//   3. GECİKEN FENER: kafa lambasının ışığı her kare kameranın dönüşüne yumuşakça yetişir.
//   4. TİTREYEN FENER ELİ (#136): gerilim 2. kademede ışığın yönü 7 Hz'lik Perlin gürültüsüyle ±0,5°
//      titrer, av sürerken ±1,2° (FenerTitremeKurali). Görüş titremez, yalnız huzme; "Kafa sallanması"
//      ayarı kapalıysa bu da kapalı.
//
// Ayardan kapatma: Ayarlar → "Kafa sallanması: Kapalı" (Ayarlar.Sallanma, #92) ya da testler için Kapali
// (static) true ise sallanma ve sarsıntı yok (hareket hassasiyeti olan oyuncular için). Fenerin gecikmesi
// görüşü oynatmadığı için kalır.
//
// ---------- AYNI TAHTAYA İKİ KİŞİ YAZIYOR ----------
// Kameranın yerel konumu ve dönüşü tek bir "tahta". Ona İKİ bileşen yazıyor:
//   - BirinciSahisKontrolcu (Update'te): Bak() dönüşü baştan yazar (yukarı-aşağı bakış),
//     Egil() konumun yüksekliğini eğik/ayakta göz yüksekliğine doğru kaydırır.
//   - Bu bileşen: sallanma ve sarsıntıyı EKLER.
// Ekleyip bırakırsak bir sonraki karede kontrolcü bizim eklediğimizi "gerçek değer" sanar:
// Egil() sallanmış yüksekliği göz yüksekliği diye okur ve her kare biraz kayar, bakış
// kilitliyken (#88 sürükleme) dönüş ofseti de birikir. Çözüm: tahtaya iki kişi yazıyorsa,
// öbürü yazmadan önce herkes KENDİ izini siler.
//   Update (kontrolcüden ÖNCE)      : geçen karenin ofsetini çıkar → tahta temiz.
//   kontrolcünün Update'i           : temiz değerin üstüne kendi işini yapar.
//   LateUpdate (Update'lerin hepsinden SONRA): yeni ofseti ekle ve ne eklediğini not al.
// "Önce" ve "sonra"yı [DefaultExecutionOrder(-50)] sağlar: Unity aynı tür fonksiyonları
// (bütün Update'leri, sonra bütün LateUpdate'leri) bu sayıya göre sıralar; küçük sayı önce
// çalışır. Varsayılan 0'dır, kontrolcü de 0'da: -50 bizim Update'imizi onunkinden önceye alır.
// LateUpdate her zaman bütün Update'lerden sonradır; -50, LateUpdate'imizi de FenerSekmesi'nin
// (0) LateUpdate'inden önceye alır: sekme ışığı, fenerin bu karedeki gecikmiş yönünü okur.

using UnityEngine;

[DefaultExecutionOrder(-50)]
public class KameraHareketi : MonoBehaviour
{
    /// <summary>
    /// True iken kafa sallanması ve sarsıntı yok (testler için; oyuncunun ayarı Ayarlar.Sallanma).
    /// "static": bileşenin kendisine değil sınıfa ait, tek bir değer; sahne yeniden yüklenince de kalır.
    /// </summary>
    public static bool Kapali = false;

    // ---- SALLANMA ÖLÇÜLERİ ----
    private const float DikeyGenlik = 0.016f;     // metre: adım başına inip kalkma
    private const float YanalGenlik = 0.008f;     // metre: sağa sola kayma
    private const float YatmaGenlik = 0.4f;       // derece: yan yatma
    private const float EgikCarpan = 0.5f;        // eğikken sallanma yarıya iner
    private const float TamHiz = 1.2f;            // bu hızda (m/sn) ve üstünde tam genlik; yavaş yürüyüşte az
    private const float GenlikGecisi = 0.15f;     // genliğin açılıp kapanma süresi (saniye)

    // ---- SARSINTI ÖLÇÜLERİ ----
    private const float SarsintiAcisi = 1.2f;     // derece: güç 1'de en büyük sapma
    private const float SarsintiFrekansi = 9f;    // Perlin gürültüsünde saniyede kaç "dalga"

    // ---- FENER GECİKMESİ ----
    private const float FenerYetisme = 12f;       // büyüdükçe fener kafaya daha çabuk yetişir
    private const float FenerSicramaAcisi = 45f;  // bundan büyük fark dönüş değil ışınlanmadır: gecikmeden atla

    private static KameraHareketi ornek;           // Sars() static olduğu için sahnedeki tek örnek

    private BirinciSahisKontrolcu kontrolcu;
    private CharacterController govde;
    private Transform isik;                         // kafa lambasının ışığı (kameranın çocuğu)

    private float faz;                              // sallanma döngüsünde neredeyiz (radyan)
    private float genlik;                           // 0-1: sallanmanın şu anki gücü (yumuşatılmış)

    private float sarsGuc, sarsSure, sarsKalan;    // anlık sarsıntı: başlangıç gücü, süresi, kalan süre
    private float gurultuZamani;                    // Perlin gürültüsünün "okuma noktası"

    // Geçen karede tahtaya EKLEDİĞİMİZ ofsetler (Update'te bunları sileriz).
    private Vector3 sonKonumOfseti = Vector3.zero;
    private Quaternion sonDonusOfseti = Quaternion.identity;

    private Quaternion fenerDonusu;                 // fenerin geçen karedeki dünya dönüşü (titremesiz)
    private float fenerTitremeGenligi;              // #136: şu anki titreme genliği (derece, yumuşatılmış)
    private float fenerGurultuZamani;               // #136: Perlin okuma konumu

    /// <summary>#136: fenerin şu anki titreme genliği (derece); test göstergesi.</summary>
    public float FenerTitremeGenligi { get { return fenerTitremeGenligi; } }
    private bool fenerHazir;                        // fenerDonusu geçerli mi (ilk karede değil)

    /// <summary>
    /// Sürekli titreme miktarı (0-1, güç birimi Sars ile aynı). GerilimEtkisi gerilime göre
    /// her kare yazar; 0 = titreme yok.
    /// </summary>
    public float SurekliSarsinti { get; set; }

    private void Awake()
    {
        ornek = this;
        // GetComponentInParent: önce kendi nesnesine, sonra ebeveynlerine bakar. Kamera
        // oyuncunun çocuğu; kontrolcü ve CharacterController oyuncunun kendisinde.
        kontrolcu = GetComponentInParent<BirinciSahisKontrolcu>();
        govde = GetComponentInParent<CharacterController>();
        // Perlin'i her oyunda başka bir yerden okumaya başla: sarsıntı hep aynı desende olmasın.
        gurultuZamani = Random.Range(0f, 100f);
    }

    private void Start()
    {
        // Feneri Start'ta buluruz, Awake'te değil: KafaLambasi ışığını kendi Awake'inde bulur ve
        // bizim Awake'imiz ondan önce çalışabilir. O zaman IsikYonu ışık yerine KAMERANIN
        // kendisini döndürürdü ve biz kamerayı geciktirmeye kalkardık. Start'a gelindiğinde
        // bütün Awake'ler bitmiştir.
        KafaLambasi lamba = GetComponent<KafaLambasi>();
        if (lamba != null && lamba.IsikYonu != transform) isik = lamba.IsikYonu;
    }

    private void OnDestroy()
    {
        if (ornek == this) ornek = null;
    }

    private void OnDisable()
    {
        // Bileşen kapanırken tahtadaki izimizi bırakmayalım: yoksa son ofset kamerada kalır.
        IziSil();
        fenerHazir = false;
    }

    /// <summary>
    /// Bir anlık sarsıntı: 'guc' 0-1 (1 = en fazla ~1,2°), 'sure' saniyede söner. Sürmekte olan
    /// daha güçlü bir sarsıntıyı zayıf bir yenisi bastırmaz.
    /// </summary>
    public static void Sars(float guc, float sure)
    {
        if (ornek == null || sure <= 0f) return;
        // #124: her kamera sarsıntısı kolda da duyulur (asansör, varış, çarpma): kamera 0,04-0,18 gücünde
        // sarsılır, kol 0,15-0,65; ağır motor önde (gümbürtü), ince motor arkada.
        Titresim.Vur(Mathf.Clamp01(guc * 3.6f), Mathf.Clamp01(guc * 1.8f), sure);
        // Yeni sarsıntı, eskisinin ŞU ANKİ gücünden büyükse onun yerini alır.
        if (guc >= ornek.AnlikSarsinti())
        {
            ornek.sarsGuc = guc;
            ornek.sarsSure = sure;
            ornek.sarsKalan = sure;
        }
    }

    // Anlık sarsıntının şu anki gücü: kalan sürenin karesiyle söner (önce sert, sonra yumuşak biter).
    private float AnlikSarsinti()
    {
        if (sarsKalan <= 0f || sarsSure <= 0f) return 0f;
        float oran = sarsKalan / sarsSure;
        return sarsGuc * oran * oran;
    }

    // ---------- UPDATE: KENDİ İZİNİ SİL ----------
    private void Update()
    {
        IziSil();
    }

    // Geçen karede eklediğimiz konum ve dönüşü geri alır. Dönüşte "çıkarma" yoktur; bir dönüşü
    // geri almak, TERSİYLE (Quaternion.Inverse) çarpmaktır. Ekleme sağdan çarparak yapıldığı
    // için (kameranın kendi eksenlerinde) geri alma da sağdan yapılır.
    private void IziSil()
    {
        transform.localPosition -= sonKonumOfseti;
        transform.localRotation = transform.localRotation * Quaternion.Inverse(sonDonusOfseti);
        sonKonumOfseti = Vector3.zero;
        sonDonusOfseti = Quaternion.identity;
    }

    // ---------- LATEUPDATE: YENİ İZİ EKLE ----------
    private void LateUpdate()
    {
        float dt = Time.deltaTime;   // ölçekli zaman: menüde (timeScale 0) her şey donar

        Vector3 konum = Vector3.zero;
        Vector3 aci = Vector3.zero;   // x = yukarı-aşağı, y = sağa-sola, z = yan yatma (derece)

        if (!Kapali && Ayarlar.Sallanma)
        {
            Sallanma(dt, ref konum, ref aci);
            Sarsinti(dt, ref aci);
        }
        else
        {
            genlik = 0f;
        }

        // Tahtaya ekle ve ne eklediğimizi not al. Konum ebeveynin (oyuncunun) ekseninde: dikey
        // sallanma kameranın baktığı yönden bağımsız olarak hep dünyanın "yukarı"sıdır.
        // Dönüş kameranın KENDİ eksenlerinde (sağdan çarpma): yan yatma bakılan yönün etrafında olur.
        Quaternion donus = Quaternion.Euler(aci);
        transform.localPosition += konum;
        transform.localRotation = transform.localRotation * donus;
        sonKonumOfseti = konum;
        sonDonusOfseti = donus;

        FeneriGeciktir(dt);
    }

    // ---------- KAFA SALLANMASI ----------
    private void Sallanma(float dt, ref Vector3 konum, ref Vector3 aci)
    {
        // Sallanma ne zaman olur: kontrolcü açık (menü / yakın bakış değil) ve ayaklar yerde.
        bool yurur = kontrolcu != null && kontrolcu.enabled && govde != null && govde.enabled && govde.isGrounded;

        // velocity: CharacterController'ın son Move'da gerçekten aldığı hız (duvara yürüyen
        // oyuncunun hızı sıfırdır; istediği değil, gittiği). Yalnızca yatay kısmı: düşmek ya da
        // merdivenden inmek sallanmayı artırmasın.
        float hiz = 0f;
        if (yurur)
        {
            Vector3 v = govde.velocity;
            hiz = new Vector2(v.x, v.z).magnitude;
        }

        // Hedef genlik: hızla orantılı (TamHiz'da 1), eğikken yarısı, yürümüyorsa 0.
        float hedef = yurur ? Mathf.Clamp01(hiz / TamHiz) : 0f;
        if (kontrolcu != null && kontrolcu.Egik) hedef *= EgikCarpan;

        // Yumuşak geçiş: her kare kalan farkın bir kısmı kapanır. 1 - e^(-dt/süre) kare hızından
        // bağımsızdır: 60 FPS'te de 165 FPS'te de aynı sürede açılır.
        genlik = Mathf.Lerp(genlik, hedef, 1f - Mathf.Exp(-dt / GenlikGecisi));

        // Faz mesafeyle ilerler: bir tam tur (2π) = iki adım. Duran oyuncunun fazı ilerlemez;
        // genlik sönerken kafa son durduğu yerden yumuşakça ortaya döner.
        faz += hiz * dt / (2f * AdimOlcer.AdimBoyu) * Mathf.PI * 2f;
        if (faz > Mathf.PI * 2f) faz -= Mathf.PI * 2f;   // sayı büyüyüp hassasiyet kaybetmesin

        // Dikey: -cos(2·faz). Faz 0'da (ayak yere değerken) en alçak; ayak altına gelince en yüksek.
        // Yanal: sin(faz). İki adımda bir tur: en yüksek noktada ağırlık bir ayağın üstündedir.
        // Yatma: yanal kaymayla aynı yöne çok küçük bir eğilme.
        konum.y += -Mathf.Cos(2f * faz) * DikeyGenlik * genlik;
        konum.x += Mathf.Sin(faz) * YanalGenlik * genlik;
        aci.z += -Mathf.Sin(faz) * YatmaGenlik * genlik;
    }

    // ---------- SARSINTI ----------
    // PERLIN GÜRÜLTÜSÜ: Mathf.PerlinNoise(x, y), 0-1 arası, yumuşak dalgalanan "rastgele" bir değer
    // verir. Random.Range'den farkı: yan yana iki nokta birbirine yakın değer verir. Okuma noktası
    // zamanla kaydırılınca kamera titrer ama zıplamaz; el kamerasının titremesi gibi doğal görünür.
    // Her eksen gürültünün başka bir satırından okunur (y = 0, 10, 20), yoksa üçü aynı anda aynı
    // yöne giderdi.
    private void Sarsinti(float dt, ref Vector3 aci)
    {
        if (sarsKalan > 0f) sarsKalan -= dt;
        float guc = Mathf.Clamp01(AnlikSarsinti() + SurekliSarsinti);
        if (guc <= 0f) return;

        gurultuZamani += dt * SarsintiFrekansi;
        float x = Mathf.PerlinNoise(gurultuZamani, 0f) * 2f - 1f;    // -1..1
        float y = Mathf.PerlinNoise(gurultuZamani, 10f) * 2f - 1f;
        float z = Mathf.PerlinNoise(gurultuZamani, 20f) * 2f - 1f;
        aci.x += x * SarsintiAcisi * guc;
        aci.y += y * SarsintiAcisi * guc;
        aci.z += z * SarsintiAcisi * 0.5f * guc;   // yan yatma daha az: çok yatan ufuk mide bulandırır
    }

    // ---------- GECİKEN FENER ----------
    // Işığın DÜNYA dönüşü, geçen karedeki dönüşünden kameranın dönüşüne doğru her kare kalan
    // farkın bir kısmı kadar ilerler (Slerp: iki dönüş arasında küresel ara değer). Kafa hızla
    // dönünce huzme bir an geride kalır, sonra yetişir. Yalnızca dönüş yazılır; ışığın kameraya
    // göre yerel KONUMU (alnın üstü) değişmez.
    private void FeneriGeciktir(float dt)
    {
        if (isik == null) return;
        Quaternion hedef = transform.rotation;

        // Gecikmesiz anlar: inceleme açıkken (eşya kameraya yapışık, huzme tam üstünde olmalı),
        // kontrolcü kapalıyken (menü, yakın bakış, final) ve ilk karede. Çok büyük bir fark da
        // dönüş değil ışınlanmadır (yakalanınca kat başına alınma): fener savrulmasın, atlasın.
        bool atla = !fenerHazir || IncelemeEkrani.Acik || kontrolcu == null || !kontrolcu.enabled
                    || Quaternion.Angle(fenerDonusu, hedef) > FenerSicramaAcisi;

        Quaternion yeni = atla ? hedef : Quaternion.Slerp(fenerDonusu, hedef, 1f - Mathf.Exp(-FenerYetisme * dt));
        isik.rotation = yeni * FenerTitremesi(dt);   // #136: titreme gecikmeye BİNER, gecikmenin hafızasına girmez
        fenerDonusu = yeni;
        fenerHazir = true;
    }

    // ---------- TİTREYEN FENER ELİ (#136) ----------
    // Hedef genlik gerilim kademesinden (Varlik) gelir; genlik bir anda değil 1,5°/sn ile değişir. Perlin
    // gürültüsü (Sarsinti'daki gibi) iki eksende okunur: yukarı-aşağı ve sağa-sola; yan yatma yok (ışık
    // konisi simetrik, yatması görünmez). Kapalı (test) ya da ayar kapalıyken genlik 0'a iner.
    private Quaternion FenerTitremesi(float dt)
    {
        float hedefGenlik = 0f;
        if (!Kapali && Ayarlar.Sallanma && kontrolcu != null && kontrolcu.enabled)
        {
            Varlik v = Varlik.Ornek;
            if (v != null) hedefGenlik = FenerTitremeKurali.Genlik(v.Kademe, v.AvSuruyor);
        }
        fenerTitremeGenligi = Mathf.MoveTowards(fenerTitremeGenligi, hedefGenlik, FenerTitremeKurali.GecisHizi * dt);
        if (fenerTitremeGenligi <= 0.001f) return Quaternion.identity;
        fenerGurultuZamani += dt * FenerTitremeKurali.Frekans;
        float x = (Mathf.PerlinNoise(fenerGurultuZamani, 7.3f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(31.7f, fenerGurultuZamani) - 0.5f) * 2f;
        return Quaternion.Euler(x * fenerTitremeGenligi, y * fenerTitremeGenligi, 0f);
    }
}
