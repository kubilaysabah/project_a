// IsikDugmesi.cs
// Kapının yanındaki bakalit elektrik düğmesi (#100): basınca tık; dairede elektrik varsa ve o odanın
// lambası yanabiliyorsa lamba söner ya da yanar. Yoksa yalnızca tık.
//
// Piyasadaki korku oyunlarının (Amnesia, Outlast, Phasmophobia) küçük ama güçlü bir alışkanlığı:
// karanlık bir odaya giren oyuncunun ilk refleksi düğmeye basmaktır. Düğme tıklar ve hiçbir şey
// olmaz: bu boşluk, odanın karanlığını oyuncunun kendisine söyletir. 4. katta sigorta takılınca aynı
// düğme işe yarar; ışık geri gelir, oyuncu onu kendisi söndürüp yakabilir.
//
// Düğme hangi odanın? Kurucu (GrayboxKurucu_Daire.DugmeKur) kapının odasını yazar (odaAdi). Lambayı
// dairenin DaireLambalari bileşeni yakar; düğme yalnızca "bu odayı elle çevir" der.

using UnityEngine;

public class IsikDugmesi : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private string odaAdi;          // düğmenin odası ("Salon", "Oda_A" ...)
    private static AudioClip tik;
    private float sonBasma = -10f;

    public string Ipucu { get { return Dil.Getir("ipucu.ac"); } }

    // Çok sık basılmasın (üst üste tık sesleri): saniyede en fazla dört kez.
    public bool EtkilesimeAcik { get { return Time.time - sonBasma > 0.25f; } }

    public void Etkilesim(GameObject oyuncu)
    {
        sonBasma = Time.time;
        if (tik == null) tik = Resources.Load<AudioClip>("Sesler/Dugme_Bakalit");
        if (tik == null) tik = Resources.Load<AudioClip>("Sesler/Lamba_Sonme");   // yeni ses yoksa eski anahtar tıkı
        if (tik != null) AudioSource.PlayClipAtPoint(tik, transform.position, 0.55f);

        // Dairenin lambaları (düğme dairenin "Ayrintilar" grubunun altında).
        DaireLambalari lambalar = GetComponentInParent<DaireLambalari>();
        if (lambalar != null && !string.IsNullOrEmpty(odaAdi)) lambalar.ElleDegistir(odaAdi);
    }
}
