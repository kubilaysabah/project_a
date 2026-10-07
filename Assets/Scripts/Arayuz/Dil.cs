// Dil.cs
// Oyunun her yerinden çağrılan yerelleştirme kapısı (#15).
//
//     Dil.Getir("ipucu.topla")   →  Türkçe seçiliyse "TOPLA", İngilizce ise "PICK UP"
//
// Tabloyu okuyan kural (MetinTablosu) saf C#; bu sınıf onun Unity tarafındaki
// eşi: dosyayı Resources'tan yükler, hangi dilin seçili olduğunu bilir, seçimi
// hatırlar ve dil değişince onu dinleyen herkese haber verir.
//
// "static class": nesnesi oluşturulmayan, tek kopyası olan sınıf. Oyunun her
// yerinden Dil.Getir(...) diye doğrudan çağrılır; sahnede bir nesneye takılması,
// bir yerden bulunması gerekmez. Oyunda tek bir "seçili dil" olduğu için uygun.
//
// DİL NASIL SEÇİLİYOR
//   1. Oyuncu daha önce menüden bir dil seçtiyse o (PlayerPrefs'te saklı).
//   2. Seçmediyse Windows'un dili: Türkçe ise Türkçe, değilse İngilizce.
// PlayerPrefs: Unity'nin küçük ayarları bilgisayarda sakladığı yer (Windows'ta
// kayıt defteri). Oyun kapanınca silinmez; bir sonraki açılışta yerinde durur.

using System;
using UnityEngine;

public static class Dil
{
    // Ayar dosyasında (KayitDeposu.Ayar, #122) seçili dilin saklandığı adın adı.
    private const string KayitAdi = "ayar_dil";

    private static MetinTablosu tablo;
    private static DilKodu etkin;
    private static bool hazir;   // tablo yüklendi mi (ilk çağrıda yüklenir)

    // ---------- DİL DEĞİŞTİ HABERİ ----------
    // "event": bir sınıfın "şu oldu" diye duyuru yapabilmesini sağlar. Bir radyo
    // yayını gibi: dinlemek isteyen frekansı açar ("Dil.Degisti += Yenile;"),
    // yayın yapıldığında ("Degisti?.Invoke()") o an dinleyen herkes duyar; artık
    // dinlemek istemeyen kapatır ("Dil.Degisti -= Yenile;").
    // "Action": parametre almayan, değer döndürmeyen bir fonksiyonun türü. Yani
    // dinleyiciler "void Yenile()" biçiminde bir fonksiyon verir.
    // Duyuran taraf kimin dinlediğini bilmez; tabela da, not da, menü de kendi
    // kendine abone olur. Yeni bir yazı eklemek Dil'de hiçbir değişiklik istemez.
    public static event Action Degisti;

    /// <summary>Şu an seçili dil.</summary>
    public static DilKodu Etkin
    {
        get { Hazirla(); return etkin; }
    }

    /// <summary>Anahtarın seçili dildeki metni.</summary>
    public static string Getir(string anahtar)
    {
        Hazirla();
        return tablo.Getir(anahtar, etkin);
    }

    /// <summary>Anahtarın BELİRLİ bir dildeki metni (Editor araçları sahneyi Türkçe kurar).</summary>
    public static string Getir(string anahtar, DilKodu dil)
    {
        Hazirla();
        return tablo.Getir(anahtar, dil);
    }

    /// <summary>Dili değiştirir, seçimi saklar ve dinleyenlere haber verir.</summary>
    public static void Ayarla(DilKodu yeni)
    {
        Hazirla();
        if (yeni == etkin) return;   // zaten o dil: kimseyi boşuna uğraştırma

        etkin = yeni;
        KayitDeposu.Ayar.SetInt(KayitAdi, (int)yeni);   // enum sayı olarak saklanır (TR=0, EN=1)
        KayitDeposu.Ayar.Save();                        // hemen diske yaz; oyun çökse de kalsın

        // "?." : dinleyen kimse yoksa Degisti boştur (null); o zaman hiçbir şey yapma.
        Degisti?.Invoke();
    }

    /// <summary>Sıradaki dile geçer (TR → EN → TR). Menüdeki düğme bunu çağırır.</summary>
    public static void Sonraki()
    {
        // % : bölümden kalan. (0+1)%2 = 1, (1+1)%2 = 0: sona gelince başa döner.
        Ayarla((DilKodu)(((int)Etkin + 1) % MetinTablosu.DilSayisi));
    }

    // ---------- YÜKLEME ----------
    private static void Hazirla()
    {
        if (hazir) return;
        hazir = true;

        // Resources.Load: "Resources" klasöründeki bir dosyayı adıyla (uzantısız) yükler.
        // TextAsset: Unity'nin metin dosyası türü; .text içeriği bir string olarak verir.
        TextAsset dosya = Resources.Load<TextAsset>("Metinler");
        if (dosya == null)
            Debug.LogError("[Dil] Assets/Resources/Metinler.txt bulunamadı; bütün yazılar #anahtar# görünecek.");

        tablo = MetinTablosu.Oku(dosya != null ? dosya.text : "");

        // Tabloda bir yazım hatası varsa oyunu durdurmadan Console'a yaz.
        foreach (string hata in tablo.Hatalar) Debug.LogWarning("[Dil] Metinler.txt, " + hata);

        if (KayitDeposu.Ayar.HasKey(KayitAdi))
        {
            // Clamp: değeri iki sınır arasına sıkıştırır. Saklı değer bozulmuşsa
            // (ör. ileride kaldırılmış bir dilin numarası) var olmayan bir dile düşmeyelim.
            etkin = (DilKodu)Mathf.Clamp(KayitDeposu.Ayar.GetInt(KayitAdi), 0, MetinTablosu.DilSayisi - 1);
        }
        else
        {
            // Application.systemLanguage: Windows'un dili ("Turkish", "English"...).
            etkin = DilSecimi.SistemDilinden(Application.systemLanguage.ToString());
        }
    }

    // ---------- OYUN BAŞINDA SIFIRLA ----------
    // Static değişkenler, Unity'nin "Play'e basınca kodu yeniden yükleme" ayarı
    // kapalıysa bir önceki oynatmadan kalır: eski dinleyiciler (artık yok edilmiş
    // nesneler) listede durur, tablo da dosya düzenlendikten sonra eski kalır.
    // Bu özellik, fonksiyonu her oynatmanın en başında Unity'ye çağırtır.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla()
    {
        hazir = false;
        Degisti = null;
    }
}
