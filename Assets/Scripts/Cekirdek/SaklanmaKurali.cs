// SaklanmaKurali.cs
// Gardıroba saklanma (#97): hangi mobilya saklanma yeri olur, göz içeride nereye konur?
//
// Bu dosya düz C#'tır (Unity'ye dokunmaz); bu yüzden testle denenebilir (SaklanmaKuraliTestleri).
// Sahnedeki yarısı SaklanmaYeri bileşenidir: mobilyanın ölçüsünü ölçer, buraya sorar.
//
// KURAL
// Bir mobilya saklanma yeridir, eğer:
//   1. adı bir gardırop ya da boy dolabı adıysa (aşağıdaki anahtar kelimeler), ve
//   2. adı dışlananlardan değilse (kapakları ardına kadar açık gardırop, klasörle dolu arşiv dolabı), ve
//   3. bir insan içine sığacak kadar büyükse: en az 1,6 m boy, 0,45 m derinlik, 0,55 m genişlik.
// Ad tek başına yetmez: "Tel_Dolap" 80 cm'lik bir mutfak dolabıdır, içine kimse giremez.

public static class SaklanmaKurali
{
    // Ölçü sınırları (metre). Yetişkin bir insan omuz omuza ~45 cm, ayakta ~1,6 m yer ister.
    public const float EnAzYukseklik = 1.6f;
    public const float EnAzDerinlik = 0.45f;
    public const float EnAzGenislik = 0.55f;

    // Göz, ön yüzün (kapakların) bu kadar arkasına konur: kapağın aralığından bakıyormuş gibi.
    public const float OnPayi = 0.15f;
    // #127: gözün kapak düzleminin ÖNÜNDE durduğu pay. Göz içeride (ön yüzün 15 cm arkasında) durunca kamera
    // kapalı kapakların iç yüzüne bakıyordu: oda hiç görünmüyor, "aralık" kaplaması siyah gösteriyordu.
    public const float DisPayi = 0.06f;
    // Göz yüksekliği en çok bu kadar; alçak bir dolapta tavanın 15 cm altı.
    public const float GozTavani = 1.55f;
    public const float TavanPayi = 0.15f;

    // Av Gölgesi mobilyanın önüne bu kadar yakınsa saklanılamaz (gözünün önünde dolaba girilmez).
    public const float GolgeYakinligi = 1.5f;

    // Gizliyken etrafa bakma sınırları (derece): kapak aralığından biraz sağa-sola, biraz yukarı-aşağı.
    public const float YatayBakisSiniri = 35f;
    public const float DikeyBakisSiniri = 20f;

    // Adında bunlardan biri geçen mobilya aday olur (küçük harfle aranır).
    // "dolap" ve "dolab": Türkçede ünlüyle başlayan ek gelince p b'ye döner (dolap → dolabı).
    private static readonly string[] AnahtarKelimeler =
        { "gardirop", "dolap", "dolab", "bufe", "wardrobe", "armoire", "painted_wooden_cabinet", "gothiccabinet" };

    // Adında bunlardan biri geçen mobilya aday OLMAZ.
    //   acik_        : 1. kattaki açık gardırop; kapakları ardına kadar açık, içinde saklanmak saklanmak değil.
    //   arsiv_dolabi : 8. kattaki arşiv dolabı; üstü camlı, içi klasörle dolu ve açılmıyor.
    // "buzdolab": buzdolabının adında da "dolab" geçer ama içi raf ve kompresör; insan sığmaz (oyun içi
    // denetimde iki buzdolabı saklanma yeri çıkmıştı).
    private static readonly string[] Dislananlar = { "acik_", "arsiv_dolabi", "buzdolab" };

    /// <summary>
    /// Bu ad ve ölçüdeki mobilya saklanma yeri mi? genislik: modelin sağ-sol boyu, derinlik: önden
    /// arkaya (modelin +Z ekseni), yukseklik: yerden tepeye; hepsi metre.
    /// </summary>
    public static bool Aday(string ad, float genislik, float derinlik, float yukseklik)
    {
        if (string.IsNullOrEmpty(ad)) return false;
        // ToLowerInvariant: dilden bağımsız küçük harf. Türkçe ayarlı bir bilgisayarda ToLower "I"yı
        // "ı" yapar; nesne adları ASCII olduğu için dilden bağımsız olanı kullanırız.
        string kucuk = ad.ToLowerInvariant();
        bool kelimeVar = false;
        foreach (string k in AnahtarKelimeler) if (kucuk.Contains(k)) { kelimeVar = true; break; }
        if (!kelimeVar) return false;
        foreach (string d in Dislananlar) if (kucuk.Contains(d)) return false;
        return yukseklik >= EnAzYukseklik && derinlik >= EnAzDerinlik && genislik >= EnAzGenislik;
    }

    /// <summary>Gözün yerden yüksekliği: en çok 1,55 m, alçak dolapta tepenin 15 cm altı.</summary>
    public static float GozYuksekligi(float yukseklik)
    {
        float tavanAlti = yukseklik - TavanPayi;
        return tavanAlti < GozTavani ? tavanAlti : GozTavani;
    }

    /// <summary>Bir açıyı ±sinir arasında tutar (bakış sınırı).</summary>
    public static float Sinirla(float aci, float sinir)
    {
        if (aci > sinir) return sinir;
        if (aci < -sinir) return -sinir;
        return aci;
    }
}
