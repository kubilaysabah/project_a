// KosmaKuraliTestleri.cs
// Koşma / nefes kuralının testleri (#129).

using NUnit.Framework;

public class KosmaKuraliTestleri
{
    [Test]
    public void Dolu_nefesle_kosulur_ve_DoluSure_sonra_biter()
    {
        var k = new KosmaKurali();
        Assert.IsTrue(k.Ilerlet(0.1f, true, true, false));
        float t = 0.1f;
        while (k.Ilerlet(0.1f, true, true, false)) t += 0.1f;
        Assert.AreEqual(KosmaKurali.DoluSure, t, 0.15f, "koşu DoluSure kadar sürmeli");
        Assert.IsTrue(k.Tukendi);
        Assert.AreEqual(0f, k.Nefes, 0.001f);
    }

    [Test]
    public void Tukenince_esige_dolana_kadar_kosulmaz()
    {
        var k = new KosmaKurali();
        while (k.Ilerlet(0.1f, true, true, false)) { }
        // Tuş basılı tutuluyor ama nefes biter bitmez koşulmaz; eşik dolunca (0,3 × 7 sn = 2,1 sn) yeniden.
        float t = 0f;
        while (!k.Ilerlet(0.1f, true, true, false)) { t += 0.1f; Assert.Less(t, 5f, "yeniden koşma hiç gelmedi"); }
        Assert.AreEqual(KosmaKurali.YenidenEsik * KosmaKurali.DolmaSuresi, t, 0.2f);
    }

    [Test]
    public void Duran_ya_da_egik_oyuncu_kosmaz_ve_nefes_harcamaz()
    {
        var k = new KosmaKurali();
        Assert.IsFalse(k.Ilerlet(0.5f, true, false, false), "duran oyuncu");
        Assert.IsFalse(k.Ilerlet(0.5f, true, true, true), "eğik oyuncu");
        Assert.AreEqual(1f, k.Nefes, 0.001f);
    }

    [Test]
    public void Nefes_DolmaSuresi_icinde_dolar_ve_Doldur_sifirlar()
    {
        var k = new KosmaKurali();
        while (k.Ilerlet(0.1f, true, true, false)) { }
        for (int i = 0; i < 70; i++) k.Ilerlet(0.1f, false, false, false);   // 7 sn
        Assert.AreEqual(1f, k.Nefes, 0.02f);
        Assert.IsFalse(k.Tukendi);
        while (k.Ilerlet(0.1f, true, true, false)) { }
        k.Doldur();
        Assert.AreEqual(1f, k.Nefes, 0.001f);
        Assert.IsFalse(k.Tukendi);
        Assert.IsTrue(k.Ilerlet(0.1f, true, true, false));
    }
}
