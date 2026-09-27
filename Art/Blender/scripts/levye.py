# levye.py
# Levye (#65, 5. kat): ağır gardırobu yerden kaldırmak için. Alettir: kapıcının çantasında
# kalır, bitmez, sonraki katlarda da işe yarar (tornavida gibi).
#
# PARÇALAR
#   Levye : altıgen kesitli kırmızı çubuk (60 cm), bir ucu yassı ve kıvrık, öbür ucu çatal.
#           Pivot çubuğun ortası, tabanı (yere yatık konur). Uzun ekseni x.
# Yere yatırılmış hâlde modellenir: Unity'de olduğu gibi yere konur.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

UZUN, KALIN = 0.60, 0.022

ortak.sahneyi_temizle()
kirmizi = ortak.malzeme("Levye_Kirmizi", (0.55, 0.06, 0.04), puruz=0.45, metal=0.6)
celik = ortak.malzeme("Levye_Celik", (0.55, 0.55, 0.55), puruz=0.3, metal=1.0)

# Gövde: yatay çubuk (kutu ölçüleriyle; döndürme yok, bkz. ortak.kutu tuzağı).
parcalar = [ortak.kutu("Govde", (UZUN - 0.12, KALIN, KALIN), (0, 0, KALIN / 2), kirmizi, 0.004)]
# Kıvrık uç: iki kısa kutu, yukarı kalkan dirsek ve yassı ağız (çelik).
parcalar.append(ortak.kutu("Dirsek", (0.05, KALIN, 0.05), (UZUN / 2 - 0.085, 0, 0.035), kirmizi, 0.004))
parcalar.append(ortak.kutu("Agiz", (0.06, KALIN * 1.6, 0.008), (UZUN / 2 - 0.035, 0, 0.058), celik, 0.002))
# Öbür uç: yassı çatal.
parcalar.append(ortak.kutu("Catal", (0.06, KALIN * 1.8, 0.008), (-UZUN / 2 + 0.03, 0, 0.006), celik, 0.002))
levye = ortak.birlestir(parcalar, "Levye")
ortak.yumusat(levye, 35)
ortak.pivotu_tasi(levye, (0, 0, 0))
ortak.calistir_ve_bitir("Levye", [levye], 0.2, 1.0)
