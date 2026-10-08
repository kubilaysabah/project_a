// CinOlayKurali.cs
// Cin olaylarının zamanlama ve seçim kuralı (#131). Saf C# (Unity yok): CinOlaylari her kare çağırır.
//
// NEDEN: Şahan'ın build testi (2026-10-08): "korku oyunu ama fazla korkuya kapılmıyorum." Gerilim kuralı
// (#86) yavaş ısınır ve ilk kademesi hafiftir (titreme, uzak ayak sesi). Bu kural ondan bağımsızdır: oyuncu
// dairedeyken, gerilim ne olursa olsun, 25-70 saniyede bir görüş dışında bir şey olur (kapı çarpar, arkada
// bir tabak kırılır, kilitli kapıya içeriden vurulur...). Gerilim kademeleri bunun ÜSTÜNE biner.
//
// KURALLAR:
//   - Sayaç yalnızca "uygun" karelerde işler (saklanmıyor, av yok, menü kapalı). #133'ten beri sahanlıkta
//     da işler: orada yalnızca görünür olaylar (Belirme, Gecis) yapılabilir, daire içi olaylar dairede.
//   - Aralık kata göre kısalır: 1. katta 45-90 sn, 8. katta 20-50 sn (arada doğrusal).
//   - Her katın İLK olayı IlkCarpan kadar daha geç gelir: oyuncu daireyi tanısın, sonra rahatsız edilsin.
//   - Aynı olay üst üste gelmez (Son dışlanır); olaylar ağırlıklıdır (çarpma ve kırılma en sık).
//   - TORBA (#135): ağırlıklı zar yerine torba. Her olay türü ağırlığı kadar kopyayla torbaya konur, torba
//     karıştırılır, her seferinde üstten ŞU AN YAPILABİLEN ilk kopya çekilir; torba boşalınca yeniden dolar.
//     Böylece 12 olayın 4'ü kapı çarpma olmaz (Şahan'ın test turu): bir turda her tür ağırlığı kadar gelir.
//     Torbanın kalanı şu an yapılamıyorsa (sahanlıkta yalnız daire olayları kaldıysa) torba yenilenir;
//     ağırlıklar o zaman yaklaşık korunur, oyuncu beklemez.
//   - Seçilecek olay yoksa (her şey görüş içinde) kısa bir süre (BosAralik) sonra yeniden denenir.
//
// Rastgelelik System.Random ve tohumla: bir rapor yeniden oynatılabilir (AnomaliSecici kuralı).

public enum CinOlayi
{
    Yok,
    KapiCarpma,        // başka bir odanın açık kapısı sert kapanır
    Kirilma,           // oyuncunun arkasında tabak / bardak / vazo kırılır, yerde parçalar kalır
    BakmayincaDegisim, // görüş dışındaki kapalı bir kapı / çekmece açılmış bulunur
    IceridenVurus,     // kapalı (çoğu kez kilitli) bir odanın kapısına içeriden üç vuruş
    ArkadanAdim,       // oyuncunun arkasından 3-4 adım; dönünce kesilir
    Cihaz,             // katın radyosu / TV'si bir anlığına cızırtıyla açılıp kapanır
    Belirme,           // #133: Gölge görüşte, 4-9 m'de belirir, 1,4-2,2 sn durur, kaybolur (yakalamaz)
    Gecis,             // #133: Gölge görüşün bir kenarından öbürüne 4 m/sn ile geçer (yakalamaz)
    Kacis,             // #134: figür görüşte belirir, oyuncuya bakar, yüksek ayak sesiyle uzağa KAÇAR (yakalamaz)
    AyakIzi,           // #137: arkadan adımlar yerde ISLAK AYAK İZLERİ bırakır; izler kapalı bir odanın kapısında biter
    KapiGozu,          // #137 (sahanlık): bir dairenin kapı gözü yanar; oyuncu ona bakınca söner
    Zil,               // #137 (sahanlık): görüş dışındaki bir kapının zili kendi kendine çalar (koşuda en çok 2)
    EsyaDonmesi,       // #137: görüş dışındaki bir sandalye / manken oyuncuya döner, bir çerçeve yüzünü duvara çevirir
    AsansorGecis,      // #138 (sahanlık): "ARIZALI" asansör kendi kendine çalışır, kabin DURMADAN kattan geçer
}

