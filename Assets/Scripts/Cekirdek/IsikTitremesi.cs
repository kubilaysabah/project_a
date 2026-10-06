// IsikTitremesi.cs
// Kat imzası ışıklarının (#116) titreme kuralı. Saf C#: Unity'ye dokunmaz, test edilir (IsikTitremesiTestleri).
//
// NEDEN
// Bina karanlık (Şahan, #111); her katın dairesinde o katı tanıtan zayıf, ELEKTRİKSİZ bir ışık var: mum,
// gaz lambası, ocağın alevi, el feneri, pili bitmemiş bir oyuncak. Her birinin ışığı kendine göre oynar:
// mum titrer, gaz lambası daha sakin dalgalanır, oyuncak iki renk arasında yanıp söner, sokak lambası sabittir.
// Bu dosya "t anında ışık ne kadar güçlü?" sorusunu cevaplar; bileşen (KatIsigiDavranisi) bunu uygular.
//
// Her şey zamanın FONKSİYONUDUR (TitremeEgrisi ve ArizaliLambaKurali gibi): hiçbir şey hatırlanmaz, aynı an
// her zaman aynı görünür. "tohum" iki ışığın aynı anda aynı şeyi yapmasını önler.

/// <summary>Kat ışığının nasıl oynadığı. SONA eklenir: kat dosyaları sayıyı saklar.</summary>
public enum KatIsikTitremesi
{
    Sabit,   // sokak lambası, el feneri: kıpırdamaz
    Mum,     // mum alevi: hızlı, düzensiz titreme
    Alev,    // gaz lambası, ocak: yavaş, sakin dalgalanma
    Yanip,   // pilli oyuncak: iki renk arasında yanıp söner (bkz. IkinciRenk)
}

public static class IsikTitremesi
{
    public const float YanipSuresi = 0.45f;   // oyuncağın bir rengi bu kadar yanar (saniye)

    /// <summary>
    /// t anındaki parlaklık çarpanı (temel şiddetle çarpılır). Sabit 1; Mum 0,55-1,15; Alev 0,8-1,08;
    /// Yanip 0,15-1. Bozuk zaman (NaN, sonsuz) 1 döner: ışık en azından sabit yanar.
    /// </summary>
    public static float Carpan(KatIsikTitremesi tur, float t, int tohum = 0)
    {
        if (float.IsNaN(t) || float.IsInfinity(t)) return 1f;
        float s = t + tohum * 7.31f;   // tohum zamanı kaydırır: iki mum aynı anda titremez
        switch (tur)
        {
            case KatIsikTitremesi.Mum:
                // Üç farklı hızda sinüs toplamı: göz bunu "düzensiz" görür ama yine de bir fonksiyon.
                float m = 0.5f * Sin(s * 9.7f) + 0.3f * Sin(s * 23.1f + 1.3f) + 0.2f * Sin(s * 4.3f + 2.1f);
                return Sinirla(0.85f + 0.30f * m, 0.55f, 1.15f);
            case KatIsikTitremesi.Alev:
                float a = 0.6f * Sin(s * 2.1f) + 0.4f * Sin(s * 5.3f + 0.7f);
                return Sinirla(0.94f + 0.14f * a, 0.80f, 1.08f);
            case KatIsikTitremesi.Yanip:
                // Bir rengin yandığı sürenin ortasında tam parlak; renk değişirken bir an söner.
                float evre = Fraksiyon(s / YanipSuresi);
                return evre < 0.12f || evre > 0.92f ? 0.15f : 1f;
            default:
                return 1f;
        }
    }

    /// <summary>Yanip ışığı şu an ikinci renginde mi (oyuncağın mavisi)? Öbür türlerde hep false.</summary>
    public static bool IkinciRenk(KatIsikTitremesi tur, float t, int tohum = 0)
    {
        if (tur != KatIsikTitremesi.Yanip || float.IsNaN(t) || float.IsInfinity(t)) return false;
        float s = t + tohum * 7.31f;
        // Fraksiyon(x): x'in tam sayı kısmı atılmış hâli (0-1). Her YanipSuresi'nde bir renk değişir.
        return ((long)System.Math.Floor(s / YanipSuresi)) % 2 != 0;
    }

    private static float Sin(float x) { return (float)System.Math.Sin(x); }
    private static float Fraksiyon(float x) { return x - (float)System.Math.Floor(x); }
    private static float Sinirla(float d, float alt, float ust) { return d < alt ? alt : (d > ust ? ust : d); }
}
