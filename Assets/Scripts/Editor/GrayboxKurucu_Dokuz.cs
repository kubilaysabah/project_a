// GrayboxKurucu_Dokuz.cs
// Dokuzuncu katın seti (#108, Docs/hikaye.md §16): GrayboxKurucu'nun bir parçası (partial).
//
// NE KURAR
// "Dokuzuncu_Kat" kökü, dış cephe (Final) gibi sahnenin uzağında (x = -400) durur: oyuncu oraya
// asansörle (ışınlanarak) gider, merdivenle değil. İçinde:
//   Sahanlik_9  : bildiğimiz sahanlığın 1974 hâli. Kapı numaraları 33-36; katın anahtarı, asansörün
//                 "ARIZALI" notu, çöpler, 1994'ün sahanlık eşyaları (tüp, gazete) yok; ortam sesleri yok
//                 (elektrik yok: bina sessiz). Sahanlık lambası sönük. Daire_9'un kapısı (Kapi_2) açık.
//   Cekirdek_9  : merdiven çekirdeği (yukarı) ve bir alttaki çekirdek (aşağı inen kol), ikisinin de ağzı
//                 TUĞLAYLA ÖRÜLÜ: "merdiven çıkmaz, asansör çıkar". İlk iki basamak tuğlanın önünde görünür.
//   Daire_9     : aynı 3+1 plan (DaireKur). Holde üç çivi (tespih, kasket, boş çivi = CiviAskisi), salon
//                 kapısı kilitli (lamba asılınca açılır), sofra (ayrı sandalyeli Sofra_1974) dört tabakla,
//                 Gölge Cemal'in sandalyesinde oturur, oyuncunun tabağına çatı anahtarı oyunda konur
//                 (DokuzuncuKat.BolumuKur). Oda A'da Nuri'nin radyosunun ikizi, Oda B'de Hasan'ın kamp
//                 yatağı ve Cemal'in sandığı: model tekrarı burada bilerek var, tekrar hikâyenin kendisi.
//                 Banyo, mutfak, Oda C ve balkon kilitli. Yalnızca salonun lambası yanar (oyunda).
// Yönetim: DokuzuncuKat bileşeni kökte; bu dosya onun bütün bağlantılarını SerializedObject ile bağlar.

using UnityEditor;
using UnityEngine;
using static MekanOlculeri;

public static partial class GrayboxKurucu
{
    const float DokuzX = -400f;          // setin dünya konumu (Final +400'de)
    const int DokuzDairesi = 2;          // sol-uzak daire; kapısı Kapi_2 ("34")

