// GrayboxKurucu.cs
// Huzur Apartmanı'nın TEK mekânını (sahanlık + kat merdiveni) kodla kuran editör aracı.
// Neden kodla? Çünkü ölçüler tek yerde durur; bir basamağın yüksekliğini değiştirmek
// istediğimizde 60 nesneyi elle taşımak yerine tek sayıyı değiştirip yeniden kuruyoruz.
// Bu dosya OYUNDA çalışmaz; sadece Unity editöründe, menüden tetiklenir.

using UnityEngine;        // GameObject, Vector3, Transform gibi temel Unity tipleri buradan gelir.
using UnityEditor;        // MenuItem ve Undo gibi SADECE editörde çalışan araçlar buradan gelir.

// "using static": bir sınıfın içindeki değerleri, sınıf adını her seferinde
// yazmadan kullanmamızı sağlar. Bu satır sayesinde aşağıda
// "MekanOlculeri.KatYuksekligi" yerine sadece "KatYuksekligi" yazabiliyoruz.
// Ölçülerin kendisi MekanOlculeri.cs dosyasında; burada kopyası YOK.
using static MekanOlculeri;

// "static" sınıf: içinden nesne üretilemez, sadece fonksiyonları çağrılır.
// Bu sınıfın hiçbir hafızası yok; bir düğmeye basınca çalışan bir alet çantası gibi.
public static class GrayboxKurucu
{
    // Bütün ölçüler MekanOlculeri.cs dosyasındadır; yukarıdaki "using static"
    // sayesinde buradan doğrudan adlarıyla kullanılıyorlar.

    // ---------- MENÜ GİRİŞİ ----------
    // [MenuItem] bir "öznitelik"tir: fonksiyonun üstüne yapıştırılan etikettir.
    // Unity bu etiketi görünce menü çubuğuna bir satır ekler. Tıklayınca fonksiyon çalışır.
    [MenuItem("Huzur/Graybox Sahanlığı Yeniden Kur")]
    public static void YenidenKur()
    {
        Temizle();   // önce eski kopyaları sil, üst üste binmesin
        Kur();       // sonra sıfırdan kur
    }

    // ---------- TEMİZLİK ----------
    // Aracı ikinci kez çalıştırdığımızda sahnede iki kopya olmasın diye önce siliyoruz.
    static void Temizle()
    {
        // Silinecek kök nesnelerin adları. string[] = metin dizisi.
        string[] adlar =
        {
            "Sahanlik_A", "Sahanlik_B",
            "Merdiven_Yukari", "Merdiven_Yukari2", "Merdiven_Asagi",
            "Kapak_Alt", "CatiBolumu",

            // ESKİ İSİMLER. Bu araç bir zamanlar "Kapak_Ust" adlı bir duvar kuruyordu;
            // yerini CatiBolumu aldı. Ama eski adı listeden ÇIKARIRSAK, daha önce
            // kurulmuş sahnelerde o duvar silinmeden kalır ve çatı kapısının önünü keser.
            // Kural: bu liste, aracın BUGÜNE KADAR ürettiği her ismi içerir.
            "Kapak_Ust"
        };

        // foreach: dizideki her elemanı sırayla gez.
        foreach (string ad in adlar)
        {
            // Find, sahnede o isimde bir nesne arar; bulamazsa null döner.
            GameObject bulunan = GameObject.Find(ad);

            // null kontrolü: yoksa silmeye çalışmayalım, yoksa hata verir.
            if (bulunan != null)
            {
                // DestroyImmediate, editörde ANINDA siler.
                // (Oyun içinde Destroy kullanılır; o, kareyi bitirince siler.)
                Object.DestroyImmediate(bulunan);
            }
        }
    }

