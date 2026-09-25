// ArayuzOlcekKurali.cs
// Ekrandaki arayüzün ne kadar büyütülüp küçültüleceği (#34).
//
// Arayüz (menüler, not, jenerik) 1080 piksel yüksekliğindeki bir ekrana göre tasarlandı:
// 24 piksellik yazı, 360 piksellik düğme orada doğru boyutta. Ekran daha yüksekse
// (1440p, 4K) arayüz aynı oranda büyütülür; daha alçaksa (720p, Steam Deck) küçültülür.
// Böylece arayüz her ekranın aynı oranını kaplar.
//
// Bu dosyada Unity'ye ait tek satır yok: kural testle korunabilsin.

public static class ArayuzOlcekKurali
{
    /// <summary>Arayüzün tasarlandığı ekran yüksekliği (piksel).</summary>
    public const float TasarimYuksekligi = 1080f;

    // Sınırlar: çok küçük bir pencerede yazı okunmaz hâle gelmesin (0,5), çok büyük bir
    // ekranda da anlamsızca şişmesin (4 = 8K). Arada ölçek ekranla doğru orantılı.
    public const float EnAz = 0.5f;
    public const float EnCok = 4f;

    /// <summary>Verilen ekran yüksekliği için arayüz ölçeği (1080 → 1, 2160 → 2).</summary>
    public static float Olcek(int ekranYuksekligi)
    {
        float olcek = ekranYuksekligi / TasarimYuksekligi;
        if (olcek < EnAz) return EnAz;
        if (olcek > EnCok) return EnCok;
        return olcek;
    }
}
