// KatOlayPlani.cs
// Her katta hangi "kat olayı" sesinin duyulacağını ve ne zaman çalacağını belirleyen kural (#35).
//
// Şahan'ın kararı (2026-09-26, build'de sesleri dinledikten sonra):
//   "Bazı seslerin sürekli olması rahatsızlık veriyor. Her katta damlayan su sesi,
//    bebek ağlaması, kapı çarpması olmasın ama her katta bunlardan biri olsun."
//
// Kurallar:
//   - Her kata bir olay düşer: BEBEK ya da SESSİZ (#74, Şahan'ın ses kararları, 2026-09-27:
//     uzak kapı, kalorifer ve araba kalktı; üst kat adımları Gezen Gölge'nin habercisi oldu).
//     Yeni olay sesleri gelince enum'a eklenir.
//   - Komşu iki katta bebek ağlamaz: art arda iki katta aynı bebek "yine mi" dedirtir.
//     Sessiz her yere düşebilir.
//   - Olay, oyuncu kata vardıktan birkaç saniye sonra bir kez, bir süre sonra bir kez
//     daha çalar. İki çalış arası, en uzun olayın (bebek, ~10 sn) süresinden uzundur:
//     olay kendi kendisinin içine girmez.
//   - Bir turda plan SABİTTİR (aynı kata dönen oyuncu aynı olayı duyar); cezadan sonra
//     yeniden çekilir. Çöp planıyla aynı düzen (CopPlani.cs).
//
// Bu dosyada Unity'ye ait tek satır yok: kural, Unity açılmadan test edilir.
// Sesleri sahnede çalma işi KatYoneticisi'nde; burası yalnızca "hangisi, ne zaman" bilir.

using System.Collections.Generic;   // Dictionary için

// enum: sabit bir seçenek listesi (EtkiTuru ve KararSonucu gibi). Sayı yerine isim
// kullanırız; "olay 2" yerine "UstKatAdimlari" yazmak hata yapmayı zorlaştırır.
public enum KatOlayi
{
    Bebek,            // 4 numaranın içinde bebek bir nöbet ağlar
    Sessiz            // bu katta olay sesi yok
}

public class KatOlayPlani
{
    // ---------- SABİTLER ----------

    /// <summary>Kaç çeşit olay var (enum'daki seçenek sayısı).</summary>
    public const int OlaySayisi = 2;

    /// <summary>İlk çalış, kata varıştan en erken ve en geç bu kadar saniye sonra.</summary>
    public const float IlkEnErken = 4f, IlkEnGec = 9f;

    /// <summary>İkinci çalış, ilkinden en az ve en çok bu kadar saniye sonra.
    /// En kısası (16 sn) bile en uzun olaydan (bebek, ~10,5 sn) uzun: üst üste binmez.</summary>
    public const float AraEnAz = 16f, AraEnCok = 24f;

    // ---------- İÇ DURUM ----------

    // Bir katın planı: hangi olay, ilk ve ikinci çalış zamanı.
    // struct: küçük, "değer gibi" davranan bir veri paketi (class'ın hafif kardeşi).
    private struct KatKaydi
    {
        public KatOlayi olay;
        public float ilk, ikinci;
    }

    // Tohumlu üretici: aynı tohum aynı turu verir (CopPlani ile aynı sebep).
    private readonly System.Random rastgele;

    // Kat numarası -> o katın planı. İlk sorulduğunda çekilir, sonra buradan okunur.
    private readonly Dictionary<int, KatKaydi> katlar = new Dictionary<int, KatKaydi>();

    // ---------- KURULUM ----------

    /// <param name="tohum">Rastgelelik tohumu. Aynı tohum aynı planı üretir.</param>
    public KatOlayPlani(int tohum)
    {
        rastgele = new System.Random(tohum);
    }

    // ---------- SORGU ----------

    /// <summary>Verilen katta hangi olayın duyulacağı.</summary>
    public KatOlayi KatIcin(int kat)
    {
        return Kayit(kat).olay;
    }

    /// <summary>
    /// Olayın çalacağı iki an, kata varıştan itibaren saniye cinsinden: [ilk, ikinci].
    /// Kopya bir dizi döner; çağıran onu değiştirse de plan bozulmaz.
    /// </summary>
    public float[] ZamanlarKatIcin(int kat)
    {
        KatKaydi k = Kayit(kat);
        return new[] { k.ilk, k.ikinci };
    }

    // Katın kaydını getirir; yoksa şimdi çeker.
    private KatKaydi Kayit(int kat)
    {
        if (katlar.TryGetValue(kat, out KatKaydi kayitli)) return kayitli;

        // Komşu katlardan birinde bebek ağlıyorsa bu katta ağlamaz. İki komşu da çekilmiş
        // olabilir (oyuncu 3'ten 5'e, sonra geri 4'e gelirse). Sessiz hep aday: seçenek bitmez.
        var adaylar = new List<KatOlayi>();
        for (int i = 0; i < OlaySayisi; i++)
        {
            var aday = (KatOlayi)i;   // sayıyı enum'a çevir: 0 -> Bebek, 1 -> Sessiz
            if (aday != KatOlayi.Sessiz && (KomsuBunuAldi(kat - 1, aday) || KomsuBunuAldi(kat + 1, aday))) continue;
            adaylar.Add(aday);
        }

        var yeni = new KatKaydi();
        yeni.olay = adaylar[rastgele.Next(adaylar.Count)];
        // NextDouble(): 0 ile 1 arası bir ondalık sayı. Aralığa yaymak için:
        // en küçük + oran × (en büyük - en küçük).
        yeni.ilk = IlkEnErken + (float)rastgele.NextDouble() * (IlkEnGec - IlkEnErken);
        yeni.ikinci = yeni.ilk + AraEnAz + (float)rastgele.NextDouble() * (AraEnCok - AraEnAz);

        katlar[kat] = yeni;
        return yeni;
    }

    private bool KomsuBunuAldi(int komsu, KatOlayi olay)
    {
        return katlar.TryGetValue(komsu, out KatKaydi k) && k.olay == olay;
    }

    // ---------- SIFIRLAMA ----------

    /// <summary>
    /// Cezadan sonra çağrılır: bütün katların planı unutulur, sonraki soruda yeniden
    /// çekilir. Üretici başa sarılmaz; sarılsaydı aynı plan geri gelirdi.
    /// </summary>
    public void Sifirla()
    {
        katlar.Clear();
    }
}
