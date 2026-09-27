# kaydirici.py
# Mobilya kaydırıcısı (#65, 5. kat): ağır bir mobilyanın ayaklarının altına konan dört keçe
# pabuç; gardırop ancak bunlarla parkede kayar. Alettir: kapıcının çantasında kalır.
#
# PARÇALAR
#   Kaydirici : karton bir kartın üstünde 2x2 dizili dört yuvarlak keçe pabuç. Pivot kartın
#               alt ortası (düz bir yüzeye konur).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
karton = ortak.malzeme("Kaydirici_Karton", (0.62, 0.50, 0.32), puruz=0.9)
kece = ortak.malzeme("Kaydirici_Kece", (0.30, 0.30, 0.32), puruz=1.0)
plastik = ortak.malzeme("Kaydirici_Plastik", (0.10, 0.10, 0.11), puruz=0.5)

parcalar = [ortak.kutu("Kart", (0.13, 0.13, 0.003), (0, 0, 0.0015), karton)]
for x in (-0.032, 0.032):
    for y in (-0.032, 0.032):
        parcalar.append(ortak.silindir("Kece", 0.026, 0.008, (x, y, 0.003 + 0.004), kece, kose=20))
        parcalar.append(ortak.silindir("Kapak", 0.022, 0.004, (x, y, 0.003 + 0.010), plastik, kose=20))
k = ortak.birlestir(parcalar, "Kaydirici")
ortak.yumusat(k, 35)
ortak.pivotu_tasi(k, (0, 0, 0))
ortak.calistir_ve_bitir("Kaydirici", [k], 0.1, 0.5)
