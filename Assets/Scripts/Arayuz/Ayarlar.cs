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
//   Görüntü (#30)      → GorunumUygulayici: çözünürlük, ekran modu, V-Sync, kalite, parlaklık

using System;
using UnityEngine;

public static class Ayarlar
{
    // Ayar dosyasındaki (KayitDeposu.Ayar: ayarlar.txt, #122; eskiden PlayerPrefs) adlar. "ayar_dil" Dil.cs'te.
    private const string HassasiyetAdi = "ayar_hassasiyet";
    private const string SesAdi = "ayar_ses";
    private const string MuzikAdi = "ayar_muzik";       // #118: müzik kanalı
    private const string KonusmaAdi = "ayar_konusma";   // #118: konuşma kanalı (telefon, radyo)
    private const string TersAdi = "ayar_ters";
    private const string SallanmaAdi = "ayar_sallanma";   // kafa sallanması (#92)
    private const string TitresimAdi = "ayar_titresim";   // oyun kolu titreşimi (#124)
    private const string GorusAdi = "ayar_gorus";
    private const string KaliteAdi = "ayar_kalite";
    private const string ParlaklikAdi = "ayar_parlaklik";
    private const string TamEkranAdi = "ayar_tamekran";
    private const string VSyncAdi = "ayar_vsync";
    private const string GenislikAdi = "ayar_cozunurluk_g";
    private const string YukseklikAdi = "ayar_cozunurluk_y";

    private static float hassasiyet, ses, gorus, parlaklik;
    private static float muzik = AyarKurallari.MuzikVarsayilan, konusma = AyarKurallari.KonusmaVarsayilan;
    private static bool ters, tamEkran, vsync;
    private static bool sallanma = AyarKurallari.SallanmaVarsayilan;
    private static bool titresim = AyarKurallari.TitresimVarsayilan;
    private static int kalite, genislik, yukseklik;
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

    /// <summary>Müzik kanalı (0–1), genel seviyeyle çarpılır; ArkaMuzik okur (#118).</summary>
    public static float Muzik
    {
        get { Hazirla(); return muzik; }
        set { Hazirla(); muzik = AyarKurallari.Muzik(value); Kaydet(); }
    }

    /// <summary>Konuşma kanalı (0–1): telefon ve radyo seslendirmeleri (#118).</summary>
    public static float Konusma
    {
        get { Hazirla(); return konusma; }
        set { Hazirla(); konusma = AyarKurallari.Konusma(value); Kaydet(); }
    }

    /// <summary>Fare yukarı itilince aşağı bakılsın mı.</summary>
    /// <summary>Yürürken kafa sallanması (ve kamera sarsıntısı) açık mı? (#92, KameraHareketi okur)</summary>
    public static bool Sallanma
    {
        get { Hazirla(); return sallanma; }
        set { Hazirla(); sallanma = value; Kaydet(); }
    }

    /// <summary>Oyun kolu titreşimi (#124): av, kapı çarpması, gök gürültüsü, yakalanma. Kapalıysa Titresim.Vur hiç titretmez.</summary>
    public static bool Titresim
    {
        get { Hazirla(); return titresim; }
        set { Hazirla(); titresim = value; Kaydet(); }
    }

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

    // ---------- GÖRÜNTÜ (#30) ----------

    /// <summary>Grafik kalitesi: 0 Düşük, 1 Orta, 2 Yüksek.</summary>
    public static int Kalite
    {
        get { Hazirla(); return kalite; }
        set { Hazirla(); kalite = AyarKurallari.Kalite(value); Kaydet(); }
    }

    /// <summary>Parlaklık (pozlamaya eklenen değer, -1…+1).</summary>
    public static float Parlaklik
    {
        get { Hazirla(); return parlaklik; }
        set { Hazirla(); parlaklik = AyarKurallari.Parlaklik(value); Kaydet(); }
    }

    /// <summary>Tam ekran mı (değilse pencere).</summary>
    public static bool TamEkran
    {
        get { Hazirla(); return tamEkran; }
        set { Hazirla(); tamEkran = value; Kaydet(); }
    }

    /// <summary>V-Sync açık mı.</summary>
    public static bool VSync
    {
        get { Hazirla(); return vsync; }
        set { Hazirla(); vsync = value; Kaydet(); }
    }

    /// <summary>Seçilen çözünürlüğün genişliği; 0 = hiç seçilmedi (ekranın kendi çözünürlüğü).</summary>
    public static int CozunurlukGenislik { get { Hazirla(); return genislik; } }

    /// <summary>Seçilen çözünürlüğün yüksekliği; 0 = hiç seçilmedi.</summary>
    public static int CozunurlukYukseklik { get { Hazirla(); return yukseklik; } }

    /// <summary>Çözünürlüğü seçer (iki sayı birlikte değiştiği için özellik değil fonksiyon).</summary>
    public static void CozunurlukAyarla(int yeniGenislik, int yeniYukseklik)
    {
        Hazirla();
        genislik = yeniGenislik;
        yukseklik = yeniYukseklik;
        Kaydet();
    }

