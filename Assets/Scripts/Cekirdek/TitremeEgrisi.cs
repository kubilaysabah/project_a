// TitremeEgrisi.cs
// Lambaların normal katta, her an ne kadar parlak yanacağını söyleyen kural (#38).
//
// Şahan'ın isteği (2026-09-26): "ışıklar çok az yansın", seçimi: loş ve arada titreyen.
//
// İKİ ŞART:
//   1. HER KATTA AYNI: sahnede üst üste birçok sahanlık var. Titreme her lambada ayrı
//      rastgele olsaydı, bir sahanlık titrerken öbürü titremezdi; oyuncu bunu anomali
//      sanardı, ışınlanmada da ışık bir anda zıplardı. Bu yüzden titreme ZAMANIN bir
//      fonksiyonu: aynı saniyede bütün lambalar aynı değeri alır. Gerçekte de bir binada
//      voltaj düşünce bütün ampuller birlikte kararır; "şebeke titremesi" gibi.
//   2. ANOMALİDEN AYIRT EDİLEBİLİR: "titreyen_ampul" kartında ampul TAMAMEN söner ve
//      yanar, hızlı ve peş peşe. Normal titreme ise hiçbir zaman sönmez (en az %40) ve
//      seyrektir (ortalama 14 sn'de bir kısa bir kararma).
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir.
// Lambalara uygulama işi Mekan/LambaTitremesi.cs'de.

public static class TitremeEgrisi
{
    /// <summary>Kararma anındaki en düşük parlaklık oranı. Hiçbir zaman sönmez.</summary>
    public const float EnAz = 0.4f;

    /// <summary>Zamanın bölündüğü dilimlerin uzunluğu (saniye). Bir kararma bir dilim sürer.</summary>
    public const float DilimSuresi = 0.25f;

    /// <summary>Bir dilimin kararma dilimi olma olasılığı. 0,018 × 4 dilim/sn ≈ 14 sn'de bir.</summary>
    public const float KararmaOlasiligi = 0.018f;

    /// <summary>
    /// t saniyesinde lambanın parlaklık oranı (EnAz ile 1 arası). 1 = tam şiddet.
    /// Aynı t her zaman aynı değeri verir; bütün lambalar aynı saati kullanır.
    /// </summary>
    public static float Deger(float t)
    {
        // Sürekli, çok hafif bir kıpırtı (en fazla %4): eski tesisatın "canlı" ışığı.
        // İki farklı hızda sinüsün çarpımı düzenli bir nabız gibi durmasın diye.
        double tt = t;
        float kipirti = 0.04f * (float)(0.5 + 0.5 * System.Math.Sin(2 * System.Math.PI * 0.9 * tt) * System.Math.Sin(2 * System.Math.PI * 2.3 * tt));
        float deger = 1f - kipirti;

        // Kararma: zaman dilimlere bölünür, her dilim için "rastgele ama sabit" bir sayı
        // üretilir (Karistir). Sayı küçükse o dilim bir kararmadır.
        long dilim = (long)System.Math.Floor(tt / DilimSuresi);
        if (Karistir(dilim, 1) < KararmaOlasiligi)
        {
            // Kararmanın derinliği de dilime bağlı: 0,4 ile 0,7 arası.
            float derinlik = EnAz + 0.3f * Karistir(dilim, 2);
            // Dilim içindeki yer (0-1). Kararma yumuşak iner ve çıkar; ortada iki küçük
            // sıçrama var, ampul tel gibi "tutunmaya" çalışır.
            float u = (float)(tt / DilimSuresi - dilim);
            float zarf = (float)System.Math.Sin(System.Math.PI * u);             // 0 -> 1 -> 0
            float sicrama = 0.5f + 0.5f * (float)System.Math.Cos(2 * System.Math.PI * 3 * u);
            float hedef = derinlik + (1f - derinlik) * 0.35f * sicrama;
            deger *= 1f - zarf * (1f - hedef);
        }

        return deger < EnAz ? EnAz : (deger > 1f ? 1f : deger);
    }

    // Bir tam sayıyı 0-1 arası, dağınık görünen bir sayıya çevirir. Aynı girdi hep aynı
    // sonucu verir (System.Random'dan farkı: sıraya bağlı değil, istenen dilim doğrudan
    // hesaplanır). "tuz" aynı dilimden farklı sayılar almak için.
    // Yöntem: bitleri kaydırıp çarparak karıştırmak (yaygın bir "hash" karıştırıcısı).
    private static float Karistir(long dilim, int tuz)
    {
        unchecked   // taşma olursa hata verme, sarılsın: karıştırmanın parçası
        {
            ulong x = (ulong)(dilim * 2654435761L + tuz * 40503L);
            x ^= x >> 33; x *= 0xff51afd7ed558ccdUL;
            x ^= x >> 33; x *= 0xc4ceb9fe1a85ec53UL;
            x ^= x >> 33;
            return (x >> 40) / (float)(1UL << 24);   // üstteki 24 bit -> 0-1
        }
    }
}
