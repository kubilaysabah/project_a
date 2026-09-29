# kat3_esyalar.py
# 3. kat (#83): apartmanın 1 numaralı dairesi, Kerem'in ailesi (anne Ayla, baba Murat, 7 yaşındaki Kerem ve
# bir bebek). Kalabalık, renkli, yaşanmış bir ev; gece yarısı terk edilmiş gibi. Kerem geceleri uyanıp
# "dokuzuncu kattaki adamı" çiziyor. Katlarda aynı model yok: hepsi bu kata özgü (FBX adları _K3 ile biter).
#   blender -b --factory-startup --python kat3_esyalar.py -- <ad> [--onizleme dosya.png]
#
# Bu dosya iki iş yapar: (1) 3. katın bütün eşya betiklerinin paylaştığı yardımcılar (kat3_esyalar_2.py
# bunları "import kat3_esyalar" ile kullanır), (2) antre, hol, salon ve mutfak eşyaları:
# ANTRE
#   askilik        Askilik_K3          : çam askı çıtası; annenin bej paltosu, babanın koyu paltosu, Kerem'in
#                                        sarı yağmurluğu ve asılı bir şemsiye (duvar eşyası).
#   ayakkabi       Ayakkabi_Yigini_K3  : kapı dibinde aile ayakkabıları: erkek, kadın, çocuk spor ayakkabısı, çizme.
#   canta          Okul_Cantasi_K3     : sırtüstü yatık 90'lar okul çantası, kapağı açık; defter ve kalem kutusu kaymış.
#   bebek_arabasi  Bebek_Arabasi_K3    : duvara dayalı, katlanmış baston tipi bebek arabası.
#   terlik         Terlikler_K3        : iki yetişkin çifti ve pembe, kulaklı bir çocuk terliği.
# HOL
#   cizimler       Cocuk_Cizimleri_K3  : duvara bantlanmış altı boya kalemi resmi (biri simsiyah karalama,
#                                        birinde pencerede uzun bir adam) (duvar eşyası).
#   oyuncak        Dagilmis_Oyuncaklar_K3 : yere dağılmış oyuncak araba, tahta bloklar, top, plastik kamyon.
#   yolluk         Yolluk_K3           : bordo-yeşil geometrik desenli hol yolluğu.
# SALON
#   tv             Tuplu_TV_K3         : gümüş-gri 21" tüplü TV, üstünde dantel ve tavşan kulağı anten.
#   boyama         Boyama_Sehpasi_K3   : alçak sehpa; açık boyama kitabı, dağınık boya kalemleri, yarım süt bardağı.
#   park           Oyun_Parki_K3       : ahşap parmaklıklı bebek oyun parkı; içinde battaniye ve çıngırak, bebek yok.
#   hali           Hali_K3             : kırmızı zeminli, göbek motifli salon halısı.
#   perde          Perde_K3            : iki yana bağlanmış sarı pamuklu perde, ortada tül (pivot çubuk yüksekliği).
#   raf            Duvar_Rafi_K3       : duvar rafı; porselen biblolar ve küçük vazo (duvar eşyası).
# MUTFAK
#   mama           Mama_Sandalyesi_K3  : beyaz-pembe plastik ve metal, tablalı mama sandalyesi.
#   masa           Masa_K3             : mavi muşamba örtülü yuvarlak masa; meyve kâsesi ve şekerlik.
#   sandalye       Sandalye_K3         : mavi boyalı ahşap mutfak sandalyesi.
#   buzdolabi      Buzdolabi_K3        : krem, üstü dondurucu iki kapılı buzdolabı; kapıda mıknatıslı resimler (tek parça).
#   ocak           Ocak_K3             : beyaz dört gözlü fırınlı ocak; süt tenceresi ve iki katlı çaydanlık.
#   camasir        Camasir_Makinesi_K3 : önden yüklemeli beyaz çamaşır makinesi; üstünde dolu çamaşır sepeti.
#   biberon        Biberon_Takimi_K3   : iki biberon ve bir mama tenekesi (tezgâh üstü).
# Ön yüz -y (Unity'de +Z). Pivot: yer ve masa üstü eşyalarında tabanın ortası; duvar eşyalarında arka yüzün
# ortası; duvara dayalı eşyada duvarın dibi (arka y = 0, taban z = 0); perdede çubuk yüksekliğinde arka kenar.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import bmesh
from mathutils import Vector, Matrix

# P: o an kurulan eşyanın parça listesi. Yardımcılar ürettikleri parçayı buraya ekler; bitir() hepsini
# tek nesnede birleştirir. İkinci betik de aynı listeyi (k3.P) kullanır.
P = []
BILGI = []                      # rapora girecek ek satırlar (üst yüz yüksekliği gibi)
rnd = random.Random(1994)

# ---------- MALZEMELER ----------
# Renkler Blender'da doğrusal (linear) değerdir: Unity'de daha açık görünür, bu yüzden koyu tutulur.
RENK = {
    "kagit": ((0.80, 0.79, 0.74), 0.9, 0.0),
    "bant": ((0.62, 0.56, 0.40), 0.35, 0.0),
    "boya_siyah": ((0.012, 0.012, 0.015), 0.85, 0.0),
    "boya_kirmizi": ((0.45, 0.03, 0.02), 0.85, 0.0),
    "boya_mavi": ((0.03, 0.10, 0.45), 0.85, 0.0),
    "boya_sari": ((0.80, 0.55, 0.03), 0.85, 0.0),
    "boya_yesil": ((0.04, 0.30, 0.05), 0.85, 0.0),
    "boya_kahve": ((0.16, 0.07, 0.02), 0.85, 0.0),
    "boya_turuncu": ((0.70, 0.22, 0.02), 0.85, 0.0),
    "boya_mor": ((0.20, 0.04, 0.30), 0.85, 0.0),
    "boya_pembe": ((0.75, 0.25, 0.35), 0.85, 0.0),
    "boya_gri": ((0.25, 0.25, 0.26), 0.85, 0.0),
    "boya_lacivert": ((0.02, 0.03, 0.15), 0.85, 0.0),
    "boya_acikmavi": ((0.25, 0.50, 0.80), 0.85, 0.0),
    "pirinc": ((0.55, 0.40, 0.13), 0.3, 0.9),
    "krom": ((0.62, 0.62, 0.60), 0.2, 1.0),
    "celik": ((0.60, 0.60, 0.58), 0.35, 0.8),
    "ic_koyu": ((0.02, 0.015, 0.012), 0.9, 0.0),
    "siyah_plastik": ((0.02, 0.02, 0.02), 0.4, 0.0),
    "beyaz_plastik": ((0.80, 0.80, 0.78), 0.35, 0.0),
    "pembe_plastik": ((0.70, 0.28, 0.38), 0.35, 0.0),
    "beyaz_emaye": ((0.80, 0.80, 0.77), 0.25, 0.0),
    "krem_emaye": ((0.72, 0.66, 0.50), 0.3, 0.0),
    "porselen": ((0.85, 0.84, 0.80), 0.15, 0.0),
    "cam": ((0.55, 0.62, 0.62), 0.05, 0.0),
    "cam_agaci": ((0.30, 0.19, 0.07), 0.55, 0.0),
    "akcaagac": ((0.34, 0.20, 0.085), 0.3, 0.0),
    "akcaagac_ic": ((0.40, 0.26, 0.12), 0.6, 0.0),
    "ahsap_orta": ((0.12, 0.06, 0.025), 0.5, 0.0),
    "bal_ahsap": ((0.22, 0.12, 0.045), 0.45, 0.0),
    "mavi_boya": ((0.05, 0.14, 0.36), 0.45, 0.0),
    "cocuk_mavi": ((0.06, 0.18, 0.45), 0.4, 0.0),
    "sut": ((0.85, 0.84, 0.80), 0.3, 0.0),
    "kumas_beyaz": ((0.78, 0.77, 0.72), 0.95, 0.0),
}


def mt(ad):
    """Tablodaki bir malzemeyi verir. Adı "K3_" ile başlar ki başka katların malzemeleriyle karışmasın."""
    renk, puruz, metal = RENK[ad]
    return ortak.malzeme("K3_" + ad, renk, puruz, metal)


def M(ad, renk, puruz=0.6, metal=0.0):
    """Tabloda olmayan, tek eşyaya özgü malzeme."""
    return ortak.malzeme("K3_" + ad, renk, puruz, metal)


# ---------- TEMEL ŞEKİLLER (hepsi parçayı P'ye ekler) ----------

def ekle(n):
    P.append(n)
    return n


def sec(n):
    """Blender komutları SEÇİLİ ve ETKİN nesneye uygulanır; önce yalnız bu nesneyi seç."""
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n


def kutu(olcu, konum, mat, pah=0.0):
    """olcu: (x, y, z) metre; konum: kutunun merkezi."""
    return ekle(ortak.kutu("Parca", olcu, konum, mat, pah))


def sil(r, boy, konum, mat, kose=16, pah=0.0):
    """Dik (z ekseninde) silindir; konum merkezi."""
    return ekle(ortak.silindir("Sil", r, boy, konum, mat, kose, pah))


def don(n, rx=0.0, ry=0.0, rz=0.0):
    """
    Parçayı KENDİ MERKEZİ etrafında döndürür ve dönüşü meshe işler (birleştirince bozulmasın).
    DİKKAT: ortak.kutu ve kure "transform_apply" ile konumu da meshe işler; pivotları dünyanın merkezinde
    kalır. Doğrudan döndürseydik parça dünya merkezi etrafında savrulurdu (bebek arabasının bohçası 13 cm
    duvarın içine kaçtı). Bu yüzden önce pivotu parçanın sınır kutusunun ortasına alıyoruz.
    """
    vs = n.data.vertices
    c = Vector(((min(v.co.x for v in vs) + max(v.co.x for v in vs)) / 2,
                (min(v.co.y for v in vs) + max(v.co.y for v in vs)) / 2,
                (min(v.co.z for v in vs) + max(v.co.z for v in vs)) / 2))
    n.data.transform(Matrix.Translation(-c))     # mesh'i kendi merkezine çek
    n.location = n.location + c                  # nesneyi aynı miktar geri koy: görünüş değişmez
    sec(n)
    n.rotation_euler = (rx, ry, rz)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    return n


def olcekle(n, s):
    sec(n)
    n.scale = s
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return n


