// IcKapi.cs
// Dairenin içindeki menteşeli oda kapısı (#53): açılır, kapanır, kilitlenebilir.
//
// Bakınca "AÇ (E)" ya da "KAPAT (E)"; kilitliyse "KİLİTLİ (E)" ve basınca kol sarsılır,
// kapı yerinden oynamaz. Balkon kapısı hep kilitlidir (hepKilitli). Kilidin NASIL açılacağı
// (anahtar, şifre, sigorta, tornavida) #54'ün işi; bu bileşen yalnızca "kilitli mi" bilgisini
// tutar ve kilit sistemi Kilitle(false) diyerek kapıyı serbest bırakır.
//
// Sahanlıktaki kat kapısı (KatKapisi) gibi IEtkilesimli sözleşmesini imzalar: oyuncunun
// nişan bileşeni (Etkilesimci) kapının ne olduğunu bilmez, yalnızca ipucunu gösterir ve
// Etkilesim'i çağırır.
//
// Kanadın hangi yöne açılacağını (açının işaretini) kurucu hesaplar: dört daire birbirinin
// aynası olduğu için aynı kapı bir dairede +95°, aynasında -95° döner. Kapı her zaman ait
// olduğu odanın İÇİNE açılır, hole taşmaz.

using System.Collections;
using UnityEngine;

public class IcKapi : MonoBehaviour, IEtkilesimli
{
    private const float HareketSuresi = 0.9f;   // açılma/kapanma süresi (sn)

    [SerializeField] private Transform mentese;    // kanadın döndüğü nokta (menteşe hattı)
    [SerializeField] private Transform kanat;      // kanadın kendisi (kapanma denetimi için)
    [SerializeField] private float acikAci = 95f;  // işaretli: odanın içine doğru açılan yön
    [SerializeField] private bool kilitli;
    [SerializeField] private bool hepKilitli;      // balkon kapısı: hiçbir zaman açılmaz

    private bool acik;
    private float aci;                             // şu anki açı (0 kapalı)
    private Coroutine hareket;
    private static AudioClip acilmaSesi, kilitliSesi;

    public bool Acik { get { return acik; } }
    public bool Kilitli { get { return kilitli; } }
    /// <summary>Menteşeleri sökülüp devrildi mi (#54)? Devrik kapı açık bir kapı gibidir: sesi boğmaz.</summary>
    public bool Devrik { get { return devrik; } }

    private void Awake()
    {
        // Sesler Resources/Sesler'den bir kez yüklenir; bütün kapılar aynı klipleri paylaşır
        // ("static": sınıfa ait tek bir kopya, her kapıya ayrı değil).
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Acilma");
        if (kilitliSesi == null) kilitliSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Kilitli");
        if (hepKilitli) kilitli = true;
    }

    public string Ipucu
    {
        get
        {
            // Zincirin kilidi varsa ve çözülmemişse ipucunu o belirler (#54): doğru anahtar
            // eldeyse "AÇ", değilse "KİLİTLİ".
            Kilit kilit = GetComponent<Kilit>();
            if (kilit != null && !kilit.Cozuk) return kilit.Ipucu;
            if (kilitli) return Dil.Getir("ipucu.kilitli");
            return acik ? Dil.Getir("ipucu.kapat") : Dil.Getir("ipucu.ac");
        }
    }

    // Nişangâh simgesi (#104): Ipucu ile aynı kararlar.
    public NisanSimgesi Simge
    {
        get
        {
            Kilit kilit = GetComponent<Kilit>();
            if (kilit != null && !kilit.Cozuk) return kilit.Simge;
            if (kilitli) return NisanSimgesi.Kilitli;
            return acik ? NisanSimgesi.Kapat : NisanSimgesi.Ac;
        }
    }

    // Kapı dönerken ikinci bir basış yok sayılır; dönüş bitince yeniden etkileşilebilir.
    // Kilitli kapı da etkileşime açık: oyuncu dener, "kilitli" sesini duyar (Şahan, 2026-09-29: bu
    // katta açılmayan oda kapılarında da nişangâh dolsun, eski hâli).
    public bool EtkilesimeAcik { get { return hareket == null && !devrik; } }

