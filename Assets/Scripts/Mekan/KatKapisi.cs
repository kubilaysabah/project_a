// KatKapisi.cs
// Katlar arasındaki kilitli demir parmaklıklı kapı (#52).
//
// Çıkış kolunun tepesinde, ara sahanlığın girişinde durur (bkz. kat_kapisi.py). Kapalıyken
// kanadın çarpışma kutusu oyuncuyu durdurur; oyuncu kilitli kapıya E basınca kapı sarsılır
// ("KİLİTLİ"). Elinde bu katın anahtarı varsa "AÇ (E)": kilit sesi, kanat menteşesinde
// gıcırdayarak 95° döner ve açık kalır. Aşağı inen oyuncu açık kapılardan geçer.
//
// Kapının hangi katın kapısı olduğunu ve açık mı kapalı mı duracağını KatYoneticisi
// söyler (Ayarla): sahanlık kat N'yi gösteriyorsa kapı N'nin kapısıdır; N'nin anahtarı
// kullanıldıysa açıktır. Aynı kapı nesnesi her katta yeniden kullanılır (döngü hilesi).

using System.Collections;
using UnityEngine;

public class KatKapisi : MonoBehaviour, IEtkilesimli
{
    private const float AcikAci = 95f;      // kanat merdivene doğru bu kadar döner
    private const float AcilmaSuresi = 1.6f;

    [SerializeField] private Transform kanat;    // menteşe hattında pivotlu kanat
    [SerializeField] private int kat;
    [SerializeField] private bool acik;

    private static AudioClip kilitli, kilitAcilma, gicirti;
    private Coroutine hareket;
    private float kanatAcisi;                     // 0 kapalı .. AcikAci açık

    public int Kat { get { return kat; } }
    public bool Acik { get { return acik; } }

    /// <summary>Kapı açıldı (kat numarasıyla). KatYoneticisi kaydeder ve katı ilerletir.</summary>
    public event System.Action<int> Acildi;

    /// <summary>Anahtar kapıya takıldı, kapı açılmaya başladı (kat numarasıyla). Kaçış anı (#58)
    /// bunu dinler: kapıya ulaşıp anahtarı takan oyuncu kaçmış sayılır, kanadın açılmasını beklemez.</summary>
    public event System.Action<int> AcilisBasladi;

    private void Awake()
    {
        if (kanat == null) kanat = transform.Find("Kanat");
        if (kilitli == null) kilitli = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilitli");
        if (kilitAcilma == null) kilitAcilma = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilit");
        if (gicirti == null) gicirti = Resources.Load<AudioClip>("Sesler/KatKapisi_Gicirti");
    }

    public string Ipucu
    {
        get { return Cep.Tasiyor(EsyaTuru.KatAnahtari, kat) ? Dil.Getir("ipucu.ac") : Dil.Getir("ipucu.kilitli"); }
    }

    public bool EtkilesimeAcik { get { return !acik && hareket == null; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!Cep.Kullan(EsyaTuru.KatAnahtari, kat))
        {
            // Anahtar yok: kapıyı sarsar, çubuklar şangırdar. Cevap kapının kendisi.
            if (kilitli != null) AudioSource.PlayClipAtPoint(kilitli, transform.position, 0.8f);
            return;
        }
        AcilisBasladi?.Invoke(kat);
        hareket = StartCoroutine(Ac());
    }

    private IEnumerator Ac()
    {
        if (kilitAcilma != null) AudioSource.PlayClipAtPoint(kilitAcilma, transform.position, 0.8f);
        yield return new WaitForSeconds(0.7f);
        if (gicirti != null) AudioSource.PlayClipAtPoint(gicirti, transform.position, 0.8f);
        float bas = kanatAcisi;
        for (float t = 0f; t < AcilmaSuresi; t += Time.deltaTime)
        {
            // Yavaş başlayıp yavaş biten bir eğri (sinüsün yarısı): ağır demir kanat.
            float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(t / AcilmaSuresi));
            KanadiDondur(Mathf.Lerp(bas, AcikAci, e));
            yield return null;
        }
        KanadiDondur(AcikAci);
        acik = true;
        hareket = null;
        Debug.Log($"[KAPI] {kat}. katın kapısı açıldı.");
        Acildi?.Invoke(kat);
    }

    /// <summary>Sahanlık yeni bir katı gösteriyor: kapı o katın; açıksa hemen açık dursun.</summary>
    public void Ayarla(int yeniKat, bool acikDursun)
    {
        if (hareket != null) { StopCoroutine(hareket); hareket = null; }
        kat = yeniKat;
        acik = acikDursun;
        KanadiDondur(acikDursun ? AcikAci : 0f);
    }

    private void KanadiDondur(float aci)
    {
        kanatAcisi = aci;
        // Kanat menteşe hattından merdiven gözüne (-X) uzanır; +Y ekseninde eksi açı onu
        // merdivene (+Z) doğru çevirir. Kanadın çarpışma kutusu kanatla döner.
        if (kanat != null) kanat.localRotation = Quaternion.Euler(0f, -aci, 0f);
    }
}
