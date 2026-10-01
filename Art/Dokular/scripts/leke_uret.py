# leke_uret.py
# Duvar lekeleri için saydam arka planlı (RGBA) dokular üretir (#87). Bu dokular Unity'de URP
# "Decal Projector" (çıkartma yansıtıcı) ile duvar kâğıdının ÜSTÜNE basılır: dokunun alfası
# (saydamlık kanalı) lekenin nerede ne kadar görüneceğini, RGB'si ise rengini verir.
# İndirme yok, lisans sorusu yok: her piksel bu betikte hesaplanır (kir_uret.py ile aynı yöntem).
#
# Çalıştırma (numpy Blender'ın Python'unda var; PIL yüklü değil, bu yüzden PNG'yi Blender yazar):
#   blender -b --factory-startup --python Art/Dokular/scripts/leke_uret.py
#
# ÜRETİLENLER (Assets/Textures/Lekeler/, hepsi 1024 x 1024 RGBA)
#   Leke_Su.png       : su / rutubet lekesi; kenarında koyu "gelgit" halkaları olan düzensiz yayılma.
#   Leke_Rutubet.png  : tavan küfü; gri-yeşil-siyah benekli bulut, ortası sık, dışı seyrek.
#   Leke_Catlak.png   : sıva çatlağı; dallanan tek bir koyu çizgi ve ince açık renk "dökülmüş" kenar.
#   Leke_Kabarma.png  : kabarmış/dökülmüş boya; altından çıkan sıva, kenarda gölge, birkaç pul.
#   Leke_Is.png       : is / el kiri; bir yandan başlayıp sönen yumuşak koyu bulaşık (düşük alfa).
#   Leke_Sizinti.png  : üst kenardan inen 3-7 ince kahverengi akıntı; aşağı doğru solar.
#
# ORTAK KURAL: her lekenin alfası doku kenarına varmadan 0'a iner (kenar_sondur), yoksa çıkartmanın
# dikdörtgen sınırı duvarda keskin bir çizgi olarak görünür.

import os
import numpy as np
import bpy

BURASI = os.path.dirname(os.path.abspath(__file__))
# Çıktı klasörü: repo kökü / Assets / Textures / Lekeler (yoksa oluşturulur).
HEDEF = os.path.normpath(os.path.join(BURASI, "..", "..", "..", "Assets", "Textures", "Lekeler"))
os.makedirs(HEDEF, exist_ok=True)
rng = np.random.default_rng(1987)   # sabit tohum: betik her çalıştığında aynı lekeler çıksın

N = 1024                            # her dokunun kenar uzunluğu (piksel)
# Piksel koordinatları: x ve y -0,5 .. +0,5 arasında, (0, 0) dokunun ortası.
# Satır 0 dokunun ÜSTÜ (kaydet() diziyi ters çevirir, çünkü Blender pikselleri alttan yukarı sayar).
y, x = np.mgrid[0:N, 0:N] / (N - 1) - 0.5


# ---------- YARDIMCILAR ----------
def kaydet(dizi, ad):
    """(h, w, 4) RGBA diziyi PNG olarak yazar. RGB sRGB renk olarak kaydedilir."""
    h, w = dizi.shape[:2]
    img = bpy.data.images.new(ad, w, h, alpha=True)
    img.colorspace_settings.name = 'sRGB'       # renkli doku: Unity'de "sRGB" işaretli kalsın
    img.alpha_mode = 'STRAIGHT'                 # alfa renge çarpılmaz; saydam yerde renk korunur
    # Blender piksel dizisini ALT satırdan başlayarak okur; bizim satır 0'ımız üst olduğu için
    # kaydetmeden önce dikeyde ters çeviriyoruz (flipud = "flip up-down").
    duz = np.flipud(np.clip(dizi, 0, 1)).astype(np.float32).ravel()
    img.pixels.foreach_set(duz)
    img.filepath_raw = os.path.join(HEDEF, ad + ".png")
    img.file_format = 'PNG'
    img.save()
    print("[LEKE]", img.filepath_raw)


