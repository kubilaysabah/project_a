// EsyaAdlari.cs
// Zincirdeki bir eşyanın oyuncuya görünen adını, eşyanın kimliğinden çıkaran kural (#75).
//
// T listesi her katın eşyalarını adıyla gösterir. Adları kat dosyalarına tek tek yazmak
// yerine kimlikten türetiyoruz, çünkü kimlikler zaten anlamlı ("salon_anahtari",
// "tornavida") ve katlar arasında tekrarlanıyor: 5. kattaki tornavida ile 7. kattaki
// tornavida aynı adı almalı. Yeni bir eşya eklenince tabloya tek satır eklemek yeter.
//
// Bu dosya Unity bilmez (Dil'e de dokunmaz): yalnızca "hangi metin anahtarları, hangi
// sırayla" sorusuna cevap verir. Metni tablodan çekip birleştirmek arayüzün işi. Böylece
// kural Unity'yi açmadan test edilebilir.

/// <summary>
/// Bir eşya adının parçaları. Ekrandaki metin: string.Format(Dil(Ana), Dil(Ic), Sayi).
/// Ana her zaman dolu; Ic boş olabilir (o zaman {0} kullanılmaz).
/// </summary>
public struct EsyaAdi
{
    public string Ana;   // ana metin anahtarı ("esya.tornavida" ya da "esya.kirik_bas")
    public string Ic;    // Ana'nın içine {0} olarak girecek anahtar ("esya.salon_anahtari"); yoksa null
    public int Sayi;     // Ana'nın içine {1} olarak girecek sayı (çizim parçasının numarası); yoksa 0
}

public static class EsyaAdlari
{
    /// <summary>Eşya adlarının metin tablosundaki öneki.</summary>
    public const string Onek = "esya.";

    // İkinci dairenin (#70) eşyaları "a_" / "b_" ile başlayabilir ("a_mutfak_anahtari").
    // Oyuncu için ikisi de aynı eşya türüdür: önek ada girmez.
    private static readonly string[] DaireOnekleri = { "a_", "b_" };

    // Sofradaki isimlerin (bina.isim_riza ...) öneki. İki parçadan birleştiriliyor, çünkü metin
    // testi koddaki tırnak içindeki bina-önekli her yazıyı TAM bir anahtar sanar; bu ise yalnızca
    // anahtarın başı. Tam anahtarların tabloda olduğunu ZincirVarlikTestleri denetliyor.
    private const string IsimOneki = "bina" + ".isim_";

    /// <summary>Kimlik ve türe göre eşyanın adını kurar.</summary>
    public static EsyaAdi Coz(string kimlik, EsyaTuru tur)
    {
        string k = OnekiAt(kimlik ?? "");

        switch (tur)
        {
            case EsyaTuru.Tabak:
                // "tabak_riza" -> "Tabak ({0})", {0} = sofradaki ismin kendisi (bina.isim_riza).
                // Tabağın üstünde yazan isimle listedeki isim aynı anahtardan gelir.
                return new EsyaAdi { Ana = Onek + "tabak", Ic = IsimOneki + SonEk(k, "tabak_") };

            case EsyaTuru.Parca:
                // "p3" -> "Yırtık çizim parçası ({1})". Numara kimliğin sonundaki rakamlar.
                return new EsyaAdi { Ana = Onek + "cizim_parcasi", Sayi = SondakiSayi(k) };

            case EsyaTuru.AnahtarParcasi:
                // "salon_anahtari_bas" -> "{0} (sap)", {0} = "Salon anahtarı".
                // Kimliği "_bas" ile biterse sap, değilse uç (Envanter'deki kuralla aynı).
                bool bas = k.EndsWith("_bas");
                string anahtar = bas ? k.Substring(0, k.Length - 4)
                               : k.EndsWith("_uc") ? k.Substring(0, k.Length - 3) : k;
                return new EsyaAdi { Ana = Onek + (bas ? "kirik_bas" : "kirik_uc"), Ic = Onek + anahtar };

            default:
                return new EsyaAdi { Ana = Onek + k };
        }
    }

    private static string OnekiAt(string k)
    {
        foreach (string o in DaireOnekleri)
            if (k.StartsWith(o)) return k.Substring(o.Length);
        return k;
    }

    private static string SonEk(string k, string bas)
    {
        return k.StartsWith(bas) ? k.Substring(bas.Length) : k;
    }

    // Kimliğin sonundaki rakamları sayıya çevirir ("p12" -> 12). Rakam yoksa 0.
    private static int SondakiSayi(string k)
    {
        int i = k.Length;
        while (i > 0 && char.IsDigit(k[i - 1])) i--;
        int sayi;
        return i < k.Length && int.TryParse(k.Substring(i), out sayi) ? sayi : 0;
    }
}
