// LambaGerilimKuraliTestleri.cs
// Kafa lambasının gerilim göstergesi kuralının testleri (#105).

using NUnit.Framework;

[TestFixture]
public class LambaGerilimKuraliTestleri
{
    [Test]
    public void Gerilim_yokken_lamba_normal()
    {
        Assert.AreEqual(0f, LambaGerilimKurali.Karisim(0f));
        Assert.AreEqual(1f, LambaGerilimKurali.GucCarpani(0f));
        Assert.AreEqual(0f, LambaGerilimKurali.Vinlama(0f));
        Assert.AreEqual(1f, LambaGerilimKurali.VinlamaPerdesi(0f));
    }

    [Test]
    public void Esiklerin_altinda_hicbir_sey_degismez()
    {
        // Sararma 0,15'e, vınlama 0,25'e kadar sıfır: oyuncu daha ilk adımda uyarılmasın.
        Assert.AreEqual(0f, LambaGerilimKurali.Karisim(LambaGerilimKurali.SararmaBas));
        Assert.AreEqual(0f, LambaGerilimKurali.Vinlama(LambaGerilimKurali.VinlamaBas));
        Assert.AreEqual(0f, LambaGerilimKurali.Vinlama(0.20f), "0,20'de vınlama henüz yok");
        Assert.Greater(LambaGerilimKurali.Karisim(0.20f), 0f, "0,20'de sararma başlamış");
    }

    [Test]
    public void Av_esiginde_tepki_tamdir()
    {
        float av = 0.85f;   // GerilimKurali.AvEsigi
        Assert.AreEqual(1f, LambaGerilimKurali.Karisim(av));
        Assert.AreEqual(LambaGerilimKurali.EnDusukGuc, LambaGerilimKurali.GucCarpani(av), 1e-5f);
        Assert.AreEqual(1f, LambaGerilimKurali.Vinlama(av));
        Assert.AreEqual(LambaGerilimKurali.EnTizPerde, LambaGerilimKurali.VinlamaPerdesi(av), 1e-5f);
    }

    [Test]
    public void Gerilim_arttikca_tepki_hic_geri_gitmez()
    {
        // Tekdüze (monoton): her adımda sararma ve vınlama artar ya da aynı kalır, güç azalır ya da aynı kalır.
        float oncekiK = 0f, oncekiV = 0f, oncekiG = 1f;
        for (int i = 0; i <= 100; i++)
        {
            float g = i / 100f;
            float k = LambaGerilimKurali.Karisim(g), v = LambaGerilimKurali.Vinlama(g), gc = LambaGerilimKurali.GucCarpani(g);
            Assert.GreaterOrEqual(k, oncekiK, $"sararma {g}");
            Assert.GreaterOrEqual(v, oncekiV, $"vınlama {g}");
            Assert.LessOrEqual(gc, oncekiG, $"güç {g}");
            Assert.That(k, Is.InRange(0f, 1f)); Assert.That(v, Is.InRange(0f, 1f));
            oncekiK = k; oncekiV = v; oncekiG = gc;
        }
    }

    [Test]
    public void Gecis_yumusaktir_sinirda_sicrama_yok()
    {
        // Eşiğin hemen üstü neredeyse sıfır: smoothstep'in uçlarda eğimi sıfırdır.
        Assert.Less(LambaGerilimKurali.Karisim(LambaGerilimKurali.SararmaBas + 0.02f), 0.01f);
        Assert.Greater(LambaGerilimKurali.Karisim(LambaGerilimKurali.SararmaSon - 0.02f), 0.99f);
        // Ortada tam yarı: (0,15 + 0,85) / 2 = 0,50 → 0,5.
        Assert.AreEqual(0.5f, LambaGerilimKurali.Karisim(0.5f), 1e-5f);
    }

    [Test]
    public void Bozuk_ve_aralik_disi_degerler_guvenli()
    {
        Assert.AreEqual(0f, LambaGerilimKurali.Karisim(float.NaN));
        Assert.AreEqual(1f, LambaGerilimKurali.GucCarpani(float.NaN));
        Assert.AreEqual(0f, LambaGerilimKurali.Karisim(-3f));
        Assert.AreEqual(1f, LambaGerilimKurali.Karisim(7f));
        Assert.AreEqual(1f, LambaGerilimKurali.Vinlama(float.PositiveInfinity));
    }
}
