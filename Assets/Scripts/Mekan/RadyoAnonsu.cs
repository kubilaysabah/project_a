// RadyoAnonsu.cs
// Her katta radyodan ya da televizyondan gelen tek bir haber cümlesi (#106).
//
// NEDEN
// Hikâye yalnızca okuyarak geliyordu (notlar, fotoğraf arkaları). Korku oyunları hikâyeyi sesle de
// anlatır. Her dairede bir ses cihazı var (radyo, televizyon, pikap); oyuncu daireye girdikten bir
// süre sonra cihaz cızırtıyla açılır, 1974'teki olaya dolaylı değinen BİR cümle söyler ve cızırtıya
// döner. Cümleler görevi anlatmaz (#83: ipucu yasağı); hepsi aynı olaya başka bir açıdan değinir.
//
// NASIL
// Kat dosyası (KatZinciriVarligi.anons) cihazın yolunu, gecikmeyi ve ses dosyasının adını verir;
// ZincirUygulayici bu bileşeni cihaza ekler, kat değişince söker. Oyuncu dairenin içindeyken süre
// işler (CalanEsya ile aynı yol: plan eksenlerinde hangi odada olduğu). Süre dolunca:
//   düğme tıkı → parazit kabarır (0,8 sn) → seslendirme (varsa) → parazit (0,6 sn) → söner.
// Seslendirme dosyası Resources/Sesler/Konusmalar/<Dosya> adıyla aranır (telefon kayıtlarıyla aynı
// klasör, Şahan'ın okumaları kayit_isle.py radyo_anons ile işlenir). Dosya henüz yoksa yalnızca
// parazit duyulur: cihaz "canlanır", bir şey söylemez. Her daire açılışında bir kez.

using System.Collections;
using UnityEngine;

public class RadyoAnonsu : MonoBehaviour
{
    private const float ParazitGiris = 0.8f, ParazitCikis = 0.6f;
    private const float ParazitSes = 0.35f;   // seslendirmeye göre parazitin gücü
    private const float AltyaziMenzili = 9f;  // oyuncu cihaza bu kadar yakınsa cümle alt yazı olur (#118)
    // Cümlelerin metin anahtarları (#118): "anons." + dosya adı. Liste düz yazılmıştır ki metin bütünlük
    // testi anahtarları kodda bulsun (2. katın cümlesi oyunda çalmaz ama tabloda durur).
    private static readonly string[] AltyaziAnahtarlari = {
        "anons.radyo_kat1", "anons.radyo_kat2", "anons.radyo_kat3", "anons.radyo_kat4",
        "anons.radyo_kat5", "anons.radyo_kat6", "anons.radyo_kat7", "anons.radyo_kat8",
    };

    private float gecikme, ses;
    private string dosya;
    private DaireEksenleri eksen;
    private Transform oyuncu;
    private float iceridekiSure;
    private bool oynadi;
    private AudioSource kaynak;
    private AudioSource sesKaynagi;   // seslendirme (yayın sırasında; Sifirla da siler, #119)
    private Coroutine akis;
    private string gosterilenAltyazi;  // bu cihazın açtığı alt yazı (Sifirla kapatır)

    /// <summary>Kat dosyasından kurulur. dosya: Konusmalar klasöründeki seslendirme (boş olabilir).</summary>
    public void Hazirla(float gecikmeSn, string dosyaAdi, float sesDuzeyi, DaireEksenleri eksenler)
    {
        gecikme = gecikmeSn;
        dosya = dosyaAdi ?? "";
        ses = sesDuzeyi;
        eksen = eksenler;
        BirinciSahisKontrolcu k = FindAnyObjectByType<BirinciSahisKontrolcu>();
        oyuncu = k != null ? k.transform : null;
        // 3B kaynak cihazın üstünde: oyuncu sesin yerini duyar (hangi oda?).
        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 1f;
        kaynak.minDistance = 1.2f;
        kaynak.maxDistance = 14f;
        kaynak.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>Yayın (anons) ya da kısa parazit şu an çalıyor mu (#131: cin olayı üstüne binmesin).</summary>
    public bool Yayinda { get { return akis != null || parazitAkisi != null; } }
    private Coroutine parazitAkisi;

    /// <summary>
    /// Cin olayı (#131): cihaz kendiliğinden bir anlığına açılır: düğme tıkı, 1,2 sn kabarıp sönen parazit,
    /// düğme tıkı. Anonsun kendisi çalmaz; anons sürerken ya da kaynak yokken hiçbir şey yapmaz.
    /// </summary>
    public void KisaParazit()
    {
        if (kaynak == null || Yayinda) return;
        parazitAkisi = StartCoroutine(KisaParazitAkisi());
    }

    private IEnumerator KisaParazitAkisi()
    {
        AudioClip parazit = Resources.Load<AudioClip>("Sesler/Radyo_Parazit");
        AudioClip dugme = Resources.Load<AudioClip>("Sesler/Radyo_Dugme");
        if (dugme != null) kaynak.PlayOneShot(dugme, 0.5f);
        if (parazit != null)
        {
            kaynak.clip = parazit; kaynak.loop = true; kaynak.volume = 0f; kaynak.Play();
            const float Sure = 1.2f;
            for (float t = 0f; t < Sure; t += Time.deltaTime)
            {
                // Çabuk kabarır, sonunda söner (sin yarım dalgası): "açıldı, kapandı".
                kaynak.volume = ses * ParazitSes * 1.4f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / Sure));
                yield return null;
            }
            kaynak.Stop(); kaynak.loop = false;
        }
        if (dugme != null) kaynak.PlayOneShot(dugme, 0.4f);
        parazitAkisi = null;
    }

