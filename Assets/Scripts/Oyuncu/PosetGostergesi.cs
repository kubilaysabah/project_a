// PosetGostergesi.cs
// Taşınan poşet sayısını ekranın sağ alt köşesinde kısa süre gösteren işaret (#43).
//
// Oyunun ilkesi "ekranda yazı yok": kuralı binanın kendisi söyler. Bu yüzden işaret
// küçük ve geçici: yalnızca yeni bir poşet alınınca belirir, 3 saniye durur, yarım
// saniyede solar. İçinde yalnızca bir poşet simgesi ve rakam var; dil gerektirmez.
// Başlık ekranında görünmez. Arayüz ölçekleme kuralına uyar (#34): her çözünürlükte
// ekranın aynı oranını kaplar.
//
// CopToplayici bu bileşeni oyun başlarken kendisi ekler; sahnede ayrıca kurulması gerekmez.

using UnityEngine;

public class PosetGostergesi : MonoBehaviour
{
    private const float GorunmeSuresi = 3f;
    private const float SolmaSuresi = 0.5f;

    private CopToplayici toplayici;
    private int sonSayi;
    private float kalan;                 // işaretin daha ne kadar görüneceği
    private Texture2D simge;
    private GUIStyle yazi;

    private void Awake()
    {
        toplayici = GetComponent<CopToplayici>();
        simge = SimgeUret();
    }

    private void Update()
    {
        if (toplayici == null) return;
        int sayi = toplayici.TasinanPoset;
        if (sayi != sonSayi)
        {
            // Sayı arttıysa (yeni poşet) göster; ceza sonrası sıfırlandıysa sessizce güncelle.
            if (sayi > sonSayi) kalan = GorunmeSuresi;
            sonSayi = sayi;
        }
        if (kalan > 0f) kalan -= Time.unscaledDeltaTime;
    }

    private void OnGUI()
    {
        if (kalan <= 0f || sonSayi <= 0 || DuraklatmaMenusu.BaslikEkraninda) return;
        ArayuzOlcegi.Uygula();

        float saydamlik = Mathf.Clamp01(kalan / SolmaSuresi);   // son yarım saniyede solar
        float w = ArayuzOlcegi.Genislik, h = ArayuzOlcegi.Yukseklik;
        Rect kutu = new Rect(w - 190f, h - 110f, 150f, 64f);

        GUI.color = new Color(0f, 0f, 0f, 0.45f * saydamlik);
        GUI.DrawTexture(kutu, Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.88f, 0.82f, saydamlik);
        GUI.DrawTexture(new Rect(kutu.x + 12f, kutu.y + 8f, 48f, 48f), simge);

        if (yazi == null) yazi = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        yazi.normal.textColor = new Color(0.9f, 0.88f, 0.82f, saydamlik);
        GUI.Label(new Rect(kutu.x + 70f, kutu.y, 80f, kutu.height), "×" + sonSayi, yazi);
        GUI.color = Color.white;
    }

    // Poşet simgesini kodla çiziyor: şişkin bir gövde (elips) ve tepesinde düğüm (üçgen
    // boyun + iki kulak). Beyaz çiziliyor; rengi GUI.color veriyor. 64x64 piksel,
    // kenarlar yumuşak (piksel sınırdan uzaklığına göre saydamlaşıyor).
    private static Texture2D SimgeUret()
    {
        const int N = 64;
        var t = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float u = (x + 0.5f) / N * 2f - 1f;        // -1..1, sağa doğru
                float v = (y + 0.5f) / N * 2f - 1f;        // -1..1, yukarı doğru (dokuda y yukarı)
                // Gövde: alt kısımda geniş bir elips.
                float govde = (u * u) / (0.72f * 0.72f) + ((v + 0.25f) * (v + 0.25f)) / (0.62f * 0.62f);
                // Boyun: tepeye doğru daralan bir üçgen.
                bool boyun = v > 0.2f && v < 0.62f && Mathf.Abs(u) < 0.22f * (0.62f - v) / 0.42f + 0.05f;
                // İki kulak: tepede yana açılan iki küçük elips.
                float k1 = ((u - 0.2f) * (u - 0.2f)) / 0.03f + ((v - 0.72f) * (v - 0.72f)) / 0.012f;
                float k2 = ((u + 0.2f) * (u + 0.2f)) / 0.03f + ((v - 0.72f) * (v - 0.72f)) / 0.012f;
                float a = Mathf.Clamp01((1f - govde) * 8f);
                if (boyun) a = 1f;
                a = Mathf.Max(a, Mathf.Clamp01((1f - k1) * 4f), Mathf.Clamp01((1f - k2) * 4f));
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        return t;
    }
}
