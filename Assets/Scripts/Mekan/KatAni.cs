// KatAni.cs
// Her katın imza korku anı (#96): bir kez olan, senaryolu küçük bir olay. Sallanan sandalye,
// arkadan çarpan kapı, düşen koli, kendiliğinden açılan tepegöz, bakmayınca dönen manken,
// bir anlığına yanan lambanın altında duran siluet, beklenmedik bir ses.
//
// ---------- NEDEN VERİ ----------
// Anomali kartlarında olduğu gibi (CLAUDE.md: "içerik veridir") her katın anı kat dosyasında
// (KatZinciriVarligi.an) yazılı bir tariftir: türü, hangi nesne, ne zaman tetiklenir, hangi ses.
// Bu bileşen tarifi okuyup oynatır; yeni bir an eklemek yeni kod değil, yeni veri demektir
// (yeni bir TÜR gerekirse KatAniTuru listesinin SONUNA eklenir: dosyalar türü sayı olarak saklar).
//
// ---------- NE ZAMAN OLUR ----------
// Tarifte doldurulan her koşul sağlanınca:
//   TetikOda  : oyuncu o odadaysa (CalanEsya gibi DaireEksenleri + DairePlani.NoktaninOdasi ile).
//   TetikEsya : o zincir eşyası alındıktan sonra (eldeyse ya da alet çantasına girdiyse).
//   TetikKilit: o kilit çözüldükten sonra (Kayit.KilitCozukMu).
// Koşullar tutunca Gecikme saniye sayılır; sonra türün kendi ön şartı beklenir (çarpacak kapı açık
// olmalı, siluetin duracağı yer oyuncunun ekranında olmalı). Oyunu durduran her şey sayacı da durdurur:
// test botu (Varlik.Kapali), başlık ekranı, not/şifre/menü (kontrolcü kapalı).
//
// ---------- BİR KEZ ----------
// An başlarken kayda "an" kilidi olarak yazılır (Kayit.KilitCozuldu). Oyuncu yakalanıp katın başında
// uyanınca (kilitler kalır) an yeniden oynamaz; katı baştan yüklemek (Kayit.KatBasinaDon) kat kilitlerini
// sildiği için anı da geri getirir: kat ilk kez giriliyormuş gibi.
//
// Bileşeni ZincirUygulayici.DekorUygula dairenin köküne ekler, DaireyiTemizle hemen (DestroyImmediate) söker.

using System.Collections;
using UnityEngine;

/// <summary>
/// Anın türü. YALNIZCA SONUNA EKLENİR: kat dosyaları türü sırasındaki sayıyla saklar; araya bir tür
/// girerse kayıtlı bütün anlar bir sonrakine kayar.
/// </summary>
public enum KatAniTuru
{
    Sallanma,        // nesne (sallanan sandalye, beşik) kendiliğinden ileri geri sallanır
    KapiCarpma,      // odanın kapısı oyuncunun arkasından hızla kapanır
    Devrilme,        // nesne (üstteki koli) devrilip yere düşer
    EkranAc,         // ekranlı/ışıklı bir cihaz (tepegöz) cızırdayarak açılır, sonra söner
    BakmayincaDon,   // nesne (manken) oyuncu her bakmadığında yüzünü ona çevirir
    IsikSiluet,      // bir oda lambası bir an yanar: altında siluet; sönünce yok
    Ses,             // bir çapadan tek seferlik bir ses
}

