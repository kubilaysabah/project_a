// ParlamaKurali.cs
// Kafa lambasının ışığı bir görev eşyasına düşünce eşyanın ne kadar parlayacağı (#83).
//
// Neden var: Şahan'la yapılan denetimde küçük eşyalar (sandıktaki anahtar, büfedeki anahtar,
// kanepenin altındaki çizim parçası) kafa lambasının ışığında bile birkaç piksellik bir noktada
// kalıyordu. Gerçekte metal bir anahtar el fenerinin ışığını yakalar ve bir an "göz kırpar".
// Oyun bunu taklit eder: eşya lambanın ışık konisinin içindeyse küçük bir parıltı belirir.
//
// Kurallar:
//   - Eşya ışık konisinin dışındaysa (koninin yarı açısından büyük bir açıdaysa) parıltı yok.
//   - Menzilden uzaktaysa parıltı yok.
//   - Koninin ortasına ve oyuncuya yaklaştıkça güçlenir: oyuncu ışığı eşyaya tutup yürüdükçe
//     parıltı artar, böylece "orada bir şey var" duygusu yavaş yavaş gelir.
//   - Kırpışma: parıltı sabit bir ampul gibi durmaz; zamanla hafifçe dalgalanır (metalin ışığı
//     yakalaması gibi). Her eşyanın kendi fazı vardır, iki anahtar aynı anda kırpışmaz.
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir (ParlamaKuraliTestleri).
// Açıyı, mesafeyi ölçmek ve parıltıyı çizmek FenerParlamasi'nin işi.

using System;

public static class ParlamaKurali
{
    /// <summary>Parıltının görüldüğü en uzak mesafe (metre). Lambanın menzili 10 m ama küçük bir
    /// anahtarın ışığı o kadar uzaktan seçilmez; 5 m oda boyu.</summary>
    public const float Menzil = 5f;

    /// <summary>
    /// Parıltının gücü (0 = yok, 1 = en güçlü).
    /// aci: lambanın baktığı yön ile eşyaya giden yön arasındaki açı (derece).
    /// mesafe: lambadan eşyaya uzaklık (metre).
    /// koniYariAci: lamba ışığının dış konisinin yarısı (derece).
    /// menzil: bu mesafeden uzakta parıltı yok (metre).
    /// </summary>
    public static float Guc(float aci, float mesafe, float koniYariAci, float menzil)
    {
        // Geçersiz ölçü (sıfır ya da eksi koni, eksi mesafe) parıltı vermez; bir hata oyunu
        // bozmasın, yalnızca parıltı görünmesin.
        if (koniYariAci <= 0f || menzil <= 0f || mesafe < 0f || aci < 0f) return 0f;
        if (aci >= koniYariAci || mesafe >= menzil) return 0f;

        // Koninin içinde ne kadar ortada: 1 = tam ortada, 0 = koninin kenarında.
        float ortada = 1f - aci / koniYariAci;
        // Ne kadar yakın: 1 = lambanın dibinde, 0 = menzilin sınırında.
        float yakin = 1f - mesafe / menzil;
        // Karekök: kenara doğru yavaş sönsün (kenarda hemen kaybolmasın, belli belirsiz kalsın).
        return (float)Math.Sqrt(ortada) * yakin;
    }

    /// <summary>
    /// Kırpışma çarpanı (0,6 ile 1 arası): zamanla dalgalanır. faz her eşyada farklıdır.
    /// İki farklı hızda iki sinüs toplanır; tek sinüs düzenli bir "nefes" gibi okunurdu.
    /// </summary>
    public static float Kirpisma(float zaman, float faz)
    {
        double a = Math.Sin(zaman * 2.3 + faz);          // yavaş dalga
        double b = Math.Sin(zaman * 5.7 + faz * 1.7);    // hızlı dalga
        double toplam = (a + b) / 2.0;                   // -1 ile 1 arası
        return (float)(0.8 + 0.2 * toplam);              // 0,6 ile 1 arası
    }
}
