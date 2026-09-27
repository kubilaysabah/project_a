// Kayit.cs
// Oyunun kaldığı yer (#52): otomatik kayıt.
//
// Eskiden bir tur birkaç dakikaydı; artık oyun iki saat sürecek (Docs/hikaye.md §9), oyuncu
// kapatıp açınca kaldığı kattan devam etmeli. Kayıt noktası: bir kat kapısı açılıp üst kata
// varılınca. (İleride #54: dairede her kilit çözülünce de.)
//
// Şimdilik PlayerPrefs'te tek sayı ("kayit_kat"): oyuncunun ulaştığı en yüksek kat. Yeni
// oyun 1'dir. Ayarlar ve defter de PlayerPrefs'te; hepsi aynı yerde, yedeklemesi kolay.

using UnityEngine;

public static class Kayit
{
    private const string KatAnahtari = "kayit_kat";
    private const string KilitAnahtari = "kayit_kilitler";   // çözülen kilitler (#54)

    // Kilit kaydı ilk sorulduğunda PlayerPrefs'ten bir kez okunur, sonra bellekte tutulur.
    private static KilitKaydi kilitler;
    private static KilitKaydi Kilitler
    {
        get
        {
            if (kilitler == null) kilitler = KilitKaydi.Oku(PlayerPrefs.GetString(KilitAnahtari, ""));
            return kilitler;
        }
    }

    /// <summary>Bir kilit çözüldü: kaydedilir (#54). Oyun açılınca o kilit çözük başlar.</summary>
    public static void KilitCozuldu(int kat, string kimlik)
    {
        if (!Kilitler.Ekle(kat, kimlik)) return;
        PlayerPrefs.SetString(KilitAnahtari, Kilitler.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] {kat}. kat: '{kimlik}' kilidi kaydedildi.");
    }

    /// <summary>Bu katın bu kilidi daha önce çözülmüş mü?</summary>
    public static bool KilitCozukMu(int kat, string kimlik) { return Kilitler.Iceriyor(kat, kimlik); }

    /// <summary>Oyuncunun ulaştığı kat; 1 = başlangıç (kayıt yok).</summary>
    public static int Kat
    {
        get { return Mathf.Clamp(PlayerPrefs.GetInt(KatAnahtari, KatSayaci.BaslangicKati), KatSayaci.BaslangicKati, KatSayaci.CatiKati); }
        set
        {
            PlayerPrefs.SetInt(KatAnahtari, Mathf.Clamp(value, KatSayaci.BaslangicKati, KatSayaci.CatiKati));
            PlayerPrefs.Save();
            Debug.Log($"[KAYIT] Kat {Kat} kaydedildi.");
        }
    }

    /// <summary>Kayıt var mı (1. kattan ileride mi)?</summary>
    public static bool Var { get { return Kat > KatSayaci.BaslangicKati; } }

    /// <summary>Kaydı siler: yeni oyun.</summary>
    public static void Sil()
    {
        PlayerPrefs.DeleteKey(KatAnahtari);
        PlayerPrefs.DeleteKey(KilitAnahtari);
        kilitler = null;
        Aletler.Bosalt();   // yeni oyun: bulunan aletler de gider (#61)
        PlayerPrefs.Save();
    }
}
