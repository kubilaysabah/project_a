// BuildAlici.cs
// Oyunun Windows build'ini tek adımda alan Editor aracı (#26).
// Menü: Huzur → Windows Build Al (Yayın)  /  Huzur → Windows Build Al (Geliştirme)
//
// BUILD NEDİR: Editor bir atölye gibidir; her alet elinin altında, her şey yarı açık.
// Build, oyunun atölyeden çıkıp paketlenmiş hâlidir: Unity yalnızca oyunun gerçekten
// kullandığı dosyaları toplar ve bir .exe üretir. Oyuncunun indireceği şey budur.
//
// İKİ ÇEŞİT
//   Geliştirme (Development): test göstergesi (kat, anomali, FPS) görünür, hata
//                             mesajları ayrıntılı yazılır. Denemek için.
//   Yayın (Release):          oyuncunun göreceği temiz hâl. Demo ve Steam için.
//
// Çıktı proje klasöründeki Builds/Windows/<çeşit>/ altına gider. Bu klasör git'e
// girmez (.gitignore'da /[Bb]uilds/ var): build'ler büyük ve her an yeniden üretilebilir.
//
// Bu dosya Editor klasöründe: yalnızca Editor'de derlenir, build'in içine girmez.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;   // BuildReport: build'in sonucunu anlatan rapor
using UnityEngine;

public static class BuildAlici
{
    // Proje kök klasörüne göre (Assets'in yanı). Assets'in İÇİNDE olsaydı Unity
    // build dosyalarını da içe aktarmaya çalışırdı.
    private const string KokKlasor = "Builds/Windows";

    [MenuItem("Huzur/Windows Build Al (Yayın)")]
    private static void YayinAl()
    {
        Al(gelistirme: false);
    }

    [MenuItem("Huzur/Windows Build Al (Geliştirme)")]
    private static void GelistirmeAl()
    {
        Al(gelistirme: true);
    }

    /// <summary>Build'i alır ve sonucu Console'a yazar. Başarılıysa true döner.</summary>
    public static bool Al(bool gelistirme)
    {
        string cesit = gelistirme ? "Geliştirme" : "Yayın";

        // Build, sahneyi DİSKTEKİ hâliyle paketler. Kaydedilmemiş değişiklik varsa
        // build'e girmez ve "ama ben bunu değiştirmiştim" şaşkınlığı yaşanır.
        // Sessizce kaydetmek yerine durup haber veriyoruz: belki o değişiklik
        // bir anomali önizlemesidir ve hiç kaydedilmemesi gerekir.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError($"[BUILD] {cesit}: sahnede kaydedilmemiş değişiklik var. " +
                           "Önce Ctrl+S ile kaydedin (anomali önizlemesi açıksa önce geri alın).");
            return false;
        }

        // Build listesindeki (File → Build Profiles) işaretli sahneler.
        var sahneler = new List<string>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (s.enabled) sahneler.Add(s.path);
        }
        if (sahneler.Count == 0)
        {
            Debug.LogError("[BUILD] Build listesinde hiç sahne yok.");
            return false;
        }

        // Klasörü önce temizle: eski build'den kalan dosyalar boyut ölçümünü şişirir
        // ve artık oyunda olmayan bir dosya yanlışlıkla pakette kalabilir.
        string klasor = Path.Combine(KokKlasor, gelistirme ? "Gelistirme" : "Yayin");
        if (Directory.Exists(klasor)) Directory.Delete(klasor, recursive: true);

        // .exe'nin adı ürün adıdır (Project Settings → Player → Product Name).
        string exe = Path.Combine(klasor, PlayerSettings.productName + ".exe");

        // BuildPlayerOptions: build'in tarifi. Hangi sahneler, nereye, hangi platform,
        // hangi seçeneklerle. StandaloneWindows64 = 64 bit Windows.
        var tarif = new BuildPlayerOptions
        {
            scenes = sahneler.ToArray(),
            locationPathName = exe,
            target = BuildTarget.StandaloneWindows64,
            options = gelistirme ? BuildOptions.Development : BuildOptions.None
        };

        Debug.Log($"[BUILD] {cesit} build başladı → {exe}");

        // Asıl iş: Unity sahneleri, kodu, sesleri, shader'ları paketler. İlk build
        // uzun sürer (shader'lar derlenir); sonrakiler önbellek sayesinde hızlanır.
        BuildReport rapor = BuildPipeline.BuildPlayer(tarif);
        BuildSummary ozet = rapor.summary;

        if (ozet.result == BuildResult.Succeeded)
        {
            // totalSize bayt cinsinden; 1024 x 1024 = 1 MB.
            Debug.Log($"[BUILD] {cesit} build TAMAM: {ozet.totalSize / (1024f * 1024f):0.0} MB, " +
                      $"{ozet.totalTime.TotalSeconds:0} sn, {ozet.totalWarnings} uyarı → {Path.GetFullPath(exe)}");
            return true;
        }

        Debug.LogError($"[BUILD] {cesit} build BAŞARISIZ: {ozet.result}, {ozet.totalErrors} hata. " +
                       "Ayrıntı Console'daki kırmızı satırlarda.");
        return false;
    }
}
