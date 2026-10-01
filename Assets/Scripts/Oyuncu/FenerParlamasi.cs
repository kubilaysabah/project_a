// FenerParlamasi.cs
// Görev eşyası kafa lambasının ışığında hafifçe parlar (#83, Şahan: karanlık saklama yerleri).
//
// Eşyanın şablonunda "Parilti" adlı küçük bir dörtgen (quad) çocuk vardır: ortası parlak, kenarı
// saydam, ışınlı bir yıldız dokusu, toplamalı (additive) malzemeyle. Toplamalı demek: altındaki
// rengin üstüne ışık EKLER, hiçbir şeyi karartmaz; karanlıkta küçük bir ışık gibi görünür.
//
// Her karede:
//   1. Lamba yanmıyorsa parıltı söner.
//   2. Lambadan eşyaya giden yönün lambanın baktığı yönle açısı ve mesafe ölçülür;
//      ParlamaKurali ne kadar parlayacağını söyler (koninin dışı ve 5 m'den uzak = hiç).
//   3. Arada bir engel (duvar, dolap kapağı) varsa parıltı yok: ışık eşyaya ulaşmıyor.
//   4. Parıltı kameraya döndürülür (billboard: her açıdan yüzü görünen düz resim), eşyanın 4 cm
//      önüne alınır ki çekmecenin tabanına gömülüp yarısı kaybolmasın, yavaşça kendi etrafında döner.
//
// Eşya elde taşınırken nesnesi kapalıdır (Esya.Etkilesim), bu bileşen de o sırada çalışmaz.

using UnityEngine;

public class FenerParlamasi : MonoBehaviour
{
    // Parıltının rengi: lambanın sıcak beyazı. Kâğıt parçalarında şablon daha düşük bir çarpan verir.
    [SerializeField] private float gucCarpani = 1f;
    [SerializeField] private float boy = 0.12f;   // parıltının dünyadaki genişliği (metre)

    private const float SonmeHizi = 4f;           // saniyede bu kadar yaklaşır (ani yanıp sönmesin)
    private const float OneAlma = 0.07f;          // kameraya doğru bu kadar öne alınır (metre)
    // HDR parlaklık: 1'in üstündeki renk son işlemenin ışıma (bloom) efektini tetikler. 1 iken gece
    // ton eşlemesi parıltıyı karanlığın içinde neredeyse siliyordu (denetim fotoğrafı).
    private const float Parlaklik = 4f;

    private static KafaLambasi lamba;             // sahnede tek lamba: bir kez bulunur, herkes paylaşır
    private Transform parilti;
    private Esya esya;                            // bu eşya (kabı açık mı diye sorulur)
    private Renderer cizici;
    private Vector3 yerel;                        // parıltının şablondaki yeri (eşyaya göre)
    private float faz;                            // kırpışma fazı: her eşyada farklı
    private float mevcut;                         // şu anki güç (hedefe yumuşakça yaklaşır)
    private MaterialPropertyBlock blok;

    // ---------- ZORLA PARLAMA (#98) ----------
    // Takılan oyuncuya bina yazısız ipucu verir (TakilmaIpucu): eşya, lambanın ışığında olmasa da
    // kısa bir an parlar. Parlama bir "nabız" gibi gelir ve gider: sinüs eğrisinin yarım tepesi.
    private const float ZorlaGuc = 0.8f;          // nabzın tepesindeki güç (ışıktaki en güçlü parıltı 1)
    private float zorlaKalan;                     // zorla parlamanın kalan süresi (saniye)
    private float zorlaSure;                      // zorla parlamanın toplam süresi (saniye)

    /// <summary>
    /// Eşyayı lambanın ışığından bağımsız olarak 'sure' saniye parlatır (#98). Kabı kapalıysa
    /// (kapalı çekmece, buzdolabı) yine parlamaz: içeride ne olduğunu ele vermesin.
    /// </summary>
    public void Parla(float sure)
    {
        if (!(sure > 0f)) return;   // sıfır, eksi ya da bozuk (NaN) süre: hiçbir şey yapma
        zorlaSure = sure;
        zorlaKalan = sure;
    }

    private void Awake()
    {
        parilti = transform.Find("Parilti");
        if (parilti == null) { enabled = false; return; }
        cizici = parilti.GetComponent<Renderer>();
        esya = GetComponent<Esya>();
        yerel = parilti.localPosition;
        // Rastgele bir faz: aynı sahnede iki eşya aynı anda kırpışmasın. Oynanışı etkilemeyen, yalnızca
        // görüntüye ait bir değer olduğu için tohumlu üretici gerekmez (UnityEngine.Random yeter).
        faz = Random.Range(0f, 10f);
        blok = new MaterialPropertyBlock();
        cizici.enabled = false;
    }

    private void OnDisable()
    {
        mevcut = 0f;
        zorlaKalan = 0f;   // eşya ele alındı ya da kat değişti: yarım kalan zorla parlama da biter
        if (cizici != null) cizici.enabled = false;
    }

    private void LateUpdate() { Guncelle(false); }

