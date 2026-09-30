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
