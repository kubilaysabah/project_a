# tup_hortumu.py
# Ocaktan sökülmüş tüp hortumu (#83, 1. katın mutfağı; Şahan'ın seçimi 2026-09-29).
# Taşınan aile tüpü ocaktan ayırmış: turuncu gaz hortumu tüpün regülatöründen çıkıp yerde bir kavis
# çiziyor, ucundaki kelepçe boşta. Tüp ayrı model (tup.py); bu model yalnızca regülatör ve hortum.
#
# PARÇALAR
#   Tup_Hortumu : tek parça. Pivot tüpün tabanının ortası (tüple aynı nokta): regülatör tüpün tepesinde
#                 (tüp ~0,60 m), hortum oradan yere iner ve önde (-y) kavisle uzanır. Ön yüz -y.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
hortum = ortak.malzeme("Hortum", (0.55, 0.16, 0.03), puruz=0.6)       # turuncu gaz hortumu
metal = ortak.malzeme("Regulator", (0.55, 0.55, 0.52), puruz=0.3, metal=0.9)

TEPE = 0.60                         # tüpün vanasının yüksekliği
p = []
# Regülatör: tüpün vanasına takılı küçük gövde ve yana çıkan çıkış borusu.
p.append(ortak.silindir("Regulator", 0.035, 0.04, (0, 0, TEPE + 0.02), metal, kose=20))
p.append(ortak.kutu("Cikis", (0.06, 0.016, 0.016), (0.05, 0, TEPE + 0.02), metal, 0.003))

# Hortum: noktalar arasında kısa silindir parçaları (her biri iki nokta arasına yönlendirilir).
noktalar = [(0.08, 0.0, TEPE + 0.02), (0.13, -0.03, TEPE - 0.05), (0.16, -0.08, 0.35), (0.15, -0.14, 0.12),
            (0.10, -0.22, 0.02), (0.00, -0.32, 0.015), (-0.14, -0.36, 0.015), (-0.28, -0.30, 0.015), (-0.36, -0.20, 0.015)]
for (a, b) in zip(noktalar, noktalar[1:]):
    ax, ay, az = a
    bx, by, bz = b
    dx, dy, dz = bx - ax, by - ay, bz - az
    boy = math.sqrt(dx * dx + dy * dy + dz * dz)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.009, depth=boy + 0.01, location=((ax + bx) / 2, (ay + by) / 2, (az + bz) / 2))
    s = bpy.context.active_object
    # Silindirin ekseni z; iki nokta arasındaki yöne çevrilir, dönüş meshe işlenir.
    s.rotation_mode = "QUATERNION"
    import mathutils
    s.rotation_quaternion = mathutils.Vector((dx, dy, dz)).to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(s, hortum)
    p.append(s)
# Boşta kalan kelepçe: hortumun ucunda metal halka.
ux, uy, uz = noktalar[-1]
p.append(ortak.silindir("Kelepce", 0.013, 0.02, (ux, uy, uz), metal, kose=16))

h = ortak.birlestir(p, "Tup_Hortumu")
ortak.yumusat(h, 40)
ortak.pivotu_tasi(h, (0, 0, 0))
ortak.calistir_ve_bitir("Tup_Hortumu", [h], 0.6, 1.6)
