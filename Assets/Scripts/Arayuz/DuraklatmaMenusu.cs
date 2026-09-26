// DuraklatmaMenusu.cs
// Esc (ya da oyun kolunun Start tuşu) ile açılan duraklatma menüsü (#15, #27, #29, #30).
// Ana sayfa: Devam, Ayarlar, Görüntü, Dil, Oyundan Çık.
// Ayarlar sayfası: hassasiyet, ses, görüş açısı, fareyi ters çevir, varsayılana dön, geri.
// Görüntü sayfası: çözünürlük, ekran modu, V-Sync, grafik kalitesi, parlaklık, varsayılana dön, geri.
//
// Menü açılınca:
//   - oyunun saati durur (Time.timeScale = 0),
//   - sesler duraklar (AudioListener.pause),
//   - oyuncu yürüyemez, bakamaz, etkileşemez,
//   - fare serbest kalır ve görünür olur.
// Kapanınca hepsi TAM OLARAK ESKİ HÂLİNE döner; "açık/kapalı"ya değil, menü
// açılmadan önceki değere. Ceza karanlığında ses zaten kapalıysa (KatYoneticisi
// kapatmıştı) menüden dönünce ses açılmamalı; aksi hâlde karanlığın sessizliği bozulur.
//
// Menü NE ZAMAN açılamaz: oyuncunun kontrolü zaten kapalıyken. Bu iki durumu kapsar:
// yönetici notu açıkken (Esc orada notu kapatır) ve final sırasında (kamera dışarıda).
//
// ÜÇ YOLLA KULLANILIR (#29)
//   Fare       → düğmelere tıkla, kaydırıcıları sürükle.
//   Klavye     → yön tuşları / W-S gezinir, A-D ya da sol-sağ kaydırıcıyı değiştirir,
//                Enter / Space seçer, Esc geri gider.
//   Oyun kolu  → d-pad / sol çubuk gezinir, sol-sağ kaydırıcıyı değiştirir,
//                A seçer, B geri gider, Start menüyü kapatır.
// Seçili öğenin etrafında bir çerçeve durur; hangi öğede olunduğu hep görünür.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuraklatmaMenusu : MonoBehaviour
{
    private bool acik;

    // Bir önceki karenin sonunda oyuncu oynayabiliyor muydu? Esc'yi bununla
    // karşılaştırıyoruz. Neden bir önceki kare: notu kapatan Esc basışı, aynı karede
    // oyuncunun kontrolünü geri açıyor. Bu karenin durumuna bakarsak aynı basış
    // menüyü de açardı; not kapanır kapanmaz menü açılırdı.
    private bool oncekiKareOynanabilir;

    private BirinciSahisKontrolcu kontrolcu;

    // Menünün kendisinin kapattığı bileşenler. Geri açarken yalnızca bunları açarız;
    // başka biri (not, final) kapattıysa ona dokunmayız.
    private readonly List<Behaviour> kapattiklarim = new List<Behaviour>();

    // Menü açılmadan önceki değerler; kapanınca bunlara dönülür.
    private float eskiZamanOlcegi = 1f;
    private bool eskiSesDuraklamasi;
    private CursorLockMode eskiFareKilidi;
    private bool eskiFareGorunur;

    // Menünün dört sayfası var: ana sayfa, ayarlar (#27), görüntü (#30) ve anomali defteri (#45).
    private const int SayfaAna = 0, SayfaAyarlar = 1, SayfaGoruntu = 2, SayfaDefter = 3;
    private int sayfa = SayfaAna;

    // ---------- KLAVYE / KOL İLE GEZİNME (#29) ----------
    // Öğelerin sırası (yukarıdan aşağı). Numara, gezginin "Secili" değeriyle eşleşir.
    private const int AnaDevam = 0, AnaAyarlar = 1, AnaGoruntu = 2, AnaDefter = 3, AnaDil = 4, AnaCik = 5, AnaOgeSayisi = 6;
    private const int AyarHassasiyet = 0, AyarSes = 1, AyarGorus = 2, AyarTers = 3,
                      AyarVarsayilan = 4, AyarGeri = 5, AyarOgeSayisi = 6;
    private const int GorCozunurluk = 0, GorEkran = 1, GorVSync = 2, GorKalite = 3,
                      GorParlaklik = 4, GorVarsayilan = 5, GorGeri = 6, GorOgeSayisi = 7;

    // Kalite seviyelerinin tablodaki adları: [0] Düşük, [1] Orta, [2] Yüksek.
    private static readonly string[] KaliteAnahtarlari = { "ayar.dusuk", "ayar.orta", "ayar.yuksek" };

    // Görüntü sayfası açılınca ekranın desteklediği çözünürlüklerden kurulan liste (#30).
    // Aynı genişlik x yükseklik, farklı yenileme hızlarıyla birden çok kez gelir; tekler tutulur.
    private int[] cozunurlukGenislik = new int[0], cozunurlukYukseklik = new int[0];
    private int cozunurlukSirasi;

    private readonly MenuGezgini gezgin = new MenuGezgini(AnaOgeSayisi);
    private readonly TekrarZamanlayici dikeyTekrar = new TekrarZamanlayici();
    private readonly TekrarZamanlayici yatayTekrar = new TekrarZamanlayici();

    // Projenin hazır "UI" eylemleri: Navigate (yön tuşları, W-S-A-D, d-pad, sol çubuk
    // hepsi birden) ve Submit (Enter, kolun A tuşu). Bugüne kadar hiç kullanılmıyordu.
    private InputAction gezinEylemi, onayEylemi;

    // Devam, basıldığı karede değil karenin sonunda (LateUpdate) yapılır. Neden:
    // kolun A tuşu hem menüde "seç" hem oyunda "etkileşim". A ile "Devam"a basılınca
    // oyuncunun etkileşimi o karede geri açılırsa, aynı basışı görüp oyuncunun baktığı
    // poşeti toplayabilirdi. Sonraki karede o basış artık "yeni" sayılmıyor.
    private bool devamIstendi;

    // Ses seviyesi değişince çalan örnek ses (#27). Menü açıkken bütün sesler
    // duraklatılıyor (AudioListener.pause); oyuncu kaydırıcıyı oynatırken farkı
    // duyamazdı. Bu kaynak ignoreListenerPause ile o duraklatmanın DIŞINDA kalıyor,
    // ama ortak ses seviyesinden (AudioListener.volume) etkileniyor: tam olarak
    // oyuncunun seçtiği seviyede çalıyor.
    private AudioSource ornekSes;
    private float sonOrnekZamani = -1f;

    // ---------- BAŞLIK EKRANI (#32) ----------
    // Oyun açılınca menü "başlık modunda" başlar: ana sayfada "Devam" yerine "Başla",
    // üstte oyunun adı, arkada merdiven boşluğundan yukarı bakan yavaşça dönen bir
    // kamera. Ayarlar / Görüntü / Dil sayfaları Esc menüsündekilerin aynısı.
    // Ayrı bir sahne değil: final sonrası sahne baştan yüklenince oyun kendiliğinden
    // yine buraya döner.
    [Tooltip("Oyun başlık ekranıyla mı açılsın? Kapatılırsa doğrudan kapıcı odasında başlar (test için).")]
    [SerializeField] private bool baslikEkraniGoster = true;

    /// <summary>Şu an başlık ekranında mıyız (test göstergesi bunu görünce çizilmez).</summary>
    public static bool BaslikEkraninda { get; private set; }

    private bool baslikta;          // başlık ekranı açık
    private bool geciste;           // "Başla"ya basıldı, kararma sürüyor
    private float karartma;         // 0 = açık, 1 = simsiyah (OnGUI çizer)
    private Camera baslikKamerasi, oyuncuKamerasi;
    private AudioListener oyuncuKulagi;
    private float baslikDonusu;     // başlık kamerasının dik eksen etrafındaki açısı

    // Merdiven gözünün ortası (x = 0, gözün z aralığı 0…2,16), 1. katın biraz üstü.
    // Kamera neredeyse dik yukarı bakar; katlar kat kat dönerek karanlığa yükselir.
    // Birkaç açı denenip seçildi. Dış cephe bilerek kullanılmadı: bina dışarıdan 9
    // katlı görünür ve finaldeki sürprizi (#16) başlık ekranında bozardı.
    private static readonly Vector3 BaslikKameraKonumu = new Vector3(0f, 0.3f, 1.08f);
    private const float BaslikKameraEgimi = -85f;      // derece; eksi = yukarı
    private const float BaslikKameraDonusHizi = 2.5f;  // derece/saniye; sarmal gibi yavaş döner

    private void Start()
    {
        kontrolcu = FindAnyObjectByType<BirinciSahisKontrolcu>();

        ornekSes = gameObject.AddComponent<AudioSource>();
        ornekSes.playOnAwake = false;
        ornekSes.spatialBlend = 0f;              // 2B: yeri yok, "kulağın içinde"
        ornekSes.ignoreListenerPause = true;     // menü duraklatması bunu susturmasın
        ornekSes.clip = Resources.Load<AudioClip>("Sesler/Poset_Hisirti");

        if (InputSystem.actions != null)
        {
            gezinEylemi = InputSystem.actions.FindAction("UI/Navigate");
            onayEylemi = InputSystem.actions.FindAction("UI/Submit");
        }
        // Enable: eylemi dinlemeye başlat (kontrolcünün kendi eylemleri için yaptığı gibi).
        gezinEylemi?.Enable();
        onayEylemi?.Enable();

        if (baslikEkraniGoster) BaslikEkraniniAc();
    }

    private void Update()
    {
        if (baslikta) BaslikKamerasiniDondur();
        if (geciste) return;                            // kararma sırasında girdi okuma

        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool baslat = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        bool geri = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (!acik)
        {
            if ((esc || baslat) && oncekiKareOynanabilir) Ac();
            return;
        }

        if (devamIstendi) return;                       // kapanmak üzere; başka girdi okuma

        // Start her sayfadan menüyü kapatır. Başlık ekranında kapatılacak bir menü yok
        // (arkada devam edilecek bir oyun yok), o yüzden orada bir şey yapmaz.
        if (baslat && !baslikta) { DevamIste(); return; }
        if (esc || geri)                                // Esc / B: bir geri git
        {
            if (sayfa != SayfaAna) SayfaAc(SayfaAna);
            else if (!baslikta) DevamIste();            // başlık ekranının ana sayfasında geri yok
            return;
        }

        // Yön: yukarı/aşağı seçimi kaydırır, sol/sağ kaydırıcıyı değiştirir. Hangi eksen
        // baskınsa o sayılır; çubuğu çapraz iten oyuncu iki işi birden yapmasın.
        Vector2 yon = gezinEylemi != null ? gezinEylemi.ReadValue<Vector2>() : Vector2.zero;

        // ReadValue, KARENİN SONUNDAKİ durumu verir. Tuşa basılıp aynı karenin içinde
        // bırakılırsa (çok hızlı bir dokunuş, düşük kare hızı) değer 0 okunur ve dokunuş
        // kaybolur. Bunu yakalamak için d-pad ve klavye yön tuşlarının "bu karede
        // basıldı mı?" bilgisine de bakıyoruz; basıldıysa o yönü değere yazıyoruz.
        // Test sırasında kısa basışların hiç işlemediği görülünce eklendi.
        if (YonBasildi(g => g.dpad.up, Key.UpArrow, Key.W)) yon = Vector2.up;
        else if (YonBasildi(g => g.dpad.down, Key.DownArrow, Key.S)) yon = Vector2.down;
        else if (YonBasildi(g => g.dpad.left, Key.LeftArrow, Key.A)) yon = Vector2.left;
        else if (YonBasildi(g => g.dpad.right, Key.RightArrow, Key.D)) yon = Vector2.right;

        bool dikey = Mathf.Abs(yon.y) > 0.5f && Mathf.Abs(yon.y) >= Mathf.Abs(yon.x);
        bool yatay = Mathf.Abs(yon.x) > 0.5f && Mathf.Abs(yon.x) > Mathf.Abs(yon.y);

        // unscaledTime: oyun saati durmuşken de ilerleyen gerçek zaman.
        // Defter sayfasında yukarı/aşağı listeyi kaydırır (seçilecek öğe yok, yalnızca "Geri").
        if (dikeyTekrar.Guncelle(dikey, Time.unscaledTime))
        {
            if (sayfa == SayfaDefter) DefteriKaydir(yon.y > 0f ? -1 : +1);
            else gezgin.Tasi(yon.y > 0f ? -1 : +1);   // yukarı = listede geri
        }
        if (yatayTekrar.Guncelle(yatay, Time.unscaledTime)) Kaydir(yon.x > 0f ? +1 : -1);

        bool onay = (onayEylemi != null && onayEylemi.WasPressedThisFrame())
                    || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
        if (onay) SeciliyiCalistir();
    }

    // LateUpdate: bütün Update'lerden sonra çalışır. Ertelenen "Devam"ı yapar ve
    // karenin SON durumunu kaydeder.
    private void LateUpdate()
    {
        if (devamIstendi)
        {
            devamIstendi = false;
            Devam();
        }
        oncekiKareOynanabilir = !acik && kontrolcu != null && kontrolcu.enabled;

        // Başlık ekranında fare hep serbest ve görünür kalsın. Oyuncu kontrolcüsü açılışta
        // fareyi kilitliyor; Start'ların hangi sırayla çalışacağı belli olmadığı için
        // kilidi her karenin sonunda geri açıyoruz.
        if (baslikta)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ---------- BAŞLIK EKRANI (#32) ----------
    private void BaslikEkraniniAc()
    {
        baslikta = true;
        BaslikEkraninda = true;
        acik = true;
        SayfaAc(SayfaAna);
        gezgin.Sec(AnaDevam);   // "Başla" seçili

        // Oyuncuyu dondur: yürümesin, bakmasın, etkileşmesin. Oyun saati durdurulmuyor;
        // ortam sesleri çalsın, bina başlık ekranında da "yaşasın".
        kapattiklarim.Clear();
        if (kontrolcu != null)
        {
            KapatVeHatirla(kontrolcu);
            KapatVeHatirla(kontrolcu.GetComponent<Etkilesimci>());
            oyuncuKamerasi = kontrolcu.GetComponentInChildren<Camera>();
        }

        // Oyuncunun kamerasını ve kulağını kapat; ikisini başlık kamerası üstlenir.
        // Sahnede aynı anda iki AudioListener olursa Unity uyarı verir ve ses karışır.
        if (oyuncuKamerasi != null)
        {
            oyuncuKamerasi.enabled = false;
            oyuncuKulagi = oyuncuKamerasi.GetComponent<AudioListener>();
            if (oyuncuKulagi != null) oyuncuKulagi.enabled = false;
        }

        // Başlık kamerası: oyun sırasında kodla kuruluyor, sahneye kaydedilmiyor (yalnızca
        // başlık ekranı boyunca yaşıyor). Gece efektleri görünsün diye URP'nin kamera
        // verisi eklenip post-processing açılıyor (AtmosferKurucu'nun oyuncu kamerasına
        // yaptığının aynısı).
        GameObject nesne = new GameObject("Baslik_Kamerasi");
        baslikKamerasi = nesne.AddComponent<Camera>();
        baslikKamerasi.fieldOfView = 70f;
        baslikKamerasi.nearClipPlane = 0.05f;
        var veri = nesne.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        veri.renderPostProcessing = true;
        veri.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        nesne.AddComponent<AudioListener>();
        // #51: bina karanlık; başlık ekranında merdiven boşluğu okunsun diye kameraya küçük,
        // sıcak bir ışık (yerde duran bir el feneri gibi). Kamerayla birlikte silinir.
        GameObject baslikIsigi = new GameObject("Baslik_Isigi");
        baslikIsigi.transform.SetParent(nesne.transform, false);
        Light bi = baslikIsigi.AddComponent<Light>();
        bi.type = LightType.Point;
        bi.range = 7f;
        bi.intensity = 1.2f;
        bi.color = new Color(1f, 0.88f, 0.70f);
        bi.shadows = LightShadows.None;
        BaslikKamerasiniDondur();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void BaslikKamerasiniDondur()
    {
        if (baslikKamerasi == null) return;
        baslikDonusu += BaslikKameraDonusHizi * Time.unscaledDeltaTime;
        // Euler(eğim, dönüş, 0): önce dik eksen etrafında dön, sonra yukarı bak.
        // Kamera yukarı bakarken dik eksen etrafında dönmek görüntüyü ortası etrafında
        // çevirir: merdivenler sarmal gibi döner.
        baslikKamerasi.transform.SetPositionAndRotation(BaslikKameraKonumu,
            Quaternion.Euler(BaslikKameraEgimi, baslikDonusu, 0f));
    }

    private void Basla()
    {
        if (geciste) return;
        StartCoroutine(BaslaGecisi());
    }

    // Ekran kararır → başlık kamerası kalkar, oyuncu kapıcı odasında uyanır → ekran açılır.
    // Coroutine: "yield return" satırında durur, sonraki karede kaldığı yerden devam eder
    // (FinalSahnesi'ndeki gibi). Süreler gerçek zamanla (unscaled) ölçülüyor.
    private System.Collections.IEnumerator BaslaGecisi()
    {
        geciste = true;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.8f)
        {
            karartma = t;
            yield return null;
        }
        karartma = 1f;

        // Ekran simsiyahken geçişi yap: oyuncu hiçbir ara kareyi görmez.
        if (baslikKamerasi != null) Destroy(baslikKamerasi.gameObject);
        if (oyuncuKamerasi != null) oyuncuKamerasi.enabled = true;
        if (oyuncuKulagi != null) oyuncuKulagi.enabled = true;

        baslikta = false;
        BaslikEkraninda = false;
        acik = false;
        foreach (Behaviour b in kapattiklarim) if (b != null) b.enabled = true;
        kapattiklarim.Clear();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return null;   // bir kare simsiyah kalsın: oyuncunun kamerası yerine otursun

        for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime / 0.8f)
        {
            karartma = t;
            yield return null;
        }
        karartma = 0f;
        geciste = false;
    }

    // Static değer Play'ler arasında kalmasın (Dil.cs'teki gibi).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla()
    {
        BaslikEkraninda = false;
    }

    // Bir yön bu karede basıldı mı: oyun kolunun d-pad yönü ya da iki klavye tuşundan biri.
    // System.Func<Gamepad, ButtonControl>: "koldan bir tuş seçen" kısa bir fonksiyon;
    // çağıran "g => g.dpad.up" gibi hangi tuşu istediğini söylüyor.
    private static bool YonBasildi(System.Func<Gamepad, UnityEngine.InputSystem.Controls.ButtonControl> kolTusu, Key tus1, Key tus2)
    {
        if (Gamepad.current != null && kolTusu(Gamepad.current).wasPressedThisFrame) return true;
        Keyboard k = Keyboard.current;
        return k != null && (k[tus1].wasPressedThisFrame || k[tus2].wasPressedThisFrame);
    }

    // ---------- SEÇİLİ ÖĞE ----------
    // Enter / Space / A basılınca seçili öğenin işini yapar. Fare tıklaması aynı işleri
    // OnGUI'deki düğmelerden yapıyor; ikisi aynı fonksiyonları çağırıyor.
    private void SeciliyiCalistir()
    {
        if (sayfa == SayfaAna)
        {
            switch (gezgin.Secili)
            {
                case AnaDevam: if (baslikta) Basla(); else DevamIste(); break;
                case AnaAyarlar: SayfaAc(SayfaAyarlar); break;
                case AnaGoruntu: SayfaAc(SayfaGoruntu); break;
                case AnaDefter: SayfaAc(SayfaDefter); break;
                case AnaDil: Dil.Sonraki(); break;
                case AnaCik: OyundanCik(); break;
            }
            return;
        }

        if (sayfa == SayfaDefter) { SayfaAc(SayfaAna); return; }   // defterde tek düğme: Geri

        if (sayfa == SayfaAyarlar)
        {
            switch (gezgin.Secili)
            {
                case AyarTers: Ayarlar.Ters = !Ayarlar.Ters; break;
                case AyarVarsayilan: Ayarlar.VarsayilanaDon(); break;
                case AyarGeri: SayfaAc(SayfaAna); break;
                // Kaydırıcılarda onay tuşunun işi yok; sol/sağ ile değiştirilir.
            }
            return;
        }

        // Görüntü sayfası: düğmelerde onay, durumu bir ileri alır (fare tıklaması gibi).
        switch (gezgin.Secili)
        {
            case GorCozunurluk: CozunurlukDegistir(+1); break;
            case GorEkran: Ayarlar.TamEkran = !Ayarlar.TamEkran; break;
            case GorVSync: Ayarlar.VSync = !Ayarlar.VSync; break;
            case GorKalite: Ayarlar.Kalite = (Ayarlar.Kalite + 1) % KaliteAnahtarlari.Length; break;
            case GorVarsayilan: Ayarlar.GoruntuVarsayilanaDon(); CozunurlukListesiniKur(); break;
            case GorGeri: SayfaAc(SayfaAna); break;
        }
    }

    // Sol (-1) / sağ (+1): seçili kaydırıcıyı bir adım oynatır. Açık/kapalı ve dil
    // düğmelerinde de durumu değiştirir; sol/sağ oyuncuya orada da doğal gelir.
    private void Kaydir(int yon)
    {
        if (sayfa == SayfaAna)
        {
            if (gezgin.Secili == AnaDil) Dil.Sonraki();
            return;
        }

        if (sayfa == SayfaGoruntu)
        {
            switch (gezgin.Secili)
            {
                case GorCozunurluk: CozunurlukDegistir(yon); break;
                case GorEkran: Ayarlar.TamEkran = !Ayarlar.TamEkran; break;
                case GorVSync: Ayarlar.VSync = !Ayarlar.VSync; break;
                // Kalite sol/sağ ile bir aşağı / yukarı; uçlarda durur (dolaşmaz: "Düşük"ten
                // bir sol daha "Yüksek"e atlasa oyuncu şaşırırdı). Sınırı Ayarlar koyuyor.
                case GorKalite: Ayarlar.Kalite = Ayarlar.Kalite + yon; break;
                case GorParlaklik: Ayarlar.Parlaklik = Mathf.Round((Ayarlar.Parlaklik + 0.1f * yon) * 10f) / 10f; break;
            }
            return;
        }

        switch (gezgin.Secili)
        {
            case AyarHassasiyet: Ayarlar.Hassasiyet = Mathf.Round((Ayarlar.Hassasiyet + 0.05f * yon) * 20f) / 20f; break;
            case AyarSes: Ayarlar.Ses = Mathf.Round((Ayarlar.Ses + 0.05f * yon) * 20f) / 20f; OrnekSesCal(); break;
            case AyarGorus: Ayarlar.Gorus = Mathf.Round(Ayarlar.Gorus + yon); break;
            case AyarTers: Ayarlar.Ters = !Ayarlar.Ters; break;
        }
    }

    private void SayfaAc(int yeni)
    {
        int onceki = sayfa;
        sayfa = yeni;
        gezgin.OgeSayisiniAyarla(yeni == SayfaAyarlar ? AyarOgeSayisi : yeni == SayfaGoruntu ? GorOgeSayisi
                                 : yeni == SayfaDefter ? 1 : AnaOgeSayisi);

        // Bir sayfaya girince ilk öğe seçili. Ana sayfaya dönünce geldiği sayfanın düğmesi
        // seçili kalsın (oyuncu nereden geldiğini görür, tekrar girmek isterse tek tuş yeter).
        if (yeni == SayfaAna) gezgin.Sec(onceki == SayfaGoruntu ? AnaGoruntu : onceki == SayfaDefter ? AnaDefter : AnaAyarlar);
        else gezgin.Sec(0);
        if (yeni == SayfaDefter) DefteriHazirla();

        if (yeni == SayfaGoruntu) CozunurlukListesiniKur();
    }

    // ---------- ÇÖZÜNÜRLÜK LİSTESİ (#30) ----------
    // Screen.resolutions: ekranın (monitör + ekran kartı) desteklediği çözünürlükler.
    // Aynı boyut her yenileme hızı için (60 Hz, 144 Hz...) ayrı gelir; tekleri alınır.
    private void CozunurlukListesiniKur()
    {
        var genislik = new List<int>();
        var yukseklik = new List<int>();
        foreach (Resolution r in Screen.resolutions)
        {
            bool varMi = false;
            for (int i = 0; i < genislik.Count; i++)
                if (genislik[i] == r.width && yukseklik[i] == r.height) { varMi = true; break; }
            if (!varMi) { genislik.Add(r.width); yukseklik.Add(r.height); }
        }
        // Liste boş gelirse (bazı sürücüler, Editor) en azından şu anki boyut seçilebilsin.
        if (genislik.Count == 0) { genislik.Add(Screen.width); yukseklik.Add(Screen.height); }

        cozunurlukGenislik = genislik.ToArray();
        cozunurlukYukseklik = yukseklik.ToArray();

        // Kayıtlı seçim yoksa (0) ekranın kendi çözünürlüğü kabul edilir.
        int g = Ayarlar.CozunurlukGenislik > 0 ? Ayarlar.CozunurlukGenislik : Screen.currentResolution.width;
        int y = Ayarlar.CozunurlukYukseklik > 0 ? Ayarlar.CozunurlukYukseklik : Screen.currentResolution.height;
        cozunurlukSirasi = Mathf.Max(0, AyarKurallari.CozunurlukSec(cozunurlukGenislik, cozunurlukYukseklik, g, y));
    }

    private void CozunurlukDegistir(int yon)
    {
        int n = cozunurlukGenislik.Length;
        if (n == 0) return;
        cozunurlukSirasi = ((cozunurlukSirasi + yon) % n + n) % n;   // uçlarda dolaşır
        Ayarlar.CozunurlukAyarla(cozunurlukGenislik[cozunurlukSirasi], cozunurlukYukseklik[cozunurlukSirasi]);
    }

    private void OrnekSesCal()
    {
        // En fazla 0,2 sn'de bir: sürüklerken ya da tuşa basılı tutarken her seferinde
        // çalsaydı sesler üst üste binip cızırtıya dönerdi.
        if (ornekSes != null && ornekSes.clip != null && Time.unscaledTime - sonOrnekZamani > 0.2f)
        {
            ornekSes.PlayOneShot(ornekSes.clip, 0.8f);
            sonOrnekZamani = Time.unscaledTime;
        }
    }

    // ---------- AÇ / KAPAT ----------
    private void Ac()
    {
        acik = true;
        SayfaAc(SayfaAna);
        gezgin.Sec(AnaDevam);   // menü her zaman ana sayfada, "Devam" seçili açılır

        // Time.timeScale: oyun saatinin hızı. 1 = normal, 0 = durmuş. Sıfırken
        // Time.deltaTime 0 olur: hareket, fizik, WaitForSeconds bekleyen ceza ve final
        // sayaçları durur. Update yine çalışır; menü bu yüzden tepki vermeye devam eder.
        eskiZamanOlcegi = Time.timeScale;
        Time.timeScale = 0f;

        eskiSesDuraklamasi = AudioListener.pause;
        AudioListener.pause = true;

        // Oyuncunun girişi okuyan bileşenlerini kapat (açık olanları).
        kapattiklarim.Clear();
        if (kontrolcu != null)
        {
            KapatVeHatirla(kontrolcu);
            KapatVeHatirla(kontrolcu.GetComponent<Etkilesimci>());
        }

        eskiFareKilidi = Cursor.lockState;
        eskiFareGorunur = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DevamIste()
    {
        devamIstendi = true;   // asıl iş LateUpdate'te (nedeni devamIstendi'nin yanında)
    }

    private void Devam()
    {
        acik = false;

        Time.timeScale = eskiZamanOlcegi;
        AudioListener.pause = eskiSesDuraklamasi;

        foreach (Behaviour b in kapattiklarim) if (b != null) b.enabled = true;
        kapattiklarim.Clear();

        Cursor.lockState = eskiFareKilidi;
        Cursor.visible = eskiFareGorunur;
    }

    private void KapatVeHatirla(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        kapattiklarim.Add(b);
    }

    // ---------- PENCERE ODAĞI (#26) ----------
    // Oyuncu alt-tab ile başka pencereye geçerse oyun kendiliğinden durup menüyü açar.
    // Korku oyununda ekran başında yokken cezanın ya da bir anomalinin işlemesi can
    // sıkıcı olur; dönünce oyuncu kaldığı anı menünün arkasında bekler bulur.
    //
    // Editor'de varsayılan olarak KAPALI: Editor'de odak, Console'a veya Inspector'a
    // her tıklandığında kaybolur; oyun geliştirirken sürekli kendi kendine dururdu.
    // Editor'de denemek için bu kutuyu işaretleyin.
    [Tooltip("Editor'de de pencere odağı kaybolunca menü açılsın mı? Build'de her zaman açılır.")]
    [SerializeField] private bool editordeOdakKaybindaDuraklat = false;

    // OnApplicationFocus: Unity, oyun penceresi odağı kaybettiğinde (false) ve geri
    // kazandığında (true) bu fonksiyonu kendiliğinden çağırır.
    private void OnApplicationFocus(bool odakta)
    {
        if (odakta) return;                                              // geri dönüşte bir şey yapma; menü açık bekler
        if (Application.isEditor && !editordeOdakKaybindaDuraklat) return;
        if (!acik && oncekiKareOynanabilir) Ac();                        // not/final sırasında açılmaz (Esc ile aynı kural)
    }

    // Menü açıkken sahne kapanırsa (final sonrası yeniden yükleme, Play'den çıkış)
    // saat durmuş kalmasın: Time.timeScale sahneden sahneye taşınan bir ayar.
    private void OnDisable()
    {
        if (acik) Devam();
    }

    private void OyundanCik()
    {
        // #if: derleme anında seçim. UNITY_EDITOR yalnızca Editor'de tanımlı; bu satır
        // oyunun kendisine (build) hiç girmez. Editor'de Application.Quit bir şey
        // yapmadığı için Play modundan çıkıyoruz.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- ÇİZİM ----------
    // Not ve jenerik gibi OnGUI ile çiziliyor. GUI.Button bir düğme çizer ve o karede
    // tıklandıysa true döner; düğmenin ne yapacağı hemen altındaki if'te.
    private GUIStyle baslikStili, dugmeStili, yaziStili, oyunAdiStili, surumStili;

    private void OnGUI()
    {
        GUI.depth = -200;   // not ve jeneriğin de üstünde
        ArayuzOlcegi.Uygula();   // her ekranda aynı oranda görünsün (#34); yerleşim sanal ekranda

        if (acik) MenuyuCiz();

        // "Başla" geçişindeki kararma (#32): menüden bağımsız, her şeyin en üstüne çizilir.
        if (karartma > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, karartma);
            GUI.DrawTexture(new Rect(0, 0, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    private void MenuyuCiz()
    {
        if (baslikStili == null)
        {
            baslikStili = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            baslikStili.normal.textColor = new Color(0.92f, 0.90f, 0.84f);
            dugmeStili = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            yaziStili.normal.textColor = new Color(0.92f, 0.90f, 0.84f);
            oyunAdiStili = new GUIStyle(baslikStili) { fontSize = 72 };
            surumStili = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            surumStili.normal.textColor = new Color(0.6f, 0.58f, 0.54f);
        }

        // Ekranı karart. Başlık ekranının ana sayfasında hafif: arkadaki merdiven görünsün.
        GUI.color = new Color(0f, 0f, 0f, baslikta && sayfa == SayfaAna ? 0.3f : 0.7f);
        GUI.DrawTexture(new Rect(0, 0, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (sayfa == SayfaAyarlar) { AyarlariCiz(); return; }
        if (sayfa == SayfaGoruntu) { GoruntuCiz(); return; }
        if (sayfa == SayfaDefter) { DefteriCiz(); return; }

        float en = 360f, boy = 56f, ara = 16f;
        float x = (ArayuzOlcegi.Genislik - en) / 2f;
        float y;

        if (baslikta)
        {
            // Oyunun adı (#1'de isim belli olunca yalnızca tablodaki metin değişir) ve
            // sol altta sürüm. Application.version: Project Settings → Player → Version (0.1.0).
            GUI.Label(new Rect(0, ArayuzOlcegi.Yukseklik * 0.16f, ArayuzOlcegi.Genislik, 100f), Dil.Getir("menu.oyun_adi"), oyunAdiStili);
            GUI.Label(new Rect(16, ArayuzOlcegi.Yukseklik - 36f, 400f, 28f), string.Format(Dil.Getir("menu.surum"), Application.version), surumStili);
            y = ArayuzOlcegi.Yukseklik * 0.38f;
        }
        else
        {
            y = ArayuzOlcegi.Yukseklik / 2f - 260f;   // altı düğme (#45'te Defter eklendi)
            GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 60f), Dil.Getir("menu.baslik"), baslikStili);
            y += 60f + 2f * ara;
        }

        // İlk düğme: başlık ekranında "Başla", Esc menüsünde "Devam".
        Rect r = new Rect(x, y, en, boy);
        Vurgula(AnaDevam, r);
        if (GUI.Button(r, baslikta ? Dil.Getir("menu.basla") : Dil.Getir("menu.devam"), dugmeStili))
        {
            if (baslikta) Basla(); else DevamIste();
        }
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(AnaAyarlar, r);
        if (GUI.Button(r, Dil.Getir("menu.ayarlar"), dugmeStili)) SayfaAc(SayfaAyarlar);
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(AnaGoruntu, r);
        if (GUI.Button(r, Dil.Getir("menu.goruntu"), dugmeStili)) SayfaAc(SayfaGoruntu);
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(AnaDefter, r);
        if (GUI.Button(r, Dil.Getir("menu.defter"), dugmeStili)) SayfaAc(SayfaDefter);
        y += boy + ara;

        // "Dil: Türkçe" / "Language: English". Dilin adı her zaman kendi dilinde
        // yazılır; İngilizce bir oyuncu da "Türkçe"yi, Türk oyuncu da "English"i tanır.
        r = new Rect(x, y, en, boy);
        Vurgula(AnaDil, r);
        string dilYazisi = string.Format(Dil.Getir("menu.dil"), Dil.Getir("dil.ad"));
        if (GUI.Button(r, dilYazisi, dugmeStili)) Dil.Sonraki();
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(AnaCik, r);
        if (GUI.Button(r, Dil.Getir("menu.cik"), dugmeStili)) OyundanCik();
    }

    // Seçili öğenin etrafına sıcak renkli bir çerçeve çizer (#29). Kontrolden ÖNCE
    // çağrılır; çerçeve kontrolün 4 piksel dışına taşar, kontrol de üstüne çizilir.
    private void Vurgula(int sira, Rect r)
    {
        if (gezgin.Secili != sira) return;
        const float k = 3f, pay = 4f;   // çizgi kalınlığı ve kontrolden uzaklık
        Rect d = new Rect(r.x - pay, r.y - pay, r.width + 2f * pay, r.height + 2f * pay);
        GUI.color = new Color(0.95f, 0.72f, 0.35f, 0.95f);
        GUI.DrawTexture(new Rect(d.x, d.y, d.width, k), Texture2D.whiteTexture);                 // üst
        GUI.DrawTexture(new Rect(d.x, d.yMax - k, d.width, k), Texture2D.whiteTexture);          // alt
        GUI.DrawTexture(new Rect(d.x, d.y, k, d.height), Texture2D.whiteTexture);                // sol
        GUI.DrawTexture(new Rect(d.xMax - k, d.y, k, d.height), Texture2D.whiteTexture);         // sağ
        GUI.color = Color.white;
    }

    // ---------- ANOMALİ DEFTERİ (#45) ----------
    // Bütün anomaliler katmanlarına göre sıralı bir liste. Bulunanların adı yazar;
    // bulunmamışlar "???" görünür, yalnızca katmanı belli (Şahan'ın seçimi: aramaya
    // teşvik eder ama ipucu vermez). Liste uzun olduğu için yukarı/aşağı ile ya da fare
    // tekerleğiyle kaydırılır.
    private struct DefterSatiri { public string kimlik; public int katman; }
    private DefterSatiri[] defterSatirlari = new DefterSatiri[0];
    private int defterBasi;                      // listenin ekrandaki ilk satırı
    private const int DefterGorunenSatir = 14;
    private static readonly string[] KatmanAnahtarlari = { "defter.fisilti", "defter.tekinsiz", "defter.kabus" };
    private static readonly Color[] KatmanRenkleri =
        { new Color(0.62f, 0.72f, 0.80f), new Color(0.88f, 0.72f, 0.42f), new Color(0.90f, 0.35f, 0.30f) };

    // Kartları yükleyip sıralar: önce katman (fısıltı, tekinsiz, kâbus), sonra kimlik.
    private void DefteriHazirla()
    {
        AnomaliKarti[] kartlar = Resources.LoadAll<AnomaliKarti>("Anomaliler");
        var liste = new System.Collections.Generic.List<DefterSatiri>();
        foreach (AnomaliKarti k in kartlar)
            liste.Add(new DefterSatiri { kimlik = k.Kimlik, katman = (int)k.Katman });
        liste.Sort((a, b) => a.katman != b.katman ? a.katman.CompareTo(b.katman) : string.CompareOrdinal(a.kimlik, b.kimlik));
        defterSatirlari = liste.ToArray();
        defterBasi = 0;
    }

    private void DefteriKaydir(int yon)
    {
        int enSon = Mathf.Max(0, defterSatirlari.Length - DefterGorunenSatir);
        defterBasi = Mathf.Clamp(defterBasi + yon, 0, enSon);
    }

    private void DefteriCiz()
    {
        float en = 620f, satirBoy = 34f;
        float x = (ArayuzOlcegi.Genislik - en) / 2f;
        float y = ArayuzOlcegi.Yukseklik / 2f - 330f;

        // Başlık: "DEFTER  12 / 66" ve katman sayıları.
        int bulunan = 0;
        int[] katmanToplam = new int[3], katmanBulunan = new int[3];
        foreach (DefterSatiri d in defterSatirlari)
        {
            int k = Mathf.Clamp(d.katman, 0, 2);
            katmanToplam[k]++;
            if (Defter.BulunduMu(d.kimlik)) { bulunan++; katmanBulunan[k]++; }
        }
        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 60f),
                  string.Format(Dil.Getir("defter.baslik"), bulunan, defterSatirlari.Length), baslikStili);
        y += 64f;
        string ozet = "";
        for (int k = 0; k < 3; k++)
            ozet += (k > 0 ? "     " : "") + Dil.Getir(KatmanAnahtarlari[k]) + " " + katmanBulunan[k] + "/" + katmanToplam[k];
        var ortali = new GUIStyle(yaziStili) { alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 30f), ozet, ortali);
        y += 44f;

        // Fare tekerleği: listeyi kaydırır.
        Event olay = Event.current;
        if (olay.type == EventType.ScrollWheel) { DefteriKaydir(olay.delta.y > 0f ? 1 : -1); olay.Use(); }

        // Satırlar: solda katman rengiyle küçük bir şerit, katman adı, sonra ad ya da ???.
        var katmanStili = new GUIStyle(yaziStili) { fontSize = 16 };
        var adStili = new GUIStyle(yaziStili) { fontSize = 20 };
        var soluk = new GUIStyle(adStili);
        soluk.normal.textColor = new Color(0.5f, 0.48f, 0.45f);
        // Etiket stilleri fare üstüne gelince kendi "hover" rengini kullanır (beyaz);
        // bir satırın fareyle parlaması seçilebilir bir şeymiş gibi yanıltıyordu.
        adStili.hover.textColor = adStili.normal.textColor;
        soluk.hover.textColor = soluk.normal.textColor;
        int son = Mathf.Min(defterSatirlari.Length, defterBasi + DefterGorunenSatir);
        for (int i = defterBasi; i < son; i++)
        {
            DefterSatiri d = defterSatirlari[i];
            int k = Mathf.Clamp(d.katman, 0, 2);
            bool var_ = Defter.BulunduMu(d.kimlik);
            Rect satir = new Rect(x, y, en, satirBoy - 4f);
            GUI.color = new Color(1f, 1f, 1f, 0.06f);
            GUI.DrawTexture(satir, Texture2D.whiteTexture);
            GUI.color = KatmanRenkleri[k];
            GUI.DrawTexture(new Rect(x, y, 5f, satirBoy - 4f), Texture2D.whiteTexture);
            katmanStili.normal.textColor = KatmanRenkleri[k];
            katmanStili.hover.textColor = KatmanRenkleri[k];
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 16f, y, 140f, satirBoy - 4f), Dil.Getir(KatmanAnahtarlari[k]), katmanStili);
            GUI.Label(new Rect(x + 160f, y, en - 170f, satirBoy - 4f),
                      var_ ? Dil.Getir("anomali." + d.kimlik) : Dil.Getir("defter.bilinmiyor"), var_ ? adStili : soluk);
            y += satirBoy;
        }

        // Kaydırma göstergesi: listenin neresinde olunduğu (sağda ince bir çubuk).
        if (defterSatirlari.Length > DefterGorunenSatir)
        {
            float alanUst = ArayuzOlcegi.Yukseklik / 2f - 330f + 108f, alanBoy = DefterGorunenSatir * satirBoy;
            float oran = (float)DefterGorunenSatir / defterSatirlari.Length;
            float bas = (float)defterBasi / defterSatirlari.Length;
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(new Rect(x + en + 10f, alanUst, 4f, alanBoy), Texture2D.whiteTexture);
            GUI.color = new Color(0.95f, 0.72f, 0.35f, 0.9f);
            GUI.DrawTexture(new Rect(x + en + 10f, alanUst + bas * alanBoy, 4f, oran * alanBoy), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        y = ArayuzOlcegi.Yukseklik / 2f - 330f + 108f + DefterGorunenSatir * satirBoy + 18f;
        Rect r = new Rect((ArayuzOlcegi.Genislik - 240f) / 2f, y, 240f, 50f);
        Vurgula(0, r);
        if (GUI.Button(r, Dil.Getir("menu.geri"), dugmeStili)) SayfaAc(SayfaAna);
    }

    // ---------- AYARLAR SAYFASI (#27) ----------
    // GUI.HorizontalSlider: bir kaydırıcı çizer ve o karedeki değerini döndürür;
    // oyuncu sürüklemediyse verdiğimiz değerin aynısı döner. Değer değiştiyse Ayarlar'a
    // yazıyoruz; Ayarlar onu sınırlar, kaydeder ve uygular (anında, menüden çıkmadan).
    private void AyarlariCiz()
    {
        float en = 480f, ara = 14f;
        float x = (ArayuzOlcegi.Genislik - en) / 2f;
        float y = ArayuzOlcegi.Yukseklik / 2f - 230f;

        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 60f), Dil.Getir("menu.ayarlar"), baslikStili);
        y += 60f + 2f * ara;

        // Sayılar dile göre yazılır: Türkçede "1,50", İngilizcede "1.50".
        // CultureInfo: bir dilin sayı, tarih yazım kuralları.
        System.Globalization.CultureInfo kultur = Dil.Etkin == DilKodu.TR
            ? new System.Globalization.CultureInfo("tr-TR")
            : System.Globalization.CultureInfo.InvariantCulture;

        // Fare hassasiyeti: 0,05'lik adımlarla (1,00 / 1,05 / 1,10...).
        Vurgula(AyarHassasiyet, new Rect(x, y, en, 52f));
        float h = Ayarlar.Hassasiyet;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.hassasiyet"), h.ToString("0.00", kultur)), yaziStili);
        y += 32f;
        float yeniH = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), h, AyarKurallari.HassasiyetEnAz, AyarKurallari.HassasiyetEnCok) * 20f) / 20f;
        if (!Mathf.Approximately(yeniH, h)) Ayarlar.Hassasiyet = yeniH;
        y += 20f + ara;

        // Ses seviyesi: %5'lik adımlar. Değişince örnek ses çalar.
        Vurgula(AyarSes, new Rect(x, y, en, 52f));
        float s = Ayarlar.Ses;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.ses"), Mathf.RoundToInt(s * 100f)), yaziStili);
        y += 32f;
        float yeniS = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), s, AyarKurallari.SesEnAz, AyarKurallari.SesEnCok) * 20f) / 20f;
        if (!Mathf.Approximately(yeniS, s))
        {
            Ayarlar.Ses = yeniS;
            OrnekSesCal();
        }
        y += 20f + ara;

        // Görüş açısı: 1 derecelik adımlar.
        Vurgula(AyarGorus, new Rect(x, y, en, 52f));
        float g = Ayarlar.Gorus;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.gorus"), Mathf.RoundToInt(g)), yaziStili);
        y += 32f;
        float yeniG = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), g, AyarKurallari.GorusEnAz, AyarKurallari.GorusEnCok));
        if (!Mathf.Approximately(yeniG, g)) Ayarlar.Gorus = yeniG;
        y += 20f + ara;

        // Fareyi ters çevir: "Dil" düğmesi gibi, her tıklama durumu değiştirir. İlk
        // denemede Unity'nin işaret kutusu (GUI.Toggle) kullanıldı; kutucuk çok küçük
        // kaldı ve karanlık menüde neredeyse görünmüyordu.
        Rect r = new Rect(x, y, en, 44f);
        Vurgula(AyarTers, r);
        string tersYazisi = string.Format(Dil.Getir("ayar.ters"), Ayarlar.Ters ? Dil.Getir("ayar.acik") : Dil.Getir("ayar.kapali"));
        if (GUI.Button(r, tersYazisi, dugmeStili)) Ayarlar.Ters = !Ayarlar.Ters;
        y += 44f + 2f * ara;

        float dugmeEn = (en - ara) / 2f, boy = 52f;
        r = new Rect(x, y, dugmeEn, boy);
        Vurgula(AyarVarsayilan, r);
        if (GUI.Button(r, Dil.Getir("ayar.varsayilan"), dugmeStili)) Ayarlar.VarsayilanaDon();

        r = new Rect(x + dugmeEn + ara, y, dugmeEn, boy);
        Vurgula(AyarGeri, r);
        if (GUI.Button(r, Dil.Getir("menu.geri"), dugmeStili)) SayfaAc(SayfaAna);
    }

    // ---------- GÖRÜNTÜ SAYFASI (#30) ----------
    // Çözünürlük, ekran modu, V-Sync ve kalite düğmeleri tıklanınca bir sonraki seçeneğe
    // geçer (klavye/kolda sol-sağ da değiştirir). Parlaklık bir kaydırıcı.
    private void GoruntuCiz()
    {
        float en = 480f, ara = 14f, boy = 44f;
        float x = (ArayuzOlcegi.Genislik - en) / 2f;
        float y = ArayuzOlcegi.Yukseklik / 2f - 250f;

        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 60f), Dil.Getir("menu.goruntu"), baslikStili);
        y += 60f + 2f * ara;

        System.Globalization.CultureInfo kultur = Dil.Etkin == DilKodu.TR
            ? new System.Globalization.CultureInfo("tr-TR")
            : System.Globalization.CultureInfo.InvariantCulture;

        // Çözünürlük: "1920 × 1080".
        string cozunurluk = cozunurlukGenislik.Length > 0
            ? cozunurlukGenislik[cozunurlukSirasi] + " × " + cozunurlukYukseklik[cozunurlukSirasi]
            : Screen.width + " × " + Screen.height;
        Rect r = new Rect(x, y, en, boy);
        Vurgula(GorCozunurluk, r);
        if (GUI.Button(r, string.Format(Dil.Getir("ayar.cozunurluk"), cozunurluk), dugmeStili)) CozunurlukDegistir(+1);
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(GorEkran, r);
        string ekran = Ayarlar.TamEkran ? Dil.Getir("ayar.tamekran") : Dil.Getir("ayar.pencere");
        if (GUI.Button(r, string.Format(Dil.Getir("ayar.ekran"), ekran), dugmeStili)) Ayarlar.TamEkran = !Ayarlar.TamEkran;
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(GorVSync, r);
        string vsync = Ayarlar.VSync ? Dil.Getir("ayar.acik") : Dil.Getir("ayar.kapali");
        if (GUI.Button(r, string.Format(Dil.Getir("ayar.vsync"), vsync), dugmeStili)) Ayarlar.VSync = !Ayarlar.VSync;
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(GorKalite, r);
        string kalite = Dil.Getir(KaliteAnahtarlari[Ayarlar.Kalite]);
        if (GUI.Button(r, string.Format(Dil.Getir("ayar.kalite"), kalite), dugmeStili))
            Ayarlar.Kalite = (Ayarlar.Kalite + 1) % KaliteAnahtarlari.Length;
        y += boy + ara;

        // Parlaklık: 0,1'lik adımlar, işaretiyle yazılır ("+0,3", "-0,5", "0,0").
        Vurgula(GorParlaklik, new Rect(x, y, en, 52f));
        float p = Ayarlar.Parlaklik;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.parlaklik"), p.ToString("+0.0;-0.0;0.0", kultur)), yaziStili);
        y += 32f;
        float yeniP = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), p, AyarKurallari.ParlaklikEnAz, AyarKurallari.ParlaklikEnCok) * 10f) / 10f;
        if (!Mathf.Approximately(yeniP, p)) Ayarlar.Parlaklik = yeniP;
        y += 20f + 2f * ara;

        float dugmeEn = (en - ara) / 2f;
        r = new Rect(x, y, dugmeEn, 52f);
        Vurgula(GorVarsayilan, r);
        if (GUI.Button(r, Dil.Getir("ayar.varsayilan"), dugmeStili)) { Ayarlar.GoruntuVarsayilanaDon(); CozunurlukListesiniKur(); }

        r = new Rect(x + dugmeEn + ara, y, dugmeEn, 52f);
        Vurgula(GorGeri, r);
        if (GUI.Button(r, Dil.Getir("menu.geri"), dugmeStili)) SayfaAc(SayfaAna);
    }
}
