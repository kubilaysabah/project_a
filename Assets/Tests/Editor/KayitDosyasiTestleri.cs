// KayitDosyasiTestleri.cs
// Kayıt dosyası biçiminin testleri (#122): gidiş-dönüş, kaçışlar, bozuk satırlar, sayı biçimi.

using System.Collections.Generic;
using NUnit.Framework;

public class KayitDosyasiTestleri
{
    [Test]
    public void Yazilan_metin_ayni_sozluge_doner()
    {
        var s = new Dictionary<string, string> { { "kayit_kat", "5" }, { "kayit_kilitler", "1:salon;2:kiler" }, { "kayit_notlar", "" } };
        var geri = KayitDosyasiKurali.Oku(KayitDosyasiKurali.Metin(s));
        Assert.AreEqual(3, geri.Count);
        Assert.AreEqual("5", geri["kayit_kat"]);
        Assert.AreEqual("1:salon;2:kiler", geri["kayit_kilitler"]);
        Assert.AreEqual("", geri["kayit_notlar"]);
    }

    [Test]
    public void Sekme_ve_satir_sonu_kacislanir()
    {
        var s = new Dictionary<string, string> { { "a", "iki\tsütun\nve satır\\ters" } };
        string metin = KayitDosyasiKurali.Metin(s);
        Assert.AreEqual(2, metin.Split('\n').Length - 1, "değerin içindeki satır sonu yeni satır açmamalı (başlık + 1 satır)");
        Assert.AreEqual("iki\tsütun\nve satır\\ters", KayitDosyasiKurali.Oku(metin)["a"]);
    }

    [Test]
    public void Bozuk_ve_yorum_satirlari_atlanir()
    {
        var geri = KayitDosyasiKurali.Oku("# yorum\n\nanahtarsiz\n\tdegerli\nkayit_kat\t3\r\nkayit_kat\t4\n");
        Assert.AreEqual(1, geri.Count);
        Assert.AreEqual("4", geri["kayit_kat"], "aynı anahtarın sonuncusu geçerli");
    }

    [Test]
    public void Sayilar_dile_bagli_olmadan_yazilir_ve_okunur()
    {
        Assert.AreEqual("0.85", KayitDosyasiKurali.SayiMetni(0.85f));
        Assert.AreEqual(0.85f, KayitDosyasiKurali.Sayi("0.85", 1f), 0.0001f);
        Assert.AreEqual(1f, KayitDosyasiKurali.Sayi("0,85", 1f), 0.0001f, "virgüllü sayı bozuk sayılır, varsayılan döner");
        Assert.AreEqual(7, KayitDosyasiKurali.TamSayi("7", 0));
        Assert.AreEqual(-1, KayitDosyasiKurali.TamSayi("yedi", -1));
    }

    [Test]
    public void Bos_metin_bos_sozluk()
    {
        Assert.AreEqual(0, KayitDosyasiKurali.Oku("").Count);
        Assert.AreEqual(0, KayitDosyasiKurali.Oku(null).Count);
    }
}
