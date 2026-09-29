# kat7_esyalar.py
# 7. kat: kapıcı Cemal'in kendi dairesi (#83). 1994, Sivaslı, 33 yaşında, bekâr. Elektrik yok.
# Cemal binayı araştırıyormuş; gitmek için eşyasını toplamış ama gidememiş, kaybolmuş.
# Sade, yoksul, dağınık bir bekâr evi. Katlarda aynı model yok: hepsi yeni ve _K7 ekli.
#   blender -b --factory-startup --python kat7_esyalar.py -- <ad> [--onizleme yol.png]
#
# Bu dosya 1-14 numaralı eşyaları yapar; 15-25 kat7_esyalar_2.py'de (yardımcıları buradan alır).
# ANTRE
#   botlar    Is_Botlari_K7     : bir çift kahverengi iş botu ve kapıcının kırmızı tabanlı lastik çizmesi (biri devrik).
#   tulum     Tulum_Askisi_K7   : çam askılık; kancalarda lacivert iş tulumu, kasket ve çok anahtarlı halka.
#   supurge   Supurge_K7        : duvara dayalı geniş kıllı itme süpürgesi ve teneke faraş.
# HOL
#   konsol    Hol_Konsolu_K7    : AÇILIR (Govde + Cekmece_1) tek çekmeceli hol konsolu; üstünde boş kül tablası.
# SALON
#   kilim     Sivas_Kilimi_K7   : kırmızı zeminli, lacivert kenarlı, basamaklı baklava göbekli Sivas kilimi.
#   kanepe    Kanepe_Yatak_K7   : kahverengi çekyat; üstünde asker battaniyesi ve yastık (Cemal salonda yatıyor).
#   bufe      Bufe_Cemal_K7     : AÇILIR (Govde + Kapi_1 + Kapi_2) iki kapaklı alt dolap, üstü sürgü camlı vitrin.
#   sehpa     Kucuk_Sehpa_K7    : alçak, eğik ayaklı sehpa; izmarit dolu kül tablası, yarım çay, transistörlü radyo.
#   tv        TV_Taburesi_K7    : ahşap tabure üstünde siyah 14" portatif televizyon.
#   perde     Perde_K7          : beyaz plastik çubukta ucuz, ince, yarı çekili perde (bir halkası kopmuş, sarkık).
# MUTFAK
#   ocak      Tup_Ocak_K7       : mavi piknik tüpü üstünde tek gözlü ocak ve çelik ikili çaydanlık.
#   masa      Mutfak_Masasi_K7  : çapraz ayaklı katlanır masa; tek tabakta zeytin, yarım somun ekmek.
#   sandalye  Sandalye_K7       : çıtalı arkalıklı basit çam sandalye.
#   bulasik   Bulasik_Yigini_K7 : tezgâh üstünde kirli tabak yığını, kapağı kaymış tencere, çay bardakları.
# Ön yüz -y (Unity'de +Z). Pivot: yer eşyası tabanın ortası; duvar eşyası arka yüzün (y=0) ortası;
# duvara dayalı yer eşyası duvarın dibi (arka y=0, taban z=0); perde çubuk yüksekliğinde arka üst kenar.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import bmesh
from mathutils import Matrix, Vector

p = []                    # şu anki eşyanın parçaları (birleştirilecek)
rnd = random.Random(7)    # sabit tohum: betik her çalıştığında aynı "dağınıklık"


# ---------- YARDIMCILAR (kat7_esyalar_2.py de kullanır) ----------

def al(n):
    """Parçayı listeye ekle ve geri ver (tek satırda hem yarat hem ekle)."""
    p.append(n)
    return n


def kutu(ad, olcu, konum, mat, pah=0.0):
    return al(ortak.kutu(ad, olcu, konum, mat, pah))


def sil(ad, r, boy, konum, mat, kose=24, pah=0.0):
    return al(ortak.silindir(ad, r, boy, konum, mat, kose, pah))


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına uzanan silindir (boru, sap, ip)."""
    d = Vector(b) - Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((Vector(a) + Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")   # silindirin z eksenini a->b yönüne çevir
    bpy.ops.object.transform_apply(rotation=True)       # dönüşü meshe işle (birleştirince bozulmasın)
    ortak.ata(n, mat)
    return al(n)


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(6, seg // 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    return al(n)


def dik_disk(r, kal, konum, mat, kose=24):
    """Yüzü -y'ye bakan disk (kadran, düğme, ayna)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=kal, location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return al(n)


def simit(R, r, konum, mat, rot=(0, 0, 0), seg=(24, 8)):
    """Halka (torus): anahtarlık halkası, kulp, çember. rot radyan."""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg[0], minor_segments=seg[1], location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return al(n)


def torna(ad, profil, konum, mat, kose=24, olcek=(1, 1, 1)):
    """
    TORNA: (yarıçap, yükseklik) noktalarından oluşan bir kesiti z ekseni etrafında döndürüp gövde yapar.
    Çömlekçinin çarkı gibi: bardak, kova, tencere, şişe hep böyle çıkar. Kesit eksende (r=0) başlayıp
    eksende biterse kapalı bir cisim olur; iç duvarı da kesite yazarsak içi oyuk (bardak) olur.
    """
    verts, halkalar = [], []
    for (r, z) in profil:
        if r < 1e-6:                                   # eksen üstündeki nokta: tek köşe (kutup)
            halkalar.append([len(verts)])
            verts.append((0.0, 0.0, z))
        else:                                          # halka: kose kadar köşe
            h = []
            for j in range(kose):
                a = 2 * math.pi * j / kose
                h.append(len(verts))
                verts.append((r * math.cos(a), r * math.sin(a), z))
            halkalar.append(h)
    yuzler = []
    for A, B in zip(halkalar, halkalar[1:]):
        if len(A) == 1 and len(B) == 1:
            continue
        for j in range(kose):
            k = (j + 1) % kose
            if len(A) == 1:
                yuzler.append((A[0], B[j], B[k]))
            elif len(B) == 1:
                yuzler.append((A[j], A[k], B[0]))
            else:
                yuzler.append((A[j], A[k], B[k], B[j]))
    mesh = bpy.data.meshes.new(ad)
    mesh.from_pydata(verts, [], yuzler)
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)   # yüz yönlerini dışarı çevir (içeriden görünmesin)
    bm.to_mesh(mesh)
    bm.free()
    n = bpy.data.objects.new(ad, mesh)
    bpy.context.collection.objects.link(n)
    n.location = konum
    n.scale = olcek
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    return al(n)


def yuzey(ad, f, nu, nv, mat, kal=0.003):
    """
    Kumaş gibi ince, kıvrımlı yüzey: f(u, v) -> (x, y, z), u ve v 0..1. SOLIDIFY değiştiricisi ona
    kal kadar kalınlık verir; böylece iki yüzü de görünür (Unity arka yüzleri çizmez).
    """
    verts = [f(i / nu, j / nv) for j in range(nv + 1) for i in range(nu + 1)]
    yuzler = [(j * (nu + 1) + i, (j + 1) * (nu + 1) + i, (j + 1) * (nu + 1) + i + 1, j * (nu + 1) + i + 1)
              for j in range(nv) for i in range(nu)]
    mesh = bpy.data.meshes.new(ad)
    mesh.from_pydata(verts, [], yuzler)
    n = bpy.data.objects.new(ad, mesh)
    bpy.context.collection.objects.link(n)
    mod = n.modifiers.new("Kalinlik", "SOLIDIFY")
    mod.thickness = kal
    mod.offset = 0.0
    ortak.ata(n, mat)
    return al(n)


def dondur(nesneler, aci, eksen="Z", merkez=(0, 0, 0)):
    """Parçaları bir nokta etrafında döndür (derece) ve dönüşü meshe işle."""
    m = Vector(merkez)
    M = Matrix.Translation(m) @ Matrix.Rotation(math.radians(aci), 4, eksen) @ Matrix.Translation(-m)
    for n in nesneler:
        n.matrix_world = M @ n.matrix_world
        bpy.ops.object.select_all(action="DESELECT")
        n.select_set(True)
        bpy.context.view_layer.objects.active = n
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)


