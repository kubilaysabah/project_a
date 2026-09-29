# foto_isle.py
# Seçilen aile fotoğrafı adaylarını eskitip oyuna hazırlar (#69).
#
# Çalıştırma (numpy Blender'ın Python'unda var):
#   blender -b --factory-startup --python Art/Fotograflar/scripts/foto_isle.py
#
# Her fotoğraf için: SECIMLER'deki aday okunur, uzun kenarı 1024'e indirilir, renkler soldurulur
# (film yıllar içinde sararır, gölgeler morarır), grenlenir, köşeleri kararır. Bazılarına ek bozulma:
# tükenmez kalemle karalanmış bir yüz (#69: "yukarı çıktıkça küçük bozulmalar").
# Çıktı: Assets/Textures/Fotograflar/<ad>.png. Adaylar Art/Fotograflar/aday/ altında durur.

import os
import numpy as np
import bpy

BURASI = os.path.dirname(os.path.abspath(__file__))
ADAY = os.path.normpath(os.path.join(BURASI, "..", "aday"))
HEDEF = os.path.normpath(os.path.join(BURASI, "..", "..", "..", "Assets", "Textures", "Fotograflar"))
os.makedirs(HEDEF, exist_ok=True)

# ad: (aday dosyası, sararma 0-1, karalanacak yüzler, gölgelenecek yüzler[, siluetler])
# Siluet: [(x, üst, boy, koyuluk)] (bkz. siluet); model ikinci adamı çizmediyse eklenir.
# Yüzler [(x, y, yarıçap)] oran olarak; y yukarıdan. Karalama: tükenmez kalem. Gölge: yüzü
# yumuşak kenarlı bir karanlık yutar ("dokuzuncu kattaki adam"ın yüzü hiç görünmez, #69).
SECIMLER = {
    "kat1": ("kat1_1974.png", 0.35, [], []),
    "kat2": ("kat2_9.png", 0.45, [], []),
    "kat3": ("kat3_1994.png", 0.30, [], []),
    "kat4": ("kat4_1974.png", 0.55, [], [(0.515, 0.155, 0.075)]),
    "kat5": ("kat5_1974.png", 0.45, [], [], [(0.80, 0.15, 0.38, 0.84)]),
    "kat6": ("kat6_1974.png", 0.40, [], [(0.52, 0.15, 0.072), (0.52, 0.15, 0.05)]),
    "kat7": ("kat7_9.png", 0.45, [(0.285, 0.275, 0.07)], []),
    "kat8": ("kat8_1994.png", 0.30, [], [], [(0.645, 0.19, 0.58, 0.62)]),
    "kapici": ("kapici_1994.png", 0.35, [], []),
}

# Duvar portreleri (#83): aile albümüne girmez, süs olarak duvarda asılır; ayrı klasöre yazılır.
# ad: (aday dosyası, sararma). Siyah-beyaz baskı olduğu için renk solmasından çok sararma alır.
PORTRE_HEDEF = os.path.normpath(os.path.join(BURASI, "..", "..", "..", "Assets", "Textures", "Portreler"))
PORTRELER = {
    "kat2_es": ("kat2_es_1994.png", 0.55),   # Nuri Bey'in rahmetli eşi (2. katın salonu)
}

rng = np.random.default_rng(1994)


def oku(yol):
    img = bpy.data.images.load(yol)
    w, h = img.size
    uzun = max(w, h)
    if uzun > 1024:
        img.scale(int(w * 1024 / uzun), int(h * 1024 / uzun))
        w, h = img.size
    a = np.empty(w * h * 4, dtype=np.float32)
    img.pixels.foreach_get(a)
    bpy.data.images.remove(img)
    return a.reshape(h, w, 4), w, h   # Blender'da satır 0 resmin ALTI


def yaz(p, w, h, yol):
    img = bpy.data.images.new(os.path.basename(yol), w, h, alpha=True)
    img.pixels.foreach_set(np.clip(p, 0, 1).astype(np.float32).ravel())
    img.filepath_raw = yol
    img.file_format = 'PNG'
    img.save()
    bpy.data.images.remove(img)


def karala(p, w, h, x, y, r):
    """(x, y) merkezli, r yarıçaplı bölgeye koyu mavi tükenmez kalemle sık, kıvrık çizgiler çizer."""
    yy, xx = np.mgrid[0:h, 0:w]
    cx, cy, rr = x * w, (1 - y) * h, r * w          # y yukarıdan verildi; Blender'da aşağıdan
    murekkep = np.array([0.05, 0.06, 0.16])
    for _ in range(46):
        a = rng.uniform(0, np.pi)
        ofs = rng.uniform(-rr, rr)
        # Bölgeyi kesen, hafif kıvrık bir çizgi: uzaklık = |doğruya dik uzaklık - dalga|
        nx, ny = np.cos(a), np.sin(a)
        d = (xx - cx) * nx + (yy - cy) * ny - ofs
        boyuna = -(xx - cx) * ny + (yy - cy) * nx
        d = np.abs(d - 3.0 * np.sin(boyuna / rng.uniform(6, 14)))
        icinde = ((xx - cx) ** 2 + (yy - cy) ** 2) < (rr * rng.uniform(0.85, 1.1)) ** 2
        kalem = np.clip(1.6 - d, 0, 1) * icinde * rng.uniform(0.6, 0.95)
        p[..., :3] = p[..., :3] * (1 - kalem[..., None]) + murekkep * kalem[..., None]


