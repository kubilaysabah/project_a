// ParlamaKuraliTestleri.cs
// Kafa lambasının ışığında görev eşyasının parlaması kuralının testleri (#83).

using NUnit.Framework;

[TestFixture]
public class ParlamaKuraliTestleri
{
    const float Koni = 32f;   // kafa lambasının dış konisinin yarısı (64° / 2)

    [Test]
    public void Koninin_disinda_parlamaz()
    {
        Assert.That(ParlamaKurali.Guc(32f, 1f, Koni, 5f), Is.EqualTo(0f));
        Assert.That(ParlamaKurali.Guc(50f, 1f, Koni, 5f), Is.EqualTo(0f));
    }

    [Test]
    public void Menzilin_disinda_parlamaz()
    {
        Assert.That(ParlamaKurali.Guc(0f, 5f, Koni, 5f), Is.EqualTo(0f));
        Assert.That(ParlamaKurali.Guc(0f, 9f, Koni, 5f), Is.EqualTo(0f));
    }

    [Test]
    public void Tam_ortada_ve_dibinde_en_guclu()
    {
        Assert.That(ParlamaKurali.Guc(0f, 0f, Koni, 5f), Is.EqualTo(1f).Within(1e-5f));
    }

    [Test]
    public void Yaklastikca_ve_ortaya_geldikce_guclenir()
    {
        // Aynı açıda: yakın olan daha güçlü.
        Assert.That(ParlamaKurali.Guc(10f, 1f, Koni, 5f), Is.GreaterThan(ParlamaKurali.Guc(10f, 3f, Koni, 5f)));
        // Aynı mesafede: ortadaki daha güçlü.
        Assert.That(ParlamaKurali.Guc(5f, 2f, Koni, 5f), Is.GreaterThan(ParlamaKurali.Guc(25f, 2f, Koni, 5f)));
    }

    [Test]
    public void Guc_hep_sifir_ile_bir_arasinda()
    {
        for (float aci = 0f; aci <= 40f; aci += 2.5f)
            for (float mesafe = 0f; mesafe <= 6f; mesafe += 0.25f)
                Assert.That(ParlamaKurali.Guc(aci, mesafe, Koni, 5f), Is.InRange(0f, 1f));
    }

    [Test]
    public void Gecersiz_olcu_parlama_vermez()
    {
        Assert.That(ParlamaKurali.Guc(0f, 1f, 0f, 5f), Is.EqualTo(0f));
        Assert.That(ParlamaKurali.Guc(0f, 1f, Koni, 0f), Is.EqualTo(0f));
        Assert.That(ParlamaKurali.Guc(0f, -1f, Koni, 5f), Is.EqualTo(0f));
        Assert.That(ParlamaKurali.Guc(-5f, 1f, Koni, 5f), Is.EqualTo(0f));
    }

    [Test]
    public void Kirpisma_alti_ile_bir_arasinda_ve_fazla_degisir()
    {
        for (float t = 0f; t < 20f; t += 0.05f)
            Assert.That(ParlamaKurali.Kirpisma(t, 1.3f), Is.InRange(0.6f, 1f));
        // Farklı fazdaki iki eşya aynı anda aynı değeri vermez.
        Assert.That(ParlamaKurali.Kirpisma(2f, 0f), Is.Not.EqualTo(ParlamaKurali.Kirpisma(2f, 2f)).Within(1e-3f));
    }
}
