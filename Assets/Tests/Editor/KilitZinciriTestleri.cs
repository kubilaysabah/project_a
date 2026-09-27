// KilitZinciriTestleri.cs
// Kilit zinciri kuralının testleri (#54). Her test küçük bir zincir kurar ve kuralın
// "çözülebilir / çözülemez" kararını denetler. Katların gerçek zincirleri (#55) aynı kuralla
// denetlenecek; bu testler kuralın kendisinin doğru çalıştığını kanıtlar.

using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class KilitZinciriTestleri
{
    // Plandaki oda adları.
    private static List<string> Odalar()
    {
        var l = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) l.Add(o.Ad);
        return l;
    }

    private static ZincirYeri Yer(string kimlik, string oda, string yol = "") { return new ZincirYeri { Kimlik = kimlik, Oda = oda, Yol = yol }; }
    private static ZincirEsyasi Esya(string kimlik, EsyaTuru tur, string yer) { return new ZincirEsyasi { Kimlik = kimlik, Tur = tur, Yer = yer }; }
    private static ZincirKilidi OdaKilidi(string kimlik, KilitTuru tur, string oda, string gerekli, string sifre = "")
    { return new ZincirKilidi { Kimlik = kimlik, Tur = tur, OdaKapisi = true, Hedef = oda, Gerekli = gerekli, Sifre = sifre }; }
    private static ZincirKilidi YerKilidi(string kimlik, KilitTuru tur, string yer, string gerekli, string sifre = "")
    { return new ZincirKilidi { Kimlik = kimlik, Tur = tur, OdaKapisi = false, Hedef = yer, Gerekli = gerekli, Sifre = sifre }; }

    // 1. kattaki zincir (Docs/hikaye.md §7): Oda A'nın anahtarı mutfakta kavanozda, kat anahtarı
    // Oda A'nın komodininde.
    private static KilitZinciri BirinciKat()
    {
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("kavanoz", "Mutfak"));
        z.Yerler.Add(Yer("komodin", "Oda_A"));
        z.Esyalar.Add(Esya("odaA_anahtari", EsyaTuru.OdaAnahtari, "kavanoz"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "komodin"));
        z.Kilitler.Add(OdaKilidi("odaA", KilitTuru.Anahtar, "Oda_A", "odaA_anahtari"));
        return z;
    }

    [Test]
    public void Birinci_kat_zinciri_cozulur()
    {
        var s = BirinciKat().Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        CollectionAssert.AreEqual(new[] { "odaA" }, s.AcilanKilitler);
    }

    [Test]
    public void Anahtar_kendi_kilitledigi_odada_kalirsa_cozulmez()
    {
        var z = BirinciKat();
        z.YerBul("kavanoz").Oda = "Oda_A";   // anahtar kilitli odanın içinde
        var s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        StringAssert.Contains("odaA", string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Kapali_odadaki_esyaya_ulasilamaz()
    {
        var z = BirinciKat();
        z.KapaliOdalar.Add("Mutfak");         // anahtarın durduğu mutfak o katta hiç açılmıyor
        Assert.IsFalse(z.Cozumle(Odalar()).Cozulebilir);
    }

    [Test]
    public void Yanlis_turde_esya_veri_hatasidir()
    {
        var z = BirinciKat();
        z.EsyaBul("odaA_anahtari").Tur = EsyaTuru.Tornavida;   // anahtar kilidi tornavidayla açılmaz
        var s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        StringAssert.Contains("OdaAnahtari", string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Olmayan_oda_ve_yer_veri_hatasidir()
    {
        var z = BirinciKat();
        z.Yerler.Add(Yer("dolap", "Mutfak_2"));
        z.Kilitler.Add(YerKilidi("x", KilitTuru.Anahtar, "yok_boyle_yer", "odaA_anahtari"));
        var s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        Assert.GreaterOrEqual(s.Hatalar.Count, 2);
    }

    [Test]
    public void Tek_kat_anahtari_olmali()
    {
        var z = BirinciKat();
        z.Esyalar.Add(Esya("ikinci", EsyaTuru.KatAnahtari, "kavanoz"));
        Assert.IsFalse(z.Cozumle(Odalar()).Cozulebilir);
    }

    [Test]
    public void Sifre_ipucu_kilitli_odadaysa_once_o_oda_acilmali()
    {
        // 3. kat benzeri: Oda B'nin anahtarı salondaki oyuncak kutusunda; Oda B'deki dolap asma
        // kilitli, şifre Oda B'deki boy cetvelinde; kat anahtarı dolapta.
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("oyuncak", "Salon"));
        z.Yerler.Add(Yer("cetvel", "Oda_B"));
        z.Yerler.Add(Yer("dolap", "Oda_B"));
        z.Esyalar.Add(Esya("odaB_anahtari", EsyaTuru.OdaAnahtari, "oyuncak"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "dolap"));
        z.Kilitler.Add(OdaKilidi("odaB", KilitTuru.Anahtar, "Oda_B", "odaB_anahtari"));
        z.Kilitler.Add(YerKilidi("dolap", KilitTuru.Sifre, "dolap", "cetvel", "1987"));
        var s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        CollectionAssert.AreEqual(new[] { "odaB", "dolap" }, s.AcilanKilitler, "önce oda, sonra dolap");
    }

    [Test]
    public void Sifre_dort_rakam_olmali()
    {
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("takvim", "Salon"));
        z.Yerler.Add(Yer("kiler_rafi", "Oda_C"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "kiler_rafi"));
        z.Kilitler.Add(OdaKilidi("kiler", KilitTuru.Sifre, "Oda_C", "takvim", "19A7"));
        Assert.IsFalse(z.Cozumle(Odalar()).Cozulebilir);
        z.Kilitler[0].Sifre = "1987";
        Assert.IsTrue(z.Cozumle(Odalar()).Cozulebilir);
    }

    [Test]
    public void Kol_kilidi_kola_ulasinca_acilir()
    {
        // Kilerdeki kol banyoyu açar; kiler ise Oda A'nın anahtarıyla açılır.
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("kavanoz", "Mutfak"));
        z.Yerler.Add(Yer("kol", "Oda_C"));
        z.Yerler.Add(Yer("lavabo", "Banyo"));
        z.Esyalar.Add(Esya("kiler_anahtari", EsyaTuru.OdaAnahtari, "kavanoz"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "lavabo"));
        z.Kilitler.Add(OdaKilidi("kiler", KilitTuru.Anahtar, "Oda_C", "kiler_anahtari"));
        z.Kilitler.Add(OdaKilidi("banyo", KilitTuru.Kol, "Banyo", "kol"));
        var s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        CollectionAssert.AreEqual(new[] { "kiler", "banyo" }, s.AcilanKilitler);
    }

    [Test]
    public void Bes_turun_hepsi_tek_zincirde_cozulur()
    {
        // Anahtar -> şifre -> kol -> sigorta -> tornavida: test zincirinin şekli.
        var z = new KilitZinciri();
        z.KapaliOdalar.Add("Oda_B");
        z.Yerler.Add(Yer("kavanoz", "Mutfak"));
        z.Yerler.Add(Yer("takvim", "Salon"));
        z.Yerler.Add(Yer("kol", "Oda_C"));
        z.Yerler.Add(Yer("komodin", "Oda_A"));
        z.Yerler.Add(Yer("buzdolabi", "Mutfak"));
        z.Yerler.Add(Yer("alet", "Oda_C"));
        z.Yerler.Add(Yer("lavabo", "Banyo"));
        z.Esyalar.Add(Esya("salon_anahtari", EsyaTuru.OdaAnahtari, "kavanoz"));
        z.Esyalar.Add(Esya("sigorta", EsyaTuru.Sigorta, "komodin"));
        z.Esyalar.Add(Esya("tornavida", EsyaTuru.Tornavida, "buzdolabi"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "lavabo"));
        z.Kilitler.Add(OdaKilidi("salon", KilitTuru.Anahtar, "Salon", "salon_anahtari"));
        z.Kilitler.Add(OdaKilidi("kiler", KilitTuru.Sifre, "Oda_C", "takvim", "0612"));
        z.Kilitler.Add(OdaKilidi("odaA", KilitTuru.Kol, "Oda_A", "kol"));
        z.Kilitler.Add(YerKilidi("buzdolabi", KilitTuru.Sigorta, "buzdolabi", "sigorta"));
        z.Kilitler.Add(OdaKilidi("banyo", KilitTuru.Tornavida, "Banyo", "tornavida"));
        var s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        CollectionAssert.AreEqual(new[] { "salon", "kiler", "odaA", "buzdolabi", "banyo" }, s.AcilanKilitler);
    }
}
