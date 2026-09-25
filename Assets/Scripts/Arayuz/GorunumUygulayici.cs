// GorunumUygulayici.cs
// Oyuncunun görüntü ayarlarını uygular (#30): grafik kalitesi, parlaklık, V-Sync,
// çözünürlük ve ekran modu. Sahnedeki "Atmosfer" nesnesinde (gece post-processing'i
// taşıyan Volume'un yanında) durur; AtmosferKurucu takar ve kalite dosyalarını bağlar.
//
// Değerler Ayarlar'da saklanıyor; bu bileşen oyun başlarken ve her Ayarlar.Degisti
// haberinde onları Unity'ye uygular.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class GorunumUygulayici : MonoBehaviour
{
    // Üç kalite seviyesinin URP ayar dosyası: [0] Düşük, [1] Orta, [2] Yüksek.
    // KaliteKurucu üretir, AtmosferKurucu buraya bağlar.
    [SerializeField] private RenderPipelineAsset[] kaliteler = new RenderPipelineAsset[3];

    private ColorAdjustments renkAyari;   // gece profilinin çalışma anı kopyasındaki renk ayarı
    private float temelPozlama;           // profilin kendi pozlaması (parlaklık buna eklenir)

    // Son uygulanan ekran ayarı. Static: final sonrası sahne yeniden yüklenince bileşen
    // baştan kurulur ama ekran zaten o ayarda; tekrar uygulamak ekranı boşuna titretirdi.
    private static int uygulananGenislik = -1, uygulananYukseklik = -1;
    private static bool uygulananTamEkran;

    private void Start()
    {
        // .profile (sharedProfile DEĞİL): Unity oyun sırasında profilin bir KOPYASINI
        // üretip onu verir. Parlaklık bu kopyaya yazılır; Assets/Settings/Atmosfer_Gece
        // dosyası hiç değişmez. sharedProfile'a yazsaydık Editor'de oynarken proje
        // dosyası kalıcı olarak değişirdi.
        Volume hacim = GetComponent<Volume>();
        if (hacim.profile.TryGet(out renkAyari)) temelPozlama = renkAyari.postExposure.value;

        Uygula();
        Ayarlar.Degisti += Uygula;
    }

    private void OnDestroy()
    {
        Ayarlar.Degisti -= Uygula;
    }

    private void Uygula()
    {
        // ---- KALİTE ----
        // QualitySettings.renderPipeline: şu anki kalite seviyesinin kullandığı URP dosyası.
        // Yalnızca değiştiyse atanır; her atama URP'nin kendini yeniden kurmasına yol açar.
        int k = Ayarlar.Kalite;
        if (kaliteler != null && k >= 0 && k < kaliteler.Length && kaliteler[k] != null
            && QualitySettings.renderPipeline != kaliteler[k])
        {
            QualitySettings.renderPipeline = kaliteler[k];
        }

        // ---- PARLAKLIK ----
        // postExposure: pozlama, "EV" cinsinden. +1 görüntüyü iki kat aydınlatır.
        if (renkAyari != null) renkAyari.postExposure.value = temelPozlama + Ayarlar.Parlaklik;

        // ---- V-SYNC ----
        // vSyncCount 1: her monitör yenilemesinde bir kare. 0: beklemeden çiz.
        // Kapalıyken kare hızı sınırlanır (nedeni AyarKurallari.VSyncKapaliKareSiniri'nda).
        QualitySettings.vSyncCount = Ayarlar.VSync ? 1 : 0;
        Application.targetFrameRate = Ayarlar.VSync ? -1 : AyarKurallari.VSyncKapaliKareSiniri;

        // ---- ÇÖZÜNÜRLÜK VE EKRAN MODU ----
        // 0 = oyuncu hiç seçmedi: ekranın kendi çözünürlüğü (Screen.currentResolution
        // pencere modunda bile masaüstünün çözünürlüğünü verir).
        int g = Ayarlar.CozunurlukGenislik, y = Ayarlar.CozunurlukYukseklik;
        if (g <= 0 || y <= 0) { g = Screen.currentResolution.width; y = Screen.currentResolution.height; }
        bool tam = Ayarlar.TamEkran;

        // Yalnızca değiştiyse: ses kaydırıcısı oynatılırken de Degisti geliyor; her seferinde
        // SetResolution çağırmak ekranı titretirdi.
        if (g != uygulananGenislik || y != uygulananYukseklik || tam != uygulananTamEkran)
        {
            // FullScreenWindow: kenarlıksız tam ekran (alt-tab hızlı). Windowed: pencere.
            // Not: Editor'ün Game penceresinde bu çağrı hiçbir şey yapmaz; build'de çalışır.
            Screen.SetResolution(g, y, tam ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            uygulananGenislik = g;
            uygulananYukseklik = y;
            uygulananTamEkran = tam;
            Debug.Log($"[GÖRÜNTÜ] {g}x{y} {(tam ? "tam ekran" : "pencere")}, kalite {k}, V-Sync {(Ayarlar.VSync ? "açık" : "kapalı")}, parlaklık {Ayarlar.Parlaklik:+0.0;-0.0;0.0}");
        }
    }

    // Her oynatmanın başında static'leri sıfırla (Dil.cs'teki gibi).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla()
    {
        uygulananGenislik = -1;
        uygulananYukseklik = -1;
#if UNITY_EDITOR
        // Editor'de oynarken değiştirilen kalite ve V-Sync, proje ayarlarında (Quality
        // Settings) kalıcı kalmasın: Play'den çıkarken eski değerlerine döndürülür.
        // Yoksa Editor'de denenen "Düşük" kalite bir sonraki kayıtta proje dosyasına
        // yazılır ve commit'lere sızardı. Build'de bu blok yok.
        editordekiRP = QualitySettings.renderPipeline;
        editordekiVSync = QualitySettings.vSyncCount;
        editordekiKareSiniri = Application.targetFrameRate;
        Application.quitting -= EditorAyarlariniGeriAl;
        Application.quitting += EditorAyarlariniGeriAl;   // Editor'de Play'den çıkış da "quitting" sayılır
#endif
    }

#if UNITY_EDITOR
    private static RenderPipelineAsset editordekiRP;
    private static int editordekiVSync, editordekiKareSiniri;

    private static void EditorAyarlariniGeriAl()
    {
        QualitySettings.renderPipeline = editordekiRP;
        QualitySettings.vSyncCount = editordekiVSync;
        Application.targetFrameRate = editordekiKareSiniri;
        Application.quitting -= EditorAyarlariniGeriAl;
    }
#endif
}
