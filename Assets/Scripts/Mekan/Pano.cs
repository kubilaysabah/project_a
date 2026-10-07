// Pano.cs
// Kapıcı odasının mantar panosu (#56): oyuncunun okuduğu her not ve telefon konuşması buraya
// iğnelenir. Panoda okunan not sayısı kadar kâğıt görünür; bakınca "OKU (E)", basınca liste açılır:
// solda notların ilk satırı, sağda seçilenin tamamı. Yukarı/aşağı (W/S, oklar, kolda d-pad) seçer,
// E ya da Esc (kolda A ya da B) kapatır.
//
// Not ekranının kalıbı (YoneticiNotu): açılınca oyuncunun kontrolü ve etkileşimi kapanır, ortak
// Interact eylemi yeniden açılır, kontrol LateUpdate'te geri verilir.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pano : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private Material kagitMalzemesi;
    private const int EnCokKagit = 12;

    private readonly List<GameObject> kagitlar = new List<GameObject>();
    private bool acik, geriVerilecek;
    private int acilisKaresi, secili;
    private List<string> liste = new List<string>();
    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;
    private GUIStyle baslikStili, satirStili, yaziStili;

    // Pano her açıldığında (zemin kat etkinleşince) okunan not kadar kâğıt iğnelenir.
    private void OnEnable() { KagitlariDiz(); }

    private void KagitlariDiz()
    {
        int n = Mathf.Min(EnCokKagit, Kayit.OkunanNotlar().Count);
        while (kagitlar.Count < n)
        {
            int i = kagitlar.Count;
            GameObject k = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(k.GetComponent<Collider>());   // nişan panoya gelsin
            k.name = "Kagit_" + i;
            k.transform.SetParent(transform, false);
            // Dört sütun, üç satır; her kâğıt biraz eğik (elle iğnelenmiş gibi).
            float x = -0.36f + (i % 4) * 0.24f, y = 0.2f - (i / 4) * 0.22f;
            k.transform.localPosition = new Vector3(x, y, 0.012f);
            k.transform.localRotation = Quaternion.Euler(0f, 180f, ((i * 37) % 11) - 5f);
            k.transform.localScale = new Vector3(0.16f, 0.19f, 1f);
            if (kagitMalzemesi != null) k.GetComponent<MeshRenderer>().sharedMaterial = kagitMalzemesi;
            kagitlar.Add(k);
        }
    }

    public string Ipucu { get { return Dil.Getir("ipucu.oku"); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.Goz; } }   // #104

    public bool EtkilesimeAcik { get { return !acik && Kayit.OkunanNotlar().Count > 0; } }

    public void Etkilesim(GameObject oyuncu)
    {
        liste = Kayit.OkunanNotlar();
        if (liste.Count == 0) return;
        secili = Mathf.Clamp(secili, 0, liste.Count - 1);
        acik = true;
        acilisKaresi = Time.frameCount;
        kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
        etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();
        KagitlariDiz();
    }

    private void Update()
    {
        if (!acik || Time.frameCount <= acilisKaresi) return;
        var kb = Keyboard.current;
        var kol = Gamepad.current;
        bool yukari = (kb != null && (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)) || (kol != null && kol.dpad.up.wasPressedThisFrame);
        bool asagi = (kb != null && (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)) || (kol != null && kol.dpad.down.wasPressedThisFrame);
        if (yukari) secili = (secili - 1 + liste.Count) % liste.Count;
        if (asagi) secili = (secili + 1) % liste.Count;
        bool kapat = (etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame())
                  || (kb != null && kb.escapeKey.wasPressedThisFrame) || (kol != null && kol.buttonEast.wasPressedThisFrame);
        if (kapat) { acik = false; geriVerilecek = true; }
    }

    private void LateUpdate()
    {
        if (!geriVerilecek) return;
        geriVerilecek = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
    }

    // Notun ilk satırı, biçim etiketleri (<size=..>) atılarak: listedeki başlık.
    private static string Baslik(string anahtar)
    {
        string metin = Regex.Replace(Dil.Getir(anahtar), "<[^>]+>", "");
        foreach (string satir in metin.Split('\n'))
        {
            string t = satir.Trim();
            if (t.Length > 0) return t.Length > 38 ? t.Substring(0, 36) + "…" : t;
        }
        return anahtar;
    }

    private void OnGUI()
    {
        if (!acik) return;
        ArayuzOlcegi.Uygula();
        float W = ArayuzOlcegi.Genislik, H = ArayuzOlcegi.Yukseklik;
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture);
        if (baslikStili == null)
        {
            baslikStili = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            baslikStili.normal.textColor = new Color(0.92f, 0.9f, 0.84f);
            satirStili = new GUIStyle(GUI.skin.label) { fontSize = 19 };
            yaziStili = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true };
            yaziStili.normal.textColor = new Color(0.12f, 0.10f, 0.08f);
        }
        GUI.color = Color.white;
        GUI.Label(new Rect(0, H / 2f - 330f, W, 50f), string.Format(Dil.Getir("pano.baslik"), liste.Count), baslikStili);

        // Sol: liste.
        float solX = W / 2f - 560f, y = H / 2f - 260f;
        for (int i = 0; i < liste.Count && i < 16; i++)
        {
            bool sec = i == secili;
            if (sec)
            {
                GUI.color = new Color(0.95f, 0.72f, 0.35f, 0.35f);
                GUI.DrawTexture(new Rect(solX - 8f, y, 440f, 30f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            satirStili.normal.textColor = sec ? new Color(1f, 0.95f, 0.85f) : new Color(0.7f, 0.68f, 0.62f);
            GUI.Label(new Rect(solX, y, 430f, 30f), Baslik(liste[i]), satirStili);
            y += 32f;
        }

        // Sağ: seçili notun kâğıdı.
        Rect kagit = new Rect(W / 2f - 80f, H / 2f - 270f, 620f, 520f);
        GUI.color = KagitDokusu.Renk;                                       // #128: kâğıt dokusu
        GUI.DrawTexture(kagit, KagitDokusu.Al() ?? Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(kagit.x + 32f, kagit.y + 28f, kagit.width - 64f, kagit.height - 56f), Dil.Getir(liste[secili]), yaziStili);

        var ipucu = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        ipucu.normal.textColor = new Color(0.8f, 0.78f, 0.72f);
        bool kolda = GirisCihazi.Kol;
        GUI.Label(new Rect(0, H / 2f + 280f, W, 30f), string.Format(Dil.Getir("pano.tuslar"), string.Format(Dil.Getir(kolda ? "tus.kapat_kol" : "tus.kapat_klavye"), kolda ? Tuslar.KolAdi(TusEylemi.Etkilesim) : Tuslar.KlavyeAdi(TusEylemi.Etkilesim))), ipucu);   // #125
    }
}
