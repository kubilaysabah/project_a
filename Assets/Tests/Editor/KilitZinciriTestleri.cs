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
    public void Kirma_aleti_sahanlikta_bulunur_ve_zinciri_baslatir()
    {
        // #61: kapının önündeki ayakkabılıkta tornavida; tornavidayla kavanoz kırılır, içinde
        // salon anahtarı; kat anahtarı salonda. Aynı tornavida banyonun menteşelerini de söker.
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("ayakkabilik", KilitZinciri.SahanlikOdasi));
        z.Yerler.Add(Yer("kavanoz", "Mutfak"));
        z.Yerler.Add(Yer("takvim", "Salon"));
        z.Esyalar.Add(Esya("tornavida", EsyaTuru.Tornavida, "ayakkabilik"));
        z.Esyalar.Add(Esya("salon_anahtari", EsyaTuru.OdaAnahtari, "kavanoz"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "takvim"));
        z.Kilitler.Add(YerKilidi("kavanoz", KilitTuru.Kirma, "kavanoz", "tornavida"));
        z.Kilitler.Add(OdaKilidi("salon", KilitTuru.Anahtar, "Salon", "salon_anahtari"));
        z.Kilitler.Add(OdaKilidi("banyo", KilitTuru.Tornavida, "Banyo", "tornavida"));
        var s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        Assert.AreEqual("kavanoz", s.AcilanKilitler[0], "önce kavanoz kırılır");
    }

    [Test]
    public void Kirma_yalniz_aletle_acilir()
    {
        var z = new KilitZinciri();
        z.Yerler.Add(Yer("kavanoz", "Mutfak"));
        z.Yerler.Add(Yer("raf", "Mutfak"));
        z.Esyalar.Add(Esya("bir_anahtar", EsyaTuru.OdaAnahtari, "raf"));
        z.Esyalar.Add(Esya("kat_anahtari", EsyaTuru.KatAnahtari, "kavanoz"));
        z.Kilitler.Add(YerKilidi("kavanoz", KilitTuru.Kirma, "kavanoz", "bir_anahtar"));   // anahtarla kırılmaz
        var s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        StringAssert.Contains("Tornavida", string.Join("; ", s.Hatalar));
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

    // ---------- #63: yırtık çizim ve planda nokta olan yerler ----------

    // Salonun yerinde bir parça, mutfakta bir parça; şifreli Oda A'da katın anahtarı.
    private static KilitZinciri ParcaliZincir()
    {
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Oda_B", "Oda_C", "Banyo" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", PlandaNokta = true, Oda = "Salon", U = 1.0f, V = 7.0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "tezgah", Yol = "Esyalar/Tezgah", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p1", Tur = EsyaTuru.Parca, Yer = "salon_yeri" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p2", Tur = EsyaTuru.Parca, Yer = "tezgah" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Sifre = "0309",
                                          Parcalar = new List<string> { "p1", "p2" } });
        return z;
    }

    [Test]
    public void Parcali_sifre_butun_parcalara_ulasilinca_cozulur()
    {
        KilitZinciri.Sonuc s = ParcaliZincir().Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Parcanin_biri_kapali_odadaysa_sifre_okunamaz()
    {
        KilitZinciri z = ParcaliZincir();
        z.KapaliOdalar.Add("Mutfak");                        // ikinci parça artık ulaşılamaz
        KilitZinciri.Sonuc s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        Assert.IsTrue(s.Hatalar.Exists(h => h.Contains("odaA")), string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Parca_olmayan_esya_parca_listesinde_hata()
    {
        KilitZinciri z = ParcaliZincir();
        z.Esyalar[0].Tur = EsyaTuru.OdaAnahtari;
        KilitZinciri.Sonuc s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Hatalar.Exists(h => h.Contains("Parca değil")), string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Planda_nokta_olan_yer_kendi_odasinda_olmali()
    {
        KilitZinciri z = ParcaliZincir();
        z.Yerler[0].U = 9.0f;                               // Oda A'ya düşer, salon yazıyor
        KilitZinciri.Sonuc s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Hatalar.Exists(h => h.Contains("salon_yeri")), string.Join("; ", s.Hatalar));
    }

    // ---------- #65: birkaç aletle açılan kilit (ağır gardırop) ----------

    private static KilitZinciri EkipmanliZincir()
    {
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Mutfak", "Oda_A", "Oda_B", "Banyo" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_yeri", Yol = "x", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", Yol = "y", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "vana", Yol = "Esyalar/Vana/Cark", Oda = "Salon", KilitYolu = "Esyalar/Gardirop" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sifon", Yol = "z", Oda = "Salon" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "levye", Tur = EsyaTuru.Levye, Yer = "kiler_yeri" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kaydirici", Tur = EsyaTuru.Kaydirici, Yer = "salon_yeri" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sifon" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "gardirop", Tur = KilitTuru.Ekipman, OdaKapisi = false, Hedef = "vana",
                                          Gerekenler = new List<string> { "levye", "kaydirici" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sifon", Tur = KilitTuru.Kol, OdaKapisi = false, Hedef = "sifon", Gerekli = "vana" });
        return z;
    }

    [Test]
    public void Ekipman_kilidi_butun_aletler_bulununca_acilir_ve_arkasindaki_kola_ulasilir()
    {
        KilitZinciri.Sonuc s = EkipmanliZincir().Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        Assert.AreEqual(new List<string> { "gardirop", "sifon" }, s.AcilanKilitler);
    }

    [Test]
    public void Aletlerden_biri_kapali_odadaysa_gardirop_ve_vana_acilmaz()
    {
        KilitZinciri z = EkipmanliZincir();
        z.KapaliOdalar.Add("Oda_C");                         // levye artık ulaşılamaz
        KilitZinciri.Sonuc s = z.Cozumle(Odalar());
        Assert.IsFalse(s.Cozulebilir);
        Assert.IsTrue(s.Hatalar.Exists(h => h.Contains("gardirop")) && s.Hatalar.Exists(h => h.Contains("sifon")), string.Join("; ", s.Hatalar));
    }

    [Test]
    public void Ekipman_listesinde_alet_olmayan_esya_hata()
    {
        KilitZinciri z = EkipmanliZincir();
        z.Esyalar[1].Tur = EsyaTuru.OdaAnahtari;
        KilitZinciri.Sonuc s = z.Cozumle(Odalar());
        Assert.IsTrue(s.Hatalar.Exists(h => h.Contains("bir alet değil")), string.Join("; ", s.Hatalar));
    }
}
