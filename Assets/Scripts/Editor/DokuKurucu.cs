// DokuKurucu.cs
// Graybox'ın düz beyaz kutularını gerçek dokularla kaplayan editör aracı (#41).
// Menü: Huzur → Dokuları Uygula. Graybox yeniden kurulunca da kendiliğinden çalışır.
//
// Dokular Poly Haven'dan, CC0 (Şahan seçti; kaynaklar Art/Dokular/LISANSLAR.md):
//   duvar  worn_plaster_wall      yıpranmış sıva
//   zemin  marble_mosaic_tiles    mermer mozaik (zemin, ara sahanlık, basamaklar)
//   tavan  plaster_grey_04        gri sıva (tavanlar ve döşemelerin altı)
//   kapı   sapele_veneer          kırmızımsı kahve kaplama (daire kapıları)
//   çatı   worn_concrete_floor    aşınmış beton
//
// ---------- ASIL SORUN: DOKU KOORDİNATLARI (UV) ----------
// Bir dokunun bir yüzeye nasıl yapışacağını "UV" söyler: yüzeydeki her noktanın dokunun
// hangi noktasına denk geldiği (U yatay, V dikey, 0-1 arası). Unity'nin hazır küpünde
// her yüz dokunun TAMAMINI alır. Küp 7 m uzunluğa ölçeklenince doku da 7 m'ye gerilir:
// sıvanın çatlakları uzar, mozaik taşları dikdörtgen olur. Bu yüzden her kutu için
// kendi mesh'ini (şekil verisini) üretiyoruz ve UV'yi DÜNYA ölçüsünden hesaplıyoruz:
//   UV = köşenin dünyadaki konumu / döşeme boyu
// Böylece 1,5 m'lik bir doku her 1,5 m'de bir tekrar eder, kutu ne kadar büyük olursa olsun.
// Yan yana duran iki duvar parçasında da desen kesintisiz devam eder (ikisi de aynı
// dünya koordinatını kullanıyor).
//
// Yüzün yönüne göre hangi iki eksenin kullanılacağı seçilir (kutu izdüşümü, "box mapping"):
//   yukarı/aşağı bakan yüz: (x, z)    sağa/sola bakan yüz: (z, y)    öne/arkaya bakan: (x, y)
//
// ---------- İKİ İNCE KURAL ----------
// 1) DİKEY DÖŞEME 3 M'Yİ TAM BÖLMELİ. Katlar üst üste 3 m aralıklı ve oyuncu kat değişince
//    3 m aşağı ışınlanıyor. Duvar dokusu 1,8 m'de tekrar etseydi, ışınlanma anında duvardaki
//    desen 1,2 m kayardı. 1,5 m (ya da 1, 0,75) seçince her katın duvarı birebir aynı görünür.
// 2) HAREKET EDEN PARÇALAR DÜNYAYI DEĞİL KENDİLERİNİ KULLANIR. Kapı kanadı anomalide açılıp
//    dönüyor. UV dünya konumundan hesaplansaydı bile mesh bir kez üretildiği için doku yine
//    kapıyla birlikte dönerdi; ama kapıların DÜNYA konumu her kapıda farklı olduğundan
//    dört kapı dört farklı desenle görünürdü. Kapılar kendi (ölçekli) yerel koordinatlarını
//    kullanır: dört kapı aynı görünür.
//
// ---------- YÜZE GÖRE MALZEME ----------
// Bir döşeme kutusunun üstü zemindir (mozaik), altı ise alt katın tavanıdır (sıva).
// Bu yüzden mesh'i üç parçaya (submesh) ayırıyoruz: üst yüz, alt yüz, yan yüzler; her
// parçaya ayrı malzeme veriyoruz.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class DokuKurucu
{
    const string DokuKlasoru = "Assets/Textures/";
    const string MalzemeKlasoru = "Assets/Materials/";

    // Bir yüzey türünün malzemesi: doku adı (null = dokusuz, düz renk), döşeme boyu, parlaklık, renk.
    struct Tur
    {
        public string ad, doku;
        public float doseme, parlaklik;
        public Color renk;
        public Tur(string ad, string doku, float doseme, float parlaklik, Color renk)
        { this.ad = ad; this.doku = doku; this.doseme = doseme; this.parlaklik = parlaklik; this.renk = renk; }
    }

    // Döşeme boyları (metre). Dikey yüzeylerde kullanılanlar 3'ü tam bölüyor (kural 1).
    static readonly Tur Duvar   = new Tur("Doku_Duvar", "worn_plaster_wall", 1.5f, 0.12f, Color.white);
    static readonly Tur Zemin   = new Tur("Doku_Zemin", "marble_mosaic_tiles", 1.5f, 0.45f, Color.white);
    static readonly Tur Tavan   = new Tur("Doku_Tavan", "plaster_grey_04", 1.5f, 0.08f, new Color(0.9f, 0.9f, 0.9f));
    // #81 (Şahan'ın seçimi, "ceviz kaplama çıtalı kapı"): sapele kaplamanın turuncusu koyu cevize çekilir.
    static readonly Tur Kapi    = new Tur("Doku_Kapi", "sapele_veneer", 1.0f, 0.35f, new Color(0.55f, 0.38f, 0.27f));
    static readonly Tur Cati    = new Tur("Doku_Cati", "worn_concrete_floor", 2.0f, 0.08f, Color.white);
    // Dokusuz, düz renkli küçük parçalar.
    static readonly Tur Demir   = new Tur("Doku_Demir", null, 1f, 0.3f, new Color(0.10f, 0.10f, 0.09f));    // korkuluk dikmeleri: koyu boyalı demir
    static readonly Tur Tirabzan = new Tur("Doku_Tirabzan", null, 1f, 0.35f, new Color(0.22f, 0.12f, 0.07f)); // tirabzan: koyu ahşap
    static readonly Tur Boya    = new Tur("Doku_Boya", null, 1f, 0.3f, new Color(0.78f, 0.76f, 0.70f));     // pencere, plaka, zil: kirli beyaz yağlı boya
    static readonly Tur Madeni  = new Tur("Doku_Madeni", null, 1f, 0.55f, new Color(0.52f, 0.40f, 0.20f));  // kapı kolu, kapı gözü: eskimiş pirinç (#81)
    static readonly Tur Naylon  = new Tur("Doku_Naylon", null, 1f, 0.6f, new Color(0.04f, 0.04f, 0.045f)); // çöp poşeti: siyah naylon
    static readonly Tur Kumas   = new Tur("Doku_Kumas", null, 1f, 0.05f, new Color(0.70f, 0.68f, 0.62f));   // yastık
    // 3+1 daire (#53). Bu dört tür dairenin EDİTÖRDEKİ görünüşü ve yüzeyin kimliğidir: oyunda
    // KatYoneticisi her katta bunların yerine o katın kaplamasını (KatKaplamalari) takar ve
    // hangi parçanın hangi yüzey olduğunu malzemenin bu adından tanır. Döşeme boyu 1: UV'ler
    // metre cinsinden; katın malzemesi dokuyu kendi gerçek boyuna göre tekrar eder.
    static readonly Tur DaireDuvar  = new Tur("Doku_DaireDuvar", "beige_wall_002", 1.0f, 0.08f, Color.white);
    static readonly Tur Parke       = new Tur("Doku_Parke", "herringbone_parquet", 1.0f, 0.35f, Color.white);
    static readonly Tur Fayans      = new Tur("Doku_Fayans", "terrazzo_tiles", 1.0f, 0.50f, Color.white);
    static readonly Tur FayansDuvar = new Tur("Doku_FayansDuvar", "long_white_tiles", 1.0f, 0.50f, Color.white);
    static readonly Tur IcBoya      = new Tur("Doku_IcBoya", null, 1f, 0.35f, new Color(0.58f, 0.56f, 0.51f)); // oda kapıları: kirli krem yağlı boya (#126: 0,80 fenerin 1 m'sinde bembeyaz patlıyordu)

    // Bir kutuya ne olacağı: üst, alt ve yan yüzlerin türü ve UV'nin nereden hesaplanacağı.
    struct Kural
    {
        public Tur ust, alt, yan;
        public bool yerel;   // true: kendi koordinatı (hareket eden parçalar, kural 2)
    }

    [MenuItem("Huzur/Dokuları Uygula")]
    public static void Dokula()
    {
        var malzemeler = new Dictionary<string, Material>();
        int sayi = 0;

        // Sahnedeki bütün kutuları dolaş. Yalnızca hâlâ düz beyaz varsayılan malzemeyi
        // taşıyanlar dokulanır: kendi malzemesi olan (cephe, asansör, Blender prop'ları)
        // parçalara dokunmuyoruz. Bir kez dokulanmış kutu da "Doku_" malzemesi taşır;
        // araç ikinci kez çalışınca onları da yeniler.
        foreach (MeshRenderer r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            Material m = r.sharedMaterial;
            if (m == null) continue;
            bool beyaz = m.name == "Lit" || m.name == "Default-Material" || m.name.StartsWith("Doku_");
            if (!beyaz) continue;

            MeshFilter f = r.GetComponent<MeshFilter>();
            if (f == null || f.sharedMesh == null) continue;
            // Yalnızca küpler: 24 köşeli hazır küp ya da daha önce ürettiğimiz dokulu küp.
            if (f.sharedMesh.name != "Cube" && !f.sharedMesh.name.StartsWith("Dokulu_")) continue;

            Kural? kural = KuralBul(r.transform);
            if (kural == null) continue;

            Kural k = kural.Value;
            f.sharedMesh = KupUret(r.transform, k);
            // Üç yüz de aynı türdeyse tek parça, tek malzeme: her parça ayrı bir çizim
            // çağrısı demek; 700 küpte gereksiz yere üç katı iş olurdu.
            r.sharedMaterials = TekTur(k)
                ? new[] { MalzemeAl(k.yan, malzemeler) }
                : new[] { MalzemeAl(k.ust, malzemeler), MalzemeAl(k.alt, malzemeler), MalzemeAl(k.yan, malzemeler) };
            sayi++;
        }
        KatKaplamalariniKur();
        AssetDatabase.SaveAssets();
        Debug.Log($"[DOKU] {sayi} kutu dokulandı, {malzemeler.Count} malzeme.");
    }

    // ---------- KAT KAPLAMALARI (#53) ----------
    // Sekiz katın her biri için dört malzeme (oda duvarı, parke, ıslak zemin, ıslak duvar):
    // Assets/Materials/Kaplama_K1_OdaDuvari.mat ... Kaplama_K8_IslakDuvar.mat. Hangi dokunun
    // hangi kata gideceği KatKaplamalari'nda (düz C#, testli). Malzemeler sahnedeki
    // KatYoneticisi'ne BAĞLANIR: oyun build'de onları adıyla aramaz, elindeki listeden alır
    // (build'e yalnızca bağlı olan varlıklar girer; adla arananlar girmeyebilir).
    public static void KatKaplamalariniKur()
    {
        int n = KatKaplamalari.KatSayisi * KatKaplamalari.YuzeySayisi;
        var liste = new Material[n];
        for (int kat = 1; kat <= KatKaplamalari.KatSayisi; kat++)
            for (int y = 0; y < KatKaplamalari.YuzeySayisi; y++)
            {
                var yuzey = (KatKaplamalari.Yuzey)y;
                KatKaplamalari.Kaplama k = KatKaplamalari.Getir(kat, yuzey);
                string yol = $"{MalzemeKlasoru}Kaplama_K{kat}_{yuzey}.mat";
                Material m = AssetDatabase.LoadAssetAtPath<Material>(yol);
                if (m == null)
                {
                    m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(m, yol);
                }
                m.SetColor("_BaseColor", new Color(k.R, k.G, k.B));
                m.SetFloat("_Smoothness", k.Parlaklik);
                m.SetFloat("_Metallic", 0f);
                m.SetTexture("_BaseMap", DokuAl(k.Doku + "_renk", false));
                m.SetTexture("_BumpMap", DokuAl(k.Doku + "_normal", true));
                m.EnableKeyword("_NORMALMAP");
                // UV metre cinsinden; doku "Boy" metrede bir tekrar etsin: 1 / Boy kez.
                m.SetTextureScale("_BaseMap", Vector2.one / k.Boy);
                // Terk edilmiş dairenin kiri (#78): 2,5 metrede bir tekrar eden leke ve toz katmanı.
                KirKurucu.KirEkle(m, 0.4f);
                EditorUtility.SetDirty(m);
                liste[(kat - 1) * KatKaplamalari.YuzeySayisi + y] = m;
            }

        // Sahnedeki KatYoneticisi'ne bağla. SerializedObject: bileşenin kaydedilen alanlarını
        // (Inspector'da görünen) koddan düzenlemenin yolu; özel (private) alanlara da ulaşır.
        KatYoneticisi yonetici = Object.FindAnyObjectByType<KatYoneticisi>(FindObjectsInactive.Include);
        if (yonetici == null) { Debug.LogWarning("[DOKU] Sahnede KatYoneticisi yok; kat kaplamaları bağlanmadı."); return; }
        var seri = new SerializedObject(yonetici);
        SerializedProperty dizi = seri.FindProperty("kaplamalar");
        dizi.arraySize = n;
        for (int i = 0; i < n; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = liste[i];
        seri.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[DOKU] {KatKaplamalari.KatSayisi} katın {n} kaplama malzemesi kuruldu ve KatYoneticisi'ne bağlandı.");
    }

    // ---------- HANGİ KUTU NE OLACAK ----------
    // Kutuları adlarıyla tanıyoruz. Adlar graybox kurucusunun verdiği adlar; anomali
    // kartları da aynı adları kullandığı için zaten sabitler.
    static Kural? KuralBul(Transform t)
    {
        string ad = Regex.Replace(t.name, "[0-9_]+$", "");   // "Basamak_07" -> "Basamak"
        string ebeveyn = t.parent != null ? t.parent.name : "";
        bool cati = KokAdi(t) == "CatiBolumu";   // çatıdaki oda duvarları beton

        // Daire (#53): önce bunlar, çünkü "Daire_Tavan" gibi adlar aşağıdaki genel kurallara uymaz.
        if (ad == "Daire_Parke") return Yuzeyler(Parke, Tavan, Parke);
        if (ad == "Daire_Fayans") return Yuzeyler(Fayans, Tavan, Fayans);
        if (ad == "Daire_Duvar") return Tek(DaireDuvar);
        if (ad == "Daire_Fayans_Duvar") return Tek(FayansDuvar);
        if (ad == "Daire_Tavan") return Tek(Tavan);
        if (ad == "Ic_Kanat") return Tek(IcBoya, yerel: true);
        if (ad == "Ic_Kasa") return Tek(IcBoya);
        if (ad == "Zemin" || ad == "Ara_Sahanlik") return Yuzeyler(Zemin, Tavan, Tavan);
        if (ad == "Basamak") return Yuzeyler(Zemin, Tavan, Zemin);
        if (ad == "Tavan" || ad == "Oda_Tavan") return Tek(Tavan);
        if (ad == "Cati_Zemin") return Yuzeyler(Cati, Tavan, Cati);
        if (ad.StartsWith("Parapet")) return Tek(Cati);
        if (ad.StartsWith("Duvar") || ad.StartsWith("Oda_Duvar")) return Tek(cati ? Cati : Duvar);
        if (ad == "Kanat" || ad == "CatiKapisi") return Tek(Kapi, yerel: true);
        if (ad == "Kasa_Cita") return Tek(Kapi, yerel: true);   // #81: kapının çıtaları kanatla döner, doku kaymasın
        if (ad.StartsWith("Kasa")) return Tek(Kapi);
        if (ad.StartsWith("Dikme")) return Tek(Demir);
        if (ad.StartsWith("Tirabzan")) return Tek(Tirabzan);
        if (ad.StartsWith("Cerceve") || ad == "Denizlik" || ad == "Kayit" || ad == "Numaralik" || ad == "Zil") return Tek(Boya);
        if (ad == "Kol" || ad == "Kapi_Gozu") return Tek(Madeni);
        if (ad == "Poset") return Tek(Naylon, yerel: true);
        if (ad == "Yastik") return Tek(Kumas);
        return null;   // tanımadığımız kutu: olduğu gibi bırak
    }

    static bool TekTur(Kural k) { return k.ust.ad == k.yan.ad && k.alt.ad == k.yan.ad; }

    static string KokAdi(Transform t)
    {
        while (t.parent != null) t = t.parent;
        return t.name;
    }

    static Kural Tek(Tur tur, bool yerel = false) { return new Kural { ust = tur, alt = tur, yan = tur, yerel = yerel }; }
    static Kural Yuzeyler(Tur ust, Tur alt, Tur yan) { return new Kural { ust = ust, alt = alt, yan = yan }; }

    // ---------- DOKULU KÜP ----------
    // Unity'nin küpüyle aynı şekil (24 köşe: her yüzün kendi 4 köşesi, keskin kenarlar için),
    // ama UV'ler dünya ölçüsünde ve yüzler üç parçaya (üst / alt / yan) ayrılmış.
    static Mesh KupUret(Transform t, Kural k)
    {
        var koseler = new List<Vector3>();
        var normaller = new List<Vector3>();
        var uvler = new List<Vector2>();
        var parcalar = new[] { new List<int>(), new List<int>(), new List<int>() };   // 0 üst, 1 alt, 2 yan

        // Altı yüz: her biri bir yön (normal) ve o yüzün iki kenar yönü.
        Vector3[] yonler = { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        foreach (Vector3 n in yonler)
        {
            // Yüzün düzlemindeki iki eksen: n ile çakışmayan iki eksen, sağ el kuralıyla.
            Vector3 a = Mathf.Abs(n.y) > 0.5f ? Vector3.right : Vector3.up;
            Vector3 b = Vector3.Cross(n, a);
            a = Vector3.Cross(b, n);

            int bas = koseler.Count;
            for (int i = 0; i < 4; i++)
            {
                float sa = (i == 1 || i == 2) ? 0.5f : -0.5f;
                float sb = (i >= 2) ? 0.5f : -0.5f;
                Vector3 p = n * 0.5f + a * sa + b * sb;      // küpün yerel köşesi (-0,5..0,5)
                koseler.Add(p);
                normaller.Add(n);
                uvler.Add(UV(t, p, n, k));
            }

            // Hangi parçaya: dünyada yukarı bakan yüz "üst", aşağı bakan "alt", gerisi "yan".
            // (Kutu döndürülmüşse yerel yön değil dünyadaki yön önemli.)
            float dunyaY = t.TransformDirection(n).y;
            int parca = dunyaY > 0.7f ? 0 : (dunyaY < -0.7f ? 1 : 2);
            parcalar[parca].AddRange(new[] { bas, bas + 2, bas + 1, bas, bas + 3, bas + 2 });
        }

        var mesh = new Mesh { name = "Dokulu_" + t.name };
        mesh.SetVertices(koseler);
        mesh.SetNormals(normaller);
        mesh.SetUVs(0, uvler);
        if (TekTur(k))
        {
            var hepsi = new List<int>();
            foreach (var parca in parcalar) hepsi.AddRange(parca);
            mesh.subMeshCount = 1;
            mesh.SetTriangles(hepsi, 0);
        }
        else
        {
            mesh.subMeshCount = 3;
            for (int i = 0; i < 3; i++) mesh.SetTriangles(parcalar[i], i);
        }
        // Üçgenlerin yönü (saat yönü / tersi) yüzün görünen tarafını belirler. Yukarıdaki sıra
        // doğru tarafı dışarı çeviriyor mu diye bakmak yerine, normal ile karşılaştırıp gerekirse
        // çeviriyoruz: yanlış yönde bir yüz içeriden görünür, dışarıdan görünmez olurdu.
        YonleriDuzelt(mesh);
        mesh.RecalculateTangents();   // normal haritası (yüzey kabartması) için gerekli
        mesh.RecalculateBounds();
        return mesh;
    }

    // Bir köşenin UV'si: dünya (ya da ölçekli yerel) konumunun, yüzün düzlemine düşen iki
    // bileşeni, döşeme boyuna bölünür. Tür, köşenin hangi yüzde olduğuna göre seçilir.
    static Vector2 UV(Transform t, Vector3 yerelKose, Vector3 yerelNormal, Kural k)
    {
        Vector3 p, n;
        if (k.yerel)
        {
            p = Vector3.Scale(yerelKose, t.localScale);   // kendi ölçüsüyle, konumdan bağımsız
            n = yerelNormal;
        }
        else
        {
            p = t.TransformPoint(yerelKose);
            n = t.TransformDirection(yerelNormal);
        }
        float dunyaY = t.TransformDirection(yerelNormal).y;
        Tur tur = dunyaY > 0.7f ? k.ust : (dunyaY < -0.7f ? k.alt : k.yan);
        float d = tur.doseme;

        float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
        if (ay >= ax && ay >= az) return new Vector2(p.x, p.z) / d;   // yukarı/aşağı bakan
        if (ax >= az) return new Vector2(p.z, p.y) / d;               // sağa/sola bakan
        return new Vector2(p.x, p.y) / d;                             // öne/arkaya bakan
    }

    static void YonleriDuzelt(Mesh mesh)
    {
        Vector3[] v = mesh.vertices, nr = mesh.normals;
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            int[] u = mesh.GetTriangles(s);
            for (int i = 0; i < u.Length; i += 3)
            {
                Vector3 yuz = Vector3.Cross(v[u[i + 1]] - v[u[i]], v[u[i + 2]] - v[u[i]]);
                if (Vector3.Dot(yuz, nr[u[i]]) < 0f) { int g = u[i + 1]; u[i + 1] = u[i + 2]; u[i + 2] = g; }
            }
            mesh.SetTriangles(u, s);
        }
    }

    // ---------- MALZEMELER ----------
    // Her tür için Assets/Materials altında bir malzeme dosyası: yoksa üretilir, varsa
    // ayarları yenilenir (dosya silinmez; sahnedeki bağlantılar kopmasın).
    static Material MalzemeAl(Tur tur, Dictionary<string, Material> onbellek)
    {
        if (onbellek.TryGetValue(tur.ad, out Material m)) return m;

        string yol = MalzemeKlasoru + tur.ad + ".mat";
        m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, yol);
        }

        m.SetColor("_BaseColor", tur.renk);
        m.SetFloat("_Smoothness", tur.parlaklik);
        m.SetFloat("_Metallic", 0f);
        if (tur.doku != null)
        {
            m.SetTexture("_BaseMap", DokuAl(tur.doku + "_renk", false));
            m.SetTexture("_BumpMap", DokuAl(tur.doku + "_normal", true));
            m.EnableKeyword("_NORMALMAP");   // shader'a "normal haritası var" demek
        }
        else
        {
            m.SetTexture("_BaseMap", null);
            m.SetTexture("_BumpMap", null);
            m.DisableKeyword("_NORMALMAP");
        }
        // UV'ler zaten metre/döşeme boyu; malzemedeki tekrar 1 kalır.
        m.SetTextureScale("_BaseMap", Vector2.one);
        EditorUtility.SetDirty(m);
        onbellek[tur.ad] = m;
        return m;
    }

    // Dokuyu yükler; normal haritasıysa içe aktarma ayarını "Normal map" yapar. Normal
    // haritası bir renk değil, yüzeyin her noktasının hangi yöne baktığını saklar; Unity'ye
    // bunu söylemezsek onu renk sanıp yanlış çözer ve ışık tuhaf görünür.
    public static Texture2D DokuAl(string ad, bool normal)
    {
        string yol = DokuKlasoru + ad + ".jpg";
        var ice = AssetImporter.GetAtPath(yol) as TextureImporter;
        if (ice != null)
        {
            bool degisti = false;
            if (normal && ice.textureType != TextureImporterType.NormalMap) { ice.textureType = TextureImporterType.NormalMap; degisti = true; }
            if (ice.maxTextureSize != 1024) { ice.maxTextureSize = 1024; degisti = true; }
            if (ice.wrapMode != TextureWrapMode.Repeat) { ice.wrapMode = TextureWrapMode.Repeat; degisti = true; }
            if (degisti) ice.SaveAndReimport();
        }
        Texture2D d = AssetDatabase.LoadAssetAtPath<Texture2D>(yol);
        if (d == null) Debug.LogWarning("[DOKU] Doku bulunamadı: " + yol);
        return d;
    }
}
