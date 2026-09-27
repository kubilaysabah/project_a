// Esya.cs
// Zincirde bulunan, elle alınan eşya (#54): oda anahtarı, kat anahtarı, sigorta, tornavida.
// Bakınca "AL (E)"; basınca ele süzülür (UcanKopya), köşede simgesi belirir (EsyaGostergesi).
//
// ELDE TEK EŞYA, DOLUYSA DEĞİŞ TOKUŞ
// Kapıcı elinde tek eşya taşır (Docs/hikaye.md §4). Eli doluyken yeni bir eşya alırsa
// elindeki, yeni eşyanın durduğu yere bırakılır (değiş tokuş). Böylece hiçbir eşya kaybolmaz
// ve oyuncu "önce şunu kullan" diye takılıp kalmaz; bıraktığı yeri de hatırlar.
//
// KABIN İÇİNDEKİ EŞYA
// Eşya bir çekmecenin, sandığın, kavanozun içinde durabilir (zincir verisi söyler). Kabı
// kapalıyken alınamaz: önce çekmece açılmalı. Bu yüzden EtkilesimeAcik, eşyanın üstündeki
// (ebeveyni olan) açılır parçaların hepsinin açık olmasını ister.

using UnityEngine;

public class Esya : MonoBehaviour, IEtkilesimli
{
    [SerializeField] private EsyaTuru tur = EsyaTuru.OdaAnahtari;
    [SerializeField] private string kimlik = "";
    [SerializeField] private int kat;

    private static AudioClip sikirti;
    // Eşyanın durduğu kabın kapağı (#64), zincir biliyorsa: tezgâhın dolabındaki sigorta için
    // "Kapak_1". Verilmişse kardeş kapaklardan yalnızca bu sayılır; tezgâhın öbür kapağı ve
    // çekmeceleri kapalı diye dolaptaki eşya alınamaz olmasın.
    private AcilirParca kap;

    /// <summary>Zincir uygulayıcısı: eşyanın kabının kapağı (yerin kilitlenen parçası).</summary>
    public void KapAyarla(AcilirParca kapak) { kap = kapak; }

    public EsyaTuru Tur { get { return tur; } }
    public string Kimlik { get { return kimlik; } }

    /// <summary>Zincir uygulayıcısı eşyayı yerleştirirken kimliğini ve katını yazar.</summary>
    public void Ayarla(EsyaTuru yeniTur, string yeniKimlik, int yeniKat)
    {
        tur = yeniTur;
        kimlik = yeniKimlik ?? "";
        kat = yeniKat;
    }

    public string Ipucu { get { return Dil.Getir("ipucu.al"); } }

    public bool EtkilesimeAcik
    {
        get
        {
            if (!gameObject.activeInHierarchy) return false;
            // Üstteki kaplardan biri kapalıysa (kapalı çekmece) eşya alınamaz.
            for (Transform t = transform.parent; t != null; t = t.parent)
            {
                AcilirParca kap = t.GetComponent<AcilirParca>();
                if (kap != null && !kap.Acik) return false;
            }
            // Eşyanın doğrudan içinde durduğu kabın kapağı ya da kapısı (kavanozun kapağı,
            // sandığın kapağı, buzdolabının kapısı) kapalıysa da alınamaz. Kapak eşyanın
            // ebeveyni değil kardeşidir; yukarıdaki döngü onu görmez. Bu kural olmasa oyuncu
            // kapak kapalıyken kabın yan duvarının içinden eşyayı alabilirdi.
            if (kap != null)
                return !kap.gameObject.activeSelf || kap.Acik;
            if (transform.parent != null)
                foreach (Transform kardes in transform.parent)
                {
                    AcilirParca kapak = kardes.GetComponent<AcilirParca>();
                    // Kırılan kavanozun kapağı gizlenir (#61): görünmeyen kapak engel değil.
                    if (kapak != null && kardes.gameObject.activeSelf && !kapak.Acik) return false;
                }
            return true;
        }
    }

