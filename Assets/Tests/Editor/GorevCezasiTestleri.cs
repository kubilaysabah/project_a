// GorevCezasiTestleri.cs
// "Görevini yapmadan üst kata çıkarsan ceza alırsın" kuralının testleri.
//
// Şahan'ın kararı (2026-09-22): çöp poşetini almadan ilerlemek hata sayılır.
// Kural KatSayaci'nın içinde tek bir bayrakla ifade edildi; hangi görevin
// eksik olduğunu bilmesine gerek yok, sadece "eksik var mı" bilgisine bakıyor.

using NUnit.Framework;

[TestFixture]
public class GorevCezasiTestleri
{
    private KatSayaci sayac;

    [SetUp]
    public void Hazirla()
    {
        sayac = new KatSayaci();
    }

    // Kuralın kalbi: doğru karar bile olsa görev eksikse ceza gelir.
    [Test]
    public void Normal_katta_gorev_eksikse_yukari_cikmak_cezalandirilir()
    {
        sayac.KatiHazirla(anomaliVar: false);

        KararSonucu sonuc = sayac.KararVer(Karar.Yukari, gorevEksik: true);

        Assert.AreEqual(KararSonucu.Sifirla, sonuc);
        Assert.AreEqual(1, sayac.MevcutKat);
        Assert.AreEqual(1, sayac.ToplamHata);
    }

    [Test]
    public void Normal_katta_gorev_tamamsa_yukari_cikmak_ilerletir()
    {
        sayac.KatiHazirla(anomaliVar: false);

        KararSonucu sonuc = sayac.KararVer(Karar.Yukari, gorevEksik: false);

        Assert.AreEqual(KararSonucu.Ilerle, sonuc);
        Assert.AreEqual(2, sayac.MevcutKat);
    }

    // Anomalili katta görev bayrağı zaten gönderilmiyor (KatYoneticisi
    // göndermiyor), ama gönderilse bile kural bozulmamalı: bayrak yoksa
    // geri dönmek doğru karardır.
    [Test]
    public void Anomalili_katta_gorev_aranmazsa_geri_donmek_ilerletir()
    {
        sayac.KatiHazirla(anomaliVar: true);

        KararSonucu sonuc = sayac.KararVer(Karar.Geri, gorevEksik: false);

        Assert.AreEqual(KararSonucu.Ilerle, sonuc);
        Assert.AreEqual(2, sayac.MevcutKat);
    }

    [Test]
    public void Bayrak_verilmezse_eski_davranis_aynen_surer()
    {
        // Eski çağrı biçimi (tek parametreli) hâlâ çalışmalı ve
        // görev eksikmiş gibi davranmamalı.
        sayac.KatiHazirla(anomaliVar: false);

        Assert.AreEqual(KararSonucu.Ilerle, sayac.KararVer(Karar.Yukari));
        Assert.AreEqual(0, sayac.ToplamHata);
    }

    [Test]
    public void Gorev_eksikken_yanlis_karar_tek_hata_sayilir()
    {
        // İki sebep birden varsa (hem yanlış karar hem eksik görev)
        // hata sayacı yine de bir artmalı.
        sayac.KatiHazirla(anomaliVar: false);

        sayac.KararVer(Karar.Geri, gorevEksik: true);

        Assert.AreEqual(1, sayac.ToplamHata);
    }

    [Test]
    public void Gorev_eksikligi_catiya_ulasmayi_engeller()
    {
        // 7 doğru karar: 8. kata gel.
        for (int i = 0; i < 7; i++)
        {
            sayac.KatiHazirla(anomaliVar: false);
            sayac.KararVer(Karar.Yukari);
        }
        Assert.AreEqual(8, sayac.MevcutKat);

        // Son katta çöp alınmadan çıkılırsa çatı açılmaz, başa dönülür.
        sayac.KatiHazirla(anomaliVar: false);
        KararSonucu sonuc = sayac.KararVer(Karar.Yukari, gorevEksik: true);

        Assert.AreEqual(KararSonucu.Sifirla, sonuc);
        Assert.IsFalse(sayac.TurBitti);
        Assert.AreEqual(1, sayac.MevcutKat);
    }
}
