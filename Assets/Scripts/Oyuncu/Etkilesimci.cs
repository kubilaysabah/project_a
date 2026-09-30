// Etkilesimci.cs
// Oyuncunun baktığı şeyle etkileşim kurmasını sağlayan tek merkez.
//
// Üç iş yapar:
//   1. Her karede oyuncunun neye baktığını bulur.
//   2. Ekranın ortasına yuvarlak beyaz bir nişangâh çizer; baktığı şey etkileşime açıksa
//      nişangâhın içi kırmızıyla dolar (#72, Şahan: No70'teki gibi). Eskiden hedefin üstünde
//      "TOPLA (E)" gibi bir yazı paneli çıkıyordu; Şahan yazıları kaldırttı, eylem aynen duruyor.
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
    [SerializeField] private float menzil = 2.8f;   // #61: 2,2'den (Şahan: nesnelere uzanmak zordu)

    // Nişan ışını ince bir KÜRE olarak taranır (#61): anahtar, buşon gibi küçük eşyaları tam
    // ortalamadan da yakalar. 6 cm: yakındaki duvarın kenarına takılmayacak kadar ince.
    private const float NisanYaricapi = 0.06f;

    [Tooltip("Nişan alınan kamera. Boş bırakılırsa çocuklardan bulunur.")]
    [SerializeField] private Transform kamera;

    // Şu anda nişan alınan nesne. Başka scriptler (test göstergesi) okuyabilir.
    public IEtkilesimli Hedef { get; private set; }

    private InputAction etkilesimEylemi;

    // Nişangâhı çizmeden önce bakılır: oyuncu yürüyemiyorsa (duraklatma, kararma) çizilmez.
    private BirinciSahisKontrolcu kontrolcu;

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

        kontrolcu = GetComponent<BirinciSahisKontrolcu>();
    }

    private void OnEnable() { etkilesimEylemi?.Enable(); }
    private void OnDisable() { etkilesimEylemi?.Disable(); }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (kamera == null) return;

        Hedef = HedefBul(out Transform hedefGovdesi);

        if (etkilesimEylemi == null) return;
        if (!etkilesimEylemi.WasPressedThisFrame()) return;

        if (Hedef != null && Hedef.EtkilesimeAcik)
        {
            Hedef.Etkilesim(gameObject);

            // Etkileşimden sonra hedef genelde kaybolur; nişangâh bu karede boşalsın.
            Hedef = null;
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
        // SphereCastAll: ışın yerine verilen yarıçapta bir küre gönderir; yol boyunca değdiği
        // bütün çarpışmaları döndürür (başlangıçta küreyle iç içe olanlar 0 uzaklıkla gelir;
        // oyuncunun kendi kapsülü de onlardan, aşağıda eleniyor).
        RaycastHit[] carpmalar = Physics.SphereCastAll(isin, NisanYaricapi, menzil);

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

        // KABIN İÇİNDEKİ EŞYA (#54). Çekmecedeki anahtar, çekmecenin (ve komodinin) çarpışma
        // kutusunun İÇİNDE durur; en yakın çarpışma hep kabın kendisi olur ve anahtar hiç
        // seçilemezdi. Kural: bir eşyanın önündeki bütün çarpışmalar onu içinde taşıyan
        // kaplarsa (eşya onların çocuğuysa) eşya seçilir. Arada kapalı bir kapak ya da kapı
        // varsa (onlar eşyanın ebeveyni değil) seçilmez; araya duvar girerse de seçilmez.
        Collider icteki = null;
        float icMesafe = float.MaxValue;
        foreach (RaycastHit h in carpmalar)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            Esya esya = h.collider.GetComponentInParent<Esya>();
            if (esya == null || !esya.EtkilesimeAcik || h.distance >= icMesafe) continue;
            bool onundekilerKap = true;
            foreach (RaycastHit o in carpmalar)
            {
                if (o.collider.transform.IsChildOf(transform) || o.distance >= h.distance) continue;
                if (!esya.transform.IsChildOf(o.collider.transform) && !IcindeMi(h.collider, o.collider)) { onundekilerKap = false; break; }
            }
            if (onundekilerKap) { icteki = h.collider; icMesafe = h.distance; }
        }
        IEtkilesimli bulunan = icteki != null ? Kullanilabilir(icteki) : null;

        // KABIN İÇİNDEKİ NESNE (#61). En yakın çarpışmanın kendi etkileşimi yoksa ama arkasında
        // onun ÇOCUĞU olan bir çarpışma varsa (tezgâhın üstündeki kavanoz; tezgâhın çarpışma
        // kutusu musluğu da kapsadığı için kavanoz onun içinde kalıyor), içeri bakmaya devam
        // edilir. İçinde bir şey olmayan engel (duvar) nişanı durdurur.
        if (bulunan == null)
        {
            var sirali = new System.Collections.Generic.List<RaycastHit>();
            foreach (RaycastHit h in carpmalar) if (!h.collider.transform.IsChildOf(transform)) sirali.Add(h);
            // Sort: listeyi uzaklığa göre (yakından uzağa) dizer.
            sirali.Sort((x, y) => x.distance.CompareTo(y.distance));
            for (int i = 0; i < sirali.Count && bulunan == null; i++)
            {
                Collider c = sirali[i].collider;
                // Gövdeden parçaya yönlendiren bileşenlere nişanın değdiği noktayı bildir (#83): en yakın
                // parçayı seçsinler. Uzaklık 0 ise küre gövdenin içinden başladı, nokta anlamsız.
                foreach (Yonlendirici yon in c.GetComponentsInParent<Yonlendirici>())
                    yon.NoktaAyarla(sirali[i].point, sirali[i].distance > 0f);
                bulunan = Kullanilabilir(c);
                if (bulunan != null) break;
                bool icindeBirSeyVar = false;
                for (int j = i + 1; j < sirali.Count; j++)
                    if (sirali[j].collider.transform.IsChildOf(c.transform) || IcindeMi(sirali[j].collider, c)) { icindeBirSeyVar = true; break; }
                if (!icindeBirSeyVar) break;   // duvar gibi: arkasına bakılmaz
            }
        }

        if (bulunan == null) return null;

        // Sözleşmeyi imzalayan her nesne aslında bir MonoBehaviour; gövdesini de döndürüyoruz.
        // (Eskiden "TOPLA (E)" yazısı onun üstüne konuyordu; #72'den beri nişangâh ekranın
        // ortasında. Gövde, ileride hedefi vurgulamak isteyen bir özellik için duruyor.)
        MonoBehaviour bilesen = bulunan as MonoBehaviour;
        if (bilesen != null) govde = bilesen.transform;
        // Yönlendirici ise gövde, yönlendirilen parça (kapak) olsun.
        if (bulunan is Yonlendirici y) govde = y.HedefGovdesi;

        return bulunan;
    }

    // İÇİNDE Mİ (#83): "ic" çarpışmasının ortası "kab" çarpışmasının kutusunun (bounds) içinde mi? Eşya bir
    // mobilyanın çocuğu olmadan da onun çarpışma kutusunun içinde durabilir: tamir tezgâhının kutusu arkadaki pano ve
    // radyo yüzünden üstündeki tenekeyi, çalışma masasının kutusu lamba yüzünden masadaki anahtarı içine alıyordu
    // ve nişan tezgâhta takılıp kalıyordu (görüş testi, 2026-09-30). Böyle bir kutu da kap sayılır. Duvar ve kapı
    // kanadı ince olduğu için arkalarındaki eşyayı içine almaz; kapalı kapak ve kapı eskisi gibi nişanı durdurur.
    // bounds: çarpışmanın dünya eksenlerindeki sınır kutusu; Contains bir noktanın onun içinde olup olmadığını söyler.
    private static bool IcindeMi(Collider ic, Collider kab)
    {
        return kab.bounds.Contains(ic.bounds.center);
    }

    // Bir çarpışmanın şu an yapılabilir etkileşimi. GetComponentsInParent: nesne birkaç
    // parçadan oluşuyorsa üst parçalarda da arar ve BÜTÜN etkileşimleri verir (en yakından
    // uzağa); yapılabilir olan ilki seçilir (#61). Bir gövdede hem sigorta yuvası hem kapağa
    // yönlendirici olabilir: elde buşon yokken yuva "yapılamaz" der, sıra yönlendiriciye gelir
    // ve kapağın "AÇ"ı görünür.
    private static IEtkilesimli Kullanilabilir(Collider c)
    {
        foreach (IEtkilesimli e in c.GetComponentsInParent<IEtkilesimli>())
            if (e.EtkilesimeAcik) return e;
        return null;
    }

    // ---------- İPUCU ----------

    // ---------- NİŞANGÂH (#72) ----------
    // Ekranın tam ortasında küçük, yuvarlak, beyaz bir halka. Baktığın şeyle etkileşime
    // girebiliyorsan (eşya, kapı, çekmece, not...) halkanın içi kırmızıyla dolar ve biraz büyür.
    // Hangi eylemin yapılacağı yazmaz (AÇ, AL, KİLİTLİ); oyuncu dener ve görür.
    //
    // OnGUI: Unity'nin ekrana düz (iki boyutlu) çizim yaptığı fonksiyon; her karede çağrılır.
    // Bütün arayüz gibi sanal 1080 piksel yüksekliğindeki ekrana çizilir (ArayuzOlcegi, #34):
    // halka her çözünürlükte ekranın aynı oranını kaplar.
    private const float NisanBoyu = 14f;          // halkanın çapı (sanal piksel)
    private const float DoluBoyu = 20f;           // hedef varken
    private static readonly Color HalkaRengi = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color DolguRengi = new Color(0.86f, 0.08f, 0.06f, 0.95f);
    private static Texture2D halkaDokusu, diskDokusu;

    private void OnGUI()
    {
        // Başlık ekranında, duraklatmada (zaman durur) ve oyuncu yürüyemezken (not, şifre ekranı,
        // yakalanınca kararma) nişangâh çizilmez. Not ve şifre ekranı bu bileşeni zaten kapatır.
        if (DuraklatmaMenusu.BaslikEkraninda || Time.timeScale == 0f) return;
        if (kontrolcu != null && !kontrolcu.enabled) return;
        if (Event.current.type != EventType.Repaint) return;   // yalnızca çizim olayında çiz
        ArayuzOlcegi.Uygula();
        if (halkaDokusu == null) DokulariUret();

        bool hedefVar = Hedef != null && Hedef.EtkilesimeAcik;
        float boy = hedefVar ? DoluBoyu : NisanBoyu;
        Rect alan = new Rect(ArayuzOlcegi.Genislik / 2f - boy / 2f, ArayuzOlcegi.Yukseklik / 2f - boy / 2f, boy, boy);
        if (hedefVar)
        {
            GUI.color = DolguRengi;
            GUI.DrawTexture(alan, diskDokusu);
        }
        GUI.color = HalkaRengi;
        GUI.DrawTexture(alan, halkaDokusu);
        GUI.color = Color.white;
    }

    // Halka ve disk dokuları kodla üretilir (dosya gerekmez, build'e girmesi dert olmaz).
    // 64 × 64 piksel; her pikselin saydamlığı merkeze uzaklığına göre hesaplanır. Kenarlarda
    // bir piksellik yumuşak geçiş (Clamp01) halkanın küçültülünce tırtıklı görünmesini önler.
    private static void DokulariUret()
    {
        const int N = 64;
        const float Dis = 30f, Ic = 24f;              // halkanın dış ve iç yarıçapı (piksel)
        halkaDokusu = new Texture2D(N, N, TextureFormat.RGBA32, false);
        diskDokusu = new Texture2D(N, N, TextureFormat.RGBA32, false);
        float merkez = (N - 1) / 2f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = Mathf.Sqrt((x - merkez) * (x - merkez) + (y - merkez) * (y - merkez));
                // Halka: dış kenarın içinde VE iç kenarın dışında.
                float halka = Mathf.Clamp01(Dis + 0.5f - d) * Mathf.Clamp01(d - (Ic - 0.5f));
                // Disk: iç kenarın içi (halkanın boşluğunu doldurur).
                float disk = Mathf.Clamp01(Ic + 0.5f - d);
                halkaDokusu.SetPixel(x, y, new Color(1f, 1f, 1f, halka));
                diskDokusu.SetPixel(x, y, new Color(1f, 1f, 1f, disk));
            }
        halkaDokusu.filterMode = FilterMode.Bilinear;
        diskDokusu.filterMode = FilterMode.Bilinear;
        halkaDokusu.Apply();
        diskDokusu.Apply();
    }
}
