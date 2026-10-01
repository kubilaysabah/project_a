// FenerSekmesi.cs
// Kafa lambasının SAHTE SEKME ışığı (#89): huzmenin vurduğu noktaya konan yumuşak bir nokta ışığı,
// duvardan dönen ışık gibi odanın geri kalanını hafifçe aydınlatır.
//
// Şahan'ın No70 karşılaştırması (2026-10-01): "fenerin vurduğu duvar odanın geri kalanını
// aydınlatmıyor, huzmenin dışı simsiyah." Gerçekte ışık duvardan SEKER (dolaylı aydınlatma):
// fenerle beyaz bir duvara bakınca odanın tamamı az da olsa aydınlanır. No70 bunu Unreal'ın
// gerçek zamanlı sekme hesabıyla yapar; URP'de böyle bir hesap yok. Pişirilmiş çözümler (probe
// volume) yalnızca SABİT ışıkların sekmesini tutar; bizim ışığımız oyuncuyla gezen fener, sekmesi
// pişirilemez. Onun yerine eski bir oyun hilesi:
//   1. Her kare kameradan huzmenin yönünde bir ışın atılır; ışın neye çarpıyorsa fener oraya vuruyor.
//   2. Çarpma noktasının 25 cm önüne (yüzeyin normali yönünde) gölgesiz, kısa menzilli bir NOKTA
//      ışığı konur. Şiddeti: fener uzaktaysa az (ışık mesafeyle zayıflar), yüzey koyuysa az
//      (siyah duvar ışık geri vermez), rengi yüzeyin rengine çalar (kırmızı halı kırmızı sekme).
//   3. Nokta ve şiddet yumuşakça kayar: bakış duvardan kapı boşluğuna geçince ışık bir anda
//      sönmesin, 0,1 saniyede sönsün.
// Göz bunu "duvardan dönen ışık" diye okur; bir ışık ve bir ışın, ucuz.
//
// Kafa lambası kapalıyken ya da titrerken (IsikYaniyor) sekme de yoktur: ışık yoksa sekmesi de
// olmaz. Düşük kalitede kapalı (Ayarlar.Kalite). Işık nesnesi oyuncunun çocuğu DEĞİLDİR: dünyada
// durur; KatYoneticisi'nin "sahanlık lambası" listesine girmez (o liste spotları sayar, bu nokta).

using UnityEngine;

public class FenerSekmesi : MonoBehaviour
{
    /// <summary>Sekme ışığının menzili (metre) ve fener şiddetine oranı.</summary>
    public const float Menzil = 5.5f;
    public const float Oran = 0.22f;
    private const float Aciklik = 0.9f;        // çarpma noktasından yüzeyin önüne (duvara yakın dursa duvarda ikinci bir lamba lekesi olurdu)
    private const float GecisSuresi = 0.1f;    // konum ve şiddetin kayma süresi

    private KafaLambasi lamba;
    private Light isik;
    private Transform oyuncu;
    private float hedefSiddet;
    private Vector3 hedefNokta;
    private Color hedefRenk = KafaLambasi.Renk;
    private bool acik = true;

    private void Awake()
    {
        lamba = GetComponent<KafaLambasi>();
        BirinciSahisKontrolcu k = GetComponentInParent<BirinciSahisKontrolcu>();
        oyuncu = k != null ? k.transform : transform;

        GameObject g = new GameObject("Fener_Sekmesi");
        isik = g.AddComponent<Light>();
        isik.type = LightType.Point;
        isik.range = Menzil;
        isik.intensity = 0f;
        isik.shadows = LightShadows.None;
        isik.color = KafaLambasi.Renk;
    }

    private void OnEnable() { Ayarlar.Degisti += KaliteyiUygula; KaliteyiUygula(); }
    private void OnDisable() { Ayarlar.Degisti -= KaliteyiUygula; if (isik != null) isik.intensity = 0f; }
    private void OnDestroy() { if (isik != null) Destroy(isik.gameObject); }

    private void KaliteyiUygula() { acik = Ayarlar.Kalite != AyarKurallari.KaliteDusuk; }

