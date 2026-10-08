// CinOlayKuraliTestleri.cs
// Cin olayları kuralının testleri (#131).

using NUnit.Framework;

public class CinOlayKuraliTestleri
{
    private static bool[] Hepsi()
    {
        var m = new bool[CinOlayKurali.OlaySayisi];
        for (int i = 1; i < m.Length; i++) m[i] = true;
        return m;
    }

    // Sayaç dolana kadar uygun saniyeleri sayar; dolduğunda geçen süreyi döner.
    private static float DolanaKadar(CinOlayKurali k, float enCok = 400f)
    {
        float t = 0f;
        while (!k.Ilerlet(0.1f, true)) { t += 0.1f; if (t > enCok) Assert.Fail("sayaç hiç dolmadı"); }
        return t;
    }

    [Test]
    public void Ilk_olay_katin_araliginin_IlkCarpan_kati_icinde_gelir()
    {
        for (int kat = 1; kat <= 8; kat++)
        {
            var k = new CinOlayKurali(kat * 17);
            k.KatAyarla(kat);
            float t = DolanaKadar(k);
            CinOlayKurali.AralikSiniri(kat, out float enAz, out float enCok);
            Assert.GreaterOrEqual(t, enAz * CinOlayKurali.IlkCarpan - 0.2f, "kat " + kat);
            Assert.LessOrEqual(t, enCok * CinOlayKurali.IlkCarpan + 0.2f, "kat " + kat);
        }
    }

    [Test]
    public void Sonraki_olaylar_katin_araliginda_ve_ust_katta_daha_sik()
    {
        var k1 = new CinOlayKurali(5); k1.KatAyarla(1);
        var k8 = new CinOlayKurali(5); k8.KatAyarla(8);
        DolanaKadar(k1); k1.Sec(Hepsi());
        DolanaKadar(k8); k8.Sec(Hepsi());
        float t1 = DolanaKadar(k1), t8 = DolanaKadar(k8);
        Assert.GreaterOrEqual(t1, CinOlayKurali.EnAzAralikKat1 - 0.2f);
        Assert.LessOrEqual(t1, CinOlayKurali.EnCokAralikKat1 + 0.2f);
        Assert.GreaterOrEqual(t8, CinOlayKurali.EnAzAralikKat8 - 0.2f);
        Assert.LessOrEqual(t8, CinOlayKurali.EnCokAralikKat8 + 0.2f);
    }

    [Test]
    public void Uygun_degilken_sayac_islemez_ve_dolunca_Sec_bekler()
    {
        var k = new CinOlayKurali(3);
        for (int i = 0; i < 5000; i++) Assert.IsFalse(k.Ilerlet(0.1f, false), "sahanlıkta sayaç işlemez");
        DolanaKadar(k);
        Assert.IsTrue(k.Ilerlet(0.1f, true), "Sec çağrılana kadar dolu kalır");
        Assert.AreNotEqual(CinOlayi.Yok, k.Sec(Hepsi()));
        Assert.IsFalse(k.Ilerlet(0.1f, true), "seçimden sonra yeniden kuruldu");
    }

    [Test]
    public void Ayni_olay_ust_uste_gelmez()
    {
        var k = new CinOlayKurali(11);
        CinOlayi onceki = CinOlayi.Yok;
        for (int i = 0; i < 40; i++)
        {
            DolanaKadar(k);
            CinOlayi o = k.Sec(Hepsi());
            Assert.AreNotEqual(CinOlayi.Yok, o);
            Assert.AreNotEqual(onceki, o, "tekrar: " + o);
            onceki = o;
        }
    }

    [Test]
    public void Yalnizca_mevcut_olaylar_secilir_hicbiri_yoksa_Yok_ve_BosAralik()
    {
        var k = new CinOlayKurali(7);
        var yalniz = new bool[CinOlayKurali.OlaySayisi];
        yalniz[(int)CinOlayi.ArkadanAdim] = true;
        DolanaKadar(k);
        Assert.AreEqual(CinOlayi.ArkadanAdim, k.Sec(yalniz));
        // Şimdi yalnızca son olay mevcut: tekrar etmek yerine beklenir.
        DolanaKadar(k);
        Assert.AreEqual(CinOlayi.Yok, k.Sec(yalniz));
        float t = DolanaKadar(k);
        Assert.AreEqual(CinOlayKurali.BosAralik, t, 0.15f);
        // Hiçbir olay yok.
        Assert.AreEqual(CinOlayi.Yok, k.Sec(new bool[CinOlayKurali.OlaySayisi]));
    }

