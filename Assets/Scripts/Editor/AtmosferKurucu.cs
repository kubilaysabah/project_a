// AtmosferKurucu.cs
// Gece atmosferini kuran editör aracı (#11): ortam ışığı, ay ışığı, post-processing
// ve kamera ayarı. Graybox yeniden kurulduğunda da otomatik çalışır, böylece
// atmosfer hiçbir zaman "bir zamanlar elle ayarlanmıştı" durumunda kalmaz.
//
// ---------- ÜÇ KAVRAM ----------
// ORTAM IŞIĞI (ambient): hiçbir lambadan gelmeyen, her yüzeye eşit vuran genel ışık.
// Varsayılanı gökyüzünden gelir ve tam şiddettedir; bu yüzden graybox her yerde
// eşit gri görünüyordu ve ampuller sönünce bile ortalık kararmıyordu. Burada neredeyse
// siyaha çekiyoruz: sahneyi artık ampuller aydınlatacak, ampulden uzak köşe kararacak.
//
// POST-PROCESSING: kamera görüntüyü çizdikten SONRA, bitmiş kareye uygulanan efektler.
// Bir fotoğrafa sonradan filtre uygulamak gibi: sahnedeki hiçbir nesne değişmez,
// yalnızca ekrana giden resim değişir.
//
// VOLUME: post-processing ayarlarının sahnedeki taşıyıcısı. "Global" bir Volume
// kameranın nerede olduğuna bakmaksızın her yerde geçerlidir. Ayarların kendisi
// ayrı bir dosyada (VolumeProfile) durur; Volume o dosyayı gösterir.

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;               // Volume, VolumeProfile
using UnityEngine.Rendering.Universal;     // URP'nin efektleri ve kamera verisi

public static class AtmosferKurucu
{
    const string ProfilYolu = "Assets/Settings/Atmosfer_Gece.asset";

    [MenuItem("Huzur/Atmosferi Kur")]
    public static void Kur()
    {
        OrtamIsigi();
        AyIsigi();
        PostProcessing();
        Kamera();
        MuzikVeDisSesler();
        Debug.Log("Atmosfer kuruldu: karanlık ortam, ay ışığı, gece post-processing profili, müzik ve dış sesler.");
    }

    // ---------- ORTAM IŞIĞI ----------
    static void OrtamIsigi()
    {
        // Flat: gökyüzüne bakmadan, tek ve sabit bir renk kullan.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Neredeyse siyah, hafif mavi: gece binanın içine sızan en ufak ışık.
        // Tamamen siyah yapmıyoruz; oyuncu karanlık köşede duvarın varlığını
        // hâlâ seçebilmeli, yoksa ekran düz siyah bir leke olur.
        // #38: daha karanlık bir gece için üçte bir kısıldı (0,018 / 0,020 / 0,028 idi).
        RenderSettings.ambientLight = new Color(0.012f, 0.013f, 0.019f);

        // Yansımalar da gökyüzünden geliyor: parlak gündüz göğü asansörün çeliğinde
        // ve pirinç isimlikte yansıyıp gece hissini bozuyordu. Neredeyse kapatıyoruz.
        RenderSettings.reflectionIntensity = 0.12f;
    }

    // ---------- AY IŞIĞI ----------
    // Şablondan gelen güneş ışığı (Directional Light) gündüz şiddetindeydi.
    // Onu soluk mavi bir ay ışığına çeviriyoruz: pencereden ve merdiven gözünden
    // hafifçe sızar, ampullerin sıcak sarısıyla soğuk bir zıtlık kurar.
    static void AyIsigi()
    {
        Light ay = null;
        foreach (Light l in Object.FindObjectsByType<Light>())
        {
            if (l.type == LightType.Directional) { ay = l; break; }
        }
        if (ay == null) return;

        ay.name = "Ay_Isigi";
        ay.color = new Color(0.55f, 0.65f, 1.0f);
        ay.intensity = 0.045f;   // #38: 0,06 idi
        ay.shadows = LightShadows.Soft;

        // Işığın yönü: +z'den (pencerenin dışından) içeri ve aşağı doğru.
        ay.transform.rotation = Quaternion.Euler(35f, 180f, 0f);
    }

    // ---------- POST-PROCESSING ----------
    // Profile bir efekt ekler VE onu profil dosyasının içine alt nesne olarak kaydeder (#31).
    //
    // profil.Add<T>() efekti yalnızca BELLEKTE oluşturur. Bir ScriptableObject'in diske
    // yazılması için ya kendi dosyası olmalı ya da başka bir dosyanın içine eklenmeli
    // (AddObjectToAsset). #11'de bu adım yoktu: dosyaya efektlerin yerine boş kayıtlar
    // (fileID: 0) yazıldı. Efektler yalnızca kurucu çalıştığı an bellekte vardı; Unity
    // kodu yeniden yükleyince (her Play'de) kayboldular. Oyun modunda ve build'de hiç
    // post-processing olmadı; Editor'deki kareler ise efektli göründüğü için fark edilmedi.
    //
    // <T> (generic, "tür parametresi"): fonksiyon her efekt türü için ayrı yazılmasın diye
    // türü parametre olarak alır: Ekle<Bloom>(profil), Ekle<Vignette>(profil)...
    // "where T : VolumeComponent": T yalnızca bir post-processing efekti olabilir.
    static T Ekle<T>(VolumeProfile profil) where T : VolumeComponent
    {
        T efekt = profil.Add<T>(true);
        efekt.name = typeof(T).Name;   // dosyada okunur bir ad
        // Unity'nin kendi Volume editörü de böyle yapar: alt nesneler Project penceresinde
        // ayrı birer dosya gibi görünüp kafa karıştırmasın.
        efekt.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(efekt, profil);
        return efekt;
    }

