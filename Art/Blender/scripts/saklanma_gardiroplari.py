# saklanma_gardiroplari.py
# İçine saklanılabilen altı gardırop (#97). Her kat kendi modelini alır (#83: "katlar arasında model tekrarı yok"),
# her biri o katın kiracısını anlatır:
#   Gardirop_K1 : 1. kat, tahliye edilen kiracı. 80'lerin ucuz sunta gardırobu, solmuş beyaz laminat, köşesi
#                 kırık, bir kapağın kulbu düşmüş (iki vida deliği ve kulbun bıraktığı açık renkli iz kalmış).
#   Gardirop_K2 : 2. kat, yaşlı tamirci Nuri. Dar (0,75 m), tek kapılı koyu meşe; kapıda kemerli küçük ayna,
#                 tepesinde oymalı taç. 2. katın odaları dar olduğu için en çok 0,80 x 0,58.
#   Gardirop_K3 : 3. kat, Kerem'in genç ailesi. Pastel maviye boyalı iki kapılı dolap, altta (sabit) çekmece,
#                 sağ kapakta çocuk boyunda çıkartmalar, sol kapağın altında mum boya karalaması.
#   Gardirop_K4 : 4. kat, Hasan'ın saklandığı boş daire. Duvara gömme görünümlü, boyasız, lekeli kontrplak dolap;
#                 Kapi_1'in İÇ yüzünde tırnak izleri ve gün sayan çentikler (Hasan burada saklandı).
#   Gardirop_K7 : 7. kat, eski kapıcı Cemal'in evi. Askerî tip gri-yeşil sac dolap, iki dar kapak, kapaklarda
#                 GERÇEK havalandırma yarıkları (içeride saklanan oyuncu yarıklardan dışarıyı görür).
#   Gardirop_K8 : 8. kat, Yönetim bürosu. Uzun, iki kapılı bej sac büro dolabı, sağ kapakta silindir kilit plakası.
#
# NASIL ÇALIŞTIRILIR (her model ayrı bir Blender çağrısı; ad "--" işaretinden sonra verilir):
#   blender -b --factory-startup --python saklanma_gardiroplari.py -- Gardirop_K1 [--onizleme yol.png]
# --onizleme verilirse beş görüntü yazılır: yol.png (çaprazdan, kapılar kapalı), yol_on.png (tam önden),
# yol_acik.png (kapılar açık), yol_ic.png (önden içeri bakış) ve yol_kapi.png (Kapi_1'in iç yüzü).
# Görüntüler depoya girmez, yalnızca gözle kontrol içindir.
#
# PARÇALAR (acilir_ortak.py sözleşmesi, Unity'de GrayboxKurucu.AcilirSablonKur okur)
#   Govde  : gövde; içi boş, önü açık. PİVOTU ARKA-ALT ORTA: duvarın dibi, yerde (Blender (0, 0, 0)).
#            Bütün model y <= 0 tarafında durur: arka yüz y = 0'da, ön yüz -y'de (Unity'de +Z).
#   Kapi_1 : sol kapı (önden bakana göre sol). PİVOTU sol dış kenarın, ön yüzün, alt ucunun üstünde.
#   Kapi_2 : sağ kapı (iki kapılılarda). PİVOTU sağ dış kenarın, ön yüzün, alt ucunun üstünde.
# Unity kapının hangi yönde döneceğini parçanın gövdesinin pivotun hangi yanında durduğundan bulur;
# iki kapı da odaya (öne) doğru açılır.
#
# HER LEVHA İKİ KATMAN
# Duvarlar 18 mm. Dışta 15 mm "dış" malzeme, içte 3 mm "iç" malzeme var: içerisi dıştan daha koyu ve
# yıpranmış görünsün diye (saklanan oyuncu yalnızca içeriyi görür). İki katman bitişik olduğu için aralarındaki
# yüzler görünmez; dışarıdan bakan için tek bir 18 mm'lik levhadır.
#
# DÖNDÜRME YOK (ortak.kutu tuzağı, bkz. vana.py): eğik çizik, yıldız, kemer gibi şekiller döndürülmüş kutuyla
# değil, köşe noktaları tek tek hesaplanan "prizma" ile yapılır. Renkler doğrusal değerdir (bkz. gardirop.py):
# Unity onları sRGB'ye çevirip daha açık gösterir, bu yüzden koyu seçilir.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))   # ortak.py ve acilir_ortak.py bu klasörde; Python onları bulsun
import bpy                                   # Blender'ın kendisi
import bmesh                                 # köşe noktası düzeyinde mesh kurmak için (prizma)
import ortak                                 # bütün eşya betiklerinin ortak yardımcıları
import acilir_ortak as ac                    # açılır mobilya yardımcıları (levha, KAL)
from mathutils import Vector                 # 3B vektör (çubuk yönü için)

KAL = ac.KAL          # 0,018: bir levhanın toplam kalınlığı (18 mm)
DIS = 0.015           # dış katman (görünen yüz) kalınlığı
ICK = KAL - DIS       # iç katman (koyu astar) kalınlığı: 3 mm

p = []                      # şu anda kurulan parçanın (gövde ya da kapı) alt parçaları; sonunda birleştirilir
rnd = random.Random(97)     # sabit tohum: lekeler ve çizikler her çalıştırmada aynı çıksın
M = ortak.malzeme           # kısa ad: M("ad", (r, g, b), pürüz, metal)


# ---------- YARDIMCILAR ----------

def al(n):
    """Parçayı şu anki listeye ekler ve geri verir (tek satırda hem yarat hem ekle)."""
    p.append(n)
    return n


def L(ad, x0, x1, y0, y1, z0, z1, mat, pah=0.0):
    """Köşe değerleriyle verilen dikdörtgen levha. pah > 0 ise kenarları hafifçe yuvarlatılır."""
    return al(ac.levha(ad, min(x0, x1), max(x0, x1), min(y0, y1), max(y0, y1), min(z0, z1), max(z0, z1), mat, pah))


def kutu(ad, olcu, konum, mat, pah=0.0):
    """Merkezi ve ölçüsüyle verilen kutu (ortak.kutu + listeye ekle)."""
    return al(ortak.kutu(ad, olcu, konum, mat, pah))


def prizma(ad, noktalar, k0, k1, mat, eksen="y"):
    """
    Düz bir çokgeni kalınlaştırır: yıldız, kemer, lekeli yama, kırık köşe gibi şekiller için.
    eksen "y": noktalar (x, z) çiftleri, kalınlık y'de k0'dan k1'e (öne/arkaya bakan yüzeyler).
    eksen "x": noktalar (y, z) çiftleri, kalınlık x'te k0'dan k1'e (yana bakan yüzeyler).
    Köşe noktaları doğrudan hesaplandığı için hiçbir döndürme gerekmez.
    """
    bm = bmesh.new()                                         # boş bir "düzenlenebilir mesh"
    if eksen == "y":
        on = [bm.verts.new((a, k0, b)) for (a, b) in noktalar]   # birinci yüzün köşeleri
        ar = [bm.verts.new((a, k1, b)) for (a, b) in noktalar]   # ikinci yüzün köşeleri
    else:
        on = [bm.verts.new((k0, a, b)) for (a, b) in noktalar]
        ar = [bm.verts.new((k1, a, b)) for (a, b) in noktalar]
    bm.faces.new(on)                                         # ön yüz (çokgenin kendisi)
    bm.faces.new(list(reversed(ar)))                         # arka yüz (ters sırayla: dışa baksın)
    n = len(noktalar)
    for i in range(n):                                       # kenarları kapatan dörtgenler
        j = (i + 1) % n                                      # son nokta ilk noktaya bağlanır
        bm.faces.new((on[i], on[j], ar[j], ar[i]))
    # İçbükey çokgenler (yıldız, kalp) Unity'de yanlış üçgenlenebilir; burada "kulak kesme" yöntemiyle
    # kendimiz üçgenleriz, Unity'ye hazır üçgen gider.
    bmesh.ops.triangulate(bm, faces=[f for f in bm.faces if len(f.verts) > 4], quad_method="BEAUTY", ngon_method="EAR_CLIP")
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])     # bütün yüzler dışa baksın (yönü biz düşünmeyelim)
    me = bpy.data.meshes.new(ad)                             # Blender mesh verisi
    bm.to_mesh(me)                                           # bmesh'i ona yaz
    bm.free()                                                # belleği bırak
    nesne = bpy.data.objects.new(ad, me)                     # mesh'i taşıyan nesne
    bpy.context.collection.objects.link(nesne)               # sahneye ekle (yoksa seçilemez, birleştirilemez)
    ortak.ata(nesne, mat)
    return al(nesne)


