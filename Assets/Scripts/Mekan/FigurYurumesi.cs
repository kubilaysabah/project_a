// FigurYurumesi.cs
// Üç harfli figürlerinin İSKELETSİZ yürüyüşü ve koşusu (#140).
//
// NEDEN
// Sketchfab'dan seçilen altı figürün kemiği (iskeleti) yok: kolunu bacağını oynatamayız. Eski kutu Gölge'nin
// altı ayrı parçası vardı (GolgeAnimasyonu bacakları sallıyordu); bu figürler tek parça. Avda ve 7. kat
// kovalamacasında figür kayarak gelseydi bir buz pistinde süzülüyormuş gibi dururdu. Bir insanın yürüyüşünü
// gözün ilk okuduğu şey bacaklar değil, GÖVDENİN ritmidir: her adımda biraz iner kalkar, ağırlık bir ayaktan
// öbürüne geçtikçe sağa sola yatar, koşarken öne eğilir. Bu bileşen bunu tek parça gövdeye uygular.
//
// NASIL
// Figür prefab'ının kökü boş bir nesnedir; ağ (mesh) onun "Govde" adlı çocuğundadır (FigurKurucu böyle kurar).
// Hareket ettiriciler (Varlik, KacisAni, GezenGolge, CinOlaylari...) yalnız KÖKÜ taşır ve döndürür. Bu bileşen
// kökün kareden kareye ne kadar yer değiştirdiğini ölçer (hız), ve GÖVDEYE eğilme, iniş-kalkış, yan yatma verir:
//   yürürken (≤ 2,2 m/sn) : 2,5 cm iniş-kalkış, 3° öne eğilme, ±3° yan yatma; bir adım 0,75 m
//   koşarken (≥ 4 m/sn)   : 7 cm sıçrama, 14° öne eğilme, ±4° yan yatma; bir adım 1,1 m (kaçışın eski görünüşü)
//   dururken             : yavaş bir nefes (±0,8° öne-arkaya) ve 3-7 sn'de bir başın seyirmesi gibi kısa bir yan sıçrayış
// Kök taşındığı için ölçüm hareket ettiricinin hangi hızla yürüttüğünü bilmeden çalışır: av 2,0 m/sn (yürüyüş),
// 7. kat kovalamacası 2,2, geçiş 2,5, kaçış 4,5 (koşu). Bir karede 1,5 m'den fazla yer değiştirme ışınlanmadır
// (yakalanınca kat başına alınma): adım sayılmaz.
//
// Kök değil gövde oynadığı için çarpışma, yakalama mesafesi ve ayak sesleri değişmez; gözler (GozParlamasi)
// gövdenin çocuğudur, gövdeyle birlikte eğilir.
//
// #142 (A1): bir hareket ettirici figürü bilerek bir anda taşıyınca (karartmalı belirmede 1-1,5 m yaklaşma)
// Isinlandi() çağırır: o sıçrama adım sayılmaz. Yoksa 1,5 m'lik bir karelik sıçrama ~90 m/sn hız gibi okunur,
// gövde ve parlayan gözler ~0,4 sn koşu eğilmesiyle hoplardı (tam oyuncunun yalnız gözleri gördüğü an).
//
// #142 (A3): sahnedeki bütün figürler bir listede tutulur (SahnedeBaskasiVar). Bir yaratık belirmeden önce
// sahnede başka bir figür var mı diye bakar: cin olayı, gezen gölge, duvar gölgesi, ayna, şimşek ve av aynı
// anda iki yaratık göstermesin ("binanın tek varlığı var", #86).
//
// #142 (B3): DefaultExecutionOrder(-10): bu bileşenin LateUpdate'i göz parıltısınınkinden (sıra 0) ÖNCE
// çalışır; gözün fener konisinde olup olmadığı o karenin gövde duruşuyla ölçülür, bir önceki kareninkiyle değil.
// Unity aynı olay fonksiyonunu (LateUpdate) bileşenler arasında bu sayıya göre sıralar, küçük önce.

