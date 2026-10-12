// GezenGolge.cs
// Bütün katlarda gezen Gölge (#71).
//
// Gölge'nin KENDİ SAATİ YOK (Şahan, 2026-10-01, #86: "gezen gölgenin zamanlayıcısı kalksın, sadece
// gerilimle gelsin"). Eskiden her 1-2 dakikada bir kendiliğinden belirirdi (GolgeZamanlayici, #71);
// şimdi onu yalnızca Varlık çağırır (gerilim 2. kademeye varınca, HaberciyleBelir) ve yanlış kapı
// çalınca SahanlikKapisi çağırır (Belir). HaberciyleBelir'de önce ÜST KATTAN AĞIR ADIMLAR duyulur;
// sesin ortasında 7. kattaki kaçış Gölgesi'nin aynısı ANİDEN belirir: oyuncunun önünde, görebileceği
// bir yerde (#74, Şahan: "canavar gelmeden önce çalalım, sesin ortasında canavar belirsin").
// Adımlar bitince yer yoksa Gölge çıkmaz, ses kalır.
// 2 saniye oyuncuya doğru koşar; oyuncuya 1,2 m'den fazla yaklaşmaz, dokunmaz. Sonra kafa lambası
// titrer ve Gölge kaybolur. Hasar yok, ölüm yok, kat sıfırlanmaz (Şahan'ın isteği): yalnızca korku.
//
// Nerede belirir? Oyuncunun baktığı yönde, kafa lambasının huzmesinin ortasında (±12°), 3-5,5 m
// uzakta birkaç nokta denenir. Gölge çok koyu: huzmenin dışında (ilk denemede 24° yanda) yalnızca
// kırmızı gözleri seçiliyordu. Bir nokta şunları sağlamalı:
//   - oyuncunun gözüyle Gölge'nin göğsü arasında duvar ya da eşya yok (Physics.Linecast);
//   - altında oyuncuyla aynı yükseklikte zemin var (merdivende ya da boşlukta belirmesin);
//   - bir insan sığacak kadar boş (Physics.CheckCapsule: duvarın içinde belirmesin).
// Böylece Gölge koşarken de duvardan geçmez: oyuncuyla arasındaki yol zaten açık.
// Uygun nokta yoksa (oyuncu duvara dönük duruyor) Gölge bu seferlik çıkmaz; Varlık sonraki tepkide yine dener.
//
// Ne zaman sayılmaz? Kat yöneticisi "oyuncu katta sıkışık değil" dediğinde (açılış, kat kapısı
// açıldı, çatı), başlık ekranında, oyuncunun kontrolü kapalıyken (duraklatma, not, şifre ekranı)
// ve 7. katın kaçış anı sürerken.
//
// Bileşen KatYoneticisi'nin nesnesinde durur; zincir kurucusu ekler ve şablonu bağlar.

using System.Collections;
using UnityEngine;

public class GezenGolge : MonoBehaviour
{
    [SerializeField] private GameObject sablon;          // Kacis_Golgesi şablonu (7. katla aynı)
    [SerializeField] private AudioClip haberci;          // üst kattan ağır adımlar (Olay_UstKatAdimlari)

    private const float KosuSuresi = 2f;                 // kaç saniye kovalar
    private const float Hiz = 2.6f;                      // m/sn (oyuncu 1,8 yürür)
    private const float EnYakin = 1.2f;                  // oyuncuya en fazla bu kadar yaklaşır
    private const float AdimAraligi = 0.4f;

    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private Transform oyuncu;
    private GameObject golge;                            // şu an sahnedeki Gölge (yoksa null)
    private static AudioClip[] adimlar;
    private bool haberciCaliyor;                         // adımlar duyuluyor, Gölge henüz yok

    private void Start()
    {
        yonetici = GetComponent<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        oyuncu = kontrol != null ? kontrol.transform : null;
        if (adimlar == null)
        {
            // Yedek adım: zemin bulunamazsa parke adımı (#101'den beri gerçek kayıt).
            adimlar = new[] { Resources.Load<AudioClip>("Sesler/Adim_1") };
        }
    }

    /// <summary>
    /// Önce üst kattan adımlar, sesin ortasında Gölge (Varlık, 2. kademe). Oyuncunun önünde yer yoksa
    /// adımlar da çalmaz ve false döner (Varlık o zaman kapıyı tıklatır).
    /// </summary>
    public bool HaberciyleBelir()
    {
        if (golge != null || haberciCaliyor || sablon == null || oyuncu == null) return false;
        if (FigurYurumesi.SahnedeBaskasiVar(null)) return false;   // #142 (A3): sahnede başka bir figür var
        if (!YerVarMi()) return false;
        StartCoroutine(HaberciVeBelir());
        return true;
    }