public class CinOlayKurali
{
    // Torba (#135): çekilecek olay türlerinin numaraları; üstten (son elemandan) çekilir.
    private readonly System.Collections.Generic.List<int> torba = new System.Collections.Generic.List<int>();

    public const float EnAzAralikKat1 = 45f, EnCokAralikKat1 = 90f;   // 1. katta aralık (saniye)
    public const float EnAzAralikKat8 = 20f, EnCokAralikKat8 = 50f;   // 8. katta aralık
    public const float IlkCarpan = 1.5f;                              // katın ilk olayı bu kadar geç
    public const float BosAralik = 10f;                               // seçilecek olay yoksa yeniden deneme
    public const int OlaySayisi = 15;                                 // CinOlayi'nin eleman sayısı (Yok dahil)

    // Ağırlıklar, CinOlayi sırasıyla (Yok 0): çarpma ve kırılma en sık, cihaz en seyrek; görünür olaylar
    // (#133) orta sıklıkta: Şahan "ara ara gözüksün" dedi, her seferinde değil.
    private static readonly int[] Agirliklar = { 0, 3, 3, 2, 2, 2, 1, 2, 2, 3, 2, 2, 1, 2, 2 };   // Kacis (#134) çarpma/kırılma kadar sık; #137: ayak izi 2, kapı gözü 2, zil 1, eşya 2; #138: asansör 2

    private readonly System.Random rastgele;
    private int kat = 1;
    private float kalan;          // bir sonraki olaya kalan süre (uygun saniyeler)
    private bool bekliyor;        // sayaç doldu, Sec çağrılmayı bekliyor (yeniden kurulana kadar dolmaz)
    private bool ilk = true;      // bu katın ilk olayı mı

    public CinOlayi Son { get; private set; }
    /// <summary>Torbada çekilmeyi bekleyen kopya sayısı (testler).</summary>
    public int TorbadaKalan { get { return torba.Count; } }
    /// <summary>Bir olay türünün ağırlığı = bir torbadaki kopya sayısı (testler).</summary>
    public static int Agirlik(CinOlayi olay) { return Agirliklar[(int)olay]; }
    /// <summary>Bir torbadaki toplam kopya (testler).</summary>
    public static int TorbaBoyu { get { int t = 0; for (int i = 1; i < OlaySayisi; i++) t += Agirliklar[i]; return t; } }
    /// <summary>Bir sonraki olaya kalan uygun saniye (test göstergesi).</summary>
    public float Kalan { get { return kalan; } }
    public int Kat { get { return kat; } }

    public CinOlayKurali(int tohum)
    {
        rastgele = new System.Random(tohum);
        KatAyarla(1);
    }

    /// <summary>Yeni kat: sayaç baştan, ilk olay geç, son olay unutulur.</summary>
    public void KatAyarla(int yeniKat)
    {
        kat = yeniKat < 1 ? 1 : (yeniKat > 8 ? 8 : yeniKat);
        ilk = true;
        Son = CinOlayi.Yok;
        bekliyor = false;
        kalan = YeniAralik();
    }

    /// <summary>
    /// Her kare. gecen: saniye; uygun: oyuncu dairede ve rahatsız edilebilir. Dönüş: sayaç doldu, bir olay
    /// seçilmeli (Sec ile). Dolduktan sonra Sec çağrılana kadar true dönmeye devam eder.
    /// </summary>
    public bool Ilerlet(float gecen, bool uygun)
    {
        if (bekliyor) return true;
        if (!uygun || gecen <= 0f) return false;
        kalan -= gecen;
        if (kalan > 0f) return false;
        kalan = 0f;
        bekliyor = true;
        return true;
    }

