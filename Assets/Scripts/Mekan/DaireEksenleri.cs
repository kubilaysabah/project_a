// DaireEksenleri.cs
// Bir dairenin kendi eksenleri (#55): plandaki (u, v) noktasını sahanlığın (x, z) noktasına
// çevirir. Kurucudaki DaireYonu'nun oyun sırasındaki eşi; kurucu değerleri buraya yazar.
//
// Neden gerekli: dört daire birbirinin aynası. Bir kolinin "antrenin köşesinde" durması için
// her dairede başka bir (x, z) gerekir. Kat dosyası yerleri planın eksenleriyle yazar
// (u: sahanlık duvarından içeri derinlik, v: sahanlık duvarı boyunca, bkz. DairePlani);
// bu bileşen onları o dairenin gerçek konumuna çevirir. Aynalamayı nesneyi -1 ile ölçekleyerek
// değil, noktayı hesaplayarak yapıyoruz (yazılar ters okunmasın diye, bkz. GrayboxKurucu_Daire).

using UnityEngine;

public class DaireEksenleri : MonoBehaviour
{
    [SerializeField] private float yon = 1f;       // +1 sağ duvar, -1 sol duvar (u'nun x işareti)
    [SerializeField] private float sv = 1f;        // v'nin z'deki işareti: yakın daire +1, uzak -1
    [SerializeField] private float kapiZ;          // giriş kapısının z'si
    [SerializeField] private float disX = 3.7f;    // sahanlık yan duvarının dış yüzü (u = 0)

    /// <summary>Plandaki (u, y, v) noktası, dairenin kendi (yerel) uzayında.</summary>
    public Vector3 Nokta(float u, float y, float v) { return new Vector3(yon * (disX + u), y, kapiZ + sv * v); }

    /// <summary>Plandaki bir yön (du, dv), dairenin yerel uzayında.</summary>
    public Vector3 Yon(float du, float dv) { return new Vector3(yon * du, 0f, sv * dv); }
}
