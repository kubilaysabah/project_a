# kat8_esyalar.py
# 8. kat, 32 numaralı daire: apartman Yönetimi'nin bürosu (1994) (#83). 90'ların Türk resmî dairesi:
# koyu maun, gri boyalı sac dolaplar, renkli sırtlı klasörler, daktilo. Katlarda aynı model yok: hepsi yeni.
#   blender -b --factory-startup --python kat8_esyalar.py -- <ad> [--onizleme <png>]
#
# ANTRE (bekleme yeri)
#   bekleme          Bekleme_Sandalyeleri_K8 : kirişle birbirine bağlı üçlü metal bekleme sandalyesi, vinil oturak.
#   pano             Ilan_Panosu_K8          : mantar ilan panosu; raptiyeli aidat listesi, su kesintisi duyurusu.
# HOL
#   anahtar          Anahtar_Dolabi_K8       : AÇILIR (Govde + Kapi_1) tek kapaklı duvar anahtar dolabı; etiketli
#                                              kancalar, altta küçük raf (rafa bir anahtar konur).
#   kutular          Arsiv_Kutulari_K8       : duvar boyunca üst üste karton arşiv kutuları, biri açık.
# SALON (müdür odası)
#   masa             Mudur_Masasi_K8         : koyu maun makam masası; daktilo, çevirmeli telefon, mühür takımı,
#                                              evrak yığını, yeşil camlı masa lambası, isimlik (tek parça).
#   makam            Makam_Koltugu_K8        : yüksek, kapitone arkalıklı deri makam koltuğu, beş tekerlekli.
#   misafir          Misafir_Koltugu_K8      : maun kolçaklı, deri döşemeli misafir koltuğu.
#   dosyarafi        Dosya_Rafi_K8           : açık maun raf; renkli sırtlı klasörler, sırtlarda yıl etiketleri.
#   kasa             Kasa_K8                 : eski yeşil demir kasa; çevirmeli kilit, kol, pirinç plaka (açılmaz).
#   jaluzi           Jaluzi_K8               : 1,4 m pencereye yatay ofis jaluzisi, yarıya kadar indirilmiş.
#   saat             Duvar_Saati_K8          : yuvarlak sade ofis duvar saati; tam dokuzda durmuş.
# MUTFAK (çay ocağı)
#   cayocagi         Cay_Ocagi_K8            : küçük dolap üstünde elektrikli semaver + demlik, tepside ince belli bardaklar.
#   buzdolabi        Buzdolabi_K8            : küçük büro buzdolabı (açılmaz), üstünde sürahi ve şeker kutusu.
#   sandalye         Mutfak_Sandalyesi_K8    : krom boru ayaklı, turuncu plastik oturaklı sandalye.
# BANYO
#   araba            Temizlik_Arabasi_K8     : hademe arabası: kova, saçaklı paspas, deterjan şişeleri, bezler.
# KİLER (depo)
#   arsivrafi        Arsiv_Rafi_K8           : delikli köşebentli gri-yeşil metal raf; eski dosya kutuları, iple
#                                              bağlı evrak desteleri.
#   daktilolar       Eski_Daktilolar_K8      : yerde bozuk iki daktilo (biri yan yatmış) ve bir koli sürekli form.
# ODA A (arşiv)
#   arsivdolabi      Arsiv_Dolabi_K8         : üstü camlı kapaklı uzun maun arşiv dolabı, içinde klasörler (açılmaz).
#   haritamasa       Harita_Masasi_K8        : eğimli çizim masası; üstünde mavi kopya kat planı, altında rulo planlar.
#   plan             Plan_Cercevesi_K8       : çerçeveli bina cephe çizimi (zemin + KAT_SAYISI kat, pencereler).
# ODA B (toplantı odası)
#   toplantimasa     Toplanti_Masasi_K8      : koyu maun toplantı masası; dosyalar, izmaritli cam kül tablası.
#   toplantisandalye Toplanti_Sandalyesi_K8  : bordo kumaş döşemeli maun sandalye.
#   evrak            Evrak_Dolabi_K8         : AÇILIR (Govde + Cekmece_1 + Cekmece_2) iki çekmeceli alçak sac dolap.
#   tepegoz          Tepegoz_K8              : tekerlekli sehpa üstünde 90'lar tepegözü.
#   tahta            Kara_Tahta_K8           : duvar tahtası; tebeşir izleri, çetele, kat şeması ve iri bir "9".
#
# Ön yüz -y; Unity'de +Z. Pivot: yer eşyalarında tabanın ortası, duvar eşyalarında arka yüzün ortası,
# jaluzide üst rayın arka üst kenarının ortası. Renkler doğrusal (linear): Unity'de daha açık görünür.
# CAM: "K8_Cam" malzemesi yarı saydamdır (alfa 0,25); Unity'de saydam malzemeye eşlenmesi gerekir.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import mathutils

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "saat"
ortak.sahneyi_temizle()
rnd = random.Random(8)     # sabit tohum: betik her çalıştığında aynı dağınıklık
p = []                      # birleştirilecek parçalar
parcalar = []               # açılır eşyalarda Govde dışındaki ayrı nesneler (kapı, çekmece)
acik = False                # açılır eşya mı


# ---------- KÜÇÜK YARDIMCILAR ----------
# Hepsi yaptığı parçayı p listesine ekler; eşyanın sonunda p tek nesnede birleşir.

def sec(n):
    """Yalnızca n'yi seçip etkin yap: Blender komutları seçili nesnelere uygulanır."""
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n


def K(ad, olcu, konum, mat, pah=0.0, rot=None):
    """
    Kutu (ortak.kutu) + istenirse kendi merkezi çevresinde dönüş, doğrudan meshe işlenir.
    DİKKAT: ortak.kutu transform_apply'ı varsayılanlarla çağırır, yani KONUMU da meshe işler ve pivot
    dünya merkezinde kalır. rotation_euler ile döndürmek kutuyu dünya merkezi çevresinde savururdu
    (ilk denemede jaluzinin lamelleri 30 cm geriye kaydı). Bu yüzden dönüşü kutunun merkezine taşınmış
    bir matrisle meshin kendisine uyguluyoruz: merkeze götür, döndür, geri getir.
    """
    n = ortak.kutu(ad, olcu, konum, mat, pah)
    if rot:
        merkez = mathutils.Vector(konum)
        M = (mathutils.Matrix.Translation(merkez) @ mathutils.Euler(rot).to_matrix().to_4x4()
             @ mathutils.Matrix.Translation(-merkez))
        n.data.transform(M)
    p.append(n)
    return n


def S(ad, r, boy, konum, mat, kose=24, pah=0.0):
    """Dik (z eksenli) silindir."""
    n = ortak.silindir(ad, r, boy, konum, mat, kose, pah)
    p.append(n)
    return n


def dondurulmus_silindir(r, boy, konum, rot, mat, kose):
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def dik_disk(r, kal, konum, mat, kose=24):
    """Ön yüzü -y'ye bakan disk (düğme, kadran, saat)."""
    return dondurulmus_silindir(r, kal, konum, (math.pi / 2, 0, 0), mat, kose)


def x_silindir(r, boy, konum, mat, kose=16):
    """x ekseni boyunca yatan silindir (merdane, rulo, çubuk)."""
    return dondurulmus_silindir(r, boy, konum, (0, math.pi / 2, 0), mat, kose)


def koni(r_alt, r_ust, boy, konum, mat, kose=20):
    """Kesik koni: alttaki yarıçap r_alt, üstteki r_ust (bardak, kova, abajur)."""
    bpy.ops.mesh.primitive_cone_add(vertices=kose, radius1=r_alt, radius2=r_ust, depth=boy, location=konum)
    n = bpy.context.active_object
    ortak.ata(n, mat)
    p.append(n)
    return n


def cubuk(a, b, r, mat, kose=8):
    """a noktasından b noktasına silindir çubuk (boru, sap, kablo)."""
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


def kure(r, konum, mat, olcek=(1, 1, 1), seg=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=konum, segments=seg, ring_count=8)
    n = bpy.context.active_object
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def halka(R, r, konum, mat, rot=(0, 0, 0), seg=(24, 8)):
    """Simit (torus): kulp, kenar halkası, anahtar halkası."""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, location=konum, rotation=rot,
                                     major_segments=seg[0], minor_segments=seg[1])
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    p.append(n)
    return n


def cizgi_xz(a, b, y, w, mat, kal=0.0006):
    """Duvar düzleminde (x-z) a'dan b'ye ince çizgi: tebeşir, mürekkep, çizim çizgisi."""
    dx, dz = b[0] - a[0], b[1] - a[1]
    L = math.hypot(dx, dz)
    return K("Cizgi", (L + w * 0.6, kal, w), ((a[0] + b[0]) / 2, y, (a[1] + b[1]) / 2), mat,
             rot=(0, math.atan2(-dz, dx), 0))


def cizgi_xy(a, b, z, w, mat, kal=0.0006):
    """Yatay düzlemde (x-y) a'dan b'ye ince çizgi: masadaki planın çizgileri."""
    dx, dy = b[0] - a[0], b[1] - a[1]
    L = math.hypot(dx, dy)
    return K("Cizgi", (L + w * 0.6, w, kal), ((a[0] + b[0]) / 2, (a[1] + b[1]) / 2, z), mat,
             rot=(0, 0, math.atan2(dy, dx)))


def grup(bas, pivot=(0, 0, 0), rot=(0, 0, 0), kaydir=(0, 0, 0)):
    """
    p[bas:] parçalarını tek nesne yapar, pivot noktası çevresinde döndürür ve kaydırır.
    Bir alt eşyayı (daktilo, telefon, kutu) merkezde kurup masaya/rafa eğik koymanın yolu.
    """
    liste = p[bas:]
    del p[bas:]
    n = liste[0] if len(liste) == 1 else ortak.birlestir(liste, "Grup")
    ortak.pivotu_tasi(n, pivot)
    n.rotation_euler = rot
    n.location = (pivot[0] + kaydir[0], pivot[1] + kaydir[1], pivot[2] + kaydir[2])
    sec(n)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    p.append(n)
    return n


def yere_oturt(n):
    """Nesnenin en alt noktasını z = 0'a indirir (yan yatırılmış daktilo gibi)."""
    bpy.context.view_layer.update()
    mz = min((n.matrix_world @ mathutils.Vector(c)).z for c in n.bound_box)
    n.location.z -= mz
    sec(n)
    bpy.ops.object.transform_apply(location=True)


def cam_malzeme(ad, renk, alfa):
    """Yarı saydam cam. Unity FBX'ten saydamlığı güvenle okumaz: bu adla saydam malzemeye eşlenmeli."""
    m = ortak.malzeme(ad, renk, 0.05, 0.0)
    m.node_tree.nodes.get("Principled BSDF").inputs["Alpha"].default_value = alfa
    if hasattr(m, "surface_render_method"):
        m.surface_render_method = "BLENDED"     # önizlemede arkası görünsün
    return m


# ---------- MALZEMELER (hepsi K8_ önekli: başka katın malzemesiyle karışmasın) ----------
maun = ortak.malzeme("K8_Maun", (0.050, 0.019, 0.010), puruz=0.40)
maun_ac = ortak.malzeme("K8_MaunAcik", (0.100, 0.045, 0.020), puruz=0.45)
maun_koyu = ortak.malzeme("K8_MaunKoyu", (0.022, 0.010, 0.006), puruz=0.5)
pirinc = ortak.malzeme("K8_Pirinc", (0.55, 0.40, 0.13), puruz=0.30, metal=0.9)
krom = ortak.malzeme("K8_Krom", (0.60, 0.60, 0.58), puruz=0.20, metal=1.0)
siyah_metal = ortak.malzeme("K8_SiyahMetal", (0.025, 0.025, 0.025), puruz=0.45, metal=0.5)
ofis_gri = ortak.malzeme("K8_OfisGrisi", (0.26, 0.27, 0.24), puruz=0.50, metal=0.35)   # boyalı sac
kagit = ortak.malzeme("K8_Kagit", (0.78, 0.76, 0.68), puruz=0.90)
kagit_eski = ortak.malzeme("K8_KagitEski", (0.55, 0.48, 0.33), puruz=0.95)             # sararmış
etiket_m = ortak.malzeme("K8_Etiket", (0.70, 0.66, 0.55), puruz=0.85)
murekkep = ortak.malzeme("K8_Murekkep", (0.030, 0.030, 0.045), puruz=0.80)
delik = ortak.malzeme("K8_Delik", (0.012, 0.010, 0.008), puruz=0.95)                   # oyuk, delik gölgesi
karton = ortak.malzeme("K8_Karton", (0.32, 0.20, 0.09), puruz=0.90)
karton2 = ortak.malzeme("K8_Karton2", (0.25, 0.18, 0.11), puruz=0.90)
karton3 = ortak.malzeme("K8_Karton3", (0.40, 0.30, 0.15), puruz=0.90)
deri = ortak.malzeme("K8_Deri", (0.040, 0.018, 0.010), puruz=0.45)
lastik = ortak.malzeme("K8_Lastik", (0.015, 0.015, 0.015), puruz=0.70)
porselen = ortak.malzeme("K8_Porselen", (0.80, 0.78, 0.72), puruz=0.20)
cay = ortak.malzeme("K8_Cay", (0.15, 0.03, 0.008), puruz=0.08)
cam = cam_malzeme("K8_Cam", (0.60, 0.64, 0.62), 0.25)
ip = ortak.malzeme("K8_Ip", (0.45, 0.38, 0.25), puruz=0.95)
klasor_renkleri = [ortak.malzeme("K8_Klasor_" + a, c, puruz=0.55) for a, c in (
    ("Mavi", (0.02, 0.05, 0.18)), ("Kirmizi", (0.25, 0.03, 0.02)), ("Yesil", (0.03, 0.10, 0.04)),
    ("Siyah", (0.02, 0.02, 0.02)), ("Gri", (0.15, 0.15, 0.14)), ("Hardal", (0.35, 0.25, 0.04)),
    ("Bordo", (0.12, 0.02, 0.03)))]


# ---------- ALT EŞYALAR (birden çok eşyada kullanılır; hepsi merkezde, tabanı z = 0, önü -y) ----------

def arsiv_kutusu(en, der, boy, kmat, kapakli=True):
    """Karton arşiv kutusu: gövde, kapak, tutamak deliği, etiket. Kapaksızsa içi oyuk ve evrak dolu."""
    gb = boy - (0.03 if kapakli else 0.0)
    if kapakli:
        K("Kutu", (en, der, gb), (0, 0, gb / 2), kmat, 0.004)
        K("KKapak", (en + 0.008, der + 0.008, 0.035), (0, 0, boy - 0.0175), kmat, 0.004)
    else:
        g = K("Kutu", (en, der, gb), (0, 0, gb / 2), kmat)
        ortak.oy(g, ortak.kutu("Oyuk", (en - 0.01, der - 0.01, gb), (0, 0, gb / 2 + 0.006), kmat))
        K("Evrak", (en - 0.02, der - 0.02, gb - 0.07), (0, 0, 0.006 + (gb - 0.07) / 2), kagit_eski)
        for i in range(4):   # desteden taşan birkaç dik kâğıt
            K("Kagit", (en - 0.04, 0.002, 0.10), (rnd.uniform(-0.01, 0.01), -der / 2 + 0.03 + i * 0.05, gb - 0.03),
              kagit if i % 2 else kagit_eski, rot=(rnd.uniform(-0.2, 0.2), rnd.uniform(-0.1, 0.1), 0))
    K("Tutamak", (0.08, 0.002, 0.022), (0, -der / 2 - 0.001, boy * 0.62), delik)
    K("Etiket", (en * 0.55, 0.0012, 0.07), (0, -der / 2 - 0.0006, boy * 0.32), etiket_m)
    for i in range(2):
        K("Yazi", (en * 0.35 - i * 0.04, 0.0008, 0.005), (0, -der / 2 - 0.0016, boy * 0.34 + 0.01 - i * 0.02), murekkep)


def paket(en, der, boy):
    """İple çapraz bağlanmış sararmış evrak destesi."""
    K("Paket", (en, der, boy), (0, 0, boy / 2), kagit_eski, 0.003)
    K("Ip", (en + 0.004, 0.006, boy + 0.004), (0, 0, boy / 2), ip)
    K("Ip", (0.006, der + 0.004, boy + 0.004), (0, 0, boy / 2), ip)


def klasor(x, z, w, renk, y_on, der=0.28, boy=0.315):
    """Dik duran klasör: sol kenarı x, tabanı z, sırtı y_on düzleminde (-y'ye bakar). Mutlak konumda kurulur."""
    xc = x + w / 2
    K("Klasor", (w, der, boy), (xc, y_on + der / 2, z + boy / 2), renk, 0.004)
    K("KEtiket", (w * 0.62, 0.002, 0.075), (xc, y_on - 0.001, z + boy * 0.70), etiket_m)
    K("KYazi", (w * 0.35, 0.001, 0.008), (xc, y_on - 0.0022, z + boy * 0.73), murekkep)
    K("KYil", (w * 0.45, 0.002, 0.018), (xc, y_on - 0.001, z + boy * 0.40), kagit)    # yıl etiketi
    if w > 0.06:
        dik_disk(0.011, 0.003, (xc, y_on - 0.0005, z + boy * 0.18), delik, kose=14)     # parmak deliği


