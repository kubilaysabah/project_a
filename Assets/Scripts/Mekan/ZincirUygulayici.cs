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
    /// <summary>Tornavida kilitli kapılara takılan menteşe plakası (iki vidalı prefab).</summary>
    public GameObject MentesePlakasiSablonu;
    /// <summary>İpucu yazıları bu yazının yazı tipini ve malzemesini kullanır (kapı numarası).</summary>
    public TextMesh YaziOrnegi;

    // Bu uygulayıcının dairelere koyduğu eşyalar; bir sonraki temizlikte kaldırılır.
    private readonly List<GameObject> konanlar = new List<GameObject>();

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

        // 3. Eşyalar.
        foreach (ZincirEsyasi e in zincir.Esyalar)
        {
            Transform yer = YerYolu(daire, zincir, e.Yer);
            if (yer == null) { Debug.LogWarning($"[ZINCIR] '{e.Kimlik}' eşyasının yeri bulunamadı."); continue; }
            if (!sablonlar.TryGetValue(e.Tur, out GameObject sablon)) { Debug.LogWarning($"[ZINCIR] {e.Tur} şablonu yok."); continue; }
            // Instantiate: şablonun bir kopyasını yaratır; ikinci değer ebeveyni, false "yerel konumu koru".
            GameObject nesne = Object.Instantiate(sablon, yer, false);
            nesne.name = "Zincir_" + e.Kimlik;
            nesne.transform.localPosition = new Vector3(e.X, e.Y, e.Z);
            nesne.transform.localRotation = Quaternion.Euler(0f, e.Donus, 0f);
            Esya bilesen = nesne.GetComponent<Esya>();
            if (bilesen != null) bilesen.Ayarla(e.Tur, e.Kimlik, e.Tur == EsyaTuru.KatAnahtari ? kat : 0);
            konanlar.Add(nesne);
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
        foreach (Kilit k in daire.GetComponentsInChildren<Kilit>(true)) Object.Destroy(k);
        // Devrilen kapılar yerine konur, vidalar geri takılır.
        foreach (MenteseSokucu m in daire.GetComponentsInChildren<MenteseSokucu>(true)) { m.Sifirla(); Object.Destroy(m); }
        // Sigorta yuvası: eksik sigorta geri görünür, elektrik gelir.
        foreach (SigortaYuvasi y in daire.GetComponentsInChildren<SigortaYuvasi>(true)) { y.Sifirla(); Object.Destroy(y); }
        Elektrik(daire).Ayarla(true);
        // Çekilmiş kollar eski duruşuna döner, bileşenleri sökülür.
        foreach (CekilenKol kol in daire.GetComponentsInChildren<CekilenKol>(true)) { kol.Sifirla(); Object.Destroy(kol); }
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
        yazi.characterSize = 0.006f;
        yazi.color = new Color(0.03f, 0.03f, 0.03f);
        if (YaziOrnegi != null)
        {
            yazi.font = YaziOrnegi.font;
            g.GetComponent<MeshRenderer>().sharedMaterial = YaziOrnegi.GetComponent<MeshRenderer>().sharedMaterial;
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
        return daire.Find(string.IsNullOrEmpty(y.KilitYolu) ? y.Yol : y.KilitYolu);
    }

    private static Transform YerYolu(Transform daire, KilitZinciri zincir, string yerKimligi)
    {
        ZincirYeri y = zincir.YerBul(yerKimligi);
        return y != null ? daire.Find(y.Yol) : null;
    }
}
