// KaliteKurucu.cs
// Grafik kalitesinin üç seviyesini üreten Editor aracı (#30).
// Menü: Huzur → Kalite Seviyelerini Kur  (AtmosferKurucu da her kurulumda çağırır)
//
// URP'de gölge keskinliği, gölge kademesi sayısı, görüntünün hangi çözünürlükte
// çizileceği gibi pahalı ayarlar bir "Render Pipeline ayar dosyası"nda (URP Asset)
// durur. Oyunun bugünkü hâli Assets/Settings/PC_RPAsset.asset: bu dosya "Yüksek"
// seviye olarak AYNEN kalır. Bu araç ondan iki kopya çıkarıp ayarlarını düşürür:
//
//   Orta   → gölgeler yarı keskinlikte (1024 piksel), 2 kademe, yumuşatma düşük
//   Düşük  → lamba gölgeleri KAPALI, ay gölgesi 512 piksel ve tek kademe,
//            yumuşak gölge yok, görüntü %75 çözünürlükte çizilip ekrana büyütülür
//
// Oyun sırasında GorunumUygulayici bu üç dosyadan birini etkin yapar.
// Kopya, çizim ayarlarını (Renderer) ortak kullanır; yalnızca yukarıdaki değerler farklı.

using UnityEditor;
using UnityEngine;

public static class KaliteKurucu
{
    public const string YuksekYolu = "Assets/Settings/PC_RPAsset.asset";
    public const string OrtaYolu = "Assets/Settings/Kalite_Orta_RPAsset.asset";
    public const string DusukYolu = "Assets/Settings/Kalite_Dusuk_RPAsset.asset";

    [MenuItem("Huzur/Kalite Seviyelerini Kur")]
    public static void Kur()
    {
        // ORTA
        SerializedObject orta = Kopyala(OrtaYolu);
        if (orta != null)
        {
            // FindProperty: dosyadaki bir alanı adıyla bulur (Inspector'ın gördüğü alanlar).
            orta.FindProperty("m_MainLightShadowmapResolution").intValue = 1024;
            orta.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = 2048;   // #112: 1024'e 5-6 lamba sığmıyordu
            orta.FindProperty("m_ShadowCascadeCount").intValue = 2;
            orta.FindProperty("m_SoftShadowQuality").intValue = 1;          // 1 = düşük yumuşatma
            orta.ApplyModifiedPropertiesWithoutUndo();
        }

        // DÜŞÜK
        SerializedObject dusuk = Kopyala(DusukYolu);
        if (dusuk != null)
        {
            dusuk.FindProperty("m_AdditionalLightShadowsSupported").boolValue = false;   // lamba gölgeleri yok
            dusuk.FindProperty("m_MainLightShadowmapResolution").intValue = 512;
            dusuk.FindProperty("m_ShadowCascadeCount").intValue = 1;
            dusuk.FindProperty("m_SoftShadowsSupported").boolValue = false;
            // Render scale: görüntü ekran çözünürlüğünün %75'inde çizilir ve büyütülür.
            // Piksel sayısı %56'ya iner (0,75 x 0,75); biraz bulanıklık karşılığında büyük kazanç.
            dusuk.FindProperty("m_RenderScale").floatValue = 0.75f;
            dusuk.ApplyModifiedPropertiesWithoutUndo();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Kalite seviyeleri kuruldu: Düşük, Orta (kopya), Yüksek (PC_RPAsset, değişmedi).");
    }

    // Yüksek seviyenin dosyasını hedef yola kopyalar ve kopyanın alanlarını düzenlemek
    // için bir SerializedObject döndürür. Dosya zaten varsa SİLİNMEZ, olduğu gibi
    // güncellenir: silip yeniden kopyalamak dosyanın kimliğini (GUID) değiştirirdi;
    // her graybox kurulumunda .meta dosyaları değişir, sahnedeki bağlantılar kopardı.
    private static SerializedObject Kopyala(string hedef)
    {
        Object mevcut = AssetDatabase.LoadAssetAtPath<Object>(hedef);
        if (mevcut != null) return new SerializedObject(mevcut);

        if (!AssetDatabase.CopyAsset(YuksekYolu, hedef))
        {
            Debug.LogError($"Kalite: {YuksekYolu} kopyalanamadı → {hedef}");
            return null;
        }
        return new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(hedef));
    }
}
