# petek.py
# Dilimli döküm kalorifer peteği (#83, 1. katın holü; Şahan'ın seçimi 2026-09-29).
# 1974 yapımı binanın kaloriferi: krem boyalı, on dilimli döküm petek; duvardan 5 cm açıkta iki
# ayağa oturur, sağında vanası ve yere inen boru.
#
# PARÇALAR
#   Petek : tek parça. Pivot ARKA düzlemin (duvarın) alt ortası: y = 0 duvar, z = 0 döşeme.
# Ön yüz -y tarafında; Unity'de +Z. Döşemeye ve duvara yaslı konur (Y = 0, duvarın iç yüzü).
# (Döndürme yok: ortak.kutu tuzağı. Borular dik silindir.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
boya = ortak.malzeme("Petek_Boya", (0.62, 0.58, 0.48), puruz=0.55)       # sararmış krem yağlı boya
koyu = ortak.malzeme("Petek_Koyu", (0.30, 0.27, 0.22), puruz=0.6)
pirinc = ortak.malzeme("Petek_Vana", (0.45, 0.34, 0.16), puruz=0.4, metal=0.6)

DILIM, DILIM_EN = 10, 0.065        # dilim sayısı ve bir dilimin eni
ALT, BOY = 0.14, 0.60              # döşemeden yükseklik ve peteğin boyu
ARALIK = 0.05                      # duvardan açıklık
DER = 0.16                         # peteğin derinliği
EN = DILIM * DILIM_EN
yc = -ARALIK - DER / 2             # peteğin derinlik ortası
p = []
for i in range(DILIM):
    x = -EN / 2 + DILIM_EN * (i + 0.5)
    # Her dilim: iki dikey kolon (önde ve arkada) ve aralarında ince bir perde.
    for dy in (-0.05, 0.05):
        p.append(ortak.kutu("Kolon", (DILIM_EN - 0.012, 0.045, BOY - 0.04), (x, yc + dy, ALT + BOY / 2), boya, 0.012))
    p.append(ortak.kutu("Perde", (DILIM_EN - 0.03, 0.06, BOY - 0.10), (x, yc, ALT + BOY / 2), boya, 0.004))
    # Üstte ve altta dilimleri birbirine bağlayan yuvarlak başlıklar.
    for z in (ALT + 0.03, ALT + BOY - 0.03):
        p.append(ortak.kutu("Baslik", (DILIM_EN, DER - 0.02, 0.06), (x, yc, z), boya, 0.02))
# Ayaklar: iki uçta döşemeye inen kısa dikmeler.
for x in (-EN / 2 + 0.04, EN / 2 - 0.04):
    p.append(ortak.kutu("Ayak", (0.04, DER - 0.04, ALT), (x, yc, ALT / 2), boya, 0.006))
# Duvar kelepçesi: ortada, arkadan duvara.
p.append(ortak.kutu("Kelepce", (0.03, ARALIK, 0.03), (0, -ARALIK / 2, ALT + BOY - 0.08), koyu, 0.004))
# Vana ve boru: sağ üst uçtan yana çıkan dirsek, üstünde vana; boru döşemeye iner.
vx = EN / 2 + 0.05
p.append(ortak.kutu("Dirsek", (0.06, 0.03, 0.03), (EN / 2 + 0.025, yc, ALT + BOY - 0.05), koyu, 0.006))
p.append(ortak.silindir("Boru", 0.012, ALT + BOY - 0.05, (vx, yc, (ALT + BOY - 0.05) / 2), koyu, kose=12))
p.append(ortak.silindir("VanaGovde", 0.02, 0.05, (vx, yc, ALT + BOY - 0.02), pirinc, kose=16))
p.append(ortak.silindir("VanaTopuz", 0.03, 0.015, (vx, yc, ALT + BOY + 0.015), koyu, kose=16))

petek = ortak.birlestir(p, "Petek")
ortak.yumusat(petek, 30)
ortak.pivotu_tasi(petek, (0, 0, 0))
ortak.calistir_ve_bitir("Petek", [petek], 0.8, 1.8)
