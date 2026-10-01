// SesMalzemesi.cs
// Bir nesnenin sesi, nesnenin kendisine uymalı (#99).
//
// Şahan'ın sorusu (2026-10-01): "sesin bağlı olduğu şey ile ses uyumlu mu?" Oyundaki bütün ses
// bağlantıları döküldüğünde çekmecelerin, buzdolabının, kavanoz kapağının ve çelik dolabın hep
// aynı sesi, ODA KAPISI gıcırtısını çaldığı, elde alınan her eşyanın (tornavida, tabak, kâğıt)
// anahtar şıkırtısı çıkardığı görüldü. Kulak bunu hemen yakalar: göz bir çekmece görür, kulak bir
// kapı duyar ve dünya "sahte" hissettirir.
//
// Bu dosya, bir nesnenin ADINDAN ve nasıl hareket ettiğinden hangi sesi çalacağını söyleyen kuraldır.
// Ad yolu: nesnenin kendi adı ve ebeveynlerinin adları ("Gardirop_Cemal/Govde/Kapi_1" gibi), küçük
// harfe çevrilmiş. Kural sahneyi bilmez (Unity'siz, testli); sesin kendisini sahne tarafı yükler.
//
// Neden adlardan? Eşyaların adları zaten sözleşme (kat dosyaları, kartlar bunlarla adresliyor) ve
// her eşya Blender betiğinde kendi adıyla doğuyor. Her şablona ayrı bir "malzeme" alanı yazmak 100'den
// fazla eşyaya dokunmak demekti; bir anahtar kelime listesi aynı işi tek yerde yapar.

/// <summary>Açılan bir parçanın (çekmece, kapak, kapı) ses türü.</summary>
public enum AcilirSesTuru
{
    AhsapKapak,    // dolap kapağı, sandık kapağı, komodin kapısı: küçük menteşe gıcırtısı
    Cekmece,       // ahşap ray üstünde kayan çekmece
    Buzdolabi,     // lastik contanın ayrılması + kapı rafındaki şişeler
    Metal,         // sac dolap, çelik dolap, alet kutusu, sigorta kutusu kapağı
    Kavanoz,       // kavanozun vidalı metal kapağı
    AgirMobilya,   // itilerek kayan ağır gardırop
}

/// <summary>Elde alınan eşyanın ses türü.</summary>
public enum AlmaSesTuru
{
    Anahtar,   // anahtar, kırık anahtar parçası: şıkırtı
    Metal,     // tornavida, levye, kaydırıcı: metal tıngırtı
    Kagit,     // not, çizim parçası, fotoğraf: kâğıt hışırtısı
    Seramik,   // tabak: seramik tınlaması
    Kucuk,     // sigorta: küçük porselen tık
}

public static class SesMalzemesi
{
    // Metal eşyaların adlarında geçen parçalar. Ahşap sayılmayan her şey burada olmalı; listede
    // olmayan bir dolap varsayılan olarak ahşaptır (90'lar dairesinin dolaplarının çoğu ahşap/sunta).
    private static readonly string[] MetalKelimeler =
    {
        "gardirop_k7", "gardirop_cemal",   // Cemal'in yeşil-gri çelik dolabı (şablon ve sahnedeki adı)
        "gardirop_k8", "gardirop_mudur",   // Yönetim'in bej metal ofis dolabı
        "dosya_dolabi",     // 8. kat çekmeceli dosya dolabı
        "arsiv_dolabi",
        "sigorta_kutusu",   // sac sigorta kutusu ve kapağı
        "alet_cantasi", "alet_kutusu",
        "ilac_dolabi",      // kızılhaçlı metal ilaç dolabı
        "banyo_dolabi_k5",  // aynalı sac banyo dolabı
        "tel_dolap",
        "teneke",           // düğme tenekesi, bisküvi tenekesi
        "kasa_",            // çelik kasa
        "posta_kutu",
    };

