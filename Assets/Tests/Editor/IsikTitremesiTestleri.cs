// IsikTitremesiTestleri.cs
// Kat imzası ışıklarının (#116) titreme kuralı: her tür kendi aralığında kalır, aynı an aynı sonucu verir,
// mum gaz lambasından daha çok oynar, oyuncak iki renk arasında gidip gelir, bozuk zaman ışığı söndürmez.

using NUnit.Framework;

public class IsikTitremesiTestleri
{
    [Test]
    public void Her_tur_kendi_araliginda_kalir()
    {
        for (float t = 0f; t < 60f; t += 0.013f)
        {
            Assert.AreEqual(1f, IsikTitremesi.Carpan(KatIsikTitremesi.Sabit, t), "sabit");
            Assert.That(IsikTitremesi.Carpan(KatIsikTitremesi.Mum, t, 2), Is.InRange(0.55f, 1.15f), $"mum t={t}");
            Assert.That(IsikTitremesi.Carpan(KatIsikTitremesi.Alev, t, 2), Is.InRange(0.80f, 1.08f), $"alev t={t}");
            Assert.That(IsikTitremesi.Carpan(KatIsikTitremesi.Yanip, t, 2), Is.InRange(0.15f, 1f), $"yanıp t={t}");
        }
    }

    [Test]
    public void Ayni_an_ayni_sonuc()
    {
        for (float t = 0f; t < 20f; t += 0.37f)
            Assert.AreEqual(IsikTitremesi.Carpan(KatIsikTitremesi.Mum, t, 5), IsikTitremesi.Carpan(KatIsikTitremesi.Mum, t, 5));
    }

    [Test]
    public void Mum_gaz_lambasindan_cok_oynar()
    {
        float mumMin = 9f, mumMax = 0f, alevMin = 9f, alevMax = 0f;
        for (float t = 0f; t < 30f; t += 0.01f)
        {
            float m = IsikTitremesi.Carpan(KatIsikTitremesi.Mum, t), a = IsikTitremesi.Carpan(KatIsikTitremesi.Alev, t);
            if (m < mumMin) mumMin = m; if (m > mumMax) mumMax = m;
            if (a < alevMin) alevMin = a; if (a > alevMax) alevMax = a;
        }
        Assert.Greater(mumMax - mumMin, alevMax - alevMin, "mum daha geniş aralıkta oynamalı");
        Assert.Greater(mumMax - mumMin, 0.3f, "mum gözle görülür titremeli");
    }

    [Test]
    public void Oyuncak_iki_renk_arasinda_gidip_gelir()
    {
        int degisim = 0;
        bool onceki = IsikTitremesi.IkinciRenk(KatIsikTitremesi.Yanip, 0f);
        for (float t = 0f; t < 10f; t += 0.01f)
        {
            bool simdi = IsikTitremesi.IkinciRenk(KatIsikTitremesi.Yanip, t);
            if (simdi != onceki) degisim++;
            onceki = simdi;
        }
        // 10 sn / 0,45 sn ≈ 22 renk değişimi.
        Assert.That(degisim, Is.InRange(18, 26), $"değişim {degisim}");
        Assert.IsFalse(IsikTitremesi.IkinciRenk(KatIsikTitremesi.Mum, 1.3f), "yalnızca yanıp sönen ışık renk değiştirir");
    }

    [Test]
    public void Bozuk_zaman_isigi_sondurmez()
    {
        Assert.AreEqual(1f, IsikTitremesi.Carpan(KatIsikTitremesi.Mum, float.NaN));
        Assert.AreEqual(1f, IsikTitremesi.Carpan(KatIsikTitremesi.Yanip, float.PositiveInfinity));
        Assert.IsFalse(IsikTitremesi.IkinciRenk(KatIsikTitremesi.Yanip, float.NaN));
    }
}
