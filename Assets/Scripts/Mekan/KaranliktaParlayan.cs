// KaranliktaParlayan.cs
// Karanlıkta parlayan boya (#64, 4. kat: ışık ve karanlık).
//
// Fosforlu boya gün boyu ışığı emer, karanlıkta yeşil yeşil parlar; ışık altında ise görünmez
// olur (parıltı ışığın yanında kaybolur). Önceki kapıcı Hasan, 4. katın dolap şifresini salon
// duvarına bu boyayla yazmış. Oyuncu kafa lambasını açık tuttukça yazıyı hiç görmez; lambayı
// söndürüp beklerse rakamlar yavaş yavaş belirir (göz karanlığa alışıyormuş gibi). Oyunun her
// yerinde lambayı yakmayı öğrenen oyuncuya bir kez de söndürmeyi öğretir.
//
// Kural: yazı yalnızca kafa lambası KAPALIYKEN ve dairede ELEKTRİK YOKKEN görünür (elektrik
// gelince tavan lambaları yanar, parıltı kaybolur). Belirmesi 2 sn sürer, kaybolması anlık.
//
// Bu bileşen, zincir verisinde "IpucuKaranlikta" işaretli şifre ipucunun yazısına eklenir
// (ZincirUygulayici.IpucuYaz). Yazı 3B dünya yazısıdır (TextMesh); ışıktan etkilenmeyen bir
// gölgelendirici kullanır, yani görünürlüğünü tamamen bu bileşenin yazdığı saydamlık belirler.

using UnityEngine;

public class KaranliktaParlayan : MonoBehaviour
{
    private const float BelirmeHizi = 0.5f;    // saniyede: 2 sn'de tam parlaklık
    private const float KaybolmaHizi = 8f;     // ışık yanınca hemen kaybolur

    private const float IpucuMesafesi = 3.5f;  // bu kadar yakından bakınca lamba bir an söner (#83)
    private const float IpucuAcisi = 20f;      // bakış yönüyle yazıya doğru yön arasındaki en büyük açı
    private const float IpucuSonme = 1.8f;     // lambanın sönük kaldığı süre
    private const float IpucuAraligi = 30f;    // iki ipucu arasında en az bu kadar saniye

    private TextMesh yazi;
    private float sonrakiIpucu;                // bir sonraki ipucunun en erken zamanı (Time.time)
    private Color renk;                        // tam parlaklıktaki rengi
    private float gorunurluk;                  // 0: görünmez, 1: tam parlak
    private KafaLambasi lamba;
    private DaireElektrigi elektrik;

    private void Start()
    {
        yazi = GetComponent<TextMesh>();
        if (yazi != null) renk = yazi.color;
        lamba = FindAnyObjectByType<KafaLambasi>();
        // Yazı dairenin içinde bir yerin çocuğu; dairenin elektriği yukarıda, kökte.
        elektrik = GetComponentInParent<DaireElektrigi>();
        Uygula();
    }

    private void Update()
    {
        IpucuVer();
        bool karanlik = (lamba == null || !lamba.Acik) && (elektrik == null || !elektrik.Var);
        // MoveTowards: değeri hedefe doğru en fazla verilen adım kadar yaklaştırır.
        gorunurluk = Mathf.MoveTowards(gorunurluk, karanlik ? 1f : 0f, (karanlik ? BelirmeHizi : KaybolmaHizi) * Time.deltaTime);
        Uygula();
    }

    // İPUCU (#83, not yerine ortam): ipucu notları kalktı (Şahan: oyuncu neyi nerede arayacağını bir nottan
    // okumamalı). Oyuncuya lambayı söndürmeyi düşündürecek bir şey gerekiyordu. Elektriksiz dairede lamba
    // AÇIKKEN yazının duvarına yakından ve dosdoğru bakınca lamba bir an söner (temassızlık gibi, tıkla);
    // o 1,8 saniyelik karanlıkta rakamlar belli belirsiz belirmeye başlar, lamba yanınca kaybolur. Oyuncu ne
    // gördüğünden emin olamaz; lambayı kendisi söndürüp beklerse yazıyı okur. En çok 30 saniyede bir olur.
    private void IpucuVer()
    {
        if (lamba == null || !lamba.Acik || Time.time < sonrakiIpucu) return;
        if (elektrik != null && elektrik.Var) return;
        Camera kamera = Camera.main;
        if (kamera == null) return;
        Vector3 fark = transform.position - kamera.transform.position;
        float mesafe = fark.magnitude;
        if (mesafe > IpucuMesafesi || Vector3.Angle(kamera.transform.forward, fark) > IpucuAcisi) return;
        // Arada duvar ya da eşya var mı? Yazının çarpışması yok; ışın yazıya varmadan bir şeye çarparsa görmüyor.
        // Kamera oyuncunun kendi kapsülünün içinde durur: ışın önce ona çarpar, o yüzden oyuncunun (kameranın
        // kökünün) çarpışmaları atlanır. RaycastAll: yol boyunca değdiği bütün çarpışmaları verir.
        Transform oyuncu = kamera.transform.root;
        foreach (RaycastHit h in Physics.RaycastAll(kamera.transform.position, fark / mesafe, mesafe - 0.03f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(oyuncu)) return;
        sonrakiIpucu = Time.time + IpucuAraligi;
        lamba.Sondur(IpucuSonme);
        Debug.Log("[FOSFOR] Lamba bir an söndü: fosforlu yazı ipucu.");
    }

    private void Uygula()
    {
        if (yazi == null) return;
        yazi.color = new Color(renk.r, renk.g, renk.b, renk.a * gorunurluk);
    }
}
