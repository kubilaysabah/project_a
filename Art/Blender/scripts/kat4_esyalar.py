# kat4_esyalar.py
# 4. kattaki boş, elektriksiz 4 numaralı dairenin eşyaları (#83). Önceki kapıcı Hasan elektriği kesip
# sigortayı saklamış, bir süre bu dairede karanlıkta geceler geçirmiş: kamp yatağı, mumlar, gaz lambası.
# Ev boşaltılmış, tozlu, terk edilmiş; çok az eşya var. Katlarda aynı model yok: hepsi bu kata özgü.
#   blender -b --factory-startup --python kat4_esyalar.py -- <ad> [--onizleme a.png] [--ic b.png]
#   (--ic: açılır eşyalarda kapı/kapak açık ikinci bir önizleme; yalnızca kontrol için, FBX'e girmez)
#
# ANTRE
#   gazete    Gazete_Yigini_K4   : kapının altından atılmış sararmış gazeteler ve açılmamış zarflar (yer).
# HOL
#   kablo     Sarkan_Kablo_K4    : tavandan sarkan çıplak bükümlü kablo, ucunda ampulsüz boş duy (tavan).
#   koli      Bos_Koliler_K4     : duvara dayalı iki yassı koli ve yerde bir buruşuk koli
#                                  (pivot: yerde, duvar çizgisinde; eşya -y'ye uzanır).
# SALON
#   koltuk    Ortulu_Koltuk_K4   : tozlu beyaz çarşafla örtülü berjer; çarşaf yere sarkar (yer).
#   mumlar    Mumlar_K4          : emaye tabakta erimiş mumlar, kibrit kutusu, yanık kibritler (yer).
#   sehpa     Yikik_Sehpa_K4     : bir ayağı kırık, yana devrilmiş küçük sehpa, kopan ayak yanında (yer).
# MUTFAK
#   buzdolabi Buzdolabi_K4       : AÇILIR. 70-80'ler krem, yuvarlak üstlü, krom kollu tek kapılı buzdolabı;
#                                  içi boş ve oyuk, tel raflar, üstte buzluk, sağ üstte iç lamba yuvası.
#   kova      Kova_Paspas_K4     : içinde kirli su olan galvaniz kova, kenarına dayalı paspas (yer).
#   sandalye  Kirik_Sandalye_K4  : yan yatmış, bir ayağı kopuk ahşap sandalye; kopan ayak yanında (yer).
# ODA B (Hasan'ın gece geçirdiği oda)
#   kamp      Kamp_Yatagi_K4     : katlanır yeşil brandalı kamp yatağı, üstünde ince gri battaniye (yer).
#   alet      Alet_Kutusu_K4     : AÇILIR. 90'lar kırmızı metal alet kutusu, kapağı arkadan menteşeli (yer).
#   termos    Termos_K4          : termos ve ince belli çay bardağı (yer).
#   lamba     Gaz_Lambasi_K4     : sönük lüks lambası (basınçlı gaz lambası) (yer).
#   tebesir   Duvar_Tebesir_K4   : duvarda tebeşirle gün sayma çentikleri (duvar).
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası,
# tavan eşyasında tepe noktası.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import bmesh
import mathutils
from mathutils import Matrix, Vector

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "mumlar"
ortak.sahneyi_temizle()
rnd = random.Random(44)       # sabit tohum: betik her çalıştığında aynı "rastgele" dağınıklık
p = []                        # bu eşyanın parçaları (sonunda tek nesnede birleşir)


# ---------- YARDIMCILAR ----------

def sec(n):
    """Yalnızca n seçili ve etkin olsun (Blender komutları seçili nesneye uygulanır)."""
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n


def dondur(n, rot, merkez):
    """
    Nesneyi verilen merkez etrafında döndürür, dönüşü doğrudan meshe yazar.
    DİKKAT: ortak.kutu konumu meshe işler ve pivotu dünya merkezinde bırakır; "kendi pivotu etrafında
    döndür" demek kutuyu dünya merkezi etrafında savurur. Bu yüzden merkezi her zaman açıkça veriyoruz.
    """
    n.data.transform(n.matrix_world)
    n.matrix_world = Matrix.Identity(4)
    c = Vector(merkez)
    n.data.transform(Matrix.Translation(c) @ mathutils.Euler(rot).to_matrix().to_4x4() @ Matrix.Translation(-c))
    n.data.update()


def K(ad, olcu, konum, mat, pah=0.0, rot=None):
    """Kutu ekle (isteğe bağlı kendi merkezi etrafında döndürerek) ve parça listesine koy."""
    n = ortak.kutu(ad, olcu, konum, mat, pah)
    if rot:
        dondur(n, rot, konum)
    p.append(n)
    return n


def S(ad, r, boy, konum, mat, kose=24, pah=0.0, rot=None):
    """Silindir ekle (dik; rot verilirse kendi merkezi etrafında döndürülür) ve listeye koy."""
    n = ortak.silindir(ad, r, boy, konum, mat, kose, pah)
    if rot:
        dondur(n, rot, konum)
    p.append(n)
    return n


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk (boru, tel, sap)."""
    d = Vector(b) - Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((Vector(a) + Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def yol(noktalar, r, mat, kose=6):
    """Nokta dizisinden geçen kırık çizgi boru (kablo, tel sap). Eklem yerlerine küçük küre koyar."""
    for a, b in zip(noktalar, noktalar[1:]):
        cubuk(a, b, r, mat, kose)
    for q in noktalar[1:-1]:
        kure(r, q, mat, seg=kose)


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(4, seg // 2 + 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def mesh_nesnesi(ad, verts, faces, mat):
    """Köşe ve yüz listesinden nesne kurar; yüz yönlerini dışa çevirir."""
    me = bpy.data.meshes.new(ad)
    me.from_pydata(verts, [], faces)
    me.update()
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)   # yüzlerin "önü" dışarı baksın
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(ad, me)
    bpy.context.collection.objects.link(ob)
    ortak.ata(ob, mat)
    p.append(ob)
    return ob


def torna(ad, profil, konum, mat, kose=24):
    """
    TORNA (döndürme): (yarıçap, yükseklik) noktalarından oluşan bir profili z ekseni etrafında
    döndürüp gövde yapar; bardak, kova, termos, tabak gibi yuvarlak şeyler için. Yarıçapı 0 olan
    nokta eksendeki tek köşe olur (kapalı dip/tepe). konum: eksenin tabandaki noktası.
    """
    cx, cy, cz = konum
    verts, halkalar = [], []
    for (r, z) in profil:
        if r < 1e-6:
            halkalar.append([len(verts)])
            verts.append((cx, cy, cz + z))
        else:
            h = []
            for i in range(kose):
                a = 2 * math.pi * i / kose
                h.append(len(verts))
                verts.append((cx + r * math.cos(a), cy + r * math.sin(a), cz + z))
            halkalar.append(h)
    faces = []
    for A, B in zip(halkalar, halkalar[1:]):
        if len(A) == 1 and len(B) == 1:
            continue
        if len(A) == 1:
            faces += [(A[0], B[(i + 1) % kose], B[i]) for i in range(kose)]
        elif len(B) == 1:
            faces += [(A[i], A[(i + 1) % kose], B[0]) for i in range(kose)]
        else:
            faces += [(A[i], A[(i + 1) % kose], B[(i + 1) % kose], B[i]) for i in range(kose)]
    return mesh_nesnesi(ad, verts, faces, mat)


def yuzey(ad, nu, nv, f, mat, kalin=0.004):
    """
    Izgara yüzey: f(u, v) -> (x, y, z), u ve v 0..1. Kumaş, branda, battaniye, eğik kâğıt gibi
    bükülen ince şeyler için. kalin: SOLIDIFY (kalınlaştır) değiştiricisiyle verilen et kalınlığı.
    """
    verts = [f(i / nu, j / nv) for j in range(nv + 1) for i in range(nu + 1)]
    w = nu + 1
    faces = [(j * w + i, j * w + i + 1, (j + 1) * w + i + 1, (j + 1) * w + i) for j in range(nv) for i in range(nu)]
    ob = mesh_nesnesi(ad, verts, faces, mat)
    if kalin > 0:
        m = ob.modifiers.new("Kalin", "SOLIDIFY")
        m.thickness = kalin
        m.offset = 0.0
    return ob


def birles(liste, ad):
    """ortak.birlestir'in tek parça da kabul eden hali (tek nesnede join çalışmaz)."""
    if len(liste) == 1:
        n = liste[0]
        sec(n)
        for mod in list(n.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)
        n.name = ad
        n.data.name = ad
        return n
    return ortak.birlestir(liste, ad)


def dunyaya_isle(n):
    """Nesnenin konum/dönüşünü meshe işler: köşe koordinatları artık doğrudan dünya koordinatı."""
    n.data.transform(n.matrix_world)
    n.matrix_world = Matrix.Identity(4)


