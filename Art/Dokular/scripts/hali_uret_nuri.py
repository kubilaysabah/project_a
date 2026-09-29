# hali_uret_nuri.py
# 2. katta Nuri Bey'in salonundaki halının deseni (#83, Şahan'ın seçimi 2026-09-29).
# 1. kattaki kırmızı halıdan farklı (katlarda aynı model/desen yok): lacivert zemin, bej-kiremit madalyon,
# bordo bordür; madalyon yuvarlak, zeminde sık küçük baklavalar. Çatı hali_uret.py'den.
# 90'ların salon halısı: kırmızı zemin, ortada lacivert-bej madalyon, dört köşede köşe göbekleri,
# lacivert bordür ve içinde tekrarlanan küçük motifler, iki kısa kenarda saçak hizası. Hafif soluk
# ve yer yer aşınmış (yıllarca üstünde yürünmüş). Doku yatay: halının uzun kenarı genişlik.
#
# Saf Python (PIL kurulu değil). Çıktı: Assets/Textures/Hali_Nuri.png (1024 x 724 piksel, 2,4 x 1,7 m).
#
# ÇALIŞTIRMA:  python Art/Dokular/scripts/hali_uret.py

import math, os, random, struct, zlib

KOK = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Textures", "Hali_Nuri.png")
G, Y = 1024, 724
rnd = random.Random(1970)

KIRMIZI = (34, 44, 88)        # zemin: lacivert (ad kodla uyumlu kalsın diye aynı)
KIRMIZI_KOYU = (150, 70, 44)   # madalyonun iç katmanı: kiremit
LACIVERT = (110, 24, 30)       # bordür ve madalyon kenarı: bordo
BEJ = (214, 190, 146)
SARI = (196, 150, 70)
YESIL = (48, 86, 62)


def karistir(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))


def renk(x, y):
    # Normalleştirilmiş koordinatlar: merkez (0, 0), kenarlar ±1.
    nx, ny = (x / G) * 2 - 1, (y / Y) * 2 - 1
    ax, ay = abs(nx), abs(ny)
    # Bordür: dış kenardan içeri üç bant (lacivert, bej ince, lacivert motifli).
    kx, ky = 1 - ax, 1 - ay                    # kenara uzaklık (0 kenarda)
    kenar = min(kx * G / 2, ky * Y / 2)        # piksel cinsinden kenara uzaklık
    if kenar < 10:
        return BEJ                             # dış ince bej çizgi
    if kenar < 70:
        # Bordürün içinde tekrarlanan elmas motifleri.
        boy = y if kx * G / 2 < ky * Y / 2 else x      # sol/sağ bantta boyuna y, üst/alt bantta x
        ic = (kenar - 10) / 60
        d = abs(((boy % 60) / 60) - 0.5) * 2 + abs(ic - 0.5) * 2
        if d < 0.55:
            return SARI if d < 0.25 else KIRMIZI
        return LACIVERT
    if kenar < 78:
        return BEJ
    # Zemin: kırmızı, ortada madalyon (baklava biçimli, iç içe katmanlı).
    m = math.sqrt((ax * 1.0) ** 2 + (ay * 1.45) ** 2)   # 1. kattakinden farklı: yuvarlak (elips) madalyon
    if m < 0.50:
        katman = int(m / 0.083)
        return (KIRMIZI_KOYU, BEJ, LACIVERT, BEJ, KIRMIZI_KOYU, LACIVERT)[min(katman, 5)]
    # Köşe göbekleri: köşelerde çeyrek madalyon.
    cx, cy = (ax - 0.86) / 0.3, (ay - 0.80) / 0.3
    if cx > -1 and cy > -1:
        km = abs(1 - ax) * 2.3 + abs(1 - ay) * 2.3
        if km < 0.98:
            return LACIVERT if km < 0.86 else BEJ
    # Zeminde dağınık küçük çiçek motifleri.
    # Her motif sekiz kollu küçük bir yıldız: bir baklava ve bir artı işareti üst üste.
    fx, fy = abs((x % 48) - 24), abs((y % 48) - 24)
    if fx + fy < 7:                                     # sık dizilmiş küçük baklavalar
        return SARI if (x // 48 + y // 48) % 2 == 0 else BEJ
    return KIRMIZI


pikseller = []
for y in range(Y):
    for x in range(G):
        c = renk(x, y)
        # Aşınma: ortada ve yürüme yolunda hafif açılma; iplik dokusu için küçük gürültü.
        nx, ny = (x / G) * 2 - 1, (y / Y) * 2 - 1
        asinma = 0.10 * math.exp(-(nx * nx * 2 + ny * ny * 3))
        c = karistir(c, (190, 160, 130), asinma)
        g = rnd.uniform(0.88, 1.04) * (0.96 if (x + y) % 3 == 0 else 1.0)
        pikseller.append(tuple(max(0, min(255, int(v * g))) for v in c) + (255,))


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


png_yaz(CIKTI, G, Y, pikseller)
print("[HALI_NURI]", CIKTI, G, "x", Y)
