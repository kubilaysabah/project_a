// Yazi3B.shader
// Dünyanın içindeki 3B yazılar (kapı numaraları, asansör notu, gösterge) için gölgelendirici.
//
// GÖLGELENDİRİCİ (shader) NEDİR
// Ekrana çizilen her yüzeyin her pikselinin rengini hesaplayan küçük bir programdır.
// Malzeme (material) bir gölgelendiriciyi ve onun ayarlarını (doku, renk) taşır.
//
// NEDEN KENDİ GÖLGELENDİRİCİMİZ
// Unity'nin TextMesh bileşeni varsayılan olarak ekran arayüzü için yazılmış
// "GUI/Text Shader"ı kullanır. O gölgelendirici DERİNLİĞE BAKMAZ: yazı, önünde
// duvar olsa bile her zaman en üste çizilir. Aydınlık graybox'ta fark edilmiyordu;
// gece atmosferinde (#11) bir üst kattaki asansör notu tavanın içinden görünüyor,
// kapı numaraları ceza karanlığında bile parlıyordu.
//
// Bu gölgelendiricinin tek farkı "ZTest LEqual": bir pikseli çizmeden önce, o noktada
// kameraya daha yakın bir yüzey olup olmadığına bakar; varsa yazıyı çizmez.

Shader "Huzur/Yazi3B"
{
    Properties
    {
        // Yazı tipinin harf dokusu. Harflerin şekli bu dokunun saydamlık (alfa) kanalında.
        _MainTex ("Harf Dokusu", 2D) = "white" {}
    }

    SubShader
    {
        // Saydam nesnelerle birlikte, katı nesnelerden SONRA çizilir.
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Lighting Off                        // ışıktan etkilenmez (TextMesh zaten ışık bilgisi taşımaz)
        Cull Off                            // yüzün arkası da çizilir; yazı ters dönse de kaybolmasın
        ZWrite Off                          // saydam yazı derinlik bilgisine yazmaz
        ZTest LEqual                        // ASIL DÜZELTME: önünde bir yüzey varsa çizme
        Blend SrcAlpha OneMinusSrcAlpha     // harfin kenarı yumuşak saydamlıkla karışsın

        Pass
        {
            CGPROGRAM
            #pragma vertex tepe
            #pragma fragment piksel
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Girdi: köşe noktasının konumu, rengi (TextMesh'in rengi buraya gelir) ve doku koordinatı.
            struct Girdi  { float4 konum : POSITION; fixed4 renk : COLOR; float2 uv : TEXCOORD0; };
            struct Cikti  { float4 konum : SV_POSITION; fixed4 renk : COLOR; float2 uv : TEXCOORD0; };

            // Köşe programı: her köşeyi ekrandaki yerine taşır.
            Cikti tepe (Girdi g)
            {
                Cikti c;
                c.konum = UnityObjectToClipPos(g.konum);
                c.renk = g.renk;
                c.uv = TRANSFORM_TEX(g.uv, _MainTex);
                return c;
            }

            // Piksel programı: rengi TextMesh'in rengi, saydamlığı harf dokusundan al.
            fixed4 piksel (Cikti c) : SV_Target
            {
                fixed4 sonuc = c.renk;
                sonuc.a *= tex2D(_MainTex, c.uv).a;
                return sonuc;
            }
            ENDCG
        }
    }
}
