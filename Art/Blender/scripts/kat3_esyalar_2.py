# kat3_esyalar_2.py
# 3. kat (#83), Kerem'in ailesinin dairesi: yatak odaları. Yardımcılar kat3_esyalar.py'den gelir.
#   blender -b --factory-startup --python kat3_esyalar_2.py -- <ad> [--onizleme dosya.png]
#
# ODA B (Kerem'in odası; gece uyanıp "dokuzuncu kattaki adamı" çizdiği yer)
#   yatak          Cocuk_Yatagi_K3    : mavi boyalı tek kişilik çocuk yatağı; yıldızlı örtü geri atılmış,
#                                       yastık, yastığa yaslı oyuncak ayı. Başucu +y tarafında.
#   calisma        Calisma_Masasi_K3  : 90'lar ceviz laminat sunta çalışma masası, üç çekmeceli keson (çekmeceler
#                                       süs), kenarları yeşil cam, camın altında ders programı ve fotoğraflar;
#                                       soyulmuş kenar bandı, kırık köşe. Masa lambası, boya kalemi kutusu ve
#                                       yarım kalmış "uzun adam" çizimi.
#   poster         Poster_K3          : uzay robotlu çizgi film posteri ve yanında futbol flaması (duvar eşyası).
#   yerdeki        Yerdeki_Cizimler_K3: yere saçılmış sekiz kâğıt (çoğunda kara, uzun adam), dağınık kalemler.
#   gece_lambasi   Gece_Lambasi_K3    : 90'lar küçük abajur: seramik ayak, pilili kumaş abajur; ara anahtarlı
#                                       kablosu ve yuvarlak fişiyle.
#   perde_cocuk    Perde_Cocuk_K3     : açık mavi, bulut desenli çocuk odası perdesi, beyaz çubuk (1,2 m pencere).
# ODA A (anne-babanın odası)
#   karyola        Karyola_K3         : açık akçaağaç çift kişilik karyola, dolgulu başlık, pastel çiçekli örtü
#                                       (annenin yanı geri atılmış), iki yastık. Başlık +y tarafında.
#   komodin        Komodin_K3         : AÇILIR akçaağaç komodin (Govde + Cekmece_1); üst çekmeceye katın anahtarı konur.
#   besik          Besik_K3           : sallanan ahşap bebek beşiği; içinde küçük battaniye, bebek yok.
#   sifonyer       Sifonyer_K3        : çekmeceli şifonyer (süs), devrilebilir ayna, parfüm şişeleri, pudra.
#   perde_odaA     Perde_OdaA_K3      : fırfırlı kornişli lila perde ve pirinç çubuğu (1,2 m pencere).
#   kilim          Kilim_K3           : yatak başı için küçük, baklava motifli kilim.
# Ön yüz -y (Unity'de +Z). Pivotlar kat3_esyalar.py'deki kuralla aynı.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
from mathutils import Vector, Matrix
from kat3_esyalar import *          # P, BILGI, malzemeler, şekil yardımcıları, kagit, bitir ...


def yatak():
    mv, sari = mt("cocuk_mavi"), M("Yildiz_Sari", (0.80, 0.58, 0.05), 0.5)
    lac, beyaz = M("Ortu_Gece", (0.02, 0.04, 0.14), 0.9), mt("kumas_beyaz")
    EN, BOY = 0.95, 1.95
    hy, ay = BOY / 2 - 0.025, -BOY / 2 + 0.025
    for x in (-EN / 2 + 0.025, EN / 2 - 0.025):
        kutu((0.05, 0.05, 0.74), (x, hy, 0.37), mv, 0.008)
        kure(0.032, (x, hy, 0.765), mv)
        kutu((0.05, 0.05, 0.54), (x, ay, 0.27), mv, 0.008)
        kure(0.032, (x, ay, 0.565), mv)
        kutu((0.03, BOY - 0.10, 0.12), (x, 0, 0.26), mv, 0.005)           # yan kasalar
    kutu((EN - 0.05, 0.025, 0.40), (0, hy, 0.50), mv, 0.006)               # başucu panosu
    yatik_sil(0.02, EN - 0.05, (0, hy, 0.715), "x", mv)
    kutu((EN - 0.05, 0.025, 0.22), (0, ay, 0.37), mv, 0.006)               # ayakucu panosu
    yuz = hy - 0.0125                                                      # panonun ön yüzü
    for (x, z, r) in ((-0.25, 0.56, 0.05), (0.02, 0.47, 0.035), (0.26, 0.58, 0.045)):
        prizma(yildiz(x, z, r), "xz", yuz, yuz - 0.004, sari)             # başuçta yıldızlar ve ay
    dik_disk(0.06, 0.004, (-0.02, yuz - 0.002, 0.60), sari, 24)
    dik_disk(0.055, 0.004, (0.01, yuz - 0.006, 0.615), mv, 24)
    kutu((EN - 0.09, BOY - 0.10, 0.14), (0, 0, 0.37), beyaz, 0.03)         # şilte ve çarşaf (üstü 0,44)
    # Gece örtüsü: ayakucundan y = 0,30'a kadar; ötesi geri atılmış, rulo olmuş (Kerem kalkıp gitmiş).
    y0, y1 = ay + 0.02, 0.30
    kutu((EN - 0.05, y1 - y0, 0.03), (0, (y0 + y1) / 2, 0.455), lac, 0.012)
    for x in (-EN / 2 + 0.046, EN / 2 - 0.046):
        kutu((0.006, y1 - y0, 0.14), (x, (y0 + y1) / 2, 0.40), lac)
    yatik_sil(0.035, EN - 0.08, (0, y1, 0.47), "x", lac, 14)
    don(kutu((0.30, 0.18, 0.05), (0.15, 0.45, 0.455), beyaz, 0.02), rz=0.3)   # buruşmuş çarşaf
    for i in range(16):                                                    # örtüdeki yıldızlar
        x, y = rnd.uniform(-0.38, 0.38), rnd.uniform(y0 + 0.06, y1 - 0.08)
        prizma(yildiz(x, y, rnd.uniform(0.02, 0.032), aci=rnd.uniform(0, 1.2)), "xy", 0.4702, 0.4712, sari)
    kutu((0.55, 0.30, 0.12), (0, 0.72, 0.50), beyaz, 0.05)                 # yastık (üstü ~0,56)
    # Oyuncak ayı: yastığa yaslanmış oturuyor.
    kah, acik, siyah = M("Ayi_Kahve", (0.22, 0.10, 0.04), 0.95), M("Ayi_Acik", (0.50, 0.32, 0.16), 0.95), mt("ic_koyu")
    bx, by = 0.22, 0.78
    kure(0.075, (bx, by, 0.635), kah, (1.0, 0.8, 1.1))
    kure(0.058, (bx, by - 0.01, 0.765), kah)
    for s in (-1, 1):
        kure(0.022, (bx + s * 0.045, by, 0.815), kah)
        kure(0.028, (bx + s * 0.075, by - 0.03, 0.645), kah, (0.7, 0.7, 1.4))
        kure(0.032, (bx + s * 0.045, by - 0.08, 0.575), kah, (0.8, 1.4, 0.8))
        kure(0.006, (bx + s * 0.02, by - 0.062, 0.78), siyah, seg=8, halka=6)
    kure(0.025, (bx, by - 0.06, 0.75), acik, (1.0, 0.8, 0.8))
    kure(0.008, (bx, by - 0.08, 0.755), siyah, seg=8, halka=6)
    return bitir("Cocuk_Yatagi_K3")


# ---------- 90'LAR YARDIMCILARI (çalışma masası ve gece lambası) ----------
# İlk sürümde masa beyaz-kırmızı, düz yüzeyli bir "paket mobilya", gece lambası da hilal biçimli bir oyuncak
# gibiydi: ikisi de bugünün mağazasından çıkmış duruyordu. 1994'te bir çocuk odasında olanlar ceviz desenli
# sunta masa ile seramik ayaklı, pilili abajurlu küçük lambaydı. Aşağıdaki üç yardımcı bunları kurar:
#   torna90      : çömlekçi çarkı gibi, bir yan kesiti döndürüp yuvarlak gövde yapar (seramik ayak, duy).
#   pilili_abajur: katlanmış (pilili) kumaştan, içi boş, kesik koni biçimli abajur.
#   yonga90      : bir kenardan küçük bir parça koparır ve açılan yere içerideki malzemenin rengini verir
#                  (kırık sunta köşesi, sırrı atmış seramik): yılların bıraktığı "ucuz ama dürüst" yıpranma.

def torna90(profil, mat, kose=28):
    """
    Bir yan kesiti (yarıçap, yükseklik) z ekseni etrafında döndürüp kapalı, yuvarlak bir gövde kurar.
    Çömlekçinin çarkta çamuru elleriyle biçimlendirmesi gibi: elin izlediği çizgi profil, çark döndükçe
    o çizgi bir gövdeye dönüşür. profil aşağıdan yukarı sıralanır; ilk ve son noktanın yarıçapı 0 olmalı,
    yoksa gövdenin altı ya da üstü açık kalır (Unity arkası görünmeyen yüzü çizmez, delik gibi durur).
    """
    import bmesh                                          # mesh'i nokta nokta kurmak için Blender'ın aracı
    bm = bmesh.new()                                      # boş bir mesh taslağı
    uv = bm.loops.layers.uv.new("UVMap")                  # kir dokusu otursun diye UV katmanı (adı ilkellerinkiyle aynı)
    zmin, zmax = profil[0][1], profil[-1][1]              # UV'nin dikey ekseni bu aralığa yayılacak
    halkalar = []                                         # profilin her noktası için bir halka
    for r, z in profil:
        if r <= 1e-6:                                     # eksendeki nokta: tek bir köşe (kutup) yeter
            halkalar.append([bm.verts.new((0.0, 0.0, z))])
        else:                                             # eksenden uzak nokta: çevresinde "kose" kadar köşe
            halkalar.append([bm.verts.new((r * math.cos(2 * math.pi * j / kose), r * math.sin(2 * math.pi * j / kose), z))
                             for j in range(kose)])

    def yuz(koseler, uvler):
        """Bir yüz ekler ve her köşesine UV koordinatını yazar."""
        f = bm.faces.new(koseler)
        for dongu, (u, v) in zip(f.loops, uvler):         # yüzün her köşesi ("loop") için bir UV
            dongu[uv].uv = (u, v)

    for (h0, h1), (p0, p1) in zip(zip(halkalar, halkalar[1:]), zip(profil, profil[1:])):
        v0 = (p0[1] - zmin) / max(zmax - zmin, 1e-6)      # alt halkanın UV yüksekliği (0-1)
        v1 = (p1[1] - zmin) / max(zmax - zmin, 1e-6)      # üst halkanınki
        for j in range(kose):                             # çevreyi dilim dilim dolaş
            k = (j + 1) % kose                            # komşu köşe (sonuncusu ilkine bağlanır)
            u0, u1 = j / kose, (j + 1) / kose             # dilimin UV'de sol ve sağ kenarı
            if len(h0) == 1 and len(h1) == 1:             # iki kutup üst üste: aralarında yüz yok
                break
            if len(h0) == 1:                              # alt kutuptan halkaya: üçgen yelpaze
                yuz((h0[0], h1[k], h1[j]), ((u0, v0), (u1, v1), (u0, v1)))
            elif len(h1) == 1:                            # halkadan üst kutba: üçgen yelpaze
                yuz((h0[j], h0[k], h1[0]), ((u0, v0), (u1, v0), (u0, v1)))
            else:                                         # iki halka arası: dörtgen şerit
                yuz((h0[j], h0[k], h1[k], h1[j]), ((u0, v0), (u1, v0), (u1, v1), (u0, v1)))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)     # bütün yüzler dışa baksın
    me = bpy.data.meshes.new("Torna")
    bm.to_mesh(me)                                        # taslağı gerçek mesh'e yaz
    bm.free()
    n = bpy.data.objects.new("Torna", me)                 # mesh'i taşıyan nesne
    bpy.context.collection.objects.link(n)                # sahneye koy
    ortak.ata(n, mat)
    return ekle(n)


