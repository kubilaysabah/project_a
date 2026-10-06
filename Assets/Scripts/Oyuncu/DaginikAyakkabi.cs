// DaginikAyakkabi.cs
// Bir dairenin kapısı önünde ortada kalmış bir çift ayakkabı (#44).
//
// IEtkilesimli sözleşmesini imzalar: bakınca "RAFA KOY (E)" çıkar, E'ye basınca çift uçarak
// o dairenin ayakkabılığındaki ilk boş rafa oturur. Görev İSTEĞE BAĞLI (Şahan'ın kararı):
// yapmamak hata sayılmaz, çöp gibi zorunlu değil.
//
// İstem anomalili katta da çıkar. Çıkmasaydı, istemin yokluğu "bu katta anomali var"
// diye cevabı verirdi; oyuncu anomaliyi kendi gözüyle bulmalı.
//
// Kaç çiftin dışarıda olduğunu, hangisinin rafa konduğunu AyakkabiPlani bilir; bu bileşen
// yalnızca tek bir çiftin görünüşünü ve etkileşimini taşır (CopPoseti gibi).

using UnityEngine;

public class DaginikAyakkabi : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private bool rafaKondu;

    /// <summary>Bu çift bu katta rafa kondu mu? KatYoneticisi kat değişirken plana işler.</summary>
    public bool RafaKondu { get { return rafaKondu; } }

    private static AudioClip tik;       // çift rafa otururken hafif tahta sesi (adım sesi, kısık)

    public string Ipucu { get { return Dil.Getir("ipucu.rafa_koy"); } }
    // #113: kapı önündeki dağınık ayakkabılar da süs; rafa koymanın hiçbir sonucu yoktu (bkz. CopPoseti).
    public bool EtkilesimeAcik { get { return false; } }

    public void Etkilesim(GameObject oyuncu)
    {
        Transform hedef = BosRaf();
        rafaKondu = true;
        if (hedef != null)
        {
            // Raftaki çift uçuş bitince görünür: yerden kalkıp rafına oturur gibi.
            GameObject raftaki = hedef.gameObject;
            UcanKopya.YerineUcur(transform, hedef, () =>
            {
                raftaki.SetActive(true);
                if (tik == null) tik = Resources.Load<AudioClip>("Sesler/Adim_1");
                if (tik != null) AudioSource.PlayClipAtPoint(tik, hedef.position, 0.5f);
            });
        }
        gameObject.SetActive(false);
    }

    // Bu dairenin ayakkabılığında ilk boş raf (Raf_1'den yukarı). Yol:
    // Kapi_N/Ayakkabilar/Cift_j  ->  Kapi_N/Ayakkabilik/Raf_s/Cift
    private Transform BosRaf()
    {
        Transform kapi = transform.parent != null ? transform.parent.parent : null;
        Transform raf = kapi != null ? kapi.Find("Ayakkabilik") : null;
        if (raf == null) return null;
        for (int s = 1; s <= AyakkabiPlani.RafSayisi; s++)
        {
            Transform cift = raf.Find($"Raf_{s}/Cift");
            if (cift != null && !cift.gameObject.activeSelf) return cift;
        }
        return null;
    }

    /// <summary>
    /// Çifti o katın durumuna göre hazırlar. mevcut = false: bu katta bu çift yok.
    /// konmus = true: vardı ama oyuncu bu turda zaten rafa koydu. İkisinde de görünmez.
    /// </summary>
    public void Ayarla(bool mevcut, bool konmus)
    {
        rafaKondu = konmus;
        gameObject.SetActive(mevcut && !konmus);
    }
}
