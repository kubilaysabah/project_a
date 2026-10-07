// AynaYansimasi.cs
// Banyo aynasının GERÇEK yansıması (#94). Bu bileşen aynanın cam yüzeyinde (Banyo_Aynasi/Cam) durur.
//
// ---------- AYNA NASIL ÇALIŞIR? ----------
// Gerçek bir aynaya bakınca gördüğün şey, aynanın ARKASINDA, sana göre simetrik bir yerde duran
// ikinci bir gözün gördüğüdür: aynadan 1 m uzaktaysan, bu "hayalî göz" aynanın 1 m arkasındadır.
// Biz de tam bunu yapıyoruz: aynanın arkasına ikinci bir kamera koyuyoruz (yansıma kamerası),
// oyuncunun gözünü ayna düzlemine göre katlayıp (yansıtıp) o noktaya yerleştiriyoruz ve bu kamera
// aynanın "penceresinden" odaya bakıyor. Gördüğü resmi bir RENDER TEXTURE'a çiziyoruz.
//
// RENDER TEXTURE (çizim dokusu): ekran yerine bir dokuya (resme) çizim yapan hedef. Kamera ona
// çizer, biz o dokuyu aynanın camına "duvar kâğıdı" gibi giydiririz. Fotoğraf makinesinin çektiği
// fotoğrafı anında bir çerçeveye basmak gibi düşün; her karede yeni fotoğraf.
//
// ---------- KAMERANIN "ÇERÇEVESİ" AYNANIN KENDİSİ ----------
// Normal bir kameranın görüş alanı simetrik bir piramittir (ortası tam önüne bakar). Bizim yansıma
// kameramızın piramidi ise tam olarak aynanın dört köşesinden geçmeli: pencereden bakan birinin
// gördüğü alan pencerenin kenarlarıyla sınırlıdır. Buna "eksen dışı izdüşüm" (off-axis projection)
// denir. Matrix4x4.Frustum ile kuruyoruz: sol / sağ / alt / üst kenarları ayna köşelerinin kameraya
// göre konumundan hesaplıyoruz. İki büyük kazancı var:
//   1. Kameranın gördüğü resim aynanın camına BİREBİR oturur (resmin köşeleri = aynanın köşeleri).
//      Camın kendi doku koordinatları (UV) yeter; özel bir gölgelendirici (shader) gerekmez.
//   2. Kameranın yakın kesme düzlemi (near plane) tam ayna düzlemidir. Aynanın ARKASINDAKİ her şey
//      (duvar, komşu daire) o düzlemin gerisinde kalır ve çizilmez. Ayrıca "eğik kesme" (oblique
//      near plane, CalculateObliqueMatrix) kurmaya gerek kalmaz: kamera zaten aynaya dik bakıyor.
//
// ---------- AYNA TERSTİR: SAĞ-SOL ----------
// Aynadaki dünya sağ-sol terstir. Klasik yol, kameranın matrisini aynalamaktır; ama aynalanmış bir
// matris üçgenlerin dönüş yönünü de ters çevirir ve ekran kartı "arkası dönük" yüzleri gizleyip
// "önü dönük" olanları çizer (GL.invertCulling ile her çizimde düzeltmek gerekir). Biz bunun yerine
// kamerayı NORMAL (aynalanmamış) bir duruşla yerleştiriyoruz ve çıkan resmi camın üstünde yatayda
// çeviriyoruz (doku ölçeği x = -1). Sonuç aynı ayna görüntüsü, ama hiçbir çizim kuralı bozulmuyor:
// ışıklar, gölgeler, kafa lambası yansımada da olduğu gibi çalışıyor.
//
// ---------- URP'DE KAMERAYI NASIL ÇİZDİRİYORUZ? (URP 17.5) ----------
// Eski yol UniversalRenderPipeline.RenderSingleCamera artık "obsolete" (kullanımdan kalkmış).
// Yenisi RenderPipeline.SubmitRenderRequest(SingleCameraRequest); ama bir kameranın çizimi
// sırasında (beginCameraRendering içinde) çağrılırsa "iç içe çizim" olur ve URP buna izin vermez.
// En sağlam ve en basit yol: yansıma kamerasını NORMAL bir kamera olarak bırakıp targetTexture'ını
// (çizeceği dokuyu) vermek ve yalnızca gerektiğinde AÇMAK (enabled = true). Unity dokuya çizen
// kameraları ekran kamerasından ÖNCE çizer (derinliği de bir eksik verdik). Kameranın konumunu ve
// izdüşümünü LateUpdate'te, oyuncu kamerası o karede son hâlini aldıktan sonra yazıyoruz (sıra 250:
// kat yöneticisi 100, lamba titremesi 200; titremiş ışıkları yansıma da aynı karede görür).
// Kamera kapalıyken hiç maliyeti yoktur; açıkken karede en fazla bir kez çizilir.
//
// ---------- NE ZAMAN ÇİZİLİR? ----------
// Yansıma pahalıdır (sahnenin bir kez daha çizilmesi), o yüzden yalnızca şu koşullarda:
//   - grafik kalitesi Düşük değil (Ayarlar.Kalite);
//   - oyuncu aynanın ÖNÜNDE ve 4 m'den yakın;
//   - ayna oyuncu kamerasının görüş alanında (frustum testi);
//   - oyuncunun gözüyle aynanın merkezi ya da köşelerinden biri arasında duvar/kapı yok.
// Çizilmeyen aynada cam yavaşça koyu bir cama döner (eski resim parlak kalmasın: karanlık bir
// banyoda lambası kapalı oyuncu aynada ışıklı bir oda görmesin).
//
// ---------- HARİÇ KATLAR ----------
// Daireler bir kez kurulur, her katta yeniden kullanılır; ayna da bütün katlarda aynı yerde durur.
// Bir katın kendi süsü (5. katın aynalı banyo dolabı) aynı yere geliyorsa o katta ayna gizlenir:
// liste haricKatlar'da, kurucu (AynaKurucu.AynasizKatlar) doldurur.
//
// ---------- YALNIZCA AYNADA GÖRÜNEN ŞEYLER (KATMAN 30) ----------
// Unity'de her nesnenin bir KATMANI (layer, 0-31 arası bir numara) vardır. Her kamera hangi
// katmanları çizeceğini cullingMask ile seçer (32 bitlik bir anahtar listesi: bit açıksa çizer).
// Oyuncu kamerasının 30. katman bitini kapatıyoruz, yansıma kamerasınınki açık kalıyor: 30.
// katmandaki bir nesne yalnızca aynada görünür. AynaKorkusu bunu kullanır.
// 30. katman TagManager'da bilerek ADSIZ bırakıldı: yalnızca aynaya özel nesneler için.