def daktilo(renk, eksik_tus=0.0, merdane=True):
    """90'lar mekanik daktilosu, önü (klavye) -y. eksik_tus: kopmuş tuş oranı. merdane=False: silindiri sökük."""
    tus = ortak.malzeme("K8_DaktiloTus", (0.55, 0.53, 0.45), puruz=0.4)
    K("DArka", (0.34, 0.16, 0.10), (0, 0.07, 0.05), renk, 0.012)          # mekanizma gövdesi
    K("DOn", (0.34, 0.11, 0.035), (0, -0.065, 0.0175), renk, 0.008)       # klavye tabanı
    K("DSepet", (0.20, 0.05, 0.012), (0, 0.0, 0.104), renk, 0.004)        # harf sepeti kapağı
    for r in range(4):                                                     # dört sıra tuş, arkaya doğru yükselir
        y, z = -0.105 + r * 0.022, 0.048 + r * 0.013
        for c in range(11):
            if rnd.random() < eksik_tus:
                continue
            x = -0.125 + c * 0.025 + (r % 2) * 0.008
            cubuk((x, y, 0.034), (x, y, z - 0.003), 0.0022, siyah_metal, kose=5)
            S("Tus", 0.0085, 0.006, (x, y, z), tus, kose=10)
    K("Bosluk", (0.14, 0.012, 0.008), (0, -0.117, 0.042), tus, 0.002)
    K("Marka", (0.07, 0.002, 0.012), (0, -0.011, 0.075), krom)
    for x in (-0.10, 0.10):                                                # şerit makaraları
        S("Makara", 0.028, 0.008, (x, 0.08, 0.104), siyah_metal, kose=16)
    if merdane:
        K("Sariyo", (0.38, 0.05, 0.02), (0, 0.10, 0.105), siyah_metal, 0.004)
        x_silindir(0.022, 0.36, (0, 0.10, 0.126), lastik, kose=16)
        for x in (-0.20, 0.20):
            x_silindir(0.018, 0.02, (x, 0.10, 0.126), siyah_metal, kose=14)
        cubuk((-0.19, 0.08, 0.13), (-0.24, 0.03, 0.155), 0.004, krom)     # satır başı kolu
        b = len(p)                                                         # takılı kâğıt ve yazılmış satırlar
        K("DKagit", (0.21, 0.002, 0.18), (0, 0.118, 0.215), kagit)
        for i in range(8):
            K("DSatir", (0.16 - (i % 3) * 0.03, 0.001, 0.003), (-0.01, 0.1165, 0.285 - i * 0.014), murekkep)
        grup(b, pivot=(0, 0.118, 0.126), rot=(-0.22, 0, 0))                # kâğıt arkaya yatık


def telefon(renk):
    """Çevirmeli masa telefonu, kadran -y'ye bakar."""
    beyaz = ortak.malzeme("K8_KadranBeyaz", (0.75, 0.74, 0.70), puruz=0.4)
    K("TGovde", (0.20, 0.22, 0.06), (0, 0.0, 0.03), renk, 0.02)
    K("TUst", (0.17, 0.12, 0.05), (0, 0.04, 0.075), renk, 0.02)
    b = len(p)                                                             # kadran: eğik ön yüzde
    S("Kadran", 0.046, 0.008, (0, 0, 0), renk, kose=32)
    S("KadranIc", 0.018, 0.010, (0, 0, 0.001), beyaz, kose=20)
    for i in range(10):
        a = math.radians(-60 + i * 28)
        S("Delik", 0.0075, 0.010, (0.031 * math.cos(a), 0.031 * math.sin(a), 0.001), beyaz, kose=10)
    grup(b, rot=(0.55, 0, 0), kaydir=(0, -0.055, 0.075))
    for x in (-0.06, 0.06):                                                # ahize çatalı
        K("Catal", (0.025, 0.03, 0.03), (x, 0.06, 0.11), renk, 0.006)
    x_silindir(0.016, 0.16, (0, 0.06, 0.135), renk, kose=14)             # ahize sapı
    for x in (-0.085, 0.085):
        S("Kulaklik", 0.027, 0.03, (x, 0.06, 0.125), renk, kose=18, pah=0.006)
    cubuk((-0.10, 0.03, 0.02), (-0.13, 0.00, 0.012), 0.004, renk)          # spiral kablo (sade)
    cubuk((-0.13, 0.00, 0.012), (-0.12, 0.07, 0.10), 0.004, renk)


def cay_bardagi(x, y, z, dolu=True):
    """Tabağında ince belli çay bardağı; dolu ise çay renginde."""
    S("CTabak", 0.036, 0.005, (x, y, z + 0.0025), porselen, kose=20)
    m = cay if dolu else cam
    koni(0.019, 0.015, 0.035, (x, y, z + 0.005 + 0.0175), m)
    koni(0.015, 0.023, 0.055, (x, y, z + 0.005 + 0.035 + 0.0275), m)
    K("Kasik", (0.004, 0.05, 0.002), (x + 0.028, y - 0.006, z + 0.006), krom, rot=(0, 0, 0.5))


# ======================================================================
if hangi == "bekleme":
    vinil = ortak.malzeme("K8_Vinil", (0.10, 0.030, 0.012), puruz=0.35)
    bant = ortak.malzeme("K8_KoliBandi", (0.45, 0.40, 0.28), puruz=0.5)
    # Kiriş ve iki ters T ayak: üç oturak tek kirişe vidalı (hastane/devlet dairesi bekleme sırası).
    K("Kiris", (1.48, 0.05, 0.04), (0, 0.02, 0.36), siyah_metal, 0.004)
    for x in (-0.60, 0.60):
        K("Dikme", (0.04, 0.04, 0.309), (x, 0.02, 0.1855), siyah_metal, 0.004)
        K("Ayak", (0.05, 0.48, 0.025), (x, 0.0, 0.0185), siyah_metal, 0.006)
        for y in (-0.21, 0.21):
            S("Tapa", 0.02, 0.006, (x, y, 0.003), lastik, kose=12)
    for i, x in enumerate((-0.49, 0.0, 0.49)):
        K("Baglanti", (0.30, 0.04, 0.03), (x, 0.02, 0.395), siyah_metal)
        K("Kabuk", (0.46, 0.44, 0.015), (x, -0.02, 0.4175), siyah_metal, 0.004)
        K("Minder", (0.45, 0.43, 0.06), (x, -0.025, 0.455), vinil, 0.025)
        # Arkalık: kirişten geriye, sonra yukarı çıkan lama; vinil arkalık onun önünde.
        K("Lama", (0.03, 0.27, 0.02), (x, 0.155, 0.37), siyah_metal)
        cubuk((x, 0.29, 0.37), (x, 0.29, 0.72), 0.012, siyah_metal)
        K("ArkaKabuk", (0.45, 0.012, 0.29), (x, 0.271, 0.66), siyah_metal, 0.004)
        K("Arkalik", (0.44, 0.05, 0.28), (x, 0.24, 0.66), vinil, 0.02)
    # Kolçaklar: iki uçta ve oturakların arasında, maun başlıklı.
    for x in (-0.735, -0.245, 0.245, 0.735):
        K("KolAlt", (0.025, 0.14, 0.02), (x, -0.04, 0.37), siyah_metal)
        K("KolDik", (0.025, 0.03, 0.26), (x, -0.10, 0.50), siyah_metal)
        K("Kolcak", (0.05, 0.34, 0.03), (x, -0.04, 0.645), maun_ac, 0.01)
    # Ortadaki minderin yırtığı koli bandıyla yamanmış (kullanılmışlık).
    K("Bant", (0.13, 0.05, 0.002), (0.05, -0.08, 0.486), bant, rot=(0, 0, 0.3))
    K("Bant", (0.11, 0.05, 0.002), (0.06, -0.07, 0.487), bant, rot=(0, 0, -0.5))
    ad, h, uzak, kaldir = "Bekleme_Sandalyeleri_K8", 0.8, 2.2, 0.0

elif hangi == "pano":
    mantar = ortak.malzeme("K8_Mantar", (0.30, 0.17, 0.07), puruz=0.95)
    sari = ortak.malzeme("K8_KagitSari", (0.75, 0.62, 0.25), puruz=0.9)
    pembe = ortak.malzeme("K8_KagitPembe", (0.70, 0.40, 0.42), puruz=0.9)
    rap = [ortak.malzeme("K8_Raptiye" + a, c, puruz=0.3) for a, c in
           (("Kirmizi", (0.40, 0.03, 0.02)), ("Mavi", (0.03, 0.08, 0.35)), ("Sari", (0.55, 0.45, 0.03)))]
    EN, BOY = 0.90, 0.60
    K("Arka", (EN, 0.012, BOY), (0, -0.006, 0), maun_koyu)                       # arka yüz y = 0
    K("Mantar", (EN - 0.06, 0.008, BOY - 0.06), (0, -0.016, 0), mantar)           # önü y = -0.020
    for z in (BOY / 2 - 0.015, -BOY / 2 + 0.015):
        K("Cita", (EN, 0.028, 0.03), (0, -0.014, z), maun, 0.004)
    for x in (EN / 2 - 0.015, -EN / 2 + 0.015):
        K("Cita", (0.03, 0.028, BOY - 0.06), (x, -0.014, 0), maun, 0.004)
    # Kâğıtlar: (x, z, en, boy, malzeme, tür, açı). Gerçek yazı yok; satır çizgileri yazı izlenimi verir.
    kagitlar = ((-0.22, 0.04, 0.21, 0.297, kagit, "tablo", -0.03),      # aidat listesi (tablo)
                (0.12, 0.10, 0.21, 0.15, sari, "duyuru", 0.05),         # su kesintisi duyurusu
                (0.10, -0.12, 0.18, 0.14, kagit, "duyuru", 0.02),
                (0.30, -0.14, 0.12, 0.10, pembe, "not", -0.12),
                (0.33, 0.16, 0.10, 0.14, kagit, "tablo", 0.08))
    for i, (x, z, en, boy, m, tur, aci) in enumerate(kagitlar):
        yb = -0.0205 - i * 0.0012                                       # her kâğıt öncekinin önünde
        ys = yb - 0.0011
        b = len(p)
        K("Ilan", (en, 0.0008, boy), (0, yb - 0.0004, 0), m)
        if tur == "tablo":
            K("Baslik", (en * 0.6, 0.0006, 0.012), (0, ys, boy / 2 - 0.025), murekkep)
            zz = boy / 2 - 0.05
            while zz > -boy / 2 + 0.02:
                K("Satir", (en - 0.03, 0.0006, 0.0015), (0, ys, zz), murekkep)
                zz -= 0.02
            for xx in (-en / 2 + 0.06, en / 4):
                K("Sutun", (0.0015, 0.0006, boy - 0.08), (xx, ys, -0.02), murekkep)
        elif tur == "duyuru":
            K("Baslik", (en * 0.6, 0.0006, 0.018), (0, ys, boy / 2 - 0.028), murekkep)
            for j in range(int((boy - 0.07) / 0.016)):
                K("Satir", (en * rnd.uniform(0.55, 0.8), 0.0006, 0.004), (0, ys, boy / 2 - 0.06 - j * 0.016), murekkep)
        else:
            for j in range(4):
                K("ElYazi", (en * 0.7, 0.0006, 0.004), (0, ys, boy / 2 - 0.03 - j * 0.02), murekkep, rot=(0, 0.05, 0))
        dik_disk(0.0065, 0.006, (0, yb - 0.004, boy / 2 - 0.013), rap[i % 3], kose=14)
        grup(b, rot=(0, aci, 0), kaydir=(x, 0, z))
    # Boşta kalmış raptiyeler ve koparılmış bir ilanın yırtık köşesi.
    for (x, z, j) in ((-0.05, 0.22, 0), (0.25, 0.02, 1), (-0.36, -0.20, 2)):
        dik_disk(0.0065, 0.006, (x, -0.024, z), rap[j], kose=14)
    K("Yirtik", (0.03, 0.0008, 0.025), (-0.05, -0.0206, 0.21), kagit, rot=(0, 0.6, 0))
    ad, h, uzak, kaldir = "Ilan_Panosu_K8", 0.7, 1.4, 0.35

elif hangi == "anahtar":
    acik = True
    boya = ortak.malzeme("K8_AnahtarDolabiBoya", (0.30, 0.29, 0.24), puruz=0.45, metal=0.35)
    celik = ortak.malzeme("K8_AnahtarCelik", (0.45, 0.45, 0.43), puruz=0.35, metal=0.9)
    fob = [ortak.malzeme("K8_Fob" + a, c, puruz=0.4) for a, c in
           (("Kirmizi", (0.35, 0.03, 0.02)), ("Mavi", (0.02, 0.06, 0.30)), ("Yesil", (0.03, 0.18, 0.05)))]
    EN, DER, BOY, t = 0.45, 0.10, 0.55, 0.008     # duvar eşyası: arka yüz y = 0, gövde -y'ye 10 cm
    x0, x1, zb, zt = -EN / 2, EN / 2, -BOY / 2, BOY / 2
    # Gövde levhaları (ince sac): arka, iki yan, üst, alt. Önü açık, kapı önünü kapatır.
    p += [ac.levha("Arka", x0, x1, -t, 0, zb, zt, boya, 0.001),
          ac.levha("Yan", x0, x0 + t, -DER, -t, zb, zt, boya, 0.001),
          ac.levha("Yan", x1 - t, x1, -DER, -t, zb, zt, boya, 0.001),
          ac.levha("Ust", x0 + t, x1 - t, -DER, -t, zt - t, zt, boya, 0.001),
          ac.levha("Alt", x0 + t, x1 - t, -DER, -t, zb, zb + t, boya, 0.001),
          # Alt raf: üst yüzü z = -0.16, önünde anahtar düşmesin diye dudak.
          ac.levha("Raf", x0 + t, x1 - t, -0.085, -t, -0.172, -0.160, boya, 0.001),
          ac.levha("RafDudak", x0 + t, x1 - t, -0.088, -0.083, -0.172, -0.150, boya, 0.001)]
    # Etiketli kancalar: üç sıra, beşer kanca; bazılarında anahtar asılı.
    for ri, z in enumerate((0.17, 0.06, -0.05)):
        for ci, x in enumerate((-0.16, -0.08, 0.0, 0.08, 0.16)):
            cubuk((x, -t, z), (x, -0.045, z), 0.0025, pirinc, kose=6)
            cubuk((x, -0.045, z), (x, -0.048, z + 0.012), 0.0025, pirinc, kose=6)   # kalkık uç
            K("KEtiket", (0.05, 0.001, 0.018), (x, -t - 0.0005, z + 0.03), kagit)
            K("KNo", (0.022, 0.0006, 0.004), (x, -t - 0.0013, z + 0.03), murekkep)
            if (ri * 5 + ci) % 2 == 0 and not (ri == 2 and ci == 4):
                halka(0.009, 0.0015, (x, -0.040, z - 0.008), celik, rot=(math.pi / 2, 0, 0), seg=(12, 4))
                K("Bas", (0.020, 0.003, 0.018), (x, -0.040, z - 0.024), celik, 0.002)
                K("Ag", (0.008, 0.002, 0.040), (x, -0.040, z - 0.052), celik)
                if ci % 3 == 0:
                    K("Fob", (0.018, 0.004, 0.03), (x + 0.012, -0.036, z - 0.030), fob[ri % 3], 0.003, rot=(0, 0.3, 0))
    # Menteşe silindirleri: sol kenarda, kapının dönme ekseninde.
    for z in (0.18, -0.18):
        S("Mentese", 0.005, 0.04, (x0, -DER - 0.009, z), boya, kose=10)
    # Kapı: sol kenardan menteşeli; kabartma pano, etiket çerçevesi, kilit, topuz. Pivot menteşenin alt ucu.
    on_y = -DER - ac.KAL
    b = len(p)
    p.append(ac.levha("Pano", x0 + 0.002, x1 - 0.002, on_y, on_y + ac.KAL, zb + 0.002, zt - 0.002, boya, 0.003))
    K("Kabartma", (EN - 0.08, 0.004, BOY - 0.08), (0, on_y - 0.002, 0), boya, 0.002)
    K("EtiketCerceve", (0.10, 0.003, 0.03), (0, on_y - 0.0055, 0.20), pirinc)
    K("EtiketKart", (0.085, 0.001, 0.02), (0, on_y - 0.0075, 0.20), kagit)
    K("EtiketYazi", (0.05, 0.0006, 0.004), (0, on_y - 0.0083, 0.20), murekkep)
    dik_disk(0.012, 0.006, (x1 - 0.04, on_y - 0.003, 0.02), krom, kose=16)
    K("AnahtarDeligi", (0.002, 0.002, 0.009), (x1 - 0.04, on_y - 0.0065, 0.02), murekkep)
    p.extend(ac.kulp(x1 - 0.04, on_y - 0.004, -0.05, krom, tip="topuz"))
    kapi = ortak.birlestir(p[b:], "Kapi_1")
    del p[b:]
    ortak.pivotu_tasi(kapi, (x0, on_y + ac.KAL / 2, zb))
    parcalar.append(kapi)
    ad, h, uzak, kaldir = "Anahtar_Dolabi_K8", 0.7, 1.0, 0.35

elif hangi == "kutular":
    kartonlar = [karton, karton2, karton3]
    # Dört kolon: (x, kaç kutu). Sonuncusu tek, kapağı açılmış; kapağı yerde.
    for ci, (x, n) in enumerate(((-0.53, 3), (-0.18, 2), (0.17, 3), (0.52, 1))):
        for k in range(n):
            b = len(p)
            arsiv_kutusu(0.33, 0.27, 0.25, kartonlar[(ci + k) % 3], kapakli=(ci != 3))
            a = 0.16 if (ci == 0 and k == 2) else rnd.uniform(-0.05, 0.05)
            grup(b, rot=(0, 0, a), kaydir=(x + rnd.uniform(-0.01, 0.01), rnd.uniform(-0.015, 0.01), k * 0.25))
    b = len(p)
    K("KKapak", (0.338, 0.278, 0.035), (0, 0, 0.0175), karton3, 0.004)
    grup(b, rot=(0, 0, 0.35), kaydir=(0.62, -0.33, 0))
    ad, h, uzak, kaldir = "Arsiv_Kutulari_K8", 0.75, 2.2, 0.0

