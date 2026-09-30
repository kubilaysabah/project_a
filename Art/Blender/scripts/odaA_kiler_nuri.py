# odaA_kiler_nuri.py
# 2. katta Nuri Bey'in yatak odası (Oda A) ve kileri (#83). Katlarda aynı model yok: hepsi yeni.
#   blender -b --factory-startup --python odaA_kiler_nuri.py -- <ad>
#
# ODA A (yatak odası; radyo komodinin üstünde çalar):
#   karyola   Karyola_Nuri     : ceviz başlıklı çift kişilik karyola, tığ işi yatak örtüsü; bir yanı
#                                bozulmamış (eşinin yanı), öbür yanında buruşuk yorgan ve çukur yastık.
#   komodin   Komodin_Nuri     : alçak ceviz komodin, üstü 0,55 m (radyo üstünde durur); tek çekmecesi boş.
#   tuvalet   Tuvalet_Masasi   : eşinin aynalı tuvalet masası; kolonya şişesi, tarak, pudra kutusu.
#   sandalye  Elbise_Sandalyesi: hasır oturaklı sandalye, arkalığında ceketi, oturağında katlı pantolonu.
#   kilim     Kilim_Nuri       : yatağın ayak ucunda küçük, çizgili kilim.
#   perde     Perde_Cicekli    : çiçek desenli kısa perde ve ince pirinç çubuğu.
#   levha     Hat_Levha        : yaldız çerçeveli hat levhası ("Maşallah" yazılı gibi kıvrımlı yaldız çizgiler).
# KİLER (Nuri'nin radyo tamir köşesi; kat anahtarı teneke kutuda):
#   raf       Kiler_Rafi       : galvaniz metal raf; kavanozlar, konserveler, bez torbalar.
#   tezgah    Radyo_Tezgahi    : ahşap çalışma tezgâhı; açık kasalı eski radyo, lambalar (tüp), havya, pense.
#   teneke    Teneke_Nuri      : bisküvi tenekesi (kapağı kalkar); içinde kat anahtarı durur.
#   cuval     Kiler_Cuvallari  : üç bez çuval (un, bulgur, pirinç), biri yarı boş.
#   supurge   Supurge_Faras    : saplı süpürge ve faraş, duvara dayalı.
#   damacana  Damacana         : hasır örgülü cam damacana (sirke).
#   sepet     Sogan_Sepeti     : tel sepette soğan ve patates.
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import mathutils

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "komodin"
ortak.sahneyi_temizle()
rnd = random.Random(29)
p = []
parcalar = []          # açılır eşyalarda Govde dışındaki ayrı nesneler (çekmece, kapı, kapak)


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk."""
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def yatik_silindir(r, boy, konum, rot, mat, kose=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=8)
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


ceviz = ortak.malzeme("Nuri_Ceviz", (0.045, 0.022, 0.010), puruz=0.45)
ceviz_ac = ortak.malzeme("Nuri_CevizAcik", (0.09, 0.045, 0.02), puruz=0.5)
pirinc = ortak.malzeme("Nuri_Pirinc", (0.55, 0.40, 0.13), puruz=0.3, metal=0.9)
acik = False            # açılır eşya mı (Govde + parçalar)

if hangi == "karyola":
    ortu = ortak.malzeme("Karyola_Ortu", (0.72, 0.68, 0.58), puruz=0.95)
    yorgan = ortak.malzeme("Karyola_Yorgan", (0.30, 0.10, 0.08), puruz=0.9)
    yastik = ortak.malzeme("Karyola_Yastik", (0.80, 0.78, 0.72), puruz=0.9)
    EN, BOY = 1.40, 2.00                     # x: en, y: boy (baş +y tarafında, duvara dayalı)
    # Başlık: iki dikme, kavisli üst tabla, ortada oymalı pano (üstü 0,86: pencerenin denizliğinin altında).
    for x in (-EN / 2 - 0.03, EN / 2 + 0.03):
        p.append(ortak.kutu("Dikme", (0.06, 0.06, 0.86), (x, BOY / 2 - 0.03, 0.43), ceviz, 0.008))
        p.append(ortak.kutu("DikmeA", (0.06, 0.06, 0.42), (x, -BOY / 2 + 0.03, 0.21), ceviz, 0.008))
    p.append(ortak.kutu("BasPano", (EN, 0.03, 0.44), (0, BOY / 2 - 0.03, 0.56), ceviz, 0.006))
    p.append(ortak.kutu("BasTac", (EN + 0.12, 0.05, 0.05), (0, BOY / 2 - 0.03, 0.83), ceviz, 0.01))
    for i in range(5):
        p.append(ortak.kutu("Oyma", (0.14, 0.012, 0.22), (-0.44 + i * 0.22, BOY / 2 - 0.05, 0.56), ceviz_ac, 0.004))
    p.append(ortak.kutu("AyakPano", (EN, 0.03, 0.22), (0, -BOY / 2 + 0.03, 0.31), ceviz, 0.006))
    for x in (-EN / 2, EN / 2):
        p.append(ortak.kutu("Kasa", (0.03, BOY - 0.06, 0.16), (x, 0, 0.28), ceviz, 0.004))
    # Şilte ve tığ işi örtü: yatağı örter, kenarlardan sarkar.
    p.append(ortak.kutu("Silte", (EN - 0.04, BOY - 0.10, 0.18), (0, 0, 0.44), ortu, 0.04))
    for x in (-EN / 2 - 0.005, EN / 2 + 0.005):
        p.append(ortak.kutu("Sarkma", (0.01, BOY - 0.2, 0.22), (x, -0.02, 0.36), ortu, 0.004))
    # Eşinin yanı (sol, -x): düzgün yastık. Kendi yanı (sağ, +x): çukur yastık, buruşuk yorgan.
    p.append(ortak.kutu("Yastik", (0.55, 0.36, 0.12), (-0.33, BOY / 2 - 0.30, 0.58), yastik, 0.05))
    p.append(ortak.kutu("Yastik", (0.55, 0.36, 0.08), (0.33, BOY / 2 - 0.32, 0.56), yastik, 0.035))
    for i in range(5):
        p.append(ortak.kutu("Yorgan", (0.62, 0.30, 0.07 + 0.02 * (i % 2)), (0.36 + rnd.uniform(-0.02, 0.02), 0.25 - i * 0.26, 0.565 + 0.01 * (i % 3)), yorgan, 0.03))
    # Nuri Bey'in ceketi ve pantolonu ayak ucuna, eşinin boş yanına atılmış (elbise sandalyesi odayı
    # daralttığı için kaldırıldı, #83). Şiltenin üstü z = 0,53; yorgan x > 0,05'te, ona değmez.
    ceket = ortak.malzeme("Karyola_Ceket", (0.07, 0.07, 0.09), puruz=0.85)
    pant = ortak.malzeme("Karyola_Pantolon", (0.12, 0.10, 0.08), puruz=0.85)
    yA = -BOY / 2 + 0.34                     # ceketin ortası: ayak panosundan 30 cm içeride
    p.append(ortak.kutu("Ceket", (0.62, 0.44, 0.035), (-0.32, yA, 0.548), ceket, 0.012))
    p.append(ortak.kutu("CeketYaka", (0.62, 0.07, 0.05), (-0.32, yA + 0.20, 0.555), ceket, 0.02))
    p.append(ortak.kutu("CeketKol", (0.36, 0.11, 0.03), (-0.20, yA - 0.14, 0.58), ceket, 0.01))
    # Öbür kolu yatağın kenarından sarkar (örtünün sarkan kenarının 2 cm dışında).
    p.append(ortak.kutu("CeketKolUst", (0.14, 0.11, 0.03), (-EN / 2 + 0.02, yA + 0.06, 0.55), ceket, 0.01))
    p.append(ortak.kutu("CeketKolSarkan", (0.03, 0.11, 0.36), (-EN / 2 - 0.035, yA + 0.06, 0.37), ceket, 0.01))
    p.append(ortak.kutu("Pantolon", (0.30, 0.26, 0.03), (-0.30, yA + 0.40, 0.545), pant, 0.012))
    ad, h = "Karyola_Nuri", 1.0

elif hangi == "komodin":
    acik = True
    EN, DER, H = 0.46, 0.40, 0.55
    govde = ac.govde_levhalari(EN, DER, H, ceviz, taban=0.08, raflar=(0.30,))
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
        for y in (-DER / 2 + 0.03, DER / 2 - 0.03):
            govde.append(ortak.kutu("Ayak", (0.035, 0.035, 0.09), (x, y, 0.045), ceviz, 0.006))
    # Alt bölmenin önünde sabit tabla (kapağı yok, içinde eski dergiler).
    dergi = ortak.malzeme("Komodin_Dergi", (0.45, 0.40, 0.30), puruz=0.9)
    govde.append(ortak.kutu("Dergi", (0.30, 0.24, 0.05), (0, 0.02, 0.125), dergi, 0.003))
    p = govde
    parcalar.append(ac.cekmece("Cekmece_1", 0, 0.325, EN - 0.04, 0.18, DER - 0.06, -DER / 2, ceviz_ac, ceviz, pirinc, "topuz"))
    ad, h = "Komodin_Nuri", 0.4

elif hangi == "tuvalet":
    cam = ortak.malzeme("Tuvalet_Ayna", (0.55, 0.58, 0.60), puruz=0.03, metal=1.0)
    sise = ortak.malzeme("Tuvalet_Sise", (0.25, 0.40, 0.30), puruz=0.05)
    pudra = ortak.malzeme("Tuvalet_Pudra", (0.60, 0.35, 0.40), puruz=0.4)
    EN, DER, H = 0.95, 0.42, 0.74
    p.append(ortak.kutu("Tabla", (EN, DER, 0.03), (0, 0, H - 0.015), ceviz, 0.006))
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
        for y in (-DER / 2 + 0.03, DER / 2 - 0.03):
            p.append(ortak.kutu("Ayak", (0.035, 0.035, H - 0.03), (x, y, (H - 0.03) / 2), ceviz, 0.005))
    p.append(ortak.kutu("Etek", (EN - 0.06, 0.02, 0.12), (0, -DER / 2 + 0.02, H - 0.09), ceviz, 0.004))
    p.append(ortak.kutu("KulpT", (0.08, 0.012, 0.012), (0, -DER / 2 + 0.005, H - 0.09), pirinc, 0.003))
    # Oval ayna ve çerçevesi: tablanın arkasında.
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.30, depth=0.03, location=(0, DER / 2 - 0.03, H + 0.42), rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object; n.scale = (1.0, 1.25, 1.0); bpy.ops.object.transform_apply(rotation=True, scale=True); ortak.ata(n, ceviz); p.append(n)
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.26, depth=0.01, location=(0, DER / 2 - 0.05, H + 0.42), rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object; n.scale = (1.0, 1.25, 1.0); bpy.ops.object.transform_apply(rotation=True, scale=True); ortak.ata(n, cam); p.append(n)
    for x in (-0.20, 0.20):
        p.append(ortak.kutu("AynaAyak", (0.03, 0.03, 0.14), (x, DER / 2 - 0.03, H + 0.07), ceviz, 0.004))
    # Tablanın üstü: kolonya şişesi, pudra kutusu, tarak.
    p.append(ortak.silindir("Kolonya", 0.035, 0.16, (-0.30, -0.02, H + 0.08), sise, kose=16))
    p.append(ortak.silindir("KolonyaK", 0.015, 0.03, (-0.30, -0.02, H + 0.175), pirinc, kose=12))
    p.append(ortak.silindir("Pudra", 0.05, 0.03, (0.28, -0.05, H + 0.015), pudra, kose=24))
    p.append(ortak.kutu("Tarak", (0.16, 0.035, 0.006), (0.05, -0.10, H + 0.003), ceviz_ac, 0.001))
    ad, h = "Tuvalet_Masasi", 1.1

elif hangi == "sandalye":
    hasir = ortak.malzeme("ElbiseS_Hasir", (0.35, 0.25, 0.10), puruz=0.9)
    ceket = ortak.malzeme("ElbiseS_Ceket", (0.07, 0.07, 0.09), puruz=0.85)
    pant = ortak.malzeme("ElbiseS_Pantolon", (0.12, 0.10, 0.08), puruz=0.85)
    S, H = 0.40, 0.45
    for x in (-S / 2 + 0.02, S / 2 - 0.02):
        p.append(ortak.kutu("OnAyak", (0.035, 0.035, H), (x, -S / 2 + 0.02, H / 2), ceviz_ac, 0.004))
        p.append(ortak.kutu("ArkaAyak", (0.035, 0.035, 0.95), (x, S / 2 - 0.02, 0.475), ceviz_ac, 0.004))
    p.append(ortak.kutu("Oturak", (S, S, 0.04), (0, 0, H), hasir, 0.01))
    for z in (0.70, 0.85):
        p.append(ortak.kutu("Arkalik", (S - 0.04, 0.02, 0.06), (0, S / 2 - 0.02, z), ceviz_ac, 0.004))
    # Ceket arkalığın üstüne asılı: omuzlar arkalığın tepesinde, iki yana ve öne sarkar.
    p.append(ortak.kutu("CeketOmuz", (0.46, 0.10, 0.06), (0, S / 2 - 0.02, 0.94), ceket, 0.025))
    p.append(ortak.kutu("CeketArka", (0.44, 0.03, 0.45), (0, S / 2 + 0.02, 0.70), ceket, 0.01))
    p.append(ortak.kutu("CeketOn", (0.44, 0.03, 0.30), (0, S / 2 - 0.06, 0.78), ceket, 0.01))
    p.append(ortak.kutu("Pantolon", (0.34, 0.30, 0.04), (0, -0.02, H + 0.04), pant, 0.012))
    ad, h = "Elbise_Sandalyesi", 0.9

elif hangi == "kilim":
    renk = [ortak.malzeme(f"Kilim_{i}", c, puruz=0.95) for i, c in enumerate(((0.30, 0.04, 0.03), (0.60, 0.40, 0.12), (0.05, 0.08, 0.15), (0.70, 0.66, 0.55)))]
    EN, BOY = 1.10, 0.62
    n = 11
    for i in range(n):
        p.append(ortak.kutu("Serit", (EN / n + 0.001, BOY, 0.008), (-EN / 2 + (i + 0.5) * EN / n, 0, 0.004), renk[(i * 3) % 4]))
    for x in (-EN / 2 - 0.02, EN / 2 + 0.02):
        for j in range(8):
            p.append(ortak.kutu("Puskul", (0.04, 0.012, 0.003), (x, -BOY / 2 + 0.05 + j * 0.075, 0.0015), renk[3]))
    ad, h = "Kilim_Nuri", 0.2

elif hangi == "perde":
    zemin = ortak.malzeme("PerdeC_Zemin", (0.62, 0.55, 0.40), puruz=0.9)
    cicek = ortak.malzeme("PerdeC_Cicek", (0.35, 0.08, 0.10), puruz=0.9)
    yaprak = ortak.malzeme("PerdeC_Yaprak", (0.10, 0.20, 0.06), puruz=0.9)
    EN, BOY = 1.50, 1.35                      # çubuktan aşağı
    cubuk((-EN / 2 - 0.05, -0.08, 0), (EN / 2 + 0.05, -0.08, 0), 0.009, pirinc)
    for x in (-EN / 2 - 0.05, EN / 2 + 0.05):
        p.append(ortak.kutu("Tutucu", (0.015, 0.08, 0.02), (x, -0.04, 0), pirinc))
    # İki kanat, iki yana toplanmış, pilili (dalgalı şeritler); üstlerinde çiçek lekeleri.
    for yon in (-1, 1):
        for i in range(9):
            x = yon * (EN / 2 - 0.02 - i * 0.045)
            y = -0.08 + (0.018 if i % 2 == 0 else -0.018)
            p.append(ortak.kutu("Pile", (0.062, 0.012, BOY), (x, y, -BOY / 2 - 0.01), zemin))
            for j in range(4):
                if (i + j) % 2 == 0:
                    p.append(ortak.kutu("Cicek", (0.035, 0.004, 0.035), (x, y - 0.008, -0.2 - j * 0.3 - 0.05 * (i % 3)), cicek))
                    p.append(ortak.kutu("Yaprak", (0.02, 0.004, 0.03), (x + 0.015, y - 0.008, -0.23 - j * 0.3 - 0.05 * (i % 3)), yaprak))
    ad, h = "Perde_Cicekli", 0.3

elif hangi == "levha":
    yaldiz = ortak.malzeme("Hat_Yaldiz", (0.60, 0.42, 0.10), puruz=0.3, metal=0.9)
    zemin = ortak.malzeme("Hat_Zemin", (0.03, 0.08, 0.05), puruz=0.8)
    EN, BOY = 0.56, 0.36
    p.append(ortak.kutu("Cerceve", (EN, 0.03, BOY), (0, -0.015, 0), ceviz, 0.008))
    p.append(ortak.kutu("Zemin", (EN - 0.07, 0.004, BOY - 0.07), (0, -0.032, 0), zemin))
    p.append(ortak.kutu("Ic", (EN - 0.05, 0.006, 0.01), (0, -0.031, BOY / 2 - 0.03), yaldiz))
    p.append(ortak.kutu("Ic", (EN - 0.05, 0.006, 0.01), (0, -0.031, -BOY / 2 + 0.03), yaldiz))
    # Hat: kıvrımlı yaldız çizgiler (sağdan sola yazılmış bir kelimenin izlenimi) ve noktalar.
    for i in range(7):
        x = 0.18 - i * 0.06
        for k in range(3):
            a = math.radians(-60 + 60 * k + i * 25)
            p.append(ortak.kutu("Harf", (0.05, 0.004, 0.008), (x + 0.02 * math.cos(a), -0.036, 0.02 * math.sin(a) + 0.03 * math.sin(i)), yaldiz))
        if i % 2 == 0:
            p.append(ortak.kutu("Nokta", (0.012, 0.004, 0.012), (x, -0.036, 0.08), yaldiz))
    ad, h = "Hat_Levha", 0.3

elif hangi == "raf":
    galv = ortak.malzeme("KRaf_Galvaniz", (0.35, 0.36, 0.36), puruz=0.45, metal=0.8)
    kav = ortak.malzeme("KRaf_Cam", (0.45, 0.50, 0.45), puruz=0.05)
    tursu = ortak.malzeme("KRaf_Tursu", (0.18, 0.25, 0.05), puruz=0.3)
    salca = ortak.malzeme("KRaf_Salca", (0.35, 0.04, 0.02), puruz=0.3)
    kap = ortak.malzeme("KRaf_Kapak", (0.45, 0.35, 0.08), puruz=0.4, metal=0.7)
    konserve = ortak.malzeme("KRaf_Konserve", (0.40, 0.40, 0.38), puruz=0.3, metal=0.8)
    etiket = ortak.malzeme("KRaf_Etiket", (0.55, 0.10, 0.06), puruz=0.7)
    torba = ortak.malzeme("KRaf_Torba", (0.60, 0.55, 0.42), puruz=0.95)
    EN, DER, H = 1.60, 0.38, 1.80
    for x in (-EN / 2, EN / 2):
        for y in (-DER / 2, DER / 2):
            p.append(ortak.kutu("Dikme", (0.03, 0.03, H), (x, y, H / 2), galv, 0.002))
    raflar = (0.10, 0.52, 0.94, 1.36, 1.78)
    for z in raflar:
        p.append(ortak.kutu("Raf", (EN + 0.03, DER + 0.03, 0.02), (0, 0, z), galv, 0.002))
    for ri, z in enumerate(raflar[:-1]):
        x = -EN / 2 + 0.10
        while x < EN / 2 - 0.10:
            s = rnd.random()
            if ri >= 2 and s < 0.55:          # kavanozlar üst raflarda
                icm = tursu if rnd.random() < 0.5 else salca
                h1 = rnd.uniform(0.16, 0.24)
                p.append(ortak.silindir("Kavanoz", 0.055, h1, (x, rnd.uniform(-0.08, 0.06), z + 0.01 + h1 / 2), icm, kose=14))
                p.append(ortak.silindir("Kapak", 0.056, 0.02, (x, p[-1].location.y, z + 0.02 + h1), kap, kose=14))
                x += 0.13
            elif s < 0.8:                      # konserve yığını
                for k in range(2):
                    p.append(ortak.silindir("Konserve", 0.04, 0.11, (x, 0.0, z + 0.065 + k * 0.11), konserve, kose=12))
                    p.append(ortak.silindir("Etiket", 0.0405, 0.06, (x, 0.0, z + 0.065 + k * 0.11), etiket, kose=12))
                x += 0.10
            else:                              # bez torba
                p.append(ortak.kutu("Torba", (0.18, 0.16, 0.22), (x + 0.04, 0.0, z + 0.12), torba, 0.04))
                x += 0.24
    ad, h = "Kiler_Rafi", 1.8

elif hangi == "tezgah":
    kasa = ortak.malzeme("RTezgah_Kasa", (0.12, 0.06, 0.025), puruz=0.5)
    sasi = ortak.malzeme("RTezgah_Sasi", (0.38, 0.38, 0.36), puruz=0.4, metal=0.8)
    lamba = ortak.malzeme("RTezgah_Lamba", (0.30, 0.28, 0.25), puruz=0.05, metal=0.3)
    siyah = ortak.malzeme("RTezgah_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
    kirmizi = ortak.malzeme("RTezgah_Kablo", (0.40, 0.03, 0.02), puruz=0.5)
    EN, DER, H = 1.30, 0.60, 0.78
    p.append(ortak.kutu("Tabla", (EN, DER, 0.04), (0, 0, H - 0.02), ceviz_ac, 0.006))
    for x in (-EN / 2 + 0.05, EN / 2 - 0.05):
        for y in (-DER / 2 + 0.05, DER / 2 - 0.05):
            p.append(ortak.kutu("Ayak", (0.06, 0.06, H - 0.04), (x, y, (H - 0.04) / 2), ceviz_ac, 0.006))
    p.append(ortak.kutu("AltRaf", (EN - 0.08, DER - 0.08, 0.02), (0, 0, 0.18), ceviz_ac, 0.004))
    p.append(ortak.kutu("ArkaPano", (EN, 0.02, 0.50), (0, DER / 2 - 0.01, H + 0.25), ceviz_ac, 0.004))
    # Açık kasalı radyo: ahşap kasası yanda, şasisi tablada, lambaları dikili.
    p.append(ortak.kutu("RadyoKasa", (0.42, 0.24, 0.28), (-0.40, 0.12, H + 0.14), kasa, 0.02))
    p.append(ortak.kutu("Sasi", (0.34, 0.20, 0.03), (0.02, 0.05, H + 0.015), sasi, 0.003))
    for i in range(5):
        p.append(ortak.silindir("Tup", 0.014, 0.07, (-0.10 + i * 0.06, 0.08, H + 0.065), lamba, kose=12))
    p.append(ortak.silindir("Trafo", 0.035, 0.06, (0.13, 0.00, H + 0.06), siyah, kose=12))
    # Havya ve sehpası, pense, kablo kangalı; alt rafta bir kutu yedek lamba.
    cubuk((0.32, -0.12, H + 0.015), (0.52, -0.05, H + 0.03), 0.009, siyah)
    cubuk((0.52, -0.05, H + 0.03), (0.60, -0.02, H + 0.035), 0.004, sasi)
    p.append(ortak.kutu("Pense", (0.14, 0.03, 0.012), (0.30, 0.14, H + 0.006), kirmizi, 0.004))
    bpy.ops.mesh.primitive_torus_add(major_radius=0.05, minor_radius=0.008, location=(0.45, 0.18, H + 0.01), major_segments=16, minor_segments=6)
    ortak.ata(bpy.context.active_object, kirmizi); p.append(bpy.context.active_object)
    p.append(ortak.kutu("YedekKutu", (0.30, 0.22, 0.16), (-0.25, 0.0, 0.27), kasa, 0.01))
    ad, h = "Radyo_Tezgahi", 1.0

elif hangi == "teneke":
    acik = True
    boya = ortak.malzeme("Teneke_Boya", (0.35, 0.05, 0.04), puruz=0.35, metal=0.6)
    yaldiz = ortak.malzeme("Teneke_Yaldiz", (0.60, 0.45, 0.12), puruz=0.3, metal=0.9)
    EN, DER, H = 0.24, 0.18, 0.10
    t = 0.004
    p = [ac.levha("TTaban", -EN / 2, EN / 2, -DER / 2, DER / 2, 0, t, boya, 0.0),
         ac.levha("TYan", -EN / 2, -EN / 2 + t, -DER / 2, DER / 2, 0, H, boya, 0.0),
         ac.levha("TYan", EN / 2 - t, EN / 2, -DER / 2, DER / 2, 0, H, boya, 0.0),
         ac.levha("TYan", -EN / 2, EN / 2, -DER / 2, -DER / 2 + t, 0, H, boya, 0.0),
         ac.levha("TYan", -EN / 2, EN / 2, DER / 2 - t, DER / 2, 0, H, boya, 0.0),
         ac.levha("Serit", -EN / 2 - 0.001, EN / 2 + 0.001, -DER / 2 - 0.001, -DER / 2 + 0.001, 0.04, 0.06, yaldiz, 0.0)]
    k = ac.kapak("Kapak", EN + 0.008, DER + 0.008, 0.012, H + 0.008, boya, yaldiz)
    parcalar.append(k)
    ad, h = "Teneke_Nuri", 0.2

elif hangi == "cuval":
    bez = ortak.malzeme("Cuval_Bez", (0.55, 0.47, 0.33), puruz=0.95)
    ip = ortak.malzeme("Cuval_Ip", (0.30, 0.22, 0.10), puruz=0.9)
    for i, (x, y, h1) in enumerate(((-0.25, 0.0, 0.55), (0.12, 0.05, 0.50), (0.00, -0.30, 0.30))):
        p.append(ortak.kutu("Cuval", (0.34, 0.28, h1), (x, y, h1 / 2), bez, 0.08))
        p.append(ortak.kutu("Agiz", (0.20, 0.16, 0.08), (x, y, h1 + 0.03), bez, 0.03))
        p.append(ortak.kutu("Ip", (0.21, 0.17, 0.02), (x, y, h1 + 0.005), ip, 0.005))
    ad, h = "Kiler_Cuvallari", 0.5

elif hangi == "supurge":
    sap = ortak.malzeme("Supurge_Sap", (0.20, 0.12, 0.05), puruz=0.6)
    kil = ortak.malzeme("Supurge_Kil", (0.55, 0.42, 0.15), puruz=0.95)
    plastik = ortak.malzeme("Faras_Plastik", (0.05, 0.20, 0.35), puruz=0.4)
    # Duvara 12° eğik dayalı süpürge (pivot duvarın dibinde, arka yüz +y duvar).
    e = math.radians(12)
    cubuk((0.0, -0.30, 0.10), (0.0, -0.30 + 1.25 * math.sin(e), 0.10 + 1.25 * math.cos(e)), 0.012, sap)
    p.append(ortak.kutu("Kil", (0.26, 0.08, 0.16), (0.0, -0.31, 0.08), kil, 0.02))
    p.append(ortak.kutu("KilBag", (0.27, 0.085, 0.03), (0.0, -0.31, 0.15), sap, 0.004))
    # Faraş: yerde, duvara yaslı.
    p.append(ortak.kutu("Faras", (0.26, 0.24, 0.012), (0.35, -0.14, 0.006), plastik, 0.003))
    p.append(ortak.kutu("FarasArka", (0.26, 0.012, 0.10), (0.35, -0.02, 0.05), plastik, 0.003))
    cubuk((0.35, -0.02, 0.10), (0.35, 0.0, 0.45), 0.01, plastik)
    ad, h = "Supurge_Faras", 0.7

elif hangi == "damacana":
    cam = ortak.malzeme("Damacana_Cam", (0.18, 0.26, 0.16), puruz=0.05)
    hasir = ortak.malzeme("Damacana_Hasir", (0.40, 0.28, 0.12), puruz=0.95)
    kure(0.20, (0, 0, 0.24), cam, olcek=(1, 1, 1.15), seg=20)
    p.append(ortak.silindir("Boyun", 0.04, 0.12, (0, 0, 0.50), cam, kose=16))
    p.append(ortak.silindir("Tipa", 0.035, 0.04, (0, 0, 0.57), hasir, kose=12))
    for i in range(6):
        p.append(ortak.silindir("Orgu", 0.205, 0.035, (0, 0, 0.04 + i * 0.045), hasir, kose=24))
    ad, h = "Damacana", 0.5

else:   # sepet
    tel = ortak.malzeme("Sepet_Tel", (0.25, 0.25, 0.24), puruz=0.4, metal=0.8)
    sogan = ortak.malzeme("Sepet_Sogan", (0.45, 0.22, 0.06), puruz=0.6)
    patates = ortak.malzeme("Sepet_Patates", (0.40, 0.30, 0.15), puruz=0.8)
    EN, DER, H = 0.44, 0.32, 0.26
    for z in (0.01, 0.13, H):
        for y in (-DER / 2, DER / 2):
            cubuk((-EN / 2, y, z), (EN / 2, y, z), 0.004, tel, kose=6)
        for x in (-EN / 2, EN / 2):
            cubuk((x, -DER / 2, z), (x, DER / 2, z), 0.004, tel, kose=6)
    for i in range(7):
        x = -EN / 2 + i * EN / 6
        cubuk((x, -DER / 2, 0.01), (x, -DER / 2, H), 0.003, tel, kose=5)
        cubuk((x, DER / 2, 0.01), (x, DER / 2, H), 0.003, tel, kose=5)
        cubuk((x, -DER / 2, 0.01), (x, DER / 2, 0.01), 0.003, tel, kose=5)
    for i in range(14):
        x, y = rnd.uniform(-0.17, 0.17), rnd.uniform(-0.11, 0.11)
        z = 0.06 + (i // 7) * 0.08
        if i % 2 == 0:
            kure(0.045, (x, y, z), sogan, olcek=(1, 1, 0.9))
        else:
            kure(0.042, (x, y, z), patates, olcek=(1.25, 0.9, 0.8))
    ad, h = "Sogan_Sepeti", 0.4

if acik:
    govde = ortak.birlestir(p, "Govde")
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, 1.2)
else:
    nesne = ortak.birlestir(p, ad)
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    uzak = {"karyola": 3.0, "raf": 2.6, "tezgah": 2.0, "perde": 2.0, "tuvalet": 1.8}.get(hangi, 1.2)
    ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=1.5 if hangi == "perde" else (0.3 if hangi == "levha" else 0.0))
