// GerilimEtkisi.cs
// Gerilimin ekrana ve bedene yansıması (#92): gerilim yükseldikçe ekranın kenarları kararır,
// gren artar, renk solar; oyuncu kendi kalp atışını duyar ve kamera hafifçe titrer. Av
// başlayınca hepsi en üste çıkar ve renkler kenarlarda hafifçe kayar (renk sapması).
//
// Şahan'ın No70 / Phasmophobia karşılaştırması (2026-10-01): gerilim (#86, Varlik) yalnızca
// olaylarla hissediliyordu; aralarda oyuncunun bedeni hiçbir şey söylemiyordu. Gerilim sayısı
// oyunda hiçbir yerde YAZILMAZ (#86 kuralı); onu ekran ve ses anlatır.
//
// GÜÇ (s, 0-1): Varlik.Ornek.Gerilim'den hesaplanır.
//   - 0,25'in altında hiçbir şey yok; 0,25'ten 1'e "yumuşak basamak" (SmoothStep: başta ve
//     sonda yavaş, ortada hızlı artan eğri) ile 0'dan 1'e çıkar. Av sürerken 1.
//   - Yumuşatılır: yükselirken ~1,5 sn, düşerken ~3 sn (korku çabuk gelir, yavaş geçer).
//   - Başlık ekranında, menüde (timeScale 0) ve oyuncunun kontrolü kapalıyken (yakın bakış,
//     final) hedef 0'dır; birden kesilmez, aynı yumuşaklıkla söner.
//
// EKRAN (post-processing): oyun başlarken çalışma anında YENİ bir global Volume kurulur
// (öncelik 50, gece profilinin Volume'u 0'da). Bu Volume'un ağırlığı (weight) s'dir.
//   URP birden çok Volume'u şöyle karıştırır: öncelik sırasıyla (küçükten büyüğe) her Volume,
//   işaretli (override) her ayarı kendi değerine doğru 'weight' oranında çeker:
//       sonuç = Lerp(alttakilerin sonucu, bu Volume'un değeri, weight)
//   Yani bizim değerimizi "hedef" diye mutlak bir sayı verirsek, s = 1'de gecenin değeri
//   tamamen SİLİNİR. Bu iki yerde sorun: gece profili zaten vinyet 0,45 kullanıyor (hedef 0,45
//   olsaydı hiçbir şey değişmezdi) ve pozlama oyuncunun parlaklık ayarını taşıyor (#30;
//   GorunumUygulayici gecenin pozlamasına ekliyor; mutlak bir hedef bu ayarı yutardı).
//   ÇÖZÜM, "ekleme mantığı": her kare gecenin O ANKİ değeri (taban) okunur ve bizim değerimiz
//   taban + fark yapılır. Karışım formülüne koyunca:
//       Lerp(taban, taban + fark, s) = taban + s · fark
//   Gece ne olursa olsun (parlaklık ayarı dahil) üstüne s oranında fark eklenmiş olur. Tabanı
//   AtmosferKurucu'dan sabit olarak almıyoruz: o bir Editor dosyası (oyun kodu onu göremez) ve
//   değerleri sabit değil, yazılı sayılar; çalışan profilden okumak her zaman doğrudur.
//
// SES: 2B bir kalp atışı (Resources/Sesler/Kalp_Atisi, oyuncu_sesleri_uret.py'den). Odanın
// yankısı (OdaYankisi) ona uygulanmaz: kalp odada değil, göğüste (bypassListenerEffects).
// İsteğe bağlı nefes döngüsü Resources/Sesler/Nefes: dosya varsa çalar. Canlı sesler
// SENTEZLENMEZ (proje kuralı); kaydedilene kadar nefes yok, kod ona hazır.
//
// Not (#11/#31 kuralı): AddObjectToAsset yalnızca DİSKE KAYDEDİLEN profillerde gerekir. Buradaki
// profil hiç kaydedilmez; oyun sırasında bellekte kurulur ve sahne kapanınca silinir. Kaydedilmesi
// gerekseydi kurucu bir Editor aracında AtmosferKurucu.Ekle<T> gibi yapılırdı.

using UnityEngine;
using UnityEngine.Rendering;               // Volume, VolumeProfile
using UnityEngine.Rendering.Universal;     // URP'nin efektleri: Vignette, FilmGrain, ...

public class GerilimEtkisi : MonoBehaviour
{
    // ---- GÜÇ EĞRİSİ ----
    private const float BaslamaGerilimi = 0.25f;   // bunun altında etki yok
    private const float YukselmeSuresi = 1.5f;     // saniye (zaman sabiti)
    private const float DusmeSuresi = 3f;
    private const float AvYukselme = 0.6f;         // av payının (renk sapması, güçlü titreme) açılması
    private const float AvDusme = 2f;

