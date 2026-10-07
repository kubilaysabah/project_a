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
    private AudioSource konusmaKaynagi;   // seslendirilmiş konuşma (#90; kayıt yoksa yalnızca yazı)

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
        KonusmayiCal(konusma);
    }

    // Seslendirme (#90): "not.telefon_kat2" için Resources/Sesler/Konusmalar/telefon_kat2 varsa
    // ahizeden (2B, kulakta) çalar; yazı ekranı altyazı olarak kalır. Kayıt yoksa hiçbir şey olmaz:
    // Şahan kayıtları yaptıkça dosyalar klasöre konur, kod değişmez (Docs/seslendirme.md).
    private void KonusmayiCal(string anahtar)
    {
        if (string.IsNullOrEmpty(anahtar)) return;
        string ad = anahtar.StartsWith("not.") ? anahtar.Substring(4) : anahtar;
        AudioClip klip = Resources.Load<AudioClip>("Sesler/Konusmalar/" + ad);
        if (klip == null) return;
        if (konusmaKaynagi == null)
        {
            konusmaKaynagi = gameObject.AddComponent<AudioSource>();
            konusmaKaynagi.playOnAwake = false;
            konusmaKaynagi.spatialBlend = 0f;        // ahize kulakta: iki boyutlu
            konusmaKaynagi.bypassListenerEffects = true;   // odanın yankısı hattan gelen sese işlemez
        }
        konusmaKaynagi.clip = klip;
        konusmaKaynagi.volume = 0.85f * Ayarlar.Konusma;   // #118: konuşma kanalı
        konusmaKaynagi.PlayDelayed(0.7f);   // ahize kulağa gidene kadar
    }

    private void Update()
    {
        // #119: konuşma kanalı kaydırıcısı çalan konuşmaya da işler (eskiden yalnızca başlarken okunuyordu).
        if (konusmaKaynagi != null && konusmaKaynagi.isPlaying) konusmaKaynagi.volume = 0.85f * Ayarlar.Konusma;
        // Konuşma ekranı kapanınca ahize yerine iner.
        if (ahize != null && ahize.localPosition != ahizeYeri && (konusmaEkrani == null || konusmaEkrani.EtkilesimeAcik))
        {
            ahize.localPosition = ahizeYeri;
            if (konusmaKaynagi != null && konusmaKaynagi.isPlaying) konusmaKaynagi.Stop();   // ahize kapandı, ses kesilir
        }
    }
}
