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
#   bebek_arabasi  Bebek_Arabasi_K3    : duvara dayalı, gerçek boyda körüklü bebek arabası.
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
#   kanepe         Kanepe_K3           : ceviz iskeletli, çizgili dokuma döşemeli üçlü kanepe; gevşek sırt minderleri,
#                                        çökmüş orta oturak, sırtında dantel. Altı yerden 18 cm açık (çizim parçası).
# MUTFAK
#   mama           Mama_Sandalyesi_K3  : torna ayaklı ahşap mama sandalyesi; vinil minder, ahşap tabla, sarkan önlük.
#   masa           Masa_K3             : krom ayaklı kare mutfak masası, kırmızı pötikare muşamba; meyve kâsesi ve şekerlik.
#   sandalye       Sandalye_K3         : mavi boyalı ahşap mutfak sandalyesi.
#   buzdolabi      Buzdolabi_K3        : krem, üstü dondurucu iki kapılı buzdolabı; kapıda mıknatıslı resimler (tek parça).
#   ocak           Ocak_K3             : beyaz dört gözlü fırınlı ocak; süt tenceresi ve iki katlı çaydanlık.
#   camasir        Camasir_Makinesi_K3 : önden yüklemeli beyaz çamaşır makinesi; üstünde dolu çamaşır sepeti.
#   biberon        Biberon_Takimi_K3   : iki biberon ve bir mama tenekesi (tezgâh üstü).
# ODA B (yatak odası eşyalarının çoğu kat3_esyalar_2.py'de)
#   cocuksandalye  Cocuk_Sandalyesi_K3 : yeşil boyalı çelik borulu, kontrplak oturaklı çocuk/okul sandalyesi.
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

# ---------- YUMUŞAK ŞEKİLLER (askılıktaki giysiler, ayakkabılar, okul çantası, bebek arabası) ----------
# İlk sürümde bu dört eşya kutu ve silindirlerden kuruluydu: paltolar tahta levha, ayakkabılar tuğla,
# çanta kitap, bebek arabası el arabası gibi görünüyordu. Kumaş, deri ve lastik "yumuşak" malzemelerdir;
# kenarları yuvarlak, yüzeyleri hafif şişkin ya da sarkıktır. Aşağıdaki dört yardımcı bunu sağlar:
#   egri    : birkaç kontrol noktasından geçen yumuşak bir yol (sarkan kol, kayış, sap);
#   supur   : bir kesiti (daire, yassı şerit) bu yol boyunca sürükleyip boru ya da kayış yapar;
#   yastik  : köşeleri yuvarlak, içi sık noktalı bir kutu; her noktası bir "biçim" fonksiyonuyla
#             istenen yere itilir (paltonun omzu, ayakkabının burnu, çantanın sarkması);
#   oval_halka: köşeleri yuvarlatılmış dikdörtgen bir halkanın noktaları (kenar biyesi, ağız kenarı).

def egri(kontrol, n=4):
    """
    Catmull-Rom eğrisi: verilen kontrol noktalarının HEPSİNDEN geçen yumuşak bir yol.
    n: iki kontrol noktası arasına kaç ara nokta düşeceği (çok olursa üçgen sayısı artar).
    Her ara nokta, çevresindeki dört kontrol noktasının ağırlıklı ortalamasıdır; ağırlıklar t ile
    yumuşakça değiştiği için yolda kırık köşe oluşmaz.
    """
    K = [Vector(p) for p in kontrol]                      # hesaplanabilsin diye her noktayı vektöre çevir
    if len(K) < 3:                                        # iki noktada eğilecek bir şey yok: düz çizgi
        return K
    yol = []
    for i in range(len(K) - 1):                           # her ardışık nokta çifti bir aralık
        p1, p2 = K[i], K[i + 1]                           # aralığın iki ucu
        p0 = K[i - 1] if i > 0 else p1 * 2 - p2           # öncesi yoksa uzantısını uydur
        p3 = K[i + 2] if i + 2 < len(K) else p2 * 2 - p1  # sonrası yoksa uzantısını uydur
        for k in range(n):
            t = k / n                                     # aralık içindeki konum (0 başta, 1 sonda)
            yol.append(0.5 * (2 * p1 + (p2 - p0) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t
                              + (3 * p1 - p0 - 3 * p2 + p3) * t ** 3))
    yol.append(K[-1])                                     # son noktayı da ekle, yol tam orada bitsin
    return yol


def daire_kesit(r, n=8):
    """supur için dairesel kesit: yarıçapı r, n köşeli (boru, kol, lastik)."""
    return [(r * math.cos(2 * math.pi * i / n), r * math.sin(2 * math.pi * i / n)) for i in range(n)]


def serit_kesit(en, kal):
    """supur için yassı dikdörtgen kesit (kayış, bant): en yanlara, kal kalınlık yönüne."""
    return [(-en / 2, -kal / 2), (en / 2, -kal / 2), (en / 2, kal / 2), (-en / 2, kal / 2)]


def supur(noktalar, kesit, mat, yukari=(0, 0, 1), olcek=None, kapali=False, kapak=True):
    """
    Kesiti yol boyunca sürükleyerek boru/kayış yapar (Blender'daki "sweep" işleminin elle yapılışı).
    Yolun her noktasında üç yön hesaplanır: T yolun gidiş yönü, N "yukari"nın T'ye dik kalan kısmı
    (kesitin ikinci ekseni, yassı kayışta kalınlık yönü), W ikisine dik yan yön (kesitin birinci ekseni).
    olcek: her nokta için kesit çarpanı (tek sayı ya da (yan, kalınlık) çifti): kol bileğe doğru incelir.
    kapali: yol kendi üstüne kapanır (lastik, halka); kapak: açık yolun iki ucu kapatılsın mı.
    """
    pts = [Vector(p) for p in noktalar]                   # yol noktaları
    m, k = len(pts), len(kesit)
    up = Vector(yukari).normalized()                      # referans "yukarı" yön
    bm = bmesh.new()                                      # boş bir mesh taslağı
    halkalar = []                                         # her yol noktasındaki kesit köşeleri
    for i, p in enumerate(pts):
        if kapali:                                        # kapalı yolda komşular baştan sona dolanır
            T = pts[(i + 1) % m] - pts[i - 1]
        else:                                             # açık yolda uçlarda tek komşu kullanılır
            T = pts[min(i + 1, m - 1)] - pts[max(i - 1, 0)]
        T.normalize()
        N = up - T * up.dot(T)                            # yukarının, gidişe dik kalan parçası
        if N.length < 1e-6:                               # yol tam "yukarı"ya gidiyorsa başka bir dik seç
            N = Vector((1, 0, 0)) - T * T.x
        N.normalize()
        W = N.cross(T)                                    # üçüncü yön: ikisine de dik
        s = olcek[i] if olcek else 1.0                    # bu noktadaki kesit çarpanı
        sa, sb = (s[0], s[1]) if isinstance(s, (tuple, list)) else (s, s)
        halkalar.append([bm.verts.new(p + W * (a * sa) + N * (b * sb)) for a, b in kesit])
    for i in range(m if kapali else m - 1):               # ardışık iki halkayı dörtgenlerle bağla
        r0, r1 = halkalar[i], halkalar[(i + 1) % m]
        for j in range(k):
            bm.faces.new((r0[j], r0[(j + 1) % k], r1[(j + 1) % k], r1[j]))
    if kapak and not kapali:                              # açık borunun iki ağzını kapat
        bm.faces.new(halkalar[0])
        bm.faces.new(halkalar[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)     # bütün yüzler dışa baksın
    me = bpy.data.meshes.new("Supur")
    bm.to_mesh(me)
    bm.free()
    n = bpy.data.objects.new("Supur", me)                 # mesh'i sahnede bir nesneye bağla
    bpy.context.collection.objects.link(n)
    ortak.ata(n, mat)
    return ekle(n)


def _kafes(bol):
    """
    Her yüzü ızgara gibi bölünmüş 1 m'lik küp (bmesh). bol = (x, y, z) yönünde kaç parça.
    Neden: yuvarlatma ve biçimlendirme ancak nokta olan yerde yapılabilir; düz bir küpün yalnız
    sekiz köşesi vardır. Bir yöne daha çok bölme vermek, o yönde daha çok eğilebilmek demek.
    """
    bm = bmesh.new()

    def izgara(sabit, deger, a, na, b, nb):
        """sabit eksende deger konumunda, a ve b eksenleri boyunca na x nb parçalı bir yüz ızgarası."""
        vs = {}
        for i in range(na + 1):
            for j in range(nb + 1):
                p = [0.0, 0.0, 0.0]
                p[sabit], p[a], p[b] = deger, -0.5 + i / na, -0.5 + j / nb
                vs[i, j] = bm.verts.new(p)
        for i in range(na):
            for j in range(nb):
                bm.faces.new((vs[i, j], vs[i + 1, j], vs[i + 1, j + 1], vs[i, j + 1]))

    nx, ny, nz = bol
    for d in (-0.5, 0.5):                                 # karşılıklı iki yüz: alt/üst, ön/arka, sol/sağ
        izgara(2, d, 0, nx, 1, ny)
        izgara(1, d, 0, nx, 2, nz)
        izgara(0, d, 1, ny, 2, nz)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)   # yüzlerin kenarda çakışan noktalarını kaynat
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return bm


def yastik(olcu, konum, mat, bicim=None, bol=(2, 2, 2), seviye=1):
    """
    Köşeleri yuvarlak, yumuşak kutu. olcu: son sınır kutusu (x, y, z); konum: merkezi.
    1) Izgaralı küp kurulur, 2) "Subdivision Surface" (alt bölümleme) ile köşeleri yuvarlatılır:
       Blender her dörtgeni dörde böler ve köşeleri komşularının ortalamasına çeker, kutu yastık olur;
    3) sınır kutusu tam olcu'ya getirilir (yuvarlatma küçülttüğü için yeniden ölçeklenir);
    4) bicim(x, y, z) verilmişse her nokta (merkeze göre, konum eklenmeden) oraya itilir.
       bicim mutlak konum döndürüyorsa konum (0, 0, 0) verilir.
    """
    bm = _kafes(bol)
    me = bpy.data.meshes.new("Yastik")
    bm.to_mesh(me)
    bm.free()
    n = bpy.data.objects.new("Yastik", me)
    bpy.context.collection.objects.link(n)
    if seviye:
        mod = n.modifiers.new("Yuvarla", "SUBSURF")       # alt bölümleme değiştiricisi
        mod.levels = seviye                               # kaç kez bölünsün (1: yumuşak, 2: daha yumuşak)
        mod.render_levels = seviye
        sec(n)                                            # modifier_apply etkin nesneye uygulanır
        bpy.ops.object.modifier_apply(modifier=mod.name)  # sonucu kalıcı meshe işle
    vs = n.data.vertices
    lo = [min(v.co[i] for v in vs) for i in range(3)]     # şu anki sınır kutusu
    hi = [max(v.co[i] for v in vs) for i in range(3)]
    for v in vs:
        p = [(v.co[i] - (lo[i] + hi[i]) / 2) / (hi[i] - lo[i]) * olcu[i] for i in range(3)]   # tam olcu'ya getir
        if bicim:
            p = bicim(p[0], p[1], p[2])                   # şekillendir
        v.co = Vector(p) + Vector(konum)                  # yerine koy
    n.data.update()
    ortak.ata(n, mat)
    return ekle(n)


def oval_halka(cx, cy, a, b, z, n=24, us=4.0):
    """
    (cx, cy) merkezli, a x b yarı ölçülü, köşeleri yuvarlak dikdörtgen halka noktaları (süperelips).
    us büyüdükçe köşeler keskinleşir (2: elips). z bir sayı ya da z(x, y) fonksiyonu olabilir.
    """
    pts = []
    for i in range(n):
        q = 2 * math.pi * i / n
        c, s = math.cos(q), math.sin(q)
        x = cx + a * math.copysign(abs(c) ** (2 / us), c)
        y = cy + b * math.copysign(abs(s) ** (2 / us), s)
        pts.append((x, y, z(x, y) if callable(z) else z))
    return pts


# =====================================================================================
# ANTRE: ASKILIK
# =====================================================================================