/// <summary>
/// Bir katın anının tarifi (kat dosyasında durur). Alanların hepsi her türde kullanılmaz; hangisinin
/// neye yaradığı yanında yazılı.
/// </summary>
[System.Serializable]
public class KatAniVerisi
{
    public KatAniTuru Tur;
    // Nesnenin dairedeki yolu (KatSusu/KatYerlesimi kuralı): "Sallanan_Sandalye_Nuri" (adı verilmiş süs),
    // "Kapilar/Kapi_Oda_A" (oda kapısı), "Lambalar/Lamba_Hol" (oda lambası), "B:..." ikinci daire.
    public string Yol = "";
    public string TetikOda = "";       // boş: oda şartı yok. "B:Hol" ikinci dairenin holü.
    public string TetikEsya = "";      // boş: eşya şartı yok. Zincir eşyasının kimliği ("kat_anahtari").
    public string TetikKilit = "";     // boş: kilit şartı yok. Zincir kilidinin kimliği ("sifon").
    public float Gecikme = 1.5f;       // şartlar tuttuktan kaç saniye sonra
    public AudioClip Klip;             // anın sesi (gıcırtı, gümbürtü, cızırtı...)
    [Range(0f, 1f)] public float Ses = 0.8f;
    public int Sayi = 6;               // Sallanma: salınım sayısı; BakmayincaDon: dönüş sayısı
    public float Genlik = 7f;          // Sallanma: en büyük açı; Devrilme: devrilme açısı (derece)
    public float Sure = 1.5f;          // Sallanma: bir salınımın süresi; Devrilme/KapiCarpma: hareket süresi;
                                       // EkranAc/IsikSiluet: açık kalma süresi; Ses: >0 ise bu kadar sonra kısılır
    public bool EksenZ;                // Sallanma/Devrilme: yerel Z ekseninde döner (işaretsiz: yerel X, öne-arkaya)
    public Vector3 Pivot;              // Sallanma/Devrilme: dönme noktası; EkranAc: ışığın yeri (nesnenin yerel ekseninde)
    public Vector3 Kayma;              // Devrilme: düşüşün sonunda nesnenin kayması (yerel eksende, aşağı ve ileri)
    public float U, V;                 // plan noktası (DairePlani): IsikSiluet siluetin yeri, EkranAc ışığın hedefi
    public float Yukseklik = 1.2f;     // EkranAc: ışığın hedefinin yüksekliği
    public float Siddet = 3f;          // EkranAc/IsikSiluet: ışığın şiddeti (oda lambası 3)
}

public class KatAni : MonoBehaviour
{
    private const float GormeMesafesi = 9f;       // BakmayincaDon/IsikSiluet: bundan uzaktaki "görmek" sayılmaz
    private const float SiluetYakinSiniri = 1.6f; // siluet oyuncunun burnunun dibinde belirmesin

    private int kat;
    private KatAniVerisi veri;
    private Transform hedef;              // anın nesnesi (sandalye, kapı, lamba, manken...)
    private DaireEksenleri odaEksen;      // TetikOda'nın dairesinin eksenleri
    private string oda;                   // TetikOda, "B:" öneki atılmış
    private DaireEksenleri planEksen;     // U, V plan noktasının dairesinin eksenleri
    private GameObject golgeSablonu;      // IsikSiluet'in silueti (Kacis_Golgesi)

    private enum Durum { Bekliyor, Sayiyor, Oynuyor, Bitti }
    private Durum durum = Durum.Bekliyor;
    private float sayac;

    // Eşya ve kilit şartı bir kez tutunca öyle kalır ("mandal"): eşya elden bırakılsa da an gelir.
    private bool esyaTuttu, kilitTuttu;
    private bool esyaIlkBakis = true;     // ilk karede eşya zaten eldeyse bu bir ALMA değildir

    private BirinciSahisKontrolcu kontrol;
    private Transform oyuncu;
    private Camera kamera;

    // Hedefin başlangıç duruşu: an bitince ya da bileşen sökülünce (kat değişirken) geri konur.
    // Süsler zaten yeniden kurulur ama dairenin kalıcı bir parçası (Esyalar/...) hedef olursa
    // başka bir katta devrik kalmasın.
    private Vector3 tabanKonum;
    private Quaternion tabanDonus;
    private bool oynatildi;

