# goblen_uret.py
# 1. katın holündeki goblen tablonun kanaviçe dokusu (#83, Şahan'ın seçimi 2026-09-29).
# 90'ların Türk evinde evin hanımının işlediği çerçeveli goblen: göl kıyısında bir değirmen, arkada
# dağlar, önde çiçekler. Resim piksel piksel çizilir; her piksel bir "ilmek" olur ve 8x8'lik bir
# karede çarpı (X) biçiminde işlenir, aralarından bej kanaviçe görünür. Renkler biraz soluk (eski).
#
# Saf Python: PIL kurulu değil (bkz. Art/Cizim/scripts/cizim_uret.py). Çıktı:
#   Assets/Textures/Goblen_Degirmen.png  (en 96 x boy 72 ilmek, 768 x 576 piksel)
#
# ÇALIŞTIRMA:  python Art/Dokular/scripts/goblen_uret.py

import math, os, random, struct, zlib

KOK = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Textures", "Goblen_Degirmen.png")
EN, BOY, ILMEK = 96, 72, 8           # ilmek sayısı ve bir ilmeğin piksel boyu
rnd = random.Random(1994)            # sabit tohum: her çalıştırmada aynı resim

# Palet (0-255). Goblen iplikleri: az sayıda düz renk.
GOK_UST, GOK_ALT = (120, 160, 196), (206, 214, 214)
BULUT = (232, 232, 226)
DAG_UZAK, DAG_YAKIN, KAR = (118, 128, 150), (92, 104, 118), (226, 228, 230)
TEPE_ACIK, TEPE_KOYU = (112, 150, 78), (70, 110, 56)
GOL, GOL_ACIK = (66, 104, 150), (116, 150, 182)
DUVAR, DUVAR_GOLGE, CATI = (226, 220, 204), (184, 176, 160), (150, 62, 44)
KANAT, AGAC, GOVDE = (96, 70, 48), (44, 84, 48), (84, 58, 38)
CICEK_K, CICEK_S, CICEK_B, CIMEN = (184, 44, 50), (222, 184, 58), (236, 232, 224), (84, 126, 60)
KENAR = (48, 70, 52)                 # koyu yeşil çerçeve ilmekleri
KANAVICE = (206, 194, 164)           # ilmeklerin arasından görünen bej kumaş