    // ---------- ANA KURULUM ----------
    static void Kur()
    {
        // Tekrar eden birim: SAHANLIK + MERDİVEN.
        // Oyuncunun sahnenin kenarını görmemesi için birimden fazlasını kuruyoruz:
        // altta bir merdiven, ortada sahanlık A, üstünde merdiven, sonra sahanlık B,
        // en üstte bir merdiven daha. Oyuncu yalnızca A ile B arasında yürür.

        GameObject sahanlikA = SahanlikKur("Sahanlik_A");
        sahanlikA.transform.position = Vector3.zero;   // Vector3.zero = (0, 0, 0)

        GameObject merdivenYukari = MerdivenKur("Merdiven_Yukari");
        merdivenYukari.transform.position = new Vector3(0f, 0f, SahanlikDerin / 2f);

        GameObject sahanlikB = SahanlikKur("Sahanlik_B");
        sahanlikB.transform.position = new Vector3(0f, DikeyOfset, IleriOfset);

        // Sahanlık A'nın altındaki iniş merdiveni: oyuncu arkasına baktığında
        // sahnenin bittiğini görmesin, aşağı inen basamaklar görsün.
        GameObject merdivenAsagi = MerdivenKur("Merdiven_Asagi");
        merdivenAsagi.transform.position =
            new Vector3(0f, -DikeyOfset, -(SahanlikDerin / 2f + MerdivenUzunluk));

        // Sahanlık B'nin arkasındaki çıkış merdiveni: oyuncu yukarı bakınca
        // merdivenin devam ettiğini görsün. Bu merdivene hiç basılmaz.
        GameObject merdivenYukari2 = MerdivenKur("Merdiven_Yukari2");
        merdivenYukari2.transform.position =
            new Vector3(0f, DikeyOfset, SahanlikDerin / 2f + IleriOfset);

        // En üstteki merdivenin bittiği yerde çatı bölümü: kapalı bir kapı,
        // arkasında küçük bir çatı düzlüğü. Kapı kapalıyken gökyüzünü de kapatır,
        // yani eski "Kapak_Ust" duvarının işini de görür.
        CatiBolumuKur(new Vector3(0f, DikeyOfset * 2f,
                                  SahanlikDerin / 2f + IleriOfset + MerdivenUzunluk));

        // Alt uçtaki kapak: aşağı bakıldığında sahnenin dibi görünmesin.
        KapakKur("Kapak_Alt",
            new Vector3(0f, -DikeyOfset + 1.5f,
                        -(SahanlikDerin / 2f + MerdivenUzunluk + DuvarKalin / 2f)));

        // Kamerayı göz hizasına al — AMA sadece başıboşsa.
        // Kamera artık Oyuncu'nun çocuğuysa ona dokunmayız, yoksa
        // graybox'ı her yeniden kurduğumuzda oyuncunun kafasını yerinden oynatırız.
        if (Camera.main != null && Camera.main.transform.parent == null)
        {
            Camera.main.transform.position = new Vector3(0f, 1.7f, -0.6f);
            Camera.main.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
        }

        // Konsola özet yaz: sayıların beklediğimiz gibi çıktığını gözle doğrularız.
        Debug.Log($"Graybox kuruldu. İleri ofset: {IleriOfset:0.00} m, dikey ofset: {DikeyOfset:0.00} m, "
                + $"basamak: {BasamakSayisi} x {BasamakYuksek:0.0000} m");
    }

