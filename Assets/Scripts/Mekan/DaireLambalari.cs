// DaireLambalari.cs
// Dairenin tavan lambaları (#64, 4. kat: ışık ve karanlık).
//
// Bina karanlık (#51): ışığı kafa lambası taşır, dairelerin lambaları sönüktür. Her odanın
// tavanında bir ampul durur (kurucu ekler, sönük). Bir kat dosyası "bu katta şu odaların
// lambası yanabilir" derse (KatZinciriVarligi.yananOdalar), o odaların lambaları dairede
// ELEKTRİK VARKEN yanar. 4. katta daire elektriksiz başlar; oyuncu sigortayı takınca lambalar
// titreyerek yanar: katın "ışık geri geldi" anı. Öbür katlarda liste boştur, lambalar sönük kalır.
//
// Lambalar sahanlıktakilerle aynı yapıda (GrayboxKurucu.AmpulKur): aşağı bakan gölgeli bir spot,
// tavanı aydınlatan gölgesiz bir dolgu ışığı ve cam küre. Yanınca camın malzemesi parlak olanla
// değişir. Titreme bileşeni (LambaTitremesi) şiddeti bizim yazdığımız değerden alır.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaireLambalari : MonoBehaviour
{
    [SerializeField] private Transform[] lambalar;     // "Lamba_<Oda>" nesneleri (kurucu bağlar)
    [SerializeField] private Material camYanik, camSonuk;
    [SerializeField] private float siddet = 3f;        // spotun şiddeti; dolgu bunun %30'u
    private const float DolguOrani = 0.30f;

    private readonly List<string> yanabilen = new List<string>();
    // Oyuncunun düğmeyle söndürdüğü odalar (#100, IsikDugmesi). Kat değişince (Ayarla) temizlenir.
    private readonly HashSet<string> elleKapali = new HashSet<string>();
    private DaireElektrigi elektrik;

    private void OnEnable()
    {
        elektrik = GetComponent<DaireElektrigi>();
        // Olaya abone ol: elektrik gelince ya da gidince haber gelsin (+= ekler, -= çıkarır).
        if (elektrik != null) elektrik.Degisti += ElektrikDegisti;
    }

    private void OnDisable()
    {
        if (elektrik != null) elektrik.Degisti -= ElektrikDegisti;
    }

    /// <summary>Bu katta lambası yanabilen odalar (null ya da boş: hiçbiri).</summary>
    public void Ayarla(IList<string> odalar)
    {
        yanabilen.Clear();
        if (odalar != null) yanabilen.AddRange(odalar);
        elleKapali.Clear();
        StopAllCoroutines();
        foreach (Transform l in lambalar) Yak(l, YanmaliMi(l));
    }

    /// <summary>Yanan lambalar bir an titrer, sonra yanık kalır (Varlık, #86). Sönük lamba sönük kalır.</summary>
    public void Titret()
    {
        if (!isActiveAndEnabled) return;
        foreach (Transform l in lambalar)
            if (YanmaliMi(l)) StartCoroutine(TitreyerekYak(l, Random.Range(0f, 0.2f)));
    }

    private void ElektrikDegisti()
    {
        StopAllCoroutines();
        foreach (Transform l in lambalar)
        {
            bool yan = YanmaliMi(l);
            // Elektrik gelince lamba bir anda değil, eski bir ampul gibi titreyerek yanar.
            // (Nesne kapalıyken coroutine başlatılamaz; o zaman doğrudan yakılır.)
            if (yan && isActiveAndEnabled) StartCoroutine(TitreyerekYak(l, Random.Range(0f, 0.25f)));
            else Yak(l, yan);
        }
    }

    /// <summary>
    /// Düğmeye basıldı (#100): o odanın lambası yanabiliyorsa (elektrik var, oda listede) söner ya da
    /// yanar. Yanamıyorsa hiçbir şey olmaz; düğme yalnızca tıklar. Dönüş: lamba şimdi yanıyor mu.
    /// </summary>
    public bool ElleDegistir(string oda)
    {
        bool yanabilir = yanabilen.Contains(oda) && (elektrik == null || elektrik.Var);
        if (!yanabilir) return false;
        if (!elleKapali.Remove(oda)) elleKapali.Add(oda);
        foreach (Transform l in lambalar)
            if (l != null && l.name == "Lamba_" + oda) Yak(l, YanmaliMi(l));
        return !elleKapali.Contains(oda);
    }

    private bool YanmaliMi(Transform lamba)
    {
        if (lamba == null) return false;
        string oda = lamba.name.Substring("Lamba_".Length);      // "Lamba_Salon" -> "Salon"
        return yanabilen.Contains(oda) && (elektrik == null || elektrik.Var) && !elleKapali.Contains(oda);
    }

    // Coroutine: yield return satırında durur, verilen süre sonra kaldığı yerden devam eder.
    private IEnumerator TitreyerekYak(Transform lamba, float gecikme)
    {
        yield return new WaitForSeconds(gecikme);
        float[] adimlar = { 0.07f, 0.12f, 0.05f, 0.30f };        // yan, sön, yan, sön ... sonra yanık
        for (int i = 0; i < adimlar.Length; i++)
        {
            // Titreme sürerken düğmeyle söndürüldüyse (ya da elektrik gittiyse) titreme orada biter (#103).
            if (!YanmaliMi(lamba)) { Yak(lamba, false); yield break; }
            Yak(lamba, i % 2 == 0);
            yield return new WaitForSeconds(adimlar[i]);
        }
        // Sonunda lambanın olması gereken hâli: eskiden hep "yanık"tı; titreme sırasında düğmeyle
        // söndürülen lamba yanık kalıyor, düğme ise onu kapalı sanıyordu.
        Yak(lamba, YanmaliMi(lamba));
    }

    private void Yak(Transform lamba, bool yan)
    {
        if (lamba == null) return;
        Light spot = lamba.GetComponent<Light>();
        if (spot != null) spot.intensity = yan ? siddet : 0f;
        Transform dolgu = lamba.Find("Dolgu");
        Light d = dolgu != null ? dolgu.GetComponent<Light>() : null;
        if (d != null) d.intensity = yan ? siddet * DolguOrani : 0f;
        Transform cam = lamba.Find("Cam");
        MeshRenderer r = cam != null ? cam.GetComponent<MeshRenderer>() : null;
        if (r != null && camYanik != null && camSonuk != null) r.sharedMaterial = yan ? camYanik : camSonuk;
    }
}
