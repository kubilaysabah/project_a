// SahanlikKapisi.cs
// Sahanlıktaki daire kapısını çalmak (#67, 7. kat: gözlem).
//
// 7. katta dört dairenin kapısında da "13" yazar ve dördü de kapalıdır. Yalnızca biri gerçektir
// (Cemal'in dairesi); oyuncu hangisi olduğunu Cemal'in notuyla ve kapılardaki küçük farklarla
// (paspas, terlikler, gözetleme deliğinden sızan ışık) bulur. Kapıya bakınca "KAPIYI ÇAL":
//   gerçek kapı : tık tık, sonra gıcırdayarak içeri açılır; kayda yazılır (bir daha çalınmaz).
//   sahte kapı  : tık tık, kısa bir sessizlik, sonra kapının ardından bir ses (çarpan kapı,
//                 tersten ağlama...) ve kafa lambası titrer. Oyuncu yanıldığını anlar.
// Eski oyunun "fark et" duygusu burada bir olay olarak geri gelir; ceza yok (Şahan'ın seçimi).
//
// Bu bileşeni KatYoneticisi, kat dosyası "giriş kapalı" diyen katlarda sahanlıktaki dört kapıya
// ekler, öbür katlarda söker.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SahanlikKapisi : MonoBehaviour, IEtkilesimli
{
    private const float AcikAci = 80f;         // gerçek kapının açılınca duracağı açı (içeri)
    private const float AcilmaSuresi = 1.4f;
    private const float Bekleme = 3f;          // sahte kapı üst üste çalınamasın

    private bool gercek;
    private int kat;
    private string kayitKimligi;               // gerçek kapı açılınca kayda yazılan ad
    private List<AudioClip> olaySesleri;
    private Transform mentese;
    private GameObject karanlikHol;            // kapının ardındaki karanlık kutu (Daire_Ici)
    private bool acildi;
    private float sonrakiCalma;
    private AudioClip tiklatma;                // kat dosyasından (bağlı referans: build'e girsin)
    private static AudioClip gicirti;

    public void Hazirla(bool gercekKapi, int katNo, string kayitAdi, AudioClip tiklatmaSesi, List<AudioClip> sesler)
    {
        tiklatma = tiklatmaSesi;
        gercek = gercekKapi;
        kat = katNo;
        kayitKimligi = kayitAdi;
        olaySesleri = sesler;
        mentese = transform.Find("Mentese");
        Transform hol = transform.Find("Daire_Ici");
        karanlikHol = hol != null ? hol.gameObject : null;
        // Ahşap daire kapısı: oda kapısının gıcırtısı (#99; eskiden demir parmaklıklı kat kapısınındı).
        if (gicirti == null) gicirti = Resources.Load<AudioClip>("Sesler/IcKapi_Acilma");
        acildi = false;
    }

    public string Ipucu { get { return Dil.Getir("ipucu.cal"); } }

    public bool EtkilesimeAcik { get { return !acildi && Time.time >= sonrakiCalma; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        sonrakiCalma = Time.time + Bekleme;
        if (tiklatma != null) AudioSource.PlayClipAtPoint(tiklatma, KapiNoktasi(), 0.8f);
        StartCoroutine(gercek ? Ac() : Olay());
    }

    // Gerçek kapı: tıklatmadan sonra kendiliğinden, gıcırdayarak açılır.
    private IEnumerator Ac()
    {
        acildi = true;
        Kayit.KilitCozuldu(kat, kayitKimligi);
        AltIsigiSondur();
        yield return new WaitForSeconds(0.9f);
        if (gicirti != null) AudioSource.PlayClipAtPoint(gicirti, KapiNoktasi(), 0.8f);
        Quaternion bas = mentese != null ? mentese.localRotation : Quaternion.identity;
        Quaternion son = Quaternion.Euler(0f, AcikAci, 0f);   // pozitif açı içeri açar (menteşeler aynalı)
        for (float t = 0f; t < AcilmaSuresi; t += Time.deltaTime)
        {
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t / AcilmaSuresi);
            if (mentese != null) mentese.localRotation = Quaternion.Slerp(bas, son, e);
            yield return null;
        }
        if (mentese != null) mentese.localRotation = son;
        Debug.Log($"[KAPI] {kat}. kat: gerçek kapı ({name}) açıldı.");
    }

    // Sahte kapı: kısa bir sessizlikten sonra ardından bir ses gelir, kafa lambası titrer.
    private IEnumerator Olay()
    {
        yield return new WaitForSeconds(1.1f);
        if (olaySesleri != null && olaySesleri.Count > 0)
        {
            AudioClip ses = olaySesleri[Random.Range(0, olaySesleri.Count)];
            if (ses != null) AudioSource.PlayClipAtPoint(ses, KapiNoktasi(), 0.9f);
        }
        KafaLambasi lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Titret(1.4f);
        Varlik.Gurultu(0.3f);    // yanlış kapıyı çaldı: bina duydu (#86)
        // Yanlış kapının bedeli (#83): "ardından cevap veriyorlar". Lamba titrerken Gölge oyuncunun önünde
        // belirir ve bir süre peşinden gelir (GezenGolge; zarar vermez, korkutur). Yer bulamazsa belirmez.
        GezenGolge gezen = FindAnyObjectByType<GezenGolge>();
        bool belirdi = gezen != null && gezen.Belir();
        Debug.Log($"[KAPI] {kat}. kat: yanlış kapı ({name}) çalındı; olay{(belirdi ? ", Gölge belirdi" : "")}.");
    }

    // Kapının altından sızan ışık (7. katın kartı açar) kanatla döner; kapı açılınca söndürülür.
    private void AltIsigiSondur()
    {
        Transform alt = mentese != null ? mentese.Find("Kapi_Alti_Isik") : null;
        if (alt != null) alt.gameObject.SetActive(false);
    }

    /// <summary>Kayıttan: gerçek kapı daha önce açılmışsa açık başlar, çalınmaz.</summary>
    public void AcikBaslat()
    {
        acildi = true;
        AltIsigiSondur();
        if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, AcikAci, 0f);
    }

    // Sesin çıkacağı yer: kapı kanadı (menteşe). Kapının kökü sahanlığın ortasında durur (CLAUDE.md).
    private Vector3 KapiNoktasi() { return mentese != null ? mentese.position : transform.position; }
}
