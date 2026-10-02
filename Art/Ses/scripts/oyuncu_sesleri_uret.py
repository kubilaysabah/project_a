# oyuncu_sesleri_uret.py
# Oyuncunun hareketlerinden ve oyunun kilit anlarından gelen sesleri kodla üretir (#25).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/oyuncu_sesleri_uret.py [ses adları]
#
# ÜRETİLEN DOSYALAR
#   Assets/Resources/Sesler/Adim_1..4   beton üstünde dört farklı ayak sesi
#   Assets/Resources/Sesler/Lamba_Sonme cezada sönen lambanın "tık"ı
#   Assets/Resources/Sesler/Lamba_Cizirti son lamba titrerken elektrik cızırtısı
#   Assets/Resources/Sesler/Poset_Hisirti poşet toplanırken kâğıt hışırtısı
#   Assets/Resources/Sesler/Kalp_Atisi  gerilimde oyuncunun kendi kalbi (#92)
#   Assets/Audio/Ortam/Ortam_Sabah      çatı kapısı açılınca: uzak şehir, kuşlar, rüzgâr
#
# "Resources" klasöründeki dosyaları oyun kodu adıyla yükleyebiliyor (Resources.Load);
# sahnede hiçbir yere sürüklemek gerekmiyor. Anomali kartları da aynı yolla bulunuyor.

import math, os, random, struct, sys, wave
from ses_ortak import alcak, yuksek, bant, modlar, darbe, gurultu, ekle, yanki, tepeyi_esitle, yumusak_sinirla
from ses_ortak import kaydet as kaydet_hiz

# Kısa, keskin sesler (adım, tık, poşet) 44,1 kHz üretilir: 22 kHz, çok kısa bir darbenin
# tizini yarıya kesip onu "donuk" yapar. Uzun ortam sesleri için 22 kHz yeterli.
HIZ_KESKIN = 44100

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
# #101 (2026-10-01): oyuncunun adımları artık GERÇEK KAYIT (sturmankin, CC0; kayit_isle.py adimlar).
# Parke (Adim_1), fayans ve halı adımlarının sentez üreticileri bu yüzden silindi: burada kalsalar,
# bu betik yeniden çalıştırılınca kayıttan kesilen adımların üstüne sentez yazardı.

# ---------- KALP ATIŞI (#92) ----------
# Gerilim yükselince oyuncunun kendi kalbi duyulur (GerilimEtkisi.cs). Bu ses odada çalan bir
# şey değil, göğüste HİSSEDİLEN bir şey; o yüzden kuru (yankısız) ve çok alçak.
#
# Gerçek bir kalp atışı iki kapak sesinden oluşur: "LUB" (kalbin kasılıp kapakların kapanması,
# daha uzun ve alçak) ve hemen ardından "DUB" (atardamar kapaklarının kapanması, daha kısa,
# daha hafif). Steteskopla dinlenen ses de, kulağında kendi nabzını duyan birinin duyduğu da
# 40-150 Hz arasında boğuk iki gümlemedir; hiçbir perde ÇINLAMAZ.
#
# Sinüs YOK (ses_ortak.py'nin kuralı: sinüs "bip" ya da "kova" gibi duyulur). Her gümleme:
#   1. GÜRÜLTÜ PATLAMASI: darbe() gibi ama yumuşak başlayan bir zarfla (8-10 ms yükselir,
#      sonra söner). Ani başlayan bir patlama tıkırtı yapardı; kalp "tık" demez, "güm" der.
#   2. GÖVDE MODLARI: göğüs kafesinin ve kalbin kendisinin alçak frekansları (45-90 Hz güçlü,
#      140-220 Hz hafif). q küçük (1-2): geniş bant, hiçbir perde öne çıkmaz, boğuk duyulur.
#      Hafif üst modlar, küçük hoparlörde (dizüstü) 40 Hz hiç çalınamadığı için var: onlar
#      sayesinde gümleme kulaklıksız da seçilir.
#   3. GÜÇLÜ ALÇAK GEÇİREN (400 Hz, iki kez): et ve kemik tizi tamamen yutar.
# Sonra 25 Hz altı kesilir (hoparlörü boşuna zorlayan, duyulmayan titreşim ve sıfır kayması).
# Yankı neredeyse yok: çok küçük, çok kısık bir iz (göğüs boşluğunun kendi tınısı).
def kalp_atisi():
    H = HIZ_KESKIN
    random.seed(400)                                         # tohum sabit: her çalıştırma aynı dosya
    N = int(0.45 * H)                                        # toplam 0,45 sn (oyunda en hızlı aralık da 0,45)
    s = [0.0] * N

    def gumleme(uzunluk, yukselme, modlistesi):
        """Yumuşak başlayan bir gürültü patlamasını gövde modlarından geçirir."""
        m = int(uzunluk * H)                                 # patlamanın örnek sayısı
        y = max(1, int(yukselme * H))                        # zarfın yükselme süresi (örnek)
        patlama = [0.0] * N
        for i in range(m):
            zarf = min(1.0, i / y) * (1 - i / m) ** 2        # yavaş başla, hızla sön
            patlama[i] = random.uniform(-1, 1) * zarf
        return modlar(patlama, H, modlistesi)

    # LUB: daha uzun (70 ms patlama), daha alçak, daha güçlü.
    lub = gumleme(0.07, 0.010, [(45, 1.2, 1.0), (62, 1.5, 0.9), (90, 1.3, 0.55), (150, 1.0, 0.18)])
    ekle(s, lub, int(0.005 * H), 1.0)

    # DUB: 0,13 sn sonra; daha kısa (50 ms), bir tık daha tiz, daha hafif.
    dub = gumleme(0.05, 0.008, [(55, 1.2, 0.8), (80, 1.6, 1.0), (120, 1.3, 0.5), (220, 1.0, 0.15)])
    ekle(s, dub, int((0.005 + 0.13) * H), 0.62)

    s = alcak(alcak(s, 400, H), 400, H)                      # iki kez: eğim dikleşir, tiz tamamen gider
    s = yuksek(s, 25, H)                                     # duyulmayan alt titreşimi ve sıfır kaymasını at
    s = yanki(s, H, boyut=0.2, sonum=0.8, islak=0.03, oda=0.25)   # neredeyse kuru: göğüste, odada değil
    s = yumusak_sinirla(s, 2.0)                              # tepeyi biraz bastır, gövde dolgunlaşsın
    kaydet_hiz(os.path.join(SESLER, "Kalp_Atisi.wav"), s, H, 0.8)


