// GolgeZamanlayici.cs
// Gezen Gölge'nin ne zaman belireceğini belirleyen kural (#71).
//
// Şahan'ın isteği (2026-09-27): "Tüm katlara gölgeyi ekleyelim; kattan çıkamadığımız her 1-2
// dakikada bir gölge birden belirsin, hasarı olmasın, bizi öldürmesin, 2 sn bizi kovalayıp
// kaybolsun."
//
// Kurallar:
//   - İki beliriş arası 60 ile 120 saniye arasında rastgele bir süredir. Her belirişten sonra
//     yeni bir süre çekilir: oyuncu bir düzen yakalayıp saniye sayamasın.
//   - Saat YALNIZCA oyuncu katta sıkışıkken işler (sahne tarafı "etkin" der: açılışta, menüde,
//     not okurken, kat kapısı açıldıktan sonra saat durur). Duran saat kaldığı yerden devam eder.
//   - Yeni bir kata geçilince saat baştan kurulur: oyuncu kata varır varmaz Gölge'yle karşılaşmaz.
//   - Gölge için uygun bir yer bulunamazsa (oyuncu duvara dönük durur) kısa bir süre sonra yeniden
//     denenir (Ertele).
//
// Bu dosyada Unity'ye ait tek satır yok: kural, Unity açılmadan test edilir (KatOlayPlani gibi).
// Gölge'yi sahnede belirtme işi GezenGolge'de; burası yalnızca "şimdi mi?" sorusunu cevaplar.

public class GolgeZamanlayici
{
    /// <summary>İki beliriş arasındaki en kısa ve en uzun süre (saniye).</summary>
    public const float EnAz = 60f, EnCok = 120f;

    // Tohumlu üretici: aynı tohum aynı süreleri verir (CopPlani ile aynı sebep: bir oyunu
    // tekrarlamak ya da test etmek mümkün olsun).
    private readonly System.Random rastgele;
    private float kalan;          // bir sonraki belirişe kalan süre
    private int kat = -1;         // saatin kurulduğu kat; -1 = henüz hiçbiri

    public GolgeZamanlayici(int tohum)
    {
        rastgele = new System.Random(tohum);
        YeniSure();
    }

    /// <summary>Bir sonraki belirişe kalan süre (saniye).</summary>
    public float Kalan { get { return kalan; } }

    /// <summary>
    /// Her kare çağrılır. gecen: bu karenin süresi; mevcutKat: oyuncunun bulunduğu kat;
    /// etkin: saat şu an işlemeli mi (oyuncu katta sıkışık mı). Dönüş: Gölge ŞİMDİ belirsin mi?
    /// </summary>
    public bool Ilerlet(float gecen, int mevcutKat, bool etkin)
    {
        // Yeni kat: saat baştan kurulur (etkin olmasa da; kata varış anı saati sıfırlar).
        if (mevcutKat != kat)
        {
            kat = mevcutKat;
            YeniSure();
        }
        if (!etkin || gecen <= 0f) return false;
        kalan -= gecen;
        if (kalan > 0f) return false;
        // Süre doldu: Gölge belirir, bir sonraki için yeni süre çekilir.
        YeniSure();
        return true;
    }

    /// <summary>Gölge için yer bulunamadı: bu kadar saniye sonra yeniden denensin.</summary>
    public void Ertele(float saniye)
    {
        kalan = saniye > 0f ? saniye : 1f;
    }

    // NextDouble: 0 ile 1 arasında rastgele bir sayı. EnAz + (0..1) × aralık = EnAz..EnCok.
    private void YeniSure()
    {
        kalan = EnAz + (float)rastgele.NextDouble() * (EnCok - EnAz);
    }
}
