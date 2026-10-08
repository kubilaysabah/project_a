// FenerTitremeKurali.cs
// Kafa lambasının ELİN TİTREMESİ gibi titremesinin kuralı (#136, madde 12). Saf C# (Unity yok).
//
// NEDEN
// Korkan bir insanın eli titrer; kafaya takılı bir lambada bu, huzmenin duvarda ince ince oynamasıdır.
// Görüşün kendisi titremez (mide bulandırır, #91'in sarsıntısı zaten var); yalnızca ışığın yönü titrer.
// Gerilim 2. kademede başlar, av sürerken artar; rahatken sıfırdır. "Kafa sallanması" ayarı kapalıysa
// (hareket hassasiyeti) bu da kapalıdır; o karar KameraHareketi'nde verilir, bu kural yalnız sayıyı verir.
//
// SAYILAR: insan elinin tremor sıklığı 4-12 Hz; 7 Hz'lik Perlin gürültüsü (düzgün rastgele dalga) ile
// ±genlik derece. 2. kademede 0,5°, avda 1,2°: 3 m'deki duvarda 2,6 / 6,3 cm'lik oynama.

public static class FenerTitremeKurali
{
    public const float GenlikKademe2 = 0.5f;   // derece
    public const float GenlikAv = 1.2f;        // derece
    public const float Frekans = 7f;           // Hz (Perlin "dalga" sayısı / saniye)
    public const float GecisHizi = 1.5f;       // derece / saniye: genlik bir anda değil yumuşakça değişir

    /// <summary>Hedef genlik (derece): kademe &lt; 2 ise 0.</summary>
    public static float Genlik(int kademe, bool avda)
    {
        if (avda || kademe >= 3) return GenlikAv;
        if (kademe >= 2) return GenlikKademe2;
        return 0f;
    }
}
