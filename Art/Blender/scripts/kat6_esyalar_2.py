# kat6_esyalar_2.py
# 6. kat, B dairesi (22 numara): terzi Sabri Usta'nın evi ve atölyesi (#83). Mankenler, kumaşlar, dikiş makinesi;
# karanlıkta mankenler insan gibi görünür. A dairesinin (1974 evi) eşyaları kat6_esyalar.py içinde; yardımcılar
# o betikle aynıdır (her betik kendi başına çalışsın diye burada da duruyor). Hepsi yeni ve _K6 ekli.
#   blender -b --factory-startup --python kat6_esyalar_2.py -- <ad> [--onizleme <png>]
#
#   manken      Manken_K6             : üç ayaklı terzi mankeni (başsız, oval gövde); üstünde iğnelenmiş yarım ceket,
#                                       boynunda mezura.
#   makine      Endustriyel_Makine_K6 : sanayi tipi dikiş makinesi masası; altında motor, pedal, dizlik; makine başı,
#                                       volan ve kayış, makara ayağı, kazboynu lamba.
#   kumas       Kumas_Toplari_K6      : renkli kumaş topları; beşi arka (+y) duvara yaslı dik, üçü yerde yatık.
#   terzimasa   Terzi_Masasi_K6       : büyük kesim masası; açılmış kumaş, kâğıt kalıplar, mezura, makas, tebeşirler,
#                                       iğnelik, gönye; alt rafta katlı kumaşlar.
#   askilik     Elbise_Askisi_K6      : tekerlekli krom askılık, naylon kılıflı beş elbise/ceket.
#   dikiskutu   Dikis_Kutusu_K6       : AÇILIR (Govde + Kapak) kakmalı ahşap dikiş kutusu, içi oyuk.
#   buzdolabi   Buzdolabi_K6          : AÇILIR (Govde + Kapi_1) 80'ler küçük beyaz buzdolabı; buzluk, iki tel raf, sebzelik.
#   teneke      Dugme_Tenekesi_K6     : AÇILIR (Govde + Kapak) yuvarlak köşeli mavi bisküvi tenekesi (düğme kutusu).
#   raf         Raf_K6                : delikli köşebent metal raf; kumaş kırpıntısı dolu karton kutular.
#   sepet       Camasir_Sepeti_K6     : oval hasır çamaşır sepeti, kapağı aralık, bir gömlek kolu sarkıyor.
#   havlu       Havlu_K6              : duvar tahtasında iki kanca ve iki havlu (DUVAR eşyası).
#   ayna        Boy_Aynasi_K6         : terzinin üç kanatlı prova aynası, yan kanatlar 30° açık.
#   perdeterzi  Perde_Terzi_K6        : düz gri perde ve krom çubuğu.
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası,
# perdede çubuk yüksekliğinde arka üst kenar.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import bmesh
from mathutils import Vector, Matrix

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "dikiskutu"
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




def torna2(ad, profil, mat, kose=32, a0=0.0, a1=2 * math.pi, kalinlik=0.0, alt=False, ust=False):
    """
    Oval TORNA: profil = alttan üste (x yarıçapı, y yarıçapı, z). a0-a1 yalnızca bir açı aralığını kurar
    (tam tur değilse açık bir kabuk olur: mankenin üstündeki yarım ceket gibi).
    """
    tam = abs(a1 - a0 - 2 * math.pi) < 1e-6
    nk = kose if tam else kose + 1
    v, f = [], []
    for (rx, ry, z) in profil:
        for i in range(nk):
            a = a0 + (a1 - a0) * i / kose
            v.append((rx * math.cos(a), ry * math.sin(a), z))
    for j in range(len(profil) - 1):
        for i in range(kose):
            b = (i + 1) % nk if tam else i + 1
            f.append((j * nk + i, j * nk + b, (j + 1) * nk + b, (j + 1) * nk + i))
    if tam and alt:
        f.append(tuple(reversed(range(nk))))
    if tam and ust:
        s = (len(profil) - 1) * nk
        f.append(tuple(range(s, s + nk)))
    n = yeni_mesh(ad, v, f, mat)
    if kalinlik > 0:
        m = n.modifiers.new("Kal", "SOLIDIFY")
        m.thickness = kalinlik
    return n


def serit(noktalar, en, kal, mat):
    """Yassı şerit (mezura, kordon): her iki nokta arası ince bir kutu, yönü parça boyunca."""
    for a, b in zip(noktalar, noktalar[1:]):
        a, b = Vector(a), Vector(b)
        d = (b - a)
        L = d.length
        d.normalize()
        w = Vector((-d.y, d.x, 0))
        w = w.normalized() if w.length > 1e-3 else Vector((1, 0, 0))
        nrm = d.cross(w)
        n = K("Serit", (L + kal, en, kal), (0, 0, 0), mat)
        M = Matrix((d, w, nrm)).transposed().to_4x4()
        M.translation = (a + b) / 2
        donustur(n, M)


def uygula(n):
    """Nesnenin değiştiricilerini (pah vb.) meshe işler; boolean kesimden önce gerekli."""
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    for m in list(n.modifiers):
        bpy.ops.object.modifier_apply(modifier=m.name)


def yuvarlak_kutu(ad, olcu, merkez, r, mat):
    """Köşeleri r yarıçapla yuvarlatılmış kutu (pah uygulanmış halde; listeye EKLENMEZ)."""
    n = ortak.kutu(ad, olcu, merkez, mat)
    ortak.pah(n, r, 4)
    uygula(n)
    return n


def dondur_parcalar(bas, M):
    """p listesinde 'bas' sırasından sonra eklenen bütün parçaları aynı matrisle taşır (bir grubu döndürmek)."""
    for n in p[bas:]:
        donustur(n, M)


# ---------- ORTAK MALZEMELER (terzi evi: kumaş, metal, eski boya) ----------
metal = ortak.malzeme("K6B_Metal", (0.30, 0.30, 0.29), puruz=0.45, metal=0.85)
krom = ortak.malzeme("K6B_Krom", (0.62, 0.62, 0.60), puruz=0.2, metal=1.0)
siyah = ortak.malzeme("K6B_Siyah", (0.02, 0.02, 0.02), puruz=0.5, metal=0.4)
ahsap = ortak.malzeme("K6B_Ahsap", (0.18, 0.11, 0.05), puruz=0.6)
koyu = ortak.malzeme("K6B_KoyuAhsap", (0.06, 0.03, 0.014), puruz=0.5)
pirinc = ortak.malzeme("K6B_Pirinc", (0.50, 0.36, 0.11), puruz=0.35, metal=0.9)
KUMAS = [ortak.malzeme(f"K6B_Kumas_{i}", c, puruz=0.95) for i, c in enumerate((
    (0.03, 0.04, 0.10),   # lacivert
    (0.20, 0.03, 0.04),   # bordo
    (0.10, 0.10, 0.10),   # gri
    (0.30, 0.20, 0.10),   # deve tüyü
    (0.04, 0.10, 0.05),   # koyu yeşil
    (0.015, 0.015, 0.015),  # siyah
    (0.55, 0.50, 0.40),   # bej
    (0.12, 0.06, 0.03),   # kahve
))]
acik = False

