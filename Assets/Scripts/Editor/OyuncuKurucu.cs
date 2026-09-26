// OyuncuKurucu.cs
// Oyuncu nesnesini (gövde + kamera + kontrolcü) sahneye kuran editör aracı.
// GrayboxKurucu ile aynı mantık: sahne menüden baştan kurulabilsin,
// hiçbir şey "bir zamanlar elle yapılmıştı" durumunda kalmasın.

using UnityEngine;
using UnityEditor;

public static class OyuncuKurucu
{
    // Kapıcının ölçüleri. Göz hizası, gerçek bir yetişkinin ~1.65 m'sidir.
    const float BoyUzunlugu  = 1.75f;
    const float GozHizasi    = 1.65f;
    const float GovdeYaricap = 0.3f;

    // Basamak yüksekliği 0.1875 m. Tırmanma eşiği bunun üstünde olmalı,
    // yoksa karakter merdivene takılır.
    const float BasamakEsigi = 0.35f;

    [MenuItem("Huzur/Oyuncuyu Yeniden Kur")]
    public static void YenidenKur()
    {
        // Eski oyuncu varsa sil; iki kopya olmasın.
        GameObject eski = GameObject.Find("Oyuncu");
        if (eski != null)
        {
            // Kamerayı kaybetmeyelim: silmeden önce dışarı çıkarırız.
            Camera ickamera = eski.GetComponentInChildren<Camera>();
            if (ickamera != null) ickamera.transform.SetParent(null, true);

            Object.DestroyImmediate(eski);
        }

        // ---- GÖVDE ----
        GameObject oyuncu = new GameObject("Oyuncu");

        // Oyuncu, 2. kolun tepesinden sahanlığa yeni adım atmış gibi başlar:
        // her kata tam buradan varılıyor, 1. kat da diğer yedi kat gibi görünsün.
        // 20 cm yukarıda konuyor ki döşemenin içine gömülmesin; ilk karede yere düşer.
        oyuncu.transform.position = new Vector3(MekanOlculeri.OyuncuBaslangicX, 0.2f,
                                                MekanOlculeri.OyuncuBaslangicZ);

        // Yüzü sahanlığa dönük (-z yönü). Sahanlık merdivenin -z tarafında;
        // Euler(0, 180, 0) oyuncuyu dikey eksen etrafında yarım tur çevirir.
        oyuncu.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // CharacterController: Unity'nin hazır yürüyüş bileşeni.
        // Fizik motorunun tam simülasyonunu kullanmaz; bunun yerine
        // "kapsül şeklinde bir hacmi duvarlara çarpmadan taşı" işini yapar.
        // Yürüyen karakterler için rigidbody'den çok daha öngörülebilirdir.
        CharacterController cc = oyuncu.AddComponent<CharacterController>();
        cc.height = BoyUzunlugu;
        cc.radius = GovdeYaricap;

        // center: kapsülün merkezi. Ayaklar y=0'da olsun diye boyun yarısı kadar yukarı.
        cc.center = new Vector3(0f, BoyUzunlugu / 2f, 0f);

        cc.stepOffset = BasamakEsigi;   // bu yükseklikteki engele kendiliğinden tırmanır
        cc.slopeLimit = 50f;            // bu açıdan dik yamaçlara çıkamaz
        cc.skinWidth = 0.02f;           // çarpışma toleransı; çok küçük olursa duvara girer

        // ---- KAMERA ----
        // Sahnedeki mevcut kamerayı kullan; yoksa yenisini kur.
        Camera kamera = Camera.main;
        if (kamera == null)
        {
            GameObject kamNesne = new GameObject("Kamera");
            kamera = kamNesne.AddComponent<Camera>();
            kamNesne.AddComponent<AudioListener>();
            kamNesne.tag = "MainCamera";
        }

        kamera.gameObject.name = "Kamera";

        // Kamerayı oyuncunun çocuğu yap. false = yerel konumu koru,
        // böylece aşağıdaki değerler doğrudan geçerli olur.
        kamera.transform.SetParent(oyuncu.transform, false);
        kamera.transform.localPosition = new Vector3(0f, GozHizasi, 0f);
        kamera.transform.localRotation = Quaternion.identity;   // identity = hiç dönmemiş

        // Dar bir merdiven boşluğunda geniş açı mide bulandırır; 60 derece rahat.
        kamera.fieldOfView = 60f;
        kamera.nearClipPlane = 0.05f;   // buruna çok yakın nesneler kesilmesin

        // ---- KAFA LAMBASI (#51) ----
        // Kameranın çocuğu bir spot: kamera nereye bakarsa huzme oraya. Bina artık karanlık,
        // ışığı bu lamba taşır. Ayarları KafaLambasi'nde sabit; bileşen açma-kapama, titreme
        // ve gölge kalitesini yönetir. Yeniden kurulumda eskisi silinir, çoğalmasın.
        Transform eskiLamba = kamera.transform.Find("Kafa_Lambasi");
        if (eskiLamba != null) Object.DestroyImmediate(eskiLamba.gameObject);
        GameObject lambaNesne = new GameObject("Kafa_Lambasi");
        lambaNesne.transform.SetParent(kamera.transform, false);
        lambaNesne.transform.localPosition = new Vector3(0f, 0.08f, 0.05f);   // alnın biraz üstü, biraz önü
        Light lamba = lambaNesne.AddComponent<Light>();
        lamba.type = LightType.Spot;
        lamba.spotAngle = KafaLambasi.DisAci;
        lamba.innerSpotAngle = KafaLambasi.IcAci;
        lamba.range = KafaLambasi.Menzil;
        lamba.intensity = KafaLambasi.Siddet;
        lamba.color = KafaLambasi.Renk;
        lamba.shadows = LightShadows.Soft;
        lamba.shadowStrength = 0.9f;
        lamba.shadowNearPlane = 0.1f;     // oyuncunun kendi gövdesi gölge atmasın
        if (kamera.GetComponent<KafaLambasi>() == null) kamera.gameObject.AddComponent<KafaLambasi>();

        // ---- KONTROLCÜ ----
        BirinciSahisKontrolcu kontrolcu = oyuncu.AddComponent<BirinciSahisKontrolcu>();

        // Scriptteki "kamera" alanı private olduğu için doğrudan atayamayız.
        // SerializedObject, Unity'nin Inspector'da kullandığı kapıyı bize açar:
        // private ama [SerializeField] işaretli alanlara kodla erişmeyi sağlar.
        SerializedObject seri = new SerializedObject(kontrolcu);
        seri.FindProperty("kamera").objectReferenceValue = kamera.transform;
        seri.ApplyModifiedProperties();   // değişikliği gerçekten yaz

        // ---- ETKİLEŞİM ----
        // Oyuncunun baktığı şeyle etkileşim kuran tek merkez. Poşeti de,
        // ileride gelecek zili veya notu da bu ele alacak.
        Etkilesimci etkilesimci = oyuncu.AddComponent<Etkilesimci>();

        SerializedObject seriEtkilesim = new SerializedObject(etkilesimci);
        seriEtkilesim.FindProperty("kamera").objectReferenceValue = kamera.transform;
        seriEtkilesim.FindProperty("panelMalzemesi").objectReferenceValue = IpucuPaneliMalzemesi();
        seriEtkilesim.ApplyModifiedProperties();

        // ---- ÇÖP SAYACI ----
        // Kaç poşet taşındığını tutar. Nişan alma işi Etkilesimci'de.
        oyuncu.AddComponent<CopToplayici>();

        // ---- AYAK SESİ (#25) ----
        // Oyuncunun kendi adımları. Ses kaynağını bileşen kendisi ekler.
        oyuncu.AddComponent<AyakSesi>();

        // ---- SONSUZ MERDİVEN ----
        // Oyuncuyu üst sahanlıktan alt sahanlığa sessizce taşıyan döngü.
        oyuncu.AddComponent<SonsuzMerdiven>();

        // ---- KAT YÖNETİCİSİ ----
        // Kat sayacı ile sahne arasındaki köprü. Ayrı bir nesnede durur,
        // çünkü oyuncuya değil oyunun kendisine aittir.
        //
        // Varsa DOKUNMUYORUZ. Eskiden silip yeniden kuruyorduk ve sahnede elle
        // ayarlanmış değerler (anomali olasılığı gibi) sessizce varsayılana dönüyordu.
        if (GameObject.Find("KatYoneticisi") == null)
        {
            GameObject yonetici = new GameObject("KatYoneticisi");
            yonetici.AddComponent<KatYoneticisi>();
        }

        // ---- ARAYÜZ: DURAKLATMA MENÜSÜ (#15) ----
        // Esc menüsü de oyuncuya değil oyuna ait; kat yöneticisi gibi kendi nesnesinde
        // durur ve varsa dokunulmaz.
        if (GameObject.Find("Arayuz") == null)
        {
            GameObject arayuz = new GameObject("Arayuz");
            arayuz.AddComponent<DuraklatmaMenusu>();
        }

        Selection.activeGameObject = oyuncu;   // Hierarchy'de seçili gelsin

        Debug.Log($"Oyuncu kuruldu. Boy: {BoyUzunlugu:0.00} m, göz hizası: {GozHizasi:0.00} m, " +
                  $"basamak eşiği: {BasamakEsigi:0.00} m");

        // Kurulan hâl bellekte kalıp sessizce kaybolmasın (nedeni GrayboxKurucu'da).
        GrayboxKurucu.SahneyiKaydet("Oyuncu");
    }

    // İpucu panelinin koyu, ışıktan etkilenmeyen malzemesi (#26). Dosya olarak
    // üretiliyor ki Etkilesimci'ye bağlansın ve build'e kesin girsin; varsa aynen kullanılır.
    // Unlit: ışık hesaplanmaz, renk her koşulda aynı görünür (karanlık sahanlıkta da okunur).
    private static Material IpucuPaneliMalzemesi()
    {
        const string Yol = "Assets/Materials/Ipucu_Panel.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m != null) return m;

        m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetColor("_BaseColor", new Color(0.04f, 0.04f, 0.05f));
        AssetDatabase.CreateAsset(m, Yol);
        return m;
    }
}
