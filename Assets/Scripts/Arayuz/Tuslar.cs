// Tuslar.cs
// Oyunun atanabilir tuşları (#125): kural TusAtamaKurali'nde, burası Unity tarafı. Kod artık
// "Keyboard.current.rKey" demez; "Tuslar.Basildi(TusEylemi.Incele)" der. Atamalar ayar dosyasında
// (KayitDeposu.Ayar, "tus_atamalari") durur; Kontroller sayfası Dinle ile yeni tuş alır.
//
// ETKİLEŞİM EYLEMİ: Player/Interact bir Input System eylemi; onu sekiz yerde eylem üzerinden okuyoruz.
// Tuşu değiştirmek için eylemin klavye ve kol bağlantılarına "override" (üstüne yazma) uygulanır:
// bağlantı dosyası değişmez, çalışırken yolu değişir ("<Keyboard>/e" → "<Keyboard>/q").
//
// ADLAR: Key enum adı ("LeftCtrl") saklanır; ekranda okunur ad ("Ctrl") KlavyeAdi ile; kontrol yolu adı
// ("leftCtrl") Yol ile çıkarılır. Kol düğmeleri için aynı üçlü: "South" / "A" / "buttonSouth".

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;   // GamepadButton

public static class Tuslar
{
    private const string Anahtar = "tus_atamalari";
    private static TusAtamaKurali kural;
    private static bool hazir;

    // Dinlenebilecek kol düğmeleri (d-pad ve B/Start yasak olsa da listede: oyuncuya "atanamaz" denir).
    private static readonly GamepadButton[] KolDugmeleri = {
        GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.North,
        GamepadButton.LeftShoulder, GamepadButton.RightShoulder, GamepadButton.LeftTrigger, GamepadButton.RightTrigger,
        GamepadButton.Select, GamepadButton.Start, GamepadButton.LeftStick, GamepadButton.RightStick,
        GamepadButton.DpadUp, GamepadButton.DpadDown, GamepadButton.DpadLeft, GamepadButton.DpadRight,
    };

    public static TusAtamaKurali Kural { get { Hazirla(); return kural; } }

    private static void Hazirla()
    {
        if (hazir) return;
        hazir = true;
        kural = TusAtamaKurali.Oku(KayitDeposu.Ayar.GetString(Anahtar, ""));
        EtkilesimiUygula();
    }

    // ---------- OKUMA ----------
    /// <summary>Bu karede basıldı mı (klavye ya da kol)?</summary>
    public static bool Basildi(TusEylemi e) { return KlavyeBasildi(e) || KolBasildi(e); }

    public static bool KlavyeBasildi(TusEylemi e) { KeyControl k = KlavyeKontrolu(e); return k != null && k.wasPressedThisFrame; }
    public static bool KlavyeBasili(TusEylemi e) { KeyControl k = KlavyeKontrolu(e); return k != null && k.isPressed; }
    public static bool KolBasildi(TusEylemi e) { ButtonControl b = KolKontrolu(e); return b != null && b.wasPressedThisFrame; }

    private static KeyControl KlavyeKontrolu(TusEylemi e)
    {
        if (Keyboard.current == null) return null;
        Key tus;
        if (!Enum.TryParse(Kural.Klavye(e), out tus) || tus == Key.None) return null;
        try { return Keyboard.current[tus]; } catch (ArgumentException) { return null; }
    }

    private static ButtonControl KolKontrolu(TusEylemi e)
    {
        if (Gamepad.current == null) return null;
        GamepadButton d;
        if (!Enum.TryParse(Kural.Kol(e), out d)) return null;
        return Gamepad.current[d];
    }

    // ---------- ADLAR ----------
    /// <summary>Son kullanılan cihaza göre tuşun ekrandaki adı ("E" ya da "A"); ipuçları bunu yazar.</summary>
    public static string Ad(TusEylemi e) { return GirisCihazi.Kol ? KolAdi(e) : KlavyeAdi(e); }

