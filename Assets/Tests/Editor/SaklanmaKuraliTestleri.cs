// SaklanmaKuraliTestleri.cs
// Gardıroba saklanma kuralının testleri (#97). Ölçüler sahnedeki gerçek modellerden (Blender'da
// FBX'lerin sınır kutusu ölçüldü, 2026-10-01).

using NUnit.Framework;

[TestFixture]
public class SaklanmaKuraliTestleri
{
    [Test]
    public void Gercek_gardirop_ve_boy_dolabi_saklanma_yeri()
    {
        // 6. katın büfesi (1,40 x 0,47 x 1,90) ve 5. katın mutfaktaki boy dolabı (Poly Haven).
        Assert.IsTrue(SaklanmaKurali.Aday("Bufe", 1.40f, 0.47f, 1.90f));
        Assert.IsTrue(SaklanmaKurali.Aday("Sus_painted_wooden_cabinet_02", 1.00f, 0.72f, 2.57f));
        Assert.IsTrue(SaklanmaKurali.Aday("Gardirop", 1.07f, 0.61f, 1.90f));
    }

    [Test]
    public void Adi_uysa_da_kucuk_dolaba_saklanilmaz()
    {
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Tel_Dolap_K6", 0.54f, 0.40f, 0.80f));
        Assert.IsFalse(SaklanmaKurali.Aday("Bufe_Cemal", 1.24f, 0.50f, 1.50f));   // boyu 1,6'nın altında
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Ust_Dolap", 1.26f, 0.92f, 0.65f));
        Assert.IsFalse(SaklanmaKurali.Aday("Gardirop", 1.00f, 0.44f, 1.90f));     // sığ
        Assert.IsFalse(SaklanmaKurali.Aday("Gardirop", 0.50f, 0.60f, 1.90f));     // dar
    }

    [Test]
    public void Adi_dolap_olmayan_buyuk_mobilya_saklanma_yeri_degil()
    {
        // Kitaplık ve camlı vitrin yeterince büyük ama içine girilecek mobilya değil.
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Kitaplik_K5", 0.94f, 0.50f, 1.94f));
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_vintage_cabinet_01", 2.02f, 0.67f, 2.24f));
        Assert.IsFalse(SaklanmaKurali.Aday("", 1f, 1f, 2f));
        Assert.IsFalse(SaklanmaKurali.Aday(null, 1f, 1f, 2f));
    }

    [Test]
    public void Dislananlar_saklanma_yeri_degil()
    {
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Acik_Gardirop", 1.07f, 1.03f, 1.90f));
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Arsiv_Dolabi_K8", 1.04f, 0.51f, 2.00f));
    }

    [Test]
    public void Goz_yuksekligi_tavanin_altinda_kalir()
    {
        Assert.That(SaklanmaKurali.GozYuksekligi(2.57f), Is.EqualTo(1.55f).Within(1e-5f));
        Assert.That(SaklanmaKurali.GozYuksekligi(1.65f), Is.EqualTo(1.50f).Within(1e-5f));
    }

    [Test]
    public void Bakis_sinirlanir()
    {
        Assert.That(SaklanmaKurali.Sinirla(50f, 35f), Is.EqualTo(35f));
        Assert.That(SaklanmaKurali.Sinirla(-50f, 35f), Is.EqualTo(-35f));
        Assert.That(SaklanmaKurali.Sinirla(10f, 35f), Is.EqualTo(10f));
    }

    [Test]
    public void Buzdolabi_saklanma_yeri_degildir()
    {
        // Adında "dolab" geçse de buzdolabına girilmez (Sus_Buzdolabi_K3, Buzdolabi_70).
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Buzdolabi_K3", 0.70f, 0.65f, 1.80f));
        Assert.IsFalse(SaklanmaKurali.Aday("Sus_Buzdolabi_70", 0.60f, 0.60f, 1.70f));
        Assert.IsTrue(SaklanmaKurali.Aday("Gardirop_Nermin", 1.02f, 0.57f, 1.88f));
    }
}
