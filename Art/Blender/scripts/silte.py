# silte.py
# Yere bırakılmış çıplak şilte (#83, 1. katın Oda A'sı; Şahan'ın seçimi 2026-09-29).
# Karyola sökülüp duvara dayanmış, şilte örtüsüz yerde kalmış: kapitone dikiş çizgileri ve düğmeleri,
# kenarında biye, ortası yıllarca yatılmaktan hafif çökük, bir köşesinde sararmış leke.
#
# PARÇALAR
#   Silte : tek parça. Pivot tabanın ortası. Uzun kenar Blender y boyunca (Unity'de z); ön yüz -y.
# Ölçü 1,40 (en) x 1,90 (boy) x 0,18 (kalınlık).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
kumas = ortak.malzeme("Silte_Kumas", (0.62, 0.58, 0.50), puruz=0.95)      # sararmış çizgili şilte kumaşı
cizgi = ortak.malzeme("Silte_Cizgi", (0.40, 0.44, 0.52), puruz=0.95)      # soluk mavi çizgiler
biye = ortak.malzeme("Silte_Biye", (0.30, 0.30, 0.34), puruz=0.9)
leke = ortak.malzeme("Silte_Leke", (0.45, 0.36, 0.20), puruz=0.95)

EN, BOY, KAL = 1.40, 1.90, 0.18
p = [ortak.kutu("Govde", (EN, BOY, KAL), (0, 0, KAL / 2), kumas, 0.05)]
# Boyuna soluk mavi çizgiler (kumaş deseni): üst yüzde ince şeritler.
for i in range(9):
    x = -EN / 2 + 0.10 + i * (EN - 0.2) / 8
    p.append(ortak.kutu("Cizgi", (0.025, BOY - 0.08, 0.002), (x, 0, KAL + 0.0005), cizgi))
# Kapitone düğmeler: üç sıra.
for x in (-0.40, 0.0, 0.40):
    for y in (-0.60, -0.20, 0.20, 0.60):
        p.append(ortak.silindir("Dugme", 0.018, 0.012, (x, y, KAL - 0.002), biye, kose=12))
# Biye: üst kenarı çevreleyen koyu şerit.
for yon in (-1, 1):
    p.append(ortak.kutu("BiyeU", (EN, 0.02, 0.02), (0, yon * (BOY / 2 - 0.01), KAL - 0.012), biye, 0.006))
    p.append(ortak.kutu("BiyeY", (0.02, BOY, 0.02), (yon * (EN / 2 - 0.01), 0, KAL - 0.012), biye, 0.006))
# Sararmış leke: bir köşeye yakın, yüzeyin hemen üstünde yassı düzensiz bir disk.
bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=0.16, depth=0.002, location=(0.35, 0.55, KAL + 0.001))
l = bpy.context.active_object
l.scale = (1.3, 0.8, 1)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(l, leke)
p.append(l)

silte = ortak.birlestir(p, "Silte")
# Ortadaki çöküklük: üst yüzün ortasındaki köşe noktaları biraz aşağı itilir.
for v in silte.data.vertices:
    if v.co.z > KAL * 0.7:
        uzak = (v.co.x / (EN / 2)) ** 2 + (v.co.y / (BOY / 2)) ** 2
        v.co.z -= 0.025 * max(0.0, 1.0 - uzak)
ortak.yumusat(silte, 40)
ortak.pivotu_tasi(silte, (0, 0, 0))
ortak.calistir_ve_bitir("Silte", [silte], 0.2, 3.0)
