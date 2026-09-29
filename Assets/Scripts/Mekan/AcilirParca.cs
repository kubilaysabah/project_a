// AcilirParca.cs
// Eşyaların açılan parçaları (#53): çekmece, dolap kapağı, sandık kapağı, buzdolabı kapısı,
// kavanoz kapağı. Bakınca "AÇ (E)" ya da "KAPAT (E)"; basınca parça yumuşakça kayar ya da
// menteşesinde döner. Oda kapısı (IcKapi) gibi ama her türlü küçük parça için genel.
//
// İKİ HAREKET TÜRÜ
//   Kaydir : parça kendi yerel ekseni boyunca "miktar" metre gider (çekmece ileri çıkar).
//   Dondur : parça kendi pivotu etrafında, yerel ekseninde "miktar" derece döner (kapak açılır).
//            Pivot Blender betiğinde menteşeye konduğu için parça kenarından açılır.
// Hangi eksen ve ne kadar olduğu kurucuda verilir (GrayboxKurucu_Daire). Açının işareti
// yönü belirler: kapak yukarı mı, aşağı mı; kapı sağa mı, sola mı.
//
// KİLİT: "kilitli" işaretliyse parça açılmaz, kısa bir takırtı duyulur. Kilidi neyin açacağı
// (anahtar, şifre, kol) #54'ün işi; bu bileşen yalnızca Kilitle(false) ile serbest kalır.

using System.Collections;
using UnityEngine;

public class AcilirParca : MonoBehaviour, IEtkilesimli
{
    public enum Hareket { Kaydir, Dondur }

    [SerializeField] private Hareket hareket = Hareket.Kaydir;
    [SerializeField] private Vector3 eksen = Vector3.forward;   // yerel eksen
    [SerializeField] private float miktar = 0.3f;              // metre (Kaydir) ya da derece (Dondur)
    [SerializeField] private float sure = 0.5f;
    [SerializeField] private bool kilitli;
    // Ekrandaki eylem adları (#65): çekmece "AÇ / KAPAT" der, gardırop "İT", sifon "SÖK / TAK".
    [SerializeField] private string ipucuAc = "ipucu.ac", ipucuKapat = "ipucu.kapat";

    private Vector3 kapaliKonum;
    private Quaternion kapaliDonus;
    private float oran;                  // 0 kapalı, 1 açık
    private bool acik;
    private Coroutine hareketEden;
    private static AudioClip acilmaSesi, kilitliSesi;

    public bool Acik { get { return acik; } }
    public bool Kilitli { get { return kilitli; } }

    private void Awake()
    {
        // Kapalı hâl, sahnede kurulduğu hâldir; açıklık her zaman buna göre hesaplanır.
        kapaliKonum = transform.localPosition;
        kapaliDonus = transform.localRotation;
        // Şimdilik oda kapısının seslerini kullanıyoruz (tahta ve dil tıkırtısı).
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Acilma");
        if (kilitliSesi == null) kilitliSesi = Resources.Load<AudioClip>("Sesler/IcKapi_Kilitli");
    }

    public string Ipucu
    {
        get
        {
            Kilit kilit = GetComponent<Kilit>();
            if (kilit != null && !kilit.Cozuk) return kilit.Ipucu;
            if (kilitli) return Dil.Getir("ipucu.kilitli");
            return acik ? Dil.Getir(ipucuKapat) : Dil.Getir(ipucuAc);
        }
    }

    public bool EtkilesimeAcik { get { return hareketEden == null; } }

    public void Etkilesim(GameObject oyuncu)
    {
        Kilit kilit = GetComponent<Kilit>();
        if (kilit != null && !kilit.Cozuk && kilit.Etkilesim(oyuncu)) return;
        if (kilitli) { Cal(kilitliSesi, 0.5f); return; }
        hareketEden = StartCoroutine(Git(acik ? 0f : 1f));
    }

    private IEnumerator Git(float hedef)
    {
        // Kapanırken ses yok (#74, Şahan: kapanma sesi kalksın).
        if (hedef > 0f) Cal(acilmaSesi, 0.45f);
        float bas = oran;
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            // Yavaş başlayıp yavaş biten eğri: elle çekilen bir çekmece gibi.
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(t / sure));
            Uygula(Mathf.Lerp(bas, hedef, e));
            yield return null;
        }
        Uygula(hedef);
        acik = hedef > 0.5f;
        hareketEden = null;
    }

    /// <summary>Kilidi takar ya da çözer; kilitlenen açık parça hemen kapanır.</summary>
    public void Kilitle(bool k)
    {
        kilitli = k;
        if (kilitli && acik) Ayarla(false);
    }

    /// <summary>Parçayı sessizce aralık bırakır (kat hazırlanırken, #83: boşaltılmış buzdolabının
    /// kapısı). oran 0 kapalı, 1 tam açık. Parça "açık" sayılır: oyuncu dokununca kapanır.</summary>
    public void Aralik(float oran)
    {
        if (hareketEden != null) { StopCoroutine(hareketEden); hareketEden = null; }
        if (kilitli) return;
        acik = oran > 0f;
        Uygula(Mathf.Clamp01(oran));
    }

    /// <summary>Parçayı sessizce, bir anda açık ya da kapalı yapar (kat hazırlanırken).</summary>
    public void Ayarla(bool yeniAcik)
    {
        if (hareketEden != null) { StopCoroutine(hareketEden); hareketEden = null; }
        acik = yeniAcik && !kilitli;
        Uygula(acik ? 1f : 0f);
    }

    // Açıklık oranını (0-1) konuma ya da dönüşe çevirir. Hep KAPALI hâlden hesaplanır;
    // böylece yarıda kesilen bir hareket bile hata biriktirmez.
    private void Uygula(float o)
    {
        oran = o;
        if (hareket == Hareket.Kaydir)
            transform.localPosition = kapaliKonum + kapaliDonus * eksen.normalized * (miktar * o);
        else
            transform.localRotation = kapaliDonus * Quaternion.AngleAxis(miktar * o, eksen);
    }

    private void Cal(AudioClip klip, float ses)
    {
        if (klip != null) AudioSource.PlayClipAtPoint(klip, transform.position, ses);
    }
}
