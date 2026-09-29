// GrayboxKurucu_Zincir.cs
// Kilit zinciri araçları (#54): zincir eşyalarının şablonları (prefab) ve geliştirme için
// test zinciri. Menü: Huzur → Zincir → Şablonları ve Test Zincirini Kur.
// GrayboxKurucu'nun parçasıdır ("partial"): Model yardımcısını ve sahne kaydetmeyi kullanır.
//
// ---------- ŞABLON (PREFAB) NEDİR ----------
// Prefab, projede dosya olarak saklanan hazır bir nesne kalıbıdır: model, çarpışma kutusu
// ve bileşenleriyle. Oyun sırasında bir anahtar gerektiğinde bu kalıptan kopya çıkarılır
// (Instantiate). Zincir her katta başka eşyalar koyduğu için eşyaları sahneye önceden
// kurmak yerine kalıptan üretiyoruz. Kalıplar KatYoneticisi'ne BAĞLANIR; bağlı olmayan bir
// dosya build'e girmeyebilir (yalnızca kullanılan varlıklar paketlenir).
//
// ---------- TEST ZİNCİRİ ----------
// #55 sekiz katın gerçek zincirlerini yazacak. O zamana kadar beş kilit türünü denemek için
// 1. kata, beşini birden kullanan bir zincir bağlanır (Assets/Zincirler/Zincir_Test.asset):
//   mutfaktaki kavanozda SALON anahtarı -> salon (anahtar)
//   salondaki takvimde şifre -> kiler (şifreli kilit, 0612)
//   kilerdeki kol -> Oda A (kol)
//   Oda A'daki komodinde sigorta -> sigorta kutusuna takılınca buzdolabı (sigorta)
//   buzdolabında tornavida -> Oda B'nin menteşeleri (tornavida)
//   Oda B'deki oyuncak kutusunda KATIN anahtarı. Banyo bu katta hiç açılmaz.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // #81: büfe artık oymalı dolap, oyuncak kutusu ahşap sandık (Art/Mobilya/scripts/mobilya_donustur.py, BULMACA).
    // İçlerine konan eşyaların yüksekliği yeni modellerin oyulmuş bölmesinin tabanı (dönüştürücü "IC_TABAN" yazar):
    // İlk değerler dönüştürücünün oyduğu tabandandı; ama modelin kendi raf tahtası daha yukarıda kaldı ve büfedeki
    // anahtar tahtanın 1,4 cm içinde görünmez duruyordu (Şahan'ın 6. kat testi, 2026-09-28). Tabanlar artık
    // oyunda ölçüldü (MeshCollider ile yukarıdan aşağı ışın): büfenin tahtası 25,9 cm, sandığın tabanı 1,3 cm.
    // Eşya tabana 7-11 mm yukarıdan konur.
    const float BufeTaban = 0.27f;
    const float SandikTaban = 0.02f;

    const string SablonKlasoru = "Assets/Prefabs/Esyalar";
    const string ZincirKlasoru = "Assets/Zincirler";

    [MenuItem("Huzur/Zincir/Şablonları ve Test Zincirini Kur")]
    public static void ZincirAraclariniKur()
    {
        KlasorHazirla("Assets", "Prefabs");
        KlasorHazirla("Assets/Prefabs", "Esyalar");
        KlasorHazirla("Assets", "Zincirler");

        GameObject odaAnahtari = SablonKur("Esya_OdaAnahtari", "Anahtar", EsyaTuru.OdaAnahtari, GumusMalzeme());
        GameObject katAnahtari = SablonKur("Esya_KatAnahtari", "Anahtar", EsyaTuru.KatAnahtari, null);
        GameObject sigorta = SablonKur("Esya_Sigorta", "Buson", EsyaTuru.Sigorta, null);
        GameObject tornavida = SablonKur("Esya_Tornavida", "Tornavida", EsyaTuru.Tornavida, null);
        GameObject asmaKilit = ParcaSablonuKur("Asma_Kilit", "Asma_Kilit");
        GameObject plaka = MentesePlakasiSablonuKur();
        GameObject camKirik = ParcaSablonuKur("Cam_Kiriklari", "Cam_Kiriklari");
        // Kırıkların çarpışma kutusu olmasın: yerde duran küçük parçalar anahtarı nişanlamayı
        // engellemesin, oyuncu da takılmasın.
        if (camKirik != null)
        {
            GameObject ic = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(camKirik));
            foreach (Collider c in ic.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            PrefabUtility.SaveAsPrefabAsset(ic, AssetDatabase.GetAssetPath(camKirik));
            PrefabUtility.UnloadPrefabContents(ic);
        }
        KatZinciriVarligi test = TestZinciriKur();
        // Dekor ve not şablonları (#55).
        GameObject notSablonu = NotSablonuKur();
        GameObject koliBuyuk = ParcaSablonuKur("Koli_Buyuk", "Koli_Buyuk");
        GameObject koliKucuk = ParcaSablonuKur("Koli_Kucuk", "Koli_Kucuk");
        GameObject koliAcik = ParcaSablonuKur("Koli_Acik", "Koli_Acik");
        // Antre dekoru (#83): yeni Blender modelleri ve bileşik şablonlar.
        ParcaSablonuKur("Anahtar_Askiligi", "Anahtar_Askiligi");
        ParcaSablonuKur("Nazar_Boncugu", "Nazar_Boncugu");
        TelefonTakimiSablonuKur();
        TerlikCiftiSablonuKur("Kadin");
        TerlikCiftiSablonuKur("Cocuk");
        // Hol dekoru (#83).
        ParcaSablonuKur("Petek", "Petek");
        ParcaSablonuKur("Hali_Rulolari", "Hali_Rulolari");
        GoblenSablonuKur();
        TabloIziSablonuKur("Tablo_Izi_Genis", 0.55f, 0.42f);
        TabloIziSablonuKur("Tablo_Izi_Dik", 0.36f, 0.48f);
        TabloIziSablonuKur("Tablo_Izi_Kucuk", 0.30f, 0.24f);
        TabloIziSablonuKur("Bos_Civi", 0f, 0f);
        // Salon dekoru (#83).
        ParcaSablonuKur("Carsafli_Kanepe", "Carsafli_Kanepe");
        ParcaSablonuKur("TV_Ustu", "TV_Ustu");
        ParcaSablonuKur("Fincan", "Fincan");
        HaliYariSablonuKur();
        PerdeSablonuKur();
        TabloIziSablonuKur("Hali_Izi", 2.40f, 1.70f, yatay: true);
        // Mutfak dekoru (#83).
        ParcaSablonuKur("Ust_Dolap", "Ust_Dolap");
        ParcaSablonuKur("Gazeteli_Tabaklar", "Gazeteli_Tabaklar");
        ParcaSablonuKur("Tup", "Tup");
        ParcaSablonuKur("Tup_Hortumu", "Tup_Hortumu");
        KavanozSusSablonuKur("Tursu", false, camKirik);
        KavanozSusSablonuKur("Salca", false, camKirik);
        KavanozSusSablonuKur("Recel", true, camKirik);
        KavanozSusSablonuKur("Bakliyat", false, camKirik);
        // Oda A dekoru (#83).
        ParcaSablonuKur("Silte", "Silte");
        ParcaSablonuKur("Sokuk_Karyola", "Sokuk_Karyola");
        ParcaSablonuKur("Acik_Gardirop", "Acik_Gardirop");
        ParcaSablonuKur("Acik_Bavul", "Acik_Bavul");
        ParcaSablonuKur("Abajur", "Abajur");
        KatZinciriVarligi kat1 = Kat1Kur(koliBuyuk, koliKucuk, koliAcik);
        // 2. kat antre dekoru (#83): katlarda aynı model kullanılmaz (Şahan), yeni modeller.
        ParcaSablonuKur("Portmanto_Tik", "Portmanto_Tik");
        ParcaSablonuKur("Telefon_Oturagi", "Telefon_Oturagi");
        ParcaSablonuKur("Posta_Yigini", "Posta_Yigini");
        ParcaSablonuKur("Yaprak_Takvim", "Yaprak_Takvim");
        SaatSablonuKur();
        TerlikCiftiSablonuKur("Yasli");
        // 2. kat hol dekoru (#83).
        YollukSablonuKur();
        ParcaSablonuKur("Kitaplik", "Kitaplik");
        ParcaSablonuKur("Kus_Kafesi", "Kus_Kafesi");
        ParcaSablonuKur("Ilac_Dolabi", "Ilac_Dolabi");
        ParcaSablonuKur("Sacilmis_Ilaclar", "Sacilmis_Ilaclar");
        ParcaSablonuKur("Devrik_Sandalye", "Devrik_Sandalye");
        ParcaSablonuKur("Kirik_Gozluk", "Kirik_Gozluk");
        // 2. kat salon dekoru (#83).
        HaliDuzSablonuKur("Hali_Nuri", "Assets/Textures/Hali_Nuri.png");
        PerdeKadifeSablonuKur();
        PortreSablonuKur("Portre_Kat2_Es", "Assets/Textures/Portreler/kat2_es.png");
        foreach (string ad in new[] { "Orgu_Battaniye", "Cay_Takimi", "Gazete_Kilif", "Kuru_Sardunya", "Dikis_Makinesi", "Koltuk_Danteli" })
            ParcaSablonuKur(ad, ad);
        KatZinciriVarligi kat2 = Kat2Kur();
        GameObject cizimParcasi = CizimParcasiSablonuKur();
        GameObject levye = SablonKur("Esya_Levye", "Levye", EsyaTuru.Levye, null);
        GameObject kaydirici = SablonKur("Esya_Kaydirici", "Kaydirici", EsyaTuru.Kaydirici, null);
        GameObject anahtarParcasi = AnahtarParcasiSablonuKur();
        GameObject tabak = SablonKur("Esya_Tabak", "Tabak", EsyaTuru.Tabak, null);
        GameObject isimKarti = IsimKartiSablonuKur();
        KatZinciriVarligi kat3 = Kat3Kur();
        KatZinciriVarligi kat4 = Kat4Kur();
        KatZinciriVarligi kat5 = Kat5Kur();
        GameObject golge = GolgeSablonuKur();
        KatZinciriVarligi kat6 = Kat6Kur(isimKarti, golge);
        KatZinciriVarligi kat7 = Kat7Kur(golge);
        KatZinciriVarligi kat8 = Kat8Kur();

        // Her katın mobilyası (#73): kat dosyalarının süs listesine eklenir.
        KatZinciriVarligi[] katDosyalari = { kat1, kat2, kat3, kat4, kat5, kat6, kat7, kat8 };
        for (int k = 0; k < katDosyalari.Length; k++) MobilyalariEkle(katDosyalari[k], k + 1);

        // Sahnedeki KatYoneticisi'ne bağla: şablonlar ve 1. katın zinciri.
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>(FindObjectsInactive.Include);
        if (yonetici == null) { Debug.LogWarning("[ZINCIR] Sahnede KatYoneticisi yok."); return; }
        var seri = new SerializedObject(yonetici);
        seri.FindProperty("odaAnahtariSablonu").objectReferenceValue = odaAnahtari;
        seri.FindProperty("katAnahtariSablonu").objectReferenceValue = katAnahtari;
        seri.FindProperty("sigortaSablonu").objectReferenceValue = sigorta;
        seri.FindProperty("tornavidaSablonu").objectReferenceValue = tornavida;
        seri.FindProperty("asmaKilitSablonu").objectReferenceValue = asmaKilit;
        seri.FindProperty("mentesePlakasiSablonu").objectReferenceValue = plaka;
        seri.FindProperty("camKirikSablonu").objectReferenceValue = camKirik;
        seri.FindProperty("notSablonu").objectReferenceValue = notSablonu;
        seri.FindProperty("cizimParcasiSablonu").objectReferenceValue = cizimParcasi;
        seri.FindProperty("levyeSablonu").objectReferenceValue = levye;
        seri.FindProperty("kaydiriciSablonu").objectReferenceValue = kaydirici;
        seri.FindProperty("anahtarParcasiSablonu").objectReferenceValue = anahtarParcasi;
        seri.FindProperty("tabakSablonu").objectReferenceValue = tabak;
        // #61: nişan menzili 2,8 m (sahnedeki oyuncunun kayıtlı değeri de güncellensin).
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>(FindObjectsInactive.Include);
        if (etk != null)
        {
            var etkSeri = new SerializedObject(etk);
            etkSeri.FindProperty("menzil").floatValue = 2.8f;
            etkSeri.ApplyModifiedPropertiesWithoutUndo();
        }
        SerializedProperty katlar = seri.FindProperty("katZincirleri");
        katlar.arraySize = KatSayaci.CatiKati;
        // 1. kat öğretici zincirini taşır (#55); öbür katlar yazıldıkça bağlanır. Test zinciri
        // (beş türün hepsi) dosya olarak duruyor ve testlerde denetleniyor ama bir kata bağlı değil.
        katlar.GetArrayElementAtIndex(0).objectReferenceValue = kat1;
        katlar.GetArrayElementAtIndex(1).objectReferenceValue = kat2;
        katlar.GetArrayElementAtIndex(2).objectReferenceValue = kat3;
        katlar.GetArrayElementAtIndex(3).objectReferenceValue = kat4;
        katlar.GetArrayElementAtIndex(4).objectReferenceValue = kat5;
        katlar.GetArrayElementAtIndex(5).objectReferenceValue = kat6;
        katlar.GetArrayElementAtIndex(6).objectReferenceValue = kat7;
        katlar.GetArrayElementAtIndex(7).objectReferenceValue = kat8;
        seri.ApplyModifiedPropertiesWithoutUndo();

        // Gezen Gölge (#71): bütün katlarda 1-2 dakikada bir beliren Gölge, kat yöneticisinin
        // nesnesinde. 7. kattaki kaçışla aynı şablon; habercisi üst kattan adımlar (#74).
        GezenGolge gezen = yonetici.GetComponent<GezenGolge>();
        if (gezen == null) gezen = yonetici.gameObject.AddComponent<GezenGolge>();
        var gSeri = new SerializedObject(gezen);
        gSeri.FindProperty("sablon").objectReferenceValue = golge;
        gSeri.FindProperty("haberci").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Olay/Olay_UstKatAdimlari.wav");
        gSeri.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        SahneyiKaydet("Zincir");
        Debug.Log("[ZINCIR] Şablonlar, test zinciri ve 1-8. katlar kuruldu ve bağlandı.");
    }

    static void KlasorHazirla(string ust, string ad)
    {
        if (!AssetDatabase.IsValidFolder(ust + "/" + ad)) AssetDatabase.CreateFolder(ust, ad);
    }

    // Bir eşya şablonu: modelin meshi, nişan için biraz büyük (ama kabın içine sığan)
    // bir çarpışma kutusu ve Esya bileşeni. Dosya varsa üzerine yazılır (GUID'i korunur).
    static GameObject SablonKur(string ad, string dosya, EsyaTuru tur, Material ozelMalzeme)
    {
        GameObject gecici = new GameObject(ad);
        GameObject model = Model("Model", gecici.transform, dosya, null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {dosya}.fbx yok; {ad} kurulmadı."); return null; }
        if (ozelMalzeme != null) model.GetComponent<MeshRenderer>().sharedMaterial = ozelMalzeme;

        // Çarpışma kutusu modelin sınırlarından en az 16 x 8 x 16 cm: küçük eşyayı nişanlamak
        // kolay olsun (#61, Şahan zorlandı). Kavanozun dışına taşsa da sorun değil: kapağı
        // kapalı kabın içindeki eşya zaten alınamıyor (Esya.EtkilesimeAcik).
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center + model.transform.localPosition;
        // Vector3.Max: iki vektörün her ekseninden büyük olanı alır (modelden küçük olmasın).
        c.size = Vector3.Max(b.size, new Vector3(0.16f, 0.08f, 0.16f));

        Esya esya = gecici.AddComponent<Esya>();
        esya.Ayarla(tur, "", 0);

        string yol = $"{SablonKlasoru}/{ad}.prefab";
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, yol);
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Eşya olmayan bir parça şablonu (asma kilit gibi): yalnızca model ve çarpışma kutusu.
    // Çarpışma kutusu nişanı kolaylaştırır; etkileşim kilidin takılı olduğu kapıdan gelir.
    static GameObject ParcaSablonuKur(string ad, string dosya)
    {
        GameObject gecici = new GameObject(ad);
        GameObject model = Model("Model", gecici.transform, dosya, null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {dosya}.fbx yok."); return null; }
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center;
        c.size = b.size;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Menteşe plakası şablonu: plaka ve iki vida, vidalar kökün çocukları (MenteseSokucu
    // onları adlarıyla bulup tek tek söker).
    static GameObject MentesePlakasiSablonuKur()
    {
        GameObject gecici = new GameObject("Mentese_Plakasi");
        if (Model("Plaka", gecici.transform, "Mentese_Plakasi", "Mentese_Plakasi", Vector3.zero, 0f, carpisma: false) == null)
        { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Mentese_Plakasi.fbx yok."); return null; }
        Model("Vida_1", gecici.transform, "Mentese_Plakasi", "Vida_1", Vector3.zero, 0f, carpisma: false);
        Model("Vida_2", gecici.transform, "Mentese_Plakasi", "Vida_2", Vector3.zero, 0f, carpisma: false);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Mentese_Plakasi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Kırık anahtar parçası şablonu (#65): iki model (baş ve uç) aynı prefabda; uygulayıcı
    // parçanın kimliğine göre birini gösterir. Gümüş (oda anahtarının rengi); nişan için büyükçe kutu.
    static GameObject AnahtarParcasiSablonuKur()
    {
        GameObject gecici = new GameObject("Esya_AnahtarParcasi");
        GameObject bas = Model("Model_Bas", gecici.transform, "Kirik_Anahtar", "Bas", Vector3.zero, 0f, carpisma: false);
        GameObject uc = Model("Model_Uc", gecici.transform, "Kirik_Anahtar", "Uc", Vector3.zero, 0f, carpisma: false);
        if (bas == null || uc == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Kirik_Anahtar.fbx yok."); return null; }
        // Parçalar FBX'teki kendi yerlerinde durur (birleşik anahtarın iki yarısı); ikisi de
        // prefabın ortasına alınır ki hangisi görünürse görünsün yerine otursun.
        bas.transform.localPosition = Vector3.zero;
        uc.transform.localPosition = Vector3.zero;
        uc.SetActive(false);
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.03f, 0f);
        c.size = new Vector3(0.16f, 0.08f, 0.16f);
        Esya esya = gecici.AddComponent<Esya>();
        esya.Ayarla(EsyaTuru.AnahtarParcasi, "", 0);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Esya_AnahtarParcasi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // İsim kartı şablonu (#66): kart modeli ve ön yüzünde bir yazı ("Yazi"). Metni kat dosyası
    // verir (KatSusu.Metin), dile göre YerelYazi yazar. Çarpışma kutusu yok: süs, alınmaz.
    static GameObject IsimKartiSablonuKur()
    {
        GameObject gecici = new GameObject("Isim_Karti");
        if (Model("Model", gecici.transform, "Isim_Karti", null, Vector3.zero, 0f, carpisma: false) == null)
        { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Isim_Karti.fbx yok."); return null; }
        GameObject y = new GameObject("Yazi");
        y.transform.SetParent(gecici.transform, false);
        // Kartın ön yüzü z = +0,0015; yazı 2 mm önünde, kartın ortasında. TextMesh'in okunan yüzü
        // -z'ye baktığı için 180° çevrilir (ön yüzü +Z olan modelin önünden okunsun).
        y.transform.localPosition = new Vector3(0f, 0.034f, 0.0035f);
        y.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh t = y.AddComponent<TextMesh>();
        t.text = "?";
        t.anchor = TextAnchor.MiddleCenter;
        t.alignment = TextAlignment.Center;
        t.fontSize = 96;
        t.characterSize = 0.0016f;          // 0,0011 bir adım öteden okunmuyordu (oynatma modu görüntüsü)
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.03f, 0.03f, 0.03f);
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        y.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(t.font);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Isim_Karti.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Okunabilir not kâğıdı şablonu: model, nişan için büyükçe çarpışma kutusu, YoneticiNotu.
    static GameObject NotSablonuKur()
    {
        GameObject gecici = new GameObject("Not_Kagidi");
        GameObject model = Model("Model", gecici.transform, "Not_Kagidi", null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Not_Kagidi.fbx yok."); return null; }
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center;
        c.size = Vector3.Max(b.size, new Vector3(0.18f, 0.04f, 0.24f));
        gecici.AddComponent<YoneticiNotu>();
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Not_Kagidi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // ---------- 1. KAT: ÖĞRETİCİ, ODA ODA YENİDEN DÖŞENDİ (#83, Şahan, 2026-09-29) ----------
    // Şahan katları oda oda yeniden döşedi: her oda için 90'ların evinde neler olduğu listelendi,
    // Şahan numaralarla seçti; kapılar ve görev parçaları odalar bitince kapı açılış sırasına göre
    // kuruldu. Eski tasarım (ayakkabılıkta tornavida, Nermin'in ipucu notu) git geçmişinde.
    //
    // Kapı sırası ve görev parçaları (kural: bir odada en fazla bir görev parçası, ipucu notu yok):
    //   1. Antre: portmantonun çekmecesinde tornavida (alet çantasına girer, hiç harcanmaz).
    //   2. Salon: kilitsiz; görev parçası yok (aile fotoğrafı, boşaltılan ev).
    //   3. Mutfak: kilitsiz; tezgâhta beş kavanoz, hepsi tornavidayla kırılır (süsleri KirilanKavanoz),
    //      anahtar zincir kavanozunda; cam saydam, anahtar kafa lambasıyla görünür.
    //   4. Oda A: kavanozdan çıkan anahtarla açılır; kat anahtarı komodinin kapalı çekmecesinde.
    //   Banyo, kiler (Oda C), Oda B ve balkon bu katta kapalı.
    //
    // Hikâye: Yıldız ailesi (2 numara) borç yüzünden tahliye ediliyor; ev yarı toplanmış.

    static KatZinciriVarligi Kat1Kur(GameObject koliBuyuk, GameObject koliKucuk, GameObject koliAcik)
    {
        string yol = $"{ZincirKlasoru}/Kat_1.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "1. kat, daire 2, Yıldız ailesi (tahliye), öğretici. Portmantonun çekmecesinde tornavida; " +
                     "mutfakta kavanozlar (hepsi kırılır), birinde Oda A'nın anahtarı; Oda A'da komodinin çekmecesinde " +
                     "katın anahtarı. Salon ve mutfak kilitsiz; banyo, kiler, Oda B kapalı. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_B", "Oda_C" });
        // Yerler: portmantonun çekmecesi (antre), tezgâhtaki kavanoz (mutfak; kilidi kapağında),
        // komodinin çekmecesi (Oda A).
        z.Yerler.Add(new ZincirYeri { Kimlik = "portmanto", Yol = "Esyalar/Portmanto/Cekmece", Oda = "Antre" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kavanoz", Yol = "Esyalar/Tezgah/Kavanoz", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kavanoz/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        // Tornavida çekmecenin tepsisinde yatar: tepsinin tabanı 0,752 m, ortası portmantonun ekseninden
        // 7,8 cm geride (çekmece yuvası panoya bitişik); 90° çevrilip çekmecenin enine uzanır.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "portmanto", Y = 0.752f, Z = -0.078f, Donus = 90f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "kavanoz", Y = 0.012f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.375f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kavanoz", Tur = KilitTuru.Kirma, OdaKapisi = false, Hedef = "kavanoz", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "odaA_anahtari" });
        v.zincir = z;
        v.sesler = new List<KatSesi>();
        v.parcalar = new List<KatParcasi>();

        // Dairenin kalıcı eşyalarından bu katta görünenler (oda oda seçildikçe eklenir).
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Portmanto", "Tezgah", "Buzdolabi", "Komodin" };

        // Yerel fonksiyon: bu fonksiyonun içinde kullanılan kısa bir yardımcı (bir süsü plan noktasına koyar).
        // u: landing duvarından içeri, y: yükseklik, vv: giriş kapısından boyuna; (onU, onV): modelin
        // baktığı plan yönü (DairePlani).
        KatSusu Sus(GameObject sablon, float u, float y, float vv, float onU, float onV)
        { return new KatSusu { Sablon = sablon, X = u, Y = y, Z = vv, OnU = onU, OnV = onV }; }

        // ---------- ANTRE (Şahan'ın seçimi: 1, 2, 4, 5, 6, 8, 12, 13, 15) ----------
        // Antre u 0-2,6, v -0,8-2,85; üç duvarı var: landing duvarı (u = 0,03, giriş kapısı v = 0'da),
        // dış duvar (v = -0,8, sigorta kutusu u = 1,6'da) ve banyo duvarı (u = 2,6). Hol tarafı (v = 2,9)
        // boydan boya açık. Giriş kapısı içeri, -v tarafına açılır: açık kanat u 0-0,8, v -0,4 boyunca
        // durur; o köşe boş bırakılır. Kapıdan hole giden çapraz yol ortada, 1,8 m genişliğinde boş.
        v.notlar = new List<KatNotu>
        {
            // Tahliye yazısı: sigorta kutusunun yanında duvarda (Yönetim kapıcıya bıraktırmış).
            new KatNotu { Metin = "not.kat1_tahliye", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        // Portmanto dairenin kalıcı eşyası (Esyalar/Portmanto, yukarıdaki listede): banyo duvarında,
        // girene karşıdan bakar. Ağır olduğu için geride kalmış; askıları boş.
        v.suslar = new List<KatSusu>
        {
            // Telefon sehpası (boyalı komodin) portmantonun yanında, hol ağzına yakın; üstünde dantel,
            // telefon ve rehber. Hat borç yüzünden kesilmiş: kablo duvardan sökülmüş, fişi rehberin üstünde.
            M("painted_wooden_nightstand", 2.60f - 0.255f, 0f, 2.35f, -1f, 0f),
            Sus(Sablon("Telefon_Takimi"), 2.60f - 0.255f, 0.616f, 2.35f, -1f, 0f),
            // Anahtar askılığı: kapının hemen yanında, göz hizasında. Kancalar boş (anahtarlar
            // kapıcıya teslim edilecek); birinde anahtarı sökülmüş bir halka.
            Sus(Sablon("Anahtar_Askiligi"), 0.03f, 1.55f, 0.85f, 1f, 0f),
            // Nazar boncuğu: giriş kapısının üstünde (kapı 2,05 m).
            Sus(Sablon("Nazar_Boncugu"), 0.03f, 2.20f, 0f, 1f, 0f),
            // İki eski bavul: landing duvarının dibinde, kapının yanında; götürülmeyi bekliyor.
            M("vintage_suitcase", 0.03f + 0.125f, 0f, 1.40f, 1f, 0f),
            // Terlikler: kapının içinde, burunları eve dönük. Nermin'in pembe terliği ve çocuğun terliği.
            Sus(Sablon("Terlik_Cifti_Kadin"), 0.70f, 0f, 0.10f, -1f, 0f),
            Sus(Sablon("Terlik_Cifti_Cocuk"), 0.70f, 0f, 0.42f, -1f, 0f),
            // Koliler: dış duvar ile banyo duvarının köşesinde üst üste; sigorta kutusunun altında açık koli.
            Sus(koliBuyuk, 2.20f, 0f, -0.52f, -1f, 0.3f),
            Sus(koliKucuk, 2.20f, 0.40f, -0.52f, -1f, 0.1f),
            Sus(koliAcik, 1.35f, 0f, -0.50f, 0f, 1f),

            // ---------- HOL (Şahan'ın seçimi: 2, 4, 10, 11, 12, 13) ----------
            // Hol u 0-10,5, v 2,95-4,65 (1,7 m). Güney duvarı (v = 2,95) kapıları: banyo u 3,25-3,95, kiler
            // 5,25-6,05, mutfak 7,0-7,8. Kuzey duvarı (v = 4,65): salon 0,85-1,75, Oda B 5,95-6,75,
            // Oda A 8,7-9,5. Dip duvar u = 10,5. Kapılar odalara açılır; holün ortası yol, boş kalır.
            // Yolluk ve tablolar götürülmüş: duvarda açık renkli izler ve boş bir çivi.
            // Petek: dip duvarda, ortada.
            Sus(Sablon("Petek"), 10.50f, 0f, 3.80f, -1f, 0f),
            // Goblen: salon ile Oda B kapılarının arasında; duvarda unutulmuş tek tablo.
            Sus(Sablon("Goblen"), 3.80f, 1.55f, 4.65f, 0f, -1f),
            // Tablo izleri: goblenin iki yanında ve karşı duvarda; mutfağın yanında boşta bir çivi.
            Sus(Sablon("Tablo_Izi_Genis"), 2.55f, 1.55f, 4.65f, 0f, -1f),
            Sus(Sablon("Tablo_Izi_Dik"), 5.05f, 1.60f, 4.65f, 0f, -1f),
            Sus(Sablon("Tablo_Izi_Kucuk"), 4.60f, 1.60f, 2.95f, 0f, 1f),
            Sus(Sablon("Bos_Civi"), 8.60f, 1.70f, 2.95f, 0f, 1f),
            // Durmuş saat: Oda B ile Oda A kapılarının arasında; pili bitmiş, akrep ve yelkovan kıpırdamıyor.
            M("vintage_telephone_wall_clock", 7.60f, 1.55f, 4.65f, 0f, -1f),
            // Koliler: banyo ile kiler kapılarının arasında, küçük tablo izinin altında.
            Sus(koliBuyuk, 4.60f, 0f, 2.95f + 0.21f, 0f, 1f),
            Sus(koliKucuk, 4.62f, 0.40f, 2.95f + 0.21f, 0.15f, 1f),
            // Rulo halılar: mutfak kapısından sonra güney duvarına dayalı; ikisi dik, yolluk önlerinde yerde.
            Sus(Sablon("Hali_Rulolari"), 9.30f, 0f, 2.95f, 0f, 1f),

            // ---------- SALON (Şahan'ın seçimi: 3, 5, 10, 11, 12, 13, 14, 15, 16, 17) ----------
            // Salon u 0-5,0, v 4,75-8,1. Kapı hol duvarında (v = 4,75, u 0,85-1,75, içeri açılır); karşı
            // duvarda (v = 8,1) pencere (u 0,7-2,1, denizlik 0,9) ve hep kilitli balkon kapısı (u 3,2-4,0);
            // solda landing duvarı (u = 0,03), sağda Oda B duvarı (u = 5,0). Bu katta görev parçası yok:
            // salon hikâyenin odası (kilitsiz ilk oda, aile fotoğrafı, boşaltılan ev).
            // Pencerenin altında petek; pencerede yarısı indirilmiş tül (korniş bir ucundan sökülmüş).
            Sus(Sablon("Petek"), 1.40f, 0f, 8.10f, 0f, -1f),
            Sus(Sablon("Perde"), 1.40f, 2.30f, 8.10f, 0f, -1f),
            // Aile fotoğrafı: pencere ile balkon kapısının arasında; girene karşıdan bakar.
            Foto("kat1", true, 2.65f, 1.55f, 8.10f, 0f, -1f),
            // Çarşafla örtülü kanepe: landing duvarı boyunca, kapının açılan kanadından uzakta.
            Sus(Sablon("Carsafli_Kanepe"), 0.03f + 0.46f, 0f, 6.70f, 1f, 0f),
            // Boşaltılmış vitrin: hol duvarında; rafları boş, tezgâhında tek bir kahve fincanı unutulmuş.
            M("vintage_cabinet_01", 3.40f, 0f, 4.75f + 0.335f, 0f, 1f),
            Sus(Sablon("Fincan"), 3.70f, VitrinTezgahi, 4.75f + 0.50f, 0f, 1f),
            // TV sehpası: Oda B duvarında, kanepeye bakar; üstünde dantel ve biblo, televizyon yok.
            M("vintage_wooden_drawer_01", 5.00f - 0.23f, 0f, 6.60f, -1f, 0f),
            Sus(Sablon("TV_Ustu"), 5.00f - 0.23f, 0.55f, 6.60f, -1f, 0f),
            // Televizyon: sehpanın yanında yerde, götürülmeyi bekliyor.
            M("television_02", 4.60f, 0f, 7.35f, -1f, -0.35f),
            // Koliler: balkon kapısının yanındaki köşede.
            Sus(koliBuyuk, 4.50f, 0f, 8.10f - 0.21f, 0f, -1f),
            Sus(koliKucuk, 4.48f, 0.40f, 8.10f - 0.21f, 0.15f, -1f),
            // Halı: ortada, bir ucundan yarıya kadar sarılmış; sarılan kısmın altında parkede halının izi.
            Sus(Sablon("Hali_Izi"), 2.55f, 0f, 6.55f, 0f, 1f),
            Sus(Sablon("Hali_Yari"), 2.55f, 0f, 6.55f, 0f, 1f),

            // ---------- MUTFAK (Şahan'ın seçimi: 1, 2, 3, 4, 6, 12, 13, 14, 15) ----------
            // Mutfak u 6,8-10,5, v -0,8-2,85. Kapı hol duvarında (v = 2,9, u 7,0-7,8, içeri açılır); dip
            // duvarda (u = 10,6) pencere ve altında dairenin tezgâhı (u 9,88-10,5, v 0,09-1,91, üstü 0,90);
            // köşede buzdolabı (u 6,96-7,54, v -0,8..-0,19). Tezgâh ve buzdolabı dairenin kalıcı eşyaları.
            // Görev parçası (zincir aşamasında): tezgâhtaki kavanozlardan biri (dairenin Kavanoz'u, u 10,35
            // v 1,60) içinde Oda A'nın anahtarı. Beş kavanozun hepsi tornavidayla kırılır; cam saydam,
            // anahtar kafa lambasıyla görünür. Süs kavanozları zincir kavanozunun iki yanında.
            Sus(Sablon("Kavanoz_Tursu"), 10.35f, 0.90f, 1.77f, -1f, 0f),
            Sus(Sablon("Kavanoz_Salca"), 10.35f, 0.90f, 1.43f, -1f, 0f),
            Sus(Sablon("Kavanoz_Recel"), 10.19f, 0.90f, 1.69f, -1f, 0f),
            Sus(Sablon("Kavanoz_Bakliyat"), 10.19f, 0.90f, 1.51f, -1f, 0f),
            // Ocak dış duvarda; yanında hortumu sökülmüş tüp.
            M("electric_stove", 8.30f, 0f, -0.8f + 0.34f, 0f, 1f),
            Sus(Sablon("Tup"), 8.95f, 0f, -0.52f, 0f, 1f),
            Sus(Sablon("Tup_Hortumu"), 8.95f, 0f, -0.52f, 0f, 1f),
            // Üst dolap: ocağın ve tüpün üstünde, kapakları açık, rafları boş.
            Sus(Sablon("Ust_Dolap"), 8.75f, 1.45f, -0.80f, 0f, 1f),
            // Masanın yerinde gazeteye sarılı tabaklarla dolu açık koli.
            Sus(koliAcik, 8.80f, 0f, 1.60f, -1f, 0.3f),
            Sus(Sablon("Gazeteli_Tabaklar"), 8.80f, 0f, 1.60f, -1f, 0.3f),

            // ---------- ODA A (Şahan'ın seçimi: 2, 10, 11, 12, 13, 14, 15) ----------
            // Oda A u 7,7-10,5, v 4,75-8,1. Kapı hol duvarında (u 8,7-9,5); içeri açılır, açık kanat u 8,6-8,7
            // boyunca v 4,7-5,5'te durur. Karşı duvarda (v = 8,1) perdesi indirilmiş çıplak pencere (u 8,5-9,7).
            // Dairenin komodini pencerenin solunda (u 8,1, v 7,9); görev parçası (zincir aşamasında): kat
            // anahtarı onun kapalı çekmecesinde. Radyo bu katta yok.
            // Komodinin üstünde fişi çekili abajur.
            Sus(Sablon("Abajur"), 8.10f, 0.55f, 7.85f, 0f, -1f),
            // Şilte: pencere duvarının önünde yerde, başı komodine dönük; üstünde yarım toplanmış açık bavul.
            Sus(Sablon("Silte"), 9.45f, 0f, 7.38f, 1f, 0f),
            Sus(Sablon("Acik_Bavul"), 9.25f, 0.165f, 7.40f, 0f, -1f),
            // Açık gardırop: Oda B duvarında, kapakları ardına kadar açık, içinde yalnızca tel askılar.
            Sus(Sablon("Acik_Gardirop"), 7.70f + 0.275f, 0f, 6.05f, 1f, 0f),
            // Sökülmüş karyola: başlık, ayak ucu ve yan tahtalar dip duvara (u = 10,5) dayalı.
            Sus(Sablon("Sokuk_Karyola"), 10.50f, 0f, 5.77f, -1f, 0f),
            // Koliler: hol duvarında, kapının solunda; üstlerinde düğün fotoğrafının izi ve boş çivisi.
            Sus(koliBuyuk, 8.12f, 0f, 4.75f + 0.21f, 0f, 1f),
            Sus(koliKucuk, 8.10f, 0.40f, 4.75f + 0.21f, 0.15f, 1f),
            Sus(Sablon("Tablo_Izi_Genis"), 8.12f, 1.65f, 4.75f, 0f, 1f),
        };
        // Boşaltılmış buzdolabı: fişi çekilmiş, kapısı aralık.
        v.acikParcalar = new List<AcikParca> { new AcikParca { Yol = "Esyalar/Buzdolabi/Kapi", Oran = 0.35f } };
        v.buzdolabiFisiCekili = true;

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc sonuc = z.Cozumle(odalar);
        if (!sonuc.Cozulebilir) Debug.LogError("[ZINCIR] 1. kat zinciri çözülemiyor: " + string.Join("; ", sonuc.Hatalar));
        else Debug.Log("[ZINCIR] 1. kat zinciri çözülebilir: " + string.Join(" -> ", sonuc.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // Goblen şablonu (#83): çerçeve ve kanaviçe (Goblen.fbx); kanaviçeye Goblen_Degirmen dokulu malzeme
    // takılır (Assets/Materials/Goblen.mat, yoksa kurulur). FBX'in içindeki düz malzeme yerine kendi
    // malzememiz: dokunun yolu FBX'te saklanmıyor (aktarımda silinir), Unity onu kendi bulamaz.
    static GameObject GoblenSablonuKur()
    {
        // Biraz soluk (yıllarca duvarda kalmış), kumaş: parlamaz.
        Material m = DokuluMalzeme("Goblen", "Assets/Textures/Goblen_Degirmen.png", new Color(0.85f, 0.85f, 0.85f), 0.05f);

        GameObject gecici = new GameObject("Goblen");
        GameObject cerceve = Model("Cerceve", gecici.transform, "Goblen", "Cerceve", Vector3.zero, 0f, carpisma: false);
        GameObject kanavice = Model("Kanavice", gecici.transform, "Goblen", "Kanavice", Vector3.zero, 0f, carpisma: false);
        if (cerceve == null || kanavice == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Goblen.fbx yok."); return null; }
        kanavice.GetComponent<MeshRenderer>().sharedMaterial = m;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0f, 0.015f);
        c.size = new Vector3(0.58f, 0.46f, 0.03f);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Goblen.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Kavanoz camı (#83): saydam, hafif yeşilimsi, parlak. Dairenin zincir kavanozu ve mutfaktaki süs
    // kavanozları aynı malzemeyi kullanır; içleri (anahtar, turşu...) camın arkasından görünür.
    public static Material KavanozCami()
    {
        const string Yol = "Assets/Materials/Kavanoz_Cam.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, Yol);
        }
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetColor("_BaseColor", new Color(0.60f, 0.72f, 0.64f, 0.22f));
        m.SetFloat("_Smoothness", 0.92f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(m);
        return m;
    }

    // Süs kavanozu şablonu (#83): dairenin kavanoz modeli (gövde + kapak, saydam cam), içindekiler
    // (Kavanoz_Icerik.fbx'ten bir parça), istenirse kapağın üstünde kareli bez; kökünde KirilanKavanoz
    // (tornavidayla kırılır). Pivot tabanın ortası.
    static GameObject KavanozSusSablonuKur(string icerik, bool bezli, GameObject kirik)
    {
        GameObject gecici = new GameObject("Kavanoz_" + icerik);
        GameObject govde = Model("Govde", gecici.transform, "Kavanoz", "Kavanoz", Vector3.zero, 0f, carpisma: false);
        GameObject kapak = Model("Kapak", gecici.transform, "Kavanoz", "Kapak", Vector3.zero, 0f, carpisma: false);
        GameObject ic = Model("Icerik", gecici.transform, "Kavanoz_Icerik", icerik, Vector3.zero, 0f, carpisma: false);
        if (govde == null || kapak == null || ic == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Kavanoz.fbx ya da Kavanoz_Icerik.fbx yok."); return null; }
        govde.GetComponent<MeshRenderer>().sharedMaterial = KavanozCami();
        if (bezli) Model("Bez", gecici.transform, "Kavanoz_Icerik", "Bez", Vector3.zero, 0f, carpisma: false);
        // Tek kutu çarpıştırıcı: kavanozun tamamı (nişan küçük bir hedefi de yakalasın).
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.08f, 0f);
        c.size = new Vector3(0.12f, 0.16f, 0.12f);
        gecici.AddComponent<KirilanKavanoz>().Hazirla(kirik);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Kavanoz_{icerik}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Dokulu malzeme: yoksa kurar, varsa dokusunu ve değerlerini yeniler (Goblen, halı).
    static Material DokuluMalzeme(string ad, string doku, Color renk, float parlaklik)
    {
        string yol = $"Assets/Materials/{ad}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, yol);
        }
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(doku));
        m.SetColor("_BaseColor", renk);
        m.SetFloat("_Smoothness", parlaklik);
        EditorUtility.SetDirty(m);
        return m;
    }

    // Yarı rulo halı şablonu (#83): düz kısım (Hali, dokulu) ve sarılmış kısım (Rulo) aynı pivotta;
    // pivot halının tamamının ortası, izin şablonuyla aynı nokta.
    static GameObject HaliYariSablonuKur()
    {
        Material m = DokuluMalzeme("Hali_Salon", "Assets/Textures/Hali_Salon.png", new Color(0.8f, 0.8f, 0.8f), 0.02f);
        GameObject gecici = new GameObject("Hali_Yari");
        GameObject hali = Model("Hali", gecici.transform, "Hali_Yari", "Hali", Vector3.zero, 0f, carpisma: false);
        GameObject rulo = Model("Rulo", gecici.transform, "Hali_Yari", "Rulo", Vector3.zero, 0f, carpisma: false);
        if (hali == null || rulo == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Hali_Yari.fbx yok."); return null; }
        // Düz kısmın ilk malzemesi yün (dokulu), öbürleri kenar ve saçak: yalnız yün değiştirilir.
        MeshRenderer r = hali.GetComponent<MeshRenderer>();
        Material[] mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) if (mats[i] != null && mats[i].name.StartsWith("Hali_Yun")) mats[i] = m;
        r.sharedMaterials = mats;
        // Çarpıştırıcı yalnız rulo için (düz halı 8 mm; üstünden yürünür).
        Bounds b = rulo.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center + rulo.transform.localPosition;
        c.size = b.size;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Hali_Yari.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Tül perde şablonu (#83): korniş ve pilili tül; tüle yarı saydam, iki yüzü görünen malzeme.
    static GameObject PerdeSablonuKur()
    {
        const string TulYolu = "Assets/Materials/Tul.mat";
        Material t = AssetDatabase.LoadAssetAtPath<Material>(TulYolu);
        if (t == null)
        {
            t = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(t, TulYolu);
        }
        t.SetFloat("_Surface", 1f);
        t.SetFloat("_Blend", 0f);
        t.SetFloat("_Cull", 0f);                                     // iki yüz: pencereden de içeriden de görünsün
        t.SetColor("_BaseColor", new Color(0.86f, 0.85f, 0.80f, 0.55f));
        t.SetFloat("_Smoothness", 0f);
        t.SetOverrideTag("RenderType", "Transparent");
        t.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        t.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        t.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        t.SetInt("_ZWrite", 0);
        t.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(t);

        GameObject gecici = new GameObject("Perde");
        GameObject kornis = Model("Kornis", gecici.transform, "Perde", "Kornis", Vector3.zero, 0f, carpisma: false);
        GameObject tul = Model("Tul", gecici.transform, "Perde", "Tul", Vector3.zero, 0f, carpisma: false);
        if (kornis == null || tul == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Perde.fbx yok."); return null; }
        tul.GetComponent<MeshRenderer>().sharedMaterial = t;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Perde.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Tablo izi şablonu (#83): indirilmiş bir tablonun duvarda bıraktığı açık renkli dikdörtgen ve üstünde
    // boşta kalan çivi. İz, duvarın rengini biraz açan yarı saydam beyaz bir dörtgen (Unity'nin hazır
    // Quad'ı): böylece her kattaki duvar kaplamasında çalışır. en = 0 ise yalnızca çivi. Çarpıştırıcı
    // yok: oyuncunun nişanını engellemesin. yatay: yere yatık iz (kaldırılan halının parkede bıraktığı), çivisiz.
    static GameObject TabloIziSablonuKur(string ad, float en, float boy, bool yatay = false)
    {
        const string IzYolu = "Assets/Materials/Tablo_Izi.mat";
        Material iz = AssetDatabase.LoadAssetAtPath<Material>(IzYolu);
        if (iz == null)
        {
            iz = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(iz, IzYolu);
        }
        // URP Lit, yarı saydam (Surface Type = Transparent); ayarlar MobilyaKurucu'daki camınkiyle aynı.
        iz.SetFloat("_Surface", 1f);
        iz.SetFloat("_Blend", 0f);
        iz.SetColor("_BaseColor", new Color(0.95f, 0.90f, 0.80f, 0.09f));
        iz.SetFloat("_Smoothness", 0f);
        iz.SetOverrideTag("RenderType", "Transparent");
        iz.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        iz.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        iz.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        iz.SetInt("_ZWrite", 0);
        iz.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(iz);

        const string CiviYolu = "Assets/Materials/Tablo_Civi.mat";
        Material civiM = AssetDatabase.LoadAssetAtPath<Material>(CiviYolu);
        if (civiM == null)
        {
            civiM = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            civiM.SetColor("_BaseColor", new Color(0.12f, 0.11f, 0.10f));
            civiM.SetFloat("_Metallic", 0.6f);
            AssetDatabase.CreateAsset(civiM, CiviYolu);
        }

        GameObject gecici = new GameObject(ad);
        if (en > 0f)
        {
            // Quad'ın görünen yüzü kendi -Z'sine bakar; şablonun önü +Z olduğu için 180° çevrilir.
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Iz";
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(gecici.transform, false);
            q.transform.localPosition = yatay ? new Vector3(0f, 0.002f, 0f) : new Vector3(0f, 0f, 0.003f);   // yüzeyden 2-3 mm: titremesin
            // Duvarda: yüzü şablonun +Z'sine (odaya) dönük. Yerde: yüzü yukarı (Quad'ın -Z'si +Y'ye döner).
            q.transform.localRotation = yatay ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f);
            q.transform.localScale = new Vector3(en, boy, 1f);
            q.GetComponent<MeshRenderer>().sharedMaterial = iz;
        }
        if (yatay)
        {
            GameObject yatik = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
            Object.DestroyImmediate(gecici);
            return yatik;
        }
        // Çivi: izin üst kenarının biraz altında, duvardan 1 cm çıkık.
        GameObject civi = GameObject.CreatePrimitive(PrimitiveType.Cube);
        civi.name = "Civi";
        Object.DestroyImmediate(civi.GetComponent<Collider>());
        civi.transform.SetParent(gecici.transform, false);
        civi.transform.localPosition = new Vector3(0f, en > 0f ? boy / 2f - 0.05f : 0f, 0.005f);
        civi.transform.localScale = new Vector3(0.006f, 0.006f, 0.012f);
        civi.GetComponent<MeshRenderer>().sharedMaterial = civiM;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Salondaki vitrinin (vintage_cabinet_01) alt dolabının üst yüzü: fincan buraya konur. Oyunda
    // aşağı ışın atılarak ölçüldü.
    const float VitrinTezgahi = 0.912f;

    // Dekor şablonunu adıyla bulur (Assets/Prefabs/Esyalar/<ad>.prefab); ZincirleriKur önce kurar.
    static GameObject Sablon(string ad)
    {
        GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>($"{SablonKlasoru}/{ad}.prefab");
        if (g == null) Debug.LogWarning($"[ZINCIR] {ad}.prefab yok.");
        return g;
    }

    // 2. kat salonunun ölçüleri (oyunda aşağı ışın atılarak ölçüldü): sehpanın üst yüzü, sallanan
    // sandalyenin oturağı, kanepe sırtının üst ön kenarı (plan u) ve yüksekliği.
    const float SehpaUstu = 0.52f;
    const float SandalyeOturagi = 0.43f;
    const float KanepeSirtU = 0.17f;    // sırtın tepesi u 0,12'de (0,80), öne doğru yuvarlanır
    const float KanepeSirtY = 0.80f;

    // Düz halı şablonu (#83): halı (Blender hali_duz.py, adı <ad>) ve yününe o katın desen dokusu.
    static GameObject HaliDuzSablonuKur(string ad, string doku)
    {
        Material m = DokuluMalzeme(ad, doku, new Color(0.8f, 0.8f, 0.8f), 0.02f);
        GameObject gecici = new GameObject(ad);
        GameObject h = Model("Hali", gecici.transform, ad, ad, Vector3.zero, 0f, carpisma: false);
        if (h == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {ad}.fbx yok."); return null; }
        MeshRenderer r = h.GetComponent<MeshRenderer>();
        Material[] mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) if (mats[i] != null && mats[i].name.StartsWith(ad + "_Yun")) mats[i] = m;
        r.sharedMaterials = mats;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Kadife perde şablonu (#83, 2. kat): korniş, fon perde ve tül; tüle 1. kattaki yarı saydam malzeme.
    static GameObject PerdeKadifeSablonuKur()
    {
        GameObject gecici = new GameObject("Perde_Kadife");
        GameObject kornis = Model("Kornis", gecici.transform, "Perde_Kadife", "Kornis", Vector3.zero, 0f, carpisma: false);
        GameObject fon = Model("Fon", gecici.transform, "Perde_Kadife", "Fon", Vector3.zero, 0f, carpisma: false);
        GameObject tul = Model("Tul", gecici.transform, "Perde_Kadife", "Tul", Vector3.zero, 0f, carpisma: false);
        if (kornis == null || fon == null || tul == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Perde_Kadife.fbx yok."); return null; }
        Material t = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tul.mat");
        if (t != null) tul.GetComponent<MeshRenderer>().sharedMaterial = t;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Perde_Kadife.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Duvar portresi şablonu (#83): çerçeve ve fotoğraf yüzeyi (Portre_Cercevesi.fbx); fotoğraf dokusu
    // yerel SDXL ile üretilip eskitilir (Art/Fotograflar/scripts: foto_uret.py PORTRELER, foto_isle.py).
    static GameObject PortreSablonuKur(string ad, string doku)
    {
        Material m = DokuluMalzeme(ad, doku, new Color(0.9f, 0.9f, 0.9f), 0.15f);
        GameObject gecici = new GameObject(ad);
        GameObject c = Model("Cerceve", gecici.transform, "Portre_Cercevesi", "Cerceve", Vector3.zero, 0f, carpisma: false);
        GameObject r = Model("Resim", gecici.transform, "Portre_Cercevesi", "Resim", Vector3.zero, 0f, carpisma: false);
        if (c == null || r == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Portre_Cercevesi.fbx yok."); return null; }
        r.GetComponent<MeshRenderer>().sharedMaterial = m;
        BoxCollider bc = gecici.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0f, 0.02f);
        bc.size = new Vector3(0.59f, 0.71f, 0.04f);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Yolluk şablonu (#83, 2. kat holü): halı ve uç kenarları (Yolluk.fbx); yüne desenli doku takılır.
    // Doku 1,6 m'lik bir tekrar birimi; UV'si boy boyunca tekrar eder (dokunun sarma kipi Repeat).
    static GameObject YollukSablonuKur()
    {
        Material m = DokuluMalzeme("Yolluk_Nuri", "Assets/Textures/Yolluk_Nuri.png", new Color(0.8f, 0.8f, 0.8f), 0.02f);
        GameObject gecici = new GameObject("Yolluk");
        GameObject y = Model("Hali", gecici.transform, "Yolluk", "Yolluk", Vector3.zero, 0f, carpisma: false);
        if (y == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Yolluk.fbx yok."); return null; }
        MeshRenderer r = y.GetComponent<MeshRenderer>();
        Material[] mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) if (mats[i] != null && mats[i].name.StartsWith("Yolluk_Yun")) mats[i] = m;
        r.sharedMaterials = mats;
        // Çarpıştırıcı yok: 1 cm'lik halı ve 5 cm'lik kıvrım yürürken takılmasın, nişanı da tutmasın.
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Yolluk.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Sarkaçlı duvar saati şablonu (#83, 2. kat): kasa (Saat) ve sarkaç (Sarkac, SarkacSallanir ile sallanır).
    // Kasanın alt camı saydam malzemeyle değiştirilir: sarkaç camın arkasından görünsün.
    static GameObject SaatSablonuKur()
    {
        GameObject gecici = new GameObject("Sarkacli_Saat");
        GameObject saat = Model("Saat", gecici.transform, "Sarkacli_Saat", "Saat", Vector3.zero, 0f, carpisma: false);
        if (saat == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Sarkacli_Saat.fbx yok."); return null; }
        GameObject sarkac = Model("Sarkac", saat.transform, "Sarkacli_Saat", "Sarkac", Vector3.zero, 0f, carpisma: false);
        if (sarkac != null) sarkac.AddComponent<SarkacSallanir>();
        MeshRenderer r = saat.GetComponent<MeshRenderer>();
        Material[] mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) if (mats[i] != null && mats[i].name.StartsWith("Saat_Cam")) mats[i] = KavanozCami();
        r.sharedMaterials = mats;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.05f, 0.07f);
        c.size = new Vector3(0.38f, 1.04f, 0.14f);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Sarkacli_Saat.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Telefon takımı şablonu (#83): dantel + rehber + sökülmüş kablo (Telefon_Takimi.fbx) ve üstünde
    // kapıcı odasının telefonu (Telefon.fbx: gövde ve ahize). Telefonun pivotu arka kenarının ortası;
    // telefon_takimi.py dantelin merkezini Blender'da (x -0,05, y 0) koyduğu için telefon Unity'de
    // (0,05, 0,11) noktasına oturur (Blender (x, y, z) -> Unity (-x, z, -y)).
    static GameObject TelefonTakimiSablonuKur()
    {
        GameObject gecici = new GameObject("Telefon_Takimi");
        GameObject takim = Model("Takim", gecici.transform, "Telefon_Takimi", null, Vector3.zero, 0f, carpisma: false);
        GameObject telefon = Model("Telefon", gecici.transform, "Telefon", "Telefon", new Vector3(0.05f, 0.002f, -0.11f), 0f, carpisma: false);
        if (takim == null || telefon == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Telefon_Takimi.fbx ya da Telefon.fbx yok."); return null; }
        Model("Ahize", telefon.transform, "Telefon", "Ahize", Vector3.zero, 0f, carpisma: false);
        // Tek kutu çarpıştırıcı: sehpanın üstündeki takımın hepsini kaplar.
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.08f, 0f);
        c.size = new Vector3(0.44f, 0.16f, 0.34f);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Telefon_Takimi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Bir çift terlik şablonu (#83): terlik modelleri tek terlik (sol); sağ terlik x ölçeği -1 ile aynası
    // (sahanlıktaki terliklerle aynı yol). Burunlar modelin -Z'sinde.
    static GameObject TerlikCiftiSablonuKur(string tur)
    {
        GameObject gecici = new GameObject("Terlik_Cifti_" + tur);
        GameObject sol = Model("Sol", gecici.transform, "Terlik_" + tur, null, new Vector3(-0.055f, 0f, 0f), 0f, carpisma: false);
        GameObject sag = Model("Sag", gecici.transform, "Terlik_" + tur, null, new Vector3(0.055f, 0f, 0.02f), 0f, carpisma: false);
        if (sol == null || sag == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] Terlik_{tur}.fbx yok."); return null; }
        sag.transform.localScale = new Vector3(-1f, 1f, 1f);
        // İnce kutu çarpıştırıcı: oyuncu üstünden geçer (basamak yüksekliğinin çok altında).
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.03f, 0f);
        c.size = new Vector3(0.24f, 0.06f, 0.28f);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Terlik_Cifti_{tur}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // ---------- 2. KAT: YENİDEN TASARLANIYOR (#83, Şahan, 2026-09-29) ----------
    // Daire 3 (kapıda 7), yaşlı Nuri Bey'in evi: eşini kaybetmiş, yalnız yaşayan, radyo meraklısı emekli.
    // Birkaç gün önce hastaneye kaldırılmış (Yönetim'in notu); ev dolu ve yaşanmış, aniden bırakılmış.
    // Katın imza mekaniği sesle bulma (#62): radyo kapalı bir odada kendiliğinden çalar.
    // Kat oda oda yeniden döşeniyor (1. kattaki gibi); zincir, kapılar ve görev parçaları odalar
    // bitince seçilecek. Kural: katlarda aynı model eşya kullanılmaz; ipucu notu yok (Nuri'nin
    // "şifre radyonun istasyonu" notu kaldırıldı); bir odada en fazla bir görev parçası.
    // Eski tasarım (radyo 102.7 -> kiler asma kilidi -> alet çantasında kat anahtarı) git geçmişinde.
    static KatZinciriVarligi Kat2Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_2.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "2. kat, daire 3, yaşlı Nuri Bey (hastanede). YENİDEN TASARLANIYOR: odalar oda oda döşeniyor " +
                     "(antre, hol ve salon bitti); zincir yok, odalar kilitsiz, kat anahtarı sahanlıkta.";
        v.tasarimda = true;
        v.yalnizSecilenEsyalar = true;
        v.zincir = new KilitZinciri();
        v.sesler = new List<KatSesi>();
        v.parcalar = new List<KatParcasi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu" };

        KatSusu Sus(GameObject sablon, float u, float y, float vv, float onU, float onV)
        { return new KatSusu { Sablon = sablon, X = u, Y = y, Z = vv, OnU = onU, OnV = onV }; }

        // ---------- ANTRE (Şahan'ın seçimi: 1, 2, 3, 4, 7, 8, 9, 10, 11) ----------
        // Antre u 0-2,6, v -0,8-2,85. Bu dairede giriş kapısı içeri ve +v tarafına açılır: açık kanat
        // u 0-0,8, v 0,26-0,48 boyunca durur (1. kattaki dairenin aynası). Kapıdan hole giden yol ortada boş.
        v.notlar = new List<KatNotu>
        {
            // Yönetim'in notu: sigorta kutusunun yanında (hikâye: Nuri Bey hastaneye kaldırıldı).
            new KatNotu { Metin = "not.kat2_hastane", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        v.suslar = new List<KatSusu>
        {
            // Tik portmanto: landing duvarında, kapının yanında; üstünde paltosu ve fötr şapkası,
            // yanında bastonu (ambulansla giderken hiçbirini alamamış).
            Sus(Sablon("Portmanto_Tik"), 0.03f + 0.17f, 0f, 1.75f, 1f, 0f),
            // Telefon oturağı: banyo duvarında, portmantonun karşısında; bakalit telefon prize takılı.
            Sus(Sablon("Telefon_Oturagi"), 2.60f - 0.19f, 0f, 1.20f, -1f, 0f),
            // Sarkaçlı saat: telefon oturağının yanında, hâlâ çalışıyor.
            Sus(Sablon("Sarkacli_Saat"), 2.60f, 1.55f, 2.25f, -1f, 0f),
            // Yaprak takvim: kapının yanındaki dış duvarda; hastaneye kaldırıldığı günde (12) kalmış.
            Sus(Sablon("Yaprak_Takvim"), 0.90f, 1.45f, -0.80f, 0f, 1f),
            // Kapının altından atılıp içeride biriken gazeteler ve zarflar.
            Sus(Sablon("Posta_Yigini"), 0.45f, 0f, -0.12f, 1f, 0f),
            // Terlikleri: kapının içinde, burunları eve dönük.
            Sus(Sablon("Terlik_Cifti_Yasli"), 1.05f, 0f, -0.58f, -1f, 0f),

            // ---------- HOL (Şahan'ın seçimi: 1, 2, 3, 4, 5, 9, 10, 11, 12, 13, 14) ----------
            // Hol u 0-10,5, v 2,95-4,65. Güney duvarı (v = 2,95) kapıları: banyo u 3,25-3,95, kiler 5,25-6,05,
            // mutfak 7,0-7,8. Kuzey duvarı (v = 4,65): salon 0,85-1,75, Oda B 5,95-6,75, Oda A 8,7-9,5.
            // Holün başı düzenli (tablo, kitaplık, fotoğraf); ortası olay yeri: Nuri Bey burada fenalaşmış.
            // Yolluk: baştan dibe serili; buruşmuş kıvrımı olay yerinde (u ~4,6).
            Sus(Sablon("Yolluk"), 5.15f, 0f, 3.80f, 0f, 1f),
            Sus(Sablon("Petek"), 10.50f, 0f, 3.80f, -1f, 0f),
            // Holün başı: salon ile Oda B kapıları arasında yağlı boya tablo, altında kitaplık, rafın
            // üstünde Nuri Bey'in aile fotoğrafı (Rıza Efendi'yle, 1989; masa çerçevesi).
            Sus(Sablon("Kitaplik"), 2.60f, 0f, 4.65f - 0.15f, 0f, -1f),
            M("hanging_picture_frame_02", 2.60f, 1.72f, 4.65f, 0f, -1f),
            Foto("kat2", false, 2.88f, 1.10f, 4.52f, 0f, -1f),
            // Kuş kafesi: askılı ayağıyla Oda B kapısının yanında; yem kabı boş.
            Sus(Sablon("Kus_Kafesi"), 5.25f, 0f, 4.40f, 0f, -1f),
            // Olay yeri: kiler kapısının yanında kapağı açık ilaç dolabı; altında saçılmış ilaçlar,
            // yan yatmış hasır sandalye ve kırık gözlük.
            Sus(Sablon("Ilac_Dolabi"), 4.60f, 1.45f, 2.95f, 0f, 1f),
            Sus(Sablon("Sacilmis_Ilaclar"), 4.65f, 0.004f, 3.45f, 0f, 1f),
            Sus(Sablon("Devrik_Sandalye"), 4.05f, 0f, 3.72f, 0.4f, 1f),
            Sus(Sablon("Kirik_Gozluk"), 5.05f, 0.008f, 3.95f, 0.3f, 1f),

            // ---------- SALON (Şahan'ın seçimi: 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14) ----------
            // Salon u 0-5,0, v 4,75-8,1. Kapı hol duvarında (u 0,85-1,75); bu dairede içeri, kanepe tarafına
            // açılır, açık kanat u 0,7-0,9, v 4,7-5,6'da durur. Karşı duvarda pencere (u 0,7-2,1, denizlik
            // 0,87-0,90 m, v 8,04-8,20) ve hep kilitli balkon kapısı (u 3,2-4,0). Dolu, yaşanmış bir salon.
            // Pencere: kadife fon perde ve tül; denizlikte kurumuş sardunyalar; altında petek.
            Sus(Sablon("Perde_Kadife"), 1.40f, 2.32f, 8.10f, 0f, -1f),
            Sus(Sablon("Kuru_Sardunya"), 1.40f, 0.90f, 8.12f, 0f, -1f),
            Sus(Sablon("Petek"), 1.40f, 0f, 8.10f, 0f, -1f),
            // Rahmetli eşinin portresi: pencere ile balkon kapısının arasında, köşesinde yas kurdelesi.
            Sus(Sablon("Portre_Kat2_Es"), 2.66f, 1.60f, 8.10f, 0f, -1f),
            // Lacivert halı ortada; üstünde kanepe, sehpa ve berjer.
            Sus(Sablon("Hali_Nuri"), 2.60f, 0f, 6.45f, 0f, 1f),
            M("Sofa_01", 0.03f + 0.40f, 0f, 6.50f, 1f, 0f),
            M("CoffeeTable_01", 1.50f, 0f, 6.50f, 1f, 0f),
            M("ArmChair_01", 2.35f, 0f, 5.30f, 1f, 0.6f),
            // Sehpada soğumuş çay ve çaydanlık, açık gazete ve boş gözlük kılıfı (gözlüğü holde kırık).
            Sus(Sablon("Cay_Takimi"), 1.55f, SehpaUstu, 6.00f, 1f, 0f),
            Sus(Sablon("Gazete_Kilif"), 1.45f, SehpaUstu, 6.80f, 1f, 0.15f),
            // Pencere köşesinde sallanan sandalye ve üstünde katlı örgü battaniye: Nuri Bey'in radyo köşesi.
            M("Rockingchair_01", 0.62f, 0f, 7.68f, 1f, -0.5f),
            Sus(Sablon("Orgu_Battaniye"), 0.62f + 0.02f, SandalyeOturagi, 7.68f - 0.01f, 1f, -0.5f),
            // Koltuk dantelleri: kanepenin sırtında üç, berjerin sırtında bir.
            Sus(Sablon("Koltuk_Danteli"), KanepeSirtU, KanepeSirtY, 6.00f, 1f, 0f),
            Sus(Sablon("Koltuk_Danteli"), KanepeSirtU, KanepeSirtY, 6.50f, 1f, 0f),
            Sus(Sablon("Koltuk_Danteli"), KanepeSirtU, KanepeSirtY, 7.00f, 1f, 0f),
            Sus(Sablon("Koltuk_Danteli"), 2.10f, 1.06f, 5.16f, 1f, 0.6f),   // berjerin sırtının tepesi
            // TV konsolu ve ahşap kasalı televizyon: Oda B duvarında, kanepeye bakar.
            M("ClassicConsole_01", 5.00f - 0.295f, 0f, 6.40f, -1f, 0f),
            M("Television_01", 4.75f, 0.95f, 6.40f, -1f, 0f),
            // Kristal avize: halının ortasının üstünde.
            M("Chandelier_02", 2.60f, Tavan, 6.45f, 0f, 1f),
            // Eşinden kalma dikiş makinesi: balkon kapısının yanındaki köşede, üstü dantel örtülü.
            Sus(Sablon("Dikis_Makinesi"), 4.50f, 0f, 8.10f - 0.225f, 0f, -1f),
        };
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 3. KAT: PARÇA BİRLEŞTİRME (#63, Docs/hikaye.md §7) ----------
    // Daire 1, çocuklu aile (Kerem). Kerem "dokuzuncu kattaki adamı" çizmiş, resmi dört parçaya
    // yırtıp saklamış; annesi yatak odasının (Oda A) kilidinin şifresini o resme yazdırmış
    // (koridordaki notu). Banyo ve kiler kapalı; salon ve mutfak açık.
    //   1. Kapının önündeki ayakkabılıkta ilk parça: oyuncu daha içeri girmeden merak eder.
    //   2. Salonda yerde ikinci parça (kapının arkasındaki köşede).
    //   3. Mutfakta: çekmecede Oda B'nin (çocuk odası) anahtarı, buzdolabında üçüncü parça.
    //   4. Oda B'de oyuncak sandığında dördüncü parça -> resim tamam: köşelerde 0 3 0 9.
    //   5. Oda A şifreli asma kilitli (0309); komodinin çekmecesinde katın anahtarı.
    // İki kilit (odaB anahtarlı, odaA parçalı şifreli): büyüme çizelgesinin 3. katı (1-1-2-...).
    static KatZinciriVarligi Kat3Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_3.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "3. kat, daire 1, çocuklu aile (parça birleştirme). Kerem'in yırtık çiziminin dört parçası: ayakkabılıkta, " +
                     "salonda yerde, buzdolabında, Oda B'deki oyuncak sandığında. Oda B'nin anahtarı mutfak çekmecesinde. " +
                     "Birleşen resmin köşelerinde 0 3 0 9: Oda A'nın şifresi; komodinde katın anahtarı. Banyo ve kiler kapalı.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        // Parça salonda kanepenin ALTINDA (#79): oyuncu eğilip (Ctrl) bakınca görür ve alır.
        // Eskiden antre duvarı tarafındaki köşedeydi (u 0,55, v 6,9); 5. katla her daireye gelen
        // gardırop (#65) o köşeyi kapladı ve parça gardırobun içinde kaldı. Kanepe (Sofa_01, 3. katın
        // mobilyası, #73) odalar arası duvarın önünde: ön kenarı u 4,32, altında 17 cm boşluk.
        // Parça ön kenardan 15 cm içeride: ayakta dururken kanepenin kenarı görüşü kapatır.
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", PlandaNokta = true, Oda = "Salon", U = 4.47f, Y = 0.004f, V = 6.45f, Donus = 25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p1", Tur = EsyaTuru.Parca, Yer = "ayakkabilik", Y = 1.025f, Donus = -15f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p2", Tur = EsyaTuru.Parca, Yer = "salon_yeri" });
        // Çekmecenin içi: tezgâhın ekseninde x 0,59, y 0,64, z 0,04 (tezgah.py'deki üst çekmecenin tabanı).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p3", Tur = EsyaTuru.Parca, Yer = "buzdolabi", Y = 0.47f, Donus = 10f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p4", Tur = EsyaTuru.Parca, Yer = "oyuncak", Y = SandikTaban, Donus = -20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.375f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaB_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Sifre = "0309",
                                          Parcalar = new List<string> { "p1", "p2", "p3", "p4" } });
        v.zincir = z;

        // PARÇALAR: dokular Art/Cizim/scripts/cizim_uret.py'den. Her parçanın dokudaki bölgesi
        // dosyanın saydam olmayan piksellerinden hesaplanır: resim yeniden üretilince de doğru kalır.
        v.parcalar = new List<KatParcasi>();
        for (int i = 1; i <= 4; i++)
        {
            string dokuYolu = $"Assets/Textures/Cizim/Cizim_Kat3_P{i}.png";
            CizimDokusunuAyarla(dokuYolu);
            v.parcalar.Add(new KatParcasi { Kimlik = "p" + i, Doku = AssetDatabase.LoadAssetAtPath<Texture2D>(dokuYolu), Bolge = ParcaBolgesi(dokuYolu) });
        }
        v.cizimBoyu = 0.30f;

        // NOT: annenin babaya notu koridorda, Oda B'nin kapısının solunda (kapı u = 5,95-6,75,
        // v = 4,70 duvarında). Koridorun o duvardaki yüzü v = 4,65; not koridora (-v) bakar.
        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat3_anne", X = 5.45f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
        };
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s3 = z.Cozumle(odalar);
        if (!s3.Cozulebilir) Debug.LogError("[ZINCIR] 3. kat zinciri çözülemiyor: " + string.Join("; ", s3.Hatalar));
        else Debug.Log("[ZINCIR] 3. kat zinciri çözülebilir: " + string.Join(" -> ", s3.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 4. KAT: IŞIK VE KARANLIK (#64, Docs/hikaye.md §7) ----------
    // Daire 4, boş ve elektriksiz. Elektriği önceki kapıcı Hasan kesmiş, sigortayı mutfak
    // dolabına kilitlemiş (antredeki notu). Yalnızca salon ve mutfak açık.
    //   1. KARANLIK: dolabın şifresi salonun duvarına fosforlu boyayla yazılı. Kafa lambası
    //      açıkken görünmez; oyuncu lambayı söndürüp beklerse rakamlar belirir (KaranliktaParlayan).
    //   2. Dolapta sigorta; antredeki sigorta kutusuna takılınca elektrik gelir.
    //   3. IŞIK: antre, koridor, salon ve mutfağın tavan lambaları titreyerek yanar; buzdolabı
    //      açılır, iç ışığında Hasan'ın ikinci notu ve katın anahtarı.
    // İki kilit (dolap şifreli, buzdolabı sigortalı): büyüme çizelgesinin 4. katı.
    static KatZinciriVarligi Kat4Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_4.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "4. kat, daire 4, boş ve elektriksiz (ışık ve karanlık). Dolabın şifresi salon duvarında fosforlu boyayla: " +
                     "yalnızca kafa lambası kapalıyken görünür. Dolapta sigorta; takılınca lambalar yanar ve buzdolabı açılır, " +
                     "içinde Hasan'ın notu ve katın anahtarı. Yalnızca salon ve mutfak açık.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_A", "Oda_B", "Oda_C" });
        // Salonla Oda B arasındaki duvar (u = 5,0), takvimin (v 5,84-6,16) pencere tarafında. Yazı
        // duvarın 4,5 cm önünde, göz hizasının biraz üstünde; ön yüzü odaya (-u) bakar.
        // Eskiden landing tarafındaki duvardaydı (u 0,045, v 6,4); 5. katla her daireye gelen
        // gardırop (#65) tam önünde durduğu için yazı görünmüyordu.
        z.Yerler.Add(new ZincirYeri { Kimlik = "duvar", PlandaNokta = true, Oda = "Salon", U = 4.955f, Y = 1.60f, V = 7.2f, OnU = -1f, OnV = 0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "dolap", Yol = "Esyalar/Tezgah", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kapak_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        // Sigorta dolabın iç rafında (tezgah.py: raf üstü 0,47 m; birinci kapağın arkası x 0).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "dolap", X = 0f, Y = 0.475f, Z = 0.02f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "buzdolabi", Y = 0.47f, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "dolap", Tur = KilitTuru.Sifre, OdaKapisi = false, Hedef = "dolap", Gerekli = "duvar",
                                          Sifre = "4816", IpucuMetni = "4816", IpucuOlcek = 4f, IpucuKaranlikta = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "buzdolabi", Tur = KilitTuru.Sigorta, OdaKapisi = false, Hedef = "buzdolabi", Gerekli = "sigorta" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Hasan'ın ilk notu antrede, sigorta kutusunun yanında (önceki katların notlarının yeri).
            new KatNotu { Metin = "not.kat4_giris", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            // İkinci notu buzdolabının üst rafında, arka duvara dayalı (buzdolabi.py: raf 0,80 m,
            // arka iç duvar z = -0,265). Buzdolabının iç ışığı tam bu rafı aydınlatır.
            new KatNotu { Metin = "not.kat4_buzdolabi", Yol = "Esyalar/Buzdolabi", X = 0.05f, Y = 0.805f, Z = -0.255f, Donus = -6f },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();
        v.yananOdalar = new List<string> { "Antre", "Hol", "Salon", "Mutfak" };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s4 = z.Cozumle(odalar);
        if (!s4.Cozulebilir) Debug.LogError("[ZINCIR] 4. kat zinciri çözülemiyor: " + string.Join("; ", s4.Hatalar));
        else Debug.Log("[ZINCIR] 4. kat zinciri çözülebilir: " + string.Join(" -> ", s4.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 5. KAT: MEKANİZMA (#65, Docs/hikaye.md §7) ----------
    // Daire 2, Cemal'in son çalıştığı daire. Antrede Cemal'in iş listesi: banyo lavabosu
    // damlatıyor; sifonu sökmeden ana vanayı kapat (salonda gardırobun arkasında); banyo kapısı
    // içeriden kilitli, menteşeleri sök. Mutfak, Oda A ve Oda B kapalı.
    //   1. Banyo: menteşeler tornavidayla sökülür (tornavida kapıcıda yoksa kilerde, Cemal'in
    //      alet çantasında). İçeride lavabo; sifon kilitli ("vana açık, su basar").
    //   2. MEKANİZMA: salondaki gardırop yana itilir, arkasında ana su vanası. Gardırop ağır: ancak
    //      levye (kilerde) ve mobilya kaydırıcısı (banyoda, lavabonun teknesinde) bulununca kıpırdar
    //      (Şahan, 2026-09-27: "dolabı ittirmek için birkaç ekipman bulunabilir"). Ekipman kilidi.
    //   3. MEKANİZMA: vananın çarkı çevrilir: su kesilir, lavabonun damlası susar, sifon sökülebilir.
    //   4. Sifonun içinde katın anahtarı.
    // Beş kilit (Şahan'ın zorlaştırması): banyo menteşeli, mutfak kilerdeki kola bağlı, salon kırık
    // anahtarlı (iki parça), gardırop ekipmanlı, sifon vanaya bağlı "kol". Parçalar birbirini tamamlar:
    //   banyo -> (kaydırıcı) ; kiler kolu -> mutfak -> buzdolabında anahtarın başı ;
    //   alet çantasında anahtarın ucu -> salon -> gardırop (levye + kaydırıcı) -> vana -> sifon.
    static KatZinciriVarligi Kat5Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_5.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "5. kat, daire 2, Cemal'in son çalıştığı daire (mekanizma). Mutfak kilerdeki kolla açılır; salonun anahtarı kırık, " +
                     "başı buzdolabında, ucu alet çantasında. Banyo içeriden kilitli: menteşeler tornavidayla " +
                     "sökülür. Salondaki gardırop (levye kilerde, kaydırıcı banyoda bulununca) itilince arkasında ana vana; çark çevrilince su kesilir, damla susar, lavabonun " +
                     "sifonu sökülür, içinde katın anahtarı. Oda A ve Oda B kapalı.";
        var z = new KilitZinciri();
        // #65 zorlaştırma (Şahan, 2026-09-27): salon ve mutfak da kilitli. Mutfağın sürgüsü kilerdeki
        // kolla açılır; salonun anahtarı kırık, iki parçası mutfakta ve kilerde.
        z.KapaliOdalar.AddRange(new[] { "Oda_A", "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "alet_cantasi", Yol = "Esyalar/Alet_Cantasi", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        // Vananın kilidi gardırobun kendisinde (KilitYolu): gardırop itilmeden vanaya ulaşılmaz.
        z.Yerler.Add(new ZincirYeri { Kimlik = "vana", Yol = "Esyalar/Vana/Cark", Oda = "Salon", KilitYolu = "Esyalar/Gardirop" });
        // Levye kilerin yerinde, duvar kolunun önünde yatıyor.
        z.Yerler.Add(new ZincirYeri { Kimlik = "levye_yeri", PlandaNokta = true, Oda = "Oda_C", U = 6.1f, Y = 0.004f, V = 0.3f, Donus = 70f });
        // Kaydırıcı banyoda, lavabonun sağındaki köşede yerde. (Teknenin içindeyken lavabonun
        // çarpışma kutusu onu da kapsıyordu: nişan lavaboya, oradan sifona gidiyor, "KİLİTLİ"
        // çıkıyor ve kaydırıcı alınamıyordu.)
        z.Yerler.Add(new ZincirYeri { Kimlik = "tekne", PlandaNokta = true, Oda = "Banyo", U = 4.2f, Y = 0.004f, V = -0.5f, Donus = 15f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sifon", Yol = "Esyalar/Lavabo/Sifon", Oda = "Banyo" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "alet_cantasi", Y = 0.03f, Donus = 20f });
        // Kırık anahtar: baş buzdolabının alt rafında, uç Cemal'in alet çantasında (tornavidanın yanında).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "buzdolabi", X = 0.1f, Y = 0.47f, Donus = 40f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "alet_cantasi", X = -0.12f, Y = 0.03f, Donus = -25f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "levye", Tur = EsyaTuru.Levye, Yer = "levye_yeri" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kaydirici", Tur = EsyaTuru.Kaydirici, Yer = "tekne" });
        // Anahtar sifonun ağzında, kenarın hemen altında (sifonun pivotu kendi üst ortası; kap 17 cm
        // derin). 9 cm aşağıdayken yere inen sifonun içinde görünmüyordu (oynatma modu görüntüsü).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sifon", Y = -0.02f, Donus = 30f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Salon",
                                          Gerekenler = new List<string> { "salon_anahtari_bas", "salon_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "gardirop", Tur = KilitTuru.Ekipman, OdaKapisi = false, Hedef = "vana",
                                          Gerekenler = new List<string> { "levye", "kaydirici" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sifon", Tur = KilitTuru.Kol, OdaKapisi = false, Hedef = "sifon", Gerekli = "vana" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat5_liste", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.yananOdalar = new List<string>();
        // Lavabonun damlası: oyuncu daireye girer girmez duyulur; kapatılamaz; vana çevrilince
        // ("sifon" kilidi çözülünce) kalıcı olarak susar. Ses 1,6 sn arayla düzenli bir damla.
        v.sesler = new List<KatSesi>
        {
            new KatSesi { Yol = "Esyalar/Lavabo", Oda = "Banyo", Gecikme = 0f, Ses = 0.7f, Kapatilabilir = false, SusturanKilit = "sifon",
                          Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Boru_Damla.wav") },
        };
        if (v.sesler[0].Klip == null) Debug.LogError("[ZINCIR] Boru_Damla.wav bulunamadı.");

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s5 = z.Cozumle(odalar);
        if (!s5.Cozulebilir) Debug.LogError("[ZINCIR] 5. kat zinciri çözülemiyor: " + string.Join("; ", s5.Hatalar));
        else Debug.Log("[ZINCIR] 5. kat zinciri çözülebilir: " + string.Join(" -> ", s5.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 6. KAT: ÇİFT DAİRE + SIRALAMA / YERLEŞTİRME (#70, #66, Docs/hikaye.md §7) ----------
    // Daire 3 ("kimse oturmuyor" ama sofra kurulmayı bekliyor) ve yanındaki daire 4 birlikte açık.
    // Salondaki masada dört isim kartı: Rıza Efendi, Hasan, Cemal ve Yeni Kapıcı (oyuncu). Dört
    // tabak, üstlerinde isimler, iki daireye dağılmış. Hepsi doğru yere konunca büfe açılır.
    // Altı kilit; iki daire birbirinin kilidini açar (Şahan: "kat geçme zorluğunu da arttıralım"):
    //   b_kiler  : 4'ün kileri şifreli; şifre 3'ün salon takviminde (1994) -> 4'ün notu "takvime bak"
    //   a_mutfak : 3'ün mutfağı; anahtarı 4'ün buzdolabında
    //   a_odaA   : 3'ün yatak odası; anahtarı kırık: başı 4'ün kilerinde, ucu 3'ün mutfak çekmecesinde
    //   b_banyo  : 4'ün banyosu içeriden kilitli (menteşe, tornavida)
    //   b_odaA   : 4'ün yatak odası; 3'ün kilerindeki kolla açılır (Cemal'in notu)
    //   bufe     : dört tabak doğru yerlerde -> büfenin sol kapısı; içinde katın anahtarı
    // Tabaklar: Rıza 3'ün mutfak dolabında, Cemal 3'ün yatak odasında, Hasan 4'ün banyosunda,
    // Yeni Kapıcı 4'ün yatak odasında.
    static KatZinciriVarligi Kat6Kur(GameObject isimKarti, GameObject golge)
    {
        string yol = $"{ZincirKlasoru}/Kat_6.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "6. kat, daire 3 ve 4 birlikte (çift daire, sıralama / yerleştirme). Sofrada dört isim kartı; dört tabak iki " +
                     "daireye dağılmış. Altı kilit: 4'ün kileri (şifre 3'ün takviminde), 3'ün mutfağı (anahtar 4'ün buzdolabında), " +
                     "3'ün yatak odası (kırık anahtar, iki dairede), 4'ün banyosu (menteşe), 4'ün yatak odası (3'ün kilerindeki kol), büfe (sofra).";
        v.ikinciDaire = 4;
        v.ikinciKaplamaKati = 2;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_B", "B:Oda_B" });
        // YERLER: birinci daire (3)
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_takvim", Yol = "Esyalar/Takvim", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_dolap", Yol = "Esyalar/Tezgah", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kapak_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_cekmece", Yol = "Esyalar/Tezgah/Cekmece_2", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_kol", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_odaA_yer", PlandaNokta = true, Oda = "Oda_A", U = 9.3f, Y = 0.004f, V = 5.6f, Donus = 20f });
        for (int i = 1; i <= 4; i++)
            z.Yerler.Add(new ZincirYeri { Kimlik = "yer_" + i, Yol = "Esyalar/Sofra/Yer_" + i, Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "bufe", Yol = "Esyalar/Bufe", Oda = "Salon", KilitYolu = "Esyalar/Bufe/Kapi_1" });
        // YERLER: ikinci daire (4)
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_buzdolabi", Yol = "B:Esyalar/Buzdolabi", Oda = "B:Mutfak", KilitYolu = "B:Esyalar/Buzdolabi/Kapi" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_alet", Yol = "B:Esyalar/Alet_Cantasi", Oda = "B:Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_banyo_yer", PlandaNokta = true, Oda = "B:Banyo", U = 4.2f, Y = 0.004f, V = -0.5f, Donus = -15f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_odaA_yer", PlandaNokta = true, Oda = "B:Oda_A", U = 9.3f, Y = 0.004f, V = 5.6f, Donus = 35f });

        // EŞYALAR
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "a_mutfak_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "b_buzdolabi", Y = 0.47f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "b_alet", Y = 0.03f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "b_alet", X = -0.12f, Y = 0.03f, Donus = -25f });
        // Tezgâhın ikinci çekmecesinin tabanı: x 0,59, y 0,39, z 0,04 (tezgah.py).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "a_cekmece", X = 0.59f, Y = 0.39f, Z = 0.04f, Donus = 60f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_riza", Tur = EsyaTuru.Tabak, Yer = "a_dolap", X = 0f, Y = 0.475f, Z = 0.02f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_cemal", Tur = EsyaTuru.Tabak, Yer = "a_odaA_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_hasan", Tur = EsyaTuru.Tabak, Yer = "b_banyo_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_kapici", Tur = EsyaTuru.Tabak, Yer = "b_odaA_yer" });
        // Büfenin sol dolabının tabanı 0,10 m; sol kapı Unity'de +x tarafında (Blender -x).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "bufe", X = 0.35f, Y = BufeTaban, Donus = 15f });

        // KİLİTLER
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "B:Oda_C", Gerekli = "a_takvim",
                                          Sifre = "1994", IpucuMetni = "1994", IpucuY = 0.24f, IpucuZ = 0.011f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "a_mutfak", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "a_mutfak_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "a_odaA", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Oda_A",
                                          Gerekenler = new List<string> { "odaA_anahtari_bas", "odaA_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "B:Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_odaA", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "B:Oda_A", Gerekli = "a_kol" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "bufe", Tur = KilitTuru.Yerlestirme, OdaKapisi = false, Hedef = "bufe",
                                          Gerekenler = new List<string> { "tabak_riza", "tabak_hasan", "tabak_cemal", "tabak_kapici" },
                                          Yuvalar = new List<string> { "yer_1", "yer_2", "yer_3", "yer_4" } });
        v.zincir = z;

        // İSİMLER: tabakların üstünde ve sofradaki kartlarda (metin tablosundan).
        v.esyaYazilari = new List<EsyaYazisi>
        {
            new EsyaYazisi { Kimlik = "tabak_riza", Metin = "bina.isim_riza", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_hasan", Metin = "bina.isim_hasan", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_cemal", Metin = "bina.isim_cemal", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_kapici", Metin = "bina.isim_kapici", Boy = 0.0019f },
        };
        // Kartlar masanın kenarında, her tabak yerinin önünde; ön yüzü o yerin sandalyesine bakar.
        // Önde oturanlar (z = +0,25) için kart z = +0,40, arkadakiler için -0,40 ve 180° dönük.
        KatSusu Kart(float x, float zz, float donus, string metin)
        { return new KatSusu { Sablon = isimKarti, Yol = "Esyalar/Sofra", X = x, Y = 0.757f, Z = zz, Donus = donus, Metin = metin }; }
        v.suslar = new List<KatSusu>
        {
            Kart(-0.35f, 0.40f, 0f, "bina.isim_riza"),
            Kart(0.35f, 0.40f, 0f, "bina.isim_hasan"),
            Kart(-0.35f, -0.40f, 180f, "bina.isim_cemal"),
            Kart(0.35f, -0.40f, 180f, "bina.isim_kapici"),
        };
        v.notlar = new List<KatNotu>
        {
            // 3'ün antresinde davet; 3'ün kilerinde kolun yanında Cemal'in notu; 4'ün koridorunda
            // kilerin kapısının yanında "takvime bak" notu (2. kattaki notun yeri, ikinci dairede).
            new KatNotu { Metin = "not.kat6_davet", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            new KatNotu { Metin = "not.kat6_kol", Yol = "Esyalar/Duvar_Kolu", X = 0.35f, Y = 0.25f, Z = 0.002f },
            new KatNotu { Metin = "not.kat6_takvim", Yol = "B:", X = 4.95f, Y = 1.40f, Z = 2.952f, OnU = 0f, OnV = 1f },
        };
        // Saklanma anı (#58): büfedeki anahtar alınınca kapı tıklatılır, Gölge holde devriye gezer.
        // Davet notu uyarıyor: misafir gelince kapını kapalı tut.
        v.saklanmaVar = true;
        v.kacisSablonu = golge;
        v.saklanmaSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Tiklatma.wav");
        v.parcalar = new List<KatParcasi>();
        v.sesler = new List<KatSesi>();
        v.yananOdalar = new List<string>();

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) { odalar.Add(o.Ad); odalar.Add(KilitZinciri.IkinciOnek + o.Ad); }
        KilitZinciri.Sonuc s6 = z.Cozumle(odalar);
        if (!s6.Cozulebilir) Debug.LogError("[ZINCIR] 6. kat zinciri çözülemiyor: " + string.Join("; ", s6.Hatalar));
        else Debug.Log("[ZINCIR] 6. kat zinciri çözülebilir: " + string.Join(" -> ", s6.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 7. KAT: GÖZLEM + CEMAL'İN DAİRESİ (#67, #58, Docs/hikaye.md §7) ----------
    // SAHANLIK: dört kapının hepsinde "13", dördü de kapalı. Cemal'in notu (sahanlıktaki ilan
    // panosunun yanında): "benim kapımın önünde paspas yok, terliklerimi içeri aldım, gözetleme
    // deliğimde ışık yanar". Gerçek kapı 1 numaralı dairenin (Kapi_1); kartı bu farkları verir.
    // Yanlış kapıyı çalmak bir olay tetikler (ses + kafa lambası titrer).
    // DAİRE (elektriksiz): yedi kilit, iç içe:
    //   banyo  : menteşe (tornavida; kapıcıda değilse antrede yerde)
    //   salon  : şifre = Cemal'in doğum günü (1107); nüfus cüzdanı banyoda, lavabonun üstünde
    //   kiler  : anahtarı salondaki büfenin sağ dolabında
    //   mutfak : kilerdeki kol
    //   odaA   : kırık anahtar (başı büfede, ucu mutfak çekmecesinde)
    //   odaB   : şifre 0913, yatak odasının duvarında fosforlu boyayla (lamba kapalıyken görünür)
    //   sandik : Cemal'in sandığı (Oda B'deki sandık) levyeyle kanırtılır; içinde katın anahtarı
    // Cemal'in son notu Oda B'nin duvarında: "Asansör beni bekliyor."
    static KatZinciriVarligi Kat7Kur(GameObject golge)
    {
        string yol = $"{ZincirKlasoru}/Kat_7.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "7. kat, daire 1, Cemal'in dairesi (gözlem). Sahanlıkta dört '13' kapısı, gerçeği Cemal'in notundaki farklarla " +
                     "bulunur; yanlış kapı bir olay tetikler. Elektriksiz daire, yedi kilit: banyo (menteşe), salon (doğum günü 1107, " +
                     "nüfus cüzdanı banyoda), kiler (anahtar büfede), mutfak (kiler kolu), Oda A (kırık anahtar), Oda B (fosforlu 0913), " +
                     "sandık (levye) -> katın anahtarı.";
        v.kapiNumarasi = "13";
        v.girisKapali = true;
        v.sahanlikKarti = GercekKapiKarti();
        v.tiklatmaSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Tiklatma.wav");
        v.yanlisKapiSesleri = new List<AudioClip>
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Carpma.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Bebek_Ters.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Zil_DingDong.wav"),
        };
        v.elektrikYok = true;
        v.ikinciDaire = 0;
        // KAÇIŞ ANI (#58): sandıktaki anahtar alınınca asansör "ding" der; 3 sn sonra siluet anahtarın
        // alındığı yerden çıkar ve oyuncunun yolunu 2,0 m/sn izler (oyuncu 1,8). Oda B'den kat
        // kapısına duraksamadan ~10 sn. Gölge 3 sn geç çıktığı için arada 5,4 m var, bu açığı saniyede
        // 0,2 m kapatır: ~27 sn. Yolu bilen rahat yetişir, 15 sn'den fazla oyalanan yakalanır.
        // (İlk hâli 2,2 m/sn idi; yakalanınca kat baştan başladığı için Şahan yavaşlattı, 2026-09-27.)
        v.kacisVar = true;
        v.kacisSablonu = golge;
        v.kacisGecikme = 3f;
        v.kacisHizi = 2.0f;
        v.kacisBaslangicSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Asansor_Ding.wav");
        // Siluetin habercisi üst kattan adımlar (#74; eskiden çevresinde asansör uğultusu).
        v.kacisHaberciSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Olay/Olay_UstKatAdimlari.wav");

        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "antre_yer", PlandaNokta = true, Oda = "Antre", U = 2.0f, Y = 0.004f, V = -0.4f, Donus = 70f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_kimlik", Yol = "Esyalar/Lavabo", Oda = "Banyo" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_bufe", Yol = "Esyalar/Bufe", Oda = "Salon", KilitYolu = "Esyalar/Bufe/Kapi_2" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "mutfak_cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        // Yatak odasının dip duvarı (u = 10,5); yazı 5 cm önünde, göz hizasının üstünde, odaya (-u) bakar.
        z.Yerler.Add(new ZincirYeri { Kimlik = "odaA_duvar", PlandaNokta = true, Oda = "Oda_A", U = 10.45f, Y = 1.60f, V = 6.4f, OnU = -1f, OnV = 0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "odaA_yer", PlandaNokta = true, Oda = "Oda_A", U = 8.6f, Y = 0.004f, V = 5.6f, Donus = -30f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sandik", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B", KilitYolu = "Esyalar/Oyuncak_Kutusu/Kapak" });

        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "antre_yer" });
        // Büfenin sağ dolabı Unity'de -x tarafında (Blender +x); taban 0,10 m.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kiler_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "salon_bufe", X = -0.45f, Y = BufeTaban, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "salon_bufe", X = -0.22f, Y = BufeTaban, Donus = -40f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "mutfak_cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 50f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "levye", Tur = EsyaTuru.Levye, Yer = "odaA_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sandik", Y = SandikTaban, Donus = 20f });

        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Salon", Gerekli = "b_kimlik",
                                          Sifre = "1107", IpucuYazma = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "kiler_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Oda_A",
                                          Gerekenler = new List<string> { "odaA_anahtari_bas", "odaA_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaA_duvar",
                                          Sifre = "0913", IpucuMetni = "0913", IpucuOlcek = 4f, IpucuKaranlikta = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sandik", Tur = KilitTuru.Ekipman, OdaKapisi = false, Hedef = "sandik",
                                          Gerekenler = new List<string> { "levye" }, IpucuMetni = "ipucu.kanirt" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Cemal'in notu sahanlıkta, ilan panosunun yanında (gözlem ipucu, kapılardan önce okunur).
            new KatNotu { Metin = "not.kat7_cemal", Yol = "Sahanlik:Ilan", X = 0.45f, Y = 0f, Z = 0.004f },
            // Nüfus cüzdanı banyoda, lavabonun üstünde duvarda (lavabonun pivotu duvarın dibinde).
            new KatNotu { Metin = "not.kat7_kimlik", Yol = "Esyalar/Lavabo", X = 0f, Y = 1.25f, Z = 0.004f },
            // Cemal'in son notu Oda B'nin arka duvarında (v = 8,1), odaya (-v) bakar.
            new KatNotu { Metin = "not.kat7_son", X = 5.45f, Y = 1.40f, Z = 8.098f, OnU = 0f, OnV = -1f },
            // İPUÇLARI (Şahan, 2026-09-27: Cemal'in odasının şifresini bulamadı; yatak odasına giden
            // yolun kilerdeki koldan geçtiğini anlatan bir şey yoktu):
            //   kilerde kolun yanında: kol mutfağın sürgüsünü açar;
            //   koridorda Oda B'nin kapısının solunda (kapı u = 5,95-6,75, koridor duvarı v = 4,65):
            //   şifre yatak odasının duvarında, karanlıkta.
            new KatNotu { Metin = "not.kat7_kol", Yol = "Esyalar/Duvar_Kolu", X = 0.35f, Y = 0.25f, Z = 0.002f },
            new KatNotu { Metin = "not.kat7_sifre", X = 5.45f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();
        v.yananOdalar = new List<string>();
        v.esyaYazilari = new List<EsyaYazisi>();

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s7 = z.Cozumle(odalar);
        if (!s7.Cozulebilir) Debug.LogError("[ZINCIR] 7. kat zinciri çözülemiyor: " + string.Join("; ", s7.Hatalar));
        else Debug.Log("[ZINCIR] 7. kat zinciri çözülebilir: " + string.Join(" -> ", s7.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // 8. KAT (#68): daire 4, Yönetim dairesi. İmza mekaniği ASANSÖR. Son kat, en uzun zincir (sekiz
    // kilit, Şahan'ın kuralı: kat çıktıkça bulmak zorlaşır). Oda sırası ve neyin neyi açtığı:
    //   banyo (menteşe, tornavida)         -> yerde kilerin anahtarı
    //   kiler (anahtar)                    -> alet çantasında mutfağın anahtarı; duvarda kol
    //   Oda B (kilerdeki kol)              -> sandıkta salon anahtarının başı
    //   mutfak (anahtar)                   -> buzdolabında salon anahtarının ucu
    //   salon (kırık anahtar, iki parça)   -> duvarda iskân ruhsatı: "zemin + 8 kat, yapım yılı 1974"
    //   Oda A, arşiv (şifre 1974)          -> komodinde dosya dolabının anahtarı
    //   dosya dolabı, üst çekmece (anahtar)-> asansörün sigortası ve Cemal'in notu
    //   asansör (sigorta panoya, çağır)    -> kabinde ÇATI ANAHTARI (katın anahtarı; çatı kapısını açar)
    // Boş kaplar (tezgah çekmeceleri, büfe, dolabın alt çekmecesi) aramayı uzatır; dolabın öbür
    // iki çekmecesinde kapıcıların dosyaları ve dokuzuncu katın planı durur (hikâye, anahtar yok).
    static KatZinciriVarligi Kat8Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_8.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "8. kat, daire 4, Yönetim dairesi (asansör). Sekiz kilit: banyo (menteşe), kiler (anahtar banyoda), " +
                     "Oda B (kilerin kolu), mutfak (anahtar alet çantasında), salon (kırık anahtar: sandık + buzdolabı), " +
                     "Oda A (şifre 1974, salondaki iskân ruhsatı), dosya dolabı (anahtar komodinde), asansör (sigorta " +
                     "dolapta; panoya takılır, çağrılır, gösterge 9 der) -> kabinde çatı anahtarı.";
        v.kapiNumarasi = "";
        v.girisKapali = false;
        v.sahanlikKarti = null;
        v.elektrikYok = false;
        v.ikinciDaire = 0;
        v.kacisVar = false;

        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "antre_yer", PlandaNokta = true, Oda = "Antre", U = 2.0f, Y = 0.004f, V = -0.4f, Donus = 70f });
        // Banyonun kapıya göre sağ köşesi. Kapı (u = 3,25-3,95) menteşeden sökülünce banyonun içine
        // devrilir ve v = 0,9'a kadar zemini örter; ilk yer (3,3; 2,1) onun altında kaldı, Şahan
        // anahtarı göremedi. Kural artık bunu yakalıyor (KilitZinciri.DevrilmeAlaninda).
        z.Yerler.Add(new ZincirYeri { Kimlik = "banyo_yer", PlandaNokta = true, Oda = "Banyo", U = 4.2f, Y = 0.004f, V = 1.0f, Donus = -25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "alet_cantasi", Yol = "Esyalar/Alet_Cantasi", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        // İskân ruhsatı salonun koridor duvarında, salon kapısıyla dosya dolabı arasında (ipucu yeri).
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_ruhsat", PlandaNokta = true, Oda = "Salon", U = 2.9f, Y = 1.6f, V = 4.8f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        // Dosya dolabının üst çekmecesi: hem kilidin hedefi hem sigortanın yeri (çekmeceyle birlikte kayar).
        z.Yerler.Add(new ZincirYeri { Kimlik = "dosya_ust", Yol = "Esyalar/Dosya_Dolabi/Cekmece_1", Oda = "Salon" });
        // Asansörün kabini sahanlıkta (dairenin dışında): kapılar açılana kadar gizli.
        z.Yerler.Add(new ZincirYeri { Kimlik = "kabin", Yol = "Sahanlik:Asansor/Kabin", Oda = KilitZinciri.SahanlikOdasi });

        // Tornavida 1. kattan beri kapıcının çantasında; kayıtsız bir oyunda antrede bulunur.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "antre_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kiler_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "banyo_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "mutfak_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "alet_cantasi", Y = 0.03f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "oyuncak", Y = SandikTaban, Donus = -30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "buzdolabi", Y = 0.47f, Donus = 10f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "dosya_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "komodin", Y = 0.375f, Donus = -15f });
        // Çekmecenin tabanı 1,0 m'de (dosya_dolabi.py: üst yuvanın tepsisi); sigorta sağda, not solda.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "asansor_sigortasi", Tur = EsyaTuru.Sigorta, Yer = "dosya_ust", X = 0.09f, Y = 1.0f, Z = 0.02f, Donus = 80f });
        // Çatı anahtarı kabinin zemininde (zeminin üstü 8 mm), kapıdan 55 cm içeride.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "cati_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "kabin",
                                         X = 0.12f, Y = 0.012f, Z = -MekanOlculeri.SahanlikDerin - MekanOlculeri.DuvarKalin - 0.55f, Donus = 35f });

        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "kiler_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "mutfak_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Salon",
                                          Gerekenler = new List<string> { "salon_anahtari_bas", "salon_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "salon_ruhsat",
                                          Sifre = "1974", IpucuYazma = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "dosya", Tur = KilitTuru.Anahtar, OdaKapisi = false, Hedef = "dosya_ust", Gerekli = "dosya_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "asansor", Tur = KilitTuru.Asansor, OdaKapisi = false, Hedef = "kabin", Gerekli = "asansor_sigortasi" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Yönetim'in notu antrede, sigorta kutusunun yanında (kapıdan girince ilk görülen duvar).
            new KatNotu { Metin = "not.kat8_yonetim", Yol = "Esyalar/Sigorta_Kutusu", X = 0.42f, Y = 0f, Z = 0.004f },
            // Kilerde kolun yanında: kol Oda B'yi açar, mutfağın anahtarı çantada.
            new KatNotu { Metin = "not.kat8_kiler", Yol = "Esyalar/Duvar_Kolu", X = 0.35f, Y = 0.25f, Z = 0.002f },
            // Koridorda salon kapısının yanında (kapı u = 0,85-1,75, koridor duvarı v = 4,65): kırık anahtar.
            new KatNotu { Metin = "not.kat8_salon", X = 2.2f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
            // Koridorda arşivin (Oda A) kapısının solunda (kapı u = 8,7-9,5): şifre binanın yaşı değil yılı.
            new KatNotu { Metin = "not.kat8_arsiv", X = 8.2f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
            // İskân ruhsatı salonun koridor duvarında, salona (+v) bakar.
            new KatNotu { Metin = "not.kat8_ruhsat", X = 2.9f, Y = 1.55f, Z = 4.752f, OnU = 0f, OnV = 1f },
            // Dosya dolabının çekmecelerinde yatık kâğıtlar: üstte Cemal'in notu (sigortanın yanında),
            // ikincide kapıcıların dosyaları, üçüncüde dokuzuncu katın planı; dördüncü boş.
            new KatNotu { Metin = "not.kat8_sigorta", Yol = "Esyalar/Dosya_Dolabi/Cekmece_1", X = -0.08f, Y = 1.0f, Z = 0f, Yatik = true },
            new KatNotu { Metin = "not.kat8_dosyalar", Yol = "Esyalar/Dosya_Dolabi/Cekmece_2", X = 0f, Y = 0.69f, Z = 0f, Yatik = true },
            new KatNotu { Metin = "not.kat8_plan", Yol = "Esyalar/Dosya_Dolabi/Cekmece_3", X = 0f, Y = 0.38f, Z = 0f, Yatik = true },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();
        // Yönetim'in ofisi (salon) aydınlık: ruhsat okunabilsin. Öbür odalar kafa lambasıyla.
        v.yananOdalar = new List<string> { "Salon" };
        v.esyaYazilari = new List<EsyaYazisi>();

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s8 = z.Cozumle(odalar);
        if (!s8.Cozulebilir) Debug.LogError("[ZINCIR] 8. kat zinciri çözülemiyor: " + string.Join("; ", s8.Hatalar));
        else Debug.Log("[ZINCIR] 8. kat zinciri çözülebilir: " + string.Join(" -> ", s8.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // Kovalayan siluetin şablonu (#58): yalnızca model; çarpışma yok (oyuncuyu itmesin, nişanı engellemesin).
    static GameObject GolgeSablonuKur()
    {
        GameObject gecici = new GameObject("Kacis_Golgesi");
        if (Model("Model", gecici.transform, "Golge", null, Vector3.zero, 0f, carpisma: false) == null)
        { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Golge.fbx yok."); return null; }
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Kacis_Golgesi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // 7. katın sahanlık kartı (#67): gerçek kapının (Kapi_1) farkları. Rastgele seçilen kartların
    // klasöründe (Resources/Anomaliler) DEĞİL: yalnızca kat dosyasından uygulanır.
    static AnomaliKarti GercekKapiKarti()
    {
        const string Yol = "Assets/Zincirler/Kart_Kat7_GercekKapi.asset";
        AnomaliKarti k = AssetDatabase.LoadAssetAtPath<AnomaliKarti>(Yol);
        if (k == null)
        {
            k = ScriptableObject.CreateInstance<AnomaliKarti>();
            AssetDatabase.CreateAsset(k, Yol);
        }
        var s = new SerializedObject(k);
        s.FindProperty("kimlik").stringValue = "kat7_gercek_kapi";
        s.FindProperty("aciklama").stringValue = "7. kat: Cemal'in kapısı. Paspası ve terlikleri yok, gözetleme deliğinden ışık sızıyor.";
        SerializedProperty etkiler = s.FindProperty("etkiler");
        string[] yollar = { "Kapi_1/Paspas", "Kapi_1/Terlikler", "Kapi_1/Mentese/Kapi_Gozu_Isik" };
        bool[] gorunur = { false, false, true };
        etkiler.arraySize = yollar.Length;
        for (int i = 0; i < yollar.Length; i++)
        {
            SerializedProperty e = etkiler.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("hedefYol").stringValue = yollar[i];
            e.FindPropertyRelative("tur").enumValueIndex = (int)EtkiTuru.Gorunurluk;
            e.FindPropertyRelative("gorunur").boolValue = gorunur[i];
            e.FindPropertyRelative("gecikme").floatValue = 0f;
        }
        s.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(k);
        return k;
    }

    // Çizim dokusunun içe aktarma ayarları: saydamlık korunsun, kenardan taşmasın (Clamp),
    // arayüzde bulanık durmasın diye sıkıştırma yüksek kalite.
    static void CizimDokusunuAyarla(string yol)
    {
        TextureImporter ti = AssetImporter.GetAtPath(yol) as TextureImporter;
        if (ti == null) { Debug.LogError("[ZINCIR] Çizim dokusu yok: " + yol + " (Art/Cizim/scripts/cizim_uret.py)"); return; }
        bool degisti = !ti.alphaIsTransparency || ti.wrapMode != TextureWrapMode.Clamp || ti.textureCompression != TextureImporterCompression.CompressedHQ;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        if (degisti) ti.SaveAndReimport();
    }

    // Dokudaki saydam olmayan piksellerin sınırı, UV olarak (0-1, sol alttan). Dosyayı diskten
    // okur (içe aktarılmış doku okunamaz olarak işaretli olabilir).
    static Rect ParcaBolgesi(string yol)
    {
        var t = new Texture2D(2, 2);
        t.LoadImage(System.IO.File.ReadAllBytes(yol));      // PNG'yi çözer; boyu kendisi ayarlar
        Color32[] p = t.GetPixels32();                       // satırlar ALTTAN başlar (UV ile aynı)
        int w = t.width, h = t.height, x0 = w, y0 = h, x1 = -1, y1 = -1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (p[y * w + x].a > 0) { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y); }
        Object.DestroyImmediate(t);
        if (x1 < 0) return new Rect(0, 0, 1, 1);
        return new Rect((float)x0 / w, (float)y0 / h, (float)(x1 - x0 + 1) / w, (float)(y1 - y0 + 1) / h);
    }

    // Yere serilen çizim parçası: nişan için bir çarpışma kutusu, Esya (Parca) ve yüzü yukarı
    // bakan ince bir dörtgen ("Kagit"). Dörtgenin boyu ve gösterdiği bölge her parçada ayrı;
    // uygulayıcı yerleştirirken ayarlar (ZincirUygulayici.ParcayiGiydir).
    static GameObject CizimParcasiSablonuKur()
    {
        GameObject gecici = new GameObject("Esya_CizimParcasi");
        // CreatePrimitive(Quad): Unity'nin hazır 1 x 1 m dörtgeni. Çarpışma bileşenini atıyoruz:
        // nişanı kökteki kutu karşılıyor.
        GameObject kagit = GameObject.CreatePrimitive(PrimitiveType.Quad);
        kagit.name = "Kagit";
        Object.DestroyImmediate(kagit.GetComponent<Collider>());
        kagit.transform.SetParent(gecici.transform, false);
        kagit.transform.localPosition = new Vector3(0f, 0.002f, 0f);
        // Dörtgenin yüzü -Z'ye bakar; x ekseninde 90° çevirince yukarı bakar.
        kagit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        kagit.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
        kagit.GetComponent<MeshRenderer>().sharedMaterial = CizimMalzemesi();
        kagit.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.03f, 0f);
        c.size = new Vector3(0.18f, 0.06f, 0.18f);
        Esya esya = gecici.AddComponent<Esya>();
        esya.Ayarla(EsyaTuru.Parca, "", 0);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Esya_CizimParcasi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Parçaların ortak malzemesi: URP Lit, "alfa kırpma" açık (yırtık kenarın dışı görünmez).
    // Doku ve bölge nesne başına MaterialPropertyBlock ile verilir.
    static Material CizimMalzemesi()
    {
        const string Yol = "Assets/Materials/Cizim_Parca.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, Yol);
        }
        m.SetFloat("_AlphaClip", 1f);
        m.SetFloat("_Cutoff", 0.5f);
        m.EnableKeyword("_ALPHATEST_ON");
        m.SetFloat("_Smoothness", 0.1f);
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Cizim/Cizim_Kat3_P1.png"));
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material GumusMalzeme()
    {
        const string Yol = "Assets/Materials/Anahtar_Gumus.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m != null) return m;
        m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", new Color(0.72f, 0.73f, 0.75f));
        m.SetFloat("_Metallic", 1f);
        m.SetFloat("_Smoothness", 0.55f);
        AssetDatabase.CreateAsset(m, Yol);
        return m;
    }

    // Test zinciri: her kurulumda koddan yeniden yazılır, böylece elle bozulamaz.
    static KatZinciriVarligi TestZinciriKur()
    {
        string yol = $"{ZincirKlasoru}/Zincir_Test.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "Geliştirme zinciri (#54, #61): altı kilit türü tek dairede. Kapının önündeki ayakkabılıkta tornavida; " +
                     "tornavidayla kavanoz kırılır, içinde salon anahtarı; takvimde kilerin şifresi; kilerdeki kol Oda A'yı açar; " +
                     "komodinde sigorta, buzdolabını açar; buzdolabında Oda B'nin anahtarı; oyuncak kutusunda katın anahtarı. " +
                     "Banyonun menteşeleri aynı tornavidayla sökülür (yan oda).";
        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kavanoz", Yol = "Esyalar/Tezgah/Kavanoz", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kavanoz/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "takvim", Yol = "Esyalar/Takvim", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kol", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "kavanoz", Y = 0.012f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "komodin", Y = 0.375f, Donus = 10f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "ayakkabilik", Y = 1.025f, Donus = 80f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "buzdolabi", Y = 0.47f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "oyuncak", Y = SandikTaban, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kavanoz", Tur = KilitTuru.Kirma, OdaKapisi = false, Hedef = "kavanoz", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Salon", Gerekli = "salon_anahtari" });
        // Şifrenin ipucu takvimin yaprağına yazılır (yaprağın üst yarısı, ön yüzün 1 mm önü).
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "takvim",
                                          Sifre = "0612", IpucuMetni = "0612", IpucuY = 0.24f, IpucuZ = 0.011f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "kol" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "buzdolabi", Tur = KilitTuru.Sigorta, OdaKapisi = false, Hedef = "buzdolabi", Gerekli = "sigorta" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaB_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        v.zincir = z;

        // Kural: kaydetmeden önce zincirin çözülebilir olduğunu doğrula.
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s = z.Cozumle(odalar);
        if (!s.Cozulebilir) Debug.LogError("[ZINCIR] Test zinciri çözülemiyor: " + string.Join("; ", s.Hatalar));
        else Debug.Log("[ZINCIR] Test zinciri çözülebilir: " + string.Join(" -> ", s.AcilanKilitler));

        EditorUtility.SetDirty(v);
        return v;
    }
}