def palto(x, ust, omuz, alt, boy, y0, y1, mat, ust_z=0.02, kapson=False, serit=None, geri=(False, False)):
    """
    Kancadan asılı giysi. x: kancanın x'i; ust: kancadaki (yaka) eni; omuz: omuz eni; alt: etek eni;
    boy: kancadan eteğe; y0: sırtın (duvar tarafı) y'si, y1: önün y'si; ust_z: tepe yüksekliği.
    Kancaya asılan palto askıdaki gibi dik durmaz: yakası kancada toplanır, omuzları kolların ağırlığıyla
    aşağı düşer (çadır gibi), kollar yanlarda sarkar, etek hafif dalgalanır. kapson: yağmurluk kapşonundan
    asılır, gövde kapşonun altından başlar. serit: kolların bilek üstüne reflektör bandı malzemesi.
    geri: (sol kol, sağ kol); True olan kol öndeki paltonun arkasına sıkışmış: sırta yakın ve yassı
    sarkar. Yoksa öndeki paltonun içinden sivri bir kama gibi dışarı taşıyordu.
    """
    kal = y0 - y1                                         # gövdenin en kalın yerdeki kalınlığı
    ic = mt("ic_koyu")
    z_alt = ust_z - boy                                   # eteğin yüksekliği
    z_ust = ust_z - (0.10 if kapson else 0.0)             # gövdenin tepesi (kapşonluda kapşonun altı)
    gboy = z_ust - z_alt                                  # gövdenin kendi boyu

    def yarim_en(t):
        """t: 0 etek, 1 yaka. Etekten omza hafif daralır; omuzdan yakaya yuvarlak bir eğimle toplanır."""
        if t < 0.80:
            return alt / 2 + (omuz / 2 - alt / 2) * (t / 0.80)
        u = (t - 0.80) / 0.20
        return ust / 2 + (omuz / 2 - ust / 2) * math.cos(u * math.pi / 2) ** 0.7

    def derinlik(t):
        """Gövdenin kalınlığı: göğüste en kalın, etekte ve kancada incelir (kumaş orada yassılaşır)."""
        if t < 0.80:
            return kal * (0.62 + 0.38 * math.sin(t / 0.80 * math.pi / 2))
        return kal * (1.0 - 0.35 * (t - 0.80) / 0.20)

    def bicim(px, py, pz):
        """Yastığın her noktasını palto biçimine iter (mutlak konum döner)."""
        t = pz / gboy + 0.5                               # 0 etek, 1 tepe
        xn = px / (alt / 2)                               # -1 sol kenar, +1 sağ kenar
        on = (kal / 2 - py) / kal                         # 0 sırt (duvara yaslı, düz), 1 ön yüz
        yy = y0 - on * derinlik(t)                        # sırt y0'da sabit, ön yüz derinlik kadar önde
        yy -= 0.010 * math.sin(xn * 2.5 * math.pi + 0.7) * (1 - t) ** 1.5 * on   # etekte dikey kumaş kıvrımları
        zz = z_alt + t * gboy
        zz += 0.008 * math.cos(xn * 2.0 * math.pi) * max(0.0, 0.08 - t) / 0.08   # etek ucu hafif dalgalı
        return (x + xn * yarim_en(t), yy, zz)

    yastik((alt, kal, gboy), (0, 0, 0), mat, bicim, bol=(5, 1, 6))
    on_nokta = lambda xx, t: Vector(bicim(xx, -kal / 2, (t - 0.5) * gboy))   # ön yüzeyde, gövdeye yapışık nokta

    # Kollar: omuzdan sarkan, bileğe doğru incelen, hafif öne kıvrılmış borular.
    # Kesit 11 köşeli: bitir() 35°'den keskin kenarları sert bırakır; 8 köşede yüzler arası 45° olduğu
    # için kol köşeli görünüyordu, 11 köşede 33° kalır ve yumuşak gölgelenir.
    to = 0.80                                             # omuzun gövdedeki yüksekliği (t)
    zs = z_alt + to * gboy
    kr = min(0.05, derinlik(to) * 0.55)                   # kolun kalınlığı
    kl = boy * 0.60                                       # kol boyu
    kesit = daire_kesit(1.0, 11)
    for s in (-1, 1):
        sikis = geri[0 if s < 0 else 1]                   # bu kol öndeki paltonun arkasında mı
        ym = (y0 - kr * 0.45) if sikis else (y0 - derinlik(to) * 0.5 - 0.01)   # kolun ekseni
        one = 0.0 if sikis else 1.0                       # sıkışan kol öne kıvrılmaz
        xs = x + s * (yarim_en(to) - 0.035)               # omuz ucunun biraz içi
        yol = egri([(xs - s * 0.012, ym, zs + 0.035),     # omuz başı (dikişin üstü)
                    (xs + s * 0.020, ym - 0.008 * one, zs - 0.03),
                    (xs + s * 0.030, ym - 0.018 * one, zs - kl * 0.40),
                    (xs + s * 0.022, ym - 0.026 * one, zs - kl * 0.75),
                    (xs + s * 0.012, ym - 0.030 * one, zs - kl)], 2)
        m = len(yol)
        yas = 0.45 if sikis else 1.0                      # sıkışan kol y'de yassı
        olc = [(kr * 0.9 * (1 - 0.22 * i / (m - 1)), kr * yas * (1 - 0.22 * i / (m - 1))) for i in range(m)]   # bileğe doğru incel
        supur(yol, kesit, mat, yukari=(0, -1, 0), olcek=olc)
        # Kol ağzı: bilekte az kalın bir manşet, içinde koyu boşluk (kol içi boş görünsün).
        supur(yol[-2:], kesit, mat, yukari=(0, -1, 0), olcek=[(o[0] * 1.08, o[1] * 1.08) for o in olc[-2:]])
        uc = yol[-1] + (yol[-1] - yol[-2]).normalized() * 0.004
        supur([yol[-2], uc], kesit, ic, yukari=(0, -1, 0), olcek=[(o[0] * 0.8, o[1] * 0.8) for o in olc[-2:]])
        if serit is not None:                             # reflektör bandı: kolun dörtte üçünde halka
            i0 = int(m * 0.70)
            supur(yol[i0:i0 + 2], kesit, serit, yukari=(0, -1, 0),
                  olcek=[(o[0] * 1.06, o[1] * 1.06) for o in olc[i0:i0 + 2]])

    # Ön pat (düğmelerin dikildiği şerit): ön yüzeyin tam ortasını izleyen ince bant.
    pat = [on_nokta(0.0, t) + Vector((0, -0.002, 0)) for t in (0.02, 0.25, 0.5, 0.7)]
    supur(pat, serit_kesit(0.016, 0.004), mat, yukari=(0, -1, 0))
    for i, t in enumerate((0.66, 0.50, 0.34, 0.18)[:4 if boy > 0.7 else 3]):   # düğmeler (kapşonluda geçme)
        p = on_nokta(0.0, t) + Vector((0, -0.004, 0))
        if kapson:
            kutu((0.026, 0.008, 0.008), tuple(p), ic, 0.002)          # yağmurluğun siyah geçme tokaları
        else:
            dik_disk(0.010, 0.006, tuple(p), ic, 8)
    for s in (-1, 1):                                     # cep kapakları (ön yüzeye oturur)
        p = on_nokta(s * alt * 0.24, 0.30) + Vector((0, -0.004, 0))
        kutu((0.10, 0.008, 0.028), tuple(p), mat, 0.004)

    if kapson:
        # Kapşon: kancada tepesi toplanmış, aşağıda genişleyen bir kese; önünde yüzün gireceği koyu ağız.
        kb = 0.17                                         # kapşonun boyu
        kd = kal * 1.35                                   # kapşon gövdeden kalın (içi hava dolu)

        def kbicim(px, py, pz):
            tt = pz / kb + 0.5                            # 0 alt, 1 kanca
            on2 = (kal / 2 - py) / kal
            return (x + px * (1 - 0.55 * tt * tt),        # tepeye doğru daralır
                    y0 - 0.005 - on2 * kd * (0.75 + 0.25 * math.sin(math.pi * (1 - tt))),
                    ust_z - kb + tt * kb)

        yastik((0.19, kal, kb), (0, 0, 0), mat, kbicim, bol=(3, 1, 3))
        agiz = [(x + 0.055 * math.cos(q), y0 - 0.005 - kd * 0.98, ust_z - 0.11 + 0.042 * math.sin(q))
                for q in (2 * math.pi * i / 12 for i in range(12))]
        yastik((0.10, 0.006, 0.075), (x, y0 - 0.005 - kd * 0.97, ust_z - 0.11), ic, bol=(2, 1, 2))   # yüz ağzı
        supur(agiz, daire_kesit(0.007, 5), mat, yukari=(0, -1, 0), kapali=True)                        # ağız kenarı
    else:
        # Yaka: iki yana açılmış klapa ve ense tarafında yuvarlak katlanmış yaka.
        yf = on_nokta(0.0, 0.92).y                        # yakanın oturduğu ön yüzey
        for s in (-1, 1):
            prizma([(x - s * 0.012, z_ust - 0.27), (x - s * ust * 0.72, z_ust - 0.10),
                    (x - s * ust * 0.55, z_ust - 0.03), (x - s * ust * 0.22, z_ust - 0.05)],
                   "xz", yf + 0.004, yf - 0.010, mat)
        yaka = egri([(x - ust * 0.55, yf - 0.006, z_ust - 0.035), (x - ust * 0.35, (y0 + yf) / 2, z_ust + 0.004),
                     (x, y0 - kal * 0.35, z_ust + 0.012), (x + ust * 0.35, (y0 + yf) / 2, z_ust + 0.004),
                     (x + ust * 0.55, yf - 0.006, z_ust - 0.035)], 2)
        supur(yaka, daire_kesit(1.0, 8), mat, olcek=[(0.022, 0.014)] * len(yaka), yukari=(0, 0, 1))
        # Askı ilmeği: yakanın içinden kancaya geçen küçük kumaş halka.
        ilmek = [(x + 0.012 * math.cos(q), -0.058, ust_z + 0.004 + 0.012 * math.sin(q))
                 for q in (2 * math.pi * i / 10 for i in range(10))]
        supur(ilmek, daire_kesit(0.003, 4), ic, yukari=(0, 1, 0), kapali=True)


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
    # Annenin bej yün paltosu, babanın koyu lacivert paltosu (bej olanın önünde), Kerem'in sarı yağmurluğu
    # kapşonundan en öne asılmış; kollarında gri reflektör bandı.
    palto(-0.30, 0.20, 0.40, 0.46, 1.00, -0.030, -0.125, M("Palto_Bej", (0.42, 0.33, 0.21), 0.9), geri=(False, True))
    palto(0.02, 0.20, 0.42, 0.48, 0.98, -0.050, -0.150, M("Palto_Koyu", (0.025, 0.03, 0.045), 0.9))
    palto(0.22, 0.14, 0.26, 0.30, 0.58, -0.160, -0.215, M("Yagmurluk_Sari", (0.75, 0.50, 0.02), 0.35),
          kapson=True, serit=M("Reflektor", (0.45, 0.45, 0.45), 0.3))
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


# =====================================================================================
# ANTRE: AYAKKABILAR
# =====================================================================================

