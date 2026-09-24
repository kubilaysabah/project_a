# oyuncu_sesleri_uret.py
# Oyuncunun hareketlerinden ve oyunun kilit anlarından gelen sesleri kodla üretir (#25).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/oyuncu_sesleri_uret.py
#
# ÜRETİLEN DOSYALAR
#   Assets/Resources/Sesler/Adim_1..4   beton üstünde dört farklı ayak sesi
#   Assets/Resources/Sesler/Lamba_Sonme cezada sönen lambanın "tık"ı
#   Assets/Resources/Sesler/Lamba_Cizirti son lamba titrerken elektrik cızırtısı
#   Assets/Resources/Sesler/Poset_Hisirti poşet toplanırken naylon hışırtısı
#   Assets/Audio/Ortam/Ortam_Sabah      çatı kapısı açılınca: uzak şehir, kuşlar, rüzgâr
#
# "Resources" klasöründeki dosyaları oyun kodu adıyla yükleyebiliyor (Resources.Load);
# sahnede hiçbir yere sürüklemek gerekmiyor. Anomali kartları da aynı yolla bulunuyor.

import math, os, random, struct, wave

HIZ = 22050

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")
ORTAM = os.path.join(KOK, "Assets", "Audio", "Ortam")


def suz(ornekler, k):
    """Alçak geçiren süzgeç: tizleri törpüler. k küçüldükçe ses boğuklaşır."""
    sonuc, onceki = [], 0.0
    for x in ornekler:
        onceki += k * (x - onceki)
        sonuc.append(onceki)
    return sonuc


def tiz(ornekler, k):
    """Yüksek geçiren süzgeç: alçak sesi çıkarır, yalnızca tizi bırakır (ham - boğuk)."""
    b = suz(ornekler, k)
    return [x - y for x, y in zip(ornekler, b)]


def kaydet(klasor, ad, ornekler, tepe=0.8):
    enbuyuk = max(abs(x) for x in ornekler) or 1.0
    olcek = tepe / enbuyuk
    os.makedirs(klasor, exist_ok=True)
    with wave.open(os.path.join(klasor, ad + ".wav"), "wb") as dosya:
        dosya.setnchannels(1)
        dosya.setsampwidth(2)
        dosya.setframerate(HIZ)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in ornekler))
    print("OK", ad, round(len(ornekler) / HIZ, 2), "sn")


# ---------- AYAK SESİ ----------
# Bir adım: topuğun betona tok vuruşu (alçak, çok kısa) + tabanın sürtünmesi
# (kısa, hışırtılı bir gürültü). Dört çeşidin her biri biraz farklı perde ve
# sürtünmeyle üretiliyor; hep aynı ses tekrar etse kulak makineyi fark ederdi.
def adimlar():
    for n in range(1, 5):
        random.seed(100 + n)
        s = [0.0] * int(0.28 * HIZ)
        topuk_f = random.uniform(85, 120)
        for i in range(len(s)):
            t = i / HIZ
            s[i] += math.sin(2 * math.pi * topuk_f * t) * math.exp(-t * 55)          # tok vuruş
        gurultu = tiz([random.uniform(-1, 1) for _ in range(len(s))], 0.15)
        bas = random.uniform(0.012, 0.03)
        for i in range(len(s)):
            t = i / HIZ
            if t > bas:
                g = t - bas
                s[i] += 0.55 * gurultu[i] * math.exp(-g * random.uniform(35, 50))    # sürtünme
        kaydet(SESLER, f"Adim_{n}", suz(s, 0.5), 0.8)


# ---------- LAMBA SÖNMESİ ----------
# Eski bir lambanın elektriği kesilince: rölenin keskin "tık"ı, ardından ampul
# telinin soğurken çıkardığı çok ince, kısa bir çınlama.
def lamba_sonme():
    random.seed(7)
    s = [0.0] * int(0.6 * HIZ)
    for i in range(len(s)):
        t = i / HIZ
        s[i] += random.uniform(-1, 1) * math.exp(-t * 900)                            # tık
        s[i] += 0.5 * math.sin(2 * math.pi * 2900 * t) * math.exp(-t * 60)           # röle yayı
        s[i] += 0.12 * math.sin(2 * math.pi * 6100 * t) * math.exp(-t * 9)           # tel çınlaması
        s[i] += 0.4 * math.sin(2 * math.pi * 140 * t) * math.exp(-t * 80)            # gövde
    kaydet(SESLER, "Lamba_Sonme", s, 0.75)


