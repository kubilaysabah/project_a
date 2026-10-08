# ayak_izi_uret.py
# Islak ayak izi çıkartma dokuları (#137, madde 7): Assets/Textures/Lekeler/Leke_AyakIzi_Sol.png ve _Sag.png.
#
# NEDEN İKİ DOKU: URP'nin DecalProjector'ı dokuyu aynalayamaz; sol ve sağ ayak ayrı dokudur (sağ, solun
# yatay aynası). Çıplak bir ayak izi: topuk, taban, ayak tarağı ve beş parmak; başparmak İÇ tarafta.
#
# Saf Python (PIL yok, numpy yok): her piksel burada hesaplanır, PNG'yi zlib ile biz yazarız
# (cizim_uret.py / kagit_uret.py yöntemi). İndirme yok, lisans sorusu yok.
#
# Çalıştırma:  python Art/Dokular/scripts/ayak_izi_uret.py
#
# Doku 256 x 256 RGBA; ayak yukarı bakar (parmaklar üstte): decal projektörün y ekseni yürüme yönüne
# çevrilince iz o yöne bakar. Alfa ortada 0,6, kenarlarda yumuşakça 0'a iner (ıslak iz keskin değildir);
# renk koyu mavi-gri: ıslak zemin karanlıkta koyu görünür, fenerde hafif parlar (Normal_Blend 0).
import os, zlib, struct, math, random

BURASI = os.path.dirname(os.path.abspath(__file__))
HEDEF = os.path.normpath(os.path.join(BURASI, "..", "..", "..", "Assets", "Textures", "Lekeler"))
os.makedirs(HEDEF, exist_ok=True)
N = 256
random.seed(1994)


def elips(x, y, cx, cy, rx, ry):
    """Noktanın elipse göre 'uzaklığı': 1'in altı içeride, 1 kenar, üstü dışarıda."""
    return math.hypot((x - cx) / rx, (y - cy) / ry)


def yumusak(d, kenar=0.22):
    """Elips uzaklığından kapsama: içeride 1, kenarda 1 → 0'a yumuşak geçiş."""
    if d <= 1.0 - kenar: return 1.0
    if d >= 1.0: return 0.0
    t = (1.0 - d) / kenar
    return t * t * (3 - 2 * t)


def ayak(x, y):
    """Sol ayağın kapsaması (0-1): parçaların en büyüğü. Koordinatlar piksel, y yukarıdan."""
    k = 0.0
    k = max(k, yumusak(elips(x, y, 128, 196, 33, 38)))          # topuk
    k = max(k, yumusak(elips(x, y, 124, 148, 29, 50)))          # taban ortası (iç tarafa hafif kayık: kavis)
    k = max(k, yumusak(elips(x, y, 130, 96, 44, 36)))           # ayak tarağı
    # Parmaklar: sol ayak, başparmak SAĞDA (iç taraf). (x, y, yarıçap)
    for px, py, r in ((166, 54, 15), (146, 42, 10), (128, 40, 9), (110, 44, 8), (93, 52, 8)):
        k = max(k, yumusak(elips(x, y, px, py, r, r * 1.15), 0.35))
    return k


def png_yaz(yol, satirlar):
    """satirlar: her satır RGBA bayt dizisi. PNG: imza + IHDR + IDAT (zlib) + IEND."""
    def parca(ad, veri):
        return struct.pack(">I", len(veri)) + ad + veri + struct.pack(">I", zlib.crc32(ad + veri) & 0xffffffff)
    ham = b"".join(b"\x00" + s for s in satirlar)
    with open(yol, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(parca(b"IHDR", struct.pack(">IIBBBBB", N, N, 8, 6, 0, 0, 0)))
        f.write(parca(b"IDAT", zlib.compress(ham, 9)))
        f.write(parca(b"IEND", b""))


def uret(ad, ayna):
    satirlar = []
    for y in range(N):
        s = bytearray()
        for x in range(N):
            xx = (N - 1 - x) if ayna else x
            k = ayak(xx + 0.5, y + 0.5)
            # Islaklık dokusu: ortada daha koyu, rastgele küçük kuru benekler (iz tam dolu değildir).
            benek = 1.0 if random.random() > 0.08 else 0.55
            alfa = k * 0.62 * benek
            # Renk: koyu mavi-gri (ıslak); kenara doğru biraz açılır (ince su).
            r, g, b = 22, 28, 36
            s += bytes((r, g, b, int(round(alfa * 255))))
        satirlar.append(bytes(s))
    yol = os.path.join(HEDEF, ad)
    png_yaz(yol, satirlar)
    print("yazildi", yol)


uret("Leke_AyakIzi_Sol.png", ayna=False)
uret("Leke_AyakIzi_Sag.png", ayna=True)
