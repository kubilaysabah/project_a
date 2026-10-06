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

    // Toplama sesi. "static": bütün poşetler tek bir kopyayı paylaşır; ilk toplamada
    // bir kez yüklenir, sonrakilerde yeniden aranmaz.
    private static AudioClip hisirti;

    // ---------- SÖZLEŞME ----------

    /// <summary>Ekranda görünecek eylem adı, seçili dilde (#15): "TOPLA" / "PICK UP".</summary>
    public string Ipucu { get { return Dil.Getir("ipucu.topla"); } }

    /// <summary>Alınmış poşet artık etkileşime açık değildir.</summary>
    // #113 (Şahan, 2026-10-06): poşet toplamanın #59'dan beri hiçbir sonucu yoktu ama oyuncu toplamaya
    // uğraşıyordu. Poşetler sahanlıkta SÜS olarak kalır (kat planı yine çizer, yerleri yine hatırlanır);
    // nişangâh onları göstermez, E onlara bir şey yapmaz. Geri açmak için yalnızca bu satır değişir.
    public bool EtkilesimeAcik { get { return false; } }

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

        // Poşetin görsel bir kopyası ele doğru süzülür (#43); asıl poşet hemen alınır.
        UcanKopya.ElineUcur(transform);
        Al();
        toplayici.PosetSay();

        // Naylon hışırtısı, poşetin durduğu yerden (#25). PlayClipAtPoint o noktada
        // geçici bir ses kaynağı kurar ve ses bitince kendisi siler; tek seferlik
        // bir ses için en kısa yol.
        if (hisirti == null) hisirti = Resources.Load<AudioClip>("Sesler/Poset_Hisirti");
        if (hisirti != null) AudioSource.PlayClipAtPoint(hisirti, transform.position, 0.8f);
    }

    // ---------- DURUM ----------

    /// <summary>Poşeti alır: işaretler ve gizler.</summary>
    public void Al()
    {
        alindi = true;
        gameObject.SetActive(false);
    }

    /// <summary>Yeni katta poşeti geri getirir.</summary>
    /// <summary>
    /// Poşeti o katın durumuna göre hazırlar.
    /// mevcut = false: bu katta bu poşet YOK (çöp planı daha az poşet çekti).
    /// alinmis = true: poşet vardı ama oyuncu bu turda zaten topladı.
    /// İki durumda da poşet görünmez; ama yalnızca "alinmis" durumu toplanmış
    /// sayılır. Olmayan poşet eksik görev sayılmaz, çünkü kapalı nesneleri
    /// görev kontrolü hiç görmüyor.
    /// </summary>
    public void Ayarla(bool mevcut, bool alinmis)
    {
        alindi = alinmis;
        gameObject.SetActive(mevcut && !alinmis);
    }

    public void Yenile()
    {
        alindi = false;
        gameObject.SetActive(true);
    }
}
