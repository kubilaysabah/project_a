// KacisAni.cs
// Senaryolu kaçış anı (#58, 7. kat; Docs/hikaye.md §9).
//
// Katın anahtarı alındığı an kafa lambası titrer, asansörün sesi gelir. Birkaç saniye sonra
// karanlık bir siluet anahtarın alındığı yerden çıkar ve oyuncunun o andan beri YÜRÜDÜĞÜ YOLU
// izler (oyuncunun konumları kısa aralıklarla "iz" olarak kaydedilir; siluet izleri sırayla
// takip eder). Böylece duvarların içinden geçmez, kapılardan ve merdivenden oyuncunun geçtiği
// yerden geçer. Siluet oyuncudan biraz hızlıdır: duraksamadan kat kapısına giden yetişir,
// oyalanan yakalanır.
//
//   Kaçtı    : oyuncu anahtarı katın kapısına takar (KatKapisi.AcilisBasladi) -> siluet kaybolur.
//   Yakalandı: siluet oyuncuya 0,8 m'den yaklaşır -> ekran kararır, KAT BAŞTAN başlar
//              (Şahan'ın kararı, 2026-09-27): katın kilitleri ve parçaları kayıttan silinir,
//              anahtar elden düşer, oyuncu sahanlığın başında uyanır. Oyun değil, yalnızca kat.
//              Bu işi KatYoneticisi.KacistaYakalandi yapar (neden orada: oraya bakın).
// Kaçış kayıtta "kacis" olarak işaretlenir ve bir daha olmaz; yakalanınca bu işaret de silinir,
// kat yeniden oynanırken Gölge yine çıkar.
//
// Bu bileşeni zincir uygulayıcısı "kaçışı olan" katın dairesine ekler; kat değişince söker.

using System.Collections.Generic;
using UnityEngine;

public class KacisAni : MonoBehaviour
{
    private const float IzAraligi = 0.25f;       // iz kaydı: saniyede dört konum
    private const float YakalamaMesafesi = 0.8f;
    private const float AdimAraligi = 0.55f;

    private int kat;
    private GameObject sablon;
    private float gecikme, hiz;
    private AudioClip baslangicSesi, haberciSesi;

    private enum Durum { Bekliyor, Basladi, Bitti }
    private Durum durum = Durum.Bekliyor;

    /// <summary>Kovalamaca şu an sürüyor mu (silüet oyuncunun peşinde)? Saklanma yeri bakar (#103).</summary>
    public bool Suruyor { get { return durum == Durum.Basladi; } }
    private readonly List<Vector3> izler = new List<Vector3>();
    private float izSayaci, adimSayaci, gecenSure;   // gecenSure: kaçış başladığından beri ETKİN geçen süre (#112)
    private BirinciSahisKontrolcu kontrol;
    private int hedefIz;
    private GameObject golge;
    private Transform oyuncu;
    private static AudioClip[] adimlar;

    // ---- Dokuzuncu kat (#108) ----
    // HazirGolge: şablondan kopya yerine sahnede zaten duran bir Gölge kovalar (sofrada oturan). Verilirse
    // bekleme bitince olduğu yerden kalkıp izi sürmeye başlar; yok edilmez, yalnızca durdurulur.
    // YakalanmaIsleyici: yakalanınca KatYoneticisi'nin kat baştan kurması yerine çağrılacak iş (dokuzuncu kat
    // kayda girmez, kendi başa dönüşü var). Bir "delegate" (temsilci): bir fonksiyonu değişken gibi tutar.
    // KapidanKacis: 7. katta kaçış kat kapısı açılınca biter; dokuzuncu katta kabin kapısı kapanınca (Kacti()).
    public GameObject HazirGolge;
    public System.Action<int> YakalanmaIsleyici;
    public bool KapidanKacis = true;

    public void Hazirla(int katNo, GameObject golgeSablonu, float bekleme, float hizi, AudioClip ilkSes, AudioClip haberci)
    {
        kat = katNo;
        sablon = golgeSablonu;
        gecikme = bekleme;
        hiz = hizi;
        baslangicSesi = ilkSes;
        haberciSesi = haberci;
        BirinciSahisKontrolcu k = FindAnyObjectByType<BirinciSahisKontrolcu>();
        kontrol = k;
        oyuncu = k != null ? k.transform : null;
        // Kat kapısı açılınca kaçış biter. Olay (event): kapı açıldığında haber veren liste.
        if (KapidanKacis) foreach (KatKapisi kapi in FindObjectsByType<KatKapisi>()) kapi.AcilisBasladi += KapiAcildi;
        if (adimlar == null)
        {
            // Yedek adım: zemin bulunamazsa parke adımı (#101'den beri gerçek kayıt).
            adimlar = new[] { Resources.Load<AudioClip>("Sesler/Adim_1") };
        }
    }

