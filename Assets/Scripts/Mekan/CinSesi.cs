// CinSesi.cs
// Üç harflinin GÖRÜNÜŞ SESİ (#143, madde 10). Şahan'ın 13 Ekim testi: "üç harfliler çıkınca görsel olarak
// korkuyorum ama arkadan korkutucu sesler çıkmıyor." Anahtarlık imzası (GolgeImzasi, #105) belirişten ÖNCE
// gelen bir haberci; bu sınıf belirişin KENDİSİNİN sesidir ve iki yerden çalar:
//   1. STINGER — figürün yerinden (3B): tırmanan uyumsuz bir küme + alt-bas darbe (Cin_Stinger_1/2, sentez,
//      cin_sesleri_uret.py). Oyuncu figürün nerede olduğunu kulağıyla da bulur.
//   2. ARKADAN — oyuncunun tam arkasından (3B, 1,2 m geride, kulak hizasında): alçak bir uğultu (Cin_Ugultu,
//      sentez) ve VARSA bir nefes (Resources/Sesler/Nefes_Cin_1..3; canlı ses, Şahan kaydeder, sentezlenmez;
//      dosya yoksa sessizce atlanır). "Arkamda bir şey var" hissi: figür önde dursa bile ses arkadan gelir.
//
// Kullanan yerler (hepsi tek fonksiyonu çağırır): cin olayları (belirme, karartmalı yaklaşma, geçiş, kaçış),
// gezen gölge, av, 7. kat kovalamacası, 6. kat saklanma anı, ayna korkusu, duvardaki gölge (yalnız arkadan),
// dokuzuncu katta kalkan figür. Yeni bir beliriş eklenirse önce GolgeImzasi.Cal, sonra CinSesi.Belirdi.
//
// YENİ KAVRAM — "static class": örneği yaratılmayan, yalnız fonksiyon taşıyan sınıf; sahneye konmaz, her
// yerden adıyla çağrılır (GolgeImzasi gibi). Ses kaynaklarını çaldığı anda yaratır, bitince siler.
using UnityEngine;

public static class CinSesi
{
    public const float StingerSes = 0.9f;
    public const float UgultuSes = 0.7f;
    public const float NefesSes = 0.8f;
    public const float ArkaMesafe = 1.2f;       // uğultu ve nefes oyuncunun bu kadar gerisinden
    public const float EnAzAra = 1.5f;          // iki beliriş sesi arasında en az bu kadar saniye (üst üste binmesin)

    private static AudioClip[] stingerler, nefesler;
    private static AudioClip ugultu;
    private static bool arandi;
    private static float sonCalma = -99f;

    /// <summary>Test göstergesi: son çalmanın zamanı (Time.time); -99 = hiç.</summary>
    public static float SonCalma { get { return sonCalma; } }

    private static void Ara()
    {
        if (arandi) return;
        arandi = true;
        var s = new System.Collections.Generic.List<AudioClip>();
        for (int i = 1; i <= 2; i++) { AudioClip c = Resources.Load<AudioClip>("Sesler/Cin_Stinger_" + i); if (c != null) s.Add(c); }
        stingerler = s.ToArray();
        ugultu = Resources.Load<AudioClip>("Sesler/Cin_Ugultu");
        var n = new System.Collections.Generic.List<AudioClip>();
        for (int i = 1; i <= 3; i++) { AudioClip c = Resources.Load<AudioClip>("Sesler/Nefes_Cin_" + i); if (c != null) n.Add(c); }
        nefesler = n.ToArray();
    }

    /// <summary>
    /// Figür 'yer'de belirdi: stinger oradan, uğultu ve nefes oyuncunun arkasından. guc 0-1 bütün sesleri ölçekler
    /// (uzak/sessiz belirişler için). stingerli=false: yalnız arkadan uğultu ve nefes (duvardaki gölge, karanlıkta
    /// yaklaşma gibi "vuruşsuz" anlar).
    /// </summary>
    public static void Belirdi(Vector3 yer, float guc = 1f, bool stingerli = true)
    {
        Ara();
        if (Time.time - sonCalma < EnAzAra) return;
        sonCalma = Time.time;
        guc = Mathf.Clamp01(guc);
        if (stingerli && stingerler != null && stingerler.Length > 0)
            Cal(stingerler[Random.Range(0, stingerler.Length)], yer, StingerSes * guc, 2f, 25f, "Cin_Stinger");
        Transform oyuncu = OyuncuBul();
        if (oyuncu == null) return;
        // Arkası: oyuncunun baktığı yönün tersi, kulak hizası.
        Vector3 arka = oyuncu.position + Vector3.up * 1.6f - oyuncu.forward * ArkaMesafe;
        if (ugultu != null) Cal(ugultu, arka, UgultuSes * guc, 0.8f, 6f, "Cin_Ugultu");
        if (nefesler != null && nefesler.Length > 0)
            Cal(nefesler[Random.Range(0, nefesler.Length)], arka, NefesSes * guc, 0.8f, 6f, "Cin_Nefes");
        Debug.Log($"[CIN SES] beliriş sesi: figür {yer:F1}, arkadan {(nefesler != null && nefesler.Length > 0 ? "uğultu+nefes" : "uğultu")}, güç {guc:0.00}.");
    }

    private static Transform OyuncuBul()
    {
        BirinciSahisKontrolcu k = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        return k != null ? k.transform : null;
    }

    private static void Cal(AudioClip klip, Vector3 yer, float ses, float yakin, float uzak, string ad)
    {
        GameObject g = new GameObject(ad);
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip;
        a.spatialBlend = 1f;
        a.volume = Mathf.Clamp01(ses);
        a.minDistance = yakin;
        a.maxDistance = uzak;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();
        Object.Destroy(g, klip.length + 0.1f);
    }
}
