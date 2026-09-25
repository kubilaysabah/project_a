// Etkilesimci.cs
// Oyuncunun baktığı şeyle etkileşim kurmasını sağlayan tek merkez.
//
// Üç iş yapar:
//   1. Her karede oyuncunun neye baktığını bulur.
//   2. Baktığı şey etkileşime açıksa üstünde "TOPLA (E)" gibi bir ipucu gösterir.
//   3. Tuşa basılınca o nesnenin Etkilesim fonksiyonunu çağırır.
//
// Hangi nesneyle ne yapıldığını BİLMEZ. Poşeti de, ileride gelecek zili de,
// notu da aynı şekilde ele alır; hepsi IEtkilesimli sözleşmesini imzalıyor.
// Yeni bir etkileşimli nesne eklemek bu dosyayı değiştirmeyi gerektirmez.

using UnityEngine;
using UnityEngine.InputSystem;

public class Etkilesimci : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Kaç metreden etkileşime girilebilir.")]
    [SerializeField] private float menzil = 2.2f;

    [Tooltip("Nişan alınan kamera. Boş bırakılırsa çocuklardan bulunur.")]
    [SerializeField] private Transform kamera;

    [Header("İpucu yazısı")]
    [Tooltip("Yazının nesnenin üstünde ne kadar yükseklikte duracağı (metre).")]
    [SerializeField] private float ipucuYuksekligi = 0.18f;

    [Tooltip("Harf boyu. Yazı dünya içinde durduğu için metre cinsindendir.")]
    [SerializeField] private float ipucuBoyu = 0.012f;

    // Panelin koyu malzemesi (#26). OyuncuKurucu bağlar: Assets/Materials/Ipucu_Panel.mat.
    // Neden dosya: build, yalnızca sahnede bir yere BAĞLANMIŞ dosyaları pakete koyar.
    // Shader'ı oyun sırasında adıyla aramak (Shader.Find) ancak o shader başka bir
    // yüzden pakete girmişse çalışır; girmemişse panel pembe ya da görünmez çıkar.
    [Tooltip("İpucu panelinin malzemesi. Boşsa shader adıyla aranır (build'de riskli).")]
    [SerializeField] private Material panelMalzemesi;

    // Şu anda nişan alınan nesne. Başka scriptler (test göstergesi) okuyabilir.
    public IEtkilesimli Hedef { get; private set; }

    private InputAction etkilesimEylemi;

    // İpucu yazısı TEK bir nesnedir ve hedeften hedefe taşınır.
    // Her etkileşimli nesneye ayrı yazı koymak yerine böyle yapmak,
    // hem sahneyi temiz tutar hem de yazının görünümünü tek yerden değiştirtir.
    private GameObject ipucuNesnesi;
    private GameObject ipucuPaneli;
    private TextMesh ipucuYazisi;

    // ---------- KURULUM ----------

    private void Awake()
    {
        if (kamera == null && transform.childCount > 0) kamera = transform.GetChild(0);

        var varlik = InputSystem.actions;

        if (varlik == null)
        {
            Debug.LogError("[ETKİLEŞİM] Proje geneli giriş varlığı bulunamadı.");
        }
        else
        {
            etkilesimEylemi = varlik.FindAction("Player/Interact");
        }

        IpucunuKur();
    }

    private void OnEnable() { etkilesimEylemi?.Enable(); }
    private void OnDisable() { etkilesimEylemi?.Disable(); }

    // İpucu yazısını kodla üretiyoruz, sahneye elle koymuyoruz.
    // Sebebi: bu bir sahne nesnesi değil, oyuncunun kendi aleti. Sahneye
    // konsaydı graybox her yeniden kurulduğunda kaybolur ya da çoğalırdı.
    private void IpucunuKur()
    {
        ipucuNesnesi = new GameObject("EtkilesimIpucu");

        // ---- ARKA PANEL ----
        // Yazı tek başına yetmiyor: açık duvarda koyu yazı, karanlıkta açık yazı
        // kayboluyor. Oyun karardıkça bu daha da kötüleşecek. Koyu bir panelin
        // üstüne açık yazı, arka plan ne olursa olsun okunur. Oyunların
        // neredeyse hepsinin bu işi böyle yapmasının sebebi bu.
        ipucuPaneli = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ipucuPaneli.name = "Panel";
        ipucuPaneli.transform.SetParent(ipucuNesnesi.transform, false);

        // Panelin çarpışma kutusu MUTLAKA silinmeli. Kalırsa oyuncunun
        // nişan ışını kendi ipucuna çarpar ve hedefi kaybeder.
        Destroy(ipucuPaneli.GetComponent<Collider>());

        Material panelMalzeme = panelMalzemesi;
        if (panelMalzeme == null)
        {
            // Yedek yol: bağlanmamışsa eskisi gibi adıyla ara ve uyar.
            Debug.LogWarning("[İPUCU] Panel malzemesi bağlanmamış; Huzur → Oyuncuyu Yeniden Kur.");
            panelMalzeme = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            panelMalzeme.SetColor("_BaseColor", new Color(0.04f, 0.04f, 0.05f));
        }
        ipucuPaneli.GetComponent<MeshRenderer>().sharedMaterial = panelMalzeme;

        // ---- YAZI ----
        GameObject yaziNesnesi = new GameObject("Yazi");
        yaziNesnesi.transform.SetParent(ipucuNesnesi.transform, false);

        ipucuYazisi = yaziNesnesi.AddComponent<TextMesh>();
        ipucuYazisi.text = "";
        ipucuYazisi.anchor = TextAnchor.MiddleCenter;
        ipucuYazisi.alignment = TextAlignment.Center;
        ipucuYazisi.fontSize = 96;                 // büyük üret, küçült: kenarlar keskin olur
        ipucuYazisi.characterSize = ipucuBoyu;
        ipucuYazisi.fontStyle = FontStyle.Bold;
        ipucuYazisi.color = new Color(0.95f, 0.94f, 0.90f);
        ipucuYazisi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        yaziNesnesi.GetComponent<MeshRenderer>().sharedMaterial = ipucuYazisi.font.material;

        // Yazı panelin ÖNÜNDE dursun, yoksa panelin içinde kalıp görünmez olur.
        yaziNesnesi.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        ipucuNesnesi.SetActive(false);
    }

    // Paneli yazının boyutuna göre büyütür. Yazı uzunluğu eyleme göre
    // değişiyor ("TOPLA" ile "ÇAL" aynı genişlikte değil), o yüzden
    // sabit bir boyut veremiyoruz.
    private void PaneliYazidanOlcekle()
    {
        Renderer yaziCizici = ipucuYazisi.GetComponent<Renderer>();
        if (yaziCizici == null) return;

        // localBounds kullanıyoruz, bounds DEĞİL.
        //
        // "bounds" nesnenin DÜNYA eksenlerine hizalı kutusudur. İpucu her karede
        // kameraya döndüğü için bu kutu da dönüyor: yazının 0,58 metrelik genişliği
        // dünya X'inde değil Z'sinde çıkıyordu ve panel minicik kalıyordu.
        // "localBounds" nesnenin kendi eksenlerindeki ölçüsüdür, dönüşten etkilenmez.
        Vector3 boyut = yaziCizici.localBounds.size;

        ipucuPaneli.transform.localScale =
            new Vector3(boyut.x + 0.06f, boyut.y + 0.04f, 1f);
    }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (kamera == null) return;

        Hedef = HedefBul(out Transform hedefGovdesi);

        IpucunuGuncelle(hedefGovdesi);

        if (etkilesimEylemi == null) return;
        if (!etkilesimEylemi.WasPressedThisFrame()) return;

        if (Hedef != null && Hedef.EtkilesimeAcik)
        {
            Hedef.Etkilesim(gameObject);

            // Etkileşimden sonra hedef genelde kaybolur; ipucunu hemen gizliyoruz
            // ki bir kare boyunca havada asılı kalmasın.
            Hedef = null;
            ipucuNesnesi.SetActive(false);
        }
    }

    // ---------- NİŞAN ----------
    // NEDEN BASİT BİR RAYCAST YETMİYOR
    // Kamera, oyuncunun çarpışma kapsülünün İÇİNDE duruyor (göz 1,65 m,
    // kapsül 1,75 m). Basit bir ışın daha ilk 11 santimde oyuncunun kendi
    // gövdesine çarpıyor ve hedefe hiç ulaşamıyor.
    //
    // Bu yüzden yoldaki BÜTÜN çarpışmaları alıp oyuncunun kendi parçalarını
    // eliyoruz, sonra geri kalanların EN YAKININA bakıyoruz. En yakına bakmak
    // şart: araya duvar giriyorsa etkileşim olmamalı.
    private IEtkilesimli HedefBul(out Transform govde)
    {
        govde = null;

        Ray isin = new Ray(kamera.position, kamera.forward);
        RaycastHit[] carpmalar = Physics.RaycastAll(isin, menzil);

        float enYakinMesafe = float.MaxValue;
        Collider enYakin = null;

        foreach (RaycastHit carpma in carpmalar)
        {
            // IsChildOf: bu nesne oyuncunun kendisi ya da onun çocuğu mu?
            if (carpma.collider.transform.IsChildOf(transform)) continue;

            if (carpma.distance < enYakinMesafe)
            {
                enYakinMesafe = carpma.distance;
                enYakin = carpma.collider;
            }
        }

        if (enYakin == null) return null;

        // GetComponentInParent: nesne birkaç parçadan oluşuyorsa üst parçada arar.
        IEtkilesimli bulunan = enYakin.GetComponentInParent<IEtkilesimli>();

        if (bulunan == null || !bulunan.EtkilesimeAcik) return null;

        // Sözleşmeyi imzalayan her nesne aslında bir MonoBehaviour; gövdesini
        // ipucunu yerleştirmek için alıyoruz.
        MonoBehaviour bilesen = bulunan as MonoBehaviour;
        if (bilesen != null) govde = bilesen.transform;

        return bulunan;
    }

    // ---------- İPUCU ----------

    private void IpucunuGuncelle(Transform hedefGovdesi)
    {
        if (Hedef == null || hedefGovdesi == null)
        {
            if (ipucuNesnesi.activeSelf) ipucuNesnesi.SetActive(false);
            return;
        }

        // Biçim tablodan gelir (#15): "{0} ({1})". string.Format, {0} yazan yere
        // eylemin adını, {1} yazan yere tuşun adını koyar: "TOPLA (E)". Tuş işaretinin
        // yeri de böylece dile göre değişebilir (bazı dillerde tuş önce yazılır).
        // Tuş, oyuncunun en son kullandığı cihaza göre (#29): klavyede E, oyun kolunda A.
        string tus = GirisCihazi.Kol ? Dil.Getir("tus.etkilesim_kol") : Dil.Getir("tus.etkilesim_klavye");
        ipucuYazisi.text = string.Format(Dil.Getir("ipucu.bicim"), Hedef.Ipucu, tus);

        // Nesnenin ÜSTÜNE yerleştir. Görsel sınırlarını kullanıyoruz ki
        // büyük bir nesnede yazı içinde kalmasın, küçüğünde de çok yükseğe kaçmasın.
        Vector3 tepe = hedefGovdesi.position;

        Renderer cizici = hedefGovdesi.GetComponentInChildren<Renderer>();
        if (cizici != null) tepe.y = cizici.bounds.max.y;

        ipucuNesnesi.transform.position = tepe + Vector3.up * ipucuYuksekligi;

        // Yazı her zaman kameraya baksın. LookAt kameraya DOĞRU çevirir;
        // yazının okunabilmesi için ters çevirmemiz gerekiyor, bu yüzden
        // kameranın arkasındaki bir noktaya bakıyoruz.
        ipucuNesnesi.transform.rotation = Quaternion.LookRotation(
            ipucuNesnesi.transform.position - kamera.position);

        if (!ipucuNesnesi.activeSelf) ipucuNesnesi.SetActive(true);

        // Panel yazıya göre boyutlanır. Yazı değişmiş olabileceği için her karede.
        PaneliYazidanOlcekle();
    }

    private void OnDestroy()
    {
        if (ipucuNesnesi != null) Destroy(ipucuNesnesi);
    }
}
