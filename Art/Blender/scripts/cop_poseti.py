# cop_poseti.py
# Kapı önündeki siyah çöp poşetleri, ÜÇ farklı şekil (#42).
#
# Şahan'ın isteği (2026-09-26): graybox'tan kalma siyah kutular gerçek çöp poşeti olsun:
# şişkin, buruşuk, üstü düğümlü. Üç şekil var ki yan yana dizilince hepsi aynı durmasın:
#   Poset_A  dolgun, dengeli
#   Poset_B  daha uzun, biraz yana yatık
#   Poset_C  basık, geniş
# Hepsi 1 m yüksekliğinde üretiliyor; Unity'de her poşet yerine göre (büyük, orta, küçük)
# ölçekleniyor. Tek dosyada üç parça: Cop_Poseti.fbx içinde Poset_A, Poset_B, Poset_C.
#
# NASIL ŞİŞKİN VE BURUŞUK OLUYOR
#   1. Bir küreden başlanır.
#   2. Alt kısmı düzleştirilir: poşet yerde oturur, altı basılır.
#   3. Üst kısmı eksene doğru toplanır: ağzı büzülmüş, düğümlenmiş bir boyun oluşur.
#   4. Her köşe rastgele biraz içeri ya da dışarı itilir (içindeki çöpün tümsekleri),
#      sonra yumuşatılır: sert dikenler değil, yumuşak şişkinlikler kalır.
#   5. Tepeye bir düğüm ve iki yana sarkan "kulak" (bağlanmış tutma yerleri) eklenir.
# Rastgelelik tohumlu: betik her çalıştığında aynı üç poşeti üretir.
#
# ÇALIŞTIRMA:  blender -b --factory-startup --python Art/Blender/scripts/cop_poseti.py

import math, os, random, sys
sys.path.append(os.path.dirname(__file__))
import bmesh
import bpy
import ortak


