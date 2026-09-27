// FotografKurucu.cs
// Aile fotoğraflarının (#69) albümünü ve çerçeve prefab'larını kurar.
//
//   Assets/Fotograflar/Album.asset         : bütün fotoğraflar (kimlik, resim, arkasındaki metin)
//   Assets/Prefabs/Mobilya/Aile_Fotografi_Masa.prefab  : masaya konan dik çerçeve (dik fotoğraflar)
//   Assets/Prefabs/Mobilya/Aile_Fotografi_Duvar.prefab : duvara asılan çerçeve (yatay fotoğraflar)
//
// Çerçeveler Poly Haven mobilya prefab'larının kopyası (MobilyaKurucu); köklerine AileFotografi
// eklenir ve albüm bağlanır. Hangi fotoğrafı taşıyacaklarını kat dosyası söyler (KatSusu.Metin).
// Resimler foto_isle.py'den gelir: Assets/Textures/Fotograflar/<kimlik>.png.

using System.IO;
using UnityEditor;
using UnityEngine;

public static class FotografKurucu
{
    const string AlbumYolu = "Assets/Fotograflar/Album.asset";
    const string DokuKlasoru = "Assets/Textures/Fotograflar/";

    // Kimlik ve arkasındaki metnin anahtarı. Anahtarlar burada açıkça yazılı: metin bütünlük testi
    // koddaki anahtarları bu yazılardan bulur.
    static readonly string[,] Fotograflar =
    {
        { "kat1", "foto.kat1" }, { "kat2", "foto.kat2" }, { "kat3", "foto.kat3" }, { "kat4", "foto.kat4" },
        { "kat5", "foto.kat5" }, { "kat6", "foto.kat6" }, { "kat7", "foto.kat7" }, { "kat8", "foto.kat8" },
        { "kapici", "foto.kapici" },
    };

    /// <summary>Albümü kurar (resmi olmayan fotoğraf atlanır) ve döndürür.</summary>
    public static FotografAlbumu AlbumKur()
    {
        Directory.CreateDirectory("Assets/Fotograflar");
        FotografAlbumu album = AssetDatabase.LoadAssetAtPath<FotografAlbumu>(AlbumYolu);
        if (album == null)
        {
            album = ScriptableObject.CreateInstance<FotografAlbumu>();
            AssetDatabase.CreateAsset(album, AlbumYolu);
        }
        album.fotograflar.Clear();
        for (int i = 0; i < Fotograflar.GetLength(0); i++)
        {
            string yol = DokuKlasoru + Fotograflar[i, 0] + ".png";
            var ti = AssetImporter.GetAtPath(yol) as TextureImporter;
            if (ti == null) continue;
            // npotScale = None: Unity iki kuvveti olmayan boyutları (1024x796) normalde en yakın ikinin
            // kuvvetine (1024x1024) esnetir; o zaman yakın bakışta fotoğraf kare görünürdü.
            if (ti.maxTextureSize != 1024 || ti.wrapMode != TextureWrapMode.Clamp || ti.npotScale != TextureImporterNPOTScale.None)
            {
                ti.maxTextureSize = 1024;
                ti.wrapMode = TextureWrapMode.Clamp;   // kenarlar tekrarlanmasın
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.SaveAndReimport();
            }
            album.fotograflar.Add(new FotografAlbumu.Fotograf
            {
                Kimlik = Fotograflar[i, 0],
                Doku = AssetDatabase.LoadAssetAtPath<Texture2D>(yol),
                Arka = Fotograflar[i, 1],
            });
        }
        EditorUtility.SetDirty(album);
        AssetDatabase.SaveAssets();
        return album;
    }

    /// <summary>Çerçeve prefab'ı: duvar = yatay (Poly Haven fancy frame), değilse masa (standing frame).</summary>
    public static GameObject Sablon(bool duvar)
    {
        string kaynak = MobilyaKurucu.PrefabKlasoru + (duvar ? "fancy_picture_frame_01" : "standing_picture_frame_01") + ".prefab";
        string hedef = MobilyaKurucu.PrefabKlasoru + (duvar ? "Aile_Fotografi_Duvar" : "Aile_Fotografi_Masa") + ".prefab";
        FotografAlbumu album = AlbumKur();
        GameObject k = AssetDatabase.LoadAssetAtPath<GameObject>(kaynak);
        if (k == null) { Debug.LogWarning($"[FOTO] {kaynak} yok; önce Huzur → Mobilyaları Kur."); return null; }
        // LoadPrefabContents: prefab'ı sahneye koymadan, bellekte açıp düzenlemenin yolu.
        GameObject ic = PrefabUtility.LoadPrefabContents(kaynak);
        AileFotografi af = ic.GetComponent<AileFotografi>();
        if (af == null) af = ic.AddComponent<AileFotografi>();
        var seri = new SerializedObject(af);
        seri.FindProperty("album").objectReferenceValue = album;
        seri.ApplyModifiedPropertiesWithoutUndo();
        GameObject sonuc = PrefabUtility.SaveAsPrefabAsset(ic, hedef);
        PrefabUtility.UnloadPrefabContents(ic);
        return sonuc;
    }
}
