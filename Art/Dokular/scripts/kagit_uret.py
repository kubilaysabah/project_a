# -*- coding: utf-8 -*-
"""kagit_uret.py — not kâğıdı dokusu (#128). Saf Python (PIL yok): cizim_uret.py'nin PNG yazıcısıyla aynı yol.

Çıktı: Assets/Resources/Dokular/Kagit.png (512x512, RGB). Sararmış, lifli, kenarları hafif koyu bir
kâğıt; notlar (YoneticiNotu) ve pano (Pano) kâğıt dikdörtgenini düz renk yerine bununla çizer.
Çalıştır: python Art/Dokular/scripts/kagit_uret.py
"""
import math, os, random, struct, zlib

random.seed(1974)
G = 512

def png_yaz(yol, pikseller):
    def parca(tur, veri):
        return struct.pack(">I", len(veri)) + tur + veri + struct.pack(">I", zlib.crc32(tur + veri) & 0xFFFFFFFF)
    ham = b"".join(b"\x00" + bytes(satir) for satir in pikseller)
    with open(yol, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(parca(b"IHDR", struct.pack(">IIBBBBB", G, G, 8, 2, 0, 0, 0)))
        f.write(parca(b"IDAT", zlib.compress(ham, 9)))
        f.write(parca(b"IEND", b""))

# Değer gürültüsü: birkaç oktav, lifli görünsün diye yatay yönde esnetilmiş.
def gurultu_tablo(olcek, sx=1.0, sy=1.0):
    n = {}
    def deger(ix, iy):
        if (ix, iy) not in n: n[(ix, iy)] = random.random()
        return n[(ix, iy)]
    def al(x, y):
        fx, fy = x / olcek * sx, y / olcek * sy
        ix, iy = int(fx), int(fy); tx, ty = fx - ix, fy - iy
        tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty)
        a = deger(ix, iy) * (1 - tx) + deger(ix + 1, iy) * tx
        b = deger(ix, iy + 1) * (1 - tx) + deger(ix + 1, iy + 1) * tx
        return a * (1 - ty) + b * ty
    return al

kaba = gurultu_tablo(96)          # büyük lekeler (sararma)
orta = gurultu_tablo(24, 1.0, 3.0)  # lifler: yatay uzun
ince = gurultu_tablo(6, 1.0, 2.0)
satirlar = []
for y in range(G):
    satir = []
    for x in range(G):
        v = 0.55 * kaba(x, y) + 0.30 * orta(x, y) + 0.15 * ince(x, y)   # 0-1
        # Taban: sararmış beyaz; gürültü ±%6 parlaklık, sararma lekeleri biraz daha sıcak.
        r, g, b = 0.90, 0.86, 0.74
        p = 0.94 + 0.12 * (v - 0.5)
        sari = 0.03 * (kaba(x + 200, y + 100) - 0.5)
        r, g, b = r * p + sari, g * p + sari * 0.6, b * p - sari * 0.8
        # Kenarlar hafif koyu (eski kâğıt, ışıkta kalmış orta).
        dx, dy = abs(x - G / 2) / (G / 2), abs(y - G / 2) / (G / 2)
        kenar = max(dx, dy) ** 4 * 0.10
        r, g, b = r - kenar, g - kenar, b - kenar * 0.8
        satir += [max(0, min(255, int(c * 255 + 0.5))) for c in (r, g, b)]
    satirlar.append(satir)

cikti = os.path.join(os.path.dirname(__file__), "..", "..", "..", "Assets", "Resources", "Dokular")
os.makedirs(cikti, exist_ok=True)
png_yaz(os.path.join(cikti, "Kagit.png"), satirlar)
print("Kagit.png yazildi")
