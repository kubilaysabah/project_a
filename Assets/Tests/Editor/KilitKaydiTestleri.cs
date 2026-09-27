// KilitKaydiTestleri.cs
// Çözülen kilitlerin kaydının testleri (#54).

using NUnit.Framework;

[TestFixture]
public class KilitKaydiTestleri
{
    [Test]
    public void Eklenen_kilit_kata_gore_tutulur()
    {
        var k = new KilitKaydi();
        Assert.IsTrue(k.Ekle(1, "salon"));
        Assert.IsTrue(k.Iceriyor(1, "salon"));
        Assert.IsFalse(k.Iceriyor(2, "salon"), "başka katın aynı adlı kilidi çözük sayılmamalı");
        Assert.IsFalse(k.Ekle(1, "salon"), "aynı kilit iki kez eklenmez");
        Assert.AreEqual(1, k.Sayi);
    }

    [Test]
    public void Metne_yazilip_geri_okunur()
    {
        var k = new KilitKaydi();
        k.Ekle(2, "odaB"); k.Ekle(1, "salon"); k.Ekle(1, "kiler");
        string metin = k.Metin();
        Assert.AreEqual("1:kiler;1:salon;2:odaB", metin, "sıralı yazılmalı");
        var geri = KilitKaydi.Oku(metin);
        Assert.AreEqual(3, geri.Sayi);
        Assert.IsTrue(geri.Iceriyor(2, "odaB"));
    }

    [Test]
    public void Bozuk_metin_parcalari_atlanir()
    {
        var k = KilitKaydi.Oku("1:salon;;x:kiler;:yok;3:;2:odaB");
        Assert.AreEqual(2, k.Sayi);
        Assert.IsTrue(k.Iceriyor(1, "salon"));
        Assert.IsTrue(k.Iceriyor(2, "odaB"));
        Assert.AreEqual(0, KilitKaydi.Oku(null).Sayi);
    }

    [Test]
    public void Ayirici_iceren_kimlik_kabul_edilmez()
    {
        var k = new KilitKaydi();
        Assert.IsFalse(k.Ekle(1, "a;b"));
        Assert.IsFalse(k.Ekle(1, "a:b"));
        Assert.IsFalse(KilitKaydi.KimlikGecerli(""));
        Assert.IsTrue(KilitKaydi.KimlikGecerli("odaA_kapisi"));
    }

    [Test]
    public void Kat_silinince_yalniz_o_katin_kayitlari_gider()
    {
        var k = KilitKaydi.Oku("1:salon;7:banyo;7:salon;11:kiler;17:odaB");
        // 7. kat silinir: iki kaydı gider; 17. kat "7" ile bitse de, 1 ve 11 de dokunulmaz.
        Assert.AreEqual(2, k.KatiSil(7));
        Assert.IsFalse(k.Iceriyor(7, "banyo"));
        Assert.IsFalse(k.Iceriyor(7, "salon"));
        Assert.IsTrue(k.Iceriyor(17, "odaB"));
        // 1. kat silinir: 11. katın kaydı "1" ile başlasa da kalır.
        Assert.AreEqual(1, k.KatiSil(1));
        Assert.IsTrue(k.Iceriyor(11, "kiler"));
        Assert.AreEqual("11:kiler;17:odaB", k.Metin());
        // Kaydı olmayan kat: hiçbir şey silinmez.
        Assert.AreEqual(0, k.KatiSil(5));
    }
}
