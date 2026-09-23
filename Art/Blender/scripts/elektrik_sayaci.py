# elektrik_sayaci.py
# Duvardaki elektrik sayacı kutusu ve menteşeli kapağı.
# Kapak AYRI bir nesne ve pivotu menteşe çizgisinde: "sayaç kapağı açık"
# anomalisi (T17) kapağı Unity'de döndürerek açacak; pivot ortada olsaydı
# kapak kendi ortasından döner ve yarısı duvara girerdi (daire kapılarındaki
# "Mentese" dersinin aynısı).
#
# DUVAR EŞYASI: ön yüz -y tarafında. Kutunun pivotu arka yüzün alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, DERIN, BOY = 0.24, 0.10, 0.32
KAPAK_KALIN = 0.012

ortak.sahneyi_temizle()
sac = ortak.malzeme("Sayac_Sac", (0.42, 0.44, 0.42), puruz=0.55, metal=0.3)   # gri boyalı sac
ic = ortak.malzeme("Sayac_Ic", (0.05, 0.05, 0.05), puruz=0.8)
cam = ortak.malzeme("Sayac_Cam", (0.10, 0.12, 0.12), puruz=0.1)
beyaz = ortak.malzeme("Sayac_Kadran", (0.80, 0.80, 0.76), puruz=0.6)

# ---------- KUTU ----------
# Arka yüz y = 0'da, ön ağız y = -DERIN'de. Ön yüzü açık bir kutu: içini oyuyoruz.
kutu = ortak.kutu("Kutu", (EN, DERIN, BOY), (0, -DERIN / 2, BOY / 2), sac, 0.003)
ortak.oy(kutu, ortak.kutu("k", (EN - 0.02, DERIN, BOY - 0.02), (0, -DERIN / 2 - 0.012, BOY / 2), sac))

# İçerisi: koyu arka pano, üstünde sayaç gövdesi ve kadran camı.
parcalar = [kutu,
            ortak.kutu("Pano", (EN - 0.02, 0.004, BOY - 0.02), (0, -0.012, BOY / 2), ic),
            ortak.kutu("SayacGovde", (0.13, 0.05, 0.18), (0, -0.035, BOY * 0.58), beyaz, 0.004),
            ortak.kutu("Kadran", (0.09, 0.004, 0.05), (0, -0.061, BOY * 0.66), cam)]
govde = ortak.birlestir(parcalar, "Elektrik_Sayaci")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))

# ---------- KAPAK ----------
# Menteşe sol kenarda (x = -EN/2). Pivot oraya konuyor; Unity'de y ekseni
# etrafında döndürünce kapak menteşesinden açılır.
kapak = ortak.kutu("Kapak", (EN, KAPAK_KALIN, BOY), (0, -DERIN - KAPAK_KALIN / 2, BOY / 2), sac, 0.003)
pencere = ortak.kutu("Pencere", (0.10, KAPAK_KALIN * 2, 0.07), (0, -DERIN - KAPAK_KALIN / 2, BOY * 0.66), sac)
ortak.oy(kapak, pencere)   # kadranın okunduğu küçük pencere
kulp = ortak.kutu("Kilit", (0.015, 0.012, 0.03), (EN / 2 - 0.025, -DERIN - KAPAK_KALIN - 0.006, BOY / 2), ic, 0.002)
kapak = ortak.birlestir([kapak, kulp], "Kapak")
ortak.yumusat(kapak, 30)
ortak.pivotu_tasi(kapak, (-EN / 2, -DERIN, 0))

ortak.calistir_ve_bitir("Elektrik_Sayaci", [govde, kapak], 0.3, 0.9, onizlemede_kaldir=0.05)
