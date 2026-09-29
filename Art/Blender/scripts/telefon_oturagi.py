# telefon_oturagi.py
# 2. katta Nuri Bey'in antresindeki oturaklı telefon sehpası (#83, Şahan'ın seçimi 2026-09-29).
# 70'lerin "telefon oturağı": solda hardal rengi kapitone vinil oturak, sağda altında raf olan küçük
# sehpa; sivri ayaklar. Sehpanın üstünde dantel örtü ve koyu bakalit çevirmeli telefon (1. kattaki krem
# telefondan farklı model: yuvarlak kubbeli gövde, siyah); kablosu duvardaki prize takılı. Alt rafta
# büyük harfli, kalın bir telefon rehberi ve Nuri Bey'in el yazısıyla numara defteri.
#
# PARÇALAR
#   Telefon_Oturagi : tek parça. Pivot tabanın ortası, arka yüz (duvar) y = +0,19. Ön yüz -y.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
tik = ortak.malzeme("Oturak_Tik", (0.24, 0.11, 0.04), puruz=0.45)
tik_koyu = ortak.malzeme("Oturak_Tik_Koyu", (0.12, 0.05, 0.02), puruz=0.5)
vinil = ortak.malzeme("Oturak_Vinil", (0.42, 0.26, 0.04), puruz=0.35)      # hardal rengi vinil
dugme = ortak.malzeme("Oturak_Dugme", (0.20, 0.12, 0.02), puruz=0.4)
dantel = ortak.malzeme("Oturak_Dantel", (0.86, 0.82, 0.72), puruz=0.9)
bakalit = ortak.malzeme("Telefon_Bakalit", (0.02, 0.018, 0.016), puruz=0.25)
kadran = ortak.malzeme("Telefon_Kadran", (0.80, 0.78, 0.72), puruz=0.3)
metal = ortak.malzeme("Telefon_Nikel", (0.60, 0.60, 0.58), puruz=0.25, metal=0.9)
rehber_m = ortak.malzeme("Rehber_Kirmizi", (0.40, 0.05, 0.04), puruz=0.7)
defter_m = ortak.malzeme("Defter_Mavi", (0.06, 0.10, 0.30), puruz=0.7)
sayfa = ortak.malzeme("Rehber_Sayfa", (0.75, 0.72, 0.62), puruz=0.9)
kablo = ortak.malzeme("Telefon_Kablo", (0.03, 0.03, 0.03), puruz=0.6)
priz = ortak.malzeme("Priz", (0.80, 0.78, 0.72), puruz=0.4)

EN, DER = 0.95, 0.38
ARKA = DER / 2
p = []


def ayak(x, y, h):
    """Sivri ayak: alta doğru incelen iki kutu."""
    p.append(ortak.kutu("AyakUst", (0.035, 0.035, h * 0.5), (x, y, h * 0.75), tik_koyu, 0.004))
    p.append(ortak.kutu("AyakAlt", (0.022, 0.022, h * 0.5), (x, y, h * 0.25), tik_koyu, 0.003))


# Oturak: solda, 0,45 yükseklikte, kapitone vinil minder.
OX, OEN = -EN / 2 + 0.26, 0.50
p.append(ortak.kutu("OturakGovde", (OEN, DER, 0.06), (OX, 0, 0.38), tik, 0.006))
p.append(ortak.kutu("Minder", (OEN - 0.03, DER - 0.03, 0.08), (OX, 0, 0.45), vinil, 0.03))
for x in (OX - 0.12, OX, OX + 0.12):
    for y in (-0.08, 0.08):
        p.append(ortak.silindir("Kapitone", 0.012, 0.01, (x, y, 0.49), dugme, kose=10))
# Sırtlık: oturağın arkasında alçak, ince tik çubuklu.
p.append(ortak.kutu("SirtUst", (OEN, 0.03, 0.05), (OX, ARKA - 0.015, 0.78), tik, 0.006))
for x in (OX - 0.20, OX - 0.07, OX + 0.07, OX + 0.20):
    p.append(ortak.kutu("SirtCubuk", (0.02, 0.02, 0.30), (x, ARKA - 0.015, 0.60), tik_koyu, 0.003))
for x in (OX - OEN / 2 + 0.04, OX + OEN / 2 - 0.04):
    for y in (-ARKA + 0.04, ARKA - 0.04):
        ayak(x, y, 0.35)

