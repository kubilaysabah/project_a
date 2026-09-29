# kat6_esyalar.py
# 6. kat, A dairesi (21 numara): 1974'te donup kalmış ev (#83). Kimse oturmuyor ama salonda dört kişilik
# sofra kurulmayı bekliyor; her şey tozlu, 70'ler eşyası. Katlarda aynı model yok: hepsi yeni ve _K6 ekli.
# B dairesinin (terzi Sabri Usta) eşyaları kat6_esyalar_2.py içinde.
#   blender -b --factory-startup --python kat6_esyalar.py -- <ad> [--onizleme <png>]
#
#   sapka     Sapka_Askisi_K6     : ayaklı bükme ahşap şapkalık; bir kancasında eski fötr şapka, birinde kadın şalı.
#   telefon   Telefon_Masasi_K6   : küçük telefon masası, dantel örtü, yeşil çevirmeli telefon, altında rehber.
#   yolluk    Yolluk_K6           : soluk kırmızı, tozlu hol yolluğu (3,4 x 0,75), madalyonlu, saçaklı.
#   takvim    Takvim_1994_K6      : manzara resimli duvar takvimi, Eylül 1994 tablosu (rakamlar kutucukla temsil);
#                                   resmin altında oyunun yazı yazacağı BOŞ, açık renkli alan.
#   avize     Avize_K6            : pirinç, beş kollu, cam laleli avize (pivot tepede, tavana asılır).
#   perde     Perde_K6            : ahşap korniş kutusu, püsküllü saçaklı kadife fırfır, iki yana bağlanmış ağır yeşil kadife.
#   pikap     Pikap_K6            : 70'ler ayaklı dolaplı pikap; kapağı açık, üstünde plak.
#   tv        Eski_TV_K6          : 70'ler ayaklı ahşap kasalı siyah-beyaz televizyon.
#   teldolap  Tel_Dolap_K6        : tel örgülü kapaklı yiyecek dolabı (süs).
#   ocak      Ocak_K6             : ayaklı sehpa üstünde iki gözlü emaye tüplü ocak; altta küçük tüp ve hortum.
#   masa      Mutfak_Masasi_K6    : küçük ahşap mutfak masası; üstünde emaye çaydanlık (altı su, üstü demlik).
#   raf       Bos_Raf_K6          : tozlu ahşap raf, fırfırlı raf kâğıtları, iki boş kavanoz.
#   karyola   Karyola_K6          : siyah demir karyola, pirinç topuzlu; çıplak yatak ve katlı yorgan (başlık +y).
#   sandik    Ceyiz_Sandigi_K6    : AÇILIR (Govde + Kapak) oymalı çeyiz sandığı; kapak arkadan menteşeli, içi oyuk.
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import bmesh
from mathutils import Vector, Matrix

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "sandik"
ortak.sahneyi_temizle()
rnd = random.Random(61)
p = []                 # birleştirilecek parçalar
parcalar = []          # açılır eşyalarda Govde dışındaki ayrı nesneler (kapak)


# ---------- YARDIMCILAR (bu betiğe özel) ----------

def K(ad, olcu, konum, mat, pah=0.0):
    """ortak.kutu + listeye ekle: en çok kullanılan satır."""
    n = ortak.kutu(ad, olcu, konum, mat, pah)
    p.append(n)
    return n


def S(ad, r, boy, konum, mat, kose=24, pah=0.0):
    """ortak.silindir + listeye ekle (dik silindir)."""
    n = ortak.silindir(ad, r, boy, konum, mat, kose, pah)
    p.append(n)
    return n


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk (yönü to_track_quat ile hesaplanır)."""
    d = Vector(b) - Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((Vector(a) + Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)   # dönüş meshe işlensin; birleştirmede bütün modeli çevirmesin
    ortak.ata(n, mat)
    p.append(n)
    return n


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(6, seg // 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def egri(noktalar, r, mat, kose=8):
    """Noktalardan geçen kırık çizgi boru; eklem yerlerine küçük küre (boşluk görünmesin)."""
    for a, b in zip(noktalar, noktalar[1:]):
        cubuk(a, b, r, mat, kose)
    for q in noktalar[1:-1]:
        kure(r, q, mat, seg=kose)


def disk(r, kal, merkez, normal, mat, kose=24):
    """Ekseni 'normal' yönüne bakan disk (düğme, kadran)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=kal, location=merkez)
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = Vector(normal).to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def halka(R, r, merkez, normal, mat, seg=24, olcek=(1, 1, 1)):
    """Torus (simit): ekseni 'normal' yönünde."""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, location=merkez, major_segments=seg, minor_segments=6)
    n = bpy.context.active_object
    n.scale = olcek
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = Vector(normal).to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def normal_duzelt(me):
    """from_pydata ile kurulan meshte yüz yönlerini dışa çevirir (Unity arka yüzleri çizmez)."""
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me)
    bm.free()


def yeni_mesh(ad, v, f, mat):
    me = bpy.data.meshes.new(ad)
    me.from_pydata(v, [], f)
    me.update()
    normal_duzelt(me)
    n = bpy.data.objects.new(ad, me)
    bpy.context.collection.objects.link(n)
    ortak.ata(n, mat)
    p.append(n)
    return n


def torna(ad, profil, mat, kose=24, konum=(0, 0, 0), kalinlik=0.0, alt=True, ust=True, olcek=(1, 1)):
    """
    TORNA (döndürme yüzeyi): profil = alttan üste (yarıçap, z) çiftleri; her yükseklikte bir halka
    kurulup halkalar yüzle bağlanır. Vazo, çaydanlık, avize gövdesi gibi yuvarlak şekiller için.
    kalinlik > 0: açık kabuklara (lale, kavanoz) et kalınlığı verir; yoksa Unity içini görünmez çizer.
    """
    v, f = [], []
    for (r, z) in profil:
        for i in range(kose):
            a = 2 * math.pi * i / kose
            v.append((konum[0] + r * math.cos(a) * olcek[0], konum[1] + r * math.sin(a) * olcek[1], konum[2] + z))
    for j in range(len(profil) - 1):
        for i in range(kose):
            a0, a1 = j * kose + i, j * kose + (i + 1) % kose
            f.append((a0, a1, a1 + kose, a0 + kose))
    if alt:
        f.append(tuple(reversed(range(kose))))
    if ust:
        s = (len(profil) - 1) * kose
        f.append(tuple(range(s, s + kose)))
    n = yeni_mesh(ad, v, f, mat)
    if kalinlik > 0:
        m = n.modifiers.new("Kal", "SOLIDIFY")
        m.thickness = kalinlik
    return n


def prizma(ad, noktalar, t0, t1, mat, duzlem="xy"):
    """Düz bir çokgeni kalınlaştırır. duzlem 'xy': noktalar (x, y), kalınlık z'de; 'xz': noktalar (x, z), kalınlık y'de."""
    k = len(noktalar)
    v = []
    for t in (t0, t1):
        for (a, b) in noktalar:
            v.append((a, b, t) if duzlem == "xy" else (a, t, b))
    f = [tuple(range(k)), tuple(range(k, 2 * k))]
    for i in range(k):
        j = (i + 1) % k
        f.append((i, j, k + j, k + i))
    return yeni_mesh(ad, v, f, mat)


def donustur(n, M):
    """Nesneyi dünya konumuna işleyip meshini M matrisiyle döndürür/taşır (menteşe çevresinde döndürmek için)."""
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    n.data.transform(M)
    n.data.update()


def don(eksen, aci, merkez):
    """merkez noktası çevresinde 'eksen' ('X','Y','Z') etrafında aci (radyan) dönüş matrisi."""
    m = Vector(merkez)
    return Matrix.Translation(m) @ Matrix.Rotation(aci, 4, eksen) @ Matrix.Translation(-m)


# ---------- ORTAK MALZEMELER (1974 evi: koyu, tozlu, soluk) ----------
ceviz = ortak.malzeme("K6A_Ceviz", (0.050, 0.024, 0.011), puruz=0.6)
tik = ortak.malzeme("K6A_Tik", (0.13, 0.065, 0.028), puruz=0.55)
pirinc = ortak.malzeme("K6A_Pirinc", (0.42, 0.30, 0.10), puruz=0.5, metal=0.85)     # tozlu, donuk pirinç
toz = ortak.malzeme("K6A_Toz", (0.20, 0.18, 0.15), puruz=1.0)
acik = False            # açılır eşya mı (Govde + parçalar)

