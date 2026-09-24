// AdimOlcer.cs
// Oyuncunun yürüdüğü mesafeden "şimdi bir adım atıldı" anlarını çıkaran kural (#25).
// Ayak sesi bileşeni her karede buraya o karede ne kadar yürüdüğünü söyler; burası
// "adım attın, sesi çal" ya da "henüz değil" diye cevap verir.
//
// Bu dosyada Unity'ye ait tek satır yok (KatSayaci gibi): kural testle korunabilsin.
//
// ÜÇ KURAL
//   1. Yatayda ADIM_BOYU kadar yol alınınca bir adım sayılır; artan kısım bir
//      sonraki adıma devreder. Yavaş yürüyen de hızlı yürüyen de aynı mesafede
//      adım atar; hızlı yürüyenin adımları sadece daha sık gelir.
//   2. Tek karede SICRAMA_SINIRI'ndan fazla yol "yürümek" değildir: ışınlanma ya da
//      ceza sonrası başa alınma. Sayılmaz ve biriken yarım adımı da sıfırlar.
//   3. Oyuncu yerde değilse (düşüyorsa) adım sayılmaz.

public class AdimOlcer
{
    /// <summary>İki adım arasındaki yatay mesafe (metre). Rahat bir yürüyüş adımı.</summary>
    public const float AdimBoyu = 0.6f;

    /// <summary>Tek karede bundan fazla yol yürüme değil, ışınlanmadır.</summary>
    public const float SicramaSiniri = 1.0f;

    // Son adımdan beri birikmiş mesafe.
    private float biriken;

    /// <summary>Son adımdan beri kaç metre yüründü (test ve hata ayıklama için).</summary>
    public float Biriken { get { return biriken; } }

    /// <summary>
    /// Bu karede yatayda alınan yolu bildirir. Bir adım tamamlandıysa true döner.
    /// Bir karede birden fazla adım tamamlanamaz: 1 metreden fazlası zaten sıçrama sayılıyor.
    /// </summary>
    public bool Ilerle(float yatayMesafe, bool yerde)
    {
        // Işınlanma veya başa alınma: sayma, yarım kalan adımı da unut.
        if (yatayMesafe > SicramaSiniri)
        {
            biriken = 0f;
            return false;
        }

        // Havadayken (merdivenden düşerken) ayak yere değmiyor.
        if (!yerde) return false;

        biriken += yatayMesafe;

        if (biriken >= AdimBoyu)
        {
            biriken -= AdimBoyu;   // artan kısım bir sonraki adıma devreder
            return true;
        }
        return false;
    }
}
