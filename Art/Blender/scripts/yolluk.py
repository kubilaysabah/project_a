# yolluk.py
# 2. katta Nuri Bey'in holündeki yolluk halı (#83, Şahan'ın seçimi 2026-09-29).
# Holün başından dibine kadar serili 9,5 x 0,8 m'lik ince halı; Nuri Bey'in düştüğü yerde (boyunun
# 4,2 m'sinde) halı buruşup iki kıvrım yapmış. Desen ayrı doku (Assets/Textures/Yolluk_Nuri.png,
# Art/Dokular/scripts/yolluk_uret.py); Unity'deki şablon onu takar. Doku 1,6 m'de bir tekrar eder.
#
# PARÇALAR
#   Yolluk : tek parça, köşe noktalarından kurulmuş ızgara (from_pydata) ve UV'si. Pivot halının
#            ortası, döşemede. Uzun kenar Blender x boyunca. Ön yüz -y.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
yun = ortak.malzeme("Yolluk_Yun", (0.8, 0.8, 0.8), puruz=0.95)       # Unity'de dokulu malzemeyle değişir
kenar = ortak.malzeme("Yolluk_Kenar", (0.08, 0.05, 0.03), puruz=0.95)

BOY, EN, KAL = 9.5, 0.8, 0.008
TEKRAR = 1.6                                     # desenin bir tekrar biriminin boyu (m)
KIVRIM_X = -BOY / 2 + 4.2                        # buruşmanın merkezi (boy boyunca)


def yukseklik(x, y):
    """Halının o noktadaki yüksekliği: düz yerde KAL; buruşmada iki dalga, bir kenara doğru sönen."""
    d = x - KIVRIM_X
    if abs(d) > 0.35:
        return KAL
    dalga = math.sin((d + 0.35) / 0.70 * 2 * math.pi) * 0.5 + 0.5    # iki tepe
    sonum = 0.5 + 0.5 * (y / (EN / 2))                                 # bir kenarda yüksek, öbüründe alçak
    return KAL + 0.055 * dalga * max(0.15, sonum) * (1 - abs(d) / 0.35)


# Izgara: boy boyunca her 5 cm, en boyunca her 10 cm bir köşe noktası (buruşma yerinde daha sık).
xs = []
x = -BOY / 2
while x < BOY / 2:
    xs.append(x)
    x += 0.02 if abs(x - KIVRIM_X) < 0.4 else 0.25
xs.append(BOY / 2)
ys = [-EN / 2 + i * EN / 8 for i in range(9)]
nokta, yuz, uv = [], [], []
for xi in xs:
    for yi in ys:
        nokta.append((xi, yi, yukseklik(xi, yi)))
n = len(ys)
for i in range(len(xs) - 1):
    for j in range(n - 1):
        a = i * n + j
        yuz.append((a, a + n, a + n + 1, a + 1))
mesh = bpy.data.meshes.new("Yolluk")
mesh.from_pydata(nokta, [], yuz)
mesh.update()
katman = mesh.uv_layers.new(name="UV")
for poligon in mesh.polygons:
    for li in poligon.loop_indices:
        v = mesh.vertices[mesh.loops[li].vertex_index].co
        katman.data[li].uv = ((v.x + BOY / 2) / TEKRAR, (v.y + EN / 2) / EN)
yolluk = bpy.data.objects.new("Yolluk", mesh)
bpy.context.collection.objects.link(yolluk)
ortak.ata(yolluk, yun)
# İki uçta ince koyu kenar (halının kalınlığı yandan).
p = [yolluk]
for yon in (-1, 1):
    p.append(ortak.kutu("Uc", (0.01, EN, KAL), (yon * (BOY / 2 - 0.005), 0, KAL / 2), kenar))
yolluk = ortak.birlestir(p, "Yolluk")
ortak.yumusat(yolluk, 50)
ortak.pivotu_tasi(yolluk, (0, 0, 0))
ortak.calistir_ve_bitir("Yolluk", [yolluk], 0.1, 5.0)
