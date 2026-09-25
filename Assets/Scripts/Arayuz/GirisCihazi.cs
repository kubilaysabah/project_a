// GirisCihazi.cs
// Oyuncunun en son hangi cihazı kullandığını izler: klavye/fare mi, oyun kolu mu (#29).
// Tuş ipuçları buna göre değişir: kol kullanan "TOPLA (A)", klavye kullanan "TOPLA (E)" görür.
//
// NASIL: Input System, herhangi bir cihazda herhangi bir TUŞA basıldığında haber
// verebiliyor (onAnyButtonPress). Basılan tuş bir oyun kolundaysa "kol", değilse
// (klavye tuşu, fare tıklaması) "klavye" diye not alıyoruz.
//
// Neden yalnızca tuşlar, çubuk hareketi değil: bu makinedeki Xbox kolunun sol çubuğu
// kendi kendine hafifçe kayıyor (#3). Çubuk hareketini de saysaydık, kola hiç
// dokunmayan bir klavye oyuncusunun ipuçları sürekli "(A)"ya dönerdi. Tuşlar ise
// yarıdan fazla basılmadıkça sayılmıyor; kayma onları tetikleyemez.

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;   // .Call(...) bu ad alanında

public static class GirisCihazi
{
    /// <summary>En son oyun koluna mı basıldı (değilse klavye/fare).</summary>
    public static bool Kol { get; private set; }

    // Dinleme kaydı. IDisposable: "işin bitince Dispose() ile kapat" diyen bir
    // sözleşme; dinlemeyi bırakmak için ona Dispose deniyor.
    private static IDisposable dinleyici;

    // Oyun başlarken (sahne yüklendikten sonra) dinlemeye başla. Final sonrası sahne
    // yeniden yüklenince de çağrılır; ikinci kez abone olmamak için kontrol ediliyor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Baslat()
    {
        if (dinleyici != null) return;

        // "kontrol => ..." : basılan tuş (InputControl) gelince çalışan kısa fonksiyon.
        // kontrol.device: o tuşun ait olduğu cihaz. "is Gamepad": oyun kolu mu?
        dinleyici = InputSystem.onAnyButtonPress.Call(kontrol => Kol = kontrol.device is Gamepad);
    }

    // Her oynatmanın en başında temizle (Dil.cs'teki gibi): Play'ler arası kalan eski
    // dinleyici iki kez haber almaya yol açmasın.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sifirla()
    {
        dinleyici?.Dispose();
        dinleyici = null;
        Kol = false;
    }
}
