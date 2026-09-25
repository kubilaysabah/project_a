// DuraklatmaMenusu.cs
// Esc (ya da oyun kolunun Start tuşu) ile açılan duraklatma menüsü (#15, #27, #29).
// Ana sayfa: Devam, Ayarlar, Dil, Oyundan Çık. Ayarlar sayfası: hassasiyet, ses,
// görüş açısı, fareyi ters çevir, varsayılana dön, geri.
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

    // Menünün iki sayfası var: ana sayfa ve ayarlar (#27).
    private bool ayarSayfasi;

    // ---------- KLAVYE / KOL İLE GEZİNME (#29) ----------
    // Öğelerin sırası (yukarıdan aşağı). Numara, gezginin "Secili" değeriyle eşleşir.
    private const int AnaDevam = 0, AnaAyarlar = 1, AnaDil = 2, AnaCik = 3, AnaOgeSayisi = 4;
    private const int AyarHassasiyet = 0, AyarSes = 1, AyarGorus = 2, AyarTers = 3,
                      AyarVarsayilan = 4, AyarGeri = 5, AyarOgeSayisi = 6;

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
    }

    private void Update()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool baslat = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        bool geri = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (!acik)
        {
            if ((esc || baslat) && oncekiKareOynanabilir) Ac();
            return;
        }

        if (devamIstendi) return;                       // kapanmak üzere; başka girdi okuma

        if (baslat) { DevamIste(); return; }            // Start her sayfadan menüyü kapatır
        if (esc || geri)                                // Esc / B: bir geri git
        {
            if (ayarSayfasi) SayfaAc(ayarlar: false);
            else DevamIste();
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
        if (dikeyTekrar.Guncelle(dikey, Time.unscaledTime)) gezgin.Tasi(yon.y > 0f ? -1 : +1);   // yukarı = listede geri
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
        if (!ayarSayfasi)
        {
            switch (gezgin.Secili)
            {
                case AnaDevam: DevamIste(); break;
                case AnaAyarlar: SayfaAc(ayarlar: true); break;
                case AnaDil: Dil.Sonraki(); break;
                case AnaCik: OyundanCik(); break;
            }
            return;
        }

        switch (gezgin.Secili)
        {
            case AyarTers: Ayarlar.Ters = !Ayarlar.Ters; break;
            case AyarVarsayilan: Ayarlar.VarsayilanaDon(); break;
            case AyarGeri: SayfaAc(ayarlar: false); break;
            // Kaydırıcılarda onay tuşunun işi yok; sol/sağ ile değiştirilir.
        }
    }

    // Sol (-1) / sağ (+1): seçili kaydırıcıyı bir adım oynatır. Açık/kapalı ve dil
    // düğmelerinde de durumu değiştirir; sol/sağ oyuncuya orada da doğal gelir.
    private void Kaydir(int yon)
    {
        if (!ayarSayfasi)
        {
            if (gezgin.Secili == AnaDil) Dil.Sonraki();
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

    private void SayfaAc(bool ayarlar)
    {
        ayarSayfasi = ayarlar;
        gezgin.OgeSayisiniAyarla(ayarlar ? AyarOgeSayisi : AnaOgeSayisi);
        // Ayarlara girince ilk öğe; ana sayfaya dönünce "Ayarlar" düğmesi seçili kalsın
        // (oyuncu nereden geldiğini görür, tekrar girmek isterse tek tuş yeter).
        gezgin.Sec(ayarlar ? AyarHassasiyet : AnaAyarlar);
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
        SayfaAc(ayarlar: false);
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
    private GUIStyle baslikStili, dugmeStili, yaziStili;

    private void OnGUI()
    {
        if (!acik) return;
        GUI.depth = -200;   // not ve jeneriğin de üstünde

        if (baslikStili == null)
        {
            baslikStili = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            baslikStili.normal.textColor = new Color(0.92f, 0.90f, 0.84f);
            dugmeStili = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            yaziStili.normal.textColor = new Color(0.92f, 0.90f, 0.84f);
        }

        // Ekranı karart.
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (ayarSayfasi) { AyarlariCiz(); return; }

        float en = 360f, boy = 56f, ara = 16f;
        float x = (Screen.width - en) / 2f;
        float y = Screen.height / 2f - 190f;

        GUI.Label(new Rect(0, y, Screen.width, 60f), Dil.Getir("menu.baslik"), baslikStili);
        y += 60f + 2f * ara;

        Rect r = new Rect(x, y, en, boy);
        Vurgula(AnaDevam, r);
        if (GUI.Button(r, Dil.Getir("menu.devam"), dugmeStili)) DevamIste();
        y += boy + ara;

        r = new Rect(x, y, en, boy);
        Vurgula(AnaAyarlar, r);
        if (GUI.Button(r, Dil.Getir("menu.ayarlar"), dugmeStili)) SayfaAc(ayarlar: true);
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

    // ---------- AYARLAR SAYFASI (#27) ----------
    // GUI.HorizontalSlider: bir kaydırıcı çizer ve o karedeki değerini döndürür;
    // oyuncu sürüklemediyse verdiğimiz değerin aynısı döner. Değer değiştiyse Ayarlar'a
    // yazıyoruz; Ayarlar onu sınırlar, kaydeder ve uygular (anında, menüden çıkmadan).
    private void AyarlariCiz()
    {
        float en = 480f, ara = 14f;
        float x = (Screen.width - en) / 2f;
        float y = Screen.height / 2f - 230f;

        GUI.Label(new Rect(0, y, Screen.width, 60f), Dil.Getir("menu.ayarlar"), baslikStili);
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
        if (GUI.Button(r, Dil.Getir("menu.geri"), dugmeStili)) SayfaAc(ayarlar: false);
    }
}
