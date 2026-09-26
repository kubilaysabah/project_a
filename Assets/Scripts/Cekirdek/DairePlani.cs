// DairePlani.cs
// 3+1 dairenin planı ve hangi katta hangi dairenin açık olduğu (#53, Docs/hikaye.md §5).
//
// ---------- NEDEN VERİ OLARAK YAZILDI ----------
// Daire bir kez tasarlanıyor, dört yerde kuruluyor (1, 2, 3, 4 numaralar birbirinin aynası)
// ve ileride her katta farklı döşenecek. Planı kurucunun (GrayboxKurucu) içine sayılar
// olarak gömmek yerine burada, Unity'siz düz bir sınıfta tutuyoruz. İki faydası var:
//   1) Test edilebilir: "odalar üst üste binmiyor", "her odaya bir yoldan girilebiliyor",
//      "her kapı bir duvarın üstünde" gibi kuralları saniyeler içinde denetleyebiliyoruz.
//      Plan değiştiğinde (bir odayı büyütmek gibi) bozulan bir şey varsa test söyler.
//   2) Kurucu yalnızca bu listeyi dolaşıp inşa eder; planı değiştirmek için kurucuya
//      dokunmak gerekmez.
//
// ---------- KOORDİNATLAR: u VE v ----------
// Dört daire dört ayrı yöne uzanıyor. Hepsini tek bir planla anlatabilmek için dairenin
// KENDİ eksenlerini kullanıyoruz (bir haritanın "kuzey"i gibi):
//   u: sahanlık duvarından dairenin içine doğru (derinlik). u = 0 sahanlık duvarının dış yüzü.
//   v: sahanlık duvarı boyunca. v = 0 giriş kapısının ortası; eksi yön komşu daireye doğru.
// Kurucu (u, v)'yi her daire için dünyadaki (x, z)'ye çevirir; aynalama orada olur.
// Bütün ölçüler metre, odanın İÇ yüzleri. İki oda arasındaki bölme duvarı 10 cm:
// bir odanın kenarı 2,85'te bitiyorsa komşusu 2,95'te başlar, duvarın ortası 2,9'dadır.

using System.Collections.Generic;

public static class DairePlani
{
    // ---------- HANGİ KATTA HANGİ DAİRE AÇIK ----------
    // Tasarım belgesi §4: dört kapıdan yalnızca biri açık; sıra 2, 3, 1, 4 ve tekrar.
    // Zemin kat (kapıcı odası) ve çatı dairesizdir: 0 döner.
    private static readonly int[] AcikSirasi = { 2, 3, 1, 4 };

    /// <summary>Bu katta açık olan dairenin numarası (1-4); dairesiz katta 0.</summary>
    public static int AcikDaire(int kat)
    {
        if (kat < KatSayaci.BaslangicKati || kat > KatSayaci.CatiKati) return 0;
        // (kat - 1) % 4: 1. kat 0, 2. kat 1 ... 5. kat yine 0. "%" bölümden kalanı verir.
        return AcikSirasi[(kat - 1) % AcikSirasi.Length];
    }

    // ---------- ÖLÇÜLER ----------
    public const float IcDuvar = 0.10f;    // iki oda arasındaki bölme duvarı
    public const float DisDuvar = 0.20f;   // dış duvar (pencereli kenarlar, komşu daire)
    public const float TavanYuk = 2.80f;   // döşemeden tavana
    public const float KapiYuk = 2.00f;    // oda kapısı boşluğunun yüksekliği
    public const float GirisGen = 0.90f;   // giriş kapısı (sahanlıktaki daire kapısıyla aynı)
    public const float GirisYuk = 2.05f;

    // Dairenin dış sınırı (iç yüzler). Komşu daire kapısı 2 m ötede; aradaki duvarın ortası
    // v = -1'de, bu yüzden bu dairenin iç yüzü -0,8'de biter.
    public const float UMin = 0f, UMax = 10.5f, VMin = -0.8f, VMax = 8.1f;

