# anahtar_askiligi.py
# Antre duvarındaki tahta anahtar askılığı (#83, 1. katın antresi).
# 90'ların evinde kapının hemen yanında asılı küçük tahta pano: beş pirinç kanca, altında sallanan
# küçük bir nazar boncuğu. 1. kattaki taşınan aile anahtarları kapıcıya teslim edecek (tahliye yazısı);
# kancalar boş, yalnızca birinde anahtarı sökülmüş boş bir halka kalmış.
#
# PARÇALAR
#   Anahtar_Askiligi : tek parça. Pivot panonun ARKA yüzünün ortası (duvara değen nokta).
# Ön yüz -y tarafında; Unity'de +Z. Duvar eşyası: merkez yüksekliğiyle duvarın iç yüzüne konur.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
cam = ortak.malzeme("Askilik_Cam", (0.72, 0.62, 0.42), puruz=0.6)          # verniği sararmış çam
cam_koyu = ortak.malzeme("Askilik_Cam_Koyu", (0.30, 0.18, 0.08), puruz=0.6)
pirinc = ortak.malzeme("Askilik_Pirinc", (0.52, 0.40, 0.20), puruz=0.35, metal=0.6)
lacivert = ortak.malzeme("Nazar_Lacivert", (0.01, 0.03, 0.30), puruz=0.1)
beyaz = ortak.malzeme("Nazar_Beyaz", (0.85, 0.85, 0.82), puruz=0.1)
acik_mavi = ortak.malzeme("Nazar_Acik_Mavi", (0.20, 0.55, 0.85), puruz=0.1)
siyah = ortak.malzeme("Nazar_Siyah", (0.01, 0.01, 0.01), puruz=0.1)

EN, BOY, KAL = 0.32, 0.12, 0.018
p = []
p.append(ortak.kutu("Pano", (EN, KAL, BOY), (0, -KAL / 2, 0), cam, 0.004))
# Üst ve alt kenarda koyu çıta: basit bir çerçeve.
for dz in (-1, 1):
    p.append(ortak.kutu("Cita", (EN, KAL + 0.004, 0.012), (0, -KAL / 2 - 0.002, dz * (BOY / 2 - 0.006)), cam_koyu, 0.002))
# Beş kanca: panonun alt yarısında; gövde öne çıkar, ucu yukarı kıvrılır.
KANCA_Z = -0.02
kanca_x = [-0.12, -0.06, 0.0, 0.06, 0.12]
for x in kanca_x:
    p.append(ortak.kutu("KancaGovde", (0.008, 0.035, 0.008), (x, -KAL - 0.0175, KANCA_Z), pirinc, 0.002))
    p.append(ortak.kutu("KancaUc", (0.008, 0.008, 0.02), (x, -KAL - 0.031, KANCA_Z + 0.008), pirinc, 0.002))


def dik_disk(ad, yaricap, kalinlik, konum, mat, kose=24):
    """Ön yüzü -y'ye bakan ince disk: silindir eklenirken döndürülür ve dönüş meshe işlenir
    (ortak.kutu tuzağı yok, bkz. sofra.py)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=yaricap, depth=kalinlik, location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return n


# Boş anahtarlık halkası: ikinci kancada asılı, düzlemi duvara paralel (dik simit).
bpy.ops.mesh.primitive_torus_add(major_radius=0.014, minor_radius=0.0018, location=(-0.06, -KAL - 0.028, KANCA_Z - 0.014),
                                 rotation=(math.pi / 2, 0, 0), major_segments=24, minor_segments=8)
halka = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(halka, pirinc)
p.append(halka)

# Nazar boncuğu: panonun alt ortasından bir iple sallanır (dört katmanlı cam disk).
ip_ust = -BOY / 2
p.append(ortak.kutu("Ip", (0.002, 0.002, 0.04), (0, -KAL - 0.004, ip_ust - 0.02), cam_koyu))
nz = ip_ust - 0.04 - 0.02
ny = -KAL - 0.006
p.append(dik_disk("NazarLacivert", 0.02, 0.006, (0, ny, nz), lacivert))
p.append(dik_disk("NazarBeyaz", 0.013, 0.007, (0, ny - 0.0008, nz), beyaz))
p.append(dik_disk("NazarMavi", 0.009, 0.008, (0, ny - 0.0016, nz), acik_mavi))
p.append(dik_disk("NazarSiyah", 0.0045, 0.009, (0, ny - 0.0024, nz), siyah, kose=16))

askilik = ortak.birlestir(p, "Anahtar_Askiligi")
ortak.yumusat(askilik, 30)
ortak.pivotu_tasi(askilik, (0, 0, 0))
ortak.calistir_ve_bitir("Anahtar_Askiligi", [askilik], 0.25, 0.7)
