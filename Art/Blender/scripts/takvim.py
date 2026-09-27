# takvim.py
# Salon duvarındaki yaprak duvar takvimi (#53).
# Bulmaca eşyası: 2. katta asma kilidin şifresi takvimde işaretli tarih (Docs/hikaye.md §7).
# Model yalnızca kâğıt ve karton: ay adı, günler ve işaret Unity'de YAZI olarak eklenir
# (kata göre değişir, iki dilde okunur); modelde yazı yok.
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot arka yüzün alt ortası.
# PARÇALAR
#   Takvim : arka karton, üstte reklam şeridi, önde yaprak destesi ve asma deliği.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, BOY = 0.32, 0.46          # arka kartonun ölçüsü
KARTON = 0.003
DESTE_BOY = 0.30              # yaprak destesinin yüksekliği (alt kısımda)

ortak.sahneyi_temizle()
karton = ortak.malzeme("Takvim_Karton", (0.55, 0.12, 0.10), puruz=0.8)   # bordo reklam kartonu
kagit = ortak.malzeme("Takvim_Kagit", (0.86, 0.84, 0.78), puruz=0.9)
civi = ortak.malzeme("Takvim_Civi", (0.3, 0.3, 0.3), puruz=0.4, metal=1.0)

arka = ortak.kutu("Arka", (EN, KARTON, BOY), (0, -KARTON / 2, BOY / 2), karton)
# Asma deliği: üst ortada küçük kare bir delik (duvardaki çiviye takılır). Kesici kutu
# kartonu önden arkaya delecek kadar derin (2 cm); silindir kullanmadık, çünkü hazır silindir
# dikey (z) duruyor ve önce çevrilmesi gerekirdi.
ortak.oy(arka, ortak.kutu("delik", (0.012, 0.02, 0.012), (0, -KARTON / 2, BOY - 0.02), karton))
# Yaprak destesi: kartonun alt kısmında, 12 yapraklık kalınlıkta.
deste = ortak.kutu("Deste", (EN - 0.03, 0.006, DESTE_BOY), (0, -KARTON - 0.003, 0.015 + DESTE_BOY / 2), kagit)
# Destenin üstünü tutan metal şerit.
serit = ortak.kutu("Serit", (EN - 0.02, 0.004, 0.012), (0, -KARTON - 0.007, 0.015 + DESTE_BOY), civi)
takvim = ortak.birlestir([arka, deste, serit], "Takvim")
ortak.pivotu_tasi(takvim, (0, 0, 0))

ortak.calistir_ve_bitir("Takvim", [takvim], BOY, 0.8, onizlemede_kaldir=0.02)