if hangi == "manken":
    keten = ortak.malzeme("Manken_Keten", (0.36, 0.31, 0.23), puruz=0.9)
    ceket = ortak.malzeme("Manken_Ceket", (0.045, 0.045, 0.052), puruz=0.9)
    astar = ortak.malzeme("Manken_Tebesir", (0.75, 0.74, 0.70), puruz=0.9)
    sari = ortak.malzeme("Manken_Mezura", (0.55, 0.40, 0.03), puruz=0.6)
    igne = [ortak.malzeme("Manken_Igne_K", (0.45, 0.03, 0.03), puruz=0.3), ortak.malzeme("Manken_Igne_M", (0.03, 0.08, 0.40), puruz=0.3)]
    # Üç ayaklı döküm ayak ve tekerlekleri, orta boru, boy ayar bileziği.
    for k in range(3):
        a = math.radians(90 + 120 * k)
        nk = [(r * math.cos(a), r * math.sin(a), z) for (r, z) in ((0.02, 0.12), (0.09, 0.07), (0.17, 0.04), (0.22, 0.035))]
        egri(nk, 0.014, siyah, 8)
        disk(0.022, 0.014, (0.22 * math.cos(a), 0.22 * math.sin(a), 0.022), (-math.sin(a), math.cos(a), 0), siyah, 14)
    S("Gobek", 0.035, 0.05, (0, 0, 0.12), siyah, kose=16)
    cubuk((0, 0, 0.12), (0, 0, 0.98), 0.014, krom, 12)
    S("Bilezik", 0.024, 0.05, (0, 0, 0.74), siyah, kose=16)
    cubuk((0.02, 0, 0.745), (0.06, 0, 0.745), 0.005, siyah, 8)
    kure(0.012, (0.065, 0, 0.745), siyah, seg=10)
    # Gövde: oval kesitli torna; kalça, bel, göğüs, omuz, boyun (başsız).
    PR = [(0.12, 0.09, 0.95), (0.165, 0.118, 1.00), (0.172, 0.12, 1.06), (0.158, 0.112, 1.13), (0.145, 0.105, 1.19),
          (0.155, 0.11, 1.26), (0.172, 0.12, 1.33), (0.18, 0.122, 1.40), (0.178, 0.115, 1.46), (0.185, 0.095, 1.50),
          (0.15, 0.075, 1.54), (0.07, 0.055, 1.57), (0.05, 0.045, 1.59), (0.045, 0.042, 1.615)]
    torna2("Govde", PR, keten, kose=32, alt=True, ust=True)
    S("BoyunKapak", 0.048, 0.02, (0, 0, 1.625), koyu, kose=20)
    kure(0.016, (0, 0, 1.64), koyu, seg=10)
    # Yarım ceket: gövdenin sağ yarısını (+x; ön ortadan arka ortaya) saran ince kabuk, 1,4 cm açıkta.
    CP = [(rx + 0.014, ry + 0.014, z) for (rx, ry, z) in PR if 0.97 <= z <= 1.50]
    CP.insert(0, (0.172, 0.126, 0.97))
    torna2("Ceket", CP, ceket, kose=20, a0=-math.pi / 2, a1=math.pi / 2, kalinlik=0.004)

    def yuzey(prof, x, z, ek):
        """Profilin ön yüzünün (x, z) noktasındaki y'si: yarıçaplar z'ye göre ara değerlenir, ek kadar dışarıda."""
        for (r0, q0, z0), (r1, q1, z1) in zip(prof, prof[1:]):
            if z0 <= z <= z1:
                t = (z - z0) / (z1 - z0)
                rx, ry = r0 + (r1 - r0) * t, q0 + (q1 - q0) * t
                return -(ry + ek) * math.sqrt(max(0.0, 1 - (x / (rx + ek)) ** 2))
        return -0.13

    def yuzey_y(x, z, ek):
        return yuzey(CP, x, z, ek)

    # Yaka devrik ucu (klapa): ceket yüzeyine oturan ince levha.
    klapa = [(0.0, 1.22), (0.02, 1.22), (0.105, 1.43), (0.08, 1.475), (0.045, 1.45), (0.004, 1.30)]
    v = [(x, yuzey_y(x, z, 0.006), z) for (x, z) in klapa]
    kl = yeni_mesh("Klapa", v, [tuple(range(len(v)))], ceket)
    m = kl.modifiers.new("Kal", "SOLIDIFY")
    m.thickness = 0.004
    # Ön kenar boyunca iğneler (renkli başlı) ve omuz dikişi boyunca teyel iğneleri.
    for i in range(8):
        z = 1.00 + i * 0.055
        y = yuzey_y(0.012, z, 0.006)
        cubuk((0.012, y + 0.004, z), (0.012, y - 0.004, z + 0.004), 0.0008, krom, 5)
        kure(0.0035, (0.012, y - 0.005, z + 0.005), igne[i % 2], seg=8)
    for i in range(5):
        a = math.radians(80 - i * 18)
        x, y = (0.17 + 0.02) * math.cos(a) * 0.95, (0.10 + 0.02) * math.sin(a)
        kure(0.0035, (x, y, 1.515), igne[(i + 1) % 2], seg=8)
    # Düğmeler ve terzi tebeşiri çizgileri.
    for z in (1.12, 1.22):
        disk(0.011, 0.004, (0.04, yuzey_y(0.04, z, 0.018), z), (0, -1, 0), siyah, 14)
    for (x0, z0, x1, z1) in ((0.06, 1.05, 0.14, 1.07), (0.13, 1.30, 0.15, 1.42)):
        serit([(x0, yuzey_y(x0, z0, 0.019), z0), (x1, yuzey_y(x1, z1, 0.019), z1)], 0.004, 0.001, astar)
    # Boynuna asılı mezura: iki ucu önden sarkar.
    sol = [(0.0, 0.052, 1.60), (-0.065, 0.02, 1.585)] + [(-0.085, yuzey(PR, -0.085, z, 0.006), z) for z in (1.555, 1.52, 1.47, 1.40, 1.32, 1.22)]
    sag = [(0.0, 0.052, 1.60), (0.065, 0.02, 1.585), (0.08, yuzey(PR, 0.08, 1.555, 0.006), 1.555)] +           [(0.08, yuzey(CP, 0.08, z, 0.024), z) for z in (1.49, 1.42, 1.34, 1.18)]
    serit(sag, 0.016, 0.0015, sari)
    serit(sol, 0.016, 0.0015, sari)
    ad, h, uzak = "Manken_K6", 1.65, 2.4

