// KayitDeposu.cs
// Oyunun kayıt ve ayar deposu (#122): PlayerPrefs'in yerine geçen, DOSYAYA yazan küçük bir anahtar-değer kabı.
//
// NEDEN: PlayerPrefs Windows'ta kayıt defterine (registry) yazar. Steam Cloud yalnızca dosya klasörlerini
// eşler; kayıt defterindeki bir kayıt ne buluta çıkar ne Steam Deck'e gider. Bu sınıf aynı çağrıları
// (GetInt, SetString, HasKey, DeleteKey, Save...) sunar ama persistentDataPath altında bir metin dosyasına
// yazar. Kod tarafında değişiklik tek: "PlayerPrefs." yerine "KayitDeposu.Oyun." ya da "KayitDeposu.Ayar.".
//
// İKİ DOSYA:
//   kayit.txt    → ilerleme (kat, kilitler, parçalar, aletler, notlar, fotoğraflar, en yüksek kat). Buluta gider.
//   ayarlar.txt  → ayarlar ve dil (çözünürlük, pencere, kalite, ses...). Bulutta yeri yok: başka bir
//                  bilgisayarın çözünürlüğü bu bilgisayara gelmesin.
// Test yuvası (KayitYuvasi, Claude'un oynatma testleri) ilerleme dosyasının ADINI değiştirir: kayit_test.txt.
// Şahan'ın kayit.txt'sine test hiç dokunmaz. Ayar dosyası yuvaya bakmaz (testler ayarları değiştirip geri yazar).
//
// TAŞIMA: dosya henüz yoksa (ilk açılış) aynı adlı PlayerPrefs anahtarları okunup dosyaya yazılır; eski kayıt
// kaybolmaz. PlayerPrefs silinmez (geri dönüş olsun diye). Editor ve build AYNI persistentDataPath'i kullanır
// (…/LocalLow/<şirket>/<ürün>), yani Editor'deki kayıt build'de de görünür — PlayerPrefs'te ayrıydı.
// Ürün/şirket adı değişince (#1) klasör de değişir; o gün eski klasörden taşımak gerekir.
//
// YAZMA: Set'ler yalnızca sözlüğü değiştirir, Save() dosyayı yazar (PlayerPrefs ile aynı alışkanlık). Yazım
// önce geçici dosyaya, sonra yerine: oyun yazarken çökerse eski dosya bozulmaz.

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KayitDeposu
{
    /// <summary>İlerleme kaydı (kayit.txt; test yuvasında kayit_test.txt). Steam Cloud'a giden dosya.</summary>
    public static readonly KayitDeposu Oyun = new KayitDeposu("kayit", true);
    /// <summary>Ayarlar ve dil (ayarlar.txt). Makineye özel, buluta girmez.</summary>
    public static readonly KayitDeposu Ayar = new KayitDeposu("ayarlar", false);

    private readonly string ad;
    private readonly bool yuvali;             // dosya adı test yuvasına göre değişir mi
    private Dictionary<string, string> sozluk;
    private string yuklenenYol;               // sözlüğün hangi dosyadan okunduğu (yuva değişince yeniden okunur)
    private bool okunamadi;                   // dosya vardı ama okunamadı: üstüne yazma (kayıt korunsun)

    private KayitDeposu(string dosyaAdi, bool yuvaliMi) { ad = dosyaAdi; yuvali = yuvaliMi; }

    /// <summary>Dosyanın tam yolu (yuvaya göre). Belge ve testler için açık.</summary>
    public string Yol
    {
        get { return Path.Combine(Application.persistentDataPath, ad + (yuvali ? KayitYuvasi.Ek : "") + ".txt"); }
    }

    // Sözlük: ilk erişimde (ya da yuva değişince) dosyadan okunur; dosya yoksa PlayerPrefs'ten taşınır.
    private Dictionary<string, string> Sozluk
    {
        get
        {
            string yol = Yol;
            if (sozluk != null && yuklenenYol == yol) return sozluk;
            yuklenenYol = yol;
            if (File.Exists(yol))
            {
                try { sozluk = KayitDosyasiKurali.Oku(File.ReadAllText(yol)); okunamadi = false; }
                catch (System.Exception e)
                {
                    // Dosya var ama okunamıyor (antivirüs/bulut kilidi, izin): BOŞ bir sözlükle devam edilir ama bu
                    // oturumda dosyaya YAZILMAZ; yoksa bir sonraki Save gerçek kaydı boş kayıtla ezerdi.
                    Debug.LogWarning($"[KAYIT] {yol} okunamadı: {e.Message}; bu oturumda kayıt yazılmayacak.");
                    sozluk = new Dictionary<string, string>(); okunamadi = true;
                }
            }
            else
            {
                sozluk = new Dictionary<string, string>();
                PlayerPrefstenTasi();
            }
            return sozluk;
        }
    }

    // ---------- PlayerPrefs ile aynı yüz ----------
    public bool HasKey(string anahtar) { return Sozluk.ContainsKey(anahtar); }
    public string GetString(string anahtar, string varsayilan = "") { string d; return Sozluk.TryGetValue(anahtar, out d) ? d : varsayilan; }
    public int GetInt(string anahtar, int varsayilan = 0) { string d; return Sozluk.TryGetValue(anahtar, out d) ? KayitDosyasiKurali.TamSayi(d, varsayilan) : varsayilan; }
    public float GetFloat(string anahtar, float varsayilan = 0f) { string d; return Sozluk.TryGetValue(anahtar, out d) ? KayitDosyasiKurali.Sayi(d, varsayilan) : varsayilan; }
    public void SetString(string anahtar, string deger) { Sozluk[anahtar] = deger ?? ""; }
    public void SetInt(string anahtar, int deger) { Sozluk[anahtar] = deger.ToString(System.Globalization.CultureInfo.InvariantCulture); }
    public void SetFloat(string anahtar, float deger) { Sozluk[anahtar] = KayitDosyasiKurali.SayiMetni(deger); }
    public void DeleteKey(string anahtar) { Sozluk.Remove(anahtar); }

    /// <summary>Sözlüğü dosyaya yazar: önce geçici dosyaya, sonra yerine (yarım yazılmış kayıt kalmasın).</summary>
    public void Save()
    {
        string yol = Yol;
        var s = Sozluk;   // yuva değiştiyse önce doğru dosya okunsun
        if (okunamadi) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(yol));
            string gecici = yol + ".yeni";
            File.WriteAllText(gecici, KayitDosyasiKurali.Metin(s));
            if (File.Exists(yol)) File.Replace(gecici, yol, null); else File.Move(gecici, yol);
        }
        catch (System.Exception e) { Debug.LogWarning($"[KAYIT] {yol} yazılamadı: {e.Message}"); }
    }

    /// <summary>Dosyayı ve bellekteki sözlüğü siler (test yuvasını temizlemek için).</summary>
    public void DosyayiSil()
    {
        // #142: oyun kaydını yalnızca TEST YUVASI açıkken siler. 2026-10-09'da bir test betiği bunu yuva zaten
        // kapatılmışken çağırdı ve Şahan'ın kayit.txt'si silindi (dosya yokken ilk okuma eski PlayerPrefs'i taşıdı,
        // 6. kattaki kayıt 8. kata döndü). Oyun bu fonksiyonu hiç çağırmaz; yalnız testler çağırır. Ayar dosyası
        // (ayarlar.txt) yuvaya bakmaz, onun silinmesi de istenmez.
        if (this == Oyun && string.IsNullOrEmpty(KayitYuvasi.Ek))
        {
            Debug.LogWarning("[KAYIT] DosyayiSil reddedildi: test yuvası kapalı, gerçek kayıt (kayit.txt) silinmez.");
            return;
        }
        if (this == Ayar)
        {
            Debug.LogWarning("[KAYIT] DosyayiSil reddedildi: ayar dosyası silinmez.");
            return;
        }
        string yol = Yol;
        try { if (File.Exists(yol)) File.Delete(yol); } catch (System.Exception e) { Debug.LogWarning($"[KAYIT] {yol} silinemedi: {e.Message}"); }
        sozluk = null; yuklenenYol = null; okunamadi = false;
    }

    // ---------- PlayerPrefs'ten taşıma (bir kez, dosya yokken) ----------
    // Anahtarlar türleriyle (PlayerPrefs int, float ve string'i ayrı tutar; okurken türü bilmek gerekir).
    private static readonly string[] OyunTamSayilar = { "kayit_kat", "kayit_en_yuksek_kat" };
    private static readonly string[] OyunMetinler = { "kayit_kilitler", "kayit_parcalar", "kayit_fotograflar", "kayit_notlar", "kayit_aletler", "kayit_katbasi_aletler",
        "kayit_katbasi_aletler_1", "kayit_katbasi_aletler_2", "kayit_katbasi_aletler_3", "kayit_katbasi_aletler_4",
        "kayit_katbasi_aletler_5", "kayit_katbasi_aletler_6", "kayit_katbasi_aletler_7", "kayit_katbasi_aletler_8" };
    private static readonly string[] AyarTamSayilar = { "ayar_ters", "ayar_sallanma", "ayar_kalite", "ayar_tamekran", "ayar_vsync", "ayar_cozunurluk_g", "ayar_cozunurluk_y", "ayar_dil" };
    private static readonly string[] AyarSayilar = { "ayar_hassasiyet", "ayar_ses", "ayar_muzik", "ayar_konusma", "ayar_gorus", "ayar_parlaklik" };

    private void PlayerPrefstenTasi()
    {
        // Eski düzende yuva eki anahtarın sonundaydı ("kayit_kat_test"); dosyada ek dosya adında, anahtar düz.
        string ek = yuvali ? KayitYuvasi.Ek : "";
        int sayi = 0;
        if (ad == "kayit")
        {
            foreach (string a in OyunTamSayilar) if (PlayerPrefs.HasKey(a + ek)) { SetInt(a, PlayerPrefs.GetInt(a + ek)); sayi++; }
            foreach (string a in OyunMetinler) if (PlayerPrefs.HasKey(a + ek)) { SetString(a, PlayerPrefs.GetString(a + ek)); sayi++; }
        }
        else
        {
            foreach (string a in AyarTamSayilar) if (PlayerPrefs.HasKey(a)) { SetInt(a, PlayerPrefs.GetInt(a)); sayi++; }
            foreach (string a in AyarSayilar) if (PlayerPrefs.HasKey(a)) { SetFloat(a, PlayerPrefs.GetFloat(a)); sayi++; }
        }
        if (sayi == 0) return;   // taşınacak bir şey yok: dosya ilk kayıtla doğar
        Save();
        Debug.Log($"[KAYIT] PlayerPrefs'ten {sayi} değer {Yol} dosyasına taşındı (#122).");
    }
}
