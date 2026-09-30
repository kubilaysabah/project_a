// GrayboxKurucu_Kat8.cs
// 8. katın içerik dosyası (Assets/Zincirler/Kat_8.asset): oda oda döşeme ve kilit zinciri (#83).
//
// ---------- 8. KAT: ASANSÖR (#68), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// Daire 4 (kapıda 32), apartman Yönetimi'nin bürosu: bekleme yeri, müdür odası (salon), çay ocağı (mutfak),
// arşiv (Oda A), toplantı odası (Oda B), depo (kiler). Son kat, en uzun zincir (sekiz kilit). Kurallar: katlarda
// aynı model yok; ipucu notu yok (Yönetim'in notu artık yol göstermiyor; kiler, salon ve arşiv notları
// kaldırıldı); bir odada en fazla bir görev parçası.
//
// Kapı sırası ve görev parçaları:
//   Antre  : yerde tornavida (çantada zaten varsa konmaz).
//   Banyo  : (menteşe, tornavida) yerde kilerin anahtarı.
//   Kiler  : (anahtar) duvardaki kol -> toplantı odasının sürgüsü.
//   Oda B  : (kol) evrak dolabının üst çekmecesinde mutfağın anahtarı.
//   Mutfak : (anahtar) tezgâhın üst çekmecesinde salonun anahtarı.
//   Hol    : (kilitsiz) duvarda iskân ruhsatı: "yapım yılı 1974".
//   Oda A  : (şifre 1974, arşiv) duvardaki anahtar dolabında dosya dolabının anahtarı.
//   Salon  : (anahtar) dosya dolabının üst çekmecesi (anahtar): asansörün sigortası ve Cemal'in notu.
//   Sahanlık: sigorta asansör panosuna takılır, asansör çağrılır, gösterge 9 der; kabinde ÇATI ANAHTARI.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    // kat8_esyalar.py raporundan: anahtar dolabının alt rafı (Unity eksenlerinde, kökün pivotuna göre).
    const float AnahtarDolabi8RafY = -0.156f;
    const float AnahtarDolabi8RafZ = 0.047f;

    static void Kat8SablonlariniKur()
    {
        foreach (string ad in new[] { "Bekleme_Sandalyeleri_K8", "Ilan_Panosu_K8", "Arsiv_Kutulari_K8", "Mudur_Masasi_K8", "Makam_Koltugu_K8",
                                      "Misafir_Koltugu_K8", "Dosya_Rafi_K8", "Kasa_K8", "Jaluzi_K8", "Duvar_Saati_K8", "Cay_Ocagi_K8",
                                      "Buzdolabi_K8", "Mutfak_Sandalyesi_K8", "Temizlik_Arabasi_K8", "Arsiv_Rafi_K8", "Eski_Daktilolar_K8",
                                      "Arsiv_Dolabi_K8", "Harita_Masasi_K8", "Plan_Cercevesi_K8", "Toplanti_Masasi_K8",
                                      "Toplanti_Sandalyesi_K8", "Tepegoz_K8", "Kara_Tahta_K8" })
            ParcaSablonuKur(ad, ad);
        AcilirSablonKur("Anahtar_Dolabi_K8");
        AcilirSablonKur("Evrak_Dolabi_K8");
    }

    static KatZinciriVarligi Kat8Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_8.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "8. kat, daire 4, Yönetim bürosu (asansör). Sekiz kilit: banyo (menteşe; kiler anahtarı), kiler (kol), Oda B " +
                     "(evrak dolabında mutfak anahtarı), mutfak (salon anahtarı), Oda A (şifre 1974, holdeki iskân ruhsatı; anahtar " +
                     "dolabında dosya dolabının anahtarı), salon (dosya dolabında asansörün sigortası), asansör (sigorta panoya, çağır, " +
                     "gösterge 9 der) -> kabinde çatı anahtarı. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        v.kapiNumarasi = "";
        v.girisKapali = false;
        v.sahanlikKarti = null;
        v.elektrikYok = false;
        v.ikinciDaire = 0;
        v.kacisVar = false;

        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "antre_yer", PlandaNokta = true, Oda = "Antre", U = 2.0f, Y = 0.004f, V = -0.4f, Donus = 70f });
        // Banyonun kapıya göre sağ köşesi: menteşesi sökülen kapının devrildiği şeridin (v 0,9-2,9) dışında.
        z.Yerler.Add(new ZincirYeri { Kimlik = "banyo_yer", PlandaNokta = true, Oda = "Banyo", U = 4.2f, Y = 0.004f, V = 0.80f, Donus = -25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "evrak", Yol = "Evrak_Dolabi_Toplanti/Cekmece_1", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "mutfak_cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        // İskân ruhsatı holün kuzey duvarında (salon ile Oda B kapılarının arasında): şifrenin kaynağı.
        z.Yerler.Add(new ZincirYeri { Kimlik = "hol_ruhsat", PlandaNokta = true, Oda = "Hol", U = 2.7f, Y = 1.55f, V = 4.60f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "anahtar_dolabi", Yol = "Anahtar_Dolabi_Arsiv", Oda = "Oda_A", KilitYolu = "Anahtar_Dolabi_Arsiv/Kapi_1" });
        // Dosya dolabının üst çekmecesi: hem kilidin hedefi hem sigortanın yeri (çekmeceyle birlikte kayar).
        z.Yerler.Add(new ZincirYeri { Kimlik = "dosya_ust", Yol = "Esyalar/Dosya_Dolabi/Cekmece_1", Oda = "Salon" });
        // Asansörün kabini sahanlıkta (dairenin dışında): kapılar açılana kadar gizli.
        z.Yerler.Add(new ZincirYeri { Kimlik = "kabin", Yol = "Sahanlik:Asansor/Kabin", Oda = KilitZinciri.SahanlikOdasi });

        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "antre_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kiler_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "banyo_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "mutfak_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "evrak", Y = 0.004f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "mutfak_cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 35f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "dosya_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "anahtar_dolabi", Y = AnahtarDolabi8RafY, Z = AnahtarDolabi8RafZ, Donus = -15f });
        // Çekmecenin tabanı 1,0 m'de (dosya_dolabi.py: üst yuvanın tepsisi); sigorta sağda, not solda.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "asansor_sigortasi", Tur = EsyaTuru.Sigorta, Yer = "dosya_ust", X = 0.09f, Y = 1.0f, Z = 0.02f, Donus = 80f });
        // Çatı anahtarı kabinin zemininde (zeminin üstü 8 mm), kapıdan 55 cm içeride.
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "cati_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "kabin",
                                         X = 0.12f, Y = 0.012f, Z = -MekanOlculeri.SahanlikDerin - MekanOlculeri.DuvarKalin - 0.55f, Donus = 35f });

        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "kiler_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "mutfak_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Salon", Gerekli = "salon_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "hol_ruhsat",
                                          Sifre = "1974", IpucuYazma = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "dosya", Tur = KilitTuru.Anahtar, OdaKapisi = false, Hedef = "dosya_ust", Gerekli = "dosya_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "asansor", Tur = KilitTuru.Asansor, OdaKapisi = false, Hedef = "kabin", Gerekli = "asansor_sigortasi" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Yönetim'in notu antrede, sigorta kutusunun yanında (hikâye: çatı anahtarı asansörde).
            new KatNotu { Metin = "not.kat8_yonetim", Yol = "Esyalar/Sigorta_Kutusu", X = 0.42f, Y = 0f, Z = 0.004f },
            // İskân ruhsatı holün kuzey duvarında (v = 4,65), holün içine (-v) bakar.
            new KatNotu { Metin = "not.kat8_ruhsat", X = 2.7f, Y = 1.55f, Z = 4.648f, OnU = 0f, OnV = -1f },
            // Dosya dolabının çekmecelerinde yatık kâğıtlar: üstte Cemal'in notu (sigortanın yanında),
            // ikincide kapıcıların dosyaları, üçüncüde dokuzuncu katın planı; dördüncü boş.
            new KatNotu { Metin = "not.kat8_sigorta", Yol = "Esyalar/Dosya_Dolabi/Cekmece_1", X = -0.08f, Y = 1.0f, Z = 0f, Yatik = true },
            new KatNotu { Metin = "not.kat8_dosyalar", Yol = "Esyalar/Dosya_Dolabi/Cekmece_2", X = 0f, Y = 0.69f, Z = 0f, Yatik = true },
            new KatNotu { Metin = "not.kat8_plan", Yol = "Esyalar/Dosya_Dolabi/Cekmece_3", X = 0f, Y = 0.38f, Z = 0f, Yatik = true },
        };
        v.parcalar = new List<KatParcasi>();
        v.sesler = new List<KatSesi>();
        // Yönetim'in bürosu (salon) ve hol aydınlık; öbür odalar kafa lambasıyla.
        v.yananOdalar = new List<string> { "Salon", "Hol" };
        v.esyaYazilari = new List<EsyaYazisi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah", "Klozet", "Lavabo" };

        v.suslar = new List<KatSusu>
        {
            // ---------- ANTRE (bekleme) ---------- (u 0-2,6, v -0,8-2,85; giriş kapısının köşesi boş)
            D("Bekleme_Sandalyeleri_K8", 2.60f - 0.26f, 0f, 1.30f, -1f, 0f),
            D("Ilan_Panosu_K8", 2.60f, 1.60f, 1.30f, -1f, 0f),

            // ---------- HOL ---------- (u 0-10,5, v 2,95-4,65): kuzey duvarında iskân ruhsatı (not) ve eski
            // yönetimin fotoğrafı; mutfak kapısından sonra güney duvarında arşiv kutuları.
            Foto("kat8", true, 4.40f, 1.60f, 4.65f, 0f, -1f),
            D("Arsiv_Kutulari_K8", 9.30f, 0f, 2.95f + 0.34f, 0f, 1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),

            // ---------- SALON (müdür odası) ---------- (u 0-5,0, v 4,75-8,1; kapının önü boş)
            // Hol duvarında dairenin dosya dolabı (kalıcı eşya, u 4,4). Pencerenin önünde makam masası ve koltuğu,
            // karşısında iki misafir koltuğu; landing duvarında klasör rafı; köşede kasa.
            D("Mudur_Masasi_K8", 2.60f, 0f, 7.10f, 0f, -1f),
            D("Makam_Koltugu_K8", 2.60f, 0f, 7.70f, 0f, -1f),   // pencere duvarına giriyordu (çakışma testi)
            D("Misafir_Koltugu_K8", 2.15f, 0f, 6.25f, 0.2f, 1f),
            D("Misafir_Koltugu_K8", 3.05f, 0f, 6.25f, -0.2f, 1f),
            D("Dosya_Rafi_K8", 0.03f + 0.19f, 0f, 6.60f, 1f, 0f),
            D("Kasa_K8", 4.62f, 0f, 7.70f, -1f, 0f),
            D("Jaluzi_K8", 1.40f, 2.30f, 8.10f, 0f, -1f),
            D("Duvar_Saati_K8", 5.00f, 1.95f, 6.50f, -1f, 0f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),

            // ---------- MUTFAK (çay ocağı) ---------- (u 6,8-10,5, v -0,8-2,85; kapının önü boş)
            D("Buzdolabi_K8", 7.20f, 0f, -0.80f + 0.26f, 0f, 1f),
            D("Cay_Ocagi_K8", 8.00f, 0f, -0.80f + 0.24f, 0f, 1f),
            D("Mutfak_Sandalyesi_K8", 7.05f, 0f, 0.90f, 1f, 0f),
            D("Mutfak_Sandalyesi_K8", 7.05f, 0f, 1.50f, 1f, 0f),

            // ---------- BANYO ---------- (lavabo ve klozet dairenin parçaları)
            D("Temizlik_Arabasi_K8", 4.10f, 0f, 0.20f, -1f, 0f),

            // ---------- KİLER (depo) ---------- (u 4,6-6,7; duvar kolu u 6,7 v 1,2)
            D("Arsiv_Rafi_K8", 4.60f + 0.23f, 0f, 0.30f, 1f, 0f),
            D("Eski_Daktilolar_K8", 6.00f, 0f, -0.40f, 0f, 1f),

            // ---------- ODA A (arşiv) ---------- (u 7,7-10,5, v 4,75-8,1; kapının önü boş)
            D("Arsiv_Dolabi_K8", 10.50f - 0.23f, 0f, 6.40f, -1f, 0f),
            D("Harita_Masasi_K8", 8.90f, 0f, 7.40f, 0f, -1f),
            D("Plan_Cercevesi_K8", 7.70f, 1.60f, 6.50f, 1f, 0f),
            DA("Anahtar_Dolabi_Arsiv", "Anahtar_Dolabi_K8", 10.05f, 1.50f, 4.75f, 0f, 1f),

            // ---------- ODA B (toplantı odası) ---------- (u 5,1-7,6, v 4,75-8,1; kapının önü u 5,82-6,81, v 4,75-5,5 boş)
            // Masa batı duvarına yakın, boyuna; doğu yanında tek sandalye; doğuda 66 cm yürüme yolu. (Ortadayken
            // masa, sandalyeler ve tepegöz odanın girişini kapatıyordu: yürünebilirlik denetimi.)
            D("Toplanti_Masasi_K8", 5.75f, 0f, 7.00f, 1f, 0f),
            D("Toplanti_Sandalyesi_K8", 6.70f, 0f, 6.75f, -1f, 0f),
            // Evrak dolabı doğu duvarında, sandalyelerin kuzeyinde: önünde durulacak yer var.
            DA("Evrak_Dolabi_Toplanti", "Evrak_Dolabi_K8", 7.60f - 0.24f, 0f, 7.60f, -1f, 0f),
            D("Tepegoz_K8", 7.20f, 0f, 5.25f, -1f, 0.3f),
            // Tahta batı duvarında, masanın üstünde (doğu duvarında yürüme yolunu daraltıyordu).
            D("Kara_Tahta_K8", 5.10f, 1.55f, 7.00f, 1f, 0f),
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s8 = z.Cozumle(odalar);
        if (!s8.Cozulebilir) Debug.LogError("[ZINCIR] 8. kat zinciri çözülemiyor: " + string.Join("; ", s8.Hatalar));
        else Debug.Log("[ZINCIR] 8. kat zinciri çözülebilir: " + string.Join(" -> ", s8.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
