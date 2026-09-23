// CopPlaniTestleri.cs
// Çöp poşeti sayılarını belirleyen kuralın testleri.
//
// Korunan üç söz:
//   1. Sayılar her zaman 0 ile 3 arasında.
//   2. Bir tur içinde aynı kat hep aynı sayıları verir.
//   3. Cezadan sonra (Sifirla) sayılar yeniden belirlenir.

using NUnit.Framework;

[TestFixture]
public class CopPlaniTestleri
{
    private const int DaireSayisi = 4;

    [Test]
    public void Her_daire_icin_bir_sayi_uretilir()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 1);

        Assert.AreEqual(DaireSayisi, plan.KatIcin(1).Length);
    }

    [Test]
    public void Sayilar_sifir_ile_uc_arasindadir()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 7);

        // Birçok katı tek tek sorup her sayıya bakıyoruz.
        for (int kat = 1; kat <= 200; kat++)
        {
            foreach (int adet in plan.KatIcin(kat))
            {
                Assert.GreaterOrEqual(adet, 0);
                Assert.LessOrEqual(adet, CopPlani.EnFazlaPoset);
            }
        }
    }

    // Üst sınırın "hariç" olması yüzünden 3'ün hiç çıkmaması klasik bir hatadır.
    // Bu test, dört değerin de gerçekten çıktığını doğruluyor.
    [Test]
    public void Sifirdan_uce_butun_degerler_cikar()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 3);
        var goruldu = new bool[CopPlani.EnFazlaPoset + 1];

        for (int kat = 1; kat <= 200; kat++)
        {
            foreach (int adet in plan.KatIcin(kat)) goruldu[adet] = true;
        }

        for (int deger = 0; deger <= CopPlani.EnFazlaPoset; deger++)
        {
            Assert.IsTrue(goruldu[deger], $"{deger} poşet hiç çıkmadı.");
        }
    }

    [Test]
    public void Ayni_kat_tur_icinde_hep_ayni_sayilari_verir()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 11);

        int[] ilk = plan.KatIcin(4);
        plan.KatIcin(5);             // araya başka katlar girsin
        plan.KatIcin(2);

        CollectionAssert.AreEqual(ilk, plan.KatIcin(4));
    }

    // Dönen dizi kaydın kopyası olmalı; çağıran kod onu bozarsa kat değişmemeli.
    [Test]
    public void Donen_dizi_degistirilirse_kayit_bozulmaz()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 5);

        int[] ilk = plan.KatIcin(3);
        int[] beklenen = (int[])ilk.Clone();

        ilk[0] = 99;

        CollectionAssert.AreEqual(beklenen, plan.KatIcin(3));
    }

    [Test]
    public void Ayni_tohum_ayni_turu_uretir()
    {
        var a = new CopPlani(DaireSayisi, tohum: 42);
        var b = new CopPlani(DaireSayisi, tohum: 42);

        for (int kat = 1; kat <= 8; kat++)
        {
            CollectionAssert.AreEqual(a.KatIcin(kat), b.KatIcin(kat));
        }
    }

    // Cezadan sonra en az bir kat farklı çıkmalı. Tohum sabit olduğu için
    // bu test her çalıştırmada aynı sonucu verir; şansa bağlı değildir.
    [Test]
    public void Sifirlamadan_sonra_sayilar_yeniden_belirlenir()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 9);

        var once = new int[8][];
        for (int kat = 1; kat <= 8; kat++) once[kat - 1] = plan.KatIcin(kat);

        plan.Sifirla();

        bool farkVar = false;
        for (int kat = 1; kat <= 8; kat++)
        {
            int[] sonra = plan.KatIcin(kat);
            for (int i = 0; i < DaireSayisi; i++)
            {
                if (sonra[i] != once[kat - 1][i]) farkVar = true;
            }
        }

        Assert.IsTrue(farkVar, "Sıfırlamadan sonra 8 katın hepsi aynı çıktı.");
    }

    [Test]
    public void Kattaki_toplam_dairelerin_toplamidir()
    {
        var plan = new CopPlani(DaireSayisi, tohum: 2);

        int elleToplam = 0;
        foreach (int adet in plan.KatIcin(6)) elleToplam += adet;

        Assert.AreEqual(elleToplam, plan.KattakiToplam(6));
    }
}
