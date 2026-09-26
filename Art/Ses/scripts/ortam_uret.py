# ortam_uret.py
# Normal bir katın ses dokusunu ve anomali bozulmalarının seslerini kodla üretir (#14).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/ortam_uret.py [ses adları]   (ör. ugultu boru)
#
# ÜRETİLEN DOSYALAR (Assets/Audio/Ortam ve Assets/Audio/Anomali):
#   Ortam_Ugultu      binanın sürekli, alçak uğultusu (şebeke vızıltısı + hava)
#   Ortam_Boru        duvarın içinde düzensiz boru tıkırtıları
#   Ortam_Ruzgar      merdiven penceresinden gelen rüzgâr
#   Boru_Damla        sızdıran borudan makine gibi düzenli su damlası (anomali)
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

import math, os, random, struct, sys, wave
import ses_ortak as ortak          # süzgeç, darbe, yankı aletleri (2026-09-26 revizyonu)

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
# Revizyon (Şahan, 2026-09-26): "daha düşük sesli ve ufak ses yankıları olabilir; çok
# gereksiz ve yüksek bir ses". Eski uğultu sürekli, yüksek bir 50 Hz vızıltısıydı.
# Yeni hâli gece binasının "sessizliği":
#   ZEMİN:  çok alçak, kısık bir hava gürültüsü; vızıltının yalnızca izi kaldı.
#   UZAK OLAYLAR: birkaç saniyede bir binanın başka bir yerinden gelen küçük sesler
#           (üst katta kapanan bir kapı, birkaç adım, dışarıdan geçen bir araba, bir
#           kalorifer tıkırtısı). Uzaktan geldikleri için boğukturlar ve neredeyse
#           yalnızca YANKILARI duyulur: sesin kendisi az, merdiven boşluğunun yankısı çok.
# Kayıt 30 sn (eskisi 8 sn): olaylar her turda aynı yerde tekrar etmesin diye uzun.
# Anomali kartı bu katmanı susturunca uzak sesler de kesilir; boşluk daha iyi hissedilir.
def uzak_olay(tur, H):
    """Binanın başka bir yerinden gelen tek bir küçük ses (yankısız; yankı sonra eklenir)."""
    if tur == "kapi":        # uzakta kapanan bir kapı: alçak, boğuk bir güm
        n = int(0.4 * H)
        return ortak.tepeyi_esitle(ortak.alcak(ortak.alcak(ortak.darbe(n, H, 0.015), 160, H), 160, H))
    if tur == "adim":        # üst katta dört yavaş adım
        s = [0.0] * int(2.4 * H)
        for k in range(4):
            tek = ortak.tepeyi_esitle(ortak.alcak(ortak.darbe(int(0.2 * H), H, 0.005), 240, H))
            ortak.ekle(s, tek, int(k * 0.58 * H), 0.35 + 0.08 * (k % 2))
        return s
    if tur == "araba":       # dışarıdan geçen bir araba: kabarıp sönen alçak bir hışırtı
        n = int(4.0 * H)
        g = ortak.tepeyi_esitle(ortak.alcak(ortak.alcak(ortak.gurultu(n), 260, H), 260, H))
        return [0.3 * x * math.sin(math.pi * i / n) ** 2 for i, x in enumerate(g)]
    # "tik": kaloriferin metalik tıkırtısı
    tik = ortak.tepeyi_esitle(ortak.bant(ortak.darbe(int(0.3 * H), H, 0.001), 1400, H, 20))
    return [0.25 * x for x in tik]


def ugultu():
    L = 30.0
    n, gecis = int(L * HIZ), int(0.5 * HIZ)
    random.seed(1)

    # Zemin: çok alçak hava gürültüsü + vızıltının izi. Döngü için fazlası başa karışır.
    hava = ortak.tepeyi_esitle(ortak.alcak(ortak.alcak(ortak.gurultu(n + gecis), 110, HIZ), 110, HIZ))
    zemin = [0.10 * hava[i] + 0.012 * math.sin(2 * math.pi * 50 * i / HIZ) for i in range(n + gecis)]
    zemin = dongule(zemin, n, gecis)

    # Uzak olaylar, 30 sn'ye yayılmış. Olaylar kendi başına kısık; asıl duyulan yankıları.
    olaylar = [0.0] * n
    for bas, tur, guc in ((1.5, "kapi", 0.8), (6.2, "adim", 1.0), (10.8, "araba", 1.0),
                          (16.0, "tik", 1.0), (19.4, "kapi", 0.55), (23.1, "adim", 0.8), (27.2, "tik", 0.8)):
        ortak.ekle(olaylar, uzak_olay(tur, HIZ), int(bas * HIZ), guc)
    olaylar = ortak.alcak(olaylar, 900, HIZ)                         # uzaktan: tizler yolda kaldı
    olaylar = ortak.dairesel_yanki(olaylar, HIZ, 3.0, boyut=0.88, sonum=0.5, islak=1.0, kuru=0.25, oda=1.4)
    olaylar = ortak.tepeyi_esitle(olaylar, 0.6)

    kaydet(ORTAM, "Ortam_Ugultu", [a + b for a, b in zip(zemin, olaylar)], 0.6)


