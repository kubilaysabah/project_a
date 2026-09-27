// TelefonZili.cs
// Kapıcı odasının telefonu çalıyor (#56, Docs/hikaye.md §10): 2. ve 5. kata ilk varışta.
// Oyuncu yukarıdayken zil merdiven boşluğundan boğuk ve kısık duyulur; aşağı indikçe yükselir,
// zemin katta net çalar. Açmak için inmek İSTEĞE BAĞLI (Şahan'ın seçimi): inen oyuncu telefonu
// açar ve konuşmayı okur (Telefon); inmeyen oyuncu bir süre sonra panoda bir not bulur.
//
// NEDEN AYRI BİR NESNE
// Telefon zemin katta; zemin kat ise oyuncu 2. kattan yukarıdayken kapalı (KatYoneticisi.ZeminiAyarla)
// ve kapalı bir nesnenin sesi çalmaz. Zil bu yüzden kat yöneticisinin çocuğu olan, hep açık bir
// nesnede durur ve kendi 2B sesini çalar; seviyesini oyuncunun bulunduğu kata göre ayarlar.

using UnityEngine;

public class TelefonZili : MonoBehaviour
{
    public static TelefonZili Ornek { get; private set; }

    private const float CalmaSuresi = 150f;    // bu kadar açılmazsa panoya not düşer (saniye)

    private AudioSource kaynak;
    private AudioLowPassFilter boguk;
    private KatYoneticisi yonetici;
    private int caldigiKat;
    private float kalan;

    /// <summary>Zil şu an çalıyor mu?</summary>
    public bool Caliyor { get { return caldigiKat > 0; } }

    // Hangi katta hangi konuşma. Anahtarlar açıkça yazılı: metin bütünlük testi koddaki anahtarları
    // bu yazılardan bulur (birleştirilmiş bir anahtarı göremezdi).
    private static string KonusmaAnahtari(int kat) { return kat == 2 ? "not.telefon_kat2" : "not.telefon_kat5"; }
    private static string KactiAnahtari(int kat) { return kat == 2 ? "not.telefon_kat2_kacti" : "not.telefon_kat5_kacti"; }

    /// <summary>Çalan aramanın konuşması (açılınca okunur).</summary>
    public string Konusma { get { return KonusmaAnahtari(caldigiKat); } }

    private void Awake()
    {
        Ornek = this;
        yonetici = GetComponentInParent<KatYoneticisi>();
        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.clip = Resources.Load<AudioClip>("Sesler/Telefon_Zili");
        kaynak.loop = true;
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;               // 2B: kaynağın yeri değil, oyuncunun katı önemli
        boguk = gameObject.AddComponent<AudioLowPassFilter>();
    }

    private void OnDestroy() { if (Ornek == this) Ornek = null; }

    /// <summary>Bu kat için telefonu çaldırır; konuşma daha önce duyulduysa ya da not düştüyse çalmaz.</summary>
    public void Cal(int kat)
    {
        if (Kayit.NotOkunduMu(KonusmaAnahtari(kat)) || Kayit.NotOkunduMu(KactiAnahtari(kat))) return;
        // Önceki arama hâlâ çalıyorsa (oyuncu hızlı çıktı) o arama kaçmış sayılır: notu panoya düşer.
        if (Caliyor && caldigiKat != kat) Kayit.NotOkundu(KactiAnahtari(caldigiKat));
        caldigiKat = kat;
        kalan = CalmaSuresi;
        if (kaynak.clip != null) kaynak.Play();
        Debug.Log($"[TELEFON] {kat}. kata varıldı: kapıcı odasında telefon çalıyor.");
    }

    /// <summary>Telefon açıldı (Telefon çağırır): zil susar, konuşmanın anahtarı döner.</summary>
    public string Ac()
    {
        string konusma = Konusma;
        Kayit.NotOkundu(konusma);   // panoya da geçsin: yeniden okunabilir
        Durdur();
        Debug.Log($"[TELEFON] Açıldı: {konusma}");
        return konusma;
    }

    private void Durdur()
    {
        caldigiKat = 0;
        kaynak.Stop();
    }

    private void Update()
    {
        if (!Caliyor) return;
        kalan -= Time.deltaTime;
        if (kalan <= 0f)
        {
            // Kimse açmadı: panoya not düşer (inmeyen oyuncu mesajı sonra okur).
            Kayit.NotOkundu(KactiAnahtari(caldigiKat));
            Debug.Log($"[TELEFON] Açılmadı; panoya not düştü ({KactiAnahtari(caldigiKat)}).");
            Durdur();
            return;
        }
        // Oyuncu ne kadar aşağıdaysa zil o kadar yüksek ve net: zeminde tam ses, her kat yukarısı
        // hem kısar hem boğar (merdiven boşluğundan gelen ses tizlerini kaybeder).
        int kat = yonetici != null ? yonetici.BulunulanKat : 0;
        kaynak.volume = kat == 0 ? 0.55f : 0.32f / kat;
        boguk.cutoffFrequency = kat == 0 ? 8000f : Mathf.Max(600f, 2400f / kat);
    }
}