def daire(cx, cz, r, n=20, rz=None):
    """(cx, cz) merkezli r yarıçaplı çemberin n noktası (rz verilirse elips)."""
    rz = r if rz is None else rz
    return [(cx + r * math.cos(2 * math.pi * i / n), cz + rz * math.sin(2 * math.pi * i / n)) for i in range(n)]


def leke(cx, cz, rx, rz, n=16, sapma=0.35):
    """Düzensiz kenarlı bir leke çokgeni: her noktanın yarıçapı rastgele oynar."""
    s = []
    for i in range(n):
        a = 2 * math.pi * i / n
        k = 1.0 + rnd.uniform(-sapma, sapma)                 # bu noktanın yarıçap çarpanı
        s.append((cx + rx * k * math.cos(a), cz + rz * k * math.sin(a)))
    return s


def serit(ad, a, b, g0, g1, k0, k1, mat, eksen="y"):
    """a'dan b'ye uzanan, başta g0 sonda g1 genişliğinde şerit (çizik, çentik, akıntı izi)."""
    dx, dz = b[0] - a[0], b[1] - a[1]
    u = math.hypot(dx, dz)                                   # şeridin boyu
    nx, nz = -dz / u, dx / u                                 # şeride dik birim yön
    pts = [(a[0] + nx * g0 / 2, a[1] + nz * g0 / 2), (b[0] + nx * g1 / 2, b[1] + nz * g1 / 2),
           (b[0] - nx * g1 / 2, b[1] - nz * g1 / 2), (a[0] - nx * g0 / 2, a[1] - nz * g0 / 2)]
    return prizma(ad, pts, k0, k1, mat, eksen)


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına uzanan silindir (askı teli, kanca). Dönüş meshe işlenir."""
    d = Vector(b) - Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((Vector(a) + Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")        # silindirin z eksenini a->b yönüne çevir
    bpy.ops.object.transform_apply(rotation=True)            # dönüşü meshe işle; nesnede dönüş kalmasın
    ortak.ata(n, mat)
    return al(n)


def kure(r, konum, mat, olcek=(1, 1, 1), seg=14):
    """Küre (topuz, kulp başı); olcek ile basık ya da uzun yapılabilir."""
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(6, seg // 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    return al(n)


def dik_silindir(r, boy, konum, mat, kose=12):
    """Dik (z ekseninde) silindir: menteşe topuzu."""
    return al(ortak.silindir("Silindir", r, boy, konum, mat, kose))


def simit(R, r, konum, mat, seg=(24, 8)):
    """Yüzü -y'ye bakan simit (oyma kıvrım, halka kulp). Dönüş, birleştirilirken meshe işlenir."""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg[0], minor_segments=seg[1],
                                     location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return al(n)


def kasa(W, Db, H, za, dis, ic, pah=0.002):
    """
    Önü açık gövde kutusu: iki yan (yere kadar), taban (za yüksekliğinde), tavan ve arka; her biri iki katman.
    Gövde y = -Db (ön) ile y = 0 (arka, duvar) arasında durur. İç ölçüleri sözlük olarak döndürür.
    """
    for s in (-1, 1):                                        # s = -1 sol yan, +1 sağ yan
        xd, xa, xi = s * W / 2, s * (W / 2 - DIS), s * (W / 2 - KAL)   # dış yüz, katman sınırı, iç yüz
        L("Yan", xd, xa, -Db, 0, 0, H, dis, pah)                        # dış katman: yerden tepeye
        L("YanIc", xa, xi, -Db, -DIS, za + DIS, H - DIS, ic)            # koyu astar
    x0, x1 = -W / 2 + KAL, W / 2 - KAL                       # iki yanın arası
    L("Taban", x0, x1, -Db, -DIS, za, za + DIS, dis)         # taban levhası
    L("TabanIc", x0, x1, -Db, -KAL, za + DIS, za + KAL, ic)  # tabanın üst (içeriden basılan) astarı
    L("Tavan", x0, x1, -Db, 0, H - DIS, H, dis)              # tavan levhası
    L("TavanIc", x0, x1, -Db, -KAL, H - KAL, H - DIS, ic)    # tavanın alt astarı
    L("Arka", x0, x1, -DIS, 0, za, H - DIS, dis)             # arka levha (dış yüzü duvara dayanır, y = 0)
    L("ArkaIc", x0, x1, -KAL, -DIS, za + DIS, H - DIS, ic)   # arkanın astarı
    return {"en": W - 2 * KAL, "derin": Db - KAL, "zemin": za + KAL, "tavan": H - KAL}


def kanat(xm, xs, z0, z1, y_on, dis, ic, pah=0.003):
    """Kapı levhası: xm menteşe kenarı, xs serbest kenar; ön yüzü y_on'da, kalınlığı +y (içeri) yönünde."""
    L("Kanat", xm, xs, y_on, y_on + DIS, z0, z1, dis, pah)   # dış katman
    L("KanatIc", xm, xs, y_on + DIS, y_on + KAL, z0, z1, ic) # iç yüz astarı (saklanan oyuncunun gördüğü)


def parca(ad, pivot, kur):
    """
    Ayrı bir nesne (kapı) kurar: kur() fonksiyonunun yarattığı alt parçaları birleştirir, pivotu menteşeye taşır.
    Gövdenin parça listesi bu sırada kenara alınır, sonra geri konur.
    """
    global p
    eski = p                 # gövdenin listesi bekletiliyor
    p = []                   # kapının parçaları buraya
    kur()
    n = ortak.birlestir(p, ad)
    ortak.yumusat(n, 35)
    ortak.pivotu_tasi(n, pivot)
    p = eski                 # gövdenin listesine dön
    return n


def ic_olcu_yaz(ad, olc, not_=""):
    """İç boşluğu yazdırır (bir insan sığıyor mu: en >= 0,50, derin >= 0,45, boy >= 1,70)."""
    boy = olc["tavan"] - olc["zemin"]
    print("ICOLCU", ad, "en", round(olc["en"], 3), "derin", round(olc["derin"], 3), "boy", round(boy, 3), not_)


# ---------- 1. KAT: tahliye edilen kiracının ucuz sunta gardırobu ----------

