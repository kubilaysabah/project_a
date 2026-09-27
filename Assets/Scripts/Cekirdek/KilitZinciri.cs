// KilitZinciri.cs
// Bir katın dairesindeki kilit zinciri: veri biçimi ve "çözülebilir mi?" kuralı (#54).
//
// ---------- TASARIMIN TEMEL KURALI ----------
// Şahan (2026-09-27): dairenin içinde de odalar kilitlidir; bir odadan ötekine ancak bir
// ANAHTAR, bir ŞİFRE ya da bir KOL ile geçilir; bütün odalar açılmaz. Zincirin sonunda katın
// anahtarı durur, o da kilitli kat kapısını açar (Docs/hikaye.md §4, §6).
//
// ---------- VERİ ----------
// Bir zincir dört listeden oluşur:
//   Yerler      : eşyaların durabildiği ya da kilitlenebilen yerler (komodinin çekmecesi,
//                 kavanoz, buzdolabı kapısı, duvardaki kol...). Her yerin dairedeki yolu ve
//                 hangi odada olduğu yazılır.
//   Esyalar     : zincirde bulunan eşyalar (oda anahtarı, sigorta, tornavida, katın anahtarı)
//                 ve hangi yerde durdukları.
//   Kilitler    : hangi kapı ya da yer neyle kilitli ve kilidi ne açar.
//   KapaliOdalar: o katta hiç açılmayan odalar (zincirin dışında kalanlar).
//
// ---------- KURAL (Cozumle) ----------
// Oyuncunun yapabileceklerini adım adım taklit eder: ulaşabildiği odalar, o odalardaki yerler,
// o yerlerdeki eşyalar; bunlarla açabildiği kilitler; açılan kilitlerle yeni odalar... Hiçbir
// şey değişmeyene kadar sürer. Katın anahtarına ulaşılabiliyorsa zincir çözülebilir; hiçbir
// kilit boşuna değilse (hepsi açılabiliyorsa) zincir temizdir. Böylece bir katın içeriği
// yazılırken "anahtar kendi kilitlediği odanın içinde kaldı" gibi bir hata oyun oynanmadan
// yakalanır. Unity'siz, düz C#; testli.
//
// [System.Serializable]: Unity bu sınıfları bir ScriptableObject dosyasının içinde saklayıp
// Inspector'da gösterebilsin diye. Alanlar "public", çünkü Unity yalnızca onları kaydeder.

using System.Collections.Generic;

/// <summary>Beş kilit türü (Docs/hikaye.md §6).</summary>
public enum KilitTuru
{
    Anahtar,     // oda anahtarı: dairede bir yerde bulunur, tek kilidi açar
    Sifre,       // şifreli asma kilit: 4 hane; ipucu dairede bir yerde yazılı
    Kol,         // bir yerde kol çekilir, başka yerdeki bu kilit "tak" diye açılır
    Sigorta,     // sigorta teli bulunup kutuya takılınca elektrik gelir, kilit açılır
    Tornavida,   // kilidi açılmayan kapının menteşeleri tornavidayla sökülür (alet bitmez)
    Kirma,       // bir yer (cam kavanoz) doğru aletle kırılarak açılır; alet bitmez (#61).
                 // SONA eklendi: kayıtlı zincir dosyaları türü sayı olarak saklar, araya
                 // eklemek eski dosyalardaki türleri kaydırırdı.
    Ekipman,     // (#65) ağır bir şey (gardırop) ancak BİRKAÇ alet birlikte bulununca yerinden
                 // oynar: Gerekenler listesindeki aletlerin hepsi kapıcının çantasında olmalı.
    KirikAnahtar // (#65) kapının anahtarı kırık: Gerekenler listesindeki anahtar parçalarının
                 // hepsi bulununca anahtar tamamlanır ve kapı açılır.
}

