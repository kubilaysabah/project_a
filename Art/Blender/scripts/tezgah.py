# tezgah.py
# Mutfak tezgâhı: alt dolaplar, mermer tabla, eviye; solda üç çekmece, sağda iki kapak (#53).
# Bulmaca eşyası: 4. katta sigorta teli mutfak çekmecesinde (Docs/hikaye.md §7); çekmeceler
# ve dolaplar küçük eşyaların (anahtar, not, fatura) saklandığı yerler.
#
# PARÇALAR (hepsi aynı dosyada, adlarıyla)
#   Tezgah               : dolap gövdesi, mermer tabla, eviye ve musluk; pivot tabanın ortası.
#   Cekmece_1..3         : üstten alta üç çekmece; pivot tabanın ortası, Unity'de +Z'ye kayar.
#   Kapak_1, Kapak_2     : iki dolap kapağı; pivotları dış kenarlarındaki menteşe çizgisinde.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

UZUN, DERIN, BOY = 1.80, 0.60, 0.86        # gövde (tabla hariç)
TABLA = 0.04
SUPURGE = 0.10                              # alttaki geri çekik süpürgelik
BOLME_X = (-0.30, 0.30)                     # çekmece bölümü | kapak 1 | kapak 2
ON_KALIN = 0.018

ortak.sahneyi_temizle()
lamine = ortak.malzeme("Tezgah_Lamine", (0.78, 0.73, 0.62), puruz=0.45)     # krem lamine kapaklar
govde_mat = ortak.malzeme("Tezgah_Govde", (0.55, 0.48, 0.38), puruz=0.6)    # sunta gövde
mermer = ortak.malzeme("Tezgah_Mermer", (0.72, 0.71, 0.67), puruz=0.25)     # açık gri mermer
celik = ortak.malzeme("Tezgah_Celik", (0.62, 0.62, 0.60), puruz=0.25, metal=1.0)
koyu = ortak.malzeme("Tezgah_Koyu", (0.08, 0.07, 0.06), puruz=0.8)

on_y = -DERIN / 2
# ---------- GÖVDE ----------
govde = ortak.kutu("Govde", (UZUN, DERIN, BOY - SUPURGE), (0, 0, SUPURGE + (BOY - SUPURGE) / 2), govde_mat)
ortak.oy(govde, ortak.kutu("ic", (UZUN - 0.04, DERIN, BOY - SUPURGE - 0.04), (0, -0.02, SUPURGE + (BOY - SUPURGE) / 2), govde_mat))
parcalar = [govde,
            ortak.kutu("Supurge", (UZUN, DERIN - 0.06, SUPURGE), (0, 0.03, SUPURGE / 2), koyu)]
# Bölme duvarları ve kapak bölümlerinde bir iç raf.
for x in BOLME_X:
    parcalar.append(ortak.kutu(f"Bolme{x}", (0.018, DERIN - 0.02, BOY - SUPURGE - 0.04), (x, 0.01, SUPURGE + (BOY - SUPURGE) / 2), govde_mat))
parcalar.append(ortak.kutu("IcRaf", (UZUN / 3 * 2 - 0.05, DERIN - 0.04, 0.018), (0.30, 0.01, SUPURGE + 0.36), govde_mat))
# Tabla: önden ve yanlardan 1 cm taşan mermer, eviye için oyulmuş.
eviye_x, eviye_en, eviye_derin = 0.62, 0.46, 0.38
tabla = ortak.kutu("Tabla", (UZUN + 0.02, DERIN + 0.02, TABLA), (0, -0.01, BOY + TABLA / 2), mermer, 0.004)
ortak.oy(tabla, ortak.kutu("e", (eviye_en, eviye_derin, TABLA * 3), (eviye_x, -0.02, BOY + TABLA / 2), mermer))
parcalar.append(tabla)
# Eviye çanağı: tablanın altına sarkan çelik kutu, içi oyulmuş.
canak = ortak.kutu("Canak", (eviye_en, eviye_derin, 0.18), (eviye_x, -0.02, BOY + TABLA - 0.09), celik, 0.01)
ortak.oy(canak, ortak.kutu("c", (eviye_en - 0.02, eviye_derin - 0.02, 0.18), (eviye_x, -0.02, BOY + TABLA - 0.08), celik))
parcalar.append(canak)
# Musluk: arkada dik boru ve öne uzanan ağız.
parcalar.append(ortak.silindir("MuslukBoru", 0.013, 0.22, (eviye_x, DERIN / 2 - 0.07, BOY + TABLA + 0.11), celik, kose=16))
parcalar.append(ortak.kutu("MuslukAgiz", (0.025, 0.16, 0.022), (eviye_x, DERIN / 2 - 0.15, BOY + TABLA + 0.21), celik, 0.006))
tezgah = ortak.birlestir(parcalar, "Tezgah")
ortak.yumusat(tezgah, 30)
ortak.pivotu_tasi(tezgah, (0, 0, 0))

