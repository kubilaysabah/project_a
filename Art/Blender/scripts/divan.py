# divan.py
# Kapıcı odasının tahta divanı (#81, Şahan'ın seçimi: "tahta divan ve yün yorgan").
# Önceden yatak düz bir kahverengi kutu, üstünde kırmızı bir kutu (battaniye) idi. Artık alçak, boyası
# atmış tahta bir divan; üstünde ince pamuk döşek, yarısı açılmış çizgili yün yorgan ve bir yastık.
#
# PARÇALAR
#   Divan : tek parça (çerçeve, ayaklar, döşek, yorgan, yastık). Pivot tabanın ortası.
#           Uzun kenar x boyunca (1,95 m), en y boyunca (0,90 m).
# Ön yüz -y tarafında; Unity'de +Z. (Döndürme yok: ortak.kutu tuzağı.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
tahta = ortak.malzeme("Divan_Tahta", (0.30, 0.20, 0.11), puruz=0.7)        # boyası atmış çam
dosek = ortak.malzeme("Divan_Dosek", (0.72, 0.68, 0.58), puruz=0.95)       # sararmış pamuk döşek
yorgan_a = ortak.malzeme("Divan_Yorgan_A", (0.20, 0.26, 0.40), puruz=0.95) # lacivert çizgi
yorgan_b = ortak.malzeme("Divan_Yorgan_B", (0.70, 0.62, 0.48), puruz=0.95) # bej çizgi
yastik = ortak.malzeme("Divan_Yastik", (0.80, 0.76, 0.66), puruz=0.95)

UZUN, EN, YUK = 1.95, 0.90, 0.30            # çerçevenin üst kenarı 30 cm

p = []
# Çerçeve: dört yan tahta ve iç tahtalar (lata), köşelerde kare ayaklar.
p.append(ortak.kutu("YanOn", (UZUN, 0.03, 0.14), (0, -EN / 2 + 0.015, YUK - 0.07), tahta, 0.003))
p.append(ortak.kutu("YanArka", (UZUN, 0.03, 0.14), (0, EN / 2 - 0.015, YUK - 0.07), tahta, 0.003))
p.append(ortak.kutu("BasUc", (0.03, EN, 0.22), (-UZUN / 2 + 0.015, 0, YUK - 0.03), tahta, 0.003))   # baş tarafı biraz yüksek
p.append(ortak.kutu("AyakUc", (0.03, EN, 0.14), (UZUN / 2 - 0.015, 0, YUK - 0.07), tahta, 0.003))
for i in range(7):
    p.append(ortak.kutu("Lata", (0.08, EN - 0.06, 0.02), (-UZUN / 2 + 0.15 + i * 0.275, 0, YUK - 0.10), tahta))
for x in (-UZUN / 2 + 0.03, UZUN / 2 - 0.03):
    for y in (-EN / 2 + 0.03, EN / 2 - 0.03):
        p.append(ortak.kutu("Ayak", (0.06, 0.06, YUK - 0.07), (x, y, (YUK - 0.07) / 2), tahta, 0.003))

# Döşek: çerçevenin içine oturan, kenarları yuvarlatılmış kalın yastık gibi.
p.append(ortak.kutu("Dosek", (UZUN - 0.06, EN - 0.06, 0.12), (0, 0, YUK - 0.09 + 0.06), dosek, 0.03))
ust = YUK - 0.09 + 0.12

# Yorgan: ayak ucundan başlayıp döşeğin üçte ikisini örter, uç kısmı geri katlanmış; boyuna çizgili.
yorgan_bas = -UZUN / 2 + 0.60
yorgan_uzun = UZUN - 0.62
CIZGI = 12
for i in range(CIZGI):
    y = -EN / 2 + 0.01 + (i + 0.5) * (EN - 0.02) / CIZGI
    mat = yorgan_a if i % 2 == 0 else yorgan_b
    p.append(ortak.kutu("Yorgan", (yorgan_uzun, (EN - 0.02) / CIZGI, 0.05),
                        (yorgan_bas + yorgan_uzun / 2, y, ust + 0.025), mat, 0.004))
    # Katlanmış uç: başa bakan tarafta yorganın bir kısmı ters dönmüş, üstte ikinci kat.
    p.append(ortak.kutu("Kat", (0.28, (EN - 0.02) / CIZGI, 0.045),
                        (yorgan_bas + 0.14, y, ust + 0.072), mat, 0.004))
# Yorganın yanlardan sarkan kısmı (döşeğin kenarını örter).
for yon in (-1, 1):
    p.append(ortak.kutu("Sarkma", (yorgan_uzun, 0.02, 0.10), (yorgan_bas + yorgan_uzun / 2, yon * (EN / 2 - 0.01), ust - 0.03), yorgan_a, 0.004))

# Yastık: baş ucunda, yuvarlatılmış.
p.append(ortak.kutu("Yastik", (0.40, EN - 0.20, 0.12), (-UZUN / 2 + 0.28, 0, ust + 0.06), yastik, 0.05))

divan = ortak.birlestir(p, "Divan")
ortak.yumusat(divan, 30)
ortak.pivotu_tasi(divan, (0, 0, 0))
ortak.calistir_ve_bitir("Divan", [divan], 0.6, 2.6)
