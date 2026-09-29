# sokuk_karyola.py
# Sökülüp duvara dayanmış çift kişilik karyola (#83, 1. katın Oda A'sı; Şahan'ın seçimi 2026-09-29).
# Taşınmaya hazırlanan ailenin karyolası: oymalı ceviz başlık, alçak ayak ucu ve iki uzun yan tahta,
# duvara eğik dayalı; şilte ayrıca yerde (silte.py).
#
# PARÇALAR
#   Sokuk_Karyola : tek parça. Pivot duvarın (arka düzlem, y = 0) döşemedeki ortası; parçalar duvardan
#                   odaya doğru (-y) eğik durur. Ön yüz -y; Unity'de +Z.
# Eğik parçalar küp eklenirken döndürülür ve dönüş meshe işlenir (ortak.kutu döndürülmez: tuzak).

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Karyola_Ceviz", (0.06, 0.028, 0.010), puruz=0.35)
koyu = ortak.malzeme("Karyola_Koyu", (0.03, 0.013, 0.005), puruz=0.45)
pirinc = ortak.malzeme("Karyola_Pirinc", (0.50, 0.38, 0.18), puruz=0.35, metal=0.6)

p = []


def egik(ad, en, boy, kal, x, aci_derece, mat, taban_y=0.0):
    """Duvara dayalı eğik levha: tabanı döşemede, üst kenarı duvara değer.
    en: x boyunca genişlik, boy: levhanın uzunluğu, kal: kalınlık, aci: duvardan açı."""
    a = math.radians(aci_derece)
    yc = taban_y - (boy / 2) * math.sin(a) - (kal / 2) * math.cos(a)
    zc = (boy / 2) * math.cos(a)
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, yc, zc), rotation=(-a, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    n.scale = (en, kal, boy)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.pah(n, 0.004)
    ortak.ata(n, mat)
    p.append(n)
    return n


# Yan tahtalar: en arkada, uzun ve ince, duvara neredeyse dik (8°).
egik("YanTahta1", 0.20, 1.95, 0.035, -0.55, 8, koyu)
egik("YanTahta2", 0.20, 1.95, 0.035, -0.30, 9, koyu, taban_y=-0.04)
# Başlık: ortada, geniş ve yüksek, 12° eğik; üst köşelerinde pirinç topuzlar. egik() levhanın üst
# kenarını taban_y hizasına, boy x cos(açı) yüksekliğine koyar; topuzlar o köşelerin hemen önünde.
egik("Baslik", 1.50, 1.05, 0.045, 0.25, 12, ceviz, taban_y=-0.08)
a = math.radians(12)
ust_z = 1.05 * math.cos(a)
# Kuşak: başlığın ön yüzünde, üst kenarın 3 cm altında. egik() onu döşemeye koyar; sonra dik olarak
# yukarı taşınır. Üst ucu 1,0 m'de başlığın ön yüzüne değsin diye taban_y o yükseklikteki ön yüz hizası.
on_y = -0.08 - 0.045 - (ust_z - 1.0) * math.tan(a)
egik("BaslikKusak", 1.50, 0.06, 0.02, 0.25, 12, koyu, taban_y=on_y)
p[-1].location.z += 1.0 - 0.06 * math.cos(a)
for x in (0.25 - 0.72, 0.25 + 0.72):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.035, location=(x, -0.08 - 0.045, ust_z + 0.02), segments=12, ring_count=8)
    t = bpy.context.active_object
    ortak.ata(t, pirinc)
    p.append(t)
# Ayak ucu: en önde, alçak, 16° eğik.
egik("AyakUcu", 1.50, 0.60, 0.04, 0.25, 16, ceviz, taban_y=-0.34)

karyola = ortak.birlestir(p, "Sokuk_Karyola")
ortak.yumusat(karyola, 30)
ortak.pivotu_tasi(karyola, (0, 0, 0))
ortak.calistir_ve_bitir("Sokuk_Karyola", [karyola], 1.1, 3.0)
