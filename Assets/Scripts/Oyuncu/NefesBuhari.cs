// NefesBuhari.cs
// Oyuncunun fener ışığında görünen NEFES BUHARI (#136, madde 6). Kameranın üstünde durur (KafaLambasi.Start
// ekler, TozIsigi gibi); kuralı NefesKurali'nde (saf C#, testli).
//
// NASIL
// Oyun başlarken kameranın çocuğu olarak küçük bir parçacık sistemi kurulur: ağız hizasında (gözün 9 cm
// altı, 22 cm önü), ileri bakan dar bir koni. Parçacıklar toz zerreleriyle AYNI malzemeyi kullanır
// (Toz_Zerresi.mat: URP Particles/Simple Lit, yumuşak beyaz nokta): sahnede bağlı olduğu için build'de
// de vardır (#26 kuralı: adla aranmaz, referanstan alınır) ve "Lit" olduğu için yalnız ışık alınca görünür;
// fener kapalıyken zaten karanlıkta kalır. Her buğu bir "püf": 9-14 küçük parçacık (3-4,5 cm), 0,5-0,8 m/sn
// ileri, 1,0-1,4 sn yaşar, büyürken solar (size over lifetime ×3; alfa 0 → 0,04 → 0), hafifçe yükselir
// (yerçekimi -0,03). Çok ve küçük: beş iri top değil, tek bir buğu gibi okunsun.
// İlk deneme (alfa 0,22, 5-9 cm, ×3,5, 6-12 parçacık, 14 cm önde) fenerin dibinde kocaman parlak bir bulut
// oldu: parçacık lambanın kaynağına çok yakın ve "Lit" olduğu için tam ışık alır; buğu belli belirsiz olmalı.
//
// ZAMANLAMA: NefesKurali.Donem kadar beklenir, sonra NefesKurali.Guc > 0 ise püskürtülür; güç parçacık
// sayısını ve alfayı çarpar. Kontrolcü kapalıyken (not, menü, final) püf yok. Oyun saatiyle (menüde durur).
//
// YENİ KAVRAM — ParticleSystem.Emit(n): sistemin sürekli yayımı yerine, istediğimiz anda n parçacık
// doğurur. Nefes sürekli değil kesikli olduğu için sürekli yayım (rateOverTime) sıfırdır, her nefes Emit'tir.

using UnityEngine;

public class NefesBuhari : MonoBehaviour
{
    private const float AgizAsagi = 0.09f, AgizIleri = 0.22f;   // metre, kameraya göre
    private const float KoniAcisi = 14f;                        // derece
    private const float TamAlfa = 0.04f;
    private const int EnAzParcacik = 9, EnCokParcacik = 14;

    /// <summary>Test kancası: toplam püf sayısı ve son püfün gücü.</summary>
    public int ToplamPuf { get; private set; }
    public float SonGuc { get; private set; }
    /// <summary>Test kancası: -1 kapalı; 0-3 verilirse gerilim kademesi yerine bu kullanılır.</summary>
    public static int ZorlaKademe = -1;

    private ParticleSystem sistem;
    private KafaLambasi lamba;
    private BirinciSahisKontrolcu kontrolcu;
    private float sayac;
    private Color temel = new Color(0.92f, 0.95f, 1f, 1f);   // hafif mavi-beyaz: soğuk nefes

    private void Start()
    {
        lamba = GetComponent<KafaLambasi>();
        if (lamba == null) lamba = GetComponentInParent<KafaLambasi>();
        kontrolcu = GetComponentInParent<BirinciSahisKontrolcu>();
        // Malzeme: kameradaki toz zerrelerinin paylaşılan malzemesi. Toz yoksa buhar da yok.
        Transform toz = transform.Find("Toz_Zerreleri");
        ParticleSystemRenderer tozCizici = toz != null ? toz.GetComponent<ParticleSystemRenderer>() : null;
        if (tozCizici == null || tozCizici.sharedMaterial == null) { Debug.Log("[NEFES] Toz_Zerreleri yok; nefes buharı kurulmadı."); enabled = false; return; }
        Kur(tozCizici.sharedMaterial);
        sayac = NefesKurali.DonemKademe1 * 0.5f;
    }

