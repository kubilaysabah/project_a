// OrtamSesi.cs
// Normal bir katın ses dokusundaki TEK bir katman (uğultu, uzak TV, bebek, boru, rüzgâr).
// Sesin kendisini AudioSource çalar; bu bileşen yalnızca bir şeyi garanti eder:
// sahnedeki bütün kopyalar AYNI SAATE kilitli çalsın.
//
// ---------- NEDEN SAATE KİLİT ----------
// Sahnede üst üste birkaç sahanlık var ve her birinde aynı sesler çalıyor.
// Oyuncu üst kata ışınlandığında, bir sahanlığın TV'sinden diğer sahanlığın TV'sine
// "geçer". İki TV'nin kaydı farklı yerlerdeyse konuşma bir anda başka bir cümleye
// atlar ve kulak hileyi yakalar. Göz için yaptığımız "her kat birebir aynı" kuralı
// kulak için de geçerli.
//
// Çözüm: her kaynak, çalmaya başladığı anda kaydın neresinde olması gerektiğini ortak
// bir saatten hesaplar. Ortak saat AudioSettings.dspTime: ses sisteminin kendi saati,
// oyun başladığından beri geçen süre. Aynı uzunluktaki iki kayıt aynı saate göre
// konumlanırsa aynı yerde olur. Bir kart sesi susturup geri açsa bile (nesneyi
// gizleyip gösterse bile) kaynak yeniden kilitlenir.

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OrtamSesi : MonoBehaviour
{
    private AudioSource kaynak;

    private void Awake()
    {
        kaynak = GetComponent<AudioSource>();
    }

    // OnEnable: nesne her görünür (etkin) olduğunda çalışır; oyun başlarken de,
    // bir anomali kartı sesi gizleyip geri açtığında da.
    private void OnEnable()
    {
        if (!Application.isPlaying || kaynak == null || kaynak.clip == null) return;

        kaynak.Play();

        // Kaydın içindeki konum "örnek" (sample) cinsinden. Saati saniyede örnek
        // sayısıyla çarpıp kayıt uzunluğuna göre kalanını alıyoruz (%): böylece
        // döngü kaçıncı turunda olursa olsun doğru yer bulunur.
        AudioClip k = kaynak.clip;
        long konum = (long)(AudioSettings.dspTime * k.frequency) % k.samples;
        kaynak.timeSamples = (int)konum;
    }
}