    /// <summary>
    /// Açılan bir parçanın ses türü. adYolu: parça ve ebeveynlerinin adları (herhangi bir ayırıcıyla);
    /// kayar: parça dönmüyor, kayıyor mu (çekmece, itilen dolap).
    /// </summary>
    public static AcilirSesTuru AcilirTuru(string adYolu, bool kayar)
    {
        string a = (adYolu ?? "").ToLowerInvariant();
        // Sıra önemli: en belirgin olandan genele. "Buzdolabi" adında "dolab" da geçer.
        if (a.Contains("buzdolab")) return AcilirSesTuru.Buzdolabi;
        // Kavanoz kapağı ve lavabo sifonu: ikisi de vidalı, döndürülerek açılır (burgu gıcırtısı + pop).
        if (a.Contains("kavanoz") || a.Contains("sifon")) return AcilirSesTuru.Kavanoz;
        // Kayan bir gardırop: itilerek yer değiştiren ağır mobilya (5. kat). Gardırobun çekmecesi değil.
        if (kayar && a.Contains("gardirop") && !a.Contains("cekmece")) return AcilirSesTuru.AgirMobilya;
        if (MetalMi(a)) return AcilirSesTuru.Metal;
        if (kayar || a.Contains("cekmece")) return AcilirSesTuru.Cekmece;
        return AcilirSesTuru.AhsapKapak;
    }

    /// <summary>Ad yolundaki eşya metal mi (saklanma yerinin kapı sesi de buna bakar)?</summary>
    public static bool MetalMi(string adYolu)
    {
        string a = (adYolu ?? "").ToLowerInvariant();
        foreach (string k in MetalKelimeler) if (a.Contains(k)) return true;
        return false;
    }

    /// <summary>Elde alınan eşyanın ses türü, eşyanın türünden.</summary>
    public static AlmaSesTuru AlmaTuru(EsyaTuru tur)
    {
        switch (tur)
        {
            case EsyaTuru.Tornavida:
            case EsyaTuru.Levye:
            case EsyaTuru.Kaydirici:
                return AlmaSesTuru.Metal;
            case EsyaTuru.Not:
            case EsyaTuru.Parca:
                return AlmaSesTuru.Kagit;
            case EsyaTuru.Tabak:
                return AlmaSesTuru.Seramik;
            case EsyaTuru.Sigorta:
                return AlmaSesTuru.Kucuk;
            default:   // kat anahtarı, oda anahtarı, kırık anahtar parçası
                return AlmaSesTuru.Anahtar;
        }
    }

    /// <summary>Açılır ses türünün ses dosyası (Resources/Sesler altında, uzantısız).</summary>
    public static string AcilmaDosyasi(AcilirSesTuru t)
    {
        switch (t)
        {
            case AcilirSesTuru.Cekmece: return "Sesler/Cekmece_Acilma";
            case AcilirSesTuru.Buzdolabi: return "Sesler/Buzdolabi_Acilma";
            case AcilirSesTuru.Metal: return "Sesler/Metal_Kapak_Acilma";
            case AcilirSesTuru.Kavanoz: return "Sesler/Kavanoz_Acilma";
            case AcilirSesTuru.AgirMobilya: return "Sesler/Agir_Surtunme";
            default: return "Sesler/Kapak_Acilma";
        }
    }

    /// <summary>Kilitliyken çekiştirilen parçanın sesi.</summary>
    public static string KilitliDosyasi(AcilirSesTuru t)
    {
        switch (t)
        {
            case AcilirSesTuru.Kavanoz: return "Sesler/Cam_Tikirti";
            case AcilirSesTuru.AgirMobilya: return "Sesler/Agir_Surtunme";
            default: return "Sesler/Cekmece_Kilitli";
        }
    }

    /// <summary>Alma ses türünün dosyası.</summary>
    public static string AlmaDosyasi(AlmaSesTuru t)
    {
        switch (t)
        {
            case AlmaSesTuru.Metal: return "Sesler/Al_Metal";
            case AlmaSesTuru.Kagit: return "Sesler/Al_Kagit";
            case AlmaSesTuru.Seramik: return "Sesler/Al_Seramik";
            case AlmaSesTuru.Kucuk: return "Sesler/Al_Kucuk";
            default: return "Sesler/Anahtar_Al";
        }
    }
}
