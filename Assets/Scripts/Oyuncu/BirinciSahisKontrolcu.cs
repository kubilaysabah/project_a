// BirinciSahisKontrolcu.cs
// Kapıcının gözünden yürüme ve bakma. Koşma YOK: korku temposu yavaş yürüyüşle kurulur.
//
// Bu script bir "MonoBehaviour"dur. MonoBehaviour, Unity'nin her karede konuşabildiği
// script türüdür; bir GameObject'e takılır ve Unity onun Awake/Start/Update
// fonksiyonlarını kendisi çağırır. Biz hiçbir yerden elle çağırmayız.

using UnityEngine;                 // GameObject, Transform, Vector3 gibi temel tipler
using UnityEngine.InputSystem;     // Yeni Input System: klavye/fare okuma buradan yapılır

// [RequireComponent]: bu script bir nesneye takıldığında, Unity o nesnede
// CharacterController yoksa otomatik ekler ve silinmesine izin vermez.
// Böylece "controller null" hatası daha oluşmadan engellenmiş olur.
[RequireComponent(typeof(CharacterController))]
public class BirinciSahisKontrolcu : MonoBehaviour
{
    // ---------- INSPECTOR'DAN AYARLANAN DEĞERLER ----------
    // [SerializeField] : değişken private kalsın ama Unity penceresinde görünsün.
    // private tutmamızın sebebi, başka scriptlerin bunları izinsiz değiştirmemesi.
    // [Header] ve [Tooltip] sadece Inspector'ı okunur kılar, oyunu etkilemez.

    [Header("Yürüyüş")]
    [Tooltip("Saniyede kaç metre yürünür. Gerçek bir insan yürüyüşü ~1.4 m/s'dir.")]
    [SerializeField] private float yurumeHizi = 1.8f;

    [Tooltip("Yer çekimi ivmesi. Eksi değer aşağı çeker.")]
    [SerializeField] private float yercekimi = -12f;

    [Header("Bakış")]
    [Tooltip("Fare hassasiyeti. Büyüdükçe kamera daha hızlı döner.")]
    [SerializeField] private float fareHassasiyeti = 0.08f;

    [Tooltip("Oyun kolu sağ çubuğu tam eğikken saniyede kaç derece dönülür (#29).")]
    [SerializeField] private float cubukHizi = 150f;

    [Tooltip("Kameranın yukarı/aşağı bakabileceği en büyük açı (derece).")]
    [SerializeField] private float bakisSiniri = 85f;

    [Header("Ölü bölge")]
    [Tooltip("Bu büyüklüğün altındaki girişler yok sayılır. Oyun kolu çubukları " +
             "boştayken bile küçük değerler üretir (kayma); bu ayar onu susturur. " +
             "Fare hareketi piksel cinsinden çok daha büyük olduğu için fareyi etkilemez.")]
    [SerializeField] private float oluBolge = 0.15f;

    [Header("Eğilme (#79)")]
    [Tooltip("Eğilince göz yüksekliği (metre). Koltuk ve yatak altına bakabilmek için alçak.")]
    [SerializeField] private float egikGoz = 0.6f;

    [Tooltip("Eğilince çarpışma kapsülünün boyu (metre). Yarıçapın iki katından kısa olamaz.")]
    [SerializeField] private float egikBoy = 0.8f;

    [Tooltip("Eğilmiş yürüme hızı (metre/saniye).")]
    [SerializeField] private float egikHiz = 0.8f;

    [Tooltip("Eğilip kalkmanın hızı: büyüdükçe göz yeni yüksekliğine daha çabuk varır.")]
    [SerializeField] private float egilmeGecisi = 8f;

    [Header("Bağlantılar")]
    [Tooltip("Oyuncunun çocuğu olan kamera. Boş bırakılırsa çocuklardan otomatik bulunur.")]
    [SerializeField] private Transform kamera;

    // ---------- İÇ DEĞİŞKENLER ----------
    // Bunlar Inspector'da görünmez; scriptin kendi defteridir.

    private CharacterController kontrolcu;   // çarpışmayı ve kaymayı yöneten Unity bileşeni
    private float dikeyHiz;                  // düşme hızı; her kare yer çekimiyle artar
    private float bakisAcisi;                // kameranın o anki yukarı/aşağı açısı

    // Input System'deki "eylem"ler. Eylem, tuşun kendisi değil, tuşun ANLAMIDIR:
    // "Move" eylemi hem WASD'ye hem de oyun kolunun çubuğuna bağlıdır.
    private InputAction hareketEylemi;
    private InputAction bakisEylemi;

