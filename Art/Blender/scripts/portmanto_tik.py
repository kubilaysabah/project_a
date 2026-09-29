# portmanto_tik.py
# 2. katta Nuri Bey'in antresindeki 70'ler portmantosu (#83, Şahan'ın seçimi 2026-09-29).
# Katlarda aynı model kullanılmaz (Şahan): 1. kattaki koyu ceviz portmantodan farklı olarak açık renk
# tik ağacı, duvara asılı dikey çıtalı bir pano, ortasında oval ayna, tepesinde çubuklu şapka rafı,
# altta sivri ayaklı alçak bir ayakkabı dolabı (iki sürgülü kapak). Üstünde Nuri Bey'in hastaneye
# giderken alamadığı koyu gri yün paltosu ve fötr şapkası; dolaba dayalı bastonu.
#
# PARÇALAR
#   Portmanto_Tik : tek parça (dolap, pano, ayna, askılar, palto, şapka, baston). Pivot tabanın
#                   ortası, arka yüz (duvar) y = +0,17. Ön yüz -y; Unity'de +Z.
# Eğik ve yuvarlak parçalar eklenirken döndürülür, dönüş meshe işlenir (ortak.kutu tuzağı).

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy
import mathutils

ortak.sahneyi_temizle()
tik = ortak.malzeme("Tik", (0.28, 0.13, 0.05), puruz=0.45)                 # açık tik (doğrusal renk)
tik_koyu = ortak.malzeme("Tik_Koyu", (0.14, 0.06, 0.02), puruz=0.5)
ayna = ortak.malzeme("Tik_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.4)
pirinc = ortak.malzeme("Tik_Pirinc", (0.52, 0.40, 0.20), puruz=0.35, metal=0.6)
yun = ortak.malzeme("Palto_Yun", (0.07, 0.07, 0.075), puruz=0.95)         # koyu gri yün palto
astar = ortak.malzeme("Palto_Astar", (0.20, 0.12, 0.08), puruz=0.8)
kece = ortak.malzeme("Fotr_Kece", (0.10, 0.08, 0.06), puruz=0.9)          # kahverengi fötr
kurdele = ortak.malzeme("Fotr_Kurdele", (0.02, 0.02, 0.02), puruz=0.6)
baston_m = ortak.malzeme("Baston", (0.10, 0.045, 0.015), puruz=0.3)

p = []


def dondurulmus_kutu(ad, olcu, konum, rot, mat, pah=0.0):
    """Döndürülmüş kutu: küp eklenirken döndürülür, ölçeklenir, dönüş ve ölçek meshe işlenir."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=konum, rotation=rot)
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    if pah > 0:
        ortak.pah(n, pah)
    ortak.ata(n, mat)
    p.append(n)
    return n


ARKA = 0.17
# ---------- AYAKKABI DOLABI (alçak, sivri ayaklı) ----------
DOLAP_EN, DOLAP_DER, DOLAP_YUK, AYAK = 0.90, 0.32, 0.36, 0.14
dy = ARKA - DOLAP_DER / 2
p.append(ortak.kutu("Dolap", (DOLAP_EN, DOLAP_DER, DOLAP_YUK), (0, dy, AYAK + DOLAP_YUK / 2), tik, 0.006))
for yon in (-1, 1):
    # Sürgülü kapaklar: önde üst üste binen iki pano, ortalarında oval tutamak oyuğu (koyu).
    p.append(ortak.kutu("Kapak", (DOLAP_EN / 2 + 0.02, 0.012, DOLAP_YUK - 0.04), (yon * 0.21, dy - DOLAP_DER / 2 - 0.006 - (0.012 if yon > 0 else 0), AYAK + DOLAP_YUK / 2), tik, 0.003))
    p.append(ortak.kutu("Tutamak", (0.05, 0.004, 0.02), (yon * 0.34, dy - DOLAP_DER / 2 - 0.02, AYAK + DOLAP_YUK * 0.6), tik_koyu))
    # Sivri, hafif dışa açılı ayaklar (70'ler).
    for yy in (dy - DOLAP_DER / 2 + 0.05, dy + DOLAP_DER / 2 - 0.05):
        dondurulmus_kutu("Ayak", (0.03, 0.03, AYAK + 0.01), (yon * (DOLAP_EN / 2 - 0.08) + yon * 0.012, yy, AYAK / 2), (0, yon * math.radians(8), 0), tik_koyu)
# ---------- PANO (dikey çıtalı) ----------
PANO_EN, PANO_ALT, PANO_UST = 0.80, 0.62, 1.80
for i in range(9):
    x = -PANO_EN / 2 + (i + 0.5) * PANO_EN / 9
    p.append(ortak.kutu("Cita", (PANO_EN / 9 - 0.008, 0.02, PANO_UST - PANO_ALT), (x, ARKA - 0.01, (PANO_ALT + PANO_UST) / 2), tik if i % 2 == 0 else tik_koyu, 0.002))
p.append(ortak.kutu("UstKusak", (PANO_EN + 0.04, 0.03, 0.05), (0, ARKA - 0.015, PANO_UST + 0.025), tik_koyu, 0.004))
# Şapka rafı: pano tepesinde öne uzanan iki çubuk ve arasında ince çıtalar.
for x in (-PANO_EN / 2 + 0.04, PANO_EN / 2 - 0.04):
    p.append(ortak.kutu("RafKol", (0.025, 0.24, 0.025), (x, ARKA - 0.12, PANO_UST - 0.04), tik_koyu, 0.004))
for i in range(5):
    p.append(ortak.kutu("RafCubuk", (PANO_EN - 0.06, 0.012, 0.012), (0, ARKA - 0.03 - i * 0.05, PANO_UST - 0.03), tik, 0.002))
# Oval ayna: panonun ortasında; silindir x ekseni etrafında döndürülüp y'ye bakan yassı elips yapılır.
bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.5, depth=0.01, location=(0, ARKA - 0.025, 1.25), rotation=(math.pi / 2, 0, 0))
a = bpy.context.active_object
a.scale = (0.38, 0.62, 1)                                     # yerel eksende: en 0,38, boy 0,62 (dönüşten önce)
bpy.ops.object.transform_apply(rotation=True, scale=True)
ortak.ata(a, ayna)
p.append(a)
bpy.ops.mesh.primitive_torus_add(major_radius=0.5, minor_radius=0.03, location=(0, ARKA - 0.028, 1.25), rotation=(math.pi / 2, 0, 0), major_segments=40, minor_segments=8)
c = bpy.context.active_object
c.scale = (0.40, 0.64, 1)
bpy.ops.object.transform_apply(rotation=True, scale=True)
ortak.ata(c, tik_koyu)
p.append(c)
# Askılar: aynanın iki yanında pirinç topuzlu kancalar.
for x in (-0.32, 0.32):
    p.append(ortak.kutu("Kanca", (0.014, 0.07, 0.014), (x, ARKA - 0.055, 1.55), pirinc, 0.003))
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.016, location=(x, ARKA - 0.095, 1.55), segments=12, ring_count=8)
    t = bpy.context.active_object
    ortak.ata(t, pirinc)
    p.append(t)

# ---------- PALTO (sağ kancada asılı) ----------
px = 0.32
p.append(ortak.kutu("Omuz", (0.36, 0.14, 0.10), (px, ARKA - 0.12, 1.46), yun, 0.04))
for i, (en, z) in enumerate(((0.40, 1.28), (0.42, 1.08), (0.44, 0.90), (0.46, 0.74))):
    p.append(ortak.kutu("Govde", (en, 0.12 + i * 0.01, 0.20), (px, ARKA - 0.12, z), yun, 0.04))
p.append(ortak.kutu("Yaka", (0.14, 0.03, 0.14), (px, ARKA - 0.19, 1.40), yun, 0.02))
p.append(ortak.kutu("Astar", (0.10, 0.004, 0.25), (px, ARKA - 0.186, 1.26), astar))
for yon in (-1, 1):
    p.append(ortak.kutu("Kol", (0.09, 0.11, 0.62), (px + yon * 0.21, ARKA - 0.12, 1.14), yun, 0.035))
for z in (1.28, 1.12, 0.96):
    p.append(ortak.silindir("Dugme", 0.011, 0.006, (px + 0.04, ARKA - 0.186, z), kurdele, kose=10))

# ---------- FÖTR ŞAPKA (rafın üstünde) ----------
sx = -0.22
p.append(ortak.silindir("Siper", 0.16, 0.008, (sx, ARKA - 0.12, PANO_UST - 0.016), kece, kose=32))
p.append(ortak.silindir("Tepe", 0.095, 0.10, (sx, ARKA - 0.12, PANO_UST + 0.04), kece, kose=32, pah_genislik=0.02))
p.append(ortak.silindir("Kurdele", 0.097, 0.022, (sx, ARKA - 0.12, PANO_UST + 0.005), kurdele, kose=32))

# ---------- BASTON (dolabın sol yanına dayalı) ----------
bx = -DOLAP_EN / 2 - 0.05
dondurulmus_kutu("BastonGovde", (0.022, 0.022, 0.88), (bx - 0.05, ARKA - 0.14, 0.44), (0, math.radians(-7), 0), baston_m, 0.008)
bpy.ops.mesh.primitive_torus_add(major_radius=0.05, minor_radius=0.012, location=(bx - 0.005, ARKA - 0.14, 0.875), rotation=(math.pi / 2, 0, 0), major_segments=20, minor_segments=8)
k = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
# Kıvrık sapın yalnızca üst yarısı kalsın: alt yarıdaki köşe noktaları silinir.
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="DESELECT")
bpy.ops.object.mode_set(mode="OBJECT")
for v in k.data.vertices:
    v.select = v.co.z < -0.005
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.delete(type="VERT")
bpy.ops.object.mode_set(mode="OBJECT")
ortak.ata(k, baston_m)
p.append(k)

portmanto = ortak.birlestir(p, "Portmanto_Tik")
ortak.yumusat(portmanto, 30)
ortak.pivotu_tasi(portmanto, (0, 0, 0))
ortak.calistir_ve_bitir("Portmanto_Tik", [portmanto], 1.9, 3.4)
