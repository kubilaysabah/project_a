// MetinTablosu.cs
// Oyundaki bütün yazıların iki dilli sözlüğü (#15).
//
// Kodun içine "TOPLA" gibi metin yazılmaz; onun yerine bir ANAHTAR yazılır
// ("ipucu.topla"). Bu sınıf anahtarı tabloda arar ve seçili dildeki metni verir.
// Bir restoran menüsü gibi: mutfak "12 numara" der, menüde 12 numaranın adı
// Türkçe de yazabilir İngilizce de; mutfak değişmez, sadece menü değişir.
//
// Tablo, Assets/Resources/Metinler.txt dosyasıdır. Her satır:
//     anahtar <TAB> Türkçe <TAB> İngilizce
// Sütunları TAB karakteriyle ayrılmış düz metin (bu biçime TSV, "tab separated
// values" denir). Excel'de ve Not Defteri'nde açılır, çevirmene olduğu gibi
// gönderilebilir. Virgül yerine TAB seçildi, çünkü metinlerin içinde bol virgül var.
// Uzantı .txt, .tsv değil: Unity .tsv'yi kodun okuyabileceği bir metin dosyası
// (TextAsset) olarak tanımıyor; .txt'yi tanıyor.
//
// Bu dosyada Unity'ye ait tek satır yok (KatSayaci gibi): dosyayı okuma kuralı
// testle korunabilsin. Dosyayı diskten yüklemek Unity tarafının (Dil.cs) işi;
// buraya dosyanın İÇERİĞİ bir metin olarak verilir.
//
// DOSYA KURALLARI
//   - "#" ile başlayan satır açıklamadır, boş satır atlanır.
//   - Metnin içinde satır atlamak için "\n" yazılır (iki karakter: ters bölü ve n);
//     okurken gerçek satır sonuna çevrilir. Gerçek bir satır sonu yeni satır demektir.
//   - Aynı anahtar iki kez yazılırsa, sütun sayısı yanlışsa ya da bir dil boşsa
//     bu bir HATA olarak kaydedilir. Oyun yine çalışır (ilk yazılan geçerli olur),
//     ama bütünlük testi hatayı adıyla gösterir.

using System.Collections.Generic;

// Oyunun desteklediği diller. enum: sabit, isimli seçenekler listesi (EtkiTuru gibi).
// Sıra önemli: tablodaki sütun sırasıyla aynı (0 = Türkçe, 1 = İngilizce).
public enum DilKodu
{
    TR = 0,
    EN = 1
}

public class MetinTablosu
{
    // Anahtar → o anahtarın her dildeki metni (dizi: [0] Türkçe, [1] İngilizce).
    // Dictionary: anahtarla arama yapan bir kutu; bir sözlükte kelimeyi bulmak gibi,
    // bütün listeyi baştan taramadan doğrudan doğru satıra gider.
    private readonly Dictionary<string, string[]> satirlar = new Dictionary<string, string[]>();

    // Dosya okunurken bulunan sorunlar. Oyunu durdurmaz; testler bunlara bakar.
    private readonly List<string> hatalar = new List<string>();

    /// <summary>Tablodaki bütün anahtarlar (dosyadaki sırayla değil).</summary>
    public IEnumerable<string> Anahtarlar { get { return satirlar.Keys; } }

    /// <summary>Tabloda kaç anahtar var.</summary>
    public int Sayi { get { return satirlar.Count; } }

    /// <summary>Okuma sırasında bulunan sorunlar ("12. satır: ..." biçiminde).</summary>
    public IReadOnlyList<string> Hatalar { get { return hatalar; } }

    // Dil sayısı: enum'daki seçenek sayısı. Yeni dil eklenirse burası ve tablo değişir.
    public const int DilSayisi = 2;

