// Aletler.cs
// Kapıcının alet çantası: sahnedeki köprü ve kayıt (#61). Kural Cekirdek/AletCantasi.cs'de.
//
// Cep gibi "static": her yerden Aletler.Var("tornavida") denir. Bulunan aletler kayıt dosyasına (KayitDeposu.Oyun, #122)
// yazılır ("kayit_aletler"): oyun kapatılıp açılınca da kapıcıda kalır. Köşedeki simgeler
// (EsyaGostergesi) Degisti olayını dinler.

using UnityEngine;

public static class Aletler
{
    private static string KayitAnahtari { get { return KayitYuvasi.Anahtar("kayit_aletler"); } }   // yuvalı (KayitYuvasi)
    private static AletCantasi canta;

    private static AletCantasi Canta
    {
        get
        {
            if (canta == null) canta = AletCantasi.Oku(KayitDeposu.Oyun.GetString(KayitAnahtari, ""));
            return canta;
        }
    }

    /// <summary>Çantaya alet eklendi ya da çanta boşaldı.</summary>
    public static event System.Action Degisti;

    public static bool Var(string kimlik) { return Canta.Var(kimlik); }

    public static System.Collections.Generic.List<string> Liste() { return Canta.Liste(); }

    /// <summary>Çantanın kayıt metni ("tornavida;levye"): kat başı anı için (Kayit.KatBasiniKaydet).</summary>
    public static string Metin() { return Canta.Metin(); }

    /// <summary>Çantayı verilen metindeki hâline döndürür ve kaydeder (Kayit.KatBasinaDon, #83).</summary>
    public static void GeriYukle(string metin)
    {
        canta = AletCantasi.Oku(metin ?? "");
        KayitDeposu.Oyun.SetString(KayitAnahtari, canta.Metin());
        KayitDeposu.Oyun.Save();
        Degisti?.Invoke();
    }

    /// <summary>Bulunan aleti çantaya koyar ve kaydeder.</summary>
    public static void Ekle(string kimlik)
    {
        if (!Canta.Ekle(kimlik)) return;
        KayitDeposu.Oyun.SetString(KayitAnahtari, Canta.Metin());
        KayitDeposu.Oyun.Save();
        Debug.Log($"[ALET] '{kimlik}' kapıcının çantasına girdi.");
        Degisti?.Invoke();
    }

    /// <summary>Çantayı boşaltır (yeni oyun, eski kuraldaki ceza). Kayıttan da siler.</summary>
    public static void Bosalt()
    {
        Canta.Temizle();
        KayitDeposu.Oyun.DeleteKey(KayitAnahtari);
        KayitDeposu.Oyun.Save();
        Degisti?.Invoke();
    }
}
