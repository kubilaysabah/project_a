// CekilenKol.cs
// "Kol" kilidinin çekilen kolu (#54, Docs/hikaye.md §6): kilerdeki duvar kolu, sigorta
// kutusunun ana şalteri... Bakınca "ÇEK (E)"; basınca kol milinde döner, aşağıda kalır ve
// bağlı olduğu kilitler uzakta "tak" diye açılır (sesi kapının yerinden gelir, oyuncu hangi
// kapının açıldığını kulağıyla bulur).
//
// Bileşeni zincir uygulayıcısı (ZincirUygulayici) kola ekler ve hangi kilitleri açacağını
// yazar; kolun kendisi bir kilidin ne olduğunu bilmez, yalnızca Coz() der. Kat değişince
// uygulayıcı kolu eski duruşuna döndürüp bileşeni söker (Sifirla).
//
// Kol modellerde YUKARI dönük durur (çekilmemiş). Yerel x ekseni etrafında pozitif dönüş,
// kolun ucunu önce oyuncuya (+Z), sonra aşağı getirir.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CekilenKol : MonoBehaviour, IEtkilesimli
{
    private const float CekmeSuresi = 0.45f;

    private readonly List<Kilit> kilitler = new List<Kilit>();
    private float aci = 120f;            // kolun çekilince döneceği açı (derece)
    private Vector3 eksen = Vector3.right;   // dönüş ekseni (yerel); KolAyari değiştirebilir (#65)
    private float sure = CekmeSuresi;
    private string ipucuAnahtari = "ipucu.cek";
    private Quaternion ilkDonus;
    private bool cekildi;
    private bool harekette;
    // Kolun KENDİ sesi (#99): eskiden "static" idi, duvardaki kol ile 5. kattaki vana çarkı aynı sesi
    // paylaşıyordu. Çark (adında "Cark" ya da "Vana" geçen parça) dökme demir vananın dönme sesini,
    // öbür kollar kol çekme sesini çalar.
    private AudioClip ses;

    /// <summary>Uygulayıcı kolu hazırlar: çekilince açılacak kilit ve kolun döneceği açı.</summary>
    public void KilitEkle(Kilit kilit, float donusAcisi)
    {
        if (kilitler.Count == 0) ilkDonus = transform.localRotation;
        if (kilit != null && !kilitler.Contains(kilit)) kilitler.Add(kilit);
        aci = donusAcisi;
        // Parçanın kendi hareket ayarı varsa (vananın çarkı) o geçerli (#65).
        KolAyari ayar = GetComponent<KolAyari>();
        if (ayar != null) { eksen = ayar.eksen; aci = ayar.aci; sure = ayar.sure; ipucuAnahtari = ayar.ipucuAnahtari; }
    }

    public string Ipucu { get { return Dil.Getir(ipucuAnahtari); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.Kol; } }   // #104: çekilecek kol / çevrilecek çark

    // Bir kez çekilir; aşağıda kalır.
    public bool EtkilesimeAcik { get { return !cekildi && !harekette; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (cekildi || harekette) return;
        StartCoroutine(Cek());
    }

    private IEnumerator Cek()
    {
        harekette = true;
        if (ses == null)
        {
            string yol = AcilirParca.AdYolu(transform).ToLowerInvariant();
            if (yol.Contains("cark") || yol.Contains("vana")) ses = Resources.Load<AudioClip>("Sesler/Vana_Cark");
            if (ses == null) ses = Resources.Load<AudioClip>("Sesler/Kol_Cekme");
        }
        if (ses != null) AudioSource.PlayClipAtPoint(ses, transform.position, 0.7f);
        Varlik.Gurultu(0.1f);    // kol çekildi: bina duydu (#86)
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            // Hızlanarak iner (t*t): ağır bir kol önce direnir, sonra düşer.
            float e = Mathf.Clamp01(t / sure);
            // AngleAxis: verilen eksen etrafında verilen açı kadar dönüş (360'tan büyük de olur).
            transform.localRotation = ilkDonus * Quaternion.AngleAxis(aci * e * e, eksen);
            yield return null;
        }
        transform.localRotation = ilkDonus * Quaternion.AngleAxis(aci, eksen);
        cekildi = true;
        harekette = false;
        // Kısa bir gecikmeyle uzaktaki kilit açılsın: önce kolun sesi, sonra kapının "tak"ı.
        yield return new WaitForSeconds(0.35f);
        foreach (Kilit k in kilitler)
            if (k != null && !k.Cozuk) k.Coz(acilsin: false);
    }

    /// <summary>Kaydedilmiş bir çekilmiş kolu sessizce aşağıda başlatır (adım 7).</summary>
    public void CekilmisBaslat()
    {
        StopAllCoroutines();
        transform.localRotation = ilkDonus * Quaternion.AngleAxis(aci, eksen);
        cekildi = true;
        harekette = false;
    }

    /// <summary>Kat değişince kol eski duruşuna döner (bileşen sökülmeden önce).</summary>
    public void Sifirla()
    {
        StopAllCoroutines();
        if (kilitler.Count > 0 || cekildi) transform.localRotation = ilkDonus;
        cekildi = false;
        harekette = false;
        kilitler.Clear();
    }
}
