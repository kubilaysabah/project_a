// AynaKorkusu.cs
// Yalnızca aynada görünen korku (#94). AynaYansimasi ile aynı nesnede (aynanın camında) durur.
//
// ---------- NE OLUR? ----------
// Bina huzursuzken (Varlık en az 1. kademede) oyuncu aynaya yakından (2,5 m) bakıyor ve ayna
// ekranın ortasındaysa (ortadaki %40'lık alan), 0,6 saniye sonra aynada, oyuncunun 1,0-1,3 m
// ARKASINDA Gölge belirir. Oyuncu dönüp bakarsa arkasında kimse yoktur: Gölge 30. katmandadır
// ve o katmanı yalnızca yansıma kamerası çizer (bkz. AynaYansimasi). Oyuncu başını çevirince
// (ayna ekrandan çıkınca) ya da 2,5 saniye dolunca Gölge kaybolur; kafa lambası kısaca titrer.
// Hasar yok, kat sıfırlanmaz: yalnızca korku. Belirirken arkadan bir döşeme gıcırtısı duyulur,
// müzik kabarır ve Varlık biraz "duyar" (Varlik.Gurultu(0,15)).
//
// ---------- HER KATTA, HER DAİREDE BİR KEZ ----------
// Aynı korku tekrar tekrar gelirse korkutmaz, sıkar. Hangi kat ve dairede olduğunu bir
// HashSet'te tutuyoruz.
// HashSet<string>: "string" türünden öğelerin, her birinden yalnızca BİR tane tutulduğu bir küme.
// Köşeli parantezdeki tür (<string>) kümenin ne tuttuğunu söyler; buna "generic" (türü sonradan
// verilen) yapı denir. Contains ile "bu var mı?" diye çok hızlı sorulur, Add ile eklenir.
// "static": bütün aynalar AYNI kümeyi paylaşır ve küme oyun oturumu boyunca yaşar (sahne yeniden
// yüklenmedikçe). Kayda yazılmaz: oyunu yeniden açan oyuncu korkuyu yeniden yaşayabilir.
//
// ---------- GÖLGE NEREYE KONUR? ----------
// Oyuncunun baktığı yönün tam tersine, 1,0-1,3 m uzağa (birkaç uzaklık ve ±40°'ye kadar açı
// denenir). Bir nokta şunları sağlamalı (GezenGolge'nin kurallarıyla aynı aile):
//   - altında oyuncuyla aynı yükseklikte zemin var;
//   - bir insan sığacak kadar boş (duvarın, küvetin içinde belirmesin);
//   - oyuncuyla arasında duvar yok;
//   - göğsü AYNADA görünüyor: göğsün aynadaki görüntüsünden göze giden çizgi camın içinden geçiyor.
// Uygun yer yoksa korku bu sefer olmaz; oyuncu bakmaya devam ederse biraz sonra yine denenir.

using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(260)]   // AynaYansimasi'ndan (250) sonra: bu karenin "çiziliyor" bilgisini okur
[RequireComponent(typeof(AynaYansimasi))]
public class AynaKorkusu : MonoBehaviour
{
    private const float EnUzak = 2.5f;           // oyuncu aynaya bundan yakın olmalı (metre)
    private const float BakmaSuresi = 0.6f;      // ayna ekranın ortasında bu kadar kalmalı (saniye)
    private const float OrtaAlan = 0.4f;         // ekranın ortadaki şu kadarlık kısmı (%40)
    private const float GorunmeSuresi = 2.5f;    // Gölge en fazla bu kadar kalır (saniye)

    [SerializeField] private GameObject sablon;  // Kacis_Golgesi şablonu (kurucu bağlar)
    [SerializeField] private AudioClip vurgu;    // belirirken arkadan gelen kısa ses (kurucu bağlar)

    // Korkunun yaşandığı "kat:daire" anahtarları (bkz. dosyanın başı).
    private static readonly HashSet<string> yasananlar = new HashSet<string>();

    // Statik alanlar sahne yeniden yüklenince SIFIRLANMAZ (#103): kat seçince ya da finalden sonra sahne
    // yeniden yüklenir ve kat baştan oynanır, ama liste dolu kaldığı için o aynalar bir daha korkutmuyordu.
    // RuntimeInitializeOnLoadMethod: oyun başlarken Unity'nin kendiliğinden çağırdığı fonksiyon; burada
    // "her sahne yüklendiğinde listeyi boşalt" diye sahne yükleme olayına (sceneLoaded) abone olunur.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SahneOlayinaBaglan()
    {
        yasananlar.Clear();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SahneYuklendi;   // iki kez abone olmasın
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SahneYuklendi;
    }

    private static void SahneYuklendi(UnityEngine.SceneManagement.Scene sahne, UnityEngine.SceneManagement.LoadSceneMode kip)
    {
        yasananlar.Clear();
    }

