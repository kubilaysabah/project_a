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

    // ---------- GÖRÜNTÜ (#30) ----------

    [Test]
    public void Goruntu_varsayilanlari_oyunun_ayarlandigi_hal()
    {
        Assert.AreEqual(AyarKurallari.KaliteYuksek, AyarKurallari.KaliteVarsayilan);
        Assert.AreEqual(0f, AyarKurallari.ParlaklikVarsayilan);
        Assert.IsTrue(AyarKurallari.TamEkranVarsayilan);
        Assert.IsTrue(AyarKurallari.VSyncVarsayilan);
    }

    [Test]
    public void Kalite_ve_parlaklik_sinirlanir()
    {
        Assert.AreEqual(AyarKurallari.KaliteDusuk, AyarKurallari.Kalite(-3));
        Assert.AreEqual(AyarKurallari.KaliteYuksek, AyarKurallari.Kalite(9));
        Assert.AreEqual(AyarKurallari.KaliteOrta, AyarKurallari.Kalite(1));
        Assert.AreEqual(1f, AyarKurallari.Parlaklik(5f));
        Assert.AreEqual(-1f, AyarKurallari.Parlaklik(-5f));
        Assert.AreEqual(0f, AyarKurallari.Parlaklik(float.NaN));
    }

    [Test]
    public void Kayitli_cozunurluk_listede_varsa_o_secilir()
    {
        int[] g = { 1280, 1920, 2560 };
        int[] y = { 720, 1080, 1440 };
        Assert.AreEqual(1, AyarKurallari.CozunurlukSec(g, y, 1920, 1080));
    }

    [Test]
    public void Kayitli_cozunurluk_listede_yoksa_en_yakini_secilir()
    {
        // Oyuncu 4K monitörden 1440p monitöre geçmiş: kayıtlı 3840x2160 artık yok.
        int[] g = { 1280, 1920, 2560 };
        int[] y = { 720, 1080, 1440 };
        Assert.AreEqual(2, AyarKurallari.CozunurlukSec(g, y, 3840, 2160));
        Assert.AreEqual(0, AyarKurallari.CozunurlukSec(g, y, 800, 600));
    }

    [Test]
    public void Cozunurluk_listesi_bossa_eksi_bir()
    {
        Assert.AreEqual(-1, AyarKurallari.CozunurlukSec(new int[0], new int[0], 1920, 1080));
        Assert.AreEqual(-1, AyarKurallari.CozunurlukSec(null, null, 1920, 1080));
    }

    [Test]
    public void Sayi_olmayan_ya_da_sonsuz_deger_varsayilana_doner()
    {
        Assert.AreEqual(AyarKurallari.HassasiyetVarsayilan, AyarKurallari.Hassasiyet(float.NaN));
        Assert.AreEqual(AyarKurallari.SesVarsayilan, AyarKurallari.Ses(float.PositiveInfinity));
        Assert.AreEqual(AyarKurallari.GorusVarsayilan, AyarKurallari.Gorus(float.NegativeInfinity));
    }
}
