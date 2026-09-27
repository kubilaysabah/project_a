// TabakYigini.cs
// Kapıcının kucağında üst üste biriken tabaklar (#66, Şahan: "tabakların hepsini üst üste
// toplayabiliriz, 1/4-2/4 gibi biriksin").
//
// Tabaklar ele girmez (elde tek eşya kuralı onları tek tek taşıtıyordu); bulundukça yığının
// ÜSTÜNE konur. Sofrada boş bir yere bakıp E'ye basınca en üstteki tabak oraya konur (Yuva).
// Yanlış tabak konduysa E ile geri alınır ve yığının ALTINA gider: sıradaki tabak üste çıkar.
// Böylece doğru ismi doğru yere koymak hâlâ oyuncunun işi.
//
// Köşedeki gösterge (EsyaGostergesi) buradan okur: yığındaki tabak sayısı / katın tabak sayısı
// ve en üstteki tabağın ismi. Cep ve Aletler gibi "static". Bulunan tabaklar kat kaydına yazılır
// (Kayit.ParcaBulundu); oyun açılınca yeniden yere konmaz, doğrudan yığına girer.

using System.Collections.Generic;
using UnityEngine;

public static class TabakYigini
{
    // Liste: sonuncusu en üstteki tabak. Tabakların sahnedeki nesneleri (kapalı tutulur).
    private static readonly List<GameObject> yigin = new List<GameObject>();
    private static int toplam;

    /// <summary>Katın tabak sayısı (göstergedeki paydası). Zincir uygulayıcısı yazar.</summary>
    public static void Ayarla(int katinTabakSayisi) { toplam = katinTabakSayisi; }

    public static void Temizle() { yigin.Clear(); toplam = 0; }

    public static int Sayi { get { Temizlik(); return yigin.Count; } }
    public static int Toplam { get { return toplam; } }

    /// <summary>En üstteki tabak; yığın boşsa null.</summary>
    public static GameObject Ust { get { Temizlik(); return yigin.Count > 0 ? yigin[yigin.Count - 1] : null; } }

    /// <summary>En üstteki tabağın üstünde yazan isim (göstergede görünür); yoksa boş.</summary>
    public static string UstIsim
    {
        get
        {
            GameObject u = Ust;
            TextMesh t = u != null ? u.GetComponentInChildren<TextMesh>(true) : null;
            return t != null ? t.text : "";
        }
    }

    /// <summary>Bulunan tabak yığının üstüne konur.</summary>
    public static void UsteKoy(GameObject tabak)
    {
        if (tabak == null || yigin.Contains(tabak)) return;
        tabak.SetActive(false);
        yigin.Add(tabak);
    }

    /// <summary>Sofradan geri alınan tabak yığının altına konur.</summary>
    public static void AltaKoy(GameObject tabak)
    {
        if (tabak == null || yigin.Contains(tabak)) return;
        tabak.SetActive(false);
        yigin.Insert(0, tabak);
    }

    /// <summary>En üstteki tabağı yığından alır (sofraya konacak); yoksa null.</summary>
    public static GameObject UsttenAl()
    {
        GameObject u = Ust;
        if (u != null) yigin.RemoveAt(yigin.Count - 1);
        return u;
    }

    /// <summary>Bir tabak yığından çıkarılır (kayıttan sofraya oturtulurken).</summary>
    public static void Cikar(GameObject tabak) { yigin.Remove(tabak); }

    // Kat değişince tabak nesneleri yok edilir; listede kalan boş kayıtlar atılır.
    private static void Temizlik() { yigin.RemoveAll(g => g == null); }
}
