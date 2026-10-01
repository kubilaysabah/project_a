// IncelemeEkrani.cs
// Eşya inceleme (#88): bir eşyaya bakarken F (kolda X) eşyanın bir kopyasını gözün önüne getirir;
// fare (sağ çubuk) ile çevrilir, E (A) alır, F / Esc (X / B) bırakır.
//
// Şahan'ın No70 karşılaştırması (2026-10-01): "eşyaları eline alıp çevirerek incelemek yok; No70'de
// anahtarı, fotoğrafı döndürüp bakmak hem hissi hem ipucunu taşıyor."
//
// Nasıl çalışır: eşyanın kendisi yerinde kalır; yalnızca görüntüsünün bir KOPYASI (UcanKopya'daki
// gibi çarpışması ve bileşenleri sökülmüş) kameranın çocuğu yapılır, 42 cm önüne konur ve en uzun
// kenarı 16 cm olacak şekilde ölçeklenir. Kameranın çocuğu olduğu için kafa lambasının huzmesinin
// tam ortasındadır: karanlık odada bile aydınlık görünür. Çevirme: fare deltası kameranın yukarı
// ve sağ eksenleri etrafında döndürür (dünya ekseni değil; oyuncu nereye bakarsa baksın "sağa
// çekince sağa döner").
//
// Yakın bakış kalıbı (FotografEkrani, YoneticiNotu): kontrolcü ve Etkilesimci kapanır, paylaşılan
// Interact eylemi yeniden açılır, kontrol LateUpdate'te geri verilir (aynı tuş aynı karede oyun
// bileşenine ulaşmasın). Alma: E'ye basılınca ekran kapanır ve LateUpdate'te eşyanın kendi
// Etkilesim'i çağrılır; böylece anahtar da, parça da, tabak da kendi kuralıyla alınır.

using UnityEngine;
using UnityEngine.InputSystem;

public class IncelemeEkrani : MonoBehaviour
{
    private static IncelemeEkrani ornek;

    /// <summary>İnceleme şu an açık mı?</summary>
    public static bool Acik { get { return ornek != null && ornek.acik; } }

    private const float Uzaklik = 0.42f;       // kameradan (metre)
    private const float EnUzunKenar = 0.16f;   // kopyanın ekrandaki boyu (metre)
    private const float FareDerecesi = 0.4f;   // 1 piksel = bu kadar derece
    private const float CubukHizi = 140f;      // derece/sn tam itişte

    private bool acik, geriVerilecek, alinacak;
    private int acilisKaresi;
    private Esya esya;
    private GameObject oyuncu, kopya;
    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;
    private KafaLambasi lamba;
    private string ad = "";

    /// <summary>Eşyayı incelemeye açar; oyuncunun kontrolünü kapatır.</summary>
    public static void Ac(Esya e, GameObject oyuncu)
    {
        if (ornek == null)
        {
            var g = new GameObject("Inceleme_Ekrani");
            DontDestroyOnLoad(g);
            ornek = g.AddComponent<IncelemeEkrani>();
        }
        ornek.AcIc(e, oyuncu);
    }

    private void AcIc(Esya e, GameObject oyuncuNesnesi)
    {
        esya = e;
        oyuncu = oyuncuNesnesi;
        acik = true;
        alinacak = false;
        acilisKaresi = Time.frameCount;
        kontrolcu = oyuncu != null ? oyuncu.GetComponent<BirinciSahisKontrolcu>() : null;
        etkilesimci = oyuncu != null ? oyuncu.GetComponent<Etkilesimci>() : null;
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();

        // Eşyanın adı (T listesiyle aynı kural, EsyaAdlari).
        EsyaAdi a = EsyaAdlari.Coz(e.Kimlik, e.Tur);
        string ic = a.Ic != null ? Dil.Getir(a.Ic) : "";
        ad = string.Format(Dil.Getir(a.Ana), ic, a.Sayi);

        KopyaKur(e.transform);
        // Lamba kısılır: 42 cm'deki eşya tam şiddette bembeyaz patlıyordu (ekran görüntüsüyle görüldü).
        lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.IncelemeKismasi(true);
    }

