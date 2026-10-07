// YaziTipi.cs
// Dile göre yazı tipi (#123). Oyunun yazı tipi Unity'nin gömülü Liberation Sans'ıdır: Latin ve Kiril
// harflerini (Almanca, İspanyolca, Portekizce, Rusça) taşır ama Çince ve Japonca karakterleri taşımaz;
// eksik karakter ekranda kutu olarak çıkar. Bu sınıf Çince/Japonca seçiliyken Noto Sans CJK'yi (Google,
// SIL Open Font License; Assets/Resources/Fonts) verir, öbür dillerde null (= varsayılan).
//
// İKİ YER: 1) OnGUI yazıları: ArayuzOlcegi.Uygula her OnGUI başında GUI.skin.font'u buradan alır; stiller
// (GUIStyle) yazı tipini skin'den miras aldığı için tek yerden değişir. 2) Dünyadaki TextMesh yazıları
// (YerelYazi): yazı tipiyle birlikte MALZEME de değişmeli, çünkü Yazi3B gölgelendiricisi harf dokusunu
// malzemeden okur; CJK malzemesi (Yazi_3B_CJK) kurucu tarafından Resources/Fonts altına yazılır.
//
// DİNAMİK YAZI TİPİ: Unity harf dokusunu istenen harflere göre yeniden üretebilir (textureRebuilt); o an
// malzemenin dokusu eskimiş olur. Burada her iki yazı tipi için o olay dinlenir ve malzemeye yeni doku yazılır.

using UnityEngine;

public static class YaziTipi
{
    private static Font cjk;
    private static Material cjkMalzeme;
    private static bool yuklendi, dinleniyor;

    /// <summary>Dil Çince ya da Japonca mı (CJK yazı tipi gerekir)?</summary>
    public static bool CjkMi(DilKodu dil) { return dil == DilKodu.ZH || dil == DilKodu.JA; }

    /// <summary>Seçili dilin OnGUI yazı tipi; null = Unity'nin varsayılanı (Liberation Sans).</summary>
    public static Font Guncel { get { return CjkMi(Dil.Etkin) ? Cjk : null; } }

    /// <summary>Noto Sans CJK (yoksa null: harfler kutu çıkar ama oyun çalışır).</summary>
    public static Font Cjk
    {
        get { Yukle(); return cjk; }
    }

    /// <summary>Dünya yazıları için CJK malzemesi (Yazi3B gölgelendiricisi + Noto dokusu); yoksa null.</summary>
    public static Material CjkMalzeme
    {
        get { Yukle(); return cjkMalzeme; }
    }

    private static void Yukle()
    {
        if (yuklendi) return;
        yuklendi = true;
        cjk = Resources.Load<Font>("Fonts/NotoSansCJKsc-Regular");
        cjkMalzeme = Resources.Load<Material>("Fonts/Yazi_3B_CJK");
        if (cjk == null) Debug.LogWarning("[YAZI] Resources/Fonts/NotoSansCJKsc-Regular yok: Çince/Japonca kutu görünür.");
        if (cjkMalzeme != null && cjk != null) cjkMalzeme.mainTexture = cjk.material.mainTexture;
        if (!dinleniyor)
        {
            dinleniyor = true;
            Font.textureRebuilt += DokuYenilendi;
        }
    }

    // Harf dokusu yeniden üretildi: aynı yazı tipini kullanan dünya malzemesine yeni dokuyu yaz.
    private static void DokuYenilendi(Font f)
    {
        if (cjk != null && f == cjk && cjkMalzeme != null) cjkMalzeme.mainTexture = f.material.mainTexture;
    }
}
