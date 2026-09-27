// ZincirUygulayici.cs
// Bir katın kilit zincirini dairenin sahnesine uygular (#54).
//
// Daire nesnesi dört tane ve sekiz katta yeniden kullanılıyor (1. ve 5. katta aynı 2 numara).
// Bu yüzden her kat açılırken daire önce TEMİZLENİR (önceki katın eşyaları kaldırılır,
// kilitler sökülür, kapılar serbest kalır), sonra o katın zinciri kurulur:
//   1. Kapalı odaların kapıları hiç açılmayacak şekilde kilitlenir.
//   2. Her kilit, hedefi olan kapıya ya da parçaya bir Kilit bileşeni olarak eklenir.
//   3. Her eşya, şablonundan (prefab) kopyalanıp yerine konur; kimliği ve katı yazılır.
// Yerlerin ve kapıların yolu dairenin köküne göredir ("Kapilar/Kapi_Oda_A",
// "Esyalar/Komodin/Cekmece"); dört daire aynı planla kurulduğu için yollar her dairede aynı.
//
// MonoBehaviour değil: sahnede kendi başına durmaz, KatYoneticisi'nin elinde bir araçtır.

using System.Collections.Generic;
using UnityEngine;

public class ZincirUygulayici
{
    // Şablonlar: eşya türü -> prefab. KatYoneticisi bağlı referanslardan verir (build'e girsinler diye).
    private readonly Dictionary<EsyaTuru, GameObject> sablonlar = new Dictionary<EsyaTuru, GameObject>();

    // Sigorta kutusunun dairedeki yolu ve sigorta kilidi olan katta eksik bırakılan sigorta.
    private const string SigortaKutusuYolu = "Esyalar/Sigorta_Kutusu";
    private const string EksikSigorta = "Sigorta_2";

    /// <summary>Şifreli kilitlerin kapıya takılan asma kilidi (prefab).</summary>
    public GameObject AsmaKilitSablonu;
    /// <summary>Dairelere konan okunabilir not kâğıdı (prefab, YoneticiNotu bileşenli).</summary>
    public GameObject NotSablonu;
    /// <summary>Kırma kilidinde kırılan camın yere saçılan parçaları (prefab).</summary>
    public GameObject CamKirikSablonu;
    /// <summary>Tornavida kilitli kapılara takılan menteşe plakası (iki vidalı prefab).</summary>
    public GameObject MentesePlakasiSablonu;
    /// <summary>İpucu yazıları bu yazının yazı tipini ve malzemesini kullanır (kapı numarası).</summary>
    public TextMesh YaziOrnegi;

    // Bu uygulayıcının dairelere koyduğu eşyalar; bir sonraki temizlikte kaldırılır.
    private readonly List<GameObject> konanlar = new List<GameObject>();

    // Planda nokta olan yerler için konan boş çapa nesneleri (#63), yer kimliğine göre.
    // Dictionary: bir anahtardan (kimlik) bir değere (nesne) hızlı ulaşan bir sözlük.
    private readonly Dictionary<string, Transform> capalar = new Dictionary<string, Transform>();

    public void SablonEkle(EsyaTuru tur, GameObject sablon)
    {
        if (sablon != null) sablonlar[tur] = sablon;
    }

