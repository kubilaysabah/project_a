// GerilimKuraliTestleri.cs
// Varlığın gerilim kuralının testleri (#86): ısınma/soğuma hızları, kademe eşikleri, kat çarpanı,
// av sonrası sıfırlama.

using NUnit.Framework;

[TestFixture]
public class GerilimKuraliTestleri
{
    // Saniyede 10 kare, verilen durumla 'saniye' kadar ilerletir; dönen tepkileri sayar.
    private static int Oynat(GerilimKurali k, float saniye, bool dairede, bool lamba, bool egik, bool hareketli,
                             GerilimTepkisi aranan = GerilimTepkisi.Yok)
    {
        int sayi = 0;
        int kare = (int)(saniye * 10f);
        for (int i = 0; i < kare; i++)
            if (k.Ilerlet(0.1f, dairede, lamba, egik, hareketli) == aranan && aranan != GerilimTepkisi.Yok) sayi++;
        return sayi;
    }

    [Test]
    public void Baslangicta_sakin()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(4);
        Assert.AreEqual(0f, k.Deger);
        Assert.AreEqual(0, k.Kademe);
        Assert.IsFalse(k.AvSuruyor);
    }

    [Test]
    public void Dairede_lamba_acik_dururken_dolar()
    {
        // 1. katta av yok (0,85'te durmaz), çarpan 0,6: 285 / 0,6 = 475 sn'de dolar.
        var k = new GerilimKurali(1);
        k.KatAyarla(1);
        Oynat(k, 480f, dairede: true, lamba: true, egik: false, hareketli: false);
        Assert.That(k.Deger, Is.InRange(0.95f, 1.0f));
        // 4. katta ise 0,85'e varınca av başlar ve kural orada bekler.
        var d = new GerilimKurali(1);
        d.KatAyarla(4);
        Oynat(d, 280f, true, true, false, false);
        Assert.AreEqual(GerilimKurali.AvEsigi, d.Deger, 0.01f);
        Assert.IsTrue(d.AvSuruyor);
    }

    [Test]
    public void Yurumek_isinmayi_hizlandirir()
    {
        var duran = new GerilimKurali(1); duran.KatAyarla(4);
        var yuruyen = new GerilimKurali(1); yuruyen.KatAyarla(4);
        Oynat(duran, 60f, true, true, false, false);
        Oynat(yuruyen, 60f, true, true, false, true);
        Assert.Greater(yuruyen.Deger, duran.Deger);
    }

    [Test]
    public void Kademeler_sirayla_girilir_ve_giris_tepkileri_doner()
    {
        var k = new GerilimKurali(2);
        k.KatAyarla(4);
        var gorulen = new System.Collections.Generic.List<GerilimTepkisi>();
        for (int i = 0; i < 3000 && !k.AvSuruyor; i++)          // 300 sn
        {
            GerilimTepkisi t = k.Ilerlet(0.1f, true, true, false, false);
            if (t == GerilimTepkisi.Titreme && !gorulen.Contains(GerilimTepkisi.Titreme)) gorulen.Add(t);
            if (t == GerilimTepkisi.Golge && !gorulen.Contains(GerilimTepkisi.Golge)) gorulen.Add(t);
            if (t == GerilimTepkisi.Av) gorulen.Add(t);
        }
        Assert.That(gorulen, Is.EqualTo(new[] { GerilimTepkisi.Titreme, GerilimTepkisi.Golge, GerilimTepkisi.Av }));
        Assert.IsTrue(k.AvSuruyor);
        Assert.AreEqual(3, k.Kademe);
    }

    [Test]
    public void Av_surerken_kural_bekler_ve_av_bitince_sifirlanir()
    {
        var k = new GerilimKurali(2);
        k.KatAyarla(5);
        Oynat(k, 300f, true, true, false, true);           // ava kadar
        Assert.IsTrue(k.AvSuruyor);
        // Av sürerken hiçbir tepki dönmez, gürültü de işlemez.
        Assert.AreEqual(0, Oynat(k, 60f, true, true, false, true, GerilimTepkisi.Titreme));
        k.Gurultu(1f);
        Assert.IsTrue(k.AvSuruyor);
        k.AvBitti();
        Assert.IsFalse(k.AvSuruyor);
        Assert.AreEqual(0f, k.Deger);
        Assert.AreEqual(0, k.Kademe);
    }

    [Test]
    public void Gurultu_bir_anda_yukseltir_ve_kat_carpaniyla_olcur()
    {
        var alt = new GerilimKurali(1); alt.KatAyarla(2);   // 0,8
        var ust = new GerilimKurali(1); ust.KatAyarla(5);   // 1,1 (8. kat #112 ile 1,0 oldu)
        alt.Gurultu(0.25f);
        ust.Gurultu(0.25f);
        Assert.AreEqual(0.20f, alt.Deger, 1e-4f);
        Assert.AreEqual(0.275f, ust.Deger, 1e-4f);
        ust.Gurultu(5f);
        Assert.AreEqual(1f, ust.Deger, "1'i aşmaz");
    }

    [Test]
    public void Sessiz_saklanmak_hizla_sogutur()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(4);
        k.Gurultu(0.8f);
        Oynat(k, 45f, dairede: true, lamba: false, egik: true, hareketli: false);   // 45 × 0,02 = 0,9
        Assert.AreEqual(0f, k.Deger, 1e-3f);
    }

    [Test]
    public void Yalnizca_lambayi_kapatmak_az_sogutur()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(4);
        k.Gurultu(0.5f);
        Oynat(k, 30f, true, lamba: false, egik: false, hareketli: true);              // 30 × 0,004 = 0,12
        Assert.AreEqual(0.38f, k.Deger, 1e-3f);
    }

    [Test]
    public void Sahanlikta_yavas_sogur()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(4);
        k.Gurultu(0.5f);
        Oynat(k, 100f, dairede: false, lamba: true, egik: false, hareketli: true);   // 100 × 0,003 = 0,3
        Assert.AreEqual(0.2f, k.Deger, 1e-3f);
    }

    [Test]
    public void Sogurken_kademe_sessizce_iner()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(4);
        k.Gurultu(0.7f);
        Assert.AreEqual(GerilimTepkisi.Golge, k.Ilerlet(0.1f, true, true, false, false), "0,7: 2. kademeye giriş");
        int tepki = 0;
        for (int i = 0; i < 400; i++) if (k.Ilerlet(0.1f, true, false, true, false) != GerilimTepkisi.Yok) tepki++;
        Assert.AreEqual(0, k.Kademe);
        Assert.AreEqual(0, tepki, "inerken tepki yok");
        // Yeniden ısınınca 1. kademeye GİRİŞ tepkisi yine gelir.
        k.Gurultu(0.35f);
        Assert.AreEqual(GerilimTepkisi.Titreme, k.Ilerlet(0.1f, true, true, false, false));
    }

    [Test]
    public void Esikte_dalgalanmak_giris_tepkisini_tekrarlatmaz()
    {
        // #112: 0,30'un hemen altına inip yeniden çıkmak her seferinde Titreme döndürüyordu.
        var k = new GerilimKurali(1);
        k.KatAyarla(5);
        k.Gurultu(0.31f);
        Assert.AreEqual(GerilimTepkisi.Titreme, k.Ilerlet(0.1f, true, true, false, false), "ilk giriş");
        // 5. katta 0,31 × 1,1 = 0,341. Sessiz saklanma 0,02/sn soğutur: 3 sn'de 0,281 → eşiğin altında
        // ama payın (0,25) üstünde.
        int tepki = 0;
        for (int i = 0; i < 30; i++) if (k.Ilerlet(0.1f, true, false, true, false) != GerilimTepkisi.Yok) tepki++;
        Assert.Greater(k.Deger, GerilimKurali.TitremeEsigi - GerilimKurali.KademePayi);
        Assert.Less(k.Deger, GerilimKurali.TitremeEsigi);
        Assert.AreEqual(1, k.Kademe, "payın içinde kademe düşmez");
        // Yeniden 0,30'un üstüne çıkar: kademe zaten 1, giriş tepkisi YOK.
        k.Gurultu(0.02f);
        for (int i = 0; i < 20; i++) if (k.Ilerlet(0.1f, true, true, false, false) != GerilimTepkisi.Yok) tepki++;
        Assert.AreEqual(0, tepki, "dalgalanma tepki üretmez");
        // Payın altına inince (0,25) kademe düşer; sonra yükselince giriş tepkisi yine gelir (20 sn geçti).
        k.Gurultu(0f);
        for (int i = 0; i < 400; i++) k.Ilerlet(0.1f, true, false, true, false);   // sessiz soğuma
        Assert.AreEqual(0, k.Kademe);
        k.Gurultu(0.35f);
        Assert.AreEqual(GerilimTepkisi.Titreme, k.Ilerlet(0.1f, true, true, false, false));
    }

    [Test]
    public void Iki_giris_tepkisi_arasinda_en_az_20_saniye()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(5);
        k.Gurultu(0.35f);
        Assert.AreEqual(GerilimTepkisi.Titreme, k.Ilerlet(0.1f, true, true, false, false));
        // Hemen ardından 2. kademeye sıçra: kademe yükselir ama giriş tepkisi sessizdir.
        k.Gurultu(0.30f);
        Assert.AreEqual(GerilimTepkisi.Yok, k.Ilerlet(0.1f, true, true, false, false));
        Assert.AreEqual(2, k.Kademe);
        // 20 sn sonra soğuyup yeniden 2. kademeye girince giriş tepkisi (Golge) gelir.
        for (int i = 0; i < 400; i++) k.Ilerlet(0.1f, true, false, true, false);   // sıfıra iner
        Assert.AreEqual(0, k.Kademe);
        k.Gurultu(0.65f);
        Assert.AreEqual(GerilimTepkisi.Golge, k.Ilerlet(0.1f, true, true, false, false));
    }

    [Test]
    public void Av_bitince_270_saniye_yeni_av_yok()
    {
        var k = new GerilimKurali(2);
        k.KatAyarla(5);
        Oynat(k, 300f, true, true, false, true);
        Assert.IsTrue(k.AvSuruyor);
        k.AvBitti();
        Assert.AreEqual(GerilimKurali.AvSogumasi, k.AvSogumaKalan, 1e-3f);
        // Soğuma sürerken gerilim tavana vursa da av başlamaz, kademe 2'de kalır.
        k.Gurultu(1f);
        Oynat(k, 200f, true, true, false, true);
        Assert.IsFalse(k.AvSuruyor);
        Assert.AreEqual(2, k.Kademe);
        // Kat değişse de soğuma silinmez.
        k.KatAyarla(6);
        Assert.Greater(k.AvSogumaKalan, 0f);
        // Soğuma bitince av yine gelir.
        Oynat(k, 80f, false, true, false, false);          // sahanlıkta bekle: soğuma 270'i aşar
        Assert.LessOrEqual(k.AvSogumaKalan, 0f);
        k.Gurultu(1f);
        Oynat(k, 1f, true, true, false, true);
        Assert.IsTrue(k.AvSuruyor);
    }

    [Test]
    public void Kademede_kalinca_25_40_saniyede_bir_tekrar_tepkisi()
    {
        var k = new GerilimKurali(3);
        k.KatAyarla(4);
        k.Gurultu(0.45f);
        k.Ilerlet(0.1f, true, true, false, false);     // 1. kademeye giriş
        // Sonra 1. kademede tutalım: lamba kapalı ama yürüyor → saniyede 0,004 soğur; her 10 sn'de
        // 0,05 gürültü ile beslenince 120 sn boyunca 0,45-0,60 arasında kalır.
        int tekrar = 0;
        float ilk = -1f, son = -1f;
        for (int i = 0; i < 1200; i++)                 // 120 sn
        {
            if (i % 100 == 0) k.Gurultu(0.05f);
            GerilimTepkisi t = k.Ilerlet(0.1f, true, false, false, true);
            if (k.Kademe != 1) break;
            if (t == GerilimTepkisi.Titreme || t == GerilimTepkisi.UstKatAdimlari || t == GerilimTepkisi.GolgeIzi)
            {
                tekrar++;
                if (ilk < 0f) ilk = i * 0.1f; else if (son < 0f) son = i * 0.1f;
            }
        }
        Assert.GreaterOrEqual(tekrar, 2, "iki dakikada en az iki tekrar");
        Assert.That(son - ilk, Is.InRange(GerilimKurali.TepkiAraEnAz - 0.2f, GerilimKurali.TepkiAraEnCok + 0.2f));
    }

    [Test]
    public void Birinci_katta_av_yok_en_cok_ikinci_kademe()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(1);
        Oynat(k, 600f, true, true, false, true);
        Assert.AreEqual(1f, k.Deger, 1e-4f);
        Assert.AreEqual(2, k.Kademe);
        Assert.IsFalse(k.AvSuruyor);
        Assert.IsFalse(GerilimKurali.KattaAvVar(1));
        Assert.IsTrue(GerilimKurali.KattaAvVar(2));
    }

    [Test]
    public void Kat_carpani_yukari_dogru_artar()
    {
        Assert.Less(GerilimKurali.KatinCarpani(1), GerilimKurali.KatinCarpani(2));
        Assert.Less(GerilimKurali.KatinCarpani(4), GerilimKurali.KatinCarpani(5));
        Assert.AreEqual(1.0f, GerilimKurali.KatinCarpani(4));
        // #112: 8. kat 1,3 iken 9 kilitli katta 5 av çıkıyordu; artık 1,0 ve 5. kattan hızlı değil.
        Assert.AreEqual(1.0f, GerilimKurali.KatinCarpani(8));
        Assert.LessOrEqual(GerilimKurali.KatinCarpani(8), GerilimKurali.KatinCarpani(5));
    }

    [Test]
    public void Yeni_kat_gerilimi_sifirlar()
    {
        var k = new GerilimKurali(1);
        k.KatAyarla(3);
        k.Gurultu(0.9f);
        k.KatAyarla(4);
        Assert.AreEqual(0f, k.Deger);
        Assert.AreEqual(0, k.Kademe);
        Assert.AreEqual(1.0f, k.KatCarpani);
    }

    [Test]
    public void Duvardaki_golge_iki_kademede_de_tekrar_tepkisi_olarak_gelir()
    {
        // Uzun süre 1. ve 2. kademede kalan oyuncu duvardaki gölgeyi (#93) en az bir kez görmeli.
        foreach (float seviye in new[] { 0.45f, 0.70f })
        {
            var k = new GerilimKurali(4);
            k.KatAyarla(4);
            k.Gurultu(seviye);
            int iz = 0;
            for (int i = 0; i < 6000; i++)                  // 600 sn
            {
                if (i % 100 == 0) k.Gurultu(0.04f);        // soğumasın, aynı kademede kalsın
                if (k.Ilerlet(0.1f, true, false, false, true) == GerilimTepkisi.GolgeIzi) iz++;
                if (k.AvSuruyor) break;
            }
            Assert.Greater(iz, 0, $"gerilim {seviye}: duvardaki gölge hiç gelmedi");
        }
    }

    [Test]
    public void Aktarilan_soguma_avi_erteler()
    {
        // #119: sahne yeniden yüklenince yeni kurala eski soğuma aktarılır; süresi dolana kadar av yok.
        var k = new GerilimKurali(3); k.KatAyarla(5);
        k.AvSogumasiAyarla(100f);
        Assert.AreEqual(100f, k.AvSogumaKalan, 0.001f);
        for (int i = 0; i < 900; i++) k.Ilerlet(0.1f, true, true, false, true);   // 90 sn, gerilim tavana vurur
        Assert.IsFalse(k.AvSuruyor, "soğuma bitmeden av başladı");
        for (int i = 0; i < 1500; i++) if (k.Ilerlet(0.1f, true, true, false, true) == GerilimTepkisi.Av) break;
        Assert.IsTrue(k.AvSuruyor, "soğuma bitince av gelmedi");
        k.AvSogumasiAyarla(-5f);
        Assert.AreEqual(0f, k.AvSogumaKalan, 0.001f, "eksi değer 0 sayılır");
    }

    [Test]
    public void Ayni_tohum_ayni_tepkileri_verir()
    {
        var a = new GerilimKurali(9); a.KatAyarla(5);
        var b = new GerilimKurali(9); b.KatAyarla(5);
        for (int i = 0; i < 3000; i++)
            Assert.AreEqual(a.Ilerlet(0.1f, true, true, false, true), b.Ilerlet(0.1f, true, true, false, true));
    }
}
