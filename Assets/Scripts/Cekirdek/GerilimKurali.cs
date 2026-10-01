// GerilimKurali.cs
// Binadaki VARLIĞIN kuralı (#86): oyuncunun gürültüsü ve oyalanması gerilimi yükseltir, gerilim
// yükseldikçe bina tepki verir, tepe noktada kısa bir av başlar.
//
// Şahan'ın isteği (2026-10-01, No70 / Phasmophobia karşılaştırması): "1-5. katlarda hiçbir tehdit
// yok; oyuncu kendisine bir şey olmayacağını anlayınca oyun bulmacaya dönüşüyor." Phasmophobia'nın
// özü, kuralları olan ama ne yapacağı belli olmayan bir varlıktır: ses çıkarırsan, oyalanırsan
// avlanırsın. Bu dosya o kuralın "ne kadar kızgın?" sorusunu cevaplar.
//
// GERİLİM 0 ile 1 arasında bir sayıdır (bir tencerenin altındaki ateş gibi düşün):
//   - Dairenin içinde lamba açıkken zaman geçtikçe yavaşça ısınır; yürürken biraz daha hızlı.
//   - Gürültü (kavanoz kırmak, yanlış kapı çalmak, kapı devirmek) bir anda ısıtır (Gurultu).
//   - Sahanlıkta ve merdivende yavaşça soğur; dairede çömelip lambayı kapatıp durunca hızlı soğur
//     (saklanmak = ateşi kısmak). Yalnızca çömelmek ya da yalnızca lambayı kapatmak az soğutur.
//
// KADEMELER (eşikler): 0 sakin → 1 huzursuz (lambalar titrer, üst kattan adımlar) → 2 yakın
// (Gölge belirir, kapı tıklatılır, lamba söner) → 3 AV. Bir kademeye GİRİLDİĞİNDE o kademenin giriş
// tepkisi döner; 1. ve 2. kademede kalındıkça 25-40 saniyede bir tekrar tepkisi döner. Av bitince
// (kaçtı ya da yakalandı) gerilim sıfırlanır (AvBitti).
//
// KAT ÇARPANI: alt katlar yavaş ısınır, üst katlar hızlı; 1. katta av yoktur (öğretici kat; en çok
// 2. kademe). 6. ve 7. katın kendi senaryolu anları (#58) sürerken sahne tarafı kuralı durdurur.
//
// Burada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir (GolgeZamanlayici gibi).
// Tepkileri sahnede gösteren bileşen Varlik'tır; burası yalnızca "şimdi ne olsun?" sorusunu
// cevaplar.

/// <summary>Kuralın sahne tarafından istediği tepki.</summary>
public enum GerilimTepkisi
{
    Yok,
    Titreme,          // kafa lambası ve dairenin lambaları titrer, müzik hafif kabarır (1. kademe)
    UstKatAdimlari,   // üst kattan ağır adımlar (1. kademe tekrarı)
    Golge,            // Gezen Gölge koridorun ucunda belirir (2. kademe)
    Tiklatma,         // kapalı bir oda kapısı tıklatılır (2. kademe tekrarı)
    LambaSonme,       // kafa lambası kısa süre söner (2. kademe tekrarı)
    Av,               // Gölge oyuncunun izini sürer (3. kademe)
    GolgeIzi,         // duvara odada olmayan birinin gölgesi düşer (1-2. kademe tekrarı, #93); SONA eklendi
}

public class GerilimKurali
{
    // ---------- EŞİKLER ----------
    /// <summary>Kademe eşikleri: bu değerin üstü o kademedir.</summary>
    public const float TitremeEsigi = 0.30f, GolgeEsigi = 0.60f, AvEsigi = 0.85f;

