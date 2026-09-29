// GrayboxKurucu_Mobilya.cs
// Her katın dairesine kendi mobilyasını koyar (#73). Mobilyalar Poly Haven'ın CC0 modelleri
// (MobilyaKurucu prefab'larını kurar); burada yalnızca HANGİ katta NEREYE ne konacağı yazıyor.
//
// Kat dosyasının "suslar" listesine eklenir (KatSusu): kat açılınca ZincirUygulayici kopyalar,
// kat kapanınca kaldırır. Yani mobilya da içerik = veridir; kat başına kod yok.
//
// KURALLAR (Şahan, 2026-09-27)
//   - Her kat kiracısına göre döşenir (Docs/hikaye.md §7): taşınan ailenin evi boş, Nuri'nin
//     evi klasik, çocuklu ailenin evi kalabalık, Yönetim dairesi ofis gibi.
//   - Yakın katlarda aynı eşya tekrar edilmez: kanepe, berjer, TV, TV dolabı ve avize komşu
//     iki katta hiçbir zaman aynı model değil (aşağıdaki tabloda görülür).
//   - Zincir yerlerinin (plan noktaları) ve kalıcı eşyaların (sofra, büfe, gardırop, dosya
//     dolabı...) üstüne konmaz. Ölçüler plan eksenlerinde: u landing duvarından içeri, v giriş
//     kapısından (DairePlani). Ön: modelin baktığı plan yönü (OnU, OnV).
//
// DUVAR EŞYALARI (ayna, çerçeve, saat) arka yüzü duvara değecek şekilde duvarın iç yüzüne,
// merkez yüksekliğiyle konur. Avizenin noktası tepesidir; tavana (2,80) asılır.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    const float Tavan = DairePlani.TavanYuk;

    static GameObject Mob(string ad)
    {
        GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(MobilyaKurucu.PrefabKlasoru + ad + ".prefab");
        if (g == null) Debug.LogWarning($"[MOBILYA] {ad}.prefab yok; önce Huzur → Mobilyaları Kur.");
        return g;
    }

    // Aile fotoğrafı (#69): dik fotoğraf masa çerçevesine, yatay fotoğraf duvar çerçevesine. Süsün
    // Metin'i fotoğrafın kimliği (ZincirUygulayici çerçeveye takar).
    static KatSusu Foto(string kimlik, bool duvar, float u, float y, float v, float onU, float onV)
    {
        return new KatSusu { Sablon = FotografKurucu.Sablon(duvar), Metin = kimlik, X = u, Y = y, Z = v, OnU = onU, OnV = onV };
    }

    // YatakA'nın "uzun" değeri: yatağın oda derinliği (u) boyunca kapladığı yer. Gotik yatak boyuna
    // yatar (2,04); gündüz yatağı duvara yaslı, yalnızca derinliği kadar (0,85).
    // Bir mobilya: plan noktası (u, yükseklik, v) ve ön yönü. yol "B:" ise ikinci dairede (#70).
    static KatSusu M(string ad, float u, float y, float v, float onU, float onV, string yol = "")
    {
        return new KatSusu { Sablon = Mob(ad), Yol = yol, X = u, Y = y, Z = v, OnU = onU, OnV = onV };
    }

    // ---------- ORTAK YERLER ----------
    // Salon: kanepe odalar arası duvarın önünde (u = 5), yüzü salona; konsol/TV hol duvarında
    // (v = 4,75), yüzü pencereye; berjer pencere köşesinde; avize sofranın üstünde.
    static KatSusu Kanepe(string ad, float derin) { return M(ad, 5.0f - derin / 2f - 0.02f, 0f, 6.5f, -1f, 0f); }
    static KatSusu TvDolabi(string ad, float derin) { return M(ad, 2.95f, 0f, 4.77f + derin / 2f, 0f, 1f); }
    static KatSusu TvUstu(string ad, float dolapBoyu) { return M(ad, 2.95f, dolapBoyu, 5.02f, 0f, 1f); }
    static KatSusu Berjer(string ad) { return M(ad, 1.40f, 0f, 7.60f, 0.3f, -1f); }
    static KatSusu Avize(string ad) { return M(ad, 2.80f, Tavan, 6.30f, 0f, 1f); }
    static KatSusu SalonCercevesi(string ad) { return M(ad, 5.0f, 1.55f, 7.15f, -1f, 0f); }
    static KatSusu HolCercevesi(string ad) { return M(ad, 3.80f, 1.60f, 4.65f, 0f, -1f); }
    // Duvar saati (#81, Şahan): ofis tipi quartz saatin yerine eski telefonlu süs saati; 50 cm boyunda, merkezi 1,55.
    static KatSusu HolSaati() { return M("vintage_telephone_wall_clock", 7.60f, 1.55f, 4.65f, 0f, -1f); }
    static KatSusu AntreAynasi() { return M("ornate_mirror_01", 2.60f, 1.55f, 1.60f, -1f, 0f); }
    // Antre sehpası (#81, Şahan): yalın modern sehpanın yerine boyalı ahşap komodin (üstü 62 cm).
    static KatSusu AntreSehpasi() { return M("painted_wooden_nightstand", 2.35f, 0f, 1.60f, -1f, 0f); }
    static KatSusu YatakA(string ad, float uzun) { return M(ad, 10.5f - uzun / 2f - 0.02f, 0f, 6.60f, -1f, 0f); }
    static KatSusu SifonyerA(string ad, float derin) { return M(ad, 7.70f + derin / 2f + 0.02f, 0f, 5.90f, 1f, 0f); }
    static KatSusu YatakB() { return M("vintage_day_bed", 7.6f - 0.43f - 0.02f, 0f, 6.60f, -1f, 0f); }

    /// <summary>Kat dosyasına o katın mobilyalarını ekler. ZincirleriKur bütün katlar kurulunca çağırır.</summary>
    static void MobilyalariEkle(KatZinciriVarligi v, int kat)
    {
        if (v == null) return;
        var m = new List<KatSusu>();
        switch (kat)
        {
            case 2:   // Nuri: yaşlı bir adamın klasik evi.
                m.Add(Kanepe("sofa_02", 0.82f));
                m.Add(M("throw_pillows_01", 4.55f, 0.42f, 6.50f, -1f, 0f));
                m.Add(Berjer("GreenChair_01"));
                m.Add(TvDolabi("ClassicConsole_01", 0.59f));
                m.Add(TvUstu("Television_01", 0.95f));
                m.Add(M("mantel_clock_01", 3.45f, 0.95f, 4.98f, 0f, 1f));   // konsol 2,18-3,72; TV 2,65-3,25
                m.Add(Avize("Chandelier_02"));
                m.Add(SalonCercevesi("fancy_picture_frame_01"));
                m.Add(AntreSehpasi());
                m.Add(Foto("kat2", false, 2.35f, 0.62f, 1.60f, -1f, 0f));      // Nuri Bey ve Rıza Efendi
                m.Add(HolCercevesi("hanging_picture_frame_02"));
                m.Add(YatakA("GothicBed_01", 2.04f));
                m.Add(SifonyerA("GothicCommode_01", 0.58f));
                m.Add(M("Rockingchair_01", 6.30f, 0f, 6.60f, -0.3f, -1f));      // Oda B
                break;

            case 3:   // Çocuklu aile: kalabalık, renkli. Çizim parçası kanepenin altında (#79).
                m.Add(Kanepe("Sofa_01", 0.66f));
                m.Add(Berjer("ArmChair_01"));
                m.Add(TvDolabi("painted_wooden_cabinet", 0.62f));
                m.Add(TvUstu("television_02", 1.18f));
                m.Add(Avize("Chandelier_01"));
                m.Add(SalonCercevesi("hanging_picture_frame_02"));
                m.Add(AntreSehpasi());
                m.Add(M("ceramic_pot", 2.28f, 0f, 2.30f, -1f, 0f));           // #81: sırlı toprak çömlek, komodinin yanında yerde
                m.Add(Foto("kat3", true, 3.80f, 1.60f, 4.65f, 0f, -1f));       // Kerem'in yaş günü
                m.Add(HolSaati());
                m.Add(YatakA("vintage_day_bed", 0.85f));   // Oda A'da gündüz yatağı (yatak odası sade)
                m.Add(SifonyerA("vintage_wooden_drawer_01", 0.46f));
                m.Add(M("painted_wooden_chair_02", 5.70f, 0f, 5.60f, 1f, 0.2f));  // Oda B
                m.Add(M("SchoolChair_01", 8.40f, 0f, 1.00f, 0.3f, 1f));            // mutfak
                break;

            case 4:   // Boş ve elektriksiz: neredeyse hiçbir şey; karanlıkta ayna ve durmuş saat.
                m.Add(AntreAynasi());
                m.Add(M("Ottoman_01", 3.00f, 0f, 6.30f, 0.2f, 1f));
                m.Add(M("vintage_telephone_wall_clock", 5.0f, 1.55f, 5.30f, -1f, 0f));   // v 7,2'de fosforlu şifre var
                // Geride bırakılmış orta sehpa: sofra ile gardırop arasındaki boşlukta, uzun kenarı v boyunca
                // (model x'i ön yönüne dik; ön yön +u). Gardıroba ~25 cm, sofranın sandalyelerine ~15 cm kalır.
                m.Add(M("CoffeeTable_01", 1.45f, 0f, 6.10f, 1f, 0f));
                m.Add(Foto("kat4", true, 3.80f, 1.60f, 4.65f, 0f, -1f));       // eski kiracılar (holde)
                break;

            case 5:   // Cemal'in son çalıştığı daire.
                m.Add(Kanepe("sofa_02", 0.82f));
                m.Add(Berjer("Rockingchair_01"));
                m.Add(TvDolabi("ClassicConsole_01", 0.59f));
                m.Add(TvUstu("television_02", 0.95f));
                m.Add(Foto("kat5", false, 2.40f, 0.95f, 5.00f, 0.2f, 1f));      // Cemal işe başladığı gün
                m.Add(Avize("Chandelier_02"));
                m.Add(SalonCercevesi("fancy_picture_frame_01"));
                m.Add(HolSaati());
                m.Add(YatakA("GothicBed_01", 2.04f));
                m.Add(YatakB());
                break;

            case 6:   // Çift daire: 3 numara (asıl) döşeli, 4 numara (B:) seyrek.
                m.Add(Kanepe("Sofa_01", 0.66f));
                m.Add(Berjer("GreenChair_01"));
                m.Add(TvDolabi("painted_wooden_cabinet", 0.62f));
                m.Add(TvUstu("Television_01", 1.18f));
                m.Add(Avize("Chandelier_01"));
                m.Add(Foto("kat6", true, 3.80f, 1.60f, 4.65f, 0f, -1f));       // dört tabaklı sofra
                m.Add(YatakA("vintage_day_bed", 0.85f));
                m.Add(M("ArmChair_01", 1.40f, 0f, 7.60f, 0.3f, -1f, KilitZinciri.IkinciOnek));
                m.Add(M("GothicCommode_01", 7.70f + 0.31f, 0f, 5.90f, 1f, 0f, KilitZinciri.IkinciOnek));
                m.Add(M("ornate_mirror_01", 2.60f, 1.55f, 1.60f, -1f, 0f, KilitZinciri.IkinciOnek));
                break;

            case 7:   // Cemal'in eşyaları; elektrik yok. Oda A'nın dip duvarında fosforlu şifre: yatak yok.
                m.Add(Kanepe("sofa_02", 0.82f));
                m.Add(Berjer("ArmChair_01"));
                m.Add(TvDolabi("GothicCommode_01", 0.58f));
                m.Add(TvUstu("television_02", 1.21f));
                m.Add(Foto("kat7", false, 2.50f, 1.21f, 4.98f, 0.2f, 1f));      // Cemal ve arkasındaki adam
                m.Add(Avize("Chandelier_02"));
                m.Add(SalonCercevesi("hanging_picture_frame_02"));
                m.Add(HolSaati());
                m.Add(M("vintage_suitcase", 6.35f, 0f, 7.85f, 0f, -1f));         // Oda B: toplanmış bavullar
                break;

            case 8:   // Yönetim dairesi: ofis. Hol duvarında iskân ruhsatı (u 2,9): altına alçak dolap.
                m.Add(TvDolabi("vintage_wooden_drawer_01", 0.46f));
                m.Add(M("mantel_clock_01", 3.10f, 0.55f, 4.98f, 0f, 1f));
                m.Add(Berjer("GreenChair_01"));
                m.Add(M("Ottoman_01", 4.40f, 0f, 6.60f, -1f, 0f));
                // Oda B toplantı odası: ortada yuvarlak masa (1,3 m). Oyuncak sandığına (u 5,3 v 7,3,
                // zincirin yeri) ve kapıya (hol duvarı, v 4,75) yol bırakır.
                m.Add(M("small_wooden_table_01", 6.35f, 0f, 6.30f, 0f, 1f));  // #81: mermer tablalı modern masanın yerine küçük ahşap masa
                m.Add(Avize("Chandelier_01"));
                m.Add(AntreAynasi());
                m.Add(M("vintage_telephone_wall_clock", 5.0f, 1.55f, 7.15f, -1f, 0f));
                m.Add(SifonyerA("GothicCommode_01", 0.58f));
                m.Add(Foto("kat8", true, 3.80f, 1.60f, 4.65f, 0f, -1f));       // 1974 iskân töreni
                break;
        }
        // Oda oda döşenen katın (#83) mobilyası kendi kurucusunda seçilir; ortak mutfak eklenmez.
        if (v.yalnizSecilenEsyalar) { v.suslar.AddRange(m); EditorUtility.SetDirty(v); return; }
        // MUTFAK (#81, Şahan): her dairede fırınlı beyaz ocak, buzdolabının yanında dış duvara (v = -0,8)
        // yaslı; yaşanan evlerde üstünde eski çaydanlık. Taşınan aile (1) ve boş daire (4) çaydanlığı götürmüş.
        m.Add(M("electric_stove", 8.30f, 0f, -0.8f + 0.34f, 0f, 1f));
        if (kat != 1 && kat != 4) m.Add(M("vintage_electric_kettle", 8.36f, 0.86f, -0.48f, 0.3f, 1f));
        if (v.ikinciDaire > 0)   // 6. kat: iki daire birlikte açık, ikincisinin mutfağı da
        {
            m.Add(M("electric_stove", 8.30f, 0f, -0.8f + 0.34f, 0f, 1f, KilitZinciri.IkinciOnek));
            m.Add(M("vintage_electric_kettle", 8.36f, 0.86f, -0.48f, 0.3f, 1f, KilitZinciri.IkinciOnek));
        }
        v.suslar.AddRange(m);
        EditorUtility.SetDirty(v);
    }
}
