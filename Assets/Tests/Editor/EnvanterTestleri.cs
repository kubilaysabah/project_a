// EnvanterTestleri.cs
// Tek eşyalık cebin testleri (#52).

using NUnit.Framework;

[TestFixture]
public class EnvanterTestleri
{
    [Test]
    public void El_bos_baslar_ve_esya_alinir()
    {
        var e = new Envanter();
        Assert.IsTrue(e.ElBos);
        Assert.IsTrue(e.Al(EsyaTuru.KatAnahtari, 3));
        Assert.IsFalse(e.ElBos);
        Assert.IsTrue(e.Tasiyor(EsyaTuru.KatAnahtari));
        Assert.IsTrue(e.Tasiyor(EsyaTuru.KatAnahtari, 3));
        Assert.IsFalse(e.Tasiyor(EsyaTuru.KatAnahtari, 4), "başka katın anahtarı sayılmamalı");
    }

    [Test]
    public void El_doluyken_ikinci_esya_alinmaz()
    {
        var e = new Envanter();
        e.Al(EsyaTuru.Tornavida);
        Assert.IsFalse(e.Al(EsyaTuru.Sigorta));
        Assert.AreEqual(EsyaTuru.Tornavida, e.Tasinan);
    }

    [Test]
    public void Kullaninca_el_bosalir_yanlis_esya_kullanilamaz()
    {
        var e = new Envanter();
        e.Al(EsyaTuru.KatAnahtari, 2);
        Assert.IsFalse(e.Kullan(EsyaTuru.KatAnahtari, 5), "5. katın kapısı 2. katın anahtarıyla açılmamalı");
        Assert.IsFalse(e.Kullan(EsyaTuru.Sigorta));
        Assert.IsTrue(e.Kullan(EsyaTuru.KatAnahtari, 2));
        Assert.IsTrue(e.ElBos);
        Assert.IsFalse(e.Kullan(EsyaTuru.KatAnahtari, 2), "aynı anahtar iki kez kullanılamaz");
    }

    [Test]
    public void Yok_turu_alinamaz()
    {
        var e = new Envanter();
        Assert.IsFalse(e.Al(EsyaTuru.Yok));
        Assert.IsTrue(e.ElBos);
    }
}
