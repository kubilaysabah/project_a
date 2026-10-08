// EsyaEskitici.cs
// Blender'da yapılan eşyaları içe aktarılırken eskitir: renkleri soldurur, sarartır, parlaklığını alır ve
// üstüne kir dokusu serer (Şahan, 2026-10-01: "modern duran eşyaların yerine 90'lara ait eski püskü
// versiyonlarını koy").
//
// NEDEN: Poly Haven mobilyaları zaten eskitilmiş fotoğraf dokularıyla geliyor (MobilyaKurucu üstüne bir de kir
// ekliyor). Blender betikleriyle yapılan ~200 eşyanın ise düz, doygun, lekesiz renkleri vardı: modeli 90'lar
// olsa bile "dün alınmış" gibi duruyordu. Her betiği tek tek değiştirmek yerine tek bir kural koyuyoruz.
//
// AssetPostprocessor NEDİR: Unity bir dosyayı (burada FBX modeli) her içe aktardığında, bu sınıftan türeyen
// sınıflara "şunu aktardım, son bir dokunuş yapmak ister misin?" diye sorar. Bir fabrika bandının sonundaki
// eskitme işçisi gibi: her eşya banttan inerken aynı işlemden geçer. Blender betiği yeniden çalışıp FBX
// değişse de model yeniden içe aktarılır, eskitme yine uygulanır; elle yapılan bir ayar gibi kaybolmaz.
//
// OnPreprocessMaterialDescription: FBX'in içinden çıkan her malzeme kurulurken bir kez çağrılır (malzeme henüz FBX'in içinde,
// salt okunur hâle gelmeden önce). Burada yaptığımız değişiklik o malzemenin kalıcı hâli olur.
//
// Dokunulmayanlar:
//   - Görev eşyaları (anahtar, sigorta, tornavida, levye, kaydırıcı, tabak, kırık anahtar, not, isim kartı):
//     karanlıkta bulunmaları gerekir; soldurmak onları duvarın, dolabın içinde kaybettirir.
//   - Kendi ışık saçan malzemeler (asansör göstergesi, ampul camı): ışığı sönük olmasın.
//   - Gölge yaratık (Golge): rengi bilerek ayarlı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EsyaEskitici : AssetPostprocessor
{
    // Yalnızca Blender eşyalarının klasörü. Poly Haven mobilyaları başka klasörde (kendi eskitmeleri var).
    const string Klasor = "Assets/Models/Props/";
    const string KirDokusu = "Assets/Textures/Kir/Kir_Detay.png";

    // Ayarları değiştirince bu sayıyı bir artır: Unity bu sınıfın uyguladığı her modeli yeniden içe aktarır.
    // (GetVersion: Unity'nin "bu işçinin kuralı değişti mi?" diye baktığı sürüm numarası.)
    const uint Surum = 1;
    public override uint GetVersion() { return Surum; }

    // Eskitilmeyen modeller (dosya adı, uzantısız): görev eşyaları ve Gölge.
    static readonly HashSet<string> Haric = new HashSet<string>
    {
        "Anahtar", "Buson", "Tornavida", "Levye", "Kaydirici", "Tabak", "Kirik_Anahtar",
        "Not_Kagidi", "Isim_Karti", "Golge",
    };

    // #134: üç harfli figürleri (Cin_N) solgun tenlerini korusun, eskitilmez.
    static bool HaricMi(string ad) { return Haric.Contains(ad) || ad.StartsWith("Cin_"); }

    // Eskitmenin ölçüleri.
    const float Soldurma = 0.30f;     // rengin %30'u griye çekilir (güneşte, yıllarla solmuş)
    const float Karartma = 0.86f;     // %14 koyulaşır (kir, is)
    const float Sararma = 0.10f;      // mavisi %10 azalır, kırmızısı biraz artar: beyazlar kirli krem olur
    const float EnParlak = 0.32f;     // parlaklık (smoothness) en çok bu: tozlu yüzey ışığı dağıtır
    const float KirTekrari = 2.5f;    // kir dokusu UV içinde 2,5 kez döşenir
    const float KirGucu = 0.85f;      // kirin koyuluğu (1 = dokunun kendisi kadar)

    // Sıra: URP'nin kendi FBX malzeme kurucusu (FBX'teki rengi URP Lit malzemesine yazan) da bu noktada çalışır.
    // Bizimki ondan SONRA çalışmalı, yoksa URP rengi üstüne yeniden yazar ve eskitme kaybolur. Büyük sayı = sonra.
    public override int GetPostprocessOrder() { return 100; }

    // Bu FBX'lerin içe aktarma kipi "malzeme tarifinden" (materialImportMode = ImportViaMaterialDescription):
    // Unity her malzemeyi kurarken bu fonksiyonu çağırır; 'material' kurulmakta olan malzemedir.
    // (OnPostprocessMaterial bu kipte çağrılmıyordu: ilk denemede hiçbir eşya değişmedi.)
    void OnPreprocessMaterialDescription(UnityEditor.AssetImporters.MaterialDescription tarif, Material m, AnimationClip[] animasyonlar)
    {
        if (!assetPath.StartsWith(Klasor)) return;
        string dosya = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (HaricMi(dosya)) return;
        // Işık saçan malzeme (emisyon açık): göstergeler, ampul camı.
        if (m.IsKeywordEnabled("_EMISSION")) return;
        Eskit(m);
    }

    /// <summary>Bir malzemeyi eskitir: soldurur, karartır, sarartır, parlaklığını alır, kir döşer.</summary>
    public static void Eskit(Material m)
    {
        if (m.HasProperty("_BaseColor"))
        {
            Color c = m.GetColor("_BaseColor");
            // Parlaklık (luminance): insan gözünün renkleri algılayış ağırlıklarıyla gri değeri.
            float gri = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            Color solgun = Color.Lerp(c, new Color(gri, gri, gri, c.a), Soldurma) * Karartma;
            solgun.r = Mathf.Clamp01(solgun.r * (1f + Sararma * 0.4f));
            solgun.b = Mathf.Clamp01(solgun.b * (1f - Sararma));
            solgun.a = c.a;   // saydamlık (cam) değişmesin
            m.SetColor("_BaseColor", solgun);
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Mathf.Min(m.GetFloat("_Smoothness"), EnParlak));

        // Kir: URP Lit'in "detay" dokusu, ana rengin üstüne çarpılarak serilir. Doku henüz içe aktarılmadıysa
        // (projenin ilk açılışında sıra ona gelmemiş olabilir) kir atlanır; renk eskitmesi yine uygulanır.
        Texture2D kir = AssetDatabase.LoadAssetAtPath<Texture2D>(KirDokusu);
        if (kir != null && m.HasProperty("_DetailAlbedoMap"))
        {
            m.SetTexture("_DetailAlbedoMap", kir);
            m.SetTextureScale("_DetailAlbedoMap", Vector2.one * KirTekrari);
            m.SetFloat("_DetailAlbedoMapScale", KirGucu);
            // URP'nin kuralı (KirKurucu'daki gibi): güç tam 1 değilse _DETAIL_SCALED anahtar sözcüğü.
            m.DisableKeyword("_DETAIL_MULX2");
            m.EnableKeyword("_DETAIL_SCALED");
        }
    }

    // ---------- ELLE ÇALIŞTIRMA ----------
    // Kural değişince ya da ilk kurulumda bütün eşyaları yeniden içe aktarır (içe aktarma eskitmeyi uygular).
    [MenuItem("Huzur/Eşyaları Eskit (yeniden içe aktar)")]
    public static void HepsiniYenidenAktar()
    {
        string[] guidler = AssetDatabase.FindAssets("t:Model", new[] { Klasor.TrimEnd('/') });
        // StartAssetEditing / StopAssetEditing: içe aktarmaları tek tek değil toplu yapar (çok daha hızlı).
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string g in guidler)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log($"[ESKİT] {guidler.Length} eşya modeli yeniden içe aktarıldı ve eskitildi.");
    }
}
