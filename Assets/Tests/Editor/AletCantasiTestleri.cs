// AletCantasiTestleri.cs
// Kapıcının alet çantasının testleri (#61).

using NUnit.Framework;

[TestFixture]
public class AletCantasiTestleri
{
    [Test]
    public void Tornavida_alettir_anahtar_degildir()
    {
        Assert.IsTrue(AletCantasi.AletMi(EsyaTuru.Tornavida));
        Assert.IsFalse(AletCantasi.AletMi(EsyaTuru.OdaAnahtari));
        Assert.IsFalse(AletCantasi.AletMi(EsyaTuru.Sigorta));
        Assert.IsFalse(AletCantasi.AletMi(EsyaTuru.KatAnahtari));
    }

    [Test]
    public void Alet_bir_kez_eklenir_ve_kalir()
    {
        var c = new AletCantasi();
        Assert.IsTrue(c.Ekle("tornavida"));
        Assert.IsFalse(c.Ekle("tornavida"), "aynı alet iki kez eklenmez");
        Assert.IsTrue(c.Var("tornavida"));
        Assert.IsFalse(c.Var("cekic"));
        Assert.AreEqual(1, c.Sayi);
    }

    [Test]
    public void Metne_yazilip_geri_okunur()
    {
        var c = new AletCantasi();
        c.Ekle("tornavida"); c.Ekle("cekic");
        Assert.AreEqual("cekic;tornavida", c.Metin());
        var geri = AletCantasi.Oku("cekic;tornavida;;");
        Assert.AreEqual(2, geri.Sayi);
        Assert.IsTrue(geri.Var("tornavida"));
        Assert.AreEqual(0, AletCantasi.Oku(null).Sayi);
    }
}
