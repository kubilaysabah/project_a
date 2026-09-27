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
                default:
                    return Dil.Getir("ipucu.kilitli");
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
        if (acilmaSesi == null) acilmaSesi = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilit");
        if (acilmaSesi != null) AudioSource.PlayClipAtPoint(acilmaSesi, transform.position, 0.7f);
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
    }

    private void KapiyiKilitle(bool k)
    {
        IcKapi kapi = GetComponent<IcKapi>();
        if (kapi != null) kapi.Kilitle(k);
        AcilirParca parca = GetComponent<AcilirParca>();
        if (parca != null) parca.Kilitle(k);
    }
}
