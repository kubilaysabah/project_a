// LambaTitremesi.cs
// Bir lambanın ışıklarını (spot + dolgu) normal kattaki hafif titremeyle yakar (#38).
// Ne kadar parlak yanacağını TitremeEgrisi söyler; bütün lambalar aynı saati kullandığı
// için bütün katlar aynı anda titrer.
//
// ---------- ASIL ZOR KISIM: ANOMALİ KARTLARIYLA ÇAKIŞMAMAK ----------
// Işık kartları (sönük, kırmızı, göz kırpan ampul...) lambanın şiddetini doğrudan
// yazıyor, geri alırken de "kart gelmeden önceki" değeri geri yazıyor. Titreme şiddeti
// her karede değiştirseydi iki sorun çıkardı:
//   - Kartın yazdığı değer bir sonraki karede titremeyle ezilirdi.
//   - Kart, "önceki değer" olarak titremiş bir değeri kaydederdi; geri alınca lamba o
//     titremiş değerde takılı kalırdı ve her anomalide biraz daha kararırdı.
// Çözüm: titreme şiddeti yalnızca EKRANA ÇİZİLDİĞİ ANDA değiştirir.
//   Update:     şiddeti asıl değerine (tabana) geri koyar. Bütün Update'ler LateUpdate'lerden
//               önce çalışır, yani kat yöneticisi ve kartlar şiddeti hep asıl değeriyle görür.
//   LateUpdate: o karede kartların yazdığı son değeri taban olarak okur, titreme oranıyla
//               çarpıp yazar. Sıra numarası 200, kat yöneticisinden (100) sonra gelir.
// Kartlar da ceza gösterisi de (lambayı kapatıp açıyor) bu sayede olduğu gibi çalışır.

using UnityEngine;

[DefaultExecutionOrder(200)]
public class LambaTitremesi : MonoBehaviour
{
    private Light[] isiklar;
    private float[] taban;      // ışığın asıl (titremesiz) şiddeti
    private float[] yazilan;    // son karede bizim yazdığımız titremiş şiddet

    private void Awake()
    {
        // Spot ışık bu nesnede, dolgu ışığı çocuğunda. true: kapalı olanları da al.
        isiklar = GetComponentsInChildren<Light>(true);
        taban = new float[isiklar.Length];
        yazilan = new float[isiklar.Length];
        for (int i = 0; i < isiklar.Length; i++) taban[i] = yazilan[i] = isiklar[i].intensity;
    }

    private void Update()
    {
        for (int i = 0; i < isiklar.Length; i++)
        {
            // Bizim yazdığımızdan beri kimse dokunmadıysa asıl değere dön. Başka biri
            // (bizden sonra çalışan bir şey) yazdıysa, o yeni değer artık asıl değerdir.
            if (isiklar[i].intensity == yazilan[i]) isiklar[i].intensity = taban[i];
            else taban[i] = isiklar[i].intensity;
        }
    }

    private void LateUpdate()
    {
        float oran = TitremeEgrisi.Deger(Time.time);
        for (int i = 0; i < isiklar.Length; i++)
        {
            taban[i] = isiklar[i].intensity;          // bu karede kartların bıraktığı değer
            yazilan[i] = taban[i] * oran;
            isiklar[i].intensity = yazilan[i];
        }
    }
}
