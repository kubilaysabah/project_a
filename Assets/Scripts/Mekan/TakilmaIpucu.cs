// TakilmaIpucu.cs
// Oyuncu bir katta uzun süre ilerleyemezse bina ona YAZISIZ bir ipucu verir (#98).
//
// Ne olur: oyuncu açık dairenin içinde 150 saniye boyunca hiçbir eşya bulamaz, hiçbir kilit
// çözemezse, sıradaki eşyanın durduğu yerden alçak bir parke gıcırtısı gelir ve eşya bir an parlar
// (kabı kapalıysa yalnızca ses). Sonra her 75 saniyede bir, en çok dört kez; ikinci ipucundan
// itibaren kafa lambası da kısaca titrer ("bina bir şey söylemeye çalışıyor"). Ekranda hiçbir yazı
// yoktur: #83'ün "ipucu notu yok" kuralı korunur.
//
// Zamanı ve hedefi düz C# kuralı (TakilmaKurali) belirler; bu bileşen yalnızca sahneye bakar:
//   - İLERLEME: yarım saniyede bir katın "ilerleme imzası" okunur: çözülmüş kilit sayısı + bulunmuş
//     eşya sayısı (T listesiyle aynı ölçü, EsyaListesi.BulunduMu). İmza değiştiyse oyuncu ilerlemiştir.
//     Olaylara (event) abone olmak yerine yoklama seçildi: parçalar, tabaklar, aletler, eldeki eşya
//     ayrı yerlerde tutuluyor ve hepsinin bir olayı yok; yoklama hepsini tek bir sayıyla yakalar,
//     yeni bir eşya türü eklense de kendiliğinden çalışır.
//   - UYGUNLUK: saat yalnızca oyuncu gerçekten "takılmışken" işler (Uygun()).
//   - HEDEF: kuralın aday listesinden sahnede gerçekten bulunan ilk nesne.
//
// KatYoneticisi'nin nesnesine eklenir (kurucu bağlar). Sahnede başka bir bağlantı istemez; ses
// dosyası boşsa Resources'taki parke gıcırtısı kullanılır (Resources klasörü build'e girer).

using System.Collections.Generic;
using UnityEngine;

public class TakilmaIpucu : MonoBehaviour
{
    [Tooltip("İpucu sesi. Boşsa Resources/Sesler/Gicirti_2 (parke gıcırtısı) kullanılır.")]
    [SerializeField] private AudioClip tikirti;

    // Neden Gicirti_2: dairelerin parkesi zaten ara sıra gıcırdıyor (AyakSesi); yani bu ses binanın
    // doğal bir sesi, "oyun bana ipucu veriyor" diye değil "orada bir şey bastı" diye okunur. Kilitli
    // kapı sarsıntısı (IcKapi_Kilitli) kapıyla, anahtar şıngırtısı (Anahtar_Al) eşyayı almakla
    // karışırdı. Gicirti_2 1,5 sn ile üçünün en uzunu: kulağın yön bulmasına zaman tanır.
    // Küçük bir eşyanın yerinde kıpırdaması (#99): ipucu sesi aranan eşyanın yanından gelir; mutfakta,
    // banyoda parke gıcırtısı (eski hâli) yerine her zeminde inandırıcı olan küçük bir tıkırtı.
    private const string YedekSes = "Sesler/Kucuk_Tikirti";

    // ---------- SES AYARLARI ----------
    private const float SesDuzeyi = 0.35f;           // hafif: korkutmak değil, dikkat çekmek
    private const float EnYakin = 1f, EnUzak = 9f;   // 1 m'ye kadar tam ses, 9 m'den sonra azalmaz
    private const float PerdeEnAz = 0.9f, PerdeEnCok = 1.05f;   // her ipucunda biraz farklı perde

    // ---------- GÖRÜNTÜ ----------
    private const float ParlamaSuresi = 1.2f;        // eşyanın zorla parlama süresi (saniye)
    private const float TitremeSuresi = 0.3f;        // 2. ipucundan itibaren lambanın titremesi

    private const float YoklamaAra = 0.5f;           // ilerleme imzası bu sıklıkla okunur (saniye)

    private readonly TakilmaKurali kural = new TakilmaKurali();
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrolcu;
    private KafaLambasi lamba;
    private AudioClip klip;

