# zincir_esyalari.py
# Kilit zincirinde elle taşınan küçük eşyalar (#54): tornavida ve buşonlu sigorta.
# Oda anahtarı ile kat anahtarı ayrı bir betikte (kat_kapisi.py, Anahtar.fbx).
#
# İki ayrı FBX üretir; her birinin pivotu eşyanın kendi ortasında, yere yatık durur
# (çekmecede, rafta ya da yerde bulunurlar; Unity'de "Esya" bileşeniyle alınırlar).
#   Tornavida.fbx : sarı-siyah plastik saplı düz uçlu tornavida (Cemal'in).
#   Buson.fbx     : porselen gövdeli, kırmızı başlıklı vidalı sigorta (sigorta kutusuna takılır).

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak


def yatir(n, eksen="x"):
    """Dik silindiri yatırır (x ekseni boyunca uzanır) ve dönüşü meshe işler."""
    n.rotation_euler = (0, math.pi / 2, 0) if eksen == "x" else (math.pi / 2, 0, 0)
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(rotation=True)
    return n


# ---------- TORNAVİDA ----------
ortak.sahneyi_temizle()
sari = ortak.malzeme("Tornavida_Sap", (0.80, 0.55, 0.05), puruz=0.35)
siyah = ortak.malzeme("Tornavida_Siyah", (0.03, 0.03, 0.03), puruz=0.4)
celik = ortak.malzeme("Tornavida_Celik", (0.60, 0.60, 0.62), puruz=0.25, metal=1.0)
R = 0.015                                      # sap yarıçapı; tornavida yerde yatarken bu yükseklikte
sap = yatir(ortak.silindir("Sap", R, 0.10, (-0.05, 0, R), sari, kose=12, pah_genislik=0.004))
bilezik = yatir(ortak.silindir("Bilezik", R * 0.8, 0.012, (0.006, 0, R), siyah, kose=12))
govde = yatir(ortak.silindir("Govde", 0.003, 0.11, (0.067, 0, R), celik, kose=10))
uc = ortak.kutu("Uc", (0.012, 0.007, 0.0015), (0.126, 0, R), celik)
tornavida = ortak.birlestir([sap, bilezik, govde, uc], "Tornavida")
ortak.yumusat(tornavida, 40)
ortak.pivotu_tasi(tornavida, (0.02, 0, 0))     # ortası yakınında, tabanda
ortak.kaydet_ve_disari_aktar("Tornavida", [tornavida])
print("OLCU", tornavida.name, round(tornavida.dimensions.x, 3), round(tornavida.dimensions.y, 3), round(tornavida.dimensions.z, 3))

# ---------- BUŞONLU SİGORTA ----------
ortak.sahneyi_temizle()
porselen = ortak.malzeme("Buson_Porselen", (0.86, 0.85, 0.80), puruz=0.3)
kirmizi = ortak.malzeme("Buson_Kirmizi", (0.55, 0.05, 0.04), puruz=0.4)
pirinc = ortak.malzeme("Buson_Pirinc", (0.62, 0.50, 0.22), puruz=0.3, metal=1.0)
RB = 0.014
g = yatir(ortak.silindir("Govde", RB, 0.03, (0, 0, RB), porselen, kose=16))
b = yatir(ortak.silindir("Baslik", RB * 0.72, 0.008, (0.019, 0, RB), kirmizi, kose=16))
v = yatir(ortak.silindir("Vida", RB * 0.55, 0.01, (-0.02, 0, RB), pirinc, kose=16))
buson = ortak.birlestir([g, b, v], "Buson")
ortak.yumusat(buson, 40)
ortak.pivotu_tasi(buson, (0, 0, 0))
ortak.kaydet_ve_disari_aktar("Buson", [buson])
print("OLCU", buson.name, round(buson.dimensions.x, 3), round(buson.dimensions.y, 3), round(buson.dimensions.z, 3))