def gurultu(n, hucre):
    """Döşenebilir değer gürültüsü: hucre x hucre rastgele ızgara, yumuşak aradeğerle büyütülür.
    (kir_uret.py'deki ile aynı.) Izgara kenarlarda başa sardığı için dikiş görünmez."""
    g = rng.random((hucre, hucre))
    t = np.linspace(0, hucre, n, endpoint=False)
    i0 = np.floor(t).astype(int); f = t - i0
    f = f * f * (3 - 2 * f)                      # yumuşak adım
    i1 = (i0 + 1) % hucre
    a = g[i0][:, i0]; b = g[i0][:, i1]; c = g[i1][:, i0]; d = g[i1][:, i1]
    fx = f[None, :]; fy = f[:, None]
    return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy


def fbm(n, katman=6, baslangic=4):
    """Birkaç ölçekteki gürültünün toplamı (büyük lekeler + ince kumlanma). Sonuç ~0..1."""
    toplam = np.zeros((n, n)); agirlik = 0; w = 1.0
    for k in range(katman):
        toplam += w * gurultu(n, baslangic * 2 ** k); agirlik += w; w *= 0.55
    return toplam / agirlik


def yumusak(t):
    """0'ın altını 0, 1'in üstünü 1 yapar; arada S biçimli yumuşak geçiş (sert kenar yerine)."""
    t = np.clip(t, 0, 1)
    return t * t * (3 - 2 * t)


def karistir(a, b, t):
    """İki değeri (ya da iki rengi) t oranında karıştırır: t=0 → a, t=1 → b."""
    return a * (1 - t) + b * t


def renk(r, g, b):
    """Tek bir rengi (N, N, 3) boyutunda, her piksel aynı renk olan diziye çevirir."""
    return np.dstack([np.full((N, N), r), np.full((N, N), g), np.full((N, N), b)])


def bulaniklastir(dizi, sigma):
    """Gauss bulanıklığı (sigma piksel). Fourier dönüşümüyle yapılır: büyük görüntüde döngüden
    kat kat hızlıdır. Kenarlar başa sarar; lekeler kenara varmadan söndüğü için sorun olmaz."""
    f = np.fft.fftfreq(dizi.shape[0])            # her frekans için katsayı
    g = np.exp(-2 * (np.pi * sigma * f) ** 2)    # Gauss'un Fourier karşılığı da bir Gauss'tur
    return np.real(np.fft.ifft2(np.fft.fft2(dizi) * g[:, None] * g[None, :]))


def kenar_sondur(alfa, kenar=0.10, ust=None):
    """Alfayı doku kenarına doğru 0'a indirir. 'kenar' sönme bandının genişliği (0,10 = kenarın
    %10'u); 'ust' verilirse üst kenar için ayrı (daha dar) bir bant kullanılır (akıntı dokusu)."""
    if ust is None:
        ust = kenar
    sol_sag = yumusak((0.5 - np.abs(x)) / kenar)     # sol ve sağ kenara uzaklık
    alt = yumusak((0.5 - y) / kenar)                 # alt kenara uzaklık
    ust_b = yumusak((y + 0.5) / ust)                 # üst kenara uzaklık
    return alfa * sol_sag * alt * ust_b


def birlestir(rgb, alfa):
    """(N, N, 3) renk + (N, N) alfa → (N, N, 4) RGBA."""
    return np.dstack([rgb, alfa])


def egri_1b(uzunluk, dugum, genlik):
    """1 boyutlu yumuşak rastgele eğri: 'dugum' adet rastgele düğüm, aralar doğrusal aradeğer,
    sonra hafif bulanıklık. Akıntının sağa sola kıvrılması ve çatlak kalınlığının değişmesi için."""
    dugumler = rng.uniform(-1, 1, dugum)
    konum = np.linspace(0, uzunluk - 1, dugum)
    egri = np.interp(np.arange(uzunluk), konum, dugumler)
    cekirdek = np.ones(9) / 9                     # 9'luk kutu ortalaması: köşeleri yuvarlar
    egri = np.convolve(egri, cekirdek, mode='same')
    return egri * genlik


