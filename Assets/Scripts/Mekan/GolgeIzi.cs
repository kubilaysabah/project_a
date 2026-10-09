// GolgeIzi.cs
// Duvardaki gölge (#93): kafa lambası, odada OLMAYAN birinin gölgesini duvara düşürür.
//
// Şahan'ın No70 / Phasmophobia karşılaştırması (2026-10-01, grafik 3): "oyuncu önce duvardaki
// gölgeyi görsün, dönüp bakınca hiçbir şey olmasın." Bu türün en etkili hilelerinden biri ve
// gerekli her şey zaten vardı: gölge atan bir kafa lambası (#51) ve yürüyebilen bir Gölge (#90).
//
// NASIL: Gölge şablonunun bir kopyası lamba ile duvar arasına, huzmenin içine konur. Kopyanın bütün
// çizicileri "YALNIZCA GÖLGE" kipindedir (ShadowCastingMode.ShadowsOnly): Unity onu kameraya hiç
// çizmez ama ışığın gölge hesabına katar. Bir sahne ışığının önüne görünmez bir karton insan
// koymak gibi: kartonu kimse görmez, duvardaki gölgesini herkes görür. Kopya huzmenin içinde
// yana doğru 1,8 m yürür (GolgeAnimasyonu yürüyüşü kendisi yapar), 1,5 sn sonra yok olur.
//
// Ne zaman olmaz: lamba kapalıysa ya da gölgesizse (Düşük kalite), huzme 1,8 m'den yakın ya da
// 4,2 m'den uzak bir yüzeye vuruyorsa (çok yakında gölge dev, çok uzakta seçilmez), vurulan yüzey bir
// duvar değilse (zemin, tavan). O zaman false döner; Varlık onun yerine lambayı titretir.

using System.Collections;
using UnityEngine;

public class GolgeIzi : MonoBehaviour
{
    // Huzmenin vurduğu yüzeyin uzaklığı. Kafa lambası bilerek kısık (#51: 3,5 şiddet): 4 m'den uzak bir
    // duvarı ancak seçilecek kadar aydınlatır, üstündeki gölge kaybolur (ilk denemede 3,9 m'de görüldü).
    private const float EnYakin = 1.8f, EnUzak = 4.2f;
    // Gölgeyi atan kopya lamba-duvar yolunun %72'sinde durur. İlk denemede %45'teydi: lambaya o kadar
    // yakın bir insan bütün huzmeyi kapattı, oyuncu yalnızca "lamba söndü" sandı. Duvara yakın dururken
    // gölgesi kendi boyunun ~1,4 katı olur ve aydınlık lekenin içinde insan biçimi seçilir.
    private const float Oran = 0.72f;
    private const float Yol = 2.4f, Sure = 1.6f;       // yana yürüdüğü yol (m: lekeye girip çıkar) ve süre (sn)

    private Vector3 bas, son;

    /// <summary>
    /// Duvara yürüyen bir gölge düşürmeyi dener. sablon: Gölge şablonu; lamba: oyuncunun kafa lambası.
    /// Koşullar tutmazsa false (hiçbir şey kurulmaz).
    /// </summary>
    public static bool Goster(GameObject sablon, KafaLambasi lamba, Transform oyuncu)
    {
        if (sablon == null || lamba == null || !lamba.IsikYaniyor) return false;
        if (FigurYurumesi.SahnedeBaskasiVar(null)) return false;   // #142 (A3): sahnede başka bir figür var: lamba titrer
        Light isik = lamba.IsikYonu.GetComponent<Light>();
        if (isik == null || isik.shadows == LightShadows.None) return false;   // gölgesiz lamba: gölge yok

        // Huzmenin ortası neye vuruyor? Oyuncunun kendi parçaları atlanır.
        Transform yon = lamba.IsikYonu;
        RaycastHit vurus = default; float enKisa = float.MaxValue; bool var = false;
        foreach (RaycastHit h in Physics.RaycastAll(yon.position, yon.forward, EnUzak, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(oyuncu)) continue;
            if (h.distance < enKisa) { enKisa = h.distance; vurus = h; var = true; }
        }
        if (!var || vurus.distance < EnYakin) return false;
        if (Mathf.Abs(vurus.normal.y) > 0.5f) return false;   // zemin ya da tavan: insan gölgesi oraya düşmez

        // Kopyanın yeri: huzme yolunun %45'i; ayakları o noktanın altındaki ZEMİNDE. Aşağı inen ışın önce
        // bir sehpaya, halıya çarpabilir; oyuncu da bir halının, eşiğin üstünde duruyor olabilir. Bu yüzden
        // oyuncunun ayağının 1 m altı ile 30 cm üstü arasındaki çarpmaların EN ALÇAĞI zemin sayılır
        // (alt kattaki sahanlık 3 m aşağıda, bu aralığa girmez). Kopya görünmez olduğu için bir sehpanın
        // içinde durması önemli değil.
        Vector3 orta = yon.position + yon.forward * (vurus.distance * Oran);
        bool zeminVar = false;
        float zeminY = float.MaxValue;
        foreach (RaycastHit h in Physics.RaycastAll(orta + Vector3.up * 0.5f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore))
        {
            float fark = h.point.y - oyuncu.position.y;
            if (fark > 0.3f || fark < -1f) continue;   // oyuncunun hizasında değil (yüksek eşya, başka kat)
            if (h.point.y < zeminY) { zeminY = h.point.y; zeminVar = true; }
        }
        if (!zeminVar) return false;   // merdiven boşluğu ya da başka kat
        Vector3 ayak = new Vector3(orta.x, zeminY, orta.z);

        // Yana yürüme yönü: huzmeye dik, yatay. Hangi yöne yürüyeceği rastgele.
        Vector3 ileri = yon.forward; ileri.y = 0f; ileri.Normalize();
        Vector3 yan = Vector3.Cross(Vector3.up, ileri) * (Random.value < 0.5f ? 1f : -1f);

        GameObject g = Instantiate(VarlikGorunusu.Sablon(sablon), ayak - yan * (Yol / 2f), Quaternion.LookRotation(yan));   // #140: figür torbası (VarlikGorunusu)
        GolgeImzasi.Cal(ayak + Vector3.up * 1.2f, 0.6f);   // imza (#105): gölgeyi atan, görülmeyen yerden
        g.name = "Golge_Izi";
        // Kameraya hiç çizilmesin, yalnızca ışığın gölgesine girsin.
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        GolgeIzi iz = g.AddComponent<GolgeIzi>();
        // Fenerin sekme ışığı (#89) gölgesiz bir dolgu: açık kalırsa gölgenin içini aydınlatıp siler.
        FenerSekmesi.Bastir(Sure + 0.3f);
        iz.bas = ayak - yan * (Yol / 2f);
        iz.son = ayak + yan * (Yol / 2f);
        Debug.Log($"[VARLIK] duvarda gölge: duvar {vurus.distance:0.0} m, gölgeyi atan {vurus.distance * Oran:0.0} m önde.");
        return true;
    }

    private IEnumerator Start()
    {
        // Yavaş başlayıp yavaş biten bir yürüyüş; GolgeAnimasyonu konum değişiminden adımları üretir.
        for (float t = 0f; t < Sure; t += Time.deltaTime)
        {
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(t / Sure));
            transform.position = Vector3.Lerp(bas, son, e);
            yield return null;
        }
        Destroy(gameObject);
    }
}
