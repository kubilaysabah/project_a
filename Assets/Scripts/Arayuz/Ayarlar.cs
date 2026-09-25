// Ayarlar.cs
// Oyuncu ayarlarının Unity tarafı (#27): fare hassasiyeti, ses seviyesi,
// fareyi ters çevirme, görüş açısı.
//
// Dil.cs ile aynı kalıp: oyunun her yerinden Ayarlar.Hassasiyet diye okunur,
// Ayarlar.Hassasiyet = 1.5f diye yazılır. Her yazışta değer AyarKurallari'ndan
// geçip sınırlanır, PlayerPrefs'e kaydedilir ve Degisti haberi gönderilir.
//
// KİM NEYİ UYGULUYOR
//   Ses seviyesi       → burada: AudioListener.volume (bütün seslerin ortak düğmesi)
//   Hassasiyet, ters   → BirinciSahisKontrolcu her karede buradan okur
//   Görüş açısı        → BirinciSahisKontrolcu, Degisti haberini dinleyerek kameraya yazar

using System;
using UnityEngine;

public static class Ayarlar
{
    // PlayerPrefs'teki kayıt adları. "ayar_dil" Dil.cs'te.
    private const string HassasiyetAdi = "ayar_hassasiyet";
    private const string SesAdi = "ayar_ses";
    private const string TersAdi = "ayar_ters";
    private const string GorusAdi = "ayar_gorus";

    private static float hassasiyet, ses, gorus;
    private static bool ters;
    private static bool hazir;

    /// <summary>Bir ayar değişti (görüş açısını uygulayan kontrolcü dinler).</summary>
    public static event Action Degisti;

    // ---------- DEĞERLER ----------
    // "get" okurken, "set" yazarken çalışır. Yazarken "value" yazılan değerdir.

    /// <summary>Fare hassasiyeti çarpanı (0,25–3).</summary>
    public static float Hassasiyet
    {
        get { Hazirla(); return hassasiyet; }
        set { Hazirla(); hassasiyet = AyarKurallari.Hassasiyet(value); Kaydet(); }
    }

    /// <summary>Bütün seslerin ortak seviyesi (0–1).</summary>
    public static float Ses
    {
        get { Hazirla(); return ses; }
        set { Hazirla(); ses = AyarKurallari.Ses(value); AudioListener.volume = ses; Kaydet(); }
    }

    /// <summary>Fare yukarı itilince aşağı bakılsın mı.</summary>
    public static bool Ters
    {
        get { Hazirla(); return ters; }
        set { Hazirla(); ters = value; Kaydet(); }
    }

    /// <summary>Oyuncu kamerasının görüş açısı (60–90 derece).</summary>
    public static float Gorus
    {
        get { Hazirla(); return gorus; }
        set { Hazirla(); gorus = AyarKurallari.Gorus(value); Kaydet(); }
    }

    /// <summary>Bütün ayarları oyunun ayarlandığı hâle döndürür.</summary>
    public static void VarsayilanaDon()
    {
        Hazirla();
        hassasiyet = AyarKurallari.HassasiyetVarsayilan;
        ses = AyarKurallari.SesVarsayilan;
        ters = AyarKurallari.TersVarsayilan;
        gorus = AyarKurallari.GorusVarsayilan;
        AudioListener.volume = ses;
        Kaydet();
    }

    // ---------- KAYDET / YÜKLE ----------
    private static void Kaydet()
    {
        PlayerPrefs.SetFloat(HassasiyetAdi, hassasiyet);
        PlayerPrefs.SetFloat(SesAdi, ses);
        PlayerPrefs.SetInt(TersAdi, ters ? 1 : 0);   // bool doğrudan saklanamıyor; 1/0 olarak
        PlayerPrefs.SetFloat(GorusAdi, gorus);

        // Kaydırıcı sürüklenirken her karede diske yazmak gereksiz ama zararsız: Save
        // birkaç küçük değer yazar. Hemen yazmak, oyun çökse de ayarın kalmasını sağlar.
        PlayerPrefs.Save();

        Degisti?.Invoke();
    }

    private static void Hazirla()
    {
        if (hazir) return;
        hazir = true;

        // GetFloat(ad, varsayılan): kayıt yoksa ikinci değeri verir. Okunan değer yine
        // kuraldan geçer: kayıt bozuksa ya da sınırlar değiştiyse düzeltilir.
        hassasiyet = AyarKurallari.Hassasiyet(PlayerPrefs.GetFloat(HassasiyetAdi, AyarKurallari.HassasiyetVarsayilan));
        ses = AyarKurallari.Ses(PlayerPrefs.GetFloat(SesAdi, AyarKurallari.SesVarsayilan));
        ters = PlayerPrefs.GetInt(TersAdi, AyarKurallari.TersVarsayilan ? 1 : 0) == 1;
        gorus = AyarKurallari.Gorus(PlayerPrefs.GetFloat(GorusAdi, AyarKurallari.GorusVarsayilan));
    }

    // ---------- OYUN BAŞINDA ----------
    // Static değişkenler Play'ler arasında kalabildiği için her oynatmanın en başında
    // sıfırlanır (Dil.cs'teki gibi).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla()
    {
        hazir = false;
        Degisti = null;
    }

    // Sahne yüklenince ses seviyesini uygula. Hiçbir kod Ayarlar'a dokunmasa da oyun
    // kayıtlı seviyede başlasın; AudioListener.volume sahneden sahneye taşınır ama
    // oyun her açılışta 1'den başlar.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Baslat()
    {
        AudioListener.volume = Ses;
    }
}