    private AynaYansimasi ayna;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrol;
    private KafaLambasi lamba;
    private string daireAdi = "?";
    private float bakmaSayaci;                   // ayna ortadayken işleyen sayaç
    private GameObject golge;                    // şu an aynadaki Gölge (yoksa null)
    private float belirmeZamani;

    private void Start()
    {
        ayna = GetComponent<AynaYansimasi>();
        yonetici = FindAnyObjectByType<KatYoneticisi>();
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        lamba = FindAnyObjectByType<KafaLambasi>();
        // Hangi dairenin aynası? (Dairenin kökünde DaireEksenleri var: "Daire_3" gibi.)
        DaireEksenleri daire = GetComponentInParent<DaireEksenleri>(true);
        if (daire != null) daireAdi = daire.name;
    }

    private void LateUpdate()
    {
        if (ayna == null) return;

        // Gölge görünüyorsa: ne zaman kaybolacağına bak.
        if (golge != null)
        {
            bool sureDoldu = Time.time - belirmeZamani > GorunmeSuresi;
            // Ayna ekrandan çıktıysa (oyuncu döndü) ya da artık çizilmiyorsa Gölge gider.
            if (sureDoldu || !ayna.Ciziliyor || !EkranOrtasinda(1f)) Bitir(true);
            return;
        }

        if (!Uygun()) { bakmaSayaci = 0f; return; }

        // Oyuncu yakında ve ayna ekranın ortasında mı? Öyleyse sayaç işler, değilse sıfırlanır.
        Camera goz = ayna.OyuncuKamerasi;
        bool bakiyor = ayna.Ciziliyor
                       && Vector3.Distance(goz.transform.position, ayna.Merkez) <= EnUzak
                       && EkranOrtasinda(OrtaAlan);
        bakmaSayaci = bakiyor ? bakmaSayaci + Time.deltaTime : 0f;
        if (bakmaSayaci >= BakmaSuresi) Baslat();
    }

    // Korku şu an olabilir mi? (Varlık huzursuz, av yok, oyuncu oynuyor, bu katta daha olmadı.)
    private bool Uygun()
    {
        if (sablon == null || ayna.Gizli || ayna.OyuncuKamerasi == null) return false;
        if (Varlik.Kapali || Varlik.Ornek == null) return false;          // test botu, kat denetimi
        if (Varlik.Ornek.Kademe < 1 || Varlik.Ornek.AvSuruyor) return false;
        if (FigurYurumesi.SahnedeBaskasiVar(golge)) return false;          // #142 (A3): sahnede başka bir figür var
        if (yonetici == null || !yonetici.GolgeSayabilir) return false;   // açılış, kat kapısı açık, çatı
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        if (kontrol == null || !kontrol.enabled || Time.timeScale <= 0f) return false;   // menü, not, inceleme
        return !yasananlar.Contains(Anahtar());
    }

    // Bulunulan kat (#103): MevcutKat ulaşılan en yüksek kattır; oyuncu aşağı indiğinde sahanlık alt katın
    // dairesini gösterir ve korku o katın sayılmalıdır.
    private string Anahtar() { return (yonetici != null ? yonetici.BulunulanKat : 0) + ":" + daireAdi; }

    // Aynanın merkezi ekranın ortadaki "alan" kadarlık kısmında mı? (alan = 1: ekranın herhangi bir yeri.)
    // WorldToViewportPoint: dünya noktasının ekrandaki yeri, 0-1 arası (0,5; 0,5 ekranın ortası);
    // z kameradan uzaklık (eksiyse nokta kameranın arkasında).
    private bool EkranOrtasinda(float alan)
    {
        Camera goz = ayna.OyuncuKamerasi;
        if (goz == null) return false;
        Vector3 e = goz.WorldToViewportPoint(ayna.Merkez);
        float yari = alan * 0.5f;
        return e.z > 0f && Mathf.Abs(e.x - 0.5f) <= yari && Mathf.Abs(e.y - 0.5f) <= yari;
    }

