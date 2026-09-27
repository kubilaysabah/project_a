// Yonlendirici.cs
// Bir eşyanın gövdesine yapılan etkileşimi açılır parçasına yönlendirir (#61).
//
// Şahan'ın build testi: kavanozun kendisine bakınca istem çıkmıyordu; istem yalnızca üstteki
// 2 cm'lik kapağa bakınca geliyordu. Oyuncu "kavanoz"a bakar, "kavanozun kapağı"na değil.
// Bu bileşen gövdede durur ve etkileşimi sırasıyla şunlara yönlendirir:
//   1. Gövdenin üstündeki başka etkileşimler (sigorta kutusunun sigorta yuvası gibi,
//      zincir uygulayıcısı oyun sırasında ekler).
//   2. Kurucunun verdiği açılır parçalar (kapak, çekmece, kapı).
// Şu an yapılabilir olan ilki seçilir: kapak kapalıyken "AÇ", açıkken elde buşon varsa "TAK".

using UnityEngine;

public class Yonlendirici : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private MonoBehaviour[] parcalar;   // IEtkilesimli imzalayan açılır parçalar

    private IEtkilesimli Hedef
    {
        get
        {
            // "m is IEtkilesimli e": m bu sözleşmeyi imzalıyorsa, onu e adıyla bir IEtkilesimli
            // olarak kullan demektir (kontrol ve dönüştürme tek adımda).
            // Önce gövdenin kendi üstündeki öbür etkileşimler (kendisi hariç).
            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
                if (m != this && m is IEtkilesimli e && m.enabled && e.EtkilesimeAcik) return e;
            if (parcalar != null)
                foreach (MonoBehaviour m in parcalar)
                    if (m != null && m.gameObject.activeInHierarchy && m is IEtkilesimli e && e.EtkilesimeAcik) return e;
            return null;
        }
    }

    public string Ipucu { get { IEtkilesimli h = Hedef; return h != null ? h.Ipucu : ""; } }

    public bool EtkilesimeAcik { get { return Hedef != null; } }

    public void Etkilesim(GameObject oyuncu)
    {
        IEtkilesimli h = Hedef;
        if (h != null) h.Etkilesim(oyuncu);
    }

    /// <summary>İstemin yerleşeceği gövde: yönlendirilen parçanın kendisi (kapağın üstünde görünsün).</summary>
    public Transform HedefGovdesi
    {
        get { IEtkilesimli h = Hedef; MonoBehaviour m = h as MonoBehaviour; return m != null ? m.transform : transform; }
    }
}
