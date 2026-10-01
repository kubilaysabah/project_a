// TakilmaKuraliTestleri.cs
// Takılan oyuncuya yazısız ipucu kuralının testleri (#98): ilk ipucunun zamanı, tekrar aralığı,
// 4 ipucu tavanı, ilerlemede ve yeni katta sıfırlanma, uygun değilken saatin durması; ve ipucunun
// zincirin çözüm sırasına göre doğru eşyayı (ya da kilidi) seçmesi.

using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class TakilmaKuraliTestleri
{
    // Saniyede 10 kare, 'saniye' kadar ilerletir; kaç ipucu çaldığını sayar.
    private static int Oynat(TakilmaKurali k, float saniye, bool uygun = true)
    {
        int sayi = 0;
        int kare = (int)System.Math.Round(saniye * 10f);
        for (int i = 0; i < kare; i++)
            if (k.Ilerlet(0.1f, uygun)) sayi++;
        return sayi;
    }

    private static TakilmaKurali Yeni(int kat = 3)
    {
        var k = new TakilmaKurali();
        k.KatAyarla(kat);
        return k;
    }

    // ---------- SAAT ----------

    [Test]
    public void Ilk_ipucu_150_saniyede_gelir()
    {
        var k = Yeni();
        Assert.AreEqual(0, Oynat(k, 149.5f), "150 saniyeden önce ipucu yok");
        Assert.AreEqual(1, Oynat(k, 1f), "150. saniyede ilk ipucu");
        Assert.AreEqual(1, k.IpucuSayisi);
    }

    [Test]
    public void Sonraki_ipuclari_75_saniye_arayla_gelir()
    {
        var k = Yeni();
        Oynat(k, 151f);                                      // ilk ipucu 150. saniyede; 1 sn artar
        Assert.AreEqual(0, Oynat(k, 73.5f), "75 saniyeden önce ikinci ipucu yok (1 + 73,5 = 74,5)");
        Assert.AreEqual(1, Oynat(k, 1f), "75 saniye sonra ikinci ipucu");
        Assert.AreEqual(2, k.IpucuSayisi);
    }

    [Test]
    public void Bir_ilerleme_doneminde_en_cok_4_ipucu()
    {
        var k = Yeni();
        // 150 + 3 × 75 = 375 saniyede dördüncü; çok daha uzun beklense de beşinci gelmez.
        Assert.AreEqual(TakilmaKurali.EnCokIpucu, Oynat(k, 2000f));
        Assert.AreEqual(4, k.IpucuSayisi);
    }

    [Test]
    public void Ilerleme_saati_ve_sayiyi_sifirlar()
    {
        var k = Yeni();
        Oynat(k, 2000f);                                     // tavan doldu
        k.Ilerleme();
        Assert.AreEqual(0, k.IpucuSayisi);
        Assert.AreEqual(0f, k.IlerlemesizSure);
        // Yeniden ilk ipucu kuralı: 150 saniye (75 değil).
        Assert.AreEqual(0, Oynat(k, 100f));
        Assert.AreEqual(1, Oynat(k, 51f));
    }

    [Test]
    public void Uygun_degilken_saat_durur_ama_sifirlanmaz()
    {
        var k = Yeni();
        Oynat(k, 100f);                                      // 100 sn uygun
        Assert.AreEqual(0, Oynat(k, 500f, uygun: false), "merdivende/menüde ipucu yok");
        Assert.AreEqual(100f, k.IlerlemesizSure, 0.05f, "uygun olmayan süre sayılmaz");
        Assert.AreEqual(1, Oynat(k, 51f), "kalan 50 saniye dolunca ipucu gelir");
    }

    [Test]
    public void Yeni_kat_her_seyi_sifirlar()
    {
        var k = Yeni(3);
        Oynat(k, 260f);                                      // iki ipucu (150 ve 225. saniye)
        Assert.AreEqual(2, k.IpucuSayisi);
        k.KatAyarla(4);
        Assert.AreEqual(4, k.Kat);
        Assert.AreEqual(0, k.IpucuSayisi);
        Assert.AreEqual(0, Oynat(k, 149f));
        Assert.AreEqual(1, Oynat(k, 2f));
    }

    [Test]
    public void Uzun_tek_kare_ust_uste_iki_ipucu_caldirmaz()
    {
        var k = Yeni();
        Assert.IsTrue(k.Ilerlet(1000f, true));               // takılan bir yükleme karesi
        Assert.IsFalse(k.Ilerlet(0.1f, true), "hemen ardından ikinci ipucu yok");
        Assert.AreEqual(1, k.IpucuSayisi);
    }

    [Test]
    public void Bozuk_sure_saati_ilerletmez()
    {
        var k = Yeni();
        Assert.IsFalse(k.Ilerlet(float.NaN, true));
        Assert.IsFalse(k.Ilerlet(-5f, true));
        Assert.AreEqual(0f, k.IlerlemesizSure);
    }

    // ---------- HEDEF ----------

    private static List<string> Odalar() { return TakilmaKurali.ButunOdalar(); }

    // Küçük bir zincir: tornavida antrede, kavanoz mutfakta (tornavidayla kırılır), içinde Oda A'nın
    // anahtarı; Oda A'da salonun kolu (salon kapısını açar); kat anahtarı salonda.
    private static KilitZinciri Zincir()
    {
        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "portmanto", Oda = "Antre" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kavanoz", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kol", Oda = "Oda_A" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sehpa", Oda = "Salon" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "portmanto" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "kavanoz" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sehpa" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kavanoz_k", Tur = KilitTuru.Kirma, Hedef = "kavanoz", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "odaA_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Salon", Gerekli = "kol" });
        return z;
    }

    private static List<TakilmaHedefi> Adaylar(KilitZinciri z, ICollection<string> bulunan, ICollection<string> cozuk)
    {
        List<string> sira = z.Cozumle(Odalar()).AcilanKilitler;
        // "x => bulunan.Contains(x)": tek satırlık fonksiyon; kurala "bu eşya bulundu mu" diye sorulan.
        return TakilmaKurali.Adaylar(z, sira, x => bulunan.Contains(x), x => cozuk.Contains(x));
    }

    [Test]
    public void Test_zinciri_cozulur()
    {
        var s = Zincir().Cozumle(Odalar());
        Assert.IsTrue(s.Cozulebilir, string.Join("; ", s.Hatalar));
        CollectionAssert.AreEqual(new[] { "kavanoz_k", "odaA", "salon" }, s.AcilanKilitler);
    }

    [Test]
    public void Hic_ilerleme_yokken_once_ilk_kilidin_aleti_sonra_kilidin_kendisi()
    {
        var a = Adaylar(Zincir(), new string[0], new string[0]);
        Assert.AreEqual(2, a.Count);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Esya, "tornavida"), a[0]);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kilit, "kavanoz_k"), a[1]);
    }

    [Test]
    public void Esya_elindeyken_kilidin_kendisi_isaret_edilir()
    {
        var a = Adaylar(Zincir(), new[] { "tornavida" }, new string[0]);
        Assert.AreEqual(1, a.Count);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kilit, "kavanoz_k"), a[0]);
    }

    [Test]
    public void Cozulen_kilitten_sonraki_kilidin_esyasi_secilir()
    {
        var a = Adaylar(Zincir(), new[] { "tornavida" }, new[] { "kavanoz_k" });
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Esya, "odaA_anahtari"), a[0]);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kilit, "odaA"), a[1]);
    }

    [Test]
    public void Kol_kilidinde_once_kol_isaret_edilir()
    {
        var a = Adaylar(Zincir(), new[] { "tornavida", "odaA_anahtari" }, new[] { "kavanoz_k", "odaA" });
        Assert.AreEqual(2, a.Count);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kol, "kol"), a[0]);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kilit, "salon"), a[1]);
    }

    [Test]
    public void Butun_kilitler_cozulunce_kat_anahtari_sonra_hicbir_sey()
    {
        var hepsi = new[] { "kavanoz_k", "odaA", "salon" };
        var a = Adaylar(Zincir(), new[] { "tornavida", "odaA_anahtari" }, hepsi);
        Assert.AreEqual(1, a.Count);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Esya, "kat_anahtari"), a[0]);
        // Kat anahtarı da bulununca işaret edilecek bir şey kalmaz (oyuncu kat kapısına gider).
        Assert.AreEqual(0, Adaylar(Zincir(), new[] { "tornavida", "odaA_anahtari", "kat_anahtari" }, hepsi).Count);
    }

    [Test]
    public void Kirik_anahtarin_bulunmamis_parcalari_sirayla_aday()
    {
        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "canta", Oda = "Antre" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sehpa", Oda = "Salon" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "a_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "buzdolabi" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "a_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "canta" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sehpa" });
        z.Kilitler.Add(new ZincirKilidi
        {
            Kimlik = "salon", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Salon",
            Gerekenler = new List<string> { "a_bas", "a_uc" }
        });
        var a = Adaylar(z, new[] { "a_bas" }, new string[0]);
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Esya, "a_uc"), a[0], "bulunan baş atlanır, uç işaret edilir");
        Assert.AreEqual(new TakilmaHedefi(TakilmaHedefTuru.Kilit, "salon"), a[1]);
    }

    [Test]
    public void Eksik_veriyle_bos_liste()
    {
        Assert.AreEqual(0, TakilmaKurali.Adaylar(null, new List<string>(), x => false, x => false).Count);
        Assert.AreEqual(0, TakilmaKurali.Adaylar(Zincir(), null, x => false, x => false).Count);
    }
}
