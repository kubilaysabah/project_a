// IEtkilesimli.cs
// "Oyuncu bununla etkileşime girebilir" sözleşmesi.
//
// NEDEN SÖZLEŞME
// Şu an tek etkileşim çöp poşeti almak. Ama ileride başkaları gelecek:
// kapı zilini çalmak, ampule dokunmak, notu okumak, çatı kapısını açmak.
// Bunların her biri için oyuncuya ayrı kod yazmak istemiyoruz.
//
// Bu sözleşmeyi imzalayan her nesne, oyuncunun etkileşim sistemiyle
// kendiliğinden çalışır. Oyuncu tarafında hiçbir şey değişmez; sadece yeni
// nesne sözleşmeyi imzalar. Anomali kartlarındaki mantığın aynısı:
// yeni içerik eklemek, var olan kodu değiştirmeyi gerektirmemeli.

using UnityEngine;

/// <summary>
/// Nişangâhta gösterilecek küçük işaret (#104): oyuncu basmadan önce E'nin ne yapacağını görsün.
/// Yazı yok (#72 kuralı); yalnızca bir simge. Eylem türleri sınırlı tutuldu ki simgeler ezberlensin.
/// </summary>
public enum NisanSimgesi
{
    El,        // al, koy, tak, bas, çal: elle yapılan her şey
    Goz,       // oku, bak
    Ac,        // kapalı çekmece / kapak / kapı açılacak
    Kapat,     // açık çekmece / kapak / kapı kapanacak
    Kilitli,   // kilitli ve şu an açılamıyor (anahtar, alet, parça eksik)
    Anahtar,   // eldeki anahtarla / tamamlanan parçalarla açılacak
    Kol,       // çekilecek kol, çevrilecek çark
    Saklan,    // dolaba saklan
}

public interface IEtkilesimli
{
    /// <summary>
    /// Nişangâhtaki simge. Varsayılanı EL: imzalayan çoğu nesne için doğru olan bu. Yalnızca farklı
    /// bir simge isteyen nesne (çekmece, kapı, kilit, kol, dolap, not) kendi değerini yazar.
    ///
    /// C# NOTU: Bir arayüz (interface) normalde yalnızca "şu üyeler olacak" der, gövde yazmaz. C# 8'den
    /// beri bir arayüz üyesine VARSAYILAN gövde yazılabilir ("varsayılan arayüz üyesi"). Sözleşmeyi
    /// imzalayan 25 sınıfın her birine aynı satırı eklemek yerine burada bir kez yazılır; isteyen ezer.
    /// Dikkat: varsayılan üyeye sınıfın kendi türü üzerinden değil, arayüz türü üzerinden erişilir
    /// (Etkilesimci zaten hedefi IEtkilesimli olarak tutar, o yüzden sorun çıkmaz).
    /// </summary>
    NisanSimgesi Simge { get { return NisanSimgesi.El; } }

    /// <summary>
    /// Ekranda görünecek eylem adı. "TOPLA", "ÇAL", "OKU" gibi.
    /// Tuş adını buraya yazmayın; onu etkileşim sistemi ekliyor.
    /// </summary>
    string Ipucu { get; }

    /// <summary>
    /// Şu anda etkileşime girilebilir mi? Alınmış bir poşet false döner;
    /// ipucu da gösterilmez.
    /// </summary>
    bool EtkilesimeAcik { get; }

    /// <summary>
    /// Oyuncu tuşa bastığında çağrılır.
    /// </summary>
    /// <param name="oyuncu">Etkileşimi başlatan oyuncu nesnesi.</param>
    void Etkilesim(GameObject oyuncu);
}
