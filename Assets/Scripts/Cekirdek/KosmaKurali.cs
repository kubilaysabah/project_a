// KosmaKurali.cs
// Koşma ve nefes (stamina) kuralı (#129). Saf C# (Unity yok): BirinciSahisKontrolcu her kare çağırır.
//
// NEDEN: oyuncu 1,8 m/sn yürüyor, Gölge avda 2,0 m/sn: "kaçamıyorum" hissi tasarımın bir parçasıydı ama
// muadil oyunlarda (Chilla's Art) kısa bir koşu var ve oyuncu şikâyeti "koşamıyorum" oluyor. Kural: Shift
// (kolda sol çubuğa basmak) basılıyken 2,7 m/sn, ama nefes DoluSure (4,5 sn) sonra biter; sonra yeniden
// koşabilmek için nefesin en az YenidenEsik'e (0,3) dolması gerekir (histerezis: tuşu basılı tutan oyuncu
// 0 ile 0,01 arasında titremesin). Nefes DolmaSuresi'nde (7 sn) dolar. Koşmak gürültüdür (Varlik).
//
// SAYILAR: 4,5 sn × 2,7 m/sn ≈ 12 m koşu: bir kattan merdivene yeter, bütün katı koşarak geçmeye yetmez.

public class KosmaKurali
{
    public const float KosmaHizi = 2.7f;      // metre/saniye (yürüme 1,8; Gölge 2,0)
    public const float DoluSure = 4.5f;       // tam nefesle koşulan süre (saniye)
    public const float DolmaSuresi = 7f;      // boş nefesin dolma süresi (saniye)
    public const float YenidenEsik = 0.3f;    // tükenince yeniden koşabilmek için gereken nefes

    private float nefes = 1f;
    private bool tukendi;

    /// <summary>Nefes 0-1 (1 = dolu).</summary>
    public float Nefes { get { return nefes; } }
    /// <summary>Nefes bitti, eşiğe dolana kadar koşulamaz.</summary>
    public bool Tukendi { get { return tukendi; } }
    /// <summary>Son Ilerlet'te koşuldu mu.</summary>
    public bool Kosuyor { get; private set; }

    /// <summary>
    /// Her kare: gecen saniye, istek = koşma tuşu basılı, hareketli = oyuncu yürüyor (duran oyuncu koşmaz,
    /// nefes de harcamaz), egik = çömelmiş (eğikken koşulmaz). Dönüş: bu karede koşuluyor mu.
    /// </summary>
    public bool Ilerlet(float gecen, bool istek, bool hareketli, bool egik)
    {
        if (gecen < 0f) gecen = 0f;
        bool kosabilir = istek && hareketli && !egik && !tukendi && nefes > 0f;
        if (kosabilir)
        {
            nefes -= gecen / DoluSure;
            if (nefes <= 0f) { nefes = 0f; tukendi = true; kosabilir = false; }
        }
        else
        {
            nefes += gecen / DolmaSuresi;
            if (nefes > 1f) nefes = 1f;
            if (tukendi && nefes >= YenidenEsik) tukendi = false;
        }
        Kosuyor = kosabilir;
        return kosabilir;
    }

    /// <summary>Nefesi doldurur (kat başı, yeniden doğuş).</summary>
    public void Doldur() { nefes = 1f; tukendi = false; Kosuyor = false; }
}
