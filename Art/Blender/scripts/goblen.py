# goblen.py
# 1. katın holünde duvarda unutulmuş çerçeveli goblen (#83, Şahan'ın seçimi 2026-09-29).
# Yaldızlı ince iç çıtası olan koyu ahşap çerçeve, camsız; içinde kanaviçe. Kanaviçenin deseni
# ayrı bir doku (Assets/Textures/Goblen_Degirmen.png, Art/Dokular/scripts/goblen_uret.py); Unity'deki
# şablon onu kendi malzemesiyle "Kanavice" parçasına takar.
#
# PARÇALAR
#   Cerceve  : çerçeve. Pivot ARKA yüzün ortası (duvara değen nokta), merkez yüksekliğinde.
#   Kanavice : önü -y'ye bakan düz yüzey, UV'si 0-1 (dokunun tamamı). Pivot çerçeveyle aynı nokta.
# Ön yüz -y tarafında; Unity'de +Z. Duvar eşyası: merkez yüksekliğiyle duvarın iç yüzüne konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Goblen_Cerceve", (0.05, 0.025, 0.010), puruz=0.4)
yaldiz = ortak.malzeme("Goblen_Yaldiz", (0.50, 0.36, 0.14), puruz=0.35, metal=0.6)
kumas = ortak.malzeme("Goblen_Kanavice", (0.8, 0.8, 0.8), puruz=0.95)   # Unity'de dokulu malzemeyle değişir

EN, BOY = 0.48, 0.36               # kanaviçenin görünen ölçüsü (doku 4:3)
KALIN = 0.05                       # çerçeve çıtasının genişliği
DER = 0.025                        # çerçevenin duvardan çıkıntısı
p = []
# Dört çıta (dış kenar biraz kabarık), iç kenarında ince yaldız çıta.
for dz in (-1, 1):
    p.append(ortak.kutu("Yatay", (EN + 2 * KALIN, DER, KALIN), (0, -DER / 2, dz * (BOY / 2 + KALIN / 2)), ahsap, 0.006))
    p.append(ortak.kutu("YaldizYatay", (EN + 0.01, 0.006, 0.008), (0, -DER - 0.001, dz * (BOY / 2 + 0.004)), yaldiz, 0.002))
for dx in (-1, 1):
    p.append(ortak.kutu("Dikey", (KALIN, DER, BOY), (dx * (EN / 2 + KALIN / 2), -DER / 2, 0), ahsap, 0.006))
    p.append(ortak.kutu("YaldizDikey", (0.008, 0.006, BOY), (dx * (EN / 2 + 0.004), -DER - 0.001, 0), yaldiz, 0.002))
# Arkalık: kanaviçenin gerildiği karton (arkadan duvara değer).
p.append(ortak.kutu("Arkalik", (EN + 0.02, 0.004, BOY + 0.02), (0, -0.002, 0), ahsap))
cerceve = ortak.birlestir(p, "Cerceve")
ortak.yumusat(cerceve, 30)
ortak.pivotu_tasi(cerceve, (0, 0, 0))

# Kanaviçe: düzlem eklenirken döndürülür (x ekseni etrafında 90°: +z yüzü -y'ye bakar), dönüş meshe işlenir.
# primitive_plane_add'in UV'si 0-1: doku yüzeye tam oturur.
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, -0.006, 0), rotation=(math.pi / 2, 0, 0))
kanavice = bpy.context.active_object
kanavice.name = "Kanavice"
kanavice.scale = (EN, BOY, 1)
bpy.ops.object.transform_apply(rotation=True, scale=True)
ortak.ata(kanavice, kumas)
ortak.pivotu_tasi(kanavice, (0, 0, 0))

ortak.calistir_ve_bitir("Goblen", [cerceve, kanavice], 0.3, 0.9)
