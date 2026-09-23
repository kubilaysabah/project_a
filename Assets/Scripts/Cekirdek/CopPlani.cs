// CopPlani.cs
// Hangi katta, hangi dairenin önünde kaç çöp poşeti olacağını belirleyen kural.
//
// Şahan'ın kararı (2026-09-23):
//   - Her dairenin önünde 0, 1, 2 ya da 3 poşet olabilir, rastgele.
//   - Bir turda her katın poşet sayıları SABİTTİR: oyuncu aynı kata
//     dönerse aynı poşetleri görür.
//   - Oyuncu cezayla başa dönerse sayılar YENİDEN rastgele belirlenir;
//     böylece bir önceki denemenin ezberi işe yaramaz.
//
// Bu dosyada Unity'ye ait tek satır yok (KatSayaci ve AnomaliSecici gibi).
// Sebebi aynı: kuralı Unity'yi açmadan, saniyeler içinde test edebilmek.
// Poşetleri sahnede açıp kapatma işi KatYoneticisi'nde; burası sadece sayıları bilir.

using System.Collections.Generic;   // Dictionary için

public class CopPlani
{
    // ---------- SABİTLER ----------

    /// <summary>Bir dairenin önündeki en fazla poşet sayısı.</summary>
    public const int EnFazlaPoset = 3;

    // ---------- İÇ DURUM ----------

    // Kaç daire var. Kat başına 4, ama sayı sınıfa dışarıdan verilir;
    // ileride daire sayısı değişirse bu dosyaya dokunmak gerekmez.
    private readonly int daireSayisi;

    // System.Random: C#'ın kendi rastgele sayı üreticisi.
    // UnityEngine.Random yerine bunu kullanıyoruz, çünkü bir "tohum" alıyor:
    // aynı tohumla kurulan iki üretici AYNI sayı dizisini verir. Bir oyuncu
    // "3. katta hiç poşet yoktu, sonra şöyle oldu" dediğinde turu birebir
    // tekrar oynatabilmek için bu şart. (AnomaliSecici de aynı nedenle böyle.)
    private readonly System.Random rastgele;

    // Kat numarası -> o katın daire daire poşet sayıları.
    // Bir kat ilk kez sorulduğunda sayılar çekilir ve buraya yazılır;
    // sonraki sorularda buradan okunur. "Tur boyunca sabit" kuralı böyle sağlanıyor.
    private readonly Dictionary<int, int[]> katlar = new Dictionary<int, int[]>();

    // ---------- KURULUM ----------

    /// <param name="daireSayisi">Kat başına daire sayısı.</param>
    /// <param name="tohum">Rastgelelik tohumu. Aynı tohum aynı turu üretir.</param>
    public CopPlani(int daireSayisi, int tohum)
    {
        this.daireSayisi = daireSayisi;
        rastgele = new System.Random(tohum);
    }

    // ---------- SORGU ----------

    /// <summary>
    /// Verilen katta her dairenin önünde kaç poşet olduğunu döndürür.
    /// Dizinin 0. elemanı 1 numaralı kapı, 1. elemanı 2 numaralı kapı, ...
    /// </summary>
    public int[] KatIcin(int kat)
    {
        // TryGetValue: "bu anahtar sözlükte var mı, varsa değerini ver" demenin
        // tek adımlı yolu. Varsa true döner ve değeri 'kayitli'ye yazar.
        if (!katlar.TryGetValue(kat, out int[] kayitli))
        {
            kayitli = new int[daireSayisi];

            for (int i = 0; i < daireSayisi; i++)
            {
                // Next(0, 4): 0 dahil, 4 HARİÇ bir tam sayı, yani 0, 1, 2 veya 3.
                // Üst sınırın hariç olması C#'ta böyledir; +1 unutulursa
                // 3 poşet hiç çıkmaz ve bunu gözle fark etmek zordur.
                kayitli[i] = rastgele.Next(0, EnFazlaPoset + 1);
            }

            katlar[kat] = kayitli;
        }

        // Kaydın KOPYASINI veriyoruz, kendisini değil. Dizi bir referans
        // tipidir: kendisini verseydik, çağıran kod içindeki bir sayıyı
        // değiştirdiğinde sözlükteki kayıt da değişirdi ve "sabit" kat bozulurdu.
        return (int[])kayitli.Clone();
    }

    /// <summary>O kattaki toplam poşet sayısı. Test göstergesi ve günlük için.</summary>
    public int KattakiToplam(int kat)
    {
        int toplam = 0;
        foreach (int adet in KatIcin(kat)) toplam += adet;
        return toplam;
    }

    // ---------- TOPLANANLAR ----------
    // Sahnede tek bir sahanlık var ve o sahanlık farklı katları sırayla gösteriyor.
    // Oyuncu bir katta iki poşet toplayıp merdivenden biraz inip geri dönerse,
    // sahanlık o katı YENİDEN kurar. Toplanan poşetleri bir yerde hatırlamasaydık
    // poşetler geri gelirdi. Bu kayıt işte o hafıza.
    //
    // HashSet: içinde her değerin EN FAZLA BİR KEZ bulunduğu bir küme.
    // "Bu var mı?" sorusunu listeden çok daha hızlı cevaplar; sırası yoktur.
    private readonly HashSet<int> toplananlar = new HashSet<int>();

    // Üç sayıyı (kat, daire, sıra) tek bir tam sayıya çeviriyoruz ki kümede
    // tek anahtar olarak tutulabilsin. Kat 4, daire 2, sıra 3 -> 423.
    // Daire ve sıra 10'dan küçük olduğu sürece iki farklı üçlü asla aynı sayıya denk gelmez.
    private static int Anahtar(int kat, int daire, int sira)
    {
        return kat * 100 + daire * 10 + sira;
    }

    /// <summary>Bir poşetin toplandığını kaydeder. daire ve sira 0'dan başlar.</summary>
    public void ToplandiOlarakIsaretle(int kat, int daire, int sira)
    {
        toplananlar.Add(Anahtar(kat, daire, sira));
    }

    /// <summary>Bu poşet bu turda daha önce toplandı mı?</summary>
    public bool ToplandiMi(int kat, int daire, int sira)
    {
        return toplananlar.Contains(Anahtar(kat, daire, sira));
    }

    // ---------- SIFIRLAMA ----------

    /// <summary>
    /// Cezadan sonra çağrılır: bütün katların sayıları unutulur, bir sonraki
    /// soruda yeniden çekilir. Üretici BAŞA SARILMAZ, kaldığı yerden devam eder;
    /// başa sarılsaydı aynı sayılar tekrar gelirdi ve "yeniden rastgele" olmazdı.
    /// Toplanan poşet kayıtları da silinir: yeni turda her şey yerinde.
    /// </summary>
    public void Sifirla()
    {
        katlar.Clear();
        toplananlar.Clear();
    }
}