def sinirlar(n):
    xs = [v.co.x for v in n.data.vertices]
    ys = [v.co.y for v in n.data.vertices]
    zs = [v.co.z for v in n.data.vertices]
    return min(xs), max(xs), min(ys), max(ys), min(zs), max(zs)


def grup(bas, donus=Matrix.Identity(4), yere=False):
    """
    p[bas:] parçalarını tek nesnede birleştirir ve bir dönüşüm uygular (ör. devrilen sehpa, kâğıt
    yığınındaki eğik gazete). yere=True ise dönüşten sonra en alt noktası z = 0'a indirilir.
    """
    parca = p[bas:]
    del p[bas:]
    n = birles(parca, "Grup")
    dunyaya_isle(n)
    n.data.transform(donus)
    if yere:
        z0 = sinirlar(n)[4]
        n.data.transform(Matrix.Translation((0, 0, -z0)))
    n.data.update()
    p.append(n)
    return n


def Rx(d): return Matrix.Rotation(math.radians(d), 4, "X")
def Ry(d): return Matrix.Rotation(math.radians(d), 4, "Y")
def Rz(d): return Matrix.Rotation(math.radians(d), 4, "Z")
def T(x, y, z): return Matrix.Translation((x, y, z))


def yerlestir(n, mod):
    """
    Pivot kuralı: 'yer' taban ortası (xy ortası, alt z = 0); 'dayali' yerde duvar çizgisi
    (x ortası, arka y = 0, alt z = 0); 'duvar' arka yüz ortası (x ve z ortası, arka y = 0);
    'tavan' tepe (xy ortası, üst z = 0).
    """
    dunyaya_isle(n)
    x0, x1, y0, y1, z0, z1 = sinirlar(n)
    if mod == "yer":
        t = (-(x0 + x1) / 2, -(y0 + y1) / 2, -z0)
    elif mod == "dayali":
        t = (-(x0 + x1) / 2, -y1, -z0)
    elif mod == "duvar":
        t = (-(x0 + x1) / 2, -y1, -(z0 + z1) / 2)
    else:
        t = (-(x0 + x1) / 2, -(y0 + y1) / 2, -z1)
    n.data.transform(Matrix.Translation(t))
    n.data.update()


def kirik_uc(x, y, z, yon, mat, kal=0.03):
    """Kırılmış tahta ucu: yon (+1 yukarı / -1 aşağı) doğrultusunda farklı boylarda kıymıklar."""
    for i in range(4):
        dx = (i % 2 - 0.5) * kal * 0.5
        dy = (i // 2 - 0.5) * kal * 0.5
        boy = rnd.uniform(0.01, 0.035)
        K("Kiymik", (kal * 0.45, kal * 0.45, boy), (x + dx, y + dy, z + yon * boy / 2), mat,
          rot=(math.radians(rnd.uniform(-8, 8)), math.radians(rnd.uniform(-8, 8)), 0))


acik = False            # açılır eşya mı (Govde + ayrı parçalar)
parcalar = []           # açılır eşyada Govde dışındaki nesneler
mod = "yer"             # pivot kuralı (yerlestir)
kaldir = 0.0            # önizlemede eşyayı kaldırma (duvar/tavan eşyaları)

# ================================================================== ANTRE
if hangi == "gazete":
    # Sararmış gazete kâğıdı (iki ton), baskı mürekkebi, zarflar.
    sari = ortak.malzeme("Gazete4_Sari", (0.46, 0.38, 0.21), puruz=0.95)
    sari2 = ortak.malzeme("Gazete4_SariKoyu", (0.36, 0.28, 0.14), puruz=0.95)
    baski = ortak.malzeme("Gazete4_Baski", (0.06, 0.06, 0.06), puruz=0.9)
    gri = ortak.malzeme("Gazete4_Resim", (0.14, 0.13, 0.11), puruz=0.9)
    zarf = ortak.malzeme("Gazete4_Zarf", (0.60, 0.56, 0.45), puruz=0.9)
    pul = ortak.malzeme("Gazete4_Pul", (0.35, 0.05, 0.04), puruz=0.8)

    def baski_yap(z):
        """Katlanmış gazetenin üst yüzüne manşet, sütunlar ve bir fotoğraf lekesi (ortada, z yüksekliğinde)."""
        K("Manset", (0.30, 0.032, 0.0006), (0, 0.105, z), baski)
        K("Manset2", (0.20, 0.014, 0.0006), (-0.04, 0.07, z), baski)
        K("Resim", (0.13, 0.10, 0.0006), (0.10, -0.01, z), gri)
        for i in range(3):
            K("Sutun", (0.06, 0.17, 0.0005), (-0.14 + i * 0.075, -0.035, z), sari2)
        for i in range(5):
            K("Satir", (0.12, 0.004, 0.0006), (0.10, -0.075 - i * 0.012, z), baski)

    # Yığın: 13 kat katlanmış gazete, her biri biraz kaymış ve dönmüş.
    z = 0.0
    for i in range(13):
        k = 0.0045 + rnd.uniform(0, 0.0015)
        bas = len(p)
        K("Gazete", (0.40, 0.29, k), (0, 0, k / 2), sari if i % 3 else sari2, 0.001)
        if i == 12:
            baski_yap(k + 0.0003)
        grup(bas, T(rnd.uniform(-0.02, 0.02), rnd.uniform(-0.02, 0.02), z) @ Rz(rnd.uniform(-7, 7)))
        z += k
    # Kapıdan içeri itilmiş, yığından kaymış iki gazete: yerde, bir kenarı hafifçe kalkık.
    for (x, y, a, kalk) in ((-0.34, -0.10, 24, 0.035), (0.30, -0.20, -15, 0.02)):
        bas = len(p)
        yuzey("GazeteYer", 8, 4, lambda u, v, kalk=kalk: (-0.20 + 0.40 * u, -0.145 + 0.29 * v, 0.002 + kalk * max(0.0, u - 0.6) ** 2 / 0.16), sari, 0.002)
        baski_yap(0.0035) if kalk > 0.03 else None
        grup(bas, T(x, y, 0) @ Rz(a))
    # Zarflar: ikisi yığının üstünde, biri yerde; her birinde adres satırları ve pul.
    for (x, y, zz, a) in ((0.03, 0.02, z, 18), (-0.06, -0.04, z + 0.003, -9), (0.02, -0.33, 0.0, 33)):
        bas = len(p)
        K("Zarf", (0.22, 0.11, 0.003), (0, 0, 0.0015), zarf, 0.0008)
        K("Pul", (0.022, 0.026, 0.0006), (0.085, 0.03, 0.0033), pul)
        for i in range(3):
            K("Adres", (0.08 - i * 0.01, 0.0035, 0.0005), (-0.01, -0.005 - i * 0.013, 0.0033), baski)
        grup(bas, T(x, y, zz) @ Rz(a))
    ad, h, uzak = "Gazete_Yigini_K4", 0.1, 1.1

# ================================================================== HOL
elif hangi == "kablo":
    mod, kaldir = "tavan", 0.62
    alci = ortak.malzeme("Kablo4_Alci", (0.50, 0.47, 0.40), puruz=0.95)
    kordon = ortak.malzeme("Kablo4_Kordon", (0.42, 0.38, 0.30), puruz=0.8)
    bant = ortak.malzeme("Kablo4_Bant", (0.02, 0.02, 0.02), puruz=0.4)
    bakalit = ortak.malzeme("Kablo4_Bakalit", (0.035, 0.022, 0.015), puruz=0.35)
    pirinc = ortak.malzeme("Kablo4_Pirinc", (0.50, 0.36, 0.12), puruz=0.35, metal=0.9)
    # Tavan göbeği: kirli alçı kapak (pivot tepede, z = 0 tavan).
    torna("Gobek", [(0, 0), (0.045, 0), (0.045, -0.006), (0.036, -0.013), (0.012, -0.015), (0, -0.015)], (0, 0, 0), alci, 28)
    # Bükümlü iki damarlı kumaş kordon: iki tel birbirine sarılarak iner, hafif kavis yapar.
    N, L = 36, 0.44
    for faz in (0.0, math.pi):
        pts = []
        for i in range(N + 1):
            t = i / N
            zz = -0.014 - L * t
            kx = 0.012 * math.sin(t * math.pi) + 0.004 * math.sin(t * 7)   # kablonun hafif kavsi
            a = t * 14 * math.pi + faz
            pts.append((kx + 0.0028 * math.cos(a), 0.0028 * math.sin(a), zz))
        yol(pts, 0.0024, kordon, 6)
    # Tepeye yakın eski izole bant sargısı.
    S("Bant", 0.0068, 0.03, (0.003, 0, -0.05), bant, 12)
    # Ampulsüz duy: bakalit gövde, altı açık (içi oyuk), dipte pirinç kontak.
    zd = -0.014 - L
    torna("Duy", [(0, zd + 0.004), (0.010, zd + 0.004), (0.016, zd - 0.004), (0.021, zd - 0.014),
                  (0.021, zd - 0.062), (0.0145, zd - 0.062), (0.0145, zd - 0.022), (0, zd - 0.022)], (0.0, 0, 0), bakalit, 24)
    S("Kontak", 0.006, 0.003, (0.0, 0, zd - 0.0235), pirinc, 12)
    S("DuyHalka", 0.0215, 0.006, (0.0, 0, zd - 0.050), bakalit, 24)
    ad, h, uzak = "Sarkan_Kablo_K4", 0.5, 0.9

elif hangi == "koli":
    mod = "dayali"
    karton = ortak.malzeme("Koli4_Karton", (0.30, 0.18, 0.08), puruz=0.9)
    karton2 = ortak.malzeme("Koli4_KartonKoyu", (0.22, 0.13, 0.055), puruz=0.9)
    bant_m = ortak.malzeme("Koli4_Bant", (0.42, 0.33, 0.16), puruz=0.35)
    murek = ortak.malzeme("Koli4_Baski", (0.04, 0.04, 0.04), puruz=0.9)

    def yassi_koli(en, boy, kal=0.008):
        """Yassılaştırılmış koli: iki yüzün katlama çizgisi, kapakların kesik çizgileri, bant ve baskı (ön yüz -y)."""
        K("Levha", (en, kal, boy), (0, 0, boy / 2), karton, 0.002)
        on = -kal / 2 - 0.0004
        K("Kat", (0.006, 0.001, boy), (en * 0.08, on, boy / 2), karton2)                 # katlama çizgisi
        for zz in (0.13, boy - 0.13):
            K("Kesik", (en, 0.001, 0.004), (0, on, zz), karton2)                             # kapak çizgisi
            for xx in (-en * 0.25, en * 0.33):
                K("Yarik", (0.004, 0.001, 0.13), (xx, on, zz + (0.065 if zz > boy / 2 else -0.065)), karton2)
        K("Bant", (0.05, 0.0012, boy * 0.9), (-en * 0.05, on - 0.0003, boy / 2), bant_m)
        # Baskı: "bu taraf yukarı" okları ve bir kutu çerçevesi.
        for xx in (en * 0.22, en * 0.30):
            K("Ok", (0.006, 0.001, 0.07), (xx, on - 0.0004, boy * 0.62), murek)
            for s in (-1, 1):
                K("OkUc", (0.004, 0.001, 0.03), (xx + s * 0.009, on - 0.0004, boy * 0.62 + 0.03), murek, rot=(0, math.radians(s * 40), 0))
        K("Cerceve", (en * 0.32, 0.001, 0.005), (-en * 0.25, on - 0.0004, boy * 0.35), murek)
        K("Cerceve", (en * 0.32, 0.001, 0.005), (-en * 0.25, on - 0.0004, boy * 0.35 + 0.1), murek)

    # Arkadaki büyük yassı koli: 12° eğimle duvara dayalı (üst kenarı duvarda, y = 0).
    a1, L1, W1 = 12, 0.64, 0.95
    bas = len(p); yassi_koli(W1, L1)
    grup(bas, T(-0.05, -L1 * math.sin(math.radians(a1)) - 0.005, 0) @ Rx(-a1))
    # Öndeki küçük yassı koli: 25° eğimle büyük kolinin önüne yaslanmış.
    a2, L2, W2 = 25, 0.50, 0.70
    zt = L2 * math.cos(math.radians(a2))
    on1 = -0.009 - (L1 * math.cos(math.radians(a1)) - zt) * math.tan(math.radians(a1))
    bas = len(p); yassi_koli(W2, L2)
    grup(bas, T(0.22, on1 - L2 * math.sin(math.radians(a2)) - 0.006, 0) @ Rx(-a2))

    # Yerde buruşuk bir koli: alt bölünmüş küp, köşeleri ezilmiş, bir üst köşesi içe çökmüş.
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 0))
    bk = bpy.context.active_object
    bk.name = "Burusuk"
    bm = bmesh.new(); bm.from_mesh(bk.data)
    bmesh.ops.subdivide_edges(bm, edges=bm.edges[:], cuts=4, use_grid_fill=True)
    for v in bm.verts:
        x, y, zz = v.co.x * 0.40, v.co.y * 0.30, (v.co.z + 0.5) * 0.26
        ez = max(0.0, (v.co.x + v.co.y + v.co.z) - 0.6)              # +x +y üst köşe çöker
        zz -= ez * 0.09
        x -= ez * 0.03
        x += rnd.uniform(-0.006, 0.006); y += rnd.uniform(-0.006, 0.006); zz += rnd.uniform(-0.004, 0.004) * (zz > 0.01)
        v.co = Vector((x, y, max(0.0, zz)))
    bm.to_mesh(bk.data); bm.free()
    ortak.ata(bk, karton)
    p.append(bk)
    bas = len(p)
    K("BantUst", (0.05, 0.31, 0.002), (0, 0, 0.262), bant_m)
    K("KapakCizgi", (0.40, 0.004, 0.001), (0, 0, 0.2625), karton2)
    grup(bas)
    bas = len(p) - 2
    grup(bas, T(-0.58, -0.30, 0) @ Rz(-17))
    ad, h, uzak = "Bos_Koliler_K4", 0.5, 1.9

