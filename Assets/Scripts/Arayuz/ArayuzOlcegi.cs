// ArayuzOlcegi.cs
// OnGUI çizimlerini ekran çözünürlüğüne göre ölçekler (#34).
//
// KULLANIM: her OnGUI'nin başında ArayuzOlcegi.Uygula() çağrılır; yerleşim hesaplarında
// Screen.width / Screen.height yerine ArayuzOlcegi.Genislik / ArayuzOlcegi.Yukseklik
// kullanılır. Bunlar "sanal ekranın" boyutları: yükseklik hep 1080, genişlik ekranın
// en-boy oranına göre (16:9'da 1920, 16:10'da 1728...).
//
// NASIL ÇALIŞIYOR: GUI.matrix, OnGUI'nin çizdiği her şeye uygulanan bir dönüşüm
// (bir büyüteç gibi). Ölçek 2 olunca 24 piksellik bir yazı 48 piksel çizilir, 360
// piksellik bir düğme 720 piksel olur. Unity fare konumunu da aynı matrisle çevirir;
// tıklamalar düğmelerin üstüne doğru düşer.

using UnityEngine;

public static class ArayuzOlcegi
{
    /// <summary>Şu anki ekran için ölçek (1080 piksel yüksekliğe göre).</summary>
    public static float Olcek { get { return ArayuzOlcekKurali.Olcek(Screen.height); } }

    /// <summary>Sanal ekran genişliği (yerleşim hesapları için).</summary>
    public static float Genislik { get { return Screen.width / Olcek; } }

    /// <summary>Sanal ekran yüksekliği (yerleşim hesapları için; sınırlar içinde hep 1080).</summary>
    public static float Yukseklik { get { return Screen.height / Olcek; } }

    /// <summary>GUI matrisini ölçeğe ayarlar. Her OnGUI'nin başında çağrılır.</summary>
    public static void Uygula()
    {
        // #123: Çince/Japonca seçiliyken OnGUI yazı tipi Noto Sans CJK; stiller yazı tipini skin'den alır.
        GUI.skin.font = YaziTipi.Guncel;
        float o = Olcek;
        // Matrix4x4.Scale: yalnızca büyütme/küçültme içeren bir matris (x ve y aynı oranda).
        GUI.matrix = Matrix4x4.Scale(new Vector3(o, o, 1f));
    }
}