    // ---- EKRANA EKLENEN FARKLAR (s = 1'de) ----
    private const float VinyetFarki = 0.30f;       // gece 0,45 → 0,75 (0,45 eklemek kenarları simsiyah yapardı)
    private const float VinyetYumusaklikFarki = 0.05f;   // 0,45 → 0,50: kararma daha geniş bir alana yayılır
    private const float GrenFarki = 0.32f;         // gece 0,28 → 0,60
    private const float DoygunlukFarki = -35f;     // gece -18 → -53: renkler solar
    private const float PozlamaFarki = -0.3f;      // EV: biraz daha karanlık
    private const float RenkSapmasi = 0.35f;       // yalnızca avda (gecede yok, taban 0)

    // ---- KALP ----
    private const float KalpEsigi = 0.2f;          // bunun altında kalp duyulmaz
    private const float KalpEnYavas = 1.1f;        // s = 0,3'te iki atış arası (saniye)
    private const float KalpEnHizli = 0.45f;       // s = 1'de (av)
    private const float KalpSesi = 0.55f;          // s = 1'de ses seviyesi
    private const float NefesSesi = 0.4f;

    // ---- KAMERA TİTREMESİ ----
    private const float GerilimTitremesi = 0.15f;  // s = 1'de (av dışı)
    private const float AvTitremesi = 0.5f;

    private BirinciSahisKontrolcu kontrolcu;
    private KameraHareketi kameraHareketi;

    private float s;      // yumuşatılmış güç (0-1)
    private float av;     // yumuşatılmış av payı (0-1)

    // Bizim Volume'umuz ve profili.
    private GameObject hacimNesnesi;
    private Volume hacim;
    private VolumeProfile profil;
    private Vignette vinyet;
    private FilmGrain gren;
    private ColorAdjustments renk;
    private ChromaticAberration sapma;

    // Gece Volume'u (taban değerler buradan okunur).
    private Volume geceHacmi;
    private VolumeProfile geceProfili;   // en son okunduğu profil; değişirse efektler yeniden aranır
    private Vignette geceVinyet;
    private FilmGrain geceGren;
    private ColorAdjustments geceRenk;
    private ChromaticAberration geceSapma;

    private AudioSource kalpKaynagi, nefesKaynagi;
    private AudioClip kalp;
    private float kalpSayaci;

    private void Awake()
    {
        kontrolcu = GetComponentInParent<BirinciSahisKontrolcu>();
        kameraHareketi = GetComponent<KameraHareketi>();
    }

    private void Start()
    {
        HacmiKur();
        SesleriKur();
    }

    // ---------- VOLUME ----------
    private void HacmiKur()
    {
        // ScriptableObject.CreateInstance: diskte dosyası olmayan, yalnızca bellekte yaşayan bir
        // profil. (Dosyadaki gece profili ayrı; ona hiç dokunmuyoruz.)
        profil = ScriptableObject.CreateInstance<VolumeProfile>();
        profil.name = "Gerilim_Profili";

        // Add<T>(false): efekti ekler ama HİÇBİR ayarını işaretlemez. Sonra yalnızca istediğimiz
        // ayarları Override(değer) ile işaretleriz. Add<T>(true) bütün ayarları işaretlerdi:
        // vinyetin merkezi, renk ayarının kontrastı ve renk filtresi gibi dokunmak istemediğimiz
        // ayarlar da varsayılanlarına doğru çekilir, gecenin değerleri bozulurdu.
        vinyet = profil.Add<Vignette>(false);
        vinyet.intensity.Override(0f);
        vinyet.smoothness.Override(0.45f);
        vinyet.color.Override(Color.black);

        // Gren: yalnızca şiddet. Türünü (Thin2 → Medium) değiştirmiyoruz: tür bir sayı değil bir
        // seçenek, karıştırılamaz; ağırlık 0'dan büyük olduğu ANDA birden değişir ve gerilim
        // daha 0,26'dayken grenin dokusu bir anda "sıçrar". Şiddeti artırmak yeterince okunuyor.
        gren = profil.Add<FilmGrain>(false);
        gren.intensity.Override(0f);

        renk = profil.Add<ColorAdjustments>(false);
        renk.saturation.Override(0f);
        renk.postExposure.Override(0f);

        sapma = profil.Add<ChromaticAberration>(false);
        sapma.intensity.Override(0f);

        // Global Volume: kamera nerede olursa olsun geçerli. Önceliği gecenin Volume'undan
        // (0) büyük olmalı ki karışımda ondan SONRA gelsin. Oyuncunun kamerası dışında başlık ve
        // final kameraları da onu görür; ama oralarda s = 0, ağırlık 0 → hiç uygulanmaz.
        hacimNesnesi = new GameObject("Gerilim_Hacmi");
        hacim = hacimNesnesi.AddComponent<Volume>();
        hacim.isGlobal = true;
        hacim.priority = 50f;
        hacim.weight = 0f;
        hacim.sharedProfile = profil;   // kendi profilimiz: kopyalanmasına gerek yok
    }

