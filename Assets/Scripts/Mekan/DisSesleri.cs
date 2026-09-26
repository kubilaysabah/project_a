// DisSesleri.cs
// Apartmanın dışından ara sıra gelen sesleri çalar: köpek havlaması, uzak silah sesi,
// pencereden içeri süzülen rüzgâr (#37).
//
// Ne zaman ve hangi türün çalacağını DisSesPlani (düz C#, test edilen kural) söyler.
// Bu bileşen saati sayar, zamanı gelince KatYoneticisi'ne "şu an başka bir ses çalıyor
// mu?" diye sorar; çalıyorsa (kat olayı, anomali sesi, ceza) araya girmez, iki saniye
// sonra yeniden dener. Şahan'ın kuralı: sesler birbirinin içine girmesin.
//
// Ses 2B çalar: dışarıdan, pencerenin ardından geldiği hissi kayıtların kendisinde
// (boğukluk ve uzun yankı, kayit_isle.py). 3B olsaydı oyuncunun merdivende nerede
// olduğuna göre ses bir anda bir yandan öbür yana atlayabilirdi.

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DisSesleri : MonoBehaviour
{
    // Türler; sıraları DisSesPlani'ye verilen tür numaralarıdır: 0 köpek, 1 silah, 2 rüzgâr.
    // Bir türün dizisi boşsa (ör. kaydı henüz eklenmediyse) o tür hiç çalmaz.
    [SerializeField] private AudioClip[] kopek;
    [SerializeField] private AudioClip[] silah;
    [SerializeField] private AudioClip[] ruzgar;
    [SerializeField] private int tohum = 0;       // 0 = her oyun farklı

    private AudioSource kaynak;
    private KatYoneticisi yonetici;

    // Kat olayları da dış ses çalarken araya girmesin diye bunu sorar (KatOlayCalici).
    private static DisSesleri ornek;
    public static bool Caliyor { get { return ornek != null && ornek.kaynak != null && ornek.kaynak.isPlaying; } }
    private DisSesPlani plan;
    private float kalan;                          // sıradaki dış sese kalan süre

    private void Awake()
    {
        kaynak = GetComponent<AudioSource>();
        ornek = this;
        yonetici = FindAnyObjectByType<KatYoneticisi>();
        plan = new DisSesPlani(3, tohum != 0 ? tohum : System.Environment.TickCount + 31337);
        kalan = plan.SonrakiBekleme();
    }

    private void Update()
    {
        // Başlık ekranında, ceza karanlığında ve tur bitince (çatı, sabah) dış ses yok.
        if (DuraklatmaMenusu.BaslikEkraninda || AudioListener.pause) return;
        if (yonetici != null && yonetici.TurBitti) return;

        kalan -= Time.deltaTime;
        if (kalan > 0f) return;

        // Başka bir ses çalıyorsa bekle; kuyruğa girmek yerine kısa aralıkla yeniden sor.
        if (kaynak.isPlaying || (yonetici != null && yonetici.SesMesgul))
        {
            kalan = DisSesPlani.ErtelemeSuresi;
            return;
        }

        AudioClip[][] turler = { kopek, silah, ruzgar };
        var sesiVar = new bool[turler.Length];
        for (int i = 0; i < turler.Length; i++) sesiVar[i] = turler[i] != null && turler[i].Length > 0;

        int tur = plan.SonrakiTur(sesiVar);
        if (tur >= 0)
        {
            AudioClip[] cesitler = turler[tur];
            kaynak.clip = cesitler[plan.CesitSec(cesitler.Length)];
            kaynak.Play();
            Debug.Log($"[DIŞ SES] {kaynak.clip.name}");
        }
        kalan = plan.SonrakiBekleme();
    }
}
