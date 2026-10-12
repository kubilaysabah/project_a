# cin_sesleri_uret.py
# Üç harflinin GÖRÜNÜŞ sesleri (#143, madde 10; Şahan: "üç harfliler çıkınca görsel olarak korkuyorum ama
# arkadan korkutucu sesler çıkmıyor"). Görünür her belirişte (belirme, karartma, geçiş, kaçış, gezen gölge,
# av, kovalamaca, ayna, duvar gölgesi) CinSesi.Belirdi iki şey çalar:
#   1. STINGER (figürün yerinden, 3B): aniden yükselip tırmanan, uyumsuz bir "cızırtı kümesi" ve altında
#      bir alt-bas darbe. Korku filmlerinin "belirme vuruşu"; canlı ses değil, sentezlenir.
#   2. UĞULTU (oyuncunun ARKASINDAN, 3B, 2-4 sn): alçak, titreyen bir gürültü; kulağa "arkamda bir şey
#      var" dedirten şey. Bu da sentezdir.
#   Nefes ve fısıltı (canlı ses) buradan ÇIKMAZ: onları Şahan kaydeder (Resources/Sesler/Nefes_Cin_N,
#   Fisilti_N; kayıt yoksa sessiz), CinSesi kayıt varsa oyuncunun arkasından çalar.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/cin_sesleri_uret.py [stinger ugultu]
#
# ÜRETİLEN DOSYALAR (Assets/Resources/Sesler/)
#   Cin_Stinger_1, Cin_Stinger_2   1,6-2,0 sn; iki farklı tohum, kod rastgele seçer
#   Cin_Ugultu                     3,5 sn; başta yükselir, sonda söner (tek seferlik, döngü değil)
#
# KURAL (ses_ortak.py): saf sinüs yok. Stinger'ın "tonları" dar bantlı gürültüdür: beyaz gürültü, q'su
# yüksek bant geçiren süzgeçlerden (modlar) geçince yalnız o perdelerde çınlar; süzgecin frekansı zamanla
# yukarı kaydırılınca kulağa tırmanan, hafif detone bir keman kümesi gibi gelir (gerçek yaylıların
# "kaydırmalı" korku efekti). Alt-bas darbe: 45-60 Hz'e süzülmüş kısa gürültü patlaması, hızla sönen.
import math, os, random, struct, sys, wave
from ses_ortak import alcak, yuksek, bant, modlar, darbe, gurultu, ekle, yanki, yumusak_sinirla, tepeyi_esitle, kaydet

HIZ = 44100
KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")
URETILENLER = []

# Seviye: esya_sesleri_uret.py'nin ölçüsüyle (en gür 100 ms'nin RMS'i). Stinger ayak sesinden gür ama
# kalp atışını bastırmasın; uğultu kısık (arka plan).
STINGER_DB = -14.0
UGULTU_DB = -24.0


def en_gur_rms(ornekler, pencere=0.1):
    n = int(pencere * HIZ)
    if len(ornekler) <= n:
        return math.sqrt(sum(x * x for x in ornekler) / max(1, len(ornekler)))
    kare = [x * x for x in ornekler]
    toplam = sum(kare[:n]); en = toplam
    for i in range(n, len(kare)):
        toplam += kare[i] - kare[i - n]
        if toplam > en: en = toplam
    return math.sqrt(en / n)


def seviye_tepe(ornekler, hedef_db, en_fazla=0.8):
    rms = en_gur_rms(ornekler)
    tepe = max(abs(x) for x in ornekler) or 1.0
    istenen = 10 ** (hedef_db / 20)
    return min(en_fazla, tepe * istenen / rms) if rms > 0 else en_fazla


def zarf(ornekler, giris, cikis):
    n = len(ornekler); g = int(giris * HIZ); c = int(cikis * HIZ)
    s = list(ornekler)
    for i in range(min(g, n)): s[i] *= i / g
    for i in range(min(c, n)): s[n - 1 - i] *= i / c
    return s


def kayan_kume(n, perdeler, bas_carpan, bit_carpan, q):
    """
    Bir gürültüyü, frekansı zamanla bas_carpan'dan bit_carpan'a kayan birkaç dar bant süzgeçten geçirir.
    Süzgeç parça parça uygulanır (40 ms'lik dilimler, her dilimde yeni frekans): basit ve yeterli.
    perdeler: Hz listesi; birbirine yakın, uyumsuz perdeler (küme) seçilir ki "akort" olmasın.
    """
    s = [0.0] * n
    dilim = int(0.04 * HIZ)
    for f0 in perdeler:
        kaynak = gurultu(n)
        for bas in range(0, n, dilim):
            t = bas / n
            f = f0 * (bas_carpan + (bit_carpan - bas_carpan) * t)
            parca = bant(kaynak[bas:bas + dilim], f, HIZ, q)
            for i, x in enumerate(parca):
                if bas + i < n: s[bas + i] += x
    return s