    // ---------- SAHANLIK ----------
    // Tek bir sahanlığı kurar ve kök nesnesini geri döndürür.
    // Anomali sistemi (#5) ileride tam olarak bu ağacın içine bakacak,
    // o yüzden parça isimleri (Kapi_Sol, Paspas_Sag, Ampul) baştan sabit tutuluyor.
    static GameObject SahanlikKur(string ad)
    {
        // Boş bir GameObject: kendisi görünmez, sadece parçaları bir arada tutan klasör gibidir.
        GameObject kok = new GameObject(ad);
        Transform t = kok.transform;   // Transform = nesnenin konum/dönüş/ölçek bilgisi

        // Zemin. Üst yüzeyi tam y=0'da olsun diye kalınlığın yarısı kadar aşağı itilir.
        Kutu("Zemin", t,
             new Vector3(0f, -DuvarKalin / 2f, 0f),
             new Vector3(SahanlikGen, DuvarKalin, SahanlikDerin));

        // SOL DUVAR — daire kapıları burada.
        // Duvarı tek parça yapıp kapıları yüzüne yapıştırmak YANLIŞTI: kapılar
        // duvardan dışarı taşan ince levhalar gibi görünüyordu. Gerçek bir kapı
        // duvarın İÇİNE oturur. Bu yüzden duvarı kapı boşlukları bırakarak
        // dört parça halinde kuruyoruz: iki kenar, ortadaki ayak ve üstteki lento.
        float duvarX = -(SahanlikGen / 2f + DuvarKalin / 2f);

        // Kapı boşluklarının z sınırları.
        float kapi1Bas = -KapiAraZ - KapiGen / 2f;   // arka kapının başlangıcı
        float kapi1Bit = -KapiAraZ + KapiGen / 2f;
        float kapi2Bas =  KapiAraZ - KapiGen / 2f;
        float kapi2Bit =  KapiAraZ + KapiGen / 2f;
        float duvarUcu =  SahanlikDerin / 2f;

        // Arka kenar parçası.
        float kenarA = kapi1Bas - (-duvarUcu);
        Kutu("Duvar_Sol_Kenar_A", t,
             new Vector3(duvarX, KapiYuk / 2f, (-duvarUcu + kapi1Bas) / 2f),
             new Vector3(DuvarKalin, KapiYuk, kenarA));

        // İki kapının arasındaki ayak.
        Kutu("Duvar_Sol_Ayak", t,
             new Vector3(duvarX, KapiYuk / 2f, (kapi1Bit + kapi2Bas) / 2f),
             new Vector3(DuvarKalin, KapiYuk, kapi2Bas - kapi1Bit));

        // Ön kenar parçası.
        float kenarB = duvarUcu - kapi2Bit;
        Kutu("Duvar_Sol_Kenar_B", t,
             new Vector3(duvarX, KapiYuk / 2f, (kapi2Bit + duvarUcu) / 2f),
             new Vector3(DuvarKalin, KapiYuk, kenarB));

        // Lento: kapıların üstünü tavana kadar kapatan parça.
        Kutu("Duvar_Sol_Lento", t,
             new Vector3(duvarX, (KapiYuk + DuvarYuksek) / 2f, 0f),
             new Vector3(DuvarKalin, DuvarYuksek - KapiYuk, SahanlikDerin));

        // Sağ duvar: sağır duvar.
        Kutu("Duvar_Sag", t,
             new Vector3(SahanlikGen / 2f + DuvarKalin / 2f, DuvarYuksek / 2f, 0f),
             new Vector3(DuvarKalin, DuvarYuksek, SahanlikDerin));

        // Tavan. Bunu koymazsak oyuncu yukarı bakınca gökyüzünü görür ve
        // "bir binanın içindeyim" hissi anında kaybolur.
        Kutu("Tavan", t,
             new Vector3(0f, DuvarYuksek + DuvarKalin / 2f, 0f),
             new Vector3(SahanlikGen + 2f * DuvarKalin, DuvarKalin, SahanlikDerin));

        // İki daire kapısı — duvarın içine, yüzünden 6 cm geride.
        // Bu geri çekilme kapı kasası etkisi yaratır; kapı artık duvara
        // yapıştırılmış bir levha değil, duvarın içindeki bir boşluk gibi okunur.
        KapiKur("Kapi_Sol", t, -KapiAraZ);
        KapiKur("Kapi_Sag", t,  KapiAraZ);

        // Paspaslar. Kapının TAM ÖNÜNDE ve duvara dayalı olmalı;
        // önceki hallerinde duvardan 40 cm uzakta, odanın ortasında duruyorlardı.
        PaspasKur("Paspas_Sol", t, -KapiAraZ);
        PaspasKur("Paspas_Sag", t,  KapiAraZ);

        // Ayakkabılık: Türk apartmanı kimliğinin en ucuz işareti.
        Kutu("Ayakkabilik", t, new Vector3(1.05f, 0.3f, 0f), new Vector3(0.35f, 0.6f, 0.8f));

        // Ampul. "Yeşil ampul" anomalisi ileride sadece bu ışığın rengini değiştirecek.
        AmpulKur("Ampul", t, new Vector3(0f, DuvarYuksek - 0.25f, 0f), 9f, 2.2f, golgeAtsin: true);

        return kok;
    }

