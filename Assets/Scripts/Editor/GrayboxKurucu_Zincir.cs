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
        foreach (string ad in new[] { "Orgu_Battaniye", "Cay_Takimi", "Gazete_Kilif", "Kuru_Sardunya", "Koltuk_Danteli" })
            ParcaSablonuKur(ad, ad);
        // Dikiş makinesinin çekmecesi açılır (#83): 2. katta Oda A'nın anahtarı onda.
        AcilirSablonKur("Dikis_Makinesi");
        // 2. kat mutfak dekoru (#83).
        foreach (string ad in new[] { "Buzdolabi_70", "Ocak_Emaye", "Tup_Turuncu", "Mutfak_Masasi", "Mutfak_Sandalyesi",
                                      "Teneke_Rafi", "Mutfak_Perdesi", "Bulasiklik" })
            ParcaSablonuKur(ad, ad);
        // 2. kat Oda A ve kiler dekoru (#83): Blender odaA_kiler_nuri.py.
        foreach (string ad in new[] { "Karyola_Nuri", "Tuvalet_Masasi", "Elbise_Sandalyesi", "Kilim_Nuri", "Perde_Cicekli", "Hat_Levha",
                                      "Kiler_Rafi", "Radyo_Tezgahi", "Kiler_Cuvallari", "Supurge_Faras", "Damacana", "Sogan_Sepeti" })
            ParcaSablonuKur(ad, ad);
        foreach (string ad in new[] { "Komodin_Nuri", "Teneke_Nuri" })
            AcilirSablonKur(ad);
        KatZinciriVarligi kat2 = Kat2Kur();
        GameObject cizimParcasi = CizimParcasiSablonuKur();
        GameObject levye = SablonKur("Esya_Levye", "Levye", EsyaTuru.Levye, null);
        GameObject kaydirici = SablonKur("Esya_Kaydirici", "Kaydirici", EsyaTuru.Kaydirici, null);
        GameObject anahtarParcasi = AnahtarParcasiSablonuKur();
        GameObject tabak = SablonKur("Esya_Tabak", "Tabak", EsyaTuru.Tabak, null);
        GameObject isimKarti = IsimKartiSablonuKur();
        Kat3SablonlariniKur();   // #83: 3. katın Blender modelleri (GrayboxKurucu_Kat3.cs)
        KatZinciriVarligi kat3 = Kat3Kur();
        Kat4SablonlariniKur();   // #83: 4. katın Blender modelleri (GrayboxKurucu_Kat4.cs)
        KatZinciriVarligi kat4 = Kat4Kur();
        Kat5SablonlariniKur();   // #83: 5. katın Blender modelleri (GrayboxKurucu_Kat5.cs)
        KatZinciriVarligi kat5 = Kat5Kur();
        GameObject golge = GolgeSablonuKur();
        Kat6SablonlariniKur();   // #83: 6. katın Blender modelleri (GrayboxKurucu_Kat6.cs)
        KatZinciriVarligi kat6 = Kat6Kur(isimKarti, golge);
        Kat7SablonlariniKur();   // #83: 7. katın Blender modelleri (GrayboxKurucu_Kat7.cs)
        KatZinciriVarligi kat7 = Kat7Kur(golge);
        Kat8SablonlariniKur();   // #83: 8. katın Blender modelleri (GrayboxKurucu_Kat8.cs)
        KatZinciriVarligi kat8 = Kat8Kur();

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
        // Kafa lambasında parlasın (#83): kâğıt (not, çizim) metal kadar ışık yakalamaz, daha sönük.
        ParlamaEkle(gecici, b.center + model.transform.localPosition, tur == EsyaTuru.Parca ? 0.55f : 1f);

        string yol = $"{SablonKlasoru}/{ad}.prefab";
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, yol);
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // ---------- KAFA LAMBASINDA PARILTI (#83) ----------
    // Eşya şablonuna "Parilti" adlı küçük bir dörtgen ve FenerParlamasi bileşeni ekler. Dörtgen
    // kapalı başlar; lamba ışığı eşyaya düşünce FenerParlamasi onu gösterir ve kameraya çevirir.
    // merkez: parıltının eşyaya göre yeri. carpan: parlaklık (metal 1, kâğıt daha az).
    static void ParlamaEkle(GameObject kok, Vector3 merkez, float carpan)
    {
        GameObject dortgen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        dortgen.name = "Parilti";
        // Çarpışması silinir: yoksa oyuncunun nişan ışını parıltıya çarpardı (ipucu panelindeki tuzak).
        Object.DestroyImmediate(dortgen.GetComponent<Collider>());
        dortgen.transform.SetParent(kok.transform, false);
        dortgen.transform.localPosition = merkez;
        MeshRenderer mr = dortgen.GetComponent<MeshRenderer>();
        mr.sharedMaterial = ParlamaMalzemesi();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // ışığın gölgesi olmaz
        mr.receiveShadows = false;
        mr.enabled = false;
        FenerParlamasi fp = kok.AddComponent<FenerParlamasi>();
        // Özel (private) alan: kurucu SerializedObject ile yazar, Inspector'da görünen değerin aynısı.
        SerializedObject so = new SerializedObject(fp);
        so.FindProperty("gucCarpani").floatValue = carpan;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Parıltının malzemesi: URP Unlit, toplamalı (additive) saydam. Toplamalı karışım altındaki
    // rengin üstüne ışık ekler, hiçbir şeyi karartmaz: karanlıkta küçük bir ışık gibi okunur.
    // Doku Art/Isik/scripts/parilti_uret.py'den gelir. Dosya varsa ayarları yeniden yazılır.
    static Material ParlamaMalzemesi()
    {
        const string DokuYolu = "Assets/Textures/Parilti.png";
        const string Yol = "Assets/Materials/Esya_Parilti.mat";
        TextureImporter ti = AssetImporter.GetAtPath(DokuYolu) as TextureImporter;
        if (ti != null && (ti.wrapMode != TextureWrapMode.Clamp || !ti.alphaIsTransparency))
        {
            ti.wrapMode = TextureWrapMode.Clamp;      // kenarda karşı kenarın pikselleri görünmesin
            ti.alphaIsTransparency = true;
            ti.SaveAndReimport();
        }
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (m == null)
        {
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, Yol);
        }
        m.shader = sh;
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DokuYolu));
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Surface", 1f);                   // saydam
        m.SetFloat("_Blend", 2f);                     // 2 = toplamalı (Additive)
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_ZWrite", 0);                       // arkasındakini gizlemesin
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(m);
        return m;
    }

    // Blender'da yarı saydam verilen cam malzemeleri (#83): Unity FBX'teki saydamlığı okumaz; bu adlardaki
    // malzemeler saydam cama çevrilir, yoksa büfenin camı bardakları, arşiv dolabının camı klasörleri gizlerdi.
    // Açık liste, kural değil: Türkçede "cam" (glass) ile "çam" (pine) ç'siz aynı yazılır; "Askilik_Cam",
    // "K7_Sandalye_Cam" çam ağacıdır. Aynalar ("Ayna6_Cam") ve eski eşyaların camları da opak kalır.
    static readonly HashSet<string> CamMalzemeleri = new HashSet<string>
    {
        "KRaf_Cam", "Damacana_Cam", "Termos4_Cam", "Luks4_Cam", "K5_VitrinCam", "Raf6_Cam",
        "K7_Bufe_Cam", "K7_Bufe_Bardak_Cam", "K7_Kolonya_Cam", "K7_Ampul_Cam", "K7_Bulasik_Cam", "K7_Sehpa_CayCam",
        "K7_KulTablasi_Cam", "K8_Cam",
    };

    static void CamlariSaydamYap(GameObject kok)
    {
        foreach (MeshRenderer r in kok.GetComponentsInChildren<MeshRenderer>(true))
        {
            Material[] mats = r.sharedMaterials;
            bool degisti = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (CamMalzemeleri.Contains(mats[i].name)) { mats[i] = KavanozCami(); degisti = true; }
            }
            if (degisti) r.sharedMaterials = mats;
        }
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
        CamlariSaydamYap(gecici);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // AÇILIR SÜS ŞABLONU (#83): görev kabı olabilecek, kata özel bir mobilya (komodin, sandık, dolap).
    // Blender betiği (Art/Blender/scripts/acilir_ortak.py kuralı) FBX'e şu adlı nesneleri koyar:
    //   Govde       : gövde (içi oyulmuş); MeshCollider alır, nişan ışını içine, eşyaya kadar girebilsin.
    //   Cekmece_N   : öne (+Z) kayan çekmece; pivotu çekmecenin iç tabanının ortası.
    //   Kapi_N      : dik menteşeli kapı; pivotu menteşe çizgisi. Kapının gövdesi pivotun hangi yanındaysa
    //                 dönüş yönü ondan bulunur (MobilyaKurucu'daki kural): öne, odaya doğru açılır.
    //   Kapak       : arka üst kenarından menteşeli kapak (sandık); ön kenarı yukarı kalkar.
    //   Surgu_N     : yana kayan parça (yerel +X yönünde, genişliğinin %90'ı kadar).
    // Parçalar kökün çocuğudur; zincir onlara "<süsün adı>/Cekmece_1" yoluyla başvurur. Gövdeye
    // Yonlendirici eklenir: gövdeye bakan oyuncuya parçası (en yakını) sunulur. buzdolabiIsigi: kapının
    // iç ışığı (BuzdolabiIsigi) da kurulsun; ışığın yeri kökün eksenlerinde.
    static GameObject AcilirSablonKur(string ad, Vector3? buzdolabiIsigi = null)
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModelKlasoru + ad + ".fbx");
        if (fbx == null) { Debug.LogWarning($"[ZINCIR] {ad}.fbx yok."); return null; }
        GameObject gecici = new GameObject(ad);
        GameObject govde = Model("Govde", gecici.transform, ad, "Govde", Vector3.zero, 0f, carpisma: false);
        if (govde == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {ad}.fbx içinde Govde yok."); return null; }
        govde.AddComponent<MeshCollider>().sharedMesh = govde.GetComponent<MeshFilter>().sharedMesh;
        var parcalar = new List<MonoBehaviour>();
        AcilirParca ilkKapi = null;
        // FBX'in kökündeki bütün nesneler sırayla: Govde dışındakiler açılır parça ya da düz süs.
        foreach (Transform t in fbx.transform)
        {
            if (t.name == "Govde") continue;
            bool cekmece = t.name.StartsWith("Cekmece"), kapi = t.name.StartsWith("Kapi"),
                 kapak = t.name.StartsWith("Kapak"), surgu = t.name.StartsWith("Surgu");
            GameObject g = Model(t.name, gecici.transform, ad, t.name, Vector3.zero, 0f, carpisma: true);
            if (g == null || !(cekmece || kapi || kapak || surgu)) continue;
            Bounds b = g.GetComponent<MeshFilter>().sharedMesh.bounds;   // parçanın kendi (yerel) sınırı
            AcilirParca a = g.AddComponent<AcilirParca>();
            var seri = new SerializedObject(a);
            if (cekmece || surgu)
            {
                seri.FindProperty("hareket").enumValueIndex = (int)AcilirParca.Hareket.Kaydir;
                seri.FindProperty("eksen").vector3Value = cekmece ? Vector3.forward : Vector3.right;
                // Çekmece derinliğinin %85'i kadar çıkar: %65'te dikiş makinesinin çekmecesindeki anahtar tablanın altında
                // kalıyor, ayaktaki oyuncu onu göremiyordu (yürünebilirlik denetimi, 2026-09-30).
                seri.FindProperty("miktar").floatValue = cekmece ? b.size.z * 0.85f : b.size.x * 0.9f;
                seri.FindProperty("sure").floatValue = 0.45f;
            }
            else if (kapi)
            {
                // Unity'de +Y etrafında pozitif dönüş +X'i -Z'ye çevirir: gövdesi +X'te duran kapı eksi
                // yönde dönerse öne (+Z) açılır, -X'te duran artı yönde.
                seri.FindProperty("hareket").enumValueIndex = (int)AcilirParca.Hareket.Dondur;
                seri.FindProperty("eksen").vector3Value = Vector3.up;
                seri.FindProperty("miktar").floatValue = b.center.x > 0f ? -105f : 105f;
                seri.FindProperty("sure").floatValue = 0.6f;
                if (ilkKapi == null) ilkKapi = a;
            }
            else
            {
                // Kapak arka üst kenarında döner: x ekseninde eksi dönüş ön kenarı (+Z) yukarı kaldırır.
                seri.FindProperty("hareket").enumValueIndex = (int)AcilirParca.Hareket.Dondur;
                seri.FindProperty("eksen").vector3Value = Vector3.right;
                seri.FindProperty("miktar").floatValue = -100f;
                seri.FindProperty("sure").floatValue = 0.7f;
            }
            seri.ApplyModifiedPropertiesWithoutUndo();
            parcalar.Add(a);
        }
        if (parcalar.Count > 0)
        {
            Yonlendirici y = govde.AddComponent<Yonlendirici>();
            var seri = new SerializedObject(y);
            SerializedProperty dizi = seri.FindProperty("parcalar");
            dizi.arraySize = parcalar.Count;
            for (int i = 0; i < parcalar.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = parcalar[i];
            seri.ApplyModifiedPropertiesWithoutUndo();
        }
        if (buzdolabiIsigi.HasValue && ilkKapi != null)
        {
            // Buzdolabının iç ışığı (bkz. GrayboxKurucu_Daire.BuzdolabiIsigiKur): kapı açıkken ve elektrik varken yanar.
            GameObject l = new GameObject("Elektrik_Isigi");
            l.transform.SetParent(gecici.transform, false);
            l.transform.localPosition = buzdolabiIsigi.Value;
            Light isik = l.AddComponent<Light>();
            isik.type = LightType.Point;
            isik.range = 0.9f;
            isik.intensity = 0.5f;
            isik.color = new Color(0.86f, 0.93f, 1f);
            isik.shadows = LightShadows.None;
            isik.enabled = false;
            BuzdolabiIsigi bi = gecici.AddComponent<BuzdolabiIsigi>();
            var bs = new SerializedObject(bi);
            bs.FindProperty("isik").objectReferenceValue = isik;
            bs.FindProperty("kapi").objectReferenceValue = ilkKapi;
            bs.ApplyModifiedPropertiesWithoutUndo();
        }
        CamlariSaydamYap(gecici);
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
        ParlamaEkle(gecici, new Vector3(0f, 0.01f, 0f), 1f);   // kafa lambasında parlasın (#83)
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
            Sus(Sablon("Perde"), 1.40f, 2.30f, PetekOnuPerde, 0f, -1f),
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
    // Kat dekoru yardımcıları (#83): bir şablonu (adıyla) planın (u, y, v) noktasına, ön yüzü (onU, onV)
    // yönüne bakacak şekilde koyar. yol "B:" ise ikinci dairede (#70). DA: adı verilmiş süs; zincir ona bu
    // adla başvurur ("Komodin_Ayla/Cekmece_1").
    static KatSusu D(string sablon, float u, float y, float v, float onU, float onV, string yol = "")
    { return new KatSusu { Sablon = Sablon(sablon), Yol = yol, X = u, Y = y, Z = v, OnU = onU, OnV = onV }; }
    static KatSusu DA(string ad, string sablon, float u, float y, float v, float onU, float onV, string yol = "")
    { return new KatSusu { Ad = ad, Sablon = Sablon(sablon), Yol = yol, X = u, Y = y, Z = v, OnU = onU, OnV = onV }; }

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

    // ---------- 2. KAT: ODA ODA YENİDEN DÖŞENDİ (#83, 2026-09-29) ----------
    // Daire 3 (kapıda 7), yaşlı Nuri Bey'in evi: eşini kaybetmiş, yalnız yaşayan, radyo meraklısı emekli.
    // Birkaç gün önce hastaneye kaldırılmış (Yönetim'in notu); ev dolu ve yaşanmış, aniden bırakılmış.
    // Katın imza mekaniği sesle bulma (#62): radyo kilitli yatak odasında kendiliğinden çalar, kapalı
    // kapının ardından boğuk duyulur. Kurallar: katlarda aynı model yok; ipucu notu yok; bir odada en
    // fazla bir görev parçası.
    //
    // Kapı sırası ve görev parçaları:
    //   1. Salon (kilitsiz): dikiş makinesinin çekmecesinde Oda A'nın anahtarı.
    //   2. Oda A (anahtar): komodinin üstündeki radyonun kadranında 102.7.
    //   3. Kiler (şifreli asma kilit 1027): tamir tezgâhındaki bisküvi tenekesinde katın anahtarı.
    //   Antre, hol ve mutfak kilitsiz ve görev parçasız; banyo ve Oda B kapalı.
    static KatZinciriVarligi Kat2Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_2.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "2. kat, daire 3, yaşlı Nuri Bey (hastanede; sesle bulma). Salonda dikiş makinesinin çekmecesinde Oda A'nın " +
                     "anahtarı; Oda A'da radyo kendiliğinden çalar, kadranında 102.7; kilerin asma kilidi 1027, tamir tezgâhındaki " +
                     "tenekede katın anahtarı. Banyo ve Oda B kapalı. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_B" });
        // Yerler adı verilmiş süslerin parçaları (#83): dikiş makinesinin çekmecesi, teneke kutu.
        z.Yerler.Add(new ZincirYeri { Kimlik = "dikis", Yol = "Dikis_Nuri/Cekmece_1", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "radyo", Yol = "Esyalar/Radyo", Oda = "Oda_A" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "teneke", Yol = "Teneke_Nuri", Oda = "Oda_C", KilitYolu = "Teneke_Nuri/Kapak" });
        // Çekmecenin ve tenekenin pivotu iç tabanları: eşya birkaç milimetre üstünde yatar.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "dikis", Y = 0.004f, Donus = 25f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "teneke", Y = 0.008f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "odaA_anahtari" });
        // Radyonun kadranı (radyo.py): 102.7 kadran camının üstünde yazılı (eski zincirden, #62).
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "radyo",
                                          Sifre = "1027", IpucuMetni = "102.7", IpucuX = -0.085f, IpucuY = 0.137f, IpucuZ = 0.0705f,
                                          IpucuOlcek = 0.7f });
        v.zincir = z;
        v.parcalar = new List<KatParcasi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        // Radyo dairenin kalıcı eşyası (Esyalar/Radyo); zincir ona başvurduğu için görünür.
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah" };
        v.sesler = new List<KatSesi>
        {
            new KatSesi { Yol = "Esyalar/Radyo", Oda = "Oda_A", Gecikme = 4f, Ses = 0.8f,
                          Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Daire/Radyo_Istasyon.wav") },
        };
        if (v.sesler[0].Klip == null) Debug.LogError("[ZINCIR] Radyo_Istasyon.wav bulunamadı (Art/Ses/scripts/kayit_isle.py radyo).");

        KatSusu Sus(GameObject sablon, float u, float y, float vv, float onU, float onV)
        { return new KatSusu { Sablon = sablon, X = u, Y = y, Z = vv, OnU = onU, OnV = onV }; }
        // Adı verilmiş süs: zincir ona bu adla başvurur ("Dikis_Nuri/Cekmece_1").
        KatSusu SusA(string ad, GameObject sablon, float u, float y, float vv, float onU, float onV)
        { return new KatSusu { Ad = ad, Sablon = sablon, X = u, Y = y, Z = vv, OnU = onU, OnV = onV }; }

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
            // Kuş kafesi: askılı ayağıyla Oda B ile Oda A kapılarının arasında; yem kabı boş. (Olay yerinin
            // yanındayken devrik sandalyeyle birlikte holü kapatıyordu: yürünebilirlik denetimi.)
            Sus(Sablon("Kus_Kafesi"), 7.70f, 0f, 4.40f, 0f, -1f),
            // Olay yeri: kiler kapısının yanında kapağı açık ilaç dolabı; altında saçılmış ilaçlar,
            // yan yatmış hasır sandalye ve kırık gözlük.
            Sus(Sablon("Ilac_Dolabi"), 4.60f, 1.45f, 2.95f, 0f, 1f),
            Sus(Sablon("Sacilmis_Ilaclar"), 4.65f, 0.004f, 3.45f, 0f, 1f),
            // Devrik sandalye güney duvarına yaslı: koridorun ortasında yatarken holü boydan boya kapatıyordu
            // (yürünebilirlik denetimi); şimdi kuzey tarafta 75 cm geçit kalıyor.
            Sus(Sablon("Devrik_Sandalye"), 4.05f, 0f, 3.45f, 0f, 1f),
            Sus(Sablon("Kirik_Gozluk"), 5.05f, 0.008f, 3.95f, 0.3f, 1f),

            // ---------- SALON (Şahan'ın seçimi: 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14) ----------
            // Salon u 0-5,0, v 4,75-8,1. Kapı hol duvarında (u 0,85-1,75); bu dairede içeri, kanepe tarafına
            // açılır, açık kanat u 0,7-0,9, v 4,7-5,6'da durur. Karşı duvarda pencere (u 0,7-2,1, denizlik
            // 0,87-0,90 m, v 8,04-8,20) ve hep kilitli balkon kapısı (u 3,2-4,0). Dolu, yaşanmış bir salon.
            // Pencere: kadife fon perde ve tül; denizlikte kurumuş sardunyalar; altında petek.
            Sus(Sablon("Perde_Kadife"), 1.40f, 2.32f, PetekOnuPerde, 0f, -1f),
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
            SusA("Dikis_Nuri", Sablon("Dikis_Makinesi"), 4.50f, 0f, 8.10f - 0.225f, 0f, -1f),

            // ---------- MUTFAK (Şahan'ın seçimi: 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12) ----------
            // Mutfak u 6,8-10,5, v -0,8-2,85. Kapı hol duvarında (u 7,0-7,8). Dip duvarda (u = 10,6) pencere ve
            // altında dairenin tezgâhı (u 9,88-10,5, v 0,09-1,91, üstü 0,90). Onun son sabahı: masada yarım
            // kahvaltı, ocakta soğumuş çorba, bulaşıklıkta tek tabak; her şey yerli yerinde, sahibi yok.
            // 70'ler buzdolabı köşede; kapısında ilaç saatleri kâğıdı, üstünde plastik çiçekli vazo.
            Sus(Sablon("Buzdolabi_70"), 7.25f, 0f, -0.80f + 0.33f, 0f, 1f),
            // Emaye tüplü ocak ve yanında ona bağlı turuncu tüp.
            Sus(Sablon("Ocak_Emaye"), 8.25f, 0f, -0.80f + 0.31f, 0f, 1f),
            Sus(Sablon("Tup_Turuncu"), 8.95f, 0f, -0.55f, 0f, 1f),
            // Kiler duvarında teneke kutu rafı.
            Sus(Sablon("Teneke_Rafi"), 6.80f, 1.50f, 1.20f, 1f, 0f),
            // Pencerenin alt yarısında kareli kafe perdesi; tezgâhta evyenin yanında tek kişilik bulaşıklık.
            Sus(Sablon("Mutfak_Perdesi"), 10.50f, 1.62f, 1.00f, -1f, 0f),
            Sus(Sablon("Bulasiklik"), 10.22f, 0.90f, 0.95f, -1f, 0f),
            // Muşambalı masa ortada, üstünde yarım kahvaltı; tek sandalye.
            Sus(Sablon("Mutfak_Masasi"), 8.45f, 0f, 1.50f, 0f, 1f),
            Sus(Sablon("Mutfak_Sandalyesi"), 8.45f, 0f, 2.10f, 0f, -1f),

            // ---------- ODA A (yatak odası; görev: radyonun kadranı) ----------
            // Oda A u 7,7-10,5, v 4,75-8,1. Kapı hol duvarında (u 8,7-9,5), içeri açılır; açık kanat u 8,57-8,72,
            // v 4,75-5,5 boyunca durur, o köşe boş. Karşı duvarda (v = 8,1) pencere (u 8,5-9,7, denizlik 0,9).
            // Karyola başı pencerenin altında, duvara dayalı (başlık 0,86: denizliğin altında kalır); solunda
            // komodin ve üstünde dairenin radyosu (Esyalar/Radyo, u 8,1 v 7,94, komodinin üstü 0,55).
            Sus(Sablon("Karyola_Nuri"), 9.20f, 0f, 8.10f - 1.00f, 0f, -1f),
            SusA("Komodin_Nuri", Sablon("Komodin_Nuri"), 8.10f, 0f, 8.10f - 0.205f, 0f, -1f),
            Sus(Sablon("Perde_Cicekli"), 9.10f, 2.30f, 8.10f, 0f, -1f),
            // Elbise dolabı bu odaya sığmadı: hangi duvara konsa radyonun önüne giden yol oyuncunun kapsülüne dar
            // kalıyordu (yürünebilirlik denetimi). Karyola 10 cm doğuda; batısında radyoya 69 cm'lik yol.
            // Eşinin aynalı tuvalet masası dip duvarda (u = 10,5). Önündeki ceketli sandalye kaldırıldı: odada
            // kapının önünden başka yürünecek yer bırakmıyordu (yürünebilirlik denetimi, 0,9 m²). Nuri Bey'in
            // ceketi ve pantolonu artık karyola modelinde, ayak ucunda eşinin boş yanına atılmış.
            Sus(Sablon("Tuvalet_Masasi"), 10.50f - 0.215f, 0f, 5.45f, -1f, 0f),
            // Karyolanın ayak ucunda çizgili kilim; hol duvarında, köşede yaldızlı hat levhası.
            Sus(Sablon("Kilim_Nuri"), 9.25f, 0f, 5.78f, 0f, 1f),
            Sus(Sablon("Hat_Levha"), 10.00f, 1.70f, 4.75f, 0f, 1f),

            // ---------- KİLER (Oda C; görev: tenekede katın anahtarı) ----------
            // Kiler u 4,6-6,7, v -0,8-2,85. Kapı hol duvarında (u 5,25-6,05), içeri açılır; kapının önü
            // (v 1,95'ten sonrası) boş kalır. Nuri Bey'in radyo tamir köşesi: dip duvarda tezgâh, banyo
            // duvarında erzak rafı.
            Sus(Sablon("Kiler_Rafi"), 4.60f + 0.215f, 0f, 0.30f, 1f, 0f),
            Sus(Sablon("Radyo_Tezgahi"), 5.85f, 0f, -0.80f + 0.31f, 0f, 1f),
            // Bisküvi tenekesi tezgâhın önünde, şasinin önünde (tezgâhın üstü 0,78).
            SusA("Teneke_Nuri", Sablon("Teneke_Nuri"), 5.85f, 0.78f, -0.32f, 0f, 1f),
            Sus(Sablon("Kiler_Cuvallari"), 6.33f, 0f, 0.60f, -1f, 0f),
            Sus(Sablon("Supurge_Faras"), 6.70f, 0f, 1.55f, -1f, 0f),
            Sus(Sablon("Damacana"), 4.85f, 0f, 1.50f, 1f, 0f),
            Sus(Sablon("Sogan_Sepeti"), 4.86f, 0f, 1.95f, 1f, 0f),
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s2 = z.Cozumle(odalar);
        if (!s2.Cozulebilir) Debug.LogError("[ZINCIR] 2. kat zinciri çözülemiyor: " + string.Join("; ", s2.Hatalar));
        else Debug.Log("[ZINCIR] 2. kat zinciri çözülebilir: " + string.Join(" -> ", s2.AcilanKilitler));
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
        string[] yollar = { "Kapi_1/Paspas", "Kapi_1/Terlikler", "Kapi_1/Mentese/Kapi_Gozu_Isik", "Kapi_1/Mentese/Kapi_Alti_Isik" };
        bool[] gorunur = { false, false, true, true };
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
        ParlamaEkle(gecici, new Vector3(0f, 0.01f, 0f), 0.55f);   // kâğıt: metalden sönük parlar (#83)
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
