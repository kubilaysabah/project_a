// Defter.cs
// Anomali defterinin oyundaki kapısı (#45): listeyi kalıcı olarak saklar ve yeni bir
// anomali bulununca haber verir. Dil ve Ayarlar gibi "static": her yerden Defter.Isle(...)
// ile çağrılır, sahnede bir nesneye bağlı değildir.
//
// Liste PlayerPrefs'te tek satır yazı olarak durur ("defter_bulunan"): oyun kapanıp açılsa
// da bulunan anomaliler kaybolmaz. Listenin kuralı Cekirdek/AnomaliDefteri.cs'de (test ediliyor).

using UnityEngine;

public static class Defter
{
    private static string Anahtar { get { return KayitYuvasi.Anahtar("defter_bulunan"); } }   // yuvalı (KayitYuvasi)

    private static AnomaliDefteri defter;

    /// <summary>
    /// Yeni bir anomali deftere işlendiğinde çalışır (kimliğiyle). Köşedeki küçük defter
    /// simgesi (DefterGostergesi) buna abone. "event": dışarıdan yalnızca abone olunabilir,
    /// tetiklemek yalnızca bu sınıfın işidir.
    /// </summary>
    public static event System.Action<string> YeniKayit;

    // İlk kullanıldığında kayıttan okunur ("tembel yükleme").
    private static AnomaliDefteri D
    {
        get
        {
            if (defter == null) defter = AnomaliDefteri.Oku(PlayerPrefs.GetString(Anahtar, ""));
            return defter;
        }
    }

    public static int Sayi { get { return D.Sayi; } }
    public static bool BulunduMu(string kimlik) { return D.BulunduMu(kimlik); }

    /// <summary>
    /// Oyuncu bir anomalide doğru biçimde geri döndü: deftere işle. Yeniyse kaydet ve haber ver.
    /// </summary>
    public static void Isle(string kimlik)
    {
        if (!D.Isle(kimlik)) return;
        PlayerPrefs.SetString(Anahtar, D.Yazi());
        PlayerPrefs.Save();
        Debug.Log($"[DEFTER] Yeni anomali bulundu: {kimlik} ({D.Sayi})");
        YeniKayit?.Invoke(kimlik);
    }
}
