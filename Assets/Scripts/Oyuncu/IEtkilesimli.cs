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

public interface IEtkilesimli
{
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
