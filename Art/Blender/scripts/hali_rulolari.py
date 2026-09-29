# hali_rulolari.py
# Taşınan ailenin rulo yapılıp iplerle bağlanmış halıları (#83, 1. katın holü; Şahan'ın seçimi 2026-09-29).
# İki halı duvara dayalı dik duruyor, üçüncüsü (yolluk) önlerinde yerde yatıyor; hepsi götürülmeyi bekliyor.
# Rulonun ucunda halının kat kat sarıldığı görünsün diye koyu bir halka; gövdede iki-üç ip bağı.
#
# PARÇALAR
#   Hali_Rulolari : tek parça. Pivot ARKA düzlemin (duvarın) alt ortası: y = 0 duvar, z = 0 döşeme.
# Ön yüz -y tarafında; Unity'de +Z. Döşemeye ve duvara yaslı konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
bordo = ortak.malzeme("Hali_Bordo", (0.28, 0.04, 0.04), puruz=0.95)
lacivert = ortak.malzeme("Hali_Lacivert", (0.04, 0.06, 0.18), puruz=0.95)
kiremit = ortak.malzeme("Hali_Kiremit", (0.40, 0.14, 0.06), puruz=0.95)
uc = ortak.malzeme("Hali_Uc", (0.55, 0.45, 0.30), puruz=0.95)       # halının tersi (sarılmış kat)
ip = ortak.malzeme("Hali_Ip", (0.60, 0.52, 0.36), puruz=0.9)

p = []


def dik_rulo(x, r, boy, mat):
    """Duvara dayalı dik rulo: gövde, iki ucunda ters yüzün halkası, iki ip bağı."""
    y = -r - 0.01
    p.append(ortak.silindir("Rulo", r, boy, (x, y, boy / 2), mat, kose=24))
    p.append(ortak.silindir("UcHalka", r * 0.75, 0.004, (x, y, boy + 0.002), uc, kose=24))
    for z in (boy * 0.25, boy * 0.75):
        p.append(ortak.silindir("Ip", r + 0.004, 0.018, (x, y, z), ip, kose=24))


def yatik_rulo(x0, x1, r, mat):
    """Yerde yatan rulo: silindir eklenirken y ekseni etrafında 90° döndürülür (x boyunca uzanır)
    ve dönüş meshe işlenir (ortak.kutu tuzağı yok)."""
    y = -0.30 - r
    for ad, yc, boy, m in (("Yatik", r, x1 - x0, mat), ("IpA", r + 0.004, 0.018, ip), ("IpB", r + 0.004, 0.018, ip)):
        xm = (x0 + x1) / 2 if ad == "Yatik" else (x0 + (x1 - x0) * (0.25 if ad == "IpA" else 0.75))
        bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=yc, depth=boy, location=(xm, y, r), rotation=(0, math.pi / 2, 0))
        n = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(n, m)
        p.append(n)
    for xu in (x0 - 0.002, x1 + 0.002):
        bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=r * 0.75, depth=0.004, location=(xu, y, r), rotation=(0, math.pi / 2, 0))
        n = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(n, uc)
        p.append(n)


dik_rulo(0.45, 0.11, 1.55, bordo)       # büyük salon halısı
dik_rulo(0.70, 0.09, 1.25, lacivert)    # küçük oda halısı
yatik_rulo(-0.75, 0.55, 0.08, kiremit)  # holün yolluğu, önlerinde yerde

rulolar = ortak.birlestir(p, "Hali_Rulolari")
ortak.yumusat(rulolar, 40)
ortak.pivotu_tasi(rulolar, (0, 0, 0))
ortak.calistir_ve_bitir("Hali_Rulolari", [rulolar], 1.55, 3.0)
