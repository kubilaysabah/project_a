// GrayboxKurucu_Daire.cs
// 3+1 dairenin kabuğunu kuran kısım (#53): odalar, duvarlar, oda kapıları, pencereler.
// GrayboxKurucu'nun parçasıdır ("partial"); Kutu, malzeme yardımcıları ve ölçüler ortak.
//
// ---------- NE KURULUYOR ----------
// Planın kendisi Assets/Scripts/Cekirdek/DairePlani.cs'de (odalar ve açıklıklar, testli).
// Bu dosya o listeyi dolaşıp her oda için döşeme ve dört duvar, her açıklık için kapı ya da
// pencere kurar. Sahanlık A'da dört daire kurulur (Daire_1 ... Daire_4), hepsi KAPALI
// (SetActive false): oyun sırasında KatYoneticisi o katta açık olan daireyi açar.
//
// ---------- AYNALAMA ----------
// Planın u (derinlik) ve v (duvar boyu) eksenleri her daire için dünyada başka yöne bakar:
//   Daire 1: sol duvar, yakın kapı  -> u: -x, v: +z (merdivene doğru)
//   Daire 2: sol duvar, uzak kapı   -> u: -x, v: -z (ön cepheye doğru)
//   Daire 3: sağ duvar, uzak kapı   -> u: +x, v: -z
//   Daire 4: sağ duvar, yakın kapı  -> u: +x, v: +z
// Aynayı NESNEYİ -1 ile ölçekleyerek değil, her noktayı hesaplayarak yapıyoruz. Eksi ölçekli
// nesnelerde yazılar ters okunur ve çarpışma kutuları sorun çıkarabilir; ileride dairelere
// takvim, fatura gibi yazılı eşyalar gelecek.
//
// ---------- DUVARLARI HER ODA KENDİSİ KURAR ----------
// İki oda arasındaki 10 cm'lik bölme, iki odanın kurduğu 5'er cm'lik iki dilimden oluşur.
// Böylece her dilim kendi odasının adını taşır ve ileride odaya göre farklı kaplanabilir
// (salonda duvar kâğıdı, banyoda fayans). Kapı boşluğu iki dilimde de aynı yerde açılır.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MekanOlculeri;

public static partial class GrayboxKurucu
{
    // Sahanlık duvarının dairedeki yüzüne (u = 0) konan ince kaplamanın kalınlığı. Sahanlık
    // duvarı zaten orada; bu kaplama onun dairedeki yüzünü örter (iki yüz aynı düzlemde
    // üst üste binip titreşmesin diye kaplama oda tarafında durur).
    const float SahanlikKaplama = 0.03f;

    // Oda kapısı kasasının ölçüleri.
    const float KasaGen = 0.06f;     // kasa pervazının genişliği
    const float KasaDerin = 0.14f;   // bölme duvarından (10 cm) her yana 2 cm taşar
    const float KanatKalin = 0.04f;

    // ---------- DAİRENİN YÖNÜ ----------
    // Planın (u, y, v) noktasını sahanlığın yerel (x, y, z) noktasına çevirir.
    // "struct": küçük, değer olarak taşınan bir veri paketi (yon, sv, kapıZ üçlüsü).
    struct DaireYonu
    {
        public float yon;    // +1 sağ duvar, -1 sol duvar
        public float sv;     // v'nin z'deki işareti: yakın daire +1, uzak daire -1
        public float kapiZ;  // giriş kapısının z'si

        public Vector3 Nokta(float u, float y, float v)
        {
            return new Vector3(yon * (SahanlikGen / 2f + DuvarKalin + u), y, kapiZ + sv * v);
        }

        // Plandaki bir yönün (du, dv) sahanlıktaki karşılığı (konum değil, yön).
        public Vector3 Yon(float du, float dv) { return new Vector3(yon * du, 0f, sv * dv); }
    }

    static DaireYonu DaireYonuBul(int numara)
    {
        bool sag = numara == 3 || numara == 4;
        bool yakin = numara == 1 || numara == 4;
        return new DaireYonu
        {
            yon = sag ? 1f : -1f,
            sv = yakin ? 1f : -1f,
            kapiZ = yakin ? YakinKapiZ : UzakKapiZ,
        };
    }

