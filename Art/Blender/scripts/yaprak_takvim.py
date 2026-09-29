# yaprak_takvim.py
# Nuri Bey'in antresindeki yaprak takvim (#83, 2. kat; Şahan'ın seçimi 2026-09-29).
# Her sabah bir yaprağı koparılan duvar takvimi: üstte bakkal reklamlı karton (renkli resim alanı),
# altında kalın bir yaprak destesi. En üstteki yaprak hastaneye kaldırıldığı günde kaldı: kocaman
# kırmızı "12", altında "9 1994" (ay ve yıl). Rakamlar dil gerektirmesin diye yazı değil, yedi
# parçalı (dijital saat gibi) kutulardan kurulu; İngilizce oyuncu da aynı şeyi okur.
#
# PARÇALAR
#   Yaprak_Takvim : tek parça. Pivot ARKA yüzün (duvar) ortası, destenin ortası hizasında. Ön yüz -y.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
karton = ortak.malzeme("Takvim_Karton", (0.50, 0.30, 0.10), puruz=0.8)      # turuncu bakkal kartonu
resim = ortak.malzeme("Takvim_Resim", (0.10, 0.25, 0.30), puruz=0.8)        # manzara resmi (mavi-yeşil)
resim2 = ortak.malzeme("Takvim_Resim2", (0.30, 0.35, 0.10), puruz=0.8)
kagit = ortak.malzeme("Takvim_Kagit", (0.82, 0.80, 0.74), puruz=0.9)
kirmizi = ortak.malzeme("Takvim_Kirmizi", (0.55, 0.04, 0.04), puruz=0.8)
siyah = ortak.malzeme("Takvim_Siyah", (0.04, 0.04, 0.04), puruz=0.8)

p = []
# Karton: 0,20 x 0,34; üst yarısında resim alanı.
p.append(ortak.kutu("Karton", (0.20, 0.004, 0.34), (0, -0.002, 0.05), karton))
p.append(ortak.kutu("Resim", (0.17, 0.002, 0.10), (0, -0.005, 0.16), resim))
p.append(ortak.kutu("Resim2", (0.17, 0.002, 0.04), (0, -0.0055, 0.13), resim2))
# Deste: kartonun alt yarısında kalın yaprak yığını.
DY = -0.004 - 0.015
p.append(ortak.kutu("Deste", (0.14, 0.03, 0.12), (0, DY, -0.02), kagit))
on = DY - 0.015                                      # en üst yaprağın ön yüzü

# Yedi parçalı rakam: a (üst), b (sağ üst), c (sağ alt), d (alt), e (sol alt), f (sol üst), g (orta).
SEGMENT = {"0": "abcdef", "1": "bc", "2": "abged", "3": "abgcd", "4": "fgbc", "5": "afgcd",
           "6": "afgedc", "7": "abc", "8": "abcdefg", "9": "abcfgd"}


def rakam(ch, x, z, boy, kalin, mat):
    """(x, z) merkezli, boy yüksekliğinde bir rakam; yaprağın ön yüzüne yapışık ince kutular."""
    g = boy * 0.55                                   # rakamın eni
    y = on - 0.0008
    yatay = {"a": z + boy / 2, "g": z, "d": z - boy / 2}
    for s in SEGMENT[ch]:
        if s in yatay:
            p.append(ortak.kutu("Seg", (g, 0.0012, kalin), (x, y, yatay[s]), mat))
        else:
            sx = x + (g / 2 if s in "bc" else -g / 2)
            sz = z + (boy / 4 if s in "bf" else -boy / 4)
            p.append(ortak.kutu("Seg", (kalin, 0.0012, boy / 2), (sx, y, sz), mat))


# Kocaman kırmızı gün: "12".
rakam("1", -0.030, 0.0, 0.065, 0.010, kirmizi)
rakam("2", 0.030, 0.0, 0.065, 0.010, kirmizi)
# Altında ay ve yıl: "9" ve "1994", küçük siyah.
for i, ch in enumerate("9"):
    rakam(ch, -0.050, -0.058, 0.018, 0.0035, siyah)
for i, ch in enumerate("1994"):
    rakam(ch, -0.012 + i * 0.017, -0.058, 0.018, 0.0035, siyah)
# Koparılan yaprakların destenin üstünde bıraktığı yırtık şerit.
p.append(ortak.kutu("Yirtik", (0.14, 0.004, 0.006), (0, on - 0.001, 0.038), kagit))

takvim = ortak.birlestir(p, "Yaprak_Takvim")
ortak.pivotu_tasi(takvim, (0, 0, 0))
ortak.calistir_ve_bitir("Yaprak_Takvim", [takvim], 0.35, 0.8, onizlemede_kaldir=0.2)