# ================================================================== SALON
elif hangi == "koltuk":
    carsaf = ortak.malzeme("Koltuk4_Carsaf", (0.60, 0.58, 0.53), puruz=0.95)
    # Çarşafın altındaki berjerin kaba biçimi: oturak, yükselen arkalık, yuvarlak kollar.
    K("Oturak", (0.66, 0.70, 0.44), (0, -0.04, 0.22), carsaf)
    K("Minder", (0.56, 0.62, 0.08), (0, -0.07, 0.46), carsaf, 0.03)
    K("Arkalik", (0.80, 0.20, 0.88), (0, 0.31, 0.44), carsaf, rot=(math.radians(-8), 0, 0))
    S("ArkaTepe", 0.10, 0.80, (0, 0.36, 0.86), carsaf, 20, rot=(0, math.pi / 2, 0))
    for s in (-1, 1):
        K("Kol", (0.13, 0.78, 0.56), (s * 0.36, -0.01, 0.28), carsaf)
        S("KolTepe", 0.075, 0.80, (s * 0.37, -0.02, 0.57), carsaf, 20, rot=(math.pi / 2, 0, 0))
        S("KolOn", 0.085, 0.14, (s * 0.37, -0.41, 0.52), carsaf, 20, rot=(0, math.pi / 2, 0))
    n = birles(p[:], "Carsaf")
    p.clear()
    dunyaya_isle(n)
    # VOXEL REMESH: parçaları tek, kesintisiz bir kabuğa çevirir (çarşaf hepsini tek parça örter).
    m = n.modifiers.new("Kabuk", "REMESH")
    m.mode = "VOXEL"
    m.voxel_size = 0.022
    sec(n); bpy.ops.object.modifier_apply(modifier=m.name)
    # Çarşafın yere sarkması: alt kısım dışa açılır ve dalgalı kıvrımlar yapar.
    for v in n.data.vertices:
        x, y, zz = v.co
        if zz < 0.40:
            k = (1 - zz / 0.40)
            a = math.atan2(y + 0.02, x)
            f = 1 + 0.10 * k * k + 0.035 * k * math.sin(9 * a + 1.3) + 0.015 * k * math.sin(23 * a)
            v.co.x = x * f
            v.co.y = (y + 0.02) * f - 0.02
    for tur, ayar in (("SMOOTH", None), ("DISPLACE", None)):
        m = n.modifiers.new(tur, tur)
        if tur == "SMOOTH":
            m.factor, m.iterations = 0.9, 12
        else:
            doku = bpy.data.textures.new("Carsaf_Kirisik", "CLOUDS")
            doku.noise_scale = 0.09
            m.texture, m.strength, m.mid_level = doku, 0.018, 0.5
        sec(n); bpy.ops.object.modifier_apply(modifier=m.name)
    for v in n.data.vertices:
        if v.co.z < 0.0:
            v.co.z = 0.0
    # Etek açılınca taban 0,85 m'yi aşıyordu: yatayda biraz daralt.
    n.data.transform(Matrix.Diagonal((0.9, 0.87, 1.0, 1.0)))
    # Remesh 22 mm'lik sık bir ağ verir; oyun için seyrelt (biçim korunur).
    m = n.modifiers.new("Seyrelt", "DECIMATE")
    m.ratio = 0.35
    sec(n); bpy.ops.object.modifier_apply(modifier=m.name)
    p.append(n)
    ad, h, uzak = "Ortulu_Koltuk_K4", 0.9, 1.9

