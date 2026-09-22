// AnomaliOnizleyici.cs
// Seçili anomali kartını Play'e basmadan sahanlığa uygular ve geri alır.
//
// NEDEN GEREKLİ
// 60 anomali kartı yazılacak. Her birini görmek için Play'e basıp o kat
// gelene kadar merdiven çıkmak dakikalar sürer ve kartın rastgele seçilmesini
// beklemek gerekir. Bu araçla kartı seçip menüye basıyorsunuz, anomali
// anında sahanlıkta beliriyor. Bir kartın açısını veya konumunu oturtmak
// yirmi denemede olur; bu araç olmadan o yirmi deneme bir saati bulur.
//
// Bu dosya Editor klasöründedir, oyuna girmez.

using UnityEngine;
using UnityEditor;

public static class AnomaliOnizleyici
{
    // Uygulayıcıyı menü çağrıları ARASINDA tutmamız gerekiyor: biri uyguluyor,
    // diğeri geri alıyor. "static" alan bu yüzden.
    //
    // Ama dikkat: Unity kodu yeniden derlediğinde bu alan sıfırlanır ve
    // defter kaybolur. O durumda geri alma, graybox'ı baştan kurarak yapılır;
    // aşağıda bu yedek yol var.
    private static AnomaliUygulayici uygulayici;
    private static string uygulananKartAdi;

    private const string SahanlikAdi = "Sahanlik_A";

    // ---------- ÖNİZLE ----------

    [MenuItem("Huzur/Anomali/Seçili Kartı Önizle")]
    public static void Onizle()
    {
        AnomaliKarti kart = Selection.activeObject as AnomaliKarti;

        if (kart == null)
        {
            Debug.LogWarning("[ÖNİZLEME] Önce Project penceresinden bir anomali kartı seçin.");
            return;
        }

        GameObject sahanlik = GameObject.Find(SahanlikAdi);

        if (sahanlik == null)
        {
            Debug.LogWarning($"[ÖNİZLEME] Sahnede '{SahanlikAdi}' yok. " +
                             "Huzur → Graybox Sahanlığı Yeniden Kur ile kurun.");
            return;
        }

        // Önceki önizleme varsa temizle.
        if (uygulayici != null) uygulayici.GeriAl();

        uygulayici = new AnomaliUygulayici();
        uygulayici.Uygula(kart, sahanlik.transform);
        uygulananKartAdi = kart.Kimlik;

        Debug.Log($"[ÖNİZLEME] '{kart.Kimlik}' uygulandı ({kart.Katman}, zorluk {kart.Zorluk}). " +
                  "SAHNEYİ KAYDETMEYİN — kaydederseniz anomali kalıcı olur. " +
                  "Geri almak için: Huzur → Anomali → Önizlemeyi Geri Al");
    }

    // [MenuItem(yol, true)] ikinci parametresi true olan fonksiyon,
    // menü satırının tıklanabilir olup olmadığına karar verir.
    // Kart seçili değilken satır soluk görünür.
    [MenuItem("Huzur/Anomali/Seçili Kartı Önizle", true)]
    private static bool OnizleGecerliMi()
    {
        return Selection.activeObject is AnomaliKarti;
    }

    // ---------- GERİ AL ----------

    [MenuItem("Huzur/Anomali/Önizlemeyi Geri Al")]
    public static void GeriAl()
    {
        if (uygulayici != null)
        {
            uygulayici.GeriAl();
            uygulayici = null;

            Debug.Log($"[ÖNİZLEME] '{uygulananKartAdi}' geri alındı, sahanlık temiz.");
            uygulananKartAdi = null;
            return;
        }

        // YEDEK YOL. Kod yeniden derlendiyse defter kaybolmuştur ve neyin
        // değiştiğini artık bilemeyiz. Bu durumda tek güvenli çözüm
        // sahanlığı baştan kurmaktır; graybox zaten koddan üretiliyor.
        Debug.LogWarning("[ÖNİZLEME] Geri alma defteri yok (kod yeniden derlenmiş olabilir). " +
                         "Sahanlık baştan kuruluyor.");

        GrayboxKurucu.YenidenKur();
        OyuncuKurucu.YenidenKur();
    }
}
