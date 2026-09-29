# kat5_esyalar.py
# 5. katın eşyaları (#83). 1994, 18 numaralı daire: Karaca ailesi (orta yaşlı bir çift) yazlıkta.
# Kapıcı Cemal banyodaki su kaçağını onarmaya girmiş, işi yarım bırakıp kaybolmuş. Düzenli, klasik,
# kahverengi tonlu bir ev; holde tavandan su damlıyor. Katlarda aynı model yok: hepsi bu kata özgü.
#   blender -b --factory-startup --python kat5_esyalar.py -- <değişken> [--onizleme <png>]
#
# ANTRE
#   ayakkabilik  Ayakkabilik_K5      : panjur kapaklı ayakkabı dolabı (kapaklar süs), üstünde dantel ve anahtar kâsesi.
#   semsiyelik   Semsiyelik_K5       : yeşil seramik şemsiyelik, içinde iki kapalı şemsiye.
#   askı         Duvar_Askisi_K5     : üç kancalı duvar askısı; Cemal'in lacivert iş montu ve el feneri asılı.
# HOL
#   yolluk       Yolluk_K5           : mavi-krem hol yolluğu (3,4 x 0,75 m).
#   konsol       Konsol_K5           : dar hol konsolu (açılmaz), üstünde kurumuş eğrelti otlu saksı.
#   kova         Su_Kovasi_K5        : tavandan damlayan suyu toplayan dolu plastik kova ve yerde ıslak leke.
# SALON
#   kanepe       Kanepe_K5           : kahverengi kadife, ahşap kol başlıklı üçlü kanepe; arkalığında dantel.
#   tv           TV_Unitesi_K5       : cam kapaklı alçak TV ünitesi (içinde video, kasetler) ve üstünde 25" tüplü TV.
#   hali         Hali_K5             : bej-kahve Isparta tipi halı (2,5 x 1,7 m).
#   perde        Perde_K5            : ahşap kornişte kahverengi pileli fon perde ve arkasında tül (1,4 m pencere).
# MUTFAK
#   buzdolabi    Buzdolabi_K5        : 90'lar beyaz, iki kapılı, dikey kollu buzdolabı (açılmaz, tek parça).
#   ocak         Ocak_K5             : krem ocak, 3 gaz + 1 elektrik gözü, cam kapağı kalkık.
#   masa         Mutfak_Masasi_K5    : sarı formika üstlü, alüminyum kenarlı mutfak masası.
#   tabure       Tabure_K5           : aynı takımdan sarı formika tabure.
#   sepet        Ekmek_Sepeti_K5     : hasır ekmek sepeti, içinde bayat yarım ekmek.
# BANYO
#   camasir      Camasir_Makinesi_K5 : üstten yüklemeli, yarı otomatik (çift kazanlı) beyaz-mavi çamaşır makinesi.
#   havlu        Havlu_Askisi_K5     : çift çubuklu havlu askısı, iki havlu.
#   ecza         Banyo_Dolabi_K5     : aynalı ecza dolabı (süs), altında diş fırçası bardaklı küçük raf.
#   legen        Legen_Kova_K5       : yeşil plastik leğen ve mavi kova.
# KİLER
#   raf          Raf_K5              : ahşap raf; boya kutuları, kavanozlar, tenekeler.
#   lamba        Is_Lambasi_K5       : Cemal'in kafesli seyyar iş lambası, yerde kıvrılmış turuncu kablosu.
#   boya         Boya_Kovalari_K5    : üst üste boya kovaları, yanında açık bir kova, üstünde fırçalar.
# ODA A (yatak odası)
#   komodin      Komodin_K5          : koyu ahşap komodin (süs), üstünde gece lambası.
#   sifonyer     Sifonyer_K5         : koyu ahşap şifonyer, üstünde yuvarlak ayna ve mücevher kutusu.
#   perdeA       Perde_OdaA_K5       : ince metal çubukta bej perde ve pilili farbala (1,2 m pencere).
# ODA B (çalışma odası)
#   calisma      Calisma_Masasi_K5   : çekmeceli ahşap çalışma masası; daktilo, masa lambası, kâğıtlar, kül tablası.
#   kitaplik     Kitaplik_K5         : kitaplarla dolu uzun kitaplık.
#   sandalye     Sandalye_K5         : dikmeli arkalıklı, kahverengi suni deri minderli ahşap sandalye.
#   perdeB       Perde_OdaB_K5       : beyaz plastik ray kornişte yeşil perde (1,2 m pencere).
#
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası. Duvar eşyalarında arka yüz y = 0 düzleminde,
# pivot askı/çubuk hizasında (duvara tutturulan nokta). Perdelerde pivot korniş yüksekliğinde arka üst kenar.
# Hiçbiri açılır değil: her eşya tek nesne olarak aktarılır.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import mathutils

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "ayakkabilik"
ortak.sahneyi_temizle()
rnd = random.Random(55)      # sabit tohum: betik her çalıştığında aynı dağınıklık
p = []                       # birleştirilecek parçalar


# ---------- KISA YARDIMCILAR (her biri parçayı p listesine de ekler) ----------

def K(olcu, konum, mat, pah=0.0, ad="P"):
    """Kutu: olcu (en, derin, boy), konum merkezi."""
    n = ortak.kutu(ad, olcu, konum, mat, pah)
    p.append(n)
    return n


def KR(olcu, konum, mat, rot, pah=0.0, ad="P", olcek=(1, 1, 1)):
    """
    Döndürülmüş kutu: rot (x, y, z) radyan, dönüş kutunun KENDİ merkezi etrafında. Dönüş meshe işlenir.
    DİKKAT: ortak.kutu konumu meshe gömer (pivot dünya merkezinde kalır); konumlu bir kutuyu döndürmek onu
    dünya merkezi etrafında savurur. Bu yüzden kutu merkezde kurulur, döndürülür, sonra yerine taşınır.
    olcek: dönüşten SONRA uygulanan ölçek (baklava: döndürülmüş kareyi uzatmak için).
    """
    n = ortak.kutu(ad, olcu, (0, 0, 0), mat, 0.0)
    n.rotation_euler = rot
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    n.scale = olcek
    n.location = konum
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(rotation=True)
    if pah > 0:
        ortak.pah(n, pah)
    p.append(n)
    return n