# ---------- 1) SU LEKESİ ----------
# Ana biçim: ortadan uzaklık (hafif yayvan elips) + fbm bozulması. Böylece daire değil, düzensiz
# bir yayılma çıkar. "Gelgit" halkaları aynı d alanının farklı seviye çizgileridir: su kururken
# kenarda biriken kir her seferinde biraz daha içte bir halka bırakır.
r = np.hypot(x, y * 0.85)
d = r + 0.22 * (fbm(N, 5, 3) - 0.5) + 0.03 * (fbm(N, 3, 24) - 0.5)
R = 0.30
ic = yumusak((R - d) / 0.03)                                  # lekenin içi (yumuşak kenarlı)
halkalar = np.zeros((N, N))
for Rk, guc in [(R, 1.0), (R - 0.045, 0.6), (R - 0.10, 0.4), (R - 0.165, 0.25)]:
    halkalar += guc * np.exp(-((d - Rk) / 0.007) ** 2)        # her halka ~7 piksel genişlikte
halkalar *= 0.55 + 0.45 * fbm(N, 3, 12)                       # halkalar yer yer silikleşir
halkalar = np.clip(halkalar, 0, 1)
ic_alfa = ic * (0.20 + 0.14 * fbm(N, 4, 6))                   # iç kısım hafif, bulutlu
alfa = np.clip(ic_alfa + halkalar * 0.72, 0, 0.85)            # alfa en güçlü halkalarda
t = np.clip(d / R, 0, 1) ** 1.5                               # merkez 0 → kenar 1
rgb = karistir(renk(0.55, 0.48, 0.38), renk(0.35, 0.29, 0.19), t[..., None])   # ortası açık
rgb = karistir(rgb, renk(0.27, 0.21, 0.12), (halkalar * 0.75)[..., None])      # halkalar koyu
kaydet(birlestir(rgb, kenar_sondur(alfa)), "Leke_Su")

# ---------- 2) TAVAN RUTUBETİ / KÜF ----------
# Ortası yoğun bir "bulut" (fbm × merkez ağırlığı) ve üstüne ince benekler. Benek eşiği dışa doğru
# yükselir: merkezde çok benek, kenarda tek tük.
r = np.hypot(x * 1.05, y * 1.2)
yogunluk = np.exp(-(r / 0.22) ** 2)                            # merkezde 1, dışa doğru söner
bulut = fbm(N, 6, 3)
bulut_a = yumusak((bulut - 0.50 + 0.38 * yogunluk) * 2.2) * yogunluk ** 0.5 * 0.5
benek = fbm(N, 2, 128)                                        # ince taneli gürültü (~8 piksellik benekler)
esik = 0.58 + 0.34 * (1 - yogunluk)                           # dışa doğru eşik yükselir → seyrekleşir
benek_a = yumusak((benek - esik) * 9) * yumusak((0.46 - r) / 0.06)
orta_benek = yumusak((fbm(N, 3, 40) - (0.62 + 0.3 * (1 - yogunluk))) * 8)   # orta boy benekler
koloni = yumusak((fbm(N, 2, 24) - 0.70) * 10) * yogunluk ** 0.8   # merkezde birkaç iri küf kolonisi
alfa = np.clip(bulut_a + benek_a * 0.85 + orta_benek * 0.7 + koloni * 0.6, 0, 0.92)
rgb = renk(0.20, 0.22, 0.17)                                  # gri-yeşil bulut
koyu = np.clip(benek_a + orta_benek + koloni, 0, 1)
rgb = karistir(rgb, renk(0.06, 0.08, 0.06), koyu[..., None])  # benekler kara
kaydet(birlestir(rgb, kenar_sondur(alfa)), "Leke_Rutubet")

# ---------- 3) SIVA ÇATLAĞI ----------
# Çatlak bir "rastgele yürüyüş"tür: soldan başlar, her adımda yönü biraz sapar, ara sıra sert bir
# kırılma yapar, yine de genel olarak sağa ilerler. Yürüyüşün noktaları bir maskeye damgalanır
# (değer = o noktadaki kalınlık), maske bulanıklaştırılıp eşiklenince kalınlığı değişen pürüzsüz
# bir çizgi çıkar: ağır damga → geniş çizgi, hafif damga → ince çizgi.
maske = np.zeros((N, N))


