// GrayboxKurucu_Kat5.cs
// 5. katın içerik dosyası (Assets/Zincirler/Kat_5.asset): oda oda döşeme ve kilit zinciri (#83).
//
// ---------- 5. KAT: MEKANİZMA (#65), ODA ODA YENİDEN DÖŞENDİ (#83) ----------
// Daire 2 (kapıda 18), Karaca ailesinin evi; aile yazlıkta. Kapıcı Cemal banyodaki su kaçağını onarmaya
// girmiş, işi yarım bırakıp kaybolmuş. Düzenli, klasik, kahverengi tonlu bir ev; holde tavandan su damlıyor.
// Kurallar: katlarda aynı model yok; ipucu notu yok (Cemal'in iş listesi artık yalnızca iş emri);
// bir odada en fazla bir görev parçası.
//
// Kapı sırası ve görev parçaları (beş kilit, parçalar birbirini tamamlar):
//   Antre  : yerde tornavida (kapıcının çantasında zaten varsa konmaz).
//   Hol    : su kovasının ötesinde yerde mobilya kaydırıcısı.
//   Kiler  : (kilitsiz) duvardaki kol: çekilince mutfağın sürgüsü açılır.
//   Mutfak : (kol) tezgâhın ikinci çekmecesinde salon anahtarının başı.
//   Oda B  : (kilitsiz) çalışma masasının üstünde salon anahtarının ucu.
//   Oda A  : (kilitsiz) karyolanın yanında yerde levye.
//   Salon  : (kırık anahtar, iki parça) gardırop levye + kaydırıcıyla itilir; arkasında ana vana: çevrilince su kesilir.
//   Banyo  : (menteşe, tornavida) su kesilince lavabonun sifonu sökülür: ağzında katın anahtarı.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{

    static void Kat5SablonlariniKur()
    {
        foreach (string ad in new[] { "Ayakkabilik_K5", "Semsiyelik_K5", "Duvar_Askisi_K5", "Yolluk_K5", "Konsol_K5", "Su_Kovasi_K5",
                                      "Kanepe_K5", "TV_Unitesi_K5", "Hali_K5", "Perde_K5", "Buzdolabi_K5", "Ocak_K5", "Mutfak_Masasi_K5",
                                      "Tabure_K5", "Ekmek_Sepeti_K5", "Camasir_Makinesi_K5", "Havlu_Askisi_K5", "Banyo_Dolabi_K5",
                                      "Legen_Kova_K5", "Raf_K5", "Is_Lambasi_K5", "Boya_Kovalari_K5", "Komodin_K5", "Sifonyer_K5",
                                      "Perde_OdaA_K5", "Calisma_Masasi_K5", "Kitaplik_K5", "Sandalye_K5", "Perde_OdaB_K5" })
            ParcaSablonuKur(ad, ad);
    }

    static KatZinciriVarligi Kat5Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_5.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "5. kat, daire 2, Karaca ailesinin evi; Cemal'in yarım bıraktığı iş (mekanizma). Kilerdeki kol mutfağı açar; " +
                     "salonun anahtarı kırık: başı mutfak çekmecesinde, ucu Oda B'de masada. Gardırop levye (Oda A) ve kaydırıcıyla " +
                     "(hol) itilir, arkasında vana; su kesilince sifon sökülür, ağzında katın anahtarı. Banyo menteşeli. Oda oda döşendi (#83).";
        v.tasarimda = false;
        v.yalnizSecilenEsyalar = true;
        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "antre_yer", PlandaNokta = true, Oda = "Antre", U = 1.60f, Y = 0.004f, V = 0.60f, Donus = 70f });
        // #102 (kat kat zorlaşma): yer, KatDenetimi.SaklamaYeriAra'nın ölçtüğü adaylardan seçildi; parantezde
        // ayakta/eğilmiş görüldüğü nokta sayısı (20 cm ızgara, 5 m). Eski yer açıktaydı.
        // Kaydırıcılar salondaki kanepenin altında, duvar dibinde (0/5): holün ortasındaydı (150 noktadan açık).
        // Hol düz bir koridor; saklanacak yeri yok. Salon gardırobun odası, kaydırıcılar da onun için.
        z.Yerler.Add(new ZincirYeri { Kimlik = "kanepe_alti", PlandaNokta = true, Oda = "Salon", U = 4.72f, Y = 0.004f, V = 6.67f, Donus = 15f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kiler_kolu", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "cekmece", Yol = "Esyalar/Tezgah/Cekmece_2", Oda = "Mutfak" });
        // Anahtar ucu çalışma odasının kapı tarafındaki köşede, yerde (10/10): masanın üstündeydi (80 noktadan açık).
        z.Yerler.Add(new ZincirYeri { Kimlik = "oda_kosesi", PlandaNokta = true, Oda = "Oda_B", U = 5.62f, Y = 0.004f, V = 5.07f, Donus = 30f });
        // Levye yatakla komodinin arasında, yerde (11/12): odanın ortasındaydı (80 noktadan açık).
        z.Yerler.Add(new ZincirYeri { Kimlik = "levye_yer", PlandaNokta = true, Oda = "Oda_A", U = 10.22f, Y = 0.004f, V = 6.67f, Donus = 80f });
        // Vananın kilidi gardırobun kendisinde (KilitYolu): gardırop itilmeden vanaya ulaşılmaz.
        z.Yerler.Add(new ZincirYeri { Kimlik = "vana", Yol = "Esyalar/Vana/Cark", Oda = "Salon", KilitYolu = "Esyalar/Gardirop" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sifon", Yol = "Esyalar/Lavabo/Sifon", Oda = "Banyo" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "antre_yer" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kaydirici", Tur = EsyaTuru.Kaydirici, Yer = "kanepe_alti" });
        // Tezgâhın ikinci çekmecesinin tabanı: x 0,59, y 0,39, z 0,04 (tezgah.py).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_bas", Tur = EsyaTuru.AnahtarParcasi, Yer = "cekmece", X = 0.59f, Y = 0.39f, Z = 0.04f, Donus = 40f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari_uc", Tur = EsyaTuru.AnahtarParcasi, Yer = "oda_kosesi" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "levye", Tur = EsyaTuru.Levye, Yer = "levye_yer" });
        // Anahtar sifonun ağzında, kenarın hemen altında (sifonun pivotu kendi üst ortası).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sifon", Y = -0.02f, Donus = 30f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "mutfak", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Mutfak", Gerekli = "kiler_kolu" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.KirikAnahtar, OdaKapisi = true, Hedef = "Salon",
                                          Gerekenler = new List<string> { "salon_anahtari_bas", "salon_anahtari_uc" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "gardirop", Tur = KilitTuru.Ekipman, OdaKapisi = false, Hedef = "vana",
                                          Gerekenler = new List<string> { "levye", "kaydirici" } });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sifon", Tur = KilitTuru.Kol, OdaKapisi = false, Hedef = "sifon", Gerekli = "vana" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Yönetim'in iş emri (hikâye; yol göstermez): antrede, sigorta kutusunun yanında.
            new KatNotu { Metin = "not.kat5_liste", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        v.parcalar = new List<KatParcasi>();
        v.acikParcalar = new List<AcikParca>();
        v.buzdolabiFisiCekili = false;
        v.yananOdalar = new List<string>();
        v.gorunenEsyalar = new List<string> { "Sigorta_Kutusu", "Tezgah", "Klozet", "Lavabo", "Vana", "Gardirop" };
        // Lavabonun damlası: oyuncu daireye girer girmez duyulur; kapatılamaz; vana çevrilince
        // ("sifon" kilidi çözülünce) kalıcı olarak susar.
        v.sesler = new List<KatSesi>
        {
            new KatSesi { Yol = "Esyalar/Lavabo", Oda = "Banyo", Gecikme = 0f, Ses = 0.7f, Kapatilabilir = false, SusturanKilit = "sifon",
                          Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Boru_Damla.wav") },
        };
        if (v.sesler[0].Klip == null) Debug.LogError("[ZINCIR] Boru_Damla.wav bulunamadı.");

        v.suslar = new List<KatSusu>
        {
            // ---------- ANTRE ---------- (u 0-2,6, v -0,8-2,85; giriş kapısı -v yönüne açılır: u 0-0,9, v -0,9-0,1 boş)
            D("Ayakkabilik_K5", 2.60f - 0.21f, 0f, 1.40f, -1f, 0f),
            D("Semsiyelik_K5", 2.32f, 0f, 2.25f, -1f, 0f),
            D("Duvar_Askisi_K5", 2.30f, 1.65f, -0.80f, 0f, 1f),

            // ---------- HOL ---------- (u 0-10,5, v 2,95-4,65)
            // Yolluk holün başında; ortada tavandan damlayan suyu toplayan kova; Oda B ile Oda A kapıları
            // arasında konsol, üstünde saat ve ailenin fotoğrafı.
            D("Yolluk_K5", 3.90f, 0f, 3.80f, 0f, 1f),
            // Kova holün kuzey kenarında, salon ile Oda B kapılarının arasında: ortadayken koridoru, Oda B'nin
            // önündeyken kapıyı kapatıyordu (yürünebilirlik denetimi).
            // Kovanın adı var (#96): katın anının sesi ondan gelir.
            DA("Su_Kovasi_Karaca", "Su_Kovasi_K5", 4.60f, 0f, 4.25f, 0f, 1f),
            D("Konsol_K5", 7.70f, 0f, 4.65f - 0.16f, 0f, -1f),
            M("mantel_clock_01", 7.50f, 0.80f, 4.52f, 0f, -1f),
            Foto("kat5", false, 7.92f, 0.80f, 4.52f, 0f, -1f),
            D("Petek", 10.50f, 0f, 3.80f, -1f, 0f),

            // ---------- SALON ---------- (u 0-5,0, v 4,75-8,1; kapının önü boş)
            // Landing duvarında dairenin kalıcı gardırobu ve arkasındaki vana (itilince pencereye doğru kayar,
            // o şerit boş kalır). Kanepe Oda B duvarında; önünde uzun sehpa; TV ünitesi hol duvarında.
            D("Kanepe_K5", 5.00f - 0.45f - 0.02f, 0f, 6.45f, -1f, 0f),
            M("WoodenTable_01", 3.60f, 0f, 6.45f, 1f, 0f),
            M("GreenChair_01", 2.30f, 0f, 7.33f, 0.3f, -1f),   // öne alınan perdeye girmesin (çakışma testi)
            D("TV_Unitesi_K5", 3.00f, 0f, 4.75f + 0.25f, 0f, 1f),
            D("Hali_K5", 2.80f, 0f, 6.45f, 0f, 1f),
            D("Perde_K5", 1.40f, 2.46f, PetekOnuPerde, 0f, -1f),
            D("Kornis_Dirsegi", 1.40f - 0.92f, 2.448f, 8.10f, 0f, -1f),   // korniş dirsekleri (perde duvardan 22 cm önde)
            D("Kornis_Dirsegi", 1.40f + 0.92f, 2.448f, 8.10f, 0f, -1f),
            D("Petek", 1.40f, 0f, 8.10f, 0f, -1f),

            // ---------- MUTFAK ---------- (u 6,8-10,5, v -0,8-2,85; kapının önü boş; tezgâh dip duvarda)
            M("painted_wooden_cabinet_02", 6.80f + 0.36f + 0.01f, 0f, 0.90f, 1f, 0f),
            D("Buzdolabi_K5", 7.20f, 0f, -0.80f + 0.34f, 0f, 1f),
            D("Ocak_K5", 8.00f, 0f, -0.80f + 0.35f, 0f, 1f),
            M("vintage_electric_kettle", 8.10f, 0.86f, -0.36f, 0.3f, 1f),
            D("Mutfak_Masasi_K5", 8.60f, 0f, 1.40f, 0f, 1f),
            D("Tabure_K5", 8.60f, 0f, 2.00f, 0f, -1f),
            D("Tabure_K5", 8.60f, 0f, 0.80f, 0f, 1f),
            D("Ekmek_Sepeti_K5", 10.20f, 0.90f, 0.35f, -1f, 0f),

            // ---------- BANYO ---------- (u 2,7-4,5, v -0,8-2,85; menteşesi sökülen kapı v 0,9-2,9'a devrilir)
            // Lavabo (dış duvar) ve klozet dairenin kalıcı parçaları. Çamaşır makinesi ve leğen devrilme şeridinin dışında.
            D("Camasir_Makinesi_K5", 4.50f - 0.23f, 0f, 0.10f, -1f, 0f),
            D("Havlu_Askisi_K5", 2.70f, 1.30f, 1.10f, 1f, 0f),
            D("Banyo_Dolabi_K5", 3.60f, 1.50f, -0.80f, 0f, 1f),
            // Leğen klozetin yanında, antre duvarına yaslı: çamaşır makinesinin içine giriyordu (çakışma testi);
            // menteşesi sökülen kapının düştüğü şeridin (u 3,25-3,95) batısında kalır.
            D("Legen_Kova_K5", 2.70f + 0.28f, 0f, 1.45f, 1f, 0f),

            // ---------- KİLER ---------- (u 4,6-6,7, v -0,8-2,85; kapının önü boş; duvar kolu u 6,7 v 1,2)
            D("Raf_K5", 4.60f + 0.21f, 0f, 0.30f, 1f, 0f),
            D("Is_Lambasi_K5", 5.80f, 0f, 1.20f, 0f, 1f),   // boya kovalarının içine giriyordu (çakışma testi)
            D("Boya_Kovalari_K5", 6.30f, 0f, -0.50f, 0f, 1f),

            // ---------- ODA A ---------- (u 7,7-10,5, v 4,75-8,1; kapının önü boş)
            // Gotik karyola (1,49 x 2,04 m) başı doğu duvarında, odanın kuzey şeridinde; komodin güneyinde,
            // şifonyer batı duvarında kapıya yakın. Levye karyolanın güney kenarının dibinde, açık alanda yerde
            // (karyolanın batısında kalan 74 cm'lik şerit oyuncunun gövdesine dar geliyordu). Karyola ortadayken
            // kapının önünü, şifonyerle arasındaki geçidi ve levyeyi kapatıyordu; hazır mobilyaya çarpışma
            // takılınca odada 0,7 m² kaldığı görüldü (eksikler testi).
            M("GothicBed_01", 10.50f - 1.02f - 0.02f, 0f, 7.20f, -1f, 0f),
            D("Komodin_K5", 10.50f - 0.20f, 0f, 6.20f, -1f, 0f),
            D("Sifonyer_K5", 7.70f + 0.23f, 0f, 5.65f, 1f, 0f),
            D("Perde_OdaA_K5", 9.10f, 2.30f, 8.10f, 0f, -1f),

            // ---------- ODA B (çalışma odası) ---------- (u 5,1-7,6, v 4,75-8,1; kapının önü boş)
            D("Calisma_Masasi_K5", 6.35f, 0f, 8.10f - 0.41f, 0f, -1f),   // perde arkasından sarkabilsin (çakışma testi)
            D("Sandalye_K5", 6.35f, 0f, 7.12f, 0f, 1f),
            D("Kitaplik_K5", 5.10f + 0.18f, 0f, 6.30f, 1f, 0f),
            D("Perde_OdaB_K5", 6.35f, 2.30f, 8.10f, 0f, -1f),
        };

        // ---------- KATIN ANI (#96): SU YENİDEN DAMLIYOR ----------
        // Oyuncu vanayı kapatmış, damla susmuş, sifonun ağzından katın anahtarını almıştır. 3 sn sonra holdeki
        // kovaya yeniden su damlamaya başlar; önce seyrek, sonra sıklaşarak, sonra birden kesilir. Su kesik:
        // tavandan damlayan şey su değil, ya da yukarıda biri var. Cemal'in yarım işinin cevapsız sorusu.
        v.anVar = true;
        v.an = new KatAniVerisi
        {
            Tur = KatAniTuru.Ses, Yol = "Su_Kovasi_Karaca", TetikEsya = "kat_anahtari", Gecikme = 3f,
            Klip = AnSesi("An_Damla"), Ses = 0.9f, Sure = 0f,
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s5 = z.Cozumle(odalar);
        if (!s5.Cozulebilir) Debug.LogError("[ZINCIR] 5. kat zinciri çözülemiyor: " + string.Join("; ", s5.Hatalar));
        else Debug.Log("[ZINCIR] 5. kat zinciri çözülebilir: " + string.Join(" -> ", s5.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }
}
