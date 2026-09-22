// AnomaliUygulayici.cs
// Bir anomali kartını sahanlığa uygular ve İSTENDİĞİNDE EKSİKSİZ GERİ ALIR.
//
// ---------- NEDEN GERİ ALMA BU KADAR ÖNEMLİ ----------
// Sahnede tek bir sahanlık var ve onu her katta yeniden kullanıyoruz.
// Bir kart sahanlığı bozduğunda, sonraki kata geçerken bozulmanın TAMAMEN
// geri alınması şart. Tek bir değişiklik geri alınmazsa o anomali kalıcı olur,
// sonraki katta da görünür, oyuncu doğru kararı verdiği hâlde ceza yer ve
// oyun sessizce bozulur. Üstelik bu tür hatalar çok geç fark edilir.
//
// Çözüm: her değişiklikten ÖNCE nesnenin eski hâlini bir deftere yazıyoruz.
// Geri alırken deftere bakıp aynen eski hâline döndürüyoruz.
//
// Bu sınıf MonoBehaviour değil. Sahnede durmasına gerek yok; KatYoneticisi
// her sahanlık için birer tane üretip elinde tutacak.

using System.Collections.Generic;
using UnityEngine;

public class AnomaliUygulayici
{
    // ---------- DEFTER ----------
    // Yapılan her değişiklik için bir satır. Geri alırken bu satırlar okunur.
    // "private class": sadece bu dosyanın içinden görünen küçük yardımcı tip.
    private class Kayit
    {
        public GameObject nesne;
        public EtkiTuru tur;

        public bool eskiAktif;
        public Vector3 eskiKonum;
        public Quaternion eskiDonus;
        public Vector3 eskiOlcek;

        public Renderer ciziciler;       // renk değişiminde kullanılan çizici
        public Color eskiRenk;
        public float eskiSiddet;
        public string eskiMetin;
    }

    // List<T>: boyutu değişebilen dizi. Kaç değişiklik yapılacağını önceden
    // bilmediğimiz için normal dizi yerine bunu kullanıyoruz.
    private readonly List<Kayit> defter = new List<Kayit>();

    /// <summary>Şu an uygulanmış olan kart. Hiçbiri yoksa null.</summary>
    public AnomaliKarti UygulananKart { get; private set; }

    /// <summary>Şu an bir anomali uygulanmış durumda mı?</summary>
    public bool AnomaliVar { get { return UygulananKart != null; } }

    // URP'nin standart malzemesinde taban rengin adı "_BaseColor".
    // Shader.PropertyToID, metin aramayı sayıya çevirir; her karede metinle
    // arama yapmak pahalıdır, bu yüzden bir kez hesaplayıp saklıyoruz.
    private static readonly int TabanRenkKimligi = Shader.PropertyToID("_BaseColor");
    private static readonly int EskiRenkKimligi = Shader.PropertyToID("_Color");

    // ---------- UYGULA ----------

    /// <summary>
    /// Kartı verilen sahanlığa uygular. Önceki kart varsa önce geri alınır.
    /// </summary>
    public void Uygula(AnomaliKarti kart, Transform sahanlik)
    {
        // Üst üste uygulama olmasın: her zaman temiz bir sahanlıkla başla.
        GeriAl();

        if (kart == null || sahanlik == null) return;

        if (!kart.Gecerli)
        {
            Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}' kartının hiç etkisi yok, atlandı.");
            return;
        }

        foreach (AnomaliEtkisi etki in kart.Etkiler)
        {
            EtkiUygula(kart, etki, sahanlik);
        }

