# portmanto.py
# 90'ların antre portmantosu (#83, Şahan'ın seçimi 2026-09-29: 1. katın antresi).
# Türk evinin antresindeki mobilya: altta kapaklı ayakkabı dolabı, üstünde aynalı arka pano,
# aynanın iki yanında pirinç mont askıları, tepede şapka rafı, aynanın altında küçük bir çekmece.
# 1. kattaki taşınan aile onu ağır olduğu için geride bırakmış; askıları boş.
#
# PARÇALAR
#   Portmanto : gövde (dolap, pano, ayna, askılar, çekmece yuvası). Pivot tabanın ortası.
#   Cekmece   : aynanın altındaki küçük çekmece, içi boş tepsi. Pivotu gövdeyle aynı nokta (tabanın
#               ortası); Unity'de öne (+Z) kayarak açılır. 1. katta tornavida bunun içinde (#83).
# Ön yüz -y tarafında; Unity'de +Z.
# Ölçü 1,00 (en) x 0,36 (derinlik, alt dolap) x 1,95 (boy) m.
# (Döndürme yok: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
# Renkler doğrusal değer: Unity daha açık gösterir (0,05 ekranda koyu ceviz olur, bkz. sofra.py).
ceviz = ortak.malzeme("Portmanto_Ceviz", (0.06, 0.028, 0.010), puruz=0.35)
ceviz_koyu = ortak.malzeme("Portmanto_Ceviz_Koyu", (0.03, 0.013, 0.005), puruz=0.45)
# Ayna: yarı metal gümüşi gri (tam metal ayna gece göğünü yansıtıp kararıyordu, #81).
ayna = ortak.malzeme("Portmanto_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.4)
pirinc = ortak.malzeme("Portmanto_Pirinc", (0.52, 0.40, 0.20), puruz=0.35, metal=0.6)

EN, DERIN, BOY = 1.00, 0.36, 1.95
ON = -DERIN / 2                  # alt dolabın ön yüzü (y)
ARKA = DERIN / 2                 # arka yüz (duvara değer)
p = []

# ---------- ALT: AYAKKABI DOLABI ----------
DOLAP_UST = 0.55
p.append(ortak.kutu("Kaide", (EN - 0.04, DERIN - 0.03, 0.05), (0, 0.01, 0.025), ceviz_koyu, 0.003))
p.append(ortak.kutu("Dolap", (EN, DERIN, DOLAP_UST - 0.05), (0, 0, 0.05 + (DOLAP_UST - 0.05) / 2), ceviz, 0.005))
p.append(ortak.kutu("Tabla", (EN + 0.02, DERIN + 0.02, 0.025), (0, 0, DOLAP_UST + 0.0125), ceviz, 0.005))
# İki kapak: önde 1 cm çıkıntılı, üstlerinde yatay havalandırma çıtaları (ayakkabı kokusu çıksın diye).
kapak_en = EN / 2 - 0.03
for yon in (-1, 1):
    x = yon * (kapak_en / 2 + 0.004)
    p.append(ortak.kutu("Kapak", (kapak_en, 0.012, 0.40), (x, ON - 0.006, 0.30), ceviz, 0.003))
    for i in range(6):
        p.append(ortak.kutu("Cita", (kapak_en - 0.08, 0.008, 0.016), (x, ON - 0.014, 0.17 + i * 0.045), ceviz_koyu, 0.002))
    # Topuz kulp: kapağın ortaya yakın üst köşesinde.
    p.append(ortak.silindir("Kulp", 0.012, 0.02, (yon * 0.06, ON - 0.02, 0.45), pirinc, kose=12))

# ---------- ÜST: AYNALI PANO ----------
PANO_ALT, PANO_UST = DOLAP_UST + 0.025, BOY - 0.08
pano_boy = PANO_UST - PANO_ALT
p.append(ortak.kutu("Pano", (EN - 0.08, 0.022, pano_boy), (0, ARKA - 0.011, PANO_ALT + pano_boy / 2), ceviz, 0.003))
# İki yan dikme ve tepe kuşağı: panoyu çerçeveler.
for yon in (-1, 1):
    p.append(ortak.kutu("Dikme", (0.05, 0.06, BOY - DOLAP_UST), (yon * (EN / 2 - 0.025), ARKA - 0.03, DOLAP_UST + (BOY - DOLAP_UST) / 2), ceviz, 0.004))
p.append(ortak.kutu("Tepe", (EN + 0.02, 0.07, 0.07), (0, ARKA - 0.035, BOY - 0.035), ceviz, 0.005))
# Şapka rafı: tepe kuşağının altında öne uzanan raf ve iki konsolu.
p.append(ortak.kutu("SapkaRafi", (EN - 0.10, 0.22, 0.02), (0, ARKA - 0.11, BOY - 0.12), ceviz, 0.003))
for yon in (-1, 1):
    p.append(ortak.kutu("Konsol", (0.02, 0.16, 0.08), (yon * (EN / 2 - 0.10), ARKA - 0.09, BOY - 0.17), ceviz_koyu, 0.002))

# Ayna: panonun ortasında, çevresinde kalın koyu çerçeve ve üstünde basamaklı taç.
AYNA_EN, AYNA_BOY, AYNA_Z = 0.42, 0.72, 1.28
yuz = ARKA - 0.022
p.append(ortak.kutu("Ayna", (AYNA_EN, 0.006, AYNA_BOY), (0, yuz - 0.003, AYNA_Z), ayna))
for dz in (-1, 1):
    p.append(ortak.kutu("AynaCerceve", (AYNA_EN + 0.08, 0.02, 0.04), (0, yuz - 0.01, AYNA_Z + dz * (AYNA_BOY / 2 + 0.02)), ceviz_koyu, 0.004))
for dx in (-1, 1):
    p.append(ortak.kutu("AynaCerceve", (0.04, 0.02, AYNA_BOY), (dx * (AYNA_EN / 2 + 0.02), yuz - 0.01, AYNA_Z), ceviz_koyu, 0.004))
p.append(ortak.kutu("AynaTaci", (0.26, 0.02, 0.05), (0, yuz - 0.01, AYNA_Z + AYNA_BOY / 2 + 0.065), ceviz_koyu, 0.004))

# Aynanın altında çekmece yuvası: panodan öne çıkan kutu, önü açık oyuk (çekmece içine girer).
CEK_Z = 0.78
YUVA_ARKA = ARKA - 0.022                  # panonun ön yüzü
yuva_y = YUVA_ARKA - 0.075                # yuvanın merkezi (15 cm derin)
yuva = ortak.kutu("Yuva", (0.44, 0.15, 0.11), (0, yuva_y, CEK_Z), ceviz, 0.004)
ortak.oy(yuva, ortak.kutu("YuvaIc", (0.40, 0.15, 0.09), (0, yuva_y - 0.01, CEK_Z), ceviz))
p.append(yuva)

# Askılar: aynanın iki yanında, iki sıra halinde çift kollu pirinç askı (taban pulu, gövde, iki uç).
for yon in (-1, 1):
    for z in (1.52, 1.18):
        x = yon * 0.35
        p.append(ortak.kutu("AskiPul", (0.03, 0.008, 0.03), (x, yuz - 0.004, z), pirinc, 0.004))  # panodaki taban pulu
        p.append(ortak.kutu("AskiGovde", (0.012, 0.09, 0.012), (x, yuz - 0.05, z - 0.01), pirinc, 0.003))
        p.append(ortak.kutu("AskiUstUc", (0.012, 0.012, 0.05), (x, yuz - 0.095, z + 0.015), pirinc, 0.003))
        p.append(ortak.kutu("AskiAltUc", (0.012, 0.05, 0.012), (x, yuz - 0.06, z - 0.06), pirinc, 0.003))

portmanto = ortak.birlestir(p, "Portmanto")
ortak.yumusat(portmanto, 30)
ortak.pivotu_tasi(portmanto, (0, 0, 0))

# ---------- ÇEKMECE ----------
# İçi oyulmuş tepsi, önünde yuvanın ağzını kapatan ön yüz ve pirinç kulp.
on_y = yuva_y - 0.075                     # yuvanın ağzı
tepsi = ortak.kutu("Tepsi", (0.39, 0.13, 0.085), (0, yuva_y - 0.005, CEK_Z), ceviz, 0.002)
ortak.oy(tepsi, ortak.kutu("TepsiIc", (0.37, 0.11, 0.08), (0, yuva_y - 0.005, CEK_Z + 0.01), ceviz))
on = ortak.kutu("On", (0.42, 0.012, 0.10), (0, on_y - 0.006, CEK_Z), ceviz, 0.003)
kulp = ortak.kutu("Kulp", (0.08, 0.012, 0.012), (0, on_y - 0.018, CEK_Z), pirinc, 0.003)
cekmece = ortak.birlestir([tepsi, on, kulp], "Cekmece")
ortak.yumusat(cekmece, 30)
ortak.pivotu_tasi(cekmece, (0, 0, 0))

ortak.calistir_ve_bitir("Portmanto", [portmanto, cekmece], 1.95, 3.4)