    private KilitZinciri sonZincir;                  // açılış sırası hangi zincir için hesaplandı
    private List<string> acilisSirasi;               // Cozumle()'nin kilit sırası (kat başına bir kez)
    private int sonImza = -1;                        // -1: henüz okunmadı (ilk okuma ilerleme sayılmaz)
    private float yoklamaSayaci;
    private bool hedefVar;                           // işaret edilecek bir şey kaldı mı

    private void Awake()
    {
        // Aynı nesnedeki yönetici; başka bir nesneye eklendiyse sahnede aranır (Update'te).
        yonetici = GetComponent<KatYoneticisi>();
    }

    private void Update()
    {
        // Sahne yeniden yüklenince eski nesneler yok olur; Unity'de yok olmuş nesne "== null" verir.
        if (yonetici == null) yonetici = FindAnyObjectByType<KatYoneticisi>();
        if (yonetici == null) return;

        // Yeni kata varıldı: saat baştan.
        int kat = yonetici.MevcutKat;
        if (kat != kural.Kat)
        {
            kural.KatAyarla(kat);
            sonImza = -1;
            yoklamaSayaci = 0f;
        }

        // Katın zinciri (tasarımdaki katta ve zincirsiz katta null: ipucu yok). Zincir değiştiyse
        // kilitlerin açılış sırası bir kez hesaplanır (Cozumle bütün zinciri oynar; her kare değil).
        KilitZinciri zincir = yonetici.KatinZinciri(kat);
        if (zincir != sonZincir)
        {
            sonZincir = zincir;
            acilisSirasi = zincir != null ? zincir.Cozumle(TakilmaKurali.ButunOdalar()).AcilanKilitler : null;
            sonImza = -1;
        }
        if (zincir == null) return;

        // unscaledDeltaTime: oyun duraklatılmış olsa da (timeScale 0) gerçek saniyeler; yoklama menüde
        // de sürsün ki menüden çıkınca imza bayat kalmasın.
        yoklamaSayaci -= Time.unscaledDeltaTime;
        if (yoklamaSayaci <= 0f)
        {
            yoklamaSayaci = YoklamaAra;
            Yokla(kat, zincir);
        }

        // Kuralın saati: deltaTime duraklatmada zaten 0'dır; Uygun() ayrıca menüyü ve diğer hâlleri eler.
        if (kural.Ilerlet(Time.deltaTime, Uygun(kat))) IpucuVer(kat, zincir);
    }

    // ---------- İLERLEME ----------

    // İmzayı okur; değiştiyse kurala "ilerledi" der. İşaret edilecek bir şey kaldı mı da burada bulunur.
    private void Yokla(int kat, KilitZinciri zincir)
    {
        int imza = 0;
        foreach (ZincirKilidi k in zincir.Kilitler)
            if (Kayit.KilitCozukMu(kat, k.Kimlik)) imza++;
        foreach (ZincirEsyasi e in zincir.Esyalar)
            if (EsyaListesi.BulunduMu(kat, e, zincir, yonetici)) imza++;

        // İmza azaldıysa da (kaçışta yakalanıp kat baştan kurulunca) saat sıfırlanır: kat yeniden başladı.
        if (sonImza >= 0 && imza != sonImza) kural.Ilerleme();
        sonImza = imza;
        hedefVar = Adaylar(kat, zincir).Count > 0;
    }

    private List<TakilmaHedefi> Adaylar(int kat, KilitZinciri zincir)
    {
        // "x => ...": tek satırlık fonksiyon. Kural bir kimlik verir, biz "bulundu mu / çözük mü" deriz.
        return TakilmaKurali.Adaylar(zincir, acilisSirasi,
            x => { ZincirEsyasi e = zincir.EsyaBul(x); return e != null && EsyaListesi.BulunduMu(kat, e, zincir, yonetici); },
            x => Kayit.KilitCozukMu(kat, x));
    }

    // ---------- UYGUNLUK ----------

