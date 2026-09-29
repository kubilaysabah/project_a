# hali_duz.py
# Yere serili düz halı (#83): desen ayrı doku, Unity'deki şablon takar. Katlarda aynı desen yok; her
# kat kendi dokusunu ve ölçüsünü verir. İlk kullanım: 2. katta Nuri Bey'in salonu (Hali_Nuri, 3,0 x 2,1).
#
#   blender -b --factory-startup --python hali_duz.py -- <ad> <boy> <en>
#
# PARÇALAR
#   <ad> : yün (UV'si 0-1, dokunun tamamı) + kenar kalınlığı + iki kısa kenarda saçaklar. Pivot ortası,
#          döşemede. Uzun kenar Blender x boyunca. Ön yüz -y.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

arg = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
AD = arg[0] if len(arg) > 0 and not arg[0].startswith("--") else "Hali_Nuri"
BOY = float(arg[1]) if len(arg) > 1 and not arg[1].startswith("--") else 3.0
EN = float(arg[2]) if len(arg) > 2 and not arg[2].startswith("--") else 2.1
KAL = 0.01

ortak.sahneyi_temizle()
yun = ortak.malzeme(AD + "_Yun", (0.8, 0.8, 0.8), puruz=0.95)
kenar = ortak.malzeme(AD + "_Kenar", (0.10, 0.05, 0.04), puruz=0.95)
sacak = ortak.malzeme(AD + "_Sacak", (0.72, 0.66, 0.54), puruz=0.9)

bpy.ops.mesh.primitive_plane_add(size=1, location=(0, 0, KAL))
hali = bpy.context.active_object
hali.scale = (BOY, EN, 1)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(hali, yun)
p = [hali]
for yon in (-1, 1):
    p.append(ortak.kutu("KenarUzun", (BOY, 0.006, KAL), (0, yon * (EN / 2 - 0.003), KAL / 2), kenar))
    p.append(ortak.kutu("KenarKisa", (0.006, EN, KAL), (yon * (BOY / 2 - 0.003), 0, KAL / 2), kenar))
    n = int(EN / 0.05)
    for i in range(n):
        yy = -EN / 2 + 0.03 + (EN - 0.06) * i / (n - 1)
        p.append(ortak.kutu("Sacak", (0.08, 0.012, 0.003), (yon * (BOY / 2 + 0.04), yy, 0.002), sacak))
hali = ortak.birlestir(p, AD)
ortak.pivotu_tasi(hali, (0, 0, 0))
ortak.calistir_ve_bitir(AD, [hali], 0.1, max(BOY, EN) * 1.2)