using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(250)]
public class AynaYansimasi : MonoBehaviour
{
    // Yalnızca aynada görünen nesnelerin katmanı. TagManager'da adı yok (bilerek); başka hiçbir
    // şey bu katmanı kullanmamalı, yoksa oyuncu onu da göremez.
    public const int AynaKatmani = 30;

    private const float EnUzak = 4f;            // bu uzaklığa kadar her kare tam yansıma (metre)
    // #127: 4-12 m arası yansıma yine çizilir ama altı karede bir ve kısık (eski cam uzaktan koyu görünür); 12 m
    // ötesi koyu cam. Eskiden 4 m dışında ayna kapkara bir dikdörtgendi (3. kat banyosu koridordan).
    private const float UzakSinir = 12f;
    private const int UzakAralik = 6;
    private const float UzakParlaklik = 0.45f;
    private const float YansimaMenzili = 15f;   // yansıma kamerasının ne kadar uzağı çizdiği
    private const float SolmaSuresi = 0.25f;    // yansıma ile koyu cam arasındaki geçiş (saniye)
    private const int DokuYuksekligi = 384;     // çizim dokusunun piksel yüksekliği (genişlik orana göre)

    [SerializeField] private Camera yansimaKamerasi;               // aynanın arkasındaki ikinci göz (kurucu bağlar)
    [SerializeField] private Renderer[] cerceve = new Renderer[0]; // çerçeve ve arkalık: hariç katta gizlenir
    [SerializeField] private int[] haricKatlar = new int[0];       // aynanın gizlendiği katlar (kurucu yazar)
    [SerializeField] private Color aynaRengi = new Color(0.80f, 0.82f, 0.84f);  // eski cam: biraz gri, biraz yeşil
    [SerializeField] private Color koyuCam = new Color(0.05f, 0.055f, 0.065f);  // yansıma yokken camın rengi (#127: 0,02 siyah okunuyordu)

