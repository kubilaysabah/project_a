// SaklanmaYeri.cs
// Gardıroba ya da boy dolabına saklanmak (#97).
//
// Şahan'ın isteği: Gölge peşindeyken (Varlık'ın avı, #86; 6. katın saklanma anı, #58) oyuncunun
// gerçek bir saklanma yeri olsun. Çömelip lambayı kapatmak işe yarıyordu ama "dolaba girip kapak
// aralığından bakmak" korku oyunlarının en bilinen anı; No70'te de, Phasmophobia'da da var.
//
// NASIL ÇALIŞIR
// Bileşen mobilyanın kökünde durur ve IEtkilesimli sözleşmesini imzalar ("SAKLAN"). Oyuncu E'ye basınca:
//   1. Ekran bir an kararır (göz kırpar gibi), kontrolcü ve Etkilesimci kapanır, kafa lambası söner.
//   2. Oyuncu nesnesi BÜTÜNÜYLE dolabın içine taşınır. Çarpışma kapsülü (CharacterController) önce
//      kapatılır: kapalı kapsül fizikte yok sayılır, dolabın duvarları onu dışarı itmez.
//   3. Kamera dolabın önüne (+Z, bütün modellerin ön yüzü) bakar; fareyle biraz sağa-sola (±35°),
//      biraz yukarı-aşağı (±20°) bakılabilir.
//   4. Ekrana iki koyu panel çizilir; ortada dar bir dikey aralık kalır: kapak aralığından bakış.
//   5. E / A ya da Esc / B: çıkılır. Oyuncu dolabın önüne bırakılır (orası doluysa girdiği yere).
//
// Kamera dolabın içindeyken neden dolabın duvarları görünmüyor? Modellerin yüzleri dışarı bakar;
// içeriden bakınca yüzlerin ARKASI görünür ve Unity yüzlerin arkasını çizmez (backface culling).
// Kapaklar da içeriden şeffaf olur; bakış daralmasını ekrandaki aralık verir.
//
// GÖLGE NE BİLİR (Varlik.cs, SaklanmaAni.cs): saklanan oyuncu görülmez, iz bırakmaz, yakalanmaz.
// Gölge izin sonuna (oyuncunun dolaba girdiği yere) gelir, durur, bakınır ve vazgeçer.
//
// KİM EKLER: ZincirUygulayici, süsler konduktan sonra DaireyeEkle(daire) der. O da dairenin
// mobilyalarını tarar ve kurala (SaklanmaKurali) uyanlara bu bileşeni ekler.

using UnityEngine;
using UnityEngine.InputSystem;

public class SaklanmaYeri : MonoBehaviour, IEtkilesimli
{
    // ---------- DURUM (static) ----------
    // static: bu alanlar tek bir mobilyanın değil, bütün oyunun. Aynı anda yalnızca bir dolapta saklanılır.

    /// <summary>Şu an saklanılan yer (saklanılmıyorsa null).</summary>
    public static SaklanmaYeri Aktif { get; private set; }

    /// <summary>Oyuncu şu an bir dolapta saklanıyor mu?</summary>
    public static bool Saklaniyor { get { return Aktif != null; } }

    // Oyun başlarken (sahne yüklenmeden önce) sıfırlanır. Editörde "alan yeniden yüklemesi" kapalıyken
    // static alanlar bir Play'den ötekine taşınabilir; bu öznitelik onları her Play'de temizler.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla() { Aktif = null; karartma = 0f; karartmaSahibi = null; }

    // ---------- AYARLAR ----------
    private const float GirisKararmasi = 0.35f;     // içeri girince ekranın açılma süresi (saniye)
    private const float CikisKararmasi = 0.3f;
    private const float FareDerecesi = 0.08f;       // 1 piksel = bu kadar derece (normal bakıştan yavaş: dar yerdesin)
    private const float CubukHizi = 70f;            // sağ çubuk tam itişte derece/saniye
    private const float AralikOrani = 0.22f;        // ekrandaki aralığın genişliği (ekran genişliğinin oranı)
    private const float GicirtiSesi = 0.25f;        // kapak gıcırtısının sesi (küçük)

