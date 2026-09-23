# ayakkabi.py
# Kimsenin olmadığı bir kapının önünde duran TEK erkek ayakkabısı (anomali T15).
# Klasik bağcıklı deri ayakkabı; ayrıntı az, siluet önemli.
#
# Yön: burun +y tarafında. Pivot: tabanın alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

BOY = 0.28

ortak.sahneyi_temizle()
deri = ortak.malzeme("Ayakkabi_Deri", (0.07, 0.04, 0.025), puruz=0.35)   # koyu kahve, hafif parlak deri
taban = ortak.malzeme("Ayakkabi_Taban", (0.03, 0.03, 0.03), puruz=0.8)

parcalar = []

# Taban: yuvarlatılmış ince kutu.
t = ortak.kutu("Taban", (0.10, BOY, 0.018), (0, 0, 0.009), taban)
m = t.modifiers.new("Yuvarlat", "BEVEL"); m.width = 0.04; m.segments = 6; m.limit_method = "ANGLE"
parcalar.append(t)

# Topuk: arkada biraz daha yüksek bir blok.
parcalar.append(ortak.kutu("Topuk", (0.075, 0.07, 0.022), (0, -BOY / 2 + 0.04, 0.02), taban, 0.006))

# Saya (üst deri): bir kürenin uzatılıp basıklaştırılmış hali; ayağın hacmini verir.
# Önce dönüş/ölçek meshe işlenir, sonra tabanın altında kalan kısmı oyulur.
bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, radius=1, location=(0, 0.005, 0.03))
saya = bpy.context.active_object
saya.scale = (0.048, 0.135, 0.058)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(saya, deri)
ortak.oy(saya, ortak.kutu("k", (0.3, 0.4, 0.1), (0, 0, -0.03), deri))   # tabanın altını at

# Ayağın girdiği ağız: arkada üstten bir oyuk.
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=1, location=(0, -0.04, 0.085))
agiz = bpy.context.active_object
agiz.scale = (0.036, 0.07, 0.035)
bpy.ops.object.transform_apply(scale=True)
ortak.oy(saya, agiz)
parcalar.append(saya)

ayakkabi = ortak.tek_parca(parcalar, "Ayakkabi", 45)
ortak.calistir_ve_bitir("Ayakkabi", [ayakkabi], 0.1, 0.5)