    /// <summary>
    /// Parıltıyı bu karenin lambasına göre günceller. aninda: gücü yumuşatmadan hemen hedefe getirir
    /// (kat denetimi kamerayı taşıyıp aynı karede fotoğraf çeker; yumuşatma orada işe yaramaz).
    /// </summary>
    public void Guncelle(bool aninda)
    {
        if (parilti == null) return;
        if (lamba == null) lamba = FindAnyObjectByType<KafaLambasi>();
        Vector3 merkez = transform.TransformPoint(yerel);

        float hedef = 0f;
        // Kabı kapalıysa (kapalı çekmece, buzdolabı, sandık) parlamaz: parıltı kameraya doğru öne
        // alındığı için ince bir kapağın önüne taşıp içeride ne olduğunu ele verebilirdi.
        // EtkilesimeAcik tam da "kabı açık mı, alınabilir mi" sorusunu cevaplar (Esya).
        bool kabiAcik = esya == null || esya.EtkilesimeAcik;
        if (kabiAcik && lamba != null && lamba.IsikYaniyor)
        {
            Transform kaynak = lamba.IsikYonu;
            Vector3 fark = merkez - kaynak.position;
            float mesafe = fark.magnitude;
            float aci = Vector3.Angle(kaynak.forward, fark);
            hedef = ParlamaKurali.Guc(aci, mesafe, KafaLambasi.DisAci / 2f, ParlamaKurali.Menzil);
            // Işık eşyaya ulaşıyor mu? Ölçüm pahalı olduğu için yalnızca konideyse bakılır.
            if (hedef > 0f && !IsikUlasiyor(kaynak.position, merkez, mesafe)) hedef = 0f;
        }
        // Zorla parlama (#98): lambadan bağımsız, ama kap kapalıysa yine yok. Duvarın arkasındaki
        // eşyanın parıltısını derinlik testi zaten gizler (parıltı da sahnedeki her yüzey gibi
        // önündeki duvarın arkasında kalır), ayrıca ışın atmaya gerek yok.
        if (zorlaKalan > 0f)
        {
            // Kat denetimi (aninda) aynı karede fotoğraf çeker; orada zaman ilerletilmez.
            if (!aninda) zorlaKalan -= Time.deltaTime;
            if (kabiAcik && zorlaSure > 0f)
            {
                // t: 0'dan 1'e ilerleyen oran; Sin(t·π) 0'dan başlar, ortada 1 olur, sonda 0'a döner.
                float t = 1f - Mathf.Clamp01(zorlaKalan / zorlaSure);
                hedef = Mathf.Max(hedef, ZorlaGuc * Mathf.Sin(t * Mathf.PI));
            }
        }
        // MoveTowards: değeri hedefe en fazla "adım" kadar yaklaştırır; parıltı yumuşakça gelir, gider.
        mevcut = aninda ? hedef : Mathf.MoveTowards(mevcut, hedef, SonmeHizi * Time.deltaTime);
        if (mevcut < 0.01f) { cizici.enabled = false; return; }

        Camera kamera = Camera.main;
        if (kamera == null) { cizici.enabled = false; return; }
        Vector3 goz = kamera.transform.position;
        cizici.enabled = true;
        parilti.position = merkez + (goz - merkez).normalized * OneAlma;
        // Dörtgenin yüzü -Z'ye bakar: ileri yönünü kameradan uzağa çevirince yüzü kameraya döner.
        parilti.rotation = Quaternion.LookRotation(parilti.position - goz) * Quaternion.Euler(0f, 0f, Time.time * 25f + faz * 50f);
        parilti.localScale = Vector3.one * boy / Mathf.Max(0.001f, transform.lossyScale.x);

        float parlaklik = mevcut * ParlamaKurali.Kirpisma(Time.time, faz) * gucCarpani;
        cizici.GetPropertyBlock(blok);
        blok.SetColor("_BaseColor", KafaLambasi.Renk * parlaklik * Parlaklik);
        cizici.SetPropertyBlock(blok);
    }

    // Lambadan eşyaya giden ışının önünde başka bir şey var mı? Oyuncunun kendi gövdesi, eşyanın
    // kendi çarpışma kutusu ve eşyayı İÇİNE alan kutular (kavanoz, tezgâhın kutusu: Etkilesimci'deki
    // IcindeMi kuralıyla aynı sebep) engel sayılmaz. Eşyanın dibindeki 5 cm de sayılmaz.
    private bool IsikUlasiyor(Vector3 kaynak, Vector3 merkez, float mesafe)
    {
        Vector3 yon = (merkez - kaynak) / Mathf.Max(0.0001f, mesafe);
        Transform oyuncu = lamba.transform.root;
        foreach (RaycastHit h in Physics.RaycastAll(kaynak, yon, mesafe, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (t.IsChildOf(oyuncu) || t.IsChildOf(transform)) continue;
            if (h.distance > mesafe - 0.05f) continue;
            if (h.collider.bounds.Contains(merkez)) continue;
            return false;
        }
        return true;
    }
}
