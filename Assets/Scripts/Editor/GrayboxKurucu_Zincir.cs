// GrayboxKurucu_Zincir.cs
// Kilit zinciri araçları (#54): zincir eşyalarının şablonları (prefab) ve geliştirme için
// test zinciri. Menü: Huzur → Zincir → Şablonları ve Test Zincirini Kur.
// GrayboxKurucu'nun parçasıdır ("partial"): Model yardımcısını ve sahne kaydetmeyi kullanır.
//
// ---------- ŞABLON (PREFAB) NEDİR ----------
// Prefab, projede dosya olarak saklanan hazır bir nesne kalıbıdır: model, çarpışma kutusu
// ve bileşenleriyle. Oyun sırasında bir anahtar gerektiğinde bu kalıptan kopya çıkarılır
// (Instantiate). Zincir her katta başka eşyalar koyduğu için eşyaları sahneye önceden
// kurmak yerine kalıptan üretiyoruz. Kalıplar KatYoneticisi'ne BAĞLANIR; bağlı olmayan bir
// dosya build'e girmeyebilir (yalnızca kullanılan varlıklar paketlenir).
//
// ---------- TEST ZİNCİRİ ----------
// #55 sekiz katın gerçek zincirlerini yazacak. O zamana kadar beş kilit türünü denemek için
// 1. kata, beşini birden kullanan bir zincir bağlanır (Assets/Zincirler/Zincir_Test.asset):
//   mutfaktaki kavanozda SALON anahtarı -> salon (anahtar)
//   salondaki takvimde şifre -> kiler (şifreli kilit, 0612)
//   kilerdeki kol -> Oda A (kol)
//   Oda A'daki komodinde sigorta -> sigorta kutusuna takılınca buzdolabı (sigorta)
//   buzdolabında tornavida -> Oda B'nin menteşeleri (tornavida)
//   Oda B'deki oyuncak kutusunda KATIN anahtarı. Banyo bu katta hiç açılmaz.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    const string SablonKlasoru = "Assets/Prefabs/Esyalar";
    const string ZincirKlasoru = "Assets/Zincirler";

    [MenuItem("Huzur/Zincir/Şablonları ve Test Zincirini Kur")]
    public static void ZincirAraclariniKur()
    {
        KlasorHazirla("Assets", "Prefabs");
        KlasorHazirla("Assets/Prefabs", "Esyalar");
        KlasorHazirla("Assets", "Zincirler");

        GameObject odaAnahtari = SablonKur("Esya_OdaAnahtari", "Anahtar", EsyaTuru.OdaAnahtari, GumusMalzeme());
        GameObject katAnahtari = SablonKur("Esya_KatAnahtari", "Anahtar", EsyaTuru.KatAnahtari, null);
        GameObject sigorta = SablonKur("Esya_Sigorta", "Buson", EsyaTuru.Sigorta, null);
        GameObject tornavida = SablonKur("Esya_Tornavida", "Tornavida", EsyaTuru.Tornavida, null);
        GameObject asmaKilit = ParcaSablonuKur("Asma_Kilit", "Asma_Kilit");
        GameObject plaka = MentesePlakasiSablonuKur();
        GameObject camKirik = ParcaSablonuKur("Cam_Kiriklari", "Cam_Kiriklari");
        // Kırıkların çarpışma kutusu olmasın: yerde duran küçük parçalar anahtarı nişanlamayı
        // engellemesin, oyuncu da takılmasın.
        if (camKirik != null)
        {
            GameObject ic = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(camKirik));
            foreach (Collider c in ic.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            PrefabUtility.SaveAsPrefabAsset(ic, AssetDatabase.GetAssetPath(camKirik));
            PrefabUtility.UnloadPrefabContents(ic);
        }
        KatZinciriVarligi test = TestZinciriKur();
        // Dekor ve not şablonları (#55).
        GameObject notSablonu = NotSablonuKur();
        GameObject koliBuyuk = ParcaSablonuKur("Koli_Buyuk", "Koli_Buyuk");
        GameObject koliKucuk = ParcaSablonuKur("Koli_Kucuk", "Koli_Kucuk");
        GameObject koliAcik = ParcaSablonuKur("Koli_Acik", "Koli_Acik");
        KatZinciriVarligi kat1 = Kat1Kur(koliBuyuk, koliKucuk, koliAcik);
        KatZinciriVarligi kat2 = Kat2Kur();
        GameObject cizimParcasi = CizimParcasiSablonuKur();
        KatZinciriVarligi kat3 = Kat3Kur();
        KatZinciriVarligi kat4 = Kat4Kur();
        KatZinciriVarligi kat5 = Kat5Kur();

        // Sahnedeki KatYoneticisi'ne bağla: şablonlar ve 1. katın zinciri.
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>(FindObjectsInactive.Include);
        if (yonetici == null) { Debug.LogWarning("[ZINCIR] Sahnede KatYoneticisi yok."); return; }
        var seri = new SerializedObject(yonetici);
        seri.FindProperty("odaAnahtariSablonu").objectReferenceValue = odaAnahtari;
        seri.FindProperty("katAnahtariSablonu").objectReferenceValue = katAnahtari;
        seri.FindProperty("sigortaSablonu").objectReferenceValue = sigorta;
        seri.FindProperty("tornavidaSablonu").objectReferenceValue = tornavida;
        seri.FindProperty("asmaKilitSablonu").objectReferenceValue = asmaKilit;
        seri.FindProperty("mentesePlakasiSablonu").objectReferenceValue = plaka;
        seri.FindProperty("camKirikSablonu").objectReferenceValue = camKirik;
        seri.FindProperty("notSablonu").objectReferenceValue = notSablonu;
        seri.FindProperty("cizimParcasiSablonu").objectReferenceValue = cizimParcasi;
        // #61: nişan menzili 2,8 m (sahnedeki oyuncunun kayıtlı değeri de güncellensin).
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>(FindObjectsInactive.Include);
        if (etk != null)
        {
            var etkSeri = new SerializedObject(etk);
            etkSeri.FindProperty("menzil").floatValue = 2.8f;
            etkSeri.ApplyModifiedPropertiesWithoutUndo();
        }
        SerializedProperty katlar = seri.FindProperty("katZincirleri");
        katlar.arraySize = KatSayaci.CatiKati;
        // 1. kat öğretici zincirini taşır (#55); öbür katlar yazıldıkça bağlanır. Test zinciri
        // (beş türün hepsi) dosya olarak duruyor ve testlerde denetleniyor ama bir kata bağlı değil.
        katlar.GetArrayElementAtIndex(0).objectReferenceValue = kat1;
        katlar.GetArrayElementAtIndex(1).objectReferenceValue = kat2;
        katlar.GetArrayElementAtIndex(2).objectReferenceValue = kat3;
        katlar.GetArrayElementAtIndex(3).objectReferenceValue = kat4;
        katlar.GetArrayElementAtIndex(4).objectReferenceValue = kat5;
        seri.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        SahneyiKaydet("Zincir");
        Debug.Log("[ZINCIR] Şablonlar, test zinciri ve 1-5. katlar kuruldu ve bağlandı.");
    }

    static void KlasorHazirla(string ust, string ad)
    {
        if (!AssetDatabase.IsValidFolder(ust + "/" + ad)) AssetDatabase.CreateFolder(ust, ad);
    }

    // Bir eşya şablonu: modelin meshi, nişan için biraz büyük (ama kabın içine sığan)
    // bir çarpışma kutusu ve Esya bileşeni. Dosya varsa üzerine yazılır (GUID'i korunur).
    static GameObject SablonKur(string ad, string dosya, EsyaTuru tur, Material ozelMalzeme)
    {
        GameObject gecici = new GameObject(ad);
        GameObject model = Model("Model", gecici.transform, dosya, null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {dosya}.fbx yok; {ad} kurulmadı."); return null; }
        if (ozelMalzeme != null) model.GetComponent<MeshRenderer>().sharedMaterial = ozelMalzeme;

        // Çarpışma kutusu modelin sınırlarından en az 16 x 8 x 16 cm: küçük eşyayı nişanlamak
        // kolay olsun (#61, Şahan zorlandı). Kavanozun dışına taşsa da sorun değil: kapağı
        // kapalı kabın içindeki eşya zaten alınamıyor (Esya.EtkilesimeAcik).
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center + model.transform.localPosition;
        // Vector3.Max: iki vektörün her ekseninden büyük olanı alır (modelden küçük olmasın).
        c.size = Vector3.Max(b.size, new Vector3(0.16f, 0.08f, 0.16f));

        Esya esya = gecici.AddComponent<Esya>();
        esya.Ayarla(tur, "", 0);

        string yol = $"{SablonKlasoru}/{ad}.prefab";
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, yol);
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Eşya olmayan bir parça şablonu (asma kilit gibi): yalnızca model ve çarpışma kutusu.
    // Çarpışma kutusu nişanı kolaylaştırır; etkileşim kilidin takılı olduğu kapıdan gelir.
    static GameObject ParcaSablonuKur(string ad, string dosya)
    {
        GameObject gecici = new GameObject(ad);
        GameObject model = Model("Model", gecici.transform, dosya, null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning($"[ZINCIR] {dosya}.fbx yok."); return null; }
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center;
        c.size = b.size;
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/{ad}.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Menteşe plakası şablonu: plaka ve iki vida, vidalar kökün çocukları (MenteseSokucu
    // onları adlarıyla bulup tek tek söker).
    static GameObject MentesePlakasiSablonuKur()
    {
        GameObject gecici = new GameObject("Mentese_Plakasi");
        if (Model("Plaka", gecici.transform, "Mentese_Plakasi", "Mentese_Plakasi", Vector3.zero, 0f, carpisma: false) == null)
        { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Mentese_Plakasi.fbx yok."); return null; }
        Model("Vida_1", gecici.transform, "Mentese_Plakasi", "Vida_1", Vector3.zero, 0f, carpisma: false);
        Model("Vida_2", gecici.transform, "Mentese_Plakasi", "Vida_2", Vector3.zero, 0f, carpisma: false);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Mentese_Plakasi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Okunabilir not kâğıdı şablonu: model, nişan için büyükçe çarpışma kutusu, YoneticiNotu.
    static GameObject NotSablonuKur()
    {
        GameObject gecici = new GameObject("Not_Kagidi");
        GameObject model = Model("Model", gecici.transform, "Not_Kagidi", null, Vector3.zero, 0f, carpisma: false);
        if (model == null) { Object.DestroyImmediate(gecici); Debug.LogWarning("[ZINCIR] Not_Kagidi.fbx yok."); return null; }
        Bounds b = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = b.center;
        c.size = Vector3.Max(b.size, new Vector3(0.18f, 0.04f, 0.24f));
        gecici.AddComponent<YoneticiNotu>();
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Not_Kagidi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // ---------- 1. KAT: ÖĞRETİCİ ZİNCİR (#55, Docs/hikaye.md §7) ----------
    // Daire 2, taşınmış aile: ortalıkta koliler; salon, Oda B, kiler ve banyo bu katta kapalı.
    // Oyuncu burada kuralları öğrenir, her adım bir şey öğretir:
    //   1. Kapının önündeki ayakkabılıkta tornavida: alet alınınca çantaya gider (AL).
    //   2. Antrede Yönetim'in tahliye yazısı: notlar okunur (OKU).
    //   3. Mutfakta kavanoz ve yanında kiracının notu: "anahtarı kavanoza koydum, kapağı sıkıştı".
    //      Sıkışan kapak aletle kırılır (KIR); içinde Oda A'nın anahtarı.
    //   4. Oda A doğru anahtarla açılır (KİLİTLİ -> AÇ).
    //   5. Komodinin çekmecesinde katın anahtarı (çekmece AÇ, AL) -> kat kapısı.
    static KatZinciriVarligi Kat1Kur(GameObject koliBuyuk, GameObject koliKucuk, GameObject koliAcik)
    {
        string yol = $"{ZincirKlasoru}/Kat_1.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "1. kat, daire 2, taşınmış aile (öğretici). Ayakkabılıkta tornavida; kavanoz kırılır, içinde Oda A'nın " +
                     "anahtarı; Oda A'nın komodininde katın anahtarı. Salon, Oda B, kiler ve banyo kapalı. Notlar: tahliye yazısı, " +
                     "kiracının notu. Dekor: koliler.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Salon", "Oda_B", "Oda_C", "Banyo" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kavanoz", Yol = "Esyalar/Tezgah/Kavanoz", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kavanoz/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "ayakkabilik", Y = 1.025f, Donus = 80f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaA_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "kavanoz", Y = 0.012f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.375f, Donus = 15f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kavanoz", Tur = KilitTuru.Kirma, OdaKapisi = false, Hedef = "kavanoz", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "odaA_anahtari" });
        v.zincir = z;

        // NOTLAR: tahliye yazısı antrede sigorta kutusunun yanında duvarda; kiracının notu
        // tezgâhta kavanozun yanında (ikisi de çapaya göre, aynalı dairelerde de yanında durur).
        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat1_tahliye", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            // Tezgâhın arkasında, duvara dayalı dik duruyor: beyaz tezgâhın üstüne yatık konunca
            // görünmüyordu (oynatma modu görüntüsü).
            new KatNotu { Metin = "not.kat1_kavanoz", Yol = "Esyalar/Tezgah", X = 0.40f, Y = 0.902f, Z = -0.285f, Donus = -8f },
        };

        // DEKOR: koliler (plan konumları, bkz. DairePlani). Kapıların önünden ve yollardan uzakta.
        // Yerel fonksiyon: yalnızca bu fonksiyonun içinde kullanılan kısa bir yardımcı (koli yazmak kısa olsun).
        KatSusu Koli(GameObject s, float u, float y, float vv, float onU, float onV) { return new KatSusu { Sablon = s, X = u, Y = y, Z = vv, OnU = onU, OnV = onV }; }
        v.suslar = new List<KatSusu>
        {
            Koli(koliBuyuk, 2.20f, 0f, -0.45f, -1f, 0.3f),    // antre: köşede büyük koli
            Koli(koliKucuk, 2.20f, 0.40f, -0.45f, -1f, 0.1f), //        üstünde küçük koli
            Koli(koliAcik, 0.55f, 0f, 2.35f, 0.4f, -1f),     //        kapının yanında açık koli
            Koli(koliBuyuk, 4.40f, 0f, 4.30f, 0f, -1f),      // koridor: salon ile Oda B kapıları arasında
            Koli(koliKucuk, 10.10f, 0f, 3.40f, -1f, 0f),     // koridorun dibinde
            Koli(koliAcik, 8.60f, 0f, 2.30f, -0.5f, -1f),    // mutfak: kapının içinde açık koli
            Koli(koliBuyuk, 9.90f, 0f, 5.40f, -1f, 0.2f),    // Oda A: yatağın gittiği köşede
        };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s2 = z.Cozumle(odalar);
        if (!s2.Cozulebilir) Debug.LogError("[ZINCIR] 1. kat zinciri çözülemiyor: " + string.Join("; ", s2.Hatalar));
        else Debug.Log("[ZINCIR] 1. kat zinciri çözülebilir: " + string.Join(" -> ", s2.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 2. KAT: SESLE BULMA (#62, Docs/hikaye.md §7) ----------
    // Daire 3, yaşlı Nuri'nin dairesi; Nuri hastanede. Banyo ve Oda B kapalı; salon, mutfak ve
    // Oda A'nın kapıları kapalı ama kilitsiz; kiler (Oda C) şifreli asma kilitli.
    //   1. Antrede Yönetim'in notu: Nuri Bey hastaneye kaldırıldı (hikâye).
    //   2. Koridorda kilerin kapısının yanında Nuri'nin notu: kilidin numarası radyonun istasyonu.
    //   3. Oyuncu daireye girdikten 4 sn sonra radyo kendiliğinden çalar. Oda A'nın kapısı kapalı:
    //      ses boğuk gelir, oyuncu hangi kapı olduğunu kulağıyla bulur (CalanEsya).
    //   4. Oda A'da komodinin üstünde radyo; kadranında "102.7" -> kilerin şifresi 1027.
    //   5. Kilerde alet çantasının içinde katın anahtarı.
    static KatZinciriVarligi Kat2Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_2.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "2. kat, daire 3, yaşlı Nuri (sesle bulma). Radyo Oda A'da kendiliğinden çalar, kapalı kapının " +
                     "arkasından boğuk duyulur; kadranındaki istasyon (102.7) kilerin şifresi; kilerde alet çantasında " +
                     "katın anahtarı. Banyo ve Oda B kapalı. Notlar: Yönetim'in hastane notu, Nuri'nin şifre notu.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "radyo", Yol = "Esyalar/Radyo", Oda = "Oda_A" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "alet_cantasi", Yol = "Esyalar/Alet_Cantasi", Oda = "Oda_C" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "alet_cantasi", Y = 0.03f, Donus = 20f });
        // İpucu radyonun kadran camına yazılır: camın ortası radyonun ekseninde x = -0,085,
        // y = 0,137; ön yüzü z = 0,069 (radyo.py'nin yazdırdığı KADRAN_MERKEZ, Unity eksenine
        // çevrilmiş). Yazı takvimdekinin %70'i: %40'ı ("FM 102.7") 1,3 m'den okunmuyordu.
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "radyo",
                                          Sifre = "1027", IpucuMetni = "102.7", IpucuX = -0.085f, IpucuY = 0.137f, IpucuZ = 0.0705f,
                                          IpucuOlcek = 0.7f });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Yönetim'in notu antrede, sigorta kutusunun yanında (1. kattaki tahliye yazısının yeri).
            new KatNotu { Metin = "not.kat2_hastane", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            // Nuri'nin notu koridor duvarında, kilerin kapısının solunda (kapı u = 5,25-6,05).
            // Koridorun duvar yüzü v = 2,95; not ön yüzüyle koridora (+v) bakar.
            new KatNotu { Metin = "not.kat2_kiler", X = 4.95f, Y = 1.40f, Z = 2.952f, OnU = 0f, OnV = 1f },
        };
        v.suslar = new List<KatSusu>();

        v.sesler = new List<KatSesi>
        {
            new KatSesi { Yol = "Esyalar/Radyo", Oda = "Oda_A", Gecikme = 4f, Ses = 0.8f,
                          Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Daire/Radyo_Istasyon.wav") },
        };
        if (v.sesler[0].Klip == null) Debug.LogError("[ZINCIR] Radyo_Istasyon.wav bulunamadı (Art/Ses/scripts/kayit_isle.py radyo).");

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s2 = z.Cozumle(odalar);
        if (!s2.Cozulebilir) Debug.LogError("[ZINCIR] 2. kat zinciri çözülemiyor: " + string.Join("; ", s2.Hatalar));
        else Debug.Log("[ZINCIR] 2. kat zinciri çözülebilir: " + string.Join(" -> ", s2.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 3. KAT: PARÇA BİRLEŞTİRME (#63, Docs/hikaye.md §7) ----------
    // Daire 1, çocuklu aile (Kerem). Kerem "dokuzuncu kattaki adamı" çizmiş, resmi dört parçaya
    // yırtıp saklamış; annesi yatak odasının (Oda A) kilidinin şifresini o resme yazdırmış
    // (koridordaki notu). Banyo ve kiler kapalı; salon ve mutfak açık.
    //   1. Kapının önündeki ayakkabılıkta ilk parça: oyuncu daha içeri girmeden merak eder.
    //   2. Salonda yerde ikinci parça (kapının arkasındaki köşede).
    //   3. Mutfakta: çekmecede Oda B'nin (çocuk odası) anahtarı, buzdolabında üçüncü parça.
    //   4. Oda B'de oyuncak sandığında dördüncü parça -> resim tamam: köşelerde 0 3 0 9.
    //   5. Oda A şifreli asma kilitli (0309); komodinin çekmecesinde katın anahtarı.
    // İki kilit (odaB anahtarlı, odaA parçalı şifreli): büyüme çizelgesinin 3. katı (1-1-2-...).
    static KatZinciriVarligi Kat3Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_3.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "3. kat, daire 1, çocuklu aile (parça birleştirme). Kerem'in yırtık çiziminin dört parçası: ayakkabılıkta, " +
                     "salonda yerde, buzdolabında, Oda B'deki oyuncak sandığında. Oda B'nin anahtarı mutfak çekmecesinde. " +
                     "Birleşen resmin köşelerinde 0 3 0 9: Oda A'nın şifresi; komodinde katın anahtarı. Banyo ve kiler kapalı.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        // Salonun kapısı u = 0,85-1,75'te (v = 4,70 duvarında) ve içeri açılıyor; parça kapının
        // arkasında kalmasın diye antre duvarı tarafındaki köşede, yerde (u 0,5, v 6,9).
        z.Yerler.Add(new ZincirYeri { Kimlik = "salon_yeri", PlandaNokta = true, Oda = "Salon", U = 0.55f, Y = 0.004f, V = 6.9f, Donus = 25f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "cekmece", Yol = "Esyalar/Tezgah/Cekmece_1", Oda = "Mutfak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p1", Tur = EsyaTuru.Parca, Yer = "ayakkabilik", Y = 1.025f, Donus = -15f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p2", Tur = EsyaTuru.Parca, Yer = "salon_yeri" });
        // Çekmecenin içi: tezgâhın ekseninde x 0,59, y 0,64, z 0,04 (tezgah.py'deki üst çekmecenin tabanı).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "cekmece", X = 0.59f, Y = 0.64f, Z = 0.04f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p3", Tur = EsyaTuru.Parca, Yer = "buzdolabi", Y = 0.47f, Donus = 10f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "p4", Tur = EsyaTuru.Parca, Yer = "oyuncak", Y = 0.03f, Donus = -20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "komodin", Y = 0.375f, Donus = 15f });
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

        // NOT: annenin babaya notu koridorda, Oda B'nin kapısının solunda (kapı u = 5,95-6,75,
        // v = 4,70 duvarında). Koridorun o duvardaki yüzü v = 4,65; not koridora (-v) bakar.
        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat3_anne", X = 5.45f, Y = 1.40f, Z = 4.648f, OnU = 0f, OnV = -1f },
        };
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s3 = z.Cozumle(odalar);
        if (!s3.Cozulebilir) Debug.LogError("[ZINCIR] 3. kat zinciri çözülemiyor: " + string.Join("; ", s3.Hatalar));
        else Debug.Log("[ZINCIR] 3. kat zinciri çözülebilir: " + string.Join(" -> ", s3.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 4. KAT: IŞIK VE KARANLIK (#64, Docs/hikaye.md §7) ----------
    // Daire 4, boş ve elektriksiz. Elektriği önceki kapıcı Hasan kesmiş, sigortayı mutfak
    // dolabına kilitlemiş (antredeki notu). Yalnızca salon ve mutfak açık.
    //   1. KARANLIK: dolabın şifresi salonun duvarına fosforlu boyayla yazılı. Kafa lambası
    //      açıkken görünmez; oyuncu lambayı söndürüp beklerse rakamlar belirir (KaranliktaParlayan).
    //   2. Dolapta sigorta; antredeki sigorta kutusuna takılınca elektrik gelir.
    //   3. IŞIK: antre, koridor, salon ve mutfağın tavan lambaları titreyerek yanar; buzdolabı
    //      açılır, iç ışığında Hasan'ın ikinci notu ve katın anahtarı.
    // İki kilit (dolap şifreli, buzdolabı sigortalı): büyüme çizelgesinin 4. katı.
    static KatZinciriVarligi Kat4Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_4.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "4. kat, daire 4, boş ve elektriksiz (ışık ve karanlık). Dolabın şifresi salon duvarında fosforlu boyayla: " +
                     "yalnızca kafa lambası kapalıyken görünür. Dolapta sigorta; takılınca lambalar yanar ve buzdolabı açılır, " +
                     "içinde Hasan'ın notu ve katın anahtarı. Yalnızca salon ve mutfak açık.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Banyo", "Oda_A", "Oda_B", "Oda_C" });
        // Salonun landing tarafındaki duvarı (u = 0; iç yüzü astarla ~3 cm içeride). Yazı duvarın
        // 4,5 cm önünde, göz hizasının biraz üstünde; ön yüzü odaya (+u) bakar.
        z.Yerler.Add(new ZincirYeri { Kimlik = "duvar", PlandaNokta = true, Oda = "Salon", U = 0.045f, Y = 1.60f, V = 6.4f, OnU = 1f, OnV = 0f });
        z.Yerler.Add(new ZincirYeri { Kimlik = "dolap", Yol = "Esyalar/Tezgah", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kapak_1" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        // Sigorta dolabın iç rafında (tezgah.py: raf üstü 0,47 m; birinci kapağın arkası x 0).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "dolap", X = 0f, Y = 0.475f, Z = 0.02f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "buzdolabi", Y = 0.47f, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "dolap", Tur = KilitTuru.Sifre, OdaKapisi = false, Hedef = "dolap", Gerekli = "duvar",
                                          Sifre = "4816", IpucuMetni = "4816", IpucuOlcek = 4f, IpucuKaranlikta = true });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "buzdolabi", Tur = KilitTuru.Sigorta, OdaKapisi = false, Hedef = "buzdolabi", Gerekli = "sigorta" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            // Hasan'ın ilk notu antrede, sigorta kutusunun yanında (önceki katların notlarının yeri).
            new KatNotu { Metin = "not.kat4_giris", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
            // İkinci notu buzdolabının üst rafında, arka duvara dayalı (buzdolabi.py: raf 0,80 m,
            // arka iç duvar z = -0,265). Buzdolabının iç ışığı tam bu rafı aydınlatır.
            new KatNotu { Metin = "not.kat4_buzdolabi", Yol = "Esyalar/Buzdolabi", X = 0.05f, Y = 0.805f, Z = -0.255f, Donus = -6f },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.sesler = new List<KatSesi>();
        v.yananOdalar = new List<string> { "Antre", "Hol", "Salon", "Mutfak" };

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s4 = z.Cozumle(odalar);
        if (!s4.Cozulebilir) Debug.LogError("[ZINCIR] 4. kat zinciri çözülemiyor: " + string.Join("; ", s4.Hatalar));
        else Debug.Log("[ZINCIR] 4. kat zinciri çözülebilir: " + string.Join(" -> ", s4.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // ---------- 5. KAT: MEKANİZMA (#65, Docs/hikaye.md §7) ----------
    // Daire 2, Cemal'in son çalıştığı daire. Antrede Cemal'in iş listesi: banyo lavabosu
    // damlatıyor; sifonu sökmeden ana vanayı kapat (salonda gardırobun arkasında); banyo kapısı
    // içeriden kilitli, menteşeleri sök. Mutfak, Oda A ve Oda B kapalı.
    //   1. Banyo: menteşeler tornavidayla sökülür (tornavida kapıcıda yoksa kilerde, Cemal'in
    //      alet çantasında). İçeride lavabo; sifon kilitli ("vana açık, su basar").
    //   2. MEKANİZMA: salondaki gardırop yana itilir, arkasında ana su vanası.
    //   3. MEKANİZMA: vananın çarkı çevrilir: su kesilir, lavabonun damlası susar, sifon sökülebilir.
    //   4. Sifonun içinde katın anahtarı.
    // İki kilit (banyo menteşeli, sifon vanaya bağlı "kol"): büyüme çizelgesinin 5. katı.
    static KatZinciriVarligi Kat5Kur()
    {
        string yol = $"{ZincirKlasoru}/Kat_5.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "5. kat, daire 2, Cemal'in son çalıştığı daire (mekanizma). Banyo içeriden kilitli: menteşeler tornavidayla " +
                     "sökülür. Salondaki gardırop itilince arkasında ana vana; çark çevrilince su kesilir, damla susar, lavabonun " +
                     "sifonu sökülür, içinde katın anahtarı. Mutfak, Oda A, Oda B kapalı.";
        var z = new KilitZinciri();
        z.KapaliOdalar.AddRange(new[] { "Mutfak", "Oda_A", "Oda_B" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "alet_cantasi", Yol = "Esyalar/Alet_Cantasi", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "vana", Yol = "Esyalar/Vana/Cark", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "sifon", Yol = "Esyalar/Lavabo/Sifon", Oda = "Banyo" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "alet_cantasi", Y = 0.03f, Donus = 20f });
        // Anahtar sifonun ağzında, kenarın hemen altında (sifonun pivotu kendi üst ortası; kap 17 cm
        // derin). 9 cm aşağıdayken yere inen sifonun içinde görünmüyordu (oynatma modu görüntüsü).
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "sifon", Y = -0.02f, Donus = 30f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "sifon", Tur = KilitTuru.Kol, OdaKapisi = false, Hedef = "sifon", Gerekli = "vana" });
        v.zincir = z;

        v.notlar = new List<KatNotu>
        {
            new KatNotu { Metin = "not.kat5_liste", Yol = "Esyalar/Sigorta_Kutusu", X = 0.30f, Y = -0.10f, Z = 0.002f },
        };
        v.parcalar = new List<KatParcasi>();
        v.suslar = new List<KatSusu>();
        v.yananOdalar = new List<string>();
        // Lavabonun damlası: oyuncu daireye girer girmez duyulur; kapatılamaz; vana çevrilince
        // ("sifon" kilidi çözülünce) kalıcı olarak susar. Ses 1,6 sn arayla düzenli bir damla.
        v.sesler = new List<KatSesi>
        {
            new KatSesi { Yol = "Esyalar/Lavabo", Oda = "Banyo", Gecikme = 0f, Ses = 0.7f, Kapatilabilir = false, SusturanKilit = "sifon",
                          Klip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Anomali/Boru_Damla.wav") },
        };
        if (v.sesler[0].Klip == null) Debug.LogError("[ZINCIR] Boru_Damla.wav bulunamadı.");

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s5 = z.Cozumle(odalar);
        if (!s5.Cozulebilir) Debug.LogError("[ZINCIR] 5. kat zinciri çözülemiyor: " + string.Join("; ", s5.Hatalar));
        else Debug.Log("[ZINCIR] 5. kat zinciri çözülebilir: " + string.Join(" -> ", s5.AcilanKilitler));
        EditorUtility.SetDirty(v);
        return v;
    }

    // Çizim dokusunun içe aktarma ayarları: saydamlık korunsun, kenardan taşmasın (Clamp),
    // arayüzde bulanık durmasın diye sıkıştırma yüksek kalite.
    static void CizimDokusunuAyarla(string yol)
    {
        TextureImporter ti = AssetImporter.GetAtPath(yol) as TextureImporter;
        if (ti == null) { Debug.LogError("[ZINCIR] Çizim dokusu yok: " + yol + " (Art/Cizim/scripts/cizim_uret.py)"); return; }
        bool degisti = !ti.alphaIsTransparency || ti.wrapMode != TextureWrapMode.Clamp || ti.textureCompression != TextureImporterCompression.CompressedHQ;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        if (degisti) ti.SaveAndReimport();
    }

    // Dokudaki saydam olmayan piksellerin sınırı, UV olarak (0-1, sol alttan). Dosyayı diskten
    // okur (içe aktarılmış doku okunamaz olarak işaretli olabilir).
    static Rect ParcaBolgesi(string yol)
    {
        var t = new Texture2D(2, 2);
        t.LoadImage(System.IO.File.ReadAllBytes(yol));      // PNG'yi çözer; boyu kendisi ayarlar
        Color32[] p = t.GetPixels32();                       // satırlar ALTTAN başlar (UV ile aynı)
        int w = t.width, h = t.height, x0 = w, y0 = h, x1 = -1, y1 = -1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (p[y * w + x].a > 0) { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y); }
        Object.DestroyImmediate(t);
        if (x1 < 0) return new Rect(0, 0, 1, 1);
        return new Rect((float)x0 / w, (float)y0 / h, (float)(x1 - x0 + 1) / w, (float)(y1 - y0 + 1) / h);
    }

    // Yere serilen çizim parçası: nişan için bir çarpışma kutusu, Esya (Parca) ve yüzü yukarı
    // bakan ince bir dörtgen ("Kagit"). Dörtgenin boyu ve gösterdiği bölge her parçada ayrı;
    // uygulayıcı yerleştirirken ayarlar (ZincirUygulayici.ParcayiGiydir).
    static GameObject CizimParcasiSablonuKur()
    {
        GameObject gecici = new GameObject("Esya_CizimParcasi");
        // CreatePrimitive(Quad): Unity'nin hazır 1 x 1 m dörtgeni. Çarpışma bileşenini atıyoruz:
        // nişanı kökteki kutu karşılıyor.
        GameObject kagit = GameObject.CreatePrimitive(PrimitiveType.Quad);
        kagit.name = "Kagit";
        Object.DestroyImmediate(kagit.GetComponent<Collider>());
        kagit.transform.SetParent(gecici.transform, false);
        kagit.transform.localPosition = new Vector3(0f, 0.002f, 0f);
        // Dörtgenin yüzü -Z'ye bakar; x ekseninde 90° çevirince yukarı bakar.
        kagit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        kagit.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
        kagit.GetComponent<MeshRenderer>().sharedMaterial = CizimMalzemesi();
        kagit.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        BoxCollider c = gecici.AddComponent<BoxCollider>();
        c.center = new Vector3(0f, 0.03f, 0f);
        c.size = new Vector3(0.18f, 0.06f, 0.18f);
        Esya esya = gecici.AddComponent<Esya>();
        esya.Ayarla(EsyaTuru.Parca, "", 0);
        GameObject sablon = PrefabUtility.SaveAsPrefabAsset(gecici, $"{SablonKlasoru}/Esya_CizimParcasi.prefab");
        Object.DestroyImmediate(gecici);
        return sablon;
    }

    // Parçaların ortak malzemesi: URP Lit, "alfa kırpma" açık (yırtık kenarın dışı görünmez).
    // Doku ve bölge nesne başına MaterialPropertyBlock ile verilir.
    static Material CizimMalzemesi()
    {
        const string Yol = "Assets/Materials/Cizim_Parca.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, Yol);
        }
        m.SetFloat("_AlphaClip", 1f);
        m.SetFloat("_Cutoff", 0.5f);
        m.EnableKeyword("_ALPHATEST_ON");
        m.SetFloat("_Smoothness", 0.1f);
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Cizim/Cizim_Kat3_P1.png"));
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material GumusMalzeme()
    {
        const string Yol = "Assets/Materials/Anahtar_Gumus.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (m != null) return m;
        m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", new Color(0.72f, 0.73f, 0.75f));
        m.SetFloat("_Metallic", 1f);
        m.SetFloat("_Smoothness", 0.55f);
        AssetDatabase.CreateAsset(m, Yol);
        return m;
    }

    // Test zinciri: her kurulumda koddan yeniden yazılır, böylece elle bozulamaz.
    static KatZinciriVarligi TestZinciriKur()
    {
        string yol = $"{ZincirKlasoru}/Zincir_Test.asset";
        KatZinciriVarligi v = AssetDatabase.LoadAssetAtPath<KatZinciriVarligi>(yol);
        if (v == null)
        {
            v = ScriptableObject.CreateInstance<KatZinciriVarligi>();
            AssetDatabase.CreateAsset(v, yol);
        }
        v.aciklama = "Geliştirme zinciri (#54, #61): altı kilit türü tek dairede. Kapının önündeki ayakkabılıkta tornavida; " +
                     "tornavidayla kavanoz kırılır, içinde salon anahtarı; takvimde kilerin şifresi; kilerdeki kol Oda A'yı açar; " +
                     "komodinde sigorta, buzdolabını açar; buzdolabında Oda B'nin anahtarı; oyuncak kutusunda katın anahtarı. " +
                     "Banyonun menteşeleri aynı tornavidayla sökülür (yan oda).";
        var z = new KilitZinciri();
        z.Yerler.Add(new ZincirYeri { Kimlik = "ayakkabilik", Yol = "Sahanlik:Kapi_{n}/Ayakkabilik", Oda = KilitZinciri.SahanlikOdasi });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kavanoz", Yol = "Esyalar/Tezgah/Kavanoz", Oda = "Mutfak", KilitYolu = "Esyalar/Tezgah/Kavanoz/Kapak" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "takvim", Yol = "Esyalar/Takvim", Oda = "Salon" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "kol", Yol = "Esyalar/Duvar_Kolu/Kol", Oda = "Oda_C" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "komodin", Yol = "Esyalar/Komodin/Cekmece", Oda = "Oda_A" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "buzdolabi", Yol = "Esyalar/Buzdolabi", Oda = "Mutfak", KilitYolu = "Esyalar/Buzdolabi/Kapi" });
        z.Yerler.Add(new ZincirYeri { Kimlik = "oyuncak", Yol = "Esyalar/Oyuncak_Kutusu", Oda = "Oda_B" });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "salon_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "kavanoz", Y = 0.012f, Donus = 30f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "sigorta", Tur = EsyaTuru.Sigorta, Yer = "komodin", Y = 0.375f, Donus = 10f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "tornavida", Tur = EsyaTuru.Tornavida, Yer = "ayakkabilik", Y = 1.025f, Donus = 80f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "odaB_anahtari", Tur = EsyaTuru.OdaAnahtari, Yer = "buzdolabi", Y = 0.47f, Donus = 20f });
        z.Esyalar.Add(new ZincirEsyasi { Kimlik = "kat_anahtari", Tur = EsyaTuru.KatAnahtari, Yer = "oyuncak", Y = 0.03f, Donus = 20f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kavanoz", Tur = KilitTuru.Kirma, OdaKapisi = false, Hedef = "kavanoz", Gerekli = "tornavida" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "salon", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Salon", Gerekli = "salon_anahtari" });
        // Şifrenin ipucu takvimin yaprağına yazılır (yaprağın üst yarısı, ön yüzün 1 mm önü).
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "kiler", Tur = KilitTuru.Sifre, OdaKapisi = true, Hedef = "Oda_C", Gerekli = "takvim",
                                          Sifre = "0612", IpucuMetni = "0612", IpucuY = 0.24f, IpucuZ = 0.011f });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaA", Tur = KilitTuru.Kol, OdaKapisi = true, Hedef = "Oda_A", Gerekli = "kol" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "buzdolabi", Tur = KilitTuru.Sigorta, OdaKapisi = false, Hedef = "buzdolabi", Gerekli = "sigorta" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "odaB", Tur = KilitTuru.Anahtar, OdaKapisi = true, Hedef = "Oda_B", Gerekli = "odaB_anahtari" });
        z.Kilitler.Add(new ZincirKilidi { Kimlik = "banyo", Tur = KilitTuru.Tornavida, OdaKapisi = true, Hedef = "Banyo", Gerekli = "tornavida" });
        v.zincir = z;

        // Kural: kaydetmeden önce zincirin çözülebilir olduğunu doğrula.
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        KilitZinciri.Sonuc s = z.Cozumle(odalar);
        if (!s.Cozulebilir) Debug.LogError("[ZINCIR] Test zinciri çözülemiyor: " + string.Join("; ", s.Hatalar));
        else Debug.Log("[ZINCIR] Test zinciri çözülebilir: " + string.Join(" -> ", s.AcilanKilitler));

        EditorUtility.SetDirty(v);
        return v;
    }
}
