// CinOlaylari.cs
// Cin olayları (#131): oyuncu dairedeyken, görüş dışında, aralıklı küçük dehşetler. Zamanlama ve seçim
// CinOlayKurali'nde (saf C#, testli); bu bileşen yöneticinin nesnesinde durur (Varlik gibi) ve seçilen olayı
// sahneye uygular.
//
// OLAYLAR (hepsi oyuncunun GÖRÜŞ DIŞINDA; Gorus.Gorunur ile denetlenir):
//   KapiCarpma        başka bir odanın açık kapısı sert kapanır (gerçek çarpma kaydı); kapalı kalır.
//   Kirilma           oyuncunun 2-4 m arkasında tabak / bardak / vazo kırılır; yerde cam kırıkları kalır.
//   BakmayincaDegisim kapalı bir oda kapısı ya da çekmece/kapak görüş dışındayken açılmış bulunur (sessiz).
//   IceridenVurus     kapalı (varsa kilitli) bir odanın kapısına içeriden üç vuruş.
//   ArkadanAdim       oyuncunun arkasından 3-4 adım (zeminin kendi sesi); oyuncu dönünce kesilir.
//   Cihaz             katın radyosu/TV'si (RadyoAnonsu) bir anlığına cızırtıyla açılıp kapanır.
//
// KURALLAR:
//   - Olaylar Varlik.Gurultu ÇAĞIRMAZ (oyuncunun gürültüsü değil, cinin işi) ve GolgeImzasi ÇALMAZ (imza
//     varlığın görünmesinin habercisi kalsın).
//   - Saklanırken, av sürerken, 6. katın saklanma anı ve 7. katın kaçışı sırasında, menüde, açılışta ve
//     dokuzuncu katta olay yok. Sahanlıkta sayaç durur (olaylar dairenin içindir).
//   - Varlik.Kapali (bot, kat denetimi) bunu da kapatır.
//   - Kat değişince kırık parçalar silinir, süren olay durur, kural baştan kurulur.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinOlaylari : MonoBehaviour
{
    [SerializeField] private AudioClip carpmaSesi;       // Kapi_Carpma (kayıt, S43)
    [SerializeField] private AudioClip vurusSesi;        // Kapi_Tiklatma (kayıt, S42)
    [SerializeField] private GameObject kirikSablonu;    // Cam_Kiriklari (kavanozla aynı şablon)

    public static CinOlaylari Ornek { get; private set; }

    private const float DaireKontrolAraligi = 0.25f;
    private const float KirilmaUzaklikEnAz = 2f, KirilmaUzaklikEnCok = 4f;
    private const float VurusMesafesi = 9f;
    private const int AdimSayisiEnAz = 3, AdimSayisiEnCok = 4;
    private const float AdimAraligi = 0.45f;

    private CinOlayKurali kural;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private Transform oyuncu;
    private Camera kamera;
    private KafaLambasi lamba;
    private int kat = -1;
    private float daireSayaci;
    private Transform daire;                       // oyuncunun içinde olduğu daire (DaireEksenleri'nin nesnesi), yoksa null
    private Coroutine suren;
    private readonly List<GameObject> konanlar = new List<GameObject>();   // kırık parçalar: kat değişince silinir
    private AudioClip[] kirilmaSesleri;

    /// <summary>Test göstergesi: son olay ve bir sonrakine kalan süre.</summary>
    public string Durum
    {
        get
        {
            if (kural == null) return "";
            return $"{kural.Son} {(daire != null ? "" : "(sahanlık) ")}{kural.Kalan:0}s";
        }
    }

    private void Awake() { Ornek = this; }
    private void OnDestroy() { if (Ornek == this) Ornek = null; }

    private void Start()
    {
        kural = new CinOlayKurali(System.Environment.TickCount);
        yonetici = GetComponent<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrol != null)
        {
            oyuncu = kontrol.transform;
            kamera = kontrol.GetComponentInChildren<Camera>();
        }
        lamba = FindAnyObjectByType<KafaLambasi>();
        // Kırılma sesleri (S79-S81): esya_sesleri_uret.py → kirilma_tabak / kirilma_bardak / kirilma_vazo.
        var liste = new List<AudioClip>();
        foreach (string ad in new[] { "Kirilma_Tabak", "Kirilma_Bardak", "Kirilma_Vazo" })
        {
            AudioClip c = Resources.Load<AudioClip>("Sesler/" + ad);
            if (c != null) liste.Add(c);
        }
        if (liste.Count == 0) { AudioClip c = Resources.Load<AudioClip>("Sesler/Cam_Kirilma"); if (c != null) liste.Add(c); }
        kirilmaSesleri = liste.ToArray();
    }

    private void Update()
    {
        if (kural == null || yonetici == null || oyuncu == null) return;

        if (yonetici.BulunulanKat != kat)
        {
            kat = yonetici.BulunulanKat;
            kural.KatAyarla(kat);
            Temizle();
        }

        bool uygun = Etkin();
        if (uygun) DairedeMi();
        uygun = uygun && daire != null && suren == null;
        if (!kural.Ilerlet(Time.deltaTime, uygun)) return;
        if (!uygun) return;   // sayaç dolu ama şu an uygun değil: uygun olunca seçilir

        bool[] mevcut = new bool[CinOlayKurali.OlaySayisi];
        mevcut[(int)CinOlayi.KapiCarpma] = CarpilacakKapi() != null;
        mevcut[(int)CinOlayi.Kirilma] = KirilmaNoktasi(out _);
        mevcut[(int)CinOlayi.BakmayincaDegisim] = DegisecekKapi() != null || DegisecekParca() != null;
        mevcut[(int)CinOlayi.IceridenVurus] = VurulacakKapi() != null;
        mevcut[(int)CinOlayi.ArkadanAdim] = true;
        mevcut[(int)CinOlayi.Cihaz] = Cihaz() != null;
        CinOlayi olay = kural.Sec(mevcut);
        if (olay == CinOlayi.Yok) return;
        Debug.Log($"[CIN] {kat}. kat: {olay}");
        switch (olay)
        {
            case CinOlayi.KapiCarpma: suren = StartCoroutine(KapiyiCarp()); break;
            case CinOlayi.Kirilma: Kir(); break;
            case CinOlayi.BakmayincaDegisim: Degistir(); break;
            case CinOlayi.IceridenVurus: suren = StartCoroutine(IceridenVur()); break;
            case CinOlayi.ArkadanAdim: suren = StartCoroutine(ArkadanYuru()); break;
            case CinOlayi.Cihaz: Cihaz()?.KisaParazit(); break;
        }
    }

    // Kural bu karede işlesin mi? Varlik.Etkin ile aynı kapılar; saklanırken ve av sürerken de kapalı.
    private bool Etkin()
    {
        if (Varlik.Kapali) return false;
        if (DokuzuncuKat.Icinde) return false;
        if (!yonetici.enabled || !yonetici.GolgeSayabilir) return false;   // açılış, kapı açık, çatı, final
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        if (kontrol == null || !kontrol.enabled) return false;              // menü, not, şifre ekranı, saklanma
        if (SaklanmaYeri.Saklaniyor) return false;
        if (Time.timeScale <= 0f) return false;
        Varlik v = Varlik.Ornek;
        if (v != null && v.AvSuruyor) return false;
        foreach (KacisAni k in FindObjectsByType<KacisAni>()) if (k.Basladi) return false;
        foreach (SaklanmaAni s in FindObjectsByType<SaklanmaAni>()) if (s.Suruyor) return false;
        return true;
    }

    // Oyuncunun altındaki zemin bir dairenin parçası mı? 0,25 sn'de bir bakılır (Varlik.DairedeMi gibi).
    private void DairedeMi()
    {
        daireSayaci += Time.deltaTime;
        if (daireSayaci < DaireKontrolAraligi) return;
        daireSayaci = 0f;
        daire = null;
        foreach (RaycastHit h in Physics.RaycastAll(oyuncu.position + Vector3.up * 0.5f, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(oyuncu)) continue;
            DaireEksenleri e = h.collider.GetComponentInParent<DaireEksenleri>();
            if (e != null && !DokuzuncuKat.SetinParcasi(e)) { daire = e.transform; break; }
        }
    }

    private void Temizle()
    {
        if (suren != null) { StopCoroutine(suren); suren = null; }
        foreach (GameObject g in konanlar) if (g != null) Destroy(g);
        konanlar.Clear();
        daire = null;
    }

    private bool Gorunur(Vector3 nokta, Transform haric) { return Gorus.Gorunur(kamera, oyuncu, nokta, haric); }

    // ---------- ADAYLAR ----------
    private IEnumerable<IcKapi> DaireKapilari()
    {
        if (daire == null) yield break;
        foreach (IcKapi k in daire.GetComponentsInChildren<IcKapi>())
            if (k.gameObject.activeInHierarchy && !k.Devrik) yield return k;
    }

    // Çarpılacak kapı: açık, oyuncunun yolunda değil, kanadı görünmüyor, 1,5-12 m arası.
    private IcKapi CarpilacakKapi()
    {
        var adaylar = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (!k.Acik || k.OyuncuYolunda || k.EtkilesimeAcik == false) continue;
            float m = Vector3.Distance(k.transform.position, oyuncu.position);
            if (m < 1.5f || m > 12f) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            adaylar.Add(k);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // Bakmayınca açılacak kapı: kapalı, kilitsiz, görünmüyor.
    private IcKapi DegisecekKapi()
    {
        var adaylar = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (k.Acik || k.Kilitli || !k.EtkilesimeAcik || k.OyuncuYolunda) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            adaylar.Add(k);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // Bakmayınca açılacak çekmece / kapak: kapalı, kilitsiz, görünmüyor.
    private AcilirParca DegisecekParca()
    {
        if (daire == null) return null;
        var adaylar = new List<AcilirParca>();
        foreach (AcilirParca p in daire.GetComponentsInChildren<AcilirParca>())
        {
            if (!p.gameObject.activeInHierarchy || p.Acik || p.Kilitli || !p.EtkilesimeAcik) continue;
            Renderer r = p.GetComponentInChildren<Renderer>();
            Vector3 nokta = r != null ? r.bounds.center : p.transform.position;
            if (Gorunur(nokta, p.transform)) continue;
            adaylar.Add(p);
        }
        return adaylar.Count == 0 ? null : adaylar[Random.Range(0, adaylar.Count)];
    }

    // İçeriden vurulacak kapı: kapalı, görünmüyor, 9 m içinde; kilitli olanlar tercih edilir (içeride "kimse" yok).
    private IcKapi VurulacakKapi()
    {
        var kilitliler = new List<IcKapi>(); var digerleri = new List<IcKapi>();
        foreach (IcKapi k in DaireKapilari())
        {
            if (k.Acik) continue;
            if (Vector3.Distance(k.transform.position, oyuncu.position) > VurusMesafesi) continue;
            if (Gorunur(k.transform.position + Vector3.up * 1.0f, k.transform)) continue;
            (k.Kilitli ? kilitliler : digerleri).Add(k);
        }
        var liste = kilitliler.Count > 0 ? kilitliler : digerleri;
        return liste.Count == 0 ? null : liste[Random.Range(0, liste.Count)];
    }

    // Kırılma noktası: oyuncunun 2-4 m arkasında, dairenin zemininde, görünmeyen bir yer.
    private bool KirilmaNoktasi(out Vector3 nokta)
    {
        nokta = Vector3.zero;
        if (daire == null) return false;
        for (int deneme = 0; deneme < 8; deneme++)
        {
            float uzaklik = Random.Range(KirilmaUzaklikEnAz, KirilmaUzaklikEnCok);
            Vector3 geri = -oyuncu.forward * uzaklik + oyuncu.right * Random.Range(-1f, 1f);
            Vector3 ust = oyuncu.position + geri + Vector3.up * 1.2f;
            if (!Physics.Raycast(ust, Vector3.down, out RaycastHit h, 1.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (h.collider.transform.IsChildOf(oyuncu)) continue;
            DaireEksenleri e = h.collider.GetComponentInParent<DaireEksenleri>();
            if (e == null || e.transform != daire) continue;
            if (!Gorus.Arkada(kamera, h.point) || Gorunur(h.point + Vector3.up * 0.1f, null)) continue;
            nokta = h.point;
            return true;
        }
        return false;
    }

    private RadyoAnonsu Cihaz()
    {
        if (daire == null) return null;
        foreach (RadyoAnonsu r in daire.GetComponentsInChildren<RadyoAnonsu>())
            if (r.gameObject.activeInHierarchy && !r.Yayinda) return r;
        return null;
    }

    // ---------- OLAYLAR ----------
    private IEnumerator KapiyiCarp()
    {
        IcKapi kapi = CarpilacakKapi();
        if (kapi != null)
        {
            Titresim.Vur(0.6f, 0.45f, 0.35f);
            if (lamba != null) lamba.Titret(0.5f);
            yield return kapi.Carp(0.16f, carpmaSesi);
        }
        suren = null;
    }

    private void Kir()
    {
        if (!KirilmaNoktasi(out Vector3 nokta)) return;
        if (kirilmaSesleri != null && kirilmaSesleri.Length > 0)
            Cal(kirilmaSesleri[Random.Range(0, kirilmaSesleri.Length)], nokta + Vector3.up * 0.15f, 0.9f);
        if (kirikSablonu != null)
        {
            GameObject g = Instantiate(kirikSablonu, nokta + Vector3.up * 0.005f, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), daire);
            g.name = "Cin_Kiriklari";
            foreach (Collider c in g.GetComponentsInChildren<Collider>()) Destroy(c);   // yerdeki parçaya takılma yok
            konanlar.Add(g);
        }
        Titresim.Vur(0.25f, 0.5f, 0.2f);
    }

    // Sessiz: oyuncu dönünce kapattığı kapıyı açık, kapalı çekmeceyi çekilmiş bulur.
    private void Degistir()
    {
        IcKapi k = DegisecekKapi();
        AcilirParca p = DegisecekParca();
        if (k != null && (p == null || Random.value < 0.5f)) k.Ayarla(true);
        else if (p != null) p.Ayarla(true);
    }

    private IEnumerator IceridenVur()
    {
        IcKapi kapi = VurulacakKapi();
        if (kapi != null && vurusSesi != null)
        {
            Vector3 yer = kapi.transform.position + Vector3.up * 1.2f;
            Titresim.Vur(0.2f, 0.6f, 0.25f);
            Cal(vurusSesi, yer, 0.85f);
            if (lamba != null) lamba.Titret(0.6f);
        }
        yield return null;
        suren = null;
    }

    // Arkadan 3-4 adım; oyuncu dönüp bakarsa (nokta görüşe girerse) kesilir.
    private IEnumerator ArkadanYuru()
    {
        int sayi = Random.Range(AdimSayisiEnAz, AdimSayisiEnCok + 1);
        for (int i = 0; i < sayi; i++)
        {
            Vector3 nokta = oyuncu.position - oyuncu.forward * (3.0f - i * 0.5f) + Vector3.up * 0.05f;
            if (!Gorus.Arkada(kamera, nokta)) break;
            AudioClip klip = ZeminSesi.AdimKlibi(nokta, oyuncu);
            if (klip != null) Cal(klip, nokta, 0.7f);
            yield return new WaitForSeconds(AdimAraligi);
        }
        suren = null;
    }

    // Kısa ömürlü 3B ses (KatAni.Cal gibi: gerçekçi sönüm, dairenin öbür ucundan da duyulur).
    private static void Cal(AudioClip klip, Vector3 yer, float ses)
    {
        if (klip == null) return;
        GameObject g = new GameObject("Cin_Sesi");
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip;
        a.volume = ses;
        a.spatialBlend = 1f;
        a.minDistance = 1.5f;
        a.maxDistance = 20f;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();
        Destroy(g, klip.length + 0.1f);
    }
}
