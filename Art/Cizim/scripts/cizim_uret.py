# cizim_uret.py
# 3. kattaki çocuğun yırtılmış çizimi (#63): krayonla çizilmiş bir resim, dört parçaya yırtılmış.
#
# ÇALIŞTIRMA:  python Art/Cizim/scripts/cizim_uret.py
#
# Resim: dokuz katlı bir apartman (bina aslında sekiz katlı!), en üst penceresinde siyah bir
# adam; aşağıda binayı gösteren küçük bir çocuk; gökte ay ve yıldızlar. Köşelere çocuk dört
# büyük rakam yazmış: 0 3 0 9. Bunlar 3. katın asma kilidinin şifresi (annenin notu söyler).
# Dört köşe dört ayrı parçada olduğu için şifre ancak bütün parçalar bulununca okunur.
#
# NEDEN KODLA: bir resim çizme programı (PIL gibi) kurulu değil ve kurmak bir indirme olurdu.
# Krayon çizgisi basit bir kuralla taklit edilir: kalın bir fırça çizgi boyunca damga gibi
# basılır, ama kâğıdın "dişleri" (önceden rastgele üretilmiş bir pürüz haritası) boyayı yer
# yer tutmaz; çizgi krayondaki gibi grenli ve boşluklu çıkar.
#
# ÇIKTI: Assets/Textures/Cizim/Cizim_Kat3_P1..P4.png. Her parça TAM tuval boyunda (512 x 512)
# ve yalnızca kendi bölgesi opak, gerisi saydam: dört parça üst üste çizilince resim birleşir.
# Unity'deki yakın bakış ekranı bulunan parçaları aynı dikdörtgene üst üste çizer. Betik ayrıca
# her parçanın dokudaki bölgesini (UV) yazdırır; dünyadaki kâğıt parçası yalnızca o bölgeyi gösterir.

import math, os, random, struct, zlib

N = 512
random.seed(63)
KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Textures", "Cizim")

# ---------- TUVAL ----------
# Her piksel [r, g, b] (0-1). Kâğıt kırık beyaz, hafif lekeli.
tuval = [[0.0, 0.0, 0.0] for _ in range(N * N)]
for i in range(N * N):
    lek = random.uniform(-0.015, 0.015)
    tuval[i] = [0.93 + lek, 0.90 + lek, 0.82 + lek]

# Kâğıdın dişleri: her piksel için 0-1 arası bir değer; küçük değerli yerler boyayı tutmaz.
disler = [random.random() for _ in range(N * N)]


def boya(x, y, renk, guc):
    """(x, y) pikseline krayon rengini 'guc' oranında sürer (kâğıdın dişi izin verirse)."""
    if not (0 <= x < N and 0 <= y < N):
        return
    i = y * N + x
    if disler[i] < 0.28:               # diş boşluğu: krayon buraya değmez
        return
    p = tuval[i]
    for k in range(3):
        p[k] = p[k] * (1 - guc) + renk[k] * guc


def damga(cx, cy, r, renk):
    """Yuvarlak bir fırça izi; kenara doğru hafifleyen."""
    for y in range(int(cy - r), int(cy + r) + 1):
        for x in range(int(cx - r), int(cx + r) + 1):
            d = math.hypot(x - cx, y - cy)
            if d <= r:
                boya(x, y, renk, 0.85 * (1 - 0.5 * d / r))


def cizgi(noktalar, renk, r=4.0):
    """Noktaları sırayla birleştiren krayon çizgisi. Çocuk eli: her damga biraz titrer."""
    for (x0, y0), (x1, y1) in zip(noktalar, noktalar[1:]):
        uzun = max(1.0, math.hypot(x1 - x0, y1 - y0))
        adim = int(uzun / 1.5) + 1
        for s in range(adim + 1):
            t = s / adim
            damga(x0 + (x1 - x0) * t + random.uniform(-0.8, 0.8),
                  y0 + (y1 - y0) * t + random.uniform(-0.8, 0.8), r, renk)


def daire_noktalari(cx, cy, rx, ry, bas=0.0, son=2 * math.pi, n=40):
    return [(cx + rx * math.cos(bas + (son - bas) * i / n), cy + ry * math.sin(bas + (son - bas) * i / n)) for i in range(n + 1)]


def karala(x0, y0, x1, y1, renk, r=2.5, ara=6):
    """Dikdörtgeni çocuk gibi zikzakla doldurur."""
    noktalar, sag = [], True
    y = y0
    while y <= y1:
        noktalar.append((x1 if sag else x0, y))
        sag = not sag
        y += ara
    cizgi(noktalar, renk, r)


