# kat7_esyalar_2.py
# 7. kat, Cemal'in dairesi (#83): 15-25 numaralı eşyalar. Yardımcılar (kutu, torna, yuzey, dondur, bitir...)
# kat7_esyalar.py'den gelir; hikâye ve kurallar orada.
#   blender -b --factory-startup --python kat7_esyalar_2.py -- <ad> [--onizleme yol.png]
#
# BANYO
#   tiras     Tiras_Takimi_K7      : çividen zincirle asılı yuvarlak tıraş aynası; altındaki emaye rafta ustura,
#                                    tıraş fırçası, limon kolonyası, tıraş sabunu kabı.
#   havlu     Havlu_K7             : duvar kancasında tek, solmuş bordo havlu (kancada toplanmış, iki kat sarkar).
#   kova      Kova_K7              : yeşil plastik kova (teli yana yatmış) ve yanında gümüş hamam tası.
# KİLER
#   raf       Raf_K7               : yeşilimsi gri metal raf; çamaşır suyu şişeleri, ampul kutuları, bez yığını,
#                                    yedek sigorta kutusu, boya tenekeleri.
#   merdiven  Merdiven_K7          : duvara dayalı, katlanmış alüminyum seyyar merdiven.
# ODA A (yatak odası)
#   yatak     Tek_Kisilik_Yatak_K7 : koyu yeşil boyalı demir çerçeveli tek kişilik yatak, dağınık battaniye (başlık +y).
#   komodin   Komodin_K7           : krem boyalı küçük komodin (süs, açılmaz); üstünde kurmalı çalar saat, sigara paketi.
#   koliler   Koliler_K7           : bantlanmış, kalemle yazılmış taşınma kolileri (üç üst üste, biri yanda).
# ODA B
#   sandik    Sandik_Cemal_K7      : AÇILIR (Govde + Kapak) demir şeritli koyu ceviz sandık; önde asma kilit
#                                    halkası ve kapaktan sarkan köprü; içi oyuk.
#   bavullar  Bavullar_K7          : kayışla bağlanmış iki dolu bavul (arkada dik, önde yatık).
#   duvar     Arastirma_Duvari_K7  : duvara iğnelenmiş gazete kupürleri, dokuz katlı bina kesiti çizimi,
#                                    el notları, bir fotoğraf ve iğneler arasında kırmızı ipler.

import os, sys, math, random
sys.path.append(os.path.dirname(__file__))
import ortak
import acilir_ortak as ac
import bpy
import kat7_esyalar as k7
from mathutils import Vector
from kat7_esyalar import p, kutu, sil, cubuk, kure, dik_disk, simit, torna, yuzey, dondur, tasi, bitir, seffaf

rnd = random.Random(77)


