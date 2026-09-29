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
//
// NİŞANIN DEĞDİĞİ NOKTA (#83, Şahan'ın 1. kat testi, 2026-09-29): portmantonun alttaki ayakkabı dolabı
// kapaklarına basınca üstteki çekmece açılıyordu, o kapaklar açılmadığı hâlde nişangâh kırmızı
// doluyordu. Etkilesimci artık nişanın gövdeye değdiği noktayı bildirir; açılır parçalardan o noktaya
// en yakın olanı seçilir ve yalnızca 10 cm içindeyse. Yakında açılır parça yoksa (açılmayan bir
// kapağa bakılıyorsa) etkileşim yok, nişangâh dolmaz.

using UnityEngine;

public class Yonlendirici : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private MonoBehaviour[] parcalar;   // IEtkilesimli imzalayan açılır parçalar

    // Nişanın gövdeye değdiği son nokta (dünya koordinatı) ve bilinip bilinmediği. Bilinmiyorsa
    // (nişan gövdenin içinden başladı) eski kural: yapılabilir ilk parça.
    private Vector3 nokta;
    private bool noktaVar;

    // Bir parçanın bakılan noktaya en fazla bu kadar uzak olması gerekir (metre).
    private const float EnFazlaUzaklik = 0.10f;

    /// <summary>Etkilesimci her karede, nişanın bu gövdeye değdiği noktayı bildirir.</summary>
    public void NoktaAyarla(Vector3 dunyaNoktasi, bool biliniyor)
    {
        nokta = dunyaNoktasi;
        noktaVar = biliniyor;
    }

    // Parçanın çarpışma kutularından bakılan noktaya en kısa uzaklık. SqrDistance: noktanın kutuya
    // olan uzaklığının karesi (kutunun içindeyse 0); karekökü alınınca metre olur.
    private float Uzaklik(MonoBehaviour parca)
    {
        float enKisa = float.MaxValue;
        foreach (Collider c in parca.GetComponentsInChildren<Collider>())
            if (c.enabled) enKisa = Mathf.Min(enKisa, Mathf.Sqrt(c.bounds.SqrDistance(nokta)));
        return enKisa;
    }

    private IEtkilesimli Hedef
    {
        get
        {
            // "m is IEtkilesimli e": m bu sözleşmeyi imzalıyorsa, onu e adıyla bir IEtkilesimli
            // olarak kullan demektir (kontrol ve dönüştürme tek adımda).
            // Önce gövdenin kendi üstündeki öbür etkileşimler (kendisi hariç).
            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
                if (m != this && m is IEtkilesimli e && m.enabled && e.EtkilesimeAcik) return e;
            if (parcalar == null) return null;
            if (!noktaVar)
            {
                foreach (MonoBehaviour m in parcalar)
                    if (m != null && m.gameObject.activeInHierarchy && m is IEtkilesimli e && e.EtkilesimeAcik) return e;
                return null;
            }
            // Bakılan noktaya en yakın yapılabilir parça, en fazla 10 cm uzakta.
            IEtkilesimli enIyi = null;
            float enYakin = EnFazlaUzaklik;
            foreach (MonoBehaviour m in parcalar)
            {
                if (m == null || !m.gameObject.activeInHierarchy || !(m is IEtkilesimli e) || !e.EtkilesimeAcik) continue;
                float d = Uzaklik(m);
                if (d <= enYakin) { enYakin = d; enIyi = e; }
            }
            return enIyi;
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
