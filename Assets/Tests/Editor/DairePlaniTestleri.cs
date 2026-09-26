// DairePlaniTestleri.cs
// 3+1 daire planının kuralları (#53). Plan bir kez yazılıyor, dört dairede ve sekiz katta
// kullanılıyor; bir hata her yerde görünür. Bu testler planın "gezilebilir bir daire"
// olduğunu denetler: odalar üst üste binmez, her kapı bir duvarda durur, her odaya girilir.

using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class DairePlaniTestleri
{
    [Test]
    public void Acik_daire_sirasi_2_3_1_4_ve_tekrar()
    {
        int[] beklenen = { 2, 3, 1, 4, 2, 3, 1, 4 };
        for (int kat = 1; kat <= 8; kat++)
            Assert.AreEqual(beklenen[kat - 1], DairePlani.AcikDaire(kat), $"{kat}. kat");
    }

    [Test]
    public void Zemin_kat_ve_cati_dairesiz()
    {
        Assert.AreEqual(0, DairePlani.AcikDaire(0), "zemin kat: kapıcı odası");
        Assert.AreEqual(0, DairePlani.AcikDaire(9), "çatı");
    }

    [Test]
    public void Tam_yedi_oda_ve_bir_antre_var()
    {
        // 3+1: salon + üç oda (A, B, C); artı antre, hol, mutfak, banyo.
        string[] adlar = { "Antre", "Hol", "Banyo", "Oda_C", "Mutfak", "Salon", "Oda_B", "Oda_A" };
        Assert.AreEqual(adlar.Length, DairePlani.Odalar.Length);
        foreach (string ad in adlar)
            Assert.IsTrue(DairePlani.OdaBul(ad, out _), $"{ad} planda yok");
    }

    [Test]
    public void Odalar_ust_uste_binmiyor_ve_dairenin_icinde()
    {
        var o = DairePlani.Odalar;
        for (int i = 0; i < o.Length; i++)
        {
            Assert.Greater(o[i].U1, o[i].U0, o[i].Ad);
            Assert.Greater(o[i].V1, o[i].V0, o[i].Ad);
            Assert.GreaterOrEqual(o[i].U0, DairePlani.UMin, o[i].Ad);
            Assert.LessOrEqual(o[i].U1, DairePlani.UMax, o[i].Ad);
            Assert.GreaterOrEqual(o[i].V0, DairePlani.VMin, $"{o[i].Ad} komşu dairenin tarafına taşıyor");
            Assert.LessOrEqual(o[i].V1, DairePlani.VMax, o[i].Ad);

            for (int j = i + 1; j < o.Length; j++)
            {
                // İki dikdörtgen, iki eksende de örtüşüyorsa üst üste biner. Aralarında en az
                // bölme duvarı (10 cm, küçük yuvarlama payıyla) kalmalı.
                bool uOrtusme = o[i].U0 < o[j].U1 - 0.09f && o[j].U0 < o[i].U1 - 0.09f;
                bool vOrtusme = o[i].V0 < o[j].V1 - 0.09f && o[j].V0 < o[i].V1 - 0.09f;
                Assert.IsFalse(uOrtusme && vOrtusme, $"{o[i].Ad} ile {o[j].Ad} üst üste biniyor");
            }
        }
    }

    [Test]
    public void Her_kapi_iki_odanin_arasinda_her_pencere_dis_duvarda()
    {
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
        {
            List<string> odalar = DairePlani.AciklikOdalari(a);
            Assert.Contains(a.Oda, odalar, $"{a.Ad}: ait olduğu oda '{a.Oda}' açıklığın yanında değil");

            bool icAciklik = a.Tur == DairePlani.AciklikTuru.Kapi || a.Tur == DairePlani.AciklikTuru.Gecit;
            if (icAciklik)
                Assert.AreEqual(2, odalar.Count, $"{a.Ad} iki odayı birleştirmeli, bulunan: {string.Join(", ", odalar)}");
            else
                Assert.AreEqual(1, odalar.Count, $"{a.Ad} dış duvarda olmalı, bulunan: {string.Join(", ", odalar)}");

            Assert.Greater(a.Ust, a.Alt, a.Ad);
            Assert.LessOrEqual(a.Ust, DairePlani.TavanYuk, a.Ad);
        }
    }

    [Test]
    public void Kapilar_oyuncunun_gecebilecegi_genislikte()
    {
        // Oyuncunun gövdesi 60 cm çapında (CharacterController yarıçapı 0,3). Kasa payıyla
        // 70 cm'den dar kapıdan geçemez.
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
            if (a.Tur != DairePlani.AciklikTuru.Pencere)
                Assert.GreaterOrEqual(a.Gen, 0.70f, a.Ad);
    }

    [Test]
    public void Her_odaya_antreden_bir_yol_var()
    {
        // Odalar düğüm, kapılar ve geçitler bağlantı: antreden başlayıp komşulara yayılıyoruz
        // (bir mürekkep damlasının kâğıtta yayılması gibi). Sonunda her oda boyanmış olmalı.
        var ulasilan = new HashSet<string> { "Antre" };
        bool degisti = true;
        while (degisti)
        {
            degisti = false;
            foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
            {
                if (a.Tur != DairePlani.AciklikTuru.Kapi && a.Tur != DairePlani.AciklikTuru.Gecit) continue;
                List<string> iki = DairePlani.AciklikOdalari(a);
                if (iki.Count != 2) continue;
                if (ulasilan.Contains(iki[0]) != ulasilan.Contains(iki[1]))
                {
                    ulasilan.Add(iki[0]);
                    ulasilan.Add(iki[1]);
                    degisti = true;
                }
            }
        }
        foreach (DairePlani.Oda o in DairePlani.Odalar)
            Assert.IsTrue(ulasilan.Contains(o.Ad), $"{o.Ad} odasına antreden girilemiyor");
    }

    [Test]
    public void Giris_sahanlik_duvarinda_ve_v_sifirda()
    {
        int giris = 0;
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
        {
            if (a.Tur != DairePlani.AciklikTuru.Giris) continue;
            giris++;
            Assert.IsTrue(a.UDuvari);
            Assert.AreEqual(0f, a.Cizgi, 1e-4f);
            Assert.AreEqual(0f, a.Merkez, 1e-4f, "giriş sahanlıktaki daire kapısının tam karşısında olmalı");
        }
        Assert.AreEqual(1, giris, "tek giriş olmalı");
    }
}