def kutu(x0, y0, x1, y1, renk, r=3.0):
    cizgi([(x0, y0), (x1, y0), (x1, y1), (x0, y1), (x0, y0)], renk, r)


# ---------- RENKLER ----------
KAHVE = (0.45, 0.28, 0.14)
SARI = (0.95, 0.78, 0.15)
MAVI = (0.22, 0.35, 0.75)
SIYAH = (0.08, 0.07, 0.07)
KIRMIZI = (0.80, 0.10, 0.08)
YESIL = (0.20, 0.55, 0.20)

# ---------- RESİM ----------
# Zemin: dalgalı yeşil çizgi.
cizgi([(x, 468 + 5 * math.sin(x / 23.0)) for x in range(20, 500, 12)], YESIL, 3.5)

# Bina: zemin kat (kapı) ve üstünde DOKUZ kat, her katta iki pencere (gerçek bina sekiz katlı:
# çocuk bir kat fazla çizmiş). Sekiz kat sarı ya da mavi karalanmış; en üstteki (dokuzuncu)
# katın pencereleri boş, soldakinde siyah bir adam, gözleri kırmızı.
bx0, bx1, by0, by1 = 186, 326, 58, 468
kutu(bx0, by0, bx1, by1, KAHVE, 4.0)
cizgi([(bx0 - 10, by0 + 2), (256, by0 - 42), (bx1 + 10, by0 + 2)], KAHVE, 4.0)   # çatı üçgeni
KAT = 10                                                  # zemin + dokuz kat
kat_boy = (by1 - by0) / KAT
for k in range(KAT):
    y_ust = by0 + k * kat_boy
    for px in (bx0 + 22, bx1 - 22 - 34):
        wy0, wy1 = y_ust + 8, y_ust + 8 + 24
        if k == KAT - 1:
            continue                                      # en alt kat: kapı var, pencere yok
        kutu(px, wy0, px + 34, wy1, SIYAH, 2.0)
        if k == 0:
            continue                                      # dokuzuncu kat: içi boş
        karala(px + 4, wy0 + 4, px + 30, wy1 - 4, SARI if (k + px) % 3 else MAVI)
# Kapı.
kutu(236, by1 - 40, 276, by1, KAHVE, 3.0)
# Dokuzuncu kattaki adam (sol pencere): penceresinden büyük, çocuk çizimi gibi orantısız.
ax, ay = bx0 + 22 + 17, by0 + 8
cizgi(daire_noktalari(ax, ay + 7, 7, 7, n=20), SIYAH, 2.6)          # baş
karala(ax - 4, ay + 3, ax + 4, ay + 11, SIYAH, 2.0, 3)              # kapkara yüz
cizgi([(ax, ay + 14), (ax, ay + 34)], SIYAH, 2.8)                   # gövde (pencereden taşar)
cizgi([(ax - 14, ay + 18), (ax + 14, ay + 18)], SIYAH, 2.6)         # uzun kollar
damga(ax - 3, ay + 6, 2.0, KIRMIZI)                                 # kırmızı gözler
damga(ax + 3, ay + 6, 2.0, KIRMIZI)

# Aşağıda binayı gösteren küçük çocuk.
cx, cy = 128, 400
cizgi(daire_noktalari(cx, cy, 9, 9, n=20), SIYAH, 2.5)
cizgi([(cx, cy + 9), (cx, cy + 40)], SIYAH, 2.5)
cizgi([(cx, cy + 20), (cx + 26, cy + 2)], SIYAH, 2.5)                # binayı gösteren kol
cizgi([(cx, cy + 20), (cx - 14, cy + 30)], SIYAH, 2.5)
cizgi([(cx, cy + 40), (cx - 10, cy + 62)], SIYAH, 2.5)
cizgi([(cx, cy + 40), (cx + 10, cy + 62)], SIYAH, 2.5)

# Ay (hilal) ve yıldızlar.
cizgi(daire_noktalari(140, 175, 22, 22, math.radians(80), math.radians(290), 30), SARI, 4.0)
cizgi(daire_noktalari(150, 175, 16, 18, math.radians(95), math.radians(265), 24), SARI, 3.0)
for sx, sy in ((380, 170), (420, 230), (110, 260), (400, 300), (160, 110)):
    cizgi([(sx - 7, sy), (sx + 7, sy)], SARI, 2.2)
    cizgi([(sx, sy - 7), (sx, sy + 7)], SARI, 2.2)


