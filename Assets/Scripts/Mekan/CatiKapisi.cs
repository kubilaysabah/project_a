// CatiKapisi.cs
// Çatıya çıkan kapı (#68). Yönetim'in ilk notu "sabah çatı kapısını kilitle" der; kapının
// anahtarı binanın son sırrıdır: 8. katta asansör dokuzuncu kattan iner ve anahtar kabinde
// gelir. 8. katın zincirinde çatı anahtarı katın anahtarıdır (KatAnahtari, kat 8): her katın
// anahtarı o katın çıkış kapısını açar, 8. katın çıkışı da çatıdır.
//
//   Anahtar elde değilse : "KİLİTLİ"; basınca kapı sarsılır (kat kapısının kilitli sesi).
//   Anahtar eldeyse      : "AÇ"; anahtar kullanılır, kat yöneticisi kapıyı açar, final başlar.
//
// Eski kuralda (dönüp dolaşma oyunu, KatYoneticisi.eskiKural) kapı çatıya varınca kendiliğinden
// açılıyordu; o kural açıkken bu bileşen istem göstermez.
//
// Graybox kurucusu çatı bölümündeki kapıya ekler.

using UnityEngine;

public class CatiKapisi : MonoBehaviour, IEtkilesimli
{
    private static AudioClip kilitli, kilitAcilma;
    private KatYoneticisi yonetici;

    private KatYoneticisi Yonetici
    {
        get
        {
            if (yonetici == null) yonetici = FindAnyObjectByType<KatYoneticisi>();
            return yonetici;
        }
    }

    public string Ipucu
    {
        get { return Cep.Tasiyor(EsyaTuru.KatAnahtari, KatSayaci.CatiKati) ? Dil.Getir("ipucu.ac") : Dil.Getir("ipucu.kilitli"); }
    }

    public bool EtkilesimeAcik { get { return Yonetici != null && !Yonetici.EskiKural; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        if (!Cep.Kullan(EsyaTuru.KatAnahtari, KatSayaci.CatiKati))
        {
            if (kilitli == null) kilitli = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilitli");
            if (kilitli != null) AudioSource.PlayClipAtPoint(kilitli, transform.position, 0.8f);
            return;
        }
        if (kilitAcilma == null) kilitAcilma = Resources.Load<AudioClip>("Sesler/KatKapisi_Kilit");
        if (kilitAcilma != null) AudioSource.PlayClipAtPoint(kilitAcilma, transform.position, 0.8f);
        Yonetici.CatiKapisiAnahtarlaAcildi();
    }
}
