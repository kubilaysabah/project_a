# asma_kilit.py
# Dört çarklı şifreli asma kilit (#54): şifreli kilit türünün kapıda ya da dolapta görünen yüzü.
# Oyuncu kilide bakınca yakın bakış ekranı açılır (SifreEkrani); çarklar orada çevrilir.
# Doğru şifrede kilit açılır ve kapıdan düşer (Unity'de kaldırılır).
#
# PARÇALAR
#   Asma_Kilit : pirinç gövde, çelik halka ve alt yüzdeki dört çark (tek parça).
# Ön yüz -y tarafında (Unity'de +Z); pivot gövdenin arka yüzünün ortası: kapıya yapışık durur.

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, DERIN, BOY = 0.055, 0.022, 0.050

ortak.sahneyi_temizle()
pirinc = ortak.malzeme("AsmaKilit_Pirinc", (0.60, 0.46, 0.18), puruz=0.3, metal=1.0)
celik = ortak.malzeme("AsmaKilit_Celik", (0.55, 0.55, 0.57), puruz=0.25, metal=1.0)
siyah = ortak.malzeme("AsmaKilit_Cark", (0.06, 0.06, 0.06), puruz=0.5)

govde = ortak.kutu("Govde", (EN, DERIN, BOY), (0, -DERIN / 2, 0), pirinc, 0.004)
# Halka: iki dik çubuk ve üstte yatay bir kemer (U harfi).
halka = [ortak.silindir(f"Bacak{i}", 0.004, 0.035, (x, -DERIN / 2, BOY / 2 + 0.0175), celik, kose=12)
         for i, x in enumerate((-0.017, 0.017))]
kemer = ortak.kutu("Kemer", (0.042, 0.008, 0.008), (0, -DERIN / 2, BOY / 2 + 0.035), celik, 0.003)
# Alt ön yüzde dört çark: gövdeden taşan küçük silindirler (yatay eksenli).
carklar = []
for i in range(4):
    c = ortak.silindir(f"Cark{i}", 0.0055, 0.009, (-0.018 + i * 0.012, -DERIN - 0.001, -BOY / 2 + 0.012), siyah, kose=10)
    c.rotation_euler = (0, math.pi / 2, 0)
    bpy.ops.object.select_all(action="DESELECT")
    c.select_set(True)
    bpy.context.view_layer.objects.active = c
    bpy.ops.object.transform_apply(rotation=True)
    carklar.append(c)
kilit = ortak.birlestir([govde, kemer] + halka + carklar, "Asma_Kilit")
ortak.yumusat(kilit, 35)
ortak.pivotu_tasi(kilit, (0, 0, 0))

ortak.calistir_ve_bitir("Asma_Kilit", [kilit], 0.1, 0.3, onizlemede_kaldir=0.05)
