// SonsuzMerdiven.cs
// Oyunun en önemli hilesi: oyuncu sonsuza kadar merdiven çıkar,
// ama sahnede tek bir sahanlık vardır.
//
// NASIL ÇALIŞIR
// Sahnede iki sahanlık var: A (altta) ve B (üstte). B, A'nın birebir aynısıdır.
// Oyuncu merdiveni çıkıp B'ye adımını attığı anda, onu sessizce
// 6,32 metre geri ve 3 metre aşağı taşırız. Bu, tam olarak A'daki
// aynı noktaya denk gelir. İki sahanlık birebir aynı olduğu için
// ekrandaki görüntü değişmez; oyuncu hiçbir şey olmadığını sanır.
//
// NEDEN GÖZ FARK ETMEZ
// Işınlanmadan önceki ve sonraki kare piksel piksel aynıdır: aynı duvarlar,
// aynı kapılar, aynı ışık, aynı bakış açısı. Beynin hareket olarak algılayacağı
// hiçbir fark yoktur. Oyuncu yukarı çıktığını sanır, oysa yerinde saymaktadır.

using UnityEngine;

// Ölçüler MekanOlculeri.cs dosyasında; sınıf adını tekrar yazmadan kullanıyoruz.
using static MekanOlculeri;

[RequireComponent(typeof(CharacterController))]
public class SonsuzMerdiven : MonoBehaviour
{
    [Header("Durum")]
    [Tooltip("Döngü açık mı? 8. kata gelindiğinde kapatılacak, yoksa oyuncu " +
             "çatıya hiç varamaz ve sonsuza kadar döner durur.")]
    [SerializeField] private bool acik = true;

    [Tooltip("Kaç kez yukarı döngü yapıldı. Kat sayacı (#4) bunu okuyacak.")]
    public int yukariDonguSayisi;

    [Tooltip("Kaç kez aşağı döngü yapıldı.")]
    public int asagiDonguSayisi;

    // CharacterController'a ışınlanma sırasında dokunmamız gerekiyor; sebebi aşağıda.
    private CharacterController kontrolcu;

    private void Awake()
    {
        kontrolcu = GetComponent<CharacterController>();
    }

    // Döngüyü dışarıdan açıp kapatmak için. Kat sayacı 8'e ulaşınca
    // bu fonksiyon false ile çağrılacak ve oyuncu gerçekten üst sahanlığa çıkacak.
    public void DonguyuAyarla(bool yeniDurum)
    {
        acik = yeniDurum;
    }

    // ---------- LATEUPDATE ----------
    // Update değil LateUpdate kullanıyoruz.
    // Update: oyuncu kontrolcüsünün oyuncuyu hareket ettirdiği an.
    // LateUpdate: o karedeki BÜTÜN hareketler bittikten sonra çalışır.
    // Yani burada oyuncunun karenin sonundaki KESİN konumunu görürüz.
    // Update'te baksaydık, hareketten önceki eski konumu okuma riskimiz olurdu.
    private void LateUpdate()
    {
        if (!acik) return;

        // Sadece z eksenine bakıyoruz, çünkü merdiven +z yönünde çıkıyor
        // ve sahne orijinde kurulu. Bu varsayım MekanOlculeri'nde de yazılı.
        float z = transform.position.z;

        if (z >= UstIsinlanmaEsigi)
        {
            // Yukarı çıkıldı: oyuncuyu geri ve aşağı taşı.
            Tasi(new Vector3(0f, -DikeyOfset, -IleriOfset));
            yukariDonguSayisi++;
        }
        else if (z <= AltIsinlanmaEsigi)
        {
            // Aşağı inildi: oyuncuyu ileri ve yukarı taşı.
            Tasi(new Vector3(0f, DikeyOfset, IleriOfset));
            asagiDonguSayisi++;
        }

        // NEDEN İKİ EŞİK BİRBİRİNE YAPIŞIK DEĞİL
        // Üst eşik 5,50 m; oradan taşınınca oyuncu -0,82 m'ye düşer.
        // Alt eşik -1,20 m. Yani ışınlanma sonrası oyuncu alt eşiğin
        // 38 cm uzağında kalır. Eşikler bitişik olsaydı, taşınır taşınmaz
        // diğer eşiği tetikler ve oyuncu iki nokta arasında titrerdi.
    }

    // ---------- TAŞIMA ----------
    private void Tasi(Vector3 kayma)
    {
        // CharacterController'ı KAPATMADAN transform.position'ı değiştiremeyiz.
        // Sebebi şu: CharacterController oyuncunun konumunu kendi içinde ayrıca tutar
        // ve her karede onu geri yazar. Açıkken konumu elle değiştirirsek,
        // bileşen bir sonraki karede "hayır, sen şuradaydın" deyip geri alır.
        // Kapatıp taşıyıp tekrar açmak, ona yeni konumu kabul ettirmenin yoludur.
        kontrolcu.enabled = false;

        transform.position += kayma;

        kontrolcu.enabled = true;

        // Fizik motorunun çarpışma verisi bir kare geriden gelir.
        // SyncTransforms, yeni konumu fizik dünyasına ANINDA bildirir;
        // yoksa ışınlanmadan hemen sonraki karede oyuncu zemine gömülebilir.
        Physics.SyncTransforms();
    }
}