    // ---------- ALANLAR ----------
    private static float karartma;                  // 1 = simsiyah, 0 = açık
    // Karartmayı yalnızca son girilen/çıkılan dolap söndürür ve çizer; yoksa sahnedeki her dolap
    // aynı değeri bir kez daha azaltır ve ekran dolap sayısı kadar hızlı açılırdı.
    private static SaklanmaYeri karartmaSahibi;
    private static AudioClip gicirti;

    private GameObject oyuncu;
    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private CharacterController govde;
    private Transform kamera;
    private KafaLambasi lamba;
    private InputAction etkilesimEylemi;

    private Vector3 eskiKonum;                      // girmeden önce oyuncunun durduğu yer (yedek çıkış)
    private Quaternion eskiDonus;
    private Quaternion eskiKameraDonusu;            // kameranın kontrolcüdeki yukarı-aşağı açısı
    private bool lambaAcikti;
    private int acilisKaresi;
    private bool cikilacak;
    private float yatay, dikey;                     // gizliyken bakış açıları (derece)

    // ---------- SÖZLEŞME (IEtkilesimli) ----------

    public string Ipucu { get { return Dil.Getir("ipucu.saklan"); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.Saklan; } }   // #104

    /// <summary>
    /// Şu an saklanılabilir mi? Hayır, eğer: zaten bir yerde saklanılıyorsa, dolabın (ya da kapağının)
    /// zincir kilidi çözülmemişse (6. katın büfesi sofra kurulana kadar kilitli), ya da bir Gölge
    /// dolabın önüne 1,5 m'den yakınsa (gözünün önünde dolaba girilmez).
    /// </summary>
    public bool EtkilesimeAcik
    {
        get
        {
            if (Saklaniyor || !isActiveAndEnabled) return false;
            if (KilitliMi()) return false;
            if (YakindaGolgeVar(OnNoktasi(), SaklanmaKurali.GolgeYakinligi)) return false;
            // 7. katın kovalamacasında dolap yok (#103): silüet oyuncunun yürüdüğü yolu adım adım izler ve
            // yolun sonunda dolabın içindeki oyuncuyu bulurdu. Kovalamacanın kuralı kaçmaktır.
            foreach (KacisAni k in FindObjectsByType<KacisAni>()) if (k.Suruyor) return false;
            return true;
        }
    }

    public void Etkilesim(GameObject oyuncuNesnesi)
    {
        if (!EtkilesimeAcik || oyuncuNesnesi == null) return;
        Gir(oyuncuNesnesi);
    }

    // ---------- GİRİŞ ----------