elif hangi == "mumlar":
    emaye = ortak.malzeme("Mum4_Emaye", (0.68, 0.68, 0.64), puruz=0.3)
    mavi = ortak.malzeme("Mum4_EmayeKenar", (0.03, 0.06, 0.20), puruz=0.3)
    kirik = ortak.malzeme("Mum4_EmayeKirik", (0.04, 0.035, 0.03), puruz=0.7)
    mum = ortak.malzeme("Mum4_Mum", (0.72, 0.68, 0.56), puruz=0.55)
    fitil = ortak.malzeme("Mum4_Fitil", (0.02, 0.018, 0.016), puruz=0.9)
    kibrit_k = ortak.malzeme("Mum4_KibritKutu", (0.50, 0.40, 0.22), puruz=0.8)
    etiket = ortak.malzeme("Mum4_KibritEtiket", (0.32, 0.04, 0.03), puruz=0.7)
    sari = ortak.malzeme("Mum4_KibritSari", (0.60, 0.42, 0.05), puruz=0.7)
    surt = ortak.malzeme("Mum4_Surtunme", (0.10, 0.05, 0.03), puruz=0.95)
    cop = ortak.malzeme("Mum4_Cop", (0.45, 0.33, 0.16), puruz=0.8)
    bas_m = ortak.malzeme("Mum4_KibritBas", (0.30, 0.04, 0.02), puruz=0.7)
    # Emaye tabak (mavi kenarlı, kenarında çatlamış emaye lekeleri).
    torna("Tabak", [(0, 0), (0.068, 0), (0.098, 0.014), (0.102, 0.018), (0.097, 0.019), (0.066, 0.004), (0, 0.004)], (0, 0, 0), emaye, 32)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.1, minor_radius=0.0025, location=(0, 0, 0.0175), major_segments=32, minor_segments=6)
    ortak.ata(bpy.context.active_object, mavi); p.append(bpy.context.active_object)
    for a in (40, 170, 250):
        K("Catlak", (0.012, 0.008, 0.002), (0.099 * math.cos(math.radians(a)), 0.099 * math.sin(math.radians(a)), 0.0185), kirik, rot=(0, 0, math.radians(a)))
    # Tabağa yayılmış erimiş mum gölü.
    torna("MumGolu", [(0, 0.004), (0.055, 0.004), (0.052, 0.0065), (0.02, 0.0075), (0, 0.0075)], (0.0, 0.0, 0), mum, 20)

    def eriyik_mum(x, y, boy, r=0.011):
        """Yanmış mum: gövde, çukurlaşmış tepe, yanlardan süzülen damlalar, kararmış kıvrık fitil."""
        z0 = 0.007
        torna("Mum", [(0, z0), (r + 0.002, z0), (r, z0 + 0.006), (r, z0 + boy), (r * 0.8, z0 + boy - 0.002), (r * 0.45, z0 + boy - 0.004), (0, z0 + boy - 0.004)], (x, y, 0), mum, 16)
        for i in range(4):
            a = rnd.uniform(0, 2 * math.pi)
            dz = rnd.uniform(0.3, 0.9) * boy
            kure(0.0035, (x + r * math.cos(a), y + r * math.sin(a), z0 + dz), mum, olcek=(1, 1, 3.2), seg=8)
        yol([(x, y, z0 + boy - 0.005), (x + 0.001, y, z0 + boy + 0.004), (x + 0.004, y + 0.001, z0 + boy + 0.008)], 0.0009, fitil, 5)

    eriyik_mum(0.020, 0.028, 0.075)
    eriyik_mum(-0.038, -0.006, 0.042, 0.012)
    kure(0.024, (0.030, -0.045, 0.007), mum, olcek=(1.2, 1, 0.45), seg=14)       # tamamen erimiş mum topağı
    yol([(0.030, -0.045, 0.014), (0.032, -0.044, 0.021)], 0.0009, fitil, 5)

    # Kibrit kutusu: kılıf (etiketli üst, yanlarda sürtünme şeridi), çekmecesi yarı çekilmiş.
    bas = len(p)
    K("Kilif", (0.052, 0.036, 0.015), (0, 0, 0.0075), kibrit_k, 0.001)
    K("Etiket", (0.046, 0.030, 0.0006), (0, 0, 0.0152), etiket)
    K("EtiketSerit", (0.046, 0.007, 0.0007), (0, 0.004, 0.0153), sari)
    for s in (-1, 1):
        K("Surtunme", (0.046, 0.0006, 0.011), (0, s * 0.0181, 0.0075), surt)
    K("Cekmece", (0.050, 0.033, 0.012), (0.020, 0, 0.0068), cop, 0.0005)
    for i in range(5):
        K("KibritBas", (0.004, 0.0035, 0.004), (0.043, -0.012 + i * 0.006, 0.0125), bas_m)
    grup(bas, T(0.17, 0.035, 0) @ Rz(28))
    # Yanık kibritler (kararmış uçlu) ve yere düşmüş iki mum parçası, yerde mum damlaları.
    for (x, y, a) in ((0.12, -0.08, 60), (0.15, -0.10, 20), (-0.11, 0.09, -35)):
        bas = len(p)
        K("Cop", (0.045, 0.0022, 0.0022), (0, 0, 0.0011), cop)
        K("Yanik", (0.012, 0.0028, 0.0028), (0.026, 0, 0.0014), fitil)
        grup(bas, T(x, y, 0) @ Rz(a))
    S("Parca", 0.011, 0.035, (-0.10, -0.10, 0.011), mum, 16, rot=(0, math.pi / 2, math.radians(25)))
    S("Parca", 0.009, 0.022, (-0.14, -0.05, 0.009), mum, 16, rot=(0, math.pi / 2, math.radians(-50)))
    for (x, y, r) in ((-0.06, -0.13, 0.008), (0.09, 0.12, 0.006), (-0.15, 0.03, 0.005)):
        kure(r, (x, y, 0.001), mum, olcek=(1.3, 1, 0.25), seg=10)
    ad, h, uzak = "Mumlar_K4", 0.1, 0.65

elif hangi == "sehpa":
    ceviz = ortak.malzeme("Sehpa4_Ceviz", (0.06, 0.03, 0.013), puruz=0.5)
    ceviz_ac = ortak.malzeme("Sehpa4_Cizik", (0.16, 0.09, 0.04), puruz=0.7)
    EN, DER, H = 0.60, 0.40, 0.455
    # Önce dik sehpa: tabla, etek, dört ayak, alt raf. Arka sağ ayak (x+, y+) kırık: kök kısmı kalmış.
    K("Tabla", (EN, DER, 0.025), (0, 0, H - 0.0125), ceviz, 0.004)
    for (a, b) in ((0.06, 0.20), (-0.10, -0.12), (0.18, -0.05)):
        K("Cizik", (0.10, 0.002, 0.0006), (a, b * 0.8, H + 0.0003), ceviz_ac, rot=(0, 0, math.radians(20 + a * 100)))
    for s in (-1, 1):
        K("Etek", (EN - 0.10, 0.018, 0.06), (0, s * (DER / 2 - 0.045), H - 0.055), ceviz, 0.003)
        K("Etek", (0.018, DER - 0.10, 0.06), (s * (EN / 2 - 0.045), 0, H - 0.055), ceviz, 0.003)
    for x in (-1, 1):
        for y in (-1, 1):
            px, py = x * (EN / 2 - 0.045), y * (DER / 2 - 0.045)
            if x == 1 and y == 1:
                K("AyakKok", (0.034, 0.034, 0.19), (px, py, H - 0.025 - 0.095), ceviz, 0.003)
                kirik_uc(px, py, H - 0.025 - 0.19, -1, ceviz_ac, 0.034)
            else:
                K("Ayak", (0.034, 0.034, H - 0.025), (px, py, (H - 0.025) / 2), ceviz, 0.003)
    K("AltRaf", (EN - 0.10, DER - 0.10, 0.015), (0, 0, 0.12), ceviz, 0.003)
    # Devir: uzun kenarı üstüne yatır (x ekseni etrafında 90°), ayak uçları yere değsin diye 3° daha.
    grup(0, Rx(-3) @ Rx(90), yere=True)
    # Kopan ayak parçası: yerde, sehpanın önünde, kırık ucu kıymıklı.
    bas = len(p)
    K("KopukAyak", (0.034, 0.034, 0.24), (0, 0, 0.12), ceviz, 0.003)
    kirik_uc(0, 0, 0.24, 1, ceviz_ac, 0.034)
    grup(bas, T(0.10, -0.62, 0.017) @ Rz(38) @ Ry(90))
    ad, h, uzak = "Yikik_Sehpa_K4", 0.4, 1.4

