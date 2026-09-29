// KatZinciriVarligi.cs
// Bir katın içerik dosyası (#54, #55, #62, #63): kilit zinciri, notlar, dekor, kendiliğinden çalan
// eşyalar ve yırtık çizimin parçaları.
//
// ScriptableObject: sahnedeki bir nesneye değil, projedeki bir DOSYAYA ait veri kabı
// (anomali kartları da böyle). Zincirin kendisi (KilitZinciri) Unity'siz, düz C# ve testli;
// notlar ve dekor ise model şablonlarına (prefab) başvurduğu için burada, Unity tarafında.
// Her katın bir dosyası var (Assets/Zincirler/Kat_1.asset ...); geliştirme için beş kilit
// türünü birden kullanan bir test zinciri de duruyor (Zincir_Test.asset).

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dairede bir şeyin yeri. İki yolla yazılır:
///   Yol dolu : bir çapa nesnesine göre (dairenin kökünden yolu, "Esyalar/Tezgah"); X, Y, Z o
///              nesnenin kendi eksenlerinde, Donus onun y ekseninde (derece). Eşyanın yanına
///              konacak şeyler için (kavanozun yanındaki not).
///   Yol boş  : planın eksenlerinde: X = u, Z = v, Y yükseklik (DairePlani); OnU/OnV nesnenin ön
///              yüzünün baktığı plan yönü. Odaya serpiştirilen şeyler için (koliler).
/// Yatik: nesne yatırılır (duvar kâğıdı gibi dik duran bir not masaya konunca).
/// </summary>
[System.Serializable]
public class KatYerlesimi
{
    public string Yol = "";
    public float X, Y, Z;
    public float Donus;
    public float OnU, OnV = 1f;
    public bool Yatik;
}

// ": KatYerlesimi" (kalıtım): KatNotu, KatYerlesimi'nin bütün alanlarını (Yol, X, Y, Z...) aynen
// taşır ve üstüne kendi alanını ekler. Yer yazma kuralı iki sınıfta tekrar edilmesin diye.
[System.Serializable]
public class KatNotu : KatYerlesimi
{
    public string Metin;          // metin tablosundaki anahtar ("not.kat1_tahliye")
}

[System.Serializable]
public class KatSusu : KatYerlesimi
{
    public GameObject Sablon;     // koyulacak modelin şablonu (koli...)
    public string Metin;          // şablonun "Yazi" çocuğuna yazılacak metin anahtarı (isim kartı, #66); boş: yok
}

/// <summary>Bir zincir eşyasının üstüne yazılacak metin (#66: tabaktaki isim).</summary>
[System.Serializable]
public class EsyaYazisi
{
    public string Kimlik;         // eşyanın zincirdeki kimliği
    public string Metin;          // metin tablosu anahtarı
    public float Yukseklik = 0.02f;
    public float Boy = 0.0019f;       // 0,0012 masanın başından okunmuyordu
}

/// <summary>
/// Katta kendiliğinden çalan bir eşya (#62): dairedeki bir mobilyayı (radyo) "çalan eşya" yapar.
/// Yol: eşyanın dairenin kökünden yolu; Oda: bulunduğu oda (kapısı kapalıyken ses boğulur);
/// Gecikme: oyuncu daireye girdikten kaç saniye sonra çalmaya başlar.
/// </summary>
[System.Serializable]
public class KatSesi
{
    public string Yol;
    public string Oda;
    public AudioClip Klip;
    public float Gecikme = 5f;
    [Range(0f, 1f)] public float Ses = 0.8f;
    // #65: oyuncu E ile açıp kapatabilir mi (radyo evet; damlayan musluk hayır).
    public bool Kapatilabilir = true;
    // #65: bu kilit çözülünce ses kalıcı olarak susar (vana kapanınca damla durur). Boş: susmaz.
    public string SusturanKilit = "";
}

/// <summary>
/// Yırtık bir çizimin bir parçası (#63). Kimlik: zincirdeki Parca eşyasının kimliği; Doku: tam
/// resim boyunda, yalnızca bu parçanın bölgesi opak doku; Bolge: o bölgenin dokudaki yeri (UV,
/// 0-1, sol alttan). Dünyadaki kâğıt parçası yalnızca Bolge'yi gösterir; yakın bakış ekranı
/// bütün dokuyu çizer (parçalar üst üste birleşir).
/// </summary>
[System.Serializable]
public class KatParcasi
{
    public string Kimlik;
    public Texture2D Doku;
    public Rect Bolge;
}

/// <summary>Kat açılınca aralık ya da açık başlayan bir parça (#83: boşaltılmış buzdolabının kapısı).</summary>
[System.Serializable]
public class AcikParca
{
    public string Yol;            // dairedeki AcilirParca'nın yolu ("Esyalar/Buzdolabi/Kapi")
    public float Oran = 1f;       // 0 kapalı, 1 tam açık
}

