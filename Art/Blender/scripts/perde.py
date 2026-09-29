# perde.py
# Salon penceresinin yarısı indirilmiş tül perdesi (#83, 1. katın salonu; Şahan'ın seçimi 2026-09-29).
# Taşınan aile perdeleri indirmeye başlamış: korniş bir ucundan sökülüp yamulmuş, sol yarıda tül hâlâ
# asılı (pilili, yere kadar), sağ yarıda yalnızca boş halkalar kalmış.
#
# PARÇALAR
#   Kornis : korniş borusu (sağ ucu 6 cm aşağı düşmüş), iki taşıyıcı ve boş halkalar.
#   Tul    : sol yarıdaki pilili tül. Unity'deki şablon ona yarı saydam malzeme takar.
# Pivot ikisinde de ARKA düzlemin (duvarın) ortası, korniş yüksekliğinde. Ön yüz -y; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
pirinc = ortak.malzeme("Kornis_Pirinc", (0.45, 0.34, 0.16), puruz=0.35, metal=0.7)
tul = ortak.malzeme("Tul", (0.85, 0.84, 0.80), puruz=0.9)

EN = 1.90                          # korniş boyu (pencere 1,40 + iki yanda 0,25)
DUS = 0.06                         # sağ ucun düştüğü yükseklik
ARA = 0.10                         # kornişin duvardan uzaklığı
EGIM = math.atan2(DUS, EN)

# Korniş borusu: y ekseni etrafında hafif eğik silindir (eklenirken döndürülür, dönüş meshe işlenir).
bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.012, depth=EN, location=(0, -ARA, -DUS / 2),
                                    rotation=(0, math.pi / 2 + EGIM, 0))
boru = bpy.context.active_object
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(boru, pirinc)
p = [boru]
# Taşıyıcılar: sol uçta duvara sağlam, sağ uçta sökülmüş (yalnızca duvardaki dübeli kalmış).
p.append(ortak.kutu("TasiyiciSol", (0.02, ARA, 0.02), (-EN / 2 + 0.10, -ARA / 2, 0.0), pirinc, 0.003))
p.append(ortak.kutu("DubelSag", (0.012, 0.01, 0.012), (EN / 2 - 0.10, -0.005, 0.0), pirinc))
# Boş halkalar: sağ yarıda borunun üstünde dizili.
for i in range(6):
    x = 0.10 + i * 0.13
    z = -DUS * (x + EN / 2) / EN
    bpy.ops.mesh.primitive_torus_add(major_radius=0.02, minor_radius=0.003, location=(x, -ARA, z - 0.012),
                                     rotation=(0, math.pi / 2, 0), major_segments=16, minor_segments=6)
    h = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(h, pirinc)
    p.append(h)
kornis = ortak.birlestir(p, "Kornis")
ortak.yumusat(kornis, 40)
ortak.pivotu_tasi(kornis, (0, 0, 0))

# Tül: sol yarıda, kornişten yere kadar (2,30 m). Pili zikzak bir yüzey: her 5 cm'de öne-arkaya kırılır.
# Mesh köşe noktalarından doğrudan kurulur (from_pydata): üst ve alt sıra, aralarında dörtgenler.
TUL_EN, TUL_BOY = 0.95, 2.28
x_bas = -EN / 2 + 0.05
adim = 0.05
noktalar, yuzler = [], []
n = int(TUL_EN / adim) + 1
for i in range(n):
    x = x_bas + i * adim
    yy = -ARA + (0.025 if i % 2 == 0 else -0.025)
    z_ust = -DUS * (x + EN / 2) / EN - 0.02
    noktalar.append((x, yy, z_ust))
    noktalar.append((x + (0.01 if i % 3 == 0 else -0.01), yy - 0.01, z_ust - TUL_BOY))   # altta hafif dağınık
for i in range(n - 1):
    a, b = 2 * i, 2 * (i + 1)
    yuzler.append((a, b, b + 1, a + 1))
mesh = bpy.data.meshes.new("Tul")
mesh.from_pydata(noktalar, [], yuzler)
mesh.update()
tul_n = bpy.data.objects.new("Tul", mesh)
bpy.context.collection.objects.link(tul_n)
ortak.ata(tul_n, tul)
ortak.pivotu_tasi(tul_n, (0, 0, 0))

ortak.calistir_ve_bitir("Perde", [kornis, tul_n], 0.5, 3.4, onizlemede_kaldir=2.35)