def gardirop_k1():
    lam = M("Gardirop_K1_Laminat", (0.50, 0.48, 0.42), 0.45)        # solmuş, sararmış beyaz laminat
    iz = M("Gardirop_K1_KulpIzi", (0.60, 0.58, 0.52), 0.40)         # kulbun altında solmadan kalan açık iz
    kir = M("Gardirop_K1_Kir", (0.30, 0.28, 0.23), 0.80)            # el ve ayakkabı kiri
    sunta = M("Gardirop_K1_Sunta", (0.20, 0.13, 0.06), 0.95)        # kırık köşeden görünen sunta
    ic = M("Gardirop_K1_Ic", (0.17, 0.16, 0.13), 0.85)              # içi: koyu, yıpranmış
    supurge = M("Gardirop_K1_Supurgelik", (0.26, 0.24, 0.20), 0.70) # süpürgelik (tekmelenmiş)
    kulp = M("Gardirop_K1_Kulp", (0.08, 0.05, 0.03), 0.40)          # kahverengi plastik kulp
    vida = M("Gardirop_K1_Vida", (0.02, 0.02, 0.02), 0.60)          # vida delikleri
    bant = M("Gardirop_K1_Bant", (0.33, 0.20, 0.07), 0.30)          # yırtılmış koli bandı
    krom = M("Gardirop_K1_Boru", (0.45, 0.45, 0.44), 0.35, 0.8)     # askı borusu ve tel askı

    W, Db, H, ZA = 1.00, 0.54, 1.86, 0.07       # en, gövde derinliği, gövde boyu, taban yüksekliği
    YON = -(Db + 0.002 + KAL)                   # kapıların ön yüzü (-0,56): gövdeden 2 mm aralıkla bindirme kapı
    olc = kasa(W, Db, H, ZA, lam, ic)
    L("Supurgelik", -W / 2 + KAL, W / 2 - KAL, -Db + 0.03, -Db + 0.03 + KAL, 0, ZA, supurge)   # 3 cm içeride
    prizma("Surtme", leke(-0.20, 0.035, 0.05, 0.015), -Db + 0.0295, -Db + 0.0305, kir)        # süpürgelikte ayakkabı izi
    prizma("Surtme", leke(0.25, 0.03, 0.07, 0.012), -Db + 0.0295, -Db + 0.0305, kir)
    # Üst tabla: gövdenin ve kapıların üstüne 1 cm taşan ince levha.
    L("UstTabla", -0.51, 0.51, YON - 0.01, 0, H, H + 0.018, lam, 0.003)
    # Tablanın sol ön köşesi kırık: laminat kopmuş, altındaki kahverengi sunta görünüyor.
    prizma("Kirik", [(-0.51, H), (-0.51, H + 0.018), (-0.488, H + 0.018), (-0.484, H + 0.012), (-0.476, H + 0.011),
                     (-0.471, H + 0.005), (-0.462, H)], YON - 0.0111, YON - 0.0099, sunta)
    L("KirikYan", -0.5111, -0.5099, YON - 0.01, YON + 0.012, H, H + 0.018, sunta)
    # Askı borusu: 1,805 m'de, altında 1,70 m boşluk kalsın diye tavana yakın.
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.0125, depth=W - 2 * KAL, location=(0, -0.27, 1.805),
                                        rotation=(0, math.pi / 2, 0))
    boru = bpy.context.active_object
    ortak.ata(boru, krom)
    al(boru)
    for s in (-1, 1):                                        # borunun iki ucundaki taşıyıcılar
        kutu("BoruTasiyici", (0.006, 0.04, 0.04), (s * (W / 2 - KAL - 0.003), -0.27, 1.805), krom)
    # Taşınırken unutulmuş tek bir tel askı (borunun sağ ucunda, derinliğe doğru asılı).
    x = 0.40
    for a, b in (((x, -0.27, 1.775), (x, -0.27, 1.822)), ((x, -0.27, 1.822), (x, -0.256, 1.826)),
                 ((x, -0.256, 1.826), (x, -0.247, 1.812)),                                      # boruyu saran kanca
                 ((x, -0.27, 1.775), (x, -0.46, 1.635)), ((x, -0.27, 1.775), (x, -0.08, 1.635)),  # omuzlar
                 ((x, -0.46, 1.635), (x, -0.08, 1.635))):                                       # alt çubuk
        cubuk(a, b, 0.0013, krom, 6)
    ic_olcu_yaz("Gardirop_K1", olc, "(askı borusunun altı: 1,7015)")

    Z0, Z1 = ZA + 0.005, H - 0.005               # kapıların alt ve üst kenarı
    xm1, xm2 = -W / 2 + 0.002, W / 2 - 0.002     # menteşe kenarları (dış kenarlar)

    def sol():
        kanat(xm1, -0.0015, Z0, Z1, YON, lam, ic)
        # Düşmüş kulp: etrafında el kiri, ortasında kulbun güneşten koruduğu açık renkli iz, iki vida deliği.
        prizma("ElKiri", leke(-0.045, 1.025, 0.035, 0.12, 18, 0.15), YON - 0.0008, YON + 0.0002, kir)
        kutu("KulpIzi", (0.024, 0.0008, 0.165), (-0.045, YON - 0.0012, 1.025), iz)
        for z in (0.955, 1.095):
            kutu("VidaDeligi", (0.005, 0.001, 0.005), (-0.045, YON - 0.0018, z), vida)
        # Kapıları kapalı tutmak için yapıştırılıp yırtılmış koli bandının kalan ucu.
        prizma("Bant", [(-0.06, 1.235), (-0.0015, 1.232), (-0.0015, 1.278), (-0.052, 1.281), (-0.057, 1.27),
                        (-0.05, 1.262), (-0.059, 1.25), (-0.053, 1.243)], YON - 0.0016, YON - 0.0006, bant)
        prizma("Surtme", leke(-0.30, 0.20, 0.06, 0.03), YON - 0.0008, YON + 0.0002, kir)        # alt köşede ayakkabı izi

    def sag():
        kanat(xm2, 0.0015, Z0, Z1, YON, lam, ic)
        prizma("ElKiri", leke(0.045, 1.025, 0.035, 0.13, 18, 0.15), YON - 0.0008, YON + 0.0002, kir)
        kutu("Kulp", (0.014, 0.014, 0.15), (0.045, YON - 0.026, 1.025), kulp, 0.003)   # dik plastik kulp
        for z in (0.965, 1.085):
            kutu("KulpAyak", (0.012, 0.02, 0.014), (0.045, YON - 0.010, z), kulp, 0.002)
        # Alt dış köşe kırık: önde, yanda ve altta laminat kopmuş.
        prizma("Kirik", [(xm2, Z0), (0.455, Z0), (0.462, Z0 + 0.008), (0.470, Z0 + 0.006), (0.474, Z0 + 0.017),
                         (0.483, Z0 + 0.022), (0.488, Z0 + 0.037), (xm2, Z0 + 0.043)], YON - 0.001, YON + 0.0002, sunta)
        L("KirikYan", xm2 - 0.0001, xm2 + 0.0009, YON, YON + 0.012, Z0, Z0 + 0.035, sunta)
        L("KirikAlt", 0.47, xm2, YON, YON + 0.010, Z0 - 0.0009, Z0 + 0.0001, sunta)
        prizma("Bant", [(0.0015, 1.232), (0.066, 1.236), (0.060, 1.247), (0.068, 1.258), (0.061, 1.268), (0.067, 1.279),
                        (0.0015, 1.278)], YON - 0.0016, YON - 0.0006, bant)

    k1 = parca("Kapi_1", (xm1, YON, Z0), sol)
    k2 = parca("Kapi_2", (xm2, YON, Z0), sag)
    return [k1, k2]


# ---------- 2. KAT: Nuri'nin dar, tek kapılı meşe gardırobu ----------

