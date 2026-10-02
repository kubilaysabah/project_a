# sofra_1974.py
# Dokuzuncu katın sofrası (#108): 6. katın sofrasının (sofra.py) PARÇALI ikizi. Aynı masa, aynı dört sandalye,
# aynı ölçüler (tabak yuvaları Yer_1..4 kurucuda aynı yerlere konur); fark, sandalyelerin AYRI nesne olması.
# Neden: dokuzuncu katta iki sandalye geri itilmiş (kalkıp gitmişler), biri çekilmiş (oyuncunun), birinde Gölge
# oturuyor. Tek parça bir mesh'in sandalyesi oynatılamaz; her sandalyenin pivotu KENDİ tabanının ortasında.
# Model tekrarı burada bilerek var (hikaye.md §16: "tekrar hikâyenin kendisidir").
#
# PARÇALAR (FBX'te en üst düzeyde, kendi yerlerinde):
#   Masa        : tabla, ayaklar, dantel örtü, runner. Pivot tabanın ortası (0, 0, 0).
#   Sandalye_1..4: pivot sandalyenin taban ortası (x, y, 0). Numaralar tabak yuvalarıyla eşleşir
#                 (Unity Yer_i: 1 (-0,35, +0,25) 2 (+0,35, +0,25) 3 (-0,35, -0,25) 4 (+0,35, -0,25);
#                 Blender→Unity x aynalanır, y → -z: Blender (x, y) = (-Ux, -Uz)).
# Çalıştırma: blender -b --factory-startup --python sofra_1974.py
import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Sofra74_Ceviz", (0.05, 0.022, 0.008), puruz=0.3)
dantel = ortak.malzeme("Sofra74_Dantel", (0.86, 0.82, 0.72), puruz=0.9)
runner = ortak.malzeme("Sofra74_Runner", (0.55, 0.12, 0.10), puruz=0.8)
kumas = ortak.malzeme("Sofra74_Kumas", (0.30, 0.05, 0.06), puruz=0.95)

EN, DERIN, BOY = 1.40, 0.90, 0.75
SARKMA = 0.12

# ---- MASA ----
p = [ortak.kutu("Tabla", (EN, DERIN, 0.035), (0, 0, BOY - 0.0175), ceviz, 0.006),
     ortak.kutu("Etek", (EN - 0.10, DERIN - 0.10, 0.08), (0, 0, BOY - 0.035 - 0.04), ceviz, 0.004)]
for x in (-EN / 2 + 0.07, EN / 2 - 0.07):
    for y in (-DERIN / 2 + 0.07, DERIN / 2 - 0.07):
        h = BOY - 0.035
        p.append(ortak.kutu("AyakUst", (0.06, 0.06, 0.12), (x, y, h - 0.06), ceviz, 0.004))
        p.append(ortak.silindir("AyakGovde", 0.022, h - 0.20, (x, y, 0.06 + (h - 0.20) / 2), ceviz, kose=16))
        for z, r in ((0.10, 0.032), (h * 0.55, 0.030), (h - 0.14, 0.028)):
            p.append(ortak.silindir("Bilezik", r, 0.035, (x, y, z), ceviz, kose=16))
        p.append(ortak.silindir("AyakUc", 0.026, 0.06, (x, y, 0.03), ceviz, kose=16))
p.append(ortak.kutu("Ortu", (EN + 0.02, DERIN + 0.02, 0.004), (0, 0, BOY + 0.002), dantel))
for yon in (-1, 1):
    p.append(ortak.kutu("OrtuUzun", (EN + 0.02, 0.004, SARKMA), (0, yon * (DERIN / 2 + 0.012), BOY - SARKMA / 2), dantel))
    p.append(ortak.kutu("OrtuKisa", (0.004, DERIN + 0.02, SARKMA), (yon * (EN / 2 + 0.012), 0, BOY - SARKMA / 2), dantel))
fisto_z = BOY - SARKMA


def dik_disk(x, y, eksen_y):
    rot = (math.pi / 2, 0, 0) if eksen_y else (0, math.pi / 2, 0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.03, depth=0.004, location=(x, y, fisto_z), rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, dantel)
    return n


for i in range(22):
    x = -EN / 2 + (i + 0.5) * EN / 22
    for yon in (-1, 1):
        p.append(dik_disk(x, yon * (DERIN / 2 + 0.012), True))
for i in range(14):
    y = -DERIN / 2 + (i + 0.5) * DERIN / 14
    for yon in (-1, 1):
        p.append(dik_disk(yon * (EN / 2 + 0.012), y, False))
p.append(ortak.kutu("Runner", (EN - 0.30, 0.30, 0.003), (0, 0, BOY + 0.0055), runner))
for yon in (-1, 1):
    p.append(ortak.kutu("Sirma", (EN - 0.30, 0.012, 0.0035), (0, yon * 0.14, BOY + 0.006), dantel))
masa = ortak.birlestir(p, "Masa")
ortak.yumusat(masa, 30)
ortak.pivotu_tasi(masa, (0, 0, 0))


# ---- SANDALYELER (her biri ayrı nesne) ----
def sandalye(ad, x, yon):
    """x: masa boyunca; yon: -1 ön kenar (-y), +1 arka kenar (+y). Sırtı masadan uzakta, önü masaya bakar."""
    y = yon * 0.62
    q = [ortak.kutu("Oturak", (0.44, 0.42, 0.045), (x, y, 0.45), ceviz, 0.008),
         ortak.kutu("Minder", (0.40, 0.38, 0.05), (x, y, 0.495), kumas, 0.015)]
    sy = y + yon * 0.19
    for dx in (-0.19, 0.19):
        q.append(ortak.kutu("ArkaAyak", (0.035, 0.035, 0.95), (x + dx, sy, 0.475), ceviz, 0.004))
        q.append(ortak.silindir("OnAyak", 0.017, 0.43, (x + dx, y - yon * 0.18, 0.215), ceviz, kose=12))
        q.append(ortak.silindir("OnBilezik", 0.024, 0.03, (x + dx, y - yon * 0.18, 0.30), ceviz, kose=12))
    q.append(ortak.kutu("SirtTepe", (0.42, 0.035, 0.07), (x, sy, 0.90), ceviz, 0.01))
    q.append(ortak.kutu("SirtTepe2", (0.22, 0.035, 0.03), (x, sy, 0.945), ceviz, 0.008))
    q.append(ortak.kutu("SirtAlt", (0.40, 0.03, 0.035), (x, sy, 0.56), ceviz, 0.004))
    for i in range(5):
        q.append(ortak.silindir("Cubuk", 0.009, 0.30, (x - 0.14 + i * 0.07, sy, 0.715), ceviz, kose=10))
    q.append(ortak.kutu("Kusak", (0.38, 0.02, 0.02), (x, y, 0.14), ceviz))
    s = ortak.birlestir(q, ad)
    ortak.yumusat(s, 30)
    ortak.pivotu_tasi(s, (x, y, 0))   # pivot kendi tabanının ortası: itilince / döndürülünce yerinde döner
    return s


# Unity Yer_i ile eşleşme (üstteki not): 1 → Blender (+0,35, -0,62), 2 → (-0,35, -0,62), 3 → (+0,35, +0,62), 4 → (-0,35, +0,62)
sandalyeler = [sandalye("Sandalye_1", 0.35, -1), sandalye("Sandalye_2", -0.35, -1),
               sandalye("Sandalye_3", 0.35, 1), sandalye("Sandalye_4", -0.35, 1)]
ortak.calistir_ve_bitir("Sofra_1974", [masa] + sandalyeler, 1.2, 3.2)
