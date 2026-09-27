// MuzikDalgasi.cs
// Arka müziğin kendi kendine alçalıp yükselmesinin kuralı (#49). Şahan: "arkada gerilim
// müziğinin hiç eksik olmaması gerekiyor; ara ara yüksek, ara ara alçak."
//
// NASIL
// Müziğin seviyesi normal seviyenin bir çarpanıyla çalınır. Çarpan, periyotları birbirinin katı
// olmayan üç yavaş dalganın (sinüs) toplamından gelir: ~61, ~97 ve ~173 saniye. Katı olmadıkları
// için toplam hiçbir zaman aynı biçimde tekrar etmez; oyuncu dakikalar süren kabarma ve
// sönmeleri duyar ama bir sonrakinin ne zaman geleceğini kestiremez. Tohum her dalganın
// başlangıcını (evresini) kaydırır: her oyun farklı yerden başlar.
//
// Dalga anomaliye ya da olaylara bağlı DEĞİL (#40 kuralı: müzik cevap vermez). Olay kabarması
// ArkaMuzik'te bunun üstüne biner. Çarpan hiçbir zaman sıfıra inmez: müzik susmaz.
//
// Unity bilmez: saf C#, test edilir (MuzikDalgasiTestleri).

using System;

public class MuzikDalgasi
{
    /// <summary>Çarpanın alt ve üst sınırı (normal seviyenin katı).</summary>
    public const float EnAz = 0.35f, EnCok = 1.6f;

    /// <summary>Merdivende biraz yüksek, sahanlıkta ve dairede biraz alçak (kapıda dinlemek için).</summary>
    public const float MerdivenCarpani = 1.15f, DuzCarpani = 0.85f;

    private static readonly float[] Periyotlar = { 61f, 97f, 173f };   // saniye
    private static readonly float[] Agirliklar = { 0.25f, 0.35f, 0.40f };
    private readonly float[] evreler = new float[3];

    public MuzikDalgasi(int tohum)
    {
        var r = new Random(tohum);
        for (int i = 0; i < evreler.Length; i++) evreler[i] = (float)(r.NextDouble() * Math.PI * 2.0);
    }

    /// <summary>t saniyedeki dalga çarpanı: EnAz ile EnCok arasında.</summary>
    public float Carpan(float t)
    {
        // Her dalga -1..1 arası; ağırlıklı toplam da -1..1 arasında kalır (ağırlıkların toplamı 1).
        double toplam = 0;
        for (int i = 0; i < Periyotlar.Length; i++)
            toplam += Agirliklar[i] * Math.Sin(2.0 * Math.PI * t / Periyotlar[i] + evreler[i]);
        // -1..1'i 0..1'e, sonra EnAz..EnCok'a taşı.
        float oran = (float)((toplam + 1.0) / 2.0);
        return EnAz + (EnCok - EnAz) * oran;
    }

    /// <summary>Oyuncunun yerine göre ton farkı.</summary>
    public static float KonumCarpani(bool merdivende) { return merdivende ? MerdivenCarpani : DuzCarpani; }
}
