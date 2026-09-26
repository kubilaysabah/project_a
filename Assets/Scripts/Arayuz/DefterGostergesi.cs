// DefterGostergesi.cs
// Yeni bir anomali deftere işlenince sağ altta kısa süre beliren küçük defter simgesi ve
// hafif bir kâğıt sesi (#45).
//
// Şahan'ın seçimi: yazı yok (oyunun "ekranda yazı yok" ilkesi), yalnızca simge ve ses.
// Poşet sayacının (PosetGostergesi) hemen üstünde; 3 saniye durur, yarım saniyede solar.
//
// Sahnede kurulmasına gerek yok: oyun her başladığında kendini oluşturur ve sahne yeniden
// yüklense de (finalden sonra) yaşamaya devam eder.

using UnityEngine;

public class DefterGostergesi : MonoBehaviour
{
    private const float GorunmeSuresi = 3f, SolmaSuresi = 0.5f;

    private float kalan;
    private Texture2D simge;
    private AudioSource kaynak;
    private AudioClip kagit;

    // RuntimeInitializeOnLoadMethod: Unity bu fonksiyonu oyun başlarken kendisi çağırır.
    // AfterSceneLoad: ilk sahne yüklendikten sonra.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Olustur()
    {
        if (FindAnyObjectByType<DefterGostergesi>() != null) return;
        var g = new GameObject("Defter_Gostergesi");
        DontDestroyOnLoad(g);             // sahne yeniden yüklense de silinmesin
        g.AddComponent<DefterGostergesi>();
    }

    private void Awake()
    {
        simge = SimgeUret();
        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;         // 2B
        kagit = Resources.Load<AudioClip>("Sesler/Poset_Hisirti");   // kâğıt hışırtısı: sayfaya yazılıyor
    }

    private void OnEnable() { Defter.YeniKayit += Goster; }
    private void OnDisable() { Defter.YeniKayit -= Goster; }

    private void Goster(string kimlik)
    {
        kalan = GorunmeSuresi;
        if (kagit != null) kaynak.PlayOneShot(kagit, 0.45f);
    }

    private void Update()
    {
        if (kalan > 0f) kalan -= Time.unscaledDeltaTime;
    }

    private void OnGUI()
    {
        if (kalan <= 0f || DuraklatmaMenusu.BaslikEkraninda) return;
        ArayuzOlcegi.Uygula();
        float saydamlik = Mathf.Clamp01(kalan / SolmaSuresi);
        float w = ArayuzOlcegi.Genislik, h = ArayuzOlcegi.Yukseklik;
        Rect kutu = new Rect(w - 190f, h - 186f, 150f, 64f);   // poşet sayacının üstü
        GUI.color = new Color(0f, 0f, 0f, 0.45f * saydamlik);
        GUI.DrawTexture(kutu, Texture2D.whiteTexture);
        GUI.color = new Color(0.95f, 0.72f, 0.35f, saydamlik);     // sıcak turuncu: menüdeki seçim rengi
        GUI.DrawTexture(new Rect(kutu.x + 51f, kutu.y + 8f, 48f, 48f), simge);
        GUI.color = Color.white;
    }

    // Açık bir defter simgesi: iki sayfa, ortada cilt, sayfalarda satır çizgileri. 64x64, beyaz.
    private static Texture2D SimgeUret()
    {
        const int N = 64;
        var t = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float u = (x + 0.5f) / N * 2f - 1f, v = (y + 0.5f) / N * 2f - 1f;
                // Sayfalar: ortadan iki yana hafif yukarı kalkan iki dikdörtgen.
                float egim = Mathf.Abs(u) * 0.15f;
                bool sayfa = Mathf.Abs(u) < 0.9f && v > -0.62f + egim && v < 0.55f + egim;
                bool cilt = Mathf.Abs(u) < 0.04f && v > -0.66f && v < 0.58f;
                // Satırlar: sayfaların içinde, saydam şeritler (simge içi "boşluk").
                bool satir = sayfa && Mathf.Abs(u) > 0.15f && Mathf.Abs(u) < 0.78f &&
                             Mathf.Repeat(v - egim + 0.62f, 0.24f) < 0.07f && v - egim > -0.5f && v - egim < 0.45f;
                float a = (sayfa || cilt) && !satir ? 1f : 0f;
                if (cilt) a = 1f;
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        return t;
    }
}