    // ---------- ODALAR ----------
    /// <summary>Bir oda: adı ve iç yüzlerinin sınırları. Islak: zemini fayans (banyo, mutfak).</summary>
    public struct Oda
    {
        public string Ad;
        public float U0, U1, V0, V1;
        public bool Islak;
        public Oda(string ad, float u0, float u1, float v0, float v1, bool islak = false)
        { Ad = ad; U0 = u0; U1 = u1; V0 = v0; V1 = v1; Islak = islak; }
        public float Alan { get { return (U1 - U0) * (V1 - V0); } }
    }

    //   v
    //   8.1 +-----------+--------+--------+
    //       |  Salon    | Oda B  | Oda A  |      üst bant: pencereler arka cephede
    //   4.7 +--d--------+---d----+---d----+
    //       |      Hol (koridor)          |
    //   2.9 +  (açık)  -+-d-+--d--+--d----+
    //       |  Antre    |Bny|OdaC | Mutfak |=    mutfak penceresi dipte (u = 10,5)
    //  -0.8 +--G--------+---+-----+-------+
    //       0          2.6                10.5  u
    //   G: giriş kapısı (sahanlıktaki daire kapısı), d: oda kapısı
    public static readonly Oda[] Odalar =
    {
        new Oda("Antre",  0.00f,  2.60f, -0.80f, 2.85f),
        new Oda("Hol",    0.00f, 10.50f,  2.95f, 4.65f),
        new Oda("Banyo",  2.70f,  4.50f, -0.80f, 2.85f, islak: true),
        new Oda("Oda_C",  4.60f,  6.70f, -0.80f, 2.85f),
        new Oda("Mutfak", 6.80f, 10.50f, -0.80f, 2.85f, islak: true),
        new Oda("Salon",  0.00f,  5.00f,  4.75f, 8.10f),
        new Oda("Oda_B",  5.10f,  7.60f,  4.75f, 8.10f),
        new Oda("Oda_A",  7.70f, 10.50f,  4.75f, 8.10f),
    };

    // ---------- AÇIKLIKLAR (kapılar, pencereler, geçitler) ----------
    public enum AciklikTuru
    {
        Giris,          // sahanlıktaki daire kapısının boşluğu (kanadı sahanlık kurucusunda)
        Kapi,           // menteşeli, kilitlenebilir oda kapısı
        Gecit,          // kapısız, tavana kadar açık geçiş (antre ile hol arası)
        Pencere,        // dış duvarda pencere
        BalkonKapisi,   // camlı, hep kilitli
    }

    /// <summary>
    /// Bir duvardaki boşluk. Duvar bir çizgiyle tanımlanır: UDuvari ise "u = Cizgi" (v boyunca
    /// uzanan duvar), değilse "v = Cizgi" (u boyunca uzanan duvar). Merkez boşluğun duvar
    /// boyunca ortası; Alt ve Ust döşemeden yükseklikler. Oda: kapının açıldığı (ait olduğu) oda.
    /// </summary>
    public struct Aciklik
    {
        public string Ad;
        public AciklikTuru Tur;
        public string Oda;
        public bool UDuvari;
        public float Cizgi, Merkez, Gen, Alt, Ust;
        public Aciklik(string ad, AciklikTuru tur, string oda, bool uDuvari, float cizgi, float merkez,
                       float gen, float alt, float ust)
        { Ad = ad; Tur = tur; Oda = oda; UDuvari = uDuvari; Cizgi = cizgi; Merkez = merkez; Gen = gen; Alt = alt; Ust = ust; }
    }

