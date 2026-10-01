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
# Revizyon (Şahan, 2026-09-26): "sanki ayağına demir kova giymiş gibi". Eski adım bir
# sinüs dalgasıydı; sinüs tek perdede çınlar, kova sesi tam olarak budur. Gerçek bir
# adımda çınlayan hiçbir şey yoktur, üç kısa gürültü vardır:
#   TOPUK: ayakkabının betona ilk değişi. Kısa bir darbe, alçak-orta frekanslarda
#          GENİŞ bantlardan geçer (q küçük: hiçbir perde öne çıkmaz, "tok" duyulur).
#   TABAN: 45-75 ms sonra ayağın önü basar; daha hafif ve biraz daha parlak.
#   SÜRTÜNME: taban betonun tozunu hafifçe sürter; çok kısık bir hışırtı.
# Sonra merdiven boşluğunun kısa bir yankısı eklenir. Dört çeşit, frekansları ve
# zamanlamaları biraz farklı; oyunda perde de her adımda değişiyor (AyakSesi.cs).
def adimlar():
    H = HIZ_KESKIN
    for n in range(1, 2):   # #74: yalnızca Adim_1 (Şahan: mevcut adım sesi kalsın; öbür çeşitler kalktı)
        random.seed(100 + n)
        N = int(0.42 * H)
        k = random.uniform(0.88, 1.12)                       # bu çeşidin "ayakkabı tonu"
        s = [0.0] * N

        topuk = modlar(darbe(N, H, 0.003), H,
                       [(150 * k, 1.1, 1.0), (360 * k, 1.6, 0.75), (820 * k, 1.4, 0.5), (1800 * k, 1.2, 0.4), (3600 * k, 1.0, 0.18)])
        ekle(s, topuk, int(0.004 * H))

        # 2. tur (Şahan: "biraz daha gelişebilir"): betonun üstündeki kum ve toz. Topukla
        # birlikte ezilen minik taneler; adıma gerçek zeminin "cızırtısını" katar.
        for _ in range(random.randint(6, 10)):
            m = random.randint(int(0.0005 * H), int(0.002 * H))
            tane = bant([random.uniform(-1, 1) * (1 - j / m) for j in range(m)] + [0.0] * int(0.004 * H),
                        random.uniform(2500, 6000), H, 2.0)
            ekle(s, tane, int(random.uniform(0.005, 0.06) * H), random.uniform(0.05, 0.15))

        taban = modlar(darbe(N, H, 0.003), H, [(300 * k, 1.4, 0.6), (1150 * k, 1.2, 0.5), (2900 * k, 1.0, 0.3)])
        ekle(s, taban, int(random.uniform(0.045, 0.075) * H), random.uniform(0.3, 0.5))

        hisirti = bant(gurultu(N), 2500 * k, H, 0.8)          # sürtünme
        sure = random.uniform(0.06, 0.1)
        for i in range(N):
            t = i / H - 0.012
            if 0 < t < sure:
                s[i] += 0.06 * hisirti[i] * math.sin(math.pi * t / sure)

        s = alcak(s, 6000, H)                                # çok tiz bir tıkırtı kalmasın
        s = yanki(s, H, boyut=0.45, sonum=0.45, islak=0.16, oda=0.6)   # kısa merdiven yankısı
        s = yumusak_sinirla(s, 4.0)                          # eski adım kadar gür duyulsun
        kaydet_hiz(os.path.join(SESLER, f"Adim_{n}.wav"), s, H, 0.8)


# ---------- ZEMİNE GÖRE AYAK SESİ (#87) ----------
# Seviye hizalama: kaydet_hiz dosyayı EN YÜKSEK NOKTAYA göre ölçekler. Ama kulak sesin
# gövdesini (ortalama gücünü, "RMS") duyar, tepesini değil; halı gibi yumuşak bir ses
# tepesi eşit olsa bile gövdesi bambaşka çıkabilir (ilk denemede iki halı çeşidi
# arasında 4,6 dB fark vardı). O yüzden yeni adımlar Adim_1'in ölçülen gövde seviyesine
# hizalanır: tepe öyle seçilir ki RMS hedefe gelsin, yine de 0,8'i aşmasın.
# RMS: örneklerin karelerinin ortalamasının karekökü; dB = 20·log10(rms).
ADIM_RMS_DB = -24.6          # Adim_1.wav'ın ölçülen gövde seviyesi (dBFS), 2026-10-01


