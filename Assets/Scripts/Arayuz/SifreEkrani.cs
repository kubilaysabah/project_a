// SifreEkrani.cs
// Şifreli asma kilidin yakın bakış ekranı (#54). Oyuncu kilide dokununca açılır: ekranın
// ortasında kilit ve dört çarkı. Yazı yok: yalnızca çarkların rakamları ve seçili çarkın
// üstünde, altında küçük oklar.
//
// KONTROLLER
//   Çark seçmek   : sol / sağ ok, A / D, kolun yön tuşları ya da sol çubuk (yatay).
//   Rakam çevirmek: yukarı / aşağı ok, W / S, kolun yön tuşları, sol çubuk (dikey), fare tekerleği.
//   Çıkmak        : E (kolda A), Esc (kolda B).
// Dört rakam kilidin şifresine denk gelince kilit "tak" diye açılır, ekran kendiliğinden kapanır.
//
// Oyuncunun yürümesi ve etkileşimi ekran açıkken kapanır; kapanırken karenin sonunda
// (LateUpdate) geri verilir, yönetici notundaki gibi (#29: kapatan basış aynı karede yeni bir
// etkileşim başlatmasın). Çarklar en son bırakıldığı yerde kalır: oyuncu yarım bıraktığı
// denemeye döner.

using UnityEngine;
using UnityEngine.InputSystem;

public class SifreEkrani : MonoBehaviour
{
    private Kilit kilit;
    private readonly int[] rakam = new int[4];
    private int secili;
    private bool acik;
    private int acilisKaresi;
    private bool geriVerilecek;
    private float cubukBekleme;               // çubukla tek tek adım atmak için kısa bekleme

    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;
    private static AudioClip tik;
    private GUIStyle rakamStili;

    /// <summary>Herhangi bir şifre ekranı açık mı? (Başka arayüzler buna bakabilir.)</summary>
    public static bool BiriAcik { get; private set; }

    public void Ac(Kilit hedef, GameObject oyuncu)
    {
        kilit = hedef;
        acik = true;
        BiriAcik = true;
        acilisKaresi = Time.frameCount;
        if (oyuncu != null)
        {
            kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
            etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        }
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        // Etkilesimci kapanırken paylaşılan "Player/Interact" eylemini de kapatır (#29); E ile
        // çıkabilmek için burada yeniden açıyoruz.
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();
        if (tik == null) tik = Resources.Load<AudioClip>("Sesler/Sifre_Cark");
    }

    private void Update()
    {
        if (!acik || Time.frameCount <= acilisKaresi) return;

        var k = Keyboard.current;
        var g = Gamepad.current;
        bool cik = (etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame())
                   || (k != null && k.escapeKey.wasPressedThisFrame)
                   || (g != null && g.buttonEast.wasPressedThisFrame);
        if (cik) { Kapat(); return; }

        int yatay = 0, dikey = 0;
        if (k != null)
        {
            if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) yatay = -1;
            if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) yatay = 1;
            if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) dikey = 1;
            if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) dikey = -1;
        }
        if (g != null)
        {
            if (g.dpad.left.wasPressedThisFrame) yatay = -1;
            if (g.dpad.right.wasPressedThisFrame) yatay = 1;
            if (g.dpad.up.wasPressedThisFrame) dikey = 1;
            if (g.dpad.down.wasPressedThisFrame) dikey = -1;
            // Sol çubuk: eşiği geçince bir adım, sonra 0,25 sn bekle (basılı tutunca da yürüsün).
            Vector2 c = g.leftStick.ReadValue();
            cubukBekleme -= Time.unscaledDeltaTime;
            if (c.magnitude > 0.6f && cubukBekleme <= 0f)
            {
                if (Mathf.Abs(c.x) > Mathf.Abs(c.y)) yatay = c.x > 0 ? 1 : -1; else dikey = c.y > 0 ? 1 : -1;
                cubukBekleme = 0.25f;
            }
            if (c.magnitude < 0.3f) cubukBekleme = 0f;
        }
        var fare = Mouse.current;
        if (fare != null)
        {
            float tekerlek = fare.scroll.ReadValue().y;
            if (tekerlek > 0.1f) dikey = 1; else if (tekerlek < -0.1f) dikey = -1;
        }

        if (yatay != 0) secili = (secili + yatay + 4) % 4;
        if (dikey != 0)
        {
            // % 10 ile 9'dan sonra 0'a, 0'dan önce 9'a döner (gerçek çark gibi).
            rakam[secili] = (rakam[secili] + dikey + 10) % 10;
            if (tik != null) AudioSource.PlayClipAtPoint(tik, transform.position, 0.5f);
            if (kilit != null && Deneme() == kilit.Veri.Sifre)
            {
                kilit.Coz(acilsin: false);
                Kapat();
            }
        }
    }

    // Çarkların şu anki dört rakamı yan yana ("0612").
    private string Deneme() { return $"{rakam[0]}{rakam[1]}{rakam[2]}{rakam[3]}"; }

    private void Kapat()
    {
        acik = false;
        BiriAcik = false;
        geriVerilecek = true;
    }

    private void LateUpdate()
    {
        if (!geriVerilecek) return;
        geriVerilecek = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
    }

    private void OnGUI()
    {
        if (!acik) return;
        ArayuzOlcegi.Uygula();
        float w = ArayuzOlcegi.Genislik, h = ArayuzOlcegi.Yukseklik;

        // Arka planı karart.
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);

        // Kilidin pirinç gövdesi ve çelik halkası.
        float en = 420f, boy = 300f;
        Rect govde = new Rect((w - en) / 2f, (h - boy) / 2f + 60f, en, boy);
        GUI.color = new Color(0.45f, 0.45f, 0.47f);
        GUI.DrawTexture(new Rect(govde.x + 90f, govde.y - 120f, 34f, 140f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(govde.xMax - 124f, govde.y - 120f, 34f, 140f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(govde.x + 90f, govde.y - 120f, en - 180f, 30f), Texture2D.whiteTexture);
        GUI.color = new Color(0.62f, 0.47f, 0.18f);
        GUI.DrawTexture(govde, Texture2D.whiteTexture);

        if (rakamStili == null)
            rakamStili = new GUIStyle(GUI.skin.label) { fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        // Dört çark: koyu silindirler, üstünde rakam; seçili olan açık renkte ve oklu.
        float carkEn = 70f, carkBoy = 110f, ara = 16f;
        float toplam = 4 * carkEn + 3 * ara;
        float x0 = govde.x + (en - toplam) / 2f, y0 = govde.y + (boy - carkBoy) / 2f;
        for (int i = 0; i < 4; i++)
        {
            Rect r = new Rect(x0 + i * (carkEn + ara), y0, carkEn, carkBoy);
            bool s = i == secili;
            GUI.color = s ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.08f, 0.08f, 0.08f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = s ? new Color(1f, 0.92f, 0.7f) : new Color(0.75f, 0.72f, 0.65f);
            GUI.Label(r, rakam[i].ToString(), rakamStili);
            if (s)
            {
                // Üstte ve altta küçük üçgen oklar yerine ince çubuklar: "çevrilebilir" işareti.
                GUI.DrawTexture(new Rect(r.x + 20f, r.y - 18f, carkEn - 40f, 6f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + 20f, r.yMax + 12f, carkEn - 40f, 6f), Texture2D.whiteTexture);
            }
        }
        GUI.color = Color.white;
    }
}