nesneler = [tezgah]

# ---------- ÇEKMECELER ----------
# Çekmece bölümü x = -0.90 .. -0.30. Ön yükseklik (süpürgelikten tablaya) üç eşit parçaya bölünür.
cek_x0, cek_x1 = -UZUN / 2 + 0.02, BOLME_X[0] - 0.009
cek_en = cek_x1 - cek_x0
cek_ortax = (cek_x0 + cek_x1) / 2
on_boy = (BOY - SUPURGE - 0.02) / 3
for i in range(3):
    z = BOY - 0.01 - on_boy / 2 - i * on_boy          # üstten alta
    on = ortak.kutu("On", (cek_en - 0.006, ON_KALIN, on_boy - 0.006), (cek_ortax, on_y - ON_KALIN / 2, z), lamine, 0.003)
    tepsi = ortak.kutu("Tepsi", (cek_en - 0.03, DERIN - 0.08, on_boy - 0.06), (cek_ortax, on_y + (DERIN - 0.08) / 2, z - 0.01), govde_mat)
    ortak.oy(tepsi, ortak.kutu("t", (cek_en - 0.05, DERIN - 0.10, on_boy), (cek_ortax, on_y + (DERIN - 0.08) / 2, z + 0.01), govde_mat))
    kulp = ortak.kutu("Kulp", (0.12, 0.022, 0.014), (cek_ortax, on_y - ON_KALIN - 0.011, z + on_boy / 2 - 0.04), celik, 0.004)
    c = ortak.birlestir([on, tepsi, kulp], f"Cekmece_{i + 1}")
    ortak.yumusat(c, 30)
    ortak.pivotu_tasi(c, (0, 0, 0))
    nesneler.append(c)

# ---------- DOLAP KAPAKLARI ----------
# Kapak 1: x = -0.30 .. 0.30, menteşe sol kenarda. Kapak 2: x = 0.30 .. 0.90, menteşe sağ kenarda.
kapak_boy = BOY - SUPURGE - 0.02
kapak_z = SUPURGE + 0.01 + kapak_boy / 2
for i, (x0, x1, mentese_x) in enumerate(((BOLME_X[0], BOLME_X[1], BOLME_X[0]),
                                         (BOLME_X[1], UZUN / 2 - 0.02, UZUN / 2 - 0.02))):
    en = x1 - x0 - 0.008
    orta = (x0 + x1) / 2
    panel = ortak.kutu("Panel", (en, ON_KALIN, kapak_boy), (orta, on_y - ON_KALIN / 2, kapak_z), lamine, 0.003)
    # Kulp menteşenin karşı kenarında, üstte.
    karsi = x1 - 0.05 if mentese_x == x0 else x0 + 0.05
    kulp = ortak.kutu("Kulp", (0.014, 0.022, 0.12), (karsi, on_y - ON_KALIN - 0.011, BOY - 0.12), celik, 0.004)
    k = ortak.birlestir([panel, kulp], f"Kapak_{i + 1}")
    ortak.yumusat(k, 30)
    ortak.pivotu_tasi(k, (mentese_x, on_y - ON_KALIN / 2, 0))
    nesneler.append(k)

ortak.calistir_ve_bitir("Tezgah", nesneler, BOY, 2.8)
