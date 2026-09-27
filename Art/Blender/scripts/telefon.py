# telefon.py
# Kapıcı odasının masasındaki 90'lar Türk çevirmeli telefonu (#56): krem plastik gövde, önü eğik,
# ortada çevirme diski, üstte iki çatala oturan ahize.
#
# PARÇALAR
#   Telefon : gövde, disk, çatallar. Pivot tabanın ortası.
#   Ahize   : ahize (sap + iki uç). Pivot kendi ortası; açılınca Unity'de biraz kalkar.
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).
# DİKKAT: kutu ve silindirler döndürülmez (döndürme dünya merkezine göre savrulur, bkz. CLAUDE.md);
# eğik ön yüz basamaklı kutularla verilir.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
plastik = ortak.malzeme("Telefon_Plastik", (0.78, 0.72, 0.60), puruz=0.35)   # sararmış krem
koyu = ortak.malzeme("Telefon_Koyu", (0.05, 0.05, 0.05), puruz=0.4)
disk = ortak.malzeme("Telefon_Disk", (0.85, 0.83, 0.78), puruz=0.3)
metal = ortak.malzeme("Telefon_Metal", (0.6, 0.6, 0.58), puruz=0.3, metal=0.9)

parcalar = []
# Gövde: arkası yüksek, önü alçak üç basamak (eğik görünüm) ve üstte çatal yuvası.
parcalar.append(ortak.kutu("Taban", (0.22, 0.22, 0.05), (0, -0.11, 0.025), plastik, 0.012))
parcalar.append(ortak.kutu("Orta", (0.21, 0.11, 0.035), (0, -0.065, 0.067), plastik, 0.012))
parcalar.append(ortak.kutu("Arka", (0.20, 0.08, 0.03), (0, -0.05, 0.098), plastik, 0.012))
# Çatallar: ahizenin oturduğu iki tümsek.
for x in (-0.075, 0.075):
    parcalar.append(ortak.kutu("Catal", (0.03, 0.04, 0.025), (x, -0.05, 0.123), plastik, 0.008))
    parcalar.append(ortak.kutu("Dugme", (0.012, 0.012, 0.01), (x, -0.05, 0.138), metal, 0.002))
# Çevirme diski: önde, gövdenin üstünde yassı bir silindir ve delikler için koyu halka.
parcalar.append(ortak.silindir("Disk", 0.045, 0.008, (0, -0.168, 0.054), disk, kose=40))
parcalar.append(ortak.silindir("Halka", 0.034, 0.009, (0, -0.168, 0.055), koyu, kose=40))
parcalar.append(ortak.silindir("Orta", 0.015, 0.01, (0, -0.168, 0.056), disk, kose=24))
parcalar.append(ortak.kutu("Kablo", (0.012, 0.03, 0.012), (0.09, 0.0, 0.02), koyu))
telefon = ortak.birlestir(parcalar, "Telefon")
ortak.yumusat(telefon, 35)
ortak.pivotu_tasi(telefon, (0, 0, 0))

# Ahize: yatay bir sap ve iki ucunda yuvarlak kulaklık / ağızlık (x boyunca uzanır).
ahize = ortak.birlestir([
    ortak.kutu("Sap", (0.20, 0.035, 0.03), (0, -0.05, 0.15), plastik, 0.012),
    ortak.silindir("Kulak", 0.028, 0.03, (-0.095, -0.05, 0.14), plastik, kose=24),
    ortak.silindir("Agiz", 0.028, 0.03, (0.095, -0.05, 0.14), plastik, kose=24),
], "Ahize")
ortak.yumusat(ahize, 35)
ortak.pivotu_tasi(ahize, (0, -0.05, 0.15))

ortak.calistir_ve_bitir("Telefon", [telefon, ahize], 0.25, 0.7)
