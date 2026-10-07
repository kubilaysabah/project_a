// AyarKurallari.cs
// Oyuncu ayarlarının sınırları ve varsayılan değerleri (#27).
//
// Her ayarın izin verilen bir aralığı var: fare hassasiyeti 0,25× ile 3× arası,
// ses %0 ile %100 arası, görüş açısı 60° ile 90° arası. Bir değer bu aralığın
// dışına çıkarsa (bozuk bir kayıt, elle düzenlenmiş bir ayar dosyası, ileride
// değişen bir sınır) en yakın sınıra çekilir. Değer hiç sayı değilse (NaN) ya da
// sonsuzsa varsayılana döner: böyle bir değer kamerayı ya da sesi tamamen bozardı.
//
// Bu dosyada Unity'ye ait tek satır yok (MetinTablosu gibi): kural testle korunabilsin.
// Kaydetme ve uygulama Unity tarafının (Ayarlar.cs) işi.

public static class AyarKurallari
{
    // ---------- FARE HASSASİYETİ ----------
    // Kontrolcüdeki temel hassasiyetin ÇARPANI. 1 = oyunun ayarlandığı hâl.
    public const float HassasiyetEnAz = 0.25f;
    public const float HassasiyetEnCok = 3f;
    public const float HassasiyetVarsayilan = 1f;

    // ---------- SES SEVİYESİ ----------
    // Bütün seslerin ortak seviyesi. 0 = sessiz, 1 = tam.
    public const float SesEnAz = 0f;
    public const float SesEnCok = 1f;
    public const float SesVarsayilan = 1f;
    // #118: müzik ve konuşma kanalları (genel seviyeyle çarpılır; efektler yalnızca genel kanalda).
    public const float MuzikEnAz = 0f, MuzikEnCok = 1f, MuzikVarsayilan = 1f;
    public const float KonusmaEnAz = 0f, KonusmaEnCok = 1f, KonusmaVarsayilan = 1f;

    // ---------- GÖRÜŞ AÇISI (FOV) ----------
    // Kameranın dikey görüş açısı, derece. 60 dar ve sinematik (oyunun ayarlandığı
    // hâl); 90 geniş, bazı oyuncular baş dönmesi yaşamamak için geniş açı ister.
    public const float GorusEnAz = 60f;
    public const float GorusEnCok = 90f;
    public const float GorusVarsayilan = 60f;

    // ---------- FAREYİ TERS ÇEVİR ----------
    // Açıkken fareyi yukarı itmek aşağı baktırır (uçak kumandası gibi).
    public const bool TersVarsayilan = false;

    // ---------- KAFA SALLANMASI (#92) ----------
    // Yürürken kameranın hafifçe sallanması. Bazı oyuncuların midesini bulandırır (hareket
    // hassasiyeti); bu yüzden kapatılabilir. Varsayılan açık: oyunun hissinin parçası.
    public const bool SallanmaVarsayilan = true;

    // ---------- GÖRÜNTÜ (#30) ----------
    // Grafik kalitesi: 0 = Düşük, 1 = Orta, 2 = Yüksek. Oyun Yüksek'te ayarlandı.
    public const int KaliteDusuk = 0, KaliteOrta = 1, KaliteYuksek = 2;
    public const int KaliteVarsayilan = KaliteYuksek;

    // Parlaklık: gece post-processing'inin pozlamasına eklenen değer ("EV", pozlama
    // adımı). +1 görüntüyü iki kat aydınlatır, -1 yarıya indirir. 0 = oyunun ayarlandığı hâl.
    public const float ParlaklikEnAz = -1f;
    public const float ParlaklikEnCok = 1f;
    public const float ParlaklikVarsayilan = 0f;

    public const bool TamEkranVarsayilan = true;
    public const bool VSyncVarsayilan = true;

    // V-Sync kapalıyken kare hızı sınırı. Sınırsız bırakılırsa ekran kartı, menüde bile,
    // saniyede yüzlerce gereksiz kare çizip %100 çalışır (#6'da yaşandı).
    public const int VSyncKapaliKareSiniri = 240;

    public static int Kalite(int deger)
    {
        if (deger < KaliteDusuk) return KaliteDusuk;
        if (deger > KaliteYuksek) return KaliteYuksek;
        return deger;
    }

    public static float Parlaklik(float deger)
    {
        return Sinirla(deger, ParlaklikEnAz, ParlaklikEnCok, ParlaklikVarsayilan);
    }

    /// <summary>
    /// Ekranın desteklediği çözünürlükler arasından kayıtlı olana en uygun olanın
    /// sırasını verir. Birebir aynısı varsa o; yoksa piksel sayısı en yakın olan
    /// (ör. başka bir monitöre geçilmişse). Liste boşsa -1.
    /// genislikler[i] ve yukseklikler[i] birlikte bir çözünürlüğü anlatır.
    /// </summary>
    public static int CozunurlukSec(int[] genislikler, int[] yukseklikler, int genislik, int yukseklik)
    {
        if (genislikler == null || genislikler.Length == 0) return -1;

        int enIyi = 0;
        long enKucukFark = long.MaxValue;
        long hedef = (long)genislik * yukseklik;   // long: büyük çarpımlar int'e sığmayabilir

        for (int i = 0; i < genislikler.Length; i++)
        {
            if (genislikler[i] == genislik && yukseklikler[i] == yukseklik) return i;   // birebir

            long fark = (long)genislikler[i] * yukseklikler[i] - hedef;
            if (fark < 0) fark = -fark;
            if (fark < enKucukFark) { enKucukFark = fark; enIyi = i; }
        }
        return enIyi;
    }

    public static float Hassasiyet(float deger)
    {
        return Sinirla(deger, HassasiyetEnAz, HassasiyetEnCok, HassasiyetVarsayilan);
    }

    public static float Ses(float deger)
    {
        return Sinirla(deger, SesEnAz, SesEnCok, SesVarsayilan);
    }

    public static float Muzik(float deger)
    {
        return Sinirla(deger, MuzikEnAz, MuzikEnCok, MuzikVarsayilan);
    }

    public static float Konusma(float deger)
    {
        return Sinirla(deger, KonusmaEnAz, KonusmaEnCok, KonusmaVarsayilan);
    }

    public static float Gorus(float deger)
    {
        return Sinirla(deger, GorusEnAz, GorusEnCok, GorusVarsayilan);
    }

    // Değeri [enAz, enCok] aralığına sıkıştırır. Sayı değilse varsayılanı verir.
    private static float Sinirla(float deger, float enAz, float enCok, float varsayilan)
    {
        // float.IsNaN: "sayı değil" (ör. 0/0). float.IsInfinity: sonsuz. İkisi de
        // karşılaştırmalarda beklenmedik davrandığı için önce ayıklanır.
        if (float.IsNaN(deger) || float.IsInfinity(deger)) return varsayilan;
        if (deger < enAz) return enAz;
        if (deger > enCok) return enCok;
        return deger;
    }
}
