// UcanKopya.cs
// Bir nesnenin görsel kopyasını kısa bir kavisle bir yere uçurur (#43, #44).
//   ElineUcur:  toplanan poşet kameranın sağ altına (kapıcının eline) süzülüp küçülür.
//   YerineUcur: rafa konan ayakkabı çifti yerden kalkıp rafına oturur.
// Asıl nesne kural açısından ANINDA işini bitirmiş olur (poşet alınmış, çift rafa konmuş);
// bu bileşen yalnızca görüntü. Hiçbir oyun kuralı bu hareketin bitmesini beklemez.
// (#43'teki PosetUcusu'nun genelleştirilmiş hâli: ayakkabı için hedefe uçma eklendi.)

using UnityEngine;

public class UcanKopya : MonoBehaviour
{
    private const float Sure = 0.35f;

    private Vector3 baslangic, baslangicOlcek;
    private Quaternion baslangicDonus;
    private Transform hedef;              // null: kameranın sağ altı (el)
    private System.Action bitince;
    private float gecen;

    /// <summary>Kopyayı kameranın sağ altına uçurur ve küçülterek yok eder (poşet toplama).</summary>
    public static void ElineUcur(Transform kaynak)
    {
        if (Camera.main == null) return;
        Kur(kaynak, null, null);
    }

    /// <summary>Kopyayı hedefin yerine ve duruşuna uçurur, varınca 'bitince'yi çağırır (ayakkabı rafa).</summary>
    public static void YerineUcur(Transform kaynak, Transform hedef, System.Action bitince)
    {
        Kur(kaynak, hedef, bitince);
    }

    // Kaynağın bir kopyasını üretir ve kopyadan görüntü dışındaki her şeyi söker
    // (çarpışma, etkileşim bileşenleri): kopya yalnızca bakılacak bir şey.
    private static void Kur(Transform kaynak, Transform hedef, System.Action bitince)
    {
        GameObject g = Instantiate(kaynak.gameObject, kaynak.position, kaynak.rotation);
        g.name = "Ucan_Kopya";
        g.transform.localScale = kaynak.lossyScale;
        foreach (Collider c in g.GetComponentsInChildren<Collider>()) Destroy(c);
        foreach (MonoBehaviour m in g.GetComponentsInChildren<MonoBehaviour>()) Destroy(m);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // havada gölge oyunu olmasın
        g.SetActive(true);

        UcanKopya u = g.AddComponent<UcanKopya>();
        u.hedef = hedef;
        u.bitince = bitince;
    }

    private void Start()
    {
        baslangic = transform.position;
        baslangicOlcek = transform.localScale;
        baslangicDonus = transform.rotation;
    }

    private void Update()
    {
        gecen += Time.deltaTime;
        float t = Mathf.Clamp01(gecen / Sure);
        float e = t * t;                  // yavaş başlar, hızlanır

        Vector3 varis;
        Quaternion varisDonus;
        Vector3 varisOlcek;
        if (hedef != null)
        {
            varis = hedef.position;
            varisDonus = hedef.rotation;
            varisOlcek = hedef.lossyScale;
        }
        else
        {
            Camera kamera = Camera.main;
            if (kamera == null) { Destroy(gameObject); return; }
            Transform k = kamera.transform;
            // Kameranın biraz önü, sağı ve altı; oyuncu başını çevirse de her karede yeniden.
            varis = k.position + k.forward * 0.45f + k.right * 0.25f - k.up * 0.35f;
            varisDonus = baslangicDonus * Quaternion.Euler(0f, 90f, 20f);
            varisOlcek = baslangicOlcek * 0.3f;
        }

        // Hafif bir kavis: yolun ortasında 15 cm yukarı kalkar.
        transform.position = Vector3.Lerp(baslangic, varis, e) + Vector3.up * (0.15f * Mathf.Sin(Mathf.PI * t));
        transform.rotation = Quaternion.Slerp(baslangicDonus, varisDonus, e);
        transform.localScale = Vector3.Lerp(baslangicOlcek, varisOlcek, e);

        if (t >= 1f)
        {
            bitince?.Invoke();
            Destroy(gameObject);
        }
    }
}