def yuru(bas_x, bas_y, aci, uzunluk, kalinlik, hedef_aci=None, sapma=0.16):
    """Bir çatlak kolu yürütür ve maskeye damgalar. Dönüş: yürüyüş noktaları (dallanma için)."""
    noktalar = []
    px, py = bas_x, bas_y
    egri = egri_1b(uzunluk, max(4, uzunluk // 60), 0.45)   # kalınlık boyunca yavaş dalgalanma
    for i in range(uzunluk):
        aci += rng.normal(0, sapma)                            # küçük sapma
        if hedef_aci is not None:
            aci += (hedef_aci - aci) * 0.03                    # genel yöne geri çekilme
        if rng.random() < 0.015:                               # ara sıra keskin kırılma
            aci += rng.choice([-1, 1]) * rng.uniform(0.4, 0.9)
        px += np.cos(aci) * 0.7; py += np.sin(aci) * 0.7       # 0,7 piksellik adım (boşluk kalmaz)
        if not (0.08 * N < px < 0.92 * N and 0.08 * N < py < 0.92 * N):
            break                                              # kenara yaklaştı: dur
        uc = min(i / (0.12 * uzunluk), (uzunluk - i) / (0.18 * uzunluk), 1)   # uçlarda sivrilir
        w = kalinlik * (0.75 + egri[i]) * uc
        noktalar.append((px, py, aci))
        maske[int(py), int(px)] = max(maske[int(py), int(px)], w)
    return noktalar


bas_y = N * rng.uniform(0.38, 0.62)
ana = yuru(0.12 * N, bas_y, rng.uniform(-0.3, 0.3), int(1.5 * N), 1.0, hedef_aci=rng.uniform(-0.25, 0.25))
# Dallar: ana çatlağın rastgele noktalarından, 0,5-1,1 radyan sapan, daha ince ve kısa kollar.
for _ in range(rng.integers(3, 6)):
    px, py, aci = ana[rng.integers(len(ana) // 6, len(ana) * 5 // 6)]
    yon = aci + rng.choice([-1, 1]) * rng.uniform(0.5, 1.1)
    dal = yuru(px, py, yon, int(rng.uniform(70, 260)), 0.55, hedef_aci=yon, sapma=0.2)
    if dal and rng.random() < 0.5:                             # bazen dalın da dalı olur
        px2, py2, aci2 = dal[len(dal) // 2]
        yuru(px2, py2, aci2 + rng.choice([-1, 1]) * rng.uniform(0.5, 1.0), int(rng.uniform(30, 90)), 0.35)

ince = bulaniklastir(maske, 1.4)                               # dar bulanıklık: çizginin gövdesi
cekirdek = yumusak((ince - 0.055) / 0.05)                      # 2-6 piksellik koyu çizgi
oz = yumusak((ince - 0.13) / 0.06)                             # çizginin en kalın yerinde daha da koyu öz
hale = np.clip(bulaniklastir(maske, 5.0) / 0.075, 0, 1) * 0.32   # çevresinde soluk açık "dökülmüş sıva"
alfa = np.maximum(cekirdek * 0.95, hale)
rgb = renk(0.74, 0.71, 0.65)                                  # hale: açık sıva tozu
rgb = karistir(rgb, renk(0.12, 0.10, 0.08), cekirdek[..., None])   # çizgi koyu
rgb = karistir(rgb, renk(0.04, 0.035, 0.03), oz[..., None])        # öz neredeyse siyah
kaydet(birlestir(rgb, kenar_sondur(alfa, 0.07)), "Leke_Catlak")

# ---------- 4) KABARAN / DÖKÜLEN BOYA ----------
# Üst boya tabakasının gittiği, altından çıplak sıvanın göründüğü düzensiz bir yama. Kenar pürüzlü
# (iki ölçekte fbm); hemen içinde ince bir gölge çizgisi (boya tabakasının kalınlığı gölge yapar),
# hemen dışında açık renkli ince bir "dudak" (kalkmış boyanın kenarı). Kenar yakınında birkaç
# kıvrılmış pul.
r = np.hypot(x * 1.1, y * 0.9)
d = r + 0.20 * (fbm(N, 5, 3) - 0.5) + 0.05 * (fbm(N, 3, 16) - 0.5)
R = 0.27
ic = yumusak((R - d) / 0.004)                                 # ~4 piksel yumuşaklıkta keskin kenar
siva_ton = 0.92 + 0.16 * fbm(N, 4, 8)                         # sıvanın hafif alacası
siva = renk(0.62, 0.58, 0.52) * siva_ton[..., None]
siva = karistir(siva, renk(0.55, 0.54, 0.52), (yumusak((fbm(N, 3, 12) - 0.55) * 4) * 0.5)[..., None])  # grimsi bölgeler
golge = np.exp(-((d - (R - 0.006)) / 0.005) ** 2) * ic        # iç kenar boyunca ince gölge bandı
golge *= 0.45 + 0.55 * fbm(N, 3, 10)                          # gölge yer yer zayıflar
dudak = np.exp(-((d - (R + 0.006)) / 0.0045) ** 2) * (1 - ic) # dış kenar: kalkmış boyanın açık kenarı
rgb = karistir(siva, renk(0.17, 0.14, 0.11), (golge * 0.85)[..., None])
rgb = karistir(rgb, renk(0.82, 0.80, 0.76), np.clip(dudak, 0, 1)[..., None])
alfa = np.maximum(ic * 0.96, np.clip(dudak, 0, 1) * 0.55)
# Pullar: kenara yakın piksellerden rastgele seçilen merkezlerde küçük, döndürülmüş, pürüzlü elipsler.
kenar_pikseller = np.argwhere(np.abs(d - R) < 0.03)
puruz = fbm(N, 3, 64) - 0.5
for _ in range(rng.integers(7, 11)):
    cy, cx = kenar_pikseller[rng.integers(len(kenar_pikseller))]
    px, py = x[cy, cx], y[cy, cx]
    a = rng.uniform(0, np.pi); bx, by = rng.uniform(0.012, 0.030), rng.uniform(0.007, 0.016)
    u = (x - px) * np.cos(a) + (y - py) * np.sin(a)             # elipsin kendi eksenlerine çevir
    v = -(x - px) * np.sin(a) + (y - py) * np.cos(a)
    pul_d = np.hypot(u / bx, v / by) + puruz * 1.1                # pürüz: elips değil, yırtık pul
    pul = yumusak((1 - pul_d) / 0.06)
    # Pulun 2-3 piksel sağ altına küçük bir gölge: pul duvardan hafif kalkmış gibi dursun.
    kay = 3
    pul_golge = np.zeros((N, N)); pul_golge[kay:, kay:] = pul[:-kay, :-kay]
    pul_golge = np.clip(pul_golge - pul, 0, 1) * 0.6
    rgb = karistir(rgb, renk(0.15, 0.13, 0.11), pul_golge[..., None])
    alfa = np.maximum(alfa, pul_golge)
    rgb = karistir(rgb, renk(0.80, 0.77, 0.72) * (0.9 + 0.2 * rng.random()), pul[..., None])
    alfa = np.maximum(alfa, pul * 0.95)
kaydet(birlestir(rgb, kenar_sondur(alfa)), "Leke_Kabarma")

# ---------- 5) İS / EL KİRİ ----------
# Kaynak alt kenara yakın (y = +0,30): orada en koyu, yukarı doğru uzun bir kuyrukla söner
# (radyatör üstündeki is böyle yükselir). Çıkartma döndürülerek priz ve anahtar çevresine de konur.
dy = y - 0.30
sigma_y = np.where(dy < 0, 0.34, 0.11)                        # yukarı uzun, aşağı kısa kuyruk
g = np.exp(-((x / 0.20) ** 2 + (dy / sigma_y) ** 2))
alfa = g * (0.70 + 0.30 * fbm(N, 5, 4)) * (0.85 + 0.15 * fbm(N, 3, 24))   # bulutlu ve ince taneli
alfa = np.clip(alfa * 0.72, 0, 0.60)                          # en koyu yerde bile ≤ 0,60
rgb = renk(0.06, 0.055, 0.05)                                 # neredeyse siyah
kaydet(birlestir(rgb, kenar_sondur(alfa)), "Leke_Is")

# ---------- 6) SIZINTI AKINTILARI ----------
# Üst kenardan inen 3-7 ince akıntı. Her akıntının kendi x konumu, hafif kıvrımı, kalınlığı ve
# uzunluğu var; alfa aşağı doğru söner ve ucunda küçük bir damla kabarcığı biter. Akıntının
# kenarları ortasından biraz daha koyu (kurumuş suyun kiri kenarda birikir).
sayi = int(rng.integers(3, 8))
konumlar = np.sort(rng.uniform(-0.36, 0.36, sayi))
konumlar += np.linspace(-0.02, 0.02, sayi)                    # iki akıntı üst üste binmesin
px_sutun = (x[0] + 0.5) * (N - 1)                             # her sütunun piksel x'i
py_satir = (y[:, 0] + 0.5) * (N - 1)                          # her satırın piksel y'si
alfa = np.zeros((N, N)); kenar_koyu = np.zeros((N, N))
for k in range(sayi):
    uzunluk = rng.uniform(0.30, 0.85)                         # yüksekliğin bu kadarı kadar iner
    w = rng.uniform(2.5, 7.0)                                 # kalınlık (piksel)
    xc = (konumlar[k] + 0.5) * (N - 1) + egri_1b(N, 14, 2.0) + egri_1b(N, 5, 6.0)   # kıvrılan merkez
    dist = np.abs(px_sutun[None, :] - xc[:, None])            # her pikselin akıntı merkezine uzaklığı
    profil = yumusak((w / 2 + 1.0 - dist) / 1.6)              # yumuşak kenarlı şerit
    t = (py_satir / (N - 1)) / uzunluk                        # 0 üstte, 1 akıntının ucunda
    sonme = np.clip(1 - t, 0, 1) ** 1.3                       # aşağı doğru solma
    sonme *= 0.75 + 0.25 * np.clip(egri_1b(N, 30, 1.0) + 0.5, 0, 1)   # boyunca hafif leke leke
    serit = profil * sonme[:, None]
    # Damla: akıntının bittiği yerde küçük bir elips.
    uc_y = uzunluk * (N - 1); uc_x = xc[min(int(uc_y), N - 1)]
    damla = yumusak((1 - np.hypot((px_sutun[None, :] - uc_x) / (w * 1.1), (py_satir[:, None] - uc_y) / (w * 1.7))) / 0.25)
    serit = np.maximum(serit, damla * 0.7)
    alfa = np.maximum(alfa, serit * 0.78)
    kenar_koyu = np.maximum(kenar_koyu, serit * yumusak((dist - w / 2 + 1.5) / 1.5))   # kenar bandı
rgb = karistir(renk(0.43, 0.34, 0.23), renk(0.29, 0.22, 0.13), np.clip(kenar_koyu, 0, 1)[..., None])
kaydet(birlestir(rgb, kenar_sondur(alfa, kenar=0.08, ust=0.03)), "Leke_Sizinti")


# ---------- DOĞRULAMA ----------
# Yazılan PNG'ler diskten yeniden yüklenir ve kontrol edilir: boyut 1024 x 1024 mi, 4 kanal (RGBA)
# var mı, kenar satır/sütunlarında alfa 1'e varıyor mu (varmamalı), saydam pikselde renk korunmuş mu.
print("[LEKE] --- doğrulama ---")
for ad in ["Leke_Su", "Leke_Rutubet", "Leke_Catlak", "Leke_Kabarma", "Leke_Is", "Leke_Sizinti"]:
    yol = os.path.join(HEDEF, ad + ".png")
    img = bpy.data.images.load(yol, check_existing=False)
    w, h = img.size
    piks = np.empty(w * h * img.channels, dtype=np.float32)
    img.pixels.foreach_get(piks)
    piks = piks.reshape(h, w, img.channels)
    a = piks[..., 3] if img.channels == 4 else None
    if a is None:
        print(f"[LEKE] {ad}: HATA, alfa kanalı yok ({img.channels} kanal)")
        continue
    kenar_maks = max(a[0].max(), a[-1].max(), a[:, 0].max(), a[:, -1].max())
    saydam = np.argwhere(a < 0.002)
    ornek = piks[saydam[0][0], saydam[0][1], :3] if len(saydam) else None
    durum = "TAMAM" if (w == 1024 and h == 1024 and kenar_maks < 1.0) else "HATA"
    print(f"[LEKE] {ad}: {w}x{h}, {img.channels} kanal, alfa en çok {a.max():.3f}, "
          f"kenarda en çok {kenar_maks:.4f}, saydam piksel sayısı {len(saydam)}, "
          f"saydam yerde RGB {None if ornek is None else np.round(ornek, 3)} -> {durum}")