if hangi == "sapka":
    bukme = ortak.malzeme("Sapka_Bukme", (0.06, 0.03, 0.013), puruz=0.45)
    fotr = ortak.malzeme("Sapka_Fotr", (0.028, 0.025, 0.022), puruz=0.9)
    bant = ortak.malzeme("Sapka_Bant", (0.015, 0.012, 0.012), puruz=0.6)
    sal = ortak.malzeme("Sapka_Sal", (0.10, 0.02, 0.035), puruz=0.95)
    cubuk((0, 0, 0.10), (0, 0, 1.72), 0.019, bukme, 12)                # ana direk
    kure(0.028, (0, 0, 1.74), bukme, olcek=(1, 1, 1.35))               # tepe topuzu
    # Dört bükme ayak: direkten dışa ve aşağı kıvrılır, ucunda küçük topuz.
    for k in range(4):
        a = math.radians(45 + 90 * k)
        nk = []
        for i in range(7):
            t = i / 6
            r = 0.018 + 0.23 * t
            nk.append((r * math.cos(a), r * math.sin(a), 0.36 * (1 - t) ** 2 + 0.018))
        egri(nk, 0.014, bukme)
        kure(0.02, nk[-1], bukme, olcek=(1, 1, 0.7))
    halka(0.10, 0.008, (0, 0, 0.17), (0, 0, 1), bukme, seg=28)         # ayakları birbirine bağlayan halka
    # Tepe kancaları: üçü yüksek (kısa), üçü alçak (uzun), bükme ahşap gibi kıvrık.
    for k in range(6):
        a = math.radians(30 + 60 * k)
        z0, L = (1.62, 0.17) if k % 2 == 0 else (1.50, 0.22)
        nk = []
        for i in range(6):
            t = i / 5
            r = 0.018 + L * math.sin(t * math.pi / 2)
            nk.append((r * math.cos(a), r * math.sin(a), z0 + 0.10 * t ** 2.2))
        egri(nk, 0.011, bukme)
        kure(0.016, nk[-1], bukme)
    # Fötr şapka: 30° kancasının (ön sağ) ucuna takılı, hafif yana eğik.
    a = math.radians(30)
    hx, hy = 0.19 * math.cos(a), 0.19 * math.sin(a)
    sp = [torna("Kenar", [(0.145, 0.0), (0.15, 0.006), (0.10, 0.012)], fotr, kose=32, konum=(hx, hy, 1.68)),
          torna("Tepe", [(0.092, 0.0), (0.09, 0.07), (0.075, 0.105), (0.03, 0.115)], fotr, kose=32, konum=(hx, hy, 1.685), olcek=(1, 0.85)),
          torna("Bant", [(0.094, 0.0), (0.094, 0.025)], bant, kose=32, konum=(hx, hy, 1.688), olcek=(1, 0.86), alt=False, ust=False, kalinlik=0.002)]
    for n in sp:
        donustur(n, don("Y", math.radians(-12), (hx, hy, 1.70)))
    # Kadın şalı: 210° kancasından (arka sol) iki kat sarkar; ucunda saçaklar.
    a = math.radians(210)
    sx, sy = 0.20 * math.cos(a), 0.20 * math.sin(a)
    for i, (dy, uz) in enumerate(((-0.012, 0.62), (0.012, 0.52))):
        n = K("Sal", (0.30, 0.012, uz), (sx, sy + dy, 1.58 - uz / 2), sal, 0.004)
        donustur(n, don("Z", a + math.pi / 2, (sx, sy, 1.58)) @ don("X", math.radians(4 * (1 - 2 * i)), (sx, sy, 1.58)))
    cubuk((sx - 0.15 * math.cos(a + math.pi / 2), sy - 0.15 * math.sin(a + math.pi / 2), 1.585),
          (sx + 0.15 * math.cos(a + math.pi / 2), sy + 0.15 * math.sin(a + math.pi / 2), 1.585), 0.018, sal, 10)
    for j in range(12):
        t = -0.14 + j * 0.0255
        cx, cy = sx + t * math.cos(a + math.pi / 2), sy + t * math.sin(a + math.pi / 2)
        cubuk((cx, cy - 0.012, 0.96), (cx + rnd.uniform(-0.01, 0.01), cy - 0.012, 0.90), 0.003, sal, 5)
    ad, h, uzak = "Sapka_Askisi_K6", 1.8, 2.6

