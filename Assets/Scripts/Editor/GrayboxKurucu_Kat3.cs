// GrayboxKurucu_Kat3.cs
// 3. katın içerik dosyası (Assets/Zincirler/Kat_3.asset): oda oda döşeme ve kilit zinciri (#83).
// GrayboxKurucu'nun parçasıdır ("partial"); şablon ve yerleşim yardımcıları GrayboxKurucu_Zincir.cs'te.
//
// ---------- 3. KAT: PARÇA BİRLEŞTİRME (#63), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// Daire 1, Kerem'in ailesi (anne Ayla, baba Murat, 7 yaşında Kerem ve bir bebek). Kalabalık, renkli,
// yaşanmış bir ev, gece yarısı bırakılmış gibi. Kerem "dokuzuncu kattaki adamı" çizmiş, resmi dört
// parçaya yırtıp saklamış; parçalar birleşince köşelerde 0 3 0 9 okunur: yatak odasının şifresi.
// Kurallar: katlarda aynı model yok; ipucu notu yok (annenin notu artık şifreyi söylemiyor);
// bir odada en fazla bir görev parçası.
//
// Kapı sırası ve görev parçaları:
//   Sahanlık  : dairenin kapısının önündeki ayakkabılıkta 1. parça.
//   Antre     : yerdeki okul çantasının üstünde 2. parça.
//   Salon     : kanepenin altında 3. parça (eğilince görülür, #79).
//   Mutfak    : tezgâhın üst çekmecesinde Oda B'nin (Kerem'in odası) anahtarı.
//   Oda B     : (anahtar) oyuncak sandığında 4. parça -> resim tamam.
//   Oda A     : (şifreli asma kilit 0309) komodinin çekmecesinde katın anahtarı.
//   Hol hikâye odası (çocuk resimleri, annenin notu); banyo ve kiler kapalı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // 3. katın Blender modelleri (Art/Blender/scripts/kat3_esyalar.py): şablonları kurulur.
    static void Kat3SablonlariniKur()
    {
        foreach (string ad in new[] { "Askilik_K3", "Ayakkabi_Yigini_K3", "Okul_Cantasi_K3", "Bebek_Arabasi_K3", "Terlikler_K3",
                                      "Cocuk_Cizimleri_K3", "Dagilmis_Oyuncaklar_K3", "Yolluk_K3",
                                      "Tuplu_TV_K3", "Boyama_Sehpasi_K3", "Oyun_Parki_K3", "Hali_K3", "Perde_K3", "Duvar_Rafi_K3",
                                      "Mama_Sandalyesi_K3", "Masa_K3", "Sandalye_K3", "Buzdolabi_K3", "Ocak_K3", "Camasir_Makinesi_K3", "Biberon_Takimi_K3",
                                      "Cocuk_Yatagi_K3", "Calisma_Masasi_K3", "Poster_K3", "Yerdeki_Cizimler_K3", "Gece_Lambasi_K3", "Perde_Cocuk_K3",
                                      "Karyola_K3", "Besik_K3", "Sifonyer_K3", "Perde_OdaA_K3", "Kilim_K3" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Komodin_K3");
    }

    // Okul çantasının üst yüzü (kat3_esyalar.py, yerde yatan çanta): 2. parça bunun üstünde durur.
    const float CantaUstu3 = 0.124f;

    static KatZinciriVarligi Kat3Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_3.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "3. kat, daire 1, Kerem'in ailesi (parça birleştirme). Kerem'in yırtık çiziminin dört parçası: sahanlıktaki " +
                     "ayakkabılıkta, antrede okul çantasının üstünde, salonda kanepenin altında, Oda B'deki oyuncak sandığında. " +
                     "Oda B'nin anahtarı mutfak çekmecesinde. Birleşen resmin köşelerinde 0 3 0 9: Oda A'nın şifresi; komodinin " +
                     "çekmecesinde katın anahtarı. Banyo ve kiler kapalı. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        // Okul çantası antrede yerde (u 1,9, v 1,9); parça onun üst yüzünde.
        z.Yerler.Add(new ZincirYeri { Kimlik = "canta", PlandaNokta = true, Oda = "Antre", U = 1.975f, Y = CantaUstu3, V = 1.90f, Donus = -20f });
        // Kanepe (sofa_02, 0,82 derin) Oda B duvarının önünde, ön kenarı u 4,16; altında 19 cm boşluk.
        // Parça ön kenardan 15 cm içeride: ayakta dururken kanepenin kenarı görüşü kapatır.
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", PlandaNokta = true, Oda = "Salon", U = 4.31f, Y = 0.004f, V = 6.45f, Donus = 25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Komodin_Ayla/Cekmece_1", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p1", Tur = EsyaTuru.Parca, Yer = "ayakkabilik", Y = 1.025f, Donus = -15f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p2", Tur = EsyaTuru.Parca, Yer = "canta" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p3", Tur = EsyaTuru.Parca, Yer = "salon_yeri" });
        // Çekmecenin içi: tezgâhın ekseninde x 0,59, y 0,64, z 0,04 (tezgah.py'deki üst çekmecenin tabanı).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p4", Tur = EsyaTuru.Parca, Yer = "oyuncak", Y = SandikTaban, Donus = -20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.005f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaB_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Sifre = "0309",
                                          Parcalar = new List<string> { "p1", "p2", "p3", "p4" } });
        v.zincir = z;

        // PARÇALAR: dokular Art/Cizim/scripts/cizim_uret.py'den. Her parçanın dokudaki bölgesi
        // dosyanın saydam olmayan piksellerinden hesaplanır: resim yeniden üretilince de doğru kalır.
        v.parcalar = new List<KatParcasi>();
        for (int i = 1; i <= 4; i++)
        {
            string dokuYolu = $"Assets/Textures/Cizim/Cizim_Kat3_P{i}.png";
            CizimDokusunuAyarla(dokuYolu);
            v.parcalar.Add(new KatParcasi { Kimlik = "p" + i, Doku = AssetDatabase.LoadAssetAtPath<Texture2D>(dokuYolu), Bolge = ParcaBolgesi(dokuYolu) });
        }
        v.cizimBoyu = 0.30f;
        v.sesler = new List<KatSesi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah" };

        // Annenin notu hikâye notudur (şifreyi söylemez): koridorda, Oda B'nin kapısının solunda.
        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat3_anne", X = 5.45f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
        };

        v.suslar = new List<KatSusu>
        {
            // ---------- ANTRE ----------
            // Antre u 0-2,6, v -0,8-2,85. Giriş kapısının açıldığı köşe (u 0-0,9, v -0,9-0,9) boş kalır;
            // sigorta kutusu dış duvarda (u 1,6). Banyo duvarında (u = 2,6) askılık; kapının yanında ayakkabılar.
            D("Askilik_K3", 2.60f, 1.29f, 1.20f, -1f, 0f),
            D("Ayakkabi_Yigini_K3", 0.35f, 0f, 1.30f, 1f, 0f),
            D("Terlikler_K3", 1.15f, 0f, 0.45f, -1f, 0f),
            D("Okul_Cantasi_K3", 1.90f, 0f, 1.90f, 1f, 0f),
            D("Bebek_Arabasi_K3", 2.25f, 0f, -0.80f, 0f, 1f),

            // ---------- HOL ----------
            // Hol u 0-10,5, v 2,95-4,65; kapılar odalara açılır, ortası yol. Kuzey duvarında (v = 4,65) salon
            // ile Oda B kapılarının arasında Kerem'in resimleri; güney duvarında aile fotoğrafı.
            D("Yolluk_K3", 5.60f, 0f, 3.80f, 0f, 1f),
            D("Cocuk_Cizimleri_K3", 3.80f, 1.00f, 4.65f, 0f, -1f),
            Foto("kat3", true, 4.60f, 1.60f, 2.95f, 0f, 1f),
            D("Dagilmis_Oyuncaklar_K3", 7.20f, 0f, 3.50f, 0f, 1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),

            // ---------- SALON ----------
            // Salon u 0-5,0, v 4,75-8,1. Kapı hol duvarında (u 0,71-1,81 önü boş); karşıda pencere (u 0,7-2,1)
            // ve hep kilitli balkon kapısı (u 3,2-4,0). Kanepe Oda B duvarında, karşısında TV dolabı.
            M("sofa_02", 5.00f - 0.41f - 0.02f, 0f, 6.45f, -1f, 0f),
            M("throw_pillows_01", 4.62f, 0.40f, 6.45f, -1f, 0f),
            M("painted_wooden_cabinet", 0.03f + 0.31f + 0.01f, 0f, 6.45f, 1f, 0f),
            D("Tuplu_TV_K3", 0.36f, 1.18f, 6.45f, 1f, 0f),
            M("Chandelier_01", 2.50f, Tavan, 6.45f, 0f, 1f),
            D("Hali_K3", 2.80f, 0f, 6.45f, 0f, 1f),
            D("Boyama_Sehpasi_K3", 3.40f, 0f, 6.45f, 1f, 0f),
            D("Oyun_Parki_K3", 2.85f, 0f, 7.40f, 0f, -1f),   // perdenin kenarına girmesin (çakışma testi)
            D("Perde_K3", 1.40f, 2.30f, PetekOnuPerde, 0f, -1f),
            D("Kornis_Dirsegi", 1.40f - 0.86f, 2.288f, 8.10f, 0f, -1f),   // korniş dirsekleri (perde duvardan 22 cm önde)
            D("Kornis_Dirsegi", 1.40f + 0.86f, 2.288f, 8.10f, 0f, -1f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),
            D("Duvar_Rafi_K3", 3.00f, 1.60f, 4.75f, 0f, 1f),

            // ---------- MUTFAK ----------
            // Mutfak u 6,8-10,5, v -0,8-2,85. Kapının önü (u 6,87-7,86, v 2,0-2,85) boş; dip duvarda tezgâh
            // (dairenin kalıcı eşyası, u 9,88-10,5). Dış duvar (v = -0,8) boyunca buzdolabı, ocak, çamaşır makinesi.
            D("Buzdolabi_K3", 7.20f, 0f, -0.80f + 0.33f, 0f, 1f),
            D("Ocak_K3", 8.00f, 0f, -0.80f + 0.33f, 0f, 1f),
            D("Camasir_Makinesi_K3", 8.75f, 0f, -0.80f + 0.30f, 0f, 1f),
            // Masa kiler duvarına yakın (ortadayken üç sandalye ve mama sandalyesiyle mutfak yürünemiyordu,
            // tezgâhın çekmecesine ulaşılamıyordu: yürünebilirlik denetimi). Bir sandalye ve masanın yanında mama sandalyesi.
            D("Masa_K3", 7.45f, 0f, 1.00f, 0f, 1f),
            D("Sandalye_K3", 7.45f, 0f, 1.75f, 0f, -1f),
            D("Mama_Sandalyesi_K3", 8.20f, 0f, 1.00f, -1f, 0f),
            D("Biberon_Takimi_K3", 10.20f, 0.90f, 1.72f, -1f, 0f),

            // ---------- ODA B (Kerem'in odası) ----------
            // Oda B u 5,1-7,6, v 4,75-8,1. Kapının önü (u 5,82-6,81, v 4,75-5,5) boş; pencere v = 8,1 (u 5,75-6,95).
            // Oyuncak sandığı dairenin kalıcı eşyası (u 5,3 v 7,3). Yatak Oda A duvarında, başı pencere tarafında.
            // Yatak pencere duvarı boyunca, başı Oda A duvarında: masa, sandalye ve oyuncak sandığının önünde
            // yürüme yolu kalsın diye (yürünebilirlik denetimi; eski yerinde oda 2 m²'ye iniyordu).
            D("Cocuk_Yatagi_K3", 7.60f - 0.98f, 0f, 8.10f - 0.48f, -1f, 0f),
            D("Calisma_Masasi_K3", 5.10f + 0.27f, 0f, 6.10f, 1f, 0f),
            M("SchoolChair_01", 5.95f, 0f, 6.10f, -1f, 0f),
            M("alarm_clock_01", 5.28f, 0.72f, 6.45f, 1f, 0f),
            D("Poster_K3", 5.10f, 1.55f, 6.10f, 1f, 0f),
            D("Yerdeki_Cizimler_K3", 6.55f, 0.002f, 6.40f, 0f, 1f),
            D("Gece_Lambasi_K3", 7.35f, 0f, 6.95f, -1f, 0f),
            D("Perde_Cocuk_K3", 6.35f, 2.30f, 8.10f, 0f, -1f),

            // ---------- ODA A (anne-babanın odası) ----------
            // Oda A u 7,7-10,5, v 4,75-8,1. Kapının önü (u 8,57-9,56, v 4,75-5,5) boş; pencere v = 8,1 (u 8,5-9,7).
            // Karyolanın başı dip duvarda (u = 10,5); başucunda komodin (katın anahtarı çekmecesinde), öbür
            // yanında boş beşik. Karşı duvarda aynalı şifonyer.
            // Yerleşim yürünebilirlik denetimine göre: karyolanın başı dip duvarda (u = 10,5), komodin başucunun
            // güneyinde, önü odaya dönük (kapıdan girince 90 cm'lik şeritten ulaşılır); beşik kapının batısında,
            // şifonyer Oda B duvarında. Önceki iki yerleşimde odaya ya da komodinin önüne girilemiyordu.
            D("Karyola_K3", 10.50f - 1.005f, 0f, 7.12f, -1f, 0f),   // pencere perdesinin fırfırına değmesin (çakışma testi)
            DA("Komodin_Ayla", "Komodin_K3", 10.50f - 0.23f, 0f, 6.08f, -1f, 0f),   // karyolayla birlikte kaydı (çakışma testi)
            D("Besik_K3", 7.70f + 0.26f, 0f, 5.40f, 1f, 0f),
            D("Sifonyer_K3", 7.70f + 0.26f, 0f, 6.90f, 1f, 0f),
            D("Perde_OdaA_K3", 9.10f, 2.30f, 8.10f, 0f, -1f),
            D("Kilim_K3", 9.30f, 0f, 5.95f, 0f, 1f),
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s3 = z.Cozumle(odalar);
        if (!s3.Cozulebilir) Debug.LogError("[ZINCIR] 3. kat zinciri çözülemiyor: " + string.Join("; ", s3.Hatalar));
        else Debug.Log("[ZINCIR] 3. kat zinciri çözülebilir: " + string.Join(" -> ", s3.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