def karistir(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


# ---------- RESİM (ilmek ızgarası) ----------
resim = [[None] * EN for _ in range(BOY)]      # resim[y][x], y = 0 üstte
for y in range(BOY):
    for x in range(EN):
        resim[y][x] = karistir(GOK_UST, GOK_ALT, y / 38)
# Bulutlar: iki yassı elips.
for cx, cy, rx, ry in ((22, 9, 9, 2.5), (66, 6, 12, 2.8)):
    for y in range(BOY):
        for x in range(EN):
            if ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 < 1:
                resim[y][x] = BULUT
# Uzak dağlar: iki sıra, üstleri karlı.
for x in range(EN):
    h1 = 30 - 9 * abs(math.sin(x / 13.0)) - 4 * math.sin(x / 5.0 + 1)
    h2 = 36 - 6 * abs(math.sin(x / 9.0 + 2)) - 3 * math.sin(x / 4.0)
    for y in range(BOY):
        if y > h1:
            resim[y][x] = KAR if y < h1 + 2.2 and h1 < 25 else DAG_UZAK
        if y > h2:
            resim[y][x] = DAG_YAKIN
# Göl: ortada yatay bant, yansımalı çizgiler.
for y in range(40, 50):
    for x in range(EN):
        resim[y][x] = GOL_ACIK if (y in (42, 46) and (x // 3) % 3 == 0) else GOL
# Tepeler: gölün önünde yeşil yamaçlar.
for x in range(EN):
    h = 48 + 3 * math.sin(x / 11.0) + 2 * math.sin(x / 4.0 + 1)
    for y in range(BOY):
        if y > h:
            resim[y][x] = TEPE_ACIK if y < h + 6 else TEPE_KOYU
# Değirmen: sağda tepede; gövde, çatı ve çarpı kanatlar.
DX, DY = 70, 34                        # kanatların merkezi
for y in range(DY, 53):
    for x in range(DX - 4, DX + 5):
        genis = 3 + (y - DY) * 0.18
        if abs(x - DX) <= genis:
            resim[y][x] = DUVAR_GOLGE if x > DX + genis - 1.5 else DUVAR
for y in range(DY - 3, DY + 1):
    for x in range(DX - 4, DX + 5):
        if abs(x - DX) <= (y - (DY - 4)):
            resim[y][x] = CATI
for i in range(-11, 12):
    for (px, py) in ((DX + i, DY - 2 + i), (DX + i, DY - 2 - i)):
        if 0 <= px < EN and 0 <= py < BOY and abs(i) > 1:
            resim[py][px] = KANAT
resim[48][DX] = resim[49][DX] = resim[50][DX] = KANAT      # kapı
# Ağaçlar: solda üç yuvarlak ağaç.
for cx, cy, r in ((12, 47, 4.5), (20, 49, 3.5), (6, 51, 3.0)):
    for y in range(BOY):
        for x in range(EN):
            if (x - cx) ** 2 + ((y - cy) * 1.2) ** 2 < r * r:
                resim[y][x] = AGAC
    for y in range(int(cy + r), int(cy + r + 4)):
        resim[y][cx] = GOVDE
# Ön plan: çimen ve çiçekler.
for y in range(60, BOY):
    for x in range(EN):
        resim[y][x] = CIMEN
for _ in range(70):
    x, y = rnd.randrange(3, EN - 3), rnd.randrange(60, BOY - 3)
    renk = rnd.choice((CICEK_K, CICEK_K, CICEK_S, CICEK_B))
    for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
        resim[y + dy][x + dx] = renk if (dx, dy) != (0, 0) else CICEK_S
# Çerçeve: iki ilmek kalınlığında koyu yeşil kenar.
for y in range(BOY):
    for x in range(EN):
        if x < 2 or y < 2 or x >= EN - 2 or y >= BOY - 2:
            resim[y][x] = KENAR


# ---------- İLMEKLER (piksel) ----------
G, Y = EN * ILMEK, BOY * ILMEK
pikseller = [KANAVICE + (255,)] * (G * Y)
for iy in range(BOY):
    for ix in range(EN):
        renk = resim[iy][ix]
        soluk = rnd.uniform(0.86, 1.0)                     # iplik rengindeki küçük farklar ve solma
        renk = tuple(min(255, int(c * soluk + 18)) for c in renk)
        for py in range(ILMEK):
            for px in range(ILMEK):
                # Çarpı ilmek: iki çapraz çizgi; kenarlara yakın yerde kumaş görünür.
                d1, d2 = abs(px - py), abs(px + py - (ILMEK - 1))
                if min(d1, d2) <= 1.2 and 0 < px < ILMEK - 1 and 0 < py < ILMEK - 1:
                    golge = 0.82 if d1 <= d2 else 1.0              # üstteki çapraz daha açık
                    c = tuple(int(v * golge) for v in renk)
                    pikseller[(iy * ILMEK + py) * G + ix * ILMEK + px] = c + (255,)
                elif px in (0, ILMEK - 1) or py in (0, ILMEK - 1):
                    k = tuple(int(v * 0.8) for v in KANAVICE)      # kanaviçe deliklerinin gölgesi
                    pikseller[(iy * ILMEK + py) * G + ix * ILMEK + px] = k + (255,)


def png_yaz(yol, genislik, yukseklik, piksel):
    """RGBA piksel listesini (0-255) sıkıştırılmış PNG olarak yazar (zlib, filtre 0)."""
    ham = bytearray()
    for y in range(yukseklik):
        ham.append(0)
        for x in range(genislik):
            ham.extend(piksel[y * genislik + x])

    def blok(tur, veri):
        return struct.pack(">I", len(veri)) + tur + veri + struct.pack(">I", zlib.crc32(tur + veri) & 0xFFFFFFFF)

    with open(yol, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(blok(b"IHDR", struct.pack(">IIBBBBB", genislik, yukseklik, 8, 6, 0, 0, 0)))
        f.write(blok(b"IDAT", zlib.compress(bytes(ham), 9)))
        f.write(blok(b"IEND", b""))


png_yaz(CIKTI, G, Y, pikseller)
print("[GOBLEN]", CIKTI, G, "x", Y)