elif hangi == "telefon":
    yesil = ortak.malzeme("Telefon_Yesil", (0.05, 0.20, 0.08), puruz=0.3)
    krem = ortak.malzeme("Telefon_Krem", (0.75, 0.72, 0.60), puruz=0.4)
    siyah = ortak.malzeme("Telefon_Kadran", (0.02, 0.02, 0.02), puruz=0.4, metal=0.6)
    dantel = ortak.malzeme("Telefon_Dantel", (0.72, 0.68, 0.56), puruz=0.95)
    rehber = ortak.malzeme("Telefon_Rehber", (0.50, 0.40, 0.12), puruz=0.9)
    EN, DER, H = 0.45, 0.35, 0.70
    K("Tabla", (EN, DER, 0.025), (0, 0, H - 0.0125), tik, 0.006)
    K("Etek", (EN - 0.06, 0.018, 0.06), (0, -DER / 2 + 0.03, H - 0.055), tik, 0.003)
    K("Etek", (EN - 0.06, 0.018, 0.06), (0, DER / 2 - 0.03, H - 0.055), tik, 0.003)
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
        K("EtekY", (0.018, DER - 0.06, 0.06), (x, 0, H - 0.055), tik, 0.003)
        for y in (-DER / 2 + 0.035, DER / 2 - 0.035):
            # Tornalı ayak: şişkin-ince-şişkin, 70'lerin telefon sehpası.
            torna("Ayak", [(0.013, 0), (0.017, 0.04), (0.011, 0.10), (0.020, 0.24), (0.012, 0.40), (0.017, 0.56), (0.016, H - 0.025)], tik, kose=12, konum=(x, y, 0))
    K("AltRaf", (EN - 0.05, DER - 0.05, 0.015), (0, 0, 0.20), tik, 0.003)
    K("Rehber", (0.21, 0.15, 0.035), (-0.05, 0.02, 0.225), rehber, 0.004)
    K("Rehber2", (0.18, 0.13, 0.02), (-0.03, 0.03, 0.2525), krem, 0.003)
    # Oval dantel örtü ve fistolu kenarı.
    torna("Dantel", [(0.17, 0), (0.17, 0.002)], dantel, kose=40, konum=(0, 0, H), olcek=(1, 0.78))
    for i in range(18):
        a = 2 * math.pi * i / 18
        S("Fisto", 0.022, 0.002, (0.17 * math.cos(a), 0.17 * 0.78 * math.sin(a), H + 0.001), dantel, kose=10)
    # Telefon gövdesi: önü eğik (kadran eğik yüzde), arkası yüksek; kesik piramit, pahlı.
    z0 = H + 0.002
    v = [(-0.11, -0.12, z0), (0.11, -0.12, z0), (0.11, 0.11, z0), (-0.11, 0.11, z0),
         (-0.09, -0.02, z0 + 0.11), (0.09, -0.02, z0 + 0.11), (0.09, 0.09, z0 + 0.12), (-0.09, 0.09, z0 + 0.12)]
    f = [(0, 1, 2, 3), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
    g = yeni_mesh("Govde", v, f, yesil)
    ortak.pah(g, 0.012, 3)
    # Kadran: eğik ön yüzün ortasında; on parmak deliği (krem numara kartı görünür), ortada krem etiket.
    nrm = Vector((0, -0.11, 0.10)).normalized()
    mrk = Vector((0, -0.07, z0 + 0.055)) + nrm * 0.004
    disk(0.048, 0.008, tuple(mrk), nrm, siyah, 32)
    disk(0.040, 0.006, tuple(mrk + nrm * 0.002), nrm, krem, 32)
    u = Vector((1, 0, 0))
    w = nrm.cross(u)
    for i in range(10):
        a = math.radians(-60 + i * 28)
        q = mrk + nrm * 0.0052 + (u * math.cos(a) + w * math.sin(a)) * 0.030
        disk(0.0065, 0.002, tuple(q), nrm, siyah, 12)
    disk(0.015, 0.003, tuple(mrk + nrm * 0.006), nrm, yesil, 20)
    # Ahize: üstteki çatalın üzerinde, x boyunca yatık; iki ucunda kulaklık ve mikrofon.
    for x in (-0.07, 0.07):
        K("Catal", (0.025, 0.05, 0.02), (x, 0.04, z0 + 0.125), yesil, 0.006)
    cubuk((-0.10, 0.04, z0 + 0.155), (0.10, 0.04, z0 + 0.155), 0.017, yesil, 16)
    for x in (-0.105, 0.105):
        torna("Uc", [(0.024, 0), (0.032, 0.02), (0.030, 0.035)], yesil, kose=20, konum=(x, 0.04, z0 + 0.125))
    # Kıvrık kordon: gövdenin solundan masaya sarkıp ahizenin ucuna çıkar.
    yol = [Vector((-0.11, 0.04, z0 + 0.02)), Vector((-0.17, 0.0, z0 + 0.008)), Vector((-0.19, -0.05, z0 + 0.008)),
           Vector((-0.16, -0.07, z0 + 0.03)), Vector((-0.12, 0.03, z0 + 0.13))]
    for a, b in zip(yol, yol[1:]):
        adim = max(2, int((b - a).length / 0.012))
        for i in range(adim):
            q = a + (b - a) * (i / adim)
            halka(0.007, 0.0018, tuple(q), b - a, siyah, seg=10)
    ad, h, uzak = "Telefon_Masasi_K6", 0.8, 1.3

elif hangi == "yolluk":
    kirmizi = ortak.malzeme("Yolluk_Kirmizi", (0.26, 0.06, 0.05), puruz=0.95)
    lacivert = ortak.malzeme("Yolluk_Kenar", (0.06, 0.055, 0.08), puruz=0.95)
    krem = ortak.malzeme("Yolluk_Krem", (0.42, 0.36, 0.26), puruz=0.95)
    tozm = ortak.malzeme("Yolluk_Toz", (0.21, 0.09, 0.07), puruz=1.0)
    L, W = 3.40, 0.75
    K("Zemin", (L, W, 0.006), (0, 0, 0.003), kirmizi)
    # Kenar bordürü (lacivert) ve içindeki krem çizgi.
    for y in (-W / 2 + 0.04, W / 2 - 0.04):
        K("Kenar", (L, 0.08, 0.001), (0, y, 0.0065), lacivert)
        K("Cizgi", (L - 0.16, 0.012, 0.001), (0, y - 0.05 if y > 0 else y + 0.05, 0.0065), krem)
    for x in (-L / 2 + 0.04, L / 2 - 0.04):
        K("KenarU", (0.08, W - 0.16, 0.001), (x, 0, 0.0065), lacivert)
    # Ortada beş madalyon (baklava) ve aralarında küçük yıldızlar.
    for i in range(5):
        x = -1.2 + i * 0.6
        prizma("Madalyon", [(x - 0.20, 0), (x, -0.20), (x + 0.20, 0), (x, 0.20)], 0.006, 0.0068, lacivert)
        prizma("MadalyonIc", [(x - 0.11, 0), (x, -0.11), (x + 0.11, 0), (x, 0.11)], 0.0068, 0.0074, krem)
        prizma("Gobek", [(x - 0.04, 0), (x, -0.04), (x + 0.04, 0), (x, 0.04)], 0.0074, 0.0078, kirmizi)
        if i < 4:
            for yy in (-0.18, 0.18):
                prizma("Yildiz", [(x + 0.3 - 0.03, yy), (x + 0.3, yy - 0.03), (x + 0.3 + 0.03, yy), (x + 0.3, yy + 0.03)], 0.006, 0.0068, krem)
    # Saçaklar: iki kısa kenarda.
    for sx in (-1, 1):
        for j in range(34):
            y = -W / 2 + 0.02 + j * (W - 0.04) / 33
            K("Sacak", (0.05, 0.006, 0.003), (sx * (L / 2 + 0.025), y + rnd.uniform(-0.002, 0.002), 0.0015), krem)
    # Toz lekeleri ve ortası yürümekten açılmış iz (soluk, düzensiz).
    for i in range(6):
        K("Toz", (rnd.uniform(0.15, 0.35), rnd.uniform(0.06, 0.14), 0.0005), (rnd.uniform(-1.5, 1.5), rnd.uniform(-0.2, 0.2), 0.0081), tozm)
    ad, h, uzak = "Yolluk_K6", 0.2, 2.6

elif hangi == "takvim":
    kart = ortak.malzeme("Takvim6_Kart", (0.66, 0.62, 0.50), puruz=0.9)
    gok = ortak.malzeme("Takvim6_Gok", (0.22, 0.36, 0.55), puruz=0.8)
    dag = ortak.malzeme("Takvim6_Dag", (0.10, 0.12, 0.16), puruz=0.8)
    kar = ortak.malzeme("Takvim6_Kar", (0.80, 0.80, 0.78), puruz=0.8)
    cayir = ortak.malzeme("Takvim6_Cayir", (0.10, 0.22, 0.05), puruz=0.8)
    gol = ortak.malzeme("Takvim6_Gol", (0.08, 0.20, 0.34), puruz=0.5)
    bos = ortak.malzeme("Takvim6_YaziAlani", (0.84, 0.82, 0.74), puruz=0.9)
    kirmizi = ortak.malzeme("Takvim6_Kirmizi", (0.40, 0.04, 0.03), puruz=0.8)
    hucre = ortak.malzeme("Takvim6_Hucre", (0.78, 0.75, 0.66), puruz=0.9)
    rakam = ortak.malzeme("Takvim6_Rakam", (0.04, 0.04, 0.04), puruz=0.9)
    metal = ortak.malzeme("Takvim6_Metal", (0.40, 0.40, 0.40), puruz=0.4, metal=1.0)
    EN, BOY = 0.35, 0.50
    K("Arka", (EN, 0.003, BOY), (0, -0.0015, 0), kart)               # arka yüz y = 0 (duvar)
    # Manzara resmi (üstte): gökyüzü zemini, iki dağ ve karlı tepeleri, çayır, göl.
    K("Resim", (0.31, 0.001, 0.19), (0, -0.0035, 0.125), gok)
    prizma("Dag", [(-0.155, 0.08), (-0.06, 0.19), (0.03, 0.08)], -0.0040, -0.0048, dag, "xz")
    prizma("Dag", [(-0.02, 0.08), (0.07, 0.17), (0.155, 0.08)], -0.0040, -0.0046, dag, "xz")
    prizma("Kar", [(-0.08, 0.167), (-0.06, 0.19), (-0.04, 0.167)], -0.0048, -0.0052, kar, "xz")
    prizma("Kar", [(0.052, 0.15), (0.07, 0.17), (0.088, 0.15)], -0.0046, -0.0050, kar, "xz")
    K("Cayir", (0.31, 0.0012, 0.05), (0, -0.0046, 0.055), cayir)
    K("Gol", (0.13, 0.0006, 0.018), (0.05, -0.0055, 0.048), gol)
    # OYUNUN YAZI ALANI: resmin hemen altında, boş ve açık renkli (0,25 x 0,08). Ön yüzü y = -0,004.
    K("YaziAlani", (0.25, 0.001, 0.08), (0, -0.0035, -0.02), bos)
    # Ay tablosu: kırmızı gün başlığı şeridi + 5 hafta x 7 gün kutucuk. Eylül 1994 perşembe başlar (pazartesi = 0).
    K("Baslik", (0.31, 0.001, 0.018), (0, -0.0035, -0.080), kirmizi)
    for c in range(7):
        K("GunAdi", (0.014, 0.0005, 0.004), (-0.155 + 0.0443 * (c + 0.5), -0.0043, -0.080), hucre)
    for d in range(1, 31):
        i = d - 1 + 3
        r, c = i // 7, i % 7
        x = -0.155 + 0.0443 * (c + 0.5)
        z = -0.109 - r * 0.028
        K("Hucre", (0.040, 0.001, 0.024), (x, -0.0035, z), hucre)
        m = kirmizi if c == 6 else rakam
        if d < 10:
            K("Rakam", (0.004, 0.0006, 0.012), (x, -0.0043, z), m)
        else:
            K("Rakam", (0.004, 0.0006, 0.012), (x - 0.004, -0.0043, z), m)
            K("Rakam", (0.004, 0.0006, 0.012), (x + 0.004, -0.0043, z), m)
    K("Serit", (EN, 0.004, 0.012), (0, -0.005, BOY / 2 - 0.006), metal, 0.001)
    halka(0.012, 0.0015, (0, -0.002, BOY / 2 + 0.012), (0, 1, 0), metal, seg=16)   # asma halkası
    ad, h, uzak = "Takvim_1994_K6", 0.5, 0.9

elif hangi == "avize":
    cam = ortak.malzeme("Avize_Cam", (0.72, 0.66, 0.50), puruz=0.35)        # buzlu, sararmış lale camı
    # Tavan göbeği (pivot tepede, z = 0 tavan).
    torna("Gobek", [(0.012, -0.065), (0.035, -0.05), (0.06, -0.02), (0.065, -0.004), (0.065, 0.0)], pirinc, kose=28)
    # Zincir: birbirine dik dönmüş halkalar.
    for i in range(8):
        z = -0.075 - i * 0.022
        halka(0.011, 0.0028, (0, 0, z), (1, 0, 0) if i % 2 == 0 else (0, 1, 0), pirinc, seg=12, olcek=(1, 1.5, 1))
    # Orta gövde: tornalı pirinç (alttan üste profil), altta damla topuz.
    torna("Govde", [(0.004, -0.60), (0.018, -0.585), (0.03, -0.55), (0.055, -0.49), (0.07, -0.455), (0.07, -0.435),
                    (0.05, -0.41), (0.028, -0.38), (0.026, -0.33), (0.042, -0.30), (0.02, -0.27), (0.012, -0.255)], pirinc, kose=28)
    # Beş kol: gövdeden dışa-aşağı kıvrılıp yukarı döner; ucunda tabla, duy ve yukarı bakan cam lale.
    for k in range(5):
        a = math.radians(90 + 72 * k)
        rz = [(0.06, -0.445), (0.12, -0.475), (0.19, -0.47), (0.25, -0.44), (0.285, -0.39), (0.29, -0.35)]
        egri([(r * math.cos(a), r * math.sin(a), z) for (r, z) in rz], 0.009, pirinc, 10)
        cx, cy = 0.29 * math.cos(a), 0.29 * math.sin(a)
        torna("Tabla", [(0.01, -0.355), (0.042, -0.34), (0.046, -0.332)], pirinc, kose=20, konum=(cx, cy, 0))
        S("Duy", 0.014, 0.035, (cx, cy, -0.322), pirinc, kose=12)
        torna("Lale", [(0.02, -0.325), (0.034, -0.31), (0.052, -0.27), (0.062, -0.225), (0.066, -0.20)], cam, kose=24,
              konum=(cx, cy, 0), alt=False, ust=False, kalinlik=0.003)
        kure(0.008, (0.07 * math.cos(a), 0.07 * math.sin(a), -0.445), pirinc, seg=8)
    ad, h, uzak = "Avize_K6", 0.6, 1.4

elif hangi == "perde":
    kadife = ortak.malzeme("Perde6_Kadife", (0.035, 0.075, 0.035), puruz=0.85)     # tozlu koyu yeşil kadife
    puskul = ortak.malzeme("Perde6_Puskul", (0.40, 0.30, 0.09), puruz=0.6)
    EN, BOY = 2.00, 2.25                       # korniş boyu; kornişten perdenin eteğine
    # Ahşap korniş kutusu: duvardan 14 cm çıkar; üst arka kenarı pivot (0, 0, 0).
    K("KornisOn", (EN, 0.02, 0.12), (0, -0.13, -0.06), ceviz, 0.004)
    K("KornisUst", (EN, 0.14, 0.018), (0, -0.07, -0.009), ceviz, 0.003)
    for x in (-EN / 2 + 0.01, EN / 2 - 0.01):
        K("KornisYan", (0.02, 0.14, 0.12), (x, -0.07, -0.06), ceviz, 0.003)
    K("Pervaz", (EN + 0.03, 0.03, 0.02), (0, -0.14, -0.012), ceviz, 0.006)      # üstte profil
    K("Pervaz", (EN + 0.02, 0.025, 0.015), (0, -0.14, -0.115), ceviz, 0.005)    # altta profil
    # Kadife fırfır: korniş önünde fistolu (yarım daire dilimli) şerit ve püsküllü saçak.
    for i in range(10):
        x = -EN / 2 + 0.1 + i * 0.2
        nk = [(x - 0.1, -0.125)]
        for j in range(1, 8):
            t = j / 8
            nk.append((x - 0.1 + 0.2 * t, -0.125 - 0.10 * math.sin(t * math.pi)))
        nk.append((x + 0.1, -0.125))
        prizma("Firfir", [(q[0], q[1]) for q in nk] + [(x + 0.1, -0.02), (x - 0.1, -0.02)], -0.160, -0.148, kadife, "xz")
        for j in range(5):
            t = (j + 0.5) / 5
            cubuk((x - 0.1 + 0.2 * t, -0.162, -0.125 - 0.10 * math.sin(t * math.pi)),
                  (x - 0.1 + 0.2 * t, -0.162, -0.125 - 0.10 * math.sin(t * math.pi) - 0.04), 0.003, puskul, 5)
    # İki kanat: üstte açık, bağ yerinde toplanmış, eteğe doğru yeniden açılır. Her pli bir kırık çizgi boru.
    BAG = -1.15
    for yon in (-1, 1):
        n = 9
        for i in range(n):
            t = i / (n - 1)
            xu = yon * (0.56 + 0.40 * t)                 # üst: 0,56 .. 0,96
            xb = yon * (0.83 + 0.10 * t)                 # bağ: toplanmış
            xe = yon * (0.66 + 0.32 * t)                 # etek: yeniden açık
            y0 = -0.075 - (0.022 if i % 2 == 0 else 0.0)
            nk = [(xu, y0, -0.10), ((xu * 0.55 + xb * 0.45), y0 - 0.01, -0.60), (xb, -0.105, BAG),
                  ((xb * 0.6 + xe * 0.4), y0 - 0.02, -1.65), (xe, y0 - 0.03, -BOY)]
            egri(nk, 0.036, kadife, 10)
        # Kadife bağ ve altından sarkan püskül.
        halka(0.11, 0.022, (yon * 0.88, -0.105, BAG), (0, 0, 1), kadife, seg=20, olcek=(1, 0.55, 1))
        cubuk((yon * 0.88, -0.17, BAG), (yon * 0.88, -0.17, BAG - 0.10), 0.004, puskul, 6)
        kure(0.022, (yon * 0.88, -0.17, BAG - 0.11), puskul, seg=10)
        torna("Puskul", [(0.03, BAG - 0.26), (0.03, BAG - 0.24), (0.018, BAG - 0.15), (0.012, BAG - 0.125)], puskul, kose=14,
              konum=(yon * 0.88, -0.17, 0))
    ad, h, uzak = "Perde_K6", 2.3, 3.2

elif hangi == "pikap":
    kumas = ortak.malzeme("Pikap_Kumas", (0.13, 0.10, 0.06), puruz=0.95)
    alu = ortak.malzeme("Pikap_Alu", (0.55, 0.55, 0.53), puruz=0.35, metal=0.9)
    plak = ortak.malzeme("Pikap_Plak", (0.012, 0.012, 0.012), puruz=0.25)
    etiket = ortak.malzeme("Pikap_Etiket", (0.40, 0.05, 0.04), puruz=0.6)
    panel = ortak.malzeme("Pikap_Panel", (0.03, 0.028, 0.025), puruz=0.5)
    kadran = ortak.malzeme("Pikap_Kadran", (0.70, 0.62, 0.40), puruz=0.5)
    EN, DER, Z0, Z1 = 0.80, 0.40, 0.16, 0.50
    K("Govde", (EN, DER, Z1 - Z0), (0, 0, (Z0 + Z1) / 2), tik, 0.008)
    # Kuyunun kenarı: kapak kapanınca oturduğu çerçeve.
    for y in (-DER / 2 + 0.01, DER / 2 - 0.01):
        K("Kenar", (EN, 0.02, 0.02), (0, y, Z1 + 0.01), tik, 0.003)
    for x in (-EN / 2 + 0.01, EN / 2 - 0.01):
        K("KenarY", (0.02, DER - 0.04, 0.02), (x, 0, Z1 + 0.01), tik, 0.003)
    K("Ic", (EN - 0.04, DER - 0.04, 0.004), (0, 0, Z1 + 0.002), panel)
    # Konik, dışa açık dört ayak ve pirinç uçları.
    for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
        for y in (-DER / 2 + 0.06, DER / 2 - 0.06):
            cubuk((x, y, Z0), (x * 1.08, y * 1.15, 0.03), 0.017, tik, 10)
            cubuk((x * 1.08, y * 1.15, 0.03), (x * 1.085, y * 1.16, 0.0), 0.011, pirinc, 10)
    # Ön: iki yanda kumaşlı hoparlör ve dikey çıtalar; ortada plak bölmesi kapağı.
    for sx in (-1, 1):
        K("Hoparlor", (0.24, 0.004, 0.26), (sx * 0.26, -DER / 2 - 0.002, 0.33), kumas)
        for j in range(6):
            K("Cita", (0.012, 0.012, 0.26), (sx * 0.26 - 0.10 + j * 0.04, -DER / 2 - 0.006, 0.33), tik, 0.002)
    K("OrtaKapak", (0.24, 0.012, 0.26), (0, -DER / 2 - 0.006, 0.33), tik, 0.004)
    kure(0.012, (0, -DER / 2 - 0.016, 0.33), pirinc)
    # Pikap tablası: pikap solda, plak üstünde; sağda radyo kadranı ve düğmeler.
    S("Tabla", 0.15, 0.016, (-0.14, 0.02, Z1 + 0.012), alu, kose=40)
    S("Plak", 0.15, 0.003, (-0.14, 0.02, Z1 + 0.0215), plak, kose=40)
    for r in (0.13, 0.10, 0.075):
        halka(r, 0.0008, (-0.14, 0.02, Z1 + 0.023), (0, 0, 1), panel, seg=40)
    S("Etiket", 0.045, 0.004, (-0.14, 0.02, Z1 + 0.022), etiket, kose=24)
    S("Mil", 0.004, 0.02, (-0.14, 0.02, Z1 + 0.03), alu, kose=8)
    S("KolAyak", 0.02, 0.03, (0.10, 0.12, Z1 + 0.017), alu, kose=16)
    egri([(0.10, 0.12, Z1 + 0.035), (0.08, 0.02, Z1 + 0.038), (-0.02, -0.06, Z1 + 0.034)], 0.004, alu)
    K("Kafa", (0.03, 0.018, 0.01), (-0.03, -0.068, Z1 + 0.03), panel, 0.002)
    K("KadranCam", (0.20, 0.06, 0.006), (0.25, 0.08, Z1 + 0.005), kadran, 0.002)
    K("Ibre", (0.002, 0.05, 0.007), (0.21, 0.08, Z1 + 0.006), etiket)
    for i in range(4):
        S("Dugme", 0.017, 0.025, (0.18 + i * 0.05, -0.10, Z1 + 0.014), panel, kose=16)
    # Açık kapak: arka üst kenardan menteşeli, 100° kalkık (hafif arkaya yaslı); iç yüzü kumaş kaplı.
    kp = [K("Kapak", (EN, DER, 0.03), (0, 0, Z1 + 0.035), tik, 0.006),
          K("KapakIc", (EN - 0.08, DER - 0.08, 0.004), (0, 0, Z1 + 0.018), kumas)]
    for n in kp:
        donustur(n, don("X", math.radians(-100), (0, DER / 2, Z1 + 0.02)))
    ad, h, uzak = "Pikap_K6", 0.9, 1.7

elif hangi == "tv":
    ekran = ortak.malzeme("TV6_Ekran", (0.02, 0.025, 0.025), puruz=0.12)
    cerceve = ortak.malzeme("TV6_Cerceve", (0.50, 0.48, 0.42), puruz=0.35, metal=0.4)
    kumas = ortak.malzeme("TV6_Kumas", (0.10, 0.08, 0.05), puruz=0.95)
    bakalit = ortak.malzeme("TV6_Dugme", (0.02, 0.02, 0.02), puruz=0.35)
    dantel = ortak.malzeme("TV6_Dantel", (0.70, 0.66, 0.55), puruz=0.95)
    vazo = ortak.malzeme("TV6_Vazo", (0.55, 0.52, 0.45), puruz=0.2)
    EN, DER, Z0, Z1 = 0.70, 0.45, 0.22, 0.95
    K("Kasa", (EN, DER, Z1 - Z0), (0, 0, (Z0 + Z1) / 2), tik, 0.012)
    K("Arka", (EN - 0.2, 0.08, Z1 - Z0 - 0.15), (0, DER / 2 + 0.03, (Z0 + Z1) / 2), ceviz, 0.02)   # tüpün çıkıntısı
    for x in (-EN / 2 + 0.07, EN / 2 - 0.07):
        for y in (-DER / 2 + 0.07, DER / 2 - 0.07):
            cubuk((x, y, Z0), (x * 1.1, y * 1.12, 0.02), 0.018, ceviz, 10)
            cubuk((x * 1.1, y * 1.12, 0.02), (x * 1.105, y * 1.125, 0.0), 0.012, pirinc, 10)
    # Ekran: kavisli (iri pahlı) koyu cam, gümüşi çerçeve; solda.
    K("Cerceve", (0.50, 0.02, 0.42), (-0.08, -DER / 2 - 0.008, 0.61), cerceve, 0.02)
    K("Ekran", (0.44, 0.03, 0.35), (-0.08, -DER / 2 - 0.012, 0.61), ekran, 0.035)
    # Sağ sütun: kanal düğmesi (büyük), ses düğmesi, altta hoparlör kumaşı ve yatay çıtalar.
    disk(0.036, 0.03, (0.26, -DER / 2 - 0.015, 0.76), (0, -1, 0), bakalit, 24)
    K("Isaret", (0.006, 0.004, 0.03), (0.26, -DER / 2 - 0.032, 0.775), cerceve)
    disk(0.022, 0.025, (0.26, -DER / 2 - 0.012, 0.66), (0, -1, 0), bakalit, 20)
    K("Hoparlor", (0.12, 0.004, 0.20), (0.26, -DER / 2 - 0.002, 0.48), kumas)
    for j in range(6):
        K("Cita", (0.12, 0.01, 0.01), (0.26, -DER / 2 - 0.006, 0.40 + j * 0.032), tik, 0.002)
    K("Serit", (EN - 0.04, 0.012, 0.03), (0, -DER / 2 - 0.006, 0.33), cerceve, 0.004)   # markalık şerit
    K("AltKusak", (EN, 0.02, 0.05), (0, -DER / 2 - 0.004, Z0 + 0.025), ceviz, 0.004)
    # Üstte dantel ve küçük porselen vazo (70'lerin televizyon süsü).
    torna("Dantel", [(0.22, 0), (0.22, 0.002)], dantel, kose=36, konum=(0, 0, Z1), olcek=(1, 0.7))
    torna("Vazo", [(0.03, 0), (0.05, 0.04), (0.045, 0.10), (0.022, 0.15), (0.03, 0.17)], vazo, kose=20, konum=(0.18, 0.02, Z1 + 0.002))
    ad, h, uzak = "Eski_TV_K6", 1.0, 1.7

elif hangi == "teldolap":
    boya = ortak.malzeme("TelDolap_Boya", (0.30, 0.40, 0.32), puruz=0.6)     # solmuş nane yeşili
    tel = ortak.malzeme("TelDolap_Tel", (0.22, 0.22, 0.21), puruz=0.5, metal=0.8)
    ic = ortak.malzeme("TelDolap_Ic", (0.05, 0.05, 0.045), puruz=0.9)
    tabak = ortak.malzeme("TelDolap_Tabak", (0.70, 0.68, 0.62), puruz=0.3)
    EN, DER, H = 0.50, 0.35, 0.80
    x0, x1, y0, y1 = -EN / 2, EN / 2, -DER / 2, DER / 2
    for x in (x0 + 0.02, x1 - 0.02):
        for y in (y0 + 0.02, y1 - 0.02):
            K("Dikme", (0.035, 0.035, H - 0.04), (x, y, (H - 0.04) / 2), boya, 0.004)
    K("Ust", (EN + 0.04, DER + 0.03, 0.03), (0, 0, H - 0.015), boya, 0.006)
    K("UstPervaz", (EN + 0.02, 0.02, 0.02), (0, y0 - 0.01, H - 0.04), boya, 0.005)
    K("Alt", (EN - 0.02, DER - 0.02, 0.02), (0, 0, 0.09), boya, 0.003)
    K("Ara", (EN - 0.02, DER - 0.02, 0.02), (0, 0, 0.23), boya, 0.003)
    K("Arka", (EN - 0.04, 0.012, H - 0.30), (0, y1 - 0.01, 0.50), ic)
    K("IcRaf", (EN - 0.05, DER - 0.06, 0.015), (0, 0.01, 0.50), boya, 0.002)
    for i in range(2):
        S("Tabak", 0.09, 0.012, (-0.10 + i * 0.2, 0.02, 0.514), tabak, kose=24)
    # Çekmece önü (süs, açılmaz) ve topuzu.
    K("Cekmece", (EN - 0.06, 0.018, 0.11), (0, y0 - 0.005, 0.16), boya, 0.004)
    kure(0.012, (0, y0 - 0.02, 0.16), ceviz)
    # İki kapak: çerçeve + tel örgü (ızgara çubuklar); ortada döner mandal.
    zk0, zk1 = 0.245, 0.755
    for sx in (-1, 1):
        xa, xb = (x0 + 0.01, -0.003) if sx < 0 else (0.003, x1 - 0.01)
        K("KapiS", (0.03, 0.02, zk1 - zk0), (xa + 0.015, y0 - 0.005, (zk0 + zk1) / 2), boya, 0.003)
        K("KapiS", (0.03, 0.02, zk1 - zk0), (xb - 0.015, y0 - 0.005, (zk0 + zk1) / 2), boya, 0.003)
        K("KapiR", (xb - xa, 0.02, 0.03), ((xa + xb) / 2, y0 - 0.005, zk0 + 0.015), boya, 0.003)
        K("KapiR", (xb - xa, 0.02, 0.03), ((xa + xb) / 2, y0 - 0.005, zk1 - 0.015), boya, 0.003)
        xi0, xi1, zi0, zi1 = xa + 0.03, xb - 0.03, zk0 + 0.03, zk1 - 0.03
        nx = int((xi1 - xi0) / 0.018)
        for i in range(1, nx + 1):
            x = xi0 + i * (xi1 - xi0) / (nx + 1)
            K("TelD", (0.0025, 0.0025, zi1 - zi0), (x, y0 - 0.003, (zi0 + zi1) / 2), tel)
        nz = int((zi1 - zi0) / 0.018)
        for i in range(1, nz + 1):
            z = zi0 + i * (zi1 - zi0) / (nz + 1)
            K("TelY", (xi1 - xi0, 0.0025, 0.0025), ((xi0 + xi1) / 2, y0 - 0.0055, z), tel)
        kure(0.010, (sx * 0.035, y0 - 0.02, 0.50), ceviz)
    K("Mandal", (0.012, 0.01, 0.05), (0, y0 - 0.018, 0.62), ceviz, 0.002)
    # Yanlar: üst bölmede tel örgü pencere.
    for sx in (-1, 1):
        x = sx * (EN / 2 - 0.005)
        for i in range(1, 15):
            y = y0 + 0.04 + i * (DER - 0.08) / 15
            K("YanTel", (0.0025, 0.0025, 0.46), (x, y, 0.50), tel)
        for i in range(1, 24):
            K("YanTelY", (0.0025, DER - 0.07, 0.0025), (x + sx * 0.003, 0, 0.26 + i * 0.02), tel)
    ad, h, uzak = "Tel_Dolap_K6", 0.8, 1.4

elif hangi == "ocak":
    emaye = ortak.malzeme("Ocak6_Emaye", (0.22, 0.40, 0.36), puruz=0.3)       # açık turkuaz emaye
    beyaz = ortak.malzeme("Ocak6_Beyaz", (0.75, 0.74, 0.70), puruz=0.3)
    demir = ortak.malzeme("Ocak6_Demir", (0.025, 0.025, 0.025), puruz=0.6, metal=0.5)
    bakalit = ortak.malzeme("Ocak6_Bakalit", (0.02, 0.02, 0.02), puruz=0.35)
    leke = ortak.malzeme("Ocak6_Leke", (0.12, 0.07, 0.03), puruz=0.6)
    sehpa = ortak.malzeme("Ocak6_Sehpa", (0.08, 0.10, 0.07), puruz=0.5, metal=0.5)   # boyalı demir sehpa
    tup = ortak.malzeme("Ocak6_Tup", (0.40, 0.25, 0.04), puruz=0.45)
    alu = ortak.malzeme("Ocak6_Alu", (0.45, 0.45, 0.43), puruz=0.45, metal=0.9)
    hortum = ortak.malzeme("Ocak6_Hortum", (0.06, 0.06, 0.06), puruz=0.6)
    EN, DER, HS = 0.60, 0.45, 0.75
    # Demir sehpa: dört köşe ayak, üst tabla (ahşap), alt raf.
    for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
        for y in (-DER / 2 + 0.02, DER / 2 - 0.02):
            K("Ayak", (0.025, 0.025, HS - 0.02), (x, y, (HS - 0.02) / 2), sehpa, 0.003)
    K("Tabla", (EN, DER, 0.02), (0, 0, HS - 0.01), tik, 0.004)
    K("AltRaf", (EN - 0.02, DER - 0.02, 0.012), (0, 0, 0.16), sehpa, 0.002)
    for y in (-DER / 2 + 0.02, DER / 2 - 0.02):
        K("Kusak", (EN - 0.04, 0.012, 0.04), (0, y, HS - 0.04), sehpa, 0.002)
    # İki gözlü emaye ocak: gövde, beyaz üst kapak, iki göz (bek + ızgara), önde iki bakalit düğme.
    OZ = HS
    K("OcakGovde", (0.52, 0.36, 0.08), (0, 0.0, OZ + 0.04), emaye, 0.012)
    K("OcakUst", (0.50, 0.34, 0.006), (0, 0.0, OZ + 0.083), beyaz, 0.003)
    K("OnSerit", (0.52, 0.004, 0.012), (0, -0.182, OZ + 0.06), beyaz)
    for x in (-0.13, 0.13):
        S("Bek", 0.045, 0.012, (x, 0.0, OZ + 0.092), demir, kose=24)
        S("BekKapak", 0.03, 0.008, (x, 0.0, OZ + 0.102), demir, kose=20)
        halka(0.085, 0.006, (x, 0.0, OZ + 0.110), (0, 0, 1), demir, seg=24)
        for k in range(4):
            a = math.radians(45 + 90 * k)
            cubuk((x + 0.03 * math.cos(a), 0.03 * math.sin(a), OZ + 0.112), (x + 0.1 * math.cos(a), 0.1 * math.sin(a), OZ + 0.112), 0.005, demir, 6)
            K("IzgaraAyak", (0.01, 0.01, 0.022), (x + 0.095 * math.cos(a), 0.095 * math.sin(a), OZ + 0.098), demir)
        disk(0.02, 0.025, (x * 0.8, -0.192, OZ + 0.04), (0, -1, 0), bakalit, 16)
        K("DugmeIsaret", (0.004, 0.004, 0.018), (x * 0.8, -0.206, OZ + 0.048), beyaz)
    torna("Leke", [(0.05, 0), (0.05, 0.0008)], leke, kose=14, konum=(0.02, 0.08, OZ + 0.086), olcek=(1.6, 0.7))
    # Sol gözde eğri büğrü alüminyum cezve.
    torna("Cezve", [(0.035, 0), (0.042, 0.03), (0.036, 0.07), (0.04, 0.085)], alu, kose=20, konum=(-0.13, 0.0, OZ + 0.117), kalinlik=0.002, ust=False)
    cubuk((-0.13 - 0.035, 0.0, OZ + 0.19), (-0.13 - 0.16, -0.02, OZ + 0.215), 0.006, alu, 8)
    # Alttaki tüp ve ocağa çıkan hortum.
    S("Tup", 0.11, 0.30, (0.05, 0.03, 0.166 + 0.15), tup, kose=28, pah=0.03)
    torna("TupOmuz", [(0.11, 0.0), (0.09, 0.03), (0.05, 0.05)], tup, kose=24, konum=(0.05, 0.03, 0.466))
    S("Regulator", 0.028, 0.04, (0.05, 0.03, 0.535), alu, kose=14)
    halka(0.08, 0.009, (0.05, 0.03, 0.47), (0, 0, 1), tup, seg=24)
    egri([(0.078, 0.03, 0.54), (0.20, 0.06, 0.58), (0.27, 0.09, 0.68), (0.27, 0.08, OZ + 0.03), (0.26, 0.06, OZ + 0.04)], 0.007, hortum, 8)
    ad, h, uzak = "Ocak_K6", 0.95, 1.5

elif hangi == "masa":
    cam = ortak.malzeme("Masa6_Bardak", (0.22, 0.20, 0.16), puruz=0.08)
    emaye = ortak.malzeme("Caydanlik6_Emaye", (0.72, 0.70, 0.64), puruz=0.25)
    mavi = ortak.malzeme("Caydanlik6_Mavi", (0.03, 0.06, 0.20), puruz=0.3)
    cicek = ortak.malzeme("Caydanlik6_Cicek", (0.40, 0.06, 0.05), puruz=0.4)
    cam_t = ortak.malzeme("Masa6_Tabak", (0.65, 0.62, 0.55), puruz=0.3)
    cam_m = ortak.malzeme("Masa6_Cam", (0.20, 0.12, 0.05), puruz=0.5)
    EN, DER, H = 0.80, 0.60, 0.75
    K("Tabla", (EN, DER, 0.03), (0, 0, H - 0.015), cam_m, 0.006)
    for x in (-EN / 2 + 0.035, EN / 2 - 0.035):
        for y in (-DER / 2 + 0.035, DER / 2 - 0.035):
            # Aşağı doğru incelen kare ayak.
            v, f = [], []
            for zz, s in ((0.0, 0.016), (H - 0.03, 0.022)):
                for (dx, dy) in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
                    v.append((x + dx * s, y + dy * s, zz))
            yeni_mesh("Ayak", v, [(0, 1, 2, 3), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)], cam_m)
    for y in (-DER / 2 + 0.035, DER / 2 - 0.035):
        K("Kusak", (EN - 0.1, 0.02, 0.08), (0, y, H - 0.07), cam_m, 0.003)
    for x in (-EN / 2 + 0.035, EN / 2 - 0.035):
        K("KusakY", (0.02, DER - 0.1, 0.08), (x, 0, H - 0.07), cam_m, 0.003)
    K("Cekmece", (0.30, 0.012, 0.06), (0, -DER / 2 + 0.023, H - 0.07), cam_m, 0.003)
    kure(0.01, (0, -DER / 2 + 0.012, H - 0.07), pirinc)
    # Emaye çaydanlık (altta su kazanı, üstünde demlik), mavi kenarlı, kırmızı çiçekli.
    cx, cy, z = 0.14, 0.06, H
    torna("Kazan", [(0.07, 0), (0.095, 0.012), (0.10, 0.05), (0.09, 0.105), (0.065, 0.125)], emaye, kose=32, konum=(cx, cy, z))
    halka(0.066, 0.004, (cx, cy, z + 0.125), (0, 0, 1), mavi, seg=28)
    halka(0.072, 0.004, (cx, cy, z + 0.004), (0, 0, 1), mavi, seg=28)
    torna("Demlik", [(0.045, 0.125), (0.062, 0.14), (0.066, 0.17), (0.05, 0.205), (0.035, 0.212)], emaye, kose=28, konum=(cx, cy, z))
    torna("DemlikKapak", [(0.037, 0.212), (0.03, 0.225), (0.008, 0.232)], mavi, kose=20, konum=(cx, cy, z))
    kure(0.011, (cx, cy, z + 0.240), mavi)
    for i in range(5):
        a = math.radians(i * 72 + 20)
        disk(0.012, 0.002, (cx + 0.1 * math.cos(a), cy + 0.1 * math.sin(a), z + 0.06), (math.cos(a), math.sin(a), 0), cicek, 10)
    cubuk((cx - 0.085, cy, z + 0.05), (cx - 0.16, cy - 0.01, z + 0.12), 0.012, emaye, 12)       # kazan emziği
    cubuk((cx - 0.055, cy, z + 0.16), (cx - 0.11, cy, z + 0.20), 0.008, emaye, 10)              # demlik emziği
    egri([(cx + 0.09, cy, z + 0.10), (cx + 0.13, cy, z + 0.09), (cx + 0.13, cy, z + 0.04), (cx + 0.095, cy, z + 0.03)], 0.007, mavi)
    egri([(cx + 0.058, cy, z + 0.19), (cx + 0.09, cy, z + 0.185), (cx + 0.085, cy, z + 0.145), (cx + 0.062, cy, z + 0.14)], 0.006, mavi)
    # Ters kapatılmış ince belli çay bardağı ve tabağı; tozlu.
    S("Tabak", 0.055, 0.006, (-0.15, -0.08, z + 0.003), cam_t, kose=24)
    torna("Bardak", [(0.03, 0.006), (0.029, 0.03), (0.022, 0.05), (0.026, 0.085), (0.02, 0.092)], cam, kose=20, konum=(-0.15, -0.08, z))
    ad, h, uzak = "Mutfak_Masasi_K6", 0.95, 1.5

