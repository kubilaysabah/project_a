// GrayboxKurucu_Kat7.cs
// 7. katın içerik dosyası (Assets/Zincirler/Kat_7.asset): oda oda döşeme ve kilit zinciri (#83).
//
// ---------- 7. KAT: GÖZLEM + KOVALAMACA (#67, #58), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// SAHANLIK: dört kapının hepsinde "13", dördü de kapalı. Gerçek kapı 1 numaralı dairenin (Kapi_1): önünde
// paspas ve terlik yok, gözetleme deliğinde ışık yanar (sahanlık kartı). Cemal'in notu artık bunu tarif
// etmiyor (ipucu notu yok); oyuncu farkı kendisi görür. Yanlış kapıyı çalmak bir olay tetikler.
// DAİRE: kapıcı Cemal'in kendi evi (Sivaslı, bekâr); elektrik yok. Cemal binayı araştırıyormuş (Oda B'de
// gazete kupürleri, dokuz katlı bina çizimi); gitmek için eşyalarını toplamış ama gidememiş.
// Kurallar: katlarda aynı model yok; ipucu notu yok (kol ve şifre notları kaldırıldı); bir odada en fazla
// bir görev parçası.
//
// Kapı sırası ve görev parçaları (yedi kilit):
//   Antre  : yerde tornavida (çantada zaten varsa konmaz).
//   Banyo  : (menteşe, tornavida) lavabonun üstünde duvarda Cemal'in nüfus cüzdanı: doğum günü 11.07.
//   Salon  : (şifre 1107) büfenin alt dolabında kilerin anahtarı.
//   Kiler  : (anahtar) duvardaki kol -> mutfağın sürgüsü.
//   Mutfak : (kol) tezgâhın üst çekmecesinde Oda A anahtarının ucu.
//   Hol    : (kilitsiz) konsolun çekmecesinde Oda A anahtarının başı.
//   Oda A  : (kırık anahtar) dip duvarda fosforlu boyayla 0913: yalnızca kafa lambası kapalıyken görünür.
//   Oda B  : (şifre 0913) Cemal'in sandığı tornavidayla kanırtılır: içinde katın anahtarı -> kaçış anı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // kat7_esyalar.py raporundan: büfenin alt dolabının tabanı ve birinci kapağın bölmesinin merkezi,
    // sandığın iç tabanı (Unity eksenlerinde).
    const float Bufe7Taban = 0.103f;
    const float Bufe7KapiX = 0.296f;
    const float Sandik7Taban = 0.054f;

    static void Kat7SablonlariniKur()
    {
        foreach (string ad in new[] { "Is_Botlari_K7", "Tulum_Askisi_K7", "Supurge_K7", "Sivas_Kilimi_K7", "Kanepe_Yatak_K7",
                                      "Kucuk_Sehpa_K7", "TV_Taburesi_K7", "Perde_K7", "Tup_Ocak_K7", "Mutfak_Masasi_K7", "Sandalye_K7",
                                      "Bulasik_Yigini_K7", "Tiras_Takimi_K7", "Havlu_K7", "Kova_K7", "Raf_K7", "Merdiven_K7",
                                      "Tek_Kisilik_Yatak_K7", "Komodin_K7", "Koliler_K7", "Bavullar_K7", "Arastirma_Duvari_K7" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Hol_Konsolu_K7");
        AcilirSablonKur("Bufe_Cemal_K7");
        AcilirSablonKur("Sandik_Cemal_K7");
        // Saklanılabilen gardırop (#97): iki dar kapaklı (Gardirop_K7.fbx); adında "Gardirop" geçtiği için
        // daire açılınca SaklanmaYeri de eklenir (SaklanmaKurali).
        AcilirSablonKur("Gardirop_K7");
    }

    static KatZinciriVarligi Kat7Kur(GameObject golge)
    {
        string yol = $"{ZincirKlasoru}/Kat_7.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "7. kat, daire 1, Cemal'in dairesi (gözlem). Sahanlıkta dört '13' kapısı, gerçeği farklarıyla bulunur; yanlış kapı " +
                     "bir olay tetikler. Elektriksiz daire, yedi kilit: banyo (menteşe; nüfus cüzdanı), salon (1107; büfede kiler anahtarı), " +
                     "kiler (kol), mutfak (çekmecede anahtarın ucu; başı holdeki konsolda), Oda A (kırık anahtar; fosforlu 0913), Oda B " +
                     "(0913), sandık (tornavidayla kanırt) -> katın anahtarı, kaçış. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        v.kapiNumarasi = "13";
        v.girisKapali = true;
        v.sahanlikKarti = GercekKapiKarti();
        v.tiklatmaSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Tiklatma.wav");
        v.yanlisKapiSesleri = new List<AudioClip>
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Carpma.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Bebek_Ters.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Zil_DingDong.wav"),
        };
        v.elektrikYok = true;
        v.ikinciDaire = 0;
        // KAÇIŞ ANI (#58): sandıktaki anahtar alınınca asansör "ding" der; 3 sn sonra siluet anahtarın alındığı
        // yerden çıkar ve oyuncunun yolunu 2,0 m/sn izler (oyuncu 1,8). Yakalanınca kat baştan başlar.
        v.kacisVar = true;
        v.kacisSablonu = golge;
        v.kacisGecikme = 3f;
        v.kacisHizi = 2.0f;
        v.kacisBaslangicSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Asansor_Ding.wav");
        v.kacisHaberciSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Olay/Olay_UstKatAdimlari.wav");

        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "antre_yer", PlandaNokta = true, Oda = "Antre", U = 2.0f, Y = 0.004f, V = -0.4f, Donus = 70f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_kimlik", Yol = "Esyalar/Lavabo", Oda = "Banyo" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_bufe", Yol = "Bufe_Cemal", Oda = "Salon", KilitYolu = "Bufe_Cemal/Kapi_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        // #102 (kat kat zorlaşma): yer, KatDenetimi.SaklamaYeriAra'nın ölçtüğü adaylardan seçildi; parantezde
        // ayakta/eğilmiş görüldüğü nokta sayısı (20 cm ızgara, 5 m). Eski yer açıktaydı.
        // Anahtar ucu mutfağın dip köşesinde, tezgâhın dibinde (0/13). Eskiden tezgâhın üst çekmecesindeydi:
        // 3. ve 8. katta da AYNI çekmecenin aynı köşesinde anahtar vardı (Şahan: "hep benzer yerlerde").
        z.Yerler.Add(new ZincirYeri { Kimlik = "mutfak_kose", PlandaNokta = true, Oda = "Mutfak", U = 10.12f, Y = 0.004f, V = 0.12f, Donus = 50f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "hol_konsol", Yol = "Hol_Konsolu_Cemal/Cekmece_1", Oda = "Hol" });
        // Yatak odasının dip duvarı: dış duvar odanın dışına örülür, iç yüzü u = 10,5. Yazı yüzün yarım
        // santim önünde (5 cm öndeyken havada duruyordu: eksikler testi), göz hizasının üstünde, odaya (-u) bakar.
        z.Yerler.Add(new ZincirYeri { Kimlik = "odaA_duvar", PlandaNokta = true, Oda = "Oda_A", U = 10.495f, Y = 1.60f, V = 6.4f, OnU = -1f, OnV = 0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sandik", Yol = "Sandik_Cemal", Oda = "Oda_B", KilitYolu = "Sandik_Cemal/Kapak" });

        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "antre_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kiler_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "salon_bufe", X = Bufe7KapiX, Y = Bufe7Taban, Z = 0.004f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "hol_konsol", Y = 0.004f, Donus = -40f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "mutfak_kose" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sandik", Y = Sandik7Taban, Donus = 20f });

        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Salon", Gerekli = "b_kimlik",
                                          Sifre = "1107", IpucuYazma = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "kiler_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Oda_A",
                                          Gerekenler = new List<string> { "odaA_anahtari_bas", "odaA_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaA_duvar",
                                          Sifre = "0913", IpucuMetni = "0913", IpucuOlcek = 4f, IpucuKaranlikta = true });
        // Sandık tornavidayla kanırtılır (levye 5. katta kaldı; tornavida kapıcının çantasında).
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sandik", Tur = KilitTuru.Ekipman, OdaKapisi = false, Hedef = "sandik",
                                          Gerekenler = new List<string> { "tornavida" }, IpucuMetni = "ipucu.kanirt" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Cemal'in notu sahanlıkta, ilan panosunun yanında (hikâye: bütün kapılar 13 oldu).
            new KatNotu { Metin = "not.kat7_cemal", Yol = "Sahanlik:Ilan", X = 0.45f, Y = 0f, Z = 0.004f },
            // Nüfus cüzdanı banyoda, lavabonun üstünde duvarda (şifrenin kaynağı; lavabonun pivotu duvarın dibinde).
            new KatNotu { Metin = "not.kat7_kimlik", Yol = "Esyalar/Lavabo", X = 0f, Y = 1.25f, Z = 0.004f },
            // Cemal'in son notu Oda B'nin arka duvarında (v = 8,1), odaya (-v) bakar.
            new KatNotu { Metin = "not.kat7_son", X = 5.45f, Y = 1.40f, Z = 8.098f, OnU = 0f, OnV = -1f },
        };
        v.parcalar = new List<KatParcasi>();
        v.sesler = new List<KatSesi>();
        v.yananOdalar = new List<string>();
        v.esyaYazilari = new List<EsyaYazisi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah", "Klozet", "Lavabo" };

        v.suslar = new List<KatSusu>
        {
            // ---------- ANTRE ---------- (u 0-2,6, v -0,8-2,85; giriş kapısının köşesi boş)
            D("Is_Botlari_K7", 0.35f, 0f, 1.30f, 1f, 0f),
            D("Tulum_Askisi_K7", 2.60f, 1.08f, 1.20f, -1f, 0f),
            D("Supurge_K7", 2.30f, 0f, -0.80f, 0f, 1f),

            // ---------- HOL ---------- (u 0-10,5, v 2,95-4,65): salon ile Oda B kapıları arasında konsol,
            // üstünde Cemal'in fotoğrafı (arkasında biri duruyor).
            DA("Hol_Konsolu_Cemal", "Hol_Konsolu_K7", 3.40f, 0f, 4.65f - 0.16f, 0f, -1f),
            Foto("kat7", false, 3.62f, 0.80f, 4.52f, 0f, -1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),

            // ---------- SALON ---------- (u 0-5,0, v 4,75-8,1; kapının önü boş)
            // Cemal salonda yatıyor: Oda B duvarında çekyat, önünde sehpa, karşısında taburenin üstünde TV;
            // hol duvarında büfe (alt dolabında kilerin anahtarı); ortada Sivas kilimi.
            D("Sivas_Kilimi_K7", 2.80f, 0f, 6.45f, 0f, 1f),
            D("Kanepe_Yatak_K7", 5.00f - 0.44f - 0.02f, 0f, 6.45f, -1f, 0f),
            D("Kucuk_Sehpa_K7", 3.55f, 0f, 6.45f, 1f, 0f),
            D("TV_Taburesi_K7", 0.45f, 0f, 6.45f, 1f, 0f),
            DA("Bufe_Cemal", "Bufe_Cemal_K7", 3.00f, 0f, 4.75f + 0.24f, 0f, 1f),
            M("ceramic_pot", 4.50f, 0f, 7.75f, -1f, -1f),
            D("Perde_K7", 1.40f, 2.30f, 8.10f, 0f, -1f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),

            // ---------- MUTFAK ---------- (u 6,8-10,5, v -0,8-2,85; kapının önü boş; tezgâh dip duvarda)
            D("Tup_Ocak_K7", 8.00f, 0f, -0.50f, 0f, 1f),
            D("Mutfak_Masasi_K7", 8.60f, 0f, 1.30f, 0f, 1f),
            D("Sandalye_K7", 8.60f, 0f, 1.95f, 0f, -1f),
            D("Bulasik_Yigini_K7", 10.20f, 0.90f, 0.45f, -1f, 0f),

            // ---------- BANYO ---------- (lavabo ve klozet dairenin parçaları; kapı v 0,9-2,9'a devrilir)
            D("Tiras_Takimi_K7", 4.50f, 1.45f, 0.40f, -1f, 0f),
            D("Havlu_K7", 2.70f, 1.30f, 1.20f, 1f, 0f),
            D("Kova_K7", 4.20f, 0f, 0.72f, -1f, 0f),

            // ---------- KİLER ---------- (u 4,6-6,7; duvar kolu u 6,7 v 1,2)
            D("Raf_K7", 4.60f + 0.21f, 0f, 0.30f, 1f, 0f),
            D("Merdiven_K7", 5.60f, 0f, -0.80f, 0f, 1f),

            // ---------- ODA A ---------- (u 7,7-10,5, v 4,75-8,1; kapının önü boş; dip duvarın u 10,0-10,5,
            // v 5,8-7,0 bölümü fosforlu şifre için boş kalır)
            D("Tek_Kisilik_Yatak_K7", 7.70f + 0.51f, 0f, 8.10f - 0.98f, 0f, -1f),
            D("Komodin_K7", 8.95f, 0f, 8.10f - 0.19f, 0f, -1f),
            D("Koliler_K7", 10.28f, 0f, 5.33f, -1f, 0f),
            // Cemal'in gardırobu (#97): Oda B duvarına (u = 7,7) sırtını dayamış, odaya (+u) bakar; eşyalarını
            // kolilere toplamış, gardırop boş; içinde saklanılabilir. v 5,15-5,95: yatağa (v 6,14'ten) değmez;
            // kapakları açılınca u 8,62'ye kadar gelir, açık oda kapısının kanadı (u 8,7) serbest kalır.
            DA("Gardirop_Cemal", "Gardirop_K7", 7.70f, 0f, 5.55f, 1f, 0f),

            // ---------- ODA B ---------- (u 5,1-7,6, v 4,75-8,1; kapının önü boş)
            // Salon duvarında Cemal'in sandığı; pencerenin yanında bavullar; Oda A duvarında araştırma duvarı.
            DA("Sandik_Cemal", "Sandik_Cemal_K7", 5.10f + 0.26f, 0f, 7.20f, 1f, 0f),
            D("Bavullar_K7", 7.00f, 0f, 7.65f, 0f, -1f),
            D("Arastirma_Duvari_K7", 7.60f, 1.50f, 6.40f, -1f, 0f),
        };

        // ---------- KATIN ANI (#96): ARKADAN ÇARPAN KAPI ----------
        // Oda A'nın kapısı kırık anahtarla açılır; oyuncu karanlık yatak odasına girer (fosforlu şifre burada,
        // kafa lambasını kapatması gerekecek). 2 sn sonra, oyuncu kapıdan en az 1,5 m içerideyken kapı arkasından
        // hızla çarparak kapanır (0,2 sn). Kapı kilitlenmez; açılır. Ama oyuncu artık karanlıkta, kapalı bir
        // odada, ışığını söndürmek zorunda.
        v.anVar = true;
        v.an = new KatAniVerisi
        {
            Tur = KatAniTuru.KapiCarpma, Yol = "Kapilar/Kapi_Oda_A", TetikOda = "Oda_A", Gecikme = 2f, Sure = 0.2f,
            Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Carpma.wav"), Ses = 1f,
        };
        if (v.an.Klip == null) Debug.LogError("[ZINCIR] Kapi_Carpma.wav bulunamadı.");

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s7 = z.Cozumle(odalar);
        if (!s7.Cozulebilir) Debug.LogError("[ZINCIR] 7. kat zinciri çözülemiyor: " + string.Join("; ", s7.Hatalar));
        else Debug.Log("[ZINCIR] 7. kat zinciri çözülebilir: " + string.Join(" -> ", s7.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
