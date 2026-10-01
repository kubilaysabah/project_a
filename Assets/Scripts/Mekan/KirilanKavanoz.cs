// KirilanKavanoz.cs
// Tornavidayla kırılabilen süs kavanozu (#83, 1. katın mutfağı; Şahan'ın isteği 2026-09-29).
//
// Mutfak tezgâhında beş kavanoz durur: dördü süs (turşu, salça, reçel, bakliyat), biri içinde Oda A'nın
// anahtarı olan zincir kavanozu. Şahan'ın kuralı: HEPSİ tornavidayla kırılabilsin; oyuncu anahtarın
// hangisinde olduğunu bir nottan değil, camın arkasına bakarak ya da kırarak bulsun. Ekranda yazı
// çıkmaz; bakılan kavanoz etkileşime açıksa nişangâhın içi kırmızı dolar (Etkilesimci).
//
// Zincir kavanozunu "kırma" kilidi yönetir (Kilit + Kirilan); bu bileşen yalnızca süs kavanozları için.
// Süs kavanozu kat dosyasının süsüdür: kat her açıldığında yeniden konur, yani kırılan süs kavanozu
// katı tekrar açınca yerinde durur (kaydedilmez; görevle ilgisi yok).

using UnityEngine;

public class KirilanKavanoz : MonoBehaviour, IEtkilesimli
{
    // Kırılınca yerine saçılacak cam kırıkları (şablonu kuran GrayboxKurucu bağlar: prefab içinde
    // başvuru, derlenmiş oyunda da çalışsın diye adla aranmaz).
    [SerializeField] private GameObject kirikSablonu;

    // Hangi aletle kırılır: alet çantasındaki kimliği (Aletler).
    [SerializeField] private string alet = "tornavida";

    private bool kirildi;
    private static AudioClip kirilmaSesi, kilitliSes;

    /// <summary>Şablonu kurarken kırık şablonunu bağlar (yalnızca editörden).</summary>
    public void Hazirla(GameObject kirik) { kirikSablonu = kirik; }

    // IEtkilesimli sözleşmesi. Yazı ekranda gösterilmiyor (#72) ama sözleşme bir ipucu ister;
    // zincir kavanozunun kilidiyle aynı sözcükler.
    public string Ipucu { get { return Dil.Getir(Aletler.Var(alet) ? "ipucu.kir" : "ipucu.kilitli"); } }

    // Kırılmamışsa her zaman açık: tornavida yokken de nişangâh dolar ve kilitli sesi duyulur,
    // zincir kavanozuyla aynı davranış (oyuncu hangisinin farklı olduğunu buradan anlayamasın).
    public bool EtkilesimeAcik { get { return !kirildi; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (kirildi) return;
        if (!Aletler.Var(alet))
        {
            // Alet yok: kapağı sıkışık, açılmıyor. İç kapıların "kilitli" sesi.
            if (kilitliSes == null) kilitliSes = Resources.Load<AudioClip>("Sesler/IcKapi_Kilitli");
            if (kilitliSes != null) AudioSource.PlayClipAtPoint(kilitliSes, transform.position, 0.5f);
            return;
        }
        Kir();
    }

    // Kavanozu kırar: kendisinin ve çocuklarının (cam, kapak, içindekiler) görüntüsü ve çarpışma
    // kutuları kapanır, yerine cam kırıkları saçılır, cam kırılma sesi çalar.
    private void Kir()
    {
        kirildi = true;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (kirikSablonu != null)
        {
            // Kırıklar kavanozun çocuğu olur: kat kapanınca süsle birlikte silinir.
            GameObject kirik = Instantiate(kirikSablonu, transform);
            kirik.name = "Cam_Kiriklari";
            kirik.transform.localPosition = Vector3.zero;
            kirik.transform.localRotation = Quaternion.identity;
        }
        if (kirilmaSesi == null) kirilmaSesi = Resources.Load<AudioClip>("Sesler/Cam_Kirilma");
        if (kirilmaSesi != null) AudioSource.PlayClipAtPoint(kirilmaSesi, transform.position, 0.8f);
        Varlik.Gurultu(0.25f);   // cam kırıldı: bina duydu (#86)
    }
}
