# paspas.py
# Kapı önü paspası: koyu kauçuk kenar içinde hindistan cevizi lifi (koko) yüzey.
# Ölçü graybox'la aynı: 62 x 50 cm. Pivot: alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, BOY = 0.62, 0.50

ortak.sahneyi_temizle()
kenar_mat = ortak.malzeme("Paspas_Kenar", (0.05, 0.05, 0.05), puruz=0.9)   # siyah kauçuk
lif_mat = ortak.malzeme("Paspas_Lif", (0.32, 0.20, 0.10), puruz=1.0)       # koko lifi: tamamen mat

kenar = ortak.kutu("Kenar", (EN, BOY, 0.010), (0, 0, 0.005), kenar_mat, 0.004)
# Lif yüzey kenardan biraz yüksek: gerçek paspaslarda lifler kenarın üstüne taşar.
lif = ortak.kutu("Lif", (EN - 0.05, BOY - 0.05, 0.016), (0, 0, 0.008), lif_mat, 0.003)

paspas = ortak.tek_parca([kenar, lif], "Paspas", 30)
ortak.calistir_ve_bitir("Paspas", [paspas], 0.05, 0.9)
