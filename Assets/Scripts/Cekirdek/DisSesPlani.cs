// DisSesPlani.cs
// Apartmanın dışından gelen seslerin (köpek, uzak silah, içeri süzülen rüzgâr) ne zaman
// ve hangisinin çalacağını belirleyen kural (#37).
//
// Şahan'ın seçimi (2026-09-26): kat olaylarından AYRI, rastgele bir dış katman.
//   - İki dış ses arası 40-90 sn. İlki oyun başladıktan 20-40 sn sonra (hemen değil).
//   - Art arda iki kez aynı TÜR gelmez (iki köpek üst üste gelirse "yine mi" olur).
//   - O an bir kat olayı ya da anomali sesi çalıyorsa dış ses araya girmez; birkaç
//     saniye sonra yeniden dener (ErtelemeSuresi). Bunu çalan bileşen sorar; bu dosya
//     yalnızca süreleri ve sırayı bilir.
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir.

public class DisSesPlani
{
    public const float IlkEnAz = 20f, IlkEnCok = 40f;          // ilk dış ses (saniye)
    public const float AraEnAz = 40f, AraEnCok = 90f;          // iki dış ses arası
    public const float ErtelemeSuresi = 2f;                    // meşgulse bu kadar sonra yeniden dene

    private readonly int turSayisi;
    private readonly System.Random rastgele;   // tohumlu: aynı tohum aynı sırayı verir
    private int oncekiTur = -1;
    private bool ilkVerildi;

    /// <param name="turSayisi">Kaç çeşit dış ses var (köpek, silah, rüzgâr = 3).</param>
    public DisSesPlani(int turSayisi, int tohum)
    {
        this.turSayisi = turSayisi;
        rastgele = new System.Random(tohum);
    }

    /// <summary>Bir sonraki dış sese kadar beklenecek süre. İlk soruda "ilk" aralık kullanılır.</summary>
    public float SonrakiBekleme()
    {
        float enAz = ilkVerildi ? AraEnAz : IlkEnAz;
        float enCok = ilkVerildi ? AraEnCok : IlkEnCok;
        ilkVerildi = true;
        return enAz + (float)rastgele.NextDouble() * (enCok - enAz);
    }

    /// <summary>
    /// Çalacak türün sırası (0..turSayisi-1). Bir önceki tür tekrar gelmez.
    /// kullanilabilir: o an elinde sesi olan türler (ör. henüz köpek kaydı yoksa köpek hariç).
    /// Hiçbiri kullanılamıyorsa -1.
    /// </summary>
    public int SonrakiTur(bool[] kullanilabilir)
    {
        var adaylar = new System.Collections.Generic.List<int>();
        for (int i = 0; i < turSayisi; i++)
            if (kullanilabilir[i] && i != oncekiTur) adaylar.Add(i);

        // Tek bir tür varsa (ve o da bir öncekiyse) tekrar etmesine izin ver: hiç ses
        // çalmamaktan iyidir.
        if (adaylar.Count == 0)
            for (int i = 0; i < turSayisi; i++) if (kullanilabilir[i]) adaylar.Add(i);
        if (adaylar.Count == 0) return -1;

        oncekiTur = adaylar[rastgele.Next(adaylar.Count)];
        return oncekiTur;
    }

    /// <summary>Bir türün kaç çeşidi varsa aralarından birini seçer (ör. iki silah sesi).</summary>
    public int CesitSec(int cesitSayisi)
    {
        return cesitSayisi <= 1 ? 0 : rastgele.Next(cesitSayisi);
    }
}
