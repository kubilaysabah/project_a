// CividekiLamba.cs
// Kapıcı odasının duvarındaki çivide asılı duran Cemal'in kafa lambası (#56).
//
// Oyun kapıcı odasında başlar (açılış). Oyuncunun kafasında henüz lamba yoktur; yönetimin notu
// "kafa lambası çivide asılı" der. Lamba çivide asılıyken YANIYOR: Cemal onu açık bırakıp gitmiş.
// Karanlık odayı bu ışık aydınlatır, oyuncu hem notu hem lambayı onun sayesinde görür.
// "AL (E)" denince çividen kalkar, oyuncunun kafasındaki lamba (KafaLambasi) takılır ve yanar;
// çivi duvarda boş kalır.
//
// Etkileşim sözleşmesi (IEtkilesimli): oyuncunun Etkilesimci'si bu nesnenin ne olduğunu bilmez;
// yalnızca Ipucu, EtkilesimeAcik ve Etkilesim'i çağırır.
//
// KAYIT YOK, bilerek: lamba oyuncuda mı, oyunun nasıl başladığından çıkar. Açılışla başlayan oyunda
// (yeni oyun, 1. kattan önce kapatılmış oyun, finalden sonraki yeniden başlangıç) lamba çividedir;
// kayıttan devam eden oyunda oyuncu onu zaten almıştır (KatYoneticisi açılışta KafaLambasi.Birak der).

using UnityEngine;

public class CividekiLamba : MonoBehaviour, IEtkilesimli
{
    // Lambanın çivideyken yanan kendi ışığı (kurucu bağlar). Lamba alınınca nesneyle birlikte kaybolur.
    [SerializeField] private Light isik;

    private KafaLambasi oyuncununLambasi;   // oyuncunun kafasındaki lamba (kameranın bileşeni)

    // Ekranda "AL (E)" yazısı yok artık (#72, nişangâh); eylem adı yine de sözleşmenin parçası.
    public string Ipucu { get { return Dil.Getir("ipucu.al"); } }

    // Oyuncu lambayı zaten taşıyorsa (kayıttan devam) çividen alınacak bir şey yoktur.
    public bool EtkilesimeAcik { get { return Lamba() != null && !Lamba().Takili; } }

    // Zemin kat her etkinleştiğinde (açılış, ya da oyuncu merkeze geri indiğinde, #56) bakılır:
    // oyuncu lambayı almışsa çivideki kopya görünmez. Aynı lamba iki yerde olamaz.
    private void OnEnable()
    {
        if (Lamba() != null && Lamba().Takili) gameObject.SetActive(false);
    }

    public void Etkilesim(GameObject oyuncu)
    {
        // Oyuncunun kafasındaki lambayı tak ve yak (tık sesi KafaLambasi'nda).
        KafaLambasi lamba = oyuncu.GetComponentInChildren<KafaLambasi>(true);
        if (lamba == null) lamba = Lamba();
        if (lamba == null) return;
        lamba.Tak();
        Debug.Log("[LAMBA] Cemal'in kafa lambası çividen alındı.");
        // Çivideki lamba (ışığıyla birlikte) kaybolur; çivi ayrı bir nesne, duvarda kalır.
        gameObject.SetActive(false);
    }

    // Oyuncunun lambasını bir kez arar ve saklar (sahnede tek bir KafaLambasi var).
    private KafaLambasi Lamba()
    {
        if (oyuncununLambasi == null) oyuncununLambasi = FindAnyObjectByType<KafaLambasi>(FindObjectsInactive.Include);
        return oyuncununLambasi;
    }
}