    // ---------- HIZLAR (saniyede) ----------
    /// <summary>Dairede, lamba açık, ayakta, duruyor: 0'dan 1'e 285 saniye (ışık varlığı çeker).</summary>
    public const float TabanHiz = 0.0035f;
    /// <summary>Yürürken eklenen hız (ayak sesi).</summary>
    public const float HareketEki = 0.001f;
    /// <summary>Çömelmiş + lamba kapalı + hareketsiz: 1'den 0'a 50 saniye.</summary>
    public const float SessizSogma = 0.02f;
    /// <summary>Yalnızca çömelmiş ya da yalnızca lamba kapalı (yürüse de): yavaş soğuma.</summary>
    public const float YariSogma = 0.004f;
    /// <summary>Daire dışında (sahanlık, merdiven): yavaş soğuma.</summary>
    public const float SahanlikSogma = 0.003f;

    // ---------- TEKRAR TEPKİLERİ ----------
    /// <summary>1-2. kademede kalındıkça iki tepki arası (saniye).</summary>
    public const float TepkiAraEnAz = 25f, TepkiAraEnCok = 40f;

    // ---------- AV ----------
    /// <summary>Av en çok bu kadar sürer; yakalayamazsa Gölge kaybolur.</summary>
    public const float AvSuresi = 18f;
    /// <summary>Gölge oyuncuyu bu kadar süre göremezse (çömelmiş, lamba kapalı, arada duvar) izi kaybeder.</summary>
    public const float GizlenmeSuresi = 2.5f;
    /// <summary>Gölge'nin av hızı (m/sn; oyuncu 1,8 yürür, 0,8 çömelir). Kat çarpanıyla çarpılır.</summary>
    public const float AvHizi = 2.0f;

    private readonly System.Random rastgele;
    private float deger;          // 0-1 gerilim
    private int kademe;           // 0-3
    private float tepkiSayaci;    // bir sonraki tekrar tepkisine kalan süre
    private float katCarpani = 1f;
    private bool avVar = true;
    private bool avSuruyor;

    public GerilimKurali(int tohum)
    {
        rastgele = new System.Random(tohum);
        tepkiSayaci = YeniAra();
    }

    /// <summary>Gerilim (0-1).</summary>
    public float Deger { get { return deger; } }
    /// <summary>Kademe (0 sakin, 1 huzursuz, 2 yakın, 3 av).</summary>
    public int Kademe { get { return kademe; } }
    /// <summary>Av şu an sürüyor mu (sahne tarafı AvBitti diyene kadar)?</summary>
    public bool AvSuruyor { get { return avSuruyor; } }
    /// <summary>Bu katta ısınma çarpanı.</summary>
    public float KatCarpani { get { return katCarpani; } }

    /// <summary>
    /// Katın ısınma çarpanı: alt katlar yavaş, üst katlar hızlı. 1. kat öğretici: en yavaş.
    /// (Kat dosyasında ayrı bir alan yok; tablo burada ki testlenebilsin ve tek yerde dursun.)
    /// </summary>
    public static float KatinCarpani(int kat)
    {
        switch (kat)
        {
            case 1: return 0.6f;
            case 2: return 0.8f;
            case 3: return 0.9f;
            case 4: return 1.0f;
            case 5: return 1.1f;
            case 6: return 1.0f;    // kendi saklanma anı var (#58); kural o sırada durur
            case 7: return 1.1f;    // kendi kaçış anı var (#58)
            default: return kat >= 8 ? 1.3f : 1.0f;
        }
    }

    /// <summary>Bu katta av var mı? 1. katta yok: oyuncu önce binayı tanısın.</summary>
    public static bool KattaAvVar(int kat) { return kat >= 2; }

    /// <summary>Yeni kata varıldı: gerilim sıfırlanır, katın çarpanı ve av izni kurulur.</summary>
    public void KatAyarla(int kat)
    {
        katCarpani = KatinCarpani(kat);
        avVar = KattaAvVar(kat);
        deger = 0f;
        kademe = 0;
        avSuruyor = false;
        tepkiSayaci = YeniAra();
    }

