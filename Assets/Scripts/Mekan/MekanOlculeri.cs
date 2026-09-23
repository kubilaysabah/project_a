// MekanOlculeri.cs
// Huzur Apartmanı'nın bütün ölçüleri TEK bir yerde.
//
// Neden ayrı bir dosya?
// Editor klasöründeki kod SADECE editörde çalışır; oyun çalışırken yok sayılır.
// Işınlanma scripti gibi oyun içi kodların da bu sayılara ihtiyacı var, o yüzden
// ölçüler hem editörün hem oyunun görebildiği bu ortak dosyada duruyor.
//
// Kural: bu sayılar başka hiçbir dosyada elle yazılmaz. Bir sayı iki yerde durursa,
// biri değiştiğinde diğeri sessizce yanlışa döner ve bu hatayı bulmak çok zordur.
//
// ---------- YERLEŞİM (2026-09-23, #23) ----------
// Merdiven artık YARIM DÖNÜŞLÜ KARE bir merdiven. Yukarıdan bakınca:
//
//        z ↑
//          |   +--------------------------+
//          |   |      ARA SAHANLIK        |   (+1,5 m)   arkada pencere
//          |   +--------+-------+---------+
//          |   | 2. KOL |  göz  | 1. KOL  |
//          |   | (iner  |       | (çıkar  |
//          |   |  gibi  |       |  +z'ye) |
//          |   |  -z'ye)|       |         |
//     z=0  +---+--------+-------+---------+---+
//          |          KAT SAHANLIĞI            |   4 daire kapısı yan duvarlarda
//          |                                   |
//          +-----------[ ASANSÖR ]-------------+   z = -5
//                           x →
//
// Oyuncu sahanlıktan sağdaki 1. kolla ara sahanlığa çıkar, orada geri döner,
// soldaki 2. kolla bir üst katın sahanlığına varır. Üst katın sahanlığı TAM
// OLARAK bu sahanlığın 3 m üstündedir. Döngü hilesi bu yüzden artık yalnızca
// dikey: oyuncuyu 3 m aşağı almak yetiyor, ileri geri taşımaya gerek kalmadı.

// "static" sınıf: nesnesi üretilemez, sadece içindeki değerler okunur.
// Bir ölçü cetveli gibi; kullanmak için kopyasını almaya gerek yok.
public static class MekanOlculeri
{
    // ---------- KAT ----------

    public const float KatYuksekligi   = 3.0f;    // iki kat sahanlığı arasındaki dikey mesafe
    public const float DuvarYuksek     = 2.8f;    // döşemeden tavana net yükseklik
    public const float DuvarKalin      = 0.2f;    // duvar ve döşeme kalınlığı

    // DuvarYuksek + DuvarKalin = KatYuksekligi olmalı: bir katın tavanı,
    // bir üst katın döşemesinin alt yüzüdür. İkisi ayrı ayrı kurulsaydı
    // aynı yerde iki kutu üst üste biner ve yüzeyler titreşirdi.

    // ---------- BASAMAK ----------

    public const int   KolBasamakSayisi = 8;      // bir merdiven kolundaki basamak adedi
    public const float BasamakDerinlik  = 0.27f;  // basamağın ayak basılan derinliği

    // Bir kat iki kolla çıkılıyor; her kol yarım kat yükseltiyor.
    public const float BasamakYuksek = KatYuksekligi / (2 * KolBasamakSayisi);   // 0.1875 m
    public const float KolUzunluk    = KolBasamakSayisi * BasamakDerinlik;       // 2.16 m
    public const float AraKot        = KatYuksekligi / 2f;                       // 1.5 m

    // ---------- MERDİVEN ÇEKİRDEĞİ ----------
    // İki kol yan yana, aralarında "merdiven gözü" denen boşluk var.
    // Göz sayesinde oyuncu korkuluktan eğilip aşağıdaki ve yukarıdaki
    // katları görebiliyor.

