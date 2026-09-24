// FinalSahnesi.cs
// Oyunun sonu (#16): çatı kapısı açılır, sabah olur, ekran kararır ve binaya
// DIŞARIDAN bakılır. Bina dokuz katlıdır; oyuncu sekiz kat çıkmıştı. Jenerik akar,
// oyun baştan başlar.
//
// Dış cephe ayrı bir sahne değil: bu nesnenin altında, merdiven boşluğundan çok
// uzakta (x = 400) duran basit bir bina, bir sokak ve bir kamera. Oyuncunun
// kamerası o tarafı hiçbir zaman görmüyor; final başlayınca kamera değiştiriliyor.
//
// SIRA
//   1. KatYoneticisi çatı kapısını açınca CatiAcildi() çağrılır: ışık birkaç saniyede
//      şafağa döner, güvercinler havalanır.
//   2. Oyuncu kapıdan çatıya çıkınca 5 saniye beklenir (sabahın tadı çıkarılsın).
//   3. Ekran kararır; oyuncunun kamerası kapanır, dış kamera açılır.
//   4. Kararma açılır; kamera sokaktan geri çekilip yükselir, bütün bina kadraja girer.
//   5. Jenerik akar, ekran kararır, sahne baştan yüklenir.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalSahnesi : MonoBehaviour
{
    [Header("Süreler (saniye)")]
    [SerializeField] private float catidaBekleme = 5f;
    [SerializeField] private float kararma = 2f;
    [SerializeField] private float kameraSuresi = 16f;
    [SerializeField] private float jenerikSuresi = 16f;

    // Kameranın başlangıç ve bitiş duruşu (bu nesneye göre yerel): önce sokakta,
    // alçakta, giriş kapısına ve tabelaya; sonra geride ve yüksekte, bütün bina.
    private static readonly Vector3 KameraBas = new Vector3(0f, 1.6f, -10f);
    private static readonly Vector3 BakisBas  = new Vector3(0f, 7f, 0f);
    private static readonly Vector3 KameraSon = new Vector3(0f, 13f, -52f);
    private static readonly Vector3 BakisSon  = new Vector3(0f, 16.5f, 0f);

    // Şafak ışığı ayarları (Safak() bunlara doğru geçiş yapar).
    // Ortam ışığı gecenin neredeyse siyahından yüksek bir değere çıkıyor: şafakta
    // aydınlık gökyüzü her yeri doldurur, gölgeler bile gri kalır. Değer yüksek
    // görünüyor, çünkü sahnenin post-processing'i (ACES + kontrast + gölgelere mavi
    // katan split toning) gece için kuruldu ve koyu tonları eziyor: 0,30 ve 0,46
    // denendi, cephe dışında her şey (yol, arsa, komşular) kapkara kaldı.
    public static readonly Color SafakOrtam = new Color(0.78f, 0.78f, 0.85f);
    public static readonly Color SafakSis = new Color(0.62f, 0.56f, 0.58f);
    public static readonly Color SafakGunesRengi = new Color(1f, 0.74f, 0.55f);
    // Güneşin yönü: 22° yükseklik, ışık +z'ye (cepheye doğru) gidiyor. 14° denendi:
    // yatay yüzeylere (yol, arsa) neredeyse hiç vurmuyor, kadrajın altı kararıyordu.
    public static readonly Vector3 SafakGunesDonus = new Vector3(22f, 25f, 0f);
    public const float SafakGunesSiddeti = 1.6f;

    private Transform oyuncu;
    private bool basladi;
    private float karartma;            // 0 = açık, 1 = simsiyah (OnGUI çizer)
    private float jenerikKonum = -1f;  // -1 = jenerik yok; 0..1 arası akış

    private Camera finalKamera;

    private readonly string[] jenerik =
    {
        "HUZUR APARTMANI",
        "",
        "Tasarım ve Geliştirme",
        "Şahan Boz",
        "",
        "Birlikte",
        "Kubilay",
        "",
        "",
        "Oynadığın için teşekkürler."
    };

    private void Awake()
    {
        Transform k = transform.Find("Final_Kamera");
        if (k != null) finalKamera = k.GetComponent<Camera>();
    }

    // ---------- 1. ÇATI KAPISI AÇILDI ----------
    public void CatiAcildi(Transform oyuncuTransform)
    {
        oyuncu = oyuncuTransform;
        StartCoroutine(Safak());

        // Ürken güvercinler: çatının ortasından havalanan bir sürü.
        AudioClip kanat = Resources.Load<AudioClip>("Sesler/Guvercin_Kanat");
        if (kanat != null) AudioSource.PlayClipAtPoint(kanat, new Vector3(0f, 4.5f, -3.5f), 1f);
    }

    // Gece ışığından şafağa 4 saniyede geçiş. Sahne her final sonunda baştan
    // yüklendiği için eski değerleri saklayıp geri koymaya gerek yok.
    private IEnumerator Safak()
    {
        Light ay = null;
        foreach (Light l in FindObjectsByType<Light>()) if (l.type == LightType.Directional) ay = l;

        Color ortamIlk = RenderSettings.ambientLight, sisIlk = RenderSettings.fogColor;
        float sisBasIlk = RenderSettings.fogStartDistance, sisSonIlk = RenderSettings.fogEndDistance;
        Color ayRenkIlk = ay != null ? ay.color : Color.white;
        float aySiddetIlk = ay != null ? ay.intensity : 0f;
        Quaternion ayDonusIlk = ay != null ? ay.transform.rotation : Quaternion.identity;

        // Şafak: güneş ufkun hemen üstünde, KAMERANIN ARKASINDA; alçak, sıcak ışık
        // cepheyi önden aydınlatır. İlk denemede güneş binanın arkasındaydı ve bina
        // karanlık bir siluete dönüştü: pencereler ve kat bantları kayboldu, katları
        // saymak imkânsızlaştı. Sürprizin okunması katların SAYILABİLMESİNE bağlı.
        Color ortamSon = SafakOrtam;
        Color sisSon = SafakSis;
        Color ayRenkSon = SafakGunesRengi;
        Quaternion ayDonusSon = Quaternion.Euler(SafakGunesDonus);

        for (float t = 0f; t < 1f; t += Time.deltaTime / 4f)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);
            RenderSettings.ambientLight = Color.Lerp(ortamIlk, ortamSon, s);
            RenderSettings.fogColor = Color.Lerp(sisIlk, sisSon, s);
            RenderSettings.fogStartDistance = Mathf.Lerp(sisBasIlk, 40f, s);
            RenderSettings.fogEndDistance = Mathf.Lerp(sisSonIlk, 260f, s);
            if (ay != null)
            {
                ay.color = Color.Lerp(ayRenkIlk, ayRenkSon, s);
                ay.intensity = Mathf.Lerp(aySiddetIlk, SafakGunesSiddeti, s);
                ay.transform.rotation = Quaternion.Slerp(ayDonusIlk, ayDonusSon, s);
            }
            yield return null;
        }
    }

    // ---------- 2. OYUNCU ÇATIYA ÇIKTI MI ----------
    private void Update()
    {
        if (basladi || oyuncu == null) return;

        // Çatı döşemesi y = 3'te; kapı z = -1,2'de. Kapıdan dışarı adım atınca final başlar.
        Vector3 p = oyuncu.position;
        if (p.y > 2.5f && p.z < -1.6f)
        {
            basladi = true;
            StartCoroutine(Final());
        }
    }

    // ---------- 3-5. FİNAL ----------
    private IEnumerator Final()
    {
        yield return new WaitForSeconds(catidaBekleme);

        yield return Karart(0f, 1f);

        // Oyuncuyu durdur, onun kamerasını ve kulağını kapat.
        Camera oyuncuKamera = oyuncu.GetComponentInChildren<Camera>();
        if (oyuncuKamera != null)
        {
            oyuncuKamera.enabled = false;
            AudioListener kulak = oyuncuKamera.GetComponent<AudioListener>();
            if (kulak != null) kulak.enabled = false;
        }
        foreach (MonoBehaviour b in oyuncu.GetComponents<MonoBehaviour>()) b.enabled = false;

        // Test göstergesi ve kat mantığı artık çizilmesin, çalışmasın.
        KatYoneticisi yonetici = FindAnyObjectByType<KatYoneticisi>();
        if (yonetici != null) yonetici.enabled = false;

        // Dış kamerayı ve onun sokak sesini aç.
        finalKamera.gameObject.SetActive(true);
        KamerayiYerlestir(0f);

        yield return Karart(1f, 0f);

        // Kamera yavaşça geri çekilip yükselir; katlar teker teker geçer.
        for (float t = 0f; t < 1f; t += Time.deltaTime / kameraSuresi)
        {
            KamerayiYerlestir(Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        KamerayiYerlestir(1f);

        yield return new WaitForSeconds(3f);

        // Jenerik: ekranın altından yukarı akar.
        for (jenerikKonum = 0f; jenerikKonum < 1f; jenerikKonum += Time.deltaTime / jenerikSuresi)
            yield return null;

        yield return Karart(0f, 1f);
        yield return new WaitForSeconds(1f);

        // Oyun baştan: sahne yeniden yüklenir, her şey (ışık, katlar, kartlar) ilk hâline döner.
        SceneManager.LoadScene(gameObject.scene.name);
    }

    private void KamerayiYerlestir(float s)
    {
        Vector3 konum = Vector3.Lerp(KameraBas, KameraSon, s);
        Vector3 bakis = Vector3.Lerp(BakisBas, BakisSon, s);
        finalKamera.transform.position = transform.TransformPoint(konum);
        finalKamera.transform.rotation = Quaternion.LookRotation(transform.TransformPoint(bakis) - finalKamera.transform.position);
    }

    private IEnumerator Karart(float bas, float son)
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / kararma)
        {
            karartma = Mathf.Lerp(bas, son, t);
            yield return null;
        }
        karartma = son;
    }

    // ---------- ÇİZİM ----------
    private GUIStyle baslik, satir;

    private void OnGUI()
    {
        GUI.depth = -100;   // her şeyin üstünde

        if (jenerikKonum >= 0f)
        {
            if (baslik == null)
            {
                baslik = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                baslik.normal.textColor = new Color(0.95f, 0.92f, 0.85f);
                satir = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
                satir.normal.textColor = new Color(0.9f, 0.88f, 0.82f);
            }

            // Metnin arkasına hafif bir karartma: gökyüzünde de okunsun.
            GUI.color = new Color(0f, 0f, 0f, 0.45f * Mathf.Clamp01(jenerikKonum * 6f));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float satirBoyu = 40f;
            float toplam = jenerik.Length * satirBoyu;
            float y = Mathf.Lerp(Screen.height, -toplam, jenerikKonum);
            for (int i = 0; i < jenerik.Length; i++)
            {
                GUI.Label(new Rect(0, y + i * satirBoyu, Screen.width, satirBoyu * (i == 0 ? 1.6f : 1f)),
                          jenerik[i], i == 0 ? baslik : satir);
            }
        }

        if (karartma > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, karartma);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