    private void Gir(GameObject oyuncuNesnesi)
    {
        oyuncu = oyuncuNesnesi;
        kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
        etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        govde = oyuncu.GetComponent<CharacterController>();
        // Kamera oyuncunun ilk çocuğu (kontrolcü de böyle bulur).
        Camera c = oyuncu.GetComponentInChildren<Camera>();
        kamera = c != null ? c.transform : (oyuncu.transform.childCount > 0 ? oyuncu.transform.GetChild(0) : null);
        if (kamera == null) return;
        lamba = FindAnyObjectByType<KafaLambasi>();

        // Eski hâl kaydedilir: çıkışta geri verilecek.
        eskiKonum = oyuncu.transform.position;
        eskiDonus = oyuncu.transform.rotation;
        eskiKameraDonusu = kamera.localRotation;

        // Yakın bakış kalıbı (IncelemeEkrani, YoneticiNotu): kontrolcü ve Etkilesimci kapanır. Etkilesimci
        // kapanırken PAYLAŞILAN Interact eylemini de kapatır (OnDisable); çıkış tuşu onu okuyacağı için
        // eylem yeniden açılır.
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();

        // Kafa lambası söner (karanlık dolapta ışık sızdırmak saklanmanın tersi); çıkışta eski hâline döner.
        lambaAcikti = lamba != null && lamba.Takili && lamba.Acik;
        if (lambaAcikti) lamba.Degistir();

        // Kapsül kapanır: dolabın içindeki oyuncuyu duvarlar dışarı itmesin, Gölge'nin ışınları da
        // (QueryTriggerInteraction.Ignore ile atılanlar dahil) ona çarpmasın.
        if (govde != null) govde.enabled = false;

        // Oyuncu dolabın önüne döner; kamera düz bakar. Sonra oyuncu, KAMERASI içerideki göz noktasına
        // gelecek şekilde taşınır (kamera oyuncunun içinde 1,65 m yukarıda; eğikse 0,6 m).
        Vector3 on = transform.forward; on.y = 0f;
        if (on.sqrMagnitude < 0.0001f) on = Vector3.forward;
        oyuncu.transform.rotation = Quaternion.LookRotation(on.normalized, Vector3.up);
        kamera.localRotation = Quaternion.identity;
        Vector3 kameraFarki = kamera.position - oyuncu.transform.position;
        oyuncu.transform.position = IcNokta() - kameraFarki;
        // SyncTransforms: fizik dünyasına "nesneler yer değiştirdi" der; yoksa bu karedeki ışınlar
        // eski konumları görür.
        Physics.SyncTransforms();

        yatay = 0f; dikey = 0f;
        cikilacak = false;
        acilisKaresi = Time.frameCount;   // içeri sokan E aynı karede dışarı çıkarmasın
        karartma = 1f;
        karartmaSahibi = this;
        Aktif = this;
        GicirtiCal();
        Debug.Log($"[SAKLANMA] {name} içine saklanıldı.");
    }

    // ---------- HER KARE ----------

    private void Update()
    {
        // Karartma söner (girişte ve çıkışta). MoveTowards: değeri hedefe en çok verilen adım kadar yaklaştırır.
        if (karartmaSahibi == this && karartma > 0f)
            karartma = Mathf.MoveTowards(karartma, 0f, Time.deltaTime / (Aktif == this ? GirisKararmasi : CikisKararmasi));

        if (Aktif != this || kamera == null) return;
        if (Time.timeScale <= 0f) return;   // zaman durmuşsa (menü) tuş okunmaz

        // Etrafa bakmak: fareyi ve sağ çubuğu doğrudan okuruz (kontrolcü kapalı).
        float h = Ayarlar.Hassasiyet;
        Vector2 delta = Vector2.zero;
        if (Mouse.current != null) delta = Mouse.current.delta.ReadValue() * FareDerecesi * h;
        if (Gamepad.current != null)
        {
            Vector2 cubuk = Gamepad.current.rightStick.ReadValue();
            if (cubuk.magnitude > 0.2f) delta += cubuk * CubukHizi * h * Time.deltaTime;   // ölü bölge: kayan çubuk
        }
        if (Ayarlar.Ters) delta.y = -delta.y;
        yatay = SaklanmaKurali.Sinirla(yatay + delta.x, SaklanmaKurali.YatayBakisSiniri);
        dikey = SaklanmaKurali.Sinirla(dikey - delta.y, SaklanmaKurali.DikeyBakisSiniri);   // eksi: fare yukarı = yukarı bak
        kamera.localRotation = Quaternion.Euler(dikey, yatay, 0f);

        if (Time.frameCount <= acilisKaresi) return;
        bool etkilesim = etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame();
        bool geri = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                 || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        if (etkilesim || geri) cikilacak = true;
    }

    // Kontrol LateUpdate'te geri verilir: çıkışa basılan tuş aynı karede Etkilesimci'ye ulaşıp
    // (dolabın kapağını açmak gibi) bir şey yapmasın; Etkilesimci'nin Update'i bu karede çoktan geçti.
    private void LateUpdate()
    {
        if (Aktif == this && cikilacak) Cik(true);
    }

    // ---------- ÇIKIŞ ----------

