// PosetUcusu.cs
// Toplanan poşetin kısa "ele gelme" hareketi (#43).
//
// Şahan: poşet E'ye basınca anında kayboluyordu; toplamanın bir hissi yoktu. Artık poşet
// yerden kalkıp kameranın sağ altına (kapıcının eline) doğru süzülüyor, küçülüyor ve
// kayboluyor; 0,35 saniye sürüyor.
//
// Asıl poşet (CopPoseti) oyun kuralı açısından ANINDA alınmış ve gizlenmiş oluyor; bu
// bileşen yalnızca onun görsel bir kopyasını uçuruyor. Böylece çöp planı, görev kontrolü
// ve kat değişimi hiçbir şekilde bu hareketi beklemiyor.

using UnityEngine;

public class PosetUcusu : MonoBehaviour
{
    private const float Sure = 0.35f;

    private Vector3 baslangic;
    private Vector3 baslangicOlcek;
    private Quaternion baslangicDonus;
    private float gecen;

    /// <summary>
    /// Verilen poşetin görünüşünün bir kopyasını üretip ele doğru uçurur.
    /// Kopya yalnızca görünüştür: çarpışması, bileşenleri yok.
    /// </summary>
    public static void Baslat(Transform poset)
    {
        MeshFilter kaynakMesh = poset.GetComponent<MeshFilter>();
        MeshRenderer kaynakCizici = poset.GetComponent<MeshRenderer>();
        if (kaynakMesh == null || kaynakCizici == null || Camera.main == null) return;

        var g = new GameObject("Poset_Ucusu");
        g.transform.SetPositionAndRotation(poset.position, poset.rotation);
        g.transform.localScale = poset.lossyScale;
        g.AddComponent<MeshFilter>().sharedMesh = kaynakMesh.sharedMesh;
        var cizici = g.AddComponent<MeshRenderer>();
        cizici.sharedMaterials = kaynakCizici.sharedMaterials;
        cizici.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // havada gölge oyunu olmasın
        g.AddComponent<PosetUcusu>();
    }

    private void Start()
    {
        baslangic = transform.position;
        baslangicOlcek = transform.localScale;
        baslangicDonus = transform.rotation;
    }

    private void Update()
    {
        Camera kamera = Camera.main;
        if (kamera == null) { Destroy(gameObject); return; }

        gecen += Time.deltaTime;
        float t = Mathf.Clamp01(gecen / Sure);
        // Yavaş başlayıp hızlanır (t²): önce yerden kalkar, sonra ele "çekilir".
        float e = t * t;

        // Hedef: kameranın biraz önü, sağı ve altı. Her karede yeniden hesaplanıyor,
        // çünkü oyuncu bu sırada başını çevirebilir.
        Transform k = kamera.transform;
        Vector3 hedef = k.position + k.forward * 0.45f + k.right * 0.25f - k.up * 0.35f;
        // Yol düz değil, hafif bir kavis: ortada 15 cm yukarı kalkar.
        Vector3 konum = Vector3.Lerp(baslangic, hedef, e) + Vector3.up * (0.15f * Mathf.Sin(Mathf.PI * t));

        transform.position = konum;
        transform.localScale = baslangicOlcek * Mathf.Lerp(1f, 0.3f, e);
        transform.rotation = baslangicDonus * Quaternion.Euler(0f, 90f * e, 20f * e);

        if (t >= 1f) Destroy(gameObject);
    }
}
