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
//     da işler. Nerede ne olur: daire içi olaylar (çarpma, kırılma, bakmayınca değişim, içeriden vuruş, arkadan
//     adım, cihaz, ayak izi, eşya dönmesi) yalnız dairede; sahanlık olayları (kapı gözü, zil, asansör) yalnız
//     A sahanlığının zemininde; figürlü olaylar (belirme, geçiş, kaçış) ikisinde de. Yerin uygunluğunu kural
//     değil CinOlaylari.Hazirla bilir; kural yalnız "bu tür şimdi yapılabilir mi?" diye sorar.
//   - 2. kat, ilk karşılaşma dersi (#112, kayıtta "ilkav") oynanana kadar 1. kat gibi yalnız sesli olay alır
//     (#142; denetimi Hazirla yapar, kayda baktığı için).
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
//   - 1. KAT ÖĞRETİCİ (#141): 1. katta yalnız SESLİ olaylar (çarpma, kırılma, bakmayınca açılan kapı, içeriden
//     vuruş, arkadan adım, cihaz); oyuncu önce binayı tanısın, figür 2. kattan itibaren görünür. Zemin katta
//     (kapıcı odası, kat 0) hiç olay yok. Kural: KattaIzinli.
//   - TEMBEL SEÇİM (#141): Sec, türleri torbadaki sırayla TEK TEK sorar; bir tür "yapılabilir" derse durur.
//     Böylece her olayda bütün türlerin yeri aranmaz (eskiden tek karede binlerce fizik sorgusu yapılıyordu).
//   - SESSİZLİK (#141): görünür bir olaydan sonraki sessizlik üst katlarda kısalır (SessizlikAraligi): 1. katta
//     15-20 sn, 8. katta 8-12 sn; hiçbir katta bir sonraki olayın en kısa aralığından uzun değil.
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
    public const float SessizlikKat1EnAz = 15f, SessizlikKat1EnCok = 20f;   // #141: 1. katta sessizlik (saniye)
    public const float SessizlikKat8EnAz = 8f, SessizlikKat8EnCok = 12f;    // #141: 8. katta sessizlik

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
    /// <summary>Ağırlık dizisinin uzunluğu (test: enum ve OlaySayisi ile aynı olmalı, #141).</summary>
    public static int AgirlikSayisi { get { return Agirliklar.Length; } }
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
        kat = yeniKat < 0 ? 0 : (yeniKat > 8 ? 8 : yeniKat);   // #141: 0 = zemin kat (olay yok, KattaIzinli)
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
    /// Sayaç dolunca: mevcut[i] = i numaralı olay şu an yapılabilir mi (CinOlayi sırasıyla). Testlerin ve eski
    /// çağrıların kullandığı biçim; aşağıdaki tembel Sec'e bir "soru fonksiyonu" olarak verilir.
    /// </summary>
    public CinOlayi Sec(bool[] mevcut)
    {
        // "o => Uygun(mevcut, (int)o)" bir LAMBDA: adı olmayan, yerinde yazılmış küçük bir fonksiyon. "o" girdisi,
        // "=>" işaretinin sağı döndürdüğü değer. Burada "o türü mevcut dizisinde işaretli mi?" sorusunu yazar.
        return Sec(o => Uygun(mevcut, (int)o));
    }

    /// <summary>
    /// Tembel seçim (#141). yapilabilir: "bu tür ŞU AN yapılabilir mi?" diye sorulan fonksiyon (CinOlaylari'nda
    /// yalnız o türün yerini arar). Torbanın üstünden başlanır; son olay ve bu katta izinsiz türler atlanır; her
    /// tür bu seçimde EN ÇOK BİR KEZ sorulur (cevap saklanır); ilk "evet"te durulur, o kopya torbadan çıkar.
    /// Torbanın kalanında yapılabilen yoksa torba yenilenip bir kez daha bakılır. Hiçbiri yoksa Yok döner ve
    /// BosAralik sonra yeniden sorulur.
    ///
    /// System.Func&lt;CinOlayi, bool&gt;: "CinOlayi alıp bool döndüren bir fonksiyon" türü. Fonksiyonu bir değişken
    /// gibi taşımaya yarar (C#'ta bunun adı "delegate"); kural ne zaman soracağını kendisi seçer.
    /// </summary>
    public CinOlayi Sec(System.Func<CinOlayi, bool> yapilabilir)
    {
        bekliyor = false;
        // Cevap defteri: 0 = henüz sorulmadı, 1 = evet, 2 = hayır. Bir tür torbada birkaç kopya olsa da bir kez sorulur.
        int[] cevap = new int[OlaySayisi];
        for (int tur = 0; tur < 2; tur++)
        {
            if (torba.Count == 0) TorbayiDoldur();
            for (int j = torba.Count - 1; j >= 0; j--)
            {
                int i = torba[j];
                CinOlayi olay = (CinOlayi)i;
                if (olay == Son || !KattaIzinli(olay, kat)) continue;       // tekrar yok; bu katta izinsiz
                if (cevap[i] == 0) cevap[i] = yapilabilir(olay) ? 1 : 2;     // ilk kez soruluyor
                if (cevap[i] != 1) continue;
                torba.RemoveAt(j);
                Son = olay; ilk = false; kalan = YeniAralik();
                return Son;
            }
            torba.Clear();   // kalan kopyaların hiçbiri şu an yapılamıyor: yeni torba
        }
        // Yeni torbada da yapılabilen yok (ya da yalnızca son olay yapılabiliyor): bekle, sonra yeniden sor.
        kalan = BosAralik;
        return CinOlayi.Yok;
    }

    /// <summary>
    /// #141 (B2): olay bu katta olabilir mi? Zemin katta (0) hiçbiri; 1. katta (öğretici) yalnız sesli olaylar
    /// ve bakmayınca açılan kapı (görünmeyen değişiklik); 2. kattan itibaren hepsi. (Asansör geçişinin kendi
    /// "2. kattan itibaren, 8. katta değil" kuralı CinOlaylari'nda.)
    /// </summary>
    public static bool KattaIzinli(CinOlayi olay, int kat)
    {
        if (olay == CinOlayi.Yok || kat < 1) return false;
        if (kat >= 2) return true;
        switch (olay)
        {
            case CinOlayi.KapiCarpma:
            case CinOlayi.Kirilma:
            case CinOlayi.BakmayincaDegisim:
            case CinOlayi.IceridenVurus:
            case CinOlayi.ArkadanAdim:
            case CinOlayi.Cihaz:
                return true;
        }
        return false;
    }

    /// <summary>#141 (B4): görünür olaydan sonraki sessizliğin süre sınırları (saniye); 1 → 8 arası doğrusal.</summary>
    public static void SessizlikAraligi(int kat, out float enAz, out float enCok)
    {
        float t = Mathf01((kat - 1) / 7f);
        enAz = SessizlikKat1EnAz + (SessizlikKat8EnAz - SessizlikKat1EnAz) * t;
        enCok = SessizlikKat1EnCok + (SessizlikKat8EnCok - SessizlikKat1EnCok) * t;
    }

    // 0-1 arasına sıkıştırma (Unity'nin Mathf.Clamp01'i; bu dosya Unity'siz kalsın diye kendimiz yazdık).
    private static float Mathf01(float x) { return x < 0f ? 0f : (x > 1f ? 1f : x); }

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
        float t = Mathf01((kat - 1) / 7f);   // zemin kat (0) 1. katın aralığını alır
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
