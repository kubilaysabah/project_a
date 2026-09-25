// MenuGezgini.cs
// Menüde klavye ve oyun koluyla gezinmenin kuralları (#29).
//
// İki küçük parça:
//   MenuGezgini       → hangi öğenin seçili olduğunu tutar; aşağı/yukarı hareket
//                       ettirir, en alttan bir aşağı en üste döner (ve tersi).
//   TekrarZamanlayici → bir yön tuşu basılı tutulunca seçimin ne zaman tekrar
//                       ilerleyeceğine karar verir: ilk basışta hemen, sonra kısa
//                       bir beklemeden sonra düzenli aralıklarla. Bilgisayar
//                       klavyesinde bir harfe basılı tutunca olan şeyin aynısı.
//
// Bu dosyada Unity'ye ait tek satır yok (AyarKurallari gibi): kural testle korunabilsin.
// Zaman dışarıdan verilir (saniye); Unity tarafı gerçek zamanı (unscaledTime) verir,
// çünkü menü açıkken oyun saati durmuş olur.

public class MenuGezgini
{
    private int oge;      // menüdeki öğe sayısı
    private int secili;   // seçili öğenin sırası (0 = en üst)

    public MenuGezgini(int ogeSayisi)
    {
        OgeSayisiniAyarla(ogeSayisi);
    }

    /// <summary>Seçili öğenin sırası (0 en üstteki).</summary>
    public int Secili { get { return secili; } }

    /// <summary>Menüdeki öğe sayısı.</summary>
    public int OgeSayisi { get { return oge; } }

    /// <summary>
    /// Öğe sayısını değiştirir (menü sayfa değiştirince). Seçim yeni sınırların
    /// dışında kaldıysa en son öğeye çekilir.
    /// </summary>
    public void OgeSayisiniAyarla(int ogeSayisi)
    {
        oge = ogeSayisi < 1 ? 1 : ogeSayisi;   // boş menü olmaz; en az bir öğe
        if (secili >= oge) secili = oge - 1;
    }

    /// <summary>Belirli bir öğeyi seçer (sınırlar içinde tutulur).</summary>
    public void Sec(int sira)
    {
        secili = sira < 0 ? 0 : (sira >= oge ? oge - 1 : sira);
    }

    /// <summary>
    /// Seçimi yon kadar kaydırır: +1 aşağı, -1 yukarı. Uçlarda karşı uca döner.
    /// </summary>
    public void Tasi(int yon)
    {
        // % : bölümden kalan. Eksi sayıların kalanı C#'ta eksi çıkar (-1 % 4 = -1),
        // bu yüzden önce oge eklenip sonra kalan alınıyor: (-1 + 4) % 4 = 3.
        secili = ((secili + yon) % oge + oge) % oge;
    }
}

public class TekrarZamanlayici
{
    /// <summary>İlk basıştan sonra tekrarın başlaması için beklenen süre (saniye).</summary>
    public const float IlkBekleme = 0.4f;

    /// <summary>Tekrar başladıktan sonra iki ilerleme arası (saniye).</summary>
    public const float Aralik = 0.12f;

    private bool basiliydi;          // bir önceki çağrıda tuş basılı mıydı
    private float sonrakiZaman;      // bir sonraki tekrarın zamanı

    /// <summary>
    /// Her karede çağrılır. basili: yön şu an basılı mı; zaman: şu anki saat (saniye).
    /// Bu karede seçim ilerlemeliyse true döner.
    /// </summary>
    public bool Guncelle(bool basili, float zaman)
    {
        if (!basili)
        {
            basiliydi = false;   // bırakıldı: bir sonraki basış yine "ilk basış" sayılır
            return false;
        }

        if (!basiliydi)
        {
            // İlk basış: hemen ilerle, tekrar için bekle.
            basiliydi = true;
            sonrakiZaman = zaman + IlkBekleme;
            return true;
        }

        if (zaman >= sonrakiZaman)
        {
            // Basılı tutuluyor ve sırası geldi. Bir sonraki zamanı "şimdi + aralık"
            // değil "planlanan + aralık" yapmak, kare hızı ne olursa olsun tekrarın
            // düzenli kalmasını sağlar.
            sonrakiZaman += Aralik;
            if (sonrakiZaman < zaman) sonrakiZaman = zaman + Aralik;   // uzun bir takılmadan sonra birikme olmasın
            return true;
        }

        return false;
    }
}
