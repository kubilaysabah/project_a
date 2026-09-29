# gazeteli_tabaklar.py
# Açık kolinin içinde gazete kâğıdına sarılmış tabaklar (#83, 1. katın mutfağı; Şahan'ın seçimi 2026-09-29).
# Koli ayrı şablon (Koli_Acik); bu model yalnızca içindekiler: kolinin ağzından taşan buruşuk gazete
# topakları, arasında dik duran sarılı tabaklar ve en üstte sarılmamış tek bir tabak.
#
# PARÇALAR
#   Gazeteli_Tabaklar : tek parça. Pivot kolinin tabanının ortası (koliyle aynı nokta). Ön yüz -y.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
rnd = random.Random(831)
gazete = ortak.malzeme("Gazete_Kagit", (0.62, 0.60, 0.54), puruz=0.9)
gazete_koyu = ortak.malzeme("Gazete_Yazi", (0.34, 0.33, 0.30), puruz=0.9)     # baskı yoğun sayfalar
porselen = ortak.malzeme("Tabak_Porselen", (0.88, 0.87, 0.84), puruz=0.2)

p = []
UST = 0.40                         # kolinin ağzı (Koli_Acik iç yüksekliği ~0,46; kapakları açık)
# Buruşuk gazete topakları: kolinin ağzında, basık ve düzensiz küreler.
for i in range(9):
    x, y = rnd.uniform(-0.22, 0.22), rnd.uniform(-0.15, 0.15)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=rnd.uniform(0.06, 0.09), location=(x, y, UST + rnd.uniform(-0.02, 0.03)))
    t = bpy.context.active_object
    t.scale = (rnd.uniform(0.9, 1.3), rnd.uniform(0.8, 1.2), rnd.uniform(0.5, 0.8))
    bpy.ops.object.transform_apply(scale=True)
    for v in t.data.vertices:                  # buruşukluk: köşe noktaları biraz rastgele itilir
        v.co *= rnd.uniform(0.85, 1.12)
    ortak.ata(t, gazete if i % 3 else gazete_koyu)
    p.append(t)
# Dik duran sarılı tabaklar: yassı, kalınca diskler (gazeteye sarılı).
for i, x in enumerate((-0.12, -0.04, 0.05)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=0.12, depth=0.035, location=(x, 0.02, UST - 0.02), rotation=(0, math.pi / 2, 0))
    t = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    for v in t.data.vertices:
        v.co.x += rnd.uniform(-0.006, 0.006)
    ortak.ata(t, gazete)
    p.append(t)
# En üstte sarılmamış tek tabak, hafif eğik yatıyor (eklenirken döndürülür, dönüş meshe işlenir).
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.11, depth=0.012, location=(0.10, -0.05, UST + 0.06), rotation=(0.25, 0.1, 0))
t = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(t, porselen)
p.append(t)

tabaklar = ortak.birlestir(p, "Gazeteli_Tabaklar")
ortak.yumusat(tabaklar, 50)
ortak.pivotu_tasi(tabaklar, (0, 0, 0))
ortak.calistir_ve_bitir("Gazeteli_Tabaklar", [tabaklar], 0.5, 1.4)
