// NefesVeFenerTestleri.cs
// Nefes buharı ve fener titremesi kurallarının testleri (#136).

using NUnit.Framework;

public class NefesVeFenerTestleri
{
    [Test]
    public void Nefes_donemi_kademeyle_kisalir_ve_kosarken_daha_da_kisalir()
    {
        float k1 = NefesKurali.Donem(1, false, false), k2 = NefesKurali.Donem(2, false, false), av = NefesKurali.Donem(2, false, true);
        Assert.Greater(k1, k2);
        Assert.Greater(k2, av);
        Assert.AreEqual(NefesKurali.DonemAv, NefesKurali.Donem(3, false, false), 1e-5f, "3. kademe av gibi");
        Assert.AreEqual(k1 * NefesKurali.KosmaCarpani, NefesKurali.Donem(1, true, false), 1e-5f);
    }

    [Test]
    public void Nefes_gucu_rahatken_sifir_kademeyle_artar_avda_tam()
    {
        Assert.AreEqual(0f, NefesKurali.Guc(0, false, false, true, false));
        Assert.AreEqual(NefesKurali.GucKademe1, NefesKurali.Guc(1, false, false, true, false));
        Assert.AreEqual(NefesKurali.GucKademe2, NefesKurali.Guc(2, false, false, true, false));
        Assert.AreEqual(NefesKurali.GucAv, NefesKurali.Guc(1, true, false, true, false));
        Assert.AreEqual(NefesKurali.GucAv, NefesKurali.Guc(3, false, false, true, false));
    }

    [Test]
    public void Nefes_saklanirken_lamba_sonukken_ve_incelemede_gorunmez()
    {
        Assert.AreEqual(0f, NefesKurali.Guc(2, true, true, true, false), "saklanırken");
        Assert.AreEqual(0f, NefesKurali.Guc(2, true, false, false, false), "lamba sönük");
        Assert.AreEqual(0f, NefesKurali.Guc(2, true, false, true, true), "incelemede");
    }

    [Test]
    public void Fener_titremesi_ikinci_kademede_baslar_avda_artar()
    {
        Assert.AreEqual(0f, FenerTitremeKurali.Genlik(0, false));
        Assert.AreEqual(0f, FenerTitremeKurali.Genlik(1, false));
        Assert.AreEqual(FenerTitremeKurali.GenlikKademe2, FenerTitremeKurali.Genlik(2, false));
        Assert.AreEqual(FenerTitremeKurali.GenlikAv, FenerTitremeKurali.Genlik(2, true));
        Assert.AreEqual(FenerTitremeKurali.GenlikAv, FenerTitremeKurali.Genlik(3, false));
        Assert.Greater(FenerTitremeKurali.GenlikAv, FenerTitremeKurali.GenlikKademe2);
    }
}
