// LekeKurucu.cs
// Duvar lekelerinin kurucusu (#87): URP'ye Decal özelliğini ekler, leke dokularından decal
// malzemeleri yapar ve her daireye DaireLekeleri bileşenini bağlar. Lekelerin yerleşimi kat
// açılınca çalışma zamanında yapılır (DaireLekeleri.Kur); burası yalnızca hazırlık.
//
// RENDERER FEATURE NEDİR: URP'nin çizim hattına eklenen bir adım. Decal'lar kendi başlarına
// çizilmez; "Decal" özelliği hatta ekli değilse DecalProjector bileşenleri sessizce hiçbir şey
// göstermez. Özellik PC_Renderer'a bir kez eklenir; Orta ve Düşük kalite de aynı renderer'ı
// kullanır (KaliteKurucu), bu yüzden üçü de alır. Düşük kalite lekeleri bileşen tarafında kapatır.
//
// Alt varlık kuralı (#31 dersi): renderer'a eklenen ScriptableObject dosyaya
// AssetDatabase.AddObjectToAsset ile yazılmazsa yalnızca bellekte kalır ve build'de yok olur.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class LekeKurucu
{
    const string RendererYolu = "Assets/Settings/PC_Renderer.asset";
    const string DokuKlasoru = "Assets/Textures/Lekeler/";
    const string MalzemeKlasoru = "Assets/Materials/Lekeler/";

    // DaireLekeleri.Tur sırasıyla doku adları.
    static readonly string[] Dokular = { "Leke_Su", "Leke_Rutubet", "Leke_Catlak", "Leke_Kabarma", "Leke_Is", "Leke_Sizinti" };

    [MenuItem("Huzur/Lekeleri Kur")]
    public static void Kur()
    {
        RendererHazirla();
        Material[] malzemeler = Malzemeler();
        int bagli = 0;
        foreach (DaireEksenleri e in Object.FindObjectsByType<DaireEksenleri>(FindObjectsInactive.Include))
        {
            if (e.GetComponentInParent<KatYoneticisi>() != null) continue;   // yalnızca sahnedeki daireler
            Bagla(e.gameObject, malzemeler);
            bagli++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[LEKE] Decal özelliği hazır, {malzemeler.Length} leke malzemesi, {bagli} daireye bağlandı.");
    }

    /// <summary>Daireye DaireLekeleri bileşenini ekler ve malzemeleri bağlar (DaireleriKur de çağırır).</summary>
    public static void Bagla(GameObject daire, Material[] malzemeler = null)
    {
        if (malzemeler == null) malzemeler = Malzemeler();
        DaireLekeleri l = daire.GetComponent<DaireLekeleri>();
        if (l == null) l = daire.AddComponent<DaireLekeleri>();
        var seri = new SerializedObject(l);
        SerializedProperty dizi = seri.FindProperty("malzemeler");
        dizi.arraySize = malzemeler.Length;
        for (int i = 0; i < malzemeler.Length; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = malzemeler[i];
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    // PC_Renderer'a Decal özelliği (bir kez). URP'nin kendi editörünün yaptığı iş: özellik nesnesi
    // renderer dosyasının içine alt varlık olarak eklenir, listeye ve kimlik haritasına yazılır.
    public static void RendererHazirla()
    {
        var veri = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererYolu);
        if (veri == null) { Debug.LogWarning("[LEKE] PC_Renderer.asset bulunamadı."); return; }
        foreach (ScriptableRendererFeature f in veri.rendererFeatures)
            if (f is DecalRendererFeature) return;   // zaten var

        var decal = ScriptableObject.CreateInstance<DecalRendererFeature>();
        decal.name = "Decal";
        AssetDatabase.AddObjectToAsset(decal, veri);

        var seri = new SerializedObject(veri);
        SerializedProperty liste = seri.FindProperty("m_RendererFeatures");
        liste.arraySize++;
        liste.GetArrayElementAtIndex(liste.arraySize - 1).objectReferenceValue = decal;
        // Kimlik haritası: URP her özelliğin yerel dosya kimliğini tutar (eksik özellik ayıklaması için).
        SerializedProperty harita = seri.FindProperty("m_RendererFeatureMap");
        harita.arraySize++;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(decal, out string _, out long yerelKimlik);
        harita.GetArrayElementAtIndex(harita.arraySize - 1).longValue = yerelKimlik;
        seri.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(veri);
        AssetDatabase.SaveAssets();
        Debug.Log("[LEKE] PC_Renderer'a Decal özelliği eklendi.");
    }

    // Islak ayak izi malzemeleri (#137): sol ve sağ ayak (decal aynalanamaz, iki doku). Dokular
    // Art/Dokular/scripts/ayak_izi_uret.py ile üretilir; CinOlaylari'na GrayboxKurucu_Zincir bağlar.
    public static Material[] AyakIziMalzemeleri()
    {
        if (!AssetDatabase.IsValidFolder(MalzemeKlasoru.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/Materials", "Lekeler");
        Shader sh = Shader.Find("Shader Graphs/Decal");
        string[] adlar = { "Leke_AyakIzi_Sol", "Leke_AyakIzi_Sag" };
        var sonuc = new Material[adlar.Length];
        for (int i = 0; i < adlar.Length; i++)
        {
            string yol = MalzemeKlasoru + adlar[i] + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, yol); }
            Texture2D doku = AssetDatabase.LoadAssetAtPath<Texture2D>(DokuKlasoru + adlar[i] + ".png");
            if (doku == null) Debug.LogWarning($"[LEKE] {adlar[i]}.png yok (Art/Dokular/scripts/ayak_izi_uret.py).");
            m.SetTexture("Base_Map", doku);
            m.SetFloat("Normal_Blend", 0f);
            EditorUtility.SetDirty(m);
            sonuc[i] = m;
        }
        AssetDatabase.SaveAssets();
        return sonuc;
    }

    // Her leke dokusundan bir decal malzemesi (URP'nin "Shader Graphs/Decal" gölgelendiricisi).
    static Material[] Malzemeler()
    {
        if (!AssetDatabase.IsValidFolder(MalzemeKlasoru.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/Materials", "Lekeler");
        Shader sh = Shader.Find("Shader Graphs/Decal");
        var sonuc = new Material[Dokular.Length];
        for (int i = 0; i < Dokular.Length; i++)
        {
            string yol = MalzemeKlasoru + Dokular[i] + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (m == null)
            {
                m = new Material(sh);
                AssetDatabase.CreateAsset(m, yol);
            }
            Texture2D doku = AssetDatabase.LoadAssetAtPath<Texture2D>(DokuKlasoru + Dokular[i] + ".png");
            if (doku == null) Debug.LogWarning($"[LEKE] {Dokular[i]}.png yok (Art/Dokular/scripts/leke_uret.py).");
            m.SetTexture("Base_Map", doku);
            m.SetFloat("Normal_Blend", 0f);     // lekenin normali yok; yüzeyin kendi normali kalsın
            EditorUtility.SetDirty(m);
            sonuc[i] = m;
        }
        return sonuc;
    }
}