def rms_icin_tepe(ornekler, hedef_db, en_fazla=0.8):
    """Sesi 'hedef_db' RMS'e getirecek tepe değerini döndürür (en fazla 'en_fazla')."""
    rms = math.sqrt(sum(x * x for x in ornekler) / len(ornekler)) or 1e-9
    tepe = max(abs(x) for x in ornekler) or 1.0
    hedef = 10 ** (hedef_db / 20)                            # dB -> doğrusal
    return min(en_fazla, tepe * hedef / rms)                 # tepe, rms'i hedefe taşıyacak kadar ölçeklenir

# Adim_1 beton merdiven için yapıldı; dairenin içinde de aynı ses çalıyordu. Oysa ayak
# sesini belirleyen şey ayakkabı değil, ZEMİNDİR: aynı darbe gürültüsü hangi cisme
# vurursa onun modlarını titreştirir. O yüzden her iki yeni ses de Adim_1'in üç
# katmanlı iskeletini (topuk, taban, sürtünme) olduğu gibi kullanır; yalnızca modlar,
# süzgeç ve oda değişir. Oyun kodu (AyakSesi.cs) zemine göre doğru aileyi seçer.
#
# FAYANS (banyo, mutfak): seramik sert ve incedir; vurulduğunda enerjiyi hemen geri
# verir. Bunun ses karşılığı iki şey:
#   - modlar TİZDİR (400-5500 Hz) ve q'ları BÜYÜKTÜR: her perde dar bir bantta, bir an
#     daha uzun "çın" eder — topuğun fayansa "tık" demesi budur. Betonun geniş, tok
#     bantlarının tam tersi.
#   - darbe daha KISADIR (1,5 ms): sert yüzeyde ayakkabı tabanı ezilmez, anında durur.
# Kum/toz yoktur (fayans silinir); sürtünme nerdeyse sıfır. Oda ise küçük ve fayans
# kaplı bir banyo: yansımalar sık (oda küçük), tizler az sönüyor (sonum küçük) ve
# yankının payı betondakinden büyük (islak) — küçük, parlak, "banyo" yankısı.
def adim_fayans():
    H = HIZ_KESKIN
    for n in range(1, 3):                                    # iki çeşit: Adim_Fayans_1, _2
        random.seed(200 + n)                                 # tohum sabit: her çalıştırma aynı dosya
        N = int(0.35 * H)                                    # 0,35 sn; fayans adımı betondan kısa söner
        k = random.uniform(0.92, 1.08)                       # bu çeşidin ayakkabı/karo tonu
        s = [0.0] * N

        # TOPUK: çok kısa darbe, dar ve tiz bantlar (q 3-5). 1200 Hz en güçlü mod:
        # ince seramik karonun "tık" perdesi orada.
        topuk = modlar(darbe(N, H, 0.0015), H,
                       [(400 * k, 3.0, 0.8), (1200 * k, 4.5, 1.0), (2800 * k, 5.0, 0.7), (5500 * k, 4.0, 0.35)])
        ekle(s, topuk, int(0.004 * H))

        # Nerdeyse hiç kum yok: en fazla iki minik tane, çok kısık (temiz karo yüzeyi).
        for _ in range(random.randint(0, 2)):
            m = random.randint(int(0.0005 * H), int(0.0015 * H))
            tane = bant([random.uniform(-1, 1) * (1 - j / m) for j in range(m)] + [0.0] * int(0.004 * H),
                        random.uniform(4000, 7000), H, 2.5)
            ekle(s, tane, int(random.uniform(0.01, 0.05) * H), random.uniform(0.03, 0.06))

        # TABAN: 40-65 ms sonra ayağın önü basar; topuğa göre daha hafif ama yine çınlayan.
        taban = modlar(darbe(N, H, 0.0015), H, [(600 * k, 3.0, 0.5), (1800 * k, 3.5, 0.5), (4000 * k, 3.0, 0.3)])
        ekle(s, taban, int(random.uniform(0.04, 0.065) * H), random.uniform(0.3, 0.45))

        # SÜRTÜNME: sert, pürüzsüz yüzeyde kayma çok kısa ve çok kısık bir "ssst".
        hisirti = bant(gurultu(N), 3500 * k, H, 1.0)
        sure = random.uniform(0.03, 0.05)
        for i in range(N):
            t = i / H - 0.01
            if 0 < t < sure:
                s[i] += 0.025 * hisirti[i] * math.sin(math.pi * t / sure)

        s = alcak(s, 9000, H)                                # betona göre daha parlak kalsın (6000 -> 9000)
        s = yanki(s, H, boyut=0.35, sonum=0.25, islak=0.3, oda=0.35)   # küçük, fayanslı, parlak oda
        s = yumusak_sinirla(s, 4.0)                          # Adim_1 ile aynı sürüş: gövde aynı oranda şişer
        kaydet_hiz(os.path.join(SESLER, f"Adim_Fayans_{n}.wav"), s, H,
                   rms_icin_tepe(s, ADIM_RMS_DB))            # Adim_1 ile aynı gövde seviyesi


