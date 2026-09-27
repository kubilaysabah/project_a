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

    private TextMesh yazi;
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
        bool karanlik = (lamba == null || !lamba.Acik) && (elektrik == null || !elektrik.Var);
        // MoveTowards: değeri hedefe doğru en fazla verilen adım kadar yaklaştırır.
        gorunurluk = Mathf.MoveTowards(gorunurluk, karanlik ? 1f : 0f, (karanlik ? BelirmeHizi : KaybolmaHizi) * Time.deltaTime);
        Uygula();
    }

    private void Uygula()
    {
        if (yazi == null) return;
        yazi.color = new Color(renk.r, renk.g, renk.b, renk.a * gorunurluk);
    }
}