def uret(hangi):
    if hangi == "tiras":
        krom = ortak.malzeme("K7_Tiras_Krom", (0.55, 0.55, 0.53), puruz=0.25, metal=1.0)
        ayna = ortak.malzeme("K7_Tiras_Ayna", (0.60, 0.62, 0.62), puruz=0.04, metal=1.0)
        emaye = ortak.malzeme("K7_Tiras_Raf", (0.70, 0.68, 0.62), puruz=0.35)
        pas = ortak.malzeme("K7_Tiras_Pas", (0.20, 0.08, 0.03), puruz=0.9)
        boynuz = ortak.malzeme("K7_Ustura_Sap", (0.03, 0.022, 0.015), puruz=0.35)
        bakalit = ortak.malzeme("K7_Firca_Sap", (0.55, 0.45, 0.30), puruz=0.3)
        kil = ortak.malzeme("K7_Firca_Kil", (0.25, 0.20, 0.14), puruz=0.95)
        kolonya = seffaf(ortak.malzeme("K7_Kolonya_Cam", (0.50, 0.48, 0.16), puruz=0.05), 0.6)
        kirmizi = ortak.malzeme("K7_Kolonya_Kapak", (0.35, 0.04, 0.03), puruz=0.4)
        etiket = ortak.malzeme("K7_Kolonya_Etiket", (0.65, 0.55, 0.20), puruz=0.7)
        sabun = ortak.malzeme("K7_Tiras_Sabunu", (0.72, 0.70, 0.64), puruz=0.6)
        # Çivi ve iki zincir kolu: yuvarlak ayna çividen sarkar.
        cubuk((0, 0, 0.30), (0, -0.02, 0.305), 0.003, krom)
        sil("CiviBas", 0.005, 0.002, (0, -0.02, 0.305), krom, 10)
        dondur([p[-1]], 90, "X", (0, -0.02, 0.305))
        for yon in (-1, 1):
            cubuk((0, -0.018, 0.30), (yon * 0.07, -0.012, 0.215), 0.0015, krom, 5)
        # Ayna: krom çember, içinde ayna camı; kenarında pas lekesi.
        simit(0.12, 0.008, (0, -0.012, 0.11), krom, rot=(math.pi / 2, 0, 0), seg=(32, 8))
        dik_disk(0.118, 0.004, (0, -0.010, 0.11), ayna, 32)
        kutu("Pas", (0.03, 0.001, 0.02), (0.07, -0.0125, 0.03), pas)
        # Raf: emaye levha, iki konsol ayak; üstünde tıraş takımı.
        kutu("Raf", (0.40, 0.11, 0.012), (0, -0.055, -0.12), emaye, 0.003)
        for x in (-0.15, 0.15):
            kutu("KonsolDik", (0.012, 0.006, 0.08), (x, -0.003, -0.166), emaye)
            cubuk((x, -0.006, -0.20), (x, -0.095, -0.127), 0.004, emaye)
        z = -0.114
        u0 = len(p)
        kutu("UsturaSap", (0.13, 0.016, 0.012), (-0.11, -0.06, z + 0.006), boynuz, 0.004)
        kutu("UsturaAgiz", (0.04, 0.014, 0.004), (-0.035, -0.06, z + 0.006), krom, 0.001)
        dondur(p[u0:], 12, "Z", (-0.11, -0.06, z))
        sil("FircaSap", 0.014, 0.05, (-0.01, -0.055, z + 0.025), bakalit, 16)
        torna("FircaKil", [(0, 0.05), (0.016, 0.05), (0.022, 0.075), (0.02, 0.09), (0.01, 0.097), (0, 0.098)], (-0.01, -0.055, z), kil, 16)
        kutu("Kolonya", (0.05, 0.03, 0.12), (0.08, -0.055, z + 0.06), kolonya, 0.004)
        kutu("KolonyaEtiket", (0.04, 0.002, 0.05), (0.08, -0.0715, z + 0.05), etiket)
        sil("KolonyaBoyun", 0.008, 0.015, (0.08, -0.055, z + 0.1275), kolonya, 12)
        sil("KolonyaKapak", 0.011, 0.02, (0.08, -0.055, z + 0.145), kirmizi, 14)
        torna("SabunKabi", [(0, 0), (0.028, 0), (0.030, 0.03), (0.027, 0.03), (0.025, 0.004), (0, 0.004)], (0.15, -0.055, z), krom, 20)
        sil("Sabun", 0.025, 0.012, (0.15, -0.055, z + 0.01), sabun, 20)
        bitir("Tiras_Takimi_K7", 0.7, 1.0, kip="duvar")

    elif hangi == "havlu":
        demir = ortak.malzeme("K7_Havlu_Kanca", (0.50, 0.50, 0.48), puruz=0.3, metal=1.0)
        havlu = ortak.malzeme("K7_Havlu", (0.16, 0.035, 0.035), puruz=0.95)
        serit_m = ortak.malzeme("K7_Havlu_Serit", (0.45, 0.35, 0.22), puruz=0.95)
        dik_disk(0.018, 0.006, (0, -0.003, 0), demir, 16)
        cubuk((0, -0.006, 0), (0, -0.04, -0.005), 0.004, demir)
        cubuk((0, -0.04, -0.005), (0, -0.05, 0.015), 0.004, demir)

        # HAVLU TEK PARÇA KUMAŞ: eski sürümde iki ayrı düz levha ve üstte bir silindir vardı; sivri
        # omuzlu tek kırmızı biçim önlük gibi okunuyordu. Şimdi havlu, ikiye katlanıp kancanın üstünden
        # aşırılmış TEK bir yüzey: arka kat duvara yaslı iner, kancanın üstünden yuvarlak bir kıvrımla
        # döner, ön kat öne sarkar. Her "sütun" (a) havlunun enine bir çizgisidir; o çizgi kendi omuz
        # noktasından (katlama kenarı) aşağı iner. Ortadaki sütunlar kancada toplanır, dıştakiler
        # omuzdan yuvarlanarak düşer; böylece üstte büzgü, aşağıda açılan kıvrımlar oluşur.
        YARIM = 0.17            # havlunun aşağıdaki yarım eni (eski 0,34 m'lik en korunuyor)
        KAT = 0.011             # iki kat arasındaki mesafe aşağıda: iki kumaş kalınlığı + 3 mm boşluk
        KALIN = 0.008           # havlu kumaşının kalınlığı (havlu bez kalındır; ince levha gibi durmasın)

        def kolon(a):
            """a = -1 (sol kenar) .. +1 (sağ kenar). Bu sütunun katlama kenarındaki tepe noktası."""
            t = abs(a)                                           # kenara uzaklık (0 orta, 1 kenar)
            fi = t * math.pi / 2                                 # omuz açısı: 0 kancada, 90° yan kenarda
            # Omuz eğrisi bir elips yayı: tepede yatık, yana doğru dikleşir. Sivri üçgen omuz (önlük gibi
            # duruyordu) yerine kancanın üstünden yuvarlanıp düşen bir kubbe. exp terimi kancanın tam
            # üstünde küçük bir tepe bırakır: kumaş kancada tutuluyor, orada biraz yukarı çekilmiş.
            zt = -0.007 - 0.185 * (1 - math.cos(fi)) + 0.015 * math.exp(-(t / 0.12) ** 2)
            # Katlama kenarındaki iki kat arası: kancada kumaş kancanın sapını sarar (geniş, 2,6 cm),
            # omuzlarda yalnızca iki katın kalınlığı kadar kalır.
            ayrik = KAT + 0.015 * (1 - t) ** 2
            # Tepede sütunun x'i aynı elips yayından: kubbe yan kenarda tam enin %90'ına ulaşır.
            xt = math.copysign(YARIM * 0.9 * math.sin(fi), a)
            return t, zt, ayrik, xt

        def katman(a, z, on_kat):
            """Havlunun a sütununda, z yüksekliğinde arka (on_kat=False) ya da ön kattaki nokta."""
            t, zt, ayrik, xt = kolon(a)
            R = ayrik / 2                                        # katlama kıvrımının yarıçapı
            d = max(0.0, (zt - R) - z)                           # kıvrımın altından bu yana ne kadar inildi
            ara = KAT + (ayrik - KAT) * math.exp(-d / 0.05)      # iki kat aşağı indikçe birbirine yaklaşır
            # En: tepedeki toplanmış x'ten aşağıdaki tam ene yavaşça açılır (üstel yaklaşım, sivri köşe yok).
            x = xt + (a * YARIM - xt) * (1 - math.exp(-d / 0.08))
            # Kıvrımlar: kancadan yelpaze gibi açılan dikey dalgalar. a'ya bağlı oldukları için tepede
            # (x sıkışıkken) sık, aşağıda (x açılınca) geniş görünürler; tepede az, aşağıda derin.
            A = 0.004 + 0.008 * min(1.0, d / 0.2)
            w = A * math.sin(a * math.pi * 5 + 0.6) + 0.0025 * math.sin(a * math.pi * 8.5 + z * 12) * min(1.0, d / 0.1)
            yb = -0.020 + 0.006 * math.exp(-d / 0.04) - w        # arka kat: duvarın (y = 0) hep önünde
            return (x, yb - ara if on_kat else yb, z)

        def etek(a, on_kat):
            """Etek ucunun z'si. Orta biraz yukarıda, köşeler aşağıda; en uçta köşe yuvarlatılır."""
            t = abs(a)
            taban = -0.415 if on_kat else -0.536                 # ön kat kısa, arka kat uzun (ikisi de görünsün);
            # arka etek eski modelin boyunu (0,55 m) tutacak kadar aşağıda: pivot boyun ortası olduğu için
            # boy değişirse kanca da yerinden kayardı.
            kose = 0.028 * max(0.0, (t - 0.82) / 0.18) ** 2      # son %18'de etek yukarı kıvrılır: yuvarlak köşe
            return taban + 0.012 * (1 - t * t) + kose

        def havlu_f(u, v):
            """yuzey için f(u, v): v = 0 arka etek, 0,5'e kadar arka kat, 0,5-0,625 kanca üstü kıvrım,
            0,625-1 ön kat ve ön etek. Bölme noktaları 32 satırlık ızgaranın tam çizgilerine düşer
            (16 + 4 + 12 satır); düşmeseydi kıvrımın köşesi bir satırın ortasında kalır, kırık görünürdü."""
            a = u * 2 - 1
            t, zt, ayrik, xt = kolon(a)
            R = ayrik / 2
            zc = zt - R                                          # kıvrımın merkez yüksekliği
            if v <= 0.5:                                         # ARKA KAT: etekten kıvrıma yukarı
                z = etek(a, False) + (zc - etek(a, False)) * (v / 0.5)
                return katman(a, z, False)
            if v >= 0.625:                                       # ÖN KAT: kıvrımdan eteğe aşağı
                z = zc + (etek(a, True) - zc) * ((v - 0.625) / 0.375)
                return katman(a, z, True)
            # KIVRIM: arka kattan ön kata yarım çember (kancanın sapı bu çemberin içinden geçer).
            th = math.pi * (v - 0.5) / 0.125                     # 0 (arka) .. π (ön)
            xb, yb, _ = katman(a, zc, False)                     # kıvrımın arka ucu
            yc = yb - R                                          # çemberin merkezi: iki katın ortası
            return (xb, yc + R * math.cos(th), zc + R * math.sin(th))

        yuzey("Havlu", havlu_f, 30, 32, havlu, KALIN)

        def one_it(a, z, on_kat, pay):
            """Kumaşın (a, z) noktasını yüzeyin DİK doğrultusunda (normal) öne, pay kadar it.
            Yalnızca -y yönünde itmek kıvrımın eğik yamaçlarında yetmiyordu: şerit kumaşa gömülüp
            yandan bakınca kesik kesik görünüyordu. Normal, yüzeyin iki küçük adımdaki değişiminin
            çapraz çarpımıdır (iki teğetin ikisine de dik olan yön)."""
            P = Vector(katman(a, z, on_kat))
            Ta = Vector(katman(a + 0.01, z, on_kat)) - P         # enine teğet
            Tz = Vector(katman(a, z + 0.01, on_kat)) - P         # dikey teğet
            n = Ta.cross(Tz).normalized()
            if n.y > 0:                                          # odaya (-y) bakan yönü seç
                n = -n
            return tuple(P + n * pay)

        # İki soluk şerit: her iki katın eteğine yakın, kumaşın ön yüzünün hemen önünde. Etek eğrisini
        # izledikleri için köşede onlar da yukarı kıvrılır. Arka katınki ön katın altından görünür.
        for on_kat in (True, False):
            for alt, ust in ((0.042, 0.056), (0.066, 0.080)):
                def bant(u, v, on_kat=on_kat, alt=alt, ust=ust):
                    a = min(0.995, max(-0.995, u * 2 - 1))       # en uçta teğet için a ± 0,01 taşmasın
                    z = etek(a, on_kat) + alt + (ust - alt) * v
                    return one_it(a, z, on_kat, KALIN / 2 + 0.0015)   # kumaş yüzeyinin 1,5 mm önünde
                yuzey("Serit", bant, 30, 1, serit_m, 0.0015)
        bitir("Havlu_K7", 0.4, 1.0, kip="duvar")

    elif hangi == "kova":
        plastik = ortak.malzeme("K7_Kova_Plastik", (0.04, 0.16, 0.06), puruz=0.5)
        tel = ortak.malzeme("K7_Kova_Tel", (0.40, 0.40, 0.39), puruz=0.35, metal=0.9)
        tutacak = ortak.malzeme("K7_Kova_Tutacak", (0.02, 0.02, 0.02), puruz=0.5)
        gumus = ortak.malzeme("K7_Hamam_Tasi", (0.62, 0.60, 0.56), puruz=0.3, metal=1.0)
        su = ortak.malzeme("K7_Kova_Su", (0.10, 0.11, 0.09), puruz=0.05)
        torna("Kova", [(0, 0), (0.118, 0), (0.122, 0.01), (0.150, 0.27), (0.158, 0.272), (0.158, 0.285), (0.146, 0.285), (0.116, 0.012), (0, 0.012)], (0, 0, 0), plastik, 32)
        sil("Su", 0.125, 0.004, (0, 0, 0.06), su, 28)                      # dipte bir parmak bulanık su
        for yon in (-1, 1):
            kutu("Kulak", (0.02, 0.025, 0.04), (yon * 0.155, 0, 0.255), plastik, 0.005)
        # Tel sap: iki kulak arasında yay; yana (öne) yatmış.
        e = math.radians(68)
        d = (0.0, -math.sin(e), math.cos(e))
        onceki = None
        for i in range(13):
            a = math.pi * i / 12
            nokta = (0.158 * math.cos(a), 0.158 * math.sin(a) * d[1], 0.26 + 0.158 * math.sin(a) * d[2])
            if onceki:
                cubuk(onceki, nokta, 0.0028, tel, 6)
            onceki = nokta
        orta = (0, 0.158 * d[1], 0.26 + 0.158 * d[2])
        cubuk((-0.04, orta[1], orta[2]), (0.04, orta[1], orta[2]), 0.01, tutacak, 10)
        # Hamam tası: gümüş, kabartmalı kenar, ortada göbek; kovanın yanında yerde.
        hx, hy = 0.27, -0.06
        torna("Tas", [(0, 0), (0.05, 0), (0.085, 0.04), (0.092, 0.05), (0.088, 0.053), (0.080, 0.046), (0.046, 0.007), (0, 0.007)], (hx, hy, 0), gumus, 32)
        kure(0.025, (hx, hy, 0.007), gumus, (1, 1, 0.5), 16)
        simit(0.088, 0.0025, (hx, hy, 0.036), gumus, seg=(32, 4))
        for i in range(16):                                                  # kenar kabartmaları
            a = 2 * math.pi * i / 16
            kure(0.006, (hx + 0.082 * math.cos(a), hy + 0.082 * math.sin(a), 0.038), gumus, (1, 1, 0.6), 8)
        bitir("Kova_K7", 0.3, 1.1)

    elif hangi == "raf":
        boya = ortak.malzeme("K7_Raf_Boya", (0.10, 0.13, 0.10), puruz=0.5, metal=0.6)
        beyaz = ortak.malzeme("K7_CamasirSuyu", (0.72, 0.72, 0.68), puruz=0.4)
        mavi = ortak.malzeme("K7_CamasirSuyu_Kapak", (0.03, 0.08, 0.30), puruz=0.4)
        etiket = ortak.malzeme("K7_CamasirSuyu_Etiket", (0.05, 0.15, 0.40), puruz=0.6)
        karton = ortak.malzeme("K7_Ampul_Kutu", (0.55, 0.40, 0.16), puruz=0.9)
        karton_b = ortak.malzeme("K7_Ampul_Kutu_Bant", (0.35, 0.05, 0.03), puruz=0.8)
        ampul = seffaf(ortak.malzeme("K7_Ampul_Cam", (0.70, 0.70, 0.66), puruz=0.05), 0.6)
        duy = ortak.malzeme("K7_Ampul_Duy", (0.45, 0.40, 0.30), puruz=0.3, metal=0.9)
        bezler = [ortak.malzeme(f"K7_Bez_{i}", c, puruz=0.95) for i, c in enumerate(((0.35, 0.30, 0.22), (0.08, 0.10, 0.20), (0.30, 0.06, 0.05), (0.40, 0.38, 0.33)))]
        gri = ortak.malzeme("K7_Sigorta_Kutu", (0.25, 0.26, 0.25), puruz=0.45, metal=0.7)
        porselen = ortak.malzeme("K7_Sigorta_Porselen", (0.75, 0.73, 0.68), puruz=0.3)
        teneke = ortak.malzeme("K7_Boya_Teneke", (0.40, 0.40, 0.38), puruz=0.35, metal=0.9)
        boya_d = ortak.malzeme("K7_Boya_Damla", (0.60, 0.58, 0.50), puruz=0.5)
        EN, DER, H = 1.00, 0.40, 1.80
        # Köşe dikmeleri: L profil (iki ince levha), delikli görünüm için koyu yuva sırası.
        for sx in (-1, 1):
            for sy in (-1, 1):
                x, y = sx * (EN / 2 - 0.015), sy * (DER / 2 - 0.015)
                kutu("DikmeA", (0.035, 0.003, H), (x, sy * (DER / 2 - 0.0015), H / 2), boya)
                kutu("DikmeB", (0.003, 0.035, H), (sx * (EN / 2 - 0.0015), y, H / 2), boya)
        raflar = (0.08, 0.50, 0.92, 1.34, 1.76)
        for z in raflar:
            kutu("Raf", (EN - 0.006, DER - 0.006, 0.004), (0, 0, z), boya)
            for sy in (-1, 1):                                       # rafların kıvrık kenarları
                kutu("Kenar", (EN - 0.006, 0.003, 0.03), (0, sy * (DER / 2 - 0.004), z - 0.013), boya)
        # 1. raf (en alt): bez yığını.
        z = raflar[0] + 0.002
        for i in range(7):
            b = kutu("Bez", (rnd.uniform(0.18, 0.28), rnd.uniform(0.18, 0.26), rnd.uniform(0.04, 0.07)), (-0.20 + (i % 3) * 0.18 + rnd.uniform(-0.03, 0.03), rnd.uniform(-0.04, 0.04), z + 0.03 + (i // 3) * 0.05), bezler[i % 4], 0.015)
            dondur([b], rnd.uniform(-25, 25), "Z", tuple(b.location))
        kutu("SarkanBez", (0.16, 0.004, 0.075), (-0.30, -DER / 2 - 0.004, 0.045), bezler[1], 0.002)
        # 2. raf: çamaşır suyu şişeleri (biri devrik).
        z = raflar[1] + 0.002
        for i, x in enumerate((0.05, 0.16, 0.27, 0.38)):
            n0 = len(p)
            kutu("Sise", (0.09, 0.065, 0.22), (0, 0, 0.11), beyaz, 0.025)
            kutu("Sap", (0.03, 0.05, 0.07), (0.03, 0, 0.225), beyaz, 0.01)
            sil("Boyun", 0.015, 0.03, (-0.02, 0, 0.235), beyaz, 12)
            sil("Kapak", 0.018, 0.022, (-0.02, 0, 0.26), mavi, 12)
            kutu("Etiket", (0.07, 0.002, 0.09), (0, -0.0335, 0.10), etiket)
            if i == 3:
                dondur(p[n0:], 90, "X")
                tasi(p[n0:], (0, 0.10, 0.0325))
                dondur(p[n0:], 20, "Z")
            tasi(p[n0:], (x - 0.05 if i == 3 else x, -0.03 if i < 3 else -0.08, z))
        for i in range(3):                                           # boş yer: tozlu iki eski şişe izi
            sil("Iz", 0.03, 0.0008, (-0.3 + i * 0.1, 0.02, z + 0.0004), beyaz, 12)
        # 3. raf: ampul kutuları yığını ve bir çıplak ampul.
        z = raflar[2] + 0.002
        for i in range(7):
            sira = [0, 0, 0, 0, 1, 1, 2][i]
            x = -0.40 + [0, 1, 2, 3, 0.5, 1.5, 1][i] * 0.08
            kutu("AmpulKutu", (0.075, 0.075, 0.12), (x, -0.02, z + 0.06 + sira * 0.12), karton, 0.003)
            kutu("Bant", (0.0755, 0.0755, 0.03), (x, -0.02, z + 0.08 + sira * 0.12), karton_b)
        kure(0.03, (-0.02, -0.06, z + 0.03), ampul, (1, 1, 1), 16)
        sil("Duy", 0.013, 0.03, (-0.02, -0.06 + 0.045, z + 0.03), duy, 12)
        dondur([p[-1]], 90, "X", (-0.02, -0.015, z + 0.03))
        kutu("Kutu", (0.28, 0.20, 0.10), (0.30, 0.0, z + 0.05), karton, 0.004)
        # 4. raf: yedek sigorta kutusu (gri, sapı üstte), yanında porselen sigortalar, kablo kangalı.
        z = raflar[3] + 0.002
        kutu("SigortaKutu", (0.22, 0.13, 0.14), (-0.20, 0.0, z + 0.07), gri, 0.006)
        kutu("KapakCizgi", (0.222, 0.132, 0.003), (-0.20, 0.0, z + 0.10), boya)
        cubuk((-0.25, 0, z + 0.14), (-0.25, 0, z + 0.16), 0.004, gri)
        cubuk((-0.15, 0, z + 0.14), (-0.15, 0, z + 0.16), 0.004, gri)
        cubuk((-0.25, 0, z + 0.16), (-0.15, 0, z + 0.16), 0.006, gri)
        kutu("Mandal", (0.03, 0.006, 0.02), (-0.20, -0.068, z + 0.10), boya)
        for i in range(4):
            sil("Sigorta", 0.012, 0.04, (0.0 + i * 0.03, -0.08, z + 0.02), porselen, 12)
            sil("SigortaUc", 0.0125, 0.008, (0.0 + i * 0.03, -0.08, z + 0.044), duy, 12)
        simit(0.08, 0.012, (0.30, 0.02, z + 0.013), bezler[2], seg=(24, 8))
        simit(0.075, 0.012, (0.30, 0.02, z + 0.037), bezler[2], seg=(24, 8))
        # 5. raf (tepe): boya tenekeleri, biri damlamış.
        z = raflar[4] + 0.002
        for i, x in enumerate((-0.30, -0.12, 0.25)):
            sil("Teneke", 0.085, 0.19 - 0.04 * (i == 2), (x, 0.0, z + 0.095 - 0.02 * (i == 2)), teneke, 24)
            sil("TenekeKapak", 0.086, 0.01, (x, 0.0, z + 0.19 - 0.04 * (i == 2)), teneke, 24)
        kutu("Damla", (0.02, 0.004, 0.08), (-0.30, -0.086, z + 0.14), boya_d, 0.003)
        bitir("Raf_K7", 1.8, 3.0)

    elif hangi == "merdiven":
        al_m = ortak.malzeme("K7_Merdiven_Aluminyum", (0.55, 0.56, 0.57), puruz=0.35, metal=0.9)
        siyah = ortak.malzeme("K7_Merdiven_Plastik", (0.02, 0.02, 0.02), puruz=0.6)
        boya = ortak.malzeme("K7_Merdiven_Boya", (0.60, 0.55, 0.45), puruz=0.6)
        # Dik hâlde kurulur (arka yüz y=0), sonra ön alt kenar etrafında duvara yaslanacak kadar döndürülür.
        H = 1.55
        for x in (-0.20, 0.20):
            kutu("OnDikme", (0.05, 0.025, H), (x, -0.1175, H / 2), al_m, 0.003)             # ön ayaklar
            kutu("ArkaDikme", (0.04, 0.02, H - 0.10), (x * 0.95, -0.01, (H - 0.10) / 2 + 0.03), al_m, 0.003)  # katlanmış arka ayaklar
            kutu("Lastik", (0.055, 0.03, 0.03), (x, -0.1175, 0.015), siyah, 0.005)
            kutu("Makas", (0.012, 0.09, 0.012), (x * 0.9, -0.06, 0.62), al_m)                # katlanmış açılma makası
        for i, z in enumerate((0.30, 0.56, 0.82, 1.08)):
            kutu("Basamak", (0.36, 0.085, 0.025), (0, -0.0625, z), al_m, 0.003)
            for k in range(3):
                kutu("Ol", (0.35, 0.004, 0.003), (0, -0.09 + k * 0.025, z + 0.0135), al_m)
        kutu("Tepe", (0.46, 0.14, 0.06), (0, -0.065, H + 0.02), siyah, 0.01)
        kutu("Leke", (0.08, 0.002, 0.05), (0.1, -0.131, 0.9), boya)                           # boya lekesi
        dondur(p, -9.3, "X", (0, -0.13, 0))
        bitir("Merdiven_K7", 1.6, 2.4, kip="dip")

    elif hangi == "yatak":
        demir = ortak.malzeme("K7_Yatak_Demir", (0.03, 0.045, 0.035), puruz=0.5, metal=0.4)
        pas = ortak.malzeme("K7_Yatak_Pas", (0.18, 0.07, 0.03), puruz=0.9)
        pirinc = ortak.malzeme("K7_Yatak_Top", (0.45, 0.33, 0.12), puruz=0.35, metal=0.9)
        yay = ortak.malzeme("K7_Yatak_Yay", (0.15, 0.15, 0.14), puruz=0.5, metal=0.8)
        silte = ortak.malzeme("K7_Yatak_Silte", (0.50, 0.46, 0.36), puruz=0.9)
        cizgi = ortak.malzeme("K7_Yatak_Silte_Cizgi", (0.20, 0.22, 0.30), puruz=0.9)
        batt = ortak.malzeme("K7_Yatak_Battaniye", (0.20, 0.21, 0.15), puruz=0.95)   # #110: haki-gri asker battaniyesi (koyu kahve kapıdan okunmuyordu)
        batt_s = ortak.malzeme("K7_Yatak_Battaniye_Serit", (0.30, 0.22, 0.10), puruz=0.95)
        yastik = ortak.malzeme("K7_Yatak_Yastik", (0.55, 0.53, 0.47), puruz=0.9)
        EN, BOY = 0.95, 1.95
        xs, yb, ya = EN / 2 - 0.02, BOY / 2 - 0.02, -BOY / 2 + 0.02
        # Başlık (+y) ve ayak ucu: dört dikme boru, yatay borular, arada dik parmaklıklar, tepelerde pirinç top.
        for x in (-xs, xs):
            cubuk((x, yb, 0), (x, yb, 0.80), 0.016, demir, 12)
            cubuk((x, ya, 0), (x, ya, 0.55), 0.016, demir, 12)
            kure(0.022, (x, yb, 0.81), pirinc)
            kure(0.022, (x, ya, 0.56), pirinc)
            kutu("Kasa", (0.03, BOY - 0.04, 0.04), (x, 0, 0.30), demir, 0.003)
        for (y, z1) in ((yb, 0.76), (ya, 0.51)):
            cubuk((-xs, y, z1), (xs, y, z1), 0.012, demir, 10)
            cubuk((-xs, y, 0.40), (xs, y, 0.40), 0.012, demir, 10)
            for i in range(1, 8):
                x = -xs + i * (2 * xs) / 8
                cubuk((x, y, 0.40), (x, y, z1), 0.007, demir, 8)
        kutu("Pas", (0.02, 0.002, 0.06), (xs - 0.01, yb - 0.017, 0.62), pas)
        # Yay tabanı ve ince çizgili şilte.
        kutu("YayTabani", (EN - 0.08, BOY - 0.08, 0.01), (0, 0, 0.31), yay)
        for i in range(9):
            cubuk((-EN / 2 + 0.05, -BOY / 2 + 0.1 + i * 0.22, 0.30), (EN / 2 - 0.05, -BOY / 2 + 0.2 + i * 0.22, 0.30), 0.003, yay, 5)
        kutu("Silte", (EN - 0.08, BOY - 0.08, 0.12), (0, 0, 0.375), silte, 0.03)
        for k in range(7):
            kutu("Cizgi", (0.012, BOY - 0.10, 0.1205), (-0.36 + k * 0.12, 0, 0.375), cizgi)
        # Yastık: başta, ortası çökük (burada uyunmuş).
        kutu("Yastik", (0.58, 0.34, 0.10), (-0.02, 0.72, 0.485), yastik, 0.04)
        kutu("Cukur", (0.30, 0.2, 0.02), (0.0, 0.70, 0.535), yastik, 0.008)
        # Dağınık battaniye: yatağın ayak tarafına itilmiş, kabarık tümsekler, bir kenarı yere sarkıyor.
        for i in range(6):
            y = -0.75 + i * 0.22
            b = kutu("Battaniye", (0.86 - 0.1 * (i % 2), 0.28, 0.05 + rnd.uniform(0, 0.05)), (rnd.uniform(-0.04, 0.04), y, 0.46 + rnd.uniform(0, 0.02)), batt, 0.02)
            dondur([b], rnd.uniform(-12, 12), "Z", (0, y, 0.46))
        cubuk((-0.40, 0.30, 0.49), (0.38, 0.40, 0.50), 0.05, batt, 12)                    # dürülmüş üst kenar
        f = kutu("Sarkan", (0.02, 1.00, 0.34), (EN / 2 - 0.02, -0.30, 0.30), batt, 0.008)
        dondur([f], -10, "Y", (EN / 2 - 0.02, -0.30, 0.46))
        kutu("SarkanSerit", (0.024, 1.00, 0.025), (EN / 2 - 0.02, -0.30, 0.18), batt_s)
        dondur([p[-1]], -10, "Y", (EN / 2 - 0.02, -0.30, 0.46))
        bitir("Tek_Kisilik_Yatak_K7", 0.9, 2.8)

    elif hangi == "komodin":
        boya = ortak.malzeme("K7_Komodin_Boya", (0.55, 0.50, 0.40), puruz=0.5)
        dokuk = ortak.malzeme("K7_Komodin_Dokuk", (0.18, 0.12, 0.06), puruz=0.7)
        kulp = ortak.malzeme("K7_Komodin_Kulp", (0.04, 0.03, 0.02), puruz=0.4)
        kitap = ortak.malzeme("K7_Komodin_Kitap", (0.20, 0.08, 0.05), puruz=0.8)
        kirmizi = ortak.malzeme("K7_Saat_Kirmizi", (0.33, 0.04, 0.03), puruz=0.3, metal=0.4)
        krom = ortak.malzeme("K7_Saat_Krom", (0.55, 0.55, 0.53), puruz=0.25, metal=1.0)
        kadran = ortak.malzeme("K7_Saat_Kadran", (0.70, 0.66, 0.55), puruz=0.4)
        siyah = ortak.malzeme("K7_Saat_Akrep", (0.02, 0.02, 0.02), puruz=0.5)
        paket = ortak.malzeme("K7_Sigara_Paketi", (0.60, 0.58, 0.52), puruz=0.6)
        paket_k = ortak.malzeme("K7_Sigara_Paketi_Serit", (0.35, 0.05, 0.04), puruz=0.6)
        EN, DER, H = 0.40, 0.35, 0.50
        for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
            for y in (-DER / 2 + 0.02, DER / 2 - 0.02):
                kutu("Ayak", (0.03, 0.03, 0.06), (x, y, 0.03), boya, 0.004)
        kutu("Govde", (EN - 0.01, DER - 0.01, H - 0.08), (0, 0.0, 0.06 + (H - 0.08) / 2), boya)
        kutu("Tabla", (EN, DER, 0.02), (0, 0, H - 0.01), boya, 0.005)
        kutu("CekmeceOn", (EN - 0.05, 0.01, 0.11), (0, -DER / 2, H - 0.09), boya, 0.004)   # açılmayan çekmece (süs)
        kutu("Kulp", (0.06, 0.012, 0.012), (0, -DER / 2 - 0.009, H - 0.09), kulp, 0.004)
        n = kutu("Nis", (EN - 0.05, DER - 0.04, 0.19), (0, -0.02, 0.19), boya)                  # alt açık bölme
        p.remove(n)                                                                     # kesici parça listeye girmesin
        govde = [o for o in p if o.name.startswith("Govde")][0]
        ortak.oy(govde, n)
        kutu("Kitap", (0.16, 0.22, 0.03), (-0.05, 0.0, 0.1125), kitap, 0.003)
        kutu("Kitap", (0.15, 0.21, 0.025), (-0.045, 0.01, 0.14), dokuk, 0.003)
        kutu("Dokuk", (0.05, 0.001, 0.02), (0.14, -DER / 2 - 0.0005 + 0.005, 0.25), dokuk)    # boya dökülmesi
        # Kurmalı çalar saat: kırmızı gövde, krom çanlar ve çekiç, iki ayak.
        cx, cy, cz = 0.07, 0.0, H
        dik_disk(0.05, 0.035, (cx, cy, cz + 0.065), kirmizi, 28)
        dik_disk(0.043, 0.004, (cx, cy - 0.0185, cz + 0.065), kadran, 28)
        simit(0.045, 0.003, (cx, cy - 0.019, cz + 0.065), krom, rot=(math.pi / 2, 0, 0), seg=(28, 6))
        kutu("Akrep", (0.003, 0.002, 0.025), (cx, cy - 0.021, cz + 0.075), siyah)
        k = kutu("Yelkovan", (0.002, 0.002, 0.035), (cx + 0.012, cy - 0.022, cz + 0.07), siyah)
        dondur([k], -60, "Y", (cx, cy - 0.022, cz + 0.065))
        for yon in (-1, 1):
            kure(0.022, (cx + yon * 0.032, cy + 0.004, cz + 0.12), krom, (1, 1, 0.8), 14)
            cubuk((cx + yon * 0.028, cy, cz + 0.02), (cx + yon * 0.038, cy, cz + 0.002), 0.004, krom)
        cubuk((cx, cy + 0.004, cz + 0.11), (cx, cy + 0.004, cz + 0.13), 0.003, krom)
        cubuk((cx - 0.015, cy + 0.004, cz + 0.13), (cx + 0.015, cy + 0.004, cz + 0.13), 0.003, krom)
        cubuk((cx - 0.012, cy + 0.02, cz + 0.07), (cx - 0.012, cy + 0.03, cz + 0.07), 0.008, krom)   # kurma kolu
        # Sigara paketi, yanında kibrit kutusu.
        s0 = len(p)
        kutu("Paket", (0.055, 0.022, 0.085), (-0.10, -0.04, H + 0.011), paket, 0.002)
        kutu("PaketSerit", (0.0555, 0.0225, 0.02), (-0.10, -0.04, H + 0.02), paket_k)
        dondur(p[s0:], 90, "X", (-0.10, -0.04, H + 0.011))
        tasi(p[s0:], (0, 0.03, 0.0))
        dondur(p[s0:], 25, "Z", (-0.10, -0.01, H))
        kutu("Kibrit", (0.05, 0.035, 0.015), (-0.05, 0.08, H + 0.0075), dokuk, 0.002)
        bitir("Komodin_K7", 0.6, 1.2)

    elif hangi == "koliler":
        karton = ortak.malzeme("K7_Koli_Karton", (0.33, 0.21, 0.10), puruz=0.9)
        karton2 = ortak.malzeme("K7_Koli_Karton2", (0.28, 0.18, 0.085), puruz=0.9)
        bant = ortak.malzeme("K7_Koli_Bant", (0.45, 0.33, 0.16), puruz=0.3)
        kalem = ortak.malzeme("K7_Koli_Kalem", (0.015, 0.015, 0.02), puruz=0.8)

        def koli(en, der, boy, konum, aci, mat):
            """Bantlanmış koli: üstte kapak ek yeri boyunca bant, önde ve arkada aşağı iner; önde kalem yazısı."""
            n0 = len(p)
            kutu("Koli", (en, der, boy), (0, 0, boy / 2), mat, 0.006)
            kutu("EkYeri", (en * 0.98, 0.003, 0.002), (0, 0, boy + 0.0005), kalem)
            kutu("Bant", (0.05, der + 0.004, 0.002), (0, 0, boy + 0.001), bant)
            for sy in (-1, 1):
                kutu("Bant", (0.05, 0.002, 0.12), (0, sy * (der / 2 + 0.001), boy - 0.06), bant)
            # Kalemle yazı izlenimi: iki satır kısa, eğri çizgiler (harf harf değil, el yazısı hissi).
            for s_ in range(2):
                x = -en * 0.32
                while x < en * 0.25:
                    w = rnd.uniform(0.015, 0.045)
                    c = kutu("Yazi", (w, 0.002, 0.006), (x + w / 2, -der / 2 - 0.0012, boy * 0.55 - s_ * 0.05 + rnd.uniform(-0.006, 0.006)), kalem)
                    dondur([c], rnd.uniform(-25, 25), "Y", tuple(c.location))
                    x += w + rnd.uniform(0.004, 0.02)
            kutu("Cizik", (0.10, 0.002, 0.004), (en * 0.2, -der / 2 - 0.0012, boy * 0.30), kalem)
            dondur(p[n0:], aci)
            tasi(p[n0:], konum)

        koli(0.60, 0.40, 0.40, (0, 0, 0), 0, karton)
        koli(0.50, 0.36, 0.34, (0.02, 0.0, 0.40), 6, karton2)
        koli(0.36, 0.30, 0.24, (-0.03, 0.01, 0.74), -9, karton)
        koli(0.45, 0.35, 0.30, (0.56, 0.03, 0), 4, karton2)
        bitir("Koliler_K7", 1.0, 2.0)

    elif hangi == "sandik":
        ceviz = ortak.malzeme("K7_Sandik_Ceviz", (0.035, 0.018, 0.008), puruz=0.5)
        ic = ortak.malzeme("K7_Sandik_Ic", (0.09, 0.05, 0.022), puruz=0.8)
        demir = ortak.malzeme("K7_Sandik_Demir", (0.03, 0.03, 0.03), puruz=0.45, metal=0.8)
        pas = ortak.malzeme("K7_Sandik_Pas", (0.16, 0.06, 0.025), puruz=0.9)
        L = ac.levha
        EN, DER, HG = 0.80, 0.45, 0.46              # HG: gövdenin üst kenarı; kapak 0.46-0.50
        t = 0.02
        x0, x1, y0, y1 = -EN / 2, EN / 2, -DER / 2, DER / 2
        for x in (-0.30, 0.30):
            kutu("Kizak", (0.06, DER, 0.03), (x, 0, 0.015), demir, 0.004)
        p.extend([L("Taban", x0, x1, y0, y1, 0.03, 0.05, ic),
                  L("On", x0, x1, y0, y0 + t, 0.03, HG, ceviz), L("Arka", x0, x1, y1 - t, y1, 0.03, HG, ceviz),
                  L("Yan", x0, x0 + t, y0 + t, y1 - t, 0.03, HG, ceviz), L("Yan", x1 - t, x1, y0 + t, y1 - t, 0.03, HG, ceviz)])
        # Demir şeritler: önde ve arkada iki dikey, altta ve üstte çevre bandı, köşelerde köşebent; perçinler.
        for x in (-0.25, 0.25):
            for sy in (-1, 1):
                kutu("Serit", (0.04, 0.005, HG - 0.04), (x, sy * (DER / 2 + 0.0025), 0.03 + (HG - 0.04) / 2 + 0.005), demir)
                for z in (0.10, 0.25, 0.40):
                    kure(0.005, (x, sy * (DER / 2 + 0.005), z), demir, (1, 0.6, 1), 8)
        for z in (0.07, HG - 0.02):
            for sy in (-1, 1):
                kutu("Bant", (EN + 0.01, 0.005, 0.03), (0, sy * (DER / 2 + 0.0025), z), demir)
            for sx in (-1, 1):
                kutu("Bant", (0.005, DER + 0.01, 0.03), (sx * (EN / 2 + 0.0025), 0, z), demir)
        for sx in (-1, 1):
            for sy in (-1, 1):
                kutu("Kose", (0.05, 0.05, 0.06), (sx * (EN / 2 - 0.02), sy * (DER / 2 - 0.02), 0.06), demir)
        kutu("Pas", (0.03, 0.002, 0.04), (-0.25, -DER / 2 - 0.006, 0.12), pas)
        # Yan kulplar: kulak plakası ve U biçimli sap.
        for sx in (-1, 1):
            xk = sx * (EN / 2 + 0.006)
            kutu("KulpPlaka", (0.004, 0.10, 0.05), (xk, 0, 0.33), demir)
            cubuk((xk + sx * 0.002, -0.04, 0.32), (xk + sx * 0.03, -0.04, 0.30), 0.005, demir)
            cubuk((xk + sx * 0.03, -0.04, 0.30), (xk + sx * 0.03, 0.04, 0.30), 0.005, demir)
            cubuk((xk + sx * 0.03, 0.04, 0.30), (xk + sx * 0.002, 0.04, 0.32), 0.005, demir)
        # Asma kilit halkası: önde, kapağın köprüsü bunun üstüne iner.
        kutu("HalkaPlaka", (0.05, 0.005, 0.04), (0, y0 - 0.0025, 0.405), demir)
        simit(0.012, 0.003, (0, y0 - 0.014, 0.405), demir, rot=(0, math.pi / 2, 0), seg=(16, 6))
        # Arka menteşe yarıları (gövdede).
        for x in (-0.25, 0.25):
            kutu("Mentese", (0.06, 0.004, 0.04), (x, y1 + 0.006, HG - 0.02), demir)
        # KAPAK: ayrı nesne, arka üst kenarından menteşeli. Şeritleri ve köprüsü kapakla birlikte açılır.
        n0 = len(p)
        kutu("KapakTabla", (EN + 0.01, DER + 0.01, 0.036), (0, 0, HG + 0.018), ceviz, 0.004)
        for x in (-0.25, 0.25):
            kutu("KSerit", (0.04, DER + 0.02, 0.004), (x, 0, HG + 0.038), demir)
            kutu("KSeritOn", (0.04, 0.004, 0.04), (x, y0 - 0.007, HG + 0.02), demir)
            kutu("KMentese", (0.06, 0.004, 0.03), (x, y1 + 0.007, HG + 0.02), demir)
            for y in (-0.15, 0.0, 0.15):
                kure(0.005, (x, y, HG + 0.04), demir, (1, 1, 0.6), 8)
        kutu("KBant", (EN + 0.02, 0.005, 0.03), (0, y0 - 0.0075, HG + 0.02), demir)
        kutu("Kopru", (0.036, 0.005, 0.10), (0, y0 - 0.0125, HG - 0.02), demir, 0.002)   # halkanın üstüne inen köprü
        kutu("KopruYuva", (0.012, 0.006, 0.03), (0, y0 - 0.014, 0.405), pas)
        kapak_parca = p[n0:]
        del p[n0:]
        kapak = ortak.birlestir(kapak_parca, "Kapak")
        ortak.yumusat(kapak, 35)
        ortak.pivotu_tasi(kapak, (0, y1 + 0.005, HG + 0.036))
        bitir("Sandik_Cemal_K7", 0.6, 1.4, parcalar=[kapak])

    elif hangi == "bavullar":
        vinil = ortak.malzeme("K7_Bavul_Vinil", (0.09, 0.05, 0.025), puruz=0.5)
        kanvas = ortak.malzeme("K7_Bavul_Kanvas", (0.05, 0.06, 0.045), puruz=0.9)
        kenar = ortak.malzeme("K7_Bavul_Kenar", (0.03, 0.018, 0.01), puruz=0.5)
        metal = ortak.malzeme("K7_Bavul_Metal", (0.45, 0.42, 0.35), puruz=0.35, metal=0.9)
        kayis = ortak.malzeme("K7_Bavul_Kayis", (0.05, 0.03, 0.015), puruz=0.6)
        kayis2 = ortak.malzeme("K7_Bavul_Kayis2", (0.18, 0.13, 0.06), puruz=0.8)
        # A: dik duran kahverengi vinil bavul (arkada, duvara yakın). Şişkin; sapı üstte; iki kayışla sarılı.
        ax, ay = -0.05, 0.12
        kutu("BavulA", (0.62, 0.21, 0.44), (ax, ay, 0.22), vinil, 0.035)
        kutu("EkYeri", (0.625, 0.012, 0.445), (ax, ay, 0.2225), kenar, 0.01)
        for sx in (-1, 1):
            for sz in (0, 1):
                kutu("Kose", (0.05, 0.215, 0.05), (ax + sx * 0.29, ay, 0.025 + sz * 0.39), metal, 0.01)
        for x in (-0.12, 0.12):
            kutu("Kilit", (0.035, 0.012, 0.025), (ax + x, ay - 0.105, 0.43), metal, 0.003)
        cubuk((ax - 0.07, ay, 0.44), (ax - 0.06, ay, 0.47), 0.008, kenar)
        cubuk((ax + 0.07, ay, 0.44), (ax + 0.06, ay, 0.47), 0.008, kenar)
        cubuk((ax - 0.06, ay, 0.47), (ax + 0.06, ay, 0.47), 0.012, kenar, 10)
        for x in (-0.18, 0.18):                     # kayış: önden, üstten ve arkadan geçen tek bant
            kutu("Kayis", (0.035, 0.004, 0.45), (ax + x, ay - 0.107, 0.225), kayis)
            kutu("Kayis", (0.035, 0.004, 0.45), (ax + x, ay + 0.107, 0.225), kayis)
            kutu("Kayis", (0.035, 0.218, 0.004), (ax + x, ay, 0.442), kayis)
            kutu("Toka", (0.045, 0.008, 0.04), (ax + x, ay - 0.11, 0.30), metal, 0.002)
        # B: önde yatık, yeşil-gri kanvas bavul, deri kenarlı; ip gibi sarılmış açık renk kayış; biraz dönük.
        n0 = len(p)
        kutu("BavulB", (0.70, 0.46, 0.20), (0, 0, 0.10), kanvas, 0.03)
        kutu("EkYeri", (0.705, 0.465, 0.012), (0, 0, 0.10), kenar, 0.005)
        for sx in (-1, 1):
            for sy in (-1, 1):
                kutu("Kose", (0.06, 0.06, 0.205), (sx * 0.325, sy * 0.205, 0.1025), kenar, 0.01)
        cubuk((-0.07, -0.23, 0.10), (-0.06, -0.26, 0.10), 0.008, kenar)
        cubuk((0.07, -0.23, 0.10), (0.06, -0.26, 0.10), 0.008, kenar)
        cubuk((-0.06, -0.26, 0.10), (0.06, -0.26, 0.10), 0.012, kenar, 10)
        for x in (-0.14, 0.14):
            kutu("Kilit", (0.035, 0.012, 0.025), (x, -0.232, 0.14), metal, 0.003)
        kutu("Kayis", (0.72, 0.03, 0.004), (0, 0.02, 0.202), kayis2)
        for sx in (-1, 1):
            kutu("Kayis", (0.004, 0.03, 0.205), (sx * 0.352, 0.02, 0.10), kayis2)
        kutu("Dugum", (0.05, 0.05, 0.02), (0.12, 0.02, 0.21), kayis2, 0.008)
        dondur(p[n0:], 8)
        tasi(p[n0:], (0.03, -0.26, 0))
        bitir("Bavullar_K7", 0.5, 1.8)

    elif hangi == "duvar":
        kagit = ortak.malzeme("K7_Arastirma_Kagit", (0.62, 0.60, 0.52), puruz=0.9)
        gazete = ortak.malzeme("K7_Arastirma_Gazete", (0.50, 0.46, 0.36), puruz=0.9)
        gazete2 = ortak.malzeme("K7_Arastirma_Gazete2", (0.42, 0.36, 0.24), puruz=0.9)    # daha sararmış
        satir = ortak.malzeme("K7_Arastirma_Satir", (0.12, 0.12, 0.11), puruz=0.9)
        baslik = ortak.malzeme("K7_Arastirma_Baslik", (0.03, 0.03, 0.03), puruz=0.9)
        resim = ortak.malzeme("K7_Arastirma_Resim", (0.06, 0.06, 0.055), puruz=0.8)
        kursun = ortak.malzeme("K7_Arastirma_Kursun", (0.05, 0.05, 0.06), puruz=0.8)
        tukenmez = ortak.malzeme("K7_Arastirma_Tukenmez", (0.03, 0.05, 0.25), puruz=0.8)
        not_k = ortak.malzeme("K7_Arastirma_Not", (0.60, 0.52, 0.22), puruz=0.9)
        kirmizi = ortak.malzeme("K7_Arastirma_Ip", (0.40, 0.02, 0.02), puruz=0.8)
        igne = [ortak.malzeme(f"K7_Igne_{i}", c, puruz=0.3) for i, c in enumerate(((0.45, 0.03, 0.02), (0.55, 0.45, 0.05), (0.04, 0.10, 0.40), (0.05, 0.25, 0.06)))]
        bant = seffaf(ortak.malzeme("K7_Arastirma_Bant", (0.60, 0.58, 0.45), puruz=0.3), 0.5)
        igneler = []                                  # iplerin bağlanacağı iğne noktaları

        def kagit_parca(cx, cz, en, boy, aci, mat, y=-0.001):
            """Duvara yapışık kâğıt (y: kâğıdın ön yüzü). Parçaları döndürmek için listeyi geri verir."""
            return kutu("Kagit", (en, 0.0008, boy), (cx, y + 0.0004, cz), mat)

        def gazete_kupuru(cx, cz, en, boy, aci, mat, resimli):
            n0 = len(p)
            y = -0.001 - 0.0006 * (len(igneler) % 3)         # kâğıtlar üst üste binerse titreşmesin
            kagit_parca(cx, cz, en, boy, aci, mat, y)
            yy = y - 0.0009
            kutu("Baslik", (en * 0.85, 0.0006, boy * 0.09), (cx, yy, cz + boy / 2 - boy * 0.10), baslik)
            ust = cz + boy / 2 - boy * 0.2
            sutun = 2 if en > 0.14 else 1
            sg = (en * 0.86) / sutun
            for s_ in range(sutun):
                sx = cx - en * 0.43 + sg * (s_ + 0.5)
                z = ust
                alt = cz - boy / 2 + 0.012
                if resimli and s_ == 0:
                    kutu("Resim", (sg * 0.9, 0.0006, boy * 0.30), (sx, yy, ust - boy * 0.15), resim)
                    z = ust - boy * 0.32
                while z > alt:
                    kutu("Satir", (sg * rnd.uniform(0.7, 0.92), 0.0006, 0.0025), (sx, yy, z), satir)
                    z -= 0.009
            dondur(p[n0:], aci, "Y", (cx, 0, cz))
            igneler.append((cx + rnd.uniform(-0.02, 0.02), cz + boy / 2 - 0.012))

        # Bina kesiti: ortada büyük kâğıt; dokuz kat, merdiven zikzakı, pencereler; 9. kat kırmızı daire içinde.
        bx, bz, bw, bh = 0.0, 0.0, 0.32, 0.44
        n0 = len(p)
        kagit_parca(bx, bz, bw, bh, 0, kagit)
        y = -0.0019
        gx0, gx1, gz0, gz1 = bx - 0.09, bx + 0.09, bz - 0.19, bz + 0.17
        kutu("Zemin", (0.26, 0.0006, 0.003), (bx, y, gz0), kursun)
        for x in (gx0, gx1):
            kutu("Duvar", (0.0028, 0.0006, gz1 - gz0), (x, y, (gz0 + gz1) / 2), kursun)
        kat = (gz1 - gz0) / 9.6                                   # zemin kat biraz alçak, dokuz kat çizgisi
        for i in range(10):
            z = gz0 + 0.6 * kat + i * kat if i > 0 else gz0 + 0.6 * kat
            kutu("Kat", (gx1 - gx0, 0.0006, 0.0022), (bx, y, z if i < 9 else gz1), kursun)
        for i in range(9):                                        # her kat: merdiven zikzakı ve iki pencere
            z = gz0 + 0.6 * kat + i * kat
            m = kutu("Merdiven", (0.05, 0.0006, 0.0018), (bx, y, z + kat / 2), kursun)
            dondur([m], -30 if i % 2 == 0 else 30, "Y", (bx, y, z + kat / 2))
            for x in (gx0 + 0.025, gx1 - 0.025):
                kutu("Pencere", (0.018, 0.0006, kat * 0.45), (x, y, z + kat / 2), kursun)
            for k in range(i + 1 if i < 8 else 0):                # sağda kat sayma çentikleri: 1..8
                kutu("Centik", (0.0018, 0.0006, 0.010), (gx1 + 0.012 + k * 0.004, y, z + kat / 2), kursun)
        z9 = gz0 + 0.6 * kat + 8 * kat + kat / 2
        simit(0.115, 0.0022, (bx, y - 0.0003, z9), kirmizi, rot=(math.pi / 2, 0, 0), seg=(36, 4))
        bpy.context.active_object.scale = (1.0, 1.0, 0.32)
        bpy.ops.object.transform_apply(scale=True)
        for s_ in (-1, 1):                                        # 9. katın yanında iki kırmızı ünlem çizgisi
            kutu("Unlem", (0.003, 0.0006, 0.022), (gx1 + 0.045 + s_ * 0.006, y - 0.0003, z9 + 0.006), kirmizi)
        for k in range(4):                                        # altta tükenmezle notlar
            kutu("Not", (rnd.uniform(0.08, 0.2), 0.0006, 0.0028), (bx - 0.03 + rnd.uniform(-0.02, 0.02), y, gz0 - 0.013 - k * 0.009), tukenmez)
        dondur(p[n0:], 1.5, "Y", (bx, 0, bz))
        igneler.append((bx - 0.13, bz + bh / 2 - 0.012))
        igneler.append((bx + 0.13, bz + bh / 2 - 0.012))
        dokuz = (gx1 + 0.02, z9)                                  # iplerin toplandığı nokta: 9. kat
        igneler.append(dokuz)
        # Gazete kupürleri çevrede.
        for (cx, cz, en, boy, aci, mat, res) in ((-0.42, 0.28, 0.20, 0.24, -4, gazete, True), (-0.47, -0.05, 0.16, 0.20, 3, gazete2, False),
                                                  (-0.36, -0.33, 0.22, 0.15, -2, gazete, True), (0.40, 0.30, 0.18, 0.22, 5, gazete2, True),
                                                  (0.46, 0.01, 0.14, 0.18, -3, gazete, False), (0.36, -0.30, 0.22, 0.18, 2, gazete2, True),
                                                  (-0.20, 0.36, 0.12, 0.10, 6, gazete, False)):
            gazete_kupuru(cx, cz, en, boy, aci, mat, res)
        # El notları (sarı kâğıt) ve bir fotoğraf (beyaz kenarlı, karanlık bina).
        for (cx, cz, aci) in ((0.21, -0.12, -7), (-0.22, -0.18, 5), (0.23, 0.35, 3)):
            n0 = len(p)
            kutu("Not", (0.08, 0.0008, 0.08), (cx, -0.0022, cz), not_k)
            for k in range(5):
                kutu("Yazi", (rnd.uniform(0.03, 0.06), 0.0006, 0.002), (cx - 0.005, -0.0030, cz + 0.025 - k * 0.012), tukenmez)
            dondur(p[n0:], aci, "Y", (cx, 0, cz))
            igneler.append((cx, cz + 0.03))
        n0 = len(p)
        kutu("Foto", (0.09, 0.0008, 0.11), (0.22, -0.0024, 0.12), kagit)
        kutu("FotoResim", (0.078, 0.0006, 0.08), (0.22, -0.0032, 0.127), resim)
        kutu("FotoBina", (0.03, 0.0006, 0.06), (0.22, -0.0036, 0.117), baslik)
        dondur(p[n0:], -4, "Y", (0.22, 0, 0.12))
        igneler.append((0.22, 0.17))
        kutu("Bant", (0.05, 0.0005, 0.018), (-0.20, -0.0035, 0.40), bant)
        # İğneler ve kırmızı ipler: kupürlerden 9. kata, bir kısmı birbirine.
        for i, (x, z) in enumerate(igneler):
            kure(0.0065, (x, -0.007, z), igne[i % 4], (1, 0.8, 1), 10)
            cubuk((x, -0.001, z), (x, -0.006, z), 0.0009, igne[0], 4)
        yi = -0.0075
        # Sıra: 0-1 çizimin iğneleri, 2 dokuzuncu kat, 3-9 kupürler, 10-12 notlar, 13 fotoğraf.
        baglar = [(3, 2), (4, 2), (5, 2), (6, 2), (8, 2), (13, 2), (10, 2), (7, 13), (9, 3), (11, 5), (12, 6)]
        for a, b in baglar:
            if a < len(igneler) and b < len(igneler):
                (xa, za), (xb, zb) = igneler[a], igneler[b]
                cubuk((xa, yi, za), (xb, yi, zb), 0.0012, kirmizi, 4)
        bitir("Arastirma_Duvari_K7", 1.0, 1.3, kip="duvar")

    else:
        raise SystemExit("bilinmeyen eşya: " + hangi)


if __name__ == "__main__":
    hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "tiras"
    ortak.sahneyi_temizle()
    uret(hangi)