    // Oyuncu şu an "takılmış" sayılabilir mi? Hayırsa saat durur (sıfırlanmaz).
    private bool Uygun(int kat)
    {
        if (!hedefVar) return false;                                   // işaret edilecek bir şey yok
        if (Varlik.Kapali) return false;                               // test botu / kat denetimi çalışıyor
        if (DokuzuncuKat.Icinde) return false;                         // dokuzuncu kat bulmacasız: ipucu yok (#108)
        if (DuraklatmaMenusu.BaslikEkraninda || Time.timeScale <= 0f) return false;   // başlık, menü
        if (yonetici.AcilisModunda || yonetici.TurBitti) return false; // açılış (zemin kat) ya da final
        if (yonetici.BulunulanKat != kat) return false;                // oyuncu aşağı inmiş, başka kattaki dairede
        if (yonetici.KatKapisiAcikMi(kat)) return false;               // kat bitti: kapı açık
        // Av sürerken ipucu yok: o anda korku konuşur, bina yardım etmez. (#112: eskiden 2. kademede de
        // duruyordu; 7-8. katta lamba hep açık olduğundan gerilim 2. kademede kalıyor ve bütün oyunda tek
        // ipucu çalıyordu. Artık yalnızca av durdurur.)
        Varlik v = Varlik.Ornek;
        if (v != null && v.AvSuruyor) return false;
        // Senaryolu anlar sürerken ipucu saati durur (6. katın saklanma anı, 7. katın kaçışı): oyuncu o an
        // kaçmakla ya da saklanmakla meşgul; o sırada bir gıcırtı kafa karıştırır.
        foreach (SaklanmaAni s in FindObjectsByType<SaklanmaAni>()) if (s.Suruyor) return false;
        foreach (KacisAni k in FindObjectsByType<KacisAni>()) if (k.Basladi) return false;
        if (SaklanmaYeri.Saklaniyor) return false;

        // Oyuncunun kontrolü kapalıysa (not, şifre, çizim, fotoğraf ekranı, yakalanma karartması) hayır.
        if (kontrolcu == null) kontrolcu = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (kontrolcu == null || !kontrolcu.enabled) return false;

        return DairedeMi(kontrolcu.transform.position);
    }

    // Oyuncu açık dairelerden birinin bir odasında mı? (CalanEsya ve OdaYankisi ile aynı yöntem:
    // dünya noktası dairenin plan eksenlerine çevrilir, plan hangi odada olduğunu söyler.)
    private static bool DairedeMi(Vector3 konum)
    {
        // FindObjectsByType yalnızca etkin nesneleri verir: kapalı üç daire listeye girmez.
        foreach (DaireEksenleri e in FindObjectsByType<DaireEksenleri>())
        {
            e.PlanNoktasi(konum, out float u, out float v);
            if (DairePlani.NoktaninOdasi(u, v) != null) return true;
        }
        return false;
    }

    // ---------- İPUCU ----------

    private void IpucuVer(int kat, KilitZinciri zincir)
    {
        foreach (TakilmaHedefi aday in Adaylar(kat, zincir))
        {
            FenerParlamasi parlama;
            Transform hedef = NesneBul(aday, kat, zincir, out parlama);
            if (hedef == null) continue;   // sahnede yoksa (elde, başka yerde) sıradaki adaya geç

            // Ses: eşyada nesnenin kendisi (küçük), kapı ve kolda görünen gövdesinin ortası.
            SesCal(aday.Tur == TakilmaHedefTuru.Esya ? hedef.position : Merkez(hedef));

            // Parıltı: yalnızca eşyalarda (kapının parıltısı yok). Kap kapalıysa Parla kendisi atlar.
            if (parlama != null && parlama.isActiveAndEnabled) parlama.Parla(ParlamaSuresi);

            // İkinci ipucundan itibaren kafa lambası da kısaca titrer: bina ısrar ediyor.
            if (kural.IpucuSayisi >= 2)
            {
                if (lamba == null) lamba = FindAnyObjectByType<KafaLambasi>();
                if (lamba != null) lamba.Titret(TitremeSuresi);
            }

            Debug.Log($"[IPUCU] {kat}. kat: {aday.Kimlik} ipucu {kural.IpucuSayisi}/{kural.EnCok}.");
            return;
        }
        Debug.LogWarning($"[IPUCU] {kat}. kat: ipucu zamanı geldi ama işaret edilecek nesne sahnede bulunamadı.");
    }

