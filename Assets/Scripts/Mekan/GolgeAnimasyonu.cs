// GolgeAnimasyonu.cs
// Gölge'nin kodla üretilen canlılığı (#90): yürürken bacaklar ve kollar sallanır, gövde seker;
// dururken nefes alır; başı oyuncuya döner; ara sıra seğirir.
//
// Şahan'ın karşılaştırması (2026-10-01): "Gölge canlı değil: kayıyor gibi yürüyor, nefes almıyor,
// kafasını çevirmiyor." Hazır bir yürüme animasyonu (Mixamo gibi) indirmek gerekirdi; onun yerine
// model altı parçaya bölündü (golge.py: Govde, Bas, Kol_Sol, Kol_Sag, Bacak_Sol, Bacak_Sag;
// pivotlar omuz, kalça ve boyunda) ve hareket burada hesaplanıyor. Avantajı: Gölge'yi hareket
// ettiren hiçbir kod (GezenGolge, Varlik, KacisAni, SaklanmaAni) değişmedi; bu bileşen şablonda
// durur ve KENDİ konumunun değişiminden hızı ölçüp ona göre yürür.
//
// YÜRÜYÜŞ DÖNGÜSÜ: faz, alınan yolla ilerler (her 0,75 m'de bir tam adım çifti): hız ne olursa
// olsun ayaklar yere "uyar". Bacaklar sinüsle ±25° öne-arkaya (biri öndeyken öbürü arkada),
// kollar bacakların tersine ±18°, gövde adım başına 3 cm seker (|sin 2φ|). Figür +Z'ye bakar,
// pivotlar eklemlerde: yerel X ekseni etrafında dönüş = öne-arkaya sallanma.
// NEFES: dururken gövde 1,3 Hz'de 4 mm iner kalkar. BAŞ: oyuncuya döner (±70° yatay, ±25° dikey,
// yumuşak). SEĞİRME: 3-7 sn'de bir başın 0,12 sn'lik ani eğilmesi; korkunun küçük ayrıntısı.

using UnityEngine;

public class GolgeAnimasyonu : MonoBehaviour
{
    private const float AdimBoyu = 0.75f;      // bir tam döngünün yolu (metre)
    private const float BacakAcisi = 25f, KolAcisi = 18f;
    private const float Sekme = 0.03f;         // gövde sekmesi (metre)
    private const float NefesGenligi = 0.004f, NefesHizi = 1.3f;
    private const float BasDonusHizi = 6f;     // saniyede kalan yolun katı (yumuşak takip)
    // Oturma (#108): bacaklar kalçadan öne (-X ekseni: aşağı sarkan bacak +Z'ye, yani öne döner), kollar masaya.
    // Tek parça bacak dizden kırılmaz; öne uzanan bacak sofra örtüsünün altında kalır, görünmez.
    private const float OturmaBacak = -85f, OturmaKol = -40f;   // -62'de uzun kol masanın öbür ucuna uzanıyordu
    /// <summary>Sandalyede oturuyor: yürüyüş yok, bacaklar öne, kollar masada; nefes ve baş takibi sürer.</summary>
    public bool Oturuyor;

    private Transform govde, bas, kolSol, kolSag, bacakSol, bacakSag, oyuncu;
    private Quaternion govdeTemel, basTemel, kolSolTemel, kolSagTemel, bacakSolTemel, bacakSagTemel;
    private Vector3 govdeKonum;
    private Vector3 oncekiKonum;
    private float faz, hiz, segirmeZamani, segirmeAcisi;

    private void Awake()
    {
        govde = transform.Find("Govde");
        if (govde == null) { enabled = false; return; }
        bas = govde.Find("Bas"); kolSol = govde.Find("Kol_Sol"); kolSag = govde.Find("Kol_Sag");
        bacakSol = govde.Find("Bacak_Sol"); bacakSag = govde.Find("Bacak_Sag");
        govdeTemel = govde.localRotation; govdeKonum = govde.localPosition;
        if (bas != null) basTemel = bas.localRotation;
        if (kolSol != null) kolSolTemel = kolSol.localRotation;
        if (kolSag != null) kolSagTemel = kolSag.localRotation;
        if (bacakSol != null) bacakSolTemel = bacakSol.localRotation;
        if (bacakSag != null) bacakSagTemel = bacakSag.localRotation;
        oncekiKonum = transform.position;
        segirmeZamani = Time.time + Random.Range(3f, 7f);
    }

