// FigurKurucu.cs
// Üç harfli figürlerinin prefab'ları (#134): Assets/Models/Figurler/Cin_N/Cin_N.fbx dosyalarından çarpışmasız
// prefab'lar üretir ve sahnedeki CinOlaylari'nın "figurler" dizisine bağlar. Menü: Huzur → Cin Figürlerini Kur.
//
// NEDEN PREFAB: CinOlaylari figürü oyun sırasında Instantiate ile yaratır; build'de nesneler adıyla aranmaz,
// REFERANSLA bulunur (#26 kuralı). Prefab, FBX'in kendisi değil: FBX'in kökü bir seviye ekler ve içe aktarma
// ayarları değişince prefab bozulmasın diye kopyasını alırız.
//
// Çarpışma yok: figür görüş dışına kaçar, oyuncuya dokunmaz; bir kapsül eklenseydi oyuncunun yolunu keserdi.
// Dokular FBX'in yanındaki dosyalardan ImportViaMaterialDescription ile gelir (MobilyaKurucu'daki ayar).
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FigurKurucu
{
    const string ModelKlasoru = "Assets/Models/Figurler";
    const string PrefabKlasoru = "Assets/Prefabs/Figurler";

    [MenuItem("Huzur/Cin Figürlerini Kur")]
    public static void Kur()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(PrefabKlasoru)) AssetDatabase.CreateFolder("Assets/Prefabs", "Figurler");

        var prefablar = new List<GameObject>();
        for (int n = 1; n <= 6; n++)
        {
            string fbx = $"{ModelKlasoru}/Cin_{n}/Cin_{n}.fbx";
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (model == null) { Debug.Log($"[FIGUR] {fbx} yok, atlandı."); continue; }

            // İçe aktarma: malzemeler dokularıyla, animasyon yok (sabit mesh), ölçek 1 (Blender metre yazdı).
            var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
            if (mi != null && (mi.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription || mi.importAnimation))
            {
                mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                mi.importAnimation = false;
                mi.animationType = ModelImporterAnimationType.None;
                mi.SaveAndReimport();
                model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            }

            GameObject gecici = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(gecici, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            gecici.name = $"Cin_{n}";
            foreach (Collider c in gecici.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (Renderer r in gecici.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;   // fenerde duvara gölgesi düşsün
            }
            string yol = $"{PrefabKlasoru}/Cin_{n}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gecici, yol);
            Object.DestroyImmediate(gecici);
            prefablar.Add(prefab);
        }

        CinOlaylari cin = Object.FindAnyObjectByType<CinOlaylari>(FindObjectsInactive.Include);
        if (cin == null) { Debug.LogWarning("[FIGUR] Sahnede CinOlaylari yok; prefab'lar bağlanmadı."); return; }
        var seri = new SerializedObject(cin);
        SerializedProperty dizi = seri.FindProperty("figurler");
        dizi.arraySize = prefablar.Count;
        for (int i = 0; i < prefablar.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = prefablar[i];
        seri.ApplyModifiedPropertiesWithoutUndo();
        GrayboxKurucu.SahneyiKaydet("FigurKurucu");
        Debug.Log($"[FIGUR] {prefablar.Count} cin figürü prefab'ı kuruldu ve CinOlaylari'na bağlandı.");
    }
}