# ---------- CIZIRTI ----------
# Titreyen lambanın elektrik vızıltısı: 50 Hz şebeke vızıltısı ve üst tonları,
# üstünde düzensiz çıtırtılar (kıvılcım).
def lamba_cizirti():
    random.seed(8)
    L = 0.9
    s = [0.0] * int(L * HIZ)
    for i in range(len(s)):
        t = i / HIZ
        v = sum(math.sin(2 * math.pi * 50 * h * t) / h for h in (1, 2, 3, 4, 6, 8))  # vızıltı
        e = min(1.0, t / 0.03, (L - t) / 0.1)
        s[i] = 0.5 * e * v
    for _ in range(40):                                                              # çıtırtılar
        bas = random.randint(0, len(s) - 400)
        guc = random.uniform(0.4, 1.2)
        for j in range(400):
            s[bas + j] += guc * random.uniform(-1, 1) * math.exp(-j / 40)
    kaydet(SESLER, "Lamba_Cizirti", suz(s, 0.7), 0.7)


# ---------- NAYLON HIŞIRTISI ----------
# Poşet kavranınca naylon buruşur: çok sayıda minik, tiz çıtırtı, yarım saniyeye yayılmış.
def poset():
    random.seed(9)
    L = 0.55
    s = [0.0] * int(L * HIZ)
    for _ in range(220):
        t0 = random.betavariate(1.6, 2.8) * (L - 0.02)          # çıtırtılar başta yoğun, sona doğru seyrek
        bas = int(t0 * HIZ)
        guc = random.uniform(0.2, 1.0)
        uz = random.randint(20, 90)
        for j in range(uz):
            if bas + j < len(s):
                s[bas + j] += guc * random.uniform(-1, 1) * (1 - j / uz)
    kaydet(SESLER, "Poset_Hisirti", tiz(s, 0.25), 0.7)


# ---------- SABAH ----------
# Çatı kapısı açılınca: uzaktan şehrin alçak uğultusu, hafif rüzgâr ve kuşlar.
# Kuş cıvıltısı: tiz (2,5-5 kHz), çok kısa, perdesi hızla kayan notalar; ikişer üçer.
# Döngü kusursuz olsun diye rastgele kısım biraz uzun üretilip başa karıştırılıyor.
def sabah():
    L = 20.0
    n, gecis = int(L * HIZ), int(0.8 * HIZ)
    random.seed(10)
    sehir = suz(suz([random.uniform(-1, 1) for _ in range(n + gecis)], 0.02), 0.05)
    ruzgar = suz([random.uniform(-1, 1) for _ in range(n + gecis)], 0.06)
    s = [4.0 * sehir[i] + 0.6 * ruzgar[i] * (0.6 + 0.4 * math.sin(2 * math.pi * i / HIZ / 10))
         for i in range(n + gecis)]
    t = 0.5
    while t < L - 1.0:                                           # kuşlar
        f0 = random.uniform(2500, 4200)
        for k in range(random.randint(2, 4)):
            bas = t + k * random.uniform(0.09, 0.16)
            sure = random.uniform(0.05, 0.12)
            kayma = random.uniform(-1500, 1500)
            faz = 0.0
            for i in range(int(bas * HIZ), int((bas + sure) * HIZ)):
                g = i / HIZ - bas
                f = f0 + kayma * (g / sure) + 150 * math.sin(2 * math.pi * 30 * g)
                faz += 2 * math.pi * f / HIZ
                s[i] += 0.35 * math.sin(math.pi * g / sure) * math.sin(faz)
        t += random.uniform(0.6, 2.2)
    sonuc = s[:n]
    for i in range(gecis):
        a = i / gecis
        sonuc[i] = s[i] * a + s[n + i] * (1 - a)
    kaydet(ORTAM, "Ortam_Sabah", sonuc, 0.7)


if __name__ == "__main__":
    adimlar()
    lamba_sonme()
    lamba_cizirti()
    poset()
    sabah()
