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
        // Oyun sırasında koddan eklenen yazı (#66: isim kartı, tabak) zaten açıktır; OnEnable
        // anahtarsız çalışıp geçti. Metni burada hemen yaz.
        if (isActiveAndEnabled) Yenile();
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

    private Font ilkYaziTipi;          // sahnedeki yazı tipi (Liberation Sans) ve malzemesi: CJK'den dönünce geri
    private Material ilkMalzeme;

    private void Yenile()
    {
        if (string.IsNullOrEmpty(anahtar)) return;
        TextMesh tm = GetComponent<TextMesh>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (ilkYaziTipi == null) { ilkYaziTipi = tm.font; if (mr != null) ilkMalzeme = mr.sharedMaterial; }
        // #123: Çince/Japonca harfleri gömülü yazı tipinde yok; yazı tipiyle birlikte harf dokusunu taşıyan
        // malzeme de değişir (Yazi3B gölgelendiricisi dokuyu malzemeden okur). Öbür dillerde eski ikili.
        bool cjk = YaziTipi.CjkMi(Dil.Etkin) && YaziTipi.Cjk != null && YaziTipi.CjkMalzeme != null;
        tm.font = cjk ? YaziTipi.Cjk : ilkYaziTipi;
        if (mr != null && ilkMalzeme != null) mr.sharedMaterial = cjk ? YaziTipi.CjkMalzeme : ilkMalzeme;
        tm.text = Dil.Getir(anahtar);
    }
}
