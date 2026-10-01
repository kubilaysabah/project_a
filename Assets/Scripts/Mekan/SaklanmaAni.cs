// SaklanmaAni.cs
// 6. katın saklanma anı (#58). Katın anahtarı alınınca dairenin kapısında tıklatma duyulur,
// kafa lambası titrer; birkaç saniye sonra Gölge giriş kapısından içeri girer, holün sonuna kadar
// yürür ve geri döner. Oyuncunun işi görünmemek: bir odaya girip kapısını kapatmak ya da bir
// eşyanın arkasına eğilmek (#79). Kaçmak (7. kattaki kovalamaca, KacisAni) burada işe yaramaz.
//
// GÖRMEK NE DEMEK
// Her karede Gölge'nin başından oyuncunun gözüne düz bir çizgi (Physics.Linecast) çekilir. Arada
// bir çarpıştırıcı yoksa (duvar, kapalı kapı, mobilya), oyuncu yakınsa ve Gölge'nin önündeki
// koni içindeyse Gölge onu görüyordur. Kısa bir bakış affedilir; görmek bir süre sürerse yakalar.
//
// Yakalanınca (#58'in kuralı): karanlık, oyuncu katın başında uyanır, çözülmüş kilitler çözülmüş
// kalır, anahtar büfeye döner ve an yeniden kurulur. Gölge oyuncuyu görmeden turunu bitirirse an
// biter ve kaydedilir ("saklanma"); oyuncu kat kapısını açarsa da biter.

using System.Collections.Generic;
using UnityEngine;

public class SaklanmaAni : MonoBehaviour
{
    private const float Uyari = 6f;              // tıklatmadan Gölge'nin girişine kadar (saniye)
    private const float Hiz = 1.3f;              // Gölge'nin yürüyüşü (m/sn)
    private const float GormeMesafesi = 9f;
    private const float GormeAcisi = 70f;        // önündeki koninin yarı açısı (derece)
    private const float GormeSuresi = 0.4f;      // bu kadar süre görürse yakalar
    private const float BasYuksekligi = 1.85f;

    private int kat;
    private GameObject sablon;
    private AudioClip tiklatma;
    private readonly List<Vector3> yol = new List<Vector3>();

    private enum Durum { Bekliyor, Uyari, Devriye, Bitti }
    private Durum durum = Durum.Bekliyor;
    private float sayac, gormeSayaci;
    private int hedef;
    private GameObject golge;
    private Transform oyuncu, goz;
    private BirinciSahisKontrolcu kontrolcu;

    /// <summary>
    /// Anı kurar. yolNoktalari: Gölge'nin yürüyeceği noktalar (dünya konumu, ayak hizası);
    /// ilki giriş kapısı. kapi: tıklatmanın duyulacağı yer.
    /// </summary>
    public void Hazirla(int katNo, GameObject golgeSablonu, AudioClip tiklatmaSesi, List<Vector3> yolNoktalari)
    {
        kat = katNo;
        sablon = golgeSablonu;
        tiklatma = tiklatmaSesi;
        yol.Clear();
        yol.AddRange(yolNoktalari);
        kontrolcu = FindAnyObjectByType<BirinciSahisKontrolcu>();
        oyuncu = kontrolcu != null ? kontrolcu.transform : null;
        goz = kontrolcu != null && kontrolcu.transform.childCount > 0 ? kontrolcu.transform.GetChild(0) : null;
        foreach (KatKapisi kapi in FindObjectsByType<KatKapisi>()) kapi.AcilisBasladi += KapiAcildi;
    }

    /// <summary>Kayıttan: bu katın saklanma anı daha önce atlatıldı.</summary>
    public void BittiBaslat() { durum = Durum.Bitti; }

    /// <summary>Uyarı ya da devriye sürüyor mu (Varlık, #86, bu sırada durur)?</summary>
    public bool Suruyor { get { return durum == Durum.Uyari || durum == Durum.Devriye; } }

    /// <summary>Devriyedeki Gölge (yoksa null). SaklanmaYeri (#97): Gölge yakındayken dolaba girilmez.</summary>
    public Transform Golge { get { return golge != null ? golge.transform : null; } }

    private void OnDestroy()
    {
        foreach (KatKapisi kapi in FindObjectsByType<KatKapisi>()) kapi.AcilisBasladi -= KapiAcildi;
        if (golge != null) Destroy(golge);
    }

    private void KapiAcildi(int acilanKat)
    {
        if (acilanKat != kat || durum == Durum.Bitti) return;
        Bitir("kat kapısı açıldı");
    }

