// KatOlayCalici.cs
// Alt sahanlıkta (A) o katın "kat olayı" sesini planlanan anlarda çalar (#35).
//
// Hangi olayın ve ne zaman çalacağını KatOlayPlani (düz C#, test edilen kural) söyler;
// bu sınıf yalnızca sahnedeki ses kaynaklarını bulur ve saati gelince çalar.
// MonoBehaviour değil: kendi Update'i yok. Saatini KatYoneticisi ilerletir, tıpkı
// anomali kartlarının zamanlı etkileri gibi (AnomaliUygulayici.Ilerlet). Böylece
// olay yalnızca oyuncunun BULUNDUĞU katta çalar; merdivenden bakarken üst sahanlıkta
// kendiliğinden bir ses çıkmaz.
//
// Kaynakların yerleri (sahanlığa göre yol):
//   Bebek            Kapi_4/Ses_Bebek              (4 numaranın içi; bebek kartları da bunu hedefliyor)
//   UzakKapi ...     Ses_Ugultu/Olay_...           (uğultunun çocukları)
// Uzak olayların uğultunun ÇOCUĞU olmasının sebebi: "sessizlik" ve "karanlik_kat"
// kartları Ses_Ugultu'yu gizliyor. Çocukları da onunla birlikte gizlenir; ölü bir katta
// uzaktan bir kapı sesi gelmez. Kartlara dokunmak gerekmedi.

using UnityEngine;

public class KatOlayCalici
{
    // Enum sırasıyla aynı sırada: KatOlayi.Bebek = 0, UzakKapi = 1, ...
    // Yeni bir olay eklenirse hem enum'a hem buraya eklenir (test ikisini de kontrol ediyor).
    public static readonly string[] KaynakYollari =
    {
        "Kapi_4/Ses_Bebek",
        "Ses_Ugultu/Olay_UzakKapi",
        "Ses_Ugultu/Olay_UstKatAdimlari",
        "Ses_Ugultu/Olay_Kalorifer",
        "Ses_Ugultu/Olay_Araba",
    };

    private readonly AudioSource[] kaynaklar = new AudioSource[KaynakYollari.Length];

    // Şu anki katın planı.
    private AudioSource secili;      // bu katta çalacak kaynak; null = bu katta olay yok
    private float[] zamanlar;        // çalış anları (kata varıştan saniye)
    private int sonraki;             // sıradaki çalışın dizideki yeri
    private float saat;              // kata varıştan beri geçen süre

    public KatOlayCalici(Transform sahanlik)
    {
        if (sahanlik == null) return;
        for (int i = 0; i < KaynakYollari.Length; i++)
        {
            Transform t = sahanlik.Find(KaynakYollari[i]);
            if (t != null) kaynaklar[i] = t.GetComponent<AudioSource>();
            if (kaynaklar[i] == null) Debug.LogWarning($"[OLAY] '{KaynakYollari[i]}' ses kaynağı bulunamadı; bu olay sessiz kalacak.");
        }
    }

    /// <summary>
    /// Sahanlık yeni bir katı göstermeye başladığında çağrılır. Önceki katın sesi susar,
    /// saat sıfırlanır. olayVar false ise (sesli anomali katı, çatı...) bu katta olay çalmaz.
    /// </summary>
    public void Hazirla(bool olayVar, KatOlayi olay, float[] yeniZamanlar)
    {
        // Eski katın sesi yarıda kalsın: oyuncu artık başka bir kattadır.
        foreach (AudioSource k in kaynaklar) if (k != null) k.Stop();

        secili = olayVar ? kaynaklar[(int)olay] : null;
        zamanlar = yeniZamanlar;
        sonraki = 0;
        saat = 0f;
    }

    /// <summary>Oyuncu bu kattayken her kare çağrılır; saati gelen çalışı başlatır.</summary>
    public void Ilerlet(float gecen)
    {
        if (secili == null || zamanlar == null) return;
        saat += gecen;

        while (sonraki < zamanlar.Length && saat >= zamanlar[sonraki])
        {
            sonraki++;
            // Bir kart kaynağı gizlemişse (sessizlik, bebek kartları) çalmaya çalışmayız:
            // kapalı bir AudioSource'a Play demek Console'a uyarı yazar.
            if (!secili.isActiveAndEnabled) continue;
            secili.Play();
            Debug.Log($"[OLAY] {secili.name} çaldı ({saat:0.0} sn)");
        }
    }
}