def gardirop_k2():
    mese = M("Gardirop_K2_Mese", (0.065, 0.038, 0.016), 0.40)        # koyu cilalı meşe
    koyu = M("Gardirop_K2_Mese_Koyu", (0.032, 0.018, 0.008), 0.45)   # oyma ve çıtalar
    ic = M("Gardirop_K2_Ic", (0.040, 0.028, 0.017), 0.85)            # içi: cilasız, kararmış
    # Ayna: gardirop.py'deki gibi yarım metal, gümüşi gri (tam metal ayna Unity'de simsiyah görünüyordu).
    ayna = M("Gardirop_K2_Ayna", (0.42, 0.44, 0.45), 0.12, 0.4)
    pirinc = M("Gardirop_K2_Pirinc", (0.50, 0.38, 0.18), 0.40, 0.6)
    delik = M("Gardirop_K2_Delik", (0.01, 0.008, 0.005), 0.8)

    W, Db, HB, ZA = 0.75, 0.53, 1.84, 0.09
    YON = -(Db + 0.002 + KAL)                     # kapının ön yüzü: -0,55
    olc = kasa(W, Db, HB, ZA, mese, ic)
    # Kaide: önde ortası kemer biçiminde oyulmuş tahta; iki yanda "ayak" kalır (eski dolapların köşe ayakları).
    kemer = [(-0.375, 0), (-0.375, ZA), (0.375, ZA), (0.375, 0), (0.27, 0)]
    kemer += [(0.27 * math.cos(math.pi * i / 16), 0.045 * math.sin(math.pi * i / 16)) for i in range(1, 16)]
    kemer += [(-0.27, 0)]
    prizma("KaideOn", kemer, YON, -Db + 0.002, mese)
    L("KaideArka", -W / 2 + KAL, W / 2 - KAL, -Db + 0.04, -Db + 0.04 + KAL, 0, ZA, koyu)   # kemerin ardı karanlık
    for s in (-1, 1):                                        # yanlarda ince kaide pervazı
        L("KaideYan", s * W / 2, s * (W / 2 + 0.006), YON, 0, 0, ZA, mese, 0.002)
    # Taç: üç basamaklı korniş, en üstte ortası kabarık oymalı alın tahtası.
    L("Tac1", -0.383, 0.383, YON - 0.008, 0, HB, HB + 0.025, mese, 0.003)
    L("Tac2", -0.390, 0.390, YON - 0.016, 0, HB + 0.025, HB + 0.040, koyu, 0.003)
    L("Tac3", -0.395, 0.395, YON - 0.025, 0, HB + 0.040, HB + 0.060, mese, 0.004)
    zb = HB + 0.06                                           # alın tahtasının oturduğu yükseklik (1,90)
    alin = [(-0.30, zb)]
    for i in range(0, 41):                                   # üst kenar: ortası tepe, uçlarda küçük dalgalar
        x = -0.30 + 0.60 * i / 40
        z = zb + 0.025 + 0.045 * math.cos(math.pi * x / 0.6) + 0.004 * math.sin(10 * math.pi * x / 0.6)
        alin.append((x, z))
    alin.append((0.30, zb))
    prizma("Alin", alin, YON - 0.020, YON + 0.002, mese)
    # Oyma gül: ortada koyu bir göbek, çevresinde sekiz yaprak, ortada küçük topuz.
    prizma("Gul", daire(0, zb + 0.035, 0.030, 24), YON - 0.026, YON - 0.020, koyu)
    for i in range(8):
        a = 2 * math.pi * i / 8
        prizma("Yaprak", daire(0.020 * math.cos(a), zb + 0.035 + 0.020 * math.sin(a), 0.009, 12),
               YON - 0.028, YON - 0.026, mese)
    prizma("Topuz", daire(0, zb + 0.035, 0.008, 12), YON - 0.0295, YON - 0.028, koyu)
    for s in (-1, 1):                                        # alın tahtasının iki yanında kıvrık oyma
        simit(0.022, 0.0045, (s * 0.20, YON - 0.0235, zb + 0.028), koyu)
        simit(0.010, 0.0035, (s * 0.245, YON - 0.0235, zb + 0.020), koyu)
    ic_olcu_yaz("Gardirop_K2", olc)

    Z0, Z1 = ZA + 0.005, HB - 0.005
    xm, xs = -W / 2 + 0.002, W / 2 - 0.002       # menteşe solda, kulp sağda

    def kapi():
        kanat(xm, xs, Z0, Z1, YON, mese, ic)
        for (a, b, c, d) in ((xm + 0.035, xs - 0.035, Z1 - 0.049, Z1 - 0.035), (xm + 0.035, xs - 0.035, Z0 + 0.035, Z0 + 0.049),
                             (xm + 0.035, xm + 0.049, Z0 + 0.035, Z1 - 0.035), (xs - 0.049, xs - 0.035, Z0 + 0.035, Z1 - 0.035)):
            L("Cita", a, b, YON - 0.006, YON, c, d, koyu, 0.001)          # kapı kenarında çerçeve çıtası
        # Kemerli ayna: koyu bir çerçevenin önünde, kenarlarda 1,8 cm çerçeve görünür.
        def kemerli(r, alt, ust, yay):
            s = [(-r, alt), (r, alt), (r, ust)]
            s += [(r * math.cos(math.pi * i / 20), ust + yay * math.sin(math.pi * i / 20)) for i in range(1, 20)]
            return s + [(-r, ust)]
        prizma("AynaCerceve", kemerli(0.170, 1.02, 1.56, 0.080), YON - 0.007, YON + 0.001, koyu)
        prizma("Ayna", kemerli(0.152, 1.038, 1.56, 0.062), YON - 0.0085, YON - 0.0065, ayna)
        prizma("Gul", daire(0, 1.72, 0.024, 20), YON - 0.006, YON + 0.001, koyu)          # aynanın üstünde küçük oyma
        # Alt göbek: kabarık pano ve çevresinde koyu boncuk çıtası.
        L("Gobek", -0.26, 0.26, YON - 0.006, YON, 0.25, 0.86, mese, 0.003)
        for (a, b, c, d) in ((-0.272, 0.272, 0.238, 0.25), (-0.272, 0.272, 0.86, 0.872),
                             (-0.272, -0.26, 0.25, 0.86), (0.26, 0.272, 0.25, 0.86)):
            L("Boncuk", a, b, YON - 0.008, YON, c, d, koyu, 0.001)
        # Pirinç topuz ve kalkan biçimli anahtar deliği aynası.
        prizma("TopuzAlti", daire(0.33, 1.00, 0.016, 16), YON - 0.004, YON + 0.001, pirinc)
        kure(0.014, (0.33, YON - 0.017, 1.00), pirinc, (1, 0.9, 1))
        kalkan = [(0.318, 0.945), (0.342, 0.945), (0.342, 0.915), (0.33, 0.895), (0.318, 0.915)]
        prizma("AnahtarAynasi", kalkan, YON - 0.003, YON + 0.001, pirinc)
        L("AnahtarDeligi", 0.328, 0.332, YON - 0.0035, YON - 0.002, 0.910, 0.930, delik)
        for z in (0.30, 0.96, 1.62):                         # menteşe topuzları: tam menteşe çizgisinde
            dik_silindir(0.006, 0.06, (xm, YON, z), pirinc)

    return [parca("Kapi_1", (xm, YON, Z0), kapi)]


# ---------- 3. KAT: Kerem'in ailesinin pastel mavi dolabı ----------

