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
#   buzdolabi    Buzdolabi_K5        : 80'lerin sonu sararmış emaye buzdolabı; yuvarlak köşeli, ayrı dondurucu kapılı (açılmaz).
#   ocak         Ocak_K5             : krem ocak, 3 gaz + 1 elektrik gözü, cam kapağı kalkık.
#   masa         Mutfak_Masasi_K5    : sarı formika üstlü, alüminyum kenarlı mutfak masası.
#   tabure       Tabure_K5           : aynı takımdan sarı formika tabure.
#   sepet        Ekmek_Sepeti_K5     : hasır ekmek sepeti, içinde bayat yarım ekmek.
# BANYO
#   camasir      Camasir_Makinesi_K5 : üstten yüklemeli, yarı otomatik (çift kazanlı) beyaz-mavi çamaşır makinesi.
#   havlu        Havlu_Askisi_K5     : çift çubuklu havlu askısı, iki havlu.
#   ecza         Banyo_Dolabi_K5     : bizote aynalı boyalı sac banyo dolabı, ayna lambası; altında kolonyalı, paslı krom cam raf.
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


def halka_loft(ad, halkalar, mat, kose=20, us=3.0, kivrim=None, alt=True, ust=True):
    """
    LOFT (halkalardan örme): üstten alta sıralı YATAY halkaları yüzlerle birbirine bağlar; mont gövdesi, kol.
    halkalar: (cx, cy, z, yarı_en_x, yarı_derinlik_y) listesi, z yukarıdan aşağı.
    Her halka bir SÜPER ELİPS: |x/en|^us + |y/der|^us = 1. us = 2 düz elips; us büyüdükçe köşeleri
    yuvarlatılmış dikdörtgene yaklaşır (boş asılı bir mont yassı ama köşesiz durur).
    kivrim(xr, z): halka merkezine göre x'teki (xr) noktayı y'de ne kadar kaydıracağımız; kumaşın
    dikey kıvrımları. Ön ve arka yüz BİRLİKTE kayar, çünkü kıvrılan kumaşın iki yüzü de birlikte kıvrılır.
    """
    import bmesh                                   # yüz yönlerini düzeltmek için Blender'ın mesh aracı
    v, f = [], []                                  # köşe noktaları ve yüzler
    e = 2.0 / us                                   # süper elipsin açı üssü (us = 2 ise 1: düz çember)
    for (cx, cy, z, w, d) in halkalar:
        for i in range(kose):
            a = 2 * math.pi * i / kose             # halkanın etrafında eşit açılı adımlar
            c, s = math.cos(a), math.sin(a)
            x = cx + w * math.copysign(abs(c) ** e, c)   # işareti koruyarak üs al: köşe yuvarlaklığı
            y = cy + d * math.copysign(abs(s) ** e, s)
            if kivrim:
                y += kivrim(x - cx, z)             # kumaş kıvrımı: aynı x'teki ön ve arka nokta birlikte kayar
            v.append((x, y, z))
    for j in range(len(halkalar) - 1):             # her iki komşu halka arasına dörtgen şerit
        for i in range(kose):
            a0, a1 = j * kose + i, j * kose + (i + 1) % kose
            f.append((a0, a1, a1 + kose, a0 + kose))
    if ust:
        f.append(tuple(range(kose)))               # en üst halkayı kapat
    if alt:
        s0 = (len(halkalar) - 1) * kose
        f.append(tuple(reversed(range(s0, s0 + kose))))   # en alt halkayı kapat (ters sıra: dışa baksın)
    m = bpy.data.meshes.new(ad)
    m.from_pydata(v, [], f)
    m.update()
    bm = bmesh.new()                               # yüz yönlerini tutarlı biçimde DIŞA çevir:
    bm.from_mesh(m)                                # Unity arka yüzleri çizmez, içe bakan yüz görünmez olur
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(m)
    bm.free()
    o = bpy.data.objects.new(ad, m)
    bpy.context.collection.objects.link(o)
    ortak.ata(o, mat)
    p.append(o)
    return o


def ara_halka(halkalar, z):
    """Loft halkaları arasında z yüksekliğindeki kesiti doğrusal ara değerle bulur: (cx, cy, en, der)."""
    for h0, h1 in zip(halkalar, halkalar[1:]):
        if h1[2] <= z <= h0[2]:                    # z bu iki halkanın arasında mı (liste yukarıdan aşağı)
            t = (h0[2] - z) / max(1e-6, h0[2] - h1[2])
            return tuple(h0[k] + (h1[k] - h0[k]) * t for k in (0, 1, 3, 4))
    h = halkalar[0] if z > halkalar[0][2] else halkalar[-1]   # aralık dışı: en yakın uçtaki halka
    return (h[0], h[1], h[3], h[4])


def on_y(halkalar, xr, z, us=3.0, kivrim=None):
    """Loft gövdenin ÖN yüzünün (−y) o noktadaki y değeri: cep, fermuar, yaka ucu bu yüzeye oturur."""
    cx, cy, w, d = ara_halka(halkalar, z)
    oran = max(0.0, 1 - abs(xr / w) ** us) ** (1 / us)   # süper elipste merkezden xr uzaktaki derinlik oranı
    return cy - d * oran + (kivrim(xr, z) if kivrim else 0.0)