# ---------- LAMBA VINLAMASI (#105) ----------
# Gerilim yükselince kafa lambasından gelen ince elektrik vınlaması (diyegetik gerilim göstergesi:
# ekranda yazı yok, oyuncu lambasından anlar). Düzgün döngü için her bileşen 3 saniyeye TAM sığan
# frekanslardan: 100 Hz'in katları (300 tam periyot) ve 1 Hz'lik dalgalanma (3 tam periyot). Böylece
# dosyanın sonu başına dikişsiz bağlanır. İnce bir 7,8 kHz ıslık (eski transformatörlerin "cıyaklaması")
# ve bant geçiren gürültü üstüne; gürültü döngüde takırdamasın diye kenarları 20 ms'de kısılır.
def lamba_vinlama():
    H = HIZ_KESKIN
    random.seed(500)
    sure = 3.0
    N = int(sure * H)
    s = [0.0] * N
    for i in range(N):
        t = i / H
        dalga = 0.85 + 0.15 * math.sin(2 * math.pi * 1.0 * t)                 # yavaş nefes (1 Hz, 3 tam periyot)
        vinlama = (math.sin(2 * math.pi * 100 * t) * 0.5 + math.sin(2 * math.pi * 200 * t) * 0.3
                   + math.sin(2 * math.pi * 300 * t) * 0.15 + math.sin(2 * math.pi * 400 * t) * 0.08)
        islik = math.sin(2 * math.pi * 7800 * t) * 0.06                         # 7800 = 100 × 78: tam periyot
        s[i] = (vinlama + islik) * dalga
    cizirti = bant(gurultu(N), 2400, H, 1.5)
    kenar = int(0.02 * H)
    for i in range(N):
        zarf = min(1.0, i / kenar, (N - 1 - i) / kenar)                         # döngü dikişi için kenarlar kısık
        s[i] += 0.18 * cizirti[i] * zarf
    s = alcak(s, 9000, H)
    kaydet_hiz(os.path.join(SESLER, "Lamba_Vinlama.wav"), s, H, 0.6)