    public void Etkilesim(GameObject oyuncu)
    {
        // TABAK (#66): ele girmez, kapıcının kucağındaki yığına girer. Sofradaki bir yerden
        // (Yuva) geri alınıyorsa yığının ALTINA (sıradaki tabak üste çıksın), yerde bulunduysa
        // ÜSTÜNE konur ve kat kaydına yazılır.
        if (tur == EsyaTuru.Tabak)
        {
            bool sofradan = transform.parent != null && transform.parent.GetComponent<Yuva>() != null;
            SikirtiCal();
            if (sofradan) TabakYigini.AltaKoy(gameObject);
            else
            {
                Kayit.ParcaBulundu(kat, kimlik);
                UcanKopya.ElineUcur(transform);
                TabakYigini.UsteKoy(gameObject);
            }
            return;
        }

        // KIRIK ANAHTAR PARÇASI (#65): ele girmez; kat kaydına yazılır, köşedeki sayaç artar.
        // Parçaların hepsi bulununca kilidi (KirikAnahtar) kendiliğinden "AÇ" der.
        if (tur == EsyaTuru.AnahtarParcasi)
        {
            Kayit.ParcaBulundu(kat, kimlik);
            UcanKopya.ElineUcur(transform);
            SikirtiCal();
            gameObject.SetActive(false);
            return;
        }

        // ÇİZİM PARÇASI (#63): ele de çantaya da girmez; katın çizim ekranı onu kaydeder ve
        // o ana kadar birleşen resmi yakından gösterir. Ekran dairenin kökündedir.
        if (tur == EsyaTuru.Parca)
        {
            CizimEkrani ekran = CizimEkraniniBul();
            SikirtiCal();
            gameObject.SetActive(false);
            if (ekran != null) ekran.ParcaAlindi(kimlik, oyuncu);
            else Debug.LogWarning($"[CIZIM] '{kimlik}' parçasının çizim ekranı bulunamadı.");
            return;
        }


        // ALET (#61): tornavida gibi aletler ele değil kapıcının çantasına gider; eli meşgul
        // etmez, bitmez, sonraki katlarda da işe yarar (Şahan'ın kararı).
        if (AletCantasi.AletMi(tur))
        {
            Aletler.Ekle(kimlik);
            UcanKopya.ElineUcur(transform);
            SikirtiCal();
            gameObject.SetActive(false);
            return;
        }

        // El doluysa: elindekini buraya bırak (aynı ebeveyn, aynı yer ve duruş).
        if (!Cep.ElBos)
        {
            GameObject eldeki = Cep.TasinanNesne;
            if (eldeki != null && eldeki != gameObject)
            {
                eldeki.transform.SetParent(transform.parent, false);
                eldeki.transform.localPosition = transform.localPosition;
                eldeki.transform.localRotation = transform.localRotation;
                eldeki.SetActive(true);
            }
            else if (eldeki == null)
            {
                // Elindekinin sahnede karşılığı yok (kat anahtarı sahanlıktan alındıysa):
                // değiş tokuş yapılamaz, eşya alınmaz. Oyuncu önce elindekini kullanmalı.
                return;
            }
            Cep.Bosalt();
        }

        if (!Cep.Al(tur, kat, kimlik)) return;
        Cep.TasinanNesne = gameObject;
        UcanKopya.ElineUcur(transform);
        SikirtiCal();
        gameObject.SetActive(false);
    }

    // Parçanın çizim ekranı: parça dairenin içindeyse yukarı doğru (ebeveynlerinde) bulunur;
    // sahanlıktaki ayakkabılıkta duruyorsa daireye bağlı değildir, o zaman sahnedeki çizim
    // ekranlarından bu parçayı tanıyan alınır (aynı anda yalnızca açık dairede bir tane olur).
    private CizimEkrani CizimEkraniniBul()
    {
        CizimEkrani ekran = GetComponentInParent<CizimEkrani>();
        if (ekran != null) return ekran;
        foreach (CizimEkrani c in FindObjectsByType<CizimEkrani>())
            if (c.ParcasiMi(kimlik)) return c;
        return null;
    }

    private void SikirtiCal()
    {
        if (sikirti == null) sikirti = Resources.Load<AudioClip>("Sesler/Anahtar_Al");
        if (sikirti != null) AudioSource.PlayClipAtPoint(sikirti, transform.position, 0.6f);
    }
}