def stinger(tohum, ad):
    random.seed(tohum)
    n = int(1.9 * HIZ)
    s = [0.0] * n
    # 1) Tırmanan uyumsuz küme: 4 perde, yarım ses ve çeyrek ses aralıklarıyla (uyumsuz), 1,0 -> 1,9 kat.
    kok = random.choice([196.0, 220.0, 233.0])
    perdeler = [kok, kok * 1.06, kok * 1.19, kok * 1.42]
    kume = kayan_kume(n, perdeler, 1.0, 1.9, 24)
    # Zarf: 60 ms'de patlar, 1,2 sn tepede titrer, sonda keskin kesilir (korku vuruşu bitişi).
    for i in range(n):
        t = i / HIZ
        g = min(1.0, t / 0.06) * (1.0 if t < 1.3 else max(0.0, 1.0 - (t - 1.3) / 0.45))
        titre = 1.0 + 0.18 * math.sin(2 * math.pi * 9.5 * t + 0.4 * math.sin(2 * math.pi * 1.3 * t))
        kume[i] *= g * titre
    ekle(s, tepeyi_esitle(kume), 0, 0.85)
    # 2) Alt-bas darbe: aynı anda, 45-70 Hz'e süzülmüş kısa gürültü patlaması.
    bas = alcak(alcak(darbe(int(0.5 * HIZ), HIZ, 0.09), 70, HIZ), 70, HIZ)
    ekle(s, tepeyi_esitle(bas), int(0.01 * HIZ), 0.9)
    # 3) Tiz cızırtı: 3-6 kHz'de tane tane gürültü (dişlerin arasından ses gibi), kümenin üstünde, kısık.
    ciz = alcak(yuksek(gurultu(n), 3000, HIZ), 6500, HIZ)
    for i in range(n):
        t = i / HIZ
        ciz[i] *= (0.5 + 0.5 * math.sin(2 * math.pi * 23 * t)) * (1.0 if 0.1 < t < 1.3 else 0.0)
    ekle(s, tepeyi_esitle(ciz), 0, 0.22)
    s = zarf(s, 0.003, 0.12)
    s = yanki(s, HIZ, boyut=0.9, sonum=0.5, islak=0.22, kuru=1.0, oda=1.4)
    yaz(ad, yumusak_sinirla(s, 2.0), STINGER_DB)


def ugultu():
    random.seed(4303)
    n = int(3.5 * HIZ)
    # Alçak, titreyen gürültü: 60-180 Hz bandı, üstünde 1,5 Hz'lik bir "nefes" dalgalanması; 0,6 sn'de
    # yükselir, 2 sn durur, 0,9 sn'de söner.
    g = alcak(yuksek(gurultu(n), 55, HIZ), 180, HIZ)
    g = alcak(g, 180, HIZ)
    for i in range(n):
        t = i / HIZ
        z = min(1.0, t / 0.6) * (1.0 if t < 2.6 else max(0.0, 1.0 - (t - 2.6) / 0.9))
        g[i] *= z * (0.7 + 0.3 * math.sin(2 * math.pi * 1.5 * t))
    # Üstüne 300-700 Hz'de çok kısık, taneli bir hırıltı (boğazdan ses gibi ama sinüs değil).
    h = alcak(yuksek(gurultu(n), 300, HIZ), 700, HIZ)
    for i in range(n):
        t = i / HIZ
        z = min(1.0, t / 0.6) * (1.0 if t < 2.6 else max(0.0, 1.0 - (t - 2.6) / 0.9))
        h[i] *= z * (0.5 + 0.5 * math.sin(2 * math.pi * 31 * t)) * 0.35
    s = [a + b for a, b in zip(g, h)]
    s = zarf(s, 0.05, 0.3)
    yaz("Cin_Ugultu", yumusak_sinirla(s, 1.6), UGULTU_DB)


def yaz(ad, s, hedef_db):
    yol = os.path.join(SESLER, ad + ".wav")
    kaydet(yol, s, HIZ, seviye_tepe(s, hedef_db))
    URETILENLER.append(yol)


def rapor():
    print()
    print(f"{'dosya':<20}{'süre':>7}{'tepe dB':>9}{'en gür 100ms':>14}")
    for yol in URETILENLER:
        with wave.open(yol, "rb") as d:
            n = d.getnframes(); x = [v / 32768 for v in struct.unpack("<%dh" % n, d.readframes(n))]
        tepe = max(abs(v) for v in x)
        print(f"{os.path.basename(yol)[:-4]:<20}{len(x) / HIZ:>7.2f}{20 * math.log10(tepe):>9.1f}{20 * math.log10(en_gur_rms(x)):>14.1f}")


if __name__ == "__main__":
    secim = sys.argv[1:] or ["stinger", "ugultu"]
    if "stinger" in secim:
        stinger(4301, "Cin_Stinger_1")
        stinger(4302, "Cin_Stinger_2")
    if "ugultu" in secim:
        ugultu()
    rapor()
