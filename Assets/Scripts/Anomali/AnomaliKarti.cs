// AnomaliKarti.cs
// Bir anomalinin tarifi. Oyunun bütün içeriği bu karttan üretilecek.
//
// ---------- SCRIPTABLEOBJECT NEDİR ----------
// Şimdiye kadar yazdığımız scriptler MonoBehaviour'du: bir nesneye takılıyor,
// sahnede duruyor ve her karede çalışıyorlardı.
//
// ScriptableObject sahnede durmaz. Diskte bir DOSYA olarak yaşar.
// Yemek tarifi kartı gibi düşünün: tarif kartı yemek değildir, mutfakta durmaz,
// kutuda durur. Altmış tarif kartınız olabilir ve hiçbiri pişmiyor olabilir.
// Yemek yapmak istediğinizde kutudan bir kart çeker, üstündekini uygularsınız.
//
// Neden böyle yapıyoruz? Çünkü altmış anomaliyi altmış script olarak yazmak
// üç ayda bitmez. Kart olarak yazmak ise her biri için birkaç dakika sürer
// ve tek satır kod gerektirmez. Projenin "İÇERİK VERİDİR" ilkesi budur.

using UnityEngine;

// ---------- CREATEASSETMENU ----------
// Bu etiket, Unity'nin Assets menüsüne bir satır ekler. O satıra tıklayınca
// bu sınıftan yeni bir dosya üretilir. Etiket olmasaydı kart dosyası
// oluşturmanın kolay bir yolu olmazdı.
//   fileName : yeni dosyanın önerilen adı
//   menuName : menüde görünecek yol
[CreateAssetMenu(fileName = "Anomali_Yeni", menuName = "Huzur/Anomali Kartı")]
public class AnomaliKarti : ScriptableObject, IAnomaliKart
{
    // "ScriptableObject, IAnomaliKart" yazarken iki şey söylüyoruz:
    // bu sınıf bir ScriptableObject'tir VE IAnomaliKart sözleşmesini imzalar.
    // Sözleşme üç bilgi istiyordu: kimlik, katman, zorluk. Aşağıda veriyoruz.

    [Header("Kimlik")]

    [Tooltip("Kartın kısa kimliği. Boş bırakılırsa dosya adı kullanılır.")]
    [SerializeField] private string kimlik;

    [Tooltip("Anomalinin sertlik katmanı.")]
    [SerializeField] private AnomaliKatmani katman = AnomaliKatmani.Fisilti;

    [Tooltip("1 = hemen göze çarpar, 5 = çok zor fark edilir. " +
             "Zorluk eğrisi bu sayıya göre kurulacak.")]
    [Range(1, 5)]
    [SerializeField] private int zorluk = 1;

    [Tooltip("Oyuncunun neyi fark etmesi bekleniyor. Sadece bizim notumuz, " +
             "oyunda hiçbir yerde görünmez.")]
    [TextArea(2, 4)]
    [SerializeField] private string aciklama;

    [Header("Sahanlıkta ne değişecek")]

    [Tooltip("Bu anomalinin uyguladığı değişiklikler. Bir kart birden fazla " +
             "etki taşıyabilir; hepsi aynı anda uygulanır.")]
    [SerializeField] private AnomaliEtkisi[] etkiler = new AnomaliEtkisi[0];

    // ---------- SÖZLEŞMENİN KARŞILANMASI ----------
    // Aşağıdakiler "özellik"tir: dışarıdan okunur, yazılamaz.
    // Kartın içeriği yalnızca Inspector'dan değişsin istiyoruz; hiçbir kod
    // çalışma sırasında bir kartın içini değiştirmemeli. Kart bir tariftir,
    // tarif pişirilirken değişmez.

    /// <summary>Kartın kimliği. Boşsa dosya adı kullanılır.</summary>
    public string Kimlik
    {
        get
        {
            // string.IsNullOrWhiteSpace: metin boş mu, ya da sadece boşluk mu?
            if (string.IsNullOrWhiteSpace(kimlik)) return name;   // name = dosya adı
            return kimlik;
        }
    }

    /// <summary>Anomalinin katmanı.</summary>
    public AnomaliKatmani Katman { get { return katman; } }

    /// <summary>Fark edilme zorluğu, 1 ile 5 arası.</summary>
    public int Zorluk { get { return zorluk; } }

    /// <summary>Bizim notumuz; oyunda görünmez.</summary>
    public string Aciklama { get { return aciklama; } }

    /// <summary>Sahanlığa uygulanacak değişiklikler.</summary>
    public AnomaliEtkisi[] Etkiler { get { return etkiler; } }

    /// <summary>Kartın hiç etkisi yoksa uygulanacak bir şey de yoktur.</summary>
    public bool Gecerli
    {
        get { return etkiler != null && etkiler.Length > 0; }
    }
}
