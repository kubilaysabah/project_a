# salon_nuri_esyalar.py
# 2. katta Nuri Bey'in salonunun küçük eşyaları (#83, Şahan'ın seçimi 2026-09-29). Her biri kendi FBX'i;
# betik argümanla seçilir:
#   blender -b --factory-startup --python salon_nuri_esyalar.py -- battaniye | cay | gazete | sardunya | dikis | dantel
#
#   Orgu_Battaniye   : sallanan sandalyenin oturağında katlı duran, renkli karelerden örgü battaniye.
#   Cay_Takimi       : ince belli bardakta yarım, soğumuş çay (tabağı, kaşığı) ve iki katlı çaydanlık.
#   Gazete_Kilif     : sehpada açık bırakılmış gazete ve içi boş gözlük kılıfı (gözlük holde kırık).
#   Kuru_Sardunya    : denizlikte üç toprak saksı, günlerdir sulanmamış, kurumuş sardunyalar.
#   Dikis_Makinesi   : eşinden kalma pedallı dikiş makinesi masası; makinenin üstünde dantel örtü.
#                      Tablanın altında öne kayan çekmecesi ayrı parça (Cekmece_1): 2. katın görev kabı.
#   Koltuk_Danteli   : koltuk sırtının üstüne atılmış dantel (üstte yatay kısım, önde sarkan kısım).
# Ön yüz -y; Unity'de +Z. Pivot: masaüstü eşyalarında tabanın ortası; dantelde sırtın üst ön kenarı.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "cay"
ortak.sahneyi_temizle()
rnd = random.Random(71)
p = []
parcalar = []          # açılır eşyada Govde dışındaki ayrı nesneler (dikiş masasının çekmecesi)


