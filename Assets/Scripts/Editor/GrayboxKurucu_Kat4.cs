// GrayboxKurucu_Kat4.cs
// 4. katın içerik dosyası (Assets/Zincirler/Kat_4.asset): oda oda döşeme ve kilit zinciri (#83).
//
// ---------- 4. KAT: IŞIK VE KARANLIK (#64), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// Daire 4, boş ve elektriksiz. Elektriği önceki kapıcı Hasan kesmiş, sigortayı saklamış; kendisi bir süre
// bu dairede karanlıkta gece geçirmiş (Oda B: kamp yatağı, mumlar, gaz lambası, duvarda gün sayma çizgileri).
// Kurallar: katlarda aynı model yok; ipucu notu yok (Hasan'ın notu artık yol göstermiyor); bir odada en
// fazla bir görev parçası.
//
// Kapı sırası ve görev parçaları:
//   Salon  : (kilitsiz) Oda B duvarında fosforlu boyayla 4816: yalnızca kafa lambası kapalıyken görünür.
//   Oda B  : (şifreli asma kilit 4816) Hasan'ın alet kutusunda sigorta.
//   Antre  : sigorta kutusu (dairenin kalıcı parçası): sigorta takılınca elektrik gelir, lambalar yanar.
//   Hol    : yerde, güney duvarının dibinde mutfağın anahtarı; fosforlu anahtarlığı lamba kapalıyken parlar (#116, B8).
//   Mutfak : (anahtar) elektrik gelince buzdolabının kapısı açılır: içinde Hasan'ın notu ve katın anahtarı.
//   Banyo, kiler ve Oda A kapalı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // #116: mutfak anahtarının holdeki yeri ve yanan mumun alevi (Mumlar_K4'ün içinde, ölçüldü).
    const float Hol4U = 7.60f, Hol4V = 3.10f;   // hol boş, gizli köşesi yok (SaklamaYeriAra); kolilerin dibinde eğik kartonun
                                                // arkasında kalıyordu (ne lambayla ne karanlıkta görünüyordu); güney duvarının dibi
    const float Mum4X = 0f, Mum4Ust = 0.150f, Mum4Z = 0f;   // en uzun mumun fitili 0,134 (ışınla ölçüldü)
    // Buzdolabının iç ışığının yeri (Blender kat4_esyalar.py'nin iç lamba noktası, Unity eksenlerinde).
    static readonly Vector3 Buzdolabi4Isik = new Vector3(-0.19f, 1.24f, -0.05f);
    // Buzdolabının orta rafının üstü ve alet kutusunun iç tabanı (kat4_esyalar.py raporu).
    const float Buzdolabi4Raf = 0.758f;   // üç tel rafın ortancası
    const float AletKutusu4Taban = 0.006f;

    static void Kat4SablonlariniKur()
    {
        foreach (string ad in new[] { "Gazete_Yigini_K4", "Sarkan_Kablo_K4", "Bos_Koliler_K4", "Ortulu_Koltuk_K4", "Mumlar_K4",
                                      "Yikik_Sehpa_K4", "Kova_Paspas_K4", "Kirik_Sandalye_K4", "Kamp_Yatagi_K4", "Termos_K4",
                                      "Gaz_Lambasi_K4", "Duvar_Tebesir_K4", "Radyo_Cep_K4" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Buzdolabi_K4", Buzdolabi4Isik);
        AcilirSablonKur("Alet_Kutusu_K4");
        // Saklanılabilen gardırop (#97): iki kapaklı (Gardirop_K4.fbx); adında "Gardirop" geçtiği için
        // daire açılınca SaklanmaYeri de eklenir (SaklanmaKurali).
        AcilirSablonKur("Gardirop_K4");
    }

    static KatZinciriVarligi Kat4Kur(GameObject golge)
    {
        string yol = $"{ZincirKlasoru}/Kat_4.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "4. kat, daire 4, boş ve elektriksiz (ışık ve karanlık). Oda B'nin şifresi salon duvarında fosforlu boyayla: " +
                     "yalnızca kafa lambası kapalıyken görünür. Oda B'de Hasan'ın alet kutusunda sigorta; takılınca lambalar yanar ve " +
                     "buzdolabı açılır, içinde Hasan'ın notu ve katın anahtarı. Banyo, kiler ve Oda A kapalı. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_A", "Oda_C" });
        // Salonla Oda B arasındaki duvar: duvarlar odanın dışına örülür, salon tarafındaki yüzü u = 5,0.
        // Yazı yüzün yarım santim önünde (4,5 cm öndeyken havada duruyordu: eksikler testi), salona (-u) bakar.
        z.Yerler.Add(new ZincirYeri { Kimlik = "duvar", PlandaNokta = true, Oda = "Salon", U = 4.995f, Y = 1.60f, V = 7.2f, OnU = -1f, OnV = 0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kutu", Yol = "Alet_Kutusu_Hasan", Oda = "Oda_B", KilitYolu = "Alet_Kutusu_Hasan/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Buzdolabi_Hasan", Oda = "Mutfak", KilitYolu = "Buzdolabi_Hasan/Kapi_1" });
        // #116: mutfağın anahtarı holde, güney duvarının dibinde (yer ölçülerek seçildi); fosforlu anahtarlıklı.
        z.Yerler.Add(new ZincirYeri { Kimlik = "hol_yer", PlandaNokta = true, Oda = "Hol", U = Hol4U, Y = 0.004f, V = Hol4V, Donus = 35f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "kutu", Y = AletKutusu4Taban, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "mutfak_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "hol_yer", KaranliktaParlar = true });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "buzdolabi", X = 0.06f, Y = Buzdolabi4Raf + 0.004f, Z = -0.03f, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "duvar",
                                          Sifre = "4816", IpucuMetni = "4816", IpucuOlcek = 4f, IpucuKaranlikta = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "mutfak_anahtari" });   // #116
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "buzdolabi", Tur = KilitTuru.Sigorta, OdaKapisi = false, Hedef = "buzdolabi", Gerekli = "sigorta" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Hasan'ın ilk notu antrede, sigorta kutusunun yanında (hikâye: elektriği o kesti).
            new KatNotu { Metin = "not.kat4_giris", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            // İkinci notu buzdolabının rafında, anahtarın yanında; buzdolabının iç ışığı onu aydınlatır.
            new KatNotu { Metin = "not.kat4_buzdolabi", Yol = "Buzdolabi_Hasan", X = -0.10f, Y = Buzdolabi4Raf + 0.005f, Z = -0.06f, Donus = -6f },
        };
        v.parcalar = new List<KatParcasi>();
        // Kat imzası ışığı (#116): Hasan'ın salonda yere dizdiği erimiş mumlardan biri hâlâ yanıyor. Menzil kısa:
        // Oda B duvarındaki fosforlu şifreyi (u 5,0, v 7,2; mumlardan ~2 m) aydınlatıp söndürmesin.
        v.isiklar = new List<KatIsigi>
        {
            new KatIsigi { Yol = "Sus_Mumlar_K4", X = Mum4X, Y = Mum4Ust, Z = Mum4Z, Renk = new Color(1f, 0.62f, 0.30f), Siddet = 0.9f,
                           Menzil = 1.8f, Titreme = KatIsikTitremesi.Mum, AlevBoyu = new Vector3(0.012f, 0.028f, 0.012f), KaynakParlakligi = 5f },
        };
        v.sesler = new List<KatSesi>();
        v.anons = new KatAnonsu { Yol = "Sus_Radyo_Cep_K4", Gecikme = 35f, Dosya = "radyo_kat4" };   // #106: elektriksiz dairede pilli radyo
        // Hasan'ın gardırobunun bir kapağı aralık başlar (#97): %12 açık (~13°). Boş dairede biri az önce içine
        // girip çıkmış gibi; oyuncu dokununca kapanır (AcilirParca.Aralik).
        v.acikParcalar = new List<AcikParca> { new AcikParca { Yol = "Gardirop_Hasan/Kapi_1", Oran = 0.12f } };
        v.buzdolabiFisiCekili = false;
        v.yananOdalar = new List<string> { "Antre", "Hol", "Salon", "Mutfak", "Oda_B" };
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah" };

        v.suslar = new List<KatSusu>
        {
            // ---------- ANTRE ---------- (u 0-2,6, v -0,8-2,85; kapının açıldığı köşe boş)
            // Kapının altından atılmış gazeteler; banyo duvarında tozlu, kararmış bir ayna.
            D("Gazete_Yigini_K4", 0.55f, 0f, 1.15f, 1f, 0f),
            M("ornate_mirror_01", 2.60f, 1.55f, 1.60f, -1f, 0f),

            // ---------- HOL ---------- (u 0-10,5, v 2,95-4,65)
            // Tavandan ampulsüz sarkan kablo; banyo ile kiler kapıları arasında boş koliler; eski kiracıların fotoğrafı.
            D("Sarkan_Kablo_K4", 5.20f, Tavan, 3.80f, 0f, 1f),
            D("Bos_Koliler_K4", 4.60f, 0f, 2.95f, 0f, 1f),
            Foto("kat4", true, 3.80f, 1.60f, 4.65f, 0f, -1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),

            // ---------- SALON ---------- (u 0-5,0, v 4,75-8,1; kapının önü u 0,71-1,81, v 4,75-5,6 boş)
            // Boşaltılmış salon: çarşaflı tek koltuk, yerde erimiş mumlar, ayağı kırık sehpa. Fosforlu şifre
            // Oda B duvarında (u 5,0, v 7,2) serbest kalır.
            D("Ortulu_Koltuk_K4", 2.00f, 0f, 6.80f, 1f, 0.3f),
            D("Mumlar_K4", 3.20f, 0f, 6.20f, 0f, 1f),
            D("Yikik_Sehpa_K4", 3.80f, 0f, 7.50f, 0f, -1f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),
            // Hasan'ın gardırobu (#97): Oda B duvarına (u = 5,0) sırtını dayamış, salona (-u) bakar; içinde
            // saklanılabilir (Hasan karanlıkta burada saklanmış). v 4,975-6,025: hol duvarından 22 cm uzakta
            // (kapağın ucu açılınca 12 cm taşar); fosforlu şifrenin yeri (v 7,2) serbest kalır.
            DA("Gardirop_Hasan", "Gardirop_K4", 5.00f, 0f, 5.50f, -1f, 0f),

            // ---------- MUTFAK ---------- (u 6,8-10,5, v -0,8-2,85; kapının önü boş; tezgâh dip duvarda)
            DA("Buzdolabi_Hasan", "Buzdolabi_K4", 7.20f, 0f, -0.80f + 0.32f, 0f, 1f),
            D("Kova_Paspas_K4", 8.30f, 0f, -0.50f, 0f, 1f),
            D("Kirik_Sandalye_K4", 8.60f, 0f, 1.50f, 0f, 1f),

            // ---------- ODA B (Hasan'ın gece geçirdiği oda) ---------- (u 5,1-7,6, v 4,75-8,1; kapının önü boş)
            // Oda A duvarı boyunca kamp yatağı; başucunda termos, gaz lambası ve alet kutusu; duvarda gün çizgileri.
            D("Kamp_Yatagi_K4", 7.60f - 0.37f, 0f, 6.90f, 0f, -1f),
            // #106: Hasan'ın pilli cep radyosu yatağın ayak ucunda, yatık (yatağın o ucunun üstü 0,41).
            D("Radyo_Cep_K4", 7.33f, 0.41f, 6.35f, 0f, -1f),
            DA("Alet_Kutusu_Hasan", "Alet_Kutusu_K4", 6.55f, 0f, 7.70f, 0f, -1f),
            D("Termos_K4", 6.55f, 0f, 6.30f, 1f, 0f),
            D("Gaz_Lambasi_K4", 6.20f, 0f, 7.85f, 0f, -1f),
            D("Duvar_Tebesir_K4", 5.10f, 1.00f, 6.50f, 1f, 0f),
        };

        // ---------- KATIN ANI (#96): IŞIKTA SİLUET ----------
        // Oyuncu Hasan'ın alet kutusundan sigortayı alır. Daire hâlâ elektriksiz; ama oyuncu Oda B'nin kapısına
        // döndüğü an holün lambası bir anlığına titreyerek yanar: kapının hemen önünde, holde, biri duruyor.
        // 0,6 sn sonra lamba söner; kafa lambası titrer; ışık dönünce hol boş. An, siluetin yeri oyuncunun
        // ekranına girene kadar bekler (sırtı dönükken yanan lambayı kimse görmez); oyuncu çıkmak için kapıya
        // dönmek zorunda. Siluetin yeri holde, Oda B'nin kapısının (u 5,95-6,75) tam karşısında, 1,15 m önünde.
        v.anVar = true;
        v.kacisSablonu = golge;   // siluet: kaçış ve saklanma anlarının Gölge'si
        v.an = new KatAniVerisi
        {
            Tur = KatAniTuru.IsikSiluet, Yol = "Lambalar/Lamba_Hol", TetikEsya = "sigorta", Gecikme = 0.8f,
            U = 6.35f, V = 3.55f, Sure = 0.6f, Siddet = 3f,
            Klip = AnSesi("An_Vizilti"), Ses = 0.8f,
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s4 = z.Cozumle(odalar);
        if (!s4.Cozulebilir) Debug.LogError("[ZINCIR] 4. kat zinciri çözülemiyor: " + string.Join("; ", s4.Hatalar));
        else Debug.Log("[ZINCIR] 4. kat zinciri çözülebilir: " + string.Join(" -> ", s4.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