    public void Etkilesim(GameObject oyuncu)
    {
        // Önce kilit: doğru anahtar eldeyse kilit çözülür ve kapıyı kendisi açar.
        Kilit kilit = GetComponent<Kilit>();
        if (kilit != null && !kilit.Cozuk && kilit.Etkilesim(oyuncu)) return;
        if (kilitli)
        {
            // Kol iner ama dil kilitte: kapı sarsılır, açılmaz. Cevap kapının kendisi.
            Cal(kilitliSesi, 0.7f);
            Varlik.Gurultu(0.04f);   // kapı sarsıldı (#86)
            return;
        }
        // Kapanırken kanadın süpüreceği yerde oyuncu duruyorsa kapatmıyoruz: yoksa kanat
        // oyuncunun içinden geçer ve oyuncu kapının içinde sıkışabilir.
        if (acik && KanatYoluDolu()) return;
        hareket = StartCoroutine(Dondur(acik ? 0f : acikAci));
    }

    private IEnumerator Dondur(float hedef)
    {
        bool aciliyor = hedef != 0f;
        // Kapanırken ses yok (#74, Şahan: kapanma sesi kalksın).
        if (aciliyor) Cal(acilmaSesi, 0.6f);
        float bas = aci;
        for (float t = 0f; t < HareketSuresi; t += Time.deltaTime)
        {
            // Yavaş başlayıp yavaş biten eğri (kosinüsün yarısı): elle itilen tahta kapı.
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(t / HareketSuresi));
            AciyiUygula(Mathf.Lerp(bas, hedef, e));
            yield return null;   // bir kare bekle
        }
        AciyiUygula(hedef);
        acik = aciliyor;
        hareket = null;
    }

    // ---------- SÜRÜKLEME (#88) ----------
    // Şahan: "kapılar E'ye basınca kendi kendine açılıyor." E basılı tutulup fare itilince kanat
    // fareyle döner, bırakınca olduğu yerde kalır (aralık kapı). Kısa bir E dokunuşu eskisi gibi
    // tam açar/kapar (Etkilesim). Sürükleme yalnızca kilitsiz, devrilmemiş, hareketsiz kapıda.

    /// <summary>Şu an sürüklenebilir mi (kilit yok ya da çözük, devrik değil, hareket etmiyor)?</summary>
    public bool Suruklenebilir
    {
        get
        {
            if (hareket != null || devrik || kilitli) return false;
            Kilit kilit = GetComponent<Kilit>();
            return kilit == null || kilit.Cozuk;
        }
    }

    /// <summary>
    /// Fareyi ileri itmek kapıyı oyuncudan UZAĞA itmeli. Kanat açılırken kameradan uzaklaşıyorsa
    /// +1 (ileri = aç), yaklaşıyorsa -1 (ileri = kapat). Kanadın ortasının şimdiki ve 5° daha açık
    /// konumu karşılaştırılır.
    /// </summary>
    public float AcilisIsareti(Vector3 kameraIleri)
    {
        if (mentese == null || kanat == null || mentese.parent == null) return 1f;
        Vector3 simdi = kanat.position;
        float isaret = Mathf.Sign(acikAci);
        Vector3 sonra = mentese.parent.TransformPoint(mentese.localPosition + Quaternion.Euler(0f, aci + 5f * isaret, 0f) * kanat.localPosition);
        Vector3 acilma = sonra - simdi;   // açılırken kanadın gittiği yön
        return Vector3.Dot(acilma, kameraIleri) >= 0f ? 1f : -1f;
    }

    /// <summary>Kanadı 'derece' kadar açar (artı) ya da kapatır (eksi); 0 ile tam açık arasında kalır.</summary>
    public void Surukle(float derece)
    {
        if (!Suruklenebilir || Mathf.Abs(derece) < 0.0001f) return;
        float isaret = Mathf.Sign(acikAci);
        float yeni = Mathf.Clamp(aci * isaret + derece, 0f, Mathf.Abs(acikAci)) * isaret;
        // Kapanırken kanadın süpüreceği yerde oyuncu varsa durur (kapı oyuncunun üstüne kapanmaz).
        if (Mathf.Abs(yeni) < Mathf.Abs(aci) && KanatYoluDolu()) return;
        if (Mathf.Approximately(aci, 0f) && !Mathf.Approximately(yeni, 0f)) Cal(acilmaSesi, 0.5f);   // kapalıdan ayrılırken gıcırtı
        GicirtiyiBesle(Mathf.Abs(yeni - aci));
        AciyiUygula(yeni);
    }

    /// <summary>Sürükleme bitti: 3°'den fazla açıksa "açık" sayılır (E'ye dokununca kapanır).</summary>
    public void SuruklemeBitti()
    {
        acik = Mathf.Abs(aci) > 3f;
    }

    // ---------- SÜRÜKLEME GICIRTISI (#100) ----------
    // Amnesia'nın kapıları gibi: kapı fareyle itilirken menteşe SÜREKLİ gıcırdar; yavaş itilen kapı
    // kısık ve uzun, hızlı itilen kapı gür gıcırdar, durunca susar. Döngülü bir gıcırtı (menteşede,
    // 3B) hep çalar ama sesi kapının dönüş hızına göre açılıp kısılır; ses yoksa (kapı duruyor) kaynak
    // durdurulur. Saniyede 120° ve üstü tam ses.
    private const float GicirtiTamHiz = 120f, GicirtiEnSes = 0.55f;
    private static AudioClip gicirtiDongu;
    private AudioSource gicirtiKaynagi;
    private float gicirtiHedef;       // bu karede istenen ses (Surukle besler)

    private void GicirtiyiBesle(float donulenDerece)
    {
        if (gicirtiDongu == null) gicirtiDongu = Resources.Load<AudioClip>("Sesler/Kapi_Gicirti_Dongu");
        if (gicirtiDongu == null || mentese == null || Time.deltaTime <= 0f) return;
        if (gicirtiKaynagi == null)
        {
            gicirtiKaynagi = mentese.gameObject.AddComponent<AudioSource>();
            gicirtiKaynagi.clip = gicirtiDongu;
            gicirtiKaynagi.loop = true;
            gicirtiKaynagi.playOnAwake = false;
            gicirtiKaynagi.spatialBlend = 1f;      // menteşeden gelir
            gicirtiKaynagi.minDistance = 0.8f;
            gicirtiKaynagi.maxDistance = 12f;
            gicirtiKaynagi.volume = 0f;
        }
        float hiz = donulenDerece / Time.deltaTime;
        gicirtiHedef = Mathf.Max(gicirtiHedef, Mathf.Clamp01(hiz / GicirtiTamHiz) * GicirtiEnSes);
        // Perde hızla biraz değişir: yavaş itilen kapı daha derin inler.
        gicirtiKaynagi.pitch = Mathf.Lerp(0.85f, 1.1f, Mathf.Clamp01(hiz / GicirtiTamHiz));
        if (!gicirtiKaynagi.isPlaying) gicirtiKaynagi.Play();
    }

    private void LateUpdate()
    {
        if (gicirtiKaynagi == null) return;
        // Ses hedefe yumuşakça gider; hedef her kare sıfırlanır, Surukle o kare beslemediyse ses söner.
        float t = 1f - Mathf.Exp(-Time.deltaTime / 0.08f);
        gicirtiKaynagi.volume = Mathf.Lerp(gicirtiKaynagi.volume, gicirtiHedef, t);
        gicirtiHedef = 0f;
        if (gicirtiKaynagi.isPlaying && gicirtiKaynagi.volume < 0.005f) gicirtiKaynagi.Stop();
    }

    /// <summary>Kilidi takar ya da çözer. Hep kilitli kapı (balkon) çözülmez.</summary>
    /// <summary>
    /// Kapıyı sert çarpar (#131, cin olayı): açık kanat 'sure' saniyede hızlanarak kapanır, çarpma sesi
    /// kanadın yerinde çalar, kapı kapalı kalır. Oyuncu yoldaysa yalnızca ses çalar (kanat oyuncunun
    /// içinden geçmez, #103). Süren bir hareket varsa durdurulur. Coroutine olarak beklenebilir.
    /// </summary>
    public IEnumerator Carp(float sure, AudioClip carpma)
    {
        Vector3 yer = (mentese != null ? mentese.position : transform.position) + Vector3.up * 1.0f;
        if (!acik || KanatYoluDolu())
        {
            if (carpma != null) AudioSource.PlayClipAtPoint(carpma, yer, 1f);
            yield break;
        }
        if (hareket != null) { StopCoroutine(hareket); hareket = null; }
        float bas = aci;
        sure = Mathf.Max(0.06f, sure);
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            float e = t / sure;
            AciyiUygula(Mathf.Lerp(bas, 0f, e * e));   // hızlanarak: itilen değil, savrulan kapı
            yield return null;
        }
        AciyiUygula(0f);
        acik = false;
        if (carpma != null) AudioSource.PlayClipAtPoint(carpma, yer, 1f);
    }

    public void Kilitle(bool k)
    {
        kilitli = k || hepKilitli;
        // Açık bir kapı kilitlenirse önce kapanır: açık duran kilitli kapı olmaz.
        if (kilitli && acik) Ayarla(false);
    }

    /// <summary>Kapıyı sessizce, bir anda açık ya da kapalı konuma getirir (kat hazırlanırken).</summary>
    public void Ayarla(bool yeniAcik)
    {
        if (hareket != null) { StopCoroutine(hareket); hareket = null; }
        acik = yeniAcik && !kilitli;
        AciyiUygula(acik ? acikAci : 0f);
    }

    /// <summary>
    /// Kapının DIŞ yüzünde (açıldığı odanın karşı tarafında, koridor tarafında), kolun yanında,
    /// verilen yükseklikteki nokta ve dışarı bakan yön. Asma kilit buraya takılır.
    /// ebeveyn: noktanın bağlanacağı nesne (menteşe; kapı dönerse onunla döner).
    /// </summary>
    /// kenarOrani: menteşeden serbest kenara doğru ne kadar (0 menteşe, 1 serbest kenar).
    public bool OnNoktasi(float kenarOrani, float yukseklik, out Vector3 konum, out Vector3 disaYon, out Transform ebeveyn)
    {
        konum = Vector3.zero; disaYon = Vector3.forward; ebeveyn = mentese;
        if (mentese == null || kanat == null) return false;
        // Menteşeden kanadın serbest kenarına giden yatay vektör (menteşenin kendi ekseninde).
        Vector3 kenar = new Vector3(kanat.localPosition.x, 0f, kanat.localPosition.z) * 2f;
        // Kanat açılırken serbest kenarın gittiği yön = odanın tarafı. Küçük bir açıyla bakıyoruz
        // ki yön kanada dik çıksın; dışarısı bunun tersi.
        Vector3 odaYonu = (Quaternion.Euler(0f, Mathf.Sign(acikAci) * 10f, 0f) * kenar - kenar).normalized;
        Vector3 yerel = kenar * kenarOrani + Vector3.up * yukseklik - odaYonu * 0.022f;
        konum = mentese.TransformPoint(yerel);
        disaYon = mentese.TransformDirection(-odaYonu);
        return true;
    }

    // ---------- DEVRİLME (tornavida kilidi, MenteseSokucu) ----------
    private bool devrik;

    /// <summary>Menteşeleri sökülen kapı devrildi: artık açılıp kapanmaz.</summary>
    public void Devir()
    {
        if (hareket != null) { StopCoroutine(hareket); hareket = null; }
        devrik = true;
    }

    /// <summary>Kat değişince devrik kapı yerine konur.</summary>
    public void Kaldir() { devrik = false; }

    /// <summary>Kapının devrileceği eksen: alt kenarının oda tarafı; isaret tepenin odaya gitmesi için.</summary>
    public void DevrilmeEkseni(out Vector3 nokta, out Vector3 eksen, out float isaret)
    {
        Vector3 kenar = new Vector3(kanat.localPosition.x, 0f, kanat.localPosition.z) * 2f;
        Vector3 odaYonu = (Quaternion.Euler(0f, Mathf.Sign(acikAci) * 10f, 0f) * kenar - kenar).normalized;
        nokta = mentese.TransformPoint(kenar * 0.5f + odaYonu * 0.022f);
        eksen = mentese.TransformDirection(kenar.normalized);
        // Unity'de bir eksen etrafında pozitif dönüş, v vektörünü (eksen x v) yönüne iter.
        // Yukarı vektörün odaya gitmesi isteniyor: yön tutmuyorsa açı eksi olmalı.
        isaret = Vector3.Dot(Vector3.Cross(eksen, Vector3.up), mentese.TransformDirection(odaYonu)) > 0f ? 1f : -1f;
    }

    private void AciyiUygula(float a)
    {
        aci = a;
        if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, a, 0f);
    }

    /// <summary>Oyuncu kanadın kapalı konumunun içinde mi (kapı kapanırsa içinden geçer)? KatAni bakar (#103).</summary>
    public bool OyuncuYolunda { get { return KanatYoluDolu(); } }

    // Kanadın KAPALI konumunda kapladığı kutuyu yoklar: içinde oyuncunun gövdesi var mı?
    private bool KanatYoluDolu()
    {
        if (mentese == null || kanat == null) return false;
        // Kapalı konumda kanadın merkezi: menteşenin yerel konumu + kanadın menteşeye göre yeri,
        // menteşe dönmemiş gibi (kapı kapalıyken menteşenin dönüşü sıfır).
        Vector3 merkez = mentese.parent.TransformPoint(mentese.localPosition + kanat.localPosition);
        Vector3 yari = Vector3.Scale(kanat.localScale, mentese.parent.lossyScale) * 0.5f;
        // OverlapBox: verilen kutunun içine giren bütün çarpışma kutularını döndürür.
        foreach (Collider c in Physics.OverlapBox(merkez, yari, mentese.parent.rotation))
            if (c.GetComponentInParent<CharacterController>() != null) return true;
        return false;
    }

    private void Cal(AudioClip klip, float ses)
    {
        if (klip != null) AudioSource.PlayClipAtPoint(klip, mentese != null ? mentese.position : transform.position, ses);
    }
}