// CreateAssetMenu: Project penceresinde sağ tık → Create menüsüne "Huzur/Kat Zinciri" ekler.
[CreateAssetMenu(fileName = "Zincir_Yeni", menuName = "Huzur/Kat Zinciri")]
public class KatZinciriVarligi : ScriptableObject
{
    [TextArea] public string aciklama;
    // TASARIMDA (Şahan, 2026-09-29): kat yeniden tasarlanırken işaretlenir. İşaretliyken zincir
    // uygulanmaz: odaların hepsi kilitsizdir ve katın anahtarı sahanlıkta durur (kat geçilebilir
    // kalır). Notlar ve süsler (seçilen mobilyalar) konur, böylece oda oda döşenen daire oyunda
    // görülebilir. Zincir testleri bu dosyayı atlar. Kapılar ve görev parçaları seçilince kalkar.
    public bool tasarimda;
    // YALNIZ SEÇİLEN EŞYALAR (#83): işaretliyse dairenin kalıcı eşyalarından (Esyalar altındakiler:
    // sigorta kutusu, tezgâh, komodin, sofra...) yalnızca bu katın seçtikleri görünür: aşağıdaki
    // listede adı yazanlar ve kat dosyasının başvurdukları (zincir yeri, notun çapası). Oda oda
    // yeniden döşenen katlar böyle kurulur; işaretsiz katlarda eski kural geçerli (hepsi görünür,
    // bulmaca eşyaları yalnızca gerektiği katta).
    public bool yalnizSecilenEsyalar;
    public List<string> gorunenEsyalar = new List<string>();
    // Kat açılınca aralık ya da açık başlayan parçalar (dolap kapakları, buzdolabı kapısı).
    public List<AcikParca> acikParcalar = new List<AcikParca>();
    // Buzdolabının fişi çekili mi (#83): işaretliyse kapısı açılsa da iç ışığı yanmaz.
    public bool buzdolabiFisiCekili;
    public KilitZinciri zincir = new KilitZinciri();
    public List<KatNotu> notlar = new List<KatNotu>();
    public List<KatSusu> suslar = new List<KatSusu>();
    public List<KatSesi> sesler = new List<KatSesi>();
    public List<KatParcasi> parcalar = new List<KatParcasi>();
    // Bu katta tavan lambası yanabilen odalar (#64): dairede elektrik varken yanarlar. Boş: hiçbiri.
    public List<string> yananOdalar = new List<string>();
    // Çift daire (#70): bu katta açık olan ikinci dairenin numarası (1-4); 0 = yok. Zincirde
    // onun odaları ve yerleri "B:" önekiyle yazılır. İkinci daire, kendi kaplaması olsun diye
    // başka bir katın kaplamasını giyer (ikinciKaplamaKati; 0 = bu katınki).
    public int ikinciDaire;
    public int ikinciKaplamaKati;
    public List<EsyaYazisi> esyaYazilari = new List<EsyaYazisi>();

    // ---------- SAHANLIK (#67, 7. kat: gözlem) ----------
    // Doluysa bu katta sahanlıktaki dört kapının hepsinde bu numara yazar ("13").
    public string kapiNumarasi = "";
    // İşaretliyse açık dairenin sahanlık kapısı kapalı başlar; dört kapı da "KAPIYI ÇAL" der:
    // gerçek olan açılır, öbürlerinde bir olay olur (SahanlikKapisi).
    public bool girisKapali;
    // Bu katta sahanlığa uygulanan senaryolu kart (rastgele anomalinin yerine): gerçek kapının
    // farkları (paspas yok, terlikler yok, gözetleme deliğinden ışık) gibi.
    public AnomaliKarti sahanlikKarti;
    public AudioClip tiklatmaSesi;                                     // kapıyı çalma sesi
    public List<AudioClip> yanlisKapiSesleri = new List<AudioClip>();  // yanlış kapının ardından gelen sesler
    // İşaretliyse daire bu katta hiç elektrik almaz (sigortasız): fosforlu yazılar görünür.
    public bool elektrikYok;

    // ---------- KAÇIŞ ANI (#58) ----------
    // İşaretliyse katın anahtarı alınınca siluet belirir ve oyuncuyu kovalar (KacisAni).
    public bool kacisVar;
    public GameObject kacisSablonu;          // siluetin şablonu (prefab)
    public float kacisGecikme = 3f;          // anahtardan kaç saniye sonra belirir
    public float kacisHizi = 2.0f;           // m/sn (oyuncu 1,8 yürür)
    public AudioClip kacisBaslangicSesi;     // asansör "ding"
    public AudioClip kacisHaberciSesi;       // anahtar alınınca üst kattan adımlar; siluet sesin ortasında belirir (#74)
    // Parçaların yere serilen kâğıdının gerçek boyu (metre): resmin bütünü bu kadar geniş olurdu.
    public float cizimBoyu = 0.30f;

    // Saklanma anı (#58, 6. kat): anahtar alınınca kapı tıklatılır, Gölge holde devriye gezer;
    // görülmeden atlatılmalı. Gölge'nin şablonu kacisSablonu'ndan (aynı siluet).
    public bool saklanmaVar;
    public AudioClip saklanmaSesi;           // giriş kapısında tıklatma
}