def S(r, boy, konum, mat, kose=24, pah=0.0, ad="P"):
    """Dik silindir (z ekseni)."""
    n = ortak.silindir(ad, r, boy, konum, mat, kose, pah)
    p.append(n)
    return n


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk (boru, tel, sap)."""
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=d.length,
                                        location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")   # silindirin z eksenini a->b yönüne çevir
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def koni_eksen(a, b, r_a, r_b, mat, kose=16):
    """a'da r_a, b'de r_b yarıçaplı kesik koni (kapalı şemsiye, fırça sapı)."""
    d = mathutils.Vector(b) - mathutils.Vector(a)
    bpy.ops.mesh.primitive_cone_add(vertices=kose, radius1=r_a, radius2=r_b, depth=d.length,
                                    location=tuple((mathutils.Vector(a) + mathutils.Vector(b)) / 2))
    n = bpy.context.active_object
    n.rotation_mode = "QUATERNION"
    n.rotation_quaternion = d.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def koni(r_alt, r_ust, boy, konum, mat, kose=32, ekle=True):
    """Dik kesik koni; konum merkezi. ekle=False: oyma kesicisi olarak kullanılacak (listeye girmez)."""
    bpy.ops.mesh.primitive_cone_add(vertices=kose, radius1=r_alt, radius2=r_ust, depth=boy, location=konum)
    n = bpy.context.active_object
    ortak.ata(n, mat)
    if ekle:
        p.append(n)
    return n


def kure(r, konum, mat, olcek=(1, 1, 1), seg=14):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=max(6, seg // 2))
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def simit(R, r, konum, mat, olcek=(1, 1, 1), rot=(0, 0, 0), seg=24):
    """Halka (torus): kova ağzı, tel çember, sepet örgüsü."""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, location=konum, rotation=rot,
                                     major_segments=seg, minor_segments=6)
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def dik_disk(r, kal, konum, mat, kose=24):
    """Ön yüzü -y'ye bakan disk (düğme, ayna, kadran)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=kal, location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def yatay_silindir(r, boy, konum, mat, kose=16):
    """x ekseni boyunca yatan silindir (korniş, merdane)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=(0, math.pi / 2, 0))
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def kova(r_alt, r_ust, boy, x, y, mat, kal=0.006, z0=0.0):
    """İçi boş plastik kova/leğen: kesik koni, içi oyulmuş (taban kalınlığı kal), ağzında kalın halka."""
    dis = koni(r_alt, r_ust, boy, (x, y, z0 + boy / 2), mat, ekle=False)
    ic = koni(r_alt - kal, r_ust - kal + (r_ust - r_alt) * kal / boy, boy, (x, y, z0 + boy / 2 + kal), mat, ekle=False)
    ortak.oy(dis, ic)          # iç hacmi sil: ağzı açık, tabanı dolu kap
    p.append(dis)
    simit(r_ust, kal * 0.9, (x, y, z0 + boy), mat)
    return dis


def sap_yayi(x, y, z, R, mat, r=0.003, seg=10):
    """Kova sapı: x ekseninde iki kulaktan yukarı kavis yapan tel (xz düzleminde yarım çember)."""
    for i in range(seg):
        a0, a1 = math.pi * i / seg, math.pi * (i + 1) / seg
        cubuk((x - R * math.cos(a0), y, z + R * 0.8 * math.sin(a0)),
              (x - R * math.cos(a1), y, z + R * 0.8 * math.sin(a1)), r, mat, kose=6)


def pilili(ad, x0, x1, z_ust, boy, y, genlik, adim, mat, alt_oran=1.0, kal=0.004):
    """
    Zikzak pilili perde yüzeyi (from_pydata): üst kenarı x0-x1 arası z_ust'te, boy kadar aşağı sarkar,
    altta genişliği alt_oran ile daralır/açılır. Görünen yüz odaya (-y) bakar; ince kalınlık (solidify)
    verilir ki arkadan bakınca da görünsün.
    """
    n = max(2, int(abs(x1 - x0) / adim) + 1)
    orta = (x0 + x1) / 2
    satirlar = [(z_ust, 1.0), (z_ust - boy * 0.5, (1.0 + alt_oran) / 2), (z_ust - boy, alt_oran)]
    nokta, yuz = [], []
    for zi, oran in satirlar:
        for i in range(n):
            t = i / (n - 1)
            xx = orta + (x0 + (x1 - x0) * t - orta) * oran
            yy = y + (genlik if i % 2 == 0 else -genlik)
            nokta.append((xx, yy, zi))
    for r_ in range(len(satirlar) - 1):
        for i in range(n - 1):
            a = r_ * n + i
            yuz.append((a, a + n, a + n + 1, a + 1))     # bu sıra yüzün normalini -y'ye çevirir
    m = bpy.data.meshes.new(ad)
    m.from_pydata(nokta, [], yuz)
    m.update()
    o = bpy.data.objects.new(ad, m)
    bpy.context.collection.objects.link(o)
    ortak.ata(o, mat)
    kalinlik = o.modifiers.new("Kalinlik", "SOLIDIFY")   # kumaşa kalınlık: birleştirirken uygulanır
    kalinlik.thickness = kal
    p.append(o)
    return o


# ---------- ORTAK MALZEMELER ----------
ceviz = ortak.malzeme("K5_Ceviz", (0.075, 0.038, 0.016), puruz=0.45)        # evin klasik orta ceviz tonu
ceviz_ac = ortak.malzeme("K5_CevizAcik", (0.12, 0.065, 0.028), puruz=0.5)
koyu_ahsap = ortak.malzeme("K5_KoyuAhsap", (0.045, 0.022, 0.010), puruz=0.4)
pirinc = ortak.malzeme("K5_Pirinc", (0.55, 0.40, 0.14), puruz=0.3, metal=0.9)
krom = ortak.malzeme("K5_Krom", (0.62, 0.62, 0.60), puruz=0.2, metal=1.0)
dantel_m = ortak.malzeme("K5_Dantel", (0.82, 0.79, 0.70), puruz=0.9)
siyah = ortak.malzeme("K5_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
cam_koyu = ortak.malzeme("K5_CamKoyu", (0.05, 0.06, 0.06), puruz=0.05)

duvar = False        # duvar/perde eşyası mı (önizlemede yukarı kaldırılır)

# =====================================================================
# ANTRE
# =====================================================================
if hangi == "ayakkabilik":
    EN, DER, H = 0.80, 0.35, 0.90
    K((EN - 0.04, DER - 0.04, 0.06), (0, 0.01, 0.03), koyu_ahsap, 0.004)                       # geri çekik süpürgelik
    K((EN, DER, H - 0.085), (0, 0, 0.06 + (H - 0.085) / 2), ceviz, 0.004)                     # gövde
    K((EN + 0.02, DER + 0.02, 0.025), (0, -0.005, H - 0.0125), ceviz, 0.006)                  # üst tabla
    # İki kapak: çerçeve ve yatay panjur çıtaları (ayakkabı dolabı havalandırması). Kapaklar süs.
    y_on = -DER / 2
    for xk in (-EN / 4, EN / 4):
        w = EN / 2 - 0.02
        K((w, 0.016, H - 0.14), (xk, y_on - 0.008, 0.075 + (H - 0.14) / 2), ceviz_ac, 0.003)
        for j in range(9):
            K((w - 0.07, 0.012, 0.022), (xk, y_on - 0.020, 0.15 + j * 0.068), ceviz, 0.002)
        kure(0.014, (xk + (-1 if xk > 0 else 1) * (w / 2 - 0.04), y_on - 0.03, 0.55), pirinc)
    # Üstte dantel örtü, ön kenardan sarkan fistolar.
    K((0.56, DER - 0.02, 0.003), (0, -0.01, H + 0.0015), dantel_m)
    for i in range(12):
        K((0.03, 0.003, 0.028), (-0.26 + i * 0.0473, y_on - 0.018, H - 0.012), dantel_m)
    # Anahtar kâsesi: içi oyulmuş seramik kâse, içinde halkalı iki anahtar.
    seramik = ortak.malzeme("K5_Kase", (0.30, 0.12, 0.05), puruz=0.25)
    kase = ortak.silindir("Kase", 0.07, 0.045, (0.20, 0.0, H + 0.003 + 0.0225), seramik, 28, 0.008)
    ortak.oy(kase, ortak.silindir("KaseIc", 0.062, 0.05, (0.20, 0.0, H + 0.003 + 0.0225 + 0.008), seramik, 28))
    p.append(kase)
    simit(0.016, 0.002, (0.19, 0.01, H + 0.013), krom, seg=16)
    K((0.045, 0.012, 0.003), (0.22, 0.012, H + 0.013), pirinc)
    K((0.04, 0.011, 0.003), (0.19, -0.02, H + 0.016), krom)
    ad, h, uzak = "Ayakkabilik_K5", 0.9, 1.4

elif hangi == "semsiyelik":
    seramik = ortak.malzeme("K5_Semsiyelik", (0.04, 0.12, 0.07), puruz=0.2)
    yaldiz = ortak.malzeme("K5_Yaldiz", (0.50, 0.36, 0.10), puruz=0.3, metal=0.9)
    kumas1 = ortak.malzeme("K5_Semsiye_Siyah", (0.025, 0.025, 0.03), puruz=0.7)
    kumas2 = ortak.malzeme("K5_Semsiye_Bordo", (0.22, 0.03, 0.04), puruz=0.7)
    govde = ortak.silindir("Govde", 0.11, 0.52, (0, 0, 0.26), seramik, 32, 0.01)
    ortak.oy(govde, ortak.silindir("Ic", 0.098, 0.52, (0, 0, 0.26 + 0.015), seramik, 32))
    p.append(govde)
    for z in (0.08, 0.44):
        S(0.1115, 0.02, (0, 0, z), yaldiz, 32)                                              # yaldız bantlar
    simit(0.104, 0.007, (0, 0, 0.52), seramik, seg=32)

    def semsiye(uc, tepe, kumas, tutamak):
        """Kapalı şemsiye: uç demiri, katlanmış kumaş (iki koni), mil, sap."""
        a, b = mathutils.Vector(uc), mathutils.Vector(tepe)
        u = (b - a).normalized()
        L = (b - a).length
        cubuk(a, a + u * 0.06, 0.005, krom)                                   # uç demiri
        koni_eksen(a + u * 0.06, a + u * (L * 0.64), 0.008, 0.045, kumas, 18)    # katlanmış kumaş: uca doğru incelir
        koni_eksen(a + u * (L * 0.64), a + u * (L * 0.76), 0.045, 0.011, kumas, 18)
        cubuk(a + u * (L * 0.56), a + u * (L * 0.60), 0.043, kumas, 18)         # cırtlı sargı bandı
        cubuk(a + u * (L * 0.76), b, 0.006, krom)                              # mil
        if tutamak == "J":
            w = mathutils.Vector((1, 0, 0)) - u * u.x                          # eksene dik yön
            w.normalize()
            onceki = b
            cubuk(b - u * 0.10, b, 0.012, koyu_ahsap, 10)
            for i in range(1, 11):
                t = math.pi * 1.15 * i / 10
                q = b + u * (0.045 * math.sin(t)) + w * (0.045 * (1 - math.cos(t)))
                cubuk(onceki, q, 0.012, koyu_ahsap, 10)
                onceki = q
        else:
            cubuk(b - u * 0.12, b, 0.014, ceviz_ac, 10)
            kure(0.02, tuple(b), ceviz_ac)

    semsiye((-0.03, 0.02, 0.02), (0.05, -0.05, 0.92), kumas1, "J")
    semsiye((0.035, -0.01, 0.02), (0.09, 0.07, 0.86), kumas2, "duz")
    ad, h, uzak = "Semsiyelik_K5", 0.9, 1.3

elif hangi == "aski":
    duvar = True
    lacivert = ortak.malzeme("K5_Mont", (0.012, 0.018, 0.06), puruz=0.85)
    fermuar = ortak.malzeme("K5_Fermuar", (0.25, 0.25, 0.25), puruz=0.4, metal=0.7)
    fener_m = ortak.malzeme("K5_Fener", (0.30, 0.04, 0.02), puruz=0.4)
    K((0.60, 0.022, 0.10), (0, -0.011, 0), ceviz, 0.006)                                      # duvar tahtası
    for x in (-0.20, 0.0, 0.20):
        dik_disk(0.014, 0.004, (x, -0.024, 0), pirinc, 16)                                     # duvar rozeti
        cubuk((x, -0.022, 0.0), (x, -0.07, -0.025), 0.006, pirinc)                             # kanca kolu
        cubuk((x, -0.07, -0.025), (x, -0.085, 0.015), 0.006, pirinc)                           # kanca ucu
        kure(0.009, (x, -0.085, 0.015), pirinc)
    # Sol kancada lacivert iş montu: askı ilmeğinden asılı, omuzları düşük, kolları iki yanda sarkık.
    mx, my = -0.20, -0.085
    cubuk((mx, -0.075, -0.02), (mx, -0.075, -0.06), 0.004, lacivert)                           # askı ilmeği
    K((0.16, 0.08, 0.06), (mx, my, -0.07), lacivert, 0.02)                                     # yaka
    K((0.36, 0.11, 0.24), (mx, my, -0.20), lacivert, 0.03)                                     # omuz ve göğüs
    K((0.46, 0.13, 0.52), (mx, my, -0.56), lacivert, 0.03)                                     # gövde
    for s in (-1, 1):
        KR((0.10, 0.10, 0.56), (mx + s * 0.225, my, -0.45), lacivert, (0, s * 0.10, 0), 0.03)   # kollar
    K((0.012, 0.004, 0.70), (mx, my - 0.066, -0.47), fermuar)                                  # fermuar
    for s in (-1, 1):
        K((0.13, 0.008, 0.05), (mx + s * 0.11, my - 0.068, -0.52), lacivert, 0.004)            # cep kapakları
    K((0.46, 0.006, 0.03), (mx, my - 0.066, -0.40), ortak.malzeme("K5_Reflektor", (0.45, 0.45, 0.40), puruz=0.3))
    # Sağ kancada el feneri: bilek ipinden sarkıyor, kafası aşağıda.
    fx = 0.20
    simit(0.03, 0.0025, (fx, -0.085, -0.03), siyah, olcek=(0.5, 1, 1.6), rot=(0, math.pi / 2, 0), seg=16)
    S(0.019, 0.17, (fx, -0.085, -0.17), fener_m, 16, 0.003)
    koni(0.028, 0.019, 0.04, (fx, -0.085, -0.275), fener_m, 16)
    S(0.028, 0.012, (fx, -0.085, -0.30), krom, 16)
    K((0.012, 0.006, 0.03), (fx, -0.105, -0.13), siyah)                                        # anahtar düğmesi
    ad, h, uzak = "Duvar_Askisi_K5", 0.95, 1.6
    kaldir = 0.87

# =====================================================================
# HOL
# =====================================================================
elif hangi == "yolluk":
    krem = ortak.malzeme("K5_Yolluk_Krem", (0.62, 0.55, 0.40), puruz=0.95)
    mavi = ortak.malzeme("K5_Yolluk_Mavi", (0.03, 0.07, 0.20), puruz=0.95)
    acik_mavi = ortak.malzeme("K5_Yolluk_AcikMavi", (0.12, 0.22, 0.40), puruz=0.95)
    kiremit = ortak.malzeme("K5_Yolluk_Kiremit", (0.30, 0.07, 0.03), puruz=0.95)
    L, W = 3.40, 0.75
    K((L, W, 0.008), (0, 0, 0.004), krem)
    # Kenar kuşağı: dışta koyu mavi bant, içte ince açık mavi çizgi (her kat 0,5 mm yukarıda).
    for s in (-1, 1):
        K((L - 0.06, 0.07, 0.001), (0, s * (W / 2 - 0.065), 0.0085), mavi)
        K((0.07, W - 0.06, 0.001), (s * (L / 2 - 0.065), 0, 0.0085), mavi)
        K((L - 0.26, 0.012, 0.001), (0, s * (W / 2 - 0.13), 0.0085), acik_mavi)
        K((0.012, W - 0.26, 0.001), (s * (L / 2 - 0.13), 0, 0.0085), acik_mavi)
    # Kenar bandındaki küçük krem baklavalar.
    x = -L / 2 + 0.16
    while x < L / 2 - 0.12:
        for s in (-1, 1):
            KR((0.03, 0.03, 0.001), (x, s * (W / 2 - 0.065), 0.009), krem, (0, 0, math.pi / 4))
        x += 0.12
    # Orta alan: dört madalyon (mavi baklava, içinde kiremit ve krem).
    for mx in (-1.2, -0.4, 0.4, 1.2):
        for olc, mat, z in ((0.34, mavi, 0.0085), (0.22, kiremit, 0.009), (0.10, krem, 0.0095), (0.04, mavi, 0.010)):
            KR((olc, olc, 0.001), (mx, 0, z), mat, (0, 0, math.pi / 4))
    for mx in (-0.8, 0.0, 0.8):
        for sy in (-1, 1):
            K((0.10, 0.03, 0.001), (mx, sy * 0.12, 0.0085), acik_mavi)
    # Kısa kenarlarda püsküller.
    for s in (-1, 1):
        for j in range(24):
            K((0.05, 0.008, 0.002), (s * (L / 2 + 0.022), -W / 2 + 0.03 + j * (W - 0.06) / 23, 0.001), krem)
    ad, h, uzak = "Yolluk_K5", 0.2, 2.6

elif hangi == "konsol":
    toprak = ortak.malzeme("K5_Saksi", (0.35, 0.28, 0.20), puruz=0.3)        # krem sırlı saksı
    kuru = ortak.malzeme("K5_KuruYaprak", (0.16, 0.085, 0.025), puruz=0.9)
    sari = ortak.malzeme("K5_SariYaprak", (0.30, 0.19, 0.05), puruz=0.9)
    topr = ortak.malzeme("K5_Toprak", (0.04, 0.025, 0.015), puruz=1.0)
    EN, DER, H = 0.90, 0.30, 0.80
    K((EN, DER, 0.025), (0, 0, H - 0.0125), ceviz, 0.006)                                    # tabla
    K((EN - 0.07, DER - 0.05, 0.08), (0, 0.005, H - 0.025 - 0.04), ceviz, 0.003)             # etek
    K((EN - 0.12, 0.008, 0.035), (0, -DER / 2 + 0.02, H - 0.065), ceviz_ac, 0.002)           # etek süs çıtası
    for x in (-EN / 2 + 0.04, EN / 2 - 0.04):                                                # tornalanmış ayaklar
        for y in (-DER / 2 + 0.035, DER / 2 - 0.035):
            koni(0.012, 0.02, H - 0.105, (x, y, (H - 0.105) / 2), ceviz, 16)
            kure(0.024, (x, y, 0.60), ceviz)
            kure(0.02, (x, y, 0.16), ceviz)
    K((EN - 0.10, DER - 0.08, 0.018), (0, 0, 0.16), ceviz, 0.003)                            # alt raf
    # Saksı: dantelin üstünde, içinde kurumuş eğrelti; yapraklar öne ve yanlara sarkıyor, arkada kısa.
    K((0.26, 0.20, 0.003), (-0.18, -0.01, H + 0.0015), dantel_m)
    sx, sy, sz = -0.18, -0.01, H + 0.003
    saksi = koni(0.055, 0.075, 0.14, (sx, sy, sz + 0.07), toprak, 24, ekle=False)
    ortak.oy(saksi, koni(0.047, 0.067, 0.14, (sx, sy, sz + 0.07 + 0.01), toprak, 24, ekle=False))
    p.append(saksi)
    simit(0.075, 0.006, (sx, sy, sz + 0.14), toprak, seg=24)
    S(0.066, 0.004, (sx, sy, sz + 0.12), topr, 24)
    ust = sz + 0.12
    for i in range(11):
        a = math.radians(170 + i * 20 + rnd.uniform(-6, 6))     # 170..370 derece: ön yarım ve yanlar
        on_mu = math.sin(a) < 0.2
        L = rnd.uniform(0.24, 0.34) if on_mu else 0.12
        onceki = (sx, sy, ust)
        for k in range(1, 7):
            t = k / 6
            nk = (sx + math.cos(a) * L * t, sy + math.sin(a) * L * t, ust + 0.20 * t - (0.30 if on_mu else 0.05) * t * t)
            cubuk(onceki, nk, 0.0022, kuru, 5)
            if k > 1:
                yon = math.atan2(nk[1] - onceki[1], nk[0] - onceki[0])
                for s in (-1, 1):
                    yl = 0.03 * (1.1 - t)
                    KR((yl, 0.009, 0.002), (nk[0] + math.cos(yon + s * 1.4) * yl / 2, nk[1] + math.sin(yon + s * 1.4) * yl / 2, nk[2] - 0.004),
                       sari if (i + k) % 3 == 0 else kuru, (0, s * 0.4, yon + s * 1.4))
            onceki = nk
    # Yere dökülmüş birkaç kuru yaprakçık (tablada).
    for i in range(5):
        KR((0.025, 0.008, 0.002), (sx + rnd.uniform(0.05, 0.25), sy + rnd.uniform(-0.12, 0.05), H + 0.004), kuru, (0, 0, rnd.uniform(0, 3)))
    ad, h, uzak = "Konsol_K5", 0.95, 1.4

elif hangi == "kova":
    plastik = ortak.malzeme("K5_Kova", (0.40, 0.07, 0.025), puruz=0.35)
    su = ortak.malzeme("K5_Su", (0.04, 0.05, 0.05), puruz=0.02)
    islak = ortak.malzeme("K5_IslakLeke", (0.025, 0.02, 0.015), puruz=0.08)
    tel = ortak.malzeme("K5_KovaTel", (0.35, 0.35, 0.34), puruz=0.35, metal=0.9)
    # Islak leke: üst üste binmiş düzensiz yassı diskler (1-2 mm), kovanın altından yayılıyor.
    for i in range(7):
        a = rnd.uniform(0, 2 * math.pi)
        d = rnd.uniform(0.0, 0.18)
        rr = rnd.uniform(0.14, 0.26)
        bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=rr, depth=0.0012, location=(math.cos(a) * d, math.sin(a) * d - 0.05, 0.0006 + i * 0.0001))
        n = bpy.context.active_object
        n.scale = (rnd.uniform(0.8, 1.3), rnd.uniform(0.6, 1.0), 1)
        n.rotation_euler = (0, 0, rnd.uniform(0, 3))
        bpy.ops.object.transform_apply(scale=True, rotation=True)
        ortak.ata(n, islak)
        p.append(n)
    for i in range(6):                                                 # sıçramış damlalar
        a = rnd.uniform(0, 2 * math.pi)
        S(rnd.uniform(0.008, 0.02), 0.0012, (math.cos(a) * 0.42, math.sin(a) * 0.36 - 0.05, 0.0008), islak, 12)
    kova(0.12, 0.155, 0.30, 0, 0, plastik, kal=0.006, z0=0.0015)
    S(0.149, 0.004, (0, 0, 0.26), su, 32)                                                   # ağzına kadar dolu
    for s in (-1, 1):
        K((0.02, 0.03, 0.03), (s * 0.155, 0, 0.285), plastik, 0.004)                         # sap kulakları
    sap_yayi(0, 0, 0.29, 0.165, tel, 0.0028)
    yatay_silindir(0.012, 0.07, (0, 0, 0.29 + 0.165 * 0.8), ortak.malzeme("K5_SapTutamak", (0.02, 0.02, 0.02), puruz=0.5), 10)
    ad, h, uzak = "Su_Kovasi_K5", 0.4, 1.3

# =====================================================================
# SALON
# =====================================================================
elif hangi == "kanepe":
    kadife = ortak.malzeme("K5_Kadife", (0.10, 0.045, 0.02), puruz=0.85)
    kadife_k = ortak.malzeme("K5_KadifeKoyu", (0.07, 0.03, 0.013), puruz=0.9)
    EN, DER = 2.00, 0.85
    for x in (-EN / 2 + 0.07, EN / 2 - 0.07):
        for y in (-DER / 2 + 0.07, DER / 2 - 0.07):
            koni(0.022, 0.03, 0.10, (x, y, 0.05), koyu_ahsap, 16)                              # kısa ahşap ayaklar
    K((EN - 0.28, DER - 0.05, 0.22), (0, 0.0, 0.21), kadife_k, 0.02)                        # oturak kasası
    for i in range(3):                                                                       # oturak minderleri
        K((0.565, 0.64, 0.13), (-0.575 + i * 0.575, -0.075, 0.385), kadife, 0.04)
    K((EN - 0.28, 0.18, 0.56), (0, DER / 2 - 0.09, 0.60), kadife_k, 0.03)                   # arkalık kasası
    for i in range(3):                                                                       # arkalık minderleri, geriye yatık
        KR((0.555, 0.15, 0.42), (-0.575 + i * 0.575, 0.21, 0.66), kadife, (math.radians(-10), 0, 0), 0.05)
        for dx in (-0.14, 0.14):
            kure(0.012, (-0.575 + i * 0.575 + dx, 0.13, 0.70), kadife_k)                   # kapitone düğmeler
    for s in (-1, 1):                                                                        # kollar
        K((0.14, DER, 0.52), (s * (EN / 2 - 0.07), 0, 0.36), kadife, 0.025)
        K((0.16, DER - 0.04, 0.035), (s * (EN / 2 - 0.07), -0.01, 0.6375), ceviz, 0.008)    # ahşap kol başlığı
        K((0.16, 0.035, 0.32), (s * (EN / 2 - 0.07), -DER / 2 - 0.005, 0.46), ceviz, 0.01)  # önde ahşap levha
        kure(0.022, (s * (EN / 2 - 0.07), -DER / 2 - 0.025, 0.56), ceviz_ac)
    # Arkalığın tepesinde ortada dantel (yarısı öne sarkıyor).
    K((0.40, 0.18, 0.003), (0, DER / 2 - 0.09, 0.8815), dantel_m)
    K((0.40, 0.003, 0.10), (0, DER / 2 - 0.18 - 0.0015, 0.83), dantel_m)
    for i in range(9):
        K((0.03, 0.003, 0.025), (-0.18 + i * 0.045, DER / 2 - 0.1815, 0.768), dantel_m)
    ad, h, uzak = "Kanepe_K5", 0.9, 2.6

elif hangi == "tv":
    plastik = ortak.malzeme("K5_TVPlastik", (0.018, 0.018, 0.02), puruz=0.45)
    tv_ahsap = ortak.malzeme("K5_TVAhsap", (0.10, 0.05, 0.02), puruz=0.35)
    ekran = ortak.malzeme("K5_Ekran", (0.025, 0.032, 0.032), puruz=0.08)
    gumus = ortak.malzeme("K5_Gumus", (0.55, 0.55, 0.55), puruz=0.3, metal=0.9)
    cam = ortak.malzeme("K5_VitrinCam", (0.08, 0.085, 0.08), puruz=0.04)
    EN, DER, H = 1.00, 0.45, 0.55
    p.extend(ac.govde_levhalari(EN, DER, H, ceviz, taban=0.05, raflar=(0.30,), dikmeler=(0.0,)))
    K((EN - 0.04, DER - 0.04, 0.05), (0, 0.01, 0.025), koyu_ahsap, 0.003)                   # süpürgelik
    # İçerik: sol üst bölmede video, alt bölmelerde kasetler, sağ üstte dergiler.
    K((0.36, 0.28, 0.075), (-0.25, 0.02, 0.30 + 0.0375), plastik, 0.004)
    K((0.20, 0.005, 0.012), (-0.28, -0.12, 0.345), siyah)
    for i in range(10):
        K((0.025, 0.19, 0.105), (-0.46 + i * 0.03, 0.02, 0.068 + 0.0525), plastik, 0.001)
    for i in range(6):
        K((0.025, 0.19, 0.105), (0.05 + i * 0.03, 0.02, 0.068 + 0.0525), ortak.malzeme("K5_KasetKapak", (0.35, 0.33, 0.28), puruz=0.6) if i % 2 else plastik, 0.001)
    K((0.30, 0.22, 0.04), (0.24, 0.02, 0.30 + 0.02), ortak.malzeme("K5_Dergi", (0.40, 0.30, 0.22), puruz=0.8), 0.002)
    # Cam kapaklar: çerçeve + koyu cam (süs, açılmaz).
    y_on = -DER / 2 - 0.009
    for x0, x1 in ((-EN / 2 + 0.01, -0.005), (0.005, EN / 2 - 0.01)):
        xm, w = (x0 + x1) / 2, x1 - x0
        for (cx, cz, cw, ch) in ((xm, 0.08, w, 0.03), (xm, 0.52, w, 0.03), (x0 + 0.015, 0.30, 0.03, 0.47), (x1 - 0.015, 0.30, 0.03, 0.47)):
            K((cw, 0.018, ch), (cx, y_on, cz), ceviz_ac, 0.003)
        K((w - 0.03, 0.004, 0.41), (xm, y_on, 0.30), cam)
        kure(0.012, (x1 - 0.04 if x0 < 0 else x0 + 0.04, y_on - 0.02, 0.30), pirinc)
    # Tüplü TV: siyah ön kasa, iki yanı ve üstü ahşap kaplama, arkada daralan tüp kasası.
    tz = H
    K((0.62, 0.30, 0.50), (0, -0.06, tz + 0.25), plastik, 0.01)
    for s in (-1, 1):
        K((0.025, 0.30, 0.52), (s * 0.3225, -0.06, tz + 0.26), tv_ahsap, 0.004)
    K((0.67, 0.30, 0.025), (0, -0.06, tz + 0.5125), tv_ahsap, 0.005)
    K((0.46, 0.12, 0.38), (0, 0.15, tz + 0.25), plastik, 0.03)
    K((0.47, 0.03, 0.37), (-0.06, -0.212, tz + 0.28), plastik, 0.01)                         # ekran çerçevesi
    K((0.43, 0.02, 0.33), (-0.06, -0.222, tz + 0.28), ekran, 0.03)                           # bombeli ekran camı
    for i in range(5):
        K((0.03, 0.008, 0.016), (0.245, -0.214, tz + 0.43 - i * 0.03), gumus if i == 0 else siyah, 0.002)
    for i in range(6):
        K((0.09, 0.004, 0.005), (0.245, -0.212, tz + 0.08 + i * 0.016), siyah)             # hoparlör yarıkları
    K((0.08, 0.004, 0.012), (-0.06, -0.212, tz + 0.065), gumus)                              # marka
    # TV'nin üstünde dantel örtü ve küçük porselen vazo.
    K((0.40, 0.26, 0.003), (0, -0.07, tz + 0.5265), dantel_m)
    for i in range(8):
        K((0.03, 0.003, 0.03), (-0.16 + i * 0.0457, -0.2115, tz + 0.51), dantel_m)
    porselen = ortak.malzeme("K5_Porselen", (0.75, 0.72, 0.66), puruz=0.15)
    S(0.03, 0.13, (0.15, -0.04, tz + 0.528 + 0.065), porselen, 20, 0.01)
    S(0.018, 0.03, (0.15, -0.04, tz + 0.528 + 0.145), porselen, 16)
    ad, h, uzak = "TV_Unitesi_K5", 1.1, 1.8

elif hangi == "hali":
    kahve = ortak.malzeme("K5_Hali_Kahve", (0.13, 0.065, 0.028), puruz=0.95)
    bej = ortak.malzeme("K5_Hali_Bej", (0.46, 0.35, 0.21), puruz=0.95)
    krem = ortak.malzeme("K5_Hali_Krem", (0.62, 0.54, 0.38), puruz=0.95)
    pas = ortak.malzeme("K5_Hali_Pas", (0.28, 0.08, 0.03), puruz=0.95)
    lac = ortak.malzeme("K5_Hali_Lacivert", (0.03, 0.04, 0.10), puruz=0.95)
    yesil = ortak.malzeme("K5_Hali_Yesil", (0.07, 0.13, 0.06), puruz=0.95)
    L, W, B = 2.50, 1.70, 0.18
    K((L, W, 0.008), (0, 0, 0.004), kahve)                                                    # taban ve dış bordür
    K((L - 2 * B, W - 2 * B, 0.001), (0, 0, 0.0085), bej)                                    # zemin
    for s in (-1, 1):                                                                        # krem şeritler
        K((L - 2 * B + 0.05, 0.025, 0.001), (0, s * (W / 2 - B), 0.009), krem)
        K((0.025, W - 2 * B + 0.05, 0.001), (s * (L / 2 - B), 0, 0.009), krem)
        K((L - 0.06, 0.012, 0.001), (0, s * (W / 2 - 0.03), 0.0085), pas)
        K((0.012, W - 0.06, 0.001), (s * (L / 2 - 0.03), 0, 0.0085), pas)
    # Bordür motifleri: kahve bant boyunca pas ve krem baklavalar.
    x = -L / 2 + 0.2
    while x < L / 2 - 0.1:
        for s in (-1, 1):
            KR((0.07, 0.07, 0.001), (x, s * (W / 2 - B / 2 - 0.01), 0.0085), pas if int(x * 5) % 2 else krem, (0, 0, math.pi / 4))
        x += 0.20
    y = -W / 2 + 0.3
    while y < W / 2 - 0.2:
        for s in (-1, 1):
            KR((0.07, 0.07, 0.001), (s * (L / 2 - B / 2 - 0.01), y, 0.0085), pas, (0, 0, math.pi / 4))
        y += 0.20

    def baklava(en, boy, konum, mat):
        """Kare 45° döndürülüp en/boy oranında ölçeklenir: uzun baklava (madalyon katmanı)."""
        return KR((1, 1, 0.001), konum, mat, (0, 0, math.pi / 4), olcek=(en / math.sqrt(2), boy / math.sqrt(2), 1))

    # Orta madalyon: kahve, pas, krem, lacivert iç içe baklavalar.
    for (e, b, mat, z) in ((1.00, 0.70, kahve, 0.0090), (0.78, 0.54, pas, 0.0095), (0.50, 0.34, krem, 0.0100), (0.20, 0.14, lac, 0.0105)):
        baklava(e, b, (0, 0, z), mat)
    for s in (-1, 1):                                                                        # madalyonun uç süsleri
        baklava(0.16, 0.10, (s * 0.60, 0, 0.0090), kahve)
    # Renkli yamalar: köşelere ve yanlara simetrik küçük motifler.
    for sx in (-1, 1):
        for sy in (-1, 1):
            baklava(0.26, 0.18, (sx * 0.78, sy * 0.44, 0.0090), kahve)
            baklava(0.12, 0.08, (sx * 0.78, sy * 0.44, 0.0095), yesil)
            K((0.06, 0.06, 0.001), (sx * 0.40, sy * 0.52, 0.0090), lac)
            K((0.03, 0.03, 0.001), (sx * 0.40, sy * 0.52, 0.0095), pas)
    for sx in (-1, 1):
        K((0.05, 0.14, 0.001), (sx * 0.92, 0, 0.0090), yesil)
    # Kısa kenarlarda püsküller.
    for s in (-1, 1):
        for j in range(40):
            K((0.06, 0.008, 0.002), (s * (L / 2 + 0.028), -W / 2 + 0.03 + j * (W - 0.06) / 39, 0.001), krem)
    ad, h, uzak = "Hali_K5", 0.2, 2.8

elif hangi == "perde":
    duvar = True
    fon = ortak.malzeme("K5_FonPerde", (0.09, 0.045, 0.022), puruz=0.85)
    tul = ortak.malzeme("K5_Tul", (0.80, 0.78, 0.72), puruz=0.9)
    EN, BOY, ARA = 1.90, 2.45, 0.11
    # Ahşap korniş: boru, iki ucunda oymalı topuz, iki taşıyıcı.
    yatay_silindir(0.017, EN, (0, -ARA, 0), ceviz)
    for s in (-1, 1):
        kure(0.032, (s * (EN / 2 + 0.03), -ARA, 0), ceviz, olcek=(1.3, 1, 1))
        yatay_silindir(0.022, 0.03, (s * (EN / 2 - 0.005), -ARA, 0), ceviz_ac)
        K((0.025, ARA, 0.035), (s * (EN / 2 - 0.12), -ARA / 2, 0), ceviz, 0.004)
    # Tül: boydan boya, küçük pililer, kornişin hemen arkasında (Unity'de yarı saydam malzeme takılabilir).
    pilili("Tul", -EN / 2 + 0.05, EN / 2 - 0.05, -0.03, BOY - 0.02, -ARA + 0.035, 0.012, 0.035, tul, 1.0, 0.002)
    # Fon perdeler: iki yanda, kalın pileler, altta hafif açılarak yere iner.
    for s in (-1, 1):
        x_dis, x_ic = s * (EN / 2 - 0.02), s * (EN / 2 - 0.50)
        pilili("Fon", min(x_dis, x_ic), max(x_dis, x_ic), -0.02, BOY, -ARA - 0.03, 0.028, 0.06, fon, 1.12, 0.005)
        for i in range(8):                                                                    # halkalar
            simit(0.024, 0.004, (x_dis - s * i * 0.065, -ARA, 0), pirinc, rot=(0, math.pi / 2, 0), seg=16)
    ad, h, uzak = "Perde_K5", 2.55, 3.4
    kaldir = 2.47

# =====================================================================
# MUTFAK
# =====================================================================
elif hangi == "buzdolabi":
    beyaz = ortak.malzeme("K5_BuzBeyaz", (0.80, 0.80, 0.77), puruz=0.3)
    gri = ortak.malzeme("K5_BuzKol", (0.45, 0.45, 0.44), puruz=0.35)
    EN, DER, H = 0.60, 0.62, 1.70
    K((EN, DER - 0.05, H), (0, 0.025, H / 2), beyaz, 0.015)                                  # gövde
    y_on = 0.025 - (DER - 0.05) / 2
    K((EN - 0.006, 0.045, 0.43), (0, y_on - 0.0225, 1.47), beyaz, 0.012)                     # dondurucu kapısı
    K((EN - 0.006, 0.045, 1.14), (0, y_on - 0.0225, 0.66 + 0.02), beyaz, 0.012)              # soğutucu kapısı
    K((EN - 0.01, 0.01, 0.012), (0, y_on - 0.004, 1.245), siyah)                             # iki kapı arası derz
    K((EN - 0.02, 0.012, 0.075), (0, y_on - 0.006, 0.0375), siyah)                          # alt ızgara
    for i in range(5):
        K((EN - 0.08, 0.004, 0.006), (0, y_on - 0.013, 0.015 + i * 0.013), gri)
    for (z0, z1) in ((1.29, 1.46), (0.98, 1.20)):                                            # dikey kollar (sağ kenarda)
        K((0.028, 0.03, z1 - z0), (EN / 2 - 0.05, y_on - 0.075, (z0 + z1) / 2), gri, 0.008)
        for z in (z0 + 0.012, z1 - 0.012):
            K((0.02, 0.03, 0.02), (EN / 2 - 0.05, y_on - 0.057, z), gri, 0.004)
    K((0.10, 0.004, 0.018), (-EN / 2 + 0.10, y_on - 0.046, 1.20), krom, 0.002)               # marka plakası
    # Soğutucu kapısında mıknatıslı küçük takvim kartı (evin düzenli sahibinden).
    K((0.08, 0.002, 0.11), (-0.12, y_on - 0.046, 0.95), ortak.malzeme("K5_Kart", (0.70, 0.62, 0.45), puruz=0.8))
    dik_disk(0.01, 0.006, (-0.12, y_on - 0.049, 1.0), ortak.malzeme("K5_Miknatis", (0.05, 0.12, 0.30), puruz=0.4), 12)
    ad, h, uzak = "Buzdolabi_K5", 1.7, 2.3

elif hangi == "ocak":
    krem = ortak.malzeme("K5_OcakKrem", (0.66, 0.58, 0.42), puruz=0.3)
    inox = ortak.malzeme("K5_OcakInox", (0.50, 0.50, 0.48), puruz=0.3, metal=0.9)
    dokum = ortak.malzeme("K5_OcakDokum", (0.02, 0.02, 0.02), puruz=0.6, metal=0.4)
    kahve_cam = ortak.malzeme("K5_OcakCam", (0.10, 0.07, 0.045), puruz=0.04)
    kirmizi = ortak.malzeme("K5_Lamba", (0.60, 0.03, 0.02), puruz=0.3)
    EN, DER, H = 0.60, 0.60, 0.85
    K((EN, DER, H - 0.01), (0, 0, (H - 0.01) / 2), krem, 0.01)
    K((EN - 0.01, DER - 0.07, 0.012), (0, -0.03, H - 0.004), inox, 0.004)                    # üst tabla
    # Gözler: üç gaz (döküm ızgara + başlık), sağ önde bir elektrik (düz siyah plaka, ortası krom).
    for (x, y, tur) in ((-0.14, -0.15, "gaz"), (-0.14, 0.09, "gaz"), (0.14, 0.09, "gaz"), (0.14, -0.15, "elektrik")):
        if tur == "gaz":
            S(0.035, 0.012, (x, y, H + 0.008), dokum, 20)
            S(0.022, 0.012, (x, y, H + 0.018), inox, 16)
            simit(0.075, 0.005, (x, y, H + 0.032), dokum, seg=20)
            for a in range(4):
                KR((0.10, 0.008, 0.012), (x + math.cos(a * math.pi / 2 + 0.78) * 0.05, y + math.sin(a * math.pi / 2 + 0.78) * 0.05, H + 0.028),
                   dokum, (0, 0, a * math.pi / 2 + 0.78))
        else:
            S(0.085, 0.014, (x, y, H + 0.009), dokum, 28, 0.003)
            S(0.015, 0.016, (x, y, H + 0.010), inox, 12)
            simit(0.087, 0.004, (x, y, H + 0.012), inox, seg=28)
    # Cam kapak: arkada menteşeli, dik kalkık (6° geriye yaslı).
    a = math.radians(6)
    y0, z0 = DER / 2 - 0.035, H + 0.012
    KR((EN - 0.02, 0.006, 0.50), (0, y0 + 0.25 * math.sin(a), z0 + 0.25 * math.cos(a)), kahve_cam, (-a, 0, 0), 0.002)
    for s in (-1, 1):
        K((0.03, 0.02, 0.025), (s * 0.22, y0, z0 + 0.01), inox, 0.004)                       # menteşeler
    # Ön panel: beş düğme (4 göz + fırın), kırmızı ikaz lambası, fırın kapağı ve cam, çekmece.
    y_on = -DER / 2
    K((EN - 0.02, 0.006, 0.08), (0, y_on - 0.003, 0.77), inox)
    for i in range(5):
        dik_disk(0.02, 0.025, (-0.22 + i * 0.105, y_on - 0.018, 0.77), siyah, 16)
        K((0.004, 0.006, 0.026), (-0.22 + i * 0.105, y_on - 0.032, 0.77), krom)
    dik_disk(0.006, 0.008, (0.27, y_on - 0.008, 0.77), kirmizi, 10)
    K((EN - 0.04, 0.025, 0.44), (0, y_on - 0.012, 0.44), krem, 0.008)
    K((EN - 0.22, 0.004, 0.22), (0, y_on - 0.026, 0.46), cam_koyu)
    K((EN - 0.12, 0.028, 0.02), (0, y_on - 0.045, 0.635), krom, 0.008)
    for s in (-1, 1):
        K((0.02, 0.03, 0.02), (s * (EN / 2 - 0.08), y_on - 0.035, 0.635), krom)
    K((EN - 0.04, 0.02, 0.13), (0, y_on - 0.01, 0.1), krem, 0.006)
    K((0.14, 0.02, 0.015), (0, y_on - 0.027, 0.12), krom, 0.004)
    ad, h, uzak = "Ocak_K5", 1.2, 1.8

elif hangi == "masa":
    formika = ortak.malzeme("K5_Formika", (0.55, 0.40, 0.05), puruz=0.35)
    alu = ortak.malzeme("K5_Aluminyum", (0.55, 0.55, 0.53), puruz=0.3, metal=0.9)
    boya = ortak.malzeme("K5_MasaBoya", (0.60, 0.55, 0.42), puruz=0.5)
    EN, DER, H = 1.00, 0.65, 0.75
    K((EN - 0.008, DER - 0.008, 0.026), (0, 0, H - 0.013), formika, 0.001)                  # formika üst
    for s in (-1, 1):                                                                        # alüminyum kenar bandı
        K((EN, 0.004, 0.028), (0, s * (DER / 2 - 0.002), H - 0.014), alu)
        K((0.004, DER, 0.028), (s * (EN / 2 - 0.002), 0, H - 0.014), alu)
    K((EN - 0.10, DER - 0.10, 0.07), (0, 0, H - 0.026 - 0.035), boya, 0.004)                 # metal çerçeve (krem boya)
    K((0.30, 0.01, 0.05), (0.20, -DER / 2 + 0.045, H - 0.061), boya, 0.003)                  # çatal kaşık çekmecesi önü
    K((0.08, 0.02, 0.012), (0.20, -DER / 2 + 0.035, H - 0.061), krom, 0.004)
    for x in (-EN / 2 + 0.06, EN / 2 - 0.06):                                               # kare boru ayaklar
        for y in (-DER / 2 + 0.06, DER / 2 - 0.06):
            K((0.03, 0.03, H - 0.026), (x, y, (H - 0.026) / 2), krom, 0.004)
            K((0.034, 0.034, 0.012), (x, y, 0.006), siyah, 0.003)
    ad, h, uzak = "Mutfak_Masasi_K5", 0.8, 1.7

elif hangi == "tabure":
    formika = ortak.malzeme("K5_Formika", (0.55, 0.40, 0.05), puruz=0.35)
    alu = ortak.malzeme("K5_Aluminyum", (0.55, 0.55, 0.53), puruz=0.3, metal=0.9)
    S_, H = 0.33, 0.45
    K((S_ - 0.006, S_ - 0.006, 0.024), (0, 0, H - 0.012), formika, 0.001)
    for s in (-1, 1):
        K((S_, 0.003, 0.026), (0, s * (S_ / 2 - 0.0015), H - 0.013), alu)
        K((0.003, S_, 0.026), (s * (S_ / 2 - 0.0015), 0, H - 0.013), alu)
    K((S_ - 0.06, S_ - 0.06, 0.03), (0, 0, H - 0.024 - 0.015), krom, 0.003)
    ayaklar = []
    for sx in (-1, 1):                                                                       # hafif açılı dört ayak
        for sy in (-1, 1):
            ust = (sx * 0.12, sy * 0.12, H - 0.04)
            alt = (sx * 0.15, sy * 0.15, 0.01)
            cubuk(ust, alt, 0.012, krom, 10)
            S(0.014, 0.012, (alt[0], alt[1], 0.006), siyah, 10)
    t = (0.18 - 0.01) / (H - 0.04 - 0.01)                                                   # ayak boyunca 0,18 m noktası
    r_ = 0.15 - 0.03 * t
    for (a, b) in (((-r_, -r_), (r_, -r_)), ((r_, -r_), (r_, r_)), ((r_, r_), (-r_, r_)), ((-r_, r_), (-r_, -r_))):
        cubuk((a[0], a[1], 0.18), (b[0], b[1], 0.18), 0.007, krom, 8)                       # ayaklık çemberi
    ad, h, uzak = "Tabure_K5", 0.5, 1.1

elif hangi == "sepet":
    hasir1 = ortak.malzeme("K5_Hasir", (0.36, 0.24, 0.09), puruz=0.9)
    hasir2 = ortak.malzeme("K5_HasirKoyu", (0.24, 0.15, 0.05), puruz=0.9)
    kabuk = ortak.malzeme("K5_Ekmek_Kabuk", (0.30, 0.15, 0.05), puruz=0.8)
    ici = ortak.malzeme("K5_Ekmek_Ic", (0.52, 0.42, 0.26), puruz=0.95)
    EN, DER, H = 0.36, 0.24, 0.10
    oran = DER / EN
    bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=EN / 2 - 0.02, depth=0.012, location=(0, 0, 0.006))
    n = bpy.context.active_object; n.scale = (1, (DER - 0.04) / (EN - 0.04), 1); bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, hasir2); p.append(n)
    for i in range(8):                                                                        # örgü halkaları, yukarı açılarak
        R = EN / 2 - 0.02 + i * 0.0035
        simit(R, 0.0075, (0, 0, 0.012 + i * 0.012), hasir1 if i % 2 == 0 else hasir2,
              olcek=(1, (R * 2 * oran + 0.01) / (R * 2), 1), seg=32)
    Rt = EN / 2 - 0.02 + 8 * 0.0035
    simit(Rt, 0.011, (0, 0, H + 0.005), hasir2, olcek=(1, (Rt * 2 * oran + 0.01) / (Rt * 2), 1), seg=32)
    # Bayat yarım ekmek: somunun bir ucu kesilmiş (kesik yüzü açık renk iç), yanında bir dilim.
    bpy.ops.mesh.primitive_uv_sphere_add(radius=1, location=(-0.03, 0.0, 0.06), segments=24, ring_count=12)
    somun = bpy.context.active_object
    somun.scale = (0.15, 0.065, 0.055)
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(somun, kabuk)
    ortak.oy(somun, ortak.kutu("Kes", (0.2, 0.2, 0.2), (0.07 + 0.1, 0, 0.06), kabuk))
    p.append(somun)
    # Kesik yüz: x'e bakan elips disk. Somunun x = 0,07'deki kesiti: yükseklik yarıçapı ~0,048, genişlik ~0,057.
    # Disk y ekseninde 90° döndürülünce yerel x dünyada dikey olur; ölçek (dikey, yatay, kalınlık).
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=1, depth=0.002, location=(0.0695, 0, 0.06), rotation=(0, math.pi / 2, 0))
    n = bpy.context.active_object; n.scale = (0.047, 0.056, 1)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, ici); p.append(n)
    KR((0.10, 0.012, 0.075), (0.125, -0.02, 0.07), ici, (0, math.radians(35), math.radians(10)), 0.01)    # sepete dayalı dilim
    ad, h, uzak = "Ekmek_Sepeti_K5", 0.15, 0.7

# =====================================================================
# BANYO
# =====================================================================
elif hangi == "camasir":
    beyaz = ortak.malzeme("K5_CamasirBeyaz", (0.78, 0.78, 0.76), puruz=0.3)
    mavi = ortak.malzeme("K5_CamasirMavi", (0.04, 0.14, 0.38), puruz=0.3)
    seffaf = ortak.malzeme("K5_CamasirKapak", (0.06, 0.20, 0.42), puruz=0.1)
    gri = ortak.malzeme("K5_Hortum", (0.30, 0.30, 0.30), puruz=0.6)
    EN, DER, H = 0.75, 0.45, 0.85
    K((EN - 0.01, DER - 0.01, 0.05), (0, 0, 0.025), mavi, 0.008)                            # mavi taban
    K((EN, DER, H - 0.13), (0, 0, 0.04 + (H - 0.13) / 2), beyaz, 0.025)                     # gövde (üst 0,76)
    K((EN, 0.11, 0.09), (0, DER / 2 - 0.055, 0.76 + 0.045), mavi, 0.012)                     # arkada kumanda paneli
    for i, x in enumerate((-0.25, 0.0, 0.25)):                                               # yıkama, seçici, sıkma düğmeleri
        dik_disk(0.028, 0.02, (x, DER / 2 - 0.11 - 0.008, 0.805), beyaz, 20)
        K((0.006, 0.008, 0.035), (x, DER / 2 - 0.11 - 0.02, 0.805), mavi)
    K((EN - 0.02, DER - 0.12, 0.012), (0, -0.055, 0.766), beyaz, 0.004)                      # üst tabla
    # İki kapak: solda büyük yıkama kazanı, sağda küçük sıkma kazanı; ön kenarda tutamak girintisi.
    for (x, w) in ((-0.13, 0.44), (0.235, 0.24)):
        K((w, DER - 0.15, 0.018), (x, -0.06, 0.781), seffaf, 0.006)
        K((0.08, 0.02, 0.01), (x, -DER / 2 + 0.05, 0.792), mavi, 0.003)
    K((0.012, DER - 0.14, 0.02), (0.1, -0.06, 0.78), mavi)                                  # kapaklar arası çıta
    # Önde mavi şerit ve marka plakası, altta tahliye hortumu yere doğru kıvrılıyor.
    K((EN + 0.002, 0.004, 0.03), (0, -DER / 2 - 0.001, 0.62), mavi)
    K((0.14, 0.004, 0.03), (-0.22, -DER / 2 - 0.002, 0.68), krom, 0.002)
    for i in range(3):
        K((0.12, 0.004, 0.008), (0.22, -DER / 2 - 0.002, 0.14 + i * 0.02), gri)               # ön havalandırma yarıkları
    onceki = (EN / 2 + 0.01, 0.10, 0.12)
    for i in range(1, 9):
        t = i / 8
        nk = (EN / 2 + 0.01 + 0.10 * math.sin(t * 2.5), 0.10 - 0.25 * t, 0.12 * (1 - t) + 0.015)
        cubuk(onceki, nk, 0.013, gri, 10)
        onceki = nk
    ad, h, uzak = "Camasir_Makinesi_K5", 0.9, 1.7

elif hangi == "havlu":
    duvar = True
    pembe = ortak.malzeme("K5_HavluPembe", (0.45, 0.16, 0.18), puruz=0.95)
    mavi = ortak.malzeme("K5_HavluMavi", (0.06, 0.12, 0.30), puruz=0.95)
    krem = ortak.malzeme("K5_HavluSerit", (0.70, 0.64, 0.50), puruz=0.95)
    EN = 0.60
    for s in (-1, 1):                                                                        # duvar taşıyıcıları
        dik_disk(0.025, 0.012, (s * EN / 2, -0.006, 0), krom, 20)
        cubuk((s * EN / 2, -0.012, 0), (s * EN / 2, -0.13, 0), 0.009, krom)
    for y in (-0.06, -0.12):                                                                 # iki çubuk
        yatay_silindir(0.008, EN, (0, y, 0), krom)

    def havlu(y, en, boy_on, boy_arka, mat, serit):
        """Çubuğa katlanıp asılmış havlu: üstte kıvrım, önde ve arkada sarkan iki kat."""
        yatay_silindir(0.016, en, (0, y, 0.004), mat, 12)
        K((en, 0.008, boy_arka), (0, y + 0.013, -boy_arka / 2), mat, 0.003)
        K((en, 0.008, boy_on), (0, y - 0.013, -boy_on / 2), mat, 0.003)
        if serit:
            K((en + 0.002, 0.009, 0.035), (0, y - 0.013, -boy_on + 0.07), serit)

    havlu(-0.06, 0.42, 0.52, 0.48, pembe, None)
    havlu(-0.12, 0.36, 0.38, 0.34, mavi, krem)
    ad, h, uzak = "Havlu_Askisi_K5", 0.6, 1.2
    kaldir = 0.55

elif hangi == "ecza":
    duvar = True
    krem = ortak.malzeme("K5_EczaKrem", (0.72, 0.68, 0.58), puruz=0.35)
    ayna = ortak.malzeme("K5_Ayna", (0.60, 0.62, 0.63), puruz=0.03, metal=1.0)
    bardak_m = ortak.malzeme("K5_Bardak", (0.30, 0.40, 0.38), puruz=0.1)
    EN, DER, H = 0.50, 0.15, 0.60
    K((EN, DER - 0.02, H), (0, -(DER - 0.02) / 2, 0), krem, 0.006)                           # gövde (arka y=0)
    K((EN + 0.02, DER + 0.005, 0.03), (0, -(DER + 0.005) / 2, H / 2 + 0.015), krem, 0.008)   # üst korniş
    K((EN - 0.02, 0.02, H - 0.03), (0, -DER + 0.01, 0), krem, 0.005)                         # kapak çerçevesi
    K((EN - 0.08, 0.004, H - 0.10), (0, -DER - 0.002, 0), ayna)                              # ayna
    K((0.012, 0.015, 0.05), (EN / 2 - 0.035, -DER - 0.009, 0), krom, 0.003)                  # kulp
    # Altında cam raf: iki diş fırçası, bir bardak ve bir diş macunu.
    K((EN - 0.06, 0.11, 0.008), (0, -0.065, -H / 2 - 0.06), bardak_m, 0.002)
    for s in (-1, 1):
        K((0.012, 0.10, 0.02), (s * (EN / 2 - 0.05), -0.05, -H / 2 - 0.05), krom, 0.003)
    bz = -H / 2 - 0.056
    bardak = ortak.silindir("Bardak", 0.03, 0.09, (-0.12, -0.07, bz + 0.045), bardak_m, 20)
    ortak.oy(bardak, ortak.silindir("BardakIc", 0.026, 0.09, (-0.12, -0.07, bz + 0.05), bardak_m, 20))
    p.append(bardak)
    for dx, renk in ((-0.008, (0.40, 0.05, 0.04)), (0.01, (0.04, 0.10, 0.35))):
        cubuk((-0.12 + dx, -0.07, bz + 0.01), (-0.12 + dx * 3, -0.075, bz + 0.17), 0.004, ortak.malzeme("K5_Firca%d" % int(dx * 1000 + 10), renk, puruz=0.4))
    KR((0.15, 0.03, 0.03), (0.08, -0.07, bz + 0.015), ortak.malzeme("K5_Macun", (0.75, 0.75, 0.75), puruz=0.3), (0, 0, 0.15), 0.01)
    ad, h, uzak = "Banyo_Dolabi_K5", 0.8, 1.2
    kaldir = 0.42

elif hangi == "legen":
    yesil = ortak.malzeme("K5_Legen", (0.05, 0.22, 0.09), puruz=0.35)
    mavi = ortak.malzeme("K5_MaviKova", (0.04, 0.10, 0.32), puruz=0.35)
    kova(0.19, 0.26, 0.13, -0.14, 0.0, yesil, kal=0.006)                                     # leğen
    for s in (-1, 1):
        K((0.05, 0.03, 0.012), (-0.14 + s * 0.265, 0, 0.122), yesil, 0.004)                  # leğen tutamakları
    # Leğenin içinde katlanmış bez ve bir kalıp sabun.
    K((0.18, 0.14, 0.02), (-0.18, 0.03, 0.017), ortak.malzeme("K5_Bez", (0.55, 0.50, 0.35), puruz=0.95), 0.008)
    K((0.08, 0.05, 0.025), (-0.06, -0.05, 0.02), ortak.malzeme("K5_Sabun", (0.60, 0.50, 0.25), puruz=0.5), 0.008)
    kova(0.105, 0.135, 0.28, 0.30, 0.05, mavi, kal=0.006)                                    # kova
    for z in (0.07, 0.14, 0.21):
        r_ = 0.105 + 0.03 * z / 0.28
        simit(r_ + 0.002, 0.004, (0.30, 0.05, z), mavi, seg=28)                             # kaburgalar
    for s in (-1, 1):
        K((0.02, 0.025, 0.03), (0.30 + s * 0.137, 0.05, 0.26), mavi, 0.004)
    sap_yayi(0.30, 0.05, 0.265, 0.145, mavi, 0.005)
    ad, h, uzak = "Legen_Kova_K5", 0.35, 1.3

# =====================================================================
# KİLER
# =====================================================================
elif hangi == "raf":
    cam = ortak.malzeme("K5_Kavanoz", (0.45, 0.50, 0.45), puruz=0.05)
    recel = ortak.malzeme("K5_Recel", (0.30, 0.05, 0.03), puruz=0.3)
    tursu = ortak.malzeme("K5_Tursu", (0.16, 0.22, 0.05), puruz=0.3)
    kapak = ortak.malzeme("K5_KavanozKapak", (0.50, 0.40, 0.10), puruz=0.35, metal=0.8)
    teneke = ortak.malzeme("K5_Teneke", (0.45, 0.45, 0.42), puruz=0.3, metal=0.85)
    etiket = [ortak.malzeme("K5_Etiket%d" % i, c, puruz=0.6) for i, c in enumerate(((0.05, 0.20, 0.06), (0.50, 0.30, 0.03), (0.40, 0.05, 0.03)))]
    boya_b = ortak.malzeme("K5_BoyaKutusu", (0.72, 0.72, 0.70), puruz=0.4)
    damla = [ortak.malzeme("K5_Boya%d" % i, c, puruz=0.3) for i, c in enumerate(((0.62, 0.55, 0.40), (0.10, 0.25, 0.45), (0.45, 0.10, 0.05)))]
    cam_ahsap = ortak.malzeme("K5_RafCam", (0.20, 0.12, 0.05), puruz=0.6)       # işlenmemiş çam
    EN, DER, H = 1.20, 0.40, 1.80
    for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
        for y in (-DER / 2 + 0.02, DER / 2 - 0.02):
            K((0.04, 0.04, H), (x, y, H / 2), cam_ahsap, 0.004)
    raflar = (0.10, 0.52, 0.94, 1.36, 1.78)
    for z in raflar:
        K((EN, DER, 0.022), (0, 0, z - 0.011), cam_ahsap, 0.003)
    for s in (-1, 1):                                                                        # arkada çapraz destek
        cubuk((-EN / 2 + 0.04, DER / 2 - 0.01, 0.12 if s > 0 else 1.34), (EN / 2 - 0.04, DER / 2 - 0.01, 1.34 if s > 0 else 0.12), 0.008, cam_ahsap)
    # Alt raf: iki yağ tenekesi ve boya kutuları.
    z = raflar[0]
    for x in (-0.42, -0.16):
        K((0.24, 0.24, 0.34), (x, 0.0, z + 0.17), teneke, 0.006)
        K((0.24 + 0.002, 0.2, 0.16), (x, 0.0, z + 0.16), etiket[1], 0.0)
        S(0.02, 0.02, (x + 0.07, -0.06, z + 0.35), teneke, 12)
    for i, x in enumerate((0.10, 0.30, 0.48)):
        S(0.085, 0.19, (x, 0.0, z + 0.095), boya_b, 28, 0.004)
        S(0.087, 0.012, (x, 0.0, z + 0.19), teneke, 28)
        S(0.0862, 0.07, (x, 0.0, z + 0.10), damla[i], 28)
        K((0.012, 0.004, 0.05), (x + 0.03, -0.086, z + 0.165), damla[i])                      # kenardan akmış boya
    # İkinci raf: küçük boya kutuları, bir tel fırça kutusu.
    z = raflar[1]
    for i in range(5):
        x = -0.48 + i * 0.12
        S(0.05, 0.11, (x, -0.03, z + 0.055), boya_b, 20, 0.003)
        S(0.0505, 0.04, (x, -0.03, z + 0.06), damla[i % 3], 20)
        S(0.052, 0.008, (x, -0.03, z + 0.114), teneke, 20)
    K((0.28, 0.20, 0.12), (0.35, 0.02, z + 0.06), ortak.malzeme("K5_Karton", (0.35, 0.24, 0.12), puruz=0.9), 0.004)
    # Üçüncü ve dördüncü raf: kavanozlar (reçel, turşu) ve dikdörtgen tenekeler.
    for zi in (2, 3):
        z = raflar[zi]
        x = -EN / 2 + 0.10
        k = 0
        while x < EN / 2 - 0.08:
            if (k + zi) % 4 != 3:
                icm = recel if (k + zi) % 2 else tursu
                h1 = 0.16 if zi == 2 else 0.20
                S(0.052, h1, (x, rnd.uniform(-0.06, 0.05), z + h1 / 2), cam, 16)
                S(0.046, h1 * 0.8, (x, p[-1].location.y, z + h1 * 0.42), icm, 14)
                S(0.054, 0.02, (x, p[-2].location.y, z + h1 + 0.008), kapak, 16)
                x += 0.13
            else:
                K((0.12, 0.10, 0.16), (x + 0.02, 0.0, z + 0.08), teneke, 0.005)
                K((0.121, 0.101, 0.07), (x + 0.02, 0.0, z + 0.08), etiket[k % 3])
                x += 0.16
            k += 1
    # En üst: katlanmış branda ve bir rulo bant.
    z = raflar[4]
    K((0.50, 0.30, 0.08), (-0.25, 0.0, z + 0.04), ortak.malzeme("K5_Branda", (0.08, 0.12, 0.06), puruz=0.9), 0.02)
    simit(0.05, 0.02, (0.25, 0.0, z + 0.02), ortak.malzeme("K5_Bant", (0.45, 0.38, 0.20), puruz=0.6), seg=20)
    ad, h, uzak = "Raf_K5", 1.8, 2.8

elif hangi == "lamba":
    kauçuk = ortak.malzeme("K5_Kaucuk", (0.02, 0.02, 0.02), puruz=0.8)
    tel = ortak.malzeme("K5_Kafes", (0.30, 0.30, 0.29), puruz=0.4, metal=0.9)
    ampul = ortak.malzeme("K5_Ampul", (0.80, 0.78, 0.70), puruz=0.1)
    kablo = ortak.malzeme("K5_Kablo", (0.45, 0.14, 0.02), puruz=0.5)
    # Lamba x ekseni boyunca kurulur (sap -x'te, kafes +x'te), sonra yere yaslanacak şekilde eğilir.
    ax = 0.055                                                         # eksenin yüksekliği (kafes yarıçapı)
    yatay_silindir(0.021, 0.15, (-0.155, 0, ax), kauçuk, 16)           # sap
    for i in range(6):
        simit(0.0225, 0.003, (-0.21 + i * 0.022, 0, ax), kauçuk, rot=(0, math.pi / 2, 0), seg=16)   # tutuş halkaları
    bpy.ops.mesh.primitive_cone_add(vertices=20, radius1=0.022, radius2=0.05, depth=0.05, location=(-0.055, 0, ax), rotation=(0, math.pi / 2, 0))
    n = bpy.context.active_object; bpy.ops.object.transform_apply(rotation=True); ortak.ata(n, kauçuk); p.append(n)
    kure(0.034, (0.02, 0, ax), ampul, olcek=(1.3, 1, 1))
    for xr in (-0.03, 0.01, 0.05, 0.085):                                # kafes halkaları
        simit(0.052, 0.0025, (xr, 0, ax), tel, rot=(0, math.pi / 2, 0), seg=20)
    for i in range(8):                                                  # boyuna teller
        a = i * math.pi / 4
        cubuk((-0.03, 0.052 * math.cos(a), ax + 0.052 * math.sin(a)), (0.085, 0.052 * math.cos(a), ax + 0.052 * math.sin(a)), 0.0022, tel, 6)
        cubuk((0.085, 0.052 * math.cos(a), ax + 0.052 * math.sin(a)), (0.105, 0, ax), 0.0022, tel, 6)
    cubuk((0.105, 0, ax), (0.13, 0, ax + 0.02), 0.003, tel, 6)          # asma kancası
    cubuk((0.13, 0, ax + 0.02), (0.14, 0, ax), 0.003, tel, 6)
    lamba = ortak.birlestir(p, "LambaGovde")
    lamba.rotation_euler = (0, -math.atan2(ax - 0.021, 0.23), 0)         # sap ucu yere değecek kadar eğ (eksi: -x ucu aşağı)
    bpy.ops.object.select_all(action="DESELECT"); lamba.select_set(True); bpy.context.view_layer.objects.active = lamba
    bpy.ops.object.transform_apply(rotation=True)
    alt = min((lamba.matrix_world @ v.co).z for v in lamba.data.vertices)
    lamba.location.z -= alt
    bpy.ops.object.transform_apply(location=True)
    p = [lamba]
    # Kablo: sapın ucundan yere iner, iki buçuk tur kıvrılır, sonra prize doğru uzar.
    uc = min(lamba.data.vertices, key=lambda v: v.co.x).co
    onceki = (uc.x, 0, 0.012)
    kx, ky = -0.40, 0.10
    noktalar = []
    for i in range(1, 41):
        a = math.pi + i * (2.5 * 2 * math.pi) / 40
        rr = 0.10 + 0.04 * i / 40
        noktalar.append((kx + rr * math.cos(a) + 0.10, ky + rr * math.sin(a), 0.006 + 0.009 * (i / 16)))
    noktalar += [(kx - 0.25, ky + 0.12, 0.006), (kx - 0.55, ky + 0.10, 0.006)]
    for nk in noktalar:
        cubuk(onceki, nk, 0.0055, kablo, 8)
        onceki = nk
    ad, h, uzak = "Is_Lambasi_K5", 0.15, 1.1

elif hangi == "boya":
    plastik = ortak.malzeme("K5_BoyaKova", (0.78, 0.78, 0.75), puruz=0.35)
    teneke = ortak.malzeme("K5_BoyaTeneke", (0.45, 0.45, 0.42), puruz=0.3, metal=0.85)
    beyaz_boya = ortak.malzeme("K5_BeyazBoya", (0.80, 0.78, 0.72), puruz=0.15)
    etiket = [ortak.malzeme("K5_BoyaEtiket%d" % i, c, puruz=0.5) for i, c in enumerate(((0.05, 0.12, 0.40), (0.50, 0.08, 0.03), (0.08, 0.25, 0.08)))]
    sap = ortak.malzeme("K5_FircaSap", (0.28, 0.14, 0.05), puruz=0.5)
    kil = ortak.malzeme("K5_FircaKil", (0.10, 0.07, 0.04), puruz=0.9)
    tel = ortak.malzeme("K5_BoyaTel", (0.35, 0.35, 0.34), puruz=0.35, metal=0.9)
    # Yığın: büyük kova, üstünde orta kova, en üstte küçük teneke.
    z = 0.0
    for i, (r_, h_) in enumerate(((0.155, 0.33), (0.13, 0.24), (0.09, 0.14))):
        mat = teneke if i == 2 else plastik
        koni(r_ - 0.012, r_, h_, (0, 0, z + h_ / 2), mat, 32)
        S(r_ + 0.006, 0.02, (0, 0, z + h_ - 0.005), mat, 32)                                  # kapak
        S(r_ + 0.001, h_ * 0.45, (0, 0, z + h_ * 0.5), etiket[i], 32)                         # etiket bandı
        K((0.012, 0.004, 0.06), (r_ * 0.5, -r_ + 0.004, z + h_ - 0.04), beyaz_boya)           # akıntı
        simit(r_ * 0.9, 0.003, (0, 0, z + h_ + 0.008), tel, olcek=(1, 1, 0.2), seg=24)        # yatırılmış tel sap
        z += h_ + 0.005
    # Yanda açık kova: ağzına kadar beyaz boya, kenarından akmış.
    kova(0.14, 0.155, 0.30, 0.36, 0.05, plastik, kal=0.006)
    S(0.149, 0.004, (0.36, 0.05, 0.25), beyaz_boya, 32)
    for a in (0.3, 1.9, 3.7):
        K((0.02, 0.006, 0.09), (0.36 + 0.157 * math.cos(a), 0.05 + 0.157 * math.sin(a), 0.255), beyaz_boya)
    for (dx, dy, rr) in ((0.50, -0.10, 0.035), (0.54, -0.16, 0.018), (0.47, -0.19, 0.012)):     # yerde damla izleri
        S(rr, 0.002, (dx, dy, 0.001), beyaz_boya, 16)
    # Fırçalar: biri yığının üstünde, biri açık kovanın ağzında.
    def firca(a, b, gen):
        u = (mathutils.Vector(b) - mathutils.Vector(a))
        koni_eksen(a, tuple(mathutils.Vector(a) + u * 0.55), 0.010, 0.013, sap, 10)
        cubuk(tuple(mathutils.Vector(a) + u * 0.55), tuple(mathutils.Vector(a) + u * 0.72), 0.015, teneke, 12)
        cubuk(tuple(mathutils.Vector(a) + u * 0.72), b, 0.016, kil, 12)
    firca((-0.09, -0.02, 0.73), (0.08, 0.03, 0.735), 0.05)
    firca((0.24, -0.12, 0.33), (0.40, 0.08, 0.275), 0.05)
    ad, h, uzak = "Boya_Kovalari_K5", 0.75, 1.5

# =====================================================================
# ODA A (yatak odası)
# =====================================================================
elif hangi == "komodin":
    kumas = ortak.malzeme("K5_Abajur", (0.55, 0.45, 0.30), puruz=0.9)
    seramik = ortak.malzeme("K5_LambaAyak", (0.35, 0.22, 0.12), puruz=0.25)
    EN, DER, H = 0.45, 0.38, 0.55
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
        for y in (-DER / 2 + 0.03, DER / 2 - 0.03):
            koni(0.012, 0.018, 0.07, (x, y, 0.035), koyu_ahsap, 12)
    K((EN, DER, H - 0.095), (0, 0, 0.07 + (H - 0.095) / 2), koyu_ahsap, 0.004)
    K((EN + 0.02, DER + 0.02, 0.025), (0, -0.005, H - 0.0125), koyu_ahsap, 0.008)
    y_on = -DER / 2
    K((EN - 0.03, 0.015, 0.12), (0, y_on - 0.0075, H - 0.1), koyu_ahsap, 0.004)              # çekmece önü (süs)
    K((EN - 0.03, 0.015, 0.28), (0, y_on - 0.0075, 0.23), koyu_ahsap, 0.004)                 # kapak (süs)
    K((EN - 0.11, 0.008, 0.20), (0, y_on - 0.018, 0.23), ceviz, 0.006)                        # kapakta tabla
    kure(0.013, (0, y_on - 0.025, H - 0.1), pirinc)
    kure(0.013, (EN / 2 - 0.06, y_on - 0.025, 0.28), pirinc)
    # Gece lambası: seramik gövde, pirinç boyun, kumaş abajur (içi boş, altı açık).
    lx, ly = 0.08, 0.04
    S(0.05, 0.02, (lx, ly, H + 0.01), seramik, 20)
    kure(0.055, (lx, ly, H + 0.08), seramik, olcek=(1, 1, 1.1), seg=18)
    cubuk((lx, ly, H + 0.14), (lx, ly, H + 0.22), 0.007, pirinc)
    abajur = koni(0.11, 0.065, 0.15, (lx, ly, H + 0.26), kumas, 24, ekle=False)
    ortak.oy(abajur, koni(0.104, 0.059, 0.16, (lx, ly, H + 0.26), kumas, 24, ekle=False))
    p.append(abajur)
    kure(0.025, (lx, ly, H + 0.23), ortak.malzeme("K5_GeceAmpul", (0.80, 0.75, 0.60), puruz=0.1))
    K((0.12, 0.17, 0.02), (-0.10, -0.03, H + 0.01), ortak.malzeme("K5_Kitap", (0.20, 0.05, 0.04), puruz=0.7), 0.003)   # başucu kitabı
    ad, h, uzak = "Komodin_K5", 0.9, 1.3

elif hangi == "sifonyer":
    ayna = ortak.malzeme("K5_SifAyna", (0.60, 0.62, 0.63), puruz=0.03, metal=1.0)
    kadife = ortak.malzeme("K5_MucevherKutu", (0.25, 0.04, 0.05), puruz=0.8)
    EN, DER, H = 1.00, 0.45, 0.85
    for x in (-EN / 2 + 0.05, EN / 2 - 0.05):                                               # soğan ayaklar
        for y in (-DER / 2 + 0.05, DER / 2 - 0.05):
            kure(0.04, (x, y, 0.04), koyu_ahsap, olcek=(1, 1, 1.0))
    K((EN, DER, H - 0.11), (0, 0, 0.08 + (H - 0.11) / 2), koyu_ahsap, 0.006)
    K((EN + 0.03, DER + 0.03, 0.03), (0, -0.005, H - 0.015), koyu_ahsap, 0.01)
    y_on = -DER / 2
    # Çekmeceler (süs): üstte iki küçük, altta üç büyük; her birinde iki pirinç damla kulp.
    sira = [(0.62, 0.66, 0.14, 2), (0.45, 0.5, 0.16, 1), (0.28, 0.33, 0.16, 1), (0.11, 0.16, 0.16, 1)]
    for zc, _, hh, adet in sira:
        w = (EN - 0.04) / adet
        for j in range(adet):
            xc = -EN / 2 + 0.02 + w * (j + 0.5)
            K((w - 0.01, 0.018, hh - 0.012), (xc, y_on - 0.009, zc + 0.03), koyu_ahsap, 0.004)
            for kx in ((-w * 0.25, w * 0.25) if adet == 1 else (0,)):
                K((0.05, 0.012, 0.012), (xc + kx, y_on - 0.03, zc + 0.04), pirinc, 0.004)
                K((0.012, 0.018, 0.02), (xc + kx, y_on - 0.022, zc + 0.05), pirinc)
    # Yuvarlak ayna: arkada iki dikmeye bağlı, üstte oymalı taç.
    for s in (-1, 1):
        K((0.04, 0.03, 0.55), (s * 0.30, DER / 2 - 0.03, H + 0.275), koyu_ahsap, 0.005)
    dik_disk(0.34, 0.03, (0, DER / 2 - 0.03, H + 0.42), koyu_ahsap, 48)
    dik_disk(0.30, 0.006, (0, DER / 2 - 0.047, H + 0.42), ayna, 48)
    K((0.22, 0.03, 0.06), (0, DER / 2 - 0.03, H + 0.77), koyu_ahsap, 0.012)
    # Üstünde dantel, mücevher kutusu ve saç fırçası.
    K((0.60, 0.30, 0.003), (0, -0.04, H + 0.0015), dantel_m)
    K((0.16, 0.11, 0.07), (-0.28, -0.05, H + 0.038), kadife, 0.01)
    K((0.05, 0.02, 0.008), (-0.28, -0.106, H + 0.05), pirinc)
    KR((0.20, 0.05, 0.03), (0.25, -0.08, H + 0.018), ceviz_ac, (0, 0, 0.3), 0.012)
    ad, h, uzak = "Sifonyer_K5", 1.6, 2.2

elif hangi == "perdeA":
    duvar = True
    bej = ortak.malzeme("K5_PerdeBej", (0.50, 0.42, 0.28), puruz=0.9)
    bej_k = ortak.malzeme("K5_FarbalaBej", (0.40, 0.32, 0.20), puruz=0.9)
    metal = ortak.malzeme("K5_Cubuk", (0.40, 0.38, 0.34), puruz=0.35, metal=0.8)
    EN, BOY, ARA = 1.60, 1.75, 0.10
    yatay_silindir(0.010, EN, (0, -ARA, 0), metal, 12)
    for s in (-1, 1):
        yatay_silindir(0.016, 0.03, (s * (EN / 2 + 0.015), -ARA, 0), metal, 12)             # uç tıpaları
        K((0.02, ARA, 0.02), (s * (EN / 2 - 0.10), -ARA / 2, 0), metal, 0.003)
    # İki kanat: pencereyi yarı yarıya kapatacak kadar içe gelmiş, düz iniyor.
    for s in (-1, 1):
        x_dis, x_ic = s * (EN / 2 - 0.02), s * 0.12
        pilili("PerdeA", min(x_dis, x_ic), max(x_dis, x_ic), -0.03, BOY, -ARA - 0.015, 0.02, 0.05, bej, 1.0, 0.004)
    # Farbala: üstte boydan boya kısa, sık pilili saçak; kornişi örter.
    pilili("Farbala", -EN / 2 - 0.02, EN / 2 + 0.02, 0.04, 0.26, -ARA - 0.05, 0.012, 0.03, bej_k, 1.0, 0.004)
    K((EN + 0.06, 0.008, 0.025), (0, -ARA - 0.065, -0.21), bej)                               # farbalanın alt şeridi
    ad, h, uzak = "Perde_OdaA_K5", 1.85, 2.6
    kaldir = 1.80

# =====================================================================
# ODA B (çalışma odası)
# =====================================================================
elif hangi == "calisma":
    daktilo = ortak.malzeme("K5_Daktilo", (0.05, 0.08, 0.07), puruz=0.35)
    tus = ortak.malzeme("K5_Tus", (0.70, 0.68, 0.60), puruz=0.4)
    kagit = ortak.malzeme("K5_Kagit", (0.80, 0.78, 0.70), puruz=0.9)
    lamba_m = ortak.malzeme("K5_MasaLambasi", (0.05, 0.16, 0.08), puruz=0.3, metal=0.5)
    cam = ortak.malzeme("K5_KulTablasi", (0.35, 0.40, 0.38), puruz=0.05)
    EN, DER, H = 1.20, 0.60, 0.75
    K((EN, DER, 0.03), (0, 0, H - 0.015), ceviz, 0.005)                                       # tabla
    K((0.40, DER - 0.02, H - 0.03), (-EN / 2 + 0.20, 0.0, (H - 0.03) / 2), ceviz, 0.004)     # sol çekmece gövdesi
    for i in range(3):                                                                        # çekmece önleri (süs)
        zc = 0.08 + i * 0.22
        K((0.37, 0.016, 0.20), (-EN / 2 + 0.20, -DER / 2 - 0.004, zc + 0.10), ceviz_ac, 0.004)
        K((0.10, 0.015, 0.012), (-EN / 2 + 0.20, -DER / 2 - 0.018, zc + 0.14), pirinc, 0.004)
    K((0.025, DER - 0.02, H - 0.03), (EN / 2 - 0.0125, 0, (H - 0.03) / 2), ceviz, 0.004)     # sağ yan tabla
    K((EN - 0.42, 0.018, 0.40), (0.20, DER / 2 - 0.03, H - 0.23), ceviz, 0.003)              # arka etek
    K((EN - 0.42, 0.02, 0.06), (0.20, -DER / 2 + 0.03, H - 0.06), ceviz, 0.003)              # ön etek
    z = H
    # Daktilo: gövde, basamaklı tuş sıraları, silindir (merdane) ve arkasından çıkan kâğıt.
    dx, dy = -0.05, -0.02
    K((0.36, 0.30, 0.06), (dx, dy, z + 0.03), daktilo, 0.01)
    KR((0.34, 0.14, 0.05), (dx, dy - 0.07, z + 0.07), daktilo, (math.radians(18), 0, 0), 0.008)
    K((0.30, 0.12, 0.08), (dx, dy + 0.08, z + 0.10), daktilo, 0.012)
    for sira in range(4):
        for i in range(10 - (sira % 2)):
            S(0.009, 0.012, (dx - 0.12 + i * 0.026 + (sira % 2) * 0.013, dy - 0.13 + sira * 0.025, z + 0.075 + sira * 0.008), tus, 10)
    K((0.14, 0.02, 0.01), (dx, dy - 0.155, z + 0.066), tus, 0.003)                           # boşluk tuşu
    yatay_silindir(0.024, 0.42, (dx, dy + 0.10, z + 0.165), siyah, 18)                       # merdane
    for s in (-1, 1):
        dik_disk(0.018, 0.02, (dx + s * 0.22, dy + 0.10, z + 0.165), siyah, 14)
    cubuk((dx - 0.22, dy + 0.08, z + 0.18), (dx - 0.30, dy - 0.01, z + 0.20), 0.004, krom)   # satır başı kolu
    KR((0.21, 0.002, 0.22), (dx, dy + 0.13, z + 0.27), kagit, (math.radians(-12), 0, 0))     # takılı kâğıt
    # Masa lambası: sağ arkada, yuvarlak ayak, iki kollu, yeşil metal şapka.
    lx, ly = 0.42, 0.18
    S(0.07, 0.025, (lx, ly, z + 0.0125), lamba_m, 24, 0.005)
    cubuk((lx, ly, z + 0.02), (lx - 0.03, ly - 0.02, z + 0.30), 0.008, krom)
    cubuk((lx - 0.03, ly - 0.02, z + 0.30), (lx - 0.14, ly - 0.10, z + 0.36), 0.008, krom)
    sapka = koni(0.075, 0.03, 0.10, (lx - 0.16, ly - 0.12, z + 0.32), lamba_m, 20, ekle=False)
    sapka.rotation_euler = (math.radians(20), math.radians(-15), 0)
    bpy.ops.object.select_all(action="DESELECT"); sapka.select_set(True); bpy.context.view_layer.objects.active = sapka
    bpy.ops.object.transform_apply(rotation=True)
    p.append(sapka)
    # Kâğıtlar: sağda deste, önünde dağınık iki yaprak; kalem ve cam kül tablası.
    K((0.21, 0.297, 0.03), (0.28, -0.08, z + 0.015), kagit, 0.002)
    KR((0.21, 0.297, 0.001), (0.18, -0.17, z + 0.0006), kagit, (0, 0, 0.35))
    KR((0.21, 0.297, 0.001), (-0.35, -0.12, z + 0.0008), kagit, (0, 0, -0.2))
    cubuk((-0.30, -0.18, z + 0.004), (-0.17, -0.14, z + 0.004), 0.004, ortak.malzeme("K5_Kalem", (0.05, 0.08, 0.30), puruz=0.3))
    kul = ortak.silindir("Kul", 0.06, 0.03, (0.45, -0.18, z + 0.015), cam, 8, 0.004)
    ortak.oy(kul, ortak.silindir("KulIc", 0.045, 0.03, (0.45, -0.18, z + 0.025), cam, 16))
    p.append(kul)
    for a in (0.2, 1.6):
        cubuk((0.45 + 0.01 * math.cos(a), -0.18 + 0.01 * math.sin(a), z + 0.012),
              (0.45 + 0.07 * math.cos(a), -0.18 + 0.07 * math.sin(a), z + 0.03), 0.0045, ortak.malzeme("K5_Izmarit", (0.60, 0.50, 0.30), puruz=0.8), 8)
    ad, h, uzak = "Calisma_Masasi_K5", 1.0, 1.9

elif hangi == "kitaplik":
    renkler = [ortak.malzeme("K5_Kitap%d" % i, c, puruz=0.7) for i, c in enumerate((
        (0.25, 0.04, 0.03), (0.04, 0.08, 0.18), (0.05, 0.12, 0.05), (0.40, 0.30, 0.15), (0.10, 0.06, 0.03),
        (0.55, 0.50, 0.40), (0.30, 0.12, 0.03), (0.08, 0.08, 0.08), (0.45, 0.35, 0.05)))]
    EN, DER, H = 0.90, 0.32, 1.90
    raflar = (0.10, 0.46, 0.82, 1.18, 1.54)
    p.extend(ac.govde_levhalari(EN, DER, H, ceviz, taban=0.08, raflar=raflar[1:]))
    K((EN - 0.02, 0.02, 0.08), (0, -DER / 2 + 0.02, 0.04), ceviz, 0.004)                      # süpürgelik
    K((EN + 0.04, DER + 0.03, 0.035), (0, -0.005, H + 0.0175), ceviz, 0.01)                   # taç
    tabanlar = (0.08 + 0.018,) + raflar[1:]
    for ri, z in enumerate(tabanlar):
        tavan = (raflar[ri + 1] - 0.018) if ri + 1 < len(raflar) else H - 0.018
        x = -EN / 2 + 0.02
        bitis = EN / 2 - 0.02
        while x < bitis - 0.03:
            r = rnd.random()
            if r < 0.08 and x < bitis - 0.25:                          # yatay yığın
                for k in range(rnd.randint(2, 4)):
                    hk = rnd.uniform(0.025, 0.04)
                    K((0.20, 0.15, hk), (x + 0.10, -0.02, z + 0.02 + k * 0.04), renkler[rnd.randint(0, 8)], 0.003)
                x += 0.22
            elif r < 0.13:                                             # boşluk
                x += rnd.uniform(0.03, 0.07)
            else:
                w = rnd.uniform(0.02, 0.05)
                hk = min(tavan - z - 0.02, rnd.uniform(0.20, 0.30))
                d = rnd.uniform(0.16, 0.22)
                K((w, d, hk), (x + w / 2, DER / 2 - 0.01 - d / 2, z + hk / 2), renkler[rnd.randint(0, 8)], 0.002)
                if rnd.random() < 0.3:
                    K((w + 0.001, 0.003, 0.012), (x + w / 2, DER / 2 - 0.01 - d - 0.001, z + hk * 0.8), pirinc)   # sırt yaldızı
                x += w + 0.002
    ad, h, uzak = "Kitaplik_K5", 1.9, 2.6

elif hangi == "sandalye":
    deri = ortak.malzeme("K5_SuniDeri", (0.09, 0.04, 0.018), puruz=0.45)
    ahsap = ceviz_ac
    S_, H = 0.45, 0.46
    for sx in (-1, 1):
        K((0.035, 0.035, H - 0.03), (sx * (S_ / 2 - 0.025), -S_ / 2 + 0.03, (H - 0.03) / 2), ahsap, 0.005)   # ön ayaklar
        KR((0.035, 0.035, 0.90), (sx * (S_ / 2 - 0.025), S_ / 2 - 0.035, 0.45), ahsap, (math.radians(-4), 0, 0), 0.005)  # arka ayak/dikme
    K((S_, S_ - 0.02, 0.03), (0, 0, H - 0.015), ahsap, 0.005)                                 # oturak tablası
    K((S_ - 0.04, S_ - 0.07, 0.04), (0, -0.015, H + 0.02), deri, 0.015)                        # suni deri minder
    for (a, b) in (((-1, -1), (1, -1)), ((-1, 1), (1, 1)), ((-1, -1), (-1, 1)), ((1, -1), (1, 1))):
        cubuk((a[0] * (S_ / 2 - 0.025), a[1] * (S_ / 2 - 0.035), 0.16), (b[0] * (S_ / 2 - 0.025), b[1] * (S_ / 2 - 0.035), 0.16), 0.01, ahsap)
    # Arkalık: kavisli üst ray (üç parça) ve üç dik tornalı çubuk. Arka dikmeler 4° geriye yatık:
    # z yüksekliğindeki dikme ekseni y = yl(z).
    def yl(zz):
        return S_ / 2 - 0.035 + (zz - 0.45) * math.tan(math.radians(4))
    for i, dxr in enumerate((-0.13, 0.0, 0.13)):
        KR((0.14, 0.025, 0.08), (dxr, yl(0.84) + (0.012 if i != 1 else 0.0), 0.84), ahsap, (math.radians(-4), 0, -dxr * 0.8), 0.006)
    K((S_ - 0.06, 0.022, 0.035), (0, yl(0.54), 0.54), ahsap, 0.004)                           # alt ray
    for dxr in (-0.09, 0.0, 0.09):
        cubuk((dxr, yl(0.54), 0.55), (dxr, yl(0.81), 0.81), 0.011, ahsap, 10)
    ad, h, uzak = "Sandalye_K5", 0.9, 1.3

else:   # perdeB
    duvar = True
    yesil = ortak.malzeme("K5_PerdeYesil", (0.03, 0.10, 0.045), puruz=0.85)
    plastik = ortak.malzeme("K5_RayKornis", (0.72, 0.70, 0.66), puruz=0.4)
    EN, BOY, ARA = 1.55, 1.55, 0.07
    # Beyaz plastik ray korniş: tavana/duvara vidalı kutu profil, iki uçta kapak.
    K((EN, 0.035, 0.022), (0, -ARA, 0.005), plastik, 0.003)
    for s in (-1, 1):
        K((0.012, 0.04, 0.03), (s * (EN / 2 + 0.006), -ARA, 0.005), plastik, 0.003)
        K((0.03, ARA - 0.015, 0.015), (s * (EN / 2 - 0.15), -(ARA - 0.015) / 2, 0.01), plastik, 0.002)
    # Kanatlar: soldaki geniş (yarı kapalı), sağdaki toplanmış; raydan küçük makaralarla sarkıyor.
    for (x0, x1, genlik, adim) in ((-EN / 2 + 0.01, -0.05, 0.022, 0.05), (EN / 2 - 0.30, EN / 2 - 0.01, 0.03, 0.035)):
        pilili("PerdeB", x0, x1, -0.015, BOY, -ARA, genlik, adim, yesil, 1.0, 0.004)
        n = int((x1 - x0) / (adim * 2)) + 1
        for i in range(n):
            K((0.008, 0.012, 0.012), (x0 + i * (x1 - x0) / max(1, n - 1), -ARA, -0.012), plastik)
    ad, h, uzak = "Perde_OdaB_K5", 1.6, 2.4
    kaldir = 1.58

# ---------- BİTİR: tek parça, pivot (0,0,0), kaydet, FBX, önizleme ----------
nesne = ortak.birlestir(p, ad)
ortak.yumusat(nesne, 35)
ortak.pivotu_tasi(nesne, (0, 0, 0))
ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=(kaldir if duvar else 0.0))
