// BulmaZorluguTestleri.cs
// Eşya bulma yardımlarının kat kat azalması kuralının testleri (#102).

using NUnit.Framework;

[TestFixture]
public class BulmaZorluguTestleri
{
    [Test]
    public void Ilk_iki_kat_ogretici_yardimlar_tam()
    {
        // 1. ve 2. kat bugüne kadarki değerlerle aynı kalır: parıltı tam güçte, 5 m'den; ipucu 150 / 75 sn, 4 kez.
        foreach (int kat in new[] { 1, 2 })
        {
            Assert.AreEqual(1f, BulmaZorlugu.ParlamaCarpani(kat));
            Assert.AreEqual(5f, BulmaZorlugu.ParlamaMenzili(kat));
            Assert.AreEqual(150f, BulmaZorlugu.IlkIpucu(kat));
            Assert.AreEqual(75f, BulmaZorlugu.IpucuAra(kat));
            Assert.AreEqual(4, BulmaZorlugu.EnCokIpucu(kat));
        }
    }

    [Test]
    public void Yardimlar_hicbir_katta_bir_alttaki_kattan_fazla_degil()
    {
        // "Kat kat zorlaşır": her kat bir alttakinden ya aynı ya daha az yardım eder; hiçbir değer geri dönmez.
        for (int kat = 2; kat <= 8; kat++)
        {
            Assert.LessOrEqual(BulmaZorlugu.ParlamaCarpani(kat), BulmaZorlugu.ParlamaCarpani(kat - 1), $"{kat}. kat parıltı gücü");
            Assert.LessOrEqual(BulmaZorlugu.ParlamaMenzili(kat), BulmaZorlugu.ParlamaMenzili(kat - 1), $"{kat}. kat parıltı menzili");
            Assert.GreaterOrEqual(BulmaZorlugu.IlkIpucu(kat), BulmaZorlugu.IlkIpucu(kat - 1), $"{kat}. kat ilk ipucu");
            Assert.GreaterOrEqual(BulmaZorlugu.IpucuAra(kat), BulmaZorlugu.IpucuAra(kat - 1), $"{kat}. kat ipucu arası");
            Assert.LessOrEqual(BulmaZorlugu.EnCokIpucu(kat), BulmaZorlugu.EnCokIpucu(kat - 1), $"{kat}. kat ipucu sayısı");
        }
    }

    [Test]
    public void Son_kat_birinciden_belirgin_bicimde_zor()
    {
        Assert.Less(BulmaZorlugu.ParlamaCarpani(8), 0.5f, "8. katta parıltı yarı güçten soluk");
        Assert.LessOrEqual(BulmaZorlugu.ParlamaMenzili(8), 2.5f, "8. katta parıltı ancak yaklaşınca");
        Assert.GreaterOrEqual(BulmaZorlugu.IlkIpucu(8), 240f, "8. katta ilk ipucu en az 4 dakika sonra");
        Assert.Less(BulmaZorlugu.EnCokIpucu(8), BulmaZorlugu.EnCokIpucu(1));
    }

    [Test]
    public void Parilti_hicbir_katta_tamamen_kaybolmaz()
    {
        // Karanlıkta birkaç piksellik bir anahtar parıltısız bulunamaz (#83'ün nedeni): en üst katta bile yakından görünür.
        for (int kat = 1; kat <= 8; kat++)
        {
            Assert.Greater(BulmaZorlugu.ParlamaCarpani(kat), 0f);
            Assert.Greater(BulmaZorlugu.ParlamaMenzili(kat), 1f);
        }
    }

    [Test]
    public void Aralik_disindaki_kat_en_yakin_katin_degerini_alir()
    {
        Assert.AreEqual(BulmaZorlugu.IlkIpucu(1), BulmaZorlugu.IlkIpucu(0), "zemin kat 1. kat gibi");
        Assert.AreEqual(BulmaZorlugu.IlkIpucu(1), BulmaZorlugu.IlkIpucu(-3));
        Assert.AreEqual(BulmaZorlugu.ParlamaMenzili(8), BulmaZorlugu.ParlamaMenzili(9), "çatının üstü 8. kat gibi");
        Assert.AreEqual(BulmaZorlugu.EnCokIpucu(8), BulmaZorlugu.EnCokIpucu(99));
    }
}
