# mutfak_nuri.py
# 2. katta Nuri Bey'in mutfağının eşyaları (#83, Şahan'ın seçimi 2026-09-29). Katlarda aynı model yok:
# hepsi 1. kattaki mutfaktan farklı. Her biri kendi FBX'i; betik argümanla seçilir:
#   blender -b --factory-startup --python mutfak_nuri.py -- buzdolabi | ocak | tup | masa | sandalye | raf | perde | bulasiklik
#
#   Buzdolabi_70   : 70'lerin yuvarlak köşeli buzdolabı, krom kol; üstünde dantel ve plastik çiçekli vazo;
#                    kapısında mıknatısla tutturulmuş ilaç saatleri kâğıdı.
#   Ocak_Emaye     : krem emaye, dört gözlü tüplü fırınlı ocak, bakalit düğmeler; bir gözünde kapağı yarı
#                    açık, soğumuş çorbalı tencere.
#   Tup_Turuncu    : turuncu tüp, ocağa bağlı hortumuyla (1. kattaki mavi tüpten farklı biçim).
#   Mutfak_Masasi  : kareli muşamba örtülü küçük masa; üstünde yarım kalmış kahvaltı (tek tabakta peynir
#                    ve zeytin, bayat ekmek, yarım çay bardağı).
#   Mutfak_Sandalyesi : metal ayaklı, vinil oturaklı formika sandalye.
#   Teneke_Rafi    : duvar rafında çay, şeker, un tenekeleri ve bir el değirmeni.
#   Mutfak_Perdesi : pencerenin alt yarısını örten kareli kafe perdesi ve ince çubuğu.
#   Bulasiklik     : evyenin yanında tel bulaşıklık; bir tabak, bir bardak, bir çatal.
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy
import mathutils

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "ocak"
ortak.sahneyi_temizle()
p = []


