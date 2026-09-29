// SarkacSallanir.cs
// Duvar saatinin sarkacını sallar (#83, 2. katta Nuri Bey'in antresindeki sarkaçlı saat).
//
// Nuri Bey hastanede ama saati hâlâ çalışıyor: sarkaç duvar düzleminde, bir yana bir yana sallanır.
// Bileşen sarkacın kendisinde durur; sarkacın pivotu asıldığı nokta (Blender'da öyle kuruldu). Salınım
// zamanın düz bir fonksiyonu (sinüs): her kare hesaplanır, birikmiş hata yok; kare hızı düşse de
// sarkaç aynı yerde olur.

using UnityEngine;

public class SarkacSallanir : MonoBehaviour
{
    [SerializeField] private float genlik = 5f;       // bir yana en fazla kaç derece
    [SerializeField] private float periyot = 2f;      // bir tam gidiş-geliş kaç saniye (saniye sarkacı)

    private Quaternion bas;                           // sarkacın duruş (dik) dönüşü

    private void Awake()
    {
        bas = transform.localRotation;
    }

    private void Update()
    {
        // Mathf.Sin: -1 ile 1 arasında gidip gelen dalga; genlikle çarpılınca derece olur.
        // Dönme ekseni yerel z: modelin ön yönü, yani duvara dik eksen; sarkaç duvar düzleminde sallanır.
        float aci = genlik * Mathf.Sin(2f * Mathf.PI * Time.time / periyot);
        transform.localRotation = bas * Quaternion.Euler(0f, 0f, aci);
    }
}
