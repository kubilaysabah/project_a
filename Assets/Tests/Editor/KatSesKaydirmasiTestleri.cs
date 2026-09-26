// KatSesKaydirmasiTestleri.cs
// Her katın TV'sinin kaydın farklı bir anını çalması kuralının testleri (#39).

using NUnit.Framework;

[TestFixture]
public class KatSesKaydirmasiTestleri
{
    private const double Uzunluk = 85.5;   // TV kaydının uzunluğu

    // İki an arasındaki mesafe, döngü olduğu için çember üzerinde ölçülür:
    // 1 sn ile 84 sn birbirine 2,5 sn uzaktır, 83 sn değil.
    private static double CemberMesafesi(double a, double b)
    {
        double d = System.Math.Abs(a - b) % Uzunluk;
        return System.Math.Min(d, Uzunluk - d);
    }

    [Test]
    public void Kaydirma_hep_kaydin_icinde()
    {
        for (int kat = -5; kat <= 20; kat++)
        {
            double s = KatSesKaydirmasi.Saniye(kat, Uzunluk);
            Assert.GreaterOrEqual(s, 0.0);
            Assert.Less(s, Uzunluk);
        }
    }

    [Test]
    public void Ayni_kat_hep_ayni_kaydirmayi_verir()
    {
        Assert.AreEqual(KatSesKaydirmasi.Saniye(4, Uzunluk), KatSesKaydirmasi.Saniye(4, Uzunluk));
    }

    // Yukarı çıkan oyuncu TV'nin kaydın bambaşka bir yerine geçtiğini duymalı.
    [Test]
    public void Komsu_katlar_kaydin_uzak_yerlerini_calar()
    {
        for (int kat = 0; kat < 9; kat++)
        {
            double mesafe = CemberMesafesi(KatSesKaydirmasi.Saniye(kat, Uzunluk), KatSesKaydirmasi.Saniye(kat + 1, Uzunluk));
            Assert.Greater(mesafe, Uzunluk * 0.25, $"{kat}. ve {kat + 1}. kat birbirine çok yakın");
        }
    }

    // Oyunun dokuz katından (zemin + 8) hiçbir ikisi aynı anı çalmamalı.
    [Test]
    public void Oyunun_katlarindan_hicbir_ikisi_yakin_degil()
    {
        for (int a = 0; a <= 8; a++)
            for (int b = a + 1; b <= 8; b++)
                Assert.Greater(CemberMesafesi(KatSesKaydirmasi.Saniye(a, Uzunluk), KatSesKaydirmasi.Saniye(b, Uzunluk)),
                               Uzunluk * 0.05, $"{a}. ve {b}. kat");
    }
}
