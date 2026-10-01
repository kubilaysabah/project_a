// DaireLekeleri.cs
// Dairenin duvar ve tavan lekeleri (#87): su lekesi, rutubet, çatlak, kabarmış boya, is, sızıntı.
//
// Şahan'ın No70 karşılaştırması (2026-10-01): "yüzeylerde yıpranma izi yok; duvarlar tek desen
// duvar kâğıdı." No70'in evleri su lekesi, kabarmış boya, priz çevresi kararması ve rutubetle dolu.
//
// DECAL NEDİR: duvara yapıştırılan bir çıkartma. Unity'nin DecalProjector bileşeni bir kutunun
// içindeki her yüzeyin üstüne bir dokuyu "yansıtır" (projeksiyon): duvarın kendi dokusu durur,
// lekenin saydam olmayan yerleri onun üstüne biner. Duvar kâğıdı her katta değiştiği hâlde
// (KatKaplamalari) lekeler aynı kalır; yüzeyi değiştirmeden kirletmenin en ucuz yolu.
//
// Lekeler KAT AÇILINCA kurulur (KatYoneticisi.KaplamayiGiydir çağırır): her katın lekesi kata
// bağlı bir tohumla çekilir, böylece aynı kat her açılışta aynı lekeleri gösterir; üst katlar daha
// yıpranmıştır (oda başına leke sayısı katla artar). Yerleşim DairePlani'nin odalarından okunur:
// duvarın hangi kenarında, hangi yükseklikte, kapı ve pencerelere çakışmadan.
//
// Düşük kalitede (Ayarlar.Kalite == KaliteDusuk) lekeler kapatılır: decal her kare bir çizim
// geçişi daha demektir.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DaireLekeleri : MonoBehaviour
{
    // Leke türleri: dizideki sıra. Malzemeleri kurucu (LekeKurucu) bağlar.
    public enum Tur { Su = 0, Rutubet = 1, Catlak = 2, Kabarma = 3, Is = 4, Sizinti = 5 }

    [SerializeField] private Material[] malzemeler;   // Tur sırasında altı malzeme
    private DaireEksenleri eksen;
    private Transform kok;                            // "Lekeler" çocuğu (her kurulumda yenilenir)
    private readonly List<DecalProjector> lekeler = new List<DecalProjector>();
    private int kuruluKat = -1;

    private const float Derinlik = 0.30f;             // projeksiyon kutusunun duvara giren derinliği
    private const float Pay = 0.12f;                  // açıklıklara ve köşelere bırakılan boşluk

    private void OnEnable()
    {
        Ayarlar.Degisti += KaliteyiUygula;
        KaliteyiUygula();
    }

    private void OnDisable() { Ayarlar.Degisti -= KaliteyiUygula; }

    /// <summary>Bu kat için lekeleri kurar (daha önce kurulu olanları atar).</summary>
    public void Kur(int kat)
    {
        if (malzemeler == null || malzemeler.Length < 6) return;
        if (eksen == null) eksen = GetComponent<DaireEksenleri>();
        if (eksen == null) return;
        if (kuruluKat == kat && kok != null) return;
        kuruluKat = kat;

        if (kok != null) Destroy(kok.gameObject);
        kok = new GameObject("Lekeler").transform;
        kok.SetParent(transform, false);
        lekeler.Clear();

        // Tohum: kat ve dairenin yönü (aynalı daireler aynı katta farklı dursun).
        var rastgele = new System.Random(kat * 131 + Mathf.RoundToInt(eksen.transform.localPosition.x * 10f));
        float yipranma = 0.6f + kat * 0.45f;   // oda başına ortalama leke sayısı: 1. kat ~1, 8. kat ~4

        foreach (DairePlani.Oda oda in DairePlani.Odalar)
        {
            int sayi = Mathf.RoundToInt(yipranma + (float)rastgele.NextDouble() * 1.5f - 0.5f);
            for (int i = 0; i < sayi; i++) OdayaLeke(oda, rastgele);
            // Tavan rutubeti ayrı: yarıdan fazla odada bir köşe (üst katlarda hepsinde).
            if (rastgele.NextDouble() < 0.35f + kat * 0.08f) TavanaRutubet(oda, rastgele);
        }
        KaliteyiUygula();
    }

    // Bir odanın bir duvarına rastgele türde bir leke.
    private void OdayaLeke(DairePlani.Oda oda, System.Random r)
    {
        // Islak odalarda (fayans) kabarma olmaz; kabarmanın yerine su lekesi.
        Tur tur;
        double z = r.NextDouble();
        if (z < 0.30) tur = Tur.Su;
        else if (z < 0.50) tur = Tur.Catlak;
        else if (z < 0.70) tur = oda.Islak ? Tur.Su : Tur.Kabarma;
        else if (z < 0.85) tur = Tur.Is;
        else tur = Tur.Sizinti;

        // Boyut ve yükseklik türe göre (metre).
        float en, boy, y;
        switch (tur)
        {
            case Tur.Su: en = Aralik(r, 0.5f, 1.2f); boy = en * Aralik(r, 0.7f, 1.1f); y = Aralik(r, 1.6f, 2.5f); break;
            case Tur.Catlak: en = Aralik(r, 0.6f, 1.3f); boy = en; y = Aralik(r, 0.8f, 2.3f); break;
            case Tur.Kabarma: en = Aralik(r, 0.4f, 0.9f); boy = en * Aralik(r, 0.8f, 1.2f); y = Aralik(r, 0.3f, 1.6f); break;
            case Tur.Is: en = Aralik(r, 0.4f, 0.8f); boy = en; y = Aralik(r, 0.6f, 1.4f); break;
            default: en = Aralik(r, 0.35f, 0.6f); boy = Aralik(r, 0.8f, 1.4f); y = DairePlani.TavanYuk - boy / 2f - 0.05f; break;
        }
        boy = Mathf.Min(boy, DairePlani.TavanYuk - 0.1f);
        y = Mathf.Clamp(y, boy / 2f + 0.05f, DairePlani.TavanYuk - boy / 2f - 0.05f);

        // Kenar: 0 u0, 1 u1, 2 v0, 3 v1. Kenar boyunca açıklıklara çakışmayan bir yer aranır.
        for (int deneme = 0; deneme < 8; deneme++)
        {
            int kenar = r.Next(4);
            bool uKenari = kenar <= 1;
            float yuz = kenar == 0 ? oda.U0 : kenar == 1 ? oda.U1 : kenar == 2 ? oda.V0 : oda.V1;
            float bas = (uKenari ? oda.V0 : oda.U0) + Pay + en / 2f;
            float son = (uKenari ? oda.V1 : oda.U1) - Pay - en / 2f;
            if (son <= bas) continue;
            float m = Aralik(r, bas, son);
            if (AciklikVar(uKenari, yuz, m - en / 2f, m + en / 2f, y - boy / 2f, y + boy / 2f)) continue;

            // Duvara bakan projeksiyon: kutu odanın içinden duvara doğru bakar; derinliğin yarısı
            // odada, yarısı duvarın içinde (duvar yüzü kutunun tam ortasında).
            float icYon = kenar == 0 || kenar == 2 ? 1f : -1f;   // odanın içi hangi yönde
            Vector3 merkez = uKenari ? eksen.Nokta(yuz + icYon * 0.0f, y, m) : eksen.Nokta(m, y, yuz);
            Vector3 bakis = uKenari ? eksen.Yon(-icYon, 0f) : eksen.Yon(0f, -icYon);   // duvara doğru
            LekeKoy(tur, merkez, Quaternion.LookRotation(bakis, Vector3.up), new Vector3(en, boy, Derinlik), r);
            return;
        }
    }

    // Tavanın bir köşesine rutubet: yukarıdan aşağı bakan projeksiyon.
    private void TavanaRutubet(DairePlani.Oda oda, System.Random r)
    {
        float en = Aralik(r, 0.7f, 1.4f), boy = Aralik(r, 0.7f, 1.4f);
        bool u1 = r.Next(2) == 0, v1 = r.Next(2) == 0;
        float u = u1 ? oda.U1 - en / 2f + 0.15f : oda.U0 + en / 2f - 0.15f;
        float v = v1 ? oda.V1 - boy / 2f + 0.15f : oda.V0 + boy / 2f - 0.15f;
        Vector3 merkez = eksen.Nokta(u, DairePlani.TavanYuk, v);
        // Aşağı bakan kutu; genişlik u, yükseklik v yönünde (kutunun x'i duvar u eksenine paralel).
        Quaternion donus = Quaternion.LookRotation(Vector3.down, eksen.Yon(0f, 1f));
        LekeKoy(Tur.Rutubet, merkez, donus, new Vector3(en, boy, Derinlik), r);
    }

    private void LekeKoy(Tur tur, Vector3 yerelMerkez, Quaternion yerelDonus, Vector3 boyut, System.Random r)
    {
        GameObject g = new GameObject("Leke_" + tur);
        g.transform.SetParent(kok, false);
        g.transform.localPosition = yerelMerkez;
        g.transform.localRotation = yerelDonus;
        DecalProjector p = g.AddComponent<DecalProjector>();
        p.material = malzemeler[(int)tur];
        p.size = boyut;
        p.pivot = Vector3.zero;             // kutu merkezde: yarısı duvarın içinde
        p.drawDistance = 25f;
        p.fadeScale = 0.6f;
        // Her leke biraz farklı dursun: aynalanmış ve hafif döndürülmüş.
        if (tur != Tur.Sizinti && r.Next(2) == 0) g.transform.localRotation *= Quaternion.Euler(0f, 0f, 180f);
        if (tur == Tur.Su || tur == Tur.Rutubet || tur == Tur.Is) g.transform.localRotation *= Quaternion.Euler(0f, 0f, (float)r.NextDouble() * 360f);
        lekeler.Add(p);
    }

    // Bu duvar parçası (kenar boyunca s0..s1, yükseklik y0..y1) bir açıklığa çakışıyor mu?
    private static bool AciklikVar(bool uKenari, float yuz, float s0, float s1, float y0, float y1)
    {
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
        {
            if (a.UDuvari != uKenari || Mathf.Abs(a.Cizgi - yuz) > 0.15f) continue;
            float a0 = a.Merkez - a.Gen / 2f - Pay, a1 = a.Merkez + a.Gen / 2f + Pay;
            if (s1 < a0 || s0 > a1) continue;
            if (y1 < a.Alt - Pay || y0 > a.Ust + Pay) continue;
            return true;
        }
        return false;
    }

    private static float Aralik(System.Random r, float a, float b) { return a + (float)r.NextDouble() * (b - a); }

    private void KaliteyiUygula()
    {
        bool acik = Ayarlar.Kalite != AyarKurallari.KaliteDusuk;
        foreach (DecalProjector p in lekeler) if (p != null) p.enabled = acik;
    }
}
