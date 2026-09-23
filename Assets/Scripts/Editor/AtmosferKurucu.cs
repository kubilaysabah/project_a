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
        Debug.Log("Atmosfer kuruldu: karanlık ortam, ay ışığı, gece post-processing profili.");
    }

    // ---------- ORTAM IŞIĞI ----------
    static void OrtamIsigi()
    {
        // Flat: gökyüzüne bakmadan, tek ve sabit bir renk kullan.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Neredeyse siyah, hafif mavi: gece binanın içine sızan en ufak ışık.
        // Tamamen siyah yapmıyoruz; oyuncu karanlık köşede duvarın varlığını
        // hâlâ seçebilmeli, yoksa ekran düz siyah bir leke olur.
        RenderSettings.ambientLight = new Color(0.018f, 0.020f, 0.028f);

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
        ay.intensity = 0.06f;
        ay.shadows = LightShadows.Soft;

        // Işığın yönü: +z'den (pencerenin dışından) içeri ve aşağı doğru.
        ay.transform.rotation = Quaternion.Euler(35f, 180f, 0f);
    }

    // ---------- POST-PROCESSING ----------
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
        foreach (VolumeComponent eski in profil.components.ToArray())
        {
            profil.Remove(eski.GetType());
            Object.DestroyImmediate(eski, true);
        }

        // "Add<T>(true)": profile T türünde bir efekt ekler. true = bütün
        // ayarlarını "bu profil belirler" diye işaretle. "Override(değer)" bir
        // ayarı hem değiştirir hem de işaretler; işaretsiz ayar hiç uygulanmaz.

        // TONEMAPPING: sahnedeki ışık aralığı ekranın gösterebileceğinden geniştir
        // (ampulün kendisi duvardan yüzlerce kat parlak). Tonemapping bu aralığı
        // gerçekçi biçimde sıkıştırır; olmasa parlak yerler düz beyaz lekeye döner.
        // ACES, sinema kameralarının tepkisini taklit eden, kontrastı yüksek bir eğri.
        Tonemapping ton = profil.Add<Tonemapping>(true);
        ton.mode.Override(TonemappingMode.ACES);

        // RENK AYARI: ACES görüntüyü biraz karartır; pozlamayla dengeliyoruz.
        // Doygunluğu kısmak 90'lar apartmanının solgun, yorgun rengini verir.
        ColorAdjustments renk = profil.Add<ColorAdjustments>(true);
        renk.postExposure.Override(0.6f);
        renk.contrast.Override(12f);
        renk.saturation.Override(-18f);

        // BÖLÜNMÜŞ TONLAMA (split toning): gölgelere bir renk, aydınlık yerlere
        // başka bir renk katar. Gölgeler soğuk mavi, ampulün aydınlattığı yerler
        // sıcak sarı: konseptteki "soğuk gece / sıcak ampul" zıtlığı tam olarak bu.
        SplitToning tonlama = profil.Add<SplitToning>(true);
        tonlama.shadows.Override(new Color(0.30f, 0.40f, 0.60f));
        tonlama.highlights.Override(new Color(0.75f, 0.58f, 0.36f));
        tonlama.balance.Override(-15f);   // eksi: gölge rengi daha geniş bir alana yayılır

        // VIGNETTE: ekran kenarlarını karartır. Göz ortaya çekilir, çevre görüşü
        // daralır; oyuncu köşelerde bir şey olup olmadığından emin olamaz.
        Vignette vinyet = profil.Add<Vignette>(true);
        vinyet.intensity.Override(0.38f);
        vinyet.smoothness.Override(0.45f);

        // FİLM GRENİ: karanlık alanlarda ince bir kumlanma. Düz, "temiz"
        // bilgisayar karanlığını kırar; eski bir kameranın gece çekimi gibi.
        FilmGrain gren = profil.Add<FilmGrain>(true);
        gren.type.Override(FilmGrainLookup.Thin2);
        gren.intensity.Override(0.28f);
        gren.response.Override(0.85f);   // yüksek: gren daha çok karanlıkta görünür

        // BLOOM: çok parlak noktaların etrafına ışık taşması. Ampulün camı
        // eşiği aşacak kadar parlak; etrafında yumuşak bir hale oluşur.
        Bloom bloom = profil.Add<Bloom>(true);
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
