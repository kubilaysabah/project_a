// CopPoseti.cs
// Kapıcının toplayacağı çöp poşeti.
//
// IEtkilesimli sözleşmesini imzalıyor. Bu sayede oyuncunun etkileşim
// sistemi onu tanıyor: yaklaşınca üstünde "TOPLA (E)" yazısı beliriyor,
// tuşa basılınca alınıyor. Oyuncu tarafında poşete özel tek satır kod yok.

using UnityEngine;

public class CopPoseti : MonoBehaviour, IEtkilesimli
{
    [Tooltip("Poşet alındıktan sonra tekrar alınmasın diye işaretlenir. " +
             "Kat değişince sahanlıkla birlikte yenilenir.")]
    [SerializeField] private bool alindi;

    /// <summary>Bu poşet daha önce alındı mı?</summary>
    public bool Alindi { get { return alindi; } }

    // ---------- SÖZLEŞME ----------

    /// <summary>Ekranda görünecek eylem adı.</summary>
    public string Ipucu { get { return "TOPLA"; } }

    /// <summary>Alınmış poşet artık etkileşime açık değildir.</summary>
    public bool EtkilesimeAcik { get { return !alindi && gameObject.activeSelf; } }

    /// <summary>Oyuncu tuşa bastığında çağrılır.</summary>
    public void Etkilesim(GameObject oyuncu)
    {
        // Sayaç oyuncuda duruyor: poşet kendini nasıl alacağını bilir,
        // kaç tane taşındığını ise oyuncu bilir.
        CopToplayici toplayici = oyuncu.GetComponent<CopToplayici>();

        if (toplayici == null)
        {
            Debug.LogWarning("[ÇÖP] Oyuncuda CopToplayici yok, poşet sayılamadı.");
            return;
        }

        Al();
        toplayici.PosetSay();
    }

    // ---------- DURUM ----------

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
