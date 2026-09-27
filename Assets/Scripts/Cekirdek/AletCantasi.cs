// AletCantasi.cs
// Kapıcının bulduğu aletler (#61): tornavida gibi.
//
// Şahan'ın kararı (2026-09-27): aletler eli meşgul etmez ve bitmez. Bir kez bulunan alet kafa
// lambası gibi kapıcıda kalır, sonraki katlarda da işe yarar. Elde tek eşya kuralı (Envanter)
// yalnızca anahtar, sigorta ve not için geçerli. Bu sınıf yalnızca "hangi aletler var"ı tutar
// ve kayda yazılabilecek bir metne çevirir ("tornavida;cekic"). Unity'siz, testli; sahnedeki
// köprü Oyuncu/Aletler.cs.

using System.Collections.Generic;

public class AletCantasi
{
    private readonly HashSet<string> aletler = new HashSet<string>();

    /// <summary>Bu eşya türü bir alet mi? (Aletler çantaya gider, ele değil.)</summary>
    public static bool AletMi(EsyaTuru tur) { return tur == EsyaTuru.Tornavida; }

    /// <summary>Aleti ekler; yeniyse true.</summary>
    public bool Ekle(string kimlik)
    {
        if (string.IsNullOrEmpty(kimlik) || kimlik.IndexOf(';') >= 0) return false;
        return aletler.Add(kimlik);
    }

    public bool Var(string kimlik) { return !string.IsNullOrEmpty(kimlik) && aletler.Contains(kimlik); }

    public int Sayi { get { return aletler.Count; } }

    public void Temizle() { aletler.Clear(); }

    /// <summary>Kimlikler, sıralı (aynı çanta hep aynı metni verir).</summary>
    public List<string> Liste()
    {
        var l = new List<string>(aletler);
        l.Sort(System.StringComparer.Ordinal);
        return l;
    }

    public string Metin() { return string.Join(";", Liste()); }

    public static AletCantasi Oku(string metin)
    {
        var c = new AletCantasi();
        if (!string.IsNullOrEmpty(metin))
            foreach (string k in metin.Split(';')) c.Ekle(k);
        return c;
    }
}
