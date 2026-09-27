# kavanoz.py
# Mutfak rafındaki cam kavanoz ve teneke kapağı (#53).
# Bulmaca eşyası: 1. katta Oda A'nın anahtarı mutfaktaki kavanozda ("anahtarlar kavanozda" notu).
#
# PARÇALAR
#   Kavanoz : içi boş cam gövde (anahtar içine konabilsin), pivot tabanın ortası.
#   Kapak   : vidalı teneke kapak; pivot da tabanın ortası, Unity'de yukarı kaldırılıp açılır.
# Cam opak ama parlak ve koyu yeşil: gece, kafa lambasıyla camın parıltısı okunur.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

R, BOY = 0.055, 0.13          # gövde yarıçapı ve yüksekliği
AGIZ_R = 0.045                # boyun yarıçapı
BOYUN = 0.015                 # boyun yüksekliği

ortak.sahneyi_temizle()
cam = ortak.malzeme("Kavanoz_Cam", (0.20, 0.28, 0.22), puruz=0.05)
teneke = ortak.malzeme("Kavanoz_Kapak", (0.62, 0.52, 0.18), puruz=0.35, metal=0.9)   # sarı teneke

# Gövde: silindir, içi 4 mm et kalınlığıyla oyulmuş; üstünde daha dar bir boyun.
govde = ortak.silindir("Govde", R, BOY, (0, 0, BOY / 2), cam, kose=40, pah_genislik=0.006)
ortak.oy(govde, ortak.silindir("ic", R - 0.004, BOY, (0, 0, BOY / 2 + 0.004), cam, kose=40))
boyun = ortak.silindir("Boyun", AGIZ_R, BOYUN, (0, 0, BOY + BOYUN / 2), cam, kose=40)
ortak.oy(boyun, ortak.silindir("ic2", AGIZ_R - 0.004, BOYUN * 2, (0, 0, BOY + BOYUN / 2), cam, kose=40))
kavanoz = ortak.birlestir([govde, boyun], "Kavanoz")
ortak.yumusat(kavanoz, 40)
ortak.pivotu_tasi(kavanoz, (0, 0, 0))

# Kapak: boynun üstüne oturan, kenarı tırtıllı görünen kısa bir silindir.
kapak = ortak.silindir("Kapak", AGIZ_R + 0.004, 0.018, (0, 0, BOY + BOYUN + 0.005), teneke, kose=40, pah_genislik=0.002)
ortak.yumusat(kapak, 40)
ortak.pivotu_tasi(kapak, (0, 0, 0))

ortak.calistir_ve_bitir("Kavanoz", [kavanoz, kapak], BOY, 0.45)
