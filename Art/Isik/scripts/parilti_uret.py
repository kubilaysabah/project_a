# parilti_uret.py
# Görev eşyalarının kafa lambasında parlaması için küçük parıltı dokusu (#83, FenerParlamasi).
#
# Ortası yumuşak parlak bir nokta, ondan dört yana incelen ışınlar ve daha kısa, sönük dört
# çapraz ışın: metal bir yüzeyin el fenerinin ışığını yakalaması gibi. Renk beyazdır; asıl rengi
# (lambanın sıcak beyazı) ve gücü oyun verir. Saydamlık (alfa) parlaklığı taşır.
#
# Saf Python (PIL kurulu değil): PNG'yi zlib ile kendimiz yazarız, cizim_uret.py'deki gibi.
# Çalıştırma: python Art/Isik/scripts/parilti_uret.py  ->  Assets/Textures/Parilti.png

import math, os, struct, zlib

N = 128                                            # doku 128 x 128 piksel
KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Textures", "Parilti.png")


def isin(boyuna, enine, uzunluk, kalinlik):
    """Bir ışının bu noktadaki parlaklığı: boyunca doğrusal incelir, enine hızla söner."""
    if abs(boyuna) >= uzunluk:
        return 0.0
    return (1.0 - abs(boyuna) / uzunluk) ** 2 * math.exp(-abs(enine) / kalinlik)


def parlaklik(x, y):
    """(x, y) dokunun ortasına göre -1..1 arası konum; dönen değer 0..1 parlaklık."""
    r = math.hypot(x, y)
    cekirdek = math.exp(-(r * r) / (2 * 0.10 * 0.10))           # ortadaki yumuşak nokta
    hale = 0.35 * math.exp(-(r * r) / (2 * 0.30 * 0.30))         # etrafındaki sönük hale
    duz = isin(x, y, 0.95, 0.018) + isin(y, x, 0.95, 0.018)      # yatay ve dikey ışınlar
    c1, c2 = (x + y) / math.sqrt(2), (x - y) / math.sqrt(2)      # 45° döndürülmüş eksenler
    capraz = 0.45 * (isin(c1, c2, 0.5, 0.014) + isin(c2, c1, 0.5, 0.014))
    kenar = max(0.0, 1.0 - r) ** 0.5                             # köşeler tam saydam kalsın
    return min(1.0, (cekirdek + hale + duz + capraz) * kenar)


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


pikseller = []
for j in range(N):
    for i in range(N):
        x = (i + 0.5) / N * 2 - 1
        y = (j + 0.5) / N * 2 - 1
        a = parlaklik(x, y)
        # Toplamalı karışımda renk * alfa eklenir: rengi beyaz, gücü alfada tutarız.
        pikseller.append((255, 255, 255, int(round(a * 255))))
os.makedirs(os.path.dirname(CIKTI), exist_ok=True)
png_yaz(CIKTI, pikseller)
print("PARILTI", CIKTI)