def ayakkabi(tur, konum, aci=0.0, yan=False):
    """
    Tek ayakkabı; burnu -y'de, tabanı z = 0'da kurulur, sonra çevrilip yerleştirilir. yan: yan yatmış.
    Her ayakkabı aynı dört parçadan yapılır: taban (tabanın ayak izi biçimi), saya (üst deri; burunda alçak,
    bileğe doğru yükselir), ağız (ayağın girdiği koyu boşluk ve çevresindeki dolgulu kenar) ve türüne özgü
    ayrıntı (bağcık, topuk, cırt bant, konç). Burun ucu yerden hafif kalkar ("burun kalkışı"): gerçek
    ayakkabının en tanıdık çizgisi budur, dümdüz oturan ayakkabı tuğla gibi görünür.
    """
    bas = len(P)
    ic = mt("ic_koyu")
    # L, W: boy ve en; th: taban kalınlığı; yay: burnun kalkışı; topuk: topuğun yerden yüksekliği;
    # hb, hs, hk: sayanın burunda, bağcık bölgesinde ve topukta tabandan yüksekliği;
    # a0, a1: ağzın boyuna başı ve sonu (t: -0,5 burun ucu, +0,5 topuk ucu); ae: ağzın eni (tabana oranla).
    if tur == "erkek":
        L, W, th, yay, topuk = 0.29, 0.105, 0.012, 0.014, 0.016
        hb, hs, hk, a0, a1, ae = 0.040, 0.070, 0.064, 0.14, 0.47, 0.62
        taban_m, yuz_m = M("Taban_Koyu", (0.03, 0.025, 0.02), 0.7), M("Deri_Kahve", (0.07, 0.03, 0.012), 0.35)
    elif tur == "kadin":
        L, W, th, yay, topuk = 0.245, 0.078, 0.006, 0.006, 0.055
        hb, hs, hk, a0, a1, ae = 0.030, 0.016, 0.040, -0.10, 0.47, 0.74
        taban_m, yuz_m = M("Taban_Koyu", (0.03, 0.025, 0.02), 0.7), M("Deri_Siyah", (0.022, 0.018, 0.02), 0.22)
    elif tur == "spor":
        L, W, th, yay, topuk = 0.19, 0.072, 0.018, 0.010, 0.0
        hb, hs, hk, a0, a1, ae = 0.034, 0.054, 0.052, 0.13, 0.46, 0.60
        taban_m, yuz_m = mt("beyaz_plastik"), M("Spor_Mavi", (0.04, 0.10, 0.35), 0.6)
    else:  # cizme: çocuk yağmur çizmesi; ağzı konçun tepesinde
        L, W, th, yay, topuk = 0.18, 0.07, 0.014, 0.008, 0.004
        hb, hs, hk, a0, a1, ae = 0.040, 0.066, 0.068, 0.0, 0.0, 0.0
        taban_m, yuz_m = M("Cizme_Taban", (0.55, 0.40, 0.03), 0.5), M("Cizme_Yesil", (0.04, 0.22, 0.05), 0.35)

    def eni(t):
        """Tabanın t noktasındaki yarı eni (W/2'ye oranla): yuvarlak burun, geniş parmak kökü, dar bel, topuk."""
        if t < -0.2:
            return 0.42 + 0.58 * math.sqrt(max(0.0, 1 - ((t + 0.2) / 0.3) ** 2))
        if t < 0.15:
            return 1.0 - 0.24 * (t + 0.2) / 0.35
        if t < 0.38:
            return 0.76 + 0.10 * (t - 0.15) / 0.23
        return 0.86 * (0.55 + 0.45 * math.sqrt(max(0.0, 1 - ((t - 0.38) / 0.12) ** 2)))

    def kalk(t):
        """O noktanın yerden kalkışı: burun ucu yukarı kıvrılır, topuklu ayakkabıda topuk yükselir."""
        k = yay * max(0.0, (-t - 0.18) / 0.32) ** 2
        u = min(1.0, max(0.0, (t + 0.05) / 0.35))
        return k + topuk * u * u * (3 - 2 * u)             # yumuşak geçiş (smoothstep)

    def yuz(t):
        """Sayanın tabandan yüksekliği: burun ucunda alçak, burun kutusu, sonra bileğe doğru yükselir."""
        if t < -0.25:
            return hb * (0.30 + 0.70 * math.sqrt(max(0.0, 1 - ((t + 0.25) / 0.25) ** 2)))
        if t < 0.10:
            return hb + (hs - hb) * (t + 0.25) / 0.35
        return hs + (hk - hs) * (t - 0.10) / 0.40

    def tepe(t):
        """Sayanın ortasındaki tepe yüksekliği (bağcık, ağız, cırt bant buna oturur)."""
        return th * 0.7 + yuz(t) + kalk(t)

    def ic_disk(kenar, mat):
        """
        Kenar noktalarından ince, kapalı bir levha (ağzın koyu içi): üst yüz noktaların kendi
        yüksekliğinde, alt yüz 2 mm aşağıda; ortadaki bir noktadan yelpaze gibi üçgenlenir.
        Noktalar farklı yükseklikte olabilir: iskarpinin ağzı topuğa doğru yükselir, levha da onu izler.
        """
        bm = bmesh.new()
        ust_h = [bm.verts.new(p) for p in kenar]                                  # üst kenar
        alt_h = [bm.verts.new((p[0], p[1], p[2] - 0.002)) for p in kenar]         # alt kenar
        orta = Vector([sum(p[i] for p in kenar) / len(kenar) for i in range(3)])  # kenarın ortası
        m0, m1 = bm.verts.new(orta), bm.verts.new(orta - Vector((0, 0, 0.002)))
        for i in range(len(kenar)):
            j = (i + 1) % len(kenar)
            bm.faces.new((m0, ust_h[i], ust_h[j]))
            bm.faces.new((m1, alt_h[j], alt_h[i]))
            bm.faces.new((ust_h[i], alt_h[i], alt_h[j], ust_h[j]))
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        me = bpy.data.meshes.new("IcDisk")
        bm.to_mesh(me)
        bm.free()
        n = bpy.data.objects.new("IcDisk", me)
        bpy.context.collection.objects.link(n)
        ortak.ata(n, mat)
        return ekle(n)

    # Taban ve saya: aynı ayak izi biçimi, taban altta ince, saya üstünde yüksek. Izgara az bölmeli
    # tutuluyor: sekiz ayakkabı tek modelde, her biri birkaç yüz üçgeni geçmemeli.
    yastik((W, L, th), (0, 0, 0), taban_m,
           lambda px, py, pz: (px * eni(py / L), py, pz + th / 2 + kalk(py / L)), bol=(1, 4, 1))
    yastik((W * 0.95, L * 0.975, 1.0), (0, 0, 0), yuz_m,
           lambda px, py, pz: (px * eni(py / L), py, th * 0.7 + (pz + 0.5) * yuz(py / L) + kalk(py / L)), bol=(2, 4, 2))

    if a1 > a0:
        # Ağız: sayanın üstüne oturan koyu iç (astar ve iç taban) ve çevresinde dolgulu yaka kenarı.
        tc, tl = (a0 + a1) / 2, (a1 - a0) / 2
        kenar, ic_kenar = [], []
        for i in range(12):
            q = 2 * math.pi * i / 12
            t = tc + tl * math.cos(q)
            kenar.append((math.sin(q) * ae * eni(t) * W * 0.46, t * L, tepe(t) + 0.002))
            ic_kenar.append((math.sin(q) * ae * eni(t) * W * 0.44, (tc + tl * 0.96 * math.cos(q)) * L, tepe(t) + 0.003))
        ic_disk(ic_kenar, ic)
        supur(kenar, daire_kesit(0.0045 if tur != "kadin" else 0.0025, 4), yuz_m, kapali=True)

    if tur == "erkek":
        # Topuk: tabanın altında ayrı, koyu bir blok (ayakkabı topukla parmak kökü üstünde durur).
        yastik((W * 0.78, L * 0.22, topuk + th), (0, 0, 0), taban_m,
               lambda px, py, pz: (px * eni(0.39), py + L * 0.385, pz + (topuk + th) / 2), bol=(1, 1, 1))
        # Dil ve çapraz bağcıklar (bağcık bölgesi t = -0,20 .. +0,10).
        dil = yastik((W * 0.42, L * 0.16, 0.008), (0, 0.09 * L, tepe(0.10) + 0.006), yuz_m, bol=(1, 1, 1))
        don(dil, rx=-0.35)
        bag = M("Bagcik", (0.02, 0.015, 0.01), 0.8)
        ts = [-0.20 + 0.075 * i for i in range(5)]
        for i in range(4):
            e = W * 0.16
            for s in (-1, 1):
                cubuk((s * e, ts[i] * L, tepe(ts[i]) + 0.002), (-s * e, ts[i + 1] * L, tepe(ts[i + 1]) + 0.002), 0.0022, bag, 4)
        for s in (-1, 1):                                  # fiyonk: iki ilmek, iki sarkan uç
            supur([(0, ts[-1] * L, tepe(ts[-1]) + 0.003), (s * 0.02, ts[-1] * L + 0.008, tepe(ts[-1]) + 0.012),
                   (s * 0.03, ts[-1] * L - 0.004, tepe(ts[-1]) + 0.006), (0, ts[-1] * L, tepe(ts[-1]) + 0.003)],
                  daire_kesit(0.0022, 4), bag, kapali=True)
            cubuk((0, ts[-1] * L, tepe(ts[-1]) + 0.003), (s * 0.028, ts[-1] * L - 0.03, tepe(ts[-1] - 0.1) - 0.004), 0.0022, bag, 4)
    elif tur == "kadin":
        # İnce topuk: yukarıda geniş, aşağı incelen bir takoz; ucunda kauçuk kapak.
        tp = kalk(0.42)                                    # topuğun taban altındaki yüksekliği
        yastik((0.028, 0.032, tp), (0, 0, 0), taban_m,
               lambda px, py, pz: (px * (0.55 + 0.45 * (pz / tp + 0.5)), py * (0.6 + 0.4 * (pz / tp + 0.5)) + L * 0.42,
                                   pz + tp / 2), bol=(1, 1, 2))
        kutu((0.016, 0.02, 0.006), (0, L * 0.42, 0.003), mt("siyah_plastik"), 0.002)
        # Burunda küçük, parlak bir toka (80-90'ların iskarpini).
        kutu((0.02, 0.006, 0.012), (0, -0.14 * L, tepe(-0.14) - 0.004), mt("krom"), 0.002)
    elif tur == "spor":
        # Beyaz kauçuk burun kapağı, iki cırt bant ve yanlarda kırmızı çizgiler (90'ların çocuk spor ayakkabısı).
        yastik((W * 0.98, L * 0.20, 0.028), (0, 0, 0), taban_m,
               lambda px, py, pz: (px * eni(py / L - 0.40), py - L * 0.40, pz + th + 0.004 + kalk(py / L - 0.40)), bol=(1, 2, 1))
        for t in (-0.14, 0.00):
            hw = eni(t) * W * 0.48
            yol = [(-hw * 1.02, t * L, th + 0.004), (-hw * 0.8, t * L, tepe(t) - 0.010), (0, t * L, tepe(t) + 0.002),
                   (hw * 0.8, t * L, tepe(t) - 0.010), (hw * 1.02, t * L, th + 0.004)]
            supur(egri(yol, 2), serit_kesit(0.020, 0.004), mt("beyaz_plastik"), yukari=(0, 0, 1))
        kir = M("Spor_Kirmizi", (0.45, 0.03, 0.02), 0.6)
        for s in (-1, 1):
            for d in (0.0, 0.035):
                yol = [(s * (eni(t) * W * 0.476 + 0.001), t * L, th + 0.010 + (tepe(t) - th - 0.02) * (t + 0.10 - d) / 0.42)
                       for t in (-0.10 + d, 0.05 + d, 0.22 + d)]
                supur(yol, serit_kesit(0.008, 0.002), kir, yukari=(s, 0, 0))
    else:
        # Konç: topuk üstünden yükselen, hafif arkaya yatık, ağza doğru genişleyen oval boru.
        yol = [(0, 0.20 * L, 0.03), (0, 0.23 * L, 0.11), (0, 0.26 * L, 0.195)]
        supur(yol, daire_kesit(1.0, 10), yuz_m, olcek=[(0.033, 0.040), (0.034, 0.042), (0.038, 0.046)], yukari=(0, -1, 0))
        ust = Vector(yol[-1])
        ic_disk([(ust.x + 0.035 * math.cos(q), ust.y + 0.043 * math.sin(q), ust.z + 0.003)
                 for q in (2 * math.pi * i / 12 for i in range(12))], ic)                        # konç içi (koyu)
        supur([(ust.x + 0.039 * math.cos(q), ust.y + 0.047 * math.sin(q), ust.z) for q in (2 * math.pi * i / 12 for i in range(12))],
              serit_kesit(0.004, 0.012), M("Cizme_Sari", (0.70, 0.48, 0.03), 0.4), yukari=(0, 0, 1), kapali=True)   # sarı ağız bandı
        supur(egri([(0, ust.y + 0.044, ust.z - 0.02), (0, ust.y + 0.058, ust.z - 0.005), (0, ust.y + 0.05, ust.z + 0.012)], 2),
              serit_kesit(0.016, 0.004), M("Cizme_Sari", (0.70, 0.48, 0.03), 0.4), yukari=(0, 1, 0))   # arkada çekme kulağı

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


# =====================================================================================
# ANTRE: OKUL ÇANTASI
# =====================================================================================