# HALI / KİLİM (90'lar salonu, yatak odası): kalın dokuma darbeyi EMER. Ayakkabı tabanı
# bir anda durmaz, liflerin içine gömülerek yavaşlar; o yüzden:
#   - darbe UZUNDUR (6 ms): yumuşak yüzeyde çarpma yayılır, "tok" yerine "puf" olur.
#   - modlar ALÇAKTIR (90-500 Hz) ve q'ları KÜÇÜKTÜR: hiçbir perde çınlamaz, yalnızca
#     döşemenin altındaki zeminin boğuk gümbürtüsü kalır.
#   - güçlü bir alçak geçiren süzgeç (1800 Hz, iki kez: eğim dikleşir) tizleri siler;
#     halı tam olarak bunu yapar, yüksek frekansları yutar.
# Tek tiz unsur liflerin tabanla sürtünmesi: kumaşın yumuşak "fışş"ı — kum cızırtısı
# değil, daha uzun ve daha alçak bir hışırtı. Oda yankısı neredeyse yok: halı, perde ve
# koltuk dolu bir salon yansıtmaz, sesi emer (kuru). Halı sesi doğası gereği kısıktır;
# dosya Adim_1'den 3 dB daha alçak gövde seviyesine (RMS) hizalanır.
def adim_hali():
    H = HIZ_KESKIN
    for n in range(1, 3):                                    # iki çeşit: Adim_Hali_1, _2
        random.seed(300 + n)                                 # tohum sabit: yeniden üretilebilir
        N = int(0.3 * H)                                     # 0,3 sn; emilen ses çabuk biter
        k = random.uniform(0.9, 1.1)                         # bu çeşidin tonu
        s = [0.0] * N

        # TOPUK: uzun, yayılmış darbe; alçak ve geniş bantlar (q ~1): boğuk gümbürtü.
        topuk = modlar(darbe(N, H, 0.006), H,
                       [(90 * k, 0.9, 1.0), (220 * k, 1.1, 0.7), (500 * k, 1.0, 0.35)])
        ekle(s, topuk, int(0.004 * H))

        # TABAN: 55-85 ms sonra, daha da hafif; halıda topuk-taban farkı silinir.
        taban = modlar(darbe(N, H, 0.006), H, [(120 * k, 1.0, 0.6), (300 * k, 1.0, 0.4)])
        ekle(s, taban, int(random.uniform(0.055, 0.085) * H), random.uniform(0.35, 0.5))

        # KUMAŞ SÜRTÜNMESİ: orta frekanslı (1200 Hz), geniş bantlı, 80-140 ms süren yumuşak
        # hışırtı; zarf sinüs biçimli (yavaş başlar, yavaş biter), kumaş gibi.
        hisirti = bant(gurultu(N), 1200 * k, H, 0.5)
        sure = random.uniform(0.08, 0.14)
        for i in range(N):
            t = i / H - 0.008
            if 0 < t < sure:
                s[i] += 0.09 * hisirti[i] * math.sin(math.pi * t / sure)

        s = alcak(alcak(s, 1800, H), 1800, H)                # iki kez: halının tiz yutması
        s = yanki(s, H, boyut=0.3, sonum=0.6, islak=0.04, oda=0.5)   # neredeyse kuru: döşeli oda
        s = yumusak_sinirla(s, 3.0)                          # daha az sürüş: yumuşak ses sivrilmesin
        kaydet_hiz(os.path.join(SESLER, f"Adim_Hali_{n}.wav"), s, H,
                   rms_icin_tepe(s, ADIM_RMS_DB - 3.0))      # Adim_1'den 3 dB kısık gövde


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
URETICILER = {"adimlar": adimlar, "adim_fayans": adim_fayans, "adim_hali": adim_hali,
              "lamba_sonme": lamba_sonme, "poset": poset, "sabah": sabah}

if __name__ == "__main__":
    # Ad verilirse yalnızca o sesler üretilir:  python oyuncu_sesleri_uret.py poset adimlar
    # Ad verilmezse hepsi. Onaylanmış bir sesi yanlışlıkla bozmamak için tek tek üretmek iyi.
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
