// AnomaliSecici.cs
// Hangi katta anomali çıkacağına ve hangi kartın geleceğine karar verir.
//
// Bu dosyada Unity'ye ait tek satır yok. Kartları `IAnomaliKart` sözleşmesi
// üzerinden tanıyor, dolayısıyla testlerde sahte kartlarla çalışabiliyor.
//
// Seçicinin çözmesi gereken dört sorun var:
//   1. Bu kat anomalili mi, normal mi?
//   2. Anomaliliyse hangi kart?
//   3. Aynı kart üst üste gelmemeli, yoksa oyuncu ezberler.
//   4. Kolay kartlar başta, zor kartlar sonda gelmeli.

using System.Collections.Generic;

public class AnomaliSecici
{
    // ---------- AYARLAR ----------

    private readonly IAnomaliKart[] kartlar;
    private readonly float anomaliOlasiligi;
    private readonly bool ilkKatHepNormal;

    // System.Random: kendi tohumunu verebildiğimiz rastgelelik kaynağı.
    // UnityEngine.Random yerine bunu kullanıyoruz çünkü o Unity'ye bağlı ve
    // testlerde aynı sonucu tekrarlatmak mümkün olmuyor. Aynı tohum,
    // her zaman aynı diziyi üretir; testler bu sayede kararlı olur.
    private readonly System.Random rastgele;

    // Son kullanılan kartların kimlikleri. Yeni seçimde bunlar elenir.
    private readonly List<string> sonKullanilanlar = new List<string>();

    // Kaç kart geriye bakılacağı. Kart sayısının yarısından fazla olamaz,
    // yoksa eleyecek kart kalmaz.
    private readonly int hafizaBoyu;

    // ---------- KAT HAFIZASI ----------
    // Bir kat için verilen karar SAKLANIR. Sebebi şu: merdivenin tepesindeki
    // sahanlık bir ÜST katı gösteriyor. Oyuncu oraya vardığında o katın
    // anomalisi yeniden seçilseydi, ışınlanma anında sahanlık değişir ve
    // hile açığa çıkardı. Aynı kat iki kez sorulduğunda aynı cevap dönmeli.
    //
    // Dictionary: "anahtar → değer" eşlemesi tutan tip. Burada anahtar kat
    // numarası, değer o katın kartı. Bir rehberdeki isim-numara eşlemesi gibi.
    private readonly Dictionary<int, IAnomaliKart> katKararlari = new Dictionary<int, IAnomaliKart>();

    // ---------- KURUCU ----------

    /// <param name="kartlar">Kullanılabilecek bütün anomali kartları.</param>
    /// <param name="tohum">Rastgeleliğin tohumu. Aynı tohum aynı oyunu üretir.</param>
    /// <param name="anomaliOlasiligi">Bir katın anomalili olma olasılığı (0-1).</param>
    /// <param name="ilkKatHepNormal">1. kat her zaman normal mi olsun?</param>
    public AnomaliSecici(IAnomaliKart[] kartlar,
                         int tohum,
                         float anomaliOlasiligi = 0.5f,
                         bool ilkKatHepNormal = true)
    {
        // null gelirse boş diziye çevir; her yerde null kontrolü yapmaktan kurtuluruz.
        this.kartlar = kartlar ?? new IAnomaliKart[0];
        this.anomaliOlasiligi = anomaliOlasiligi;
        this.ilkKatHepNormal = ilkKatHepNormal;

        rastgele = new System.Random(tohum);

        // Hafıza, kart sayısının yarısını geçmesin.
        hafizaBoyu = this.kartlar.Length / 2;
        if (hafizaBoyu > 5) hafizaBoyu = 5;
        if (hafizaBoyu < 0) hafizaBoyu = 0;
    }

    // ---------- SEÇİM ----------

    /// <summary>
    /// Verilen kat için anomali kartını döndürür. Kat normalse null döner.
    /// Aynı kat iki kez sorulursa aynı cevap gelir.
    /// </summary>
    public IAnomaliKart KatIcinSec(int kat)
    {
        // Daha önce karar verilmişse aynısını döndür.
        if (katKararlari.TryGetValue(kat, out IAnomaliKart hazir)) return hazir;

        IAnomaliKart secilen = KararVer(kat);
        katKararlari[kat] = secilen;

        if (secilen != null) HafizayaEkle(secilen);

        return secilen;
    }

