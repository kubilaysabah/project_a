# posta_yigini.py
# Kapının dibinde birikmiş gazeteler ve zarflar (#83, 2. kat antresi; Şahan'ın seçimi 2026-09-29).
# Nuri Bey günlerdir yok: kapının altından atılan gazeteler ve mektuplar içeride, kapının açılış
# yolundan kenara itilmiş hâlde yığılı. Katlanmış gazeteler (birkaçı açılmış), beyaz ve kahverengi
# fatura zarfları, bir tane kırmızı-mavi kenarlı uçak postası mektubu (uzaktaki oğlundan).
#
# PARÇALAR
#   Posta_Yigini : tek parça. Pivot yığının ortası, döşemede. Ön yüz -y.
# Hafif dönük parçalar küp eklenirken z ekseni etrafında döndürülür, dönüş meshe işlenir.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
rnd = random.Random(912)
gazete = ortak.malzeme("Posta_Gazete", (0.60, 0.58, 0.52), puruz=0.9)
baski = ortak.malzeme("Posta_Baski", (0.30, 0.29, 0.27), puruz=0.9)
zarf_b = ortak.malzeme("Posta_Zarf_Beyaz", (0.80, 0.79, 0.74), puruz=0.8)
zarf_k = ortak.malzeme("Posta_Zarf_Kraft", (0.45, 0.32, 0.18), puruz=0.8)
kirmizi = ortak.malzeme("Posta_Ucak_Kirmizi", (0.55, 0.05, 0.05), puruz=0.8)
mavi = ortak.malzeme("Posta_Ucak_Mavi", (0.05, 0.10, 0.45), puruz=0.8)

p = []


def parca(ad, olcu, x, y, z, aci, mat):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z), rotation=(0, 0, math.radians(aci)))
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


z = 0.0
# Katlanmış gazeteler: üst üste, her biri biraz dönük; üstlerinde baskı şeritleri (başlık ve sütunlar).
for i in range(5):
    x, y, a = rnd.uniform(-0.08, 0.08), rnd.uniform(-0.06, 0.06), rnd.uniform(-25, 25)
    k = 0.012
    parca("Gazete", (0.30, 0.20, k), x, y, z + k / 2, a, gazete)
    parca("Baslik", (0.26, 0.03, 0.001), x + math.sin(math.radians(-a)) * 0.07, y + math.cos(math.radians(a)) * 0.07, z + k + 0.0005, a, baski)
    parca("Sutun", (0.24, 0.11, 0.0008), x - math.sin(math.radians(-a)) * 0.02, y - math.cos(math.radians(a)) * 0.02, z + k + 0.0004, a, baski)
    z += k
# Açılıp yere yayılmış bir gazete sayfası (yığının yanında).
parca("AcikGazete", (0.55, 0.38, 0.002), 0.30, 0.10, 0.001, 18, gazete)
parca("AcikBaski", (0.48, 0.30, 0.0008), 0.30, 0.10, 0.0025, 18, baski)
# Zarflar: yığının üstüne ve kenarlarına saçılmış.
for i in range(7):
    x, y, a = rnd.uniform(-0.20, 0.22), rnd.uniform(-0.18, 0.16), rnd.uniform(-60, 60)
    mat = zarf_k if i % 3 == 0 else zarf_b
    zz = z + 0.002 + i * 0.002 if abs(x) < 0.12 and abs(y) < 0.1 else 0.002 + i * 0.001
    parca("Zarf", (0.22, 0.11, 0.003), x, y, zz, a, mat)
# Uçak postası: kenarında kırmızı-mavi çizgili zarf, en üstte.
ux, uy, ua, uz = -0.02, -0.02, 12, z + 0.02
parca("UcakZarf", (0.20, 0.10, 0.003), ux, uy, uz, ua, zarf_b)
for i in range(10):
    t = -0.09 + i * 0.02
    for kenar in (-1, 1):
        c, s = math.cos(math.radians(ua)), math.sin(math.radians(ua))
        yx, yy = t, kenar * 0.046
        parca("Serit", (0.01, 0.006, 0.001), ux + yx * c - yy * s, uy + yx * s + yy * c, uz + 0.002, ua + 35, kirmizi if i % 2 == 0 else mavi)

posta = ortak.birlestir(p, "Posta_Yigini")
ortak.pivotu_tasi(posta, (0, 0, 0))
ortak.calistir_ve_bitir("Posta_Yigini", [posta], 0.1, 1.2)
