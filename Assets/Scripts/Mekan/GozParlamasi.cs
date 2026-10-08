// GozParlamasi.cs
// Üç harfli figürlerinin KARANLIKTA PARLAYAN GÖZLERİ (#135, madde 1).
//
// NEDEN
// Şahan'ın 2026-10-08 test turu: belirme 3 m'de, fenerin parlak konisinin dışında soluk kalıyordu; figür
// duvarla aynı tondaydı. Gece bir hayvanın gözü farın ışığını geri yansıtır ("göz parıltısı"); biz bunun
// tersini yaparız: gözler karanlıkta kendiliğinden parlar, fener ÜSTLERİNE gelince sönerler. Böylece oyuncu
// karanlıkta uzaktan iki ışıklı nokta görür, fenerini çevirince noktalar söner ve o yerde bir yüz belirir.
// Karartmalı belirmede (madde 2) lamba ölünce bu gözler yaklaşır: oyuncunun karanlıkta gördüğü tek şey.
//
// NASIL
// Bileşen figürün prefab kökünde durur (FigurKurucu ekler). Oyun başlarken iki küçük küre yaratır: konumları
// figürün kendi eksenlerinde ölçülmüş göz merkezleridir (FigurKurucu, modelin köşe noktalarından: başın
// %88-95 yükseklik bandının en öndeki noktası). Küreler URP Unlit malzemeyle çizilir: ışıksız ortamda da
// tam renkte görünürler (bina zifiri karanlık, #130). Renk ve HDR parlaklık MaterialPropertyBlock ile
// verilir, tek malzeme paylaşılır (#116'nın kat ışığı kaynağıyla aynı malzeme, Kat_Isik_Kaynagi.mat).
//
// Her kare: gözün ortası kafa lambasının konisinin içinde ve 9 m'den yakınsa hedef 0 (sönük), değilse 1
// (parlak); küreler hedefe doğru ölçeklenir (0,1 sn'de söner). Fener kapalıyken hep parlak.
//
// YENİ KAVRAM — MaterialPropertyBlock: bir malzemeyi kopyalamadan tek bir çizime özel değerler vermenin
// yolu. renderer.material.color yazmak malzemeyi gizlice kopyalar ve sızdırır (#5'teki kural); blok yazmak
// kopyalamaz. Burada iki küre de aynı malzemeyi kullanır, rengi bloktan alır.

using UnityEngine;

public class GozParlamasi : MonoBehaviour
{
    [SerializeField] private Material malzeme;        // URP Unlit (Kat_Isik_Kaynagi.mat); FigurKurucu bağlar
    [SerializeField] private Vector3 solGoz, sagGoz;  // göz merkezleri, figürün kendi eksenlerinde (metre)
    [SerializeField] private float yaricap = 0.011f;  // göz küresinin yarıçapı (bir insan gözbebeği ~1 cm)

    public static readonly Color Renk = new Color(1f, 0.80f, 0.42f);   // soluk kehribar: gece hayvan gözü
    public const float Parlaklik = 2.6f;        // HDR çarpanı: gece son işlemesi 0,2'nin altını ezer (#16), bu üstünde kalır
    public const float SonmeHizi = 10f;         // oran/sn: fener üstüne gelince ~0,1 sn'de söner, çekilince ~0,1 sn'de yanar
    public const float FenerMenzili = 9f;       // bu uzaklıktan ötede fener gözü "yakalayamaz" (huzme zayıf): parlak kalır
    public const float KoniPayi = 4f;           // koninin kenarına eklenen pay (derece): kenarda titreşmesin

    /// <summary>Test kancası: -1 kapalı; 0-1 verilirse fener hesabı yerine bu oran uygulanır.</summary>
    public static float ZorlaOran = -1f;

    private readonly Transform[] gozler = new Transform[2];
    private float oran = 1f;                    // 0 = sönük, 1 = tam parlak
    private KafaLambasi lamba;

    /// <summary>Şu anki parlaklık oranı (0-1); test göstergesi.</summary>
    public float Oran { get { return oran; } }

    /// <summary>FigurKurucu çağırır: malzeme ve ölçülen göz yerleri (figürün kendi eksenlerinde).</summary>
    public void Kur(Material m, Vector3 sol, Vector3 sag)
    {
        malzeme = m;
        solGoz = sol;
        sagGoz = sag;
    }

    private void Start()
    {
        // #140: duvardaki gölge (GolgeIzi) figürü yalnız gölge olarak çizer (ShadowsOnly); o zaman göz de olmaz,
        // yoksa duvarın önünde havada iki nokta yanardı.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) { enabled = false; return; }
        lamba = FindAnyObjectByType<KafaLambasi>();
        gozler[0] = KureKur("Goz_Sol", solGoz);
        gozler[1] = KureKur("Goz_Sag", sagGoz);
    }

    // Bir göz küresi: ilkel küre, çarpışma kutusu silinir (oyuncunun nişanı ve yürüyüşü takılmasın), gölge
    // düşürmez (bir cm'lik kürenin gölgesi gürültüden başka bir şey değildir), rengi bloktan.
    private Transform KureKur(string ad, Vector3 yerel)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.name = ad;
        g.layer = gameObject.layer;
        Destroy(g.GetComponent<Collider>());
        // #140: gözler gövdenin ("Govde") çocuğu: FigurYurumesi gövdeyi eğince gözler de eğilir. Gövde dururken
        // kökle aynı yerde ve aynı açıda olduğu için kök ekseninde ölçülen göz yerleri olduğu gibi geçerli.
        Transform ebeveyn = transform.Find("Govde");
        if (ebeveyn == null) ebeveyn = transform;
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = yerel;
        g.transform.localScale = Vector3.one * (yaricap * 2f);
        var r = g.GetComponent<MeshRenderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        if (malzeme != null) r.sharedMaterial = malzeme;
        var blok = new MaterialPropertyBlock();
        blok.SetColor("_BaseColor", Renk * Parlaklik);
        r.SetPropertyBlock(blok);
        return g.transform;
    }

    // LateUpdate: lamba kendi konumunu LateUpdate'te günceller (KafaLambasi); ondan sonra okumak bir kare
    // gecikmeyi önler.
    private void LateUpdate()
    {
        float hedef = ZorlaOran >= 0f ? ZorlaOran : (Fenerde() ? 0f : 1f);
        oran = Mathf.MoveTowards(oran, hedef, SonmeHizi * Time.deltaTime);
        float olcek = yaricap * 2f * oran;
        for (int i = 0; i < gozler.Length; i++)
            if (gozler[i] != null) gozler[i].localScale = Vector3.one * olcek;
    }

    // Gözlerin ortası kafa lambasının huzmesinde mi? Lamba yanmıyorsa hayır (karanlıkta hep parlar).
    // Koni: KafaLambasi.DisAci dış açıdır (64°), yarısı eksenden kenara olan açı; küçük bir pay eklenir.
    private bool Fenerde()
    {
        if (lamba == null || !lamba.IsikYaniyor) return false;
        Transform kaynak = lamba.IsikYonu;
        // Gözlerin gerçek ortası (gövde eğilmiş olabilir); küreler yoksa ölçülen yerden.
        Vector3 goz = gozler[0] != null && gozler[1] != null ? (gozler[0].position + gozler[1].position) * 0.5f
                                                             : transform.TransformPoint((solGoz + sagGoz) * 0.5f);
        Vector3 fark = goz - kaynak.position;
        if (fark.magnitude > FenerMenzili) return false;
        return Vector3.Angle(kaynak.forward, fark) < KafaLambasi.DisAci * 0.5f + KoniPayi;
    }
}
