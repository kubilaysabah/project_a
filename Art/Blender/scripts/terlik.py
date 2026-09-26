# terlik.py
# Kapı önündeki ev terlikleri, DÖRT çeşit: her dairenin terliği farklı (#42).
#
# Şahan'ın isteği (2026-09-26): terlikler gerçek model olsun ve daireye göre değişsin.
# Oyuncu hangi kapının önünde hangi terliğin durduğunu öğrenir; bir terlik yer değiştirir,
# teke düşer ya da ters dönerse fark etmesi kolaylaşır.
#   Terlik_Erkek   1 numara: lacivert plastik taban, geniş kauçuk bant ("lastik terlik")
#   Terlik_Kadin   2 numara: pembe kumaş, burnu kapalı, üstünde ponpon
#   Terlik_Cocuk   3 numara: küçük, sarı taban, mavi bant
#   Terlik_Yasli   4 numara: kahverengi kumaş, burnu kapalı, kalın bej taban
#
# Tek tek modelleniyor; sahnede sol ve sağ terlik ayrı nesneler olmalı çünkü "tek terlik"
# anomalisi birini gizliyor. Sağ terlik Unity'de sol terliğin aynası (x ölçeği -1).
# Yön: burun +y tarafında (Unity'de -Z). Pivot: tabanın alt ortası. Eski tek model
# (Terlik.fbx) bu yönleri ve pivotu kullanıyordu; anomali kartları ona göre yazılmış.
#
# ÇALIŞTIRMA:  blender -b --factory-startup --python Art/Blender/scripts/terlik.py

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bmesh
import bpy
import ortak


def taban(ad, boy, en, kalinlik, mat):
    """
    Ayak izi biçiminde taban. Kutudan değil bir DIŞ ÇİZGİDEN üretiliyor: topuk dar,
    parmak kökü geniş, iki uç yuvarlak. Çizginin her noktası için yarı genişlik:
      w(u) = en/2 × (0,78 + 0,22 × sin(π·u)) × kök(1 − (2u − 1)^6)
    u: topuktan (0) buruna (1). İlk çarpan ortayı genişletir, ikincisi uçları yuvarlar.
    """
    noktalar = []
    adim = 40
    for i in range(adim + 1):                        # sağ kenar: topuktan buruna
        u = i / adim
        w = en / 2 * (0.78 + 0.22 * math.sin(math.pi * min(1.0, u * 1.15))) * max(0.0, 1 - (2 * u - 1) ** 6) ** 0.5
        noktalar.append((w, (u - 0.5) * boy))
    for i in range(adim - 1, 0, -1):                 # sol kenar: burundan topuğa (uçlar ortak)
        x, y = noktalar[i]
        noktalar.append((-x, y))

    mesh = bpy.data.meshes.new(ad)
    b = bmesh.new()
    alt = [b.verts.new((x, y, 0.0)) for x, y in noktalar]
    yuz = b.faces.new(alt)                           # düz bir ayak izi yüzeyi
    ekstrude = bmesh.ops.extrude_face_region(b, geom=[yuz])
    for v in ekstrude["geom"]:
        if isinstance(v, bmesh.types.BMVert):
            v.co.z = kalinlik                        # yüzeyi yukarı çekip kalınlık ver
    bmesh.ops.recalc_face_normals(b, faces=b.faces)
    b.to_mesh(mesh)
    b.free()
    n = bpy.data.objects.new(ad, mesh)
    bpy.context.collection.objects.link(n)
    ortak.ata(n, mat)
    ortak.pah(n, genislik=0.004, bolum=3)            # taban kenarı yuvarlak olsun
    return n


