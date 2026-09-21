// KatIlerlemeTakipcisi.cs
// Oyuncunun ne zaman bir kat yukarı ya da aşağı GERÇEKTEN gittiğini söyler.
//
// NEDEN BÖYLE BİR ŞEYE İHTİYAÇ VAR
// Kararı ışınlanma anına bağlamıştık ve bu bozuktu. Işınlanma eşikleri
// mekânın geometrisine göre yerleştirilmiş: yukarı eşiği varış noktasından
// 6,92 m uzakta ama aşağı eşiği sadece 0,38 m uzakta. Sonuç: oyuncu arkasını
// dönüp birkaç adım atınca bir "geri dön" kararı kaydediliyor, yürümeye devam
// ettikçe ikincisi, üçüncüsü geliyordu. 1. kattan geri dönen oyuncu 3. katta
// buluyordu kendini.
//
// ÇÖZÜM
// Işınlanma bir GÖRÜNTÜ hilesidir; kararla hiçbir ilgisi olmamalı.
// Karar, oyuncunun gerçekten kat ettiği DİKEY MESAFEYE bakmalı.
// Bir kat yüksekliği kadar çıktıysa "yukarı", bir kat kadar indiyse "geri".
// Aşağıdaki sınıf yalnızca bunu yapar ve Unity'den hiç haberi yoktur.

public class KatIlerlemeTakipcisi
{
    // Ondalık sayılarda küçük yuvarlama hataları kaçınılmazdır: bilgisayar
    // 3,0'ı bazen 2,9999997 olarak tutar. Tolerans olmadan oyuncu tam bir kat
    // çıktığı halde karar oluşmayabilir ve sahanlıkta kilitli kalır.
    // 5 mm, hatayı yutacak kadar büyük, erken karar üretemeyecek kadar küçük.
    private const float Tolerans = 0.005f;

    // Bir kat sayılması için gereken dikey mesafe (metre).
    private readonly float katYuksekligi;

    // Son kararın verildiği andaki kot. Yeni kararlar buna göre ölçülür.
    private float sonKararKotu;

    /// <summary>Takipçiyi oyuncunun o anki kotuyla başlatır.</summary>
    public KatIlerlemeTakipcisi(float katYuksekligi, float baslangicKotu)
    {
        this.katYuksekligi = katYuksekligi;
        sonKararKotu = baslangicKotu;
    }

    /// <summary>Son karardan bu yana kat edilen dikey mesafe.</summary>
    public float SonKarardanBeriDegisim(float kot) => kot - sonKararKotu;

    /// <summary>
    /// Ceza ve benzeri ara tetikler için: son karardan bu yana
    /// alınan yolun mutlak büyüklüğü.
    /// </summary>
    public float SonKarardanBeriMesafe(float kot)
    {
        float d = kot - sonKararKotu;
        return d < 0f ? -d : d;
    }

    /// <summary>
    /// Verilen kotta yeni bir karar oluşup oluşmadığını söyler.
    /// Oluştuysa true döner ve kararı "karar" değişkenine yazar.
    ///
    /// "out" demek: fonksiyon bu değişkenin içini doldurup bize geri verir.
    /// Bir fonksiyondan hem "oldu mu" hem "ne oldu" bilgisini birlikte
    /// döndürmenin en sade yolu budur.
    /// </summary>
    public bool KararOlustuMu(float kot, out Karar karar)
    {
        float degisim = kot - sonKararKotu;

        if (degisim >= katYuksekligi - Tolerans)
        {
            karar = Karar.Yukari;

            // Sayacı TAM bir kat ileri alıyoruz, oyuncunun bulunduğu kota değil.
            // Böylece her katta biriken küçük taşmalar üst üste eklenip
            // zamanla kararları kaydırmaz.
            sonKararKotu += katYuksekligi;
            return true;
        }

        if (degisim <= -(katYuksekligi - Tolerans))
        {
            karar = Karar.Geri;
            sonKararKotu -= katYuksekligi;
            return true;
        }

        // Karar yok. "default" bir değer yazmak zorundayız, yoksa derleyici
        // "karar doldurulmamış olabilir" diye uyarır.
        karar = Karar.Yukari;
        return false;
    }

    /// <summary>Ceza sonrası gibi durumlarda sayacı verilen kota sıfırlar.</summary>
    public void Sifirla(float kot)
    {
        sonKararKotu = kot;
    }
}
