// AnomaliDefteriTestleri.cs
// Anomali defteri listesinin testleri (#45).

using NUnit.Framework;

[TestFixture]
public class AnomaliDefteriTestleri
{
    [Test]
    public void Yeni_anomali_bir_kez_islenir()
    {
        var d = new AnomaliDefteri();
        Assert.IsTrue(d.Isle("acik_kapi"), "ilk kez bulunan anomali yeni sayılmalı");
        Assert.IsFalse(d.Isle("acik_kapi"), "aynı anomali ikinci kez yeni sayılmamalı");
        Assert.AreEqual(1, d.Sayi);
        Assert.IsTrue(d.BulunduMu("acik_kapi"));
        Assert.IsFalse(d.BulunduMu("ters_numara"));
    }

    [Test]
    public void Bos_ya_da_virgullu_kimlik_islenmez()
    {
        var d = new AnomaliDefteri();
        Assert.IsFalse(d.Isle(""));
        Assert.IsFalse(d.Isle(null));
        Assert.IsFalse(d.Isle("a,b"));   // virgül kayıttaki ayraç; içerisinde olursa kayıt bozulurdu
        Assert.AreEqual(0, d.Sayi);
    }

    [Test]
    public void Yaziya_cevirip_geri_okuyunca_ayni_liste_gelir()
    {
        var d = new AnomaliDefteri();
        d.Isle("yesil_ampul"); d.Isle("acik_kapi"); d.Isle("calan_zil");
        string yazi = d.Yazi();
        Assert.AreEqual("acik_kapi,calan_zil,yesil_ampul", yazi, "yazı sıralı olmalı");

        var geri = AnomaliDefteri.Oku(yazi);
        Assert.AreEqual(3, geri.Sayi);
        Assert.IsTrue(geri.BulunduMu("calan_zil"));
    }

    [Test]
    public void Bos_ya_da_bozuk_yazi_guvenle_okunur()
    {
        Assert.AreEqual(0, AnomaliDefteri.Oku(null).Sayi);
        Assert.AreEqual(0, AnomaliDefteri.Oku("").Sayi);
        Assert.AreEqual(2, AnomaliDefteri.Oku(",acik_kapi,, calan_zil ,").Sayi);
    }
}