def canta():
    kir, lac = M("Canta_Kirmizi", (0.32, 0.03, 0.025), 0.55), M("Canta_Lacivert", (0.02, 0.04, 0.14), 0.6)
    met, ic = mt("krom"), mt("ic_koyu")
    # Sırtüstü yere bırakılmış 90'lar ilkokul çantası. Koordinatlar: çantanın sırtı yerde (z = 0), önü
    # yukarıda; ağzı (üst ucu) +y'de, dibi -y'de. Kapağı ağzın arkasından menteşeli: açılıp yere yatmış.
    # Gövde: yuvarlak köşeli, yanları şişkin; ağız tarafında içi boşaldığı için üstü hafifçe çökmüş.
    GY, GZ = 0.165, 0.12                                   # gövdenin yarı boyu (y) ve yüksekliği (z)

    def gbicim(px, py, pz):
        zz = pz / GZ + 0.5                                 # 0 sırt (yer), 1 ön yüz
        yn = py / GY                                       # -1 dip, +1 ağız
        sis = 1 + 0.045 * (1 - yn * yn) * (1 - abs(zz - 0.5) * 2)          # yanlar ortada şişkin
        cok = 0.016 * zz * max(0.0, (yn - 0.55) / 0.45) ** 2                 # ağız tarafı çökük
        return (px * sis, py, pz + GZ / 2 - cok)

    govde = yastik((0.31, 2 * GY, GZ), (0, 0, 0), kir, gbicim, bol=(4, 4, 3))
    for z, zo in ((0.108, 0.93), (0.012, 0.97)):           # ön ve arka kenar biyesi (lacivert şerit)
        supur(oval_halka(0, 0, 0.155 * zo * 1.02, GY * 0.985, z, n=24, us=6.0), daire_kesit(0.005, 4), lac, kapali=True)
    # Ön cep: dipte, kendi küçük kapağı ve krom kilidiyle; üstünde yıldız çıkartma.
    yastik((0.19, 0.095, 0.030), (0, -0.105, 0.120), kir,
           lambda px, py, pz: (px, py, pz + 0.004 * (1 - (2 * px / 0.19) ** 2)), bol=(2, 2, 1))
    supur(oval_halka(0, -0.105, 0.095, 0.0475, 0.121, n=20, us=5.0), daire_kesit(0.0035, 4), lac, kapali=True)
    yastik((0.196, 0.046, 0.010), (0, -0.080, 0.137), kir, bol=(2, 2, 1))                    # cep kapağı
    kutu((0.03, 0.012, 0.008), (0, -0.103, 0.141), met, 0.002)                              # cep kilidi
    kutu((0.05, 0.03, 0.002), (0.05, -0.128, 0.1365), M("Canta_Yildiz", (0.70, 0.55, 0.05), 0.4))   # çıkartma
    for x in (-0.125, 0.125):                                                               # kapak kilitlerinin yuvası
        kutu((0.030, 0.034, 0.008), (x, -0.118, 0.117), met, 0.002)
        kutu((0.018, 0.010, 0.004), (x, -0.108, 0.122), ic)
    # Ağız: çantanın üst ucunda koyu açıklık; içinden defter kaymış.
    yastik((0.25, 0.03, 0.072), (0, 0.158, 0.060), ic, bol=(2, 1, 2))
    # Kapak: menteşe çizgisinden (y = 0,165) yere yatmış; ucu hafif kıvrık, uç köşeleri yuvarlak.
    def kbicim(px, py, pz):
        yy = py + 0.30
        dar = 1 - 0.14 * max(0.0, (yy - 0.39) / 0.045) ** 2          # uca doğru köşeler yuvarlanır
        kiv = 0.014 * max(0.0, (yy - 0.37) / 0.065) ** 2              # uç hafif yukarı kıvrık
        return (px * dar, yy, pz + 0.006 + kiv)

    yastik((0.30, 0.27, 0.012), (0, 0, 0), kir, kbicim, bol=(3, 3, 1))
    supur([(kbicim(px, py, 0.006)) for px, py in
           [(-0.15, -0.13), (-0.15, 0.06), (-0.145, 0.12), (-0.11, 0.134), (0.0, 0.135), (0.11, 0.134), (0.145, 0.12),
            (0.15, 0.06), (0.15, -0.13)]], daire_kesit(0.005, 5), lac)                     # kapak biyesi (U)
    kutu((0.07, 0.035, 0.002), (-0.085, 0.395, 0.0135), mt("kagit"))                        # isim etiketi
    for x in (-0.08, 0.08):                                                                 # kayışlar ve krom kilit dilleri
        supur([kbicim(x, py, 0.008) for py in (-0.03, 0.05, 0.10, 0.128)], serit_kesit(0.03, 0.004), lac)
        k = kbicim(x, 0.136, 0.010)
        kutu((0.034, 0.022, 0.008), (k[0], k[1] + 0.004, k[2]), met, 0.002)
    # Taşıma sapı: üst uçta sert plastik bir kulp, yatık çantada dışa doğru uzanıyor.
    supur(egri([(-0.05, 0.160, 0.092), (-0.045, 0.195, 0.095), (0.0, 0.212, 0.097), (0.045, 0.195, 0.095), (0.05, 0.160, 0.092)], 3),
          daire_kesit(1.0, 6), lac, olcek=[(0.011, 0.008)] * 13, yukari=(0, 0, 1))
    # Omuz kayışları sırtta, yani çantanın altında kalır. Sağdaki gevşemiş: üst köşeden çıkıp yerde bir
    # ilmek yaparak yeniden altına giriyor, üstünde krom ayar tokası. Soldakinin yalnız dipteki ucu görünür.
    # (İki kayış da yanlarda boydan boya uzanınca çanta mavi bir tepsiye oturmuş gibi görünüyordu.)
    supur(egri([(0.07, 0.150, 0.004), (0.135, 0.150, 0.004), (0.170, 0.095, 0.004), (0.160, 0.020, 0.004), (0.110, -0.010, 0.004)], 3),
          serit_kesit(0.032, 0.006), lac)
    kutu((0.036, 0.022, 0.009), (0.168, 0.085, 0.0065), met, 0.002)
    supur(egri([(-0.100, -0.110, 0.004), (-0.156, -0.128, 0.004), (-0.150, -0.158, 0.004)], 2),
          serit_kesit(0.032, 0.006), lac)
    # Ağızdan kaymış defter ve kalem kutusu, kapağın üstünde.
    d = kutu((0.20, 0.24, 0.008), (0.03, 0.27, 0.016), M("Defter_Mavi", (0.05, 0.12, 0.35), 0.7), 0.002)
    don(d, rz=0.18)
    kutu((0.19, 0.23, 0.004), (0.03, 0.27, 0.0135), mt("kagit"))
    kk = yastik((0.20, 0.06, 0.04), (-0.07, 0.22, 0.032), M("Kalem_Kutusu", (0.70, 0.45, 0.03), 0.5), bol=(2, 1, 1))
    don(kk, rz=-0.25)
    fer = kutu((0.19, 0.004, 0.004), (-0.07, 0.22, 0.052), met)
    don(fer, rz=-0.25)
    boya_kalemi(0.10, 0.36, 0.012, 0.7, mt("boya_kirmizi"))
    # Kerem'in çizim parçası (Unity'de CantaUstu3 = 0,124) gövdenin ön yüzüne konur: orada yüzey düz kalmalı.
    ust = max(v.co.z for v in govde.data.vertices if abs(v.co.x) < 0.03 and abs(v.co.y - 0.0625) < 0.03)
    BILGI.append("govde on yuzu parca noktasinda (pivotlamadan once x=0, y=0.0625) z=%.3f; on cep govde y -0.153..-0.058 (ust z=0.14); "
                 "son koordinat = bu degerler - PIVOT kaydirmasi" % ust)
    return bitir("Okul_Cantasi_K3")


# =====================================================================================
# ANTRE: BEBEK ARABASI
# =====================================================================================