    /// <summary>Görüntü ayarlarını oyunun ayarlandığı hâle döndürür (oyun ayarlarına dokunmaz).</summary>
    public static void GoruntuVarsayilanaDon()
    {
        Hazirla();
        kalite = AyarKurallari.KaliteVarsayilan;
        parlaklik = AyarKurallari.ParlaklikVarsayilan;
        tamEkran = AyarKurallari.TamEkranVarsayilan;
        vsync = AyarKurallari.VSyncVarsayilan;
        genislik = 0;          // ekranın kendi çözünürlüğü
        yukseklik = 0;
        Kaydet();
    }

    /// <summary>Oyun ayarlarını (hassasiyet, ses, ters, kafa sallanması, görüş) oyunun ayarlandığı hâle döndürür.</summary>
    public static void VarsayilanaDon()
    {
        Hazirla();
        hassasiyet = AyarKurallari.HassasiyetVarsayilan;
        ses = AyarKurallari.SesVarsayilan;
        muzik = AyarKurallari.MuzikVarsayilan;
        konusma = AyarKurallari.KonusmaVarsayilan;
        ters = AyarKurallari.TersVarsayilan;
        sallanma = AyarKurallari.SallanmaVarsayilan;
        titresim = AyarKurallari.TitresimVarsayilan;
        gorus = AyarKurallari.GorusVarsayilan;
        AudioListener.volume = ses;
        Kaydet();
    }

    // ---------- KAYDET / YÜKLE ----------
    private static void Kaydet()
    {
        KayitDeposu.Ayar.SetFloat(HassasiyetAdi, hassasiyet);
        KayitDeposu.Ayar.SetFloat(SesAdi, ses);
        KayitDeposu.Ayar.SetFloat(MuzikAdi, muzik);
        KayitDeposu.Ayar.SetFloat(KonusmaAdi, konusma);
        KayitDeposu.Ayar.SetInt(TersAdi, ters ? 1 : 0);   // bool doğrudan saklanamıyor; 1/0 olarak
        KayitDeposu.Ayar.SetInt(SallanmaAdi, sallanma ? 1 : 0);
        KayitDeposu.Ayar.SetInt(TitresimAdi, titresim ? 1 : 0);
        KayitDeposu.Ayar.SetFloat(GorusAdi, gorus);
        KayitDeposu.Ayar.SetInt(KaliteAdi, kalite);
        KayitDeposu.Ayar.SetFloat(ParlaklikAdi, parlaklik);
        KayitDeposu.Ayar.SetInt(TamEkranAdi, tamEkran ? 1 : 0);
        KayitDeposu.Ayar.SetInt(VSyncAdi, vsync ? 1 : 0);
        KayitDeposu.Ayar.SetInt(GenislikAdi, genislik);
        KayitDeposu.Ayar.SetInt(YukseklikAdi, yukseklik);

        // Kaydırıcı sürüklenirken her karede diske yazmak gereksiz ama zararsız: Save
        // birkaç küçük değer yazar. Hemen yazmak, oyun çökse de ayarın kalmasını sağlar.
        KayitDeposu.Ayar.Save();

        Degisti?.Invoke();
    }

    private static void Hazirla()
    {
        if (hazir) return;
        hazir = true;

        // GetFloat(ad, varsayılan): kayıt yoksa ikinci değeri verir. Okunan değer yine
        // kuraldan geçer: kayıt bozuksa ya da sınırlar değiştiyse düzeltilir.
        hassasiyet = AyarKurallari.Hassasiyet(KayitDeposu.Ayar.GetFloat(HassasiyetAdi, AyarKurallari.HassasiyetVarsayilan));
        ses = AyarKurallari.Ses(KayitDeposu.Ayar.GetFloat(SesAdi, AyarKurallari.SesVarsayilan));
        muzik = AyarKurallari.Muzik(KayitDeposu.Ayar.GetFloat(MuzikAdi, AyarKurallari.MuzikVarsayilan));
        konusma = AyarKurallari.Konusma(KayitDeposu.Ayar.GetFloat(KonusmaAdi, AyarKurallari.KonusmaVarsayilan));
        ters = KayitDeposu.Ayar.GetInt(TersAdi, AyarKurallari.TersVarsayilan ? 1 : 0) == 1;
        sallanma = KayitDeposu.Ayar.GetInt(SallanmaAdi, AyarKurallari.SallanmaVarsayilan ? 1 : 0) == 1;
        titresim = KayitDeposu.Ayar.GetInt(TitresimAdi, AyarKurallari.TitresimVarsayilan ? 1 : 0) == 1;
        gorus = AyarKurallari.Gorus(KayitDeposu.Ayar.GetFloat(GorusAdi, AyarKurallari.GorusVarsayilan));
        kalite = AyarKurallari.Kalite(KayitDeposu.Ayar.GetInt(KaliteAdi, AyarKurallari.KaliteVarsayilan));
        parlaklik = AyarKurallari.Parlaklik(KayitDeposu.Ayar.GetFloat(ParlaklikAdi, AyarKurallari.ParlaklikVarsayilan));
        tamEkran = KayitDeposu.Ayar.GetInt(TamEkranAdi, AyarKurallari.TamEkranVarsayilan ? 1 : 0) == 1;
        vsync = KayitDeposu.Ayar.GetInt(VSyncAdi, AyarKurallari.VSyncVarsayilan ? 1 : 0) == 1;
        genislik = KayitDeposu.Ayar.GetInt(GenislikAdi, 0);
        yukseklik = KayitDeposu.Ayar.GetInt(YukseklikAdi, 0);
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