    // sesli: normal çıkışta kapak gıcırdar ve ekran bir an kararır; zorla çıkışta (dolap yok olurken) yok.
    private void Cik(bool sesli)
    {
        Aktif = null;
        cikilacak = false;
        if (oyuncu == null) return;
        if (kamera != null) kamera.localRotation = eskiKameraDonusu;

        // Dolabın önü boşsa oyuncu oraya, dolaba sırtını vermiş hâlde bırakılır; doluysa (önüne bir şey
        // çekilmiş) girdiği yere ve eski yönüne.
        Vector3 on = OnNoktasi();
        if (OnBos(on)) oyuncu.transform.position = on;
        else { oyuncu.transform.position = eskiKonum; oyuncu.transform.rotation = eskiDonus; }
        Physics.SyncTransforms();

        if (govde != null) govde.enabled = true;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
        // Lamba girerken yanıyorduysa yine yanar; sönüktüyse sönük kalır. (Gizliyken F'ye basılmışsa da
        // girdiği hâle döner.)
        if (lamba != null && lamba.Takili && lamba.Acik != lambaAcikti) lamba.Degistir();

        if (sesli)
        {
            karartma = 1f;
            karartmaSahibi = this;
            GicirtiCal();
        }
        Debug.Log($"[SAKLANMA] {name} içinden çıkıldı.");
    }

    /// <summary>
    /// Oyuncu bir dolaptaysa hemen, sessizce çıkarılır (#103). Kat baştan kurulmadan ÖNCE çağrılır: yoksa
    /// kurulum dolabı kapatınca (OnDisable) oyuncu karartmanın ortasında dolabın önüne geri konur ve
    /// kontrolü erken geri alır.
    /// </summary>
    public static void ZorlaCik()
    {
        if (Aktif != null) Aktif.Cik(false);
    }

    // Dolap saklanırken yok olursa (daire yeniden kuruldu, sahne kapandı) oyuncu içeride kilitli
    // kalmasın: her şey geri verilir.
    private void OnDisable()
    {
        // Sahne kapanırken yeni ses nesnesi yaratmak "temizlenmemiş nesne" uyarısı verir: sessiz çıkış.
        if (Aktif == this) Cik(false);
        if (karartmaSahibi == this) { karartmaSahibi = null; karartma = 0f; }
    }

    // ---------- ÖLÇÜLER ----------

