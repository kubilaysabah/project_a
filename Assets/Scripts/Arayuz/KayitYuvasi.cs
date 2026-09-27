// KayitYuvasi.cs
// Kaydın "yuvası": Claude'un Editor'deki testleri Şahan'ın kaydına dokunmasın diye (2026-09-27).
//
// SORUN: oyunun kaydı (ulaşılan kat, çözülen kilitler, bulunan parçalar, aletler, defter)
// PlayerPrefs'te durur. Editor'de oynanan oyunla Claude'un oynatma modu testleri AYNI
// PlayerPrefs'i kullanıyordu. Claude her testten sonra ortalığı temizlemek için kayıt
// anahtarlarını siliyordu; Şahan Editor'de kaldığı yerden devam edemiyor, her yeni katı
// denemek için önceki katları baştan oynamak zorunda kalıyordu.
//
// ÇÖZÜM: kayıt anahtarlarının sonuna bir "yuva eki" gelir. Normalde ek boştur: anahtarlar
// eskisi gibi ("kayit_kat"). Claude test öncesinde Editor ayarlarına (EditorPrefs) bir ek
// yazar ("_test"); o zaman oyun "kayit_kat_test" gibi ayrı anahtarlara yazar ve okur. Test
// bitince Claude ekli anahtarları ve eki siler; Şahan'ın kaydı hiç değişmemiş olur.
// Build'de EditorPrefs yoktur: ek her zaman boştur.
//
// Ek açıkken oyun başında Console'a bir uyarı yazılır: unutulursa Şahan fark etsin.

using UnityEngine;

public static class KayitYuvasi
{
    /// <summary>Claude'un testlerinin Editor ayarlarına yazdığı ekin adı.</summary>
    public const string EditorAnahtari = "huzur_kayit_yuvasi";

    /// <summary>Kayıt anahtarlarına eklenen ek; normalde boş.</summary>
    public static string Ek
    {
        get
        {
            // #if UNITY_EDITOR: bu satırlar yalnızca Editor'de derlenir; build'e girmez
            // (UnityEditor build'de yoktur, derlenmeye çalışılsa hata verirdi).
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetString(EditorAnahtari, "");
#else
            return "";
#endif
        }
    }

    /// <summary>Bir kayıt anahtarının bu yuvadaki adı ("kayit_kat" -> "kayit_kat_test").</summary>
    public static string Anahtar(string ad) { return ad + Ek; }
}
