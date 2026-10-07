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
            // #123: sekiz dilin hepsi Türkçeyle aynı yer tutucuları taşımalı.
            for (int d = 1; d < MetinTablosu.DilSayisi; d++)
            {
                string diger = string.Join(",", yerTutucu.Matches(tablo.Getir(a, (DilKodu)d)).Cast<Match>().Select(m => m.Value).OrderBy(x => x));
                Assert.AreEqual(tr, diger, $"'{a}' anahtarının yer tutucuları {(DilKodu)d} dilinde farklı.");
            }
        }
    }

    [Test]
    public void Her_satir_sekiz_dilde_dolu()
    {
        // #123: tablo sekiz dile çıktı; yarım çevrilmiş bir satır sessizce İngilizceye düşer (Getir), oyuncu
        // fark etmez, biz de etmeyiz. Bu test dosyayı ham okur: her veri satırı 1 + DilSayisi sütun ve hiçbir hücre boş.
        string yol = Path.Combine(Application.dataPath, "Resources", "Metinler.txt");
        string[] satirlar = File.ReadAllText(yol).Split('\n');
        var eksik = new List<string>();
        for (int i = 0; i < satirlar.Length; i++)
        {
            string s = satirlar[i].TrimEnd('\r');
            if (s.Trim().Length == 0 || s.StartsWith("#")) continue;
            string[] p = s.Split('\t');
            if (p.Length != 1 + MetinTablosu.DilSayisi) { eksik.Add($"{i + 1}. satır ({p[0]}): {p.Length} sütun"); continue; }
            for (int d = 1; d < p.Length; d++) if (p[d].Trim().Length == 0) eksik.Add($"{i + 1}. satır ({p[0]}): {(DilKodu)(d - 1)} boş");
        }
        Assert.IsEmpty(eksik, string.Join("\n", eksik));
    }

    [Test]
    public void Kontrol_satirlari_her_dilde_uc_parca()
    {
        // Kontroller sayfası "eylem|klavye|oyun kolu" üçlüsünü '|' ile böler; bir dilde '|' eksikse sütun kayar.
        MetinTablosu tablo = TabloyuOku();
        foreach (string a in tablo.Anahtarlar)
        {
            if (!a.StartsWith("menu.kontrol_") || tablo.Getir(a, DilKodu.TR).IndexOf('|') < 0) continue;
            for (int d = 0; d < MetinTablosu.DilSayisi; d++)
                Assert.AreEqual(3, tablo.Getir(a, (DilKodu)d).Split('|').Length, $"'{a}' {(DilKodu)d}: üç parça olmalı.");
        }
    }
}
