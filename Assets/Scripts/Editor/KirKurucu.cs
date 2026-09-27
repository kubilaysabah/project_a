// KirKurucu.cs
// Terk edilmiş dairelerin kiri ve tozu (#78, Şahan: "daireler terkedilmiş olduğu için etrafın
// biraz pis ve toz olması lazım"). Dokular Art/Dokular/scripts/kir_uret.py ile üretilir.
//
// DETAY HARİTASI NEDİR
// URP Lit malzemesinin ana dokusunun (duvar kâğıdı, parke, kumaş) üstüne ikinci bir doku
// çarpılabilir: "Detail Albedo". Kural "x2 çarp": detay dokusunun 0,5 grisi hiçbir şeyi
// değiştirmez, koyusu kirletir, açığı soldurur. Böylece her duvar kâğıdını ve her mobilya
// dokusunu tek tek kirletmek yerine hepsine aynı kir katmanını ekleriz; detayın kendi tekrar
// sayısı olduğu için ana dokudan bağımsız döşenir.

using UnityEditor;
using UnityEngine;

public static class KirKurucu
{
    const string DokuKlasoru = "Assets/Textures/Kir/";
    const string MalzemeKlasoru = "Assets/Materials/";

    static Texture2D Doku(string ad, bool alfaSaydam)
    {
        string yol = DokuKlasoru + ad + ".png";
        var ti = AssetImporter.GetAtPath(yol) as TextureImporter;
        if (ti != null)
        {
            bool degisti = false;
            if (ti.wrapMode != TextureWrapMode.Repeat && !alfaSaydam) { ti.wrapMode = TextureWrapMode.Repeat; degisti = true; }
            if (alfaSaydam && ti.wrapMode != TextureWrapMode.Clamp) { ti.wrapMode = TextureWrapMode.Clamp; degisti = true; }
            if (ti.alphaIsTransparency != alfaSaydam) { ti.alphaIsTransparency = alfaSaydam; degisti = true; }
            // Küçültülmüş (mipmap) sürümlerde ince çizgiler solup alfa kesmenin altına düşer ve ağ
            // uzaktan kaybolur. "Örtüyü koru": her küçük sürümde kesilmeden kalan alan aynı kalsın.
            if (alfaSaydam && !ti.mipMapsPreserveCoverage) { ti.mipMapsPreserveCoverage = true; ti.alphaTestReferenceValue = 0.35f; degisti = true; }
            if (degisti) ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(yol);
    }

    /// <summary>
    /// Malzemeye kir detayını takar. tekrar: detay dokusunun malzemenin UV'si içinde kaç kez
    /// döşeneceği (daire kaplamalarında UV metre cinsinden: 0,4 = 2,5 metrede bir).
    /// </summary>
    public static void KirEkle(Material m, float tekrar, float guc = 1f)
    {
        Texture2D kir = Doku("Kir_Detay", false);
        if (m == null || kir == null) return;
        m.SetTexture("_DetailAlbedoMap", kir);
        m.SetTextureScale("_DetailAlbedoMap", Vector2.one * tekrar);
        m.SetFloat("_DetailAlbedoMapScale", guc);
        // URP Lit'e "detay haritası var" demenin yolu. URP'nin kuralı: güç tam 1 ise _DETAIL_MULX2,
        // değilse _DETAIL_SCALED. Yanlışını yazarsak Unity malzemeyi sonradan kendisi düzeltir ve
        // dosya her kurulumda boşuna değişir.
        bool olcekli = !Mathf.Approximately(guc, 1f);
        if (olcekli) { m.DisableKeyword("_DETAIL_MULX2"); m.EnableKeyword("_DETAIL_SCALED"); }
        else { m.EnableKeyword("_DETAIL_MULX2"); m.DisableKeyword("_DETAIL_SCALED"); }
        EditorUtility.SetDirty(m);
    }

    /// <summary>Örümcek ağı malzemesi: alfa kesme (saydam yerler hiç çizilmez), iki yüzlü.</summary>
    public static Material OrumcekAgiMalzemesi()
    {
        string yol = MalzemeKlasoru + "Orumcek_Agi.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, yol);
        }
        m.SetTexture("_BaseMap", Doku("Orumcek_Agi", true));
        m.SetColor("_BaseColor", new Color(0.75f, 0.75f, 0.73f, 1f));
        m.SetFloat("_AlphaClip", 1f);           // alfa kesme açık
        m.SetFloat("_Cutoff", 0.35f);           // bu alfanın altı çizilmez
        m.EnableKeyword("_ALPHATEST_ON");
        m.SetFloat("_Cull", 0f);                // iki yüz de çizilsin (ağın arkasından da görünür)
        m.SetFloat("_Smoothness", 0.2f);
        m.SetOverrideTag("RenderType", "TransparentCutout");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        EditorUtility.SetDirty(m);
        return m;
    }

