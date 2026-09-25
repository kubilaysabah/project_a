// GorunurlukOlcer.cs
// İki görüntü arasındaki farkı ölçen kural (#33): bir anomalinin ekranda ne kadar
// "görünür" olduğunu sayıya çevirir.
//
// Görüntü, piksellerin renk kanallarından oluşan bir dizi olarak verilir: her piksel
// için arka arkaya kırmızı, yeşil, mavi (0 = hiç, 1 = tam). Normal sahanlık ile anomalili
// sahanlığın aynı açıdan çekilmiş iki görüntüsü karşılaştırılır.
//
// PİKSEL FARKI: üç kanaldaki farkların EN BÜYÜĞÜ. İlk sürüm yalnızca parlaklığa
// bakıyordu ve "yeşil ampul" kartını silik sayıyordu: yeşil ışık ile sarı ışığın
// parlaklığı yakın, ama insan gözü renk değişimini hemen görür. En büyük kanal farkı
// hem parlaklık hem renk değişimini yakalar.
//
// İKİ ÖLÇÜ
//   Alan → farkı eşikten büyük piksellerin oranı (0-1): "ekranın ne kadarı değişti?"
//   Güç  → yalnızca değişen piksellerin ortalama farkı (0-1): "değişen yer ne kadar net?"
// Tek başına alan yanıltır: ters çevrilmiş bir kapı numarası ekranın binde birini
// değiştirir ama bakan herkes görür; geniş ama çok hafif bir ışık değişikliği ise
// karanlıkta seçilmez.
//
// Neden eşik: ekran kartları aynı sahneyi iki kez çizince bile kenarlarda çok küçük
// farklar üretebilir. Eşiğin altındaki farklar "görünmez" sayılır.
//
// Bu dosyada Unity'ye ait tek satır yok: kural testle korunabilsin. Görüntüleri
// çizip diziye çevirmek Editor aracının (AnomaliGorunurlukDenetimi) işi.

public static class GorunurlukOlcer
{
    /// <summary>Bir pikselin "değişti" sayılması için gereken en küçük kanal farkı.</summary>
    public const float VarsayilanEsik = 0.03f;

    // Sınıf sınırları: sezgisel başlangıç değerleri. Şahan'ın gözüyle birkaç kartta
    // karşılaştırılıp gerekirse ayarlanacak (#33).
    public const float GorunmezSiniri = 0.0005f;   // alan < %0,05: ekranda neredeyse hiçbir şey değişmiyor
    public const float SilikSiniri = 0.12f;        // güç < 0,12: değişiyor ama düşük kontrastla
    public const float KucukSiniri = 0.003f;       // alan < %0,3 (ama güçlü): küçük ama net

    // Sınıflar (Sinif'in döndürdüğü sayılar).
    public const int Gorunmez = 0, Silik = 1, KucukNet = 2, Belirgin = 3;

    /// <summary>
    /// İki RGB görüntünün piksel piksel fark haritası: her piksel için üç kanaldaki
    /// farkların en büyüğü. Diziler kanal kanal (r, g, b, r, g, b...) verilir; sonuç
    /// piksel sayısı kadar uzunluktadır. Boyutlar uyuşmazsa boş dizi.
    /// </summary>
    public static float[] FarkHaritasi(float[] a, float[] b)
    {
        if (a == null || b == null || a.Length != b.Length || a.Length % 3 != 0) return new float[0];

        float[] harita = new float[a.Length / 3];
        for (int p = 0; p < harita.Length; p++)
        {
            float enBuyuk = 0f;
            for (int k = 0; k < 3; k++)
            {
                float fark = a[p * 3 + k] - b[p * 3 + k];
                if (fark < 0f) fark = -fark;
                if (fark > enBuyuk) enBuyuk = fark;
            }
            harita[p] = enBuyuk;
        }
        return harita;
    }

    /// <summary>Alan: farkı eşikten büyük piksellerin oranı (0-1).</summary>
    public static float Alan(float[] harita, float esik)
    {
        if (harita == null || harita.Length == 0) return 0f;
        int degisen = 0;
        foreach (float f in harita) if (f > esik) degisen++;
        return (float)degisen / harita.Length;
    }

    /// <summary>Güç: yalnızca eşiği aşan piksellerin ortalama farkı. Hiç yoksa 0.</summary>
    public static float Guc(float[] harita, float esik)
    {
        if (harita == null || harita.Length == 0) return 0f;
        double toplam = 0;   // double: yüz binlerce küçük sayı toplanırken hassasiyet kaybolmasın
        int degisen = 0;
        foreach (float f in harita) if (f > esik) { toplam += f; degisen++; }
        return degisen == 0 ? 0f : (float)(toplam / degisen);
    }

    /// <summary>Alan ve güce göre sınıf: Gorunmez, Silik, KucukNet ya da Belirgin.</summary>
    public static int Sinif(float alan, float guc)
    {
        if (alan < GorunmezSiniri) return Gorunmez;
        if (guc < SilikSiniri) return Silik;
        if (alan < KucukSiniri) return KucukNet;
        return Belirgin;
    }
}
