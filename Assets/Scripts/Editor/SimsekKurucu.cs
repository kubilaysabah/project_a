// SimsekKurucu.cs
// Şimşeği (#95) sahneye kuran editör aracı: menü "Huzur → Şimşeği Kur".
//
// Ne yapar?
//   1. Açık sahnedeki KatYoneticisi'ni bulur ve nesnesine Simsek bileşenini ekler (yoksa).
//   2. Bileşenin ihtiyaç duyduğu DOSYALARI bağlar: Gölge şablonu, iki gök gürültüsü sesi ve parlayan
//      gökyüzünün kaynak malzemesi. Neden adıyla aratmıyoruz? Build yalnızca bir yere BAĞLANMIŞ
//      dosyaları pakete koyar; çalışma anında adıyla aranan dosya build'de bulunamayabilir (#26 kuralı).
//   3. Sahnedeki ay ışığını (Ay_Isigi) bağlar. Ay bir sahne nesnesi; bağlamak şart değil (Simsek
//      bulamazsa kendisi arar), ama bağlı olması Inspector'da neyin kullanıldığını açıkça gösterir.
//   4. Sahneyi kaydeder: koddan yapılan değişiklik sahneyi kendiliğinden "kaydedilmemiş" yapmaz,
//      kaydedilmezse sessizce kaybolur (GrayboxKurucu.SahneyiKaydet'in açıklaması).
//
// Ayrı bir şimşek ışığı KURMUYORUZ: URP'de gölge atabilen tek yönlü ışık sahnenin ana ışığıdır (ay).
// Gölgesiz ikinci bir yönlü ışık duvarların içinden geçerdi. Ayrıntı Simsek.cs'in başında.

using UnityEngine;
using UnityEditor;

public static class SimsekKurucu
{
    const string GolgeYolu = "Assets/Prefabs/Esyalar/Kacis_Golgesi.prefab";
    const string ParlamaYolu = "Assets/Materials/Ampul_Cam.mat";     // URP Unlit; çalışma anında kopyalanır
    static readonly string[] SesYollari =
    {
        "Assets/Resources/Sesler/Gok_Gurultusu_1.wav",
        "Assets/Resources/Sesler/Gok_Gurultusu_2.wav",
    };

    [MenuItem("Huzur/Şimşeği Kur")]
    public static void Kur()
    {
        // FindObjectsInactive.Include: nesne kapalı olsa bile bulunsun.
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>(FindObjectsInactive.Include);
        if (yonetici == null)
        {
            Debug.LogError("Şimşek: sahnede KatYoneticisi yok; önce Landing sahnesini açın.");
            return;
        }

        Simsek simsek = yonetici.GetComponent<Simsek>();
        if (simsek == null) simsek = yonetici.gameObject.AddComponent<Simsek>();

        // SerializedObject: bileşenin Inspector'daki alanlarına koddan yazmanın Unity yolu. Özel
        // ([SerializeField] private) alanlara da ulaşır ve değişikliği sahneye kaydedilecek biçimde yapar.
        SerializedObject seri = new SerializedObject(simsek);

        GameObject sablon = AssetDatabase.LoadAssetAtPath<GameObject>(GolgeYolu);
        if (sablon == null) Debug.LogWarning($"Şimşek: {GolgeYolu} bulunamadı; Gölge şimşekte görünmeyecek.");
        seri.FindProperty("sablon").objectReferenceValue = sablon;

        Material parlama = AssetDatabase.LoadAssetAtPath<Material>(ParlamaYolu);
        if (parlama == null) Debug.LogWarning($"Şimşek: {ParlamaYolu} bulunamadı; pencereler çakmada parlamayacak.");
        seri.FindProperty("parlamaKaynagi").objectReferenceValue = parlama;

        // Gök gürültüsü sesleri: bulunanlar diziye sırayla girer.
        SerializedProperty dizi = seri.FindProperty("gokGurultuleri");
        var sesler = new System.Collections.Generic.List<AudioClip>();
        foreach (string yol in SesYollari)
        {
            AudioClip klip = AssetDatabase.LoadAssetAtPath<AudioClip>(yol);
            if (klip != null) sesler.Add(klip);
            else Debug.LogWarning($"Şimşek: {yol} bulunamadı (python Art/Ses/scripts/hava_uret.py ile üretin).");
        }
        dizi.arraySize = sesler.Count;
        for (int i = 0; i < sesler.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = sesler[i];

        // Ay: sahnedeki yönlü ışık (AtmosferKurucu onu Ay_Isigi diye adlandırıp ayarlıyor).
        Light ay = null;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
            if (l.type == LightType.Directional) { ay = l; break; }
        if (ay == null) Debug.LogWarning("Şimşek: sahnede yönlü ışık (ay) yok; Huzur → Atmosferi Kur çalıştırılmalı.");
        else if (ay.shadows == LightShadows.None) Debug.LogWarning("Şimşek: ay gölge atmıyor; şimşek ışığı duvarlardan sızar.");
        seri.FindProperty("ay").objectReferenceValue = ay;

        // WithoutUndo: kurucu işi, Ctrl+Z geçmişine girmesin (öbür kurucular da böyle yapar).
        seri.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"Şimşek kuruldu: {yonetici.name} üzerinde Simsek; Gölge şablonu {(sablon != null ? "bağlı" : "YOK")}, " +
                  $"{sesler.Count} gök gürültüsü, parlama malzemesi {(parlama != null ? "bağlı" : "YOK")}, ay {(ay != null ? ay.name : "YOK")}.");
        GrayboxKurucu.SahneyiKaydet("Simsek");
    }
}