# Sehpa: sağda, 0,62 yükseklikte tabla ve altta raf.
SX, SEN = EN / 2 - 0.22, 0.44
p.append(ortak.kutu("Tabla", (SEN, DER, 0.03), (SX, 0, 0.62), tik, 0.006))
p.append(ortak.kutu("Raf", (SEN - 0.04, DER - 0.04, 0.02), (SX, 0, 0.26), tik, 0.004))
for x in (SX - SEN / 2 + 0.03, SX + SEN / 2 - 0.03):
    for y in (-ARKA + 0.03, ARKA - 0.03):
        ayak(x, y, 0.61)
UST = 0.635
# Dantel: sehpanın üstünde dikdörtgen, kenarı fistolu.
p.append(ortak.kutu("Dantel", (0.34, 0.26, 0.002), (SX, -0.01, UST + 0.001), dantel))
for i in range(12):
    p.append(ortak.silindir("Fisto", 0.014, 0.002, (SX - 0.17 + i * 0.031, -0.14, UST + 0.001), dantel, kose=8))

# Telefon: kubbeli yuvarlak gövde (basık küre), önde çevirme diski, üstte çatallı ahize.
TX, TY = SX + 0.01, 0.0
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.11, location=(TX, TY, UST + 0.03), segments=28, ring_count=14)
g = bpy.context.active_object
g.scale = (1.0, 0.95, 0.55)
bpy.ops.object.transform_apply(scale=True)
for v in g.data.vertices:                                   # altı düz: küre alt yarısı tabana basılır
    if v.co.z < UST + 0.002:
        v.co.z = UST + 0.002
ortak.ata(g, bakalit)
p.append(g)
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.042, depth=0.01, location=(TX, TY - 0.075, UST + 0.07), rotation=(math.radians(55), 0, 0))
dk = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(dk, kadran)
p.append(dk)
bpy.ops.mesh.primitive_torus_add(major_radius=0.032, minor_radius=0.006, location=(TX, TY - 0.079, UST + 0.073), rotation=(math.radians(55), 0, 0), major_segments=24, minor_segments=6)
hk = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(hk, metal)
p.append(hk)
for x in (TX - 0.06, TX + 0.06):
    p.append(ortak.kutu("Catal", (0.02, 0.03, 0.04), (x, TY + 0.01, UST + 0.10), bakalit, 0.006))
p.append(ortak.kutu("AhizeSap", (0.20, 0.035, 0.03), (TX, TY + 0.01, UST + 0.13), bakalit, 0.012))
for x in (TX - 0.10, TX + 0.10):
    p.append(ortak.silindir("AhizeUc", 0.03, 0.035, (x, TY + 0.01, UST + 0.115), bakalit, kose=20))
# Kablo: telefonun arkasından duvara, duvarda prize kadar yükselir (takılı).
p.append(ortak.kutu("Kablo1", (0.008, ARKA - TY - 0.07, 0.008), (TX + 0.05, (TY + 0.07 + ARKA) / 2, UST + 0.01), kablo))
p.append(ortak.kutu("Kablo2", (0.008, 0.008, 0.30), (TX + 0.05, ARKA - 0.006, UST + 0.16), kablo))
p.append(ortak.kutu("Priz", (0.07, 0.02, 0.07), (TX + 0.05, ARKA - 0.01, UST + 0.33), priz, 0.006))

# Alt rafta rehber (kırmızı, kalın) ve üstünde numara defteri (mavi, ince).
p.append(ortak.kutu("Rehber", (0.26, 0.20, 0.05), (SX, 0.0, 0.295), rehber_m, 0.004))
p.append(ortak.kutu("RehberSayfa", (0.25, 0.195, 0.045), (SX + 0.002, -0.002, 0.295), sayfa))
p.append(ortak.kutu("Defter", (0.15, 0.10, 0.012), (SX - 0.03, -0.02, 0.326), defter_m, 0.002))

oturak = ortak.birlestir(p, "Telefon_Oturagi")
ortak.yumusat(oturak, 35)
ortak.pivotu_tasi(oturak, (0, 0, 0))
ortak.calistir_ve_bitir("Telefon_Oturagi", [oturak], 0.8, 2.2)