[System.Serializable]
public class ZincirYeri
{
    public string Kimlik;       // zincirin içinde bu yere verilen ad ("komodin")
    public string Yol;          // dairenin kökünden yolu ("Esyalar/Komodin/Cekmece"); eşyalar buraya konur
    public string Oda;          // bulunduğu oda ("Oda_A"); DairePlani'ndaki adlardan
    public string KilitYolu;    // yer kilitlenirse kilidin takıldığı parça; boşsa Yol.
                                // Buzdolabı: eşyalar gövdede ("Esyalar/Buzdolabi"), kilitlenen kapısı
                                // ("Esyalar/Buzdolabi/Kapi"). Çekmecede ikisi aynıdır, boş bırakılır.
    // PLANDA BİR NOKTA (#63): PlandaNokta işaretliyse yer bir mobilya değil, odanın bir
    // noktasıdır (yerde duran bir kâğıt parçası); Yol kullanılmaz. U ve V planın eksenleri
    // (bkz. DairePlani), Y yükseklik, Donus y ekseninde açı. Uygulayıcı oraya boş bir çapa
    // nesnesi koyar, eşya onun çocuğu olur. (Boş Yol'u işaret saymadık: kuralın testleri
    // yolsuz yerlerle yazılıyor, yol orada önemsiz.)
    public bool PlandaNokta;
    public float U, Y, V, Donus;
    // Planda noktanın ön yüzünün baktığı plan yönü (#64: duvara yazılan ipucu odaya baksın).
    // İkisi de 0 ise yalnızca Donus kullanılır. Plan yönü dört aynalı dairede de doğru çevrilir.
    public float OnU, OnV;
}

[System.Serializable]
public class ZincirEsyasi
{
    public string Kimlik;       // "odaA_anahtari", "kat_anahtari", "sigorta", "tornavida"
    public EsyaTuru Tur;
    public string Yer;          // durduğu yerin kimliği
    public float X, Y, Z;       // yerin içinde, yerin kendi eksenlerinde konumu (metre)
    public float Donus;         // y ekseninde dönüşü (derece)
}

[System.Serializable]
public class ZincirKilidi
{
    public string Kimlik;       // "odaA_kapisi"
    public KilitTuru Tur;
    public bool OdaKapisi;      // true: Hedef bir odanın adı (kapısı kilitli); false: bir yerin kimliği
    public string Hedef;
    public string Gerekli;      // Anahtar/Sigorta/Tornavida: eşya kimliği. Kol: kol yerinin kimliği.
                                // Şifre: ipucunun yazılı olduğu yerin kimliği.
    public string Sifre;        // yalnızca Şifre: dört hane ("1987")
    public string IpucuMetni;   // yalnızca Şifre: ipucu yerine yazılacak metin; boşsa şifrenin kendisi
    public float IpucuX, IpucuY, IpucuZ;   // yazının ipucu yerindeki konumu (yerin kendi eksenlerinde)
    public float IpucuOlcek = 1f;          // yazının boy çarpanı (radyonun küçük kadranı için < 1)
    // Yalnızca Şifre (#63): şifre yırtık bir çizimin parçalarına bölünmüşse, parçaların eşya
    // kimlikleri. Şifre ancak bu parçaların HEPSİNE ulaşılınca okunabilir. Liste doluysa
    // Gerekli (ipucu yeri) boş bırakılabilir: ipucu yere yazılmaz, birleşen çizimdedir.
    public List<string> Parcalar = new List<string>();
    // Yalnızca Şifre (#64): ipucu karanlıkta parlayan boyayla yazılmış; yalnızca kafa lambası
    // kapalıyken ve dairede elektrik yokken görünür (KaranliktaParlayan). Kural için fark etmez.
    public bool IpucuKaranlikta;
    // Yalnızca Ekipman (#65): gereken aletlerin eşya kimlikleri (levye, kaydırıcı...). Hepsi
    // bulununca kilit açılır; aletler bitmez.
    public List<string> Gerekenler = new List<string>();
}

[System.Serializable]
public class KilitZinciri
{
    public List<string> KapaliOdalar = new List<string>();
    public List<ZincirYeri> Yerler = new List<ZincirYeri>();
    public List<ZincirEsyasi> Esyalar = new List<ZincirEsyasi>();
    public List<ZincirKilidi> Kilitler = new List<ZincirKilidi>();

