// Altyazi.cs
// Ekranın altında tek satırlık alt yazı (#118): radyo/TV anonslarının cümlesi. Seslendirme kaydı olmayan katta
// cihaz yalnızca cızırdıyordu; oyuncu hiçbir şey öğrenemiyordu. Kayıt gelince de alt yazı kalır: Steam
// oyuncusu konuşmaları yazıyla da okumak ister (erişilebilirlik).
//
// KULLANIM: Altyazi.Goster(metin, sure). Her çağıran kendi metnini verir; bu sınıf yalnızca çizer. Yeni bir
// alt yazı eskisinin yerini alır. Süre oyun saatiyle akar: menüde durur, menüyle birlikte kaybolmaz.
//
// ÇİZİM: OnGUI, ArayuzOlcegi kuralıyla (#34: sanal 1080 yüksekliğinde ekran). Koyu şerit üstünde açık yazı;
// başlık ekranında ve final sahnesinde çizilmez. GUI.depth -150: menünün (-200) altında, oyun yazılarının üstünde.

using UnityEngine;

public class Altyazi : MonoBehaviour
{
    private static Altyazi ornek;
    private string metin = "";
    private float kalan;
    private GUIStyle stil;

    /// <summary>Alt yazıyı 'sure' saniye gösterir (sure ≤ 0: metin uzunluğuna göre, en az 4 sn).</summary>
    public static void Goster(string yazi, float sure = 0f)
    {
        if (string.IsNullOrEmpty(yazi)) return;
        if (ornek == null)
        {
            GameObject g = new GameObject("Altyazi");
            ornek = g.AddComponent<Altyazi>();
        }
        ornek.metin = yazi;
        // Okuma süresi: saniyede ~14 karakter (Türkçe haber cümlesi için rahat), en az 4 sn.
        ornek.kalan = sure > 0f ? sure : Mathf.Max(4f, yazi.Length / 14f);
    }

    /// <summary>Şu an bir alt yazı görünüyor mu (test için).</summary>
    public static bool Gorunuyor { get { return ornek != null && ornek.kalan > 0f; } }
    /// <summary>Görünen metin (test için).</summary>
    public static string Metin { get { return ornek != null && ornek.kalan > 0f ? ornek.metin : ""; } }

    private void Update()
    {
        if (kalan > 0f) kalan -= Time.deltaTime;
    }

    private void OnGUI()
    {
        if (kalan <= 0f || DuraklatmaMenusu.BaslikEkraninda) return;
        GUI.depth = -150;
        ArayuzOlcegi.Uygula();
        if (stil == null)
        {
            stil = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            stil.normal.textColor = new Color(0.95f, 0.93f, 0.86f);
        }
        float en = Mathf.Min(1100f, ArayuzOlcegi.Genislik - 80f);
        float yuk = stil.CalcHeight(new GUIContent(metin), en - 40f) + 24f;
        Rect r = new Rect((ArayuzOlcegi.Genislik - en) / 2f, ArayuzOlcegi.Yukseklik - yuk - 70f, en, yuk);
        // Koyu, yarı saydam şerit: açık renkli yazı her sahnede okunsun.
        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(r.x + 20f, r.y, r.width - 40f, r.height), metin, stil);
    }
}
