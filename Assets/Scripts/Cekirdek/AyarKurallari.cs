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

    // ---------- GÖRÜŞ AÇISI (FOV) ----------
    // Kameranın dikey görüş açısı, derece. 60 dar ve sinematik (oyunun ayarlandığı
    // hâl); 90 geniş, bazı oyuncular baş dönmesi yaşamamak için geniş açı ister.
    public const float GorusEnAz = 60f;
    public const float GorusEnCok = 90f;
    public const float GorusVarsayilan = 60f;

    // ---------- FAREYİ TERS ÇEVİR ----------
    // Açıkken fareyi yukarı itmek aşağı baktırır (uçak kumandası gibi).
    public const bool TersVarsayilan = false;

    public static float Hassasiyet(float deger)
    {
        return Sinirla(deger, HassasiyetEnAz, HassasiyetEnCok, HassasiyetVarsayilan);
    }

    public static float Ses(float deger)
    {
        return Sinirla(deger, SesEnAz, SesEnCok, SesVarsayilan);
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
