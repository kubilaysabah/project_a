// YerelYazi.cs
// Sahnedeki bir 3B yazının (TextMesh) metnini tablodan alan bileşen (#15).
// Tabela, takvim, asansör notu gibi dünyaya yerleştirilmiş yazılara takılır.
//
// Yazının kendisi sahnede Türkçe olarak kuruludur (Editor'de de okunabilsin);
// oyun başlayınca bu bileşen metni seçili dile göre yeniden yazar, dil değişince
// de hemen günceller. Hangi yazının çevrileceği tablonun kararıdır (tabela Türkçe
// kalır, asansör notu ve takvim çevrilir); karar değişirse sadece tablo değişir,
// bu dosya ve sahne değişmez.
//
// DİKKAT: Bir anomali kartı bu bileşeni taşıyan bir yazının METNİNİ değiştirirse,
// oyuncu o sırada dil değiştirdiğinde kartın yazısı tablodakiyle ezilir. Bugün
// hiçbir kart bunu yapmıyor (kartlar yalnızca kapı numaralarını değiştiriyor).

using UnityEngine;

// RequireComponent: bu bileşen eklenince TextMesh yoksa Unity onu da ekler;
// TextMesh'i olmayan bir nesneye yanlışlıkla takılmasını engeller.
[RequireComponent(typeof(TextMesh))]
public class YerelYazi : MonoBehaviour
{
    // Tablodaki anahtar, ör. "bina.tabela". Sahne kurucu (GrayboxKurucu) yazar.
    [SerializeField] private string anahtar;

    public string Anahtar { get { return anahtar; } }

    /// <summary>Anahtarı atar (sahne kurulurken kullanılır).</summary>
    public void AnahtariKur(string yeniAnahtar)
    {
        anahtar = yeniAnahtar;
    }

    // Nesne açılınca: dil haberini dinlemeye başla ve metni hemen yaz.
    private void OnEnable()
    {
        Dil.Degisti += Yenile;
        Yenile();
    }

    // Nesne kapanınca (ya da yok edilince): dinlemeyi bırak. Bırakmazsak Dil,
    // yok olmuş bir nesneye haber göndermeye çalışır ve hata verir.
    private void OnDisable()
    {
        Dil.Degisti -= Yenile;
    }

    private void Yenile()
    {
        if (string.IsNullOrEmpty(anahtar)) return;
        GetComponent<TextMesh>().text = Dil.Getir(anahtar);
    }
}