    /// <summary>Kat değişince: yayın kesilir, kaynak hemen silinir (aynı karede yeniden kurulabilir).</summary>
    public void Sifirla()
    {
        if (akis != null) StopCoroutine(akis);
        if (parazitAkisi != null) { StopCoroutine(parazitAkisi); parazitAkisi = null; }
        if (kaynak != null) DestroyImmediate(kaynak);
        if (sesKaynagi != null) DestroyImmediate(sesKaynagi);           // #119: konuşma kat değişince susar
        if (gosterilenAltyazi != null) Altyazi.Gizle(gosterilenAltyazi);  // ... ve alt yazısı kalkar
        gosterilenAltyazi = null;
    }

    private void Update()
    {
        if (oynadi || kaynak == null || oyuncu == null || eksen == null) return;
        if (Time.timeScale <= 0f) return;
        // Oyuncu dairenin içinde mi? (Plan eksenlerinde bir odanın içindeyse evet.)
        eksen.PlanNoktasi(oyuncu.position, out float u, out float v);
        if (DairePlani.NoktaninOdasi(u, v) != null) iceridekiSure += Time.deltaTime;
        if (iceridekiSure >= gecikme)
        {
            oynadi = true;
            akis = StartCoroutine(Yayin());
        }
    }

    private IEnumerator Yayin()
    {
        AudioClip parazit = Resources.Load<AudioClip>("Sesler/Radyo_Parazit");   // kayit_isle.py radyo_parazit
        AudioClip dugme = Resources.Load<AudioClip>("Sesler/Radyo_Dugme");
        AudioClip konusma = string.IsNullOrEmpty(dosya) ? null : Resources.Load<AudioClip>("Sesler/Konusmalar/" + dosya);
        Debug.Log($"[ANONS] {name}: yayın başladı (seslendirme {(konusma != null ? dosya : "yok, yalnızca parazit")}).");
        if (dugme != null) kaynak.PlayOneShot(dugme, 0.5f);

        // 1. Parazit kabarır.
        if (parazit != null)
        {
            kaynak.clip = parazit; kaynak.loop = true; kaynak.volume = 0f; kaynak.Play();
            for (float t = 0f; t < ParazitGiris; t += Time.deltaTime)
            {
                kaynak.volume = ses * ParazitSes * (t / ParazitGiris);
                yield return null;
            }
        }
        // Alt yazı (#118): cümle ekranın altında, kayıt olsa da olmasa da. Menzil (9 m) cümle boyunca her
        // kare bakılır (#119): oyuncu uzak odadan yayının ortasında gelirse cümleyi kalan süreyle yine görür.
        // Eskiden tek sefer, yayının 0,8. saniyesinde bakılıyordu; uzaktaki oyuncu cümleyi hiç görmüyordu.
        string altyaziAnahtari = System.Array.IndexOf(AltyaziAnahtarlari, "anons." + dosya) >= 0 ? "anons." + dosya : null;
        float sure = konusma != null ? konusma.length : (altyaziAnahtari != null ? 5f : 1.6f);   // kayıt yoksa okunacak kadar parazit
        // 2. Seslendirme: parazitin üstüne, aynı kaynaktan değil ikinci bir kaynaktan (ikisi birlikte duyulur).
        if (konusma != null)
        {
            sesKaynagi = gameObject.AddComponent<AudioSource>();
            sesKaynagi.playOnAwake = false; sesKaynagi.spatialBlend = 1f; sesKaynagi.minDistance = 1.2f; sesKaynagi.maxDistance = 14f;
            sesKaynagi.rolloffMode = AudioRolloffMode.Logarithmic;
            sesKaynagi.clip = konusma; sesKaynagi.volume = ses * Ayarlar.Konusma; sesKaynagi.Play();   // #118: konuşma kanalı
            if (kaynak.isPlaying) kaynak.volume = ses * ParazitSes * 0.4f;   // konuşurken parazit geri çekilir
        }
        float bitis = Time.time + sure;
        while (Time.time < bitis)
        {
            if (gosterilenAltyazi == null && altyaziAnahtari != null && oyuncu != null
                && Vector3.Distance(oyuncu.position, transform.position) <= AltyaziMenzili)
            {
                gosterilenAltyazi = Dil.Getir(altyaziAnahtari);
                Altyazi.Goster(gosterilenAltyazi, Mathf.Max(4f, bitis - Time.time));
            }
            if (sesKaynagi != null) sesKaynagi.volume = ses * Ayarlar.Konusma;   // kaydırıcı yayın sırasında da işler (#119)
            yield return null;
        }
        if (sesKaynagi != null)
        {
            Destroy(sesKaynagi); sesKaynagi = null;
            if (kaynak.isPlaying) kaynak.volume = ses * ParazitSes;
            yield return new WaitForSeconds(0.4f);
        }
        // 3. Parazit söner, düğme tıkı.
        if (parazit != null)
        {
            float bas = kaynak.volume;
            for (float t = 0f; t < ParazitCikis; t += Time.deltaTime)
            {
                kaynak.volume = bas * (1f - t / ParazitCikis);
                yield return null;
            }
            kaynak.Stop();
        }
        if (dugme != null) kaynak.PlayOneShot(dugme, 0.4f);
        Debug.Log($"[ANONS] {name}: yayın bitti.");
        akis = null;
    }
}
