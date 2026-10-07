// GrayboxKurucu_Sahanlik.cs
// Sahanlık giydirmesi (#107): 90'ların Türk apartmanında sahanlık duvarının alt 1,2 metresi koyu YAĞLI BOYA,
// üstü badana; aralarında ince koyu bir BORDÜR çizgisi; dipte SÜPÜRGELİK. Daireler bunları #87'de aldı,
// sahanlık tek dokuydu.
//
// NEDEN AYRI KUTU: duvar kutusu tek bir doku (dökülmüş sıva) taşıyor; alt yarısına başka malzeme vermek için
// kutuyu ikiye bölmek yerine duvarın İÇ yüzüne 1,2 cm kalınlığında ince bir "band" kutusu konuyor. Oyuncu
// farkı göremez, duvarın geometrisi (çarpışma, anomali kartlarının hedef adları) hiç değişmez.
//
// NEDEN HER SAHANLIKTA AYNI: döngü kuralı. Oyuncu sahanlık A ile B arasında ışınlanır; A'da band olup B'de
// olmasa dikiş görünürdü. SahanlikKur her sahanlığı (A, B, sahte katlar, zemin kat, dokuzuncu kat) bu
// fonksiyondan geçirir. Dokuzuncu kat 1974'te yenidir: DokuzuncuKatiTazele aynı adlı malzemeleri taze
// karşılıklarıyla DEĞİŞTİRİR (Doku_YagliBoya → Taze_YagliBoya), geometri aynı kalır.
//
// Malzemeler DokuKurucu'nun tür tablosundan gelir (Doku_YagliBoya, Doku_Bordur): Dokula() aynı dosyayı
// aynı ayarlarla yeniler, iki ayrı kaynak çelişmez. Süpürgelik dairelerin Supurgelik.mat'ını paylaşır
// (bina demirbaşı: tekrar yasağından muaf).

using UnityEngine;
using static MekanOlculeri;   // DuvarKalin gibi ortak ölçüler (Editor dışı sınıf, bkz. GrayboxKurucu.cs)

public static partial class GrayboxKurucu
{
    const float BandYuk = 1.2f;                  // yağlı boya bandının üst sınırı (metre)
    const float BandKalin = 0.012f;              // bandın duvardan taşması
    const float BordurYuk = 0.03f;               // bordür çizgisinin yüksekliği
    const float SahanlikSupurgelikYuk = 0.08f;   // süpürgelik yüksekliği
    const float SahanlikSupurgelikKalin = 0.02f; // süpürgelik kalınlığı (banddan 8 mm daha taşkın)

    // SahanlikKur'un sonunda çağrılır: bu sahanlığın bütün duvar parçalarına band, bordür ve süpürgelik.
    static void SahanlikGiydir(Transform sahanlik)
    {
        YagliBoyaBandiKur(sahanlik,
                          DokuKurucu.Malzeme("Doku_YagliBoya"),
                          DokuKurucu.Malzeme("Doku_Bordur"),
                          BoyaliAhsapMalzeme());
    }

    // Her "Duvar_*" kutusunun (lentolar hariç: kapı üstüne band gelmez) sahanlığa bakan yüzüne
    // band + bordür + süpürgelik. Hepsi çarpışmasız: oyuncu duvara zaten giremiyor, ince kutular
    // CharacterController'ı takılacak kenar yapmasın.
    static void YagliBoyaBandiKur(Transform sahanlik, Material boya, Material bordur, Material supurgelik)
    {
        foreach (Transform duvar in sahanlik)
        {
            string ad = duvar.name;
            if (!ad.StartsWith("Duvar_") || ad.Contains("Lento")) continue;
            if (duvar.Find("Lambri") != null) continue;   // yeniden çalıştırmada iki kez konmasın

            // Sahanlığa bakan yön. SIRA ÖNEMLİ: "Duvar_On_Sol" hem "On" hem "Sol" içerir; ön duvarın iç
            // yüzü +z'ye bakar, sol duvarınki +x'e. Önce ön/arka, sonra sol/sağ sorulur.
            Vector3 ic = ad.StartsWith("Duvar_On") ? Vector3.forward
                       : ad.StartsWith("Duvar_Arka") ? Vector3.back
                       : ad.StartsWith("Duvar_Sol") ? Vector3.right
                       : Vector3.left;
            bool yanDuvar = Mathf.Abs(ic.x) > 0.5f;              // sol/sağ duvar: band z boyunca uzar
            Vector3 olcek = duvar.localScale;
            float uzunluk = yanDuvar ? olcek.z : olcek.x;
            Vector3 yuz = duvar.localPosition + ic * (DuvarKalin / 2f);   // duvarın iç yüzünün merkezi

            // Band: 0 → BandYuk, duvar yüzünden BandKalin kadar taşkın.
            Parca("Lambri", sahanlik, duvar, boya,
                  new Vector3(yuz.x, BandYuk / 2f, yuz.z) + ic * (BandKalin / 2f),
                  yanDuvar ? new Vector3(BandKalin, BandYuk, uzunluk) : new Vector3(uzunluk, BandYuk, BandKalin));
            // Bordür: bandın üst kenarında, banddan 4 mm daha taşkın ince koyu çizgi.
            Parca("Bordur", sahanlik, duvar, bordur,
                  new Vector3(yuz.x, BandYuk + BordurYuk / 2f, yuz.z) + ic * (BandKalin / 2f + 0.002f),
                  yanDuvar ? new Vector3(BandKalin + 0.004f, BordurYuk, uzunluk) : new Vector3(uzunluk, BordurYuk, BandKalin + 0.004f));
            // Süpürgelik: dipte, banddan taşkın koyu boyalı ahşap.
            Parca("Supurgelik", sahanlik, duvar, supurgelik,
                  new Vector3(yuz.x, SahanlikSupurgelikYuk / 2f, yuz.z) + ic * (SahanlikSupurgelikKalin / 2f),
                  yanDuvar ? new Vector3(SahanlikSupurgelikKalin, SahanlikSupurgelikYuk, uzunluk)
                           : new Vector3(uzunluk, SahanlikSupurgelikYuk, SahanlikSupurgelikKalin));
        }
    }

    // Sahanlığın koordinatında yerleştirilir, sonra duvarın altına alınır (duvar gizlenirse band da gizlenir).
    // Duvar kutusunun ölçeği 1 değil; önce sahanlığa göre kurup "dünya konumunu koru" ile taşımak,
    // Unity'nin ters ölçeği kendisinin hesaplamasını sağlar.
    static void Parca(string ad, Transform sahanlik, Transform duvar, Material malzeme, Vector3 konum, Vector3 olcek)
    {
        GameObject g = Kutu(ad, sahanlik, konum, olcek);
        g.transform.SetParent(duvar, true);
        Boya(g, malzeme);
        Object.DestroyImmediate(g.GetComponent<Collider>());
    }
}
