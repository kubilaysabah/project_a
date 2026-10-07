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
//   Antre     : yerdeki okul çantasının üstünde SALONUN anahtarı (#116, B8: 3. kat iki kilitliydi; 4. kattan 5. kata
//               kilit sayısı birden sıçrıyordu).
//   Banyo     : (kilitsiz, #116'dan önce kapalıydı) yerde 2. parça.
//   Salon     : (anahtar) kanepenin altında 3. parça (eğilince görülür, #79).
//   Mutfak    : tezgâhın üst çekmecesinde Oda B'nin (Kerem'in odası) anahtarı.
//   Oda B     : (anahtar) oyuncak sandığında 4. parça -> resim tamam.
//   Oda A     : (şifreli asma kilit 0309) komodinin çekmecesinde katın anahtarı.
//   Hol       : Kerem'in duvardaki resimlerinin altında, yerde 1. parça (#115: sahanlıktaki ayakkabılıktaydı; oyunda
//               dairenin dışına konmuş tek görev eşyasıydı ve oyuncu iki kattır sahanlığı süs diye öğrenmişti).
//               Annenin notu da burada; banyo ve kiler kapalı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // #115: 1. çizim parçasının holdeki yeri (plan u, v): Kerem'in resimlerinin (u 3,8, kuzey duvarı) altı.
    const float Banyo3U = 3.02f, Banyo3V = 0.32f;   // #116: 2. parçanın banyodaki yeri: dar açı, ayakta 4 / eğilmiş 19 nokta, s74
    const float Hol3U = 3.55f, Hol3V = 4.57f;   // duvarın dibi; hol uzun bir koridor, gizli köşesi yok (resimlerin altı 417 noktadan görünür; 3. katta zorlaşma kuralı yok)
    // 3. katın Blender modelleri (Art/Blender/scripts/kat3_esyalar.py): şablonları kurulur.
    static void Kat3SablonlariniKur()
    {
        foreach (string ad in new[] { "Askilik_K3", "Ayakkabi_Yigini_K3", "Okul_Cantasi_K3", "Bebek_Arabasi_K3", "Terlikler_K3",
                                      "Cocuk_Cizimleri_K3", "Dagilmis_Oyuncaklar_K3", "Yolluk_K3",
                                      "Tuplu_TV_K3", "Boyama_Sehpasi_K3", "Oyun_Parki_K3", "Hali_K3", "Perde_K3", "Duvar_Rafi_K3",
                                      "Mama_Sandalyesi_K3", "Masa_K3", "Sandalye_K3", "Buzdolabi_K3", "Ocak_K3", "Camasir_Makinesi_K3", "Biberon_Takimi_K3",
                                      "Cocuk_Yatagi_K3", "Calisma_Masasi_K3", "Poster_K3", "Yerdeki_Cizimler_K3", "Gece_Lambasi_K3", "Perde_Cocuk_K3",
                                      "Karyola_K3", "Besik_K3", "Sifonyer_K3", "Perde_OdaA_K3", "Kilim_K3",
                                      "Kanepe_K3", "Cocuk_Sandalyesi_K3" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Komodin_K3");
        // Saklanılabilen gardırop (#97): iki kapaklı, kapakları açılır (Gardirop_K3.fbx); adında "Gardirop"
        // geçtiği için daire açılınca SaklanmaYeri de eklenir (SaklanmaKurali).
        AcilirSablonKur("Gardirop_K3");
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
        z.KapaliOdalar.AddRange(new[] { "Oda_C" });   // #116: banyo açıldı (2. parça orada)
        // 1. parça holde, çocuk resimlerinin altında yerde (yer KatDenetimi.SaklamaYeriAra ile seçildi, #115).
        z.Yerler.Add(new ZincirYeri { Kimlik = "hol_resim", PlandaNokta = true, Oda = "Hol", U = Hol3U, Y = 0.004f, V = Hol3V, Donus = 15f });
        // Okul çantası antrede yerde (u 1,9, v 1,9); parça onun üst yüzünde.
        z.Yerler.Add(new ZincirYeri { Kimlik = "canta", PlandaNokta = true, Oda = "Antre", U = 1.975f, Y = CantaUstu3, V = 1.90f, Donus = -20f });
        // Kanepe (Kanepe_K3) Oda B duvarının önünde, altı açık. Parça kanepenin altında, duvara yakın:
        // ayakta görülmez, eğilince görülür; oyunun ilk "eğil ve bak" anı (#79).
        // #102 (kat kat zorlaşma): yer, KatDenetimi.SaklamaYeriAra'nın ölçtüğü adaylardan seçildi; parantezde
        // ayakta/eğilmiş görüldüğü nokta sayısı (20 cm ızgara, 5 m). Eski yer açıktaydı.
        // Eski yer u 4,31 v 6,45: kanepenin ön kenarına yakındı, ayakta 44 noktadan görülüyordu. Yeni: 0/111.
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", PlandaNokta = true, Oda = "Salon", U = 4.72f, Y = 0.004f, V = 6.47f, Donus = 25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Komodin_Ayla/Cekmece_1", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p1", Tur = EsyaTuru.Parca, Yer = "hol_resim" });
        // #116: çantanın üstünde salonun anahtarı; 2. parça banyoya taşındı (bir odada tek görev parçası).
        z.Yerler.Add(new ZincirYeri { Kimlik = "banyo_yer", PlandaNokta = true, Oda = "Banyo", U = Banyo3U, Y = 0.004f, V = Banyo3V, Donus = -30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "canta", Donus = 25f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p2", Tur = EsyaTuru.Parca, Yer = "banyo_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p3", Tur = EsyaTuru.Parca, Yer = "salon_yeri" });
        // Çekmecenin içi: tezgâhın ekseninde x 0,59, y 0,64, z 0,04 (tezgah.py'deki üst çekmecenin tabanı).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p4", Tur = EsyaTuru.Parca, Yer = "oyuncak", Y = SandikTaban, Donus = -20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.005f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Salon", Gerekli = "salon_anahtari" });   // #116
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaB_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Sifre = "0309",
                                          Parcalar = new List<string> { "p1", "p2", "p3", "p4" } });
        v.zincir = z;

        // PARÇALAR: dokular Art/Cizim/scripts/cizim_uret.py'den. Her parçanın dokudaki bölgesi
        // dosyanın saydam olmayan piksellerinden hesaplanır: resim yeniden üretilince de doğru kalır.
        v.parcalar = new List<KatParcasi>();
        // Kat imzası ışığı (#116): holdeki dağılmış oyuncakların arasında pili bitmemiş polis arabası, kırmızı-mavi.
        v.isiklar = new List<KatIsigi>
        {
            new KatIsigi { Yol = "Sus_Dagilmis_Oyuncaklar_K3", X = 0f, Y = 0.06f, Z = 0f, Renk = new Color(1f, 0.12f, 0.10f),
                           Renk2 = new Color(0.15f, 0.30f, 1f), Siddet = 0.6f, Menzil = 2.0f, Titreme = KatIsikTitremesi.Yanip,
                           AlevBoyu = new Vector3(0.016f, 0.010f, 0.016f), KaynakParlakligi = 3f },
        };
        for (int i = 1; i <= 4; i++)
        {
            string dokuYolu = $"Assets/Textures/Cizim/Cizim_Kat3_P{i}.png";
            CizimDokusunuAyarla(dokuYolu);
            v.parcalar.Add(new KatParcasi { Kimlik = "p" + i, Doku = AssetDatabase.LoadAssetAtPath<Texture2D>(dokuYolu), Bolge = ParcaBolgesi(dokuYolu) });
        }
        v.cizimBoyu = 0.30f;
        v.sesler = new List<KatSesi>();
        v.anons = new KatAnonsu { Yol = "Sus_Tuplu_TV_K3", Gecikme = 22f, Dosya = "radyo_kat3" };   // #106: salondaki tüplü TV
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah", "Lavabo", "Klozet" };   // #116: banyo açık

        // Annenin notu hikâye notudur (şifreyi söylemez): koridorda, Oda B'nin kapısının solunda.
        v.notlar = new List<KatNotu>
        {
            // İlan panosundaki Yönetim duyurusu (#57): daireye NEDEN girildiğini anlatır; nerede ne aranacağını asla söylemez.
            new KatNotu { Metin = "not.kat3_pano", Yol = "Sahanlik:Ilan", X = PanoNotX, Y = PanoNotY, Z = PanoNotZ },
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
            // Ailenin hol dolabı (#97): holün dibinde, güney duvarına (v = 2,95) sırtını dayamış, kuzeye (+v) bakar;
            // içinde saklanılabilir. u 9,09-10,07: doğu kapağının ucu açılınca 16 cm taşar (10,23'e kadar); dip
            // duvardaki peteğin ön yüzü u 10,29'da (duvardan 21 cm), kapak ona değmez. Önünde Oda A kapısına giden
            // 1,1 m'lik geçit açık kalır.
            DA("Gardirop_Murat", "Gardirop_K3", 9.58f, 0f, 2.95f, 0f, 1f),

            // ---------- SALON ----------
            // Salon u 0-5,0, v 4,75-8,1. Kapı hol duvarında (u 0,71-1,81 önü boş); karşıda pencere (u 0,7-2,1)
            // ve hep kilitli balkon kapısı (u 3,2-4,0). Kanepe Oda B duvarında, hol tarafında; karşısında TV dolabı.
            // Şahan: "salonu biraz genişlet" (#83). Oyun parkı odanın ortasından kanepenin kuzeyine, pencere
            // duvarının köşesine; kanepe ve önündeki boyama sehpası hol tarafına kaydı: salonun ortası ve pencere
            // önü boş kaldı. Kerem'in çizim parçası (salon_yeri, v 6,45) yine kanepenin altında.
            // 90'lar kumaş kanepe (Blender; eskiden Poly Haven'ın siyah deri chesterfield'ı modern duruyordu, #81).
            // Altı açık: Kerem'in çizim parçası altında yatar.
            D("Kanepe_K3", 5.00f - 0.41f - 0.02f, 0f, 5.905f, -1f, 0f),
            // (Poly Haven zikzak yastıklar kaldırıldı: desenleri modern duruyordu ve yeni kanepenin kendi sırt
            // minderlerinin içine giriyordu, #81.)
            M("painted_wooden_cabinet", 0.03f + 0.31f + 0.01f, 0f, 6.45f, 1f, 0f),
            D("Tuplu_TV_K3", 0.36f, 1.18f, 6.45f, 1f, 0f),
            M("Chandelier_01", 2.50f, Tavan, 6.45f, 0f, 1f),
            D("Hali_K3", 2.80f, 0f, 6.45f, 0f, 1f),
            D("Boyama_Sehpasi_K3", 3.40f, 0f, 5.905f, 1f, 0f),
            D("Oyun_Parki_K3", 4.45f, 0f, 7.55f, 0f, -1f),   // köşede, kanepenin kuzeyinde
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
            D("Cocuk_Sandalyesi_K3", 5.95f, 0f, 6.10f, -1f, 0f),   // ahşap çocuk sandalyesi (eskiden plastik okul sandalyesi, #81)
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
            // Beşiğin adı var (#96): katın anında kendiliğinden sallanır.
            DA("Besik_Ayla", "Besik_K3", 7.70f + 0.26f, 0f, 5.40f, 1f, 0f),
            D("Sifonyer_K3", 7.70f + 0.26f, 0f, 6.90f, 1f, 0f),
            D("Perde_OdaA_K3", 9.10f, 2.30f, 8.10f, 0f, -1f),
            D("Kilim_K3", 9.30f, 0f, 5.95f, 0f, 1f),
        };

        // ---------- KATIN ANI (#96): SALLANAN BEŞİK ----------
        // Şifreyle açılan son oda anne-babanın odası. Oyuncu girdikten 1,5 sn sonra kapının yanındaki boş beşik
        // kendiliğinden sallanır: içinde bebek yok, kimse itmiyor. Yedi salınım, her biri 1,5 sn, 7°'den sönerek.
        // Beşiğin kızakları uzun kenarının iki ucunda, yerel YZ düzleminde (kat3_esyalar_2.py): beşik yerel X
        // ekseninde (uzun ekseninde) sallanır. Kızağın alt kavisi z = 0,96 y² parabolü; tabanındaki eğrilik
        // yarıçapı 1 / (2 × 0,96) ≈ 0,52 m. Dönme noktası o çemberin merkezi (yerden 0,52 m): kızak yere
        // değerek sallanır, havaya kalkmaz.
        v.anVar = true;
        v.an = new KatAniVerisi
        {
            Tur = KatAniTuru.Sallanma, Yol = "Besik_Ayla", TetikOda = "Oda_A", Gecikme = 1.5f,
            Sayi = 7, Genlik = 7f, Sure = 1.5f, Pivot = new Vector3(0f, 0.52f, 0f),
            Klip = AnSesi("An_Gicirti"), Ses = 0.5f,
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
