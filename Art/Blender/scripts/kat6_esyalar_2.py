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
#   askilik     Elbise_Askisi_K6      : tekerlekli krom askılık; askılarda iki ceket, bir elbise, bir palto ve
#                                       naylon kılıflı uzun bir gece elbisesi.
#   dikiskutu   Dikis_Kutusu_K6       : AÇILIR (Govde + Kapak) kakmalı ahşap dikiş kutusu, içi oyuk.
#   buzdolabi   Buzdolabi_K6          : AÇILIR (Govde + Kapi_1) 1974'ten kalma yuvarlak hatlı, sararmış emaye buzdolabı;
#                                       krom kollu kulp ve el yazısı marka, içte alüminyum buzluk, iki tel raf, sebzelik.
#   puf         Puf_K6                : 70'lerin döşemeli pufu: kilim dokumalı yanlar, düğmeli aşınmış kadife üst, biye,
#                                       kısa torna ayaklar; üstü çökmüş, bir köşesi yırtık.
#   teneke      Dugme_Tenekesi_K6     : AÇILIR (Govde + Kapak) yuvarlak köşeli mavi bisküvi tenekesi (düğme kutusu).
#   raf         Raf_K6                : delikli köşebent metal raf; kumaş kırpıntısı dolu karton kutular.
#   sepet       Camasir_Sepeti_K6     : oval hasır çamaşır sepeti, kapağı aralık, bir gömlek kolu sarkıyor.
#   havlu       Havlu_K6              : duvar tahtasında iki kanca ve iki havlu (DUVAR eşyası).
#   ayna        Boy_Aynasi_K6         : terzinin üç kanatlı prova aynası, yan kanatlar 30° açık.
#   perdeterzi  Perde_Terzi_K6        : halkalı krom çubukta tek parça pileli gri perde.
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