# ================================================================== MUTFAK
elif hangi == "buzdolabi":
    acik = True
    emaye = ortak.malzeme("Buz4_Krem", (0.64, 0.54, 0.32), puruz=0.35)
    ic = ortak.malzeme("Buz4_Ic", (0.58, 0.58, 0.54), puruz=0.45)
    krom = ortak.malzeme("Buz4_Krom", (0.62, 0.62, 0.60), puruz=0.2, metal=1.0)
    conta = ortak.malzeme("Buz4_Conta", (0.07, 0.07, 0.065), puruz=0.7)
    izgara_m = ortak.malzeme("Buz4_Izgara", (0.04, 0.04, 0.04), puruz=0.6, metal=0.4)
    tel = ortak.malzeme("Buz4_Tel", (0.55, 0.55, 0.53), puruz=0.3, metal=0.9)
    buzluk = ortak.malzeme("Buz4_Buzluk", (0.45, 0.46, 0.47), puruz=0.35, metal=0.8)
    pas = ortak.malzeme("Buz4_Pas", (0.16, 0.06, 0.02), puruz=0.9)
    lamba_m = ortak.malzeme("Buz4_LambaKapak", (0.70, 0.68, 0.60), puruz=0.2)
    W, H = 0.60, 1.45
    YON, YARKA = -0.255, 0.31          # gövdenin ön ve arka yüzü (kapı önünde, y = -0.31 .. -0.255)
    R = 0.085                          # üstün ön ve arka kenarındaki yuvarlaklık yarıçapı
    # Dış gövde: yandan bakınca üstü yuvarlatılmış profil, x boyunca uzatılır (prizma).
    prof = [(YON, 0.0), (YARKA, 0.0)]
    for k in range(10):
        a = k / 9 * math.pi / 2
        prof.append((YARKA - R + R * math.cos(a), H - R + R * math.sin(a)))
    for k in range(10):
        a = math.pi / 2 + k / 9 * math.pi / 2
        prof.append((YON + R + R * math.cos(a), H - R + R * math.sin(a)))
    n = len(prof)
    verts = [(-W / 2, y, z) for (y, z) in prof] + [(W / 2, y, z) for (y, z) in prof]
    faces = [tuple(range(n)), tuple(range(n, 2 * n))] + [(i, (i + 1) % n, n + (i + 1) % n, n + i) for i in range(n)]
    dis = mesh_nesnesi("Dis", verts, faces, emaye)
    p.remove(dis)
    # İç boşluk: önü açık oyuk (duvarlar 4,5 cm yalıtımlı, taban 0,14, tavan 1,34).
    IX, IY1, IZ0, IZ1 = 0.255, 0.26, 0.14, 1.34
    kes = ortak.kutu("Kesici", (2 * IX, IY1 - YON + 0.05, IZ1 - IZ0), (0, (IY1 + YON - 0.05) / 2, (IZ0 + IZ1) / 2), emaye)
    ortak.oy(dis, kes)
    # İç yüzeyler açık gri astar: boşluğu 3 mm'lik levhalarla kaplıyoruz (gövde dışı krem kalsın).
    p.append(dis)
    t = 0.003
    K("AstarTaban", (2 * IX, IY1 - YON, t), (0, (IY1 + YON) / 2, IZ0 + t / 2), ic)
    K("AstarTavan", (2 * IX, IY1 - YON, t), (0, (IY1 + YON) / 2, IZ1 - t / 2), ic)
    K("AstarArka", (2 * IX, t, IZ1 - IZ0), (0, IY1 - t / 2, (IZ0 + IZ1) / 2), ic)
    for s in (-1, 1):
        K("AstarYan", (t, IY1 - YON, IZ1 - IZ0), (s * (IX - t / 2), (IY1 + YON) / 2, (IZ0 + IZ1) / 2), ic)
    IX -= t
    # Tel raflar: yan duvarlardaki çıkıntılara oturur; üst yüzleri RAFLAR z'lerinde.
    RAFLAR = (0.46, 0.76, 1.03)
    RY0, RY1 = -0.185, IY1 - 0.01
    for zr in RAFLAR:
        zc = zr - 0.004
        for s in (-1, 1):
            K("RafTutucu", (0.010, RY1 - RY0, 0.008), (s * (IX - 0.005), (RY0 + RY1) / 2, zr - 0.012), ic, 0.002)
        for (a, b) in (((-IX + 0.01, RY0, zc), (IX - 0.01, RY0, zc)), ((-IX + 0.01, RY1, zc), (IX - 0.01, RY1, zc)),
                       ((-IX + 0.01, RY0, zc), (-IX + 0.01, RY1, zc)), ((IX - 0.01, RY0, zc), (IX - 0.01, RY1, zc))):
            cubuk(a, b, 0.0035, tel, 6)
        x = -IX + 0.035
        while x < IX - 0.02:
            cubuk((x, RY0, zc), (x, RY1, zc), 0.0018, tel, 5)
            x += 0.026
    # Üstte alüminyum buzluk bölmesi (sol üst, kapağı kapalı) ve sağ üstte iç lamba yuvası.
    K("Buzluk", (0.36, IY1 - (-0.20) - 0.004, 0.16), (-IX + 0.18, (IY1 - 0.20) / 2, IZ1 - t - 0.08), buzluk, 0.004)
    K("BuzlukKapak", (0.36, 0.008, 0.155), (-IX + 0.18, -0.204, IZ1 - t - 0.08), buzluk, 0.003)
    K("BuzlukDil", (0.06, 0.012, 0.012), (-IX + 0.18, -0.212, IZ1 - t - 0.145), krom, 0.003)
    K("LambaYuva", (0.05, 0.10, 0.08), (IX - 0.025, 0.05, IZ1 - t - 0.05), ic, 0.006)
    K("LambaKapak", (0.012, 0.07, 0.05), (IX - 0.056, 0.05, IZ1 - t - 0.055), lamba_m, 0.004)
    # Dış ayrıntılar: alt süpürgelikte havalandırma ızgarası, menteşe topuzları, pas lekeleri.
    K("Izgara", (0.50, 0.004, 0.07), (0, YON - 0.002, 0.055), izgara_m, 0.001)
    for i in range(5):
        K("IzgaraCizgi", (0.48, 0.002, 0.004), (0, YON - 0.0045, 0.030 + i * 0.012), krom)
    for zz in (0.093, 1.382):
        S("Mentese", 0.009, 0.022, (-W / 2 + 0.012, -0.30, zz), krom, 12)
    K("Pas", (0.05, 0.003, 0.03), (0.21, YON - 0.003, 0.08), pas, 0.001)
    K("Pas", (0.002, 0.08, 0.05), (W / 2 + 0.001, -0.05, 0.05), pas)
    govde = birles(p[:], "Govde")
    p.clear()
    # KAPI: boydan, sol kenarından (-x) menteşeli. Ön yüz y = -0.31, kalınlık 5,5 cm.
    DY0, DY1, DZ0, DZ1, DX = -0.31, YON, 0.105, 1.37, W / 2 - 0.005
    kp = ortak.kutu("KapiPano", (2 * DX, DY1 - DY0, DZ1 - DZ0), (0, (DY0 + DY1) / 2, (DZ0 + DZ1) / 2), emaye)
    m = kp.modifiers.new("Yuvarla", "BEVEL"); m.width, m.segments, m.limit_method = 0.016, 4, "ANGLE"
    p.append(kp)
    # İç yüz: boşluğa oturan astar tampon, lastik conta, iki kapı rafı (şişe/yumurta).
    K("KapiAstar", (0.46, 0.018, 1.15), (0, DY1 + 0.009, 0.74), ic, 0.006)
    for (xa, ya, za, xb, yb, zb) in ((-0.25, DY1, 0.15, 0.25, DY1 + 0.012, 0.162), (-0.25, DY1, 1.318, 0.25, DY1 + 0.012, 1.33),
                                     (-0.25, DY1, 0.15, -0.238, DY1 + 0.012, 1.33), (0.238, DY1, 0.15, 0.25, DY1 + 0.012, 1.33)):
        p.append(ac.levha("Conta", xa, xb, ya, yb, za, zb, conta, 0.002))
    for zr in (0.30, 0.66):
        p.append(ac.levha("KapiRaf", -0.20, 0.20, DY1 + 0.018, DY1 + 0.058, zr, zr + 0.008, ic, 0.002))
        p.append(ac.levha("KapiRafDil", -0.20, 0.20, DY1 + 0.054, DY1 + 0.060, zr, zr + 0.055, ic, 0.002))
        for s in (-1, 1):
            p.append(ac.levha("KapiRafYan", s * 0.20 - 0.004, s * 0.20 + 0.004, DY1 + 0.018, DY1 + 0.06, zr, zr + 0.055, ic, 0.001))
    # Krom kol: serbest kenarda (+x) 70'lerin çekmeli kolu; tabanı, çekme dili ve amblem.
    K("KolTaban", (0.05, 0.010, 0.24), (DX - 0.05, DY0 - 0.004, 1.02), krom, 0.004)
    cubuk((DX - 0.05, DY0 - 0.009, 1.12), (DX - 0.05, DY0 - 0.040, 1.10), 0.009, krom, 12)
    cubuk((DX - 0.05, DY0 - 0.040, 1.10), (DX - 0.05, DY0 - 0.040, 0.93), 0.011, krom, 12)
    cubuk((DX - 0.05, DY0 - 0.040, 0.93), (DX - 0.05, DY0 - 0.009, 0.91), 0.009, krom, 12)
    K("Amblem", (0.16, 0.004, 0.026), (0, DY0 - 0.002, 1.29), krom, 0.002)
    K("AmblemCizgi", (0.12, 0.002, 0.006), (0, DY0 - 0.0045, 1.29), izgara_m)
    # Terk edilmişlik: kapının altında pas, kolun yanında kir izi.
    K("KapiPas", (0.08, 0.002, 0.012), (-0.12, DY0 - 0.001, 0.13), pas)
    K("KapiPas", (0.02, 0.002, 0.05), (0.17, DY0 - 0.001, 0.16), pas)
    K("Kir", (0.05, 0.001, 0.14), (DX - 0.12, DY0 - 0.0006, 1.02), ortak.malzeme("Buz4_Kir", (0.35, 0.29, 0.17), puruz=0.9))
    kapi = birles(p[:], "Kapi_1")
    p.clear()
    ortak.pivotu_tasi(kapi, (-DX, (DY0 + DY1) / 2, DZ0))
    parcalar.append(kapi)
    ad, h, uzak = "Buzdolabi_K4", 1.4, 2.4