    /// <summary>Daireyi temizler ve (varsa) zinciri kurar. zincir null: bütün odalar açık.</summary>
    public void Uygula(Transform daire, KilitZinciri zincir, int kat)
    {
        Temizle(daire);
        if (zincir == null) return;
        DaireElektrigi elektrik = Elektrik(daire);

        // 1. Kapalı odalar: kilitli ve kilidi hiçbir şeyle açılmaz (üstünde Kilit bileşeni yok).
        foreach (string oda in zincir.KapaliOdalar)
        {
            IcKapi k = KapiBul(daire, oda);
            if (k != null) k.Kilitle(true);
            else Debug.LogWarning($"[ZINCIR] Kapalı oda '{oda}' için kapı bulunamadı.");
        }

        // 2. Kilitler.
        foreach (ZincirKilidi veri in zincir.Kilitler)
        {
            Transform hedef = veri.OdaKapisi ? KapiYolu(daire, veri.Hedef) : YerinKilitYolu(daire, zincir, veri.Hedef);
            if (hedef == null) { Debug.LogWarning($"[ZINCIR] '{veri.Kimlik}' kilidinin hedefi bulunamadı."); continue; }
            Kilit kilit = hedef.gameObject.AddComponent<Kilit>();
            kilit.Ayarla(veri, kat);
            if (veri.Tur == KilitTuru.Kol)
            {
                // Kolun kendisine CekilenKol eklenir; kol çekilince bu kilidi açar.
                Transform kolYeri = YerYolu(daire, zincir, veri.Gerekli);
                if (kolYeri != null)
                {
                    CekilenKol kol = kolYeri.GetComponent<CekilenKol>();
                    if (kol == null) kol = kolYeri.gameObject.AddComponent<CekilenKol>();
                    // Sigorta kutusunun ana şalteri kısa bir kol: daha az döner.
                    kol.KilitEkle(kilit, kolYeri.name == "Salter" ? 100f : 120f);
                }
            }
            if (veri.Tur == KilitTuru.Kirma)
            {
                // Kırılacak nesne yerin kendisi (kavanozun gövdesi); kilit KilitYolu'nda (kapağında).
                Transform kirilacak = YerYolu(daire, zincir, veri.Hedef);
                if (kirilacak != null)
                {
                    Kirilan k = kirilacak.GetComponent<Kirilan>();
                    if (k == null) k = kirilacak.gameObject.AddComponent<Kirilan>();
                    k.Hazirla(CamKirikSablonu);
                }
            }
            if (veri.Tur == KilitTuru.Tornavida)
            {
                // Kapının koridor tarafına iki menteşe plakası; vidaları tornavidayla sökülür.
                IcKapi ic = hedef.GetComponent<IcKapi>();
                if (ic != null)
                {
                    MenteseSokucu sokucu = hedef.gameObject.AddComponent<MenteseSokucu>();
                    konanlar.AddRange(sokucu.Hazirla(ic, MentesePlakasiSablonu));
                }
            }
            if (veri.Tur == KilitTuru.Sigorta)
            {
                // Sigorta kutusundan bir sigorta eksik, daire elektriksiz; doğru buşon takılınca
                // elektrik gelir ve bu kilit açılır.
                Transform kutu = daire.Find(SigortaKutusuYolu);
                if (kutu != null)
                {
                    SigortaYuvasi yuva = kutu.GetComponent<SigortaYuvasi>();
                    if (yuva == null)
                    {
                        yuva = kutu.gameObject.AddComponent<SigortaYuvasi>();
                        Transform eksik = kutu.Find(EksikSigorta);
                        Transform kapak = kutu.Find("Kapak");
                        yuva.Hazirla(veri.Gerekli, eksik != null ? eksik.gameObject : null,
                                     kapak != null ? kapak.GetComponent<AcilirParca>() : null, elektrik);
                    }
                    yuva.KilitEkle(kilit);
                }
                else Debug.LogWarning("[ZINCIR] Sigorta kutusu bulunamadı: " + SigortaKutusuYolu);
            }
            if (veri.Tur == KilitTuru.Sifre)
            {
                GameObject asma = kilit.AsmaKilitTak(AsmaKilitSablonu);
                if (asma != null) konanlar.Add(asma);
                IpucuYaz(daire, zincir, veri);
            }
        }

        // 3. Eşyalar. Kaydedilmiş, çözülmüş bir kilidi açmak için KULLANILAN eşya (anahtar,
        // buşon, tornavida) artık yoktur: konmaz.
        foreach (ZincirEsyasi e in zincir.Esyalar)
        {
            if (KullanilmisMi(zincir, e, kat)) continue;
            // Kapıcının çantasında zaten olan alet (önceki bir katta bulunan tornavida) yeniden konmaz.
            if (AletCantasi.AletMi(e.Tur) && Aletler.Var(e.Kimlik)) continue;
            // Bulunmuş bir çizim parçası (kayıtta) yeniden konmaz (#63).
            if (e.Tur == EsyaTuru.Parca && Kayit.ParcaBulunduMu(kat, e.Kimlik)) continue;
            Transform yer = YerYolu(daire, zincir, e.Yer);
            if (yer == null) { Debug.LogWarning($"[ZINCIR] '{e.Kimlik}' eşyasının yeri bulunamadı."); continue; }
            if (!sablonlar.TryGetValue(e.Tur, out GameObject sablon)) { Debug.LogWarning($"[ZINCIR] {e.Tur} şablonu yok."); continue; }
            // Instantiate: şablonun bir kopyasını yaratır; ikinci değer ebeveyni, false "yerel konumu koru".
            GameObject nesne = Object.Instantiate(sablon, yer, false);
            nesne.name = "Zincir_" + e.Kimlik;
            nesne.transform.localPosition = new Vector3(e.X, e.Y, e.Z);
            nesne.transform.localRotation = Quaternion.Euler(0f, e.Donus, 0f);
            Esya bilesen = nesne.GetComponent<Esya>();
            if (bilesen != null) bilesen.Ayarla(e.Tur, e.Kimlik, e.Tur == EsyaTuru.KatAnahtari || e.Tur == EsyaTuru.Parca ? kat : 0);
            // Yerin kilitlenen parçası (dolabın kapağı, buzdolabının kapısı) eşyanın kabının kapağıdır (#64).
            ZincirYeri yerVerisi = zincir.YerBul(e.Yer);
            if (bilesen != null && yerVerisi != null && !yerVerisi.PlandaNokta && !string.IsNullOrEmpty(yerVerisi.KilitYolu))
            {
                Transform kapak = YolBul(daire, yerVerisi.KilitYolu);
                AcilirParca ap = kapak != null ? kapak.GetComponent<AcilirParca>() : null;
                if (ap != null) bilesen.KapAyarla(ap);
            }
            konanlar.Add(nesne);
        }

        // 4. Kayıt (#54): bu katta daha önce çözülmüş kilitler çözük başlar.
        KayittanYukle(daire, zincir, kat);
    }

