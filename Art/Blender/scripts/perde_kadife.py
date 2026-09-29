# perde_kadife.py
# 2. katta Nuri Bey'in salon penceresindeki perde (#83, Şahan'ın seçimi 2026-09-29).
# 1. kattaki yarım inmiş tülden farklı: ahşap korniş, iki yanda toplanıp kordonla bağlanmış bordo
# kadife fon perde, ortada boydan boya asılı tül (tül Unity'de yarı saydam malzemeyle değişir).
#
# PARÇALAR
#   Kornis : ahşap boru, iki ucunda topuz, taşıyıcılar ve kordon püskülleri.
#   Fon    : iki yandaki kadife perde (pilili, bele doğru toplanmış).
#   Tul    : ortadaki pilili tül.
# Pivot hepsinde ARKA düzlemin (duvar) ortası, korniş yüksekliğinde. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Kornis_Ahsap", (0.10, 0.05, 0.02), puruz=0.4)
kadife = ortak.malzeme("Fon_Kadife", (0.18, 0.02, 0.03), puruz=0.8)
kordon = ortak.malzeme("Fon_Kordon", (0.50, 0.38, 0.12), puruz=0.5)
tul = ortak.malzeme("Tul_Nuri", (0.85, 0.84, 0.80), puruz=0.9)

EN = 2.10
ARA = 0.12
BOY = 2.30                                   # kornişten yere


def pilili_yuzey(ad, x0, x1, derinlik, adim, ust_daralma, bel_z, bel_daralma, mat):
    """Zikzak pilili yüzey (from_pydata): üstte x0-x1 arası, bel hizasında merkeze doğru toplanır,
    belden aşağısı yeniden açılır. derinlik: pililerin öne-arkaya genliği."""
    n = max(2, int((x1 - x0) / adim) + 1)
    satirlar = [(0.0, 1.0), (-bel_z, bel_daralma), (-BOY, 0.85)]      # (z, genişlik oranı)
    orta = (x0 + x1) / 2
    noktalar, yuzler = [], []
    for zi, oran in satirlar:
        for i in range(n):
            t = i / (n - 1)
            x = orta + (x0 + (x1 - x0) * t - orta) * oran
            yy = -ARA + (derinlik if i % 2 == 0 else -derinlik) * (0.4 if oran < 0.5 else 1.0)
            noktalar.append((x, yy, zi - 0.03))
    for r in range(len(satirlar) - 1):
        for i in range(n - 1):
            a = r * n + i
            # Köşe sırası yüzün önünü belirler: odaya (-y) bakan yüz görünür olsun.
            yuzler.append((a, a + n, a + 1 + n, a + 1))
    m = bpy.data.meshes.new(ad)
    m.from_pydata(noktalar, [], yuzler)
    m.update()
    o = bpy.data.objects.new(ad, m)
    bpy.context.collection.objects.link(o)
    ortak.ata(o, mat)
    return o


# Korniş: x boyunca ahşap boru (eklenirken döndürülür), iki ucunda topuz, iki taşıyıcı.
bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.016, depth=EN, location=(0, -ARA, 0), rotation=(0, math.pi / 2, 0))
boru = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(boru, ahsap)
p = [boru]
for yon in (-1, 1):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.035, location=(yon * (EN / 2 + 0.03), -ARA, 0), segments=14, ring_count=8)
    ortak.ata(bpy.context.active_object, ahsap)
    p.append(bpy.context.active_object)
    p.append(ortak.kutu("Tasiyici", (0.025, ARA, 0.03), (yon * (EN / 2 - 0.12), -ARA / 2, 0), ahsap, 0.004))
    # Kordon ve püskül: fon perdenin belini saran altın kordon, ucu sarkıyor.
    bx = yon * (EN / 2 - 0.14)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.07, minor_radius=0.008, location=(bx, -ARA, -1.05), major_segments=16, minor_segments=6)
    ortak.ata(bpy.context.active_object, kordon)
    p.append(bpy.context.active_object)
    p.append(ortak.silindir("Puskul", 0.02, 0.12, (bx + yon * 0.06, -ARA - 0.06, -1.14), kordon, kose=10))
kornis = ortak.birlestir(p, "Kornis")
ortak.yumusat(kornis, 40)
ortak.pivotu_tasi(kornis, (0, 0, 0))

# Fon perdeler: iki yanda, belde (1,05 m aşağıda) toplanmış.
fonlar = []
for yon in (-1, 1):
    x_dis = yon * (EN / 2 - 0.02)
    x_ic = yon * (EN / 2 - 0.45)
    x0, x1 = (x_dis, x_ic) if yon < 0 else (x_ic, x_dis)
    fonlar.append(pilili_yuzey("FonParca", x0, x1, 0.035, 0.05, 1.0, 1.05, 0.30, kadife))
fon = ortak.birlestir(fonlar, "Fon")
ortak.pivotu_tasi(fon, (0, 0, 0))

# Tül: ortada, fonların arasında düz asılı (bel toplaması yok).
t = pilili_yuzey("Tul", -EN / 2 + 0.35, EN / 2 - 0.35, 0.02, 0.04, 1.0, 1.05, 1.0, tul)
for v in t.data.vertices:
    v.co.y += 0.02                             # fonların hemen arkasında
ortak.pivotu_tasi(t, (0, 0, 0))

ortak.calistir_ve_bitir("Perde_Kadife", [kornis, fon, t], 0.4, 3.4, onizlemede_kaldir=2.35)
