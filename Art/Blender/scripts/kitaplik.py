# kitaplik.py
# 2. katta Nuri Bey'in holündeki alçak kitaplık (#83, Şahan'ın seçimi 2026-09-29).
# Koyu ceviz, üç raflı, 1,10 m boyunda: alt rafta gazete kuponlarıyla toplanmış 20 ciltlik bordo
# ansiklopedi takımı (sırtlarında altın şeritler), orta rafta boy boy eski romanlar ve yatık bir
# yığın, üst rafta birkaç kitap ve boşluk (aile fotoğrafı üstüne, Unity'de ayrı konur).
#
# PARÇALAR
#   Kitaplik : tek parça. Pivot tabanın ortası, arka yüz (duvar) y = +0,15. Ön yüz -y; Unity'de +Z.

import os, sys, random
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
rnd = random.Random(1971)
ceviz = ortak.malzeme("Kitaplik_Ceviz", (0.06, 0.03, 0.012), puruz=0.4)
bordo = ortak.malzeme("Ansiklopedi_Bordo", (0.22, 0.03, 0.03), puruz=0.6)
altin = ortak.malzeme("Ansiklopedi_Altin", (0.55, 0.40, 0.14), puruz=0.35, metal=0.7)
kitap_renkleri = [ortak.malzeme(f"Kitap_{i}", c, puruz=0.7) for i, c in enumerate(
    ((0.10, 0.18, 0.12), (0.30, 0.22, 0.10), (0.08, 0.10, 0.25), (0.40, 0.36, 0.28), (0.25, 0.06, 0.05), (0.15, 0.13, 0.10)))]
sayfa = ortak.malzeme("Kitap_Sayfa", (0.72, 0.68, 0.58), puruz=0.9)

EN, DER, BOY = 0.90, 0.30, 1.10
ARKA = DER / 2
ET = 0.02
p = []
# Kasa: iki yan, alt, üst, arka.
for yon in (-1, 1):
    p.append(ortak.kutu("Yan", (ET, DER, BOY), (yon * (EN / 2 - ET / 2), 0, BOY / 2), ceviz, 0.003))
p.append(ortak.kutu("Ust", (EN + 0.02, DER + 0.02, 0.025), (0, -0.01, BOY - 0.0125), ceviz, 0.004))
p.append(ortak.kutu("Kaide", (EN, DER - 0.02, 0.06), (0, 0.01, 0.03), ceviz, 0.003))
p.append(ortak.kutu("Arka", (EN - 2 * ET, 0.008, BOY - 0.04), (0, ARKA - 0.004, BOY / 2), ceviz))
RAF_Z = (0.06, 0.40, 0.74)                       # rafların üst yüzleri
for z in RAF_Z[1:]:
    p.append(ortak.kutu("Raf", (EN - 2 * ET, DER - 0.02, 0.018), (0, 0.0, z - 0.009), ceviz))
ic_en = EN - 2 * ET

# Alt raf: 20 ciltlik ansiklopedi, eş boy, sırtlarında iki altın şerit.
x = -ic_en / 2 + 0.01
for i in range(20):
    k = 0.038
    h = 0.30
    p.append(ortak.kutu("Cilt", (k - 0.002, 0.22, h), (x + k / 2, 0.02, RAF_Z[0] + h / 2), bordo, 0.002))
    for dz in (0.24, 0.05):
        p.append(ortak.kutu("Serit", (k - 0.004, 0.002, 0.008), (x + k / 2, 0.02 - 0.111, RAF_Z[0] + dz), altin))
    x += k

# Orta raf: boy boy romanlar ve sağda yatık bir yığın.
x = -ic_en / 2 + 0.01
while x < 0.15:
    k = rnd.uniform(0.025, 0.045)
    h = rnd.uniform(0.18, 0.25)
    m = rnd.choice(kitap_renkleri)
    p.append(ortak.kutu("Roman", (k - 0.002, rnd.uniform(0.14, 0.18), h), (x + k / 2, 0.03, RAF_Z[1] + h / 2), m, 0.002))
    x += k
z = RAF_Z[1]
for i in range(4):
    h = rnd.uniform(0.03, 0.045)
    p.append(ortak.kutu("Yigin", (rnd.uniform(0.17, 0.22), rnd.uniform(0.13, 0.16), h), (0.27, 0.03, z + h / 2), rnd.choice(kitap_renkleri), 0.002))
    z += h
# Üst raf: soldan birkaç kitap, sağ taraf boş.
x = -ic_en / 2 + 0.01
for i in range(7):
    k = rnd.uniform(0.03, 0.05)
    h = rnd.uniform(0.20, 0.27)
    p.append(ortak.kutu("Kitap", (k - 0.002, 0.17, h), (x + k / 2, 0.03, RAF_Z[2] + h / 2), rnd.choice(kitap_renkleri), 0.002))
    x += k

kitaplik = ortak.birlestir(p, "Kitaplik")
ortak.yumusat(kitaplik, 30)
ortak.pivotu_tasi(kitaplik, (0, 0, 0))
ortak.calistir_ve_bitir("Kitaplik", [kitaplik], 1.1, 2.4)
