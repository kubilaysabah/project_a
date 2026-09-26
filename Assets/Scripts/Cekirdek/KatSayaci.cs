// KatSayaci.cs
// Oyunun kalbi: kat sayacı ve karar kuralı.
//
// Bu dosyada Unity'ye ait TEK BİR SATIR yok. MonoBehaviour değil, GameObject bilmiyor,
// sahneden haberi yok. Sebebi şu: bu mantığı Unity'yi hiç açmadan, saniyeler içinde
// test edebilmek istiyoruz. Oyunun en kritik kuralı burada; burada bir hata olursa
// oyun bozulur. O yüzden bu kısmı otomatik testlerle koruma altına alıyoruz.
//
// Sahneyle bağlantıyı ayrı bir MonoBehaviour kuracak. O köprü görevi görecek:
// oyuncunun yürüyüşünü izleyip buraya "yukarı gitti" veya "geri döndü" diye haber verecek.

// ---------- ENUM NEDİR ----------
// "enum", sınırlı sayıdaki seçeneğe isim veren bir tiptir.
// Kararı bir sayıyla (0 = yukarı, 1 = geri) veya metinle ("yukari") de tutabilirdik.
// Ama sayı tutarsak 5 yazmak da mümkün olur ve bu anlamsızdır;
// metin tutarsak "yukarı", "Yukari", "yukari" hepsi farklı olur ve hata kaçar.
// enum ile derleyici bize garanti verir: bu değişken SADECE bu iki değerden birini alabilir.

/// <summary>Oyuncunun sahanlıkta verdiği karar.</summary>
public enum Karar
{
    Yukari,   // üst kata devam etti
    Geri      // aşağı döndü
}

/// <summary>Kararın sonucunda ne olduğu.</summary>
public enum KararSonucu
{
    Ilerle,           // doğru karar: bir üst kata geçildi
    Sifirla,          // yanlış karar: 1. kata dönülecek
    CatiyaUlasildi    // son kat da hatasız geçildi: çatı kapısı açılır
}

/// <summary>
/// Kat sayacı ve karar değerlendirmesi. Unity'den bağımsızdır.
/// </summary>
public class KatSayaci
{
    // ---------- SABİTLER ----------

    /// <summary>Çatıya çıkmak için hatasız geçilmesi gereken kat.</summary>
    public const int CatiKati = 8;

    /// <summary>Oyunun ve her cezanın başladığı kat.</summary>
    public const int BaslangicKati = 1;

    // ---------- DURUM ----------
    // Aşağıdaki yazım şekline "özellik" (property) denir.
    // "get;" = dışarıdan OKUNABİLİR.
    // "private set;" = sadece BU SINIFIN İÇİNDEN yazılabilir.
    // Böylece başka bir kod yanlışlıkla "sayac.MevcutKat = 8;" yazıp oyunu bozamaz.
    // Kat değiştirmenin tek yolu aşağıdaki KararVer fonksiyonundan geçer.

    /// <summary>Oyuncunun şu an bulunduğu kat (1 ile 8 arası).</summary>
    public int MevcutKat { get; private set; } = BaslangicKati;

    /// <summary>Şu anki sahanlıkta anomali var mı?</summary>
    public bool AnomaliVar { get; private set; }

    /// <summary>Oyun boyunca kaç yanlış karar verildi. İstatistik ve zorluk ayarı için.</summary>
    public int ToplamHata { get; private set; }

    /// <summary>Çatıya ulaşıldı mı? Ulaşıldıysa tur bitmiştir.</summary>
    public bool TurBitti { get; private set; }

    // ---------- KAYITTAN YÜKLE (#52) ----------

    /// <summary>
    /// Oyunu kaldığı kattan sürdürür: sayaç verilen katta, hatasız, tur bitmemiş başlar.
    /// Aralık dışı bir değer sınıra çekilir; bozuk bir kayıt dosyası oyunu 9. kata ya da
    /// 0. kata düşürmesin.
    /// </summary>
    public void Yukle(int kat)
    {
        if (kat < BaslangicKati) kat = BaslangicKati;
        if (kat > CatiKati) kat = CatiKati;
        MevcutKat = kat;
        AnomaliVar = false;
        TurBitti = false;
    }

    // ---------- SAHANLIĞI HAZIRLA ----------

    /// <summary>
    /// Yeni sahanlığın anomalili mi normal mi olduğunu bildirir.
    /// Anomali sistemi (#5) her kat başında bunu çağıracak.
    /// </summary>
    public void KatiHazirla(bool anomaliVar)
    {
        AnomaliVar = anomaliVar;
    }

    // ---------- KARAR ----------

    /// <summary>
    /// Oyuncunun kararını değerlendirir ve sayacı günceller.
    /// </summary>
    /// <param name="karar">Yukarı devam mı, geri dönüş mü.</param>
    /// <param name="gorevEksik">
    /// O katta yapılması gereken bir iş (çöp poşetini almak gibi) yapılmadıysa true.
    /// Doğru karar bile olsa eksik görev cezalandırılır: kapıcı işini yapmadan
    /// üst kata çıkamaz. Katta anomali varsa görev aranmaz — anomali gördüğünde
    /// işini bırakıp geri dönmek zaten doğru davranıştır. Bu ayrımı çağıran taraf
    /// yapar; burası sadece gelen bayrağa bakar.
    /// </param>
    public KararSonucu KararVer(Karar karar, bool gorevEksik = false)
    {
        // Tur bittiyse artık karar alınmaz; çağıran kod yanlış yapıyordur.
        if (TurBitti) return KararSonucu.CatiyaUlasildi;

        // DÖRT KOMBİNASYON, tek satırda:
        //   normal  + yukarı  -> DOĞRU
        //   normal  + geri    -> yanlış
        //   anomali + geri    -> DOĞRU
        //   anomali + yukarı  -> yanlış
        // Yani: anomali varsa doğru cevap Geri, yoksa doğru cevap Yukari.
        bool dogruKarar = AnomaliVar
            ? (karar == Karar.Geri)      // "?" işaretinin solundaki koşul doğruysa bu değer,
            : (karar == Karar.Yukari);   // yanlışsa bu değer alınır. Kısa if-else yazımıdır.

        // Görev eksikse doğru karar bile yetmez. Kapıcı çöpü almadan
        // üst kata çıkmışsa işini yapmamıştır ve ceza alır.
        if (gorevEksik) dogruKarar = false;

        if (!dogruKarar)
        {
            ToplamHata++;
            MevcutKat = BaslangicKati;
            return KararSonucu.Sifirla;
        }

        MevcutKat++;

        // Son kat da hatasız geçildiyse çatıya ulaşıldı.
        if (MevcutKat > CatiKati)
        {
            MevcutKat = CatiKati;   // sayaç 8'in üstüne çıkmasın
            TurBitti = true;
            return KararSonucu.CatiyaUlasildi;
        }

        return KararSonucu.Ilerle;
    }

    // ---------- SIFIRLAMA ----------

    /// <summary>Oyunu baştan başlatır. Yeni tur veya yeniden oyna için.</summary>
    public void Sifirla()
    {
        MevcutKat = BaslangicKati;
        AnomaliVar = false;
        ToplamHata = 0;
        TurBitti = false;
    }
}