    static void PostProcessing()
    {
        // Profil dosyası: varsa aç ve içini temizle, yoksa üret. Silip yeniden
        // üretmiyoruz; dosyanın kimliği (GUID) değişirse sahnedeki bağlantı kopar.
        VolumeProfile profil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilYolu);
        if (profil == null)
        {
            profil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profil, ProfilYolu);
        }
        // Eski efektleri temizle. İki yerden: profilin listesi (içinde boş kayıtlar da
        // olabilir; #31 öncesi dosyada altı boş kayıt vardı) ve dosyanın alt nesneleri
        // (listede olmayan ama dosyada kalmış yetim efektler).
        profil.components.Clear();
        foreach (Object altNesne in AssetDatabase.LoadAllAssetsAtPath(ProfilYolu))
        {
            if (altNesne is VolumeComponent) Object.DestroyImmediate(altNesne, true);
        }

        // "Add<T>(true)": profile T türünde bir efekt ekler. true = bütün
        // ayarlarını "bu profil belirler" diye işaretle. "Override(değer)" bir
        // ayarı hem değiştirir hem de işaretler; işaretsiz ayar hiç uygulanmaz.

        // TONEMAPPING: sahnedeki ışık aralığı ekranın gösterebileceğinden geniştir
        // (ampulün kendisi duvardan yüzlerce kat parlak). Tonemapping bu aralığı
        // gerçekçi biçimde sıkıştırır; olmasa parlak yerler düz beyaz lekeye döner.
        // ACES, sinema kameralarının tepkisini taklit eden, kontrastı yüksek bir eğri.
        Tonemapping ton = Ekle<Tonemapping>(profil);
        ton.mode.Override(TonemappingMode.ACES);

        // RENK AYARI: ACES görüntüyü biraz karartır; pozlamayla dengeliyoruz.
        // Doygunluğu kısmak 90'lar apartmanının solgun, yorgun rengini verir.
        ColorAdjustments renk = Ekle<ColorAdjustments>(profil);
        renk.postExposure.Override(0.6f);
        renk.contrast.Override(12f);
        renk.saturation.Override(-18f);

        // BÖLÜNMÜŞ TONLAMA (split toning): gölgelere bir renk, aydınlık yerlere
        // başka bir renk katar. Gölgeler soğuk mavi, ampulün aydınlattığı yerler
        // sıcak sarı: konseptteki "soğuk gece / sıcak ampul" zıtlığı tam olarak bu.
        SplitToning tonlama = Ekle<SplitToning>(profil);
        tonlama.shadows.Override(new Color(0.30f, 0.40f, 0.60f));
        tonlama.highlights.Override(new Color(0.75f, 0.58f, 0.36f));
        tonlama.balance.Override(-15f);   // eksi: gölge rengi daha geniş bir alana yayılır

        // VIGNETTE: ekran kenarlarını karartır. Göz ortaya çekilir, çevre görüşü
        // daralır; oyuncu köşelerde bir şey olup olmadığından emin olamaz.
        Vignette vinyet = Ekle<Vignette>(profil);
        vinyet.intensity.Override(0.45f);   // #38: 0,38 idi; kenarlar daha kapalı, daha dar bir görüş
        vinyet.smoothness.Override(0.45f);

        // FİLM GRENİ: karanlık alanlarda ince bir kumlanma. Düz, "temiz"
        // bilgisayar karanlığını kırar; eski bir kameranın gece çekimi gibi.
        FilmGrain gren = Ekle<FilmGrain>(profil);
        gren.type.Override(FilmGrainLookup.Thin2);
        gren.intensity.Override(0.28f);
        gren.response.Override(0.85f);   // yüksek: gren daha çok karanlıkta görünür

        // BLOOM: çok parlak noktaların etrafına ışık taşması. Ampulün camı
        // eşiği aşacak kadar parlak; etrafında yumuşak bir hale oluşur.
        Bloom bloom = Ekle<Bloom>(profil);
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(0.7f);
        bloom.scatter.Override(0.65f);

        EditorUtility.SetDirty(profil);
        AssetDatabase.SaveAssets();

        // Sahnedeki Volume: yoksa üret, varsa profili yeniden bağla.
        GameObject nesne = GameObject.Find("Atmosfer");
        if (nesne == null) nesne = new GameObject("Atmosfer");
        Volume hacim = nesne.GetComponent<Volume>();
        if (hacim == null) hacim = nesne.AddComponent<Volume>();
        hacim.isGlobal = true;              // kamera nerede olursa olsun geçerli
        hacim.sharedProfile = profil;

        // GÖRÜNTÜ AYARLARI (#30): oyuncunun seçtiği kaliteyi, parlaklığı, çözünürlüğü,
        // ekran modunu ve V-Sync'i uygulayan bileşen. Kalite dosyalarını ona burada
        // BAĞLIYORUZ (adıyla aratmıyoruz): build yalnızca bağlanmış dosyaları paketler.
        KaliteKurucu.Kur();
        GorunumUygulayici uygulayici = nesne.GetComponent<GorunumUygulayici>();
        if (uygulayici == null) uygulayici = nesne.AddComponent<GorunumUygulayici>();
        SerializedObject seri = new SerializedObject(uygulayici);
        SerializedProperty dizi = seri.FindProperty("kaliteler");
        string[] yollar = { KaliteKurucu.DusukYolu, KaliteKurucu.OrtaYolu, KaliteKurucu.YuksekYolu };   // sıra: 0 Düşük, 1 Orta, 2 Yüksek
        dizi.arraySize = yollar.Length;
        for (int i = 0; i < yollar.Length; i++)
            dizi.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineAsset>(yollar[i]);
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- MÜZİK VE DIŞ SESLER (#36, #37) ----------
    // İkisi de sahnenin kökünde birer nesne; ses dosyaları ADIYLA aranmıyor, burada
    // bileşene BAĞLANIYOR (build yalnızca bağlanmış dosyaları paketler, #26 kuralı).
    static void MuzikVeDisSesler()
    {
        // Arka müzik: 2B, döngüde, çok kısık. Seviye 0,15: TV (0,5) ve kat olaylarının
        // (0,12-0,55) altında kalsın; anomali seslerini hiçbir zaman bastırmasın.
        AudioSource muzik = KokKaynak("Arka_Muzik");
        muzik.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Muzik/Muzik_Gerilim.wav");
        muzik.loop = true;
        muzik.volume = 0.15f;
        if (muzik.GetComponent<ArkaMuzik>() == null) muzik.gameObject.AddComponent<ArkaMuzik>();

        // Dış sesler: 2B, tek seferlik; ne zaman çalacağını DisSesleri seçer.
        AudioSource dis = KokKaynak("Dis_Sesler");
        dis.loop = false;
        dis.volume = 0.5f;
        DisSesleri bilesen = dis.GetComponent<DisSesleri>();
        if (bilesen == null) bilesen = dis.gameObject.AddComponent<DisSesleri>();
        SerializedObject seri = new SerializedObject(bilesen);
        KlipleriBagla(seri.FindProperty("kopek"), "Dis_Kopek_");
        KlipleriBagla(seri.FindProperty("silah"), "Dis_Silah_");
        KlipleriBagla(seri.FindProperty("ruzgar"), "Dis_Ruzgar_");
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    // Sahnenin kökünde verilen adda, 2B bir AudioSource taşıyan nesne (yoksa oluşturur).
    static AudioSource KokKaynak(string ad)
    {
        GameObject g = GameObject.Find(ad);
        if (g == null) g = new GameObject(ad);
        AudioSource k = g.GetComponent<AudioSource>();
        if (k == null) k = g.AddComponent<AudioSource>();
        k.playOnAwake = false;     // çalmayı bileşenler başlatır
        k.spatialBlend = 0f;       // 2B: kafanın içinden, yeri yok
        return k;
    }

    // Assets/Audio/Dis altında adı 'onek' ile başlayan bütün sesleri diziye bağlar
    // (Dis_Silah_1, Dis_Silah_2...). Yeni bir çeşit eklemek = dosyayı koyup kurucuyu çalıştırmak.
    static void KlipleriBagla(SerializedProperty dizi, string onek)
    {
        var liste = new System.Collections.Generic.List<AudioClip>();
        foreach (string guid in AssetDatabase.FindAssets(onek + " t:AudioClip", new[] { "Assets/Audio/Dis" }))
        {
            string yol = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileName(yol).StartsWith(onek)) liste.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(yol));
        }
        liste.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
        dizi.arraySize = liste.Count;
        for (int i = 0; i < liste.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = liste[i];
    }

    // ---------- KAMERA ----------
    // URP'de post-processing kamera başına açılır. Oyuncu kamerasında URP'nin
    // ek kamera bileşeni hiç yoktu; yani profile ne eklersek ekleyelim ekrana
    // hiçbir şey yansımazdı. Burada bileşeni ekleyip açıyoruz.
    static void Kamera()
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;

        UniversalAdditionalCameraData veri = kamera.GetComponent<UniversalAdditionalCameraData>();
        if (veri == null) veri = kamera.gameObject.AddComponent<UniversalAdditionalCameraData>();

        veri.renderPostProcessing = true;
        // SMAA: kenarlardaki merdiven şeklindeki tırtıkları yumuşatır (kenar yumuşatma).
        veri.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }
}
