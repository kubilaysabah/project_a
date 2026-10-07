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
//     (#76, Şahan: ilk hâli F'ydi; "lamba" demek olan L'ye taşındı. #119, Şahan: yeniden F; inceleme R oldu.)
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
    // #120: menzil 10 → 15 m. URP spot ışığı menzile yaklaşırken yumuşakça sıfıra iner ((1-(d/r)^4)^2):
    // 10 m menzilde 7 m'deki daire kapısına ışığın yalnızca %58'i ulaşıyordu, ters kareyle birlikte %4; gece
    // post-processing'i 0,2 altını eziyor, sahanlık kapısı okunmuyordu. 15 m'de 7 m'de %91 kalır.
    public static float Menzil = 15f;
    // #120 UZAK TELAFİSİ: huzmenin ortası uzağa vuruyorsa fener güçlenir. Gerçek fener böyle yapmaz ama
    // göz yapar (uzağa bakınca iris açılır); No70 ve Chilla's Art fenerleri 8-10 m'yi okutur. Yakın duvar
    // (UzakBaslangic altı) etkilenmez: antre, çekmece, not eskisi gibi. Çarpan 3 m'de 1, 8 m'de 1+UzakKazanc.
    public static float UzakBaslangic = 3f, UzakTam = 8f, UzakKazanc = 1.6f;
    private const float UzakGecisHizi = 4f;         // çarpanın saniyede en çok ne kadar değiştiği (0,25 sn'de tam yol)
    public const float DisAci = 64f, IcAci = 26f;   // dış koni ve tam şiddet konisi (yumuşak kenar)
    public static readonly Color Renk = new Color(1f, 0.90f, 0.72f);   // sıcak beyaz, ampullerden biraz beyaz

    private Light isik;
    private AudioSource kaynak;
    private AudioClip tik;

    // ---------- GERİLİM GÖSTERGESİ (#105) ----------
    // Kural LambaGerilimKurali'nde (saf C#). Burada yalnızca uygulanır: ışığın rengi sıcak beyazdan
    // SariRenk'e kayar, gücü düşer, ince bir vınlama yükselir. Değerler hedefe yumuşakça gider
    // (saniyede ~0,6): av bitip gerilim sıfırlanınca lamba bir anda değil, iki saniyede toparlar.
    public static readonly Color SariRenk = new Color(1f, 0.93f, 0.45f);   // sodyum lambası sarısı
    private const float GostergeHizi = 0.6f;    // karışımın saniyede en çok ne kadar değiştiği
    private const float VinlamaTavani = 0.22f;  // vınlamanın en yüksek ses gücü (kalp atışının altında)
    private float karisim;                      // şu anki sararma (0-1)
    private float incelemeCarpani = 1f;         // inceleme kısması: %12 (IncelemeKismasi)
    private AudioSource vinlama;
    private bool acik = true;
    private bool takili = true;   // lamba oyuncunun kafasında mı (açılışta çividen alınana kadar değil)
    private Coroutine titreme;
    private float sonmeBitis = -1f;   // senaryolu sönmenin bittiği an (Time.time); geçmişse sönme yok (#119)

    /// <summary>Lamba oyuncunun kafasında mı? Açılışta çividen alınana kadar false (#56).</summary>
    public bool Takili { get { return takili; } }

    // ---------- KAYNAK KAFANIN GERİSİNDE (#110) ----------
    // Tam testte fener yakındaki her şeyi bembeyaz yakıyordu (kapı kasası, 50 cm'deki duvar), oda içi ise
    // kapkaraydı. Sebep fizik: ışık uzaklığın KARESİYLE azalır; kaynak gözün dibindeyken 0,5 m'deki duvar
    // 3 m'deki eşyadan 36 kat parlak olur. Kaynağı kafanın 45 cm gerisine almak oranı 36'dan 12'ye indirir
    // (0,95 m'ye karşı 3,45 m). Gücü de 3 m'de eskisiyle aynı parlaklığı verecek kadar artırırız (telafi):
    // uzak aynı kalır, yakın üç kat sönükleşir. Gerçek bir kafa lambası da gözün değil alnın üstündedir.
    // Arkada duvar varsa kaynak duvarın 10 cm önünde durur (duvarın içine girerse bütün sahneyi gölgede bırakırdı).
    public const float GeriAlma = 0.45f;          // kaynağın kameranın ne kadar gerisinde durduğu (metre)
    private const float TelafiMesafesi = 3f;      // gücün eskisiyle eşit olduğu uzaklık (metre)
    private Vector3 temelKonum;                   // ışığın kameraya göre asıl yeri (alnın üstü)
    /// <summary>Kaynağın şu an kameranın ne kadar gerisinde olduğu (arkada duvar varsa daha az).</summary>
    public float GeriAlinan { get; private set; }
    /// <summary>Uzaklık telafisi: 3 m'deki parlaklık eskisiyle aynı kalsın diye güç bu kadar artar.</summary>
    /// <summary>Uzak telafisinin o anki çarpanı (1 = yakın yüzey; #120). FenerSekmesi ve testler okur.</summary>
    public float UzakCarpani { get; private set; } = 1f;

    public float TelafiCarpani
    {
        get { float k = (TelafiMesafesi + GeriAlinan) / TelafiMesafesi; return k * k; }
    }

    /// <summary>
    /// İnceleme kısması (#88, IncelemeEkrani): eşya kameranın 42 cm önündeyken tam şiddetli lamba onu
    /// bembeyaz patlatır (ışık yakında kare-ters orantıyla kat kat güçlenir). Açıkken şiddet %12'ye
    /// iner: eşya okunur, oda arkada karanlığa çekilir; kapanınca eski şiddet.
    /// </summary>
    public void IncelemeKismasi(bool acik)
    {
        incelemeCarpani = acik ? 0.12f : 1f;
        SiddetiUygula();
    }

    // Işığın gücü: temel şiddet × inceleme kısması × gerilim çarpanı (#105).
    private void SiddetiUygula()
    {
        if (isik != null) isik.intensity = Siddet * incelemeCarpani * LambaGerilimKurali.GucCarpani(GerilimDegeri());
    }

    // Binanın gerilimi (0-1). Varlık yoksa ya da testler için kapalıysa 0: lamba normal.
    private static float GerilimDegeri()
    {
        return Varlik.Ornek != null && !Varlik.Kapali ? Varlik.Ornek.Gerilim : 0f;
    }

    /// <summary>Lamba şu an yanıyor mu (senaryolu sönme sırasında da false).</summary>
    public bool Acik { get { return acik; } }

    /// <summary>
    /// Işık şu an gerçekten yanıyor mu? Acik'ten farkı: senaryolu titremede ışık bir yanıp bir
    /// söner, Acik ise değişmez. Eşyanın parıltısı (FenerParlamasi) ışığın kendisine bakar.
    /// </summary>
    public bool IsikYaniyor { get { return takili && isik != null && isik.enabled; } }

    /// <summary>Işığın konumu ve baktığı yön (ışık yoksa lambanın kendisi).</summary>
    public Transform IsikYonu { get { return isik != null ? isik.transform : transform; } }

    private void Awake()
    {
        isik = GetComponentInChildren<Light>(true);
        if (isik != null) temelKonum = isik.transform.localPosition;
        // Ses kaynağı kameranın kendisine değil bir ALT nesneye konur (#87/#91): kamerada oda yankısı
        // filtresi (OdaYankisi) var ve Unity, aynı nesnede bir ses kaynağı bulunca filtreyi dinleyiciye değil
        // o kaynağa bağlar; yankı odadaki seslere değil lambanın tıkına uygulanırdı.
        GameObject sesNesnesi = new GameObject("Lamba_Sesi");
        sesNesnesi.transform.SetParent(transform, false);
        kaynak = sesNesnesi.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;             // kendi kafandaki lamba: 2B
        tik = Resources.Load<AudioClip>("Sesler/Lamba_Sonme");   // eski anahtar tıkı (#25'teki ses)

        // Vınlama (#105): kendi kaynağı, döngülü, 2B, oda yankısından muaf (lamba kafada, odada değil).
        GameObject vinNesnesi = new GameObject("Lamba_Vinlama");
        vinNesnesi.transform.SetParent(transform, false);
        vinlama = vinNesnesi.AddComponent<AudioSource>();
        vinlama.clip = Resources.Load<AudioClip>("Sesler/Lamba_Vinlama");
        vinlama.loop = true;
        vinlama.playOnAwake = false;
        vinlama.spatialBlend = 0f;
        vinlama.bypassListenerEffects = true;
        vinlama.volume = 0f;
    }

    // Gerilim göstergesi her karede (#105): renk, güç ve vınlama hedefe yumuşakça yaklaşır.
    private void LateUpdate()
    {
        if (isik == null) return;
        KaynagiGeriAl();
        float hedef = LambaGerilimKurali.Karisim(GerilimDegeri());
        // MoveTowards: değeri hedefe en fazla "adım" kadar yaklaştırır (ani sıçrama olmaz).
        karisim = Mathf.MoveTowards(karisim, hedef, GostergeHizi * Time.deltaTime);
        isik.color = Color.Lerp(Renk, SariRenk, karisim);
        // Güç: karışımla aynı eğride, ama çarpan olarak (kural güç çarpanını gerilimden verir; burada
        // yumuşatılmış karışımdan türetilir ki renk ve güç birlikte hareket etsin).
        UzakTelafisiniGuncelle();
        isik.intensity = Siddet * TelafiCarpani * UzakCarpani * incelemeCarpani * (1f - (1f - LambaGerilimKurali.EnDusukGuc) * karisim);

        if (vinlama == null || vinlama.clip == null) return;
        // Vınlama yalnızca lamba yanarken: sesi çıkaran lambanın kendisi. Menüde (zaman durur) kısılır.
        float vinHedef = IsikYaniyor && Time.timeScale > 0f ? LambaGerilimKurali.Vinlama(GerilimDegeri()) * VinlamaTavani : 0f;
        vinlama.volume = Mathf.MoveTowards(vinlama.volume, vinHedef, 0.3f * Time.deltaTime);
        vinlama.pitch = LambaGerilimKurali.VinlamaPerdesi(GerilimDegeri());
        if (vinlama.volume > 0.001f && !vinlama.isPlaying) vinlama.Play();
        else if (vinlama.volume <= 0.001f && vinlama.isPlaying) vinlama.Stop();
    }

    // Uzak telafisi (#120): huzmenin ortasındaki ilk engelin uzaklığı (oyuncunun kendi parçaları atlanır;
    // engel yoksa "uzak" sayılır). Çarpan hedefe yumuşakça gider ki kapı pervazından duvara bakarken titremesin.
    private void UzakTelafisiniGuncelle()
    {
        float uzaklik = UzakTam;
        Transform kok = transform.root;
        foreach (RaycastHit h in Physics.RaycastAll(isik.transform.position, isik.transform.forward, Menzil, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(kok)) continue;
            uzaklik = Mathf.Min(uzaklik, h.distance - GeriAlinan);
        }
        float hedef = 1f + UzakKazanc * Mathf.Clamp01(Mathf.InverseLerp(UzakBaslangic, UzakTam, uzaklik));
        UzakCarpani = Mathf.MoveTowards(UzakCarpani, hedef, UzakGecisHizi * Time.deltaTime);
    }

    // Kaynağı kameranın gerisine alır (#110). Arkaya kısa bir ışın atılır; oyuncunun kendi parçaları (kamera
    // kapsülün içinde) atlanır, ilk engelin 10 cm önünde durulur. Menzil de geri alınan kadar uzar ki ışık
    // eskisi kadar uzağa ulaşsın.
    private void KaynagiGeriAl()
    {
        float geri = GeriAlma;
        Transform kok = transform.root;
        foreach (RaycastHit h in Physics.RaycastAll(transform.position, -transform.forward, GeriAlma + 0.1f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(kok)) continue;
            geri = Mathf.Min(geri, h.distance - 0.1f);
        }
        GeriAlinan = Mathf.Max(0f, geri);
        // Işık kameranın çocuğu: yerel -Z kameranın arkası (alnın üstündeki asıl yerinden geri).
        isik.transform.localPosition = temelKonum + Vector3.back * GeriAlinan;
        isik.range = Menzil + GeriAlinan;
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

        // #119 (Şahan): fener F'ye döndü (#76'da L idi); inceleme R'ye taşındı. Oyun kolunda LB.
        // #125: tuş atanabilir (Tuslar); varsayılan yine F / LB.
        if (Tuslar.Basildi(TusEylemi.Fener)) Degistir();
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
        // #119: senaryolu sönme sürerken düğme işe yaramaz: tık duyulur, ışık gelmez (lamba "ölü"), sönme
        // bitince lamba eski durumuna döner. Eskiden bu basış sönmeyi kesip lambayı yakıyordu; 2. kat dersi
        // (#112) "oyuncu açamaz"a dayanır.
        if (Time.time < sonmeBitis)
        {
            if (tik != null) kaynak.PlayOneShot(tik, 0.35f);
            return;
        }
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
        if (!acik || Time.time < sonmeBitis) return;   // sönükken titreyecek ışık yok
        if (titreme != null) StopCoroutine(titreme);
        titreme = StartCoroutine(TitremeAkisi(sure));
    }

    /// <summary>Senaryolu sönme: 'sure' saniye kapanır, sonra kendiliğinden yanar (oyuncu açmaya çalışırsa yanmaz).</summary>
    public void Sondur(float sure)
    {
        if (!takili) return;   // kafada lamba yokken söndürülecek bir şey de yok
        // #119: iki sönme üst üste gelirse (4. katta parlayan kodun 1,8 sn'si + gerilim sönmesi) ikincisi
        // birincinin yerine geçmez, yalnızca bitişi uzatır. Eskiden ikinci çağrı "önceki durum"u zaten
        // sönük okuyup lambayı kalıcı sönük bırakıyordu.
        float yeniBitis = Time.time + sure;
        if (Time.time < sonmeBitis)
        {
            if (yeniBitis > sonmeBitis) sonmeBitis = yeniBitis;
            return;
        }
        if (titreme != null) StopCoroutine(titreme);
        sonmeBitis = yeniBitis;
        titreme = StartCoroutine(SonmeAkisi());
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

    private IEnumerator SonmeAkisi()
    {
        bool once = acik;
        acik = false;
        Uygula(false);
        if (tik != null) kaynak.PlayOneShot(tik, 0.3f);
        // Bitiş anı uzatılabilir (ikinci Sondur): sabit bir bekleme yerine saate bakılır.
        while (Time.time < sonmeBitis) yield return null;
        acik = once && !SaklanmaYeri.Saklaniyor;   // dolaptayken geri yanmaz (saklanma lambayı söndürmüştü)
        Uygula(acik);
        if (acik && tik != null) kaynak.PlayOneShot(tik, 0.3f);
        titreme = null;
    }

    private void Uygula(bool yansin)
    {
        if (isik != null) isik.enabled = yansin;
    }
}
