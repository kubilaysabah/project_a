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
//
// GÖZLER (#135): her prefab'a GozParlamasi eklenir. Göz yerleri modelin köşe noktalarından ölçülür: figürün
// toplam boyunun %88-95'i arasındaki band baştır; bandın en öndeki (+z) noktası yüzdür; gözler o derinlikte,
// boyun %91,5'inde, yüzün ortasından 3,1 cm sağda ve solda. Ölçüm bir figürde yanlış çıkarsa (kukuleta,
// öne düşen saç) GozDuzeltme tablosuna elle ölçülmüş değer yazılır; tablo ölçümü ezer.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FigurKurucu
{
    const string ModelKlasoru = "Assets/Models/Figurler";
    const string PrefabKlasoru = "Assets/Prefabs/Figurler";
    const string GozMalzemesi = "Assets/Materials/Kat_Isik_Kaynagi.mat";   // URP Unlit (#116'nın kaynağı)
    const float GozBandAlt = 0.88f, GozBandUst = 0.95f;   // başın bandı (boyun oranı)
    const float GozYukseklik = 0.915f;                     // göz hizası (boyun oranı)
    const float GozAraligi = 0.031f;                       // yüz ortasından her göze (metre)
    const float GozOnPayi = 0.004f;                        // yüzün 4 mm önünde (yüzeyin içinde kalmasın)
    const float BasYariGenislik = 0.12f;                   // #141 (B6): baş ortasından her yana bakılan genişlik (m)

    // Elle düzeltme: figür numarası → (sol göz, sağ göz), figürün kendi eksenlerinde. Boş: ölçüm kullanılır.
    static readonly System.Collections.Generic.Dictionary<int, (Vector3 sol, Vector3 sag)> GozDuzeltme =
        new System.Collections.Generic.Dictionary<int, (Vector3, Vector3)>();

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

            // #140: kök BOŞ bir nesne, ağ onun "Govde" çocuğunda. Hareket ettiriciler kökü taşır; FigurYurumesi
            // gövdeyi eğer ve iniş-kalkış verir (iskelet yok). SetParent(…, false): FBX'in kendi yerel konumu
            // ve açısı korunur (ikisi de sıfır: Blender dönüşümü ağa işledi).
            GameObject gecici = new GameObject($"Cin_{n}");
            GameObject govde = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(govde, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            govde.name = "Govde";
            govde.transform.SetParent(gecici.transform, false);
            foreach (Collider c in gecici.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (Renderer r in gecici.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;   // fenerde duvara gölgesi düşsün
            }
            // Gözler (#135): ölç (ya da tablodan al), bileşeni ekle, malzemeyi bağla.
            Vector3 sol, sag;
            if (GozDuzeltme.TryGetValue(n, out var duzeltme)) { sol = duzeltme.sol; sag = duzeltme.sag; }
            else if (!GozleriOlc(gecici, out sol, out sag)) Debug.LogWarning($"[FIGUR] Cin_{n}: göz yeri ölçülemedi.");
            var goz = gecici.GetComponent<GozParlamasi>();
            if (goz == null) goz = gecici.AddComponent<GozParlamasi>();
            goz.Kur(AssetDatabase.LoadAssetAtPath<Material>(GozMalzemesi), sol, sag);
            Debug.Log($"[FIGUR] Cin_{n} gözler: sol {sol:F3} sağ {sag:F3}");
            if (gecici.GetComponent<FigurYurumesi>() == null) gecici.AddComponent<FigurYurumesi>();   // #140
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

    // Göz yerlerini modelin köşe noktalarından ölç (kök eksenlerinde). Editor'de içe aktarılmış bir mesh'in
    // köşeleri okunabilir (oyun içinde Read/Write kapalı mesh'te okunamaz; bu yüzden ölçüm burada yapılır ve
    // sonuç prefab'a yazılır).
    static bool GozleriOlc(GameObject kok, out Vector3 sol, out Vector3 sag)
    {
        sol = sag = Vector3.zero;
        var noktalar = new System.Collections.Generic.List<Vector3>();
        float enUst = float.MinValue;
        foreach (MeshFilter mf in kok.GetComponentsInChildren<MeshFilter>())
        {
            Mesh m = mf.sharedMesh;
            if (m == null) continue;
            foreach (Vector3 v in m.vertices)
            {
                Vector3 d = kok.transform.InverseTransformPoint(mf.transform.TransformPoint(v));
                noktalar.Add(d);
                if (d.y > enUst) enUst = d.y;
            }
        }
        if (noktalar.Count == 0 || enUst <= 0f) return false;
        float alt = enUst * GozBandAlt, ust = enUst * GozBandUst;
        // #141 (B6): başın ortası, bandın x değerlerinin ORTANCASI (sıralanınca ortadaki değer). Ortalama değil:
        // bant hizasına kalkmış bir kol ortalamayı yana çeker, ortancayı pek oynatmaz. Sonra yalnız baş
        // genişliği içindeki (ortanca ±12 cm) noktalara bakılır; kol, el ya da kukuleta ucu yüz sanılmaz.
        var xler = new List<float>();
        foreach (Vector3 p in noktalar) if (p.y >= alt && p.y <= ust) xler.Add(p.x);
        if (xler.Count == 0) return false;
        xler.Sort();
        float basX = xler[xler.Count / 2];
        float enOn = float.MinValue;
        foreach (Vector3 p in noktalar)
            if (p.y >= alt && p.y <= ust && Mathf.Abs(p.x - basX) < BasYariGenislik && p.z > enOn) enOn = p.z;
        if (enOn == float.MinValue) return false;
        // Yüzün ortası: baş genişliği içinde, bandın en öndeki 3 cm'lik diliminde kalan noktaların x ortalaması.
        float xToplam = 0f; int adet = 0;
        foreach (Vector3 p in noktalar)
            if (p.y >= alt && p.y <= ust && Mathf.Abs(p.x - basX) < BasYariGenislik && p.z > enOn - 0.03f) { xToplam += p.x; adet++; }
        float xo = adet > 0 ? xToplam / adet : 0f;
        float y = enUst * GozYukseklik, z = enOn + GozOnPayi;
        sol = new Vector3(xo - GozAraligi, y, z);
        sag = new Vector3(xo + GozAraligi, y, z);
        return true;
    }
}