    // Görüntü kopyası: çarpışma, bileşen ve parıltı sökülür; kameranın önüne ölçeklenir.
    private void KopyaKur(Transform kaynak)
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;
        kopya = Instantiate(kaynak.gameObject);
        kopya.name = "Inceleme_Kopyasi";
        // Bileşenler ANINDA sökülür (DestroyImmediate): Destroy kare sonunu bekler ve o zamana kadar
        // kopyanın Esya/parıltı bileşenleri çalışıp onu oynatabilirdi.
        foreach (Collider c in kopya.GetComponentsInChildren<Collider>(true)) DestroyImmediate(c);
        foreach (MonoBehaviour m in kopya.GetComponentsInChildren<MonoBehaviour>(true)) DestroyImmediate(m);
        Transform parilti = kopya.transform.Find("Parilti");
        if (parilti != null) DestroyImmediate(parilti.gameObject);
        kopya.SetActive(true);

        // Önce ölçek: dünya sınır kutusunun en uzun kenarı EnUzunKenar olsun.
        Bounds b = SinirKutusu(kopya);
        float enUzun = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        float olcek = enUzun > 0.001f ? EnUzunKenar / enUzun : 1f;
        kopya.transform.SetParent(kamera.transform, false);
        kopya.transform.localScale = kaynak.lossyScale * olcek;
        kopya.transform.localRotation = Quaternion.Euler(-20f, 30f, 0f);   // hafif eğik: üç boyut okunur
        kopya.transform.localPosition = new Vector3(0f, -0.03f, Uzaklik);
        // Sonra merkezleme: kutunun merkezi tam istenen noktada dursun (pivot merkezde olmayabilir).
        Vector3 istenen = kamera.transform.TransformPoint(new Vector3(0f, -0.03f, Uzaklik));
        kopya.transform.position += istenen - SinirKutusu(kopya).center;
    }

    private static Bounds SinirKutusu(GameObject g)
    {
        Bounds b = new Bounds(g.transform.position, Vector3.zero);
        bool ilk = true;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (ilk) { b = r.bounds; ilk = false; } else b.Encapsulate(r.bounds);
        }
        return b;
    }

    private void Update()
    {
        if (!acik || Time.frameCount <= acilisKaresi) return;   // açan tuş aynı karede kapatmasın

        // Çevirme: fare deltası ya da sağ çubuk.
        Vector2 delta = Vector2.zero;
        if (Mouse.current != null) delta = Mouse.current.delta.ReadValue() * FareDerecesi;
        if (Gamepad.current != null)
        {
            Vector2 cubuk = Gamepad.current.rightStick.ReadValue();
            if (cubuk.magnitude > 0.2f) delta += cubuk * CubukHizi * Time.deltaTime;
        }
        if (kopya != null && delta.sqrMagnitude > 0f)
        {
            Transform k = kopya.transform.parent;   // kamera
            // Sağa çekince eşya sağa döner (kameranın yukarı ekseni etrafında eksi), yukarı çekince
            // üstü geriye yatar (kameranın sağ ekseni etrafında).
            kopya.transform.Rotate(k.up, -delta.x, Space.World);
            kopya.transform.Rotate(k.right, delta.y, Space.World);
        }

        bool al = etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame();
        bool kapat = (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
                  || (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame));
        if (al) { alinacak = true; Kapat(); }
        else if (kapat) Kapat();
    }

    private void Kapat()
    {
        acik = false;
        geriVerilecek = true;
        if (kopya != null) Destroy(kopya);
        kopya = null;
        if (lamba != null) lamba.IncelemeKismasi(false);
    }

    private void LateUpdate()
    {
        if (!geriVerilecek) return;
        geriVerilecek = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
        // Alma: eşyanın kendi kuralı (anahtar ele, parça çizime, tabak yığına).
        if (alinacak && esya != null && esya.EtkilesimeAcik) esya.Etkilesim(oyuncu);
        alinacak = false;
        esya = null;
    }

    private static GUIStyle adStili;

    private void OnGUI()
    {
        if (!acik) return;
        ArayuzOlcegi.Uygula();
        if (adStili == null)
        {
            adStili = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            adStili.normal.textColor = new Color(0.92f, 0.9f, 0.84f);
        }
        float y = ArayuzOlcegi.Yukseklik - 120f;
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(new Rect(ArayuzOlcegi.Genislik / 2f - 260f, y - 6f, 520f, 86f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, y, ArayuzOlcegi.Genislik, 40f), ad, adStili);
        bool kol = GirisCihazi.Kol;
        GUIStyle ipucu = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        ipucu.normal.textColor = new Color(0.8f, 0.78f, 0.72f);
        GUI.Label(new Rect(0, y + 42f, ArayuzOlcegi.Genislik, 30f),
                  string.Format(Dil.Getir("inceleme.tuslar"), kol ? "A" : "E", kol ? "X" : "F"), ipucu);
    }
}
