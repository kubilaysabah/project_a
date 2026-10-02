// GrayboxKurucu.cs
// Huzur Apartmanı'nın merdiven boşluğunu kodla kuran editör aracı.
// Neden kodla? Çünkü ölçüler tek yerde durur; bir basamağın yüksekliğini değiştirmek
// istediğimizde yüzlerce nesneyi elle taşımak yerine tek sayıyı değiştirip yeniden kuruyoruz.
// Bu dosya OYUNDA çalışmaz; sadece Unity editöründe, menüden tetiklenir.
//
// ---------- NE KURULUYOR (2026-09-23, #23) ----------
// Yarım dönüşlü kare bir merdiven ve her katta dört daireli bir sahanlık.
// Katlar TAM OLARAK üst üste durur; döngü hilesi bu yüzden sadece dikeydir.
//
//   Sahanlik_A   (y = 0)  oyuncunun bulunduğu kat. Anomaliler burada.
//   Cekirdek_A   (0 → 3)  A'dan B'ye çıkan iki kollu merdiven.
//   Sahanlik_B   (y = 3)  bir üst kat. Oyuncu buraya ayak bastığı an A'ya ışınlanır.
//   Sahte_Katlar          A'nın altında ve B'nin üstünde birkaç kat daha.
//                         Oyuncu merdiven gözünden bakınca bina devam ediyor
//                         görünsün diye var; oyuncu bunların çoğuna hiç basmaz.
//   CatiBolumu   (y = 3)  8. katta B'nin yerine geçen çatı kapısı. Başta kapalı.

using UnityEngine;        // GameObject, Vector3, Transform gibi temel Unity tipleri buradan gelir.
using UnityEditor;        // MenuItem ve AssetDatabase gibi SADECE editörde çalışan araçlar buradan gelir.

// "using static": bir sınıfın içindeki değerleri, sınıf adını her seferinde
// yazmadan kullanmamızı sağlar. Ölçülerin kendisi MekanOlculeri.cs'de; burada kopyası YOK.
using static MekanOlculeri;

// "static" sınıf: içinden nesne üretilemez, sadece fonksiyonları çağrılır.
// "partial": sınıfın birden fazla DOSYAYA bölünebileceğini söyler. Derleyici bütün parçaları
// birleştirip tek sınıf yapar. Bu dosya çok büyüdüğü için dairenin kurucusu (#53) ayrı bir
// dosyada, GrayboxKurucu_Daire.cs'de duruyor; ama aynı sınıfın parçası olduğu için buradaki
// Kutu, Model ve malzeme yardımcılarını doğrudan kullanabiliyor.
public static partial class GrayboxKurucu
{
    // Merdiven gözünden kaç kat aşağısı ve yukarısı kurulsun.
    // Daha fazlası sisin içinde zaten görünmüyor; sadece sahneyi ağırlaştırır.
    const int AltSahteKat = 3;
    const int UstSahteKat = 2;

    // ---------- MENÜ GİRİŞİ ----------
    // [MenuItem] bir "öznitelik"tir: fonksiyonun üstüne yapıştırılan etikettir.
    // Unity bu etiketi görünce menü çubuğuna bir satır ekler. Tıklayınca fonksiyon çalışır.
    [MenuItem("Huzur/Graybox Sahanlığı Yeniden Kur")]
    public static void YenidenKur()
    {
        Temizle();   // önce eski kopyaları sil, üst üste binmesin
        Kur();       // sonra sıfırdan kur
    }

    // ---------- TEMİZLİK ----------
    // Aracı ikinci kez çalıştırdığımızda sahnede iki kopya olmasın diye önce siliyoruz.
    static void Temizle()
    {
        // KURAL: bu liste, aracın BUGÜNE KADAR ürettiği her kök ismi içerir.
        // Eski bir isim listeden çıkarılırsa, eski sahnelerde o nesne silinmeden
        // kalır ve yeni geometrinin içinde görünmez bir engel olarak durur.
        // ("Kapak_Ust" bir kez tam olarak böyle çatı kapısının önünü kesmişti.)
        string[] adlar =
        {
            // Bugünkü isimler
            "Sahanlik_A", "Sahanlik_B", "Cekirdek_A", "Sahte_Katlar", "CatiBolumu", "Zemin_Kat", "Final", "Dokuzuncu_Kat",

            // Düz merdivenli eski yerleşimin isimleri (#23 öncesi)
            "Merdiven_Yukari", "Merdiven_Yukari2", "Merdiven_Asagi", "Kapak_Alt", "Kapak_Ust"
        };

        foreach (string ad in adlar)
        {
            // Kapalı (inactive) nesneleri GameObject.Find bulamaz. Çatı bölümü
            // başta kapalı durduğu için sahnenin kök nesnelerini tek tek geziyoruz.
            foreach (GameObject kok in UnityEngine.SceneManagement.SceneManager
                                          .GetActiveScene().GetRootGameObjects())
            {
                if (kok.name == ad) Object.DestroyImmediate(kok);
            }
        }
    }

    // ---------- ANA KURULUM ----------
    static void Kur()
    {
        // ---- GERÇEK KATLAR ----
        // Oyuncu yalnızca bu ikisinin ve aradaki merdivenin içinde dolaşır.
        GameObject sahanlikA = SahanlikKur("Sahanlik_A", tam: true);
        sahanlikA.transform.position = Vector3.zero;

        // 3+1 daireler (#53): yalnızca A'da, çünkü oyuncu yalnızca A'da yürüyor. Dördü de
        // kapalı kurulur; hangisinin açılacağına oyun sırasında KatYoneticisi karar verir.
        DaireleriKur(sahanlikA.transform);

        GameObject cekirdekA = CekirdekKur("Cekirdek_A");
        cekirdekA.transform.position = Vector3.zero;

        GameObject sahanlikB = SahanlikKur("Sahanlik_B", tam: true);
        sahanlikB.transform.position = new Vector3(0f, DikeyOfset, 0f);

        // ---- SAHTE KATLAR ----
        // Alt ve üst diye iki gruba ayrılıyor, çünkü 8. katta ÜST grup tamamen
        // kapatılıyor: çatının üstünde başka kat olmamalı.
        GameObject sahte = new GameObject("Sahte_Katlar");

        GameObject alt = new GameObject("Alt");
        alt.transform.SetParent(sahte.transform, false);

        for (int i = 1; i <= AltSahteKat; i++)
        {
            // i. alt katın sahanlığı ve o sahanlıktan bir üste çıkan merdiven.
            GameObject s = SahanlikKur($"Sahanlik_Alt{i}", tam: false);
            s.transform.SetParent(alt.transform, false);
            s.transform.localPosition = new Vector3(0f, -i * DikeyOfset, 0f);

            GameObject c = CekirdekKur($"Cekirdek_Alt{i}");
            c.transform.SetParent(alt.transform, false);
            c.transform.localPosition = new Vector3(0f, -i * DikeyOfset, 0f);

            // Alt katlar oyuncunun geride bıraktığı katlar: kat kapıları AÇIK kurulur (#52).
            // Şart da: açılışta oyuncu zemin kattan 1. kata Cekirdek_Alt1'in kolundan çıkıyor.
            // Bu kapıyı KatYoneticisi yönetmiyor (yalnızca A ile B'yi yönetir); kilitli
            // kurulursa oyuncu daha ilk merdivende, anahtarı olmayan bir kapıya takılır.
            Transform altKapi = c.transform.Find("Kat_Kapisi");
            KatKapisi altKapiBileseni = altKapi != null ? altKapi.GetComponent<KatKapisi>() : null;
            if (altKapiBileseni != null) altKapiBileseni.Ayarla(0, true);   // 0: hiçbir katın anahtarı açmaz
        }

        // ZEMİN KAT (#16, açılış): oyun burada, kapıcı dairesinde başlar. Normal bir
        // sahanlık olarak kurulup zemin kata çevriliyor ve 1. altındaki sahte katın
        // TAM YERİNDE duruyor. Başta KAPALI; KatYoneticisi oyun başlarken sahte katı
        // gizleyip bunu açıyor, oyuncu 1. kata çıkınca eski düzene dönüyor.
        GameObject zemin = SahanlikKur("Zemin_Kat", tam: false);
        zemin.transform.position = new Vector3(0f, ZeminKot, 0f);
        ZeminKatiDonat(zemin.transform);
        zemin.SetActive(false);

        // Kuyunun dibi: merdiven gözünden aşağı bakınca sonsuz bir karanlık.
        // Siyah malzeme ve sis birlikte dibi görünmez kılıyor.
        GameObject dip = Kutu("Kuyu_Dibi", alt.transform,
            new Vector3(0f, -AltSahteKat * DikeyOfset - DuvarKalin - 0.1f, (CekirdekDerin - SahanlikDerin) / 2f),
            new Vector3(SahanlikGen + 1f, 0.2f, SahanlikDerin + CekirdekDerin + 1f));
        dip.GetComponent<MeshRenderer>().sharedMaterial = KaranlikMalzeme();

        GameObject ust = new GameObject("Ust");
        ust.transform.SetParent(sahte.transform, false);

        // B'nin üstündeki merdiven. Oyuncu B'ye hiç basmadığı için bu merdivene de
        // basmaz, ama merdiven gözünden yukarı bakınca görünür.
        GameObject cB = CekirdekKur("Cekirdek_B");
        cB.transform.SetParent(ust.transform, false);
        cB.transform.localPosition = new Vector3(0f, DikeyOfset, 0f);

        for (int i = 1; i <= UstSahteKat; i++)
        {
            float y = (i + 1) * DikeyOfset;   // B'nin 1, 2 ... kat üstü

            GameObject s = SahanlikKur($"Sahanlik_Ust{i}", tam: false);
            s.transform.SetParent(ust.transform, false);
            s.transform.localPosition = new Vector3(0f, y, 0f);

            GameObject c = CekirdekKur($"Cekirdek_Ust{i}");
            c.transform.SetParent(ust.transform, false);
            c.transform.localPosition = new Vector3(0f, y, 0f);
        }

        // En tepe: son sahte katın tavanı ve merdiven boşluğunun kapağı.
        GameObject tepe = Kutu("Kuyu_Tepesi", ust.transform,
            new Vector3(0f, (UstSahteKat + 2) * DikeyOfset - DuvarKalin / 2f, (CekirdekDerin - SahanlikDerin) / 2f),
            new Vector3(SahanlikGen + 1f, DuvarKalin, SahanlikDerin + CekirdekDerin + 1f));
        tepe.GetComponent<MeshRenderer>().sharedMaterial = KaranlikMalzeme();

        // ---- ÇATI ----
        // 8. katta B'nin yerini alır. Başta kapalı; KatYoneticisi açar.
        GameObject cati = CatiBolumuKur();
        cati.transform.position = new Vector3(0f, DikeyOfset, 0f);
        cati.SetActive(false);

        // ---- DIŞ CEPHE ve FİNAL (#16) ----
        DisCepheKur();

        // ---- DOKUZUNCU KAT (#108) ----
        // Asansörün çıkardığı 1974 katı; dış cephe gibi uzakta (x = -400), GrayboxKurucu_Dokuz.cs kurar.
        DokuzuncuKatKur();

        // ---- SİS ----
        // Sis, uzaktaki şeyleri yavaş yavaş bir renge boğar. Burada neredeyse siyah.
        // Merdiven gözünden bakınca kurduğumuz son katın ötesi görünmesin diye şart:
        // sis olmadan kuyunun dibi ve tepesi açıkça seçilir, "sonsuz bina" hissi kaybolur.
        // Başlangıç mesafesi sahanlığın en uzak köşesinden büyük; sahanlık etkilenmez.
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;           // mesafeyle düz artan sis
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.fogStartDistance = 8f;
        RenderSettings.fogEndDistance = 17f;

        // Gece atmosferi (#11): ortam ışığı, ay ışığı, post-processing, kamera.
        // Ayrı bir araçta duruyor ama graybox her kurulduğunda o da kurulur.
        AtmosferKurucu.Kur();

        // Kamerayı göz hizasına al — AMA sadece başıboşsa. Oyuncunun
        // çocuğuysa dokunmayız, yoksa oyuncunun kafasını yerinden oynatırız.
        if (Camera.main != null && Camera.main.transform.parent == null)
        {
            Camera.main.transform.position = new Vector3(OyuncuBaslangicX, 1.65f, OyuncuBaslangicZ);
            Camera.main.transform.rotation = Quaternion.Euler(5f, 180f, 0f);
        }

        Debug.Log($"Graybox kuruldu. Sahanlık {SahanlikGen:0.0} x {SahanlikDerin:0.0} m, " +
                  $"{DaireSayisi} daire; merdiven {KolBasamakSayisi}+{KolBasamakSayisi} basamak x " +
                  $"{BasamakYuksek:0.0000} m, göz {KuyuGen:0.00} m; dikey ofset {DikeyOfset:0.00} m");

        // Düz beyaz kutuları gerçek dokularla kapla (#41). En sonda: bütün kutular kurulmuş olmalı.
        DokuKurucu.Dokula();

        SahneyiKaydet("Graybox");
    }

