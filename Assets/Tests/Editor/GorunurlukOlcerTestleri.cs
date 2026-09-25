// GorunurlukOlcerTestleri.cs
// Anomali görünürlüğünü ölçen kuralın testleri (#33).

using NUnit.Framework;

[TestFixture]
public class GorunurlukOlcerTestleri
{
    [Test]
    public void Ayni_goruntuler_arasinda_fark_yok()
    {
        float[] a = { 0.1f, 0.5f, 0.9f, 0.3f, 0.3f, 0.3f };   // iki piksel
        float[] harita = GorunurlukOlcer.FarkHaritasi(a, (float[])a.Clone());
        Assert.AreEqual(2, harita.Length);
        Assert.AreEqual(0f, GorunurlukOlcer.Alan(harita, GorunurlukOlcer.VarsayilanEsik));
        Assert.AreEqual(0f, GorunurlukOlcer.Guc(harita, GorunurlukOlcer.VarsayilanEsik));
    }

    [Test]
    public void Renk_degisimi_parlaklik_ayni_olsa_da_yakalanir()
    {
        // Sarımsı ışıktan yeşilimsi ışığa: parlaklık yaklaşık aynı, renk belirgin farklı.
        float[] sari = { 0.60f, 0.55f, 0.30f };
        float[] yesil = { 0.35f, 0.62f, 0.35f };
        float[] harita = GorunurlukOlcer.FarkHaritasi(sari, yesil);
        Assert.AreEqual(0.25f, harita[0], 1e-5f, "En büyük kanal farkı (kırmızı) alınmalı.");
    }

    [Test]
    public void Esigin_altindaki_fark_sayilmaz_ustundeki_sayilir()
    {
        float[] harita = { 0.01f, 0.10f, 0.0f, 0.30f };
        Assert.AreEqual(0.5f, GorunurlukOlcer.Alan(harita, 0.03f), 1e-5f);
        Assert.AreEqual(0.2f, GorunurlukOlcer.Guc(harita, 0.03f), 1e-5f);   // (0,10 + 0,30) / 2
    }

    [Test]
    public void Uyumsuz_ya_da_bos_dizi_bos_harita_verir()
    {
        Assert.AreEqual(0, GorunurlukOlcer.FarkHaritasi(new float[3], new float[6]).Length);
        Assert.AreEqual(0, GorunurlukOlcer.FarkHaritasi(new float[4], new float[4]).Length, "RGB değil (3'ün katı değil).");
        Assert.AreEqual(0, GorunurlukOlcer.FarkHaritasi(null, new float[3]).Length);
        Assert.AreEqual(0f, GorunurlukOlcer.Alan(new float[0], 0.03f));
        Assert.AreEqual(0f, GorunurlukOlcer.Guc(null, 0.03f));
    }

    [Test]
    public void Kucuk_ama_net_degisiklik_gorunmez_sayilmaz()
    {
        // Ters çevrilmiş kapı numarası: ekranın binde biri, ama siyah rakam açık plakada.
        Assert.AreEqual(GorunurlukOlcer.KucukNet, GorunurlukOlcer.Sinif(0.001f, 0.5f));
    }

    [Test]
    public void Genis_ama_hafif_degisiklik_silik_sayilir()
    {
        Assert.AreEqual(GorunurlukOlcer.Silik, GorunurlukOlcer.Sinif(0.2f, 0.05f));
    }

    [Test]
    public void Sinif_sinirlari()
    {
        Assert.AreEqual(GorunurlukOlcer.Gorunmez, GorunurlukOlcer.Sinif(0f, 1f));
        Assert.AreEqual(GorunurlukOlcer.Gorunmez, GorunurlukOlcer.Sinif(GorunurlukOlcer.GorunmezSiniri - 0.0001f, 1f));
        Assert.AreEqual(GorunurlukOlcer.KucukNet, GorunurlukOlcer.Sinif(GorunurlukOlcer.GorunmezSiniri, 1f));
        Assert.AreEqual(GorunurlukOlcer.Belirgin, GorunurlukOlcer.Sinif(GorunurlukOlcer.KucukSiniri, GorunurlukOlcer.SilikSiniri));
        Assert.AreEqual(GorunurlukOlcer.Belirgin, GorunurlukOlcer.Sinif(0.5f, 0.5f));
    }
}