elif hangi == "raf":
    kagit = ortak.malzeme("Raf6_Kagit", (0.66, 0.60, 0.46), puruz=0.9)
    cam = ortak.malzeme("Raf6_Cam", (0.16, 0.20, 0.18), puruz=0.05)
    kapak = ortak.malzeme("Raf6_Kapak", (0.40, 0.30, 0.08), puruz=0.5, metal=0.7)
    tozm = ortak.malzeme("Raf6_Toz", (0.14, 0.105, 0.075), puruz=1.0)
    arka = ortak.malzeme("Raf6_Arka", (0.035, 0.020, 0.010), puruz=0.8)
    EN, DER, H = 1.00, 0.35, 1.60
    for x in (-EN / 2 + 0.01, EN / 2 - 0.01):
        K("Yan", (0.02, DER, H), (x, 0, H / 2), tik, 0.004)
    K("Ust", (EN + 0.02, DER + 0.01, 0.025), (0, 0, H + 0.0125), tik, 0.005)
    K("Arka", (EN - 0.04, 0.008, H - 0.02), (0, DER / 2 - 0.004, H / 2), arka)
    K("Supurgelik", (EN - 0.04, 0.02, 0.08), (0, -DER / 2 + 0.02, 0.04), tik, 0.003)
    raflar = (0.10, 0.45, 0.80, 1.15)
    for z in raflar + (H,):
        if z < H:
            K("Raf", (EN - 0.04, DER - 0.01, 0.02), (0, 0.0, z - 0.01), tik, 0.003)
        K("Toz", (EN - 0.06, DER - 0.05, 0.0012), (0, 0.01, z + 0.0006 + (0.025 if z == H else 0)), tozm)
    # Fırfırlı raf kâğıtları: rafın ön kenarından sarkan, dişli kenarlı sararmış kâğıt.
    for z in raflar[1:]:
        K("RafKagidi", (EN - 0.05, 0.002, 0.03), (0, -DER / 2 + 0.003, z - 0.015), kagit)
        for i in range(24):
            x = -EN / 2 + 0.045 + i * (EN - 0.09) / 23
            prizma("Dis", [(x - 0.018, z - 0.03), (x + 0.018, z - 0.03), (x, z - 0.05)], -DER / 2 + 0.002, -DER / 2 + 0.004, kagit, "xz")
    # İki boş kavanoz: biri kapaklı (orta rafta), biri kapaksız, kapağı yanında (üst rafta).
    torna("Kavanoz", [(0.045, 0), (0.05, 0.01), (0.05, 0.13), (0.04, 0.15), (0.04, 0.165)], cam, kose=24, konum=(-0.28, 0.02, 0.80), kalinlik=0.003, ust=False)
    S("KavKapak", 0.043, 0.02, (-0.28, 0.02, 0.975), kapak, kose=24)
    torna("Kavanoz", [(0.055, 0), (0.06, 0.01), (0.06, 0.18), (0.048, 0.2), (0.048, 0.215)], cam, kose=24, konum=(0.22, 0.0, 1.15), kalinlik=0.003, ust=False)
    S("KavKapak", 0.051, 0.02, (0.34, -0.02, 1.16), kapak, kose=24)
    ad, h, uzak = "Bos_Raf_K6", 1.6, 2.4

