// KatOlayPlaniTestleri.cs
// Her kata hangi olay sesinin düşeceğini belirleyen kuralın testleri (#35).
//
// Korunan sözler:
//   1. Bir tur içinde aynı kat hep aynı olayı ve aynı zamanları verir.
//   2. Komşu iki kata aynı olay düşmez; katlar hangi sırayla sorulursa sorulsun.
//   3. Beş olayın hepsi gerçekten çıkabiliyor.
//   4. Zamanlar sınırların içinde: iki çalış asla üst üste binmez.
//   5. Cezadan sonra (Sifirla) plan yeniden çekilir.

using NUnit.Framework;

[TestFixture]
public class KatOlayPlaniTestleri
{
    [Test]
    public void Ayni_kat_tur_boyunca_ayni_olayi_ve_zamanlari_verir()
    {
        var plan = new KatOlayPlani(tohum: 3);
        KatOlayi olay = plan.KatIcin(4);
        float[] zaman = plan.ZamanlarKatIcin(4);

        // Araya başka katlar girse de 4. kat değişmemeli.
        for (int kat = 1; kat <= 8; kat++) plan.KatIcin(kat);

        Assert.AreEqual(olay, plan.KatIcin(4));
        CollectionAssert.AreEqual(zaman, plan.ZamanlarKatIcin(4));
    }

    [Test]
    public void Ayni_tohum_ayni_plani_uretir()
    {
        var a = new KatOlayPlani(tohum: 11);
        var b = new KatOlayPlani(tohum: 11);
        for (int kat = 1; kat <= 8; kat++) Assert.AreEqual(a.KatIcin(kat), b.KatIcin(kat));
    }

    [Test]
    public void Komsu_katlara_ayni_olay_dusmez_katlar_sirayla_sorulunca()
    {
        for (int tohum = 0; tohum < 200; tohum++)
        {
            var plan = new KatOlayPlani(tohum);
            for (int kat = 1; kat < 8; kat++)
                Assert.AreNotEqual(plan.KatIcin(kat), plan.KatIcin(kat + 1), $"tohum {tohum}, kat {kat}");
        }
    }

    // Oyuncu aşağı da inebiliyor: katlar karışık sırayla sorulabilir. Örneğin önce 3,
    // sonra 5, en son ikisinin arasındaki 4. O zaman 4 iki komşusundan da farklı olmalı.
    [Test]
    public void Komsu_katlara_ayni_olay_dusmez_katlar_karisik_sorulunca()
    {
        for (int tohum = 0; tohum < 200; tohum++)
        {
            var plan = new KatOlayPlani(tohum);
            foreach (int kat in new[] { 3, 5, 4, 1, 8, 7, 2, 6 }) plan.KatIcin(kat);
            for (int kat = 1; kat < 8; kat++)
                Assert.AreNotEqual(plan.KatIcin(kat), plan.KatIcin(kat + 1), $"tohum {tohum}, kat {kat}");
        }
    }

    [Test]
    public void Bes_olayin_hepsi_cikabilir()
    {
        var gorulen = new System.Collections.Generic.HashSet<KatOlayi>();
        for (int tohum = 0; tohum < 50; tohum++)
        {
            var plan = new KatOlayPlani(tohum);
            for (int kat = 1; kat <= 8; kat++) gorulen.Add(plan.KatIcin(kat));
        }
        Assert.AreEqual(KatOlayPlani.OlaySayisi, gorulen.Count);
    }

    [Test]
    public void Zamanlar_sinirlarin_icinde_ve_ust_uste_binmez()
    {
        for (int tohum = 0; tohum < 100; tohum++)
        {
            var plan = new KatOlayPlani(tohum);
            for (int kat = 1; kat <= 8; kat++)
            {
                float[] z = plan.ZamanlarKatIcin(kat);
                Assert.AreEqual(2, z.Length);
                Assert.GreaterOrEqual(z[0], KatOlayPlani.IlkEnErken);
                Assert.LessOrEqual(z[0], KatOlayPlani.IlkEnGec);
                Assert.GreaterOrEqual(z[1] - z[0], KatOlayPlani.AraEnAz);
                Assert.LessOrEqual(z[1] - z[0], KatOlayPlani.AraEnCok);
            }
        }
    }

    [Test]
    public void Sifirladiktan_sonra_plan_yeniden_cekilir()
    {
        var plan = new KatOlayPlani(tohum: 5);
        var once = new KatOlayi[8];
        var onceZaman = new float[8];
        for (int kat = 1; kat <= 8; kat++) { once[kat - 1] = plan.KatIcin(kat); onceZaman[kat - 1] = plan.ZamanlarKatIcin(kat)[0]; }

        plan.Sifirla();

        bool farkVar = false;
        for (int kat = 1; kat <= 8; kat++)
            if (plan.KatIcin(kat) != once[kat - 1] || plan.ZamanlarKatIcin(kat)[0] != onceZaman[kat - 1]) farkVar = true;
        Assert.IsTrue(farkVar, "Sifirla sonrası plan birebir aynı kaldı.");
    }

    // Komşu kuralı en kötü durumda iki olayı yasaklar; geriye hep seçenek kalmalı.
    // Kalmasaydı liste boş olur ve oyun ilk katta çökerdi.
    [Test]
    public void Iki_komsu_da_doluyken_bile_secenek_kalir()
    {
        for (int tohum = 0; tohum < 200; tohum++)
        {
            var plan = new KatOlayPlani(tohum);
            plan.KatIcin(1);
            plan.KatIcin(3);
            Assert.DoesNotThrow(() => plan.KatIcin(2));
        }
    }
}
