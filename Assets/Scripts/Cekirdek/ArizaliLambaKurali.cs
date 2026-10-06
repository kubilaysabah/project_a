// ArizaliLambaKurali.cs
// Arızalı bir ampulün yanıp sönme deseni (#111). Saf C#: Unity'ye dokunmaz, test edilir (ArizaliLambaKuraliTestleri).
//
// NEDEN
// Şahan'ın testi (2026-10-06): kafa lambası dokuzuncu katta çiviye asılınca 8. ve 9. katın sahanlığı tamamen
// karanlığa gömülüyordu. Asansörün üstüne arızalı bir lamba kondu: yanar, söner, kekeler. Hem yolu gösterir hem
// sahnenin gerilimine uyar. Desen her karede yeniden hesaplanır; "şimdiki parlaklık" yalnızca zamana bağlıdır.
// Böylece hiçbir şey hatırlanmaz, kaydedilmez, aynı an her zaman aynı görünür (testte de oyunda da).
//
// NASIL
// Zaman 1,1 saniyelik BLOKLARA bölünür; her bloğun huyu zar atılarak seçilir:
//   - sönük blok (%22): neredeyse hep karanlık, ara ara bir kıvılcım;
//   - kekeleme bloğu (%28): 70 ms'lik adımlarda yanıp söner;
//   - yanık blok (%50): hep yanar, ara ara bir an göz kırpar.
// "Zar" gerçek rastgelelik değildir: sayıyı karıştıran bir formüldür (hash). Aynı sayı her zaman aynı sonucu verir.
// UnityEngine.Random kullanılmaz: o her çağrıda değişir, burada ise zamanın bir FONKSİYONU istiyoruz.

public static class ArizaliLambaKurali
{
    public const float BlokSuresi = 1.1f;    // bir huyun sürdüğü süre (saniye)
    public const float AdimSuresi = 0.07f;   // kekeleme adımı (saniye): göz bunu "titreme" olarak görür

    /// <summary>
    /// t anındaki parlaklık, 0 (sönük) ile 1 (tam yanık) arası. tohum: iki lamba aynı anda aynı şeyi yapmasın diye
    /// her lambaya ayrı bir sayı. Bozuk zaman (NaN, sonsuz) sönük döner.
    /// </summary>
    public static float Parlaklik(float t, int tohum = 0)
    {
        // float.IsNaN: "sayı değil" mi? Böyle bir zamanla hesap anlamsız; lamba sönük kalır.
        if (float.IsNaN(t) || float.IsInfinity(t)) return 0f;
        if (t < 0f) t = -t;                                  // negatif zaman: aynanın aynısı (tanımsız bırakmayalım)
        int blok = (int)(t / BlokSuresi);                    // kaçıncı bloktayız
        int adim = (int)(t / AdimSuresi);                    // kaçıncı 70 ms'lik adımdayız
        float huy = Zar(blok * 7919 + tohum * 131);          // bloğun huyu
        float an = Zar(adim * 104729 + tohum * 31 + 1);      // bu adımın zarı
        if (huy < 0.22f) return an < 0.06f ? 0.6f : 0f;      // sönük blok: yüzde 6 ihtimalle bir kıvılcım
        if (huy < 0.50f) return an < 0.5f ? 1f : 0f;         // kekeleme: yarı yarıya yanıp söner
        return an < 0.05f ? 0.15f : 1f;                      // yanık blok: yüzde 5 ihtimalle bir an kararır
    }

    // Bir tamsayıyı 0-1 arası "rastgele görünen" bir sayıya çevirir (hash). unchecked: çarpma taşarsa hata verme,
    // taşan bitler atılsın; karıştırmanın kendisi zaten bu taşmaya dayanır.
    private static float Zar(int n)
    {
        unchecked
        {
            uint h = (uint)n * 2654435761u;   // büyük bir tek sayıyla çarp: bitler dağılır
            h ^= h >> 15;                     // üst bitleri alta katla
            h *= 2246822519u;
            h ^= h >> 13;
            return (h & 0xFFFFFF) / 16777216f;   // alttaki 24 biti 0-1 aralığına böl
        }
    }
}
