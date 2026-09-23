// AnomaliZamanlamaTestleri.cs
// Zamanlı etkilerin ve ses etkisinin testleri (#13, KÂBUS anları).
//
// Korunan sözler:
//   1. Gecikmeli bir etki, süresi dolmadan uygulanmaz; dolunca uygulanır.
//   2. Geri alma, henüz gelmemiş etkileri de iptal eder: kat değişince eski kartın
//      senaryosu yeni katta oynamaz.
//   3. Zamanla uygulanmış etki de geri alınır.
//   4. Ses etkisinin ürettiği ses kaynağı geri alınınca silinir (ses susar).
//   5. Birden fazla zamanlı etki, zamanlarının sırasıyla uygulanır.
//
// Gerçek sahneye gerek yok: her test kendine küçük bir "sahanlık" nesnesi kurar.

using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AnomaliZamanlamaTestleri
{
    private GameObject sahanlik;
    private Transform kapi;

    [SetUp]
    public void Hazirla()
    {
        sahanlik = new GameObject("Test_Sahanlik");
        kapi = new GameObject("Kapi").transform;
        kapi.SetParent(sahanlik.transform, false);
    }

    [TearDown]
    public void Topla()
    {
        Object.DestroyImmediate(sahanlik);
    }

    // Kartın alanları dışarıya kapalı (kart yalnızca Inspector'dan düzenlensin diye).
    // Testte kart kurmak için "yansıma" (reflection) kullanıyoruz: C#'ın, bir sınıfın
    // gizli alanlarına adıyla ulaşmayı sağlayan aracı. Oyun kodunda kullanılmaz,
    // yalnızca testlerde kapalı bir kutunun içine bakmak için.
    private static AnomaliKarti KartKur(params AnomaliEtkisi[] etkiler)
    {
        var kart = ScriptableObject.CreateInstance<AnomaliKarti>();
        var bayrak = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(AnomaliKarti).GetField("kimlik", bayrak).SetValue(kart, "test_karti");
        typeof(AnomaliKarti).GetField("etkiler", bayrak).SetValue(kart, etkiler);
        return kart;
    }

    private static AnomaliEtkisi Kaydir(float gecikme, float y)
    {
        return new AnomaliEtkisi { hedefYol = "Kapi", tur = EtkiTuru.Konum, vektor = new Vector3(0f, y, 0f), gecikme = gecikme };
    }

    [Test]
    public void Gecikmeli_etki_suresi_dolmadan_uygulanmaz_dolunca_uygulanir()
    {
        var u = new AnomaliUygulayici();
        u.Uygula(KartKur(Kaydir(5f, 1f)), sahanlik.transform);

        u.Ilerlet(4.9f);
        Assert.AreEqual(0f, kapi.localPosition.y, 1e-4, "5 saniye dolmadan etki uygulandı.");
        Assert.AreEqual(1, u.BekleyenEtkiSayisi);

        u.Ilerlet(0.2f);
        Assert.AreEqual(1f, kapi.localPosition.y, 1e-4, "Süre dolduğu hâlde etki uygulanmadı.");
        Assert.AreEqual(0, u.BekleyenEtkiSayisi);
    }

    [Test]
    public void Geri_alma_bekleyen_etkiyi_iptal_eder()
    {
        var u = new AnomaliUygulayici();
        u.Uygula(KartKur(Kaydir(5f, 1f)), sahanlik.transform);

        u.GeriAl();
        u.Ilerlet(10f);

        Assert.AreEqual(0f, kapi.localPosition.y, 1e-4, "Geri alınan kartın etkisi sonradan yine de uygulandı.");
        Assert.AreEqual(0, u.BekleyenEtkiSayisi);
    }

    [Test]
    public void Zamanla_uygulanmis_etki_geri_alinir()
    {
        var u = new AnomaliUygulayici();
        u.Uygula(KartKur(Kaydir(2f, 1f)), sahanlik.transform);
        u.Ilerlet(3f);

        u.GeriAl();

        Assert.AreEqual(0f, kapi.localPosition.y, 1e-4, "Zamanlı etki geri alınmadı.");
    }

    [Test]
    public void Zamanli_etkiler_zaman_sirasiyla_uygulanir()
    {
        // Karta ters sırada yazılmış iki etki: 3. saniyede +1, 1. saniyede +10.
        var u = new AnomaliUygulayici();
        u.Uygula(KartKur(Kaydir(3f, 1f), Kaydir(1f, 10f)), sahanlik.transform);

        u.Ilerlet(1.5f);
        Assert.AreEqual(10f, kapi.localPosition.y, 1e-4, "Erken olan etki önce uygulanmadı.");

        u.Ilerlet(2f);
        Assert.AreEqual(11f, kapi.localPosition.y, 1e-4);
    }

    [Test]
    public void Ses_etkisi_ses_kaynagi_uretir_geri_alininca_siler()
    {
        // Testte gerçek bir ses dosyasına gerek yok: bellekte kısa, sessiz bir klip üretiyoruz.
        AudioClip klip = AudioClip.Create("test_sesi", 4410, 1, 44100, false);
        var etki = new AnomaliEtkisi { hedefYol = "Kapi", tur = EtkiTuru.Ses, ses = klip, sayi = 1f };

        var u = new AnomaliUygulayici();
        u.Uygula(KartKur(etki), sahanlik.transform);

        Transform ses = kapi.Find("Anomali_Sesi");
        Assert.IsNotNull(ses, "Ses etkisi hedefin altında ses kaynağı üretmedi.");
        Assert.AreSame(klip, ses.GetComponent<AudioSource>().clip);

        u.GeriAl();

        Assert.IsNull(kapi.Find("Anomali_Sesi"), "Geri alınınca ses kaynağı silinmedi; ses çalmaya devam ederdi.");
        Object.DestroyImmediate(klip);
    }
}