elif hangi == "makine":
    formika = ortak.malzeme("Makine_Formika", (0.40, 0.36, 0.28), puruz=0.5)
    emaye = ortak.malzeme("Makine_Emaye", (0.08, 0.11, 0.09), puruz=0.35, metal=0.3)
    iplik = [ortak.malzeme("Makine_Iplik_B", (0.75, 0.74, 0.70), puruz=0.8), KUMAS[0]]
    kumas = KUMAS[2]
    EN, DER, H = 1.10, 0.55, 0.75
    K("Tabla", (EN, DER, 0.035), (0, 0, H - 0.0175), formika, 0.004)
    K("TablaKenar", (EN + 0.004, 0.006, 0.035), (0, -DER / 2, H - 0.0175), siyah)
    # Demir ayaklar (T biçimli), alt kuşak, tabla altı taşıyıcılar.
    for x in (-0.50, 0.50):
        K("Dikme", (0.045, 0.045, H - 0.075), (x, 0, 0.04 + (H - 0.075) / 2), metal, 0.004)
        K("Taban", (0.055, DER, 0.04), (x, 0, 0.02), metal, 0.006)
        K("Tasiyici", (0.04, DER - 0.05, 0.03), (x, 0, H - 0.05), metal, 0.003)
    K("Kusak", (1.0, 0.03, 0.03), (0, 0, 0.22), metal, 0.003)
    # Tabla altında debriyajlı motor, kasnağı, askı plakası; ayak pedalı ve çekme kolu; dizlik.
    cubuk((0.22, 0.0, 0.61), (0.46, 0.0, 0.61), 0.08, emaye, 20)
    disk(0.08, 0.02, (0.47, 0.0, 0.61), (1, 0, 0), siyah, 20)
    disk(0.035, 0.02, (0.19, 0.0, 0.61), (1, 0, 0), siyah, 16)
    K("MotorAski", (0.20, 0.10, 0.10), (0.34, 0.0, 0.68), metal, 0.004)
    K("MotorKol", (0.02, 0.10, 0.02), (0.30, -0.10, 0.53), metal, 0.002)
    n = K("Pedal", (0.14, 0.26, 0.02), (0.28, -0.08, 0.06), siyah, 0.004)
    donustur(n, don("X", math.radians(-10), (0.28, 0.05, 0.06)))
    K("PedalAyak", (0.06, 0.06, 0.05), (0.28, 0.04, 0.025), metal, 0.003)
    cubuk((0.29, -0.17, 0.085), (0.30, -0.14, 0.525), 0.004, metal, 6)
    K("Dizlik", (0.06, 0.02, 0.10), (-0.05, -0.22, 0.62), metal, 0.003)
    cubuk((-0.05, -0.21, 0.67), (-0.05, -0.05, 0.71), 0.006, metal, 6)
    # Sol altta küçük çekmece.
    K("Cekmece", (0.36, 0.40, 0.08), (-0.30, 0.0, 0.67), formika, 0.003)
    K("CekmeceOn", (0.36, 0.015, 0.08), (-0.30, -0.207, 0.67), formika, 0.003)
    K("CekmeceKulp", (0.08, 0.012, 0.012), (-0.30, -0.22, 0.67), krom, 0.003)
    # Makine başı: yatak, dikme, kol, kafa, iğne ve baskı ayağı, volan; kayış iki kol halinde motora iner.
    K("Yatak", (0.48, 0.18, 0.035), (-0.06, -0.02, H + 0.0175), emaye, 0.006)
    K("Plaka", (0.08, 0.10, 0.002), (-0.25, -0.03, H + 0.036), krom)
    K("Sutun", (0.07, 0.10, 0.26), (0.15, 0.0, H + 0.035 + 0.13), emaye, 0.015)
    K("Kol", (0.44, 0.09, 0.08), (-0.04, 0.0, 1.01), emaye, 0.025)
    K("Kafa", (0.09, 0.10, 0.22), (-0.255, 0.0, 0.95), emaye, 0.012)
    K("KafaKapak", (0.004, 0.08, 0.14), (-0.302, 0.0, 0.95), krom, 0.002)
    cubuk((-0.25, -0.03, 0.84), (-0.25, -0.03, 0.795), 0.005, krom, 8)
    K("BaskiAyagi", (0.03, 0.02, 0.008), (-0.25, -0.03, 0.791), krom, 0.002)
    cubuk((-0.23, -0.03, 0.84), (-0.23, -0.03, 0.80), 0.004, krom, 8)
    disk(0.065, 0.028, (0.20, 0.0, 1.0), (1, 0, 0), siyah, 28)
    disk(0.02, 0.035, (0.20, 0.0, 1.0), (1, 0, 0), krom, 12)
    for sy in (-1, 1):
        cubuk((0.20, sy * 0.062, 1.0), (0.19, sy * 0.034, 0.61), 0.004, siyah, 6)
    # Makara ayağı: tablanın arka sağında dikey çubuk, üstte kılavuz, iki koni iplik.
    cubuk((0.40, 0.20, H), (0.40, 0.20, 1.19), 0.005, krom, 8)
    cubuk((0.30, 0.20, 1.19), (0.50, 0.20, 1.19), 0.004, krom, 8)
    K("MakaraTaban", (0.22, 0.08, 0.01), (0.40, 0.20, H + 0.005), metal, 0.002)
    for i, x in enumerate((0.34, 0.46)):
        torna("Koni", [(0.035, 0.0), (0.03, 0.08), (0.015, 0.14), (0.01, 0.15)], iplik[i], kose=20, konum=(x, 0.20, H + 0.01))
        halka(0.008, 0.002, (x, 0.20, 1.18), (0, 0, 1), krom, seg=10)
    # Kazboynu lamba: kafanın üstünden öne kıvrılır; aşağı bakan başlık.
    egri([(-0.22, 0.03, 1.06), (-0.20, 0.03, 1.13), (-0.25, -0.01, 1.16), (-0.30, -0.06, 1.14)], 0.006, siyah, 8)
    torna("Lamba", [(0.04, 1.075), (0.032, 1.10), (0.018, 1.13), (0.012, 1.145)], siyah, kose=20, konum=(-0.31, -0.07, 0), alt=False, kalinlik=0.002)
    # Tablada dikilmekte olan kumaş.
    K("Kumas", (0.32, 0.30, 0.004), (-0.40, -0.05, H + 0.002), kumas, 0.001)
    K("Kumas2", (0.20, 0.14, 0.003), (-0.22, -0.05, H + 0.0385), kumas, 0.001)
    ad, h, uzak = "Endustriyel_Makine_K6", 1.2, 2.0

elif hangi == "kumas":
    tup = ortak.malzeme("Kumas6_Karton", (0.22, 0.14, 0.07), puruz=0.9)
    # Arka (+y) duvara yaslı dik toplar: altı yerde, tepesi duvara (y ≈ 0,30) dayanır.
    dik = [(-0.50, 0.070, 1.45, 0), (-0.34, 0.060, 1.40, 1), (-0.19, 0.075, 1.50, 3), (-0.03, 0.055, 1.35, 5), (0.11, 0.065, 1.45, 4)]
    for (x, r, L, ci) in dik:
        e = math.radians(9)
        a = Vector((x, 0.30 - r - L * math.sin(e) - 0.01, r * 0.17))
        b = a + Vector((rnd.uniform(-0.03, 0.03), L * math.sin(e), L * math.cos(e)))
        cubuk(tuple(a + Vector((0, 0, r * 0.0))), tuple(b), r, KUMAS[ci], 20)
        d = (b - a).normalized()
        cubuk(tuple(b), tuple(b + d * 0.012), 0.022, tup, 12)
        cubuk(tuple(b - d * 0.01), tuple(b + d * 0.003), r + 0.003, KUMAS[ci], 20)     # kumaşın kenar katı
    # Yerde yatık toplar: altta iki, üstünde bir; biri açılmış, ucu yerde sürünüyor.
    yat = [(0.10, -0.25, 0.065, 1.30, 1), (0.10, -0.12, 0.065, 1.20, 7), (0.10, -0.185, 0.06, 1.25, 6)]
    for i, (x, y, r, L, ci) in enumerate(yat):
        z = r if i < 2 else 0.065 * 2 + r - 0.018
        cubuk((x - L / 2 + 0.15, y, z), (x + L / 2 - 0.15, y, z), r, KUMAS[ci], 20)
        for sx in (-1, 1):
            cubuk((x + sx * (L / 2 - 0.15), y, z), (x + sx * (L / 2 - 0.15 + 0.012), y, z), 0.022, tup, 12)
    K("Acilan", (0.50, 0.30, 0.003), (0.10, -0.46, 0.0015), KUMAS[1], 0.001)
    ad, h, uzak = "Kumas_Toplari_K6", 1.5, 2.6

