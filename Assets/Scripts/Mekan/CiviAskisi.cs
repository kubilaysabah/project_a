// CiviAskisi.cs
// Dokuzuncu katın holündeki BOŞ çivi (#108, hikaye.md §16): oyuncu Cemal'in kafa lambasını buraya asar.
//
// NEDEN
// Oyun kapıcı odasındaki çividen lambayı almakla başladı (CividekiLamba, "AL"). Dokuzuncu katta aynı
// hareketin tersi var: lamba çiviye asılır ("AS"). Rıza Efendi'nin tespihi ve Hasan'ın kasketi yan
// çivilerde asılı; çıkıp gidenler bir şey bırakmış. Üçüncü çivi boş: binanın beklediği şey lambadır.
// Bu bir kapı anıdır, bulmaca değil: holde etkileşilebilen tek şey bu çividir.
//
// NASIL
// Etkileşim: oyuncunun lambası kafasından iner (KafaLambasi.Birak: ışık söner, L tuşu bir şey yapmaz),
// çivideki lamba modeli yanar hâlde görünür (kapıcı odasındaki Cividen_Lamba'nın aynısı: GrayboxKurucu
// kurar, "asiliLamba" alanına bağlar) ve Asildi olayı DokuzuncuKat'a haber verir; o da salon kapısını
// açar. Asılan lamba geri ALINAMAZ: bina onu tutar (final: pencerede yanan ışık bu lambadır).
// Sifirla(): yakalanma sonrası bölüm başa dönerken lamba çividen kalkar, oyuncuya geri verilir.

using UnityEngine;

public class CiviAskisi : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private GameObject asiliLamba;   // çividen sarkan yanık lamba (model + ışık), başta kapalı

    private bool asili;
    private KafaLambasi oyuncununLambasi;

    /// <summary>Lamba asıldığında bir kez haber verir (DokuzuncuKat salon kapısını açar).</summary>
    public event System.Action Asildi;

    public bool Asili { get { return asili; } }

    public string Ipucu { get { return Dil.Getir("ipucu.as"); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.El; } }

    // Yalnızca lamba oyuncunun kafasındayken ve çivi boşken: asılmış lamba geri alınamaz.
    public bool EtkilesimeAcik { get { return !asili && Lamba() != null && Lamba().Takili; } }

    public void Etkilesim(GameObject oyuncu)
    {
        if (!EtkilesimeAcik) return;
        KafaLambasi lamba = oyuncu != null ? oyuncu.GetComponentInChildren<KafaLambasi>(true) : null;
        if (lamba == null) lamba = Lamba();
        if (lamba == null) return;
        lamba.Birak();                                   // kafadan iner: ışık söner, tuş bir şey yapmaz
        asili = true;
        if (asiliLamba != null) asiliLamba.SetActive(true);   // çivide yanar hâlde görünür
        Debug.Log("[DOKUZ] Cemal'in lambası boş çiviye asıldı.");
        Asildi?.Invoke();
    }

    /// <summary>Bölüm başa dönerken: lamba çividen kalkar ve oyuncunun kafasına geri takılır.</summary>
    public void Sifirla()
    {
        asili = false;
        if (asiliLamba != null) asiliLamba.SetActive(false);
        KafaLambasi lamba = Lamba();
        if (lamba != null && !lamba.Takili) lamba.Tak();
    }

    private KafaLambasi Lamba()
    {
        if (oyuncununLambasi == null) oyuncununLambasi = FindAnyObjectByType<KafaLambasi>(FindObjectsInactive.Include);
        return oyuncununLambasi;
    }
}
