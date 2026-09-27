// KilitKaydi.cs
// Çözülen kilitlerin kaydı (#54): hangi katta hangi kilit çözüldü.
//
// Oyun kapatılıp açılınca oyuncu kaldığı kattan devam eder (Kayit, #52); o katın dairesinde
// çözdüğü kilitler de çözük kalmalı, yoksa anahtarı kullanılmış bir kapının önünde kalırdı.
// Kayıt tek bir metin olarak saklanır (PlayerPrefs'e yazılabilsin): "1:salon;1:kiler;2:odaB".
// Bu sınıf yalnızca metni okur, yazar ve sorgular; Unity'siz, testli. Sahnedeki köprü Kayit.cs.

using System.Collections.Generic;

public class KilitKaydi
{
    // HashSet: aynı değeri iki kez tutmayan bir küme; "içinde var mı?" sorusuna hızlı cevap verir.
    private readonly HashSet<string> cozulenler = new HashSet<string>();

    private static string Anahtar(int kat, string kimlik) { return kat + ":" + kimlik; }

    /// <summary>Kimlik kayda uygun mu? Ayırıcı karakterler (; ve :) kimlikte olamaz.</summary>
    public static bool KimlikGecerli(string kimlik)
    {
        return !string.IsNullOrEmpty(kimlik) && kimlik.IndexOf(';') < 0 && kimlik.IndexOf(':') < 0;
    }

    /// <summary>Kilidi çözülmüş olarak ekler. Yeni eklendiyse true (zaten varsa false).</summary>
    public bool Ekle(int kat, string kimlik)
    {
        if (!KimlikGecerli(kimlik)) return false;
        return cozulenler.Add(Anahtar(kat, kimlik));
    }

    public bool Iceriyor(int kat, string kimlik) { return cozulenler.Contains(Anahtar(kat, kimlik)); }

    public int Sayi { get { return cozulenler.Count; } }

    public void Temizle() { cozulenler.Clear(); }

    /// <summary>Saklanacak metin. Sıralı yazılır: aynı kayıt hep aynı metni verir.</summary>
    public string Metin()
    {
        var liste = new List<string>(cozulenler);
        liste.Sort(System.StringComparer.Ordinal);
        return string.Join(";", liste);
    }

    /// <summary>Metinden okur; bozuk parçaları (sayı olmayan kat, boş kimlik) atlar.</summary>
    public static KilitKaydi Oku(string metin)
    {
        var k = new KilitKaydi();
        if (string.IsNullOrEmpty(metin)) return k;
        foreach (string parca in metin.Split(';'))
        {
            int iki = parca.IndexOf(':');
            if (iki <= 0) continue;
            if (!int.TryParse(parca.Substring(0, iki), out int kat)) continue;
            k.Ekle(kat, parca.Substring(iki + 1));
        }
        return k;
    }
}
