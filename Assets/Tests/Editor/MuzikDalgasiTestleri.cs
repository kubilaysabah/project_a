// MuzikDalgasiTestleri.cs
// Arka müziğin dalgalanma kuralının (#49) testleri.

using NUnit.Framework;

[TestFixture]
public class MuzikDalgasiTestleri
{
    // Müzik hiç susmaz ve patlamaz: çarpan her an sınırların içinde.
    [Test]
    public void Carpan_hep_sinirlarin_icinde_ve_sifirdan_buyuk()
    {
        var d = new MuzikDalgasi(1994);
        for (float t = 0f; t < 3600f; t += 0.5f)
        {
            float c = d.Carpan(t);
            Assert.GreaterOrEqual(c, MuzikDalgasi.EnAz - 1e-4f, $"t={t}");
            Assert.LessOrEqual(c, MuzikDalgasi.EnCok + 1e-4f, $"t={t}");
            Assert.Greater(c, 0f);
        }
    }

    // "Belirgin biçimde alçalıp yükseliyor": on dakikada en alçak ile en yüksek arası büyük olmalı.
    [Test]
    public void On_dakikada_belirgin_alcalip_yukselir()
    {
        var d = new MuzikDalgasi(7);
        float enAz = float.MaxValue, enCok = float.MinValue;
        for (float t = 0f; t < 600f; t += 1f)
        {
            float c = d.Carpan(t);
            if (c < enAz) enAz = c;
            if (c > enCok) enCok = c;
        }
        Assert.Greater(enCok - enAz, 0.5f, $"en az {enAz:0.00}, en çok {enCok:0.00}");
    }

    // Yavaş dalga: bir saniyede ani sıçrama yok (dakikalar süren kabarmalar).
    [Test]
    public void Bir_saniyede_ani_sicrama_yapmaz()
    {
        var d = new MuzikDalgasi(42);
        for (float t = 0f; t < 1200f; t += 1f)
            Assert.Less(System.Math.Abs(d.Carpan(t + 1f) - d.Carpan(t)), 0.05f, $"t={t}");
    }

    [Test]
    public void Ayni_tohum_ayni_dalga_farkli_tohum_farkli_dalga()
    {
        var a = new MuzikDalgasi(5);
        var b = new MuzikDalgasi(5);
        var c = new MuzikDalgasi(6);
        Assert.AreEqual(a.Carpan(123f), b.Carpan(123f));
        Assert.AreNotEqual(a.Carpan(123f), c.Carpan(123f));
    }

    [Test]
    public void Merdivende_sahanliktan_yuksek()
    {
        Assert.Greater(MuzikDalgasi.KonumCarpani(true), MuzikDalgasi.KonumCarpani(false));
    }
}
