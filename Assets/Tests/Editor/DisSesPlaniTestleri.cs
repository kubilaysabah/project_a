// DisSesPlaniTestleri.cs
// Dış seslerin zamanlama kuralının testleri (#37).
//
// Korunan sözler:
//   1. İlk dış ses 20-40 sn, sonrakiler 40-90 sn arayla gelir.
//   2. Art arda iki kez aynı tür gelmez.
//   3. Sesi olmayan tür (ör. kaydı henüz eklenmemiş köpek) hiç seçilmez.
//   4. Aynı tohum aynı sırayı verir.

using NUnit.Framework;

[TestFixture]
public class DisSesPlaniTestleri
{
    private static readonly bool[] Hepsi = { true, true, true };

    [Test]
    public void Ilk_bekleme_kisa_sonrakiler_uzun()
    {
        for (int tohum = 0; tohum < 100; tohum++)
        {
            var plan = new DisSesPlani(3, tohum);
            float ilk = plan.SonrakiBekleme();
            Assert.GreaterOrEqual(ilk, DisSesPlani.IlkEnAz);
            Assert.LessOrEqual(ilk, DisSesPlani.IlkEnCok);
            for (int i = 0; i < 20; i++)
            {
                float b = plan.SonrakiBekleme();
                Assert.GreaterOrEqual(b, DisSesPlani.AraEnAz);
                Assert.LessOrEqual(b, DisSesPlani.AraEnCok);
            }
        }
    }

    [Test]
    public void Ayni_tur_art_arda_gelmez()
    {
        for (int tohum = 0; tohum < 100; tohum++)
        {
            var plan = new DisSesPlani(3, tohum);
            int onceki = plan.SonrakiTur(Hepsi);
            for (int i = 0; i < 50; i++)
            {
                int tur = plan.SonrakiTur(Hepsi);
                Assert.AreNotEqual(onceki, tur, $"tohum {tohum}");
                onceki = tur;
            }
        }
    }

    [Test]
    public void Sesi_olmayan_tur_secilmez()
    {
        var plan = new DisSesPlani(3, 7);
        var koepeksiz = new[] { false, true, true };
        for (int i = 0; i < 100; i++) Assert.AreNotEqual(0, plan.SonrakiTur(koepeksiz));
    }

    [Test]
    public void Tek_tur_varsa_o_tekrar_eder_hic_yoksa_eksi_bir()
    {
        var plan = new DisSesPlani(3, 1);
        var tek = new[] { false, false, true };
        for (int i = 0; i < 5; i++) Assert.AreEqual(2, plan.SonrakiTur(tek));
        Assert.AreEqual(-1, plan.SonrakiTur(new[] { false, false, false }));
    }

    [Test]
    public void Ayni_tohum_ayni_sirayi_verir()
    {
        var a = new DisSesPlani(3, 42);
        var b = new DisSesPlani(3, 42);
        for (int i = 0; i < 20; i++)
        {
            Assert.AreEqual(a.SonrakiBekleme(), b.SonrakiBekleme());
            Assert.AreEqual(a.SonrakiTur(Hepsi), b.SonrakiTur(Hepsi));
        }
    }

    [Test]
    public void Cesit_secimi_sinirlar_icinde()
    {
        var plan = new DisSesPlani(3, 3);
        Assert.AreEqual(0, plan.CesitSec(1));
        Assert.AreEqual(0, plan.CesitSec(0));
        for (int i = 0; i < 100; i++)
        {
            int c = plan.CesitSec(3);
            Assert.GreaterOrEqual(c, 0);
            Assert.Less(c, 3);
        }
    }
}