    // Antre ve koridor hep açıktır: zincir onlardan başlar, bütün oda kapıları koridora açılır.
    // "Sahanlik" planda bir oda değil: açık dairenin kapısının önü (ayakkabılık, paspas...).
    // Yolu "Sahanlik:" ile başlayan yerler oradadır ve hep ulaşılır (#61).
    public const string SahanlikOdasi = "Sahanlik";
    public static readonly string[] HepAcikOdalar = { "Antre", "Hol", SahanlikOdasi };

    /// <summary>Kuralın sonucu: hangi kilitler hangi sırayla açıldı, katın anahtarına ulaşıldı mı.</summary>
    public class Sonuc
    {
        public readonly List<string> AcilanKilitler = new List<string>();
        public readonly List<string> Hatalar = new List<string>();
        public bool KatAnahtarinaUlasildi;
        public bool Cozulebilir { get { return KatAnahtarinaUlasildi && Hatalar.Count == 0; } }
    }

    public ZincirYeri YerBul(string kimlik)
    {
        foreach (ZincirYeri y in Yerler) if (y.Kimlik == kimlik) return y;
        return null;
    }

    public ZincirEsyasi EsyaBul(string kimlik)
    {
        foreach (ZincirEsyasi e in Esyalar) if (e.Kimlik == kimlik) return e;
        return null;
    }

    /// <summary>Zinciri oyuncu gibi çözmeye çalışır; oda adları DairePlani'ndaki adlardır.</summary>
    public Sonuc Cozumle(ICollection<string> butunOdalar)
    {
        var sonuc = new Sonuc();
        VeriHatalariniBul(butunOdalar, sonuc);
        if (sonuc.Hatalar.Count > 0) return sonuc;

        var acilan = new HashSet<string>();
        bool degisti = true;
        while (degisti)
        {
            degisti = false;
            foreach (ZincirKilidi k in Kilitler)
            {
                if (acilan.Contains(k.Kimlik)) continue;
                if (!KilidinOnuneUlasilir(k, acilan)) continue;   // kilide gidebiliyor mu?
                if (!GerekliElde(k, acilan)) continue;            // açacak şey elde mi?
                acilan.Add(k.Kimlik);
                sonuc.AcilanKilitler.Add(k.Kimlik);
                degisti = true;
            }
        }

        foreach (ZincirKilidi k in Kilitler)
            if (!acilan.Contains(k.Kimlik)) sonuc.Hatalar.Add($"'{k.Kimlik}' kilidi hiç açılamıyor");

        ZincirEsyasi katAnahtari = null;
        foreach (ZincirEsyasi e in Esyalar) if (e.Tur == EsyaTuru.KatAnahtari) katAnahtari = e;
        sonuc.KatAnahtarinaUlasildi = katAnahtari != null && EsyayaUlasilir(katAnahtari, acilan);
        if (katAnahtari != null && !sonuc.KatAnahtarinaUlasildi) sonuc.Hatalar.Add("katın anahtarına ulaşılamıyor");
        return sonuc;
    }

    // ---------- ULAŞILABİLİRLİK ----------
    private bool OdayaUlasilir(string oda, HashSet<string> acilan)
    {
        foreach (string a in HepAcikOdalar) if (a == oda) return true;
        if (KapaliOdalar.Contains(oda)) return false;
        // Odanın kapısını kilitleyen bir kilit varsa, o kilit açılmış olmalı.
        foreach (ZincirKilidi k in Kilitler)
            if (k.OdaKapisi && k.Hedef == oda && !acilan.Contains(k.Kimlik)) return false;
        return true;
    }

    private bool YereUlasilir(string yerKimligi, HashSet<string> acilan)
    {
        ZincirYeri y = YerBul(yerKimligi);
        if (y == null || !OdayaUlasilir(y.Oda, acilan)) return false;
        // Yerin kendisi kilitliyse (asma kilitli dolap gibi) kilidi açılmış olmalı.
        foreach (ZincirKilidi k in Kilitler)
            if (!k.OdaKapisi && k.Hedef == yerKimligi && !acilan.Contains(k.Kimlik)) return false;
        return true;
    }

    private bool EsyayaUlasilir(ZincirEsyasi e, HashSet<string> acilan) { return YereUlasilir(e.Yer, acilan); }

