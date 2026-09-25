// MenuGezginiTestleri.cs
// Menüde klavye/kolla gezinme kurallarının testleri (#29).

using NUnit.Framework;

[TestFixture]
public class MenuGezginiTestleri
{
    [Test]
    public void Asagi_ve_yukari_hareket_eder()
    {
        var g = new MenuGezgini(4);
        g.Tasi(+1);
        g.Tasi(+1);
        Assert.AreEqual(2, g.Secili);
        g.Tasi(-1);
        Assert.AreEqual(1, g.Secili);
    }

    [Test]
    public void En_alttan_asagi_en_uste_doner()
    {
        var g = new MenuGezgini(4);
        g.Sec(3);
        g.Tasi(+1);
        Assert.AreEqual(0, g.Secili);
    }

    [Test]
    public void En_ustten_yukari_en_alta_doner()
    {
        var g = new MenuGezgini(4);
        g.Tasi(-1);
        Assert.AreEqual(3, g.Secili, "Eksi sayının kalanı yüzünden -1'e düşmemeli.");
    }

    [Test]
    public void Oge_sayisi_azalinca_secim_sinirda_kalir()
    {
        var g = new MenuGezgini(6);
        g.Sec(5);
        g.OgeSayisiniAyarla(4);
        Assert.AreEqual(3, g.Secili);
    }

    [Test]
    public void Sec_sinirlarin_disina_cikamaz()
    {
        var g = new MenuGezgini(4);
        g.Sec(10);
        Assert.AreEqual(3, g.Secili);
        g.Sec(-2);
        Assert.AreEqual(0, g.Secili);
    }

    [Test]
    public void Bos_menu_en_az_bir_ogeli_sayilir()
    {
        var g = new MenuGezgini(0);
        g.Tasi(+1);   // sıfıra bölme hatası vermemeli
        Assert.AreEqual(0, g.Secili);
    }

    // ---------- BASILI TUTMA ----------

    [Test]
    public void Ilk_basista_hemen_ilerler()
    {
        var z = new TekrarZamanlayici();
        Assert.IsTrue(z.Guncelle(true, 10f));
    }

    [Test]
    public void Basili_tutulunca_ilk_beklemeden_once_tekrar_etmez()
    {
        var z = new TekrarZamanlayici();
        z.Guncelle(true, 0f);
        Assert.IsFalse(z.Guncelle(true, TekrarZamanlayici.IlkBekleme - 0.01f));
        Assert.IsTrue(z.Guncelle(true, TekrarZamanlayici.IlkBekleme));
    }

    [Test]
    public void Tekrar_duzenli_araliklarla_surer()
    {
        var z = new TekrarZamanlayici();
        int adim = 0;
        // 60 FPS'te 0,95 saniye basılı tut. Süre bilerek bir tekrar anının tam üstünde
        // değil: kayan noktalı sayılarda 24/60 ile 0,4 birebir eşit çıkmayabilir.
        for (int i = 0; i <= 57; i++) if (z.Guncelle(true, i / 60f)) adim++;

        // 1 (ilk basış) + 0.40, 0.52, 0.64, 0.76, 0.88 → 6
        Assert.AreEqual(6, adim);
    }

    [Test]
    public void Birakip_yeniden_basmak_ilk_basis_sayilir()
    {
        var z = new TekrarZamanlayici();
        z.Guncelle(true, 0f);
        z.Guncelle(false, 0.05f);
        Assert.IsTrue(z.Guncelle(true, 0.1f), "Bırakıp basınca beklemeden ilerlemeli.");
    }

    [Test]
    public void Uzun_takilmadan_sonra_tekrarlar_birikmez()
    {
        var z = new TekrarZamanlayici();
        z.Guncelle(true, 0f);
        z.Guncelle(true, 0.4f);                 // tekrar başladı
        Assert.IsTrue(z.Guncelle(true, 5f));    // oyun 4,6 sn takıldı: bir kez ilerle
        Assert.IsFalse(z.Guncelle(true, 5.01f), "Takılmada kaçan tekrarlar birden gelmemeli.");
    }
}
