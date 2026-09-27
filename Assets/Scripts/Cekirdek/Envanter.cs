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
    OdaAnahtari,    // dairedeki tek bir kilidi açar; hangisi olduğu Kimlik'te (#54)
    Sigorta,        // sigorta kutusuna takılır (#54)
    Tornavida,      // menteşe söker (#54)
    Not,            // okunur, panoya asılır (#56)
    Parca           // yırtık çizimin bir parçası (#63): ele girmez, kapıcı biriktirir; hepsi
                    // bulununca çizim birleşir. SONA eklendi: dosyalar türü sayı olarak saklar.
}

public class Envanter
{
    /// <summary>Eldeki eşya; Yok = el boş.</summary>
    public EsyaTuru Tasinan { get; private set; } = EsyaTuru.Yok;

    /// <summary>Eşyanın hangi kata ait olduğu (kat anahtarı için); diğerlerinde 0.</summary>
    public int Kat { get; private set; }

    /// <summary>Eşyanın zincirdeki kimliği ("odaA_anahtari"); kimliksiz eşyalarda boş.
    /// Oda anahtarı yalnızca kimliği kilidin istediği kimlik olan kilidi açar.</summary>
    public string Kimlik { get; private set; } = "";

    public bool ElBos { get { return Tasinan == EsyaTuru.Yok; } }

    /// <summary>
    /// Eşyayı almaya çalışır. El doluysa alınmaz ve false döner: oyuncu önce elindekini
    /// kullanmalı. (Tek eşya kuralı burada, tek yerde uygulanır.)
    /// </summary>
    public bool Al(EsyaTuru tur, int kat = 0, string kimlik = "")
    {
        if (tur == EsyaTuru.Yok || !ElBos) return false;
        Tasinan = tur;
        Kat = kat;
        Kimlik = kimlik ?? "";
        return true;
    }

    /// <summary>Eldeki eşya bu tür (ve bu kat) mı? Kilitler "anahtarın var mı" diye bunu sorar.</summary>
    public bool Tasiyor(EsyaTuru tur, int kat = 0)
    {
        return Tasinan == tur && (kat == 0 || Kat == kat);
    }

    /// <summary>Eldeki eşyanın kimliği bu mu? (Oda anahtarı, sigorta, tornavida zincirde kimlikle aranır.)</summary>
    public bool KimligiTasiyor(string kimlik)
    {
        return !ElBos && !string.IsNullOrEmpty(kimlik) && Kimlik == kimlik;
    }

    /// <summary>Eşya kullanıldı: el boşalır. Kullanılacak eşya elde değilse false döner.</summary>
    public bool Kullan(EsyaTuru tur, int kat = 0)
    {
        if (!Tasiyor(tur, kat)) return false;
        Bosalt();
        return true;
    }

    /// <summary>Kimliği verilen eşya elde ise kullanır (el boşalır); değilse false.</summary>
    public bool KimlikleKullan(string kimlik)
    {
        if (!KimligiTasiyor(kimlik)) return false;
        Bosalt();
        return true;
    }

    /// <summary>Eli boşaltır (ceza, yeniden başlama).</summary>
    public void Bosalt()
    {
        Tasinan = EsyaTuru.Yok;
        Kat = 0;
        Kimlik = "";
    }
}
