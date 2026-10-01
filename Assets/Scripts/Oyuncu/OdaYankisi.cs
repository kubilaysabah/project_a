// OdaYankisi.cs
// Bulunulan yere göre yankı (#87): fayanslı banyo ve mutfak sert ve kısa, halılı oda yumuşak ve
// boğuk, beton merdiven ve sahanlık uzun çınlar.
//
// Şahan'ın Phasmophobia karşılaştırması (2026-10-01): "ses mekânı yansıtmıyor; fayanslı banyo ile
// halılı salon aynı tınlıyor." Phasmophobia'nın gücünün çoğu sesten gelir; yankı, mekânın
// büyüklüğünü ve malzemesini kulağa anlatır.
//
// YANKI (reverb) NEDİR: bir ses çıkınca duvarlardan dönen binlerce küçük yansımanın toplamı. Sert,
// düz yüzeyler (fayans, beton) sesi uzun süre ve tiz tutar; halı, perde, kumaş yutar. Unity'nin
// AudioReverbFilter bileşeni, dinleyiciye (kamera) takılınca ONA ULAŞAN BÜTÜN sesleri bu
// yansımayla işler: ayak sesi, kapı, Gölge'nin adımları hepsi bulunulan odanın yankısını alır.
// Müzik gibi "mekânda olmayan" sesler bypassListenerEffects ile bunun dışında tutulur (ArkaMuzik).
//
// Oda, oyuncunun plan noktasından bulunur (CalanEsya'daki yol): açık dairenin eksenleri plan
// (u, v) verir, DairePlani odayı söyler. Daire dışı = sahanlık/merdiven (beton). Odadan odaya
// geçerken değerler 0,6 saniyede yumuşakça kayar; kapı eşiğinde ses birden değişmesin.

using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class OdaYankisi : MonoBehaviour
{
    // Üç mekân tipi. Değerler Unity'nin hazır yankı tiplerinden (Bathroom, Carpeted/Livingroom,
    // StoneCorridor) alınıp bu binaya göre hafifletildi. Birimler: mB (milibel: 100 mB = 1 dB) ve saniye.
    private struct Ayar
    {
        public float oda, odaTiz, sonum, tizOrani, yansima, yansimaGecikme, yanki, yankiGecikme, yayilma, yogunluk;
    }

    private static readonly Ayar Fayans = new Ayar { oda = -1000f, odaTiz = -1200f, sonum = 1.3f, tizOrani = 0.54f, yansima = -500f, yansimaGecikme = 0.007f, yanki = 600f, yankiGecikme = 0.011f, yayilma = 100f, yogunluk = 60f };
    private static readonly Ayar Oda = new Ayar { oda = -1000f, odaTiz = -5000f, sonum = 0.45f, tizOrani = 0.15f, yansima = -1400f, yansimaGecikme = 0.002f, yanki = -1300f, yankiGecikme = 0.004f, yayilma = 100f, yogunluk = 100f };
    private static readonly Ayar Beton = new Ayar { oda = -1000f, odaTiz = -500f, sonum = 2.1f, tizOrani = 0.75f, yansima = -1200f, yansimaGecikme = 0.013f, yanki = 100f, yankiGecikme = 0.020f, yayilma = 100f, yogunluk = 100f };

    private const float KontrolAraligi = 0.25f;
    private const float GecisSuresi = 0.6f;

    private AudioReverbFilter filtre;
    private Transform oyuncu;
    private Ayar hedef = Beton, simdiki = Beton;
    private float sayac;
    private string mekan = "beton";

    /// <summary>Şu anki mekân adı (test ve gösterge için): fayans / oda / beton.</summary>
    public string Mekan { get { return mekan; } }

    private void Awake()
    {
        filtre = GetComponent<AudioReverbFilter>();
        if (filtre == null) filtre = gameObject.AddComponent<AudioReverbFilter>();
        filtre.reverbPreset = AudioReverbPreset.User;   // değerleri biz yazacağız
        filtre.dryLevel = 0f;
        Uygula(simdiki);
        BirinciSahisKontrolcu k = GetComponentInParent<BirinciSahisKontrolcu>();
        oyuncu = k != null ? k.transform : transform;
    }

    private void Update()
    {
        sayac += Time.deltaTime;
        if (sayac >= KontrolAraligi)
        {
            sayac = 0f;
            string yeni = MekanBul();
            if (yeni != mekan)
            {
                mekan = yeni;
                hedef = yeni == "fayans" ? Fayans : yeni == "oda" ? Oda : Beton;
            }
        }
        // Hedefe doğru kay: her karede kalan yolun bir kısmı (Lerp).
        float t = 1f - Mathf.Exp(-Time.deltaTime / GecisSuresi * 3f);
        simdiki = Kay(simdiki, hedef, t);
        Uygula(simdiki);
    }

    // Oyuncu hangi mekânda? Açık dairelerin eksenlerine sorulur; hiçbiri "içeride" demezse beton.
    private string MekanBul()
    {
        foreach (DaireEksenleri e in FindObjectsByType<DaireEksenleri>())
        {
            if (!e.gameObject.activeInHierarchy) continue;
            e.PlanNoktasi(oyuncu.position, out float u, out float v);
            string odaAdi = DairePlani.NoktaninOdasi(u, v);
            if (odaAdi == null) continue;
            if (DairePlani.OdaBul(odaAdi, out DairePlani.Oda oda) && oda.Islak) return "fayans";
            return "oda";
        }
        return "beton";
    }

    private static Ayar Kay(Ayar a, Ayar b, float t)
    {
        return new Ayar
        {
            oda = Mathf.Lerp(a.oda, b.oda, t), odaTiz = Mathf.Lerp(a.odaTiz, b.odaTiz, t),
            sonum = Mathf.Lerp(a.sonum, b.sonum, t), tizOrani = Mathf.Lerp(a.tizOrani, b.tizOrani, t),
            yansima = Mathf.Lerp(a.yansima, b.yansima, t), yansimaGecikme = Mathf.Lerp(a.yansimaGecikme, b.yansimaGecikme, t),
            yanki = Mathf.Lerp(a.yanki, b.yanki, t), yankiGecikme = Mathf.Lerp(a.yankiGecikme, b.yankiGecikme, t),
            yayilma = Mathf.Lerp(a.yayilma, b.yayilma, t), yogunluk = Mathf.Lerp(a.yogunluk, b.yogunluk, t),
        };
    }

    private void Uygula(Ayar a)
    {
        filtre.room = a.oda;
        filtre.roomHF = a.odaTiz;
        filtre.decayTime = a.sonum;
        filtre.decayHFRatio = a.tizOrani;
        filtre.reflectionsLevel = a.yansima;
        filtre.reflectionsDelay = a.yansimaGecikme;
        filtre.reverbLevel = a.yanki;
        filtre.reverbDelay = a.yankiGecikme;
        filtre.diffusion = a.yayilma;
        filtre.density = a.yogunluk;
    }
}