elif hangi == "kova":
    galv = ortak.malzeme("Kova4_Galvaniz", (0.34, 0.35, 0.34), puruz=0.45, metal=0.85)
    su = ortak.malzeme("Kova4_KirliSu", (0.045, 0.040, 0.028), puruz=0.05)
    tel = ortak.malzeme("Kova4_Tel", (0.30, 0.30, 0.29), puruz=0.4, metal=0.9)
    sap = ortak.malzeme("Kova4_Sap", (0.20, 0.12, 0.05), puruz=0.7)
    bez = ortak.malzeme("Kova4_Bez", (0.16, 0.15, 0.13), puruz=0.95)
    # Konik galvaniz kova: dış yüz yukarı, iç yüz aşağı (et kalınlığı 3 mm), ağız kenarında kıvrık halka.
    torna("Kova", [(0, 0), (0.118, 0), (0.121, 0.008), (0.149, 0.282), (0.146, 0.284), (0.1425, 0.282), (0.115, 0.011), (0, 0.011)], (0, 0, 0), galv, 32)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.149, minor_radius=0.0045, location=(0, 0, 0.283), major_segments=32, minor_segments=8)
    ortak.ata(bpy.context.active_object, galv); p.append(bpy.context.active_object)
    for zz in (0.08, 0.20):
        r = 0.121 + (0.149 - 0.121) * (zz - 0.008) / 0.274
        S("Bogum", r + 0.002, 0.008, (0, 0, zz), galv, 32)
    # Kirli su: iç duvara oturan bulanık yüzey.
    torna("Su", [(0, 0.012), (0.117, 0.012), (0.1305, 0.176), (0, 0.176)], (0, 0, 0), su, 32)
    # Kulaklar ve geriye yatık tel sap (menteşe ekseni x, 65° geriye düşmüş).
    fi = math.radians(65)
    for s in (-1, 1):
        K("Kulak", (0.012, 0.022, 0.03), (s * 0.150, 0, 0.255), galv, 0.003)
    pts = []
    for i in range(17):
        a = math.pi * i / 16
        yuk = 0.158 * math.sin(a)
        pts.append((-0.158 * math.cos(a), yuk * math.sin(fi), 0.255 + yuk * math.cos(fi)))
    yol(pts, 0.0025, tel, 6)
    # Paspas: tahta T başlık yerde (gri yer bezi sarılı), sapı kovanın ağzına dayalı.
    hx = 0.285
    ust = Vector((0.155, 0.0, 0.300))
    alt = Vector((hx, 0.0, 0.045))
    d = (ust - alt).normalized()
    cubuk(tuple(alt), tuple(alt + d * 1.20), 0.012, sap, 10)
    K("TBaslik", (0.035, 0.34, 0.03), (hx + 0.01, 0, 0.03), sap, 0.004)
    K("BezSarili", (0.085, 0.38, 0.055), (hx + 0.012, 0, 0.031), bez, 0.02)
    yuzey("BezYer", 6, 6, lambda u, v: (hx + 0.05 + 0.20 * u, -0.17 + 0.30 * v, 0.004 + 0.012 * math.sin(u * 6 + v * 4) ** 2 * (1 - u)), bez, 0.006)
    # Yere sızmış kirli su birikintisi.
    torna("Birikinti", [(0, 0), (0.10, 0), (0.095, 0.0015), (0, 0.0015)], (0.30, -0.16, 0), su, 20)
    ad, h, uzak = "Kova_Paspas_K4", 0.6, 1.6

elif hangi == "sandalye":
    kayin = ortak.malzeme("Sandalye4_Kayin", (0.24, 0.13, 0.055), puruz=0.6)
    oturak_m = ortak.malzeme("Sandalye4_Oturak", (0.16, 0.085, 0.035), puruz=0.55)
    kiymik = ortak.malzeme("Sandalye4_Kiymik", (0.42, 0.28, 0.13), puruz=0.8)
    S_, H = 0.42, 0.45
    # Dik sandalye: ön sol ayak (x-, y-) oturağın altından kopmuş, kökü kalmış; ön ve sol bağlantı çubukları yok.
    for x in (-1, 1):
        for y in (-1, 1):
            px, py = x * (S_ / 2 - 0.025), y * (S_ / 2 - 0.025)
            if y == 1:
                K("ArkaAyak", (0.036, 0.036, 0.90), (px, py, 0.45), kayin, 0.004)
            elif x == -1:
                K("AyakKok", (0.034, 0.034, 0.10), (px, py, H - 0.05 - 0.02), kayin, 0.004)
                kirik_uc(px, py, H - 0.12, -1, kiymik, 0.034)
            else:
                K("OnAyak", (0.034, 0.034, H - 0.02), (px, py, (H - 0.02) / 2), kayin, 0.004)
    K("Oturak", (S_, S_, 0.022), (0, 0, H), oturak_m, 0.006)
    for s in (-1, 1):
        K("Kusak", (S_ - 0.05, 0.018, 0.05), (0, s * (S_ / 2 - 0.025), H - 0.04), kayin, 0.003)
        K("Kusak", (0.018, S_ - 0.05, 0.05), (s * (S_ / 2 - 0.025), 0, H - 0.04), kayin, 0.003)
    K("Baglanti", (0.018, S_ - 0.05, 0.02), (S_ / 2 - 0.025, 0, 0.15), kayin, 0.003)       # sağ
    K("Baglanti", (S_ - 0.05, 0.018, 0.02), (0, S_ / 2 - 0.025, 0.15), kayin, 0.003)       # arka
    K("ArkaUst", (S_ - 0.02, 0.022, 0.08), (0, S_ / 2 - 0.025, 0.85), kayin, 0.006)
    for zz in (0.58, 0.70):
        K("ArkaCita", (S_ - 0.05, 0.016, 0.035), (0, S_ / 2 - 0.025, zz), kayin, 0.003)
    # Devir: sağ yanının üstüne yatar (y ekseni etrafında 90°, kırık ayak yukarıda kalır), biraz döner.
    grup(0, Rz(18) @ Ry(90), yere=True)
    # Kopan ayak: sandalyenin önünde yerde, kırık ucu kıymıklı.
    bas = len(p)
    K("KopukAyak", (0.034, 0.034, 0.31), (0, 0, 0.155), kayin, 0.004)
    kirik_uc(0, 0, 0.31, 1, kiymik, 0.034)
    grup(bas, T(0.05, -0.40, 0.017) @ Rz(-62) @ Ry(90))
    ad, h, uzak = "Kirik_Sandalye_K4", 0.5, 1.6