# ---------- LAMBA SÖNMESİ ----------
# Eski bir duvar anahtarının "tık"ı: iki kısa darbe, 11 ms arayla (önce tuşun basılması,
# sonra mekanizmanın oturması; ikincisi daha güçlü). Her darbe sert plastik gövdenin
# modlarından geçer.
# Revizyon geçmişi (Şahan):
#   1. tur: "daha tok, lamba anahtarı gibi" -> tiz röle çınlaması çıkarıldı.
#   2. tur: "çok fazla ve yüksek, bir şeyi bir yere vuruyormuşsun gibi" -> 120-240 Hz'lik
#      alçak gövde modları çıkarıldı (o "vurma" hissini veren oydu), yumuşak sınırlayıcı
#      ve yankının çoğu kaldırıldı, dosya daha kısık kaydediliyor. Küçük, kuru bir tık.
def lamba_sonme():
    H = HIZ_KESKIN
    random.seed(7)
    N = int(0.4 * H)
    s = [0.0] * N
    for gecikme, guc in ((0.0, 0.45), (0.011, 1.0)):
        tik = modlar(darbe(N, H, 0.001), H,
                     [(650, 3.0, 0.3), (1400, 5.0, 0.8), (2600, 6.0, 0.6), (4200, 5.0, 0.3)])
        ekle(s, tik, int((0.005 + gecikme) * H), guc)
    s = alcak(yuksek(s, 300, H), 8000, H)
    s = yanki(s, H, boyut=0.5, sonum=0.5, islak=0.08, oda=0.7)      # yalnızca hafif bir oda izi
    kaydet_hiz(os.path.join(SESLER, "Lamba_Sonme.wav"), s, H, 0.7)


# ---------- KÂĞIT POŞET ----------
# Revizyon (Şahan): "poşetten ziyade elektrik hışırtısı gibi; kâğıt poşet sesi yap".
# Eski ses 220 minik, çok tiz çıtırtıydı; o kadar küçük ve tiz tıklar kulağa elektrik
# gibi gelir. Kâğıt başka türlü ses çıkarır:
#   ÇITIRTI: daha az (70), daha uzun (2-12 ms) ve daha alçak (0,9-4,5 kHz) kırışmalar;
#            her biri kendi frekans bandından geçer, bu yüzden hepsi biraz farklı.
#   HIŞIRTI: kâğıdın elde kayması; orta frekanslarda sürekli, dalgalanan bir sürtünme.
# Poşet kavranınca en yoğun, bırakılırken seyrek.
def poset():
    H = HIZ_KESKIN
    random.seed(9)
    L = 0.7
    N = int(L * H)
    s = [0.0] * N

    hisirti = bant(gurultu(N), 1800, H, 0.6)
    for i in range(N):
        t = i / H
        e = min(1.0, t / 0.06) * max(0.0, 1 - t / 0.62)
        e *= 0.55 + 0.45 * math.sin(2 * math.pi * 6 * t) ** 2          # el hareketiyle dalgalanır
        s[i] += 0.14 * hisirti[i] * e

    for _ in range(70):
        t0 = random.betavariate(1.5, 2.6) * (L - 0.12)
        m = random.randint(int(0.002 * H), int(0.012 * H))
        kirisma = [random.uniform(-1, 1) * (1 - j / m) ** 1.5 for j in range(m)] + [0.0] * int(0.01 * H)
        kirisma = bant(kirisma, random.uniform(900, 4500), H, random.uniform(1.0, 3.0))
        ekle(s, kirisma, int(t0 * H), random.uniform(0.3, 1.0))

    s = alcak(yuksek(s, 250, H), 9000, H)
    s = yanki(s, H, boyut=0.5, sonum=0.5, islak=0.1, oda=0.7)
    s = yumusak_sinirla(s, 1.5)
    kaydet_hiz(os.path.join(SESLER, "Poset_Hisirti.wav"), s, H, 0.7)


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


# ---------- GÜVERCİNLER ----------
# İlk turda burada formülle üretiliyordu; Şahan'ın dinleme testinde "güvercinle hiç
# alakası yok" dendi. 2026-09-26'dan beri gerçek kayıttan: kayit_isle.py -> guvercin.


# #74: lamba cızırtısı kalktı; adım üreticisi yalnızca Adim_1'i üretir (öbür çeşitler kalktı).
# #87: zemine göre iki yeni adım ailesi (fayans, halı).
# #92: gerilimde duyulan kalp atışı.
URETICILER = {"kalp_atisi": kalp_atisi, "lamba_vinlama": lamba_vinlama,
              "lamba_sonme": lamba_sonme, "poset": poset, "sabah": sabah}

if __name__ == "__main__":
    # Ad verilirse yalnızca o sesler üretilir:  python oyuncu_sesleri_uret.py poset adimlar
    # Ad verilmezse hepsi. Onaylanmış bir sesi yanlışlıkla bozmamak için tek tek üretmek iyi.
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
