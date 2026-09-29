# sarkacli_saat.py
# 2. katta Nuri Bey'in antresindeki sarkaçlı duvar saati (#83, Şahan'ın seçimi 2026-09-29).
# Ceviz kasa, üstte sivri alınlık, camlı kapağın ardında beyaz kadran (rakam yerine saat çizgileri,
# kalın 12-3-6-9), akrep ve yelkovan 10:10'u gösterir; altta camın ardında pirinç sarkaç. Sarkaç ayrı
# parça: Unity'de SarkacSallanir onu duvar düzleminde sallar. Nuri Bey hastanedeyken saat hâlâ çalışıyor.
#
# PARÇALAR
#   Saat   : kasa, kadran, akrep-yelkovan, cam çerçeve. Pivot ARKA yüzün (duvar) ortası, 0,45 m yükseklikte.
#   Sarkac : çubuk ve yuvarlak ağırlık. Pivotu asıldığı nokta (kadranın altı); Unity'de yerel z ekseni
#            (duvara dik) etrafında sallanır.
# Ön yüz -y; Unity'de +Z. Duvar eşyası: merkez yüksekliğiyle duvarın iç yüzüne konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Saat_Ceviz", (0.07, 0.032, 0.012), puruz=0.35)
koyu = ortak.malzeme("Saat_Koyu", (0.03, 0.013, 0.005), puruz=0.45)
kadran_m = ortak.malzeme("Saat_Kadran", (0.82, 0.80, 0.72), puruz=0.5)
siyah = ortak.malzeme("Saat_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
pirinc = ortak.malzeme("Saat_Pirinc", (0.55, 0.40, 0.16), puruz=0.3, metal=0.85)
cam = ortak.malzeme("Saat_Cam", (0.30, 0.32, 0.33), puruz=0.05, metal=0.3)

EN, DER, BOY = 0.34, 0.12, 0.90
# Pivot: arka yüzün ortası, kasanın ortasında (z = 0 kasa ortası). Kasa -BOY/2 .. +BOY/2.
yc = -DER / 2
p = []
kasa = ortak.kutu("Kasa", (EN, DER, BOY), (0, yc, 0), ceviz, 0.008)
# Alt bölmenin içi oyuk (sarkaç orada sallanır); önü açık, camı ayrı.
ortak.oy(kasa, ortak.kutu("Oyuk", (EN - 0.06, DER, BOY * 0.52), (0, yc - 0.02, -BOY * 0.2), ceviz))
p.append(kasa)
p.append(ortak.kutu("IcArka", (EN - 0.065, 0.004, BOY * 0.52), (0, -0.012, -BOY * 0.2), koyu))
# Alınlık: tepede basamaklı sivri taç ve iki yan topuz.
p.append(ortak.kutu("Taç1", (EN + 0.04, DER + 0.02, 0.03), (0, yc - 0.01, BOY / 2 + 0.015), koyu, 0.004))
p.append(ortak.kutu("Taç2", (EN * 0.7, DER, 0.04), (0, yc, BOY / 2 + 0.05), ceviz, 0.006))
p.append(ortak.kutu("Taç3", (EN * 0.35, DER - 0.02, 0.04), (0, yc, BOY / 2 + 0.09), ceviz, 0.006))
p.append(ortak.kutu("Kaide", (EN + 0.04, DER + 0.02, 0.03), (0, yc - 0.01, -BOY / 2 - 0.015), koyu, 0.004))

# Kadran: üst bölmede, önü -y'ye bakan disk; etrafında pirinç halka; saat çizgileri.
KZ = BOY / 2 - 0.19                                   # kadranın merkezi
on = -DER - 0.001


def dik_disk(ad, r, kal, y, z, mat, kose=40):
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=kal, location=(0, y, z), rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


dik_disk("Kadran", 0.13, 0.006, on + 0.002, KZ, kadran_m)
bpy.ops.mesh.primitive_torus_add(major_radius=0.135, minor_radius=0.008, location=(0, on, KZ), rotation=(math.pi / 2, 0, 0), major_segments=40, minor_segments=8)
h = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(h, pirinc)
p.append(h)
for i in range(12):
    a = 2 * math.pi * i / 12
    kalin = i % 3 == 0
    uz = 0.03 if kalin else 0.018
    r = 0.105
    # Çizgi: kadranın düzleminde (x-z), merkeze doğru uzanan ince kutu. Açılı yerleşim için
    # küp eklenirken y ekseni etrafında döndürülür (döndürme meshe işlenir).
    bpy.ops.mesh.primitive_cube_add(size=1, location=(math.sin(a) * r, on - 0.001, KZ + math.cos(a) * r), rotation=(0, a, 0))
    t = bpy.context.active_object
    t.scale = (0.010 if kalin else 0.005, 0.002, uz)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(t, siyah)
    p.append(t)
# Akrep ve yelkovan (10:10): merkezden çıkan iki ince kutu.
for ad, aci, uz, en in (("Akrep", -60.0, 0.065, 0.010), ("Yelkovan", 60.0, 0.095, 0.006)):
    a = math.radians(aci)
    bpy.ops.mesh.primitive_cube_add(size=1, location=(math.sin(a) * uz / 2, on - 0.003, KZ + math.cos(a) * uz / 2), rotation=(0, a, 0))
    t = bpy.context.active_object
    t.scale = (en, 0.002, uz)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(t, siyah)
    p.append(t)
dik_disk("Mil", 0.008, 0.006, on - 0.004, KZ, pirinc, kose=12)
# Alt bölmenin camı ve çerçevesi (sarkaç arkasında görünür).
CZ, CBOY = -BOY * 0.2, BOY * 0.5
p.append(ortak.kutu("Cam", (EN - 0.07, 0.003, CBOY), (0, on + 0.004, CZ), cam))
for dz in (-1, 1):
    p.append(ortak.kutu("CamCerceve", (EN - 0.04, 0.01, 0.02), (0, on + 0.002, CZ + dz * (CBOY / 2 + 0.01)), koyu, 0.002))
for dx in (-1, 1):
    p.append(ortak.kutu("CamCerceve", (0.02, 0.01, CBOY + 0.04), (dx * (EN / 2 - 0.02), on + 0.002, CZ), koyu, 0.002))
saat = ortak.birlestir(p, "Saat")
ortak.yumusat(saat, 30)
ortak.pivotu_tasi(saat, (0, 0, 0))

# Sarkaç: kadranın altından (asıldığı nokta) aşağı inen çubuk ve yuvarlak pirinç ağırlık.
ASKI_Z = KZ - 0.15
SY = -DER / 2 + 0.01
p = []
p.append(ortak.kutu("Cubuk", (0.008, 0.004, 0.30), (0, SY, ASKI_Z - 0.15), pirinc))
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.045, depth=0.012, location=(0, SY, ASKI_Z - 0.32), rotation=(math.pi / 2, 0, 0))
w = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(w, pirinc)
p.append(w)
sarkac = ortak.birlestir(p, "Sarkac")
ortak.pivotu_tasi(sarkac, (0, SY, ASKI_Z))

ortak.calistir_ve_bitir("Sarkacli_Saat", [saat, sarkac], 1.0, 1.6, onizlemede_kaldir=0.6)
