// GrayboxKurucu_Kat6.cs
// 6. katın içerik dosyası (Assets/Zincirler/Kat_6.asset): iki daire, oda oda döşeme ve kilit zinciri (#83).
//
// ---------- 6. KAT: ÇİFT DAİRE + SIRALAMA / YERLEŞTİRME (#70, #66), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// Daire 3 (kapıda 21, "A"): 1974'te donup kalmış bir ev; kimse oturmuyor ama salonda dört kişilik sofra kurulmayı
// bekliyor; her şey tozlu. Daire 4 (kapıda 22, "B:" önekli yollar): terzi Sabri Usta'nın evi ve atölyesi; karanlıkta
// mankenler insan gibi durur. Sofrada dört isim kartı (Rıza Efendi, Hasan, Cemal, Yeni Kapıcı); isimli dört tabak
// iki daireye dağılmış; hepsi doğru yere konunca büfe açılır, içinde katın anahtarı. Büfedeki anahtar alınınca
// saklanma anı (#58). Kurallar: katlarda aynı model yok; ipucu notu yok (Cemal'in "kol" ve "takvim" notları
// kaldırıldı); bir odada en fazla bir görev parçası.
//
// Kapı sırası ve görev parçaları (altı kilit; iki daire birbirinin kilidini açar):
//   A Hol     : duvar takviminde 1974 -> B'nin kilerinin şifresi. (#110: 1994'tü; bu daire 1974'te donup kalmış
//               bir ev, takvimi de o yılda durmuş. Dokuzuncu katın EKİM 1974'ü ile aynı yıl.)
//   A Kiler   : (kilitsiz) duvardaki kol -> B'nin yatak odasının sürgüsü.
//   A Mutfak  : (anahtar, B'nin buzdolabında) tezgâh dolabında Rıza Efendi'nin tabağı.
//   A Oda A   : (kırık anahtar) çeyiz sandığında Cemal'in tabağı.
//   A Salon   : sofra ve büfe (yerleştirme) -> katın anahtarı.
//   B Antre   : giriş kapısının arkasındaki köşede banyo anahtarı (#114: banyo menteşeyle açılıyordu; tornavida
//               1. kattan beri çantada olduğu için 5-8. katlarda dört banyo tek tıkla açılıyordu).
//   B Salon   : terzi masasındaki dikiş kutusunda A'nın yatak odası anahtarının ucu.
//   B Mutfak  : buzdolabında A'nın mutfak anahtarı.
//   B Kiler   : (şifre 1974) raftaki düğme tenekesinde A'nın yatak odası anahtarının başı.
//   B Banyo   : (anahtar, antrede) yerde Hasan'ın tabağı.
//   B Oda A   : (A'nın kilerindeki kol) yerde Yeni Kapıcı'nın tabağı.
//   A banyo, A Oda B ve B Oda B kapalı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // #114: B'nin banyo anahtarının B antresindeki yeri (plan u, v). A'nın antresi boş: SaklamaYeriAra orada
    // hiçbir yer bulmadı. Burası giriş kapısının arkası, dar açı: ayakta 11 / eğilmiş 15 noktadan görünür, s276.
    const float B6AntreU = 0.12f, B6AntreV = -0.48f;
    // kat6_esyalar.py raporundan: sandığın, dikiş kutusunun ve düğme tenekesinin iç tabanı; buzdolabının rafı;
    // terzi masasının üstü; takvimdeki boş yazı alanının merkezi (Unity eksenlerinde).
    const float Sandik6Taban = 0.084f;
    const float DikisKutusu6Taban = 0.016f;
    const float Teneke6Taban = 0.008f;
    const float Buzdolabi6Raf = 0.504f;   // iki tel rafın alttakinin üstü (0,50)
    const float TerziMasasi6Ustu = 0.855f;
    static readonly Vector3 Takvim6Yazi = new Vector3(0f, -0.02f, 0.006f);
    static readonly Vector3 Buzdolabi6Isik = new Vector3(0f, 1.00f, -0.10f);

    static void Kat6SablonlariniKur()
    {
        foreach (string ad in new[] { "Sapka_Askisi_K6", "Telefon_Masasi_K6", "Yolluk_K6", "Takvim_1994_K6", "Avize_K6", "Perde_K6",
                                      "Pikap_K6", "Eski_TV_K6", "Tel_Dolap_K6", "Ocak_K6", "Mutfak_Masasi_K6", "Bos_Raf_K6", "Karyola_K6",
                                      "Manken_K6", "Endustriyel_Makine_K6", "Kumas_Toplari_K6", "Terzi_Masasi_K6", "Elbise_Askisi_K6",
                                      "Raf_K6", "Camasir_Sepeti_K6", "Havlu_K6", "Boy_Aynasi_K6", "Perde_Terzi_K6", "Puf_K6" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Ceyiz_Sandigi_K6");
        AcilirSablonKur("Dikis_Kutusu_K6");
        AcilirSablonKur("Dugme_Tenekesi_K6");
        AcilirSablonKur("Buzdolabi_K6", Buzdolabi6Isik);
    }

    static KatZinciriVarligi Kat6Kur(GameObject isimKarti, GameObject golge)
    {
        string yol = $"{ZincirKlasoru}/Kat_6.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "6. kat, daire 3 (1974'te donup kalmış ev) ve 4 (terzi Sabri Usta) birlikte (sıralama / yerleştirme). Sofrada dört " +
                     "isim kartı; dört tabak iki daireye dağılmış. Altı kilit: B'nin kileri (şifre A'nın holündeki takvimde), A'nın mutfağı " +
                     "(anahtar B'nin buzdolabında), A'nın yatak odası (kırık anahtar: baş B'nin kilerinde, uç B'nin dikiş kutusunda), " +
                     "B'nin banyosu (menteşe), B'nin yatak odası (A'nın kilerindeki kol), büfe (sofra). Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        v.ikinciDaire = 4;
        v.ikinciKaplamaKati = 2;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_B", "B:Oda_B" });
        // YERLER: birinci daire (3)
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_takvim", Yol = "Takvim_Hol", Oda = "Hol" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_dolap", Yol = "Esyalar/Tezgah", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kapak_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_kol", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "a_sandik", Yol = "Ceyiz_Sandigi_A", Oda = "Oda_A", KilitYolu = "Ceyiz_Sandigi_A/Kapak" });
        for (int i = 1; i <= 4; i++)
            z.Yerler.Add(new ZincirYeri { Kimlik = "yer_" + i, Yol = "Esyalar/Sofra/Yer_" + i, Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "bufe", Yol = "Esyalar/Bufe", Oda = "Salon", KilitYolu = "Esyalar/Bufe/Kapi_1" });
        // YERLER: ikinci daire (4)
        // #114: B'nin banyo anahtarı B'nin antresinde, giriş kapısının arkasındaki köşede (yer KatDenetimi ile seçildi).
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_antre_yer", PlandaNokta = true, Oda = "B:Antre", U = B6AntreU, Y = 0.004f, V = B6AntreV, Donus = 40f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_dikis", Yol = "B:Dikis_Kutusu_Sabri", Oda = "B:Salon", KilitYolu = "B:Dikis_Kutusu_Sabri/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_buzdolabi", Yol = "B:Buzdolabi_Sabri", Oda = "B:Mutfak", KilitYolu = "B:Buzdolabi_Sabri/Kapi_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_teneke", Yol = "B:Dugme_Tenekesi_Sabri", Oda = "B:Oda_C", KilitYolu = "B:Dugme_Tenekesi_Sabri/Kapak" });
        // #102 (kat kat zorlaşma): yer, KatDenetimi.SaklamaYeriAra'nın ölçtüğü adaylardan seçildi; parantezde
        // ayakta/eğilmiş görüldüğü nokta sayısı (20 cm ızgara, 5 m). Eski yer açıktaydı.
        // Hasan'ın tabağı banyonun köşesinde (1/19): banyonun ortasındaydı (47 noktadan açık).
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_banyo_yer", PlandaNokta = true, Oda = "B:Banyo", U = 3.02f, Y = 0.004f, V = 0.32f, Donus = -15f });
        // Kapıcının tabağı yatağın altında (0/78): odanın yerindeydi (77 noktadan açık).
        z.Yerler.Add(new ZincirYeri { Kimlik = "b_odaA_yer", PlandaNokta = true, Oda = "B:Oda_A", U = 10.22f, Y = 0.004f, V = 6.47f, Donus = 35f });

        // EŞYALAR
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "b_banyo_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "b_antre_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "a_mutfak_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "b_buzdolabi", X = 0.05f, Y = Buzdolabi6Raf, Z = -0.04f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "b_dikis", Y = DikisKutusu6Taban, Donus = 60f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "b_teneke", Y = Teneke6Taban, Donus = -25f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_riza", Tur = EsyaTuru.Tabak, Yer = "a_dolap", X = 0f, Y = 0.475f, Z = 0.02f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_cemal", Tur = EsyaTuru.Tabak, Yer = "a_sandik", Y = Sandik6Taban });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_hasan", Tur = EsyaTuru.Tabak, Yer = "b_banyo_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tabak_kapici", Tur = EsyaTuru.Tabak, Yer = "b_odaA_yer" });
        // Büfenin sol dolabının tabanı; sol kapı Unity'de +x tarafında (Blender -x).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "bufe", X = 0.35f, Y = BufeTaban, Donus = 15f });

        // KİLİTLER
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "B:Oda_C", Gerekli = "a_takvim",
                                          Sifre = "1974", IpucuMetni = "1974", IpucuX = Takvim6Yazi.x, IpucuY = Takvim6Yazi.y, IpucuZ = Takvim6Yazi.z });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "a_mutfak", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "a_mutfak_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "a_odaA", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Oda_A",
                                          Gerekenler = new List<string> { "odaA_anahtari_bas", "odaA_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_banyo", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "B:Banyo", Gerekli = "b_banyo_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "b_odaA", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "B:Oda_A", Gerekli = "a_kol" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "bufe", Tur = KilitTuru.Yerlestirme, OdaKapisi = false, Hedef = "bufe",
                                          Gerekenler = new List<string> { "tabak_riza", "tabak_hasan", "tabak_cemal", "tabak_kapici" },
                                          Yuvalar = new List<string> { "yer_1", "yer_2", "yer_3", "yer_4" } });
        v.zincir = z;

        // İSİMLER: tabakların üstünde ve sofradaki kartlarda (metin tablosundan).
        v.esyaYazilari = new List<EsyaYazisi>
        {
            new EsyaYazisi { Kimlik = "tabak_riza", Metin = "bina.isim_riza", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_hasan", Metin = "bina.isim_hasan", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_cemal", Metin = "bina.isim_cemal", Boy = 0.0019f },
            new EsyaYazisi { Kimlik = "tabak_kapici", Metin = "bina.isim_kapici", Boy = 0.0019f },
        };
        // Kartlar masanın kenarında, her tabak yerinin önünde; ön yüzü o yerin sandalyesine bakar.
        KatSusu Kart(float x, float zz, float donus, string metin)
        { return new KatSusu { Sablon = isimKarti, Yol = "Esyalar/Sofra", X = x, Y = 0.757f, Z = zz, Donus = donus, Metin = metin }; }
        const string B = KilitZinciri.IkinciOnek;
        v.suslar = new List<KatSusu>
        {
            Kart(-0.35f, 0.40f, 0f, "bina.isim_riza"),
            Kart(0.35f, 0.40f, 0f, "bina.isim_hasan"),
            Kart(-0.35f, -0.40f, 180f, "bina.isim_cemal"),
            Kart(0.35f, -0.40f, 180f, "bina.isim_kapici"),

            // ================= A: DAİRE 3 (1974) =================
            // ANTRE (u 0-2,6, v -0,8-2,85; giriş kapısının köşesi boş): şapkalık ve telefon masası.
            D("Sapka_Askisi_K6", 2.28f, 0f, -0.48f, -1f, 0.3f),
            D("Telefon_Masasi_K6", 2.60f - 0.18f, 0f, 1.30f, -1f, 0f),
            // HOL (u 0-10,5, v 2,95-4,65): yolluk; banyo ile kiler kapıları arasında takvim (1974); fotoğraf.
            D("Yolluk_K6", 5.60f, 0f, 3.80f, 0f, 1f),
            DA("Takvim_Hol", "Takvim_1994_K6", 4.60f, 1.45f, 2.95f, 0f, 1f),
            Foto("kat6", true, 3.80f, 1.60f, 4.65f, 0f, -1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),
            // SALON (u 0-5,0, v 4,75-8,1): ortada dairenin sofrası, landing duvarında büfe (ikisi de kalıcı eşya);
            // üstte avize; Oda B duvarında pikap ve üstünde çerçeve; balkon kapısının yanında ayaklı eski TV.
            D("Avize_K6", 2.80f, Tavan, 6.30f, 0f, 1f),
            D("Perde_K6", 1.40f, 2.30f, PetekOnuPerde, 0f, -1f),
            D("Kornis_Dirsegi", 1.40f - 0.92f, 2.288f, 8.10f, 0f, -1f),   // korniş dirsekleri (perde duvardan 22 cm önde)
            D("Kornis_Dirsegi", 1.40f + 0.92f, 2.288f, 8.10f, 0f, -1f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),
            D("Pikap_K6", 5.00f - 0.28f, 0f, 6.30f, -1f, 0f),
            M("fancy_picture_frame_01", 5.00f, 1.60f, 6.30f, -1f, 0f),
            D("Eski_TV_K6", 4.40f, 0f, 7.62f, -0.5f, -1f),
            // (Pikabın üstündeki ayaklı çerçeve kaldırıldı: açık kapaklı pikabın gövdesine gömülüyordu, çakışma testi.)
            // MUTFAK (u 6,8-10,5, v -0,8-2,85; kapının önü boş): tel dolap, emaye ocak, küçük masa.
            D("Tel_Dolap_K6", 6.80f + 0.21f, 0f, 0.90f, 1f, 0f),
            D("Ocak_K6", 8.00f, 0f, -0.80f + 0.24f, 0f, 1f),
            D("Mutfak_Masasi_K6", 8.60f, 0f, 1.30f, 0f, 1f),
            // KİLER (u 4,6-6,7): banyo duvarında boş raf; mutfak duvarında dairenin kolu (u 6,7 v 1,2).
            D("Bos_Raf_K6", 4.60f + 0.18f, 0f, 0.30f, 1f, 0f),
            // ODA A (u 7,7-10,5, v 4,75-8,1; kapının önü boş): demir karyola, batı duvarında çeyiz sandığı.
            // Yürünebilirlik denetimine göre: karyola (1,48 m geniş) başı doğu duvarında, boyuna pencere
            // duvarı boyunca yatar; güneyinde kapıdan 1,9 m derin boş alan kalır. Sandık batı duvarında, önü
            // odaya dönük. (Başı pencere duvarındayken açık kapı kanadının ucuyla karyolanın köşesi arasından
            // geçilemiyordu; sandık ayak ucundayken kapının önünü kapatıyordu, odada 0,3 m² yürünüyordu.)
            D("Karyola_K6", 10.50f - 1.02f, 0f, 8.10f - 0.76f, -1f, 0f),
            DA("Ceyiz_Sandigi_A", "Ceyiz_Sandigi_K6", 7.70f + 0.25f, 0f, 6.00f, 1f, 0f),

            // ================= B: DAİRE 4 (terzi Sabri Usta) =================
            // HOL: koridorun dibinde bir manken. #96: adı var ve yüzü DUVARA dönük (kuzey duvarı, +v); katın anında
            // oyuncu her gözünü ayırdığında ona döner (eskiden girene dönüktü; duvardan dönmesi ilk dönüşü belli eder).
            DA("Manken_Hol_Sabri", "Manken_K6", 10.05f, 0f, 3.80f, 0f, 1f, B),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f, B),
            // SALON (atölye): pencerenin önünde sanayi makinesi, ortada kesim masası (üstünde dikiş kutusu),
            // Oda B duvarında elbise askısı, köşelerde mankenler ve kumaş topları.
            D("Endustriyel_Makine_K6", 1.60f, 0f, 8.10f - 0.28f - 0.13f, 0f, -1f, B),   // kısa perde arkasından sarksın (çakışma testi)
            D("Terzi_Masasi_K6", 2.80f, 0f, 6.20f, 0f, 1f, B),   // makineyle arasında yol kalsın (yürünebilirlik)
            DA("Dikis_Kutusu_Sabri", "Dikis_Kutusu_K6", 3.25f, TerziMasasi6Ustu, 6.10f, 0f, 1f, B),
            D("Elbise_Askisi_K6", 5.00f - 0.26f, 0f, 5.85f, -1f, 0f, B),
            D("Manken_K6", 0.75f, 0f, 6.25f, 1f, 0f, B),
            D("Manken_K6", 3.60f, 0f, 7.30f, -0.3f, -1f, B),
            D("Kumas_Toplari_K6", 4.45f, 0f, 7.50f, -1f, 0f, B),   // duvara ve mankene gömülüyordu (çakışma testi)
            D("Perde_Terzi_K6", 1.40f, 2.30f, 8.10f, 0f, -1f, B),   // atölyede petek yok: perde duvarda
            // MUTFAK: köşede buzdolabı (A'nın mutfak anahtarı), ortada küçük masa.
            DA("Buzdolabi_Sabri", "Buzdolabi_K6", 7.20f, 0f, -0.80f + 0.30f, 0f, 1f, B),
            M("small_wooden_table_01", 8.50f, 0f, 1.30f, 0f, 1f, B),
            // KİLER: banyo duvarında metal raf (rafları kumaş kutularıyla dolu); düğme tenekesi rafın ucunda yerde.
            D("Raf_K6", 4.60f + 0.21f, 0f, 0.30f, 1f, 0f, B),
            DA("Dugme_Tenekesi_Sabri", "Dugme_Tenekesi_K6", 4.85f, 0f, 1.25f, 1f, 0.3f, B),
            // BANYO (lavabo ve klozet dairenin parçaları): çamaşır sepeti, havlu.
            D("Camasir_Sepeti_K6", 4.25f, 0f, 0.20f, -1f, 0f, B),
            D("Havlu_K6", 2.70f, 1.30f, 1.10f, 1f, 0f, B),
            // ODA A: dip duvarda gündüz yatağı, karşısında prova aynası ve puf.
            M("vintage_day_bed", 10.50f - 0.43f - 0.02f, 0f, 6.60f, -1f, 0f, B),
            D("Boy_Aynasi_K6", 7.70f + 0.21f, 0f, 6.50f, 1f, 0f, B),
            D("Puf_K6", 7.70f + 0.32f, 0f, 7.60f, 1f, 0f, B),   // 70'ler kumaş puf (eskiden modern deri puf, #81)   // köşede: odanın ortasındaki geçidi daraltıyordu (eksikler testi)
            D("Perde_Terzi_K6", 9.10f, 2.30f, 8.10f, 0f, -1f, B),
        };
        v.notlar = new List<KatNotu>
        {
            // A'nın antresinde Yönetim'in davet notu (hikâye; misafir gelince ortada durma).
            new KatNotu { Metin = "not.kat6_davet", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        // Saklanma anı (#58): büfedeki anahtar alınınca kapı tıklatılır, Gölge holde devriye gezer.
        v.saklanmaVar = true;
        v.kacisSablonu = golge;
        v.saklanmaSesi = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Tiklatma.wav");
        v.parcalar = new List<KatParcasi>();
        v.sesler = new List<KatSesi>();
        v.anons = new KatAnonsu { Yol = "Sus_Eski_TV_K6", Gecikme = 28f, Dosya = "radyo_kat6" };   // #106: 1974 dairesinin eski televizyonu (A dairesi)
        v.yananOdalar = new List<string>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah", "Klozet", "Lavabo" };

        // ---------- KATIN ANI (#96): BAKMAYINCA DÖNEN MANKEN ----------
        // B dairesinin holüne girilince (Sabri Usta'nın evi) koridorun dibindeki manken, oyuncu onu gördükten sonra
        // her gözünü ondan ayırdığında yüzünü oyuncuya çevirir; dört kez. Dönerken hiç görülmez, yalnızca ayağının
        // tahtada sürtündüğü duyulur. Mankenin yüzü başta duvara dönük (yukarıdaki süs).
        v.anVar = true;
        v.an = new KatAniVerisi
        {
            Tur = KatAniTuru.BakmayincaDon, Yol = B + "Manken_Hol_Sabri", TetikOda = B + "Hol", Gecikme = 1f,
            Sayi = 4, Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anlar/An_Surtunme.wav"), Ses = 0.45f,
        };
        // An_Surtunme (#99): mankenin ahşap ayağının döşemede dönmesi. Yoksa eski parke gıcırtısı.
        if (v.an.Klip == null) v.an.Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sesler/Gicirti_2.wav");
        if (v.an.Klip == null) Debug.LogError("[ZINCIR] mankenin sesi bulunamadı.");

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) { odalar.Add(o.Ad); odalar.Add(KilitZinciri.IkinciOnek + o.Ad); }
        KilitZinciri.Sonuc s6 = z.Cozumle(odalar);
        if (!s6.Cozulebilir) Debug.LogError("[ZINCIR] 6. kat zinciri çözülemiyor: " + string.Join("; ", s6.Hatalar));
        else Debug.Log("[ZINCIR] 6. kat zinciri çözülebilir: " + string.Join(" -> ", s6.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
