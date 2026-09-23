// AnomaliSeciciTestleri.cs
// Seçicinin testleri. Unity hiç açılmadan çalışır.
//
// Bunu mümkün kılan şey IAnomaliKart arayüzü: seçici karşısındakinin
// gerçek bir Unity kartı mı yoksa aşağıdaki sahte kart mı olduğunu bilmez.

using NUnit.Framework;

[TestFixture]
public class AnomaliSeciciTestleri
{
    // ---------- SAHTE KART ----------
    // Sözleşmeyi imzalayan en küçük sınıf. Gerçek kartın Unity tarafına
    // hiç ihtiyacımız yok; seçici sadece bu üç bilgiyi soruyor.
    private class SahteKart : IAnomaliKart
    {
        private readonly string kimlik;
        private readonly int zorluk;

        public SahteKart(string kimlik, int zorluk)
        {
            this.kimlik = kimlik;
            this.zorluk = zorluk;
        }

        public string Kimlik { get { return kimlik; } }
        public AnomaliKatmani Katman { get { return AnomaliKatmani.Fisilti; } }
        public int Zorluk { get { return zorluk; } }
    }

    // Her zorluk seviyesinden ikişer kart: toplam 10.
    private static IAnomaliKart[] OnKart()
    {
        return new IAnomaliKart[]
        {
            new SahteKart("k1a", 1), new SahteKart("k1b", 1),
            new SahteKart("k2a", 2), new SahteKart("k2b", 2),
            new SahteKart("k3a", 3), new SahteKart("k3b", 3),
            new SahteKart("k4a", 4), new SahteKart("k4b", 4),
            new SahteKart("k5a", 5), new SahteKart("k5b", 5)
        };
    }

    // ---------- OLASILIK ----------

