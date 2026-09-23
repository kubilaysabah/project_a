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
public static class GrayboxKurucu
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
            "Sahanlik_A", "Sahanlik_B", "Cekirdek_A", "Sahte_Katlar", "CatiBolumu",

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
        }

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

            // Ayakkabılık: 4 numaranın yanında, sağ duvara dayalı, merdiven tarafında.
            Kutu("Ayakkabilik", t,
                 new Vector3(SahanlikGen / 2f - 0.18f, 0.3f, -0.55f),
                 new Vector3(0.35f, 0.6f, 0.8f));

            // Elektrik sayacı: arka duvarın sol parçasında.
            Kutu("Elektrik_Sayaci", t,
                 new Vector3(-(cekirdekDis + parcaGen / 2f), 1.55f, -0.05f),
                 new Vector3(0.22f, 0.28f, 0.10f));

            // Yönetici ilanı: ön duvarda, oyuncuya göre asansörün solunda (+x), göz hizasında.
            Kutu("Ilan", t,
                 new Vector3(1.4f, 1.50f, onZ + 0.01f),
                 new Vector3(0.21f, 0.30f, 0.02f));
        }

        // Sahanlık ampulü. Sahte katlarda da var: merdiven gözünden bakınca
        // alt katların karanlık bir delik gibi değil, ışıklı katlar gibi görünmesi için.
        AmpulKur("Ampul", t, new Vector3(0f, DuvarYuksek - 0.25f, onZ / 2f), 7f, 2.0f);

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

        // Kapıların arkasında karanlık bir kuyu. Kapı bir gün aralanırsa
        // (anomali) oyuncu hiçliği değil, simsiyah bir asansör boşluğunu görür.
        GameObject kuyu = Kutu("Kuyu", a, new Vector3(0f, AsansorYuk / 2f, yuz - DuvarKalin - 0.6f),
             new Vector3(AsansorGen + 0.2f, AsansorYuk + 0.2f, 0.1f));
        kuyu.GetComponent<MeshRenderer>().sharedMaterial = KaranlikMalzeme();

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
        YaziKur("Yazi", a, new Vector3(0f, AsansorYuk + 0.22f, yuz + 0.035f), "-", 0.012f);

        if (tam)
        {
            // "ARIZALI" notu: oyuncunun sağındaki kanada (-x) bantlanmış bir kâğıt.
            float kanatOnu = yuz - 0.05f + 0.025f;   // kanadın ön yüzü
            Kutu("Not", a, new Vector3(-0.22f, 1.45f, kanatOnu + 0.004f),
                 new Vector3(0.26f, 0.20f, 0.008f));
            YaziKur("Not_Yazi", a, new Vector3(-0.22f, 1.46f, kanatOnu + 0.01f),
                    "ARIZALI\n<size=48>Yönetim</size>", 0.0055f);
        }

    }

    // Asansör yazıları için 3B yazı. Sahanlığa (+z) bakar.
    static void YaziKur(string ad, Transform ebeveyn, Vector3 konum, string metin, float boy)
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
        yazi.color = new Color(0.10f, 0.10f, 0.12f);
        yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        g.GetComponent<MeshRenderer>().sharedMaterial = yazi.font.material;
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

        // Sahte katlarda kapı sadece kanat, kol ve plakadan ibaret.
        if (!tam) return;

        // NUMARA YAZISI. KatYoneticisi her katta günceller.
        GameObject yaziNesne = new GameObject("Numara");
        yaziNesne.transform.SetParent(m, false);
        yaziNesne.transform.localPosition = new Vector3(-yon * 0.06f, 1.78f, kanatZ);
        yaziNesne.transform.localRotation = Quaternion.Euler(0f, sagDuvar ? 90f : -90f, 0f);

        TextMesh yazi = yaziNesne.AddComponent<TextMesh>();
        yazi.text = "0";
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.fontSize = 96;
        yazi.characterSize = 0.014f;
        yazi.color = new Color(0.12f, 0.12f, 0.12f);
        yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        yaziNesne.GetComponent<MeshRenderer>().sharedMaterial = yazi.font.material;

        // Kapı gözü: kanadın ortasında, göz hizasında. Kanatla birlikte döner.
        Kutu("Kapi_Gozu", m, new Vector3(-yon * 0.035f, 1.60f, kanatZ),
             new Vector3(0.03f, 0.05f, 0.05f));

        // ---- KAPI ÇEVRESİ EŞYALARI ----
        // Zil: kapının yanında, duvarda.
        Kutu("Zil", k, new Vector3(duvarYuzu - yon * 0.025f, 1.35f, KapiGen / 2f + 0.12f),
             new Vector3(0.05f, 0.08f, 0.06f));

        // Paspas: kapının tam önünde, duvara dayalı. Artık kapının çocuğu;
        // böylece dört kapının paspası dört ayrı isim yerine "Kapi_N/Paspas" oluyor.
        const float PaspasEn = 0.62f, PaspasBoy = 0.50f;
        Kutu("Paspas", k, new Vector3(duvarYuzu - yon * PaspasEn / 2f, 0.015f, 0f),
             new Vector3(PaspasEn, 0.03f, PaspasBoy));

        // Terlikler: paspasın üstünde bir çift, ayrı ayrı ("tek terlik" anomalisi).
        GameObject terlikler = new GameObject("Terlikler");
        terlikler.transform.SetParent(k, false);
        Kutu("Terlik_Sol", terlikler.transform,
             new Vector3(duvarYuzu - yon * 0.32f, 0.045f, -0.11f),
             new Vector3(0.26f, 0.06f, 0.10f));
        Kutu("Terlik_Sag", terlikler.transform,
             new Vector3(duvarYuzu - yon * 0.32f, 0.045f, 0.11f),
             new Vector3(0.26f, 0.06f, 0.10f));

        // Kapının arkasındaki karanlık hol.
        DaireIciKur(k, yon);

        // Çöpler: kapının yanında en fazla üç poşet, büyükten küçüğe.
        CoplerKur(k, duvarYuzu, yon);
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
    static void CoplerKur(Transform kapi, float duvarYuzu, float yon)
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
        float z = -(KapiGen / 2f + 0.10f);   // kapı kenarının biraz ötesinden başla, ön duvara doğru diz

        for (int i = 0; i < olculer.Length; i++)
        {
            Vector3 o = olculer[i];
            z -= o.z / 2f;                   // bu poşetin merkezi

            GameObject poset = Kutu($"Poset_{i + 1}", kok.transform,
                new Vector3(duvarYuzu - yon * (o.x / 2f + 0.05f), o.y / 2f, z),
                o);

            // İşaret bileşeni: oyuncunun nişan ışını buna çarpınca "bu alınabilir
            // bir poşet" olduğunu anlıyor. İsme değil bileşene bakıyoruz.
            poset.AddComponent<CopPoseti>();

            z -= o.z / 2f + Aralik;          // bir sonrakinin başlangıcı
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

        // KORKULUKLAR: merdiven gözünü çevreler. Oyuncu gözden düşemesin ama
        // üstünden eğilip aşağı ve yukarı bakabilsin diye dolu duvar değil,
        // tırabzan + ince dikmeler. Dikmeler arası 24 cm; oyuncunun gövdesi
        // (60 cm) arasından geçemez.
        KorkuluklariKur(t);

        // Merdiven ampulü: ara sahanlığın üstünde.
        AmpulKur("Ampul_Merdiven", t,
                 new Vector3(0f, AraKot + 2.35f, KolUzunluk + AraSahanlikDerin / 2f), 6f, 1.6f);

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
        GameObject gece = Kutu("Disari", p, new Vector3(0f, (Alt + Ust) / 2f, CekirdekDerin + DuvarKalin + 1.0f),
             new Vector3(10f, Ust - Alt, 0.1f));
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
        Kutu("CatiKapisi", t, new Vector3(kapiX, KapiYukC / 2f, onZ),
             new Vector3(KapiGenC, KapiYukC, DuvarKalin * 0.6f));

        // KORKULUKLAR (parapet): çatının kenarları.
        Kutu("Parapet_Sol", t, new Vector3(-disX + DuvarKalin / 2f, Parapet / 2f, -SahanlikDerin / 2f),
             new Vector3(DuvarKalin, Parapet, SahanlikDerin));
        Kutu("Parapet_Sag", t, new Vector3(disX - DuvarKalin / 2f, Parapet / 2f, -SahanlikDerin / 2f),
             new Vector3(DuvarKalin, Parapet, SahanlikDerin));
        Kutu("Parapet_On", t, new Vector3(0f, Parapet / 2f, -SahanlikDerin - DuvarKalin / 2f),
             new Vector3(2f * disX, Parapet, DuvarKalin));

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

    // Adı, ebeveyni, yerel konumu ve ölçeği verilen bir küp üretir.
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

    // Nokta ışık üretir. 1994 akkor ampulünün sıcak sarısı.
    //
    // HİÇBİR AMPUL GÖLGE ATMIYOR. Önceki düzende sahanlık ampulleri atıyordu.
    // Artık sahnede on dört ampul var ve gölge atan bir nokta ışık sahneyi altı kez
    // daha çizdirir. Daha önemlisi, döngü hilesi her katın BİREBİR aynı
    // aydınlanmasını istiyor: bazı ampuller gölge atıp bazıları atmasaydı
    // ışık döşemelerden farklı sızar ve ışınlanma anında aydınlık değişirdi.
    // Gerçek ışık düzeni art pass'te (#10) kurulacak.
    static void AmpulKur(string ad, Transform ebeveyn, Vector3 konum, float menzil, float siddet)
    {
        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        Light isik = g.AddComponent<Light>();
        isik.type = LightType.Point;                    // her yöne yayılan ampul
        isik.range = menzil;
        isik.intensity = siddet;
        isik.color = new Color(1f, 0.92f, 0.76f);       // sıcak sarı
        isik.shadows = LightShadows.None;
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