def pilili_abajur(r_alt, r_ust, z_alt, z_ust, pili, derin, kal, mat, leke=None):
    """
    Pilili kumaş abajur: kesik koni biçiminde, akordeon gibi katlanmış, içi boş bir kumaş kabuk.
    Çevrede 2 x pili köşe var: çiftler dışta (katın sırtı), tekler "derin" kadar içeride (katın dibi);
    böylece yüzey zikzak çizer ve ışık her katta kırılır, kumaş olduğu anlaşılır.
    kal: kumaşın kalınlığı. Abajurun bir İÇ yüzü de var; yoksa alttan bakınca içi boşluk gibi görünürdü.
    leke: (malzeme, merkez_aci, genislik) verilirse dış yüzün üst kısmında o açıdaki dilimler bu malzemeyi
    alır: yıllarca yanan ampulün ısısıyla kumaşın sararmış yeri.
    """
    import bmesh
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    n2 = 2 * pili                                         # çevredeki köşe sayısı (sırt + dip)
    z_dis = (z_alt, z_alt + 0.5 * (z_ust - z_alt), z_alt + 0.78 * (z_ust - z_alt), z_ust)   # dış yüzde dört halka: leke yukarıda genişlesin

    def nokta(i, z, ic):
        """i. köşenin z yüksekliğindeki konumu; ic ise kumaş kalınlığı kadar içeride."""
        a = 2 * math.pi * i / n2                          # köşenin açısı
        r = r_alt + (r_ust - r_alt) * (z - z_alt) / (z_ust - z_alt)   # koni: yukarı çıktıkça daralır
        r -= derin if i % 2 else 0.0                      # tek köşeler katın dibi
        r -= kal if ic else 0.0                           # iç yüz kalınlık kadar içeride
        return (r * math.cos(a), r * math.sin(a), z)

    dis = [[bm.verts.new(nokta(i, z, False)) for i in range(n2)] for z in z_dis]          # dış halkalar
    ic = [[bm.verts.new(nokta(i, z, True)) for i in range(n2)] for z in (z_alt, z_ust)]   # iç halkalar

    def yuz(koseler, uvler, malz=0):
        f = bm.faces.new(koseler)
        f.material_index = malz                           # 0 kumaş, 1 sararmış leke
        for dongu, (u, v) in zip(f.loops, uvler):
            dongu[uv].uv = (u, v)

    vz = [(z - z_alt) / (z_ust - z_alt) for z in z_dis]   # halkaların UV yüksekliği
    for i in range(n2):
        j = (i + 1) % n2                                  # komşu köşe
        u0, u1 = i / n2, (i + 1) / n2                     # UV'de dilimin iki kenarı
        a = 2 * math.pi * (i + 0.5) / n2                  # dilimin orta açısı (leke için)
        for s in range(3):                                # dış yüz: alt, orta ve üst şerit
            malz = 0
            if leke and s > 0:                            # leke orta şeritte dar, üst şeritte geniş: ısı yukarı yayılır
                fark = math.atan2(math.sin(a - leke[1]), math.cos(a - leke[1]))   # açı farkı (-pi..pi)
                malz = 1 if abs(fark) < leke[2] / 2 * (0.45 if s == 1 else 1.0) else 0
            yuz((dis[s][i], dis[s][j], dis[s + 1][j], dis[s + 1][i]),
                ((u0, vz[s]), (u1, vz[s]), (u1, vz[s + 1]), (u0, vz[s + 1])), malz)
        yuz((ic[0][j], ic[0][i], ic[1][i], ic[1][j]), ((u1, 0), (u0, 0), (u0, 1), (u1, 1)))   # iç yüz (ters sıra)
        yuz((dis[0][i], ic[0][i], ic[0][j], dis[0][j]), ((u0, 0), (u0, 0), (u1, 0), (u1, 0)))  # alt kenar (kumaşın kesiti)
        yuz((dis[3][j], ic[1][j], ic[1][i], dis[3][i]), ((u1, 1), (u1, 1), (u0, 1), (u0, 1)))  # üst kenar
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new("Abajur")
    # Malzeme yuvaları doğrudan eklenir; ortak.ata kullanılmaz, çünkü ata önce listeyi siler ve bu
    # silme yüzlerin yuva numaralarını da sıfırlıyordu (ilk denemede sararmış leke hiç görünmedi).
    me.materials.append(mat)                              # 0. malzeme yuvası: kumaş
    if leke:
        me.materials.append(leke[0])                      # 1. malzeme yuvası: leke
    bm.to_mesh(me)
    bm.free()
    n = bpy.data.objects.new("Abajur", me)
    bpy.context.collection.objects.link(n)
    return ekle(n)


def yonga90(hedef, merkez, olcu, mat_ic):
    """
    Bir kenardan küçük bir parça koparır: elipsoid biçimli bir "kesici" hedefin içinden oyulur (BOOLEAN),
    sonra oyulan yerin yüzlerine içerideki malzemenin rengi verilir. Gerçekte de sunta masanın köşesi
    vurulunca kaplama kopar, altından açık renkli yonga levha görünür; seramikte sır atar, pişmiş toprak çıkar.
    olcu: elipsoidin (x, y, z) yarıçapları. Hedefte değiştirici (pah gibi) olmamalı, yoksa oyma sırası bozulur.
    """
    bpy.ops.mesh.primitive_uv_sphere_add(radius=1.0, location=merkez, segments=12, ring_count=8)
    kes = bpy.context.active_object                       # kesici küre
    kes.scale = olcu                                      # küreyi elipsoide çek
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)   # ölçeği meshe işle
    ortak.ata(kes, mat_ic)                                # kesiciye içerideki malzemeyi ver: oyuğun yüzeyi ondan gelir
    # ortak.oy yerine BOOLEAN'ı burada kuruyoruz, çünkü bir ayar daha gerekiyor: material_mode = "TRANSFER".
    # Bu ayar "kesicinin oyduğu yüzler kesicinin malzemesini alsın" demek; Blender malzemeyi hedefe yeni bir
    # yuva olarak ekler. Varsayılan ayarda oyuk boş (malzemesiz) bir yuvaya düşüyordu.
    mod = hedef.modifiers.new("Yonga", "BOOLEAN")
    mod.operation = "DIFFERENCE"                          # fark: kesicinin hacmi silinir
    mod.solver = "EXACT"                                  # kesin çözücü: küçük, eğri oyuklarda temiz sonuç
    mod.material_mode = "TRANSFER"
    mod.object = kes
    sec(hedef)                                            # değiştirici etkin nesneye uygulanır
    bpy.ops.object.modifier_apply(modifier=mod.name)      # sonucu kalıcı meshe işle
    bpy.data.objects.remove(kes, do_unlink=True)          # kesicinin işi bitti
    return hedef