def gardirop_k3():
    boya = M("Gardirop_K3_Boya", (0.33, 0.48, 0.62), 0.55)           # pastel mavi boya
    beyaz = M("Gardirop_K3_Beyaz", (0.62, 0.62, 0.58), 0.50)         # beyaz pervaz ve çıtalar
    ic = M("Gardirop_K3_Ic", (0.15, 0.18, 0.20), 0.85)               # içi: koyu, yıpranmış
    topuz = M("Gardirop_K3_Topuz", (0.70, 0.68, 0.62), 0.25)         # beyaz porselen topuz
    sari = M("Gardirop_K3_Cikartma_Sari", (0.80, 0.60, 0.05), 0.35)
    turuncu = M("Gardirop_K3_Cikartma_Turuncu", (0.85, 0.30, 0.02), 0.35)
    kirmizi = M("Gardirop_K3_Cikartma_Kirmizi", (0.60, 0.04, 0.03), 0.35)
    pembe = M("Gardirop_K3_Cikartma_Pembe", (0.75, 0.20, 0.35), 0.35)
    siyah = M("Gardirop_K3_Cikartma_Siyah", (0.02, 0.02, 0.02), 0.40)
    kagit = M("Gardirop_K3_Cikartma_Kagit", (0.72, 0.71, 0.66), 0.70) # sökülmüş çıkartmadan kalan kâğıt
    mumboya = M("Gardirop_K3_MumBoya", (0.04, 0.10, 0.45), 0.60)     # mavi mum boya

    W, Db, H, ZA = 0.95, 0.56, 1.98, 0.22
    YON = -(Db + 0.002 + KAL)                     # kapı ve çekmece ön yüzü: -0,58
    olc = kasa(W, Db, H, ZA, boya, ic)
    # Çekmece bölmesi: gövdenin altında, ayrı taban levhasıyla kapalı; çekmece önü sabit (açılmaz).
    L("AltTaban", -W / 2 + KAL, W / 2 - KAL, -Db, -DIS, 0.04, 0.058, boya)
    L("Supurgelik", -W / 2 + KAL, W / 2 - KAL, -Db + 0.03, -Db + 0.03 + KAL, 0, 0.04, beyaz)
    L("CekmeceOn", -0.473, 0.473, YON, YON + KAL, 0.045, 0.218, boya, 0.003)
    for (a, b, c, d) in ((-0.43, 0.43, 0.075, 0.087), (-0.43, 0.43, 0.176, 0.188), (-0.43, -0.418, 0.087, 0.176), (0.418, 0.43, 0.087, 0.176)):
        L("CekmeceCita", a, b, YON - 0.005, YON, c, d, beyaz, 0.001)
    for x in (-0.22, 0.22):
        prizma("TopuzAlti", daire(x, 0.132, 0.014, 16), YON - 0.004, YON + 0.001, topuz)
        kure(0.016, (x, YON - 0.018, 0.132), topuz, (1, 0.85, 1))
    L("Korniz", -0.49, 0.49, YON - 0.015, 0, H, H + 0.02, beyaz, 0.006)   # tepede yuvarlak kenarlı beyaz korniş
    # İç yanlarda tavana yakın birer elbise kancası (iç genişliği yalnızca köşede 5 cm daraltır).
    for s in (-1, 1):
        xi = s * (W / 2 - KAL)
        cubuk((xi, -0.28, 1.85), (xi - s * 0.05, -0.28, 1.85), 0.004, beyaz)
        cubuk((xi - s * 0.05, -0.28, 1.85), (xi - s * 0.055, -0.28, 1.875), 0.004, beyaz)
    ic_olcu_yaz("Gardirop_K3", olc)

    Z0, Z1 = 0.226, H - 0.005
    xm1, xm2 = -W / 2 + 0.002, W / 2 - 0.002

    def cerceve(a, b):
        """Kapının önünde 5 cm içeride beyaz çıta çerçeve."""
        for (x0, x1, z0, z1) in ((a + 0.05, b - 0.05, Z1 - 0.062, Z1 - 0.05), (a + 0.05, b - 0.05, Z0 + 0.05, Z0 + 0.062),
                                 (a + 0.05, a + 0.062, Z0 + 0.05, Z1 - 0.05), (b - 0.062, b - 0.05, Z0 + 0.05, Z1 - 0.05)):
            L("Cita", x0, x1, YON - 0.005, YON, z0, z1, beyaz, 0.001)

    def topuz_tak(x):
        prizma("TopuzAlti", daire(x, 1.05, 0.015, 16), YON - 0.004, YON + 0.001, topuz)
        kure(0.017, (x, YON - 0.019, 1.05), topuz, (1, 0.85, 1))

    def sol():
        kanat(xm1, -0.0015, Z0, Z1, YON, boya, ic)
        cerceve(xm1, -0.0015)
        topuz_tak(-0.045)
        # Çocuğun mum boya karalaması: alt göbekte zikzak (tabanı 45-60 cm, oturan bir çocuğun boyu).
        noktalar = [(-0.36 + 0.022 * i, 0.47 + (0.07 if i % 2 else 0) + rnd.uniform(-0.012, 0.012)) for i in range(11)]
        for a, b in zip(noktalar, noktalar[1:]):
            serit("Karalama", a, b, 0.003, 0.0025, YON - 0.0007, YON + 0.0002, mumboya)

    def sag():
        kanat(xm2, 0.0015, Z0, Z1, YON, boya, ic)
        cerceve(0.0015, xm2)
        topuz_tak(0.045)
        on, yuz = YON - 0.0008, YON + 0.0002              # çıkartmalar kapı yüzünden 0,8 mm önde
        yildiz = []
        for i in range(10):                                 # beş köşeli sarı yıldız
            a = math.pi / 2 + 2 * math.pi * i / 10
            r = 0.042 if i % 2 == 0 else 0.018
            yildiz.append((0.17 + r * math.cos(a), 0.96 + r * math.sin(a)))
        prizma("Yildiz", yildiz, on, yuz, sari)
        prizma("Gunes", daire(0.31, 0.985, 0.033, 24), on, yuz, turuncu)          # gülen güneş
        for i in range(8):                                   # güneşin ışınları (üçgenler)
            a = 2 * math.pi * i / 8 + math.pi / 8
            ux, uz = math.cos(a), math.sin(a)
            prizma("Isin", [(0.31 + 0.036 * ux - 0.006 * uz, 0.985 + 0.036 * uz + 0.006 * ux),
                            (0.31 + 0.052 * ux, 0.985 + 0.052 * uz),
                            (0.31 + 0.036 * ux + 0.006 * uz, 0.985 + 0.036 * uz - 0.006 * ux)], on, yuz, turuncu)
        for x in (0.299, 0.321):                             # gözler
            prizma("Goz", daire(x, 0.994, 0.0042, 10), on - 0.0006, on + 0.0001, siyah)
        gulus = [(0.31 + 0.017 * math.cos(math.pi * (1.15 + 0.7 * i / 5)), 0.982 + 0.017 * math.sin(math.pi * (1.15 + 0.7 * i / 5)))
                 for i in range(6)]
        for a, b in zip(gulus, gulus[1:]):                   # ağız (kısa şeritlerden yay)
            serit("Agiz", a, b, 0.003, 0.003, on - 0.0006, on + 0.0001, siyah)
        # Kırmızı araba: gövde, üstte kabin, iki siyah tekerlek.
        prizma("Araba", [(0.20, 0.817), (0.30, 0.817), (0.30, 0.842), (0.28, 0.844), (0.267, 0.866), (0.228, 0.866),
                         (0.215, 0.844), (0.20, 0.842)], on, yuz, kirmizi)
        for x in (0.222, 0.278):
            prizma("Teker", daire(x, 0.817, 0.011, 14), on - 0.0006, on + 0.0001, siyah)
        kalp = [(0.12 + 0.0021 * 16 * math.sin(t) ** 3,
                 0.85 + 0.0021 * (13 * math.cos(t) - 5 * math.cos(2 * t) - 2 * math.cos(3 * t) - math.cos(4 * t)))
                for t in (2 * math.pi * i / 28 for i in range(28))]
        prizma("Kalp", kalp, on, yuz, pembe)                 # pembe kalp
        prizma("SokukCikartma", [(0.345, 0.885), (0.378, 0.889), (0.374, 0.902), (0.380, 0.912), (0.366, 0.918),
                                 (0.358, 0.910), (0.348, 0.916), (0.342, 0.900)], on, yuz, kagit)   # sökülmüş çıkartma

    k1 = parca("Kapi_1", (xm1, YON, Z0), sol)
    k2 = parca("Kapi_2", (xm2, YON, Z0), sag)
    return [k1, k2]


# ---------- 4. KAT: Hasan'ın saklandığı gömme kontrplak dolap ----------

