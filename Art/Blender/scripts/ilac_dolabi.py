# ilac_dolabi.py
# 2. katta Nuri Bey'in holündeki duvar ecza dolabı, kapağı açık (#83, Şahan'ın seçimi 2026-09-29).
# 90'ların başında her evde olan fırın boyalı sac ecza dolabı: kenarları yuvarlatılmış küçük kutu, üstünde
# ince bir şapka, kapağının dışında kırmızı haç, içinde küçük bir ayna. Kapak menteşesinden 90° açık kalmış
# (açık kapak, gövdenin sağ kenarında duvara dik levha: döndürme gerekmez). İçinde iki cam raf; rafların
# çoğu boşalmış (ilaçlar yere saçılmış, ayrı model), kalanlar devrik: eski karton ilaç kutuları, kahverengi
# cam şişeler, damlalık, pamuk. Boya köşelerden atmış, menteşe altında pas akmış.
#
# PARÇALAR
#   Ilac_Dolabi : tek parça. Pivot ARKA yüzün (duvar) ortası. Ön yüz -y; Unity'de +Z.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
rnd = random.Random(2)        # sabit tohum: pas lekeleri her çalıştırmada aynı yere düşsün

# ---------- MALZEMELER (renkler doğrusal: Unity'de daha açık görünür) ----------
emaye = ortak.malzeme("Ecza_Emaye", (0.70, 0.69, 0.62), puruz=0.35)          # sararmış beyaz fırın boyası
ic_boya = ortak.malzeme("Ecza_IcBoya", (0.55, 0.55, 0.50), puruz=0.5)         # dolabın içi: biraz kirli, mat
ayna = ortak.malzeme("Ecza_Ayna", (0.42, 0.44, 0.45), puruz=0.12, metal=0.6)
kararmis = ortak.malzeme("Ecza_AynaKararmis", (0.05, 0.045, 0.035), puruz=0.5, metal=0.3)
cam = ortak.malzeme("Ecza_Cam_Raf", (0.40, 0.52, 0.48), puruz=0.08)          # kalın camın yeşilimsi kenarı
kahve = ortak.malzeme("Ilac_Sise_Kahve", (0.16, 0.06, 0.015), puruz=0.12)     # kahverengi eczane camı
beyaz = ortak.malzeme("Ilac_Kutu", (0.72, 0.70, 0.62), puruz=0.75)           # sararmış karton kutu
kirmizi = ortak.malzeme("Ilac_Serit", (0.50, 0.06, 0.05), puruz=0.6)          # kutu şeritleri ve haç
mavi = ortak.malzeme("Ilac_Serit_Mavi", (0.05, 0.14, 0.38), puruz=0.6)
yesil = ortak.malzeme("Ilac_Serit_Yesil", (0.06, 0.22, 0.10), puruz=0.6)
etiket = ortak.malzeme("Ilac_Etiket", (0.62, 0.56, 0.40), puruz=0.8)         # şişe etiketi: sararmış kâğıt
lastik = ortak.malzeme("Ilac_Lastik", (0.04, 0.035, 0.03), puruz=0.8)         # damlalık lastiği, şişe kapağı
pamuk = ortak.malzeme("Ilac_Pamuk", (0.78, 0.78, 0.74), puruz=0.95)
metal = ortak.malzeme("Ecza_Metal", (0.60, 0.60, 0.58), puruz=0.3, metal=0.9)   # menteşe, kilit
pas = ortak.malzeme("Ecza_Pas", (0.17, 0.065, 0.02), puruz=0.9)
demir = ortak.malzeme("Ecza_Demir", (0.03, 0.028, 0.025), puruz=0.7)          # boyası atmış kararmış sac

EN, DER, BOY = 0.40, 0.14, 0.53               # gövde: en, derinlik, boy (şapka bunun üstüne 2 cm ekler)
Z_ALT = -0.275                                # gövdenin alt kenarı (pivot dolabın yükseklik ortasında)
zo = Z_ALT + BOY / 2                          # gövdenin orta yüksekliği
yc = -DER / 2                                 # gövdenin derinlik ortası (arka yüz y = 0)
p = []                                        # birleştirilecek parçalar


