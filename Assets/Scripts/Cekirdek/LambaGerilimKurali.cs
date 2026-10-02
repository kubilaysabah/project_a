// LambaGerilimKurali.cs
// Kafa lambasının gerilime nasıl tepki vereceğinin kuralı (#105): diyegetik gerilim göstergesi.
//
// NEDEN
// Binanın varlığı bir sayıyla (GerilimKurali, 0-1) yaklaşır ama oyuncu o sayıyı hiç görmez; av
// "durup dururken" başlıyormuş gibi hissedilir. Phasmophobia'nın korkusu kuralların öğrenilmesinden
// gelir: oyuncu "şunu yaparsam gelir" diye bilir. Ekrana yazı ya da çubuk koymak yasak (#72, #86:
// gerilim arayüzde gösterilmez). Onun yerine oyuncunun elindeki tek alet konuşur: gerilim arttıkça
// kafa lambası hafifçe SARARIR, GÜÇSÜZLEŞİR ve lambadan ince bir elektrik VINLAMASI yükselir. Geiger
// sayacı gibi, ama yazısız. Gerilim düşünce lamba toparlar.
//
// Eşikler GerilimKurali'nin kademeleriyle hizalı: 0,30 (titreme), 0,60 (Gölge), 0,85 (av).
//   - Sararma 0,15'te başlar, 0,85'te tamdır: oyuncu ilk kademeden önce fark etmeye başlar.
//   - Vınlama 0,25'te başlar, 0,85'te tamdır: biraz daha geç, çünkü ses rengin önüne geçmesin.
//   - Güç 1,0'dan 0,72'ye iner: oda kararır ama oyun oynanmaz olmaz.
// Hepsi yumuşak geçişle (smoothstep): kademe sınırında ani bir sıçrama yoktur.
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir (LambaGerilimKuraliTestleri).
// Renk karışımını Light'a uygulayan, sesi çalan KafaLambasi'dir.

public static class LambaGerilimKurali
{
    /// <summary>Sararmanın başladığı ve tamamlandığı gerilim.</summary>
    public const float SararmaBas = 0.15f, SararmaSon = 0.85f;
    /// <summary>Vınlamanın başladığı ve tamamlandığı gerilim.</summary>
    public const float VinlamaBas = 0.25f, VinlamaSon = 0.85f;
    /// <summary>Gerilim tamken lambanın gücü (1 = normal).</summary>
    public const float EnDusukGuc = 0.72f;
    /// <summary>Gerilim tamken vınlamanın perdesi (1 = kayıt perdesi): daha tiz, daha "cıyak".</summary>
    public const float EnTizPerde = 1.08f;

    /// <summary>
    /// Lambanın rengi ne kadar sarıya kaymış (0 = normal sıcak beyaz, 1 = tam sarı)?
    /// gerilim: 0-1 (GerilimKurali.Deger). Bozuk değer (NaN) ve aralık dışı güvenle karşılanır.
    /// </summary>
    public static float Karisim(float gerilim)
    {
        return Yumusak(gerilim, SararmaBas, SararmaSon);
    }

    /// <summary>Lambanın ışık gücü bu sayıyla çarpılır (1 → EnDusukGuc).</summary>
    public static float GucCarpani(float gerilim)
    {
        return 1f - (1f - EnDusukGuc) * Karisim(gerilim);
    }

    /// <summary>Vınlamanın ses gücü (0 = sessiz, 1 = tam; KafaLambasi bunu kendi tavanıyla çarpar).</summary>
    public static float Vinlama(float gerilim)
    {
        return Yumusak(gerilim, VinlamaBas, VinlamaSon);
    }

    /// <summary>Vınlamanın perdesi (1 → EnTizPerde).</summary>
    public static float VinlamaPerdesi(float gerilim)
    {
        return 1f + (EnTizPerde - 1f) * Vinlama(gerilim);
    }

    // Smoothstep: 'bas'ın altında 0, 'son'un üstünde 1, arada S biçiminde yumuşak bir eğri
    // (3t² − 2t³). Düz bir doğru yerine bunu seçmemizin nedeni uçlardaki eğimin sıfır olması:
    // sararma "başladı" diye hissedilen bir an yok, fark edilmeden içine girilir.
    private static float Yumusak(float deger, float bas, float son)
    {
        if (float.IsNaN(deger)) return 0f;                       // bozuk değer: lamba normal kalsın
        if (deger <= bas) return 0f;
        if (deger >= son) return 1f;
        float t = (deger - bas) / (son - bas);
        return t * t * (3f - 2f * t);
    }
}
