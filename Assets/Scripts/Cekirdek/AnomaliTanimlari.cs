// AnomaliTanimlari.cs
// Anomalilerin Unity'den BAĞIMSIZ tanımları.
//
// Bu dosya neden çekirdekte?
// Kart seçici (hangi katta hangi anomali çıkacak, aynı kart üst üste gelmesin,
// zorluk kata göre artsın) düz C# olacak ve testle korunacak. O seçicinin
// kartın katmanını ve zorluğunu bilmesi gerekiyor, ama kartın Unity tarafını
// bilmesine gerek yok. Bu yüzden kartın "sadece bilgi" kısmı buraya,
// Unity'ye bağlı kısmı ayrı bir dosyaya yazılıyor.

// ---------- KATMAN ----------
// Konsept belgesindeki üç anomali katmanı.
/// <summary>Anomalinin ne kadar sert olduğu.</summary>
public enum AnomaliKatmani
{
    /// <summary>Fark etmesi zor, küçük ayrıntı. Ters kapı numarası, tek terlik.</summary>
    Fisilti,

    /// <summary>Gözle görünür ama açıklanabilir. Yeşil ampul, aralık kapı.</summary>
    Tekinsiz,

    /// <summary>Nadir, klip değerinde an. Koridorun ucundaki figür.</summary>
    Kabus
}

// ---------- ARAYÜZ NEDİR ----------
// "interface", bir sözleşmedir. İçinde kod yoktur; sadece "bu sözleşmeyi
// imzalayan her sınıf şu bilgileri vermek ZORUNDADIR" der.
//
// Neden işimize yarıyor? Seçici, gerçek bir Unity kartıyla da çalışabilmeli,
// testlerde kullanacağımız sahte bir kartla da. İkisinin ortak noktası
// "kimliği, katmanı ve zorluğu var" olmasıdır. Seçiciye bu sözleşmeyi
// verdiğimizde, karşısındakinin gerçek mi sahte mi olduğunu hiç umursamaz.
// Böylece seçiciyi Unity'yi hiç açmadan test edebiliriz.

/// <summary>Seçicinin bir karttan bilmesi gereken her şey.</summary>
public interface IAnomaliKart
{
    /// <summary>Kartın kimliği. Kayıt ve hata ayıklama için.</summary>
    string Kimlik { get; }

    /// <summary>Anomalinin katmanı.</summary>
    AnomaliKatmani Katman { get; }

    /// <summary>1 (kolay fark edilir) ile 5 (çok zor fark edilir) arası.</summary>
    int Zorluk { get; }
}
