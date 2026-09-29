# kat3_esyalar_2.py
# 3. kat (#83), Kerem'in ailesinin dairesi: yatak odaları. Yardımcılar kat3_esyalar.py'den gelir.
#   blender -b --factory-startup --python kat3_esyalar_2.py -- <ad> [--onizleme dosya.png]
#
# ODA B (Kerem'in odası; gece uyanıp "dokuzuncu kattaki adamı" çizdiği yer)
#   yatak          Cocuk_Yatagi_K3    : mavi boyalı tek kişilik çocuk yatağı; yıldızlı örtü geri atılmış,
#                                       yastık, yastığa yaslı oyuncak ayı. Başucu +y tarafında.
#   calisma        Calisma_Masasi_K3  : beyaz-kırmızı küçük çalışma masası (çekmeceler süs); masa lambası,
#                                       boya kalemi kutusu ve yarım kalmış "uzun adam" çizimi.
#   poster         Poster_K3          : uzay robotlu çizgi film posteri ve yanında futbol flaması (duvar eşyası).
#   yerdeki        Yerdeki_Cizimler_K3: yere saçılmış sekiz kâğıt (çoğunda kara, uzun adam), dağınık kalemler.
#   gece_lambasi   Gece_Lambasi_K3    : ay biçimli küçük gece lambası, kablosu ve fişiyle.
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


def calisma():
    beyaz, kir = M("Calisma_Beyaz", (0.75, 0.74, 0.70), 0.4), M("Calisma_Kirmizi", (0.40, 0.04, 0.03), 0.4)
    kutu((0.90, 0.50, 0.025), (0, 0, 0.7075), beyaz, 0.006)
    kutu((0.90, 0.012, 0.027), (0, -0.256, 0.7075), kir, 0.003)            # ön kenar bandı
    kutu((0.025, 0.48, 0.695), (-0.4375, 0, 0.3475), beyaz, 0.004)          # sol yan tabla
    kutu((0.30, 0.48, 0.695), (0.30, 0, 0.3475), beyaz, 0.004)             # sağda çekmece gövdesi
    for (z, h) in ((0.585, 0.19), (0.375, 0.21), (0.135, 0.25)):           # çekmece önleri (süs, açılmaz)
        kutu((0.28, 0.015, h - 0.012), (0.30, -0.2475, z), kir, 0.004)
        kutu((0.09, 0.014, 0.014), (0.30, -0.262, z + h / 2 - 0.045), beyaz, 0.004)
    kutu((0.56, 0.015, 0.30), (-0.13, 0.225, 0.54), beyaz, 0.004)          # arka etek
    T = 0.72
    # 90'lar yaylı masa lambası (yeşil metal).
    yes = M("Lamba_Yesil", (0.04, 0.18, 0.08), 0.35, 0.4)
    sil(0.065, 0.02, (-0.33, 0.14, T + 0.01), yes, 24, 0.004)
    cubuk((-0.33, 0.14, T + 0.02), (-0.30, 0.10, T + 0.30), 0.008, yes)
    kure(0.013, (-0.30, 0.10, T + 0.30), mt("krom"))
    cubuk((-0.30, 0.10, T + 0.30), (-0.18, 0.02, T + 0.36), 0.008, yes)
    sh = koni(0.075, 0.025, 0.12, (-0.16, 0.0, T + 0.31), yes, 24, rot=(-0.35, 0, 0))
    kes = koni(0.068, 0.018, 0.12, (-0.16, 0.0, T + 0.30), yes, 24, rot=(-0.35, 0, 0))
    P.pop()
    ortak.oy(sh, kes)
    kure(0.028, (-0.16, 0.0, T + 0.30), M("Ampul", (0.80, 0.75, 0.55), 0.2))
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
    beyaz, ay = mt("beyaz_plastik"), M("Ay_Sari", (0.80, 0.68, 0.28), 0.4)
    sil(0.05, 0.02, (0, 0, 0.01), beyaz, 28, 0.006)
    m = dik_disk(0.06, 0.03, (0, 0, 0.085), ay, 36)
    kes = yatik_sil(0.05, 0.04, (0.03, 0, 0.105), "y", ay, 36)
    P.pop()
    ortak.oy(m, kes)                                                       # dolunaydan hilal oy
    prizma(yildiz(0.034, 0.038, 0.018), "xz", -0.016, -0.024, ay)          # önde küçük yıldız
    kutu((0.012, 0.012, 0.012), (0.0, 0.045, 0.006), beyaz, 0.003)         # kablo çıkışı
    for (a, b) in (((0.0, 0.05, 0.004), (0.02, 0.10, 0.004)), ((0.02, 0.10, 0.004), (0.01, 0.16, 0.004))):
        cubuk(a, b, 0.003, beyaz, 6)
    kutu((0.035, 0.03, 0.02), (0.01, 0.175, 0.01), beyaz, 0.004)           # fiş
    for x in (0.0, 0.02):
        cubuk((x, 0.19, 0.01), (x, 0.21, 0.01), 0.002, mt("krom"), 6)
    return bitir("Gece_Lambasi_K3")


