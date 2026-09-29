# yolluk_uret.py
# 2. katta Nuri Bey'in holündeki yolluk halının deseni (#83, Şahan'ın seçimi 2026-09-29).
# 1. kattaki kırmızı madalyonlu salon halısından farklı: bej zemin, iki uzun kenarda lacivert ve
# kiremit renkli kilim bordürü, ortada sırayla dizilen baklava (göz) motifleri. Doku bir "tekrar
# birimi": 1,6 m x 0,8 m'lik parça; yolluk boyunca tekrar eder (UV'si uzunluk / 1,6).
#
# Saf Python (PIL kurulu değil). Çıktı: Assets/Textures/Yolluk_Nuri.png (512 x 256 piksel).
# ÇALIŞTIRMA:  python Art/Dokular/scripts/yolluk_uret.py

import os, random, struct, zlib

KOK = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Textures", "Yolluk_Nuri.png")
G, Y = 512, 256
rnd = random.Random(1989)

BEJ = (196, 176, 138)
LACIVERT = (34, 44, 78)
KIREMIT = (150, 66, 40)
KAHVE = (92, 60, 38)
KREM = (222, 208, 176)


def renk(x, y):
    # y: halının eni (0-255), x: boyu (0-511, tekrar eder). Kenarlardan içeri bantlar.
    k = min(y, Y - 1 - y)                       # uzun kenara uzaklık (piksel)
    if k < 8:
        return KAHVE
    if k < 30:
        # Bordür: kiremit zeminde lacivert zikzak.
        zz = abs(((x % 32) - 16)) + 8           # zikzağın yüksekliği (8-24)
        return LACIVERT if abs((k - 8) - zz * 0.9) < 3 else KIREMIT
    if k < 36:
        return KREM
    # Orta alan: bej zemin, her 128 pikselde bir baklava (göz) motifi.
    cx = (x // 128) * 128 + 64
    cy = Y / 2
    d = abs(x - cx) / 56 + abs(y - cy) / 72    # baklava uzaklığı
    if d < 1.0:
        katman = int(d * 5)
        return (LACIVERT, KREM, KIREMIT, KREM, LACIVERT)[katman]
    # Baklavaların arasında küçük artılar.
    ax, ay = abs((x % 128) - 0), abs(y - cy)
    if (ax < 3 or ax > 125) and ay < 14:
        return KIREMIT
    return BEJ


piks = []
for y in range(Y):
    for x in range(G):
        c = renk(x, y)
        g = rnd.uniform(0.88, 1.04)
        piks.append(tuple(max(0, min(255, int(v * g))) for v in c) + (255,))


def png_yaz(yol, genislik, yukseklik, piksel):
    """RGBA piksel listesini (0-255) sıkıştırılmış PNG olarak yazar (zlib, filtre 0)."""
    ham = bytearray()
    for yy in range(yukseklik):
        ham.append(0)
        for xx in range(genislik):
            ham.extend(piksel[yy * genislik + xx])

    def blok(tur, veri):
        return struct.pack(">I", len(veri)) + tur + veri + struct.pack(">I", zlib.crc32(tur + veri) & 0xFFFFFFFF)

    with open(yol, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(blok(b"IHDR", struct.pack(">IIBBBBB", genislik, yukseklik, 8, 6, 0, 0, 0)))
        f.write(blok(b"IDAT", zlib.compress(bytes(ham), 9)))
        f.write(blok(b"IEND", b""))


png_yaz(CIKTI, G, Y, piks)
print("[YOLLUK]", CIKTI, G, "x", Y)
