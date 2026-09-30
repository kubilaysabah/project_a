// Kayit.cs
// Oyunun kaldığı yer (#52): otomatik kayıt.
//
// Eskiden bir tur birkaç dakikaydı; artık oyun iki saat sürecek (Docs/hikaye.md §9), oyuncu
// kapatıp açınca kaldığı kattan devam etmeli. Kayıt noktası: bir kat kapısı açılıp üst kata
// varılınca. (İleride #54: dairede her kilit çözülünce de.)
//
// Şimdilik PlayerPrefs'te tek sayı ("kayit_kat"): oyuncunun ulaştığı en yüksek kat. Yeni
// oyun 1'dir. Ayarlar ve defter de PlayerPrefs'te; hepsi aynı yerde, yedeklemesi kolay.

using UnityEngine;

public static class Kayit
{
    // Anahtarların adı yuvaya göre (KayitYuvasi): normalde "kayit_kat", Claude'un testinde
    // "kayit_kat_test". Sabit (const) değil, her okunuşta hesaplanan birer özellik.
    private static string KatAnahtari { get { return KayitYuvasi.Anahtar("kayit_kat"); } }
    private static string KilitAnahtari { get { return KayitYuvasi.Anahtar("kayit_kilitler"); } }   // çözülen kilitler (#54)
    private static string ParcaAnahtari { get { return KayitYuvasi.Anahtar("kayit_parcalar"); } }   // bulunan çizim parçaları (#63)

    private static string FotografAnahtari { get { return KayitYuvasi.Anahtar("kayit_fotograflar"); } }   // bakılan aile fotoğrafları (#69)
    private static string KatBasiAletAnahtari { get { return KayitYuvasi.Anahtar("kayit_katbasi_aletler"); } }   // kat başındaki alet çantası (#83)

    // ---------- AİLE FOTOĞRAFLARI (#69) ----------
    // Oyuncunun baktığı fotoğraflar bir koleksiyon: menüdeki Fotoğraflar sayfası onları gösterir.
    // Kat baştan kurulunca (kaçışta yakalanma) silinmez; bakılmış bir fotoğraf unutulmaz. Kilit
    // kaydıyla aynı biçimi kullanır ("kat:kimlik"), fotoğrafların katı yok sayılır: 0.
    private static KilitKaydi fotograflar;
    private static KilitKaydi Fotograflar
    {
        get
        {
            if (fotograflar == null) fotograflar = KilitKaydi.Oku(PlayerPrefs.GetString(FotografAnahtari, ""));
            return fotograflar;
        }
    }

    /// <summary>Oyuncu bu fotoğrafa baktı: koleksiyona eklenir (zaten varsa bir şey olmaz).</summary>
    public static void FotografBulundu(string kimlik)
    {
        if (!Fotograflar.Ekle(0, kimlik)) return;
        PlayerPrefs.SetString(FotografAnahtari, Fotograflar.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] '{kimlik}' fotoğrafı koleksiyona eklendi.");
    }

    public static bool FotografBulunduMu(string kimlik) { return Fotograflar.Iceriyor(0, kimlik); }

    // ---------- OKUNAN NOTLAR (#56) ----------
    // Oyuncunun okuduğu notlar (metin anahtarıyla) ve telefon konuşmaları. Kapıcı odasının panosu
    // onları listeler; kat sıfırlansa da kalır (fotoğraflar gibi). Biçim yine "0:anahtar".
    private static string NotAnahtari { get { return KayitYuvasi.Anahtar("kayit_notlar"); } }
    private static KilitKaydi notlar;
    private static KilitKaydi Notlar
    {
        get
        {
            if (notlar == null) notlar = KilitKaydi.Oku(PlayerPrefs.GetString(NotAnahtari, ""));
            return notlar;
        }
    }

    public static void NotOkundu(string anahtar)
    {
        if (string.IsNullOrEmpty(anahtar) || !Notlar.Ekle(0, anahtar)) return;
        PlayerPrefs.SetString(NotAnahtari, Notlar.Metin());
        PlayerPrefs.Save();
    }

    public static bool NotOkunduMu(string anahtar) { return Notlar.Iceriyor(0, anahtar); }

    /// <summary>Okunan notların anahtarları (sıralı).</summary>
    public static System.Collections.Generic.List<string> OkunanNotlar()
    {
        var liste = new System.Collections.Generic.List<string>();
        foreach (string parca in Notlar.Metin().Split(';'))
        {
            int iki = parca.IndexOf(':');
            if (iki > 0) liste.Add(parca.Substring(iki + 1));
        }
        return liste;
    }

    // Kilit kaydı ilk sorulduğunda PlayerPrefs'ten bir kez okunur, sonra bellekte tutulur.
    private static KilitKaydi kilitler;
    private static KilitKaydi Kilitler
    {
        get
        {
            if (kilitler == null) kilitler = KilitKaydi.Oku(PlayerPrefs.GetString(KilitAnahtari, ""));
            return kilitler;
        }
    }

    /// <summary>Bir kilit çözüldü: kaydedilir (#54). Oyun açılınca o kilit çözük başlar.</summary>
    public static void KilitCozuldu(int kat, string kimlik)
    {
        if (!Kilitler.Ekle(kat, kimlik)) return;
        PlayerPrefs.SetString(KilitAnahtari, Kilitler.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] {kat}. kat: '{kimlik}' kilidi kaydedildi.");
    }

