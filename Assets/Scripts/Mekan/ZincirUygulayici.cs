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
        // Kilidi sökülen kapılar serbest; balkon kapısı kendi kuralıyla (hep kilitli) kalır.
        foreach (IcKapi k in daire.GetComponentsInChildren<IcKapi>(true)) k.Kilitle(false);
        foreach (AcilirParca p in daire.GetComponentsInChildren<AcilirParca>(true)) p.Kilitle(false);
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
