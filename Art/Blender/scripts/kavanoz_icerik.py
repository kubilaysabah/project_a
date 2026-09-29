# kavanoz_icerik.py
# Mutfak tezgâhındaki kavanozların içindekiler (#83, 1. katın mutfağı; Şahan'ın seçimi 2026-09-29).
# Kavanozun kendisi kavanoz.py (gövde + kapak); Unity'de camı saydam yapılır, böylece içi görünür.
# Anahtarlı kavanoz da aynı kavanoz: içinde yalnızca anahtar var. Hepsi tornavidayla kırılabilir.
#
# PARÇALAR (hepsinin pivotu kavanozun tabanının ortası; kavanozun iç yarıçapı 0,051, boyu 0,13)
#   Tursu    : yeşilimsi salamura ve içinde dilim dilim salatalık.
#   Salca    : dörtte üçüne kadar koyu kırmızı domates salçası, üstü hafif kabarık.
#   Recel    : yarısına kadar vişne reçeli.
#   Bakliyat : ağzına kadar kuru fasulye (küçük yassı taneler).
#   Bez      : reçelin kapağının üstüne gerilmiş kareli bez ve ipi (90'ların ev reçeli).
# Ön yüz -y; Unity'de +Z.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
rnd = random.Random(1994)
salamura = ortak.malzeme("Tursu_Su", (0.35, 0.40, 0.18), puruz=0.1)
salatalik = ortak.malzeme("Tursu_Salatalik", (0.12, 0.22, 0.06), puruz=0.5)
salca = ortak.malzeme("Salca", (0.30, 0.03, 0.02), puruz=0.35)
recel = ortak.malzeme("Recel", (0.16, 0.01, 0.03), puruz=0.2)
fasulye = ortak.malzeme("Fasulye", (0.70, 0.64, 0.52), puruz=0.6)
bez_a = ortak.malzeme("Bez_Kirmizi", (0.50, 0.06, 0.05), puruz=0.9)
bez_b = ortak.malzeme("Bez_Beyaz", (0.80, 0.78, 0.72), puruz=0.9)
ip = ortak.malzeme("Bez_Ip", (0.55, 0.48, 0.34), puruz=0.9)

R = 0.049                          # içeriğin yarıçapı (camın iç yüzünden 2 mm geride)
TABAN = 0.005                      # camın dibi
sonuc = []

# Turşu: salamura silindiri ve içinde dik salatalık dilimleri.
p = [ortak.silindir("Su", R, 0.115, (0, 0, TABAN + 0.0575), salamura, kose=32)]
for i in range(7):
    a = 2 * math.pi * i / 7
    p.append(ortak.silindir("Salatalik", 0.011, 0.10, (math.cos(a) * 0.030, math.sin(a) * 0.030, TABAN + 0.052), salatalik, kose=10))
tursu = ortak.birlestir(p, "Tursu")
ortak.pivotu_tasi(tursu, (0, 0, 0))
sonuc.append(tursu)

# Salça: dörtte üç doluluk, üstünde kabarık bir tümsek.
p = [ortak.silindir("Dolgu", R, 0.095, (0, 0, TABAN + 0.0475), salca, kose=32)]
bpy.ops.mesh.primitive_uv_sphere_add(radius=R * 0.9, location=(0, 0, TABAN + 0.095), segments=24, ring_count=12)
t = bpy.context.active_object
t.scale = (1, 1, 0.18)
bpy.ops.object.transform_apply(scale=True)
ortak.ata(t, salca)
p.append(t)
s = ortak.birlestir(p, "Salca")
ortak.pivotu_tasi(s, (0, 0, 0))
sonuc.append(s)

# Reçel: yarı doluluk.
r = ortak.silindir("Recel", R, 0.065, (0, 0, TABAN + 0.0325), recel, kose=32)
ortak.pivotu_tasi(r, (0, 0, 0))
sonuc.append(r)

# Bakliyat: ağzına kadar dolu; yüzeyde rastgele yassı taneler (kuru fasulye).
p = [ortak.silindir("Dolgu", R, 0.118, (0, 0, TABAN + 0.059), fasulye, kose=32)]
for i in range(26):
    a, rr = rnd.uniform(0, 2 * math.pi), rnd.uniform(0, R - 0.008)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.006, location=(math.cos(a) * rr, math.sin(a) * rr, TABAN + 0.119), segments=8, ring_count=6)
    tane = bpy.context.active_object
    tane.scale = (1.4, 0.9, 0.6)
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(tane, fasulye)
    p.append(tane)
b = ortak.birlestir(p, "Bakliyat")
ortak.pivotu_tasi(b, (0, 0, 0))
sonuc.append(b)

# Bez: kapağın üstünde (kapak üstü ~0,158) kareli bez; kenarları kavanozun boynuna sarkar, ip bağlı.
KAPAK_UST = 0.158
p = []
for i in range(6):
    for j in range(6):
        x = -0.06 + i * 0.024 + 0.012
        y = -0.06 + j * 0.024 + 0.012
        if x * x + y * y > 0.062 ** 2:
            continue
        p.append(ortak.kutu("Kare", (0.024, 0.024, 0.002), (x, y, KAPAK_UST + 0.001), bez_a if (i + j) % 2 == 0 else bez_b))
for i in range(12):
    a = 2 * math.pi * i / 12
    p.append(ortak.kutu("Etek", (0.02, 0.004, 0.03), (math.cos(a) * 0.056, math.sin(a) * 0.056, KAPAK_UST - 0.014), bez_a if i % 2 == 0 else bez_b))
p.append(ortak.silindir("Ip", 0.052, 0.004, (0, 0, KAPAK_UST - 0.022), ip, kose=24))
bez = ortak.birlestir(p, "Bez")
ortak.pivotu_tasi(bez, (0, 0, 0))
sonuc.append(bez)

ortak.calistir_ve_bitir("Kavanoz_Icerik", sonuc, 0.16, 0.6)
