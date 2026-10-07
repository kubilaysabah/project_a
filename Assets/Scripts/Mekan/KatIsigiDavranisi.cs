// KatIsigiDavranisi.cs
// Bir kat imzası ışığını (#116) oynatır: şiddeti ve rengi IsikTitremesi'nden okur, parlayan kaynağı (alev,
// fener camı) da aynı çarpanla parlatır. ZincirUygulayici daire açılınca kurar; kat değişince ışıkla
// birlikte silinir (konanlar).
//
// Parlayan kaynak bir küre (ilkel şekil, URP Unlit malzeme). Rengini MaterialPropertyBlock ile alır: tek
// malzeme bütün katlarda paylaşılır, her kaynak kendi rengini (sıcak mum, mavi ocak, beyaz fener) taşır.

using UnityEngine;

public class KatIsigiDavranisi : MonoBehaviour
{
    private Light isik;
    private Renderer kaynak;                 // alev / fener camı (yoksa null)
    private KatIsikTitremesi tur;
    private Color renk, renk2;
    private float temelSiddet, kaynakParlakligi;
    private int tohum;
    private bool golgeli;                    // kat dosyası gölge istedi mi (kalite izin verirse uygulanır, #119)
    private MaterialPropertyBlock blok;

    public void Hazirla(Light l, Renderer kaynakRenderer, KatIsikTitremesi titreme, Color r1, Color r2, float kaynakParlak, int sira, bool golge = false)
    {
        isik = l;
        golgeli = golge;
        GolgeyiUygula();
        Ayarlar.Degisti += GolgeyiUygula;   // kalite Düşük → Orta/Yüksek olunca gölge de gelsin (#119)
        kaynak = kaynakRenderer;
        tur = titreme;
        renk = r1;
        renk2 = r2;
        temelSiddet = l != null ? l.intensity : 1f;
        kaynakParlakligi = kaynakParlak;
        tohum = sira;
        blok = new MaterialPropertyBlock();
        Uygula();
    }

    private void OnDestroy() { Ayarlar.Degisti -= GolgeyiUygula; }

    // Gölge yalnızca spot ışıkta ve Düşük kalite dışında (Düşük'ün URP varlığı ek ışık gölgesini zaten kapatır).
    private void GolgeyiUygula()
    {
        if (isik == null) return;
        isik.shadows = golgeli && isik.type == LightType.Spot && Ayarlar.Kalite > 0 ? LightShadows.Soft : LightShadows.None;
    }

    private void Update() { Uygula(); }

    private void Uygula()
    {
        if (isik == null) return;
        float c = IsikTitremesi.Carpan(tur, Time.time, tohum);
        Color simdiki = IsikTitremesi.IkinciRenk(tur, Time.time, tohum) ? renk2 : renk;
        isik.intensity = temelSiddet * c;
        isik.color = simdiki;
        if (kaynak != null)
        {
            // HDR renk: 1'in üstü, bloom'un (post-processing) parlattığı değer.
            blok.SetColor("_BaseColor", simdiki * (kaynakParlakligi * c));
            kaynak.SetPropertyBlock(blok);
        }
    }
}