def gardirop_k4():
    kont = M("Gardirop_K4_Kontrplak", (0.24, 0.17, 0.09), 0.80)      # boyasız, eskimiş kontrplak
    lek = M("Gardirop_K4_Leke", (0.10, 0.065, 0.03), 0.90)           # su lekeleri, kir
    ic = M("Gardirop_K4_Ic", (0.09, 0.065, 0.035), 0.95)             # içi: kararmış, rutubetli
    cizik = M("Gardirop_K4_Cizik", (0.42, 0.31, 0.18), 0.90)         # tırnakla kazınmış açık renkli ahşap
    pas = M("Gardirop_K4_Pas", (0.12, 0.07, 0.04), 0.80, 0.5)        # paslı tel kulp ve menteşe
    yun = M("Gardirop_K4_Battaniye", (0.10, 0.10, 0.09), 1.00)       # içeride katlanmış battaniye

    W, D, H = 1.05, 0.60, 1.98
    YON = -D                                       # yüz çerçevesi ve gömme kapılar aynı düzlemde: ön yüz -0,60
    Db = D - KAL                                   # yanlar yüz çerçevesinin arkasında biter (-0,582)
    ZA = 0.102
    olc = kasa(W, Db, H, ZA, kont, ic)
    olc["derin"] = D - KAL - KAL                   # içeride: kapının iç yüzünden arka astara kadar
    # Yüz çerçevesi (gömme dolap görünümü): iki dikme, üstte geniş başlık, altta kayıt, en altta karanlık tekmelik.
    L("Dikme", -0.525, -0.4785, YON, -Db, 0.03, H, kont, 0.002)
    L("Dikme", 0.4785, 0.525, YON, -Db, 0.03, H, kont, 0.002)
    L("Baslik", -0.4785, 0.4785, YON, -Db, 1.86, H, kont, 0.002)
    L("AltKayit", -0.4785, 0.4785, YON, -Db, 0.03, 0.12, kont, 0.002)
    L("Tekmelik", -W / 2 + KAL, W / 2 - KAL, -Db + 0.02, -Db + 0.02 + KAL, 0, 0.03, lek)
    for s in (-1, 1):                                        # tekmeliğin iki yanında yanların alt ucu
        L("YanAlt", s * 0.525, s * 0.507, YON, -Db, 0, 0.03, kont)
    on, yuz = YON - 0.0008, YON + 0.0002                     # lekeler ön yüzden 0,8 mm önde
    prizma("SuLekesi", leke(0.30, 1.92, 0.12, 0.035), on, yuz, lek)             # başlıkta tavandan sızmış su
    for x, uz in ((0.24, 0.10), (0.29, 0.22), (0.33, 0.06), (0.37, 0.15)):    # başlıktan aşağı akıntılar
        serit("Akinti", (x, 1.90), (x + 0.004, 1.90 - uz), 0.012, 0.002, on, yuz, lek)
    serit("Akinti", (0.50, 1.88), (0.502, 1.50), 0.010, 0.002, on, yuz, lek)  # sağ dikmeden inen uzun akıntı
    prizma("TabanLekesi", [(-0.525, 0.03), (-0.4785, 0.03), (-0.4785, 0.17), (-0.49, 0.15), (-0.50, 0.19), (-0.515, 0.16),
                            (-0.525, 0.18)], on, yuz, lek)                       # sol dikmenin dibinde rutubet
    prizma("TabanLekesi", [(-0.4785, 0.03), (0.4785, 0.03), (0.4785, 0.10), (0.30, 0.11), (0.10, 0.095), (-0.15, 0.115),
                            (-0.4785, 0.10)], on, yuz, lek)                      # alt kayıtta yerden çekmiş su izi
    # İçeride, arka sol köşede katlanmış gri battaniye (Hasan'dan kalmış).
    kutu("Battaniye", (0.40, 0.28, 0.06), (-0.27, -0.17, olc["zemin"] + 0.03), yun, 0.012)
    kutu("Battaniye", (0.38, 0.26, 0.03), (-0.26, -0.18, olc["zemin"] + 0.075), yun, 0.01)
    ic_olcu_yaz("Gardirop_K4", olc, "(kapı açıklığının boyu 1,737)")

    Z0, Z1 = 0.1215, 1.8585                      # kapılar yüz çerçevesinin içinde, 1,5 mm aralıkla
    xm1, xm2 = -0.4765, 0.4765                   # menteşeler dikmelerin iç kenarında

    def kontrplak_kenari(x0, x1):
        """Kapının serbest kenarında kontrplak katmanları: iki koyu şerit."""
        for (y0, y1) in ((YON + 0.0036, YON + 0.0072), (YON + 0.0108, YON + 0.0144)):
            L("Katman", x0, x1, y0, y1, Z0 + 0.002, Z1 - 0.002, lek)

    def tel_kulp(x):
        """Paslı, U biçiminde tel kulp."""
        for a, b in (((x, YON, 0.95), (x, YON - 0.03, 0.96)), ((x, YON - 0.03, 0.96), (x, YON - 0.03, 1.10)),
                     ((x, YON - 0.03, 1.10), (x, YON, 1.11))):
            cubuk(a, b, 0.004, pas, 8)

    def tirnak(cx, cz, boy, egim, ic_yuz):
        """Dört paralel tırnak çiziği: üstte derin, aşağıda incelen; ic_yuz kapının iç yüzü (y)."""
        a = math.radians(egim)
        dx, dz = math.sin(a), -math.cos(a)                  # çizik yönü: aşağı, egim kadar yatık
        for i in range(4):
            ox = (i - 1.5) * 0.012 * math.cos(a) + rnd.uniform(-0.002, 0.002)
            oz = (i - 1.5) * 0.012 * math.sin(a)
            u = boy * rnd.uniform(0.75, 1.0)                # her çiziğin boyu biraz farklı
            serit("Tirnak", (cx + ox, cz + oz), (cx + ox + dx * u, cz + oz + dz * u), 0.0028, 0.0006,
                  ic_yuz, ic_yuz - 0.0008, cizik)            # iç yüzden 0,8 mm içeride (dolabın içine doğru)

    def centikler(x0, z0, adet, ic_yuz):
        """Gün sayan çentikler: dörtlü dik çizgi + üstünden çapraz beşinci."""
        x = x0
        for i in range(adet):
            if i % 5 == 4:                                   # beşinci çentik grubu çapraz keser
                serit("Centik", (x - 0.048, z0 + 0.008), (x - 0.002, z0 + 0.052), 0.0025, 0.0018, ic_yuz, ic_yuz - 0.0008, cizik)
                x += 0.022                                   # gruplar arası boşluk
            else:
                serit("Centik", (x, z0 + rnd.uniform(-0.003, 0.003)), (x + rnd.uniform(-0.003, 0.003), z0 + 0.06),
                      0.0025, 0.0018, ic_yuz, ic_yuz - 0.0008, cizik)
                x += 0.011

    def sol():
        kanat(xm1, -0.0015, Z0, Z1, YON, kont, ic)
        kontrplak_kenari(-0.0015, -0.0011)
        tel_kulp(-0.05)
        prizma("SuLekesi", leke(-0.30, 0.25, 0.10, 0.07), on, yuz, lek)
        icy = YON + KAL                                      # Kapi_1'in iç yüzü (dolabın içine bakan)
        # Hasan'ın izleri: kapının aralığına (serbest kenara) yakın tırnak çizikleri ve 23 günlük çentik.
        tirnak(-0.10, 1.25, 0.28, 12, icy)
        tirnak(-0.16, 1.05, 0.22, -8, icy)
        tirnak(-0.09, 0.85, 0.25, -14, icy)
        tirnak(-0.12, 1.50, 0.18, 5, icy)
        centikler(-0.44, 1.62, 20, icy)                      # üst sıra: dört tam grup
        centikler(-0.44, 1.52, 3, icy)                       # alt sıra: üç çentik daha

    def sag():
        kanat(xm2, 0.0015, Z0, Z1, YON, kont, ic)
        kontrplak_kenari(0.0011, 0.0015)
        tel_kulp(0.05)
        prizma("SuLekesi", leke(0.22, 1.36, 0.05, 0.07, 22, 0.15), on, yuz, lek)          # üst üste binmiş su halkaları
        prizma("SuLekesi", leke(0.27, 1.27, 0.04, 0.06, 22, 0.15), on - 0.0004, yuz, lek)
        serit("Akinti", (0.25, 1.24), (0.252, 1.08), 0.014, 0.003, on, yuz, lek)          # aşağı süzülmüş iz
        tirnak(0.10, 1.10, 0.20, -10, YON + KAL)             # sağ kapının içinde de bir grup çizik

    k1 = parca("Kapi_1", (xm1, YON, Z0), sol)
    k2 = parca("Kapi_2", (xm2, YON, Z0), sag)
    # Menteşe topuzları gövdeye bağlı (kapı dönerken yerinde kalmalı): dikmenin iç kenarında, kapının ön yüzünde.
    for s in (-1, 1):
        for z in (0.30, 1.68):
            dik_silindir(0.006, 0.07, (s * 0.4775, YON - 0.002, z), pas)
    return [k1, k2]


# ---------- 7. KAT: Cemal'in askerî sac dolabı ----------

