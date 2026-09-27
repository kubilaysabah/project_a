// KatZinciriVarligi.cs
// Bir katın içerik dosyası (#54, #55, #62, #63): kilit zinciri, notlar, dekor, kendiliğinden çalan
// eşyalar ve yırtık çizimin parçaları.
//
// ScriptableObject: sahnedeki bir nesneye değil, projedeki bir DOSYAYA ait veri kabı
// (anomali kartları da böyle). Zincirin kendisi (KilitZinciri) Unity'siz, düz C# ve testli;
// notlar ve dekor ise model şablonlarına (prefab) başvurduğu için burada, Unity tarafında.
// Her katın bir dosyası var (Assets/Zincirler/Kat_1.asset ...); geliştirme için beş kilit
// türünü birden kullanan bir test zinciri de duruyor (Zincir_Test.asset).

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dairede bir şeyin yeri. İki yolla yazılır:
///   Yol dolu : bir çapa nesnesine göre (dairenin kökünden yolu, "Esyalar/Tezgah"); X, Y, Z o
///              nesnenin kendi eksenlerinde, Donus onun y ekseninde (derece). Eşyanın yanına
///              konacak şeyler için (kavanozun yanındaki not).
///   Yol boş  : planın eksenlerinde: X = u, Z = v, Y yükseklik (DairePlani); OnU/OnV nesnenin ön
///              yüzünün baktığı plan yönü. Odaya serpiştirilen şeyler için (koliler).
/// Yatik: nesne yatırılır (duvar kâğıdı gibi dik duran bir not masaya konunca).
/// </summary>
[System.Serializable]
public class KatYerlesimi
{
    public string Yol = "";
    public float X, Y, Z;
    public float Donus;
    public float OnU, OnV = 1f;
    public bool Yatik;
}

// ": KatYerlesimi" (kalıtım): KatNotu, KatYerlesimi'nin bütün alanlarını (Yol, X, Y, Z...) aynen
// taşır ve üstüne kendi alanını ekler. Yer yazma kuralı iki sınıfta tekrar edilmesin diye.
[System.Serializable]
public class KatNotu : KatYerlesimi
{
    public string Metin;          // metin tablosundaki anahtar ("not.kat1_kavanoz")
}

[System.Serializable]
public class KatSusu : KatYerlesimi
{
    public GameObject Sablon;     // koyulacak modelin şablonu (koli...)
}

/// <summary>
/// Katta kendiliğinden çalan bir eşya (#62): dairedeki bir mobilyayı (radyo) "çalan eşya" yapar.
/// Yol: eşyanın dairenin kökünden yolu; Oda: bulunduğu oda (kapısı kapalıyken ses boğulur);
/// Gecikme: oyuncu daireye girdikten kaç saniye sonra çalmaya başlar.
/// </summary>
[System.Serializable]
public class KatSesi
{
    public string Yol;
    public string Oda;
    public AudioClip Klip;
    public float Gecikme = 5f;
    [Range(0f, 1f)] public float Ses = 0.8f;
}

/// <summary>
/// Yırtık bir çizimin bir parçası (#63). Kimlik: zincirdeki Parca eşyasının kimliği; Doku: tam
/// resim boyunda, yalnızca bu parçanın bölgesi opak doku; Bolge: o bölgenin dokudaki yeri (UV,
/// 0-1, sol alttan). Dünyadaki kâğıt parçası yalnızca Bolge'yi gösterir; yakın bakış ekranı
/// bütün dokuyu çizer (parçalar üst üste birleşir).
/// </summary>
[System.Serializable]
public class KatParcasi
{
    public string Kimlik;
    public Texture2D Doku;
    public Rect Bolge;
}

// CreateAssetMenu: Project penceresinde sağ tık → Create menüsüne "Huzur/Kat Zinciri" ekler.
[CreateAssetMenu(fileName = "Zincir_Yeni", menuName = "Huzur/Kat Zinciri")]
public class KatZinciriVarligi : ScriptableObject
{
    [TextArea] public string aciklama;
    public KilitZinciri zincir = new KilitZinciri();
    public List<KatNotu> notlar = new List<KatNotu>();
    public List<KatSusu> suslar = new List<KatSusu>();
    public List<KatSesi> sesler = new List<KatSesi>();
    public List<KatParcasi> parcalar = new List<KatParcasi>();
    // Parçaların yere serilen kâğıdının gerçek boyu (metre): resmin bütünü bu kadar geniş olurdu.
    public float cizimBoyu = 0.30f;
}