    // Ana özgü geçici şeyler: söküldüğünde temizlenir.
    private GameObject golge, isikNesnesi, sesNesnesi;
    private Light lambaSpot, lambaDolgu;
    private float lambaSpotEski, lambaDolguEski;
    private bool lambaYanikTutuluyor;
    private IcKapi carpanKapi;
    private bool kapiCarpiyor;

    /// <summary>Anı kurar (ZincirUygulayici.DekorUygula). hedef null olabilir (yol boş ya da bulunamadı).</summary>
    public void Hazirla(int katNo, KatAniVerisi tarif, Transform nesne, DaireEksenleri odaEksenleri, string odaAdi,
                        DaireEksenleri planEksenleri, GameObject siluet)
    {
        kat = katNo;
        veri = tarif;
        hedef = nesne;
        odaEksen = odaEksenleri;
        oda = odaAdi ?? "";
        planEksen = planEksenleri;
        golgeSablonu = siluet;
        if (hedef != null) { tabanKonum = hedef.localPosition; tabanDonus = hedef.localRotation; }
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrol != null)
        {
            oyuncu = kontrol.transform;
            kamera = kontrol.GetComponentInChildren<Camera>();
        }
    }

    /// <summary>Kayıttan: bu katın anı daha önce yaşandı (bir daha oynamaz).</summary>
    public void BittiBaslat() { durum = Durum.Bitti; }

    // Anın sayacı bu karede işlesin mi? (Varlık'ın kapılarıyla aynı mantık.)
    private bool Etkin()
    {
        if (Varlik.Kapali) return false;                       // oyun botu, kat denetimi
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        if (kontrol == null || !kontrol.enabled) return false; // menü, not, şifre ekranı, karartma
        return Time.timeScale > 0f;
    }

    private void Update()
    {
        if (veri == null || oyuncu == null) return;
        if (durum == Durum.Bitti || durum == Durum.Oynuyor) return;
        if (!Etkin()) return;

        if (durum == Durum.Bekliyor)
        {
            if (!SartlarTuttu()) return;
            durum = Durum.Sayiyor;
            sayac = 0f;
        }

        // Durum.Sayiyor: gecikme bitince türün ön şartı beklenir, sonra an başlar.
        sayac += Time.deltaTime;
        if (sayac < veri.Gecikme || !OnSartTuttu()) return;
        Baslat();
    }

    // ---------- ŞARTLAR ----------
    private bool SartlarTuttu()
    {
        // Eşya: "alındı" anı, eldeki eşyanın ya da alet çantasının değişiminden anlaşılır.
        if (!string.IsNullOrEmpty(veri.TetikEsya) && !esyaTuttu)
        {
            bool elde = Cep.KimligiTasiyor(veri.TetikEsya) || Aletler.Var(veri.TetikEsya);
            if (esyaIlkBakis) { esyaIlkBakis = false; if (elde) return false; }   // zaten eldeydi: yeni alma değil
            if (elde) esyaTuttu = true;
        }
        if (!string.IsNullOrEmpty(veri.TetikKilit) && !kilitTuttu && Kayit.KilitCozukMu(kat, veri.TetikKilit)) kilitTuttu = true;

        if (!string.IsNullOrEmpty(veri.TetikEsya) && !esyaTuttu) return false;
        if (!string.IsNullOrEmpty(veri.TetikKilit) && !kilitTuttu) return false;
        if (!string.IsNullOrEmpty(oda) && OyuncununOdasi() != oda) return false;
        return true;
    }

    // Oyuncu tetik odasının dairesinde hangi odada (yoksa null)? Yükseklik de bakılır: merdivende
    // ya da başka bir kattayken plan noktası tesadüfen bir odaya düşmesin.
    private string OyuncununOdasi()
    {
        if (odaEksen == null) return null;
        Vector3 yerel = odaEksen.transform.InverseTransformPoint(oyuncu.position);
        if (yerel.y < -0.6f || yerel.y > 2.0f) return null;
        odaEksen.PlanNoktasi(oyuncu.position, out float u, out float v);
        return DairePlani.NoktaninOdasi(u, v);
    }

    // Türün kendi ön şartı: an ancak görülebilecek / olabilecek hâldeyken başlar.
    private bool OnSartTuttu()
    {
        switch (veri.Tur)
        {
            case KatAniTuru.KapiCarpma:
                // Kapı açık olmalı ve oyuncu kanadın süpüreceği yerden uzak (kapı oyuncunun üstüne kapanmaz).
                if (hedef == null) return false;
                IcKapi k = hedef.GetComponent<IcKapi>();
                Transform m = hedef.Find("Mentese");
                if (k == null || m == null || k.Kilitli || k.Devrik) return false;
                if (Mathf.Abs(Mathf.DeltaAngle(0f, m.localEulerAngles.y)) < 20f) return false;
                // Oyuncu hâlâ odanın içinde olmalı: kapı ARKASINDAN çarpsın, önünde değil.
                if (!string.IsNullOrEmpty(oda) && OyuncununOdasi() != oda) return false;
                Vector3 ara = oyuncu.position - m.position; ara.y = 0f;
                return ara.magnitude > 1.5f;
            case KatAniTuru.IsikSiluet:
                // Siluetin duracağı yer oyuncunun ekranında, arada duvar yok, ne çok yakın ne çok uzak.
                if (planEksen == null || golgeSablonu == null) return false;
                Vector3 yer = SiluetYeri();
                float uzak = Vector3.Distance(new Vector3(yer.x, oyuncu.position.y, yer.z), oyuncu.position);
                return uzak >= SiluetYakinSiniri && uzak <= GormeMesafesi && Gorunur(yer + Vector3.up * 1.2f, null);
            default:
                return true;
        }
    }

    // ---------- BAŞLAT ----------
    private void Baslat()
    {
        durum = Durum.Oynuyor;
        oynatildi = true;
        Kayit.KilitCozuldu(kat, "an");   // bir daha oynamasın (yakalanıp katın başında uyanınca da)
        Debug.Log($"[AN] {kat}. kat: {veri.Tur} başladı ('{veri.Yol}').");
        IEnumerator akis;
        switch (veri.Tur)
        {
            case KatAniTuru.Sallanma: akis = Sallan(); break;
            case KatAniTuru.KapiCarpma: akis = KapiyiCarp(); break;
            case KatAniTuru.Devrilme: akis = Devril(); break;
            case KatAniTuru.EkranAc: akis = EkraniAc(); break;
            case KatAniTuru.BakmayincaDon: akis = BakmayincaDon(); break;
            case KatAniTuru.IsikSiluet: akis = IsikSiluet(); break;
            default: akis = SesCal(); break;
        }
        StartCoroutine(Sar(akis));
    }

    // Türün akışını oynatır, bitince anı "bitti" yapar.
    private IEnumerator Sar(IEnumerator akis)
    {
        yield return StartCoroutine(akis);
        durum = Durum.Bitti;
        Debug.Log($"[AN] {kat}. kat: an bitti.");
    }

    // Korkunun kendisi oldu: bina da duyar (Varlık'ın gerilimi biraz artar), müzik kabarır.
    private static void Korkut()
    {
        Varlik.Gurultu(0.12f);
        ArkaMuzik.Ornek?.Kabar();
    }

    // ---------- SALLANMA ----------
    // Açı = Genlik × sin(2π t / Sure) × (1 − t / toplam): sinüs ileri geri götürür, sondaki çarpan
    // salınımı yavaşça söndürür (kimse itmiyor, sürtünme durduruyor). Her uç noktada (salınımın
    // durup döndüğü an) tahta bir kez gıcırdar; ses de salınımla birlikte kısılır.
    private IEnumerator Sallan()
    {
        if (hedef == null) yield break;
        Korkut();
        float donem = Mathf.Max(0.3f, veri.Sure);
        float toplam = Mathf.Max(1, veri.Sayi) * donem;
        float sonrakiGicirti = donem / 4f;           // ilk uç nokta çeyrek salınımda
        for (float t = 0f; t < toplam; t += Time.deltaTime)
        {
            float sonum = 1f - t / toplam;
            Durus(veri.Genlik * Mathf.Sin(2f * Mathf.PI * t / donem) * sonum, Vector3.zero);
            if (t >= sonrakiGicirti)
            {
                Cal(veri.Klip, hedef.position + Vector3.up * 0.4f, veri.Ses * (0.35f + 0.65f * sonum), Random.Range(0.92f, 1.08f));
                sonrakiGicirti += donem / 2f;            // bir sonraki uç yarım salınım sonra
            }
            yield return null;
        }
        Durus(0f, Vector3.zero);
    }

    // ---------- DEVRİLME ----------
    // Nesne Pivot'taki kenarı üstünden döner (t² ile: düşen bir şey gibi hızlanarak), aynı anda
    // Kayma kadar aşağı ve ileri iner. Yere değdiği an gümbürtü; sonra hafifçe sekip oturur.
    private IEnumerator Devril()
    {
        if (hedef == null) yield break;
        float sure = Mathf.Max(0.15f, veri.Sure);
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            float e = t / sure;
            Durus(veri.Genlik * e * e, veri.Kayma * (e * e * e));
            yield return null;
        }
        Durus(veri.Genlik, veri.Kayma);
        Cal(veri.Klip, hedef.position, veri.Ses, 1f);
        Korkut();
        // Seken son: 4° geri kalkar, düşer.
        for (float t = 0f; t < 0.16f; t += Time.deltaTime)
        {
            Durus(veri.Genlik - 4f * Mathf.Sin(Mathf.PI * t / 0.16f), veri.Kayma);
            yield return null;
        }
        Durus(veri.Genlik, veri.Kayma);
        // Devrilen nesne devrik kalır (an bitti diye ayağa kalkmaz); kat değişince sökülürken eski hâline döner.
    }

    // Hedefi taban duruşundan "aci" derece döndürülmüş ve "kayma" kadar kaydırılmış konuma koyar.
    // Dönme noktası Pivot: Pivot'taki nokta yerinde kalır, nesne onun çevresinde döner. Formül:
    //   yerel dönüş = taban × q
    //   yerel konum = taban konum + taban dönüş × (Pivot − q × Pivot + kayma)
    // (q nesnenin kendi ekseninde bir dönüş; Pivot − q × Pivot, pivotu yerinde tutan düzeltme.)
    private void Durus(float aci, Vector3 kayma)
    {
        Vector3 eksen = veri.EksenZ ? Vector3.forward : Vector3.right;
        Quaternion q = Quaternion.AngleAxis(aci, eksen);
        hedef.localRotation = tabanDonus * q;
        hedef.localPosition = tabanKonum + tabanDonus * (veri.Pivot - q * veri.Pivot + kayma);
    }

    // ---------- KAPI ÇARPMA ----------
    // IcKapi'nin kendi açılıp kapanması yavaş ve sessizdir (elle itilen kapı). Burada kanadı biz
    // döndürüyoruz: Sure saniyede, t² ile hızlanarak (biri hızla itmiş gibi), sonunda çarpma sesi.
    // Bitince IcKapi.Ayarla(false): kapının kendi "açık mı" bilgisi de kapalıya döner.
    private IEnumerator KapiyiCarp()
    {
        carpanKapi = hedef.GetComponent<IcKapi>();
        Transform mentese = hedef.Find("Mentese");
        float bas = Mathf.DeltaAngle(0f, mentese.localEulerAngles.y);   // -180..180 arası şimdiki açı
        float sure = Mathf.Max(0.08f, veri.Sure);
        kapiCarpiyor = true;
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            float e = t / sure;
            mentese.localRotation = Quaternion.Euler(0f, Mathf.Lerp(bas, 0f, e * e), 0f);
            yield return null;
        }
        carpanKapi.Ayarla(false);
        kapiCarpiyor = false;
        Cal(veri.Klip, mentese.position + Vector3.up * 1.0f, veri.Ses, 1f);
        KafaLambasi lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Titret(0.5f);
        Korkut();
    }

    // ---------- EKRAN AÇ ----------
    // Cihazın (tepegöz) önünde bir spot ışık yanar ve plan noktasına (kara tahta) bakar; Sure boyunca
    // eski bir lamba gibi titrer ve arada bir tamamen söner, cızırtı sesi çalar; sonra bir anda kesilir.
    // Işığın rengi soğuk beyaz (halojen); gölgeli tek bir spot (CLAUDE.md: noktasal ışığa gölge verme).
    private IEnumerator EkraniAc()
    {
        if (hedef == null) yield break;
        isikNesnesi = new GameObject("An_Ekran_Isigi");
        isikNesnesi.transform.SetParent(hedef, false);
        isikNesnesi.transform.localPosition = veri.Pivot;
        Vector3 hedefNokta = planEksen != null
            ? planEksen.transform.TransformPoint(planEksen.Nokta(veri.U, veri.Yukseklik, veri.V))
            : isikNesnesi.transform.position + hedef.forward;
        Vector3 yon = hedefNokta - isikNesnesi.transform.position;
        if (yon.sqrMagnitude > 0.0001f) isikNesnesi.transform.rotation = Quaternion.LookRotation(yon);
        Light isik = isikNesnesi.AddComponent<Light>();
        isik.type = LightType.Spot;
        isik.color = new Color(0.86f, 0.92f, 1f);
        isik.spotAngle = 34f;
        isik.innerSpotAngle = 22f;
        isik.range = yon.magnitude + 2.5f;
        isik.shadows = LightShadows.Soft;
        isik.intensity = 0f;

        AudioSource kaynak = Kaynak(isikNesnesi, veri.Klip, veri.Ses);
        if (kaynak != null) kaynak.Play();
        Korkut();
        float sure = Mathf.Max(0.5f, veri.Sure);
        float blok = 0f, deger = 1f;
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            // Her 50 ms'de bir yeni bir parlaklık: çoğu zaman %60-100, arada bir (%12) karanlık.
            blok -= Time.deltaTime;
            if (blok <= 0f) { blok = 0.05f; deger = Random.value < 0.12f ? 0f : Random.Range(0.6f, 1f); }
            // İlk 0,3 sn ısınma: lamba yavaşça gelir.
            isik.intensity = veri.Siddet * deger * Mathf.Clamp01(t / 0.3f);
            yield return null;
        }
        isik.intensity = 0f;
        if (kaynak != null) kaynak.Stop();
        yield return new WaitForSeconds(0.1f);
        Destroy(isikNesnesi);
        isikNesnesi = null;
    }

    // ---------- BAKMAYINCA DÖN ----------
    // Oyuncu nesneyi en az 0,4 sn gördükten sonra gözünü ondan ayırdığı an (ekrandan çıktı ya da arada
    // bir duvar var), nesne yüzünü oyuncuya çevirir. Sayi kez olur. Dönüşü hiçbir zaman görmez; yalnızca
    // geri baktığında yönün değiştiğini fark eder (ve dönüşün hafif sürtünme sesini duyabilir).
    private IEnumerator BakmayincaDon()
    {
        if (hedef == null) yield break;
        int donus = 0;
        float gorulen = 0f;
        while (donus < Mathf.Max(1, veri.Sayi))
        {
            if (!Etkin()) { yield return null; continue; }
            Vector3 nokta = hedef.position + Vector3.up * 1.0f;
            bool yakin = Vector3.Distance(nokta, oyuncu.position) < GormeMesafesi;
            if (yakin && Gorunur(nokta, hedef)) gorulen += Time.deltaTime;
            else if (gorulen >= 0.4f && yakin)
            {
                // Gözden çıktı: oyuncuya dön (yalnızca yatayda; manken eğilmez).
                Vector3 bakis = oyuncu.position - hedef.position; bakis.y = 0f;
                if (bakis.sqrMagnitude > 0.01f) hedef.rotation = Quaternion.LookRotation(bakis);
                Cal(veri.Klip, hedef.position, veri.Ses, Random.Range(0.85f, 1.0f));
                if (donus == 0) Korkut();
                donus++;
                gorulen = 0f;
                Debug.Log($"[AN] {kat}. kat: manken döndü ({donus}/{veri.Sayi}).");
            }
            yield return null;
        }
    }

    // ---------- IŞIKTA SİLUET ----------
    // Oda lambası bir anda yanar (kısa bir titreme: yan, sön, yan), Sure boyunca yanık kalır; ışığın
    // altında, plan noktasında siluet durur ve oyuncuya bakar. Lamba söner, karanlıkta siluet kaldırılır.
    // Lamba yanmadan önceki hâline döner (4. katta sigorta takılmadan önce sönük, sonra yanık).
    private IEnumerator IsikSiluet()
    {
        if (hedef == null || golgeSablonu == null || planEksen == null) yield break;
        lambaSpot = hedef.GetComponent<Light>();
        Transform dolgu = hedef.Find("Dolgu");
        lambaDolgu = dolgu != null ? dolgu.GetComponent<Light>() : null;
        lambaSpotEski = lambaSpot != null ? lambaSpot.intensity : 0f;
        lambaDolguEski = lambaDolgu != null ? lambaDolgu.intensity : 0f;

        Vector3 yer = SiluetYeri();
        Vector3 bakis = oyuncu.position - yer; bakis.y = 0f;
        golge = Instantiate(golgeSablonu, yer, bakis.sqrMagnitude > 0.001f ? Quaternion.LookRotation(bakis) : Quaternion.identity);
        golge.name = "An_Golgesi";

        Cal(veri.Klip, hedef.position, veri.Ses, 1f);
        lambaYanikTutuluyor = true;
        float[] adimlar = { 0.06f, 0.07f, 0.05f };      // yan, sön, yan (titreyerek gelen elektrik)
        for (int i = 0; i < adimlar.Length; i++)
        {
            LambaYak(i % 2 == 0);
            yield return new WaitForSeconds(adimlar[i]);
        }
        LambaYak(true);
        Korkut();
        yield return new WaitForSeconds(Mathf.Max(0.2f, veri.Sure));
        LambaYak(false);
        yield return new WaitForSeconds(0.05f);
        LambaGeriAl();
        // Karanlıkta yok olur: oyuncu ışık dönünce boşluğa bakar.
        if (golge != null) Destroy(golge);
        golge = null;
        KafaLambasi kafa = FindAnyObjectByType<KafaLambasi>();
        if (kafa != null) kafa.Titret(0.4f);
    }

    // Siluetin yeri: plan noktası, dairenin zemininde (dünya konumu).
    private Vector3 SiluetYeri()
    {
        return planEksen.transform.TransformPoint(planEksen.Nokta(veri.U, 0f, veri.V));
    }

    private void LambaYak(bool yan)
    {
        if (lambaSpot != null) lambaSpot.intensity = yan ? veri.Siddet : 0f;
        if (lambaDolgu != null) lambaDolgu.intensity = yan ? veri.Siddet * 0.3f : 0f;   // dolgu spotun %30'u (DaireLambalari)
    }

    private void LambaGeriAl()
    {
        if (!lambaYanikTutuluyor) return;
        lambaYanikTutuluyor = false;
        if (lambaSpot != null) lambaSpot.intensity = lambaSpotEski;
        if (lambaDolgu != null) lambaDolgu.intensity = lambaDolguEski;
    }

    // ---------- SES ----------
    // Çapanın yerinden tek seferlik bir ses. Sure > 0 ise o kadar sonra yarım saniyede kısılır.
    private IEnumerator SesCal()
    {
        if (hedef == null || veri.Klip == null) yield break;
        sesNesnesi = new GameObject("An_Sesi");
        sesNesnesi.transform.SetParent(hedef, false);
        AudioSource kaynak = Kaynak(sesNesnesi, veri.Klip, veri.Ses);
        kaynak.Play();
        Korkut();
        float sure = veri.Sure > 0f ? Mathf.Min(veri.Sure, veri.Klip.length) : veri.Klip.length;
        yield return new WaitForSeconds(sure);
        for (float t = 0f; t < 0.5f && kaynak.isPlaying; t += Time.deltaTime)
        {
            kaynak.volume = veri.Ses * (1f - t / 0.5f);
            yield return null;
        }
        Destroy(sesNesnesi);
        sesNesnesi = null;
    }

    // ---------- YARDIMCILAR ----------

    // Nokta oyuncunun ekranında mı ve kameradan noktaya açık bir çizgi var mı? (oyuncunun kendi kapsülü
    // ve "haric" nesnesi engel sayılmaz: kamera kapsülün içinde, manken kendi çarpıştırıcısıyla kendini örtmesin.)
    private bool Gorunur(Vector3 nokta, Transform haric)
    {
        if (kamera == null) return false;
        Vector3 e = kamera.WorldToViewportPoint(nokta);
        if (e.z <= 0f || e.x < 0.05f || e.x > 0.95f || e.y < 0.05f || e.y > 0.95f) return false;
        Vector3 goz = kamera.transform.position;
        Vector3 fark = nokta - goz;
        foreach (RaycastHit h in Physics.RaycastAll(goz, fark, fark.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (t.IsChildOf(oyuncu) || (haric != null && t.IsChildOf(haric))) continue;
            return false;
        }
        return true;
    }

    // Verilen yerde kısa ömürlü 3B bir ses (AudioSource.PlayClipAtPoint gibi, ama perdesi ayarlanabilir).
    private static void Cal(AudioClip klip, Vector3 yer, float ses, float perde)
    {
        if (klip == null) return;
        GameObject g = new GameObject("An_Tek_Ses");
        g.transform.position = yer;
        AudioSource a = Kaynak(g, klip, ses);
        a.pitch = perde;
        a.Play();
        Destroy(g, klip.length / Mathf.Max(0.1f, perde) + 0.1f);
    }

    // Anların ortak ses ayarı: tamamen 3B, gerçekçi sönüm, dairenin öbür ucundan da duyulur.
    private static AudioSource Kaynak(GameObject g, AudioClip klip, float ses)
    {
        if (klip == null) return null;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip;
        a.playOnAwake = false;
        a.spatialBlend = 1f;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.minDistance = 1.2f;
        a.maxDistance = 18f;
        a.dopplerLevel = 0f;
        a.volume = ses;
        return a;
    }

    // Bileşen sökülürken (kat değişiyor, daire yeniden kuruluyor): yarım kalan her şey toparlanır.
    // DaireyiTemizle bunu lambaları söndürmeden ÖNCE çağırır; lambayı burada eski hâline almak oradaki
    // "hepsi sönük" kararını bozmaz.
    private void OnDestroy()
    {
        StopAllCoroutines();
        LambaGeriAl();
        if (golge != null) Destroy(golge);
        if (isikNesnesi != null) Destroy(isikNesnesi);
        if (sesNesnesi != null) Destroy(sesNesnesi);
        // Yarıda kalan kapı çarpması: kapının kendi bilgisi kanatla aynı olsun.
        if (kapiCarpiyor && carpanKapi != null) carpanKapi.Ayarla(false);
        // Hedef eski duruşuna (kapı ve lamba kendi bileşenlerince yönetilir, onlara dokunulmaz).
        if (oynatildi && hedef != null && (veri.Tur == KatAniTuru.Sallanma || veri.Tur == KatAniTuru.Devrilme || veri.Tur == KatAniTuru.BakmayincaDon))
        {
            hedef.localPosition = tabanKonum;
            hedef.localRotation = tabanDonus;
        }
    }
}