elif hangi == "karyola":
    demir = ortak.malzeme("Karyola6_Demir", (0.015, 0.015, 0.016), puruz=0.45, metal=0.6)
    tel = ortak.malzeme("Karyola6_Tel", (0.10, 0.10, 0.10), puruz=0.5, metal=0.7)
    dosek = ortak.malzeme("Karyola6_Dosek", (0.52, 0.49, 0.42), puruz=0.95)
    cizgi = ortak.malzeme("Karyola6_Cizgi", (0.10, 0.14, 0.28), puruz=0.95)
    leke = ortak.malzeme("Karyola6_Leke", (0.42, 0.34, 0.20), puruz=0.95)
    yorgan = ortak.malzeme("Karyola6_Yorgan", (0.32, 0.10, 0.12), puruz=0.5)        # soluk gül rengi saten
    EN, BOY = 1.40, 2.00
    X, Y = EN / 2, BOY / 2 - 0.02
    # Dört direk, tepelerinde pirinç topuz ve bilezik.
    for x in (-X, X):
        for (y, hz) in ((Y, 1.03), (-Y, 0.80)):
            cubuk((x, y, 0.0), (x, y, hz), 0.022, demir, 12)
            S("Bilezik", 0.028, 0.02, (x, y, hz - 0.12), pirinc, kose=16)
            kure(0.038, (x, y, hz + 0.035), pirinc, seg=16)
            S("Pabuc", 0.028, 0.02, (x, y, 0.01), pirinc, kose=16)
    # Başlık (+y) ve ayak ucu (-y): kemerli üst boru, düz alt boru, dikey çubuklar; ortada pirinç rozetler.
    for (y, zalt, zust, kemer, nc) in ((Y, 0.55, 0.96, 0.08, 9), (-Y, 0.45, 0.74, 0.05, 7)):
        cubuk((-X, y, zalt), (X, y, zalt), 0.014, demir, 10)
        nk = [(-X + 2 * X * i / 12, y, zust + kemer * math.sin(math.pi * i / 12)) for i in range(13)]
        egri(nk, 0.016, demir, 10)
        for i in range(1, nc + 1):
            x = -X + 2 * X * i / (nc + 1)
            zt = zust + kemer * math.sin(math.pi * (x + X) / (2 * X))
            cubuk((x, y, zalt), (x, y, zt), 0.008, demir, 8)
            if i % 2 == 1:
                kure(0.016, (x, y, (zalt + zt) / 2), pirinc, seg=10)
        halka(0.06, 0.008, (0, y, (zalt + zust) / 2 + kemer * 0.5), (0, 1, 0), pirinc, seg=24)
    # Yan köşebentler ve telli somya.
    for x in (-X + 0.02, X - 0.02):
        K("Kosebent", (0.04, 2 * Y, 0.04), (x, 0, 0.33), demir, 0.002)
    K("Somya", (EN - 0.08, 2 * Y - 0.04, 0.008), (0, 0, 0.345), tel)
    for i in range(9):
        K("SomyaTel", (EN - 0.08, 0.006, 0.006), (0, -Y + 0.1 + i * (2 * Y - 0.2) / 8, 0.34), demir)
    # Çıplak döşek: çizgili kumaş, kenar biyesi, sararmış leke.
    K("Dosek", (EN - 0.06, 2 * Y - 0.06, 0.18), (0, 0, 0.44), dosek, 0.03)
    for i in range(13):
        x = -X + 0.08 + i * (EN - 0.16) / 12
        K("Cizgi", (0.012, 2 * Y - 0.12, 0.001), (x, 0, 0.5305), cizgi)
    torna("Leke", [(0.14, 0), (0.14, 0.0008)], leke, kose=16, konum=(0.25, 0.2, 0.5312), olcek=(1.3, 0.8))
    # Ayak ucunda katlı yorgan: üç kat, kenarları hafif kayık.
    for i in range(3):
        K("Yorgan", (0.95 - i * 0.01, 0.46, 0.04), (0.02 * (i - 1), -0.62 + 0.01 * i, 0.555 + i * 0.04), yorgan, 0.015)
    ad, h, uzak = "Karyola_K6", 1.1, 3.2