    // Kilidin önüne gidilebiliyor mu: oda kapıları koridora açılır (hep ulaşılır); bir yerin
    // kilidine ise yerin bulunduğu odaya girebilen ulaşır.
    private bool KilidinOnuneUlasilir(ZincirKilidi k, HashSet<string> acilan)
    {
        if (k.OdaKapisi) return true;
        ZincirYeri y = YerBul(k.Hedef);
        return y != null && OdayaUlasilir(y.Oda, acilan);
    }

    private bool GerekliElde(ZincirKilidi k, HashSet<string> acilan)
    {
        switch (k.Tur)
        {
            case KilitTuru.Sifre:
                // İpucu bir yere yazılıysa oraya, çizime bölünmüşse bütün parçalarına ulaşılmalı.
                if (!string.IsNullOrEmpty(k.Gerekli) && !YereUlasilir(k.Gerekli, acilan)) return false;
                if (k.Parcalar != null)
                    foreach (string p in k.Parcalar)
                    {
                        ZincirEsyasi parca = EsyaBul(p);
                        if (parca == null || !EsyayaUlasilir(parca, acilan)) return false;
                    }
                return true;
            case KilitTuru.Kol:
                // Kolu çekmek için o yere ulaşmak yeter.
                return YereUlasilir(k.Gerekli, acilan);
            case KilitTuru.Ekipman:
            case KilitTuru.KirikAnahtar:
                // Gereken aletlerin (ya da anahtar parçalarının) hepsine ulaşılmalı.
                if (k.Gerekenler == null || k.Gerekenler.Count == 0) return false;
                foreach (string g in k.Gerekenler)
                {
                    ZincirEsyasi alet = EsyaBul(g);
                    if (alet == null || !EsyayaUlasilir(alet, acilan)) return false;
                }
                return true;
            default:
                ZincirEsyasi e = EsyaBul(k.Gerekli);
                return e != null && EsyayaUlasilir(e, acilan);
        }
    }

