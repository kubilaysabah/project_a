// KatIlerlemeTakipcisiTestleri.cs
// Kararın gerçekten kat edilen dikey mesafeye bağlı olduğunu kanıtlayan testler.
// Buradaki asıl amaç, 1. kattan geri dönenin 3. kata atlaması hatasının
// bir daha geri gelmemesini garanti altına almak.

using NUnit.Framework;

[TestFixture]
public class KatIlerlemeTakipcisiTestleri
{
    const float KatYuksekligi = 3f;

    private KatIlerlemeTakipcisi takipci;

    [SetUp]
    public void Hazirla()
    {
        // Oyuncu 0 kotunda başlıyor.
        takipci = new KatIlerlemeTakipcisi(KatYuksekligi, 0f);
    }

    [Test]
    public void Bir_kat_cikinca_yukari_karari_olusur()
    {
        Assert.IsTrue(takipci.KararOlustuMu(3f, out Karar karar));
        Assert.AreEqual(Karar.Yukari, karar);
    }

    [Test]
    public void Bir_kat_inince_geri_karari_olusur()
    {
        Assert.IsTrue(takipci.KararOlustuMu(-3f, out Karar karar));
        Assert.AreEqual(Karar.Geri, karar);
    }

    // Asıl hata buydu: yarım metre inince karar oluşuyordu.
    [Test]
    public void Yarim_metre_inmek_karar_olusturmaz()
    {
        Assert.IsFalse(takipci.KararOlustuMu(-0.5f, out _));
    }

    [Test]
    public void Yarim_metre_cikmak_karar_olusturmaz()
    {
        Assert.IsFalse(takipci.KararOlustuMu(0.5f, out _));
    }

    [Test]
    public void Neredeyse_bir_kat_yetmez()
    {
        Assert.IsFalse(takipci.KararOlustuMu(2.9f, out _));
        Assert.IsFalse(takipci.KararOlustuMu(-2.9f, out _));
    }

    // Aşağı ve yukarı yönler AYNI mesafeyi istemeli.
    // Hata tam olarak bu simetrinin bozulmasından çıkmıştı.
    [Test]
    public void Yukari_ve_asagi_esit_mesafe_ister()
    {
        var yukari = new KatIlerlemeTakipcisi(KatYuksekligi, 0f);
        var asagi  = new KatIlerlemeTakipcisi(KatYuksekligi, 0f);

        Assert.IsFalse(yukari.KararOlustuMu(2.9f, out _), "2.9 m çıkmak yetmemeli");
        Assert.IsFalse(asagi.KararOlustuMu(-2.9f, out _), "2.9 m inmek de yetmemeli");

        Assert.IsTrue(yukari.KararOlustuMu(3.0f, out _), "3.0 m çıkmak yetmeli");
        Assert.IsTrue(asagi.KararOlustuMu(-3.0f, out _), "3.0 m inmek de yetmeli");
    }

    [Test]
    public void Karardan_sonra_yeni_karar_icin_bir_kat_daha_gerekir()
    {
        Assert.IsTrue(takipci.KararOlustuMu(3f, out _));

        // Aynı kotta ikinci bir karar oluşmamalı.
        Assert.IsFalse(takipci.KararOlustuMu(3f, out _));

        // Yarım kat daha çıkmak da yetmez.
        Assert.IsFalse(takipci.KararOlustuMu(4.5f, out _));

        // Tam bir kat daha çıkınca oluşur.
        Assert.IsTrue(takipci.KararOlustuMu(6f, out Karar karar));
        Assert.AreEqual(Karar.Yukari, karar);
    }

    [Test]
    public void Cikip_geri_inmek_iki_ayri_karar_uretir()
    {
        Assert.IsTrue(takipci.KararOlustuMu(3f, out Karar birinci));
        Assert.AreEqual(Karar.Yukari, birinci);

        Assert.IsTrue(takipci.KararOlustuMu(0f, out Karar ikinci));
        Assert.AreEqual(Karar.Geri, ikinci);

        // Başlangıç kotuna dönüldü; yeni karar için yine bir tam kat gerekir.
        Assert.IsFalse(takipci.KararOlustuMu(1.5f, out _));
    }

    [Test]
    public void Uc_kat_ciksa_bile_her_seferinde_tek_karar_verilir()
    {
        // Oyuncu durmadan 9 m çıkarsa üç ayrı karar oluşmalı, hepsi tek tek.
        int sayac = 0;
        float kot = 0f;

        // 0.1 m'lik adımlarla 9 m çık.
        for (int i = 0; i < 90; i++)
        {
            kot += 0.1f;
            if (takipci.KararOlustuMu(kot, out _)) sayac++;
        }

        Assert.AreEqual(3, sayac, "9 m çıkışta tam üç karar beklenir");
    }

    [Test]
    public void Sifirla_sayaci_verilen_kota_alir()
    {
        takipci.KararOlustuMu(3f, out _);
        takipci.Sifirla(100f);

        Assert.IsFalse(takipci.KararOlustuMu(100f, out _));
        Assert.IsTrue(takipci.KararOlustuMu(103f, out _));
    }

    [Test]
    public void Mesafe_olcumu_mutlak_deger_verir()
    {
        Assert.AreEqual(1.5f, takipci.SonKarardanBeriMesafe(1.5f), 0.0001f);
        Assert.AreEqual(1.5f, takipci.SonKarardanBeriMesafe(-1.5f), 0.0001f);
    }
}