def yatik_sil(r, boy, konum, eksen, mat, kose=16):
    """Yatık silindir: eksen "x" ya da "y" boyunca uzanır."""
    rot = (0, math.pi / 2, 0) if eksen == "x" else (math.pi / 2, 0, 0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return ekle(n)


def koni(r1, r2, boy, konum, mat, kose=16, rot=(0, 0, 0)):
    """Kesik koni: r1 alt (−z), r2 üst (+z) yarıçap."""
    bpy.ops.mesh.primitive_cone_add(vertices=kose, radius1=r1, radius2=r2, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return ekle(n)


def kure(r, konum, mat, olcek=(1, 1, 1), seg=14, halka=8):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=halka)
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    return ekle(n)


def elips(en, boy, yuk, konum, mat, kose=24):
    """Yatay elips disk (terlik tabanı, kâse dibi): en x boy, yuk kalınlık."""
    n = sil(1.0, yuk, konum, mat, kose)
    return olcekle(n, (en / 2, boy / 2, 1))


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk (boru, anten, sap)."""
    d = Vector(b) - Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length, location=tuple((Vector(a) + Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return ekle(n)


def dik_disk(r, kal, konum, mat, kose=20):
    """Yüzü -y'ye bakan disk (düğme, kapak camı): ekseni y boyunca."""
    return yatik_sil(r, kal, konum, "y", mat, kose)


def yay_cubuk(merkez, r, a0, a1, kal, mat, n=8, duzlem="xz"):
    """Yay biçimli boru (şemsiye sapı, kulp): merkez etrafında a0'dan a1'e (radyan) n parça çubuk."""
    cx, cy, cz = merkez
    def nokta(a):
        if duzlem == "xz":
            return (cx + r * math.cos(a), cy, cz + r * math.sin(a))
        return (cx, cy + r * math.cos(a), cz + r * math.sin(a))      # yz düzlemi
    for i in range(n):
        cubuk(nokta(a0 + (a1 - a0) * i / n), nokta(a0 + (a1 - a0) * (i + 1) / n), kal, mat, 6)


def prizma(noktalar, duzlem, d0, d1, mat):
    """
    2B bir çokgeni kalınlık vererek katı yapar (üçgen çatı, yıldız, palto silueti).
    duzlem "xz": noktalar (x, z), kalınlık y'de d0..d1; "xy": (x, y), kalınlık z'de; "yz": (y, z), kalınlık x'te.
    Yüzler merkezden yelpaze gibi üçgenlenir: yıldız gibi girintili şekiller de doğru kapanır.
    """
    def v3(a, b, d):
        return (a, d, b) if duzlem == "xz" else ((a, b, d) if duzlem == "xy" else (d, a, b))
    me = bpy.data.meshes.new("Prizma")
    bm = bmesh.new()
    k = len(noktalar)
    ca = sum(q[0] for q in noktalar) / k
    cb = sum(q[1] for q in noktalar) / k
    on = [bm.verts.new(v3(a, b, d0)) for a, b in noktalar]
    ar = [bm.verts.new(v3(a, b, d1)) for a, b in noktalar]
    m0, m1 = bm.verts.new(v3(ca, cb, d0)), bm.verts.new(v3(ca, cb, d1))
    for i in range(k):
        j = (i + 1) % k
        bm.faces.new((m0, on[i], on[j]))
        bm.faces.new((m1, ar[j], ar[i]))
        bm.faces.new((on[i], ar[i], ar[j], on[j]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)    # bütün yüzler dışa baksın
    bm.to_mesh(me)
    bm.free()
    n = bpy.data.objects.new("Prizma", me)
    bpy.context.collection.objects.link(n)
    ortak.ata(n, mat)
    return ekle(n)


def yildiz(cu, cv, r, kose=5, ic=0.45, aci=math.pi / 2):
    """Yıldızın 2B köşe noktaları (prizma için)."""
    return [(cu + (r if i % 2 == 0 else r * ic) * math.cos(aci + i * math.pi / kose),
             cv + (r if i % 2 == 0 else r * ic) * math.sin(aci + i * math.pi / kose)) for i in range(kose * 2)]


def grupla(bas, konum=(0, 0, 0), rot=(0, 0, 0), olcek=(1, 1, 1)):
    """
    P[bas:] parçalarını tek nesne yapar ve bu alt montajı döndürüp yerine taşır. Bir ayakkabıyı,
    kâğıdı, oyuncak arabayı önce dünyanın merkezinde düz kurup sonra çevirip yerleştirmenin yolu.
    Dönüş dünya merkezi etrafında yapılır, sonra konuma taşınır.
    """
    parca = P[bas:]
    del P[bas:]
    n = parca[0] if len(parca) == 1 else ortak.birlestir(parca, "Grup")
    ortak.pivotu_tasi(n, (0, 0, 0))
    sec(n)
    n.rotation_euler = rot
    n.scale = olcek
    n.location = konum
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return ekle(n)


# ---------- KÂĞIT VE ÇOCUK ÇİZİMLERİ ----------
# Çizimler kâğıdın üstünde çok ince katmanlardır. Aynı kod iki düzlemde çalışır:
#   "duvar": kâğıt xz düzleminde, arka yüzü y = 0'da, çizim -y yönüne (öne) kabarır;
#   "yer"  : kâğıt xy düzleminde, alt yüzü z = 0'da, çizim +z yönüne kabarır.
# (u, v) kâğıdın kendi eksenleri: u sağa, v yukarı (yerde: v kâğıdın "üst" kenarına doğru).
KAGIT_K = 0.0008                # kâğıt kalınlığı
KAT_K = 0.0004                  # her çizim katmanının kalınlığı (üst üste binenler z-kavgası yapmasın)


def _derin(kat):
    return KAGIT_K + kat * KAT_K + KAT_K / 2


def c_kutu(u, v, w, h, mat, kat, duz, aci=0.0):
    """Kâğıtta dikdörtgen leke; aci: kâğıt düzleminde saat yönünün tersine dönüş (radyan)."""
    d = _derin(kat)
    if duz == "duvar":
        n = kutu((w, KAT_K, h), (u, -d, v), mat)
        if aci:
            don(n, ry=-aci)      # y ekseninde eksi açı, önden bakınca saat yönünün tersi demek
    else:
        n = kutu((w, h, KAT_K), (u, v, d), mat)
        if aci:
            don(n, rz=aci)
    return n


def c_cizgi(u0, v0, u1, v1, kal, mat, kat, duz):
    """Kâğıtta (u0, v0)'dan (u1, v1)'e boya kalemi çizgisi."""
    L = math.hypot(u1 - u0, v1 - v0)
    if L < 1e-4:
        return None
    return c_kutu((u0 + u1) / 2, (v0 + v1) / 2, L, kal, mat, kat, duz, math.atan2(v1 - v0, u1 - u0))


def c_disk(u, v, r, mat, kat, duz, kose=16, olcek=(1.0, 1.0)):
    d = _derin(kat)
    if duz == "duvar":
        bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=KAT_K, location=(u, -d, v), rotation=(math.pi / 2, 0, 0))
        n = bpy.context.active_object
        n.scale = (olcek[0], olcek[1], 1)          # yerel y, döndükten sonra dünyada z (yukarı) olur
    else:
        bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=KAT_K, location=(u, v, d))
        n = bpy.context.active_object
        n.scale = (olcek[0], olcek[1], 1)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    return ekle(n)


def c_poly(noktalar, mat, kat, duz):
    d0 = KAGIT_K + kat * KAT_K
    if duz == "duvar":
        return prizma(noktalar, "xz", -d0, -(d0 + KAT_K), mat)
    return prizma(noktalar, "xy", d0, d0 + KAT_K, mat)


def c_halka(u, v, r, kal, mat, kat, duz, n=12, sx=1.0, sy=1.0):
    """Boyama kitabındaki gibi çember kontur."""
    for i in range(n):
        a0, a1 = 2 * math.pi * i / n, 2 * math.pi * (i + 1) / n
        c_cizgi(u + r * sx * math.cos(a0), v + r * sy * math.sin(a0), u + r * sx * math.cos(a1), v + r * sy * math.sin(a1), kal, mat, kat, duz)


def cop_adam(u, v0, boy, mat, duz, kat=1, etek=None, kal=0.004):
    """Çocuk çizimi çöp adam: yuvarlak kafa, çizgi gövde, kollar, bacaklar; etek verilirse üçgen elbise."""
    br = boy * 0.13
    ust = v0 + boy - 2 * br
    kalca = v0 + boy * 0.40
    c_disk(u, v0 + boy - br, br, mat, kat, duz)
    c_cizgi(u, ust, u, kalca, kal, mat, kat, duz)
    kv = ust - boy * 0.06
    c_cizgi(u, kv, u - boy * 0.26, kv - boy * 0.14, kal, mat, kat, duz)
    c_cizgi(u, kv, u + boy * 0.26, kv - boy * 0.10, kal, mat, kat, duz)
    c_cizgi(u, kalca, u - boy * 0.14, v0, kal, mat, kat, duz)
    c_cizgi(u, kalca, u + boy * 0.14, v0, kal, mat, kat, duz)
    if etek is not None:
        c_poly([(u - boy * 0.18, kalca - boy * 0.06), (u + boy * 0.18, kalca - boy * 0.06), (u, ust)], etek, kat + 1, duz)


def uzun_adam(u, v0, boy, duz, r, kat=1):
    """Kerem'in "dokuzuncu kattaki adamı": upuzun, ince, kara bir figür; yere kadar sarkan kollar, kırmızı gözler."""
    sy, kr = mt("boya_siyah"), mt("boya_kirmizi")
    g = boy * r.uniform(0.09, 0.12)
    c_poly([(u - g * 0.5, v0 + boy * 0.35), (u + g * 0.5, v0 + boy * 0.35), (u + g * 0.3, v0 + boy * 0.87), (u - g * 0.3, v0 + boy * 0.87)], sy, kat, duz)
    c_disk(u, v0 + boy * 0.925, boy * 0.055, sy, kat, duz, olcek=(0.8, 1.15))
    for s in (-1, 1):
        c_cizgi(u + s * g * 0.3, v0 + boy * 0.84, u + s * g * 1.3, v0 + boy * 0.22, 0.0045, sy, kat, duz)
        for f in range(3):                          # uzun parmaklar
            c_cizgi(u + s * g * 1.3, v0 + boy * 0.22, u + s * g * (1.2 + 0.15 * f), v0 + boy * 0.16, 0.002, sy, kat, duz)
        c_cizgi(u + s * g * 0.2, v0 + boy * 0.36, u + s * g * 0.35, v0, 0.005, sy, kat, duz)
    for s in (-1, 1):
        c_disk(u + s * boy * 0.018, v0 + boy * 0.935, boy * 0.011, kr, kat + 1, duz, kose=8)
    for i in range(6):                              # çevresine bastıra bastıra çekilmiş çizgiler
        a = r.uniform(0, math.pi)
        uu, vv = u + r.uniform(-0.05, 0.05), v0 + boy * r.uniform(0.2, 0.9)
        c_cizgi(uu - 0.03 * math.cos(a), vv - 0.03 * math.sin(a), uu + 0.03 * math.cos(a), vv + 0.03 * math.sin(a), 0.003, sy, kat, duz)


