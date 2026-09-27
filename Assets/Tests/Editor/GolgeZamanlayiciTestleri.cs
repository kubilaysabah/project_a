// GolgeZamanlayiciTestleri.cs
// Gezen Gölge zamanlayıcısının testleri (#71): 1-2 dakikada bir, yalnızca oyuncu katta
// sıkışıkken, yeni katta baştan.

using NUnit.Framework;

[TestFixture]
public class GolgeZamanlayiciTestleri
{
    [Test]
    public void Ilk_sure_bir_ile_iki_dakika_arasinda()
    {
        for (int tohum = 0; tohum < 50; tohum++)
        {
            var z = new GolgeZamanlayici(tohum);
            Assert.That(z.Kalan, Is.InRange(GolgeZamanlayici.EnAz, GolgeZamanlayici.EnCok));
        }
    }

    [Test]
    public void Sure_dolunca_bir_kez_belirir_ve_yeni_sure_cekilir()
    {
        var z = new GolgeZamanlayici(7);
        z.Ilerlet(0f, 3, true);                  // 3. kata varış: saat kurulur
        int belirme = 0;
        // 121 saniye, saniyede 10 kare: en uzun süre bile dolmuş olmalı, tam bir kez.
        for (int i = 0; i < 1210; i++) if (z.Ilerlet(0.1f, 3, true)) belirme++;
        Assert.That(belirme, Is.InRange(1, 2), "121 sn'de en az bir, en çok iki beliriş (en kısa süre 60)");
        Assert.That(z.Kalan, Is.LessThanOrEqualTo(GolgeZamanlayici.EnCok));
    }

    [Test]
    public void Etkin_degilken_saat_durur()
    {
        var z = new GolgeZamanlayici(3);
        z.Ilerlet(0f, 2, true);
        float once = z.Kalan;
        for (int i = 0; i < 2000; i++) Assert.IsFalse(z.Ilerlet(0.1f, 2, false));   // 200 sn menüde / notta
        Assert.AreEqual(once, z.Kalan, 1e-4f, "saat kaldığı yerde bekler");
    }

    [Test]
    public void Yeni_katta_saat_bastan_kurulur()
    {
        var z = new GolgeZamanlayici(11);
        z.Ilerlet(0f, 4, true);
        for (int i = 0; i < 500; i++) z.Ilerlet(0.1f, 4, true);   // 50 sn geçti (en kısa süre 60)
        Assert.That(z.Kalan, Is.LessThan(GolgeZamanlayici.EnCok - 49f));
        z.Ilerlet(0f, 5, true);                                    // 5. kata çıkıldı
        Assert.That(z.Kalan, Is.InRange(GolgeZamanlayici.EnAz, GolgeZamanlayici.EnCok), "yeni kat: tam bir süre");
    }

    [Test]
    public void Ertelenince_kisa_sure_sonra_yeniden_dener()
    {
        var z = new GolgeZamanlayici(5);
        z.Ilerlet(0f, 1, true);
        z.Ertele(8f);
        Assert.AreEqual(8f, z.Kalan, 1e-4f);
        bool belirdi = false;
        for (int i = 0; i < 81; i++) belirdi |= z.Ilerlet(0.1f, 1, true);
        Assert.IsTrue(belirdi, "8 sn sonra yeniden denenir");
    }

    [Test]
    public void Ayni_tohum_ayni_sureleri_verir()
    {
        var a = new GolgeZamanlayici(42);
        var b = new GolgeZamanlayici(42);
        Assert.AreEqual(a.Kalan, b.Kalan);
    }
}
