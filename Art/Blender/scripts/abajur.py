# abajur.py
# Komodinin üstündeki başucu abajuru (#83, 1. katın Oda A'sı; Şahan'ın seçimi 2026-09-29).
# 90'ların abajuru: seramik gövde (krem, altın şeritli), üstünde pilili kumaş şapka hafif yamuk
# oturmuş; kablosu komodinin arkasından sarkıyor, fişi çekili (bu katta yanmaz).
#
# PARÇALAR
#   Abajur : tek parça. Pivot tabanın ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
seramik = ortak.malzeme("Abajur_Seramik", (0.72, 0.66, 0.52), puruz=0.25)
altin = ortak.malzeme("Abajur_Altin", (0.55, 0.40, 0.14), puruz=0.3, metal=0.8)
kumas = ortak.malzeme("Abajur_Kumas", (0.62, 0.50, 0.34), puruz=0.95)      # sararmış bej pilili kumaş
kablo = ortak.malzeme("Abajur_Kablo", (0.08, 0.07, 0.06), puruz=0.6)

p = []
# Gövde: üst üste silindirlerle vazo biçimi (taban, karın, boyun), bir altın şerit.
for ad, r, h, z, m in (("Taban", 0.060, 0.020, 0.010, seramik), ("Karin", 0.075, 0.12, 0.080, seramik),
                       ("Serit", 0.076, 0.012, 0.090, altin), ("Omuz", 0.055, 0.03, 0.155, seramik),
                       ("Boyun", 0.022, 0.06, 0.200, altin), ("Duy", 0.018, 0.04, 0.250, altin)):
    p.append(ortak.silindir(ad, r, h, (0, 0, z), m, kose=28))
# Şapka: kesik koni (alt geniş), yüzeyde pili çizgileri; hafif yamuk (x ekseni etrafında 6°).
bpy.ops.mesh.primitive_cone_add(vertices=24, radius1=0.17, radius2=0.10, depth=0.20, location=(0, 0, 0.31), rotation=(math.radians(6), 0, 0))
sapka = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
# Pililer: her ikinci köşe noktası biraz dışa itilir, yüzey dalgalanır.
for i, v in enumerate(sapka.data.vertices):
    if i % 2 == 0:
        v.co.x *= 1.04
        v.co.y *= 1.04
ortak.ata(sapka, kumas)
p.append(sapka)
# Kablo: gövdenin arkasından komodinin kenarına uzanan ince çubuk (arka kenardan aşağı sarkar).
p.append(ortak.kutu("Kablo", (0.006, 0.16, 0.006), (0.03, 0.10, 0.004), kablo))
p.append(ortak.kutu("KabloSarkan", (0.006, 0.006, 0.20), (0.03, 0.18, -0.10), kablo))

abajur = ortak.birlestir(p, "Abajur")
ortak.yumusat(abajur, 40)
ortak.pivotu_tasi(abajur, (0, 0, 0))
ortak.calistir_ve_bitir("Abajur", [abajur], 0.42, 1.2)