elif hangi == "terzimasa":
    kraft = ortak.malzeme("Terzi_Kraft", (0.40, 0.30, 0.16), puruz=0.9)
    kraft2 = ortak.malzeme("Terzi_Kraft2", (0.55, 0.45, 0.28), puruz=0.9)
    sari = ortak.malzeme("Terzi_Mezura", (0.55, 0.40, 0.03), puruz=0.6)
    tebesir = [ortak.malzeme("Terzi_Tebesir_M", (0.10, 0.22, 0.50), puruz=0.9), ortak.malzeme("Terzi_Tebesir_B", (0.80, 0.80, 0.76), puruz=0.9)]
    kirmizi = ortak.malzeme("Terzi_Iğnelik", (0.40, 0.04, 0.03), puruz=0.8)
    EN, DER, H = 1.60, 0.80, 0.85
    K("Tabla", (EN, DER, 0.05), (0, 0, H - 0.025), ahsap, 0.006)
    for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
        for y in (-DER / 2 + 0.06, DER / 2 - 0.06):
            K("Ayak", (0.07, 0.07, H - 0.05), (x, y, (H - 0.05) / 2), ahsap, 0.006)
    for y in (-DER / 2 + 0.06, DER / 2 - 0.06):
        K("Etek", (EN - 0.12, 0.025, 0.10), (0, y, H - 0.10), ahsap, 0.004)
    for x in (-EN / 2 + 0.06, EN / 2 - 0.06):
        K("EtekY", (0.025, DER - 0.12, 0.10), (x, 0, H - 0.10), ahsap, 0.004)
    K("AltRaf", (EN - 0.10, DER - 0.10, 0.02), (0, 0, 0.20), ahsap, 0.004)
    # Alt rafta katlanmış kumaş yığınları.
    for i, x in enumerate((-0.45, 0.0, 0.45)):
        for j in range(3 + i % 2):
            K("Katli", (0.34 + rnd.uniform(-0.02, 0.02), 0.26, 0.045), (x + rnd.uniform(-0.02, 0.02), rnd.uniform(-0.02, 0.02), 0.2325 + j * 0.046), KUMAS[(i * 3 + j) % 8], 0.012)
    Z = H
    # Tablada açılmış lacivert kumaş ve üstünde ceket ön parçası kalıbı; sağda kol ve sırt kalıpları.
    K("Kumas", (0.80, 0.55, 0.003), (-0.30, 0.02, Z + 0.0015), KUMAS[0], 0.001)
    prizma("KalipOn", [(-0.62, -0.20), (-0.26, -0.20), (-0.20, 0.02), (-0.25, 0.16), (-0.36, 0.22), (-0.47, 0.18), (-0.55, 0.22), (-0.62, 0.10)], Z + 0.003, Z + 0.0042, kraft)
    prizma("KalipSirt", [(0.20, 0.02), (0.62, 0.04), (0.66, 0.30), (0.52, 0.36), (0.40, 0.33), (0.26, 0.36), (0.18, 0.28)], Z, Z + 0.0012, kraft2)
    prizma("KalipKol", [(0.06, -0.33), (0.40, -0.32), (0.46, -0.08), (0.36, 0.03), (0.24, 0.07), (0.12, 0.03), (0.04, -0.08)], Z + 0.0012, Z + 0.0024, kraft)
    for (x0, y0, x1, y1) in ((-0.58, -0.05, -0.28, -0.05), (-0.45, -0.18, -0.45, 0.15)):
        serit([(x0, y0, Z + 0.0045), (x1, y1, Z + 0.0045)], 0.003, 0.0008, tebesir[1])
    # Mezura: sağ önde sarılı rulosu, tablaya uzanıp ön kenardan sarkar.
    S("MezuraRulo", 0.035, 0.02, (0.62, -0.26, Z + 0.01), sari, kose=20)
    serit([(0.59, -0.27, Z + 0.004), (0.40, -0.36, Z + 0.004), (0.10, -0.37, Z + 0.004), (-0.10, -0.35, Z + 0.006),
           (-0.30, -0.38, Z + 0.004), (-0.40, -0.40, Z + 0.001), (-0.405, -0.402, Z - 0.03), (-0.41, -0.403, Z - 0.26)], 0.016, 0.0015, sari)
    # Terzi makası: iki uzun ağız, siyah halkalı sapları; 25° döndürülmüş.
    bas = len(p)
    prizma("Agiz", [(-0.01, -0.007), (0.23, 0.002), (0.23, 0.008), (0.0, 0.011), (-0.02, 0.004)], Z + 0.001, Z + 0.005, krom)
    prizma("Agiz", [(-0.01, 0.007), (0.23, -0.004), (0.23, -0.010), (0.0, -0.012), (-0.02, -0.004)], Z + 0.005, Z + 0.009, krom)
    disk(0.008, 0.012, (0.0, 0.0, Z + 0.006), (0, 0, 1), krom, 12)
    cubuk((-0.01, 0.005, Z + 0.004), (-0.07, 0.03, Z + 0.004), 0.006, siyah, 8)
    cubuk((-0.01, -0.005, Z + 0.008), (-0.07, -0.035, Z + 0.008), 0.006, siyah, 8)
    halka(0.024, 0.006, (-0.095, 0.035, Z + 0.006), (0, 0, 1), siyah, seg=16)
    halka(0.032, 0.006, (-0.10, -0.045, Z + 0.008), (0, 0, 1), siyah, seg=16, olcek=(1.2, 1, 1))
    dondur_parcalar(bas, Matrix.Translation((0.25, 0.14, 0)) @ Matrix.Rotation(math.radians(25), 4, "Z"))
    # Terzi tebeşirleri (üçgen), iğnelik (domates) ve iğneleri, L gönye.
    for i, (x, y, a) in enumerate(((0.55, 0.22, 0.3), (0.02, 0.30, 1.2))):
        pts = [(x + 0.03 * math.cos(a + k * 2.094), y + 0.03 * math.sin(a + k * 2.094)) for k in range(3)]
        prizma("Tebesir", pts, Z + (0.0012 if i == 0 else 0.0), Z + 0.008, tebesir[i])
    kure(0.04, (-0.62, -0.32, Z + 0.022), kirmizi, olcek=(1, 1, 0.6), seg=16)
    for k in range(7):
        a = math.radians(k * 51)
        q = Vector((-0.62 + 0.025 * math.cos(a), -0.32 + 0.025 * math.sin(a), Z + 0.035))
        cubuk(tuple(q), tuple(q + Vector((0.3 * math.cos(a), 0.3 * math.sin(a), 1)).normalized() * 0.02), 0.0008, krom, 5)
        kure(0.003, tuple(q + Vector((0.3 * math.cos(a), 0.3 * math.sin(a), 1)).normalized() * 0.021), KUMAS[k % 3], seg=6)
    K("GonyeUzun", (0.60, 0.035, 0.004), (0.36, -0.15 + 0.0, Z + 0.0045), ahsap, 0.001)
    K("GonyeKisa", (0.035, 0.30, 0.004), (0.675, 0.0, Z + 0.0045), ahsap, 0.001)
    ad, h, uzak = "Terzi_Masasi_K6", 0.9, 2.2

