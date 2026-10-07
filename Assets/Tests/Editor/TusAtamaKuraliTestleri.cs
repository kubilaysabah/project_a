// TusAtamaKuraliTestleri.cs
// Tuş atama kuralının testleri (#125): varsayılanlar, çatışma, yasak tuşlar, kayıt metni, bozuk metin.

using NUnit.Framework;

public class TusAtamaKuraliTestleri
{
    [Test]
    public void Varsayilanlar_bugunku_tuslar()
    {
        var k = new TusAtamaKurali();
        Assert.AreEqual("E", k.Klavye(TusEylemi.Etkilesim));
        Assert.AreEqual("R", k.Klavye(TusEylemi.Incele));
        Assert.AreEqual("F", k.Klavye(TusEylemi.Fener));
        Assert.AreEqual("T", k.Klavye(TusEylemi.GorevListesi));
        Assert.AreEqual("LeftCtrl", k.Klavye(TusEylemi.Egil));
        Assert.AreEqual("LeftShift", k.Klavye(TusEylemi.Kos));
        Assert.AreEqual("LeftStick", k.Kol(TusEylemi.Kos));
        Assert.AreEqual("South", k.Kol(TusEylemi.Etkilesim));
        Assert.AreEqual("LeftShoulder", k.Kol(TusEylemi.Fener));
        Assert.IsTrue(k.VarsayilanMi);
    }

    [Test]
    public void Atama_calisir_ve_catisma_reddedilir()
    {
        var k = new TusAtamaKurali();
        TusEylemi c;
        Assert.AreEqual(AtamaSonucu.Tamam, k.Ata(TusEylemi.Incele, "Q", false, out c));
        Assert.AreEqual("Q", k.Klavye(TusEylemi.Incele));
        Assert.IsFalse(k.VarsayilanMi);
        // F feneri: incelemeye verilemez, çatışan eylem Fener
        Assert.AreEqual(AtamaSonucu.Catisma, k.Ata(TusEylemi.Incele, "F", false, out c));
        Assert.AreEqual(TusEylemi.Fener, c);
        Assert.AreEqual("Q", k.Klavye(TusEylemi.Incele), "çatışmada hiçbir şey değişmez");
        // Aynı eyleme aynı tuş: sorun yok
        Assert.AreEqual(AtamaSonucu.Tamam, k.Ata(TusEylemi.Incele, "Q", false, out c));
        // Kol: X (West) zaten incelemede; fenere verilemez
        Assert.AreEqual(AtamaSonucu.Catisma, k.Ata(TusEylemi.Fener, "West", true, out c));
        Assert.AreEqual(TusEylemi.Incele, c);
    }

    [Test]
    public void Sabit_tuslar_atanamaz()
    {
        var k = new TusAtamaKurali();
        TusEylemi c;
        foreach (string t in new[] { "Escape", "W", "A", "S", "D", "UpArrow", "Space", "Enter" })
            Assert.AreEqual(AtamaSonucu.Yasak, k.Ata(TusEylemi.Fener, t, false, out c), t);
        foreach (string t in new[] { "East", "Start", "DpadUp" })
            Assert.AreEqual(AtamaSonucu.Yasak, k.Ata(TusEylemi.Fener, t, true, out c), t);
        Assert.AreEqual(AtamaSonucu.Gecersiz, k.Ata(TusEylemi.Fener, "", false, out c));
        Assert.IsTrue(k.VarsayilanMi, "reddedilen atamalar tabloyu değiştirmez");
    }

    [Test]
    public void Kayit_metni_gidip_gelir()
    {
        var k = new TusAtamaKurali();
        TusEylemi c;
        k.Ata(TusEylemi.Incele, "Q", false, out c);
        Assert.AreEqual(AtamaSonucu.Catisma, k.Ata(TusEylemi.Egil, "LeftShift", false, out c), "Shift koşmanın (#129)");
        k.Ata(TusEylemi.Egil, "LeftAlt", false, out c);
        k.Ata(TusEylemi.GorevListesi, "North", true, out c);
        string metin = k.Metin();
        Assert.AreEqual("k:E,Q,F,T,LeftAlt,LeftShift;g:South,West,LeftShoulder,North,RightStick,LeftStick", metin);
        var geri = TusAtamaKurali.Oku(metin);
        Assert.AreEqual("Q", geri.Klavye(TusEylemi.Incele));
        Assert.AreEqual("LeftAlt", geri.Klavye(TusEylemi.Egil));
        Assert.AreEqual("North", geri.Kol(TusEylemi.GorevListesi));
        Assert.AreEqual("E", geri.Klavye(TusEylemi.Etkilesim));
    }

    [Test]
    public void Bozuk_metin_varsayilana_doner()
    {
        Assert.IsTrue(TusAtamaKurali.Oku("").VarsayilanMi);
        Assert.IsTrue(TusAtamaKurali.Oku("saçma").VarsayilanMi);
        Assert.IsTrue(TusAtamaKurali.Oku("k:E,E,F,T,LeftCtrl,LeftShift").VarsayilanMi, "iki eylemde aynı tuş: varsayılan");
        Assert.IsTrue(TusAtamaKurali.Oku("k:Escape,R,F,T,LeftCtrl,LeftShift").VarsayilanMi, "yasak tuş: varsayılan");
        var eksik = TusAtamaKurali.Oku("k:E,Q");
        Assert.AreEqual("Q", eksik.Klavye(TusEylemi.Incele));
        Assert.AreEqual("F", eksik.Klavye(TusEylemi.Fener), "yazılmayan eylem varsayılanda kalır");
    }
}
