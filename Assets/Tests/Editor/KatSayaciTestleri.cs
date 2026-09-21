// KatSayaciTestleri.cs
// KatSayaci sınıfının otomatik testleri.
//
// OTOMATİK TEST NEDİR
// Kodun doğru çalıştığını iddia eden, kendisi de koddan ibaret küçük denemelerdir.
// Elle test etmek için Unity'yi açıp Play'e basıp sekiz kat çıkmak gerekir; bu dakikalar sürer
// ve her seferinde aynı şeyi tekrarlamak zordur. Bu dosya aynı işi saniyenin altında yapar
// ve hiçbir adımı atlamaz. Terminalden `unity test --mode EditMode` ile çalıştırılır.
//
// Neden burası ayrı bir klasör ve ayrı bir derleme birimi?
// Test kodu oyunun içine GİRMEZ. Oyuncunun indirdiği dosyada bu dosya bulunmaz.

using NUnit.Framework;   // Test, SetUp, Assert gibi test araçları bu kütüphaneden gelir

// [TestFixture]: "bu sınıf testler içerir" demektir.
[TestFixture]
public class KatSayaciTestleri
{
    // Her testin kendi taze sayacı olsun diye burada tutuyoruz.
    private KatSayaci sayac;

    // [SetUp]: HER testten önce yeniden çalışır.
    // Böylece bir testin bıraktığı durum diğerini etkilemez.
    // Testlerin birbirinden bağımsız olması şarttır; yoksa sıra değişince sonuç değişir.
    [SetUp]
    public void Hazirla()
    {
        sayac = new KatSayaci();
    }

    // ---------- DÖRT KOMBİNASYON ----------
    // Oyunun bütün kuralı bu dört satırdır. Dördü de ayrı ayrı test edilir.

    [Test]
    public void Normal_katta_yukari_gitmek_ilerletir()
    {
        sayac.KatiHazirla(anomaliVar: false);

        KararSonucu sonuc = sayac.KararVer(Karar.Yukari);

        // Assert.AreEqual(beklenen, gercek): ikisi eşit değilse test kırmızı yanar.
        Assert.AreEqual(KararSonucu.Ilerle, sonuc);
        Assert.AreEqual(2, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata);
    }

    [Test]
    public void Normal_katta_geri_donmek_sifirlar()
    {
        sayac.KatiHazirla(anomaliVar: false);

        KararSonucu sonuc = sayac.KararVer(Karar.Geri);

        Assert.AreEqual(KararSonucu.Sifirla, sonuc);
        Assert.AreEqual(1, sayac.MevcutKat);
        Assert.AreEqual(1, sayac.ToplamHata);
    }

    [Test]
    public void Anomalili_katta_geri_donmek_ilerletir()
    {
        sayac.KatiHazirla(anomaliVar: true);

        KararSonucu sonuc = sayac.KararVer(Karar.Geri);

        Assert.AreEqual(KararSonucu.Ilerle, sonuc);
        Assert.AreEqual(2, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata);
    }

    [Test]
    public void Anomalili_katta_yukari_gitmek_sifirlar()
    {
        sayac.KatiHazirla(anomaliVar: true);

        KararSonucu sonuc = sayac.KararVer(Karar.Yukari);

        Assert.AreEqual(KararSonucu.Sifirla, sonuc);
        Assert.AreEqual(1, sayac.MevcutKat);
        Assert.AreEqual(1, sayac.ToplamHata);
    }

    // ---------- TAM TUR ----------

    [Test]
    public void Sekiz_kat_hatasiz_gecilirse_catiya_ulasilir()
    {
        // 1'den 8'e kadar sekiz doğru karar. Sekizincisi çatıyı açmalı.
        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            Assert.AreEqual(kat, sayac.MevcutKat, $"{kat}. katta olunmalıydı");

            sayac.KatiHazirla(anomaliVar: false);
            KararSonucu sonuc = sayac.KararVer(Karar.Yukari);

            if (kat < KatSayaci.CatiKati)
            {
                Assert.AreEqual(KararSonucu.Ilerle, sonuc, $"{kat}. katta ilerlenmeliydi");
            }
            else
            {
                Assert.AreEqual(KararSonucu.CatiyaUlasildi, sonuc, "8. katta çatıya ulaşılmalıydı");
            }
        }

        Assert.IsTrue(sayac.TurBitti);
        Assert.AreEqual(KatSayaci.CatiKati, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata);
    }

    [Test]
    public void Yedinci_katta_yapilan_hata_bir_numaraya_dondurur()
    {
        // Altı doğru karar: 1'den 7'ye çık.
        for (int i = 0; i < 6; i++)
        {
            sayac.KatiHazirla(anomaliVar: false);
            sayac.KararVer(Karar.Yukari);
        }
        Assert.AreEqual(7, sayac.MevcutKat, "altı doğru karardan sonra 7. katta olunmalı");

        // Yedinci katta anomali var ama oyuncu yukarı çıkıyor: ceza.
        sayac.KatiHazirla(anomaliVar: true);
        KararSonucu sonuc = sayac.KararVer(Karar.Yukari);

        Assert.AreEqual(KararSonucu.Sifirla, sonuc);
        Assert.AreEqual(1, sayac.MevcutKat, "hata sonrası 1. kata dönülmeli");
        Assert.AreEqual(1, sayac.ToplamHata);
        Assert.IsFalse(sayac.TurBitti);
    }

    // ---------- SINIR DURUMLARI ----------

    [Test]
    public void Tur_bittikten_sonra_karar_alinmaz()
    {
        for (int i = 0; i < KatSayaci.CatiKati; i++)
        {
            sayac.KatiHazirla(anomaliVar: false);
            sayac.KararVer(Karar.Yukari);
        }
        Assert.IsTrue(sayac.TurBitti);

        // Tur bitmişken verilen karar durumu değiştirmemeli.
        KararSonucu sonuc = sayac.KararVer(Karar.Geri);

        Assert.AreEqual(KararSonucu.CatiyaUlasildi, sonuc);
        Assert.AreEqual(KatSayaci.CatiKati, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata, "tur bittikten sonra hata sayılmamalı");
    }

    [Test]
    public void Sifirla_her_seyi_basa_alir()
    {
        sayac.KatiHazirla(anomaliVar: true);
        sayac.KararVer(Karar.Yukari);   // hata yap
        sayac.KatiHazirla(anomaliVar: true);

        sayac.Sifirla();

        Assert.AreEqual(1, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata);
        Assert.IsFalse(sayac.AnomaliVar);
        Assert.IsFalse(sayac.TurBitti);
    }

    [Test]
    public void Baslangicta_birinci_katta_ve_temiz_olunur()
    {
        Assert.AreEqual(1, sayac.MevcutKat);
        Assert.AreEqual(0, sayac.ToplamHata);
        Assert.IsFalse(sayac.AnomaliVar);
        Assert.IsFalse(sayac.TurBitti);
    }
}