        UygulananKart = kart;
    }

    private void EtkiUygula(AnomaliKarti kart, AnomaliEtkisi etki, Transform sahanlik)
    {
        // Transform.Find eğik çizgili yolu anlar: "Kapi_Sol/Kanat" gibi.
        Transform hedef = sahanlik.Find(etki.hedefYol);

        if (hedef == null)
        {
            // Yol yanlış yazılmışsa sessizce geçmek en kötüsü olur: anomali
            // hiç görünmez ve neden görünmediği anlaşılmaz. Yüksek sesle söylüyoruz.
            Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}' kartı '{etki.hedefYol}' " +
                             $"yolunu bulamadı. Sahanlık: {sahanlik.name}");
            return;
        }

        // Eski hâli deftere yaz. Bunu DEĞİŞİKLİKTEN ÖNCE yapmak zorundayız.
        Kayit kayit = new Kayit
        {
            nesne = hedef.gameObject,
            tur = etki.tur,
            eskiAktif = hedef.gameObject.activeSelf,
            eskiKonum = hedef.localPosition,
            eskiDonus = hedef.localRotation,
            eskiOlcek = hedef.localScale
        };

        switch (etki.tur)
        {
            case EtkiTuru.Gorunurluk:
                hedef.gameObject.SetActive(etki.gorunur);
                break;

            case EtkiTuru.Konum:
                hedef.localPosition += etki.vektor;
                break;

            case EtkiTuru.Donus:
                // Quaternion çarpımı iki dönüşü birleştirir: mevcut dönüşün
                // ÜSTÜNE kartın açısını ekler.
                hedef.localRotation *= Quaternion.Euler(etki.vektor);
                break;

            case EtkiTuru.Olcek:
                // Vektör çarpan olarak kullanılıyor. Sıfır gelirse nesne yok olur
                // ve bu neredeyse her zaman yazım hatasıdır; uyarıp atlıyoruz.
                if (etki.vektor == Vector3.zero)
                {
                    Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}' kartında ölçek çarpanı " +
                                     $"sıfır. Gizlemek için Gorunurluk türünü kullanın.");
                    return;
                }
                hedef.localScale = Vector3.Scale(hedef.localScale, etki.vektor);
                break;

            case EtkiTuru.Renk:
                if (!RenkUygula(kart, hedef, etki.renk, kayit)) return;
                break;

            case EtkiTuru.Isik:
                Light isik = hedef.GetComponent<Light>();
                if (isik == null)
                {
                    Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}': '{etki.hedefYol}' " +
                                     $"üzerinde ışık yok.");
                    return;
                }
                kayit.eskiRenk = isik.color;
                kayit.eskiSiddet = isik.intensity;
                isik.color = etki.renk;
                isik.intensity = etki.sayi;
                break;

            case EtkiTuru.Metin:
                TextMesh yazi = hedef.GetComponent<TextMesh>();
                if (yazi == null)
                {
                    Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}': '{etki.hedefYol}' " +
                                     $"üzerinde yazı yok.");
                    return;
                }
                kayit.eskiMetin = yazi.text;
                yazi.text = etki.metin;
                break;

            case EtkiTuru.Ses:
                // Ses anlık çalar ve kendiliğinden biter; geri alınacak bir şey yok.
                // Sürekli çalan ambiyans anomalileri ses tasarımıyla (#14) gelecek.
                if (etki.ses != null)
                {
                    AudioSource.PlayClipAtPoint(etki.ses, hedef.position);
                }
                return;   // deftere yazmaya gerek yok
        }

        defter.Add(kayit);
    }

    // ---------- RENK ----------
    // Malzemenin rengini doğrudan değiştirmek TUZAKTIR: Unity o anda
    // malzemenin bir KOPYASINI üretir, kopya sahnede kalır ve bellek sızar.
    // Doğru yol MaterialPropertyBlock: malzemeye dokunmadan, sadece o çizici
    // için geçici bir renk değeri verir. Geri almak da bloğu temizlemek kadar kolay.
    private bool RenkUygula(AnomaliKarti kart, Transform hedef, Color renk, Kayit kayit)
    {
        Renderer cizici = hedef.GetComponent<Renderer>();

        if (cizici == null)
        {
            Debug.LogWarning($"[ANOMALİ] '{kart.Kimlik}': '{hedef.name}' üzerinde çizici yok.");
            return false;
        }

        kayit.ciziciler = cizici;

        // Eski rengi malzemeden okuyoruz (kopya üretmeyen sharedMaterial ile).
        Material malzeme = cizici.sharedMaterial;
        if (malzeme != null && malzeme.HasProperty(TabanRenkKimligi))
            kayit.eskiRenk = malzeme.GetColor(TabanRenkKimligi);
        else if (malzeme != null && malzeme.HasProperty(EskiRenkKimligi))
            kayit.eskiRenk = malzeme.GetColor(EskiRenkKimligi);
        else
            kayit.eskiRenk = Color.white;

        MaterialPropertyBlock blok = new MaterialPropertyBlock();
        cizici.GetPropertyBlock(blok);
        blok.SetColor(TabanRenkKimligi, renk);
        blok.SetColor(EskiRenkKimligi, renk);
        cizici.SetPropertyBlock(blok);

        return true;
    }

    // ---------- GERİ AL ----------

    /// <summary>Uygulanmış bütün değişiklikleri eski hâline döndürür.</summary>
    public void GeriAl()
    {
        // Defteri TERSTEN okuyoruz. İki etki aynı nesneye dokunmuşsa,
        // son yapılan ilk geri alınmalı; yoksa ara bir hâl kalıcı olur.
        for (int i = defter.Count - 1; i >= 0; i--)
        {
            Kayit k = defter[i];

            // Nesne bu arada silinmiş olabilir; Unity'de yok olmuş nesne
            // null gibi davranır, o yüzden kontrol ediyoruz.
            if (k.nesne == null) continue;

            Transform t = k.nesne.transform;

            switch (k.tur)
            {
                case EtkiTuru.Gorunurluk:
                    k.nesne.SetActive(k.eskiAktif);
                    break;

                case EtkiTuru.Konum:
                    t.localPosition = k.eskiKonum;
                    break;

                case EtkiTuru.Donus:
                    t.localRotation = k.eskiDonus;
                    break;

                case EtkiTuru.Olcek:
                    t.localScale = k.eskiOlcek;
                    break;

                case EtkiTuru.Renk:
                    if (k.ciziciler != null)
                    {
                        // Bloğu temizlemek, çiziciyi malzemenin kendi rengine döndürür.
                        k.ciziciler.SetPropertyBlock(null);
                    }
                    break;

                case EtkiTuru.Isik:
                    Light isik = k.nesne.GetComponent<Light>();
                    if (isik != null)
                    {
                        isik.color = k.eskiRenk;
                        isik.intensity = k.eskiSiddet;
                    }
                    break;

                case EtkiTuru.Metin:
                    TextMesh yazi = k.nesne.GetComponent<TextMesh>();
                    if (yazi != null) yazi.text = k.eskiMetin;
                    break;
            }
        }

        defter.Clear();
        UygulananKart = null;
    }
}