elif hangi == "askilik":
    naylon = ortak.malzeme("Askilik_Naylon", (0.30, 0.31, 0.33), puruz=0.2)
    naylon2 = ortak.malzeme("Askilik_Naylon2", (0.26, 0.27, 0.29), puruz=0.25)
    etiket = ortak.malzeme("Askilik_Etiket", (0.75, 0.72, 0.62), puruz=0.9)
    X = 0.58
    # Krom askılık: iki dikme, üst boru, tabanda iki ayak borusu ve dört tekerlek.
    for x in (-X, X):
        cubuk((x, 0, 0.07), (x, 0, 1.60), 0.014, krom, 12)
        cubuk((x, -0.24, 0.07), (x, 0.24, 0.07), 0.015, krom, 12)
        for y in (-0.22, 0.22):
            K("Catal", (0.02, 0.03, 0.03), (x, y, 0.05), krom, 0.003)
            disk(0.028, 0.02, (x, y, 0.029), (1, 0, 0), siyah, 16)
    cubuk((-X - 0.02, 0, 1.60), (X + 0.02, 0, 1.60), 0.014, krom, 12)
    cubuk((-X, 0, 0.30), (X, 0, 0.30), 0.010, krom, 10)
    # Beş askı: tel askı kancası, naylon kılıflı elbise/ceket; kılıfın altından giysinin eteği görünür.
    giysi = [(-0.42, 0, 0.52, 0.12), (-0.21, 1, 0.35, 0.10), (0.0, 7, 0.55, 0.12), (0.21, 5, 0.25, 0.09), (0.42, 3, 0.52, 0.11)]
    for i, (x, ci, alt, kal) in enumerate(giysi):
        halka(0.018, 0.0022, (x, 0.0, 1.605), (1, 0, 0), metal, seg=12)
        cubuk((x, 0.0, 1.587), (x, 0.0, 1.55), 0.0022, metal, 5)
        egri([(x, -0.20, 1.47), (x, 0.0, 1.55), (x, 0.20, 1.47), (x, -0.20, 1.47)], 0.0025, metal, 5)
        prizma("Kilif", [(-0.23, 1.44), (-0.06, 1.535), (0.06, 1.535), (0.23, 1.44), (0.245, alt + 0.30), (-0.245, alt + 0.30)],
               -kal / 2, kal / 2, naylon if i % 2 == 0 else naylon2, "xz")
        # prizma 'xz' düzleminde, merkezde kurulur; askıdaki giysi y boyunca uzandığı için z çevresinde 90°
        # döndürülüp askıdaki yerine (x) taşınır.
        donustur(p[-1], Matrix.Translation((x, 0, 0)) @ Matrix.Rotation(math.pi / 2, 4, "Z"))
        K("Etek", (kal - 0.02, 0.44, 0.34), (x, 0.0, alt + 0.15), KUMAS[ci], 0.01)
        K("Etiket", (0.002, 0.04, 0.06), (x + kal / 2 + 0.002, 0.10, 1.40), etiket)
    ad, h, uzak = "Elbise_Askisi_K6", 1.6, 2.6

elif hangi == "dikiskutu":
    acik = True
    kakma = ortak.malzeme("DikisK_Kakma", (0.30, 0.20, 0.09), puruz=0.5)
    makara = ortak.malzeme("DikisK_Makara", (0.30, 0.18, 0.08), puruz=0.6)
    EN, DER, H = 0.35, 0.22, 0.13            # gövde; kapakla toplam 0,16
    T = 0.01
    x0, x1, y0, y1 = -EN / 2, EN / 2, -DER / 2, DER / 2
    lev = ac.levha
    p += [lev("Taban", x0, x1, y0, y1, 0.0, 0.012, koyu, 0.003),
          lev("On", x0, x1, y0, y0 + T, 0.012, H, koyu, 0.002),
          lev("Arka", x0, x1, y1 - T, y1, 0.012, H, koyu, 0.002),
          lev("Yan", x0, x0 + T, y0 + T, y1 - T, 0.012, H, koyu, 0.002),
          lev("Yan", x1 - T, x1, y0 + T, y1 - T, 0.012, H, koyu, 0.002),
          lev("Serit", x0 - 0.002, x1 + 0.002, y0 - 0.002, y0, 0.03, 0.04, kakma, 0.0)]
    # Bir köşede bir makara ve yüksük (kutunun geri kalanı boş: anahtar parçası konacak).
    S("Makara", 0.016, 0.035, (x0 + 0.03, y1 - 0.03, 0.012 + 0.0175), makara, kose=14)
    S("Iplik", 0.014, 0.028, (x0 + 0.03, y1 - 0.03, 0.012 + 0.0175), KUMAS[1], kose=14)
    torna("Yuksuk", [(0.009, 0.0), (0.0085, 0.015), (0.006, 0.02)], krom, kose=14, konum=(x0 + 0.065, y1 - 0.025, 0.012))
    K("Kilit", (0.03, 0.004, 0.025), (0, y0 - 0.002, H - 0.012), pirinc, 0.001)
    for x in (-0.10, 0.10):
        K("Mentese", (0.03, 0.003, 0.02), (x, y1 + 0.0015, H - 0.005), pirinc, 0.001)
    kp = [lev("KapakTabla", x0 - 0.002, x1 + 0.002, y0 - 0.002, y1 + 0.002, H + 0.002, H + 0.03, koyu, 0.004),
          prizma("Kakma", [(0, -0.07), (0.12, 0), (0, 0.07), (-0.12, 0)], H + 0.03, H + 0.0315, kakma),
          lev("KapakDil", -0.012, 0.012, y0 - 0.006, y0 - 0.002, H - 0.004, H + 0.02, pirinc, 0.001)]
    p.remove(kp[1])                 # prizma listeye kendiliğinden eklenir; kapağa ait olduğu için gövdeden çıkar
    kapak = ortak.birlestir(kp, "Kapak")
    ortak.pivotu_tasi(kapak, (0, y1 + 0.002, H + 0.03))
    parcalar.append(kapak)
    ad, h, uzak = "Dikis_Kutusu_K6", 0.2, 0.7

