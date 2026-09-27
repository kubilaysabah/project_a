// MobilyaKurucu.cs
// Poly Haven mobilyalarından (Assets/Models/Mobilya, mobilya_donustur.py üretir) oyunda
// kullanılacak prefab'ları kurar (#73). Menü: Huzur → Mobilyaları Kur.
//
// PREFAB NEDİR
// Prefab, bir nesnenin bütün parçaları, bileşenleri ve ayarlarıyla birlikte kaydedilmiş bir
// "kalıbı"dır. Kat dosyaları (KatZinciriVarligi.suslar) bu kalıbı gösterir; kat açılınca
// ZincirUygulayici kalıptan bir kopya çıkarıp daireye koyar. Kalıp bir kez doğru kurulursa her
// kopya doğru gelir.
//
// BU ARAÇ NE YAPAR (her model için)
//   1) Dokuların içe aktarma ayarı: "_normal" normal haritası, "_ms" (metal + pürüzsüzlük)
//      doğrusal veri (renk dönüşümü yapılmaz), "_renk" renkli resim. En büyük boy 1024.
//   2) Malzemeler: FBX'teki her malzeme adı ("sofa_02__0") için Assets/Materials/Mobilya/
//      altında bir URP Lit malzeme kurulur ve FBX'in o malzemesi ona YÖNLENDİRİLİR (remap).
//      Böylece modeli kim kullanırsa kullansın bizim malzememizi görür.
//      Camlar ortak koyu cam malzemesini, avize ampulleri sönük bir malzemeyi alır (bina elektriksiz).
//   3) Prefab: gövdeye mesh çarpıştırıcı (MeshCollider) takılır. Kutu çarpıştırıcı daha ucuzdur
//      ama koltuğun altındaki boşluğu da kapatırdı; oyuncu eğilip altına bakamazdı (#79).
//      Çekmeceler öne kayar, kapaklar menteşesinden döner (AcilirParca); gövdeye bakınca da
//      parça seçilir (Yonlendirici). Avizeye çarpıştırıcı konmaz: tavanda, nişanı kapatmasın.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MobilyaKurucu
{
    const string ModelKlasoru = "Assets/Models/Mobilya/";
    const string DokuKlasoru = "Assets/Models/Mobilya/Dokular/";
    const string MalzemeKlasoru = "Assets/Materials/Mobilya/";
    public const string PrefabKlasoru = "Assets/Prefabs/Mobilya/";

    // Çarpıştırıcı konmayacak modeller: tavandan sarkan avizeler.
    static readonly HashSet<string> CarpistiriciYok = new HashSet<string> { "Chandelier_01", "Chandelier_02" };

    [MenuItem("Huzur/Mobilyaları Kur")]
    public static void Kur()
    {
        Directory.CreateDirectory(MalzemeKlasoru);
        Directory.CreateDirectory(PrefabKlasoru);
        DokulariAyarla();

        Material cam = OrtakMalzeme("Mob_Cam", m =>
        {
            // Yarı saydam koyu cam: URP Lit'in "Surface Type = Transparent" ayarı.
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetColor("_BaseColor", new Color(0.05f, 0.06f, 0.06f, 0.35f));
            m.SetFloat("_Smoothness", 0.9f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            // URP Lit, "speküler ışığı koru" açıkken kaynağı One ile karıştırır (renk alfayla önceden
            // çarpılır). SrcAlpha yazsaydık Unity malzemeyi sonradan kendisi düzeltir, dosya boşuna değişirdi.
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        });

        int sayi = 0;
        foreach (string yol in Directory.GetFiles(ModelKlasoru, "*.fbx"))
        {
            string kimlik = Path.GetFileNameWithoutExtension(yol);
            string varlikYolu = ModelKlasoru + kimlik + ".fbx";
            MalzemeleriBagla(varlikYolu, cam);
            PrefabKur(kimlik, varlikYolu);
            sayi++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[MOBILYA] {sayi} mobilya prefab'ı kuruldu: {PrefabKlasoru}");
    }

    // ---------- DOKULAR ----------
    static void DokulariAyarla()
    {
        foreach (string yol in Directory.GetFiles(DokuKlasoru, "*.png"))
        {
            string v = yol.Replace('\\', '/');
            var ti = AssetImporter.GetAtPath(v) as TextureImporter;
            if (ti == null) continue;
            bool degisti = false;
            bool normal = v.EndsWith("_normal.png"), ms = v.EndsWith("_ms.png");
            TextureImporterType tur = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (ti.textureType != tur) { ti.textureType = tur; degisti = true; }
            // sRGB: renkli resimlerde açık; veri taşıyan (metal, pürüzsüzlük) haritada kapalı.
            bool srgb = !normal && !ms;
            if (ti.sRGBTexture != srgb) { ti.sRGBTexture = srgb; degisti = true; }
            if (ti.maxTextureSize != 1024) { ti.maxTextureSize = 1024; degisti = true; }
            if (degisti) ti.SaveAndReimport();
        }
    }

    static Texture2D Doku(string ad)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(DokuKlasoru + ad + ".png");
    }

    static Material OrtakMalzeme(string ad, System.Action<Material> ayarla)
    {
        string yol = MalzemeKlasoru + ad + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, yol);
        }
        ayarla(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---------- MALZEMELER ----------
    // FBX'in içindeki malzemelerin adlarını okur, her biri için bizim malzememizi kurar ve
    // içe aktarıcıya "bu adı gördüğünde şunu kullan" der (AddRemap).
    static void MalzemeleriBagla(string fbxYolu, Material cam)
    {
        var mi = AssetImporter.GetAtPath(fbxYolu) as ModelImporter;
        if (mi == null) return;
        mi.importCameras = false;
        mi.importLights = false;
        mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

        var adlar = new List<string>();
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(fbxYolu))
            if (o is Material && !adlar.Contains(o.name)) adlar.Add(o.name);
        // Daha önce yönlendirilmişse içe aktarılan malzeme görünmez; eski yönlendirmelerden okuruz.
        foreach (var kv in mi.GetExternalObjectMap())
            if (kv.Key.type == typeof(Material) && !adlar.Contains(kv.Key.name)) adlar.Add(kv.Key.name);

        foreach (string ad in adlar)
        {
            Material m;
            if (ad.EndsWith("_cam")) m = cam;
            else
            {
                bool ampul = ad.EndsWith("_ampul");
                m = OrtakMalzeme(ad, x =>
                {
                    x.SetTexture("_BaseMap", Doku(ad + "_renk"));
                    // Toz (#78): renkler hafifçe grileşir; ampul ayrıca sönük (bina elektriksiz).
                    x.SetColor("_BaseColor", ampul ? new Color(0.35f, 0.33f, 0.3f) : new Color(0.86f, 0.84f, 0.8f));
                    Texture2D n = Doku(ad + "_normal");
                    x.SetTexture("_BumpMap", n);
                    if (n != null) x.EnableKeyword("_NORMALMAP"); else x.DisableKeyword("_NORMALMAP");
                    Texture2D ms = Doku(ad + "_ms");
                    x.SetTexture("_MetallicGlossMap", ms);
                    if (ms != null) x.EnableKeyword("_METALLICSPECGLOSSMAP"); else x.DisableKeyword("_METALLICSPECGLOSSMAP");
                    // ms haritası varken bu değer çarpandır (haritanın A kanalı asıl değer). 0,55: tozlu
                    // yüzey matlaşır, cilalı ahşap ve deri eskisi kadar parlamaz (#78).
                    x.SetFloat("_Smoothness", 0.55f);
                    KirKurucu.KirEkle(x, 3f, 0.9f);
                });
            }
            mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), ad), m);
        }
        mi.SaveAndReimport();
    }

    // ---------- PREFAB ----------
    static void PrefabKur(string kimlik, string fbxYolu)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxYolu);
        if (model == null) { Debug.LogWarning($"[MOBILYA] {fbxYolu} yüklenemedi."); return; }

        // Kopya çıkar ve FBX bağlantısını çöz: prefab kendi başına bir nesne olsun.
        GameObject kok = (GameObject)PrefabUtility.InstantiatePrefab(model);
        PrefabUtility.UnpackPrefabInstance(kok, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        kok.name = kimlik;

        Transform govde = kok.transform.Find("Govde");
        var parcalar = new List<MonoBehaviour>();
        foreach (Transform t in kok.transform)
        {
            MeshFilter mf = t.GetComponent<MeshFilter>();
            if (mf == null) continue;
            bool cekmece = t.name.StartsWith("Cekmece"), kapak = t.name.StartsWith("Kapak");
            if (cekmece || kapak)
            {
                // Parça gövdenin çocuğu olsun: gövde döndürülünce parça da onunla döner.
                if (govde != null) t.SetParent(govde, true);
                t.gameObject.AddComponent<BoxCollider>();   // nişan için küçük, ucuz kutu
                AcilirParca a = t.gameObject.AddComponent<AcilirParca>();
                Bounds b = mf.sharedMesh.bounds;   // parçanın kendi (yerel) sınırı
                var seri = new SerializedObject(a);
                if (cekmece)
                {
                    // Çekmece önüne (+Z) derinliğinin %60'ı kadar kayar.
                    seri.FindProperty("hareket").enumValueIndex = (int)AcilirParca.Hareket.Kaydir;
                    seri.FindProperty("eksen").vector3Value = Vector3.forward;
                    seri.FindProperty("miktar").floatValue = b.size.z * 0.6f;
                }
                else
                {
                    // Kapak menteşesinde döner. Unity'de +Y etrafında pozitif dönüş +X'i -Z'ye çevirir;
                    // kapağın öne (+Z) açılması için +X'e uzanan kapak eksi, -X'e uzanan artı döner.
                    seri.FindProperty("hareket").enumValueIndex = (int)AcilirParca.Hareket.Dondur;
                    seri.FindProperty("eksen").vector3Value = Vector3.up;
                    seri.FindProperty("miktar").floatValue = b.center.x > 0f ? -100f : 100f;
                }
                seri.FindProperty("sure").floatValue = 0.5f;
                seri.ApplyModifiedPropertiesWithoutUndo();
                parcalar.Add(a);
            }
            else if (!CarpistiriciYok.Contains(kimlik))
            {
                MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        if (govde != null && parcalar.Count > 0)
        {
            Yonlendirici y = govde.gameObject.AddComponent<Yonlendirici>();
            var seri = new SerializedObject(y);
            SerializedProperty dizi = seri.FindProperty("parcalar");
            dizi.arraySize = parcalar.Count;
            for (int i = 0; i < parcalar.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = parcalar[i];
            seri.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(kok, PrefabKlasoru + kimlik + ".prefab");
        Object.DestroyImmediate(kok);
    }
}