def silindir_yatik(ad, r, boy, konum, rot, mat, kose=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


if hangi == "battaniye":
    renkler = [ortak.malzeme(f"Orgu_{i}", c, puruz=0.95) for i, c in enumerate(
        ((0.45, 0.10, 0.08), (0.50, 0.38, 0.08), (0.10, 0.25, 0.12), (0.10, 0.12, 0.35), (0.60, 0.55, 0.45)))]
    kenar = ortak.malzeme("Orgu_Kenar", (0.05, 0.04, 0.03), puruz=0.95)
    # Dört kat katlanmış: 0,42 x 0,36 x 0,06; üstünde 5 x 4 renkli kare motifi, kenarında koyu çerçeve.
    p.append(ortak.kutu("Govde", (0.42, 0.36, 0.06), (0, 0, 0.03), kenar, 0.015))
    for i in range(5):
        for j in range(4):
            p.append(ortak.kutu("Kare", (0.075, 0.075, 0.004), (-0.17 + i * 0.085, -0.13 + j * 0.087, 0.061), renkler[(i + j * 2) % 5], 0.01))
    # Bir ucu sandalyenin kenarından sarkıyor.
    p.append(ortak.kutu("Sarkan", (0.40, 0.02, 0.22), (0, -0.19, -0.08), renkler[0], 0.01))
    ad, h = "Orgu_Battaniye", 0.2

elif hangi == "cay":
    cam = ortak.malzeme("Cay_Bardagi", (0.60, 0.62, 0.60), puruz=0.05)
    cay = ortak.malzeme("Cay", (0.25, 0.06, 0.01), puruz=0.05)
    porselen = ortak.malzeme("Cay_Tabagi", (0.85, 0.84, 0.80), puruz=0.2)
    metal = ortak.malzeme("Caydanlik_Celik", (0.55, 0.55, 0.53), puruz=0.25, metal=0.9)
    kara = ortak.malzeme("Caydanlik_Sap", (0.03, 0.03, 0.03), puruz=0.5)
    # Bardak (ince belli): tabakta; alt geniş, bel dar, ağız geniş; içinde yarım çay.
    bx = -0.12
    p.append(ortak.silindir("Tabak", 0.055, 0.006, (bx, 0, 0.003), porselen, kose=24))
    for ad_, r, h_, z in (("Dip", 0.022, 0.02, 0.016), ("Bel", 0.017, 0.03, 0.041), ("Agiz", 0.024, 0.04, 0.076)):
        g = ortak.silindir(ad_, r, h_, (bx, 0, z), cam, kose=20)
        p.append(g)
    p.append(ortak.silindir("Cay", 0.019, 0.035, (bx, 0, 0.04), cay, kose=20))
    p.append(ortak.kutu("Kasik", (0.004, 0.09, 0.003), (bx + 0.035, -0.02, 0.008), metal))
    # Çaydanlık: altta su kazanı, üstünde demlik; ikisinin de sapı ve kapağı.
    cx = 0.10
    p.append(ortak.silindir("Kazan", 0.085, 0.14, (cx, 0.02, 0.07), metal, kose=28))
    p.append(ortak.silindir("Demlik", 0.06, 0.10, (cx, 0.02, 0.19), metal, kose=24))
    p.append(ortak.silindir("DemlikKapak", 0.04, 0.02, (cx, 0.02, 0.25), metal, kose=20))
    p.append(ortak.silindir("Topuz", 0.012, 0.02, (cx, 0.02, 0.27), kara, kose=12))
    p.append(ortak.kutu("Sap", (0.02, 0.10, 0.02), (cx, 0.02 + 0.09, 0.19), kara, 0.005))
    silindir_yatik("Emzik", 0.01, 0.07, (cx - 0.08, 0.02, 0.19), (0, math.radians(-60), 0), metal)
    ad, h = "Cay_Takimi", 0.3

elif hangi == "gazete":
    gazete = ortak.malzeme("Gazete_Nuri", (0.62, 0.60, 0.54), puruz=0.9)
    baski = ortak.malzeme("Gazete_Nuri_Baski", (0.28, 0.27, 0.25), puruz=0.9)
    manset = ortak.malzeme("Gazete_Manset", (0.45, 0.05, 0.05), puruz=0.9)
    kilif = ortak.malzeme("Gozluk_Kilifi", (0.15, 0.07, 0.03), puruz=0.5)
    ic = ortak.malzeme("Kilif_Ici", (0.40, 0.05, 0.08), puruz=0.9)
    # Açık gazete: iki sayfa yan yana, ortadan hafif kalkık (katlama çizgisinde ince tümsek).
    for yon in (-1, 1):
        p.append(ortak.kutu("Sayfa", (0.28, 0.40, 0.003), (yon * 0.142, 0, 0.0015 + (0.006 if yon > 0 else 0)), gazete))
        p.append(ortak.kutu("Sutunlar", (0.24, 0.30, 0.001), (yon * 0.142, -0.02, 0.0035 + (0.006 if yon > 0 else 0)), baski))
    p.append(ortak.kutu("Manset", (0.24, 0.05, 0.001), (-0.142, 0.16, 0.0035), manset))
    # Gözlük kılıfı: gazetenin yanında, kapağı açık, içi boş (kırmızı kadife astar görünür).
    kx, ky = 0.36, -0.08
    kilif_n = ortak.kutu("Kilif", (0.16, 0.06, 0.035), (kx, ky, 0.0175), kilif, 0.012)
    ortak.oy(kilif_n, ortak.kutu("Oyuk", (0.14, 0.045, 0.03), (kx, ky, 0.03), kilif))
    p.append(kilif_n)
    p.append(ortak.kutu("Astar", (0.14, 0.045, 0.002), (kx, ky, 0.008), ic))
    bpy.ops.mesh.primitive_cube_add(size=1, location=(kx, ky + 0.045, 0.05), rotation=(math.radians(-70), 0, 0))
    k = bpy.context.active_object
    k.scale = (0.16, 0.06, 0.012)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.pah(k, 0.005)
    ortak.ata(k, kilif)
    p.append(k)
    ad, h = "Gazete_Kilif", 0.1

elif hangi == "sardunya":
    toprak = ortak.malzeme("Saksi_Toprak", (0.35, 0.12, 0.05), puruz=0.8)
    kuru_toprak = ortak.malzeme("Kuru_Toprak", (0.15, 0.10, 0.06), puruz=0.95)
    kuru = ortak.malzeme("Kuru_Yaprak", (0.18, 0.12, 0.05), puruz=0.9)
    solgun = ortak.malzeme("Solgun_Yaprak", (0.20, 0.22, 0.06), puruz=0.9)
    cicek = ortak.malzeme("Kuru_Cicek", (0.25, 0.06, 0.06), puruz=0.9)
    for sx in (-0.30, 0.0, 0.30):
        # Saksı: alta doğru daralan iki silindir ve ağız halkası; içinde çatlamış kuru toprak.
        p.append(ortak.silindir("SaksiAlt", 0.050, 0.06, (sx, 0, 0.03), toprak, kose=20))
        p.append(ortak.silindir("SaksiUst", 0.060, 0.06, (sx, 0, 0.09), toprak, kose=20))
        p.append(ortak.silindir("Agiz", 0.066, 0.02, (sx, 0, 0.12), toprak, kose=20))
        p.append(ortak.silindir("Toprak", 0.056, 0.01, (sx, 0, 0.12), kuru_toprak, kose=20))
        # Kurumuş saplar ve sarkık yapraklar.
        for k in range(5):
            a = 2 * math.pi * k / 5 + rnd.uniform(-0.3, 0.3)
            egim = rnd.uniform(0.3, 0.9)
            boy = rnd.uniform(0.12, 0.20)
            bx, by = sx + math.cos(a) * 0.015, math.sin(a) * 0.015
            silindir_yatik("Sap", 0.003, boy, (bx + math.cos(a) * boy * 0.3 * egim, by + math.sin(a) * boy * 0.3 * egim, 0.125 + boy * 0.45),
                           (math.sin(a) * egim * 0.6, -math.cos(a) * egim * 0.6, 0), kuru, kose=6)
            yx, yy = bx + math.cos(a) * boy * 0.55 * egim, by + math.sin(a) * boy * 0.55 * egim
            bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.025, depth=0.002, location=(yx, yy, 0.125 + boy * 0.75), rotation=(rnd.uniform(0.5, 1.3), 0, a))
            y_ = bpy.context.active_object
            bpy.ops.object.transform_apply(rotation=True)
            ortak.ata(y_, kuru if k % 2 else solgun)
            p.append(y_)
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.02, location=(sx + 0.02, 0.01, 0.125 + 0.2), segments=8, ring_count=6)
        ortak.ata(bpy.context.active_object, cicek)
        p.append(bpy.context.active_object)
    ad, h = "Kuru_Sardunya", 0.35

