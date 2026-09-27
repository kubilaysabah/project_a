// EsyaListesi.cs
// T tuşuyla açılıp kapanan kat listesi (#75, Şahan'ın isteği). Bulunulan katın kilit
// zincirindeki bütün eşyaları adıyla gösterir: bulunanlar açık renkte, henüz ulaşılmamış
// olanlar kırmızı. Yerleri yazmaz; yalnızca "bu katta neyi arıyorum" sorusuna cevap verir.
//
// Oyun kolunda Select (View) düğmesi. Liste açıkken oyuncu yürümeye devam edebilir: bir
// kapıcının cebindeki iş listesi gibi, oyunu durdurmaz. Menü, not, şifre ya da çizim
// ekranı açıkken (oyuncunun kontrolü kapalıyken) çizilmez.
//
// Sahnede kurulmasına gerek yok: oyun başında kendini oluşturur (EsyaGostergesi gibi).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EsyaListesi : MonoBehaviour
{
    // ---------- BU OTURUMDA ELE ALINANLAR ----------
    // Anahtar ve sigorta gibi elde taşınan eşyalar kaydedilmez (kullanılmadan oyundan çıkılırsa
    // yeniden yerine konur). Oyuncu birini eline alıp başka bir eşyayla değiştirdiyse onu yine de
    // BULMUŞTUR; listede kırmızıya dönmemeli. Bu yüzden ele alınan her eşyayı burada hatırlıyoruz.
    // "static": sahnedeki tek bir nesneye değil, sınıfın kendisine ait; Esya doğrudan yazabilir.
    private static readonly HashSet<string> alinanlar = new HashSet<string>();

    private static string Anahtar(int kat, string kimlik) { return kat + ":" + kimlik; }

    /// <summary>
    /// Elde taşınan bir eşya alındı (Esya çağırır). Oda anahtarı ve sigorta kat numarasız (0)
    /// taşınır (ZincirUygulayici öyle kurar); o zaman eşyanın bulunulan katta alındığını yazarız.
    /// </summary>
    public static void Alindi(int kat, string kimlik)
    {
        if (kat == 0)
        {
            KatYoneticisi y = FindAnyObjectByType<KatYoneticisi>();
            if (y != null) kat = y.MevcutKat;
        }
        alinanlar.Add(Anahtar(kat, kimlik));
    }

    /// <summary>Kat baştan kuruldu (kaçışta yakalanma, #58): o katta alınanlar unutulur.</summary>
    public static void KatiUnut(int kat)
    {
        // RemoveWhere: koşulu sağlayan bütün elemanları siler. "a => ..." her eleman için
        // çalışan tek satırlık bir fonksiyon: "a'yı al, bu katın ise sil".
        alinanlar.RemoveWhere(a => a.StartsWith(kat + ":"));
    }

    /// <summary>Yeni oturum (sahne yüklendi): hiçbir şey ele alınmamış.</summary>
    public static void Temizle() { alinanlar.Clear(); }

    // ---------- DURUM ----------
    private bool acik;
    private KatYoneticisi yonetici;
    private BirinciSahisKontrolcu kontrolcu;
    private GUIStyle baslikStili, satirStili, altStili;

    // Renkler: bulunan eşya kâğıt beyazı, bulunmayan koyu kırmızı (Şahan: "kırmızı görünsün").
    private static readonly Color Bulunan = new Color(0.90f, 0.87f, 0.80f);
    private static readonly Color Bulunmayan = new Color(0.86f, 0.22f, 0.18f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Olustur()
    {
        if (FindAnyObjectByType<EsyaListesi>() != null) return;
        var g = new GameObject("Esya_Listesi");
        DontDestroyOnLoad(g);   // sahne yeniden yüklense de (finalden sonra) yaşasın
        g.AddComponent<EsyaListesi>();
    }

    // Liste gösterilebilir mi? Başlık ekranında, açılışta (zemin kat) ve oyuncunun kontrolü
    // kapalıyken (menü, not, şifre ekranı, yakalanma karartması) hayır.
    private bool Gosterilebilir()
    {
        if (DuraklatmaMenusu.BaslikEkraninda) return false;
        // Sahne yeniden yüklenince eski nesneler yok olur; Unity'de yok olmuş nesne "== null" verir.
        if (yonetici == null) yonetici = FindAnyObjectByType<KatYoneticisi>();
        if (kontrolcu == null) kontrolcu = FindAnyObjectByType<BirinciSahisKontrolcu>();
        if (yonetici == null || kontrolcu == null || !kontrolcu.enabled) return false;
        if (yonetici.AcilisModunda || yonetici.TurBitti) return false;
        return yonetici.KatinZinciri(yonetici.MevcutKat) != null;
    }

    private void Update()
    {
        bool basildi = (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
                    || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame);
        if (basildi && Gosterilebilir()) acik = !acik;
    }

    /// <summary>Bu katın bu eşyası bulundu mu (ya da kullanıldı mı)?</summary>
    public static bool BulunduMu(int kat, ZincirEsyasi e, KilitZinciri zincir, KatYoneticisi yonetici)
    {
        switch (e.Tur)
        {
            // Biriktirilenler kayıtta tutulur (#63, #65, #66).
            case EsyaTuru.Parca:
            case EsyaTuru.AnahtarParcasi:
            case EsyaTuru.Tabak:
                return Kayit.ParcaBulunduMu(kat, e.Kimlik);
        }
        // Aletler katlar arası kalır (#61): önceki bir katta bulunan tornavida burada da bulunmuş sayılır.
        if (AletCantasi.AletMi(e.Tur)) return Aletler.Var(e.Kimlik);

        // Elde taşınanlar: şu an eldeyse, bu oturumda ele alındıysa ya da açtığı kilit çözüldüyse.
        // Cep.Kat 0 olabilir (kat numarasız eşya); o zaman yalnızca kimliğe bakılır.
        if (!Cep.ElBos && Cep.Kimlik == e.Kimlik && (Cep.Kat == kat || Cep.Kat == 0)) return true;
        if (alinanlar.Contains(Anahtar(kat, e.Kimlik))) return true;
        foreach (ZincirKilidi l in zincir.Kilitler)
            if (l.Gerekli == e.Kimlik && Kayit.KilitCozukMu(kat, l.Kimlik)) return true;
        // Kat anahtarı kat kapısını açınca harcanır; kapı açıksa bulunmuştu.
        return e.Tur == EsyaTuru.KatAnahtari && yonetici != null && yonetici.KatKapisiAcikMi(kat);
    }

    /// <summary>Eşyanın oyuncunun dilindeki adı (EsyaAdlari kuralı + metin tablosu).</summary>
    public static string Ad(ZincirEsyasi e)
    {
        EsyaAdi a = EsyaAdlari.Coz(e.Kimlik, e.Tur);
        string ic = a.Ic != null ? Dil.Getir(a.Ic) : "";
        return string.Format(Dil.Getir(a.Ana), ic, a.Sayi);
    }

    private void OnGUI()
    {
        if (!acik) return;
        if (!Gosterilebilir()) { acik = false; return; }   // bir ekran açıldıysa liste kapanır
        ArayuzOlcegi.Uygula();   // her ekranda aynı oranda (#34)

        if (baslikStili == null)
        {
            baslikStili = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            baslikStili.normal.textColor = Bulunan;
            satirStili = new GUIStyle(GUI.skin.label) { fontSize = 21 };
            altStili = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleRight };
            altStili.normal.textColor = new Color(0.6f, 0.58f, 0.54f);
        }

        int kat = yonetici.MevcutKat;
        KilitZinciri zincir = yonetici.KatinZinciri(kat);
        List<ZincirEsyasi> esyalar = zincir.Esyalar;

        // Sol ortada, yarı saydam koyu bir kâğıt. Boyu eşya sayısına göre.
        const float en = 420f, satirBoy = 32f, ust = 64f, alt = 36f;
        float boy = ust + esyalar.Count * satirBoy + alt;
        Rect kutu = new Rect(40f, (ArayuzOlcegi.Yukseklik - boy) / 2f, en, boy);
        GUI.color = new Color(0.03f, 0.03f, 0.035f, 0.82f);
        GUI.DrawTexture(kutu, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(kutu.x + 20f, kutu.y + 14f, en - 40f, 34f), string.Format(Dil.Getir("liste.baslik"), kat), baslikStili);

        float y = kutu.y + ust;
        int bulunan = 0;
        foreach (ZincirEsyasi e in esyalar)
        {
            bool var_ = BulunduMu(kat, e, zincir, yonetici);
            if (var_) bulunan++;
            Color renk = var_ ? Bulunan : Bulunmayan;
            // Solda küçük bir kare: bulunmuşsa dolu, bulunmamışsa yalnızca çerçeve.
            GUI.color = renk;
            Rect kare = new Rect(kutu.x + 22f, y + 9f, 14f, 14f);
            if (var_) GUI.DrawTexture(kare, Texture2D.whiteTexture);
            else
            {
                GUI.DrawTexture(new Rect(kare.x, kare.y, kare.width, 2f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(kare.x, kare.yMax - 2f, kare.width, 2f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(kare.x, kare.y, 2f, kare.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(kare.xMax - 2f, kare.y, 2f, kare.height), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            satirStili.normal.textColor = renk;
            GUI.Label(new Rect(kutu.x + 48f, y, en - 64f, satirBoy), Ad(e), satirStili);
            y += satirBoy;
        }

        GUI.Label(new Rect(kutu.x + 20f, kutu.yMax - alt, en - 40f, alt - 6f),
                  string.Format(Dil.Getir("liste.alt"), bulunan, esyalar.Count), altStili);
    }
}
