// Simsek.cs
// Pencerelerden çakan şimşek ve ardından gelen gök gürültüsü (#95).
//
// Her 45-120 saniyede bir dışarıda şimşek çakar: pencerelerin ardındaki gece bir an bembeyaz olur,
// odaya pencereden keskin, soğuk bir ışık düşer (iki üç kısa çakma), sonra 1-3 saniye içinde gök
// gürler. Gecikme ne kadar uzunsa yıldırım o kadar uzaktır: gürültü daha kısık ve daha boğuk gelir
// (gerçekte de ses ışıktan çok yavaştır, saniyede ~340 m; havada tizini de kaybeder).
//
// ---------- IŞIK: NEDEN YENİ BİR IŞIK DEĞİL DE AY? ----------
// Şimşeğin ışığı pencereden girmeli, DUVARDAN GEÇMEMELİ. Bunun için ışığın gölge atması gerekir:
// gölgesi olmayan bir ışık duvarın arkasını da aydınlatır (ışık hesabı "arada duvar var mı" diye
// bakmaz; buna bakan şey gölge haritasıdır). URP'de yönlü (directional) bir ışığın gölge atabilmesi
// için sahnenin ANA ışığı olması gerekir; ikinci bir yönlü ışık "ek ışık" sayılır ve URP ek yönlü
// ışıklara gölge vermez. Sahnenin ana ışığı zaten ay (Ay_Isigi, AtmosferKurucu): yumuşak gölgeli,
// 0,03 şiddetinde soluk mavi. Gölge haritası onun için her karede zaten çiziliyor; yani şimşek için
// ayı birkaç kesir saniye güçlendirmek HİÇ ek maliyet getirmez. Çakma boyunca ayı:
//   - oyuncunun bulunduğu dairenin pencerelerinin dışından içeri, 35° aşağı bakacak yöne çeviririz
//     (sahanlık ve merdivende: ara sahanlığın penceresinden, +z'den -z'ye),
//   - şiddetini ve rengini çakmanın parlaklığıyla yükseltiriz,
//   - bitince eski yönüne, şiddetine ve rengine döndürürüz.
//
// ---------- PENCEREYİ IŞIK GEÇİRİR HÂLE GETİRMEK ----------
// İki şey ışığın önünü kesiyordu: dairelerin camı (Pencere_Cam, opak bir kutu) ve merdiven
// penceresinin 1 m ötesindeki gece yüzeyi (Disari). İkisi de gölge atar; ay pencereden giremezdi.
// Çakma boyunca bunların gölge atmasını kapatırız (shadowCastingMode = Off) ve malzemelerini
// parlak, ışıktan etkilenmeyen (Unlit) bir kopyayla değiştiririz: oyuncu camda gökyüzünün
// aydınlandığını görür. Çakma bitince ikisi de eski hâline döner.
//
// Malzemenin kopyası ÇALIŞMA ANINDA üretilir (new Material): kaynağı kurucunun bağladığı
// Ampul_Cam.mat (URP Unlit). Gölgelendiriciyi adıyla aratmıyoruz (Shader.Find build'de yalnızca
// başka bir şey tesadüfen onu pakete koyduysa çalışır, #26 kuralı); bağlı bir malzemenin kopyası
// her zaman çalışır.
//
// ---------- GÖLGE (Varlık 2. kademe) ----------
// Bina kızgınken (Varlik.Kademe >= 2) şimşek bazen bir şey gösterir: oyuncunun odasında, 3-6 m
// ötede, kafa lambasının huzmesinin kenarında, bir köşede Gölge durur. YALNIZCA ışık yanarken
// görünür; çakma bitince yok olur. En fazla üç çakmada bir (her şimşekte olsa alışılır).
//
// Ne zaman çakmaz? Başlık ekranında, oyun duraklatılmışken (Time.timeScale = 0), açılışta (zemin
// kat), çatı kapısı açılınca (final kendi ışığını kurar) ve testlerde (Varlik.Kapali).
//
// Bileşen KatYoneticisi'nin nesnesinde durur (GezenGolge ve Varlik gibi); SimsekKurucu ekler ve bağlar.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simsek : MonoBehaviour
{
    [Header("Bağlantılar (SimsekKurucu bağlar)")]
    [SerializeField] private GameObject sablon;            // Kacis_Golgesi şablonu (GezenGolge ile aynı)
    [SerializeField] private AudioClip[] gokGurultuleri;   // Gok_Gurultusu_1, _2 (hava_uret.py)
    [SerializeField] private Material parlamaKaynagi;      // Ampul_Cam.mat (URP Unlit): parlayan gökyüzünün kaynağı
    [SerializeField] private Light ay;                     // Ay_Isigi: sahnenin ana (gölgeli) yönlü ışığı

    [Header("Ayarlar")]
    [Tooltip("Rastgelelik tohumu. 0 = her oyunda farklı; bir sayı verilirse şimşekler aynı sırayla gelir.")]
    [SerializeField] private int tohum = 0;

    // ---------- SABİTLER ----------
    private const float AralikEnAz = 45f, AralikEnCok = 120f;   // iki şimşek arası (saniye)
    // Çakma anında ayın şiddeti. Post-processing (ACES + kontrast) koyuları ezdiği için düşük bir değer
    // okunmaz; 3,5 ile pencere önündeki zemin bir an neredeyse beyaz görünür. Az gelirse burayı artırın.
    private const float SimsekSiddeti = 3.5f;
    private static readonly Color SimsekRengi = new Color(0.82f, 0.88f, 1.0f);   // soğuk, mavimsi beyaz
    // Pencerenin ardındaki gökyüzünün çakma anındaki rengi (HDR: 1'in üstü bloom'u tetikler).
    private static readonly Color GokRengi = new Color(2.2f, 2.4f, 3.0f);
    // Çakma yokken o yüzeylerin Unlit karşılığı: eski koyu lacivert gece (Gece.mat'ın rengi).
    private static readonly Color GeceRengi = new Color(0.015f, 0.02f, 0.05f);
    private const float Egim = 35f;                              // ışığın yere iniş açısı (derece)
    private const float GolgeEnAz = 3f, GolgeEnCok = 6f;         // Gölge'nin oyuncuya uzaklığı (metre)
    private const int GolgeAraligi = 3;                          // Gölge en fazla bu kadar çakmada bir

    // ---------- DURUM ----------
    private System.Random rastgele;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private Transform oyuncu;
    private AudioSource gokSesi;                 // gök gürültüsünü çalan 2B kaynak
    private AudioLowPassFilter bogucu;           // uzaklık arttıkça tizleri kısar
    private Material parlama;                    // parlamaKaynagi'nin çalışma anındaki kopyası
    private float sayac;                         // sıradaki şimşeğe kalan süre
    private bool suruyor;                        // şu an bir çakma sürüyor mu
    private int golgesizCakma = GolgeAraligi;    // son Gölge'den beri kaç çakma oldu (ilk çakmada izin var)

    // Pencere yüzeyleri (camlar ve merdiven penceresinin ardındaki gece) ve özgün hâlleri.
    private readonly List<MeshRenderer> yuzeyler = new List<MeshRenderer>();
    private readonly List<Material[]> ozgunMalzemeler = new List<Material[]>();
    private readonly List<UnityEngine.Rendering.ShadowCastingMode> ozgunGolgeler = new List<UnityEngine.Rendering.ShadowCastingMode>();

    // Ayın çakmadan önceki hâli (geri yüklemek için).
    private float ayOzgunSiddet, ayOzgunGolgeGucu;
    private Color ayOzgunRenk;
    private Quaternion ayOzgunDonus;
    private bool ayDegisti;

    private GameObject golge;                    // çakma sırasında görünen Gölge (yoksa null)

    /// <summary>Şu an bir şimşek çakıyor mu (test için)?</summary>
    public bool Suruyor { get { return suruyor; } }

    private void Start()
    {
        // System.Random: tohumla kurulabilen rastgele sayı üreticisi. UnityEngine.Random herkesin
        // ortak kullandığı tek bir üreticidir; ona tohum vermek başka sistemlerin sırasını da bozardı.
        rastgele = new System.Random(tohum != 0 ? tohum : System.Environment.TickCount);
        yonetici = GetComponent<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        oyuncu = kontrol != null ? kontrol.transform : null;

        // Ay bağlanmamışsa (kurucu çalışmadan önceki bir sahne) sahnedeki yönlü ışığı kullan.
        // Bu bir SAHNE nesnesi; build'de de sahnede durduğu için aramak güvenli (dosya değil).
        if (ay == null)
            foreach (Light l in FindObjectsByType<Light>())
                if (l.type == LightType.Directional) { ay = l; break; }

        // Parlayan gökyüzü malzemesi: bağlı Unlit malzemenin kopyası.
        if (parlamaKaynagi != null)
        {
            parlama = new Material(parlamaKaynagi);
            parlama.name = "Simsek_Parlama (çalışma anı)";
            parlama.SetColor("_BaseColor", GeceRengi);
        }

        YuzeyleriBul();

        // Gök gürültüsünün kaynağı: 2B (kafanın içinden; yıldırım binanın dışında, her yerden duyulur).
        GameObject g = new GameObject("Gok_Gurultusu");
        g.transform.SetParent(transform, false);
        gokSesi = g.AddComponent<AudioSource>();
        gokSesi.playOnAwake = false;
        gokSesi.spatialBlend = 0f;
        // Yankı sese zaten işlendi (hava_uret.py, çok büyük bir alan); odanın yankısı (OdaYankisi, #87)
        // üstüne bir de kendi yankısını eklemesin. Müzik de böyle (ArkaMuzik).
        gokSesi.bypassListenerEffects = true;
        // AudioLowPassFilter: kaynağın sesini belirli bir frekansın üstünde kısar (boğar).
        bogucu = g.AddComponent<AudioLowPassFilter>();
        bogucu.cutoffFrequency = 5000f;

        sayac = Aralik(25f, 60f);   // ilk şimşek biraz daha erken: oyuncu oyunun bu parçasını erken tanısın
    }

    // Daire camlarını ve merdiven penceresinin ardındaki gece yüzeylerini bulur (kapalı daireler dahil:
    // daireler katlara göre açılıp kapanıyor). Bunlar sahne nesneleri; adıyla aramak build'de de çalışır.
    private void YuzeyleriBul()
    {
        foreach (MeshRenderer r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            if (r.name != "Pencere_Cam" && r.name != "Disari") continue;
            yuzeyler.Add(r);
            ozgunMalzemeler.Add(r.sharedMaterials);
            ozgunGolgeler.Add(r.shadowCastingMode);
        }
    }

    private float Aralik(float enAz, float enCok) { return enAz + (float)rastgele.NextDouble() * (enCok - enAz); }

    // Şimşek şu an çakabilir mi?
    private bool IzinVar()
    {
        if (Varlik.Kapali) return false;                              // test (bot, kat denetimi)
        if (DuraklatmaMenusu.BaslikEkraninda) return false;            // başlık ekranı
        if (Time.timeScale <= 0f) return false;                       // duraklatma menüsü
        if (yonetici == null || !yonetici.enabled) return false;
        if (yonetici.AcilisModunda || yonetici.TurBitti) return false; // açılış (zemin kat) ve final
        if (yonetici.MevcutKat < 1) return false;
        if (ay == null || oyuncu == null) return false;
        return true;
    }

    private void Update()
    {
        if (suruyor || rastgele == null) return;
        // Sayaç yalnızca şimşeğe izin varken işler: başlık ekranında ya da açılışta geçen süre sayılmaz.
        if (!IzinVar()) return;
        sayac -= Time.deltaTime;
        if (sayac > 0f) return;
        sayac = Aralik(AralikEnAz, AralikEnCok);
        StartCoroutine(Cakma());
    }

    /// <summary>Şimdi bir şimşek çaktırır (test ve deneme için). Çakamazsa false.</summary>
    public bool Cak()
    {
        if (suruyor || !IzinVar()) return false;
        StartCoroutine(Cakma());
        return true;
    }

    // ---------- ÇAKMA ----------
    // Desen: kısa bir çakma (0,06 sn), kısa karanlık (0,08 sn), sonra uzun ve sönerek biten asıl
    // çakma (0,18 sn); bazen sonunda soluk bir üçüncü titreme. Yıldırım gerçekte de böyle titrer:
    // aynı kanaldan art arda birkaç boşalma geçer.
    private IEnumerator Cakma()
    {
        suruyor = true;
        // Desen: (süre, başta parlaklık, sonda parlaklık). Parlaklık 0 = karanlık.
        var desen = new List<Vector3>
        {
            new Vector3(0.06f, Aralik(0.7f, 1f), Aralik(0.6f, 0.9f)),
            new Vector3(Aralik(0.06f, 0.1f), 0f, 0f),
            new Vector3(Aralik(0.15f, 0.22f), 1f, 0f),
        };
        if (rastgele.NextDouble() < 0.5)
        {
            desen.Add(new Vector3(Aralik(0.08f, 0.14f), 0f, 0f));
            desen.Add(new Vector3(0.05f, Aralik(0.3f, 0.5f), 0.1f));
        }

        AyiHazirla();
        yansimaVar = PencereliYerde();
        bool golgeli = GolgeGerekirMi() && GolgeKoy();
        if (golgeli) golgesizCakma = 0; else golgesizCakma++;

        // Gök gürültüsünün gecikmesi şimdi seçilir; çakma sürerken de geri sayar.
        float gecikme = Aralik(1f, 3f);
        float gecen = 0f;

        foreach (Vector3 parca in desen)
        {
            for (float t = 0f; t < parca.x; t += Time.deltaTime)
            {
                // Final çakmanın ortasında başladıysa hemen bırak: final ayı kendisi yönetir.
                if (yonetici.TurBitti) { Bitir(); yield break; }
                float p = Mathf.Lerp(parca.y, parca.z, t / parca.x);
                Uygula(p);
                gecen += Time.deltaTime;
                yield return null;
            }
        }
        Bitir();

        // Gök gürültüsü: kalan gecikme kadar sonra. (WaitForSeconds oyun zamanıyla sayar; menü açılınca durur.)
        if (gecikme > gecen) yield return new WaitForSeconds(gecikme - gecen);
        if (!yonetici.TurBitti) Gurle(gecikme);
    }

    // Ayın özgün hâlini saklar ve yönünü oyuncunun bulunduğu yerin pencerelerine çevirir.
    private void AyiHazirla()
    {
        ayOzgunSiddet = ay.intensity;
        ayOzgunRenk = ay.color;
        ayOzgunDonus = ay.transform.rotation;
        ayOzgunGolgeGucu = ay.shadowStrength;
        ayDegisti = true;

        // Işığın yatayda gideceği yön (dışarıdan içeri).
        Vector3 yatay = Vector3.back;   // sahanlık/merdiven: ara sahanlığın penceresi +z'de, ışık -z'ye gider
        DaireEksenleri eksen = OyuncununDairesi(out _);
        if (eksen != null)
        {
            // Dairenin pencereleri v = 8,2 duvarında (salon, odalar) ve u = 10,6 duvarında (mutfak).
            // Işık ikisinin de dışından gelsin diye çapraz: -v yönünde, biraz da -u yönünde gider.
            // Yon(du, dv) dairenin yerel uzayında bir yön; TransformDirection onu dünyaya çevirir
            // (daireler aynalı; hesap her dairede doğru tarafı verir).
            yatay = eksen.transform.TransformDirection(eksen.Yon(-0.45f, -1f));
            yatay.y = 0f;
            yatay.Normalize();
        }
        // Yatay yönü 35° aşağı eğ: cos ile yatay pay, sin ile aşağı pay.
        float e = Egim * Mathf.Deg2Rad;
        Vector3 yon = yatay * Mathf.Cos(e) + Vector3.down * Mathf.Sin(e);
        // LookRotation: ışığın ileri ekseni (+z) bu yöne baksın. Yönlü ışık ileri ekseni boyunca parlar.
        ay.transform.rotation = Quaternion.LookRotation(yon);
        ay.shadowStrength = 1f;   // çakmada gölgeler tam koyu: pencere çerçevesinin izi yere keskin düşer

        // Pencere yüzeyleri: gölge atmasın (ışık geçsin), parlak malzemeye geçsin.
        if (parlama != null)
            for (int i = 0; i < yuzeyler.Count; i++)
            {
                if (yuzeyler[i] == null) continue;
                Material[] dizi = new Material[ozgunMalzemeler[i].Length];
                for (int k = 0; k < dizi.Length; k++) dizi[k] = parlama;
                yuzeyler[i].sharedMaterials = dizi;
                yuzeyler[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
    }

    // p = 0 (karanlık) ... 1 (tam çakma): ayı, gökyüzünü ve Gölge'yi buna göre ayarlar.
    // ---------- ODAYA YANSIYAN IŞIK ----------
    // İlk denemede şimşek fiziksel olarak doğru düştü ama yalnızca pencerenin önündeki zemine bir leke
    // bıraktı; pencereye sırtı dönük oyuncu simsiyah bir oda gördü. Gerçekte o leke duvarlardan, tavandan
    // seker ve bütün odayı bir anlığına aydınlatır. URP'de sekme hesabı yok; onun yerine flaş süresince
    // ORTAM IŞIĞI (RenderSettings.ambientLight: her yüzeye her yönden gelen düz ışık) yükselir. Yalnızca
    // oyuncunun bulunduğu yerde pencere varsa: penceresiz banyoda şimşek görülmez (ışık sızmasın).
    // Soğuk mavi-beyaz, tepe değer. Gece görünümü (ACES + kontrast) koyuları ezdiği için 0,3 ve 0,6 oda
    // seçilmedi; 1,5 civarında flaşın tepesinde oda bütünüyle okunur (oyun içi görüntüyle ölçüldü). Flaş
    // 0,3 sn sürer ve sönerek biter; göz yormaz.
    private static readonly Color YansimaRengi = new Color(1.40f, 1.60f, 2.10f);
    private Color ortamOzgun;
    private bool ortamDegisti;
    private bool yansimaVar;

    private void Uygula(float p)
    {
        if (yansimaVar)
        {
            if (!ortamDegisti) { ortamOzgun = RenderSettings.ambientLight; ortamDegisti = true; }
            RenderSettings.ambientLight = Color.Lerp(ortamOzgun, YansimaRengi, p);
        }
        ay.intensity = Mathf.Lerp(ayOzgunSiddet, SimsekSiddeti, p);
        ay.color = Color.Lerp(ayOzgunRenk, SimsekRengi, p);
        if (parlama != null) parlama.SetColor("_BaseColor", Color.Lerp(GeceRengi, GokRengi, p));
        // Gölge yalnızca ışık yeterince güçlüyken var; karanlık aralarda yok olur.
        if (golge != null) golge.SetActive(p > 0.15f);
    }

    // Her şeyi çakmadan önceki hâline döndürür. OnDisable'dan da çağrılır (yarıda kalan çakma).
    private void Bitir()
    {
        if (ortamDegisti) { RenderSettings.ambientLight = ortamOzgun; ortamDegisti = false; }
        if (ayDegisti && ay != null)
        {
            ay.intensity = ayOzgunSiddet;
            ay.color = ayOzgunRenk;
            ay.transform.rotation = ayOzgunDonus;
            ay.shadowStrength = ayOzgunGolgeGucu;
        }
        ayDegisti = false;
        for (int i = 0; i < yuzeyler.Count; i++)
        {
            if (yuzeyler[i] == null) continue;
            yuzeyler[i].sharedMaterials = ozgunMalzemeler[i];
            yuzeyler[i].shadowCastingMode = ozgunGolgeler[i];
        }
        if (parlama != null) parlama.SetColor("_BaseColor", GeceRengi);
        if (golge != null) Destroy(golge);
        golge = null;
        suruyor = false;
    }

    // ---------- GÖK GÜRÜLTÜSÜ ----------
    // gecikme 1 sn (yakın) ... 3 sn (uzak). Uzak gök gürültüsü kısık ve boğuk gelir.
    private void Gurle(float gecikme)
    {
        if (gokGurultuleri == null || gokGurultuleri.Length == 0 || gokSesi == null) return;
        AudioClip klip = gokGurultuleri[rastgele.Next(gokGurultuleri.Length)];
        if (klip == null) return;
        float uzaklik = Mathf.InverseLerp(1f, 3f, gecikme);            // 0 yakın, 1 uzak
        gokSesi.clip = klip;
        gokSesi.volume = Mathf.Lerp(0.6f, 0.3f, uzaklik) * Aralik(0.85f, 1f);
        gokSesi.pitch = Aralik(0.92f, 1.04f);                           // her gürültü biraz farklı
        bogucu.cutoffFrequency = Mathf.Lerp(4000f, 900f, uzaklik);
        gokSesi.Play();
        Titresim.Vur(Mathf.Lerp(0.6f, 0.25f, uzaklik), 0.15f, Mathf.Lerp(0.9f, 0.5f, uzaklik));   // #124: yakın gürültü sert
        Debug.Log($"[SIMSEK] Gök gürledi ({gecikme:0.0} sn sonra, ses {gokSesi.volume:0.00}, boğma {bogucu.cutoffFrequency:0} Hz).");
    }

    // ---------- GÖLGE ----------
    private bool GolgeGerekirMi()
    {
        if (sablon == null || golgesizCakma < GolgeAraligi - 1) return false;   // üç çakmada en fazla bir
        if (Varlik.Ornek == null || Varlik.Ornek.Kademe < 2 || Varlik.Ornek.AvSuruyor) return false;
        if (kontrol == null || !kontrol.enabled) return false;                  // not, şifre ekranı açık
        if (FigurYurumesi.SahnedeBaskasiVar(null)) return false;               // #142 (A3): sahnede başka bir figür var
        return true;
    }

    // Oyuncunun bulunduğu yerde pencere var mı? Daire dışı (sahanlık, merdiven) evet: ara sahanlıkta
    // merdiven penceresi var. Dairede: odasının bir penceresi ya da balkon kapısı varsa; antre ve hol
    // pencereli bir odaya açık olduğu için onlar da sayılır (salondan sızan ışık).
    private bool PencereliYerde()
    {
        DaireEksenleri e = OyuncununDairesi(out string oda);
        if (e == null) return true;
        if (oda == "Hol" || oda == "Antre") return true;
        string yalin = oda.StartsWith(KilitZinciri.IkinciOnek) ? oda.Substring(KilitZinciri.IkinciOnek.Length) : oda;
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
            if (a.Oda == yalin && (a.Tur == DairePlani.AciklikTuru.Pencere || a.Tur == DairePlani.AciklikTuru.BalkonKapisi))
                return true;
        return false;
    }

    // Oyuncunun içinde durduğu açık daire (yoksa null) ve plandaki odası.
    private DaireEksenleri OyuncununDairesi(out string oda)
    {
        oda = null;
        if (oyuncu == null) return null;
        // Yalnızca etkin daireler (kapalı dairelerin eksenleri FindObjectsByType'a görünmez).
        foreach (DaireEksenleri e in FindObjectsByType<DaireEksenleri>())
        {
            e.PlanNoktasi(oyuncu.position, out float u, out float v);
            string o = DairePlani.NoktaninOdasi(u, v);
            if (o != null) { oda = o; return e; }
        }
        return null;
    }

    // Oyuncunun odasında, 3-6 m ötede, görebileceği karanlık bir köşe arar ve Gölge'yi oraya koyar
    // (gizli; Uygula ışık yanınca gösterir). GezenGolge.YerBul'un kurallarına ek olarak: huzmenin
    // ortasında değil KENARINDA (kafa lambası onu sürekli aydınlatmasın, şimşek göstersin) ve
    // duvara yakın (köşe) noktalar tercih edilir.
    private bool GolgeKoy()
    {
        Camera kamera = kontrol.GetComponentInChildren<Camera>();
        if (kamera == null) return false;
        DaireEksenleri eksen = OyuncununDairesi(out string oyuncuOdasi);

        Vector3 ileri = kamera.transform.forward;
        ileri.y = 0f;
        if (ileri.sqrMagnitude < 0.01f) return false;   // tavana ya da yere bakıyor
        ileri.Normalize();
        Vector3 goz = kamera.transform.position;
        float ayak = oyuncu.position.y;

        bool bulundu = false;
        Vector3 enIyi = Vector3.zero;
        float enIyiPuan = float.MinValue;
        for (int deneme = 0; deneme < 30; deneme++)
        {
            float aci = Aralik(-30f, 30f);
            float mesafe = Aralik(GolgeEnAz, GolgeEnCok);
            Vector3 aday = oyuncu.position + Quaternion.Euler(0f, aci, 0f) * ileri * mesafe;
            // Altında oyuncuyla aynı yükseklikte zemin olmalı (merdivende, boşlukta belirmesin).
            if (!Physics.Raycast(aday + Vector3.up * 1f, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Mathf.Abs(zemin.point.y - ayak) > 0.3f) continue;
            aday.y = zemin.point.y;
            // Oyuncunun odasında mı? (Dairedeyse aynı oda; sahanlıkta oda sorusu yok.)
            if (eksen != null)
            {
                eksen.PlanNoktasi(aday, out float u, out float v);
                if (DairePlani.NoktaninOdasi(u, v) != oyuncuOdasi) continue;
            }
            // Gözle göğsü ve dizi arasında engel yok (oyuncu onu görebilmeli).
            if (Physics.Linecast(goz, aday + Vector3.up * 1.3f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Physics.Linecast(goz, aday + Vector3.up * 0.5f, ~0, QueryTriggerInteraction.Ignore)) continue;
            // Bir insan sığar mı (duvarın, eşyanın içinde belirmesin)?
            if (Physics.CheckCapsule(aday + Vector3.up * 0.35f, aday + Vector3.up * 1.6f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) continue;
            // Ekranda mı? Göğsü görüntünün ortadaki %80'lik alanında olmalı.
            Vector3 ekran = kamera.WorldToViewportPoint(aday + Vector3.up * 1.2f);
            if (ekran.z <= 0f || ekran.x < 0.1f || ekran.x > 0.9f || ekran.y < 0.1f || ekran.y > 0.9f) continue;

            // PUAN: köşelik (çevresinde 0,9 m içinde kaç duvar var) + huzmenin kenarına yakınlık.
            int duvar = 0;
            for (int k = 0; k < 8; k++)
            {
                Vector3 yon = Quaternion.Euler(0f, k * 45f, 0f) * Vector3.forward;
                if (Physics.Raycast(aday + Vector3.up * 1f, yon, 0.9f, ~0, QueryTriggerInteraction.Ignore)) duvar++;
            }
            float puan = duvar + Mathf.Abs(aci) / 15f;
            if (puan > enIyiPuan) { enIyiPuan = puan; enIyi = aday; bulundu = true; }
        }
        if (!bulundu) return false;

        Vector3 bakis = oyuncu.position - enIyi;
        bakis.y = 0f;
        golge = Instantiate(VarlikGorunusu.Sablon(sablon), enIyi, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);   // #142 (A2): figür torbası
        golge.name = "Simsek_Golgesi";
        golge.SetActive(false);   // ışık yanınca Uygula gösterir
        ArkaMuzik.Ornek?.Kabar();
        Debug.Log($"[SIMSEK] Gölge şimşekte göründü ({Vector3.Distance(enIyi, oyuncu.position):0.0} m).");
        return true;
    }

    private void OnDisable()
    {
        // Çakma yarıda kaldıysa (sahne kapanıyor, bileşen kapatıldı) ay ve pencereler eski hâline dönsün.
        StopAllCoroutines();
        Bitir();
    }

    private void OnDestroy()
    {
        // Çalışma anında üretilen malzeme kimseye ait değil; elle silinmezse bellekte kalır.
        if (parlama != null) Destroy(parlama);
    }
}
