// GrayboxKurucu_Mobilya.cs
// Poly Haven'ın CC0 mobilyalarını (MobilyaKurucu prefab'larını kurar, #73) ve aile fotoğraflarını (#69) kat
// dosyasının süs listesine koyan küçük yardımcılar. HANGİ katta NEREYE ne konacağı artık her katın kendi
// kurucusunda (GrayboxKurucu_Kat1..8 ve Zincir dosyasındaki 1-2. katlar), oda oda (#83).
//
// KURALLAR
//   - Katlarda aynı model yok (Şahan, #83): bir Poly Haven modeli yalnızca bir katta kullanılır.
//   - Zincir yerlerinin (plan noktaları) ve kalıcı eşyaların üstüne konmaz. Ölçüler plan eksenlerinde:
//     u landing duvarından içeri, v giriş kapısından (DairePlani). Ön: modelin baktığı plan yönü (OnU, OnV).
//   - Duvar eşyaları (ayna, çerçeve, saat) arka yüzü duvara değecek şekilde duvarın iç yüzüne, merkez
//     yüksekliğiyle konur. Avizenin noktası tepesidir; tavana (2,80) asılır.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class GrayboxKurucu
{
    const float Tavan = DairePlani.TavanYuk;
    // Salon penceresinin altında petek var (20 cm derin). Perde duvara asılınca peteğin içinden geçiyordu
    // (çakışma testi); korniş duvardan 22 cm açıkta, perde peteğin önünden sarkar (90'larda korniş
    // uzun ayaklarla ya da tavana takılırdı). Pencere duvarı v = 8,10.
    const float PetekOnuPerde = 8.10f - 0.22f;

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

    // Bir mobilya: plan noktası (u, yükseklik, v) ve ön yönü. yol "B:" ise ikinci dairede (#70).
    static KatSusu M(string ad, float u, float y, float v, float onU, float onV, string yol = "")
    {
        return new KatSusu { Sablon = Mob(ad), Yol = yol, X = u, Y = y, Z = v, OnU = onU, OnV = onV };
    }
}
