// KatKapisiSahneTestleri.cs
// Kaydedilmiş sahnedeki kat kapılarının (#52) doğru hâlde kurulduğunu sınar.
//
// ---------- BU TEST NEDEN VAR ----------
// Oyuncu açılışta zemin kattan 1. kata, 1. katın ALTINDAKİ sahte çekirdeğin
// merdiveninden çıkar. O çekirdeğin de bir kat kapısı var ve oyun onu yönetmez
// (KatYoneticisi yalnızca A ile B çekirdeklerinin kapılarını ayarlar). Kapı kilitli
// kurulursa oyuncu daha ilk merdivende anahtarı olmayan bir kapıya takılır: Şahan'ın
// build testinde tam olarak bu oldu. Oynatma testleri oyuncuyu doğrudan 1. kata
// koyduğu için hatayı göremedi; bu test sahnenin kendisine bakıyor.
//
// Kapının bileşeni (KatKapisi) oyunun varsayılan kodunda duruyor ve test derlemesi onu
// göremiyor (asmdef kuralı). Bu yüzden kapının açık olduğunu kanadın DÖNÜŞÜNDEN okuyoruz:
// kapalı kanat 0°, açık kanat 95° döner.

using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[TestFixture]
public class KatKapisiSahneTestleri
{
    const string SahneYolu = "Assets/Scenes/Landing.unity";

    private Scene sahne;

    [SetUp]
    public void Hazirla()
    {
        // Ek olarak açılıyor: geliştiricinin açık sahnesi kapanmasın.
        sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive);
    }

    [TearDown]
    public void Topla()
    {
        if (sahne.IsValid()) EditorSceneManager.CloseScene(sahne, removeScene: true);
    }

    // Sahnenin kök nesnelerinden verilen yoldaki nesneyi bulur (kapalı olsa bile).
    private Transform Bul(string kokAdi, string yol)
    {
        foreach (GameObject kok in sahne.GetRootGameObjects())
            if (kok.name == kokAdi) return kok.transform.Find(yol);
        return null;
    }

    // Kanat menteşesinde ne kadar dönmüş (derece, 0..180).
    private static float KanatAcisi(Transform kapi)
    {
        Transform kanat = kapi.Find("Kanat");
        Assert.IsNotNull(kanat, $"{kapi.name} altında 'Kanat' yok.");
        return Quaternion.Angle(kanat.localRotation, Quaternion.identity);
    }

    [Test]
    public void Alt_sahte_katlarin_kapilari_acik()
    {
        // Kaç alt sahte kat olduğunu kurucudan değil sahneden sayıyoruz: sayı değişirse
        // test de kendiliğinden hepsini kapsar.
        Transform alt = Bul("Sahte_Katlar", "Alt");
        Assert.IsNotNull(alt, "Sahte_Katlar/Alt bulunamadı; graybox yeniden kurulmalı.");

        int sayilan = 0;
        foreach (Transform cocuk in alt)
        {
            if (!cocuk.name.StartsWith("Cekirdek_Alt")) continue;
            Transform kapi = cocuk.Find("Kat_Kapisi");
            Assert.IsNotNull(kapi, $"{cocuk.name} altında Kat_Kapisi yok.");
            Assert.Greater(KanatAcisi(kapi), 80f,
                $"{cocuk.name}/Kat_Kapisi kapalı kurulmuş: zemin kattan 1. kata çıkış kilitlenir.");
            sayilan++;
        }
        Assert.Greater(sayilan, 0, "Hiç alt sahte çekirdek bulunamadı.");
    }

    [Test]
    public void Oyunun_yonettigi_kapilar_kapali_kurulur()
    {
        // A ile B'nin kapılarını oyun her kat için ayarlıyor; sahnede kapalı durmaları
        // gerekir, yoksa ilk karede bir an açık görünüp kapanırlar.
        // Yollar KatYoneticisi.KapilariBul'un aradığı yollarla aynı: biri değişirse bu test
        // kırılır ve oyunun kapıyı artık bulamayacağı hemen görülür.
        Transform kapiA = Bul("Cekirdek_A", "Kat_Kapisi");
        Transform kapiB = Bul("Sahte_Katlar", "Ust/Cekirdek_B/Kat_Kapisi");
        Assert.IsNotNull(kapiA, "Cekirdek_A/Kat_Kapisi bulunamadı.");
        Assert.IsNotNull(kapiB, "Sahte_Katlar/Ust/Cekirdek_B/Kat_Kapisi bulunamadı.");
        Assert.Less(KanatAcisi(kapiA), 1f, "Cekirdek_A/Kat_Kapisi açık kurulmuş.");
        Assert.Less(KanatAcisi(kapiB), 1f, "Cekirdek_B/Kat_Kapisi açık kurulmuş.");
    }
}
