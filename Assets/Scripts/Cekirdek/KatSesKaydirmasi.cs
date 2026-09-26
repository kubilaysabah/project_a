// KatSesKaydirmasi.cs
// Her katın TV'sinin (ve boru tıkırtılarının) kaydın hangi anını çalacağını söyleyen kural (#39).
//
// Şahan build'de: "üst katlara çıktığım zaman bu sesin devam etmemesi lazım". Bütün katların
// TV'si aynı saate kilitliydi (OrtamSesi); yukarı çıktıkça aynı program kesintisiz devam
// ediyordu, sanki TV peşinden geliyordu. Artık her katın kendi sabit kaydırması var:
//   çalınacak an = (ortak saat + katın kaydırması) mod kayıt uzunluğu.
// Kaydırma katın numarasından hesaplandığı için sabit: aynı kata dönen oyuncu o katın
// TV'sini yine aynı yerinden duyar, ışınlanmada da A ile B aynı katı gösterdiği an aynı
// yeri çalar (ses atlamaz).
//
// NASIL DAĞITIYORUZ: kat × 0,618... (altın oran) sayısının kesirli kısmı. Bu dizi 0-1
// aralığına çok düzgün yayılır: ardışık katlar kaydın birbirinden uzak yerlerine düşer,
// ilk dokuz kattan hiçbir ikisi birbirine yakın değildir. Rastgele sayı kullansaydık iki
// kat şans eseri aynı yere düşebilirdi.
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir.

public static class KatSesKaydirmasi
{
    private const double AltinOranKesri = 0.6180339887498949;

    /// <summary>
    /// Verilen katın kayıttaki kaydırması (saniye, 0 ile uzunluk arası).
    /// kat 0 ya da eksi olabilir (zemin kat, sahte katlar); sonuç yine aralıkta kalır.
    /// </summary>
    public static double Saniye(int kat, double uzunluk)
    {
        double x = kat * AltinOranKesri;
        double kesir = x - System.Math.Floor(x);   // Floor: aşağı yuvarla; eksi sayılarda da kesir 0-1 olur
        return kesir * uzunluk;
    }
}
