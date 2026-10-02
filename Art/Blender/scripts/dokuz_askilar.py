# dokuz_askilar.py
# Dokuzuncu katın küçük eşyaları (#108, hikaye.md §16). Holdeki üç çividen ikisinde çıkıp gidenlerin bıraktıkları
# asılı: Rıza Efendi'nin tespihi ve Hasan'ın kasketi; üçüncü çivi boş (oyuncu Cemal'in lambasını oraya asar).
# Sofrada Gölge'nin önünde anahtarlığı durur (#105'teki şıkırtının sahibi).
#
# PARÇALAR (hepsi tek FBX'te, kendi yerlerinde; Model() parça adıyla çeker)
#   Tespih     : 33 kehribar tane, iki sıra hâlinde çividen sarkan U; altta imame ve püskül. Pivot asıldığı nokta
#                (çivinin ucu): kurucu çivi yerine koyar, tespih aşağı sarkar. Ön yüz Blender -y → Unity +Z.
#   Kasket     : yassı kumaş kasket, arka kenarından çiviye asılı; tepesi odaya bakar, siperi aşağıda. Pivot asılma
#                noktası (üst kenar).
#   Anahtarlik : halka üstünde üç anahtar ve deri bir fob, masanın üstünde yatık. Pivot tabanın ortası.
# Çalıştırma: blender -b --factory-startup --python dokuz_askilar.py
import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
# Blender malzeme renkleri doğrusal; Unity açık gösterir, koyu seçildi (CLAUDE.md).
kehribar = ortak.malzeme("Dokuz_Kehribar", (0.45, 0.17, 0.02), puruz=0.25)
ipek = ortak.malzeme("Dokuz_Puskul", (0.30, 0.04, 0.03), puruz=0.9)
yun = ortak.malzeme("Dokuz_Kasket", (0.06, 0.055, 0.045), puruz=0.95)      # gri-kahve balıksırtı yün
siper = ortak.malzeme("Dokuz_Siper", (0.035, 0.03, 0.025), puruz=0.7)
pirinc = ortak.malzeme("Dokuz_Pirinc", (0.50, 0.36, 0.12), puruz=0.35, metal=1.0)
celik = ortak.malzeme("Dokuz_Celik", (0.40, 0.41, 0.42), puruz=0.3, metal=1.0)
deri = ortak.malzeme("Dokuz_Deri", (0.09, 0.045, 0.02), puruz=0.8)


def kure(ad, yaricap, konum, mat, bolum=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=bolum * 2, ring_count=bolum, radius=yaricap, location=konum)
    n = bpy.context.active_object
    n.name = ad
    ortak.ata(n, mat)
    return n


def yassi_kure(ad, olcu, konum, mat, bolum=12):
    """Üç eksende ayrı ölçekli küre (elipsoit); ölçek meshe işlenir."""
    n = kure(ad, 0.5, konum, mat, bolum)
    n.scale = olcu
    bpy.ops.object.transform_apply(scale=True)
    return n


# ---- TESPİH: çividen sarkan U biçimi (xz düzlemi), 33 tane iki kolda + imame ----
# Asılma noktası (0, 0, 0). Kollar y = -0,004 ve +0,004'te (tane çapı 9 mm, iki sıra yan yana). U'nun dibi z = -0,15.
p = []
tane_r = 0.0045
n_tane = 33
# Tanelerin yolu: asılma noktasından sağa doğru inen bir kol, dipte dönüş, soldan yukarı çıkış. Parametre t 0..1.
def tespih_yolu(t):
    # Yarı eliptik U: x = 0,028·sin(πt) işaretli, z = -0,15·sin(πt)… basitçe: sol kol t<0,5 iner, sağ kol çıkar.
    a = t * math.pi
    x = -0.026 * math.cos(a)          # -0,026 (sol üst) → +0,026 (sağ üst)
    z = -0.15 * math.sin(a) ** 0.85   # dipte yumuşak dönüş
    return (x, 0.0, z)
for i in range(n_tane):
    t = (i + 0.5) / n_tane
    x, y, z = tespih_yolu(t)
    p.append(kure("Tane", tane_r, (x, y, z), kehribar, 8))