    /// <summary>
    /// Katın notlarını ve dekorunu (koliler...) daireye koyar (#55). Uygula'dan SONRA çağrılır;
    /// konanlar bir sonraki katta Uygula'nın temizliğiyle kalkar.
    /// </summary>
    public void DekorUygula(Transform daire, KatZinciriVarligi varlik, int kat)
    {
        if (varlik == null) return;
        DaireEksenleri eksen = daire.GetComponent<DaireEksenleri>();
        // Yırtık çizim (#63): dairenin köküne çizim ekranı; konan parçalar kendi bölgelerini gösterir.
        if (varlik.parcalar != null && varlik.parcalar.Count > 0)
        {
            CizimEkrani ekran = daire.gameObject.AddComponent<CizimEkrani>();
            ekran.Hazirla(kat, varlik.parcalar);
            foreach (GameObject g in konanlar)
            {
                Esya e = g != null ? g.GetComponent<Esya>() : null;
                if (e == null || e.Tur != EsyaTuru.Parca) continue;
                foreach (KatParcasi p in varlik.parcalar)
                    if (p.Kimlik == e.Kimlik) ParcayiGiydir(g, p, varlik.cizimBoyu);
            }
        }
        foreach (KatNotu n in varlik.notlar)
        {
            GameObject g = Yerlestir(daire, eksen, NotSablonu, n, "Not_" + n.Metin);
            YoneticiNotu not = g != null ? g.GetComponent<YoneticiNotu>() : null;
            if (not != null) not.Ayarla(n.Metin);
        }
        foreach (KatSusu s in varlik.suslar)
            Yerlestir(daire, eksen, s.Sablon, s, "Sus_" + (s.Sablon != null ? s.Sablon.name : "?"));
        // Tavan lambaları (#64): bu katta lambası yanabilen odalar (elektrik varken yanarlar).
        DaireLambalari lambalar = daire.GetComponent<DaireLambalari>();
        if (lambalar != null) lambalar.Ayarla(varlik.yananOdalar);
        // Kendiliğinden çalan eşyalar (#62): mobilyanın kendisine CalanEsya eklenir; kapısı
        // kapalıyken sesini boğabilsin diye odasının kapısı da verilir.
        foreach (KatSesi ks in varlik.sesler)
        {
            Transform t = YolBul(daire, ks.Yol);
            if (t == null) { Debug.LogWarning($"[ZINCIR] Çalan eşya '{ks.Yol}' bulunamadı."); continue; }
            CalanEsya c = t.gameObject.AddComponent<CalanEsya>();
            c.Hazirla(ks.Klip, ks.Gecikme, ks.Ses, ks.Oda, KapiBul(daire, ks.Oda), eksen, ks.Kapatilabilir, ks.SusturanKilit, kat);
        }
    }