    public const float KolGen          = 1.4f;    // bir merdiven kolunun genişliği
    public const float KuyuGen         = 0.8f;    // merdiven gözünün genişliği
    public const float AraSahanlikDerin = 1.4f;   // dönüş sahanlığının derinliği (z)

    public const float CekirdekGen   = 2f * KolGen + KuyuGen;          // 3.6 m
    public const float CekirdekDerin = KolUzunluk + AraSahanlikDerin;  // 3.56 m

    // 1. kol (çıkan) sağda, 2. kol (varan) solda. Kolların orta çizgileri:
    public const float CikisKoluX = KuyuGen / 2f + KolGen / 2f;        // +1.1 m
    public const float VarisKoluX = -CikisKoluX;                       // -1.1 m

    // Korkuluğun basamak üstünden yüksekliği.
    public const float KorkulukYuk = 0.95f;

    // ---------- KAT SAHANLIĞI ----------
    // Kat başına dört daire: iki yan duvarda ikişer kapı, karşılıklı.

    public const float SahanlikGen   = 7.0f;    // x ekseni
    public const float SahanlikDerin = 5.0f;    // z ekseni; sahanlık z = -5 ile 0 arasında

    public const int DaireSayisi = 4;

    // Kapı merkezlerinin z konumları. Yakın kapı merdivene, uzak kapı asansöre yakın.
    public const float YakinKapiZ = -1.5f;
    public const float UzakKapiZ  = -3.5f;

    public const float KapiGen  = 0.90f;   // kapı boşluğunun genişliği
    public const float KapiYuk  = 2.05f;   // kapı boşluğunun yüksekliği

    // ---------- DÖNGÜ ----------

    /// <summary>Işınlanmada oyuncunun taşındığı dikey mesafe: tam bir kat.</summary>
    public const float DikeyOfset = KatYuksekligi;   // 3.00 m

    /// <summary>
    /// Yukarı çıkarken ışınlanma kotu. Oyuncu üst katın sahanlığına ayak bastığı
    /// anda (en son basamak sahanlıkla aynı kotta) 3 m aşağı alınır.
    ///
    /// Eşik neden yarım basamak aşağıda? CharacterController basamağa TEK karede
    /// çıkar: y bir karede 2,81'den 3,00'e zıplar. Eşiği iki değerin ortasına
    /// koyunca, ışınlanma tam olarak oyuncunun sahanlığa vardığı karede olur.
    /// O kare aynı zamanda kat kararının verildiği karedir; ikisi aynı karede
    /// olduğunda sahanlığın yenilenmesi ekranda hiç görünmez (bkz. KatYoneticisi).
    /// </summary>
    public const float UstIsinlanmaKotu = KatYuksekligi - BasamakYuksek / 2f;   // 2.906 m

    /// <summary>
    /// Aşağı inerken ışınlanma kotu: oyuncu alttaki kola üç basamak kadar
    /// indiğinde 3 m yukarı alınır ve kendini aynı noktada, bir kat yukarıda bulur.
    ///
    /// İki eşik arasında boşluk bırakmak ŞART. Yukarı ışınlanan oyuncu y≈0'a
    /// düşer, alt eşik -0,6'da; aşağı ışınlanan oyuncu 2,4'e çıkar, üst eşik
    /// 2,9'da. Eşikler bitişik olsaydı oyuncu iki nokta arasında titrerdi.
    /// </summary>
    public const float AltIsinlanmaKotu = -3.2f * BasamakYuksek;                // -0.6 m

    /// <summary>
    /// Oyuncunun başladığı nokta: 2. kolun tepesinden sahanlığa yeni adım atmış,
    /// yüzü sahanlığa dönük. Her kata aynı yerden varıldığı için 1. kat da
    /// diğer yedi kat gibi görünür.
    /// </summary>
    public const float OyuncuBaslangicX = VarisKoluX;   // -1.1 m
    public const float OyuncuBaslangicZ = -0.6f;
}