elif hangi == "buzdolabi":
    acik = True
    beyaz = ortak.malzeme("Buz6_Beyaz", (0.78, 0.78, 0.74), puruz=0.3)
    astar = ortak.malzeme("Buz6_Astar", (0.66, 0.67, 0.63), puruz=0.35)
    conta = ortak.malzeme("Buz6_Conta", (0.12, 0.12, 0.12), puruz=0.6)
    izgara = ortak.malzeme("Buz6_Izgara", (0.03, 0.03, 0.03), puruz=0.5)
    sebzelik = ortak.malzeme("Buz6_Sebzelik", (0.45, 0.50, 0.48), puruz=0.15)
    EN, Y0, Y1, Z0, Z1, T = 0.55, -0.24, 0.29, 0.06, 1.25, 0.035
    X = EN / 2
    lev = ac.levha
    # Gövde kabuğu: kalın (yalıtımlı) yan, üst, alt, arka levhalar; önü açık. Altta ızgaralı kaide.
    p += [lev("Yan", -X, -X + T, Y0, Y1, Z0, Z1, beyaz, 0.006),
          lev("Yan", X - T, X, Y0, Y1, Z0, Z1, beyaz, 0.006),
          lev("Ust", -X + T, X - T, Y0, Y1, Z1 - T, Z1, beyaz, 0.004),
          lev("Alt", -X + T, X - T, Y0, Y1, Z0, Z0 + T, astar, 0.002),
          lev("Arka", -X + T, X - T, Y1 - T, Y1, Z0 + T, Z1 - T, astar, 0.0),
          lev("Kaide", -X + 0.01, X - 0.01, Y0 + 0.02, Y1 - 0.01, 0.0, Z0, conta, 0.004),
          lev("Izgara", -X + 0.03, X - 0.03, Y0 + 0.015, Y0 + 0.02, 0.01, Z0 - 0.01, izgara, 0.0)]
    for x in (-X + 0.04, X - 0.04):
        K("Ayak", (0.04, 0.04, 0.012), (x, Y0 + 0.05, 0.006), conta)
    # İç: üstte küçük buzluk (kapaklı), altında lamba kutusu; iki tel raf; altta sebzelik ve cam örtüsü.
    K("Buzluk", (0.40, 0.39, 0.17), (0, 0.055, 1.12), astar, 0.01)
    K("BuzlukKapak", (0.40, 0.012, 0.16), (0, -0.146, 1.12), sebzelik, 0.004)
    K("BuzlukKulp", (0.08, 0.015, 0.015), (0, -0.157, 1.06), astar, 0.004)
    K("LambaKutusu", (0.08, 0.05, 0.025), (0, 0.10, 1.022), astar, 0.004)
    S("Ampul", 0.012, 0.02, (0, 0.10, 1.0), ortak.malzeme("Buz6_Ampul", (0.8, 0.78, 0.7), puruz=0.2), kose=12)
    for zr in (0.50, 0.78):
        for x in (-X + T, X - T):
            K("Ray", (0.01, 0.38, 0.008), (x - 0.005 if x > 0 else x + 0.005, 0.05, zr - 0.012), astar)
        K("RafOn", (2 * (X - T) - 0.004, 0.008, 0.008), (0, -0.155, zr - 0.004), krom)
        K("RafArka", (2 * (X - T) - 0.004, 0.008, 0.008), (0, 0.245, zr - 0.004), krom)
        for i in range(12):
            x = -X + T + 0.01 + i * (2 * (X - T) - 0.02) / 11
            K("RafTel", (0.004, 0.40, 0.004), (x, 0.045, zr - 0.004), krom)
    for (a, b, c, d) in ((-0.22, 0.22, -0.162, -0.155), (-0.22, 0.22, 0.235, 0.242), (-0.22, -0.213, -0.155, 0.235), (0.213, 0.22, -0.155, 0.235)):
        p.append(lev("Sebzelik", a, b, c, d, Z0 + T, 0.235, sebzelik, 0.0))
    p.append(lev("SebzelikCam", -0.235, 0.235, -0.165, 0.25, 0.235, 0.245, sebzelik, 0.002))
    for zb in (Z0, Z1):
        K("MenteseKapak", (0.03, 0.03, 0.012), (X - 0.02, -0.265, zb + (0.006 if zb == Z1 else -0.006)), astar, 0.003)
    # Kapı: dış levha, contası, iç astar ve üç kapı balkonu; sol kenarda dikey kulp; sağ kenardan menteşeli.
    kp = [lev("Kapi", -X, X, -0.29, -0.25, Z0 + 0.005, Z1 - 0.005, beyaz, 0.012),
          lev("Conta", -0.255, 0.255, -0.25, -0.24, 0.08, 0.095, conta, 0.0),
          lev("Conta", -0.255, 0.255, -0.25, -0.24, 1.215, 1.23, conta, 0.0),
          lev("Conta", -0.255, -0.24, -0.25, -0.24, 0.095, 1.215, conta, 0.0),
          lev("Conta", 0.24, 0.255, -0.25, -0.24, 0.095, 1.215, conta, 0.0),
          lev("KapiAstar", -0.22, 0.22, -0.24, -0.225, 0.12, 1.19, astar, 0.004),
          lev("Kulp", -X + 0.03, -X + 0.055, -0.305, -0.29, 0.80, 1.05, astar, 0.005),
          lev("Marka", -0.06, 0.06, -0.2915, -0.29, 1.17, 1.19, krom, 0.0)]
    for zb in (0.30, 0.62, 0.92):
        kp.append(lev("Balkon", -0.20, 0.20, -0.225, -0.175, zb, zb + 0.008, astar, 0.0))
        kp.append(lev("BalkonOn", -0.20, 0.20, -0.18, -0.175, zb, zb + 0.05, sebzelik, 0.0))
    kapi = ortak.birlestir(kp, "Kapi_1")
    ortak.pivotu_tasi(kapi, (X, -0.27, Z0 + 0.005))
    parcalar.append(kapi)
    ad, h, uzak = "Buzdolabi_K6", 1.3, 2.0

elif hangi == "teneke":
    acik = True
    mavi = ortak.malzeme("DugmeT_Mavi", (0.025, 0.06, 0.22), puruz=0.35, metal=0.5)
    yaldiz = ortak.malzeme("DugmeT_Yaldiz", (0.55, 0.42, 0.12), puruz=0.3, metal=0.9)
    krem = ortak.malzeme("DugmeT_Krem", (0.72, 0.68, 0.56), puruz=0.5)
    resim = [ortak.malzeme(f"DugmeT_Resim_{i}", c, puruz=0.5) for i, c in enumerate(((0.35, 0.05, 0.05), (0.08, 0.25, 0.08), (0.50, 0.35, 0.10)))]
    EN, DER, H = 0.22, 0.16, 0.086
    # Yuvarlak köşeli teneke gövde: dolu kutudan aynı biçimde ama küçük bir kutu oyulur (et 4 mm, taban 4 mm).
    g = yuvarlak_kutu("Teneke", (EN, DER, H), (0, 0, H / 2), 0.02, mavi)
    ortak.oy(g, yuvarlak_kutu("Ic", (EN - 0.008, DER - 0.008, 0.12), (0, 0, 0.004 + 0.06), 0.016, mavi))
    p.append(g)
    # Alt yaldız bant: gövdeyi saran halka (dış kabuk eksi iç).
    bnt = yuvarlak_kutu("Bant", (EN + 0.002, DER + 0.002, 0.012), (0, 0, 0.018), 0.021, yaldiz)
    ortak.oy(bnt, yuvarlak_kutu("BantIc", (EN - 0.004, DER - 0.004, 0.02), (0, 0, 0.018), 0.018, yaldiz))
    p.append(bnt)
    # İçte bir köşede birkaç düğme (geri kalanı boş).
    for i in range(5):
        S("Dugme", rnd.uniform(0.006, 0.01), 0.003, (0.07 + rnd.uniform(-0.015, 0.015), 0.045 + rnd.uniform(-0.012, 0.012), 0.0055 + i * 0.0005), resim[i % 3], kose=12)
    # Kapak: gövdenin dışına geçen yuvarlak köşeli kapak; üstünde krem resim alanı ve yaldız çerçeve.
    k = yuvarlak_kutu("Kapak", (EN + 0.012, DER + 0.012, 0.022), (0, 0, 0.089), 0.022, mavi)
    ortak.oy(k, yuvarlak_kutu("KapakIc", (EN + 0.002, DER + 0.002, 0.02), (0, 0, 0.078), 0.02, mavi))
    kp = [k,
          ortak.kutu("Resim", (0.15, 0.09, 0.001), (0, 0, 0.1005), krem),
          ortak.kutu("Cerceve", (0.16, 0.006, 0.0012), (0, -0.048, 0.1006), yaldiz),
          ortak.kutu("Cerceve", (0.16, 0.006, 0.0012), (0, 0.048, 0.1006), yaldiz),
          ortak.kutu("Cerceve", (0.006, 0.09, 0.0012), (-0.078, 0, 0.1006), yaldiz),
          ortak.kutu("Cerceve", (0.006, 0.09, 0.0012), (0.078, 0, 0.1006), yaldiz),
          ortak.kutu("Ev", (0.04, 0.03, 0.0008), (-0.02, 0.0, 0.1012), resim[0]),
          ortak.kutu("Cati", (0.05, 0.012, 0.0008), (-0.02, 0.019, 0.1012), resim[2]),
          ortak.kutu("Agac", (0.025, 0.035, 0.0008), (0.035, 0.005, 0.1012), resim[1]),
          ortak.kutu("Yol", (0.08, 0.008, 0.0008), (0.0, -0.028, 0.1012), resim[2])]
    kapak = ortak.birlestir(kp, "Kapak")
    ortak.pivotu_tasi(kapak, (0, DER / 2 + 0.006, 0.10))
    parcalar.append(kapak)
    ad, h, uzak = "Dugme_Tenekesi_K6", 0.15, 0.55