def perde_kanadi(ad, x0, x1, z_ust, boy, y0, genlik, dalga, mat, kal=0.004, bas=0.5, etek=1.12, tohum=0.0, sutun=16):
    """
    TEK PARÇA pileli perde kanadı. Eski 'pilili' zikzak yaptığı için keskin kırışıklar ayrı ayrı çıtalar gibi
    görünüyordu; burada kıvrımlar yumuşak bir SİNÜS dalgası ve bütün kanat tek bir kesintisiz yüzey.
      x0-x1 : kanadın genişliği (x), z_ust: üst kenarı, boy: aşağı sarkma, y0: kıvrımların ortalama düzlemi.
      genlik: kıvrımın öne-arkaya yarı derinliği; dalga: bir kıvrımın genişliği (m).
      bas   : üst şeritte (büzgülü başlık) kıvrım genliğinin oranı: kumaş rayda sıkı toplanır.
      etek  : alt kenarda genlik oranı: ağırlıkla hafif açılır.
      tohum : kanattan kanada kıvrımların düzensizliği farklı olsun diye faz kaydırması.
      sutun : bir dalgadaki köşe sayısı. Az olursa (ör. 8) komşu yüzler arası açı yumuşatma sınırını
              (35°) aşar ve kıvrımın tepesi yine keskin kırık gibi görünür; çok olursa üçgen sayısı şişer.
    Üstte başlık bandı, altta etek bandı ayrı ince şeritler: kumaşın 2,5 mm önünde, aynı kıvrımı izler;
    kenar çizgileri dikiş gibi okunur. Döndürdüğü liste: rayın makara/halka takılacak x konumları.
    """
    genislik = x1 - x0
    nd = max(1, round(genislik / dalga))           # TAM SAYI kıvrım: iki kenar aynı fazda biter
    k = 2 * math.pi * nd / genislik                # sinüsün açısal sıklığı (radyan / metre)

    def genl(u, d):
        """u: kanat solundan uzaklık, d: üst kenardan aşağı uzaklık; o noktadaki kıvrım genliği."""
        t = min(1.0, d / 0.30)
        yumusak = t * t * (3 - 2 * t)              # 'smoothstep': 0'dan 1'e kırıksız geçiş
        ust_kisim = bas + (1 - bas) * yumusak      # başlıkta sıkı, 30 cm aşağıda tam genlik
        alt_kisim = 1 + (etek - 1) * (d / boy) ** 2   # aşağı indikçe hafif açılma
        duzensiz = 1 + 0.12 * math.sin(k * u / 3.7 + tohum) + 0.05 * math.sin(k * u / 1.9 + 2 * tohum)
        return genlik * ust_kisim * alt_kisim * duzensiz   # her kıvrım biraz farklı derin: el yapımı kumaş

    def yy(x, d):
        """Kumaş yüzeyinin y değeri. Kıvrımlar aşağı indikçe fazı hafifçe kayar (kumaş dimdik inmez)."""
        u = x - x0
        kayma = 0.35 * (d / boy) * math.sin(k * u / 5.3 + 1.3 * tohum)
        return y0 - genl(u, d) * math.sin(k * u + kayma)

    def etek_d(u):
        """Alt kenar dümdüz değil: 0-6 mm arası yukarı dalgalanır (yalnız yukarı: ölçü kutusu büyümesin)."""
        return boy - 0.006 * (1 + math.sin(k * u / 2.3 + tohum)) / 2

    ns = nd * sutun                                 # x boyunca aralık sayısı
    # Köşeler eşit aralıklı değil: kıvrımın TEPESİNDE (eğrilik en büyük) sık, yan yüzlerinde (düz iniş) seyrek.
    # θ = φ + 0.2·sin(2φ): φ eşit adımlı, θ tepelerde (π/2, 3π/2 ...) yavaşlar. Aynı yumuşaklık daha az üçgenle.
    xs = []
    for i in range(ns + 1):
        fi = 2 * math.pi * nd * i / ns
        xs.append(x0 + (fi + 0.2 * math.sin(2 * fi)) / k)

    def yuzey(ad_, dler, ofset, kalinlik):
        """dler: üstten aşağı satır derinlikleri (None = etek kenarı). ofset: kumaşın önüne kaydırma (m)."""
        n = len(xs)
        nokta, yuz = [], []
        for dd in dler:
            for x in xs:
                d = etek_d(x - x0) if dd is None else dd
                y = yy(x, d)
                # Yüzeyin odaya bakan normali (xy düzleminde): eğim türevden bulunur, (eğim, −1) yönü.
                egim = (yy(x + 0.001, d) - yy(x - 0.001, d)) / 0.002
                boy_n = math.sqrt(1 + egim * egim)
                nokta.append((x + ofset * egim / boy_n, y - ofset / boy_n, z_ust - d))
        for r_ in range(len(dler) - 1):
            for i in range(n - 1):
                a = r_ * n + i
                yuz.append((a, a + n, a + n + 1, a + 1))   # bu köşe sırası yüzün normalini odaya (−y) çevirir
        m = bpy.data.meshes.new(ad_)
        m.from_pydata(nokta, [], yuz)
        m.update()
        o = bpy.data.objects.new(ad_, m)
        bpy.context.collection.objects.link(o)
        ortak.ata(o, mat)
        if kalinlik > 0:
            sol = o.modifiers.new("Kalinlik", "SOLIDIFY")   # kalınlık arkaya (+y) doğru eklenir
            sol.thickness = kalinlik
        p.append(o)
        return o

    # Ana kumaş: satırlar başlıkta sık (orada genlik hızla değişiyor), gövdede seyrek (kumaş düz iniyor);
    # etek bandının hizasında (boy − 5 cm) bir satır daha: band kumaşı birebir izlesin, içine girmesin.
    yuzey(ad, [0.0, 0.08, 0.30, 0.55 * boy, boy - 0.05, None], 0.0, kal)
    yuzey(ad + "_Baslik", [0.0, 0.06], 0.0025, 0.0)             # büzgülü başlık bandı
    yuzey(ad + "_Etek", [boy - 0.045, None], 0.0025, 0.0)       # kıvrılıp dikilmiş etek bandı
    # Makara/halka yerleri: her kıvrımın duvara en yakın (arka) tepesi ve iki kenar.
    return [x0] + [x0 + (2 * math.pi * j + 1.5 * math.pi) / k for j in range(nd)] + [x1]


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
    reflektor = ortak.malzeme("K5_Reflektor", (0.45, 0.45, 0.40), puruz=0.3)
    # Sol kancada lacivert iş montu, ENSESİNDEKİ askı ilmeğinden asılı. Boş bir mont kancaya asılınca:
    # yaka kancanın hemen altında toplanır, omuzlar dik bir eğimle düşer, gövde aşağıda genişler,
    # kollar iki yanda hafif öne düşerek sarkar, etek duvara doğru yaslanır.
    # Gövde yatay halkalardan örülür (halka_loft): her satır (z, yarı en, arka yüz y, ön yüz y).
    mx = -0.20                                   # kancanın x'i: montun ortası
    GOVDE = [(-0.050, 0.075, -0.060, -0.110),    # boyun: yakanın içinde kalır
             (-0.075, 0.120, -0.050, -0.120),    # omuz başlangıcı: dik eğim
             (-0.110, 0.175, -0.042, -0.128),
             (-0.150, 0.205, -0.036, -0.135),    # omuz ucu (kolun başladığı yer)
             (-0.200, 0.215, -0.032, -0.140),
             (-0.320, 0.215, -0.027, -0.143),    # göğüs: en dolgun yer
             (-0.450, 0.210, -0.024, -0.139),
             (-0.600, 0.212, -0.021, -0.133),
             (-0.775, 0.222, -0.020, -0.124),    # etek bandının üstü (hafif açılır)
             (-0.790, 0.221, -0.020, -0.122),
             (-0.808, 0.216, -0.021, -0.119)]    # etek kenarı biraz içe kıvrık

    def kivrim(xr, z):
        """Göğüsten aşağı giderek belirginleşen dikey kumaş kıvrımları (en fazla 6 mm)."""
        g = min(1.0, max(0.0, (-z - 0.28) / 0.45))
        return 0.006 * g * math.sin(xr * 2 * math.pi / 0.15 + 0.7)

    # Satırları loft halkalarına çevir: merkez y ön ile arkanın ortası, yarı derinlik farkın yarısı.
    # Etek hafifçe sağa kaçar (cx aşağı indikçe 1 cm kayar): hiçbir mont kancada tam simetrik durmaz.
    hg = [(mx + 0.01 * (z / -0.8) ** 2, (arka + on) / 2, z, w, (arka - on) / 2) for (z, w, arka, on) in GOVDE]
    halka_loft("Mont", hg, lacivert, kose=20, us=3.0, kivrim=kivrim)
    # Yaka: boynun etrafında gövdeden biraz büyük, yatık bir bant; tepesi kancanın koluna değer.
    halka_loft("Yaka", [(mx, -0.085, -0.030, 0.080, 0.030), (mx, -0.086, -0.045, 0.096, 0.036),
                        (mx, -0.087, -0.068, 0.114, 0.043)], lacivert, kose=20, us=2.6)
    # Askı ilmeği: yakanın içinden kancaya çıkan V biçimli şerit.
    for s in (-1, 1):
        cubuk((mx + s * 0.008, -0.066, -0.040), (mx, -0.071, -0.022), 0.003, lacivert, 6)
    # Yaka uçları: göğsün üstüne yatan iki sivri kanat, dışa doğru eğik.
    for s in (-1, 1):
        KR((0.06, 0.008, 0.05), (mx + s * 0.048, on_y(hg, s * 0.048, -0.090) - 0.006, -0.090), lacivert,
           (math.radians(8), s * 0.45, 0), 0.002)
    # Kollar: omuz ucundan başlar (ilk halka gövdenin içinde), dirseğe doğru hafif dışa, bileğe doğru öne düşer.
    # Bilekte lastikli manşet: kısa, biraz daha dar bir bölüm; en alttaki küçük halka kolun ağzını kapatır.
    kollar = []
    for s in (-1, 1):
        kol = [(mx + s * 0.170, -0.087, -0.130, 0.045, 0.045),
               (mx + s * 0.205, -0.090, -0.180, 0.052, 0.050),
               (mx + s * 0.228, -0.095, -0.300, 0.050, 0.047),
               (mx + s * 0.240, -0.100, -0.450, 0.047, 0.045),
               (mx + s * 0.242, -0.106, -0.600, 0.044, 0.042),
               (mx + s * 0.239, -0.110, -0.690, 0.042, 0.040),
               (mx + s * 0.238, -0.111, -0.697, 0.039, 0.037),   # manşet başlangıcı: kumaş içe büzülür
               (mx + s * 0.237, -0.112, -0.735, 0.038, 0.036),
               (mx + s * 0.237, -0.112, -0.740, 0.030, 0.028)]
        halka_loft("Kol", kol, lacivert, kose=12, us=2.4)
        kollar.append(kol)
    # Reflektör şerit: gövdeyi ve iki kolu aynı yükseklikte saran gri bant. Gövdeden 3 mm büyük halkalar,
    # aynı kıvrım fonksiyonuyla: şerit kumaşın üstüne yapışık durur, havada kalmaz.
    SZ = (-0.385, -0.415)
    halka_loft("Serit", [(*ara_halka(hg, z)[:2], z, ara_halka(hg, z)[2] + 0.003, ara_halka(hg, z)[3] + 0.003)
                         for z in SZ], reflektor, kose=20, us=3.0, kivrim=kivrim)
    for kol in kollar:
        halka_loft("KolSerit", [(*ara_halka(kol, z)[:2], z, ara_halka(kol, z)[2] + 0.003, ara_halka(kol, z)[3] + 0.003)
                                for z in SZ], reflektor, kose=12, us=2.4)
    # Fermuar: ön ortada, yakadan eteğe; ön yüzeyi izleyen kısa çubuklardan bir zincir, üstte çekme dili.
    zler = [-0.075 - i * 0.06 for i in range(13)]
    for za, zb in zip(zler, zler[1:]):
        xa, xb = ara_halka(hg, za)[0], ara_halka(hg, zb)[0]
        cubuk((xa, on_y(hg, 0.0, za, 3.0, kivrim) - 0.002, za), (xb, on_y(hg, 0.0, zb, 3.0, kivrim) - 0.002, zb), 0.0035, fermuar, 6)
    K((0.012, 0.005, 0.03), (mx, on_y(hg, 0.0, -0.10, 3.0, kivrim) - 0.007, -0.10), fermuar, 0.001)

    def on_plaka(xr, z, en, boy, mat, ek=0.003):
        """Ön yüzeye oturan düz plaka (cep, kapak). Yüzey kıvrımlı olduğu için plakanın arka yüzü yüzeyin
        en geride kalan noktasına, ön yüzü en önde kalan noktasının ek kadar önüne konur: hiç gömülmez, havada kalmaz."""
        cx = ara_halka(hg, z)[0]
        ys = [on_y(hg, xr + en * (t / 4 - 0.5), z + boy * (q - 0.5), 3.0, kivrim) for t in range(5) for q in (0, 0.5, 1)]
        arka, on = max(ys) + 0.001, min(ys) - ek
        K((en, arka - on, boy), (cx + xr, (arka + on) / 2, z), mat, 0.002)
        return on                                 # plakanın ön yüzü: üstüne düğme/çıtçıt oturtmak için

    # Cepler: altta iki büyük yama cep ve kapakları (çıtçıtlı), göğüste solda küçük bir cep.
    for s in (-1, 1):
        on_plaka(s * 0.105, -0.615, 0.13, 0.14, lacivert, 0.004)
        kap_on = on_plaka(s * 0.105, -0.535, 0.145, 0.05, lacivert, 0.009)
        dik_disk(0.005, 0.003, (ara_halka(hg, -0.54)[0] + s * 0.105, kap_on - 0.001, -0.54), fermuar, 10)   # çıtçıt
    on_plaka(0.095, -0.285, 0.10, 0.11, lacivert, 0.004)
    kap_on = on_plaka(0.095, -0.222, 0.11, 0.04, lacivert, 0.009)
    dik_disk(0.0045, 0.003, (ara_halka(hg, -0.226)[0] + 0.095, kap_on - 0.001, -0.226), fermuar, 10)
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
    # 80'lerin sonu / 90'ların başı yerli buzdolabı (o yılların Arçelik/Beko çizgisi, marka yazısı yok):
    # köşeleri iri yuvarlatılmış emaye gövde, üstte ayrı küçük dondurucu kapısı, altta büyük soğutucu kapısı,
    # her kapının arkasında kaburgalı lastik conta, sağ kenarda uzun dikey kollar, kapı üstünde marka şeridi.
    # Emaye 30 yılda sararmış; alt köşeler paslanmış, kol çevresi el kiri tutmuş. Üstünde dantel örtü,
    # kapısında mıknatıslarla tutturulmuş 1994 takvim yaprağı. Açılmaz, tek parça.
    beyaz = ortak.malzeme("K5_BuzEmaye", (0.74, 0.71, 0.60), puruz=0.32)      # sararmış emaye (doğrusal renk)
    lastik = ortak.malzeme("K5_BuzConta", (0.055, 0.055, 0.05), puruz=0.85)   # kapı contası: mat koyu lastik
    kol_m = ortak.malzeme("K5_BuzKol", (0.55, 0.52, 0.44), puruz=0.4)         # kolun bej plastik gövdesi
    pas = ortak.malzeme("K5_BuzPas", (0.16, 0.06, 0.02), puruz=0.9)           # pas: koyu turuncu-kahve, mat
    demir = ortak.malzeme("K5_BuzDemir", (0.03, 0.028, 0.025), puruz=0.7)     # emaye atınca görünen kararmış sac
    kir = ortak.malzeme("K5_BuzElKiri", (0.38, 0.34, 0.25), puruz=0.7)        # kol çevresinde el kiri
    altin = ortak.malzeme("K5_BuzYaldiz", (0.50, 0.38, 0.12), puruz=0.35, metal=0.9)   # marka "harfleri"
    EN, H = 0.60, 1.69                                # genişlik ve gövde üstünün yüksekliği
    Y_ARKA, Y_ON = 0.31, -0.245                       # gövdenin arka (duvar) ve ön yüzü; eski ölçü kutusu korunur
    # Gövde: bütün kenarları iri yuvarlatılmış (o yılların pres sacı keskin köşe yapmazdı).
    govde = K((EN, Y_ARKA - Y_ON, H - 0.02), (0, (Y_ARKA + Y_ON) / 2, 0.02 + (H - 0.02) / 2), beyaz, 0.035)
    govde.modifiers["Pah"].segments = 4              # 4 bölümlü pah: köşe kırık değil, yumuşak bir kavis olur
    # Ayaklar: önde iki ayarlı siyah ayak, arkada iki sabit takoz (gövde yerden 2 cm yukarıda durur).
    for x in (-EN / 2 + 0.05, EN / 2 - 0.05):
        S(0.016, 0.02, (x, Y_ON + 0.04, 0.01), siyah, 12)                                    # ön vidalı ayak
        K((0.04, 0.04, 0.02), (x, Y_ARKA - 0.05, 0.01), siyah)                               # arka takoz
    # Alt ızgara: kapının altında, gövdenin önünde siyah zeminli, bej çıtalı hava ızgarası.
    K((EN - 0.07, 0.006, 0.075), (0, Y_ON - 0.003, 0.068), siyah, 0.002)
    for i in range(5):
        K((EN - 0.11, 0.006, 0.005), (0, Y_ON - 0.007, 0.042 + i * 0.013), kol_m)
    # Kapılar: (alt z, üst z). Önce conta katmanları, sonra kapı levhası.
    y_kapi = Y_ON - 0.015 - 0.0225                    # kapı levhasının ortası (conta 1,5 cm + kapı 4,5 cm)
    y_yuz = Y_ON - 0.06                               # kapının ön yüzü: rozet, mıknatıs, takvim buraya yapışır
    for (z0, z1) in ((0.115, 1.285), (1.30, H - 0.02)):
        h = z1 - z0                                   # kapının yüksekliği
        # KABURGALI CONTA: üç ince lastik katman, ortadaki daha dar. Yandan bakınca girintili-çıkıntılı
        # koyu bir şerit görünür; gerçek buzdolabı contasının körüklü kesiti böyle okunur.
        for k_, daralma in enumerate((0.022, 0.034, 0.022)):
            K((EN - daralma, 0.005, h - daralma), (0, Y_ON - 0.0025 - k_ * 0.005, (z0 + z1) / 2), lastik, 0.0015)
        kapi = K((EN - 0.006, 0.045, h), (0, y_kapi, (z0 + z1) / 2), beyaz, 0.022)        # kapı levhası
        kapi.modifiers["Pah"].segments = 3            # kapı kenarları da yuvarlak: sac kapı kıvrılarak bükülmüş
    # Menteşe kapakları sol kenarda: üstte, iki kapı arasında, altta (bej plastik başlıklar).
    for z in (H - 0.005, 1.2925, 0.105):
        K((0.05, 0.05, 0.016), (-EN / 2 + 0.04, Y_ON - 0.03, z), kol_m, 0.004)
    # UZUN DİKEY KOLLAR (sağ kenarda): bej plastik tutamak, önünde krom şerit, iki ucunda kapıya vidalı ayak.
    xk = EN / 2 - 0.045                               # kolların x konumu
    for (z0, z1) in ((0.80, 1.25), (1.33, 1.50)):     # soğutucu kolu uzun, dondurucu kolu kısa
        K((0.03, 0.022, z1 - z0), (xk, y_yuz - 0.031, (z0 + z1) / 2), kol_m, 0.006)       # tutamak gövdesi
        K((0.010, 0.004, z1 - z0 - 0.03), (xk, y_yuz - 0.043, (z0 + z1) / 2), krom)      # krom ön şerit
        for z in (z0 + 0.015, z1 - 0.015):
            K((0.026, 0.022, 0.03), (xk, y_yuz - 0.011, z), kol_m, 0.004)                # vidalı ayaklar
        # Kolun alt ucunun altında yılların el kiri: tutamağı kavrayan parmakların bıraktığı koyu iz.
        kure(0.02, (xk - 0.004, y_yuz + 0.0085, z0 + 0.05), kir, (0.9, 0.5, 2.2), seg=10)
    # MARKA ŞERİDİ: soğutucu kapısının üstünde krom çerçeveli siyah şerit, içinde yaldız "harfler"
    # (gerçek marka adı yazmıyoruz: kısa yaldız çubuklar uzaktan yazı gibi okunur) ve küçük kırmızı amblem.
    K((0.30, 0.004, 0.032), (-0.10, y_yuz - 0.002, 1.235), krom, 0.001)                     # krom çerçeve
    K((0.29, 0.005, 0.024), (-0.10, y_yuz - 0.003, 1.235), siyah)                           # siyah zemin
    for i, w in enumerate((0.022, 0.018, 0.024, 0.020, 0.018, 0.022, 0.016)):
        K((w, 0.002, 0.011), (-0.215 + i * 0.033, y_yuz - 0.0065, 1.235), altin)            # yaldız harfler
    K((0.02, 0.003, 0.02), (0.035, y_yuz - 0.006, 1.235), ortak.malzeme("K5_BuzAmblem", (0.45, 0.04, 0.03), puruz=0.4))
    # Dondurucu kapısında "üç yıldız" plakası (o yılların dondurucu sınıfı işareti): her yıldız üç ince çubuk.
    K((0.11, 0.003, 0.04), (-0.18, y_yuz - 0.0015, 1.36), krom, 0.001)
    for j in range(3):
        for a_ in (0, math.pi / 3, 2 * math.pi / 3):  # 60° arayla üç çubuk: altı kollu yıldız
            KR((0.022, 0.002, 0.004), (-0.215 + j * 0.035, y_yuz - 0.004, 1.36), siyah, (0, a_, 0))
    # 1994 TAKVİM YAPRAĞI: kâğıt, kırmızı başlık bandı, ızgara çizgileri; hafif eğik asılmış.
    kagit = ortak.malzeme("K5_Takvim", (0.70, 0.66, 0.52), puruz=0.85)
    cizgi = ortak.malzeme("K5_TakvimCizgi", (0.12, 0.11, 0.10), puruz=0.8)
    tx, tz, eg = -0.10, 0.92, math.radians(-4)        # takvimin merkezi ve eğikliği (y ekseni etrafında)
    KR((0.17, 0.0015, 0.23), (tx, y_yuz - 0.001, tz), kagit, (0, eg, 0))

    def takvim_noktasi(dx, dz):
        """Takvimin kendi düzlemindeki (dx, dz) noktasını, eğiklik verilmiş haliyle dünyaya çevirir."""
        return (tx + dx * math.cos(eg) + dz * math.sin(eg), tz - dx * math.sin(eg) + dz * math.cos(eg))

    x_, z_ = takvim_noktasi(0, 0.095)
    KR((0.17, 0.002, 0.04), (x_, y_yuz - 0.0018, z_), ortak.malzeme("K5_TakvimBaslik", (0.40, 0.04, 0.03), puruz=0.7), (0, eg, 0))
    for r_ in range(6):                               # yatay ızgara çizgileri (hafta satırları)
        x_, z_ = takvim_noktasi(0, 0.06 - r_ * 0.032)
        KR((0.15, 0.002, 0.0018), (x_, y_yuz - 0.0019, z_), cizgi, (0, eg, 0))
    for c_ in range(8):                               # dikey ızgara çizgileri (yedi gün sütunu)
        x_, z_ = takvim_noktasi(-0.075 + c_ * 0.0214, -0.02)
        KR((0.0018, 0.002, 0.16), (x_, y_yuz - 0.0019, z_), cizgi, (0, eg, 0))
    # Mıknatıslar: takvimi tutan kırmızı elma (sapıyla), biraz ötede sarı limon ve yeşil yaprak.
    x_, z_ = takvim_noktasi(0, 0.115)
    kure(0.016, (x_, y_yuz - 0.009, z_), ortak.malzeme("K5_MiknatisElma", (0.42, 0.03, 0.02), puruz=0.35), (1, 0.55, 1))
    K((0.003, 0.004, 0.012), (x_, y_yuz - 0.012, z_ + 0.017), ortak.malzeme("K5_MiknatisSap", (0.08, 0.04, 0.01), puruz=0.6))
    kure(0.014, (0.10, y_yuz - 0.008, 1.08), ortak.malzeme("K5_MiknatisLimon", (0.60, 0.48, 0.04), puruz=0.35), (1.35, 0.55, 1))
    kure(0.012, (0.06, y_yuz - 0.007, 0.70), ortak.malzeme("K5_MiknatisYaprak", (0.06, 0.25, 0.05), puruz=0.35), (0.7, 0.55, 1.5))
    # Yeşil yaprağın tuttuğu küçük bakkal veresiye pusulası.
    KR((0.07, 0.0012, 0.09), (0.06, y_yuz - 0.0008, 0.66), ortak.malzeme("K5_Pusula", (0.66, 0.64, 0.58), puruz=0.9), (0, math.radians(6), 0))
    # AŞINMA: kapıların alt kenarında emaye atmış, ortası kararmış sac, çevresi pas halesi
    # (yer silinirken yıllarca su değmiş, kapı tekmeyle kapatılmış). Her leke tek bir dörtgen değil,
    # rastgele kaydırılmış birkaç yassı diskin üst üste binmesi: kenarı düzensiz, gerçek pas gibi okunur.
    def pas_lekesi(x, z, r):
        """Kapı yüzünde (x, z) çevresinde, yaklaşık r yarıçaplı düzensiz pas lekesi ve ortasında çıplak sac."""
        for _ in range(4):                            # dört pas diski, her biri biraz kayık ve farklı boyda
            dik_disk(r * rnd.uniform(0.5, 0.9), 0.0012,
                     (x + rnd.uniform(-r, r) * 0.6, y_yuz - 0.0006, z + rnd.uniform(-r, r) * 0.4), pas, 10)
        for _ in range(2):                            # iki küçük koyu disk: emayenin tamamen atıp sacın göründüğü yer
            dik_disk(r * rnd.uniform(0.2, 0.35), 0.0018,
                     (x + rnd.uniform(-r, r) * 0.3, y_yuz - 0.0009, z + rnd.uniform(-r, r) * 0.2), demir, 8)
    for (x, z, r) in ((-0.25, 0.16, 0.03), (-0.19, 0.135, 0.016), (0.235, 0.14, 0.024),
                      (0.08, 1.316, 0.014), (-0.04, 0.128, 0.012)):
        pas_lekesi(x, z, r)
    for s_ in (-1, 1):
        # Gövdenin iki yan yüzünde, yere yakın pas akıntıları: aşağı doğru uzamış yassı damlalar.
        for (y_, hh) in ((-0.17, 0.10), (-0.12, 0.06), (0.05, 0.08)):
            kure(0.008, (s_ * (EN / 2 - 0.001), y_, 0.03 + hh / 2), pas, (0.25, 1.0, hh / 0.016), seg=10)
    # Üstte dantel örtü ve ön kenardan sarkan fistolar (Türk evinde buzdolabının üstü hiç çıplak durmaz).
    K((0.42, 0.36, 0.003), (0, 0.02, H + 0.0015), dantel_m)
    for i in range(11):
        K((0.028, 0.003, 0.03), (-0.19 + i * 0.038, Y_ON + 0.012, H - 0.012), dantel_m)
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
    # 90'ların başı banyo dolabı: fırın boyalı sac dolap, önünde kenarı pahlı (bizote) ayna kapak; aynanın
    # gümüşü yıllar içinde kenarlardan içeri doğru kararmış, sol alt köşesi çatlak. Üstünde opal camlı
    # krom ayna lambası ve çekme ipi. Altında krom konsollu cam raf: limon kolonyası, kahverengi cam
    # saç losyonu şişesi, diş fırçalı bardak, tıraş fırçası, sabunluk ve aşınmış sabun. Krom yer yer paslı.
    duvar = True
    boya = ortak.malzeme("K5_EczaBoya", (0.70, 0.67, 0.56), puruz=0.4)       # sararmış krem fırın boyası
    ayna = ortak.malzeme("K5_Ayna", (0.62, 0.63, 0.63), puruz=0.03, metal=1.0)   # sağlam gümüşlü ayna
    kararmis = ortak.malzeme("K5_AynaKararmis", (0.045, 0.038, 0.03), puruz=0.55, metal=0.3)   # gümüşü atmış kenar
    cam_raf = ortak.malzeme("K5_CamRaf", (0.30, 0.42, 0.38), puruz=0.05)      # kalın camın yeşilimsi rengi
    bardak_m = ortak.malzeme("K5_Bardak", (0.30, 0.40, 0.38), puruz=0.1)
    pas = ortak.malzeme("K5_EczaPas", (0.17, 0.065, 0.02), puruz=0.9)
    demir = ortak.malzeme("K5_EczaDemir", (0.03, 0.028, 0.025), puruz=0.7)
    opal = ortak.malzeme("K5_OpalCam", (0.80, 0.78, 0.70), puruz=0.25)        # süt beyazı lamba camı
    EN, DER = 0.50, 0.12                              # dolap gövdesinin eni ve derinliği (arka yüz y = 0)
    Z_ALT, Z_UST = -0.20, 0.26                        # gövdenin alt ve üst kenarı (pivot askı hizasında, z = 0)
    zo, H = (Z_ALT + Z_UST) / 2, Z_UST - Z_ALT        # gövdenin orta yüksekliği ve boyu
    K((EN, DER, H), (0, -DER / 2, zo), boya, 0.006)                                           # sac gövde
    K((EN + 0.02, DER + 0.01, 0.02), (0, -(DER + 0.01) / 2, Z_UST + 0.01), boya, 0.006)       # üst korniş
    K((EN + 0.01, DER + 0.005, 0.012), (0, -(DER + 0.005) / 2, Z_ALT - 0.006), boya, 0.004)   # alt pervaz
    # KAPAK: boyalı ince çerçeve, içinde iki katlı ayna. Alttaki katman kararmış gümüş, kenarları pahlı;
    # üstteki parlak ayna ondan biraz küçük: aradaki koyu şerit "gümüşü kenardan atmış ayna" gibi okunur.
    y_k = -DER                                        # kapağın arka yüzü gövdenin ön yüzüne yaslı
    K((EN - 0.02, 0.018, H - 0.01), (0, y_k - 0.009, zo), boya, 0.004)                        # kapak çerçevesi
    taban = K((EN - 0.06, 0.004, H - 0.06), (0, y_k - 0.020, zo), kararmis, 0.010)           # kararmış alt katman
    taban.modifiers["Pah"].segments = 1               # tek bölümlü pah: yuvarlak değil, düz bizote kesim
    y_ayna = y_k - 0.0225                             # parlak aynanın ön yüzü
    K((EN - 0.10, 0.0012, H - 0.10), (0, y_ayna + 0.0006, zo), ayna)                         # parlak ayna
    # Kararma kenardan içeri sızar: aynanın kenarı boyunca dizilmiş, içeri doğru kaydırılmış küçük koyu
    # diskler; köşelerde daha iri ve sık (nem en çok köşeden girer). Merkezler kenardan içeride tutulur ki
    # lekeler aynanın dışına, boyalı çerçevenin üstüne taşmasın.
    yx, yz = (EN - 0.10) / 2, (H - 0.10) / 2          # parlak aynanın yarı eni ve yarı boyu
    lekeler = []                                      # (x, z, yarıçap) listesi
    for (kx_, kz_) in ((-1, -1), (1, -1), (-1, 1), (1, 1)):          # dört köşe
        for _ in range(4):
            r = rnd.uniform(0.008, 0.020) * (1.4 if kz_ < 0 else 1.0)   # alt köşeler daha çok kararmış
            lekeler.append((kx_ * (yx - r * 0.7 - rnd.uniform(0, 0.02)), zo + kz_ * (yz - r * 0.7 - rnd.uniform(0, 0.02)), r))
    for _ in range(14):                               # kenarlar boyunca serpiştirilmiş küçük lekeler
        r = rnd.uniform(0.004, 0.011)
        if rnd.random() < 0.5:                        # yarısı sol/sağ kenarda
            kx_ = rnd.choice((-1, 1))
            lekeler.append((kx_ * (yx - r * 0.8), zo + rnd.uniform(-yz, yz) * 0.85, r))
        else:                                         # yarısı alt/üst kenarda (altta daha çok)
            kz_ = -1 if rnd.random() < 0.7 else 1
            lekeler.append((rnd.uniform(-yx, yx) * 0.85, zo + kz_ * (yz - r * 0.8), r))
    for (x, z, r) in lekeler:
        dik_disk(r, 0.0008, (x, y_ayna - 0.0002, z), kararmis, 10)
    # Sol alt köşede çatlak: aynanın üstünde iki ince koyu çizgi (birbirinden ayrılan iki kırık hattı).
    for (x, z, uz_, a_) in ((-0.12, zo - 0.13, 0.13, 0.55), (-0.15, zo - 0.10, 0.08, -0.35)):
        KR((uz_, 0.0006, 0.0015), (x, y_ayna - 0.0004, z), siyah, (0, a_, 0))
    # Menteşe (solda, boydan boya krom piyano menteşe) ve kulp (sağ kenarda küçük krom topuz).
    cubuk((-EN / 2 + 0.008, y_k - 0.012, Z_ALT + 0.01), (-EN / 2 + 0.008, y_k - 0.012, Z_UST - 0.01), 0.004, krom)
    dik_disk(0.005, 0.012, (EN / 2 - 0.022, y_k - 0.024, zo), krom, 12)                       # topuzun boynu
    kure(0.008, (EN / 2 - 0.022, y_k - 0.032, zo), krom, (1, 0.7, 1))
    # Boyanın atıp pasın göründüğü yerler: kapak çerçevesinin alt kenarı ve köşeleri (banyonun buharı
    # yıllarca sacın kenarına oturmuş). Tek iri leke değil, kümelenmiş minik kabarcıklar.
    for (x, z, n) in ((-EN / 2 + 0.03, Z_ALT + 0.012, 6), (EN / 2 - 0.035, Z_ALT + 0.011, 4), (0.08, Z_ALT + 0.01, 3),
                      (-EN / 2 + 0.018, Z_ALT + 0.08, 3)):
        for _ in range(n):
            dik_disk(rnd.uniform(0.0015, 0.004), 0.0012,
                     (x + rnd.uniform(-0.015, 0.015), y_k - 0.0186, z + rnd.uniform(-0.004, 0.004)), pas, 8)
    # AYNA LAMBASI: kornişin üstünde krom ayak, opal cam tüp, iki ucunda krom başlık; sağdan sarkan çekme ipi.
    K((0.40, 0.05, 0.016), (0, -0.07, Z_UST + 0.028), krom, 0.003)                            # krom taşıyıcı
    yatay_silindir(0.02, 0.34, (0, -0.075, Z_UST + 0.052), opal, 20)                         # opal cam tüp
    for s_ in (-1, 1):
        yatay_silindir(0.023, 0.018, (s_ * 0.179, -0.075, Z_UST + 0.052), krom, 20)          # uç başlıkları
    cubuk((0.19, -0.095, Z_UST + 0.045), (0.19, -0.158, Z_UST + 0.02), 0.0012, siyah, 6)      # ip başlıktan öne
    cubuk((0.19, -0.158, Z_UST + 0.02), (0.19, -0.158, 0.06), 0.0012, siyah, 6)              # ve aşağı sarkar
    kure(0.006, (0.19, -0.158, 0.054), ortak.malzeme("K5_IpBoncuk", (0.60, 0.55, 0.42), puruz=0.4), (1, 1, 1.6), seg=10)
    # CAM RAF: iki krom konsol (duvar plakası, raf kolu, çapraz destek), kalın cam, önünde krom korkuluk.
    z_raf = -0.33                                     # cam rafın üst yüzü
    K((0.44, 0.11, 0.006), (0, -0.07, z_raf - 0.003), cam_raf, 0.0015)
    for s_ in (-1, 1):
        x = s_ * 0.19
        K((0.022, 0.006, 0.05), (x, -0.003, z_raf - 0.02), krom, 0.002)                       # duvar plakası
        K((0.012, 0.12, 0.007), (x, -0.066, z_raf - 0.0095), krom, 0.002)                    # rafın altındaki kol
        cubuk((x, -0.005, z_raf - 0.04), (x, -0.085, z_raf - 0.012), 0.003, krom)            # çapraz destek
        cubuk((x, -0.128, z_raf), (x, -0.128, z_raf + 0.024), 0.0025, krom)                  # korkuluk dikmesi
    cubuk((-0.205, -0.128, z_raf + 0.024), (0.205, -0.128, z_raf + 0.024), 0.003, krom)       # ön korkuluk
    # Kromun pas noktaları: kaplaması kalkmış küçük kahverengi kabarcıklar.
    for (x, y_, z) in ((-0.12, -0.131, z_raf + 0.024), (0.03, -0.131, z_raf + 0.024), (0.15, -0.131, z_raf + 0.024),
                       (0.19, -0.035, z_raf - 0.03), (-0.19, -0.06, z_raf - 0.026), (-0.19, -0.0065, z_raf - 0.03)):
        kure(0.0035, (x, y_, z), pas, (1.3, 0.8, 1.0), seg=8)
    # 1) Limon kolonyası: yassı köşeli cam şişe, içinde açık sarı kolonya, yaldızlı kapak (Türk evinin simgesi).
    kolonya = ortak.malzeme("K5_Kolonya", (0.55, 0.52, 0.22), puruz=0.06)
    yaldiz_k = ortak.malzeme("K5_KolonyaKapak", (0.55, 0.40, 0.12), puruz=0.3, metal=0.9)
    K((0.052, 0.03, 0.085), (-0.17, -0.07, z_raf + 0.0425), kolonya, 0.008)
    K((0.03, 0.0012, 0.035), (-0.17, -0.0856, z_raf + 0.04), ortak.malzeme("K5_KolonyaEtiket", (0.60, 0.50, 0.25), puruz=0.7))
    S(0.008, 0.014, (-0.17, -0.07, z_raf + 0.092), kolonya, 12)                               # boyun
    S(0.011, 0.018, (-0.17, -0.07, z_raf + 0.108), yaldiz_k, 16, 0.002)                       # kapak
    # 2) Kahverengi cam saç losyonu şişesi: gövde, omuz, boyun, siyah kapak, krem etiket.
    kahve_cam = ortak.malzeme("K5_KahveCam", (0.16, 0.06, 0.015), puruz=0.08)
    S(0.02, 0.07, (-0.105, -0.05, z_raf + 0.035), kahve_cam, 18)
    koni(0.02, 0.008, 0.016, (-0.105, -0.05, z_raf + 0.078), kahve_cam, 18)
    S(0.008, 0.012, (-0.105, -0.05, z_raf + 0.092), kahve_cam, 12)
    S(0.0095, 0.012, (-0.105, -0.05, z_raf + 0.104), siyah, 12)
    S(0.0205, 0.035, (-0.105, -0.05, z_raf + 0.035), ortak.malzeme("K5_LosyonEtiket", (0.62, 0.56, 0.42), puruz=0.7), 18)
    # 3) Bardak ve öne yaslanmış iki eski tip düz saplı diş fırçası (başları kabin önüne taşar).
    bx, by = -0.035, -0.10
    bardak = ortak.silindir("Bardak", 0.027, 0.085, (bx, by, z_raf + 0.0425), bardak_m, 20)
    ortak.oy(bardak, ortak.silindir("BardakIc", 0.023, 0.085, (bx, by, z_raf + 0.048), bardak_m, 20))
    p.append(bardak)
    for dx, renk in ((-0.008, (0.40, 0.05, 0.04)), (0.01, (0.04, 0.10, 0.35))):
        alt = (bx + dx, by, z_raf + 0.008)            # sapın bardak dibindeki ucu
        ust = (bx + dx * 2.5, by - 0.058, z_raf + 0.158)   # sapın öne yaslanmış üst ucu
        cubuk(alt, ust, 0.0035, ortak.malzeme("K5_Firca%d" % int(dx * 1000 + 10), renk, puruz=0.4))
        # Kıl demeti: sapın üst ucunda, sapa paralel duran küçük blok; fırçanın ön (−y) yüzüne bakar.
        # Sap öne doğru θ kadar eğik; blok da aynı açıyla x ekseni etrafında döndürülür.
        teta = math.atan2(ust[1] - alt[1], ust[2] - alt[2]) * -1   # sapın dikeyle yaptığı açı
        kx2, ky2, kz2 = ust[0], ust[1] - 0.006 * math.cos(teta), ust[2] - 0.010 - 0.006 * math.sin(teta)
        KR((0.009, 0.010, 0.024), (kx2, ky2, kz2), ortak.malzeme("K5_FircaKil", (0.62, 0.60, 0.55), puruz=0.9), (teta, 0, 0), 0.002)
    # 4) Tıraş fırçası: fildişi renkli sap, üstünde kabarık porsuk kılı.
    S(0.011, 0.04, (0.04, -0.06, z_raf + 0.02), ortak.malzeme("K5_TirasSap", (0.62, 0.56, 0.42), puruz=0.3), 16, 0.002)
    kure(0.016, (0.04, -0.06, z_raf + 0.056), ortak.malzeme("K5_TirasKil", (0.30, 0.24, 0.17), puruz=0.95), (1, 1, 1.3), seg=12)
    # 5) Sabunluk: oval, içi çukur pastel yeşil plastik; içinde kenarları erimiş bir kalıp sabun.
    sabunluk_m = ortak.malzeme("K5_Sabunluk", (0.30, 0.48, 0.38), puruz=0.35)
    sabunluk = ortak.silindir("Sabunluk", 0.045, 0.014, (0.135, -0.07, z_raf + 0.007), sabunluk_m, 24, 0.003)
    ortak.oy(sabunluk, ortak.silindir("SabunlukIc", 0.039, 0.014, (0.135, -0.07, z_raf + 0.011), sabunluk_m, 24))
    sabunluk.scale = (1.0, 0.7, 1.0)                  # daireyi ovale bas (silindirin pivotu kendi merkezinde)
    p.append(sabunluk)
    K((0.058, 0.034, 0.016), (0.135, -0.07, z_raf + 0.013), ortak.malzeme("K5_Sabun", (0.62, 0.55, 0.38), puruz=0.5), 0.007)
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
    # Kanatlar: soldaki geniş (yarı kapalı, kıvrımları gevşek), sağdaki toplanmış (dar ve sık kıvrımlı).
    # Her kanat TEK PARÇA kumaş (perde_kanadi): yumuşak sinüs kıvrımları, büzgülü başlık, dikişli etek.
    # Raydaki küçük makaralar her kıvrımın duvara yakın tepesinde: kumaş gerçekten oradan asılı.
    for (x0, x1, genlik, dalga, tohum, sutun) in ((-EN / 2 + 0.01, -0.05, 0.020, 0.12, 0.4, 12),
                                                  (EN / 2 - 0.30, EN / 2 - 0.01, 0.018, 0.072, 2.1, 14)):
        makaralar = perde_kanadi("PerdeB", x0, x1, -0.015, BOY, -ARA, genlik, dalga, yesil, 0.004,
                                 tohum=tohum, sutun=sutun)
        for xm in makaralar:
            K((0.008, 0.012, 0.012), (xm, -ARA, -0.012), plastik)
    ad, h, uzak = "Perde_OdaB_K5", 1.6, 2.4
    kaldir = 1.58

# ---------- BİTİR: tek parça, pivot (0,0,0), kaydet, FBX, önizleme ----------
nesne = ortak.birlestir(p, ad)
ortak.yumusat(nesne, 35)
ortak.pivotu_tasi(nesne, (0, 0, 0))
ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=(kaldir if duvar else 0.0))
