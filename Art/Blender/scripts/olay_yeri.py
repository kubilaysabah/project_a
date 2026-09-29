# olay_yeri.py
# 2. katta Nuri Bey'in holünde fenalaştığı yer (#83, Şahan'ın seçimi 2026-09-29). Üç ayrı model:
#   Sacilmis_Ilaclar : yere dağılmış ilaç kutuları, devrik şişeler, haplar ve devrilmiş su bardağı.
#   Devrik_Sandalye  : yan yatmış hasır sandalye (ayakta kurulur, sonra bütün olarak yana yatırılır).
#   Kirik_Gozluk     : camı çatlamış, sapı açık kalmış gözlük.
# Her biri kendi FBX'ine (ortak.calistir_ve_bitir tek ad alır): betik üç kez çalışır, argümanla seçilir:
#   blender -b --factory-startup --python olay_yeri.py -- ilaclar | sandalye | gozluk
# Ön yüz -y; Unity'de +Z. Pivot hepsinde döşemedeki ortası.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "ilaclar"
ortak.sahneyi_temizle()
rnd = random.Random(1203)
p = []


def parca(ad, olcu, konum, rot, mat, pah=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=konum, rotation=rot)
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    if pah:
        ortak.pah(n, pah)
    ortak.ata(n, mat)
    p.append(n)
    return n


def silindir_yatik(ad, r, boy, konum, aci_z, mat):
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=r, depth=boy, location=konum, rotation=(0, math.pi / 2, aci_z))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


if hangi == "ilaclar":
    beyaz = ortak.malzeme("Ilac_Kutu", (0.80, 0.80, 0.76), puruz=0.6)
    kirmizi = ortak.malzeme("Ilac_Serit", (0.50, 0.06, 0.05), puruz=0.6)
    mavi = ortak.malzeme("Ilac_Serit_Mavi", (0.08, 0.16, 0.45), puruz=0.6)
    kahve = ortak.malzeme("Ilac_Sise_Kahve", (0.20, 0.08, 0.02), puruz=0.2)
    hap = ortak.malzeme("Ilac_Hap", (0.85, 0.84, 0.80), puruz=0.4)
    cam = ortak.malzeme("Su_Bardagi", (0.55, 0.60, 0.60), puruz=0.05)
    su = ortak.malzeme("Su_Birikinti", (0.20, 0.22, 0.24), puruz=0.02)
    for i in range(4):
        x, y, a = rnd.uniform(-0.3, 0.3), rnd.uniform(-0.2, 0.2), rnd.uniform(0, math.pi)
        parca("Kutu", (0.09, 0.05, 0.03), (x, y, 0.015), (0, 0, a), beyaz, 0.002)
        parca("Serit", (0.092, 0.018, 0.031), (x, y, 0.015), (0, 0, a), kirmizi if i % 2 else mavi)
    for i in range(3):
        silindir_yatik("Sise", 0.022, 0.08, (rnd.uniform(-0.35, 0.35), rnd.uniform(-0.25, 0.25), 0.022), rnd.uniform(0, math.pi), kahve)
    for i in range(24):
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.004, location=(rnd.uniform(-0.4, 0.4), rnd.uniform(-0.3, 0.3), 0.003), segments=8, ring_count=5)
        h = bpy.context.active_object
        h.scale = (1.4, 1.0, 0.6)
        bpy.ops.object.transform_apply(scale=True)
        ortak.ata(h, hap)
        p.append(h)
    # Devrilmiş su bardağı ve yere yayılmış su birikintisi (parlak, yassı).
    silindir_yatik("Bardak", 0.035, 0.10, (0.28, 0.18, 0.035), 0.6, cam)
    bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=0.16, depth=0.001, location=(0.18, 0.10, 0.0006))
    b = bpy.context.active_object
    b.scale = (1.3, 0.7, 1)
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(b, su)
    p.append(b)
    ad, olcu_h = "Sacilmis_Ilaclar", 0.1