# ---------- TV VE BEBEK ----------
# İlk turda burada formülle üretiliyorlardı (konuşma ve ağlama taklidi). Şahan'ın dinleme
# testinde "TV ile alakası yok", "ağlayan uzaylı gibi" diye geri döndüler; 2026-09-26'dan
# beri gerçek kayıtlardan üretiliyorlar: kayit_isle.py (bebek, tv). Buradan silindiler ki
# bu betik çalıştırılınca gerçek kayıtlardan üretilen dosyaların üstüne yazmasın.


# ---------- BORU ----------
# Revizyon (Şahan, 2026-09-26): "daha yankılı olabilir, borulardan gelen sesler net gelmez".
# Eski tıkırtı sinüs dalgalarıyla yapılmıştı ve fazla netti. Şimdi:
#   - Vuruş bir gürültü darbesi; metal borunun dar modlarından geçiyor (q büyük: kısa
#     ama metalik bir çınlama) ve altında boğuk bir güm var.
#   - Ses duvarın içinden geliyor: 700 Hz üstü güçlü biçimde törpüleniyor (net değil).
#   - Boru ve beton boşluğun uzun yankısı ekleniyor; kuru sesten çok yankı duyuluyor.
BORU_MODLARI = [(610, 70, 1.0), (1020, 80, 0.7), (1580, 70, 0.5), (2350, 60, 0.3)]


def boru_vurusu(guc):
    n = int(0.5 * HIZ)
    metal = ortak.tepeyi_esitle(ortak.modlar(ortak.darbe(n, HIZ, 0.002), HIZ, BORU_MODLARI))
    gum = ortak.tepeyi_esitle(ortak.alcak(ortak.darbe(n, HIZ, 0.006), 150, HIZ))
    return [guc * (0.6 * a + 1.0 * b) for a, b in zip(metal, gum)]


def boru():
    L = 16.0
    n = int(L * HIZ)
    random.seed(4)
    s = [0.0] * n
    t = 0.8
    while t < L - 1.0:
        ortak.ekle(s, boru_vurusu(random.uniform(0.5, 1.0)), int(t * HIZ))
        if random.random() < 0.4:                                   # bazen art arda ikinci tık
            ortak.ekle(s, boru_vurusu(random.uniform(0.3, 0.6)), int((t + random.uniform(0.12, 0.25)) * HIZ))
        t += random.uniform(1.2, 3.4)
    s = ortak.alcak(s, 700, HIZ)                                    # duvarın içinden: boğuk
    s = ortak.dairesel_yanki(s, HIZ, 3.0, boyut=0.88, sonum=0.3, islak=0.7, kuru=0.6, oda=1.2)
    kaydet(ORTAM, "Ortam_Boru", s, 0.7)


# ---------- DÜZENLİ DAMLA (anomali) ----------
# Revizyon (Şahan, 2026-09-26): "su damlama sesi yaparsan daha güzel olabilir". Kart
# (duzenli_boru) hâlâ aynı fikri taşıyor: normal katta boru tıkırtıları DÜZENSİZDİR;
# anomali katında onların yerine sızdıran borudan makine gibi düzenli bir damla gelir:
# hep aynı damla, tam 1,6 sn arayla.
# 2. tur (Şahan): "düşüş sesi güzel, çok yankılı ve çok seri damlıyor" -> aralık 0,8'den
# 1,6 sn'ye çıktı, yankı üçte birine indi.
# Bir su damlasının "plip" sesi aslında suya çarpan damlanın hapsettiği küçük hava
# kabarcığının titreşimidir; kabarcık küçüldükçe perdesi hızla YÜKSELİR. Bu yüzden
# tek bir sinüs ama perdesi 20 ms içinde 850 Hz'den 2150 Hz'e kayıyor. (Burada sinüs
# doğru seçim: kabarcık gerçekten tek perdede öter.)
def damla():
    n = int(0.12 * HIZ)
    s, faz = [], 0.0
    for i in range(n):
        t = i / HIZ
        f = 850 + 1300 * (1 - math.exp(-t / 0.012))                 # yükselen perde
        faz += 2 * math.pi * f / HIZ
        e = (1 - math.exp(-t / 0.0008)) * math.exp(-t / 0.022)     # çok hızlı başlar, çabuk söner
        s.append(math.sin(faz) * e)
    tik = ortak.tepeyi_esitle(ortak.bant(ortak.darbe(n, HIZ, 0.0005), 3000, HIZ, 2.0))   # suya değdiği an
    return [a + 0.3 * b for a, b in zip(s, tik)]


def duzenli_damla():
    L = 16.0                                                        # 16 sn / 1,6 sn = tam 10 damla
    n = int(L * HIZ)
    s = [0.0] * n
    tek = damla()
    t = 0.4
    while t < L:
        ortak.ekle(s, tek, int(t * HIZ))
        t += 1.6
    s = ortak.alcak(s, 5000, HIZ)
    s = ortak.dairesel_yanki(s, HIZ, 2.0, boyut=0.6, sonum=0.45, islak=0.15, oda=0.9)
    kaydet(ANOMALI, "Boru_Damla", s, 0.7)


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


URETICILER = {"ugultu": ugultu, "boru": boru,
              "duzenli_damla": duzenli_damla, "ruzgar": ruzgar, "asansor_ugultu": asansor_ugultu}

if __name__ == "__main__":
    # Ad verilirse yalnızca o sesler üretilir; verilmezse hepsi. Onaylanmış bir sesi
    # yanlışlıkla bozmamak için değişen sesleri tek tek üretmek iyi.
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