def perde_kanadi(ad, x0, x1, z_ust, boy, y0, genlik, dalga, mat, kal=0.004, bas=0.5, etek=1.12, tohum=0.0, sutun=16):
    """
    TEK PARÇA pileli perde kanadı (kat5_esyalar.py'deki yardımcının aynısı; her betik kendi başına çalışsın diye kopya). Eski 'pilili' zikzak yaptığı için keskin kırışıklar ayrı ayrı çıtalar gibi
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


# ---------- GİYSİ YARDIMCILARI (askılıktaki ceket, elbise, palto) ----------
# Askıdaki giysi rayın altında y boyunca uzanır (omuzlar ±y'de), ön yüzü +x'e bakar.
# Gövde, yukarıdan aşağı yatay halkalardan örülür: her halka (cx, cy, z, yarı_kalınlık_x, yarı_en_y).

def giysi_loft(ad, halkalar, mat, kose=16, us=3.0, kivrim=None, alt=True, ust=True, kal=0.0):
    """
    LOFT: halkaları sırayla yüzlerle bağlar. Her halka süper elips |x/t|^us + |y/w|^us = 1:
    us = 2 düz elips, büyüdükçe köşeleri yuvarlak dikdörtgen (asılı boş ceket yassı ama köşesizdir).
    kivrim(yr, z): o noktayı x'te kaydırır; ön ve arka yüz birlikte kayar = kumaşın dikey kıvrımı.
    kal > 0: iki ucu açık halkalara (dik yaka) et kalınlığı verir.
    """
    v, f = [], []
    e = 2.0 / us                                   # süper elipsin açı üssü
    for (cx, cy, z, t, w) in halkalar:
        for i in range(kose):
            a = 2 * math.pi * i / kose
            c, s = math.cos(a), math.sin(a)
            y = cy + w * math.copysign(abs(s) ** e, s)
            x = cx + t * math.copysign(abs(c) ** e, c)
            if kivrim:
                x += kivrim(y - cy, z)              # kıvrım: aynı y'deki ön ve arka nokta birlikte kayar
            v.append((x, y, z))
    for j in range(len(halkalar) - 1):
        for i in range(kose):
            a0, a1 = j * kose + i, j * kose + (i + 1) % kose
            f.append((a0, a1, a1 + kose, a0 + kose))
    if ust:
        f.append(tuple(range(kose)))
    if alt:
        s0 = (len(halkalar) - 1) * kose
        f.append(tuple(reversed(range(s0, s0 + kose))))
    n = yeni_mesh(ad, v, f, mat)                    # yeni_mesh yüzleri dışa çevirir ve listeye ekler
    if kal > 0:
        m = n.modifiers.new("Kal", "SOLIDIFY")
        m.thickness = kal
    return n


def ara6(halkalar, z):
    """z yüksekliğindeki kesit (cx, cy, t, w): komşu iki halkanın doğrusal ara değeri."""
    for h0, h1 in zip(halkalar, halkalar[1:]):
        if h1[2] <= z <= h0[2]:
            u = (h0[2] - z) / max(1e-6, h0[2] - h1[2])
            return tuple(h0[k] + (h1[k] - h0[k]) * u for k in (0, 1, 3, 4))
    h = halkalar[0] if z > halkalar[0][2] else halkalar[-1]
    return (h[0], h[1], h[3], h[4])


def on_x(halkalar, yr, z, us=3.0, kivrim=None):
    """Gövdenin ÖN yüzünün (+x) o noktadaki x değeri: yaka, cep kapağı, düğme bu yüzeye oturur."""
    cx, cy, t, w = ara6(halkalar, z)
    oran = max(0.0, 1 - min(1.0, abs(yr / w)) ** us) ** (1 / us)
    return cx + t * oran + (kivrim(yr, z) if kivrim else 0.0)


def on_serit(ad, hk, ic0, ic1, dis0, dis1, mat, us=3.0, kivrim=None, satir=4, one=0.004, geri=0.002):
    """
    Ön yüzeye YAPIŞIK ince plaka (klapa, yaka kanadı, cep kapağı, etiket). (y, z) düzleminde iki kenarla
    tanımlanır: iç kenar ic0→ic1, dış kenar dis0→dis1; arası 'satir' dilime bölünür. Her köşe yüzeyin o
    noktasına oturtulur (önü one kadar önde, arkası geri kadar içeride): uzun bir klapa göğsün kavsini izler,
    ortası gövdeye gömülmez. Dörtgenler hep dışbükey kalır (Unity içbükey çokgeni yanlış üçgenleyebilir).
    """
    v, f = [], []
    for i in range(satir + 1):
        u = i / satir
        for (a, b) in ((ic0, ic1), (dis0, dis1)):
            y, z = a[0] + (b[0] - a[0]) * u, a[1] + (b[1] - a[1]) * u
            x = on_x(hk, y, z, us, kivrim)
            v += [(x + one, y, z), (x - geri, y, z)]    # önce ön yüz, sonra arka yüz köşesi
    # Köşe sırası: satır i'de [iç-ön, iç-arka, dış-ön, dış-arka] = 4i .. 4i+3
    for i in range(satir):
        a, b = 4 * i, 4 * (i + 1)
        f += [(a, a + 2, b + 2, b), (a + 1, b + 1, b + 3, a + 3),         # ön ve arka yüz
              (a, b, b + 1, a + 1), (a + 2, a + 3, b + 3, b + 2)]         # iç ve dış kenar
    f += [(0, 1, 3, 2), (4 * satir, 4 * satir + 2, 4 * satir + 3, 4 * satir + 1)]   # iki uç kapak
    return yeni_mesh(ad, v, f, mat)


def on_dikdortgen(ad, hk, y0, y1, z0, z1, mat, us=3.0, kivrim=None, one=0.004):
    """on_serit'in dikdörtgen hali (cep kapağı, etiket): dilimler y boyunca, çünkü yüzey en çok y'de kavislidir."""
    return on_serit(ad, hk, (y0, z1), (y1, z1), (y0, z0), (y1, z0), mat, us, kivrim, satir=2, one=one)


def kanca_zinciri(noktalar, r, mat, kose=6):
    """Eklem küresi koymadan kırık çizgi boru (askının kolu): küreler üçgen sayısını boşuna artırır."""
    for a, b in zip(noktalar, noktalar[1:]):
        cubuk(a, b, r, mat, kose)


# ---------- YUVARLAK HATLI ESKİ EŞYA YARDIMCILARI (60'lar buzdolabı, 70'ler puf) ----------

def yuvarlak_dikdortgen(x0, x1, z0, z1, r_ust, r_alt, n=6):
    """
    xz düzleminde (önden bakınca) köşeleri yuvarlatılmış dikdörtgenin çevre noktaları.
    Üst iki köşe r_ust, alt iki köşe r_alt yarıçaplı: 60'ların buzdolabı önden bakınca tepesi yuvarlak, altı düzdür.
    Her köşe n+1 noktadan oluşur; aynı n ile kurulan iki dikdörtgenin noktaları BİREBİR karşılık gelir,
    bu yüzden 'cerceve' ikisinin arasını dörtgenlerle örebilir.
    """
    nokta = []                                              # sonuç: (x, z) çiftleri, saat yönünün tersine
    koseler = ((x1 - r_alt, z0 + r_alt, r_alt, -90),        # sağ alt köşenin merkezi, yarıçapı, başlangıç açısı
               (x1 - r_ust, z1 - r_ust, r_ust, 0),          # sağ üst
               (x0 + r_ust, z1 - r_ust, r_ust, 90),         # sol üst
               (x0 + r_alt, z0 + r_alt, r_alt, 180))        # sol alt
    for (cx, cz, r, a0) in koseler:
        for i in range(n + 1):                              # her köşede çeyrek daire: 90° n parçaya bölünür
            a = math.radians(a0 + 90 * i / n)
            nokta.append((cx + r * math.cos(a), cz + r * math.sin(a)))
    return nokta


def cerceve(ad, dis, ic, y0, y1, mat):
    """
    İki iç içe çevre çizgisinin (dis, ic; aynı nokta sayısı) arasını y0'dan y1'e kalınlaştırır: ortası delik
    bir halka/kabuk. Buzdolabının gövde kabuğu (önü açık), kapı contası ve iç astar bununla kurulur;
    boolean (oyma) gerekmez, dörtgenler temiz kalır.
    """
    k = len(dis)
    v = ([(x, y0, z) for x, z in dis] + [(x, y0, z) for x, z in ic] +       # ön yüzün dış ve iç köşeleri
         [(x, y1, z) for x, z in dis] + [(x, y1, z) for x, z in ic])        # arka yüzün dış ve iç köşeleri
    f = []
    for i in range(k):
        j = (i + 1) % k                                     # bir sonraki nokta (son noktadan sonra ilk nokta)
        f.append((i, j, k + j, k + i))                      # ön yüz: dış kenarla iç kenar arası şerit
        f.append((2 * k + i, 3 * k + i, 3 * k + j, 2 * k + j))   # arka yüz
        f.append((i, 2 * k + i, 2 * k + j, j))              # dış yan yüz (gövdenin dışı)
        f.append((k + i, k + j, 3 * k + j, 3 * k + i))      # iç yan yüz (deliğin duvarı)
    return yeni_mesh(ad, v, f, mat)                         # yeni_mesh yüzleri dışa çevirir ve listeye ekler


def boru_yol(noktalar, r, mat, kose=6, kapali=False, ref=(0, 0, 1), ad="Boru"):
    """
    Noktalar boyunca TEK PARÇA boru (biye, krom yazı). cubuk+kure zinciri her eklemde ayrı parça ve küre
    üretir, üçgen sayısı şişer; bu ise her noktada bir halka kurup halkaları örer.
    ref: borunun kesitini döndürmemek için sabit bir yön. Yol xy düzlemindeyse z, xz düzlemindeyse y verilir;
    yoksa yol dikleştiği yerde kesit aniden döner ve boru burkulur.
    kapali: son nokta ilkine bağlanır (pufun kenar biyesi gibi halka).
    """
    P = [Vector(q) for q in noktalar]
    N = len(P)
    R = Vector(ref)
    v, f = [], []
    for i in range(N):
        if kapali:
            t = P[(i + 1) % N] - P[i - 1]                   # teğet: komşu iki noktanın farkı
        else:
            t = P[min(i + 1, N - 1)] - P[max(i - 1, 0)]
        t.normalize()
        a = t.cross(R).normalized()                         # kesitin birinci ekseni (teğete ve ref'e dik)
        b = t.cross(a)                                      # ikinci ekseni
        for k in range(kose):
            aci = 2 * math.pi * k / kose
            v.append(tuple(P[i] + r * (math.cos(aci) * a + math.sin(aci) * b)))
    for i in range(N if kapali else N - 1):
        j = (i + 1) % N
        for k in range(kose):
            k2 = (k + 1) % kose
            f.append((i * kose + k, i * kose + k2, j * kose + k2, j * kose + k))
    if not kapali:                                          # açık borunun iki ucu kapatılır (içi görünmesin)
        f.append(tuple(range(kose)))
        f.append(tuple(range((N - 1) * kose, N * kose)))
    return yeni_mesh(ad, v, f, mat)


def leke(merkez, eksen, r, mat, kal=0.0008, tohum=0):
    """
    Düzensiz kenarlı ince pul: emayenin attığı yer, pas çiçeği, aşınma. Yüzeyin içine yarı gömülür, yarısı
    (0,4 mm) taşar. eksen: yüzeyin normal ekseni ('x', 'y', 'z'). Kenarı rastgele olduğu için içbükey olabilir;
    Unity içbükey çokgeni yanlış üçgenleyebilir, bu yüzden ortadaki noktadan YELPAZE üçgenlerle kurulur.
    """
    rr = random.Random(tohum)                               # her leke kendi tohumuyla: aynı betik hep aynı lekeyi verir
    k = 9                                                   # kenar nokta sayısı
    kenar = []
    for i in range(k):
        a = 2 * math.pi * i / k
        rad = r * rr.uniform(0.55, 1.15)                    # yarıçap her noktada farklı: tırtıklı kenar
        kenar.append((rad * math.cos(a), rad * math.sin(a) * rr.uniform(0.8, 1.0)))
    v = []
    for t in (-kal / 2, kal / 2):                           # alt ve üst katman
        for (a, b) in [(0.0, 0.0)] + kenar:                 # ilk nokta merkez
            if eksen == "x":
                v.append((merkez[0] + t, merkez[1] + a, merkez[2] + b))
            elif eksen == "y":
                v.append((merkez[0] + a, merkez[1] + t, merkez[2] + b))
            else:
                v.append((merkez[0] + a, merkez[1] + b, merkez[2] + t))
    m = k + 1                                               # bir katmandaki nokta sayısı
    f = []
    for i in range(k):
        a, b = 1 + i, 1 + (i + 1) % k
        f.append((0, b, a))                                 # alt katmanın yelpaze üçgeni
        f.append((m, m + a, m + b))                         # üst katmanın yelpaze üçgeni
        f.append((a, b, m + b, m + a))                      # kenar şeridi
    return yeni_mesh("Leke", v, f, mat)


def kutu_uv(n):
    """
    Bütün yüzlere metre ölçekli KUTU İZDÜŞÜMÜ UV verir. from_pydata ile kurulan meshlerin UV'si yoktur; Unity'deki
    kir dokusu UV'ye göre serilir, UV'siz yüzde tek renk kalır. Her yüz, normalinin en büyük olduğu eksene
    dik düzlemden izdüşürülür (yan yüz yz'den, ön yüz xz'den, üst yüz xy'den): 1 m = 1 UV birimi, her yerde aynı yoğunluk.
    """
    me = n.data
    uv = me.uv_layers[0] if len(me.uv_layers) else me.uv_layers.new(name="UVMap")
    for poly in me.polygons:
        nx, ny, nz = (abs(c) for c in poly.normal)
        for li in poly.loop_indices:
            co = me.vertices[me.loops[li].vertex_index].co
            if nx >= ny and nx >= nz:
                uv.data[li].uv = (co.y, co.z)
            elif ny >= nz:
                uv.data[li].uv = (co.x, co.z)
            else:
                uv.data[li].uv = (co.x, co.y)


def superelips(A, B, us, N):
    """
    Süper elips |x/A|^us + |y/B|^us = 1 üzerinde EŞİT ARALIKLI N nokta (xy düzlemi). us büyüdükçe köşeleri yuvarlak
    dikdörtgene yaklaşır (8: yumuşak köşeli puf). Açıyla eşit adım atılırsa noktalar köşelere yığılır; burada önce
    çok sık örneklenip çevre uzunluğu ölçülür, sonra uzunluğa göre eşit bölünür: kilim deseninin kareleri eşit olsun.
    Başlangıç noktası (A, 0): sağ yüzün ortası; noktalar saat yönünün tersine ilerler.
    """
    M = 4000
    ham = []
    for i in range(M + 1):
        a = 2 * math.pi * i / M
        c, s = math.cos(a), math.sin(a)
        ham.append((A * math.copysign(abs(c) ** (2 / us), c), B * math.copysign(abs(s) ** (2 / us), s)))
    L = [0.0]                                               # her ham noktaya kadarki çevre uzunluğu
    for i in range(1, M + 1):
        L.append(L[-1] + math.dist(ham[i], ham[i - 1]))
    sonuc, j = [], 0
    for k in range(N):
        hedef = L[-1] * k / N                               # k. noktanın çevre üzerindeki uzaklığı
        while L[j + 1] < hedef:
            j += 1
        u = (hedef - L[j]) / max(1e-9, L[j + 1] - L[j])     # iki ham nokta arasında doğrusal ara değer
        sonuc.append((ham[j][0] + (ham[j + 1][0] - ham[j][0]) * u, ham[j][1] + (ham[j + 1][1] - ham[j][1]) * u))
    return sonuc


def cok_malzemeli(ad, v, f, idx, mats):
    """
    Her yüzü kendi malzemesiyle tek mesh: kilim deseni dokusuz, yüz yüz boyanır (kilimin kendisi de basamaklı
    kare desendir). idx[i]: f[i] yüzünün mats içindeki sırası. Yüz yönleri burada köşe sırasıyla doğru verilir.
    """
    me = bpy.data.meshes.new(ad)
    me.from_pydata(v, [], f)
    me.update()
    for m in mats:
        me.materials.append(m)                              # malzeme yuvaları
    for poly, k in zip(me.polygons, idx):
        poly.material_index = k                             # her yüz kendi yuvasını gösterir
    n = bpy.data.objects.new(ad, me)
    bpy.context.collection.objects.link(n)
    p.append(n)
    return n


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
    # Beş askı, soldan sağa: lacivert kruvaze ceket, bordo askılı elbise, kahve tek düğmeli ceket, naylon
    # kılıfta uzun siyah gece elbisesi (kılıfın altından eteği görünür), deve tüyü kemerli palto.
    # Hepsinin ön yüzü +x'e bakar; omuzları askının iki kolunun üstünde, kolları iki yanda sarkık.
    # Omuz halkaları: dik boyundan (1,515) aşağı doğru hızla genişler; askının tahta kolu bu eğimin hemen altında
    # kalır, yani kumaşın içinden taşmaz ama yakanın içinde görünür.
    UST = [(1.515, 0.040, 0.080), (1.505, 0.049, 0.130), (1.490, 0.055, 0.175), (1.470, 0.058, 0.200), (1.450, 0.060, 0.207)]

    def kivrim_yap(genlik, dalga, bas_z, faz):
        """bas_z'den aşağı yarım metrede tam genliğe çıkan dikey kumaş kıvrımı fonksiyonu üretir."""
        def kv(yr, z):
            g = min(1.0, max(0.0, (bas_z - z) / 0.5))
            return genlik * g * math.sin(yr * 2 * math.pi / dalga + faz)
        return kv

    def aski_kancasi(x, alt_z):
        """Raya takılı tel kanca (halka) ve askının ortasına inen boyun teli."""
        halka(0.018, 0.0022, (x, 0.0, 1.605), (1, 0, 0), metal, seg=8)
        cubuk((x, 0.0, 1.587), (x, 0.0, alt_z), 0.0022, metal, 5)

    def tahta_aski(x):
        """Terzinin tahta askısı: ortası yüksek, iki kolu omuzlara inen kalın ahşap çubuk."""
        aski_kancasi(x, 1.522)
        kanca_zinciri([(x, -0.175, 1.462), (x, -0.09, 1.498), (x, 0.0, 1.522), (x, 0.09, 1.498), (x, 0.175, 1.462)], 0.010, ahsap, 6)

    def kollar(x, mat, kol_z, buyuk=0.0):
        """İki kol: ilk halka omzun içinde (dikiş yeri görünmesin), bileğe doğru hafifçe öne (+x) düşer."""
        for sy in (-1, 1):
            giysi_loft("Kol", [(x + 0.008, sy * 0.170, 1.475, 0.040, 0.036),
                               (x + 0.012, sy * 0.195, 1.440, 0.050 + buyuk, 0.045),
                               (x + 0.018, sy * 0.200, 1.300, 0.048 + buyuk, 0.044),
                               (x + 0.026, sy * 0.202, 1.100, 0.044 + buyuk, 0.041),
                               (x + 0.032, sy * 0.200, kol_z + 0.012, 0.040 + buyuk, 0.038),
                               (x + 0.032, sy * 0.200, kol_z, 0.033, 0.031)], mat, kose=10, us=2.4)

    def yaka_klapa(hk, kv, mat, klapa, kanat, ust_yaka):
        """Dik yaka bandı (iki ucu açık, 4 mm et) + iki yanda klapa ve yaka kanadı (ayna simetrisi)."""
        giysi_loft("Yaka", ust_yaka, mat, kose=16, us=2.6, alt=False, ust=False, kal=0.004)
        for sy in (-1, 1):
            L = [(sy * a, b) for (a, b) in klapa]        # y'yi aynala: sol ve sağ klapa
            on_serit("Klapa", hk, L[0], L[1], L[2], L[3], mat, kivrim=kv)
            L = [(sy * a, b) for (a, b) in kanat]
            on_serit("YakaKanadi", hk, L[0], L[1], L[2], L[3], mat, kivrim=kv, satir=1)

    def dugmeler(hk, kv, noktalar, r=0.011):
        """Ön yüzeye oturan siyah düğmeler: (y, z) listesi."""
        for (dy, dz) in noktalar:
            disk(r, 0.005, (on_x(hk, dy, dz, 3.0, kv) + 0.004, dy, dz), (1, 0, 0), siyah, 8)

    def ceket(x, mat, etek, kruvaze, faz):
        kv = kivrim_yap(0.003, 0.25, 1.20, faz)       # ceket kumaşı sert: kıvrım çok hafif
        hk = [(x, 0.0, z, t, w) for (z, t, w) in UST + [(1.33, 0.062, 0.205), (1.15, 0.052, 0.188), (1.00, 0.055, 0.198),
                                                       (etek + 0.02, 0.053, 0.204), (etek, 0.050, 0.201)]]
        giysi_loft("Ceket", hk, mat, kose=20, us=3.0, kivrim=kv)
        tahta_aski(x)
        kollar(x, mat, 0.87)
        kir = 1.20 if kruvaze else 1.22                # klapaların birleştiği (kırıldığı) yükseklik
        yaka_klapa(hk, kv, mat,
                   [(0.058, 1.505), (0.0, kir), (0.128, 1.462), (0.016, kir)],      # iç kenar üst/alt, dış kenar üst/alt
                   [(0.048, 1.522), (0.060, 1.497), (0.103, 1.510), (0.117, 1.476)],
                   [(x - 0.004, 0.0, 1.552, 0.044, 0.070), (x, 0.0, 1.512, 0.050, 0.088)])
        for sy in (-1, 1):
            on_dikdortgen("CepKapagi", hk, sy * 0.045, sy * 0.175, 0.985, 1.03, mat, kivrim=kv)
        on_dikdortgen("GogusCebi", hk, -0.16, -0.075, 1.318, 1.338, mat, kivrim=kv)
        dugmeler(hk, kv, [(0.055, 1.18), (-0.055, 1.18), (0.055, 1.06), (-0.055, 1.06)] if kruvaze else [(0.0, 1.17), (0.0, 1.07)])
        return hk, kv

    def palto(x, mat, etek, faz):
        kv = kivrim_yap(0.007, 0.18, 1.10, faz)
        hk = [(x, 0.0, z, t, w) for (z, t, w) in UST + [(1.33, 0.066, 0.210), (1.12, 0.058, 0.198), (0.90, 0.064, 0.218),
                                                       (0.62, 0.070, 0.232), (etek + 0.02, 0.072, 0.238), (etek, 0.069, 0.235)]]
        giysi_loft("Palto", hk, mat, kose=24, us=3.0, kivrim=kv)
        tahta_aski(x)
        kollar(x, mat, 0.84, 0.004)
        yaka_klapa(hk, kv, mat,
                   [(0.060, 1.510), (0.0, 1.14), (0.160, 1.440), (0.030, 1.14)],        # geniş palto klapası
                   [(0.050, 1.530), (0.064, 1.500), (0.125, 1.515), (0.150, 1.455)],
                   [(x - 0.004, 0.0, 1.565, 0.050, 0.080), (x, 0.0, 1.512, 0.054, 0.095)])
        for sy in (-1, 1):
            on_dikdortgen("CepKapagi", hk, sy * 0.05, sy * 0.19, 0.86, 0.91, mat, kivrim=kv)
        dugmeler(hk, kv, [(sy * 0.075, z) for sy in (-1, 1) for z in (1.26, 0.98, 0.84)], 0.012)
        # Kemer: bel hizasında gövdeden 5 mm büyük bant, önde metal toka ve tokadan sarkan uç.
        kemer = [(*ara6(hk, z)[:2], z, ara6(hk, z)[2] + 0.005, ara6(hk, z)[3] + 0.005) for z in (1.14, 1.105)]
        giysi_loft("Kemer", kemer, mat, kose=24, us=3.0, kivrim=kv)
        bx = on_x(hk, 0.0, 1.1225, 3.0, kv) + 0.009
        halka(0.024, 0.004, (bx, 0.0, 1.1225), (1, 0, 0), metal, seg=12, olcek=(1.3, 1, 1))
        K("KemerUcu", (0.006, 0.035, 0.11), (bx, 0.045, 1.07), mat, 0.002)
        return hk, kv

    def elbise(x, mat, etek, faz):
        kv = kivrim_yap(0.012, 0.16, 1.00, faz)       # ince kumaş: etekte belirgin, yumuşak kıvrımlar
        hk = [(x, 0.0, z, t, w) for (z, t, w) in [(1.468, 0.032, 0.120), (1.455, 0.045, 0.150), (1.40, 0.056, 0.170),
                                                 (1.34, 0.052, 0.162), (1.20, 0.042, 0.138), (1.15, 0.045, 0.145),
                                                 (1.06, 0.058, 0.175), (0.85, 0.078, 0.212), (0.62, 0.092, 0.234),
                                                 (etek + 0.015, 0.098, 0.240), (etek, 0.094, 0.236)]]
        giysi_loft("Elbise", hk, mat, kose=28, us=2.6, kivrim=kv)
        # Tel askı bütünüyle görünür: kanca, iki kol ve alt çubuk. Elbisenin askıları kolların üstünden dolanır.
        aski_kancasi(x, 1.545)
        kanca_zinciri([(x, -0.19, 1.458), (x, 0.0, 1.545), (x, 0.19, 1.458), (x, -0.19, 1.458)], 0.0025, metal, 5)
        for sy in (-1, 1):
            kanca_zinciri([(x + 0.025, sy * 0.105, 1.462), (x + 0.012, sy * 0.145, 1.486),
                           (x - 0.012, sy * 0.145, 1.486), (x - 0.025, sy * 0.105, 1.462)], 0.006, mat, 6)
        # Belde ince siyah kuşak.
        kusak = [(*ara6(hk, z)[:2], z, ara6(hk, z)[2] + 0.004, ara6(hk, z)[3] + 0.004) for z in (1.19, 1.165)]
        giysi_loft("Kusak", kusak, siyah, kose=28, us=2.6, kivrim=kv)
        return hk, kv

    def kilifli(x, mat, faz):
        kv = kivrim_yap(0.005, 0.20, 1.40, faz)       # naylon kılıfın buruşukları
        hk = [(x, 0.0, z, t, w) for (z, t, w) in [(1.560, 0.012, 0.020), (1.540, 0.040, 0.085), (1.515, 0.055, 0.160),
                                                 (1.485, 0.064, 0.215), (1.45, 0.068, 0.230), (1.20, 0.069, 0.233),
                                                 (0.90, 0.068, 0.235), (0.63, 0.066, 0.233), (0.600, 0.056, 0.225),
                                                 (0.585, 0.030, 0.200)]]
        giysi_loft("Kilif", hk, naylon, kose=20, us=3.0, kivrim=kv)
        aski_kancasi(x, 1.555)                         # askının yalnız kancası kılıfın tepesinden çıkar
        # Kılıfın önünde boydan boya fermuar.
        zler = [1.50 - i * 0.11 for i in range(9)]
        for za, zb in zip(zler, zler[1:]):
            cubuk((on_x(hk, 0.0, za, 3.0, kv) + 0.002, 0.0, za), (on_x(hk, 0.0, zb, 3.0, kv) + 0.002, 0.0, zb), 0.003, metal, 5)
        # Kılıfın altından sarkan elbise eteği (üst ucu kılıfın içinde kalır).
        giysi_loft("Etek", [(x, 0.0, z, t, w) for (z, t, w) in [(0.64, 0.050, 0.200), (0.55, 0.056, 0.208), (0.42, 0.066, 0.222),
                                                               (0.265, 0.076, 0.232), (0.25, 0.073, 0.229)]],
                   mat, kose=20, us=2.6, kivrim=kivrim_yap(0.008, 0.16, 0.64, faz + 1.0))
        return hk, kv

    # (x, tür, kumaş rengi, etek yüksekliği, dönüş açısı). Her giysi askıda kendi ekseni çevresinde birkaç derece
    # dönük: hepsi aynı yöne döner ki komşular birbirine girmesin, ama açılar farklı olsun ki dizi yapay durmasın.
    giysiler = [(-0.42, "kruvaze", 0, 0.76, 4.0), (-0.21, "elbise", 1, 0.45, 2.0), (0.0, "ceket", 7, 0.78, 3.5),
                (0.21, "kilifli", 5, 0.25, 1.5), (0.42, "palto", 3, 0.40, 3.0)]
    for i, (x, tur, ci, etek, aci) in enumerate(giysiler):
        bas = len(p)                                    # bu giysinin parçaları p'de buradan başlıyor
        if tur in ("ceket", "kruvaze"):
            hk, kv = ceket(x, KUMAS[ci], etek, tur == "kruvaze", i * 1.3)
        elif tur == "palto":
            hk, kv = palto(x, KUMAS[ci], etek, i * 1.3)
        elif tur == "elbise":
            hk, kv = elbise(x, KUMAS[ci], etek, i * 1.3)
        else:
            hk, kv = kilifli(x, KUMAS[ci], i * 1.3)
        on_dikdortgen("Etiket", hk, 0.10, 0.14, 1.25, 1.31, etiket, kivrim=kv, one=0.006)   # terzinin kâğıt fişi
        dondur_parcalar(bas, don("Z", math.radians(aci), (x, 0.0, 0.0)))
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
    # 1974'te alınmış, yirmi yıldır aynı köşede duran yerli buzdolabı: tepesi yuvarlak gövde, kalın kapı,
    # büyük krom kollu kulp, kapıda krom el yazısı marka, içte küçük alüminyum buzluk. Emaye sararmış,
    # kulp çevresinde ve alt köşelerde emaye atmış, pas çiçeklenmiş.
    # ZİNCİR SÖZLEŞMESİ (GrayboxKurucu_Kat6.cs okur, DEĞİŞMEZ): alt tel rafın üstü z = 0,50 (anahtar 0,504'e konur),
    # ampul (0, 0,10, 1,00)'da (iç ışık oraya konur), kapının menteşe çizgisi x = 0,275, y = -0,27, alt ucu z = 0,065;
    # kapı menteşenin -x yanında durur (Unity açılma yönünü buradan bulur). Gövde içi boş, önü açık.
    acik = True
    emaye = ortak.malzeme("Buz6_Emaye", (0.72, 0.69, 0.58), puruz=0.25)       # sararmış fırın emayesi (doğrusal renk)
    astar = ortak.malzeme("Buz6_Astar", (0.60, 0.61, 0.56), puruz=0.35)       # iç kabın emayesi, biraz grimsi
    conta = ortak.malzeme("Buz6_Conta", (0.09, 0.09, 0.085), puruz=0.75)      # sertleşmiş kauçuk conta
    izgara = ortak.malzeme("Buz6_Izgara", (0.04, 0.04, 0.04), puruz=0.5, metal=0.5)
    sebzelik = ortak.malzeme("Buz6_Sebzelik", (0.45, 0.50, 0.48), puruz=0.15)
    alu = ortak.malzeme("Buz6_Aluminyum", (0.50, 0.50, 0.48), puruz=0.5, metal=0.8)   # buzluk: donuk alüminyum
    bkrom = ortak.malzeme("Buz6_Krom", (0.60, 0.60, 0.58), puruz=0.28, metal=1.0)     # yıllanmış, hafif donuk krom
    pas = ortak.malzeme("Buz6_Pas", (0.16, 0.06, 0.02), puruz=0.9)
    kirik = ortak.malzeme("Buz6_Kirik", (0.03, 0.03, 0.03), puruz=0.6, metal=0.3)     # emaye atınca görünen kararmış sac
    EN, Y0, Y1, Z0, Z1, T = 0.55, -0.24, 0.29, 0.06, 1.25, 0.035   # gövde: en, ön/arka y, alt/üst z, duvar kalınlığı
    X = EN / 2
    RU, RA = 0.09, 0.012                     # önden bakınca üst köşelerin ve alt köşelerin yarıçapı
    YD = yuvarlak_dikdortgen
    lev = ac.levha

    # GÖVDE KABUĞU: dış çevre (tepesi yuvarlak) ile iç çevre arası, önden arkaya; önü açık, içi oyuk.
    dis = YD(-X, X, Z0, Z1, RU, RA)
    ic = YD(-X + T, X - T, Z0 + T, Z1 - T, RU - T, 0.004)              # iç köşe yarıçapı dıştakinden T kadar küçük: et her yerde eşit
    kabuk = cerceve("Kabuk", dis, ic, Y0, Y1, emaye)
    ortak.pah(kabuk, 0.012, 2)                                          # kenarlar yuvarlak: 60'ların preste bükülmüş sacı
    prizma("Arka", ic, Y1 - T, Y1, astar, "xz")                         # arka duvar: iç deliği arkadan kapatır
    cerceve("Astar", ic, YD(-X + T + 0.004, X - T - 0.004, Z0 + T + 0.004, Z1 - T - 0.004, RU - T - 0.004, 0.004),
            Y0 + 0.002, Y1 - T, astar)                                  # 4 mm'lik iç kap: içerisi astar renginde, ağzında ince bir kenar
    # Kaide: içeri çekik koyu taban, önünde yatay havalandırma ızgarası; altında dört lastik ayak.
    p.append(lev("Kaide", -X + 0.012, X - 0.012, Y0 + 0.025, Y1 - 0.01, 0.0, Z0 + 0.002, conta, 0.004))
    for z in (0.014, 0.027, 0.040):
        p.append(lev("Izgara", -X + 0.04, X - 0.04, Y0 + 0.018, Y0 + 0.026, z, z + 0.006, izgara, 0.0))
    for x in (-X + 0.05, X - 0.05):
        for y in (Y0 + 0.06, Y1 - 0.05):
            S("Ayak", 0.018, 0.012, (x, y, 0.006), conta, kose=10)
    # Menteşe: kapının döndüğü çizgide (x = X, y = -0,27) altta ve üstte krom topuz, gövdenin yanına vidalı yaprak.
    for zm in (0.12, 1.08):
        S("Mentese", 0.009, 0.05, (X, -0.27, zm), bkrom, kose=12, pah=0.002)
        p.append(lev("MenteseYaprak", X - 0.001, X + 0.003, -0.266, -0.236, zm - 0.022, zm + 0.022, bkrom, 0.0))
    # Yıpranma: yan sacların altında pas, üstte emaye atığı (kir dokusunu Unity ayrıca ekler).
    leke((-X - 0.0003, -0.10, 0.15), "x", 0.035, pas, tohum=1)
    leke((-X - 0.0006, -0.10, 0.15), "x", 0.014, kirik, kal=0.0012, tohum=2)
    leke((-X - 0.0003, 0.05, 0.10), "x", 0.022, pas, tohum=6)
    leke((X + 0.0003, 0.16, 0.11), "x", 0.028, pas, tohum=3)
    leke((X + 0.0006, 0.16, 0.11), "x", 0.010, kirik, kal=0.0012, tohum=7)
    leke((0.05, 0.12, Z1 + 0.0003), "z", 0.007, kirik, tohum=4)
    leke((-0.12, -0.05, Z1 + 0.0003), "z", 0.012, pas, tohum=5)

    # İÇ: üstte küçük alüminyum buzluk (önü menteşeli kapakçık), altında damla tavası; arkada lamba kutusu.
    p.append(lev("Buzluk", -0.17, 0.17, -0.13, 0.245, 1.06, 1.205, alu, 0.004))
    p.append(lev("BuzlukKapak", -0.165, 0.165, -0.142, -0.13, 1.064, 1.20, alu, 0.003))
    for z in (1.10, 1.16):
        p.append(lev("BuzlukOluk", -0.15, 0.15, -0.1435, -0.142, z, z + 0.004, izgara, 0.0))   # preslenmiş oluklar
    K("BuzlukMandal", (0.05, 0.012, 0.012), (0, -0.148, 1.188), bkrom, 0.003)
    p.append(lev("DamlaTavasi", -0.18, 0.18, -0.15, 0.04, 1.035, 1.043, sebzelik, 0.002))
    p.append(lev("DamlaTavasiOn", -0.18, 0.18, -0.152, -0.146, 1.035, 1.056, sebzelik, 0.0))
    K("LambaKutusu", (0.08, 0.05, 0.05), (0, 0.10, 1.035), astar, 0.004)
    S("Ampul", 0.012, 0.02, (0, 0.10, 1.0), ortak.malzeme("Buz6_Ampul", (0.8, 0.78, 0.7), puruz=0.2), kose=12)
    # Termostat düğmesi: sağ iç duvarda bakalit topuz ve çizgisi.
    disk(0.022, 0.012, (X - T - 0.010, -0.08, 0.97), (-1, 0, 0), conta, kose=16)
    K("TermostatCizgi", (0.002, 0.004, 0.016), (X - T - 0.0165, -0.08, 0.978), astar)
    # İki krom tel raf: üst yüzleri 0,50 (anahtar bunun üstünde) ve 0,78; yanlarda taşıyıcı raylar.
    xi = X - T - 0.004                                     # iç kabın iç yüzü
    for zr in (0.50, 0.78):
        for sx in (-1, 1):
            K("Ray", (0.01, 0.38, 0.008), (sx * (xi - 0.005), 0.05, zr - 0.012), astar)
        K("RafOn", (2 * xi - 0.004, 0.008, 0.008), (0, -0.155, zr - 0.004), bkrom)
        K("RafArka", (2 * xi - 0.004, 0.008, 0.008), (0, 0.245, zr - 0.004), bkrom)
        for i in range(12):
            x = -xi + 0.012 + i * (2 * xi - 0.024) / 11
            K("RafTel", (0.004, 0.40, 0.004), (x, 0.045, zr - 0.004), bkrom)
    # Sebzelik: dört ince duvar ve üstünde camı.
    zt = Z0 + T + 0.004
    for (a, b, c, d) in ((-0.22, 0.22, -0.162, -0.155), (-0.22, 0.22, 0.235, 0.242), (-0.22, -0.213, -0.155, 0.235), (0.213, 0.22, -0.155, 0.235)):
        p.append(lev("Sebzelik", a, b, c, d, zt, 0.235, sebzelik, 0.0))
    p.append(lev("SebzelikCam", -0.225, 0.225, -0.165, 0.245, 0.235, 0.245, sebzelik, 0.002))

    # KAPI: bütün ön yüzü kaplayan kalın kapı; aynı yuvarlak hat, yuvarlatılmış kalın kenar.
    bas = len(p)                                           # bundan sonra eklenen her parça kapıya ait
    D = YD(-X + 0.003, X - 0.003, Z0 + 0.005, Z1 - 0.003, RU - 0.003, RA)
    ortak.pah(prizma("Kapi", D, -0.29, -0.25, emaye, "xz"), 0.014, 2)
    # Conta: gövdenin ağız kenarına basan kauçuk halka; içte kapı astarı (kapanınca ağızdan içeri girer).
    cerceve("Conta", YD(-X + 0.015, X - 0.015, Z0 + 0.017, Z1 - 0.015, RU - 0.015, 0.004),
            YD(-X + 0.033, X - 0.033, Z0 + 0.035, Z1 - 0.033, RU - 0.033, 0.004), -0.2515, -0.2405, conta)
    ortak.pah(prizma("KapiAstar", YD(-X + 0.053, X - 0.053, Z0 + 0.058, Z1 - 0.053, RU - 0.053, 0.004),
                     -0.25, -0.225, astar, "xz"), 0.006, 2)
    # Kapı balkonları: astar renkli tabla, iki yanağı ve önünde iki krom tel (60'larda plastik değil tel).
    for zb in (0.30, 0.62, 0.92):
        p.append(lev("Balkon", -0.19, 0.19, -0.225, -0.18, zb, zb + 0.008, astar, 0.0))   # küçük parçalarda pah yok: üçgen tasarrufu
        for sx in (-1, 1):
            p.append(lev("BalkonYanak", sx * 0.19 - 0.004, sx * 0.19 + 0.004, -0.225, -0.18, zb, zb + 0.06, astar, 0.0))
        for dz in (0.022, 0.05):
            cubuk((-0.19, -0.184, zb + dz), (0.19, -0.184, zb + dz), 0.0035, bkrom, 6)
    # Kulp: serbest kenarda (sol, -x) krom taban levhası, önünde D biçimli kavrama ve tepesinde basılan dil.
    xk = -0.215
    K("KulpTaban", (0.042, 0.008, 0.25), (xk, -0.293, 0.90), bkrom, 0.003)
    boru_yol([(xk, -0.296, 1.00), (xk, -0.306, 0.993), (xk, -0.3135, 0.975), (xk, -0.316, 0.94), (xk, -0.316, 0.86),
              (xk, -0.3135, 0.825), (xk, -0.306, 0.807), (xk, -0.296, 0.80)], 0.0095, bkrom, kose=12, ref=(1, 0, 0), ad="Kavrama")
    dil = K("KulpDili", (0.034, 0.028, 0.011), (xk, -0.297 - 0.014, 1.022), bkrom, 0.003)
    donustur(dil, don("X", math.radians(-22), (xk, -0.297, 1.022)))   # dil öne-yukarı kalkık: kapıyı açmak için bastırılır
    # Krom el yazısı marka (uydurma, okunmayan bir eğri: gerçek marka kullanılmaz) ve altında kuyruklu çizgisi,
    # onun altında kapı boyunca ince krom şerit.
    yazi = []
    for i in range(56):
        t = i / 55
        w = 2 * math.pi * 6 * t                              # altı ilmek: el yazısının birbirine bağlı harfleri
        z = 1.118 - 0.010 * math.cos(w) * (1.0 if 0.12 < t < 0.9 else 1.6)   # baş harf ve son harf büyük
        x = -0.095 + 0.19 * t - 0.011 * math.sin(w) + 0.35 * (z - 1.118)     # 0,35: yazı sağa yatık
        yazi.append((x, -0.2912, z))
    boru_yol(yazi, 0.0026, bkrom, kose=5, ref=(0, 1, 0), ad="Yazi")
    boru_yol([(-0.11 + 0.22 * i / 23, -0.2912, 1.094 + 0.006 * math.sin(math.pi * i / 23) - 0.004 * i / 23) for i in range(24)],
             0.0022, bkrom, kose=5, ref=(0, 1, 0), ad="YaziAlti")
    K("KromSerit", (0.40, 0.003, 0.010), (0.03, -0.2912, 1.05), bkrom, 0.001)
    # Kapı yıpranması: kulp çevresinde elle aşınmış emaye, alt köşelerde pas, ortada birkaç darbe izi.
    # Her satır: (x, z, yarıçap, malzeme, tohum). Pasın ortasında kararmış sac (kirik) biraz daha önde durur.
    for (x, z, r, m, tohum) in ((-0.225, 0.12, 0.032, pas, 11), (-0.228, 0.118, 0.013, kirik, 12),
                                (0.205, 0.11, 0.024, pas, 13), (0.207, 0.108, 0.009, kirik, 19),
                                (-0.10, 0.10, 0.016, pas, 18), (0.04, 0.098, 0.012, pas, 20),          # alt kenar boyunca pas
                                (-0.243, 0.96, 0.012, kirik, 14), (-0.188, 0.83, 0.009, kirik, 15),   # kulp çevresi: tırnak ve yüzük izi
                                (-0.243, 0.80, 0.007, kirik, 21), (0.08, 0.43, 0.010, kirik, 16),
                                (0.12, 0.70, 0.006, kirik, 17), (-0.02, 0.55, 0.005, kirik, 22)):
        leke((x, -0.2902 - (0.0004 if m is kirik else 0.0), z), "y", r, m, kal=0.0008, tohum=tohum)
    kp = p[bas:]                                           # kapı parçaları ana listeden ayrılır
    del p[bas:]
    kapi = ortak.birlestir(kp, "Kapi_1")
    ortak.yumusat(kapi, 35)
    kutu_uv(kapi)
    ortak.pivotu_tasi(kapi, (X, -0.27, Z0 + 0.005))        # menteşe çizgisinin alt ucu (eskisiyle aynı)
    parcalar.append(kapi)
    ad, h, uzak = "Buzdolabi_K6", 1.3, 2.0

elif hangi == "puf":
    # 70'lerin döşemeli pufu (Poly Haven'ın siyah deri modern pufunun yerine): yanları kilim dokumalı kumaş,
    # üstü aşınmış kahverengi kadife, düğmeli (kapitone), kenarlarında biye, altında dört kısa torna ahşap ayak.
    # Üstü yıllarca oturulmaktan ortası ve sol yanı çökmüş; ön sağ köşesinde kadife yırtılmış, sünger görünüyor.
    # Ölçü: 0,88 x 0,62 x ~0,42 m. Pivot tabanın ortası, ön yüz -y.
    A, B, US = 0.43, 0.30, 8.0               # gövde kesiti: süper elipsin yarı eni, yarı derinliği ve üssü (yumuşak köşe)
    Z0, Z1 = 0.085, 0.375                    # yan kumaşın altı (ayakların üstü) ve üstü (kadife tablanın kenarı)
    SISKIN = 0.012                           # yanların ortada dışa şişmesi (dolgu bastırır): oran
    kadife = ortak.malzeme("Puf6_Kadife", (0.08, 0.04, 0.02), puruz=0.85)          # koyu kahve kadife
    asinmis = ortak.malzeme("Puf6_Asinmis", (0.11, 0.065, 0.035), puruz=0.65)          # oturulan yerde tüyü yatmış, açılmış kadife
    yirtik = ortak.malzeme("Puf6_Yirtik", (0.03, 0.015, 0.008), puruz=0.95)         # yırtığın karanlık ağzı
    sunger = ortak.malzeme("Puf6_Sunger", (0.45, 0.36, 0.14), puruz=0.95)           # sararmış sünger
    alt_bez = ortak.malzeme("Puf6_AltBez", (0.05, 0.04, 0.03), puruz=1.0)           # alttaki kaba bez
    kirmizi = ortak.malzeme("Puf6_KilimKirmizi", (0.24, 0.04, 0.028), puruz=0.95)  # kök boya kırmızı (zemin)
    lacivert = ortak.malzeme("Puf6_KilimLacivert", (0.025, 0.03, 0.09), puruz=0.95) # çivit
    hardal = ortak.malzeme("Puf6_KilimHardal", (0.38, 0.22, 0.04), puruz=0.95)      # cehri sarısı
    krem = ortak.malzeme("Puf6_KilimKrem", (0.50, 0.43, 0.30), puruz=0.95)          # boyanmamış yün
    kahve = ortak.malzeme("Puf6_KilimKahve", (0.07, 0.035, 0.018), puruz=0.95)      # kenar şeridi

    # YAN KUMAŞ: süper elips halkaları alttan üste örülür; her yüz kilim deseninin bir "ilmeği" kadar
    # (yaklaşık 3,2 x 2,6 cm) ve kendi renginde. 90 sütun = 10 tam desen tekrarı: başlangıç dikişinde desen kesilmez.
    NS, NR = 90, 11                          # çevre boyunca sütun, yükseklikte satır sayısı
    cevre = superelips(A, B, US, NS)
    v, f, idx = [], [], []
    for j in range(NR + 1):
        t = j / NR
        z = Z0 + (Z1 - Z0) * t
        s = 1 + SISKIN * math.sin(math.pi * t)               # ortada şişkin, alt ve üst kenarda biyeye oturur
        for (x, y) in cevre:
            v.append((x * s, y * s, z))
    MATS = [kirmizi, lacivert, hardal, krem, kahve, alt_bez]

    def desen(i, j):
        """Kilim deseni: kenarlarda kahve ve krem şerit, ortada basamaklı baklava (göbek hardal, çerçeve lacivert)."""
        if j in (0, NR - 1):
            return 4                                         # en alt ve en üst satır: kahve kenar
        if j in (1, NR - 2):
            return 3                                         # krem şerit
        du, dv = (i % 9) - 4, j - NR // 2                    # baklavanın merkezine uzaklık (sütun, satır)
        d = abs(du) + abs(dv)                                # "basamaklı" uzaklık: kilimin kırık çizgili baklavası
        if d <= 1:
            return 2                                         # hardal göbek
        if d == 3:
            return 1                                         # lacivert çerçeve
        if abs(du) == 4 and dv == 0:
            return 3                                         # iki baklava arasında krem "küpe" motifi
        return 0                                             # kırmızı zemin
    for j in range(NR):
        for i in range(NS):
            a, b = j * NS + i, j * NS + (i + 1) % NS
            f.append((a, b, b + NS, a + NS))                 # saat yönü tersine halka + yukarı = normal dışa bakar
            idx.append(desen(i, j))
    f.append(tuple(reversed(range(NS))))                     # alt kapak (yere bakan bez)
    idx.append(5)
    cok_malzemeli("Yan", v, f, idx, MATS)

    # ÜST TABLA: kare ızgara süper elipse "şişirilerek" kurulur; yüksekliği kabarıklık - çökme - düğme çukurları.
    NU, NV = 31, 23
    dugmeler = [(-0.24, -0.10), (0.0, -0.10), (0.24, -0.10), (-0.12, 0.10), (0.12, 0.10)]   # kapitone düğmeleri

    def tabla_z(x, y, rho):
        kabar = 0.034 * max(0.0, 1 - rho ** 4) ** 0.5        # kenarda dik kalkan, ortada düz kabarıklık
        cokme = 0.024 * math.exp(-((x + 0.09) ** 2 / (2 * 0.21 ** 2) + (y + 0.01) ** 2 / (2 * 0.16 ** 2)))  # sol-ortada oturma çukuru
        cukur = sum(0.022 * math.exp(-((x - bx) ** 2 + (y - by) ** 2) / (2 * 0.028 ** 2)) for bx, by in dugmeler)
        return Z1 + kabar - cokme - cukur * (1 - rho ** 6)
    v, f, idx = [], [], []
    for jv in range(NV):
        for iu in range(NU):
            u, w = -1 + 2 * iu / (NU - 1), -1 + 2 * jv / (NV - 1)
            m = max(abs(u), abs(w))                          # karedeki "halka" numarası (kenarda 1)
            nrm = (abs(u) ** US + abs(w) ** US) ** (1 / US)
            k = m / nrm if nrm > 1e-9 else 1.0               # kareyi süper elipse şişiren çarpan: kenar tam elips üstüne düşer
            x, y = A * u * k, B * w * k
            v.append((x, y, tabla_z(x, y, m)))
    for jv in range(NV - 1):
        for iu in range(NU - 1):
            a = jv * NU + iu
            f.append((a, a + 1, a + NU + 1, a + NU))         # +x, +y sırası: normal yukarı
            q = [v[a], v[a + 1], v[a + NU + 1], v[a + NU]]
            cx, cy = sum(c[0] for c in q) / 4, sum(c[1] for c in q) / 4
            if ((cx - 0.30) / 0.045) ** 2 + ((cy + 0.21) / 0.03) ** 2 < 1:
                idx.append(2)                                # ön sağ köşede yırtık
            elif ((cx + 0.10) / 0.22) ** 2 + ((cy + 0.02) / 0.14) ** 2 < 0.85 + 0.25 * math.sin(cx * 41) * math.sin(cy * 37):
                idx.append(1)                                # oturulan yer: tüyü yatmış kadife (sınırı sinüslerle tırtıklı, elips gibi durmasın)
            else:
                idx.append(0)
    cok_malzemeli("Tabla", v, f, idx, [kadife, asinmis, yirtik])
    # Yırtıktan taşan sünger ve kapitone düğmeleri (kumaş kaplı, çukurun dibinde).
    kure(0.028, (0.30, -0.21, tabla_z(0.30, -0.21, 0.75) + 0.002), sunger, olcek=(1.3, 0.8, 0.35), seg=10)
    for (bx, by) in dugmeler:
        rho = max(abs(bx) / A, abs(by) / B)
        kure(0.012, (bx, by, tabla_z(bx, by, rho) + 0.003), kadife, olcek=(1, 1, 0.45), seg=8)

    # BİYE: üst ve alt kenarda kumaş kaplı kordon; yan kumaşla tabla arasındaki dikişi örter.
    for (z, mat) in ((Z1, kadife), (Z0, kahve)):
        boru_yol([(x, y, z) for (x, y) in superelips(A, B, US, 64)], 0.0085, mat, kose=5, kapali=True, ref=(0, 0, 1), ad="Biye")
    # Ayaklar: dört kısa, aşağı doğru incelen torna ceviz ayak; uçta pirinç pabuç.
    for sx in (-1, 1):
        for sy in (-1, 1):
            q = (sx * 0.34, sy * 0.21, 0.0)
            torna("Ayak", [(0.016, 0.008), (0.019, 0.03), (0.024, 0.065), (0.028, 0.075), (0.028, Z0)], koyu, kose=10, konum=q)
            torna("Pabuc", [(0.0155, 0.0), (0.0165, 0.008)], pirinc, kose=10, konum=q)
    ad, h, uzak = "Puf_K6", 0.45, 1.5

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

else:   # perdeterzi: pileli gri perde ve krom çubuğu
    gri = ortak.malzeme("PerdeT_Gri", (0.16, 0.16, 0.17), puruz=0.9)
    EN, BOY = 1.80, 1.75                       # çubuk boyu; çubuktan perde eteğine
    R = 0.012
    # Çubuk: merkezi z = -R (üstü pivot yüksekliğinde), duvardan 8 cm önde; iki taşıyıcı, uçlarda top.
    cubuk((-EN / 2, -0.08, -R), (EN / 2, -0.08, -R), R, krom, 14)
    for sx in (-1, 1):
        kure(0.022, (sx * (EN / 2 + 0.018), -0.08, -R), krom, seg=12)
        cubuk((sx * (EN / 2 - 0.08), 0.0, -R), (sx * (EN / 2 - 0.08), -0.08, -R), 0.008, krom, 8)
        disk(0.022, 0.006, (sx * (EN / 2 - 0.08), -0.003, -R), (0, -1, 0), krom, 16)

    # Kanatlar: TEK PARÇA kumaş (perde_kanadi): yumuşak sinüs kıvrımları, halkalara büzülmüş başlık, dikişli etek.
    # Sol kanat yarıya kadar çekili (geniş, gevşek kıvrımlar), sağ kanat kenarda toplanmış (dar, sık kıvrımlar).
    # Kumaşın üst kenarı halkaların 3,5 cm altında; her halkadan inen kısa tel kumaşın başlığına takılı.
    for (x0, x1, genlik, dalga, tohum, sutun) in ((-0.86, -0.20, 0.024, 0.11, 0.8, 12), (0.62, 0.86, 0.020, 0.08, 1.9, 14)):
        halkalar_x = perde_kanadi("Kanat", x0, x1, -0.035, BOY - 0.035, -0.08, genlik, dalga, gri, 0.004,
                                  tohum=tohum, sutun=sutun)
        for x in halkalar_x:
            halka(0.02, 0.003, (x, -0.08, -R), (1, 0, 0), krom, seg=10)
            cubuk((x, -0.08, -R - 0.02), (x, -0.08, -0.04), 0.002, krom, 5)

    ad, h, uzak = "Perde_Terzi_K6", 1.8, 2.6

# Önizlemede kaldırma: perde aşağı sarkar, duvar eşyasının yarısı zemin altındadır.
kaldir = {"perdeterzi": 1.8, "havlu": 0.28}.get(hangi, 0.0)
# Kendi meshini from_pydata ile kuran (UV'siz parçası olan) yeni modeller: sonda kutu izdüşümü UV alırlar.
# Eski modeller listede yok; onların FBX'i bu değişiklikle değişmesin.
YENI_UV = ("buzdolabi", "puf")
if acik:
    govde = ortak.birlestir(p, "Govde")
    ortak.yumusat(govde, 35)
    if hangi in YENI_UV:
        kutu_uv(govde)                       # metre ölçekli UV: Unity'nin kir dokusu her yüzde aynı yoğunlukta serilsin
    ortak.pivotu_tasi(govde, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [govde] + parcalar, h, uzak)
else:
    nesne = ortak.birlestir(p, ad)
    ortak.yumusat(nesne, 35)
    if hangi in YENI_UV:
        kutu_uv(nesne)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    ortak.calistir_ve_bitir(ad, [nesne], h, uzak, onizlemede_kaldir=kaldir)