elif hangi == "masa":
    yesil_deri = ortak.malzeme("K8_SumenDeri", (0.02, 0.055, 0.03), puruz=0.5)
    yesil_cam = ortak.malzeme("K8_LambaCam", (0.02, 0.14, 0.04), puruz=0.08)
    ampul = ortak.malzeme("K8_Ampul", (0.85, 0.80, 0.60), puruz=0.3)
    mor = ortak.malzeme("K8_Istampa", (0.12, 0.03, 0.20), puruz=0.6)
    dosya_m = [ortak.malzeme("K8_Dosya" + a, c, puruz=0.8) for a, c in
               (("Mavi", (0.05, 0.12, 0.30)), ("Pembe", (0.55, 0.25, 0.28)), ("Manila", (0.55, 0.42, 0.20)))]
    daktilo_m = ortak.malzeme("K8_DaktiloZeytin", (0.05, 0.06, 0.05), puruz=0.35, metal=0.3)
    T = 0.76
    # Masa: tabla, iki keson, ziyaretçi yüzünde (-y) kapalı etek; müdür tarafı (+y) çekmeceli.
    K("Tabla", (1.50, 0.75, 0.04), (0, 0, T - 0.02), maun, 0.006)
    K("TablaAlt", (1.46, 0.71, 0.02), (0, 0, T - 0.05), maun_koyu, 0.003)
    for x in (-0.52, 0.52):
        K("Keson", (0.42, 0.68, 0.66), (x, 0, 0.39), maun, 0.006)
        K("Kaide", (0.40, 0.64, 0.06), (x, 0, 0.03), maun_koyu, 0.004)
        K("OnPano", (0.34, 0.006, 0.52), (x, -0.343, 0.40), maun_ac, 0.003)
    K("Etek", (0.62, 0.025, 0.46), (0, -0.30, 0.47), maun, 0.004)
    K("EtekPano", (0.52, 0.006, 0.34), (0, -0.3155, 0.47), maun_ac, 0.003)
    K("KalemCekmecesi", (0.60, 0.02, 0.07), (0, 0.35, 0.665), maun_ac, 0.004)

    def cekme_kulp(x, z):
        """+y'ye bakan pirinç çubuk kulp (müdür tarafı)."""
        K("Kulp", (0.10, 0.012, 0.012), (x, 0.378, z), pirinc, 0.003)
        for dx in (-0.044, 0.044):
            K("KulpAyak", (0.012, 0.018, 0.012), (x + dx, 0.366, z), pirinc)

    cekme_kulp(0, 0.665)
    for (za, zu) in ((0.52, 0.70), (0.30, 0.50), (0.08, 0.28)):          # sol keson: üç çekmece
        K("Cekmece", (0.38, 0.02, zu - za), (-0.52, 0.35, (za + zu) / 2), maun_ac, 0.004)
        cekme_kulp(-0.52, (za + zu) / 2 + 0.03)
    K("Cekmece", (0.38, 0.02, 0.12), (0.52, 0.35, 0.64), maun_ac, 0.004)   # sağ keson: çekmece + kapak
    cekme_kulp(0.52, 0.64)
    K("Kapak", (0.38, 0.02, 0.48), (0.52, 0.35, 0.32), maun_ac, 0.004)
    K("KapakTopuz", (0.02, 0.02, 0.02), (0.38, 0.37, 0.40), pirinc, 0.006)
    K("KapakKilit", (0.012, 0.004, 0.02), (0.38, 0.362, 0.45), pirinc)
    # Sümen: yeşil deri, koyu kenar şeritleri; üstünde açık bir dosya ve kâğıt.
    K("Sumen", (0.62, 0.40, 0.004), (-0.12, 0.12, T + 0.002), yesil_deri)
    for x in (-0.43, 0.19):
        K("SumenKenar", (0.04, 0.40, 0.006), (x, 0.12, T + 0.003), deri, 0.002)
    K("AcikDosya", (0.42, 0.30, 0.002), (-0.12, 0.13, T + 0.005), dosya_m[2])
    K("DKagit", (0.20, 0.28, 0.001), (-0.02, 0.13, T + 0.0065), kagit)
    for i in range(9):
        K("DSatir", (0.14 - (i % 4) * 0.02, 0.003, 0.0006), (-0.03, 0.25 - i * 0.022, T + 0.0073), murekkep)
    # Evrak yığını (sol ön): kâğıtlar ve renkli dosyalar hafif kaymış.
    zz = T
    for i in range(10):
        if i in (3, 7):
            K("Dosya", (0.24, 0.32, 0.006), (-0.58 + rnd.uniform(-0.01, 0.01), 0.18, zz + 0.003), dosya_m[i % 2],
              rot=(0, 0, rnd.uniform(-0.06, 0.06)))
            zz += 0.006
        else:
            K("Kagit", (0.21, 0.297, 0.004), (-0.58 + rnd.uniform(-0.015, 0.015), 0.18 + rnd.uniform(-0.01, 0.01), zz + 0.002),
              kagit if i % 2 else kagit_eski, rot=(0, 0, rnd.uniform(-0.09, 0.09)))
            zz += 0.004
    # Daktilo: sağda, klavyesi müdüre dönük.
    b = len(p)
    daktilo(daktilo_m)
    grup(b, rot=(0, 0, math.pi + 0.06), kaydir=(0.45, 0.07, T))
    # Çevirmeli telefon: sol arkada, müdüre dönük.
    b = len(p)
    telefon(ortak.malzeme("K8_Bakalit", (0.02, 0.02, 0.02), puruz=0.25))
    grup(b, rot=(0, 0, math.pi - 0.35), kaydir=(-0.30, -0.20, T))
    # Mühür takımı: askılıkta üç mühür, yanında kapağı açık ıstampa.
    b = len(p)
    K("MTaban", (0.22, 0.08, 0.02), (0, 0, 0.01), maun_ac, 0.004)
    for x in (-0.10, 0.10):
        K("MDikme", (0.02, 0.03, 0.10), (x, 0, 0.07), maun_ac, 0.003)
    K("MRay", (0.22, 0.05, 0.015), (0, 0, 0.1275), maun_ac, 0.003)
    for x in (-0.055, 0.0, 0.055):
        K("Lastik", (0.04, 0.025, 0.015), (x, 0, 0.0375), lastik, 0.002)
        S("MSap", 0.006, 0.10, (x, 0, 0.095), maun_koyu, kose=10)
        kure(0.014, (x, 0, 0.152), maun_koyu, olcek=(1, 1, 0.8))
    K("IKutu", (0.11, 0.07, 0.012), (0.20, 0.0, 0.006), siyah_metal, 0.002)
    K("IPed", (0.10, 0.06, 0.002), (0.20, 0.0, 0.0125), mor)
    b2 = len(p)
    K("IKapak", (0.11, 0.004, 0.07), (0.20, 0.035, 0.047), siyah_metal, 0.001)   # dik açılmış kapak
    grup(b2, pivot=(0.20, 0.035, 0.012), rot=(-0.25, 0, 0))
    grup(b, rot=(0, 0, math.pi + 0.1), kaydir=(0.02, -0.22, T))
    # Yeşil camlı banker lambası: sol arka köşe.
    b = len(p)
    K("LTaban", (0.18, 0.11, 0.022), (0, 0, 0.011), pirinc, 0.008)
    S("LGovde", 0.009, 0.28, (0, 0.03, 0.162), pirinc, kose=12)
    cubuk((0, 0.03, 0.30), (0, -0.005, 0.33), 0.008, pirinc)
    bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=0.065, depth=0.26, location=(0, -0.02, 0.33), rotation=(0, math.pi / 2, 0))
    ab = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(ab, yesil_cam)
    bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=0.058, depth=0.25, location=(0, -0.02, 0.33), rotation=(0, math.pi / 2, 0))
    ic = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.oy(ab, ic)                                                       # içi oyuk
    ortak.oy(ab, ortak.kutu("Kes", (0.30, 0.20, 0.08), (0, -0.02, 0.29), yesil_cam))   # alt yarısı kesik
    p.append(ab)
    kure(0.02, (0, -0.02, 0.318), ampul)
    cubuk((0.03, -0.02, 0.30), (0.03, -0.02, 0.24), 0.0015, pirinc, kose=5)          # çekme zinciri
    kure(0.005, (0.03, -0.02, 0.238), pirinc)
    grup(b, rot=(0, 0, 0.25), kaydir=(-0.60, -0.26, T))
    # İsimlik: ziyaretçiye dönük eğik levha (yazı yerine pirinç şerit ve koyu çizgiler).
    b = len(p)
    K("ITaban", (0.24, 0.05, 0.012), (0, 0, 0.006), maun, 0.003)
    b2 = len(p)
    K("IYuz", (0.24, 0.008, 0.065), (0, 0, 0.0445), maun, 0.002)
    K("IPlaka", (0.20, 0.002, 0.035), (0, -0.005, 0.045), pirinc)
    for i, w in enumerate((0.12, 0.08)):
        K("IYazi", (w, 0.001, 0.005), (0, -0.0065, 0.052 - i * 0.013), murekkep)
    grup(b2, pivot=(0, 0, 0.012), rot=(-0.3, 0, 0))
    grup(b, kaydir=(0.35, -0.29, T))
    # Yarım kalmış çay: sümenin sağ üst köşesinde.
    cay_bardagi(0.15, 0.29, T + 0.004)
    ad, h, uzak, kaldir = "Mudur_Masasi_K8", 1.0, 2.4, 0.0

elif hangi == "makam":
    # Beş kollu ayak, tekerlekler, amortisör; kapitone rulolu yüksek arkalık hafif geriye yatık.
    for i in range(5):
        a = 2 * math.pi * i / 5 + math.pi / 2
        ux, uy = 0.28 * math.cos(a), 0.28 * math.sin(a)
        K("Kol", (0.28, 0.045, 0.03), (ux / 2, uy / 2, 0.07), siyah_metal, 0.008, rot=(0, 0, a))
        K("Catal", (0.022, 0.022, 0.03), (ux, uy, 0.055), siyah_metal)
        kure(0.026, (ux, uy, 0.026), lastik, olcek=(1, 0.7, 1))
    S("Gobek", 0.05, 0.06, (0, 0, 0.07), siyah_metal, kose=20)
    S("Kolon", 0.025, 0.30, (0, 0, 0.25), krom, kose=16)
    S("Kilif", 0.035, 0.14, (0, 0, 0.17), siyah_metal, kose=16)
    K("Mekanizma", (0.22, 0.22, 0.04), (0, 0, 0.41), siyah_metal, 0.006)
    K("Oturak", (0.54, 0.52, 0.10), (0, -0.02, 0.48), deri, 0.04)
    K("OturakUst", (0.48, 0.44, 0.03), (0, -0.03, 0.54), deri, 0.015)
    for s in (-1, 1):
        K("KolBag", (0.30, 0.05, 0.02), (s * 0.16, 0.05, 0.42), siyah_metal)
        K("KolDik", (0.03, 0.05, 0.22), (s * 0.29, 0.05, 0.52), siyah_metal)
        K("Kolcak", (0.08, 0.36, 0.06), (s * 0.29, 0.0, 0.66), deri, 0.025)
    K("ArkaBag", (0.08, 0.20, 0.03), (0, 0.20, 0.41), siyah_metal)
    b = len(p)
    K("ArkaDestek", (0.08, 0.03, 0.25), (0, 0.30, 0.50), siyah_metal)
    K("Arkalik", (0.50, 0.10, 0.66), (0, 0.24, 0.88), deri, 0.04)
    for z in (0.66, 0.78, 0.90, 1.02):
        x_silindir(0.045, 0.44, (0, 0.18, z), deri, kose=16)                # kapitone rulolar
    x_silindir(0.055, 0.48, (0, 0.20, 1.14), deri, kose=18)                  # baş rulosu
    grup(b, pivot=(0, 0.24, 0.55), rot=(-0.18, 0, 0))
    ad, h, uzak, kaldir = "Makam_Koltugu_K8", 1.2, 1.9, 0.0

elif hangi == "misafir":
    deri2 = ortak.malzeme("K8_DeriBordo", (0.11, 0.022, 0.014), puruz=0.45)
    for x in (-0.245, 0.245):
        K("OnAyak", (0.04, 0.04, 0.64), (x, -0.23, 0.32), maun, 0.006)
        K("ArkaAyak", (0.04, 0.04, 0.85), (x, 0.23, 0.425), maun, 0.006)
        K("YanKusak", (0.03, 0.43, 0.07), (x, 0, 0.38), maun, 0.004)
        K("Kolcak", (0.06, 0.52, 0.03), (x, -0.01, 0.655), maun, 0.01)
        K("AltKusak", (0.025, 0.43, 0.025), (x, 0, 0.12), maun, 0.003)
    for y in (-0.23, 0.23):
        K("Kusak", (0.45, 0.03, 0.07), (0, y, 0.38), maun, 0.004)
    K("Minder", (0.44, 0.42, 0.08), (0, -0.01, 0.455), deri2, 0.03)
    K("Arkalik", (0.44, 0.06, 0.36), (0, 0.225, 0.66), deri2, 0.025)
    K("TepeKusak", (0.53, 0.04, 0.05), (0, 0.23, 0.83), maun, 0.008)
    for i in range(9):                                                       # minder önünde pirinç çiviler
        kure(0.005, (-0.20 + i * 0.05, -0.221, 0.43), pirinc, seg=8)
    ad, h, uzak, kaldir = "Misafir_Koltugu_K8", 0.85, 1.6, 0.0

elif hangi == "dosyarafi":
    EN, DER, H = 1.20, 0.35, 2.00
    for x in (EN / 2 - 0.011, -EN / 2 + 0.011):
        K("Yan", (0.022, DER, H), (x, 0, H / 2), maun, 0.004)
    K("Tepe", (EN, DER + 0.01, 0.025), (0, -0.005, H - 0.0125), maun, 0.004)
    K("Baza", (EN - 0.044, 0.02, 0.08), (0, -DER / 2 + 0.03, 0.04), maun_koyu)
    K("Arka", (EN - 0.044, 0.008, H - 0.10), (0, DER / 2 - 0.004, 0.05 + (H - 0.10) / 2), maun_koyu)
    tabanlar = (0.10, 0.48, 0.86, 1.24, 1.62)                                # rafların üst yüzleri
    for z in tabanlar:
        K("Raf", (EN - 0.044, DER - 0.012, 0.022), (0, -0.006, z - 0.011), maun, 0.003)
    for si, z in enumerate(tabanlar):
        x = -0.575
        son = 0.575 if si != 4 else 0.12                                     # en üst raf yarı boş
        while True:
            w = 0.075 if rnd.random() < 0.75 else 0.05
            if x + w > son - 0.17:
                break
            klasor(x, z, w, rnd.choice(klasor_renkleri), -0.16)
            x += w + 0.002
        if si in (1, 3):                                                     # sıranın sonunda yatık klasör
            a = 0.25
            b = len(p)
            klasor(0.0, 0.0, 0.075, rnd.choice(klasor_renkleri), -0.16)
            grup(b, rot=(0, -a, 0), kaydir=(x + 0.315 * math.sin(a), 0, z))
    for i in range(3):                                                       # üst rafta yatay yığın
        K("YatikKlasor", (0.315, 0.28, 0.075), (0.40, -0.02, 1.62 + 0.0375 + i * 0.075), klasor_renkleri[(i * 3) % 7], 0.004)
        K("YEtiket", (0.08, 0.002, 0.045), (0.32, -0.161, 1.62 + 0.0375 + i * 0.075), etiket_m)
    ad, h, uzak, kaldir = "Dosya_Rafi_K8", 2.0, 3.0, 0.0

elif hangi == "kasa":
    yesil = ortak.malzeme("K8_KasaBoya", (0.025, 0.050, 0.035), puruz=0.45, metal=0.5)
    bordo = ortak.malzeme("K8_DefterBez", (0.14, 0.025, 0.02), puruz=0.8)
    for x in (-0.20, 0.20):
        for y in (-0.19, 0.21):
            K("Ayak", (0.06, 0.06, 0.06), (x, y, 0.03), siyah_metal, 0.006)
    K("Govde", (0.50, 0.48, 0.64), (0, 0.01, 0.38), yesil, 0.02)
    K("Kapi", (0.40, 0.02, 0.52), (0, -0.235, 0.38), yesil, 0.01)
    for (w, hh, x, z) in ((0.34, 0.004, 0, 0.61), (0.34, 0.004, 0, 0.15), (0.004, 0.46, -0.17, 0.38), (0.004, 0.46, 0.17, 0.38)):
        K("Yaldiz", (w, 0.002, hh), (x, -0.2455, z), pirinc)                 # yaldız fitil çerçeve
    dik_disk(0.05, 0.02, (0, -0.255, 0.47), krom, kose=32)                  # şifre kadranı
    dik_disk(0.02, 0.012, (0, -0.269, 0.47), siyah_metal, kose=16)
    for i in range(20):
        f = 2 * math.pi * i / 20
        K("Cizik", (0.003, 0.002, 0.009 if i % 5 else 0.014), (0.042 * math.sin(f), -0.2655, 0.47 + 0.042 * math.cos(f)),
          porselen, rot=(0, f, 0))
    K("Isaret", (0.004, 0.003, 0.012), (0, -0.2465, 0.535), pirinc)
    dik_disk(0.022, 0.02, (0, -0.255, 0.31), krom, kose=20)                 # kol göbeği
    K("Kol", (0.16, 0.018, 0.02), (0, -0.272, 0.31), krom, 0.006, rot=(0, 0.3, 0))
    K("Rozet", (0.03, 0.004, 0.05), (0.13, -0.247, 0.47), pirinc, 0.003)
    K("Anahtarlik", (0.003, 0.002, 0.014), (0.13, -0.2495, 0.47), delik)
    K("Plaka", (0.16, 0.003, 0.04), (0, -0.2465, 0.57), pirinc, 0.002)
    for i, w in enumerate((0.11, 0.07)):
        K("PYazi", (w, 0.001, 0.005), (0, -0.2485, 0.577 - i * 0.013), murekkep)
    for z in (0.22, 0.54):
        S("Mentese", 0.012, 0.08, (-0.205, -0.24, z), yesil, kose=12)
    K("Defter", (0.22, 0.30, 0.04), (0.05, 0.0, 0.72), bordo, 0.006, rot=(0, 0, 0.12))   # üstte kayıt defteri
    K("Sayfa", (0.21, 0.29, 0.03), (0.055, 0.003, 0.72), kagit_eski, rot=(0, 0, 0.12))
    ad, h, uzak, kaldir = "Kasa_K8", 0.75, 1.5, 0.0

