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
    private InputAction bakisEylemi;      // sürüklerken fare/çubuk hareketi buradan okunur (#88)

    // ---------- SÜRÜKLEME DURUMU (#88) ----------
    // E'ye basılınca hedef sürüklenebilir bir kapı ya da çekmeceyse hemen açılmaz: önce bakılır,
    // oyuncu E'yi tutup fareyi itiyor mu? 0,18 sn'den uzun tutulursa ya da fare 5 pikselden çok
    // oynarsa SÜRÜKLEME başlar (bakış kilitlenir, hareket kapıya gider); bırakınca kapı olduğu
    // yerde kalır. Daha önce bırakılırsa bu kısa bir DOKUNUŞTUR: eski davranış, tam aç/kapa.
    private const float SuruklemeSuresi = 0.18f, SuruklemePikseli = 5f;
    private const float DereceBasinaPiksel = 0.35f;   // kapı: 1 piksel = 0,35°
    private const float OranBasinaPiksel = 0.0025f;   // çekmece: 1 piksel = %0,25
    private const float KolKapiHizi = 110f, KolCekmeceHizi = 0.8f;   // kolda saniyede (tam itiş)
    private IEtkilesimli basiliHedef;
    private IcKapi basiliKapi;
    private AcilirParca basiliParca;
    private float basmaZamani;
    private Vector2 birikim;
    private bool surukluyor;
    private float suruklenen;   // bu tutuşta kapıya/çekmeceye gerçekten gidilen toplam hareket (piksel)

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
            bakisEylemi = varlik.FindAction("Player/Look");
        }

        kontrolcu = GetComponent<BirinciSahisKontrolcu>();
    }

    private void OnEnable() { etkilesimEylemi?.Enable(); }
    private void OnDisable()
    {
        etkilesimEylemi?.Disable();
        // Yakın ekran ya da karartma sırasında kapanırsak sürükleme yarıda kalır: kilidi bırak.
        if (surukluyor && kontrolcu != null) kontrolcu.BakisKilidi = false;
        basiliHedef = null; basiliKapi = null; basiliParca = null; surukluyor = false;
    }

    // ---------- HER KARE ----------

    private void Update()
    {
        if (kamera == null) return;

        // Sürükleme sürerken nişan değişmez: fare kapıdadır, kafada değil.
        if (basiliHedef != null) { SuruklemeyiSur(); return; }

        Hedef = HedefBul(out Transform hedefGovdesi);

        if (etkilesimEylemi == null) return;

        // İnceleme (#88): F (kolda X) bir eşyayı gözün önüne getirir.
        bool incele = Tuslar.Basildi(TusEylemi.Incele);   // #125: atanabilir (varsayılan R / X)
        if (incele && !IncelemeEkrani.Acik && Hedef is Esya esya && esya.EtkilesimeAcik)
        {
            IncelemeEkrani.Ac(esya, gameObject);
            return;
        }

        if (!etkilesimEylemi.WasPressedThisFrame()) return;
        if (Hedef == null || !Hedef.EtkilesimeAcik) return;

        // Sürüklenebilir kapı ya da çekmece: hemen değil, bırakınca ya da fare oynayınca karar.
        IcKapi kapi = Hedef as IcKapi;
        AcilirParca parca = Hedef as AcilirParca;
        if (parca == null && hedefGovdesi != null) parca = hedefGovdesi.GetComponent<AcilirParca>();
        if ((kapi != null && kapi.Suruklenebilir) || (parca != null && parca.Suruklenebilir))
        {
            basiliHedef = Hedef; basiliKapi = kapi; basiliParca = kapi != null ? null : parca;
            basmaZamani = Time.time; birikim = Vector2.zero; surukluyor = false; suruklenen = 0f;
            return;
        }

        Hedef.Etkilesim(gameObject);

        // Etkileşimden sonra hedef genelde kaybolur; nişangâh bu karede boşalsın.
        Hedef = null;
    }

    // E basılıyken: kısa dokunuş mu, sürükleme mi? Sürüklemede fare/çubuk hareketi kapıya gider.
    private void SuruklemeyiSur()
    {
        bool basili = etkilesimEylemi != null && etkilesimEylemi.IsPressed();
        Vector2 delta = bakisEylemi != null ? bakisEylemi.ReadValue<Vector2>() : Vector2.zero;
        bool kol = bakisEylemi != null && bakisEylemi.activeControl != null && bakisEylemi.activeControl.device is Gamepad;

        if (!surukluyor)
        {
            birikim += kol ? delta * 10f : delta;
            if (Time.time - basmaZamani > SuruklemeSuresi || birikim.magnitude > SuruklemePikseli)
            {
                surukluyor = true;
                if (kontrolcu != null) kontrolcu.BakisKilidi = true;
            }
        }
        if (surukluyor && kamera != null)
        {
            // Fare yukarı = oyuncudan uzağa it; işaret kapının açılış yönüne göre.
            float ileri = delta.y;
            suruklenen += Mathf.Abs(kol ? ileri * 10f : ileri);
            if (basiliKapi != null)
                basiliKapi.Surukle((kol ? ileri * KolKapiHizi * Time.deltaTime : ileri * DereceBasinaPiksel) * basiliKapi.AcilisIsareti(kamera.forward));
            else if (basiliParca != null)
                basiliParca.Surukle((kol ? ileri * KolCekmeceHizi * Time.deltaTime : ileri * OranBasinaPiksel) * basiliParca.AcilisIsareti(kamera.forward));
        }
        if (basili) return;

        // Bırakıldı. E uzun tutulsa bile fare oynamadıysa bu bir DOKUNUŞTUR: tam aç/kapa.
        if (kontrolcu != null) kontrolcu.BakisKilidi = false;
        if (surukluyor && suruklenen >= SuruklemePikseli)
        {
            basiliKapi?.SuruklemeBitti();
            basiliParca?.SuruklemeBitti();
        }
        else if (basiliHedef.EtkilesimeAcik)
        {
            basiliHedef.Etkilesim(gameObject);   // kısa dokunuş: eskisi gibi tam aç/kapa
        }
        basiliHedef = null; basiliKapi = null; basiliParca = null; surukluyor = false;
        Hedef = null;
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

        // KABIN İÇİNDEKİ ŞEY (#54, #104). Çekmecedeki anahtar, çekmecenin (ve komodinin) çarpışma
        // kutusunun İÇİNDE durur; en yakın çarpışma hep kabın kendisi olur ve anahtar hiç
        // seçilemezdi. Kural: bir içeriğin önündeki bütün çarpışmalar onu içinde taşıyan
        // kaplarsa (içerik onların çocuğuysa) içerik seçilir. Arada kapalı bir kapak ya da kapı
        // varsa (onlar içeriğin ebeveyni değil) seçilmez; araya duvar girerse de seçilmez.
        // #104: kural eskiden yalnızca EŞYA (Esya) içindi; çekmecedeki NOT eşya olmadığı için
        // nişan çekmeceyi seçiyor ve E çekmeceyi kapatıyordu (Şahan). Şimdi kabın içindeki her
        // etkileşim (not, fotoğraf, tabak yuvası, kavanoz...) kabın kendisinden önce gelir.
        // İnce ışın (yarıçapsız): bir şeyin içeriğin ÖNÜNÜ kapatıp kapatmadığına bununla bakılır. Küre
        // 6 cm geniş olduğu için yoldaki başka şeylere sürter (açık çekmecedeki nota bakarken alttaki
        // kapalı çekmecenin ön yüzüne değiyordu) ve not "önü kapalı" sayılıp eleniyordu.
        RaycastHit[] inceCarpmalar = Physics.RaycastAll(isin, menzil);
        IEtkilesimli bulunan = IctekiniBul(carpmalar, inceCarpmalar);

        // AÇIK KAP NİŞANI AFFEDER (#104). Nişan tam notun üstünde değilse küre notun çarpışmasına
        // değmez; en yakın çarpışma açık çekmece olur ve E onu KAPATIR. Oyuncu açık bir çekmeceye
        // bakarken niyeti neredeyse hep içindekidir: o durumda iki kat geniş bir küreyle içerik yeniden
        // aranır. Çekmeceyi kapatmak için ön yüzüne, içindekinin olmadığı yere bakmak yeter.
        if (bulunan == null && Kullanilabilir(enYakin) is AcilirParca acikKap && acikKap.Acik)
            bulunan = IctekiniBul(Physics.SphereCastAll(isin, NisanYaricapi * 2.5f, menzil), inceCarpmalar);

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

    // Çarpışmalar (küre) arasında, bakış çizgisinin (ince ışın) önünde yalnızca kendi kapları olan en yakın
    // İÇERİĞİ bulur (yoksa null). Küre içeriği yakalar; önü kapatanı ince ışın belirler.
    private IEtkilesimli IctekiniBul(RaycastHit[] carpmalar, RaycastHit[] inceCarpmalar)
    {
        Collider icteki = null;
        float icMesafe = float.MaxValue;
        foreach (RaycastHit h in carpmalar)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            MonoBehaviour icerik = Icerik(h.collider);
            if (icerik == null || h.distance >= icMesafe) continue;
            bool onundekilerKap = true;
            foreach (RaycastHit o in inceCarpmalar)
            {
                if (o.collider.transform.IsChildOf(transform) || o.distance >= h.distance) continue;
                if (!icerik.transform.IsChildOf(o.collider.transform) && !IcindeMi(h.collider, o.collider)) { onundekilerKap = false; break; }
            }
            if (onundekilerKap) { icteki = h.collider; icMesafe = h.distance; }
        }
        return icteki != null ? Kullanilabilir(icteki) : null;
    }

    // İÇERİK (#104): bu çarpışma, bir kabın içinde duran ve kendi etkileşimi olan bir şeye mi ait
    // (eşya, not, fotoğraf, tabak yuvası, kavanoz, radyo)? Kapların kendisi (çekmece/kapak/kapı, oda kapısı,
    // gövdeden parçaya yönlendiren) içerik DEĞİLDİR. Çarpışmadan yukarı çıkarken ilk bulunan etkileşim bir
    // kapsa, bu çarpışma kabın kendisidir; içerik değilse ya da şu an yapılamıyorsa null.
    private static MonoBehaviour Icerik(Collider c)
    {
        foreach (IEtkilesimli e in c.GetComponentsInParent<IEtkilesimli>())
        {
            if (e is AcilirParca || e is Yonlendirici || e is IcKapi || e is SaklanmaYeri) return null;
            return e.EtkilesimeAcik ? e as MonoBehaviour : null;
        }
        return null;
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

    // ---------- NİŞANGÂH (#72, simge #104) ----------
    // Ekranın tam ortasında küçük, yuvarlak, beyaz bir halka. Baktığın şeyle etkileşime
    // girebiliyorsan (eşya, kapı, çekmece, not...) halkanın içi kırmızıyla dolar, büyür ve ortasında
    // E'nin NE YAPACAĞINI söyleyen küçük beyaz bir simge belirir: el (al), göz (oku), ok (aç / kapat),
    // kilit, anahtar, kol, dolap. Yazı yine yok (#72); simge, çekmeceyi kapatmakla içindekini almayı
    // basmadan önce ayırt ettirir (Şahan'ın çekmece şikâyeti).
    //
    // OnGUI: Unity'nin ekrana düz (iki boyutlu) çizim yaptığı fonksiyon; her karede çağrılır.
    // Bütün arayüz gibi sanal 1080 piksel yüksekliğindeki ekrana çizilir (ArayuzOlcegi, #34):
    // halka her çözünürlükte ekranın aynı oranını kaplar.
    private const float NisanBoyu = 14f;          // halkanın çapı (sanal piksel)
    private const float DoluBoyu = 36f;           // hedef varken (simge sığacak kadar)
    private const float SimgeBoyu = 26f;          // simgenin kenarı (16 piksellik desen ~1,6 kat)
    private static readonly Color HalkaRengi = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color DolguRengi = new Color(0.86f, 0.08f, 0.06f, 0.95f);
    private static Texture2D halkaDokusu, diskDokusu;
    private static Texture2D[] simgeDokulari;     // NisanSimgesi sırasıyla

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
        float ox = ArayuzOlcegi.Genislik / 2f, oy = ArayuzOlcegi.Yukseklik / 2f;
        Rect alan = new Rect(ox - boy / 2f, oy - boy / 2f, boy, boy);
        if (hedefVar)
        {
            GUI.color = DolguRengi;
            GUI.DrawTexture(alan, diskDokusu);
        }
        GUI.color = HalkaRengi;
        GUI.DrawTexture(alan, halkaDokusu);
        if (hedefVar)
        {
            // Simge: hedefin söylediği işaret, kırmızı diskin ortasında beyaz.
            Texture2D simge = simgeDokulari[(int)Hedef.Simge];
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(ox - SimgeBoyu / 2f, oy - SimgeBoyu / 2f, SimgeBoyu, SimgeBoyu), simge);
        }
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
        SimgeleriUret();
    }

    // ---------- SİMGELER (#104) ----------
    // Her simge 16 × 16'lık bir desen: '#' dolu, '.' boş. Desen metin olarak burada durur; bir dosyaya
    // gerek yok (build'e girmesi dert olmaz) ve bir simgeyi değiştirmek metni düzeltmekten ibaret.
    // Sıra NisanSimgesi'ndeki sırayla AYNI olmalı: El, Goz, Ac, Kapat, Kilitli, Anahtar, Kol, Saklan,
    // Sifre, Agir, Alet (son üçü #114).
    // (#114: önceden şifreli kilit, ağır gardırop ve aletle sökülecek kapı hepsi "Kilitli" asma kilidini
    // gösteriyordu; oyuncu 5. katın gardırobunun neden kıpırdamadığını anlayamıyordu.)
    private static readonly string[][] SimgeDesenleri =
    {
        new[] {   // EL: işaret parmağı kalkık bir el
            "......##........",
            ".....#..#.......",
            ".....#..#.......",
            ".....#..#.......",
            ".....#..####....",
            ".....#..#..##...",
            ".....#..#..#.##.",
            "..##.#..#..#..#.",
            ".#..##........#.",
            ".#............#.",
            "..#...........#.",
            "..#..........#..",
            "...#.........#..",
            "...#........#...",
            "....########....",
            "................",
        },
        new[] {   // GÖZ
            "................",
            "................",
            "................",
            "......####......",
            "....##....##....",
            "..##...##...##..",
            ".#....####....#.",
            "#....######....#",
            "#....######....#",
            ".#....####....#.",
            "..##...##...##..",
            "....##....##....",
            "......####......",
            "................",
            "................",
            "................",
        },
        new[] {   // AÇ: çubuktan dışarı çıkan ok (çekmece dışarı)
            "................",
            ".##.............",
            ".##.............",
            ".##......#......",
            ".##......##.....",
            ".##.......##....",
            ".##........##...",
            ".##.##########..",
            ".##.##########..",
            ".##........##...",
            ".##.......##....",
            ".##......##.....",
            ".##......#......",
            ".##.............",
            ".##.............",
            "................",
        },
        new[] {   // KAPAT: çubuğa doğru giden ok (çekmece içeri)
            "................",
            ".............##.",
            ".............##.",
            "......#......##.",
            "......##.....##.",
            ".......##....##.",
            "........##...##.",
            "..##########.##.",
            "..##########.##.",
            "........##...##.",
            ".......##....##.",
            "......##.....##.",
            "......#......##.",
            ".............##.",
            ".............##.",
            "................",
        },
        new[] {   // KİLİTLİ: asma kilit
            "................",
            ".....######.....",
            "....##....##....",
            "....#......#....",
            "....#......#....",
            "....#......#....",
            "..############..",
            "..############..",
            "..####....####..",
            "..####....####..",
            "..#####..#####..",
            "..############..",
            "..############..",
            "..############..",
            "...##########...",
            "................",
        },
        new[] {   // ANAHTAR
            "................",
            ".....####.......",
            "....#....#......",
            "...#......#.....",
            "...#......#.....",
            "....#....#......",
            ".....####.......",
            ".......##.......",
            ".......##.......",
            ".......##.......",
            ".......####.....",
            ".......##.......",
            ".......####.....",
            ".......##.......",
            "................",
            "................",
        },
        new[] {   // KOL: tabanından yükselen eğik kol ve topuzu
            "................",
            "...........##...",
            "..........####..",
            "..........####..",
            ".........##.##..",
            "........##......",
            ".......##.......",
            "......##........",
            ".....##.........",
            "....##..........",
            "...##...........",
            "..##............",
            ".############...",
            ".############...",
            "................",
            "................",
        },
        new[] {   // SAKLAN: iki kapaklı dolap
            "................",
            "..###########...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#..#.#.#..#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..#....#....#...",
            "..###########...",
            "..#.........#...",
            "................",
        },
        new[] {   // ŞİFRE: asma kilit, gövdesinde çevirme diski (halka + ortada topuz). İlk çizim (üç çark ızgarası)
                  // küçükken takvime benziyordu.
            "................",
            ".....######.....",
            "....##....##....",
            "....#......#....",
            "....#......#....",
            "..############..",
            "..#####..#####..",
            "..###......###..",
            "..##...##...##..",
            "..##..####..##..",
            "..##..####..##..",
            "..##...##...##..",
            "..###......###..",
            "..#####..#####..",
            "...##########...",
            "................",
        },
        new[] {   // AĞIR: halteri andıran ağırlık (yerinden oynamıyor, ekipman gerek)
            "................",
            "................",
            "......####......",
            ".....#....#.....",
            ".....#....#.....",
            "......####......",
            "....########....",
            "...##########...",
            "..############..",
            "..############..",
            ".##############.",
            ".##############.",
            ".##############.",
            ".##############.",
            "................",
            "................",
        },
        new[] {   // ALET: dik duran tornavida, altta kalın sap, üstte ince uç. Eğik çizimler küçük boyda
                  // "giriş yasak" işaretine benziyordu (kırmızı daire + çapraz çizgi).
            "................",
            ".......##.......",
            ".......##.......",
            ".......##.......",
            ".......##.......",
            ".......##.......",
            "......####......",
            ".....######.....",
            ".....######.....",
            ".....##..##.....",
            ".....######.....",
            ".....##..##.....",
            ".....######.....",
            "......####......",
            "................",
            "................",
        },
    };

    // Desenleri dokuya çevirir. Desenin ilk satırı dokunun ÜSTÜ olmalı: Unity dokularında y = 0 alt
    // satırdır, bu yüzden satır sırası ters çevrilerek yazılır. Point süzme: küçük desen bulanmasın.
    private static void SimgeleriUret()
    {
        simgeDokulari = new Texture2D[SimgeDesenleri.Length];
        for (int s = 0; s < SimgeDesenleri.Length; s++)
        {
            string[] desen = SimgeDesenleri[s];
            var doku = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    doku.SetPixel(x, 15 - y, desen[y][x] == '#' ? Color.white : Color.clear);
            doku.filterMode = FilterMode.Point;
            doku.Apply();
            simgeDokulari[s] = doku;
        }
    }
}
