# ortam_uret.py
# Normal bir katın ses dokusunu ve anomali bozulmalarının seslerini kodla üretir (#14).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/ortam_uret.py
#
# ÜRETİLEN DOSYALAR (Assets/Audio/Ortam ve Assets/Audio/Anomali):
#   Ortam_Ugultu      binanın sürekli, alçak uğultusu (şebeke vızıltısı + hava)
#   Ortam_TV          bir dairenin içinden, duvar arkasından boğuk TV mırıltısı
#   Ortam_Bebek       başka bir dairede arada bir ağlayan bebek (çoğu zaman sessiz)
#   Ortam_Boru        duvarın içinde düzensiz boru tıkırtıları
#   Ortam_Ruzgar      merdiven penceresinden gelen rüzgâr
#   Bebek_Ters        ağlamanın TERSTEN çalınmışı (anomali)
#   Bebek_Yakin       aynı ağlama ama boğuk değil: duvar arkasından değil, çok yakından (anomali)
#   Boru_Ritim        boru tıkırtısı makine gibi düzenli (anomali)
#   Asansor_Ugultu    arızalı asansör kuyusundan yükselen derin uğultu (anomali)
#
# ---------- DÖNGÜ (LOOP) NASIL KUSURSUZ OLUR ----------
# Ortam sesleri sonsuza kadar tekrar eder. Kaydın sonu ile başı birbirine uymazsa
# her turda bir "tık" ya da ani bir değişim duyulur. İki önlem:
#   1. Titreşimlerin frekansı, kayıt süresinde TAM SAYIDA tur atacak şekilde seçilir
#      (8 saniyelik kayıtta 50 Hz = 400 tam tur): son örnek ilk örneğe kusursuz bağlanır.
#   2. Rastgele gürültü için kayıt biraz uzun üretilir ve fazlası başa yumuşakça
#      karıştırılır (crossfade): sonun sesi, başın sesine erir.
#
# ---------- SÜZGEÇ ----------
# "Alçak geçiren süzgeç" (low-pass) tiz sesleri törpüler, alçak sesleri bırakır.
# Duvarın arkasından gelen ses tam olarak böyle duyulur: duvar tizleri yutar.
# Tek satırlık hali: yeni = önceki + k x (girdi - önceki). k küçüldükçe ses boğuklaşır.

import math, os, random, struct, wave

HIZ = 22050          # saniyedeki örnek: ortam sesleri için yeterli, dosyalar yarı boyutta

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
ORTAM = os.path.join(KOK, "Assets", "Audio", "Ortam")
ANOMALI = os.path.join(KOK, "Assets", "Audio", "Anomali")


# ---------- YARDIMCILAR ----------

def suz(ornekler, k):
    """Alçak geçiren süzgeç. k: 0-1 arası; küçük = daha boğuk."""
    sonuc, onceki = [], 0.0
    for x in ornekler:
        onceki += k * (x - onceki)
        sonuc.append(onceki)
    return sonuc


def dongule(ornekler, uzunluk, gecis):
    """
    'uzunluk + gecis' örneklik bir kaydı 'uzunluk' örnekliğe indirir: fazladan kalan
    son kısım, kaydın başına yumuşakça karıştırılır. Sonuç kusursuz döner.
    """
    sonuc = ornekler[:uzunluk]
    for i in range(gecis):
        a = i / gecis
        sonuc[i] = ornekler[i] * a + ornekler[uzunluk + i] * (1 - a)
    return sonuc


def kaydet(klasor, ad, ornekler, tepe=0.8):
    """En yüksek noktayı 'tepe'ye ölçekleyip 16 bit mono WAV olarak yazar."""
    enbuyuk = max(abs(x) for x in ornekler) or 1.0
    olcek = tepe / enbuyuk
    os.makedirs(klasor, exist_ok=True)
    yol = os.path.join(klasor, ad + ".wav")
    with wave.open(yol, "wb") as dosya:
        dosya.setnchannels(1)          # mono: Unity 3B konumu kendisi hesaplıyor
        dosya.setsampwidth(2)
        dosya.setframerate(HIZ)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in ornekler))
    print("OK", ad, round(len(ornekler) / HIZ, 1), "sn")


def zarf(t, bas, sure, giris=0.05, cikis=0.1):
    """Bir olayın ses şiddeti eğrisi: yumuşak başlar, sürer, yumuşak biter. Dışında 0."""
    if t < bas or t > bas + sure:
        return 0.0
    g = t - bas
    return min(1.0, g / giris, (sure - g) / cikis)


