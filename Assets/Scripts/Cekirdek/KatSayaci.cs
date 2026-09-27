// KatSayaci.cs
// Kat sayacı: oyuncunun hangi katta olduğu.
//
// Bu dosyada Unity'ye ait TEK BİR SATIR yok. MonoBehaviour değil, GameObject bilmiyor,
// sahneden haberi yok. Sebebi şu: bu mantığı Unity'yi hiç açmadan, saniyeler içinde
// test edebilmek istiyoruz.
//
// #59 (Şahan, 2026-09-27): eski oyunun "anomali varsa geri dön, yoksa yukarı çık; yanlış
// karar 1. kata ceza" kuralı tamamen kalktı. Artık bir üst kata yalnızca kat kapısı açılıp
// merdivenden çıkılarak geçilir; aşağı inmek katı değiştirmez (KatYoneticisi bunu sayar).
// Bu sınıf yalnızca katı tutar, kayıttan yükler ve bir üst kata geçirir.

// ---------- ENUM NEDİR ----------
// "enum", sınırlı sayıdaki seçeneğe isim veren bir tiptir. Kararı bir sayıyla (0 = yukarı,
// 1 = geri) de tutabilirdik; ama o zaman 5 yazmak da mümkün olurdu. enum ile derleyici bize
// garanti verir: bu değişken SADECE bu iki değerden birini alabilir.

/// <summary>Oyuncunun merdivende bir kat boyu yol alırken gittiği yön (KatIlerlemeTakipcisi söyler).</summary>
public enum Karar
{
    Yukari,   // bir kat yukarı çıktı
    Geri      // bir kat aşağı indi
}

/// <summary>Kat sayacı. Unity'den bağımsızdır.</summary>
public class KatSayaci
{
    // ---------- SABİTLER ----------

    /// <summary>Son kat: bunun üstünde yalnızca çatı var.</summary>
    public const int CatiKati = 8;

    /// <summary>Oyunun başladığı kat.</summary>
    public const int BaslangicKati = 1;

    // ---------- DURUM ----------
    // "get;" = dışarıdan OKUNABİLİR, "private set;" = yalnızca BU SINIFIN İÇİNDEN yazılabilir.
    // Böylece başka bir kod yanlışlıkla "sayac.MevcutKat = 8;" yazıp oyunu bozamaz.

    /// <summary>Oyuncunun şu an bulunduğu kat (1 ile 8 arası).</summary>
    public int MevcutKat { get; private set; } = BaslangicKati;

    // ---------- KAYITTAN YÜKLE (#52) ----------

    /// <summary>
    /// Oyunu kaldığı kattan sürdürür. Aralık dışı bir değer sınıra çekilir; bozuk bir kayıt
    /// dosyası oyunu 9. kata ya da 0. kata düşürmesin.
    /// </summary>
    public void Yukle(int kat)
    {
        if (kat < BaslangicKati) kat = BaslangicKati;
        if (kat > CatiKati) kat = CatiKati;
        MevcutKat = kat;
    }

    // ---------- İLERLE ----------

    /// <summary>
    /// Bir üst kata geçildi. Geçildiyse true; zaten son kattaysa false (8. katın üstü kat değil,
    /// çatıdır: çatıya çatı kapısından çıkılır).
    /// </summary>
    public bool Ilerle()
    {
        if (MevcutKat >= CatiKati) return false;
        MevcutKat++;
        return true;
    }
}
