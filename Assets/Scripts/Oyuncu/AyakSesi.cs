// AyakSesi.cs
// Oyuncunun kendi ayak seslerini çalar (#25).
//
// Ne zaman adım atıldığına AdimOlcer (saf C#, testli) karar verir; bu bileşen
// her karede oyuncunun yatayda ne kadar yol aldığını ölçüp ona sorar ve "adım"
// cevabı gelince bir adım sesi çalar.
//
// Sesin kendisi oyuncunun üstündeki AudioSource'tan, üç boyutlu DEĞİL çalar
// (spatialBlend = 0): kendi ayak sesini hep aynı yerden, "kendinden" duyarsın.
//
// ZEMİN (#74, Şahan'ın ses kararları): ayağın altındaki zemin bir DAİRENİN zeminiyse parke adımı
// (Adim_1) çalar ve ara sıra eski parkenin gıcırtısı karışır (Gicirti_1-3); merdivende ve
// sahanlıkta beton adımları (Merdiven_Adim_1-4) çalar. Zeminin kime ait olduğu aşağı atılan bir
// ışınla bulunur: çarptığı çarpıştırıcının ebeveynlerinde DaireEksenleri varsa daire içidir.

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AyakSesi : MonoBehaviour
{
    [Tooltip("Adım sesinin şiddeti (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float siddet = 0.35f;

    private CharacterController govde;
    private AudioSource kaynak;
    private AudioClip[] adimlar;            // parke adımı (daire içi)
    private AudioClip[] merdivenAdimlari;   // beton adımı (merdiven, sahanlık)
    private AudioClip[] gicirtilar;         // parke gıcırtısı, ara sıra

    [Tooltip("Dairede bir adımın gıcırtıyla birlikte çalma olasılığı (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float gicirtiOlasiligi = 0.12f;
    private float sonGicirti = -10f;        // iki gıcırtı arasında en az birkaç saniye
    private int sonMerdiven = -1;

    private readonly AdimOlcer olcer = new AdimOlcer();
    private Vector3 oncekiKonum;
    private int sonCalinan = -1;

    private void Awake()
    {
        govde = GetComponent<CharacterController>();

        // Ses kaynağını kodla ekliyoruz; sahnede ayrıca kurmaya gerek yok.
        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;          // 0 = iki boyutlu: ses "kafanın içinden" gelir

        // Resources.LoadAll: "Resources/Sesler" klasöründe adı "Adim" ile başlayanları bulur.
        // Beşinci bir adım sesi eklemek için dosyayı klasöre koymak yeter.
        var liste = new System.Collections.Generic.List<AudioClip>();
        var merdiven = new System.Collections.Generic.List<AudioClip>();
        var gicirti = new System.Collections.Generic.List<AudioClip>();
        foreach (AudioClip k in Resources.LoadAll<AudioClip>("Sesler"))
        {
            if (k.name.StartsWith("Adim")) liste.Add(k);
            else if (k.name.StartsWith("Merdiven_Adim")) merdiven.Add(k);
            else if (k.name.StartsWith("Gicirti")) gicirti.Add(k);
        }
        adimlar = liste.ToArray();
        merdivenAdimlari = merdiven.ToArray();
        gicirtilar = gicirti.ToArray();

        oncekiKonum = transform.position;
    }

    private void LateUpdate()
    {
        // Yalnızca YATAY yolu ölçüyoruz: y ekseni merdiven çıkarken de değişir ve
        // ışınlanma tamamen dikey (3 m) olduğu için hesaba katılmamalı.
        Vector3 simdi = transform.position;
        Vector2 fark = new Vector2(simdi.x - oncekiKonum.x, simdi.z - oncekiKonum.z);
        oncekiKonum = simdi;

        if (adimlar.Length == 0) return;

        if (olcer.Ilerle(fark.magnitude, govde.isGrounded)) AdimCal();
    }

    private void AdimCal()
    {
        bool daire = DaireZemininde();
        AudioClip[] cesitler = daire || merdivenAdimlari.Length == 0 ? adimlar : merdivenAdimlari;
        int son = daire ? sonCalinan : sonMerdiven;   // bu zeminde en son çalınan çeşit

        // Bir öncekiyle aynı sesi arka arkaya çalma; tekrar kulağa hemen batar.
        int i = Random.Range(0, cesitler.Length);
        if (cesitler.Length > 1 && i == son) i = (i + 1) % cesitler.Length;
        if (daire) sonCalinan = i; else sonMerdiven = i;

        // Her adımda perde ve şiddet biraz değişsin: gerçek adımlar hiç aynı değildir.
        kaynak.pitch = Random.Range(0.92f, 1.08f);
        kaynak.PlayOneShot(cesitler[i], siddet * Random.Range(0.85f, 1.0f));

        // Eski parke: ara sıra adımın altından bir gıcırtı (iki gıcırtı arası en az 3 sn).
        if (daire && gicirtilar.Length > 0 && Time.time - sonGicirti > 3f && Random.value < gicirtiOlasiligi)
        {
            sonGicirti = Time.time;
            kaynak.PlayOneShot(gicirtilar[Random.Range(0, gicirtilar.Length)], siddet * 0.8f);
        }
    }

    // Ayağın altındaki zemin bir dairenin mi? Işın oyuncunun kendi kapsülünün içinden başlar;
    // RaycastAll bütün çarpmaları verir, oyuncunun kendi parçaları atlanır (Etkilesimci'deki ders).
    private bool DaireZemininde()
    {
        RaycastHit[] vuruslar = Physics.RaycastAll(transform.position + Vector3.up * 0.3f, Vector3.down, 0.8f,
                                                   ~0, QueryTriggerInteraction.Ignore);
        float enYakin = float.MaxValue;
        Transform zemin = null;
        foreach (RaycastHit v in vuruslar)
        {
            if (v.collider.transform.IsChildOf(transform)) continue;
            if (v.distance < enYakin) { enYakin = v.distance; zemin = v.collider.transform; }
        }
        return zemin != null && zemin.GetComponentInParent<DaireEksenleri>() != null;
    }
}