elif hangi == "sandalye":
    ahsap = ortak.malzeme("Hasir_Sandalye_Ahsap", (0.10, 0.05, 0.02), puruz=0.45)
    hasir = ortak.malzeme("Hasir_Orgu", (0.45, 0.34, 0.16), puruz=0.9)
    # Ayakta kurulur (oturak 0,45 m): dört ayak, oturak çerçevesi ve hasır örgü, iki çıtalı sırt.
    for x in (-0.19, 0.19):
        for y in (-0.19, 0.19):
            h = 0.90 if y > 0 else 0.45
            p.append(ortak.kutu("Ayak", (0.035, 0.035, h), (x, y, h / 2), ahsap, 0.004))
    p.append(ortak.kutu("Cerceve", (0.42, 0.42, 0.03), (0, 0, 0.45), ahsap, 0.004))
    for i in range(10):
        p.append(ortak.kutu("Orgu", (0.37, 0.034, 0.012), (0, -0.17 + i * 0.038, 0.466), hasir, 0.003))
    for z in (0.62, 0.82):
        p.append(ortak.kutu("SirtCita", (0.38, 0.025, 0.06), (0, 0.19, z), ahsap, 0.004))
    for z in (0.14,):
        for yon in (-1, 1):
            p.append(ortak.kutu("Kusak", (0.02, 0.38, 0.02), (yon * 0.19, 0, z), ahsap))
    ad, olcu_h = "Devrik_Sandalye", 0.5

else:
    cerceve = ortak.malzeme("Gozluk_Cerceve", (0.05, 0.03, 0.02), puruz=0.3)
    cam = ortak.malzeme("Gozluk_Cam", (0.50, 0.55, 0.55), puruz=0.05)
    catlak = ortak.malzeme("Gozluk_Catlak", (0.85, 0.88, 0.88), puruz=0.3)
    # Gözlük yere düşmüş: camlar hafif eğik, sağ sapı açık, sol sapı kapanmış.
    for x in (-0.034, 0.034):
        bpy.ops.mesh.primitive_torus_add(major_radius=0.024, minor_radius=0.0025, location=(x, 0, 0.012), rotation=(math.radians(80), 0, 0), major_segments=20, minor_segments=6)
        t = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(t, cerceve)
        p.append(t)
        bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=0.023, depth=0.002, location=(x, 0, 0.012), rotation=(math.radians(80), 0, 0))
        c = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(c, cam)
        p.append(c)
    parca("Kopru", (0.02, 0.003, 0.003), (0, 0, 0.03), (0, 0, 0), cerceve)
    parca("SapSag", (0.003, 0.12, 0.003), (0.06, 0.06, 0.02), (0, 0, 0), cerceve)
    parca("SapSol", (0.10, 0.003, 0.003), (0.0, 0.006, 0.02), (0, 0, 0), cerceve)
    # Çatlak: sağ camda üç ince beyaz çizgi.
    for a in (0.3, 1.2, 2.3):
        parca("Catlak", (0.022, 0.0005, 0.0006), (0.034, -0.0025, 0.012), (math.radians(80), a, 0), catlak)
    ad, olcu_h = "Kirik_Gozluk", 0.05

nesne = ortak.birlestir(p, ad)
if hangi == "sandalye":
    # Bütün sandalye yan yatırılır: dünya merkezi (pivot) etrafında y ekseninde 90° (bu kez isteyerek),
    # sonra döşemeye oturtulur.
    nesne.rotation_euler = (0, math.radians(90), math.radians(20))
    bpy.context.view_layer.objects.active = nesne
    bpy.ops.object.transform_apply(rotation=True)
    alt = min((nesne.matrix_world @ v.co).z for v in nesne.data.vertices)
    for v in nesne.data.vertices:
        v.co.z -= alt
ortak.yumusat(nesne, 35)
ortak.pivotu_tasi(nesne, (0, 0, 0))
ortak.calistir_ve_bitir(ad, [nesne], olcu_h, 1.0 if hangi != "sandalye" else 2.0)
