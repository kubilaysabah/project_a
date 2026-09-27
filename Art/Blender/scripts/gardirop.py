# gardirop.py
# İki kapılı ceviz gardırop (#65, 5. kat: mekanizma). Salonda duvara dayalı; oyuncu onu yana
# İTER ve arkasındaki ana su vanasını bulur. Kapıları açılmaz (süs); gardırobun tamamı kayar.
#
# PARÇALAR
#   Gardirop : gövde, iki kapı paneli, kulplar, üst korniş, ayaklar. Pivot tabanın ortası.
# Ön yüz -y tarafında; Unity'de +Z.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 1.00, 0.55, 1.90

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Gardirop_Ceviz", (0.24, 0.14, 0.07), puruz=0.45)
acik = ortak.malzeme("Gardirop_Kapak", (0.30, 0.18, 0.09), puruz=0.4)
pirinc = ortak.malzeme("Gardirop_Kulp", (0.55, 0.42, 0.20), puruz=0.3, metal=0.8)

parcalar = [ortak.kutu("Govde", (EN, DERIN, BOY - 0.08), (0, 0, 0.08 + (BOY - 0.08) / 2), ceviz, 0.005)]
parcalar.append(ortak.kutu("Korniş", (EN + 0.04, DERIN + 0.03, 0.05), (0, -0.01, BOY + 0.02), ceviz, 0.006))
for x in (-EN / 4, EN / 4):
    parcalar.append(ortak.kutu("Kapi", (EN / 2 - 0.03, 0.018, BOY - 0.20), (x, -DERIN / 2 - 0.009, 0.08 + (BOY - 0.08) / 2), acik, 0.004))
for x in (-0.04, 0.04):
    parcalar.append(ortak.kutu("Kulp", (0.014, 0.025, 0.14), (x, -DERIN / 2 - 0.03, 1.05), pirinc, 0.004))
for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
    for y in (-DERIN / 2 + 0.06, DERIN / 2 - 0.06):
        parcalar.append(ortak.kutu("Ayak", (0.05, 0.05, 0.08), (x, y, 0.04), ceviz))
g = ortak.birlestir(parcalar, "Gardirop")
ortak.yumusat(g, 30)
ortak.pivotu_tasi(g, (0, 0, 0))
ortak.calistir_ve_bitir("Gardirop", [g], BOY, 3.0)
