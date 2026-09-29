# kus_kafesi.py
# 2. katta Nuri Bey'in holündeki ayaklı kuş kafesi ve muhabbet kuşu (#83, Şahan'ın seçimi 2026-09-29).
# Nuri Bey'in yalnızlığına arkadaş: sarı-yeşil muhabbet kuşu, askılı ayağa asılmış telden kubbe
# kafeste tüneğinde duruyor. Yem kabı boş, suyu bitmiş; Nuri Bey günlerdir yok.
#
# PARÇALAR
#   Kus_Kafesi : tek parça (ayak, askı kolu, kafes, tünek, kuş, kaplar). Pivot ayağın tabanının ortası.
# Ön yüz -y; Unity'de +Z. Teller ince silindirler; kavisli kubbe telleri noktalar arası kısa parçalar.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy
import mathutils

ortak.sahneyi_temizle()
metal = ortak.malzeme("Kafes_Tel", (0.55, 0.50, 0.40), puruz=0.35, metal=0.8)     # eski pirinç tel
ayak_m = ortak.malzeme("Kafes_Ayak", (0.05, 0.05, 0.05), puruz=0.5, metal=0.6)
taban_m = ortak.malzeme("Kafes_Taban", (0.30, 0.34, 0.30), puruz=0.6)
tunek_m = ortak.malzeme("Kafes_Tunek", (0.30, 0.18, 0.08), puruz=0.6)
yesil = ortak.malzeme("Kus_Yesil", (0.20, 0.45, 0.08), puruz=0.6)
sari = ortak.malzeme("Kus_Sari", (0.70, 0.60, 0.08), puruz=0.6)
siyah = ortak.malzeme("Kus_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
kap = ortak.malzeme("Kafes_Kap", (0.70, 0.66, 0.55), puruz=0.4)

p = []


def cubuk(a, b, r, mat, kose=8):
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)


# Ayak: üç ayaklı taban, dik direk, tepede kafesi taşıyan kıvrık kol.
for i in range(3):
    a = 2 * math.pi * i / 3
    cubuk((0, 0, 0.06), (math.cos(a) * 0.25, math.sin(a) * 0.25, 0.01), 0.012, ayak_m)
cubuk((0, 0, 0.0), (0, 0, 1.62), 0.013, ayak_m)
cubuk((0, 0, 1.62), (0, -0.18, 1.70), 0.01, ayak_m)
cubuk((0, -0.18, 1.70), (0, -0.28, 1.62), 0.01, ayak_m)

# Kafes: kolun ucundan sarkan halka; altında silindir gövde ve kubbe.
KX, KY = 0, -0.28
R, ALT, UST = 0.17, 1.02, 1.36                # kafesin yarıçapı, tabanı, gövdenin tepesi
cubuk((KX, KY, 1.62), (KX, KY, 1.52), 0.006, metal)
p.append(ortak.silindir("Taban", R + 0.01, 0.04, (KX, KY, ALT - 0.02), taban_m, kose=32))
N = 20
for i in range(N):
    a = 2 * math.pi * i / N
    x, y = KX + math.cos(a) * R, KY + math.sin(a) * R
    cubuk((x, y, ALT), (x, y, UST), 0.0025, metal, kose=5)
    # Kubbe teli: gövdenin tepesinden tepe noktasına kavis (dört kısa parça).
    onceki = (x, y, UST)
    for k in range(1, 5):
        t = k / 4
        rr = R * math.cos(t * math.pi / 2)
        z = UST + 0.16 * math.sin(t * math.pi / 2)
        nokta = (KX + math.cos(a) * rr, KY + math.sin(a) * rr, z)
        cubuk(onceki, nokta, 0.0025, metal, kose=5)
        onceki = nokta
for z in (ALT + 0.01, ALT + 0.12, UST):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=0.003, location=(KX, KY, z), major_segments=32, minor_segments=5)
    ortak.ata(bpy.context.active_object, metal)
    p.append(bpy.context.active_object)
cubuk((KX, KY, UST + 0.16), (KX, KY, 1.52), 0.004, metal)

# Tünek ve kuş: kafesin ortasında yatay tünek, üstünde yan dönmüş muhabbet kuşu.
cubuk((KX - R + 0.01, KY, ALT + 0.15), (KX + R - 0.01, KY, ALT + 0.15), 0.006, tunek_m)
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.035, location=(KX, KY, ALT + 0.20), segments=16, ring_count=10)
g = bpy.context.active_object
g.scale = (0.9, 0.8, 1.25)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(g, yesil)
p.append(g)
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.022, location=(KX, KY - 0.012, ALT + 0.26), segments=14, ring_count=8)
ortak.ata(bpy.context.active_object, sari)
p.append(bpy.context.active_object)
p.append(ortak.kutu("Gaga", (0.008, 0.012, 0.01), (KX, KY - 0.033, ALT + 0.255), sari))
for x in (KX - 0.012, KX + 0.012):
    p.append(ortak.kutu("Goz", (0.005, 0.004, 0.005), (x, KY - 0.03, ALT + 0.268), siyah))
p.append(ortak.kutu("Kuyruk", (0.018, 0.012, 0.07), (KX, KY + 0.028, ALT + 0.15), yesil))
# Yem ve su kapları: kafesin kenarında, boş.
for x in (KX - 0.10, KX + 0.10):
    kap_n = ortak.silindir("Kap", 0.03, 0.03, (x, KY - R + 0.035, ALT + 0.045), kap, kose=16)
    ortak.oy(kap_n, ortak.silindir("KapIc", 0.026, 0.03, (x, KY - R + 0.035, ALT + 0.055), kap, kose=16))
    p.append(kap_n)

kafes = ortak.birlestir(p, "Kus_Kafesi")
ortak.yumusat(kafes, 40)
ortak.pivotu_tasi(kafes, (0, 0, 0))
ortak.calistir_ve_bitir("Kus_Kafesi", [kafes], 1.7, 2.8)
