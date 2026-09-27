# kapi_sesleri_uret.py
# Kat kapısının sesleri (#52): kilitli kapıyı sarsma, kilidin açılması, demir kapının gıcırtısı.
# Dairedeki oda kapılarının sesleri (#53): açılma, kapanma, kilitli kolu zorlama.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/kapi_sesleri_uret.py
#
# Hepsi ses_ortak.py'deki yöntemle: gürültü darbesi → cismin (demir) modları → merdiven yankısı.
# Demir, tahtadan farklı olarak İNCE ve UZUN çınlar (q büyük); parmaklıklı kapıda birçok
# çubuk birlikte titrediği için birkaç yakın mod üst üste biner ("şangırtı").

import math, os, random, sys
sys.path.append(os.path.dirname(__file__))
from ses_ortak import alcak, yuksek, bant, modlar, darbe, gurultu, ekle, yanki, tepeyi_esitle, yumusak_sinirla, kaydet

HIZ = 44100
KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")

# Demir parmaklığın modları: birbirine yakın, dar, uzun çınlayan frekanslar.
PARMAKLIK = [(410, 60, 0.6), (620, 80, 1.0), (935, 90, 0.7), (1380, 70, 0.5), (2100, 50, 0.3), (3300, 40, 0.15)]
# Kilit mekanizmasının modları: küçük, sert metal parçalar; kısa ve tiz.
KILIT = [(1800, 25, 0.8), (2700, 30, 1.0), (4100, 20, 0.5), (6200, 15, 0.25)]


def sarsma():
    """Kilitli kapı: kanadı çekip bırakma; kanat kasaya iki üç kez çarpar, çubuklar şangırdar."""
    random.seed(21)
    n = int(1.2 * HIZ)
    s = [0.0] * n
    for t, guc in ((0.0, 1.0), (0.09, 0.7), (0.21, 0.45)):
        vurus = tepeyi_esitle(modlar(darbe(int(0.6 * HIZ), HIZ, 0.003), HIZ, PARMAKLIK))
        gum = tepeyi_esitle(alcak(darbe(int(0.3 * HIZ), HIZ, 0.008), 220, HIZ))
        ekle(s, vurus, int(t * HIZ), guc)
        ekle(s, gum, int(t * HIZ), guc * 0.8)
    s = yanki(s, HIZ, boyut=0.8, sonum=0.4, islak=0.35, oda=1.2)
    kaydet(os.path.join(SESLER, "KatKapisi_Kilitli.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.8)


def kilit_acilma():
    """Anahtar deliğe girer (sürtünme), döner (iki tık), dil çekilir (tok bir şak)."""
    random.seed(22)
    n = int(1.0 * HIZ)
    s = [0.0] * n
    surt = bant(gurultu(n), 3500, HIZ, 1.2)
    for i in range(int(0.02 * HIZ), int(0.14 * HIZ)):
        s[i] += 0.12 * surt[i] * math.sin(math.pi * (i / HIZ - 0.02) / 0.12)
    for t, guc in ((0.28, 0.7), (0.40, 1.0)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.25 * HIZ), HIZ, 0.001), HIZ, KILIT)), int(t * HIZ), guc)
    sak = tepeyi_esitle(modlar(darbe(int(0.4 * HIZ), HIZ, 0.004), HIZ, [(300, 8, 1.0), (760, 20, 0.6), (1500, 25, 0.4)]))
    ekle(s, sak, int(0.55 * HIZ), 1.0)
    s = yanki(s, HIZ, boyut=0.7, sonum=0.45, islak=0.25, oda=1.0)
    kaydet(os.path.join(SESLER, "KatKapisi_Kilit.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.75)


def gicirti():
    """Ağır demir kanadın menteşede gıcırdaması: 1,6 sn boyunca perdesi kayan, kesik kesik sürtünme."""
    random.seed(23)
    n = int(1.8 * HIZ)
    s = [0.0] * n
    faz = 0.0
    for i in range(n):
        t = i / HIZ
        if t > 1.6:
            break
        f = 900 + 500 * math.sin(2 * math.pi * 0.4 * t) + 120 * math.sin(2 * math.pi * 7 * t)   # perde kayar
        faz += 2 * math.pi * f / HIZ
        # "yapış-kay": sürtünme kesik kesik; 9 Hz'lik bir titreme ve rastgele kopmalar
        zarf = 0.5 + 0.5 * math.sin(2 * math.pi * 9 * t) ** 2
        zarf *= 1.0 if random.random() > 0.002 else 0.0
        e = min(1.0, t / 0.15, (1.6 - t) / 0.4)
        s[i] = e * zarf * (math.sin(faz) + 0.5 * math.sin(2 * faz) + 0.25 * math.sin(3 * faz))
    s = bant(s, 1400, HIZ, 0.5)
    ekle(s, tepeyi_esitle(alcak(darbe(int(0.3 * HIZ), HIZ, 0.01), 180, HIZ)), int(1.62 * HIZ), 0.7)   # kanat sona dayanır
    s = yanki(s, HIZ, boyut=0.8, sonum=0.4, islak=0.3, oda=1.2)
    kaydet(os.path.join(SESLER, "KatKapisi_Gicirti.wav"), s, HIZ, 0.7)


def anahtar_al():
    """Anahtarı yerden alma: küçük bir pirinç şıkırtı."""
    random.seed(24)
    n = int(0.5 * HIZ)
    s = [0.0] * n
    for t, guc in ((0.0, 1.0), (0.05, 0.6)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.3 * HIZ), HIZ, 0.0008), HIZ, [(3200, 40, 1.0), (5100, 35, 0.6), (7400, 25, 0.3)])), int(t * HIZ), guc)
    kaydet(os.path.join(SESLER, "Anahtar_Al.wav"), s, HIZ, 0.6)


