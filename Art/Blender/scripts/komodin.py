# komodin.py
# Yatak başı komodini: üstte bir çekmece, altta açık bir raf (#53).
# Bulmaca eşyası: 1. katta kat anahtarı Oda A'daki komodinin çekmecesinde (Docs/hikaye.md §7).
#
# PARÇALAR
#   Komodin  : gövde (çekmece yuvası ve alt raf oyulmuş), pivot tabanın ortası.
#   Cekmece  : çekmecenin kendisi. Pivotu da tabanın ortasında (gövdeyle aynı nokta):
#              Unity'de yalnızca ileri (+Z) kaydırılarak açılır, dönmez.
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.45, 0.38, 0.55     # metre
CEK_BOY = 0.14                        # çekmece yüksekliği
UST_KALIN = 0.02                      # üst tablanın kalınlığı

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Komodin_Ceviz", (0.26, 0.15, 0.08), puruz=0.45)       # koyu ceviz kaplama
acik = ortak.malzeme("Komodin_On", (0.33, 0.20, 0.11), puruz=0.4)           # çekmece önü, biraz açık
pirinc = ortak.malzeme("Komodin_Kulp", (0.55, 0.42, 0.20), puruz=0.3, metal=0.8)

# ---------- GÖVDE ----------
govde_boy = BOY - UST_KALIN
govde = ortak.kutu("Govde", (EN, DERIN, govde_boy), (0, 0, govde_boy / 2), ceviz, 0.004)
# Çekmece yuvası: üstten 3 cm aşağıda, önden açık, arkada 2 cm'lik duvar kalır.
cek_z = govde_boy - 0.03 - CEK_BOY / 2          # çekmecenin orta yüksekliği
ortak.oy(govde, ortak.kutu("y1", (EN - 0.04, DERIN, CEK_BOY), (0, -0.02, cek_z), ceviz))
# Alt açık raf: 6 cm'lik süpürgeliğin üstünde, önden açık.
ortak.oy(govde, ortak.kutu("y2", (EN - 0.04, DERIN, 0.26), (0, -0.03, 0.06 + 0.13), ceviz))
ust = ortak.kutu("Ust", (EN + 0.02, DERIN + 0.02, UST_KALIN), (0, 0, BOY - UST_KALIN / 2), ceviz, 0.004)
govde = ortak.birlestir([govde, ust], "Komodin")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))

# ---------- ÇEKMECE ----------
# Ön panel gövdenin önünden 1,2 cm taşar: üst tabla 1 cm taşıyor ve Unity'de gövdenin
# çarpışma kutusu tablanın kenarına kadar uzanıyor. Çekmece önü o kutunun gerisinde kalsaydı
# oyuncunun nişan ışını önce gövdeye çarpar, çekmecenin "AÇ" istemi hiç çıkmazdı.
on_y = -DERIN / 2 - 0.012 + 0.009
on = ortak.kutu("On", (EN - 0.046, 0.018, CEK_BOY - 0.006), (0, on_y, cek_z), acik, 0.003)
tepsi_derin = DERIN - 0.05
tepsi_y = -DERIN / 2 + 0.018 + tepsi_derin / 2
tepsi = ortak.kutu("Tepsi", (EN - 0.06, tepsi_derin, CEK_BOY - 0.03), (0, tepsi_y, cek_z - 0.01), ceviz)
ortak.oy(tepsi, ortak.kutu("ic", (EN - 0.08, tepsi_derin - 0.02, CEK_BOY), (0, tepsi_y, cek_z + 0.005), ceviz))
kulp = ortak.kutu("Kulp", (0.09, 0.02, 0.014), (0, -DERIN / 2 - 0.01, cek_z), pirinc, 0.003)
cekmece = ortak.birlestir([on, tepsi, kulp], "Cekmece")
ortak.yumusat(cekmece, 30)
ortak.pivotu_tasi(cekmece, (0, 0, 0))

ortak.calistir_ve_bitir("Komodin", [govde, cekmece], BOY, 1.3)