    /// <summary>
    /// Havadaki toz zerrelerinin malzemesi. "Particles/Simple Lit": ışık alan parçacık; kafa lambası
    /// ya da bir ampul değmedikçe görünmez, yani zerreler ancak ışık huzmesinin içinde parlar.
    /// </summary>
    public static Material TozMalzemesi()
    {
        string yol = MalzemeKlasoru + "Toz_Zerresi.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        Shader s = Shader.Find("Universal Render Pipeline/Particles/Simple Lit");
        if (m == null)
        {
            m = new Material(s);
            AssetDatabase.CreateAsset(m, yol);
        }
        m.shader = s;
        m.SetTexture("_BaseMap", Doku("Toz_Zerresi", true));
        m.SetColor("_BaseColor", new Color(1f, 0.97f, 0.9f, 0.55f));
        // Saydam, yumuşak kenarlı: Surface = Transparent, Blend = Alpha.
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---------- HAVADAKİ TOZ ----------
    // Oyuncunun kamerasının çocuğu olan bir parçacık sistemi. Parçacıklar kameranın önündeki bir
    // kutunun içinde doğar ama DÜNYADA kalır (simulationSpace = World): oyuncu yürüdükçe toz
    // havada asılı durur, oyuncu içinden geçer. Işık alan malzeme sayesinde yalnızca kafa
    // lambasının huzmesinde ya da bir ampulün altında görünür.
    [MenuItem("Huzur/Toz Zerrelerini Kur")]
    public static void TozZerreleriniKur()
    {
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>(FindObjectsInactive.Include);
        if (oyuncu == null || oyuncu.transform.childCount == 0) { Debug.LogWarning("[KIR] Oyuncu ya da kamerası yok."); return; }
        Transform kamera = oyuncu.transform.GetChild(0);
        Transform eski = kamera.Find("Toz_Zerreleri");
        if (eski != null) Object.DestroyImmediate(eski.gameObject);

        GameObject g = new GameObject("Toz_Zerreleri");
        g.transform.SetParent(kamera, false);
        g.transform.localPosition = new Vector3(0f, 0f, 1.8f);   // kutunun ortası: gözün 1,8 m önü
        ParticleSystem ps = g.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // "main": bütün parçacıkların ortak ayarları. MinMaxCurve(a, b): a ile b arasında rastgele.
        var ana = ps.main;
        ana.loop = true;
        ana.prewarm = true;                                   // oyun başında hava zaten tozlu olsun
        ana.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
        ana.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.03f);
        ana.startSize = new ParticleSystem.MinMaxCurve(0.006f, 0.016f);
        ana.startColor = new Color(1f, 0.97f, 0.9f, 0.7f);
        ana.gravityModifier = 0.002f;                         // çok yavaş çöker
        ana.simulationSpace = ParticleSystemSimulationSpace.World;
        ana.maxParticles = 450;

        var yayim = ps.emission;
        yayim.rateOverTime = 36f;

        var sekil = ps.shape;
        sekil.shapeType = ParticleSystemShapeType.Box;
        sekil.scale = new Vector3(4.5f, 2.4f, 3.6f);          // en, boy, derinlik (metre)

        // Gürültü: havadaki hafif akıntı; zerreler düz çizgide gitmesin, süzülsün.
        var gurultu = ps.noise;
        gurultu.enabled = true;
        gurultu.strength = 0.04f;
        gurultu.frequency = 0.25f;
        gurultu.scrollSpeed = 0.05f;

        var cizici = g.GetComponent<ParticleSystemRenderer>();
        cizici.sharedMaterial = TozMalzemesi();
        cizici.renderMode = ParticleSystemRenderMode.Billboard;
        cizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cizici.receiveShadows = false;

        GrayboxKurucu.SahneyiKaydet("Toz");
        Debug.Log("[KIR] Havadaki toz kuruldu.");
    }
}
