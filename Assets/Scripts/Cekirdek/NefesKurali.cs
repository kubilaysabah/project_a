// NefesKurali.cs
// Oyuncunun NEFES BUHARININ kuralı (#136, madde 6). Saf C# (Unity yok): NefesBuhari her kare sorar.
//
// NEDEN
// Şahan'ın 2026-10-08 test turu: korku oyuncunun bedeninde de görünmeli. 1994'te geceyarısı, kaloriferi
// sönmüş bir apartmanda nefes buğu olur; gerilim yükselince nefes sıklaşır. Fenerin ışığında beliren buğu
// hem "soğuk" der hem "korkuyorsun" der; oyuncu kendi nefesini görünce kendi korkusunu görür.
//
// KURAL
//   - Gerilim kademesi 0 iken (rahat) buhar yok; 1'de hafif, 2'de belirgin, av sürerken tam.
//   - Nefes dönemi (iki nefes arası) kademeyle kısalır: 3,8 → 3,0 → 2,2 sn; koşarken yarıdan az (×0,55).
//   - Saklanırken yok (dolabın içinde nefesini tutuyor), lamba yanmıyorken yok (ışıksız buğu görünmez;
//     parçacık boşa yayılmasın), inceleme ekranında yok (eşya kameraya yapışık, buğu üstüne binerdi).

public static class NefesKurali
{
    public const float DonemKademe1 = 3.8f;   // saniye: rahat ama tedirgin
    public const float DonemKademe2 = 3.0f;   // gergin
    public const float DonemAv = 2.2f;        // av / 3. kademe: soluk soluğa
    public const float KosmaCarpani = 0.55f;  // koşarken nefes sıklaşır
    public const float GucKademe1 = 0.45f;    // buharın gücü (yoğunluk 0-1)
    public const float GucKademe2 = 0.7f;
    public const float GucAv = 1f;

    /// <summary>İki nefes arası süre (saniye).</summary>
    public static float Donem(int kademe, bool kosuyor, bool avda)
    {
        float d = avda || kademe >= 3 ? DonemAv : (kademe >= 2 ? DonemKademe2 : DonemKademe1);
        return kosuyor ? d * KosmaCarpani : d;
    }

    /// <summary>Buharın gücü 0-1; 0 = nefes görünmez (bu kare püskürtülmez).</summary>
    public static float Guc(int kademe, bool avda, bool saklaniyor, bool lambaYaniyor, bool incelemede)
    {
        if (saklaniyor || !lambaYaniyor || incelemede) return 0f;
        if (avda || kademe >= 3) return GucAv;
        if (kademe >= 2) return GucKademe2;
        if (kademe >= 1) return GucKademe1;
        return 0f;
    }
}