def kagit(tur, duz, tohum=0, boyut=None, zemin="kagit"):
    """
    A4 kâğıt ve üstündeki çocuk çizimini dünyanın merkezinde kurar; P'deki başlangıç sırasını döner
    (sonra grupla ile döndürülüp yerine konur). tur: ev, aile, gunes, karalama, pencere, araba,
    uzun_adam, dokuz_kat, bos, boyama_kedi, boyama_cicek.
    """
    bas = len(P)
    r = random.Random(tohum)
    yatay = tur in ("ev", "aile", "araba")
    W, H = boyut or ((0.297, 0.21) if yatay else (0.21, 0.297))
    if duz == "duvar":
        kutu((W, KAGIT_K, H), (0, -KAGIT_K / 2, 0), mt(zemin))
    else:
        kutu((W, H, KAGIT_K), (0, 0, KAGIT_K / 2), mt(zemin))
    b = lambda ad: mt("boya_" + ad)
    if tur == "ev":
        c_kutu(0, -H / 2 + 0.02, W - 0.02, 0.022, b("yesil"), 0, duz)
        c_kutu(-0.04, -0.03, 0.10, 0.085, b("kirmizi"), 0, duz)
        c_poly([(-0.10, 0.012), (0.02, 0.012), (-0.04, 0.07)], b("kahve"), 0, duz)
        c_kutu(-0.04, -0.055, 0.024, 0.04, b("kahve"), 1, duz)
        for x in (-0.072, -0.008):
            c_kutu(x, -0.018, 0.022, 0.022, b("sari"), 1, duz)
        c_disk(0.10, 0.06, 0.024, b("sari"), 0, duz)
        for i in range(8):
            a = i * math.pi / 4
            c_cizgi(0.10 + 0.03 * math.cos(a), 0.06 + 0.03 * math.sin(a), 0.10 + 0.045 * math.cos(a), 0.06 + 0.045 * math.sin(a), 0.003, b("sari"), 0, duz)
        c_kutu(0.075, -0.05, 0.012, 0.05, b("kahve"), 0, duz)
        c_disk(0.075, -0.012, 0.028, b("yesil"), 1, duz)
        for x in (-0.02, 0.0):
            c_disk(x, 0.075, 0.015, b("acikmavi"), 0, duz)
    elif tur == "aile":
        c_kutu(0, -H / 2 + 0.018, W - 0.02, 0.016, b("yesil"), 0, duz)
        v0 = -H / 2 + 0.026
        cop_adam(-0.09, v0, 0.13, b("mavi"), duz)
        cop_adam(-0.03, v0, 0.12, b("kirmizi"), duz, etek=b("pembe"))
        cop_adam(0.03, v0, 0.08, b("yesil"), duz)
        cop_adam(0.07, v0, 0.05, b("turuncu"), duz)
        c_disk(0.11, 0.07, 0.022, b("sari"), 0, duz)
        c_disk(-0.03, v0 + 0.12 - 0.0156, 0.0156, b("kahve"), 2, duz, olcek=(1.2, 0.6))   # annenin saçı
    elif tur == "gunes":
        c_disk(0.0, 0.08, 0.04, b("sari"), 0, duz)
        for i in range(12):
            a = i * math.pi / 6
            c_cizgi(0.0 + 0.048 * math.cos(a), 0.08 + 0.048 * math.sin(a), 0.07 * math.cos(a), 0.08 + 0.07 * math.sin(a), 0.004, b("turuncu"), 0, duz)
        for i, (x, rk) in enumerate(((-0.06, "kirmizi"), (0.0, "mor"), (0.06, "pembe"))):
            c_cizgi(x, -0.13, x, -0.04, 0.004, b("yesil"), 0, duz)
            for k in range(6):
                a = k * math.pi / 3
                c_disk(x + 0.014 * math.cos(a), -0.035 + 0.014 * math.sin(a), 0.009, b(rk), 1, duz, kose=10)
            c_disk(x, -0.035, 0.008, b("sari"), 2, duz, kose=10)
        c_kutu(0, -H / 2 + 0.012, W - 0.02, 0.012, b("yesil"), 0, duz)
    elif tur == "karalama":
        m = 0.014
        for i in range(55):
            u0, v0 = r.uniform(-W / 2 + m, W / 2 - m), r.uniform(-H / 2 + m, H / 2 - m)
            a, L = r.uniform(0, math.pi), r.uniform(0.08, 0.25)
            u1 = min(max(u0 + L * math.cos(a), -W / 2 + m), W / 2 - m)
            v1 = min(max(v0 + L * math.sin(a), -H / 2 + m), H / 2 - m)
            c_cizgi(u0, v0, u1, v1, r.uniform(0.012, 0.022), b("siyah"), i % 4, duz)
        for s in (-1, 1):                                        # karanlığın içinde iki kırmızı nokta
            c_disk(s * 0.012, 0.06, 0.004, b("kirmizi"), 5, duz, kose=8)
    elif tur in ("pencere", "dokuz_kat"):
        c_kutu(0, 0.02, W - 0.02, H - 0.06, b("lacivert"), 0, duz)
        c_disk(0.065, 0.115, 0.016, b("sari"), 1, duz)
        c_kutu(0, -H / 2 + 0.015, W - 0.02, 0.02, b("yesil"), 0, duz)
        if tur == "pencere":
            bw, bh, sat, sut = 0.12, 0.21, 5, 3
        else:
            bw, bh, sat, sut = 0.08, 0.25, 9, 2
        tb = -H / 2 + 0.025
        c_kutu(0, tb + bh / 2, bw, bh, b("gri"), 1, duz)
        for i in range(sat):
            for j in range(sut):
                u = -bw / 2 + bw * (j + 0.5) / sut
                v = tb + 0.02 + (bh - 0.03) * (i + 0.5) / sat
                ust = i == sat - 1 and j == sut // 2
                if ust:                                          # en üst pencere yanıyor, içinde uzun adam
                    c_kutu(u, v, 0.024, 0.024, b("sari"), 2, duz)
                    c_kutu(u, v - 0.002, 0.004, 0.017, b("siyah"), 3, duz)
                    c_disk(u, v + 0.008, 0.0028, b("siyah"), 3, duz, kose=8)
                else:
                    c_kutu(u, v, 0.016, 0.014, b("sari") if r.random() < 0.3 else b("lacivert"), 2, duz)
        cop_adam(bw / 2 + 0.02, tb, 0.03, b("kirmizi"), duz, kat=2, kal=0.0025)   # aşağıda küçücük Kerem
    elif tur == "araba":
        c_kutu(0, -H / 2 + 0.035, W - 0.02, 0.03, b("gri"), 0, duz)
        c_kutu(0, -0.03, 0.18, 0.045, b("kirmizi"), 1, duz)
        c_poly([(-0.06, -0.008), (0.05, -0.008), (0.03, 0.03), (-0.04, 0.03)], b("kirmizi"), 1, duz)
        c_kutu(-0.005, 0.008, 0.07, 0.022, b("acikmavi"), 2, duz)
        for x in (-0.055, 0.055):
            c_disk(x, -0.055, 0.02, b("siyah"), 2, duz)
        c_disk(-0.10, 0.07, 0.02, b("sari"), 0, duz)
    elif tur == "uzun_adam":
        uzun_adam(r.uniform(-0.02, 0.03), -H / 2 + 0.02, r.uniform(0.21, 0.25), duz, r)
        if r.random() < 0.6:                                     # yanında küçük bir ev, ölçeği versin
            c_kutu(-0.07, -H / 2 + 0.035, 0.04, 0.03, b("kahve"), 1, duz)
            c_poly([(-0.095, -H / 2 + 0.05), (-0.045, -H / 2 + 0.05), (-0.07, -H / 2 + 0.075)], b("kirmizi"), 1, duz)
    elif tur == "bos":
        for i in range(5):
            u0, v0 = r.uniform(-0.07, 0.07), r.uniform(-0.1, 0.1)
            c_cizgi(u0, v0, u0 + r.uniform(-0.05, 0.05), v0 + r.uniform(-0.05, 0.05), 0.004, b(r.choice(("mavi", "yesil", "siyah"))), i % 2, duz)
    elif tur == "boyama_kedi":
        sy = b("siyah")
        c_disk(0.0, 0.05, 0.042, b("turuncu"), 0, duz)          # yarısı boyanmış kafa
        c_halka(0.0, 0.05, 0.045, 0.002, sy, 1, duz)
        c_halka(0.0, -0.05, 0.06, 0.002, sy, 1, duz, sx=0.9, sy=1.1)
        c_kutu(-0.02, -0.06, 0.05, 0.07, b("turuncu"), 0, duz, 0.3)
        for s in (-1, 1):
            c_cizgi(s * 0.02, 0.085, s * 0.035, 0.11, 0.002, sy, 1, duz)
            c_cizgi(s * 0.035, 0.11, s * 0.042, 0.075, 0.002, sy, 1, duz)
    elif tur == "boyama_cicek":
        sy = b("siyah")
        for k in range(6):
            a = k * math.pi / 3
            c_halka(0.03 * math.cos(a), 0.05 + 0.03 * math.sin(a), 0.02, 0.002, sy, 1, duz, n=10)
            if k < 3:
                c_disk(0.03 * math.cos(a), 0.05 + 0.03 * math.sin(a), 0.018, b("mor"), 0, duz, kose=10)
        c_cizgi(0, 0.02, 0, -0.11, 0.002, sy, 1, duz)
        c_kutu(0.0, -0.05, 0.004, 0.10, b("yesil"), 0, duz)
    return bas


def boya_kalemi(x, y, z, aci, mat, boy=0.09, r=0.0045, dik=False):
    """Boya kalemi: yatık (yerde, tabla üstünde) ya da dik (kutuda). Uçu sivri koni."""
    bas = len(P)
    if dik:
        sil(r, boy, (0, 0, boy / 2), mat, 8)
        koni(r, 0.0012, 0.012, (0, 0, boy + 0.006), mat, 8)
        return grupla(bas, (x, y, z))
    yatik_sil(r, boy, (0, 0, r), "x", mat, 8)
    koni(r, 0.0012, 0.012, (boy / 2 + 0.006, 0, r), mat, 8, rot=(0, math.pi / 2, 0))
    return grupla(bas, (x, y, z), (0, 0, aci))


# ---------- BİTİRİŞ ----------