def calisma():
    # 1990'ların çocuk çalışma masası: ceviz desenli laminat kaplı sunta, sağda üç çekmeceli keson, solda
    # kalemlik çekmecesi, tablada kenarları yeşil cam ve camın altına sıkıştırılmış ders programı,
    # fotoğraflar, çıkartmalar. Yıpranma: ön kenar bandı soyulmuş, sağ ön köşe kırık (altından sunta
    # görünüyor), orta çekmecenin kulpu kopmuş (iki vida deliği kalmış), yan tablaya tükenmez kalemle
    # karalanmış, çekmecede yarısı yırtılmış çıkartma.
    ceviz = M("Masa90_Ceviz", (0.075, 0.036, 0.016), 0.45)        # ceviz desenli laminat (doğrusal renk: koyu)
    ceviz_k = M("Masa90_CevizKoyu", (0.048, 0.022, 0.010), 0.4)   # çekmece önleri bir ton koyu, o yılların modası
    sunta = M("Masa90_Sunta", (0.32, 0.22, 0.11), 0.95)           # kaplamanın altındaki yonga levha (açık, mat)
    bant = M("Masa90_KenarBandi", (0.11, 0.058, 0.026), 0.35)     # kenar bandı: kaplamadan biraz açık ve parlak
    oluk = mt("ic_koyu")                                          # oluklar, vida delikleri, gölgeli boşluklar
    kulp = M("Masa90_Kulp", (0.50, 0.37, 0.15), 0.35, 0.85)       # altın renkli eloksal alüminyum kulp
    cam_k = M("Masa90_CamKenar", (0.16, 0.28, 0.24), 0.1)         # camın yeşil kenarı
    T0 = 0.715                                                    # tablanın üst yüzü (camın altı)
    T = 0.72                                                      # camın üstü: masadaki her şey buraya oturur
    G = T0 - 0.022                                                # tablanın alt yüzü: gövdeler buraya kadar çıkar

    # --- Tabla ve kırık köşe ---
    tabla = kutu((0.90, 0.50, 0.022), (0, 0, T0 - 0.011), ceviz)  # 22 mm sunta tabla (pahsız: yonga oyulacak)
    yonga90(tabla, (0.45, -0.25, T0 - 0.004), (0.024, 0.017, 0.013), sunta)   # sağ ön köşe vurulup kırılmış
    # Ön kenar bandı: sağda yerinde, solda 16 cm'lik kısmı soyulup kıvrılmış; altından sunta görünüyor.
    kutu((0.71, 0.0015, 0.022), (0.065, -0.25075, T0 - 0.011), bant)          # yapışık kalan bant
    kutu((0.16, 0.001, 0.022), (-0.37, -0.2505, T0 - 0.011), sunta)           # bandın altından çıkan çıplak kenar
    yol = egri([(-0.29, -0.2509, T0 - 0.011), (-0.33, -0.2525, T0 - 0.012), (-0.37, -0.260, T0 - 0.016),
                (-0.40, -0.272, T0 - 0.026), (-0.412, -0.276, T0 - 0.042)], 4)   # bant öne ve aşağı kıvrılıyor
    supur(yol, serit_kesit(0.0015, 0.022), bant)                # 1,5 mm kalın, 22 mm geniş kıvrık şerit

    # --- Sol yan tabla: tükenmez kalem karalaması ve çizikler ---
    kutu((0.020, 0.48, G), (-0.44, 0.0, G / 2), ceviz, 0.002)    # sol ayak tablası (yere kadar)
    tukenmez = M("Masa90_Tukenmez", (0.02, 0.04, 0.22), 0.6)     # mavi tükenmez kalem mürekkebi
    for (y, z, L, a) in ((-0.10, 0.47, 0.06, 0.5), (-0.07, 0.47, 0.06, -0.6), (-0.04, 0.475, 0.05, 0.7),
                         (-0.01, 0.47, 0.06, -0.4), (0.03, 0.44, 0.09, 0.1), (-0.06, 0.42, 0.04, 1.2)):
        don(kutu((0.0006, L, 0.0016), (-0.4503, y, z), tukenmez), rx=a)   # yan yüze karalanmış çizgiler
    for (y, z, L, a) in ((0.08, 0.20, 0.12, 0.15), (0.11, 0.17, 0.08, 0.05), (-0.15, 0.30, 0.10, -0.2)):
        don(kutu((0.0006, L, 0.0012), (-0.4503, y, z), sunta), rx=a)      # sandalye ve ayakkabı çizikleri

    # --- Sağda çekmece kesonu ---
    kutu((0.30, 0.47, G), (0.30, 0.005, G / 2), ceviz, 0.002)    # keson gövdesi (ön yüzü y = -0,23)
    kutu((0.27, 0.004, 0.045), (0.30, -0.232, 0.0225), oluk)     # alttaki içeri çekik süpürgelik gölgesi
    cekmeceler = ((0.052, 0.300), (0.304, 0.502), (0.506, G - 0.003))   # (alt z, üst z): altta derin çekmece
    for i, (z0, z1) in enumerate(cekmeceler):
        h, zc = z1 - z0, (z0 + z1) / 2                           # çekmece önünün boyu ve ortası
        kutu((0.292, 0.018, h), (0.30, -0.239, zc), ceviz_k, 0.0025)   # çekmece önü (süs, açılmaz)
        yf = -0.2483                                             # ön yüzün hemen önü: oluk çizgileri burada
        for dz in (-1, 1):                                       # 90'ların "yivli" önü: içte ince oluk çerçeve
            kutu((0.242, 0.0006, 0.002), (0.30, yf, zc + dz * (h / 2 - 0.025)), oluk)
        for dx in (-1, 1):
            kutu((0.002, 0.0006, h - 0.05), (0.30 + dx * 0.121, yf, zc), oluk)
        zk = z1 - 0.045                                          # kulp önün üst kısmında
        if i == 1:                                               # orta çekmecenin kulpu kopmuş: iki vida deliği
            for dx in (-0.048, 0.048):
                dik_disk(0.0025, 0.001, (0.30 + dx, -0.2485, zk), oluk, 8)
        else:                                                    # altın renkli çubuk kulp, iki ayağıyla
            for dx in (-0.048, 0.048):
                cubuk((0.30 + dx, -0.248, zk), (0.30 + dx, -0.266, zk), 0.0035, kulp, 8)
            yatik_sil(0.0045, 0.12, (0.30, -0.268, zk), "x", kulp, 10)
    # Orta çekmecede yarısı yırtılmış futbol çıkartması, altta yapıştırıcı izi kalmış kâğıt.
    prizma([(0.30 + 0.022 * math.cos(a), 0.37 + 0.022 * math.sin(a)) for a in [k * math.pi / 8 for k in range(-4, 5)]],
           "xz", -0.2482, -0.2488, M("Cikartma_Sari", (0.75, 0.52, 0.03), 0.5))          # sarı yarısı duruyor
    prizma([(0.30, 0.392), (0.292, 0.38), (0.296, 0.37), (0.289, 0.36), (0.294, 0.35), (0.30, 0.348)],
           "xz", -0.2482, -0.2486, M("Cikartma_Kirmizi", (0.42, 0.03, 0.02), 0.5))       # kırmızı yarıdan kalan
    prizma([(0.25, 0.13), (0.29, 0.135), (0.285, 0.115), (0.27, 0.112), (0.252, 0.118)],
           "xz", -0.2482, -0.2485, mt("kagit"))                                         # alt çekmecede kâğıt kalıntısı

    # --- Solda kalemlik çekmecesi ve arka etek ---
    kutu((0.56, 0.44, 0.07), (-0.14, -0.01, G - 0.035), ceviz)    # çekmece kutusu (diz boşluğunun tavanı)
    kutu((0.572, 0.018, 0.085), (-0.14, -0.239, G - 0.0455), ceviz_k, 0.0025)   # çekmece önü
    for dx in (-0.04, 0.04):
        cubuk((-0.14 + dx, -0.248, G - 0.045), (-0.14 + dx, -0.264, G - 0.045), 0.0035, kulp, 8)
    yatik_sil(0.0045, 0.10, (-0.14, -0.266, G - 0.045), "x", kulp, 10)
    kutu((0.58, 0.016, 0.36), (-0.14, 0.232, G - 0.07 - 0.18), ceviz)   # arka etek (tahta perde)

    # --- Cam ve altındakiler ---
    # Cam Unity'de opak bir malzeme olur ve altındakileri gizlerdi; bu yüzden yalnızca yeşil KENARLARI
    # modellenir. Göz camı kenarından tanır; altındaki kâğıtlar tablanın üstünde görünür kalır.
    for s in (-1, 1):
        kutu((0.88, 0.007, 0.005), (0, s * 0.2365, T0 + 0.0025), cam_k)    # ön ve arka cam kenarı
        kutu((0.007, 0.466, 0.005), (s * 0.4365, 0, T0 + 0.0025), cam_k)   # yan cam kenarları
    b = lambda ad: mt("boya_" + ad)
    # Ders programı: mavi çizgili tablo, başlık şeridi, hücrelerde el yazısı gibi kısa çizgiler.
    bas = len(P)
    W, H = 0.20, 0.14
    kutu((W, H, KAGIT_K), (0, 0, KAGIT_K / 2), mt("kagit"))
    c_kutu(0, H / 2 - 0.012, W - 0.01, 0.016, b("acikmavi"), 0, "yer")       # başlık şeridi
    for k in range(6):                                                       # sütun çizgileri (saat + 5 gün)
        c_kutu(-W / 2 + 0.005 + k * (W - 0.01) / 5, -0.006, 0.0012, H - 0.024, b("mavi"), 1, "yer")
    for k in range(7):                                                       # satır çizgileri
        c_kutu(0, H / 2 - 0.02 - k * 0.019, W - 0.01, 0.0012, b("mavi"), 1, "yer")
    r = random.Random(23)
    for k in range(18):                                                      # hücrelerdeki ders adları (çizgi)
        u = -W / 2 + 0.024 + (k % 5) * 0.038 + r.uniform(-0.003, 0.003)
        c_kutu(u, H / 2 - 0.03 - (k // 5) * 0.019, r.uniform(0.012, 0.024), 0.0015, b("siyah"), 2, "yer")
    grupla(bas, (0.03, 0.12, T0), (0, 0, 0.035))
    # İki eski fotoğraf: beyaz kenarlı, renkleri solmuş (biri yaylada aile, biri deniz kenarı).
    foto_k = M("Foto_Kenar", (0.72, 0.70, 0.62), 0.6)
    for (x, y, a, gok, yer, kisi) in ((-0.30, -0.16, 0.16, (0.30, 0.40, 0.45), (0.10, 0.16, 0.05), 3),
                                      (0.32, -0.15, -0.22, (0.35, 0.45, 0.55), (0.55, 0.45, 0.28), 2)):
        bas = len(P)
        kutu((0.09, 0.065, KAGIT_K), (0, 0, KAGIT_K / 2), foto_k)
        c_kutu(0, 0.008, 0.08, 0.035, M("Foto_Gok%d" % kisi, gok, 0.6), 0, "yer")    # gökyüzü
        c_kutu(0, -0.017, 0.08, 0.017, M("Foto_Yer%d" % kisi, yer, 0.6), 0, "yer")   # çimen ya da kum
        for k in range(kisi):                                                 # fotoğraftaki küçük insanlar
            c_kutu(-0.018 + k * 0.016, -0.006, 0.007, 0.022, b("kahve" if k % 2 else "gri"), 1, "yer")
            c_disk(-0.018 + k * 0.016, 0.008, 0.0035, M("Foto_Ten", (0.45, 0.28, 0.18), 0.6), 2, "yer", kose=8)
        grupla(bas, (x, y, T0), (0, 0, a))
    # Bayrak çıkartması: o yıllarda hemen her çocuğun masasında, defterinde olurdu. Kırmızı zemin, beyaz ay
    # (beyaz daireye kırmızı daire bindirilerek oyulmuş gibi görünür) ve beyaz yıldız; katmanlar üst üste.
    bas = len(P)
    kutu((0.075, 0.05, KAGIT_K), (0, 0, KAGIT_K / 2), M("Bayrak_Kirmizi", (0.42, 0.02, 0.02), 0.6))
    c_disk(-0.012, 0.0, 0.0125, mt("kumas_beyaz"), 0, "yer", kose=24)         # ayın dış dairesi
    c_disk(-0.0088, 0.0, 0.010, M("Bayrak_Kirmizi", (0.42, 0.02, 0.02), 0.6), 1, "yer", kose=24)   # iç daire: hilal kalır
    c_poly(yildiz(0.0065, 0.0, 0.0062, aci=math.pi), mt("kumas_beyaz"), 2, "yer")   # yıldız (bir ucu aya bakar)
    grupla(bas, (-0.38, 0.03, T0), (0, 0, -0.08))
    # Gülen yüz çıkartması (90'ların simgesi): sarı daire, iki göz, gülümseme.
    bas = len(P)
    c_disk(0.0, 0.0, 0.018, M("Cikartma_Sari", (0.75, 0.52, 0.03), 0.5), 0, "yer")
    for s_ in (-1, 1):
        c_disk(s_ * 0.006, 0.005, 0.0022, b("siyah"), 1, "yer", kose=8)          # gözler
    for k in range(6):                                                       # gülümseme: yay boyunca kısa çizgiler
        a0, a1 = math.pi * (1.2 + 0.1 * k), math.pi * (1.2 + 0.1 * (k + 1))
        c_cizgi(0.010 * math.cos(a0), 0.010 * math.sin(a0), 0.010 * math.cos(a1), 0.010 * math.sin(a1), 0.0018, b("siyah"), 1, "yer")
    grupla(bas, (0.15, -0.02, T0), (0, 0, 0.3))
    # Diğer çıkartmalar: mavi yıldız ve yeşil daire.
    prizma(yildiz(0.41, 0.06, 0.02), "xy", T0 + 0.0004, T0 + 0.0008, b("mavi"))
    c_disk(-0.17, 0.19, 0.014, b("yesil"), 0, "yer")
    grupla(len(P) - 1, (0, 0, T0))

    # 80'ler-90'lar yaylı masa lambası: yeşil emaye, ağır yuvarlak döküm taban, ikişer paralel çubuktan iki kol,
    # kol başlarında kanatlı somunlu mafsallar, alt kolda gergi yayları, ağzı kıvrık çan biçimli başlık ve
    # başlığın arkasında tırtıllı döner anahtar. (İlk sürüm düz çubuklu, koni başlıklı, bugünkü gibi sade bir
    # lambaydı; o dönemin lambası ağır, metal ve "mekanik" görünürdü.)
    yes = M("Lamba_Yesil", (0.04, 0.18, 0.08), 0.35, 0.4)                  # yeşil fırın boyası
    krom = mt("krom")
    bas = len(P)                                                            # taban: dünya merkezinde kurulur
    torna90([(0.0, 0.0), (0.066, 0.0), (0.067, 0.006), (0.060, 0.013), (0.036, 0.021), (0.018, 0.025),
             (0.014, 0.032), (0.0, 0.032)], yes, 32)                        # basamaklı ağır taban
    grupla(bas, (-0.33, 0.14, T))                                           # masadaki yerine
    kutu((0.018, 0.012, 0.007), (-0.33, 0.098, T + 0.018), mt("siyah_plastik"), 0.002)   # tabandaki basmalı düğme
    kord = M("Lamba_Kordon", (0.10, 0.06, 0.03), 0.8)                       # kahverengi kumaş kaplı kordon
    supur(egri([(-0.33, 0.20, T + 0.004), (-0.32, 0.225, T + 0.0025), (-0.30, 0.245, T + 0.0025),
                (-0.292, 0.2525, T - 0.01), (-0.29, 0.2525, T - 0.12)], 4), daire_kesit(0.0025, 6), kord)
    # Kollar: her kol iki paralel çubuk (x'te ±12 mm); mafsallarda yatay mil ve iki ucunda kanatlı somun.
    A, B, C = Vector((-0.33, 0.14, T + 0.032)), Vector((-0.30, 0.10, T + 0.30)), Vector((-0.185, 0.03, T + 0.352))
    for dx in (-0.012, 0.012):
        d = Vector((dx, 0, 0))
        cubuk(A + d, B + d, 0.0045, yes, 8)                                 # alt kol çubukları
        cubuk(B + d, C + d, 0.0045, yes, 8)                                 # üst kol çubukları
    for m in (A, B, C):                                                     # üç mafsal
        yatik_sil(0.007, 0.034, tuple(m), "x", krom, 12)                    # mafsal mili
        for s in (-1, 1):
            kutu((0.003, 0.016, 0.007), (m.x + s * 0.019, m.y, m.z), krom, 0.001)   # kanatlı somun
    # Gergi yayları: alt kolun yanında, sarmal tel. Yol, kolun ekseni boyunca ilerlerken çevresinde döner.
    for dx in (-0.024, 0.024):
        a0, a1 = A + Vector((dx, 0.0, 0.03)), A + (B - A) * 0.62 + Vector((dx, 0.0, 0.0))
        ek = (a1 - a0).normalized()                                         # yayın ekseni
        u = ek.cross(Vector((1, 0, 0))).normalized()                        # eksene dik iki yön
        w = ek.cross(u)
        yol = [a0 + (a1 - a0) * (k / 72) + (u * math.cos(k * 1.0) + w * math.sin(k * 1.0)) * 0.004 for k in range(73)]   # ~11 sarım
        supur(yol, daire_kesit(0.0009, 4), krom, kapak=False)
    # Başlık: çan biçimli, ağzı kıvrık. Profil, boynu z = 0'da olacak şekilde ters (ağzı aşağıda) kurulur,
    # sonra öne eğilip üst kolun ucuna taşınır. Profil içten başlayıp ağızdan dönerek dışa çıkar: içi boş kabuk.
    bas = len(P)
    torna90([(0.0, -0.006), (0.016, -0.008), (0.025, -0.016), (0.033, -0.028), (0.041, -0.045), (0.048, -0.064),
             (0.053, -0.082), (0.056, -0.090), (0.061, -0.092), (0.062, -0.087), (0.058, -0.085),
             (0.050, -0.066), (0.044, -0.046), (0.036, -0.030), (0.028, -0.014), (0.020, -0.004), (0.0, 0.0)], yes, 32)
    sil(0.006, 0.014, (0.0, 0.0, 0.007), krom, 12)                          # boyun: kola bağlanan mil
    yatik_sil(0.007, 0.010, (0.0, 0.026, -0.012), "y", krom, 14)            # arkadaki tırtıllı döner anahtar
    for k in range(8):                                                      # tırtıl dişleri
        a = 2 * math.pi * k / 8
        kutu((0.0016, 0.010, 0.0016), (0.0075 * math.cos(a), 0.026, -0.012 + 0.0075 * math.sin(a)), krom)
    kure(0.022, (0.0, 0.0, -0.058), M("Ampul", (0.80, 0.75, 0.55), 0.2))   # içindeki armut ampul
    grupla(bas, tuple(C + Vector((0.0, 0.0, 0.004))), (-0.55, 0.0, 0.0))   # öne eğik, kolun ucunda
    # Boya kalemi kutusu: kapağı arkaya açık, içinde dik kalemler.
    kar = M("Kalem_Karton", (0.70, 0.45, 0.03), 0.7)
    kutu((0.12, 0.08, 0.035), (0.20, 0.14, T + 0.0175), kar, 0.003)
    don(kutu((0.12, 0.004, 0.08), (0.20, 0.183, T + 0.07), kar, 0.002), rx=-0.2)
    for i, rk in enumerate(("kirmizi", "mavi", "yesil", "sari", "turuncu", "mor", "kahve", "pembe")):
        boya_kalemi(0.155 + (i % 4) * 0.03, 0.125 + (i // 4) * 0.03, T + 0.004, 0, mt("boya_" + rk), boy=0.07 - 0.004 * (i % 3), dik=True)
    # Masada yarım kalmış çizim ve kısalmış siyah kalem.
    bas = kagit("uzun_adam", "yer", tohum=7)
    grupla(bas, (-0.03, -0.08, T), (0, 0, 0.12))
    boya_kalemi(0.10, -0.13, T + 0.0035, 2.4, mt("boya_siyah"), boy=0.045)
    return bitir("Calisma_Masasi_K3")


def poster():
    W, H = 0.60, 0.80
    kutu((W, KAGIT_K, H), (0, -KAGIT_K / 2, 0), mt("kagit"))
    b = lambda ad: mt("boya_" + ad)
    D = "duvar"
    c_kutu(0, -0.03, W - 0.03, H - 0.10, b("lacivert"), 0, D)             # uzay
    r = random.Random(5)
    for i in range(28):                                                    # yıldızlar
        c_disk(r.uniform(-0.27, 0.27), r.uniform(-0.36, 0.30), r.uniform(0.003, 0.006), mt("kumas_beyaz"), 1, D, kose=8)
    c_disk(0.18, 0.21, 0.10, b("sari"), 1, D, olcek=(1.0, 0.22))           # halkalı gezegen
    c_disk(0.18, 0.21, 0.055, b("turuncu"), 2, D)
    c_disk(-0.16, 0.24, 0.10, b("gri"), 1, D, olcek=(1.0, 0.30))           # uçan daire
    c_disk(-0.16, 0.265, 0.038, b("acikmavi"), 2, D, olcek=(1.0, 0.8))
    for k in range(4):
        c_disk(-0.22 + k * 0.04, 0.235, 0.006, b("sari"), 2, D, kose=8)
    # Robot: kırmızı gövde, sarı V göğüs, mavi kollar, boynuzlu kask.
    for s in (-1, 1):
        c_kutu(s * 0.05, -0.26, 0.06, 0.14, b("gri"), 2, D)
        c_kutu(s * 0.056, -0.335, 0.085, 0.035, b("mavi"), 3, D)
        c_kutu(s * 0.14, -0.07, 0.05, 0.17, b("mavi"), 2, D, s * -0.15)
        c_kutu(s * 0.155, -0.17, 0.06, 0.05, b("gri"), 3, D)
        c_kutu(s * 0.12, 0.005, 0.08, 0.05, b("gri"), 3, D)
        c_poly([(s * 0.02, 0.10), (s * 0.045, 0.10), (s * 0.075, 0.165)], b("sari"), 3, D)
    c_kutu(0, -0.175, 0.16, 0.05, b("mavi"), 2, D)
    c_kutu(0, -0.07, 0.20, 0.17, b("kirmizi"), 2, D)
    c_poly([(-0.08, 0.0), (0.08, 0.0), (0.0, -0.07)], b("sari"), 3, D)
    c_kutu(0, 0.06, 0.09, 0.08, b("gri"), 2, D)
    c_kutu(0, 0.065, 0.07, 0.02, b("sari"), 3, D)
    c_kutu(0, 0.34, W - 0.06, 0.08, b("sari"), 1, D)                      # başlık bandı ve "harfler"
    for i in range(7):
        c_kutu(-0.21 + i * 0.07, 0.34, 0.045, 0.05, b("kirmizi"), 2, D)
    for sx in (-1, 1):                                                     # köşelerde bant
        for sz in (-1, 1):
            c_kutu(sx * (W / 2 - 0.012), sz * (H / 2 - 0.012), 0.05, 0.018, mt("bant"), 5, D, sx * sz * 0.7)
    # Futbol flaması: sarı-kırmızı üçgen, çubuğu ve ipiyle bir çiviye asılı.
    ah = mt("ahsap_orta")
    kutu((0.012, 0.012, 0.28), (0.36, -0.008, 0.08), ah, 0.003)
    prizma([(0.366, 0.08), (0.366, 0.20), (0.84, 0.08)], "xz", -0.003, -0.0045, M("Flama_Sari", (0.75, 0.52, 0.03), 0.8))
    prizma([(0.366, -0.04), (0.366, 0.08), (0.84, 0.08)], "xz", -0.003, -0.0045, M("Flama_Kirmizi", (0.45, 0.03, 0.02), 0.8))
    kutu((0.03, 0.002, 0.24), (0.382, -0.0055, 0.08), mt("kumas_beyaz"))
    cubuk((0.84, -0.009, 0.08), (0.85, -0.009, 0.03), 0.003, M("Flama_Sari", (0.75, 0.52, 0.03), 0.8), 6)
    kure(0.008, (0.85, -0.009, 0.025), M("Flama_Sari", (0.75, 0.52, 0.03), 0.8), seg=8, halka=6)
    cubuk((0.36, -0.006, 0.22), (0.36, -0.006, 0.30), 0.0015, mt("ic_koyu"), 5)   # ip
    dik_disk(0.004, 0.012, (0.36, -0.006, 0.30), mt("celik"), 8)             # çivi
    return bitir("Poster_K3", "duvar")


def yerdeki():
    plan = [(-0.28, 0.14, "uzun_adam", 0.4, 11), (-0.04, 0.18, "karalama", -0.3, 12), (0.22, 0.13, "uzun_adam", 0.9, 13),
            (-0.22, -0.12, "dokuz_kat", -0.6, 14), (0.04, -0.07, "uzun_adam", 0.2, 15), (0.28, -0.14, "bos", 1.4, 16),
            (0.00, 0.04, "pencere", 2.6, 17), (0.30, 0.02, "uzun_adam", -1.2, 18)]
    for i, (x, y, tur, a, t) in enumerate(plan):
        bas = kagit(tur, "yer", tohum=t)
        grupla(bas, (x, y, i * 0.002), (0, 0, a))
    # Dağınık kalemler: üç kısalmış siyah, kırık bir siyah parça, kırmızı ve mavi.
    for (x, y, a, rk, boy) in ((0.43, -0.02, 0.4, "siyah", 0.05), (-0.42, 0.00, 2.0, "siyah", 0.035), (0.40, 0.26, 1.1, "siyah", 0.06),
                              (-0.37, -0.27, 0.8, "siyah", 0.02), (0.14, -0.30, 2.8, "kirmizi", 0.085), (-0.14, 0.31, 0.2, "mavi", 0.08)):
        boya_kalemi(x, y, 0.0, a, mt("boya_" + rk), boy=boy)
    return bitir("Yerdeki_Cizimler_K3")


def gece_lambasi():
    # 1990'ların küçük çocuk odası abajuru: krem sırlı, mavi şeritli seramik ayak, pirinç boyun, döner
    # anahtarlı bakalit duy, mum ampul, soluk mavi pilili kumaş abajur (kenarları krem örgü şeritli).
    # Kordonu krem plastik yassı kablo: ortasında makaralı ara anahtar, ucunda yuvarlak fiş.
    # Yıpranma: abajur biraz yamulmuş, ampulün üstüne gelen yeri sararmış, ayağın dibinden sır atmış.
    sir = M("Gece90_Seramik", (0.68, 0.60, 0.44), 0.25)          # krem sırlı seramik (parlak)
    kobalt = M("Gece90_Kobalt", (0.03, 0.07, 0.32), 0.3)         # elle boyanmış kobalt mavisi şerit
    biskuvi = M("Gece90_Biskuvi", (0.50, 0.37, 0.24), 0.95)      # sırsız pişmiş toprak (sır atan yerde)
    pirinc, bakalit = mt("pirinc"), M("Gece90_Bakalit", (0.03, 0.022, 0.016), 0.4)
    kumas = M("Gece90_Abajur", (0.30, 0.44, 0.60), 0.95)         # soluk açık mavi pilili kumaş
    sarari = M("Gece90_AbajurLeke", (0.40, 0.40, 0.33), 0.95)    # ampul ısısıyla sararmış kumaş (mavinin üstüne sarı bindirmiş)
    galon = M("Gece90_Galon", (0.68, 0.62, 0.48), 0.9)           # krem örgülü kenar şeridi
    kablo = M("Gece90_Kablo", (0.66, 0.62, 0.52), 0.45)          # sararmış krem plastik (kablo, anahtar, fiş)

    # --- Seramik ayak: yan kesiti döndürülerek kurulur (alttan yukarı: taban, göbek, boyun) ---
    ayak = torna90([(0.0, 0.0), (0.035, 0.0), (0.038, 0.002), (0.039, 0.007), (0.036, 0.011), (0.039, 0.016),
                    (0.044, 0.028), (0.046, 0.040), (0.044, 0.052), (0.037, 0.063), (0.026, 0.072),
                    (0.018, 0.078), (0.017, 0.082), (0.020, 0.085), (0.020, 0.088), (0.0, 0.088)], sir, 32)
    yonga90(ayak, (0.026, -0.027, 0.008), (0.009, 0.009, 0.007), biskuvi)    # ön sağ dipten sır atmış
    halka = lambda r, z, n=32: [(r * math.cos(2 * math.pi * k / n), r * math.sin(2 * math.pi * k / n), z) for k in range(n)]
    supur(halka(0.0462, 0.040), serit_kesit(0.0012, 0.008), kobalt, kapali=True)   # göbekteki geniş mavi şerit
    supur(halka(0.0392, 0.007), serit_kesit(0.0010, 0.003), kobalt, kapali=True)   # tabandaki ince şerit
    # --- Pirinç boyun, bakalit duy ve döner anahtarı, mum ampul ---
    torna90([(0.0, 0.086), (0.010, 0.086), (0.010, 0.090), (0.006, 0.092), (0.006, 0.104), (0.0, 0.104)], pirinc, 16)
    torna90([(0.0, 0.102), (0.012, 0.102), (0.013, 0.106), (0.013, 0.124), (0.010, 0.127), (0.0, 0.127)], bakalit, 20)
    yatik_sil(0.0035, 0.010, (0.017, 0.0, 0.114), "x", pirinc, 10)            # duyun yanındaki döner anahtar
    kure(0.004, (0.023, 0.0, 0.114), pirinc, seg=10, halka=6)
    torna90([(0.0, 0.127), (0.006, 0.127), (0.009, 0.134), (0.009, 0.143), (0.005, 0.150), (0.0, 0.153)],
            M("Gece90_Ampul", (0.82, 0.78, 0.66), 0.3), 16)                   # buzlu mum ampul

    # --- Abajur: önce dik kurulur, sonra hafifçe yamultulur (yıllarca itilip kakılmış) ---
    # Dönme noktası duyun üstü (z = 0,12): parçalar bu noktaya göre kurulur, grupla ile döndürülüp geri konur.
    oz = -0.12                                                    # her z'den çıkarılan dönme noktası yüksekliği
    bas = len(P)
    za, zu, ra, ru = 0.084 + oz, 0.158 + oz, 0.066, 0.038        # abajurun alt/üst yüksekliği ve yarıçapı
    pilili_abajur(ra, ru, za, zu, 36, 0.0035, 0.0012, kumas, leke=(sarari, -math.pi / 2 + 0.45, 1.1))
    supur(halka(ra + 0.0006, za + 0.002, 48), daire_kesit(0.0022, 6), galon, kapali=True)   # alt kenar örgüsü
    supur(halka(ru + 0.0006, zu - 0.0015, 36), daire_kesit(0.0018, 6), galon, kapali=True)  # üst kenar örgüsü
    for k in range(3):                                            # üç tel kol abajuru ortadaki rondelaya bağlar
        a = 2 * math.pi * k / 3 + 0.3
        cubuk((0.0, 0.0, zu + 0.002), (0.035 * math.cos(a), 0.035 * math.sin(a), zu - 0.002), 0.0011, pirinc, 6)
    kure(0.0055, (0.0, 0.0, zu + 0.005), pirinc, seg=12, halka=8)  # tepedeki pirinç topuz
    grupla(bas, (0.0, 0.0, -oz), (0.07, -0.04, 0.0))               # öne ve yana hafifçe yamulmuş, yerine konur

    # --- Kordon: arkadan çıkar, yerde kıvrılır; ortada ara anahtar, ucunda yuvarlak fiş ---
    supur(egri([(0.0, 0.030, 0.003), (0.006, 0.055, 0.0016), (0.017, 0.075, 0.0016), (0.013, 0.092, 0.0016)], 4),
          serit_kesit(0.006, 0.0028), kablo)                      # yassı ikiz kablo (en 6 mm, kalınlık 2,8 mm)
    kutu((0.017, 0.030, 0.011), (0.013, 0.106, 0.0055), kablo, 0.003)      # makaralı ara anahtarın gövdesi
    yatik_sil(0.0045, 0.010, (0.013, 0.106, 0.010), "x", M("Gece90_Makara", (0.12, 0.06, 0.03), 0.5), 12)   # kahverengi makara
    supur(egri([(0.013, 0.121, 0.0016), (0.008, 0.135, 0.0016), (-0.002, 0.145, 0.0016), (-0.006, 0.152, 0.004)], 4),
          serit_kesit(0.006, 0.0028), kablo)
    koni(0.006, 0.012, 0.010, (-0.006, 0.156, 0.016), kablo, 12, rot=(-math.pi / 2, 0, 0))   # fişin kablo girişi (yumuşak boyun)
    yatik_sil(0.016, 0.022, (-0.006, 0.172, 0.016), "y", kablo, 20)        # yuvarlak fiş gövdesi
    for dy in (0.165, 0.176):                                     # parmakların kavradığı iki oluk
        yatik_sil(0.0163, 0.0015, (-0.006, dy, 0.016), "y", mt("ic_koyu"), 20)
    for dx in (-0.0095, 0.0095):                                  # iki yuvarlak pim (Türkiye prizi)
        cubuk((-0.006 + dx, 0.183, 0.016), (-0.006 + dx, 0.200, 0.016), 0.002, mt("krom"), 8)
    return bitir("Gece_Lambasi_K3")


# ---------- PİLİLİ KUMAŞ (iki perdenin ortak yardımcıları) ----------
# Eskiden perde kanatları yan yana dizilmiş ince kutulardı; aralarındaki boşluk yüzünden kumaş değil,
# jaluzi çıtası gibi görünüyordu. Şimdi her kanat TEK PARÇA bir yüzey: genişliği boyunca yumuşak bir
# dalga (sinüs) çizer, yani kumaşın öne-arkaya kıvrılması gibi. Yaklaşım perde_kadife.py'deki
# pilili_yuzey ve kat5_esyalar.py'deki pilili ile aynı (noktaları ızgara gibi dizip from_pydata ile
# yüzey kurmak); fark, zikzak yerine yuvarlak sinüs kıvrımları ve alta doğru açılan, derinleşen pililer.

def perde_yuzeyi(x0, x1, ust, alt, y_orta, kivrim, genlik_ust, genlik_alt, acilma=1.0, tohum=0.0):
    """
    Bir perde kanadının "şekil kuralını" kurar ve bir fonksiyon olarak geri verir.
    Dönen nokta(tx, s) fonksiyonu kumaş üzerindeki bir noktanın (x, y, z) konumunu hesaplar:
      tx: kanadın genişliği boyunca 0 (sol kenar) ile 1 (sağ kenar) arası oran,
      s : yukarıdan aşağı 0 (üst kenar) ile 1 (etek) arası oran.
    Fonksiyon döndürmenin sebebi: aynı kuralı hem kumaşın kendisi, hem etek şeridi, hem de kumaşın
    üstündeki bulut desenleri kullansın; hepsi kıvrımları birebir takip etsin.
    ust / alt: üst ve alt kenarın z'si; sayı ya da tx alan bir fonksiyon (fırfır eteği gibi dalgalı kenar için).
    kivrim: kanattaki tam dalga sayısı. genlik_ust/alt: kıvrımın öne-arkaya genliği (metre), üstte ve altta.
    acilma: etekte genişliğin üst genişliğe oranı (1,25 = altta %25 açılır).
    tohum: kıvrımlara küçük düzensizlik veren sabit sayı; her kanat biraz farklı dökülsün.
    """
    orta = (x0 + x1) / 2                                          # kanadın ortası: açılma bunun etrafında olur
    ust_f = ust if callable(ust) else (lambda tx: ust)             # sayı verildiyse her tx için aynı z'yi veren fonksiyona çevir
    alt_f = alt if callable(alt) else (lambda tx: alt)             # alt kenar için de aynısı

    def oran(s):
        """s yüksekliğinde genişliğin üst genişliğe oranı. s**1,5: açılma çoğunlukla aşağıda olur."""
        return 1.0 + (acilma - 1.0) * s ** 1.5

    def nokta(tx, s):
        x = orta + (x0 + (x1 - x0) * tx - orta) * oran(s)         # üstteki x'i ortadan dışa doğru açarak etekteki x'i bul
        zu, za = ust_f(tx), alt_f(tx)                             # bu dikey çizginin üst ve alt ucu
        z = zu + (za - zu) * s                                    # aradaki yükseklik: doğrusal geçiş
        A = genlik_ust + (genlik_alt - genlik_ust) * s            # kıvrım aşağı indikçe derinleşir (kumaş ağırlığıyla açılır)
        A *= 1.0 + 0.18 * math.sin(2 * math.pi * kivrim * tx / 2.3 + tohum)     # her kıvrım aynı derinlikte olmasın
        faz = 2 * math.pi * kivrim * tx                           # sinüsün açısı: tx 0→1 giderken "kivrim" kez tam tur
        faz += 0.35 * s * math.sin(2 * math.pi * 1.3 * tx + tohum)   # aşağıda kıvrımlar hafifçe yana kayar, cetvel gibi durmaz
        y = y_orta - A * math.sin(faz)                            # öne (-y) ve arkaya dalgalanan kumaş
        return (x, y, z)

    def en(s):
        """s yüksekliğinde kanadın genişliği (metre); bulut desenini doğru boyda çizmek için."""
        return (x1 - x0) * oran(s)

    def boy(tx):
        """tx çizgisinde kanadın yüksekliği (metre)."""
        return ust_f(tx) - alt_f(tx)

    return nokta, en, boy                                         # üç fonksiyonu birlikte ver


def kumas_orgusu(nokta, sutun, satirlar, mat, kal=0.003, y_kaydir=0.0, s0=0.0, s1=1.0):
    """
    nokta(tx, s) kuralından tek parça bir kumaş yüzeyi örer (sutun x satır dörtgen).
    satirlar: 0-1 arası dikey oranlar listesi (s0-s1 aralığına yayılır; etek şeridi yalnızca altı alır).
    kal: kumaş kalınlığı (Solidify); 0 verilirse tek yüz kalır (kumaşa dikili şerit gibi).
    y_kaydir: bütün yüzeyi öne (-) ya da arkaya (+) kaydırır; şerit kumaşın hemen önünde dursun diye.
    """
    noktalar, yuzler = [], []                                     # köşe listesi ve yüz listesi (from_pydata ister)
    ss = [s0 + (s1 - s0) * q for q in satirlar]                   # satır oranlarını istenen aralığa ölçekle
    for s in ss:                                                  # her satır (yukarıdan aşağı)
        for i in range(sutun + 1):                                # her sütun (soldan sağa); n dörtgen için n+1 köşe
            x, y, z = nokta(i / sutun, s)
            noktalar.append((x, y + y_kaydir, z))
    n = sutun + 1                                                 # bir satırdaki köşe sayısı
    for r in range(len(ss) - 1):
        for i in range(sutun):
            a = r * n + i                                         # dörtgenin sol üst köşesinin sırası
            # Sol üst → sol alt → sağ alt → sağ üst: bu sıra yüzün önünü odaya (-y) çevirir.
            yuzler.append((a, a + n, a + n + 1, a + 1))
    me = bpy.data.meshes.new("Kumas")
    me.from_pydata(noktalar, [], yuzler)                          # köşelerden ve yüzlerden mesh kur
    me.update()
    uv = me.uv_layers.new(name="UVMap")                           # doku koordinatı: diğer parçalarla aynı adda olsun ki birleşince kaybolmasın
    for d in me.loops:                                            # her yüz köşesine (loop) metre cinsinden (x, z) ver
        v = me.vertices[d.vertex_index].co
        uv.data[d.index].uv = (v.x, v.z)
    o = bpy.data.objects.new("Kumas", me)                         # mesh'i taşıyan nesne
    bpy.context.collection.objects.link(o)                        # sahneye ekle (yoksa birleştirilemez)
    ortak.ata(o, mat)
    if kal > 0:
        # Solidify kumaşa kalınlık verir; kalınlık yüzün ARKASINA (duvara doğru) eklenir, ön yüz yerinde kalır.
        # Böylece kenardan ve etekten bakınca kâğıt inceliğinde değil, kumaş gibi görünür. bitir() uygular.
        o.modifiers.new("Kalinlik", "SOLIDIFY").thickness = kal
    return ekle(o)


def bulut_deseni(nokta, en, boy, sutun, tx, s, mat, one=0.003):
    """
    Kumaşın üstüne basılı gibi duran beyaz bulut: eski bulutla aynı biçim (üç daire + düz taban),
    ama düz bir levha değil; kumaşın kıvrımlarıyla birlikte dalgalanır.
    Bulut, kumaşın SÜTUNLARIYLA aynı dikey çizgilerden örülür (sutun: kumaşın sütun sayısı). Böylece
    bulutun her yüzü altındaki kumaş yüzüne paralel olur: ışığı kumaşla aynı alır ve kıvrım tepelerinde
    kumaş bulutun içinden dışarı taşmaz (ilk denemedeki dairesel ızgara lekeli görünüyordu).
    tx, s: bulutun merkezinin kanat üzerindeki yeri (oran). one: kumaşın kaç metre önünde durduğu.
    """
    daireler = [(-0.035, 0.0, 0.035), (0.0, 0.018, 0.045), (0.04, 0.0, 0.035)]   # (u, w, yarıçap), u yatay, w dikey

    def icinde(u, w):
        """(u, w) noktası bulutun içinde mi?"""
        for (cu, cw, r) in daireler:
            if (u - cu) ** 2 + (w - cw) ** 2 <= r * r:
                return True
        return -0.055 <= u <= 0.055 and -0.033 <= w <= -0.003        # alttaki düz taban

    def aralik(u):
        """u dikey çizgisinde bulutun alt ve üst kenarı (w); çizgi bulutu kesmiyorsa None."""
        ws = [w * 0.0005 for w in range(-80, 140) if icinde(u, w * 0.0005)]   # yarım milimetrelik adımlarla tara
        return (ws[0], ws[-1]) if ws else None

    W = en(s)                                                     # bulut hizasında kanadın genişliği (metre)
    U0, U1 = -0.0695, 0.0745                                      # bulutun en sol ve en sağ noktası (u)
    # Dikey çizgiler: bulutun iki ucu + aradaki bütün kumaş sütunları (kumaşla aynı x'ler).
    us = [U0] + [(i / sutun - tx) * W for i in range(sutun + 1) if U0 < (i / sutun - tx) * W < U1] + [U1]
    SATIR = 3                                                     # her dikey çizgide alttan üste 3 aralık
    noktalar, yuzler = [], []
    for u in us:
        w0, w1 = aralik(u)
        for k in range(SATIR + 1):
            w = w0 + (w1 - w0) * k / SATIR                        # alt kenardan üst kenara eşit adımlar
            x, y, z = nokta(tx + u / W, s - w / boy(tx))          # yatay metreyi orana, yukarıyı küçülen s'ye çevir
            noktalar.append((x, y - one, z))                      # kumaşın biraz önüne
    n = SATIR + 1                                                 # bir dikey çizgideki köşe sayısı
    for c in range(len(us) - 1):
        for k in range(SATIR):
            a = c * n + k                                         # sol alt köşe
            # Sol alt → sağ alt → sağ üst → sol üst: yüz odaya (-y) bakar.
            yuzler.append((a, a + n, a + n + 1, a + 1))
    me = bpy.data.meshes.new("Bulut")
    me.from_pydata(noktalar, [], yuzler)
    me.update()
    me.uv_layers.new(name="UVMap")                                # birleştirmede UV katmanı eşleşsin
    o = bpy.data.objects.new("Bulut", me)
    bpy.context.collection.objects.link(o)
    ortak.ata(o, mat)
    return ekle(o)


def perde_cocuk():
    mavi, bulut = M("Perde_Bulut_Mavi", (0.30, 0.50, 0.75), 0.95), mt("kumas_beyaz")
    bey, mk = M("Cubuk_Beyaz", (0.78, 0.78, 0.75), 0.4), M("Topuz_Mavi", (0.05, 0.15, 0.45), 0.4)
    Y = -0.09                                                     # çubuğun duvardan uzaklığı (değişmedi)
    cubuk((-0.76, Y, 0), (0.76, Y, 0), 0.014, bey, 12)            # beyaz çubuk, z = 0'da (pivot yüksekliği)
    for s in (-1, 1):
        kure(0.03, (s * 0.79, Y, 0), mk)                          # uçlarda mavi topuzlar
        kutu((0.02, 0.09, 0.02), (s * 0.68, -0.045, 0), bey, 0.004)   # duvar tutucuları
        dik_disk(0.02, 0.006, (s * 0.68, -0.003, 0), bey)         # tutucunun duvardaki rozeti
    # İki kanat: (sol x, sağ x, kıvrım sayısı, tohum). Eskisi gibi sol kanat biraz daha geniş.
    SATIR = [0.0, 0.10, 0.25, 0.45, 0.65, 0.85, 1.0]              # yukarıdan aşağı satırlar (kıvrımlar boyca yavaş değişir, 6 aralık yeter)
    UST, ALT, SARKMA = -0.018, -1.33, 0.012                        # kumaş halkaların içinden başlar, etek eskisiyle aynı
    for (x0, x1, kv, th) in ((-0.74, -0.26, 6, 0.4), (0.30, 0.74, 5, 2.1)):
        # Üst kenar: halkaların tuttuğu noktalarda yüksek, iki halka arasında 1,2 cm sarkar (kumaş halkaya asılı).
        ust = (lambda tx, kv=kv: UST - SARKMA * (1 - math.cos(2 * math.pi * kv * tx)) / 2)
        nokta, en, boy = perde_yuzeyi(x0, x1, ust, ALT, Y, kv, 0.008, 0.011, acilma=1.06, tohum=th)
        kumas_orgusu(nokta, kv * 10, SATIR, mavi)                 # kıvrım başına 10 sütun: tepeler yuvarlak kalsın
        kumas_orgusu(nokta, kv * 10, [0.0, 1.0], bulut, kal=0.0, y_kaydir=-0.002, s0=1 - 0.04 / 1.31)   # beyaz etek şeridi
        for j in range(kv + 1):                                   # her kıvrımın başında bir halka (kumaş orada çubuk hizasında)
            x = nokta(j / kv, 0.0)[0]
            for xt in (-0.68, 0.68):                              # halka duvar tutucusuna çarpmasın: 1,6 cm yana it
                if abs(x - xt) < 0.016:
                    x = xt + math.copysign(0.016, x - xt)
            bpy.ops.mesh.primitive_torus_add(major_radius=0.02, minor_radius=0.004, location=(x, Y, 0), rotation=(0, math.pi / 2, 0), major_segments=10, minor_segments=4)
            bpy.ops.object.transform_apply(rotation=True)         # dönüşü meshe işle (birleştirince bozulmasın)
            ortak.ata(bpy.context.active_object, bey)
            ekle(bpy.context.active_object)
        # Bulutlar: eskisiyle aynı yerlerde; yer artık kanadın içindeki oran (tx) ve yükseklik oranı (s).
        for (u, z) in ((-0.62, -0.25), (-0.40, -0.55), (-0.60, -0.85), (-0.38, -1.12), (0.42, -0.30), (0.62, -0.62), (0.45, -0.95), (0.62, -1.20)):
            if x0 <= u <= x1:                                     # yalnızca bu kanada düşen bulutlar
                s = (UST - z) / (UST - ALT)
                tx = ((u - (x0 + x1) / 2) / (en(s) / (x1 - x0)) + (x0 + x1) / 2 - x0) / (x1 - x0)   # açılmayı geri alıp orana çevir
                bulut_deseni(nokta, en, boy, kv * 10, tx, s, bulut)
    return bitir("Perde_Cocuk_K3", "perde")


def karyola():
    ak, kum = mt("akcaagac"), M("Karyola_Kumas", (0.55, 0.30, 0.22), 0.9)
    kum_k = M("Karyola_KumasKoyu", (0.38, 0.19, 0.13), 0.9)
    ortu, bey = M("Ortu_Pastel", (0.66, 0.47, 0.46), 0.9), mt("kumas_beyaz")
    EN, BOY = 1.60, 2.00
    hy, ay = BOY / 2 - 0.03, -BOY / 2 + 0.03
    for x in (-0.77, 0.77):
        kutu((0.06, 0.06, 1.0), (x, hy, 0.5), ak, 0.01)
        kutu((0.06, 0.06, 0.50), (x, ay, 0.25), ak, 0.01)
        kutu((0.03, BOY - 0.12, 0.18), (x - math.copysign(0.005, x), 0, 0.30), ak, 0.005)   # yan kasalar
    kutu((EN, 0.07, 0.08), (0, hy, 0.96), ak, 0.025)                      # başlık tepesi
    kutu((1.48, 0.07, 0.46), (0, hy, 0.66), kum, 0.03)                     # dolgulu pano
    for i in range(7):
        x = -0.63 + i * 0.21
        kutu((0.006, 0.004, 0.42), (x, hy - 0.0355, 0.66), kum_k)
        for z in (0.55, 0.77):
            if i < 6:
                kure(0.011, (x + 0.105, hy - 0.036, z), kum_k, seg=10, halka=6)
    kutu((1.48, 0.03, 0.24), (0, ay, 0.34), ak, 0.006)                     # ayakucu
    kutu((1.52, 0.035, 0.03), (0, ay, 0.47), ak, 0.01)
    kutu((1.50, 1.90, 0.20), (0, 0, 0.45), bey, 0.04)                      # şilte (üstü 0,55)
    # Örtü: Murat'ın yanı (+x) örtülü; Ayla'nın yanı (−x) aceleyle geri atılmış, rulo olmuş.
    kutu((0.76, 1.45, 0.03), (0.38, -0.215, 0.565), ortu, 0.012)
    kutu((0.76, 0.80, 0.03), (-0.38, -0.54, 0.565), ortu, 0.012)
    yatik_sil(0.045, 0.76, (-0.38, -0.12, 0.59), "x", ortu, 16)
    kutu((0.006, 1.45, 0.22), (0.757, -0.215, 0.45), ortu)
    kutu((0.006, 0.80, 0.22), (-0.757, -0.54, 0.45), ortu)
    kutu((1.52, 0.006, 0.20), (0, -0.953, 0.46), ortu)
    kutu((0.76, 0.12, 0.035), (0.38, 0.47, 0.57), bey, 0.012)              # çarşafın kıvrımı
    cicek = [M("Cicek_Lila", (0.28, 0.12, 0.40), 0.9), M("Cicek_Mint", (0.12, 0.36, 0.20), 0.9), M("Cicek_Sari", (0.72, 0.45, 0.06), 0.9)]
    yap = M("Cicek_Yaprak", (0.08, 0.22, 0.06), 0.9)
    r = random.Random(28)
    for (x0, x1, y0, y1, n) in ((0.04, 0.72, -0.90, 0.40, 18), (-0.72, -0.04, -0.90, -0.22, 9)):
        for i in range(n):
            x, y = r.uniform(x0, x1), r.uniform(y0, y1)
            sil(0.028, 0.001, (x, y, 0.5805), cicek[i % 3], 10)
            sil(0.009, 0.001, (x, y, 0.5815), cicek[(i + 2) % 3], 8)
            don(elips(0.03, 0.014, 0.001, (x + 0.03, y - 0.012, 0.5805), yap, 8), rz=r.uniform(0, 3))
    for (x, a, h) in ((-0.37, 0.05, 0.14), (0.37, -0.03, 0.12)):          # iki yastık
        don(kutu((0.62, 0.36, h), (x, 0.74, 0.55 + h / 2), mt("kumas_beyaz"), 0.06), rz=a)
    return bitir("Karyola_K3")


def komodin():
    ak, ak_ic, pr = mt("akcaagac"), mt("akcaagac_ic"), mt("pirinc")
    EN, DER, H = 0.46, 0.40, 0.55
    g = ac.govde_levhalari(EN, DER, H, ak, taban=0.06, raflar=(0.37,))
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):                              # kısa konik ayaklar
        for y in (-DER / 2 + 0.03, DER / 2 - 0.03):
            g.append(ortak.silindir("Ayak", 0.018, 0.06, (x, y, 0.03), ak, 12))
    g.append(ortak.kutu("UstPervaz", (EN + 0.02, DER + 0.015, 0.012), (0, -0.0075, H + 0.006), ak, 0.004))
    # Alt bölmenin önü: sabit (süs) çekmece önü; gövdenin parçası, açılmaz.
    g.append(ac.levha("SusOn", -EN / 2 + 0.02, EN / 2 - 0.02, -DER / 2 - ac.KAL, -DER / 2, 0.085, 0.355, ak))
    g += ac.kulp(0, -DER / 2 - ac.KAL, 0.22, pr, tip="topuz")
    cek = ac.cekmece("Cekmece_1", 0, 0.378, EN - 0.04, 0.15, DER - 0.06, -DER / 2, ak_ic, ak, pr, "topuz")
    zt = 0.378 + 0.012 + 0.008
    BILGI.append("Cekmece_1 pivotu ic taban ortasi, Blender (0, %.3f, %.3f); ic taban z=%.3f, cekmece yan ust z=%.3f, "
                 "govde ust tablasinin alti z=%.3f (ic kullanilabilir yukseklik ~%.3f); ic en %.3f, ic derinlik %.3f"
                 % ((-DER / 2 + ac.KAL + (-DER / 2 + DER - 0.06)) / 2, zt, zt, 0.378 + 0.15 * 0.78, H - ac.KAL,
                    H - ac.KAL - zt, EN - 0.04 - 0.048, DER - 0.06 - ac.KAL - 0.012))
    bitir_acilir("Komodin_K3", g, [cek], 0.5, 1.2)


def besik():
    ah, bey = M("Besik_Ahsap", (0.28, 0.16, 0.06), 0.45), mt("kumas_beyaz")
    # İki uçta sallanma kızakları: yz düzleminde yay biçimli, altı kavisli.
    for x in (-0.40, 0.40):
        alt = [(y, 0.06 * (y / 0.25) ** 2) for y in [(-0.25 + 0.5 * i / 10) for i in range(11)]]
        prizma(alt + [(0.25, 0.12), (-0.25, 0.12)], "yz", x - 0.015, x + 0.015, ah)
        kutu((0.03, 0.06, 0.12), (x, 0, 0.16), ah, 0.006)                  # kızaktan gövdeye ayak
    # Uç panoları: başucu yüksek ve kemerli, ayakucu alçak.
    for (x, ust, zc) in ((0.39, 0.62, 0.57), (-0.39, 0.55, 0.45)):
        kutu((0.025, 0.46, ust - 0.20), (x, 0, (ust + 0.20) / 2), ah, 0.005)
        yatik_sil(0.23, 0.025, (x, 0, zc), "x", ah, 32)
    kutu((0.76, 0.44, 0.02), (0, 0, 0.21), ah, 0.004)                      # taban
    for y in (-0.22, 0.22):
        kutu((0.78, 0.03, 0.03), (0, y, 0.55), ah, 0.006)                  # üst korkuluk
        kutu((0.78, 0.03, 0.03), (0, y, 0.215), ah, 0.006)
        for i in range(11):
            sil(0.01, 0.31, (-0.33 + i * 0.066, y, 0.38), ah, 8)
    kutu((0.74, 0.42, 0.06), (0, 0, 0.25), bey, 0.02)                      # minder
    kutu((0.22, 0.30, 0.05), (0.26, 0, 0.30), bey, 0.02)                   # küçük yastık
    bat = M("Besik_Battaniye", (0.70, 0.62, 0.30), 0.95)
    don(kutu((0.36, 0.34, 0.03), (-0.06, 0.01, 0.295), bat, 0.012), rz=0.12)
    don(kutu((0.16, 0.20, 0.05), (0.10, 0.06, 0.31), bat, 0.02), rz=-0.4)  # buruşuk kenar
    for i in range(4):                                                     # battaniyede kareler
        kutu((0.06, 0.06, 0.001), (-0.18 + i * 0.08, -0.05 + (i % 2) * 0.08, 0.3106), M("Battaniye_Kare", (0.35, 0.50, 0.70), 0.95))
    return bitir("Besik_K3")


def sifonyer():
    ak, pr = mt("akcaagac"), mt("pirinc")
    ayna = M("Ayna", (0.55, 0.58, 0.60), 0.03, 1.0)
    kutu((0.96, 0.42, 0.05), (0, 0, 0.025), M("Sifonyer_Baza", (0.18, 0.10, 0.04), 0.45), 0.005)
    kutu((1.0, 0.45, 0.80), (0, 0, 0.45), ak, 0.008)
    kutu((1.03, 0.47, 0.025), (0, -0.005, 0.8625), ak, 0.008)
    yf = -0.225 - 0.0075
    for (z0, z1) in ((0.07, 0.25), (0.27, 0.45), (0.47, 0.63)):            # geniş çekmece önleri (süs)
        kutu((0.97, 0.015, z1 - z0), (0, yf, (z0 + z1) / 2), ak, 0.004)
        for x in (-0.25, 0.25):
            ac_k = ac.kulp(x, yf - 0.0075, (z0 + z1) / 2, pr, tip="topuz")
            for n in ac_k:
                ekle(n)
    for s in (-1, 1):                                                      # üstte iki küçük çekmece
        kutu((0.475, 0.015, 0.17), (s * 0.2425, yf, 0.74), ak, 0.004)
        for n in ac.kulp(s * 0.2425, yf - 0.0075, 0.74, pr, tip="topuz"):
            ekle(n)
    T = 0.875
    # Devrilebilir ayna: iki dikme arasında, pimlerinden geriye 6° yatık.
    for s in (-1, 1):
        kutu((0.03, 0.04, 0.56), (s * 0.355, 0.17, T + 0.28), ak, 0.006)
        yatik_sil(0.009, 0.03, (s * 0.335, 0.17, T + 0.33), "x", pr, 12)
    bas = len(P)
    kutu((0.64, 0.03, 0.60), (0, 0, 0), ak, 0.01)
    kutu((0.56, 0.005, 0.52), (0, -0.017, 0), ayna)
    grupla(bas, (0, 0.17, T + 0.33), (-0.10, 0, 0))
    kutu((0.80, 0.30, 0.002), (0, -0.04, T + 0.001), mt("kumas_beyaz"))    # dantel örtü
    # Parfüm şişeleri, pudra kutusu, bebek pudrası, mücevher kutusu, fırça.
    pembe_cam, amber = M("Sise_Pembe", (0.60, 0.25, 0.30), 0.05), M("Sise_Amber", (0.45, 0.20, 0.03), 0.05)
    yatik_sil(0.045, 0.025, (-0.30, -0.05, T + 0.047), "y", pembe_cam, 24)
    sil(0.01, 0.02, (-0.30, -0.05, T + 0.10), pr, 10)
    kutu((0.05, 0.03, 0.11), (-0.20, -0.02, T + 0.057), amber, 0.008)
    sil(0.012, 0.025, (-0.20, -0.02, T + 0.125), mt("siyah_plastik"), 12)
    sil(0.025, 0.08, (-0.12, -0.08, T + 0.042), mt("cam"), 16)
    kure(0.022, (-0.075, -0.10, T + 0.022), M("Puskurtec", (0.35, 0.05, 0.08), 0.5))
    cubuk((-0.12, -0.08, T + 0.07), (-0.075, -0.10, T + 0.03), 0.003, M("Puskurtec", (0.35, 0.05, 0.08), 0.5), 6)
    sil(0.035, 0.12, (0.12, -0.05, T + 0.062), M("Bebek_Pudrasi", (0.80, 0.80, 0.76), 0.35), 20, 0.008)
    sil(0.02, 0.015, (0.12, -0.05, T + 0.13), M("Pudra_Kapak", (0.70, 0.35, 0.45), 0.35), 16)
    kutu((0.14, 0.09, 0.06), (0.28, -0.02, T + 0.032), M("Mucevher_Kutusu", (0.25, 0.03, 0.06), 0.6), 0.008)
    kutu((0.145, 0.095, 0.015), (0.28, -0.02, T + 0.068), M("Mucevher_Kutusu", (0.25, 0.03, 0.06), 0.6), 0.005)
    don(kutu((0.20, 0.05, 0.03), (0.02, -0.14, T + 0.016), M("Firca", (0.20, 0.08, 0.03), 0.5), 0.012), rz=0.3)
    return bitir("Sifonyer_K3")


def perde_odaA():
    lila, lila_k = M("Perde_Lila", (0.42, 0.26, 0.46), 0.95), M("Perde_LilaKoyu", (0.30, 0.16, 0.34), 0.95)
    pr = mt("pirinc")
    Y = -0.09
    cubuk((-0.76, Y, 0), (0.76, Y, 0), 0.010, pr, 12)
    for s in (-1, 1):
        koni(0.018, 0.004, 0.04, (s * 0.78, Y, 0), pr, 12, rot=(0, s * math.pi / 2, 0))
        kutu((0.02, 0.15, 0.02), (s * 0.70, -0.075, 0.02), pr, 0.004)
        dik_disk(0.02, 0.006, (s * 0.70, -0.003, 0.02), pr)
    # İki kanat: üstte korniş fırfırının arkasında toplanmış, aşağı indikçe açılan (%25) ve derinleşen
    # tek parça pilili kumaş; eteğinde aynı kumaştan kıvrık bir baskı şeridi.
    SATIR = [0.0, 0.10, 0.25, 0.45, 0.65, 0.85, 1.0]                     # 6 dikey aralık
    UST, ALT = -0.02, -1.38                                                # eski şeritlerin üst ve alt ucu
    for (x0, x1, th) in ((-0.74, -0.34, 1.1), (0.34, 0.74, 3.7)):
        nokta, en, boy = perde_yuzeyi(x0, x1, UST, ALT, Y - 0.01, 5, 0.006, 0.012, acilma=1.25, tohum=th)
        kumas_orgusu(nokta, 50, SATIR, lila)                               # 5 kıvrım x 10 sütun
        kumas_orgusu(nokta, 50, [0.0, 1.0], lila, kal=0.0, y_kaydir=-0.0025, s0=1 - 0.045 / (UST - ALT))   # etek baskısı
    # Fırfırlı korniş: tek parça, sık büzgülü koyu lila fırfır; eteği ponponların altında sarkan kavislerle
    # (13 kavis, her birinin en alçak noktasında bir ponpon). Üstünde düz büzgü bandı.
    PX = [-0.72 + i * 0.12 for i in range(13)]                             # ponponların x'i (eskisiyle aynı)
    # Kavisli alt kenar: tx'ten x'e geçilir (x = -0,78 + 1,56 tx). cos² ponpon hizalarında 1 (kenar 3,5 cm
    # aşağı sarkar, z = -0,22), iki ponpon arasında 0'dır (kenar z = -0,185'te kalkar); 12 cm'de bir tekrarlar.
    etek = (lambda tx: -0.185 - 0.035 * math.cos(math.pi * (-0.78 + 1.56 * tx + 0.72) / 0.12) ** 2)
    firfir, _, _ = perde_yuzeyi(-0.78, 0.78, 0.035, etek, Y - 0.05, 16, 0.010, 0.010, tohum=0.7)
    kumas_orgusu(firfir, 16 * 8, [0.0, 0.5, 1.0], lila_k)                  # bandın arkasından başlar; kavisli etek için kıvrım başına 8 sütun
    # Büzgü bandı: çok sığ kıvrımlı, tutucuların ucunun (y = -0,15) önünde; eski bant gibi z 0,00-0,04.
    bant, _, _ = perde_yuzeyi(-0.785, 0.785, 0.04, 0.0, Y - 0.065, 16, 0.003, 0.003, tohum=0.7)
    kumas_orgusu(bant, 16 * 4, [0.0, 1.0], lila_k)                         # sığ kıvrım: kıvrım başına 4 sütun yeter
    for x in PX:                                                           # ponponlar: kavisin dibinde, kumaşın önünde
        tx = (x + 0.78) / 1.56
        _, y, z = firfir(tx, 1.0)
        kure(0.009, (x, y - 0.007, z - 0.006), lila, seg=8, halka=5)
    return bitir("Perde_OdaA_K3", "perde")


def kilim():
    kir, lac = M("Kilim_KirmiziTuruncu", (0.40, 0.08, 0.02), 0.95), M("Kilim_Lacivert", (0.03, 0.04, 0.12), 0.95)
    krem, sari = M("Kilim_Krem", (0.70, 0.62, 0.45), 0.95), M("Kilim_Sari", (0.65, 0.40, 0.05), 0.95)
    EN, BOY, T = 1.10, 0.65, 0.007
    kutu((EN, BOY, T), (0, 0, T / 2), kir)
    z = [T + 0.0005 + 0.0008 * i for i in range(4)]
    for s in (-1, 1):
        kutu((EN - 0.04, 0.06, 0.001), (0, s * (BOY / 2 - 0.05), z[0]), lac)
        kutu((0.06, BOY - 0.04, 0.001), (s * (EN / 2 - 0.05), 0, z[0]), lac)
    for i in range(16):                                                    # bordürde krem üçgenler
        x = -EN / 2 + 0.09 + i * (EN - 0.18) / 15
        for s in (-1, 1):
            prizma([(x - 0.02, s * (BOY / 2 - 0.07)), (x + 0.02, s * (BOY / 2 - 0.07)), (x, s * (BOY / 2 - 0.03))], "xy", z[1], z[1] + 0.001, krem)
    for x in (-0.33, 0.0, 0.33):                                           # üç kancalı baklava
        don(kutu((0.20, 0.20, 0.001), (x, 0, z[0]), lac), rz=math.pi / 4)
        don(kutu((0.14, 0.14, 0.001), (x, 0, z[1]), krem), rz=math.pi / 4)
        don(kutu((0.08, 0.08, 0.001), (x, 0, z[2]), sari), rz=math.pi / 4)
        don(kutu((0.03, 0.03, 0.001), (x, 0, z[3]), lac), rz=math.pi / 4)
        for (dx, dy) in ((0.155, 0), (-0.155, 0), (0, 0.155), (0, -0.155)):
            kutu((0.025, 0.025, 0.001), (x + dx, dy, z[0]), krem)
    for x in (-0.165, 0.165):                                              # aralarda krem artı
        kutu((0.05, 0.012, 0.001), (x, 0.12, z[0]), krem)
        kutu((0.012, 0.05, 0.001), (x, 0.12, z[0]), krem)
        kutu((0.05, 0.012, 0.001), (x, -0.12, z[0]), krem)
        kutu((0.012, 0.05, 0.001), (x, -0.12, z[0]), krem)
    for s in (-1, 1):                                                      # saçaklar
        for j in range(12):
            kutu((0.05, 0.012, 0.003), (s * (EN / 2 + 0.024), -BOY / 2 + 0.05 + j * (BOY - 0.1) / 11, 0.0015), krem)
    return bitir("Kilim_K3")


ESYALAR2 = {
    "yatak": yatak, "calisma": calisma, "poster": poster, "yerdeki": yerdeki, "gece_lambasi": gece_lambasi,
    "perde_cocuk": perde_cocuk, "karyola": karyola, "komodin": komodin, "besik": besik, "sifonyer": sifonyer,
    "perde_odaA": perde_odaA, "kilim": kilim,
}

if __name__ == "__main__":
    hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "yatak"
    ortak.sahneyi_temizle()
    ESYALAR2[hangi]()
