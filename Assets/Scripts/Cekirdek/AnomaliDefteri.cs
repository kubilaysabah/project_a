// AnomaliDefteri.cs
// Oyuncunun bulduğu anomalilerin listesi (#45).
//
// Şahan'ın kararları (2026-09-26):
//   - Bir anomali, oyuncu o katta DOĞRU biçimde geri döndüğünde bulunmuş sayılır: fark
//     ettiğini kanıtlamış olur. Fark etmeden yukarı çıkarsa bulunmamış sayılır.
//   - Liste oyunlar arası kalıcı: 66 anomalinin hepsini bulmak için oyun tekrar oynanır.
//
// Bu dosya yalnızca listeyi tutar ve onu tek satırlık bir yazıya çevirir (kaydetmek için).
// Nereye kaydedileceği (PlayerPrefs) Arayuz/Defter.cs'nin işi. Unity'ye ait tek satır yok:
// kural Unity açılmadan test edilir.

using System.Collections.Generic;

public class AnomaliDefteri
{
    // Bulunan anomalilerin kimlikleri. HashSet: aynı kimlik iki kez girmez.
    private readonly HashSet<string> bulunanlar = new HashSet<string>();

    /// <summary>Bulunan anomali sayısı.</summary>
    public int Sayi { get { return bulunanlar.Count; } }

    /// <summary>
    /// Bir anomaliyi deftere işler. YENİ bulunduysa true döner (bildirim ve kayıt için);
    /// zaten defterdeyse false.
    /// </summary>
    public bool Isle(string kimlik)
    {
        if (string.IsNullOrEmpty(kimlik) || kimlik.Contains(",")) return false;   // virgül ayraç
        return bulunanlar.Add(kimlik);
    }

    /// <summary>Bu anomali daha önce bulundu mu?</summary>
    public bool BulunduMu(string kimlik) { return kimlik != null && bulunanlar.Contains(kimlik); }

    /// <summary>
    /// Listeyi tek satırlık bir yazıya çevirir: "acik_kapi,ters_numara,...". Sıralı: aynı
    /// liste her zaman aynı yazıyı verir (kayıt dosyası gereksiz yere değişmesin).
    /// </summary>
    public string Yazi()
    {
        var liste = new List<string>(bulunanlar);
        liste.Sort(System.StringComparer.Ordinal);
        return string.Join(",", liste);
    }

    /// <summary>Yazıdan (Yazi()'nin çıktısından) defteri geri kurar. Boş ya da bozuk parçalar atlanır.</summary>
    public static AnomaliDefteri Oku(string yazi)
    {
        var defter = new AnomaliDefteri();
        if (string.IsNullOrEmpty(yazi)) return defter;
        foreach (string parca in yazi.Split(','))
        {
            string k = parca.Trim();
            if (k.Length > 0) defter.bulunanlar.Add(k);
        }
        return defter;
    }
}
