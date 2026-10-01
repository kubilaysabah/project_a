// TakilmaKurali.cs
// Oyuncu bir katta uzun süre ilerleyemezse binanın ona YAZISIZ bir ipucu vermesinin kuralı (#98).
//
// Neden var: #83'ün "ipucu notu yok" kuralından sonra bir oyuncu bir eşyayı hiç bulamazsa oyunda
// sonsuza kadar takılı kalabilir. Not yazmak yasak; ama bina "konuşabilir": takılan oyuncu bir süre
// sonra sıradaki eşyanın durduğu yerden hafif bir gıcırtı duyar, eşya bir an parlar. Bir kapıcının
// eski binada "şu dolaptan bir ses geldi" diye bakması gibi, oyuncu ipucu aldığını değil binanın
// tuhaf olduğunu hisseder.
//
// Bu dosya iki soruyu cevaplar (ikisi de Unity açılmadan test edilir):
//   1. "Şimdi ipucu zamanı mı?"  (TakilmaKurali nesnesi: bir saat)
//   2. "Neyi işaret edelim?"     (Adaylar: zincirin çözüm sırasına göre sıradaki eşya ya da kilit)
//
// ---------- 1. SAAT ----------
// Saat, bulunulan katta SON İLERLEMEDEN beri geçen süreyi sayar. İlerleme = bir eşya bulundu ya da
// bir kilit çözüldü (sahne tarafı söyler: Ilerleme()). Kurallar (Şahan'ın istediği tempo):
//   - 1. katta ilk ipucu 150 saniye ilerlemesizlikten sonra (2,5 dakika: oyuncu önce kendi arasın).
//   - Sonra her 75 saniyede bir.
//   - Bir ilerleme döneminde en çok 4 ipucu: bina ısrarcı bir öğretmene dönüşmesin.
//   - Üst katlarda ipucu daha geç, daha seyrek ve daha az gelir (#102, kat kat zorlaşma): sayılar
//     BulmaZorlugu tablosundan, katı kurarken (KatAyarla) okunur. 8. katta ilk ipucu 270 sn, en çok 2.
//   - Saat yalnızca "uygun" anlarda işler: oyuncu açık dairenin içinde, menüde ya da yakın bakış
//     ekranında değil, av yok, gerilim düşük. Yani merdivende dolaşmak, menüde beklemek ya da
//     Gölge'den saklanmak "takılmak" sayılmaz.
//   - Yeni kata varınca (KatAyarla) her şey sıfırlanır.
//
// ---------- 2. HEDEF ----------
// KilitZinciri.Cozumle() kilitleri, bir oyuncunun açabileceği sırayla verir (her kilit, kendinden
// ÖNCEKİ kilitler açılınca açılabilir hâle gelir). Bu yüzden sıradaki ilk ÇÖZÜLMEMİŞ kilit, oyuncunun
// şu an kesinlikle ulaşabildiği bir kilittir; onu açan eşyalar da ulaşılabilir yerlerdedir.
// İpucu önce o kilidi açan bulunmamış eşyaları, sonra (kol kilidiyse) kolu, en son kilidin kendisini
// işaret eder. Bütün kilitler çözülmüşse sırada katın anahtarı vardır.
//
// Burada Unity'ye ait tek satır yok (GerilimKurali gibi): sahnede sesi çalan, eşyayı parlatan
// bileşen TakilmaIpucu'dur.

using System.Collections.Generic;

/// <summary>İpucunun işaret ettiği şeyin türü.</summary>
public enum TakilmaHedefTuru
{
    Esya,   // zincirdeki bir eşya (Kimlik = eşyanın kimliği)
    Kol,    // bir kol kilidinin çekilecek kolu (Kimlik = kolun durduğu YERİN kimliği)
    Kilit   // kilidin kendisi (Kimlik = kilidin kimliği): kapı, asma kilitli dolap...
}

/// <summary>İpucunun bir adayı: ne (tür) ve hangisi (kimlik).</summary>
public struct TakilmaHedefi
{
    public TakilmaHedefTuru Tur;
    public string Kimlik;

    public TakilmaHedefi(TakilmaHedefTuru tur, string kimlik)
    {
        Tur = tur;
        Kimlik = kimlik;
    }
}

