// KatZinciriVarligi.cs
// Bir katın içerik dosyası (#54, #55): kilit zinciri, notlar ve dekor.
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

// CreateAssetMenu: Project penceresinde sağ tık → Create menüsüne "Huzur/Kat Zinciri" ekler.
[CreateAssetMenu(fileName = "Zincir_Yeni", menuName = "Huzur/Kat Zinciri")]
public class KatZinciriVarligi : ScriptableObject
{
    [TextArea] public string aciklama;
    public KilitZinciri zincir = new KilitZinciri();
    public List<KatNotu> notlar = new List<KatNotu>();
    public List<KatSusu> suslar = new List<KatSusu>();
}