# ---------- ODA KAPILARI (#53) ----------
# Tahta kapı demirin tersine KALIN ve SÖNÜMLÜ çınlar: modları alçak ve kısa (q küçük).
# Kol ve dil (kapının kasaya giren küçük demir parçası) küçük metal modlar: kısa, tiz tıklar.
TAHTA = [(95, 3, 1.0), (180, 4, 0.7), (310, 5, 0.45), (520, 6, 0.25)]
DIL = [(2300, 18, 1.0), (3400, 22, 0.6), (5200, 15, 0.3)]
ODA = dict(boyut=0.35, sonum=0.55, islak=0.18, oda=0.6)   # küçük, eşyalı oda: kısa yankı


def ic_acilma():
    """Kol iner (tık), dil kasadan çıkar (tık), kanat hafifçe gıcırdayarak açılır."""
    random.seed(31)
    n = int(1.1 * HIZ)
    s = [0.0] * n
    for t, guc in ((0.0, 0.6), (0.07, 1.0)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.2 * HIZ), HIZ, 0.0008), HIZ, DIL)), int(t * HIZ), guc * 0.7)
    # Kısa, alçak bir menteşe gıcırtısı: demir kapınınkinden kısa ve daha tok.
    faz = 0.0
    for i in range(int(0.15 * HIZ), int(0.85 * HIZ)):
        t = i / HIZ - 0.15
        f = 520 + 180 * math.sin(2 * math.pi * 0.9 * t)
        faz += 2 * math.pi * f / HIZ
        zarf = min(1.0, t / 0.1, (0.7 - t) / 0.25) * (0.6 + 0.4 * math.sin(2 * math.pi * 11 * t) ** 2)
        s[i] += 0.25 * zarf * (math.sin(faz) + 0.4 * math.sin(2 * faz))
    s = bant(s, 900, HIZ, 0.6)
    s = yanki(s, HIZ, **ODA)
    kaydet(os.path.join(SESLER, "IcKapi_Acilma.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.6)


def ic_kapanma():
    """Kanat kasaya oturur (tok tahta vuruşu), dil yerine girer (tık)."""
    random.seed(32)
    n = int(0.9 * HIZ)
    s = [0.0] * n
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.5 * HIZ), HIZ, 0.004), HIZ, TAHTA)), 0, 1.0)
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.2 * HIZ), HIZ, 0.0008), HIZ, DIL)), int(0.02 * HIZ), 0.5)
    s = yanki(s, HIZ, **ODA)
    kaydet(os.path.join(SESLER, "IcKapi_Kapanma.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.7)


def ic_kilitli():
    """Kol iki kez aşağı bastırılır; dil kilide dayanır, kanat kasada hafifçe takırdar."""
    random.seed(33)
    n = int(1.0 * HIZ)
    s = [0.0] * n
    for t, guc in ((0.0, 1.0), (0.32, 0.8)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.2 * HIZ), HIZ, 0.0008), HIZ, DIL)), int(t * HIZ), guc * 0.6)
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.35 * HIZ), HIZ, 0.003), HIZ, TAHTA)), int((t + 0.03) * HIZ), guc * 0.8)
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.25 * HIZ), HIZ, 0.0008), HIZ, DIL)), int((t + 0.14) * HIZ), guc * 0.4)
    s = yanki(s, HIZ, **ODA)
    kaydet(os.path.join(SESLER, "IcKapi_Kilitli.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.7)


# ---------- ŞİFRELİ ASMA KİLİT (#54) ----------
def sifre_cark():
    """Çarkın bir rakam dönüşü: küçük, kuru bir metal tık (yankısız, yakından duyulur)."""
    random.seed(41)
    n = int(0.12 * HIZ)
    s = [0.0] * n
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.08 * HIZ), HIZ, 0.0004), HIZ, [(3800, 30, 1.0), (5600, 25, 0.5)])), 0, 1.0)
    kaydet(os.path.join(SESLER, "Sifre_Cark.wav"), s, HIZ, 0.5)