    // #135: torba. Bir torba boşalana kadar çekilen olaylarda hiçbir tür ağırlığından fazla çıkamaz; torba
    // yenilenmeden boşalan bir turda her tür TAM ağırlığı kadar çıkar (tohumlar denenir, en az biri böyle
    // bir tur verir). Aynı tür art arda gelmez.
    [Test]
    public void Torba_bir_turda_hicbir_turu_agirligindan_fazla_cikarmaz_ve_tam_tur_agirliklari_verir()
    {
        bool tamTurGoruldu = false;
        for (int tohum = 1; tohum <= 30; tohum++)
        {
            var k = new CinOlayKurali(tohum);
            var sayim = new int[CinOlayKurali.OlaySayisi];
            int cekilen = 0; bool yenilendi = false;
            CinOlayi onceki = CinOlayi.Yok;
            while (true)
            {
                DolanaKadar(k);
                int onceKalan = k.TorbadaKalan;
                CinOlayi o = k.Sec(Hepsi());
                Assert.AreNotEqual(CinOlayi.Yok, o);
                Assert.AreNotEqual(onceki, o, "tekrar (tohum " + tohum + "): " + o);
                onceki = o;
                if (cekilen > 0 && k.TorbadaKalan >= onceKalan) { yenilendi = true; break; }   // torba yenilendi
                sayim[(int)o]++; cekilen++;
                if (k.TorbadaKalan == 0) break;
            }
            for (int i = 1; i < CinOlayKurali.OlaySayisi; i++)
                Assert.LessOrEqual(sayim[i], CinOlayKurali.Agirlik((CinOlayi)i), "tohum " + tohum + " tür " + (CinOlayi)i);
            if (!yenilendi && cekilen == CinOlayKurali.TorbaBoyu)
            {
                for (int i = 1; i < CinOlayKurali.OlaySayisi; i++)
                    Assert.AreEqual(CinOlayKurali.Agirlik((CinOlayi)i), sayim[i], "tam tur, tohum " + tohum + " tür " + (CinOlayi)i);
                tamTurGoruldu = true;
            }
        }
        Assert.IsTrue(tamTurGoruldu, "30 tohumda bir kez bile torba yenilenmeden boşalmadı");
    }

    // #137: dört yeni tür torbada ağırlığıyla var; kural tabloları tutarlı.
    [Test]
    public void Yeni_olay_turleri_torbada_agirligiyla_var()
    {
        Assert.AreEqual(14, CinOlayKurali.OlaySayisi);
        Assert.AreEqual(2, CinOlayKurali.Agirlik(CinOlayi.AyakIzi));
        Assert.AreEqual(2, CinOlayKurali.Agirlik(CinOlayi.KapiGozu));
        Assert.AreEqual(1, CinOlayKurali.Agirlik(CinOlayi.Zil));
        Assert.AreEqual(2, CinOlayKurali.Agirlik(CinOlayi.EsyaDonmesi));
        Assert.AreEqual(27, CinOlayKurali.TorbaBoyu);
    }

    // #135: sahanlıkta yalnız görünür olaylar yapılabilir; torbanın kalanı daire olaylarıysa oyuncu
    // beklemez, torba yenilenir ve görünür bir olay gelir.
    [Test]
    public void Torbanin_kalani_yapilamiyorsa_torba_yenilenir_ve_yapilabilen_olay_gelir()
    {
        var k = new CinOlayKurali(4);
        var yalnizGorunur = new bool[CinOlayKurali.OlaySayisi];
        yalnizGorunur[(int)CinOlayi.Belirme] = true;
        yalnizGorunur[(int)CinOlayi.Gecis] = true;
        yalnizGorunur[(int)CinOlayi.Kacis] = true;
        for (int i = 0; i < 40; i++)
        {
            DolanaKadar(k);
            CinOlayi o = k.Sec(yalnizGorunur);
            Assert.IsTrue(o == CinOlayi.Belirme || o == CinOlayi.Gecis || o == CinOlayi.Kacis, "çekilen: " + o);
        }
    }

    [Test]
    public void Kat_degisince_son_olay_unutulur_ve_ilk_gecikme_geri_gelir()
    {
        var k = new CinOlayKurali(2);
        DolanaKadar(k); k.Sec(Hepsi());
        Assert.AreNotEqual(CinOlayi.Yok, k.Son);
        k.KatAyarla(2);
        Assert.AreEqual(CinOlayi.Yok, k.Son);
        float t = DolanaKadar(k);
        CinOlayKurali.AralikSiniri(2, out float enAz, out _);
        Assert.GreaterOrEqual(t, enAz * CinOlayKurali.IlkCarpan - 0.2f);
    }
}
