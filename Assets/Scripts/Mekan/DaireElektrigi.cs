// DaireElektrigi.cs
// Dairede elektrik var mı (#54). Sigorta kilidi olan katta daire elektriksiz başlar; eksik
// sigorta sigorta kutusuna takılınca elektrik gelir. Elektrikle çalışan şeyler (buzdolabının
// ışığı, #55'te lambalar, radyo...) bu bileşene bakar ya da Degisti olayına abone olur.
//
// Bileşen dairenin kökünde durur; zincir uygulayıcısı her kat başında durumunu yazar.

using UnityEngine;

public class DaireElektrigi : MonoBehaviour
{
    [SerializeField] private bool var = true;

    public bool Var { get { return var; } }

    /// <summary>Elektrik geldi ya da gitti.</summary>
    public event System.Action Degisti;

    public void Ayarla(bool yeni)
    {
        if (var == yeni) return;
        var = yeni;
        Degisti?.Invoke();
    }
}