    private void OnDestroy()
    {
        foreach (KatKapisi kapi in FindObjectsByType<KatKapisi>()) kapi.AcilisBasladi -= KapiAcildi;
        if (golge != null && golge != HazirGolge) Destroy(golge);   // hazır Gölge sahnenin malı, yok edilmez
    }

    private void Update()
    {
        if (oyuncu == null || durum == Durum.Bitti) return;

        if (durum == Durum.Bekliyor)
        {
            // Tetik: katın anahtarı ele geçti.
            if (Cep.Tasiyor(EsyaTuru.KatAnahtari, kat)) Baslat();
            return;
        }

        // #112: not, şifre, inceleme ya da fotoğraf ekranı açıkken (kontrolcü kapalı) ve menüde kovalamaca DONAR:
        // siluet yürümez, süre işlemez. Eskiden oyuncu ekran açıkken yakalanıyordu. Kontrolcü yalnızca kaçış
        // bittikten sonraki karartmada da kapalıdır; o zaman durum Bitti olduğundan buraya gelinmez.
        if (Time.timeScale <= 0f || (kontrol != null && !kontrol.enabled)) return;
        gecenSure += Time.deltaTime;
        // #139: bu blok (#112'de) iki kez yazılmıştı; süre her kare iki kat sayılıyor, siluet 3 sn yerine 1,5 sn
        // sonra beliriyordu. Tek kez sayılır.

        // İz kaydı: oyuncunun yürüdüğü yol.
        izSayaci += Time.deltaTime;
        if (izSayaci >= IzAraligi) { izSayaci = 0f; izler.Add(oyuncu.position); }

        // Bekleme bitince siluet anahtarın alındığı yerde (ilk iz) belirir ve izleri takip eder.
        if (golge == null)
        {
            if (gecenSure < gecikme) return;
            if (HazirGolge != null) { golge = HazirGolge; golge.SetActive(true); }   // oturan Gölge kalkar, olduğu yerden
            else { golge = Instantiate(VarlikGorunusu.Sablon(sablon), izler[0], Quaternion.identity); golge.name = "Kacis_Golgesi"; }   // #140: figür torbası (VarlikGorunusu)
            // Siluetin çevresindeki uğultu #74'te kalktı; gelişini üst kattan adımlar haber verir.
            hedefIz = 1;
            Debug.Log("[KACIS] Siluet belirdi.");
        }
        Ilerle();
    }

    private void Baslat()
    {
        durum = Durum.Basladi;
        gecenSure = 0f;
        izler.Clear();
        izler.Add(oyuncu.position);
        KafaLambasi lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Titret(2.2f);
        GolgeImzasi.Cal(izler[0] + Vector3.up * 1.2f);   // imza (#105): siluetin belireceği yerden
        if (baslangicSesi != null) AudioSource.PlayClipAtPoint(baslangicSesi, oyuncu.position + Vector3.up, 0.9f);
        // Üst kattan ağır adımlar (#74, Şahan: "canavar gelmeden önce çalalım, sesin ortasında
        // canavar belirsin"): ses ~5,4 sn, siluet 3 sn sonra, yani sesin ortasında belirir.
        if (haberciSesi != null) AudioSource.PlayClipAtPoint(haberciSesi, oyuncu.position + Vector3.up * 3f, 0.8f);
        ArkaMuzik.Ornek?.Kabar();
        Debug.Log($"[KACIS] {kat}. kat: kaçış başladı ({gecikme:0.0} sn sonra siluet, {hiz:0.0} m/sn).");
    }

