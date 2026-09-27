# mentese_plakasi.py
# Kapı menteşesinin plakası ve iki vidası (#54, "tornavida" kilidi).
# Tornavidayla sökülecek kapının koridor tarafına, menteşe kenarına iki tane takılır (üstte ve
# altta). Oyuncu her basışta bir vida söker; dört vida da çıkınca kapı devrilir.
#
# PARÇALAR
#   Mentese_Plakasi : pirinç renkli ince plaka ve ortasındaki menteşe borusu.
#   Vida_1, Vida_2  : yıldız başlı vidalar; pivotları kendi merkezlerinde, Unity'de tek tek
#                     öne doğru çıkarılıp düşürülür.
# Ön yüz -y (Unity'de +Z); pivot plakanın arka yüzünün ortası (kapının yüzüne yapışık).

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, KALIN, BOY = 0.032, 0.003, 0.10

ortak.sahneyi_temizle()
pirinc = ortak.malzeme("Mentese_Pirinc", (0.50, 0.40, 0.18), puruz=0.35, metal=1.0)
celik = ortak.malzeme("Mentese_Vida", (0.45, 0.45, 0.45), puruz=0.3, metal=1.0)


def ileri_yatir(n):
    """Dik silindiri ön-arka yöne (y) yatırır ve dönüşü meshe işler."""
    n.rotation_euler = (math.pi / 2, 0, 0)
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(rotation=True)
    return n


plaka = ortak.kutu("Plaka", (EN, KALIN, BOY), (0, -KALIN / 2, 0), pirinc, 0.0015)
boru = ortak.silindir("Boru", 0.006, BOY, (-EN / 2, -KALIN, 0), pirinc, kose=12)
plaka = ortak.birlestir([plaka, boru], "Mentese_Plakasi")
ortak.yumusat(plaka, 35)
ortak.pivotu_tasi(plaka, (0, 0, 0))
nesneler = [plaka]

for i, z in enumerate((BOY * 0.28, -BOY * 0.28)):
    bas = ileri_yatir(ortak.silindir("Bas", 0.0055, 0.003, (0.004, -KALIN - 0.0015, z), celik, kose=12))
    yiv1 = ortak.kutu("Y1", (0.008, 0.004, 0.0012), (0.004, -KALIN - 0.003, z), celik)
    yiv2 = ortak.kutu("Y2", (0.0012, 0.004, 0.008), (0.004, -KALIN - 0.003, z), celik)
    vida = ortak.birlestir([bas, yiv1, yiv2], f"Vida_{i + 1}")
    ortak.pivotu_tasi(vida, (0.004, -KALIN - 0.0015, z))
    nesneler.append(vida)

ortak.calistir_ve_bitir("Mentese_Plakasi", nesneler, 0.12, 0.3, onizlemede_kaldir=0.06)