# ---------- UĞULTU ----------
def ugultu():
    L = 8.0
    n, gecis = int(L * HIZ), int(0.5 * HIZ)
    random.seed(1)
    gurultu = suz([random.uniform(-1, 1) for _ in range(n + gecis)], 0.02)   # derin hava sesi
    s = []
    for i in range(n + gecis):
        t = i / HIZ
        # Şebeke vızıltısı: 50 Hz ve katları. 8 sn'de hepsi tam sayıda tur atar.
        v = (0.5 * math.sin(2 * math.pi * 50 * t) + 0.35 * math.sin(2 * math.pi * 100 * t)
             + 0.15 * math.sin(2 * math.pi * 150 * t) + 0.05 * math.sin(2 * math.pi * 300 * t))
        v *= 1 + 0.08 * math.sin(2 * math.pi * 0.25 * t)                  # hafif dalgalanma (8 sn'de 2 tur)
        s.append(0.6 * v + 3.0 * gurultu[i])
    kaydet(ORTAM, "Ortam_Ugultu", dongule(s, n, gecis), 0.7)


# ---------- TV ----------
# Konuşma taklidi: hecelerin ritmiyle (saniyede ~5) açılıp kapanan, perdesi (f0)
# cümle boyunca inip çıkan bir "vızıltı". Her hecede ünlü değişir (farklı üst tonlar
# öne çıkar). Duvarın arkasından geldiği için kelime seçilmez; zaten seçilmemeli.
def tv():
    L = 24.0
    n, gecis = int(L * HIZ), int(0.4 * HIZ)
    random.seed(2)
    toplam = n + gecis

    # Cümleleri ve heceleri önceden planla.
    heceler = []                      # (başlangıç, süre, f0, ünlü)
    t = 0.2
    while t < L + 0.4:
        konusmaci_f0 = random.choice([115, 125, 205, 220])      # erkek / kadın sesi
        cumle = random.uniform(1.4, 3.6)
        son = t + cumle
        while t < son:
            hs = random.uniform(0.11, 0.22)
            heceler.append((t, hs, konusmaci_f0 * random.uniform(0.9, 1.15), random.randint(0, 3)))
            t += hs + random.uniform(0.0, 0.05)
        t += random.uniform(0.3, 1.1)                             # cümle arası boşluk

    # Ünlüler: her biri iki "formant" (öne çıkan frekans bölgesi) ile tanımlı.
    unluler = [(700, 1200), (400, 2000), (300, 900), (500, 1600)]

    s = [0.0] * toplam
    for bas, sure, f0, u in heceler:
        f1, f2 = unluler[u]
        i0, i1 = int(bas * HIZ), min(toplam, int((bas + sure) * HIZ))
        for i in range(i0, i1):
            tt = i / HIZ
            g = tt - bas
            e = math.sin(math.pi * g / sure) ** 2                   # hecenin şiddet eğrisi
            v = 0.0
            for h in range(1, 13):                                  # üst tonlar
                f = f0 * h
                w = math.exp(-((f - f1) / 250) ** 2) + 0.6 * math.exp(-((f - f2) / 400) ** 2)
                v += w * math.sin(2 * math.pi * f * g)
            s[i] += e * v

    # Arka planda arada bir kısık bir müzik akoru (reklam, dizi jeneriği gibi).
    for bas in (6.0, 17.5):
        for i in range(int(bas * HIZ), int((bas + 3.2) * HIZ)):
            tt = i / HIZ
            e = zarf(tt, bas, 3.2, 0.4, 0.8)
            s[i] += 0.5 * e * (math.sin(2 * math.pi * 220 * tt) + math.sin(2 * math.pi * 277 * tt) + math.sin(2 * math.pi * 330 * tt))

    random.seed(22)
    s = [x + 0.08 * random.uniform(-1, 1) for x in s]               # yayın cızırtısı
    s = suz(suz(s, 0.10), 0.18)                                     # duvar arkası: çok boğuk
    kaydet(ORTAM, "Ortam_TV", dongule(s, n, gecis), 0.8)


# ---------- BEBEK ----------
# Ağlama taklidi: yüksek perdeli (400-560 Hz), perdesi her nöbette yükselip alçalan,
# titreyen (vibrato) bir "vaaa". Nöbetler arasında kısa, hırıltılı nefes alışlar.
def aglama_nobeti(sure_toplam, seed):
    """Tek bir ağlama nöbeti (birkaç 'vaaa' + nefesler). Süzgeçsiz, yakın ses."""
    random.seed(seed)
    s = [0.0] * int(sure_toplam * HIZ)
    t = 0.2
    while t < sure_toplam - 1.2:
        vaa = random.uniform(0.9, 1.5)
        f_bas, f_tepe, f_son = random.uniform(400, 450), random.uniform(520, 580), random.uniform(360, 400)
        faz = 0.0
        for i in range(int(t * HIZ), min(len(s), int((t + vaa) * HIZ))):
            g = i / HIZ - t
            p = g / vaa
            f0 = f_bas + (f_tepe - f_bas) * min(1, p * 2.5) - (f_tepe - f_son) * max(0, p - 0.4) / 0.6
            f0 += 14 * math.sin(2 * math.pi * 6 * g)                  # vibrato
            faz += 2 * math.pi * f0 / HIZ
            e = min(1.0, g / 0.08, (vaa - g) / 0.18)
            v = 0.0
            for h in range(1, 9):
                f = f0 * h
                w = math.exp(-((f - 1100) / 450) ** 2) + 0.7 * math.exp(-((f - 2600) / 600) ** 2) + 0.15
                v += w * math.sin(faz * h)
            s[i] += e * v
        t += vaa
        nefes = random.uniform(0.3, 0.5)                               # hırıltılı iç çekiş
        for i in range(int(t * HIZ), min(len(s), int((t + nefes) * HIZ))):
            g = i / HIZ - t
            s[i] += 0.25 * math.sin(math.pi * g / nefes) * random.uniform(-1, 1)
        t += nefes
    return s


