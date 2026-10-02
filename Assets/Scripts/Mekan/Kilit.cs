// Kilit.cs
// Bir oda kapısının (IcKapi) ya da bir eşya parçasının (AcilirParca) kilidi (#54).
//
// Kilit, zincir uygulayıcısı (ZincirUygulayici) dairenin o katını hazırlarken kapıya ya da
// parçaya eklenir ve zincirdeki bir kilidin verisini (ZincirKilidi) taşır. Kapı kendisine
// dokunulunca önce kilidine sorar: kilit çözülmemişse ipucunu ve etkileşimi kilit belirler.
//
// Türlere göre:
//   Anahtar   : doğru oda anahtarı eldeyse "AÇ" — basınca anahtar kullanılır, kilit çözülür,
//               kapı açılır. Değilse "KİLİTLİ" ve kapı sarsılır.
//   Şifre     : asma kilidin çarkları (adım 3).
//   Kol       : kapıdan açılmaz; başka yerde çekilen kol Coz() der (adım 4).
//   Sigorta   : kapıdan açılmaz; sigorta takılınca Coz() denir (adım 5).
//   Tornavida : tornavida eldeyse menteşeler sökülür (adım 6).
//
// Çözülen her kilit Cozuldu olayıyla duyurulur: kayıt (adım 7) ve sesler buna abone olur.

using UnityEngine;

public class Kilit : MonoBehaviour
{
    public ZincirKilidi Veri { get; private set; }
    public int Kat { get; private set; }
    public bool Cozuk { get; private set; }

    /// <summary>Bir kilit çözüldü (kat, kilit kimliği). Kayıt ve ses buna abone olur.</summary>
    public static event System.Action<int, string> Cozuldu;

    private static AudioClip acilmaSesi;

    public void Ayarla(ZincirKilidi veri, int kat)
    {
        Veri = veri;
        Kat = kat;
        Cozuk = false;
        KapiyiKilitle(true);
    }

    /// <summary>Kapının üstünde görünecek ipucu (kilit çözülmemişken).</summary>
    public string Ipucu
    {
        get
        {
            switch (Veri.Tur)
            {
                case KilitTuru.Anahtar:
                    return Cep.KimligiTasiyor(Veri.Gerekli) ? Dil.Getir("ipucu.ac") : Dil.Getir("ipucu.kilitli");
                case KilitTuru.Sifre:
                    return Dil.Getir("ipucu.sifre");
                // Aletler elde değil kapıcının çantasında (#61).
                case KilitTuru.Tornavida:
                    return Aletler.Var(Veri.Gerekli) ? Dil.Getir("ipucu.sok") : Dil.Getir("ipucu.kilitli");
                case KilitTuru.Kirma:
                    return Aletler.Var(Veri.Gerekli) ? Dil.Getir("ipucu.kir") : Dil.Getir("ipucu.kilitli");
                case KilitTuru.KirikAnahtar:
                    // Parçalar tamamsa "AÇ"; değilse "KİLİTLİ" (kaç parça bulunduğu köşede yazar).
                    return ParcalarTamam() ? Dil.Getir("ipucu.ac") : Dil.Getir("ipucu.kilitli");
                case KilitTuru.Ekipman:
                    // Aletler tamamsa "İT", değilse "AĞIR": oyuncu neden kıpırdamadığını anlasın.
                    // Veride kendi istemi varsa (sandık: "KANIRT") o; yoksa gardırobun "İT / AĞIR"ı.
                    if (!string.IsNullOrEmpty(Veri.IpucuMetni))
                        return EkipmanTamam() ? Dil.Getir(Veri.IpucuMetni) : Dil.Getir("ipucu.kilitli");
                    return EkipmanTamam() ? Dil.Getir("ipucu.it") : Dil.Getir("ipucu.agir");
                default:
                    return Dil.Getir("ipucu.kilitli");
            }
        }
    }

