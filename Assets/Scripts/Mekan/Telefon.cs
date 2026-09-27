// Telefon.cs
// Kapıcı odasının masasındaki telefon (#56). Çalarken bakınca "AÇ (E)"; açınca ahize kalkar
// ve konuşma not ekranında okunur (YoneticiNotu'nun ekranı: aynı kâğıt, aynı kapatma tuşları).
// Çalmıyorken sessiz bir eşya: etkileşim yok.
//
// Not ekranı bu nesnenin çarpıştırıcısı olmayan bir çocuğunda ("Konusma"); böylece nişan hep
// telefona gelir, ekran yalnızca telefon açılınca açılır.

using UnityEngine;

public class Telefon : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private YoneticiNotu konusmaEkrani;
    [SerializeField] private Transform ahize;

    private static AudioClip ahizeSesi;
    private Vector3 ahizeYeri;

    private void Awake()
    {
        if (ahize != null) ahizeYeri = ahize.localPosition;
    }

    public string Ipucu { get { return Dil.Getir("ipucu.telefon"); } }

    public bool EtkilesimeAcik { get { return TelefonZili.Ornek != null && TelefonZili.Ornek.Caliyor; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        string konusma = TelefonZili.Ornek.Ac();
        if (ahizeSesi == null) ahizeSesi = Resources.Load<AudioClip>("Sesler/Telefon_Ahize");
        if (ahizeSesi != null) AudioSource.PlayClipAtPoint(ahizeSesi, transform.position, 0.8f);
        // Ahize çataldan kalkar (kulağa götürülmüş gibi masadan biraz yukarıda durur).
        if (ahize != null) ahize.localPosition = ahizeYeri + Vector3.up * 0.12f;
        if (konusmaEkrani != null)
        {
            konusmaEkrani.Ayarla(konusma);
            konusmaEkrani.Etkilesim(oyuncu);
        }
    }

    private void Update()
    {
        // Konuşma ekranı kapanınca ahize yerine iner.
        if (ahize != null && ahize.localPosition != ahizeYeri && (konusmaEkrani == null || konusmaEkrani.EtkilesimeAcik))
            ahize.localPosition = ahizeYeri;
    }
}
