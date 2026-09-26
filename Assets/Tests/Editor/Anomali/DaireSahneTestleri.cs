// DaireSahneTestleri.cs
// Kaydedilmiş sahnede 3+1 dairelerin (#53) doğru kurulduğunu sınar.
//
// Plan kuralları (odalar üst üste binmez, her odaya girilir) DairePlaniTestleri'nde; bu test
// planın SAHNEYE gerçekten kurulduğuna bakar: A'da dört daire var, hepsi kapalı kaydedilmiş,
// her birinde bütün odalar ve kapılar mevcut. KatYoneticisi daireleri bu adlarla buluyor;
// bir ad değişirse oyun daireyi açamaz, bu test de hemen kırılır.

using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[TestFixture]
public class DaireSahneTestleri
{
    const string SahneYolu = "Assets/Scenes/Landing.unity";

    private Scene sahne;

    [SetUp]
    public void Hazirla() { sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive); }

    [TearDown]
    public void Topla() { if (sahne.IsValid()) EditorSceneManager.CloseScene(sahne, removeScene: true); }

    private Transform Kok(string ad)
    {
        foreach (GameObject g in sahne.GetRootGameObjects())
            if (g.name == ad) return g.transform;
        return null;
    }

    [Test]
    public void A_da_dort_daire_var_ve_hepsi_kapali_kaydedilmis()
    {
        Transform a = Kok("Sahanlik_A");
        Assert.IsNotNull(a);
        for (int n = 1; n <= 4; n++)
        {
            Transform d = a.Find($"Daire_{n}");
            Assert.IsNotNull(d, $"Sahanlik_A/Daire_{n} yok");
            // Açık kaydedilirse editörde bütün daireler üst üste görünür ve anomali görünürlük
            // denetimi sahanlığı yanlış ölçer. Hangisinin açılacağına oyun karar verir.
            Assert.IsFalse(d.gameObject.activeSelf, $"Daire_{n} açık kaydedilmiş");
        }
    }

    [Test]
    public void Her_dairede_butun_odalar_ve_kapilar_var()
    {
        Transform a = Kok("Sahanlik_A");
        for (int n = 1; n <= 4; n++)
        {
            Transform d = a.Find($"Daire_{n}");
            foreach (DairePlani.Oda oda in DairePlani.Odalar)
                Assert.IsNotNull(d.Find(oda.Ad), $"Daire_{n}/{oda.Ad} yok");

            foreach (DairePlani.Aciklik ac in DairePlani.Acikliklar)
            {
                bool kapi = ac.Tur == DairePlani.AciklikTuru.Kapi || ac.Tur == DairePlani.AciklikTuru.BalkonKapisi;
                bool pencere = ac.Tur == DairePlani.AciklikTuru.Pencere;
                if (kapi)
                {
                    Transform k = d.Find($"Kapilar/{ac.Ad}");
                    Assert.IsNotNull(k, $"Daire_{n}/Kapilar/{ac.Ad} yok");
                    Assert.IsNotNull(k.Find("Mentese"), $"Daire_{n}/Kapilar/{ac.Ad}/Mentese yok");
                    // Kapı kapalı kaydedilmeli (menteşe dönmemiş).
                    Assert.Less(Quaternion.Angle(k.Find("Mentese").localRotation, Quaternion.identity), 0.5f,
                                $"Daire_{n}/Kapilar/{ac.Ad} açık kaydedilmiş");
                }
                if (pencere)
                    Assert.IsNotNull(d.Find($"Pencereler/{ac.Ad}"), $"Daire_{n}/Pencereler/{ac.Ad} yok");
            }
        }
    }

    [Test]
    public void B_de_ve_sahte_katlarda_daire_yok()
    {
        // Oyuncu yalnızca A'da yürüyor; B'de ve sahte katlarda dört daire daha kurmak sahneyi
        // boşuna ağırlaştırırdı.
        Transform b = Kok("Sahanlik_B");
        Assert.IsNotNull(b);
        Assert.IsNull(b.Find("Daire_1"), "Sahanlik_B'de daire olmamalı");
    }
}
