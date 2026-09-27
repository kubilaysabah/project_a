# sofra.py
# Dört kişilik yemek masası ve sandalyeleri (#66, 6. kat: sıralama / yerleştirme).
# 6. katta "kimsenin oturmadığı" dairede sofra kurulmayı bekliyor: dört tabak, dört isim kartı
# (Rıza Efendi, Hasan, Cemal ve "Yeni Kapıcı"). Tabaklar doğru yerlere konunca büfe açılır.
# Tabak yuvaları (Yer_1..4) Unity'de kurucu tarafından masanın üstüne eklenir.
#
# PARÇALAR
#   Sofra : masa (140 x 90 cm, 75 cm yükseklik) ve uzun kenarlarda ikişer sandalye. Tek parça.
#           Pivot tabanın ortası. Masanın uzun ekseni x; sandalyeler y = +-0,62'de.
# Ön yüz -y tarafında; Unity'de +Z. (Döndürme yok: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Sofra_Ceviz", (0.30, 0.18, 0.09), puruz=0.4)
ortu = ortak.malzeme("Sofra_Ortu", (0.78, 0.74, 0.66), puruz=0.9)     # dantel örtü rengi
kumas = ortak.malzeme("Sofra_Kumas", (0.35, 0.10, 0.08), puruz=0.9)   # bordo sandalye minderi

EN, DERIN, BOY = 1.40, 0.90, 0.75
p = [ortak.kutu("Tabla", (EN, DERIN, 0.035), (0, 0, BOY - 0.0175), ceviz, 0.004),
     ortak.kutu("Ortu", (EN - 0.30, DERIN - 0.20, 0.004), (0, 0, BOY + 0.002), ortu)]
for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
    for y in (-DERIN / 2 + 0.06, DERIN / 2 - 0.06):
        p.append(ortak.kutu("Ayak", (0.05, 0.05, BOY - 0.035), (x, y, (BOY - 0.035) / 2), ceviz))
# Sandalyeler: ön kenarda (-y) iki, arka kenarda (+y) iki; sırtları masadan uzakta.
for x in (-0.35, 0.35):
    for yon in (-1, 1):
        y = yon * 0.62
        p.append(ortak.kutu("Oturak", (0.42, 0.42, 0.04), (x, y, 0.45), ceviz, 0.004))
        p.append(ortak.kutu("Minder", (0.38, 0.38, 0.03), (x, y, 0.485), kumas, 0.006))
        p.append(ortak.kutu("Sirt", (0.42, 0.03, 0.45), (x, y + yon * 0.195, 0.45 + 0.245), ceviz, 0.004))
        for dx in (-0.18, 0.18):
            for dy in (-0.18, 0.18):
                p.append(ortak.kutu("SAyak", (0.035, 0.035, 0.43), (x + dx, y + dy, 0.215), ceviz))
sofra = ortak.birlestir(p, "Sofra")
ortak.yumusat(sofra, 30)
ortak.pivotu_tasi(sofra, (0, 0, 0))
ortak.calistir_ve_bitir("Sofra", [sofra], 1.2, 3.2)
