# kirik_anahtar.py
# Ortasından kırılmış oda anahtarı (#65, 5. kat: parçaları birbirini tamamlar). Oyuncu iki
# parçayı evin farklı yerlerinde bulur; ikisi bir araya gelince anahtar tamamlanır ve salonun
# kapısı açılır. Ölçüler oda anahtarıyla aynı (kat_kapisi.py'deki Anahtar).
#
# PARÇALAR (tek FBX: Kirik_Anahtar.fbx)
#   Bas : halka ve gövdenin arka yarısı; kırık ucu pürüzlü. Pivot kendi ortası.
#   Uc  : gövdenin ön yarısı, dil ve diş; kırık ucu pürüzlü. Pivot kendi ortası.
# Yere yatık modellenir: uzunluk y ekseni, kalınlık z.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

ortak.sahneyi_temizle()
gumus = ortak.malzeme("KirikAnahtar_Gumus", (0.72, 0.73, 0.75), puruz=0.45, metal=1.0)

# ---------- BAŞ ----------
bpy.ops.mesh.primitive_torus_add(major_radius=0.016, minor_radius=0.004, location=(0, -0.03, 0.004), major_segments=24, minor_segments=8)
halka = bpy.context.active_object
ortak.ata(halka, gumus)
bas = [halka,
       ortak.kutu("GovdeArka", (0.006, 0.024, 0.006), (0, -0.003, 0.004), gumus, 0.001),
       # Kırık yüz: iki küçük, kaydırılmış kutu (pürüzlü görünsün).
       ortak.kutu("Kirik1", (0.004, 0.004, 0.006), (0.001, 0.011, 0.004), gumus)]
bas = ortak.birlestir(bas, "Bas")
ortak.yumusat(bas, 40)
ortak.pivotu_tasi(bas, (0, -0.015, 0.004))

# ---------- UÇ ----------
uc = [ortak.kutu("GovdeOn", (0.006, 0.026, 0.006), (0, 0.027, 0.004), gumus, 0.001),
      ortak.kutu("Kirik2", (0.004, 0.003, 0.006), (-0.001, 0.0125, 0.004), gumus),
      ortak.kutu("Dil", (0.014, 0.012, 0.006), (0.006, 0.033, 0.004), gumus, 0.001),
      ortak.kutu("Dis", (0.006, 0.006, 0.006), (0.012, 0.024, 0.004), gumus, 0.001)]
uc = ortak.birlestir(uc, "Uc")
ortak.yumusat(uc, 40)
ortak.pivotu_tasi(uc, (0, 0.027, 0.004))

ortak.calistir_ve_bitir("Kirik_Anahtar", [bas, uc], 0.05, 0.25)
