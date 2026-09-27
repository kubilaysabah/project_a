# duvar_kolu.py
# Duvara vidalı eski bir demir kol (manivela): "kol" kilidi (#54, Docs/hikaye.md §6).
# Oyuncu kolu aşağı çeker, başka bir yerdeki kapının kilidi "tak" diye açılır. Kilerde,
# banyoda ya da bir dolabın içinde durabilir; hangi kapıya bağlı olduğu Unity'de veridedir.
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot arka yüzün alt ortası.
# PARÇALAR
#   Duvar_Kolu : duvar plakası, ortasında kolun hareket ettiği yarık ve mil kovanı.
#   Kol        : kolun kendisi ve ucundaki topuz; pivotu MİLDE. Modelde kol YUKARI dönük
#                (çekilmemiş); Unity'de x ekseni etrafında ~120° döndürülünce aşağı iner.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, KALIN, BOY = 0.11, 0.015, 0.26
MIL_Z = 0.09                      # mil plakanın alt kısmında; kol yukarı uzanır
KOL_BOY = 0.22

ortak.sahneyi_temizle()
demir = ortak.malzeme("Kol_Demir", (0.12, 0.13, 0.12), puruz=0.55, metal=0.8)   # koyu boyalı demir
pas = ortak.malzeme("Kol_Pas", (0.30, 0.14, 0.06), puruz=0.8, metal=0.3)       # paslı yerler
topuz_mat = ortak.malzeme("Kol_Topuz", (0.04, 0.04, 0.04), puruz=0.4)         # siyah bakalit topuz

plaka = ortak.kutu("Plaka", (EN, KALIN, BOY), (0, -KALIN / 2, BOY / 2), demir, 0.003)
# Kolun hareket yarığı: milden aşağı doğru (kol aşağı inince içine girer).
ortak.oy(plaka, ortak.kutu("yarik", (0.022, KALIN * 3, 0.11), (0, -KALIN / 2, MIL_Z - 0.04), demir))
parcalar = [plaka]
# Dört köşede vida başı ve milin kovanı.
for x in (-EN / 2 + 0.015, EN / 2 - 0.015):
    for z in (0.015, BOY - 0.015):
        parcalar.append(ortak.silindir(f"Vida{x}{z}", 0.006, 0.006, (x, -KALIN - 0.003, z), pas, kose=10))
kovan = ortak.kutu("Kovan", (0.05, 0.03, 0.04), (0, -KALIN - 0.015, MIL_Z), demir, 0.006)
parcalar.append(kovan)
taban = ortak.birlestir(parcalar, "Duvar_Kolu")
ortak.yumusat(taban, 30)
ortak.pivotu_tasi(taban, (0, 0, 0))

# Kol: milden yukarı uzanan kare çubuk, ucunda topuz.
mil = (0, -KALIN - 0.035, MIL_Z)
cubuk = ortak.kutu("Cubuk", (0.018, 0.018, KOL_BOY), (0, mil[1], MIL_Z + KOL_BOY / 2), demir, 0.003)
topuz = ortak.silindir("Topuz", 0.02, 0.05, (0, mil[1], MIL_Z + KOL_BOY + 0.02), topuz_mat, kose=20, pah_genislik=0.006)
kol = ortak.birlestir([cubuk, topuz], "Kol")
ortak.yumusat(kol, 30)
ortak.pivotu_tasi(kol, mil)

ortak.calistir_ve_bitir("Duvar_Kolu", [taban, kol], BOY + 0.1, 0.8, onizlemede_kaldir=0.05)
