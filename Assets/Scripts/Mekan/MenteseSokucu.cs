// MenteseSokucu.cs
// "Tornavida" kilidi (#54, Docs/hikaye.md §6): kilidi açılmayan kapının menteşeleri sökülür.
//
// Zincir uygulayıcısı kapının KORİDOR tarafına, menteşe kenarına iki menteşe plakası takar;
// her plakada iki vida. Oyuncunun elinde doğru tornavida varken kapıya her basışında bir vida
// dönerek çıkar ve yere düşer. Dört vida da çıkınca kapı odanın içine devrilir, yol açılır.
// (Gerçekte menteşe pimleri kapının açıldığı taraftadır; oyunda oyuncunun göreceği tarafa
// koyuyoruz: bulmacanın cevabı gözünün önünde olsun.)
//
// Kapının menteşesi (Mentese nesnesi) bütünüyle devrilir: kanat, kollar ve plakalar birlikte.
// Kat değişince uygulayıcı kapıyı ve vidaları eski hâline döndürür (Sifirla).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenteseSokucu : MonoBehaviour
{
    private IcKapi kapi;
    private Transform mentese;
    private readonly List<Transform> vidalar = new List<Transform>();
    private readonly List<GameObject> plakalar = new List<GameObject>();
    private int sokulen;
    private bool devrildi;
    private bool mesgul;
    private Vector3 ilkKonum;
    private Quaternion ilkDonus;
    private static AudioClip vidaSesi, devrilmeSesi;

    public bool Devrildi { get { return devrildi; } }

    /// <summary>İki menteşe plakasını kapıya takar; takılan plakaları döndürür (temizlik için).</summary>
    public List<GameObject> Hazirla(IcKapi hedefKapi, GameObject plakaSablonu)
    {
        kapi = hedefKapi;
        plakalar.Clear();
        vidalar.Clear();
        sokulen = 0;
        devrildi = false;
        if (kapi == null || plakaSablonu == null) return plakalar;
        foreach (float y in new[] { 0.28f, 1.72f })
        {
            if (!kapi.OnNoktasi(0.035f, y, out Vector3 konum, out Vector3 disaYon, out Transform ebeveyn)) continue;
            mentese = ebeveyn;
            GameObject p = Instantiate(plakaSablonu, ebeveyn);
            p.name = "Mentese_Plakasi";
            p.transform.position = konum;
            // Plakanın menteşe borusu modelin +X tarafında: menteşe kenarına (menteşeye doğru)
            // baksın. Tersine düşerse plakayı ters çeviriyoruz (plaka simetrik, vidalar yer değişir).
            Quaternion d = Quaternion.LookRotation(disaYon, Vector3.up);
            Vector3 menteseyeDogru = (ebeveyn.position - konum); menteseyeDogru.y = 0f;
            if (Vector3.Dot(d * Vector3.right, menteseyeDogru) < 0f) d = Quaternion.LookRotation(disaYon, Vector3.down);
            p.transform.rotation = d;
            plakalar.Add(p);
            foreach (Transform c in p.transform) if (c.name.StartsWith("Vida")) vidalar.Add(c);
        }
        if (mentese != null) { ilkKonum = mentese.localPosition; ilkDonus = mentese.localRotation; }
        return plakalar;
    }

    /// <summary>Bir vida söker. Son vida da çıktıysa kapı devrilir ve true döner.</summary>
    public bool VidaSok()
    {
        if (devrildi || mesgul) return devrildi;
        if (sokulen < vidalar.Count)
        {
            StartCoroutine(VidayiCikar(vidalar[sokulen]));
            sokulen++;
        }
        if (sokulen >= vidalar.Count)
        {
            StartCoroutine(Devril());
            return true;
        }
        return false;
    }

    private IEnumerator VidayiCikar(Transform vida)
    {
        if (vidaSesi == null) vidaSesi = Resources.Load<AudioClip>("Sesler/Vida_Sokme");
        if (vidaSesi != null) AudioSource.PlayClipAtPoint(vidaSesi, vida.position, 0.6f);
        // Vida kendi ekseninde dönerek 1,5 cm dışarı çıkar, sonra düşüp kaybolur.
        Vector3 bas = vida.localPosition;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            vida.localPosition = bas + Vector3.forward * (0.015f * t / 0.5f);
            vida.localRotation = Quaternion.Euler(0f, 0f, -720f * t);
            yield return null;
        }
        vida.gameObject.SetActive(false);
    }

    // Kapı, alt kenarının oda tarafı etrafında, tepesi odaya doğru devrilir.
    private IEnumerator Devril()
    {
        mesgul = true;
        yield return new WaitForSeconds(0.6f);
        devrildi = true;
        if (kapi != null) kapi.Devir();
        if (mentese == null) { mesgul = false; yield break; }
        kapi.DevrilmeEkseni(out Vector3 nokta, out Vector3 eksen, out float isaret);
        if (devrilmeSesi == null) devrilmeSesi = Resources.Load<AudioClip>("Sesler/Kapi_Devrilme");
        float toplam = 0f;
        const float Hedef = 86f;
        // Hızlanarak düşer (serbest düşme gibi): her karede açı hızı artar.
        for (float t = 0f; toplam < Hedef; t += Time.deltaTime)
        {
            float adim = Mathf.Min(Hedef - toplam, (40f + 400f * t) * Time.deltaTime);
            mentese.RotateAround(nokta, eksen, adim * isaret);
            toplam += adim;
            yield return null;
        }
        if (devrilmeSesi != null) AudioSource.PlayClipAtPoint(devrilmeSesi, nokta, 0.9f);
        mesgul = false;
    }

    /// <summary>Kaydedilmiş bir kapıyı sessizce devrik başlatır (adım 7).</summary>
    public void DevrikBaslat()
    {
        foreach (Transform v in vidalar) v.gameObject.SetActive(false);
        sokulen = vidalar.Count;
        devrildi = true;
        if (kapi == null || mentese == null) return;
        kapi.Devir();
        kapi.DevrilmeEkseni(out Vector3 nokta, out Vector3 eksen, out float isaret);
        mentese.RotateAround(nokta, eksen, 86f * isaret);
    }

    /// <summary>Kat değişince: kapı yerine döner, vidalar yerinde, plakalar sökülür.</summary>
    public void Sifirla()
    {
        StopAllCoroutines();
        mesgul = false;
        if (mentese != null) { mentese.localPosition = ilkKonum; mentese.localRotation = ilkDonus; }
        if (kapi != null) kapi.Kaldir();
        foreach (GameObject p in plakalar) if (p != null) Destroy(p);
        plakalar.Clear();
        vidalar.Clear();
        devrildi = false;
        sokulen = 0;
    }
}
