# golge.py
# Kovalayan siluet (#58, 7. katın kaçış anı): uzun boylu, ince, kapkara bir insan biçimi; iki
# küçük kırmızı göz (Kerem'in çizimindeki "dokuzuncu kattaki adam" gibi). Kat anahtarı alınınca
# anahtarın alındığı yerden çıkar ve oyuncunun yürüdüğü yolu izler (KacisAni).
#
# PARÇALAR
#   Golge : 2,05 m boyunda gövde, uzun kollar, küçük baş, gözler. Pivot ayakların arası (yer).
# Ön yüz -y tarafında; Unity'de +Z. (Kutular döndürülmez: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

ortak.sahneyi_temizle()
kara = ortak.malzeme("Golge_Kara", (0.035, 0.032, 0.034), puruz=0.9)   # 0,012 kafa lambasında bile görünmüyordu
goz = ortak.malzeme("Golge_Goz", (0.9, 0.04, 0.03), puruz=0.3)

p = []
# Bacaklar: ince ve uzun.
for x in (-0.09, 0.09):
    p.append(ortak.kutu("Bacak", (0.09, 0.10, 0.95), (x, 0, 0.475), kara, 0.02))
# Gövde: dar omuzlar, biraz öne eğik görünsün diye üst kısmı öne kayık iki kutu.
p.append(ortak.kutu("Kalca", (0.30, 0.16, 0.25), (0, 0, 1.05), kara, 0.03))
p.append(ortak.kutu("Govde", (0.34, 0.18, 0.55), (0, -0.03, 1.42), kara, 0.04))
# Kollar: dizlerin altına kadar sarkan, orantısız uzun kollar.
for x in (-0.22, 0.22):
    p.append(ortak.kutu("Kol", (0.07, 0.08, 0.95), (x, -0.02, 1.18), kara, 0.02))
# Boyun ve baş (küçük küre).
p.append(ortak.kutu("Boyun", (0.08, 0.08, 0.10), (0, -0.05, 1.73), kara))
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.12, location=(0, -0.07, 1.88), segments=16, ring_count=10)
bas = bpy.context.active_object
ortak.ata(bas, kara)
p.append(bas)
govde = ortak.birlestir(p, "Golge")
ortak.yumusat(govde, 30)
# Gözler: başın önünde iki küçük kırmızı nokta.
gozler = []
for x in (-0.04, 0.04):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.014, location=(x, -0.18, 1.90), segments=8, ring_count=6)
    g = bpy.context.active_object
    ortak.ata(g, goz)
    gozler.append(g)
golge = ortak.birlestir([govde] + gozler, "Golge")
ortak.pivotu_tasi(golge, (0, 0, 0))
ortak.calistir_ve_bitir("Golge", [golge], 2.0, 4.0)