elif hangi == "jaluzi":
    bej = ortak.malzeme("K8_JaluziBej", (0.52, 0.49, 0.42), puruz=0.4, metal=0.3)
    ipm = ortak.malzeme("K8_JaluziIp", (0.60, 0.58, 0.52), puruz=0.9)
    # Pivot: üst rayın arka üst kenarı (y = 0, z = 0); ray -y ve -z yönüne uzanır.
    K("Ray", (1.42, 0.05, 0.045), (0, -0.025, -0.0225), bej, 0.004)
    for x in (-0.70, 0.70):
        K("Tutucu", (0.03, 0.055, 0.05), (x, -0.0275, -0.025), bej, 0.003)
    i, z = 0, -0.07
    while z > -0.63:                                                          # yarıya kadar inmiş lameller, açık
        r = (0.60, 0, 0)
        if i == 9:
            r = (0.95, 0.03, 0)                                              # eğilmiş bir lamel
        elif i == 17:
            r = (0.35, -0.02, 0)
        K("Lamel", (1.38, 0.025, 0.0015), (0, -0.025, z), bej, rot=r)
        i += 1
        z -= 0.022
    K("AltRay", (1.38, 0.03, 0.016), (0, -0.025, -0.645), bej, 0.004)
    for x in (-0.58, 0.0, 0.58):                                              # merdiven ipleri ve kaldırma ipi
        for y in (-0.0135, -0.0365):
            K("Merdiven", (0.0015, 0.0015, 0.60), (x, y, -0.345), ipm)
        K("KaldirmaIpi", (0.002, 0.002, 0.60), (x, -0.025, -0.345), ipm)
    cubuk((0.66, -0.04, -0.045), (0.66, -0.045, -0.93), 0.002, ipm, kose=5)  # çekme ipi ve püskülü
    S("Puskul", 0.006, 0.035, (0.66, -0.045, -0.95), bej, kose=10)
    cubuk((-0.66, -0.05, -0.045), (-0.66, -0.055, -0.75), 0.005, bej, kose=8)   # çevirme çubuğu
    ad, h, uzak, kaldir = "Jaluzi_K8", 1.2, 2.4, 1.1

elif hangi == "saat":
    siyah_p = ortak.malzeme("K8_SaatKasa", (0.02, 0.02, 0.02), puruz=0.35)
    kadran = ortak.malzeme("K8_SaatKadran", (0.80, 0.79, 0.74), puruz=0.6)
    kirmizi = ortak.malzeme("K8_SaatSaniye", (0.40, 0.02, 0.02), puruz=0.4)
    dik_disk(0.172, 0.040, (0, -0.020, 0), siyah_p, kose=48)                 # kasa: arka yüz y = 0
    halka(0.166, 0.010, (0, -0.041, 0), siyah_p, rot=(math.pi / 2, 0, 0), seg=(48, 8))
    dik_disk(0.158, 0.004, (0, -0.039, 0), kadran, kose=48)                  # kadran önü y = -0.041
    for i in range(60):
        f = 2 * math.pi * i / 60
        if i % 5 == 0:
            K("Saat", (0.008, 0.002, 0.022), (0.135 * math.sin(f), -0.042, 0.135 * math.cos(f)), siyah_p, rot=(0, f, 0))
        else:
            K("Dakika", (0.003, 0.002, 0.008), (0.145 * math.sin(f), -0.042, 0.145 * math.cos(f)), siyah_p, rot=(0, f, 0))
    K("Marka", (0.04, 0.001, 0.005), (0, -0.0415, 0.06), siyah_p)
    # Akrep 9'da, yelkovan 12'de: saat dokuzda durmuş. Açı 12'den saat yönünde ölçülür (+x izleyicinin sağı).
    for (f, L, w, y, m) in ((math.radians(270), 0.085, 0.012, -0.044, siyah_p),
                            (0.0, 0.125, 0.008, -0.0455, siyah_p),
                            (math.radians(200), 0.13, 0.002, -0.047, kirmizi)):
        d = L / 2 - 0.015
        K("Ibre", (w, 0.0015, L), (d * math.sin(f), y, d * math.cos(f)), m, rot=(0, f, 0))
    dik_disk(0.008, 0.006, (0, -0.047, 0), siyah_p, kose=16)
    ad, h, uzak, kaldir = "Duvar_Saati_K8", 0.6, 0.9, 0.3

elif hangi == "cayocagi":
    formika = ortak.malzeme("K8_Formika", (0.40, 0.36, 0.26), puruz=0.35)
    krem = ortak.malzeme("K8_DolapKrem", (0.55, 0.50, 0.38), puruz=0.45)
    celik = ortak.malzeme("K8_SemaverCelik", (0.60, 0.58, 0.55), puruz=0.30, metal=0.6)   # tam metal karanlık odada kapkara görünür
    bakalit = ortak.malzeme("K8_SemaverBakalit", (0.02, 0.015, 0.012), puruz=0.35)
    mavi = ortak.malzeme("K8_DemlikBant", (0.05, 0.10, 0.35), puruz=0.25)
    EN, DER, H = 0.80, 0.45, 0.85
    K("Kaide", (EN - 0.06, DER - 0.08, 0.08), (0, 0.02, 0.04), maun_koyu)
    K("Dolap", (EN - 0.02, DER - 0.02, 0.73), (0, 0.01, 0.445), krem, 0.004)
    K("Tezgah", (EN, DER, 0.04), (0, 0, 0.83), formika, 0.004)
    K("Kenar", (EN + 0.004, 0.006, 0.03), (0, -DER / 2 - 0.003, 0.83), krom)
    for x in (-0.19, 0.19):
        K("Kapak", (0.37, 0.018, 0.68), (x, -0.224, 0.46), krem, 0.004)
        K("Kulp", (0.012, 0.02, 0.10), (x * 0.16, -0.24, 0.62), krom, 0.003)
    T = H
    # Semaver (sol): taban, gövde, iki bakalit kulp, önde musluk; tepesinde demlik.
    x, y = -0.20, 0.03
    S("STaban", 0.10, 0.03, (x, y, T + 0.015), celik, kose=28)
    S("SGovde", 0.105, 0.20, (x, y, T + 0.13), celik, kose=32, pah=0.01)
    S("SOmuz", 0.08, 0.03, (x, y, T + 0.245), celik, kose=28)
    for s in (-1, 1):
        halka(0.03, 0.006, (x + s * 0.115, y, T + 0.18), bakalit, rot=(math.pi / 2, 0, 0), seg=(16, 6))
    cubuk((x, y - 0.10, T + 0.08), (x, y - 0.15, T + 0.07), 0.008, celik)
    K("Musluk", (0.04, 0.01, 0.012), (x, y - 0.14, T + 0.092), bakalit, 0.003)
    S("Demlik", 0.065, 0.09, (x, y, T + 0.305), porselen, kose=28, pah=0.015)
    S("DBant", 0.066, 0.012, (x, y, T + 0.31), mavi, kose=28)
    S("DKapak", 0.045, 0.015, (x, y, T + 0.3575), porselen, kose=24)
    kure(0.012, (x, y, T + 0.372), porselen)
    cubuk((x, y - 0.055, T + 0.29), (x, y - 0.11, T + 0.335), 0.009, porselen)
    halka(0.032, 0.007, (x, y + 0.075, T + 0.31), porselen, rot=(0, math.pi / 2, 0), seg=(16, 6))
    cubuk((x, y + 0.10, T + 0.05), (x, DER / 2 - 0.01, T + 0.02), 0.004, bakalit)     # elektrik kablosu
    # Yuvarlak çelik tepsi ve altı bardak (dördü dolu, ikisi boş).
    tx, ty = 0.18, -0.02
    S("Tepsi", 0.17, 0.006, (tx, ty, T + 0.003), celik, kose=40)
    halka(0.168, 0.004, (tx, ty, T + 0.006), celik, seg=(40, 6))
    for i, (dx, dy) in enumerate(((-0.08, -0.06), (0.0, -0.06), (0.08, -0.06), (-0.08, 0.05), (0.0, 0.05), (0.08, 0.05))):
        cay_bardagi(tx + dx, ty + dy, T + 0.006, dolu=(i not in (2, 4)))
    S("Sekerlik", 0.04, 0.05, (0.30, 0.13, T + 0.025), porselen, kose=20, pah=0.008)
    S("SKapak", 0.042, 0.01, (0.30, 0.13, T + 0.055), porselen, kose=20)
    kure(0.008, (0.30, 0.13, T + 0.064), porselen)
    ad, h, uzak, kaldir = "Cay_Ocagi_K8", 1.2, 2.0, 0.0

elif hangi == "buzdolabi":
    # 80'lerin sonu / 90'ların başının yerli mini buzdolabı (büro ve çay ocağı tipi). Bugünün keskin köşeli,
    # parlak beyaz kutusu değil: köşeleri iri yuvarlatılmış sac gövde, kalın kapı, krom çubuk kol, kırmızı
    # zeminli krom harfli marka rozeti, sararmış fırın emayesi. Yıllarca silinmiş: kapı dibi paslı, kol
    # çevresinde emaye atmış, yan yüzde bir göçük. Üstünde dantel örtü, termos, çay bardağı, şeker kutusu.
    # Ölçü kutusu eskisiyle aynı: 0,50 x 0,52 x 1,07 m (gövde 0,85 m, üstündekilerle 1,07 m).
    mine = ortak.malzeme("K8_BuzMine", (0.62, 0.57, 0.44), puruz=0.35)       # sararmış emaye (doğrusal renk)
    pas = ortak.malzeme("K8_Pas", (0.20, 0.065, 0.02), puruz=0.9)             # pas lekesi: mat, koyu turuncu
    kirik = ortak.malzeme("K8_EmayeKirigi", (0.035, 0.035, 0.04), puruz=0.55, metal=0.4)  # emayesi atmış çıplak sac
    rozet_z = ortak.malzeme("K8_RozetZemin", (0.12, 0.012, 0.010), puruz=0.35)   # rozetin koyu kırmızı zemini
    cikartma = ortak.malzeme("K8_Cikartma", (0.08, 0.16, 0.35), puruz=0.6)    # solmuş mavi yuvarlak çıkartma
    termos_m = ortak.malzeme("K8_Termos", (0.30, 0.06, 0.02), puruz=0.35)     # solmuş turuncu-kırmızı plastik termos
    krem = ortak.malzeme("K8_TermosKrem", (0.60, 0.55, 0.42), puruz=0.4)      # termosun krem kapağı ve çiçekleri
    sari = ortak.malzeme("K8_CicekSari", (0.55, 0.38, 0.05), puruz=0.4)       # çiçek göbeği
    dantel = ortak.malzeme("K8_Dantel", (0.70, 0.66, 0.52), puruz=0.95)       # elde örülmüş, sararmış dantel
    seker = ortak.malzeme("K8_SekerKutusu", (0.40, 0.05, 0.03), puruz=0.6)    # kesme şeker kutusu

    def yuvarla(n, bolum):
        """K'nin eklediği pah değiştiricisine çok bölüm verir: köşe tek pahla kırılmaz, yuvarlak olur."""
        n.modifiers["Pah"].segments = bolum   # bölüm sayısı arttıkça kavis yumuşar (eski sac kalıp köşesi)

    pas_koyu = ortak.malzeme("K8_PasKoyu", (0.07, 0.028, 0.012), puruz=0.95)  # pasın kabuklanmış koyu göbeği

    def leke(x, z, r, mat, y=-0.2483, adet=3):
        """Kapının ön yüzüne yapışık düzensiz leke: üst üste binen birkaç küçük disk (pas, emaye kırığı)."""
        for i in range(adet):                                                # birkaç disk: tek daire yapay durur
            dik_disk(r * rnd.uniform(0.4, 1.0), 0.0012,                      # yarıçap rastgele küçülür; 1,2 mm kalınlık
                     (x + rnd.uniform(-r, r) * 1.6, y, z + rnd.uniform(-r, r) * 0.6), mat, kose=6)

    # GÖVDE: iri yuvarlatılmış sac kutu. Pah 4 cm, 4 bölüm: 80'lerin kalıptan çekilmiş köşesi.
    g = K("Govde", (0.50, 0.46, 0.83), (0, 0.02, 0.435), mine, 0.04)
    yuvarla(g, 4)                                                             # köşeleri yuvarla
    # Sağ yan yüzde bir göçük. Boolean ile oymak, koca düz yüzü tek bir çokgene çevirip yumuşak gölgede
    # yüz boyu bir çizgi bırakıyordu. Onun yerine: pahı uygula, yan yüzü ızgaraya böl, göçük bölgesindeki
    # köşeleri içeri it. Izgara küçük olduğu için gölge yalnızca göçüğün çevresinde değişir.
    import bmesh                                                              # meshi köşe köşe düzenleme kütüphanesi
    sec(g)                                                                    # modifier_apply yalnız etkin nesneye uygulanır
    bpy.ops.object.modifier_apply(modifier="Pah")                            # pahı meshe işle
    bm = bmesh.new()
    bm.from_mesh(g.data)                                                      # meshi düzenlenebilir hâle al
    yan = [f for f in bm.faces if f.normal.x > 0.99 and f.calc_center_median().x > 0.24]   # sağ yanın düz yüzü
    kenarlar = list({e for f in yan for e in f.edges})                        # o yüzün dört kenarı
    bmesh.ops.subdivide_edges(bm, edges=kenarlar, cuts=18, use_grid_fill=True)   # yüzü 19 x 19 ızgaraya böl
    GY, GZ, RY, RZ = 0.10, 0.34, 0.08, 0.065                                  # göçüğün merkezi ve iki yarıçapı (elips)
    for v in bm.verts:
        if v.co.x > 0.249:                                                    # yalnız yan yüzdeki köşeler
            t = ((v.co.y - GY) / RY) ** 2 + ((v.co.z - GZ) / RZ) ** 2         # elips içinde mi (t < 1)
            if t < 1:
                v.co.x -= 0.008 * (1 - t) ** 2                                # ortada 8 mm, kenara doğru yumuşak sıfır
    bm.to_mesh(g.data)                                                        # değişikliği meshe geri yaz
    bm.free()
    # Ayaklar: dört kısa lastik ayak.
    for x in (-0.21, 0.21):
        for y in (-0.17, 0.21):
            S("Ayak", 0.015, 0.02, (x, y, 0.01), lastik, kose=10)
    # Alt ön süpürgelik: kapının altında siyah plastik havalandırma ızgarası, iki sıra yarık.
    K("Supurgelik", (0.42, 0.03, 0.034), (0, -0.197, 0.04), siyah_metal, 0.004)
    for sira in range(2):
        for i in range(6):
            K("Yarik", (0.045, 0.002, 0.006), (-0.15 + i * 0.06, -0.2125, 0.033 + sira * 0.014), delik)
    # KAPI: kalın, yuvarlak kenarlı; gövdeden 3 mm önde, arada siyah conta çizgisi görünür.
    d = K("Kapi", (0.49, 0.035, 0.78), (0, -0.2305, 0.45), mine, 0.03)       # ön yüzü y = -0,248
    yuvarla(d, 3)
    K("Conta", (0.475, 0.004, 0.765), (0, -0.211, 0.45), lastik)              # kapı ile gövde arasındaki lastik
    # Menteşe kapakları: sol üst ve sol alt köşede siyah plastik.
    for z in (0.844, 0.056):
        K("Mentese", (0.04, 0.032, 0.008), (-0.215, -0.2305, z), siyah_metal)
    # Üstte yatay krom şerit ve altında marka rozeti: krom çerçeve, kırmızı zemin, krom harf blokları.
    K("Serit", (0.43, 0.004, 0.010), (0, -0.2495, 0.785), krom, 0.002)
    K("RozetCerceve", (0.125, 0.002, 0.036), (-0.12, -0.249, 0.748), krom, 0.002)
    K("Rozet", (0.115, 0.003, 0.028), (-0.12, -0.2495, 0.748), rozet_z, 0.002)
    for i in range(6):                                                        # okunmayan, soyut harf blokları
        K("Harf", (0.010, 0.0015, 0.014 if i % 2 else 0.012), (-0.16 + i * 0.016, -0.2513, 0.748), krom)
    # KOL: iki krom ayağa oturan dik krom çubuk; çubukta pas noktaları.
    for z in (0.42, 0.66):
        K("KolAyak", (0.022, 0.014, 0.03), (0.205, -0.255, z), krom)
    S("Kol", 0.0075, 0.30, (0.205, -0.262, 0.54), krom, kose=12, pah=0.003)
    for z in (0.47, 0.52, 0.61):
        K("KolPas", (0.004, 0.003, 0.005), (0.205, -0.269, z + rnd.uniform(-0.01, 0.01)), pas_koyu)
    # Yıllarca tutulan kolun çevresinde emaye atmış; kapı dibinde pas çiçeklenmiş.
    for z in (0.50, 0.57, 0.63, 0.45):
        leke(0.180 + rnd.uniform(-0.01, 0.01), z, rnd.uniform(0.003, 0.006), kirik, adet=2)
    # Kapı dibi: alt kenar boyunca pas kümeleri; her kümenin ortası kabuklanmış (koyu), dibe doğru akıntı izi.
    for x in (-0.19, -0.16, -0.06, 0.08, 0.15, 0.19):
        z = 0.098 + rnd.uniform(0.0, 0.012)                                   # yuvarlak kenarın hemen üstü
        leke(x, z, rnd.uniform(0.005, 0.010), pas, adet=3)                    # açık pas halesi
        leke(x, z, 0.003, pas_koyu, y=-0.2486, adet=1)                        # koyu göbek, halenin önünde
        if rnd.random() < 0.6:
            K("PasAkinti", (0.0025, 0.0012, 0.02), (x + rnd.uniform(-0.004, 0.004), -0.2484, z - 0.008), pas)
    # Sağ yanın dibinde de pas (yer silinirken su sıçramış).
    for y in (-0.14, -0.06, 0.08, 0.17):
        for i in range(3):
            x_silindir(rnd.uniform(0.003, 0.009), 0.0012,
                       (0.2503, y + rnd.uniform(-0.025, 0.025), 0.085 + rnd.uniform(0.0, 0.02)), pas, kose=6)
    # Bantlı el yazısı not (eskiden de vardı), yanında eski bir bantın sararmış artığı ve yarısı kazınmış çıkartma.
    K("Not", (0.10, 0.002, 0.07), (-0.05, -0.2492, 0.64), kagit_eski, rot=(0, 0.05, 0))
    for i in range(3):
        K("NotYazi", (0.07, 0.001, 0.004), (-0.05, -0.2505, 0.655 - i * 0.015), murekkep, rot=(0, 0.05, 0))
    K("Bant", (0.03, 0.001, 0.012), (-0.05, -0.2505, 0.676), etiket_m)
    K("BantIzi", (0.03, 0.0008, 0.012), (0.06, -0.2486, 0.70), etiket_m, rot=(0, -0.3, 0))
    dik_disk(0.02, 0.0012, (0.10, -0.2485, 0.30), cikartma, kose=20)
    dik_disk(0.011, 0.0012, (0.104, -0.2492, 0.303), kagit_eski, kose=14)    # kazınıp beyazı açılmış yer
    # Arka ızgara (kondenser): yatay çubuklar ve iki dik tel.
    for i in range(6):
        K("Izgara", (0.40, 0.01, 0.006), (0, 0.255, 0.15 + i * 0.1), siyah_metal)
    for x in (-0.18, 0.18):
        cubuk((x, 0.257, 0.12), (x, 0.257, 0.67), 0.003, siyah_metal, kose=6)
    # ÜSTÜ (gövde üstü z = 0,85): dantel örtü, termos, boş çay bardağı, kesme şeker kutusu.
    S("Dantel", 0.14, 0.002, (-0.03, 0.02, 0.851), dantel, kose=32)
    for i in range(26):                                                       # tırtıllı dantel kenarı: sık, küçük fistolar
        a = 2 * math.pi * i / 26
        S("DantelKenar", 0.011, 0.0018, (-0.03 + 0.142 * math.cos(a), 0.02 + 0.142 * math.sin(a), 0.8509), dantel, kose=6)
    for R in (0.10, 0.06):                                                    # örgü halkaları (kabarık iplik)
        halka(R, 0.0016, (-0.03, 0.02, 0.852), dantel, seg=(24, 3))
    TX, TY = -0.09, 0.07                                                      # termosun yeri
    TR = 0.043                                                                # termos yarıçapı (8,6 cm çap, 1 litrelik)
    S("Termos", TR, 0.15, (TX, TY, 0.927), termos_m, kose=24, pah=0.006)    # z 0,852..1,002
    for z in (0.862, 0.992):
        halka(TR, 0.0035, (TX, TY, z), krom, seg=(24, 3))                     # alt ve üst krom bilezik
    koni(TR, 0.038, 0.015, (TX, TY, 1.0095), krem, kose=24)                   # omuz (z 1,002..1,017)
    koni(0.039, 0.041, 0.05, (TX, TY, 1.042), krem, kose=24)                  # bardak olan kapak (üstü 1,067)
    halka(0.0405, 0.0015, (TX, TY, 1.056), krem, seg=(24, 3))                 # kapağın tutma yivi
    K("TermosKulp", (0.013, 0.02, 0.11), (TX + TR + 0.016, TY, 0.93), termos_m, 0.005)
    for z in (0.88, 0.98):
        K("KulpBag", (0.02, 0.016, 0.014), (TX + TR + 0.006, TY, z), termos_m)
    for fi, z in ((-0.5, 0.94), (0.2, 0.91), (0.9, 0.95)):                    # yüzeye teğet çiçek baskıları
        q = (TX + (TR + 0.0005) * math.sin(fi), TY - (TR + 0.0005) * math.cos(fi), z)
        dondurulmus_silindir(0.010, 0.002, q, (math.pi / 2, 0, fi), krem, 10)
        q = (TX + (TR + 0.0015) * math.sin(fi), TY - (TR + 0.0015) * math.cos(fi), z)
        dondurulmus_silindir(0.0035, 0.002, q, (math.pi / 2, 0, fi), sari, 6)
    cay_bardagi(0.10, -0.09, 0.85, dolu=False)                                # dibinde çay izi kalmış boş bardak
    K("Seker", (0.12, 0.08, 0.06), (0.13, 0.10, 0.88), seker, 0.004, rot=(0, 0, 0.25))
    K("SekerBant", (0.122, 0.082, 0.018), (0.13, 0.10, 0.885), kagit, rot=(0, 0, 0.25))   # beyaz bant
    ad, h, uzak, kaldir = "Buzdolabi_K8", 1.0, 1.6, 0.0

