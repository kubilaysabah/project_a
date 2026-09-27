// KatKaplamalariTestleri.cs
// Kat başına daire kaplamalarının kuralları (#53). Şahan'ın isteği: her katın dairesi
// farklı görünsün. Bu testler sekiz katın oda duvarının, parkesinin ve ıslak zemininin
// birbirinden gerçekten farklı olduğunu ve tablonun eksiksiz olduğunu denetler.

using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class KatKaplamalariTestleri
{
    [Test]
    public void Sekiz_katin_hepsi_tanimli()
    {
        Assert.AreEqual(KatSayaci.CatiKati, KatKaplamalari.KatSayisi, "her katın bir takımı olmalı");
        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
            for (int y = 0; y < KatKaplamalari.YuzeySayisi; y++)
            {
                var k = KatKaplamalari.Getir(kat, (KatKaplamalari.Yuzey)y);
                Assert.IsFalse(string.IsNullOrEmpty(k.Doku), $"{kat}. kat, yüzey {y}: doku yok");
                Assert.Greater(k.Boy, 0.1f, $"{kat}. kat, yüzey {y}: boy");
            }
    }

    // Her katta bu yüzeyin dokusu başka olmalı.
    private static void HepsiFarkli(KatKaplamalari.Yuzey yuzey)
    {
        var gorulen = new Dictionary<string, int>();
        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
        {
            string d = KatKaplamalari.Getir(kat, yuzey).Doku;
            Assert.IsFalse(gorulen.ContainsKey(d), $"{yuzey}: '{d}' hem {(gorulen.ContainsKey(d) ? gorulen[d] : 0)}. hem {kat}. katta");
            gorulen[d] = kat;
        }
    }

    [Test] public void Oda_duvarlari_her_katta_farkli() { HepsiFarkli(KatKaplamalari.Yuzey.OdaDuvari); }
    [Test] public void Parkeler_her_katta_farkli() { HepsiFarkli(KatKaplamalari.Yuzey.Parke); }
    [Test] public void Islak_zeminler_her_katta_farkli() { HepsiFarkli(KatKaplamalari.Yuzey.IslakZemin); }

    [Test]
    public void Islak_zemin_ve_duvar_ayni_katta_ayni_doku_degil()
    {
        // Banyonun zemini ve duvarı aynı karoyla kaplanırsa oda tek parça kutu gibi görünür.
        for (int kat = 1; kat <= KatSayaci.CatiKati; kat++)
            Assert.AreNotEqual(KatKaplamalari.Getir(kat, KatKaplamalari.Yuzey.IslakZemin).Doku,
                               KatKaplamalari.Getir(kat, KatKaplamalari.Yuzey.IslakDuvar).Doku, $"{kat}. kat");
    }

    [Test]
    public void Tablonun_disindaki_kat_en_yakina_yuvarlanir()
    {
        Assert.AreEqual(KatKaplamalari.Getir(1, KatKaplamalari.Yuzey.Parke).Doku, KatKaplamalari.Getir(0, KatKaplamalari.Yuzey.Parke).Doku);
        Assert.AreEqual(KatKaplamalari.Getir(8, KatKaplamalari.Yuzey.Parke).Doku, KatKaplamalari.Getir(9, KatKaplamalari.Yuzey.Parke).Doku);
    }
}