    /// <summary>
    /// Nişangâh simgesi (#104): Ipucu ile aynı kararlar, yazı yerine işaret. Anahtarla ya da tamamlanan
    /// parçalarla açılacaksa ANAHTAR; aletle sökülecek / kırılacak / itilecekse EL; şifre KİLİTLİ (çevirmeli
    /// kilit görüntüsü zaten şifreyi anlatır); eksik bir şey varsa KİLİTLİ.
    /// </summary>
    public NisanSimgesi Simge
    {
        get
        {
            switch (Veri.Tur)
            {
                case KilitTuru.Anahtar: return Cep.KimligiTasiyor(Veri.Gerekli) ? NisanSimgesi.Anahtar : NisanSimgesi.Kilitli;
                case KilitTuru.KirikAnahtar: return ParcalarTamam() ? NisanSimgesi.Anahtar : NisanSimgesi.Kilitli;
                case KilitTuru.Tornavida:
                case KilitTuru.Kirma: return Aletler.Var(Veri.Gerekli) ? NisanSimgesi.El : NisanSimgesi.Kilitli;
                case KilitTuru.Ekipman: return EkipmanTamam() ? NisanSimgesi.El : NisanSimgesi.Kilitli;
                default: return NisanSimgesi.Kilitli;
            }
        }
    }

    /// <summary>Oyuncu kilitli kapıya dokundu. true: kilit bu dokunuşu karşıladı.</summary>
    public bool Etkilesim(GameObject oyuncu)
    {
        if (Cozuk) return false;
        if (Veri.Tur == KilitTuru.Anahtar && Cep.KimlikleKullan(Veri.Gerekli))
        {
            Coz(acilsin: true);
            return true;
        }
        if (Veri.Tur == KilitTuru.Tornavida && Aletler.Var(Veri.Gerekli))
        {
            // Her basış bir vida; son vida çıkınca kapı devrilir. Tornavida bir alettir,
            // bitmez: oyuncunun elinde kalır, başka bir yerde yine işe yarayabilir (#61).
            MenteseSokucu sokucu = GetComponent<MenteseSokucu>();
            if (sokucu == null || sokucu.VidaSok()) Coz(acilsin: false);
            return true;
        }
        if (Veri.Tur == KilitTuru.Kirma && Aletler.Var(Veri.Gerekli))
        {
            // Kilit kavanozun kapağında; kırılan kavanozun kendisi (kapağın ebeveyni).
            // Alet elde kalır.
            Kirilan kirilan = GetComponentInParent<Kirilan>();
            if (kirilan != null) kirilan.Kir(sesli: true);
            Coz(acilsin: false);
            return true;
        }
        if (Veri.Tur == KilitTuru.KirikAnahtar && ParcalarTamam())
        {
            // Anahtar tamamlandı: kapı açılır; parçalar kullanılmış olur (kayıtta çözük kilit).
            Coz(acilsin: true);
            return true;
        }
        if (Veri.Tur == KilitTuru.Ekipman && EkipmanTamam())
        {
            // Aletler çantada: kilit çözülür ve parça (gardırop) hemen hareket eder.
            Coz(acilsin: true);
            return true;
        }
        if (Veri.Tur == KilitTuru.Sifre)
        {
            // Asma kilidin yakın bakış ekranı; doğru şifrede ekran Coz'u kendisi çağırır.
            SifreEkrani ekran = GetComponent<SifreEkrani>();
            if (ekran == null) ekran = gameObject.AddComponent<SifreEkrani>();
            ekran.Ac(this, oyuncu);
            return true;
        }
        // Açacak şey yok: kapı ya da parça kendi "kilitli" sesini çalsın.
        return false;
    }

