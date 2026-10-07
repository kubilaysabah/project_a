// KatDenetimi.cs
// Açık katın yürünebilirlik, nişan ve karanlık denetimi (#83, 2026-09-30).
// YALNIZCA EDITOR'DE derlenir (#if UNITY_EDITOR): oyunun build'ine girmez. Oyun modunda, katın dairesi
// açıkken çağrılır:  KatDenetimi.Denetle(kat, "C:/klasor");
//
// NEDEN GEREKLİ
// Oyun botu (OyunBotu) eşyalara doğrudan dokunarak oynar; yürümez, nişan almaz, karanlığı görmez.
// Bu yüzden kat 2'deki tenekenin ve kat 5'teki masanın üstündeki anahtarların hiç alınamadığını
// yakalayamadı. Bu denetim oyuncunun gerçekten yapabildiğini ölçer:
//   1. YÜRÜME: oyuncunun kapsülü (yarıçap 0,3; ayakta 1,75, eğilmiş 0,8) sahanlıktaki başlangıç
//      noktasından 10 cm'lik adımlarla dolaşır (genişlik öncelikli arama, "BFS": bir noktadan başlayıp
//      önce bütün komşuları, sonra onların komşularını ziyaret eder). Basamak eşiği (0,35) altındaki
//      engeller aşılır. Bütün oda kapıları ve açık dairelerin giriş kapıları açık varsayılır (her kilit
//      sonunda açılıyor). Sonuç: her odanın yürünebilir alanı; açılması gereken ama hiç ulaşılamayan oda
//      "ULAŞILMIYOR" diye yazılır, yolu en çok tıkayan eşyalar "engel" listesinde.
//   2. NİŞAN: her görev eşyası ve kilit parçası için yalnızca YÜRÜNEBİLİR noktalardan (ayakta göz 1,65,
//      eğilmiş 0,6) oyuncunun kamerası ona çevrilir ve oyunun kendi nişan fonksiyonu
//      (Etkilesimci.HedefBul) çağrılır. Hiçbir noktadan seçilemeyen "NİŞANLANAMIYOR" diye yazılır.
//   3. HARİTA: her açık dairenin planı tepeden bir resim olarak yazılır (yürünen, tıkalı, ulaşılmayan yer).
//   4. KARANLIK: her görev eşyası en iyi noktadan, oyunun gerçek ışığıyla (daire karanlık, yalnızca kafa
//      lambası) oyuncunun kamerasıyla çekilir; eşyanın bölgesinin parlaklığı ölçülür, görüntü yazılır.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class KatDenetimi
{
    const float Adim = 0.10f;          // arama ızgarasının adımı (m)
    const float Yaricap = 0.30f;       // oyuncunun kapsül yarıçapı (OyuncuKurucu.GovdeYaricap)
    const float AyaktaBoy = 1.75f, EgikBoy = 0.80f;
    const float AyaktaGoz = 1.65f, EgikGoz = 0.60f;
    const float Basamak = 0.35f;       // CharacterController.stepOffset: bu yükseklikteki engel aşılır
    const float Menzil = 2.75f;        // nişan menzili (Etkilesimci 2,8)

    // Bir ızgara hücresi: zeminin yüksekliği ve oyuncu orada ayakta durabiliyor mu (yoksa yalnızca eğilerek).
    struct Hucre { public float Y; public bool Ayakta; }


    // Dokuzuncu kat seti (#108) sahnede hep açık durur; denetim yalnızca katın daireleri ve kapılarıyla ilgilenir.
    // (#110: 6. katta Daire_9 ilk sırada gelince sahanlık yanlış bulunuyor, her eşya "0 nokta" çıkıyordu.)
    static DaireEksenleri[] KatDaireleri()
    {
        return System.Array.FindAll(Object.FindObjectsByType<DaireEksenleri>(), d => !DokuzuncuKat.SetinParcasi(d));
    }
    static IcKapi[] KatKapilari()
    {
        return System.Array.FindAll(Object.FindObjectsByType<IcKapi>(), k => !DokuzuncuKat.SetinParcasi(k));
    }

    public static string Denetle(int kat, string klasor)
    {
        var rapor = new StringBuilder();
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>();
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>();
        if (oyuncu == null || etk == null || yonetici == null) return "oyuncu ya da kat yöneticisi yok (oyun modunda mı?)";
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        Varlik.Kapali = true;   // denetim oyuncuyu ışınlar; Varlık (#86) işlemesin

        // Bütün oda kapıları açılır (balkon kapısı hep kilitli kalır; onu açmaya gerek yok).
        foreach (IcKapi k in KatKapilari())
            if (k.gameObject.activeInHierarchy && !k.name.Contains("Balkon")) { k.Kilitle(false); k.Ayarla(true); }

        // Açık daireler ve sahanlıktaki giriş kapıları (7. katta gerçek kapı normalde çalınınca açılır).
        var daireler = new List<DaireEksenleri>();
        foreach (DaireEksenleri d in KatDaireleri())
            if (d.gameObject.activeInHierarchy) daireler.Add(d);
        Transform sahanlik = daireler.Count > 0 ? daireler[0].transform.parent : null;
        foreach (DaireEksenleri d in daireler)
        {
            Transform mentese = sahanlik != null ? sahanlik.Find("Kapi_" + d.name.Replace("Daire_", "") + "/Mentese") : null;
            if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, 80f, 0f);
        }
        Physics.SyncTransforms();

        // Oyuncu açık dairenin sahanlığında değilse (1. katta oyun zemin kattan başlar) dairenin kapısının önüne,
        // sahanlığın ortasına doğru 0,9 m içeri taşınır.
        if (sahanlik != null && daireler.Count > 0 && Mathf.Abs(oyuncu.transform.position.y - sahanlik.position.y) > 0.5f)
        {
            Transform kapi = sahanlik.Find("Kapi_" + daireler[0].name.Replace("Daire_", "") + "/Mentese");
            if (kapi != null)
            {
                Vector3 orta = new Vector3(sahanlik.position.x, kapi.position.y, sahanlik.position.z);
                Vector3 yer = kapi.position + (orta - kapi.position).normalized * 0.9f;
                yer.y = sahanlik.position.y + 0.05f;
                CharacterController cc = oyuncu.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                oyuncu.transform.position = yer;
                if (cc != null) cc.enabled = true;
                Physics.SyncTransforms();
            }
        }

        // 1. YÜRÜME: oyuncunun durduğu yerden (sahanlık), sahanlığın katında.
        var hucreler = new Dictionary<Vector2Int, Hucre>();
        var engeller = new Dictionary<string, int>();
        Dolas(oyuncu.transform.position, sahanlik, oyuncu.transform, hucreler, engeller);

        KilitZinciri zincir = yonetici.KatinZinciri(kat);
        ZincirUygulayici uygulayici = (ZincirUygulayici)typeof(KatYoneticisi).GetField("zincirUygulayici", Gizli).GetValue(yonetici);
        foreach (DaireEksenleri d in daireler)
        {
            string onek = uygulayici != null && uygulayici.IkinciDaire == d.transform ? "B:" : "";
            foreach (DairePlani.Oda o in DairePlani.Odalar)
            {
                int ayakta = 0, egik = 0;
                foreach (var h in hucreler)
                {
                    Vector3 p = new Vector3(h.Key.x * Adim, h.Value.Y, h.Key.y * Adim);
                    d.PlanNoktasi(p, out float u, out float v);
                    if (u < o.U0 || u > o.U1 || v < o.V0 || v > o.V1) continue;
                    if (h.Value.Ayakta) ayakta++; else egik++;
                }
                bool kapali = zincir != null && zincir.KapaliOdalar.Contains(onek + o.Ad);
                float alan = (ayakta + egik) * Adim * Adim;
                if (!kapali && ayakta == 0) rapor.Append($"ULAŞILMIYOR:{onek}{o.Ad} ");
                else if (!kapali) rapor.Append($"{onek}{o.Ad}={alan:0.0}m² ");
            }
        }
        // Yolu tıkayan eşyalar: yürünebilir alanın kenarında kapsülün çarptığı çarpışmalar (en çok çarpılanlar).
        var liste = new List<KeyValuePair<string, int>>(engeller);
        liste.Sort((x, y) => y.Value.CompareTo(x.Value));
        rapor.Append("| engel: ");
        for (int i = 0; i < liste.Count && i < 8; i++) rapor.Append($"{liste[i].Key}×{liste[i].Value} ");
        rapor.Append("| ");

        // 2. NİŞAN: kaplar (kilitliler de) açılır; hedefler zincirin parçaları.
        foreach (AcilirParca a in Object.FindObjectsByType<AcilirParca>())
            if (a.gameObject.activeInHierarchy) { a.Kilitle(false); a.Ayarla(true); }
        Physics.SyncTransforms();
        var hedefler = new List<MonoBehaviour>();
        foreach (MonoBehaviour m in Object.FindObjectsByType<MonoBehaviour>())
        {
            if (!m.isActiveAndEnabled) continue;
            string tur = m.GetType().Name;
            if (m is Esya es && string.IsNullOrEmpty(es.Kimlik)) continue;
            // Yuva (sofradaki tabak yeri) ancak elde tabak varken seçilir; burada denetlenmez.
            if (tur == "Esya" || tur == "CekilenKol" || tur == "SigortaYuvasi" || tur == "Kirilan" || tur == "MenteseSokucu"
                || (tur == "Kilit" && m.GetComponent<IcKapi>() == null))
                hedefler.Add(m);
        }
        Transform kamera = (Transform)typeof(Etkilesimci).GetField("kamera", Gizli).GetValue(etk);
        MethodInfo hedefBul = typeof(Etkilesimci).GetMethod("HedefBul", Gizli);
        Vector3 eskiKonum = kamera.position; Quaternion eskiDonus = kamera.rotation;
        int kotu = 0;
        var enIyiler = new List<(Esya esya, Vector3 goz, Vector3 hedef)>();
        var kotuHedefler = new Dictionary<string, Vector3>();
        foreach (MonoBehaviour m in hedefler)
        {
            Collider c = m.GetComponentInChildren<Collider>();
            Vector3 hedef = c != null ? c.bounds.center : m.transform.position;
            int goren = 0; float enIyi = 99f; Vector3 enIyiGoz = Vector3.zero; bool enIyiAyakta = false;
            foreach (var h in hucreler)
            {
                // Nişan için 20 cm'de bir nokta yeter (her hücre on binlerce nişan demek, CLI süresini aşar).
                if (h.Key.x % 2 != 0 || h.Key.y % 2 != 0) continue;
                Vector3 zemin = new Vector3(h.Key.x * Adim, h.Value.Y, h.Key.y * Adim);
                Vector2 yatay = new Vector2(zemin.x - hedef.x, zemin.z - hedef.z);
                if (yatay.sqrMagnitude > Menzil * Menzil) continue;
                foreach (bool ayakta in h.Value.Ayakta ? new[] { true, false } : new[] { false })
                {
                    Vector3 goz = zemin + Vector3.up * (ayakta ? AyaktaGoz : EgikGoz);
                    float mesafe = (hedef - goz).magnitude;
                    if (mesafe > Menzil) continue;
                    kamera.position = goz;
                    kamera.rotation = Quaternion.LookRotation(hedef - goz);
                    var sonuc = hedefBul.Invoke(etk, new object[] { null }) as MonoBehaviour;
                    if (sonuc == null || !(sonuc.gameObject == m.gameObject || m.transform.IsChildOf(sonuc.transform) || sonuc.transform.IsChildOf(m.transform))) continue;
                    goren++;
                    // Ayakta görülen nokta eğilerek görülenden açıkça önce gelir (oyuncu önce ayakta bakar; masanın
                    // altından yakın bir eğilme noktası "en iyi" sayılınca karanlık fotoğraf anlamsızlaşıyordu).
                    float puan = mesafe + (ayakta ? 0f : 2.0f);
                    if (puan < enIyi) { enIyi = puan; enIyiGoz = goz; enIyiAyakta = ayakta; }
                }
            }
            string ad = m is Esya e2 ? e2.Kimlik : m.GetType().Name + "@" + m.name;
            // Kırılacak kabın (kavanoz) içindeki eşya kap kırılmadan seçilemez; bu denetim kabı kırmaz.
            bool kirilacakta = m is Esya && m.GetComponentInParent<Kirilan>() != null;
            if (goren == 0 && kirilacakta) rapor.Append($"{ad}=(kırılınca) ");
            else if (goren == 0) { kotu++; rapor.Append($"NİŞANLANAMIYOR:{ad} "); kotuHedefler[ad] = hedef; }
            else
            {
                rapor.Append($"{ad}={goren}{(enIyiAyakta ? "a" : "e")} ");
                if (m is Esya e3) enIyiler.Add((e3, enIyiGoz, hedef));
            }
        }
        rapor.Append($"| {hedefler.Count} hedef, {kotu} nişanlanamıyor | ");

        // 3. HARİTA: her dairenin planı tepeden (u sağa, v yukarı), 10 cm = 4 piksel. Yeşil: ayakta yürünür,
        // sarı: yalnızca eğilerek, kırmızı: bir eşya kaplıyor, gri: boş ama ulaşılmıyor, siyah: zemin yok (duvar).
        // Mavi nokta: seçilebilen hedef; mor büyük nokta: seçilemeyen hedef.
        foreach (DaireEksenleri d in daireler)
        {
            int gen = 110, yuk = 92, olcek = 4;
            var harita = new Texture2D(gen * olcek, yuk * olcek, TextureFormat.RGB24, false);
            for (int i = 0; i < gen; i++)
                for (int j = 0; j < yuk; j++)
                {
                    float u = -0.3f + i * Adim, v = -1.0f + j * Adim;
                    Vector3 w = d.transform.TransformPoint(d.Nokta(u, 0f, v));
                    Vector2Int anahtar = new Vector2Int(Mathf.RoundToInt(w.x / Adim), Mathf.RoundToInt(w.z / Adim));
                    Color renk;
                    if (hucreler.TryGetValue(anahtar, out Hucre hc)) renk = hc.Ayakta ? new Color(0.2f, 0.7f, 0.25f) : new Color(0.85f, 0.8f, 0.2f);
                    else if (!ZemineBak(w + Vector3.up * 0.8f, out float zy)) renk = new Color(0.05f, 0.05f, 0.05f);
                    else renk = Durabilir(new Vector3(w.x, zy, w.z), oyuncu.transform, out bool _, out string _) ? new Color(0.45f, 0.45f, 0.45f) : new Color(0.75f, 0.15f, 0.12f);
                    for (int a = 0; a < olcek; a++) for (int b = 0; b < olcek; b++) harita.SetPixel(i * olcek + a, j * olcek + b, renk);
                }
            foreach (var (esya, goz, hedef) in enIyiler) Nokta(harita, d, hedef, new Color(0.2f, 0.4f, 1f), 3);
            foreach (var kh in kotuHedefler) Nokta(harita, d, kh.Value, new Color(0.8f, 0.1f, 0.9f), 5);
            harita.Apply();
            System.IO.File.WriteAllBytes($"{klasor}/kat{kat}_harita_{d.name}.png", ImageConversion.EncodeToPNG(harita));
        }

        // 4. KARANLIK: oyuncunun kamerası, kafa lambası açık, gerçek ışık ve son işlemeyle.
        Camera cam = kamera.GetComponent<Camera>();
        KafaLambasi lamba = Object.FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Tak();
        var rt = new RenderTexture(800, 500, 24);
        var doku = new Texture2D(800, 500, TextureFormat.RGB24, false);
        RenderTexture eskiHedef = cam.targetTexture;
        cam.targetTexture = rt;
        foreach (var (esya, goz, hedef) in enIyiler)
        {
            kamera.position = goz;
            kamera.rotation = Quaternion.LookRotation(hedef - goz);
            // Eşyaların kafa lambası parıltısı (#83) normalde bir sonraki karede güncellenir; fotoğraf bu
            // karede çekildiği için hemen güncellenir, yoksa bir önceki bakışın parıltısı görünürdü.
            foreach (FenerParlamasi fp in Object.FindObjectsByType<FenerParlamasi>()) fp.Guncelle(true);
            cam.Render();
            RenderTexture.active = rt;
            doku.ReadPixels(new Rect(0, 0, 800, 500), 0, 0); doku.Apply();
            RenderTexture.active = null;
            // Eşyanın ekrandaki yeri (ekranın ortası, kamera ona bakıyor) çevresinde 40x40 piksellik ortalama parlaklık.
            float toplam = 0f; int n = 0;
            for (int x = 380; x < 420; x += 2)
                for (int y = 230; y < 270; y += 2) { Color p = doku.GetPixel(x, y); toplam += 0.2126f * p.r + 0.7152f * p.g + 0.0722f * p.b; n++; }
            rapor.Append($"{esya.Kimlik}:{toplam / n:0.00} ");
            System.IO.File.WriteAllBytes($"{klasor}/kat{kat}_{esya.Kimlik}.jpg", ImageConversion.EncodeToJPG(doku, 80));
        }
        cam.targetTexture = eskiHedef;
        Object.Destroy(rt);
        kamera.position = eskiKonum; kamera.rotation = eskiDonus;
        return rapor.ToString();
    }

    // ---------- BULUNABİLİRLİK (#102) ----------
    // Şahan: "Nesneleri hep benzer yerlerde mi buluyoruz? Çok kolay bulunuyor." Kolaylığı tahminle değil
    // ölçerek konuşmak için: her görev eşyası, oyuncunun yürüyebildiği her noktadan (20 cm'de bir, ayakta ve
    // eğilmiş, 5 m içinde) GÖZLE görülüyor mu diye iki kez denenir. Görmek: gözden eşyanın beş noktasına
    // giden ışınlardan biri, eşyanın kendisinden başka bir şeye çarpmadan ona ulaşıyor.
    // (Nişan değil: oyunun nişanı, bir mobilyanın kutusunun içinde kalan eşyayı "kabında" sayıp mobilyanın
    // içinden seçer; yatağın altındaki anahtar ayakta seçilebilir ama görülemez. Bulmayı görmek belirler.)
    //   ÖNCE: kat ilk açıldığı gibi (çekmeceler, kapaklar kapalı). Ayakta bir noktadan görülüyorsa eşya
    //         AÇIKTA durur: odaya giren onu ilk bakışta görür.
    //   SONRA: bütün kaplar açık. Önce görünmeyip şimdi görünen eşya bir KABIN içindedir.
    // Ayrıca kaplar açıkken eşyanın oyunun nişanıyla kaç noktadan SEÇİLEBİLDİĞİ yazılır (s:N): 0 ise eşya
    // görülse de alınamıyordur, bu bir hatadır.
    // Sınıflar (kolaydan zora):
    //   acik     : kaplar kapalıyken ayakta görülüyor.
    //   egil     : kaplar kapalıyken yalnızca eğilerek görülüyor (mobilyanın altı).
    //   kap      : bir çekmeceyi/kapağı açınca ayakta görülüyor.
    //   kap+egil : kabı açınca bile yalnızca eğilerek görülüyor.
    //   kirilinca: kırılacak kabın (kavanoz) içinde; bu ölçüm kabı kırmaz.
    //   gizli    : hiçbir noktadan görülmüyor (asansör kabini gibi sonradan açılan yer ya da HATA).
    // Rakamlar: kaç noktadan görüldüğü (ayakta/eğilmiş). Az nokta = dar bir açıdan, aranarak bulunur.
    public static string Bulunabilirlik(int kat)
    {
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>();
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>();
        if (oyuncu == null || etk == null || yonetici == null) return "oyuncu ya da kat yöneticisi yok (oyun modunda mı?)";
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        Varlik.Kapali = true;   // ölçüm oyuncuyu ışınlar; Varlık (#86) işlemesin

        // Oda kapıları ve dairelerin giriş kapıları açılır (Denetle'deki gibi): ölçülen, eşyanın odasına
        // girmiş bir oyuncunun onu ne kadar kolay gördüğüdür, kapının kilidi değil. Kaplara dokunulmaz.
        foreach (IcKapi k in KatKapilari())
            if (k.gameObject.activeInHierarchy && !k.name.Contains("Balkon")) { k.Kilitle(false); k.Ayarla(true); }
        var daireler = new List<DaireEksenleri>();
        foreach (DaireEksenleri d in KatDaireleri())
            if (d.gameObject.activeInHierarchy) daireler.Add(d);
        Transform sahanlik = daireler.Count > 0 ? daireler[0].transform.parent : null;
        foreach (DaireEksenleri d in daireler)
        {
            Transform mentese = sahanlik != null ? sahanlik.Find("Kapi_" + d.name.Replace("Daire_", "") + "/Mentese") : null;
            if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, 80f, 0f);
        }
        Physics.SyncTransforms();
        // 1. katta oyun zemin kattaki açılıştan başlar: oyuncu açık dairenin sahanlığına taşınır (Denetle'deki gibi).
        SahanligaTasi(oyuncu, sahanlik, daireler);

        // Yürünebilir alan: oyuncunun durduğu yerden (açık dairenin sahanlığı).
        var hucreler = new Dictionary<Vector2Int, Hucre>();
        Dolas(oyuncu.transform.position, sahanlik, oyuncu.transform, hucreler, new Dictionary<string, int>());

        // Ölçülecekler: kimliği olan, sahnede etkin görev eşyaları.
        var esyalar = new List<Esya>();
        foreach (Esya e in Object.FindObjectsByType<Esya>())
            if (e.isActiveAndEnabled && !string.IsNullOrEmpty(e.Kimlik) && e.GetComponentInParent<DokuzuncuKat>() == null)   // dokuzuncu katın anahtarı ayrı set (#108)
                esyalar.Add(e);
        esyalar.Sort((a, b) => string.CompareOrdinal(a.Kimlik, b.Kimlik));

        Transform kamera = (Transform)typeof(Etkilesimci).GetField("kamera", Gizli).GetValue(etk);
        MethodInfo hedefBul = typeof(Etkilesimci).GetMethod("HedefBul", Gizli);
        Vector3 eskiKonum = kamera.position; Quaternion eskiDonus = kamera.rotation;

        // Görüş kabukları (aşağıdaki GorusKabuguKur): ışınlar modellerin gerçek şekline çarpsın.
        List<GameObject> kabuklar = GorusKabuguKur(sahanlik);

        // ÖNCE: kaplar kapalı (katın açıldığı hâl).
        var once = new Dictionary<Esya, Vector2Int>();
        foreach (Esya e in esyalar) once[e] = Gorenler(e, hucreler, oyuncu.transform);
        // SONRA: bütün kaplar açık (kilitliler de).
        foreach (AcilirParca a in Object.FindObjectsByType<AcilirParca>())
            if (a.gameObject.activeInHierarchy) { a.Kilitle(false); a.Ayarla(true); }
        Physics.SyncTransforms();   // açılan çekmecelerin kabukları da (onların çocukları) yerine gelir

        var rapor = new StringBuilder();
        var sayac = new Dictionary<string, int>();
        foreach (Esya e in esyalar)
        {
            Vector2Int o = once[e];
            Vector2Int s = Gorenler(e, hucreler, oyuncu.transform);
            int secen = Secenler(e, hucreler, kamera, hedefBul, etk);
            string sinif;
            if (o.x > 0) sinif = "acik";
            else if (o.y > 0) sinif = "egil";
            else if (s.x > 0) sinif = "kap";
            else if (s.y > 0) sinif = "kap+egil";
            else if (e.GetComponentInParent<Kirilan>() != null) sinif = "kirilinca";
            else sinif = "gizli";
            sayac.TryGetValue(sinif, out int n); sayac[sinif] = n + 1;
            rapor.Append($"{e.Kimlik}={sinif}({o.x}/{o.y}|{s.x}/{s.y})s:{secen} ");
        }
        kamera.position = eskiKonum; kamera.rotation = eskiDonus;
        GorusKabuguSil(kabuklar);

        var ozet = new StringBuilder();
        foreach (string sinif in new[] { "acik", "egil", "kap", "kap+egil", "kirilinca", "gizli" })
            if (sayac.TryGetValue(sinif, out int n)) ozet.Append($"{sinif}={n} ");
        return $"KAT {kat}: {ozet}| {rapor}";
    }

    // ---------- YER DENEMESİ (#102) ----------
    // Bir eşyayı açık dairede plan noktasına (u, y, v) GEÇİCİ olarak taşır ve orada bulunabilirliğini ölçer:
    // "acik/egil/kap..." ile ayakta/eğilmiş görülme sayıları. Kat dosyasını değiştirip bütün katları yeniden
    // kurmadan, aday saklama yerlerini oyun modunda hızlıca denemek için. ikinci = true: ikinci dairenin
    // (6. katta B) ekseniyle. Yürüme taraması kat başına bir kez yapılır ve saklanır (her denemede tekrar
    // etmek dakikalar sürerdi); kat değişince ya da 'yenile' verilince baştan yapılır.
    static Dictionary<Vector2Int, Hucre> denemeHucreleri;
    static int denemeKati = -1;

    public static string YerDene(int kat, string kimlik, float u, float y, float v, bool ikinci = false, bool yenile = false)
    {
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>();
        if (oyuncu == null || etk == null) return "oyun modunda değil";
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        Varlik.Kapali = true;

        Esya esya = null;
        foreach (Esya e in Object.FindObjectsByType<Esya>()) if (e.isActiveAndEnabled && e.Kimlik == kimlik) { esya = e; break; }
        if (esya == null) return "eşya yok: " + kimlik;

        // Daireler: kat_gir sırası (Daire_N adına göre) ilk = asıl daire, ikinci = B.
        var daireler = new List<DaireEksenleri>();
        foreach (DaireEksenleri d in KatDaireleri()) if (d.gameObject.activeInHierarchy) daireler.Add(d);
        if (daireler.Count == 0) return "açık daire yok";
        Transform sahanlik = daireler[0].transform.parent;
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>();
        ZincirUygulayici uyg = (ZincirUygulayici)typeof(KatYoneticisi).GetField("zincirUygulayici", Gizli).GetValue(yonetici);
        DaireEksenleri hedefDaire = null;
        foreach (DaireEksenleri d in daireler)
            if ((uyg != null && uyg.IkinciDaire == d.transform) == ikinci) { hedefDaire = d; break; }
        if (hedefDaire == null) return "daire bulunamadı";

        if (denemeHucreleri == null || denemeKati != kat || yenile)
        {
            foreach (IcKapi k in KatKapilari())
                if (k.gameObject.activeInHierarchy && !k.name.Contains("Balkon")) { k.Kilitle(false); k.Ayarla(true); }
            foreach (DaireEksenleri d in daireler)
            {
                Transform mentese = sahanlik.Find("Kapi_" + d.name.Replace("Daire_", "") + "/Mentese");
                if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, 80f, 0f);
            }
            Physics.SyncTransforms();
            SahanligaTasi(oyuncu, sahanlik, daireler);
            denemeHucreleri = new Dictionary<Vector2Int, Hucre>();
            Dolas(oyuncu.transform.position, sahanlik, oyuncu.transform, denemeHucreleri, new Dictionary<string, int>());
            denemeKati = kat;
        }

        // Eşya kabından çıkarılıp dairenin köküne alınır, kap bağı kopar (kapalı çekmece artık onu tutmaz).
        esya.transform.SetParent(hedefDaire.transform, true);
        esya.KapAyarla(null);
        esya.transform.position = hedefDaire.transform.TransformPoint(hedefDaire.Nokta(u, y, v));
        Physics.SyncTransforms();

        Transform kamera = (Transform)typeof(Etkilesimci).GetField("kamera", Gizli).GetValue(etk);
        MethodInfo hedefBul = typeof(Etkilesimci).GetMethod("HedefBul", Gizli);
        Vector3 eskiKonum = kamera.position; Quaternion eskiDonus = kamera.rotation;
        List<GameObject> kabuklar = GorusKabuguKur(sahanlik);
        Vector2Int g = Gorenler(esya, denemeHucreleri, oyuncu.transform);
        GorusKabuguSil(kabuklar);
        int secen = Secenler(esya, denemeHucreleri, kamera, hedefBul, etk);
        kamera.position = eskiKonum; kamera.rotation = eskiDonus;
        string sinif = g.x > 0 ? "acik" : g.y > 0 ? "egil" : "gizli";
        return $"{kimlik} @({u:0.00},{y:0.000},{v:0.00}) = {sinif} ({g.x}/{g.y}) s:{secen}";
    }

    // ---------- GÖRÜŞ FOTOĞRAFI (#102) ----------
    // Bir eşyayı, gözle GÖRÜLDÜĞÜ noktaların en yakınından (ayakta görülüyorsa ayakta, yoksa eğilmiş göz
    // yüksekliğinden) oyunun gerçek karanlığında kafa lambasıyla çeker. Denetle'nin fotoğrafı nişanın en iyi
    // noktasından çekilir; nişan mobilyanın içinden geçebildiği için (IcindeMi) o kare eşyayı göstermeyebilir.
    // Mobilyanın altına saklanan eşyanın gerçekten "eğilince görünür" olduğunu görüntüyle doğrulamak için.
    public static string GorusFotografi(int kat, string klasor, string kimlikler)
    {
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>();
        if (oyuncu == null || etk == null) return "oyun modunda değil";
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        Varlik.Kapali = true;
        // Kapılar ve sahanlık (Bulunabilirlik'teki gibi); kaplara dokunulmaz.
        foreach (IcKapi k in KatKapilari())
            if (k.gameObject.activeInHierarchy && !k.name.Contains("Balkon")) { k.Kilitle(false); k.Ayarla(true); }
        var daireler = new List<DaireEksenleri>();
        foreach (DaireEksenleri d in KatDaireleri()) if (d.gameObject.activeInHierarchy) daireler.Add(d);
        if (daireler.Count == 0) return "açık daire yok";
        Transform sahanlik = daireler[0].transform.parent;
        foreach (DaireEksenleri d in daireler)
        {
            Transform mentese = sahanlik.Find("Kapi_" + d.name.Replace("Daire_", "") + "/Mentese");
            if (mentese != null) mentese.localRotation = Quaternion.Euler(0f, 80f, 0f);
        }
        Physics.SyncTransforms();
        SahanligaTasi(oyuncu, sahanlik, daireler);
        var hucreler = new Dictionary<Vector2Int, Hucre>();
        Dolas(oyuncu.transform.position, sahanlik, oyuncu.transform, hucreler, new Dictionary<string, int>());

        Transform kamera = (Transform)typeof(Etkilesimci).GetField("kamera", Gizli).GetValue(etk);
        Camera cam = kamera.GetComponent<Camera>();
        KafaLambasi lamba = Object.FindAnyObjectByType<KafaLambasi>();
        if (lamba != null) lamba.Tak();
        Vector3 eskiKonum = kamera.position; Quaternion eskiDonus = kamera.rotation;
        var rt = new RenderTexture(800, 500, 24);
        var doku = new Texture2D(800, 500, TextureFormat.RGB24, false);
        RenderTexture eskiHedef = cam.targetTexture;
        var rapor = new StringBuilder();
        List<GameObject> kabuklar = GorusKabuguKur(sahanlik);
        foreach (string kimlik in kimlikler.Split(','))
        {
            Esya esya = null;
            foreach (Esya e in Object.FindObjectsByType<Esya>()) if (e.isActiveAndEnabled && e.Kimlik == kimlik) { esya = e; break; }
            if (esya == null) { rapor.Append($"{kimlik}=YOK "); continue; }
            Bounds b = new Bounds(esya.transform.position, Vector3.zero);
            bool ilk = true;
            foreach (Renderer r in esya.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                if (ilk) { b = r.bounds; ilk = false; } else b.Encapsulate(r.bounds);
            }
            // En iyi nokta: önce ayakta görülen en yakın nokta; yoksa eğilmiş görülen en yakın nokta.
            Vector3 enIyi = Vector3.zero, bakis = b.center; float enYakin = 99f; bool ayaktaBulundu = false;
            Vector3[] noktalar = BesNokta(b);
            foreach (bool ayaktaTur in new[] { true, false })
            {
                foreach (var h in hucreler)
                {
                    if (h.Key.x % 2 != 0 || h.Key.y % 2 != 0) continue;
                    if (ayaktaTur && !h.Value.Ayakta) continue;
                    Vector3 goz = new Vector3(h.Key.x * Adim, h.Value.Y, h.Key.y * Adim) + Vector3.up * (ayaktaTur ? AyaktaGoz : EgikGoz);
                    float m = (b.center - goz).magnitude;
                    if (m < 0.5f || m >= enYakin) continue;
                    foreach (Vector3 n in noktalar)
                        if (GorusAcik(goz, n, esya.transform, oyuncu.transform)) { enYakin = m; enIyi = goz; bakis = n; ayaktaBulundu = ayaktaTur; break; }
                }
                if (enYakin < 99f) break;
            }
            if (enYakin >= 99f) { rapor.Append($"{kimlik}=GÖRÜLMÜYOR "); continue; }
            GorusKabuguSil(kabuklar); kabuklar.Clear();   // fotoğrafta kabuk çarpışmaları önemsiz ama temiz olsun
            kamera.position = enIyi;
            kamera.rotation = Quaternion.LookRotation(bakis - enIyi);
            // Kafa lambası kameranın arkasından gecikmeli döner (#92, KameraHareketi): kamera bir anda taşınınca
            // lamba hâlâ eski yöne bakar ve eşya fenerin ışığında olmaz. Çekimden önce lamba kameraya hizalanır.
            if (lamba != null) lamba.IsikYonu.rotation = kamera.rotation;
            foreach (FenerParlamasi fp in Object.FindObjectsByType<FenerParlamasi>()) fp.Guncelle(true);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            doku.ReadPixels(new Rect(0, 0, 800, 500), 0, 0); doku.Apply();
            RenderTexture.active = null;
            cam.targetTexture = eskiHedef;
            System.IO.File.WriteAllBytes($"{klasor}/gorus_kat{kat}_{kimlik}.jpg", ImageConversion.EncodeToJPG(doku, 85));
            rapor.Append($"{kimlik}={(ayaktaBulundu ? "ayakta" : "egik")}:{enYakin:0.0}m ");
            kabuklar = GorusKabuguKur(sahanlik);
        }
        GorusKabuguSil(kabuklar);
        Object.Destroy(rt);
        kamera.position = eskiKonum; kamera.rotation = eskiDonus;
        return rapor.ToString();
    }

    // ---------- SAKLAMA YERİ ARAMASI (#102) ----------
    // Bir odanın zeminini 20 cm'lik ızgarayla tarar, eşyayı her noktaya geçici olarak koyup ölçer ve oyunun
    // nişanıyla seçilebilen iki tür yeri yazar:
    //   E (eğilme): ayakta hiç görülmüyor, eğilince görülüyor; eğilerek görüldüğü nokta sayısı çoktan aza.
    //   D (dar açı): ayakta yalnızca 1-20 noktadan görülüyor (koltuğun arkası, dolapla duvarın arası);
    //      odaya giren onu ilk bakışta görmez, ancak dolaşıp doğru yerden bakınca görür. Azdan çoğa.
    // Kat dosyasına hangi yeri yazacağımızı tahminle değil bu listeyle seçeriz. y: döşemeden yükseklik (m).
    public static string SaklamaYeriAra(int kat, string kimlik, string oda, float y = 0.004f, bool ikinci = false)
    {
        string hazirlik = YerDene(kat, kimlik, 0f, -5f, 0f, ikinci);   // yürüme taraması + kapılar (eşya geçici olarak yerin altına)
        if (hazirlik.StartsWith("eşya yok") || hazirlik.StartsWith("oyun") || hazirlik.StartsWith("açık") || hazirlik.StartsWith("daire")) return hazirlik;
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        BirinciSahisKontrolcu oyuncu = Object.FindAnyObjectByType<BirinciSahisKontrolcu>();
        Etkilesimci etk = Object.FindAnyObjectByType<Etkilesimci>();
        Esya esya = null;
        foreach (Esya e in Object.FindObjectsByType<Esya>()) if (e.isActiveAndEnabled && e.Kimlik == kimlik) { esya = e; break; }
        DaireEksenleri daire = esya.GetComponentInParent<DaireEksenleri>();   // YerDene eşyayı dairenin köküne aldı
        DairePlani.Oda? bulunan = null;
        foreach (DairePlani.Oda o in DairePlani.Odalar) if (o.Ad == oda) bulunan = o;
        if (bulunan == null) return "oda yok: " + oda;
        DairePlani.Oda r = bulunan.Value;

        List<GameObject> kabuklar = GorusKabuguKur(daire.transform.parent);
        var egilme = new List<(float u, float v, Vector2Int g)>();
        var dar = new List<(float u, float v, Vector2Int g)>();
        for (float u = r.U0 + 0.12f; u <= r.U1 - 0.12f; u += 0.2f)
            for (float v = r.V0 + 0.12f; v <= r.V1 - 0.12f; v += 0.2f)
            {
                esya.transform.position = daire.transform.TransformPoint(daire.Nokta(u, y, v));
                Physics.SyncTransforms();
                Vector2Int g = Gorenler(esya, denemeHucreleri, oyuncu.transform);
                if (g.x == 0 && g.y > 0) egilme.Add((u, v, g));
                else if (g.x >= 1 && g.x <= 20) dar.Add((u, v, g));
            }
        GorusKabuguSil(kabuklar);

        egilme.Sort((a1, a2) => a2.g.y.CompareTo(a1.g.y));
        dar.Sort((a1, a2) => a1.g.x.CompareTo(a2.g.x));
        Transform kamera = (Transform)typeof(Etkilesimci).GetField("kamera", Gizli).GetValue(etk);
        MethodInfo hedefBul = typeof(Etkilesimci).GetMethod("HedefBul", Gizli);
        Vector3 eskiKonum = kamera.position; Quaternion eskiDonus = kamera.rotation;
        var rapor = new StringBuilder($"{kimlik} {oda}: E{egilme.Count} D{dar.Count} | E: ");
        foreach (var liste in new[] { egilme, dar })
        {
            int yazilan = 0;
            foreach (var a3 in liste)
            {
                esya.transform.position = daire.transform.TransformPoint(daire.Nokta(a3.u, y, a3.v));
                Physics.SyncTransforms();
                int secen = Secenler(esya, denemeHucreleri, kamera, hedefBul, etk);
                if (secen == 0) continue;   // görülüyor ama alınamıyor: işe yaramaz
                rapor.Append($"({a3.u:0.00},{a3.v:0.00}) {a3.g.x}/{a3.g.y} s{secen}; ");
                if (++yazilan >= 6) break;
            }
            if (liste == egilme) rapor.Append("| D: ");
        }
        kamera.position = eskiKonum; kamera.rotation = eskiDonus;
        return rapor.ToString();
    }

    // Oyuncu açık dairenin sahanlığında değilse dairenin kapısının önüne, sahanlığın ortasına doğru 0,9 m
    // içeri taşınır (Denetle'deki adımın aynısı).
    static void SahanligaTasi(BirinciSahisKontrolcu oyuncu, Transform sahanlik, List<DaireEksenleri> daireler)
    {
        if (sahanlik == null || daireler.Count == 0 || Mathf.Abs(oyuncu.transform.position.y - sahanlik.position.y) <= 0.5f) return;
        Transform kapi = sahanlik.Find("Kapi_" + daireler[0].name.Replace("Daire_", "") + "/Mentese");
        if (kapi == null) return;
        Vector3 orta = new Vector3(sahanlik.position.x, kapi.position.y, sahanlik.position.z);
        Vector3 yer = kapi.position + (orta - kapi.position).normalized * 0.9f;
        yer.y = sahanlik.position.y + 0.05f;
        CharacterController cc = oyuncu.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        oyuncu.transform.position = yer;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    const float GormeMenzili = 5f;     // kafa lambasının parıltısının menzili (ParlamaKurali.Menzil)

    // Bir eşyanın yürünebilir noktalardan kaç kez GÖZLE görüldüğü: x = ayakta, y = eğilmiş (20 cm'de bir, 5 m içinde).
    static Vector2Int Gorenler(Esya m, Dictionary<Vector2Int, Hucre> hucreler, Transform oyuncu)
    {
        // Eşyanın görünen çizicilerinin kutusu (parıltı dörtgeni kapalıdır, sayılmaz).
        Bounds b = new Bounds(m.transform.position, Vector3.zero);
        bool ilk = true;
        foreach (Renderer r in m.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            if (ilk) { b = r.bounds; ilk = false; } else b.Encapsulate(r.bounds);
        }
        Vector3[] noktalar = BesNokta(b);
        Vector3 c = b.center;
        int ayaktaSay = 0, egikSay = 0;
        foreach (var h in hucreler)
        {
            if (h.Key.x % 2 != 0 || h.Key.y % 2 != 0) continue;
            Vector3 zemin = new Vector3(h.Key.x * Adim, h.Value.Y, h.Key.y * Adim);
            Vector2 yatay = new Vector2(zemin.x - c.x, zemin.z - c.z);
            if (yatay.sqrMagnitude > GormeMenzili * GormeMenzili) continue;
            foreach (bool ayakta in h.Value.Ayakta ? new[] { true, false } : new[] { false })
            {
                Vector3 goz = zemin + Vector3.up * (ayakta ? AyaktaGoz : EgikGoz);
                foreach (Vector3 n in noktalar)
                    if (GorusAcik(goz, n, m.transform, oyuncu)) { if (ayakta) ayaktaSay++; else egikSay++; break; }
            }
        }
        return new Vector2Int(ayaktaSay, egikSay);
    }

    // Beş nokta: kutunun ortası ve üst yüzünün dört köşesine yakın noktalar (kenarın %70'i). Eşyanın bir köşesi
    // görünüyorsa oyuncu onu görmüş sayılır (bir anahtarın ucu kanepenin altından görünür).
    static Vector3[] BesNokta(Bounds b)
    {
        Vector3 c = b.center, e = b.extents;
        float ust = b.max.y - 0.002f;
        return new[]
        {
            c,
            new Vector3(c.x + e.x * 0.7f, ust, c.z + e.z * 0.7f), new Vector3(c.x - e.x * 0.7f, ust, c.z + e.z * 0.7f),
            new Vector3(c.x + e.x * 0.7f, ust, c.z - e.z * 0.7f), new Vector3(c.x - e.x * 0.7f, ust, c.z - e.z * 0.7f),
        };
    }

    // ---------- GÖRÜŞ KABUKLARI ----------
    // Mobilyaların çoğunun çarpışması bütün hacmini saran bir kutudur: yürümek için doğru, "göz ne görür" için
    // yanlış (masanın üstündeki anahtar kutunun içinde kalır, ışın masaya gelmeden kutuya çarpar; ilk ölçümde
    // masadaki anahtar "gizli" çıktı). Ölçüm süresince sahanlıktaki ve dairelerdeki her görünen modelin altına,
    // modelin KENDİ şekliyle (MeshCollider, sharedMesh) bir kabuk eklenir; kabuklar boş 31. katmandadır ve
    // görüş ışınları yalnızca o katmana atılır. Kabuklar aynı eval içinde silinir: araya bir kare girmez,
    // oyunun fiziği (oyuncunun yürümesi) onları hiç görmez.
    const int GorusKatmani = 31;

    static List<GameObject> GorusKabuguKur(Transform kok)
    {
        var liste = new List<GameObject>();
        if (kok == null) return liste;
        foreach (MeshRenderer r in kok.GetComponentsInChildren<MeshRenderer>())
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            var go = new GameObject("GorusKabugu") { layer = GorusKatmani };
            go.transform.SetParent(r.transform, false);   // modelin yerinde, dönüşünde ve ölçeğinde
            go.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
            liste.Add(go);
        }
        Physics.SyncTransforms();
        return liste;
    }

    static void GorusKabuguSil(List<GameObject> liste)
    {
        // DestroyImmediate: Destroy karenin sonunu beklerdi; ölçüm aynı eval'de sürdüğü için hemen silinir.
        foreach (GameObject go in liste) if (go != null) Object.DestroyImmediate(go);
        Physics.SyncTransforms();
    }

    // Gözden noktaya giden ışın, eşyanın ve oyuncunun kendisi dışında bir modelin kabuğuna çarpıyor mu?
    static bool GorusAcik(Vector3 goz, Vector3 nokta, Transform esya, Transform oyuncu)
    {
        Vector3 fark = nokta - goz;
        float mesafe = fark.magnitude;
        if (mesafe > GormeMenzili) return false;
        // Işın hedefe 3 cm kala durur: eşyanın üstünde yattığı yüzey (masa, çekmece tabanı, döşeme) o son
        // santimlerde çarpılır; o taşıyıcıdır, saklayan değil (ilk ölçümde masanın üstündeki anahtar "gizli" çıktı).
        foreach (RaycastHit h in Physics.RaycastAll(goz, fark / mesafe, mesafe - 0.03f, 1 << GorusKatmani, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(esya) || h.collider.transform.IsChildOf(oyuncu)) continue;
            return false;
        }
        return true;
    }

    // Kaplar açıkken eşyanın oyunun nişanıyla (Etkilesimci.HedefBul) kaç noktadan seçilebildiği (ayakta + eğilmiş).
    // Denetle'deki nişan döngüsünün aynısı (20 cm'de bir nokta, 2,75 m menzil).
    static int Secenler(Esya m, Dictionary<Vector2Int, Hucre> hucreler, Transform kamera, MethodInfo hedefBul, Etkilesimci etk)
    {
        Collider c = m.GetComponentInChildren<Collider>();
        Vector3 hedef = c != null ? c.bounds.center : m.transform.position;
        int say = 0;
        foreach (var h in hucreler)
        {
            if (h.Key.x % 2 != 0 || h.Key.y % 2 != 0) continue;
            Vector3 zemin = new Vector3(h.Key.x * Adim, h.Value.Y, h.Key.y * Adim);
            Vector2 yatay = new Vector2(zemin.x - hedef.x, zemin.z - hedef.z);
            if (yatay.sqrMagnitude > Menzil * Menzil) continue;
            foreach (bool ayakta in h.Value.Ayakta ? new[] { true, false } : new[] { false })
            {
                Vector3 goz = zemin + Vector3.up * (ayakta ? AyaktaGoz : EgikGoz);
                if ((hedef - goz).magnitude > Menzil) continue;
                kamera.position = goz;
                kamera.rotation = Quaternion.LookRotation(hedef - goz);
                var sonuc = hedefBul.Invoke(etk, new object[] { null }) as MonoBehaviour;
                // Yalnızca eşyanın KENDİSİ seçildiyse sayılır (onu taşıyan çekmece değil).
                if (sonuc != null && (sonuc.gameObject == m.gameObject || sonuc.transform.IsChildOf(m.transform))) say++;
            }
        }
        return say;
    }

    // ---------- KAYIT TESTİ (kat başı kaydı, #83) ----------
    // Kural: kat, kata varıldığı an kaydedilir; kat içindeki ilerleme oyun kapatılıp açılınca SİLİNİR, kat ilk kez
    // giriliyormuş gibi başlar (Kayit.KatBasinaDon). 1. adım (oyun modunda, kat açıkken): kat başı anı alınır,
    // zincirin ilk yarısı kayda "çözüldü" diye yazılır ve çantaya sahte bir alet konur (o katta bulunmuş gibi).
    // Sonra oyun kapatılıp aynı test kaydıyla yeniden açılır ve KayitDenetle çağrılır.
    const string DenemeAleti = "deneme_aleti";

    public static string KayitHazirla(int kat)
    {
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>();
        KilitZinciri zincir = yonetici != null ? yonetici.KatinZinciri(kat) : null;
        if (zincir == null) return "zincir yok";
        Kayit.KatBasiniKaydet();          // kata varış anı (bu testte kat_gir.cs katı doğrudan açtı)
        Aletler.Ekle(DenemeAleti);        // kat içinde bulunan bir alet: yeniden açılışta gitmeli
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) { odalar.Add(o.Ad); odalar.Add(KilitZinciri.IkinciOnek + o.Ad); }
        KilitZinciri.Sonuc sonuc = zincir.Cozumle(odalar);
        int yari = sonuc.AcilanKilitler.Count / 2;
        var yazilan = new List<string>();
        for (int i = 0; i < yari; i++) { Kayit.KilitCozuldu(kat, sonuc.AcilanKilitler[i]); yazilan.Add(sonuc.AcilanKilitler[i]); }
        KayitDeposu.Oyun.Save();   // #122: dosya
        return "kayda çözüldü yazılan: " + string.Join(",", yazilan);
    }

    // 2. adım (yeniden açılan oyunda): kat baştan başlamış mı? Katın hiçbir kilidi çözük olmamalı (kapılar ve
    // parçalar kilitli), kat içinde bulunan alet çantadan gitmiş olmalı. Ayrıca her kilidin sahnedeki durumu
    // kayıtla aynı olmalı (eski denetim).
    public static string KayitDenetle(int kat)
    {
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>();
        KilitZinciri zincir = yonetici != null ? yonetici.KatinZinciri(kat) : null;
        if (zincir == null) return "zincir yok";
        var rapor = new StringBuilder();
        int hata = 0, cozuk = 0, kilitli = 0;
        foreach (Kilit k in Object.FindObjectsByType<Kilit>())
        {
            if (k.Kat != kat || k.Veri == null) continue;
            bool beklenen = Kayit.KilitCozukMu(kat, k.Veri.Kimlik);
            IcKapi kapi = k.GetComponent<IcKapi>();
            AcilirParca parca = k.GetComponent<AcilirParca>();
            bool dogru = k.Cozuk == beklenen;
            if (kapi != null && kapi.Kilitli == beklenen) dogru = false;
            if (parca != null && parca.Kilitli == beklenen) dogru = false;
            if (!dogru) { hata++; rapor.Append($"HATA:{k.Veri.Kimlik}(kayıt {beklenen}, kilit {k.Cozuk}) "); }
            else if (beklenen) cozuk++; else kilitli++;
        }
        foreach (ZincirEsyasi e in zincir.Esyalar)
        {
            bool kullanildi = false;
            foreach (ZincirKilidi k in zincir.Kilitler)
                if (k.Gerekli == e.Kimlik && (k.Tur == KilitTuru.Anahtar || k.Tur == KilitTuru.Sigorta) && Kayit.KilitCozukMu(kat, k.Kimlik)) kullanildi = true;
            if (!kullanildi) continue;
            foreach (Esya es in Object.FindObjectsByType<Esya>())
                if (es.Kimlik == e.Kimlik && es.gameObject.activeInHierarchy) { hata++; rapor.Append($"HATA:kullanılmış {e.Kimlik} yeniden konmuş "); }
        }
        if (cozuk > 0) { hata++; rapor.Append($"HATA:kat baştan başlamadı ({cozuk} kilit çözük kaldı) "); }
        if (Aletler.Var(DenemeAleti)) { hata++; rapor.Append("HATA:kat içinde bulunan alet çantada kaldı "); }
        return $"{cozuk} çözük, {kilitli} kilitli, {hata} hata " + rapor;
    }

    // ---------- ÇAKIŞMA TESTİ ----------
    // Açık dairelerdeki süslerin ve kalıcı eşyaların çarpışma kutuları birbirine ya da duvara 6 cm'den fazla
    // gömülüyor mu? Üst üste duranlar (TV dolabın üstünde) ve yassı olanlar (halı) sayılmaz. Kutu sınırları
    // (bounds) üzerinden ölçülür: 90 derecenin katları dışında döndürülmüş eşyada biraz fazla çıkabilir.
    public static string Cakismalar()
    {
        var rapor = new StringBuilder();
        var eslesmeler = new HashSet<string>();
        foreach (DaireEksenleri d in KatDaireleri())
        {
            if (!d.gameObject.activeInHierarchy) continue;
            var parcalar = new List<(Collider c, string kok, bool yapi)>();
            foreach (Collider c in d.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger || c.GetComponentInParent<Esya>() != null || c.GetComponentInParent<IcKapi>() != null) continue;
                Transform kok = c.transform;
                while (kok.parent != null && kok.parent != d.transform && kok.parent.name != "Esyalar") kok = kok.parent;
                bool yapi = YapiMi(kok.name) || kok.name == "Esyalar";
                parcalar.Add((c, kok.name, yapi));
            }
            for (int i = 0; i < parcalar.Count; i++)
                for (int j = i + 1; j < parcalar.Count; j++)
                {
                    var a = parcalar[i]; var b = parcalar[j];
                    if (a.kok == b.kok || (a.yapi && b.yapi)) continue;
                    Bounds x = a.c.bounds, y = b.c.bounds;
                    if (x.size.y < 0.05f || y.size.y < 0.05f) continue;                         // yassı: halı, kâğıt
                    if (x.min.y >= y.max.y - 0.05f || y.min.y >= x.max.y - 0.05f) continue;      // üst üste duruyor
                    float dx = Mathf.Min(x.max.x, y.max.x) - Mathf.Max(x.min.x, y.min.x);
                    float dz = Mathf.Min(x.max.z, y.max.z) - Mathf.Max(x.min.z, y.min.z);
                    float dy = Mathf.Min(x.max.y, y.max.y) - Mathf.Max(x.min.y, y.min.y);
                    float derinlik = Mathf.Min(dx, Mathf.Min(dz, dy));
                    if (derinlik <= 0.06f) continue;
                    string ad = a.kok + "<>" + b.kok;
                    if (eslesmeler.Add(ad)) rapor.Append($"{ad}:{derinlik:0.00}m ");
                }
        }
        return eslesmeler.Count + " çakışma | " + rapor;
    }

    // ---------- EKSİKLER TESTİ (#83) ----------
    // Öteki testlerin görmediği dört tür kusuru arar (oyun modunda, kat açıkken):
    //   HAVADA:       ne yere, ne bir eşyanın üstüne, ne duvara değen süs (kat dosyasında yanlış yükseklik).
    //   CARPISMASIZ:  yerde duran, boyu 35 cm'yi aşan ama çarpışması olmayan eşya: oyuncu içinden geçer.
    //   MALZEME:      boş malzeme yuvası ya da pembe hata gölgelendiricisi (kırık malzeme).
    //   KAPI:         oda kapısının kanadı açılırken (0°'den açık açıya) bir eşyanın içine 3 cm'den fazla giriyor.
    // Perdeler CARPISMASIZ'da ayrı sayılır: kumaş, içinden geçilmesi kusur sayılmaz.
    public static string Eksikler()
    {
        const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;
        var rapor = new StringBuilder();
        int havada = 0, carpismasiz = 0, malzeme = 0, kapi = 0;
        foreach (DaireEksenleri d in KatDaireleri())
        {
            if (!d.gameObject.activeInHierarchy) continue;
            float taban = d.transform.position.y;                     // dairenin döşemesi
            float tavan = taban + DairePlani.TavanYuk;

            // Süslerin ve kalıcı eşyaların kökleri: dairenin (ya da Esyalar'ın) doğrudan çocukları.
            var kokler = new List<Transform>();
            foreach (Transform c in d.transform)
            {
                if (!c.gameObject.activeInHierarchy) continue;
                if (c.name == "Esyalar") { foreach (Transform e2 in c) if (e2.gameObject.activeInHierarchy) kokler.Add(e2); }
                else if (!YapiMi(c.name)) kokler.Add(c);
            }
            foreach (Transform kok in kokler)
            {
                // Yalnızca görünen çiziciler: kapalı olanın (eşyanın parıltı dörtgeni) sınırı anlamsızdır.
                var rs = new List<Renderer>();
                foreach (Renderer r0 in kok.GetComponentsInChildren<Renderer>()) if (r0.enabled) rs.Add(r0);
                if (rs.Count == 0) continue;
                // MALZEME: her çizicinin her yuvası dolu ve hata gölgelendiricisi değil mi?
                foreach (Renderer r in rs)
                    foreach (Material m in r.sharedMaterials)
                        if (m == null || m.shader == null || m.shader.name == "Hidden/InternalErrorShader")
                        { malzeme++; rapor.Append($"MALZEME:{kok.name}/{r.name} "); }
                Bounds b = rs[0].bounds;
                foreach (Renderer r in rs) b.Encapsulate(r.bounds);
                if (b.size.y < 0.005f) continue;                       // kâğıt, halı gibi yassı şeyler

                bool yerde = b.min.y < taban + 0.03f;
                bool tavanda = b.max.y > tavan - 0.10f;
                // Duvara değiyor mu: kutunun 4 cm büyüğü bir yapı çarpışmasına (duvar, kapı kasası) değer mi?
                bool duvarda = false;
                foreach (Collider c in Physics.OverlapBox(b.center, b.extents + Vector3.one * 0.04f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                    if (!c.transform.IsChildOf(kok) && YapiKokuMu(c.transform, d.transform)) { duvarda = true; break; }
                // Bir şeyin üstünde mi: altının beş noktasından aşağı 6 cm bak (kendi parçaları sayılmaz).
                bool ustunde = false;
                foreach (Vector2 o in new[] { Vector2.zero, new Vector2(0.4f, 0.4f), new Vector2(-0.4f, 0.4f), new Vector2(0.4f, -0.4f), new Vector2(-0.4f, -0.4f) })
                {
                    Vector3 bas = new Vector3(b.center.x + o.x * b.extents.x, b.min.y + 0.02f, b.center.z + o.y * b.extents.z);
                    foreach (RaycastHit h in Physics.RaycastAll(bas, Vector3.down, 0.08f, ~0, QueryTriggerInteraction.Ignore))
                        if (!h.collider.transform.IsChildOf(kok)) { ustunde = true; break; }
                    if (ustunde) break;
                }
                // Görüntüsüyle destek: başka bir kökün çizicisinin üst yüzü bu eşyanın altına 3 cm yakınsa ve
                // yatayda altında kalıyorsa (ışın, çarpışması olmayan desteği ya da kutusunun içini göremez).
                if (!ustunde)
                    foreach (Transform o2 in kokler)
                    {
                        if (o2 == kok) continue;
                        foreach (Renderer r2 in o2.GetComponentsInChildren<Renderer>())
                        {
                            if (!r2.enabled) continue;
                            Bounds b2 = r2.bounds;
                            // Başka bir eşyaya 1 cm yakından değiyorsa ona asılı ya da dayalıdır (perde → korniş dirseği).
                            Bounds genis = b; genis.Expand(0.02f);
                            if (genis.Intersects(b2)) { ustunde = true; break; }
                            if (b2.max.y < b.min.y - 0.03f || b2.min.y > b.min.y + 0.01f) continue;
                            if (b.center.x < b2.min.x || b.center.x > b2.max.x || b.center.z < b2.min.z || b.center.z > b2.max.z) continue;
                            ustunde = true; break;
                        }
                        if (ustunde) break;
                    }
                if (!yerde && !tavanda && !duvarda && !ustunde)
                { havada++; rapor.Append($"HAVADA:{kok.name}(alt {b.min.y - taban:0.00}m) "); }

                // CARPISMASIZ: yerde, 35 cm'den uzun, tabanı 15 x 15 cm'den geniş, hiç çarpışması yok.
                if (yerde && b.size.y > 0.35f && b.size.x > 0.15f && b.size.z > 0.15f && kok.GetComponentInChildren<Collider>() == null)
                {
                    bool perde = kok.name.Contains("Perde");
                    if (!perde) carpismasiz++;
                    rapor.Append(perde ? $"(perde çarpışmasız:{kok.name}) " : $"CARPISMASIZ:{kok.name} ");
                }
            }

            // KAPI: her oda kapısını 0°'den açık açıya 15°'lik adımlarla döndür, kanadın çarpışmasının bir
            // eşyaya ne kadar girdiğini ölç (ComputePenetration: iki çarpışmanın iç içe geçme derinliği).
            foreach (IcKapi k in d.GetComponentsInChildren<IcKapi>())
            {
                if (k.name.StartsWith("Balkon")) continue;   // balkon kapısı hep kilitli, hiç açılmaz
                float acikAci = (float)typeof(IcKapi).GetField("acikAci", Gizli).GetValue(k);
                MethodInfo uygula = typeof(IcKapi).GetMethod("AciyiUygula", Gizli);
                Transform kanat = (Transform)typeof(IcKapi).GetField("kanat", Gizli).GetValue(k);
                if (kanat == null) continue;
                bool eskiAcik = k.Acik;
                string enKotu = null; float enDerin = 0.03f; float enAci = 0f;
                for (int adim = 1; adim <= 6; adim++)
                {
                    float a = acikAci * adim / 6f;
                    uygula.Invoke(k, new object[] { a });
                    Physics.SyncTransforms();
                    foreach (Collider kc in kanat.GetComponentsInChildren<Collider>())
                        foreach (Collider c in Physics.OverlapBox(kc.bounds.center, kc.bounds.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                        {
                            if (c.transform.IsChildOf(k.transform) || YapiKokuMu(c.transform, d.transform)) continue;
                            if (c.GetComponentInParent<Esya>() != null) continue;
                            if (Physics.ComputePenetration(kc, kc.transform.position, kc.transform.rotation, c, c.transform.position, c.transform.rotation, out Vector3 _, out float derin) && derin > enDerin)
                            { enDerin = derin; enKotu = KokAdi(c.transform, d.transform); enAci = a; }
                        }
                }
                uygula.Invoke(k, new object[] { eskiAcik ? acikAci : 0f });
                if (enKotu != null) { kapi++; rapor.Append($"KAPI:{k.name}<>{enKotu}({enDerin:0.00}m, {enAci:0}°) "); }
            }
            Physics.SyncTransforms();
        }
        return $"havada {havada}, çarpışmasız {carpismasiz}, malzeme {malzeme}, kapı {kapi} | " + rapor;
    }

    // Bir çarpışmanın, dairenin altındaki kökünün adı (Esyalar'ın içindeyse onun çocuğu).
    static string KokAdi(Transform t, Transform daire)
    {
        while (t.parent != null && t.parent != daire && t.parent.name != "Esyalar") t = t.parent;
        return t.name;
    }

    // Çarpışma yapıya mı ait (duvar, döşeme, kapı kasası)? Dairenin dışındaki her şey (sahanlık) de yapıdır.
    static bool YapiKokuMu(Transform t, Transform daire)
    {
        if (!t.IsChildOf(daire)) return true;
        return YapiMi(KokAdi(t, daire));
    }

    // Yürünebilir alanı başlangıç noktasından doldurur (genişlik öncelikli arama). Sınır: sahanlığın (ve onun
    // çocukları olan dairelerin) dünya kutusu, başlangıcın katından ±0,4 m (merdivene inip çıkmasın).
    static void Dolas(Vector3 basla, Transform sahanlik, Transform oyuncu, Dictionary<Vector2Int, Hucre> hucreler, Dictionary<string, int> engeller)
    {
        Bounds sinirKutusu = new Bounds(basla, Vector3.one);
        if (sahanlik != null)
            foreach (Renderer r in sahanlik.GetComponentsInChildren<Renderer>()) sinirKutusu.Encapsulate(r.bounds);
        if (!ZemineBak(basla + Vector3.up * 0.5f, out float y0)) return;
        float katY = y0;
        var kuyruk = new Queue<Vector2Int>();
        Vector2Int ilk = new Vector2Int(Mathf.RoundToInt(basla.x / Adim), Mathf.RoundToInt(basla.z / Adim));
        hucreler[ilk] = new Hucre { Y = y0, Ayakta = true };
        kuyruk.Enqueue(ilk);
        Vector2Int[] komsular = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        int sinir = 60000;
        while (kuyruk.Count > 0 && sinir-- > 0)
        {
            Vector2Int h = kuyruk.Dequeue();
            float y = hucreler[h].Y;
            foreach (Vector2Int k in komsular)
            {
                Vector2Int n = h + k;
                if (hucreler.ContainsKey(n)) continue;
                Vector3 ust = new Vector3(n.x * Adim, y + Basamak + 0.4f, n.y * Adim);
                if (ust.x < sinirKutusu.min.x || ust.x > sinirKutusu.max.x || ust.z < sinirKutusu.min.z || ust.z > sinirKutusu.max.z) continue;
                if (!ZemineBak(ust, out float ny) || Mathf.Abs(ny - y) > Basamak || Mathf.Abs(ny - katY) > 0.4f) continue;
                if (!Durabilir(new Vector3(n.x * Adim, ny, n.y * Adim), oyuncu, out bool ayakta, out string engel))
                {
                    if (engel != null && !YapiMi(engel)) { engeller.TryGetValue(engel, out int say); engeller[engel] = say + 1; }
                    continue;
                }
                hucreler[n] = new Hucre { Y = ny, Ayakta = ayakta };
                kuyruk.Enqueue(n);
            }
        }
    }

    // Duvar, oda kabuğu, kapı gibi yapı parçası mı (engel listesinde sayılmaz; yalnızca eşyalar sayılır)?
    static bool YapiMi(string ad)
    {
        // Ayrintilar (#87: süpürgelik, pervaz, düğme, priz, kablo) ve Lekeler bilerek çarpışmasız bina yapısıdır.
        if (ad == "Kapilar" || ad == "Lambalar" || ad == "Ayrintilar" || ad == "Lekeler" || ad.StartsWith("Sahanlik") || ad.StartsWith("Cekirdek") || ad.StartsWith("Daire")) return true;
        foreach (DairePlani.Oda o in DairePlani.Odalar) if (o.Ad == ad) return true;
        return false;
    }

    // Haritaya bir dünya noktasını (dairenin planına çevirip) renkli kare olarak işler.
    static void Nokta(Texture2D harita, DaireEksenleri d, Vector3 dunya, Color renk, int yari)
    {
        d.PlanNoktasi(dunya, out float u, out float v);
        int x = Mathf.RoundToInt((u + 0.3f) / Adim * 4), y = Mathf.RoundToInt((v + 1.0f) / Adim * 4);
        for (int a = -yari; a <= yari; a++)
            for (int b = -yari; b <= yari; b++)
                if (x + a >= 0 && x + a < harita.width && y + b >= 0 && y + b < harita.height) harita.SetPixel(x + a, y + b, renk);
    }

    // Aşağı ışınla zemini bulur (yatay yüzey). Eşyaların nişan kutuları zemin sayılmaz.
    static bool ZemineBak(Vector3 ust, out float y)
    {
        y = 0f;
        bool bulundu = false;     // zemin tam y = 0'da olabilir: "bulunamadı" için 0'a güvenilmez
        foreach (RaycastHit h in Physics.RaycastAll(ust, Vector3.down, 1.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.GetComponentInParent<Esya>() != null) continue;
            if (h.normal.y < 0.7f) continue;
            if (!bulundu || h.point.y > y) { y = h.point.y; bulundu = true; }
        }
        return bulundu;
    }

    // Oyuncu bu zemin noktasında durabilir mi (ayakta ya da en azından eğilerek)? engel: kapsülün çarptığı eşya.
    static bool Durabilir(Vector3 zemin, Transform oyuncu, out bool ayakta, out string engel)
    {
        ayakta = Bos(zemin, AyaktaBoy, oyuncu, out engel);
        if (ayakta) return true;
        return Bos(zemin, EgikBoy, oyuncu, out engel);
    }

    // Kapsül boş mu; değilse çarptığı ilk çarpışmanın kök adı. Basamak eşiğinin altında kalan engeller
    // (halı, eşik) ve eşyaların nişan kutuları sayılmaz.
    static bool Bos(Vector3 zemin, float boy, Transform oyuncu, out string engel)
    {
        engel = null;
        Vector3 alt = zemin + Vector3.up * (Yaricap + 0.05f);
        Vector3 ust = zemin + Vector3.up * (boy - Yaricap);
        foreach (Collider c in Physics.OverlapCapsule(alt, ust, Yaricap, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(oyuncu)) continue;
            if (c.GetComponentInParent<Esya>() != null) continue;
            if (c.bounds.max.y <= zemin.y + Basamak) continue;
            // Kök ad: dairenin ya da Esyalar'ın hemen altındaki nesne (süsün adı, "Sus_Masa_K3" gibi).
            Transform kok = c.transform;
            while (kok.parent != null && kok.parent.GetComponent<DaireEksenleri>() == null && kok.parent.name != "Esyalar") kok = kok.parent;
            engel = kok.name;
            return false;
        }
        return true;
    }
}
#endif
