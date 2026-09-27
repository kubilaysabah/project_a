# dosya_dolabi.py
# Yönetim dairesinin dört çekmeceli, sac dosya dolabı (#68, 8. kat).
# Bulmaca eşyası: en üst çekmece anahtarla kilitli; içinde kapıcıların dosyaları ve asansörün
# sigortası durur (Docs/hikaye.md §7). Öbür üç çekmece kilitsiz; bazıları boş (kat çıktıkça
# boş kaplar artar, Şahan'ın kuralı).
#
# PARÇALAR
#   Dosya_Dolabi : gövde (dört çekmece yuvası önden oyulmuş), pivot tabanın ortası.
#   Cekmece_1..4 : çekmeceler, 1 en üstte. Pivotları da tabanın ortasında (gövdeyle aynı nokta):
#                  Unity'de yalnızca ileri (+Z) kaydırılarak açılır, dönmez.
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.47, 0.60, 1.32     # metre: gerçek dört çekmeceli dosya dolabı ölçüsü
TABAN = 0.05                          # en alt çekmecenin altındaki süpürgelik
UST_KALIN = 0.02                      # üst sacın kalınlığı
CEKMECE_SAYISI = 4
# Dört çekmece gövdenin iç yüksekliğini eşit paylaşır; aralarda 1 cm'lik sac kalır.
YUVA_BOY = (BOY - TABAN - UST_KALIN - 0.01 * (CEKMECE_SAYISI + 1)) / CEKMECE_SAYISI

ortak.sahneyi_temizle()
sac = ortak.malzeme("Dosya_Sac", (0.30, 0.33, 0.29), puruz=0.5, metal=0.6)      # soluk yeşil-gri boyalı sac
on_sac = ortak.malzeme("Dosya_On", (0.34, 0.37, 0.33), puruz=0.45, metal=0.6)  # çekmece önü, biraz açık
krom = ortak.malzeme("Dosya_Kulp", (0.62, 0.62, 0.60), puruz=0.25, metal=0.9)
etiket = ortak.malzeme("Dosya_Etiket", (0.80, 0.77, 0.66), puruz=0.8)          # sararmış kâğıt etiket

# Bir çekmece yuvasının orta yüksekliği (i = 0 en üst).
def yuva_z(i):
    ust = BOY - UST_KALIN - 0.01 - i * (YUVA_BOY + 0.01)
    return ust - YUVA_BOY / 2

# ---------- GÖVDE ----------
govde = ortak.kutu("Govde", (EN, DERIN, BOY - UST_KALIN), (0, 0, (BOY - UST_KALIN) / 2), sac, 0.003)
# Her yuva önden açık, arkada 2 cm'lik sac kalır; yanlarda 1,5 cm.
for i in range(CEKMECE_SAYISI):
    ortak.oy(govde, ortak.kutu(f"y{i}", (EN - 0.03, DERIN, YUVA_BOY), (0, -0.02, yuva_z(i)), sac))
ust = ortak.kutu("Ust", (EN + 0.01, DERIN + 0.01, UST_KALIN), (0, 0, BOY - UST_KALIN / 2), sac, 0.004)
govde = ortak.birlestir([govde, ust], "Dosya_Dolabi")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))

# ---------- ÇEKMECELER ----------
# Ön panel gövdenin önünden 1,2 cm taşar (komodindeki ders): gövdenin çarpışma kutusu üst sacın
# kenarına kadar uzanıyor; çekmece önü onun gerisinde kalsaydı nişan ışını önce gövdeye çarpar,
# çekmecenin "AÇ" istemi hiç çıkmazdı.
cekmeceler = []
for i in range(CEKMECE_SAYISI):
    z = yuva_z(i)
    on_y = -DERIN / 2 - 0.012 + 0.009
    on = ortak.kutu("On", (EN - 0.036, 0.018, YUVA_BOY - 0.006), (0, on_y, z), on_sac, 0.003)
    # Tepsi: kâğıtların durduğu kutu; önden 1,8 cm'lik ön panelin arkasından başlar.
    tepsi_derin = DERIN - 0.06
    tepsi_y = -DERIN / 2 + 0.018 + tepsi_derin / 2
    tepsi = ortak.kutu("Tepsi", (EN - 0.05, tepsi_derin, YUVA_BOY - 0.03), (0, tepsi_y, z - 0.01), sac)
    ortak.oy(tepsi, ortak.kutu("ic", (EN - 0.07, tepsi_derin - 0.02, YUVA_BOY), (0, tepsi_y, z + 0.005), sac))
    # Yatay kulp ve üstünde etiket çerçevesi: dosya dolabını dosya dolabı yapan iki ayrıntı.
    kulp = ortak.kutu("Kulp", (0.14, 0.022, 0.018), (0, -DERIN / 2 - 0.012, z - 0.03), krom, 0.004)
    cerceve = ortak.kutu("Cerceve", (0.10, 0.006, 0.045), (0, -DERIN / 2 - 0.006, z + 0.05), krom, 0.001)
    kagit = ortak.kutu("Etiket", (0.085, 0.004, 0.032), (0, -DERIN / 2 - 0.009, z + 0.05), etiket)
    parcalar = [on, tepsi, kulp, cerceve, kagit]
    # En üst çekmecede küçük silindir kilit göbeği: anahtarla açıldığını gözle anlatır.
    if i == 0:
        parcalar.append(ortak.kutu("Kilit_Gobegi", (0.022, 0.012, 0.022), (EN / 2 - 0.06, -DERIN / 2 - 0.009, z + 0.05), krom, 0.004))
    c = ortak.birlestir(parcalar, f"Cekmece_{i + 1}")
    ortak.yumusat(c, 30)
    ortak.pivotu_tasi(c, (0, 0, 0))
    cekmeceler.append(c)

ortak.calistir_ve_bitir("Dosya_Dolabi", [govde] + cekmeceler, BOY, 2.2)