else:   # sandik: AÇILIR çeyiz sandığı (Govde + Kapak)
    acik = True
    oyma = ortak.malzeme("Sandik6_Oyma", (0.12, 0.055, 0.022), puruz=0.55)
    sandik = ortak.malzeme("Sandik6_Ahsap", (0.075, 0.032, 0.013), puruz=0.5)
    ic = ortak.malzeme("Sandik6_Ic", (0.16, 0.10, 0.05), puruz=0.8)
    EN, DER, H = 0.90, 0.45, 0.465          # gövde (kapak ayrı; kapak üstü 0,50)
    T = 0.02                                 # tahta kalınlığı
    x0, x1, y0, y1 = -EN / 2, EN / 2, -DER / 2, DER / 2
    TAB = 0.08                               # iç taban üst yüzü
    lev = ac.levha
    p += [lev("Kaide", x0 - 0.01, x1 + 0.01, y0 - 0.01, y1 + 0.01, 0.0, 0.06, sandik, 0.006),
          lev("Taban", x0 + T, x1 - T, y0 + T, y1 - T, 0.06, TAB, ic, 0.0),
          lev("On", x0, x1, y0, y0 + T, 0.06, H, sandik),
          lev("Arka", x0, x1, y1 - T, y1, 0.06, H, sandik),
          lev("Yan", x0, x0 + T, y0 + T, y1 - T, 0.06, H, sandik),
          lev("Yan", x1 - T, x1, y0 + T, y1 - T, 0.06, H, sandik),
          lev("Kusak", x0 - 0.005, x1 + 0.005, y0 - 0.008, y0, 0.06, 0.085, oyma, 0.003)]
    # Ön yüz oyması: iki kabarık pano, her birinde sekiz yapraklı gül; ortada dikey burma şerit.
    for sx in (-1, 1):
        cx = sx * 0.215
        p.append(lev("Pano", cx - 0.17, cx + 0.17, y0 - 0.008, y0, 0.12, 0.40, oyma, 0.004))
        p.append(lev("PanoIc", cx - 0.145, cx + 0.145, y0 - 0.012, y0 - 0.008, 0.145, 0.375, sandik, 0.002))
        disk(0.04, 0.008, (cx, y0 - 0.016, 0.26), (0, -1, 0), oyma, 20)
        for k in range(8):
            a = math.radians(k * 45)
            n = K("Yaprak", (0.018, 0.006, 0.05), (cx, y0 - 0.014, 0.26 + 0.07), oyma, 0.003)
            donustur(n, don("Y", a, (cx, y0 - 0.014, 0.26)))
        for (qx, qz) in ((-0.13, 0.155), (0.13, 0.155), (-0.13, 0.365), (0.13, 0.365)):
            kure(0.01, (cx + qx, y0 - 0.013, qz), oyma, seg=8)
    for i in range(9):
        z = 0.13 + i * 0.032
        n = K("Burma", (0.03, 0.008, 0.014), (0, y0 - 0.006, z), oyma, 0.003)
        donustur(n, don("Y", math.radians(30 if i % 2 == 0 else -30), (0, y0 - 0.006, z)))
    # Pirinç anahtar aynası (kilit deliği ön üst ortada), yan tutamaklar, arkada menteşeler.
    K("Ayna", (0.05, 0.004, 0.07), (0, y0 - 0.012, 0.425), pirinc, 0.002)
    K("Delik", (0.006, 0.002, 0.018), (0, y0 - 0.0145, 0.425), ic)
    for sx in (-1, 1):
        K("TutamakPlaka", (0.004, 0.06, 0.04), (sx * (x1 + 0.002), 0, 0.36), pirinc, 0.001)
        halka(0.03, 0.005, (sx * (x1 + 0.008), 0, 0.33), (1, 0, 0), pirinc, seg=16)
    for x in (-0.28, 0.28):
        S("Mentese", 0.007, 0.07, (x, y1 + 0.002, H + 0.008), pirinc, kose=10)
        donustur(p[-1], don("Y", math.pi / 2, (x, y1 + 0.002, H + 0.008)))
    # Kapak: üst tabla + dört yanda etek (gövdenin dışına biner) + oymalı orta pano + pirinç köşelikler.
    kp = [lev("KapakTabla", x0 - 0.015, x1 + 0.015, y0 - 0.015, y1 + 0.015, 0.47, 0.50, sandik, 0.006),
          lev("Etek", x0 - 0.015, x1 + 0.015, y0 - 0.015, y0 - 0.003, 0.452, 0.47, sandik, 0.003),
          lev("Etek", x0 - 0.015, x1 + 0.015, y1 + 0.003, y1 + 0.015, 0.452, 0.47, sandik, 0.003),
          lev("Etek", x0 - 0.015, x0 - 0.003, y0 - 0.003, y1 + 0.003, 0.452, 0.47, sandik, 0.003),
          lev("Etek", x1 + 0.003, x1 + 0.015, y0 - 0.003, y1 + 0.003, 0.452, 0.47, sandik, 0.003),
          lev("KapakPano", -0.34, 0.34, -0.15, 0.15, 0.50, 0.508, oyma, 0.004)]
    for sx in (-1, 1):
        for sy in (-1, 1):
            kp.append(lev("Koselik", sx * (x1 + 0.015) - sx * 0.06 if sx > 0 else x0 - 0.015, sx * (x1 + 0.015) if sx > 0 else x0 - 0.015 + 0.06,
                          sy * (y1 + 0.015) - 0.06 if sy > 0 else y0 - 0.015, sy * (y1 + 0.015) if sy > 0 else y0 - 0.015 + 0.06,
                          0.50, 0.503, pirinc, 0.001))
    kapak = ortak.birlestir(kp, "Kapak")
    ortak.pivotu_tasi(kapak, (0, y1 + 0.015, 0.50))       # arka üst kenar: menteşe çizgisi
    parcalar.append(kapak)
    ad, h, uzak = "Ceyiz_Sandigi_K6", 0.5, 1.5

# Önizlemede kaldırma: tavan eşyası ve perde aşağı sarkar, duvar eşyasının yarısı zemin altındadır.
kaldir = {"avize": 0.7, "perde": 2.32, "takvim": 0.28}.get(hangi, 0.0)
if acik:
    govde = ortak.birlestir(p, "Govde")
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, uzak)
else:
    nesne = ortak.birlestir(p, ad)
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=kaldir)
