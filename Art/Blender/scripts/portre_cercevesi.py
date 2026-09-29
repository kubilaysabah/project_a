# portre_cercevesi.py
# 2. katta Nuri Bey'in salonundaki rahmetli eşinin portresi (#83, Şahan'ın seçimi 2026-09-29).
# Koyu ahşap, içi oymalı kalın çerçeve, altın iç fileto; sol üst köşesinde çapraz siyah yas kurdelesi.
# Fotoğrafın kendisi ayrı doku (Assets/Textures/Portreler/kat2_es.png, yerel SDXL ile üretildi,
# Art/Fotograflar); Unity'deki şablon onu "Resim" parçasına takar.
#
# PARÇALAR
#   Cerceve : çerçeve, fileto, arkalık ve kurdele. Pivot ARKA yüzün (duvar) ortası.
#   Resim   : ön yüzü -y'ye bakan düzlem, UV'si 0-1 (fotoğraf 896 x 1152, oran 0,78).
# Ön yüz -y; Unity'de +Z. Duvar eşyası: merkez yüksekliğiyle duvarın iç yüzüne konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Portre_Cerceve", (0.04, 0.018, 0.007), puruz=0.35)
altin = ortak.malzeme("Portre_Fileto", (0.55, 0.40, 0.14), puruz=0.3, metal=0.8)
siyah = ortak.malzeme("Yas_Kurdelesi", (0.01, 0.01, 0.01), puruz=0.6)
foto = ortak.malzeme("Portre_Resim", (0.8, 0.8, 0.8), puruz=0.6)      # Unity'de dokulu malzemeyle değişir

EN, BOY = 0.42, 0.54                 # fotoğrafın görünen ölçüsü (oran 0,78)
K = 0.08                             # çerçeve genişliği
DER = 0.035
p = []
for dz in (-1, 1):
    p.append(ortak.kutu("Yatay", (EN + 2 * K, DER, K), (0, -DER / 2, dz * (BOY / 2 + K / 2)), ahsap, 0.01))
    p.append(ortak.kutu("FiletoY", (EN + 0.012, 0.006, 0.01), (0, -DER - 0.002, dz * (BOY / 2 + 0.005)), altin, 0.002))
for dx in (-1, 1):
    p.append(ortak.kutu("Dikey", (K, DER, BOY), (dx * (EN / 2 + K / 2), -DER / 2, 0), ahsap, 0.01))
    p.append(ortak.kutu("FiletoD", (0.01, 0.006, BOY), (dx * (EN / 2 + 0.005), -DER - 0.002, 0), altin, 0.002))
p.append(ortak.kutu("Arkalik", (EN + 0.02, 0.004, BOY + 0.02), (0, -0.002, 0), ahsap))
# Yas kurdelesi: sol üst köşede çapraz iki şerit (küp eklenirken y ekseninde döndürülür).
kx, kz = -(EN / 2 + K / 2), BOY / 2 + K / 2
for aci in (45, -45):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(kx + 0.05, -DER - 0.006, kz - 0.05), rotation=(0, math.radians(aci), 0))
    k = bpy.context.active_object
    k.scale = (0.035, 0.004, 0.24)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(k, siyah)
    p.append(k)
cerceve = ortak.birlestir(p, "Cerceve")
ortak.yumusat(cerceve, 30)
ortak.pivotu_tasi(cerceve, (0, 0, 0))

bpy.ops.mesh.primitive_plane_add(size=1, location=(0, -0.006, 0), rotation=(math.pi / 2, 0, 0))
resim = bpy.context.active_object
resim.name = "Resim"
resim.scale = (EN, BOY, 1)
bpy.ops.object.transform_apply(rotation=True, scale=True)
ortak.ata(resim, foto)
ortak.pivotu_tasi(resim, (0, 0, 0))

ortak.calistir_ve_bitir("Portre_Cercevesi", [cerceve, resim], 0.4, 1.2, onizlemede_kaldir=0.5)