# İp: tanelerin içinden geçen ince silindirler yerine, kolların üstünde çiviye binen iki kısa ip ve dipte imame.
p.append(ortak.silindir("Ip_Sol", 0.0008, 0.012, (-0.026, 0, -0.004), ipek, kose=8))
p.append(ortak.silindir("Ip_Sag", 0.0008, 0.012, (0.026, 0, -0.004), ipek, kose=8))
# İmame (uzun tane) dipte, aşağı sarkar; altında püskül.
p.append(ortak.silindir("Imame", 0.004, 0.022, (0, 0, -0.163), kehribar, kose=10))
p.append(ortak.silindir("Puskul", 0.0045, 0.03, (0, 0, -0.19), ipek, kose=8))
p.append(ortak.silindir("PuskulUc", 0.006, 0.012, (0, 0, -0.21), ipek, kose=8))
tespih = ortak.birlestir(p, "Tespih")
ortak.yumusat(tespih, 40)
ortak.pivotu_tasi(tespih, (0, 0, 0))

# ---- KASKET: dik duran yassı kasket, üst kenarından asılı; tepesi -y (odaya) bakar, siper altta ----
# Asılma noktası (0, 0, 0) = arka kenarın üstü. Taç: xz düzleminde 26 cm çap, y'de 6 cm kalın elipsoit.
q = [yassi_kure("Tac", (0.26, 0.06, 0.24), (0, -0.025, -0.12), yun, 14)]
# Siper: taçın altından öne (-y) ve aşağı eğik çıkan yassı parça (ortak.kutu döndürülmez: son ölçüsüyle kutu).
q.append(ortak.kutu("Siper", (0.17, 0.065, 0.012), (0, -0.07, -0.235), siper, 0.003))
# Kafa bandı: taçın alt kenarında ince şerit.
q.append(ortak.kutu("Bant", (0.20, 0.03, 0.012), (0, -0.04, -0.225), siper))
# Tepedeki düğme.
q.append(kure("Dugme", 0.008, (0, -0.056, -0.12), yun, 8))
kasket = ortak.birlestir(q, "Kasket")
ortak.yumusat(kasket, 40)
ortak.pivotu_tasi(kasket, (0, 0, 0))

# ---- ANAHTARLIK: masada yatık; halka, üç anahtar yelpaze gibi, deri fob ----
r = []
bpy.ops.mesh.primitive_torus_add(major_radius=0.014, minor_radius=0.0015, major_segments=28, minor_segments=8,
                                 location=(0, 0, 0.0015))
halka = bpy.context.active_object
halka.name = "Halka"
ortak.ata(halka, celik)
r.append(halka)
def anahtar(ad, aci_derece, mat):
    """Halkadan dışa uzanan bir anahtar: baş (halka içinden geçen delikli disk) + sap + diş. Açı meshe işlenir."""
    a = math.radians(aci_derece)
    parcalar = [ortak.silindir("Bas", 0.009, 0.0018, (0, 0, 0.0025), mat, kose=16),
                ortak.kutu("Sap", (0.006, 0.040, 0.0016), (0, 0.028, 0.0025), mat),
                ortak.kutu("Dis1", (0.004, 0.006, 0.0016), (0.004, 0.038, 0.0025), mat),
                ortak.kutu("Dis2", (0.003, 0.005, 0.0016), (0.0035, 0.028, 0.0025), mat)]
    k = ortak.birlestir(parcalar, ad)
    # Halkanın kenarına taşı ve döndür: baş halkanın üstünde (0, 0,012), sap dışa.
    k.location = (0, 0.012, 0)
    k.rotation_euler = (0, 0, a)
    bpy.ops.object.select_all(action="DESELECT"); k.select_set(True); bpy.context.view_layer.objects.active = k
    bpy.ops.object.transform_apply(location=True, rotation=True)
    return k
r.append(anahtar("Anahtar_1", -35, pirinc))
r.append(anahtar("Anahtar_2", 0, celik))
r.append(anahtar("Anahtar_3", 30, pirinc))
# Deri fob: halkanın öbür yanında yassı dikdörtgen, bir köşesi halkaya değer.
r.append(ortak.kutu("Fob", (0.022, 0.034, 0.004), (0, -0.030, 0.002), deri, 0.0015))
anahtarlik = ortak.birlestir(r, "Anahtarlik")
ortak.yumusat(anahtarlik, 40)
ortak.pivotu_tasi(anahtarlik, (0, 0, 0))

ortak.calistir_ve_bitir("Dokuz_Askilar", [tespih, kasket, anahtarlik], 0.3, 0.6, onizlemede_kaldir=0.25)