def kutu(olcu, konum, mat, pah=0.0):
    """Kısa yol: kutuyu kur, listeye ekle. (ortak.kutu konumu meshe gömer: döndürülmez.)"""
    n = ortak.kutu("P", olcu, konum, mat, pah)
    p.append(n)
    return n


def silindir(r, boy, konum, mat, kose=16, rot=None):
    """Silindir; rot verilirse kendi merkezi etrafında döndürülür ve dönüş meshe işlenir."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot or (0, 0, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)   # silindir kendi konumunda kurulduğu için savrulmaz
    ortak.ata(n, mat)
    p.append(n)
    return n


def disk(r, konum, mat, eksen, kal=0.0012, kose=10):
    """İnce disk (leke, kabarcık). eksen: 'y' ise yüzü −y'ye (ön), 'x' ise yüzü x'e (yan yüz) bakar."""
    rot = (math.pi / 2, 0, 0) if eksen == "y" else (0, math.pi / 2, 0)
    return silindir(r, kal, konum, mat, kose, rot)


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    """Ölçeklenmiş küre: damlalık lastiği, pamuk, pas akıntısı."""
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(6, seg // 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


# ---------- GÖVDE ----------
# Boyalı sac kutu: kenarları yuvarlatılmış (sac bükülerek yapılır, keskin köşe olmaz), önü oyuk.
kasa = ortak.kutu("Kasa", (EN, DER, BOY), (0, yc, zo), emaye, 0.008)
kasa.modifiers["Pah"].segments = 3            # 3 bölümlü pah: yumuşak kıvrım
ortak.oy(kasa, ortak.kutu("Oyuk", (EN - 0.03, DER, BOY - 0.03), (0, yc - 0.015, zo), emaye))
p.append(kasa)
kutu((EN - 0.032, 0.002, BOY - 0.032), (0, -0.0155, zo), ic_boya)                     # içi: kirli mat arka yüz
# Şapka: gövdenin üstünde biraz taşan ince levha (o yılların dolaplarında üst kenar hep böyle çıkıntılıydı).
kutu((EN + 0.01, DER + 0.012, 0.02), (0, -(DER + 0.012) / 2, Z_ALT + BOY + 0.01), emaye, 0.005)

# ---------- CAM RAFLAR ----------
# İki kalın cam raf, her biri iki yanda küçük metal tırnaklara oturur.
for z in (-0.09, 0.09):
    kutu((EN - 0.035, DER - 0.03, 0.006), (0, yc + 0.005, z), cam, 0.0015)
    for s in (-1, 1):
        kutu((0.008, 0.02, 0.004), (s * (EN / 2 - 0.019), yc + 0.02, z - 0.005), metal)

z_taban = Z_ALT + 0.015                       # dolabın iç tabanı (gövde sacının üstü)
z_orta, z_ust = -0.087, 0.093                 # alt ve üst cam rafın üst yüzleri

# ---------- KALAN İLAÇLAR ----------
# Taban: yan yatmış kahverengi şişe (yuvarlanıp öne gelmiş), yatık bir karton kutu, mavi kâğıtlı pamuk rulosu.
ad_ = -0.6                                    # yatık şişenin yere göre dönüşü: ağzı öne-sağa bakıyor
yon = (math.cos(ad_), math.sin(ad_))          # şişenin yatay ekseni (gövdeden ağza doğru)
sx, sy, sz = 0.07, yc - 0.01, z_taban + 0.02  # gövdenin merkezi
silindir(0.02, 0.065, (sx, sy, sz), kahve, 16, (0, math.pi / 2, ad_))                     # gövde
bpy.ops.mesh.primitive_cone_add(vertices=16, radius1=0.02, radius2=0.008, depth=0.016,
                                location=(sx + 0.0405 * yon[0], sy + 0.0405 * yon[1], sz), rotation=(0, math.pi / 2, ad_))
omuz_y = bpy.context.active_object            # omuz: gövdeden boyuna daralan koni
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(omuz_y, kahve)
p.append(omuz_y)
silindir(0.0085, 0.012, (sx + 0.054 * yon[0], sy + 0.054 * yon[1], sz), lastik, 12, (0, math.pi / 2, ad_))   # siyah kapak
silindir(0.0205, 0.03, (sx - 0.005 * yon[0], sy - 0.005 * yon[1], sz), etiket, 16, (0, math.pi / 2, ad_))    # etiket
kutu((0.09, 0.055, 0.025), (-0.09, yc + 0.005, z_taban + 0.0125), beyaz, 0.002)
kutu((0.092, 0.057, 0.008), (-0.09, yc + 0.005, z_taban + 0.012), yesil)
silindir(0.035, 0.06, (-0.02, yc + 0.02, z_taban + 0.03 + 0.02), mavi, 18)               # pamuk paketi (mavi kâğıt)
kure(0.03, (-0.02, yc + 0.02, z_taban + 0.083), pamuk, (1, 1, 0.55), seg=12)             # paketten taşan pamuk

# Alt raf: üst üste iki eski ilaç kutusu (biri kaymış), yanında lastik başlı damlalık şişesi.
kutu((0.08, 0.05, 0.10), (-0.10, yc + 0.01, z_orta + 0.05), beyaz, 0.002)                # dik kutu
kutu((0.082, 0.052, 0.015), (-0.10, yc + 0.01, z_orta + 0.075), kirmizi)                 # kırmızı şerit
kutu((0.07, 0.045, 0.02), (-0.085, yc + 0.0, z_orta + 0.11), beyaz, 0.002)               # üstündeki yassı kutu
kutu((0.072, 0.047, 0.006), (-0.085, yc + 0.0, z_orta + 0.11), mavi)
silindir(0.014, 0.045, (0.03, yc + 0.0, z_orta + 0.0225), kahve, 14)                     # damlalık şişesi
silindir(0.0145, 0.02, (0.03, yc + 0.0, z_orta + 0.02), etiket, 14)
silindir(0.007, 0.008, (0.03, yc + 0.0, z_orta + 0.049), metal, 10)                      # boyun bileziği
kure(0.008, (0.03, yc + 0.0, z_orta + 0.062), lastik, (1, 1, 1.5), seg=10)               # lastik puar

# Üst raf: uzun kahverengi şurup şişesi (etiketli) ve kırmızı kapaklı beyaz plastik hap kutusu.
silindir(0.022, 0.085, (0.10, yc + 0.01, z_ust + 0.0425), kahve, 16)
bpy.ops.mesh.primitive_cone_add(vertices=16, radius1=0.022, radius2=0.009, depth=0.018, location=(0.10, yc + 0.01, z_ust + 0.094))
omuz = bpy.context.active_object                                                         # şişenin omzu
ortak.ata(omuz, kahve)
p.append(omuz)
silindir(0.0105, 0.016, (0.10, yc + 0.01, z_ust + 0.111), beyaz, 14)                     # plastik kapak
silindir(0.0225, 0.04, (0.10, yc + 0.01, z_ust + 0.04), etiket, 16)                      # kâğıt etiket
silindir(0.016, 0.04, (0.02, yc + 0.015, z_ust + 0.02), beyaz, 14)                       # hap kutusu
silindir(0.017, 0.012, (0.02, yc + 0.015, z_ust + 0.046), kirmizi, 14)                   # kırmızı kapak

# ---------- AÇIK KAPAK ----------
# Sağ kenardan menteşeli, 90° açık, duvara dik. Dış yüzü (+x, odaya bakan) beyaz boyalı ve ortasında
# kırmızı haç; iç yüzünde (−x) küçük, kenarları kararmış bir ayna.
kx = EN / 2 + 0.008                           # kapak levhasının x ortası
ky = -DER - (EN - 0.01) / 2                   # kapak levhasının y ortası
kapak = kutu((0.016, EN - 0.01, BOY - 0.01), (kx, ky, zo), emaye, 0.005)
kapak.modifiers["Pah"].segments = 2
# Kırmızı haç: dış yüzde iki ince levha (dikey ve yatay kol).
kutu((0.002, 0.07, 0.22), (kx + 0.009, ky, zo + 0.02), kirmizi)
kutu((0.002, 0.22, 0.07), (kx + 0.009, ky, zo + 0.02), kirmizi)
# Kilit: dış yüzde serbest kenara yakın küçük metal rozet, içinde unutulmuş anahtar.
y_kilit = -DER - EN + 0.035                   # serbest kenara 3,5 cm
silindir(0.009, 0.003, (kx + 0.0095, y_kilit, zo - 0.03), metal, 14, (0, math.pi / 2, 0))
kutu((0.012, 0.004, 0.012), (kx + 0.016, y_kilit, zo - 0.03), metal)                     # anahtarın sapı
silindir(0.008, 0.003, (kx + 0.024, y_kilit, zo - 0.03), metal, 12, (0, math.pi / 2, 0))   # anahtarın başı
# İç yüz: kararmış alt katman üstünde biraz küçük parlak ayna; aradaki koyu şerit "gümüşü atmış kenar".
kutu((0.002, 0.22, 0.30), (kx - 0.009, ky, zo + 0.02), kararmis)
kutu((0.0012, 0.20, 0.28), (kx - 0.0105, ky, zo + 0.02), ayna)
for _ in range(16):                           # kenarlar boyunca serpiştirilmiş küçük kararma lekeleri
    r = rnd.uniform(0.002, 0.007)             # küçük ve farklı boylarda: vida başı gibi düzenli görünmesin
    if rnd.random() < 0.5:                    # yarısı uzun (dikey) kenarlarda
        yy = ky + rnd.choice((-1, 1)) * (0.10 - r * 0.8)
        zz = zo + 0.02 + rnd.uniform(-0.14, 0.14)
    else:                                     # yarısı alt/üst kenarda (alt kenar daha çok: nem aşağıda toplanır)
        yy = ky + rnd.uniform(-0.10, 0.10)
        zz = zo + 0.02 + (-1 if rnd.random() < 0.7 else 1) * (0.14 - r * 0.8)
    disk(r, (kx - 0.0113, yy, zz), kararmis, "x", 0.0008)
kutu((0.01, 0.02, 0.04), (kx - 0.012, -DER - EN + 0.03, zo), metal, 0.002)               # mıknatıslı mandal
# Menteşe: gövdenin sağ ön köşesinde boydan boya ince piyano menteşesi.
silindir(0.0045, BOY - 0.03, (EN / 2 + 0.002, -DER - 0.002, zo), metal, 10)

# ---------- AŞINMA ----------
# Boyanın atıp pasın göründüğü yerler: gövde ön kenarının köşeleri ve kapağın alt kenarı (kapak hep
# alttan tutulup çekilmiş). Her leke birkaç küçük diskten oluşan düzensiz bir küme.
y_on = -DER - 0.0006                          # gövdenin ön kenar yüzü
for (x, z, n) in ((-EN / 2 + 0.008, Z_ALT + 0.02, 5), (-EN / 2 + 0.008, Z_ALT + BOY - 0.03, 3), (0.05, Z_ALT + 0.008, 3)):
    for _ in range(n):
        disk(rnd.uniform(0.002, 0.005), (x + rnd.uniform(0, 0.012), y_on, z + rnd.uniform(-0.01, 0.01)), pas, "y")
    disk(0.002, (x + 0.004, y_on - 0.0004, z), demir, "y", 0.002, 8)
for _ in range(6):                            # kapağın dış yüzünde, alt kenar boyunca pas kabarcıkları
    disk(rnd.uniform(0.002, 0.006), (kx + 0.0086, ky + rnd.uniform(-0.17, 0.17), Z_ALT + 0.012 + rnd.uniform(0, 0.012)), pas, "x")
# Menteşe hizasında kapağın dış yüzünden aşağı akmış pas izi: aşağı uzamış yassı damlalar.
for (dy, uz) in ((0.010, 0.06), (0.018, 0.035)):
    kure(0.004, (kx + 0.0082, -DER - dy, Z_ALT + 0.015 + uz / 2), pas, (0.25, 0.6, uz / 0.008), seg=8)

dolap = ortak.birlestir(p, "Ilac_Dolabi")
ortak.yumusat(dolap, 30)
ortak.pivotu_tasi(dolap, (0, 0, 0))
ortak.calistir_ve_bitir("Ilac_Dolabi", [dolap], 0.6, 1.4, onizlemede_kaldir=0.4)