public class TakilmaKurali
{
    // ---------- TEMPO ----------
    // Katın temposu (#102): KatAyarla bunları BulmaZorlugu'ndan doldurur. Kat kurulmadan önce 1. katın değerleri.
    private float ilkIpucu = 150f;   // ilk ipucuna kadar geçmesi gereken ilerlemesiz süre (saniye)
    private float ipucuAra = 75f;    // sonraki ipuçları arası (saniye)
    private int enCok = 4;           // bir ilerleme döneminde en çok bu kadar ipucu

    /// <summary>Bu katta ilk ipucuna kadar beklenen ilerlemesiz süre (saniye).</summary>
    public float IlkIpucu { get { return ilkIpucu; } }
    /// <summary>Bu katta sonraki ipuçları arası (saniye).</summary>
    public float IpucuAra { get { return ipucuAra; } }
    /// <summary>Bu katta bir ilerleme döneminde en çok bu kadar ipucu.</summary>
    public int EnCok { get { return enCok; } }

    private int kat;                 // saatin ait olduğu kat
    private int ipucuSayisi;         // bu ilerleme döneminde verilen ipucu sayısı
    private float ilerlemesizSure;   // son ilerlemeden beri "uygun" geçen toplam süre
    private float sayac;             // son ipucundan (ya da ilerlemeden) beri geçen uygun süre

    /// <summary>Saatin ait olduğu kat (henüz kurulmadıysa 0).</summary>
    public int Kat { get { return kat; } }
    /// <summary>Bu ilerleme döneminde verilmiş ipucu sayısı (0 ile EnCok arası).</summary>
    public int IpucuSayisi { get { return ipucuSayisi; } }
    /// <summary>Son ilerlemeden beri "uygun" geçen saniye (hata ayıklama için).</summary>
    public float IlerlemesizSure { get { return ilerlemesizSure; } }

    /// <summary>Yeni kata varıldı: saat ve ipucu sayısı sıfırdan başlar, katın temposu okunur.</summary>
    public void KatAyarla(int yeniKat)
    {
        kat = yeniKat;
        // Üst katta bina daha az yardım eder (#102): tempo katın tablosundan.
        ilkIpucu = BulmaZorlugu.IlkIpucu(yeniKat);
        ipucuAra = BulmaZorlugu.IpucuAra(yeniKat);
        enCok = BulmaZorlugu.EnCokIpucu(yeniKat);
        Ilerleme();
    }

    /// <summary>Oyuncu ilerledi (eşya buldu, kilit çözdü): saat ve ipucu sayısı sıfırlanır.</summary>
    public void Ilerleme()
    {
        ipucuSayisi = 0;
        ilerlemesizSure = 0f;
        sayac = 0f;
    }

    /// <summary>
    /// Her kare çağrılır. gecen: karenin süresi (saniye); uygun: oyuncu şu an takılmış sayılabilecek
    /// bir durumda mı (dairede, menüsüz, av yok...). Dönüş: true ise ipucu ŞİMDİ çalınmalı.
    /// </summary>
    public bool Ilerlet(float gecen, bool uygun)
    {
        // Uygun değilse saat durur (sıfırlanmaz): oyuncu merdivene çıkıp dönünce kaldığı yerden sayar.
        // "!(gecen > 0f)" yazımı NaN'ı (sayı olmayan bozuk değer) da yakalar: NaN ile her karşılaştırma
        // false döner, "gecen <= 0f" NaN'ı geçirirdi.
        if (!uygun || !(gecen > 0f)) return false;
        if (ipucuSayisi >= enCok) return false;   // tavan doldu: bir sonraki ilerlemeyi bekle

        ilerlemesizSure += gecen;
        sayac += gecen;

        // İlk ipucu için katın ilk süresi (1. katta 150), sonrakiler için aralığı (1. katta 75) beklenir.
        float esik = ipucuSayisi == 0 ? ilkIpucu : ipucuAra;
        if (sayac < esik) return false;

        // Sayaç sıfırlanır (eşiğin üstündeki kısmı taşınmaz): çok uzun tek bir kare (takılan bir
        // yükleme) üst üste iki ipucu çaldırmasın, her ipucu en az katın aralığı kadar arayla gelsin.
        sayac = 0f;
        ipucuSayisi++;
        return true;
    }

    // ---------- HEDEF ----------