elif hangi == "sandalye":
    turuncu = ortak.malzeme("K8_PlastikTuruncu", (0.38, 0.09, 0.02), puruz=0.35)
    for x in (-0.18, 0.18):
        cubuk((x, -0.18, 0.0), (x, -0.18, 0.44), 0.011, krom)
        cubuk((x, 0.18, 0.0), (x, 0.18, 0.44), 0.011, krom)
        cubuk((x, 0.18, 0.44), (x, 0.22, 0.84), 0.011, krom)                 # arka dikme hafif geriye
        cubuk((x, -0.18, 0.44), (x, 0.18, 0.44), 0.011, krom)
        cubuk((x, -0.18, 0.12), (x, 0.18, 0.12), 0.008, krom)
        for y in (-0.18, 0.18):
            S("Tapa", 0.013, 0.01, (x, y, 0.005), lastik, kose=10)
    cubuk((-0.18, -0.18, 0.12), (0.18, -0.18, 0.12), 0.008, krom)
    K("Oturak", (0.42, 0.42, 0.02), (0, -0.005, 0.461), turuncu, 0.008)
    x_silindir(0.012, 0.40, (0, -0.21, 0.459), turuncu, kose=12)            # kıvrık ön kenar
    K("Arkalik", (0.40, 0.018, 0.20), (0, 0.188, 0.72), turuncu, 0.008, rot=(-0.1, 0, 0))
    ad, h, uzak, kaldir = "Mutfak_Sandalyesi_K8", 0.85, 1.5, 0.0