    // Siluet izleri sırayla takip eder; oyuncuya yaklaşınca yakalar.
    private void Ilerle()
    {
        float kalan = hiz * Time.deltaTime;
        while (kalan > 0f && hedefIz < izler.Count)
        {
            Vector3 hedef = izler[hedefIz];
            Vector3 fark = hedef - golge.transform.position;
            float mesafe = fark.magnitude;
            if (mesafe <= kalan) { golge.transform.position = hedef; kalan -= mesafe; hedefIz++; }
            else { golge.transform.position += fark / mesafe * kalan; kalan = 0f; }
            Vector3 yatay = new Vector3(fark.x, 0f, fark.z);
            if (yatay.sqrMagnitude > 0.0001f) golge.transform.rotation = Quaternion.LookRotation(yatay);
        }
        // Son ize ulaştıysa doğrudan oyuncuya yürür (oyuncu durduysa).
        if (hedefIz >= izler.Count)
        {
            Vector3 fark = oyuncu.position - golge.transform.position;
            if (fark.magnitude > 0.01f) golge.transform.position += fark.normalized * Mathf.Min(fark.magnitude, kalan);
        }
        // Ağır adımlar: oyuncunun adım sesleri, kalın (perdesi düşük).
        adimSayaci += Time.deltaTime;
        if (adimSayaci >= AdimAraligi && adimlar != null)
        {
            adimSayaci = 0f;
            // Bastığı zeminin adımı (#99, ZeminSesi).
            AudioClip ad = ZeminSesi.AdimKlibi(golge.transform.position, golge.transform);
            if (ad == null) ad = adimlar[Random.Range(0, adimlar.Length)];
            if (ad != null) AdimCal(ad);
        }
        Vector3 aradaki = oyuncu.position - golge.transform.position;
        aradaki.y = 0f;
        if (aradaki.magnitude < YakalamaMesafesi && Mathf.Abs(oyuncu.position.y - golge.transform.position.y) < 1.2f)
            Yakalandi();
    }

    private void AdimCal(AudioClip klip)
    {
        GameObject g = new GameObject("Kacis_Adim");
        g.transform.position = golge.transform.position;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip; a.spatialBlend = 1f; a.pitch = 0.62f; a.volume = 1f; a.maxDistance = 16f;
        a.Play();
        Destroy(g, klip.length / a.pitch + 0.1f);
    }

    // Yakalandı: Gölge durur; ekranı karartıp katı baştan kurmak kat yöneticisinin işi.
    // "kacis" kayda YAZILMAZ: kat yeniden oynanırken kaçış anı yine olmalı.
    private void Yakalandi()
    {
        durum = Durum.Bitti;
        Titresim.Vur(1f, 0.8f, 0.9f);   // #124
        Debug.Log($"[KACIS] {kat}. kat: yakalandı; kat baştan başlıyor.");
        if (YakalanmaIsleyici != null) { YakalanmaIsleyici(kat); return; }   // dokuzuncu kat kendi başa dönüşünü yapar
        KatYoneticisi y = FindAnyObjectByType<KatYoneticisi>();
        if (y != null) y.KacistaYakalandi(kat);
    }

    /// <summary>Dışarıdan bildirilen kaçış (#108: kabin kapıları kapandı). Kayda yazılmaz.</summary>
    public void Kacti()
    {
        // Bekliyor'ken de biter: oyuncu anahtarı alıp aynı anda kabine girmişse kaçış hiç başlamaz.
        if (durum == Durum.Bitti) return;
        durum = Durum.Bitti;
        if (golge != null && golge != HazirGolge) Destroy(golge);
        Debug.Log($"[KACIS] {kat}. kat: kaçtı (kabin kapıları kapandı).");
    }

    /// <summary>Bölüm başa dönerken: yine anahtarın alınmasını bekler; iz ve Gölge sıfırlanır.</summary>
    public void YenidenBekle()
    {
        if (golge != null && golge != HazirGolge) Destroy(golge);
        golge = null;
        izler.Clear();
        hedefIz = 0;
        durum = Durum.Bekliyor;
    }

    // Kaçtı: anahtar katın kapısına takıldı (kanadın açılması beklenmez).
    private void KapiAcildi(int acilanKat)
    {
        if (acilanKat != kat || durum != Durum.Basladi) return;
        durum = Durum.Bitti;
        Kayit.KilitCozuldu(kat, "kacis");
        if (golge != null) Destroy(golge);
        Debug.Log($"[KACIS] {kat}. kat: kaçtı (kat kapısı açıldı).");
    }

    /// <summary>Kaçış daha önce yaşandı (kayıt): bileşen hiçbir şey yapmaz.</summary>
    public void BittiBaslat() { durum = Durum.Bitti; }

    public bool Basladi { get { return durum == Durum.Basladi; } }
}
