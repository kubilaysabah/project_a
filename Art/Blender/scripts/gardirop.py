# gardirop.py
# İki kapılı, aynalı ceviz gardırop (#81, Şahan'ın seçimi 2026-09-29: "Blender'da iki kapılı aynalı ceviz gardırop").
# 5. katta vananın önünde duran, iki alet (levye + kaydırıcı) bulununca itilen dolap budur (#65).
# Önceki hâli Poly Haven'ın vitrinli dolabının %60'ıydı (1,21 x 1,34 m); Şahan gerçek boyda bir gardırop istedi.
#
# 90'ların Türk evindeki gardırop: koyu cilalı ceviz, altta süpürgelik (kaide), üstte basamaklı taç,
# her kapıda üstte buzlu kenarlı bir ayna ve altta çıtalı bir göbek, ortada pirinç kulplar ve anahtar deliği.
#
# PARÇALAR
#   Gardirop : tek parça (gövde, kaide, taç, iki kapı, aynalar, kulplar). Pivot tabanın ortası.
#              Kapılar açılmaz: gardırop bütün olarak itilir (Unity'de kökteki AcilirParca kaydırır).
# Ölçü 1,00 (en) x 0,55 (derinlik) x 1,90 (boy) m.
# Ön yüz -y tarafında; Unity'de +Z. (Döndürme yok: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
# Renkler doğrusal değer (Unity sRGB'ye çevirip açık gösterir): 0,05 ekranda koyu ceviz olur (bkz. sofra.py).
ceviz = ortak.malzeme("Gardirop_Ceviz", (0.05, 0.022, 0.008), puruz=0.35)       # cilalı ceviz
ceviz_koyu = ortak.malzeme("Gardirop_Ceviz_Koyu", (0.03, 0.013, 0.005), puruz=0.45)  # çıtalar, kaide
# Ayna: tam metal ve parlak olsaydı Unity'de yansıtacak tek şey gece göğü olurdu ve simsiyah görünürdü
# (#81'deki kara ayna). Metalliği yarım, rengi gümüşi gri: karanlıkta soluk, lamba vurunca parlayan eski cam.
ayna = ortak.malzeme("Gardirop_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.4)
pirinc = ortak.malzeme("Gardirop_Pirinc", (0.52, 0.40, 0.20), puruz=0.35, metal=0.6)

EN, DERIN, BOY = 1.00, 0.55, 1.90
KAIDE = 0.08                    # alttaki süpürgelik yüksekliği
TAC = 0.09                      # üstteki taç (iki basamak) toplam yüksekliği
ON = -DERIN / 2                 # gövdenin ön yüzü (y)

p = []
# Kaide: gövdeden 2 cm içeride, koyu.
p.append(ortak.kutu("Kaide", (EN - 0.04, DERIN - 0.04, KAIDE), (0, 0.01, KAIDE / 2), ceviz_koyu, 0.004))
# Gövde: kaide ile taç arası.
govde_boy = BOY - KAIDE - TAC
p.append(ortak.kutu("Govde", (EN, DERIN, govde_boy), (0, 0, KAIDE + govde_boy / 2), ceviz, 0.006))
# Taç: iki basamak, alttaki dar, üstteki taşkın; önde ve yanlarda gövdeden çıkar.
p.append(ortak.kutu("Tac1", (EN + 0.03, DERIN + 0.02, 0.04), (0, -0.01, BOY - TAC + 0.02), ceviz, 0.004))
p.append(ortak.kutu("Tac2", (EN + 0.07, DERIN + 0.04, 0.05), (0, -0.02, BOY - 0.025), ceviz, 0.006))

# Kapılar: gövdenin önüne 2 cm çıkıntılı iki kanat, ortada 6 mm aralık.
KAPI_ALT, KAPI_UST = KAIDE + 0.03, BOY - TAC - 0.03
kapi_boy = KAPI_UST - KAPI_ALT
kapi_en = EN / 2 - 0.03
kapi_y = ON - 0.01                                  # kanadın merkezi (2 cm kalınlık)
yuz = ON - 0.02                                     # kanadın ön yüzü
for yon in (-1, 1):
    x = yon * (kapi_en / 2 + 0.003)
    p.append(ortak.kutu("Kanat", (kapi_en, 0.02, kapi_boy), (x, kapi_y, KAPI_ALT + kapi_boy / 2), ceviz, 0.004))
    # Ayna: kanadın üst üçte ikisi, kenarlarda 7 cm ahşap kalır; çevresinde ince koyu çıta.
    ayna_alt = KAPI_ALT + kapi_boy * 0.36
    ayna_ust = KAPI_UST - 0.08
    ayna_en = kapi_en - 0.14
    ayna_boy = ayna_ust - ayna_alt
    p.append(ortak.kutu("Ayna", (ayna_en, 0.004, ayna_boy), (x, yuz - 0.001, ayna_alt + ayna_boy / 2), ayna))
    for dz in (ayna_alt - 0.008, ayna_ust + 0.008):             # üst ve alt çıta
        p.append(ortak.kutu("AynaCita", (ayna_en + 0.032, 0.008, 0.016), (x, yuz - 0.003, dz), ceviz_koyu, 0.002))
    for dx in (-1, 1):                                          # yan çıtalar
        p.append(ortak.kutu("AynaCita", (0.016, 0.008, ayna_boy), (x + dx * (ayna_en / 2 + 0.008), yuz - 0.003,
                                                                     ayna_alt + ayna_boy / 2), ceviz_koyu, 0.002))
    # Alt göbek: aynanın altında çıtalı, hafif kabarık bir pano.
    gob_alt, gob_ust = KAPI_ALT + 0.08, ayna_alt - 0.08
    gob_en = kapi_en - 0.16
    gob_boy = gob_ust - gob_alt
    p.append(ortak.kutu("Gobek", (gob_en, 0.01, gob_boy), (x, yuz - 0.004, gob_alt + gob_boy / 2), ceviz, 0.004))
    for dz in (gob_alt - 0.01, gob_ust + 0.01):
        p.append(ortak.kutu("GobekCita", (gob_en + 0.04, 0.006, 0.014), (x, yuz - 0.002, dz), ceviz_koyu, 0.002))
    for dx in (-1, 1):
        p.append(ortak.kutu("GobekCita", (0.014, 0.006, gob_boy + 0.034), (x + dx * (gob_en / 2 + 0.013), yuz - 0.002,
                                                                           gob_alt + gob_boy / 2), ceviz_koyu, 0.002))
    # Kulp: ortadaki aralığın yanında dikey pirinç tutamak ve iki ayağı.
    kx = yon * 0.045
    p.append(ortak.silindir("Kulp", 0.007, 0.16, (kx, yuz - 0.03, 1.02), pirinc, kose=12))
    for dz in (0.95, 1.09):
        p.append(ortak.kutu("KulpAyak", (0.012, 0.03, 0.012), (kx, yuz - 0.015, dz), pirinc, 0.002))

# Anahtar deliği: sağ kanatta, kulpun altında pirinç ayna (plaka) ve koyu yarık.
p.append(ortak.kutu("AnahtarPlaka", (0.03, 0.004, 0.06), (0.045, yuz - 0.002, 0.88), pirinc, 0.003))
p.append(ortak.kutu("AnahtarDelik", (0.006, 0.002, 0.02), (0.045, yuz - 0.0045, 0.875), ceviz_koyu))
# Ortadaki kapı aralığının arkasındaki kör çıta (aralıktan gövdenin içi görünmesin).
p.append(ortak.kutu("KorCita", (0.03, 0.006, kapi_boy), (0, ON - 0.001, KAPI_ALT + kapi_boy / 2), ceviz_koyu))

gardirop = ortak.birlestir(p, "Gardirop")
ortak.yumusat(gardirop, 30)
ortak.pivotu_tasi(gardirop, (0, 0, 0))
ortak.calistir_ve_bitir("Gardirop", [gardirop], 1.9, 3.4)