    public static string KlavyeAdi(TusEylemi e)
    {
        // Klavye düzenine göre gerçek harf (AZERTY'de "A" tuşu "Q" yazar); yoksa enum adından.
        KeyControl k = KlavyeKontrolu(e);
        if (k != null && !string.IsNullOrEmpty(k.displayName) && k.displayName.Length <= 3) return k.displayName.ToUpperInvariant();
        return KlavyeAdi(Kural.Klavye(e));
    }
    public static string KolAdi(TusEylemi e) { return KolAdi(Kural.Kol(e)); }

    /// <summary>Key enum adından okunur ad: "LeftCtrl" → "Ctrl", "Digit1" → "1", "Numpad5" → "Num 5".</summary>
    public static string KlavyeAdi(string ad)
    {
        if (string.IsNullOrEmpty(ad)) return "?";
        switch (ad)
        {
            case "LeftCtrl": case "RightCtrl": return "Ctrl";
            case "LeftShift": case "RightShift": return "Shift";
            case "LeftAlt": case "RightAlt": return "Alt";
            case "Backspace": return "Backspace";
            case "CapsLock": return "Caps Lock";
            case "Tab": return "Tab";
            case "Semicolon": return ";";
            case "Comma": return ",";
            case "Period": return ".";
            case "Slash": return "/";
            case "Backslash": return "\\";
            case "Quote": return "'";
            case "Backquote": return "`";
            case "LeftBracket": return "[";
            case "RightBracket": return "]";
            case "Minus": return "-";
            case "Equals": return "=";
        }
        if (ad.StartsWith("Digit")) return ad.Substring(5);
        if (ad.StartsWith("Numpad")) return "Num " + ad.Substring(6);
        return ad;   // harfler ve F1..F12 olduğu gibi
    }

    /// <summary>GamepadButton adından Xbox dilinde okunur ad ("South" → "A", "LeftShoulder" → "LB").</summary>
    public static string KolAdi(string ad)
    {
        switch (ad)
        {
            case "South": return "A";
            case "East": return "B";
            case "West": return "X";
            case "North": return "Y";
            case "LeftShoulder": return "LB";
            case "RightShoulder": return "RB";
            case "LeftTrigger": return "LT";
            case "RightTrigger": return "RT";
            case "LeftStick": return "LS";
            case "RightStick": return "RS";
            case "Select": return "Select";
            case "Start": return "Start";
            case "DpadUp": return "D-pad ↑";
            case "DpadDown": return "D-pad ↓";
            case "DpadLeft": return "D-pad ←";
            case "DpadRight": return "D-pad →";
        }
        return string.IsNullOrEmpty(ad) ? "?" : ad;
    }

    // ---------- ATAMA ----------
    /// <summary>Eyleme yeni tuş atar, kaydeder ve etkileşim eylemine uygular. Sonuç kuraldan gelir.</summary>
    public static AtamaSonucu Ata(TusEylemi e, string tus, bool kolMu, out TusEylemi catisan)
    {
        AtamaSonucu sonuc = Kural.Ata(e, tus, kolMu, out catisan);
        if (sonuc == AtamaSonucu.Tamam) { Kaydet(); EtkilesimiUygula(); }
        return sonuc;
    }

    public static void VarsayilanaDon()
    {
        Kural.VarsayilanaDon();
        Kaydet();
        EtkilesimiUygula();
    }

    private static void Kaydet()
    {
        KayitDeposu.Ayar.SetString(Anahtar, kural.Metin());
        KayitDeposu.Ayar.Save();
    }

