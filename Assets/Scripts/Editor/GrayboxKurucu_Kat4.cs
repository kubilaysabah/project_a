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
//   Mutfak : (kilitsiz) elektrik gelince buzdolabının kapısı açılır: içinde Hasan'ın notu ve katın anahtarı.
//   Hol boş; banyo, kiler ve Oda A kapalı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // Buzdolabının iç ışığının yeri (Blender kat4_esyalar.py'nin iç lamba noktası, Unity eksenlerinde).
    static readonly Vector3 Buzdolabi4Isik = new Vector3(-0.19f, 1.24f, -0.05f);
    // Buzdolabının orta rafının üstü ve alet kutusunun iç tabanı (kat4_esyalar.py raporu).
    const float Buzdolabi4Raf = 0.758f;   // üç tel rafın ortancası
    const float AletKutusu4Taban = 0.006f;

    static void Kat4SablonlariniKur()
    {
        foreach (string ad in new[] { "Gazete_Yigini_K4", "Sarkan_Kablo_K4", "Bos_Koliler_K4", "Ortulu_Koltuk_K4", "Mumlar_K4",
                                      "Yikik_Sehpa_K4", "Kova_Paspas_K4", "Kirik_Sandalye_K4", "Kamp_Yatagi_K4", "Termos_K4",
                                      "Gaz_Lambasi_K4", "Duvar_Tebesir_K4" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Buzdolabi_K4", Buzdolabi4Isik);
        AcilirSablonKur("Alet_Kutusu_K4");
    }

    static KatZinciriVarligi Kat4Kur()
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
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "kutu", Y = AletKutusu4Taban, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "buzdolabi", X = 0.06f, Y = Buzdolabi4Raf + 0.004f, Z = -0.03f, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "duvar",
                                          Sifre = "4816", IpucuMetni = "4816", IpucuOlcek = 4f, IpucuKaranlikta = true });
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
        v.sesler = new List<KatSesi>();
        v.acikParcalar = new List<AcikParca>();
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

            // ---------- MUTFAK ---------- (u 6,8-10,5, v -0,8-2,85; kapının önü boş; tezgâh dip duvarda)
            DA("Buzdolabi_Hasan", "Buzdolabi_K4", 7.20f, 0f, -0.80f + 0.32f, 0f, 1f),
            D("Kova_Paspas_K4", 8.30f, 0f, -0.50f, 0f, 1f),
            D("Kirik_Sandalye_K4", 8.60f, 0f, 1.50f, 0f, 1f),

            // ---------- ODA B (Hasan'ın gece geçirdiği oda) ---------- (u 5,1-7,6, v 4,75-8,1; kapının önü boş)
            // Oda A duvarı boyunca kamp yatağı; başucunda termos, gaz lambası ve alet kutusu; duvarda gün çizgileri.
            D("Kamp_Yatagi_K4", 7.60f - 0.37f, 0f, 6.90f, 0f, -1f),
            DA("Alet_Kutusu_Hasan", "Alet_Kutusu_K4", 6.55f, 0f, 7.70f, 0f, -1f),
            D("Termos_K4", 6.55f, 0f, 6.30f, 1f, 0f),
            D("Gaz_Lambasi_K4", 6.20f, 0f, 7.85f, 0f, -1f),
            D("Duvar_Tebesir_K4", 5.10f, 1.00f, 6.50f, 1f, 0f),
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
