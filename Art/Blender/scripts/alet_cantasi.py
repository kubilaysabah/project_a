# alet_cantasi.py
# Cemal'in kırmızı teneke alet çantası (#53).
# Bulmaca eşyası: 5. katta tornavida salondaki alet çantasında; banyo kapısının menteşeleri
# onunla sökülür (Docs/hikaye.md §7). Aynı çanta kapıcı odasında da durabilir.
#
# PARÇALAR
#   Alet_Cantasi : içi boş gövde, pivot tabanın ortası.
#   Kapak        : üstteki kapak ve sapı; pivotu ARKA üst kenarda (menteşe çizgisi).
#                  Unity'de x ekseni etrafında döndürülünce arkasından açılır.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.46, 0.20, 0.17     # gövde (kapak hariç)
KAPAK_BOY = 0.05

ortak.sahneyi_temizle()
kirmizi = ortak.malzeme("Alet_Kirmizi", (0.45, 0.04, 0.03), puruz=0.45, metal=0.5)   # boyalı sac
siyah = ortak.malzeme("Alet_Siyah", (0.03, 0.03, 0.03), puruz=0.6)
celik = ortak.malzeme("Alet_Celik", (0.55, 0.55, 0.55), puruz=0.3, metal=1.0)

# ---------- GÖVDE ----------
govde = ortak.kutu("Govde", (EN, DERIN, BOY), (0, 0, BOY / 2), kirmizi, 0.004)
ortak.oy(govde, ortak.kutu("ic", (EN - 0.012, DERIN - 0.012, BOY), (0, 0, BOY / 2 + 0.006), kirmizi))
# Ön yüzde iki mandal (kilit dili).
mandallar = [ortak.kutu(f"Mandal{i}", (0.03, 0.008, 0.035), (x, -DERIN / 2 - 0.004, BOY - 0.01), celik, 0.002)
             for i, x in enumerate((-0.14, 0.14))]
govde = ortak.birlestir([govde] + mandallar, "Alet_Cantasi")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))

# ---------- KAPAK ----------
kapak = ortak.kutu("Kapak", (EN, DERIN, KAPAK_BOY), (0, 0, BOY + KAPAK_BOY / 2), kirmizi, 0.004)
ortak.oy(kapak, ortak.kutu("ic", (EN - 0.012, DERIN - 0.012, KAPAK_BOY), (0, 0, BOY + KAPAK_BOY / 2 - 0.006), kirmizi))
# Sap: iki ayak ve üstte siyah tutamak.
ayaklar = [ortak.kutu(f"Ayak{i}", (0.012, 0.02, 0.03), (x, 0, BOY + KAPAK_BOY + 0.015), celik) for i, x in enumerate((-0.07, 0.07))]
tutamak = ortak.kutu("Tutamak", (0.16, 0.025, 0.02), (0, 0, BOY + KAPAK_BOY + 0.035), siyah, 0.006)
kapak = ortak.birlestir([kapak, tutamak] + ayaklar, "Kapak")
ortak.yumusat(kapak, 30)
ortak.pivotu_tasi(kapak, (0, DERIN / 2, BOY))   # menteşe: arka üst kenar

ortak.calistir_ve_bitir("Alet_Cantasi", [govde, kapak], BOY + KAPAK_BOY, 0.9)