def seffaf(m, alfa):
    """Cam için saydamlık (Alpha). FBX bunu malzemenin opaklığı olarak taşır; Unity'de şeffaf malzeme gerekir."""
    m.node_tree.nodes.get("Principled BSDF").inputs["Alpha"].default_value = alfa
    return m


def tasi(nesneler, v):
    for n in nesneler:
        n.location += Vector(v)


def cay_bardagi(x, y, z, cam, cay, dolu=0.7, olcek=1.0):
    """İnce belli çay bardağı: altta çay (koyu kızıl), üstte boş cam kısım. dolu: çayın bardaktaki oranı."""
    s = olcek
    H = 0.095 * s
    zc = H * dolu
    def r(t):   # bardağın t yüksekliğindeki dış yarıçapı: alt geniş, bel ince, ağız geniş
        u = t / H
        return s * (0.021 - 0.006 * math.sin(min(1, u / 0.75) * math.pi) + 0.004 * max(0, u - 0.6) / 0.4)
    prof = [(0, 0), (r(0), 0)] + [(r(t), t) for t in (H * k / 8 for k in range(1, 9)) if t <= zc] + [(r(zc) - 0.001, zc), (0, zc)]
    torna("Cay", [(a, b) for a, b in prof], (x, y, z), cay, kose=16)
    ust = [(r(zc), zc), (r(zc + (H - zc) / 2), zc + (H - zc) / 2), (r(H), H), (r(H) - 0.0015, H), (r(zc) - 0.0015, zc + 0.001)]
    torna("Cam", ust, (x, y, z), cam, kose=16)


def ortala(n, kip):
    """Pivotu kurala göre yerleştirir: mesh taşınır, nesnenin konumu (0,0,0) kalır."""
    ortak.pivotu_tasi(n, (0, 0, 0))
    vs = [v.co for v in n.data.vertices]
    x0, x1 = min(v.x for v in vs), max(v.x for v in vs)
    y0, y1 = min(v.y for v in vs), max(v.y for v in vs)
    z0, z1 = min(v.z for v in vs), max(v.z for v in vs)
    if kip == "yer":        # tabanın ortası
        d = (-(x0 + x1) / 2, -(y0 + y1) / 2, -z0)
    elif kip == "duvar":    # arka yüzün (y=0) ortası
        d = (-(x0 + x1) / 2, -y1, -(z0 + z1) / 2)
    else:                   # "dip": duvara dayalı yer eşyası, duvarın dibi
        d = (-(x0 + x1) / 2, -y1, -z0)
    n.data.transform(Matrix.Translation(d))
    n.data.update()


def bitir(ad, h, uzak, kip="yer", parcalar=None, kaldir=None):
    """Birleştir, pivotu ayarla, kaydet/aktar, önizle. parcalar verilirse açılır eşya (Govde + parçalar)."""
    if parcalar is not None:
        govde = ortak.birlestir(p, "Govde")
        ortak.yumusat(govde, 35)
        ortak.pivotu_tasi(govde, (0, 0, 0))
        ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, uzak)
        return
    n = ortak.birlestir(p, ad)
    ortak.yumusat(n, 35)
    if kip:
        ortala(n, kip)
    else:
        ortak.pivotu_tasi(n, (0, 0, 0))
    if kaldir is None:
        kaldir = n.dimensions.z / 2 + 0.15 if kip == "duvar" else 0.0
    ortak.calistir_ve_bitir(ad, [n], h, uzak, onizlemede_kaldir=kaldir)


# ---------- EŞYALAR ----------

def bot_is(deri, taban, bag):
    """Bir iş botu, burnu -y'de, dünya merkezinde. Parçaları döndürür (liste)."""
    n0 = len(p)
    kutu("Taban", (0.105, 0.29, 0.03), (0, 0, 0.015), taban, 0.01)
    kutu("Ayak", (0.095, 0.25, 0.075), (0, 0.01, 0.065), deri, 0.03)
    kure(0.05, (0, -0.095, 0.058), deri, (0.95, 1.0, 0.72))
    kutu("Konc", (0.097, 0.12, 0.11), (0, 0.075, 0.15), deri, 0.025)
    kutu("Yaka", (0.10, 0.125, 0.022), (0, 0.075, 0.204), deri, 0.01)
    for i in range(4):                               # bağcıklar: konçun önünde, basamak basamak
        kutu("Bagcik", (0.07, 0.008, 0.006), (0, 0.012, 0.115 + i * 0.024), bag)
    kutu("Topuk", (0.10, 0.07, 0.012), (0, 0.11, 0.036), taban)
    return p[n0:]


def cizme(lastik, taban):
    """Lastik çizme: kırmızı taban şeridi, siyah gövde, oval ve içi boş konç."""
    n0 = len(p)
    kutu("Taban", (0.11, 0.30, 0.025), (0, 0, 0.0125), taban, 0.008)
    kutu("Ayak", (0.10, 0.27, 0.08), (0, 0.005, 0.064), lastik, 0.035)
    kure(0.052, (0, -0.10, 0.054), lastik, (0.95, 1.0, 0.75))
    torna("Konc", [(0, 0.06), (0.056, 0.06), (0.064, 0.38), (0.057, 0.38), (0.049, 0.30), (0, 0.30)], (0, 0.065, 0), lastik, kose=20, olcek=(0.9, 1.1, 1.0))
    return p[n0:]


