// Yuva.cs
// Bir eşyanın konacağı yer (#66, 6. kat: sıralama / yerleştirme).
//
// 6. katta sofranın dört yeri var: her birinin önünde bir isim kartı (Rıza Efendi, Hasan, Cemal,
// Yeni Kapıcı). Oyuncu tabakları tek tek taşır (elde tek eşya kuralı) ve bir yere bakınca
// "KOY (E)" der; tabak o yere oturur. Konan tabak yine alınabilir (yanlış yere konduysa geri
// almak için); eli doluyken alınırsa elindeki tabak onun yerine geçer (değiş tokuş, Esya'daki kural).
//
// Dört yerin hepsinde DOĞRU tabak (isim kartıyla aynı isimli) durunca bu yerleri kilitleyen
// kilit (KilitTuru.Yerlestirme) çözülür ve kilitli parça (büfenin kapısı) açılır. Tabaklar artık
// yerinde kalır. Yanlış dizilişte hiçbir şey olmaz; oyuncu isimleri okuyup düzeltir.
//
// Bu bileşeni zincir uygulayıcısı yerlerin nesnelerine ekler (sofranın Yer_1..4 parçaları);
// kat değişince söker. Yerin neyi beklediği ve hangi kilide bağlı olduğu veridedir.

using System.Collections.Generic;
using UnityEngine;

public class Yuva : MonoBehaviour, IEtkilesimli
{
    private string beklenen;             // bu yere konması gereken eşyanın kimliği
    private Kilit kilit;                 // yerlerin hepsi doğru olunca çözülecek kilit
    private List<Yuva> grup;             // aynı kilide bağlı bütün yerler (bu da içinde)
    private static AudioClip koymaSesi;

    public void Hazirla(string beklenenKimlik, Kilit bagliKilit, List<Yuva> hepsi)
    {
        beklenen = beklenenKimlik;
        kilit = bagliKilit;
        grup = hepsi;
        if (koymaSesi == null) koymaSesi = Resources.Load<AudioClip>("Sesler/Anahtar_Al");
    }

    /// <summary>Bu yerde şu an duran eşya (etkin bir çocuk Esya); yoksa null.</summary>
    public Esya Oturan
    {
        get
        {
            foreach (Transform c in transform)
            {
                Esya e = c.GetComponent<Esya>();
                if (e != null && c.gameObject.activeSelf) return e;
            }
            return null;
        }
    }

    /// <summary>Doğru eşya bu yerde mi?</summary>
    public bool Dogru { get { Esya e = Oturan; return e != null && e.Kimlik == beklenen; } }

    public string Ipucu { get { return Dil.Getir("ipucu.koy"); } }

    // Yalnızca boş bir yere, elde bir tabak varken ve kilit henüz çözülmemişken konabilir.
    public bool EtkilesimeAcik
    {
        get { return Oturan == null && Cep.Tasinan == EsyaTuru.Tabak && Cep.TasinanNesne != null && kilit != null && !kilit.Cozuk; }
    }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        GameObject tabak = Cep.TasinanNesne;
        Cep.Bosalt();
        // Tabak bu yerin çocuğu olur ve tam ortasına oturur (yerin kendi eksenleri masaya paraleldir).
        tabak.transform.SetParent(transform, false);
        tabak.transform.localPosition = Vector3.zero;
        tabak.transform.localRotation = Quaternion.identity;
        tabak.SetActive(true);
        if (koymaSesi != null) AudioSource.PlayClipAtPoint(koymaSesi, transform.position, 0.5f);
        Denetle();
    }

    // Tabaklar değiş tokuşla da yer değiştirebilir (Esya); bu yüzden her kare bakılır. Dört yer,
    // birkaç karşılaştırma: ucuz.
    private void Update()
    {
        if (kilit != null && !kilit.Cozuk && grup != null && grup.Count > 0 && grup[0] == this) Denetle();
    }

    private void Denetle()
    {
        if (kilit == null || kilit.Cozuk || grup == null) return;
        foreach (Yuva y in grup) if (y == null || !y.Dogru) return;
        // Hepsi doğru: tabaklar yerinde kalsın diye alınamaz yapılır, kilit çözülür (büfe açılır).
        foreach (Yuva y in grup) Sabitle(y.Oturan);
        Debug.Log($"[YUVA] Sofra kuruldu: '{kilit.Veri.Kimlik}' çözülüyor.");
        kilit.Coz(acilsin: true);
    }

    /// <summary>Kayıttan: kilit çözülmüşse eşya bu yere oturtulur ve alınamaz yapılır.</summary>
    public void YerlesmisBaslat(GameObject esya)
    {
        if (esya == null) return;
        esya.transform.SetParent(transform, false);
        esya.transform.localPosition = Vector3.zero;
        esya.transform.localRotation = Quaternion.identity;
        esya.SetActive(true);
        Sabitle(esya.GetComponent<Esya>());
    }

    public string Beklenen { get { return beklenen; } }

    // Eşyanın "alınabilir" bileşenini söker: model yerinde kalır, artık etkileşim yok.
    private static void Sabitle(Esya e)
    {
        if (e != null) Object.Destroy(e);
    }
}
