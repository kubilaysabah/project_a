// MetinButunlukTestleri.cs
// GERÇEK metin tablosunu (Assets/Resources/Metinler.txt) ve onu kullanan kodu
// birbirine karşı denetler (#15). Anomali kartlarının bütünlük testinin eşi.
//
// Neden gerekli: kod metni ANAHTARLA ister. Anahtarda bir harf hatası yapılırsa
// derleyici bunu fark etmez (anahtar sadece bir yazı), oyun da çökmez; ekranda
// "#ipucu.tpla#" görünür ve biri oynayıp görene kadar kimse bilmez. Bu test o
// hatayı derlemeden hemen sonra, adıyla gösterir.
//
// ANAHTAR NASIL TANINIYOR
// Kodda bir metin anahtarı her zaman bu öneklerden biriyle başlar: ipucu., not.,
// menu., dil., bina., ayar., tus. ya da tek başına "jenerik". Kodun içindeki tırnaklı yazılardan
// bu kalıba uyan her şey anahtar sayılır. Yeni bir önek eklenirse (ör. "ayar.")
// aşağıdaki Onekler listesine de eklenmeli.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class MetinButunlukTestleri
{
    private static readonly string[] Onekler = { "ipucu", "not", "menu", "dil", "bina", "ayar", "tus", "liste", "foto", "pano", "inceleme", "anons" };

    // Tablo dosyasını okuyup kurar. Testlerin hepsi buradan başlar.
    private static MetinTablosu TabloyuOku()
    {
        string yol = Path.Combine(Application.dataPath, "Resources", "Metinler.txt");
        Assert.IsTrue(File.Exists(yol), "Metin tablosu bulunamadı: " + yol);
        return MetinTablosu.Oku(File.ReadAllText(yol));
    }

    // Assets/Scripts altındaki bütün .cs dosyalarında geçen anahtarlar.
    // Regex (düzenli ifade): bir yazı kalıbını tarif eden küçük bir dil. Buradaki kalıp:
    //   "  → bir tırnak
    //   (ipucu|not|...)\.[a-z0-9_.]+  → öneklerden biri, bir nokta, sonra küçük harfler
    //   |jenerik(?:_[a-z]+)?  → ya da "jenerik" / "jenerik_terk" (#121)
    //   "  → kapanan tırnak
    private static HashSet<string> KoddakiAnahtarlar()
    {
        var kalip = new Regex("\"((?:" + string.Join("|", Onekler) + ")\\.[a-z0-9_.]+|jenerik(?:_[a-z]+)?)\"");   // jenerik_terk (#121)
        var bulunan = new HashSet<string>();   // HashSet: aynı değeri bir kez tutan küme

        string kok = Path.Combine(Application.dataPath, "Scripts");
        foreach (string dosya in Directory.GetFiles(kok, "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match m in kalip.Matches(File.ReadAllText(dosya)))
                bulunan.Add(m.Groups[1].Value);
        }
        return bulunan;
    }

    [Test]
    public void Tablo_dosyasi_hatasiz_okunur()
    {
        MetinTablosu tablo = TabloyuOku();

        // Hatalar varsa hepsini satır numarasıyla göster.
        Assert.AreEqual(0, tablo.Hatalar.Count, string.Join("\n", tablo.Hatalar));
        Assert.Greater(tablo.Sayi, 0, "Tablo boş okundu.");
    }

    [Test]
    public void Koddaki_her_anahtar_tabloda_var()
    {
        MetinTablosu tablo = TabloyuOku();

        // Where(a => ...): bir listeden koşulu sağlayan elemanları seçer (LINQ).
        // "a => !tablo.Var(a)" her eleman için çalışan tek satırlık bir fonksiyon:
        // "a'yı al, tabloda YOKSA seç". ToList() sonucu yeni bir listeye döker.
        List<string> eksik = KoddakiAnahtarlar().Where(a => !tablo.Var(a)).ToList();

        Assert.IsEmpty(eksik, "Kodda kullanılıp tabloda olmayan anahtarlar: " + string.Join(", ", eksik));
    }

    [Test]
    public void Tablodaki_her_anahtar_kodda_kullaniliyor()
    {
        MetinTablosu tablo = TabloyuOku();
        HashSet<string> kodda = KoddakiAnahtarlar();
        // "esya.<kimlik>" anahtarları kodda tek tek yazılmaz, eşya kimliğinden kurulur (T listesi,
        // #75). Onları ZincirVarlikTestleri zincirlerdeki eşyalarla eşleştirir.
        List<string> olu = tablo.Anahtarlar.Where(a => !a.StartsWith(EsyaAdlari.Onek) && !kodda.Contains(a)).ToList();

        Assert.IsEmpty(olu, "Tabloda olup hiçbir kodda kullanılmayan anahtarlar: " + string.Join(", ", olu));
    }

    [Test]
    public void Bicim_yer_tutuculari_iki_dilde_de_ayni()
    {
        // "Dil: {0}" gibi bir metinde {0}, kodun oraya koyacağı değerin yeri.
        // Çeviride unutulursa değer ekranda görünmez; fazladan yazılırsa hata verir.
        MetinTablosu tablo = TabloyuOku();
        var yerTutucu = new Regex(@"\{\d+\}");

        foreach (string a in tablo.Anahtarlar)
        {
            string tr = string.Join(",", yerTutucu.Matches(tablo.Getir(a, DilKodu.TR)).Cast<Match>().Select(m => m.Value).OrderBy(x => x));
            string en = string.Join(",", yerTutucu.Matches(tablo.Getir(a, DilKodu.EN)).Cast<Match>().Select(m => m.Value).OrderBy(x => x));
            Assert.AreEqual(tr, en, $"'{a}' anahtarının yer tutucuları iki dilde farklı.");
        }
    }
}