    /// <summary>
    /// İpucunun işaret edebileceği adaylar, öncelik sırasıyla. Sahne tarafı listeden sahnede
    /// gerçekten bulabildiği İLK adayı kullanır.
    /// zincir: katın kilit zinciri. acilisSirasi: Cozumle().AcilanKilitler (kilitlerin açılabilir
    /// sırası). esyaBulundu / kilitCozuk: oyuncunun durumu (kimlik alır, true/false verir).
    /// </summary>
    public static List<TakilmaHedefi> Adaylar(KilitZinciri zincir, IList<string> acilisSirasi,
                                               System.Predicate<string> esyaBulundu, System.Predicate<string> kilitCozuk)
    {
        // Predicate<string>: "bir metin al, true ya da false döndür" biçimindeki bir fonksiyonu
        // değişken gibi taşımaya yarar. Böylece bu kural, eşyanın bulunup bulunmadığını kendisi
        // bilmek zorunda kalmaz: oyunda kayıttan, testte basit bir listeden sorulur.
        var adaylar = new List<TakilmaHedefi>();
        if (zincir == null || acilisSirasi == null || esyaBulundu == null || kilitCozuk == null) return adaylar;

        // Sıradaki ilk çözülmemiş kilit: oyuncunun şu an ulaşabildiği kilit (dosyanın başındaki açıklama).
        foreach (string kilitKimligi in acilisSirasi)
        {
            if (kilitCozuk(kilitKimligi)) continue;
            ZincirKilidi k = KilitBul(zincir, kilitKimligi);
            if (k == null) continue;

            // 1) Kilidi açan bulunmamış eşyalar. Gerekli bazı türlerde bir eşya (anahtar, sigorta,
            //    alet), bazılarında bir yer (şifrenin ipucu yeri, kolun yeri): yalnızca zincirde eşya
            //    olarak tanımlıysa eklenir.
            EsyaEkle(adaylar, zincir, k.Gerekli, esyaBulundu);
            if (k.Parcalar != null) foreach (string p in k.Parcalar) EsyaEkle(adaylar, zincir, p, esyaBulundu);
            if (k.Gerekenler != null) foreach (string g in k.Gerekenler) EsyaEkle(adaylar, zincir, g, esyaBulundu);

            // 2) Kol kilidi kapıdan açılmaz: asıl gidilecek yer kolun kendisi.
            if (k.Tur == KilitTuru.Kol && !string.IsNullOrEmpty(k.Gerekli))
                adaylar.Add(new TakilmaHedefi(TakilmaHedefTuru.Kol, k.Gerekli));

            // 3) En son kilidin kendisi: her şey elde ama oyuncu kapıyı (dolabı) bulamadıysa.
            adaylar.Add(new TakilmaHedefi(TakilmaHedefTuru.Kilit, k.Kimlik));
            return adaylar;   // yalnızca ilk çözülmemiş kilit: sonrakiler henüz ulaşılamayabilir
        }

        // Bütün kilitler çözülmüş: sırada katın anahtarı var (henüz bulunmadıysa).
        foreach (ZincirEsyasi e in zincir.Esyalar)
            if (e.Tur == EsyaTuru.KatAnahtari && !esyaBulundu(e.Kimlik))
                adaylar.Add(new TakilmaHedefi(TakilmaHedefTuru.Esya, e.Kimlik));
        return adaylar;
    }

    /// <summary>
    /// Cozumle'ye verilecek bütün oda adları: planın odaları ve ikinci dairenin "B:" önekli
    /// odaları (#70). KatDenetimi de aynı listeyi kurar.
    /// </summary>
    public static List<string> ButunOdalar()
    {
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar)
        {
            odalar.Add(o.Ad);
            odalar.Add(KilitZinciri.IkinciOnek + o.Ad);
        }
        return odalar;
    }

    // Kimlik zincirde bir eşyaysa, bulunmamışsa ve listede yoksa aday olarak ekler.
    private static void EsyaEkle(List<TakilmaHedefi> adaylar, KilitZinciri zincir, string kimlik, System.Predicate<string> esyaBulundu)
    {
        if (string.IsNullOrEmpty(kimlik) || zincir.EsyaBul(kimlik) == null || esyaBulundu(kimlik)) return;
        foreach (TakilmaHedefi a in adaylar)
            if (a.Tur == TakilmaHedefTuru.Esya && a.Kimlik == kimlik) return;   // aynı eşya iki kez yazılmasın
        adaylar.Add(new TakilmaHedefi(TakilmaHedefTuru.Esya, kimlik));
    }

    private static ZincirKilidi KilitBul(KilitZinciri zincir, string kimlik)
    {
        foreach (ZincirKilidi k in zincir.Kilitler) if (k.Kimlik == kimlik) return k;
        return null;
    }
}
