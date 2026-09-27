// AnomaliButunlukTestleri.cs
// Kat dosyalarının HER senaryolu sahanlık kartını (Assets/Zincirler, #67) gerçek sahanlığa
// uygulayıp geri alır ve sahanlığın birebir eski hâline döndüğünü doğrular.
//
// #59: rastgele anomali kartları (Resources/Anomaliler) eski kuralla birlikte kalktı; aynı
// denetim artık kat dosyalarının kullandığı senaryolu kartlara uygulanıyor.
//
// ---------- BU TEST NEDEN VAR ----------
// Sahnede tek bir sahanlık var ve her katta tekrar kullanılıyor. Bir kart
// yaptığı değişikliği geri alamazsa o anomali KALICI olur: sonraki katta da
// görünür ve oyun sessizce bozulur.
//
// Bu tür bir hata elle test ederken aylarca fark edilmeyebilir, çünkü ancak
// belirli bir kat geldiğinde ve ondan sonraki katta ortaya çıkar. Bu test her kartı
// saniyeler içinde sınıyor ve yeni eklenen her kart otomatik olarak kapsama giriyor.
//
// Ayrıca kartlardaki YOL HATALARINI da yakalıyor. Bir kartta hedef yolu yanlış
// yazılırsa anomali hiç görünmez; oyunda bunu fark etmek çok zordur.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[TestFixture]
public class AnomaliButunlukTestleri
{
    const string SahneYolu = "Assets/Scenes/Landing.unity";
    const string SahanlikAdi = "Sahanlik_A";

    private Scene sahne;
    private Transform sahanlik;

    // [SetUp] her testten önce çalışır.
    // Sahneyi EK OLARAK açıyoruz (Additive): geliştiricinin o an açık olan
    // sahnesi kapanmasın, kaydedilmemiş işi kaybolmasın.
    [SetUp]
    public void Hazirla()
    {
        sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive);

        sahanlik = null;
        foreach (GameObject kok in sahne.GetRootGameObjects())
        {
            if (kok.name == SahanlikAdi) { sahanlik = kok.transform; break; }
        }

