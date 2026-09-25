// DuraklatmaMenusu.cs
// Esc ile açılan duraklatma menüsü (#15): Devam, Dil, Oyundan Çık.
// Dil seçimi burada yapılır; ileride fare hassasiyeti ve ses ayarı da buraya gelir.
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

    // Menünün iki sayfası var: ana sayfa (Devam / Ayarlar / Dil / Çık) ve ayarlar (#27).
    private bool ayarSayfasi;

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
    }

    private void Update()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (!esc) return;

        if (acik && ayarSayfasi) ayarSayfasi = false;   // ayarlardan ana sayfaya dön
        else if (acik) Devam();
        else if (oncekiKareOynanabilir) Ac();
    }

    // LateUpdate: bütün Update'lerden sonra çalışır. Karenin SON durumunu kaydeder.
    private void LateUpdate()
    {
        oncekiKareOynanabilir = !acik && kontrolcu != null && kontrolcu.enabled;
    }

    // ---------- AÇ / KAPAT ----------
    private void Ac()
    {
        acik = true;
        ayarSayfasi = false;   // menü her zaman ana sayfadan açılır

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

        if (GUI.Button(new Rect(x, y, en, boy), Dil.Getir("menu.devam"), dugmeStili)) Devam();
        y += boy + ara;

        if (GUI.Button(new Rect(x, y, en, boy), Dil.Getir("menu.ayarlar"), dugmeStili)) ayarSayfasi = true;
        y += boy + ara;

        // "Dil: Türkçe" / "Language: English". Dilin adı her zaman kendi dilinde
        // yazılır; İngilizce bir oyuncu da "Türkçe"yi, Türk oyuncu da "English"i tanır.
        string dilYazisi = string.Format(Dil.Getir("menu.dil"), Dil.Getir("dil.ad"));
        if (GUI.Button(new Rect(x, y, en, boy), dilYazisi, dugmeStili)) Dil.Sonraki();
        y += boy + ara;

        if (GUI.Button(new Rect(x, y, en, boy), Dil.Getir("menu.cik"), dugmeStili)) OyundanCik();
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
        float h = Ayarlar.Hassasiyet;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.hassasiyet"), h.ToString("0.00", kultur)), yaziStili);
        y += 32f;
        float yeniH = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), h, AyarKurallari.HassasiyetEnAz, AyarKurallari.HassasiyetEnCok) * 20f) / 20f;
        if (!Mathf.Approximately(yeniH, h)) Ayarlar.Hassasiyet = yeniH;
        y += 20f + ara;

        // Ses seviyesi: %5'lik adımlar. Değişince örnek ses çalar (en fazla 0,2 sn'de bir;
        // sürüklerken her karede çalsaydı ses üst üste binip cızırtıya dönerdi).
        float s = Ayarlar.Ses;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.ses"), Mathf.RoundToInt(s * 100f)), yaziStili);
        y += 32f;
        float yeniS = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), s, AyarKurallari.SesEnAz, AyarKurallari.SesEnCok) * 20f) / 20f;
        if (!Mathf.Approximately(yeniS, s))
        {
            Ayarlar.Ses = yeniS;
            // unscaledTime: oyun saati durmuşken de ilerleyen gerçek zaman.
            if (ornekSes != null && ornekSes.clip != null && Time.unscaledTime - sonOrnekZamani > 0.2f)
            {
                ornekSes.PlayOneShot(ornekSes.clip, 0.8f);
                sonOrnekZamani = Time.unscaledTime;
            }
        }
        y += 20f + ara;

        // Görüş açısı: 1 derecelik adımlar.
        float g = Ayarlar.Gorus;
        GUI.Label(new Rect(x, y, en, 30f), string.Format(Dil.Getir("ayar.gorus"), Mathf.RoundToInt(g)), yaziStili);
        y += 32f;
        float yeniG = Mathf.Round(GUI.HorizontalSlider(new Rect(x, y, en, 20f), g, AyarKurallari.GorusEnAz, AyarKurallari.GorusEnCok));
        if (!Mathf.Approximately(yeniG, g)) Ayarlar.Gorus = yeniG;
        y += 20f + ara;

        // Fareyi ters çevir: "Dil" düğmesi gibi, her tıklama durumu değiştirir. İlk
        // denemede Unity'nin işaret kutusu (GUI.Toggle) kullanıldı; kutucuk çok küçük
        // kaldı ve karanlık menüde neredeyse görünmüyordu.
        string tersYazisi = string.Format(Dil.Getir("ayar.ters"), Ayarlar.Ters ? Dil.Getir("ayar.acik") : Dil.Getir("ayar.kapali"));
        if (GUI.Button(new Rect(x, y, en, 44f), tersYazisi, dugmeStili)) Ayarlar.Ters = !Ayarlar.Ters;
        y += 44f + 2f * ara;

        float dugmeEn = (en - ara) / 2f, boy = 52f;
        if (GUI.Button(new Rect(x, y, dugmeEn, boy), Dil.Getir("ayar.varsayilan"), dugmeStili)) Ayarlar.VarsayilanaDon();
        if (GUI.Button(new Rect(x + dugmeEn + ara, y, dugmeEn, boy), Dil.Getir("menu.geri"), dugmeStili)) ayarSayfasi = false;
    }
}