elif hangi == "dikis":
    ahsap = ortak.malzeme("Dikis_Ahsap", (0.10, 0.05, 0.02), puruz=0.4)
    dokum = ortak.malzeme("Dikis_Dokum", (0.03, 0.03, 0.03), puruz=0.5, metal=0.6)
    dantel = ortak.malzeme("Dikis_Ortu", (0.84, 0.80, 0.70), puruz=0.9)
    # Masa: ahşap tabla, altında döküm ayak çerçevesi, pedal ve volan.
    EN, DER, H = 0.90, 0.45, 0.76
    p.append(ortak.kutu("Tabla", (EN, DER, 0.03), (0, 0, H - 0.015), ahsap, 0.006))
    # Çekmece (#83, 2. katın görev kabı: Oda A'nın anahtarı burada): tablanın altında asılı bir yuva
    # (iki yan, taban yok) ve ayrı bir parça olarak öne kayan çekmece (acilir_ortak kuralı).
    p.append(ortak.kutu("YuvaYan", (0.012, DER - 0.04, 0.11), (0.25 - 0.162, 0.01, H - 0.085), ahsap, 0.002))
    p.append(ortak.kutu("YuvaYan", (0.012, DER - 0.04, 0.11), (0.25 + 0.162, 0.01, H - 0.085), ahsap, 0.002))
    p.append(ortak.kutu("YuvaArka", (0.33, 0.012, 0.11), (0.25, DER / 2 - 0.02, H - 0.085), ahsap, 0.002))
    parcalar.append(ac.cekmece("Cekmece_1", 0.25, H - 0.14, 0.31, 0.105, DER - 0.07, -DER / 2 + 0.01, ahsap, ahsap, dokum, "topuz"))
    for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
        p.append(ortak.kutu("AyakYan", (0.04, DER - 0.04, 0.05), (x, 0, 0.025), dokum, 0.006))
        p.append(ortak.kutu("AyakDik", (0.03, 0.05, H - 0.10), (x, 0, (H - 0.10) / 2 + 0.03), dokum, 0.005))
    p.append(ortak.kutu("Kusak", (EN - 0.10, 0.03, 0.03), (0, 0.10, 0.20), dokum, 0.004))
    p.append(ortak.kutu("Pedal", (0.36, 0.22, 0.02), (0, -0.02, 0.10), dokum, 0.004))
    silindir_yatik("Volan", 0.14, 0.02, (-EN / 2 + 0.13, 0.05, 0.35), (0, math.pi / 2, 0), dokum, kose=28)
    # Makine başı: tablanın üstünde, dantel örtüyle örtülü (yumuşak köşeli, eteği sarkan).
    govde = ortak.kutu("Ortu", (0.50, 0.24, 0.30), (0, 0, H + 0.15), dantel, 0.06)
    p.append(govde)
    for i in range(14):
        x = -0.24 + i * 0.037
        p.append(ortak.kutu("Etek", (0.03, 0.006, 0.06), (x, -0.125 - (0.006 if i % 2 else 0), H + 0.02), dantel, 0.004))
    ad, h = "Dikis_Makinesi", 1.1

else:
    dantel = ortak.malzeme("Koltuk_Dantel", (0.86, 0.83, 0.74), puruz=0.9)
    # Sırtın üstünde yatay kısım (arkaya 0,10) ve önde sarkan kısım (0,22); alt ucu fistolu.
    p.append(ortak.kutu("Ust", (0.36, 0.12, 0.004), (0, 0.06, 0.0), dantel))
    p.append(ortak.kutu("On", (0.36, 0.004, 0.22), (0, -0.002, -0.11), dantel))
    for i in range(9):
        bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.022, depth=0.004, location=(-0.16 + i * 0.04, -0.002, -0.22), rotation=(math.pi / 2, 0, 0))
        f = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(f, dantel)
        p.append(f)
    ad, h = "Koltuk_Danteli", 0.3

if parcalar:
    # Açılır eşya (#83): gövde "Govde" adıyla, çekmece ayrı nesne (Unity: GrayboxKurucu.AcilirSablonKur).
    govde = ortak.birlestir(p, "Govde")
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, 2.2)
else:
    nesne = ortak.birlestir(p, ad)
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [nesne], h, 1.0)