    // Kâğıt parçasının yüzüne kendi bölgesini giydirir: şablondaki "Kagit" dörtgeni bölgenin
    // en-boy oranına göre ölçeklenir, malzemeye yalnızca bölgeyi gösteren döşeme (tiling) ve
    // kaydırma (offset) verilir. MaterialPropertyBlock: malzemeyi kopyalamadan tek bir nesneye
    // özel değer vermenin yolu (dört parça tek malzemeyi paylaşır; bkz. anomali renkleri).
    private static void ParcayiGiydir(GameObject parca, KatParcasi p, float cizimBoyu)
    {
        Transform kagit = parca.transform.Find("Kagit");
        MeshRenderer r = kagit != null ? kagit.GetComponent<MeshRenderer>() : null;
        if (r == null || p.Doku == null) return;
        kagit.localScale = new Vector3(cizimBoyu * p.Bolge.width, cizimBoyu * p.Bolge.height, 1f);
        var blok = new MaterialPropertyBlock();
        r.GetPropertyBlock(blok);
        blok.SetTexture("_BaseMap", p.Doku);
        // _BaseMap_ST: (döşeme x, döşeme y, kaydırma x, kaydırma y).
        blok.SetVector("_BaseMap_ST", new Vector4(p.Bolge.width, p.Bolge.height, p.Bolge.x, p.Bolge.y));
        r.SetPropertyBlock(blok);
    }

    // Bir şablonu yerleşim bilgisine göre koyar (bkz. KatYerlesimi: çapaya göre ya da plana göre).
    private GameObject Yerlestir(Transform daire, DaireEksenleri eksen, GameObject sablon, KatYerlesimi y, string ad)
    {
        if (sablon == null) { Debug.LogWarning($"[ZINCIR] '{ad}' için şablon yok."); return null; }
        Transform ebeveyn;
        Vector3 konum;
        Quaternion donus;
        if (!string.IsNullOrEmpty(y.Yol))
        {
            ebeveyn = YolBul(daire, y.Yol);
            if (ebeveyn == null) { Debug.LogWarning($"[ZINCIR] '{ad}' çapası '{y.Yol}' bulunamadı."); return null; }
            konum = new Vector3(y.X, y.Y, y.Z);
            donus = Quaternion.Euler(0f, y.Donus, 0f);
        }
        else
        {
            if (eksen == null) { Debug.LogWarning("[ZINCIR] Dairede DaireEksenleri yok; graybox yeniden kurulmalı."); return null; }
            ebeveyn = daire;
            konum = eksen.Nokta(y.X, y.Y, y.Z);
            Vector3 on = eksen.Yon(y.OnU, y.OnV);
            // Modellerin ön yüzü +Z'dir; LookRotation onu plandaki yöne çevirir.
            donus = on.sqrMagnitude > 0.001f ? Quaternion.LookRotation(on, Vector3.up) : Quaternion.identity;
        }
        // Yatık: dik duran kâğıt masaya konunca ön yüzü yukarı baksın (x ekseninde -90°).
        if (y.Yatik) donus = donus * Quaternion.Euler(-90f, 0f, 0f);
        GameObject g = Object.Instantiate(sablon, ebeveyn, false);
        g.name = ad;
        g.transform.localPosition = konum;
        g.transform.localRotation = donus;
        konanlar.Add(g);
        return g;
    }