def bebek_arabasi():
    """
    90'ların başının körüklü bebek arabası: derin bir sepet (gövde), katlanır körük, dört büyük telli
    tekerlek, yaylı şasi ve itme kolu. Antrede duvarın dibine park edilmiş: burnu duvarda (y = 0),
    kolu odaya (-y) bakıyor. Gerçek boyda (Şahan'ın isteği, #83): yaklaşık 0,50 m geniş, 0,96 m uzun,
    sapıyla 1,08 m yüksek; sepet 0,76 x 0,40 m. (İlk sürüm eski yerleşim ölçüsüne sığsın diye 0,34 x 0,45 m'ydi.)
    """
    krom, siyah = mt("krom"), mt("siyah_plastik")
    kumas = M("Araba_Kumas", (0.03, 0.05, 0.16), 0.8)
    serit = M("Araba_Serit", (0.40, 0.04, 0.03), 0.8)
    lastik = M("Araba_Lastik", (0.30, 0.30, 0.29), 0.7)
    beyaz = mt("kumas_beyaz")
    # Ölçüler: gövde y -0,10 (burun) .. -0,86 (baş), yükseklik 0,46-0,72; tekerlek yarıçapı 0,13;
    # tekerlek izi x ±0,25. Öndeki tekerleklerin önü duvara (y = 0) değer.
    GY0, GY1, GZ0, GZ1, GX = -0.100, -0.860, 0.46, 0.72, 0.200
    gy = (GY0 + GY1) / 2
    R, IZ, ON_AKS, ARKA_AKS = 0.130, 0.250, -0.140, -0.800

    # Gövde (sepet): tabana doğru daralan, tekne gibi yuvarlak bir kutu.
    def gbicim(px, py, pz):
        zz = pz / (GZ1 - GZ0) + 0.5                        # 0 dip, 1 ağız
        return (px * (0.86 + 0.14 * zz), py * (0.90 + 0.10 * zz) + gy, pz + (GZ0 + GZ1) / 2)

    yastik((2 * GX, GY0 - GY1, GZ1 - GZ0), (0, 0, 0), kumas, gbicim, bol=(4, 6, 3))
    supur(oval_halka(0, gy, GX * 0.985, (GY0 - GY1) / 2 * 0.985, GZ1 - 0.006, n=24, us=5.0),
          daire_kesit(0.008, 5), serit, kapali=True)                                        # ağız kenarı biyesi
    supur(oval_halka(0, gy, GX * 0.95, (GY0 - GY1) / 2 * 0.955, GZ0 + 0.10, n=20, us=5.0),
          daire_kesit(0.004, 4), serit, kapali=True)                                        # yan süs şeridi
    # Önlük: sepetin ayak yarısını örten kumaş; ucunda katlanmış beyaz battaniye kenarı, çıtçıtlar.
    # Önlük sepetin burnundan başlayıp boyunun yarısını örter (0,40 m); battaniye kenarı önlüğün ucunda.
    yastik((2 * GX * 0.96, 0.40, 0.024), (0, 0, 0), kumas,
           lambda px, py, pz: (px, py + GY0 - 0.20, pz + GZ1 + 0.006 + 0.016 * (1 - (px / GX) ** 2)), bol=(3, 3, 1))
    yastik((2 * GX * 0.92, 0.05, 0.030), (0, 0, 0), beyaz,
           lambda px, py, pz: (px, py + GY0 - 0.405, pz + GZ1 + 0.014 + 0.013 * (1 - (px / GX) ** 2)), bol=(3, 1, 1))
    for s in (-1, 1):
        for yy in (GY0 - 0.08, GY0 - 0.32):
            kure(0.008, (s * GX * 0.93, yy, GZ1 + 0.012), krom, seg=8, halka=5)
    yastik((2 * GX * 0.88, 0.30, 0.01), (0, GY1 + 0.17, GZ1 - 0.004), mt("ic_koyu"), bol=(2, 2, 1))   # körüğün altındaki karanlık iç

    # Körük: iki yandaki menteşeden açılan, pileli yarım kubbe. Menteşe ekseni x boyunca, ağız yüksekliğinde.
    yh, Rx, Ry, Rz, A1 = GY1 + 0.26, 0.208, 0.25, 0.32, 1.80   # menteşe başa 26 cm; körük sepetten biraz geniş

    def korukte(u, v):
        s, a = math.pi * u, A1 * v                         # s: yay boyunca (bir yandan öbür yana), a: açılma açısı
        k = 1 + 0.035 * abs(math.sin(v * 4.5 * math.pi))   # pileler: kumaş dört yerde kabarık
        return (Rx * math.cos(s), yh - Ry * math.sin(s) * math.cos(a) * k, GZ1 + Rz * math.sin(s) * math.sin(a) * k)

    bm = bmesh.new()                                       # körük kumaşını ızgara olarak kur
    nu, nv = 12, 8
    vs = [[bm.verts.new(korukte(i / nu, j / nv)) for j in range(nv + 1)] for i in range(nu + 1)]
    for i in range(nu):
        for j in range(nv):
            bm.faces.new((vs[i][j], vs[i + 1][j], vs[i + 1][j + 1], vs[i][j + 1]))
    me = bpy.data.meshes.new("Koruk")
    bm.to_mesh(me)
    bm.free()
    kor = bpy.data.objects.new("Koruk", me)
    bpy.context.collection.objects.link(kor)
    kal = kor.modifiers.new("Kalinlik", "SOLIDIFY")        # tek katlı yüzeye kumaş kalınlığı ver
    kal.thickness = 0.010
    kal.offset = 0.0                                       # kalınlık yüzeyin iki yanına eşit dağılsın
    ortak.ata(kor, kumas)
    ekle(kor)
    supur([korukte(i / 12, 1.0) for i in range(13)], daire_kesit(0.009, 5), serit, kapak=True)   # körük ağzı biyesi
    for j in (0.45, 0.72):                                                                     # iki krom körük teli
        supur([korukte(i / 12, j) for i in range(13)], daire_kesit(0.004, 4), krom)
    for s in (-1, 1):
        yatik_sil(0.028, 0.012, (s * (Rx + 0.006), yh, GZ1), "x", krom, 14)                   # körük menteşesi
        cubuk((s * (Rx + 0.008), yh, GZ1), (s * (Rx + 0.008), yh + 0.16, GZ1 + 0.03), 0.005, krom)   # katlama kolu

    # Tekerlekler: beyaz-gri lastik, krom jant ve sekiz tel.
    for yy in (ON_AKS, ARKA_AKS):
        for s in (-1, 1):
            xx = s * IZ
            supur([(xx, yy + (R - 0.013) * math.cos(q), R + (R - 0.013) * math.sin(q)) for q in (2 * math.pi * i / 20 for i in range(20))],
                  daire_kesit(0.013, 6), lastik, yukari=(1, 0, 0), kapali=True)
            supur([(xx, yy + (R - 0.03) * math.cos(q), R + (R - 0.03) * math.sin(q)) for q in (2 * math.pi * i / 16 for i in range(16))],
                  daire_kesit(0.004, 4), krom, yukari=(1, 0, 0), kapali=True)
            for i in range(10):
                q = 2 * math.pi * i / 10
                cubuk((xx, yy, R), (xx, yy + (R - 0.03) * math.cos(q), R + (R - 0.03) * math.sin(q)), 0.0022, krom, 4)
            yatik_sil(0.022, 0.034, (xx, yy, R), "x", krom, 10)                                # göbek
        cubuk((-IZ, yy, R), (IZ, yy, R), 0.008, krom, 8)                                       # dingil

    # Şasi: iki yanda tekerleklerden dik kalkıp gövdenin altında düz uzanan boru; bu düz kısmın üstünde
    # gövdeyi taşıyan dört helezon yay (yaylar raya oturur, tepesi gövdenin dibine girer; havada durmasın).
    RAY = 0.330                                            # rayın düz kısmının yüksekliği
    for s in (-1, 1):
        xx = s * 0.170
        supur(egri([(xx, ON_AKS, R), (xx, ON_AKS - 0.04, 0.27), (xx, ON_AKS - 0.11, RAY), (xx, gy, RAY + 0.006),
                    (xx, ARKA_AKS + 0.11, RAY), (xx, ARKA_AKS + 0.04, 0.27), (xx, ARKA_AKS, R)], 2),
              daire_kesit(0.011, 6), krom)
        for yy in (ON_AKS - 0.13, ARKA_AKS + 0.13):
            xy = s * 0.157                                 # yay rayın hemen içinde
            yay = [(xy + 0.02 * math.cos(q), yy + 0.02 * math.sin(q), RAY + 0.008 + (GZ0 + 0.02 - RAY - 0.008) * q / (2 * math.pi * 4))
                   for q in (2 * math.pi * 4 * i / 24 for i in range(25))]
            supur(yay, daire_kesit(0.004, 4), krom)
        cubuk((xx, ON_AKS, R), (IZ * s, ON_AKS, R), 0.007, krom, 8)
        cubuk((xx, ARKA_AKS, R), (IZ * s, ARKA_AKS, R), 0.007, krom, 8)
    # İtme kolu: arka şasiden kalkıp gövdenin dışından geçer; tepede siyah köpük tutamaklı yay biçimli sap.
    # Sepetin baş ucunun (GY1) arkasından yükselir; x ±0,215 sepetin 1,5 cm dışı.
    for s in (-1, 1):
        supur(egri([(s * 0.170, ARKA_AKS + 0.05, 0.29), (s * 0.215, GY1 + 0.02, 0.55), (s * 0.215, GY1 - 0.04, 0.82),
                    (s * 0.205, GY1 - 0.09, 1.03)], 3), daire_kesit(0.012, 6), krom)
    sap = egri([(-0.205, GY1 - 0.09, 1.03), (-0.17, GY1 - 0.095, 1.068), (0.0, GY1 - 0.10, 1.082),
                (0.17, GY1 - 0.095, 1.068), (0.205, GY1 - 0.09, 1.03)], 3)
    supur(sap, daire_kesit(0.012, 6), krom)
    supur(sap[2:-2], daire_kesit(0.019, 8), siyah)                                             # köpük tutamak
    for s in (-1, 1):
        yatik_sil(0.018, 0.026, (s * 0.215, GY1 - 0.01, 0.70), "x", siyah, 12)                 # katlama mafsalı
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


# ---------- 90'LAR YENİLEMESİ: ORTAK YARDIMCILAR (mama sandalyesi, masa, kanepe, çocuk sandalyesi) ----------
# Şahan bu dört eşyayı "fazla modern" buldu (pembe plastik mama sandalyesi, benekli yuvarlak masa, Poly Haven'ın
# siyah deri kanepesi, mavi plastik okul sandalyesi). Yerlerine 1994'te bir Türk evinde gerçekten bulunacak,
# yıpranmış eşyalar geldi. Aşağıdaki üç yardımcı bu dört modelin ortak ihtiyacı.

def uvle(olcek=1.0):
    """
    P'deki her parçaya "kutu izdüşümü" ile doku koordinatı (UV) verir.
    NEDEN: Unity içe aktarırken her eşyanın üstüne bir kir dokusu bindirir; doku, modelin UV'sine göre yerleşir.
    bmesh ile kurduğumuz parçaların (supur, yastik, prizma, örtü) hiç UV'si yoktur; birleşince bütün köşeleri
    UV'nin (0, 0) noktasına düşer ve kir o parçada tek renk bir leke olur.
    NASIL: her yüzün normali (yüzün baktığı yön) hangi eksene en yakınsa, yüz o eksene dik düzleme "gölgesi
    düşürülür": üste bakan yüz (x, y), yana bakan (y, z), öne bakan (x, z) alır. Birim metredir: kir dokusu
    bütün eşyalarda aynı ölçekte görünür (dairenin duvar ve döşemelerindeki "UV metre" kuralının aynısı).
    """
    for n in P:
        me = n.data
        if not me.uv_layers:                              # UV katmanı yoksa aç
            me.uv_layers.new(name="UVMap")
        me.uv_layers[0].name = "UVMap"                    # birleşirken katmanlar adla eşleşir: hepsi aynı adda
        uv = me.uv_layers[0].data
        dm = n.matrix_world                               # parçanın dünyadaki konumu (meshte olmayan kaydırma)
        for p in me.polygons:
            ax, ay, az = (abs(c) for c in p.normal)       # yüzün yönünün eksenlere yakınlığı
            for li in p.loop_indices:                     # yüzün her köşesi ("loop")
                q = dm @ me.vertices[me.loops[li].vertex_index].co
                if az >= ax and az >= ay:
                    uv[li].uv = (q.x * olcek, q.y * olcek)     # üst/alt yüz
                elif ax >= ay:
                    uv[li].uv = (q.y * olcek, q.z * olcek)     # sağ/sol yüz
                else:
                    uv[li].uv = (q.x * olcek, q.z * olcek)     # ön/arka yüz


def desenle(n, mats, kural):
    """
    Tek parçaya birden çok malzeme verir: her yüz, merkezinin dünyadaki konumuna göre kural(konum) ile seçilen
    malzemeyi alır. Çizgili döşemelik kumaş ve kareli muşamba böyle yapılır: doku resmi yerine yüzleri
    boyamak. Unity birleşik meshi malzeme başına bir alt parçaya (submesh) böler; ek doku dosyası gerekmez.
    """
    n.data.materials.clear()
    for m in mats:                                        # malzeme yuvaları: 0, 1, 2 ...
        n.data.materials.append(m)
    for p in n.data.polygons:
        p.material_index = kural(n.matrix_world @ p.center)
    return n


def torna(a, b, profil, mat, kose=10):
    """
    Tornada çekilmiş ahşap parça (mama sandalyesi ayağı, kanepe ayağı, parmaklık): a'dan b'ye düz bir eksen.
    profil: (t, r) çiftleri; t 0'da a ucu, 1'de b ucu; r o noktadaki yarıçap. Daire kesit eksen boyunca
    sürüklenir ve her noktada r kadar büyütülür (supur): halkalar, boğumlar, incelen uçlar tek parça olur.
    Aynı t iki kez yazılmamalı (supur gidiş yönünü komşu noktalardan hesaplar, sıfır uzunluk bozar).
    """
    A, B = Vector(a), Vector(b)
    yol = [A + (B - A) * t for t, r in profil]            # eksen üzerindeki noktalar
    return supur(yol, daire_kesit(1.0, kose), mat, yukari=(1, 0, 0), olcek=[r for t, r in profil])


