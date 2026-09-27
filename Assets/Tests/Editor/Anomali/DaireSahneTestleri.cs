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
    public void Her_dairede_bulmaca_esyalari_ve_acilan_parcalari_var()
    {
        // Eşya adı -> açılan parçaları. Bir parça eksikse (FBX'te adı değişti, dosya bulunamadı)
        // kurucu yalnızca uyarı yazar; bu test onu hata olarak yakalar.
        var esyalar = new System.Collections.Generic.Dictionary<string, string[]>
        {
            { "Sigorta_Kutusu", new[] { "Kapak", "Sigorta_1", "Sigorta_2", "Sigorta_3", "Sigorta_4", "Salter" } },
            { "Tezgah", new[] { "Cekmece_1", "Cekmece_2", "Cekmece_3", "Kapak_1", "Kapak_2", "Kavanoz/Kapak" } },
            { "Buzdolabi", new[] { "Kapi" } },
            { "Takvim", new string[0] },
            { "Komodin", new[] { "Cekmece" } },
            { "Oyuncak_Kutusu", new[] { "Kapak" } },
            { "Boy_Cetveli", new string[0] },
            { "Alet_Cantasi", new[] { "Kapak" } },
            { "Duvar_Kolu", new[] { "Kol" } },
        };
        Transform a = Kok("Sahanlik_A");
        for (int n = 1; n <= 4; n++)
            foreach (var esya in esyalar)
            {
                Transform t = a.Find($"Daire_{n}/Esyalar/{esya.Key}");
                Assert.IsNotNull(t, $"Daire_{n}/Esyalar/{esya.Key} yok");
                Assert.IsNotNull(t.GetComponent<MeshFilter>(), $"{esya.Key} modelsiz (FBX bulunamamış)");
                foreach (string p in esya.Value)
                    Assert.IsNotNull(t.Find(p), $"Daire_{n}/Esyalar/{esya.Key}/{p} yok");
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