    // Bu eşya, kayıtta çözülmüş görünen bir kilidi açmak için mi kullanıldı?
    private static bool KullanilmisMi(KilitZinciri zincir, ZincirEsyasi e, int kat)
    {
        foreach (ZincirKilidi k in zincir.Kilitler)
            // Aletler (tornavida: menteşe ve kırma) kullanılınca bitmez; yalnızca anahtar ve buşon biter.
            if (k.Gerekli == e.Kimlik && (k.Tur == KilitTuru.Anahtar || k.Tur == KilitTuru.Sigorta) && Kayit.KilitCozukMu(kat, k.Kimlik))
                return true;
        return false;
    }

    // Kaydedilmiş çözük kilitleri sessizce çözük başlatır: kapı serbest, asma kilit düşmüş,
    // kol aşağıda, sigorta takılı, devrik kapı yerde.
    private void KayittanYukle(Transform daire, KilitZinciri zincir, int kat)
    {
        foreach (ZincirKilidi veri in zincir.Kilitler)
        {
            if (!Kayit.KilitCozukMu(kat, veri.Kimlik)) continue;
            Transform hedef = veri.OdaKapisi ? KapiYolu(daire, veri.Hedef) : YerinKilitYolu(daire, zincir, veri.Hedef);
            Kilit kilit = null;
            if (hedef != null)
                foreach (Kilit k in hedef.GetComponents<Kilit>())
                    if (k.Veri == veri) kilit = k;
            switch (veri.Tur)
            {
                case KilitTuru.Kol:
                    Transform kol = YerYolu(daire, zincir, veri.Gerekli);
                    CekilenKol ck = kol != null ? kol.GetComponent<CekilenKol>() : null;
                    if (ck != null) ck.CekilmisBaslat();
                    break;
                case KilitTuru.Sigorta:
                    Transform kutu = daire.Find(SigortaKutusuYolu);
                    SigortaYuvasi yuva = kutu != null ? kutu.GetComponent<SigortaYuvasi>() : null;
                    if (yuva != null && !yuva.Takildi) yuva.TakiliBaslat();
                    break;
                case KilitTuru.Tornavida:
                    MenteseSokucu m = hedef != null ? hedef.GetComponent<MenteseSokucu>() : null;
                    if (m != null) m.DevrikBaslat();
                    break;
                case KilitTuru.Kirma:
                    Transform kirilacak = YerYolu(daire, zincir, veri.Hedef);
                    Kirilan kr = kirilacak != null ? kirilacak.GetComponent<Kirilan>() : null;
                    if (kr != null) kr.Kir(sesli: false);
                    break;
            }
            if (kilit != null && !kilit.Cozuk) kilit.CozukBaslat();
        }
    }

