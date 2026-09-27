// KolAyari.cs
// Bir "kol" parçasının nasıl hareket ettiği (#65): kurucu, kol olarak kullanılabilecek bir
// parçaya (duvardaki kol, vananın çarkı) bunu ekler; CekilenKol çekilirken buna bakar.
//
// Neden ayrı bir bileşen: duvardaki kol x ekseninde 120° aşağı iner ve "ÇEK" der; vananın
// çarkı ise kendi ön ekseni etrafında bir buçuk tur döner ve "ÇEVİR" der. Hareket, parçanın
// kendisine aittir (modeline bağlıdır), hangi katta hangi kilidi açtığına değil. Bu yüzden
// modeli kuran kurucu yazar; kilidi bağlayan zincir uygulayıcısı hiçbir parçayı adıyla tanımaz.
// Bileşen yoksa CekilenKol eski davranışla çalışır (x ekseni, uygulayıcının verdiği açı).

using UnityEngine;

public class KolAyari : MonoBehaviour
{
    [Tooltip("Parçanın döndüğü yerel eksen.")]
    public Vector3 eksen = Vector3.right;
    [Tooltip("Dönüş açısı (derece). Çark için 360'tan büyük olabilir.")]
    public float aci = 120f;
    [Tooltip("Hareketin süresi (saniye).")]
    public float sure = 0.45f;
    [Tooltip("Ekranda görünen eylemin metin anahtarı.")]
    public string ipucuAnahtari = "ipucu.cek";
}