def mama():
    # 90'ların ahşap mama sandalyesi: açılı dört torna ayak, ayaklık, vinil kaplı minder, çubuklu sırt,
    # kolluklara oturan çıkarılabilir ahşap tabla. Tablanın önünden havlu bir önlük sarkıyor.
    # Ölçüler eski plastik sandalyenin sınır kutusuyla aynı (0,45 x 0,58 x 0,95): Unity'deki yeri değişmesin.
    ah = M("Mama_Ahsap", (0.15, 0.075, 0.03), 0.45)                        # koyu cilalı kayın
    asinmis = M("Mama_Asinmis", (0.26, 0.15, 0.065), 0.7)                  # cilası gitmiş, biraz açık ahşap
    vinil = M("Mama_Vinil", (0.32, 0.13, 0.035), 0.38)                     # hardal-kahve vinil (suni deri)
    sunger = M("Mama_Sunger", (0.62, 0.50, 0.20), 0.9)                     # yırtıktan görünen sarı sünger
    alt = {(sx, sy): (sx * 0.20, sy * 0.21, 0.0) for sx in (-1, 1) for sy in (-1, 1)}     # ayak uçları (geniş)
    ust = {(sx, sy): (sx * 0.155, sy * 0.125, 0.53) for sx in (-1, 1) for sy in (-1, 1)}  # oturak altı (dar)
    lerp = lambda a, b, t: tuple(a[i] + (b[i] - a[i]) * t for i in range(3))              # a-b arası t oranında nokta
    # Ayak profili (alttan üste): ince uç, boğum halkası, uzun gövde, oturak altında ikinci halka.
    ayak = [(0.0, 0.016), (0.06, 0.018), (0.09, 0.024), (0.12, 0.017), (0.40, 0.019), (0.46, 0.021),
            (0.88, 0.022), (0.93, 0.026), (0.97, 0.019), (1.0, 0.019)]
    for k in alt:
        torna(alt[k], ust[k], ayak, ah)
    # Arka ayaklar oturağın üstünde sırt dikmesi olarak devam eder; geriye yatık, tepesinde topuz.
    for sx in (-1, 1):
        torna(ust[(sx, 1)], (sx * 0.15, 0.225, 0.93), [(0.0, 0.019), (0.5, 0.017), (0.9, 0.018), (1.0, 0.016)], ah)
        kure(0.021, (sx * 0.15, 0.226, 0.936), ah, (1, 1, 0.9), seg=10, halka=6)
    # Ayaklar arası çubuklar: yanlarda alçakta, arkada biraz yukarıda (torna, ortası kalın).
    cub = [(0.0, 0.010), (0.5, 0.013), (1.0, 0.010)]
    for sx in (-1, 1):
        torna(lerp(alt[(sx, -1)], ust[(sx, -1)], 0.28), lerp(alt[(sx, 1)], ust[(sx, 1)], 0.28), cub, ah, 8)
    torna(lerp(alt[(-1, 1)], ust[(-1, 1)], 0.42), lerp(alt[(1, 1)], ust[(1, 1)], 0.42), cub, ah, 8)
    # Ayaklık: ön ayakların önünde, iki küçük takozla ayaklara vidalı tahta.
    kutu((0.33, 0.075, 0.018), (0, -0.195, 0.25), ah, 0.004)
    for sx in (-1, 1):
        kutu((0.025, 0.05, 0.035), (sx * 0.165, -0.175, 0.228), ah, 0.003)
    elips(0.13, 0.05, 0.0012, (0.02, -0.195, 0.2596), asinmis, 14)         # basıla basıla cilası gitmiş orta
    # Oturak tahtası ve üstünde vinil minder (biyeli kenar, sol ön köşesi yırtık).
    kutu((0.35, 0.33, 0.025), (0, 0.0, 0.5425), ah, 0.005)

    def minder_bicim(px, py, pz):
        """Minderin üstü ortada hafif çökük (yıllarca oturulmuş)."""
        if pz > 0:
            pz -= 0.008 * (1 - (2 * px / 0.32) ** 2) * (1 - (2 * py / 0.30) ** 2)
        return (px, py, pz)

    yastik((0.32, 0.30, 0.04), (0, 0.005, 0.575), vinil, minder_bicim, bol=(3, 3, 1))
    supur(oval_halka(0, 0.005, 0.158, 0.148, 0.592, n=20, us=5.0), daire_kesit(0.0045, 5), vinil, kapali=True)  # biye
    kutu((0.05, 0.004, 0.012), (-0.10, -0.1465, 0.58), mt("ic_koyu"))     # yırtık dikiş (karanlık yarık)
    kure(0.014, (-0.10, -0.145, 0.581), sunger, (1.4, 0.6, 0.7), seg=8, halka=5)   # dışarı taşan sünger
    # Sırt: dikmeler arasında kavisli üst tahta, alt kayıt ve üç torna parmaklık.
    yol = [(x, 0.2125 + 0.02 * (1 - (x / 0.15) ** 2), 0.88) for x in (-0.15, -0.075, 0.0, 0.075, 0.15)]
    supur(yol, serit_kesit(0.018, 0.07), ah, yukari=(0, 0, 1))             # içe kavisli üst tahta
    kutu((0.29, 0.02, 0.03), (0, 0.17, 0.66), ah, 0.004)                   # alt kayıt
    for x in (-0.07, 0.0, 0.07):
        torna((x, 0.135, 0.555), (x, 0.205, 0.85), [(0.0, 0.009), (0.3, 0.012), (0.5, 0.009), (0.7, 0.012), (1.0, 0.009)], ah, 8)
    # Kolluklar: sırt dikmesinden öne uzanan yuvarlak uçlu tahta, önde bir torna dikme taşır.
    for sx in (-1, 1):
        kutu((0.04, 0.32, 0.022), (sx * 0.175, 0.0, 0.70), ah, 0.006)
        sil(0.02, 0.022, (sx * 0.175, -0.16, 0.70), ah, 12)
        torna((sx * 0.165, -0.13, 0.555), (sx * 0.175, -0.15, 0.69), [(0.0, 0.011), (0.5, 0.014), (1.0, 0.011)], ah, 8)
        kutu((0.03, 0.05, 0.0012), (sx * 0.175, -0.14, 0.7116), asinmis)   # el değen yerde cila aşınmış
    # Tabla: kolluklara oturan çerçeveli ahşap tepsi; önündeki kenarda diş izleri, ortasında kâse izi.
    T = 0.715                                                              # tabla üst yüzü
    kutu((0.44, 0.22, 0.015), (0, -0.22, T - 0.0075), ah, 0.003)
    for (w, d, x, y) in ((0.44, 0.014, 0, -0.323), (0.44, 0.014, 0, -0.117), (0.014, 0.22, -0.213, -0.22), (0.014, 0.22, 0.213, -0.22)):
        kutu((w, d, 0.014), (x, y, T + 0.007), ah, 0.003)
    for i in range(5):                                                     # diş izleri: ön kenarda küçük çentikler
        kutu((0.006, 0.004, 0.004), (-0.06 + i * 0.012 + (i % 2) * 0.003, -0.3285, T + 0.012), mt("ic_koyu"))
    elips(0.12, 0.10, 0.0008, (0.07, -0.215, T + 0.0004), asinmis, 16)    # tabaklar konup kalka aşınmış yer
    # Tablada emaye kâse (beyaz, mavi kenarlı, kenarı çatlamış) ve alüminyum kaşık.
    k = koni(0.03, 0.045, 0.035, (0.08, -0.22, T + 0.0175), mt("beyaz_emaye"), 20)
    ortak.oy(k, ortak.silindir("Oy", 0.038, 0.04, (0.08, -0.22, T + 0.0255), mt("sut"), 20))
    r = sil(0.0455, 0.004, (0.08, -0.22, T + 0.034), M("Emaye_Mavi", (0.03, 0.08, 0.30), 0.3), 20)
    ortak.oy(r, ortak.silindir("Oy", 0.040, 0.01, (0.08, -0.22, T + 0.034), mt("sut"), 20))   # yalnız kenar halkası kalsın
    dik_disk(0.004, 0.002, (0.08, -0.266, T + 0.022), mt("ic_koyu"), 8)   # emayesi atmış nokta
    don(kutu((0.10, 0.012, 0.004), (-0.07, -0.23, T + 0.002), M("Aluminyum", (0.55, 0.55, 0.53), 0.45, 0.8), 0.002), rz=0.25)
    # Önlük: tablanın üstünden ön kenara, oradan aşağı sarkan havlu; üstünde sarı ördek aplike ve mama lekesi.
    havlu = M("Onluk_Havlu", (0.30, 0.45, 0.60), 0.95)
    yol = egri([(-0.10, -0.17, T + 0.003), (-0.10, -0.29, T + 0.004), (-0.10, -0.334, T + 0.017),
                (-0.098, -0.338, T - 0.05), (-0.094, -0.336, T - 0.13)], 3)
    supur(yol, serit_kesit(0.17, 0.004), havlu, yukari=(0, -1, 1))
    supur(yol, serit_kesit(0.176, 0.0025), mt("kumas_beyaz"), yukari=(0, -1, 1))   # beyaz biye (kenarda taşar)
    dik_disk(0.026, 0.002, (-0.094, -0.3395, T - 0.085), mt("boya_sari"), 14)      # ördek aplikesi
    dik_disk(0.009, 0.002, (-0.076, -0.3405, T - 0.068), mt("boya_sari"), 10)      # ördeğin başı
    dik_disk(0.012, 0.001, (-0.12, -0.3395, T - 0.11), M("Mama_Leke", (0.30, 0.17, 0.05), 0.6), 10)
    uvle()
    return bitir("Mama_Sandalyesi_K3")


def musamba(W, T, sark, n_ic, mats, kal=0.0015):
    """
    Kare masaya serilmiş, kenarlardan sarkan kareli muşamba (tek parça, yüz yüz boyanmış).
    W: masa tablasının kenarı; T: muşambanın üst yüzü; sark: kenardan sarkan kumaş boyu; n_ic: üstte
    bir kenar boyunca kaç kare; mats: (beyaz, açık kırmızı, koyu kırmızı).
    Kumaş önce düz bir ızgara olarak düşünülür: (u, v) muşambanın serili hâlindeki koordinatı. Masanın
    üstüne düşen kısım düz kalır; dışarı taşan kısım (kenara uzaklığı d) önce 6 mm yarıçaplı bir yayla
    kenardan döner, sonra aşağı sarkar ve hafifçe dışa açılır. Köşelerde kumaş iki yöne birden taşar: köşe
    noktasının çevresinde dönerek sarkar, bu yüzden köşeler kenarlardan daha uzun sarkar (gerçek muşambadaki
    gibi) ve birkaç kıvrım yapar.
    Pötikare (pötikare = Fransızca "küçük kare"): dikey ve yatay açık kırmızı şeritler; ikisinin kesiştiği kare
    koyu kırmızı, hiçbirine düşmeyen kare beyaz kalır. Her ızgara hücresi bu kurala göre boyanır.
    """
    h = W / n_ic                                          # bir karenin kenarı
    k0 = 0.006                                            # kenardan dönüş yayının yarıçapı
    yay = k0 * math.pi / 2                                # yayın uzunluğu (çeyrek daire)
    # Izgara çizgileri: üstte her kare sınırı, kenarda yay için bir ara çizgi, sarkan kısımda iki çizgi.
    dis = [0.0045, yay] + [yay + (sark - yay) * k / 2 for k in (1, 2)]
    ic = [-W / 2 + i * h for i in range(n_ic + 1)]
    cizgi = [-W / 2 - d for d in reversed(dis)] + ic + [W / 2 + d for d in dis]

    def nokta(u, v):
        """Serili hâldeki (u, v) noktasının masadaki 3B yeri."""
        cu, cv = max(-W / 2, min(W / 2, u)), max(-W / 2, min(W / 2, v))   # masanın üstündeki en yakın nokta
        eu, ev = u - cu, v - cv                           # kenardan ne kadar taşıyor
        d = math.hypot(eu, ev)
        if d < 1e-9:
            return (u, v, T)                              # masanın üstünde: düz
        yx, yy = eu / d, ev / d                           # taşma yönü (kenara dik, köşede çapraz)
        if d <= yay:                                      # kenardan dönüş: çeyrek daire
            aci = d / k0
            ote, dus = k0 * math.sin(aci), k0 * (1 - math.cos(aci))
        else:                                             # sarkan kısım
            kalan = d - yay
            kose = min(abs(eu), abs(ev)) / d * 1.414      # 0 kenarın ortasında, 1 tam köşede
            dalga = 0.010 * kose * (kalan / sark) * math.sin(math.atan2(ev, eu) * 8)   # köşe kıvrımları
            ote, dus = k0 + kalan * 0.12 + dalga, k0 + kalan
        return (cu + yx * ote, cv + yy * ote, T - dus)

    m = len(cizgi)
    noktalar = [nokta(u, v) for u in cizgi for v in cizgi]   # i. u çizgisi, j. v çizgisi → i * m + j
    yuzler, renk = [], []
    for i in range(m - 1):
        for j in range(m - 1):
            a = i * m + j
            yuzler.append((a, a + m, a + m + 1, a + 1))   # +x sonra +y: yukarıdan saat yönünün tersi, yüz yukarı bakar
            uc, vc = (cizgi[i] + cizgi[i + 1]) / 2, (cizgi[j] + cizgi[j + 1]) / 2   # hücre ortası (serili hâlde)
            su = math.floor((uc + W / 2) / h) % 2 == 0    # dikey kırmızı şeride mi düşüyor
            sv = math.floor((vc + W / 2) / h) % 2 == 0    # yatay kırmızı şeride mi
            renk.append(int(su) + int(sv))                # 0 beyaz, 1 açık kırmızı, 2 koyu kırmızı
    me = bpy.data.meshes.new("Musamba")
    me.from_pydata(noktalar, [], yuzler)
    me.update()
    o = bpy.data.objects.new("Musamba", me)
    bpy.context.collection.objects.link(o)
    for mat in mats:
        o.data.materials.append(mat)
    for p, r in zip(me.polygons, renk):
        p.material_index = r
    # Kalınlık aşağıya (masaya doğru) eklenir: üst yüz T'de kalır, alttan ve etekten bakınca kâğıt gibi incecik görünmez.
    o.modifiers.new("Kalinlik", "SOLIDIFY").thickness = kal
    return ekle(o)