def gardirop_k7():
    boya = M("Gardirop_K7_Boya", (0.085, 0.11, 0.075), 0.50, 0.3)    # gri-yeşil askerî boya
    celik = M("Gardirop_K7_Celik", (0.35, 0.35, 0.34), 0.40, 0.85)   # boyası kalkmış çıplak sac, kol, menteşe
    pas = M("Gardirop_K7_Pas", (0.16, 0.06, 0.02), 0.90)             # pas akıntısı
    ic = M("Gardirop_K7_Ic", (0.05, 0.06, 0.045), 0.70)              # içi: koyu, yıpranmış
    koyu = M("Gardirop_K7_Koyu", (0.03, 0.03, 0.03), 0.60)           # kaide, kol çukuru
    kagit = M("Gardirop_K7_Kagit", (0.55, 0.53, 0.45), 0.80)         # etiketlikteki kart

    W, D, H, ZA = 0.80, 0.55, 1.90, 0.10
    YON = -D                                     # yüz çerçevesi ve kapılar aynı düzlemde
    Db = D - KAL
    olc = kasa(W, Db, H, ZA, boya, ic)
    olc["derin"] = D - KAL - KAL
    L("Dikme", -0.40, -0.376, YON, -Db, 0, H, boya, 0.002)          # ön dikmeler yere kadar iner (ayak gibi)
    L("Dikme", 0.376, 0.40, YON, -Db, 0, H, boya, 0.002)
    L("UstKayit", -0.376, 0.376, YON, -Db, 1.85, H, boya, 0.002)
    L("AltKayit", -0.376, 0.376, YON, -Db, ZA, 0.125, boya, 0.002)
    L("Kaide", -W / 2 + KAL, W / 2 - KAL, -Db + 0.03, -Db + 0.03 + KAL, 0, ZA, koyu)
    on, yuz = YON - 0.0008, YON + 0.0002
    for x in (-0.388, 0.388):                                # dikmelerin dibinde pas
        serit("Pas", (x, 0.002), (x + 0.001, 0.09), 0.018, 0.004, on, yuz, pas)
    prizma("Soyuk", leke(0.39, 1.20, 0.008, 0.03), on, yuz, celik)       # sağ dikmede boya kalkmış
    prizma("Soyuk", leke(-0.10, 1.875, 0.03, 0.012), on, yuz, celik)     # üst kayıtta çarpma izi
    # İçeride arka duvarda iki elbise kancası.
    for x in (-0.22, 0.22):
        cubuk((x, -KAL, 1.78), (x, -0.07, 1.78), 0.005, celik)
        cubuk((x, -0.07, 1.78), (x, -0.075, 1.805), 0.005, celik)
    ic_olcu_yaz("Gardirop_K7", olc, "(kapı açıklığının boyu 1,72)")

    Z0, Z1 = 0.1275, 1.8475
    xm1, xm2 = -0.374, 0.374
    # Üst yarıkların alt kenarları. İkinci yarık 1,545-1,557'de: saklanan oyuncunun gözü (1,55 m, bkz.
    # SaklanmaKurali.GozYuksekligi) tam bir yarığın hizasına gelsin, dışarıyı ince bir şeritten görsün.
    UST = [1.511 + 0.034 * i for i in range(6)]
    ALT = [0.24 + 0.034 * i for i in range(5)]               # alt yarıklar
    YB = 0.012                                               # yarık yüksekliği

    def yarikli_kanat(xm, xs):
        """Gerçekten delikli kapak: ortadaki sütun yarıkların arasında kalan dilimlerden kurulur."""
        a, b = min(xm, xs), max(xm, xs)
        c = (a + b) / 2
        sx0, sx1 = c - 0.10, c + 0.10                        # yarıkların yatay sınırları
        for (x0, x1) in ((a, sx0), (sx1, b)):                # yarıksız iki yan şerit, boydan boya
            L("Kanat", x0, x1, YON, YON + DIS, Z0, Z1, boya, 0.002)
            L("KanatIc", x0, x1, YON + DIS, YON + KAL, Z0, Z1, ic)
        sinirlar = [Z0]                                      # orta sütunun dolu dilimleri: yarıkların arası
        for z in ALT + UST:
            sinirlar += [z, z + YB]
        sinirlar.append(Z1)
        for i in range(0, len(sinirlar), 2):
            z0, z1 = sinirlar[i], sinirlar[i + 1]
            L("Dilim", sx0, sx1, YON, YON + DIS, z0, z1, boya)
            L("DilimIc", sx0, sx1, YON + DIS, YON + KAL, z0, z1, ic)
        for z in ALT + UST:                                  # her yarığın üstünde dışa kıvrık panjur dudağı
            L("Dudak", sx0, sx1, YON - 0.006, YON, z + YB - 0.003, z + YB + 0.002, boya)
        # Etiketlik: çelik çerçeve; içi boş (kartı yalnızca sol kapakta var).
        for (x0, x1, z0, z1) in ((c - 0.045, c + 0.045, 1.80, 1.806), (c - 0.045, c + 0.045, 1.749, 1.755),
                                 (c - 0.045, c - 0.039, 1.755, 1.80), (c + 0.039, c + 0.045, 1.755, 1.80)):
            L("Etiketlik", x0, x1, YON - 0.004, YON, z0, z1, celik)
        for z in (0.35, 0.99, 1.63):                         # menteşe topuzları: menteşe çizgisinde
            dik_silindir(0.007, 0.06, (xm, YON, z), celik)
            serit("Pas", (xm + (0.012 if xm < 0 else -0.012), z - 0.03), (xm + (0.013 if xm < 0 else -0.013), z - 0.15),
                  0.008, 0.002, YON - 0.0008, YON + 0.0002, pas)      # menteşenin altından inen pas
        serit("Pas", (c, Z0), (c + 0.002, Z0 + 0.07), 0.08, 0.02, YON - 0.0008, YON + 0.0002, pas)   # alt kenar paslı

    def sol():
        yarikli_kanat(xm1, -0.0015)
        L("Kart", -0.226, -0.15, YON - 0.003, YON - 0.0005, 1.756, 1.799, kagit)   # etiketlikte boş, sararmış kart
        prizma("Soyuk", leke(-0.02, 1.02, 0.012, 0.025), YON - 0.0008, YON + 0.0002, celik)

    def sag():
        yarikli_kanat(xm2, 0.0015)
        # Kaldırmalı kol: koyu çukur plakası ve önünde çelik kol.
        L("KolCukuru", 0.032, 0.068, YON - 0.0015, YON, 0.93, 1.07, koyu)
        kutu("Kol", (0.018, 0.012, 0.10), (0.05, YON - 0.008, 1.00), celik, 0.003)
        simit(0.009, 0.0025, (0.05, YON - 0.016, 0.935), celik, (16, 6))           # asma kilit halkası
        prizma("Soyuk", leke(0.05, 1.09, 0.03, 0.012), YON - 0.0008, YON + 0.0002, celik)   # kolun üstü aşınmış
        prizma("Soyuk", leke(0.05, 0.91, 0.025, 0.010), YON - 0.0008, YON + 0.0002, celik)

    k1 = parca("Kapi_1", (xm1, YON, Z0), sol)
    k2 = parca("Kapi_2", (xm2, YON, Z0), sag)
    return [k1, k2]


# ---------- 8. KAT: Yönetim bürosunun bej sac dolabı ----------

