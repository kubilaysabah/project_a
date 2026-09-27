// CizimEkrani.cs
// Yırtık çizimin yakın bakış ekranı (#63, 3. kat: parça birleştirme).
//
// Çocuk çizimini dört parçaya yırtıp evin dört yerine saklamış. Kapıcı bir parça bulunca
// parça ele girmez; kapıcı onu biriktirir (aletler gibi) ve bu ekran açılır: kararmış ekranın
// ortasında o ana kadar bulunan parçalar, resimdeki yerlerinde. Eksik parçaların yeri boş.
// Dört parça tamamlanınca resim bütündür ve köşelerdeki rakamlar okunur.
//
// NASIL BİRLEŞİYOR: her parçanın dokusu TAM resim boyundadır (512 x 512) ve yalnızca kendi
// bölgesi görünür, gerisi saydamdır (Art/Cizim/scripts/cizim_uret.py). Bu yüzden bulunan
// parçaları aynı dikdörtgene üst üste çizmek yeter; her biri kendi yerine oturur. Yeni bir
// yırtık çizim (başka bir kat) de yalnızca yeni dokular ve veri ister.
//
// Bu bileşen katın dairesine eklenir (ZincirUygulayici.DekorUygula) ve kat değişince sökülür.
// Şifre ekranı da çizimi buradan ister (Ciz): oyuncu çarkları çevirirken çizimi yanında görür.
//
// KONTROLLER: E (kolda A), Esc (kolda B) kapatır. Oyuncunun yürümesi ve etkileşimi ekran
// açıkken kapanır, karenin sonunda (LateUpdate) geri verilir (şifre ekranındaki gibi, #29).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CizimEkrani : MonoBehaviour
{
    private int kat;
    private readonly List<KatParcasi> parcalar = new List<KatParcasi>();

    private bool acik;
    private int acilisKaresi;
    private bool geriVerilecek;
    private string yeniParca;                  // son bulunan parça: bir an parlak çizilir
    private float acilisZamani;

    private BirinciSahisKontrolcu kontrolcu;
    private Etkilesimci etkilesimci;
    private InputAction etkilesimEylemi;
    private GUIStyle sayacStili;

    /// <summary>Katın parçaları (kat dosyasından).</summary>
    public void Hazirla(int katNo, List<KatParcasi> liste)
    {
        kat = katNo;
        parcalar.Clear();
        if (liste != null) parcalar.AddRange(liste);
    }

    /// <summary>Bu kimlik bu çizimin bir parçası mı?</summary>
    public bool ParcasiMi(string kimlik)
    {
        foreach (KatParcasi p in parcalar) if (p.Kimlik == kimlik) return true;
        return false;
    }

    /// <summary>Kaç parça bulundu.</summary>
    public int Bulunan
    {
        get
        {
            int n = 0;
            foreach (KatParcasi p in parcalar) if (Kayit.ParcaBulunduMu(kat, p.Kimlik)) n++;
            return n;
        }
    }

    /// <summary>Bir parça alındı: kaydet ve ekranı aç.</summary>
    public void ParcaAlindi(string kimlik, GameObject oyuncu)
    {
        Kayit.ParcaBulundu(kat, kimlik);
        yeniParca = kimlik;
        Debug.Log($"[CIZIM] '{kimlik}' bulundu ({Bulunan}/{parcalar.Count}).");
        Ac(oyuncu);
    }

    private void Ac(GameObject oyuncu)
    {
        acik = true;
        acilisKaresi = Time.frameCount;
        acilisZamani = Time.unscaledTime;
        if (oyuncu != null)
        {
            kontrolcu = oyuncu.GetComponent<BirinciSahisKontrolcu>();
            etkilesimci = oyuncu.GetComponent<Etkilesimci>();
        }
        if (kontrolcu != null) kontrolcu.enabled = false;
        if (etkilesimci != null) etkilesimci.enabled = false;
        // Etkilesimci kapanırken paylaşılan "Player/Interact" eylemini de kapatır (#29).
        if (etkilesimEylemi == null && InputSystem.actions != null) etkilesimEylemi = InputSystem.actions.FindAction("Player/Interact");
        etkilesimEylemi?.Enable();
    }

    private void Update()
    {
        // Açıldığı karede kapanmasın: parçayı alan E basışı hâlâ "bu karede basıldı" görünür.
        if (!acik || Time.frameCount <= acilisKaresi) return;
        var k = Keyboard.current;
        var g = Gamepad.current;
        bool cik = (etkilesimEylemi != null && etkilesimEylemi.WasPressedThisFrame())
                   || (k != null && k.escapeKey.wasPressedThisFrame)
                   || (g != null && g.buttonEast.wasPressedThisFrame);
        if (cik) { acik = false; geriVerilecek = true; }
    }

    private void LateUpdate()
    {
        if (!geriVerilecek) return;
        geriVerilecek = false;
        if (kontrolcu != null) kontrolcu.enabled = true;
        if (etkilesimci != null) etkilesimci.enabled = true;
    }

    /// <summary>Çizimi (bulunan parçalarıyla) verilen dikdörtgene çizer. Şifre ekranı da kullanır.</summary>
    public void Ciz(Rect r)
    {
        // Eksik parçaların yeri: kâğıdın arkasındaki koyu masa. Çerçeve yok; yırtık kenarlar görünsün.
        foreach (KatParcasi p in parcalar)
        {
            if (p.Doku == null || !Kayit.ParcaBulunduMu(kat, p.Kimlik)) continue;
            // Yeni bulunan parça ilk yarım saniye biraz parlak: gözü yeni yerine çeker.
            float parla = p.Kimlik == yeniParca ? Mathf.Clamp01(1f - (Time.unscaledTime - acilisZamani) / 0.6f) : 0f;
            GUI.color = Color.Lerp(Color.white, new Color(1.3f, 1.25f, 1.1f), parla);
            // ScaleToFit: doku dikdörtgene oranı bozulmadan sığdırılır.
            GUI.DrawTexture(r, p.Doku, ScaleMode.ScaleToFit, true);
        }
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        if (!acik) return;
        ArayuzOlcegi.Uygula();
        float w = ArayuzOlcegi.Genislik, h = ArayuzOlcegi.Yukseklik;

        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float boy = 760f;
        Ciz(new Rect((w - boy) / 2f, (h - boy) / 2f - 20f, boy, boy));

        // Altta kaç parça bulunduğu: yalnızca rakam ("2 / 4"); metin tablosuna gerek yok.
        if (sayacStili == null)
            sayacStili = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUI.color = new Color(0.85f, 0.82f, 0.74f);
        GUI.Label(new Rect(0, (h + boy) / 2f - 10f, w, 60f), $"{Bulunan} / {parcalar.Count}", sayacStili);
        GUI.color = Color.white;
    }
}
