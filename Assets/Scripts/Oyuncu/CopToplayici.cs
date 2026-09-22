// CopToplayici.cs
// Kapıcının işi: kapı önündeki çöp poşetlerini almak.
//
// ---------- BU GÖREV NEDEN VAR ----------
// Poşet toplamak bir "meşguliyet" değil, bir TASARIM BAHANESİ.
// Oyuncuyu sahanlıkta oyalar ve kapıya YAKLAŞTIRIR. Anomalilerin çoğu
// kapının üstünde ve önünde: numara, zil, gözetleme deliği, terlikler, paspas.
// Oyuncu poşeti almak için oraya gittiğinde onları zaten görmüş olur.
// Yani görev, anomaliyi fark etme süresini zorlamadan uzatır.
//
// Etkileşim mantığı bu scriptte, yürüyüş kontrolcüsünde değil. Sebebi iş bölümü:
// kontrolcü yürümeyi ve bakmayı bilir, toplayıcı da toplamayı. İleride başka
// etkileşimler eklenirse (kapı zilini çalmak gibi) onlar da kendi scriptine gider.

using UnityEngine;
using UnityEngine.InputSystem;

public class CopToplayici : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Poşete kaç metreden uzanılabilir.")]
    [SerializeField] private float menzil = 2.2f;

    [Tooltip("Nişan alınan kamera. Boş bırakılırsa çocuklardan bulunur.")]
    [SerializeField] private Transform kamera;

    [Header("Durum")]
    [Tooltip("Şu ana kadar toplanan poşet sayısı.")]
    [SerializeField] private int tasinanPoset;

    /// <summary>Oyuncunun taşıdığı poşet sayısı.</summary>
    public int TasinanPoset { get { return tasinanPoset; } }

    /// <summary>Oyuncunun o anda baktığı poşet; yoksa null. Test göstergesi kullanıyor.</summary>
    public CopPoseti BakilanPoset { get; private set; }

    private InputAction etkilesimEylemi;

    // ---------- KURULUM ----------

    private void Awake()
    {
        if (kamera == null && transform.childCount > 0) kamera = transform.GetChild(0);

        var varlik = InputSystem.actions;

        if (varlik == null)
        {
            Debug.LogError("[ÇÖP] Proje geneli giriş varlığı bulunamadı.");
            return;
        }

        etkilesimEylemi = varlik.FindAction("Player/Interact");
    }

    private void OnEnable() { etkilesimEylemi?.Enable(); }
    private void OnDisable() { etkilesimEylemi?.Disable(); }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (kamera == null) return;

        // Her karede neye baktığımızı buluyoruz. Bunu sadece tuşa basınca
        // yapsaydık oyuncu poşete bakıp bakmadığını anlayamazdı; ileride
        // ekranda "E ile al" ipucu göstereceksek bu bilgi gerekli.
        BakilanPoset = HedeftekiPoset();

        if (etkilesimEylemi == null) return;
        if (!etkilesimEylemi.WasPressedThisFrame()) return;

        if (BakilanPoset != null)
        {
            BakilanPoset.Al();
            tasinanPoset++;

            Debug.Log($"[ÇÖP] Poşet alındı. Taşınan: {tasinanPoset}");

            BakilanPoset = null;
        }
    }

    // ---------- NİŞAN ----------
    // Kameradan ileri doğru görünmez bir ışın atar ve çarptığı şeyde
    // CopPoseti bileşeni var mı diye bakar.
    //
    // NEDEN BASİT BİR RAYCAST YETMİYOR
    // Kamera, oyuncunun çarpışma kapsülünün İÇİNDE duruyor (göz hizası 1,65 m,
    // kapsül 1,75 m). Basit bir ışın daha ilk 11 santimde oyuncunun kendi
    // gövdesine çarpıyor ve hiçbir zaman poşete ulaşamıyordu.
    //
    // Çözüm: yoldaki BÜTÜN çarpışmaları alıp oyuncunun kendi parçalarını
    // eliyoruz, sonra geri kalanların EN YAKININA bakıyoruz. En yakına bakmak
    // şart, çünkü araya duvar giriyorsa poşet alınamamalı.
    private CopPoseti HedeftekiPoset()
    {
        Ray isin = new Ray(kamera.position, kamera.forward);

        RaycastHit[] carpmalar = Physics.RaycastAll(isin, menzil);

        float enYakinMesafe = float.MaxValue;
        Collider enYakin = null;

        foreach (RaycastHit carpma in carpmalar)
        {
            // IsChildOf: bu nesne oyuncunun kendisi ya da onun çocuğu mu?
            // Öyleyse kendi gövdemizdir, yok sayıyoruz.
            if (carpma.collider.transform.IsChildOf(transform)) continue;

            if (carpma.distance < enYakinMesafe)
            {
                enYakinMesafe = carpma.distance;
                enYakin = carpma.collider;
            }
        }

        if (enYakin == null) return null;

        // GetComponentInParent: çarpılan parçanın kendisinde yoksa
        // ebeveynlerinde de arar. Poşet ileride birkaç parçadan oluşursa
        // kod değişmeden çalışmaya devam eder.
        return enYakin.GetComponentInParent<CopPoseti>();
    }

    // ---------- SIFIRLAMA ----------

    /// <summary>Yanlış karar sonrası 1. kata dönülünce çağrılır.</summary>
    public void Sifirla()
    {
        tasinanPoset = 0;
    }
}