def gardirop_k8():
    bej = M("Gardirop_K8_Bej", (0.50, 0.44, 0.32), 0.45, 0.2)        # bej fırın boyası
    krom = M("Gardirop_K8_Krom", (0.60, 0.60, 0.60), 0.20, 1.0)      # kilit, kulp, etiketlik
    koyu = M("Gardirop_K8_Koyu", (0.03, 0.03, 0.03), 0.60)           # kulp yuvası, anahtar yarığı, raf delikleri
    ic = M("Gardirop_K8_Ic", (0.22, 0.20, 0.15), 0.75)               # içi: soluk, aşınmış
    kaide = M("Gardirop_K8_Kaide", (0.06, 0.06, 0.06), 0.70)
    kir = M("Gardirop_K8_Kir", (0.30, 0.27, 0.20), 0.80)             # alt kısımda ayakkabı ve el kiri
    ray = M("Gardirop_K8_Ray", (0.20, 0.19, 0.16), 0.60, 0.4)        # delikli raf rayları
    kagit = M("Gardirop_K8_Kagit", (0.55, 0.53, 0.46), 0.80)
    alu = M("Gardirop_K8_Demirbas", (0.45, 0.45, 0.43), 0.40, 0.8)   # demirbaş plakası
    lacivert = M("Gardirop_K8_Klasor_Lacivert", (0.06, 0.10, 0.22), 0.60)
    bordo = M("Gardirop_K8_Klasor_Bordo", (0.25, 0.04, 0.03), 0.60)

    W, D, H, ZA = 0.92, 0.55, 1.98, 0.06
    YON = -D                                     # kapılar yanların arasında, ön yüzleri yanlarla aynı düzlemde
    olc = kasa(W, D, H, ZA, bej, ic)
    olc["derin"] = D - KAL - KAL
    x0, x1 = -W / 2 + KAL, W / 2 - KAL
    L("UstBant", x0, x1, YON, YON + KAL, 1.942, H - DIS, bej, 0.002)      # kapıların üstünde 4 cm'lik bant
    L("AltBant", x0, x1, YON, YON + KAL, ZA, 0.08, bej, 0.002)
    L("Kaide", x0, x1, YON + 0.02, YON + 0.02 + KAL, 0, ZA, kaide)
    # Üst raf: 1,82 m'de; altında 1,70 m boşluk kalır. Ön kenarı aşağı kıvrık sac.
    L("Raf", x0, x1, YON + KAL + 0.01, -KAL, 1.805, 1.82, ic)
    L("RafDudak", x0, x1, YON + KAL + 0.01, YON + KAL + 0.018, 1.795, 1.82, ic)
    # Rafta yatık iki klasör (sırt etiketleri öne bakar) ve bir deste kâğıt.
    for i, mat in enumerate((lacivert, bordo)):
        z = 1.82 + 0.0275 + i * 0.055
        kutu("Klasor", (0.32, 0.29, 0.055), (-0.15, -0.20, z), mat, 0.004)
        kutu("SirtEtiketi", (0.10, 0.002, 0.03), (-0.15, -0.346, z), kagit)
    kutu("Kagitlar", (0.22, 0.31, 0.03), (0.22, -0.21, 1.835), kagit, 0.002)
    # İç yanlarda delikli raf rayları (raflar sökülmüş; yalnızca üst raf kalmış).
    for s in (-1, 1):
        xi = s * (W / 2 - KAL)
        for yr in (-0.48, -0.07):
            L("Ray", xi, xi - s * 0.0015, yr - 0.011, yr + 0.011, 0.10, 1.94, ray)
            for k in range(33):
                z = 0.15 + 0.05 * k
                L("RayDeligi", xi - s * 0.0015, xi - s * 0.002, yr - 0.004, yr + 0.004, z - 0.006, z + 0.006, koyu)
    on, yuz = YON - 0.0008, YON + 0.0002
    prizma("Kir", leke(-0.30, 0.03, 0.08, 0.012), YON + 0.0192, YON + 0.0202, kir)   # kaidede ayakkabı izi
    ic_olcu_yaz("Gardirop_K8", olc, "(rafın altı: 1,717)")

    Z0, Z1 = 0.082, 1.938
    xm1, xm2 = -0.44, 0.44

    def gomme_kulp(x0, x1):
        """Kapının birleşme kenarında dik, gömme kulp: koyu yuva ve içinde krom tutamak."""
        L("KulpYuvasi", x0, x1, YON - 0.0008, YON + 0.0002, 0.95, 1.25, koyu)
        L("Kulp", x0 + 0.003, x1 - 0.003, YON - 0.010, YON - 0.0008, 1.235, 1.245, krom, 0.002)
        L("Kulp", x0 + 0.003, x1 - 0.003, YON - 0.010, YON - 0.0008, 0.955, 0.965, krom, 0.002)
        L("Kulp", x0 + 0.003, x1 - 0.003, YON - 0.012, YON - 0.010, 0.955, 1.245, krom, 0.002)

    def sol():
        kanat(xm1, -0.0015, Z0, Z1, YON, bej, ic, 0.002)
        gomme_kulp(-0.034, -0.012)
        # Etiketlik (içinde boş kart) ve küçük alüminyum demirbaş plakası.
        for (a, b, c, d) in ((-0.30, -0.18, 1.72, 1.726), (-0.30, -0.18, 1.674, 1.68), (-0.30, -0.294, 1.68, 1.72), (-0.186, -0.18, 1.68, 1.72)):
            L("Etiketlik", a, b, YON - 0.004, YON, c, d, krom)
        L("Kart", -0.294, -0.186, YON - 0.003, YON - 0.0005, 1.68, 1.72, kagit)
        L("Demirbas", -0.27, -0.21, YON - 0.0015, YON, 1.80, 1.825, alu, 0.001)
        prizma("Kir", leke(-0.22, 0.25, 0.07, 0.05), on, yuz, kir)       # alt köşede ayakkabı izi
        prizma("Kir", leke(-0.05, 1.10, 0.025, 0.10, 16, 0.2), on, yuz, kir)   # kulbun yanında el kiri

    def sag():
        kanat(xm2, 0.0015, Z0, Z1, YON, bej, ic, 0.002)
        gomme_kulp(0.012, 0.034)
        # Kilit plakası: dikdörtgen krom plaka, önünde yuvarlak silindir ve anahtar yarığı.
        L("KilitPlakasi", 0.025, 0.055, YON - 0.003, YON, 1.30, 1.37, krom, 0.0015)
        prizma("Silindir", daire(0.04, 1.335, 0.011, 20), YON - 0.012, YON - 0.003, krom)
        L("AnahtarYarigi", 0.0388, 0.0412, YON - 0.0125, YON - 0.011, 1.328, 1.342, koyu)
        prizma("Kir", leke(0.05, 1.10, 0.025, 0.10, 16, 0.2), on, yuz, kir)
        prizma("Kir", leke(0.30, 0.18, 0.06, 0.04), on, yuz, kir)

    k1 = parca("Kapi_1", (xm1, YON, Z0), sol)
    k2 = parca("Kapi_2", (xm2, YON, Z0), sag)
    return [k1, k2]


# ---------- ÇALIŞTIRMA ----------

MODELLER = {"Gardirop_K1": gardirop_k1, "Gardirop_K2": gardirop_k2, "Gardirop_K3": gardirop_k3,
            "Gardirop_K4": gardirop_k4, "Gardirop_K7": gardirop_k7, "Gardirop_K8": gardirop_k8}


def ek_onizlemeler(kapilar):
    """Kapıları açıp iki görüntü daha alır: dolabın içi ve Kapi_1'in iç yüzü (yalnızca kontrol için)."""
    if "--onizleme" not in sys.argv:
        return
    yol = sys.argv[sys.argv.index("--onizleme") + 1]
    sahne = bpy.context.scene
    kam = sahne.camera
    hedef = kam.constraints[0].target                        # kameranın baktığı boş nesne
    kam.location, hedef.location = (0, -3.6, 1.0), (0, 0, 1.0)   # tam önden, kapılar kapalı
    sahne.render.filepath = yol.replace(".png", "_on.png")
    bpy.ops.render.render(write_still=True)
    kam.location, hedef.location = (2.9, -3.6, 2.98), (0, 0, 1.0)  # ilk görüntünün açısına dön
    for k in kapilar:
        orta_x = sum(v.co.x for v in k.data.vertices) / len(k.data.vertices)   # kapı pivotun hangi yanında?
        k.rotation_euler.z = math.radians(-100 if orta_x > 0 else 100)         # öne (-y) doğru 100° aç
    bpy.ops.object.light_add(type="POINT", location=(0.2, -1.3, 1.5))           # içeriyi aydınlatan ek ışık
    bpy.context.active_object.data.energy = 150
    sahne.render.filepath = yol.replace(".png", "_acik.png")
    bpy.ops.render.render(write_still=True)
    kam.location, hedef.location = (0, -3.4, 1.15), (0, -0.25, 1.0)
    sahne.render.filepath = yol.replace(".png", "_ic.png")
    bpy.ops.render.render(write_still=True)
    for k in kapilar[1:]:                                    # Kapi_2 kapanır, yoksa kameranın önünü keser
        k.rotation_euler.z = 0
    bpy.context.view_layer.update()
    k1 = kapilar[0]
    ws = [k1.matrix_world @ v.co for v in k1.data.vertices]
    orta = sum(ws, Vector()) / len(ws)                       # açık kapının dünyadaki ortası
    kam.location = orta + Vector((2.2, -0.4, 0.1))     # iç yüz açılınca +x yönüne bakar; kamera o tarafta
    hedef.location = orta
    sahne.render.filepath = yol.replace(".png", "_kapi.png")
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    ad = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Gardirop_K1"
    if ad not in MODELLER:
        raise SystemExit("bilinmeyen gardırop: " + ad)
    ortak.sahneyi_temizle()
    # Önizlemede renkler "Standard" dönüşümle gösterilir (varsayılan AgX koyu içi soluk gösteriyordu).
    bpy.context.scene.view_settings.view_transform = "Standard"
    kapilar = MODELLER[ad]()                                 # gövde parçaları p'de, kapılar ayrı nesneler
    govde = ortak.birlestir(p, "Govde")                      # gövdenin bütün levhaları tek nesne
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))                      # pivot: arka-alt orta (duvarın dibi)
    for k in kapilar:
        print("MENTESE", ad, k.name, "blender", tuple(round(c, 4) for c in k.location),
              "unity", (round(-k.location.x, 4), round(k.location.z, 4), round(-k.location.y, 4)))
    ortak.calistir_ve_bitir(ad, [govde] + kapilar, 2.0, 3.6)
    ek_onizlemeler(kapilar)
