// TusAtamaKurali.cs
// Tuş atamalarının kuralı (#125): hangi eylem hangi klavye tuşuna ve hangi oyun kolu düğmesine bağlı, hangi
// tuşlar atanamaz, iki eylem aynı tuşu paylaşamaz. Saf C# (Unity yok) ki testlenebilsin; tuşu gerçekten
// okuyan taraf Tuslar.cs'tir.
//
// NEDEN YALNIZCA BEŞ EYLEM: Hareket (W A S D / sol çubuk) ve Bakış (fare / sağ çubuk) oyunun her yerinde
// aynı; Esc / Start / B menünün sabit tuşları. Oyuncunun değiştirmek isteyeceği şey elinin alıştığı
// etkileşim, inceleme, fener, görev listesi ve eğilme tuşlarıdır. Bu beşi bu kuralda; gerisi sabit.
//
// TUŞ ADLARI: klavye için Unity'nin Key enum adı ("E", "LeftCtrl", "Digit1"), kol için GamepadButton adı
// ("South", "LeftShoulder"). Metin olarak tutulur ki bu dosya Unity'yi tanımak zorunda kalmasın ve ayar
// dosyasına olduğu gibi yazılsın.

public enum TusEylemi
{
    Etkilesim = 0,     // al, aç, oku (Player/Interact eyleminin tuşu)
    Incele = 1,        // eşyayı gözün önüne getir (#88)
    Fener = 2,         // kafa lambası (#76, #119)
    GorevListesi = 3,  // T listesi (#75)
    Egil = 4,          // çömel (#79)
    Kos = 5            // koş, basılı tut (#129)
}

public enum AtamaSonucu
{
    Tamam,      // atandı
    Catisma,    // başka bir eylem o tuşu kullanıyor
    Yasak,      // sabit bir tuş (Esc, W A S D, yön tuşları, kolun B/Start/d-pad'i)
    Gecersiz    // boş ad
}

public class TusAtamaKurali
{
    public const int EylemSayisi = 6;

    // Varsayılanlar: eylem sırasıyla (TusEylemi'nin sayı değeri dizinin sırası).
    private static readonly string[] VarsayilanKlavye = { "E", "R", "F", "T", "LeftCtrl", "LeftShift" };
    private static readonly string[] VarsayilanKol = { "South", "West", "LeftShoulder", "Select", "RightStick", "LeftStick" };

    // Atanamayan tuşlar: menü ve hareket bunları sabit kullanır.
    private static readonly string[] YasakKlavye = { "Escape", "W", "A", "S", "D", "UpArrow", "DownArrow", "LeftArrow", "RightArrow", "Enter", "Space", "None" };
    private static readonly string[] YasakKol = { "East", "Start", "DpadUp", "DpadDown", "DpadLeft", "DpadRight" };

    private readonly string[] klavye = new string[EylemSayisi];
    private readonly string[] kol = new string[EylemSayisi];

    public TusAtamaKurali() { VarsayilanaDon(); }

    public string Klavye(TusEylemi e) { return klavye[(int)e]; }
    public string Kol(TusEylemi e) { return kol[(int)e]; }
    public static string VarsayilanKlavyeTusu(TusEylemi e) { return VarsayilanKlavye[(int)e]; }
    public static string VarsayilanKolTusu(TusEylemi e) { return VarsayilanKol[(int)e]; }

    /// <summary>Hepsi varsayılanda mı (Kontroller sayfası "Varsayılana dön" düğmesini soluk çizebilsin)?</summary>
    public bool VarsayilanMi
    {
        get
        {
            for (int i = 0; i < EylemSayisi; i++) if (klavye[i] != VarsayilanKlavye[i] || kol[i] != VarsayilanKol[i]) return false;
            return true;
        }
    }

    public void VarsayilanaDon()
    {
        for (int i = 0; i < EylemSayisi; i++) { klavye[i] = VarsayilanKlavye[i]; kol[i] = VarsayilanKol[i]; }
    }

    /// <summary>
    /// Eyleme tuş atar. kolMu: oyun kolu düğmesi mi (yoksa klavye tuşu). Aynı tuş aynı eylemde zaten
    /// varsa Tamam; başka eylemdeyse Catisma (catisan o eylem); sabit tuşsa Yasak. Çatışma ve yasakta
    /// hiçbir şey değişmez.
    /// </summary>
    public AtamaSonucu Ata(TusEylemi e, string tus, bool kolMu, out TusEylemi catisan)
    {
        catisan = e;
        if (string.IsNullOrEmpty(tus)) return AtamaSonucu.Gecersiz;
        string[] yasak = kolMu ? YasakKol : YasakKlavye;
        foreach (string y in yasak) if (y == tus) return AtamaSonucu.Yasak;
        string[] tablo = kolMu ? kol : klavye;
        for (int i = 0; i < EylemSayisi; i++)
        {
            if (i == (int)e || tablo[i] != tus) continue;
            catisan = (TusEylemi)i;
            return AtamaSonucu.Catisma;
        }
        tablo[(int)e] = tus;
        return AtamaSonucu.Tamam;
    }

    /// <summary>Kayıt metni: "k:E,R,F,T,LeftCtrl;g:South,West,LeftShoulder,Select,RightStick".</summary>
    public string Metin()
    {
        return "k:" + string.Join(",", klavye) + ";g:" + string.Join(",", kol);
    }

    /// <summary>Kayıt metninden kurar; boş ya da bozuk metin varsayılanı verir (eksik eylem varsayılanda kalır).</summary>
    public static TusAtamaKurali Oku(string metin)
    {
        var k = new TusAtamaKurali();
        if (string.IsNullOrEmpty(metin)) return k;
        foreach (string parca in metin.Split(';'))
        {
            if (parca.Length < 3 || parca[1] != ':') continue;
            bool kolMu = parca[0] == 'g';
            if (!kolMu && parca[0] != 'k') continue;
            string[] adlar = parca.Substring(2).Split(',');
            string[] hedef = kolMu ? k.kol : k.klavye;
            for (int i = 0; i < EylemSayisi && i < adlar.Length; i++)
                if (adlar[i].Length > 0) hedef[i] = adlar[i];
        }
        // Okunan tabloda çatışma ya da yasak varsa (elle bozulmuş dosya) varsayılana dön: oyun oynanabilir kalsın.
        for (int i = 0; i < EylemSayisi; i++)
        {
            bool kotu = false;
            foreach (string y in YasakKlavye) if (y == k.klavye[i]) kotu = true;
            foreach (string y in YasakKol) if (y == k.kol[i]) kotu = true;
            for (int j = 0; j < i; j++) if (k.klavye[j] == k.klavye[i] || k.kol[j] == k.kol[i]) kotu = true;
            if (kotu) { k.VarsayilanaDon(); break; }
        }
        return k;
    }
}