    [Test]
    public void Olasilik_sifirsa_hicbir_kat_anomalili_olmaz()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 1, anomaliOlasiligi: 0f);

        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            Assert.IsNull(secici.KatIcinSec(kat), $"{kat}. kat normal olmalıydı");
        }
    }

    [Test]
    public void Olasilik_birse_ilk_ve_son_kat_disinda_hepsi_anomalili_olur()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 1, anomaliOlasiligi: 1f);

        Assert.IsNull(secici.KatIcinSec(1), "1. kat her zaman normal olmalı");
        Assert.IsNull(secici.KatIcinSec(KatSayaci.CatiKati), "son kat her zaman normal olmalı");

        for (int kat = 2; kat < KatSayaci.CatiKati; kat++)
        {
            Assert.IsNotNull(secici.KatIcinSec(kat), $"{kat}. kat anomalili olmalıydı");
        }
    }

    // 1. kat oyuncuya NORMALİN neye benzediğini öğretir.
    [Test]
    public void Ilk_kat_varsayilan_olarak_hep_normaldir()
    {
        for (int tohum = 0; tohum < 30; tohum++)
        {
            var secici = new AnomaliSecici(OnKart(), tohum, anomaliOlasiligi: 1f);
            Assert.IsNull(secici.KatIcinSec(1), $"tohum {tohum}: 1. kat normal olmalıydı");
        }
    }

    [Test]
    public void Ilk_kat_kurali_kapatilabilir()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 1,
                                       anomaliOlasiligi: 1f, ilkKatHepNormal: false);

        Assert.IsNotNull(secici.KatIcinSec(1));
    }

    // Son katın üstü çatı. Anomalili olsaydı doğru hamle (geri dönmek) de
    // "çatıya ulaşıldı" sayılırdı ve oyuncu var olmayan bir 9. katı görürdü.
    // Kural kapatılamaz: ilk kat kuralı kapalıyken bile son kat temiz kalmalı.
    [Test]
    public void Son_kat_her_zaman_normaldir_ve_kapatilamaz()
    {
        for (int tohum = 0; tohum < 30; tohum++)
        {
            var secici = new AnomaliSecici(OnKart(), tohum,
                                           anomaliOlasiligi: 1f, ilkKatHepNormal: false);
            Assert.IsNull(secici.KatIcinSec(KatSayaci.CatiKati),
                          $"tohum {tohum}: son kat normal olmalıydı");
            Assert.IsNull(secici.KatIcinSec(KatSayaci.CatiKati + 1),
                          $"tohum {tohum}: çatının üstünde anomali olmamalı");
        }
    }

    // ---------- KAT HAFIZASI ----------
    // Üst sahanlık bir üst katı gösterdiği için aynı kat iki kez sorulur.
    // İki cevap farklı olsaydı ışınlanma anında sahanlık değişir, hile açığa çıkardı.

    [Test]
    public void Ayni_kat_iki_kez_sorulursa_ayni_cevap_doner()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 7, anomaliOlasiligi: 0.5f);

        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            IAnomaliKart birinci = secici.KatIcinSec(kat);
            IAnomaliKart ikinci = secici.KatIcinSec(kat);

            Assert.AreSame(birinci, ikinci, $"{kat}. kat için iki farklı cevap geldi");
        }
    }

    [Test]
    public void Sifirla_kat_kararlarini_temizler()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 3, anomaliOlasiligi: 1f);

        secici.KatIcinSec(2);
        secici.KatIcinSec(3);
        Assert.AreEqual(2, secici.KararVerilenKatSayisi);

        secici.Sifirla();

        Assert.AreEqual(0, secici.KararVerilenKatSayisi);
    }

    // ---------- TEKRAR ENGELLEME ----------

    [Test]
    public void Ayni_kart_ust_uste_gelmez()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 11, anomaliOlasiligi: 1f);

        string oncekiKimlik = null;

        // Birçok tur oyna: her turda 2-7 arası katlar anomalili gelir (8. kat hep normal).
        for (int tur = 0; tur < 40; tur++)
        {
            for (int kat = 2; kat < KatSayaci.CatiKati; kat++)
            {
                IAnomaliKart kart = secici.KatIcinSec(kat);
                Assert.IsNotNull(kart);

                Assert.AreNotEqual(oncekiKimlik, kart.Kimlik,
                    $"'{kart.Kimlik}' üst üste iki kez geldi");

                oncekiKimlik = kart.Kimlik;
            }
            secici.Sifirla();
        }
    }

    // ---------- ZORLUK EĞRİSİ ----------

    [Test]
    public void Zorluk_tavani_kat_ile_yukselir()
    {
        Assert.AreEqual(1, AnomaliSecici.ZorlukTavani(1));
        Assert.AreEqual(5, AnomaliSecici.ZorlukTavani(KatSayaci.CatiKati));

        // Aradaki her adım bir öncekinden küçük olmamalı.
        for (int kat = 2; kat <= KatSayaci.CatiKati; kat++)
        {
            Assert.GreaterOrEqual(AnomaliSecici.ZorlukTavani(kat),
                                  AnomaliSecici.ZorlukTavani(kat - 1),
                                  $"{kat}. katta tavan düştü");
        }
    }

    [Test]
    public void Secilen_kart_kattaki_zorluk_tavanini_asmaz()
    {
        var secici = new AnomaliSecici(OnKart(), tohum: 5, anomaliOlasiligi: 1f);

        for (int tur = 0; tur < 30; tur++)
        {
            for (int kat = 2; kat < KatSayaci.CatiKati; kat++)   // 8. kat hep normal
            {
                IAnomaliKart kart = secici.KatIcinSec(kat);
                int tavan = AnomaliSecici.ZorlukTavani(kat);

                Assert.LessOrEqual(kart.Zorluk, tavan,
                    $"{kat}. katta tavan {tavan} ama '{kart.Kimlik}' zorluğu {kart.Zorluk}");
            }
            secici.Sifirla();
        }
    }

    // ---------- SINIR DURUMLARI ----------

    [Test]
    public void Kart_yoksa_cokmez_ve_normal_doner()
    {
        var secici = new AnomaliSecici(new IAnomaliKart[0], tohum: 1, anomaliOlasiligi: 1f);

        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            Assert.IsNull(secici.KatIcinSec(kat));
        }
    }

    [Test]
    public void Kart_listesi_null_gelse_bile_cokmez()
    {
        var secici = new AnomaliSecici(null, tohum: 1, anomaliOlasiligi: 1f);

        Assert.IsNull(secici.KatIcinSec(3));
    }

    // Erken katlarda sadece zor kart varsa yine de bir kart dönmeli;
    // yoksa anomalili olması gereken kat sessizce normal kalır ve kural çöker.
    [Test]
    public void Sadece_zor_kart_varsa_erken_katta_bile_kart_doner()
    {
        var sadeceZor = new IAnomaliKart[]
        {
            new SahteKart("zor1", 5),
            new SahteKart("zor2", 5)
        };

        var secici = new AnomaliSecici(sadeceZor, tohum: 2, anomaliOlasiligi: 1f);

        IAnomaliKart kart = secici.KatIcinSec(2);

        Assert.IsNotNull(kart, "zorluk sınırı yüzünden kat anomalisiz kalmamalı");
    }

    [Test]
    public void Tek_kart_varsa_da_calisir()
    {
        var tek = new IAnomaliKart[] { new SahteKart("tek", 1) };
        var secici = new AnomaliSecici(tek, tohum: 4, anomaliOlasiligi: 1f);

        for (int kat = 2; kat < KatSayaci.CatiKati; kat++)   // 8. kat hep normal
        {
            Assert.AreEqual("tek", secici.KatIcinSec(kat).Kimlik);
        }
    }

    // ---------- KARARLILIK ----------

    [Test]
    public void Ayni_tohum_ayni_oyunu_uretir()
    {
        var a = new AnomaliSecici(OnKart(), tohum: 42, anomaliOlasiligi: 0.5f);
        var b = new AnomaliSecici(OnKart(), tohum: 42, anomaliOlasiligi: 0.5f);

        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            IAnomaliKart ka = a.KatIcinSec(kat);
            IAnomaliKart kb = b.KatIcinSec(kat);

            string na = ka == null ? "normal" : ka.Kimlik;
            string nb = kb == null ? "normal" : kb.Kimlik;

            Assert.AreEqual(na, nb, $"{kat}. kat iki seçicide farklı çıktı");
        }
    }
}
