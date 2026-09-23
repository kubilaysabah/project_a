# saksi.py
# Kapı yanında toprak saksıda sardunya: Türk apartmanı sahanlıklarının klasiği.
# "Eksik saksı" anomalisi (F30) için ayrı bir nesne olarak sahneye konacak.
# Pivot: saksının alt ortası.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

random.seed(7)   # yaprakların yerleşimi rastgele ama her çalıştırmada AYNI olsun

ortak.sahneyi_temizle()
toprak_rengi = ortak.malzeme("Saksi_Pismis_Toprak", (0.50, 0.22, 0.11), puruz=0.85)
toprak = ortak.malzeme("Saksi_Toprak", (0.10, 0.06, 0.04), puruz=1.0)
yaprak = ortak.malzeme("Saksi_Yaprak", (0.08, 0.25, 0.06), puruz=0.6)
cicek = ortak.malzeme("Saksi_Cicek", (0.70, 0.05, 0.06), puruz=0.5)        # kırmızı sardunya

parcalar = []

# SAKSI: aşağıda dar, yukarıda geniş bir koni. Koni, alt ve üst yarıçapı farklı bir silindirdir.
bpy.ops.mesh.primitive_cone_add(vertices=40, radius1=0.075, radius2=0.10, depth=0.17, location=(0, 0, 0.085))
saksi = bpy.context.active_object
ortak.ata(saksi, toprak_rengi)
parcalar.append(saksi)

# Ağız kenarı: üstte kalın bir halka (simit).
bpy.ops.mesh.primitive_torus_add(major_radius=0.10, minor_radius=0.012, location=(0, 0, 0.17))
kenar = bpy.context.active_object
ortak.ata(kenar, toprak_rengi)
parcalar.append(kenar)

# Toprak yüzeyi: ağzın biraz altında.
parcalar.append(ortak.silindir("Toprak", 0.094, 0.01, (0, 0, 0.162), toprak, 40))

# YAPRAKLAR: basık küreler, saksının üstünde bir tutam oluşturacak şekilde çevreye dağılır.
for i in range(9):
    aci = i / 9 * 2 * math.pi + random.uniform(-0.3, 0.3)   # çevre boyunca eşit aralık + biraz sapma
    uzak = random.uniform(0.03, 0.09)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=0.045,
        location=(math.cos(aci) * uzak, math.sin(aci) * uzak, 0.19 + random.uniform(0, 0.07)))
    y = bpy.context.active_object
    y.scale = (1.0, 1.0, 0.35)          # yassı yaprak
    y.rotation_euler = (random.uniform(-0.4, 0.4), random.uniform(-0.4, 0.4), aci)
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    ortak.ata(y, yaprak)
    parcalar.append(y)

# ÇİÇEKLER: tepede küçük kırmızı top kümeleri.
for (x, yy, z) in ((0.02, 0.01, 0.29), (-0.04, 0.03, 0.27), (0.03, -0.05, 0.265)):
    for k in range(5):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=10, ring_count=6, radius=0.012,
            location=(x + random.uniform(-0.015, 0.015), yy + random.uniform(-0.015, 0.015), z + random.uniform(0, 0.015)))
        c = bpy.context.active_object
        ortak.ata(c, cicek)
        parcalar.append(c)

saksi_son = ortak.tek_parca(parcalar, "Saksi", 50)
ortak.calistir_ve_bitir("Saksi", [saksi_son], 0.3, 0.9)
