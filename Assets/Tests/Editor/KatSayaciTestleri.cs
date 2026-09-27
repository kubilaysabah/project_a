// KatSayaciTestleri.cs
// KatSayaci sınıfının otomatik testleri.
//
// OTOMATİK TEST NEDİR
// Kodun doğru çalıştığını iddia eden, kendisi de koddan ibaret küçük denemelerdir.
// Elle test etmek için Unity'yi açıp Play'e basıp sekiz kat çıkmak gerekir; bu dosya aynı
// işi saniyenin altında yapar ve hiçbir adımı atlamaz.
//
// #59: eski "anomali / doğru-yanlış karar / ceza" testleri kural kalkınca silindi.

using NUnit.Framework;   // Test, Assert gibi test araçları bu kütüphaneden gelir

[TestFixture]
public class KatSayaciTestleri
{
    // #52: kayıttan yükleme. Sınır dışı değerler sınıra çekilir; bozuk kayıt oyunu bozmaz.
    [Test]
    public void Yukle_kati_ayarlar_ve_sinirlari_asmaz()
    {
        var s = new KatSayaci();
        s.Yukle(5);
        Assert.AreEqual(5, s.MevcutKat);
        s.Yukle(99);
        Assert.AreEqual(KatSayaci.CatiKati, s.MevcutKat);
        s.Yukle(-3);
        Assert.AreEqual(KatSayaci.BaslangicKati, s.MevcutKat);
    }

    [Test]
    public void Baslangicta_birinci_katta_olunur()
    {
        Assert.AreEqual(1, new KatSayaci().MevcutKat);
    }

    [Test]
    public void Ilerle_bir_ust_kata_gecirir()
    {
        var s = new KatSayaci();
        Assert.IsTrue(s.Ilerle());
        Assert.AreEqual(2, s.MevcutKat);
    }

    // 8. katın üstü kat değil, çatıdır: sayaç 8'de durur.
    [Test]
    public void Sekizinci_katta_durur()
    {
        var s = new KatSayaci();
        for (int kat = 1; kat < KatSayaci.CatiKati; kat++) Assert.IsTrue(s.Ilerle(), $"{kat}. kattan ilerlenmeliydi");
        Assert.AreEqual(KatSayaci.CatiKati, s.MevcutKat);
        Assert.IsFalse(s.Ilerle(), "8. kattan yukarısı yok");
        Assert.AreEqual(KatSayaci.CatiKati, s.MevcutKat);
    }
}