    // Plandaki bir kutuyu (u0..u1, y0..y1, v0..v1) sahanlığa kurar. Aynalanmış dairede
    // köşeler yer değiştirebilir; merkez ve mutlak boyut her durumda doğrudur.
    static GameObject DaireKutu(string ad, Transform ebeveyn, DaireYonu d,
                                float u0, float u1, float y0, float y1, float v0, float v1)
    {
        Vector3 a = d.Nokta(u0, y0, v0), b = d.Nokta(u1, y1, v1);
        Vector3 boyut = new Vector3(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y), Mathf.Abs(b.z - a.z));
        return Kutu(ad, ebeveyn, (a + b) / 2f, boyut);
    }

    // ---------- DÖRT DAİRE ----------
    static void DaireleriKur(Transform sahanlik)
    {
        for (int n = 1; n <= DaireSayisi; n++)
        {
            GameObject daire = new GameObject($"Daire_{n}");
            daire.transform.SetParent(sahanlik, false);
            DaireKur(daire.transform, DaireYonuBul(n));
            // Kapalı kurulur: kayıtlı sahnede dört daire de gizli, kapılar kapalı.
            daire.SetActive(false);
        }
    }

    static void DaireKur(Transform daire, DaireYonu d)
    {
        foreach (DairePlani.Oda oda in DairePlani.Odalar)
            OdaKur(daire, d, oda);

        // Tavan: bütün dairenin üstünde tek parça. Üst yüzü 2,95'te; B sahanlığının daire
        // holleri 2,9-3,0 arasında duruyor, üst yüzler aynı düzleme denk gelip titreşmesin.
        DaireKutu("Daire_Tavan", daire, d, 0f, DairePlani.UMax + DairePlani.DisDuvar,
                  DairePlani.TavanYuk, 2.95f,
                  DairePlani.VMin - DairePlani.DisDuvar, DairePlani.VMax + DairePlani.DisDuvar);

        GameObject kapilar = new GameObject("Kapilar");
        kapilar.transform.SetParent(daire, false);
        GameObject pencereler = new GameObject("Pencereler");
        pencereler.transform.SetParent(daire, false);

        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
        {
            if (a.Tur == DairePlani.AciklikTuru.Kapi)
                OdaKapisiKur(kapilar.transform, d, a, cam: false);
            else if (a.Tur == DairePlani.AciklikTuru.BalkonKapisi)
                OdaKapisiKur(kapilar.transform, d, a, cam: true);
            else if (a.Tur == DairePlani.AciklikTuru.Pencere)
                DairePenceresiKur(pencereler.transform, d, a);
            // Giriş: kanadı sahanlıktaki daire kapısı. Geçit: kapısız. İkisinde de yalnızca
            // duvardaki boşluk var; onu OdaKur açtı.
        }
    }

    // ---------- ODA: DÖŞEME VE DÖRT DUVAR ----------
    // Kenar: 0 u0, 1 u1, 2 v0, 3 v1.
    static void OdaKur(Transform daire, DaireYonu d, DairePlani.Oda oda)
    {
        GameObject kok = new GameObject(oda.Ad);
        kok.transform.SetParent(daire, false);
        Transform t = kok.transform;

        // Her kenarın duvar kalınlığı: komşu oda varsa bölmenin yarısı, yoksa dış duvar;
        // sahanlık tarafı (u = 0) ince kaplama.
        float[] kalin = new float[4];
        for (int k = 0; k < 4; k++) kalin[k] = KenarKalinligi(oda, k);

        // DÖŞEME: odanın içi artı duvarlarının altı (bölmelerin altında boşluk kalmasın).
        // Sahanlık tarafında uzatılmaz: orada sahanlığın zemini başlıyor, iki üst yüz
        // aynı düzlemde üst üste binerse kapı eşiğinde titreşirdi.
        float du0 = oda.U0 <= 0.001f ? 0f : kalin[0];
        string zemin = oda.Islak ? "Daire_Fayans" : "Daire_Parke";
        DaireKutu(zemin, t, d, oda.U0 - du0, oda.U1 + kalin[1], -DuvarKalin, 0f,
                  oda.V0 - kalin[2], oda.V1 + kalin[3]);

        string duvarAdi = oda.Islak ? "Daire_Fayans_Duvar" : "Daire_Duvar";
        for (int k = 0; k < 4; k++)
            KenarDuvariKur(t, d, oda, k, kalin[k], duvarAdi);
    }

    static float KenarKalinligi(DairePlani.Oda oda, int kenar)
    {
        if (kenar == 0 && oda.U0 <= 0.001f) return SahanlikKaplama;
        return KomsuVar(oda, kenar) ? DairePlani.IcDuvar / 2f : DairePlani.DisDuvar;
    }

    // Bu kenarın karşısında (bölme mesafesinde) başka bir oda var mı?
    static bool KomsuVar(DairePlani.Oda oda, int kenar)
    {
        foreach (DairePlani.Oda o in DairePlani.Odalar)
        {
            if (o.Ad == oda.Ad) continue;
            bool karsida, ortusuyor;
            if (kenar <= 1)
            {
                float cizgi = kenar == 0 ? oda.U0 : oda.U1;
                float karsi = kenar == 0 ? o.U1 : o.U0;
                karsida = Mathf.Abs(cizgi - karsi) < 0.15f;
                ortusuyor = o.V0 < oda.V1 && oda.V0 < o.V1;
            }
            else
            {
                float cizgi = kenar == 2 ? oda.V0 : oda.V1;
                float karsi = kenar == 2 ? o.V1 : o.V0;
                karsida = Mathf.Abs(cizgi - karsi) < 0.15f;
                ortusuyor = o.U0 < oda.U1 && oda.U0 < o.U1;
            }
            if (karsida && ortusuyor) return true;
        }
        return false;
    }

    // Bir kenarın duvarı: kenar boyunca dilimler, aralarında açıklıklar (kapı, pencere, geçit).
    static void KenarDuvariKur(Transform t, DaireYonu d, DairePlani.Oda oda, int kenar, float kalin, string ad)
    {
        bool uKenari = kenar <= 1;   // u sabit, duvar v boyunca uzanıyor
        float yuz = kenar == 0 ? oda.U0 : kenar == 1 ? oda.U1 : kenar == 2 ? oda.V0 : oda.V1;
        // Duvar dilimi odanın DIŞINA doğru durur (u0/v0 kenarında eksi yöne, u1/v1'de artı).
        // Sahanlık kaplaması istisna: sahanlık duvarının dairedeki yüzünün önünde, odanın içinde.
        float ic, dis;
        if (kenar == 0 && oda.U0 <= 0.001f) { ic = yuz + kalin; dis = yuz; }
        else if (kenar == 0 || kenar == 2) { ic = yuz; dis = yuz - kalin; }
        else { ic = yuz; dis = yuz + kalin; }
        float d0 = Mathf.Min(ic, dis), d1 = Mathf.Max(ic, dis);

        // Kenar boyunca uzunluk: köşelerde boşluk kalmasın diye iki uçtan duvar kalınlığı
        // kadar uzatılır. Komşu kenarın dilimiyle köşede üst üste biner; ikisi de aynı dokuyu
        // dünya koordinatından aldığı için örtüşen yüzeyler aynı görünür.
        float bas = (uKenari ? oda.V0 : oda.U0) - kalin;
        float son = (uKenari ? oda.V1 : oda.U1) + kalin;

        // Bu kenara düşen açıklıklar: aynı eksende, çizgisi kenara yakın, ortası kenar boyunca.
        var bosluklar = new List<float[]>();   // {başlangıç, bitiş, alt, üst}
        foreach (DairePlani.Aciklik a in DairePlani.Acikliklar)
        {
            if (a.UDuvari != uKenari) continue;
            if (Mathf.Abs(a.Cizgi - yuz) > 0.15f) continue;
            if (a.Merkez <= bas || a.Merkez >= son) continue;
            bosluklar.Add(Bosluk(a));
        }
        // Boşlukları kenar boyunca sıraya diz (küçükten büyüğe). "Sort" listeyi yerinde sıralar;
        // parantez içi iki boşluğun hangisinin önce geleceğini söyleyen kısa bir fonksiyon.
        bosluklar.Sort((x, y) => x[0].CompareTo(y[0]));

        float h = DairePlani.TavanYuk;
        float imlec = bas;
        foreach (float[] b in bosluklar)
        {
            if (b[0] > imlec + 0.001f) Dilim(ad, t, d, uKenari, d0, d1, imlec, b[0], 0f, h);
            if (b[2] > 0.001f) Dilim(ad, t, d, uKenari, d0, d1, b[0], b[1], 0f, b[2]);        // pencere altı
            if (b[3] < h - 0.001f) Dilim(ad, t, d, uKenari, d0, d1, b[0], b[1], b[3], h);     // lento
            imlec = b[1];
        }
        if (son > imlec + 0.001f) Dilim(ad, t, d, uKenari, d0, d1, imlec, son, 0f, h);
    }

    // Açıklığın duvarda kestiği boşluk. Kapıda kasa boşluğun İÇİNE değil KENARINA oturur:
    // boşluk kanattan her yana kasa kadar geniş kesilir, kasa 1 cm duvara gömülür (kasanın
    // dış yüzü duvarın ucuyla aynı düzleme denk gelip titreşmesin). Lento kasanın başlığının
    // 2 cm içinden başlar (alt yüzleri üst üste binmesin).
    static float[] Bosluk(DairePlani.Aciklik a)
    {
        float yari = a.Gen / 2f;
        switch (a.Tur)
        {
            case DairePlani.AciklikTuru.Kapi:
            case DairePlani.AciklikTuru.BalkonKapisi:
                float pay = KasaGen - 0.01f;
                return new[] { a.Merkez - yari - pay, a.Merkez + yari + pay, 0f, a.Ust + 0.02f };
            case DairePlani.AciklikTuru.Pencere:
                return new[] { a.Merkez - yari - 0.01f, a.Merkez + yari + 0.01f, a.Alt - 0.01f, a.Ust + 0.01f };
            default:   // giriş ve geçit: kasasız, tam ölçü
                return new[] { a.Merkez - yari, a.Merkez + yari, a.Alt, a.Ust };
        }
    }

    // Bir duvar dilimi: kalınlık yönünde d0..d1, kenar boyunca s0..s1, yükseklikte y0..y1.
    static void Dilim(string ad, Transform t, DaireYonu d, bool uKenari,
                      float d0, float d1, float s0, float s1, float y0, float y1)
    {
        if (uKenari) DaireKutu(ad, t, d, d0, d1, y0, y1, s0, s1);
        else DaireKutu(ad, t, d, s0, s1, y0, y1, d0, d1);
    }

    // Açıklığın plandaki bir noktası: duvar boyunca m, kalınlıkta c (çizgiden uzaklık).
    static Vector3 AciklikNoktasi(DaireYonu d, DairePlani.Aciklik a, float m, float y, float c)
    {
        return a.UDuvari ? d.Nokta(a.Cizgi + c, y, m) : d.Nokta(m, y, a.Cizgi + c);
    }

    // Açıklığa bağlı bir kutu: duvar boyunca m0..m1, kalınlıkta c0..c1 (çizgiye göre).
    static GameObject AciklikKutusu(string ad, Transform ebeveyn, DaireYonu d, DairePlani.Aciklik a,
                                    float m0, float m1, float y0, float y1, float c0, float c1)
    {
        return a.UDuvari
            ? DaireKutu(ad, ebeveyn, d, a.Cizgi + c0, a.Cizgi + c1, y0, y1, m0, m1)
            : DaireKutu(ad, ebeveyn, d, m0, m1, y0, y1, a.Cizgi + c0, a.Cizgi + c1);
    }

    // ---------- ODA KAPISI ----------
    // Kök nesne (Kapilar/Kapi_Banyo gibi) IcKapi bileşenini taşır. Altında kasa ve menteşe;
    // menteşenin altında kanat ve iki kol. Menteşe açıklığın küçük uçunda (m0); kanat oradan
    // büyük uca uzanır ve odanın içine doğru döner.
    static void OdaKapisiKur(Transform ebeveyn, DaireYonu d, DairePlani.Aciklik a, bool cam)
    {
        float m0 = a.Merkez - a.Gen / 2f, m1 = a.Merkez + a.Gen / 2f;
        float ust = a.Ust;

        GameObject kok = new GameObject(a.Ad);
        kok.transform.SetParent(ebeveyn, false);
        Transform k = kok.transform;
        k.localPosition = AciklikNoktasi(d, a, a.Merkez, 0f, 0f);

        // KASA: iki dikme ve başlık. Konumlar kökün içinde ama hesap sahanlık koordinatında
        // yapılıyor; bu yüzden kutuyu önce sahanlığa kurup sonra köke taşıyoruz
        // (SetParent(..., true): dünyadaki yerini koruyarak).
        float ky = ust + 0.05f;
        Tasi(AciklikKutusu("Ic_Kasa", k.parent, d, a, m0 - KasaGen, m0, 0f, ky, -KasaDerin / 2f, KasaDerin / 2f), k);
        Tasi(AciklikKutusu("Ic_Kasa", k.parent, d, a, m1, m1 + KasaGen, 0f, ky, -KasaDerin / 2f, KasaDerin / 2f), k);
        Tasi(AciklikKutusu("Ic_Kasa", k.parent, d, a, m0 - KasaGen, m1 + KasaGen, ust, ky, -KasaDerin / 2f, KasaDerin / 2f), k);

        // MENTEŞE: kanadın döndüğü nokta, açıklığın m0 ucunda, duvarın ortasında.
        GameObject menteseNesne = new GameObject("Mentese");
        menteseNesne.transform.SetParent(k, false);
        menteseNesne.transform.position = k.parent.TransformPoint(AciklikNoktasi(d, a, m0, 0f, 0f));
        Transform m = menteseNesne.transform;

        // Kanadın plandaki yönü (menteşeden serbest uca) ve odanın bulunduğu taraf.
        Vector3 kanatYonu = a.UDuvari ? d.Yon(0f, 1f) : d.Yon(1f, 0f);
        DairePlani.OdaBul(a.Oda, out DairePlani.Oda oda);
        float odaTarafi = a.UDuvari ? Mathf.Sign((oda.U0 + oda.U1) / 2f - a.Cizgi)
                                    : Mathf.Sign((oda.V0 + oda.V1) / 2f - a.Cizgi);
        Vector3 odaYonu = a.UDuvari ? d.Yon(odaTarafi, 0f) : d.Yon(0f, odaTarafi);

        // Unity'de y ekseni etrafında POZİTİF dönüş, ileri (+z) yönünü sağa (+x) çevirir;
        // bir yönü (x, z) pozitif dönüşün ittiği yön (z, -x)'tir. Serbest uç odaya doğru
        // gitmeliyse ve bu yön odaya bakıyorsa açı pozitif, bakmıyorsa negatif.
        Vector3 itis = new Vector3(kanatYonu.z, 0f, -kanatYonu.x);
        float isaret = Vector3.Dot(itis, odaYonu) > 0f ? 1f : -1f;

        float gen = a.Gen - 0.01f, boy = ust - 0.01f;
        Vector3 kanatMerkez = kanatYonu * (a.Gen / 2f) + Vector3.up * (boy / 2f);
        GameObject kanat;
        if (!cam)
        {
            kanat = Kutu("Ic_Kanat", m, kanatMerkez, Boyut(kanatYonu, gen, boy, KanatKalin));
            // Kol: kanadın serbest ucuna yakın, iki yüzünde, 1 m yükseklikte.
            Vector3 dik = new Vector3(-kanatYonu.z, 0f, kanatYonu.x);   // kanadın yüzüne dik yön
            foreach (float yan in new[] { -1f, 1f })
                Kutu("Kol", m, kanatYonu * (a.Gen - 0.08f) + Vector3.up * 1.0f + dik * yan * 0.045f,
                     Boyut(kanatYonu, 0.12f, 0.03f, 0.04f));
        }
        else
        {
            // Balkon kapısı: boyalı çerçeve, ortasında cam. Çarpışmayı cam taşır.
            float c = 0.06f;
            Kutu("Cerceve", m, kanatYonu * (c / 2f) + Vector3.up * (boy / 2f), Boyut(kanatYonu, c, boy, KanatKalin));
            Kutu("Cerceve", m, kanatYonu * (a.Gen - c / 2f) + Vector3.up * (boy / 2f), Boyut(kanatYonu, c, boy, KanatKalin));
            Kutu("Cerceve", m, kanatYonu * (a.Gen / 2f) + Vector3.up * (c / 2f), Boyut(kanatYonu, gen, c, KanatKalin));
            Kutu("Cerceve", m, kanatYonu * (a.Gen / 2f) + Vector3.up * (boy - c / 2f), Boyut(kanatYonu, gen, c, KanatKalin));
            Kutu("Cerceve", m, kanatYonu * (a.Gen / 2f) + Vector3.up * 1.0f, Boyut(kanatYonu, gen, c, KanatKalin));
            kanat = Kutu("Pencere_Cam", m, kanatMerkez, Boyut(kanatYonu, gen - 2f * c, boy - 2f * c, 0.01f));
            kanat.GetComponent<MeshRenderer>().sharedMaterial = PencereCamMalzeme();
        }

        IcKapi bilesen = kok.AddComponent<IcKapi>();
        var seri = new SerializedObject(bilesen);
        seri.FindProperty("mentese").objectReferenceValue = m;
        seri.FindProperty("kanat").objectReferenceValue = kanat.transform;
        seri.FindProperty("acikAci").floatValue = 95f * isaret;
        seri.FindProperty("hepKilitli").boolValue = cam;
        seri.FindProperty("kilitli").boolValue = cam;
        seri.ApplyModifiedPropertiesWithoutUndo();
    }

    // Bir yönde "gen", yukarı "boy", ona dik yönde "kalin" olan kutunun ölçeği.
    // Yön ya x ya z ekseni boyunca olduğu için hangisinin genişlik olduğu yönün kendisinden okunur.
    static Vector3 Boyut(Vector3 yon, float gen, float boy, float kalin)
    {
        return Mathf.Abs(yon.x) > 0.5f ? new Vector3(gen, boy, kalin) : new Vector3(kalin, boy, gen);
    }

    static void Tasi(GameObject g, Transform yeniEbeveyn) { g.transform.SetParent(yeniEbeveyn, true); }

    // ---------- PENCERE ----------
    // Çerçeve (dört parça ve ortada bir kayıt), koyu cam ve içeride denizlik. Cam opak ve
    // parlaktır: gece dışarısı karanlık, camda yalnızca kafa lambasının parıltısı görünür.
    // Pencerenin arkasına bir şey kurmak gerekmiyor.
    static void DairePenceresiKur(Transform ebeveyn, DaireYonu d, DairePlani.Aciklik a)
    {
        GameObject kok = new GameObject(a.Ad);
        kok.transform.SetParent(ebeveyn, false);
        Transform p = kok.transform;

        float m0 = a.Merkez - a.Gen / 2f, m1 = a.Merkez + a.Gen / 2f;
        const float c = 0.06f;                 // çerçeve kalınlığı
        const float cd = 0.08f;                // çerçeve derinliği
        float ortaM = a.Merkez;

        // Çerçeve duvara 1 cm gömülü (boşluk 1 cm geniş kesildi).
        AciklikKutusu("Cerceve", p, d, a, m0 - 0.02f, m0 + c, a.Alt - 0.02f, a.Ust + 0.02f, -cd / 2f, cd / 2f);
        AciklikKutusu("Cerceve", p, d, a, m1 - c, m1 + 0.02f, a.Alt - 0.02f, a.Ust + 0.02f, -cd / 2f, cd / 2f);
        AciklikKutusu("Cerceve", p, d, a, m0 + c, m1 - c, a.Alt - 0.02f, a.Alt + c, -cd / 2f, cd / 2f);
        AciklikKutusu("Cerceve", p, d, a, m0 + c, m1 - c, a.Ust - c, a.Ust + 0.02f, -cd / 2f, cd / 2f);
        AciklikKutusu("Kayit", p, d, a, ortaM - 0.02f, ortaM + 0.02f, a.Alt + c, a.Ust - c, -0.03f, 0.03f);

        GameObject cam = AciklikKutusu("Pencere_Cam", p, d, a, m0 + c, m1 - c, a.Alt + c, a.Ust - c, -0.005f, 0.005f);
        cam.GetComponent<MeshRenderer>().sharedMaterial = PencereCamMalzeme();

        // Denizlik: odanın içine 6 cm taşan pencere tahtası. Oda duvarın hangi tarafında?
        DairePlani.OdaBul(a.Oda, out DairePlani.Oda oda);
        float taraf = a.UDuvari ? Mathf.Sign((oda.U0 + oda.U1) / 2f - a.Cizgi)
                                : Mathf.Sign((oda.V0 + oda.V1) / 2f - a.Cizgi);
        float icYuz = taraf * DairePlani.DisDuvar / 2f;    // duvarın oda tarafındaki yüzü
        float c0 = Mathf.Min(0f, icYuz + taraf * 0.06f), c1 = Mathf.Max(0f, icYuz + taraf * 0.06f);
        AciklikKutusu("Denizlik", p, d, a, m0 - 0.05f, m1 + 0.05f, a.Alt - 0.03f, a.Alt, c0, c1);
    }

    // Gece camı: koyu lacivert, çok parlak (kafa lambası camda parlasın).
    static Material PencereCamMalzeme()
    {
        const string Yol = "Assets/Materials/Pencere_Cam.mat";
        Material mevcut = AssetDatabase.LoadAssetAtPath<Material>(Yol);
        if (mevcut != null) return mevcut;
        Material yeni = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        yeni.SetColor("_BaseColor", new Color(0.01f, 0.012f, 0.02f));
        yeni.SetFloat("_Smoothness", 0.95f);
        AssetDatabase.CreateAsset(yeni, Yol);
        AssetDatabase.SaveAssets();
        return yeni;
    }
}
