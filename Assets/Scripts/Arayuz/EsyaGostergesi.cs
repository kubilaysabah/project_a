// EsyaGostergesi.cs
// Eldeki eşyanın köşedeki küçük simgesi (#52). Yazı yok; anahtar, sigorta, tornavida ve
// not için kodla çizilmiş simgeler. Eşya alınınca belirir, elde durdukça kalır (poşet
// sayacından farkı: solmaz, çünkü "elimde ne var" her an bilinmeli), kullanılınca kaybolur.
// Sağ altta, poşet sayacının solunda. Başlık ekranında görünmez; ölçekleme kuralına uyar (#34).
// Oyun başında kendini oluşturur (DefterGostergesi gibi).

using UnityEngine;

public class EsyaGostergesi : MonoBehaviour
{
    private Texture2D anahtar, sigorta, tornavida, not;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Olustur()
    {
        if (FindAnyObjectByType<EsyaGostergesi>() != null) return;
        var g = new GameObject("Esya_Gostergesi");
        DontDestroyOnLoad(g);
        g.AddComponent<EsyaGostergesi>();
    }

    private void Awake()
    {
        anahtar = Ciz((u, v) =>
        {
            // Yatay anahtar: solda halka, sağa uzanan gövde, sağ altta iki diş.
            float halka = ((u + 0.55f) * (u + 0.55f)) / 0.09f + (v * v) / 0.09f;
            bool halkaCizgi = halka < 1f && halka > 0.35f;
            bool govde = u > -0.28f && u < 0.85f && Mathf.Abs(v) < 0.09f;
            bool dis1 = u > 0.55f && u < 0.68f && v < 0f && v > -0.34f;
            bool dis2 = u > 0.75f && u < 0.86f && v < 0f && v > -0.28f;
            return halkaCizgi || govde || dis1 || dis2;
        });
        sigorta = Ciz((u, v) =>
        {
            // Sigorta: dikdörtgen gövde, üstte küçük kol.
            bool govde = Mathf.Abs(u) < 0.32f && v > -0.7f && v < 0.35f;
            bool kol = Mathf.Abs(u) < 0.1f && v >= 0.35f && v < 0.75f;
            bool pencere = Mathf.Abs(u) < 0.14f && v > -0.4f && v < 0.05f;
            return (govde && !pencere) || kol;
        });
        tornavida = Ciz((u, v) =>
        {
            // Tornavida: çapraz sap ve ince uç.
            float a = (u + v) / 1.414f, b = (u - v) / 1.414f;   // 45° döndürülmüş eksenler
            bool sap = a > -0.85f && a < -0.1f && Mathf.Abs(b) < 0.16f;
            bool uc = a >= -0.1f && a < 0.8f && Mathf.Abs(b) < 0.05f;
            bool agiz = a >= 0.8f && a < 0.9f && Mathf.Abs(b) < 0.1f;
            return sap || uc || agiz;
        });
        not = Ciz((u, v) =>
        {
            bool kagit = Mathf.Abs(u) < 0.55f && Mathf.Abs(v) < 0.72f;
            bool satir = kagit && Mathf.Abs(u) < 0.38f && Mathf.Repeat(v + 0.72f, 0.3f) < 0.08f && v < 0.5f && v > -0.6f;
            return kagit && !satir;
        });
    }

    private void OnGUI()
    {
        if (DuraklatmaMenusu.BaslikEkraninda) return;
        ArayuzOlcegi.Uygula();
        float w = ArayuzOlcegi.Genislik, h = ArayuzOlcegi.Yukseklik;

        // ALETLER (#61): elin simgesinin solunda, daha küçük kutucuklarda, hep görünür.
        var aletler = Aletler.Liste();
        for (int i = 0; i < aletler.Count; i++)
        {
            Rect k = new Rect(w - 270f - 58f * (i + 1), h - 98f, 48f, 48f);
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.DrawTexture(k, Texture2D.whiteTexture);
            GUI.color = new Color(0.75f, 0.78f, 0.80f, 1f);   // çelik grisi
            GUI.DrawTexture(new Rect(k.x + 6f, k.y + 6f, 36f, 36f), tornavida);
        }
        GUI.color = Color.white;

        if (Cep.ElBos) return;
        Rect kutu = new Rect(w - 270f, h - 110f, 64f, 64f);   // poşet sayacının solu
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(kutu, Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.78f, 0.45f, 1f);        // pirinç sarısı
        // Oda anahtarı da kat anahtarı gibi anahtar simgesiyle çizilir; rengi gümüş (aşağıda).
        if (Cep.Tasinan == EsyaTuru.OdaAnahtari) GUI.color = new Color(0.78f, 0.80f, 0.82f, 1f);
        bool anahtarMi = Cep.Tasinan == EsyaTuru.KatAnahtari || Cep.Tasinan == EsyaTuru.OdaAnahtari;
        Texture2D t = anahtarMi ? anahtar : Cep.Tasinan == EsyaTuru.Sigorta ? sigorta
                    : Cep.Tasinan == EsyaTuru.Tornavida ? tornavida : not;
        GUI.DrawTexture(new Rect(kutu.x + 8f, kutu.y + 8f, 48f, 48f), t);
        GUI.color = Color.white;
    }

    // 64x64 beyaz simge; 'icinde' fonksiyonu (-1..1, -1..1) düzleminde pikselin simgede
    // olup olmadığını söyler. Func<float,float,bool>: iki sayı alıp doğru/yanlış dönen fonksiyon.
    private static Texture2D Ciz(System.Func<float, float, bool> icinde)
    {
        const int N = 64;
        var t = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float u = (x + 0.5f) / N * 2f - 1f, v = (y + 0.5f) / N * 2f - 1f;
                t.SetPixel(x, y, new Color(1f, 1f, 1f, icinde(u, v) ? 1f : 0f));
            }
        t.Apply();
        return t;
    }
}
