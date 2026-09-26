// AyakkabiPlaniTestleri.cs
// Ayakkabılık ve dağınık ayakkabı kuralının testleri (#44).
//
// Korunan sözler:
//   1. Rafta 2-3, kapı önünde 0-2 çift; toplamı hiçbir zaman raf sayısını (5) geçmez.
//   2. Bir turda aynı kat hep aynı sayıları verir.
//   3. Rafa konan çift hatırlanır ve dolu raf sayısını artırır.
//   4. Cezadan sonra (Sifirla) sayılar da rafa konanlar da sıfırlanır.

using NUnit.Framework;

[TestFixture]
public class AyakkabiPlaniTestleri
{
    private const int Daire = 4;

    [Test]
    public void Sayilar_sinirlarin_icinde_ve_rafa_sigar()
    {
        for (int tohum = 0; tohum < 50; tohum++)
        {
            var plan = new AyakkabiPlani(Daire, tohum);
            for (int kat = 1; kat <= 8; kat++)
            {
                int[] raf = plan.Raftakiler(kat), dis = plan.Disaridakiler(kat);
                for (int d = 0; d < Daire; d++)
                {
                    Assert.GreaterOrEqual(raf[d], AyakkabiPlani.RaftaEnAz);
                    Assert.LessOrEqual(raf[d], AyakkabiPlani.RaftaEnCok);
                    Assert.GreaterOrEqual(dis[d], 0);
                    Assert.LessOrEqual(dis[d], AyakkabiPlani.DisaridaEnCok);
                    Assert.LessOrEqual(raf[d] + dis[d], AyakkabiPlani.RafSayisi, "dağınık çiftler rafa sığmıyor");
                }
            }
        }
    }

    // Üst sınırın "hariç" olması yüzünden 3 ve 2'nin hiç çıkmaması klasik bir hatadır.
    [Test]
    public void Uc_sinir_degerleri_de_cikar()
    {
        var plan = new AyakkabiPlani(Daire, 3);
        bool raf2 = false, raf3 = false, dis0 = false, dis2 = false;
        for (int kat = 1; kat <= 100; kat++)
            for (int d = 0; d < Daire; d++)
            {
                int r = plan.Raftakiler(kat)[d], s = plan.Disaridakiler(kat)[d];
                raf2 |= r == 2; raf3 |= r == 3; dis0 |= s == 0; dis2 |= s == 2;
            }
        Assert.IsTrue(raf2 && raf3 && dis0 && dis2);
    }

    [Test]
    public void Ayni_kat_tur_boyunca_ayni_sayilari_verir()
    {
        var plan = new AyakkabiPlani(Daire, 9);
        int[] raf = plan.Raftakiler(4), dis = plan.Disaridakiler(4);
        for (int kat = 1; kat <= 8; kat++) plan.Raftakiler(kat);
        CollectionAssert.AreEqual(raf, plan.Raftakiler(4));
        CollectionAssert.AreEqual(dis, plan.Disaridakiler(4));
    }

    [Test]
    public void Rafa_konan_cift_hatirlanir_ve_dolu_rafi_artirir()
    {
        var plan = new AyakkabiPlani(Daire, 1);
        // Dışarıda en az bir çift olan bir kat ve daire bul.
        int kat = 1, daire = -1;
        for (; kat < 50 && daire < 0; kat++)
            for (int d = 0; d < Daire; d++) if (plan.Disaridakiler(kat)[d] > 0) { daire = d; break; }
        kat--;
        Assert.GreaterOrEqual(daire, 0);

        int once = plan.DoluRaf(kat, daire);
        Assert.IsFalse(plan.RafaKonduMu(kat, daire, 0));
        plan.RafaKonduOlarakIsaretle(kat, daire, 0);
        Assert.IsTrue(plan.RafaKonduMu(kat, daire, 0));
        Assert.AreEqual(once + 1, plan.DoluRaf(kat, daire));
        Assert.IsFalse(plan.RafaKonduMu(kat + 1, daire, 0), "başka katın çifti etkilenmemeli");
    }

    [Test]
    public void Sifirladiktan_sonra_her_sey_yeniden_cekilir()
    {
        var plan = new AyakkabiPlani(Daire, 5);
        var once = new System.Collections.Generic.List<int>();
        for (int kat = 1; kat <= 8; kat++) { once.AddRange(plan.Raftakiler(kat)); once.AddRange(plan.Disaridakiler(kat)); }
        plan.RafaKonduOlarakIsaretle(2, 1, 0);

        plan.Sifirla();

        Assert.IsFalse(plan.RafaKonduMu(2, 1, 0));
        var sonra = new System.Collections.Generic.List<int>();
        for (int kat = 1; kat <= 8; kat++) { sonra.AddRange(plan.Raftakiler(kat)); sonra.AddRange(plan.Disaridakiler(kat)); }
        CollectionAssert.AreNotEqual(once, sonra);
    }
}