    // ---- EĞİLME ----
    private float ayaktaBoy, ayaktaGoz;   // ayakta dururken kapsül boyu ve göz yüksekliği (Awake'te okunur)
    private bool egik;                    // şu an eğik mi (ya da eğilmek üzere mi)
    private bool koldanEgik;              // oyun kolunda eğilme aç-kapa düğmesiyle; klavyede Ctrl basılı tutulur

    /// <summary>Oyuncu eğik mi (#79)?</summary>
    public bool Egik { get { return egik; } }

    // ---------- AWAKE ----------
    // Awake, nesne sahnede belirdiği anda BİR KEZ çalışır. Start'tan da öncedir.
    // Bağlantıları burada kurarız ki sonraki her karede hazır olsunlar.
    private void Awake()
    {
        // GetComponent: aynı nesnedeki başka bir bileşeni bulur.
        kontrolcu = GetComponent<CharacterController>();

        // Kamera Inspector'da boş bırakıldıysa çocuklar arasında ara.
        if (kamera == null && transform.childCount > 0)
        {
            kamera = transform.GetChild(0);
        }

        // Proje geneli giriş varlığı: Assets/InputSystem_Actions.inputactions
        // Proje Ayarları'nda "Project-wide Actions" olarak atanmış durumda,
        // bu yüzden koddan doğrudan erişebiliyoruz. Elle bağlama gerekmiyor.
        var varlik = InputSystem.actions;

        if (varlik == null)
        {
            // Hata sessizce yutulmasın; ne yapılacağını da yazalım.
            Debug.LogError("Proje geneli giriş varlığı bulunamadı. " +
                           "Edit > Project Settings > Input System Package bölümünden atayın.");
            return;
        }

        // FindAction: eylemi "harita/eylem" yoluyla bulur.
        hareketEylemi   = varlik.FindAction("Player/Move");
        bakisEylemi     = varlik.FindAction("Player/Look");

        // Ayaktaki ölçüler, oyuncu kurulurken verilenler (OyuncuKurucu). Eğilince bunlardan
        // eğik ölçülere, kalkınca yeniden bunlara döneriz.
        ayaktaBoy = kontrolcu.height;
        ayaktaGoz = kamera != null ? kamera.localPosition.y : 1.65f;
    }

    // ---------- ONENABLE / ONDISABLE ----------
    // Nesne açıldığında eylemleri dinlemeye başla, kapandığında bırak.
    // Bırakmazsak nesne yok olduktan sonra da giriş dinlenmeye devam eder.

    private void OnEnable()
    {
        // "?." işareti: soldaki null ise hiçbir şey yapma, hata verme.
        hareketEylemi?.Enable();
        bakisEylemi?.Enable();
    }

    private void OnDisable()
    {
        hareketEylemi?.Disable();
        bakisEylemi?.Disable();
    }

    // ---------- START ----------
    // Start, ilk karede bir kez çalışır. Awake'ten sonradır.
    private void Start()
    {
        FareyiKilitle(true);

        // Görüş açısı ayarı (#27). Haberi OnEnable/OnDisable'da değil burada dinliyoruz:
        // Esc menüsü açıkken bu bileşen KAPALI. OnDisable'da dinlemeyi bıraksaydık,
        // oyuncu menüde görüş açısını değiştirdiğinde kamera menüden çıkana kadar
        // değişmezdi. Start/OnDestroy nesnenin ömrü boyunca bir kez çalışır.
        if (kamera != null) kameraBileseni = kamera.GetComponent<Camera>();
        GorusAcisiniUygula();
        Ayarlar.Degisti += GorusAcisiniUygula;
    }

    private void OnDestroy()
    {
        // Nesne yok olurken dinlemeyi bırak; yoksa Ayarlar yok olmuş bir kameraya yazmaya çalışır.
        Ayarlar.Degisti -= GorusAcisiniUygula;
    }

    private Camera kameraBileseni;

    private void GorusAcisiniUygula()
    {
        // fieldOfView: kameranın dikey görüş açısı (derece). Yalnızca oyuncunun kamerası;
        // finaldeki dış kamera ayrı bir kamera ve kendi açısında kalır.
        if (kameraBileseni != null) kameraBileseni.fieldOfView = Ayarlar.Gorus;
    }

