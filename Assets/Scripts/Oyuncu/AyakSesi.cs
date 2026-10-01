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
// ZEMİN (#74, Şahan'ın ses kararları; #87 zemine göre): ayağın altındaki zemin aşağı atılan bir
// ışınla bulunur ve dört aileden biri çalar:
//   - beton (Merdiven_Adim_1-8): merdiven, sahanlık, daire dışı her yer;
//   - parke (Adim_1-8 + ara sıra Gicirti_1-3): dairenin kuru odaları ("Daire_Parke" kutusu);
//   - fayans (Adim_Fayans_1-8): banyo ve mutfak ("Daire_Fayans" kutusu);
//   - halı (Adim_Hali_1-8): halı, kilim, yolluk, paçavra üstü (süs eşyasının adından).
// Şahan'ın Phasmophobia karşılaştırması (2026-10-01): "parke, fayans ve halı aynı sesi çıkarıyor."
// #101 (2026-10-01): dört ailenin hepsi GERÇEK KAYIT; aynı kişi, aynı ayakkabı, aynı mikrofon
// (sturmankin, CC0), zemin başına 8 adım. Korku oyunlarının adımları böyle kaydedilir: zemin
// değişince "aynı ayak başka yere bastı" diye duyulur. Kesim: kayit_isle.py adimlar.

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
    private AudioClip[] fayansAdimlari;     // fayans adımı (banyo, mutfak) (#87)
    private AudioClip[] haliAdimlari;       // halı adımı (#87)
    private AudioClip[] gicirtilar;         // parke gıcırtısı, ara sıra

    /// <summary>Ayağın altındaki zemin türü (test ve gösterge için).</summary>
    public enum Zemin { Beton, Parke, Fayans, Hali }
    public Zemin SonZemin { get; private set; }
    private readonly int[] sonCalinanlar = { -1, -1, -1, -1 };   // zemin başına son çeşit

    [Tooltip("Dairede bir adımın gıcırtıyla birlikte çalma olasılığı (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float gicirtiOlasiligi = 0.12f;
    private float sonGicirti = -10f;        // iki gıcırtı arasında en az birkaç saniye

    private readonly AdimOlcer olcer = new AdimOlcer();
    private Vector3 oncekiKonum;

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
        var fayans = new System.Collections.Generic.List<AudioClip>();
        var hali = new System.Collections.Generic.List<AudioClip>();
        var gicirti = new System.Collections.Generic.List<AudioClip>();
        foreach (AudioClip k in Resources.LoadAll<AudioClip>("Sesler"))
        {
            // Sıra önemli: "Adim_Fayans" de "Adim" ile başlar; önce özel aileler ayrılır.
            if (k.name.StartsWith("Adim_Fayans")) fayans.Add(k);
            else if (k.name.StartsWith("Adim_Hali")) hali.Add(k);
            else if (k.name.StartsWith("Adim")) liste.Add(k);
            else if (k.name.StartsWith("Merdiven_Adim")) merdiven.Add(k);
            else if (k.name.StartsWith("Gicirti")) gicirti.Add(k);
        }
        adimlar = liste.ToArray();
        merdivenAdimlari = merdiven.ToArray();
        fayansAdimlari = fayans.ToArray();
        haliAdimlari = hali.ToArray();
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
        Zemin zemin = ZeminBul();
        SonZemin = zemin;
        AudioClip[] cesitler = zemin == Zemin.Fayans ? fayansAdimlari
                             : zemin == Zemin.Hali ? haliAdimlari
                             : zemin == Zemin.Parke ? adimlar : merdivenAdimlari;
        if (cesitler.Length == 0) cesitler = adimlar;   // aile yoksa parke
        bool daire = zemin == Zemin.Parke;
        int son = sonCalinanlar[(int)zemin];            // bu zeminde en son çalınan çeşit

        // Bir öncekiyle aynı sesi arka arkaya çalma; tekrar kulağa hemen batar.
        int i = Random.Range(0, cesitler.Length);
        if (cesitler.Length > 1 && i == son) i = (i + 1) % cesitler.Length;
        sonCalinanlar[(int)zemin] = i;

        // Her adımda perde ve şiddet biraz değişsin: gerçek adımlar hiç aynı değildir.
        // #101: ±%4 (eskiden ±%8). Sentez adımın tek çeşidi vardı, çeşidi perde taşıyordu; şimdi
        // her zeminde 8 gerçek adım var ve gerçek kayıtta ±%8 perde ayakkabıyı değiştirmiş gibi
        // duyulur (kaba bir ayakkabı ince, ince bir ayakkabı kaba).
        kaynak.pitch = Random.Range(0.96f, 1.04f);
        kaynak.PlayOneShot(cesitler[i], siddet * Random.Range(0.85f, 1.0f));

        // Eski parke: ara sıra adımın altından bir gıcırtı (iki gıcırtı arası en az 3 sn).
        if (daire && gicirtilar.Length > 0 && Time.time - sonGicirti > 3f && Random.value < gicirtiOlasiligi)
        {
            sonGicirti = Time.time;
            kaynak.PlayOneShot(gicirtilar[Random.Range(0, gicirtilar.Length)], siddet * 0.8f);
        }
    }

    // Ayağın altındaki zemin ne? Kural ZeminSesi'nde (#99): Gölge'nin adımları da aynı kuralı kullanır.
    // İki sayımın (enum) sırası aynı: Beton, Parke, Fayans, Hali.
    private Zemin ZeminBul()
    {
        return (Zemin)(int)ZeminSesi.Bul(transform.position, transform);
    }
}
