// ArizaliLambaKuraliTestleri.cs
// Arızalı lamba deseninin (#111) kuralları: aralıkta kalır, aynı an aynı sonucu verir, hem yanar hem söner,
// karanlık anları gerçekten "karanlık" (en az birkaç adım), bozuk zaman lambayı söndürür.

using NUnit.Framework;

public class ArizaliLambaKuraliTestleri
{
    [Test]
    public void Parlaklik_her_zaman_0_ile_1_arasinda()
    {
        for (float t = 0f; t < 120f; t += 0.013f)
        {
            float p = ArizaliLambaKurali.Parlaklik(t, 3);
            Assert.That(p, Is.InRange(0f, 1f), $"t={t}");
        }
    }

    [Test]
    public void Ayni_an_ayni_parlaklik()
    {
        for (float t = 0f; t < 30f; t += 0.37f)
            Assert.AreEqual(ArizaliLambaKurali.Parlaklik(t, 7), ArizaliLambaKurali.Parlaklik(t, 7), $"t={t}");
    }

    [Test]
    public void Bir_dakikada_hem_yanar_hem_soner()
    {
        int yanik = 0, toplam = 0, gecis = 0;
        bool onceki = ArizaliLambaKurali.Parlaklik(0f) > 0.5f;
        for (float t = 0f; t < 60f; t += 0.01f)
        {
            bool simdi = ArizaliLambaKurali.Parlaklik(t) > 0.5f;
            if (simdi) yanik++;
            if (simdi != onceki) gecis++;
            onceki = simdi;
            toplam++;
        }
        float oran = yanik / (float)toplam;
        // Arızalı ama yol gösteren bir lamba: zamanın üçte birinden fazlası yanık, hep yanık da değil.
        Assert.That(oran, Is.InRange(0.35f, 0.85f), $"yanık oranı {oran:0.00}");
        // Yanıp sönme göze görünür sıklıkta olmalı (dakikada en az 30 geçiş).
        Assert.That(gecis, Is.GreaterThan(30), $"geçiş {gecis}");
    }

    [Test]
    public void Iki_dakikada_en_az_bir_uzun_karanlik_var()
    {
        // Sönük blok gerçekten karanlık bir an yaratmalı: art arda en az 0,4 sn sönük.
        float enUzun = 0f, simdiki = 0f;
        for (float t = 0f; t < 120f; t += 0.01f)
        {
            if (ArizaliLambaKurali.Parlaklik(t) < 0.5f) { simdiki += 0.01f; if (simdiki > enUzun) enUzun = simdiki; }
            else simdiki = 0f;
        }
        Assert.That(enUzun, Is.GreaterThanOrEqualTo(0.4f), $"en uzun karanlık {enUzun:0.00} sn");
    }

    [Test]
    public void Farkli_tohum_farkli_desen()
    {
        int farkli = 0;
        for (float t = 0f; t < 20f; t += 0.05f)
            if (ArizaliLambaKurali.Parlaklik(t, 1) != ArizaliLambaKurali.Parlaklik(t, 2)) farkli++;
        Assert.That(farkli, Is.GreaterThan(20), "iki lamba aynı anda aynı şeyi yapmamalı");
    }

    [Test]
    public void Bozuk_zaman_lambayi_sondurur()
    {
        Assert.AreEqual(0f, ArizaliLambaKurali.Parlaklik(float.NaN));
        Assert.AreEqual(0f, ArizaliLambaKurali.Parlaklik(float.PositiveInfinity));
    }
}
