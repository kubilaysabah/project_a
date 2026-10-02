# dokuz_askilar.py
# Dokuzuncu katın küçük eşyaları (#108, #109; hikaye.md §16). Holdeki üç çividen ikisinde çıkıp gidenlerin
# bıraktıkları asılı: Rıza Efendi'nin tespihi ve Hasan'ın kasketi; üçüncü çivi boş (oyuncu Cemal'in lambasını
# oraya asar). Sofrada Gölge'nin önünde anahtarlığı durur (#105'teki şıkırtının sahibi).
#
# PARÇALAR (hepsi tek FBX'te, kendi yerlerinde; Model() parça adıyla çeker)
#   Tespih     : 33 kehribar tane iki kol hâlinde çividen sarkar; kollar hafif burulur ve dipte birleşir, altta
#                düz bir ara tane (dural), imame ve püskül. Pivot asıldığı nokta (çivinin ucu). Ön yüz -y → +Z.
#   Kasket     : yün kasket, arka kenarından çiviye asılı; tepesi odaya bakar, siperi aşağıda öne eğik. Tepe
#                düğmesi, sekiz dilim dikişi, dikiş halkası, kafa bandı. Pivot asılma noktası (üst kenar).
#   Anahtarlik : halka üstünde üç anahtar ve deri bir fob, masanın üstünde yatık. Pivot tabanın ortası.
# #109 (Şahan): ilk tespih düz bir U, kasket yassı bir diskti; 1 m'den tanınmıyordu. Bu sürüm daha dolu.
# Çalıştırma: blender -b --factory-startup --python dokuz_askilar.py
import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
# Blender malzeme renkleri doğrusal; Unity açık gösterir, koyu seçildi (CLAUDE.md).
kehribar = ortak.malzeme("Dokuz_Kehribar", (0.52, 0.19, 0.03), puruz=0.22)
ipek = ortak.malzeme("Dokuz_Puskul", (0.30, 0.04, 0.03), puruz=0.9)
yun = ortak.malzeme("Dokuz_Kasket", (0.045, 0.040, 0.034), puruz=0.95)      # koyu gri-kahve yün
dikis = ortak.malzeme("Dokuz_Dikis", (0.02, 0.018, 0.015), puruz=0.9)        # dikişler biraz daha koyu
siper = ortak.malzeme("Dokuz_Siper", (0.03, 0.026, 0.022), puruz=0.6)
pirinc = ortak.malzeme("Dokuz_Pirinc", (0.50, 0.36, 0.12), puruz=0.35, metal=1.0)
celik = ortak.malzeme("Dokuz_Celik", (0.40, 0.41, 0.42), puruz=0.3, metal=1.0)
deri = ortak.malzeme("Dokuz_Deri", (0.09, 0.045, 0.02), puruz=0.8)


def kure(ad, yaricap, konum, mat, bolum=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=bolum * 2, ring_count=bolum, radius=yaricap, location=konum)
    n = bpy.context.active_object
    n.name = ad
    ortak.ata(n, mat)
    return n


def yassi_kure(ad, olcu, konum, mat, bolum=12, rot=(0, 0, 0)):
    """Üç eksende ayrı ölçekli küre (elipsoit); ölçek ve dönüş meshe işlenir (ortak.kutu tuzağı yok)."""
    n = kure(ad, 0.5, konum, mat, bolum)
    n.scale = olcu
    n.rotation_euler = rot
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    return n