    private Renderer cam;                      // bu nesnenin (camın) çiziciri
    private RenderTexture doku;                // yansıma kamerasının çizdiği resim
    private MaterialPropertyBlock blok;        // malzemeyi kopyalamadan cama doku ve renk vermek için
    private Camera oyuncuKamerasi;             // oyuncunun gözü
    private Transform oyuncu;                  // oyuncunun gövdesi (ışın testinde kendi çarpıştırıcısını atlamak için)
    private KatYoneticisi yonetici;            // hangi kattayız
    private readonly Plane[] gorusDuzlemleri = new Plane[6];      // oyuncu kamerasının görüş piramidinin 6 yüzü
    private readonly RaycastHit[] isinlar = new RaycastHit[16];   // ışın testi için tekrar kullanılan dizi
    private float parlaklik;                   // 0 koyu cam, 1 tam yansıma (geçiş için)
    private bool gizli;                        // bu katta ayna gizli mi

    // ---------- AynaKorkusu'nun okuduğu bilgiler ----------
    /// <summary>Bu karede yansıma çiziliyor mu?</summary>
    public bool Ciziliyor { get; private set; }
    /// <summary>Bu katta ayna gizli mi (hariç kat)?</summary>
    public bool Gizli { get { return gizli; } }
    /// <summary>Oyuncunun kamerası.</summary>
    public Camera OyuncuKamerasi { get { return oyuncuKamerasi; } }
    /// <summary>Camın ortası (dünyada).</summary>
    public Vector3 Merkez { get { return transform.position; } }
    /// <summary>Camın odaya bakan yönü. Unity'nin Quad'ı -Z yönüne bakar, o yüzden eksi forward.</summary>
    public Vector3 Normal { get { return -transform.forward; } }
    /// <summary>Aynaya bakan oyuncunun sağı (camın +x'i).</summary>
    public Vector3 Sag { get { return transform.right; } }
    /// <summary>Camın yukarısı.</summary>
    public Vector3 Yukari { get { return transform.up; } }
    /// <summary>Camın yarı genişliği (Quad 1 x 1 m; boyutu ölçeğinden gelir).</summary>
    public float YariGenislik { get { return transform.lossyScale.x * 0.5f; } }
    /// <summary>Camın yarı yüksekliği.</summary>
    public float YariYukseklik { get { return transform.lossyScale.y * 0.5f; } }

    private void Awake()
    {
        cam = GetComponent<Renderer>();
        blok = new MaterialPropertyBlock();
        // Kurucu kamerayı KAPALI kaydeder; ne olur ne olmaz, oyun başında da kapatıyoruz.
        if (yansimaKamerasi != null) yansimaKamerasi.enabled = false;
    }

    private void Start()
    {
        // Oyuncuyu ve gözünü buluyoruz (Varlik ve GezenGolge de böyle bulur).
        BirinciSahisKontrolcu kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrol != null)
        {
            oyuncu = kontrol.transform;
            // true: kapalı olsa da bul (başlık ekranında oyuncu kamerası kapalı olabilir).
            oyuncuKamerasi = kontrol.GetComponentInChildren<Camera>(true);
        }
        yonetici = FindAnyObjectByType<KatYoneticisi>();

