// AtmosferProfiliTestleri.cs
// Gece post-processing profilinin DOSYADA gerçekten kayıtlı olduğunu denetler (#31).
//
// #11'de kurulan profil, efektleri yalnızca bellekte tutuyordu: dosyaya efektlerin
// yerine boş kayıtlar (fileID: 0) yazılmıştı. Editor'deki kareler efektli göründüğü
// için hata aylarca fark edilmedi; oyun modunda ve build'de ise hiç efekt yoktu.
// Bu test dosyanın kendisini okur: Editor'ün belleği ne gösterirse göstersin, diske
// yazılmamış bir efekt testi geçemez.

using System.IO;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AtmosferProfiliTestleri
{
    private const string ProfilYolu = "Settings/Atmosfer_Gece.asset";   // Assets/ altında

    // Profilde olması gereken efektler (AtmosferKurucu.PostProcessing ile aynı liste).
    private static readonly string[] Efektler = { "Tonemapping", "ColorAdjustments", "SplitToning", "Vignette", "FilmGrain", "Bloom", "LiftGammaGain" };

    private static string DosyayiOku()
    {
        string yol = Path.Combine(Application.dataPath, ProfilYolu);
        Assert.IsTrue(File.Exists(yol), "Profil dosyası yok: " + yol);
        return File.ReadAllText(yol);
    }

    [Test]
    public void Profilde_bos_efekt_kaydi_yok()
    {
        // "- {fileID: 0}": listede bir efekt var gibi görünüp dosyada karşılığı olmayan kayıt.
        StringAssert.DoesNotContain("- {fileID: 0}", DosyayiOku(),
            "Profilde boş efekt kaydı var: efektler bellekte kalmış, dosyaya yazılmamış (#31).");
    }

    [Test]
    public void Her_efekt_dosyanin_icinde_kayitli()
    {
        string metin = DosyayiOku();
        foreach (string efekt in Efektler)
        {
            // Alt nesne olarak kaydedilen her efekt dosyada kendi adıyla bir bölüm açar.
            StringAssert.Contains("m_Name: " + efekt, metin, $"'{efekt}' profil dosyasında kayıtlı değil.");
        }
    }
}