elif hangi == "raf":
    boya = ortak.malzeme("Raf6B_Boya", (0.10, 0.13, 0.11), puruz=0.55, metal=0.6)     # çekiçli yeşil-gri boya
    delik = ortak.malzeme("Raf6B_Delik", (0.01, 0.01, 0.01), puruz=0.9)
    karton = ortak.malzeme("Raf6B_Karton", (0.28, 0.18, 0.09), puruz=0.9)
    etiket = ortak.malzeme("Raf6B_Etiket", (0.72, 0.70, 0.62), puruz=0.9)
    EN, DER, H = 1.20, 0.40, 1.80
    X, Y = EN / 2, DER / 2
    # Delikli köşebent dikmeler (L profil): her köşede iki levha ve üstlerinde delik sırası.
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * X, sy * Y
            K("L", (0.04, 0.003, H), (x - sx * 0.02, y, H / 2), boya)
            K("L", (0.003, 0.04, H), (x, y - sy * 0.02, H / 2), boya)
            for i in range(22):
                z = 0.06 + i * 0.08
                K("Delik", (0.008, 0.001, 0.016), (x - sx * 0.02, y + sy * 0.002, z), delik)
                K("Delik", (0.001, 0.008, 0.016), (x + sx * 0.002, y - sy * 0.02, z), delik)
    raflar = (0.08, 0.50, 0.92, 1.34, 1.76)
    for z in raflar:
        K("Raf", (EN, DER, 0.004), (0, 0, z - 0.002), boya, 0.001)
        K("RafKenar", (EN, 0.004, 0.03), (0, -Y, z - 0.015), boya)
        K("RafKenar", (EN, 0.004, 0.03), (0, Y, z - 0.015), boya)
    for (za, zb, s) in ((0.10, 0.90, 1), (0.10, 0.90, -1)):
        serit([(-s * (X - 0.03), Y + 0.004, za), (s * (X - 0.03), Y + 0.004, zb)], 0.025, 0.003, boya)
    # Karton kutular: kapakları açık, içlerinden renkli kumaş kırpıntıları taşar; önlerinde etiket.
    for zi, z in enumerate(raflar[:4]):
        x = -X + 0.03
        while x < X - 0.30:
            w, d, hh = rnd.uniform(0.30, 0.42), rnd.uniform(0.28, 0.34), rnd.uniform(0.20, 0.28)
            if x + w > X - 0.03:
                break
            cx = x + w / 2
            K("Kutu", (w, d, hh), (cx, 0.02, z + hh / 2), karton, 0.003)
            for yon in (-1, 1):
                # Ön kapak öne doğru sarkık açık, arka kapak hafif geriye yatık dik (raf arkasından taşmasın).
                L = min(d / 2 - 0.01, 0.12)
                n = K("KutuKapak", (w - 0.01, 0.003, L), (cx, 0.02 + yon * d / 2, z + hh + L / 2), karton)
                aci = math.radians(rnd.uniform(35, 70)) if yon < 0 else math.radians(-rnd.uniform(5, 15))
                donustur(n, don("X", aci, (cx, 0.02 + yon * d / 2, z + hh)))
            K("Etiket", (0.10, 0.002, 0.06), (cx, 0.02 - d / 2 - 0.001, z + hh * 0.6), etiket)
            for k in range(6):
                q = (cx + rnd.uniform(-w / 2 + 0.06, w / 2 - 0.06), 0.02 + rnd.uniform(-d / 2 + 0.06, d / 2 - 0.06), z + hh - 0.01 + rnd.uniform(0.0, 0.03))
                kure(rnd.uniform(0.04, 0.07), q, KUMAS[rnd.randrange(8)], olcek=(1.2, 1.0, 0.5), seg=10)
            n = K("Kirpinti", (0.02, 0.10, 0.004), (cx + 0.05, 0.02, z + hh + 0.04), KUMAS[rnd.randrange(8)])
            donustur(n, don("Y", math.radians(rnd.uniform(-60, 60)), (cx + 0.05, 0.02, z + hh)))
            x += w + 0.02
    # En üst rafta ince bir kalıp kâğıdı destesi.
    K("KalipDeste", (0.50, 0.32, 0.03), (-0.2, 0.0, 1.775), etiket, 0.004)
    ad, h, uzak = "Raf_K6", 1.8, 2.8

elif hangi == "sepet":
    hasir = ortak.malzeme("Sepet6_Hasir", (0.34, 0.24, 0.11), puruz=0.9)
    hasir2 = ortak.malzeme("Sepet6_Hasir2", (0.26, 0.17, 0.07), puruz=0.9)
    gomlek = ortak.malzeme("Sepet6_Gomlek", (0.16, 0.22, 0.32), puruz=0.95)
    OL = (1.0, 0.78)
    # Oval, yukarı doğru hafif genişleyen örgü gövde: iki tonlu yatay şeritler ve dikey çubuklar.
    for i in range(12):
        z0 = i * 0.043
        r0, r1 = 0.20 + 0.025 * z0 / 0.52, 0.20 + 0.025 * (z0 + 0.043) / 0.52
        torna("Orgu", [(r0, z0), (r1 + 0.004, z0 + 0.02), (r1, z0 + 0.043)], hasir if i % 2 == 0 else hasir2, kose=32, olcek=OL,
              alt=(i == 0), ust=(i == 11))
    for k in range(24):
        a = 2 * math.pi * k / 24
        cubuk((0.203 * math.cos(a), 0.203 * 0.78 * math.sin(a), 0.0), (0.228 * math.cos(a), 0.228 * 0.78 * math.sin(a), 0.515), 0.004, hasir2, 5)
    halka(0.228, 0.01, (0, 0, 0.515), (0, 0, 1), hasir2, seg=32, olcek=(1, 0.78, 1))
    for sx in (-1, 1):
        halka(0.035, 0.008, (sx * 0.235, 0, 0.44), (1, 0, 0), hasir2, seg=14)
    # İçi dolu (çamaşır yığını) ve hafif aralık kalmış kapak; önden bir gömlek kolu sarkıyor.
    kure(0.20, (0, 0, 0.47), gomlek, olcek=(1.0, 0.75, 0.3), seg=16)
    bas = len(p)
    torna("Kapak", [(0.235, 0.0), (0.238, 0.02), (0.20, 0.035), (0.05, 0.04)], hasir, kose=32, konum=(0, 0, 0.52), olcek=OL)
    kure(0.02, (0, 0, 0.565), hasir2, olcek=(1.5, 1, 0.8), seg=10)
    dondur_parcalar(bas, don("X", math.radians(-9), (0, 0.18, 0.525)))
    egri([(0.04, -0.12, 0.53), (0.05, -0.19, 0.515), (0.06, -0.205, 0.45), (0.065, -0.20, 0.36), (0.06, -0.195, 0.31)], 0.024, gomlek, 10)
    cubuk((0.06, -0.195, 0.31), (0.058, -0.193, 0.28), 0.026, gomlek, 10)          # kol ağzı (manşet)
    ad, h, uzak = "Camasir_Sepeti_K6", 0.6, 1.3