elif hangi == "araba":
    # 90'larda bir apartman hademesinin derme çatma "arabası": markalı plastik temizlik arabası değil.
    # Meyve kasası tahtalarından çakılmış alçak bir platform, altına eski koltuk tekerlekleri vidalanmış
    # (biri sonradan takılmış, rengi farklı), arkasına bükülmüş su borusundan bir itme kolu, kolu bez
    # bantla sarılı. Üstünde: göçük, paslı galvaniz kova, içinde tahta saplı saçaklı paspas, kenarında
    # yer bezi; sağda bir meyve kasası, içinde 5 litrelik çamaşır suyu bidonu ve dik duran saman süpürge;
    # itme koluna tel çengelle asılmış teneke faraş; kovanın önünde kalıp yeşil sabun.
    # Ölçü kutusu eskisiyle aynı: yaklaşık 0,79 x 0,42 x 1,15 m. Pivot tabanın ortası, ön yüz -y.
    import bmesh                                                              # meshi köşe köşe düzenleme kütüphanesi
    galv = ortak.malzeme("K8_Galvaniz", (0.40, 0.41, 0.40), puruz=0.6, metal=0.6)   # eskimiş, donuk çinko kaplama
    galv_koyu = ortak.malzeme("K8_GalvanizKoyu", (0.18, 0.19, 0.18), puruz=0.7, metal=0.5)   # kararmış çinko
    pas = ortak.malzeme("K8_Pas", (0.20, 0.065, 0.02), puruz=0.9)
    tahta = ortak.malzeme("K8_KasaTahta", (0.30, 0.21, 0.11), puruz=0.85)    # ham kavak tahtası
    tahta_gri = ortak.malzeme("K8_KasaTahtaGri", (0.17, 0.145, 0.11), puruz=0.9)   # yıllanıp grileşmiş tahta
    sap = ortak.malzeme("K8_PaspasSap", (0.24, 0.13, 0.05), puruz=0.6)       # elle aşınmış çıplak kayın sap
    tutamak_m = ortak.malzeme("K8_KovaTutamak", (0.06, 0.03, 0.015), puruz=0.6)   # kararmış tahta tutamak
    sacak = ortak.malzeme("K8_PaspasSacak", (0.24, 0.22, 0.17), puruz=0.95)  # kirden grileşmiş pamuk saçak
    bez_m = ortak.malzeme("K8_YerBezi", (0.26, 0.25, 0.22), puruz=0.95)      # gri yer bezi
    bant_m = ortak.malzeme("K8_BezBant", (0.10, 0.085, 0.07), puruz=0.9)     # kola sarılmış kararmış bez şerit
    su = ortak.malzeme("K8_KirliSu", (0.10, 0.11, 0.08), puruz=0.08)
    saman = ortak.malzeme("K8_Saman", (0.42, 0.30, 0.09), puruz=0.9)         # süpürge darısı (saman sarısı)
    saman_koyu = ortak.malzeme("K8_SamanKoyu", (0.25, 0.16, 0.05), puruz=0.9)   # uçları kirlenmiş saman
    iplik_k = ortak.malzeme("K8_SupurgeIpKirmizi", (0.30, 0.03, 0.02), puruz=0.8)
    iplik_y = ortak.malzeme("K8_SupurgeIpYesil", (0.03, 0.15, 0.05), puruz=0.8)
    bidon_m = ortak.malzeme("K8_Bidon", (0.60, 0.58, 0.50), puruz=0.45)      # sararmış beyaz plastik bidon
    mavi_k = ortak.malzeme("K8_KapakMavi", (0.03, 0.08, 0.35), puruz=0.4)
    kirmizi_m = ortak.malzeme("K8_EtiketKirmizi", (0.35, 0.04, 0.03), puruz=0.7)
    sabun_m = ortak.malzeme("K8_SabunYesil", (0.09, 0.20, 0.05), puruz=0.3)  # kalıp yeşil sabun
    teker_krem = ortak.malzeme("K8_TekerKrem", (0.45, 0.40, 0.30), puruz=0.5)  # sonradan takılmış teker

    # ---------- PLATFORM: dört kasa tahtası, altında iki kayıt, dört eski teker ----------
    UST = 0.093                                                               # platformun üst yüzü
    for i, y in enumerate((-0.15, -0.05, 0.05, 0.15)):                         # tahtalar arasında 5 mm aralık
        K("Tahta", (0.78 - rnd.uniform(0.0, 0.015), 0.095, 0.018), (rnd.uniform(-0.004, 0.004), y, UST - 0.009),
          tahta if i % 2 else tahta_gri)
        for x in (-0.30, 0.30):                                               # kayda çakılmış çivi başları
            S("Civi", 0.004, 0.0015, (x + rnd.uniform(-0.01, 0.01), y, UST + 0.0004), pas, kose=6)
    for x in (-0.30, 0.30):
        K("Kayit", (0.05, 0.40, 0.02), (x, 0, UST - 0.028), tahta_gri)        # alttaki çapraz kayıtlar
    for i, (x, y) in enumerate(((-0.32, -0.15), (-0.32, 0.15), (0.32, -0.15), (0.32, 0.15))):
        K("TekerPlaka", (0.05, 0.05, 0.004), (x, y, 0.053), galv_koyu)       # kayda vidalı plaka
        for s in (-1, 1):
            K("TekerCatal", (0.03, 0.004, 0.035), (x, y + s * 0.013, 0.037), pas)   # paslı çatal
        dik_disk(0.030, 0.020, (x, y, 0.030), teker_krem if i == 2 else lastik, kose=12)   # teker (y ekseninde döner)
        dik_disk(0.010, 0.024, (x, y, 0.030), galv_koyu, kose=8)              # mil göbeği

    # ---------- İTME KOLU: bükülmüş su borusu, bez bantla sarılı tutamak ----------
    XK = 0.37                                                                 # borunun x'i
    for y in (-0.16, 0.16):
        S("Flans", 0.025, 0.006, (XK, y, UST + 0.003), galv_koyu, kose=10)
        cubuk((XK, y, UST), (XK, y, 0.89), 0.011, galv, kose=10)              # dik boru
        cubuk((XK, y, UST + 0.004), (XK, y, UST + 0.045), 0.0118, pas, kose=10)   # dipte pas halkası
        kure(0.013, (XK, y, 0.89), galv, seg=8)                               # dirsek
    cubuk((XK, -0.16, 0.89), (XK, 0.16, 0.89), 0.011, galv, kose=10)          # yatay kol
    cubuk((XK, -0.10, 0.89), (XK, 0.10, 0.89), 0.0145, bant_m, kose=10)       # bez bant sargı
    for y in (-0.085, -0.03, 0.025, 0.08):
        halka(0.0148, 0.0022, (XK, y, 0.89), bant_m, rot=(math.pi / 2, 0, 0), seg=(10, 3))   # sargı katları

    # ---------- KOVA: konik galvaniz kova, iki göçük, pas, tel sap ----------
    KX, KY, TABAN, KH, RA, RU = -0.20, 0.0, UST, 0.28, 0.12, 0.15
    AGIZ = TABAN + KH                                                         # kova ağzının yüksekliği

    def kova_r(z):
        """z yüksekliğinde kovanın dış yarıçapı (aşağıdan yukarı genişler)."""
        return RA + (RU - RA) * (z - TABAN) / KH

    # Kabuk: üstü açık koni, yan yüzü yatay halkalara bölünür (göçük için köşe lazım), göçük bölgesindeki
    # köşeler eksene doğru itilir, sonra Solidify (kalınlık) ile 5 mm et kalınlığı verilir. Boolean ile
    # oymaktan farkı: göçük duvarın iki yüzünü birlikte büker, duvar delinmez.
    bpy.ops.mesh.primitive_cone_add(vertices=28, radius1=RA, radius2=RU, depth=KH,
                                    location=(KX, KY, TABAN + KH / 2), end_fill_type="NGON")
    kova = bpy.context.active_object                                          # köşeleri nesnenin kendi ekseninde
    bm = bmesh.new()
    bm.from_mesh(kova.data)
    ust_yuz = [f for f in bm.faces if all(v.co.z > KH / 2 - 1e-4 for v in f.verts)]
    bmesh.ops.delete(bm, geom=ust_yuz, context="FACES_ONLY")                  # ağzı aç
    dik = [e for e in bm.edges if abs(e.verts[0].co.z - e.verts[1].co.z) > KH * 0.9]
    bmesh.ops.subdivide_edges(bm, edges=dik, cuts=10, use_grid_fill=True)    # yan yüzü 11 halkaya böl
    for aci, zg, R, derin in ((-0.75, TABAN + 0.14, 0.045, 0.011),            # önde, alt bantla dip arasında
                              (2.30, TABAN + 0.235, 0.035, 0.008)):           # arkada, ağza yakın
        zl = zg - (TABAN + KH / 2)                                            # dünya yüksekliği -> nesnenin ekseni
        c = mathutils.Vector((kova_r(zg) * math.cos(aci), kova_r(zg) * math.sin(aci), zl))
        for v in bm.verts:
            d = (v.co - c).length
            if d < R:
                radyal = mathutils.Vector((v.co.x, v.co.y, 0)).normalized()  # eksenden dışarı doğru
                v.co -= radyal * derin * (1 - (d / R) ** 2)                   # ortada derin, kenarda sıfır
    bm.to_mesh(kova.data)
    bm.free()
    kal = kova.modifiers.new("Kalinlik", "SOLIDIFY")                          # birleştirmede uygulanır
    kal.thickness = 0.005
    kal.offset = -1                                                           # içeri doğru kalınlaş (dış ölçü aynı)
    ortak.ata(kova, galv)
    p.append(kova)
    for zb in (TABAN + 0.09, TABAN + 0.19):                                    # sacı sertleştiren iki boğum
        halka(kova_r(zb) + 0.001, 0.004, (KX, KY, zb), galv, seg=(28, 3))
    halka(RU, 0.006, (KX, KY, AGIZ), galv, seg=(28, 4))                        # kıvrılmış ağız kenarı
    halka(RA + 0.002, 0.005, (KX, KY, TABAN + 0.005), pas, seg=(28, 3))        # paslanmış dip çemberi
    S("Su", kova_r(0.25) - 0.006, 0.004, (KX, KY, 0.25), su, kose=28)        # kirli su

    def kova_lekesi(aci, z, en, boy, mat):
        """Kova yüzüne teğet ince leke (pas, kararmış çinko)."""
        r = kova_r(z) + 0.0005
        K("KovaLeke", (en, 0.0015, boy), (KX + r * math.cos(aci), KY + r * math.sin(aci), z), mat,
          rot=(0, 0, aci + math.pi / 2))                                     # kutunun ince ekseni dışarı bakar

    for aci, z, en, boy in ((-0.25, 0.115, 0.02, 0.014), (-1.25, 0.12, 0.016, 0.02), (0.35, 0.11, 0.025, 0.012),
                            (-0.55, 0.335, 0.012, 0.01)):
        kova_lekesi(aci, z, en, boy, pas)
    for aci, z, en, boy in ((-1.45, 0.30, 0.05, 0.035), (-0.05, 0.235, 0.04, 0.05)):
        kova_lekesi(aci, z, en, boy, galv_koyu)
    # Kulaklar ve tel sap: sap kulaklarda döner; biraz sağa (kasaya) yatmış, ortasında tahta tutamak.
    ZK = AGIZ - 0.035
    for s in (-1, 1):
        K("Kulak", (0.03, 0.006, 0.035), (KX, s * (kova_r(ZK) + 0.003), ZK), galv_koyu)
        dik_disk(0.004, 0.0075, (KX, s * (kova_r(ZK) + 0.004), ZK), galv, kose=6)   # perçin
    RS, EGIM = kova_r(ZK) + 0.006, 0.35                                       # sapın yarıçapı ve yatma açısı
    nok = []
    for i in range(11):                                                       # yarım daire, 10 parça tel
        t = math.pi * i / 10
        nok.append((KX + math.sin(EGIM) * RS * math.sin(t), -RS * math.cos(t), ZK + math.cos(EGIM) * RS * math.sin(t)))
    for a, b in zip(nok, nok[1:]):
        cubuk(a, b, 0.0028, galv, kose=6)
    cubuk(nok[4], nok[6], 0.011, tutamak_m, kose=10)                          # tahta tutamak
    # Saçaklı paspas: sap duvara yaslı, başı kovanın içinde; saçakların bir kısmı suda, bir kısmı
    # kova ağzından dışarı sarkmış (ıslak, birbirine yapışmış, uçları dağınık).
    H = mathutils.Vector((KX + 0.01, 0.02, 0.335))                            # paspas başı: ağzın altında, suya yakın
    T = mathutils.Vector((-0.25, 0.185, 1.13))                                # sapın tepesi (arkadaki duvara yaslı)
    yon = (T - H).normalized()
    cubuk(H, H + yon * 0.06, 0.02, galv_koyu, kose=12)                        # saçağı sıkan sac kelepçe
    cubuk(H + yon * 0.05, T, 0.012, sap, kose=10)                            # tahta sap
    S("SapUcu", 0.0125, 0.012, tuple(T), tutamak_m, kose=10)                 # elle kararmış sap ucu
    for i in range(26):
        a = 2 * math.pi * i / 26 + rnd.uniform(-0.1, 0.1)                     # saçaklar eşit aralıklı değil
        r0 = rnd.uniform(0.008, 0.018)                                        # kelepçeden çıktığı halka
        bas = (H.x + r0 * math.cos(a), H.y + r0 * math.sin(a), H.z - rnd.uniform(0.0, 0.015))
        kal_s = rnd.uniform(0.005, 0.0075)                                    # her saçak farklı kalınlıkta (ıslak, topaklı)
        if 3.5 < a < 5.0:                                                     # ön-sol: ağızdan dışarı sarkanlar
            kenar = (KX + 0.152 * math.cos(a), KY + 0.152 * math.sin(a), AGIZ + 0.012)
            ru = rnd.uniform(0.155, 0.168)
            uc = (KX + ru * math.cos(a + rnd.uniform(-0.12, 0.12)), KY + ru * math.sin(a + rnd.uniform(-0.12, 0.12)),
                  AGIZ - rnd.uniform(0.06, 0.17))                             # uçlar farklı boyda, yana savrulmuş
            cubuk(bas, kenar, kal_s, sacak, kose=5)
            cubuk(kenar, uc, kal_s, sacak, kose=5)
        else:                                                                 # suya inenler (uçları su altında)
            r = rnd.uniform(0.04, 0.12)
            cubuk(bas, (KX + r * math.cos(a), KY + r * math.sin(a), 0.24), kal_s, sacak, kose=5)
    # Yer bezi: kovanın ön-sağ ağzına atılmış; içeride biraz suya iner, kenardan aşıp dışarı sarkar.
    # Izgara düzlemi (UV'si hazır) yol boyunca bükülür, sonra 3 mm kalınlık verilir.
    yol = [(0.130, 0.30), (0.140, 0.355), (0.150, 0.383), (0.158, 0.386), (0.163, 0.36),
           (0.166, 0.30), (0.168, 0.24), (0.170, 0.19)]                       # (eksenden uzaklık, yükseklik)
    A0, A1 = 5.2, 6.05                                                        # bezin kapladığı açı aralığı
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=9, y_subdivisions=12, size=1, location=(0, 0, 0))
    bez = bpy.context.active_object
    for v in bez.data.vertices:
        u, w = v.co.x + 0.5, v.co.y + 0.5                                     # 0..1: u bez eni, w yol boyu
        s = w * (len(yol) - 1)
        i = min(int(s), len(yol) - 2)
        f = s - i
        r = yol[i][0] + (yol[i + 1][0] - yol[i][0]) * f                       # yol üzerinde ara değer
        z = yol[i][1] + (yol[i + 1][1] - yol[i][1]) * f
        r += 0.008 * math.sin(u * 11 + w * 4) * w                             # kumaş kırışığı (dışarı sarktıkça artar)
        if w > 0.6:
            z += 0.025 * math.sin(u * 7 + 1.3) * (w - 0.6)                    # sarkan ucun düzensiz kenarı
        a = A0 + (A1 - A0) * u
        v.co = (KX + r * math.cos(a), KY + r * math.sin(a), z)
    kal = bez.modifiers.new("Kalinlik", "SOLIDIFY")
    kal.thickness = 0.003
    kal.offset = 0
    ortak.ata(bez, bez_m)
    p.append(bez)
    # Kovanın önünde, platformun köşesinde kullanılmış kalıp yeşil sabun.
    K("Sabun", (0.085, 0.055, 0.028), (-0.27, -0.16, UST + 0.014), sabun_m, 0.009, rot=(0, 0, 0.25))

    # ---------- MEYVE KASASI: köşe dikmeleri, uç tahtaları (el delikli), çıtalar; ön üst çıta kırık ----------
    X0, X1, YK, KY2 = 0.0, 0.335, 0.175, 0.23                                 # sol/sağ kenar, yarı derinlik, yükseklik
    for x in (X0 + 0.011, X1 - 0.011):
        for y in (-YK + 0.011, YK - 0.011):
            K("Dikme", (0.022, 0.022, KY2), (x, y, UST + KY2 / 2), tahta_gri)
    for x, s in ((X0 + 0.006, -1), (X1 - 0.006, 1)):
        K("UcAlt", (0.012, 2 * YK - 0.004, 0.09), (x, 0, UST + 0.055), tahta)
        K("UcUst", (0.012, 2 * YK - 0.004, 0.09), (x, 0, UST + 0.175), tahta)
        K("ElDeligi", (0.0016, 0.08, 0.024), (x + s * 0.0062, 0, UST + 0.19), delik)
    for s in (-1, 1):                                                         # ön ve arka çıtalar
        y = s * (YK - 0.005)
        for j, z in enumerate((UST + 0.035, UST + 0.11, UST + 0.19)):
            if s == -1 and j == 2:                                            # ön üst çıta kırık: iki parça
                K("Cita", (0.15, 0.01, 0.045), (X0 + 0.075, y, z), tahta)
                K("Cita", (0.10, 0.01, 0.045), (X1 - 0.05, y, z), tahta)
                K("Kiymik", (0.04, 0.008, 0.012), (X0 + 0.165, y, z + 0.012), tahta, rot=(0, 0.35, 0))
                K("Kiymik", (0.03, 0.008, 0.010), (X1 - 0.11, y, z - 0.01), tahta, rot=(0, -0.5, 0))
            else:
                K("Cita", (X1 - X0, 0.01, 0.045), ((X0 + X1) / 2, y, z), tahta if (j + s) % 2 else tahta_gri)
            if s == -1:                                                       # öndeki çivi başları
                for x in (X0 + 0.011, X1 - 0.011):
                    dik_disk(0.0035, 0.0015, (x, -YK - 0.0005, z), pas, kose=6)
    for y in (-0.12, 0.0, 0.12):
        K("KasaTaban", (X1 - X0, 0.09, 0.01), ((X0 + X1) / 2, y, UST + 0.005), tahta_gri)
    K("KasaEtiket", (0.0012, 0.13, 0.07), (X1 + 0.0006, 0.02, UST + 0.17), kagit_eski, rot=(0.06, 0, 0))
    K("KasaEtiketBaski", (0.0014, 0.09, 0.012), (X1 + 0.0008, 0.02, UST + 0.185), kirmizi_m, rot=(0.06, 0, 0))

    # ---------- ÇAMAŞIR SUYU BİDONU: 5 litre, sararmış beyaz, mavi kapak, kâğıt etiket ----------
    BX, BY, BZ = 0.10, -0.06, UST + 0.01                                      # bidonun tabanı kasanın dibinde
    bd = K("Bidon", (0.15, 0.105, 0.29), (BX, BY, BZ + 0.145), bidon_m, 0.02)
    bd.modifiers["Pah"].segments = 3                                          # şişirme plastik: yuvarlak köşe
    K("BidonOmuz", (0.12, 0.085, 0.03), (BX - 0.01, BY, BZ + 0.30), bidon_m, 0.012)
    S("BidonAgiz", 0.017, 0.02, (BX - 0.045, BY, BZ + 0.32), bidon_m, kose=12)
    S("BidonKapak", 0.019, 0.022, (BX - 0.045, BY, BZ + 0.338), mavi_k, kose=12, pah=0.003)
    halka(0.032, 0.008, (BX + 0.03, BY, BZ + 0.305), bidon_m, rot=(math.pi / 2, 0, 0), seg=(14, 5))   # kulp
    K("BidonEtiket", (0.11, 0.0012, 0.10), (BX, BY - 0.0531, BZ + 0.22), kagit_eski)
    K("BidonBant", (0.11, 0.0014, 0.025), (BX, BY - 0.0533, BZ + 0.25), kirmizi_m)
    for i in range(3):
        K("BidonYazi", (0.07 - i * 0.012, 0.0014, 0.005), (BX, BY - 0.0533, BZ + 0.215 - i * 0.015), murekkep)
    K("Akinti", (0.006, 0.0012, 0.08), (BX + 0.05, BY - 0.0531, BZ + 0.15), saman_koyu)   # sararmış akıntı izi

    # ---------- SAMAN SÜPÜRGE: sapı kasada, başı yukarıda (saman ezilmesin diye), kola yaslı ----------
    B0 = mathutils.Vector((0.26, 0.09, UST + 0.01))                           # sapın dibi (kasanın tabanı)
    B1 = mathutils.Vector((0.326, 0.075, 0.76))                               # sapın süpürge başına girdiği yer
    dy = (B1 - B0).normalized()
    cubuk(B0, B1 + dy * 0.10, 0.012, sap, kose=10)                           # sap başın içine 10 cm girer
    b = len(p)                                                                # süpürge başı: merkezde kurulur
    for k in range(13):                                                       # yelpaze gibi açılan on üç ince demet
        ang = (k - 6) / 6 * 0.32 + rnd.uniform(-0.02, 0.02)
        L = 0.33 + rnd.uniform(-0.035, 0.03)                                  # uçlar düzensiz boyda
        yon_d = mathutils.Vector((0, math.sin(ang), math.cos(ang)))
        bas_d = mathutils.Vector((rnd.uniform(-0.004, 0.004), (k - 6) * 0.0055, 0))   # tek düz levha olmasın
        K("Demet", (rnd.uniform(0.009, 0.015), 0.022, L), tuple(bas_d + yon_d * L / 2),
          saman if k % 4 else saman_koyu, rot=(-ang, 0, 0))
        K("DemetUc", (0.010, 0.021, 0.03), tuple(bas_d + yon_d * (L - 0.012)), saman_koyu, rot=(-ang, 0, 0))
    for k in range(10):                                                       # demetlerden kaçmış tek tük saman çöpü
        ang = rnd.uniform(-0.45, 0.45)
        c0 = mathutils.Vector((rnd.uniform(-0.008, 0.008), math.sin(ang) * 0.18, math.cos(ang) * 0.18))
        cubuk(tuple(c0), tuple(c0 + mathutils.Vector((rnd.uniform(-0.02, 0.02), math.sin(ang * 1.3),
                                                      math.cos(ang * 1.3))) * rnd.uniform(0.14, 0.19)),
              0.0016, saman, kose=4)
    for j, l in enumerate((0.03, 0.07, 0.11, 0.15)):                          # renkli iplik dikiş sıraları
        K("Iplik", (0.022, 0.085 + 2 * 0.31 * l, 0.008), (0, 0, l), iplik_k if j % 2 == 0 else iplik_y)
    grup(b, pivot=(0, 0, 0), rot=(-math.asin(dy.y), math.atan2(dy.x, dy.z), 0), kaydir=tuple(B1))

    # ---------- TENEKE FARAŞ: itme koluna tel çengelle asılı, içi dışarı (+x) bakar ----------
    # Ağzı aşağıda asılı: taban sacı dik, yan duvarlar sapa (yukarı) doğru yükselir, ağza doğru incelir.
    YD, XP, FD = -0.06, 0.36, 0.06                                            # faraşın y'si, taban sacının x'i, duvar derinliği
    taban = K("FarasTaban", (0.0015, 0.20, 0.20), (XP, YD, 0.52), galv)       # z 0,42..0,62
    for v in taban.data.vertices:                                             # yamuk taban: ağız (alt) daha geniş
        if v.co.z < 0.52:
            v.co.y = YD + (v.co.y - YD) * 1.2                                 # alt kenar 20 cm -> 24 cm
    for s in (-1, 1):
        yan = K("FarasYan", (FD, 0.0015, 0.20), (XP + FD / 2, YD + s * 0.10, 0.52), galv)
        for v in yan.data.vertices:                                           # kutuyu kamaya çevir
            if v.co.z < 0.52:
                v.co.y += s * 0.02                                            # alt uç tabanın geniş kenarına gider
                if v.co.x > XP + 0.01:
                    v.co.x -= FD - 0.008                                      # ağız tarafında duvar 8 mm'ye iner
    K("FarasArka", (FD, 0.20, 0.0015), (XP + FD / 2, YD, 0.62), galv)         # sapın bağlandığı arka duvar
    K("FarasDudak", (0.004, 0.24, 0.004), (XP + 0.001, YD, 0.421), galv_koyu)   # ağızdaki kıvrık sac
    cubuk((XP + 0.012, YD, 0.62), (0.372, YD, 0.845), 0.009, galv, kose=8)    # boru sap, kancaya uzanır
    halka(0.03, 0.002, (XK, YD, 0.875), galv_koyu, rot=(math.pi / 2, 0, 0), seg=(14, 3))   # tel çengel
    for y in (-0.08, -0.03, 0.05, 0.09):                                      # ağız kenarında pas
        K("FarasPas", (0.0012, rnd.uniform(0.015, 0.03), rnd.uniform(0.006, 0.014)), (XP + 0.0009, YD + y, 0.428), pas)

    # ---------- Yerde birkaç kopmuş saman çöpü ----------
    for i in range(4):
        x = rnd.uniform(-0.03, 0.28)
        cubuk((x, -0.188, UST + 0.0015), (x + rnd.uniform(-0.06, 0.06), -0.188 + rnd.uniform(-0.006, 0.006),
              UST + 0.0015), 0.0014, saman, kose=4)
    ad, h, uzak, kaldir = "Temizlik_Arabasi_K8", 1.0, 2.0, 0.0

elif hangi == "arsivrafi":
    boya = ortak.malzeme("K8_RafBoya", (0.17, 0.20, 0.17), puruz=0.5, metal=0.4)
    kartonlar = [karton, karton2, karton3]
    EN, DER, H = 1.40, 0.45, 2.00
    # Delikli L köşebent dikmeler (her köşede iki levha).
    for sx in (-1, 1):
        for sy in (-1, 1):
            cx, cy = sx * EN / 2, sy * DER / 2
            K("KoseX", (0.04, 0.003, H), (cx - sx * 0.02, cy - sy * 0.0015, H / 2), boya)
            K("KoseY", (0.003, 0.04, H), (cx - sx * 0.0015, cy - sy * 0.02, H / 2), boya)
            if sy == -1:                                                     # ön yüzde delik sırası
                z = 0.05
                while z < H - 0.03:
                    K("Delik", (0.008, 0.001, 0.018), (cx - sx * 0.02, cy - 0.0035, z), delik)
                    z += 0.05
    raf_z = (0.08, 0.46, 0.84, 1.22, 1.60, 1.98)
    for z in raf_z:
        K("Raf", (EN, DER, 0.004), (0, 0, z), boya)
        for sy in (-1, 1):
            K("Dudak", (EN, 0.003, 0.03), (0, sy * (DER / 2 - 0.0015), z - 0.013), boya)
    for (z0, z1) in ((0.10, 1.0), (1.0, 1.95)):                               # arkada çapraz gergi
        cubuk((-0.69, 0.224, z0), (0.69, 0.224, z1), 0.005, boya, kose=6)
        cubuk((0.69, 0.224, z0), (-0.69, 0.224, z1), 0.005, boya, kose=6)
    # Raflarda: arşiv kutuları, dik klasör kutuları, iple bağlı desteler (rastgele ama sabit tohumlu).
    for z in raf_z[:-1]:
        zt = z + 0.002
        x = -0.67
        while x < 0.60:
            s = rnd.random()
            if s < 0.4 and x + 0.32 < 0.68:
                b = len(p)
                arsiv_kutusu(0.30, 0.38, 0.26, rnd.choice(kartonlar))
                if rnd.random() < 0.5:
                    b2 = len(p)
                    paket(0.22, 0.30, 0.07)
                    grup(b2, rot=(0, 0, rnd.uniform(-0.2, 0.2)), kaydir=(0, 0, 0.26))
                grup(b, rot=(0, 0, rnd.uniform(-0.04, 0.04)), kaydir=(x + 0.16, 0.0, zt))
                x += 0.33
            elif s < 0.7:
                for k in range(rnd.randint(3, 5)):
                    if x + 0.085 > 0.68:
                        break
                    klasor(x, zt, 0.08, rnd.choice(klasor_renkleri[3:5] + [karton2, karton3]), -0.16, der=0.30, boy=0.31)
                    x += 0.085
            else:
                if x + 0.25 > 0.68:
                    break
                for k in range(rnd.randint(2, 4)):
                    b = len(p)
                    paket(0.22, 0.30, 0.08)
                    grup(b, rot=(0, 0, rnd.uniform(-0.12, 0.12)), kaydir=(x + 0.125, -0.02, zt + k * 0.083))
                x += 0.26
            x += rnd.uniform(0.0, 0.04)
    ad, h, uzak, kaldir = "Arsiv_Rafi_K8", 2.0, 3.2, 0.0