    private IAnomaliKart KararVer(int kat)
    {
        // 1. kat ÖĞRETİR. Oyuncunun anomaliyi fark edebilmesi için önce
        // normalin neye benzediğini görmesi gerekir. Bu yüzden ilk kat
        // varsayılan olarak her zaman temizdir.
        if (ilkKatHepNormal && kat <= KatSayaci.BaslangicKati) return null;

        if (kartlar.Length == 0) return null;

        // NextDouble 0 ile 1 arasında bir sayı verir.
        if (rastgele.NextDouble() >= anomaliOlasiligi) return null;

        return KartSec(kat);
    }

    private IAnomaliKart KartSec(int kat)
    {
        int zorlukTavani = ZorlukTavani(kat);

        // Önce en dar eleme: hem zorluk sınırına uyan hem de yakında
        // kullanılmamış kartlar.
        List<IAnomaliKart> adaylar = Adaylar(zorlukTavani, hafizayiUygula: true);

        // Aday kalmadıysa sırayla gevşetiyoruz. Hiçbir zaman "kart bulamadım"
        // deyip boş dönmemeliyiz; oyun anomalisiz kalırsa kural çöker.
        if (adaylar.Count == 0) adaylar = Adaylar(zorlukTavani, hafizayiUygula: false);
        if (adaylar.Count == 0) adaylar = Adaylar(5, hafizayiUygula: true);
        if (adaylar.Count == 0) adaylar = Adaylar(5, hafizayiUygula: false);

        if (adaylar.Count == 0) return null;   // hiç kart yok

        return adaylar[rastgele.Next(adaylar.Count)];
    }

    private List<IAnomaliKart> Adaylar(int zorlukTavani, bool hafizayiUygula)
    {
        List<IAnomaliKart> liste = new List<IAnomaliKart>();

        foreach (IAnomaliKart kart in kartlar)
        {
            if (kart == null) continue;
            if (kart.Zorluk > zorlukTavani) continue;
            if (hafizayiUygula && sonKullanilanlar.Contains(kart.Kimlik)) continue;

            liste.Add(kart);
        }

        return liste;
    }

    // ---------- ZORLUK EĞRİSİ ----------

    /// <summary>
    /// O katta izin verilen en yüksek zorluk. Alt katlarda kolay, üst
    /// katlarda zor kartlar gelir; böylece oyuncu önce gözünü eğitir.
    /// </summary>
    public static int ZorlukTavani(int kat)
    {
        if (kat < 1) kat = 1;

        // Kat 1 -> 1, kat 8 -> 5 olacak şekilde yukarı yuvarlayarak ölçekle.
        // (kat * 5 + tavan - 1) / tavan  tam sayı bölmesiyle yukarı yuvarlar.
        int tavan = KatSayaci.CatiKati;
        int sonuc = (kat * 5 + tavan - 1) / tavan;

        if (sonuc < 1) sonuc = 1;
        if (sonuc > 5) sonuc = 5;
        return sonuc;
    }

    // ---------- HAFIZA ----------

    private void HafizayaEkle(IAnomaliKart kart)
    {
        sonKullanilanlar.Add(kart.Kimlik);

        // Liste hafıza boyunu aşarsa en eskisini at.
        while (sonKullanilanlar.Count > hafizaBoyu)
        {
            sonKullanilanlar.RemoveAt(0);
        }
    }

    // ---------- SIFIRLAMA ----------

    /// <summary>
    /// Yanlış karardan sonra çağrılır. Kat kararları silinir, böylece
    /// oyuncu aynı turu ezberleyemez.
    ///
    /// Son kullanılanlar listesi BİLEREK korunur: sıfırlamanın hemen
    /// ardından aynı kartın tekrar gelmesi en çok göze batacağı andır.
    /// </summary>
    public void Sifirla()
    {
        katKararlari.Clear();
    }

    /// <summary>Kaç kat için karar verilmiş olduğu. Test ve hata ayıklama için.</summary>
    public int KararVerilenKatSayisi { get { return katKararlari.Count; } }
}
