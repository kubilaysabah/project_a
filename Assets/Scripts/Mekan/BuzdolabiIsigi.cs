// BuzdolabiIsigi.cs
// Buzdolabının iç ışığı (#54): kapı açıkken ve dairede elektrik varken yanar.
// 4. katta (elektriksiz daire) sigorta takılınca buzdolabı içi aydınlanır ve Oda B'nin
// anahtarı görünür (Docs/hikaye.md §7). Işık gölge atmadığı için kapı kapalıyken yanık
// kalsaydı kapının içinden odaya sızardı; bu yüzden yalnızca kapı açıkken yanar.

using UnityEngine;

public class BuzdolabiIsigi : MonoBehaviour
{
    [SerializeField] private Light isik;
    [SerializeField] private AcilirParca kapi;
    private DaireElektrigi elektrik;

    // Fişi çekilmiş buzdolabı (#83, 1. kat: taşınan ailenin boşalttığı buzdolabı): elektrik olsa da
    // ışığı yanmaz. Zincir uygulayıcısı her kat açılışında kat dosyasına göre yazar.
    public bool FisCekili { get; set; }

    private void Start()
    {
        // Dairenin kökünde duran elektrik bileşeni (yukarı doğru aranır).
        elektrik = GetComponentInParent<DaireElektrigi>(true);
    }

    private void Update()
    {
        if (isik == null) return;
        bool yansin = !FisCekili && kapi != null && kapi.Acik && (elektrik == null || elektrik.Var);
        if (isik.enabled != yansin) isik.enabled = yansin;
    }
}