def masa():
    # 90'ların kare mutfak masası: krom borulu dört ayak, sunta tabla, üstünde kenarlardan sarkan kırmızı
    # pötikare muşamba (çay bardağı izleri, sigara yanığı, bıçak kesiği). Üstündekiler eskisi gibi:
    # meyve kâsesi ve şekerlik. Sınır kutusu eski yuvarlak masayla aynı (0,95 x 0,95 x 0,89).
    T = 0.754                                                              # muşambanın üst yüzü (eskisiyle aynı)
    W = 0.90                                                               # tablanın kenarı
    sunta = M("Masa_Sunta", (0.30, 0.22, 0.13), 0.7)                       # tablanın kenarı ve altı: çıplak sunta
    krom = M("Masa_Krom", (0.55, 0.55, 0.53), 0.3, 0.9)                    # matlaşmış krom boru
    pas = M("Masa_Pas", (0.20, 0.07, 0.02), 0.9)
    kutu((W, W, 0.022), (0, 0, T - 0.0015 - 0.011), sunta, 0.003)          # tabla
    for s in (-1, 1):                                                      # tablanın altındaki çerçeve (kayıtlar)
        kutu((W - 0.08, 0.02, 0.05), (0, s * (W / 2 - 0.04), T - 0.0235 - 0.025), sunta, 0.002)
        kutu((0.02, W - 0.08, 0.05), (s * (W / 2 - 0.04), 0, T - 0.0235 - 0.025), sunta, 0.002)
    for x in (-1, 1):
        for y in (-1, 1):
            px, py = x * (W / 2 - 0.05), y * (W / 2 - 0.05)
            sil(0.015, T - 0.0235 - 0.012, (px, py, 0.012 + (T - 0.0235 - 0.012) / 2), krom, 14)   # krom ayak
            sil(0.017, 0.024, (px, py, 0.012), mt("siyah_plastik"), 14)                            # plastik ayak pabucu
            kutu((0.06, 0.06, 0.006), (px, py, T - 0.0235 - 0.003), krom)                         # tablaya vidalı tabla
            if x * y > 0:                                                  # iki ayakta paslanmış halka
                sil(0.0153, 0.012, (px, py, 0.055 + 0.02 * (x > 0)), pas, 14)
    musamba(W, T, 0.09, 14, (M("Musamba_Beyaz", (0.80, 0.78, 0.72), 0.35), M("Musamba_Kirmizi", (0.62, 0.10, 0.07), 0.35),
                            M("Musamba_Koyu", (0.32, 0.03, 0.025), 0.35)))
    # Yıpranma: iki çay bardağı halkası, bir sigara yanığı, bıçak kesiği.
    leke = M("Musamba_Cay_Izi", (0.22, 0.10, 0.04), 0.5)
    for (cx, cy) in ((0.20, 0.17), (0.25, 0.10)):
        supur(oval_halka(cx, cy, 0.025, 0.025, T + 0.0004, n=16, us=2.0), serit_kesit(0.004, 0.0006), leke, kapali=True)
    sil(0.006, 0.0006, (-0.27, -0.20, T + 0.0003), mt("ic_koyu"), 10)                 # sigara yanığı
    sil(0.010, 0.0004, (-0.27, -0.20, T + 0.0002), M("Musamba_Yanik", (0.25, 0.14, 0.05), 0.6), 12)   # yanığın sararmış çevresi
    don(kutu((0.11, 0.0015, 0.0006), (0.05, -0.30, T + 0.0003), mt("ic_koyu")), rz=0.35)   # bıçak kesiği
    # Meyve kâsesi (beyaz, mavi kenarlı) ve meyveler.
    k = koni(0.06, 0.13, 0.07, (-0.10, 0.06, T + 0.035), mt("porselen"), 32)
    ortak.oy(k, ortak.silindir("Oy", 0.115, 0.07, (-0.10, 0.06, T + 0.035 + 0.012), mt("porselen"), 32))
    r = sil(0.131, 0.008, (-0.10, 0.06, T + 0.066), M("Kase_Mavi", (0.04, 0.10, 0.40), 0.2), 32)
    ortak.oy(r, ortak.silindir("Oy", 0.118, 0.02, (-0.10, 0.06, T + 0.066), mt("sut"), 32))   # mavi kenar bir halka; kâsenin ağzını kapatmasın
    elma, port = M("Elma", (0.35, 0.03, 0.02), 0.4), M("Portakal", (0.60, 0.20, 0.02), 0.6)
    for (x, y, z, m, r) in ((-0.14, 0.04, 0.045, elma, 0.036), (-0.07, 0.10, 0.045, elma, 0.036), (-0.06, 0.02, 0.047, port, 0.038),
                            (-0.13, 0.11, 0.05, port, 0.038), (-0.10, 0.06, 0.09, elma, 0.035)):
        kure(r, (x, y, T + z), m, seg=12, halka=8)
    ban = M("Muz", (0.70, 0.50, 0.05), 0.5)
    for i in range(4):                                                     # kavisli muz
        a0, a1 = 0.3 + i * 0.35, 0.3 + (i + 1) * 0.35
        cubuk((-0.10 + 0.09 * math.cos(a0) - 0.02, 0.06 + 0.02, T + 0.07 + 0.05 * math.sin(a0)),
              (-0.10 + 0.09 * math.cos(a1) - 0.02, 0.06 + 0.02, T + 0.07 + 0.05 * math.sin(a1)), 0.016, ban, 8)
    # Şekerlik: kapaklı porselen, kenarında çay kaşığı.
    sil(0.04, 0.06, (0.18, -0.12, T + 0.03), mt("porselen"), 20, 0.006)
    sil(0.043, 0.008, (0.18, -0.12, T + 0.064), mt("porselen"), 20)
    kure(0.01, (0.18, -0.12, T + 0.075), M("Sekerlik_Yaldiz", (0.60, 0.42, 0.10), 0.3, 0.9), seg=8, halka=6)
    sil(0.0405, 0.008, (0.18, -0.12, T + 0.03), M("Kase_Mavi", (0.04, 0.10, 0.40), 0.2), 20)
    don(kutu((0.11, 0.008, 0.003), (0.26, -0.14, T + 0.0015), mt("celik")), rz=0.4)
    uvle()
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


# =====================================================================================
# SALON: KANEPE (Poly Haven'ın siyah deri chesterfield'ının yerine)
# =====================================================================================

def cizgili_kural(cx, sira, gen, eksen=0):
    """
    Çizgili döşemelik kumaşın kuralı (desenle için): yüzün eksen koordinatı, minderin ortasından (cx) ne kadar
    uzaksa sira dizisinde o kadar ilerlenir; her adım gen metre genişliğinde bir şerittir. Ortadan iki yana
    simetrik gider: döşemeci kumaşı minderin ortasına göre keser.
    """
    def kural(c):
        i = int(abs(c[eksen] - cx) / gen)                 # ortadan kaçıncı şerit
        return sira[i % len(sira)]
    return kural


def kanepe():
    # 90'ların üçlü kanepesi: ceviz ahşap iskelet (torna kısa ayaklar, önde görünen ön kayıt, ahşap kol başlıkları
    # ve sırt kapağı), bej-kahve-kiremit çizgili dokuma döşeme, üç oturak ve üç gevşek sırt minderi, ortadaki
    # oturak çökmüş. Sırtta tığ işi dantel. Ölçü: 1,81 x 0,82, ön yüz -y.
    # ÖNEMLİ: kanepenin altında yerde Kerem'in yırtık çizim parçası duruyor; oyuncu çömelip altına bakıyor.
    # Bu yüzden ayaklar dışında her şey yerden 18 cm yukarıda başlar, altı baştan sona açıktır.
    ceviz = M("Kanepe_Ceviz", (0.06, 0.03, 0.013), 0.45)                   # koyu cilalı ceviz
    asinmis = M("Kanepe_Ceviz_Asinmis", (0.09, 0.05, 0.022), 0.7)         # ellerin değdiği yerde cilası gitmiş
    # Dokuma renkleri birbirine yakın, kirli ve sıcak tutuldu: parlak, zıt çizgiler bugünün plaj minderi gibi duruyordu.
    bej = M("Kanepe_Bej", (0.30, 0.21, 0.11), 0.95)                        # dokumanın zemin rengi (kirli bej)
    kahve = M("Kanepe_Kahve", (0.09, 0.045, 0.02), 0.95)
    kiremit = M("Kanepe_Kiremit", (0.22, 0.075, 0.03), 0.95)               # soluk kiremit
    hardal = M("Kanepe_Hardal", (0.34, 0.22, 0.06), 0.95)                  # sırma gibi hardal sarısı
    zeytin = M("Kanepe_Zeytin", (0.10, 0.10, 0.04), 0.95)
    kumas = [bej, kahve, kiremit, hardal, zeytin]
    # Şerit sırası (ortadan dışa): bej zemin, kahve-kiremit-kahve üçlüsü, bej, hardal, ince zeytin, bej ...
    # Her şerit tam bir yüz genişliğindedir: şerit yüzden darsa bazı renkler hiç bir yüze düşmez ve desen
    # rastgele kalın bantlara dönüşür (ilk denemede öyle oldu). Minderin 0,53 m'lik eni 20 yüze bölünüyor.
    sira = [0, 0, 1, 2, 1, 0, 3, 0, 4, 0, 0, 1, 3, 1]
    G = 0.53 / 20                                                          # bir şeridin (bir yüzün) genişliği
    YUK = 0.18                                                             # ahşap iskeletin yerden yüksekliği (alt boşluk)
    XA = 0.80                                                              # iç genişliğin yarısı (kolların iç yüzü)
    # --- Ayaklar: dört köşede ve arkanın ortasında kısa torna ayak, hafif dışa açık.
    ayak = [(0.0, 0.017), (0.10, 0.019), (0.55, 0.024), (0.62, 0.029), (0.70, 0.024), (0.92, 0.027), (1.0, 0.027)]
    for (x, y) in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        torna((x * 0.865, y * 0.355, 0.0), (x * 0.852, y * 0.342, YUK), ayak, ceviz, 8)
    torna((0.0, 0.36, 0.0), (0.0, 0.35, YUK), ayak, ceviz, 8)               # arka orta ayak (önde ayak yok: parça görünsün)
    # --- Ahşap iskelet: ön kayıt (alt kenarı kavisli değil, oluklu), arka kayıt, iki yan kayıt.
    kutu((2 * XA, 0.04, 0.12), (0, -0.375, YUK + 0.06), ceviz, 0.004)     # ön kayıt, önden görünen ahşap
    kutu((2 * XA - 0.04, 0.004, 0.006), (0, -0.396, YUK + 0.085), mt("ic_koyu"))   # ön kayıttaki oyma oluk
    kutu((2 * XA, 0.04, 0.10), (0, 0.38, YUK + 0.05), ceviz)              # arka kayıt (duvara bakar, pahsız)
    for s in (-1, 1):
        kutu((0.105, 0.80, 0.08), (s * 0.8525, 0.0, YUK + 0.04), ceviz, 0.004)          # kolun alt kaydı
        kutu((0.06, 0.045, 0.42), (s * 0.8525, -0.3825, YUK + 0.21), ceviz, 0.005)      # kolun ön dikmesi (ahşap)
        kutu((0.06, 0.045, 0.42), (s * 0.8525, 0.3825, YUK + 0.21), ceviz)              # kolun arka dikmesi (pahsız)
    # --- Oturma tablası (minderlerin altındaki döşeli kasa) ve sırt kasası.
    kutu((2 * XA, 0.70, 0.06), (0, -0.01, 0.31), kahve, 0.01)
    yastik((2 * XA, 0.13, 0.42), (0, 0.335, 0.30 + 0.21), bej, bol=(6, 1, 2))   # minderlerin arkasında kalır: düz bej
    kutu((2 * XA + 0.04, 0.155, 0.03), (0, 0.33, 0.735), ceviz, 0.008)    # sırtın ahşap kapağı
    # --- Kollar: döşeli yan pano + üstte ahşap başlık; başlığın önü el değe değe açılmış, birinde sigara yanığı.
    for s in (-1, 1):
        kol = yastik((0.09, 0.74, 0.34), (s * 0.8525, 0.0, YUK + 0.08 + 0.17), bej, bol=(1, 8, 2))
        desenle(kol, kumas, cizgili_kural(0.0, sira, 0.74 / 16, eksen=1))  # 16 yüz: şerit = bir yüz
        kutu((0.12, 0.75, 0.03), (s * 0.845, 0.025, 0.615), ceviz, 0.01)  # kol başlığı
        sil(0.06, 0.03, (s * 0.845, -0.35, 0.615), ceviz, 16)             # başlığın yuvarlak önü
        elips(0.06, 0.14, 0.001, (s * 0.845, -0.32, 0.6305), asinmis, 14)  # el değen yer
    sil(0.006, 0.0008, (0.84, 0.05, 0.6304), mt("ic_koyu"), 10)            # sigara yanığı
    # --- Oturak minderleri: üç tane; ortadaki yıllarca oturulmaktan çökmüş, sağdakinin ön dikişi patlamış.
    for i, cx in enumerate((-0.535, 0.0, 0.535)):
        cokme = 0.035 if i == 1 else 0.015                                 # orta minder daha çok çökük

        def oturak(px, py, pz, cokme=cokme):
            if pz > 0:                                                     # yalnız üst yüz çöker
                pz -= cokme * (1 - (2 * px / 0.53) ** 2) * max(0.0, 1 - ((py + 0.06) / 0.31) ** 2) * (pz / 0.065)
            if py < -0.2:                                                  # ön kenar biraz şişkin, öne taşar
                py -= 0.008 * (1 - (2 * pz / 0.13) ** 2)
            return (px + cx, py - 0.075, pz + 0.405)

        m = yastik((0.53, 0.62, 0.13), (0, 0, 0), bej, oturak, bol=(10, 2, 1))
        desenle(m, kumas, cizgili_kural(cx, sira, G))
    kutu((0.08, 0.006, 0.012), (0.56, -0.392, 0.41), mt("ic_koyu"))       # patlamış dikiş
    kure(0.02, (0.56, -0.392, 0.408), M("Kanepe_Sunger", (0.60, 0.50, 0.22), 0.9), (1.8, 0.5, 0.45), seg=8, halka=5)  # sünger
    elips(0.09, 0.07, 0.0015, (-0.04, -0.12, 0.4368), M("Kanepe_Leke", (0.20, 0.13, 0.06), 0.95), 14)  # çökük minderde çay lekesi
    # --- Sırt minderleri: oturakların üstünde, arkaya yatık, ortası kabarık; tepeleri ahşap kapağın altında kalır.
    orta = None
    for cx in (-0.535, 0.0, 0.535):
        def sirt_m(px, py, pz, cx=cx):
            if py < 0:                                                     # ön yüz ortada kabarık
                py *= 1 + 0.35 * (1 - (2 * px / 0.52) ** 2) * (1 - (2 * pz / 0.25) ** 2)
            return (px + cx, py + 0.165 + pz * 0.25, pz + 0.47 + 0.125)   # tepe arkaya yatık
        m = yastik((0.52, 0.14, 0.25), (0, 0, 0), bej, sirt_m, bol=(10, 1, 2))
        desenle(m, kumas, cizgili_kural(cx, sira, G))
        if cx == 0.0:
            orta = m                                                       # dantel bunun üstüne serilecek
    # --- Dantel: sırt kapağının üstüne atılmış, öne doğru orta minderin tepesine ve önüne sarkan tığ işi örtü.
    # Minderin yüzeyi biçimlendirilmiş ve yuvarlatılmış olduğu için yerini hesapla bulmak zor; onun yerine
    # minderin üstüne ve önüne "ışın" atıyoruz (ray_cast: bir noktadan bir yöne giden çizginin yüzeye çarptığı
    # yer). Dantel çarpma noktalarının 3 mm dışından geçer: yüzeye yapışık durur, içine gömülmez.
    bpy.context.view_layer.update()
    dx = 0.04                                                              # tam ortada değil, biraz kaymış
    yol = [Vector((dx, 0.414, 0.69)), Vector((dx, 0.412, 0.748)), Vector((dx, 0.39, 0.7535)),
           Vector((dx, 0.31, 0.7535)), Vector((dx, 0.255, 0.751))]          # ahşap kapağın arkası ve üstü
    for y in (0.225, 0.19, 0.155, 0.125):                                  # minderin tepesine yukarıdan ışın
        ok, q, nrm, _ = orta.ray_cast(Vector((dx, y, 1.0)), Vector((0, 0, -1)))
        if ok:
            yol.append(q + nrm * 0.003)
    on = []
    for z in (0.69, 0.67, 0.65, 0.63):                                     # minderin önüne, önden ışın
        ok, q, nrm, _ = orta.ray_cast(Vector((dx, -0.5, z)), Vector((0, 1, 0)))
        if ok:
            on.append(q + nrm * 0.003)
    yol += on
    dantel_m = M("Kanepe_Dantel", (0.66, 0.63, 0.54), 0.95)               # sararmış beyaz pamuk iplik
    supur(yol, serit_kesit(0.30, 0.002), dantel_m, yukari=(0, -1, 1))
    # Tığ işinin görüntüsü: sarkan ucunda fistolar (yarım daire dişler), içinde sıra sıra delikler. Delik
    # gerçekten açılmaz (üçgen sayısı patlar); arkadaki kumaşın gölgesi renginde küçük kareler delik gibi okunur.
    uc = on[-1]
    golge = M("Kanepe_Dantel_Golge", (0.16, 0.11, 0.06), 0.95)
    for k in range(9):
        dik_disk(0.017, 0.002, (dx - 0.136 + k * 0.034, uc.y + 0.0005, uc.z), dantel_m, 8)
        kutu((0.008, 0.0012, 0.008), (dx - 0.136 + k * 0.034, uc.y - 0.0012, uc.z - 0.006), golge)
    for j, q in enumerate(on[1:-1]):                                       # iki sıra delik (üst sıra minderin kıvrımında kalır)
        for k in range(8 if j % 2 else 9):
            kutu((0.007, 0.0012, 0.007), (dx - 0.136 + k * 0.034 + (0.017 if j % 2 else 0), q.y - 0.002, q.z), golge)
    uvle()
    return bitir("Kanepe_K3")


