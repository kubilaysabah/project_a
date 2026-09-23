# isimlik.py
# Kapı yanına vidalı pirinç isimlik. Üstündeki soyadı yazısı Unity'de ayrı bir
# yazı olarak eklenecek (anomali kartları o yazıyı değiştirebilsin diye).
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot ARKA yüzün ortası. Unity'de duvar
# yüzeyinin konumunu vermek yeter, eşya tam duvara yapışır.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, BOY, KALIN = 0.11, 0.035, 0.004

ortak.sahneyi_temizle()
pirinc = ortak.malzeme("Isimlik_Pirinc", (0.72, 0.55, 0.24), puruz=0.3, metal=1.0)
vida = ortak.malzeme("Isimlik_Vida", (0.5, 0.5, 0.5), puruz=0.4, metal=1.0)

# Plaka: arka yüzü y = 0'da, ön yüzü y = -KALIN'de.
plaka = ortak.kutu("Plaka", (EN, KALIN, BOY), (0, -KALIN / 2, 0), pirinc, 0.001)
parcalar = [plaka]

# İki vida başı: plakanın iki ucunda, önden hafif taşan küçük silindirler.
for x in (-EN / 2 + 0.008, EN / 2 - 0.008):
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.0035, depth=0.002,
                                        location=(x, -KALIN - 0.001, 0), rotation=(1.5708, 0, 0))
    v = bpy.context.active_object      # x ekseninde 90° → silindir duvara dik bakar
    ortak.ata(v, vida)
    parcalar.append(v)

isimlik = ortak.birlestir(parcalar, "Isimlik")
ortak.yumusat(isimlik, 30)
ortak.pivotu_tasi(isimlik, (0, 0, 0))   # arka yüzün ortası
ortak.calistir_ve_bitir("Isimlik", [isimlik], 0.04, 0.25, onizlemede_kaldir=0.03)
