# hali_yari.py
# Salonda yarıya kadar rulo yapılıp bırakılmış yün halı (#83, 1. katın salonu; Şahan'ın seçimi 2026-09-29).
# 2,4 x 1,7 m'lik halının 1,4 m'si yerde düz duruyor, kalanı bir ucundan sarılmış. Sarılan kısmın
# altında parkede halının izi kalır (Unity'de ayrı bir iz şablonu). Açık uçta saçaklar var.
# Desen ayrı doku: Assets/Textures/Hali_Salon.png (Art/Dokular/scripts/hali_uret.py); Unity'deki şablon
# onu "Hali" parçasına takar.
#
# PARÇALAR
#   Hali : yerdeki düz kısım (UV'si dokunun sol 1,4/2,4'ü). Pivot HALININ TAMAMININ ortası (izle aynı).
#   Rulo : sarılmış kısım ve uçlarındaki halka. Pivot yine halının tamamının ortası.
# Halının uzun kenarı Blender x ekseni; Unity'de -x (ön yüz kuralı gereği aynalanır, bkz. ortak.py).

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
yun = ortak.malzeme("Hali_Yun", (0.8, 0.8, 0.8), puruz=0.95)          # Unity'de dokulu malzemeyle değişir
ters = ortak.malzeme("Hali_Ters", (0.36, 0.26, 0.18), puruz=0.95)     # halının tersi (sarılınca görünen)
kenar = ortak.malzeme("Hali_Kenar", (0.10, 0.03, 0.03), puruz=0.95)
sacak = ortak.malzeme("Hali_Sacak", (0.70, 0.64, 0.52), puruz=0.9)

UZUN, EN, KALIN = 2.40, 1.70, 0.008
DUZ = 1.40                          # yerde duran kısmın boyu
x0 = -UZUN / 2                      # düz kısmın başladığı uç (saçaklı açık uç)
x1 = x0 + DUZ                       # sarılmanın başladığı yer

# Düz kısım: düzlem (UV 0-1), sonra UV'nin x'i DUZ/UZUN ile küçültülür: dokunun yalnız o kadarı görünür.
bpy.ops.mesh.primitive_plane_add(size=1, location=((x0 + x1) / 2, 0, KALIN))
hali = bpy.context.active_object
hali.name = "Hali"
hali.scale = (DUZ, EN, 1)
bpy.ops.object.transform_apply(scale=True)
for veri in hali.data.uv_layers.active.data:
    veri.uv.x *= DUZ / UZUN
ortak.ata(hali, yun)
# Kenar kalınlığı: düz kısmın iki uzun kenarında ve açık ucunda ince koyu şerit (yandan halı kalınlığı).
kenarlar = [hali]
for yon in (-1, 1):
    kenarlar.append(ortak.kutu("KenarUzun", (DUZ, 0.006, KALIN), (hali.location.x, yon * (EN / 2 - 0.003), KALIN / 2), kenar))
kenarlar.append(ortak.kutu("KenarUc", (0.006, EN, KALIN), (x0 + 0.003, 0, KALIN / 2), kenar))
# Saçaklar: açık uçta kısa beyaz püsküller.
for i in range(34):
    yy = -EN / 2 + 0.04 + (EN - 0.08) * i / 33
    kenarlar.append(ortak.kutu("Sacak", (0.07, 0.012, 0.003), (x0 - 0.035, yy, 0.002), sacak))
hali = ortak.birlestir(kenarlar, "Hali")
ortak.pivotu_tasi(hali, (0, 0, 0))

# Rulo: sarılmış 1,0 m halı; ekseni y (halının eni) boyunca; üstü ters yüz, uçlarında sarım halkası.
R = 0.085
bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=R, depth=EN, location=(x1 + R - 0.01, 0, R), rotation=(math.pi / 2, 0, 0))
rulo = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(rulo, ters)
parca = [rulo]
for yon in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=R * 0.8, depth=0.004, location=(x1 + R - 0.01, yon * (EN / 2 + 0.002), R), rotation=(math.pi / 2, 0, 0))
    h = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(h, kenar)
    parca.append(h)
rulo = ortak.birlestir(parca, "Rulo")
ortak.yumusat(rulo, 40)
ortak.pivotu_tasi(rulo, (0, 0, 0))

ortak.calistir_ve_bitir("Hali_Yari", [hali, rulo], 0.2, 3.2)
