// CopToplayici.cs
// Kapıcının kaç poşet taşıdığını tutan sayaç.
//
// ---------- BU GÖREV NEDEN VAR ----------
// Poşet toplamak bir meşguliyet değil, bir TASARIM BAHANESİ.
// Oyuncuyu sahanlıkta oyalar ve kapıya YAKLAŞTIRIR. Anomalilerin çoğu
// kapının üstünde ve önünde: numara, zil, gözetleme deliği, terlikler, paspas.
// Oyuncu poşeti almak için oraya gittiğinde onları zaten görmüş olur.
//
// Nişan alma ve ipucu gösterme işi burada DEĞİL, Etkilesimci'de.
// Bu script sadece sayıyor. Poşetin kendisi nasıl alınacağını biliyor,
// oyuncu ise kaç tane taşıdığını.

using UnityEngine;

public class CopToplayici : MonoBehaviour
{
    [Tooltip("Şu ana kadar toplanan poşet sayısı.")]
    [SerializeField] private int tasinanPoset;

    /// <summary>Oyuncunun taşıdığı poşet sayısı.</summary>
    public int TasinanPoset { get { return tasinanPoset; } }

    // Taşınan poşet sayısını köşede kısa süre gösteren işaret (#43). Oyun başlarken
    // eklenir; sahnede ayrıca kurmak gerekmez.
    private void Awake()
    {
        if (GetComponent<PosetGostergesi>() == null) gameObject.AddComponent<PosetGostergesi>();
    }

    /// <summary>Bir poşet daha toplandı. CopPoseti çağırır.</summary>
    public void PosetSay()
    {
        tasinanPoset++;
        Debug.Log($"[ÇÖP] Poşet alındı. Taşınan: {tasinanPoset}");
    }

    /// <summary>Yanlış karar sonrası 1. kata dönülünce çağrılır.</summary>
    public void Sifirla()
    {
        tasinanPoset = 0;
    }
}