    /// <summary>Gürültü: gerilimi bir anda bu kadar yükseltir (kat çarpanıyla).</summary>
    public void Gurultu(float miktar)
    {
        if (miktar <= 0f || avSuruyor) return;
        deger = Sinirla(deger + miktar * katCarpani);
    }

    /// <summary>
    /// Her kare çağrılır. gecen: karenin süresi; dairede: oyuncu dairenin içinde mi; lambaAcik:
    /// kafa lambası yanıyor mu; egik: çömelmiş mi; hareketli: yürüyor mu. Dönüş: sahnenin ŞİMDİ
    /// göstereceği tepki (çoğu karede Yok).
    /// </summary>
    public GerilimTepkisi Ilerlet(float gecen, bool dairede, bool lambaAcik, bool egik, bool hareketli)
    {
        if (gecen <= 0f || avSuruyor) return GerilimTepkisi.Yok;

        deger = Sinirla(deger + Hiz(dairede, lambaAcik, egik, hareketli) * gecen);

        int yeni = KademeHesapla(deger);
        if (yeni > kademe)
        {
            // Yeni bir kademeye girildi: giriş tepkisi. Tekrar sayacı da baştan kurulur ki giriş
            // tepkisinin hemen ardından bir tekrar gelmesin.
            kademe = yeni;
            tepkiSayaci = YeniAra();
            if (kademe == 3) { avSuruyor = true; return GerilimTepkisi.Av; }
            return kademe == 1 ? GerilimTepkisi.Titreme : GerilimTepkisi.Golge;
        }
        if (yeni < kademe) kademe = yeni;   // soğudu: sessizce iner, tepki yok

        // 1. ve 2. kademede kalındıkça arada bir tekrar tepkisi.
        if (kademe >= 1)
        {
            tepkiSayaci -= gecen;
            if (tepkiSayaci <= 0f)
            {
                tepkiSayaci = YeniAra();
                return TekrarTepkisi(kademe);
            }
        }
        return GerilimTepkisi.Yok;
    }

    /// <summary>Av bitti (kaçtı, saklandı ya da yakalandı): gerilim sıfırdan başlar.</summary>
    public void AvBitti()
    {
        avSuruyor = false;
        deger = 0f;
        kademe = 0;
        tepkiSayaci = YeniAra();
    }

    // Isınma ya da soğuma hızı (saniyede; eksi = soğuma).
    private float Hiz(bool dairede, bool lambaAcik, bool egik, bool hareketli)
    {
        if (!dairede) return -SahanlikSogma;
        bool sessiz = !lambaAcik && egik && !hareketli;
        if (sessiz) return -SessizSogma;
        if (!lambaAcik || egik) return -YariSogma;
        float hiz = TabanHiz;
        if (hareketli) hiz += HareketEki;
        return hiz * katCarpani;
    }

    private int KademeHesapla(float d)
    {
        if (d >= AvEsigi && avVar) return 3;
        if (d >= GolgeEsigi) return 2;
        if (d >= TitremeEsigi) return 1;
        return 0;
    }

    private GerilimTepkisi TekrarTepkisi(int k)
    {
        // Next(n): 0 ile n-1 arasında rastgele tam sayı.
        if (k == 1)
        {
            switch (rastgele.Next(3))
            {
                case 0: return GerilimTepkisi.Titreme;
                case 1: return GerilimTepkisi.UstKatAdimlari;
                default: return GerilimTepkisi.GolgeIzi;   // duvardaki gölge (#93)
            }
        }
        switch (rastgele.Next(4))
        {
            case 0: return GerilimTepkisi.Golge;
            case 1: return GerilimTepkisi.Tiklatma;
            case 2: return GerilimTepkisi.GolgeIzi;
            default: return GerilimTepkisi.LambaSonme;
        }
    }

    private float YeniAra()
    {
        return TepkiAraEnAz + (float)rastgele.NextDouble() * (TepkiAraEnCok - TepkiAraEnAz);
    }

    private static float Sinirla(float d)
    {
        return d < 0f ? 0f : (d > 1f ? 1f : d);
    }
}
