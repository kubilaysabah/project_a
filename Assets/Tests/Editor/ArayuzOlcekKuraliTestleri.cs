// ArayuzOlcekKuraliTestleri.cs
// Arayüz ölçeğinin testleri (#34).

using NUnit.Framework;

[TestFixture]
public class ArayuzOlcekKuraliTestleri
{
    [Test]
    public void Tasarim_yuksekliginde_olcek_bir()
    {
        Assert.AreEqual(1f, ArayuzOlcekKurali.Olcek(1080), 1e-5f);
    }

    [Test]
    public void Olcek_ekran_yuksekligiyle_dogru_orantili()
    {
        Assert.AreEqual(2f, ArayuzOlcekKurali.Olcek(2160), 1e-5f);        // 4K
        Assert.AreEqual(1440f / 1080f, ArayuzOlcekKurali.Olcek(1440), 1e-5f);
        Assert.AreEqual(800f / 1080f, ArayuzOlcekKurali.Olcek(800), 1e-5f);  // Steam Deck
    }

    [Test]
    public void Olcek_sinirlarin_disina_cikmaz()
    {
        Assert.AreEqual(ArayuzOlcekKurali.EnAz, ArayuzOlcekKurali.Olcek(200));
        Assert.AreEqual(ArayuzOlcekKurali.EnCok, ArayuzOlcekKurali.Olcek(20000));
        Assert.AreEqual(ArayuzOlcekKurali.EnAz, ArayuzOlcekKurali.Olcek(0));
    }
}