    // Mobilyanın kendi eksenlerindeki sınır kutusu. Renderer.bounds dünya eksenlerinde ve eğik duran
    // bir dolapta şişer; pasif nesnede de boş gelir. Bunun yerine her ağın (mesh) kendi kutusunun
    // sekiz köşesi mobilyanın köküne göre hesaplanır: dönük de dursa, gizli de olsa doğru ölçü.
    private static bool YerelSinir(Transform kok, out Bounds kutu)
    {
        kutu = new Bounds();
        bool ilk = true;
        foreach (MeshFilter mf in kok.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            Bounds b = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                // i'nin üç biti köşenin hangi uçta olduğunu seçer (0 = min, 1 = max).
                Vector3 k = new Vector3((i & 1) == 0 ? b.min.x : b.max.x,
                                        (i & 2) == 0 ? b.min.y : b.max.y,
                                        (i & 4) == 0 ? b.min.z : b.max.z);
                Vector3 yerel = kok.InverseTransformPoint(mf.transform.TransformPoint(k));
                if (ilk) { kutu = new Bounds(yerel, Vector3.zero); ilk = false; }
                else kutu.Encapsulate(yerel);
            }
        }
        return !ilk;
    }

    // Göz noktası (dünya): kutunun ortasında, ön yüzün 15 cm arkasında, yerden min(1,55, tepe-0,15).
    // Modellerin ölçeği 1 kabul edilir (bütün mobilya şablonları öyle).
    private Vector3 IcNokta()
    {
        if (!YerelSinir(transform, out Bounds b)) return transform.position + Vector3.up * 1.5f;
        float goz = SaklanmaKurali.GozYuksekligi(b.size.y);
        return transform.TransformPoint(new Vector3(b.center.x, b.min.y + goz, b.max.z - SaklanmaKurali.OnPayi));
    }

    // Ön nokta (dünya, ayak hizası): dolabın ön yüzünün önünde, oyuncunun kapsülü sığacak kadar ötede.
    private Vector3 OnNoktasi()
    {
        if (!YerelSinir(transform, out Bounds b)) return transform.position + transform.forward;
        float yaricap = govde != null ? govde.radius : 0.3f;
        Vector3 on = transform.forward; on.y = 0f;
        if (on.sqrMagnitude < 0.0001f) on = Vector3.forward;
        return transform.TransformPoint(new Vector3(b.center.x, b.min.y, b.max.z)) + on.normalized * (yaricap + 0.12f);
    }

    // Ön nokta boş mu? Oyuncunun kapsülü kadar bir hacimde başka çarpıştırıcı var mı (CheckCapsule)?
    // Oyuncunun kendi kapsülü şu an kapalı, sayılmaz. Tetikleyiciler (trigger) yok sayılır.
    private bool OnBos(Vector3 ayak)
    {
        float r = govde != null ? govde.radius : 0.3f;
        float boy = govde != null ? govde.height : 1.75f;
        return !Physics.CheckCapsule(ayak + Vector3.up * (r + 0.05f), ayak + Vector3.up * (boy - r), r - 0.02f, ~0, QueryTriggerInteraction.Ignore);
    }

    // Dolabın ya da bir parçasının (kapağının) çözülmemiş bir zincir kilidi var mı?
    private bool KilitliMi()
    {
        foreach (Kilit k in GetComponentsInChildren<Kilit>(true)) if (!k.Cozuk) return true;
        return false;
    }

    /// <summary>
    /// Bir Gölge (Varlık'ın av Gölgesi ya da 6. katın saklanma Gölgesi) bu noktaya yatayda
    /// 'mesafe'den yakın mı?
    /// </summary>
    public static bool YakindaGolgeVar(Vector3 nokta, float mesafe)
    {
        if (Varlik.Ornek != null && YatayYakin(Varlik.Ornek.AvGolgesi, nokta, mesafe)) return true;
        foreach (SaklanmaAni s in FindObjectsByType<SaklanmaAni>())
            if (YatayYakin(s.Golge, nokta, mesafe)) return true;
        return false;
    }

    private static bool YatayYakin(Transform t, Vector3 nokta, float mesafe)
    {
        if (t == null) return false;
        Vector3 fark = t.position - nokta; fark.y = 0f;
        return fark.magnitude < mesafe && Mathf.Abs(t.position.y - nokta.y) < 1.5f;
    }

    // Dolabın kapı sesi dolabın kendisine uyar (#99): çelik dolap tınlar, ahşap gardırobun kapağı
    // gıcırdar. Eskiden hepsi parke gıcırtısıydı.
    private void GicirtiCal()
    {
        bool metal = SesMalzemesi.MetalMi(AcilirParca.AdYolu(transform));
        AudioClip klip = Resources.Load<AudioClip>(metal ? "Sesler/Metal_Kapak_Acilma" : "Sesler/Kapak_Acilma");
        if (klip == null)
        {
            if (gicirti == null) gicirti = Resources.Load<AudioClip>("Sesler/Gicirti_1");
            klip = gicirti;
        }
        if (klip != null) AudioSource.PlayClipAtPoint(klip, OnNoktasi() + Vector3.up * 1.2f, GicirtiSesi);
    }

    // ---------- DAİREYE EKLEME ----------

    /// <summary>
    /// Dairenin mobilyalarını tarar; saklanma kuralına uyanlara (SaklanmaKurali.Aday) bu bileşeni
    /// ekler. Kaç kez çağrılsa da her mobilyaya tek bileşen düşer. Dönüş: dairede şu an görünen
    /// saklanma yeri sayısı. ZincirUygulayici süsleri koyduktan sonra çağırır (iki daireli katta
    /// her daire için ayrı).
    /// Bakılan yerler: dairenin doğrudan çocukları (süsler "Sus_..." ya da adı verilmiş süsler) ve
    /// "Esyalar" altındaki kalıcı eşyalar (6. katın büfesi gibi bulmaca eşyaları).
    /// </summary>
    public static int DaireyeEkle(Transform daire)
    {
        if (daire == null) return 0;
        int sayi = 0;
        sayi += Tara(daire);
        Transform esyalar = daire.Find("Esyalar");
        if (esyalar != null) sayi += Tara(esyalar);
        return sayi;
    }

    private static int Tara(Transform ebeveyn)
    {
        int sayi = 0;
        foreach (Transform t in ebeveyn)
        {
            if (!SaklanmaYeriMi(t)) continue;
            if (t.GetComponent<SaklanmaYeri>() == null) t.gameObject.AddComponent<SaklanmaYeri>();
            if (t.gameObject.activeInHierarchy) sayi++;
        }
        return sayi;
    }

    private static bool SaklanmaYeriMi(Transform t)
    {
        // Kökü kayan mobilya saklanma yeri olmaz: 5. katın gardırobu kökünden itilir (AcilirParca) ve
        // onun "İT"i hep önce gelirdi. Kapakları ve çekmeceleri açılan dolaplar (parçalar çocukta) olur.
        if (t.GetComponent<AcilirParca>() != null) return false;
        if (!YerelSinir(t, out Bounds b)) return false;
        return SaklanmaKurali.Aday(t.name, b.size.x, b.size.z, b.size.y);
    }

    // ---------- EKRAN ----------
    // Kapak aralığı: ekranın iki yanına koyu paneller; ortada ekranın %22'si kadar dikey bir aralık.
    // Panellerin aralığa bakan kenarı yumuşak: birkaç ince şerit, aralığa yaklaştıkça daha saydam.
    private static GUIStyle ipucuStili;
    private const int KenarAdimi = 8;               // yumuşak kenardaki şerit sayısı
    private const float KenarGenisligi = 70f;       // yumuşak kenarın toplam genişliği (sanal piksel)
    private static readonly Color Panel = new Color(0.01f, 0.008f, 0.006f, 0.97f);

    private void OnGUI()
    {
        bool cizAralik = Aktif == this;
        bool cizKarartma = karartma > 0f && karartmaSahibi == this;
        if (!cizAralik && !cizKarartma) return;
        if (Event.current.type != EventType.Repaint) return;
        ArayuzOlcegi.Uygula();
        float G = ArayuzOlcegi.Genislik, Y = ArayuzOlcegi.Yukseklik;

        if (cizAralik)
        {
            float aralik = G * AralikOrani;
            float sol = (G - aralik) / 2f;              // sol panelin sağ kenarı (aralığın solu)
            float sag = sol + aralik;                   // sağ panelin sol kenarı
            // Tam koyu kısımlar: yumuşak kenarın dışında kalan alan.
            GUI.color = Panel;
            GUI.DrawTexture(new Rect(0f, 0f, sol - KenarGenisligi, Y), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(sag + KenarGenisligi, 0f, G - sag - KenarGenisligi, Y), Texture2D.whiteTexture);
            // Yumuşak kenar: dıştan içe saydamlaşan şeritler.
            float serit = KenarGenisligi / KenarAdimi;
            for (int i = 0; i < KenarAdimi; i++)
            {
                float a = Panel.a * (1f - (i + 1f) / (KenarAdimi + 1f));
                GUI.color = new Color(Panel.r, Panel.g, Panel.b, a);
                GUI.DrawTexture(new Rect(sol - KenarGenisligi + i * serit, 0f, serit + 0.5f, Y), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(sag + KenarGenisligi - (i + 1) * serit - 0.5f, 0f, serit + 0.5f, Y), Texture2D.whiteTexture);
            }

            // Çıkış ipucu: "E: çık" (kolda "A: çık").
            if (ipucuStili == null)
            {
                ipucuStili = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                ipucuStili.normal.textColor = new Color(0.8f, 0.78f, 0.72f);
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(0f, Y - 70f, G, 30f),
                      string.Format(Dil.Getir("ipucu.saklanma_tuslar"), GirisCihazi.Kol ? "A" : "E"), ipucuStili);
        }

        if (cizKarartma)
        {
            GUI.color = new Color(0f, 0f, 0f, karartma);
            GUI.DrawTexture(new Rect(0f, 0f, G, Y), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }
}