    // ---------- SAHNEYİ KAYDET ----------
    // Kodla nesne eklemek Unity'de sahneyi kendiliğinden "kaydedilmemiş değişiklik var"
    // (dirty) durumuna GETİRMİYOR. Bu yüzden kurulumdan sonra sahne "temiz" görünüyor
    // ama yeni hâl yalnızca bellekte duruyordu. Kayıtlı sahneyi açan bir şey (anomali
    // bütünlük testleri bunu yapıyor) Unity'ye hiç sormadan bellekteki hâli atıyordu:
    // #16'nın sokak düzenlemesi ve #15'in yazı bileşenleri böyle sessizce kayboldu.
    // Kurucunun ürettiği şey her zaman saklanmak için üretiliyor; o yüzden kaydediyoruz.
    // OyuncuKurucu da aynı fonksiyonu kullanır.
    public static void SahneyiKaydet(string kim)
    {
        UnityEngine.SceneManagement.Scene sahne = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // MarkSceneDirty: "bu sahne değişti" diye işaretler. SaveScene: diske yazar.
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sahne);
        bool tamam = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sahne);

        if (tamam) Debug.Log($"{kim}: sahne kaydedildi ({sahne.path}).");
        else Debug.LogError($"{kim}: sahne KAYDEDİLEMEDİ ({sahne.path}); Ctrl+S ile elle kaydedin.");
    }

    // =====================================================================
    // KAT SAHANLIĞI
    // =====================================================================
    // Yerel orijin: sahanlık döşemesinin üst yüzü (y = 0), merdivene bakan kenarı (z = 0).
    // Sahanlık z = -5 ile 0 arasında uzanır. Merdiven z > 0 tarafındadır.
    //
    // tam = true  -> oyuncunun gördüğü gerçek sahanlık: bütün eşyalar, numaralar, poşetler.
    // tam = false -> sahte kat: duvarlar, kapılar, asansör ve ampul. Eşya yok,
    //                çünkü merdiven gözünden bunlar zaten görünmüyor.
    static GameObject SahanlikKur(string ad, bool tam)
    {
        GameObject kok = new GameObject(ad);
        Transform t = kok.transform;

        float disX = SahanlikGen / 2f + DuvarKalin;    // yan duvarların dış yüzü (3.7)
        float onZ  = -SahanlikDerin;                   // ön duvarın iç yüzü (-5)

        // DÖŞEME. Aynı zamanda bir alt katın TAVANI. Yan ve ön duvarların
        // altına da uzanır ki köşelerde boşluk kalmasın.
        Kutu("Zemin", t,
             new Vector3(0f, -DuvarKalin / 2f, (onZ - DuvarKalin) / 2f),
             new Vector3(2f * disX, DuvarKalin, SahanlikDerin + DuvarKalin));

        // YAN DUVARLAR. Her birinde iki daire kapısının boşluğu var.
        float[] kapiZ = { YakinKapiZ, UzakKapiZ };
        DuvarKapiliKur("Duvar_Sol", t, -1f, kapiZ);
        DuvarKapiliKur("Duvar_Sag", t, +1f, kapiZ);

        // ÖN DUVAR, ortasında asansör boşluğu. Oyuncu merdivenden çıkıp
        // sahanlığa döndüğünde ilk gördüğü şey bu duvar ve arızalı asansör.
        OnDuvarKur(t);

        // ARKA DUVAR PARÇALARI. Arka kenarın ortası merdiven çekirdeğine açık;
        // iki yanında sahanlık çekirdekten geniş olduğu için duvar kalıyor.
        float cekirdekDis = CekirdekGen / 2f + DuvarKalin;   // 2.0
        float parcaGen = disX - cekirdekDis;                 // 1.7
        Kutu("Duvar_Arka_Sol", t,
             new Vector3(-(cekirdekDis + parcaGen / 2f), DuvarYuksek / 2f, DuvarKalin / 2f),
             new Vector3(parcaGen, DuvarYuksek, DuvarKalin));
        Kutu("Duvar_Arka_Sag", t,
             new Vector3(cekirdekDis + parcaGen / 2f, DuvarYuksek / 2f, DuvarKalin / 2f),
             new Vector3(parcaGen, DuvarYuksek, DuvarKalin));

        // DÖRT DAİRE. Numara sırası sahanlığın etrafında dönerek gider:
        // 1 sol-yakın, 2 sol-uzak, 3 sağ-uzak, 4 sağ-yakın.
        // Numaralar kat sayacına bağlıdır: N. katta 4N-3, 4N-2, 4N-1, 4N.
        KapiKur("Kapi_1", t, sagDuvar: false, YakinKapiZ, tam);
        KapiKur("Kapi_2", t, sagDuvar: false, UzakKapiZ,  tam);
        KapiKur("Kapi_3", t, sagDuvar: true,  UzakKapiZ,  tam);
        KapiKur("Kapi_4", t, sagDuvar: true,  YakinKapiZ, tam);

        // ASANSÖR: ön duvarın ortasında, arızalı.
        AsansorKur(t, tam);

        if (tam)
        {
            // ---- DUVAR EŞYALARI ----
            // Anomali sistemi ancak sahanlıkta BOZULACAK ŞEY kadar zengin olabilir.
            // İsimler sabittir; anomali kartları bu isimlerle hedef gösterir.

            // Her eşya için önce Blender modeli denenir (#24); bulunamazsa beyaz kutu konur.
            // Modellerin ön yüzü +Z'ye bakar (FBX aktarımının sonucu, bkz. ortak.py);
            // y ekseni etrafındaki dönüş ön yüzü doğru yöne çevirir.

            // Ayakkabılık: eskiden burada bütün dairelerin ortak kullandığı tek bir ayakkabılık
            // vardı. #44'ten beri her dairenin kendi 5 raflı ayakkabılığı var (KapiKur ->
            // AyakkabilikKur); onu hedefleyen kartlar Kapi_4/Ayakkabilik'e taşındı.

            // Elektrik sayacı: arka duvarın sol parçasında. Pivotu arka yüzünde; 180°
            // dönüşle ön yüzü sahanlığa (-z) bakar. Menteşeli kapağı "Kapak" adlı çocuğu;
            // ileride "sayaç kapağı açık" anomalisi onu döndürecek.
            GameObject sayac = Model("Elektrik_Sayaci", t, "Elektrik_Sayaci", "Elektrik_Sayaci",
                                     new Vector3(-(cekirdekDis + parcaGen / 2f), 1.40f, 0f), 180f, carpisma: true);
            if (sayac != null)
                Model("Kapak", sayac.transform, "Elektrik_Sayaci", "Kapak", Vector3.zero, 0f, carpisma: false);
            else
                Kutu("Elektrik_Sayaci", t,
                     new Vector3(-(cekirdekDis + parcaGen / 2f), 1.55f, -0.05f),
                     new Vector3(0.22f, 0.28f, 0.10f));

            // Yönetici ilanı: ön duvarda, oyuncuya göre asansörün solunda (+x), göz hizasında.
            // Model ahşap çerçeveli bir pano; adı yine "Ilan" çünkü kartlar bu adı hedefliyor.
            if (Model("Ilan", t, "IlanPanosu", null, new Vector3(1.4f, 1.50f, onZ), 0f, carpisma: false) == null)
                Kutu("Ilan", t,
                     new Vector3(1.4f, 1.50f, onZ + 0.01f),
                     new Vector3(0.21f, 0.30f, 0.02f));

            // Kapı önü ek eşyaları (saksı, tüp, gazete, tek ayakkabı).
            KapiEkEsyalari(t);

            // Olmaması gereken beşinci daire kapısı (KÂBUS anomalisi K02). Gizli başlar.
            BesinciKapiKur(t);
        }

        // Sahanlık ampulü. Sahte katlarda da var: merdiven gözünden bakınca
        // alt katların karanlık bir delik gibi değil, ışıklı katlar gibi görünmesi için.
        // #51 (Şahan'ın build testi): kat ampulleri SÖNÜK, zemin kat dahil; ışığı kafa lambası
        // taşır. Sönük ampul yine yerinde durur: "ampul değiştir" görevi (#57) ve ışık kartları
        // onu yakabilir.
        AmpulKur("Ampul", t, new Vector3(0f, DuvarYuksek - 0.30f, onZ / 2f), 8f, LambaSiddeti, yanik: false);

        // Normal katın ses dokusu (#14). Sahte katlarda da var: komşu katların sesi
        // hafifçe duyulur ve ışınlanmada ses ortamı değişmez.
        OrtamSesleriKur(t);

        // Katın anahtarı (#52): yalnızca gerçek sahanlıklarda (A ve B).
        if (tam) KatAnahtariKur(t);

        return kok;
    }

    // ---------- KAPI BOŞLUKLU YAN DUVAR ----------
    // yon: +1 sağ duvar (+x), -1 sol duvar (-x). kapiZ: kapı merkezleri.
    // Duvar, kapı boşlukları arasında kalan dolu parçalardan ve her kapının
    // üstündeki lentodan kurulur. Parçalar arka köşeden (z = 0) ön köşeye (z = -5) sıralanır.
    static void DuvarKapiliKur(string ad, Transform t, float yon, float[] kapiZ)
    {
        float duvarX = yon * (SahanlikGen / 2f + DuvarKalin / 2f);

        // Dolu parçaların sınırları: arka köşe, her kapının iki kenarı, ön köşe.
        // Kapılar z'de azalan sırada verildiği için (yakın -1.5, uzak -3.5) bu sıra doğru.
        float baslangic = 0f;
        for (int i = 0; i <= kapiZ.Length; i++)
        {
            float bitis = (i < kapiZ.Length) ? kapiZ[i] + KapiGen / 2f : -SahanlikDerin;

            Kutu($"{ad}_Parca_{i + 1}", t,
                 new Vector3(duvarX, DuvarYuksek / 2f, (baslangic + bitis) / 2f),
                 new Vector3(DuvarKalin, DuvarYuksek, baslangic - bitis));

            if (i < kapiZ.Length)
            {
                // Lento: kapının üstünü tavana kadar kapatan parça.
                Kutu($"{ad}_Lento_{i + 1}", t,
                     new Vector3(duvarX, (KapiYuk + DuvarYuksek) / 2f, kapiZ[i]),
                     new Vector3(DuvarKalin, DuvarYuksek - KapiYuk, KapiGen));

                baslangic = kapiZ[i] - KapiGen / 2f;   // bir sonraki parça kapının öbür kenarından başlar
            }
        }
    }

    // ---------- ÖN DUVAR ----------
    const float AsansorGen = 1.1f;    // asansör boşluğunun genişliği
    const float AsansorYuk = 2.1f;    // asansör boşluğunun yüksekliği

    static void OnDuvarKur(Transform t)
    {
        float disX = SahanlikGen / 2f + DuvarKalin;
        float z = -SahanlikDerin - DuvarKalin / 2f;
        float parcaGen = disX - AsansorGen / 2f;

        Kutu("Duvar_On_Sol", t,
             new Vector3(-(AsansorGen / 2f + parcaGen / 2f), DuvarYuksek / 2f, z),
             new Vector3(parcaGen, DuvarYuksek, DuvarKalin));
        Kutu("Duvar_On_Sag", t,
             new Vector3(AsansorGen / 2f + parcaGen / 2f, DuvarYuksek / 2f, z),
             new Vector3(parcaGen, DuvarYuksek, DuvarKalin));
        Kutu("Duvar_On_Lento", t,
             new Vector3(0f, (AsansorYuk + DuvarYuksek) / 2f, z),
             new Vector3(AsansorGen, DuvarYuksek - AsansorYuk, DuvarKalin));
    }

    // ---------- ASANSÖR ----------
    // Arızalı bir asansör: kapısı kapalı, üstünde el yazısıyla "ARIZALI" notu.
    // Oyuncuyu merdivene mahkûm eden şeyin kendisi; aynı zamanda ileride
    // anomali hedefi olacak parçalar içeriyor (not kaybolur, kapı aralanır,
    // gösterge bir kat numarası yazar...). O yüzden her parça ayrı ve isimli.
    static void AsansorKur(Transform ebeveyn, bool tam)
    {
        GameObject kok = new GameObject("Asansor");
        kok.transform.SetParent(ebeveyn, false);
        Transform a = kok.transform;

        float yuz = -SahanlikDerin;   // ön duvarın iç yüzü; +z sahanlığa bakar

        // Kapıların arkasında (model olsa da olmasa da) karanlık bir kuyu. Kapı bir gün aralanırsa
        // (anomali) oyuncu hiçliği değil, simsiyah bir asansör boşluğunu görür.
        GameObject kuyu = Kutu("Kuyu", a, new Vector3(0f, AsansorYuk / 2f, yuz - DuvarKalin - 0.6f),
             new Vector3(AsansorGen + 0.2f, AsansorYuk + 0.2f, 0.1f));
        kuyu.GetComponent<MeshRenderer>().sharedMaterial = KaranlikMalzeme();

        // Blender modeli varsa (#24) kasa, kanatlar, gösterge ve düğme modelden gelir.
        // Model parçalarının pivotu boşluğun alt ortası; hepsi aynı noktaya konur.
        GameObject kasa = Model("Kasa", a, "Asansor", "Kasa", new Vector3(0f, 0f, yuz), 0f, carpisma: true);
        bool model = kasa != null;
        if (model)
        {
            // Kasanın kutu biçimli çarpışma alanı kapı boşluğunu da kaplıyordu (kutu, meshin dış
            // sınırlarına oturur): 8. katta kapılar açılınca oyuncu kabine giremiyor, içindeki
            // anahtara nişan alamıyordu (#68). MeshCollider çerçevenin kendi biçimini sarar.
            Collider eski = kasa.GetComponent<Collider>();
            if (eski != null) Object.DestroyImmediate(eski);
            kasa.AddComponent<MeshCollider>().sharedMesh = kasa.GetComponent<MeshFilter>().sharedMesh;
            Model("Kapi_Sol", a, "Asansor", "Kapi_Sol", new Vector3(0f, 0f, yuz), 0f, carpisma: true);
            Model("Kapi_Sag", a, "Asansor", "Kapi_Sag", new Vector3(0f, 0f, yuz), 0f, carpisma: true);
            Model("Gosterge", a, "Asansor", "Gosterge", new Vector3(0f, 0f, yuz), 0f, carpisma: false);
            Model("Cagri_Dugmesi", a, "Asansor", "Cagri_Dugmesi", new Vector3(0f, 0f, yuz), 0f, carpisma: false);
        }
        else
        {
            // Kasa: kapı boşluğunun etrafında duvardan biraz taşan çerçeve.
            Kutu("Kasa_Sol", a, new Vector3(-(AsansorGen / 2f + 0.03f), AsansorYuk / 2f, yuz + 0.02f),
                 new Vector3(0.06f, AsansorYuk, 0.04f));
            Kutu("Kasa_Sag", a, new Vector3(AsansorGen / 2f + 0.03f, AsansorYuk / 2f, yuz + 0.02f),
                 new Vector3(0.06f, AsansorYuk, 0.04f));
            Kutu("Kasa_Ust", a, new Vector3(0f, AsansorYuk + 0.03f, yuz + 0.02f),
                 new Vector3(AsansorGen + 0.12f, 0.06f, 0.04f));

            // İki kapı kanadı, boşluğun 5 cm içinde.
            float kanatGen = AsansorGen / 2f - 0.01f;
            Kutu("Kapi_Sol", a, new Vector3(-kanatGen / 2f - 0.005f, AsansorYuk / 2f, yuz - 0.05f),
                 new Vector3(kanatGen, AsansorYuk - 0.02f, 0.05f));
            Kutu("Kapi_Sag", a, new Vector3(kanatGen / 2f + 0.005f, AsansorYuk / 2f, yuz - 0.05f),
                 new Vector3(kanatGen, AsansorYuk - 0.02f, 0.05f));

            // Çağrı düğmesi: oyuncunun SAĞINDA, bel hizasında bir plaka ve üstünde düğme.
            // Oyuncu asansöre bakarken -z yönüne bakıyor; onun sağı dünyanın -x tarafı.
            GameObject dugme = Kutu("Cagri_Dugmesi", a, new Vector3(-(AsansorGen / 2f + 0.3f), 1.1f, yuz + 0.015f),
                 new Vector3(0.10f, 0.18f, 0.03f));
            Kutu("Dugme", dugme.transform, new Vector3(0f, 0f, 0.6f), new Vector3(0.4f, 0.25f, 0.8f));

            // Kat göstergesi: kapının üstünde küçük bir ekran. Asansör arızalı,
            // o yüzden sadece bir çizgi yazıyor. İleride bir anomali buraya
            // olmaması gereken bir kat numarası yazabilir.
            Kutu("Gosterge", a, new Vector3(0f, AsansorYuk + 0.22f, yuz + 0.015f),
                 new Vector3(0.30f, 0.12f, 0.03f));
        }

        // Göstergedeki yazı: modelde koyu kırmızı ekranın önünde kırmızı rakam.
        // 90'lar modelinde (#77) ekranın ön yüzü duvardan 4,3 cm önde; yazı onun 3 mm önünde.
        YaziKur("Yazi", a, new Vector3(0f, AsansorYuk + 0.22f, yuz + (model ? 0.046f : 0.04f)), "-", 0.012f,
                model ? new Color(0.9f, 0.15f, 0.1f) : new Color(0.03f, 0.03f, 0.035f));

        // Kanadın ön yüzü: kutu sürümünde 2,5 cm içeride. 90'lar modelinde (#77) camın çevresindeki
        // kasnak kanattan öne çıkıyor (ön yüzü duvardan 3 mm içeride); "ARIZALI" kâğıdı kasnağın
        // önünde dursun, içine gömülmesin.
        float kanatOnu = model ? yuz - 0.0025f : yuz - 0.05f + 0.025f;

        if (tam)
        {
            // "ARIZALI" notu: oyuncunun sağındaki kanada (-x) bantlanmış bir kâğıt.
            Kutu("Not", a, new Vector3(-0.22f, 1.45f, kanatOnu + 0.004f),
                 new Vector3(0.26f, 0.20f, 0.008f));
            YaziKur("Not_Yazi", a, new Vector3(-0.22f, 1.46f, kanatOnu + 0.01f),
                    Dil.Getir("bina.asansor_notu", DilKodu.TR), 0.0055f);
            YerelYap(a.Find("Not_Yazi").gameObject, "bina.asansor_notu");
        }

        // PANO (#68): asansörün solunda (oyuncuya göre; dünyada +x) sigorta panosu. Üç sigortadan
        // biri eksik: asansör bu yüzden arızalı. Her sahanlıkta aynı görünür (döngü aynı katları
        // göstermeli); yalnızca 8. katta zincirin parçasıdır.
        GameObject pano = PanoKur(a, yuz);

        if (tam)
        {
            // KABİN: kapıların ardında, gizli. Asansör 8. katta gelince görünür (AsansorPanosu).
            GameObject kabin = KabinKur(a, yuz);
            kabin.SetActive(false);

            // Çağrı düğmesine nişan alınabilsin: modelde çarpışma kutusu yok (duvara yapışık küçük
            // bir parça). BoxCollider eklenince meshin sınırlarına kendiliğinden oturur.
            Transform dugme = a.Find("Cagri_Dugmesi");
            if (dugme != null && dugme.GetComponent<Collider>() == null) dugme.gameObject.AddComponent<BoxCollider>();

            AsansorPanosu ap = pano.AddComponent<AsansorPanosu>();
            var seri = new SerializedObject(ap);
            seri.FindProperty("eksikSigorta").objectReferenceValue = pano.transform.Find("Sigorta_3").gameObject;
            seri.FindProperty("lamba").objectReferenceValue = pano.transform.Find("Lamba").GetComponent<MeshRenderer>();
            seri.FindProperty("kapiSol").objectReferenceValue = a.Find("Kapi_Sol");
            seri.FindProperty("kapiSag").objectReferenceValue = a.Find("Kapi_Sag");
            // "ARIZALI" kâğıdı ve yazısı sol kanatla birlikte kayar.
            SerializedProperty ekler = seri.FindProperty("solKanattakiler");
            ekler.arraySize = 2;
            ekler.GetArrayElementAtIndex(0).objectReferenceValue = a.Find("Not");
            ekler.GetArrayElementAtIndex(1).objectReferenceValue = a.Find("Not_Yazi");
            seri.FindProperty("kabin").objectReferenceValue = kabin;
            seri.FindProperty("kuyu").objectReferenceValue = kuyu;
            seri.FindProperty("gosterge").objectReferenceValue = a.Find("Yazi").GetComponent<TextMesh>();
            seri.FindProperty("dingSesi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Asansor_Ding.wav");
            seri.FindProperty("ugultuSesi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Asansor_Ugultu.wav");
            seri.ApplyModifiedPropertiesWithoutUndo();

            if (dugme != null)
            {
                AsansorDugmesi ad = dugme.gameObject.AddComponent<AsansorDugmesi>();
                var dSeri = new SerializedObject(ad);
                dSeri.FindProperty("pano").objectReferenceValue = ap;
                dSeri.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    // Asansörün sigorta panosu (#68): duvara vidalı koyu bir sac kutu, içinde üç sigorta yuvası
    // ve bir durum lambası. Kök boş bir nesne (ölçeksiz); parçalar onun çocuğu, yalnızca gövdenin
    // çarpışma kutusu var (nişan panoya bakınca gövdeye çarpar, bileşen kökte).
    static GameObject PanoKur(Transform a, float yuz)
    {
        GameObject pano = new GameObject("Pano");
        pano.transform.SetParent(a, false);
        pano.transform.localPosition = new Vector3(AsansorGen / 2f + 0.31f, 1.35f, yuz);

        Material sac = MalzemeBul("Assets/Materials/Asansor_Pano.mat", new Color(0.16f, 0.17f, 0.18f));
        Material seramik = MalzemeBul("Assets/Materials/Sigorta_Seramik.mat", new Color(0.82f, 0.80f, 0.74f));
        Boya(Kutu("Govde", pano.transform, new Vector3(0f, 0f, 0.035f), new Vector3(0.30f, 0.40f, 0.07f)), sac);
        for (int i = 1; i <= 3; i++)
        {
            float x = (i - 2) * 0.08f;
            // Yuva: sigortanın oturduğu koyu çukur. Sigorta: beyaz seramik gövde.
            GameObject yuva = Kutu($"Yuva_{i}", pano.transform, new Vector3(x, 0.01f, 0.0705f), new Vector3(0.045f, 0.11f, 0.002f));
            Boya(yuva, KaranlikMalzeme());
            Object.DestroyImmediate(yuva.GetComponent<Collider>());
            GameObject sigorta = Kutu($"Sigorta_{i}", pano.transform, new Vector3(x, 0.01f, 0.083f), new Vector3(0.03f, 0.085f, 0.025f));
            Boya(sigorta, seramik);
            Object.DestroyImmediate(sigorta.GetComponent<Collider>());
            // Üçüncü sigorta eksik: yuvası boş görünür. 8. katta takılınca açılır.
            if (i == 3) sigorta.SetActive(false);
        }
        GameObject lamba = Kutu("Lamba", pano.transform, new Vector3(0f, 0.15f, 0.075f), new Vector3(0.03f, 0.03f, 0.012f));
        Boya(lamba, AsansorLambaMalzeme());
        Object.DestroyImmediate(lamba.GetComponent<Collider>());
        return pano;
    }

    // Asansörün kabini (#68): kapıların ardında, sahanlığın ön duvarının arkasında. Ahşap kaplama
    // duvarlar, koyu zemin, arka duvarda tutamak, tavanda sıcak bir lamba. Zemin duvarın içinden
    // sahanlığa kadar uzanır (eşik); üstü sahanlık zemininden 8 mm yukarıda: aynı düzlemde iki
    // farklı malzeme titreşirdi (daire zeminlerindeki ders).
    static GameObject KabinKur(Transform a, float yuz)
    {
        GameObject kabin = new GameObject("Kabin");
        kabin.transform.SetParent(a, false);
        Transform k = kabin.transform;
        float ic = yuz - DuvarKalin;                 // duvarın arka yüzü: kabin buradan başlar
        const float Derin = 1.2f, Gen = 1.2f, Yuk = 2.25f;
        float orta = ic - Derin / 2f;

        // Koyu ceviz kaplama. İlk hâli (0,36 0,29 0,21) kabinin lambasıyla neredeyse beyaz görünüyor,
        // karşıdan bakınca arka duvar kapalı bir kapı gibi okunuyordu. MalzemeBul var olan dosyayı
        // olduğu gibi döndürdüğü için renk burada yeniden yazılır.
        Material kaplama = MalzemeBul("Assets/Materials/Asansor_Kabin.mat", new Color(0.20f, 0.14f, 0.09f));
        kaplama.SetColor("_BaseColor", new Color(0.20f, 0.14f, 0.09f));
        // Zemin koyu gri; anahtar üstünde seçilebilsin diye simsiyah değil (0,10 fazla karanlıktı).
        Material zemin = MalzemeBul("Assets/Materials/Asansor_Kabin_Zemin.mat", new Color(0.22f, 0.21f, 0.20f));
        zemin.SetColor("_BaseColor", new Color(0.22f, 0.21f, 0.20f));
        Material metal = MalzemeBul("Assets/Materials/Metal_Gri.mat", new Color(0.42f, 0.44f, 0.45f));

        Boya(Kutu("Zemin", k, new Vector3(0f, 0.008f - 0.025f, (yuz + ic - Derin) / 2f), new Vector3(Gen, 0.05f, Derin + DuvarKalin)), zemin);
        Boya(Kutu("Duvar_Sol", k, new Vector3(-Gen / 2f, Yuk / 2f, orta), new Vector3(0.05f, Yuk, Derin)), kaplama);
        Boya(Kutu("Duvar_Sag", k, new Vector3(Gen / 2f, Yuk / 2f, orta), new Vector3(0.05f, Yuk, Derin)), kaplama);
        Boya(Kutu("Duvar_Arka", k, new Vector3(0f, Yuk / 2f, ic - Derin), new Vector3(Gen + 0.05f, Yuk, 0.05f)), kaplama);
        Boya(Kutu("Tavan", k, new Vector3(0f, Yuk, orta), new Vector3(Gen + 0.05f, 0.05f, Derin)), kaplama);
        GameObject tutamak = Kutu("Tutamak", k, new Vector3(0f, 0.95f, ic - Derin + 0.05f), new Vector3(0.8f, 0.03f, 0.03f));
        Boya(tutamak, metal);

        // Tavan lambası: parlak (Unlit, HDR) bir panel ve altında gölgesiz, sıcak bir nokta ışığı.
        GameObject panel = Kutu("Lamba_Panel", k, new Vector3(0f, Yuk - 0.03f, orta), new Vector3(0.5f, 0.01f, 0.3f));
        Boya(panel, KabinIsikMalzeme());
        Object.DestroyImmediate(panel.GetComponent<Collider>());
        panel.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GameObject isikNesnesi = new GameObject("Isik");
        isikNesnesi.transform.SetParent(k, false);
        isikNesnesi.transform.localPosition = new Vector3(0f, Yuk - 0.25f, orta);
        Light isik = isikNesnesi.AddComponent<Light>();
        isik.type = LightType.Point;
        // 1,2 açık duvarlarla kabini beyaza boğuyordu, 0,55 koyu duvarlarla anahtarı karanlıkta bıraktı.
        isik.range = 3.0f;
        isik.intensity = 1.0f;
        isik.color = new Color(1f, 0.92f, 0.78f);
        isik.shadows = LightShadows.None;
        return kabin;
    }

    // Panonun durum lambası: ışıktan etkilenmeyen (Unlit) malzeme; rengini AsansorPanosu verir.
    static Material AsansorLambaMalzeme()
    {
        const string Yol = "Assets/Materials/Asansor_Lamba.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;
        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        yeni.SetColor("_BaseColor", new Color(0.22f, 0.02f, 0.02f));
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }

    // Kabinin tavan paneli: sıcak beyaz, 1'in üstünde (HDR), bloom hafif bir hale verir.
    static Material KabinIsikMalzeme()
    {
        const string Yol = "Assets/Materials/Asansor_Kabin_Isik.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;
        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        yeni.SetColor("_BaseColor", new Color(1f, 0.9f, 0.72f) * 1.6f);
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }

    // Dünyadaki bir yazıyı metin tablosuna bağlar (#15): YerelYazi bileşenini ekler
    // ve anahtarını yazar. Sahne Türkçe kurulur (Editor'de okunabilsin); oyun
    // başlayınca YerelYazi metni seçili dilde yeniden yazar.
    static void YerelYap(GameObject nesne, string anahtar)
    {
        nesne.AddComponent<YerelYazi>().AnahtariKur(anahtar);
    }

    // Asansör yazıları için 3B yazı. Sahanlığa (+z) bakar.
    static void YaziKur(string ad, Transform ebeveyn, Vector3 konum, string metin, float boy)
    {
        YaziKur(ad, ebeveyn, konum, metin, boy, new Color(0.03f, 0.03f, 0.035f));
    }

    // Aynı isimde ama renk de alan ikinci sürüm. C#'ta aynı isimli fonksiyon farklı
    // parametrelerle birden fazla kez yazılabilir ("aşırı yükleme"); derleyici
    // çağrıdaki parametrelere bakıp doğrusunu seçer.
    static void YaziKur(string ad, Transform ebeveyn, Vector3 konum, string metin, float boy, Color renk)
    {
        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        // TextMesh'in okunan yüzü -z'ye bakar. Sahanlıktan (+z tarafından)
        // okunabilmesi için 180 derece çeviriyoruz.
        g.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        TextMesh yazi = g.AddComponent<TextMesh>();
        yazi.text = metin;
        yazi.richText = true;                     // <size> etiketi çalışsın
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.fontSize = 96;
        yazi.characterSize = boy;
        yazi.color = renk;
        yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        g.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(yazi.font);
    }

    // =====================================================================
    // DAİRE KAPISI
    // =====================================================================
    // Kapı kökü sahanlığın ortasında değil, kapının kendi z konumunda durur.
    // Bütün parçalar bu köke göre konumlanır; böylece aynı kod dört kapıyı da kurar.
    // sagDuvar = true ise +x duvarındaki kapı (-x yönüne bakar), false ise tersi.
    static void KapiKur(string ad, Transform ebeveyn, bool sagDuvar, float zMerkez, bool tam)
    {
        GameObject kapi = new GameObject(ad);
        kapi.transform.SetParent(ebeveyn, false);
        kapi.transform.localPosition = new Vector3(0f, 0f, zMerkez);
        Transform k = kapi.transform;

        // yon: bütün ölçüler bununla çarpılıyor; tek kod iki duvarı da kuruyor.
        float yon = sagDuvar ? 1f : -1f;

        float duvarYuzu = yon * (SahanlikGen / 2f);     // duvarın iç yüzü
        float kanatX    = duvarYuzu + yon * 0.09f;      // kanat duvarın içine, 9 cm geriye

        // ---- MENTEŞE ----
        // Kapıya ait her parça bu boş nesnenin ÇOCUĞU olur; menteşeyi döndürünce
        // kanat, kol, numaralık ve gözetleme deliği hep birlikte açılır.
        //
        // MENTEŞE TARAFI AYNALIDIR: sağ duvardaki kapının menteşesi -z ucunda,
        // sol duvardakinin +z ucunda. Böylece POZİTİF bir açı her kapıyı
        // daireye doğru (içeri) açar; kart yazan kişi işaret ezberlemek zorunda kalmaz.
        float menteseZ = yon * -(KapiGen / 2f);
        float kanatZ   = yon * (KapiGen / 2f);

        GameObject mentese = new GameObject("Mentese");
        mentese.transform.SetParent(k, false);
        mentese.transform.localPosition = new Vector3(kanatX, 0f, menteseZ);
        Transform m = mentese.transform;

        // Bundan sonraki konumlar MENTEŞEYE göredir.
        Kutu("Kanat", m, new Vector3(0f, KapiYuk / 2f, kanatZ),
             new Vector3(0.06f, KapiYuk, KapiGen));
        Kutu("Kol", m, new Vector3(-yon * 0.07f, 1.05f, yon * (KapiGen - 0.12f)),
             new Vector3(0.09f, 0.04f, 0.12f));
        Kutu("Numaralik", m, new Vector3(-yon * 0.04f, 1.78f, kanatZ),
             new Vector3(0.02f, 0.16f, 0.26f));
        KapiCitalari(m, yon, kanatZ);

        // Sahte katlarda kapı sadece kanat, kol, plaka ve çıtalardan ibaret (çıtalar sahte katta da:
        // döngü bütün katların aynı görünmesine dayanır).
        if (!tam) return;

        // NUMARA YAZISI. KatYoneticisi her katta günceller.
        GameObject yaziNesne = new GameObject("Numara");
        yaziNesne.transform.SetParent(m, false);
        yaziNesne.transform.localPosition = new Vector3(-yon * 0.06f, 1.78f, kanatZ);
        yaziNesne.transform.localRotation = Quaternion.Euler(0f, sagDuvar ? 90f : -90f, 0f);

        TextMesh yazi = yaziNesne.AddComponent<TextMesh>();
        // Yer tutucu: oyunda KatYoneticisi gerçek numarayı her katta yeniden yazar.
        // "0" değil "?", çünkü "değişen numara" kartı numarayı "0" yapıyor; yer tutucu
        // da "0" olsaydı bütünlük testi editördeki sahnede o kartın hiçbir şey
        // değiştirmediğini sanıp hata verirdi. Kartlar "?" kullanmamalı.
        yazi.text = "?";
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.fontSize = 96;
        yazi.characterSize = 0.014f;
        // Neredeyse siyah: yazı ışıktan etkilenmediği için rengi karanlıkta da aynı
        // kalır. Koyu gri (0,12) karanlıkta hafif parlıyordu; siyaha yakın rakam
        // aydınlık plakada okunur, karanlıkta görünmez (#11).
        yazi.color = new Color(0.03f, 0.03f, 0.03f);
        yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        yaziNesne.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(yazi.font);

        // Kapı gözü: kanadın ortasında, göz hizasında. Kanatla birlikte döner.
        Kutu("Kapi_Gozu", m, new Vector3(-yon * 0.035f, 1.60f, kanatZ),
             new Vector3(0.03f, 0.05f, 0.05f));
        // Gözetleme deliğinden sızan ışık (#67): içeride biri varmış gibi. Kapalı kurulur; 7. katın
        // kartı yalnızca gerçek kapınınkini açar (gözlem ipucu). Küçük ve sıcak, gölgesiz.
        GameObject gozIsik = new GameObject("Kapi_Gozu_Isik");
        gozIsik.transform.SetParent(m, false);
        gozIsik.transform.localPosition = new Vector3(-yon * 0.09f, 1.60f, kanatZ);
        Light gl = gozIsik.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = new Color(1f, 0.78f, 0.45f);
        gl.intensity = 0.15f;       // 0,35 kapının üst yarısını parlatıp gerçeği bağırıyordu
        gl.range = 0.6f;
        gl.shadows = LightShadows.None;
        gozIsik.SetActive(false);
        // Kapının altından sızan ışık (#83, 7. kat gözlem ipucu): Cemal'in notu artık gerçek kapıyı tarif
        // etmiyor; içeride ışığı hep yanık olduğu için kapının altından ince bir ışık sızar ve sahanlığın
        // karanlık zeminine sıcak bir leke düşer. Kapalı kurulur; 7. katın kartı yalnızca gerçek kapınınkini
        // açar. Kanatla birlikte döndüğü için kapı açılınca SahanlikKapisi onu söndürür.
        GameObject altIsik = new GameObject("Kapi_Alti_Isik");
        altIsik.transform.SetParent(m, false);
        GameObject cizgi = Kutu("Cizgi", altIsik.transform, new Vector3(-yon * 0.03f, 0.006f, kanatZ), new Vector3(0.02f, 0.012f, 0.80f));
        Object.DestroyImmediate(cizgi.GetComponent<Collider>());   // nişanı ve adımı engellemesin
        // Ampul camı (6 kat parlak) burada fazla göze batıyordu (Şahan: "biraz kıs"); çizginin kendi sönük malzemesi var.
        cizgi.GetComponent<MeshRenderer>().sharedMaterial = KapiAltiMalzeme();
        GameObject leke = new GameObject("Leke");
        leke.transform.SetParent(altIsik.transform, false);
        leke.transform.localPosition = new Vector3(-yon * 0.30f, 0.05f, kanatZ);
        Light ll = leke.AddComponent<Light>();
        ll.type = LightType.Point;
        ll.color = new Color(1f, 0.78f, 0.45f);
        ll.intensity = 0.18f;   // 0,35 iken kapı uzaktan hemen seçiliyordu; şimdi yakına gelince fark edilir
        ll.range = 0.9f;
        ll.shadows = LightShadows.None;
        altIsik.SetActive(false);

        // ---- KAPI ÇEVRESİ EŞYALARI ----
        // Zil: kapının yanında, duvarda.
        Kutu("Zil", k, new Vector3(duvarYuzu - yon * 0.025f, 1.35f, KapiGen / 2f + 0.12f),
             new Vector3(0.05f, 0.08f, 0.06f));

        // Paspas: kapının tam önünde, duvara dayalı. Artık kapının çocuğu;
        // böylece dört kapının paspası dört ayrı isim yerine "Kapi_N/Paspas" oluyor.
        const float PaspasEn = 0.62f, PaspasBoy = 0.50f;
        if (Model("Paspas", k, "Paspas", null, new Vector3(duvarYuzu - yon * PaspasEn / 2f, 0f, 0f), 0f, false) == null)
            Kutu("Paspas", k, new Vector3(duvarYuzu - yon * PaspasEn / 2f, 0.015f, 0f),
                 new Vector3(PaspasEn, 0.03f, PaspasBoy));

        // Terlikler: paspasın üstünde bir çift, ayrı ayrı ("tek terlik" anomalisi).
        GameObject terlikler = new GameObject("Terlikler");
        terlikler.transform.SetParent(k, false);
        // Model varsa: burunlar kapıya dönük (daireye girerken çıkarılmış gibi).
        // Terlik modelinin burnu -Z'ye bakar; -90 x yon derece dönüş burnu duvara çevirir.
        // Sağ terlik, sol terliğin AYNASI: x ölçeği -1. Ayrı nesneler olmaları şart,
        // çünkü "tek terlik" anomalisi yalnızca birini gizliyor.
        // #42: her dairenin terliği farklı (erkek, kadın, çocuk, yaşlı). Nesne adları ve
        // yolları aynı kaldı; terlik anomalileri hangi model olduğunu bilmeden çalışır.
        string terlikModeli = TerlikModeli(ad);
        GameObject terlikSol = Model("Terlik_Sol", terlikler.transform, terlikModeli, null,
                                     new Vector3(duvarYuzu - yon * 0.30f, 0f, -0.07f), -90f * yon, false);
        if (terlikSol != null)
        {
            GameObject terlikSag = Model("Terlik_Sag", terlikler.transform, terlikModeli, null,
                                         new Vector3(duvarYuzu - yon * 0.30f, 0f, 0.07f), -90f * yon, false);
            terlikSag.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            Kutu("Terlik_Sol", terlikler.transform,
                 new Vector3(duvarYuzu - yon * 0.32f, 0.045f, -0.11f),
                 new Vector3(0.26f, 0.06f, 0.10f));
            Kutu("Terlik_Sag", terlikler.transform,
                 new Vector3(duvarYuzu - yon * 0.32f, 0.045f, 0.11f),
                 new Vector3(0.26f, 0.06f, 0.10f));
        }

        // İsimlik: zilin üstünde, duvarda. Pivotu arka yüzünde; -90 x yon ön yüzü sahanlığa çevirir.
        Model("Isimlik", k, "Isimlik", null, new Vector3(duvarYuzu, 1.55f, KapiGen / 2f + 0.12f), -90f * yon, false);

        // Kapının arkasındaki karanlık hol.
        DaireIciKur(k, yon);

        // #44: iki kapının arası (1,1 m) hem poşetlere hem bir rafa yetmiyor. Yakın kapıların
        // (1 ve 4) poşetleri arka duvar tarafına, rafları iki kapının arasına; uzak kapıların
        // (2 ve 3) poşetleri ön duvar tarafında kalır, rafları yine iki kapının arasında.
        // Böylece iki kapının arasında iki raf yan yana durur.
        bool yakinKapi = Mathf.Approximately(zMerkez, YakinKapiZ);
        float arayaDogru = yakinKapi ? -1f : 1f;     // iki kapının arası: yakın kapı için -z, uzak için +z

        // Çöpler: kapının yanında en fazla üç poşet, büyükten küçüğe.
        CoplerKur(k, duvarYuzu, yon, -arayaDogru);

        // Kendi ayakkabılığı ve kapı önündeki dağınık çiftler.
        AyakkabilikKur(k, duvarYuzu, yon, arayaDogru);
    }

    // ---------- DAİRENİN AYAKKABILIĞI (#44) ----------
    // 5 raflı dar ayakkabılık (ayakkabilik.py) ve her rafta bir çift için yer. Raftaki
    // çiftlerin kaçının görüneceğine KatYoneticisi, AyakkabiPlani'na sorarak karar verir.
    // Kapı önünde 0-2 dağınık çift (DaginikAyakkabi): oyuncu E ile rafa koyabilir.
    //
    // Raf ve dağınık çiftler rastgele olduğu için hiçbir anomali kartı onları hedefleyemez
    // (poşetlerle aynı kural, bütünlük testi denetliyor). Kartlar yalnızca rafın kendisini
    // (Kapi_N/Ayakkabilik) hedefleyebilir; çiftler rafın çocuğu olduğu için onunla birlikte hareket eder.
    public static readonly float[] RafYukseklikleri = { 0.05f, 0.24f, 0.43f, 0.62f, 0.81f };   // ayakkabilik.py ile aynı
    const float RafEn = 0.34f, RafDerin = 0.33f;

    static void AyakkabilikKur(Transform kapi, float duvarYuzu, float yon, float arayaDogru)
    {
        float z = arayaDogru * (KapiGen / 2f + 0.04f + RafEn / 2f);
        GameObject raf = Model("Ayakkabilik", kapi, "Ayakkabilik", null,
                               new Vector3(duvarYuzu - yon * (RafDerin / 2f + 0.01f), 0f, z), -90f * yon, carpisma: true);
        if (raf == null) return;

        // Beş raf yuvası. Rafın yerel ekseninde: ön yüz +Z, ayakkabının burnu -Z'ye (rafın
        // içine) bakar; model zaten burnu -Z'ye bakacak biçimde geliyor, dönüş gerekmiyor.
        for (int s = 0; s < RafYukseklikleri.Length; s++)
        {
            GameObject yuva = new GameObject($"Raf_{s + 1}");
            yuva.transform.SetParent(raf.transform, false);
            yuva.transform.localPosition = new Vector3(0f, RafYukseklikleri[s], -0.015f);
            CiftKur("Cift", yuva.transform, Vector3.zero, 0f);
        }

        // Kapı önündeki dağınık çiftler: paspasın önünde, sahanlığın ortasına doğru, gelişigüzel.
        GameObject kok = new GameObject("Ayakkabilar");
        kok.transform.SetParent(kapi, false);
        int kapiNo = kapi.name[kapi.name.Length - 1] - '0';
        float[] zler = { -0.20f, 0.22f };
        float[] donusler = { 20f + kapiNo * 31f, -35f + kapiNo * 17f };
        for (int j = 0; j < zler.Length; j++)
        {
            GameObject cift = CiftKur($"Cift_{j + 1}", kok.transform,
                                      new Vector3(duvarYuzu - yon * 0.80f, 0f, zler[j]), donusler[j]);
            if (cift == null) continue;
            BoxCollider c = cift.AddComponent<BoxCollider>();   // nişan ışını çarpsın diye
            c.center = new Vector3(0f, 0.06f, 0f);
            c.size = new Vector3(0.26f, 0.12f, 0.30f);
            cift.AddComponent<DaginikAyakkabi>();
        }
    }

    // Bir çift ayakkabı: iki ayakkabı modeli yan yana, sağdaki soldakinin aynası.
    static GameObject CiftKur(string ad, Transform ebeveyn, Vector3 konum, float yDonus)
    {
        GameObject cift = new GameObject(ad);
        cift.transform.SetParent(ebeveyn, false);
        cift.transform.localPosition = konum;
        cift.transform.localRotation = Quaternion.Euler(0f, yDonus, 0f);
        GameObject sol = Model("Sol", cift.transform, "Ayakkabi", null, new Vector3(-0.055f, 0f, 0f), 0f, false);
        GameObject sag = Model("Sag", cift.transform, "Ayakkabi", null, new Vector3(0.055f, 0f, 0f), 0f, false);
        if (sag != null) sag.transform.localScale = new Vector3(-1f, 1f, 1f);
        return sol != null ? cift : null;
    }

    // Hangi dairenin kapısında hangi terlik (#42). Oyuncu bir süre sonra bunu öğrenir:
    // 1 numarada lacivert erkek terliği, 2'de pembe kadın terliği... Bir terlik yer
    // değiştirir ya da kaybolursa fark etmesi kolaylaşır.
    static string TerlikModeli(string kapiAdi)
    {
        switch (kapiAdi)
        {
            case "Kapi_1": return "Terlik_Erkek";
            case "Kapi_2": return "Terlik_Kadin";
            case "Kapi_3": return "Terlik_Cocuk";
            default:       return "Terlik_Yasli";   // Kapi_4 (ve zemin kattaki kapılar)
        }
    }

    // ---------- ÇÖP POŞETLERİ ----------
    // Her kapının yanında üç poşet YERİ var. Kaç tanesinin görüneceğine
    // KatYoneticisi, CopPlani'na sorarak karar veriyor (0-3). Görünenler hep
    // baştan sayılır: 2 poşet varsa Poset_1 ve Poset_2 görünür. Poşetler
    // büyükten küçüğe dizildiği için yan yana durunca doğal bir sıra oluşuyor.
    //
    // DİKKAT: poşet sayısı rastgele olduğu için hiçbir anomali kartı poşetleri
    // hedef alamaz. "Eksik poşet" anomalisi artık anlamsız: oyuncu kaç poşet
    // olması gerektiğini bilmiyor. Bütünlük testi bu kuralı denetliyor.
    // yonZ: poşetlerin kapının hangi yanına dizileceği (-1 ön duvara doğru, +1 arka duvara doğru).
    static void CoplerKur(Transform kapi, float duvarYuzu, float yon, float yonZ)
    {
        GameObject kok = new GameObject("Copler");
        kok.transform.SetParent(kapi, false);

        // Her poşetin ölçüsü: genişlik (x), yükseklik (y), derinlik (z).
        Vector3[] olculer =
        {
            new Vector3(0.34f, 0.38f, 0.34f),   // Poset_1: büyük
            new Vector3(0.28f, 0.31f, 0.28f),   // Poset_2: orta
            new Vector3(0.22f, 0.25f, 0.22f)    // Poset_3: küçük
        };

        const float Aralik = 0.04f;
        float z = yonZ * (KapiGen / 2f + 0.10f);   // kapı kenarının biraz ötesinden başla, dışa doğru diz

        // #42: kutular yerine gerçek poşet modelleri (Cop_Poseti.fbx: Poset_A, B, C).
        // Hangi şeklin hangi yere geleceği ve ne kadar dönük duracağı kapıya ve sıraya
        // bağlı SABİT değerler, rastgele değil: bütün katlarda aynı dizilmeli, yoksa
        // ışınlanmada poşetler bir anda şekil değiştirirdi.
        int kapiNo = kapi.name.Length > 0 ? kapi.name[kapi.name.Length - 1] - '0' : 0;
        string[] sekiller = { "Poset_A", "Poset_B", "Poset_C" };
        float[] donusler = { 25f, -40f, 65f };

        for (int i = 0; i < olculer.Length; i++)
        {
            Vector3 o = olculer[i];
            z += yonZ * o.z / 2f;            // bu poşetin merkezi

            // Model 1 m yüksekliğinde üretildi; ölçek = poşetin boyu.
            // Duvardan uzaklık: en geniş poşet şekli boyunun ~1,1 katı; dönük dursa da duvara girmesin.
            GameObject poset = Model($"Poset_{i + 1}", kok.transform, "Cop_Poseti", sekiller[(i + kapiNo) % 3],
                new Vector3(duvarYuzu - yon * (o.y * 0.56f + 0.05f), 0f, z), donusler[i] + kapiNo * 37f, true);
            if (poset != null) poset.transform.localScale = Vector3.one * o.y;
            else poset = Kutu($"Poset_{i + 1}", kok.transform,
                new Vector3(duvarYuzu - yon * (o.x / 2f + 0.05f), o.y / 2f, z),
                o);

            // İşaret bileşeni: oyuncunun nişan ışını buna çarpınca "bu alınabilir
            // bir poşet" olduğunu anlıyor. İsme değil bileşene bakıyoruz.
            poset.AddComponent<CopPoseti>();

            z += yonZ * (o.z / 2f + Aralik); // bir sonrakinin başlangıcı
        }
    }

    // ---------- DAİRE İÇİ ----------
    // Kapının ardındaki 1,2 metrelik kapalı hol. Kapı içeri açıldığında oyuncu
    // hiçliği değil dairenin karanlık holünü görür. Daire içi modellenmiyor.
    static void DaireIciKur(Transform ebeveyn, float yon)
    {
        const float Derinlik = 1.2f;
        const float Kalinlik = 0.1f;

        float genislik = KapiGen + 0.2f;
        float yukseklik = KapiYuk;
        float disYuzey = SahanlikGen / 2f + DuvarKalin;
        float merkezX = yon * (disYuzey + Derinlik / 2f);

        GameObject kok = new GameObject("Daire_Ici");
        kok.transform.SetParent(ebeveyn, false);
        Transform d = kok.transform;

        Kutu("Arka", d, new Vector3(yon * (disYuzey + Derinlik), yukseklik / 2f, 0f),
             new Vector3(Kalinlik, yukseklik + Kalinlik, genislik + Kalinlik));
        Kutu("Zemin", d, new Vector3(merkezX, -Kalinlik / 2f, 0f),
             new Vector3(Derinlik, Kalinlik, genislik));
        Kutu("Tavan", d, new Vector3(merkezX, yukseklik + Kalinlik / 2f, 0f),
             new Vector3(Derinlik, Kalinlik, genislik));
        Kutu("Yan_A", d, new Vector3(merkezX, yukseklik / 2f, genislik / 2f),
             new Vector3(Derinlik, yukseklik, Kalinlik));
        Kutu("Yan_B", d, new Vector3(merkezX, yukseklik / 2f, -(genislik / 2f)),
             new Vector3(Derinlik, yukseklik, Kalinlik));

        // Koyu malzeme: ortam ışığı beyaz kutuları aydınlatıp karanlığı bozmasın.
        Material karanlik = KaranlikMalzeme();
        foreach (MeshRenderer cizici in kok.GetComponentsInChildren<MeshRenderer>())
        {
            cizici.sharedMaterial = karanlik;
        }
    }

    // =====================================================================
    // MERDİVEN ÇEKİRDEĞİ
    // =====================================================================
    // Bir kat sahanlığından bir üst kat sahanlığına çıkan iki kollu merdiven.
    // Yerel orijin: alt sahanlığın döşeme kotu (y = 0), sahanlık kenarı (z = 0).
    //
    //   1. kol (Kol_Cikis): sağda, +z yönünde y = 0'dan 1,5'e çıkar.
    //   Ara sahanlık:       z = 2,16 ile 3,56 arası, y = 1,5; arka duvarda pencere.
    //   2. kol (Kol_Varis): solda, -z yönünde 1,5'ten 3,0'a çıkar ve üst sahanlığa varır.
    //   Merdiven gözü:      iki kolun arasındaki 80 cm'lik boşluk; korkulukla çevrili.
    static GameObject CekirdekKur(string ad)
    {
        GameObject kok = new GameObject(ad);
        Transform t = kok.transform;

        // BASAMAKLAR. Eski merdivende her basamak yere kadar inen dolu bir bloktu.
        // Artık öyle olamaz: kolların altında bir alt katın kolları var ve
        // oyuncu onların üstünden geçerken başını çarpmamalı. Her basamak,
        // basamak yüksekliği artı 20 cm'lik bir dilim; yan yana gelince
        // basamaklı bir döşeme oluşturuyorlar. Altında 2,6 m boşluk kalıyor.
        float dilim = BasamakYuksek + DuvarKalin;

        GameObject cikis = new GameObject("Kol_Cikis");
        cikis.transform.SetParent(t, false);
        for (int i = 0; i < KolBasamakSayisi; i++)
        {
            float ust = (i + 1) * BasamakYuksek;                   // basamağın üst yüzü
            float z = i * BasamakDerinlik + BasamakDerinlik / 2f;  // basamağın ortası
            Kutu($"Basamak_{i + 1:00}", cikis.transform,
                 new Vector3(CikisKoluX, ust - dilim / 2f, z),
                 new Vector3(KolGen, dilim, BasamakDerinlik));
        }

        GameObject varis = new GameObject("Kol_Varis");
        varis.transform.SetParent(t, false);
        for (int j = 0; j < KolBasamakSayisi; j++)
        {
            float ust = AraKot + (j + 1) * BasamakYuksek;
            float z = KolUzunluk - (j * BasamakDerinlik + BasamakDerinlik / 2f);
            Kutu($"Basamak_{j + 1:00}", varis.transform,
                 new Vector3(VarisKoluX, ust - dilim / 2f, z),
                 new Vector3(KolGen, dilim, BasamakDerinlik));
        }

        // ARA SAHANLIK: iki kolu birleştiren dönüş sahanlığı.
        Kutu("Ara_Sahanlik", t,
             new Vector3(0f, AraKot - DuvarKalin / 2f, KolUzunluk + AraSahanlikDerin / 2f),
             new Vector3(CekirdekGen, DuvarKalin, AraSahanlikDerin));

        // YAN DUVARLAR: çekirdeğin iki yanı, bir kat boyu.
        float yanX = CekirdekGen / 2f + DuvarKalin / 2f;
        float boy = CekirdekDerin + DuvarKalin;
        Kutu("Duvar_Sol", t, new Vector3(-yanX, KatYuksekligi / 2f, boy / 2f),
             new Vector3(DuvarKalin, KatYuksekligi, boy));
        Kutu("Duvar_Sag", t, new Vector3(yanX, KatYuksekligi / 2f, boy / 2f),
             new Vector3(DuvarKalin, KatYuksekligi, boy));

        // ARKA DUVAR ve PENCERE.
        PencereliArkaDuvarKur(t);

        // KİLİTLİ KAT KAPISI (#52): çıkış kolunun tepesinde, ara sahanlığın girişinde.
        KatKapisiKur(t);

        // KORKULUKLAR: merdiven gözünü çevreler. Oyuncu gözden düşemesin ama
        // üstünden eğilip aşağı ve yukarı bakabilsin diye dolu duvar değil,
        // tırabzan + ince dikmeler. Dikmeler arası 24 cm; oyuncunun gövdesi
        // (60 cm) arasından geçemez.
        KorkuluklariKur(t);

        // Pencere rüzgârı #74'te kalktı (Şahan'ın ses kararları).

        // Merdiven ampulü: ara sahanlığın üstünde. #51: neredeyse sönük (0,25); merdiven
        // karanlık, oyuncu kafa lambasıyla çıkar.
        AmpulKur("Ampul_Merdiven", t,
                 new Vector3(0f, AraKot + 2.35f, KolUzunluk + AraSahanlikDerin / 2f), 7f, LambaSiddeti * 0.25f, yanik: false);

        return kok;
    }

    // ---------- PENCERELİ ARKA DUVAR ----------
    // Pencere ara sahanlığın arka duvarında. Duvar bu yüzden katın döşemesine
    // göre değil ara sahanlığa göre bölünüyor: her çekirdek arka duvarın
    // y = 1,0 ile 4,0 arasını kuruyor (bir kat boyu, kesintisiz dizilir) ve
    // pencere bu aralığın içinde, ara sahanlığın 0,9 m üstünde başlıyor.
    static void PencereliArkaDuvarKur(Transform t)
    {
        const float Alt = 1.0f, Ust = 4.0f;                     // duvar bandı
        const float PencereAlt = AraKot + 0.9f;                 // 2.4
        const float PencereUst = AraKot + 2.0f;                 // 3.5
        const float PencereGen = 1.2f;

        float z = CekirdekDerin + DuvarKalin / 2f;
        float ic = CekirdekGen / 2f;                            // yan duvarların iç yüzü
        float yanGen = ic - PencereGen / 2f;

        Kutu("Duvar_Arka_Alt", t, new Vector3(0f, (Alt + PencereAlt) / 2f, z),
             new Vector3(CekirdekGen, PencereAlt - Alt, DuvarKalin));
        Kutu("Duvar_Arka_Ust", t, new Vector3(0f, (PencereUst + Ust) / 2f, z),
             new Vector3(CekirdekGen, Ust - PencereUst, DuvarKalin));

        float ortaY = (PencereAlt + PencereUst) / 2f;
        float pencereH = PencereUst - PencereAlt;
        Kutu("Duvar_Arka_Sol", t, new Vector3(-(PencereGen / 2f + yanGen / 2f), ortaY, z),
             new Vector3(yanGen, pencereH, DuvarKalin));
        Kutu("Duvar_Arka_Sag", t, new Vector3(PencereGen / 2f + yanGen / 2f, ortaY, z),
             new Vector3(yanGen, pencereH, DuvarKalin));

        // PENCERE: çerçeve, ortada bir kayıt ve mermer denizlik.
        GameObject pencere = new GameObject("Pencere");
        pencere.transform.SetParent(t, false);
        Transform p = pencere.transform;

        float cz = z;   // çerçeve duvarın ortasında
        Kutu("Cerceve_Alt", p, new Vector3(0f, PencereAlt + 0.025f, cz), new Vector3(PencereGen, 0.05f, 0.08f));
        Kutu("Cerceve_Ust", p, new Vector3(0f, PencereUst - 0.025f, cz), new Vector3(PencereGen, 0.05f, 0.08f));
        Kutu("Cerceve_Sol", p, new Vector3(-PencereGen / 2f + 0.025f, ortaY, cz), new Vector3(0.05f, pencereH, 0.08f));
        Kutu("Cerceve_Sag", p, new Vector3(PencereGen / 2f - 0.025f, ortaY, cz), new Vector3(0.05f, pencereH, 0.08f));
        Kutu("Kayit", p, new Vector3(0f, ortaY, cz), new Vector3(0.04f, pencereH, 0.06f));
        Kutu("Denizlik", p, new Vector3(0f, PencereAlt - 0.02f, CekirdekDerin - 0.06f),
             new Vector3(PencereGen + 0.16f, 0.04f, 0.14f));

        // DIŞARISI: pencerenin 1 m ötesinde gece. Koyu lacivert bir yüzey;
        // oyuncu camdan baktığında boşluğu değil, karanlık bir geceyi görür.
        //
        // Yüzey GENİŞ (10 m) ve TAM BİR KAT BOYU (3 m) yüksekliğinde. İlk denemede
        // pencereden biraz büyüktü ve yandan bakınca kenarı görünüp arkasından
        // gökyüzü sızıyordu. Yüksekliğin tam kat boyu olması da bilerek:
        // üst üste dizilen katlarda bu yüzeyler birbirine değip kesintisiz bir
        // karanlık duvar oluşturuyor, arada ne boşluk kalıyor ne üst üste binme.
        // #53: genişlik sahanlığın dış genişliği (7,4 m). Eskiden 10 m'ydi; 1 ve 4 numaralı
        // daireler merdiven çekirdeğinin arkasına kadar uzandığı için 10 m'lik yüzey salonun
        // içinden geçip odada lacivert bir duvar dilimi gibi görünüyordu.
        GameObject gece = Kutu("Disari", p, new Vector3(0f, (Alt + Ust) / 2f, CekirdekDerin + DuvarKalin + 1.0f),
             new Vector3(SahanlikGen + 2f * DuvarKalin, Ust - Alt, 0.1f));
        gece.GetComponent<MeshRenderer>().sharedMaterial = GeceMalzeme();
    }

    // ---------- KORKULUKLAR ----------
    const float DikmeKalin = 0.03f;
    const float TirabzanKalin = 0.05f;

    static void KorkuluklariKur(Transform t)
    {
        GameObject kok = new GameObject("Korkuluklar");
        kok.transform.SetParent(t, false);
        Transform k = kok.transform;

        float kenar = KuyuGen / 2f + DikmeKalin / 2f + 0.01f;   // dikmeler kolun göz kenarında
        float h = BasamakYuksek, d = BasamakDerinlik;

        // 1. kol boyunca (sağda, x = +0,4): her basamağa bir dikme.
        for (int i = 0; i < KolBasamakSayisi; i++)
        {
            float ust = (i + 1) * h;
            Kutu($"Dikme_Cikis_{i + 1:00}", k,
                 new Vector3(kenar, ust + KorkulukYuk / 2f, i * d + d / 2f),
                 new Vector3(DikmeKalin, KorkulukYuk, DikmeKalin));
        }
        // Tırabzan: dikmelerin tepesinden geçen eğik çubuk.
        EgimliKutu("Tirabzan_Cikis", k,
                   new Vector3(kenar, KorkulukYuk + 0.5f * h, 0f),
                   new Vector3(kenar, KorkulukYuk + (KolBasamakSayisi + 0.5f) * h, KolUzunluk),
                   TirabzanKalin);

        // 2. kol boyunca (solda, x = -0,4).
        for (int j = 0; j < KolBasamakSayisi; j++)
        {
            float ust = AraKot + (j + 1) * h;
            Kutu($"Dikme_Varis_{j + 1:00}", k,
                 new Vector3(-kenar, ust + KorkulukYuk / 2f, KolUzunluk - (j * d + d / 2f)),
                 new Vector3(DikmeKalin, KorkulukYuk, DikmeKalin));
        }
        EgimliKutu("Tirabzan_Varis", k,
                   new Vector3(-kenar, AraKot + KorkulukYuk + 0.5f * h, KolUzunluk),
                   new Vector3(-kenar, AraKot + KorkulukYuk + (KolBasamakSayisi + 0.5f) * h, 0f),
                   TirabzanKalin);

        // Gözün iki ucu: ara sahanlık kenarı ve kat sahanlığı kenarı.
        // Buralar düz korkuluk; dikmeler 20 cm arayla.
        DuzKorkulukKur("Korkuluk_Ara", k, AraKot, KolUzunluk + DikmeKalin / 2f + 0.01f);
        DuzKorkulukKur("Korkuluk_Sahanlik", k, 0f, -(DikmeKalin / 2f + 0.01f));
    }

    // Gözün bir ucunu x = -0,4 ile +0,4 arasında kapatan yatay korkuluk.
    static void DuzKorkulukKur(string ad, Transform ebeveyn, float kot, float z)
    {
        GameObject kok = new GameObject(ad);
        kok.transform.SetParent(ebeveyn, false);

        int adet = Mathf.RoundToInt(KuyuGen / 0.2f) + 1;   // 5 dikme: -0,4 ... +0,4
        for (int i = 0; i < adet; i++)
        {
            float x = -KuyuGen / 2f + i * (KuyuGen / (adet - 1));
            Kutu($"Dikme_{i + 1}", kok.transform, new Vector3(x, kot + KorkulukYuk / 2f, z),
                 new Vector3(DikmeKalin, KorkulukYuk, DikmeKalin));
        }
        Kutu("Tirabzan", kok.transform, new Vector3(0f, kot + KorkulukYuk, z),
             new Vector3(KuyuGen + 0.06f, TirabzanKalin, TirabzanKalin));
    }

    // =====================================================================
    // ÇATI BÖLÜMÜ
    // =====================================================================
    // 8. katta Sahanlik_B ve üstündeki bütün sahte katlar kapanır, yerine bu açılır.
    // Oyuncu 8. kattan çıkınca bir sahanlığa değil, bir "merdiven evi"ne varır:
    // üstü kapalı küçük bir oda ve önünde çatıya açılan kapı.
    // Yerel orijin B ile aynı: y = 0 son merdivenin vardığı kot.
    //
    // Kapı KAPALI başlar. 8. kat hatasız geçildiğinde KatYoneticisi
    // "CatiKapisi" nesnesini kapatır; kapı açılır, gökyüzü görünür.
    static GameObject CatiBolumuKur()
    {
        GameObject kok = new GameObject("CatiBolumu");
        Transform t = kok.transform;

        const float OdaDerin   = 1.2f;    // merdiven başı sahanlığının derinliği
        const float KapiGenC   = 1.0f;
        const float KapiYukC   = 2.05f;
        const float Parapet    = 1.0f;

        float disX = SahanlikGen / 2f + DuvarKalin;          // 3.7
        float cekirdekDis = CekirdekGen / 2f + DuvarKalin;   // 2.0

        // DÖŞEME: B'nin döşemesiyle BİREBİR aynı ölçüde. Aynı zamanda
        // 8. katın tavanı; farklı olsaydı oyuncu yukarı bakınca fark ederdi.
        Kutu("Cati_Zemin", t,
             new Vector3(0f, -DuvarKalin / 2f, (-SahanlikDerin - DuvarKalin) / 2f),
             new Vector3(2f * disX, DuvarKalin, SahanlikDerin + DuvarKalin));

        // Merdiven boşluğunun ağzı: sağ kol ve göz aşağıya açık. Korkulukla kapatılıyor.
        GameObject agiz = new GameObject("Korkuluk_Agiz");
        agiz.transform.SetParent(t, false);
        float agizBas = -KuyuGen / 2f, agizBit = CekirdekGen / 2f;
        int adet = Mathf.RoundToInt((agizBit - agizBas) / 0.2f) + 1;
        for (int i = 0; i < adet; i++)
        {
            float x = agizBas + i * ((agizBit - agizBas) / (adet - 1));
            Kutu($"Dikme_{i + 1:00}", agiz.transform, new Vector3(x, KorkulukYuk / 2f, -0.025f),
                 new Vector3(DikmeKalin, KorkulukYuk, DikmeKalin));
        }
        Kutu("Tirabzan", agiz.transform, new Vector3((agizBas + agizBit) / 2f, KorkulukYuk, -0.025f),
             new Vector3(agizBit - agizBas + 0.06f, TirabzanKalin, TirabzanKalin));

        // MERDİVEN EVİ: yan duvarlar, arka duvar ve tavan.
        float odaBoy = OdaDerin + CekirdekDerin + DuvarKalin;
        float odaMerkezZ = (-OdaDerin + CekirdekDerin + DuvarKalin) / 2f;
        float yanX = CekirdekGen / 2f + DuvarKalin / 2f;
        Kutu("Oda_Duvar_Sol", t, new Vector3(-yanX, DuvarYuksek / 2f, odaMerkezZ),
             new Vector3(DuvarKalin, DuvarYuksek, odaBoy));
        Kutu("Oda_Duvar_Sag", t, new Vector3(yanX, DuvarYuksek / 2f, odaMerkezZ),
             new Vector3(DuvarKalin, DuvarYuksek, odaBoy));

        // Arka duvar: çekirdeğin pencereli bandı y = 4,0'da (burada 1,0'da) bitiyor.
        Kutu("Oda_Duvar_Arka", t,
             new Vector3(0f, (1.0f + DuvarYuksek) / 2f, CekirdekDerin + DuvarKalin / 2f),
             new Vector3(CekirdekGen, DuvarYuksek - 1.0f, DuvarKalin));

        Kutu("Oda_Tavan", t,
             new Vector3(0f, DuvarYuksek + DuvarKalin / 2f, odaMerkezZ),
             new Vector3(2f * cekirdekDis, DuvarKalin, odaBoy));

        // ÖN DUVAR ve KAPI: kapı, oyuncunun vardığı sol kolun tam karşısında.
        float onZ = -OdaDerin - DuvarKalin / 2f;
        float kapiX = VarisKoluX;
        float solBas = -cekirdekDis, solBit = kapiX - KapiGenC / 2f;
        float sagBas = kapiX + KapiGenC / 2f, sagBit = cekirdekDis;
        Kutu("Oda_Duvar_On_Sol", t, new Vector3((solBas + solBit) / 2f, DuvarYuksek / 2f, onZ),
             new Vector3(solBit - solBas, DuvarYuksek, DuvarKalin));
        Kutu("Oda_Duvar_On_Sag", t, new Vector3((sagBas + sagBit) / 2f, DuvarYuksek / 2f, onZ),
             new Vector3(sagBit - sagBas, DuvarYuksek, DuvarKalin));
        Kutu("Oda_Duvar_On_Lento", t, new Vector3(kapiX, (KapiYukC + DuvarYuksek) / 2f, onZ),
             new Vector3(KapiGenC, DuvarYuksek - KapiYukC, DuvarKalin));

        // KAPI. Açılması = bu nesnenin kapatılması.
        // Yeni ilerlemede (#68) kapı çatı anahtarıyla açılır: CatiKapisi "KİLİTLİ / AÇ" der.
        Kutu("CatiKapisi", t, new Vector3(kapiX, KapiYukC / 2f, onZ),
             new Vector3(KapiGenC, KapiYukC, DuvarKalin * 0.6f)).AddComponent<CatiKapisi>();

        // KORKULUKLAR (parapet): çatının kenarları.
        Kutu("Parapet_Sol", t, new Vector3(-disX + DuvarKalin / 2f, Parapet / 2f, -SahanlikDerin / 2f),
             new Vector3(DuvarKalin, Parapet, SahanlikDerin));
        Kutu("Parapet_Sag", t, new Vector3(disX - DuvarKalin / 2f, Parapet / 2f, -SahanlikDerin / 2f),
             new Vector3(DuvarKalin, Parapet, SahanlikDerin));
        Kutu("Parapet_On", t, new Vector3(0f, Parapet / 2f, -SahanlikDerin - DuvarKalin / 2f),
             new Vector3(2f * disX, Parapet, DuvarKalin));

        // SABAH SESİ (#25): çatının ortasında, KAPALI bekler. Çatı kapısı açılınca
        // KatYoneticisi onu açar ve ses kapıdan içeri dolar.
        OrtamKaynagi("Ses_Sabah", t, new Vector3(0f, 1.5f, -3.2f), "Ortam_Sabah", 0.6f, 3f, 20f);
        Transform sabah = t.Find("Ses_Sabah");
        if (sabah != null) sabah.gameObject.SetActive(false);

        float arkaGen = disX - cekirdekDis;
        Kutu("Parapet_Arka_Sol", t, new Vector3(-(cekirdekDis + arkaGen / 2f), Parapet / 2f, DuvarKalin / 2f),
             new Vector3(arkaGen, Parapet, DuvarKalin));
        Kutu("Parapet_Arka_Sag", t, new Vector3(cekirdekDis + arkaGen / 2f, Parapet / 2f, DuvarKalin / 2f),
             new Vector3(arkaGen, Parapet, DuvarKalin));

        return kok;
    }

    // =====================================================================
    // KÜÇÜK YARDIMCILAR
    // =====================================================================

    // ---------- ZEMİN KAT (#16) ----------
    // Normal bir sahanlığı zemin kata çevirir: 1 numaranın yerinde kapıcı dairesi
    // (kapısı açık, arkasında oda ve yönetici notu), ön duvarda bina girişi,
    // arka duvarda posta kutuları, aşağı inen merdivenin başında bodrum kapısı.
    /// <summary>
    /// Çividen sarkan YANIK kafa lambası (#56 kapıcı odası, #108 dokuzuncu kat): model, cam diski, gölgeli
    /// spot ve küçük saçılma ışığı. Pivot çivinin duvara girdiği nokta; +Z odaya bakar. civiIsigi: spot.
    /// </summary>
    static GameObject AsiliLambaKur(Transform ebeveyn, Vector3 yer, float yDonus, out Light civiIsigi)
    {
        civiIsigi = null;
        GameObject asiliLamba = Model("Cividen_Lamba", ebeveyn, "Kafa_Lambasi", "Lamba", yer, yDonus, carpisma: true);
        if (asiliLamba == null) return null;
        // Lambanın camı: parlayan ampul camının malzemesiyle küçük bir disk (bloom hale yapar),
        // yani oyuncu ışığın nereden geldiğini görür. Çarpıştırıcısı silinir: nişan lambaya gelsin.
        GameObject camIsigi = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        camIsigi.name = "Cam_Isigi";
        Object.DestroyImmediate(camIsigi.GetComponent<Collider>());
        camIsigi.transform.SetParent(asiliLamba.transform, false);
        camIsigi.transform.localPosition = new Vector3(0f, -0.202f, 0.0585f);   // modelin camının önü
        camIsigi.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);       // silindirin ekseni +z'ye
        camIsigi.transform.localScale = new Vector3(0.029f, 0.001f, 0.029f);
        camIsigi.GetComponent<MeshRenderer>().sharedMaterial = CamMalzeme();

        // Lambanın ışığı: camdan odaya, 35° aşağı ve biraz kapıya (+x) eğik, geniş bir spot.
        // Odanın ortasını, masayı ve kapının dibindeki notu aydınlatır: oyuncu kapıya dönük uyanır,
        // ilk gördüğü şey ışıkta duran not olsun. Tek hareketsiz spot, gölgesi ucuz; gölge odaya
        // derinlik verir.
        GameObject isikNesne = new GameObject("Isik");
        isikNesne.transform.SetParent(asiliLamba.transform, false);
        isikNesne.transform.localPosition = new Vector3(0f, -0.202f, 0.075f);
        // +x dönüşü ileriyi aşağı eğer; +y dönüşü ileriyi (+z) kapıya (+x) çevirir.
        isikNesne.transform.localRotation = Quaternion.Euler(35f, 8f, 0f);
        civiIsigi = isikNesne.AddComponent<Light>();
        civiIsigi.type = LightType.Spot;
        civiIsigi.spotAngle = 105f;
        civiIsigi.innerSpotAngle = 55f;
        civiIsigi.range = 7f;
        civiIsigi.intensity = 4f;
        civiIsigi.color = KafaLambasi.Renk;             // kafadaki lambayla aynı sıcak beyaz: aynı lamba
        civiIsigi.shadows = LightShadows.Soft;

        // Saçılma: camın çevresine sızan ışık. Spot yalnızca önünü aydınlatır; lambanın kendisi,
        // kayışı ve çivi karanlıkta kalıp görünmüyordu. Kısa menzilli, gölgesiz, loş bir nokta
        // ışık lambayı ve arkasındaki duvarı hafifçe gösterir: oyuncu ışığın kaynağını bulur.
        GameObject sacilim = new GameObject("Sacilim");
        sacilim.transform.SetParent(asiliLamba.transform, false);
        sacilim.transform.localPosition = new Vector3(0f, -0.15f, 0.18f);
        Light sacilimIsigi = sacilim.AddComponent<Light>();
        sacilimIsigi.type = LightType.Point;
        sacilimIsigi.range = 0.9f;
        sacilimIsigi.intensity = 0.12f;   // 0,35'te lambanın kırmızı gövdesi bembeyaz görünüyordu
        sacilimIsigi.color = KafaLambasi.Renk;
        sacilimIsigi.shadows = LightShadows.None;
        return asiliLamba;
    }

    static void ZeminKatiDonat(Transform z)
    {
        Material ahsap = MalzemeBul("Assets/Materials/Ahsap_Koyu.mat", new Color(0.24f, 0.14f, 0.07f));
        Material kagit = MalzemeBul("Assets/Materials/Kagit.mat", new Color(0.86f, 0.82f, 0.70f));
        Material metal = MalzemeBul("Assets/Materials/Metal_Gri.mat", new Color(0.42f, 0.44f, 0.45f));

        float disX = SahanlikGen / 2f + DuvarKalin;   // yan duvarın dış yüzü (3.7)

        // ---- KAPICI DAİRESİNİN KAPISI: ardına kadar açık (pozitif açı içeri açar) ----
        Transform mentese = z.Find("Kapi_1/Mentese");
        if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // ---- KAPICI ODASI: sol duvarın dışında, kapının arkasında ----
        GameObject oda = new GameObject("Kapici_Odasi");
        oda.transform.SetParent(z, false);
        Transform o = oda.transform;

        float icX = -disX, disOda = -disX - OdaDerin;          // oda x = -3.7 ile -6.9 arası
        float ortaX = (icX + disOda) / 2f;
        float zOn = YakinKapiZ + OdaGen / 2f, zArka = YakinKapiZ - OdaGen / 2f;   // -0.2 ile -2.8

        Kutu("Zemin", o, new Vector3(ortaX, -DuvarKalin / 2f, YakinKapiZ), new Vector3(OdaDerin + DuvarKalin, DuvarKalin, OdaGen + 2f * DuvarKalin));
        Kutu("Tavan", o, new Vector3(ortaX, DuvarYuksek + DuvarKalin / 2f, YakinKapiZ), new Vector3(OdaDerin + DuvarKalin, DuvarKalin, OdaGen + 2f * DuvarKalin));
        Kutu("Duvar_Arka", o, new Vector3(disOda - DuvarKalin / 2f, DuvarYuksek / 2f, YakinKapiZ), new Vector3(DuvarKalin, DuvarYuksek, OdaGen + 2f * DuvarKalin));
        Kutu("Duvar_On", o, new Vector3(ortaX, DuvarYuksek / 2f, zOn + DuvarKalin / 2f), new Vector3(OdaDerin, DuvarYuksek, DuvarKalin));
        Kutu("Duvar_Yan", o, new Vector3(ortaX, DuvarYuksek / 2f, zArka - DuvarKalin / 2f), new Vector3(OdaDerin, DuvarYuksek, DuvarKalin));

        // Eşyalar: dar bir kapıcı odası. Yatak arka köşede, masa karşı duvarda.
        // #81 (Şahan'ın seçimi): tahta divan ve çizgili yün yorgan (Art/Blender/scripts/divan.py). Önceden üç düz
        // kutuydu (yatak, battaniye, yastık). Yastık arka duvar tarafında kalsın diye 180° çevrilir.
        if (Model("Yatak", o, "Divan", "Divan", new Vector3(disOda + 1.05f, 0f, zArka + 0.5f), 180f, carpisma: true) == null)
            Kutu("Yatak", o, new Vector3(disOda + 1.05f, 0.22f, zArka + 0.5f), new Vector3(1.95f, 0.44f, 0.9f)).GetComponent<MeshRenderer>().sharedMaterial = ahsap;
        // #81 (Şahan'ın seçimi): masa, sandalye, dolap ve masadaki kutu artık Poly Haven modelleri (MobilyaKurucu
        // prefab'ları). Masa eski ahşap masa: 1,8 m uzun, 55 cm alçak bir masa; odaya sığsın ve telefon eskisi gibi
        // 74 cm'de dursun diye boyu 0,72, yüksekliği 1,35 kat (bacaklar biraz incelir, göze batmaz). Dolap 2,57 m:
        // %80'e küçülür (2,06 m). Radyo konmayacaktı (#56); o gri kutunun yerinde 90'lar çalar saati.
        MobKoy("Masa", o, "WoodenTable_01", new Vector3(disOda + 1.2f, 0f, zOn - 0.36f), 180f, new Vector3(0.72f, 1.35f, 1f));
        MobKoy("Sandalye", o, "painted_wooden_chair_02", new Vector3(disOda + 1.1f, 0f, zOn - 0.95f), 0f, Vector3.one);
        MobKoy("Calar_Saat", o, "alarm_clock_01", new Vector3(disOda + 0.8f, 0.7425f, zOn - 0.32f), 200f, Vector3.one);
        MobKoy("Dolap", o, "painted_wooden_cabinet_02", new Vector3(disOda + 0.30f, 0f, YakinKapiZ + 0.15f), 90f, Vector3.one * 0.8f);

        // Duvar takvimi: tarihi söyleyen tek ayrıntı. Oyuncu odanın içinden (-z tarafından)
        // bakıyor; TextMesh'in okunan yüzü de o yöne dönük, döndürmeye gerek yok.
        Kutu("Takvim", o, new Vector3(icX - 0.9f, 1.5f, zOn - 0.01f), new Vector3(0.3f, 0.42f, 0.01f)).GetComponent<MeshRenderer>().sharedMaterial = kagit;
        GameObject takvimYazi = new GameObject("Takvim_Yazi");
        takvimYazi.transform.SetParent(o, false);
        takvimYazi.transform.localPosition = new Vector3(icX - 0.9f, 1.62f, zOn - 0.02f);
        TextMesh ty = takvimYazi.AddComponent<TextMesh>();
        ty.text = Dil.Getir("bina.takvim", DilKodu.TR);
        YerelYap(takvimYazi, "bina.takvim");
        ty.anchor = TextAnchor.MiddleCenter;
        ty.alignment = TextAlignment.Center;
        ty.fontSize = 96;
        ty.characterSize = 0.008f;
        ty.color = new Color(0.45f, 0.05f, 0.05f);
        ty.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        takvimYazi.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(ty.font);

        // Önceki üç kapıcının fotoğrafı (#69): masanın üstünde, ön duvarda. Duvar çerçevesinin noktası
        // arka yüzü; ön yüzü (+Z) odanın içine (-z) dönsün diye 180° çevrilir. Fotoğrafı oyun başında
        // kimliğinden kendisi takar (AileFotografi.Start).
        GameObject fotoSablonu = FotografKurucu.Sablon(duvar: true);
        if (fotoSablonu != null)
        {
            GameObject foto = (GameObject)PrefabUtility.InstantiatePrefab(fotoSablonu, o);
            foto.name = "Kapici_Fotografi";
            foto.transform.localPosition = new Vector3(disOda + 1.35f, 1.45f, zOn);
            foto.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var fotoSeri = new SerializedObject(foto.GetComponent<AileFotografi>());
            fotoSeri.FindProperty("kimlik").stringValue = "kapici";
            fotoSeri.ApplyModifiedPropertiesWithoutUndo();
        }
        // Menüdeki Fotoğraflar sayfası da albümü bilsin.
        DuraklatmaMenusu menu = Object.FindAnyObjectByType<DuraklatmaMenusu>(FindObjectsInactive.Include);
        if (menu != null)
        {
            var menuSeri = new SerializedObject(menu);
            menuSeri.FindProperty("album").objectReferenceValue = FotografKurucu.AlbumKur();
            menuSeri.ApplyModifiedPropertiesWithoutUndo();
        }

        // TELEFON (#56): masada, radyonun yanında, yüzü odaya (-z). Çalınca açılır; konuşma not
        // ekranında okunur. Ekran çarpıştırıcısız bir çocukta: nişan hep telefona gelir.
        GameObject telefon = Model("Telefon", o, "Telefon", "Telefon", new Vector3(disOda + 1.42f, 0.74f, zOn - 0.35f), 180f, carpisma: true);
        if (telefon != null)
        {
            GameObject ahize = Model("Ahize", telefon.transform, "Telefon", "Ahize", Vector3.zero, 0f, carpisma: false);
            GameObject konusma = new GameObject("Konusma");
            konusma.transform.SetParent(telefon.transform, false);
            YoneticiNotu ekran = konusma.AddComponent<YoneticiNotu>();
            Telefon tel = telefon.AddComponent<Telefon>();
            var telSeri = new SerializedObject(tel);
            telSeri.FindProperty("konusmaEkrani").objectReferenceValue = ekran;
            telSeri.FindProperty("ahize").objectReferenceValue = ahize != null ? ahize.transform : null;
            telSeri.ApplyModifiedPropertiesWithoutUndo();
        }

        // PANO (#56): yatağın üstündeki duvarda (z = zArka, yüzü odaya +z) mantar pano. Okunan notlar
        // üstüne kâğıt olarak iğnelenir (Pano). Görsel kutular kökün çocuğu; kökte Pano ve çarpıştırıcı.
        GameObject pano = new GameObject("Pano");
        pano.transform.SetParent(o, false);
        pano.transform.localPosition = new Vector3(disOda + 1.3f, 1.45f, zArka);
        Material mantar = MalzemeBul("Assets/Materials/Pano_Mantar.mat", new Color(0.45f, 0.30f, 0.16f));
        Kutu("Mantar", pano.transform, new Vector3(0f, 0f, 0f), new Vector3(1.0f, 0.72f, 0.02f)).GetComponent<MeshRenderer>().sharedMaterial = mantar;
        foreach (var cita in new[] { (0f, 0.375f, 1.06f, 0.03f), (0f, -0.375f, 1.06f, 0.03f), (-0.515f, 0f, 0.03f, 0.72f), (0.515f, 0f, 0.03f, 0.72f) })
            Kutu("Cita", pano.transform, new Vector3(cita.Item1, cita.Item2, 0.004f), new Vector3(cita.Item3, cita.Item4, 0.03f)).GetComponent<MeshRenderer>().sharedMaterial = ahsap;
        foreach (Collider c in pano.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        BoxCollider panoCarpisma = pano.AddComponent<BoxCollider>();
        panoCarpisma.size = new Vector3(1.06f, 0.78f, 0.04f);
        Pano panoBileseni = pano.AddComponent<Pano>();
        var panoSeri = new SerializedObject(panoBileseni);
        panoSeri.FindProperty("kagitMalzemesi").objectReferenceValue = kagit;
        panoSeri.ApplyModifiedPropertiesWithoutUndo();

        // ÇİVİ VE CEMAL'İN KAFA LAMBASI (#56): yan duvarda (z = zArka, yüzü odaya +z), yatağın ayak
        // ucuyla kapı arasında, göz hizasının biraz altında. Oyuncu uyanınca sağ önünde görür. Lamba
        // çivide asılıyken yanıyor: karanlık odanın tek ışığı (notu da o gösterir). Kapıya 1,15 m uzak:
        // daha yakında, içeri ardına kadar açık kapının kanadı (z ≈ -1,05'te, x -3,7 ile -4,6 arası)
        // ışığı kesiyor ve odayı karanlıkta bırakıyordu. İki parçanın pivotu da çivinin duvara girdiği
        // nokta, ikisi aynı yere konur. Model yoksa (FBX silinmişse) atlanır.
        Vector3 civiYeri = new Vector3(icX - 1.15f, 1.62f, zArka);
        Model("Civi", o, "Kafa_Lambasi", "Civi", civiYeri, 0f, carpisma: false);
        GameObject asiliLamba = AsiliLambaKur(o, civiYeri, 0f, out Light civiIsigi);
        if (asiliLamba != null)
        {
            CividekiLamba cl = asiliLamba.AddComponent<CividekiLamba>();
            var clSeri = new SerializedObject(cl);
            clSeri.FindProperty("isik").objectReferenceValue = civiIsigi;
            clSeri.ApplyModifiedPropertiesWithoutUndo();
        }

        // İKİNCİ ÇİVİ VE MERDİVEN KAPISININ ANAHTARI (#56, Şahan 2026-09-28): lambanın 16 cm
        // kapı tarafında. Açılışta zemin kattan 1. kata çıkan kapı kilitli (KatYoneticisi kilitler);
        // anahtarı burada asılı, lambanın saçılma ışığı onu da gösterir. Anahtar modeli yatık
        // yapılmış (sahanlıkta rafta durur): X ekseninde -90° çevrilince halkası yukarıda (çivide),
        // ucu aşağıda duvara sarkar (+90° baş aşağı asıyordu). KatAnahtari "0. kat": yalnızca zemin
        // kapısını açar.
        Vector3 civi2Yeri = civiYeri + new Vector3(0.16f, 0f, 0f);
        Model("Civi_2", o, "Kafa_Lambasi", "Civi", civi2Yeri, 0f, carpisma: false);
        GameObject zeminAnahtari = Model("Zemin_Anahtari", o, "Anahtar", null,
                                         civi2Yeri + new Vector3(0f, -0.045f, 0.02f), 0f, carpisma: true);
        if (zeminAnahtari != null)
        {
            zeminAnahtari.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            // Anahtar küçük: nişan ıskalamasın diye çarpışma kutusu büyütülür (sahanlıktaki gibi).
            BoxCollider zc = zeminAnahtari.GetComponent<BoxCollider>();
            zc.center = new Vector3(0f, 0.02f, 0f);
            zc.size = new Vector3(0.12f, 0.06f, 0.14f);
            zeminAnahtari.AddComponent<KatAnahtari>();
        }

        // Oda lambası: loş, sıcak, gölgesiz tek bir ampul.
        GameObject lamba = new GameObject("Oda_Lambasi");
        lamba.transform.SetParent(o, false);
        lamba.transform.localPosition = new Vector3(ortaX, DuvarYuksek - 0.4f, YakinKapiZ);
        Light isik = lamba.AddComponent<Light>();
        isik.type = LightType.Point;
        isik.range = 4.5f;
        isik.intensity = 0f;      // #51: kapıcı odası da karanlık (Şahan); ışık nesnesi duruyor, ileride yakılabilir
        isik.color = new Color(1f, 0.78f, 0.5f);
        isik.shadows = LightShadows.None;

        // YÖNETİCİ NOTU: kapının dibinde, içeri itilmiş gibi yerde.
        GameObject not = Kutu("Yonetici_Notu", o, new Vector3(icX - 0.45f, 0.004f, YakinKapiZ + 0.15f), new Vector3(0.21f, 0.006f, 0.28f));
        not.transform.localRotation = Quaternion.Euler(0f, 17f, 0f);
        not.GetComponent<MeshRenderer>().sharedMaterial = kagit;
        not.AddComponent<YoneticiNotu>();

        // ---- BİNA GİRİŞİ: ön duvarda, oyuncuya göre asansörün solunda (+x) ----
        GameObject giris = new GameObject("Bina_Girisi");
        giris.transform.SetParent(z, false);
        giris.transform.localPosition = new Vector3(2.3f, 0f, -SahanlikDerin);
        Transform g = giris.transform;
        Kutu("Kasa_Sol", g, new Vector3(-0.64f, 1.13f, 0.03f), new Vector3(0.08f, 2.26f, 0.06f)).GetComponent<MeshRenderer>().sharedMaterial = metal;
        Kutu("Kasa_Sag", g, new Vector3(0.64f, 1.13f, 0.03f), new Vector3(0.08f, 2.26f, 0.06f)).GetComponent<MeshRenderer>().sharedMaterial = metal;
        Kutu("Kasa_Ust", g, new Vector3(0f, 2.3f, 0.03f), new Vector3(1.36f, 0.08f, 0.06f)).GetComponent<MeshRenderer>().sharedMaterial = metal;
        Kutu("Cam_Sol", g, new Vector3(-0.3f, 1.1f, 0.02f), new Vector3(0.58f, 2.2f, 0.02f)).GetComponent<MeshRenderer>().sharedMaterial = GeceMalzeme();
        Kutu("Cam_Sag", g, new Vector3(0.3f, 1.1f, 0.02f), new Vector3(0.58f, 2.2f, 0.02f)).GetComponent<MeshRenderer>().sharedMaterial = GeceMalzeme();
        Kutu("Kayit", g, new Vector3(0f, 1.1f, 0.035f), new Vector3(0.04f, 2.2f, 0.03f)).GetComponent<MeshRenderer>().sharedMaterial = metal;

        // ---- POSTA KUTULARI: arka duvarın sağ parçasında ----
        float cekirdekDis = CekirdekGen / 2f + DuvarKalin;
        GameObject posta = new GameObject("Posta_Kutulari");
        posta.transform.SetParent(z, false);
        posta.transform.localPosition = new Vector3(cekirdekDis + (disX - cekirdekDis) / 2f, 1.3f, -0.12f);
        for (int satir = 0; satir < 2; satir++)
            for (int sutun = 0; sutun < 4; sutun++)
                Kutu($"Kutu_{satir * 4 + sutun + 1}", posta.transform,
                     new Vector3(-0.45f + sutun * 0.3f, -0.18f + satir * 0.36f, 0f),
                     new Vector3(0.28f, 0.34f, 0.24f)).GetComponent<MeshRenderer>().sharedMaterial = metal;

        // ---- BODRUM KAPISI: aşağı inen merdivenin başını kapatır ----
        // Zemin kat binanın dibi; oyuncu açılışta aşağıdaki sahte katlara inmesin.
        // #81: eskiden kolun tamamını kapatan düz açık gri bir levhaydı; testte dokusuz bir yüzey gibi
        // göründü. Artık sıvalı bir set (malzemesi verilmez: DokuKurucu adından duvar dokusunu verir) ve
        // ortasında 90'lar bodrumlarının koyu yeşil boyalı, paslı demir kapısı. Çarpışma kutusu setin kendisi:
        // oyuncu yine geçemez.
        Kutu("Duvar_Bodrum", z, new Vector3(VarisKoluX, 1.1f, 0.14f), new Vector3(KolGen, 2.2f, 0.06f));   // adı "Duvar": sıva dokusu
        Material bodrumBoya = MalzemeBul("Assets/Materials/Bodrum_Kapi_Boya.mat", new Color(0.13f, 0.20f, 0.16f));
        KirKurucu.KirEkle(bodrumBoya, 1.2f);
        GameObject bodrumKanat = Kutu("Bodrum_Kanadi", z, new Vector3(VarisKoluX, 1.0f, 0.105f), new Vector3(0.84f, 2.0f, 0.03f));
        bodrumKanat.GetComponent<MeshRenderer>().sharedMaterial = bodrumBoya;
        Object.DestroyImmediate(bodrumKanat.GetComponent<Collider>());   // set zaten kapatıyor
        foreach (var kasa in new[] { (-0.44f, 1.02f, 0.04f, 2.04f), (0.44f, 1.02f, 0.04f, 2.04f), (0f, 2.03f, 0.92f, 0.04f) })
        {
            GameObject k = Kutu("Bodrum_Kasa", z, new Vector3(VarisKoluX + kasa.Item1, kasa.Item2, 0.1f), new Vector3(kasa.Item3, kasa.Item4, 0.05f));
            k.GetComponent<MeshRenderer>().sharedMaterial = metal;
            Object.DestroyImmediate(k.GetComponent<Collider>());
        }
        GameObject bodrumKol = Kutu("Bodrum_Kolu", z, new Vector3(VarisKoluX + 0.32f, 1.0f, 0.08f), new Vector3(0.12f, 0.03f, 0.04f));
        bodrumKol.GetComponent<MeshRenderer>().sharedMaterial = metal;
        Object.DestroyImmediate(bodrumKol.GetComponent<Collider>());
    }

    // ---------- DIŞ CEPHE ve FİNAL (#16) ----------
    // Final sahnesinin dış görüntüsü: merdiven boşluğundan çok uzakta (x = 400),
    // oyuncunun kamerasının hiçbir zaman görmediği bir yerde basit bir apartman,
    // iki alçak komşu, bir sokak ve final kamerası. Bina ZEMİN + 9 KAT: oyuncu
    // sekiz kat çıktı ama dışarıdan bakınca bir kat fazla görüyor. 9. katta tek bir
    // pencere yanıyor.
    //
    // Bina cephesi z = 0'da ve -z yönüne (kameraya) bakıyor.
    const int CepheKatSayisi = 9;
    const float CepheZemin = 3.6f;     // zemin katın yüksekliği (normal katlardan yüksek)
    const float CepheGen = 16f, CepheDerin = 12f;

    static void DisCepheKur()
    {
        GameObject kok = new GameObject("Final");
        kok.transform.position = new Vector3(400f, 0f, 0f);
        Transform f = kok.transform;

        Material beton = MalzemeBul("Assets/Materials/Cephe_Beton.mat", new Color(0.62f, 0.58f, 0.52f));
        Material bant = MalzemeBul("Assets/Materials/Cephe_Bant.mat", new Color(0.48f, 0.45f, 0.41f));
        Material cam = MalzemeBul("Assets/Materials/Cephe_Cam.mat", new Color(0.05f, 0.06f, 0.08f));
        Material komsu = MalzemeBul("Assets/Materials/Cephe_Komsu.mat", new Color(0.40f, 0.38f, 0.36f));
        // Asfalt gerçekte daha koyudur, ama sahnenin gece ton eğrisi 0,2'nin altını
        // simsiyah gösteriyor (0,09 ve 0,17 denendi: yol kadrajda bir delik gibiydi).
        Material asfalt = MalzemeBul("Assets/Materials/Asfalt.mat", new Color(0.28f, 0.28f, 0.29f));
        // MalzemeBul var olan dosyayı olduğu gibi döndürür; eski, daha koyu bir
        // asfalt dosyası kalmışsa rengini burada düzeltiyoruz.
        asfalt.SetColor("_BaseColor", new Color(0.28f, 0.28f, 0.29f));
        Material kaldirim = MalzemeBul("Assets/Materials/Kaldirim.mat", new Color(0.36f, 0.35f, 0.34f));
        Material metal = MalzemeBul("Assets/Materials/Metal_Gri.mat", new Color(0.42f, 0.44f, 0.45f));

        float ustKot = CepheZemin + CepheKatSayisi * KatYuksekligi;   // 30,6 m

        // ---- GÖVDE ----
        Boya(Kutu("Govde", f, new Vector3(0f, ustKot / 2f, CepheDerin / 2f), new Vector3(CepheGen, ustKot, CepheDerin)), beton);

        // ---- ZEMİN KAT: giriş, sundurma, tabela ----
        Boya(Kutu("Giris_Kapisi", f, new Vector3(0f, 1.2f, -0.03f), new Vector3(1.6f, 2.4f, 0.06f)), cam);
        Boya(Kutu("Sundurma", f, new Vector3(0f, 2.75f, -0.6f), new Vector3(2.6f, 0.12f, 1.2f)), bant);
        for (int i = 0; i < 4; i++)
        {
            float x = (i < 2 ? -1 : 1) * (3.2f + (i % 2) * 2.8f);
            Boya(Kutu($"Zemin_Pencere_{i + 1}", f, new Vector3(x, 1.7f, -0.03f), new Vector3(1.6f, 1.2f, 0.06f)), cam);
        }
        GameObject tabela = new GameObject("Tabela");
        tabela.transform.SetParent(f, false);
        tabela.transform.localPosition = new Vector3(0f, 3.15f, -0.05f);
        TextMesh ty = tabela.AddComponent<TextMesh>();
        ty.text = Dil.Getir("bina.tabela", DilKodu.TR);
        YerelYap(tabela, "bina.tabela");
        ty.anchor = TextAnchor.MiddleCenter;
        ty.fontSize = 96;
        ty.characterSize = 0.035f;
        ty.color = new Color(0.12f, 0.10f, 0.08f);
        ty.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tabela.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(ty.font);

        // ---- 9 NORMAL KAT ----
        // Her kat çizgisinde öne taşan bir bant: katları SAYMAYI kolaylaştırıyor,
        // sürprizin okunması buna bağlı.
        float[] pencereX = { -6f, -3f, 0f, 3f, 6f };
        Material isikliCam = IsikliPencereMalzeme();
        for (int kat = 1; kat <= CepheKatSayisi; kat++)
        {
            float y0 = CepheZemin + (kat - 1) * KatYuksekligi;
            Boya(Kutu($"Bant_{kat}", f, new Vector3(0f, y0, -0.08f), new Vector3(CepheGen + 0.3f, 0.22f, 0.16f)), bant);

            for (int p = 0; p < pencereX.Length; p++)
            {
                // 9. katın en sağdaki penceresi finalin sorusu. Önce KARANLIK: final sahnesi
                // asansörün sesinden sonra onu yakar (#68). Işıklı malzeme FinalSahnesi'ne bağlanır.
                Boya(Kutu($"Pencere_{kat}_{p + 1}", f, new Vector3(pencereX[p], y0 + 1.65f, -0.03f), new Vector3(1.3f, 1.5f, 0.06f)), cam);
                Boya(Kutu($"Denizlik_{kat}_{p + 1}", f, new Vector3(pencereX[p], y0 + 0.87f, -0.08f), new Vector3(1.5f, 0.06f, 0.16f)), bant);
            }

            // İki balkon: -3 ve +3'teki pencerelerin önünde döşeme ve korkuluk.
            foreach (float bx in new[] { -3f, 3f })
            {
                Boya(Kutu($"Balkon_{kat}_{bx}", f, new Vector3(bx, y0 + 0.06f, -0.55f), new Vector3(2.2f, 0.12f, 1.1f)), bant);
                Boya(Kutu($"Korkuluk_{kat}_{bx}", f, new Vector3(bx, y0 + 0.55f, -1.07f), new Vector3(2.2f, 0.9f, 0.05f)), metal);
            }
        }

        // ---- ÇATI: korkuluk, merdiven evi, su deposu, anten ----
        Boya(Kutu("Cati_Korkuluk", f, new Vector3(0f, ustKot + 0.5f, 0.1f), new Vector3(CepheGen, 1f, 0.2f)), bant);
        Boya(Kutu("Merdiven_Evi", f, new Vector3(-4f, ustKot + 1.4f, 6f), new Vector3(4f, 2.8f, 4f)), beton);
        GameObject depo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        depo.name = "Su_Deposu";
        depo.transform.SetParent(f, false);
        depo.transform.localPosition = new Vector3(3.5f, ustKot + 1.0f, 5f);
        depo.transform.localScale = new Vector3(1.6f, 1.0f, 1.6f);
        Boya(depo, metal);
        Boya(Kutu("Anten", f, new Vector3(-4f, ustKot + 4.3f, 6f), new Vector3(0.05f, 3f, 0.05f)), metal);
        Boya(Kutu("Anten_Kol", f, new Vector3(-4f, ustKot + 5.2f, 6f), new Vector3(1.2f, 0.04f, 0.04f)), metal);

        // ---- KOMŞULAR: alçak, koyu; bizim bina yanlarında yükseliyor ----
        Boya(Kutu("Komsu_Sol", f, new Vector3(-CepheGen / 2f - 5f, 6.3f, CepheDerin / 2f + 0.5f), new Vector3(10f, 12.6f, CepheDerin)), komsu);
        Boya(Kutu("Komsu_Sag", f, new Vector3(CepheGen / 2f + 5f, 7.8f, CepheDerin / 2f + 0.5f), new Vector3(10f, 15.6f, CepheDerin)), komsu);

        // ---- SOKAK ----
        Boya(Kutu("Kaldirim", f, new Vector3(0f, 0.07f, -2f), new Vector3(80f, 0.14f, 4f)), kaldirim);
        // Yol 10 m: iki şerit. İlk denemede 52 m derinliğinde bir asfalt vardı; alçak
        // şafak güneşi yatay yüzeyi çok az aydınlattığı için kadrajın altı simsiyah
        // bir boşluk gibi görünüyordu.
        Boya(Kutu("Yol", f, new Vector3(0f, -0.05f, -9f), new Vector3(120f, 0.1f, 10f)), asfalt);
        for (int serit = -9; serit <= 9; serit++)   // kesik orta şerit: 2 m çizgi, 2 m boşluk
            Boya(Kutu("Serit_" + (serit + 10), f, new Vector3(serit * 4f, 0.005f, -9f), new Vector3(2f, 0.01f, 0.14f)), kaldirim);
        Boya(Kutu("Kaldirim_Karsi", f, new Vector3(0f, 0.07f, -15.5f), new Vector3(80f, 0.14f, 3f)), kaldirim);

        // Karşı kaldırımın ardında boş bir arsa: sıkışmış açık renkli toprak. Kamera
        // son duruşunda buradan (z = -52) binaya bakıyor; açık renk, alçak güneşte
        // bile kadrajın altını karanlık bir delik olmaktan çıkarıyor.
        Boya(Kutu("Bos_Arsa", f, new Vector3(0f, -0.02f, -48f), new Vector3(120f, 0.1f, 62f)),
             MalzemeBul("Assets/Materials/Toprak.mat", new Color(0.46f, 0.40f, 0.32f)));
        Boya(Kutu("Lamba_Diregi", f, new Vector3(-6f, 2.6f, -3.6f), new Vector3(0.12f, 5.2f, 0.12f)), metal);
        GameObject sokakIsigi = new GameObject("Sokak_Lambasi");
        sokakIsigi.transform.SetParent(f, false);
        sokakIsigi.transform.localPosition = new Vector3(-6f, 5.1f, -3.2f);
        Light si = sokakIsigi.AddComponent<Light>();
        si.type = LightType.Point;
        si.range = 12f;
        si.intensity = 2.5f;
        si.color = new Color(1f, 0.72f, 0.42f);
        si.shadows = LightShadows.None;

        // ---- FİNAL KAMERASI: başta kapalı; final başlayınca açılır ----
        GameObject kam = new GameObject("Final_Kamera");
        kam.transform.SetParent(f, false);
        Camera c = kam.AddComponent<Camera>();
        c.fieldOfView = 60f;
        c.farClipPlane = 600f;
        UnityEngine.Rendering.Universal.UniversalAdditionalCameraData veri =
            kam.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        veri.renderPostProcessing = true;
        veri.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        kam.AddComponent<AudioListener>();

        // Kameranın kulağında sabah sesi: iki boyutlu, sokağın sesi.
        AudioSource sokak = kam.AddComponent<AudioSource>();
        sokak.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ortam/Ortam_Sabah.wav");
        sokak.loop = true;
        sokak.playOnAwake = true;          // kamera açılınca çalmaya başlar
        sokak.spatialBlend = 0f;
        sokak.volume = 0.7f;
        kam.SetActive(false);

        FinalSahnesi final = kok.AddComponent<FinalSahnesi>();
        // Dokuzuncu katın penceresi, ışığı ve asansörün sesi (#68). SerializedObject: bileşenin
        // özel (private) kayıtlı alanlarını koddan doldurmanın yolu.
        var seri = new SerializedObject(final);
        Transform pencere = f.Find($"Pencere_{CepheKatSayisi}_5");
        seri.FindProperty("dokuzuncuPencere").objectReferenceValue = pencere != null ? pencere.GetComponent<MeshRenderer>() : null;
        seri.FindProperty("pencereIsigi").objectReferenceValue = isikliCam;
        seri.FindProperty("asansorSesi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Asansor_Ding.wav");
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Boya(GameObject g, Material m)
    {
        g.GetComponent<MeshRenderer>().sharedMaterial = m;
    }

    // 9. kattaki yanan pencere: ışıktan etkilenmeyen (Unlit), 1'in üstünde parlak
    // sıcak bir renk. Bloom efekti etrafında hafif bir hale oluşturur.
    static Material IsikliPencereMalzeme()
    {
        const string Yol = "Assets/Materials/Pencere_Isik.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;

        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        yeni.SetColor("_BaseColor", new Color(1f, 0.72f, 0.40f) * 2.2f);
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }

    // ---------- ORTAM SESLERİ (#14) ----------
    // Bir sahanlığın ses katmanları. Hepsi sahanlığın (ya da bir kapının) çocuğu,
    // yani anomali kartları onları da yolla hedefleyebilir: "Kapi_2/Ses_TV" gibi.
    // Bir sesi susturmak = o nesneyi gizlemek; sesi başka yere taşımak = konumunu
    // değiştirmek. Ses bozulmaları da kod değil, kart verisi.
    //
    // Ses dosyaları Art/Ses/scripts/ortam_uret.py ile kodla üretiliyor.
    static void OrtamSesleriKur(Transform sahanlik)
    {
        float sol = -(SahanlikGen / 2f + DuvarKalin), sag = SahanlikGen / 2f + DuvarKalin;

        // Uğultu: tavanın ortasından; bütün sahanlığı dolduracak kadar geniş menzil.
        OrtamKaynagi("Ses_Ugultu", sahanlik, new Vector3(0f, 2.4f, -SahanlikDerin / 2f),
                     "Ortam_Ugultu", 0.25f, 4f, 9f);

        // Uzak TV: 2 numaranın İÇİNDEN, holün 80 cm gerisinden.
        OrtamKaynagi("Ses_TV", sahanlik.Find("Kapi_2"), new Vector3(sol - 0.8f, 1.2f, 0f),
                     "Ortam_TV", 0.5f, 1.5f, 9f);

        // ---- KAT OLAYLARI (#35) ----
        // Döngüde DEĞİL: her katta yalnızca biri, KatYoneticisi'nin seçtiği anlarda bir iki
        // kez çalar (KatOlayPlani + KatOlayCalici). Önce bebek her katta 30 sn'de bir
        // ağlıyordu ve uzak olaylar uğultunun içindeydi; Şahan build'de "sürekli olması
        // rahatsız ediyor" dedi.
        // Bebek: 4 numaranın içinden. Yolu değişmedi; bebek kartları onu hedefliyor.
        OlayKaynagi("Ses_Bebek", sahanlik.Find("Kapi_4"), new Vector3(sag + 0.8f, 1.0f, 0f),
                    "Ortam/Ortam_Bebek", 0.55f, 1.5f, 10f);

        // Uzak kapı, kalorifer ve araba olayları ile boru katmanı #74'te kalktı (Şahan'ın ses
        // kararları). Üst kat adımları artık Gezen Gölge'nin ve 7. kattaki kaçışın habercisi.
    }

    // Tek seferlik bir olay sesi kaynağı (#35): üç boyutlu, döngüsüz, kendiliğinden çalmaz.
    // Çalma zamanını KatOlayCalici belirler. klip: Assets/Audio altındaki yol, uzantısız.
    static void OlayKaynagi(string ad, Transform ebeveyn, Vector3 konum, string klip,
                            float ses, float yakin, float uzak)
    {
        if (ebeveyn == null) return;
        AudioClip kayit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + klip + ".wav");
        if (kayit == null) { Debug.LogWarning("[GRAYBOX] Olay sesi bulunamadı: " + klip); return; }

        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        AudioSource kaynak = g.AddComponent<AudioSource>();
        kaynak.clip = kayit;
        kaynak.loop = false;               // bir kez çalar ve susar
        kaynak.playOnAwake = false;        // sahne açılınca çalmasın; zamanı KatOlayCalici seçer
        kaynak.volume = ses;
        kaynak.spatialBlend = 1f;
        kaynak.rolloffMode = AudioRolloffMode.Linear;
        kaynak.minDistance = yakin;
        kaynak.maxDistance = uzak;
    }

    // Tek bir ortam sesi kaynağı: döngüde çalan, üç boyutlu bir AudioSource ve
    // onu diğer kopyalarla aynı saate kilitleyen OrtamSesi bileşeni.
    //   ses:      ses şiddeti (0-1)
    //   yakin:    bu mesafeye kadar tam şiddet (metre)
    //   uzak:     bu mesafede tamamen sessiz; aradaki mesafede düzgün azalır
    static void OrtamKaynagi(string ad, Transform ebeveyn, Vector3 konum, string klip,
                             float ses, float yakin, float uzak)
    {
        if (ebeveyn == null) return;
        AudioClip kayit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ortam/" + klip + ".wav");
        if (kayit == null) return;

        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        AudioSource kaynak = g.AddComponent<AudioSource>();
        kaynak.clip = kayit;
        kaynak.loop = true;
        kaynak.playOnAwake = false;        // çalmayı OrtamSesi başlatır, saate kilitleyerek
        kaynak.volume = ses;
        kaynak.spatialBlend = 1f;          // tamamen üç boyutlu: yönü ve uzaklığı duyulur
        kaynak.rolloffMode = AudioRolloffMode.Linear;
        kaynak.minDistance = yakin;
        kaynak.maxDistance = uzak;

        g.AddComponent<OrtamSesi>();
    }

    // ---------- BLENDER MODELLERİ (#24) ----------
    // Eşyaların modelleri Art/Blender/scripts altındaki betiklerle üretilip
    // Assets/Models/Props'a FBX olarak konuyor.
    const string ModelKlasoru = "Assets/Models/Props/";

    // Bir FBX'teki tek bir parçayı sahneye, verilen isimle koyar.
    //   ad:       sahnedeki isim (anomali kartlarının hedef aldığı isim)
    //   dosya:    FBX dosyasının adı (uzantısız)
    //   parca:    FBX içindeki parçanın adı; tek parçalı dosyada null
    //   konum:    ebeveyne göre yerel konum
    //   yDonus:   dikey eksen etrafında dönüş (ön yüzü çevirmek için)
    //   carpisma: oyuncu içinden geçemesin diye kutu şeklinde çarpışma eklensin mi
    // FBX bulunamazsa null döner; çağıran taraf o zaman eski beyaz kutuyu koyar.
    //
    // NEDEN FBX'İ OLDUĞU GİBİ SAHNEYE KOYMUYORUZ
    // Asansörün parçaları "Asansor/Kapi_Sol" yolunda durmalı (kartlar bu yolu hedefliyor).
    // FBX olduğu gibi konsaydı araya dosyanın kendi kök nesnesi girer ve yol
    // "Asansor/Asansor/Kapi_Sol" olurdu. Parçanın meshini ve malzemelerini kendi
    // isimlendirdiğimiz nesneye kopyalamak yolu bizim kontrolümüzde tutuyor.
    static GameObject Model(string ad, Transform ebeveyn, string dosya, string parca,
                            Vector3 konum, float yDonus, bool carpisma)
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModelKlasoru + dosya + ".fbx");
        if (fbx == null) return null;

        // Parçayı bul: adı verildiyse ÖNCE FBX'in içinde ara. Önce içeride aramak şart:
        // sayaç dosyasında hem dosyanın kök nesnesi hem içindeki parça "Elektrik_Sayaci"
        // adını taşıyor; kökü seçseydik (kökte mesh yok) model sessizce kutuya dönerdi.
        // Ad verilmediyse, tek parçalı dosyada mesh kökün kendisindedir.
        Transform kaynak = parca == null ? fbx.transform : fbx.transform.Find(parca);
        if (kaynak == null && parca == fbx.name) kaynak = fbx.transform;
        if (kaynak == null) return null;

        MeshFilter kaynakMesh = kaynak.GetComponent<MeshFilter>();
        if (kaynakMesh == null) return null;

        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);

        // Parçanın FBX içindeki kendi konumu da eklenir: sayaç kapağının pivotu
        // menteşede olduğu için FBX'te kutunun yanında, kaydırılmış duruyor.
        bool kokParca = kaynak == fbx.transform;
        g.transform.localPosition = konum + (kokParca ? Vector3.zero : kaynak.localPosition);
        g.transform.localRotation = Quaternion.Euler(0f, yDonus, 0f) *
                                    (kokParca ? Quaternion.identity : kaynak.localRotation);

        // sharedMesh / sharedMaterials: kopya üretmeden aynı meshi ve malzemeleri
        // paylaş. Her sahanlıkta ayrı kopya üretseydik bellek boşa harcanırdı.
        g.AddComponent<MeshFilter>().sharedMesh = kaynakMesh.sharedMesh;
        g.AddComponent<MeshRenderer>().sharedMaterials = kaynak.GetComponent<MeshRenderer>().sharedMaterials;

        // BoxCollider eklendiği anda kendini meshin sınırlarına göre boyutlandırır.
        if (carpisma) g.AddComponent<BoxCollider>();

        return g;
    }

    // Kapıların önündeki ek eşyalar. Hepsi bir kapının çocuğu, böylece yolları
    // "Kapi_N/..." biçiminde ve kapının kendi yerel eksenlerinde.
    //
    // GÖRÜNMEZ BAŞLAYANLAR: tüp, gazete ve tek ayakkabı normal bir katta yoktur.
    // #8'deki "olmaması gereken bir şey" anomalileri (F29, F31, T15) bunları
    // görünür yaparak kurulacak. Nesne sahnede hazır beklediği için o kartlar
    // tek satır veriyle yazılabilecek.
    static void KapiEkEsyalari(Transform sahanlik)
    {
        // Sol duvar kapıları -x duvarında, sağ duvardakiler +x'te.
        float sol = -(SahanlikGen / 2f), sag = SahanlikGen / 2f;

        // Saksı: 1 numaranın merdiven tarafında, duvara yakın. Normalde VAR (F30 onu gizler).
        // #44: saksı eskiden 1 numaranın arka tarafındaydı; orası artık poşetlerin yeri.
        // İki kapının arasındaki iki ayakkabılığın ortasında (1 numaraya göre z = -1,0).
        Model("Saksi", sahanlik.Find("Kapi_1"), "Saksi", null,
              new Vector3(sol + 0.18f, 0f, -1.0f), 0f, true);

        // Tüp: 2 numaranın önünde, kapıdan 75 cm uzakta. Normalde YOK (F29).
        GameObject tup = Model("Tup", sahanlik.Find("Kapi_2"), "Tup", null,
                               new Vector3(sol + 0.75f, 0f, 0f), 0f, true);
        if (tup != null) tup.SetActive(false);

        // Gazete: 3 numaranın paspasının üstünde. Normalde YOK (F31).
        GameObject gazete = Model("Gazete", sahanlik.Find("Kapi_3"), "Gazete", null,
                                  new Vector3(sag - 0.30f, 0.016f, 0.04f), 12f, false);
        if (gazete != null) gazete.SetActive(false);

        // Tek erkek ayakkabısı: 4 numaranın önünde, burnu kapıya dönük. Normalde YOK (T15).
        GameObject ayakkabi = Model("Ayakkabi", sahanlik.Find("Kapi_4"), "Ayakkabi", null,
                                    new Vector3(sag - 0.45f, 0f, -0.2f), -90f, false);
        if (ayakkabi != null) ayakkabi.SetActive(false);
    }

    // ---------- BEŞİNCİ KAPI (#13, K02) ----------
    // Her katta dört daire var. Bu kapı ön duvarda, oyuncuya göre asansörün SAĞINDA
    // (dünyanın -x tarafı; oyuncu -z yönüne bakıyor) belirir ve
    // normal bir katta HİÇ görünmez; bir KÂBUS kartı onu görünür yaparak oyuncuya
    // "bu binada olmaması gereken bir daire" gösterir. Numara plakası bilerek BOŞ.
    //
    // Duvarda boşluk açılmıyor: kapı kasasıyla birlikte duvarın yüzüne oturuyor.
    // Gizliyken duvar kesintisiz görünmeli; boşluk açsaydık normal katlarda da
    // duvarda bir delik olurdu.
    static void BesinciKapiKur(Transform sahanlik)
    {
        const float Gen = 0.9f, Yuk = 2.05f;

        GameObject kok = new GameObject("Kapi_5");
        kok.transform.SetParent(sahanlik, false);
        kok.transform.localPosition = new Vector3(-2.3f, 0f, -SahanlikDerin);   // ön duvarın iç yüzü
        Transform k = kok.transform;

        // Kasa: iki dikme ve üst başlık, duvardan 5 cm taşar.
        Kutu("Kasa_Sol", k, new Vector3(-(Gen / 2f + 0.03f), (Yuk + 0.06f) / 2f, 0.025f), new Vector3(0.06f, Yuk + 0.06f, 0.05f));
        Kutu("Kasa_Sag", k, new Vector3(Gen / 2f + 0.03f, (Yuk + 0.06f) / 2f, 0.025f), new Vector3(0.06f, Yuk + 0.06f, 0.05f));
        Kutu("Kasa_Ust", k, new Vector3(0f, Yuk + 0.03f, 0.025f), new Vector3(Gen + 0.12f, 0.06f, 0.05f));

        // Kanat, kol (oyuncuya göre sağda, yani -x), boş numara plakası ve kapı gözü.
        Kutu("Kanat", k, new Vector3(0f, Yuk / 2f, 0.02f), new Vector3(Gen, Yuk, 0.04f));
        Kutu("Kol", k, new Vector3(-0.33f, 1.05f, 0.07f), new Vector3(0.12f, 0.04f, 0.06f));
        Kutu("Numaralik", k, new Vector3(0f, 1.78f, 0.05f), new Vector3(0.26f, 0.16f, 0.02f));
        Kutu("Kapi_Gozu", k, new Vector3(0f, 1.60f, 0.055f), new Vector3(0.05f, 0.05f, 0.03f));

        kok.SetActive(false);
    }

    // Adı, ebeveyni, yerel konumu ve ölçeği verilen bir küp üretir.
    // ---------- DAİRE KAPISININ ÇITALARI (#81) ----------
    // Şahan'ın seçimi: "ceviz kaplama çıtalı kapı". Kanadın iki yüzünde ikişer tabla (üst ve alt dikdörtgen),
    // her biri dört ince çıtadan. Çıtalar menteşenin çocuğu: kapı açılınca kanatla birlikte döner. Adları
    // "Kasa_Cita": DokuKurucu "Kasa" ile başlayan her şeye kapının ceviz kaplamasını verir. Çarpışmaları
    // silinir: kanadın kendi kutusu yeter, 1 cm'lik çıtalar oyuncuyu takılmasın.
    static void KapiCitalari(Transform m, float yon, float kanatZ)
    {
        const float Cikinti = 0.012f, Kalin = 0.035f;
        float genislik = KapiGen - 0.24f;                       // kenarlardan 12'şer cm içeride
        var tablalar = new[] { (0.18f, 0.98f), (1.10f, 1.95f) }; // alt ve üst tabla (y aralığı)
        foreach (float yuz in new[] { -1f, 1f })                // -yon: sahanlık yüzü, +yon: daire yüzü
        {
            float x = yuz * yon * (0.03f + Cikinti / 2f);
            foreach (var t in tablalar)
            {
                float yOrta = (t.Item1 + t.Item2) / 2f, yBoy = t.Item2 - t.Item1;
                var citalar = new[] {
                    (new Vector3(x, t.Item1, kanatZ), new Vector3(Cikinti, Kalin, genislik)),
                    (new Vector3(x, t.Item2, kanatZ), new Vector3(Cikinti, Kalin, genislik)),
                    (new Vector3(x, yOrta, kanatZ - genislik / 2f), new Vector3(Cikinti, yBoy, Kalin)),
                    (new Vector3(x, yOrta, kanatZ + genislik / 2f), new Vector3(Cikinti, yBoy, Kalin)) };
                foreach (var c in citalar)
                {
                    GameObject cita = Kutu("Kasa_Cita", m, c.Item1, c.Item2);
                    Object.DestroyImmediate(cita.GetComponent<Collider>());
                }
            }
        }
    }

    // Poly Haven mobilyasının prefab'ından sahneye kalıcı bir kopya koyar (#81, kapıcı odası). Kat mobilyaları
    // oyun sırasında kat dosyasından konur (GrayboxKurucu_Mobilya); zemin kat her zaman aynı olduğu için bunlar
    // sahneye doğrudan yerleşir. Prefab yoksa (Mobilyaları Kur çalışmadıysa) uyarır ve geçer.
    static GameObject MobKoy(string ad, Transform ebeveyn, string kimlik, Vector3 konum, float yDonus, Vector3 olcek)
    {
        GameObject sablon = Mob(kimlik);
        if (sablon == null) return null;
        GameObject g = (GameObject)PrefabUtility.InstantiatePrefab(sablon, ebeveyn);
        g.name = ad;
        g.transform.localPosition = konum;
        g.transform.localRotation = Quaternion.Euler(0f, yDonus, 0f);
        g.transform.localScale = olcek;
        return g;
    }

    static GameObject Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 olcek)
    {
        // CreatePrimitive hazır bir küp yapar: mesh, malzeme ve çarpışma kutusu dahil.
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ad;

        // SetParent(..., false): "yerel konumunu koru" demektir.
        g.transform.SetParent(ebeveyn, false);

        g.transform.localPosition = konum;
        g.transform.localScale = olcek;
        return g;
    }

    // İki nokta arasında uzanan ince bir çubuk (eğik tırabzan için).
    // Quaternion.LookRotation(yon): nesnenin ileri (z) eksenini verilen yöne çevirir.
    // Kutunun z ölçüsünü iki nokta arasındaki mesafe yapınca çubuk tam uçtan uca oturur.
    static void EgimliKutu(string ad, Transform ebeveyn, Vector3 a, Vector3 b, float kalinlik)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ad;
        g.transform.SetParent(ebeveyn, false);

        Vector3 fark = b - a;
        g.transform.localPosition = (a + b) / 2f;
        g.transform.localRotation = Quaternion.LookRotation(fark);
        g.transform.localScale = new Vector3(kalinlik, kalinlik, fark.magnitude);
    }

    // ---------- KİLİTLİ KAT KAPISI (#52) ----------
    // Demir parmaklıklı kapı (kat_kapisi.py). Menteşe duvar tarafında (x = kolun dış kenarı),
    // kanat merdiven gözüne doğru (-X) uzanır ve açılınca merdivene (+Z, ara sahanlığa) döner.
    // Kolun DİBİNE değil TEPESİNE konuyor: dipte açılan kanat basamaklara çarpardı; ara
    // sahanlık düz ve 1,4 m derin, 1,3 m'lik kanat oraya rahat açılır. Oyuncu sekiz basamak
    // çıkıp dönüşte parmaklığa dayanır, üst kolun karanlığını çubukların arasından görür.
    static void KatKapisiKur(Transform cekirdek)
    {
        GameObject kok = new GameObject("Kat_Kapisi");
        kok.transform.SetParent(cekirdek, false);
        kok.transform.localPosition = new Vector3(CikisKoluX + KolGen / 2f - 0.02f, AraKot, KolUzunluk + 0.05f);

        GameObject kasa = Model("Kasa", kok.transform, "Kat_Kapisi", "Kasa", Vector3.zero, 0f, carpisma: false);
        GameObject kanat = Model("Kanat", kok.transform, "Kat_Kapisi", "Kanat", Vector3.zero, 0f, carpisma: false);
        if (kasa == null || kanat == null) { Debug.LogWarning("[GRAYBOX] Kat_Kapisi.fbx bulunamadı; kat kapısı kurulmadı."); return; }

        // Kanadın çarpışma kutusu kanatla döner: kapalıyken geçit kapalı, açıkken serbest.
        BoxCollider kc = kanat.AddComponent<BoxCollider>();
        kc.center = new Vector3(-KolGen / 2f + 0.05f, 1.05f, 0f);
        kc.size = new Vector3(KolGen - 0.1f, 2.1f, 0.08f);
        // Kasanın dikmeleri de dursun (ince ama oyuncu içinden geçmesin).
        foreach (float x in new[] { -0.03f, -(KolGen - 0.03f) })
        {
            BoxCollider dc = kasa.AddComponent<BoxCollider>();
            dc.center = new Vector3(x, 1.08f, 0f);
            dc.size = new Vector3(0.06f, 2.16f, 0.07f);
        }

        KatKapisi bilesen = kok.AddComponent<KatKapisi>();
        var seri = new SerializedObject(bilesen);
        seri.FindProperty("kanat").objectReferenceValue = kanat.transform;
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- KATIN ANAHTARI (#52) ----------
    // Şimdilik her katta aynı yerde: 1 numaranın ayakkabılığının üstünde. #55'te her katın
    // içerik dosyası anahtarın yerini (paspas altı, sayaç dolabı, asansör...) söyleyecek.
    static void KatAnahtariKur(Transform sahanlik)
    {
        float sol = -(SahanlikGen / 2f);
        GameObject anahtar = Model("Kat_Anahtari", sahanlik, "Anahtar", null,
                                   new Vector3(sol + RafDerin / 2f + 0.01f, 1.025f, YakinKapiZ - (KapiGen / 2f + 0.04f + RafEn / 2f)), 35f, carpisma: true);
        if (anahtar == null) return;
        // Anahtar 4 x 9 cm: nişan ışını ıskalamasın diye çarpışma kutusu büyütülüyor.
        BoxCollider c = anahtar.GetComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.02f, 0f);
        c.size = new Vector3(0.16f, 0.06f, 0.16f);
        anahtar.AddComponent<KatAnahtari>();
    }

    // ---------- LAMBA (#11) ----------
    // Tavandan sarkan çıplak bir akkor ampul. Üç parçadan oluşur:
    //
    //   Ampul        aşağı bakan SPOT ışık, gölge atar. Kartların hedefi bu nesne.
    //   Ampul/Dolgu gölgesiz, zayıf NOKTA ışık: tavanı ve duvarların üstünü aydınlatır.
    //   Ampul/Cam    parlayan cam; bloom efekti etrafında hale oluşturur.
    //
    // NEDEN SPOT + DOLGU, NEDEN TEK NOKTA IŞIK DEĞİL
    // Gölge, ışığın "gözünden" sahnenin bir kez daha çizilmesidir (gölge haritası).
    // Nokta ışık her yöne baktığı için bunu ALTI kez yapar; spot ışık tek yöne
    // baktığı için BİR kez. Sahnede on dört lamba var: nokta ışıkla gölge
    // performansı öldürürdü (#6'da beş gölgeli nokta ışık GPU'yu tavana vurmuştu).
    // Aşağı bakan geniş bir spot, zemini ve eşyaları gölgeli aydınlatır; spotun
    // göremediği tavanı da gölgesiz dolgu ışığı kurtarır.
    //
    // HER KATTA AYNI: bütün lambalar (sahte katlardakiler dahil) birebir aynı
    // kurulur. Döngü hilesi her katın aynı aydınlanmasını istiyor; bazı lambalar
    // farklı olsaydı ışınlanma anında aydınlık değişirdi.
    //
    // DİKKAT: ışık kartlarındaki şiddet MUTLAK değerdir (kart "yarıya indir" değil
    // "0,5 yap" der). Buradaki şiddetler değişirse ampul kartları da güncellenmeli.
    // İlk denemede 2,6 idi ve sahanlık zifiri karanlıktı: ortam ışığı kalkınca
    // her şeyi lamba taşımak zorunda. Işık uzaklığın KARESİYLE zayıflar; tavandaki
    // ampulden 4 m ötedeki köşeye şiddetin ancak onda biri ulaşır. Kapı numaraları
    // ve terlikler okunmalı (anomaliler onlarda), o yüzden şiddet bunu sağlayacak kadar.
    // #38 (2026-09-26): Şahan "ışıklar çok az yansın" dedi; 10'dan 7'ye indi. Işık
    // kartlarındaki mutlak şiddetler de aynı oranla (0,7) güncellendi. 5,5 denendi:
    // görünürlük denetiminde görünmez kart 6'dan 10'a çıktı (yerdeki terlikler, yan
    // kapılar karanlıkta kayboldu); karanlığın geri kalanı ortam ışığı ve vinyetten geliyor.
    // #51 (2026-09-26): bina karanlık, ışığı kafa lambası taşıyor; sahanlık ampulü 7'den 2,5'e
    // indi (sönük, titrek bir ampul), ışık kartları yine aynı oranla (0,357) ölçeklendi.
    public const float LambaSiddeti = 2.5f;       // spotun şiddeti
    public const float DolguOrani   = 0.30f;      // dolgu ışığı = spotun bu kadarı

    static readonly Color AkkorRengi = new Color(1f, 0.82f, 0.58f);   // 2700 K civarı sıcak sarı

    // yanik = false: ampul yerinde ama sönük; ışıklar 0 şiddette AÇIK kalır (kartlar ve görevler
    // şiddet yazarak yakabilir), camı karanlık malzemeyi kullanır.
    static void AmpulKur(string ad, Transform ebeveyn, Vector3 konum, float menzil, float siddet, bool yanik = true)
    {
        if (!yanik) siddet = 0f;
        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        // Spot ışık yerel +z yönüne bakar. x ekseninde 90° döndürünce aşağı bakar.
        g.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Light spot = g.AddComponent<Light>();
        spot.type = LightType.Spot;
        // Dış açı: neredeyse yarım küre. 150° ile kapı numara plakaları (ampule göre
        // ~79° açıda) koninin DIŞINDA kalıyor ve okunmuyordu; "ters numara" anomalisi
        // tam o plakada olduğu için plakalar aydınlık olmak zorunda.
        spot.spotAngle = 170f;
        // İç açı: bu koninin içinde tam şiddet, dışına doğru azalır. İlk denemede 70°
        // idi; kapılar ampule göre ~67° açıda kaldığı için azalma bölgesine düşüp
        // karanlıkta kalıyordu. 120° ile ışık sahanlığa daha eşit yayılıyor.
        spot.innerSpotAngle = 130f;
        spot.range = menzil;
        spot.intensity = siddet;
        spot.color = AkkorRengi;
        spot.shadows = LightShadows.Soft;
        spot.shadowStrength = 0.92f;
        spot.shadowNearPlane = 0.08f;       // camın kendisi gölge atmasın diye çok yakını yok say

        // DOLGU: spotun göremediği tavanı aydınlatır. Gölge atmaz.
        GameObject dolgu = new GameObject("Dolgu");
        dolgu.transform.SetParent(g.transform, false);
        Light nokta = dolgu.AddComponent<Light>();
        nokta.type = LightType.Point;
        nokta.range = menzil * 0.7f;
        nokta.intensity = siddet * DolguOrani;
        nokta.color = AkkorRengi;
        nokta.shadows = LightShadows.None;

        // CAM: parlak, ışık hesabından etkilenmeyen (Unlit) küçük bir küre.
        // Rengi 1'in çok üstünde (HDR): bloom yalnızca eşiği aşan pikselleri parlatır.
        GameObject cam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cam.name = "Cam";
        cam.transform.SetParent(g.transform, false);
        cam.transform.localScale = Vector3.one * 0.07f;
        Object.DestroyImmediate(cam.GetComponent<Collider>());   // oyuncu ampule takılmasın
        MeshRenderer camCizici = cam.GetComponent<MeshRenderer>();
        camCizici.sharedMaterial = yanik ? CamMalzeme() : CamSonukMalzeme();
        camCizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // ışığın önünü kesmesin

        // Normal kattaki hafif titreme (#38): bütün lambalar aynı saatle, birlikte titrer.
        g.AddComponent<LambaTitremesi>();

        // KABLO: ampulden tavana ince bir kordon. Dönmüş nesnenin çocuğu olduğu
        // için dünya konumu ve dönüşü ayrıca veriliyor (kordon hep dik dursun).
        GameObject kablo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        kablo.name = "Kablo";
        kablo.transform.SetParent(g.transform, false);
        kablo.transform.position = g.transform.position + Vector3.up * 0.14f;
        kablo.transform.rotation = Quaternion.identity;
        kablo.transform.localScale = new Vector3(0.012f, 0.11f, 0.012f);   // silindir 2 birim boyunda: 0,22 m
        Object.DestroyImmediate(kablo.GetComponent<Collider>());
        MeshRenderer kabloCizici = kablo.GetComponent<MeshRenderer>();
        kabloCizici.sharedMaterial = KaranlikMalzeme();
        kabloCizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // Dünyadaki 3B yazıların malzemesi (#11). Yazı tipinin kendi malzemesi ekran
    // arayüzü için yapılmış ve derinliğe bakmıyor: yazı duvarların içinden görünüyordu
    // (bir üst kattaki asansör notu tavandan, kapı numaraları ceza karanlığında).
    // Assets/Shaders/Yazi3B.shader aynı işi derinliğe saygı göstererek yapıyor.
    static Material YaziMalzeme(Font yaziTipi)
    {
        const string Yol = "Assets/Materials/Yazi_3B.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Huzur/Yazi3B"));
            AssetDatabase.CreateAsset(m, Yol);
        }
        // Harf dokusu yazı tipinin kendi dokusu. Her kurulumda yeniden bağlanıyor:
        // yazı tipi dinamik, dokusu yeni harf istendikçe yeniden üretilebiliyor.
        m.mainTexture = yaziTipi.material.mainTexture;
        EditorUtility.SetDirty(m);
        return m;
    }

    // Ampul camının malzemesi: URP'nin ışıktan etkilenmeyen (Unlit) gölgelendiricisi.
    // Sönük ampulün camı: mat, koyu gri; içinden ışık gelmez.
    static Material CamSonukMalzeme()
    {
        const string Yol = "Assets/Materials/Ampul_Cam_Sonuk.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;

        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        yeni.SetColor("_BaseColor", new Color(0.35f, 0.33f, 0.30f));
        yeni.SetFloat("_Smoothness", 0.6f);
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }

    static Material CamMalzeme()
    {
        const string Yol = "Assets/Materials/Ampul_Cam.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;

        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        yeni.SetColor("_BaseColor", AkkorRengi * 6f);   // HDR: 6 kat parlak
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }

    // Kapının altından sızan ışık çizgisi (7. kat): ampul camının rengi, 6 değil 1,5 kat parlak.
    // Dosya zaten varsa rengi yeniden yazılır, böylece bu satırdaki ayar her kurulumda geçerli olur.
    static Material KapiAltiMalzeme()
    {
        const string Yol = "Assets/Materials/Kapi_Alti_Isik.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(m, Yol);
        }
        m.SetColor("_BaseColor", AkkorRengi * 1.5f);
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        return m;
    }

    // Daire holü, asansör kuyusu ve kuyu dibi için koyu malzeme.
    static Material KaranlikMalzeme()
    {
        return MalzemeBul("Assets/Materials/Daire_Karanlik.mat", new Color(0.04f, 0.04f, 0.045f));
    }

    // Pencereden görünen gece için koyu lacivert.
    static Material GeceMalzeme()
    {
        return MalzemeBul("Assets/Materials/Gece.mat", new Color(0.015f, 0.02f, 0.05f));
    }

    // Malzemeyi diskte bulur, yoksa bir kez üretip kaydeder.
    // Her kurulumda yenisini üretmiyoruz; tek malzeme bütün kopyalarda paylaşılıyor.
    static Material MalzemeBul(string yol, Color renk)
    {
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (mevcut != null) return mevcut;

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        yeni.SetColor("_BaseColor", renk);
        yeni.SetFloat("_Smoothness", 0.05f);   // mat yüzey, parlama olmasın

        AssetDatabase.CreateAsset(yeni, yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }
}
