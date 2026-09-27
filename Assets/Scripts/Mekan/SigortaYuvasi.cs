// SigortaYuvasi.cs
// Sigorta kutusunun boş yuvası (#54, "sigorta" kilidi). Zincirde sigorta kilidi olan katta
// kutudaki dört sigortadan biri eksiktir ve dairede elektrik yoktur. Oyuncu buşonu bulur,
// kutunun kapağını açar; eli doğru buşonu taşırken kutuya bakınca "TAK (E)". Takınca yuva
// dolar, elektrik gelir (DaireElektrigi) ve sigortaya bağlı kilitler açılır.
//
// Bileşen sigorta kutusunun gövdesinde durur (zincir uygulayıcısı ekler). Kapak kapalıyken
// ya da elde buşon yokken istem çıkmaz: kutu yalnızca "bir sigortası eksik" görünür; oyuncu
// ne araması gerektiğini gözüyle anlar.

using System.Collections.Generic;
using UnityEngine;

public class SigortaYuvasi : MonoBehaviour, IEtkilesimli
{
    private string gerekliSigorta;
    private GameObject eksikSigorta;         // gizlenen sigorta: takılınca görünür
    private AcilirParca kapak;
    private DaireElektrigi elektrik;
    private readonly List<Kilit> kilitler = new List<Kilit>();
    private bool takildi;
    private static AudioClip ses;

    public bool Takildi { get { return takildi; } }

    public void Hazirla(string sigortaKimligi, GameObject eksik, AcilirParca kutuKapagi, DaireElektrigi daireElektrigi)
    {
        gerekliSigorta = sigortaKimligi;
        eksikSigorta = eksik;
        kapak = kutuKapagi;
        elektrik = daireElektrigi;
        takildi = false;
        if (eksikSigorta != null) eksikSigorta.SetActive(false);
        if (elektrik != null) elektrik.Ayarla(false);
    }

    public void KilitEkle(Kilit k) { if (k != null && !kilitler.Contains(k)) kilitler.Add(k); }

    public string Ipucu { get { return Dil.Getir("ipucu.tak"); } }

    public bool EtkilesimeAcik
    {
        get { return !takildi && (kapak == null || kapak.Acik) && Cep.KimligiTasiyor(gerekliSigorta); }
    }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik || !Cep.KimlikleKullan(gerekliSigorta)) return;
        Tak(sesli: true);
    }

    private void Tak(bool sesli)
    {
        takildi = true;
        if (eksikSigorta != null) eksikSigorta.SetActive(true);
        if (sesli)
        {
            if (ses == null) ses = Resources.Load<AudioClip>("Sesler/Sigorta_Takma");
            if (ses != null) AudioSource.PlayClipAtPoint(ses, transform.position, 0.7f);
        }
        if (elektrik != null) elektrik.Ayarla(true);
        foreach (Kilit k in kilitler)
            if (k != null && !k.Cozuk)
            {
                if (sesli) k.Coz(acilsin: false); else k.CozukBaslat();
            }
    }

    /// <summary>Kaydedilmiş bir takılı sigortayı sessizce takılı başlatır (adım 7).</summary>
    public void TakiliBaslat() { Tak(sesli: false); }

    /// <summary>Kat değişince: eksik sigorta geri görünür, elektrik gelir, bileşen sökülmeden önce.</summary>
    public void Sifirla()
    {
        if (eksikSigorta != null) eksikSigorta.SetActive(true);
        if (elektrik != null) elektrik.Ayarla(true);
        kilitler.Clear();
        takildi = false;
    }
}
