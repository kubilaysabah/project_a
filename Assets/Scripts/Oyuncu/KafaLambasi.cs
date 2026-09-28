// KafaLambasi.cs
// Kapıcının kafa lambası (#51): bina artık karanlık, ışığı oyuncu taşır.
//
// Hikâye bağı: lamba önceki kapıcı Cemal'in; oyunun ilk dakikasında kapıcı odasındaki
// çividen alınır (#56, CividekiLamba). Açılışla başlayan oyunda KatYoneticisi Birak() der:
// lamba "takılı değil", ışık sönük, L bir şey yapmaz. Çivideki lamba alınınca Tak() çağrılır.
// Kayıttan devam eden ya da açılışsız başlayan oyunda lamba baştan takılıdır.
//
// NASIL ÇALIŞIR
//   - Kameranın çocuğu olan bir SPOT ışık: kamera nereye bakarsa huzme oraya. Oyuncu yalnızca
//     huzmedekini görür; korku, bakışa bağlanır (bir şey huzmenin kenarında durabilir).
//   - L ya da kolun sol omzu (L1) lambayı açıp kapatır; eski bir anahtar tıkı duyulur.
//     (#76, Şahan: ilk hâli F'ydi; "lamba" demek olan L'ye taşındı.)
//     Karanlıkta durup dinlemek ve saklanmak (#58 kaçış anları) için gerekli.
//   - Pil YOK (Şahan'ın kararı): sayaç eklenince korku yerine muhasebe olur. Lamba yalnızca
//     senaryolu anlarda titrer ya da bir anlığına söner; bunun için Titret() ve Sondur() var,
//     olay kodları (ve ileride kartlar) buradan çağırır.
//   - Gölge atar: tek bir hareketli spot, ucuz. Düşük kalitede gölge kapalı (Ayarlar.Kalite).
//
// Kat yöneticisi sahnedeki spotları "sahanlık lambası" sayıp cezada söndürüyor; bu lamba
// oyuncunun çocuğu olduğu için o listeden dışlanır (KatYoneticisi.Awake).

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KafaLambasi : MonoBehaviour
{
    // Işığın ayarları. Kurucu (OyuncuKurucu) ışığı bu değerlerle kurar; buradaki kopya
    // Inspector'dan ince ayar içindir.
    // Şahan'ın build testi (2026-09-26): "çok güçlü ama küçük bir alanı aydınlatıyor" →
    // şiddet 6'dan 3,5'e, dış koni 44°'den 64°'ye; iç koni 26°'de kaldı ki kenar geniş bir
    // geçişle yumuşasın.
    public const float Siddet = 3.5f;
    public const float Menzil = 10f;
    public const float DisAci = 64f, IcAci = 26f;   // dış koni ve tam şiddet konisi (yumuşak kenar)
    public static readonly Color Renk = new Color(1f, 0.90f, 0.72f);   // sıcak beyaz, ampullerden biraz beyaz

    private Light isik;
    private AudioSource kaynak;
    private AudioClip tik;
    private bool acik = true;
    private bool takili = true;   // lamba oyuncunun kafasında mı (açılışta çividen alınana kadar değil)
    private Coroutine titreme;

    /// <summary>Lamba oyuncunun kafasında mı? Açılışta çividen alınana kadar false (#56).</summary>
    public bool Takili { get { return takili; } }

    /// <summary>Lamba şu an yanıyor mu (senaryolu sönme sırasında da false).</summary>
    public bool Acik { get { return acik; } }

    private void Awake()
    {
        isik = GetComponentInChildren<Light>(true);
        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;             // kendi kafandaki lamba: 2B
        tik = Resources.Load<AudioClip>("Sesler/Lamba_Sonme");   // eski anahtar tıkı (#25'teki ses)
    }

    private void Start()
    {
        GolgeyiAyarla();
        Ayarlar.Degisti += GolgeyiAyarla;
    }

    private void OnDestroy()
    {
        Ayarlar.Degisti -= GolgeyiAyarla;
    }

    // Düşük kalitede gölge yok: hareketli bir spotun gölgesi her karede yeniden çizilir.
    private void GolgeyiAyarla()
    {
        if (isik != null) isik.shadows = Ayarlar.Kalite == 0 ? LightShadows.None : LightShadows.Soft;
    }

    private void Update()
    {
        // Menü açıkken ve başlık ekranında tuş okunmaz (kontrolcü zaten kapalı, bu bileşen değil).
        if (DuraklatmaMenusu.BaslikEkraninda || Time.timeScale == 0f) return;
        if (!takili) return;   // lamba henüz çivide: kafada yanacak bir şey yok

        bool l = Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame;
        bool omuz = Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame;
        if (l || omuz) Degistir();
    }

    /// <summary>
    /// Lamba oyuncuda değil (açılış, #56): ışık söner ve takılana kadar tuş da senaryolu
    /// titreme/sönme de bir şey yapmaz. KatYoneticisi açılışı başlatırken çağırır.
    /// </summary>
    public void Birak()
    {
        takili = false;
        acik = false;
        if (titreme != null) { StopCoroutine(titreme); titreme = null; }
        Uygula(false);
    }

    /// <summary>Çividen alınan lamba kafaya takılır ve yanar, anahtar tıkıyla (#56, CividekiLamba).</summary>
    public void Tak()
    {
        if (takili) return;
        takili = true;
        acik = true;
        Uygula(true);
        if (tik != null) kaynak.PlayOneShot(tik, 0.35f);
    }

    /// <summary>Lambayı açar ya da kapatır (oyuncunun tuşu).</summary>
    public void Degistir()
    {
        if (!takili) return;
        acik = !acik;
        if (titreme != null) { StopCoroutine(titreme); titreme = null; }
        Uygula(acik);
        if (tik != null) kaynak.PlayOneShot(tik, 0.35f);
    }

    /// <summary>
    /// Senaryolu titreme: 'sure' saniye boyunca düzensiz yanıp söner, sonra eski hâline döner.
    /// Oyuncunun kendi açma-kapaması bunu keser.
    /// </summary>
    public void Titret(float sure)
    {
        if (!acik) return;
        if (titreme != null) StopCoroutine(titreme);
        titreme = StartCoroutine(TitremeAkisi(sure));
    }

    /// <summary>Senaryolu sönme: 'sure' saniye kapanır, sonra kendiliğinden yanar (oyuncu açmaya çalışırsa yanmaz).</summary>
    public void Sondur(float sure)
    {
        if (!takili) return;   // kafada lamba yokken söndürülecek bir şey de yok
        if (titreme != null) StopCoroutine(titreme);
        titreme = StartCoroutine(SonmeAkisi(sure));
    }

    private IEnumerator TitremeAkisi(float sure)
    {
        float bitis = Time.time + sure;
        while (Time.time < bitis)
        {
            Uygula(false);
            yield return new WaitForSeconds(Random.Range(0.03f, 0.09f));
            Uygula(true);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
        }
        Uygula(acik);
        titreme = null;
    }

    private IEnumerator SonmeAkisi(float sure)
    {
        bool once = acik;
        acik = false;
        Uygula(false);
        if (tik != null) kaynak.PlayOneShot(tik, 0.3f);
        yield return new WaitForSeconds(sure);
        acik = once;
        Uygula(acik);
        if (acik && tik != null) kaynak.PlayOneShot(tik, 0.3f);
        titreme = null;
    }

    private void Uygula(bool yansin)
    {
        if (isik != null) isik.enabled = yansin;
    }
}