elif hangi == "daktilolar":
    koli_m = karton
    yesil_bant = ortak.malzeme("K8_FormYesil", (0.30, 0.45, 0.30), puruz=0.9)
    tus = ortak.malzeme("K8_DaktiloTus", (0.55, 0.53, 0.45), puruz=0.4)
    # 1) Dik duran, tuşları dökülmüş bej daktilo.
    b = len(p)
    daktilo(ortak.malzeme("K8_DaktiloBej", (0.30, 0.28, 0.22), puruz=0.4, metal=0.2), eksik_tus=0.3)
    grup(b, rot=(0, 0, 0.35), kaydir=(-0.42, 0.10, 0))
    # 2) Yan yatmış, merdanesi sökük koyu mavi daktilo; merdanesi yanında yerde.
    b = len(p)
    daktilo(ortak.malzeme("K8_DaktiloMavi", (0.04, 0.06, 0.10), puruz=0.35, metal=0.3), eksik_tus=0.1, merdane=False)
    yere_oturt(grup(b, rot=(0, math.pi / 2, -0.4), kaydir=(0.05, 0.16, 0)))
    b = len(p)
    x_silindir(0.022, 0.36, (0, 0, 0.022), lastik, kose=16)
    for x in (-0.19, 0.19):
        x_silindir(0.018, 0.02, (x, 0, 0.022), siyah_metal, kose=14)
    grup(b, rot=(0, 0, 0.5), kaydir=(0.02, -0.22, 0))
    for (x, y) in ((-0.20, -0.12), (0.22, -0.02), (-0.05, -0.30)):             # yere saçılmış tuşlar
        S("Tus", 0.0085, 0.006, (x, y, 0.003), tus, kose=10)
    # 3) Kapakları açık koli; içinde yeşil bantlı sürekli form, bir kısmı öne dökülmüş.
    kx, ky, KE, KD, KH = 0.62, 0.0, 0.40, 0.30, 0.28
    koli = K("Koli", (KE, KD, KH), (kx, ky, KH / 2), koli_m)
    ortak.oy(koli, ortak.kutu("Oyuk", (KE - 0.01, KD - 0.01, KH), (kx, ky, KH / 2 + 0.006), koli_m))
    K("Form", (KE - 0.02, KD - 0.02, 0.22), (kx, ky, 0.006 + 0.11), kagit)
    for i in range(5):
        K("Bant", (KE - 0.02, 0.022, 0.0008), (kx, ky - 0.11 + i * 0.05, 0.2264), yesil_bant)
    for (hx, hy, e, d, r) in ((0, -KD / 2, KE, 0.15, (1.9, 0, 0)), (0, KD / 2, KE, 0.15, (-1.9, 0, 0))):
        b = len(p)
        K("Kanat", (e, 0.003, d), (kx + hx, ky + hy, KH + d / 2), koli_m)
        grup(b, pivot=(kx + hx, ky + hy, KH), rot=r)
    for (s, r) in ((-1, -1.9), (1, 1.9)):
        b = len(p)
        K("Kanat", (0.003, KD, 0.12), (kx + s * KE / 2, ky, KH + 0.06), koli_m)
        grup(b, pivot=(kx + s * KE / 2, ky, KH), rot=(0, r, 0))
    b = len(p)                                                                # öne dökülen form kâğıdı
    K("Dokulen", (KE - 0.04, 0.002, 0.26), (kx, ky - KD / 2 - 0.001, 0.13), kagit)
    grup(b, pivot=(kx, ky - KD / 2 - 0.001, 0.26), rot=(0.35, 0, 0))
    K("Yerde", (KE - 0.04, 0.26, 0.002), (kx, ky - KD / 2 - 0.22, 0.001), kagit)
    for i in range(3):
        K("YBant", (KE - 0.04, 0.022, 0.0008), (kx, ky - KD / 2 - 0.14 - i * 0.06, 0.0024), yesil_bant)
    ad, h, uzak, kaldir = "Eski_Daktilolar_K8", 0.4, 1.8, 0.0

elif hangi == "arsivdolabi":
    EN, DER, H = 1.00, 0.45, 2.00
    # Kapalı gövde (açılmaz): yanlar, baza, taban, orta ara, iki iç raf, arka, taç.
    for x in (EN / 2 - 0.0125, -EN / 2 + 0.0125):
        K("Yan", (0.025, DER, H - 0.08), (x, 0, 0.08 + (H - 0.08) / 2), maun, 0.004)
    K("Baza", (EN - 0.02, DER - 0.04, 0.08), (0, 0.01, 0.04), maun_koyu)
    K("Taban", (EN - 0.05, DER - 0.02, 0.025), (0, 0.0, 0.0925), maun)
    K("Ara", (EN - 0.05, DER - 0.02, 0.03), (0, 0.0, 0.80), maun)
    for z in (1.20, 1.60):
        K("Raf", (EN - 0.05, DER - 0.04, 0.022), (0, 0.01, z - 0.011), maun)
    K("Arka", (EN - 0.05, 0.01, H - 0.13), (0, DER / 2 - 0.005, 0.08 + (H - 0.13) / 2), maun_koyu)
    K("Tac", (EN + 0.04, DER + 0.03, 0.05), (0, -0.005, H - 0.025), maun, 0.008)
    K("TacAlt", (EN + 0.01, DER + 0.01, 0.02), (0, -0.002, H - 0.06), maun_ac, 0.004)
    # Camın ardında görünen klasörler (üç bölme).
    for z in (0.815, 1.20, 1.60):
        x = -0.465
        while x < 0.40:
            w = 0.075 if rnd.random() < 0.8 else 0.05
            if x + w > 0.465:
                break
            klasor(x, z, w, rnd.choice(klasor_renkleri), -0.14)
            x += w + 0.002 + (0.05 if rnd.random() < 0.06 else 0.0)
    # Alt dolu kapaklar: kabartma pano, pirinç topuz.
    yk = -DER / 2 - 0.011
    for s in (-1, 1):
        xc = s * 0.2475
        K("AltKapak", (0.485, 0.022, 0.70), (xc, yk, 0.4475), maun, 0.004)
        K("AltPano", (0.38, 0.008, 0.56), (xc, yk - 0.015, 0.4475), maun_ac, 0.003)
        kure(0.014, (s * 0.04, yk - 0.022, 0.50), pirinc, seg=12)
    # Üst camlı kapaklar: çerçeve (dikme, kayıt, orta kayıt) + ince cam.
    for s in (-1, 1):
        xc = s * 0.2475
        za, zu = 0.83, 1.94
        for dx in (-0.2175, 0.2175):
            K("Dikme", (0.05, 0.022, zu - za), (xc + dx, yk, (za + zu) / 2), maun, 0.003)
        for zz in (za + 0.03, zu - 0.03, (za + zu) / 2):
            K("Kayit", (0.385, 0.022, 0.06 if zz != (za + zu) / 2 else 0.03), (xc, yk, zz), maun, 0.003)
        K("Cam", (0.39, 0.004, zu - za - 0.10), (xc, yk + 0.004, (za + zu) / 2), cam)
        kure(0.012, (s * 0.04, yk - 0.022, 1.30), pirinc, seg=12)
    K("Rozet", (0.02, 0.004, 0.045), (0.0, yk - 0.013, 1.36), pirinc, 0.002)
    ad, h, uzak, kaldir = "Arsiv_Dolabi_K8", 2.0, 3.0, 0.0

elif hangi == "haritamasa":
    vinil = ortak.malzeme("K8_CizimVinil", (0.28, 0.34, 0.28), puruz=0.6)
    mavi = ortak.malzeme("K8_MaviKopya", (0.02, 0.06, 0.22), puruz=0.8)
    mavi_cizgi = ortak.malzeme("K8_MaviKopyaCizgi", (0.55, 0.62, 0.70), puruz=0.8)
    beyaz_rulo = ortak.malzeme("K8_PlanRulo", (0.70, 0.70, 0.66), puruz=0.9)
    sari = ortak.malzeme("K8_Kalem", (0.60, 0.40, 0.03), puruz=0.5)
    seffaf = ortak.malzeme("K8_Gonye", (0.35, 0.30, 0.12), puruz=0.2)
    lastik_b = ortak.malzeme("K8_LastikBant", (0.35, 0.08, 0.03), puruz=0.6)
    E = 0.14                                                                  # tabla eğimi (radyan, ~8°)
    # --- Eğik tabla: düz kurulur (üst yüzü z = 0, ön kenarı y = -0.45), sonra ön kenarından döndürülür.
    b = len(p)
    K("Tabla", (1.40, 0.90, 0.03), (0, 0, -0.015), maun_ac, 0.004)
    K("Kaplama", (1.36, 0.86, 0.002), (0, 0, 0.001), vinil)
    K("KalemPervazi", (1.40, 0.04, 0.025), (0, -0.47, 0.005), maun_ac, 0.003)
    sx, sy = -0.12, 0.02                                                      # mavi kopyanın merkezi
    K("MaviKopya", (0.84, 0.60, 0.001), (sx, sy, 0.0025), mavi)
    x_silindir(0.012, 0.84, (sx, sy + 0.31, 0.013), mavi, kose=12)          # arka kenarı kıvrılmış
    zc = 0.0033
    # Kat planı (3+1): dış duvarlar, oda bölmeleri, kapı boşlukları, ölçü çizgileri.
    W, D = 0.30, 0.20
    for (a, c) in (((-W, -D), (W, -D)), ((W, -D), (W, D)), ((W, D), (-W, D)), ((-W, D), (-W, -D)),
                   ((-0.05, -D), (-0.05, -0.04)), ((-0.05, 0.02), (-0.05, D)), ((-W, 0.0), (-0.12, 0.0)),
                   ((-0.08, 0.0), (-0.05, 0.0)), ((-0.05, 0.08), (0.08, 0.08)), ((0.13, 0.08), (W, 0.08)),
                   ((0.12, 0.08), (0.12, D)), ((0.12, -D), (0.12, -0.08)), ((0.12, -0.03), (0.12, 0.08))):
        cizgi_xy((sx + a[0], sy + a[1]), (sx + c[0], sy + c[1]), zc, 0.004, mavi_cizgi)
    for i in range(7):                                                        # ölçü çizgisi ve çentikler
        xx = sx - W + i * 0.1
        cizgi_xy((xx, sy - D - 0.035), (xx, sy - D - 0.02), zc, 0.0015, mavi_cizgi)
    cizgi_xy((sx - W, sy - D - 0.028), (sx + W, sy - D - 0.028), zc, 0.0015, mavi_cizgi)
    K("Antet", (0.16, 0.08, 0.0006), (sx + 0.32, sy - 0.24, zc), mavi_cizgi)      # antet kutusu
    K("AntetIc", (0.15, 0.07, 0.0006), (sx + 0.32, sy - 0.24, zc + 0.0004), mavi)
    for i in range(3):
        cizgi_xy((sx + 0.25, sy - 0.215 - i * 0.02), (sx + 0.38, sy - 0.215 - i * 0.02), zc + 0.0008, 0.003, mavi_cizgi)
    for (x, y) in ((sx - 0.40, sy - 0.27), (sx + 0.10, sy + 0.27)):           # köşe ağırlıkları
        S("Agirlik", 0.02, 0.025, (x, y, 0.0155), siyah_metal, kose=16)
    K("Cetvel", (1.36, 0.05, 0.012), (0, -0.20, 0.009), maun, 0.002)          # paralel cetvel
    for x in (-0.66, 0.66):
        S("CetvelMakara", 0.012, 0.018, (x, -0.20, 0.012), krom, kose=12)
    V = [(0.20, -0.36, 0), (0.44, -0.36, 0), (0.20, -0.12, 0)]               # gönye (üçgen)
    me = bpy.data.meshes.new("Gonye")
    me.from_pydata([(x, y, 0.016) for x, y, _ in V] + [(x, y, 0.019) for x, y, _ in V], [],
                   [(0, 2, 1), (3, 4, 5), (0, 1, 4, 3), (1, 2, 5, 4), (2, 0, 3, 5)])
    go = bpy.data.objects.new("Gonye", me)
    bpy.context.collection.objects.link(go)
    ortak.ata(go, seffaf)
    p.append(go)
    cubuk((-0.50, -0.43, 0.012), (-0.34, -0.44, 0.012), 0.004, sari, kose=6)  # pervazda kalem
    x_silindir(0.03, 0.60, (0.25, -0.42, 0.03), beyaz_rulo, kose=16)          # pervaza dayalı rulo plan
    grup(b, pivot=(0, -0.45, 0), rot=(E, 0, 0), kaydir=(0, 0, 0.78))
    # --- Sehpa: iki sehpa ayağı (üstleri tablanın eğimini izler), gergi, alt raf, raftaki rulolar.
    for x in (-0.62, 0.62):
        K("OnAyak", (0.05, 0.05, 0.76), (x, -0.36, 0.38), maun, 0.004)
        K("ArkaAyak", (0.05, 0.05, 0.86), (x, 0.36, 0.43), maun, 0.004)
        K("Kizak", (0.05, 0.80, 0.04), (x, 0, 0.02), maun, 0.004)
        K("UstKiris", (0.05, 0.80, 0.03), (x, 0, 0.78 + 0.45 * math.tan(E) - 0.05), maun, 0.003, rot=(E, 0, 0))
        K("RafKiris", (0.04, 0.50, 0.03), (x, 0.05, 0.165), maun, 0.003)
    K("Gergi", (1.24, 0.04, 0.04), (0, 0, 0.30), maun, 0.004)
    K("AltRaf", (1.20, 0.50, 0.02), (0, 0.05, 0.19), maun_ac, 0.003)
    for i, (y, r, L, m) in enumerate(((-0.12, 0.035, 0.80, beyaz_rulo), (-0.05, 0.03, 0.70, mavi),
                                      (0.02, 0.04, 0.90, beyaz_rulo), (0.09, 0.032, 0.62, mavi), (0.16, 0.036, 0.75, beyaz_rulo))):
        x_silindir(r, L, (rnd.uniform(-0.1, 0.1), y, 0.20 + r), m, kose=16)
        if i == 2:
            halka(0.041, 0.003, (0.0, y, 0.24), lastik_b, rot=(0, math.pi / 2, 0), seg=(16, 4))
    x_silindir(0.034, 0.72, (0.0, -0.085, 0.20 + 0.034 + 0.058), beyaz_rulo, kose=16)   # ikinci sıra
    ad, h, uzak, kaldir = "Harita_Masasi_K8", 1.0, 2.4, 0.0

elif hangi == "plan":
    zemin = ortak.malzeme("K8_PlanKagit", (0.72, 0.70, 0.62), puruz=0.9)
    pencere = ortak.malzeme("K8_PlanPencere", (0.10, 0.10, 0.11), puruz=0.9)
    # Zemin + 8 kat: bu, yönetimin duvarındaki RESMÎ cephe çizimi; iskân ruhsatıyla (not.kat8_ruhsat,
    # "Zemin + 8 kat") ve 1974 fotoğrafının arkasındaki "Ruhsatta sekiz kat yazıyor" satırıyla aynı
    # olmalı. Dokuzuncu kat yalnızca gizli planda (not.kat8_plan: "Ruhsatta yok"), notlarda ve asansörde
    # var: "kâğıtta olmayan kat" fikri resmî belgelerin sekiz kat demesiyle işler (#83, 9 idi).
    KAT_SAYISI = 8
    EN, BOY = 0.70, 0.50
    K("Arka", (EN, 0.006, BOY), (0, -0.003, 0), maun_koyu)
    K("Kagit", (EN - 0.07, 0.002, BOY - 0.07), (0, -0.012, 0), zemin)        # önü y = -0.013
    for z in (BOY / 2 - 0.0175, -BOY / 2 + 0.0175):
        K("Cerceve", (EN, 0.022, 0.035), (0, -0.011, z), siyah_metal, 0.003)
    for x in (EN / 2 - 0.0175, -EN / 2 + 0.0175):
        K("Cerceve", (0.035, 0.022, BOY - 0.07), (x, -0.011, 0), siyah_metal, 0.003)
    y = -0.0136
    bx0, bx1, z0 = -0.18, 0.08, -0.17
    fh = 0.33 / (KAT_SAYISI + 1)                                             # bir katın çizimdeki yüksekliği
    zt = z0 + fh * (KAT_SAYISI + 1)
    cizgi_xz((-0.24, z0), (0.14, z0), y, 0.003, murekkep)                    # zemin çizgisi
    for (a, c) in (((bx0, z0), (bx0, zt)), ((bx1, z0), (bx1, zt)), ((bx0 - 0.005, zt), (bx1 + 0.005, zt))):
        cizgi_xz(a, c, y, 0.0025, murekkep)
    for k in range(1, KAT_SAYISI + 1):                                       # kat çizgileri
        cizgi_xz((bx0, z0 + k * fh), (bx1, z0 + k * fh), y, 0.0012, murekkep)
    for k in range(KAT_SAYISI + 1):                                          # pencereler (merdiven sütunu hariç)
        for xx in (-0.155, -0.115, 0.015, 0.055):
            if k == 0 and xx in (-0.115, 0.015):
                continue
            K("Pencere", (0.022, 0.0006, fh * 0.45), (xx, y, z0 + k * fh + fh * 0.5), pencere)
        if k > 0:                                                            # merdiven penceresi yarım katta
            K("MPencere", (0.016, 0.0006, fh * 0.35), (-0.05, y, z0 + k * fh - fh * 0.05), pencere)
    K("GirisKapisi", (0.03, 0.0006, fh * 0.75), (-0.05, y, z0 + fh * 0.375), pencere)
    K("SuDeposu", (0.05, 0.0006, 0.018), (0.03, y, zt + 0.009), pencere)
    for k in range(KAT_SAYISI + 2):                                          # solda kat ölçü çizgisi
        cizgi_xz((-0.215, z0 + k * fh), (-0.205, z0 + k * fh), y, 0.0012, murekkep)
    cizgi_xz((-0.21, z0), (-0.21, zt), y, 0.0012, murekkep)
    for (a, c) in (((0.14, -0.19), (0.29, -0.19)), ((0.29, -0.19), (0.29, -0.10)), ((0.29, -0.10), (0.14, -0.10)),
                   ((0.14, -0.10), (0.14, -0.19)), ((0.14, -0.13), (0.29, -0.13))):   # antet kutusu
        cizgi_xz(a, c, y, 0.0015, murekkep)
    for i, w in enumerate((0.11, 0.08, 0.10)):
        K("AntetYazi", (w, 0.0006, 0.005), (0.215, y, -0.113 - i * 0.01 if i else -0.113), murekkep)
    K("Baslik", (0.18, 0.0006, 0.010), (0.19, y, 0.17), murekkep)            # başlık satırı izlenimi
    K("AltBaslik", (0.12, 0.0006, 0.005), (0.16, y, 0.15), murekkep)
    ad, h, uzak, kaldir = "Plan_Cercevesi_K8", 0.6, 1.2, 0.3

