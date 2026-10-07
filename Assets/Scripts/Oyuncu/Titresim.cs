// Titresim.cs
// Oyun kolu titreşimi (#124). Phasmophobia ve No70 gibi oyunlarda korku ele de gelir: av başlayınca, kapı
// çarpınca, gök gürleyince, yakalanınca kol titrer. Burada tek bir kapı var: Titresim.Vur(dusuk, yuksek, sure).
//
// NASIL: Unity'nin yeni Input System'inde Gamepad.SetMotorSpeeds(dusuk, yuksek) iki motoru sürer: "düşük
// frekanslı" motor (sol, ağır gümbürtü) ve "yüksek frekanslı" motor (sağ, ince vınlama). Süre dolunca
// motorlar durdurulmalı, yoksa kol sonsuza kadar titrer; bu yüzden bu sınıf bir MonoBehaviour: kendi
// Update'inde süreyi sayar. Nesne ilk çağrıda kendiliğinden kurulur (Altyazi gibi).
//
// KURALLAR: Ayarlar.Titresim kapalıysa hiçbir şey olmaz. Menüde (zaman durunca) motorlar durur, menüden
// çıkınca kalan süre sürmez (korku anı geçmiştir). Daha güçlü bir titreşim sürerken zayıf bir yenisi
// onu bastırmaz (KameraHareketi.Sars ile aynı kural). Oyun kapanırken ve nesne yok olurken motorlar
// sıfırlanır (ResetHaptics): Editor'de oyun durunca kol titremeye devam etmesin.

using UnityEngine;
using UnityEngine.InputSystem;

public class Titresim : MonoBehaviour
{
    private static Titresim ornek;
    private float dusuk, yuksek, bitis = -1f;
    private bool suruyor;

    /// <summary>Son isteğin motor güçleri ve bitiş anı (test göstergesi ve testler için).</summary>
    public static float SonDusuk { get { return ornek != null ? ornek.dusuk : 0f; } }
    public static float SonYuksek { get { return ornek != null ? ornek.yuksek : 0f; } }
    public static bool Suruyor { get { return ornek != null && ornek.suruyor; } }

    /// <summary>
    /// Kolu titretir: dusuk/yuksek 0-1 (sol ağır motor, sağ ince motor), sure saniye. Ayar kapalıysa ya da kol
    /// yoksa sessizce geçer. Sürmekte olan daha güçlü bir titreşimi bastırmaz.
    /// </summary>
    public static void Vur(float dusukGuc, float yuksekGuc, float sure)
    {
        if (!Ayarlar.Titresim || sure <= 0f) return;
        if (ornek == null)
        {
            GameObject g = new GameObject("Titresim");
            ornek = g.AddComponent<Titresim>();
        }
        float yeni = Mathf.Max(dusukGuc, yuksekGuc);
        if (ornek.suruyor && yeni < ornek.AnlikGuc()) return;
        ornek.dusuk = Mathf.Clamp01(dusukGuc);
        ornek.yuksek = Mathf.Clamp01(yuksekGuc);
        ornek.bitis = Time.unscaledTime + sure;
        ornek.suruyor = true;
        ornek.Uygula();
    }

    /// <summary>Motorları hemen durdurur (kat sıfırlama, final, menü).</summary>
    public static void Durdur()
    {
        if (ornek == null) return;
        ornek.suruyor = false;
        ornek.Sifirla();
    }

    // Sürmekte olan titreşimin şu anki gücü: kalan süreyle doğrusal söner (sonu yumuşak).
    private float AnlikGuc()
    {
        float kalan = bitis - Time.unscaledTime;
        if (kalan <= 0f) return 0f;
        return Mathf.Max(dusuk, yuksek) * Mathf.Clamp01(kalan / 0.4f);
    }

    private void Update()
    {
        if (!suruyor) return;
        // Menüde (zaman durur) ve ayar kapatılınca motorlar durur; titreşim orada biter.
        if (Time.timeScale <= 0f || !Ayarlar.Titresim || Time.unscaledTime >= bitis) { suruyor = false; Sifirla(); return; }
        Uygula();
    }

    // Kalan süreye göre sönen güç: son 0,4 saniyede doğrusal iner (ani kesilme yerine).
    private void Uygula()
    {
        Gamepad kol = Gamepad.current;
        if (kol == null) return;
        float kalan = bitis - Time.unscaledTime;
        float oran = Mathf.Clamp01(kalan / 0.4f);
        kol.SetMotorSpeeds(dusuk * oran, yuksek * oran);
    }

    private void Sifirla()
    {
        Gamepad kol = Gamepad.current;
        if (kol != null) kol.ResetHaptics();
    }

    private void OnDisable() { Sifirla(); }
    private void OnDestroy() { Sifirla(); if (ornek == this) ornek = null; }
    private void OnApplicationQuit() { Sifirla(); }
}
