# telefon_uret.py
# Kapıcı odasındaki eski telefonun sesleri (#56). Proje kuralı: darbe ve metal sesleri
# gürültüden sentezlenir (ses_ortak.py); canlı sesler kayıttan gelir.
#
#   python telefon_uret.py            -> ikisi de
#   python telefon_uret.py zil        -> yalnızca zil
#
# ZİL: 90'ların duvar ya da masa telefonunda elektromıknatısla titreşen bir tokmak iki küçük
# metal zile sırayla vurur: saniyede ~25 vuruş, "trrrrrring". Bir çalma iki saniye sürer, sonra
# dört saniye sessizlik; dosya tam bir döngü (6 sn), Unity'de döngüye alınır.
# Her vuruş kısa bir gürültü patlaması; zillerin kendi frekansları (modlar) onu çınlatır. İki zilin
# frekansları biraz farklı: gerçek telefonda da zil çiftinin perdesi aynı değildir, bu yüzden
# ses titrek ve "eski" duyulur.
#
# AHİZE: ahizenin çatalından kalkması: plastik bir tık ve çatalın yay sesi.

import os, sys, random
sys.path.append(os.path.dirname(__file__))
import ses_ortak as s

HIZ = 44100
HEDEF = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", "..", "Assets", "Resources", "Sesler"))
random.seed(1994)

ZIL_A = [(1210, 55, 1.0), (2480, 70, 0.55), (3420, 80, 0.35), (4650, 90, 0.2)]
ZIL_B = [(1290, 55, 0.9), (2610, 70, 0.5), (3580, 80, 0.3), (4800, 90, 0.18)]


def zil():
    toplam = [0.0] * int(6.0 * HIZ)
    vurus = int(HIZ / 25)                      # iki vuruş arası
    uyari_a = [0.0] * len(toplam)
    uyari_b = [0.0] * len(toplam)
    for k, bas in enumerate(range(0, int(2.0 * HIZ), vurus)):
        d = s.darbe(int(0.004 * HIZ), HIZ, 0.003)
        s.ekle(uyari_a if k % 2 == 0 else uyari_b, d, bas, random.uniform(0.8, 1.0))
    a = s.modlar(uyari_a, HIZ, ZIL_A)
    b = s.modlar(uyari_b, HIZ, ZIL_B)
    for i in range(len(toplam)):
        toplam[i] = a[i] + b[i]
    toplam = s.yuksek(toplam, 400, HIZ)       # küçük zil: gümbürtü yok
    toplam = s.yanki(toplam, HIZ, boyut=0.35, sonum=0.5, islak=0.18, kuru=1.0, oda=0.5)   # küçük oda
    s.kaydet(os.path.join(HEDEF, "Telefon_Zili.wav"), s.yumusak_sinirla(toplam, 1.6), HIZ, tepe=0.8)


def ahize():
    n = int(0.5 * HIZ)
    toplam = [0.0] * n
    tik = s.modlar(s.darbe(n, HIZ, 0.006), HIZ, [(1800, 8, 1.0), (3400, 10, 0.6), (650, 5, 0.4)])
    yay = s.modlar(s.darbe(n, HIZ, 0.002), HIZ, [(2900, 40, 0.5), (5200, 50, 0.3)])
    s.ekle(toplam, tik, 0, 1.0)
    s.ekle(toplam, yay, int(0.03 * HIZ), 0.35)
    toplam = s.yanki(toplam, HIZ, boyut=0.3, sonum=0.5, islak=0.12, kuru=1.0, oda=0.5)
    s.kaydet(os.path.join(HEDEF, "Telefon_Ahize.wav"), s.yumusak_sinirla(toplam, 1.4), HIZ, tepe=0.7)


URETICILER = {"zil": zil, "ahize": ahize}

if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