elif hangi == "toplantimasa":
    dosya_m = [ortak.malzeme("K8_TDosya" + a, c, puruz=0.8) for a, c in
               (("Kirmizi", (0.30, 0.04, 0.03)), ("Mavi", (0.04, 0.10, 0.28)), ("Manila", (0.55, 0.42, 0.20)))]
    kul_cam = ortak.malzeme("K8_KulTablasi", (0.30, 0.33, 0.30), puruz=0.05)
    kul = ortak.malzeme("K8_Kul", (0.25, 0.25, 0.24), puruz=1.0)
    filtre = ortak.malzeme("K8_Filtre", (0.50, 0.28, 0.08), puruz=0.8)
    T = 0.75
    # 1,6 x 0,8 (2,0 x 0,9 iken 2,5 m genişliğindeki toplantı odasında yürünecek yer kalmıyordu).
    K("Tabla", (1.6, 0.8, 0.04), (0, 0, T - 0.02), maun, 0.008)
    K("TablaAlt", (1.56, 0.76, 0.015), (0, 0, T - 0.0475), maun_koyu, 0.003)
    for y in (-0.35, 0.35):
        K("Etek", (1.40, 0.02, 0.08), (0, y, T - 0.095), maun, 0.004)
    for x in (-0.70, 0.70):
        K("EtekYan", (0.02, 0.66, 0.08), (x, 0, T - 0.095), maun, 0.004)
    for x in (-0.60, 0.60):
        K("Ayak", (0.05, 0.54, 0.63), (x, 0, 0.355), maun, 0.006)
        K("Kizak", (0.08, 0.70, 0.04), (x, 0, 0.02), maun_koyu, 0.006)
    K("Gergi", (1.14, 0.04, 0.10), (0, 0, 0.30), maun, 0.004)
    # Dosyalar: ikisi kapalı (lastikli), biri açık ve kâğıtları kaymış.
    for i, (x, y, a) in enumerate(((-0.45, -0.20, 0.1), (0.50, 0.18, -0.15))):
        b = len(p)
        K("Dosya", (0.25, 0.33, 0.008), (0, 0, 0.004), dosya_m[i])
        K("DLastik", (0.006, 0.334, 0.010), (0.08, 0, 0.004), lastik)
        grup(b, rot=(0, 0, a), kaydir=(x, y, T))
    b = len(p)
    K("AcikDosya", (0.50, 0.33, 0.003), (0, 0, 0.0015), dosya_m[2])
    for j in range(4):
        K("Kagit", (0.21, 0.297, 0.002), (0.13 + j * 0.015, j * 0.01, 0.004 + j * 0.002), kagit, rot=(0, 0, j * 0.06))
    for j in range(6):
        K("Satir", (0.14, 0.003, 0.0006), (0.17, 0.10 - j * 0.02, 0.0115), murekkep, rot=(0, 0, 0.18))
    grup(b, rot=(0, 0, -0.08), kaydir=(0.05, -0.18, T))
    # Cam kül tablası: oyuk, üç sigara yuvası, kül ve izmaritler.
    kt = S("KulTablasi", 0.07, 0.03, (-0.10, 0.12, T + 0.015), kul_cam, kose=32)
    ortak.oy(kt, ortak.silindir("Oyuk", 0.055, 0.03, (-0.10, 0.12, T + 0.021), kul_cam, 32))
    for a in (0.3, 2.4, 4.5):
        ortak.oy(kt, ortak.kutu("Yuva", (0.03, 0.012, 0.02), (-0.10 + 0.065 * math.cos(a), 0.12 + 0.065 * math.sin(a), T + 0.03),
                                kul_cam))
    S("Kul", 0.052, 0.003, (-0.10, 0.12, T + 0.0075), kul, kose=24)
    for i, (dx, dy, a) in enumerate(((0.01, 0.02, 0.4), (-0.02, -0.01, 1.9), (0.02, -0.025, 2.8), (-0.015, 0.025, 4.0))):
        b = len(p)
        x_silindir(0.0045, 0.025, (0.0125, 0, 0.0045), kagit, kose=8)
        x_silindir(0.0046, 0.012, (-0.006, 0, 0.0045), filtre, kose=8)
        grup(b, rot=(0, 0, a), kaydir=(-0.10 + dx, 0.12 + dy, T + 0.009))
    cubuk((0.35, -0.05, T + 0.004), (0.50, -0.02, T + 0.004), 0.004, ortak.malzeme("K8_Kalem", (0.60, 0.40, 0.03), 0.5), kose=6)
    ad, h, uzak, kaldir = "Toplanti_Masasi_K8", 0.9, 3.0, 0.0

elif hangi == "toplantisandalye":
    kumas = ortak.malzeme("K8_KumasBordo", (0.16, 0.025, 0.025), puruz=0.9)
    for x in (-0.20, 0.20):
        K("OnAyak", (0.04, 0.04, 0.44), (x, -0.20, 0.22), maun, 0.005)
        K("ArkaAyak", (0.04, 0.04, 0.44), (x, 0.21, 0.22), maun, 0.005)
        K("ArkaUst", (0.04, 0.035, 0.47), (x, 0.225, 0.665), maun, 0.005, rot=(-0.07, 0, 0))
        K("YanKusak", (0.025, 0.37, 0.06), (x, 0.005, 0.40), maun, 0.004)
        K("AltKusak", (0.02, 0.37, 0.025), (x, 0.005, 0.12), maun, 0.003)
    for y in (-0.20, 0.21):
        K("Kusak", (0.36, 0.025, 0.06), (0, y, 0.40), maun, 0.004)
    K("OnGergi", (0.36, 0.02, 0.025), (0, -0.20, 0.12), maun, 0.003)
    K("Minder", (0.46, 0.44, 0.07), (0, -0.01, 0.465), kumas, 0.03)
    K("Arkalik", (0.34, 0.05, 0.28), (0, 0.225, 0.70), kumas, 0.02, rot=(-0.07, 0, 0))
    K("TepeKusak", (0.44, 0.035, 0.05), (0, 0.24, 0.875), maun, 0.006, rot=(-0.07, 0, 0))
    ad, h, uzak, kaldir = "Toplanti_Sandalyesi_K8", 0.9, 1.6, 0.0

elif hangi == "evrak":
    acik = True
    EN, DER, H = 0.80, 0.45, 0.75
    boya = ortak.malzeme("K8_EvrakDolabiBoya", (0.24, 0.25, 0.23), puruz=0.45, metal=0.4)
    # Önü açık gövde: iki yan, üst, alt (0,05'te), arka, iki çekmeceyi ayıran ray (üstü 0,40).
    p = ac.govde_levhalari(EN, DER, H, boya, taban=0.05, raflar=(0.40,))
    p.append(ortak.kutu("Kaide", (EN - 0.04, DER - 0.06, 0.05), (0, 0.01, 0.025), siyah_metal))
    for i, c in enumerate(((0.05, 0.12, 0.30), (0.55, 0.42, 0.20))):          # üstünde iki dosya
        K("UstDosya", (0.25, 0.33, 0.008), (-0.18 + i * 0.02, 0.0, H + 0.004 + i * 0.008),
          ortak.malzeme("K8_EDosya%d" % i, c, puruz=0.8), rot=(0, 0, 0.05 - i * 0.12))
    y_on = -DER / 2

    def cekmece_yap(ad_, z_alt, boy):
        """acilir_ortak.cekmece + etiket çerçevesi ve kilit. Pivot iç tabanın ortası (değişmez)."""
        c = ac.cekmece(ad_, 0, z_alt, EN - 0.04, boy, DER - 0.06, y_on, boya, boya, krom)
        zk = z_alt + boy * 0.78
        ek = [ortak.kutu("EtiketCerceve", (0.10, 0.003, 0.035), (0, y_on - 0.0015, zk), krom),
              ortak.kutu("EtiketKart", (0.085, 0.001, 0.024), (0, y_on - 0.0035, zk), kagit),
              ortak.kutu("EtiketYazi", (0.05, 0.0006, 0.004), (0, y_on - 0.0042, zk), murekkep)]
        bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=0.009, depth=0.006, location=(0.30, y_on - 0.003, z_alt + boy - 0.03),
                                            rotation=(math.pi / 2, 0, 0))
        kl = bpy.context.active_object
        bpy.ops.object.transform_apply(rotation=True)
        ortak.ata(kl, krom)
        ek.append(kl)
        return ortak.birlestir([c] + ek, ad_)

    parcalar.append(cekmece_yap("Cekmece_1", 0.41, 0.31))    # üst çekmece: iç tabanı z = 0.43
    parcalar.append(cekmece_yap("Cekmece_2", 0.075, 0.30))   # alt çekmece: iç tabanı z = 0.095
    ad, h, uzak, kaldir = "Evrak_Dolabi_K8", 0.8, 1.5, 0.0

elif hangi == "tepegoz":
    govde_m = ortak.malzeme("K8_TepegozGovde", (0.32, 0.31, 0.26), puruz=0.45)
    cam_k = ortak.malzeme("K8_TepegozCam", (0.12, 0.14, 0.14), puruz=0.03)
    ayna = ortak.malzeme("K8_TepegozAyna", (0.50, 0.52, 0.52), puruz=0.05, metal=1.0)
    asetat = ortak.malzeme("K8_Asetat", (0.55, 0.60, 0.58), puruz=0.1)
    # Tekerlekli sehpa: dört kare boru dikme, iki raf.
    for x in (-0.26, 0.26):
        for y in (-0.21, 0.21):
            K("Dikme", (0.025, 0.025, 0.57), (x, y, 0.345), ofis_gri, 0.003)
            K("Catal", (0.02, 0.02, 0.03), (x, y, 0.05), siyah_metal)
            kure(0.025, (x, y, 0.025), lastik, olcek=(0.7, 1, 1))
    T = 0.63
    K("UstRaf", (0.58, 0.48, 0.02), (0, 0, T - 0.01), ofis_gri, 0.004)
    K("AltRaf", (0.56, 0.46, 0.02), (0, 0, 0.18), ofis_gri, 0.004)
    # Tepegöz: gövde, sahne camı, havalandırma yarıkları, arka direk, kol, başlık (mercek + eğik ayna).
    K("PGovde", (0.36, 0.36, 0.13), (0, -0.02, T + 0.065), govde_m, 0.015)
    K("Sahne", (0.29, 0.29, 0.004), (0, -0.02, T + 0.131), cam_k)
    for i in range(6):
        K("Yarik", (0.20, 0.002, 0.005), (0, -0.201, T + 0.03 + i * 0.012), delik)
    K("Direk", (0.035, 0.035, 0.30), (0.12, 0.14, T + 0.28), siyah_metal, 0.004)
    dik_disk(0.018, 0.02, (0.12, 0.12, T + 0.30), siyah_metal, kose=14)
    cubuk((0.12, 0.14, T + 0.42), (0.03, -0.02, T + 0.42), 0.012, siyah_metal)
    K("Baslik", (0.12, 0.10, 0.07), (0, -0.02, T + 0.42), govde_m, 0.01)
    S("Mercek", 0.045, 0.04, (0, -0.02, T + 0.37), siyah_metal, kose=20)
    b = len(p)
    K("Ayna", (0.11, 0.004, 0.10), (0, -0.075, T + 0.43), ayna)
    grup(b, pivot=(0, -0.07, T + 0.43), rot=(-0.78, 0, 0))
    K("Tutamak", (0.02, 0.10, 0.02), (-0.19, -0.02, T + 0.09), siyah_metal, 0.005)
    cubuk((0.0, 0.16, T + 0.03), (0.05, 0.25, T - 0.05), 0.004, lastik)       # kablo aşağı sarkar
    cubuk((0.05, 0.25, T - 0.05), (0.08, 0.26, 0.30), 0.004, lastik)
    # Alt rafta asetat kutusu ve sarılmış uzatma kablosu.
    K("AsetatKutu", (0.26, 0.22, 0.06), (-0.10, 0.02, 0.22), karton2, 0.004)
    K("Asetat", (0.21, 0.297, 0.01), (-0.10, 0.02, 0.255), asetat, rot=(0, 0, 0.1))
    halka(0.06, 0.008, (0.14, 0.05, 0.198), lastik, seg=(20, 6))
    ad, h, uzak, kaldir = "Tepegoz_K8", 1.1, 1.9, 0.0

elif hangi == "tahta":
    tahta = ortak.malzeme("K8_KaraTahta", (0.018, 0.035, 0.028), puruz=0.9)
    tebesir = ortak.malzeme("K8_Tebesir", (0.55, 0.55, 0.52), puruz=0.95)
    soluk = ortak.malzeme("K8_TebesirSoluk", (0.20, 0.22, 0.21), puruz=0.95)
    leke = ortak.malzeme("K8_TahtaLeke", (0.026, 0.044, 0.036), puruz=1.0)
    kece = ortak.malzeme("K8_SilgiKece", (0.08, 0.08, 0.07), puruz=1.0)
    EN, BOY = 1.20, 0.80
    K("Tahta", (EN, 0.02, BOY), (0, -0.01, 0), tahta)                        # önü y = -0.02
    for z in (BOY / 2 + 0.0175, -BOY / 2 - 0.0175):
        K("Cerceve", (EN + 0.07, 0.03, 0.035), (0, -0.015, z), maun_ac, 0.004)
    for x in (EN / 2 + 0.0175, -EN / 2 - 0.0175):
        K("Cerceve", (0.035, 0.03, BOY), (x, -0.015, 0), maun_ac, 0.004)
    K("Oluk", (1.18, 0.08, 0.012), (0, -0.04, -0.441), maun_ac, 0.003)       # tebeşirlik
    K("OlukDudak", (1.18, 0.008, 0.02), (0, -0.078, -0.431), maun_ac, 0.002)
    for i, x in enumerate((-0.35, -0.28, 0.12)):
        b = len(p)
        x_silindir(0.005, 0.07 - i * 0.02, (0, 0, 0.005), tebesir, kose=8)
        grup(b, rot=(0, 0, 0.2 * i), kaydir=(x, -0.045, -0.435))
    K("SilgiSirt", (0.13, 0.05, 0.02), (0.30, -0.045, -0.425), maun_ac, 0.004)
    K("SilgiKece", (0.13, 0.05, 0.012), (0.30, -0.045, -0.429), kece)
    yl, yc = -0.0202, -0.0206                                                 # leke ve tebeşir düzlemleri
    for (x, z, w, hh, a) in ((-0.30, 0.10, 0.40, 0.18, 0.05), (0.10, -0.15, 0.50, 0.14, -0.03), (0.35, 0.22, 0.30, 0.12, 0.1)):
        K("Leke", (w, 0.0004, hh), (x, yl, z), leke, rot=(0, a, 0))           # silinmiş tebeşir bulutu
    # Çetele: dörder çizgi ve çaprazı (üç tam grup, bir yarım).
    for g in range(4):
        x0 = -0.52 + g * 0.08
        n = 4 if g < 3 else 2
        for j in range(n):
            cizgi_xz((x0 + j * 0.012, 0.25), (x0 + j * 0.012 + rnd.uniform(-0.004, 0.004), 0.32), yc, 0.005, tebesir)
        if g < 3:
            cizgi_xz((x0 - 0.008, 0.26), (x0 + 0.045, 0.31), yc - 0.0002, 0.005, tebesir)
    # Kat şeması: üst üste kutular (katlar) ve yanında ok; yazı yerine kısa çizgiler.
    for k in range(5):
        z = -0.30 + k * 0.07
        for (a, c) in (((-0.50, z), (-0.36, z)), ((-0.50, z), (-0.50, z + 0.07)), ((-0.36, z), (-0.36, z + 0.07))):
            cizgi_xz(a, c, yc, 0.004, soluk if k < 3 else tebesir)
        cizgi_xz((-0.33, z + 0.035), (-0.27, z + 0.035), yc, 0.003, soluk)
    cizgi_xz((-0.50, 0.05), (-0.36, 0.05), yc, 0.004, tebesir)
    cizgi_xz((-0.25, -0.28), (-0.25, 0.02), yc, 0.004, tebesir)               # yukarı ok
    cizgi_xz((-0.25, 0.02), (-0.27, -0.01), yc, 0.004, tebesir)
    cizgi_xz((-0.25, 0.02), (-0.23, -0.01), yc, 0.004, tebesir)
    for j in range(4):
        cizgi_xz((-0.10, 0.18 - j * 0.04), (-0.10 + rnd.uniform(0.08, 0.16), 0.18 - j * 0.04), yc, 0.003, soluk)
    # İri "9": halka (18 parça) + aşağı inip sola kıvrılan kuyruk (eğri).
    cx, cz, R = 0.28, 0.10, 0.10
    for i in range(18):
        a0, a1 = 2 * math.pi * i / 18, 2 * math.pi * (i + 1) / 18
        cizgi_xz((cx + R * math.cos(a0), cz + R * math.sin(a0)), (cx + R * math.cos(a1), cz + R * math.sin(a1)), yc, 0.02, tebesir)
    P0, P1, P2 = (cx + R, cz), (cx + R, cz - 0.30), (cx - R * 0.6, cz - 0.28)
    onceki = P0
    for i in range(1, 11):
        t = i / 10
        q = ((1 - t) ** 2 * P0[0] + 2 * (1 - t) * t * P1[0] + t * t * P2[0], (1 - t) ** 2 * P0[1] + 2 * (1 - t) * t * P1[1] + t * t * P2[1])
        cizgi_xz(onceki, q, yc, 0.02, tebesir)
        onceki = q
    ad, h, uzak, kaldir = "Kara_Tahta_K8", 1.0, 1.9, 0.5

else:
    raise SystemExit("bilinmeyen eşya: " + hangi)


# ---------- BİTİR: birleştir, pivotu koy, kaydet, dışa aktar, önizle ----------
if acik:
    govde = ortak.birlestir(p, "Govde")
    ortak.yumusat(govde, 35)
    ortak.pivotu_tasi(govde, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, uzak, onizlemede_kaldir=kaldir)
    if "--onizleme" in sys.argv:
        # İkinci önizleme: kapı açık / çekmeceler çekilmiş hâli (yalnızca görüntü; dışa aktarım bitti).
        yol = sys.argv[sys.argv.index("--onizleme") + 1]
        for n in parcalar:
            if n.name.startswith("Kapi"):
                n.rotation_euler.z = math.radians(-105)
            elif n.name.startswith("Cekmece"):
                n.location.y -= 0.28 if n.name.endswith("1") else 0.0
        bpy.context.scene.render.filepath = yol[:-4] + "_acik.png"
        bpy.ops.render.render(write_still=True)
else:
    nesne = ortak.birlestir(p, ad)
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=kaldir)
