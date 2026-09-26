// Envanter.cs
// Kapıcının elinde taşıdığı TEK eşya (#52, #54).
//
// Tasarım (Docs/hikaye.md §4): envanter ekranı yok; elde en fazla bir şey taşınır (kat
// anahtarı, sigorta, tornavida, not). Eldeki eşya köşede küçük bir simgeyle görünür.
// Tek eşya, oyunu No70 gibi "doğru eşyayı bul, doğru yerde kullan" ritmine sokar ve
// oyuncuyu eşya listesiyle uğraştırmaz.
//
// Bu dosya yalnızca kuralı tutar: ne taşınıyor, alınabilir mi, kullanılınca ne olur.
// Unity'ye ait tek satır yok; test edilir. Sahnedeki köprü Oyuncu/Cep.cs.

// enum: sabit bir seçenek listesi. Yeni bir eşya türü eklemek buraya bir satır eklemektir.
public enum EsyaTuru
{
    Yok,
    KatAnahtari,    // kilitli kat kapısını açar (#52)
    Sigorta,        // sigorta kutusuna takılır (#54)
    Tornavida,      // menteşe söker (#54)
    Not             // okunur, panoya asılır (#56)
}

public class Envanter
{
    /// <summary>Eldeki eşya; Yok = el boş.</summary>
    public EsyaTuru Tasinan { get; private set; } = EsyaTuru.Yok;

    /// <summary>Eşyanın hangi kata ait olduğu (kat anahtarı için); diğerlerinde 0.</summary>
    public int Kat { get; private set; }

    public bool ElBos { get { return Tasinan == EsyaTuru.Yok; } }

    /// <summary>
    /// Eşyayı almaya çalışır. El doluysa alınmaz ve false döner: oyuncu önce elindekini
    /// kullanmalı. (Tek eşya kuralı burada, tek yerde uygulanır.)
    /// </summary>
    public bool Al(EsyaTuru tur, int kat = 0)
    {
        if (tur == EsyaTuru.Yok || !ElBos) return false;
        Tasinan = tur;
        Kat = kat;
        return true;
    }

    /// <summary>Eldeki eşya bu tür (ve bu kat) mı? Kilitler "anahtarın var mı" diye bunu sorar.</summary>
    public bool Tasiyor(EsyaTuru tur, int kat = 0)
    {
        return Tasinan == tur && (kat == 0 || Kat == kat);
    }

    /// <summary>Eşya kullanıldı: el boşalır. Kullanılacak eşya elde değilse false döner.</summary>
    public bool Kullan(EsyaTuru tur, int kat = 0)
    {
        if (!Tasiyor(tur, kat)) return false;
        Tasinan = EsyaTuru.Yok;
        Kat = 0;
        return true;
    }

    /// <summary>Eli boşaltır (ceza, yeniden başlama).</summary>
    public void Bosalt()
    {
        Tasinan = EsyaTuru.Yok;
        Kat = 0;
    }
}