# ================================================================== ODA B
elif hangi == "kamp":
    boru = ortak.malzeme("Kamp4_Aluminyum", (0.45, 0.46, 0.45), puruz=0.35, metal=0.9)
    branda = ortak.malzeme("Kamp4_Branda", (0.06, 0.09, 0.035), puruz=0.9)
    battaniye = ortak.malzeme("Kamp4_Battaniye", (0.14, 0.14, 0.135), puruz=0.98)
    serit = ortak.malzeme("Kamp4_BattaniyeSerit", (0.05, 0.05, 0.05), puruz=0.98)
    lastik = ortak.malzeme("Kamp4_Lastik", (0.02, 0.02, 0.02), puruz=0.8)
    mont = ortak.malzeme("Kamp4_Mont", (0.07, 0.05, 0.035), puruz=0.85)
    RX, RZ, YL = 0.34, 0.385, 0.95

    def branda_z(x, y):
        """Brandanın o noktadaki yüksekliği: ortası ağırlıktan hafifçe çökük."""
        u = min(1.0, max(0.0, (x + RX) / (2 * RX)))
        v = min(1.0, max(0.0, (y + YL) / (2 * YL)))
        return RZ - 0.035 * math.sin(math.pi * u) * (0.35 + 0.65 * math.sin(math.pi * v))

    # Çerçeve: iki uzun yan boru, iki uç boru, üç X ayak (kesişen iki boru), lastik ayak uçları.
    for s in (-1, 1):
        cubuk((s * RX, -YL, RZ), (s * RX, YL, RZ), 0.012, boru, 10)
        cubuk((-RX, s * YL, RZ), (RX, s * YL, RZ), 0.011, boru, 10)
    for yy in (-0.72, 0.0, 0.72):
        for s in (-1, 1):
            a = (-s * 0.30, yy + s * 0.012, 0.012)
            b = (s * RX, yy + s * 0.012, RZ - 0.012)
            cubuk(a, b, 0.010, boru, 8)
            S("LastikAyak", 0.014, 0.024, (a[0], a[1], 0.012), lastik, 10)
        S("Perno", 0.006, 0.05, (0, yy, (RZ) / 2), boru, 8, rot=(math.pi / 2, 0, 0))
    # Branda: çökük yüzey + yan borulara sarılan kılıflar.
    yuzey("Branda", 10, 20, lambda u, v: (-RX + 2 * RX * u, -YL + 0.03 + (2 * YL - 0.06) * v, branda_z(-RX + 2 * RX * u, -YL + 0.03 + (2 * YL - 0.06) * v) + 0.002), branda, 0.003)
    for s in (-1, 1):
        S("Kilif", 0.017, 2 * YL - 0.06, (s * RX, 0, RZ), branda, 12, rot=(math.pi / 2, 0, 0))
    # İnce gri battaniye: brandanın üstünde kırışık, sağ yandan yere doğru sarkar.
    GEN, SARK, r = 0.66, 0.30, 0.03
    yay = r * math.pi / 2

    def battaniye_f(u, v):
        s = -0.32 + u * (GEN + SARK)
        y = -0.82 + 1.40 * v
        kir = 0.010 * math.sin(v * 15 + u * 4) + 0.007 * math.sin(u * 19 + v * 6)
        kenar = RX - 0.005
        if s <= kenar:
            return (s, y, branda_z(s, y) + 0.012 + kir * (0.5 + 0.5 * u))
        zt = branda_z(kenar, y) + 0.012 + r
        d = s - kenar
        if d < yay:
            aci = d / r
            return (kenar + r * math.sin(aci) + 0.006, y, zt - r + r * math.cos(aci))
        return (kenar + r + 0.006 + 0.012 * math.sin(v * 11), y, zt - r - (d - yay))

    yuzey("Battaniye", 26, 24, battaniye_f, battaniye, 0.008)
    for yy in (-0.70, 0.46):
        yuzey("Serit", 26, 1, lambda u, v, yy=yy: (battaniye_f(u, (yy + 0.82) / 1.40)[0], yy - 0.02 + 0.04 * v, battaniye_f(u, (yy + 0.82) / 1.40)[2] + 0.0045), serit, 0.001)
    # Yastık yerine rulo yapılmış mont.
    S("Mont", 0.065, 0.46, (-0.02, 0.76, branda_z(0, 0.76) + 0.05), mont, 16, rot=(0, math.pi / 2, math.radians(4)))
    ad, h, uzak = "Kamp_Yatagi_K4", 0.45, 2.6

elif hangi == "alet":
    acik = True
    boya = ortak.malzeme("Alet4_Kirmizi", (0.33, 0.035, 0.025), puruz=0.35, metal=0.5)
    krom = ortak.malzeme("Alet4_Krom", (0.58, 0.58, 0.56), puruz=0.25, metal=1.0)
    siyah = ortak.malzeme("Alet4_Sap", (0.02, 0.02, 0.02), puruz=0.5)
    cizik = ortak.malzeme("Alet4_Cizik", (0.10, 0.10, 0.10), puruz=0.5, metal=0.8)
    W, D, HB, HT, t = 0.45, 0.22, 0.165, 0.22, 0.004
    # Gövde: 4 mm sac levhalar, içi boş; iç taban z = t.
    p += [ac.levha("Taban", -W / 2, W / 2, -D / 2, D / 2, 0, t, boya, 0.001),
          ac.levha("On", -W / 2, W / 2, -D / 2, -D / 2 + t, 0, HB, boya, 0.001),
          ac.levha("Arka", -W / 2, W / 2, D / 2 - t, D / 2, 0, HB, boya, 0.001),
          ac.levha("Yan", -W / 2, -W / 2 + t, -D / 2 + t, D / 2 - t, 0, HB, boya, 0.001),
          ac.levha("Yan", W / 2 - t, W / 2, -D / 2 + t, D / 2 - t, 0, HB, boya, 0.001)]
    # Presli kabartma çizgileri (ön ve yanlarda), iki krom kilit mandalı, isim plakası.
    for zz in (0.045, 0.095):
        p.append(ac.levha("Kabartma", -W / 2 + 0.03, W / 2 - 0.03, -D / 2 - 0.002, -D / 2, zz, zz + 0.01, boya, 0.001))
    for s in (-1, 1):
        p.append(ac.levha("Mandal", s * 0.14 - 0.018, s * 0.14 + 0.018, -D / 2 - 0.006, -D / 2, HB - 0.045, HB - 0.004, krom, 0.001))
        p.append(ac.levha("MandalDil", s * 0.14 - 0.008, s * 0.14 + 0.008, -D / 2 - 0.009, -D / 2 - 0.005, HB - 0.02, HB - 0.002, krom, 0.001))
        p.append(ac.levha("Menteşe", s * 0.13 - 0.03, s * 0.13 + 0.03, D / 2, D / 2 + 0.004, HB - 0.02, HB, krom, 0.001))
    p.append(ac.levha("Plaka", -0.04, 0.04, -D / 2 - 0.0015, -D / 2, 0.064, 0.080, krom, 0.0005))
    # Boya atıkları (koyu metal görünen çizikler).
    for (x, zz, w) in ((-0.17, 0.02, 0.03), (0.09, 0.13, 0.02), (0.20, 0.06, 0.015)):
        p.append(ac.levha("Cizik", x - w / 2, x + w / 2, -D / 2 - 0.0008, -D / 2, zz, zz + 0.004, cizik, 0.0))
    # Kapak: sığ tepsi (üst levha + dört etek), üstünde katlanır sap; pivot arka üst kenar.
    govde = birles(p[:], "Govde")
    p.clear()
    o = 0.003
    X0, X1, Y0, Y1, Z0 = -W / 2 - o, W / 2 + o, -D / 2 - o, D / 2 + o, HB + 0.002
    p += [ac.levha("KUst", X0, X1, Y0, Y1, HT - t, HT, boya, 0.0015),
          ac.levha("KEtek", X0, X1, Y0, Y0 + t, Z0, HT, boya, 0.001),
          ac.levha("KEtek", X0, X1, Y1 - t, Y1, Z0, HT, boya, 0.001),
          ac.levha("KEtek", X0, X0 + t, Y0 + t, Y1 - t, Z0, HT, boya, 0.001),
          ac.levha("KEtek", X1 - t, X1, Y0 + t, Y1 - t, Z0, HT, boya, 0.001)]
    for s in (-1, 1):
        p.append(ac.levha("Tirnak", s * 0.14 - 0.01, s * 0.14 + 0.01, Y0 - 0.003, Y0, Z0 + 0.004, Z0 + 0.02, krom, 0.001))
        p.append(ac.levha("SapKulak", s * 0.11 - 0.012, s * 0.11 + 0.012, -0.012, 0.012, HT, HT + 0.012, krom, 0.002))
    cubuk((-0.115, 0.0, HT + 0.012), (0.115, 0.0, HT + 0.012), 0.005, krom, 10)
    S("SapTutamak", 0.011, 0.13, (0, 0.0, HT + 0.012), siyah, 12, rot=(0, math.pi / 2, 0))
    p.append(ac.levha("KCizik", 0.05, 0.09, -0.03, -0.026, HT, HT + 0.0006, cizik, 0.0))
    kapak = birles(p[:], "Kapak")
    p.clear()
    ortak.pivotu_tasi(kapak, (0, Y1, HT))
    parcalar.append(kapak)
    ad, h, uzak = "Alet_Kutusu_K4", 0.25, 0.9

elif hangi == "termos":
    govde_m = ortak.malzeme("Termos4_Govde", (0.035, 0.11, 0.06), puruz=0.35)
    desen = ortak.malzeme("Termos4_Desen", (0.55, 0.45, 0.20), puruz=0.4)
    krom = ortak.malzeme("Termos4_Krom", (0.58, 0.58, 0.56), puruz=0.25, metal=1.0)
    kapak_m = ortak.malzeme("Termos4_Kapak", (0.30, 0.04, 0.03), puruz=0.4)
    cam = ortak.malzeme("Termos4_Cam", (0.62, 0.64, 0.60), puruz=0.05)
    cay = ortak.malzeme("Termos4_CayIzi", (0.14, 0.035, 0.01), puruz=0.2)
    # Termos gövdesi (yeşil, omuzlu), krom halkalar, kupa kapak, yandan tutma kulpu, iki şerit desen.
    torna("Termos", [(0, 0), (0.044, 0), (0.046, 0.004), (0.046, 0.215), (0.040, 0.235), (0.030, 0.245), (0, 0.245)], (0, 0, 0), govde_m, 28)
    for (zz, r) in ((0.006, 0.0475), (0.212, 0.0475)):
        S("Halka", r, 0.010, (0, 0, zz), krom, 28)
    for zz in (0.07, 0.15):
        S("Desen", 0.0465, 0.02, (0, 0, zz), desen, 28)
    torna("Kupa", [(0, 0.243), (0.034, 0.243), (0.040, 0.25), (0.041, 0.318), (0.030, 0.322), (0, 0.322)], (0, 0, 0), kapak_m, 28)
    yol([(0.046, 0, 0.18), (0.075, 0, 0.17), (0.078, 0, 0.09), (0.046, 0, 0.08)], 0.008, kapak_m, 8)
    # İnce belli çay bardağı: dış profil yukarı, iç profil aşağı; dibinde kurumuş çay izi.
    gx, gy = 0.11, -0.03
    torna("Bardak", [(0, 0), (0.020, 0), (0.021, 0.004), (0.019, 0.02), (0.0155, 0.046), (0.019, 0.072), (0.0245, 0.096), (0.025, 0.1),
                     (0.0235, 0.1), (0.023, 0.096), (0.0175, 0.072), (0.014, 0.046), (0.0175, 0.02), (0.017, 0.009), (0, 0.009)], (gx, gy, 0), cam, 24)
    torna("CayIzi", [(0, 0.009), (0.0172, 0.009), (0.016, 0.014), (0, 0.014)], (gx, gy, 0), cay, 20)
    ad, h, uzak = "Termos_K4", 0.3, 0.8