    /// <summary>
    /// Kontroller sayfası dinlerken her kare çağırır: basılan ilk klavye tuşu ya da kol düğmesi eyleme atanır.
    /// Dönüş: bitti mi; iptal (Esc ya da B) ise sonuc Gecersiz ve iptal true. catisan: Catisma'da öbür eylem.
    /// </summary>
    public static bool Dinle(TusEylemi e, out AtamaSonucu sonuc, out TusEylemi catisan, out bool iptal)
    {
        sonuc = AtamaSonucu.Gecersiz; catisan = e; iptal = false;
        if (Keyboard.current != null)
        {
            foreach (KeyControl k in Keyboard.current.allKeys)
            {
                if (!k.wasPressedThisFrame) continue;
                if (k.keyCode == Key.Escape) { iptal = true; return true; }
                sonuc = Ata(e, k.keyCode.ToString(), false, out catisan);
                return true;
            }
        }
        if (Gamepad.current != null)
        {
            foreach (GamepadButton d in KolDugmeleri)
            {
                if (!Gamepad.current[d].wasPressedThisFrame) continue;
                if (d == GamepadButton.East) { iptal = true; return true; }
                sonuc = Ata(e, KanonikKolAdi(d), true, out catisan);
                return true;
            }
        }
        return false;
    }

    // GamepadButton'da South/A/Cross aynı sayıdır; d.ToString() hangisini vereceğini söylemez ("A" verdi,
    // kuraldaki "South" ile eşleşmedi, çatışma denetimi atlandı). Kayıtta hep kanonik ad durur.
    public static string KanonikKolAdi(GamepadButton d)
    {
        if (d == GamepadButton.South) return "South";
        if (d == GamepadButton.East) return "East";
        if (d == GamepadButton.West) return "West";
        if (d == GamepadButton.North) return "North";
        return d.ToString();
    }

    // ---------- ETKİLEŞİM EYLEMİ (Player/Interact) ----------
    // Eylemin klavye bağlantısı "<Keyboard>/e", kol bağlantısı "<Gamepad>/buttonSouth". Override ile yol
    // değişir; eylemi okuyan sekiz yer hiçbir şey bilmeden yeni tuşu görür.
    private static void EtkilesimiUygula()
    {
        if (InputSystem.actions == null) return;
        InputAction eylem = InputSystem.actions.FindAction("Player/Interact");
        if (eylem == null) return;
        string klavyeYolu = "<Keyboard>/" + KlavyeYolu(kural.Klavye(TusEylemi.Etkilesim));
        string kolYolu = "<Gamepad>/" + KolYolu(kural.Kol(TusEylemi.Etkilesim));
        for (int i = 0; i < eylem.bindings.Count; i++)
        {
            string yol = eylem.bindings[i].path;
            if (yol.StartsWith("<Keyboard>/")) { if (eylem.bindings[i].effectivePath != klavyeYolu) eylem.ApplyBindingOverride(i, klavyeYolu); }
            else if (yol.StartsWith("<Gamepad>/")) { if (eylem.bindings[i].effectivePath != kolYolu) eylem.ApplyBindingOverride(i, kolYolu); }
        }
    }

    // Key enum adından kontrol yolu: "E" → "e", "LeftCtrl" → "leftCtrl", "Digit1" → "1", "Numpad1" → "numpad1", "F3" → "f3".
    public static string KlavyeYolu(string ad)
    {
        if (string.IsNullOrEmpty(ad)) return "e";
        if (ad.StartsWith("Digit")) return ad.Substring(5);
        return char.ToLowerInvariant(ad[0]) + ad.Substring(1);
    }

    public static string KolYolu(string ad)
    {
        switch (ad)
        {
            case "South": return "buttonSouth";
            case "East": return "buttonEast";
            case "West": return "buttonWest";
            case "North": return "buttonNorth";
            case "LeftStick": return "leftStickPress";
            case "RightStick": return "rightStickPress";
            case "DpadUp": return "dpad/up";
            case "DpadDown": return "dpad/down";
            case "DpadLeft": return "dpad/left";
            case "DpadRight": return "dpad/right";
        }
        return string.IsNullOrEmpty(ad) ? "buttonSouth" : char.ToLowerInvariant(ad[0]) + ad.Substring(1);   // leftShoulder, select, start...
    }
}