    // Gece Volume'unu ve efektlerini bulur. Volume'u ADIYLA değil, üstündeki bileşenin TÜRÜYLE
    // buluyoruz (GorunumUygulayici yalnızca gece Volume'unda var): ad değişse de bozulmaz.
    private void GeceyiBul()
    {
        if (geceHacmi == null)
        {
            GorunumUygulayici g = FindAnyObjectByType<GorunumUygulayici>();
            if (g != null) geceHacmi = g.GetComponent<Volume>();
            if (geceHacmi == null) return;
        }
        // GorunumUygulayici parlaklığı Volume'un ÇALIŞMA KOPYASINA (profile) yazar. Kopya henüz
        // yoksa dosyadaki profili (sharedProfile) okuruz. 'profile'ı kendimiz çağırmıyoruz:
        // çağırmak kopyayı yaratır, okumak için buna gerek yok.
        VolumeProfile p = geceHacmi.HasInstantiatedProfile() ? geceHacmi.profile : geceHacmi.sharedProfile;
        if (p == geceProfili) return;   // aynı profil: efektler zaten elimizde
        geceProfili = p;
        geceVinyet = null; geceGren = null; geceRenk = null; geceSapma = null;
        if (p == null) return;
        // TryGet: profilde o türde bir efekt varsa 'out' ile verir. "out" parametresi:
        // fonksiyonun bize ikinci bir sonuç doldurduğu değişken.
        p.TryGet(out geceVinyet);
        p.TryGet(out geceGren);
        p.TryGet(out geceRenk);
        p.TryGet(out geceSapma);
    }

    // Bir ayarın gecedeki değeri: gece o ayarı işaretlemişse onun değeri, yoksa URP'nin
    // varsayılanı (efekt hiç yokmuş gibi).
    private static float Taban(VolumeParameter<float> ayar, float varsayilan)
    {
        return ayar != null && ayar.overrideState ? ayar.value : varsayilan;
    }

    // ---------- SESLER ----------
    private void SesleriKur()
    {
        // Kalp: 2B, mekânın yankısının dışında. ignoreListenerPause false (varsayılan): menüde
        // AudioListener.pause açılınca o da durur.
        kalp = Resources.Load<AudioClip>("Sesler/Kalp_Atisi");
        kalpKaynagi = gameObject.AddComponent<AudioSource>();
        kalpKaynagi.playOnAwake = false;
        kalpKaynagi.spatialBlend = 0f;              // 2B: kafanın içinde
        kalpKaynagi.bypassListenerEffects = true;   // OdaYankisi'nin yankısı uygulanmasın
        kalpKaynagi.bypassReverbZones = true;
        kalpKaynagi.ignoreListenerPause = false;

        // Nefes: dosya henüz yok; olunca kendiliğinden devreye girer.
        AudioClip nefes = Resources.Load<AudioClip>("Sesler/Nefes");
        if (nefes != null)
        {
            nefesKaynagi = gameObject.AddComponent<AudioSource>();
            nefesKaynagi.clip = nefes;
            nefesKaynagi.loop = true;
            nefesKaynagi.playOnAwake = false;
            nefesKaynagi.spatialBlend = 0f;
            nefesKaynagi.bypassListenerEffects = true;
            nefesKaynagi.bypassReverbZones = true;
            nefesKaynagi.volume = 0f;
            nefesKaynagi.Play();
        }
    }

    private void OnDestroy()
    {
        // Bellekte kurduklarımızı temizle: sahne yeniden yüklenince (final sonrası) birikmesinler.
        if (hacimNesnesi != null) Destroy(hacimNesnesi);
        if (profil != null)
        {
            foreach (VolumeComponent c in profil.components) if (c != null) Destroy(c);
            Destroy(profil);
        }
    }