elif hangi == "lamba":
    pirinc = ortak.malzeme("Luks4_Pirinc", (0.45, 0.30, 0.09), puruz=0.35, metal=0.9)
    nikel = ortak.malzeme("Luks4_Nikel", (0.30, 0.30, 0.29), puruz=0.3, metal=0.9)
    cam = ortak.malzeme("Luks4_Cam", (0.30, 0.29, 0.26), puruz=0.1)
    is_m = ortak.malzeme("Luks4_Is", (0.03, 0.028, 0.025), puruz=0.8)
    gomlek = ortak.malzeme("Luks4_Gomlek", (0.62, 0.60, 0.55), puruz=0.95)
    kadran = ortak.malzeme("Luks4_Kadran", (0.70, 0.68, 0.60), puruz=0.4)
    siyah = ortak.malzeme("Luks4_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
    # Pirinç gaz haznesi (tabanda halka ayak).
    torna("Hazne", [(0, 0), (0.07, 0), (0.07, 0.012), (0.08, 0.02), (0.082, 0.07), (0.07, 0.095), (0.03, 0.106), (0, 0.106)], (0, 0, 0), pirinc, 32)
    # Pompa kolu (yanda), basınç göstergesi (önde), yakıt vanası çarkı.
    cubuk((0.08, 0, 0.05), (0.115, 0, 0.05), 0.005, nikel, 10)
    kure(0.012, (0.12, 0, 0.05), siyah, seg=12)
    S("Gosterge", 0.016, 0.012, (0, -0.083, 0.058), nikel, 20, rot=(math.pi / 2, 0, 0))
    S("Kadran", 0.012, 0.002, (0, -0.090, 0.058), kadran, 20, rot=(math.pi / 2, 0, 0))
    K("Ibre", (0.002, 0.001, 0.010), (0.002, -0.0915, 0.061), siyah, rot=(0, math.radians(-30), 0))
    cubuk((-0.04, 0.02, 0.10), (-0.05, 0.02, 0.135), 0.004, nikel, 8)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.014, minor_radius=0.003, location=(-0.05, 0.02, 0.137), major_segments=16, minor_segments=6)
    ortak.ata(bpy.context.active_object, nikel); p.append(bpy.context.active_object)
    # Orta kolon, galeri tablası, üç dikme, is tutmuş cam fanus, içinde küllenmiş gömlek.
    S("Kolon", 0.012, 0.07, (0, 0, 0.14), nikel, 16)
    torna("Galeri", [(0, 0.172), (0.072, 0.172), (0.075, 0.178), (0, 0.178)], (0, 0, 0), nikel, 32)
    torna("Fanus", [(0.056, 0.178), (0.058, 0.18), (0.058, 0.286), (0.055, 0.288), (0.053, 0.286), (0.053, 0.18), (0.054, 0.178)], (0, 0, 0), cam, 32)
    torna("IsBant", [(0.0585, 0.25), (0.0585, 0.285), (0.0578, 0.2855), (0.0578, 0.25)], (0, 0, 0), is_m, 32)
    for i in range(3):
        a = 2 * math.pi * i / 3 + 0.4
        cubuk((0.066 * math.cos(a), 0.066 * math.sin(a), 0.178), (0.066 * math.cos(a), 0.066 * math.sin(a), 0.292), 0.003, nikel, 6)
    S("Meme", 0.006, 0.045, (0, 0, 0.20), nikel, 10)
    kure(0.017, (0, 0, 0.238), gomlek, olcek=(1, 1, 1.3), seg=12)
    # Üst şapka (havalandırma kubbesi), tepe düğmesi ve yana yatık taşıma sapı.
    torna("Sapka", [(0, 0.288), (0.082, 0.288), (0.086, 0.294), (0.05, 0.326), (0.026, 0.341), (0.026, 0.356), (0, 0.360)], (0, 0, 0), nikel, 32)
    kure(0.008, (0, 0, 0.362), nikel, seg=10)
    pts = []
    fi = math.radians(55)
    for i in range(13):
        a = math.pi * i / 12
        yuk = 0.048 * math.sin(a)
        pts.append((-0.048 * math.cos(a), yuk * math.sin(fi), 0.35 + yuk * math.cos(fi)))
    yol(pts, 0.0025, nikel, 6)
    ad, h, uzak = "Gaz_Lambasi_K4", 0.4, 0.9

else:   # tebesir
    mod, kaldir = "duvar", 0.35
    tebesir = ortak.malzeme("Tebesir4_Beyaz", (0.74, 0.74, 0.70), puruz=1.0)
    silik = ortak.malzeme("Tebesir4_Silik", (0.30, 0.30, 0.28), puruz=1.0)
    Y = -0.0006                        # çizgiler duvardan 0,6 mm önde (arka yüz y = 0)

    def cizgi(x0, z0, x1, z1):
        """Tebeşir çizgisi: biri ince biri kalın iki üst üste şerit, uçları hafif titrek (elle çizilmiş)."""
        dx, dz = x1 - x0, z1 - z0
        L = math.hypot(dx, dz)
        a = math.atan2(dx, dz)
        for (gen, kay) in ((0.0055, 0.0), (0.0032, rnd.uniform(-0.0012, 0.0012))):
            K("Cizgi", (gen, 0.0012, L * rnd.uniform(0.94, 1.02)), ((x0 + x1) / 2 + kay, Y, (z0 + z1) / 2 + rnd.uniform(-0.002, 0.002)), tebesir,
              rot=(0, a + math.radians(rnd.uniform(-1.5, 1.5)), 0))

    def grup_ciz(x, z, adet=4, capraz=True):
        """Dörtlü dik çizgi ve üstünden geçen bir çapraz: beş gün."""
        for k in range(adet):
            xx = x + k * 0.026 + rnd.uniform(-0.003, 0.003)
            boy = rnd.uniform(0.078, 0.092)
            egim = rnd.uniform(-0.006, 0.006)
            cizgi(xx - egim, z - boy / 2, xx + egim, z + boy / 2)
        if capraz:
            cizgi(x - 0.014, z - 0.035, x + 0.092, z + 0.032)

    # Üst sıra: dört tam grup; alt sıra: iki tam grup ve üç çizgi (toplam 33 gün).
    for i in range(4):
        grup_ciz(-0.27 + i * 0.145, 0.085)
    for i in range(2):
        grup_ciz(-0.27 + i * 0.145, -0.075)
    grup_ciz(0.02, -0.075, 3, False)
    # Elle silinmiş eski bir sayımın beyazımsı izi (sağ altta) ve çizgi altında tebeşir tozu lekesi.
    K("Silik", (0.14, 0.001, 0.08), (0.19, -0.0005, -0.085), silik, rot=(0, math.radians(8), 0))
    K("Toz", (0.30, 0.001, 0.012), (-0.10, -0.0004, -0.135), silik)
    ad, h, uzak = "Duvar_Tebesir_K4", 0.0, 0.9

# ---------- BİTİR ----------
if acik:
    govde.name = "Govde"
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    for q in parcalar:
        ortak.yumusat(q, 35)
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, uzak)
    if "--ic" in sys.argv:
        # Kontrol görüntüsü: kapı/kapak açık (FBX ve .blend önceden yazıldı, etkilenmez).
        for q in parcalar:
            if q.name.startswith("Kapi"):
                q.rotation_euler.z = math.radians(-100)
            elif q.name.startswith("Kapak"):
                q.rotation_euler.x = math.radians(-100)
        ortak.onizleme_al(sys.argv[sys.argv.index("--ic") + 1], h, uzak * 0.9)
else:
    nesne = birles(p, ad)
    yerlestir(nesne, mod)
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [nesne], max(h, 0.1), uzak, onizlemede_kaldir=kaldir)
