// BulmaZorlugu.cs
// Eşyaları bulmanın kat kat zorlaşması (#102). Şahan: "Nesneler çok kolay bulunuyor; bulunma olaylarını kat
// kat zorlaştır."
//
// Bulmayı kolaylaştıran iki yardım var ve ikisi de bugüne kadar her katta AYNI güçteydi:
//   1. PARILTI (#83): görev eşyası kafa lambasının ışığında küçük bir yıldız gibi parlar (FenerParlamasi).
//   2. TAKILMA İPUCU (#98): oyuncu uzun süre ilerleyemezse sıradaki eşyanın yerinden bir gıcırtı gelir,
//      eşya bir an parlar (TakilmaIpucu).
// Bu dosya her kat için bu iki yardımın ne kadar güçlü olacağını söyler. Kural basit: 1. ve 2. kat öğreticidir
// (yardımlar tam), sonra her kat biraz daha az yardım eder. 8. katta parıltı ancak eşyaya iki metre
// yaklaşınca, soluk biçimde görünür; ipucu da daha geç ve daha az gelir.
//
// Neden bir tablo? Her değer elle ayarlanabilsin ve tek bakışta okunabilsin diye. Bir formül (örneğin
// "her kat %10 az") ayarlamayı zorlaştırırdı: 1. ve 2. katın aynı kalmasını, 8. katın biraz daha sert
// düşmesini bir formüle anlatmak tabloyu yazmaktan uzun sürer.
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir (BulmaZorluguTestleri).

public static class BulmaZorlugu
{
    // Dizilerin 0. elemanı 1. kat, 7. elemanı 8. kattır (dizi: aynı türden değerlerin numaralı sırası).

    // Parıltının gücünün çarpanı (1 = tam güç, 0 = hiç parlamaz).
    private static readonly float[] parlamaCarpani = { 1.00f, 1.00f, 0.85f, 0.75f, 0.60f, 0.50f, 0.40f, 0.30f };
    // Parıltının görüldüğü en uzak mesafe (metre). 1. katta oda boyu (5 m), 8. katta iki adım.
    private static readonly float[] parlamaMenzili = { 5.0f, 5.0f, 4.5f, 4.0f, 3.5f, 3.0f, 2.5f, 2.0f };
    // İlk takılma ipucuna kadar geçmesi gereken ilerlemesiz süre (saniye).
    private static readonly float[] ilkIpucu = { 150f, 150f, 165f, 180f, 200f, 220f, 240f, 270f };
    // Sonraki ipuçları arası (saniye).
    private static readonly float[] ipucuAra = { 75f, 75f, 80f, 90f, 100f, 110f, 120f, 120f };
    // Bir ilerleme döneminde en çok bu kadar ipucu.
    private static readonly int[] enCokIpucu = { 4, 4, 4, 3, 3, 3, 2, 2 };

    /// <summary>Kat numarasından dizi sırası. 1'den küçük kat 1. kat, 8'den büyük kat 8. kat sayılır
    /// (zemin kat 0 ya da bozuk bir değer oyunu bozmasın; en yakın katın değeri kullanılır).</summary>
    private static int Sira(int kat)
    {
        if (kat < 1) return 0;
        if (kat > 8) return 7;
        return kat - 1;
    }

    /// <summary>Bu katta parıltının gücü bu sayıyla çarpılır (0-1).</summary>
    public static float ParlamaCarpani(int kat) { return parlamaCarpani[Sira(kat)]; }

    /// <summary>Bu katta parıltının görüldüğü en uzak mesafe (metre).</summary>
    public static float ParlamaMenzili(int kat) { return parlamaMenzili[Sira(kat)]; }

    /// <summary>Bu katta ilk takılma ipucuna kadar beklenen ilerlemesiz süre (saniye).</summary>
    public static float IlkIpucu(int kat) { return ilkIpucu[Sira(kat)]; }

    /// <summary>Bu katta sonraki takılma ipuçları arasındaki süre (saniye).</summary>
    public static float IpucuAra(int kat) { return ipucuAra[Sira(kat)]; }

    /// <summary>Bu katta bir ilerleme döneminde verilecek en çok takılma ipucu.</summary>
    public static int EnCokIpucu(int kat) { return enCokIpucu[Sira(kat)]; }
}