    private void Start()
    {
        BirinciSahisKontrolcu k = FindAnyObjectByType<BirinciSahisKontrolcu>();
        oyuncu = k != null ? k.transform : null;
    }

    // LateUpdate: Gölge'yi yürüten kodlar Update'te ve coroutine'lerde taşır; biz sonra ölçeriz.
    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Hız: kendi konumunun değişiminden (yatay). Ani ışınlanmalar (test) döngüyü patlatmasın: 2 m üstü sayılmaz.
        Vector3 fark = transform.position - oncekiKonum; fark.y = 0f;
        oncekiKonum = transform.position;
        float yol = fark.magnitude;
        if (yol > 2f) yol = 0f;
        hiz = Mathf.Lerp(hiz, yol / dt, 1f - Mathf.Exp(-10f * dt));
        faz += yol / AdimBoyu * Mathf.PI * 2f;
        float yurume = Oturuyor ? 0f : Mathf.Clamp01(hiz / 0.5f);   // 0,5 m/sn'den hızlı = tam yürüyüş; otururken yok
        float oturma = Oturuyor ? 1f : 0f;

        float s = Mathf.Sin(faz);
        if (bacakSol != null) bacakSol.localRotation = bacakSolTemel * Quaternion.Euler(s * BacakAcisi * yurume + OturmaBacak * oturma, 0f, 0f);
        if (bacakSag != null) bacakSag.localRotation = bacakSagTemel * Quaternion.Euler(-s * BacakAcisi * yurume + OturmaBacak * oturma, 0f, 0f);
        // Kollar bacakların tersine; dururken hafifçe sarkık (nefesle birlikte 2° salınır).
        float sarkma = Mathf.Sin(Time.time * NefesHizi * Mathf.PI * 2f) * 2f * (1f - yurume);
        if (kolSol != null) kolSol.localRotation = kolSolTemel * Quaternion.Euler(-s * KolAcisi * yurume + sarkma + OturmaKol * oturma, 0f, 0f);
        if (kolSag != null) kolSag.localRotation = kolSagTemel * Quaternion.Euler(s * KolAcisi * yurume + sarkma + OturmaKol * oturma, 0f, 0f);

        // Gövde: adım sekmesi + nefes.
        float sekme = Mathf.Abs(Mathf.Sin(faz * 2f)) * Sekme * yurume;
        float nefes = Mathf.Sin(Time.time * NefesHizi * Mathf.PI * 2f) * NefesGenligi * (1f - yurume * 0.7f);
        govde.localPosition = govdeKonum + Vector3.up * (sekme + nefes);
        // Yürürken gövde hafifçe yana yalpalar.
        govde.localRotation = govdeTemel * Quaternion.Euler(0f, 0f, Mathf.Sin(faz) * 3f * yurume);

        BasiDondur(dt);
    }

    // Baş oyuncuya döner (sınırlı), ara sıra seğirir.
    private void BasiDondur(float dt)
    {
        if (bas == null) return;
        Quaternion hedef = basTemel;
        if (oyuncu != null)
        {
            Vector3 goz = oyuncu.position + Vector3.up * 1.6f;
            Vector3 yerelYon = govde.InverseTransformDirection(goz - bas.position);
            // Yatay açı (y ekseni) ve dikey açı (x ekseni), figür +Z'ye bakar.
            float yatay = Mathf.Atan2(yerelYon.x, yerelYon.z) * Mathf.Rad2Deg;
            float dikey = -Mathf.Atan2(yerelYon.y, new Vector2(yerelYon.x, yerelYon.z).magnitude) * Mathf.Rad2Deg;
            yatay = Mathf.Clamp(yatay, -70f, 70f);
            dikey = Mathf.Clamp(dikey, -25f, 25f);
            hedef = basTemel * Quaternion.Euler(dikey, yatay, 0f);
        }
        // Seğirme: zamanı gelince başa ani bir yan eğim, 0,12 sn sonra geri.
        if (Time.time >= segirmeZamani)
        {
            segirmeAcisi = Random.Range(10f, 18f) * (Random.value < 0.5f ? -1f : 1f);
            segirmeZamani = Time.time + Random.Range(3f, 7f);
        }
        if (Mathf.Abs(segirmeAcisi) > 0.01f)
        {
            hedef *= Quaternion.Euler(0f, 0f, segirmeAcisi);
            segirmeAcisi = Mathf.MoveTowards(segirmeAcisi, 0f, 120f * dt);
        }
        bas.localRotation = Quaternion.Slerp(bas.localRotation, hedef, 1f - Mathf.Exp(-BasDonusHizi * dt));
    }
}
