# terlik.py
# Kapı önündeki ev terliği: plastik taban ve üstünde tek bant. Tek tek modelleniyor;
# sahnede sol ve sağ terlik ayrı nesneler olmalı çünkü "tek terlik" anomalisi
# birini gizliyor. Sağ terlik, Unity'de bu modelin aynalanmış kopyası olacak.
#
# Yön: burun +y tarafında. Pivot: tabanın alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

BOY, EN, TABAN = 0.26, 0.10, 0.022   # 26 cm boy, 10 cm en, 2,2 cm taban kalınlığı

ortak.sahneyi_temizle()
taban_mat = ortak.malzeme("Terlik_Taban", (0.10, 0.06, 0.04), puruz=0.7)   # koyu kahve kauçuk
bant_mat = ortak.malzeme("Terlik_Bant", (0.35, 0.05, 0.06), puruz=0.55)    # bordo bant

# TABAN: büyük bir pah, kutunun köşelerini yuvarlatıp tabanı terlik şekline sokar.
taban = ortak.kutu("Taban", (EN, BOY, TABAN), (0, 0, TABAN / 2), taban_mat)
mod = taban.modifiers.new("Yuvarlat", "BEVEL")
mod.width = 0.035          # köşeler 3,5 cm yuvarlanır
mod.segments = 6           # yuvarlak görünsün diye çok bölüm
mod.affect = "EDGES"
mod.limit_method = "ANGLE"

# BANT: yatay bir silindirden yay şeklinde bir kemer. Önce silindir, sonra
# içini ve alt yarısını oyarak ayağın gireceği kavisli bandı bırakıyoruz.
bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.046, depth=EN * 1.02,
                                    location=(0, BOY * 0.18, TABAN), rotation=(0, 1.5708, 0))
bant = bpy.context.active_object       # rotation: y ekseninde 90° → silindir x boyunca yatar
ortak.ata(bant, bant_mat)
# DİKKAT: nesneyi döndürünce KENDİ eksenleri de döner. Dönüşü meshe işlemeden
# ölçek verirsek "z'de basıklaştır" dediğimiz şey aslında terliğin enine uygulanır
# (ilk iki denemede bant bu yüzden tünel gibi yüksek kaldı). Önce dönüşü işle,
# eksenler yeniden dünya eksenleriyle hizalansın; ölçeği sonra ver.
bpy.ops.object.transform_apply(rotation=True)
bant.scale = (1, 1.25, 0.9)            # ~4 cm yüksek, burundan 2 cm geride biten bant
bpy.ops.object.transform_apply(scale=True)
ortak.oy(bant, ortak.kutu("k1", (0.3, 0.3, 0.1), (0, BOY * 0.18, TABAN - 0.05), bant_mat))   # alt yarıyı at
bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.040, depth=EN * 1.2,
                                    location=(0, BOY * 0.18, TABAN), rotation=(0, 1.5708, 0))
ic = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ic.scale = (1, 1.3, 0.85)
bpy.ops.object.transform_apply(scale=True)
ortak.oy(bant, ic)                     # içini boşalt: geriye ince bir kemer kalır

terlik = ortak.tek_parca([taban, bant], "Terlik", 40)
ortak.calistir_ve_bitir("Terlik", [terlik], 0.08, 0.45)