def bant(ad, boy, en, kalinlik, derinlik, yukseklik, konum_y, mat, kapali=False):
    """
    Ayağın girdiği üst kısım: yatay bir silindirden kesilmiş kemer.
    derinlik: bandın burun-topuk yönündeki genişliği. kapali=True: burnu kapalı terlik
    (bant buruna kadar uzanır, önü yuvarlak bir kapakla kapanır).
    """
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.5, depth=1.0,
                                        location=(0, konum_y, kalinlik), rotation=(0, math.pi / 2, 0))
    dis = bpy.context.active_object
    dis.name = ad
    # Dönüşü önce meshe işle, sonra ölçekle (ilk terlikte öğrenilen tuzak: tersi,
    # ölçeği yanlış eksene uygular).
    bpy.ops.object.transform_apply(rotation=True)
    dis.scale = (en * 1.04, derinlik, yukseklik * 2)
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(dis, mat)
    # Alt yarıyı at: kemerin yalnızca tabanın üstünde kalan kısmı lazım.
    ortak.oy(dis, ortak.kutu("k", (1, 1, 1), (0, konum_y, kalinlik - 0.5), mat))
    # İçini oy: geriye ~6 mm kalınlığında bir kabuk kalsın.
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.5, depth=1.0,
                                        location=(0, konum_y, kalinlik), rotation=(0, math.pi / 2, 0))
    ic = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ic.scale = (en * 0.9, derinlik * 0.8 if kapali else derinlik * 1.5, (yukseklik - 0.006) * 2)
    bpy.ops.object.transform_apply(scale=True)
    if kapali:
        ic.location.y -= derinlik * 0.2              # oyuğu topuk tarafına kaydır: burun kapalı kalır
    ortak.oy(dis, ic)
    return dis


def terlik(ad, boy, en, taban_kalinlik, taban_renk, ust_renk, ust_puruz, kapali, ponpon=False):
    ortak.sahneyi_temizle()
    taban_mat = ortak.malzeme(ad + "_Taban", taban_renk, puruz=0.7)
    ust_mat = ortak.malzeme(ad + "_Ust", ust_renk, puruz=ust_puruz)

    parcalar = [taban(ad + "_Taban", boy, en, taban_kalinlik, taban_mat)]
    if kapali:
        # Burnu kapalı: bant tabanın ön yarısını örter.
        parcalar.append(bant(ad + "_Ust", boy, en, taban_kalinlik, boy * 0.48, 0.05, boy * 0.2, ust_mat, kapali=True))
    else:
        # Açık terlik: parmak köküne tek geniş bant.
        parcalar.append(bant(ad + "_Ust", boy, en, taban_kalinlik, boy * 0.3, 0.042, boy * 0.16, ust_mat))
    if ponpon:
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.022, location=(0, boy * 0.22, taban_kalinlik + 0.038))
        p = bpy.context.active_object
        ortak.ata(p, ust_mat)
        parcalar.append(p)

    n = ortak.tek_parca(parcalar, ad, 40)
    ortak.kaydet_ve_disari_aktar(ad, [n])
    print("OLCU", ad, round(n.dimensions.x, 3), round(n.dimensions.y, 3), round(n.dimensions.z, 3))
    # Önizleme istenirse her çeşit kendi dosyasına: <yol>_Terlik_Erkek.png ...
    if "--onizleme" in sys.argv:
        yol = sys.argv[sys.argv.index("--onizleme") + 1]
        ortak.onizleme_al(yol.replace(".png", "_" + ad + ".png"), 0.08, 0.42)


# (ad, boy, en, taban kalınlığı, taban rengi, üst rengi, üst pürüzü, burnu kapalı, ponpon)
terlik("Terlik_Erkek", 0.28, 0.105, 0.024, (0.03, 0.05, 0.12), (0.05, 0.07, 0.16), 0.5, False)
terlik("Terlik_Kadin", 0.25, 0.095, 0.020, (0.55, 0.40, 0.42), (0.75, 0.30, 0.42), 0.9, True, ponpon=True)
terlik("Terlik_Cocuk", 0.18, 0.075, 0.018, (0.85, 0.65, 0.05), (0.08, 0.30, 0.75), 0.5, False)
terlik("Terlik_Yasli", 0.27, 0.105, 0.030, (0.62, 0.55, 0.42), (0.22, 0.13, 0.08), 0.95, True)
