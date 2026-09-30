// KatSecimiTestleri.cs
// Menüdeki "Katlar" sayfasının kuralı (#85): hangi kat açık, seçilince hangi katlar baştan başlar.

using NUnit.Framework;

[TestFixture]
public class KatSecimiTestleri
{
    const int Cati = 8;

    [Test]
    public void Ulasilan_kata_kadar_acik_ustu_kilitli()
    {
        Assert.That(KatSecimi.Acik(1, 4, Cati), Is.True);
        Assert.That(KatSecimi.Acik(4, 4, Cati), Is.True);
        Assert.That(KatSecimi.Acik(5, 4, Cati), Is.False);
        Assert.That(KatSecimi.Acik(8, 4, Cati), Is.False);
    }

    [Test]
    public void Birinci_kat_kayit_yokken_de_acik()
    {
        Assert.That(KatSecimi.Acik(1, 0, Cati), Is.True);
        Assert.That(KatSecimi.Acik(1, 1, Cati), Is.True);
        Assert.That(KatSecimi.Acik(2, 1, Cati), Is.False);
    }

    [Test]
    public void Listede_olmayan_kat_acik_degil()
    {
        Assert.That(KatSecimi.Acik(0, 8, Cati), Is.False);
        Assert.That(KatSecimi.Acik(9, 9, Cati), Is.False);
    }

    [Test]
    public void Secilen_kattan_catiya_kadar_silinir()
    {
        Assert.That(KatSecimi.SilinecekKatlar(3, Cati), Is.EqualTo(new[] { 3, 4, 5, 6, 7, 8 }));
        Assert.That(KatSecimi.SilinecekKatlar(8, Cati), Is.EqualTo(new[] { 8 }));
        Assert.That(KatSecimi.SilinecekKatlar(1, Cati).Count, Is.EqualTo(8));
    }

    [Test]
    public void Asagi_kat_secmek_en_yuksek_kati_dusurmez()
    {
        Assert.That(KatSecimi.YeniEnYuksek(6, 3), Is.EqualTo(6));
        Assert.That(KatSecimi.YeniEnYuksek(6, 7), Is.EqualTo(7));
    }
}