def donmus_kutu(ad, olcu, konum, rot, mat):
    """Döndürülmüş kutu: ekleme anında döndürülür, dönüş meshe işlenir (kutu pivotu dünya merkezinde kalır)."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=konum, rotation=rot)
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    ortak.ata(n, mat)
    return n


def halka(ad, buyuk, kucuk, konum, mat, rot=(0, 0, 0), bolum=28):
    bpy.ops.mesh.primitive_torus_add(major_radius=buyuk, minor_radius=kucuk, major_segments=bolum, minor_segments=8,
                                     location=konum, rotation=rot)
    n = bpy.context.active_object
    n.name = ad
    ortak.ata(n, mat)
    return n


# ---- TESPİH ----
# Asılma noktası (0, 0, 0). İki kol çivinin iki yanından sarkar (x = ±0,012), aşağı indikçe hafifçe açılıp
# yeniden kapanır (U'nun karnı) ve z = -0,15'te birleşir; kollar derinlikte (y) birbirine karşı burulur.
p = []
tane_r = 0.0055
n_kol = 16                   # kol başına tane; 2 x 16 + tepedeki 1 = 33
for kol in (-1, 1):
    for i in range(n_kol):
        t = (i + 0.5) / n_kol                      # 0 üst, 1 dip
        z = -0.014 - t * 0.132
        acilma = 0.012 + 0.010 * math.sin(math.pi * t)       # ortada en geniş
        x = kol * acilma * (1.0 - 0.75 * t ** 3)             # dipte kapanır
        y = kol * 0.0035 * math.sin(2.5 * math.pi * t)       # burulma: kollar öne arkaya salınır
        p.append(kure("Tane", tane_r, (x, y, z), kehribar, 8))
# Tepedeki tane: çivinin üstünde, iki kolu birleştirir.
p.append(kure("Tane_Tepe", tane_r, (0, 0, 0.004), kehribar, 8))
# İp: kollar boyunca ince silindirler yerine yalnızca dipte birleşme ve dural.
p.append(ortak.silindir("Dural", 0.0065, 0.0025, (0, 0, -0.150), kehribar, kose=12))
p.append(ortak.silindir("Imame", 0.0042, 0.026, (0, 0, -0.165), kehribar, kose=10))
p.append(kure("Imame_Uc", 0.0045, (0, 0, -0.179), kehribar, 8))
# Püskül: küçük bir halka altında altı ince iplik, hafifçe yelpaze.
p.append(ortak.silindir("Puskul_Bas", 0.0045, 0.006, (0, 0, -0.186), ipek, kose=10))
for k in range(6):
    a = k * math.pi / 3
    dx, dy = math.cos(a) * 0.0028, math.sin(a) * 0.0028
    p.append(donmus_kutu("Iplik", (0.0012, 0.0012, 0.034), (dx * 1.8, dy * 1.8, -0.206),
                         (-dy * 2.2, dx * 2.2, 0), ipek))
tespih = ortak.birlestir(p, "Tespih")
ortak.yumusat(tespih, 40)
ortak.pivotu_tasi(tespih, (0, 0, 0))

# ---- KASKET ----
# Asılma noktası (0, 0, 0) = arka kenarın üstü. Taç xz düzleminde 26 cm çapında, y'de 8 cm kalın bir elipsoit,
# tepesi odaya (-y) bakar. Tepe düğmesi, sekiz dilim dikişi (düğmeden kenara doğru), dikiş halkası, altta kafa
# bandı ve öne-aşağı eğik siper.
q = [yassi_kure("Tac", (0.26, 0.08, 0.24), (0, -0.03, -0.12), yun, 16)]
q.append(kure("Dugme", 0.009, (0, -0.072, -0.12), dikis, 8))
for k in range(8):
    a = k * math.pi / 4
    # Dikiş: düğmeden kenara uzanan ince şerit, taçın ön yüzüne yaslanır (yüz y ≈ -0,07, kenarda -0,03).
    uz = 0.10
    cx, cz = math.cos(a) * uz / 2, -0.12 + math.sin(a) * uz / 2
    q.append(donmus_kutu("Dikis", (0.003, 0.006, uz), (cx, -0.058, cz), (math.pi / 2, 0, -a + math.pi / 2), dikis))
q.append(halka("Dikis_Halkasi", 0.112, 0.0022, (0, -0.045, -0.12), dikis, rot=(math.pi / 2, 0, 0), bolum=36))
q.append(ortak.kutu("Bant", (0.21, 0.034, 0.016), (0, -0.038, -0.233), siper))
# Siper: yassı yarım elipsoit, taçın altından öne ve 28° aşağı eğik.
q.append(yassi_kure("Siper", (0.20, 0.09, 0.012), (0, -0.085, -0.248), siper, 14, rot=(math.radians(-28), 0, 0)))
kasket = ortak.birlestir(q, "Kasket")
ortak.yumusat(kasket, 40)
ortak.pivotu_tasi(kasket, (0, 0, 0))

# ---- ANAHTARLIK: masada yatık; halka, üç anahtar yelpaze gibi, deri fob ----
r = [halka("Halka", 0.014, 0.0015, (0, 0, 0.0015), celik)]
def anahtar(ad, aci_derece, mat):
    """Halkadan dışa uzanan bir anahtar: baş (halka içinden geçen delikli disk) + sap + diş. Açı meshe işlenir."""
    a = math.radians(aci_derece)
    parcalar = [ortak.silindir("Bas", 0.009, 0.0018, (0, 0, 0.0025), mat, kose=16),
                ortak.kutu("Sap", (0.006, 0.040, 0.0016), (0, 0.028, 0.0025), mat),
                ortak.kutu("Dis1", (0.004, 0.006, 0.0016), (0.004, 0.038, 0.0025), mat),
                ortak.kutu("Dis2", (0.003, 0.005, 0.0016), (0.0035, 0.028, 0.0025), mat)]
    k = ortak.birlestir(parcalar, ad)
    k.location = (0, 0.012, 0)
    k.rotation_euler = (0, 0, a)
    bpy.ops.object.select_all(action="DESELECT"); k.select_set(True); bpy.context.view_layer.objects.active = k
    bpy.ops.object.transform_apply(location=True, rotation=True)
    return k
r.append(anahtar("Anahtar_1", -35, pirinc))
r.append(anahtar("Anahtar_2", 0, celik))
r.append(anahtar("Anahtar_3", 30, pirinc))
r.append(ortak.kutu("Fob", (0.022, 0.034, 0.004), (0, -0.030, 0.002), deri, 0.0015))
anahtarlik = ortak.birlestir(r, "Anahtarlik")
ortak.yumusat(anahtarlik, 40)
ortak.pivotu_tasi(anahtarlik, (0, 0, 0))

ortak.calistir_ve_bitir("Dokuz_Askilar", [tespih, kasket, anahtarlik], 0.3, 0.6, onizlemede_kaldir=0.25)