    // ---------- VERİ HATALARI ----------
    // Yazım hataları (olmayan oda, yer ya da eşya; yanlış türde eşya) kuralı çalıştırmadan yakalanır.
    private void VeriHatalariniBul(ICollection<string> butunOdalar, Sonuc sonuc)
    {
        foreach (string o in KapaliOdalar)
            if (!butunOdalar.Contains(o)) sonuc.Hatalar.Add($"kapalı oda '{o}' planda yok");
        foreach (ZincirYeri y in Yerler)
            if (y.Oda != SahanlikOdasi && !butunOdalar.Contains(y.Oda)) sonuc.Hatalar.Add($"'{y.Kimlik}' yerinin odası '{y.Oda}' planda yok");
        // Planda bir nokta olan yer, yazıldığı odanın içinde olmalı (#63): yanlış bir sayı
        // kâğıdı duvarın içine ya da komşu odaya koyardı; kural o odayı açık sanardı.
        foreach (ZincirYeri y in Yerler)
            if (y.PlandaNokta && DairePlani.NoktaninOdasi(y.U, y.V) != y.Oda)
                sonuc.Hatalar.Add($"'{y.Kimlik}' yerinin noktası ({y.U}, {y.V}) '{y.Oda}' odasında değil");
        foreach (ZincirEsyasi e in Esyalar)
            if (YerBul(e.Yer) == null) sonuc.Hatalar.Add($"'{e.Kimlik}' eşyasının yeri '{e.Yer}' tanımlı değil");

        int katAnahtari = 0;
        foreach (ZincirEsyasi e in Esyalar) if (e.Tur == EsyaTuru.KatAnahtari) katAnahtari++;
        if (katAnahtari != 1) sonuc.Hatalar.Add($"zincirde tam bir kat anahtarı olmalı ({katAnahtari} var)");

        foreach (ZincirKilidi k in Kilitler)
            if (!KilitKaydi.KimlikGecerli(k.Kimlik)) sonuc.Hatalar.Add($"kilit kimliği '{k.Kimlik}' kayda uygun değil (boş ya da ; : içeriyor)");

        foreach (ZincirKilidi k in Kilitler)
        {
            if (k.OdaKapisi && !butunOdalar.Contains(k.Hedef)) sonuc.Hatalar.Add($"'{k.Kimlik}': oda '{k.Hedef}' planda yok");
            if (!k.OdaKapisi && YerBul(k.Hedef) == null) sonuc.Hatalar.Add($"'{k.Kimlik}': yer '{k.Hedef}' tanımlı değil");
            switch (k.Tur)
            {
                case KilitTuru.Sifre:
                    bool parcali = k.Parcalar != null && k.Parcalar.Count > 0;
                    if (!parcali && YerBul(k.Gerekli) == null) sonuc.Hatalar.Add($"'{k.Kimlik}': ipucu yeri '{k.Gerekli}' yok");
                    if (parcali && !string.IsNullOrEmpty(k.Gerekli) && YerBul(k.Gerekli) == null)
                        sonuc.Hatalar.Add($"'{k.Kimlik}': ipucu yeri '{k.Gerekli}' yok");
                    if (parcali)
                        foreach (string p in k.Parcalar)
                        {
                            ZincirEsyasi pe = EsyaBul(p);
                            if (pe == null) sonuc.Hatalar.Add($"'{k.Kimlik}': parça '{p}' zincirde yok");
                            else if (pe.Tur != EsyaTuru.Parca) sonuc.Hatalar.Add($"'{k.Kimlik}': '{p}' bir Parca değil, {pe.Tur}");
                        }
                    if (!DortHane(k.Sifre)) sonuc.Hatalar.Add($"'{k.Kimlik}': şifre dört rakam olmalı ('{k.Sifre}')");
                    break;
                case KilitTuru.Kol:
                    if (YerBul(k.Gerekli) == null) sonuc.Hatalar.Add($"'{k.Kimlik}': kol yeri '{k.Gerekli}' yok");
                    break;
                case KilitTuru.KirikAnahtar:
                    if (k.Gerekenler == null || k.Gerekenler.Count < 2) sonuc.Hatalar.Add($"'{k.Kimlik}': kırık anahtarın en az iki parçası olmalı");
                    else
                        foreach (string g in k.Gerekenler)
                        {
                            ZincirEsyasi p = EsyaBul(g);
                            if (p == null) sonuc.Hatalar.Add($"'{k.Kimlik}': anahtar parçası '{g}' zincirde yok");
                            else if (p.Tur != EsyaTuru.AnahtarParcasi) sonuc.Hatalar.Add($"'{k.Kimlik}': '{g}' bir anahtar parçası değil, {p.Tur}");
                        }
                    break;
                case KilitTuru.Ekipman:
                    if (k.Gerekenler == null || k.Gerekenler.Count == 0) sonuc.Hatalar.Add($"'{k.Kimlik}': gereken alet yazılmamış");
                    else
                        foreach (string g in k.Gerekenler)
                        {
                            ZincirEsyasi alet = EsyaBul(g);
                            if (alet == null) sonuc.Hatalar.Add($"'{k.Kimlik}': gereken alet '{g}' zincirde yok");
                            else if (!AletCantasi.AletMi(alet.Tur)) sonuc.Hatalar.Add($"'{k.Kimlik}': '{g}' bir alet değil, {alet.Tur}");
                        }
                    break;
                default:
                    ZincirEsyasi e = EsyaBul(k.Gerekli);
                    EsyaTuru beklenen = k.Tur == KilitTuru.Anahtar ? EsyaTuru.OdaAnahtari
                                      : k.Tur == KilitTuru.Sigorta ? EsyaTuru.Sigorta : EsyaTuru.Tornavida;
                    if (e == null) sonuc.Hatalar.Add($"'{k.Kimlik}': gereken eşya '{k.Gerekli}' yok");
                    else if (e.Tur != beklenen) sonuc.Hatalar.Add($"'{k.Kimlik}': '{k.Gerekli}' bir {beklenen} değil, {e.Tur}");
                    break;
            }
        }
    }

    private static bool DortHane(string s)
    {
        if (s == null || s.Length != 4) return false;
        foreach (char c in s) if (c < '0' || c > '9') return false;
        return true;
    }
}