        if (oyuncuKamerasi != null)
        {
            // Oyuncu kamerası 30. katmanı ÇİZMESİN. "~" bitleri ters çevirir, "&=" o biti kapatır.
            // (1 << 30): yalnızca 30. biti açık bir sayı. Dört dairenin aynası da bunu yapar; aynı
            // biti ikinci kez kapatmak bir şey değiştirmez.
            oyuncuKamerasi.cullingMask &= ~(1 << AynaKatmani);
        }
        KamerayiHazirla();
    }

    // Yansıma kamerasının sabit ayarları (bir kez).
    private void KamerayiHazirla()
    {
        if (yansimaKamerasi == null) return;

        // Çizim dokusu: camın en-boy oranında. DefaultHDR: 1'den parlak değerleri (ampul camı)
        // de saklar; böylece aynadaki ampul de oyuncu kamerasının parlama (bloom) efektine girer.
        int yukseklik = DokuYuksekligi;
        int genislik = Mathf.Max(16, Mathf.RoundToInt(DokuYuksekligi * YariGenislik / Mathf.Max(0.01f, YariYukseklik)));
        doku = new RenderTexture(genislik, yukseklik, 24, RenderTextureFormat.DefaultHDR);
        doku.name = "Ayna_Yansimasi";
        doku.Create();
        // İlk çizimden önce doku boş (rastgele) olabilir; siyaha boyuyoruz.
        RenderTexture onceki = RenderTexture.active;
        RenderTexture.active = doku;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = onceki;

        yansimaKamerasi.targetTexture = doku;      // ekrana değil bu dokuya çiz
        yansimaKamerasi.cullingMask = ~0;           // her katman, 30 dahil
        yansimaKamerasi.useOcclusionCulling = false;
        yansimaKamerasi.allowMSAA = false;
        yansimaKamerasi.allowHDR = true;
        if (oyuncuKamerasi != null)
        {
            // Oyuncu kamerasından ÖNCE çizilsin (küçük derinlik önce çizilir) ve aynı arka planı kullansın.
            yansimaKamerasi.depth = oyuncuKamerasi.depth - 1f;
            yansimaKamerasi.clearFlags = oyuncuKamerasi.clearFlags;
            yansimaKamerasi.backgroundColor = oyuncuKamerasi.backgroundColor;
        }

        // URP'nin kameraya özel ayarları. Son efektler (post-processing) KAPALI: aynanın resmi
        // oyuncu kamerasının efektlerinden zaten geçiyor (cam, ekrandaki her şey gibi renk
        // ayarından geçer); iki kez uygulasaydık ayna öbür her şeyden farklı renkte görünürdü.
        UniversalAdditionalCameraData veri = yansimaKamerasi.GetComponent<UniversalAdditionalCameraData>();
        if (veri == null) veri = yansimaKamerasi.gameObject.AddComponent<UniversalAdditionalCameraData>();
        veri.renderPostProcessing = false;
        veri.renderShadows = true;                  // kafa lambasının gölgeleri yansımada da olsun
        veri.antialiasing = AntialiasingMode.None;
        veri.requiresColorOption = CameraOverrideOption.Off;
        veri.requiresDepthOption = CameraOverrideOption.Off;
    }

    private void LateUpdate()
    {
        if (yansimaKamerasi == null || doku == null) { Ciziliyor = false; return; }

        // 1) Hariç kat mı? Öyleyse ayna (cam + çerçeve) görünmez ve hiçbir şey çizilmez.
        // Bulunulan kat (#103): MevcutKat ulaşılan en yüksek kattır. Oyuncu 8. kattan 7. kata inince sahanlık
        // 7. katın dairesini gösterir; MevcutKat'a bakınca ayna Cemal'in kimlik notunun üstünde beliriyordu.
        int kat = yonetici != null ? yonetici.BulunulanKat : 0;
        bool haric = System.Array.IndexOf(haricKatlar, kat) >= 0;
        if (haric != gizli)
        {
            gizli = haric;
            if (cam != null) cam.enabled = !haric;
            foreach (Renderer r in cerceve) if (r != null) r.enabled = !haric;
        }
        if (haric) { yansimaKamerasi.enabled = false; Ciziliyor = false; return; }

        // 2) Bu karede yansıma gerekli mi? Gerekliyse kamerayı yerleştir ve aç. Uzaktayken (#127) kamera altı
        // karede bir çalışır; aradaki karelerde doku son çizimi tutar (kapalı kamera dokuyu silmez).
        Ciziliyor = CizilmeliMi();
        bool yakin = Ciziliyor && Vector3.Distance(oyuncuKamerasi.transform.position, Merkez) <= EnUzak;
        bool buKare = Ciziliyor && (yakin || Time.frameCount % UzakAralik == 0);
        if (buKare) KamerayiYerlestir();
        yansimaKamerasi.enabled = buKare;

        // 3) Cama dokuyu ve rengi ver. Parlaklık hedefe yumuşakça yaklaşır (MoveTowards: her karede
        // en fazla şu kadar adım).
        float hedef = !Ciziliyor ? 0f : (yakin ? 1f : UzakParlaklik);   // uzaktan kısık yansıma (#127)
        parlaklik = Mathf.MoveTowards(parlaklik, hedef, Time.unscaledDeltaTime / SolmaSuresi);
        if (cam != null)
        {
            cam.GetPropertyBlock(blok);
            blok.SetTexture("_BaseMap", doku);
            // _BaseMap_ST = (ölçek x, ölçek y, kaydırma x, kaydırma y). x ölçeği -1 ve kaydırma 1:
            // resim yatayda çevrilir (ayna sağ-sol terstir, bkz. dosyanın başı).
            blok.SetVector("_BaseMap_ST", new Vector4(-1f, 1f, 1f, 0f));
            blok.SetColor("_BaseColor", Color.Lerp(koyuCam, aynaRengi, parlaklik));
            cam.SetPropertyBlock(blok);
        }
    }

    // Yansıma bu karede çizilmeli mi? (Dosyanın başındaki dört koşul.)
    private bool CizilmeliMi()
    {
        if (oyuncuKamerasi == null || !oyuncuKamerasi.isActiveAndEnabled) return false;
        if (Ayarlar.Kalite == AyarKurallari.KaliteDusuk) return false;   // Düşük kalitede ayna kör cam

        Vector3 goz = oyuncuKamerasi.transform.position;
        // Oyuncu aynanın önünde mi? (Göz ile cam arasındaki farkın camın normaline izdüşümü.)
        float onunde = Vector3.Dot(goz - Merkez, Normal);
        if (onunde < 0.05f) return false;                                   // arkasında ya da cama yapışık
        if (Vector3.Distance(goz, Merkez) > UzakSinir) return false;   // #127: 12 m'ye kadar (4 m ötesi seyrek ve kısık)

        // Ayna oyuncunun görüş piramidinin içinde mi? (Camın kutusu piramidin altı yüzüyle denenir.)
        GeometryUtility.CalculateFrustumPlanes(oyuncuKamerasi, gorusDuzlemleri);
        if (cam != null && !GeometryUtility.TestPlanesAABB(gorusDuzlemleri, cam.bounds)) return false;

        // Arada duvar ya da kapalı kapı var mı? Merkez ya da köşelerden biri açıkta görünüyorsa evet.
        return AcikGoruluyor(goz);
    }

    // Gözden camın merkezine ve dört köşesine (5 cm içeriden) ışın atar; biri engelsizse true.
    private bool AcikGoruluyor(Vector3 goz)
    {
        for (int i = 0; i < 5; i++)
        {
            // i = 0 merkez; 1-4 köşeler. sx, sy: köşenin sağ/sol ve üst/alt işareti.
            float sx = i == 0 ? 0f : (i == 1 || i == 3 ? -1f : 1f);
            float sy = i == 0 ? 0f : (i <= 2 ? -1f : 1f);
            Vector3 hedef = Merkez + Sag * sx * (YariGenislik - 0.05f) + Yukari * sy * (YariYukseklik - 0.05f)
                            + Normal * 0.02f;   // camın 2 cm önü: arkasındaki duvara çarpmasın
            Vector3 fark = hedef - goz;
            float mesafe = fark.magnitude;
            if (mesafe < 0.01f) return true;
            // RaycastNonAlloc: sonuçları her seferinde yeni bir liste açmadan hazır diziye yazar.
            int sayi = Physics.RaycastNonAlloc(goz, fark / mesafe, isinlar, mesafe, ~0, QueryTriggerInteraction.Ignore);
            bool engel = false;
            for (int k = 0; k < sayi; k++)
            {
                // Kamera oyuncunun kendi kapsülünün içinde (CLAUDE.md, #7): kendi çarpıştırıcısını sayma.
                if (oyuncu != null && isinlar[k].transform.IsChildOf(oyuncu)) continue;
                engel = true;
                break;
            }
            if (!engel) return true;
        }
        return false;
    }

    // Yansıma kamerasını oyuncunun gözünün aynadaki "hayalî" yerine koyar ve görüş piramidini
    // aynanın dört köşesine oturtur.
    private void KamerayiYerlestir()
    {
        Vector3 n = Normal;                                       // camın odaya bakan yönü
        Vector3 yukari = Yukari;
        Vector3 goz = oyuncuKamerasi.transform.position;
        float d = Vector3.Dot(goz - Merkez, n);                   // gözün ayna düzlemine uzaklığı (> 0)
        Vector3 hayaliGoz = goz - 2f * d * n;                     // düzleme göre katlanmış göz (aynanın arkasında)

        // Kamera aynaya DİK bakar (odaya doğru, n yönünde); yukarısı camın yukarısı. Bu "normal"
        // bir duruştur (aynalanmış değil), o yüzden üçgenlerin yönü bozulmaz.
        Quaternion donus = Quaternion.LookRotation(n, yukari);
        yansimaKamerasi.transform.SetPositionAndRotation(hayaliGoz, donus);

        // Dünyayı kameranın gözüne çeviren matris. Kamera nesnesi ölçekli bir ebeveynin altında
        // olsa bile doğru olsun diye ölçeksiz kendimiz kuruyoruz. Unity'nin kamera uzayında kamera
        // -Z yönüne bakar; Scale(1, 1, -1) bu yüzden.
        yansimaKamerasi.worldToCameraMatrix =
            Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * Matrix4x4.TRS(hayaliGoz, donus, Vector3.one).inverse;

        // Camın merkezinin kameraya göre konumu: kameranın sağı ve yukarısı boyunca.
        Vector3 kameraSagi = donus * Vector3.right;
        Vector3 fark = Merkez - hayaliGoz;
        float xc = Vector3.Dot(fark, kameraSagi);
        float yc = Vector3.Dot(fark, yukari);
        // Kameranın ayna düzlemine uzaklığı da d (göz düzlemin önünde ne kadar uzaksa hayalî göz
        // arkasında o kadar uzak). Yakın düzlemi camın 3 mm önüne koyuyoruz: camın kendisi ve
        // duvar çizilmez.
        float yakin = d + 0.003f;
        float oran = yakin / d;                                   // köşeleri yakın düzleme taşımak için
        float sol = (xc - YariGenislik) * oran, sag = (xc + YariGenislik) * oran;
        float alt = (yc - YariYukseklik) * oran, ust = (yc + YariYukseklik) * oran;

        // Önce yakın/uzak kesme değerleri (URP gölge mesafesi için bunları da okur), sonra izdüşüm.
        // Özel izdüşüm matrisi verince kamera kendi görüş açısını (fieldOfView) artık kullanmaz.
        yansimaKamerasi.nearClipPlane = yakin;
        yansimaKamerasi.farClipPlane = yakin + YansimaMenzili;
        yansimaKamerasi.projectionMatrix = Matrix4x4.Frustum(sol, sag, alt, ust, yakin, yakin + YansimaMenzili);
    }

    private void OnDisable()
    {
        // Daire kapanınca (kat değişti) ya da lavabo gizlenince kamera da kapanır.
        if (yansimaKamerasi != null) yansimaKamerasi.enabled = false;
        Ciziliyor = false;
    }

    private void OnDestroy()
    {
        // Çizim dokusu ekran kartı belleğinde durur; nesne yok olurken elle bırakılmalı.
        if (yansimaKamerasi != null) yansimaKamerasi.targetTexture = null;
        if (doku != null) { doku.Release(); Destroy(doku); }
    }
}
