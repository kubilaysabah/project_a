# cam_kiriklari.py
# Kırılan cam kavanozun yere saçılan parçaları (#61, "kırma" kilidi).
# Kavanoz tornavidayla kırılınca gövdesi kaybolur, yerine bu parçalar konur; içindeki eşya
# ortada kalır. Tek parça bir mesh: kavanozun dibi (yarım halka) ve etrafa saçılmış ince
# cam kırıkları. Pivot kavanozun durduğu noktanın tabanı.

import math, os, random, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

random.seed(61)
ortak.sahneyi_temizle()
cam = ortak.malzeme("Kavanoz_Cam", (0.20, 0.28, 0.22), puruz=0.05)   # kavanozla aynı cam

parcalar = []
# Kavanozun dibi: kırılmadan kalan alçak bir halka parçası.
dip = ortak.silindir("Dip", 0.055, 0.02, (0, 0, 0.01), cam, kose=24)
ortak.oy(dip, ortak.silindir("dip_ic", 0.051, 0.03, (0, 0, 0.018), cam, kose=24))
ortak.oy(dip, ortak.kutu("yarim", (0.12, 0.06, 0.05), (0, 0.035, 0.01), cam))   # yarısı kırık
parcalar.append(dip)

# Saçılan kırıklar: ince, düzensiz, hafif eğik levhacıklar; 4-16 cm uzağa.
for i in range(11):
    a = random.uniform(0, 2 * math.pi)
    r = random.uniform(0.04, 0.16)
    en, boy = random.uniform(0.012, 0.035), random.uniform(0.010, 0.028)
    k = ortak.kutu(f"Kirik{i}", (en, boy, 0.0025), (math.cos(a) * r, math.sin(a) * r, 0.0015), cam)
    k.rotation_euler = (random.uniform(-0.25, 0.25), random.uniform(-0.25, 0.25), random.uniform(0, math.pi))
    parcalar.append(k)
# Birkaç büyük eğri parça: kavanoz duvarından kopmuş, yan yatmış.
for i in range(3):
    a = random.uniform(0, 2 * math.pi)
    r = random.uniform(0.07, 0.12)
    k = ortak.kutu(f"Duvar{i}", (0.045, 0.004, 0.035), (math.cos(a) * r, math.sin(a) * r, 0.012), cam)
    k.rotation_euler = (random.uniform(0.9, 1.3), 0, a)
    parcalar.append(k)

kirik = ortak.birlestir(parcalar, "Cam_Kiriklari")
ortak.pivotu_tasi(kirik, (0, 0, 0))
ortak.calistir_ve_bitir("Cam_Kiriklari", [kirik], 0.05, 0.5)
