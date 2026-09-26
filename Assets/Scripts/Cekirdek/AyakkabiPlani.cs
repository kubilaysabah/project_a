// AyakkabiPlani.cs
// Her katta her dairenin ayakkabılığında kaç çift ayakkabı durduğunu ve kapı önünde kaç
// çiftin dağınık kaldığını belirleyen kural (#44).
//
// Şahan'ın kararı (2026-09-26):
//   - Her dairenin 5 raflı ayakkabılığı var, her rafa bir çift sığar.
//   - Rafta zaten 2-3 çift durur; kapı önünde 0-2 çift ortada kalmıştır.
//   - Kapıcı isterse dağınık çiftleri rafa koyar (isteğe bağlı görev, yapmamak hata değil).
//   - Bir turda her katın sayıları SABİTTİR; rafa konan çiftler hatırlanır (oyuncu kata geri
//     dönerse rafta görür). Cezadan sonra her şey yeniden çekilir.
// Çöp planıyla (CopPlani) aynı düzen; bu dosyada da Unity'ye ait tek satır yok.

using System.Collections.Generic;

public class AyakkabiPlani
{
    /// <summary>Bir ayakkabılıktaki raf sayısı (her rafa bir çift).</summary>
    public const int RafSayisi = 5;
    /// <summary>Rafta başlangıçta en az ve en çok kaç çift durur.</summary>
    public const int RaftaEnAz = 2, RaftaEnCok = 3;
    /// <summary>Kapı önünde en fazla kaç dağınık çift olur.</summary>
    public const int DisaridaEnCok = 2;

    private readonly int daireSayisi;
    private readonly System.Random rastgele;   // tohumlu: aynı tohum aynı turu verir

    // Kat -> [daire] (raftaki, dışarıdaki). İlk sorulduğunda çekilir.
    private readonly Dictionary<int, int[]> raftakiler = new Dictionary<int, int[]>();
    private readonly Dictionary<int, int[]> disaridakiler = new Dictionary<int, int[]>();
    // Rafa konmuş dağınık çiftler: (kat, daire, sıra) tek bir sayıya çevrilerek tutulur.
    private readonly HashSet<int> konanlar = new HashSet<int>();

    public AyakkabiPlani(int daireSayisi, int tohum)
    {
        this.daireSayisi = daireSayisi;
        rastgele = new System.Random(tohum);
    }

    /// <summary>O katta her dairenin rafında BAŞLANGIÇTA kaç çift var (rafa konanlar hariç).</summary>
    public int[] Raftakiler(int kat) { Cek(kat); return (int[])raftakiler[kat].Clone(); }

    /// <summary>O katta her dairenin kapısı önünde kaç dağınık çift var (rafa konmuş olsa da sayılır).</summary>
    public int[] Disaridakiler(int kat) { Cek(kat); return (int[])disaridakiler[kat].Clone(); }

    // Katın sayılarını ilk soruluşta çeker. Raftaki + dışarıdaki hiçbir zaman raf sayısını
    // (5) geçmez: 3 + 2 = 5, yani dışarıdakilerin hepsi rafa sığar.
    private void Cek(int kat)
    {
        if (raftakiler.ContainsKey(kat)) return;
        var raf = new int[daireSayisi];
        var dis = new int[daireSayisi];
        for (int d = 0; d < daireSayisi; d++)
        {
            raf[d] = rastgele.Next(RaftaEnAz, RaftaEnCok + 1);    // üst sınır hariç: +1
            dis[d] = rastgele.Next(0, DisaridaEnCok + 1);
        }
        raftakiler[kat] = raf;
        disaridakiler[kat] = dis;
    }

    private static int Anahtar(int kat, int daire, int sira) { return kat * 100 + daire * 10 + sira; }

    /// <summary>Dağınık bir çift rafa kondu. daire ve sira 0'dan başlar.</summary>
    public void RafaKonduOlarakIsaretle(int kat, int daire, int sira) { konanlar.Add(Anahtar(kat, daire, sira)); }

    /// <summary>Bu dağınık çift bu turda rafa kondu mu?</summary>
    public bool RafaKonduMu(int kat, int daire, int sira) { return konanlar.Contains(Anahtar(kat, daire, sira)); }

    /// <summary>Bu dairenin rafında şu an dolu olan raf sayısı: başlangıçtakiler + rafa konanlar.</summary>
    public int DoluRaf(int kat, int daire)
    {
        int dolu = Raftakiler(kat)[daire];
        int dis = Disaridakiler(kat)[daire];
        for (int s = 0; s < dis; s++) if (RafaKonduMu(kat, daire, s)) dolu++;
        return dolu;
    }

    /// <summary>Cezadan sonra: bütün sayılar ve rafa konanlar unutulur, yeniden çekilir.</summary>
    public void Sifirla()
    {
        raftakiler.Clear();
        disaridakiler.Clear();
        konanlar.Clear();
    }
}
