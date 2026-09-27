// AsansorDugmesi.cs
// Asansörün çağrı düğmesi (#68). Kendi başına bir şey bilmez: basılınca panoya haber verir
// (AsansorPanosu). Elektrik yoksa pano yalnızca bir "tık" çalar; oyuncu asansörün ölü olduğunu
// duyar ve panoya bakar. İstem yalnızca panonun zincire bağlı olduğu katta (8. kat) çıkar;
// başka katlarda düğme sıradan bir süstür, çünkü asansör binanın her katında arızalıdır.
//
// Graybox kurucusu her gerçek sahanlığın düğmesine ekler ve panoyu bağlar.

using UnityEngine;

public class AsansorDugmesi : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private AsansorPanosu pano;

    public string Ipucu { get { return Dil.Getir("ipucu.cagir"); } }

    public bool EtkilesimeAcik { get { return pano != null && pano.DugmeAcik; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (EtkilesimeAcik) pano.DugmeyeBasildi();
    }
}
