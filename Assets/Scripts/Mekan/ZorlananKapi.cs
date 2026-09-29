// ZorlananKapi.cs
// Sahanlıktaki kapalı daire kapısını zorlamak (#83, Şahan'ın 1. kat testi, 2026-09-29).
//
// Her katta dört daireden yalnızca biri açıktır; öbür üçünün kapısı kilitli. Şahan: "daire kapıları
// açılmasalar bile zorlanabilmeli". Oyuncu kapıya bakınca nişangâh dolar; dokununca kolun kilitli
// sesi gelir ve kanat kilide dayanıp birkaç derece içeri esner, geri gelir. Kapı açılmaz; oyuncu
// kapının gerçek olduğunu ve kilitli olduğunu hisseder.
//
// Bileşeni KatYoneticisi, sahanlıktaki kapıları her kat için ayarlarken kapalı kapılara ekler
// (açık dairenin kapısında kapatır). 7. katta kapılarda "kapıyı çal" bileşeni (SahanlikKapisi) varsa
// o geçerlidir; bu bileşen o zaman etkileşime açık olmaz.

using System.Collections;
using UnityEngine;

public class ZorlananKapi : MonoBehaviour, IEtkilesimli
{
    // Esneme: kanadın içeri doğru kaç derece gideceği ve bir sarsıntının süresi.
    private const float EsnemeAcisi = 1.8f;
    private const float SarsintiSuresi = 0.09f;

    private Transform mentese;                  // kanadın döndüğü nokta (kapının "Mentese" çocuğu)
    private Coroutine zorlama;                  // sürmekte olan esneme (bitmeden yenisi başlamaz)
    private static AudioClip kilitliSes;

    private void Awake()
    {
        mentese = transform.Find("Mentese");
    }

    // IEtkilesimli sözleşmesi. Yazı ekranda gösterilmiyor (#72); ipucu yine de "KİLİTLİ".
    public string Ipucu { get { return Dil.Getir("ipucu.kilitli"); } }

    // Etkileşime açık: bileşen açıksa, kapı şu an esnemiyorsa ve kapıda "çal" bileşeni yoksa.
    // enabled: bileşen kapatılmışsa (açık dairenin kapısı) Etkilesimci onu yine de bulur; bu yüzden
    // burada da sorulur.
    public bool EtkilesimeAcik
    {
        get
        {
            if (!enabled || zorlama != null || mentese == null) return false;
            SahanlikKapisi cal = GetComponent<SahanlikKapisi>();
            return cal == null || !cal.enabled;
        }
    }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        if (kilitliSes == null) kilitliSes = Resources.Load<AudioClip>("Sesler/IcKapi_Kilitli");
        if (kilitliSes != null) AudioSource.PlayClipAtPoint(kilitliSes, mentese.position, 0.7f);
        zorlama = StartCoroutine(Esne());
    }

    // İki kısa sarsıntı: kanat kilide dayanıp içeri esner, geri gelir; ikincisi biraz daha az.
    // Pozitif açı kapıyı içeri (daireye doğru) açar; menteşeler bunun için aynalı kuruldu.
    private IEnumerator Esne()
    {
        Quaternion kapali = mentese.localRotation;
        foreach (float aci in new[] { EsnemeAcisi, EsnemeAcisi * 0.6f })
        {
            for (float t = 0f; t < SarsintiSuresi * 2f; t += Time.deltaTime)
            {
                // 0 -> aci -> 0: yarım sinüs dalgası.
                float oran = Mathf.Sin(Mathf.PI * t / (SarsintiSuresi * 2f));
                mentese.localRotation = kapali * Quaternion.Euler(0f, aci * oran, 0f);
                yield return null;
            }
        }
        mentese.localRotation = kapali;
        zorlama = null;
    }

    // Kat değişince (kapı açık daireye dönerse) yarım kalan esneme durur, kanat yerine döner.
    private void OnDisable()
    {
        if (zorlama != null && mentese != null)
        {
            StopCoroutine(zorlama);
            zorlama = null;
        }
    }
}
