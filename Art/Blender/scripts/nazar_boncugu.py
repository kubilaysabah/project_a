# nazar_boncugu.py
# Antrede giriş kapısının üstüne asılı büyük nazar boncuğu (#83, 1. katın antresi).
# Türk evine girenin ilk gördüğü şey: lacivert, beyaz, açık mavi ve siyah katmanlı cam göz,
# duvardaki bir çiviye kırmızı bir iple asılı, altında üç küçük boncuk sallanıyor.
#
# PARÇALAR
#   Nazar_Boncugu : tek parça. Pivot camın ARKA yüzünün ortası (duvara değen nokta).
# Ön yüz -y tarafında; Unity'de +Z. Duvar eşyası: merkez yüksekliğiyle duvarın iç yüzüne konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
lacivert = ortak.malzeme("Nazar_Lacivert", (0.01, 0.03, 0.30), puruz=0.1)
beyaz = ortak.malzeme("Nazar_Beyaz", (0.85, 0.85, 0.82), puruz=0.1)
acik_mavi = ortak.malzeme("Nazar_Acik_Mavi", (0.20, 0.55, 0.85), puruz=0.1)
siyah = ortak.malzeme("Nazar_Siyah", (0.01, 0.01, 0.01), puruz=0.1)
kirmizi = ortak.malzeme("Nazar_Ip", (0.45, 0.03, 0.03), puruz=0.8)
demir = ortak.malzeme("Nazar_Civi", (0.25, 0.25, 0.25), puruz=0.5, metal=0.8)


def dik_disk(ad, yaricap, kalinlik, arka, z, mat, kose=40):
    """Arka yüzü y = arka düzleminde, ön yüzü -y'ye bakan disk (eklenirken döndürülür, dönüş meshe işlenir)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=yaricap, depth=kalinlik,
                                        location=(0, arka - kalinlik / 2, z), rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return n


R = 0.08                          # camın yarıçapı (16 cm çap)
p = [
    dik_disk("Lacivert", R, 0.012, 0.0, 0.0, lacivert),
    dik_disk("Beyaz", R * 0.62, 0.014, 0.0, 0.0, beyaz),
    dik_disk("AcikMavi", R * 0.42, 0.016, 0.0, 0.0, acik_mavi),
    dik_disk("Siyah", R * 0.20, 0.018, 0.0, 0.0, siyah, kose=24),
]
# Kırmızı ip: camın tepesinden yukarıdaki çiviye (ters V yerine düz, ince).
p.append(ortak.kutu("Ip", (0.003, 0.003, 0.09), (0, -0.004, R + 0.045), kirmizi))
p.append(ortak.kutu("CiviBas", (0.012, 0.006, 0.012), (0, -0.003, R + 0.093), demir, 0.002))
# Altta sallanan üç küçük boncuk: kırmızı ipe dizili.
p.append(ortak.kutu("AltIp", (0.002, 0.002, 0.06), (0, -0.004, -R - 0.03), kirmizi))
for i, z in enumerate((-R - 0.02, -R - 0.042, -R - 0.064)):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.007, location=(0, -0.006, z), segments=12, ring_count=8)
    b = bpy.context.active_object
    ortak.ata(b, lacivert if i != 1 else beyaz)
    p.append(b)

nazar = ortak.birlestir(p, "Nazar_Boncugu")
ortak.yumusat(nazar, 30)
ortak.pivotu_tasi(nazar, (0, 0, 0))
ortak.calistir_ve_bitir("Nazar_Boncugu", [nazar], 0.3, 0.8)
