// AynaKurucu.cs
// Banyo aynalarını kuran Editor aracı (#94).
//
// ---------- NE KURULUYOR ----------
// Her dairenin banyosunda, lavabonun üstündeki duvarda 45 x 60 cm'lik, merkezi yerden 1,45 m
// yükseklikte bir ayna. Lavabo gibi binanın ORTAK demirbaşıdır (her dairede aynı; "katlar arası
// aynı model olmasın" yasağına girmez, CLAUDE.md #83). Parçalar:
//   Banyo_Aynasi            kök: pivot duvarın yüzünde, yerel +Z odaya bakar
//     Arkalik               ince arka levha (duvardan 2 mm açıkta: duvarla aynı düzlemde titreşmesin)
//     Cerceve_Ust/Alt/Sol/Sag   alüminyum görünümlü ince çerçeve
//     Cam                   Unity Quad'ı; AynaYansimasi + AynaKorkusu bunun üstünde; duvardan 1,1 cm
//     Yansima_Kamerasi      aynanın arkasındaki ikinci göz (KAPALI kaydedilir; oyun açar)
// Hiçbir parçada çarpıştırıcı yok: 2 cm'lik çıkıntı oyuncuyu takmaz, nişan ışını da arkadaki
// duvarda durur.
//
// Ayna LAVABONUN ÇOCUĞU olur: kat dosyası lavaboyu gizlediğinde (oda oda döşenen 1-4. katlarda
// lavabo listede yok, banyolar da kapalı) ayna da onunla gizlenir; ayrıca bir kural gerekmez.
//
// ---------- HARİÇ KATLAR ----------
// AynasizKatlar: bu katlarda ayna görünmez (AynaYansimasi oyun sırasında gizler). Daire her katta
// aynı nesne olduğu için katı kurulumda değil, oyunda ayırmak zorundayız.
//   5. kat: Banyo_Dolabi_K5 (aynalı sac dolap) tam aynı yerde (u 3,60, y 1,50, v -0,80).
//   7. kat: Cemal'in nüfus cüzdanı notu lavabonun üstünde, duvarda 1,25 m'de (not.kat7_kimlik);
//           ayna (1,15-1,75 m) onu örterdi ve 7. katın şifresi okunamazdı.
//
// ---------- MALZEMELER (diskte, BAĞLI) ----------
// Assets/Materials/Ayna.mat: URP Unlit (ışıktan etkilenmez: yansımanın ışığı zaten resmin içinde).
// Doku ölçeği x = -1, kaydırma 1: resim yatayda çevrilir (ayna sağ-sol terstir). Oyunda
// AynaYansimasi dokuyu ve rengi MaterialPropertyBlock ile verir; malzemedeki değerler yedektir.
// Malzeme sahnede bir çiziciye bağlı olduğu için yapıya (build) girer (#26: adla aranmaz).

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class AynaKurucu
{
    // Aynanın plandaki yeri (dairenin u / v eksenleri, bkz. DairePlani): lavabonun ekseni (u 3,60)
    // ve banyonun dış duvarının iç yüzü (v -0,80). Yön plan +v (odaya doğru), lavabonun önüyle aynı.
    public const float AynaU = 3.60f;
    public const float AynaV = -0.80f;
    public const float AynaMerkezY = 1.45f;    // camın ortası yerden (lavabonun üst kenarı 0,85)

    // Bu katlarda ayna gizli (gerekçeler dosyanın başında). Bir kat eklemek/çıkarmak için yalnızca
    // bu listeyi değiştirip "Huzur → Aynaları Kur" çalıştırmak yeter.
    public static readonly int[] AynasizKatlar = { 5, 7 };

    private const float CamGen = 0.45f, CamYuk = 0.60f;      // camın ölçüsü
    private const float CerceveGen = 0.025f;                  // çerçeve çıtasının genişliği
    private const float CerceveDerin = 0.018f;                // çerçevenin duvardan çıkıntısı
    private const float DuvarPayi = 0.002f;                   // arkalığın duvardan boşluğu
    private const float CamDerin = 0.011f;                    // camın duvardan uzaklığı (≈1 cm)

    private const string AynaMalzemeYolu = "Assets/Materials/Ayna.mat";
    private const string CerceveMalzemeYolu = "Assets/Materials/Ayna_Cerceve.mat";
    private const string GolgeYolu = "Assets/Prefabs/Esyalar/Kacis_Golgesi.prefab";
    // Arkadan gelen döşeme gıcırtısı (parke adımlarının sesi, AyakSesi). Ayrı bir "korku vuruşu"
    // sesi projede yok; gerçek bir gıcırtı "biri arkamda" hissini müzikli bir vuruştan iyi verir.
    private const string SesYolu = "Assets/Resources/Sesler/Gicirti_2.wav";

    /// <summary>
    /// Bir dairenin banyo aynasını kurar. daire: Daire_N kökü. ebeveyn: lavabo (yoksa daire).
    /// yerelMerkez: camın ortasının DUVAR YÜZÜNDEKİ noktası, dairenin yerel uzayında.
    /// yerelOn: odaya bakan yön, dairenin yerel uzayında.
    /// </summary>
    public static GameObject BanyoAynasiKur(Transform daire, Transform ebeveyn, Vector3 yerelMerkez, Vector3 yerelOn)
    {
        if (daire == null) return null;
        // Dairenin yerel noktasını dünyaya çeviriyoruz: ebeveyn (lavabo) dönük ve FBX'ten gelen
        // kendi duruşunu taşıyabilir, dünyada kurup sonra bağlamak her durumda doğru sonuç verir.
        Vector3 dunyaMerkez = daire.TransformPoint(yerelMerkez);
        Vector3 dunyaOn = daire.TransformDirection(yerelOn).normalized;

        GameObject kok = new GameObject("Banyo_Aynasi");
        kok.transform.SetPositionAndRotation(dunyaMerkez, Quaternion.LookRotation(dunyaOn, Vector3.up));
        // SetParent(..., true): dünyadaki yerini koru (yerel değerleri ebeveyne göre yeniden hesapla).
        Transform hedefEbeveyn = ebeveyn != null ? ebeveyn : (daire.Find("Esyalar") != null ? daire.Find("Esyalar") : daire);
        kok.transform.SetParent(hedefEbeveyn, true);
        Transform t = kok.transform;

        Material cerceveMalzeme = CerceveMalzemesi();
        float disGen = CamGen + 2f * CerceveGen, disYuk = CamYuk + 2f * CerceveGen;

        // Arkalık: camın arkasında koyu, ince levha (camın kenarındaki boşluktan duvar görünmesin).
        Renderer arka = Kutu("Arkalik", t, new Vector3(0f, 0f, DuvarPayi + 0.004f), new Vector3(disGen, disYuk, 0.008f), cerceveMalzeme);
        // Çerçevenin dört çıtası: üst/alt camın tam genişliğinde, sol/sağ camın yüksekliğinde.
        float z = DuvarPayi + CerceveDerin / 2f;
        Renderer ust = Kutu("Cerceve_Ust", t, new Vector3(0f, CamYuk / 2f + CerceveGen / 2f, z), new Vector3(disGen, CerceveGen, CerceveDerin), cerceveMalzeme);
        Renderer alt = Kutu("Cerceve_Alt", t, new Vector3(0f, -CamYuk / 2f - CerceveGen / 2f, z), new Vector3(disGen, CerceveGen, CerceveDerin), cerceveMalzeme);
        Renderer sol = Kutu("Cerceve_Sol", t, new Vector3(-CamGen / 2f - CerceveGen / 2f, 0f, z), new Vector3(CerceveGen, CamYuk, CerceveDerin), cerceveMalzeme);
        Renderer sag = Kutu("Cerceve_Sag", t, new Vector3(CamGen / 2f + CerceveGen / 2f, 0f, z), new Vector3(CerceveGen, CamYuk, CerceveDerin), cerceveMalzeme);

        // Cam: Unity'nin Quad'ı (1 x 1 m, yüzü yerel -Z'ye bakar). 180° çevirince yüzü odaya bakar.
        GameObject cam = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cam.name = "Cam";
        Object.DestroyImmediate(cam.GetComponent<Collider>());     // çarpıştırıcı yok (bkz. başlık)
        cam.transform.SetParent(t, false);
        cam.transform.localPosition = new Vector3(0f, 0f, CamDerin);
        cam.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        cam.transform.localScale = new Vector3(CamGen, CamYuk, 1f);
        MeshRenderer camCizici = cam.GetComponent<MeshRenderer>();
        camCizici.sharedMaterial = AynaMalzemesi();
        // Işıktan etkilenmeyen yüzey gölge düşürmez, gölge de almaz.
        camCizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        camCizici.receiveShadows = false;

        // Yansıma kamerası: kökün çocuğu (ölçeği 1). Cam'ın çocuğu olsaydı camın 0,45 x 0,6 ölçeği
        // kameranın matrisini bozardı. KAPALI kaydedilir: Editor'de boşuna çizmesin; oyunda
        // AynaYansimasi gerektiğinde açar.
        GameObject kameraNesnesi = new GameObject("Yansima_Kamerasi");
        kameraNesnesi.transform.SetParent(t, false);
        Camera kamera = kameraNesnesi.AddComponent<Camera>();
        kamera.enabled = false;
        kamera.nearClipPlane = 0.01f;
        kamera.farClipPlane = 15f;
        kamera.useOcclusionCulling = false;
        kamera.allowMSAA = false;
        kamera.allowHDR = true;
        UniversalAdditionalCameraData veri = kameraNesnesi.AddComponent<UniversalAdditionalCameraData>();
        veri.renderPostProcessing = false;   // efektler oyuncu kamerasında bir kez uygulanır
        veri.renderShadows = true;
        veri.antialiasing = AntialiasingMode.None;

        // Bileşenler ve bağlantıları. SerializedObject: bileşenin özel (private) kayıtlı alanlarını
        // koddan doldurmanın yolu.
        AynaYansimasi yansima = cam.AddComponent<AynaYansimasi>();
        var seri = new SerializedObject(yansima);
        seri.FindProperty("yansimaKamerasi").objectReferenceValue = kamera;
        SerializedProperty cerceveDizi = seri.FindProperty("cerceve");
        Renderer[] parcalar = { arka, ust, alt, sol, sag };
        cerceveDizi.arraySize = parcalar.Length;
        for (int i = 0; i < parcalar.Length; i++) cerceveDizi.GetArrayElementAtIndex(i).objectReferenceValue = parcalar[i];
        SerializedProperty haric = seri.FindProperty("haricKatlar");
        haric.arraySize = AynasizKatlar.Length;
        for (int i = 0; i < AynasizKatlar.Length; i++) haric.GetArrayElementAtIndex(i).intValue = AynasizKatlar[i];
        seri.ApplyModifiedPropertiesWithoutUndo();

        AynaKorkusu korku = cam.AddComponent<AynaKorkusu>();
        var korkuSeri = new SerializedObject(korku);
        GameObject golge = AssetDatabase.LoadAssetAtPath<GameObject>(GolgeYolu);
        if (golge == null) Debug.LogWarning($"[AYNA] {GolgeYolu} yok; aynadaki korku çalışmaz.");
        korkuSeri.FindProperty("sablon").objectReferenceValue = golge;
        AudioClip ses = AssetDatabase.LoadAssetAtPath<AudioClip>(SesYolu);
        if (ses == null) Debug.LogWarning($"[AYNA] {SesYolu} yok; korku sessiz olur.");
        korkuSeri.FindProperty("vurgu").objectReferenceValue = ses;
        korkuSeri.ApplyModifiedPropertiesWithoutUndo();

        return kok;
    }

    // ---------- MENÜ: AÇIK SAHNEDEKİ DÖRT DAİREYE KUR ----------
    // Bütün graybox'ı yeniden kurmadan aynaları kurar/yeniler. Daireler sahnede KAPALI durur;
    // GameObject.Find kapalı nesneleri bulmaz, bu yüzden kökten başlayıp transform.Find ile iniyoruz
    // (transform.Find kapalı çocukları da bulur).
    [MenuItem("Huzur/Aynaları Kur")]
    public static void AynalariKur()
    {
        Transform sahanlik = SahanlikBul();
        if (sahanlik == null) { Debug.LogError("[AYNA] Sahnede Daire_1'i olan Sahanlik_A bulunamadı; önce graybox kurulmalı."); return; }

        int kurulan = 0;
        for (int n = 1; n <= 4; n++)
        {
            Transform daire = sahanlik.Find($"Daire_{n}");
            if (daire == null) { Debug.LogWarning($"[AYNA] Daire_{n} yok."); continue; }
            DaireEksenleri eksen = daire.GetComponent<DaireEksenleri>();
            if (eksen == null) { Debug.LogWarning($"[AYNA] Daire_{n}'de DaireEksenleri yok; graybox yeniden kurulmalı."); continue; }

            // Eski aynayı sil (iki yerde olabilir: lavabonun altında ya da lavabo yokken Esyalar'da).
            Transform lavabo = daire.Find("Esyalar/Lavabo");
            EskiyiSil(lavabo);
            EskiyiSil(daire.Find("Esyalar"));

            BanyoAynasiKur(daire, lavabo, eksen.Nokta(AynaU, AynaMerkezY, AynaV), eksen.Yon(0f, 1f));
            kurulan++;
        }
        Debug.Log($"[AYNA] {kurulan} daireye banyo aynası kuruldu (hariç katlar: {string.Join(", ", AynasizKatlar)}).");
        // Koddan eklenen nesneler sahneyi "değişti" diye işaretlemez; kurucu kendisi kaydeder (CLAUDE.md).
        GrayboxKurucu.SahneyiKaydet("Aynalar");
    }

    private static void EskiyiSil(Transform ebeveyn)
    {
        if (ebeveyn == null) return;
        Transform eski = ebeveyn.Find("Banyo_Aynasi");
        while (eski != null)
        {
            Object.DestroyImmediate(eski.gameObject);
            eski = ebeveyn.Find("Banyo_Aynasi");
        }
    }

    // Açık sahnede Daire_1 çocuğu olan "Sahanlik_A"yı arar (kapalı nesneler dahil).
    private static Transform SahanlikBul()
    {
        foreach (GameObject kok in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            // true: kapalı olanlar da.
            foreach (Transform t in kok.GetComponentsInChildren<Transform>(true))
                if (t.name == "Sahanlik_A" && t.Find("Daire_1") != null) return t;
        }
        return null;
    }

    // ---------- YARDIMCILAR ----------
    // Çarpıştırıcısız kutu (GrayboxKurucu.Kutu'nun benzeri; o özel olduğu için buradan çağrılamaz).
    private static Renderer Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 olcek, Material malzeme)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ad;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;
        g.transform.localScale = olcek;
        MeshRenderer r = g.GetComponent<MeshRenderer>();
        r.sharedMaterial = malzeme;
        return r;
    }

    // Aynanın camı: URP Unlit. Dosya varsa değerleri yeniden yazılır (bu satırlardaki ayar her
    // kurulumda geçerli olsun).
    private static Material AynaMalzemesi()
    {
        Material m = AssetDatabase.LoadAssetAtPath<Material>(AynaMalzemeYolu);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(m, AynaMalzemeYolu);
        }
        m.SetColor("_BaseColor", new Color(0.02f, 0.022f, 0.025f));   // yansıma gelene kadar koyu cam
        m.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));          // yatayda çevir (ayna)
        m.SetTextureOffset("_BaseMap", new Vector2(1f, 0f));
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        return m;
    }

    // Çerçeve: matlaşmış alüminyum (90'ların banyo aynası).
    private static Material CerceveMalzemesi()
    {
        Material m = AssetDatabase.LoadAssetAtPath<Material>(CerceveMalzemeYolu);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, CerceveMalzemeYolu);
        }
        m.SetColor("_BaseColor", new Color(0.52f, 0.52f, 0.50f));
        m.SetFloat("_Metallic", 0.6f);
        m.SetFloat("_Smoothness", 0.45f);
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        return m;
    }
}
