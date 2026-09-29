# ilac_dolabi.py
# 2. katta Nuri Bey'in holündeki duvar ecza dolabı, kapağı açık (#83, Şahan'ın seçimi 2026-09-29).
# Beyaz emaye küçük dolap, önünde aynalı kapak menteşesinden 90° açık kalmış (açık kapak, gövdenin yan
# kenarında duvara dik levha: döndürme gerekmez). İçinde iki cam raf; birkaç ilaç şişesi devrik,
# rafların çoğu boşalmış (ilaçlar yere saçılmış, ayrı model).
#
# PARÇALAR
#   Ilac_Dolabi : tek parça. Pivot ARKA yüzün (duvar) ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
emaye = ortak.malzeme("Ecza_Emaye", (0.72, 0.72, 0.68), puruz=0.3)
ayna = ortak.malzeme("Ecza_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.4)
cam = ortak.malzeme("Ecza_Cam_Raf", (0.50, 0.56, 0.55), puruz=0.1)
kahve = ortak.malzeme("Ilac_Sise_Kahve", (0.20, 0.08, 0.02), puruz=0.2)
beyaz = ortak.malzeme("Ilac_Kutu", (0.80, 0.80, 0.76), puruz=0.6)
kirmizi = ortak.malzeme("Ilac_Serit", (0.50, 0.06, 0.05), puruz=0.6)
metal = ortak.malzeme("Ecza_Metal", (0.60, 0.60, 0.58), puruz=0.3, metal=0.9)

EN, DER, BOY = 0.40, 0.14, 0.55
yc = -DER / 2
p = []
kasa = ortak.kutu("Kasa", (EN, DER, BOY), (0, yc, 0), emaye, 0.006)
ortak.oy(kasa, ortak.kutu("Oyuk", (EN - 0.03, DER, BOY - 0.03), (0, yc - 0.015, 0), emaye))
p.append(kasa)
for z in (-0.09, 0.09):
    p.append(ortak.kutu("Raf", (EN - 0.035, DER - 0.03, 0.006), (0, yc + 0.005, z), cam))
# Kalan ilaçlar: alt rafta bir kutu ve devrilmiş bir şişe, üst rafta bir şişe.
p.append(ortak.kutu("Kutu", (0.08, 0.05, 0.10), (-0.10, yc + 0.01, -0.09 + 0.053), beyaz, 0.003))
p.append(ortak.kutu("KutuSerit", (0.082, 0.052, 0.015), (-0.10, yc + 0.01, -0.09 + 0.08), kirmizi))
bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.022, depth=0.08, location=(0.07, yc + 0.0, -0.09 + 0.025), rotation=(0, math.pi / 2, 0.3))
s = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(s, kahve)
p.append(s)
p.append(ortak.silindir("Sise", 0.02, 0.09, (0.10, yc + 0.01, 0.09 + 0.048), kahve, kose=16))
p.append(ortak.silindir("SiseKapak", 0.015, 0.02, (0.10, yc + 0.01, 0.09 + 0.103), beyaz, kose=16))
# Açık kapak: sağ kenardan menteşeli, 90° açık, duvara dik; dış yüzünde (odaya bakan) ayna.
kx = EN / 2 + 0.008
p.append(ortak.kutu("Kapak", (0.016, EN - 0.01, BOY - 0.01), (kx, -DER - (EN - 0.01) / 2, 0), emaye, 0.004))
p.append(ortak.kutu("KapakAyna", (0.003, EN - 0.07, BOY - 0.07), (kx + 0.009, -DER - (EN - 0.01) / 2, 0), ayna))
p.append(ortak.kutu("Mandal", (0.01, 0.02, 0.04), (kx - 0.012, -DER - EN + 0.03, 0), metal, 0.002))

dolap = ortak.birlestir(p, "Ilac_Dolabi")
ortak.yumusat(dolap, 30)
ortak.pivotu_tasi(dolap, (0, 0, 0))
ortak.calistir_ve_bitir("Ilac_Dolabi", [dolap], 0.6, 1.4, onizlemede_kaldir=0.4)
