// SesMalzemesiTestleri.cs
// Sesin nesneye uyması kuralının testleri (#99).

using NUnit.Framework;

[TestFixture]
public class SesMalzemesiTestleri
{
    [Test]
    public void Buzdolabi_kavanoz_ve_cekmece_kendi_sesini_alir()
    {
        Assert.AreEqual(AcilirSesTuru.Buzdolabi, SesMalzemesi.AcilirTuru("Sus_Buzdolabi_K5/Kapi_1", false));
        Assert.AreEqual(AcilirSesTuru.Kavanoz, SesMalzemesi.AcilirTuru("Esyalar/Kavanoz/Kapak", false));
        Assert.AreEqual(AcilirSesTuru.Kavanoz, SesMalzemesi.AcilirTuru("Esyalar/Lavabo/Sifon", true), "sökülen sifon kayan çekmece değil, vidalı parça");
        Assert.AreEqual(AcilirSesTuru.Cekmece, SesMalzemesi.AcilirTuru("Komodin_Ayla/Cekmece_1", true));
        Assert.AreEqual(AcilirSesTuru.Cekmece, SesMalzemesi.AcilirTuru("Esyalar/Portmanto/Cekmece", false));
    }

    [Test]
    public void Metal_dolaplar_metal_sesi_alir()
    {
        Assert.AreEqual(AcilirSesTuru.Metal, SesMalzemesi.AcilirTuru("Gardirop_Cemal/Govde/Kapi_1", false), "sahnedeki adı (DA) da tanınır");
        Assert.AreEqual(AcilirSesTuru.Metal, SesMalzemesi.AcilirTuru("Gardirop_K8/Kapi_2", false));
        Assert.AreEqual(AcilirSesTuru.Metal, SesMalzemesi.AcilirTuru("Esyalar/Sigorta_Kutusu/Kapak", false));
        Assert.AreEqual(AcilirSesTuru.Metal, SesMalzemesi.AcilirTuru("Dosya_Dolabi_K8/Cekmece_2", true), "metal dosya dolabının çekmecesi de metal");
        Assert.IsTrue(SesMalzemesi.MetalMi("Esyalar/Alet_Cantasi"));
        Assert.IsFalse(SesMalzemesi.MetalMi("Gardirop_K3"));
    }

    [Test]
    public void Itilen_gardirop_agir_mobilya_sesi_alir()
    {
        Assert.AreEqual(AcilirSesTuru.AgirMobilya, SesMalzemesi.AcilirTuru("Esyalar/Gardirop", true));
        Assert.AreEqual(AcilirSesTuru.AhsapKapak, SesMalzemesi.AcilirTuru("Gardirop_K3/Kapi_1", false), "gardırobun kapağı ahşap kapak");
    }

    [Test]
    public void Bilinmeyen_donen_parca_ahsap_kapak_sayilir()
    {
        Assert.AreEqual(AcilirSesTuru.AhsapKapak, SesMalzemesi.AcilirTuru("Bufe_Cemal/Kapi_1", false));
        Assert.AreEqual(AcilirSesTuru.AhsapKapak, SesMalzemesi.AcilirTuru(null, false));
    }

    [Test]
    public void Alma_sesi_esyanin_turunden()
    {
        Assert.AreEqual(AlmaSesTuru.Metal, SesMalzemesi.AlmaTuru(EsyaTuru.Tornavida));
        Assert.AreEqual(AlmaSesTuru.Metal, SesMalzemesi.AlmaTuru(EsyaTuru.Levye));
        Assert.AreEqual(AlmaSesTuru.Kagit, SesMalzemesi.AlmaTuru(EsyaTuru.Parca));
        Assert.AreEqual(AlmaSesTuru.Seramik, SesMalzemesi.AlmaTuru(EsyaTuru.Tabak));
        Assert.AreEqual(AlmaSesTuru.Kucuk, SesMalzemesi.AlmaTuru(EsyaTuru.Sigorta));
        Assert.AreEqual(AlmaSesTuru.Anahtar, SesMalzemesi.AlmaTuru(EsyaTuru.OdaAnahtari));
        Assert.AreEqual(AlmaSesTuru.Anahtar, SesMalzemesi.AlmaTuru(EsyaTuru.AnahtarParcasi));
    }

    [Test]
    public void Her_turun_bir_dosyasi_var()
    {
        foreach (AcilirSesTuru t in System.Enum.GetValues(typeof(AcilirSesTuru)))
        {
            StringAssert.StartsWith("Sesler/", SesMalzemesi.AcilmaDosyasi(t));
            StringAssert.StartsWith("Sesler/", SesMalzemesi.KilitliDosyasi(t));
        }
        foreach (AlmaSesTuru t in System.Enum.GetValues(typeof(AlmaSesTuru)))
            StringAssert.StartsWith("Sesler/", SesMalzemesi.AlmaDosyasi(t));
    }
}
