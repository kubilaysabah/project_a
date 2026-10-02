// YoneticiNotu.cs
// Kapıcı dairesinin kapısının altına bırakılmış yönetici notu (#16, açılış).
// Oyunun tek "eğitimi" bu kâğıt: kuralı bir öğretici ekran değil, binanın kendisi söyler.
//
// Poşet gibi IEtkilesimli sözleşmesini imzalar: oyuncu nota bakınca "OKU (E)" çıkar,
// E'ye basınca not ekrana bir kâğıt olarak gelir. Oyuncunun kodunda hiçbir değişiklik
// gerekmez; Etkilesimci neye baktığını bilmeden Etkilesim'i çağırır.
//
// Not açıkken oyuncu yürüyemez ve etrafa bakamaz (okurken dikkat dağılmasın).
// E ya da Esc notu kapatır. Not yerinde kalır, tekrar okunabilir.

using UnityEngine;
using UnityEngine.InputSystem;

public class YoneticiNotu : MonoBehaviour, IEtkilesimli
{
    // Notun metni kodda değil, metin tablosunda (Assets/Resources/Metinler.txt)
    // bu anahtarın altında durur; Türkçesi de İngilizcesi de oradan gelir (#15).
    // #55: aynı bileşen dairelerdeki notlar için de kullanılıyor; metnin anahtarı kat dosyasından
    // gelir (Ayarla). Kapıcı odasındaki yönetici notu varsayılan anahtarla kalır.
    [SerializeField] private string metinAnahtari = "not.yonetici";

    /// <summary>Notun metnini değiştirir (zincir uygulayıcısı daireye not koyarken).</summary>
    public void Ayarla(string anahtar) { if (!string.IsNullOrEmpty(anahtar)) metinAnahtari = anahtar; }

    // ---------- SÖZLEŞME ----------
    public string Ipucu { get { return Dil.Getir("ipucu.oku"); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.Goz; } }   // #104: okunacak şey

    // Not açıkken ikinci kez açılmasın.
    public bool EtkilesimeAcik { get { return !acik; } }

    private static AudioClip kagitSesi;

    public void Etkilesim(GameObject oyuncu)
    {
        acik = true;
        acilisKaresi = Time.frameCount;
        Kayit.NotOkundu(metinAnahtari);   // kapıcı odasının panosunda görünsün (#56)

        // Kâğıt açılınca hışırdar (#99: not okurken hiç ses yoktu). Telefon konuşması da bu ekranı
        // kullanır; ahizeden kâğıt sesi gelmesin diye telefonun çocuğuysa çalmaz.
        if (GetComponentInParent<Telefon>() == null)
        {
            if (kagitSesi == null) kagitSesi = Resources.Load<AudioClip>("Sesler/Al_Kagit");
            if (kagitSesi != null && Camera.main != null) AudioSource.PlayClipAtPoint(kagitSesi, Camera.main.transform.position, 0.4f);
        }

        // Oyuncunun yürümesini, bakmasını ve başka şeylerle etkileşimini durdur.
        // "enabled = false" bir bileşeni kapatır: Update'i artık çağrılmaz.
        kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
        etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;

        // Etkilesimci kapanırken "Player/Interact" eylemini de kapatıyor (OnDisable). Bu
        // not da AYNI eylemi dinliyor (proje genelindeki tek eylem nesnesi); kapalı
        // kalırsa E/A basışlarını hiç duyamaz. Bu yüzden eylemi burada yeniden açıyoruz.
        // #29'da bulundu: not bugüne kadar E ile hiç kapanmıyordu, yalnızca Esc ile.
        etkilesimEylemi?.Enable();
    }

    // ---------- DURUM ----------
    private bool acik;
    private int acilisKaresi;
    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;

    private void Awake()
    {
        if (InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        if (!acik) return;

        // Notu AÇAN E basışı aynı karede onu kapatmasın: açıldığı karede kapatma yok.
        if (Time.frameCount <= acilisKaresi) return;

        // Etkileşim eylemi klavyede E, oyun kolunda A (#29). Esc ve kolun B tuşu da kapatır.
        bool eBasildi = etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame();
        bool escBasildi = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool bBasildi = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (eBasildi || escBasildi || bBasildi) Kapat();
    }

    private bool geriVerilecek;   // oyuncunun kontrolü bu karenin sonunda geri verilecek

    private void Kapat()
    {
        acik = false;

        // Oyuncuyu HEMEN değil, karenin sonunda (LateUpdate) serbest bırak (#29).
        // Notu kapatan E/A basışı bu karede hâlâ "basıldı" görünüyor. Etkilesimci aynı
        // karede açılırsa ve onun Update'i bizden sonra çalışırsa, aynı basışı görüp
        // oyuncunun hâlâ baktığı notu hemen yeniden açardı. Bu, betiklerin çalışma
        // sırasına bağlı sinsi bir hataydı; LateUpdate bütün Update'lerden sonra geldiği
        // için sonraki karede basış artık "yeni" sayılmıyor.
        geriVerilecek = true;
    }

    private void LateUpdate()
    {
        if (!geriVerilecek) return;
        geriVerilecek = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
    }

    // ---------- KÂĞIT ----------
    // OnGUI ile ekranın ortasına sararmış bir kâğıt ve üstüne not çiziliyor.
    // Test göstergesindeki gibi basit çizim; ayrı bir arayüz sistemi kurmaya gerek yok.
    // Metin her karede tablodan istenir (#15); not açıkken dil değişirse hemen güncellenir.
    private GUIStyle yaziStili;

    private void OnGUI()
    {
        if (!acik) return;
        ArayuzOlcegi.Uygula();   // her ekranda aynı oranda görünsün (#34); yerleşim sanal ekranda

        float en = Mathf.Min(560f, ArayuzOlcegi.Genislik * 0.8f);
        float boy = Mathf.Min(420f, ArayuzOlcegi.Yukseklik * 0.75f);
        Rect kagit = new Rect((ArayuzOlcegi.Genislik - en) / 2f, (ArayuzOlcegi.Yukseklik - boy) / 2f, en, boy);

        // Ekranı hafifçe karart, kâğıdı öne çıkar.
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);

        // Kâğıt: sararmış beyaz.
        GUI.color = new Color(0.86f, 0.82f, 0.70f, 1f);
        GUI.DrawTexture(kagit, Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (yaziStili == null)
        {
            yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true };
            yaziStili.normal.textColor = new Color(0.12f, 0.10f, 0.08f);   // mürekkep
        }

        GUI.Label(new Rect(kagit.x + 36, kagit.y + 30, kagit.width - 72, kagit.height - 80), Dil.Getir(metinAnahtari), yaziStili);

        var ipucu = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.LowerRight };
        ipucu.normal.textColor = new Color(0.3f, 0.27f, 0.22f);
        // "E / Esc: kapat" ya da oyun kolu kullanılıyorsa "A / B: kapat" (#29).
        string tuslar = GirisCihazi.Kol ? Dil.Getir("tus.kapat_kol") : Dil.Getir("tus.kapat_klavye");
        GUI.Label(new Rect(kagit.x, kagit.y, kagit.width - 20, kagit.height - 12), string.Format(Dil.Getir("not.kapat"), tuslar), ipucu);
    }
}