    // ---------- MERDİVEN ----------
    // Yerel orijini merdivenin EN ALT ÖN kenarıdır; +z yönünde yukarı çıkar.
    static GameObject MerdivenKur(string ad)
    {
        GameObject kok = new GameObject(ad);
        Transform t = kok.transform;

        // Her basamak, zeminden kendi üst kotuna kadar uzanan dolu bir bloktur.
        // Graybox için en sade ve en sağlam yöntem budur.
        for (int i = 0; i < BasamakSayisi; i++)
        {
            float ustKot = (i + 1) * BasamakYuksek;   // bu basamağın üst yüzeyinin kotu

            Kutu($"Basamak_{(i + 1):00}", t,
                 new Vector3(0f, ustKot / 2f, i * BasamakDerinlik + BasamakDerinlik / 2f),
                 new Vector3(SahanlikGen, ustKot, BasamakDerinlik));
        }

        // Merdiven boyunca yükselen yan duvarlar.
        float duvarH = KatYuksekligi + DuvarYuksek;
        Kutu("Duvar_Sol", t,
             new Vector3(-(SahanlikGen / 2f + DuvarKalin / 2f), duvarH / 2f, MerdivenUzunluk / 2f),
             new Vector3(DuvarKalin, duvarH, MerdivenUzunluk));
        Kutu("Duvar_Sag", t,
             new Vector3(SahanlikGen / 2f + DuvarKalin / 2f, duvarH / 2f, MerdivenUzunluk / 2f),
             new Vector3(DuvarKalin, duvarH, MerdivenUzunluk));

        // Eğimli tavan. Yüksekliği, alttaki ve üstteki sahanlık tavanlarını
        // TAM olarak birleştirecek şekilde seçildi; arada boşluk kalırsa gökyüzü sızar.
        float egimAci = Mathf.Atan2(KatYuksekligi, MerdivenUzunluk) * Mathf.Rad2Deg;
        float egimBoy = Mathf.Sqrt(KatYuksekligi * KatYuksekligi + MerdivenUzunluk * MerdivenUzunluk);

        GameObject soffit = Kutu("Tavan_Egimli", t,
            new Vector3(0f, KatYuksekligi / 2f + DuvarYuksek + DuvarKalin / 2f, MerdivenUzunluk / 2f),
            new Vector3(SahanlikGen + 2f * DuvarKalin, DuvarKalin, egimBoy + 0.6f));

        // Quaternion.Euler: açıları dereceyle verip dönüş üretir.
        // Eksi işaret, kutunun +z ucunu YUKARI kaldırır.
        soffit.transform.localRotation = Quaternion.Euler(-egimAci, 0f, 0f);

        // Merdiven boşluğuna zayıf bir ampul: basamaklar tamamen karanlıkta kalmasın.
        AmpulKur("Ampul_Merdiven", t,
                 new Vector3(0f, KatYuksekligi / 2f + 1.9f, MerdivenUzunluk / 2f), 8f, 1.4f,
                 golgeAtsin: false);

        return kok;
    }

    // ---------- KÜÇÜK YARDIMCILAR ----------