elif hangi == "havlu":
    havlu = [ortak.malzeme("Havlu6_Sari", (0.42, 0.32, 0.09), puruz=1.0), ortak.malzeme("Havlu6_Yesil", (0.05, 0.18, 0.17), puruz=1.0)]
    serit_m = ortak.malzeme("Havlu6_Serit", (0.62, 0.60, 0.54), puruz=1.0)
    # Duvara vidalı ahşap tahta (arka yüz y = 0) ve iki krom kanca; tahta üstte, havlular aşağı sarkar.
    K("Tahta", (0.46, 0.018, 0.07), (0, -0.009, 0.20), koyu, 0.004)
    for x in (-0.18, 0.18):
        disk(0.006, 0.003, (x, -0.0195, 0.20), (0, -1, 0), krom, 10)
    for i, x in enumerate((-0.12, 0.12)):
        egri([(x, -0.018, 0.19), (x, -0.055, 0.175), (x, -0.075, 0.185), (x, -0.078, 0.205)], 0.005, krom, 8)
        kure(0.007, (x, -0.078, 0.208), krom, seg=8)
        # Havlu: kancanın üstünden katlanır; önde uzun, arkada kısa kat; önde iki çizgili şerit.
        cubuk((x - 0.11, -0.062, 0.187), (x + 0.11, -0.062, 0.187), 0.017, havlu[i], 12)
        K("Havlu", (0.22, 0.012, 0.42 - i * 0.04), (x, -0.078, 0.187 - (0.42 - i * 0.04) / 2), havlu[i], 0.004)
        K("Havlu", (0.22, 0.012, 0.25), (x, -0.047, 0.187 - 0.125), havlu[i], 0.004)
        zb = 0.187 - (0.42 - i * 0.04)
        for dz in (0.05, 0.075):
            K("Serit", (0.222, 0.013, 0.012), (x, -0.078, zb + dz), serit_m)
    ad, h, uzak = "Havlu_K6", 0.5, 1.0

elif hangi == "ayna":
    ayna = ortak.malzeme("Ayna6_Cam", (0.62, 0.64, 0.64), puruz=0.04, metal=1.0)
    cerceve = ortak.malzeme("Ayna6_Cerceve", (0.10, 0.055, 0.022), puruz=0.45)
    arka = ortak.malzeme("Ayna6_Arka", (0.20, 0.15, 0.10), puruz=0.9)
    YP, Z0, Z1 = 0.10, 0.08, 1.78              # orta kanadın ortası (y) ve yükseklik aralığı
    def kanat(xa, xb, z0, z1, taccli=False):
        """Bir ayna kanadı: çerçeve, önde ayna, arkada sunta; parçaları listeye ekler."""
        cx, w = (xa + xb) / 2, xb - xa
        K("Cerceve", (w, 0.04, z1 - z0), (cx, YP, (z0 + z1) / 2), cerceve, 0.008)
        K("Ayna", (w - 0.08, 0.006, z1 - z0 - 0.08), (cx, YP - 0.022, (z0 + z1) / 2), ayna)
        K("Arka", (w - 0.02, 0.004, z1 - z0 - 0.02), (cx, YP + 0.021, (z0 + z1) / 2), arka)
        if taccli:
            nk = [(xa + 0.02, z1)] + [(cx + (w / 2 - 0.02) * math.cos(math.pi * t / 10), z1 + 0.08 * math.sin(math.pi * t / 10)) for t in range(11)][::-1] + [(xb - 0.02, z1)]
            prizma("Tac", nk[1:-1], YP - 0.02, YP + 0.02, cerceve, "xz")
    # Orta kanat (tepesi kemerli taç) ve iki ayağı.
    kanat(-0.28, 0.28, Z0, Z1 - 0.08, True)
    for x in (-0.20, 0.20):
        K("Ayak", (0.05, 0.40, 0.04), (x, -0.05, 0.02), cerceve, 0.006)
        K("Takoz", (0.05, 0.06, 0.06), (x, YP, 0.06), cerceve, 0.004)
    # Yan kanatlar: menteşe çizgisi orta kanadın ön kenarında; 30° öne doğru açık.
    for sx in (-1, 1):
        bas = len(p)
        xa, xb = (0.28, 0.58) if sx > 0 else (-0.58, -0.28)
        kanat(xa, xb, Z0 + 0.04, Z1 - 0.16)
        K("KanatAyak", (0.26, 0.05, 0.04), ((xa + xb) / 2, YP, Z0 + 0.02), cerceve, 0.005)
        dondur_parcalar(bas, don("Z", -sx * math.radians(30), (sx * 0.28, YP - 0.02, 0)))
        for z in (0.35, 1.0, 1.45):
            S("Mentese", 0.006, 0.06, (sx * 0.28, YP - 0.02, z), pirinc, kose=10)
    ad, h, uzak = "Boy_Aynasi_K6", 1.8, 2.8

else:   # perdeterzi: düz gri perde ve krom çubuğu
    gri = ortak.malzeme("PerdeT_Gri", (0.16, 0.16, 0.17), puruz=0.9)
    EN, BOY = 1.80, 1.75                       # çubuk boyu; çubuktan perde eteğine
    R = 0.012
    # Çubuk: merkezi z = -R (üstü pivot yüksekliğinde), duvardan 8 cm önde; iki taşıyıcı, uçlarda top.
    cubuk((-EN / 2, -0.08, -R), (EN / 2, -0.08, -R), R, krom, 14)
    for sx in (-1, 1):
        kure(0.022, (sx * (EN / 2 + 0.018), -0.08, -R), krom, seg=12)
        cubuk((sx * (EN / 2 - 0.08), 0.0, -R), (sx * (EN / 2 - 0.08), -0.08, -R), 0.008, krom, 8)
        disk(0.022, 0.006, (sx * (EN / 2 - 0.08), -0.003, -R), (0, -1, 0), krom, 16)

    def dalgali(x0, x1, dalga, genlik, mat):
        """Dalgalı düz perde kanadı: üstte halkalardan asılı, sinüs kıvrımlı yüzey, 4 mm et."""
        n = int(dalga * 8)
        v, f = [], []
        for i in range(n + 1):
            t = i / n
            x = x0 + (x1 - x0) * t
            y = -0.08 + genlik * math.sin(t * dalga * 2 * math.pi)
            v += [(x, y, -0.035), (x, y - 0.006 * math.sin(t * 7), -BOY)]
        for i in range(n):
            f.append((2 * i, 2 * i + 2, 2 * i + 3, 2 * i + 1))
        m = yeni_mesh("Kanat", v, f, mat)
        mod = m.modifiers.new("Kal", "SOLIDIFY")
        mod.thickness = 0.004
        for i in range(int(dalga * 2) + 1):
            x = x0 + (x1 - x0) * i / (dalga * 2)
            halka(0.02, 0.003, (x, -0.08, -R), (1, 0, 0), krom, seg=12)
            cubuk((x, -0.08, -R - 0.02), (x, -0.08, -0.04), 0.002, krom, 5)

    dalgali(-0.86, -0.20, 5.5, 0.03, gri)       # sol kanat yarıya kadar çekili
    dalgali(0.62, 0.86, 3.0, 0.035, gri)        # sağ kanat kenarda toplanmış
    ad, h, uzak = "Perde_Terzi_K6", 1.8, 2.6

# Önizlemede kaldırma: perde aşağı sarkar, duvar eşyasının yarısı zemin altındadır.
kaldir = {"perdeterzi": 1.8, "havlu": 0.28}.get(hangi, 0.0)
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
