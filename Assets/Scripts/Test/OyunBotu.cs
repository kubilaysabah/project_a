// OyunBotu.cs
// Baştan sona oynanış testi için kendi kendine oynayan bot (gece çalışması, 2026-09-27).
// YALNIZCA EDITOR'DE derlenir (#if UNITY_EDITOR): oyunun build'ine girmez.
//
// Ne yapar?
//   Başlık ekranından sonra kapıcı odasından başlar, merdivenden 1. kata yürür. Her katta:
//     1. Katın kilit zincirini kuralın bulduğu sırayla çözer (KilitZinciri.Cozumle). Her kilidi
//        türüne göre GERÇEK yoldan açmaya çalışır: eşyayı alır (Esya.Etkilesim), anahtarı kapıda
//        kullanır (Kilit.Etkilesim), kolu çeker, sigortayı takar, vidaları söker, tabakları dizer,
//        asansörü çağırır. Şifreli kilitlerde şifre ekranı yerine kilidi doğrudan çözer.
//     2. Katın anahtarını alır, sahanlığın başına geçer, merdivenden yürüyerek kat kapısına
//        gider, anahtarı takar ve bir üst kata çıkar.
//   8. katta çatı anahtarıyla çatı kapısını açar ve çatıya yürür (final başlar).
//   Gerçek yoldan olmayan her şey (zorla çözülen kilit, takılınca ışınlanma, alınamayan eşya)
//   "[BOT] SORUN" diye Console'a yazılır; sonunda bir özet basılır.
//
// Başlatmak için (oyun modunda, başlık ekranı geçildikten sonra): OyunBotu.Baslat();

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class OyunBotu : MonoBehaviour
{
    public static OyunBotu Ornek { get; private set; }
    public readonly List<string> Sorunlar = new List<string>();
    public bool Bitti { get; private set; }
    public string Durum { get; private set; } = "başlıyor";

    private const float Hiz = 1.8f;              // oyuncunun yürüme hızı
    private BirinciSahisKontrolcu kontrol;
    private CharacterController govde;
    private KatYoneticisi yonetici;
    private const BindingFlags Gizli = BindingFlags.NonPublic | BindingFlags.Instance;

    public static OyunBotu Baslat()
    {
        if (Ornek != null) Destroy(Ornek.gameObject);
        Varlik.Kapali = true;   // bot ışınlanır ve eşyaları doğrudan çağırır; Varlık (#86) işlemesin
        GameObject g = new GameObject("Oyun_Botu");
        Ornek = g.AddComponent<OyunBotu>();
        return Ornek;
    }

    // İNİŞ KİPİ (#56): oyunu oynamaz; yalnızca merdivenden "katSayisi" kat yürüyerek iner (gerçek alt
    // katlar ve zemin kat sınaması). Zemine varırsa kapıcı odasına girer. Her katta bulunulan katı yazar.
    private int inisKatSayisi = -1;

    public static OyunBotu Inis(int katSayisi)
    {
        OyunBotu b = Baslat();
        b.inisKatSayisi = katSayisi;
        return b;
    }

    // Bir kat aşağı: varış kolundan (x = -1,1) ara sahanlığa iner, dönüp alt katın çıkış kolundan
    // (x = +1,1) aşağıdaki sahanlığa varır. Yukarı çıkışın (Merdivenden) tam tersi.
    private IEnumerator BirKatIn()
    {
        float kol = MekanOlculeri.KolUzunluk;
        yield return Yuru(new[] { new Vector3(MekanOlculeri.VarisKoluX, 0, -0.4f),
                                  new Vector3(MekanOlculeri.VarisKoluX, 0, kol + 0.4f),
                                  new Vector3(0f, 0, kol + 0.75f),
                                  new Vector3(MekanOlculeri.CikisKoluX, 0, kol + 0.5f),
                                  new Vector3(MekanOlculeri.CikisKoluX, 0, -0.3f),
                                  new Vector3(MekanOlculeri.CikisKoluX - 0.6f, 0, -0.9f) }, "iniş");   // sahanlığın içine: son basamak da inilsin
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator InisKipi()
    {
        Yaz($"iniş kipi: {inisKatSayisi} kat inecek; şimdi {yonetici.BulunulanKat}. kat");
        for (int i = 0; i < inisKatSayisi; i++)
        {
            int once = yonetici.BulunulanKat;
            yield return BirKatIn();
            Yaz($"iniş: {once}. kattan inildi, şimdi {yonetici.BulunulanKat}. kat (y {kontrol.transform.position.y:0.00})");
            if (yonetici.BulunulanKat != once - 1) Sorun($"{once}. kattan inince bulunulan kat {yonetici.BulunulanKat} (beklenen {once - 1})");
        }
        if (yonetici.BulunulanKat == 0)
        {
            yield return Yuru(new[] { new Vector3(1.1f, 0, -0.3f), new Vector3(-3.4f, 0, -1.45f) }, "sahanlık -> kapıcı odası");
            Yaz($"kapıcı odasında (x {kontrol.transform.position.x:0.00}, y {kontrol.transform.position.y:0.00})");
        }
        // Geri çık: indiği kadar kat yürüyerek yukarı (alt katların kapıları açık olmalı).
        if (yonetici.BulunulanKat == 0)
            yield return Yuru(new[] { new Vector3(-3.4f, 0, -1.45f), new Vector3(1.1f, 0, -0.3f) }, "kapıcı odası -> sahanlık");
        for (int i = 0; i < inisKatSayisi; i++)
        {
            int once = yonetici.BulunulanKat;
            yield return Merdivenden(null);
            yield return new WaitForSeconds(0.6f);
            Yaz($"çıkış: {once}. kattan çıkıldı, şimdi {yonetici.BulunulanKat}. kat");
            if (yonetici.BulunulanKat != once + 1) Sorun($"{once}. kattan çıkınca bulunulan kat {yonetici.BulunulanKat} (beklenen {once + 1})");
        }
        Bitti = true;
        Yaz($"İNİŞ BİTTİ. {Sorunlar.Count} sorun.");
    }

    private void Sorun(string s) { Sorunlar.Add(s); Debug.LogWarning("[BOT] SORUN: " + s); }
    private void Yaz(string s) { Durum = s; Debug.Log("[BOT] " + s); }

    private IEnumerator Start()
    {
        kontrol = FindAnyObjectByType<BirinciSahisKontrolcu>();
        govde = kontrol.GetComponent<CharacterController>();
        yonetici = FindAnyObjectByType<KatYoneticisi>();
        yield return new WaitForSeconds(1f);
        if (inisKatSayisi >= 0) { yield return InisKipi(); yield break; }

        // Açılış: kapıcı odasındaysa (zemin kat, y < -1) merdivenden 1. kata yürü.
        if (kontrol.transform.position.y < -1f)
        {
            // #56: önce çivideki kafa lambası alınır (alınmadan oyuncunun lambası yanmaz).
            CividekiLamba civi = FindAnyObjectByType<CividekiLamba>();
            KafaLambasi kafa = FindAnyObjectByType<KafaLambasi>();
            if (kafa != null && kafa.Takili) Sorun("açılışta kafa lambası zaten takılı (çivide olmalıydı)");
            if (civi == null || !civi.EtkilesimeAcik) Sorun("açılışta çivide alınacak kafa lambası yok");
            else { civi.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(0.5f); }
            if (kafa != null && (!kafa.Takili || !kafa.Acik)) Sorun("çividen alınan kafa lambası takılmadı ya da yanmıyor");
            // Merdiven kapısının anahtarı da çivide (0. kat); kapı kilitli olmalı, anahtarla açılır.
            KatAnahtari zeminAnahtari = null;
            foreach (KatAnahtari a in FindObjectsByType<KatAnahtari>()) if (a.Kat == 0 && a.gameObject.activeInHierarchy && a.name == "Zemin_Anahtari") zeminAnahtari = a;
            Transform zk = GameObject.Find("Sahte_Katlar")?.transform.Find("Alt/Cekirdek_Alt1/Kat_Kapisi");
            KatKapisi zeminKapisi = zk != null ? zk.GetComponent<KatKapisi>() : null;
            if (zeminKapisi == null) Sorun("zemin kapısı bulunamadı");
            else if (zeminKapisi.Acik) Sorun("açılışta zemin kapısı kilitli değil");
            if (zeminAnahtari == null) Sorun("çivide merdiven kapısının anahtarı yok");
            else
            {
                zeminAnahtari.Etkilesim(kontrol.gameObject);
                yield return new WaitForSeconds(0.4f);
                if (zeminKapisi != null) { zeminKapisi.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(2.6f); }
                if (zeminKapisi != null && !zeminKapisi.Acik) Sorun("zemin kapısı anahtarla açılmadı");
            }
            Yaz("açılış: kapıcı odasından 1. kata yürüyor");
            yield return Yuru(new[] { new Vector3(-3.4f, 0, -1.45f), new Vector3(1.1f, 0, -0.3f) }, "kapıcı odası -> sahanlık");
            yield return Merdivenden(null);
            yield return new WaitForSeconds(1.5f);
        }

        for (int deneme = 0; deneme < 20 && !Bitti; deneme++)
        {
            int kat = yonetici.MevcutKat;
            Yaz($"{kat}. kat başlıyor");
            yield return KatiCoz(kat);
            if (kat >= KatSayaci.CatiKati) { yield return Dokuz(); yield return Cati(); break; }
            yield return SahanligaGec();
            yield return Merdivenden(kat);
            yield return new WaitForSeconds(2f);
            if (yonetici.MevcutKat != kat + 1) Sorun($"{kat}. kattan çıkıldı ama kat {yonetici.MevcutKat} (beklenen {kat + 1})");
        }
        Bitti = true;
        Yaz($"BİTTİ. {Sorunlar.Count} sorun." + (Sorunlar.Count > 0 ? "\n - " + string.Join("\n - ", Sorunlar) : ""));
    }

    // ---------- KAT ----------
    private KilitZinciri Zincir(int kat, out KatZinciriVarligi varlik)
    {
        var dizi = typeof(KatYoneticisi).GetField("katZincirleri", Gizli).GetValue(yonetici) as KatZinciriVarligi[];
        varlik = dizi != null && kat >= 1 && kat <= dizi.Length ? dizi[kat - 1] : null;
        return varlik != null ? varlik.zincir : null;
    }

    private IEnumerator KatiCoz(int kat)
    {
        KilitZinciri z = Zincir(kat, out KatZinciriVarligi varlik);
        // Yeniden tasarlanan kat: daire boş, katın anahtarı sahanlıkta; bot onu alıp geçer.
        if (varlik != null && varlik.tasarimda)
        {
            KatAnahtari sahanlikta = null;
            foreach (KatAnahtari a in FindObjectsByType<KatAnahtari>())
                if (a.Kat == kat && a.gameObject.activeInHierarchy && a.EtkilesimeAcik) sahanlikta = a;
            if (sahanlikta == null) { Sorun($"{kat}. kat tasarımda ama sahanlıkta kat anahtarı yok"); yield break; }
            sahanlikta.Etkilesim(kontrol.gameObject);
            yield return new WaitForSeconds(0.5f);
            if (!Cep.Tasiyor(EsyaTuru.KatAnahtari, kat)) Sorun($"{kat}. kat: sahanlıktaki kat anahtarı alınamadı");
            yield break;
        }
        if (z == null) { Sorun($"{kat}. katın zinciri yok"); yield break; }

        // 7. kat gibi girişi kapalı katlar: gerçek kapıyı çal.
        if (varlik.girisKapali)
        {
            SahanlikKapisi gercek = null;
            foreach (SahanlikKapisi k in FindObjectsByType<SahanlikKapisi>())
                if ((bool)typeof(SahanlikKapisi).GetField("gercek", Gizli).GetValue(k)) gercek = k;
            if (gercek == null) Sorun($"{kat}. kat: gerçek kapı bulunamadı");
            else { gercek.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(3f); }
        }

        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(o.Ad);
        // Çift daireli katta (#70) ikinci dairenin odaları "B:" önekiyle yazılır.
        if (varlik.ikinciDaire > 0) foreach (DairePlani.Oda o in DairePlani.Odalar) odalar.Add(KilitZinciri.IkinciOnek + o.Ad);
        KilitZinciri.Sonuc s = z.Cozumle(odalar);
        if (!s.Cozulebilir) Sorun($"{kat}. kat zinciri kurala göre çözülemiyor: {string.Join("; ", s.Hatalar)}");

        foreach (string kimlik in s.AcilanKilitler)
        {
            ZincirKilidi veri = z.Kilitler.Find(k => k.Kimlik == kimlik);
            if (veri == null) continue;
            yield return KilidiAc(kat, z, veri);
        }

        // Katın anahtarı.
        ZincirEsyasi anahtar = z.Esyalar.Find(e => e.Tur == EsyaTuru.KatAnahtari);
        if (anahtar != null) yield return Al(kat, anahtar.Kimlik);
        // Asansörle biten zincirde (8. kat, #108) anahtar bu katta değil: dokuzuncu katta alınır (Dokuz()).
        if (anahtar == null && z.AsansorKilidi() != null) { }
        else if (!Cep.Tasiyor(EsyaTuru.KatAnahtari, kat)) Sorun($"{kat}. kat: kat anahtarı elde değil");
    }

    private Kilit KilitBul(int kat, string kimlik)
    {
        foreach (Kilit k in FindObjectsByType<Kilit>(FindObjectsInactive.Include))
            if (k.Veri != null && k.Veri.Kimlik == kimlik && k.Kat == kat) return k;
        return null;
    }

    private IEnumerator KilidiAc(int kat, KilitZinciri z, ZincirKilidi veri)
    {
        Kilit kilit = KilitBul(kat, veri.Kimlik);
        if (kilit == null) { Sorun($"{kat}. kat '{veri.Kimlik}': Kilit bileşeni sahnede yok"); yield break; }
        if (kilit.Cozuk) yield break;
        Yaz($"{kat}. kat: '{veri.Kimlik}' ({veri.Tur})");
        GameObject o = kontrol.gameObject;

        switch (veri.Tur)
        {
            case KilitTuru.Anahtar:
                yield return Al(kat, veri.Gerekli);
                kilit.Etkilesim(o);
                break;
            case KilitTuru.Sifre:
                // Şifre ekranı (IMGUI) bot için atlanır; parçalı şifrelerde parçalar toplanır.
                if (veri.Parcalar != null) foreach (string p in veri.Parcalar) yield return Al(kat, p);
                kilit.Coz(acilsin: true);
                break;
            case KilitTuru.Kol:
                CekilenKol kol = null;
                foreach (CekilenKol c in FindObjectsByType<CekilenKol>())
                    if ((typeof(CekilenKol).GetField("kilitler", Gizli).GetValue(c) as List<Kilit>).Contains(kilit)) kol = c;
                if (kol == null) Sorun($"{kat}. kat '{veri.Kimlik}': kolu bulunamadı");
                else kol.Etkilesim(o);
                break;
            case KilitTuru.Sigorta:
                yield return Al(kat, veri.Gerekli);
                SigortaYuvasi yuva = FindAnyObjectByType<SigortaYuvasi>();
                if (yuva == null) { Sorun($"{kat}. kat: sigorta yuvası yok"); break; }
                var kapak = typeof(SigortaYuvasi).GetField("kapak", Gizli).GetValue(yuva) as AcilirParca;
                if (kapak != null && !kapak.Acik) { kapak.Etkilesim(o); yield return new WaitForSeconds(1f); }
                if (!yuva.EtkilesimeAcik) Sorun($"{kat}. kat: sigorta yuvası takmaya açık değil (el: {Cep.Tasinan})");
                yuva.Etkilesim(o);
                break;
            case KilitTuru.Tornavida:
                yield return AletAl(kat, veri.Gerekli);
                MenteseSokucu sokucu = kilit.GetComponent<MenteseSokucu>();
                for (int i = 0; i < 12 && sokucu != null && !sokucu.Devrildi; i++)
                {
                    kilit.Etkilesim(o);
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            case KilitTuru.Kirma:
                yield return AletAl(kat, veri.Gerekli);
                kilit.Etkilesim(o);
                break;
            case KilitTuru.Ekipman:
                foreach (string g in veri.Gerekenler) yield return AletAl(kat, g);
                kilit.Etkilesim(o);
                break;
            case KilitTuru.KirikAnahtar:
                foreach (string g in veri.Gerekenler) yield return Al(kat, g);
                kilit.Etkilesim(o);
                break;
            case KilitTuru.Yerlestirme:
                foreach (string g in veri.Gerekenler) yield return Al(kat, g);
                yield return SofrayiKur(kat, veri);
                break;
            case KilitTuru.Asansor:
                yield return Al(kat, veri.Gerekli);
                // Sahanlık A'nın panosu ve düğmesi: dokuzuncu katın asansörü de sahnede (#108), rastgele bulunmaz.
                Transform asansorA = GameObject.Find("Sahanlik_A/Asansor")?.transform;
                AsansorPanosu pano = asansorA != null ? asansorA.GetComponentInChildren<AsansorPanosu>() : null;
                if (pano == null) { Sorun("asansör panosu yok"); break; }
                if (!pano.EtkilesimeAcik) Sorun($"asansör panosu takmaya açık değil (el: {Cep.Tasinan})");
                pano.Etkilesim(o);
                asansorA.GetComponentInChildren<AsansorDugmesi>().Etkilesim(o);
                for (float t = 0; t < 15f && pano.Hal != AsansorPanosu.Durum.Geldi; t += 0.5f) yield return new WaitForSeconds(0.5f);
                break;
        }

        for (float t = 0; t < 6f && !kilit.Cozuk; t += 0.25f) yield return new WaitForSeconds(0.25f);
        if (!kilit.Cozuk)
        {
            Sorun($"{kat}. kat '{veri.Kimlik}' ({veri.Tur}) gerçek yoldan açılmadı; zorla çözülüyor");
            kilit.Coz(acilsin: true);
        }
        yield return new WaitForSeconds(0.3f);
    }

    // Sofra (#66): tabakları yığından doğru yerlere koyar.
    private IEnumerator SofrayiKur(int kat, ZincirKilidi veri)
    {
        var yuvalar = new List<Yuva>(FindObjectsByType<Yuva>());
        for (int i = 0; i < veri.Gerekenler.Count; i++)
        {
            string tabak = veri.Gerekenler[i];
            Yuva y = yuvalar.Find(v => v.Beklenen == tabak);
            if (y == null) { Sorun($"{kat}. kat: '{tabak}' için yuva yok"); continue; }
            if (y.Dogru) continue;
            // İstenen tabağı yığının üstüne getir.
            GameObject nesne = null;
            foreach (Esya e in FindObjectsByType<Esya>(FindObjectsInactive.Include)) if (e.Kimlik == tabak) nesne = e.gameObject;
            if (nesne != null) { TabakYigini.Cikar(nesne); TabakYigini.UsteKoy(nesne); }
            if (!y.EtkilesimeAcik) Sorun($"{kat}. kat: yuva '{tabak}' koymaya açık değil");
            y.Etkilesim(kontrol.gameObject);
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ---------- EŞYA ----------
    private Esya EsyaBul(string kimlik)
    {
        foreach (Esya e in FindObjectsByType<Esya>(FindObjectsInactive.Include))
            if (e.Kimlik == kimlik && e.gameObject.scene.IsValid()) return e;
        return null;
    }

    private IEnumerator AletAl(int kat, string kimlik)
    {
        Esya e = EsyaBul(kimlik);
        if (e == null || !e.gameObject.activeInHierarchy)
        {
            // Alet daha önce alınmış olabilir (aletler katlar arası kalır).
            yield break;
        }
        yield return Al(kat, kimlik);
    }

    private IEnumerator Al(int kat, string kimlik)
    {
        if (Cep.KimligiTasiyor(kimlik)) yield break;
        Esya e = EsyaBul(kimlik);
        if (e == null) { yield break; }                         // alınmış (parça, tabak, alet) ya da hiç konmamış
        if (!e.gameObject.activeInHierarchy)
        {
            // Pasif: ya alınmış ya da gizli bir yerde (kapalı kabin gibi). Ebeveyni pasifse sorun.
            if (e.transform.parent != null && !e.transform.parent.gameObject.activeInHierarchy)
                Sorun($"{kat}. kat: '{kimlik}' pasif bir yerin içinde ({e.transform.parent.name})");
            yield break;
        }
        // Kapları aç: eşyanın içinde durduğu çekmece/kapak ve kendi kapağı.
        foreach (AcilirParca p in e.GetComponentsInParent<AcilirParca>())
            if (!p.Acik) { p.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(0.8f); }
        // Kutunun kapağı eşyanın ebeveyni değil kardeşidir (alet çantası, sandık): kabın açılır
        // parçalarından kilitsiz ve kapalı olanlar da açılır.
        if (e.transform.parent != null)
            foreach (AcilirParca p in e.transform.parent.GetComponentsInChildren<AcilirParca>())
                if (!p.Acik && !p.Kilitli) { p.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(0.8f); }
        var kap = typeof(Esya).GetField("kap", Gizli)?.GetValue(e) as AcilirParca;
        // Kırılan kavanozun kapağı kırılınca gizlenir (Kirilan); gizli parça açılamaz (oyuncu da ona bakamaz).
        if (kap != null && !kap.Acik && kap.gameObject.activeInHierarchy) { kap.Etkilesim(kontrol.gameObject); yield return new WaitForSeconds(0.9f); }
        if (!e.EtkilesimeAcik)
        {
            Sorun($"{kat}. kat: '{kimlik}' alınmaya açık değil (yer: {e.transform.parent?.name}, el: {Cep.Tasinan})");
            yield break;
        }
        e.Etkilesim(kontrol.gameObject);
        yield return new WaitForSeconds(0.4f);
        YakinEkranlariKapat();
    }

    // Parça alınınca çizimin yakın ekranı açılır ve oyuncunun kontrolünü kapatır; oyuncu E ya da
    // Esc ile kapatır. Bot tuşa basmaz: ekranın kendi kapanış alanlarını ayarlar.
    private void YakinEkranlariKapat()
    {
        foreach (CizimEkrani c in FindObjectsByType<CizimEkrani>())
        {
            var alan = typeof(CizimEkrani).GetField("acik", Gizli);
            if (alan == null || !(bool)alan.GetValue(c)) continue;
            alan.SetValue(c, false);
            typeof(CizimEkrani).GetField("geriVerilecek", Gizli).SetValue(c, true);
        }
    }

    // ---------- YÜRÜME ----------
    // Oyuncuyu CharacterController ile noktadan noktaya yürütür (yalnızca yatay; basamakları
    // denetleyici kendisi çıkar). 4 sn ilerleme olmazsa takıldı sayar, bunu sorun olarak yazar
    // ve sıradaki noktaya ışınlar.
    private IEnumerator Yuru(Vector3[] noktalar, string ad)
    {
        foreach (Vector3 hedef in noktalar)
        {
            float enIyi = float.MaxValue, sonIlerleme = Time.time;
            while (true)
            {
                if (kontrol == null || !kontrol.enabled) { yield return null; continue; }   // kararma, not ekranı
                Vector3 p = kontrol.transform.position;
                Vector3 fark = new Vector3(hedef.x - p.x, 0f, hedef.z - p.z);
                float d = fark.magnitude;
                if (d < 0.2f) break;
                if (d < enIyi - 0.05f) { enIyi = d; sonIlerleme = Time.time; }
                if (Time.time - sonIlerleme > 4f)
                {
                    Sorun($"{ad}: ({hedef.x:0.0}, {hedef.z:0.0}) noktasına giderken takıldı, konum ({p.x:0.00}, {p.y:0.00}, {p.z:0.00})");
                    govde.enabled = false;
                    kontrol.transform.position = new Vector3(hedef.x, p.y + 0.3f, hedef.z);
                    govde.enabled = true;
                    Physics.SyncTransforms();
                    break;
                }
                kontrol.transform.rotation = Quaternion.LookRotation(fark);
                govde.Move(fark / d * Mathf.Min(Hiz * Time.deltaTime, d));
                yield return null;
            }
        }
    }

    // Sahanlığın başına geç (daireden çıkmak için ışınlanır; dairenin içinde yol bulmak botun işi değil).
    private IEnumerator SahanligaGec()
    {
        govde.enabled = false;
        kontrol.transform.position = new Vector3(MekanOlculeri.OyuncuBaslangicX, 0.1f, MekanOlculeri.OyuncuBaslangicZ);
        govde.enabled = true;
        Physics.SyncTransforms();
        yield return new WaitForSeconds(0.5f);
    }

    // Merdiven: çıkış kolundan ara sahanlığa, kat kapısını anahtarla aç, varış kolundan üst kata.
    private IEnumerator Merdivenden(int? kat)
    {
        float x = MekanOlculeri.CikisKoluX, kol = MekanOlculeri.KolUzunluk;
        yield return Yuru(new[] { new Vector3(x, 0, -0.3f), new Vector3(x, 0, kol - 0.45f) }, "çıkış kolu");
        if (kat.HasValue)
        {
            KatKapisi kapi = null;
            foreach (KatKapisi k in FindObjectsByType<KatKapisi>())
                if (k.Kat == kat.Value && !k.Acik) kapi = k;
            if (kapi != null)
            {
                kapi.Etkilesim(kontrol.gameObject);
                yield return new WaitForSeconds(3f);
                if (!kapi.Acik) Sorun($"{kat}. kat kapısı anahtarla açılmadı (el: {Cep.Tasinan})");
            }
        }
        yield return Yuru(new[] { new Vector3(x, 0, kol + 0.5f), new Vector3(0f, 0, kol + 0.75f),
                                  new Vector3(MekanOlculeri.VarisKoluX, 0, kol + 0.4f),
                                  new Vector3(MekanOlculeri.VarisKoluX, 0, -0.4f) }, "varış kolu");
    }

    // ---------- ÇATI ----------
    // ---------- DOKUZUNCU KAT (#108) ----------
    // 8. katın asansörü geldi, kabin boş: bot kabine yürür, yolculuğu bekler, lambayı çiviye asar, salon
    // açılınca anahtarı tabaktan alır ve (yol bulmadan, ışınlanarak) kabine kaçar; iniş bitince çatıya.
    private IEnumerator Dokuz()
    {
        Yaz("8. kat: kabine biniyor (dokuzuncu kat)");
        DokuzuncuKat dk = DokuzuncuKat.Ornek;
        if (dk == null) { Sorun("DokuzuncuKat yok (graybox yeniden kurulmalı)"); yield break; }
        yield return Yuru(new[] { new Vector3(0f, 0f, -4.3f) }, "kabin önüne");
        // Kabine adım adım: pano oyuncuyu içeride görür görmez yolculuk başlar (kontrol kapanır, oyuncu
        // ışınlanır); Yuru() o anda "takıldı" sanıp geri ışınlardı. Burada yolculuk başlayınca durulur.
        for (float t = 0; t < 12f && !DokuzuncuKat.Icinde; t += Time.deltaTime)
        {
            if (kontrol.enabled) { kontrol.transform.rotation = Quaternion.LookRotation(Vector3.back); govde.Move(Vector3.back * Hiz * Time.deltaTime); }
            yield return null;
        }
        if (!DokuzuncuKat.Icinde) { Sorun("asansör dokuzuncu kata çıkmadı (oyuncu kabinde görülmedi)"); yield break; }
        for (float t = 0; t < 20f && !kontrol.enabled; t += 0.25f) yield return new WaitForSeconds(0.25f);
        if (!kontrol.enabled) { Sorun("dokuzuncu kat yolculuğu bitmedi (kontrol geri gelmedi)"); yield break; }
        Vector3 kok = dk.transform.position;
        if (Vector3.Distance(new Vector3(kontrol.transform.position.x, 0, 0), new Vector3(kok.x, 0, 0)) > 20f)
            Sorun($"oyuncu dokuzuncu katta değil (x {kontrol.transform.position.x:0.0})");
        Yaz("9. kat: lambayı çiviye asıyor");
        CiviAskisi civi = dk.GetComponentInChildren<CiviAskisi>(true);
        if (civi == null) { Sorun("dokuzuncu katta boş çivi yok"); yield break; }
        Isinla(civi.transform.position + dk.transform.TransformDirection(Vector3.zero) + (civi.transform.forward * 0.9f), kok.y);
        if (!civi.EtkilesimeAcik) Sorun("boş çivi asmaya açık değil (lamba kafada değil mi?)");
        civi.Etkilesim(kontrol.gameObject);
        yield return new WaitForSeconds(2.2f);
        IcKapi salon = null;
        foreach (IcKapi k in dk.GetComponentsInChildren<IcKapi>(true)) if (k.name == "Kapi_Salon") salon = k;
        if (salon == null) Sorun("dokuzuncu katta salon kapısı bulunamadı");
        else if (!salon.Acik) Sorun("salon kapısı lamba asılınca açılmadı");
        Yaz("9. kat: sofradan çatı anahtarını alıyor");
        Esya anahtar = null;
        foreach (Esya e in dk.GetComponentsInChildren<Esya>(true)) if (e.Tur == EsyaTuru.KatAnahtari) anahtar = e;
        if (anahtar == null) { Sorun("sofrada çatı anahtarı yok"); yield break; }
        Transform masa = anahtar.transform.parent != null ? anahtar.transform.parent.parent : null;   // Yer_2 -> Masa
        Vector3 sandalyeTarafi = masa != null ? masa.forward : Vector3.forward;
        Isinla(anahtar.transform.position + sandalyeTarafi * 0.75f, kok.y);
        if (!anahtar.EtkilesimeAcik) Sorun("çatı anahtarı alınmaya açık değil");
        anahtar.Etkilesim(kontrol.gameObject);
        yield return new WaitForSeconds(0.5f);
        if (!Cep.Tasiyor(EsyaTuru.KatAnahtari, KatSayaci.CatiKati)) { Sorun("çatı anahtarı cebe girmedi"); yield break; }
        Yaz("9. kat: kabine kaçıyor");
        Transform kabin9 = dk.transform.Find("Sahanlik_9/Asansor/Kabin");
        if (kabin9 == null) { Sorun("dokuzuncu katın kabini yok"); yield break; }
        Isinla(kabin9.TransformPoint(new Vector3(0f, 0f, -MekanOlculeri.SahanlikDerin - MekanOlculeri.DuvarKalin - 0.6f)), kok.y);
        for (float t = 0; t < 25f && DokuzuncuKat.Icinde; t += 0.25f) yield return new WaitForSeconds(0.25f);
        if (DokuzuncuKat.Icinde) { Sorun("asansör 8. kata inmedi"); yield break; }
        for (float t = 0; t < 10f && !kontrol.enabled; t += 0.25f) yield return new WaitForSeconds(0.25f);
        if (!DokuzuncuKat.Yasandi) Sorun("dokuzuncu kat 'yaşandı' işaretlenmedi");
        if (Mathf.Abs(kontrol.transform.position.x) > 20f) Sorun($"oyuncu 8. kata dönmedi (x {kontrol.transform.position.x:0.0})");
        Yaz("8. kata dönüldü; çatı anahtarı elde");
    }

    // Botu bir noktaya ışınlar (CharacterController kapat-taşı-aç). y: zemin kotu (+0,05 düşsün diye).
    private void Isinla(Vector3 nokta, float zeminY)
    {
        govde.enabled = false;
        kontrol.transform.position = new Vector3(nokta.x, zeminY + 0.05f, nokta.z);
        govde.enabled = true;
        Physics.SyncTransforms();
    }

    private IEnumerator Cati()
    {
        Yaz("8. kat bitti; çatıya çıkıyor");
        yield return SahanligaGec();
        yield return Merdivenden(null);
        yield return new WaitForSeconds(1.5f);
        CatiKapisi kapi = FindAnyObjectByType<CatiKapisi>();
        if (kapi == null) { Sorun("çatı kapısı bulunamadı (çatı modu açık değil mi?)"); yield break; }
        kapi.Etkilesim(kontrol.gameObject);
        yield return new WaitForSeconds(1f);
        if (kapi != null && kapi.gameObject.activeSelf) { Sorun("çatı kapısı çatı anahtarıyla açılmadı"); yield break; }
        yield return Yuru(new[] { new Vector3(MekanOlculeri.VarisKoluX, 0, -2.8f) }, "çatıya");
        Yaz("çatıda; final başlamalı");
        yield return new WaitForSeconds(30f);
    }
}
#endif
