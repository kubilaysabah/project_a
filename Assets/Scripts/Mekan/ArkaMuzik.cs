// ArkaMuzik.cs
// Oyun boyunca arkada çok kısık çalan gerilim müziği (#36).
//
// Müziğin kendisi bir AudioSource: 2B (kafanın içinden, yeri yok), döngüde, kısık.
// Bu bileşen yalnızca iki şey yapar:
//   - Oyun başlayınca müziği başlatır.
//   - Final gelince (çatı kapısı açılınca) müziği yavaşça susturur: gece bitti, sabah
//     geldi; rahatlama anının üstünde gerilim müziği çalmamalı.
// Ceza karanlığında müzik de diğer seslerle birlikte durur (AudioListener.pause), ayrıca
// bir şey yapmaya gerek yok. Ayarlar'daki ses seviyesi de onu diğerleriyle birlikte ölçekler.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ArkaMuzik : MonoBehaviour
{
    // static: sahnede tek bir arka müzik var; FinalSahnesi ona bu yolla ulaşır, sahnede
    // aramak zorunda kalmaz. (Dil ve Ayarlar da aynı düzende.)
    public static ArkaMuzik Ornek { get; private set; }

    private AudioSource kaynak;

    private void Awake()
    {
        Ornek = this;
        kaynak = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (kaynak.clip != null && !kaynak.isPlaying) kaynak.Play();
    }

    private void OnDestroy()
    {
        if (Ornek == this) Ornek = null;
    }

    /// <summary>Müziği 'sure' saniyede yavaşça kısıp durdurur.</summary>
    public void Sustur(float sure)
    {
        StartCoroutine(Kis(sure));
    }

    private IEnumerator Kis(float sure)
    {
        float bas = kaynak.volume;
        for (float t = 0f; t < sure; t += Time.deltaTime)
        {
            kaynak.volume = Mathf.Lerp(bas, 0f, t / sure);
            yield return null;
        }
        kaynak.Stop();
        kaynak.volume = bas;
    }
}