# Köşelerdeki rakamlar: 0 3 0 9 (sol üst, sağ üst, sol alt, sağ alt), büyük ve kırmızı.
def rakam(d, cx, cy, olcek=1.0, r=6.0):
    s = 32 * olcek
    if d == 0:
        cizgi(daire_noktalari(cx, cy, s * 0.62, s, n=36), KIRMIZI, r)
    elif d == 3:
        cizgi(daire_noktalari(cx, cy - s * 0.5, s * 0.55, s * 0.5, math.radians(200), math.radians(450), 24), KIRMIZI, r)
        cizgi(daire_noktalari(cx, cy + s * 0.5, s * 0.6, s * 0.5, math.radians(-90), math.radians(160), 24), KIRMIZI, r)
    elif d == 9:
        cizgi(daire_noktalari(cx, cy - s * 0.45, s * 0.55, s * 0.55, n=30), KIRMIZI, r)
        cizgi([(cx + s * 0.55, cy - s * 0.45), (cx + s * 0.45, cy + s * 0.5), (cx - s * 0.2, cy + s)], KIRMIZI, r)


# Çocuk binanın yanına en üst katın numarasını da yazmış: küçük bir "9".
rakam(9, 350, by0 + 22, 0.45, 2.4)
rakam(0, 62, 72)
rakam(3, 450, 72)
rakam(0, 62, 430)
rakam(9, 450, 430)

# ---------- YIRTIK ÇİZGİLER ----------
# Dikey ve yatay yırtık: ortadan geçen, dalgalı ve pürüzlü iki çizgi.
def yirtik(n, taban):
    cizgi_, sapma = [], 0.0
    for i in range(n):
        sapma += random.uniform(-1.6, 1.6)                # pürüz: küçük rastgele adımlar
        sapma *= 0.92                                     # merkezden fazla uzaklaşmasın
        cizgi_.append(taban + 11 * math.sin(i / 41.0) + 5 * math.sin(i / 11.0 + 1.3) + sapma)
    return cizgi_


dikey = yirtik(N, 262)      # dikey[y] = o satırda yırtığın x'i
yatay = yirtik(N, 250)      # yatay[x] = o sütunda yırtığın y'si


def parca_no(x, y):
    """Pikselin hangi parçada olduğu (0-3) ya da yırtığın üstündeyse -1."""
    dx, dy = x - dikey[y], y - yatay[x]
    if abs(dx) < 1.6 or abs(dy) < 1.6:
        return -1
    return (0 if dy < 0 else 2) + (0 if dx < 0 else 1)


# ---------- PNG YAZMA ----------
def png_yaz(yol, pikseller):
    """RGBA piksel listesini (0-255) sıkıştırılmış PNG olarak yazar (zlib, filtre 0)."""
    ham = bytearray()
    for y in range(N):
        ham.append(0)                                     # satır filtresi: yok
        for x in range(N):
            ham.extend(pikseller[y * N + x])

    def blok(tur, veri):
        return struct.pack(">I", len(veri)) + tur + veri + struct.pack(">I", zlib.crc32(tur + veri) & 0xFFFFFFFF)

    with open(yol, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(blok(b"IHDR", struct.pack(">IIBBBBB", N, N, 8, 6, 0, 0, 0)))
        f.write(blok(b"IDAT", zlib.compress(bytes(ham), 9)))
        f.write(blok(b"IEND", b""))


os.makedirs(CIKTI, exist_ok=True)
for p in range(4):
    pikseller = []
    xmin = ymin = N
    xmax = ymax = -1
    for y in range(N):
        for x in range(N):
            if parca_no(x, y) != p:
                pikseller.append((0, 0, 0, 0))
                continue
            r, g, b = tuval[y * N + x]
            # Yırtığa 4 pikselden yakın yerde kâğıdın lifleri biraz koyu görünür.
            if min(abs(x - dikey[y]), abs(y - yatay[x])) < 4:
                r, g, b = r * 0.88, g * 0.88, b * 0.88
            pikseller.append((int(max(0, min(1, r)) * 255), int(max(0, min(1, g)) * 255), int(max(0, min(1, b)) * 255), 255))
            xmin, xmax, ymin, ymax = min(xmin, x), max(xmax, x), min(ymin, y), max(ymax, y)
    yol = os.path.join(CIKTI, f"Cizim_Kat3_P{p + 1}.png")
    png_yaz(yol, pikseller)
    # Unity'de doku koordinatı (UV) sol ALTTAN başlar; resimde y yukarıdan aşağı.
    u0, u1 = xmin / N, (xmax + 1) / N
    v0, v1 = 1 - (ymax + 1) / N, 1 - ymin / N
    print(f"OK P{p + 1} bolge u={u0:.4f}-{u1:.4f} v={v0:.4f}-{v1:.4f}")