def cubuk(a, b, r, mat, kose=8):
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def dik_disk(ad, r, kal, konum, mat, kose=24):
    """Ön yüzü -y'ye bakan disk (düğme, saat): silindir eklenirken döndürülür."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=kal, location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def kareli(ad, x0, y0, en, boy, z, kare, mat_a, mat_b, kal=0.002):
    """Yatay kareli yüzey (muşamba): en x boy alanı kare x kare karelere böler."""
    nx, ny = int(round(en / kare)), int(round(boy / kare))
    for i in range(nx):
        for j in range(ny):
            p.append(ortak.kutu(ad, (en / nx, boy / ny, kal), (x0 + (i + 0.5) * en / nx, y0 + (j + 0.5) * boy / ny, z), mat_a if (i + j) % 2 == 0 else mat_b))


if hangi == "buzdolabi":
    emaye = ortak.malzeme("Buz70_Emaye", (0.78, 0.76, 0.66), puruz=0.25)
    krom = ortak.malzeme("Buz70_Krom", (0.62, 0.62, 0.60), puruz=0.2, metal=1.0)
    koyu = ortak.malzeme("Buz70_Conta", (0.08, 0.08, 0.08), puruz=0.6)
    dantel = ortak.malzeme("Buz70_Dantel", (0.86, 0.82, 0.72), puruz=0.9)
    vazo = ortak.malzeme("Buz70_Vazo", (0.10, 0.25, 0.40), puruz=0.3)
    cicek = ortak.malzeme("Plastik_Cicek", (0.60, 0.15, 0.25), puruz=0.5)
    yaprak = ortak.malzeme("Plastik_Yaprak", (0.10, 0.30, 0.08), puruz=0.5)
    kagit = ortak.malzeme("Ilac_Kagidi", (0.82, 0.80, 0.72), puruz=0.9)
    murekkep = ortak.malzeme("Ilac_Kagidi_Yazi", (0.10, 0.12, 0.35), puruz=0.9)
    miknatis = ortak.malzeme("Miknatis", (0.55, 0.10, 0.06), puruz=0.4)
    EN, DER, BOY = 0.62, 0.62, 1.40
    p.append(ortak.kutu("Govde", (EN, DER - 0.05, BOY), (0, 0.025, BOY / 2), emaye, 0.09))     # iri yuvarlak köşeler
    p.append(ortak.kutu("Kapi", (EN - 0.02, 0.05, BOY - 0.12), (0, -DER / 2 + 0.03, BOY / 2 + 0.03), emaye, 0.07))
    p.append(ortak.kutu("Conta", (EN - 0.05, 0.012, BOY - 0.15), (0, -DER / 2 + 0.057, BOY / 2 + 0.03), koyu, 0.01))
    # Krom kol: kapının sağ kenarında dikey, iri, 70'lerin kol mekanizması.
    p.append(ortak.kutu("Kol", (0.04, 0.05, 0.28), (EN / 2 - 0.07, -DER / 2 - 0.015, 1.05), krom, 0.015))
    p.append(ortak.kutu("Logo", (0.12, 0.006, 0.03), (0, -DER / 2 + 0.003 - 0.002, BOY - 0.10), krom, 0.004))
    p.append(ortak.kutu("Izgara", (EN - 0.10, 0.01, 0.06), (0, -DER / 2 + 0.02, 0.05), koyu, 0.004))
    # Üstte dantel ve plastik çiçekli vazo.
    p.append(ortak.kutu("Dantel", (0.40, 0.36, 0.003), (0, 0, BOY + 0.0015), dantel))
    p.append(ortak.silindir("Vazo", 0.04, 0.16, (0.08, 0.02, BOY + 0.08), vazo, kose=16, pah_genislik=0.01))
    for dx, dy, h in ((0.0, 0.0, 0.14), (0.03, 0.01, 0.12), (-0.02, 0.02, 0.13)):
        cubuk((0.08 + dx * 0.3, 0.02 + dy * 0.3, BOY + 0.16), (0.08 + dx, 0.02 + dy, BOY + 0.16 + h), 0.003, yaprak, kose=5)
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.022, location=(0.08 + dx, 0.02 + dy, BOY + 0.16 + h), segments=10, ring_count=6)
        ortak.ata(bpy.context.active_object, cicek)
        p.append(bpy.context.active_object)
    # İlaç saatleri kâğıdı: kapıda göz hizasında, iki mıknatısla; üstünde el yazısı satırlar (çizgi).
    ky = -DER / 2 - 0.001
    p.append(ortak.kutu("Kagit", (0.15, 0.002, 0.21), (-0.08, ky, 1.05), kagit))
    for i in range(6):
        p.append(ortak.kutu("Satir", (0.09 + (i % 3) * 0.015, 0.001, 0.004), (-0.09, ky - 0.0015, 1.13 - i * 0.03), murekkep))
    for x in (-0.13, -0.03):
        p.append(ortak.silindir("Miknatis", 0.012, 0.01, (x, ky - 0.003, 1.15), miknatis, kose=12))
    ad, h = "Buzdolabi_70", 1.6

elif hangi == "ocak":
    emaye = ortak.malzeme("Ocak_Emaye", (0.72, 0.68, 0.56), puruz=0.3)
    siyah = ortak.malzeme("Ocak_Izgara", (0.03, 0.03, 0.03), puruz=0.5, metal=0.5)
    bakalit = ortak.malzeme("Ocak_Dugme", (0.05, 0.03, 0.02), puruz=0.3)
    cam = ortak.malzeme("Ocak_Firin_Cami", (0.05, 0.05, 0.05), puruz=0.1)
    krom = ortak.malzeme("Ocak_Krom", (0.62, 0.62, 0.60), puruz=0.2, metal=1.0)
    tencere_m = ortak.malzeme("Tencere_Emaye", (0.45, 0.10, 0.05), puruz=0.3)
    corba = ortak.malzeme("Corba", (0.45, 0.25, 0.08), puruz=0.3)
    EN, DER, BOY = 0.60, 0.58, 0.85
    p.append(ortak.kutu("Govde", (EN, DER, BOY), (0, 0, BOY / 2), emaye, 0.02))
    p.append(ortak.kutu("FirinKapak", (EN - 0.06, 0.02, 0.42), (0, -DER / 2 - 0.01, 0.38), emaye, 0.01))
    p.append(ortak.kutu("FirinCam", (EN - 0.20, 0.004, 0.20), (0, -DER / 2 - 0.021, 0.40), cam))
    p.append(ortak.kutu("FirinKol", (EN - 0.14, 0.03, 0.02), (0, -DER / 2 - 0.035, 0.57), krom, 0.008))
    p.append(ortak.kutu("Cekmece", (EN - 0.06, 0.02, 0.10), (0, -DER / 2 - 0.01, 0.08), emaye, 0.008))
    for i in range(4):
        dik_disk("Dugme", 0.022, 0.03, (-0.21 + i * 0.14, -DER / 2 - 0.015, 0.75), bakalit, kose=16)
    # Dört göz: siyah ızgaralar ve alev halkaları.
    for x in (-0.15, 0.15):
        for y in (-0.12, 0.12):
            p.append(ortak.silindir("Goz", 0.055, 0.012, (x, y, BOY + 0.006), siyah, kose=20))
            p.append(ortak.kutu("IzgaraX", (0.18, 0.012, 0.012), (x, y, BOY + 0.02), siyah))
            p.append(ortak.kutu("IzgaraY", (0.012, 0.18, 0.012), (x, y, BOY + 0.02), siyah))
    p.append(ortak.kutu("ArkaPano", (EN, 0.04, 0.10), (0, DER / 2 - 0.02, BOY + 0.05), emaye, 0.01))
    # Tencere: sol arka gözde; kapağı yarı açık (kenarda eğik), içinde soğumuş çorba.
    tx, ty = -0.15, 0.12
    ten = ortak.silindir("Tencere", 0.10, 0.14, (tx, ty, BOY + 0.026 + 0.07), tencere_m, kose=28)
    ortak.oy(ten, ortak.silindir("TencereIc", 0.092, 0.14, (tx, ty, BOY + 0.036 + 0.07), tencere_m, kose=28))
    p.append(ten)
    p.append(ortak.silindir("Corba", 0.091, 0.005, (tx, ty, BOY + 0.026 + 0.09), corba, kose=28))
    for yon in (-1, 1):
        p.append(ortak.kutu("Kulp", (0.04, 0.02, 0.015), (tx + yon * 0.115, ty, BOY + 0.026 + 0.12), siyah, 0.004))
    bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=0.105, depth=0.012, location=(tx + 0.03, ty - 0.03, BOY + 0.026 + 0.16), rotation=(0.25, 0.15, 0))
    kp = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(kp, tencere_m)
    p.append(kp)
    ad, h = "Ocak_Emaye", 1.0

elif hangi == "tup":
    turuncu = ortak.malzeme("Tup_Turuncu", (0.55, 0.16, 0.02), puruz=0.45)
    metal = ortak.malzeme("Tup2_Metal", (0.50, 0.50, 0.48), puruz=0.3, metal=0.9)
    hortum = ortak.malzeme("Tup2_Hortum", (0.08, 0.08, 0.08), puruz=0.6)
    # Daha uzun, ince gövde; üstte açık halka, altta çember ayak (1. kattakinden farklı biçim).
    p.append(ortak.silindir("AyakCember", 0.13, 0.05, (0, 0, 0.025), turuncu, kose=28))
    p.append(ortak.silindir("Govde", 0.15, 0.50, (0, 0, 0.32), turuncu, kose=32, pah_genislik=0.04))
    for z in (0.20, 0.45):
        p.append(ortak.silindir("Bilezik", 0.152, 0.02, (0, 0, z), turuncu, kose=32))
    p.append(ortak.silindir("Omuz", 0.10, 0.06, (0, 0, 0.60), turuncu, kose=28, pah_genislik=0.02))
    bpy.ops.mesh.primitive_torus_add(major_radius=0.09, minor_radius=0.012, location=(0, 0, 0.70), rotation=(math.pi / 2, 0, 0), major_segments=24, minor_segments=8)
    bpy.ops.object.transform_apply(rotation=True)      # dönüş meshe işlenir: birleştirmede bütün modeli çevirmesin
    ortak.ata(bpy.context.active_object, turuncu)
    p.append(bpy.context.active_object)
    p.append(ortak.silindir("Vana", 0.025, 0.05, (0, 0, 0.655), metal, kose=16))
    p.append(ortak.silindir("Regulator", 0.035, 0.035, (0, 0, 0.695), metal, kose=16))
    # Hortum: regülatörden ocağın arkasına doğru (-x) inen kavis; ocağa bağlı.
    nokta = [(0, 0, 0.71), (-0.06, 0.02, 0.62), (-0.14, 0.05, 0.40), (-0.22, 0.08, 0.30), (-0.30, 0.10, 0.35)]
    for a, b in zip(nokta, nokta[1:]):
        cubuk(a, b, 0.009, hortum)
    ad, h = "Tup_Turuncu", 0.7

elif hangi == "masa":
    ahsap = ortak.malzeme("MMasa_Ahsap", (0.20, 0.12, 0.05), puruz=0.5)
    musamba_a = ortak.malzeme("Musamba_Kirmizi", (0.50, 0.06, 0.05), puruz=0.3)
    musamba_b = ortak.malzeme("Musamba_Beyaz", (0.80, 0.78, 0.72), puruz=0.3)
    tabak = ortak.malzeme("Kahvalti_Tabak", (0.85, 0.84, 0.80), puruz=0.2)
    peynir = ortak.malzeme("Beyaz_Peynir", (0.88, 0.86, 0.78), puruz=0.6)
    zeytin = ortak.malzeme("Zeytin", (0.05, 0.04, 0.04), puruz=0.3)
    ekmek = ortak.malzeme("Bayat_Ekmek", (0.50, 0.32, 0.12), puruz=0.8)
    ekmek_ic = ortak.malzeme("Ekmek_Ici", (0.75, 0.66, 0.48), puruz=0.9)
    cam = ortak.malzeme("MMasa_Bardak", (0.60, 0.62, 0.60), puruz=0.05)
    cay = ortak.malzeme("MMasa_Cay", (0.25, 0.06, 0.01), puruz=0.05)
    metal = ortak.malzeme("Catal", (0.60, 0.60, 0.58), puruz=0.25, metal=0.9)
    EN, DER, H = 0.80, 0.60, 0.74
    for x in (-EN / 2 + 0.04, EN / 2 - 0.04):
        for y in (-DER / 2 + 0.04, DER / 2 - 0.04):
            p.append(ortak.kutu("Ayak", (0.035, 0.035, H - 0.03), (x, y, (H - 0.03) / 2), ahsap, 0.004))
    p.append(ortak.kutu("Tabla", (EN, DER, 0.03), (0, 0, H - 0.015), ahsap, 0.005))
    # Kareli muşamba: tablanın üstü ve kenarlardan sarkan kısmı.
    kareli("Musamba", -EN / 2 - 0.02, -DER / 2 - 0.02, EN + 0.04, DER + 0.04, H + 0.001, 0.07, musamba_a, musamba_b)
    for yon in (-1, 1):
        p.append(ortak.kutu("Sarkma", (EN + 0.04, 0.003, 0.08), (0, yon * (DER / 2 + 0.02), H - 0.04), musamba_a))
        p.append(ortak.kutu("SarkmaK", (0.003, DER + 0.04, 0.08), (yon * (EN / 2 + 0.02), 0, H - 0.04), musamba_b))
    Z = H + 0.003
    # Yarım kalmış kahvaltı: tabakta peynir dilimi ve zeytinler, yanında bayat ekmek, çatal, yarım çay.
    p.append(ortak.silindir("Tabak", 0.11, 0.012, (-0.08, -0.06, Z + 0.006), tabak, kose=28))
    p.append(ortak.kutu("Peynir", (0.08, 0.05, 0.02), (-0.10, -0.06, Z + 0.022), peynir, 0.004))
    for i in range(5):
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.009, location=(-0.03 + (i % 3) * 0.018, -0.02 - (i // 3) * 0.02, Z + 0.02), segments=8, ring_count=6)
        z_ = bpy.context.active_object
        z_.scale = (1, 1.3, 1)
        bpy.ops.object.transform_apply(scale=True)
        ortak.ata(z_, zeytin)
        p.append(z_)
    p.append(ortak.kutu("Catal", (0.012, 0.16, 0.004), (0.05, -0.07, Z + 0.004), metal))
    p.append(ortak.kutu("Ekmek", (0.16, 0.08, 0.07), (0.20, 0.10, Z + 0.035), ekmek, 0.03))
    p.append(ortak.kutu("EkmekKesik", (0.004, 0.07, 0.06), (0.12, 0.10, Z + 0.035), ekmek_ic))
    for ad_, r, h_, z in (("Dip", 0.022, 0.02, 0.012), ("Bel", 0.017, 0.03, 0.037), ("Agiz", 0.024, 0.04, 0.072)):
        p.append(ortak.silindir(ad_, r, h_, (0.10, -0.14, Z + z), cam, kose=20))
    p.append(ortak.silindir("Cay", 0.019, 0.03, (0.10, -0.14, Z + 0.035), cay, kose=20))
    ad, h = "Mutfak_Masasi", 0.9

elif hangi == "sandalye":
    metal = ortak.malzeme("Formika_Metal", (0.60, 0.60, 0.58), puruz=0.25, metal=0.9)
    vinil = ortak.malzeme("Formika_Vinil", (0.35, 0.20, 0.06), puruz=0.4)
    # Metal boru ayaklar ve sırt çerçevesi, vinil kaplı oturak ve sırt yastığı.
    for x in (-0.19, 0.19):
        cubuk((x, -0.19, 0.0), (x, -0.19, 0.44), 0.011, metal)
        cubuk((x, 0.19, 0.0), (x, 0.21, 0.86), 0.011, metal)
    cubuk((-0.19, 0.19, 0.44), (0.19, 0.19, 0.44), 0.009, metal)
    cubuk((-0.19, -0.19, 0.44), (0.19, -0.19, 0.44), 0.009, metal)
    p.append(ortak.kutu("Oturak", (0.42, 0.42, 0.06), (0, 0, 0.47), vinil, 0.025))
    p.append(ortak.kutu("Sirt", (0.40, 0.05, 0.22), (0, 0.205, 0.72), vinil, 0.02))
    ad, h = "Mutfak_Sandalyesi", 0.9

elif hangi == "raf":
    ahsap = ortak.malzeme("TRaf_Ahsap", (0.20, 0.12, 0.05), puruz=0.5)
    renkler = [ortak.malzeme(f"Teneke_{i}", c, puruz=0.4, metal=0.5) for i, c in enumerate(
        ((0.50, 0.08, 0.06), (0.12, 0.30, 0.12), (0.60, 0.50, 0.12), (0.10, 0.15, 0.40)))]
    etiket = ortak.malzeme("Teneke_Etiket", (0.82, 0.78, 0.66), puruz=0.8)
    demir = ortak.malzeme("Degirmen_Demir", (0.15, 0.15, 0.15), puruz=0.4, metal=0.8)
    p.append(ortak.kutu("Raf", (0.80, 0.18, 0.025), (0, -0.09, 0), ahsap, 0.004))
    for x in (-0.33, 0.33):
        p.append(ortak.kutu("Konsol", (0.02, 0.14, 0.12), (x, -0.07, -0.07), ahsap, 0.003))
    # Tenekeler: dört boy boy silindir kutu, önlerinde kâğıt etiket şeridi.
    for i, (x, r, h_) in enumerate(((-0.28, 0.055, 0.16), (-0.14, 0.05, 0.14), (0.0, 0.05, 0.12), (0.13, 0.045, 0.10))):
        p.append(ortak.silindir("Teneke", r, h_, (x, -0.09, 0.0125 + h_ / 2), renkler[i], kose=24))
        p.append(ortak.silindir("Kapak", r + 0.003, 0.012, (x, -0.09, 0.0125 + h_ + 0.006), renkler[i], kose=24))
        p.append(ortak.kutu("Etiket", (r * 1.2, 0.004, h_ * 0.4), (x, -0.09 - r, 0.0125 + h_ / 2), etiket))
    # El değirmeni: sağda, ahşap kutu gövde, üstte demir kol ve topuz.
    p.append(ortak.kutu("DegirmenGovde", (0.09, 0.09, 0.10), (0.29, -0.09, 0.0625), ahsap, 0.006))
    cubuk((0.29, -0.09, 0.115), (0.33, -0.09, 0.17), 0.004, demir)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.012, location=(0.33, -0.09, 0.175), segments=8, ring_count=6)
    ortak.ata(bpy.context.active_object, ahsap)
    p.append(bpy.context.active_object)
    ad, h = "Teneke_Rafi", 0.3

elif hangi == "perde":
    a_m = ortak.malzeme("MPerde_Kirmizi", (0.45, 0.06, 0.05), puruz=0.9)
    b_m = ortak.malzeme("MPerde_Beyaz", (0.82, 0.80, 0.74), puruz=0.9)
    metal = ortak.malzeme("MPerde_Cubuk", (0.55, 0.55, 0.52), puruz=0.3, metal=0.9)
    # Kafe perdesi: pencere pervazının içinde, orta yükseklikte ince çubuk; altta kareli perde (pilili).
    EN, BOY = 1.20, 0.55
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.006, depth=EN, location=(0, -0.03, 0), rotation=(0, math.pi / 2, 0))
    bpy.ops.object.transform_apply(rotation=True)      # dönüş meshe işlenir: birleştirmede bütün modeli çevirmesin
    ortak.ata(bpy.context.active_object, metal)
    p.append(bpy.context.active_object)
    n = 24
    for i in range(n):
        x = -EN / 2 + (i + 0.5) * EN / n
        for j in range(8):
            z = -0.02 - (j + 0.5) * BOY / 8
            p.append(ortak.kutu("Kare", (EN / n + 0.004, 0.004, BOY / 8), (x, -0.03 + (0.008 if i % 2 == 0 else -0.008), z), a_m if (i // 2 + j) % 2 == 0 else b_m))
    ad, h = "Mutfak_Perdesi", 0.3

else:
    tel = ortak.malzeme("Bulasik_Tel", (0.60, 0.60, 0.58), puruz=0.3, metal=0.9)
    tabak = ortak.malzeme("Bulasik_Tabak", (0.85, 0.84, 0.80), puruz=0.2)
    cam = ortak.malzeme("Bulasik_Bardak", (0.60, 0.62, 0.60), puruz=0.05)
    # Tel bulaşıklık: taban çerçevesi, dik teller (tabak yuvaları), bardak sepeti.
    EN, DER = 0.36, 0.26
    for y in (-DER / 2, DER / 2):
        cubuk((-EN / 2, y, 0.01), (EN / 2, y, 0.01), 0.003, tel)
    for x in (-EN / 2, EN / 2):
        cubuk((x, -DER / 2, 0.01), (x, DER / 2, 0.01), 0.003, tel)
    for i in range(9):
        x = -EN / 2 + 0.03 + i * 0.035
        cubuk((x, -0.05, 0.01), (x, -0.05, 0.12), 0.002, tel, kose=5)
        cubuk((x, 0.02, 0.01), (x, 0.02, 0.09), 0.002, tel, kose=5)
    # Tek tabak (dik), tek bardak (ters), tek çatal (bardak sepetinde).
    bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=0.10, depth=0.012, location=(-0.03, -0.015, 0.11), rotation=(math.pi / 2, 0, 0))
    ortak.ata(bpy.context.active_object, tabak)
    bpy.ops.object.transform_apply(rotation=True)
    p.append(bpy.context.active_object)
    p.append(ortak.silindir("Bardak", 0.03, 0.10, (0.13, 0.07, 0.06), cam, kose=20))
    cubuk((0.14, 0.00, 0.02), (0.15, 0.00, 0.17), 0.003, tel, kose=6)
    ad, h = "Bulasiklik", 0.2

nesne = ortak.birlestir(p, ad)
ortak.yumusat(nesne, 35)
ortak.pivotu_tasi(nesne, (0, 0, 0))
ortak.calistir_ve_bitir(ad, [nesne], h, 2.0 if hangi in ("buzdolabi", "ocak", "masa") else 1.2)