def golgele(p, w, h, x, y, r):
    """(x, y) çevresini yumuşak kenarlı bir karanlıkla örter; ortası neredeyse siyah."""
    yy, xx = np.mgrid[0:h, 0:w]
    d = np.hypot(xx - x * w, yy - (1 - y) * h) / (r * w)
    karanlik = np.clip(1.25 - d, 0, 1) ** 0.8 * 0.93
    p[..., :3] *= (1 - karanlik)[..., None]


def siluet(p, w, h, x, ust, boy, koyuluk):
    """Fotoğrafa yüzsüz, uzun bir insan silueti koyar: tepesi (x, ust) oran olarak (ust yukarıdan),
    boyu resmin yüksekliğine oranla. Baş bir daire, gövde omuzdan aşağı daralan bir elips; kenarlar
    yumuşak, koyuluk 1'e yaklaştıkça simsiyah (0,6-0,8: yarı karanlıkta kalan biri)."""
    yy, xx = np.mgrid[0:h, 0:w]
    B = boy * h
    tepe = (1 - ust) * h                        # Blender'da satırlar aşağıdan yukarı
    bx = x * w
    bas_r = B * 0.075
    bas_y = tepe - bas_r
    bas = np.hypot(xx - bx, yy - bas_y) / bas_r
    govde_y = tepe - B * 0.52                   # gövdenin ortası: üstü boyna kadar çıkar
    govde = np.hypot((xx - bx) / (B * 0.17), (yy - govde_y) / (B * 0.40))
    # Omuzlar: başın hemen altında geniş, basık bir elips (gövde yukarıda sivrilmesin).
    omuz = np.hypot((xx - bx) / (B * 0.2), (yy - (tepe - B * 0.24)) / (B * 0.075))
    boyun = np.hypot((xx - bx) / (B * 0.035), (yy - (tepe - B * 0.17)) / (B * 0.05))
    maske = np.maximum.reduce([np.clip(1.15 - bas, 0, 1), np.clip(1.1 - govde, 0, 1),
                               np.clip(1.1 - omuz, 0, 1), np.clip(1.1 - boyun, 0, 1)])
    maske = np.clip(maske * 4.5, 0, 1) * koyuluk
    p[..., :3] = p[..., :3] * (1 - maske[..., None]) + np.array([0.02, 0.02, 0.025]) * maske[..., None]


def eskit(p, w, h, sararma):
    rgb = p[..., :3]
    gri = rgb.mean(axis=2, keepdims=True)
    rgb = rgb * 0.8 + gri * 0.2                                    # renkler biraz söner
    rgb = rgb * (1 - 0.25 * sararma) + np.array([0.95, 0.82, 0.55]) * 0.25 * sararma   # sararma
    rgb = rgb * 0.88 + 0.07                                        # siyahlar açılır, film solar
    rgb[..., 2] += 0.03 * (1 - gri[..., 0])                        # gölgeler hafif morarır
    yy, xx = np.mgrid[0:h, 0:w]
    uz = np.hypot((xx - w / 2) / (w / 2), (yy - h / 2) / (h / 2))
    rgb *= (1 - 0.35 * np.clip(uz - 0.55, 0, 1) ** 1.5)[..., None]  # köşe kararması
    rgb += rng.normal(0, 0.028, (h, w, 1))                         # film greni
    p[..., :3] = rgb
    return p


if __name__ == "__main__":
    for ad, secim in SECIMLER.items():
        dosya, sararma, yuzler, golgeler = secim[:4]
        p, w, h = oku(os.path.join(ADAY, dosya))
        for (x, ust, boy, koyu) in (secim[4] if len(secim) > 4 else []):
            siluet(p, w, h, x, ust, boy, koyu)
        for (x, y, r) in golgeler:
            golgele(p, w, h, x, y, r)
        for (x, y, r) in yuzler:
            karala(p, w, h, x, y, r)
        p = eskit(p, w, h, sararma)
        yaz(p, w, h, os.path.join(HEDEF, ad + ".png"))
        print(f"[FOTO] {ad}: {w}x{h} -> {os.path.join(HEDEF, ad + '.png')}")
    os.makedirs(PORTRE_HEDEF, exist_ok=True)
    for ad, (dosya, sararma) in PORTRELER.items():
        p, w, h = oku(os.path.join(ADAY, dosya))
        p = eskit(p, w, h, sararma)
        yaz(p, w, h, os.path.join(PORTRE_HEDEF, ad + ".png"))
        print(f"[PORTRE] {ad}: {w}x{h} -> {os.path.join(PORTRE_HEDEF, ad + '.png')}")
