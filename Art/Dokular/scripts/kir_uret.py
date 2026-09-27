# kir_uret.py
# Terk edilmiş dairelerin kiri ve tozu için dokular üretir (#78). İndirme yok, lisans sorusu yok:
# her piksel bu betikte hesaplanır.
#
# Çalıştırma (numpy Blender'ın Python'unda var):
#   blender -b --factory-startup --python Art/Dokular/scripts/kir_uret.py
#
# ÜRETİLENLER (Assets/Textures/Kir/)
#   Kir_Detay.png    : döşenebilir (kenarları birbirine uyan) gri "grunge". Unity URP Lit'in DETAY
#                      haritası olarak kullanılır: 0,5 gri = değişiklik yok, koyusu kirletir, açığı
#                      toz gibi soldurur (URP "x2 çarp" kuralı: renk × doku × 2).
#   Orumcek_Agi.png  : köşeye gerilmiş örümcek ağı; saydam arka plan (alfa kesme malzemesi için).
#   Toz_Zerresi.png  : yumuşak kenarlı küçük bir nokta; havadaki toz parçacıklarının resmi.

import os
import numpy as np
import bpy

BURASI = os.path.dirname(os.path.abspath(__file__))
HEDEF = os.path.normpath(os.path.join(BURASI, "..", "..", "..", "Assets", "Textures", "Kir"))
os.makedirs(HEDEF, exist_ok=True)
rng = np.random.default_rng(1994)   # sabit tohum: betik her çalıştığında aynı doku çıksın


def kaydet(dizi, ad, renkli=False):
    """(h, w) gri ya da (h, w, 4) diziyi PNG olarak yazar."""
    if dizi.ndim == 2:
        dizi = np.dstack([dizi, dizi, dizi, np.ones_like(dizi)])
    h, w = dizi.shape[:2]
    img = bpy.data.images.new(ad, w, h, alpha=True)
    img.colorspace_settings.name = 'sRGB' if renkli else 'Non-Color'
    img.pixels.foreach_set(np.clip(dizi, 0, 1).astype(np.float32).ravel())
    img.filepath_raw = os.path.join(HEDEF, ad + ".png")
    img.file_format = 'PNG'
    img.save()
    print("[KIR]", img.filepath_raw)


def gurultu(n, hucre):
    """Döşenebilir değer gürültüsü: hucre x hucre rastgele ızgara, yumuşak aradeğerle büyütülür.
    Izgara kenarlarda başa sardığı (np.roll ile) için doku yan yana dizilince dikiş görünmez."""
    g = rng.random((hucre, hucre))
    t = np.linspace(0, hucre, n, endpoint=False)
    i0 = np.floor(t).astype(int); f = t - i0
    f = f * f * (3 - 2 * f)                      # yumuşak adım
    i1 = (i0 + 1) % hucre
    a = g[i0][:, i0]; b = g[i0][:, i1]; c = g[i1][:, i0]; d = g[i1][:, i1]
    fx = f[None, :]; fy = f[:, None]
    return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy


def fbm(n, katman=6, baslangic=4):
    """Birkaç ölçekteki gürültünün toplamı (büyük lekeler + ince kumlanma)."""
    toplam = np.zeros((n, n)); agirlik = 0; w = 1.0
    for k in range(katman):
        toplam += w * gurultu(n, baslangic * 2 ** k); agirlik += w; w *= 0.55
    return toplam / agirlik


# ---------- KİR DETAYI ----------
N = 1024
leke = fbm(N, 6, 4)                                  # büyük, yumuşak kararmalar
leke = np.clip((leke - 0.35) * 2.2, 0, 1)
kum = fbm(N, 4, 64)                                  # ince kumlanma
damla = fbm(N, 3, 16)
damla = np.clip((damla - 0.62) * 6, 0, 1)            # seyrek, keskin kenarlı lekeler (su izi)
toz = np.clip((fbm(N, 5, 8) - 0.55) * 3, 0, 1)       # açık, soluk toz bölgeleri
# 0,5 = nötr. Kararmalar ve lekeler aşağı, toz yukarı iter.
detay = 0.5 - 0.16 * leke - 0.10 * damla + 0.06 * toz + 0.05 * (kum - 0.5)
kaydet(detay, "Kir_Detay")

# ---------- ÖRÜMCEK AĞI ----------
# Köşe (0, 0) sol altta; ağ çeyrek daire içinde. Işınlar köşeden çıkar, sarmal halkalar
# ışınlar arasında hafifçe sarkar. Alfa: çizgilerde opak, gerisi saydam.
M = 512
y, x = np.mgrid[0:M, 0:M] / (M - 1)
r = np.hypot(x, y); aci = np.arctan2(y, x)            # 0 .. pi/2
alfa = np.zeros((M, M))
isinlar = np.sort(rng.uniform(0.05, np.pi / 2 - 0.05, 9))
for a in isinlar:
    d = np.abs(np.sin(aci - a)) * r * M              # ışına piksel uzaklığı
    alfa = np.maximum(alfa, np.clip(3.2 - d, 0, 1) * (r < 0.95))   # ışınlar ~6 piksel
def parca_uzakligi(px, py, x1, y1, x2, y2):
    """Her pikselin (px, py) bir doğru parçasına [(x1, y1) - (x2, y2)] uzaklığı."""
    dx, dy = x2 - x1, y2 - y1
    t = np.clip(((px - x1) * dx + (py - y1) * dy) / (dx * dx + dy * dy + 1e-9), 0, 1)
    return np.hypot(px - (x1 + t * dx), py - (y1 + t * dy))


# Halkalar: her iki komşu ışının arasına gerilmiş, ortası köşeye doğru biraz sarkan iki parça.
for halka in np.linspace(0.12, 0.88, 11):
    for a1, a2 in zip(isinlar[:-1], isinlar[1:]):
        x1, y1 = halka * np.cos(a1), halka * np.sin(a1)
        x2, y2 = halka * np.cos(a2), halka * np.sin(a2)
        am = (a1 + a2) / 2; rm = halka * np.cos((a2 - a1) / 2) * 0.96   # kiriş ortası, sarkık
        xm, ym = rm * np.cos(am), rm * np.sin(am)
        d = np.minimum(parca_uzakligi(x, y, x1, y1, xm, ym), parca_uzakligi(x, y, xm, ym, x2, y2)) * M
        alfa = np.maximum(alfa, np.clip(2.4 - d, 0, 1) * 0.85)   # halkalar ~4 piksel
alfa *= np.clip(1 - r, 0, 1) ** 0.3 * (0.6 + 0.4 * fbm(M, 3, 8))   # dışa doğru incelir, yer yer kopuk
ag = np.dstack([np.full((M, M), 0.82), np.full((M, M), 0.82), np.full((M, M), 0.8), alfa])
kaydet(ag, "Orumcek_Agi", renkli=True)

# ---------- TOZ ZERRESİ ----------
K = 64
y, x = np.mgrid[0:K, 0:K] / (K - 1) - 0.5
yumusak = np.clip(1 - np.hypot(x, y) * 2, 0, 1) ** 2
kaydet(np.dstack([np.ones((K, K)), np.ones((K, K)), np.ones((K, K)), yumusak]), "Toz_Zerresi", renkli=True)