def poset(ad, tohum, genislik, derinlik, taban_z, egim, mat):
    random.seed(tohum)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=40, ring_count=24, radius=0.5, location=(0, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    ortak.ata(n, mat)

    # İçindeki çöpün büyük tümsekleri: rastgele beş yön; o yöne yakın köşeler dışarı itilir.
    tumsekler = []
    for _ in range(5):
        aci, yuk = random.uniform(0, 2 * math.pi), random.uniform(-0.3, 0.1)
        tumsekler.append((math.cos(aci), math.sin(aci), yuk, random.uniform(0.06, 0.12)))
    kivrim_faz = random.uniform(0, 6.28)

    b = bmesh.new()
    b.from_mesh(n.data)
    for v in b.verts:
        x, y, z = v.co
        aci = math.atan2(y, x)
        # 2) Alt: taban_z'nin altı yere basılmış gibi yayılır.
        if z < taban_z:
            z = taban_z - (taban_z - z) * 0.15
            x *= 1.06; y *= 1.06
        # Büyük tümsekler (içindeki çöp).
        r = (x * x + y * y) ** 0.5 + 1e-6
        itme = 0.0
        for tx, ty, tz, guc in tumsekler:
            yakinlik = (x * tx + y * ty) / r                   # o yöne ne kadar bakıyor (-1..1)
            itme += guc * max(0.0, yakinlik) ** 6 * math.exp(-((z - tz) / 0.25) ** 2)
        x *= 1 + itme; y *= 1 + itme
        # 3) Üst: 0,15'in üstü eksene toplanır (büzülmüş ağız); toplandıkça naylon katlanır:
        #    açıya göre dalgalanan kıvrımlar (ağza doğru yaklaşan pileler).
        if z > 0.15:
            f = min(1.0, (z - 0.15) / 0.35)
            kivrim = 1 + 0.16 * f * math.sin(aci * 9 + kivrim_faz) + 0.07 * f * math.sin(aci * 17)
            x *= (1 - 0.86 * f) * kivrim
            y *= (1 - 0.86 * f) * kivrim
            z += 0.12 * f
        # Genel oran ve yana yatıklık.
        x *= genislik
        y *= derinlik
        x += egim * (z + 0.5) * 0.25
        v.co = (x, y, z)
    # Küçük düzensizlik: her köşe normali boyunca biraz itilir.
    bmesh.ops.recalc_face_normals(b, faces=b.faces)
    for v in b.verts:
        v.co += v.normal * random.uniform(-0.012, 0.012)
    b.to_mesh(n.data)
    b.free()
    # Hafif yumuşatma: sivri dikenler gitsin ama tümsekler ve kıvrımlar kalsın.
    mod = n.modifiers.new("Yumusat", "SMOOTH")
    mod.factor = 0.6
    mod.iterations = 2
    # Naylonun kırışıklıkları: hücreli (Voronoi) bir desenle kabartma; buruşuk naylonun
    # keskin kat izlerine benziyor.
    doku = bpy.data.textures.new(ad + "_Kirisik", type="VORONOI")
    doku.noise_scale = 0.09
    kab = n.modifiers.new("Kirisik", "DISPLACE")
    kab.texture = doku
    kab.strength = 0.03

    # 5) Düğüm ve iki kulak.
    tepe = 0.5 + 0.12
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=10, radius=0.07, location=(egim * 0.28, 0, tepe))
    dugum = bpy.context.active_object
    dugum.scale = (1.0, 0.85, 0.8)
    ortak.ata(dugum, mat)
    parcalar = [n, dugum]
    for yon in (-1, 1):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=10, radius=0.12,
                                             location=(egim * 0.28 + yon * 0.11, 0, tepe + 0.07))
        kulak = bpy.context.active_object
        kulak.scale = (1.0, 0.22, 0.5)               # yassı, sarkık bir yaprak
        kulak.rotation_euler = (random.uniform(-0.3, 0.3), yon * 0.7, random.uniform(-0.5, 0.5))
        ortak.ata(kulak, mat)
        parcalar.append(kulak)

    p = ortak.birlestir(parcalar, ad)
    ortak.yumusat(p, 80)
    # Pivot: tabanın ortası (Unity'de y = 0 yere oturur). Önce en alt noktayı sıfıra getir.
    alt = min((p.matrix_world @ v.co).z for v in p.data.vertices)
    p.location.z -= alt
    bpy.ops.object.select_all(action="DESELECT")
    p.select_set(True)
    bpy.context.view_layer.objects.active = p
    bpy.ops.object.transform_apply(location=True)
    ortak.pivotu_dunya_merkezine_al(p)
    # Yüksekliği tam 1 m yap: Unity'de ölçek doğrudan poşetin boyu olsun.
    olcek = 1.0 / p.dimensions.z
    p.scale = (olcek, olcek, olcek)
    bpy.ops.object.transform_apply(scale=True)
    return p


ortak.sahneyi_temizle()
# Siyah naylon: çok koyu, hafif mavimsi, parlak (pürüz 0,3): lamba ışığında parlama yapar.
naylon = ortak.malzeme("Cop_Naylon", (0.012, 0.012, 0.016), puruz=0.3)
a = poset("Poset_A", 11, 1.00, 0.95, -0.36, 0.0, naylon)
b = poset("Poset_B", 23, 0.85, 0.80, -0.40, 0.25, naylon)
c = poset("Poset_C", 37, 1.15, 1.05, -0.28, -0.1, naylon)
# Üç parça da merkezde (her birinin pivotu kendi tabanında). Önce dışa aktar; önizleme
# istenirse ancak ondan SONRA yan yana diz: Unity'ye giden dosya etkilenmesin.
ortak.kaydet_ve_disari_aktar("Cop_Poseti", [a, b, c])
for n in (a, b, c):
    print("OLCU", n.name, round(n.dimensions.x, 3), round(n.dimensions.y, 3), round(n.dimensions.z, 3))
if "--onizleme" in sys.argv:
    b.location.x, c.location.x = 1.3, -1.3
    ortak.onizleme_al(sys.argv[sys.argv.index("--onizleme") + 1], 1.0, 3.2)