        Assert.IsNotNull(sahanlik,
            $"'{SahneYolu}' içinde '{SahanlikAdi}' bulunamadı. " +
            "Sahne Huzur → Graybox Sahanlığı Yeniden Kur ile kurulmuş olmalı.");
    }

    // [TearDown] her testten sonra çalışır. Açtığımız sahneyi kapatıyoruz,
    // yoksa testler editörde çöp bırakır.
    [TearDown]
    public void Topla()
    {
        if (sahne.IsValid())
        {
            EditorSceneManager.CloseScene(sahne, removeScene: true);
        }
    }

    // ---------- FOTOĞRAF ----------
    // Sahanlığın o anki hâlini tek bir metne çevirir. İki fotoğrafı
    // karşılaştırmak, yüzlerce değeri tek tek karşılaştırmakla aynı şeydir
    // ama çok daha kısa yazılır ve hiçbir alanı atlamaz.
    private string Fotograf()
    {
        var sb = new System.Text.StringBuilder();

        // true: kapatılmış (gizlenmiş) nesneler de dahil olsun.
        // Bu şart: "Gorunurluk" etkisi nesneyi gizliyor, gizlenen nesne
        // listeye girmezse gizlemenin geri alınıp alınmadığını göremeyiz.
        foreach (Transform t in sahanlik.GetComponentsInChildren<Transform>(true))
        {
            sb.Append(t.name).Append('|')
              .Append(t.gameObject.activeSelf).Append('|')
              .Append(t.localPosition.ToString("F4")).Append('|')
              .Append(t.localRotation.ToString("F4")).Append('|')
              .Append(t.localScale.ToString("F4")).Append(';');

            Light isik = t.GetComponent<Light>();
            if (isik != null)
            {
                sb.Append("ISIK:").Append(isik.color.ToString("F4"))
                  .Append(isik.intensity.ToString("F4"))
                  .Append(isik.enabled).Append(';');
            }

            TextMesh yazi = t.GetComponent<TextMesh>();
            if (yazi != null) sb.Append("YAZI:").Append(yazi.text).Append(';');

            // Renk etkisi malzemeyi değil, çiziciye takılan geçici bir blok
            // kullanıyor. Geri alınıp alınmadığını görmek için onu da yazıyoruz.
            Renderer cizici = t.GetComponent<Renderer>();
            if (cizici != null)
            {
                var blok = new MaterialPropertyBlock();
                cizici.GetPropertyBlock(blok);
                sb.Append("BLOK:").Append(blok.isEmpty ? "bos" : "dolu").Append(';');
            }
        }

        return sb.ToString();
    }

    // AssetDatabase.FindAssets: bir klasördeki belirli türden bütün dosyaları bulur (yalnızca
    // Editor'de çalışır; testler de Editor'de koşuyor). "t:AnomaliKarti" = bu türden olanlar.
    private static AnomaliKarti[] TumKartlar()
    {
        var liste = new List<AnomaliKarti>();
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:AnomaliKarti", new[] { "Assets/Zincirler" }))
            liste.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<AnomaliKarti>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)));
        return liste.ToArray();
    }

    // ---------- ASIL TEST ----------

    [Test]
    public void Her_kart_uygulanip_geri_alininca_sahanlik_ilk_haline_doner()
    {
        AnomaliKarti[] kartlar = TumKartlar();
        Assert.Greater(kartlar.Length, 0, "Assets/Zincirler klasöründe hiç sahanlık kartı yok.");

        string ilkHal = Fotograf();
        var uygulayici = new AnomaliUygulayici();

        foreach (AnomaliKarti kart in kartlar)
        {
            uygulayici.Uygula(kart, sahanlik);
            uygulayici.Ilerlet(float.MaxValue);   // zamanlı etkiler de denensin (#13)
            uygulayici.GeriAl();

            string sonHal = Fotograf();

            Assert.AreEqual(ilkHal, sonHal,
                $"'{kart.Kimlik}' kartı geri alındıktan sonra sahanlık eski hâline dönmedi. " +
                "Bu kart uygulandığı katın ardından KALICI bir iz bırakır.");
        }
    }

    [Test]
    public void Her_kart_sahanlikta_gercekten_bir_sey_degistirir()
    {
        AnomaliKarti[] kartlar = TumKartlar();
        var uygulayici = new AnomaliUygulayici();

        foreach (AnomaliKarti kart in kartlar)
        {
            string once = Fotograf();

            uygulayici.Uygula(kart, sahanlik);
            uygulayici.Ilerlet(float.MaxValue);   // zamanlı etkiler de denensin (#13)
            string uygulanmis = Fotograf();

            uygulayici.GeriAl();

            // Hiçbir şey değişmediyse kart ya boştur ya da hedef yolu yanlıştır.
            // İkisi de sessiz hatadır: kat dosyası bir iz bekler, ortada iz yoktur.
            Assert.AreNotEqual(once, uygulanmis,
                $"'{kart.Kimlik}' kartı sahanlıkta hiçbir şey değiştirmedi. " +
                "Hedef yolu yanlış yazılmış olabilir.");
        }
    }

    [Test]
    public void Her_kartin_hedef_yollari_sahanlikta_bulunuyor()
    {
        AnomaliKarti[] kartlar = TumKartlar();
        var hatalar = new List<string>();

        foreach (AnomaliKarti kart in kartlar)
        {
            foreach (AnomaliEtkisi etki in kart.Etkiler)
            {
                if (sahanlik.Find(etki.hedefYol) == null)
                {
                    hatalar.Add($"'{kart.Kimlik}' → '{etki.hedefYol}'");
                }
            }
        }

        Assert.IsEmpty(hatalar,
            "Şu kartların hedef yolları sahanlıkta yok: " + string.Join(", ", hatalar));
    }

    // Poşet sayısı her katta rastgele (0-3). Bir kart poşeti gizlese o poşet zaten plan
    // gereği yok olabilir; kart hiçbir şey değiştirmemiş olur. Bu yüzden poşetler yasak hedef.
    [Test]
    public void Hicbir_kart_rastgele_poset_sayisina_dokunmaz()
    {
        var hatalar = new List<string>();

        foreach (AnomaliKarti kart in TumKartlar())
        {
            foreach (AnomaliEtkisi etki in kart.Etkiler)
            {
                if (etki.hedefYol.Contains("Copler")) hatalar.Add($"'{kart.Kimlik}' → '{etki.hedefYol}'");
            }
        }

        Assert.IsEmpty(hatalar,
            "Şu kartlar çöp poşetlerini hedef alıyor (poşet sayısı rastgele olduğu için yasak): " +
            string.Join(", ", hatalar));
    }

    // Ayakkabılar da rastgele (#44): raftaki çift sayısı (2-3 + rafa konanlar) ve kapı
    // önündeki dağınık çiftler (0-2) her katta farklı. Poşetlerle aynı sebeple yasak hedef.
    // Rafın KENDİSİ (Kapi_N/Ayakkabilik) serbest: her katta aynı yerde duruyor.
    [Test]
    public void Hicbir_kart_rastgele_ayakkabilara_dokunmaz()
    {
        var hatalar = new List<string>();
        foreach (AnomaliKarti kart in TumKartlar())
            foreach (AnomaliEtkisi etki in kart.Etkiler)
                if (etki.hedefYol.Contains("Ayakkabilar") || etki.hedefYol.Contains("Ayakkabilik/Raf_"))
                    hatalar.Add($"'{kart.Kimlik}' → '{etki.hedefYol}'");
        Assert.IsEmpty(hatalar, "Şu kartlar rastgele ayakkabıları hedef alıyor (yasak): " + string.Join(", ", hatalar));
    }

    // Kartın yazdığı metin iki dilli tablodan geçmez (kart sadece bir yazı taşır).
    // Harf içeren bir metin ("GİRME" gibi) İngilizce oynayana Türkçe kalırdı. Bugün
    // kartlar yalnızca kapı numarası yazıyor; kelime yazan bir kart gerekirse önce
    // kartın metni bir tablo anahtarına bağlanacak şekilde sistem genişletilmeli (#15).
    [Test]
    public void Hicbir_kart_harf_iceren_metin_yazmaz()
    {
        var hatalar = new List<string>();

        foreach (AnomaliKarti kart in TumKartlar())
        {
            foreach (AnomaliEtkisi etki in kart.Etkiler)
            {
                if (etki.tur != EtkiTuru.Metin || string.IsNullOrEmpty(etki.metin)) continue;

                // char.IsLetter: harf mi (Türkçe harfler dahil); rakam ve işaretler geçer.
                foreach (char c in etki.metin)
                {
                    if (char.IsLetter(c)) { hatalar.Add($"'{kart.Kimlik}' → \"{etki.metin}\""); break; }
                }
            }
        }

        Assert.IsEmpty(hatalar, "Şu kartlar çevrilemeyen bir kelime yazıyor: " + string.Join(", ", hatalar));
    }

    // Normal katın ses dokusu (#14). Biri sahanlığı yeniden düzenlerken bir katmanı silerse
    // ya da döngüsünü bozarsa bina sessizce susardı.
    [Test]
    public void Ortam_ses_katmanlari_sahanlikta_ve_dongude()
    {
        // Bebek artık döngü değil, bir kat olayı (#35); aşağıdaki testte.
        // Boru katmanı #74'te kalktı (Şahan'ın ses kararları).
        foreach (string yol in new[] { "Ses_Ugultu", "Kapi_2/Ses_TV" })
        {
            Transform t = sahanlik.Find(yol);
            Assert.IsNotNull(t, $"'{yol}' ses katmanı sahanlıkta yok.");

            AudioSource kaynak = t.GetComponent<AudioSource>();
            Assert.IsNotNull(kaynak, $"'{yol}' üzerinde AudioSource yok.");
            Assert.IsNotNull(kaynak.clip, $"'{yol}' için ses dosyası bağlanmamış.");
            Assert.IsTrue(kaynak.loop, $"'{yol}' döngüde çalmıyor; bir süre sonra susar.");
            // OrtamSesi oyunun varsayılan derlemesinde (Assembly-CSharp); test derlemeleri
            // oraya başvuramaz. Bileşeni tipiyle değil ADIYLA arıyoruz.
            Assert.IsNotNull(t.GetComponent("OrtamSesi"),
                $"'{yol}' saate kilitli değil (OrtamSesi yok); ışınlanmada ses atlar.");
        }
    }

    // Kat olayı kaynakları (#35). KatOlayCalici bunları bu yollarla buluyor; biri
    // silinir ya da yeniden adlandırılırsa o olay sessizce hiç çalmaz. Döngüde ya da
    // saate kilitli OLMAMALILAR: öyle olsalar her katta sürekli çalarlardı (Şahan'ın
    // şikâyeti tam buydu). Yollar KatOlayCalici.KaynakYollari ile aynı sırada.
    [Test]
    public void Kat_olayi_kaynaklari_sahanlikta_ve_tek_seferlik()
    {
        // #74: yalnızca bebek kaldı; "Sessiz" olayın kaynağı yok.
        string[] yollar = { "Kapi_4/Ses_Bebek" };
        Assert.AreEqual(KatOlayPlani.OlaySayisi - 1, yollar.Length, "Sessiz dışındaki her olay çeşidi için bir kaynak olmalı.");

        foreach (string yol in yollar)
        {
            Transform t = sahanlik.Find(yol);
            Assert.IsNotNull(t, $"'{yol}' olay kaynağı sahanlıkta yok.");
            AudioSource kaynak = t.GetComponent<AudioSource>();
            Assert.IsNotNull(kaynak, $"'{yol}' üzerinde AudioSource yok.");
            Assert.IsNotNull(kaynak.clip, $"'{yol}' için ses dosyası bağlanmamış.");
            Assert.IsFalse(kaynak.loop, $"'{yol}' döngüde; her katta sürekli çalar.");
            Assert.IsFalse(kaynak.playOnAwake, $"'{yol}' sahne açılınca kendiliğinden çalıyor.");
            Assert.IsNull(t.GetComponent("OrtamSesi"), $"'{yol}' saate kilitli; kendiliğinden çalar.");
        }
    }

    // Arka müzik (#36) ve dış sesler (#37). Ses dosyaları bileşenlere BAĞLI olmalı: adıyla
    // aranan bir dosya build'e girmez (#26 kuralı), bağlı değilse build'de sessizce çalmaz.
    [Test]
    public void Muzik_ve_dis_sesler_sahnede_ve_bagli()
    {
        GameObject muzik = null, dis = null;
        foreach (GameObject kok in sahne.GetRootGameObjects())
        {
            if (kok.name == "Arka_Muzik") muzik = kok;
            if (kok.name == "Dis_Sesler") dis = kok;
        }
        Assert.IsNotNull(muzik, "Sahnede Arka_Muzik yok.");
        AudioSource m = muzik.GetComponent<AudioSource>();
        Assert.IsNotNull(m.clip, "Arka müziğin ses dosyası bağlanmamış.");
        Assert.IsTrue(m.loop, "Arka müzik döngüde değil.");
        Assert.AreEqual(0f, m.spatialBlend, "Arka müzik 2B olmalı.");
        Assert.LessOrEqual(m.volume, 0.3f, "Arka müzik binanın seslerini bastıracak kadar yüksek.");
        Assert.IsNotNull(muzik.GetComponent("ArkaMuzik"));

        Assert.IsNotNull(dis, "Sahnede Dis_Sesler yok.");
        Component bilesen = dis.GetComponent("DisSesleri");
        Assert.IsNotNull(bilesen);
        var seri = new UnityEditor.SerializedObject(bilesen);
        Assert.Greater(seri.FindProperty("silah").arraySize + seri.FindProperty("kopek").arraySize, 0,
                       "Dış seslere hiç ses dosyası bağlanmamış.");
        Assert.IsFalse(dis.GetComponent<AudioSource>().loop, "Dış ses kaynağı döngüde; sürekli çalar.");
    }

    [Test]
    public void Hicbir_kart_bos_degildir()
    {
        foreach (AnomaliKarti kart in TumKartlar())
        {
            Assert.IsTrue(kart.Gecerli, $"'{kart.Kimlik}' kartının hiç etkisi yok.");
        }
    }

    [Test]
    public void Kart_kimlikleri_benzersizdir()
    {
        var gorulen = new HashSet<string>();

        foreach (AnomaliKarti kart in TumKartlar())
        {
            // Add, daha önce eklenmişse false döner.
            Assert.IsTrue(gorulen.Add(kart.Kimlik),
                $"'{kart.Kimlik}' kimliği birden fazla kartta kullanılmış. " +
                "Kimlikler kayıt ve hata ayıklamada karışır.");
        }
    }

    // Bütün kartlar arka arkaya uygulanırsa bile sahanlık temiz kalmalı.
    // Oyunda kat kat ilerlerken tam olarak bu oluyor.
    [Test]
    public void Kartlar_arka_arkaya_uygulaninca_birikme_olmaz()
    {
        AnomaliKarti[] kartlar = TumKartlar();
        string ilkHal = Fotograf();

        var uygulayici = new AnomaliUygulayici();

        // Üç tur boyunca bütün kartları sırayla uygula; aralarda geri alma YOK.
        // Uygulayıcı her Uygula çağrısında kendisi temizlemeli.
        for (int tur = 0; tur < 3; tur++)
        {
            foreach (AnomaliKarti kart in kartlar)
            {
                uygulayici.Uygula(kart, sahanlik);
                uygulayici.Ilerlet(float.MaxValue);   // zamanlı etkiler de denensin (#13)
            }
        }

        uygulayici.GeriAl();

        Assert.AreEqual(ilkHal, Fotograf(),
            "Kartlar arka arkaya uygulanınca sahanlıkta artık kaldı.");
    }
}