    // ---------- HER KARE ----------
    private void Update()
    {
        // Ölçeksiz zaman: menüde timeScale 0 iken de etki sönebilsin (ölçekli zaman orada durur).
        float dt = Time.unscaledDeltaTime;

        // Hedef güç.
        float hedef = 0f;
        bool avda = false;
        Varlik varlik = Varlik.Ornek;
        if (varlik != null)
        {
            float t = Mathf.InverseLerp(BaslamaGerilimi, 1f, varlik.Gerilim);   // 0,25→0, 1→1
            hedef = Mathf.SmoothStep(0f, 1f, t);
            avda = varlik.AvSuruyor;
            if (avda) hedef = 1f;
        }
        bool kapali = DuraklatmaMenusu.BaslikEkraninda || Time.timeScale == 0f
                      || kontrolcu == null || !kontrolcu.enabled;
        if (kapali) { hedef = 0f; avda = false; }

        // Yumuşatma: yükselirken ve düşerken farklı süre. 1 - e^(-dt/süre): kare hızından bağımsız.
        s = Mathf.Lerp(s, hedef, 1f - Mathf.Exp(-dt / (hedef > s ? YukselmeSuresi : DusmeSuresi)));
        float avHedef = avda ? 1f : 0f;
        av = Mathf.Lerp(av, avHedef, 1f - Mathf.Exp(-dt / (avHedef > av ? AvYukselme : AvDusme)));

        EkraniUygula();
        KalbiUygula();

        // Kamera titremesi: gerilimle hafif, avda belirgin.
        if (kameraHareketi != null)
            kameraHareketi.SurekliSarsinti = Mathf.Lerp(GerilimTitremesi * s, AvTitremesi, av);
    }

    private void EkraniUygula()
    {
        if (hacim == null) return;
        // Çok küçük güçte ağırlık tam 0: URP ağırlığı 0 olan Volume'u hiç işlemez (bedava).
        float w = s < 0.001f ? 0f : s;
        hacim.weight = w;
        if (w <= 0f) return;

        GeceyiBul();
        // Ekleme mantığı (dosyanın başındaki açıklama): değer = taban + fark.
        // Varsayılanlar URP'nin kendi varsayılanları (Vignette.cs, FilmGrain.cs, ...).
        float tVinyet = Taban(geceVinyet != null ? geceVinyet.intensity : null, 0f);
        float tYumus = Taban(geceVinyet != null ? geceVinyet.smoothness : null, 0.2f);
        float tGren = Taban(geceGren != null ? geceGren.intensity : null, 0f);
        float tDoygun = Taban(geceRenk != null ? geceRenk.saturation : null, 0f);
        float tPoz = Taban(geceRenk != null ? geceRenk.postExposure : null, 0f);
        float tSapma = Taban(geceSapma != null ? geceSapma.intensity : null, 0f);

        // ClampedFloatParameter değeri kendi sınırına kırpar (vinyet ve gren 0-1, doygunluk ±100).
        vinyet.intensity.value = tVinyet + VinyetFarki;
        vinyet.smoothness.value = tYumus + VinyetYumusaklikFarki;
        gren.intensity.value = tGren + GrenFarki;
        renk.saturation.value = tDoygun + DoygunlukFarki;
        renk.postExposure.value = tPoz + PozlamaFarki;

        // Renk sapması yalnızca avda ve s'den ayrı: sonuç = taban + w · (değer - taban). Bunu
        // taban + av · RenkSapmasi yapmak için değeri ağırlığa böleriz. Avda s zaten 1'e gider;
        // bölme yalnızca av payı ağırlıktan önde olduğu kısa anda değeri büyütür (1'e kırpılır).
        sapma.intensity.value = tSapma + av * RenkSapmasi / w;
    }

    private void KalbiUygula()
    {
        // Nefes döngüsü (varsa): ses güçle birlikte, 0,3'ün altında duyulmaz.
        if (nefesKaynagi != null) nefesKaynagi.volume = NefesSesi * Mathf.InverseLerp(0.3f, 1f, s);

        if (kalp == null) return;
        // Menüde (timeScale 0) yeni atış başlatma: dinleyici duraklatılmışken başlayan ses
        // menüden çıkınca birden çalardı.
        if (Time.timeScale == 0f) return;

        if (s < KalpEsigi)
        {
            // Eşiğin altında sayaç sıfırda bekler: güç eşiği geçince ilk atış hemen duyulsun.
            kalpSayaci = 0f;
            return;
        }

        kalpSayaci -= Time.deltaTime;
        if (kalpSayaci > 0f) return;

        // Bir atış (lub-dub tek dosyada). Ses seviyesi eşikten 1'e 0 → 0,55.
        float ses = KalpSesi * Mathf.InverseLerp(KalpEsigi, 1f, s);
        kalpKaynagi.PlayOneShot(kalp, ses);

        // Bir sonraki atışa kadar: s 0,3'te 1,1 sn, 1'de 0,45 sn. ±%3 oynama: kalp metronom değil.
        float aralik = Mathf.Lerp(KalpEnYavas, KalpEnHizli, Mathf.InverseLerp(0.3f, 1f, s));
        kalpSayaci = aralik * Random.Range(0.97f, 1.03f);
    }
}
