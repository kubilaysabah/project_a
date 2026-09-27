// KatZinciriVarligi.cs
// Bir katın kilit zincirini proje dosyası olarak saklayan varlık (#54, #55).
//
// ScriptableObject: sahnedeki bir nesneye değil, projedeki bir DOSYAYA ait veri kabı
// (anomali kartları da böyle). Zincirin kendisi (KilitZinciri) Unity'siz, düz C# ve testli;
// bu sınıf yalnızca onu bir .asset dosyasının içinde tutar, Inspector'da görünür kılar.
// #55'te her katın bir dosyası olacak; şimdilik geliştirme için beş kilit türünü birden
// kullanan bir test zinciri var (Assets/Zincirler/Zincir_Test.asset).

using UnityEngine;

// CreateAssetMenu: Project penceresinde sağ tık → Create menüsüne "Huzur/Kat Zinciri" ekler.
[CreateAssetMenu(fileName = "Zincir_Yeni", menuName = "Huzur/Kat Zinciri")]
public class KatZinciriVarligi : ScriptableObject
{
    [TextArea] public string aciklama;
    public KilitZinciri zincir = new KilitZinciri();
}
