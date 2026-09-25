// AyarKurallariTestleri.cs
// Oyuncu ayarlarının sınırlarını ve varsayılanlarını koruyan testler (#27).

using NUnit.Framework;

[TestFixture]
public class AyarKurallariTestleri
{
    [Test]
    public void Varsayilanlar_kendi_araliklarinin_icinde()
    {
        // Varsayılan aralığın dışındaysa oyun ilk açılışta bile düzeltilmiş bir değerle
        // başlar ve "varsayılana dön" düğmesi başka bir değere döner; sessiz bir hata.
        Assert.AreEqual(AyarKurallari.HassasiyetVarsayilan, AyarKurallari.Hassasiyet(AyarKurallari.HassasiyetVarsayilan));
        Assert.AreEqual(AyarKurallari.SesVarsayilan, AyarKurallari.Ses(AyarKurallari.SesVarsayilan));
        Assert.AreEqual(AyarKurallari.GorusVarsayilan, AyarKurallari.Gorus(AyarKurallari.GorusVarsayilan));
    }

    [Test]
    public void Oyunun_ayarlandigi_hal_varsayilan()
    {
        // Oyun 1× hassasiyet, tam ses, 60° görüş açısıyla ayarlandı; ilk açılış bu olmalı.
        Assert.AreEqual(1f, AyarKurallari.HassasiyetVarsayilan);
        Assert.AreEqual(1f, AyarKurallari.SesVarsayilan);
        Assert.AreEqual(60f, AyarKurallari.GorusVarsayilan);
        Assert.IsFalse(AyarKurallari.TersVarsayilan);
    }

    [Test]
    public void Araligin_disindaki_deger_en_yakin_sinira_cekilir()
    {
        Assert.AreEqual(AyarKurallari.HassasiyetEnAz, AyarKurallari.Hassasiyet(0f));
        Assert.AreEqual(AyarKurallari.HassasiyetEnCok, AyarKurallari.Hassasiyet(10f));
        Assert.AreEqual(0f, AyarKurallari.Ses(-0.5f));
        Assert.AreEqual(1f, AyarKurallari.Ses(1.7f));
        Assert.AreEqual(60f, AyarKurallari.Gorus(30f));
        Assert.AreEqual(90f, AyarKurallari.Gorus(179f));
    }

    [Test]
    public void Araligin_icindeki_deger_aynen_kalir()
    {
        Assert.AreEqual(1.5f, AyarKurallari.Hassasiyet(1.5f));
        Assert.AreEqual(0.4f, AyarKurallari.Ses(0.4f));
        Assert.AreEqual(75f, AyarKurallari.Gorus(75f));
    }

    [Test]
    public void Sayi_olmayan_ya_da_sonsuz_deger_varsayilana_doner()
    {
        Assert.AreEqual(AyarKurallari.HassasiyetVarsayilan, AyarKurallari.Hassasiyet(float.NaN));
        Assert.AreEqual(AyarKurallari.SesVarsayilan, AyarKurallari.Ses(float.PositiveInfinity));
        Assert.AreEqual(AyarKurallari.GorusVarsayilan, AyarKurallari.Gorus(float.NegativeInfinity));
    }
}