    // Adayın sahnedeki nesnesini bulur. Eşyaysa parıltı bileşenini de verir.
    private static Transform NesneBul(TakilmaHedefi aday, int kat, KilitZinciri zincir, out FenerParlamasi parlama)
    {
        parlama = null;
        switch (aday.Tur)
        {
            case TakilmaHedefTuru.Esya:
                // Eldeki eşyanın nesnesi kapalıdır; FindObjectsByType onu zaten vermez.
                foreach (Esya e in FindObjectsByType<Esya>())
                    if (e.Kimlik == aday.Kimlik && e.gameObject.activeInHierarchy)
                    {
                        parlama = e.GetComponent<FenerParlamasi>();
                        return e.transform;
                    }
                return null;

            case TakilmaHedefTuru.Kilit:
                foreach (Kilit k in FindObjectsByType<Kilit>())
                    if (k.Veri != null && k.Kat == kat && k.Veri.Kimlik == aday.Kimlik && !k.Cozuk)
                        return k.transform;
                return null;

            case TakilmaHedefTuru.Kol:
                return YerBul(zincir.YerBul(aday.Kimlik), kat);
        }
        return null;
    }

    // Zincirdeki bir yerin sahnedeki nesnesi (kolun yeri). Yol "B:" ile başlıyorsa ikinci dairede,
    // "Sahanlik:" ile başlıyorsa sahanlıkta (ZincirUygulayici.YolBul bunu çözer).
    private static Transform YerBul(ZincirYeri yer, int kat)
    {
        if (yer == null) return null;
        Transform ana = null, ikinci = null;
        string anaAd = "Daire_" + DairePlani.AcikDaire(kat);
        foreach (DaireEksenleri e in FindObjectsByType<DaireEksenleri>())
        {
            if (DokuzuncuKat.SetinParcasi(e)) continue;   // dokuzuncu kat seti hep açık; katın dairesi değil (#110)
            if (e.name == anaAd) ana = e.transform;
            else ikinci = e.transform;   // katın açık ikinci dairesi (#70)
        }

        if (yer.PlandaNokta)
        {
            // Plandaki noktaya uygulayıcı "Yer_<kimlik>" adlı boş bir çapa koyar (dairenin çocuğu).
            Transform c = ana != null ? ana.Find("Yer_" + yer.Kimlik) : null;
            if (c == null && ikinci != null) c = ikinci.Find("Yer_" + yer.Kimlik);
            return c;
        }

        string yol = yer.Yol;
        Transform daire = ana;
        if (!string.IsNullOrEmpty(yol) && yol.StartsWith(KilitZinciri.IkinciOnek))
        {
            yol = yol.Substring(KilitZinciri.IkinciOnek.Length);
            daire = ikinci;
        }
        return daire != null ? ZincirUygulayici.YolBul(daire, yol) : null;
    }

    // Bir nesnenin görünen gövdesinin ortası (kapının menteşe ucu ya da yerdeki pivotu değil).
    private static Vector3 Merkez(Transform t)
    {
        Renderer r = t.GetComponentInChildren<Renderer>();
        return r != null ? r.bounds.center : t.position;
    }

    // Hedefin yerinde tek seferlik 3B ses. PlayClipAtPoint en yakın/en uzak mesafeyi ayarlatmadığı
    // için geçici bir nesne kurulur ve ses bitince silinir.
    private void SesCal(Vector3 konum)
    {
        if (klip == null) klip = tikirti != null ? tikirti : Resources.Load<AudioClip>(YedekSes);
        if (klip == null) { Debug.LogWarning($"[IPUCU] Ses bulunamadı ({YedekSes})."); return; }

        var g = new GameObject("Takilma_Ipucu_Sesi");
        g.transform.position = konum;
        AudioSource s = g.AddComponent<AudioSource>();
        s.clip = klip;
        s.spatialBlend = 1f;                              // tamamen 3B: kulak yönü bulsun
        s.rolloffMode = AudioRolloffMode.Logarithmic;     // gerçek dünyadaki gibi: yakında hızlı azalır
        s.minDistance = EnYakin;
        s.maxDistance = EnUzak;
        s.volume = SesDuzeyi;
        s.dopplerLevel = 0f;                              // sabit bir kaynak: perdesi kaymasın
        s.pitch = Random.Range(PerdeEnAz, PerdeEnCok);    // görüntüye ait rastgelelik; tohum gerekmez
        s.Play();
        // Destroy(nesne, saniye): nesneyi o kadar saniye sonra siler. Perde düşükse ses uzar.
        Destroy(g, klip.length / s.pitch + 0.2f);
    }
}
