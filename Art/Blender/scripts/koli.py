# koli.py
# Taşınan ailenin karton kolileri (#55, 1. kat "taşınmış aile" dekoru).
#
# Üç ayrı FBX üretir; pivotları tabanın ortasında, ön yüzleri -y'de (Unity'de +Z):
#   Koli_Buyuk.fbx : kapalı, üstü koli bandıyla bantlı büyük koli (60 x 40 x 40 cm).
#   Koli_Kucuk.fbx : kapalı küçük koli (40 x 30 x 30 cm), büyüğün üstüne konabilir.
#   Koli_Acik.fbx  : kapakları açık, içi boş koli (50 x 40 x 35 cm).
# Karton rengi hafif farklı üç tonda, bant sarımsı-şeffaf görünümlü.

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak


def koli(ad, en, derin, boy, ton, acik=False):
    ortak.sahneyi_temizle()
    karton = ortak.malzeme(f"Koli_Karton_{ad}", ton, puruz=0.85)
    bant = ortak.malzeme("Koli_Bant", (0.62, 0.52, 0.32), puruz=0.35)
    kalem = ortak.malzeme("Koli_Kalem", (0.05, 0.05, 0.06), puruz=0.8)
    parcalar = []
    govde = ortak.kutu("Govde", (en, derin, boy), (0, 0, boy / 2), karton, 0.004)
    if acik:
        # İçini oy; dört kapak dışa doğru eğik açılmış.
        ortak.oy(govde, ortak.kutu("ic", (en - 0.01, derin - 0.01, boy), (0, 0, boy / 2 + 0.005), karton))
        for i, (x, y, ex, ey, rx, ry) in enumerate((
                (0, derin / 2, en, derin / 2, -1.9, 0), (0, -derin / 2, en, derin / 2, 1.9, 0),
                (en / 2, 0, en / 2, derin, 0, 1.9), (-en / 2, 0, en / 2, derin, 0, -1.9))):
            k = ortak.kutu(f"Kapak{i}", (ex if ry == 0 else ex, ey if rx == 0 else ey, 0.004), (x, y, boy), karton)
            # Kapakları kenarlarından dışarı devir: önce pivotu kenara al, sonra döndür.
            ortak.pivotu_tasi(k, (x, y, boy))
            k.rotation_euler = (rx * 0.55, ry * 0.55, 0)
            parcalar.append(k)
    else:
        # Üstte ve iki yanda koli bandı; ön yüzde kalemle yazılmış bir şerit (etiket yeri).
        parcalar.append(ortak.kutu("BantUst", (0.05, derin + 0.002, 0.002), (0, 0, boy + 0.001), bant))
        parcalar.append(ortak.kutu("BantOn", (0.05, 0.002, 0.12), (0, -derin / 2 - 0.001, boy - 0.06), bant))
        parcalar.append(ortak.kutu("BantArka", (0.05, 0.002, 0.12), (0, derin / 2 + 0.001, boy - 0.06), bant))
        parcalar.append(ortak.kutu("Yazi", (en * 0.45, 0.001, 0.03), (-en * 0.18, -derin / 2 - 0.0015, boy * 0.55), kalem))
    n = ortak.birlestir([govde] + parcalar, ad)
    ortak.pivotu_tasi(n, (0, 0, 0))
    ortak.kaydet_ve_disari_aktar(ad, [n])
    print("OLCU", ad, round(n.dimensions.x, 3), round(n.dimensions.y, 3), round(n.dimensions.z, 3))


koli("Koli_Buyuk", 0.60, 0.40, 0.40, (0.55, 0.40, 0.24))
koli("Koli_Kucuk", 0.40, 0.30, 0.30, (0.60, 0.45, 0.28))
koli("Koli_Acik", 0.50, 0.40, 0.35, (0.52, 0.38, 0.22), acik=True)
