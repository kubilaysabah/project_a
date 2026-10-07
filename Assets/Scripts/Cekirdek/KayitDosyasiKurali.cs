// KayitDosyasiKurali.cs
// Kayıt dosyasının biçimi (#122): "anahtar<TAB>değer" satırları. Saf C# (Unity yok) ki testlenebilsin.
//
// NEDEN DOSYA: kayıt PlayerPrefs'teydi; Windows'ta PlayerPrefs = kayıt defteri (registry). Steam Cloud
// (Auto-Cloud) yalnızca DOSYA klasörlerini eşler, kayıt defterini eşlemez; Steam Deck'te de kayıt defteri yok.
// Oyun artık kaydını persistentDataPath'te küçük bir metin dosyasına yazar (KayitDeposu); bu sınıf o metni
// sözlüğe çevirir ve sözlüğü metne döker.
//
// BİÇİM: her satır "anahtar", bir sekme, "değer". Değerin içindeki sekme ve satır sonu kaçışlanır
// ("\t" → "\\t", "\n" → "\\n", "\\" → "\\\\"), çünkü satır = kayıt. Metinler.txt'nin kuralıyla aynı ruh.
// Boş satırlar ve '#' ile başlayan satırlar yok sayılır (insan okuyabilsin, elle not düşülebilsin).

using System.Collections.Generic;
using System.Text;

public static class KayitDosyasiKurali
{
    /// <summary>Dosya metnini anahtar→değer sözlüğüne çevirir. Bozuk satırlar atlanır; sonraki aynı anahtar öncekini ezer.</summary>
    public static Dictionary<string, string> Oku(string metin)
    {
        var sozluk = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(metin)) return sozluk;
        foreach (string hamSatir in metin.Split('\n'))
        {
            string satir = hamSatir.TrimEnd('\r');
            if (satir.Length == 0 || satir[0] == '#') continue;
            int sekme = satir.IndexOf('\t');
            if (sekme <= 0) continue;                       // anahtarı olmayan satır
            string anahtar = satir.Substring(0, sekme);
            string deger = KacisiCoz(satir.Substring(sekme + 1));
            sozluk[anahtar] = deger;
        }
        return sozluk;
    }

    /// <summary>Sözlüğü dosya metnine döker (anahtarlar sıralı: dosya her seferinde aynı görünür, git/yedek dostu).</summary>
    public static string Metin(Dictionary<string, string> sozluk)
    {
        var anahtarlar = new List<string>(sozluk.Keys);
        anahtarlar.Sort(string.CompareOrdinal);
        var sb = new StringBuilder();
        sb.Append("# Huzur Apartmanı kaydı. Her satır: anahtar, sekme, değer.\n");
        foreach (string a in anahtarlar)
            sb.Append(a).Append('\t').Append(Kacisla(sozluk[a] ?? "")).Append('\n');
        return sb.ToString();
    }

    // Değerin içindeki sekme ve satır sonu satırı bozmasın diye görünür kaçış dizilerine çevrilir.
    public static string Kacisla(string deger)
    {
        return deger.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    public static string KacisiCoz(string metin)
    {
        if (metin.IndexOf('\\') < 0) return metin;   // kaçış yoksa olduğu gibi (çoğu değer)
        var sb = new StringBuilder(metin.Length);
        for (int i = 0; i < metin.Length; i++)
        {
            char c = metin[i];
            if (c != '\\' || i + 1 >= metin.Length) { sb.Append(c); continue; }
            char sonraki = metin[++i];
            switch (sonraki)
            {
                case 't': sb.Append('\t'); break;
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case '\\': sb.Append('\\'); break;
                default: sb.Append('\\').Append(sonraki); break;   // bilinmeyen kaçış: olduğu gibi
            }
        }
        return sb.ToString();
    }

    /// <summary>Sayıyı dile bağlı olmadan yazar (Türkçe Windows'ta ondalık virgülü dosyaya girmesin).</summary>
    public static string SayiMetni(float deger)
    {
        return deger.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Dosyadaki sayıyı okur; bozuksa varsayılan.</summary>
    public static float Sayi(string metin, float varsayilan)
    {
        float f;
        return float.TryParse(metin, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f) ? f : varsayilan;
    }

    public static int TamSayi(string metin, int varsayilan)
    {
        int i;
        return int.TryParse(metin, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : varsayilan;
    }
}