    // ---------- OKUMA ----------
    /// <summary>TSV içeriğinden bir tablo kurar.</summary>
    public static MetinTablosu Oku(string icerik)
    {
        var tablo = new MetinTablosu();
        if (string.IsNullOrEmpty(icerik)) return tablo;

        // Bazı editörler UTF-8 dosyanın başına görünmez bir işaret (BOM, U+FEFF)
        // koyar. Kalırsa ilk anahtarın adının başına yapışır ve o anahtar bulunamaz.
        if (icerik[0] == (char)0xFEFF) icerik = icerik.Substring(1);

        // Windows satır sonu "\r\n", diğerleri "\n". Hepsini "\n"e indirip bölüyoruz.
        string[] satirDizisi = icerik.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (int i = 0; i < satirDizisi.Length; i++)
        {
            string satir = satirDizisi[i];
            int satirNo = i + 1;   // insanlar satırları 1'den sayar

            // Boş satır ve açıklama satırı: atla.
            if (satir.Trim().Length == 0) continue;
            if (satir.TrimStart().StartsWith("#")) continue;

            string[] sutunlar = satir.Split('\t');

            // anahtar + her dil için bir sütun olmalı.
            if (sutunlar.Length != 1 + DilSayisi)
            {
                tablo.hatalar.Add($"{satirNo}. satır: {1 + DilSayisi} sütun olmalıydı, {sutunlar.Length} var.");
                continue;
            }

            string anahtar = sutunlar[0].Trim();

            if (tablo.satirlar.ContainsKey(anahtar))
            {
                tablo.hatalar.Add($"{satirNo}. satır: '{anahtar}' anahtarı ikinci kez yazılmış.");
                continue;
            }

            string[] metinler = new string[DilSayisi];
            for (int d = 0; d < DilSayisi; d++)
            {
                // "\n" yazısını (ters bölü + n) gerçek satır sonuna çevir.
                metinler[d] = sutunlar[1 + d].Replace("\\n", "\n");

                if (metinler[d].Trim().Length == 0)
                    tablo.hatalar.Add($"{satirNo}. satır: '{anahtar}' için {(DilKodu)d} metni boş.");
            }

            tablo.satirlar.Add(anahtar, metinler);
        }

        return tablo;
    }

    // ---------- ARAMA ----------
    /// <summary>
    /// Anahtarın seçili dildeki metni. O dil boşsa Türkçesi verilir (hiç yazı
    /// görünmemesindense Türkçe görünmesi iyidir). Anahtar tabloda hiç yoksa
    /// "#anahtar#" döner: ekranda göze batsın ki eksik hemen fark edilsin.
    /// </summary>
    public string Getir(string anahtar, DilKodu dil)
    {
        string[] metinler;
        if (anahtar == null || !satirlar.TryGetValue(anahtar, out metinler))
            return "#" + anahtar + "#";

        string metin = metinler[(int)dil];
        if (metin.Trim().Length == 0) metin = metinler[(int)DilKodu.TR];
        return metin;
    }

    /// <summary>Anahtar tabloda var mı.</summary>
    public bool Var(string anahtar)
    {
        return anahtar != null && satirlar.ContainsKey(anahtar);
    }
}

// ---------- SİSTEM DİLİ ----------
// Oyun ilk açıldığında hangi dilde başlasın? Windows Türkçe ise Türkçe, değilse
// İngilizce. İngilizce varsayılan, çünkü Steam'de oyuncuların çoğu Türkçe bilmiyor;
// bir Almanın Türkçe bir notla karşılaşması, İngilizce bir notla karşılaşmasından kötü.
//
// Unity sistem dilini "Turkish", "English", "German" gibi bir isimle verir
// (SystemLanguage). Bu dosya Unity'yi tanımadığı için ismi metin olarak alıyor.
public static class DilSecimi
{
    public static DilKodu SistemDilinden(string sistemDiliAdi)
    {
        return sistemDiliAdi == "Turkish" ? DilKodu.TR : DilKodu.EN;
    }
}
