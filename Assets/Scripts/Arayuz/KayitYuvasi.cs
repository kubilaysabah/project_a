// KayitYuvasi.cs
// Kaydın "yuvası": Claude'un Editor'deki testleri Şahan'ın kaydına dokunmasın diye (2026-09-27).
//
// SORUN: oyunun kaydı (ulaşılan kat, çözülen kilitler, bulunan parçalar, aletler, defter)
// PlayerPrefs'te durur. Editor'de oynanan oyunla Claude'un oynatma modu testleri AYNI
// PlayerPrefs'i kullanıyordu. Claude her testten sonra ortalığı temizlemek için kayıt
// anahtarlarını siliyordu; Şahan Editor'de kaldığı yerden devam edemiyor, her yeni katı
// denemek için önceki katları baştan oynamak zorunda kalıyordu.
//
// ÇÖZÜM: bir "yuva eki". Normalde ek boştur. Claude test öncesinde Editor ayarlarına (EditorPrefs)
// bir ek yazar ("_test"); #122'den beri ek KAYIT DOSYASININ adına girer (kayit.txt → kayit_test.txt,
// KayitDeposu.Oyun.Yol), anahtar adları düz kalır. Test bitince Claude test dosyasını ve eki siler;
// Şahan'ın kayit.txt'si hiç değişmemiş olur. Build'de EditorPrefs yoktur: ek her zaman boştur.
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

    /// <summary>Kayıt anahtarının adı. #122'den beri olduğu gibi döner (yuva dosya adında); çağrı yerleri korunsun diye duruyor.</summary>
    public static string Anahtar(string ad) { return ad; }
}