def bebek():
    L = 30.0
    n = int(L * HIZ)
    nobet = aglama_nobeti(8.0, 3)
    s = [0.0] * n
    bas = int(4.0 * HIZ)
    for i, x in enumerate(nobet):
        s[bas + i] += x
    s = suz(suz(s, 0.12), 0.25)                                     # başka dairede: boğuk
    kaydet(ORTAM, "Ortam_Bebek", s, 0.8)

    # ANOMALİ 1: aynı nöbet TERSTEN (boğuk). Kaydın başına yakın, oyuncu kata varınca hemen duyulsun.
    ters = [0.0] * int(1.0 * HIZ) + suz(suz(nobet[::-1], 0.12), 0.25) + [0.0] * int(5.0 * HIZ)
    kaydet(ANOMALI, "Bebek_Ters", ters, 0.8)

    # ANOMALİ 2: aynı nöbet boğuk DEĞİL: duvar arkasından değil, sahanlıkta, yakında.
    yakin = [0.0] * int(1.0 * HIZ) + suz(nobet, 0.6) + [0.0] * int(5.0 * HIZ)
    kaydet(ANOMALI, "Bebek_Yakin", yakin, 0.8)


# ---------- BORU ----------
def tik(s, bas, guc):
    """Duvar içinde bir boru tıkırtısı: kısa metalik çın + tok bir vuruş."""
    for i in range(int(bas * HIZ), min(len(s), int((bas + 0.35) * HIZ))):
        g = i / HIZ - bas
        v = (math.sin(2 * math.pi * 900 * g) + 0.6 * math.sin(2 * math.pi * 1370 * g)
             + 0.3 * math.sin(2 * math.pi * 2410 * g)) * math.exp(-g * 28)
        v += 1.2 * math.sin(2 * math.pi * 120 * g) * math.exp(-g * 45)
        s[i] += guc * v


def boru():
    L = 16.0
    n = int(L * HIZ)
    random.seed(4)
    s = [0.0] * n
    t = 0.8
    while t < L - 1.0:
        tik(s, t, random.uniform(0.5, 1.0))
        if random.random() < 0.4:                                   # bazen art arda ikinci tık
            tik(s, t + random.uniform(0.12, 0.25), random.uniform(0.3, 0.6))
        t += random.uniform(1.2, 3.4)
    kaydet(ORTAM, "Ortam_Boru", suz(s, 0.35), 0.7)

    # ANOMALİ: makine gibi düzenli, hep aynı güçte, tam 0,8 sn arayla.
    r = [0.0] * n
    t = 0.4
    while t < L:
        tik(r, t, 0.9)
        t += 0.8
    kaydet(ANOMALI, "Boru_Ritim", suz(r, 0.35), 0.7)


# ---------- RÜZGÂR ----------
def ruzgar():
    L = 12.0
    n, gecis = int(L * HIZ), int(0.8 * HIZ)
    random.seed(5)
    g = suz(suz([random.uniform(-1, 1) for _ in range(n + gecis)], 0.04), 0.08)
    s = []
    for i in range(n + gecis):
        t = i / HIZ
        # Esintiler: 12 sn'de 1, 2 ve 3 tam tur atan yavaş dalgalar.
        e = 0.55 + 0.25 * math.sin(2 * math.pi * t / 12) + 0.15 * math.sin(2 * math.pi * 2 * t / 12 + 1) + 0.08 * math.sin(2 * math.pi * 3 * t / 12 + 2)
        s.append(e * g[i])
    kaydet(ORTAM, "Ortam_Ruzgar", dongule(s, n, gecis), 0.7)


# ---------- ASANSÖR UĞULTUSU ----------
def asansor_ugultu():
    L = 8.0
    n, gecis = int(L * HIZ), int(0.5 * HIZ)
    random.seed(6)
    g = suz([random.uniform(-1, 1) for _ in range(n + gecis)], 0.015)
    s = []
    for i in range(n + gecis):
        t = i / HIZ
        v = math.sin(2 * math.pi * 38 * t) + 0.5 * math.sin(2 * math.pi * 76 * t) + 0.2 * math.sin(2 * math.pi * 57 * t)
        v *= 0.8 + 0.2 * math.sin(2 * math.pi * 0.125 * t)            # yavaş kabarma (8 sn'de 1 tur)
        s.append(v + 4.0 * g[i])
    kaydet(ANOMALI, "Asansor_Ugultu", dongule(s, n, gecis), 0.8)


if __name__ == "__main__":
    ugultu()
    tv()
    bebek()
    boru()
    ruzgar()
    asansor_ugultu()