# ---------- KOL (#54) ----------
def kol_cekme():
    """Paslı demir kolun çekilişi: kısa bir sürtünme, cırcır tıkırtıları, sonda tok bir vuruş."""
    random.seed(42)
    n = int(0.9 * HIZ)
    s = [0.0] * n
    surt = bant(gurultu(n), 1800, HIZ, 0.8)
    for i in range(int(0.30 * HIZ)):
        s[i] += 0.18 * surt[i] * math.sin(math.pi * i / (0.30 * HIZ))
    for j, t in enumerate((0.06, 0.13, 0.19, 0.24, 0.28)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.08 * HIZ), HIZ, 0.0006), HIZ, KILIT)), int(t * HIZ), 0.35 + j * 0.05)
    vurus = tepeyi_esitle(modlar(darbe(int(0.5 * HIZ), HIZ, 0.004), HIZ, [(180, 6, 1.0), (420, 12, 0.6), (980, 20, 0.3)]))
    ekle(s, vurus, int(0.40 * HIZ), 1.0)
    s = yanki(s, HIZ, boyut=0.4, sonum=0.5, islak=0.2, oda=0.6)
    kaydet(os.path.join(SESLER, "Kol_Cekme.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.75)


# ---------- SİGORTA (#54) ----------
def sigorta_takma():
    """Buşon yuvasına vidalanır (porselen sürtünmesi, iki tık), sonra elektrik gelir: kısa bir
    cızırtı ve 50 Hz'lik alçak bir vınlama."""
    random.seed(43)
    n = int(1.4 * HIZ)
    s = [0.0] * n
    surt = bant(gurultu(n), 2600, HIZ, 1.0)
    for i in range(int(0.35 * HIZ)):
        s[i] += 0.10 * surt[i] * (0.6 + 0.4 * math.sin(2 * math.pi * 14 * i / HIZ))
    for t, guc in ((0.36, 0.8), (0.46, 1.0)):
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.1 * HIZ), HIZ, 0.0005), HIZ, KILIT)), int(t * HIZ), guc)
    # Elektrik: kısa cızırtı + sönerek süren şebeke vınlaması (50 Hz ve harmonikleri).
    ciz = yuksek(gurultu(int(0.08 * HIZ)), 3000, HIZ)
    ekle(s, tepeyi_esitle(ciz), int(0.58 * HIZ), 0.35)
    for i in range(int(0.60 * HIZ), n):
        t = (i - int(0.60 * HIZ)) / HIZ
        zarf = min(1.0, t / 0.05) * math.exp(-t * 2.2)
        s[i] += 0.22 * zarf * (math.sin(2 * math.pi * 50 * t) + 0.5 * math.sin(2 * math.pi * 100 * t) + 0.25 * math.sin(2 * math.pi * 150 * t))
    kaydet(os.path.join(SESLER, "Sigorta_Takma.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.7)


# ---------- TORNAVİDA (#54) ----------
def vida_sokme():
    """Vidanın gevşeyişi: tornavidanın vida yuvasında kısa gıcırtısı ve cızırtılı dönüşü."""
    random.seed(44)
    n = int(0.55 * HIZ)
    s = [0.0] * n
    surt = bant(gurultu(n), 4200, HIZ, 1.5)
    for i in range(int(0.45 * HIZ)):
        t = i / HIZ
        s[i] += 0.16 * surt[i] * (0.5 + 0.5 * math.sin(2 * math.pi * 18 * t) ** 2) * min(1.0, t / 0.03)
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.1 * HIZ), HIZ, 0.0005), HIZ, KILIT)), int(0.02 * HIZ), 0.6)
    kaydet(os.path.join(SESLER, "Vida_Sokme.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.55)


def kapi_devrilme():
    """Menteşesiz kapının devrilişi: tahta kanat yere çarpar, bir kez seker, oda yankılanır."""
    random.seed(45)
    n = int(1.6 * HIZ)
    s = [0.0] * n
    buyuk = [(70, 3, 1.0), (140, 4, 0.8), (260, 5, 0.5), (480, 6, 0.3)]
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.8 * HIZ), HIZ, 0.01), HIZ, buyuk)), 0, 1.0)
    ekle(s, tepeyi_esitle(modlar(darbe(int(0.5 * HIZ), HIZ, 0.006), HIZ, buyuk)), int(0.16 * HIZ), 0.35)
    ekle(s, tepeyi_esitle(alcak(gurultu(int(0.3 * HIZ)), 500, HIZ)), 0, 0.25)
    s = yanki(s, HIZ, boyut=0.6, sonum=0.45, islak=0.3, oda=0.9)
    kaydet(os.path.join(SESLER, "Kapi_Devrilme.wav"), yumusak_sinirla(s, 2.0), HIZ, 0.85)


URETICILER = {"vida_sokme": vida_sokme, "kapi_devrilme": kapi_devrilme, "sigorta_takma": sigorta_takma, "kol_cekme": kol_cekme, "sifre_cark": sifre_cark, "sarsma": sarsma, "kilit_acilma": kilit_acilma, "gicirti": gicirti, "anahtar_al": anahtar_al,
              "ic_acilma": ic_acilma, "ic_kapanma": ic_kapanma, "ic_kilitli": ic_kilitli}

if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