    private void Baslat()
    {
        bakmaSayaci = 0f;
        if (!YerBul(out Vector3 yer))
        {
            // Yer yok (oyuncu sırtını duvara vermiş): bu sefer olmasın, yarım saniye sonra yine dene.
            bakmaSayaci = -0.5f;
            return;
        }

        // Gölge oyuncuya (sırtına) dönük durur.
        Vector3 ayak = kontrol.transform.position;
        Vector3 bakis = ayak - yer; bakis.y = 0f;
        golge = Instantiate(VarlikGorunusu.Sablon(sablon), yer, bakis.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);   // #140: figür torbası (VarlikGorunusu)
        golge.name = "Ayna_Golgesi";
        // Bütün parçaları 30. katmana: yalnızca aynada görünsün. Gölge düşürmesin: oyuncu kamerası
        // bu katmanı çizmese de gölgesi yere düşebilirdi ve "aynada var, odada yok" bozulurdu.
        foreach (Transform t in golge.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = AynaYansimasi.AynaKatmani;
        foreach (Renderer r in golge.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        belirmeZamani = Time.time;
        yasananlar.Add(Anahtar());

        // İmza (#105) ve arkadan kısa bir gıcırtı (Gölge'nin durduğu yerden, 3B), müzik kabarır, Varlık duyar.
        GolgeImzasi.Cal(yer + Vector3.up * 1.2f, 0.7f);
        if (vurgu != null)
        {
            GameObject g = new GameObject("Ayna_Sesi");
            g.transform.position = yer + Vector3.up * 0.2f;
            AudioSource a = g.AddComponent<AudioSource>();
            a.clip = vurgu; a.spatialBlend = 1f; a.volume = 0.6f; a.minDistance = 1f; a.maxDistance = 8f;
            a.Play();
            Destroy(g, vurgu.length + 0.1f);
        }
        ArkaMuzik.Ornek?.Kabar();
        Varlik.Gurultu(0.15f);
        Debug.Log($"[AYNA] {Anahtar()}: Gölge aynada belirdi ({Vector3.Distance(yer, ayak):0.00} m arkada).");
    }

    // Oyuncunun arkasında, aynada görünen, boş ve zeminli bir nokta arar.
    private bool YerBul(out Vector3 yer)
    {
        yer = Vector3.zero;
        Camera goz = ayna.OyuncuKamerasi;
        Vector3 ayak = kontrol.transform.position;                 // oyuncunun kökü ayak hizasında
        Vector3 geri = -goz.transform.forward; geri.y = 0f;
        if (geri.sqrMagnitude < 0.01f) return false;
        geri.Normalize();

        float[] uzakliklar = { 1.15f, 1.0f, 1.3f };
        float[] acilar = { 0f, -20f, 20f, -40f, 40f };
        foreach (float a in acilar)
            foreach (float m in uzakliklar)
            {
                Vector3 aday = ayak + Quaternion.Euler(0f, a, 0f) * geri * m;
                // Zemin: 1 m yukarıdan aşağı ışın; oyuncunun ayağıyla aynı yükseklikte olmalı.
                if (!Physics.Raycast(aday + Vector3.up, Vector3.down, out RaycastHit zemin, 2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (Mathf.Abs(zemin.point.y - ayak.y) > 0.3f) continue;
                aday.y = zemin.point.y;
                // Boşluk: bir insan kapsülü sığıyor mu?
                if (Physics.CheckCapsule(aday + Vector3.up * 0.35f, aday + Vector3.up * 1.6f, 0.22f, ~0, QueryTriggerInteraction.Ignore)) continue;
                // Oyuncuyla arasında duvar yok (Linecast içinden başladığı oyuncu kapsülünü görmez).
                if (Physics.Linecast(ayak + Vector3.up * 1.2f, aday + Vector3.up * 1.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                // Aynada görünüyor mu? Göğsün (1,3 m) yansımasından göze giden çizgi camdan geçmeli.
                if (!AynadaGorunur(aday + Vector3.up * 1.3f, goz.transform.position)) continue;
                yer = aday;
                return true;
            }
        return false;
    }

    // Bir nokta aynada görünür mü? Noktayı ayna düzlemine göre katlarız (yansıması aynanın arkasına
    // düşer); gözden o yansımaya giden çizginin ayna düzlemini kestiği yer camın içindeyse görünür.
    private bool AynadaGorunur(Vector3 nokta, Vector3 goz)
    {
        Vector3 n = ayna.Normal, c = ayna.Merkez;
        Vector3 yansima = nokta - 2f * Vector3.Dot(nokta - c, n) * n;
        float payda = Vector3.Dot(yansima - goz, n);
        if (Mathf.Abs(payda) < 0.0001f) return false;
        float t = Vector3.Dot(c - goz, n) / payda;               // çizginin düzlemi kestiği oran (0-1 arası olmalı)
        if (t <= 0f || t >= 1f) return false;
        Vector3 kesisim = goz + (yansima - goz) * t;
        Vector3 fark = kesisim - c;
        // %90: camın tam kenarında yarım görünen bir Gölge olmasın.
        return Mathf.Abs(Vector3.Dot(fark, ayna.Sag)) <= ayna.YariGenislik * 0.9f
            && Mathf.Abs(Vector3.Dot(fark, ayna.Yukari)) <= ayna.YariYukseklik * 0.9f;
    }

    // Gölge kaybolur; titreme: "gördün mü?" anı.
    private void Bitir(bool titret)
    {
        if (golge != null) Destroy(golge);
        golge = null;
        if (titret && lamba != null) lamba.Titret(0.35f);
    }

    // Daire kapanırsa (kat değişti, lavabo gizlendi) Gölge de gider.
    private void OnDisable() { Bitir(false); bakmaSayaci = 0f; }
}