using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class FigurYurumesi : MonoBehaviour
{
    private const float KosuBaslangic = 2.2f, KosuTam = 4.0f;          // m/sn: bu aralıkta yürüyüşten koşuya geçilir
    private const float YurumeAdimi = 0.75f, KosuAdimi = 1.1f;         // m: bir adımın boyu (her adım bir iniş-kalkış)
    private const float YurumeInis = 0.025f, KosuSicrama = 0.07f;      // m: adım başına iniş-kalkış
    private const float YurumeEgilme = 3f, KosuEgilme = 14f;           // derece: öne eğilme
    private const float YurumeYatma = 3f, KosuYatma = 4f;              // derece: sağa sola yatma
    private const float HizYumusatma = 8f;                             // hız ölçümü bu hızla (1/sn) yumuşatılır
    private const float IsinlanmaSiniri = 1.5f;                        // m: bir karede bundan fazlası ışınlanma

    private Transform govde;
    private Vector3 sonKonum;
    private float hiz;          // yumuşatılmış yatay hız (m/sn)
    private float faz;          // adım döngüsü (radyan): her adımda π ilerler
    private float hareket;      // 0 = duruyor, 1 = yürüyor (yumuşak geçiş)
    private float seyirme;      // başın kısa seyirmesi (derece), sıfıra doğru söner
    private float sonrakiSeyirme;

    /// <summary>Test göstergesi: ölçülen hız (m/sn).</summary>
    public float Hiz { get { return hiz; } }

    // Sahnede şu an etkin olan figürler. "static": bütün figürlerin paylaştığı TEK liste (her figürün kendi
    // listesi değil). HashSet: aynı öğeyi iki kez tutmayan, eklemesi ve silmesi hızlı bir küme.
    private static readonly HashSet<FigurYurumesi> sahnede = new HashSet<FigurYurumesi>();

    // Bir figür sahnede belirince (Instantiate ya da SetActive(true)) listeye girer, silinince ya da
    // gizlenince çıkar. OnEnable/OnDisable bu iki anda Unity'nin kendiliğinden çağırdığı fonksiyonlardır.
    private void OnEnable() { sahnede.Add(this); }
    private void OnDisable() { sahnede.Remove(this); }

    /// <summary>
    /// #142 (A3): sahnede 'haric' dışında bir figür var mı? Bir yaratık belirmeden önce sorar; varsa belirmez
    /// (ya da sesli bir yedeğe döner). 'haric': soran kendi figürünü saymasın diye (yoksa null).
    /// </summary>
    public static bool SahnedeBaskasiVar(GameObject haric)
    {
        foreach (FigurYurumesi f in sahnede)
            if (f != null && f.gameObject != haric) return true;
        return false;
    }

    /// <summary>
    /// #142 (A1): figür bilerek bir anda taşındı (yürümedi): bu karedeki yer değiştirme adım sayılmasın.
    /// </summary>
    public void Isinlandi()
    {
        sonKonum = transform.position;
    }

    private void Start()
    {
        govde = transform.Find("Govde");
        sonKonum = transform.position;
        sonrakiSeyirme = Time.time + Random.Range(3f, 7f);
    }

    // LateUpdate: hareket ettiriciler kökü Update'te ya da eşyordamlarda (coroutine) taşır; ölçüm hepsinden sonra.
    private void LateUpdate()
    {
        if (govde == null) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;   // menüde zaman durur: gövde olduğu gibi kalır

        // 1) Yatay yer değiştirme ve hız.
        Vector3 fark = transform.position - sonKonum; fark.y = 0f;
        sonKonum = transform.position;
        float yol = fark.magnitude;
        if (yol > IsinlanmaSiniri) yol = 0f;
        // Lerp + Exp: kare hızından bağımsız yumuşatma (60 FPS'te de 240 FPS'te de aynı gecikme).
        hiz = Mathf.Lerp(hiz, yol / dt, 1f - Mathf.Exp(-HizYumusatma * dt));
        hareket = Mathf.MoveTowards(hareket, Mathf.InverseLerp(0.1f, 0.6f, hiz), 4f * dt);
        float kosu = Mathf.InverseLerp(KosuBaslangic, KosuTam, hiz);   // 0 yürüyüş, 1 koşu

        // 2) Adım döngüsü: yürünen yol / adım boyu kadar yarım tur (π). Hız değil yol: yavaş da hızlı da yürüse
        //    adım başına aynı iniş-kalkış.
        faz += yol / Mathf.Lerp(YurumeAdimi, KosuAdimi, kosu) * Mathf.PI;
        float inis = Mathf.Abs(Mathf.Sin(faz)) * Mathf.Lerp(YurumeInis, KosuSicrama, kosu) * hareket;
        float egilme = Mathf.Lerp(YurumeEgilme, KosuEgilme, kosu) * hareket;
        float yatma = Mathf.Sin(faz) * Mathf.Lerp(YurumeYatma, KosuYatma, kosu) * hareket;

        // 3) Dururken: nefes ve ara ara seyirme.
        float nefes = Mathf.Sin(Time.time * 1.7f) * 0.8f * (1f - hareket);
        if (Time.time >= sonrakiSeyirme)
        {
            seyirme = Random.Range(-6f, 6f);
            sonrakiSeyirme = Time.time + Random.Range(3f, 7f);
        }
        seyirme = Mathf.MoveTowards(seyirme, 0f, 40f * dt);

        // 4) Gövdeye yaz. Pivot ayaklarda: eğilme ayakların etrafında olur.
        govde.localPosition = Vector3.up * inis;
        govde.localRotation = Quaternion.Euler(egilme + nefes, seyirme * 0.5f, yatma + seyirme);
    }
}
