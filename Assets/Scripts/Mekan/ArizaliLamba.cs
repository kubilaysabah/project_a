// ArizaliLamba.cs
// Asansörün üstündeki arızalı lamba (#111): yanar, söner, kekeler. Deseni ArizaliLambaKurali verir.
//
// NEDEN
// Kafa lambası dokuzuncu katta çiviye asılınca (#108) 8. ve 9. katın sahanlığı tamamen karanlığa gömülüyordu
// (Şahan'ın testi, 2026-10-06). Bu lamba asansörün yerini gösterir; yanıp sönmesi de binanın arızalı hâline uyar.
//
// NEREDE YANAR
// Lamba her sahanlıkta asansörün üstünde durur (graybox kurar), ama yalnızca sahanlık 8. KATI gösterirken yanar.
// Tek sahanlık her katı gösterdiği için (#23) "hangi kattayım" sorusunu yöneticiye sorar (KatYoneticisi.SahanlikKati).
// 7. kattan yukarı bakınca üst sahanlık 8. katı gösterir: lambanın kırpıştığı oradan da görülür. Dokuzuncu katın
// sahanlığındaki lamba ise hep yanar (herZaman; o set ayrı, yönetici onun katını bilmez).
//
// IŞIK + CAM + SES
// Işık gölgesiz bir nokta ışığıdır (ucuz). Ampul camı yanarken parlayan, sönerken sönük malzemeye geçer.
// Yanarken hafif bir elektrik vızıltısı duyulur (Lamba_Vinlama, 3B, lambanın yanından).

using UnityEngine;

public class ArizaliLamba : MonoBehaviour
{
    [SerializeField] private Light isik;            // nokta ışığı (graybox bağlar)
    [SerializeField] private Renderer cam;          // ampulün camı
    [SerializeField] private Material camYanik;     // parlayan cam (HDR, ışıma efekti tetikler)
    [SerializeField] private Material camSonuk;     // sönük cam
    [SerializeField] private bool herZaman;         // dokuzuncu kat: katı sormadan hep yanar
    [SerializeField] private int tohum;             // iki lamba aynı anda aynı şeyi yapmasın

    private const float VizilSesi = 0.10f;          // vızıltının en yüksek sesi
    /// <summary>Başlık ekranı açıkken her sahanlığın arızalı lambası yanar (#128); DuraklatmaMenusu yazar.</summary>
    public static bool BaslikZorla;
    private float temelSiddet;                      // ışığın tam yanıkken gücü (sahnedeki değer)
    private AudioSource vizilti;
    private KatYoneticisi yonetici;
    private bool camYanikMi;

    private void Awake()
    {
        if (isik != null) temelSiddet = isik.intensity;
        // Vızıltı: döngülü 3B ses, lambanın yanından; yakına gelince duyulur.
        AudioClip klip = Resources.Load<AudioClip>("Sesler/Lamba_Vinlama");
        if (klip != null)
        {
            vizilti = gameObject.AddComponent<AudioSource>();
            vizilti.clip = klip;
            vizilti.loop = true;
            vizilti.playOnAwake = false;
            vizilti.spatialBlend = 1f;
            vizilti.minDistance = 0.8f;
            vizilti.maxDistance = 7f;
            // #119: Doğrusal düşüş: 7 m'de gerçekten sıfır (logaritmik eğri uzakta ~%1'de takılı kalıyordu;
            // dokuzuncu katın lambası hep yanık, vınlaması bütün oyuna sızıyordu). Pes: aynı klip kafa lambasının
            // gerilim uyarısında da çalıyor (#105); balast vınlaması daha kalın olsun ki ikisi karışmasın.
            vizilti.rolloffMode = AudioRolloffMode.Linear;
            vizilti.pitch = 0.6f;
            vizilti.volume = 0f;
        }
    }

    private void Update()
    {
        if (yonetici == null) yonetici = FindAnyObjectByType<KatYoneticisi>();
        // Bu sahanlık şu an 8. katı mı gösteriyor? (Dokuzuncu kat sormaz, hep yanar.)
        // BaslikZorla (#128): başlık ekranı A sahanlığındaki asansöre bakar; lamba orada da titrer.
        bool aktif = herZaman || BaslikZorla || (yonetici != null && yonetici.SahanlikKati(transform) == KatSayaci.CatiKati);
        float p = aktif ? ArizaliLambaKurali.Parlaklik(Time.time, tohum) : 0f;
        if (isik != null)
        {
            isik.enabled = p > 0.01f;
            isik.intensity = temelSiddet * p;
        }
        // Cam yalnızca durum değişince değiştirilir: her kare malzeme atamak gereksiz iş.
        bool yanik = p > 0.5f;
        if (cam != null && yanik != camYanikMi)
        {
            cam.sharedMaterial = yanik ? camYanik : camSonuk;
            camYanikMi = yanik;
        }
        if (vizilti != null)
        {
            vizilti.volume = aktif ? VizilSesi * p : 0f;
            if (aktif && !vizilti.isPlaying) vizilti.Play();
            else if (!aktif && vizilti.isPlaying) vizilti.Stop();
        }
    }
}