    // ---------- UPDATE ----------
    // Update HER KAREDE çalışır. Saniyede 60 kare varsa saniyede 60 kez.
    private void Update()
    {
        Bak();
        Egil();
        Yuru();

        // Escape artık burada değil, duraklatma menüsünde (#15): menü fareyi serbest
        // bırakır ve kapanınca geri kilitler. İki yer aynı tuşa tepki verirse biri
        // diğerinin yaptığını bozar.

        // Sol tık: fareyi tekrar kilitle (ör. Windows'ta başka pencereye geçip dönünce).
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            FareyiKilitle(true);
        }
    }

    // ---------- BAKIŞ ----------
    private void Bak()
    {
        if (bakisEylemi == null || kamera == null) return;

        // ReadValue: eylemin o anki değerini okur. Fare için bu değer,
        // farenin BU KAREDE kaç piksel kaydığıdır (konumu değil, kayması).
        Vector2 fare = OluBolgeUygula(bakisEylemi.ReadValue<Vector2>());

        // Fare ile oyun kolu çubuğu FARKLI şeyler söyler (#29):
        //   Fare  → "bu karede şu kadar piksel kaydım". Zaten kareye özgü bir miktar;
        //           deltaTime ile ÇARPMAYIZ, yoksa kare hızı değiştikçe his bozulur.
        //   Çubuk → "şu kadar eğiğim" (−1 ile 1 arası). Bu bir hız isteğidir; bir
        //           karede ne kadar döneceğimizi bulmak için deltaTime ile ÇARPARIZ.
        // Eskiden çubuğa da fare hesabı uygulanıyordu: bakış hızı kare hızına bağlıydı
        // (165 FPS'te 60 FPS'in ~3 katı) ve çok yavaştı (~13°/sn).
        // activeControl: eyleme bu karede değeri veren tuş/çubuk; .device onun cihazı.
        InputControl kaynak = bakisEylemi.activeControl;
        bool cubuk = kaynak != null && kaynak.device is Gamepad;
        float temel = cubuk ? cubukHizi * Time.deltaTime : fareHassasiyeti;

        // Oyuncunun ayarı (#27): temel hassasiyetin çarpanı, Esc → Ayarlar'dan. İkisine de uygulanır.
        float hassasiyet = temel * Ayarlar.Hassasiyet;
        float yatay = fare.x * hassasiyet;
        float dikey = fare.y * hassasiyet;

        // "Fareyi ters çevir" açıksa yukarı-aşağı bakış yön değiştirir.
        if (Ayarlar.Ters) dikey = -dikey;

        // Sağa-sola bakış: GÖVDEYİ döndürürüz, çünkü yürüme yönü de dönmeli.
        transform.Rotate(Vector3.up * yatay);

        // Yukarı-aşağı bakış: sadece KAMERAYI döndürürüz. Gövde dik kalır.
        bakisAcisi -= dikey;   // eksi, çünkü fareyi yukarı itince yukarı bakılmalı

        // Clamp: değeri iki sınır arasında tutar. Boynun kopmasını engeller.
        bakisAcisi = Mathf.Clamp(bakisAcisi, -bakisSiniri, bakisSiniri);

        // localEulerAngles: kameranın ebeveynine göre açısı.
        kamera.localEulerAngles = new Vector3(bakisAcisi, 0f, 0f);
    }

    // ---------- YÜRÜYÜŞ ----------
    private void Yuru()
    {
        if (hareketEylemi == null) return;

        // Move eylemi Vector2 döndürür: x = sağ/sol, y = ileri/geri.
        Vector2 giris = OluBolgeUygula(hareketEylemi.ReadValue<Vector2>());

        // Girişi oyuncunun BAKTIĞI yöne çeviriyoruz.
        // transform.right = oyuncunun sağı, transform.forward = oyuncunun önü.
        Vector3 yon = transform.right * giris.x + transform.forward * giris.y;

        // Çapraz giderken hız artmasın diye uzunluğu 1'i geçmesin.
        if (yon.sqrMagnitude > 1f) yon.Normalize();

        // Yer çekimi. isGrounded = ayaklar bir yüzeye değiyor mu?
        if (kontrolcu.isGrounded && dikeyHiz < 0f)
        {
            // Yere değiyorken küçük bir aşağı kuvvet bırakırız.
            // Tam sıfır verirsek karakter eğimlerde ve basamaklarda zıplar.
            dikeyHiz = -2f;
        }
        else
        {
            // Havadayken her kare biraz daha hızlı düş.
            // deltaTime ile çarpmak şart: bir önceki kareden bu yana geçen süre.
            dikeyHiz += yercekimi * Time.deltaTime;
        }

        // Yatay hareket ile dikey düşmeyi tek bir harekette birleştir. Eğikken daha yavaş.
        Vector3 hareket = yon * (egik ? egikHiz : yurumeHizi);
        hareket.y = dikeyHiz;

        // Move: çarpışmaları hesaba katarak taşır. Duvardan geçmeyi engeller,
        // basamakları (stepOffset kadar) kendiliğinden tırmanır.
        kontrolcu.Move(hareket * Time.deltaTime);
    }

    // ---------- EĞİLME (#79) ----------
    // Klavyede Ctrl basılı tutuldukça eğik kalınır; oyun kolunda sağ çubuğa basmak eğilir/kalkar.
    // Eğilince iki şey değişir:
    //   1) Çarpışma kapsülü kısalır: oyuncu masanın altına girebilir, alçak yerlere sığar.
    //   2) Göz (kamera) aşağı iner: koltuğun, yatağın altı görünür, nişan da oradan alınır.
    // Kapsül hemen kısalır ama göz YUMUŞAKÇA iner; ani bir sıçrama gözü yorar.
    // Kalkarken önce yukarıda yer var mı bakılır: masanın altındaki oyuncu kafasını masaya
    // gömmeden, oradan çıkana kadar eğik kalır.
    private void Egil()
    {
        // Oyun kolu: sağ çubuğa basmak (R3) aç-kapa.
        if (Gamepad.current != null && Gamepad.current.rightStickButton.wasPressedThisFrame)
            koldanEgik = !koldanEgik;
        bool klavye = Keyboard.current != null &&
                      (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
        bool istek = klavye || koldanEgik;

        if (istek && !egik) BoyuAyarla(egikBoy, true);
        else if (!istek && egik && KalkacakYerVar()) BoyuAyarla(ayaktaBoy, false);

        // Göz yeni yüksekliğine yumuşakça yaklaşır. Lerp(a, b, t): a'dan b'ye t oranında git.
        // Her kare kalan yolun bir kısmını gitmek, sona yaklaştıkça yavaşlayan doğal bir hareket verir.
        if (kamera == null) return;
        float hedef = egik ? egikGoz : ayaktaGoz;
        Vector3 k = kamera.localPosition;
        k.y = Mathf.Lerp(k.y, hedef, 1f - Mathf.Exp(-egilmeGecisi * Time.deltaTime));
        kamera.localPosition = k;
    }

    // Kapsülün boyunu değiştirir. Ayaklar yerde kalsın diye merkez hep boyun yarısında.
    private void BoyuAyarla(float boy, bool egilsin)
    {
        kontrolcu.height = boy;
        kontrolcu.center = new Vector3(0f, boy / 2f, 0f);
        egik = egilsin;
    }

    // Ayağa kalkınca kafanın gireceği yer boş mu? Eğik kapsülün ÜSTÜNDE kalan, ayakta dururken
    // kaplanacak hacmi (OverlapCapsule) sorarız; oyuncunun kendi çarpıştırıcıları sayılmaz.
    // Alt taraf sorulmaz: merdivende bir üst basamak ayak hizasındadır ve kalkmayı engellememeli.
    private bool KalkacakYerVar()
    {
        float r = kontrolcu.radius - 0.02f;   // duvara sürtünen oyuncu kalkabilsin diye biraz ince
        Vector3 alt = transform.position + Vector3.up * (egikBoy - kontrolcu.radius);
        Vector3 ust = transform.position + Vector3.up * (ayaktaBoy - kontrolcu.radius);
        foreach (Collider c in Physics.OverlapCapsule(alt, ust, r, ~0, QueryTriggerInteraction.Ignore))
            if (!c.transform.IsChildOf(transform)) return false;
        return true;
    }

    // ---------- ÖLÜ BÖLGE ----------
    // Oyun kolunun çubuğu tam ortadayken bile (0.02, -0.04) gibi küçük değerler üretir.
    // Buna "kayma" (drift) denir ve karakteri kendi kendine yürütür.
    // Çözüm: belirli bir büyüklüğün altındaki girişi hiç yokmuş gibi saymak.
    private Vector2 OluBolgeUygula(Vector2 giris)
    {
        // magnitude = vektörün uzunluğu. Eksenleri ayrı ayrı değil,
        // toplam itiş miktarını ölçeriz; yoksa çapraz itişte eşik iki kat olur.
        if (giris.magnitude < oluBolge) return Vector2.zero;
        return giris;
    }

    // ---------- FARE KİLİDİ ----------
    private void FareyiKilitle(bool kilitli)
    {
        // Locked: imleç pencerenin ortasına yapışır ve görünmez olur.
        Cursor.lockState = kilitli ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !kilitli;
    }
}
