// ArkaMuzik.cs
// Oyun boyunca arkada çok kısık çalan gerilim müziği (#36) ve olaylı anomalilerden sonra
// kısa kabarması (#40).
//
// Müziğin kendisi bir AudioSource: 2B (kafanın içinden, yeri yok), döngüde, kısık.
// Bu bileşen üç şey yapar:
//   - Oyun başlayınca müziği başlatır.
//   - KABARMA (#40): olaylı bir anomali olduğunda (kapı çarpması, zil, göz kırpan ışık...)
//     KatYoneticisi Kabar()'ı çağırır. Müzik kısa bir gecikmeden sonra yükselir, birkaç
//     saniye tutar, yavaşça eski seviyesine iner. Olaysız, sessiz anomalilerde hiçbir şey
//     olmaz: müzik her anomali katında yükselseydi oyunun cevabını verirdi (Şahan'ın seçimi).
//   - Final gelince (çatı kapısı açılınca) müziği yavaşça susturur.
// Ceza karanlığında müzik diğer seslerle birlikte durur (AudioListener.pause).

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ArkaMuzik : MonoBehaviour
{
    // static: sahnede tek bir arka müzik var; FinalSahnesi ve KatYoneticisi ona bu yolla ulaşır.
    public static ArkaMuzik Ornek { get; private set; }

    // ---- KABARMA AYARLARI ----
    private const float KabarmaGecikmesi = 0.4f;   // etkinin kendi sesi önce duyulsun
    private const float KabarmaSeviyesi = 0.42f;   // normal seviye 0,15'in yaklaşık üç katı
    private const float TutmaSuresi = 3f;          // tepede kalma süresi
    private const float YukselmeHizi = 0.3f;       // saniyede ses seviyesi artışı (~1 sn'de tepe)
    private const float InmeHizi = 0.055f;         // saniyede azalış (~5 sn'de eski seviye)

    private AudioSource kaynak;
    private float taban;                // normal (kabarmamış) seviye; sahnede ayarlanan değer
    private float kabarmaBaslangici = -1f, kabarmaBitisi = -1f;
    private bool susturuluyor;

    // Sürekli dalgalanma (#49): seviye kendi kendine dakikalar süren kabarma ve sönmelerle değişir
    // (MuzikDalgasi, saf C#); merdivende biraz yüksek, sahanlıkta ve dairede biraz alçak.
    private MuzikDalgasi dalga;
    private Transform oyuncu;
    private float susturmaHizi;

    private void Awake()
    {
        Ornek = this;
        kaynak = GetComponent<AudioSource>();
        // Müzik mekânda değil, kafanın içinde: odanın yankısı (OdaYankisi, #87) ona işlemesin.
        if (kaynak != null) kaynak.bypassListenerEffects = true;
        taban = kaynak.volume;
        dalga = new MuzikDalgasi(System.Environment.TickCount);   // her oyun farklı yerden başlar
    }

    private void Start()
    {
        if (kaynak.clip != null && !kaynak.isPlaying) kaynak.Play();
    }

    // Oyuncu merdivende mi? Sahanlıklar ve daireler 3 metrenin katlarında (zemin -3, 1. kat 0, üst
    // sahanlık 3); arada kalan yükseklik merdivendir. Döngü ışınlaması y'yi 3'er kaydırdığı için
    // kalan (Repeat) her katta aynı çalışır.
    private bool Merdivende()
    {
        if (oyuncu == null)
        {
            BirinciSahisKontrolcu k = FindAnyObjectByType<BirinciSahisKontrolcu>();
            if (k == null) return false;
            oyuncu = k.transform;
        }
        float y = Mathf.Repeat(oyuncu.position.y, 3f);
        return y > 0.25f && y < 2.7f;
    }

    private void OnDestroy()
    {
        if (Ornek == this) Ornek = null;
    }

    /// <summary>
    /// Olaylı bir anomali oldu: müzik kısa süre kabarsın. Kabarma sürerken yeniden
    /// çağrılırsa (art arda etkiler) yeniden başlamaz, yalnızca tepede kalma süresi uzar.
    /// </summary>
    public void Kabar()
    {
        if (susturuluyor) return;
        float simdi = Time.time;
        if (simdi > kabarmaBitisi) kabarmaBaslangici = simdi + KabarmaGecikmesi;   // yeni bir kabarma
        kabarmaBitisi = simdi + KabarmaGecikmesi + TutmaSuresi;
    }

    /// <summary>Müziği 'sure' saniyede yavaşça kısıp durdurur (final).</summary>
    public void Sustur(float sure)
    {
        susturuluyor = true;
        susturmaHizi = kaynak.volume / Mathf.Max(0.01f, sure);
    }

    private void Update()
    {
        if (susturuluyor)
        {
            kaynak.volume = Mathf.MoveTowards(kaynak.volume, 0f, susturmaHizi * Time.deltaTime);
            if (kaynak.volume <= 0f && kaynak.isPlaying) kaynak.Stop();
            return;
        }

        // Hedef seviye: kabarma penceresinin içindeysek yüksek, değilsek normal.
        float simdi = Time.time;
        bool kabarik = simdi >= kabarmaBaslangici && simdi < kabarmaBitisi;
        // Olay kabarması dalganın üstüne biner; kabarma yokken dalga ve oyuncunun yeri belirler.
        float hedef = kabarik ? KabarmaSeviyesi : taban * dalga.Carpan(Time.time) * MuzikDalgasi.KonumCarpani(Merdivende());
        hedef *= Ayarlar.Muzik;   // #118: müzik kanalı (0 = sessiz); genel seviye AudioListener.volume'da
        // MoveTowards: değeri hedefe doğru en fazla verilen adım kadar yaklaştırır.
        // Yükselirken hızlı, inerken yavaş: korku ani gelir, yavaş geçer.
        float hiz = hedef > kaynak.volume ? YukselmeHizi : InmeHizi;
        kaynak.volume = Mathf.MoveTowards(kaynak.volume, hedef, hiz * Time.deltaTime);
    }
}
