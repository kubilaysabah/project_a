# oyuncak_kutusu.py
# Çocuk odasındaki boyalı tahta oyuncak sandığı (#53).
# Bulmaca eşyası: 3. katta Oda B'nin anahtarı salondaki oyuncak kutusunda (Docs/hikaye.md §7).
#
# PARÇALAR
#   Oyuncak_Kutusu : içi boş sandık, iki yanında tutamak oyuğu; pivot tabanın ortası.
#   Kapak          : menteşesi ARKA üst kenarda; Unity'de x ekseni etrafında döner.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.60, 0.38, 0.36
KAPAK = 0.03

ortak.sahneyi_temizle()
mavi = ortak.malzeme("Oyuncak_Mavi", (0.20, 0.38, 0.62), puruz=0.6)       # açık mavi yağlı boya
sari = ortak.malzeme("Oyuncak_Sari", (0.85, 0.65, 0.10), puruz=0.6)      # kenar şeritleri
ic = ortak.malzeme("Oyuncak_Ic", (0.45, 0.33, 0.20), puruz=0.8)          # boyasız tahta içi

govde = ortak.kutu("Govde", (EN, DERIN, BOY), (0, 0, BOY / 2), mavi, 0.005)
ortak.oy(govde, ortak.kutu("ic", (EN - 0.03, DERIN - 0.03, BOY), (0, 0, BOY / 2 + 0.015), ic))
# Yanlarda el oyukları.
for x in (-EN / 2, EN / 2):
    ortak.oy(govde, ortak.kutu("el", (0.04, 0.12, 0.035), (x, 0, BOY - 0.07), mavi))
# Ön yüzde sarı bir şerit (süs).
serit = ortak.kutu("Serit", (EN - 0.06, 0.004, 0.04), (0, -DERIN / 2 - 0.002, BOY / 2), sari)
govde = ortak.birlestir([govde, serit], "Oyuncak_Kutusu")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))

kapak = ortak.kutu("Kapak", (EN + 0.01, DERIN + 0.01, KAPAK), (0, 0, BOY + KAPAK / 2), sari, 0.005)
ortak.yumusat(kapak, 30)
ortak.pivotu_tasi(kapak, (0, DERIN / 2, BOY))   # menteşe: arka üst kenar

ortak.calistir_ve_bitir("Oyuncak_Kutusu", [govde, kapak], BOY, 1.2)
