// VarlikGorunusu.cs
// Varlığın HER görünüşü için hangi model kullanılacağı (#140).
//
// NEDEN
// Oyuncu belirme, geçiş ve kaçışta altı üç harfli figüründen birini görüyordu (CinOlaylari'nın torbası), ama avda,
// 7. kat kovalamacasında, gezen gölgede, 6. katın saklanma anında, aynada ve duvardaki gölgede hâlâ eski kutu Gölge
// vardı: iki ayrı yaratık gibi duruyordu. Şimdi hepsi aynı torbadan çeker.
//
// NASIL
// Hareket ettiriciler (Varlik, KacisAni, GezenGolge, SaklanmaAni, AynaKorkusu, GolgeIzi) eskiden kendi şablonlarını
// (Kacis_Golgesi) doğrudan Instantiate ediyordu. Artık Sablon(eski) diyorlar: CinOlaylari sahnede varsa ve figürleri
// bağlıysa torbadan bir figür döner (aynı figür art arda gelmez); yoksa eski Gölge döner. Dokuzuncu kattaki oturan
// Gölge (KacisAni.HazirGolge) sahnenin parçasıdır, bu yoldan geçmez: oturma pozu eski Gölge'nin parçalarıyla yapılır.
//
// "static class": örneği yaratılmayan, yalnız fonksiyon taşıyan sınıf (Mathf gibi).

using UnityEngine;

public static class VarlikGorunusu
{
    /// <summary>Kullanılacak şablon: torbadan bir figür, figür yoksa verilen eski şablon.</summary>
    public static GameObject Sablon(GameObject eski)
    {
        CinOlaylari cin = CinOlaylari.Ornek;
        GameObject figur = cin != null ? cin.FigurAl() : null;
        return figur != null ? figur : eski;
    }
}