    private void Update()
    {
        if (oyuncu == null || durum == Durum.Bitti || yol.Count < 2) return;

        if (durum == Durum.Bekliyor)
        {
            if (Cep.Tasiyor(EsyaTuru.KatAnahtari, kat)) UyariBaslat();
            return;
        }

        sayac += Time.deltaTime;
        if (durum == Durum.Uyari)
        {
            if (sayac < Uyari) return;
            golge = Instantiate(sablon, yol[0], Quaternion.LookRotation(yol[1] - yol[0]));
            golge.name = "Saklanma_Golgesi";
            hedef = 1;
            durum = Durum.Devriye;
            Debug.Log($"[SAKLANMA] {kat}. kat: Gölge içeri girdi.");
            return;
        }

        Yuru();
        Bak();
    }

    private void UyariBaslat()
    {
        durum = Durum.Uyari;
        sayac = 0f;
        KafaLambasi lamba = FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Titret(1.6f);
        // Tıklatma dairenin giriş kapısından: oyuncu nereden geleceğini duyar.
        if (tiklatma != null) AudioSource.PlayClipAtPoint(tiklatma, yol[0] + Vector3.up * 1.4f, 1f);
        ArkaMuzik.Ornek?.Kabar();
        Debug.Log($"[SAKLANMA] {kat}. kat: kapı tıklatıldı; {Uyari:0} sn içinde saklanmalı.");
    }

    private void Yuru()
    {
        float kalan = Hiz * Time.deltaTime;
        while (kalan > 0f && hedef < yol.Count)
        {
            Vector3 fark = yol[hedef] - golge.transform.position;
            float mesafe = fark.magnitude;
            Vector3 yatay = new Vector3(fark.x, 0f, fark.z);
            if (yatay.sqrMagnitude > 0.0001f)
                golge.transform.rotation = Quaternion.Slerp(golge.transform.rotation, Quaternion.LookRotation(yatay), 8f * Time.deltaTime);
            if (mesafe <= kalan) { golge.transform.position = yol[hedef]; kalan -= mesafe; hedef++; }
            else { golge.transform.position += fark / mesafe * kalan; kalan = 0f; }
        }
        if (hedef >= yol.Count) Bitir("Gölge turunu bitirdi, oyuncuyu görmedi");
    }

    // Gölge oyuncuyu görüyor mu? Kısa bakışlar birikmez: görmediği anda sayaç yavaşça söner.
    // Dolapta saklanan oyuncu (#97, SaklanmaYeri) hiç görülmez: Gölge dolabın önünden geçip gider.
    private void Bak()
    {
        if (golge == null || goz == null) return;
        Vector3 bas = golge.transform.position + Vector3.up * BasYuksekligi;
        Vector3 hedefNokta = goz.position;
        Vector3 fark = hedefNokta - bas;
        bool goruyor = !SaklanmaYeri.Saklaniyor
                       && fark.magnitude < GormeMesafesi
                       && Vector3.Angle(golge.transform.forward, new Vector3(fark.x, 0f, fark.z)) < GormeAcisi
                       && !ArasiKapali(bas, hedefNokta);
        gormeSayaci = goruyor ? gormeSayaci + Time.deltaTime : Mathf.Max(0f, gormeSayaci - Time.deltaTime * 0.5f);
        if (gormeSayaci >= GormeSuresi) Yakalandi();
    }

    // İki nokta arasında Gölge'nin ve oyuncunun kendisi dışında bir çarpıştırıcı var mı?
    private bool ArasiKapali(Vector3 a, Vector3 b)
    {
        foreach (RaycastHit h in Physics.RaycastAll(a, b - a, (b - a).magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (t.IsChildOf(oyuncu) || (golge != null && t.IsChildOf(golge.transform))) continue;
            return true;
        }
        return false;
    }

    private void Yakalandi()
    {
        durum = Durum.Bitti;
        Debug.Log($"[SAKLANMA] {kat}. kat: Gölge gördü; kat başında uyanılacak (kilitler kalır).");
        if (golge != null) Destroy(golge);
        KatYoneticisi y = FindAnyObjectByType<KatYoneticisi>();
        if (y != null) y.SaklanmadaYakalandi(kat);
    }

    private void Bitir(string neden)
    {
        durum = Durum.Bitti;
        if (golge != null) Destroy(golge);
        Kayit.KilitCozuldu(kat, "saklanma");   // kaydet: bir daha tetiklenmesin
        Debug.Log($"[SAKLANMA] {kat}. kat: an bitti ({neden}).");
    }
}
