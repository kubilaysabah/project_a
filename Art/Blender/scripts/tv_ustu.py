# tv_ustu.py
# Salondaki TV sehpasının üstü (#83, 1. katın salonu; Şahan'ın seçimi 2026-09-29): oval dantel örtü ve
# üstünde porselen bir biblo (mavi şeritli küçük vazo, içinde solmuş yapay çiçekler). Televizyon
# götürülmek için sehpadan indirilmiş (yanında yerde); örtü ve biblo yerinde kalmış, dantelin ortasında
# televizyonun ayağından kalan temiz bir iz var.
#
# PARÇALAR
#   TV_Ustu : tek parça. Pivot sehpanın üst yüzünün ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
dantel = ortak.malzeme("TV_Dantel", (0.86, 0.82, 0.72), puruz=0.9)
dantel_iz = ortak.malzeme("TV_Dantel_Iz", (0.95, 0.93, 0.88), puruz=0.9)   # televizyonun altında kalan, sararmamış kısım
porselen = ortak.malzeme("Biblo_Porselen", (0.90, 0.89, 0.86), puruz=0.2)
mavi = ortak.malzeme("Biblo_Mavi", (0.06, 0.12, 0.40), puruz=0.2)
cicek = ortak.malzeme("Biblo_Cicek", (0.45, 0.20, 0.24), puruz=0.8)       # solmuş gül kurusu
sap = ortak.malzeme("Biblo_Sap", (0.20, 0.25, 0.12), puruz=0.8)

p = []
# Oval dantel: x boyunca uzun (x ölçeği 1,6); kenarında fisto.
bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=0.22, depth=0.002, location=(0, 0, 0.001))
o = bpy.context.active_object
o.scale = (1.6, 1.0, 1.0)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(o, dantel)
p.append(o)
for i in range(36):
    a = 2 * math.pi * i / 36
    p.append(ortak.silindir("Fisto", 0.02, 0.002, (math.cos(a) * 0.22 * 1.6, math.sin(a) * 0.22, 0.001), dantel, kose=10))
# Televizyonun ayağından kalan açık renkli dikdörtgen iz.
p.append(ortak.kutu("Iz", (0.36, 0.26, 0.0005), (0.05, 0.02, 0.0025), dantel_iz))
# Biblo: dantelin sol ucunda, üst üste silindirlerle vazo profili (dip, gövde, boyun, ağız) ve mavi şerit.
bx = -0.24
for ad, r, h, z, m in (("Dip", 0.030, 0.010, 0.005, porselen), ("Govde", 0.042, 0.070, 0.045, porselen),
                       ("Serit", 0.043, 0.012, 0.050, mavi), ("Omuz", 0.032, 0.020, 0.090, porselen),
                       ("Boyun", 0.018, 0.040, 0.120, porselen), ("Agiz", 0.026, 0.008, 0.144, mavi)):
    p.append(ortak.silindir(ad, r, h, (bx, 0.0, z), m, kose=24))
# Çiçekler: üç ince sap ve uçlarında küçük kurumuş gül başları.
for dx, dy, h in ((0.0, 0.0, 0.14), (0.012, -0.008, 0.11), (-0.012, 0.006, 0.12)):
    p.append(ortak.kutu("Sap", (0.003, 0.003, h), (bx + dx, dy, 0.148 + h / 2), sap))
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.014, location=(bx + dx, dy, 0.148 + h), segments=10, ring_count=8)
    c = bpy.context.active_object
    ortak.ata(c, cicek)
    p.append(c)

ust = ortak.birlestir(p, "TV_Ustu")
ortak.yumusat(ust, 40)
ortak.pivotu_tasi(ust, (0, 0, 0))
ortak.calistir_ve_bitir("TV_Ustu", [ust], 0.3, 1.0)
