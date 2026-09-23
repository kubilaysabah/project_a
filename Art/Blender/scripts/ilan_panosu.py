# ilan_panosu.py
# Yönetici ilan panosu: ahşap çerçeveli mantar pano ve üstünde raptiyeyle
# tutturulmuş bir ilan kâğıdı. Kâğıttaki yazı Unity'de ayrı bir yazı olacak
# ("ÇIKMAYIN" anomalisi T14 onu değiştirecek).
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot ARKA yüzün ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, BOY, KALIN = 0.50, 0.36, 0.015
CERCEVE = 0.025

ortak.sahneyi_temizle()
mantar = ortak.malzeme("Pano_Mantar", (0.45, 0.30, 0.16), puruz=1.0)
ahsap = ortak.malzeme("Pano_Cerceve", (0.22, 0.12, 0.06), puruz=0.6)
kagit = ortak.malzeme("Pano_Kagit", (0.82, 0.80, 0.74), puruz=0.9)
raptiye = ortak.malzeme("Pano_Raptiye", (0.7, 0.1, 0.08), puruz=0.4)

parcalar = [ortak.kutu("Mantar", (EN, KALIN, BOY), (0, -KALIN / 2, 0), mantar)]

# Çerçeve: dört çıta, mantardan biraz önde.
for (en, boy, x, z) in ((EN + CERCEVE * 2, CERCEVE, 0, BOY / 2 + CERCEVE / 2),
                        (EN + CERCEVE * 2, CERCEVE, 0, -BOY / 2 - CERCEVE / 2),
                        (CERCEVE, BOY, -EN / 2 - CERCEVE / 2, 0),
                        (CERCEVE, BOY, EN / 2 + CERCEVE / 2, 0)):
    parcalar.append(ortak.kutu("Cita", (en, KALIN + 0.008, boy), (x, -(KALIN + 0.008) / 2, z), ahsap, 0.003))

# İlan kâğıdı: A4 boyutunda (21 x 29,7 cm), hafif yamuk asılmış.
k = ortak.kutu("Kagit", (0.21, 0.001, 0.297), (0.03, -KALIN - 0.0005, -0.005), kagit)
k.rotation_euler = (0, 0.03, 0)
bpy.ops.object.transform_apply(rotation=True)
parcalar.append(k)

# Dört köşesinde raptiye.
for (x, z) in ((-0.07, 0.14), (0.13, 0.14), (-0.07, -0.15), (0.13, -0.15)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=10, ring_count=6, radius=0.006,
                                         location=(x, -KALIN - 0.003, z))
    r = bpy.context.active_object
    ortak.ata(r, raptiye)
    parcalar.append(r)

pano = ortak.birlestir(parcalar, "IlanPanosu")
ortak.yumusat(pano, 30)
ortak.pivotu_tasi(pano, (0, 0, 0))
ortak.calistir_ve_bitir("IlanPanosu", [pano], 0.1, 0.9, onizlemede_kaldir=0.22)
