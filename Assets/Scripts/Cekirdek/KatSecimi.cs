// KatSecimi.cs
// Menüdeki "Katlar" sayfasının kuralı (#85, Şahan: "ana menüde ve Esc menüsünde kayıtlı bölümler ya da
// katlar diye bir sekme"). Her kat, varıldığı an kendiliğinden kaydedilir (#84); bu sayfa o kayıtlardan
// birine dönmeyi sağlar, bir bölüm seçme ekranı gibi.
//
// Kurallar:
//   - Oyuncunun şimdiye kadar ulaştığı en yüksek kata kadar bütün katlar açıktır; üstündekiler kilitli.
//     1. kat her zaman açıktır (oyunun başı).
//   - En yüksek kat unutulmaz: 6. kattayken 3. katı seçen oyuncu sonra yine 6. kata dönebilir.
//   - Bir kat seçilince o kat ve üstündeki katların kat içi ilerlemesi silinir (hepsi ilk kez oynanacakmış
//     gibi); altındaki katlar oyuncunun bıraktığı gibi kalır (aşağı inince öyle görünürler).
//
// Bu dosyada Unity'ye ait tek satır yok: kural Unity açılmadan test edilir (KatSecimiTestleri).
// Kaydı yazmak Kayit'in, sayfayı çizmek DuraklatmaMenusu'nun işi.

using System;
using System.Collections.Generic;

public static class KatSecimi
{
    /// <summary>Bu kat menüde seçilebilir mi? enYuksek: oyuncunun ulaştığı en yüksek kat.</summary>
    public static bool Acik(int kat, int enYuksek, int catiKati)
    {
        if (kat < 1 || kat > catiKati) return false;   // listede olmayan kat
        return kat <= Math.Max(1, enYuksek);           // 1. kat hep açık
    }

    /// <summary>Bir kat seçilince kat içi ilerlemesi silinecek katlar: seçilen kattan çatıya kadar.</summary>
    public static List<int> SilinecekKatlar(int secilen, int catiKati)
    {
        var liste = new List<int>();
        for (int k = Math.Max(1, secilen); k <= catiKati; k++) liste.Add(k);
        return liste;
    }

    /// <summary>Yeni bir kata varılınca en yüksek kat: büyüğü kalır (aşağı bir kat seçmek onu düşürmez).</summary>
    public static int YeniEnYuksek(int eski, int ulasilan) { return Math.Max(eski, ulasilan); }
}