    // Önceki katın izlerini siler: konan eşyalar, eklenen kilitler, kilitlenen kapılar.
    private void Temizle(Transform daire)
    {
        foreach (GameObject g in konanlar)
        {
            if (g == null) continue;
            // Oyuncunun elindeki eşya da temizlenir: başka katın anahtarı bu katta işe yaramaz.
            if (Cep.TasinanNesne == g) Cep.TasinanNesne = null;
            Object.Destroy(g);
        }
        konanlar.Clear();
        // BİLEŞENLER HEMEN SİLİNİR (DestroyImmediate), kare sonunda değil (#64 hatası): Destroy
        // bileşeni ancak karenin sonunda kaldırır; o zamana kadar GetComponent onu bulmaya devam
        // eder. Daire aynı karede ikinci kez kurulunca (kat değişirken olabiliyor) Uygula eski
        // sigorta yuvasını "zaten var" sanıyor, elektriği kesmiyordu: 4. katta lambalar baştan
        // yanıyor, fosforlu yazı görünmüyor, buzdolabı hiç açılmıyordu. Kol, kırılan kavanoz ve
        // menteşe kilitleri de aynı "varsa yeniden ekleme" kalıbını kullanıyor.
        foreach (Kilit k in daire.GetComponentsInChildren<Kilit>(true)) Object.DestroyImmediate(k);
        // Kırılan nesneler bütünleşir. Sahanlıktaki yerler de temizlenir (dairenin ebeveyni).
        foreach (Kirilan k in daire.GetComponentsInChildren<Kirilan>(true)) { k.Sifirla(); Object.DestroyImmediate(k); }
        // Devrilen kapılar yerine konur, vidalar geri takılır.
        foreach (MenteseSokucu m in daire.GetComponentsInChildren<MenteseSokucu>(true)) { m.Sifirla(); Object.DestroyImmediate(m); }
        // Sigorta yuvası: eksik sigorta geri görünür, elektrik gelir.
        foreach (SigortaYuvasi y in daire.GetComponentsInChildren<SigortaYuvasi>(true)) { y.Sifirla(); Object.DestroyImmediate(y); }
        // Tavan lambaları: önce "hiçbiri yanamaz" (#64); yoksa hemen aşağıda elektrik gelince önceki
        // katın listesiyle yanarlardı.
        DaireLambalari lambalar = daire.GetComponent<DaireLambalari>();
        if (lambalar != null) lambalar.Ayarla(null);
        Elektrik(daire).Ayarla(true);
        // Çekilmiş kollar eski duruşuna döner, bileşenleri sökülür.
        foreach (CekilenKol kol in daire.GetComponentsInChildren<CekilenKol>(true)) { kol.Sifirla(); Object.DestroyImmediate(kol); }
        // Çalan eşyalar susar, sesleri ve ışıkları sökülür (#62).
        foreach (CalanEsya c in daire.GetComponentsInChildren<CalanEsya>(true)) { c.Sifirla(); Object.DestroyImmediate(c); }
        // Çizim ekranı ve planda nokta olan yerlerin çapaları (#63).
        foreach (CizimEkrani c in daire.GetComponentsInChildren<CizimEkrani>(true)) Object.DestroyImmediate(c);
        foreach (Transform t in capalar.Values) if (t != null) Object.Destroy(t.gameObject);
        capalar.Clear();
        // Kilidi sökülen kapılar serbest; balkon kapısı kendi kuralıyla (hep kilitli) kalır.
        foreach (IcKapi k in daire.GetComponentsInChildren<IcKapi>(true)) k.Kilitle(false);
        foreach (AcilirParca p in daire.GetComponentsInChildren<AcilirParca>(true)) p.Kilitle(false);
    }

    // Dairenin elektrik bileşeni (kurucu ekler; yoksa burada eklenir).
    private static DaireElektrigi Elektrik(Transform daire)
    {
        DaireElektrigi e = daire.GetComponent<DaireElektrigi>();
        return e != null ? e : daire.gameObject.AddComponent<DaireElektrigi>();
    }