    private void Kur(Material malzeme)
    {
        GameObject g = new GameObject("Nefes_Buhari");
        g.transform.SetParent(transform, false);
        g.transform.localPosition = new Vector3(0f, -AgizAsagi, AgizIleri);
        g.transform.localRotation = Quaternion.identity;   // koni kameranın ileri yönüne (+z) bakar
        sistem = g.AddComponent<ParticleSystem>();
        sistem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var ana = sistem.main;
        ana.loop = false;
        ana.playOnAwake = false;
        ana.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.4f);
        ana.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        ana.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.045f);
        ana.startColor = new Color(temel.r, temel.g, temel.b, TamAlfa);
        ana.gravityModifier = -0.03f;                          // buhar hafifçe yükselir
        ana.simulationSpace = ParticleSystemSimulationSpace.World;   // ağızdan çıkınca havada kalır, kafayla dönmez
        ana.maxParticles = 80;

        var yayim = sistem.emission;
        yayim.rateOverTime = 0f;                               // sürekli yayım yok; her nefes Emit

        var sekil = sistem.shape;
        sekil.shapeType = ParticleSystemShapeType.Cone;
        sekil.angle = KoniAcisi;
        sekil.radius = 0.015f;

        // Boyut: yaşamı boyunca 1 → 3 kat büyür (buğu dağılır).
        var boyut = sistem.sizeOverLifetime;
        boyut.enabled = true;
        boyut.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 3f));

        // Renk: alfa 0 → 1 → 0 (ağızdan çıkarken belirir, dağılırken solar); rengin kendisi sabit.
        var renk = sistem.colorOverLifetime;
        renk.enabled = true;
        var g1 = new Gradient();
        g1.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                   new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) });
        renk.color = new ParticleSystem.MinMaxGradient(g1);

        // Hafif türbülans: düz gitmesin, kıvrılsın.
        var gurultu = sistem.noise;
        gurultu.enabled = true;
        gurultu.strength = 0.08f;
        gurultu.frequency = 0.6f;

        var cizici = g.GetComponent<ParticleSystemRenderer>();
        cizici.sharedMaterial = malzeme;
        cizici.renderMode = ParticleSystemRenderMode.Billboard;
        cizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cizici.receiveShadows = false;
        Debug.Log("[NEFES] nefes buharı kuruldu.");
    }

    private void Update()
    {
        if (sistem == null) return;
        if (DuraklatmaMenusu.BaslikEkraninda) return;
        if (kontrolcu != null && !kontrolcu.enabled && !SaklanmaYeri.Saklaniyor) return;   // not, menü, final: donar
        Varlik v = Varlik.Ornek;
        int kademe = ZorlaKademe >= 0 ? ZorlaKademe : (v != null ? v.Kademe : 0);
        bool avda = v != null && v.AvSuruyor;
        bool kosuyor = kontrolcu != null && kontrolcu.Kosuyor;
        sayac -= Time.deltaTime;
        if (sayac > 0f) return;
        sayac = NefesKurali.Donem(kademe, kosuyor, avda);
        float guc = NefesKurali.Guc(kademe, avda, SaklanmaYeri.Saklaniyor, lamba != null && lamba.IsikYaniyor, IncelemeEkrani.Acik);
        SonGuc = guc;
        if (guc <= 0f) return;
        var ana = sistem.main;
        ana.startColor = new Color(temel.r, temel.g, temel.b, TamAlfa * Mathf.Lerp(0.6f, 1f, guc));
        int adet = Mathf.RoundToInt(Mathf.Lerp(EnAzParcacik, EnCokParcacik, guc));
        sistem.Emit(adet);
        ToplamPuf++;
    }
}