def uret(hangi):
    if hangi == "botlar":
        deri = ortak.malzeme("K7_Bot_Deri", (0.07, 0.04, 0.02), puruz=0.6)
        taban = ortak.malzeme("K7_Bot_Taban", (0.02, 0.02, 0.02), puruz=0.8)
        bag = ortak.malzeme("K7_Bot_Bagcik", (0.22, 0.17, 0.10), puruz=0.9)
        lastik = ortak.malzeme("K7_Cizme_Lastik", (0.012, 0.014, 0.012), puruz=0.35)
        kirmizi = ortak.malzeme("K7_Cizme_Taban", (0.30, 0.04, 0.03), puruz=0.5)
        camur = ortak.malzeme("K7_Camur", (0.08, 0.06, 0.035), puruz=1.0)
        # İş botları: yan yana, biraz aralık ve açıyla (çıkarılıp bırakılmış).
        for x, a in ((-0.30, 6), (-0.17, -4)):
            parca = bot_is(deri, taban, bag)
            dondur(parca, a)
            tasi(parca, (x, 0, 0))
        # Lastik çizme: biri dik, öbürü yan yatmış; tabanlarında kuru çamur.
        parca = cizme(lastik, kirmizi)
        kutu("Camur", (0.09, 0.12, 0.004), (0, -0.04, 0.002), camur)
        dondur(parca + [p[-1]], -8)
        tasi(parca + [p[-1]], (0.02, 0.0, 0))
        parca = cizme(lastik, kirmizi)
        dondur(parca, 90, "Y")                          # yan yatır: konç +x'e bakar, taban -x'e
        dondur(parca, -110)                             # tabanı duvara (+y), konçu öne-sola baksın
        tasi(parca, (0.31, 0.12, 0.056))
        bitir("Is_Botlari_K7", 0.3, 0.8)

    elif hangi == "tulum":
        cam = ortak.malzeme("K7_Askilik_Cam", (0.30, 0.18, 0.07), puruz=0.55)
        demir = ortak.malzeme("K7_Kanca", (0.03, 0.03, 0.03), puruz=0.4, metal=0.8)
        tulum = ortak.malzeme("K7_Tulum", (0.018, 0.025, 0.07), puruz=0.9)
        tulum_koyu = ortak.malzeme("K7_Tulum_Koyu", (0.012, 0.016, 0.045), puruz=0.9)
        fermuar = ortak.malzeme("K7_Fermuar", (0.35, 0.33, 0.28), puruz=0.4, metal=0.8)
        kasket = ortak.malzeme("K7_Kasket", (0.05, 0.045, 0.035), puruz=0.95)
        celik = ortak.malzeme("K7_Anahtar_Celik", (0.50, 0.50, 0.48), puruz=0.3, metal=1.0)
        pirinc = ortak.malzeme("K7_Anahtar_Pirinc", (0.50, 0.36, 0.12), puruz=0.3, metal=1.0)
        # Tahta: arka yüzü y=0'da, 70 cm; iki vida.
        kutu("Tahta", (0.70, 0.022, 0.09), (0, -0.011, 0), cam, 0.005)
        for x in (-0.31, 0.31):
            dik_disk(0.006, 0.003, (x, -0.0235, 0), demir, 10)
        # Dört kanca: dışarı ve yukarı kıvrık.
        for x in (-0.27, -0.12, 0.12, 0.27):
            kutu("KancaTaban", (0.02, 0.004, 0.04), (x, -0.024, -0.005), demir, 0.002)
            cubuk((x, -0.024, -0.015), (x, -0.07, -0.022), 0.004, demir)
            cubuk((x, -0.07, -0.022), (x, -0.082, 0.008), 0.004, demir)
        # Tulum: yakasından kancaya asılı; omuzlar düşmüş, kollar yanlarda, paçalar duvara yaslı.
        tx = -0.12
        kutu("Yaka", (0.16, 0.05, 0.05), (tx, -0.075, -0.035), tulum_koyu, 0.015)
        for yon in (-1, 1):
            o = kutu("Omuz", (0.22, 0.10, 0.06), (tx + yon * 0.11, -0.085, -0.075), tulum, 0.02)
            dondur([o], yon * 22, "Y", (tx + yon * 0.11, -0.085, -0.075))
        kutu("Govde", (0.40, 0.10, 0.56), (tx, -0.085, -0.37), tulum, 0.03)
        kutu("Kemer", (0.41, 0.105, 0.04), (tx, -0.085, -0.63), tulum_koyu, 0.01)
        for yon in (-1, 1):
            k = kutu("Kol", (0.10, 0.09, 0.55), (tx + yon * 0.235, -0.08, -0.38), tulum, 0.03)
            dondur([k], yon * 4, "Y", (tx + yon * 0.235, -0.08, -0.12))
            kutu("Bacak", (0.19, 0.095, 0.75), (tx + yon * 0.10, -0.075, -1.02), tulum, 0.03)
            kutu("Paca", (0.195, 0.10, 0.05), (tx + yon * 0.10, -0.075, -1.37), tulum_koyu, 0.01)
            kutu("Diz", (0.12, 0.004, 0.10), (tx + yon * 0.10, -0.1245, -0.98), tulum_koyu)
        kutu("Fermuar", (0.012, 0.004, 0.56), (tx, -0.1365, -0.36), fermuar)
        kutu("Cep", (0.10, 0.004, 0.11), (tx - 0.10, -0.1365, -0.22), tulum_koyu)
        kutu("Kalem", (0.008, 0.006, 0.05), (tx - 0.08, -0.139, -0.17), fermuar)
        # Kasket: kancada dik asılı; tepesi öne bakar, siperlik altta.
        kure(0.11, (0.12, -0.072, -0.085), kasket, (1.0, 0.30, 0.72), 16)     # tepe: enli, basık
        kure(0.10, (0.12, -0.088, -0.155), kasket, (0.95, 0.30, 0.28), 16)    # siperlik: altta öne çıkık
        kure(0.008, (0.12, -0.103, -0.07), kasket)                            # tepe düğmesi
        # Anahtarlık: halka ve etrafına yelpaze gibi dizilmiş altı anahtar (kapıcının bütün kapıları).
        hx, hy, hz = 0.27, -0.078, -0.03
        simit(0.028, 0.003, (hx, hy, hz), celik, rot=(math.pi / 2, 0, 0))
        for i, a in enumerate((-40, -24, -8, 8, 24, 40)):
            mat = pirinc if i % 2 == 0 else celik
            yy = hy - 0.004 + (i % 3) * 0.003
            n0 = len(p)
            dik_disk(0.011, 0.003, (hx, yy, hz - 0.036), mat, 14)
            kutu("Sap", (0.006, 0.0025, 0.045), (hx, yy, hz - 0.068), mat)
            for k in range(3):
                kutu("Dis", (0.005 + 0.002 * ((i + k) % 2), 0.0025, 0.005), (hx + 0.005, yy, hz - 0.058 - k * 0.011), mat)
            dondur(p[n0:], a, "Y", (hx, hy, hz))
        bitir("Tulum_Askisi_K7", 2.2, 0.9, kip="duvar")

    elif hangi == "supurge":
        sap = ortak.malzeme("K7_Supurge_Sap", (0.22, 0.13, 0.05), puruz=0.6)
        blok = ortak.malzeme("K7_Supurge_Blok", (0.10, 0.05, 0.02), puruz=0.6)
        kil = ortak.malzeme("K7_Supurge_Kil", (0.035, 0.03, 0.025), puruz=0.95)
        teneke = ortak.malzeme("K7_Faras_Teneke", (0.40, 0.41, 0.40), puruz=0.45, metal=0.85)
        pas = ortak.malzeme("K7_Faras_Pas", (0.20, 0.08, 0.03), puruz=0.9)
        # İtme süpürgesi: kılları yerde, sapı duvara yaslı (~13°).
        kutu("Kil", (0.40, 0.055, 0.075), (0, -0.32, 0.0375), kil, 0.006)
        kutu("Blok", (0.42, 0.07, 0.045), (0, -0.32, 0.0975), blok, 0.008)
        cubuk((0, -0.32, 0.11), (0, -0.018, 1.36), 0.013, sap)
        cubuk((-0.10, -0.32, 0.12), (0, -0.30, 0.22), 0.006, teneke)   # sapı destekleyen tel payanda
        cubuk((0.10, -0.32, 0.12), (0, -0.30, 0.22), 0.006, teneke)
        sil("SapUc", 0.0135, 0.03, (0, -0.02, 1.37), teneke, 12)
        # Teneke faraş: yerde, arkası duvara yakın; kısa dik sap, kenarında pas lekesi.
        fx = 0.36
        kutu("FarasTaban", (0.26, 0.23, 0.003), (fx, -0.15, 0.0015), teneke)
        kutu("FarasArka", (0.26, 0.004, 0.07), (fx, -0.035, 0.035), teneke)
        for yon in (-1, 1):                              # yan duvarlar: arkada yüksek, öne doğru alçalır
            kutu("FarasYan", (0.004, 0.10, 0.05), (fx + yon * 0.13, -0.09, 0.025), teneke)
            kutu("FarasYan", (0.004, 0.12, 0.02), (fx + yon * 0.13, -0.20, 0.01), teneke)
        cubuk((fx, -0.03, 0.06), (fx, -0.012, 0.26), 0.009, sap)
        kutu("Pas", (0.08, 0.06, 0.001), (fx + 0.06, -0.20, 0.0035), pas)
        bitir("Supurge_K7", 0.8, 1.9, kip="dip")

    elif hangi == "konsol":
        ahsap = ortak.malzeme("K7_Konsol_Ahsap", (0.13, 0.07, 0.035), puruz=0.5)
        ic = ortak.malzeme("K7_Konsol_Ic", (0.22, 0.15, 0.08), puruz=0.7)
        pirinc = ortak.malzeme("K7_Konsol_Kulp", (0.45, 0.33, 0.12), puruz=0.35, metal=0.9)
        kehribar = ortak.malzeme("K7_KulTablasi_Cam", (0.22, 0.10, 0.02), puruz=0.08)
        EN, DER = 0.70, 0.30
        p.extend(ac.govde_levhalari(EN, DER, 0.78, ahsap, taban=0.60))
        for x in (-EN / 2 + 0.0175, EN / 2 - 0.0175):
            for y in (-DER / 2 + 0.0175, DER / 2 - 0.0175):
                kutu("Ayak", (0.035, 0.035, 0.60), (x, y, 0.30), ahsap, 0.004)
        kutu("AltRaf", (EN - 0.07, DER - 0.07, 0.015), (0, 0, 0.14), ahsap, 0.003)
        kutu("Tabla", (0.74, 0.33, 0.022), (0, -0.005, 0.791), ahsap, 0.006)
        # Boş kül tablası: kehribar renkli basma cam, üç sigara yuvası.
        torna("KulTablasi", [(0, 0), (0.056, 0), (0.062, 0.028), (0.046, 0.028), (0.040, 0.011), (0, 0.011)], (0.20, 0.0, 0.802), kehribar, kose=28)
        for a in (0, 120, 240):
            kutu("Yuva", (0.02, 0.012, 0.006), (0.20 + 0.054 * math.cos(math.radians(a)), 0.054 * math.sin(math.radians(a)), 0.827), ahsap)
            dondur([p[-1]], a, "Z", (0.20 + 0.054 * math.cos(math.radians(a)), 0.054 * math.sin(math.radians(a)), 0.827))
        cek = ac.cekmece("Cekmece_1", 0, 0.621, EN - 0.04, 0.138, 0.25, -DER / 2, ic, ahsap, pirinc, "cubuk")
        bitir("Hol_Konsolu_K7", 0.9, 1.3, parcalar=[cek])

    elif hangi == "kilim":
        kir = ortak.malzeme("K7_Kilim_Kirmizi", (0.30, 0.035, 0.025), puruz=0.95)
        lac = ortak.malzeme("K7_Kilim_Lacivert", (0.02, 0.03, 0.09), puruz=0.95)
        bej = ortak.malzeme("K7_Kilim_Bej", (0.55, 0.45, 0.30), puruz=0.95)
        tur = ortak.malzeme("K7_Kilim_Turuncu", (0.45, 0.17, 0.04), puruz=0.95)
        yes = ortak.malzeme("K7_Kilim_Yesil", (0.05, 0.12, 0.06), puruz=0.95)
        EN, BOY = 2.2, 1.4
        K = [0.008, 0.0088, 0.0096, 0.0104]              # katman üst yüzleri: üst üste binen desen z-savaşı yapmasın

        def serit(x0, x1, y0, y1, kat, mat):
            kutu("Desen", (x1 - x0, y1 - y0, 0.0012), ((x0 + x1) / 2, (y0 + y1) / 2, K[kat] - 0.0006), mat)

        def baklava(cx, cy, w, h, n, kat, mat):
            """Basamaklı baklava: 2n+1 sıra, ortadaki en geniş (dokuma ızgarası yüzünden basamaklı)."""
            sh = h / (2 * n + 1)
            for k in range(-n, n + 1):
                ww = w * (1 - abs(k) / (n + 1))
                serit(cx - ww / 2, cx + ww / 2, cy + k * sh - sh / 2, cy + k * sh + sh / 2, kat, mat)

        kutu("Zemin", (EN, BOY, 0.008), (0, 0, 0.004), kir)
        B = 0.15                                          # kenar şeridi genişliği
        serit(-EN / 2, EN / 2, -BOY / 2, -BOY / 2 + B, 1, lac)
        serit(-EN / 2, EN / 2, BOY / 2 - B, BOY / 2, 1, lac)
        serit(-EN / 2, -EN / 2 + B, -BOY / 2 + B, BOY / 2 - B, 1, lac)
        serit(EN / 2 - B, EN / 2, -BOY / 2 + B, BOY / 2 - B, 1, lac)
        # Kenar şeridinde küçük bej "koç boynuzu" benekleri, içinde ince bej hat.
        for i in range(15):
            x = -EN / 2 + 0.12 + i * (EN - 0.24) / 14
            for y in (-BOY / 2 + B / 2, BOY / 2 - B / 2):
                baklava(x, y, 0.07, 0.07, 2, 2, bej if i % 2 == 0 else tur)
        for j in range(8):
            y = -BOY / 2 + 0.2 + j * (BOY - 0.4) / 7
            for x in (-EN / 2 + B / 2, EN / 2 - B / 2):
                baklava(x, y, 0.07, 0.07, 2, 2, bej)
        g = B + 0.02
        serit(-EN / 2 + g, EN / 2 - g, -BOY / 2 + g, -BOY / 2 + g + 0.02, 1, bej)
        serit(-EN / 2 + g, EN / 2 - g, BOY / 2 - g - 0.02, BOY / 2 - g, 1, bej)
        serit(-EN / 2 + g, -EN / 2 + g + 0.02, -BOY / 2 + g, BOY / 2 - g, 1, bej)
        serit(EN / 2 - g - 0.02, EN / 2 - g, -BOY / 2 + g, BOY / 2 - g, 1, bej)
        # Üç göbek: lacivert büyük baklava, içinde bej, ortasında kırmızı-turuncu; uçlarında çengeller.
        for cx in (-0.62, 0.0, 0.62):
            baklava(cx, 0, 0.50, 0.74, 7, 1, lac)
            baklava(cx, 0, 0.32, 0.48, 5, 2, bej)
            baklava(cx, 0, 0.14, 0.22, 3, 3, kir if cx == 0 else tur)
            for yon in (-1, 1):
                serit(cx - 0.012, cx + 0.012, yon * 0.37, yon * 0.43, 1, lac)
                serit(cx - 0.05, cx + 0.05, yon * 0.43 - 0.012 * (yon > 0), yon * 0.43 + 0.012 * (yon < 0), 1, lac)
        # Göbekler arasında küçük yeşil ve turuncu yıldızlar (artı biçimi).
        for cx, cy in ((-0.31, -0.32), (-0.31, 0.0), (-0.31, 0.32), (0.31, -0.32), (0.31, 0.0), (0.31, 0.32),
                       (-0.86, -0.46), (-0.86, 0.46), (0.86, -0.46), (0.86, 0.46)):
            if True:
                m = yes if (cx * cy) >= 0 else tur
                serit(cx - 0.04, cx + 0.04, cy - 0.012, cy + 0.012, 1, m)
                serit(cx - 0.012, cx + 0.012, cy - 0.04, cy + 0.04, 1, m)
        # Püsküller: kısa kenarlarda (x uçları) çözgü ipleri.
        for x in (-EN / 2 - 0.035, EN / 2 + 0.035):
            for j in range(26):
                kutu("Puskul", (0.07, 0.012, 0.003), (x, -BOY / 2 + 0.04 + j * (BOY - 0.08) / 25, 0.0015), bej)
        bitir("Sivas_Kilimi_K7", 0.1, 2.6)

    elif hangi == "kanepe":
        kumas = ortak.malzeme("K7_Cekyat_Kumas", (0.09, 0.05, 0.03), puruz=0.95)
        kumas_koyu = ortak.malzeme("K7_Cekyat_Dikis", (0.035, 0.02, 0.012), puruz=0.95)
        ahsap = ortak.malzeme("K7_Cekyat_Ahsap", (0.07, 0.035, 0.015), puruz=0.5)
        battaniye = ortak.malzeme("K7_Battaniye", (0.10, 0.11, 0.075), puruz=0.95)
        serit_m = ortak.malzeme("K7_Battaniye_Serit", (0.03, 0.025, 0.02), puruz=0.95)
        yastik = ortak.malzeme("K7_Yastik", (0.55, 0.52, 0.45), puruz=0.9)
        EN = 1.90
        for x in (-0.86, 0.86):
            for y in (-0.33, 0.33):
                kutu("Ayak", (0.05, 0.05, 0.05), (x, y, 0.025), ahsap, 0.005)
        kutu("Kasa", (EN, 0.80, 0.30), (0, 0.02, 0.20), kumas, 0.02)
        kutu("CekmeCizgisi", (1.62, 0.004, 0.012), (0, -0.382, 0.25), kumas_koyu)   # çekyatın açılan önü
        kutu("Oturak", (1.62, 0.62, 0.12), (0, -0.07, 0.41), kumas, 0.035)
        kutu("OturakDikis", (0.006, 0.60, 0.004), (0, -0.07, 0.471), kumas_koyu)
        s = kutu("Arkalik", (1.62, 0.22, 0.48), (0, 0.30, 0.61), kumas, 0.04)
        dondur([s], -8, "X", (0, 0.30, 0.37))
        for yon in (-1, 1):
            kutu("Kol", (0.14, 0.80, 0.30), (yon * 0.88, 0.02, 0.49), kumas, 0.02)
            kutu("KolKapak", (0.16, 0.82, 0.025), (yon * 0.88, 0.02, 0.6525), ahsap, 0.006)
        # Yastık: sol kolda, basık (üstünde yatılmış).
        y = kutu("Yastik", (0.50, 0.34, 0.12), (-0.58, 0.02, 0.53), yastik, 0.05)
        dondur([y], 18, "Y", (-0.58, 0.02, 0.53))
        # Battaniye: oturağa yayılmış, kabarık ve buruşuk; ön kenardan sarkıyor, ucu sona doğru itilmiş.
        for i in range(5):
            x = -0.30 + i * 0.25
            b = kutu("Battaniye", (0.30, 0.60, 0.035 + rnd.uniform(0, 0.03)), (x, -0.06 + rnd.uniform(-0.03, 0.03), 0.485 + rnd.uniform(0, 0.01)), battaniye, 0.015)
            dondur([b], rnd.uniform(-9, 9), "Z", (x, -0.06, 0.49))
        cubuk((0.62, -0.30, 0.52), (0.72, 0.20, 0.53), 0.045, battaniye)       # sona itilmiş rulo
        f = kutu("Sarkan", (1.0, 0.02, 0.22), (0.20, -0.39, 0.37), battaniye, 0.008)
        dondur([f], 4, "Y", (0.20, -0.39, 0.47))
        kutu("Serit", (1.0, 0.024, 0.02), (0.20, -0.39, 0.31), serit_m)
        dondur([p[-1]], 4, "Y", (0.20, -0.39, 0.47))
        bitir("Kanepe_Yatak_K7", 0.9, 2.8)

    elif hangi == "bufe":
        ahsap = ortak.malzeme("K7_Bufe_Ahsap", (0.07, 0.025, 0.012), puruz=0.35)
        koyu = ortak.malzeme("K7_Bufe_Baza", (0.025, 0.012, 0.006), puruz=0.6)
        ic = ortak.malzeme("K7_Bufe_Ic", (0.16, 0.10, 0.05), puruz=0.7)
        pirinc = ortak.malzeme("K7_Bufe_Kulp", (0.45, 0.33, 0.12), puruz=0.35, metal=0.9)
        cam = seffaf(ortak.malzeme("K7_Bufe_Cam", (0.12, 0.14, 0.14), puruz=0.05), 0.25)
        bcam = seffaf(ortak.malzeme("K7_Bufe_Bardak_Cam", (0.45, 0.50, 0.48), puruz=0.05), 0.5)
        porselen = ortak.malzeme("K7_Bufe_Porselen", (0.70, 0.68, 0.62), puruz=0.3)
        yaldiz = ortak.malzeme("K7_Bufe_Yaldiz", (0.55, 0.40, 0.10), puruz=0.3, metal=0.9)
        L = ac.levha
        EN, DER = 1.20, 0.45
        # Alt dolap: iki bölme (orta dikme), her bölmede bir raf; önü açık, kapılar ayrı nesne.
        p.extend(ac.govde_levhalari(EN, DER, 0.78, ahsap, taban=0.08, raflar=(0.43,), dikmeler=(0.0,)))
        kutu("Baza", (EN - 0.04, DER - 0.04, 0.08), (0, 0.01, 0.04), koyu, 0.004)
        # Üst vitrin: daha sığ (30 cm), arkaya yaslı; iki sürgü cam, cam raf, tepede taç.
        y0, y1, z0, z1 = -0.075, 0.225, 0.78, 1.48
        p.extend([L("VYan", -0.60, -0.582, y0, y1, z0, z1, ahsap), L("VYan", 0.582, 0.60, y0, y1, z0, z1, ahsap),
                  L("VUst", -0.60, 0.60, y0, y1, z1 - 0.018, z1, ahsap), L("VArka", -0.582, 0.582, y1 - 0.008, y1, z0, z1 - 0.018, ic),
                  L("Tac", -0.62, 0.62, y0 - 0.02, y1 + 0.005, z1, z1 + 0.022, ahsap, 0.004),
                  L("CamRaf", -0.582, 0.582, y0 + 0.02, y1 - 0.008, 1.10, 1.106, cam, 0.0),
                  L("RayAlt", -0.582, 0.582, y0, y0 + 0.02, z0, z0 + 0.02, ahsap), L("RayUst", -0.582, 0.582, y0, y0 + 0.02, z1 - 0.04, z1 - 0.018, ahsap),
                  L("CamSol", -0.582, 0.01, y0 + 0.006, y0 + 0.010, z0 + 0.02, z1 - 0.04, cam, 0.0),
                  L("CamSag", -0.01, 0.582, y0 + 0.013, y0 + 0.017, z0 + 0.02, z1 - 0.04, cam, 0.0)])
        for x in (-0.03, 0.03):                                         # camların parmak oyukları
            kutu("Oyuk", (0.03, 0.002, 0.012), (x, y0 + 0.004 + (0.007 if x > 0 else 0), 1.0), koyu)
        kutu("Yaldiz", (0.60, 0.004, 0.008), (0, y0 - 0.021, z1 + 0.011), yaldiz)
        # Vitrinin içi: altta çay bardakları ve iki su bardağı; cam rafta dik duran süs tabağı ve iki fincan.
        for i, x in enumerate((-0.44, -0.37, -0.30, -0.23)):
            cay_bardagi(x, 0.09, z0, bcam, bcam, dolu=0.0 + 0.001)
        for x in (0.18, 0.27):
            torna("SuBardagi", [(0, 0), (0.030, 0), (0.035, 0.11), (0.032, 0.11), (0.027, 0.006), (0, 0.006)], (x, 0.10, z0), bcam, 18)
        d = dik_disk(0.10, 0.012, (0.35, 0.19, 1.106 + 0.10), porselen, 28)
        dondur([d], -10, "X", (0.35, 0.19, 1.106))
        simit(0.085, 0.004, (0.35, 0.200, 1.204), yaldiz, rot=(math.pi / 2 - math.radians(10), 0, 0))
        for x in (-0.30, -0.18):
            torna("Fincan", [(0, 0), (0.022, 0), (0.030, 0.055), (0.028, 0.055), (0.020, 0.005), (0, 0.005)], (x, 0.08, 1.106), porselen, 18)
            torna("Tabak", [(0, 0), (0.05, 0), (0.06, 0.008), (0.057, 0.009), (0, 0.004)], (x, 0.08, 1.106), porselen, 20)
        # Kapılar: alt dolabın iki bölmesini örter; topuz kulplar ortada.
        y_on = -DER / 2 - ac.KAL
        k1 = ac.kapi("Kapi_1", -EN / 2 + 0.005, -0.003, 0.085, 0.758, y_on, ahsap, pirinc, "topuz")
        k2 = ac.kapi("Kapi_2", EN / 2 - 0.005, 0.003, 0.085, 0.758, y_on, ahsap, pirinc, "topuz")
        bitir("Bufe_Cemal_K7", 1.5, 2.3, parcalar=[k1, k2])

    elif hangi == "sehpa":
        ahsap = ortak.malzeme("K7_Sehpa_Ahsap", (0.16, 0.09, 0.04), puruz=0.55)
        cizik = ortak.malzeme("K7_Sehpa_Cizik", (0.30, 0.20, 0.10), puruz=0.7)
        teneke = ortak.malzeme("K7_KulT_Teneke", (0.35, 0.35, 0.34), puruz=0.4, metal=0.85)
        kul = ortak.malzeme("K7_Kul", (0.20, 0.20, 0.19), puruz=1.0)
        izmarit = ortak.malzeme("K7_Izmarit", (0.70, 0.68, 0.62), puruz=0.8)
        filtre = ortak.malzeme("K7_Izmarit_Filtre", (0.45, 0.22, 0.06), puruz=0.8)
        cam = ortak.malzeme("K7_Sehpa_CayCam", (0.55, 0.58, 0.55), puruz=0.05)
        cay = ortak.malzeme("K7_Cay", (0.20, 0.035, 0.008), puruz=0.1)
        tabak = ortak.malzeme("K7_Cay_Tabagi", (0.70, 0.68, 0.62), puruz=0.3)
        kilif = ortak.malzeme("K7_Radyo_Kilif", (0.10, 0.045, 0.02), puruz=0.6)
        izgara = ortak.malzeme("K7_Radyo_Izgara", (0.30, 0.30, 0.29), puruz=0.4, metal=0.8)
        kadran = ortak.malzeme("K7_Radyo_Kadran", (0.60, 0.55, 0.40), puruz=0.3)
        ibre = ortak.malzeme("K7_Radyo_Ibre", (0.40, 0.03, 0.02), puruz=0.5)
        H = 0.45
        kutu("Tabla", (0.60, 0.40, 0.025), (0, 0, H - 0.0125), ahsap, 0.005)
        kutu("Etek", (0.52, 0.32, 0.05), (0, 0, H - 0.05), ahsap, 0.003)
        for x in (-1, 1):                                # eğik, 50'lerden kalma sivri ayaklar
            for y in (-1, 1):
                cubuk((x * 0.23, y * 0.13, H - 0.03), (x * 0.27, y * 0.17, 0.0), 0.014, ahsap)
        for i in range(4):                               # tablada çizikler, bardak izi
            kutu("Cizik", (rnd.uniform(0.05, 0.15), 0.003, 0.0006), (rnd.uniform(-0.2, 0.2), rnd.uniform(-0.15, 0.15), H + 0.0003), cizik)
        simit(0.028, 0.002, (0.02, 0.10, H + 0.0005), cizik, seg=(20, 4))
        # İzmarit dolu teneke kül tablası.
        kx, ky = -0.17, 0.05
        torna("KulTablasi", [(0, 0), (0.055, 0), (0.060, 0.022), (0.056, 0.022), (0.050, 0.004), (0, 0.004)], (kx, ky, H), teneke, 24)
        sil("Kul", 0.049, 0.006, (kx, ky, H + 0.007), kul, 20)
        for i in range(8):
            a = math.radians(i * 45 + rnd.uniform(-15, 15))
            n0 = len(p)
            cubuk((kx + 0.012 * math.cos(a), ky + 0.012 * math.sin(a), H + 0.012), (kx + 0.030 * math.cos(a), ky + 0.030 * math.sin(a), H + 0.016 + 0.004 * (i % 2)), 0.0042, izmarit, 8)
            cubuk((kx + 0.030 * math.cos(a), ky + 0.030 * math.sin(a), H + 0.016 + 0.004 * (i % 2)), (kx + 0.050 * math.cos(a), ky + 0.050 * math.sin(a), H + 0.024 + 0.004 * (i % 2)), 0.0043, filtre, 8)
        # Tabağında yarım çay.
        torna("CayTabagi", [(0, 0), (0.045, 0), (0.055, 0.010), (0.052, 0.011), (0, 0.004)], (0.03, -0.09, H), tabak, 20)
        cay_bardagi(0.03, -0.09, H + 0.004, cam, cay, dolu=0.45)
        kutu("Kasik", (0.07, 0.006, 0.003), (0.085, -0.09, H + 0.0125), teneke)
        # Transistörlü radyo: deri kılıf, gümüş hoparlör ızgarası, kadran penceresi, teleskop anten.
        rx, ry = 0.17, 0.06
        kutu("Radyo", (0.17, 0.05, 0.10), (rx, ry, H + 0.05), kilif, 0.008)
        kutu("Izgara", (0.08, 0.004, 0.07), (rx - 0.035, ry - 0.026, H + 0.05), izgara)
        for k in range(6):
            kutu("Delik", (0.07, 0.002, 0.003), (rx - 0.035, ry - 0.0285, H + 0.025 + k * 0.01), kilif)
        kutu("Kadran", (0.055, 0.004, 0.028), (rx + 0.045, ry - 0.026, H + 0.07), kadran)
        kutu("Ibre", (0.002, 0.005, 0.024), (rx + 0.035, ry - 0.027, H + 0.07), ibre)
        cubuk((rx + 0.087, ry, H + 0.04), (rx + 0.095, ry, H + 0.04), 0.012, izgara, 16)   # ayar düğmesi
        cubuk((rx + 0.07, ry + 0.01, H + 0.10), (rx + 0.00, ry + 0.06, H + 0.36), 0.0025, izgara, 6)
        sil("AntenUc", 0.004, 0.006, (rx + 0.0, ry + 0.06, H + 0.362), izgara, 8)
        bitir("Kucuk_Sehpa_K7", 0.6, 1.3)

    elif hangi == "tv":
        cam = ortak.malzeme("K7_Tabure_Cam", (0.22, 0.13, 0.05), puruz=0.6)
        plastik = ortak.malzeme("K7_TV_Plastik", (0.018, 0.018, 0.018), puruz=0.45)
        ekran = ortak.malzeme("K7_TV_Ekran", (0.02, 0.028, 0.024), puruz=0.08)
        metal = ortak.malzeme("K7_TV_Metal", (0.45, 0.45, 0.44), puruz=0.3, metal=0.9)
        H = 0.46
        # Tabure: kare oturak, dört ayak, alçak çapraz kuşaklar.
        kutu("Oturak", (0.40, 0.40, 0.03), (0, 0, H - 0.015), cam, 0.006)
        for x in (-0.16, 0.16):
            for y in (-0.16, 0.16):
                kutu("Ayak", (0.035, 0.035, H - 0.03), (x, y, (H - 0.03) / 2), cam, 0.004)
        for y in (-0.16, 0.16):
            kutu("Kusak", (0.30, 0.02, 0.03), (0, y, 0.12), cam, 0.003)
        for x in (-0.16, 0.16):
            kutu("Kusak", (0.02, 0.30, 0.03), (x, 0, 0.18), cam, 0.003)
        # 14" portatif TV: ön kasa, arkaya daralan tüp gövdesi, sağda düğmeler, tepede sap ve V anten.
        kutu("OnKasa", (0.36, 0.20, 0.30), (0, -0.06, H + 0.15), plastik, 0.015)
        kutu("Arka", (0.26, 0.14, 0.22), (0, 0.10, H + 0.13), plastik, 0.03)
        kutu("Ekran", (0.25, 0.012, 0.20), (-0.035, -0.162, H + 0.16), ekran, 0.02)
        kure(0.14, (-0.035, -0.166, H + 0.16), ekran, (0.92, 0.05, 0.73), 20)          # tüpün hafif şişkinliği
        kutu("Panel", (0.06, 0.004, 0.24), (0.14, -0.161, H + 0.15), metal)
        for k in range(3):
            cubuk((0.14, -0.163, H + 0.23 - k * 0.055), (0.14, -0.178, H + 0.23 - k * 0.055), 0.013, plastik, 14)
        for k in range(4):
            kutu("Hoparlor", (0.04, 0.003, 0.004), (0.14, -0.1635, H + 0.07 - k * 0.01), plastik)
        cubuk((-0.12, -0.06, H + 0.30), (-0.12, -0.06, H + 0.34), 0.008, plastik)
        cubuk((0.12, -0.06, H + 0.30), (0.12, -0.06, H + 0.34), 0.008, plastik)
        cubuk((-0.12, -0.06, H + 0.34), (0.12, -0.06, H + 0.34), 0.011, plastik, 10)
        for yon in (-1, 1):
            cubuk((0.06 * yon, 0.12, H + 0.24), (0.20 * yon, 0.16, H + 0.62), 0.0025, metal, 6)
        sil("AntenTaban", 0.02, 0.012, (0, 0.12, H + 0.246), plastik, 14)
        bitir("TV_Taburesi_K7", 0.9, 1.5)

    elif hangi == "perde":
        kumas = ortak.malzeme("K7_Perde_Kumas", (0.42, 0.36, 0.20), puruz=0.9)
        cubuk_m = ortak.malzeme("K7_Perde_Cubuk", (0.70, 0.70, 0.66), puruz=0.4)
        EN = 1.60
        cubuk((-EN / 2, -0.05, 0), (EN / 2, -0.05, 0), 0.007, cubuk_m, 10)
        for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
            kutu("Tutucu", (0.012, 0.05, 0.02), (x, -0.025, 0), cubuk_m)
        L = 1.50

        def kanat(x0, x1, dalga, genlik, sarkma=None):
            """Kanat: x0..x1 arasında dalgalı ince kumaş; sarkma=(x, derinlik) orada bir halka kopmuş."""
            def f(u, v):
                x = x0 + (x1 - x0) * u
                y = -0.05 + genlik * math.sin(2 * math.pi * (x - x0) / dalga) * (0.7 + 0.3 * v)
                ust = -0.012
                if sarkma:
                    ust -= sarkma[1] * math.exp(-((x - sarkma[0]) / 0.09) ** 2)
                z = ust - (L + ust + 0.012) * v - 0.01 * math.sin(7 * x) * v
                return (x, y, z)
            return yuzey("Kanat", f, max(8, int((x1 - x0) / dalga * 8)), 6, kumas, 0.002)

        kanat(-0.78, 0.08, 0.13, 0.014, sarkma=(-0.36, 0.06))    # çekilmiş kanat, bir halkası kopuk
        kanat(0.56, 0.78, 0.045, 0.03)                           # toplanmış kanat
        for x in [(-0.76 + i * 0.09) for i in range(10) if abs(-0.76 + i * 0.09 + 0.36) > 0.05] + [0.58, 0.64, 0.70, 0.76]:
            simit(0.012, 0.0022, (x, -0.05, -0.002), cubuk_m, rot=(0, math.pi / 2, 0), seg=(12, 4))
        bitir("Perde_K7", 0.3, 2.0, kip=None, kaldir=1.6)

    elif hangi == "ocak":
        mavi = ortak.malzeme("K7_Pikniktup_Mavi", (0.02, 0.06, 0.25), puruz=0.45, metal=0.3)
        siyah = ortak.malzeme("K7_Ocak_Siyah", (0.025, 0.025, 0.025), puruz=0.5, metal=0.6)
        pirinc = ortak.malzeme("K7_Ocak_Pirinc", (0.45, 0.33, 0.12), puruz=0.35, metal=0.9)
        celik = ortak.malzeme("K7_Caydanlik_Celik", (0.45, 0.45, 0.44), puruz=0.3, metal=0.9)
        is_ = ortak.malzeme("K7_Caydanlik_Is", (0.08, 0.075, 0.07), puruz=0.7, metal=0.4)
        bakalit = ortak.malzeme("K7_Caydanlik_Bakalit", (0.02, 0.015, 0.012), puruz=0.4)
        # Piknik tüpü: halka ayak, göbekli gövde, kubbe, pirinç vana.
        torna("Tup", [(0, 0.02), (0.10, 0.02), (0.11, 0.05), (0.11, 0.22), (0.085, 0.265), (0.03, 0.285), (0.022, 0.30), (0, 0.30)], (0, 0, 0), mavi, 28)
        torna("Ayak", [(0.095, 0), (0.10, 0), (0.10, 0.035), (0.095, 0.035)], (0, 0, 0), mavi, 28)
        sil("Vana", 0.018, 0.02, (0, 0, 0.31), pirinc, 14)
        cubuk((0.015, 0, 0.312), (0.05, 0, 0.312), 0.006, pirinc)
        dik_disk(0.014, 0.012, (0.058, 0, 0.312), siyah, 12)
        dondur([p[-1]], 90, "Z", (0.058, 0, 0.312))
        # Tek göz: brülör, üç kollu tencere ayağı.
        sil("Brulor", 0.035, 0.03, (0, 0, 0.335), siyah, 20)
        sil("BrulorKapak", 0.04, 0.008, (0, 0, 0.354), siyah, 20)
        for a in (90, 210, 330):
            c, s_ = math.cos(math.radians(a)), math.sin(math.radians(a))
            cubuk((0.03 * c, 0.03 * s_, 0.34), (0.10 * c, 0.10 * s_, 0.34), 0.004, siyah)
            cubuk((0.10 * c, 0.10 * s_, 0.34), (0.10 * c, 0.10 * s_, 0.36), 0.004, siyah)
        # İkili çaydanlık: altta su kazanı (dibi is), üstte demlik.
        z = 0.36
        torna("Kazan", [(0, z), (0.082, z), (0.09, z + 0.015), (0.09, z + 0.09), (0.07, z + 0.12), (0.055, z + 0.12), (0, z + 0.12)], (0, 0, 0), celik, 28)
        torna("Is", [(0, z - 0.001), (0.0825, z - 0.001), (0.0905, z + 0.016), (0.0905, z + 0.03), (0, z + 0.03)], (0, 0, 0), is_, 28)
        cubuk((0.075, 0, z + 0.05), (0.14, 0, z + 0.115), 0.011, celik, 10)       # kazanın ağzı
        cubuk((-0.088, 0, z + 0.03), (-0.13, 0, z + 0.05), 0.006, bakalit)       # ağzın karşısında C biçimli sap
        cubuk((-0.13, 0, z + 0.05), (-0.125, 0, z + 0.10), 0.006, bakalit)
        cubuk((-0.125, 0, z + 0.10), (-0.08, 0, z + 0.105), 0.006, bakalit)
        zd = z + 0.12
        torna("Demlik", [(0, zd), (0.052, zd), (0.062, zd + 0.03), (0.06, zd + 0.07), (0.045, zd + 0.085), (0, zd + 0.085)], (0, 0, 0), celik, 24)
        kure(0.012, (0, 0, zd + 0.095), bakalit)
        cubuk((0.055, 0, zd + 0.03), (0.095, 0, zd + 0.07), 0.006, celik)
        cubuk((-0.058, 0, zd + 0.02), (-0.085, 0, zd + 0.04), 0.005, bakalit)
        cubuk((-0.085, 0, zd + 0.04), (-0.058, 0, zd + 0.07), 0.005, bakalit)
        bitir("Tup_Ocak_K7", 0.6, 1.0)

    elif hangi == "masa":
        formika = ortak.malzeme("K7_Masa_Formika", (0.55, 0.50, 0.38), puruz=0.35)
        metal = ortak.malzeme("K7_Masa_Metal", (0.45, 0.45, 0.44), puruz=0.3, metal=0.9)
        lastik = ortak.malzeme("K7_Masa_Lastik", (0.02, 0.02, 0.02), puruz=0.7)
        tabak_m = ortak.malzeme("K7_Masa_Tabak", (0.72, 0.70, 0.64), puruz=0.3)
        tabak_ku = ortak.malzeme("K7_Masa_Tabak_Serit", (0.10, 0.20, 0.30), puruz=0.3)
        zeytin = ortak.malzeme("K7_Zeytin", (0.02, 0.012, 0.018), puruz=0.2)
        kabuk = ortak.malzeme("K7_Ekmek_Kabuk", (0.32, 0.15, 0.04), puruz=0.8)
        ici = ortak.malzeme("K7_Ekmek_Ici", (0.62, 0.52, 0.34), puruz=0.95)
        H = 0.75
        kutu("Tabla", (0.80, 0.60, 0.022), (0, 0, H - 0.011), formika, 0.004)
        kutu("Kenar", (0.81, 0.61, 0.018), (0, 0, H - 0.017), metal, 0.003)
        # Çapraz (X) katlanır ayaklar kısa kenarlarda, ortada bir bağlantı borusu.
        for x in (-0.34, 0.34):
            cubuk((x, -0.25, 0.01), (x, 0.25, H - 0.03), 0.011, metal)
            cubuk((x, 0.25, 0.01), (x, -0.25, H - 0.03), 0.011, metal)
            for y in (-0.25, 0.25):
                sil("Lastik", 0.014, 0.012, (x, y, 0.006), lastik, 12)
        cubuk((-0.34, 0, 0.38), (0.34, 0, 0.38), 0.009, metal)
        cubuk((-0.34, -0.25, H - 0.03), (0.34, -0.25, H - 0.03), 0.009, metal)
        cubuk((-0.34, 0.25, H - 0.03), (0.34, 0.25, H - 0.03), 0.009, metal)
        # Tek tabak: kenarı mavi çizgili, yer yer kırık; içinde zeytinler ve iki çekirdek.
        tx, ty = -0.12, -0.06
        torna("Tabak", [(0, 0), (0.07, 0), (0.11, 0.016), (0.108, 0.018), (0.068, 0.005), (0, 0.005)], (tx, ty, H), tabak_m, 28)
        simit(0.095, 0.0025, (tx, ty, H + 0.0125), tabak_ku, seg=(28, 4))
        for i in range(7):
            a = rnd.uniform(0, 2 * math.pi)
            r = rnd.uniform(0.0, 0.045)
            kure(0.011, (tx + r * math.cos(a), ty + r * math.sin(a), H + 0.016), zeytin, (1.3, 0.95, 0.9), 10)
        for i in range(2):
            kure(0.005, (tx + 0.06 + i * 0.012, ty + 0.03, H + 0.012), ici, (1.4, 0.8, 0.7), 8)
        # Yarım somun: kesik yüzü tabağa bakıyor.
        e = kure(0.13, (0.18, 0.08, H), kabuk, (1.0, 0.62, 0.48), 20)
        ortak.oy(e, ortak.kutu("kes", (0.3, 0.3, 0.3), (0.18 - 0.15, 0.08, H), kabuk))
        ortak.oy(e, ortak.kutu("alt", (0.4, 0.3, 0.2), (0.18, 0.08, H - 0.10), kabuk))
        kure(0.12, (0.1795, 0.08, H + 0.001), ici, (0.02, 0.58, 0.42), 20)
        ortak.oy(p[-1], ortak.kutu("alt2", (0.1, 0.3, 0.2), (0.16, 0.08, H - 0.10), ici))
        for i in range(3):
            kure(0.006, (0.05 + i * 0.03, 0.06 + rnd.uniform(-0.03, 0.03), H + 0.001), kabuk, (1.0, 0.7, 0.3), 8)   # kırıntılar
        bitir("Mutfak_Masasi_K7", 0.8, 1.5)

    elif hangi == "sandalye":
        cam = ortak.malzeme("K7_Sandalye_Cam", (0.25, 0.14, 0.06), puruz=0.6)
        asinmis = ortak.malzeme("K7_Sandalye_Asinmis", (0.35, 0.22, 0.10), puruz=0.7)
        S, H = 0.42, 0.46
        for x in (-S / 2 + 0.02, S / 2 - 0.02):
            kutu("OnAyak", (0.035, 0.035, H - 0.025), (x, -S / 2 + 0.02, (H - 0.025) / 2), cam, 0.004)
            a = kutu("ArkaAyak", (0.035, 0.035, 0.90), (x, S / 2 - 0.02, 0.45), cam, 0.004)
            dondur([a], -4, "X", (x, S / 2 - 0.02, H))           # arka ayaklar oturaktan yukarı hafif geriye
        for y in (-S / 2 + 0.02, S / 2 - 0.02):
            kutu("Kusak", (S - 0.06, 0.02, 0.03), (0, y, 0.16), cam, 0.003)
            kutu("Etek", (S - 0.06, 0.02, 0.06), (0, y, H - 0.055), cam, 0.003)
        for x in (-S / 2 + 0.02, S / 2 - 0.02):
            kutu("Kusak", (0.02, S - 0.06, 0.03), (x, 0, 0.22), cam, 0.003)
            kutu("Etek", (0.02, S - 0.06, 0.06), (x, 0, H - 0.055), cam, 0.003)
        kutu("Oturak", (S, S, 0.025), (0, 0, H - 0.0125), cam, 0.006)
        kutu("Asinma", (0.24, 0.20, 0.001), (0, -0.02, H + 0.0003), asinmis)
        # Arkalık: üst tabla ve üç dik çıta (arka ayaklarla birlikte eğik).
        n0 = len(p)
        yb = S / 2 - 0.02
        kutu("UstTabla", (S - 0.02, 0.025, 0.085), (0, yb, 0.84), cam, 0.006)
        kutu("AltTabla", (S - 0.06, 0.02, 0.03), (0, yb, 0.56), cam, 0.003)
        for x in (-0.09, 0.0, 0.09):
            kutu("Cita", (0.03, 0.018, 0.27), (x, yb, 0.70), cam, 0.003)
        dondur(p[n0:], -4, "X", (0, yb, H))
        bitir("Sandalye_K7", 0.9, 1.4)

    elif hangi == "bulasik":
        tabak_m = ortak.malzeme("K7_Bulasik_Tabak", (0.72, 0.70, 0.64), puruz=0.3)
        leke = ortak.malzeme("K7_Bulasik_Leke", (0.35, 0.10, 0.03), puruz=0.6)
        yag = ortak.malzeme("K7_Bulasik_Yag", (0.30, 0.25, 0.10), puruz=0.2)
        aluminyum = ortak.malzeme("K7_Tencere_Aluminyum", (0.40, 0.40, 0.39), puruz=0.4, metal=0.9)
        yanik = ortak.malzeme("K7_Tencere_Yanik", (0.05, 0.03, 0.015), puruz=0.8)
        bakalit = ortak.malzeme("K7_Tencere_Kulp", (0.02, 0.015, 0.012), puruz=0.4)
        cam = ortak.malzeme("K7_Bulasik_Cam", (0.50, 0.53, 0.50), puruz=0.05)
        cay = ortak.malzeme("K7_Bulasik_Cay", (0.18, 0.04, 0.01), puruz=0.1)
        # Tabak yığını: beş tabak, hepsi biraz kaymış; en üsttekinde salça ve yağ lekesi, çatal.
        tx, ty = -0.12, 0.04
        z = 0.0
        for i in range(5):
            ox, oy_ = rnd.uniform(-0.01, 0.01), rnd.uniform(-0.01, 0.01)
            torna("Tabak", [(0, 0), (0.07, 0), (0.105, 0.016), (0.103, 0.018), (0.068, 0.005), (0, 0.005)], (tx + ox, ty + oy_, z), tabak_m, 24)
            z += 0.012
        sil("Leke", 0.04, 0.001, (tx + 0.01, ty - 0.01, z - 0.007), leke, 16)
        sil("Yag", 0.025, 0.001, (tx - 0.03, ty + 0.03, z - 0.0068), yag, 12)
        kutu("Catal", (0.17, 0.012, 0.003), (tx + 0.01, ty + 0.0, z + 0.0), aluminyum, 0.001)
        dondur([p[-1]], 25, "Z", (tx, ty, z))
        # Tencere: içi yanık, kapağı kaymış ve kenara dayalı.
        px_, py_ = 0.08, 0.06
        torna("Tencere", [(0, 0), (0.098, 0), (0.10, 0.01), (0.10, 0.10), (0.095, 0.10), (0.095, 0.006), (0, 0.006)], (px_, py_, 0), aluminyum, 28)
        sil("Yanik", 0.094, 0.003, (px_, py_, 0.008), yanik, 24)
        for yon in (-1, 1):
            kutu("Kulp", (0.035, 0.022, 0.015), (px_ + yon * 0.115, py_, 0.085), bakalit, 0.004)
        k0 = len(p)                                      # kapak: tencerenin yanına, tezgâha dayalı
        torna("Kapak", [(0, 0.01), (0.104, 0), (0.104, 0.006), (0, 0.016)], (0, 0, 0), aluminyum, 28)
        kure(0.012, (0, 0, 0.022), bakalit, (1, 1, 0.7))
        dondur(p[k0:], 70, "Y", (0, 0, 0))
        tasi(p[k0:], (px_ + 0.175, py_ - 0.01, 0.10))
        # Çay bardakları: ikisi dik (dipte çay artığı), biri yan yatmış; bir su bardağı.
        cay_bardagi(-0.02, -0.12, 0, cam, cay, dolu=0.12)
        cay_bardagi(0.04, -0.14, 0, cam, cay, dolu=0.08)
        n0 = len(p)
        cay_bardagi(0.0, 0.0, 0, cam, cay, dolu=0.05)
        dondur(p[n0:], 90, "X", (0, 0, 0))
        dondur(p[n0:], 35, "Z", (0, 0, 0))
        tasi(p[n0:], (0.13, -0.10, 0.022))
        torna("SuBardagi", [(0, 0), (0.030, 0), (0.035, 0.11), (0.032, 0.11), (0.027, 0.006), (0, 0.006)], (-0.20, -0.12, 0), cam, 18)
        bitir("Bulasik_Yigini_K7", 0.2, 0.7)

    else:
        raise SystemExit("bilinmeyen eşya: " + hangi)


if __name__ == "__main__":
    hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "botlar"
    ortak.sahneyi_temizle()
    uret(hangi)