    public static readonly Aciklik[] Acikliklar =
    {
        // Giriş: sahanlık duvarında (u = 0), v = 0'da.
        new Aciklik("Giris", AciklikTuru.Giris, "Antre", true, 0f, 0f, GirisGen, 0f, GirisYuk),
        // Antre ile hol arasında kapısız geçiş: antrenin bütün üst kenarı.
        new Aciklik("Gecit_Hol", AciklikTuru.Gecit, "Hol", false, 2.90f, 1.30f, 2.60f, 0f, TavanYuk),
        // Alt bant kapıları (v = 2,9 bölmesinde), hol tarafından girilir.
        new Aciklik("Kapi_Banyo",  AciklikTuru.Kapi, "Banyo",  false, 2.90f, 3.60f, 0.70f, 0f, KapiYuk),
        new Aciklik("Kapi_Oda_C",  AciklikTuru.Kapi, "Oda_C",  false, 2.90f, 5.65f, 0.80f, 0f, KapiYuk),
        new Aciklik("Kapi_Mutfak", AciklikTuru.Kapi, "Mutfak", false, 2.90f, 7.40f, 0.80f, 0f, KapiYuk),
        // Üst bant kapıları (v = 4,7 bölmesinde).
        new Aciklik("Kapi_Salon",  AciklikTuru.Kapi, "Salon",  false, 4.70f, 1.30f, 0.90f, 0f, KapiYuk),
        new Aciklik("Kapi_Oda_B",  AciklikTuru.Kapi, "Oda_B",  false, 4.70f, 6.35f, 0.80f, 0f, KapiYuk),
        new Aciklik("Kapi_Oda_A",  AciklikTuru.Kapi, "Oda_A",  false, 4.70f, 9.10f, 0.80f, 0f, KapiYuk),
        // Arka cephe (v = 8,2 dış duvarı): salon penceresi ve balkon kapısı, oda pencereleri.
        new Aciklik("Pencere_Salon", AciklikTuru.Pencere, "Salon", false, 8.20f, 1.40f, 1.40f, 0.90f, 2.20f),
        new Aciklik("Balkon_Kapisi", AciklikTuru.BalkonKapisi, "Salon", false, 8.20f, 3.60f, 0.80f, 0f, 2.10f),
        new Aciklik("Pencere_Oda_B", AciklikTuru.Pencere, "Oda_B", false, 8.20f, 6.35f, 1.20f, 0.90f, 2.20f),
        new Aciklik("Pencere_Oda_A", AciklikTuru.Pencere, "Oda_A", false, 8.20f, 9.10f, 1.20f, 0.90f, 2.20f),
        // Dip cephe (u = 10,6 dış duvarı): mutfak penceresi, tezgâhın üstünde.
        new Aciklik("Pencere_Mutfak", AciklikTuru.Pencere, "Mutfak", true, 10.60f, 1.00f, 1.20f, 1.05f, 2.20f),
    };

    /// <summary>Adı verilen odayı bulur; yoksa false.</summary>
    public static bool OdaBul(string ad, out Oda oda)
    {
        foreach (Oda o in Odalar)
            if (o.Ad == ad) { oda = o; return true; }
        oda = default(Oda);
        return false;
    }

    /// <summary>
    /// Bir açıklığın iki yanındaki odalar. Açıklığın çizgisine 15 cm'den yakın kenarı olan ve
    /// açıklığın ortasını kendi kenarı boyunca kapsayan odalar. Dış duvardaki açıklıkta tek oda.
    /// </summary>
    public static List<string> AciklikOdalari(Aciklik a)
    {
        var sonuc = new List<string>();
        foreach (Oda o in Odalar)
        {
            // Açıklık bu odanın hangi kenarına denk geliyor? Kenarın "çizgiye uzaklığı" küçük olmalı.
            bool kenarda = a.UDuvari
                ? (Yakin(o.U0, a.Cizgi) || Yakin(o.U1, a.Cizgi))
                : (Yakin(o.V0, a.Cizgi) || Yakin(o.V1, a.Cizgi));
            // Açıklığın ortası o kenarın boyunca odanın içinde mi?
            bool boyunca = a.UDuvari
                ? (a.Merkez > o.V0 && a.Merkez < o.V1)
                : (a.Merkez > o.U0 && a.Merkez < o.U1);
            if (kenarda && boyunca) sonuc.Add(o.Ad);
        }
        return sonuc;
    }

    // Bir oda kenarı bir duvar çizgisine "yakın" mı: bölme duvarının yarısı (5 cm) ya da dış
    // duvarın yarısı (10 cm) kadar uzakta olur; 15 cm'ye kadarını kabul ediyoruz.
    private static bool Yakin(float kenar, float cizgi)
    {
        float fark = kenar - cizgi;
        return fark > -0.15f && fark < 0.15f;
    }
}
