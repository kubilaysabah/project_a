// FotografEkrani.cs
// Aile fotoğrafına yakından bakış (#69). Fotoğraf beyaz kenarlı bir baskı gibi, hafif eğik durur;
// E (kolda A) çevirir: arkasında el yazısıyla ailenin hikâyesi. Esc (kolda B) kapatır.
//
// Not ekranının (YoneticiNotu) kalıbı: açılınca oyuncunun kontrolü ve etkileşimi kapanır, ortak
// "Player/Interact" eylemi yeniden açılır (Etkilesimci kapanırken onu kapatır, bkz. CLAUDE.md #29),
// kapanınca kontrol LateUpdate'te geri verilir; yoksa kapatan tuş aynı karede oyuncuya ulaşırdı.
//
// Sahnede kurulmasına gerek yok: ilk açılışta kendini oluşturur. Çizim fonksiyonu (Ciz) menüdeki
// Fotoğraflar sayfası da kullanır.

using UnityEngine;
using UnityEngine.InputSystem;

public class FotografEkrani : MonoBehaviour
{
    private static FotografEkrani ornek;

    /// <summary>Yakın bakış şu an açık mı?</summary>
    public static bool Acik { get { return ornek != null && ornek.acik; } }

    private bool acik, arka, geriVerilecek;
    private int acilisKaresi;
    private FotografAlbumu.Fotograf fotograf;
    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;
    private static AudioClip kagitSesi;

    /// <summary>Fotoğrafı açar; oyuncunun kontrolünü kapatır.</summary>
    public static void Ac(FotografAlbumu.Fotograf f, GameObject oyuncu)
    {
        if (ornek == null)
        {
            var g = new GameObject("Fotograf_Ekrani");
            DontDestroyOnLoad(g);
            ornek = g.AddComponent<FotografEkrani>();
        }
        ornek.AcIc(f, oyuncu);
    }

    private void AcIc(FotografAlbumu.Fotograf f, GameObject oyuncu)
    {
        fotograf = f;
        acik = true;
        arka = false;
        acilisKaresi = Time.frameCount;
        kontrolcu = oyuncu != null ? oyuncu.GetComponent<BirinciSahisKontrolcu>() : null;
        etkilesimci = oyuncu != null ? oyuncu.GetComponent<Etkilesimci>() : null;
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();
        SesCal();
    }

    private static void SesCal()
    {
        if (kagitSesi == null) kagitSesi = Resources.Load<AudioClip>("Sesler/Poset_Hisirti");   // kâğıt hışırtısı
        if (kagitSesi != null && Camera.main != null) AudioSource.PlayClipAtPoint(kagitSesi, Camera.main.transform.position, 0.35f);
    }

    private void Update()
    {
        if (!acik || Time.frameCount <= acilisKaresi) return;   // açan tuş aynı karede çevirmesin
        bool cevir = etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame();
        bool kapat = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                  || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        if (kapat) { acik = false; geriVerilecek = true; }
        else if (cevir) { arka = !arka; SesCal(); }
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
        if (!acik || fotograf == null) return;
        ArayuzOlcegi.Uygula();
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, ArayuzOlcegi.Genislik, ArayuzOlcegi.Yukseklik), Texture2D.whiteTexture);
        GUI.color = Color.white;
        Ciz(fotograf, arka, new Vector2(ArayuzOlcegi.Genislik / 2f, ArayuzOlcegi.Yukseklik / 2f - 20f), 640f);
        IpucuCiz(ArayuzOlcegi.Yukseklik - 60f);
    }

    private static GUIStyle ipucuStili, yaziStili;

    /// <summary>Alt ortada tuş ipucu: "E: çevir   Esc: kapat".</summary>
    public static void IpucuCiz(float y)
    {
        if (ipucuStili == null)
        {
            ipucuStili = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            ipucuStili.normal.textColor = new Color(0.8f, 0.78f, 0.72f);
        }
        bool kol = GirisCihazi.Kol;
        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 30f),
                  string.Format(Dil.Getir("foto.tuslar"), kol ? "A" : "E", kol ? "B" : "Esc"), ipucuStili);
    }

    /// <summary>
    /// Fotoğrafı (ya da arkasını) çizer: merkez noktası ve en uzun kenarın boyu (sanal piksel).
    /// Baskı hafifçe (-2°) eğik durur; masaya atılmış bir fotoğraf gibi.
    /// </summary>
    public static void Ciz(FotografAlbumu.Fotograf f, bool arkaYuz, Vector2 merkez, float uzunKenar)
    {
        if (f == null || f.Doku == null) return;
        float oran = (float)f.Doku.width / f.Doku.height;
        float en = oran >= 1f ? uzunKenar : uzunKenar * oran;
        float boy = oran >= 1f ? uzunKenar / oran : uzunKenar;
        const float kenar = 16f;   // baskının beyaz kenarı
        Rect baski = new Rect(merkez.x - en / 2f - kenar, merkez.y - boy / 2f - kenar, en + 2f * kenar, boy + 2f * kenar);

        Matrix4x4 eski = GUI.matrix;
        // RotateAroundPivot: bundan sonraki çizimleri verilen nokta etrafında döndürür (GUI.matrix'e eklenir).
        GUIUtility.RotateAroundPivot(-2f, merkez);
        // Hafif gölge, sonra baskı.
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(new Rect(baski.x + 8f, baski.y + 10f, baski.width, baski.height), Texture2D.whiteTexture);
        if (!arkaYuz)
        {
            GUI.color = new Color(0.92f, 0.90f, 0.85f);   // yıllanmış beyaz kenar
            GUI.DrawTexture(baski, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(baski.x + kenar, baski.y + kenar, en, boy), f.Doku, ScaleMode.StretchToFill);
        }
        else
        {
            GUI.color = new Color(0.86f, 0.82f, 0.72f);   // fotoğraf kâğıdının arkası
            GUI.DrawTexture(baski, Texture2D.whiteTexture);
            GUI.color = Color.white;
            if (yaziStili == null)
            {
                yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Italic, wordWrap = true, alignment = TextAnchor.MiddleCenter };
                yaziStili.normal.textColor = new Color(0.12f, 0.14f, 0.32f);   // mavi tükenmez kalem
            }
            // Yazı alanı: kâğıdın kenarlarından içeride. Metin sığmazsa punto 28'den 2'şer küçülür
            // (en az 16): menüdeki küçük baskıda uzun bir hikâye taşmasın, yakın bakışta büyük kalsın.
            // CalcHeight: bu genişlikte, satır kaydırmayla metnin kaç piksel yükseklik tuttuğunu hesaplar.
            Rect alan = new Rect(baski.x + 50f, baski.y + 40f, baski.width - 100f, baski.height - 80f);
            GUIContent metin = new GUIContent(Dil.Getir(f.Arka));
            yaziStili.fontSize = 28;
            while (yaziStili.fontSize > 16 && yaziStili.CalcHeight(metin, alan.width) > alan.height) yaziStili.fontSize -= 2;
            GUI.Label(alan, metin, yaziStili);
        }
        GUI.color = Color.white;
        GUI.matrix = eski;
    }
}
