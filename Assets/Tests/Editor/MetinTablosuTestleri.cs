// MetinTablosuTestleri.cs
// İki dilli metin tablosunu okuyan kuralın testleri (#15).
// Tablo dosyası elle düzenlenecek; burada bir insanın yapabileceği hatalar deneniyor.

using System.Linq;
using NUnit.Framework;

[TestFixture]
public class MetinTablosuTestleri
{
    // Testlerde TAB karakterini görünür kılmak için: "a" + T + "b".
    private const string T = "\t";

    [Test]
    public void Anahtar_secili_dildeki_metni_verir()
    {
        var tablo = MetinTablosu.Oku("ipucu.topla" + T + "TOPLA" + T + "PICK UP");

        Assert.AreEqual("TOPLA", tablo.Getir("ipucu.topla", DilKodu.TR));
        Assert.AreEqual("PICK UP", tablo.Getir("ipucu.topla", DilKodu.EN));
        Assert.AreEqual(0, tablo.Hatalar.Count);
    }

    [Test]
    public void Aciklama_ve_bos_satirlar_atlanir()
    {
        var tablo = MetinTablosu.Oku(
            "# anahtar\tTR\tEN\n" +
            "\n" +
            "   \n" +
            "a" + T + "bir" + T + "one\n");

        Assert.AreEqual(1, tablo.Sayi);
        Assert.AreEqual(0, tablo.Hatalar.Count);
    }

    [Test]
    public void Ters_bolu_n_gercek_satir_sonuna_cevrilir()
    {
        var tablo = MetinTablosu.Oku("not" + T + "Satır 1\\nSatır 2" + T + "Line 1\\nLine 2");
        Assert.AreEqual("Satır 1\nSatır 2", tablo.Getir("not", DilKodu.TR));
    }

    [Test]
    public void Windows_satir_sonu_ve_BOM_anahtari_bozmaz()
    {
        // U+FEFF: bazı editörlerin dosya başına koyduğu görünmez işaret.
        var tablo = MetinTablosu.Oku(((char)0xFEFF) + "a" + T + "bir" + T + "one\r\nb" + T + "iki" + T + "two\r\n");

        Assert.IsTrue(tablo.Var("a"), "İlk anahtarın başına BOM yapışmamalı.");
        Assert.AreEqual("two", tablo.Getir("b", DilKodu.EN), "Sonda \\r kalmamalı.");
    }

    [Test]
    public void Tabloda_olmayan_anahtar_goze_batan_isaretle_doner()
    {
        var tablo = MetinTablosu.Oku("a" + T + "bir" + T + "one");
        Assert.AreEqual("#yok.boyle#", tablo.Getir("yok.boyle", DilKodu.TR));
    }

    [Test]
    public void Bos_ingilizce_turkceye_duser_ve_hata_kaydedilir()
    {
        var tablo = MetinTablosu.Oku("a" + T + "bir" + T + "  ");

        Assert.AreEqual("bir", tablo.Getir("a", DilKodu.EN));
        Assert.AreEqual(1, tablo.Hatalar.Count);
        StringAssert.Contains("EN", tablo.Hatalar[0]);
    }

    [Test]
    public void Iki_kez_yazilan_anahtar_hata_olur_ve_ilki_gecerli_kalir()
    {
        var tablo = MetinTablosu.Oku(
            "a" + T + "ilk" + T + "first\n" +
            "a" + T + "ikinci" + T + "second\n");

        Assert.AreEqual("ilk", tablo.Getir("a", DilKodu.TR));
        Assert.AreEqual(1, tablo.Hatalar.Count);
        StringAssert.Contains("2. satır", tablo.Hatalar[0]);
    }

    [Test]
    public void Yanlis_sutun_sayisi_hata_olur_ve_satir_alinmaz()
    {
        // İngilizce sütunu unutulmuş: TAB bir tane eksik.
        var tablo = MetinTablosu.Oku("a" + T + "bir\nb" + T + "iki" + T + "two");

        Assert.IsFalse(tablo.Var("a"));
        Assert.IsTrue(tablo.Var("b"));
        Assert.AreEqual(1, tablo.Hatalar.Count);
        StringAssert.Contains("1. satır", tablo.Hatalar[0]);
    }

    [Test]
    public void Metnin_icindeki_virgul_ve_tirnak_bozulmaz()
    {
        var tablo = MetinTablosu.Oku("a" + T + "Bir şey yanlışsa — sorma, \"geri dön\"." + T + "x");
        Assert.AreEqual("Bir şey yanlışsa — sorma, \"geri dön\".", tablo.Getir("a", DilKodu.TR));
    }

    [Test]
    public void Anahtarlar_listesi_tablodaki_butun_anahtarlari_icerir()
    {
        var tablo = MetinTablosu.Oku("a" + T + "1" + T + "1\nb" + T + "2" + T + "2");
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, tablo.Anahtarlar.ToArray());
    }

    [Test]
    public void Turkce_Windows_turkce_digerleri_ingilizce_baslar()
    {
        Assert.AreEqual(DilKodu.TR, DilSecimi.SistemDilinden("Turkish"));
        Assert.AreEqual(DilKodu.EN, DilSecimi.SistemDilinden("English"));
        Assert.AreEqual(DilKodu.DE, DilSecimi.SistemDilinden("German"));
        Assert.AreEqual(DilKodu.PT, DilSecimi.SistemDilinden("Portuguese"));
        Assert.AreEqual(DilKodu.ZH, DilSecimi.SistemDilinden("ChineseSimplified"));
        Assert.AreEqual(DilKodu.JA, DilSecimi.SistemDilinden("Japanese"));
        Assert.AreEqual(DilKodu.EN, DilSecimi.SistemDilinden("Swahili"), "bilinmeyen dil İngilizceye düşer");
        Assert.AreEqual("pt-BR", DilSecimi.KulturAdi(DilKodu.PT));
        Assert.AreEqual("", DilSecimi.KulturAdi(DilKodu.EN));
        Assert.AreEqual(DilKodu.EN, DilSecimi.SistemDilinden("Unknown"));
    }
}
