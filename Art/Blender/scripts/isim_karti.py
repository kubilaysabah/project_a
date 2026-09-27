# isim_karti.py
# Sofradaki isim kartı (#66, 6. kat): küçük dik bir karton, alt kenarında ince bir ayak.
# İsim Unity'de yazı olarak eklenir (metin tablosundan: "Yeni Kapıcı" İngilizcede "New Janitor").
#
# PARÇALAR
#   Isim_Karti : 10 x 6 cm krem kart ve 3 cm derin ayak. Pivot ayağın alt ortası.
# Ön yüz -y tarafında; Unity'de +Z.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
krem = ortak.malzeme("IsimKarti_Krem", (0.88, 0.84, 0.72), puruz=0.9)
p = [ortak.kutu("Kart", (0.10, 0.003, 0.06), (0, 0, 0.004 + 0.03), krem),
     ortak.kutu("Ayak", (0.10, 0.03, 0.004), (0, 0.012, 0.002), krem)]
k = ortak.birlestir(p, "Isim_Karti")
ortak.pivotu_tasi(k, (0, 0, 0))
ortak.calistir_ve_bitir("Isim_Karti", [k], 0.06, 0.4)
