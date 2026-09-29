# fincan.py
# Boşaltılmış vitrinde unutulmuş tek Türk kahvesi fincanı ve tabağı (#83, 1. katın salonu).
# Beyaz porselen, ağzında ve tabağın kenarında yaldız çizgi; misafir takımından geriye kalan tek parça.
#
# PARÇALAR
#   Fincan : fincan, kulp ve tabak (tek parça). Pivot tabağın altının ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
porselen = ortak.malzeme("Fincan_Porselen", (0.90, 0.89, 0.86), puruz=0.15)
yaldiz = ortak.malzeme("Fincan_Yaldiz", (0.55, 0.40, 0.14), puruz=0.3, metal=0.8)

p = []
# Tabak: yassı disk, ortası hafif çukur görünümü için içte ikinci bir disk, kenarında yaldız halka.
p.append(ortak.silindir("Tabak", 0.058, 0.006, (0, 0, 0.003), porselen, kose=32))
p.append(ortak.silindir("TabakYaldiz", 0.059, 0.002, (0, 0, 0.006), yaldiz, kose=32))
p.append(ortak.silindir("TabakIc", 0.050, 0.003, (0, 0, 0.0075), porselen, kose=32))
# Fincan: dipten ağza hafif genişleyen üç halka, ağızda yaldız, içi koyu (boş fincan dibi).
for ad, r, h, z in (("Dip", 0.020, 0.008, 0.012), ("Gövde1", 0.024, 0.020, 0.026), ("Gövde2", 0.027, 0.020, 0.044)):
    p.append(ortak.silindir(ad, r, h, (0, 0, z), porselen, kose=24))
p.append(ortak.silindir("Agiz", 0.0275, 0.003, (0, 0, 0.0555), yaldiz, kose=24))
bos = ortak.silindir("Ic", 0.024, 0.030, (0, 0, 0.044), porselen, kose=24)
fincan_govde = ortak.birlestir(p, "Fincan")
ortak.oy(fincan_govde, bos)
# Kulp: sağda dik duran küçük halka (eklenirken x ekseni etrafında döndürülür, dönüş meshe işlenir).
bpy.ops.mesh.primitive_torus_add(major_radius=0.012, minor_radius=0.003, location=(0.033, 0, 0.038),
                                 rotation=(math.pi / 2, 0, 0), major_segments=16, minor_segments=6)
kulp = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(kulp, porselen)
fincan = ortak.birlestir([fincan_govde, kulp], "Fincan")
ortak.yumusat(fincan, 40)
ortak.pivotu_tasi(fincan, (0, 0, 0))
ortak.calistir_ve_bitir("Fincan", [fincan], 0.08, 0.35)