    /// <summary>Kilidi çözer. acilsin: kapı hemen açılsın mı (anahtarla açınca evet; uzaktaki
    /// kol ya da sigorta çözünce hayır, kapı yalnızca serbest kalır).</summary>
    public void Coz(bool acilsin)
    {
        if (Cozuk) return;
        Cozuk = true;
        KapiyiKilitle(false);
        // Oda kapısının ve asma kilidin küçük kilidi (#99; eskiden demir kat kapısının büyük kilidiydi).
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/Kilit_Kucuk");
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilit");
        if (acilmaSesi != null) AudioSource.PlayClipAtPoint(acilmaSesi, transform.position, 0.7f);
        // Asma kilit açılınca kapıdan düşer (şimdilik yalnızca kaldırılıyor).
        if (asmaKilit != null) Destroy(asmaKilit);
        if (acilsin)
        {
            IcKapi kapi = GetComponent<IcKapi>();
            if (kapi != null) kapi.Etkilesim(null);
            AcilirParca parca = GetComponent<AcilirParca>();
            if (parca != null) parca.Etkilesim(null);
        }
        Debug.Log($"[KILIT] {Kat}. kat: '{Veri.Kimlik}' ({Veri.Tur}) çözüldü.");
        Cozuldu?.Invoke(Kat, Veri.Kimlik);
    }

    /// <summary>Kaydedilmiş bir kilidi sessizce çözük başlatır (adım 7).</summary>
    public void CozukBaslat()
    {
        Cozuk = true;
        KapiyiKilitle(false);
        if (asmaKilit != null) Destroy(asmaKilit);
    }

    // ---------- ASMA KİLİT (şifreli kilidin görünen yüzü) ----------
    private GameObject asmaKilit;

    /// <summary>Şifreli kilidin asma kilidini kapıya ya da parçaya takar; takılan nesneyi döndürür.</summary>
    public GameObject AsmaKilitTak(GameObject sablon)
    {
        if (sablon == null) return null;
        Vector3 konum, disaYon;
        Transform ebeveyn;
        IcKapi kapi = GetComponent<IcKapi>();
        if (kapi == null || !kapi.OnNoktasi(0.82f, 1.15f, out konum, out disaYon, out ebeveyn))
        {
            // Kapı değilse (dolap kapağı, çekmece): parçanın ön yüzünün ortası. Modellerin ön
            // yüzü +Z'ye baktığı için meshin sınırlarının en ön (max z) yüzü kullanılır.
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return null;
            Bounds b = mf.sharedMesh.bounds;
            konum = transform.TransformPoint(new Vector3(b.center.x, b.center.y, b.max.z + 0.005f));
            disaYon = transform.forward;
            ebeveyn = transform;
        }
        asmaKilit = Instantiate(sablon, ebeveyn);
        asmaKilit.name = "Asma_Kilit";
        asmaKilit.transform.position = konum;
        // Modelin ön yüzü +Z: dışarıya (oyuncuya) baksın.
        asmaKilit.transform.rotation = Quaternion.LookRotation(disaYon, Vector3.up);
        return asmaKilit;
    }

    private void KapiyiKilitle(bool k)
    {
        IcKapi kapi = GetComponent<IcKapi>();
        if (kapi != null) kapi.Kilitle(k);
        AcilirParca parca = GetComponent<AcilirParca>();
        if (parca != null) parca.Kilitle(k);
    }

    // Kırık anahtarın bütün parçaları bulundu mu (#65)? Parçalar kat kaydında tutulur.
    private bool ParcalarTamam()
    {
        if (Veri.Gerekenler == null || Veri.Gerekenler.Count == 0) return false;
        foreach (string g in Veri.Gerekenler) if (!Kayit.ParcaBulunduMu(Kat, g)) return false;
        return true;
    }

    // Ekipman kilidinin bütün aletleri kapıcının çantasında mı (#65)?
    private bool EkipmanTamam()
    {
        if (Veri.Gerekenler == null || Veri.Gerekenler.Count == 0) return false;
        foreach (string g in Veri.Gerekenler) if (!Aletler.Var(g)) return false;
        return true;
    }
}
