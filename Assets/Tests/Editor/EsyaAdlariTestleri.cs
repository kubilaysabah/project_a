// EsyaAdlariTestleri.cs
// Eşya adı kuralının (#75) testleri: kimlikten hangi metin anahtarlarının çıktığı.

using NUnit.Framework;

[TestFixture]
public class EsyaAdlariTestleri
{
    [Test]
    public void Duz_esya_kimliginin_kendisiyle_adlanir()
    {
        EsyaAdi a = EsyaAdlari.Coz("tornavida", EsyaTuru.Tornavida);
        Assert.AreEqual("esya.tornavida", a.Ana);
        Assert.IsNull(a.Ic);
    }

    // İkinci dairenin eşyası (#70) ile birinci dairenin eşyası aynı adı alır.
    [Test]
    public void Daire_oneki_ada_girmez()
    {
        Assert.AreEqual("esya.mutfak_anahtari", EsyaAdlari.Coz("a_mutfak_anahtari", EsyaTuru.OdaAnahtari).Ana);
        Assert.AreEqual("esya.mutfak_anahtari", EsyaAdlari.Coz("b_mutfak_anahtari", EsyaTuru.OdaAnahtari).Ana);
    }

    [Test]
    public void Kirik_anahtarin_sapi_ve_ucu_anahtarin_adini_tasir()
    {
        EsyaAdi bas = EsyaAdlari.Coz("salon_anahtari_bas", EsyaTuru.AnahtarParcasi);
        Assert.AreEqual("esya.kirik_bas", bas.Ana);
        Assert.AreEqual("esya.salon_anahtari", bas.Ic);

        EsyaAdi uc = EsyaAdlari.Coz("odaA_anahtari_uc", EsyaTuru.AnahtarParcasi);
        Assert.AreEqual("esya.kirik_uc", uc.Ana);
        Assert.AreEqual("esya.odaA_anahtari", uc.Ic);
    }

    [Test]
    public void Cizim_parcasi_numarasiyla_adlanir()
    {
        EsyaAdi a = EsyaAdlari.Coz("p3", EsyaTuru.Parca);
        Assert.AreEqual("esya.cizim_parcasi", a.Ana);
        Assert.AreEqual(3, a.Sayi);
    }

    // Tabağın listedeki adı, üstünde yazan isimle aynı anahtardan gelir.
    [Test]
    public void Tabak_sahibinin_adiyla_adlanir()
    {
        EsyaAdi a = EsyaAdlari.Coz("tabak_riza", EsyaTuru.Tabak);
        Assert.AreEqual("esya.tabak", a.Ana);
        Assert.AreEqual("bina.isim_riza", a.Ic);
    }

    [Test]
    public void Bos_kimlik_cokmez()
    {
        Assert.AreEqual("esya.", EsyaAdlari.Coz(null, EsyaTuru.Sigorta).Ana);
        Assert.AreEqual(0, EsyaAdlari.Coz("p", EsyaTuru.Parca).Sayi);
    }
}
