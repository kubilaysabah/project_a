# bufe.py
# İki kapılı ceviz büfe (#66, 6. kat). Sofra doğru kurulunca sol kapısının kilidi açılır;
# içinde katın anahtarı. Sağ kapısı hep açılır (içi boş: şaşırtmaca).
#
# PARÇALAR
#   Bufe   : içi oyulmuş gövde, ortada bölme, üst tabla, ayaklar. Pivot tabanın ortası.
#   Kapi_1 : sol kapı; pivotu sol kenarda (menteşe). Unity'de y ekseninde + açıyla açılır.
#   Kapi_2 : sağ kapı; pivotu sağ kenarda. Unity'de - açıyla açılır.
# Ön yüz -y tarafında; Unity'de +Z. (Tezgâhın kapaklarıyla aynı düzen.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY, AYAK = 1.40, 0.45, 0.85, 0.08
ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Bufe_Ceviz", (0.26, 0.15, 0.07), puruz=0.4)
acik = ortak.malzeme("Bufe_Kapak", (0.33, 0.20, 0.10), puruz=0.35)
pirinc = ortak.malzeme("Bufe_Kulp", (0.58, 0.44, 0.20), puruz=0.3, metal=0.8)

govde = ortak.kutu("Govde", (EN, DERIN, BOY - AYAK), (0, 0, AYAK + (BOY - AYAK) / 2), ceviz, 0.004)
ortak.oy(govde, ortak.kutu("ic", (EN - 0.04, DERIN, BOY - AYAK - 0.04), (0, -0.02, AYAK + (BOY - AYAK) / 2), ceviz))
p = [govde,
     ortak.kutu("Bolme", (0.02, DERIN - 0.03, BOY - AYAK - 0.04), (0, 0.0, AYAK + (BOY - AYAK) / 2), ceviz),
     ortak.kutu("Tabla", (EN + 0.03, DERIN + 0.03, 0.03), (0, -0.01, BOY + 0.015), ceviz, 0.004)]
for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
    for y in (-DERIN / 2 + 0.06, DERIN / 2 - 0.06):
        p.append(ortak.kutu("Ayak", (0.05, 0.05, AYAK), (x, y, AYAK / 2), ceviz))
bufe = ortak.birlestir(p, "Bufe")
ortak.yumusat(bufe, 30)
ortak.pivotu_tasi(bufe, (0, 0, 0))

on_y = -DERIN / 2
kapi_boy = BOY - AYAK - 0.02
kapi_z = AYAK + 0.01 + kapi_boy / 2
nesneler = [bufe]
for i, (x0, x1, mentese) in enumerate(((-EN / 2 + 0.01, -0.005, -EN / 2 + 0.01), (0.005, EN / 2 - 0.01, EN / 2 - 0.01))):
    orta = (x0 + x1) / 2
    panel = ortak.kutu("Panel", (x1 - x0 - 0.006, 0.02, kapi_boy), (orta, on_y - 0.01, kapi_z), acik, 0.004)
    karsi = x1 - 0.05 if mentese == x0 else x0 + 0.05
    kulp = ortak.kutu("Kulp", (0.014, 0.022, 0.10), (karsi, on_y - 0.03, kapi_z + 0.05), pirinc, 0.004)
    k = ortak.birlestir([panel, kulp], f"Kapi_{i + 1}")
    ortak.yumusat(k, 30)
    ortak.pivotu_tasi(k, (mentese, on_y - 0.01, 0))
    nesneler.append(k)
print("IC_TABAN", AYAK + 0.02)
ortak.calistir_ve_bitir("Bufe", nesneler, BOY, 2.4)
