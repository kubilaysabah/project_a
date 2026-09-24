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
    [TextArea(4, 10)]
    [SerializeField] private string metin =
        "Kapıcı Efendi,\n\n" +
        "Gece çöpleri kat kat topla. Merdivenler hep aynıdır, alışırsın.\n\n" +
        "Bir şey yanlışsa — ne olduğunu sorma — KATA GİRME, geri dön.\n\n" +
        "Sabah çatının kapısını kilitlemeyi unutma.\n\n" +
        "— Yönetim";

    // ---------- SÖZLEŞME ----------
    public string Ipucu { get { return "OKU"; } }

    // Not açıkken ikinci kez açılmasın.
    public bool EtkilesimeAcik { get { return !acik; } }

    public void Etkilesim(GameObject oyuncu)
    {
        acik = true;
        acilisKaresi = Time.frameCount;

        // Oyuncunun yürümesini, bakmasını ve başka şeylerle etkileşimini durdur.
        // "enabled = false" bir bileşeni kapatır: Update'i artık çağrılmaz.
        kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
        etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
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

        bool eBasildi = etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame();
        bool escBasildi = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        if (eBasildi || escBasildi) Kapat();
    }

    private void Kapat()
    {
        acik = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
    }

    // ---------- KÂĞIT ----------
    // OnGUI ile ekranın ortasına sararmış bir kâğıt ve üstüne not çiziliyor.
    // Test göstergesindeki gibi basit çizim; ayrı bir arayüz sistemi kurmaya gerek yok.
    // Türkçe/İngilizce metin (#15) geldiğinde metin buradan okunacak.
    private GUIStyle yaziStili;

    private void OnGUI()
    {
        if (!acik) return;

        float en = Mathf.Min(560f, Screen.width * 0.8f);
        float boy = Mathf.Min(420f, Screen.height * 0.75f);
        Rect kagit = new Rect((Screen.width - en) / 2f, (Screen.height - boy) / 2f, en, boy);

        // Ekranı hafifçe karart, kâğıdı öne çıkar.
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        // Kâğıt: sararmış beyaz.
        GUI.color = new Color(0.86f, 0.82f, 0.70f, 1f);
        GUI.DrawTexture(kagit, Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (yaziStili == null)
        {
            yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true };
            yaziStili.normal.textColor = new Color(0.12f, 0.10f, 0.08f);   // mürekkep
        }

        GUI.Label(new Rect(kagit.x + 36, kagit.y + 30, kagit.width - 72, kagit.height - 80), metin, yaziStili);

        var ipucu = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.LowerRight };
        ipucu.normal.textColor = new Color(0.3f, 0.27f, 0.22f);
        GUI.Label(new Rect(kagit.x, kagit.y, kagit.width - 20, kagit.height - 12), "E / Esc: kapat", ipucu);
    }
}