    // Adı, ebeveyni, yerel konumu ve ölçeği verilen bir küp üretir.
    static GameObject Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 olcek)
    {
        // CreatePrimitive hazır bir küp yapar: mesh, materyal ve çarpışma kutusu dahil.
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ad;

        // SetParent(..., false): "yerel konumunu koru" demektir.
        // true verilseydi Unity dünya konumunu korumaya çalışıp değerleri bozardı.
        g.transform.SetParent(ebeveyn, false);

        g.transform.localPosition = konum;
        g.transform.localScale = olcek;
        return g;
    }

    // Nokta ışık üretir. 1994 akkor ampulünün sıcak sarısı.
    //
    // GÖLGE PAHALIDIR. Bir nokta ışık her yöne yayıldığı için gölgesini
    // ALTI ayrı yüzeye çizmek gerekir; yani gölge atan her ampul, sahneyi
    // altı kez daha çizdirir. Başta beş ampulün beşi de gölge atıyordu:
    // 30 gölge haritası. Unity bunları 2048'lik atlasa sığdıramayıp
    // çözünürlüğü dörtte bire düşürdüğünü konsola yazıyordu.
    // Artık sadece sahanlık ampulleri gölge atıyor; merdiven ampulleri
    // yalnızca aydınlatıyor. Gözle fark edilmiyor, maliyet yarıdan aza indi.
    static void AmpulKur(string ad, Transform ebeveyn, Vector3 konum,
                         float menzil, float siddet, bool golgeAtsin)
    {
        GameObject g = new GameObject(ad);
        g.transform.SetParent(ebeveyn, false);
        g.transform.localPosition = konum;

        // AddComponent: boş nesneye bir yetenek takar. Burada "ışık yayma" yeteneği.
        Light isik = g.AddComponent<Light>();
        isik.type = LightType.Point;                    // her yöne yayılan ampul
        isik.range = menzil;                            // ışığın ulaştığı yarıçap
        isik.intensity = siddet;
        isik.color = new Color(1f, 0.92f, 0.76f);       // sıcak sarı
        isik.shadows = golgeAtsin ? LightShadows.Soft : LightShadows.None;
    }

    // ---------- DAİRE KAPISI ----------
    // Kapı, kasa, kol ve numaralık. Numaralık şimdiden konuyor çünkü
    // "ters kapı numarası" en temel FISILTI anomalisi ve bir yere ihtiyacı var.
    static void KapiKur(string ad, Transform ebeveyn, float z)
    {
        GameObject kapi = new GameObject(ad);
        kapi.transform.SetParent(ebeveyn, false);
        kapi.transform.localPosition = new Vector3(0f, 0f, z);
        Transform k = kapi.transform;

        float duvarYuzu = -(SahanlikGen / 2f);          // duvarın iç yüzü
        float kanatX    = duvarYuzu - 0.09f;            // kanat 6 cm geride

        // Kapı kanadı.
        Kutu("Kanat", k, new Vector3(kanatX, KapiYuk / 2f, 0f),
             new Vector3(0.06f, KapiYuk, KapiGen));

        // Kol: kapının menteşesiz tarafında, bel hizasında.
        Kutu("Kol", k, new Vector3(kanatX + 0.07f, 1.05f, KapiGen / 2f - 0.12f),
             new Vector3(0.09f, 0.04f, 0.12f));

        // Numaralık: göz hizasının biraz üstünde, küçük bir plaka.
        Kutu("Numaralik", k, new Vector3(kanatX + 0.04f, 1.78f, 0f),
             new Vector3(0.02f, 0.16f, 0.26f));

        // NUMARA YAZISI. Plakanın üstünde duran 3B yazı.
        // KatYoneticisi her katta bu yazıyı günceller: 1. katta 1 ve 2,
        // 2. katta 3 ve 4... Böylece oyuncu kaçıncı katta olduğunu
        // ekranda bir sayaçtan değil, BİNANIN KENDİSİNDEN okur.
        GameObject yaziNesne = new GameObject("Numara");
        yaziNesne.transform.SetParent(k, false);
        yaziNesne.transform.localPosition = new Vector3(kanatX + 0.06f, 1.78f, 0f);

        // Kapı +x yönüne bakıyor; yazıyı da o yöne çeviriyoruz.
        yaziNesne.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        // TextMesh: Unity'nin en basit 3B yazı bileşeni. Ekstra paket
        // veya font kurulumu istemediği için graybox'a uygun.
        TextMesh yazi = yaziNesne.AddComponent<TextMesh>();
        yazi.text = "0";
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.fontSize = 96;              // büyük üret, sonra küçült: kenarlar keskin çıkar
        yazi.characterSize = 0.014f;   // rakam plakaya sığacak boyut
        yazi.color = new Color(0.12f, 0.12f, 0.12f);
        yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Yazının kendi materyali fontun dokusunu kullanmalı.
        yaziNesne.GetComponent<MeshRenderer>().sharedMaterial = yazi.font.material;
    }

    // ---------- PASPAS ----------
    // Kapının tam önünde, duvara dayalı.
    static void PaspasKur(string ad, Transform ebeveyn, float z)
    {
        const float PaspasEn  = 0.62f;   // duvardan içeri doğru uzunluk
        const float PaspasBoy = 0.45f;   // kapı boyunca genişlik

        float duvarYuzu = -(SahanlikGen / 2f);

        Kutu(ad, ebeveyn,
             new Vector3(duvarYuzu + PaspasEn / 2f, 0.015f, z),
             new Vector3(PaspasEn, 0.03f, PaspasBoy));
    }

    // ---------- ÇATI BÖLÜMÜ ----------
    // Son merdivenin bittiği noktaya bir kapı duvarı ve arkasına küçük bir çatı düzlüğü kurar.
    // "konum" = son merdivenin üst basamağının bittiği yer (zemin kotu).
    //
    // Kapı KAPALI başlar ve gökyüzünü kapatır. 8. kat hatasız geçildiğinde
    // KatYoneticisi "CatiKapisi" nesnesini kapatır; kapı açılır, sabah görünür.
    static void CatiBolumuKur(Vector3 konum)
    {
        GameObject kok = new GameObject("CatiBolumu");
        kok.transform.position = konum;
        Transform t = kok.transform;

        const float KapiGen  = 1.0f;    // kapı boşluğunun genişliği
        const float KapiYuk  = 2.05f;   // kapı boşluğunun yüksekliği
        const float CatiBoy  = 3.4f;    // çatı düzlüğünün derinliği
        const float Parapet  = 1.0f;    // çatı korkuluğunun yüksekliği

        float duvarZ = DuvarKalin / 2f;                       // kapı duvarının yeri
        float yanGen = (SahanlikGen - KapiGen) / 2f;          // kapının iki yanındaki dolu kısım

        // Kapı duvarının sol ve sağ parçaları.
        Kutu("Duvar_Sol", t,
             new Vector3(-(KapiGen / 2f + yanGen / 2f), KapiYuk / 2f, duvarZ),
             new Vector3(yanGen, KapiYuk, DuvarKalin));
        Kutu("Duvar_Sag", t,
             new Vector3(KapiGen / 2f + yanGen / 2f, KapiYuk / 2f, duvarZ),
             new Vector3(yanGen, KapiYuk, DuvarKalin));

        // Kapının üstünü ve merdiven boşluğunun tepesini kapatan parça.
        Kutu("Duvar_Ust", t,
             new Vector3(0f, KapiYuk + 2.5f, duvarZ),
             new Vector3(SahanlikGen + 2f * DuvarKalin, 5f, DuvarKalin));

        // KAPI. Açılması = bu nesnenin kapatılması.
        Kutu("CatiKapisi", t,
             new Vector3(0f, KapiYuk / 2f, duvarZ),
             new Vector3(KapiGen, KapiYuk, DuvarKalin * 0.6f));

        // Çatı düzlüğü. Üstü açık: kapı açılınca gökyüzü görünür.
        Kutu("Cati_Zemin", t,
             new Vector3(0f, -DuvarKalin / 2f, DuvarKalin + CatiBoy / 2f),
             new Vector3(SahanlikGen + 3f, DuvarKalin, CatiBoy));

        // Korkuluklar: oyuncu çatıdan düşmesin.
        float yariGen = (SahanlikGen + 3f) / 2f;
        Kutu("Parapet_Sol", t,
             new Vector3(-yariGen, Parapet / 2f, DuvarKalin + CatiBoy / 2f),
             new Vector3(DuvarKalin, Parapet, CatiBoy));
        Kutu("Parapet_Sag", t,
             new Vector3(yariGen, Parapet / 2f, DuvarKalin + CatiBoy / 2f),
             new Vector3(DuvarKalin, Parapet, CatiBoy));
        Kutu("Parapet_On", t,
             new Vector3(0f, Parapet / 2f, DuvarKalin + CatiBoy),
             new Vector3(SahanlikGen + 3f, Parapet, DuvarKalin));
    }

    // Sahnenin ucunu kapatan büyük duvar.
    static void KapakKur(string ad, Vector3 konum)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ad;
        g.transform.position = konum;
        g.transform.localScale = new Vector3(SahanlikGen + 2f * DuvarKalin, 8f, DuvarKalin);
    }
}
