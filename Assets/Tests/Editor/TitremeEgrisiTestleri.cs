// TitremeEgrisiTestleri.cs
// Normal lamba titremesinin kuralının testleri (#38).
//
// Korunan sözler:
//   1. Parlaklık hep EnAz ile 1 arasında: normal bir lamba hiçbir zaman sönmez
//      (sönen lamba "titreyen_ampul" anomalisidir).
//   2. Aynı an hep aynı değeri verir: bütün katlar aynı anda aynı titrer.
//   3. Kararmalar gerçekten oluyor ama seyrek: lamba çoğu zaman sakin yanar.

using NUnit.Framework;

[TestFixture]
public class TitremeEgrisiTestleri
{
    [Test]
    public void Parlaklik_hep_sinirlarin_icinde_ve_hic_sonmez()
    {
        for (int i = 0; i < 200000; i++)
        {
            float d = TitremeEgrisi.Deger(i * 0.005f);   // 1000 saniye, 5 ms arayla
            Assert.GreaterOrEqual(d, TitremeEgrisi.EnAz);
            Assert.LessOrEqual(d, 1f);
        }
    }

    [Test]
    public void Ayni_an_hep_ayni_degeri_verir()
    {
        for (int i = 0; i < 1000; i++)
        {
            float t = i * 0.37f;
            Assert.AreEqual(TitremeEgrisi.Deger(t), TitremeEgrisi.Deger(t));
        }
    }

    [Test]
    public void Kararmalar_olur_ama_seyrektir()
    {
        int karanlik = 0, toplam = 0;
        float enDusuk = 1f;
        for (int i = 0; i < 200000; i++)
        {
            float d = TitremeEgrisi.Deger(i * 0.005f);
            if (d < 0.9f) karanlik++;
            if (d < enDusuk) enDusuk = d;
            toplam++;
        }
        float oran = karanlik / (float)toplam;
        Assert.Less(enDusuk, 0.75f, "1000 saniyede hiç belirgin kararma olmadı.");
        Assert.Greater(oran, 0.002f, "Kararmalar neredeyse hiç yok.");
        Assert.Less(oran, 0.05f, "Lamba zamanın %5'inden fazlasında karanlık: titreme fazla sık.");
    }

    // Kıpırtı kararma değildir: sakin anlarda lamba %96'nın altına inmemeli.
    [Test]
    public void Sakin_anlarda_kipirti_cok_hafiftir()
    {
        int cokHafifOlmayan = 0;
        for (int i = 0; i < 200000; i++)
        {
            float d = TitremeEgrisi.Deger(i * 0.005f);
            if (d < 0.96f && d >= 0.9f) cokHafifOlmayan++;
        }
        // Yalnızca kararmaların iniş-çıkış kenarları bu aralığa düşebilir.
        Assert.Less(cokHafifOlmayan / 200000f, 0.02f);
    }
}