    // Bastırma (#93): duvardaki gölge (GolgeIzi) sürerken sekme ışığı söner. Sekme gölgesiz bir dolgu
    // ışığıdır ve duvarın önünde durur; açık kalsa gölgenin içini aydınlatıp silerdi.
    private static float bastirmaBitis;

    /// <summary>Sekme ışığını 'sure' saniye söndürür (yumuşak geçişle).</summary>
    public static void Bastir(float sure) { bastirmaBitis = Mathf.Max(bastirmaBitis, Time.time + sure); }

    private void LateUpdate()
    {
        if (isik == null) return;
        if (!acik || lamba == null || !lamba.IsikYaniyor || Time.time < bastirmaBitis) hedefSiddet = 0f;
        else Hesapla();

        // Yumuşak geçiş: her karede kalan yolun bir kısmı (Lerp), kare hızından bağımsız.
        float t = 1f - Mathf.Exp(-Time.deltaTime / GecisSuresi);
        isik.intensity = Mathf.Lerp(isik.intensity, hedefSiddet, t);
        if (hedefSiddet > 0f)
        {
            isik.transform.position = Vector3.Lerp(isik.transform.position, hedefNokta, t);
            isik.color = Color.Lerp(isik.color, hedefRenk, t);
        }
    }

    // Işın: huzmenin ortası neye vuruyor? Oyuncunun kendi parçaları atlanır (kamera kapsülün içinde).
    private void Hesapla()
    {
        Transform yon = lamba.IsikYonu;
        RaycastHit enYakin = default; float enKisa = float.MaxValue; bool var = false;
        foreach (RaycastHit h in Physics.RaycastAll(yon.position, yon.forward, KafaLambasi.Menzil, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(oyuncu)) continue;
            if (h.distance < enKisa) { enKisa = h.distance; enYakin = h; var = true; }
        }
        if (!var) { hedefSiddet = 0f; return; }

        float mesafe = enYakin.distance;
        // Yüzeyin rengi: malzemenin ana rengi (doku varsa orta ton varsayılır). Koyu yüzey az seker.
        Color yuzey = YuzeyRengi(enYakin.collider);
        float albedo = Mathf.Clamp(0.299f * yuzey.r + 0.587f * yuzey.g + 0.114f * yuzey.b, 0.15f, 0.9f);
        // Fener uzaklaştıkça duvara düşen ışık azalır (kare ters orantı; 1,5 m'den yakını doymuş):
        // 2 m'de 0,6, 4 m'de 0,18, 6 m'de 0,08.
        float dusen = Mathf.Clamp01(3f / (mesafe * mesafe + 1f));
        hedefSiddet = KafaLambasi.Siddet * Oran * albedo * dusen * 3f;
        // Işık yüzeyden 90 cm önde durur (ama kameranın gerisine geçmez): yüzeyin kendisi değil,
        // çevresi aydınlansın; 25 cm'de duvarda parlak bir leke oluşuyordu.
        hedefNokta = enYakin.point + enYakin.normal * Mathf.Min(Aciklik, mesafe * 0.6f);
        // Sekmenin rengi: fenerin sıcak beyazı ile yüzeyin rengi karışır (ışık yüzeyin rengini alır).
        Color karisim = Color.Lerp(KafaLambasi.Renk, yuzey, 0.45f);
        float en = Mathf.Max(karisim.r, Mathf.Max(karisim.g, karisim.b));
        hedefRenk = en > 0.01f ? karisim / en : KafaLambasi.Renk;
        hedefRenk.a = 1f;
    }

    private static Color YuzeyRengi(Collider c)
    {
        Renderer r = c.GetComponent<Renderer>();
        if (r == null) r = c.GetComponentInChildren<Renderer>();
        if (r == null || r.sharedMaterial == null) return Color.gray;
        Material m = r.sharedMaterial;
        Color renk = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.gray;
        // Dokulu malzemenin ana rengi çoğu zaman beyazdır; dokunun orta tonu için yarıya çekilir.
        if (m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null) renk *= 0.6f;
        renk.a = 1f;
        return renk;
    }
}