    // Üst kattan adımlar başlar; sesin ortasında Gölge belirir.
    private IEnumerator HaberciVeBelir()
    {
        haberciCaliyor = true;
        float yari = 1.5f;
        // İmza (#105): anahtarlık, Gölge'nin belireceği taraftan (oyuncunun önü), adımlardan önce.
        GolgeImzasi.Cal(oyuncu.position + oyuncu.forward * 3f + Vector3.up * 1.2f);
        if (haberci != null)
        {
            // Ses oyuncunun tam üstünden, üst kattan (3 m yukarıdan) gelir.
            AudioSource.PlayClipAtPoint(haberci, oyuncu.position + Vector3.up * 3f, 0.8f);
            yari = haberci.length / 2f;
            Debug.Log($"[GOLGE] {yonetici.MevcutKat}. kat: üst kattan adımlar.");
        }
        yield return new WaitForSeconds(yari);
        haberciCaliyor = false;
        // Oyuncu bu arada döndü ya da menü açtıysa Gölge çıkmaz; adımlar yalnız kalır.
        if (SayabilirMi()) Belir();
    }

    // Şu an oyuncunun önünde Gölge'ye uygun bir yer var mı (yer tutulmaz, yalnızca bakılır)?
    // (Oyuncu duvara dönükse adımlar da çalmasın: ses gelip Gölge çıkmaması kuralı boşa harcar.)
    private bool YerVarMi()
    {
        Camera kamera = kontrol != null ? kontrol.GetComponentInChildren<Camera>() : null;
        return kamera != null && YerBul(kamera.transform, out Vector3 yer) && EkrandaMi(kamera, yer);
    }

    // Gölge'nin göğsü ekranın ortadaki %70'lik alanında mı?
    private static bool EkrandaMi(Camera kamera, Vector3 yer)
    {
        Vector3 ekran = kamera.WorldToViewportPoint(yer + Vector3.up * 1.2f);
        return ekran.z > 0f && ekran.x >= 0.15f && ekran.x <= 0.85f && ekran.y >= 0.15f && ekran.y <= 0.85f;
    }

    // Gölge şu an belirebilir mi (adımlar çalarken oyuncu menüye girdiyse, kapıyı açtıysa belirmez)?
    private bool SayabilirMi()
    {
        if (golge != null || haberciCaliyor || sablon == null) return false;
        if (!yonetici.enabled || !yonetici.GolgeSayabilir) return false;   // açılış, kapı açık, çatı, final
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        if (kontrol == null || !kontrol.enabled) return false;              // menü, not, şifre ekranı
        foreach (KacisAni k in FindObjectsByType<KacisAni>())               // 7. katın kaçışı sürüyor
            if (k.Basladi) return false;
        if (Varlik.Ornek != null && Varlik.Ornek.AvSuruyor) return false;  // Varlığın avı sürüyor (#86)
        return true;
    }

    /// <summary>Gölge'yi şimdi belirtir (test ve deneme için de çağrılabilir). Yer bulunamazsa false.</summary>
    public bool Belir()
    {
        if (golge != null || sablon == null || oyuncu == null) return false;
        // #142 (A3): adımlar çalarken bir cin figürü belirmiş olabilir: o zaman Gölge gelmez (iki yaratık olmaz).
        if (FigurYurumesi.SahnedeBaskasiVar(null)) return false;
        Camera kamera = kontrol != null ? kontrol.GetComponentInChildren<Camera>() : null;
        if (kamera == null) return false;
        if (!YerBul(kamera.transform, out Vector3 yer)) return false;
        // Oyuncu yere ya da tavana bakıyorsa Gölge görüş alanının dışında kalır: belirmez.
        // WorldToViewportPoint: dünyadaki bir noktanın ekrandaki yeri (0-1 arası; z kameranın
        // önündeki uzaklık). Göğüs ekranın ortadaki %70'lik alanında olmalı (EkrandaMi).
        if (!EkrandaMi(kamera, yer)) return false;

        Vector3 bakis = oyuncu.position - yer;
        bakis.y = 0f;
        golge = Instantiate(VarlikGorunusu.Sablon(sablon), yer, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);   // #140: figür torbası (VarlikGorunusu)
        CinSesi.Belirdi(yer);   // #143: beliriş sesi
        golge.name = "Gezen_Golge";
        StartCoroutine(Kovala());
        Debug.Log($"[GOLGE] {yonetici.MevcutKat}. kat: Gölge belirdi ({Vector3.Distance(yer, oyuncu.position):0.0} m).");
        return true;
    }

