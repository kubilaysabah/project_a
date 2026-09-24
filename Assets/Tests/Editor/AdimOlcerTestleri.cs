// AdimOlcerTestleri.cs
// Ayak sesinin ne zaman çalacağını belirleyen kuralın testleri (#25).

using NUnit.Framework;

[TestFixture]
public class AdimOlcerTestleri
{
    [Test]
    public void Adim_boyu_kadar_yurununce_bir_adim_sayilir()
    {
        var o = new AdimOlcer();
        int adim = 0;

        // Karede 5 cm: 12 karede 60 cm = tam bir adım.
        for (int i = 0; i < 12; i++) if (o.Ilerle(0.05f, true)) adim++;

        Assert.AreEqual(1, adim);
    }

    [Test]
    public void Artan_mesafe_bir_sonraki_adima_devreder()
    {
        var o = new AdimOlcer();
        o.Ilerle(0.5f, true);
        Assert.IsTrue(o.Ilerle(0.3f, true), "0,8 m'de adım atılmalıydı.");
        Assert.AreEqual(0.2f, o.Biriken, 1e-4, "Artan 20 cm kaybolmamalı.");
    }

    [Test]
    public void Duran_oyuncu_adim_atmaz()
    {
        var o = new AdimOlcer();
        for (int i = 0; i < 1000; i++) Assert.IsFalse(o.Ilerle(0f, true));
    }

    // Işınlanma yatayda değil dikeyde oluyor ama başa alınma yatayda da sıçratıyor.
    [Test]
    public void Tek_karedeki_sicrama_adim_sayilmaz_ve_birikeni_sifirlar()
    {
        var o = new AdimOlcer();
        o.Ilerle(0.5f, true);

        Assert.IsFalse(o.Ilerle(3.0f, true), "3 m'lik sıçrama adım sayıldı.");
        Assert.AreEqual(0f, o.Biriken, 1e-6, "Sıçramadan önceki yarım adım silinmeliydi.");
    }

    [Test]
    public void Havadayken_adim_sayilmaz()
    {
        var o = new AdimOlcer();
        for (int i = 0; i < 20; i++) Assert.IsFalse(o.Ilerle(0.1f, false));
        Assert.AreEqual(0f, o.Biriken, 1e-6, "Havadaki yol birikmemeli.");
    }
}