    // Bulunan çizim parçaları (#63): kilitlerle aynı biçimde ("3:p1;3:p2"), aynı sınıfla tutulur.
    private static KilitKaydi parcalar;
    private static KilitKaydi Parcalar
    {
        get
        {
            if (parcalar == null) parcalar = KilitKaydi.Oku(PlayerPrefs.GetString(ParcaAnahtari, ""));
            return parcalar;
        }
    }

    /// <summary>Bir çizim parçası bulundu: kaydedilir; oyun açılınca yeniden konmaz.</summary>
    public static void ParcaBulundu(int kat, string kimlik)
    {
        if (!Parcalar.Ekle(kat, kimlik)) return;
        PlayerPrefs.SetString(ParcaAnahtari, Parcalar.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] {kat}. kat: '{kimlik}' parçası kaydedildi.");
    }

    public static bool ParcaBulunduMu(int kat, string kimlik) { return Parcalar.Iceriyor(kat, kimlik); }

    /// <summary>Bu katın bu kilidi daha önce çözülmüş mü?</summary>
    public static bool KilitCozukMu(int kat, string kimlik) { return Kilitler.Iceriyor(kat, kimlik); }

    /// <summary>
    /// Bir katı baştan başlatır (#58, Şahan'ın kararı): o katta çözülen kilitler ve bulunan
    /// parçalar silinir. Ulaşılan kat ve bulunan aletler kalır; aletler hangi katta bulunduğunu
    /// tutmadığı için ayrılamaz, zaten yeniden yere konmazlar (tornavida gibi, hep oyuncuda).
    /// </summary>
    public static void KatiSifirla(int kat)
    {
        int kilit = Kilitler.KatiSil(kat);
        int parca = Parcalar.KatiSil(kat);
        PlayerPrefs.SetString(KilitAnahtari, Kilitler.Metin());
        PlayerPrefs.SetString(ParcaAnahtari, Parcalar.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] {kat}. kat baştan: {kilit} kilit ve {parca} parça silindi.");
    }

    // ---------- KAT BAŞI KAYDI (#83, Şahan'ın kararı) ----------
    // "Her katı otomatik kaydetsin; katta birkaç eşya bulunmuşken oyuncu oyundan çıkarsa kat sıfırlansın,
    // sanki ilk defa çıkıyormuş gibi olsun." Oyun açıkken kat içindeki ilerleme eskisi gibi kayda yazılır
    // (aşağı inip dönünce, kovalamacada yakalanınca daire kayda bakarak kurulur). Oyun kayıttan AÇILDIĞINDA
    // ise o katın kilitleri, parçaları ve anları silinir, alet çantası kata varıldığı andaki hâline döner:
    // o katta bulunan levye gider, önceki katlardan gelen tornavida kalır.

    /// <summary>Yeni bir kata varıldı: o anki alet çantası kat başı anı olarak saklanır.</summary>
    public static void KatBasiniKaydet()
    {
        PlayerPrefs.SetString(KatBasiAletAnahtari, Aletler.Metin());
        PlayerPrefs.Save();
        Debug.Log($"[KAYIT] {Kat}. katın başı kaydedildi (aletler: '{Aletler.Metin()}').");
    }

    /// <summary>
    /// Oyun kayıttan açıldı: kat ilk kez giriliyormuş gibi başlar. Katın kilitleri ve parçaları silinir
    /// (KatiSifirla), alet çantası kat başındaki hâline döner. Kat başı anı hiç saklanmamışsa (bu kural
    /// gelmeden önceki bir kayıt) aletlere dokunulmaz.
    /// </summary>
    public static void KatBasinaDon(int kat)
    {
        KatiSifirla(kat);
        if (PlayerPrefs.HasKey(KatBasiAletAnahtari)) Aletler.GeriYukle(PlayerPrefs.GetString(KatBasiAletAnahtari));
        Debug.Log($"[KAYIT] {kat}. kat baştan başlıyor (kat başı kaydı).");
    }

    /// <summary>Oyuncunun ulaştığı kat; 1 = başlangıç (kayıt yok).</summary>
    public static int Kat
    {
        get { return Mathf.Clamp(PlayerPrefs.GetInt(KatAnahtari, KatSayaci.BaslangicKati), KatSayaci.BaslangicKati, KatSayaci.CatiKati); }
        set
        {
            PlayerPrefs.SetInt(KatAnahtari, Mathf.Clamp(value, KatSayaci.BaslangicKati, KatSayaci.CatiKati));
            PlayerPrefs.Save();
            Debug.Log($"[KAYIT] Kat {Kat} kaydedildi.");
        }
    }

    /// <summary>Kayıt var mı (1. kattan ileride mi)?</summary>
    public static bool Var { get { return Kat > KatSayaci.BaslangicKati; } }

    /// <summary>Kaydı siler: yeni oyun.</summary>
    public static void Sil()
    {
        PlayerPrefs.DeleteKey(KatAnahtari);
        PlayerPrefs.DeleteKey(KilitAnahtari);
        PlayerPrefs.DeleteKey(ParcaAnahtari);
        PlayerPrefs.DeleteKey(KatBasiAletAnahtari);
        kilitler = null;
        parcalar = null;
        Aletler.Bosalt();   // yeni oyun: bulunan aletler de gider (#61)
        PlayerPrefs.Save();
    }
}