def pivotla(n, tur):
    """
    Mesh'i kaydırarak pivotu istenen noktaya getirir (nesnenin kendisi dünya merkezinde kalır):
    yer/masa: tabanın ortası; duvar: arka yüzün ortası; dayali: arka alt kenarın ortası;
    perde: arka kenar, yükseklik çubuğun ekseni (çubuk z = 0'da kurulur, z kaydırılmaz).
    """
    vs = [v.co for v in n.data.vertices]
    x0, x1 = min(v.x for v in vs), max(v.x for v in vs)
    y0, y1 = min(v.y for v in vs), max(v.y for v in vs)
    z0, z1 = min(v.z for v in vs), max(v.z for v in vs)
    cx, cy, cz = (x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2
    ofs = {"yer": (cx, cy, z0), "duvar": (cx, y1, cz), "dayali": (cx, y1, z0), "perde": (cx, y1, 0.0)}[tur]
    n.data.transform(Matrix.Translation(-Vector(ofs)))
    n.data.update()
    print("PIVOT", n.name, tur, "kaydirma", tuple(round(c, 3) for c in ofs))
    return ofs


def bitir(ad, tur="yer", h=None, uzak=None):
    """Parçaları birleştir, pivotu ayarla, FBX + .blend yaz, önizleme al."""
    n = ortak.birlestir(P, ad)
    ortak.pivotu_tasi(n, (0, 0, 0))
    ofs = pivotla(n, tur)
    ortak.yumusat(n, 35)
    d = n.dimensions
    kaldir = 0.0
    if tur == "duvar":
        kaldir = d.z / 2 + 0.05                  # önizlemede zeminin üstüne kaldır
    elif tur == "perde":
        kaldir = d.z + 0.05
    if h is None:
        h = max(d.z + (0.1 if tur in ("duvar", "perde") else 0.0), 0.15)
    if uzak is None:
        uzak = 1.05 * max(d.x, d.y, d.z) + 0.25
    for b in BILGI:
        print("BILGI", ad, b)
    ortak.calistir_ve_bitir(ad, [n], h, uzak, onizlemede_kaldir=kaldir)
    return ofs


def bitir_acilir(ad, govde_parcalari, ayri, h, uzak):
    govde = ortak.birlestir(govde_parcalari, "Govde")
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    for n in ayri:
        print("PARCA_PIVOT", n.name, tuple(round(c, 4) for c in n.location))
    for b in BILGI:
        print("BILGI", ad, b)
    ortak.calistir_ve_bitir(ad, [govde] + ayri, h, uzak)


# =====================================================================================
# EŞYALAR
# =====================================================================================

def palto(x, ust, omuz, alt, boy, y0, y1, mat, ust_z=0.02, kapson=False):
    """Kancadan asılı palto: omuzları kancada toplanmış, aşağı genişleyen gövde, iki yanda sarkan kollar."""
    prizma([(x - ust / 2, ust_z), (x + ust / 2, ust_z), (x + omuz / 2, ust_z - 0.16), (x + alt / 2, ust_z - boy),
            (x - alt / 2, ust_z - boy), (x - omuz / 2, ust_z - 0.16)], "xz", y0, y1, mat)
    for s in (-1, 1):                                            # kollar: yanlarda, öne doğru sarkık
        kol = kutu((0.085, 0.075, boy * 0.62), (x + s * (omuz / 2 - 0.02), y1 + 0.02, ust_z - 0.16 - boy * 0.31), mat, 0.025)
        don(kol, ry=s * 0.05)
    kutu((ust + 0.06, 0.035, 0.09), (x, y1 + 0.012, ust_z - 0.05), mat, 0.02)      # yaka
    for i in range(4 if boy > 0.7 else 3):                       # düğmeler
        dik_disk(0.009, 0.006, (x, y1 - 0.001, ust_z - 0.22 - i * boy * 0.18), mt("ic_koyu"), 12)
    for s in (-1, 1):                                            # cep kapakları
        kutu((0.10, 0.01, 0.025), (x + s * alt * 0.24, y1 - 0.004, ust_z - boy * 0.62), mat, 0.004)
    if kapson:
        kutu((0.20, 0.07, 0.17), (x, (y0 + y1) / 2 + 0.012, ust_z - 0.07), mat, 0.05)


def askilik():
    cam, pr = mt("cam_agaci"), mt("pirinc")
    kutu((0.90, 0.022, 0.10), (0, -0.011, 0), cam, 0.005)                  # çıta, duvara vidalı
    kutu((0.90, 0.010, 0.014), (0, -0.027, 0.036), cam, 0.004)             # üst pervaz
    for x in (-0.40, 0.40):
        dik_disk(0.007, 0.004, (x, -0.024, 0.0), pr, 10)                   # vida başları
    for x in (-0.30, 0.02, 0.22, 0.40):                                    # dört pirinç kanca
        dik_disk(0.013, 0.006, (x, -0.025, 0.0), pr, 14)
        uzun = 0.14 if x == 0.40 else 0.075                                # şemsiyeninki uzun, öndeki paltoları aşsın
        cubuk((x, -0.025, 0.0), (x, -uzun, 0.035), 0.005, pr)
        kure(0.009, (x, -uzun, 0.035), pr)
        cubuk((x, -0.025, -0.006), (x, -0.058, -0.026), 0.004, pr)
    palto(-0.30, 0.20, 0.40, 0.46, 1.00, -0.030, -0.125, M("Palto_Bej", (0.42, 0.33, 0.21), 0.9))
    palto(0.02, 0.20, 0.42, 0.48, 0.98, -0.050, -0.150, M("Palto_Koyu", (0.025, 0.03, 0.045), 0.9))
    sari = M("Yagmurluk_Sari", (0.75, 0.50, 0.02), 0.35)
    palto(0.22, 0.14, 0.26, 0.30, 0.58, -0.155, -0.215, sari, kapson=True)
    for s in (-1, 1):                                                      # kollarda gri reflektör şerit
        kutu((0.088, 0.078, 0.02), (0.22 + s * 0.11, -0.195, 0.02 - 0.16 - 0.58 * 0.55), M("Reflektor", (0.45, 0.45, 0.45), 0.3))
    # Şemsiye: bordo, kapalı, J biçimli sapı uzun kancada.
    sem, sap = M("Semsiye", (0.12, 0.015, 0.04), 0.5), mt("ahsap_orta")
    cx, cz, r = 0.40, 0.022, 0.03
    yay_cubuk((cx, -0.14, cz), r, 0.0, math.pi, 0.008, sap, 8)
    cubuk((cx - r, -0.14, cz), (cx - r, -0.14, cz - 0.02), 0.008, sap)
    cubuk((cx + r, -0.14, cz), (cx + r, -0.14, -0.14), 0.006, M("Semsiye_Cubuk", (0.3, 0.3, 0.3), 0.3, 0.9))
    koni(0.012, 0.042, 0.62, (cx + r, -0.14, -0.14 - 0.31), sem, 16, rot=(math.pi, 0, 0))
    kutu((0.03, 0.03, 0.03), (cx + r, -0.14, -0.30), sem, 0.01)            # toplama bandı
    cubuk((cx + r, -0.14, -0.76), (cx + r, -0.14, -0.82), 0.004, pr)       # uç
    return bitir("Askilik_K3", "duvar")


def ayakkabi(tur, konum, aci=0.0, yan=False):
    """Tek ayakkabı; burnu -y'de kurulur, sonra çevrilip yerleştirilir. yan: yan yatmış."""
    bas = len(P)
    tb, ic = M("Taban_Koyu", (0.03, 0.025, 0.02), 0.7), mt("ic_koyu")
    if tur == "erkek":
        L, W = 0.29, 0.105
        deri = M("Deri_Kahve", (0.07, 0.03, 0.012), 0.35)
        kutu((W, L, 0.022), (0, 0, 0.011), tb, 0.008)
        kutu((W * 0.8, 0.07, 0.014), (0, L / 2 - 0.04, 0.028), tb, 0.003)
        kutu((W - 0.008, L * 0.62, 0.065), (0, L * 0.14, 0.0545), deri, 0.025)
        kure(W / 2 - 0.006, (0, -L / 2 + 0.072, 0.045), deri, (1.0, 1.55, 0.62))
        kutu((W - 0.035, L * 0.28, 0.006), (0, L * 0.21, 0.085), ic)
        for i in range(3):
            kutu((W * 0.5, 0.006, 0.004), (0, -0.03 - i * 0.02, 0.083 - i * 0.004), M("Bagcik", (0.02, 0.015, 0.01), 0.8))
    elif tur == "kadin":
        L, W = 0.245, 0.078
        deri = M("Deri_Siyah", (0.022, 0.018, 0.02), 0.22)
        kutu((0.03, 0.03, 0.055), (0, L / 2 - 0.025, 0.0275), tb, 0.004)       # topuk
        kutu((W, 0.10, 0.008), (0, -L / 2 + 0.05, 0.004), tb, 0.003)
        ic_t = kutu((W - 0.006, 0.163, 0.006), (0, 0.035, 0.033), ic)          # eğik iç taban
        don(ic_t, rx=math.radians(17.9))
        kure(0.036, (0, -L / 2 + 0.055, 0.0235), deri, (1.05, 1.75, 0.62))      # burun
        kutu((W - 0.004, 0.05, 0.032), (0, L / 2 - 0.03, 0.072), deri, 0.012)  # topuk kavrayıcı
        for s in (-1, 1):
            ktk = kutu((0.005, 0.13, 0.022), (s * (W / 2 - 0.004), 0.04, 0.05), deri, 0.002)
            don(ktk, rx=math.radians(17.9))
    elif tur == "spor":
        L, W = 0.19, 0.072
        mavi = M("Spor_Mavi", (0.04, 0.10, 0.35), 0.6)
        beyaz = mt("beyaz_plastik")
        kutu((W, L, 0.02), (0, 0, 0.01), beyaz, 0.007)
        kutu((W - 0.006, L * 0.64, 0.05), (0, L * 0.15, 0.045), mavi, 0.02)
        kure(W / 2 - 0.005, (0, -L / 2 + 0.046, 0.035), mavi, (1.0, 1.5, 0.6))
        for s in (-1, 1):
            k = kutu((0.004, 0.075, 0.012), (s * (W / 2 - 0.001), 0.005, 0.045), M("Spor_Kirmizi", (0.45, 0.03, 0.02), 0.6))
            don(k, rx=0.45)
        kutu((W + 0.002, 0.026, 0.008), (0, -0.012, 0.068), beyaz, 0.003)     # cırt bant
        kutu((W - 0.03, 0.05, 0.004), (0, 0.045, 0.0705), ic)
    else:  # cizme
        L, W = 0.18, 0.07
        kau = M("Cizme_Yesil", (0.04, 0.22, 0.05), 0.35)
        kutu((W, L, 0.015), (0, 0, 0.0075), M("Cizme_Taban", (0.55, 0.40, 0.03), 0.5), 0.005)
        kutu((W - 0.004, L - 0.03, 0.05), (0, 0.01, 0.038), kau, 0.02)
        kure(W / 2 - 0.004, (0, -L / 2 + 0.04, 0.035), kau, (1.0, 1.4, 0.65))
        kutu((W + 0.004, 0.085, 0.17), (0, L / 2 - 0.05, 0.115), kau, 0.02)
        kutu((W - 0.012, 0.065, 0.003), (0, L / 2 - 0.05, 0.2005), ic)
        kutu((W + 0.008, 0.089, 0.02), (0, L / 2 - 0.05, 0.19), M("Cizme_Sari", (0.70, 0.48, 0.03), 0.4), 0.005)
    x, y = konum
    if yan:
        n = grupla(bas, (x, y, 0.0), (0, math.pi / 2, aci))
        n.data.transform(Matrix.Translation((0, 0, -min(v.co.z for v in n.data.vertices))))   # yan yatınca tam yere otursun
        return n
    return grupla(bas, (x, y, 0.0), (0, 0, aci))


def ayakkabi_yigini():
    ayakkabi("erkek", (-0.30, 0.10), 0.10)
    ayakkabi("erkek", (-0.17, 0.06), -0.06)
    ayakkabi("kadin", (-0.01, 0.12), 0.18)
    ayakkabi("kadin", (0.08, 0.13), 0.05)
    ayakkabi("spor", (0.22, -0.06), -0.45)
    ayakkabi("spor", (0.31, 0.12), 0.9, yan=True)
    ayakkabi("cizme", (-0.12, -0.16), 0.30)
    ayakkabi("cizme", (0.06, -0.16), 1.95, yan=True)
    return bitir("Ayakkabi_Yigini_K3")


def canta():
    kir, lac = M("Canta_Kirmizi", (0.32, 0.03, 0.025), 0.55), M("Canta_Lacivert", (0.02, 0.04, 0.14), 0.6)
    met = mt("krom")
    # Sırtüstü yatık gövde: 0,32 x 0,34 x 0,12; ağzı +y ucunda. Üst yüz (çantanın önü) z = 0,12.
    kutu((0.32, 0.34, 0.12), (0, 0, 0.06), kir, 0.018)
    for z in (0.004, 0.116):                                              # kenar biyesi
        kutu((0.326, 0.346, 0.008), (0, 0, z), lac, 0.003)
    kutu((0.22, 0.10, 0.02), (0, -0.11, 0.128), lac, 0.008)               # ön cep (alt uçta)
    kutu((0.05, 0.02, 0.006), (0, -0.155, 0.139), met, 0.002)             # cep tokası
    kutu((0.08, 0.03, 0.002), (0.06, -0.10, 0.1385), M("Canta_Yildiz", (0.70, 0.55, 0.05), 0.4))   # yapıştırma
    kutu((0.28, 0.004, 0.09), (0, 0.169, 0.06), mt("ic_koyu"))            # açık ağız
    # Kapak: arka üst kenarından (y = 0,17, z = 0) menteşeli, açılıp yere yatmış; iç yüzü yukarıda.
    kutu((0.30, 0.26, 0.012), (0, 0.30, 0.006), kir, 0.01)
    kutu((0.30, 0.02, 0.013), (0, 0.42, 0.0065), lac, 0.004)
    for x in (-0.08, 0.08):                                               # kayış ve tokalar
        kutu((0.03, 0.10, 0.004), (x, 0.39, 0.014), lac, 0.001)
        kutu((0.036, 0.016, 0.006), (x, 0.345, 0.015), met, 0.002)
    yay_cubuk((0, 0.20, 0.008), 0.05, 0.0, math.pi, 0.008, lac, 6)        # sap (ağzın önünde yere yatık)
    # Ağızdan kaymış defter ve kalem kutusu, kapağın üstünde.
    d = kutu((0.20, 0.24, 0.008), (0.03, 0.27, 0.016), M("Defter_Mavi", (0.05, 0.12, 0.35), 0.7), 0.002)
    don(d, rz=0.18)
    kutu((0.19, 0.23, 0.004), (0.03, 0.27, 0.0135), mt("kagit"))
    kk = kutu((0.20, 0.06, 0.04), (-0.07, 0.22, 0.032), M("Kalem_Kutusu", (0.70, 0.45, 0.03), 0.5), 0.015)
    don(kk, rz=-0.25)
    fer = kutu((0.19, 0.004, 0.004), (-0.07, 0.22, 0.052), met)
    don(fer, rz=-0.25)
    boya_kalemi(0.10, 0.36, 0.012, 0.7, mt("boya_kirmizi"))
    BILGI.append("govde ust yuzu z=0.120, duz ust bolge govde y -0.055..+0.17, on cep govde y -0.16..-0.06 (ust z=0.138); "
                 "govde merkezi pivotlamadan once y=0: son koordinat = bu degerler - PIVOT kaydirmasi")
    return bitir("Okul_Cantasi_K3")


def bebek_arabasi():
    boru, sap = mt("krom"), mt("siyah_plastik")
    kumas = M("Araba_Kumas", (0.03, 0.05, 0.16), 0.8)
    serit = M("Araba_Serit", (0.40, 0.04, 0.03), 0.8)
    # Duvara 14,4° yaslanmış iki ana boru: dipte y = -0,34, tepede y = -0,10 (duvar y = 0).
    alt, ust = (-0.34, 0.065), (-0.10, 0.98)
    for x in (-0.13, 0.13):
        cubuk((x, alt[0], alt[1]), (x, ust[0], ust[1]), 0.011, boru)
        yay_cubuk((x, -0.05, ust[1]), 0.05, math.pi, 0.0, 0.011, boru, 6, duzlem="yz")   # baston sap
        cubuk((x, 0.0, ust[1]), (x, 0.0, ust[1] - 0.03), 0.011, boru)
        t0, t1 = 0.72, 0.97
        cubuk((x, alt[0] + (ust[0] - alt[0]) * t0, alt[1] + (ust[1] - alt[1]) * t0),
              (x, alt[0] + (ust[0] - alt[0]) * t1, alt[1] + (ust[1] - alt[1]) * t1), 0.017, sap, 10)   # köpük tutamak
        kure(0.016, (x, 0.0, ust[1] - 0.035), sap)
        # arka ayak borusu: ana borunun hemen arkasında, katlanmış
        cubuk((x * 0.75, alt[0] + 0.07, 0.065), (x * 0.75, -0.17, 0.62), 0.009, boru)
        for (yy, xx) in ((alt[0], x), (alt[0] + 0.07, x * 0.75)):         # çift tekerlekler
            for dx in (-0.018, 0.018):
                yatik_sil(0.062, 0.022, (xx + dx, yy, 0.062), "x", sap, 20)
            yatik_sil(0.02, 0.06, (xx, yy, 0.062), "x", boru, 10)
    # Katlanmış oturak/tente bohçası iki borunun arasında; boruyla aynı eğimde.
    e = -math.atan2(ust[0] - alt[0], ust[1] - alt[1])
    for (zc, boy, kal, m) in ((0.45, 0.62, 0.09, kumas), (0.78, 0.18, 0.11, kumas)):
        yc = alt[0] + (zc - alt[1]) * (ust[0] - alt[0]) / (ust[1] - alt[1]) + 0.01
        don(kutu((0.22, kal, boy), (0, yc, zc), m, 0.03), rx=e)
    for zc in (0.30, 0.60):
        yc = alt[0] + (zc - alt[1]) * (ust[0] - alt[0]) / (ust[1] - alt[1]) + 0.01
        don(kutu((0.225, 0.095, 0.02), (0, yc, zc), serit, 0.005), rx=e)
    yc = alt[0] + (0.45 - alt[1]) * (ust[0] - alt[0]) / (ust[1] - alt[1])
    don(kutu((0.30, 0.03, 0.04), (0, yc - 0.045, 0.45), sap, 0.008), rx=e)   # toplama kayışı ve tokası
    return bitir("Bebek_Arabasi_K3", "dayali")


def terlik(L, W, taban, bant, konum, aci, tip="acik"):
    bas = len(P)
    elips(W, L, 0.016, (0, 0, 0.008), taban)
    if tip == "acik":
        kutu((W + 0.006, L * 0.36, 0.034), (0, -L * 0.17, 0.030), bant, 0.016)
    else:  # kulaklı, kapalı burunlu çocuk terliği
        pembe_ac = M("Terlik_PembeAcik", (0.80, 0.50, 0.55), 0.9)
        kure(W / 2, (0, -L * 0.12, 0.0265), bant, (1.0, 1.55, 0.75))
        for s in (-1, 1):
            kulak = kure(0.017, (s * 0.017, -L * 0.14, 0.062), bant, (0.45, 0.8, 1.6))
            don(kulak, ry=s * 0.3)
            ic = kure(0.009, (s * 0.017, -L * 0.14 - 0.007, 0.062), pembe_ac, (0.4, 0.5, 1.4))
            don(ic, ry=s * 0.3)
            kure(0.004, (s * 0.012, -L * 0.40, 0.037), mt("ic_koyu"), seg=8, halka=6)
        kure(0.006, (0, -L * 0.47, 0.030), pembe_ac, seg=8, halka=6)
    return grupla(bas, (konum[0], konum[1], 0.0), (0, 0, aci))


def terlikler():
    k1, k1b = M("Terlik_Kahve", (0.10, 0.06, 0.03), 0.8), M("Terlik_Ekose", (0.14, 0.05, 0.03), 0.9)
    k2, k2b = M("Terlik_Bordo", (0.16, 0.02, 0.03), 0.8), M("Terlik_Kadife", (0.25, 0.04, 0.08), 0.95)
    k3 = M("Terlik_Pembe", (0.72, 0.30, 0.38), 0.95)
    for i, (x, a) in enumerate(((-0.34, 0.04), (-0.225, -0.03))):
        terlik(0.285, 0.105, k1, k1b, (x, 0.0), a)
    for x, a in ((-0.07, 0.02), (0.035, -0.05)):
        terlik(0.25, 0.092, k2, k2b, (x, 0.01), a)
    for x, a in ((0.17, 0.08), (0.25, -0.02)):
        terlik(0.18, 0.068, k3, k3, (x, -0.03), a, tip="kulakli")
    return bitir("Terlikler_K3")


def cizimler():
    # (u, v, tür, açı) ; kâğıtlar biraz eğri bantlanmış. Her kâğıt bir öncekinden 1,5 mm öne: üst üste binebilsin.
    plan = [(-0.45, 0.07, "ev", 0.05), (-0.22, -0.05, "gunes", -0.04), (0.00, 0.08, "aile", 0.03),
            (0.24, -0.04, "karalama", -0.06), (0.46, 0.05, "pencere", 0.04), (-0.34, -0.14, "araba", -0.08)]
    for i, (u, v, tur, a) in enumerate(plan):
        bas = kagit(tur, "duvar", tohum=30 + i)
        W, H = (0.297, 0.21) if tur in ("ev", "aile", "araba") else (0.21, 0.297)
        for s in (-1, 1):                                                  # üst köşelerde bant
            c_kutu(s * (W / 2 - 0.012), H / 2 - 0.006, 0.045, 0.016, mt("bant"), 6, "duvar", s * -0.5)
        c_kutu(0, -H / 2 + 0.006, 0.04, 0.015, mt("bant"), 6, "duvar", 0.1)
        grupla(bas, (u, -0.0015 * i, v), (0, -a, 0))
    return bitir("Cocuk_Cizimleri_K3", "duvar")


def oyuncaklar():
    # Oyuncak araba (kırmızı), yan dönmüş.
    bas = len(P)
    kutu((0.05, 0.12, 0.028), (0, 0, 0.026), M("Oyuncak_Kirmizi", (0.45, 0.03, 0.02), 0.3), 0.008)
    kutu((0.044, 0.06, 0.024), (0, 0.012, 0.05), M("Oyuncak_Kirmizi", (0.45, 0.03, 0.02), 0.3), 0.008)
    kutu((0.046, 0.05, 0.016), (0, 0.012, 0.051), M("Oyuncak_Cam", (0.05, 0.08, 0.10), 0.1))
    for x in (-0.027, 0.027):
        for y in (-0.038, 0.04):
            yatik_sil(0.013, 0.01, (x, y, 0.013), "x", mt("siyah_plastik"), 12)
    grupla(bas, (-0.02, -0.16, 0), (0, 0, 0.5))
    # Tahta bloklar: renkli küpler, ikisi üst üste, biri eğik; bir de üçgen çatı bloğu.
    renk = [M("Blok_Kirmizi", (0.40, 0.04, 0.03), 0.6), M("Blok_Sari", (0.70, 0.48, 0.05), 0.6),
            M("Blok_Mavi", (0.04, 0.12, 0.40), 0.6), M("Blok_Yesil", (0.05, 0.25, 0.06), 0.6)]
    s = 0.045
    for i, (x, y, z, a) in enumerate(((-0.25, 0.10, 0, 0.2), (-0.25, 0.10, s, 0.6), (-0.17, 0.15, 0, -0.3),
                                      (-0.12, 0.04, 0, 0.9), (-0.32, 0.18, 0, 0.1), (-0.20, -0.02, 0, 1.3), (-0.33, 0.00, 0, 0.5))):
        don(kutu((s, s, s), (x, y, z + s / 2), renk[i % 4], 0.004), rz=a)
    b = kutu((s, s, s), (-0.08, 0.17, s / 2 + 0.008), renk[2], 0.004)
    don(b, rx=0.4, rz=0.3)
    bas = len(P)
    prizma([(-0.03, 0.0), (0.03, 0.0), (0.0, 0.035)], "yz", -0.0225, 0.0225, renk[0])
    grupla(bas, (-0.30, -0.10, 0), (0, 0, 0.7))
    # Top: kırmızı, sarı kuşaklı.
    kure(0.07, (0.33, 0.14, 0.07), M("Top_Kirmizi", (0.45, 0.04, 0.03), 0.35), seg=20, halka=12)
    k = sil(0.0705, 0.03, (0.33, 0.14, 0.07), M("Top_Sari", (0.75, 0.52, 0.04), 0.35), 24)
    don(k, rx=0.5, ry=0.3)
    # Plastik damperli kamyon: sarı kabin ve şasi, kırmızı damper.
    bas = len(P)
    sari, kir = M("Kamyon_Sari", (0.72, 0.50, 0.03), 0.4), M("Kamyon_Kirmizi", (0.45, 0.04, 0.02), 0.4)
    kutu((0.10, 0.25, 0.03), (0, 0, 0.045), sari, 0.006)
    kutu((0.10, 0.07, 0.08), (0, -0.09, 0.10), sari, 0.012)
    kutu((0.102, 0.035, 0.035), (0, -0.09, 0.115), M("Oyuncak_Cam", (0.05, 0.08, 0.10), 0.1), 0.004)
    dam = kutu((0.11, 0.15, 0.07), (0, 0.045, 0.10), kir, 0.008)
    don(dam, rx=0.18)
    for y in (-0.09, 0.02, 0.08):
        for x in (-0.055, 0.055):
            yatik_sil(0.028, 0.02, (x, y, 0.028), "x", mt("siyah_plastik"), 14)
    grupla(bas, (0.16, -0.08, 0), (0, 0, 1.25))
    return bitir("Dagilmis_Oyuncaklar_K3")


def yolluk():
    bordo, yesil = M("Yolluk_Bordo", (0.20, 0.02, 0.025), 0.95), M("Yolluk_Yesil", (0.03, 0.12, 0.05), 0.95)
    krem, lac = M("Yolluk_Krem", (0.62, 0.52, 0.35), 0.95), M("Yolluk_Lacivert", (0.02, 0.025, 0.08), 0.95)
    EN, BOY, T = 3.40, 0.75, 0.006
    kutu((EN, BOY, T), (0, 0, T / 2), bordo)
    z1, z2, z3 = T + 0.0005, T + 0.0012, T + 0.0019
    for y in (-1, 1):                                                     # uzun kenar bordürleri
        kutu((EN - 0.08, 0.05, 0.001), (0, y * (BOY / 2 - 0.065), z1), yesil)
        kutu((EN - 0.14, 0.008, 0.001), (0, y * (BOY / 2 - 0.105), z1), krem)
    for x in (-1, 1):                                                     # kısa kenar bantları
        kutu((0.05, BOY - 0.08, 0.001), (x * (EN / 2 - 0.065), 0, z1), yesil)
        kutu((0.03, BOY - 0.20, 0.001), (x * (EN / 2 - 0.13), 0, z1), lac)
    for i in range(8):                                                    # ortada baklava dizisi
        x = -EN / 2 + 0.36 + i * (EN - 0.72) / 7
        don(kutu((0.20, 0.20, 0.001), (x, 0, z1), yesil), rz=math.pi / 4)
        don(kutu((0.12, 0.12, 0.001), (x, 0, z2), krem), rz=math.pi / 4)
        don(kutu((0.05, 0.05, 0.001), (x, 0, z3), bordo), rz=math.pi / 4)
        if i < 7:
            xm = x + (EN - 0.72) / 14
            for yy in (-0.12, 0.12):
                don(kutu((0.04, 0.04, 0.001), (xm, yy, z1), krem), rz=math.pi / 4)
            don(kutu((0.06, 0.06, 0.001), (xm, 0, z1), lac), rz=math.pi / 4)
    for x in (-1, 1):                                                     # saçaklar
        for j in range(16):
            kutu((0.05, 0.012, 0.003), (x * (EN / 2 + 0.024), -BOY / 2 + 0.04 + j * (BOY - 0.08) / 15, 0.0015), krem)
    return bitir("Yolluk_K3")


def tv():
    gumus, koyu = M("TV_Gumus", (0.36, 0.36, 0.37), 0.4), M("TV_Koyu", (0.05, 0.05, 0.055), 0.5)
    ekran = M("TV_Ekran", (0.012, 0.016, 0.018), 0.08)
    kutu((0.45, 0.35, 0.015), (0, 0.0, 0.0075), koyu, 0.004)              # alt ayak
    kutu((0.55, 0.30, 0.45), (0, -0.075, 0.24), gumus, 0.02)              # ön gövde
    kutu((0.40, 0.16, 0.33), (0, 0.145, 0.22), gumus, 0.035)              # arka tüp kısmı
    kutu((0.42, 0.012, 0.33), (-0.05, -0.226, 0.265), koyu, 0.012)        # ekran çerçevesi
    kure(0.2, (-0.05, -0.226, 0.265), ekran, (0.98, 0.06, 0.76), seg=24, halka=12)   # hafif bombeli cam
    for i in range(5):                                                     # sağda düğme sütunu
        kutu((0.03, 0.01, 0.014), (0.215, -0.228, 0.37 - i * 0.032), koyu, 0.003)
    dik_disk(0.012, 0.01, (0.215, -0.228, 0.17), M("TV_Kirmizi", (0.40, 0.02, 0.02), 0.4), 14)   # açma düğmesi
    for i in range(6):                                                     # hoparlör yarıkları
        kutu((0.07, 0.006, 0.004), (0.215, -0.2255, 0.12 - i * 0.012), koyu)
    kutu((0.10, 0.006, 0.018), (-0.05, -0.2275, 0.075), koyu, 0.003)      # marka plakası
    for y in (0.08, 0.12, 0.16):                                           # arka havalandırma
        kutu((0.30, 0.006, 0.01), (0, y, 0.387), koyu)
    # Üstte dantel örtü ve tavşan kulağı anten.
    kutu((0.34, 0.24, 0.002), (0.0, -0.08, 0.466), mt("kumas_beyaz"))
    kutu((0.34, 0.002, 0.05), (0.0, -0.2255, 0.44), mt("kumas_beyaz"))    # önden sarkan ucu
    kure(0.045, (0.0, -0.08, 0.467), koyu, (1.3, 1.0, 0.55))
    for (ux, uy) in ((-0.25, 0.08), (0.25, 0.05)):
        cubuk((0.0, -0.08, 0.49), (ux, -0.08 + uy, 0.49 + 0.30), 0.003, mt("krom"))
        kure(0.006, (ux, -0.08 + uy, 0.79), mt("krom"))
    return bitir("Tuplu_TV_K3")


def boyama():
    ah = mt("ahsap_orta")
    kutu((1.0, 0.55, 0.03), (0, 0, 0.435), ah, 0.006)
    for x in (-0.44, 0.44):
        for y in (-0.22, 0.22):
            koni(0.018, 0.024, 0.42, (x, y, 0.21), ah, 12)
    for y in (-0.24, 0.24):
        kutu((0.84, 0.02, 0.06), (0, y, 0.39), ah, 0.004)
    kutu((0.90, 0.45, 0.018), (0, 0, 0.12), ah, 0.004)                     # alt raf
    kutu((0.24, 0.30, 0.06), (0.28, 0.02, 0.159), M("Dergi_Yigini", (0.45, 0.35, 0.20), 0.9), 0.003)  # altta dergiler
    top = 0.45
    # Açık boyama kitabı (mavi kapak, iki sayfa hafif V).
    bas = len(P)
    kutu((0.43, 0.29, 0.004), (0, 0, 0.002), M("Boyama_Kapak", (0.05, 0.20, 0.45), 0.6), 0.002)
    for s, tur in ((-1, "boyama_kedi"), (1, "boyama_cicek")):
        b2 = kagit(tur, "yer", boyut=(0.20, 0.28))
        grupla(b2, (s * 0.105, 0, 0.004), (0, s * -0.05, 0))
    grupla(bas, (-0.12, -0.03, top), (0, 0, -0.15))
    renk = ["kirmizi", "mavi", "yesil", "sari", "siyah", "turuncu", "mor", "kahve"]
    for i, rk in enumerate(renk):
        boya_kalemi(0.10 + rnd.uniform(0, 0.25), -0.20 + rnd.uniform(0, 0.25), top, rnd.uniform(0, 6.28), mt("boya_" + rk))
    # Boya kalemi kutusu (açık, yarısı boş).
    kutu((0.13, 0.09, 0.012), (0.30, 0.17, top + 0.006), M("Kalem_Kutu", (0.65, 0.42, 0.03), 0.7), 0.002)
    # Yarım süt bardağı.
    bx, by = 0.40, -0.02
    g = sil(0.034, 0.10, (bx, by, top + 0.05), mt("cam"), 20)
    ortak.oy(g, ortak.silindir("Oy", 0.031, 0.10, (bx, by, top + 0.056), mt("cam"), 20))
    sil(0.0305, 0.045, (bx, by, top + 0.004 + 0.0225), mt("sut"), 20)
    return bitir("Boyama_Sehpasi_K3")


def park():
    ah = mt("bal_ahsap")
    for x in (-0.48, 0.48):
        for y in (-0.48, 0.48):
            kutu((0.04, 0.04, 0.75), (x, y, 0.375), ah, 0.006)
            kure(0.018, (x, y, 0.018), mt("siyah_plastik"))                # tekerlek
    for z, h in ((0.735, 0.03), (0.12, 0.04)):
        for y in (-0.48, 0.48):
            kutu((0.96, 0.04, h), (0, y, z), ah, 0.006)
        for x in (-0.48, 0.48):
            kutu((0.04, 0.96, h), (x, 0, z), ah, 0.006)
    for i in range(12):                                                    # parmaklıklar
        t = -0.41 + i * 0.82 / 11
        for (x, y) in ((t, -0.48), (t, 0.48), (-0.48, t), (0.48, t)):
            sil(0.011, 0.58, (x, y, 0.43), ah, 8)
    kutu((0.92, 0.92, 0.02), (0, 0, 0.11), ah, 0.004)                     # taban
    kutu((0.90, 0.90, 0.03), (0, 0, 0.135), M("Park_Minder", (0.35, 0.50, 0.65), 0.9), 0.012)
    # Buruşuk battaniye (sarı) ve çıngırak.
    bat = M("Park_Battaniye", (0.75, 0.62, 0.30), 0.95)
    for (x, y, a, h) in ((0.12, 0.14, 0.3, 0.05), (0.20, 0.05, -0.4, 0.04), (0.05, 0.22, 0.9, 0.035)):
        don(kutu((0.30, 0.20, h), (x, y, 0.15 + h / 2), bat, 0.018), rz=a)
    bas = len(P)
    cubuk((0, 0, 0.012), (0.10, 0, 0.012), 0.008, M("Cingirak_Sap", (0.70, 0.60, 0.10), 0.3))
    kure(0.028, (0.13, 0, 0.028), M("Cingirak_Top", (0.50, 0.05, 0.05), 0.3))
    for dx in (0.02, 0.05):
        sil(0.012, 0.006, (dx, 0, 0.012), M("Cingirak_Halka", (0.05, 0.25, 0.45), 0.3), 12)
    grupla(bas, (-0.25, -0.18, 0.15), (0, 0, 0.6))
    # Bir kenarda boncuklu sayı çubuğu.
    cubuk((-0.46, -0.30, 0.62), (-0.46, 0.30, 0.62), 0.005, mt("krom"))
    for j in range(6):
        kure(0.022, (-0.46, -0.20 + j * 0.06, 0.62), [M("Boncuk_%d" % k, c, 0.3) for k, c in enumerate(((0.45, 0.04, 0.03), (0.75, 0.52, 0.04), (0.04, 0.15, 0.45)))][j % 3])
    return bitir("Oyun_Parki_K3")


def hali():
    kir, lac = M("Hali_Kirmizi", (0.30, 0.03, 0.025), 0.95), M("Hali_Lacivert", (0.02, 0.025, 0.09), 0.95)
    krem, tur_ = M("Hali_Krem", (0.66, 0.56, 0.38), 0.95), M("Hali_Turuncu", (0.50, 0.20, 0.03), 0.95)
    yes = M("Hali_Yesil", (0.04, 0.12, 0.05), 0.95)
    EN, BOY, T = 2.50, 1.70, 0.008
    kutu((EN, BOY, T), (0, 0, T / 2), kir)
    z = [T + 0.0005 + 0.0008 * i for i in range(5)]
    # Bordür: lacivert bant, iki yanında krem çizgi, üstünde küçük krem baklavalar.
    for s in (-1, 1):
        kutu((EN - 0.08, 0.18, 0.001), (0, s * (BOY / 2 - 0.13), z[0]), lac)
        kutu((0.18, BOY - 0.08, 0.001), (s * (EN / 2 - 0.13), 0, z[0]), lac)
        for dd in (0.03, 0.23):
            kutu((EN - 0.06 - 2 * dd + 0.06, 0.01, 0.001), (0, s * (BOY / 2 - dd - 0.005), z[1]), krem)
            kutu((0.01, BOY - 0.06 - 2 * dd + 0.06, 0.001), (s * (EN / 2 - dd - 0.005), 0, z[1]), krem)
    for i in range(11):
        x = -EN / 2 + 0.25 + i * (EN - 0.5) / 10
        for s in (-1, 1):
            don(kutu((0.06, 0.06, 0.001), (x, s * (BOY / 2 - 0.13), z[1]), krem), rz=math.pi / 4)
    for j in range(6):
        y = -BOY / 2 + 0.30 + j * (BOY - 0.6) / 5
        for s in (-1, 1):
            don(kutu((0.06, 0.06, 0.001), (s * (EN / 2 - 0.13), y, z[1]), krem), rz=math.pi / 4)
    # Göbek: iç içe sekizgenler (x yönünde uzatılmış) ve iki uçta sarkıt.
    for i, (r, m) in enumerate(((0.42, lac), (0.34, krem), (0.28, tur_), (0.17, lac), (0.07, krem))):
        n = sil(r, 0.001, (0, 0, z[i]), m, 8)
        olcekle(n, (1.5, 1.0, 1.0))
    for s in (-1, 1):
        don(kutu((0.16, 0.16, 0.001), (s * 0.72, 0, z[0]), lac), rz=math.pi / 4)
        don(kutu((0.08, 0.08, 0.001), (s * 0.72, 0, z[1]), krem), rz=math.pi / 4)
        don(kutu((0.05, 0.05, 0.001), (s * 0.86, 0, z[0]), yes), rz=math.pi / 4)
    # Köşe üçgenleri ve dağınık küçük motifler.
    ix, iy = EN / 2 - 0.25, BOY / 2 - 0.25
    for sx in (-1, 1):
        for sy in (-1, 1):
            prizma([(sx * ix, sy * iy), (sx * (ix - 0.38), sy * iy), (sx * ix, sy * (iy - 0.30))], "xy", z[0], z[0] + 0.001, lac)
            prizma([(sx * (ix - 0.02), sy * (iy - 0.02)), (sx * (ix - 0.24), sy * (iy - 0.02)), (sx * (ix - 0.02), sy * (iy - 0.19))], "xy", z[1], z[1] + 0.001, tur_)
    for (x, y) in ((-0.55, 0.45), (0.55, 0.45), (-0.55, -0.45), (0.55, -0.45), (0, 0.55), (0, -0.55), (-0.95, 0), (0.95, 0)):
        don(kutu((0.05, 0.05, 0.001), (x, y, z[0]), krem), rz=math.pi / 4)
        don(kutu((0.02, 0.02, 0.001), (x, y, z[1]), yes), rz=math.pi / 4)
    for x in (-1, 1):                                                     # saçaklar
        for j in range(24):
            kutu((0.06, 0.012, 0.003), (x * (EN / 2 + 0.028), -BOY / 2 + 0.05 + j * (BOY - 0.1) / 23, 0.0015), krem)
    return bitir("Hali_K3")


def serit_kumas(x1, z1, x2, z2, en, kal, y, mat):
    """Perde pilisi: (x1, z1)'den (x2, z2)'ye xz düzleminde uzanan ince kumaş şeridi."""
    L = math.hypot(x2 - x1, z2 - z1)
    n = kutu((en, kal, L), ((x1 + x2) / 2, y, (z1 + z2) / 2), mat)
    return don(n, ry=math.atan2(x2 - x1, z2 - z1))


def perde():
    sari, sari_k = M("Perde_Sari", (0.72, 0.52, 0.10), 0.95), M("Perde_SariKoyu", (0.55, 0.36, 0.05), 0.95)
    tul, pr = M("Perde_Tul", (0.78, 0.77, 0.72), 0.9), mt("pirinc")
    Y = -0.10
    cubuk((-0.92, Y, 0), (0.92, Y, 0), 0.012, pr, 12)                      # çubuk
    for s in (-1, 1):
        kure(0.022, (s * 0.94, Y, 0), pr)
        kutu((0.02, 0.10, 0.02), (s * 0.80, -0.05, 0), pr, 0.004)          # duvar tutucuları
        dik_disk(0.02, 0.006, (s * 0.80, -0.003, 0), pr)
    kutu((1.70, 0.01, 0.01), (0, -0.06, -0.02), mt("beyaz_plastik"))      # tül rayı
    for i in range(24):                                                    # tül: düz pliler, arkada
        x = -0.82 + (i + 0.5) * 1.64 / 24
        kutu((0.074, 0.006, 1.58), (x, -0.06 + (0.008 if i % 2 else -0.008), -0.03 - 0.79), tul)
    # Sarı kanatlar: tepede toplanmış, bağda sıkışmış, altta açılan pililer.
    TZ, BZ = -0.95, -1.62
    for s in (-1, 1):
        for i in range(7):
            xt, xb, xa = s * (0.88 - i * 0.052), s * (0.86 - i * 0.017), s * (0.88 - i * 0.044)
            yy = Y - 0.018 + (0.012 if i % 2 else -0.004)
            serit_kumas(xt, -0.02, xb, TZ, 0.07, 0.012, yy, sari)
            serit_kumas(xb, TZ, xa, BZ, 0.07, 0.012, yy, sari)
        kutu((0.38, 0.03, 0.05), (s * 0.71, Y - 0.02, -0.035), sari_k, 0.01)   # üst büzgü bandı
        kutu((0.16, 0.06, 0.045), (s * 0.80, Y - 0.02, TZ), sari_k, 0.015)     # bağ
        kure(0.025, (s * 0.75, Y - 0.055, TZ), sari_k, (1.4, 0.6, 1.0))       # fiyonk
        cubuk((s * 0.93, 0.0, TZ), (s * 0.93, -0.06, TZ), 0.006, pr)           # duvardaki bağ kancası
    return bitir("Perde_K3", "perde")


def raf():
    ah, dan = M("Raf_Tik", (0.10, 0.05, 0.02), 0.45), mt("kumas_beyaz")
    por, pastel = mt("porselen"), M("Biblo_Mavi", (0.35, 0.50, 0.70), 0.15)
    ten, alt = M("Biblo_Ten", (0.70, 0.50, 0.38), 0.2), M("Biblo_Yaldiz", (0.60, 0.42, 0.10), 0.3, 0.9)
    kutu((0.80, 0.18, 0.022), (0, -0.09, 0), ah, 0.005)
    for x in (-0.28, 0.28):                                                # oyma konsollar
        prizma([(0.0, -0.011), (-0.15, -0.011), (0.0, -0.16)], "yz", x - 0.01, x + 0.01, ah)
    kutu((0.62, 0.14, 0.002), (0, -0.10, 0.012), dan)                     # dantel
    kutu((0.62, 0.002, 0.035), (0, -0.182, -0.005), dan)
    for i in range(12):                                                    # dantelin fisto uçları
        prizma([(-0.31 + i * 0.0517, -0.0225), (-0.31 + (i + 1) * 0.0517, -0.0225), (-0.31 + (i + 0.5) * 0.0517, -0.04)], "xz", -0.181, -0.183, dan)
    t = 0.013
    # Çoban kız biblosu.
    koni(0.035, 0.012, 0.09, (-0.27, -0.10, t + 0.045), pastel, 20)
    sil(0.011, 0.03, (-0.27, -0.10, t + 0.105), pastel, 12)
    kure(0.013, (-0.27, -0.10, t + 0.132), ten)
    sil(0.022, 0.003, (-0.27, -0.10, t + 0.143), M("Biblo_Sapka", (0.70, 0.60, 0.30), 0.3), 16)
    cubuk((-0.25, -0.12, t), (-0.245, -0.12, t + 0.13), 0.002, alt)        # çoban değneği
    # Kuğu çifti.
    for dx, a in ((-0.10, 0.0), (-0.03, math.pi)):
        bas = len(P)
        kure(0.025, (0, 0, 0.018), por, (1.0, 1.6, 0.8))
        yay_cubuk((0, -0.028, 0.03), 0.02, -0.5, 2.2, 0.006, por, 5, duzlem="yz")
        kure(0.009, (0, -0.02 + 0.02 * math.cos(2.2) - 0.008, 0.03 + 0.02 * math.sin(2.2)), por)
        kure(0.004, (0, -0.02 + 0.02 * math.cos(2.2) - 0.018, 0.03 + 0.02 * math.sin(2.2) - 0.002), M("Gaga", (0.6, 0.2, 0.02), 0.3), seg=8, halka=6)
        grupla(bas, (dx, -0.09, t), (0, 0, a + 0.5))
    # Oturan köpek.
    kure(0.028, (0.10, -0.09, t + 0.028), por, (0.9, 1.1, 1.1))
    kure(0.02, (0.10, -0.11, t + 0.07), por)
    for s in (-1, 1):
        kure(0.008, (0.10 + s * 0.017, -0.105, t + 0.075), M("Kopek_Leke", (0.20, 0.10, 0.04), 0.2), (0.6, 0.6, 1.4))
    kure(0.01, (0.085, -0.075, t + 0.04), M("Kopek_Leke", (0.20, 0.10, 0.04), 0.2))
    # Küçük vazo ve plastik çiçek.
    kure(0.035, (0.26, -0.09, t + 0.035), M("Vazo_Mavi", (0.04, 0.10, 0.35), 0.15), seg=18, halka=10)
    sil(0.014, 0.05, (0.26, -0.09, t + 0.085), M("Vazo_Mavi", (0.04, 0.10, 0.35), 0.15), 14)
    sil(0.02, 0.008, (0.26, -0.09, t + 0.11), alt, 16)
    sil(0.0355, 0.006, (0.26, -0.09, t + 0.035), alt, 18)
    cubuk((0.26, -0.09, t + 0.10), (0.27, -0.10, t + 0.22), 0.003, M("Cicek_Sap", (0.05, 0.20, 0.05), 0.5), 6)
    kure(0.02, (0.27, -0.10, t + 0.225), M("Cicek_Pembe", (0.60, 0.12, 0.25), 0.5), (1, 1, 0.6))
    return bitir("Duvar_Rafi_K3", "duvar")


def mama():
    boru, pembe, beyaz = mt("beyaz_plastik"), mt("pembe_plastik"), M("Mama_Beyaz", (0.82, 0.82, 0.80), 0.3)
    alt = {(sx, sy): (sx * 0.21, sy * 0.23, 0.012) for sx in (-1, 1) for sy in (-1, 1)}
    ust = {(sx, sy): (sx * 0.15, sy * 0.13, 0.52) for sx in (-1, 1) for sy in (-1, 1)}
    for k in alt:                                                          # açılı metal ayaklar
        cubuk(alt[k], ust[k], 0.012, M("Mama_Boru", (0.75, 0.75, 0.74), 0.3, 0.3), 10)
        kure(0.016, (alt[k][0], alt[k][1], 0.0096), mt("siyah_plastik"), (1, 1, 0.6))
    lerp = lambda a, b, t: tuple(a[i] + (b[i] - a[i]) * t for i in range(3))
    for sy in (-1, 1):                                                     # yan ayaklar arasında destek
        cubuk(lerp(alt[(-1, sy)], ust[(-1, sy)], 0.3), lerp(alt[(1, sy)], ust[(1, sy)], 0.3), 0.009, M("Mama_Boru", (0.75, 0.75, 0.74), 0.3, 0.3))
    kutu((0.36, 0.34, 0.03), (0, 0, 0.525), beyaz, 0.01)                  # oturak kabuğu
    kutu((0.33, 0.30, 0.05), (0, 0.005, 0.555), pembe, 0.02)              # minder
    s = kutu((0.34, 0.04, 0.38), (0, 0.17, 0.76), beyaz, 0.015)           # sırt
    don(s, rx=-0.14)
    s = kutu((0.30, 0.03, 0.32), (0, 0.14, 0.76), pembe, 0.012)
    don(s, rx=-0.14)
    for sx in (-1, 1):                                                     # kolluklar
        cubuk((sx * 0.17, 0.14, 0.67), (sx * 0.17, -0.12, 0.67), 0.01, beyaz)
        cubuk((sx * 0.17, -0.12, 0.67), (sx * 0.16, -0.12, 0.55), 0.01, beyaz)
    kutu((0.44, 0.24, 0.018), (0, -0.21, 0.685), beyaz, 0.008)            # tabla
    for (w, d, x, y) in ((0.44, 0.015, 0, -0.325), (0.44, 0.015, 0, -0.095), (0.015, 0.24, -0.215, -0.21), (0.015, 0.24, 0.215, -0.21)):
        kutu((w, d, 0.018), (x, y, 0.70), pembe, 0.005)
    kutu((0.26, 0.07, 0.02), (0, -0.195, 0.25), pembe, 0.006)             # ayaklık
    for sx in (-1, 1):
        cubuk((sx * 0.13, -0.195, 0.25), (sx * 0.18, -0.20, 0.25), 0.008, beyaz)
    # Tablada plastik kâse ve kaşık.
    k = koni(0.03, 0.045, 0.035, (0.08, -0.22, 0.712), M("Mama_Kase", (0.75, 0.52, 0.04), 0.35), 20)
    ortak.oy(k, ortak.silindir("Oy", 0.038, 0.04, (0.08, -0.22, 0.72 + 0.008), mt("sut"), 20))
    kutu((0.10, 0.012, 0.004), (-0.07, -0.23, 0.696), M("Mama_Kasik", (0.04, 0.15, 0.45), 0.35), 0.002)
    return bitir("Mama_Sandalyesi_K3")


def masa():
    ah, mus, beyaz = mt("ahsap_orta"), M("Musamba_Mavi", (0.04, 0.12, 0.36), 0.3), M("Musamba_Cicek", (0.80, 0.80, 0.76), 0.3)
    sil(0.45, 0.03, (0, 0, 0.735), ah, 40)
    for x in (-0.21, 0.21):
        for y in (-0.21, 0.21):
            koni(0.016, 0.022, 0.72, (x, y, 0.36), ah, 12)
    for s in (-1, 1):
        kutu((0.44, 0.02, 0.06), (0, s * 0.21, 0.69), ah, 0.004)
        kutu((0.02, 0.44, 0.06), (s * 0.21, 0, 0.69), ah, 0.004)
    sil(0.47, 0.004, (0, 0, 0.752), mus, 48)                              # muşamba üst
    e = sil(0.472, 0.11, (0, 0, 0.697), mus, 48)                          # sarkan etek
    ortak.oy(e, ortak.silindir("Oy", 0.466, 0.13, (0, 0, 0.697), mus, 48))
    for i in range(-4, 5):                                                 # beyaz çiçek benekleri
        for j in range(-4, 5):
            x, y = i * 0.1 + (0.05 if j % 2 else 0), j * 0.1
            if x * x + y * y < 0.43 ** 2:
                sil(0.012, 0.001, (x, y, 0.7545), beyaz, 8)
    for i in range(20):                                                    # etekteki benekler: önde kurup çevir
        a = i * math.pi / 10
        bas = len(P)
        dik_disk(0.012, 0.001, (0, -0.4735, 0.70), beyaz, 8)
        grupla(bas, (0, 0, 0), (0, 0, a))
    T = 0.754
    # Meyve kâsesi (beyaz, mavi kenarlı) ve meyveler.
    k = koni(0.06, 0.13, 0.07, (-0.10, 0.06, T + 0.035), mt("porselen"), 32)
    ortak.oy(k, ortak.silindir("Oy", 0.115, 0.07, (-0.10, 0.06, T + 0.035 + 0.012), mt("porselen"), 32))
    sil(0.131, 0.008, (-0.10, 0.06, T + 0.066), M("Kase_Mavi", (0.04, 0.10, 0.40), 0.2), 32)
    elma, port = M("Elma", (0.35, 0.03, 0.02), 0.4), M("Portakal", (0.60, 0.20, 0.02), 0.6)
    for (x, y, z, m, r) in ((-0.14, 0.04, 0.045, elma, 0.036), (-0.07, 0.10, 0.045, elma, 0.036), (-0.06, 0.02, 0.047, port, 0.038),
                            (-0.13, 0.11, 0.05, port, 0.038), (-0.10, 0.06, 0.09, elma, 0.035)):
        kure(r, (x, y, T + z), m, seg=16, halka=10)
    ban = M("Muz", (0.70, 0.50, 0.05), 0.5)
    for i in range(4):                                                     # kavisli muz
        a0, a1 = 0.3 + i * 0.35, 0.3 + (i + 1) * 0.35
        cubuk((-0.10 + 0.09 * math.cos(a0) - 0.02, 0.06 + 0.02, T + 0.07 + 0.05 * math.sin(a0)),
              (-0.10 + 0.09 * math.cos(a1) - 0.02, 0.06 + 0.02, T + 0.07 + 0.05 * math.sin(a1)), 0.016, ban, 8)
    # Şekerlik: kapaklı porselen, kenarında çay kaşığı.
    sil(0.04, 0.06, (0.18, -0.12, T + 0.03), mt("porselen"), 20, 0.006)
    sil(0.043, 0.008, (0.18, -0.12, T + 0.064), mt("porselen"), 20)
    kure(0.01, (0.18, -0.12, T + 0.075), M("Sekerlik_Yaldiz", (0.60, 0.42, 0.10), 0.3, 0.9))
    sil(0.0405, 0.008, (0.18, -0.12, T + 0.03), M("Kase_Mavi", (0.04, 0.10, 0.40), 0.2), 20)
    don(kutu((0.11, 0.008, 0.003), (0.26, -0.14, T + 0.0015), mt("celik")), rz=0.4)
    return bitir("Masa_K3")


def sandalye():
    mv = mt("mavi_boya")
    kutu((0.42, 0.40, 0.03), (0, -0.02, 0.45), mv, 0.008)
    for x in (-0.18, 0.18):
        kutu((0.035, 0.035, 0.435), (x, -0.19, 0.2175), mv, 0.005)
        kutu((0.035, 0.035, 0.45), (x, 0.16, 0.225), mv, 0.005)
        cubuk((x, 0.16, 0.44), (x, 0.20, 0.90), 0.019, mv, 8)               # geriye yatık arka dikme
        kutu((0.02, 0.33, 0.02), (x, -0.015, 0.15), mv, 0.003)
    kutu((0.34, 0.02, 0.02), (0, -0.19, 0.20), mv, 0.003)
    kutu((0.34, 0.02, 0.02), (0, 0.16, 0.20), mv, 0.003)
    tr = kutu((0.42, 0.035, 0.08), (0, 0.197, 0.86), mv, 0.012)           # üst kavisli çıta
    for i in range(4):
        x = -0.105 + i * 0.07
        cubuk((x, 0.165, 0.465), (x, 0.195, 0.82), 0.009, mv, 8)
    kutu((0.36, 0.028, 0.03), (0, 0.18, 0.62), mv, 0.006)
    # Aşınmış oturak ortası (boyası solmuş).
    elips(0.24, 0.20, 0.001, (0, -0.03, 0.4655), M("Boya_Asinmis", (0.10, 0.20, 0.38), 0.6), 16)
    return bitir("Sandalye_K3")


def buzdolabi():
    krem, gri = mt("krem_emaye"), M("Buz_Gri", (0.30, 0.28, 0.24), 0.5)
    kol, conta = M("Buz_Kol", (0.30, 0.26, 0.18), 0.35), M("Buz_Conta", (0.06, 0.06, 0.06), 0.6)
    kutu((0.60, 0.57, 1.60), (0, 0.025, 0.80), krem, 0.02)
    kutu((0.595, 0.04, 0.42), (0, -0.28, 1.385), krem, 0.012)             # dondurucu kapısı
    kutu((0.595, 0.04, 1.10), (0, -0.28, 0.625), krem, 0.012)             # soğutucu kapısı
    kutu((0.60, 0.035, 0.012), (0, -0.265, 1.17), conta)                  # kapı arası çizgi
    kutu((0.58, 0.03, 0.07), (0, -0.255, 0.035), gri, 0.005)              # tekme ızgarası
    for i in range(5):
        kutu((0.50, 0.004, 0.006), (0, -0.271, 0.015 + i * 0.012), conta)
    kutu((0.03, 0.03, 0.18), (-0.26, -0.315, 1.30), kol, 0.01)            # kollar (menteşe sağda)
    kutu((0.03, 0.03, 0.22), (-0.26, -0.315, 1.02), kol, 0.01)
    kutu((0.10, 0.004, 0.022), (0.18, -0.3015, 1.52), mt("krom"), 0.003)  # marka plakası
    # Kapıda mıknatısla tutturulmuş çocuk resimleri, harf mıknatısları.
    for (x, z, tur, a, tohum) in ((-0.05, 0.98, "ev", 0.06, 1), (0.13, 0.72, "aile", -0.05, 2), (-0.08, 0.60, "gunes", 0.03, 3)):
        bas = kagit(tur, "duvar", tohum)
        grupla(bas, (x, -0.3002, z), (0, -a, 0), (0.62, 1, 0.62))
    mk = [M("Miknatis_%d" % i, c, 0.35) for i, c in enumerate(((0.45, 0.03, 0.02), (0.04, 0.12, 0.40), (0.75, 0.52, 0.04), (0.05, 0.28, 0.06)))]
    for i, (x, z) in enumerate(((-0.05, 1.045), (0.13, 0.785), (-0.08, 0.69), (0.20, 0.99))):
        dik_disk(0.014, 0.008, (x, -0.305, z), mk[i % 4], 14)
    for i in range(5):                                                     # harf mıknatısları
        don(kutu((0.025, 0.008, 0.03), (0.06 + i * 0.032, -0.304, 0.45 + (i % 2) * 0.01), mk[i % 4], 0.004), ry=0.1 * (i % 3 - 1))
    return bitir("Buzdolabi_K3")


def ocak():
    be, siyah, krom = mt("beyaz_emaye"), M("Ocak_Siyah", (0.02, 0.02, 0.02), 0.45), mt("krom")
    cam = M("Firin_Cami", (0.03, 0.03, 0.035), 0.08)
    kutu((0.60, 0.60, 0.85), (0, 0, 0.425), be, 0.01)
    kutu((0.56, 0.025, 0.46), (0, -0.31, 0.39), be, 0.01)                 # fırın kapağı
    kutu((0.40, 0.004, 0.22), (0, -0.3245, 0.42), cam, 0.004)
    cubuk((-0.22, -0.345, 0.64), (0.22, -0.345, 0.64), 0.009, krom, 12)
    for x in (-0.22, 0.22):
        cubuk((x, -0.345, 0.64), (x, -0.32, 0.64), 0.007, krom)
    kutu((0.56, 0.025, 0.12), (0, -0.31, 0.08), be, 0.008)                # alt çekmece
    kutu((0.10, 0.012, 0.012), (0, -0.325, 0.12), krom, 0.003)
    kutu((0.60, 0.03, 0.08), (0, -0.305, 0.795), M("Ocak_Panel", (0.62, 0.62, 0.60), 0.3), 0.005)
    for i in range(5):
        dik_disk(0.02, 0.028, (-0.22 + i * 0.11, -0.33, 0.795), siyah, 16)
    dik_disk(0.005, 0.004, (0.26, -0.321, 0.815), M("Ocak_Lamba", (0.5, 0.05, 0.02), 0.3), 10)
    kutu((0.60, 0.03, 0.08), (0, 0.285, 0.89), be, 0.008)                 # arka sıçrama kenarı
    for (x, y, r) in ((-0.15, -0.13, 0.045), (0.15, -0.13, 0.035), (-0.15, 0.13, 0.035), (0.15, 0.13, 0.05)):
        sil(r + 0.012, 0.008, (x, y, 0.854), krom, 20)
        sil(r, 0.014, (x, y, 0.863), siyah, 20)
        for a in (0.0, math.pi / 2):
            don(kutu((0.19, 0.012, 0.012), (x, y, 0.876), siyah), rz=a + math.pi / 4)
    # Süt tenceresi (alüminyum, uzun saplı): ön sol gözde.
    tx, ty, tz = -0.15, -0.13, 0.882
    t = sil(0.065, 0.09, (tx, ty, tz + 0.045), mt("celik"), 24)
    ortak.oy(t, ortak.silindir("Oy", 0.061, 0.09, (tx, ty, tz + 0.049), mt("celik"), 24))
    sil(0.06, 0.004, (tx, ty, tz + 0.04), mt("sut"), 24)
    cubuk((tx - 0.05, ty - 0.045, tz + 0.075), (tx - 0.13, ty - 0.16, tz + 0.09), 0.009, siyah)
    # İki katlı çaydanlık: arka sağ gözde.
    cx, cy, cz = 0.15, 0.13, 0.882
    sil(0.085, 0.13, (cx, cy, cz + 0.065), mt("celik"), 28, 0.02)
    cubuk((cx - 0.07, cy - 0.02, cz + 0.04), (cx - 0.14, cy - 0.06, cz + 0.12), 0.012, mt("celik"))
    yay_cubuk((cx + 0.085, cy, cz + 0.08), 0.045, -1.2, 1.2, 0.007, siyah, 6)
    sil(0.05, 0.02, (cx, cy, cz + 0.14), mt("celik"), 20)
    sil(0.06, 0.08, (cx, cy, cz + 0.19), mt("celik"), 24, 0.015)
    cubuk((cx - 0.05, cy - 0.01, cz + 0.17), (cx - 0.10, cy - 0.04, cz + 0.22), 0.007, mt("celik"))
    kure(0.012, (cx, cy, cz + 0.238), siyah)
    yay_cubuk((cx + 0.06, cy, cz + 0.19), 0.03, -1.2, 1.2, 0.005, siyah, 5)
    return bitir("Ocak_K3")


def camasir():
    be, gri = mt("beyaz_emaye"), M("Makine_Gri", (0.45, 0.45, 0.45), 0.35)
    cam = M("Makine_Cami", (0.04, 0.055, 0.065), 0.06)
    kutu((0.60, 0.55, 0.85), (0, 0, 0.425), be, 0.015)
    kutu((0.60, 0.02, 0.12), (0, -0.285, 0.77), M("Makine_Panel", (0.72, 0.72, 0.70), 0.3), 0.006)
    kutu((0.18, 0.02, 0.08), (-0.18, -0.297, 0.77), be, 0.006)            # deterjan çekmecesi
    kutu((0.06, 0.01, 0.015), (-0.18, -0.308, 0.75), gri, 0.003)
    dik_disk(0.035, 0.03, (0.17, -0.30, 0.77), gri, 24)                   # program düğmesi
    kutu((0.006, 0.01, 0.03), (0.17, -0.316, 0.785), mt("siyah_plastik"))
    for x in (0.02, 0.07):
        kutu((0.03, 0.012, 0.02), (x, -0.298, 0.77), gri, 0.003)
    dik_disk(0.005, 0.004, (0.25, -0.297, 0.795), M("Makine_Lamba", (0.5, 0.05, 0.02), 0.3), 10)
    dik_disk(0.175, 0.03, (0, -0.29, 0.43), gri, 40)                      # kapak halkası
    dik_disk(0.13, 0.012, (0, -0.308, 0.43), cam, 36)
    dik_disk(0.10, 0.004, (0, -0.3145, 0.43), M("Makine_Ic", (0.20, 0.20, 0.22), 0.3, 0.8), 32)
    kutu((0.03, 0.02, 0.07), (0.15, -0.31, 0.43), mt("siyah_plastik"), 0.006)
    kutu((0.58, 0.01, 0.07), (0, -0.28, 0.05), be, 0.004)
    kutu((0.12, 0.004, 0.05), (0.20, -0.2855, 0.05), gri, 0.003)          # servis kapağı
    # Üstte çamaşır sepeti (açık yeşil plastik) ve taşan çamaşırlar.
    sep = M("Sepet_Yesil", (0.20, 0.40, 0.25), 0.4)
    s = kutu((0.46, 0.34, 0.22), (0, 0.02, 0.96), sep, 0.01)
    ortak.oy(s, ortak.kutu("Oy", (0.43, 0.31, 0.22), (0, 0.02, 0.975), sep))
    for i in range(6):                                                     # yan yarıklar
        kutu((0.04, 0.345, 0.012), (-0.18 + i * 0.07, 0.02, 0.93), M("Sepet_Golge", (0.08, 0.16, 0.10), 0.5))
    cam_renk = [M("Camasir_%d" % i, c, 0.95) for i, c in enumerate(((0.70, 0.35, 0.45), (0.25, 0.45, 0.70), (0.80, 0.78, 0.70), (0.40, 0.05, 0.04), (0.70, 0.60, 0.20)))]
    for i, (x, y, z, w, d, h) in enumerate(((-0.10, 0.0, 1.07, 0.24, 0.20, 0.08), (0.10, 0.06, 1.08, 0.22, 0.18, 0.09), (0.02, -0.04, 1.12, 0.20, 0.16, 0.06),
                                            (-0.12, 0.10, 1.10, 0.14, 0.12, 0.05), (0.14, -0.07, 1.10, 0.10, 0.10, 0.05))):
        don(kutu((w, d, h), (x, y, z), cam_renk[i], 0.03), rz=0.3 * i)
    kutu((0.08, 0.03, 0.26), (0.24, -0.02, 0.93), cam_renk[3], 0.015)     # kenardan sarkan kazak kolu
    return bitir("Camasir_Makinesi_K3")


def biberon():
    gov, olc = M("Biberon_Govde", (0.70, 0.72, 0.72), 0.15), M("Biberon_Cizgi", (0.05, 0.10, 0.35), 0.4)
    emzik = M("Emzik", (0.55, 0.38, 0.20), 0.45)
    for (x, y, halka, dolu) in ((-0.07, 0.00, M("Biberon_Mavi", (0.05, 0.20, 0.55), 0.3), 0.08), (-0.01, 0.05, M("Biberon_Pembe", (0.72, 0.28, 0.38), 0.3), 0.04)):
        sil(0.03, 0.15, (x, y, 0.075), gov, 20)
        sil(0.0302, dolu, (x, y, dolu / 2 + 0.002), mt("sut"), 20)           # içindeki süt
        for i in range(5):
            kutu((0.012, 0.002, 0.0015), (x, y - 0.0302, 0.03 + i * 0.022), olc)
        sil(0.033, 0.022, (x, y, 0.161), halka, 20)
        koni(0.016, 0.007, 0.03, (x, y, 0.187), emzik, 14)
        kure(0.007, (x, y, 0.203), emzik, seg=10, halka=6)
    # Mama tenekesi ve ölçü kaşığı.
    sil(0.055, 0.14, (0.08, 0.02, 0.07), M("Teneke_Govde", (0.72, 0.72, 0.68), 0.35, 0.2), 28)
    sil(0.0555, 0.07, (0.08, 0.02, 0.075), M("Teneke_Etiket", (0.10, 0.25, 0.60), 0.5), 28)
    sil(0.0557, 0.02, (0.08, 0.02, 0.075), M("Teneke_Serit", (0.75, 0.55, 0.05), 0.5), 28)
    sil(0.058, 0.018, (0.08, 0.02, 0.149), M("Teneke_Kapak", (0.75, 0.52, 0.04), 0.35), 28)
    k = sil(0.016, 0.018, (0.02, -0.07, 0.009), M("Olcu_Kasigi", (0.80, 0.80, 0.78), 0.3), 14)
    ortak.oy(k, ortak.silindir("Oy", 0.013, 0.02, (0.02, -0.07, 0.013), mt("sut"), 14))
    cubuk((0.036, -0.07, 0.01), (0.09, -0.08, 0.004), 0.004, M("Olcu_Kasigi", (0.80, 0.80, 0.78), 0.3))
    return bitir("Biberon_Takimi_K3")


ESYALAR = {
    "askilik": askilik, "ayakkabi": ayakkabi_yigini, "canta": canta, "bebek_arabasi": bebek_arabasi, "terlik": terlikler,
    "cizimler": cizimler, "oyuncak": oyuncaklar, "yolluk": yolluk,
    "tv": tv, "boyama": boyama, "park": park, "hali": hali, "perde": perde, "raf": raf,
    "mama": mama, "masa": masa, "sandalye": sandalye, "buzdolabi": buzdolabi, "ocak": ocak, "camasir": camasir, "biberon": biberon,
}

if __name__ == "__main__":
    hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "askilik"
    ortak.sahneyi_temizle()
    ESYALAR[hangi]()
