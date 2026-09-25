// AnomaliGorunurlukDenetimi.cs
// Her anomali kartının ekranda ne kadar görünür olduğunu ölçen Editor aracı (#33).
// Menü: Huzur → Anomali → Görünürlük Denetimi   →   rapor: Docs/anomali-gorunurluk.md
//
// NEDEN: #31'de gece post-processing'i ilk kez gerçekten devreye girdi ve oyun belirgin
// şekilde karardı. Bazı anomaliler artık fark edilemeyecek kadar zor olabilir; oyuncu
// göremediği bir anomali yüzünden ceza yerse bu adaletsizdir. Tek tek oynayarak bulmak
// yerine ölçüyoruz.
//
// NASIL:
//   1. Geçici bir kamera oyuncunun göz hizasına konur. İki noktadan bakılır: kata varış
//      noktası (4 yön) ve sahanlığın ortası (8 yön); oyuncu sahanlıkta dolaşıp her yere bakabilir.
//   2. Sahanlık önce NORMAL hâlde çizilir; sonra her kart uygulanıp aynı yönler çizilir.
//   3. Zamanlı kartlarda (ışık bir an söner, kapı sonra çarpar...) her etkinin gerçekleştiği
//      AN ayrı ölçülür. İlk denemede yalnızca son hâl ölçülmüştü; "söner ve geri gelir" gibi
//      anlar sonunda normale döndüğü için tamamen kaçırılıyordu.
//   4. Her yön ve an için iki görüntünün farkı iki ölçüyle alınır (GorunurlukOlcer): alan
//      (ekranın ne kadarı değişti) ve güç (değişen yerler ne kadar belirgin değişti). Kartın
//      değeri en görünür olduğu yön ve andaki değerdir.
//   5. Kart geri alınır. Bütün ölçüm gece efektleri açık ve kapalı olarak iki kez yapılır.
//
// Film greni ölçüm boyunca geçici olarak kapatılır: her karede rastgele kumlanma eklediği
// için hiçbir şey değişmemiş iki kare bile her yerde "farklı" görünürdü. Sonunda eski
// hâline döner. Araç sahneyi kaydetmez; kaydedilmemiş değişiklik varsa çalışmayı reddeder.

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AnomaliGorunurlukDenetimi
{
    private const int Genislik = 960, Yukseklik = 540;   // kapı zili, kol gibi küçük ayrıntılar birkaç piksele düşmesin
    private const string ResimKlasoru = "Builds/AnomaliGorunurluk";   // git dışında (/Builds/)
    private const string RaporYolu = "Docs/anomali-gorunurluk.md";

    // Bakış noktaları: (x, z) konumu ve dik eksen etrafında yön (derece; 180 = sahanlığa doğru, -z).
    private struct Bakis { public float X, Z, Yon; public Bakis(float x, float z, float yon) { X = x; Z = z; Yon = yon; } }
    private static readonly Bakis[] Bakislar = OlusturBakislar();

    private static Bakis[] OlusturBakislar()
    {
        var liste = new List<Bakis>();
        // Kata varış noktası: sahanlığa doğru, sağa, sola ve arkaya (merdivene).
        foreach (float yon in new[] { 180f, 125f, 235f, 0f })
            liste.Add(new Bakis(MekanOlculeri.OyuncuBaslangicX, MekanOlculeri.OyuncuBaslangicZ, yon));
        // Sahanlığın ortası: 45 derecede bir, sekiz yön.
        for (int i = 0; i < 8; i++)
            liste.Add(new Bakis(0f, -MekanOlculeri.SahanlikDerin / 2f, i * 45f));
        return liste.ToArray();
    }

    // Bir ölçüm: alan ve güç.
    private struct Olcum
    {
        public float Alan, Guc;
        public int Bakis;   // hangi yönde ölçüldü
        public int Sinif { get { return GorunurlukOlcer.Sinif(Alan, Guc); } }
        // "Daha görünür mü": önce sınıf, eşitse alan.
        public bool DahaIyi(Olcum o) { return Sinif != o.Sinif ? Sinif > o.Sinif : Alan > o.Alan; }
    }

    private class Sonuc
    {
        public AnomaliKarti Kart;
        public Olcum Efektli, Efektsiz;
        public bool SesKarti, YaziKarti, Zamanli;
        public int EnIyiBakis, EnIyiAn;   // efektli turda en görünür olduğu yön ve an (resim için)
    }

    [MenuItem("Huzur/Anomali/Görünürlük Denetimi")]
    public static void Calistir()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError("[GÖRÜNÜRLÜK] Sahnede kaydedilmemiş değişiklik var; önce kaydedin (anomali önizlemesi açıksa geri alın).");
            return;
        }

        GameObject sahanlik = GameObject.Find("Sahanlik_A");
        GameObject atmosfer = GameObject.Find("Atmosfer");
        Volume hacim = atmosfer != null ? atmosfer.GetComponent<Volume>() : null;
        if (sahanlik == null || hacim == null || hacim.sharedProfile == null)
        {
            Debug.LogError("[GÖRÜNÜRLÜK] Sahanlik_A ya da Atmosfer (Volume) bulunamadı.");
            return;
        }

        AnomaliKarti[] kartlar = Resources.LoadAll<AnomaliKarti>("Anomaliler");

        // Geçici kamera ve çizim hedefi (RenderTexture: kameranın ekrana değil, bellekteki
        // bir resme çizmesini sağlar; o resmin pikselleri okunabilir).
        GameObject kameraNesnesi = new GameObject("Denetim_Kamerasi");
        kameraNesnesi.hideFlags = HideFlags.HideAndDontSave;   // sahneye kaydedilmesin, Hierarchy'de görünmesin
        Camera kamera = kameraNesnesi.AddComponent<Camera>();
        kamera.fieldOfView = 60f;
        kamera.nearClipPlane = 0.05f;
        UniversalAdditionalCameraData veri = kameraNesnesi.AddComponent<UniversalAdditionalCameraData>();
        RenderTexture hedef = new RenderTexture(Genislik, Yukseklik, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        kamera.targetTexture = hedef;
        Texture2D okuyucu = new Texture2D(Genislik, Yukseklik, TextureFormat.RGBA32, false);

        // Film grenini geçici olarak kapat (nedeni dosya başında).
        FilmGrain gren = null;
        bool grenEskiden = false;
        if (hacim.sharedProfile.TryGet(out gren)) { grenEskiden = gren.active; gren.active = false; }

        var sonuclar = new List<Sonuc>();
        float gurultuTabani = 0f;

        try
        {
            foreach (AnomaliKarti kart in kartlar)
                sonuclar.Add(new Sonuc { Kart = kart, SesKarti = SesKartiMi(kart), YaziKarti = YaziKartiMi(kart), Zamanli = Anlar(kart).Count > 0 });

            // İki tur: önce efektler açık (bugünkü görünüş), sonra kapalı (#31 öncesi).
            foreach (bool efektli in new[] { true, false })
            {
                veri.renderPostProcessing = efektli;

                // Normal sahanlık, her yönden. Gürültü tabanı: aynı yön iki kez çizilip
                // karşılaştırılır; sıfırdan büyükse ölçümler o kadar "titrer".
                float[][] normal = new float[Bakislar.Length][];
                for (int b = 0; b < Bakislar.Length; b++)
                {
                    normal[b] = Ciz(kamera, hedef, okuyucu, Bakislar[b]);
                    float tekrar = GorunurlukOlcer.Alan(GorunurlukOlcer.FarkHaritasi(normal[b], Ciz(kamera, hedef, okuyucu, Bakislar[b])), GorunurlukOlcer.VarsayilanEsik);
                    if (efektli && tekrar > gurultuTabani) gurultuTabani = tekrar;
                }

                foreach (Sonuc s in sonuclar)
                {
                    var uygulayici = new AnomaliUygulayici();
                    uygulayici.Uygula(s.Kart, sahanlik.transform);   // gecikmesiz etkiler hemen uygulanır

                    // İlk an: kart uygulandığı hâl. Sonra her zamanlı etkinin gerçekleştiği an.
                    Olcum enIyi = OlcAn(kamera, hedef, okuyucu, normal);
                    int enIyiAn = 0;
                    float saat = 0f;
                    List<float> anlar = Anlar(s.Kart);
                    for (int a = 0; a < anlar.Count; a++)
                    {
                        uygulayici.Ilerlet(anlar[a] - saat + 0.001f);   // saati o ana ilerlet
                        saat = anlar[a] + 0.001f;
                        Olcum o = OlcAn(kamera, hedef, okuyucu, normal);
                        if (o.DahaIyi(enIyi)) { enIyi = o; enIyiAn = a + 1; }
                    }

                    if (efektli) { s.Efektli = enIyi; s.EnIyiBakis = enIyi.Bakis; s.EnIyiAn = enIyiAn; }
                    else s.Efektsiz = enIyi;
                    uygulayici.GeriAl();
                }
            }

            // RESİMLER: her görsel kart için, efektler açıkken en görünür olduğu yön ve anda
            // "normal | anomali" yan yana. Şahan tabloya körü körüne güvenmek yerine gözüyle bakabilsin.
            veri.renderPostProcessing = true;
            if (Directory.Exists(ResimKlasoru)) Directory.Delete(ResimKlasoru, true);
            Directory.CreateDirectory(ResimKlasoru);
            foreach (Sonuc s in sonuclar)
            {
                if (s.SesKarti) continue;
                Bakis bakis = Bakislar[s.EnIyiBakis];
                float[] normalResim = Ciz(kamera, hedef, okuyucu, bakis);

                var uygulayici = new AnomaliUygulayici();
                uygulayici.Uygula(s.Kart, sahanlik.transform);
                List<float> anlar = Anlar(s.Kart);
                if (s.EnIyiAn > 0) uygulayici.Ilerlet(anlar[s.EnIyiAn - 1] + 0.001f);
                float[] anomaliResim = Ciz(kamera, hedef, okuyucu, bakis);
                uygulayici.GeriAl();

                ResmiKaydet(normalResim, anomaliResim, Path.Combine(ResimKlasoru, s.Kart.Kimlik + ".png"));
            }
        }
        finally
        {
            // Ne olursa olsun (hata dahil) her şeyi eski hâline döndür.
            if (gren != null) gren.active = grenEskiden;
            kamera.targetTexture = null;
            Object.DestroyImmediate(hedef);
            Object.DestroyImmediate(okuyucu);
            Object.DestroyImmediate(kameraNesnesi);
        }

        RaporuYaz(sonuclar, gurultuTabani);
        Debug.Log($"[GÖRÜNÜRLÜK] {sonuclar.Count} kart ölçüldü, gürültü tabanı %{gurultuTabani * 100f:0.00} → {RaporYolu}");
    }

    // Şu anki sahneyi her yönden çizip normalle karşılaştırır; en görünür yöndeki ölçümü verir.
    private static Olcum OlcAn(Camera kamera, RenderTexture hedef, Texture2D okuyucu, float[][] normal)
    {
        Olcum enIyi = new Olcum();
        for (int b = 0; b < Bakislar.Length; b++)
        {
            float[] harita = GorunurlukOlcer.FarkHaritasi(normal[b], Ciz(kamera, hedef, okuyucu, Bakislar[b]));
            Olcum o = new Olcum
            {
                Alan = GorunurlukOlcer.Alan(harita, GorunurlukOlcer.VarsayilanEsik),
                Guc = GorunurlukOlcer.Guc(harita, GorunurlukOlcer.VarsayilanEsik),
                Bakis = b
            };
            if (o.DahaIyi(enIyi)) enIyi = o;
        }
        return enIyi;
    }

    // İki görüntüyü yan yana (solda normal, sağda anomali) tek bir PNG olarak kaydeder.
    private static void ResmiKaydet(float[] normal, float[] anomali, string yol)
    {
        Texture2D resim = new Texture2D(Genislik * 2, Yukseklik, TextureFormat.RGB24, false);
        Color32[] pikseller = new Color32[Genislik * 2 * Yukseklik];
        for (int y = 0; y < Yukseklik; y++)
        {
            for (int x = 0; x < Genislik; x++)
            {
                int kaynak = (y * Genislik + x) * 3;
                pikseller[y * Genislik * 2 + x] = Renk(normal, kaynak);
                pikseller[y * Genislik * 2 + Genislik + x] = Renk(anomali, kaynak);
            }
        }
        resim.SetPixels32(pikseller);
        File.WriteAllBytes(yol, resim.EncodeToPNG());
        Object.DestroyImmediate(resim);
    }

    private static Color32 Renk(float[] dizi, int i)
    {
        return new Color32((byte)(dizi[i] * 255f), (byte)(dizi[i + 1] * 255f), (byte)(dizi[i + 2] * 255f), 255);
    }

    // Kamerayı bakış noktasına koyup çizer ve parlaklık dizisini döndürür.
    private static float[] Ciz(Camera kamera, RenderTexture hedef, Texture2D okuyucu, Bakis bakis)
    {
        kamera.transform.SetPositionAndRotation(
            new Vector3(bakis.X, 1.65f, bakis.Z),        // göz hizası
            Quaternion.Euler(8f, bakis.Yon, 0f));        // 8° aşağı: oyuncunun doğal bakışı
        kamera.Render();

        // ReadPixels, o an "etkin" olan RenderTexture'dan okur.
        RenderTexture onceki = RenderTexture.active;
        RenderTexture.active = hedef;
        okuyucu.ReadPixels(new Rect(0, 0, Genislik, Yukseklik), 0, 0);
        okuyucu.Apply();
        RenderTexture.active = onceki;

        // Renk kanalları arka arkaya: r, g, b, r, g, b... (0-1). Yalnızca parlaklık değil
        // renk de karşılaştırılsın diye (nedeni GorunurlukOlcer'de).
        Color32[] pikseller = okuyucu.GetPixels32();
        float[] kanallar = new float[pikseller.Length * 3];
        for (int i = 0; i < pikseller.Length; i++)
        {
            kanallar[i * 3] = pikseller[i].r / 255f;
            kanallar[i * 3 + 1] = pikseller[i].g / 255f;
            kanallar[i * 3 + 2] = pikseller[i].b / 255f;
        }
        return kanallar;
    }

    // Kartın zamanlı etkilerinin gerçekleştiği anlar (saniye), küçükten büyüğe, tekrarsız.
    private static List<float> Anlar(AnomaliKarti kart)
    {
        var anlar = new List<float>();
        foreach (AnomaliEtkisi e in kart.Etkiler)
            if (e.gecikme > 0f && !anlar.Contains(e.gecikme)) anlar.Add(e.gecikme);
        anlar.Sort();
        return anlar;
    }

    // Yalnızca sesle anlaşılan kart: bütün etkileri ses çalıyor ya da bir ses katmanını
    // susturuyor/taşıyor (ses katmanları "Ses_" ile başlar). Görsel farkı zaten sıfırdır.
    private static bool SesKartiMi(AnomaliKarti kart)
    {
        foreach (AnomaliEtkisi e in kart.Etkiler)
        {
            bool sesle = e.tur == EtkiTuru.Ses || (e.hedefYol != null && e.hedefYol.Contains("Ses_"));
            if (!sesle) return false;
        }
        return kart.Etkiler.Length > 0;
    }

    // Yazı kartı: bütün etkileri bir yazıyı değiştiriyor (kapı numarası gibi). Oyuncu yazıyı
    // kapıya yaklaşıp OKUR; iki bakış noktasından ölçülen alan onu olduğundan görünmez gösterir.
    private static bool YaziKartiMi(AnomaliKarti kart)
    {
        foreach (AnomaliEtkisi e in kart.Etkiler) if (e.tur != EtkiTuru.Metin) return false;
        return kart.Etkiler.Length > 0;
    }

    // ---------- RAPOR ----------
    private static readonly string[] SinifAdi = { "görünmez", "silik", "küçük ama net", "belirgin" };

    private static void RaporuYaz(List<Sonuc> sonuclar, float gurultuTabani)
    {
        // En az görünenden en çok görünene (efektler açıkken): önce sınıf, sonra alan.
        sonuclar.Sort((a, b) => a.Efektli.Sinif != b.Efektli.Sinif ? a.Efektli.Sinif.CompareTo(b.Efektli.Sinif) : a.Efektli.Alan.CompareTo(b.Efektli.Alan));

        var sb = new StringBuilder();
        sb.AppendLine("# Anomali görünürlük denetimi (#33)");
        sb.AppendLine();
        sb.AppendLine($"`Huzur → Anomali → Görünürlük Denetimi` ile üretildi ({System.DateTime.Now:yyyy-MM-dd HH:mm}). Elle düzenlemeyin; aracı yeniden çalıştırın.");
        sb.AppendLine();
        sb.AppendLine("**Ne ölçüldü:** her kart için sahanlık, oyuncunun göz hizasından iki noktadan (kata varış noktası: 4 yön, sahanlığın ortası: 8 yön) normal ve anomalili hâlde çizildi. Zamanlı kartlarda her etkinin gerçekleştiği an ayrı ölçüldü. Kartın değeri en görünür olduğu yön ve andaki değerdir.");
        sb.AppendLine();
        sb.AppendLine("- **Alan:** belirgin biçimde (bir renk kanalında 0,03'ten fazla) değişen piksellerin ekrana oranı. Renk değişimleri de sayılır (yeşil ampul gibi).");
        sb.AppendLine("- **Güç:** değişen piksellerin ortalama farkı (0-1). Küçük ama net bir değişiklik (kapı numarası) alanı küçük, gücü büyüktür.");
        sb.AppendLine($"- **Sınıf** (sınırlar sezgisel başlangıç değerleri; Şahan'ın gözüyle karşılaştırılıp ayarlanacak): görünmez (alan < %{GorunurlukOlcer.GorunmezSiniri * 100f:0.00}) · silik (güç < {GorunurlukOlcer.SilikSiniri:0.00}) · küçük ama net (alan < %{GorunurlukOlcer.KucukSiniri * 100f:0.0}) · belirgin.");
        sb.AppendLine($"- **Gürültü tabanı:** %{gurultuTabani * 100f:0.00} (aynı sahne iki kez çizilince ölçülen fark; 0'a yakınsa ölçüm güvenilir).");
        sb.AppendLine("- ⚠ = tasarım zorluğu 1-2 (\"kolay\") olduğu hâlde görünmez ya da silik çıkan kart. ↓ = gece efektleri (#31) kartı daha düşük bir sınıfa indirmiş.");
        sb.AppendLine();

        int[] sayac = new int[4];
        sb.AppendLine("## Görsel kartlar (en az görünenden en çok görünene)");
        sb.AppendLine();
        sb.AppendLine("| # | Kart | Katman | Zorluk | Efektli alan · güç | Efektsiz alan · güç | Sınıf (efektli) | Not |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        int sira = 0;
        foreach (Sonuc s in sonuclar)
        {
            if (s.SesKarti || s.YaziKarti) continue;
            sira++;
            int sinif = s.Efektli.Sinif;
            sayac[sinif]++;
            var not = new List<string>();
            if (sinif <= GorunurlukOlcer.Silik && s.Kart.Zorluk <= 2) not.Add("⚠ kolay sanılıyor");
            if (s.Efektli.Sinif < s.Efektsiz.Sinif) not.Add($"↓ efektsiz: {SinifAdi[s.Efektsiz.Sinif]}");
            if (s.Zamanli) not.Add("zamanlı");
            sb.AppendLine($"| {sira} | `{s.Kart.Kimlik}` | {s.Kart.Katman} | {s.Kart.Zorluk} | %{s.Efektli.Alan * 100f:0.00} · {s.Efektli.Guc:0.00} | %{s.Efektsiz.Alan * 100f:0.00} · {s.Efektsiz.Guc:0.00} | {SinifAdi[sinif]} | {string.Join(", ", not)} |");
        }
        sb.AppendLine();
        sb.AppendLine($"**Özet (efektli):** {sayac[0]} görünmez, {sayac[1]} silik, {sayac[2]} küçük ama net, {sayac[3]} belirgin.");
        sb.AppendLine();

        sb.AppendLine("## Yazı kartları (yakından okunur)");
        sb.AppendLine();
        sb.AppendLine("Kapı numarası gibi yazıyı değiştiren kartlar. Oyuncu yazıyı kapıya yaklaşıp okur; iki bakış noktasından ölçülen değer onları olduğundan az görünür gösterir, bu yüzden sınıflandırılmadılar. Değerler yalnızca bilgi için.");
        sb.AppendLine();
        foreach (Sonuc s in sonuclar)
            if (s.YaziKarti) sb.AppendLine($"- `{s.Kart.Kimlik}` ({s.Kart.Katman}, zorluk {s.Kart.Zorluk}) — efektli alan %{s.Efektli.Alan * 100f:0.00} · güç {s.Efektli.Guc:0.00}");
        sb.AppendLine();
        sb.AppendLine($"Her görsel kartın 'normal | anomali' resmi (en görünür yön ve an, efektler açık): `{ResimKlasoru}/<kart>.png` (git dışında; aracı çalıştıran makinede).");
        sb.AppendLine();

        sb.AppendLine("## Yalnızca sesle anlaşılan kartlar");
        sb.AppendLine();
        sb.AppendLine("Görsel farkları yok; görünürlükleri ses revizyonlarıyla (#14, #25) değerlendirilir.");
        sb.AppendLine();
        foreach (Sonuc s in sonuclar)
            if (s.SesKarti) sb.AppendLine($"- `{s.Kart.Kimlik}` ({s.Kart.Katman}, zorluk {s.Kart.Zorluk})");

        Directory.CreateDirectory(Path.GetDirectoryName(RaporYolu));
        File.WriteAllText(RaporYolu, sb.ToString(), new UTF8Encoding(false));
    }
}