    // Şifrenin ipucu: ipucu yerine (takvim, boy cetveli...) yazılan metin. #55'te ipuçları
    // kata göre biçimlenecek (takvimde işaretli gün gibi); şimdilik verideki metin ya da şifre.
    private void IpucuYaz(Transform daire, KilitZinciri zincir, ZincirKilidi veri)
    {
        Transform yer = YerYolu(daire, zincir, veri.Gerekli);
        if (yer == null) return;
        GameObject g = new GameObject("Zincir_Ipucu_" + veri.Kimlik);
        g.transform.SetParent(yer, false);
        g.transform.localPosition = new Vector3(veri.IpucuX, veri.IpucuY, veri.IpucuZ);
        // TextMesh'in okunan yüzü -z'ye bakar; ön yüzü +Z'de olan eşyanın önünden okunsun diye 180°.
        g.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh yazi = g.AddComponent<TextMesh>();
        yazi.text = string.IsNullOrEmpty(veri.IpucuMetni) ? veri.Sifre : veri.IpucuMetni;
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.fontSize = 96;
        // Boy çarpanı 0 ya da boşsa 1 sayılır (eski dosyalarda alan yoktu).
        yazi.characterSize = 0.006f * (veri.IpucuOlcek > 0f ? veri.IpucuOlcek : 1f);
        yazi.color = new Color(0.03f, 0.03f, 0.03f);
        if (YaziOrnegi != null)
        {
            yazi.font = YaziOrnegi.font;
            g.GetComponent<MeshRenderer>().sharedMaterial = YaziOrnegi.GetComponent<MeshRenderer>().sharedMaterial;
        }
        // Karanlıkta parlayan boya (#64): fosfor yeşili; görünürlüğü bileşen yönetir.
        if (veri.IpucuKaranlikta)
        {
            yazi.color = new Color(0.55f, 1f, 0.6f, 1f);
            g.AddComponent<KaranliktaParlayan>();
        }
        konanlar.Add(g);
    }

    private static Transform KapiYolu(Transform daire, string oda) { return daire.Find("Kapilar/Kapi_" + oda); }

    private static IcKapi KapiBul(Transform daire, string oda)
    {
        Transform t = KapiYolu(daire, oda);
        return t != null ? t.GetComponent<IcKapi>() : null;
    }

    // Bir yer kilitlenirken kilidin takılacağı parça: KilitYolu doluysa o, değilse yerin kendisi.
    private static Transform YerinKilitYolu(Transform daire, KilitZinciri zincir, string yerKimligi)
    {
        ZincirYeri y = zincir.YerBul(yerKimligi);
        if (y == null) return null;
        return YolBul(daire, string.IsNullOrEmpty(y.KilitYolu) ? y.Yol : y.KilitYolu);
    }

    private Transform YerYolu(Transform daire, KilitZinciri zincir, string yerKimligi)
    {
        ZincirYeri y = zincir.YerBul(yerKimligi);
        if (y == null) return null;
        if (!y.PlandaNokta) return YolBul(daire, y.Yol);
        // Planda bir nokta (#63): o noktaya boş bir çapa nesnesi konur (bir kez; sonra aynısı).
        if (capalar.TryGetValue(y.Kimlik, out Transform mevcut) && mevcut != null) return mevcut;
        DaireEksenleri eksen = daire.GetComponent<DaireEksenleri>();
        if (eksen == null) return null;
        GameObject g = new GameObject("Yer_" + y.Kimlik);
        g.transform.SetParent(daire, false);
        g.transform.localPosition = eksen.Nokta(y.U, y.Y, y.V);
        // Ön yön planda verildiyse (duvardaki yazı), dairenin eksenine çevrilir; üstüne Donus.
        Vector3 on = eksen.Yon(y.OnU, y.OnV);
        Quaternion taban = on.sqrMagnitude > 0.001f ? Quaternion.LookRotation(on, Vector3.up) : Quaternion.identity;
        g.transform.localRotation = taban * Quaternion.Euler(0f, y.Donus, 0f);
        capalar[y.Kimlik] = g.transform;
        return g.transform;
    }

    /// <summary>
    /// Bir yolu bulur. "Sahanlik:" ile başlayan yol dairenin değil sahanlığın (dairenin
    /// ebeveyninin) kökünden aranır ve içindeki {n} açık dairenin numarasıyla değişir:
    /// "Sahanlik:Kapi_{n}/Ayakkabilik" 2 numarada "Kapi_2/Ayakkabilik" olur (#61).
    /// </summary>
    public static Transform YolBul(Transform daire, string yol)
    {
        const string On = "Sahanlik:";
        if (string.IsNullOrEmpty(yol)) return null;
        if (!yol.StartsWith(On)) return daire.Find(yol);
        string numara = daire.name.Substring(daire.name.LastIndexOf('_') + 1);   // "Daire_2" -> "2"
        return daire.parent != null ? daire.parent.Find(yol.Substring(On.Length).Replace("{n}", numara)) : null;
    }
}
