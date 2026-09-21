// MekanOlculeri.cs
// Huzur Apartmanı'nın bütün ölçüleri TEK bir yerde.
//
// Neden ayrı bir dosya?
// Ölçüler önce GrayboxKurucu'nun içindeydi. Ama o dosya "Editor" klasöründe duruyor
// ve Unity'de Editor klasöründeki kod SADECE editörde çalışır; oyun çalışırken
// yok sayılır. Dolayısıyla oyun içi kod oradan bir sayı okuyamaz.
// Işınlanma scriptinin bu sayılara ihtiyacı olduğu için ölçüleri buraya,
// hem editörün hem oyunun görebildiği ortak bir yere taşıdık.
//
// Kural: bu sayılar başka hiçbir dosyada elle yazılmaz. Bir sayı iki yerde durursa,
// biri değiştiğinde diğeri sessizce yanlışa döner ve bu hatayı bulmak çok zordur.

// "static" sınıf: nesnesi üretilemez, sadece içindeki değerler okunur.
// Bir ölçü cetveli gibi; kullanmak için kopyasını almaya gerek yok.
public static class MekanOlculeri
{
    // ---------- TEMEL ÖLÇÜLER (metre) ----------

    public const float KatYuksekligi   = 3.0f;    // iki sahanlık arasındaki dikey mesafe
    public const int   BasamakSayisi   = 16;      // bir kat merdivenindeki basamak adedi
    public const float BasamakDerinlik = 0.27f;   // basamağın ayak basılan derinliği
    public const float SahanlikGen     = 2.6f;    // sahanlık genişliği (x ekseni)
    public const float SahanlikDerin   = 2.0f;    // sahanlık derinliği (z ekseni)
    public const float DuvarYuksek     = 2.8f;    // sahanlıktaki tavan yüksekliği
    public const float DuvarKalin      = 0.2f;    // duvar ve döşeme kalınlığı

    // ---------- HESAPLANAN ÖLÇÜLER ----------
    // Bunlar yukarıdakilerden türetilir; elle yazılmaz ki tutarsızlık olmasın.

    public const float BasamakYuksek   = KatYuksekligi / BasamakSayisi;    // 0.1875 m
    public const float MerdivenUzunluk = BasamakSayisi * BasamakDerinlik;  // 4.32 m

    // ---------- DÖNGÜ OFSETİ ----------
    // Bir sahanlıktan bir üsttekine giderken kat edilen mesafe.
    // Işınlanma scripti oyuncuyu TAM olarak bu kadar geri taşır.

    public const float IleriOfset = SahanlikDerin + MerdivenUzunluk;   // 6.32 m
    public const float DikeyOfset = KatYuksekligi;                     // 3.00 m

    // ---------- IŞINLANMA EŞİKLERİ ----------
    // Bu iki sayı, oyuncunun hangi noktada geri taşınacağını belirler.
    // Dikkat: sahne orijinde kurulu ve merdiven +z yönünde çıkıyor;
    // bu eşikler dünya z koordinatıdır.

    // Üst sahanlığın oyuncuya bakan kenarı.
    public const float UstSahanlikKenari = SahanlikDerin / 2f + MerdivenUzunluk;   // 5.32 m

    // Yukarı çıkarken: oyuncu üst sahanlığa bu kadar adım attığında geri taşınır.
    // Tam kenarda değil biraz içeride tetikleniyor; böylece ışınlanma sonrası
    // oyuncu eşiğin güvenli tarafında kalır ve ileri geri zıplamaz.
    public const float UstIsinlanmaEsigi = UstSahanlikKenari + 0.18f;              // 5.50 m

    // Aşağı inerken: oyuncu alt sahanlığın kenarını bu kadar geçtiğinde ileri taşınır.
    public const float AltIsinlanmaEsigi = -(SahanlikDerin / 2f + 0.2f);           // -1.20 m

    /// <summary>
    /// Merdivenin tam ortası. Gecikmeli ceza burada tetiklenir: oyuncu yanlış
    /// karardan sonra yarım kat yol aldığında ışıklar sönmeye başlar.
    /// Yukarı giderken bu noktaya ALTTAN, aşağı inerken ÜSTTEN yaklaşılır.
    /// </summary>
    public const float MerdivenOrtasiZ = SahanlikDerin / 2f + MerdivenUzunluk / 2f;  // 3.16 m
}
