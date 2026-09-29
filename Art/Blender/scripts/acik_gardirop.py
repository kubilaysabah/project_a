# acik_gardirop.py
# Kapakları ardına kadar açık, boşaltılmış aynalı ceviz gardırop (#83, 1. katın Oda A'sı; 2026-09-29).
# gardirop.py'deki gardırobun aynısı (1,00 x 0,55 x 1,90, ceviz, aynalı kapaklar), ama içi oyuk: askı
# borusunda yalnızca dört tel askı sallanıyor. Kapaklar 90° açık: açık bir kapak, gövdenin ön köşesinde
# duvara dik duran bir levhadır; döndürme gerekmez (ortak.kutu tuzağı). Aynalar kapağın dış yüzünde.
#
# PARÇALAR
#   Acik_Gardirop : tek parça. Pivot tabanın ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy
import mathutils

ortak.sahneyi_temizle()
ceviz = ortak.malzeme("Gardirop_Ceviz", (0.05, 0.022, 0.008), puruz=0.35)
ceviz_koyu = ortak.malzeme("Gardirop_Ceviz_Koyu", (0.03, 0.013, 0.005), puruz=0.45)
ic_mat = ortak.malzeme("Gardirop_Ic", (0.30, 0.22, 0.14), puruz=0.7)          # içi açık renk sunta
ayna = ortak.malzeme("Gardirop_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.4)
pirinc = ortak.malzeme("Gardirop_Pirinc", (0.52, 0.40, 0.20), puruz=0.35, metal=0.6)
tel = ortak.malzeme("Aski_Tel", (0.45, 0.45, 0.45), puruz=0.35, metal=0.9)

EN, DERIN, BOY = 1.00, 0.55, 1.90
KAIDE, TAC = 0.08, 0.09
ON = -DERIN / 2
p = []
p.append(ortak.kutu("Kaide", (EN - 0.04, DERIN - 0.04, KAIDE), (0, 0.01, KAIDE / 2), ceviz_koyu, 0.004))
govde_boy = BOY - KAIDE - TAC
govde = ortak.kutu("Govde", (EN, DERIN, govde_boy), (0, 0, KAIDE + govde_boy / 2), ceviz, 0.006)
# İç oyuk: önü açık; yanlarda, arkada, altta ve üstte 2 cm duvar kalır.
ortak.oy(govde, ortak.kutu("Oyuk", (EN - 0.04, DERIN, govde_boy - 0.04), (0, -0.02, KAIDE + govde_boy / 2), ceviz))
p.append(govde)
p.append(ortak.kutu("IcArka", (EN - 0.045, 0.005, govde_boy - 0.045), (0, DERIN / 2 - 0.023, KAIDE + govde_boy / 2), ic_mat))
p.append(ortak.kutu("Tac1", (EN + 0.03, DERIN + 0.02, 0.04), (0, -0.01, BOY - TAC + 0.02), ceviz, 0.004))
p.append(ortak.kutu("Tac2", (EN + 0.07, DERIN + 0.04, 0.05), (0, -0.02, BOY - 0.025), ceviz, 0.006))
# Üstte bir raf ve altında askı borusu (x boyunca silindir: eklenirken döndürülür, dönüş meshe işlenir).
p.append(ortak.kutu("Raf", (EN - 0.045, DERIN - 0.05, 0.018), (0, 0.0, 1.62), ic_mat))
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.012, depth=EN - 0.05, location=(0, 0.0, 1.52), rotation=(0, math.pi / 2, 0))
boru = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(boru, pirinc)
p.append(boru)

# Açık kapaklar: her biri gövdenin ön köşesinde, dışa dönük aynası ve kulbuyla.
KAPI_ALT, KAPI_UST = KAIDE + 0.03, BOY - TAC - 0.03
kapi_boy = KAPI_UST - KAPI_ALT
kapi_en = EN / 2 - 0.03
for yon in (-1, 1):
    x = yon * (EN / 2 + 0.012)
    yk = ON - kapi_en / 2 - 0.005
    p.append(ortak.kutu("Kanat", (0.02, kapi_en, kapi_boy), (x, yk, KAPI_ALT + kapi_boy / 2), ceviz, 0.004))
    ayna_alt = KAPI_ALT + kapi_boy * 0.36
    ayna_ust = KAPI_UST - 0.08
    p.append(ortak.kutu("Ayna", (0.004, kapi_en - 0.14, ayna_ust - ayna_alt), (x + yon * 0.012, yk, (ayna_alt + ayna_ust) / 2), ayna))
    p.append(ortak.kutu("Gobek", (0.008, kapi_en - 0.16, ayna_alt - KAPI_ALT - 0.16), (x + yon * 0.013, yk, (KAPI_ALT + 0.08 + ayna_alt - 0.08) / 2), ceviz, 0.003))
    # Kulp: kapağın serbest ucunda (odaya bakan kenar), iç yüzde.
    p.append(ortak.kutu("Kulp", (0.03, 0.012, 0.16), (x - yon * 0.025, ON - kapi_en + 0.04, 1.02), pirinc, 0.004))


def cubuk(a, b, r, mat):
    """İki nokta arasında ince silindir (tel askının parçaları)."""
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=r, depth=d.length, location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)


# Tel askılar: boruya asılı dört askı, düzlemleri boruya dik (y-z düzlemi); biri hafif yana kaymış.
for i, x in enumerate((-0.30, -0.12, 0.14, 0.33)):
    kz, yw, alt = 1.52, 0.19, 1.34
    ky = 0.0 + (0.02 if i == 2 else 0.0)
    cubuk((x, ky, kz + 0.02), (x, ky, kz - 0.03), 0.003, tel)                  # kanca (düz basitleştirilmiş)
    cubuk((x, ky, kz - 0.03), (x, ky - yw, alt), 0.0025, tel)                  # sol omuz
    cubuk((x, ky, kz - 0.03), (x, ky + yw, alt), 0.0025, tel)                  # sağ omuz
    cubuk((x, ky - yw, alt), (x, ky + yw, alt), 0.0025, tel)                   # alt çubuk

gardirop = ortak.birlestir(p, "Acik_Gardirop")
ortak.yumusat(gardirop, 30)
ortak.pivotu_tasi(gardirop, (0, 0, 0))
ortak.calistir_ve_bitir("Acik_Gardirop", [gardirop], 1.9, 3.6)
