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
    private static AudioClip acilmaSesi, kapanmaSesi, kilitliSesi;

    public bool Acik { get { return acik; } }
    public bool Kilitli { get { return kilitli; } }

    private void Awake()
    {
        // Sesler Resources/Sesler'den bir kez yüklenir; bütün kapılar aynı klipleri paylaşır
        // ("static": sınıfa ait tek bir kopya, her kapıya ayrı değil).
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Acilma");
        if (kapanmaSesi == null) kapanmaSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Kapanma");
        if (kilitliSesi == null) kilitliSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Kilitli");
        if (hepKilitli) kilitli = true;
    }

    public string Ipucu
    {
        get
        {
            if (kilitli) return Dil.Getir("ipucu.kilitli");
            return acik ? Dil.Getir("ipucu.kapat") : Dil.Getir("ipucu.ac");
        }
    }

    // Kapı dönerken ikinci bir basış yok sayılır; dönüş bitince yeniden etkileşilebilir.
    public bool EtkilesimeAcik { get { return hareket == null; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (kilitli)
        {
            // Kol iner ama dil kilitte: kapı sarsılır, açılmaz. Cevap kapının kendisi.
            Cal(kilitliSesi, 0.7f);
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
        Cal(aciliyor ? acilmaSesi : kapanmaSesi, 0.6f);
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

    /// <summary>Kilidi takar ya da çözer. Hep kilitli kapı (balkon) çözülmez.</summary>
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

    private void AciyiUygula(float a)
    {
        aci = a;
        if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, a, 0f);
    }

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