    // Oyuncunun önünde, görebileceği, üstünde durulabilecek boş bir nokta arar.
    private bool YerBul(Transform kamera, out Vector3 yer)
    {
        yer = Vector3.zero;
        Vector3 ileri = kamera.forward;
        ileri.y = 0f;
        if (ileri.sqrMagnitude < 0.01f) ileri = oyuncu.forward;
        ileri.Normalize();
        Vector3 goz = kamera.position;
        float ayak = oyuncu.position.y;
        // Sahanlık 5 m derinliğinde, odalar daha da küçük: 4-7 m çoğu yerde duvarın ardına düşüyordu.
        for (int deneme = 0; deneme < 24; deneme++)
        {
            float aci = Random.Range(-12f, 12f);
            float mesafe = Random.Range(3f, 5.5f);
            Vector3 aday = oyuncu.position + Quaternion.Euler(0f, aci, 0f) * ileri * mesafe;
            // Altında zemin: 1 m yukarıdan aşağı ışın; zemin oyuncunun ayak hizasında olmalı.
            if (!Physics.Raycast(aday + Vector3.up * 1f, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Mathf.Abs(zemin.point.y - ayak) > 0.3f) continue;
            aday.y = zemin.point.y;
            // Göz ile Gölge'nin göğsü (1,3 m) ve dizi (0,5 m) arasında engel yok. Işın oyuncunun
            // kendi kapsülünün içinden başlar; Linecast içinden başladığı çarpıştırıcıyı görmez.
            if (Physics.Linecast(goz, aday + Vector3.up * 1.3f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Physics.Linecast(goz, aday + Vector3.up * 0.5f, ~0, QueryTriggerInteraction.Ignore)) continue;
            // Bir insan sığar mı? 25 cm yarıçaplı, 0,35-1,6 m arası bir kapsül boş olmalı.
            if (Physics.CheckCapsule(aday + Vector3.up * 0.35f, aday + Vector3.up * 1.6f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) continue;
            yer = aday;
            return true;
        }
        return false;
    }

    // 2 saniye oyuncuya doğru koşar, sonra kaybolur.
    private IEnumerator Kovala()
    {
        // Belirirken cızırtı ve çevresindeki uğultu #74'te kalktı; gelişini adımlar haber verdi.
        ArkaMuzik.Ornek?.Kabar();
        float adimSayaci = AdimAraligi;
        for (float t = 0f; t < KosuSuresi && golge != null; t += Time.deltaTime)
        {
            Vector3 fark = oyuncu.position - golge.transform.position;
            fark.y = 0f;
            float mesafe = fark.magnitude;
            if (mesafe > EnYakin)
                golge.transform.position += fark / mesafe * Mathf.Min(Hiz * Time.deltaTime, mesafe - EnYakin);
            if (mesafe > 0.01f) golge.transform.rotation = Quaternion.LookRotation(fark);
            // Ağır adımlar: oyuncunun adım sesleri, kalın (7. kattaki kaçıştaki gibi).
            adimSayaci += Time.deltaTime;
            if (adimSayaci >= AdimAraligi && adimlar != null)
            {
                adimSayaci = 0f;
                // Bastığı zeminin adımı (#99, ZeminSesi): merdivende beton, mutfakta fayans.
                AudioClip ad = ZeminSesi.AdimKlibi(golge.transform.position, golge.transform);
                if (ad == null) ad = adimlar[Random.Range(0, adimlar.Length)];
                if (ad != null) AdimCal(ad, golge.transform.position);
            }
            yield return null;
        }
        // Kaybolurken kafa lambası titrer: oyuncu bir an karanlıkta kalır, ışık dönünce Gölge yok.
        KafaLambasi lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Titret(0.8f);
        yield return new WaitForSeconds(0.15f);
        if (golge != null) Destroy(golge);
        golge = null;
        Debug.Log($"[GOLGE] {yonetici.MevcutKat}. kat: Gölge kayboldu.");
    }

    private static void AdimCal(AudioClip klip, Vector3 yer)
    {
        GameObject g = new GameObject("Golge_Adim");
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip; a.spatialBlend = 1f; a.pitch = 0.62f; a.volume = 1f; a.maxDistance = 16f;
        a.Play();
        Destroy(g, klip.length / a.pitch + 0.1f);
    }

    private void OnDisable()
    {
        if (golge != null) Destroy(golge);
        golge = null;
    }
}