    static void DokuzuncuKatKur()
    {
        GameObject kok = new GameObject("Dokuzuncu_Kat");
        kok.transform.position = new Vector3(DokuzX, 0f, 0f);
        Transform d = kok.transform;

        // ---- SAHANLIK (1974) ----
        GameObject sahanlikNesnesi = SahanlikKur("Sahanlik_9", tam: true);
        sahanlikNesnesi.transform.SetParent(d, false);
        Transform s = sahanlikNesnesi.transform;
        for (int n = 1; n <= DaireSayisi; n++)
        {
            Transform numara = s.Find($"Kapi_{n}/Mentese/Numara");
            TextMesh tm = numara != null ? numara.GetComponent<TextMesh>() : null;
            if (tm != null) tm.text = (32 + n).ToString();   // 9. kat: 33-36 (4N-3 .. 4N)
            Sil(s, $"Kapi_{n}/Copler");                      // 1974: çöp yok
            Sil(s, $"Kapi_{n}/Ayakkabilar");                 // dağınık ayakkabı yok
        }
        Sil(s, "Kat_Anahtari");          // katın anahtarı bu katta sahanlıkta değil, sofrada
        // Asansörün üstündeki arızalı lamba (#111) burada hep yanar: lamba çiviye asılınca sahanlığı o aydınlatır.
        Transform arizaLambasi = s.Find("Asansor/Ariza_Lambasi");
        if (arizaLambasi != null)
        {
            var aSeri9 = new SerializedObject(arizaLambasi.GetComponent<ArizaliLamba>());
            aSeri9.FindProperty("herZaman").boolValue = true;
            aSeri9.ApplyModifiedPropertiesWithoutUndo();
        }
        Sil(s, "Asansor/Not");           // asansör 1974'te çalışıyor: "arızalı" notu yok
        Sil(s, "Asansor/Not_Yazi");
        Sil(s, "Ses_Ugultu");            // elektrik yok: uğultu, TV, bebek yok
        Sil(s, "Kapi_2/Ses_TV");
        Sil(s, "Kapi_4/Ses_Bebek");
        Sil(s, "Kapi_2/Tup");            // 1994'ün sahanlık eşyaları (#24) henüz yok
        Sil(s, "Kapi_3/Gazete");
        Sil(s, "Kapi_4/Ayakkabi");
        // Daire_9'un sahanlık kapısı açık (KatYoneticisi'nin SahanlikKapisiniAyarla'sı gibi, ama sabit).
        Transform girisMentese = s.Find($"Kapi_{DokuzDairesi}/Mentese");
        if (girisMentese != null) girisMentese.localRotation = Quaternion.Euler(0f, 80f, 0f);
        Transform karanlikHol = s.Find($"Kapi_{DokuzDairesi}/Daire_Ici");
        if (karanlikHol != null) karanlikHol.gameObject.SetActive(false);

        // ---- ÇEKİRDEKLER VE TUĞLA ----
        GameObject cekirdek = CekirdekKur("Cekirdek_9");
        cekirdek.transform.SetParent(d, false);
        GameObject altCekirdek = CekirdekKur("Cekirdek_9_Alt");
        altCekirdek.transform.SetParent(d, false);
        altCekirdek.transform.localPosition = new Vector3(0f, -DikeyOfset, 0f);
        TuglaDuvarKur(d);
        // TAVAN: bir sahanlığın tavanı bir üst sahanlığın döşemesidir (gerçek binada Sahanlik_B ve sahte katlar).
        // Burada üstte hiçbir şey yoktu, gökyüzü görünüyordu: bir üst sahte sahanlık ve çekirdek konur (lambaları
        // sönük; tuğla duvar o katın çekirdek ağzını da kapatır, oyuncu oraya çıkamaz).
        GameObject ustSahanlik = SahanlikKur("Sahanlik_9_Ust", tam: false);
        ustSahanlik.transform.SetParent(d, false);
        ustSahanlik.transform.localPosition = new Vector3(0f, DikeyOfset, 0f);
        foreach (string ad in new[] { "Ses_Ugultu", "Kapi_2/Ses_TV", "Kapi_4/Ses_Bebek" }) Sil(ustSahanlik.transform, ad);
        GameObject ustCekirdek = CekirdekKur("Cekirdek_9_Ust");
        ustCekirdek.transform.SetParent(d, false);
        ustCekirdek.transform.localPosition = new Vector3(0f, DikeyOfset, 0f);

        // ---- DAİRE ----
        GameObject daireNesnesi = DaireKokuKur(s, DokuzDairesi);
        daireNesnesi.name = "Daire_9";
        daireNesnesi.SetActive(true);                      // sahanlıktaki dört daire kapalı saklanır; bu hep açık
        Transform daire = daireNesnesi.transform;
        DaireYonu yon = DaireYonuBul(DokuzDairesi);
        Transform esyalar = daire.Find("Esyalar");
        // Bulmaca eşyaları ve 1994 kalıntıları bu dairede yok (ZincirUygulayici.BulmacaEsyalariniAyarla'nın
        // listesi + paylaşılan sofra: onun yerine ayrı sandalyeli Sofra_1974 kurulur).
        foreach (string ad in new[] { "Bufe", "Dosya_Dolabi", "Oyuncak_Kutusu", "Boy_Cetveli", "Gardirop", "Alet_Cantasi",
                                      "Duvar_Kolu", "Sofra", "Portmanto" })
        {
            Transform t = esyalar != null ? esyalar.Find(ad) : null;
            if (t != null) t.gameObject.SetActive(false);
        }
        // Kilitli odalar: banyo, Oda C, mutfak, salon (oyunda lamba asılınca açılır). Oda A ve B açık.
        foreach (string kapiAdi in new[] { "Kapi_Banyo", "Kapi_Oda_C", "Kapi_Mutfak", "Kapi_Salon" })
            IcKapiyiKilitle(daire, kapiAdi);

        // ---- HOL: ÜÇ ÇİVİ ----
        // Hol-salon bölmesinin hol yüzü (v = 4,65), salon kapısının (u 0,85-1,75) sağında, göz hizasının biraz
        // altında. Çivi modeli duvara girdiği noktadan +Z'ye uzar: +Z, hole bakan yön (-v) olacak şekilde döndürülür.
        Vector3 holYonu = yon.Yon(0f, -1f);
        float civiDonusu = Mathf.Atan2(holYonu.x, holYonu.z) * Mathf.Rad2Deg;
        const float CiviV = 4.65f, CiviY = 1.62f;
        Vector3 civi1 = yon.Nokta(2.30f, CiviY, CiviV), civi2 = yon.Nokta(2.58f, CiviY, CiviV), civi3 = yon.Nokta(2.86f, CiviY, CiviV);
        Model("Civi_Tespih", daire, "Kafa_Lambasi", "Civi", civi1, civiDonusu, carpisma: false);
        Model("Civi_Kasket", daire, "Kafa_Lambasi", "Civi", civi2, civiDonusu, carpisma: false);
        GameObject bosCivi = Model("Civi_Bos", daire, "Kafa_Lambasi", "Civi", civi3, civiDonusu, carpisma: false);
        // Tespih ve kasket çivinin gövdesine, baştan 1 cm geride asılı (pivotları asılma noktası).
        Model("Tespih", daire, "Dokuz_Askilar", "Tespih", civi1 + holYonu * 0.035f, civiDonusu, carpisma: false);
        Model("Kasket", daire, "Dokuz_Askilar", "Kasket", civi2 + holYonu * 0.030f, civiDonusu, carpisma: false);
        // Boş çivi: oyuncunun lambayı asacağı yer. Nişan alınabilsin diye küçük bir çarpışma kutusu ve CiviAskisi.
        CiviAskisi aski = null;
        Light asiliIsik = null;
        if (bosCivi != null)
        {
            BoxCollider bc = bosCivi.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, -0.03f, 0.03f);
            bc.size = new Vector3(0.14f, 0.16f, 0.10f);
            // Parıltı (#109): holde etkileşilecek tek şey bu çividir ve fenerin ışığında parıldar (eşyalar gibi,
            // ParlamaEkle). Kat kat zorlaşma tablosu burada geçmez (8. katta 2 m, 0,30 güç olurdu): sabit 3,5 m.
            ParlamaEkle(bosCivi, new Vector3(0f, -0.004f, 0.040f), 1f);
            var fpSeri = new SerializedObject(bosCivi.GetComponent<FenerParlamasi>());
            fpSeri.FindProperty("sabitMenzil").floatValue = 3.5f;
            fpSeri.ApplyModifiedPropertiesWithoutUndo();
            GameObject asiliLamba = AsiliLambaKur(daire, civi3, civiDonusu, out asiliIsik);
            if (asiliLamba != null)
            {
                asiliLamba.name = "Asili_Lamba";
                asiliLamba.SetActive(false);               // asılınca görünür (CiviAskisi)
                aski = bosCivi.AddComponent<CiviAskisi>();
                var aSeri = new SerializedObject(aski);
                aSeri.FindProperty("asiliLamba").objectReferenceValue = asiliLamba;
                aSeri.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ---- SALON: SOFRA, TABAKLAR, İSİM KARTLARI, GÖLGE, ANAHTARLIK ----
        // Masa 6. kattaki yerinde (SofraKur: u 2,8, v 6,3, önü kapı duvarına -v). Yer_1..4 aynı noktalarda.
        Vector3 sofraOn = yon.Yon(0f, -1f);
        float sofraDonusu = Mathf.Atan2(sofraOn.x, sofraOn.z) * Mathf.Rad2Deg;
        GameObject masa = Model("Sofra_1974", esyalar != null ? esyalar : daire, "Sofra_1974", "Masa",
                                yon.Nokta(2.8f, 0f, 6.3f), sofraDonusu, carpisma: true);
        Transform anahtarYeri = null;
        GameObject golge = null;
        if (masa != null)
        {
            // Sandalyeler: 1 Cemal (Gölge oturur, yerinde), 2 oyuncu (çekilmiş), 3 Rıza ve 4 Hasan (geri itilmiş,
            // kalkıp gitmişler). Pivotları kendi tabanlarında; z ekseni masanın önü (+z kapıya doğru).
            GameObject[] sandalye = new GameObject[5];
            for (int i = 1; i <= 4; i++)
                sandalye[i] = Model($"Sandalye_{i}", masa.transform, "Sofra_1974", $"Sandalye_{i}", Vector3.zero, 0f, carpisma: true);
            if (sandalye[2] != null) sandalye[2].transform.localPosition += new Vector3(0f, 0f, 0.24f);
            if (sandalye[3] != null) { sandalye[3].transform.localPosition += new Vector3(0.04f, 0f, -0.32f); sandalye[3].transform.localRotation = Quaternion.Euler(0f, 14f, 0f); }
            if (sandalye[4] != null) { sandalye[4].transform.localPosition += new Vector3(-0.03f, 0f, -0.36f); sandalye[4].transform.localRotation = Quaternion.Euler(0f, -11f, 0f); }
            // Tabak yuvaları ve tabaklar: 1 Cemal, 2 oyuncu (anahtar buraya), 3 Rıza, 4 Hasan.
            Vector3[] yerler = { new Vector3(-0.35f, 0.757f, 0.25f), new Vector3(0.35f, 0.757f, 0.25f),
                                 new Vector3(-0.35f, 0.757f, -0.25f), new Vector3(0.35f, 0.757f, -0.25f) };
            string[] isimler = { "bina.isim_cemal", "bina.isim_kapici", "bina.isim_riza", "bina.isim_hasan" };
            GameObject kartSablonu = Sablon("Isim_Karti");
            for (int i = 0; i < 4; i++)
            {
                GameObject yer = new GameObject($"Yer_{i + 1}");
                yer.transform.SetParent(masa.transform, false);
                yer.transform.localPosition = yerler[i];
                Model("Tabak", yer.transform, "Tabak", null, Vector3.zero, 0f, carpisma: false);
                if (i == 1) anahtarYeri = yer.transform;
                if (kartSablonu != null)
                {
                    // Kart tabağın sandalye tarafında, yazısı sandalyeye dönük (ön taraf +z, arka taraf -z).
                    float taraf = yerler[i].z > 0f ? 1f : -1f;
                    GameObject kart = (GameObject)PrefabUtility.InstantiatePrefab(kartSablonu, masa.transform);
                    kart.name = $"Isim_Karti_{i + 1}";
                    kart.transform.localPosition = new Vector3(yerler[i].x, 0.757f, yerler[i].z + taraf * 0.17f);
                    kart.transform.localRotation = Quaternion.Euler(0f, taraf > 0f ? 0f : 180f, 0f);
                    Transform yazi = kart.transform.Find("Yazi");
                    if (yazi != null)
                    {
                        TextMesh ty = yazi.GetComponent<TextMesh>();
                        if (ty != null) ty.text = Dil.Getir(isimler[i], DilKodu.TR);
                        YerelYap(yazi.gameObject, isimler[i]);
                    }
                }
            }
            // Gölge'nin anahtarlığı Cemal'in tabağının önünde (#105'teki şıkırtının sahibi masada görünür).
            Model("Anahtarlik", masa.transform, "Dokuz_Askilar", "Anahtarlik", new Vector3(-0.33f, 0.758f, 0.43f), 25f, carpisma: false);
            // Oturan Gölge: Kacis_Golgesi prefabının kopyası, Cemal'in sandalyesinde (1), yüzü masaya (sırtı kapıya).
            GameObject golgeSablonu = Sablon("Kacis_Golgesi");
            if (golgeSablonu != null)
            {
                golge = (GameObject)PrefabUtility.InstantiatePrefab(golgeSablonu, daire);
                golge.name = "Oturan_Golge";
                Transform bacak = golge.transform.Find("Govde/Bacak_Sol");
                float kalca = bacak != null ? bacak.localPosition.y : 0.95f;
                Vector3 oturak = masa.transform.TransformPoint(new Vector3(-0.35f, 0f, 0.62f));
                golge.transform.position = new Vector3(oturak.x, oturak.y + 0.47f - kalca, oturak.z);   // kalça minderde
                golge.transform.rotation = Quaternion.LookRotation(-masa.transform.forward);              // masaya bakar
                var gSeri = new SerializedObject(golge.GetComponent<GolgeAnimasyonu>());
                gSeri.FindProperty("Oturuyor").boolValue = true;
                gSeri.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        else Debug.LogWarning("[DOKUZ] Sofra_1974.fbx yok (Art/Blender/scripts/sofra_1974.py).");

        // ---- TAKVİM: EKİM 1974 ----
        // Dairenin kalıcı takvimi (Esyalar/Takvim, u 5,0, v 6,0, yüzü -u: salona bakar) üstüne yıl yazısı.
        Vector3 takvimOn = yon.Yon(-1f, 0f);
        GameObject takvimYazisi = new GameObject("Takvim_Yazisi");
        takvimYazisi.transform.SetParent(daire, false);
        takvimYazisi.transform.localPosition = yon.Nokta(4.975f, 1.22f, 6.0f);
        takvimYazisi.transform.localRotation = Quaternion.Euler(0f, Mathf.Atan2(takvimOn.x, takvimOn.z) * Mathf.Rad2Deg + 180f, 0f);
        TextMesh tt = takvimYazisi.AddComponent<TextMesh>();
        tt.text = Dil.Getir("bina.takvim_1974", DilKodu.TR);
        tt.anchor = TextAnchor.MiddleCenter;
        tt.alignment = TextAlignment.Center;
        tt.fontSize = 96;
        tt.characterSize = 0.0022f;
        tt.fontStyle = FontStyle.Bold;
        tt.color = new Color(0.08f, 0.03f, 0.03f);
        tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        takvimYazisi.GetComponent<MeshRenderer>().sharedMaterial = YaziMalzeme(tt.font);
        YerelYap(takvimYazisi, "bina.takvim_1974");

        // ---- ODA A: NURİ'NİN RADYOSUNUN İKİZİ (dairenin kalıcı radyosu çalar) ----
        AudioSource radyoKaynagi = null;
        Transform radyo = esyalar != null ? esyalar.Find("Radyo") : null;
        if (radyo != null)
        {
            radyoKaynagi = radyo.gameObject.AddComponent<AudioSource>();
            radyoKaynagi.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Daire/Radyo_Istasyon.wav");
            radyoKaynagi.loop = true;
            radyoKaynagi.playOnAwake = false;     // yolculuk başlayınca DokuzuncuKat açar
            radyoKaynagi.spatialBlend = 1f;
            radyoKaynagi.minDistance = 1.0f;
            radyoKaynagi.maxDistance = 12f;
            radyoKaynagi.volume = 0.55f;
            radyoKaynagi.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        // ---- ODA B: HASAN'IN KAMP YATAĞI, CEMAL'İN SANDIĞI ----
        SusKoy("Kamp_Yatagi_K4", daire, yon, 7.23f, 0f, 6.30f, 0f, -1f);
        SusKoy("Sandik_Cemal_K7", daire, yon, 5.36f, 0f, 7.20f, 1f, 0f);

        // ---- YÖNETİCİ BİLEŞEN ----
        DokuzuncuKat dk = kok.AddComponent<DokuzuncuKat>();
        var seri = new SerializedObject(dk);
        Transform panoNesnesi = s.Find("Asansor/Pano");
        seri.FindProperty("pano").objectReferenceValue = panoNesnesi != null ? panoNesnesi.GetComponent<AsansorPanosu>() : null;
        seri.FindProperty("daire").objectReferenceValue = daire;
        seri.FindProperty("civi").objectReferenceValue = aski;
        Transform salonKapisi = daire.Find("Kapilar/Kapi_Salon");
        seri.FindProperty("salonKapisi").objectReferenceValue = salonKapisi != null ? salonKapisi.GetComponent<IcKapi>() : null;
        seri.FindProperty("lambalar").objectReferenceValue = daire.GetComponent<DaireLambalari>();
        seri.FindProperty("elektrik").objectReferenceValue = daire.GetComponent<DaireElektrigi>();
        seri.FindProperty("golge").objectReferenceValue = golge;
        seri.FindProperty("anahtarYeri").objectReferenceValue = anahtarYeri;
        seri.FindProperty("anahtarSablonu").objectReferenceValue = Sablon("Esya_KatAnahtari");
        seri.FindProperty("radyo").objectReferenceValue = radyoKaynagi;
        seri.FindProperty("carpmaSesi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Kapi_Carpma.wav");
        seri.FindProperty("asiliLambaIsigi").objectReferenceValue = asiliIsik;
        seri.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[DOKUZ] Dokuzuncu kat kuruldu (x = -400): Sahanlik_9, Cekirdek_9 (tuğlalı), Daire_9.");
    }

    // Merdiven ağzını kapatan tuğla duvar: çekirdek genişliğinde, döşemenin altından tavana. İlk iki basamak
    // (çıkış kolunda z 0-0,54) tuğlanın önünde kalır: örülmüş olduğu bir bakışta anlaşılsın. Derzler açık renk
    // ince şeritler; tuğla rengi doğrusal değer (ekranda kiremit kırmızısı).
    static void TuglaDuvarKur(Transform d)
    {
        // MalzemeBul var olan dosyayı olduğu gibi döndürür; renk her kuruluşta yeniden yazılır (asfalt gibi).
        // 0,38'lik tuğla fenerin 6 m'sinde simsiyah okunuyordu (#109): kiremit kırmızısı biraz açıldı.
        Material tugla = MalzemeBul("Assets/Materials/Tugla.mat", new Color(0.52f, 0.26f, 0.18f));
        tugla.SetColor("_BaseColor", new Color(0.52f, 0.26f, 0.18f));
        Material derz = MalzemeBul("Assets/Materials/Tugla_Derz.mat", new Color(0.62f, 0.58f, 0.52f));
        derz.SetColor("_BaseColor", new Color(0.62f, 0.58f, 0.52f));
        GameObject kok = new GameObject("Tugla_Duvar");
        kok.transform.SetParent(d, false);
        const float Z = 0.65f, Kalin = 0.25f, Gen = CekirdekGen + 0.4f;
        Boya(Kutu("Govde", kok.transform, new Vector3(0f, 2.6f, Z), new Vector3(Gen, 7.2f, Kalin)), tugla);   // -1 .. 6,2: üst sahte katın ağzı da örülü
        for (float y = 0.22f; y < 2.9f; y += 0.33f)   // derzler yalnızca görünen bölümde
            Boya(Kutu("Derz", kok.transform, new Vector3(0f, y, Z - Kalin / 2f - 0.003f), new Vector3(Gen - 0.02f, 0.012f, 0.006f)), derz);
        // Merdiven gözü (iki kol arasındaki boşluk) sahanlık kenarı ile tuğla arasında açık kalırdı: kapat.
        Boya(Kutu("Taban", kok.transform, new Vector3(0f, -DuvarKalin / 2f, (Z - Kalin / 2f) / 2f),
                  new Vector3(KuyuGen + 0.1f, DuvarKalin, Z - Kalin / 2f)), tugla);
        foreach (Transform t in kok.transform) if (t.name == "Derz") Object.DestroyImmediate(t.GetComponent<Collider>());
    }

    // Kat eşyası prefabını plan koordinatına koyar (ZincirUygulayici'nin süs yerleştirmesinin kurucu hâli).
    static void SusKoy(string sablonAdi, Transform daire, DaireYonu yon, float u, float y, float v, float onU, float onV)
    {
        GameObject sablon = Sablon(sablonAdi);
        if (sablon == null) return;
        GameObject g = (GameObject)PrefabUtility.InstantiatePrefab(sablon, daire);
        g.name = "Sus_" + sablonAdi;
        Vector3 on = yon.Yon(onU, onV);
        g.transform.localPosition = yon.Nokta(u, y, v);
        g.transform.localRotation = Quaternion.Euler(0f, Mathf.Atan2(on.x, on.z) * Mathf.Rad2Deg, 0f);
    }

    static void IcKapiyiKilitle(Transform daire, string kapiAdi)
    {
        Transform kapi = daire.Find("Kapilar/" + kapiAdi);
        IcKapi ic = kapi != null ? kapi.GetComponent<IcKapi>() : null;
        if (ic == null) { Debug.LogWarning($"[DOKUZ] {kapiAdi} bulunamadı."); return; }
        var seri = new SerializedObject(ic);
        seri.FindProperty("kilitli").boolValue = true;
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Sil(Transform kok, string yol)
    {
        Transform t = kok.Find(yol);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    // ---------- 1974: TAZE MALZEMELER (#109) ----------
    // Dokula() her kutuya adına göre 1994'ün dokusunu verir (lekeli sıva, kirli mozaik, çizik kaplama). Bu kat
    // 1974'te yenidir: aynı adların taze karşılıkları takılır. Kural ad eşlemesidir, kutu listesi değil: ileride
    // sahanlığa eklenen her parça kendiliğinden tazelenir. Daire_9 dışarıda kalır (kaplamasını KatYoneticisi takar).
    const float LambriYuk = 1.15f;   // yağlı boya bandının üst sınırı (70'lerin apartman sahanlığı)

    public static void DokuzuncuKatiTazele()
    {
        GameObject kok = GameObject.Find("Dokuzuncu_Kat");
        if (kok == null) return;
        Transform daire = kok.transform.Find("Sahanlik_9/Daire_9");
        var taze = new System.Collections.Generic.Dictionary<string, Material>
        {
            // ad → taze malzeme. Duvar ve tavan: temiz beyaz sıva (white_plaster_02), hafif kırık beyaz.
            { "Doku_Duvar",   TazeMalzeme("Taze_Duvar",   "white_plaster_02",    new Color(0.93f, 0.91f, 0.85f), 0.22f) },
            { "Doku_Tavan",   TazeMalzeme("Taze_Tavan",   "white_plaster_02",    new Color(0.95f, 0.95f, 0.92f), 0.15f) },
            // Mozaik aynı, ama cilalı ve açık (yeni döşenmiş).
            { "Doku_Zemin",   TazeMalzeme("Taze_Zemin",   "marble_mosaic_tiles", new Color(1f, 1f, 0.98f), 0.78f) },
            // Kapı kaplaması aynı ağaç, verniği yeni: daha açık ve parlak.
            { "Doku_Kapi",    TazeMalzeme("Taze_Kapi",    "sapele_veneer",       new Color(0.66f, 0.46f, 0.32f), 0.62f) },
            { "Doku_Boya",    TazeMalzeme("Taze_Boya",    null,                  new Color(0.74f, 0.74f, 0.70f), 0.55f) },   // #126: 0,94 fenerde patlıyordu
            { "Doku_Madeni",  TazeMalzeme("Taze_Madeni",  null,                  new Color(0.86f, 0.68f, 0.30f), 0.80f, metal: 0.7f) },
            { "Doku_Demir",   TazeMalzeme("Taze_Demir",   null,                  new Color(0.10f, 0.13f, 0.11f), 0.60f) },
            { "Doku_Tirabzan", TazeMalzeme("Taze_Tirabzan", null,                new Color(0.30f, 0.16f, 0.08f), 0.65f) },
        };
        int sayi = 0;
        foreach (MeshRenderer r in kok.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (daire != null && r.transform.IsChildOf(daire)) continue;
            Material[] mats = r.sharedMaterials;
            bool degisti = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && taze.TryGetValue(mats[i].name, out Material t)) { mats[i] = t; degisti = true; }
            }
            if (degisti) { r.sharedMaterials = mats; sayi++; }
        }
        foreach (string sahanlikAdi in new[] { "Sahanlik_9", "Sahanlik_9_Ust" })
        {
            Transform sh = kok.transform.Find(sahanlikAdi);
            if (sh != null) YagliBoyaBandiKur(sh);
        }
        Debug.Log($"[DOKUZ] 1974 tazelemesi: {sayi} parça taze malzeme aldı, yağlı boya bandı kuruldu.");
    }

    // Taze malzeme: DokuKurucu'nun yaptığı gibi URP Lit, aynı doku dosyaları (dokuyu DokuKurucu.DokuAl yükler ki
    // içe aktarma ayarları aynı kalsın), kendi rengi ve parlaklığı. Var olan dosya güncellenir (GUID sabit).
    static Material TazeMalzeme(string ad, string doku, Color renk, float parlaklik, float metal = 0f)
    {
        string yol = "Assets/Materials/" + ad + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, yol);
        }
        m.SetColor("_BaseColor", renk);
        m.SetFloat("_Smoothness", parlaklik);
        m.SetFloat("_Metallic", metal);
        if (doku != null)
        {
            m.SetTexture("_BaseMap", DokuKurucu.DokuAl(doku + "_renk", false));
            m.SetTexture("_BumpMap", DokuKurucu.DokuAl(doku + "_normal", true));
            m.EnableKeyword("_NORMALMAP");
        }
        else
        {
            m.SetTexture("_BaseMap", null);
            m.SetTexture("_BumpMap", null);
            m.DisableKeyword("_NORMALMAP");
        }
        m.SetTextureScale("_BaseMap", Vector2.one);
        EditorUtility.SetDirty(m);
        return m;
    }

    // 70'lerin apartmanı: duvarın alt 1,15 metresi parlak yağlı boya (çağla yeşili), üstünde koyu bir bordür.
    // Her "Duvar_*" kutusunun (lentolar hariç) sahanlığa bakan yüzüne 1,2 cm'lik bir band konur; çarpışmasız.
    static void YagliBoyaBandiKur(Transform sahanlik)
    {
        Material boya = TazeMalzeme("Taze_YagliBoya", null, new Color(0.56f, 0.70f, 0.58f), 0.72f);
        Material bordur = TazeMalzeme("Taze_Bordur", null, new Color(0.26f, 0.38f, 0.30f), 0.72f);
        foreach (Transform duvar in sahanlik)
        {
            if (!duvar.name.StartsWith("Duvar_") || duvar.name.Contains("Lento")) continue;
            if (duvar.Find("Lambri") != null) continue;   // yeniden çalıştırmada iki kez konmasın
            // Sahanlığa bakan yön: sol duvar +x, sağ duvar -x, ön duvar +z, arka duvar -z.
            Vector3 ic = duvar.name.Contains("_Sol") ? Vector3.right : duvar.name.Contains("_Sag") ? Vector3.left
                       : duvar.name.Contains("_On") ? Vector3.forward : Vector3.back;
            if (duvar.name.StartsWith("Duvar_Arka")) ic = Vector3.back;
            bool yanDuvar = Mathf.Abs(ic.x) > 0.5f;
            Vector3 olcek = duvar.localScale;
            float uzunluk = yanDuvar ? olcek.z : olcek.x;
            Vector3 yuz = duvar.localPosition + ic * (DuvarKalin / 2f);   // duvarın iç yüzü
            GameObject band = Kutu("Lambri", duvar, Vector3.zero, Vector3.one);
            band.transform.SetParent(sahanlik, false);
            band.transform.localPosition = new Vector3(yuz.x, LambriYuk / 2f, yuz.z) + ic * 0.006f;
            band.transform.localScale = yanDuvar ? new Vector3(0.012f, LambriYuk, uzunluk) : new Vector3(uzunluk, LambriYuk, 0.012f);
            band.transform.SetParent(duvar, true);
            Boya(band, boya);
            Object.DestroyImmediate(band.GetComponent<Collider>());
            GameObject cizgi = Kutu("Bordur", sahanlik, new Vector3(yuz.x, LambriYuk + 0.015f, yuz.z) + ic * 0.008f,
                                    yanDuvar ? new Vector3(0.016f, 0.03f, uzunluk) : new Vector3(uzunluk, 0.03f, 0.016f));
            cizgi.transform.SetParent(duvar, true);
            Boya(cizgi, bordur);
            Object.DestroyImmediate(cizgi.GetComponent<Collider>());
        }
    }
}