# =====================================================================================
# ODA B: ÇOCUK SANDALYESİ (Kerem'in çalışma masasının sandalyesi; mavi plastik okul sandalyesinin yerine)
# =====================================================================================

def cocuk_sandalye():
    # 90'ların okul sandalyesi, çocuk boyu: yeşile boyalı bükme çelik boru iskelet, kontrplak oturak ve kavisli
    # kontrplak sırt (perçinli). Boyası yer yer atmış (altından çelik görünür), bir ayağın dibi paslı.
    # Oturağa tükenmez kalemle çizilmiş, sırtına çıkartma yapıştırılmış, altına sakız yapıştırılmış.
    boya = M("Cocuk_Boya_Yesil", (0.06, 0.19, 0.09), 0.5)                  # fırın boyalı boru
    celik = mt("celik")                                                    # boyası atmış yerde çıplak çelik
    pas = M("Cocuk_Pas", (0.20, 0.07, 0.02), 0.9)
    kontrplak = M("Cocuk_Kontrplak", (0.30, 0.18, 0.08), 0.55)             # vernikli kayın kontrplak
    R = 0.0105                                                             # borunun yarıçapı
    kesit = daire_kesit(1.0, 8)
    yollar = {}
    for sx in (-1, 1):
        xf, xs = sx * 0.235, sx * 0.212                                    # ayak ucunda geniş, oturakta dar
        # Yan boru tek parça bükülmüş: ön ayak → oturak kaydı → arka ayak (yuvarlak dirseklerle).
        yan = egri([(xf, -0.245, 0.0), (xs + sx * 0.004, -0.205, 0.30), (xs, -0.19, 0.345), (xs, -0.15, 0.362),
                    (xs, 0.13, 0.362), (xs, 0.17, 0.345), (xs + sx * 0.004, 0.19, 0.30), (xf, 0.245, 0.0)], 3)
        supur(yan, kesit, boya, yukari=(1, 0, 0), olcek=[R] * len(yan))
        # Sırt dikmesi: oturak kaydından kalkıp geriye yatarak yükselir.
        dik = egri([(xs, 0.15, 0.362), (xs, 0.175, 0.50), (xs, 0.195, 0.62), (xs, 0.205, 0.70)], 3)
        supur(dik, kesit, boya, yukari=(1, 0, 0), olcek=[R] * len(dik))
        sil(R * 0.95, 0.008, (xs, 0.205, 0.703), mt("siyah_plastik"), 8)  # dikmenin ucundaki plastik tapa
        sil(0.0135, 0.02, (xf, -0.245, 0.01), mt("siyah_plastik"), 10)   # ön ayak lastiği
        sil(0.0135, 0.02, (xf, 0.245, 0.01), mt("siyah_plastik"), 10)              # arka ayak lastiği
        yollar[sx] = (yan, dik)
    for y in (-0.12, 0.10):                                                # iki yanı birbirine bağlayan oturak altı borular
        cubuk((-0.212, y, 0.352), (0.212, y, 0.352), 0.009, boya, 8)

    def soyuk(yol, i, uzun, mat):
        """Boru üstünde i. noktadan başlayan kısa bir halka: boyanın atıp çeliğin (ya da pasın) göründüğü yer."""
        a, b = yol[i], yol[i] + (yol[i + 1] - yol[i]).normalized() * uzun
        cubuk(tuple(a), tuple(b), R * 1.07, mat, 8)

    yan, dik = yollar[1]
    soyuk(yan, 2, 0.018, celik)                                            # sağ ön ayakta tekme izi
    soyuk(yan, 4, 0.010, celik)
    soyuk(dik, 6, 0.014, celik)                                            # sırt dikmesinde sürtünme
    yan, dik = yollar[-1]
    soyuk(yan, len(yan) - 3, 0.025, pas)                                   # sol arka ayağın dibi paslı
    soyuk(yan, 9, 0.012, celik)

    # Oturak: bükme kontrplak; ortası hafif çukur, ön kenarı aşağı kıvrık (diz altını kesmesin).
    def oturak(px, py, pz):
        z = pz + 0.010 * (2 * px / 0.40) ** 2                              # yanlara doğru hafif yükselir
        if py < -0.13:
            z -= ((-0.13 - py) / 0.05) ** 2 * 0.02                         # ön kenar aşağı kıvrılır
        return (px, py - 0.01, z + 0.379)

    yastik((0.40, 0.36, 0.012), (0, 0, 0), kontrplak, oturak, bol=(4, 4, 1))
    # Sırt: içe kavisli kontrplak, dikmelerin önüne perçinli, üstü geriye yatık.
    def sirt(px, py, pz):
        return (px, py + 0.21 - 0.025 * (2 * px / 0.42) ** 2 + pz * 0.15, pz + 0.63)

    yastik((0.42, 0.012, 0.15), (0, 0, 0), kontrplak, sirt, bol=(6, 1, 2))
    for sx in (-1, 1):                                                     # perçinler
        for z in (0.595, 0.665):
            on = 0.21 - 0.025 * (2 * 0.19 / 0.42) ** 2 - 0.006 + (z - 0.63) * 0.15
            dik_disk(0.006, 0.003, (sx * 0.19, on - 0.0008, z), celik, 8)
    # Çıkartma: sırtın önünde sarı-kırmızı yuvarlak etiket, sırtın eğimine yatırılmış.
    on = 0.21 - 0.025 * (2 * 0.08 / 0.42) ** 2 - 0.006 + 0.01 * 0.15
    don(dik_disk(0.026, 0.0012, (0.08, on - 0.0008, 0.64), mt("boya_sari"), 16), rx=-math.atan(0.15))
    don(dik_disk(0.014, 0.0012, (0.08, on - 0.0018, 0.64), mt("boya_kirmizi"), 12), rx=-math.atan(0.15))
    # Oturağın üstüne tükenmez kalemle çizilmiş çizgiler (yüzeyin kavisine göre yükseklik).
    ust = lambda x: 0.379 + 0.006 + 0.010 * (2 * x / 0.40) ** 2 + 0.0003
    for (x0, y0, x1, y1) in ((-0.09, 0.03, 0.01, 0.07), (-0.07, 0.00, 0.02, 0.035), (0.04, -0.05, 0.10, -0.02)):
        xm, ym = (x0 + x1) / 2, (y0 + y1) / 2
        don(kutu((math.hypot(x1 - x0, y1 - y0), 0.0015, 0.0004), (xm, ym - 0.01, ust(xm)), mt("boya_mavi")),
            rz=math.atan2(y1 - y0, x1 - x0))
    # Oturağın altına yapıştırılmış sakız.
    kure(0.012, (0.09, 0.04, 0.3715), M("Sakiz", (0.55, 0.38, 0.40), 0.8), (1.3, 1.0, 0.4), seg=8, halka=5)
    uvle()
    return bitir("Cocuk_Sandalyesi_K3")


ESYALAR = {
    "askilik": askilik, "ayakkabi": ayakkabi_yigini, "canta": canta, "bebek_arabasi": bebek_arabasi, "terlik": terlikler,
    "cizimler": cizimler, "oyuncak": oyuncaklar, "yolluk": yolluk,
    "tv": tv, "boyama": boyama, "park": park, "hali": hali, "perde": perde, "raf": raf,
    "mama": mama, "masa": masa, "sandalye": sandalye, "buzdolabi": buzdolabi, "ocak": ocak, "camasir": camasir, "biberon": biberon,
    "kanepe": kanepe, "cocuksandalye": cocuk_sandalye,
}

if __name__ == "__main__":
    hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "askilik"
    ortak.sahneyi_temizle()
    ESYALAR[hangi]()
