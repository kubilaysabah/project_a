// KirikAnahtarlar.cs
// Bu katın kırık anahtarları (#65): köşedeki "1/2" sayacı için.
//
// Bir kapının anahtarı kırılmış olabilir: parçaları evin farklı yerlerindedir ve ancak hepsi
// bulununca anahtar tamamlanır (KilitTuru.KirikAnahtar). Parçalar ele girmez; kapıcı onları
// biriktirir ve kayda yazılır (Kayit.ParcaBulundu, çizim parçalarıyla aynı yer). Oyuncunun
// kaçını bulduğunu görmesi için köşede kırık anahtar simgesi ve "bulunan / toplam" yazar.
//
// Zincir uygulayıcısı kat açılırken buraya o katın kırık anahtarlarını yazar, kat değişince
// temizler; köşedeki gösterge (EsyaGostergesi) buradan okur. Cep ve Aletler gibi "static".

using System.Collections.Generic;

public static class KirikAnahtarlar
{
    /// <summary>Bir kırık anahtar: açtığı kilit ve parçalarının eşya kimlikleri.</summary>
    public class Takim
    {
        public string Kilit;
        public List<string> Parcalar = new List<string>();
    }

    private static int kat;
    private static readonly List<Takim> takimlar = new List<Takim>();

    public static void Ayarla(int katNo, List<Takim> liste)
    {
        kat = katNo;
        takimlar.Clear();
        if (liste != null) takimlar.AddRange(liste);
    }

    public static void Temizle() { takimlar.Clear(); }

    /// <summary>
    /// Göstergede çizilecek anahtarlar: kilidi henüz açılmamış ve en az bir parçası bulunmuş
    /// olanlar; her biri için (bulunan, toplam).
    /// </summary>
    public static List<int[]> Gosterilecekler()
    {
        var sonuc = new List<int[]>();
        foreach (Takim t in takimlar)
        {
            if (Kayit.KilitCozukMu(kat, t.Kilit)) continue;
            int bulunan = 0;
            foreach (string p in t.Parcalar) if (Kayit.ParcaBulunduMu(kat, p)) bulunan++;
            if (bulunan > 0) sonuc.Add(new[] { bulunan, t.Parcalar.Count });
        }
        return sonuc;
    }
}
