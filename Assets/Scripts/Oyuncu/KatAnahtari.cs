// KatAnahtari.cs
// Katın demir kapısını açan anahtar (#52). Bakınca "AL (E)"; E'ye basınca cebe girer,
// yerden kalkıp ele süzülür (UcanKopya), küçük bir pirinç şıkırtısı duyulur.
//
// Anahtar HANGİ katın anahtarıysa yalnızca o katın kapısını açar (Envanter.Kat). Kat
// numarasını sahanlık yeni bir katı gösterirken KatYoneticisi yazar (Ayarla). Nereye
// konacağı şimdilik sabit (kurucu koyar); #55'te her katın içerik dosyası söyleyecek.

using UnityEngine;

public class KatAnahtari : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private int kat;
    private static AudioClip sikirti;

    public int Kat { get { return kat; } }

    public string Ipucu { get { return Dil.Getir("ipucu.al"); } }

    // El doluysa da istem görünür ("AL"), ama basınca alınmaz ve cep kısa bir uyarı
    // verir; oyuncu önce elindekini kullanması gerektiğini böyle öğrenir (#54'te eşya bırakma).
    public bool EtkilesimeAcik { get { return gameObject.activeSelf; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!Cep.Al(EsyaTuru.KatAnahtari, kat)) return;
        UcanKopya.ElineUcur(transform);
        if (sikirti == null) sikirti = Resources.Load<AudioClip>("Sesler/Anahtar_Al");
        if (sikirti != null) AudioSource.PlayClipAtPoint(sikirti, transform.position, 0.6f);
        gameObject.SetActive(false);
    }

    /// <summary>Sahanlık bu katı gösteriyor: anahtar bu katın; alınmadıysa görünür.</summary>
    public void Ayarla(int yeniKat, bool gorunur)
    {
        kat = yeniKat;
        gameObject.SetActive(gorunur);
    }
}
