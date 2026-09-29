# acik_bavul.py
# Şiltenin üstünde kapağı açık, yarım toplanmış bavul (#83, 1. katın Oda A'sı; 2026-09-29).
# 90'ların sert kenarlı kahverengi bavulu: gövdenin içinde katlanmış giysiler (kazak, gömlek, bir
# çocuk tişörtü), kapak arkaya açılmış ve bir iple tutulan cebi; bir kolu gövdeden sarkan hırka.
#
# PARÇALAR
#   Acik_Bavul : tek parça. Pivot gövdenin tabanının ortası. Ön yüz -y (kulp önde); Unity'de +Z.
# Açık kapak arkaya ~100° devrik: gövdenin arka üst kenarından dik yükselir ve biraz arkaya yaslanır.
# Kapak küp eklenirken döndürülür, dönüş meshe işlenir (ortak.kutu tuzağı).

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
deri = ortak.malzeme("Bavul_Deri", (0.18, 0.09, 0.04), puruz=0.6)
astar = ortak.malzeme("Bavul_Astar", (0.30, 0.10, 0.10), puruz=0.9)          # bordo astar
metal = ortak.malzeme("Bavul_Metal", (0.50, 0.48, 0.44), puruz=0.35, metal=0.9)
kazak = ortak.malzeme("Giysi_Kazak", (0.18, 0.26, 0.20), puruz=0.95)
gomlek = ortak.malzeme("Giysi_Gomlek", (0.70, 0.70, 0.64), puruz=0.9)
tisort = ortak.malzeme("Giysi_Tisort", (0.60, 0.40, 0.10), puruz=0.9)
hirka = ortak.malzeme("Giysi_Hirka", (0.40, 0.30, 0.34), puruz=0.95)

EN, DER, YUK = 0.62, 0.40, 0.17
p = []
govde = ortak.kutu("Govde", (EN, DER, YUK), (0, 0, YUK / 2), deri, 0.012)
ortak.oy(govde, ortak.kutu("Ic", (EN - 0.02, DER - 0.02, YUK), (0, 0, YUK / 2 + 0.01), deri))
p.append(govde)
p.append(ortak.kutu("Astar", (EN - 0.022, DER - 0.022, 0.004), (0, 0, 0.012), astar))
# Kulp ve iki kilit: ön yüzde.
p.append(ortak.kutu("Kulp", (0.14, 0.02, 0.025), (0, -DER / 2 - 0.012, YUK * 0.65), deri, 0.006))
for x in (-0.20, 0.20):
    p.append(ortak.kutu("Kilit", (0.04, 0.008, 0.03), (x, -DER / 2 - 0.004, YUK - 0.02), metal, 0.003))
# Katlanmış giysiler: gövdenin içinde üst üste yığınlar.
p.append(ortak.kutu("Kazak", (0.26, 0.30, 0.07), (-0.14, 0.02, 0.05), kazak, 0.02))
p.append(ortak.kutu("Gomlek", (0.24, 0.28, 0.04), (0.15, 0.0, 0.04), gomlek, 0.015))
p.append(ortak.kutu("Tisort", (0.18, 0.20, 0.03), (0.14, 0.03, 0.08), tisort, 0.012))
# Sarkan hırka: gövdenin sağ kenarından şilteye inen kol.
p.append(ortak.kutu("HirkaGovde", (0.22, 0.26, 0.05), (-0.10, 0.0, 0.11), hirka, 0.02))
bpy.ops.mesh.primitive_cube_add(size=1, location=(EN / 2 + 0.05, -0.02, 0.09), rotation=(0, math.radians(35), 0))
k = bpy.context.active_object
k.scale = (0.26, 0.09, 0.03)
bpy.ops.object.transform_apply(rotation=True, scale=True)
ortak.pah(k, 0.01)
ortak.ata(k, hirka)
p.append(k)
# Kapak: arka üst kenardan açık, 100° (dikten 10° arkaya yaslı); içinde bordo astar ve cep.
a = math.radians(10)
KAPAK_KAL = 0.06
yc = DER / 2 + (DER / 2) * math.sin(a) + 0.005
zc = YUK + (DER / 2) * math.cos(a)
for ad, olcu, mat, off in (("Kapak", (EN, KAPAK_KAL, DER), deri, 0.0), ("KapakAstar", (EN - 0.03, 0.004, DER - 0.03), astar, -KAPAK_KAL / 2 - 0.002),
                          ("Cep", (EN - 0.10, 0.012, DER * 0.4), astar, -KAPAK_KAL / 2 - 0.008)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, yc + off * math.cos(a), zc - off * math.sin(a) - (0.08 if ad == "Cep" else 0.0)), rotation=(a, 0, 0))
    n = bpy.context.active_object
    n.scale = olcu
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    p.append(n)

bavul = ortak.birlestir(p, "Acik_Bavul")
ortak.yumusat(bavul, 30)
ortak.pivotu_tasi(bavul, (0, 0, 0))
ortak.calistir_ve_bitir("Acik_Bavul", [bavul], 0.5, 1.6)
