// CopPoseti.cs
// Bir nesnenin "alınabilir çöp poşeti" olduğunu söyleyen İŞARET bileşeni.
//
// İçinde neredeyse hiç kod yok, olması da gerekmiyor. Tek işi, oyuncunun
// attığı ışın bir nesneye çarptığında "bu bir poşet mi?" sorusuna cevap vermek.
//
// NEDEN İSİMLE DEĞİL BİLEŞENLE
// Nesnenin adına bakıp `if (carpan.name == "Cop_Poseti")` de yazabilirdik.
// Ama isim bir gün değişirse kod sessizce çalışmaz olurdu ve derleyici
// bunu fark etmezdi. Bileşen varsa ya vardır ya yoktur; yazım hatası olmaz.

using UnityEngine;

public class CopPoseti : MonoBehaviour
{
    [Tooltip("Poşet alındıktan sonra tekrar alınmasın diye işaretlenir. " +
             "Kat değişince sahanlıkla birlikte yenilenir.")]
    [SerializeField] private bool alindi;

    /// <summary>Bu poşet daha önce alındı mı?</summary>
    public bool Alindi { get { return alindi; } }

    /// <summary>Poşeti alır: işaretler ve gizler.</summary>
    public void Al()
    {
        alindi = true;
        gameObject.SetActive(false);
    }

    /// <summary>Yeni katta poşeti geri getirir.</summary>
    public void Yenile()
    {
        alindi = false;
        gameObject.SetActive(true);
    }
}
