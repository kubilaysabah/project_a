// Kirilan.cs
// Aletle kırılan nesne (#61, "kırma" kilidi): cam kavanoz, cam dolap kapağı...
//
// Zincir uygulayıcısı bileşeni kırılacak yere (kavanozun gövdesine) ekler. Kilit, oyuncu
// doğru aleti taşırken "KIR" dediğinde Kir()'i çağırır: nesnenin görüntüsü ve çarpışma
// kutusu kapanır, kapağı gibi açılır parçaları kaybolur, yerine cam kırıkları saçılır.
// Nesnenin KENDİSİ kapanmaz (SetActive false değil): içindeki eşyalar onun çocuğu ve onlar
// ortada kalmalı; oyuncu anahtarı kırıkların arasından alır.
//
// Kat değişince uygulayıcı Sifirla ile nesneyi eski hâline döndürür.

using System.Collections.Generic;
using UnityEngine;

public class Kirilan : MonoBehaviour
{
    private GameObject kirikSablonu;
    private GameObject kirik;
    private readonly List<GameObject> gizlenenParcalar = new List<GameObject>();
    private bool kirildi;
    private static AudioClip ses;

    public bool Kirildi { get { return kirildi; } }

    public void Hazirla(GameObject kirikParcalari) { kirikSablonu = kirikParcalari; }

    /// <summary>Nesneyi kırar. sesli: false ise kayıttan sessizce kırık başlatılıyor.</summary>
    public void Kir(bool sesli)
    {
        if (kirildi) return;
        kirildi = true;
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r != null) r.enabled = false;
        foreach (Collider c in GetComponents<Collider>()) c.enabled = false;
        // Açılır parçalar (kavanozun kapağı) kırılan nesneyle birlikte kaybolur.
        foreach (Transform cocuk in transform)
            if (cocuk.GetComponent<AcilirParca>() != null && cocuk.gameObject.activeSelf)
            {
                cocuk.gameObject.SetActive(false);
                gizlenenParcalar.Add(cocuk.gameObject);
            }
        if (kirikSablonu != null)
        {
            // Kırıklar nesnenin ebeveynine (tezgâha) konur: nesnenin durduğu yere saçılır.
            kirik = Instantiate(kirikSablonu, transform.parent);
            kirik.name = "Cam_Kiriklari";
            kirik.transform.position = transform.position;
            kirik.transform.rotation = transform.rotation;
        }
        if (sesli)
        {
            if (ses == null) ses = Resources.Load<AudioClip>("Sesler/Cam_Kirilma");
            if (ses != null) AudioSource.PlayClipAtPoint(ses, transform.position, 0.8f);
            Varlik.Gurultu(0.25f);   // cam kırıldı: bina duydu (#86)
        }
    }

    /// <summary>Kat değişince nesne eski hâline döner (bileşen sökülmeden önce).</summary>
    public void Sifirla()
    {
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r != null) r.enabled = true;
        foreach (Collider c in GetComponents<Collider>()) c.enabled = true;
        foreach (GameObject g in gizlenenParcalar) if (g != null) g.SetActive(true);
        gizlenenParcalar.Clear();
        if (kirik != null) Destroy(kirik);
        kirildi = false;
    }
}
