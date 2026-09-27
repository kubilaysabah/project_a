# tabak.py
# Porselen yemek tabağı (#66, 6. kat). Dört tabak iki daireye dağılmış; her birinin üstünde
# bir isim yazılı (yazı Unity'de eklenir). Oyuncu tabakları tek tek taşıyıp sofraya koyar.
#
# PARÇALAR
#   Tabak : 24 cm çaplı, mavi kenar şeritli beyaz tabak. Pivot tabanın ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
porselen = ortak.malzeme("Tabak_Porselen", (0.90, 0.89, 0.85), puruz=0.15)
mavi = ortak.malzeme("Tabak_Mavi", (0.12, 0.22, 0.55), puruz=0.2)

govde = ortak.silindir("Govde", 0.12, 0.018, (0, 0, 0.009), porselen, kose=40)
# Çukur: ortası 6 mm oyulur (kenar yükseltisi kalır).
ortak.oy(govde, ortak.silindir("cukur", 0.085, 0.02, (0, 0, 0.018 + 0.004), porselen, kose=40))
serit = ortak.silindir("Serit", 0.118, 0.0015, (0, 0, 0.0185), mavi, kose=40)
ortak.oy(serit, ortak.silindir("s_ic", 0.106, 0.004, (0, 0, 0.0185), mavi, kose=40))
tabak = ortak.birlestir([govde, serit], "Tabak")
ortak.yumusat(tabak, 40)
ortak.pivotu_tasi(tabak, (0, 0, 0))
ortak.calistir_ve_bitir("Tabak", [tabak], 0.05, 0.5)
