// AyakSesi.cs
// Oyuncunun kendi ayak seslerini çalar (#25).
//
// Ne zaman adım atıldığına AdimOlcer (saf C#, testli) karar verir; bu bileşen
// her karede oyuncunun yatayda ne kadar yol aldığını ölçüp ona sorar ve "adım"
// cevabı gelince bir adım sesi çalar.
//
// Sesin kendisi oyuncunun üstündeki AudioSource'tan, üç boyutlu DEĞİL çalar
// (spatialBlend = 0): kendi ayak sesini hep aynı yerden, "kendinden" duyarsın.

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AyakSesi : MonoBehaviour
{
    [Tooltip("Adım sesinin şiddeti (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float siddet = 0.35f;

    private CharacterController govde;
    private AudioSource kaynak;
    private AudioClip[] adimlar;

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
        foreach (AudioClip k in Resources.LoadAll<AudioClip>("Sesler"))
        {
            if (k.name.StartsWith("Adim")) liste.Add(k);
        }
        adimlar = liste.ToArray();

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
        // Bir öncekiyle aynı sesi arka arkaya çalma; tekrar kulağa hemen batar.
        int i = Random.Range(0, adimlar.Length);
        if (adimlar.Length > 1 && i == sonCalinan) i = (i + 1) % adimlar.Length;
        sonCalinan = i;

        // Her adımda perde ve şiddet biraz değişsin: gerçek adımlar hiç aynı değildir.
        kaynak.pitch = Random.Range(0.92f, 1.08f);
        kaynak.PlayOneShot(adimlar[i], siddet * Random.Range(0.85f, 1.0f));
    }
}
