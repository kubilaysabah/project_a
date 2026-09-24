// SonsuzMerdiven.cs
// Oyunun en önemli hilesi: oyuncu sonsuza kadar merdiven çıkar,
// ama sahnede gerçek olan tek bir kat vardır.
//
// NASIL ÇALIŞIR
// Sahnede üst üste iki gerçek sahanlık var: A (y = 0) ve B (y = 3). B, A'nın
// birebir aynısıdır ve TAM OLARAK onun üstündedir (yarım dönüşlü merdiven
// sayesinde katlar üst üste biniyor). Oyuncu iki kolu çıkıp B'ye ayak bastığı
// anda onu sessizce 3 metre aşağı alırız. Bu, A'daki aynı noktaya denk gelir.
// İleri geri hiçbir kayma yok; sadece dikey bir taşıma.
//
// NEDEN GÖZ FARK ETMEZ
// Işınlanmadan önceki ve sonraki kare piksel piksel aynıdır: aynı duvarlar,
// aynı kapılar, aynı ışık, aynı bakış açısı. Merdiven gözünden aşağı ve yukarı
// bakınca görünen katlar da aynı: sahnede A'nın altında ve B'nin üstünde
// birebir aynı sahte katlar kurulu.
//
// ÖNEMLİ: bu script oyunun KURALLARINA karışmaz. Kat kararı, ışınlanmadan değil
// oyuncunun gerçekten kat ettiği yoldan okunur (bkz. KatYoneticisi, SanalKot).
// Işınlanma bir görüntü hilesidir; bir kural onu asla okumamalıdır.

using UnityEngine;

// Ölçüler MekanOlculeri.cs dosyasında; sınıf adını tekrar yazmadan kullanıyoruz.
using static MekanOlculeri;

[RequireComponent(typeof(CharacterController))]
public class SonsuzMerdiven : MonoBehaviour
{
    [Header("Durum")]
    [Tooltip("Döngü tamamen açık mı? Çatıya ulaşılınca ve ceza sırasında kapatılır.")]
    [SerializeField] private bool acik = true;

    [Tooltip("Yukarı ışınlanma açık mı? 8. katta kapatılır: oyuncu yukarı çıktığında " +
             "bir sahanlığa değil çatı kapısına varmalı. Aşağı kaçış yolu açık kalır.")]
    [SerializeField] private bool yukariAcik = true;

    [Tooltip("Aşağı ışınlanma açık mı? Açılışta (#16) oyuncu zemin kattayken kapatılır: " +
             "zemin kat binanın dibi, altında döngüye bağlanan bir kat yok.")]
    [SerializeField] private bool asagiAcik = true;

    [Tooltip("Kaç kez yukarı döngü yapıldı.")]
    public int yukariDonguSayisi;

    [Tooltip("Kaç kez aşağı döngü yapıldı.")]
    public int asagiDonguSayisi;

    /// <summary>
    /// Net döngü sayısı: yukarı döngüler eksi aşağı döngüler. Oyuncunun kurgudaki
    /// yüksekliğini hesaplamak için KatYoneticisi okur.
    /// </summary>
    public int NetDongu { get { return yukariDonguSayisi - asagiDonguSayisi; } }

    // CharacterController'a ışınlanma sırasında dokunmamız gerekiyor; sebebi aşağıda.
    private CharacterController kontrolcu;

    private void Awake()
    {
        kontrolcu = GetComponent<CharacterController>();
    }

    /// <summary>Döngüyü tamamen açar veya kapatır (çatı ve ceza için).</summary>
    public void DonguyuAyarla(bool yeniDurum)
    {
        acik = yeniDurum;
    }

    /// <summary>Yalnızca yukarı ışınlanmayı açar veya kapatır (8. kat için).</summary>
    public void YukariDonguyuAyarla(bool yeniDurum)
    {
        yukariAcik = yeniDurum;
    }

    /// <summary>Yalnızca aşağı ışınlanmayı açar veya kapatır (açılış için).</summary>
    public void AsagiDonguyuAyarla(bool yeniDurum)
    {
        asagiAcik = yeniDurum;
    }

    // ---------- LATEUPDATE ----------
    // Update değil LateUpdate: o karedeki BÜTÜN hareketler bittikten sonra çalışır,
    // yani burada oyuncunun karenin sonundaki KESİN konumunu görürüz.
    //
    // KatYoneticisi de kendi işini LateUpdate'te, ama BU scriptten SONRA yapıyor
    // (DefaultExecutionOrder ile ayarlı). Sebebi: oyuncu B'ye vardığı karede
    // önce burada A'ya taşınır, hemen ardından KatYoneticisi kararı verip
    // sahanlığı yeni kata göre yeniler. İkisi aynı karede olduğu için ekrana
    // çizilen ilk görüntü zaten yenilenmiş sahanlıktır; değişim hiç görünmez.
    private void LateUpdate()
    {
        if (!acik) return;

        float y = transform.position.y;

        if (yukariAcik && y >= UstIsinlanmaKotu)
        {
            // B'ye varıldı: oyuncuyu bir kat aşağı, A'ya taşı.
            Tasi(new Vector3(0f, -DikeyOfset, 0f));
            yukariDonguSayisi++;
        }
        else if (asagiAcik && y <= AltIsinlanmaKotu)
        {
            // A'nın altındaki merdivene inildi: oyuncuyu bir kat yukarı taşı.
            // Kendini aynı noktada, A ile B arasındaki merdivende bulur.
            Tasi(new Vector3(0f, DikeyOfset, 0f));
            asagiDonguSayisi++;
        }

        // NEDEN İKİ EŞİK BİRBİRİNE YAPIŞIK DEĞİL
        // Üst eşik 2,91 m; oradan taşınınca oyuncu ~0'a düşer, alt eşik -0,6'da.
        // Alt eşikten taşınınca oyuncu 2,4'e çıkar, üst eşik 2,91'de.
        // Eşikler bitişik olsaydı oyuncu taşınır taşınmaz öbür eşiği tetikler
        // ve iki nokta arasında titrerdi.
    }

    // ---------- TAŞIMA ----------
    private void Tasi(Vector3 kayma)
    {
        // CharacterController oyuncunun konumunu kendi içinde ayrıca tutar ve
        // açıkken elle yazılan konumu bir sonraki karede geri alır.
        // Kapatıp taşıyıp tekrar açmak, ona yeni konumu kabul ettirmenin yoludur.
        kontrolcu.enabled = false;

        transform.position += kayma;

        kontrolcu.enabled = true;

        // SyncTransforms, yeni konumu fizik dünyasına ANINDA bildirir;
        // yoksa ışınlanmadan hemen sonraki karede oyuncu zemine gömülebilir.
        Physics.SyncTransforms();
    }
}