def perde_cocuk():
    mavi, bulut = M("Perde_Bulut_Mavi", (0.30, 0.50, 0.75), 0.95), mt("kumas_beyaz")
    bey, mk = M("Cubuk_Beyaz", (0.78, 0.78, 0.75), 0.4), M("Topuz_Mavi", (0.05, 0.15, 0.45), 0.4)
    Y = -0.09
    cubuk((-0.76, Y, 0), (0.76, Y, 0), 0.014, bey, 12)
    for s in (-1, 1):
        kure(0.03, (s * 0.79, Y, 0), mk)
        kutu((0.02, 0.09, 0.02), (s * 0.68, -0.045, 0), bey, 0.004)
        dik_disk(0.02, 0.006, (s * 0.68, -0.003, 0), bey)
    yc = Y - 0.018 - 0.006 - 0.002
    for (x0, x1, n) in ((-0.74, -0.26, 8), (0.30, 0.74, 7)):
        w = (x1 - x0) / n
        for i in range(n):
            x = x0 + (i + 0.5) * w
            kutu((w + 0.012, 0.01, 1.30), (x, Y - 0.012 + (0.006 if i % 2 else -0.006), -0.68), mavi)
            bpy.ops.mesh.primitive_torus_add(major_radius=0.02, minor_radius=0.004, location=(x, Y, 0), rotation=(0, math.pi / 2, 0), major_segments=12, minor_segments=6)
            bpy.ops.object.transform_apply(rotation=True)
            ortak.ata(bpy.context.active_object, bey)
            ekle(bpy.context.active_object)
    for (u, z) in ((-0.62, -0.25), (-0.40, -0.55), (-0.60, -0.85), (-0.38, -1.12), (0.42, -0.30), (0.62, -0.62), (0.45, -0.95), (0.62, -1.20)):
        dik_disk(0.035, 0.002, (u - 0.035, yc, z), bulut, 16)
        dik_disk(0.045, 0.002, (u, yc, z + 0.018), bulut, 16)
        dik_disk(0.035, 0.002, (u + 0.04, yc, z), bulut, 16)
        kutu((0.11, 0.002, 0.03), (u, yc, z - 0.018), bulut)
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
    for (x0, x1, n) in ((-0.74, -0.34, 8), (0.34, 0.74, 8)):              # iki kanat, altta hafif açılan pililer
        c, w = (x0 + x1) / 2, (x1 - x0) / n
        for i in range(n):
            xt = x0 + (i + 0.5) * w
            xb = c + (xt - c) * 1.25
            serit_kumas(xt, -0.02, xb, -1.38, w + 0.014, 0.012, Y - 0.014 + (0.007 if i % 2 else -0.007), lila)
    kutu((1.58, 0.02, 0.04), (0, Y - 0.05, 0.02), lila_k, 0.006)           # fırfırlı korniş bandı
    for i in range(26):
        x = -0.78 + (i + 0.5) * 1.56 / 26
        L = 0.20 if i % 2 else 0.24
        kutu((0.066, 0.012, L), (x, Y - 0.052 + (0.006 if i % 2 else -0.006), 0.03 - L / 2), lila_k)
    for i in range(13):                                                    # alt kenarda ponponlar
        kure(0.009, (-0.72 + i * 0.12, Y - 0.06, -0.22), lila, seg=8, halka=6)
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
