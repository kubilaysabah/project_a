// Aletler.cs
// Kapıcının alet çantası: sahnedeki köprü ve kayıt (#61). Kural Cekirdek/AletCantasi.cs'de.
//
// Cep gibi "static": her yerden Aletler.Var("tornavida") denir. Bulunan aletler PlayerPrefs'e
// yazılır ("kayit_aletler"): oyun kapatılıp açılınca da kapıcıda kalır. Köşedeki simgeler
// (EsyaGostergesi) Degisti olayını dinler.

using UnityEngine;

public static class Aletler
{
    private const string KayitAnahtari = "kayit_aletler";
    private static AletCantasi canta;

    private static AletCantasi Canta
    {
        get
        {
            if (canta == null) canta = AletCantasi.Oku(PlayerPrefs.GetString(KayitAnahtari, ""));
            return canta;
        }
    }

    /// <summary>Çantaya alet eklendi ya da çanta boşaldı.</summary>
    public static event System.Action Degisti;

    public static bool Var(string kimlik) { return Canta.Var(kimlik); }

    public static System.Collections.Generic.List<string> Liste() { return Canta.Liste(); }

    /// <summary>Bulunan aleti çantaya koyar ve kaydeder.</summary>
    public static void Ekle(string kimlik)
    {
        if (!Canta.Ekle(kimlik)) return;
        PlayerPrefs.SetString(KayitAnahtari, Canta.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[ALET] '{kimlik}' kapıcının çantasına girdi.");
        Degisti?.Invoke();
    }

    /// <summary>Çantayı boşaltır (yeni oyun, eski kuraldaki ceza). Kayıttan da siler.</summary>
    public static void Bosalt()
    {
        Canta.Temizle();
        PlayerPrefs.DeleteKey(KayitAnahtari);
        PlayerPrefs.Save();
        Degisti?.Invoke();
    }
}