    /// <summary>
    /// Sayaç dolunca: mevcut[i] = i numaralı olay şu an yapılabilir mi (CinOlayi sırasıyla). Son olay
    /// dışlanır (en az bir başka seçenek varsa). Torbadan (#135) üstten, şu an yapılabilen ve son olaydan
    /// farklı ilk kopya çekilir; torbanın kalanında öyle bir kopya yoksa torba yenilenip bir kez daha
    /// bakılır. Sayaç yeniden kurulur. Hiçbir olay yapılamıyorsa Yok döner ve BosAralik sonra yeniden sorulur.
    /// </summary>
    public CinOlayi Sec(bool[] mevcut)
    {
        bekliyor = false;
        bool herhangi = false;
        for (int i = 1; i < OlaySayisi; i++)
            if (Uygun(mevcut, i) && (CinOlayi)i != Son) herhangi = true;
        if (!herhangi)
        {
            // Yalnızca son olay yapılabiliyorsa onu tekrar etmek yerine bekleriz; hiçbiri yoksa da bekleriz.
            kalan = BosAralik;
            return CinOlayi.Yok;
        }
        for (int tur = 0; tur < 2; tur++)
        {
            if (torba.Count == 0) TorbayiDoldur();
            for (int j = torba.Count - 1; j >= 0; j--)
            {
                int i = torba[j];
                if (!Uygun(mevcut, i) || (CinOlayi)i == Son) continue;
                torba.RemoveAt(j);
                Son = (CinOlayi)i; ilk = false; kalan = YeniAralik();
                return Son;
            }
            torba.Clear();   // kalan kopyaların hiçbiri şu an yapılamıyor: yeni torba (yapılabilen bir tür var)
        }
        kalan = BosAralik;   // buraya gelinmez (yeni torba her türü içerir); güvenlik
        return CinOlayi.Yok;
    }

    // Torbayı doldur: her tür ağırlığı kadar kopya, sonra karıştır (Fisher-Yates: sondan başa, her elemanı
    // kendisi ya da öncekilerden rastgele biriyle değiştir; her sıralama eşit olasılıklı çıkar).
    private void TorbayiDoldur()
    {
        torba.Clear();
        for (int i = 1; i < OlaySayisi; i++)
            for (int k = 0; k < Agirliklar[i]; k++) torba.Add(i);
        for (int j = torba.Count - 1; j > 0; j--)
        {
            int r = rastgele.Next(j + 1);
            int gecici = torba[j]; torba[j] = torba[r]; torba[r] = gecici;
        }
    }

    private static bool Uygun(bool[] mevcut, int i) { return mevcut != null && i < mevcut.Length && mevcut[i]; }

    // Katın aralığı: 1 → 8 arası doğrusal; ilk olay IlkCarpan kadar geç.
    private float YeniAralik()
    {
        float t = (kat - 1) / 7f;
        float enAz = EnAzAralikKat1 + (EnAzAralikKat8 - EnAzAralikKat1) * t;
        float enCok = EnCokAralikKat1 + (EnCokAralikKat8 - EnCokAralikKat1) * t;
        float aralik = enAz + (float)rastgele.NextDouble() * (enCok - enAz);
        return ilk ? aralik * IlkCarpan : aralik;
    }

    /// <summary>Aralık sınırları (testler ve gösterge için).</summary>
    public static void AralikSiniri(int kat, out float enAz, out float enCok)
    {
        float t = (kat < 1 ? 0 : (kat > 8 ? 7 : kat - 1)) / 7f;
        enAz = EnAzAralikKat1 + (EnAzAralikKat8 - EnAzAralikKat1) * t;
        enCok = EnCokAralikKat1 + (EnCokAralikKat8 - EnCokAralikKat1) * t;
    }
}
