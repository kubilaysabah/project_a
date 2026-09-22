// AnomaliEtkisi.cs
// Bir anomalinin sahanlıkta yaptığı TEK bir değişiklik.
//
// Bir anomali kartı bunlardan bir veya birkaç tane taşır. Örnek:
//   "Aralık kapı"     -> tek etki: Kapi_Sol/Kanat nesnesini biraz döndür
//   "Tek terlik"      -> tek etki: Terlik_Sag nesnesini gizle
//   "Yeşil ampul"     -> tek etki: Ampul'ün ışık rengini yeşile çevir
//   "Kapı aralık ve ışık sönük" -> iki etki birden
//
// 60 anomalinin çeşitliliği yeni kod yazmaktan değil, aşağıdaki sekiz türün
// farklı nesnelere farklı değerlerle uygulanmasından doğacak.

using UnityEngine;

// ---------- ETKİ TÜRÜ ----------
/// <summary>Bir etkinin hedef nesnede ne değiştirdiği.</summary>
public enum EtkiTuru
{
    /// <summary>Nesneyi gizler veya gösterir. Kullandığı alan: gorunur</summary>
    Gorunurluk,

    /// <summary>Nesneyi kaydırır. Kullandığı alan: vektor (metre)</summary>
    Konum,

    /// <summary>Nesneyi döndürür. Kullandığı alan: vektor (derece)</summary>
    Donus,

    /// <summary>Nesneyi büyütür veya küçültür. Kullandığı alan: vektor (çarpan)</summary>
    Olcek,

    /// <summary>Malzeme rengini değiştirir. Kullandığı alan: renk</summary>
    Renk,

    /// <summary>Işığın rengini ve şiddetini değiştirir. Kullandığı alanlar: renk, sayi</summary>
    Isik,

    /// <summary>Yazıyı değiştirir (kapı numarası). Kullandığı alan: metin</summary>
    Metin,

    /// <summary>Bir ses çalar. Kullandığı alan: ses</summary>
    Ses
}

// ---------- ETKİNİN KENDİSİ ----------
// [System.Serializable]: Unity'ye "bu sınıfı da kaydedebilirsin ve
// Inspector'da gösterebilirsin" der. Bu etiket olmadan, kartın içindeki
// etki listesi Inspector'da hiç görünmez ve diske kaydedilmez.
//
// Bu sınıf MonoBehaviour veya ScriptableObject DEĞİLDİR; kendi başına var olamaz,
// sadece bir kartın içinde yaşar. Bir zarfın içindeki pusula gibi.
[System.Serializable]
public class AnomaliEtkisi
{
    [Tooltip("Sahanlığın içindeki hedef nesnenin yolu. Örnek: Kapi_Sol/Kanat, " +
             "Kapi_Sag/Terlikler/Terlik_Sag, Paspas_Sol, Ampul. " +
             "Bu isimler GrayboxKurucu tarafından üretilir ve sabittir.")]
    public string hedefYol;

    [Tooltip("Hedef nesnede ne değişecek.")]
    public EtkiTuru tur;

    [Header("Değerler — sadece türün kullandığı alan doldurulur")]

    [Tooltip("Gorunurluk türünde: nesne görünsün mü?")]
    public bool gorunur = true;

    [Tooltip("Konum türünde kaydırma (metre), Donus türünde açı (derece), " +
             "Olcek türünde çarpan.")]
    public Vector3 vektor;

    [Tooltip("Renk ve Isik türlerinde kullanılan renk.")]
    public Color renk = Color.white;

    [Tooltip("Isik türünde ışık şiddeti.")]
    public float sayi = 1f;

    [Tooltip("Metin türünde yazılacak yazı.")]
    public string metin;

    [Tooltip("Ses türünde çalınacak ses dosyası.")]
    public AudioClip ses;

    /// <summary>Kartı Inspector'da okunur kılmak için kısa özet.</summary>
    public override string ToString()
    {
        return $"{tur} → {hedefYol}";
    }
}
