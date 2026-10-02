// AileFotografi.cs
// Dairedeki çerçeveli aile fotoğrafı (#69). Bakınca "BAK (E)"; basınca yakın bakış açılır
// (FotografEkrani): önce fotoğraf, E ile çevrilince arkasındaki el yazısı. Bakılan fotoğraf
// koleksiyona girer (Kayit) ve menüdeki Fotoğraflar sayfasında yeniden açılabilir.
//
// Çerçeve bir Poly Haven modeli (#73); resmin durduğu parçası "Resim" adlı çocuk. Hangi fotoğrafın
// takılacağını kat dosyası söyler (KatSusu.Metin = fotoğrafın kimliği); ZincirUygulayici kopyayı
// koyunca Ayarla(kimlik) çağırır.
//
// MATERIAL PROPERTY BLOCK: çizicinin malzemesini değiştirmeden, yalnızca bu çizici için bir ayarı
// (burada resim dokusunu) ezmenin yolu. Her fotoğraf için ayrı bir malzeme dosyası gerekmez;
// renderer.material'a yazmak ise malzemeyi sessizce kopyalardı (bkz. CLAUDE.md, #5).

using UnityEngine;

public class AileFotografi : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private FotografAlbumu album;
    [SerializeField] private string kimlik = "";

    public string Kimlik { get { return kimlik; } }

    // MaterialPropertyBlock sahneye kaydedilmez: sahnede kurulmuş bir çerçeve (kapıcı odası) fotoğrafını
    // oyun başlarken kimliğinden yeniden takar.
    private void Start()
    {
        if (!string.IsNullOrEmpty(kimlik)) Ayarla(kimlik);
    }

    /// <summary>Çerçeveye albümdeki fotoğrafı takar.</summary>
    public void Ayarla(string yeniKimlik)
    {
        kimlik = yeniKimlik ?? "";
        FotografAlbumu.Fotograf f = album != null ? album.Bul(kimlik) : null;
        Transform resim = transform.Find("Govde/Resim");
        if (resim == null) resim = transform.Find("Resim");
        Renderer r = resim != null ? resim.GetComponent<Renderer>() : null;
        if (f == null || r == null)
        {
            Debug.LogWarning($"[FOTO] '{name}': '{kimlik}' fotoğrafı ya da Resim parçası bulunamadı.");
            return;
        }
        var blok = new MaterialPropertyBlock();
        r.GetPropertyBlock(blok);
        blok.SetTexture("_BaseMap", f.Doku);
        r.SetPropertyBlock(blok);
    }

    public string Ipucu { get { return Dil.Getir("ipucu.bak"); } }
    public NisanSimgesi Simge { get { return NisanSimgesi.Goz; } }   // #104

    public bool EtkilesimeAcik { get { return album != null && album.Bul(kimlik) != null && !FotografEkrani.Acik; } }

    public void Etkilesim(GameObject oyuncu)
    {
        FotografAlbumu.Fotograf f = album.Bul(kimlik);
        if (f == null) return;
        Kayit.FotografBulundu(kimlik);
        FotografEkrani.Ac(f, oyuncu);
    }
}
