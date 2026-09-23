# ayakkabilik.py
# Kapı yanındaki iki kapaklı ahşap ayakkabılık: Türk apartmanı kimliğinin en ucuz işareti.
# Ölçü graybox'la aynı: 80 cm en, 35 cm derinlik, 60 cm yükseklik.
#
# Yön: ön yüz (kapaklar) -y tarafında. Pivot: tabanın alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, DERIN, BOY = 0.80, 0.35, 0.60
AYAK = 0.04   # alttaki geri çekik kaide

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Ayakkabilik_Ahsap", (0.30, 0.17, 0.08), puruz=0.6)     # ceviz kaplama
koyu = ortak.malzeme("Ayakkabilik_Kaide", (0.12, 0.07, 0.04), puruz=0.7)
metal = ortak.malzeme("Ayakkabilik_Kulp", (0.7, 0.68, 0.62), puruz=0.3, metal=1.0)

parcalar = []
# Kaide: gövdeden 2 cm geride; ayakkabılık yerde "süzülüyormuş" gibi durur.
parcalar.append(ortak.kutu("Kaide", (EN - 0.04, DERIN - 0.04, AYAK), (0, 0, AYAK / 2), koyu))
# Gövde.
govde_boy = BOY - AYAK - 0.02
parcalar.append(ortak.kutu("Govde", (EN, DERIN, govde_boy), (0, 0, AYAK + govde_boy / 2), ahsap, 0.003))
# Üst tabla: gövdeden 1 cm taşar.
parcalar.append(ortak.kutu("Tabla", (EN + 0.02, DERIN + 0.02, 0.02), (0, 0, BOY - 0.01), ahsap, 0.004))

# İki kapak: gövdenin önünde, aralarında ince bir derz.
kapak_en = EN / 2 - 0.012
for yon in (-1, 1):
    x = yon * (EN / 4)
    parcalar.append(ortak.kutu("Kapak", (kapak_en, 0.016, govde_boy - 0.03),
                               (x, -DERIN / 2 - 0.008, AYAK + govde_boy / 2), ahsap, 0.003))
    # Kulp: kapağın ortaya bakan kenarında küçük metal tutamak.
    parcalar.append(ortak.kutu("Kulp", (0.012, 0.02, 0.07),
                               (yon * 0.04, -DERIN / 2 - 0.022, AYAK + govde_boy * 0.62), metal, 0.002))

ayakkabilik = ortak.tek_parca(parcalar, "Ayakkabilik", 30)
ortak.calistir_ve_bitir("Ayakkabilik", [ayakkabilik], BOY, 1.6)
