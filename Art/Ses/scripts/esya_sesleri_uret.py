# esya_sesleri_uret.py
# Dairenin içindeki eşyaların yakın plan seslerini kodla üretir: çekmece, dolap kapağı, buzdolabı,
# saç dolap, kavanoz, ağır gardırop, eşya alma sesleri, küçük kilit, bakalit düğme, kapı gıcırtısı döngüsü.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/esya_sesleri_uret.py [ses adları]
#              (ad verilmezse hepsi; ör. "python esya_sesleri_uret.py cekmece kapak")
#
# ÜRETİLEN DOSYALAR (Assets/Resources/Sesler/, oyun kodu adıyla yükler)
#   Cekmece_Acilma      tahta çekmece tahta kızaklarda çekilir: sürtünme, sonda tık, içindekilerin şıngırtısı
#   Kapak_Acilma        küçük tahta dolap kapağı / sandık kapağı: kasadan kurtulma tıkı, küçük menteşe gıcırtısı
#   Buzdolabi_Acilma    80'lerin buzdolabı: lastik contanın "töp"ü, kapı rafındaki şişelerin şıngırtısı
#   Metal_Kapak_Acilma  saç dolap / büro dolabı kapağı: mandal tıkı, ince metal gıcırtı, sacın boğuk titremesi
#   Kavanoz_Acilma      cam kavanozun metal kapağı: döndürürken cızırtılı sürtünme, sonra küçük "pıt"
#   Agir_Surtunme       ağır tahta gardırop parkede itilir (ya da zorlanır ama kımıldamaz): alçak hırıltı
#   Cekmece_Kilitli     kilitli çekmece çekiştirilir: kilide çarpan iki tahta tıkırtısı
#   Cam_Tikirti         cam kavanoza dokunulur: iki parlak cam şıngırtısı
#   Al_Metal            metal el aleti (tornavida, levye) alınır: tahtada küçük sürtünme + kısa metal tınlaması
#   Al_Kagit            kâğıt ya da fotoğraf alınır: kuru, düzensiz kırışma
#   Al_Seramik          seramik tabak kaldırılır/konur: tek seramik "çın", ardından hafif ikinci değme
#   Al_Kucuk            küçük sigorta / porselen parça alınır: minik tık + kısa şıngırtı
#   Kilit_Kucuk         oda kapısında ya da asma kilitte küçük anahtar döner: iki net mekanik tık
#   Dugme_Bakalit       90'ların bakalit lamba anahtarı / asansör çağırma düğmesi: tek sert plastik "tak"
#   Kucuk_Tikirti       sessiz odada küçük bir şeyin yerine oturması: tek yumuşak tık (ipucu, kısık)
#   Kapi_Gicirti_Dongu  kapı sürüklenirken çalan, dikişsiz dönen eski menteşe gıcırtısı (sesi kod açıp kısar)
# ve Assets/Audio/Anlar/ altına:
#   An_Surtunme         tahta ayaklı manken yerde döner: tahta-tahta burulma sürtünmesi ve gıcırtı
#
# KURAL (ses_ortak.py'nin başı, Şahan'ın dinleme testi): saf sinüs YOK. Her ses gürültüden doğar:
#   1. bir sürtünme ya da çarpma GÜRÜLTÜSÜ (bütün frekanslar birden),
#   2. cismin kendi frekansları (MODLAR) bu gürültüden yalnızca kendi perdelerini çınlatır,
#   3. küçük bir oda yankısı (bunlar yakın plan sesler; oda küçük, yankı kısık).
# Cismi tanıtan şey modlardır: tahta ALÇAK perdeli ve çabuk söner (q küçük), metal ve cam TİZ ve uzun
# çınlar (q büyük), seramik ikisinin arasındadır. Gıcırtı ise "yapış-kay"dır: iki yüzey takılır, kurtulur,
# yine takılır; her kurtuluş bir tıktır ve tıkların ARALIĞI kulağa perde gibi gelir (sinüs değil).
# Her ses kendi tohumuyla (random.seed) üretilir: betik yeniden çalışınca aynı dosya çıkar.

import math, os, random, struct, sys, wave
from ses_ortak import alcak, yuksek, bant, modlar, darbe, gurultu, ekle, yanki, dairesel_yanki, yumusak_sinirla, tepeyi_esitle, kaydet

HIZ = 44100   # kısa, keskin sesler: 22 kHz tıkların ve cam şıngırtısının tizini keserdi

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")
ANLAR = os.path.join(KOK, "Assets", "Audio", "Anlar")


# ---------- SEVİYE ----------
# kaydet() dosyayı EN YÜKSEK NOKTAYA göre ölçekler, ama kulak sesin gövdesini (RMS: örneklerin
# karelerinin ortalamasının karekökü) duyar. Üstelik bütün dosyanın RMS'i sesin UZUNLUĞUNA bağlıdır:
# 2,8 sn'lik bir kapı sesiyle 0,15 sn'lik bir düğmenin dosya RMS'leri karşılaştırılamaz (uzun sessiz
# kuyruk ortalamayı düşürür). O yüzden "sesin en gür 100 ms'si"nin RMS'i ölçülür; kulağın "bu ses ne
# kadar gür" diye duyduğu şey odur. Örnek seslerin ölçülen değerleri (2026-10-01, wave modülüyle):
#   IcKapi_Acilma (oda kapısı)  en gür 100 ms: -16,6 dBFS   (dosya RMS -20,6, tepe -1,9)
#   Anahtar_Al    (anahtar alma) en gür 100 ms: -17,9 dBFS   (dosya RMS -28,0, tepe -1,9)
#   Adim_1        (ayak sesi)    en gür 100 ms: -18,4 dBFS   (dosya RMS -24,6, tepe -1,9)
ESYA_DB = -16.6           # eşya sesleri (çekmece, kapak, dolap...) IcKapi_Acilma kadar
ALMA_DB = -17.9           # alma sesleri Anahtar_Al kadar
TIKIRTI_DB = ALMA_DB - 6  # Kucuk_Tikirti bir ipucu: alma seslerinden 6 dB kısık


def en_gur_rms(ornekler, pencere=0.1):
    """Sesin en gür 'pencere' saniyesinin RMS'i (doğrusal). Ses pencereden kısaysa bütün ses."""
    w = min(len(ornekler), max(1, int(pencere * HIZ)))
    adim = max(1, w // 10)                                   # pencereyi 10 ms'de bir kaydır
    enbuyuk = 0.0
    for b in range(0, len(ornekler) - w + 1, adim):
        e = sum(x * x for x in ornekler[b:b + w]) / w
        enbuyuk = max(enbuyuk, e)
    return math.sqrt(enbuyuk)


def seviye_tepe(ornekler, hedef_db, en_fazla=0.8):
    """kaydet()'e verilecek tepe: en gür 100 ms'nin RMS'i 'hedef_db' olsun, tepe 0,8'i aşmasın."""
    rms = en_gur_rms(ornekler) or 1e-9
    tepe = max(abs(x) for x in ornekler) or 1.0
    return min(en_fazla, tepe * (10 ** (hedef_db / 20)) / rms)


# ---------- KÜÇÜK ALETLER ----------

def zarf(ornekler, giris, cikis):
    """Başta 'giris', sonda 'cikis' saniyelik yumuşak açılış/kapanış (dosyanın ucunda tık olmasın)."""
    n = len(ornekler)
    g, c = max(1, int(giris * HIZ)), max(1, int(cikis * HIZ))
    s = list(ornekler)
    for i in range(min(g, n)):
        s[i] *= i / g
    for i in range(min(c, n)):
        s[n - 1 - i] *= i / c
    return s


def tane_zarfi(n, kisa, uzun, alt, ust, yumusak=0.02):
    """
    Pürüzlü bir yüzeyin sürtünmesi düzgün bir "şşş" değildir: tahta lifleri, kum taneleri, kapağın dişleri
    sürtünmeyi bir anlığına artırır, bir anlığına azaltır. Bu fonksiyon o "taneli" dalgalanmayı üretir:
    'kisa'-'uzun' saniye süren parçalar, her biri 'alt'-'ust' arası rastgele bir güçte. Parçaların arası
    tek kutuplu bir süzgeçle ('yumusak') yumuşatılır; yoksa her parça sınırında bir tık duyulurdu.
    """
    z, i = [0.0] * n, 0
    while i < n:
        m = random.randint(max(1, int(kisa * HIZ)), max(1, int(uzun * HIZ)))
        g = random.uniform(alt, ust)
        for j in range(i, min(n, i + m)):
            z[j] = g
        i += m
    sonuc, onceki = [], z[0]
    for x in z:
        onceki += yumusak * (x - onceki)
        sonuc.append(onceki)
    return sonuc


def kayan_bant(ornekler, frekans_fonk, q):
    """
    Merkez frekansı ZAMANLA kayan bir bant geçiren süzgeç. Menteşe açıldıkça sürtünen yüzeylerin basıncı
    ve hızı değişir; gıcırtının çınladığı perde de kayar. ses_ortak.bant() sabit frekanslıdır; burada
    aynı formülün katsayıları her 16 örnekte yeniden hesaplanır, süzgecin hafızası (önceki iki girdi ve
    çıktı) korunur. frekans_fonk(t) saniye cinsinden zamanı alıp o anın merkez frekansını verir.
    """
    x1 = x2 = y1 = y2 = 0.0
    sonuc = []
    for i, x in enumerate(ornekler):
        if i % 16 == 0:
            w = 2 * math.pi * frekans_fonk(i / HIZ) / HIZ
            c, a = math.cos(w), math.sin(w) / (2 * q)
            a0 = 1 + a
            b0, b2, a1, a2 = a / a0, -a / a0, -2 * c / a0, (1 - a) / a0
        y = b0 * x + b2 * x2 - a1 * y1 - a2 * y2
        x2, x1, y2, y1 = x1, x, y1, y
        sonuc.append(y)
    return sonuc


def yapis_kay(n, bas, bit, siklik_fonk, guc_fonk, darbe_suresi=0.0012, titreklik=0.12):
    """
    Gıcırtının "motoru": yapış-kay tıkları dizisi. 'bas'-'bit' saniyeleri arasında, aralıkları
    1/siklik_fonk(t) olan kısa gürültü darbeleri dizer; gücü guc_fonk(t), her tıkta biraz rastgele.
    'titreklik' aralıkların ne kadar düzensiz olduğudur: sıfır olsa makine gibi düzgün bir vızıltı olurdu,
    gerçek sürtünme hep biraz düzensizdir. Bu dizi tek başına bir "tırtırtır"dır; cismin modlarından
    geçince gıcırtı olur.
    """
    s = [0.0] * n
    m = max(1, int(darbe_suresi * HIZ))
    t = bas
    while t < bit:
        ekle(s, darbe(m, HIZ, darbe_suresi * 0.6), int(t * HIZ), guc_fonk(t) * random.uniform(0.6, 1.0))
        t += (1.0 / siklik_fonk(t)) * random.uniform(1 - titreklik, 1 + titreklik)
    return s


def kucuk_oda(s, islak=0.1, boyut=0.35, oda=0.45):
    """Dairenin küçük bir odası: sık yansımalar (küçük oda), kısa kuyruk, kısık yankı payı."""
    return yanki(s, HIZ, boyut=boyut, sonum=0.5, islak=islak, oda=oda)


def yaz(klasor, ad, s, hedef_db):
    """Sesi seviyesine göre ölçekleyip kaydeder ve üretilenler listesine ekler (sonda tablo basılır)."""
    yol = os.path.join(klasor, ad + ".wav")
    kaydet(yol, s, HIZ, seviye_tepe(s, hedef_db))
    URETILENLER.append(yol)


URETILENLER = []


# ---------- 1. ÇEKMECE AÇILMA ----------
# Tahta çekmece tahta kızaklarda çekilir. Fiziksel olarak üç şey olur:
#   SÜRTÜNME: çekmecenin yan tahtası kızağın üstünde kayar. Bu, 400-2500 Hz arasında bir hışırtıdır
#     (tahta lifleri tiz bir cızırtıya izin vermeyecek kadar yumuşaktır). Lifler ve toz yüzünden sürtünme
#     taneli dalgalanır (tane_zarfi). Aynı sürtünme çekmecenin kutusunu da hafifçe titreştirir: kutunun
#     alçak, geniş modları (230/510/1150 Hz, q küçük) sese "tahta kutu" rengini verir.
#   TIK: çekmece sonuna gelince durdurucuya hafifçe çarpar: kısa bir darbe, tahtanın modlarından.
#   ŞINGIRTI: içindeki küçük eşyalar (kalem, anahtar, düğme) bu duruşla kayıp birbirine değer: birkaç
#     minik, tiz tık, giderek seyrekleşen.
def cekmece():
    random.seed(9701)
    n = int(0.62 * HIZ)
    s = [0.0] * n
    bas, bit = 0.02, 0.37                                    # sürtünme 0,35 sn sürer
    m = int((bit - bas) * HIZ)
    g = gurultu(m)
    tane = tane_zarfi(m, 0.002, 0.007, 0.25, 1.0)            # liflerin taneli dalgalanması
    hisirti = alcak(yuksek(g, 400, HIZ), 2500, HIZ)          # 400-2500 Hz arası sürtünme
    kutu = modlar(g, HIZ, [(230, 2.5, 1.0), (510, 3.0, 0.8), (1150, 3.0, 0.5)])   # çekmece kutusunun tınısı
    hisirti = tepeyi_esitle(hisirti)                         # iki katmanı aynı tepeye getir, sonra karıştır
    kutu = tepeyi_esitle(kutu)
    surt = []
    for i in range(m):
        e = i / m
        hiz_egrisi = min(1.0, e / 0.12) * (1 - 0.35 * e)     # çabuk hızlanır, sona doğru biraz yavaşlar
        surt.append((hisirti[i] + 0.35 * kutu[i]) * tane[i] * hiz_egrisi)
    ekle(s, surt, int(bas * HIZ), 0.9)
    # Durdurucuya tık: tahta modları, orta darlıkta.
    tik = modlar(darbe(int(0.2 * HIZ), HIZ, 0.0025), HIZ, [(180, 3, 0.8), (420, 4, 1.0), (950, 4, 0.7), (2100, 3, 0.35)])
    ekle(s, tik, int(0.375 * HIZ), 0.9)
    # İçindekilerin şıngırtısı: 5 minik tık, tiz modlar, giderek kısılıp seyrekleşir.
    t = 0.385
    for k in range(5):
        f = random.uniform(2500, 5000)
        minik = modlar(darbe(int(0.05 * HIZ), HIZ, 0.0006), HIZ, [(f, 6, 1.0), (f * 1.7, 5, 0.4)])
        ekle(s, minik, int(t * HIZ), 0.35 * (0.75 ** k) * random.uniform(0.7, 1.0))
        t += random.uniform(0.012, 0.025) * (1.25 ** k)
    s = zarf(s, 0.004, 0.06)
    s = kucuk_oda(s)
    yaz(SESLER, "Cekmece_Acilma", yumusak_sinirla(s, 3.5), ESYA_DB)


# ---------- 2. KAPAK AÇILMA ----------
# Küçük bir tahta dolap kapağı ya da sandık kapağı. Önce kapak kasadan (ya da mıknatıslı kilidinden)
# kurtulur: hafif bir tahta tıkı. Sonra küçük menteşe döner ve gıcırdar. Menteşe KÜÇÜK olduğu için
# oda kapısından iki yönden farklıdır:
#   - yapış-kay tıkları daha SIK gelir (180-320 Hz): küçük pim, küçük takılma mesafesi.
#   - çınlayan bant daha TİZDİR ve açıldıkça yükselir (900 -> 1400 Hz, q 7): küçük metal parça tiz çınlar,
#     kapak açıldıkça menteşedeki basınç artar, perde kayar.
# Kapağın tıkı da küçük, ince bir panelin tıkıdır: modları oda kapısınınkinden tiz (420-4100 Hz).
def kapak():
    random.seed(9702)
    n = int(0.6 * HIZ)
    s = [0.0] * n
    # Kasadan kurtulma tıkı (ince panel).
    tik = modlar(darbe(int(0.15 * HIZ), HIZ, 0.0015), HIZ, [(420, 3, 0.7), (980, 4, 1.0), (2300, 5, 0.6), (4100, 4, 0.3)])
    ekle(s, tik, int(0.012 * HIZ), 0.8)
    # Gıcırtı: 0,05-0,34 sn arası, tıklar 180'den 320 Hz'e sıklaşır, gücü ortada en büyük.
    bas, bit = 0.05, 0.34
    mot = yapis_kay(n, bas, bit,
                    lambda t: 180 + 140 * (t - bas) / (bit - bas),
                    lambda t: math.sin(math.pi * (t - bas) / (bit - bas)) ** 0.6)
    gic = kayan_bant(mot, lambda t: 900 + 500 * max(0.0, min(1.0, (t - bas) / (bit - bas))), 7)
    gic2 = kayan_bant(mot, lambda t: 2.1 * (900 + 500 * max(0.0, min(1.0, (t - bas) / (bit - bas)))), 6)
    gic = tepeyi_esitle(gic)
    gic2 = tepeyi_esitle(gic2)
    for i in range(n):
        s[i] += 0.55 * gic[i] + 0.2 * gic2[i]
    s = yuksek(s, 200, HIZ)                                  # küçük kapakta gümbürtü olmaz
    s = zarf(s, 0.003, 0.08)
    s = kucuk_oda(s)
    yaz(SESLER, "Kapak_Acilma", yumusak_sinirla(s, 2.2), ESYA_DB)


# ---------- 3. BUZDOLABI AÇILMA ----------
# 80'lerin buzdolabı. Kapının lastik contası gövdeye yapışmıştır (soğuk ve hafif vakumlu). Çekince:
#   "TÖP": conta önce gerilir, sonra birden ayrılır; içerideki hava hızla yer değiştirir. Bu, yumuşak
#     başlayan, ALÇAK bir hava patlamasıdır: gürültü 300 Hz'in altına süzülür (iki kez: eğim dikleşir),
#     kapı panelinin alçak modu (110/190 Hz) biraz gövde katar. Hemen öncesinde lastiğin soyulması:
#     çok kısık, alçak bir çıtırtı.
#   ŞIŞELER: kapı sarsılınca raftaki cam şişeler birbirine değer. Cam sert ve ince: modları tiz
#     (2-5 kHz) ve q'su büyük (uzun "çın"), ama şişeler dolu ve tutuldukları için kısa sürer. 3 değme,
#     her biri başka şişe (başka perde).
def buzdolabi():
    random.seed(9703)
    n = int(0.75 * HIZ)
    s = [0.0] * n
    # Lastiğin soyulması: 0-50 ms, kısık alçak çıtırtı.
    for _ in range(9):
        t = random.uniform(0.0, 0.045)
        cit = bant(darbe(int(0.02 * HIZ), HIZ, 0.002), random.uniform(250, 600), HIZ, 1.5)
        ekle(s, cit, int(t * HIZ), random.uniform(0.05, 0.12))
    # Töp: yumuşak başlayan 30 ms'lik gürültü patlaması, 300 Hz altına süzülür.
    m = int(0.03 * HIZ)
    patlama = [0.0] * int(0.3 * HIZ)
    for i in range(m):
        patlama[i] = random.uniform(-1, 1) * min(1.0, i / (0.004 * HIZ)) * (1 - i / m) ** 2
    top = alcak(alcak(patlama, 300, HIZ), 300, HIZ)
    govde = modlar(patlama, HIZ, [(110, 2.0, 1.0), (190, 2.5, 0.6)])
    top = [a + 0.5 * b for a, b in zip(tepeyi_esitle(top), tepeyi_esitle(govde))]
    ekle(s, top, int(0.045 * HIZ), 1.0)
    # Şişeler: üç cam değmesi, perdeleri farklı, giderek kısılır.
    for t, guc in ((0.13, 0.55), (0.21, 0.4), (0.33, 0.25)):
        k = random.uniform(0.85, 1.2)
        cin = modlar(darbe(int(0.25 * HIZ), HIZ, 0.0004), HIZ,
                     [(2200 * k, 45, 1.0), (3500 * k, 40, 0.7), (4900 * k, 30, 0.45)])
        ekle(s, cin, int(t * HIZ), guc)
    s = yuksek(s, 40, HIZ)                                   # duyulmayan alt titreşimi at
    s = zarf(s, 0.003, 0.1)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Buzdolabi_Acilma", yumusak_sinirla(s, 2.5), ESYA_DB)


# ---------- 4. METAL KAPAK AÇILMA ----------
# Saç (ince çelik levha) büro dolabının kapağı. Üç olay:
#   MANDAL: kolu çevirince dilin karşılığından çıkması: parlak, kısa metal tık (2,5-6 kHz, q 8-10).
#   GICIRTI: küçük metal menteşe. Metal-metal sürtünme, tahtadan daha sık takılır (400-560 Hz) ve daha
#     dar, tiz bir bantta çınlar (1800 -> 2300 Hz, q 15): ince, "iii" diye bir gıcırtı.
#   SAC TİTREMESİ: ince levha esner; içi boş dolap bir kutu gibi davranır. Modları alçak-orta
#     (180/420/950 Hz) ve q'su BÜYÜK (15-20): metal tahtadan çok daha uzun çınlar. Levha bir iki kez
#     dalgalanır (6 Hz'lik yavaş güç dalgalanması): "vuuub" diye hafif boğuk bir titreme.
def metal_kapak():
    random.seed(9704)
    n = int(0.8 * HIZ)
    s = [0.0] * n
    # Mandal tıkı.
    mandal = modlar(darbe(int(0.1 * HIZ), HIZ, 0.0006), HIZ, [(2500, 8, 0.8), (4200, 10, 1.0), (6000, 8, 0.5)])
    ekle(s, mandal, int(0.01 * HIZ), 0.7)
    # Sac titremesi: yumuşak, uzunca (8 ms) bir esneme darbesi, iki kez (mandal kurtuluşu + kapağın açılışı).
    for t, guc in ((0.015, 1.0), (0.09, 0.55)):
        m = int(0.008 * HIZ)
        esne = [0.0] * int(0.7 * HIZ)
        for i in range(m):
            esne[i] = random.uniform(-1, 1) * math.sin(math.pi * i / m)
        sac = modlar(esne, HIZ, [(180, 15, 1.0), (420, 20, 0.8), (950, 18, 0.45)])
        sac = [x * (0.75 + 0.25 * math.cos(2 * math.pi * 6 * i / HIZ)) for i, x in enumerate(sac)]
        ekle(s, sac, int(t * HIZ), guc)
    # İnce metal gıcırtı: 0,06-0,32 sn.
    bas, bit = 0.06, 0.32
    mot = yapis_kay(n, bas, bit,
                    lambda t: 400 + 160 * (t - bas) / (bit - bas),
                    lambda t: math.sin(math.pi * (t - bas) / (bit - bas)) ** 0.8, darbe_suresi=0.0008)
    gic = kayan_bant(mot, lambda t: 1800 + 500 * max(0.0, min(1.0, (t - bas) / (bit - bas))), 15)
    ekle(s, tepeyi_esitle(gic), 0, 0.3)
    s = zarf(s, 0.002, 0.12)
    s = kucuk_oda(s, islak=0.12, boyut=0.4, oda=0.55)        # büro: biraz daha büyük, boş oda
    yaz(SESLER, "Metal_Kapak_Acilma", yumusak_sinirla(s, 2.0), ESYA_DB)


# ---------- 5. KAVANOZ AÇILMA ----------
# Cam kavanozun teneke vida kapağı. Döndürürken kapağın dişleri camın dişleri üstünde kayar; cam
# pürüzlüdür, tenekenin içindeki conta kurumuştur: sık, sert taneli bir cızırtı (1,5-5 kHz). Her tane
# teneke kapağı da hafifçe çınlatır (ince teneke diski: 3,1 / 4,7 kHz, q 10). Sonra vakum bozulur:
# kapağın ortası "pıt" diye dışarı fırlar: kısa bir hava patlaması (600-1500 Hz) ve teneke diskin tiz
# "tink"i.
def kavanoz():
    random.seed(9705)
    n = int(0.5 * HIZ)
    s = [0.0] * n
    bas, bit = 0.01, 0.26
    m = int((bit - bas) * HIZ)
    g = gurultu(m)
    tane = tane_zarfi(m, 0.0008, 0.003, 0.0, 1.0, yumusak=0.15)   # sert, sık taneler
    cizirti = alcak(yuksek(g, 1500, HIZ), 5000, HIZ)
    cizirti = [x * tane[i] * math.sin(math.pi * i / m) ** 0.4 for i, x in enumerate(cizirti)]
    teneke = modlar(cizirti, HIZ, [(3100, 10, 1.0), (4700, 10, 0.6)])
    karisim = [a + 0.3 * b for a, b in zip(tepeyi_esitle(cizirti), tepeyi_esitle(teneke))]
    ekle(s, karisim, int(bas * HIZ), 0.6)
    # Pıt: vakum bozulur.
    pit_hava = bant(darbe(int(0.15 * HIZ), HIZ, 0.003), 1000, HIZ, 1.2)
    pit_disk = modlar(darbe(int(0.15 * HIZ), HIZ, 0.0008), HIZ, [(1900, 12, 1.0), (3400, 12, 0.6)])
    ekle(s, tepeyi_esitle(pit_hava), int(0.28 * HIZ), 0.8)
    ekle(s, tepeyi_esitle(pit_disk), int(0.281 * HIZ), 0.55)
    s = zarf(s, 0.003, 0.06)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Kavanoz_Acilma", yumusak_sinirla(s, 2.5), ESYA_DB)


# ---------- 6. AĞIR SÜRTÜNME ----------
# Ağır tahta gardırop parkede itilir. Ayakları parkeye yüzlerce kilo bastırır; sürtünme alçak ve kalındır
# (80-900 Hz): tiz bir cızırtı değil, bir hırıltı. Ağır cisim düzgün kaymaz: itilir, takılır, biraz kayar,
# yine takılır. Bu iki ölçekte duyulur:
#   YAVAŞ KABARMA: itişin kendisi; güç saniyede birkaç kez kabarıp iner (yavaş tane_zarfi).
#   SARSINTI: ayakların saniyede 25-40 kez takılıp kurtulması: alçak bir yapış-kay; gardırobun gövde
#     modlarını (95/160/340 Hz) titreştirir. "Gr-r-r-r" dokusunu veren budur.
#   TAHTA İNLEMESİ: gardırobun kendi tahtası zorlanır: arada bir kısa, orta perdeli gıcırtılar.
# Aynı ses "çok ağır, zorladım ama kımıldamadı" için de uyar: zorlanma ve inleme orada da vardır.
def agir_surtunme():
    random.seed(9706)
    n = int(1.3 * HIZ)
    bas, bit = 0.02, 1.2
    g = gurultu(n)
    hirilti = alcak(alcak(yuksek(g, 80, HIZ), 900, HIZ), 900, HIZ)
    kabarma = tane_zarfi(n, 0.07, 0.18, 0.35, 1.0, yumusak=0.0015)   # itişin yavaş dalgalanması
    hirilti = tepeyi_esitle([x * kabarma[i] for i, x in enumerate(hirilti)])
    sars = yapis_kay(n, bas, bit, lambda t: 28 + 10 * math.sin(2 * math.pi * 1.3 * t),
                     lambda t: kabarma[min(n - 1, int(t * HIZ))], darbe_suresi=0.004, titreklik=0.25)
    sars = tepeyi_esitle(modlar(sars, HIZ, [(95, 4, 1.0), (160, 4, 0.8), (340, 3, 0.5), (700, 2, 0.25)]))
    s = [0.0] * n
    for i in range(n):
        t = i / HIZ
        e = min(1.0, max(0.0, (t - bas) / 0.1)) * min(1.0, max(0.0, (bit - t) / 0.15))
        s[i] = e * (0.6 * hirilti[i] + 0.55 * sars[i])
    # Tahta inlemeleri: 5 kısa gıcırtı, rastgele anlarda.
    for _ in range(5):
        t0 = random.uniform(0.1, 1.0)
        sure = random.uniform(0.04, 0.08)
        siklik = random.uniform(120, 200)
        mot = yapis_kay(int((sure + 0.1) * HIZ), 0.0, sure, lambda t: siklik,
                        lambda t: math.sin(math.pi * t / sure), darbe_suresi=0.0015)
        f = random.uniform(450, 600)
        inle = modlar(mot, HIZ, [(f, 6, 1.0), (f * 2.3, 6, 0.5)])
        ekle(s, tepeyi_esitle(inle), int(t0 * HIZ), random.uniform(0.25, 0.45))
    s = yuksek(s, 45, HIZ)
    s = kucuk_oda(s, islak=0.12, boyut=0.4, oda=0.6)         # salon: biraz daha büyük oda
    yaz(SESLER, "Agir_Surtunme", yumusak_sinirla(s, 2.0), ESYA_DB)


# ---------- 7. ÇEKMECE KİLİTLİ ----------
# Kilitli çekmece çekiştirilir: çekmece birkaç milimetre gelir ve kilidin dili karşılığa dayanır. İki
# çekiş, iki "tak": tahta ön panelin modları (160-1900 Hz, q 3) ve kilit dilinin küçük metal tıkı
# (3,2/5,1 kHz, q 10, kısık). Her çarpışta çekmecenin içindeki şeyler hafifçe kıpırdar: bir iki minik tık.
# İkinci çekiş daha hafiftir (oyuncu kilitli olduğunu anladı).
def cekmece_kilitli():
    random.seed(9707)
    n = int(0.36 * HIZ)
    s = [0.0] * n
    for t, guc in ((0.01, 1.0), (0.11, 0.7)):
        tak = modlar(darbe(int(0.2 * HIZ), HIZ, 0.002), HIZ, [(160, 3, 0.8), (380, 3, 1.0), (850, 3, 0.7), (1900, 3, 0.35)])
        dil = modlar(darbe(int(0.1 * HIZ), HIZ, 0.0005), HIZ, [(3200, 10, 1.0), (5100, 10, 0.5)])
        ekle(s, tepeyi_esitle(tak), int(t * HIZ), guc)
        ekle(s, tepeyi_esitle(dil), int((t + 0.001) * HIZ), 0.25 * guc)
        for k in range(2):
            f = random.uniform(2500, 4500)
            minik = modlar(darbe(int(0.04 * HIZ), HIZ, 0.0005), HIZ, [(f, 6, 1.0)])
            ekle(s, tepeyi_esitle(minik), int((t + random.uniform(0.015, 0.05)) * HIZ), 0.12 * guc)
    s = zarf(s, 0.002, 0.06)
    s = kucuk_oda(s)
    yaz(SESLER, "Cekmece_Kilitli", yumusak_sinirla(s, 4.0), ESYA_DB)


# ---------- 8. CAM TIKIRTI ----------
# Cam kavanoza dokunulur ya da sallanır: iki hızlı cam değmesi. Cam çok serttir: çarpma çok kısadır
# (0,3 ms: bütün tizler birden uyarılır) ve kavanozun duvarı tiz modlarında (1,7 / 3,9 / 6,2 kHz) dar
# bantlarda çınlar (q 60-80). Kavanoz elle tutulduğu ve içi dolu olduğu için çınlama kısa söner.
# İkinci değme başka bir noktaya gelir: modlar biraz kayar (×1,04), daha hafif.
def cam_tikirti():
    random.seed(9708)
    n = int(0.3 * HIZ)
    s = [0.0] * n
    for t, k, guc in ((0.008, 1.0, 1.0), (0.085, 1.04, 0.65)):
        cin = modlar(darbe(int(0.25 * HIZ), HIZ, 0.0003), HIZ,
                     [(1700 * k, 60, 0.8), (3900 * k, 70, 1.0), (6200 * k, 60, 0.55)])
        ekle(s, cin, int(t * HIZ), guc)
    s = yuksek(s, 600, HIZ)
    s = zarf(s, 0.002, 0.05)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Cam_Tikirti", yumusak_sinirla(s, 3.0), ALMA_DB)


# ---------- 9. AL: METAL ----------
# Metal el aleti (tornavida, levye) tahtanın üstünden alınır: ucu tahtada kısa bir sürtünme yapar
# (sert uç, tiz taneli cızırtı, 2-6 kHz) ve kaldırılırken bir yere hafifçe değer: kısa bir "tın".
# Çelik çubuk modları tiz ve dar (1,8 / 3,2 / 5,7 kHz, q 40-60); yanında tahtanın tok bir izi (250/600 Hz).
def al_metal():
    random.seed(9709)
    n = int(0.42 * HIZ)
    s = [0.0] * n
    m = int(0.12 * HIZ)
    tane = tane_zarfi(m, 0.001, 0.004, 0.1, 1.0, yumusak=0.1)
    cizik = alcak(yuksek(gurultu(m), 2000, HIZ), 6000, HIZ)
    cizik = [x * tane[i] * math.sin(math.pi * i / m) for i, x in enumerate(cizik)]
    ekle(s, tepeyi_esitle(cizik), int(0.005 * HIZ), 0.35)
    tin = modlar(darbe(int(0.3 * HIZ), HIZ, 0.0005), HIZ, [(1800, 50, 0.8), (3200, 60, 1.0), (5700, 40, 0.6)])
    tok = modlar(darbe(int(0.1 * HIZ), HIZ, 0.002), HIZ, [(250, 2, 1.0), (600, 2.5, 0.6)])
    ekle(s, tepeyi_esitle(tin), int(0.13 * HIZ), 0.75)
    ekle(s, tepeyi_esitle(tok), int(0.13 * HIZ), 0.4)
    s = zarf(s, 0.003, 0.06)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Al_Metal", yumusak_sinirla(s, 3.0), ALMA_DB)


# ---------- 10. AL: KÂĞIT ----------
# Kâğıt ya da fotoğraf eline alınır. Kâğıt bükülünce lifler tek tek kırılır/kayar: her biri çok kısa
# (0,5-3 ms) bir gürültü tanesi, her tanenin parlaklığı farklı (2-8 kHz arası rastgele bir bant). Taneler
# düzensiz kümeler halinde gelir: kavrarken yoğun, bırakırken seyrek. Altta kâğıdın parmakta kayması: çok
# kısık, tiz bir hışırtı. Poşetteki kırışmalardan daha kısa ve daha tiz: tek yaprak, ince kâğıt.
def al_kagit():
    random.seed(9710)
    L = 0.42
    n = int(L * HIZ)
    s = [0.0] * n
    his = bant(gurultu(n), 4000, HIZ, 0.7)
    for i in range(n):
        t = i / HIZ
        e = min(1.0, t / 0.03) * max(0.0, 1 - t / 0.34)
        s[i] += 0.08 * his[i] * e
    for _ in range(55):
        t0 = random.betavariate(1.4, 2.8) * (L - 0.08)
        m = random.randint(int(0.0005 * HIZ), int(0.003 * HIZ))
        tane = [random.uniform(-1, 1) * (1 - j / m) ** 1.5 for j in range(m)] + [0.0] * int(0.006 * HIZ)
        tane = bant(tane, random.uniform(2000, 8000), HIZ, random.uniform(1.0, 3.0))
        ekle(s, tane, int(t0 * HIZ), random.uniform(0.3, 1.0))
    s = alcak(yuksek(s, 700, HIZ), 11000, HIZ)
    s = zarf(s, 0.002, 0.05)
    s = kucuk_oda(s, islak=0.05)
    yaz(SESLER, "Al_Kagit", yumusak_sinirla(s, 1.6), ALMA_DB)


# ---------- 11. AL: SERAMİK ----------
# Seramik tabak kaldırılır ya da konur. Seramik camdan daha yumuşak sırlı bir çömlektir: modları orta-tiz
# (1,1 / 2,7 / 4,4 kHz) ve q'su camdan küçük (25-30): "çın" der ama camın parlak şıngırtısı kadar uzun
# değil. Yanında tabağın konduğu tahtanın tok izi (300 Hz). Hemen ardından tabağın kenarı ikinci kez,
# hafifçe değer (tabak tam düz oturmaz).
def al_seramik():
    random.seed(9711)
    n = int(0.42 * HIZ)
    s = [0.0] * n
    for t, k, guc, ds in ((0.008, 1.0, 1.0, 0.0006), (0.12, 1.012, 0.3, 0.0004)):
        cin = modlar(darbe(int(0.3 * HIZ), HIZ, ds), HIZ, [(1100 * k, 25, 1.0), (2700 * k, 30, 0.75), (4400 * k, 25, 0.45)])
        tok = modlar(darbe(int(0.1 * HIZ), HIZ, 0.0015), HIZ, [(300, 2, 1.0)])
        ekle(s, tepeyi_esitle(cin), int(t * HIZ), guc)
        ekle(s, tepeyi_esitle(tok), int(t * HIZ), 0.3 * guc)
    s = zarf(s, 0.002, 0.06)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Al_Seramik", yumusak_sinirla(s, 3.2), ALMA_DB)


# ---------- 12. AL: KÜÇÜK ----------
# Küçük bir sigorta ya da porselen parça avuca alınır: parmakla ilk değme minik, sert bir tık (porselen:
# 3 / 5,5 kHz, q 10; metal başlığı için 1,4 kHz). Sonra parça elde/yüzeyde biraz yuvarlanır: birkaç
# minik, giderek kısılan tık.
def al_kucuk():
    random.seed(9712)
    n = int(0.24 * HIZ)
    s = [0.0] * n
    tik = modlar(darbe(int(0.1 * HIZ), HIZ, 0.0005), HIZ, [(1400, 6, 0.5), (3000, 10, 1.0), (5500, 10, 0.6)])
    ekle(s, tepeyi_esitle(tik), int(0.005 * HIZ), 1.0)
    t = 0.035
    for k in range(4):
        f = random.uniform(2500, 6000)
        minik = modlar(darbe(int(0.04 * HIZ), HIZ, 0.0004), HIZ, [(f, 8, 1.0), (f * 1.6, 6, 0.4)])
        ekle(s, tepeyi_esitle(minik), int(t * HIZ), 0.55 * (0.75 ** k))
        t += random.uniform(0.015, 0.03)
    s = yuksek(s, 500, HIZ)
    s = zarf(s, 0.002, 0.04)
    s = kucuk_oda(s, islak=0.06)
    yaz(SESLER, "Al_Kucuk", yumusak_sinirla(s, 4.5), ALMA_DB)


# ---------- 13. KİLİT KÜÇÜK ----------
# Oda kapısında ya da asma kilitte küçük bir anahtar döner. Kilidin içinde iki olay vardır:
#   1. TIK: anahtar pimleri hizalar, silindir dönmeye başlar ve yay yüklü küçük bir parça yerinden atlar.
#   2. TAK: dil (ya da asma kilidin halkası) geri çekilir/fırlar: ikinci, biraz daha güçlü tık.
# Kilit gövdesi küçük, sert pirinç/çelik: modlar tiz ve dar (2,4 / 4,1 / 6,3 kHz, q 20-25), küçük gövdenin
# orta modu 1,2 kHz. Arada anahtarın silindirde sürtünmesi: çok kısık tiz hışırtı. Demir kat kapısının
# kilidinden (KatKapisi_Kilit) küçüktür: alçak gövde modları yok, ses kuru ve kısa.
def kilit_kucuk():
    random.seed(9713)
    n = int(0.45 * HIZ)
    s = [0.0] * n
    # Anahtarın silindirde dönerken sürtünmesi (iki tık arası).
    m = int(0.17 * HIZ)
    surt = bant(gurultu(m), 3500, HIZ, 1.0)
    surt = [x * math.sin(math.pi * i / m) for i, x in enumerate(surt)]
    ekle(s, tepeyi_esitle(surt), int(0.035 * HIZ), 0.08)
    for t, guc, k in ((0.03, 0.7, 1.0), (0.2, 1.0, 0.92)):
        tik = modlar(darbe(int(0.15 * HIZ), HIZ, 0.0004), HIZ,
                     [(1200 * k, 8, 0.45), (2400 * k, 20, 1.0), (4100 * k, 25, 0.8), (6300 * k, 20, 0.45)])
        ekle(s, tepeyi_esitle(tik), int(t * HIZ), guc)
    s = yuksek(s, 600, HIZ)
    s = zarf(s, 0.002, 0.06)
    s = kucuk_oda(s, islak=0.08)
    yaz(SESLER, "Kilit_Kucuk", yumusak_sinirla(s, 4.0), ESYA_DB)


# ---------- 14. DÜĞME BAKALİT ----------
# 90'ların bakalit lamba anahtarı / asansör çağırma düğmesi. Bakalit sert, kırılgan bir plastiktir; içinde
# yay yüklü bir mekanizma "ölü noktayı" geçince birden yerine oturur. Ses: çok kısa (0,4 ms) bir darbe,
# sıkı (q 8-10) orta-tiz modlar (2,1 / 3,3 / 4,0 kHz) ve küçük gövdenin hafif 700 Hz'i. 3 ms önce çok kısık
# bir ön tık (parmağın tuşa değip mekanizmanın sıkışması). Yankı neredeyse yok: düğme kulağın dibinde.
def dugme_bakalit():
    random.seed(9714)
    n = int(0.16 * HIZ)
    s = [0.0] * n
    on = modlar(darbe(int(0.05 * HIZ), HIZ, 0.0003), HIZ, [(2600, 8, 1.0)])
    ekle(s, tepeyi_esitle(on), int(0.004 * HIZ), 0.15)
    tak = modlar(darbe(int(0.12 * HIZ), HIZ, 0.0004), HIZ, [(700, 4, 0.4), (2100, 12, 1.0), (3300, 14, 0.8), (4000, 12, 0.6)])
    ekle(s, tepeyi_esitle(tak), int(0.007 * HIZ), 1.0)
    s = yuksek(s, 300, HIZ)
    s = zarf(s, 0.002, 0.04)
    s = kucuk_oda(s, islak=0.04)
    yaz(SESLER, "Dugme_Bakalit", yumusak_sinirla(s, 4.5), ESYA_DB)


# ---------- 15. KÜÇÜK TIKIRTI ----------
# Sessiz odada küçük bir şey yerine oturur: çekmecede bir anahtar kayar, bir fincan oturur. Tek, yumuşak
# bir tık: kısa ama çok sert olmayan darbe (0,8 ms), orta-tiz modlar (1,8 / 3,2 kHz, q 8-10); ardından
# nesnenin dinlenmeye gelirken yaptığı iki minik kıpırtı. Kasıtlı olarak kısık: bir ipucu, bir uyarı değil.
def kucuk_tikirti():
    random.seed(9715)
    n = int(0.16 * HIZ)
    s = [0.0] * n
    tik = modlar(darbe(int(0.1 * HIZ), HIZ, 0.0008), HIZ, [(1800, 8, 1.0), (3200, 10, 0.6)])
    ekle(s, tepeyi_esitle(tik), int(0.004 * HIZ), 1.0)
    for t, guc in ((0.03, 0.2), (0.055, 0.1)):
        f = random.uniform(2500, 4000)
        ekle(s, tepeyi_esitle(modlar(darbe(int(0.03 * HIZ), HIZ, 0.0004), HIZ, [(f, 8, 1.0)])), int(t * HIZ), guc)
    s = yuksek(s, 400, HIZ)
    s = zarf(s, 0.002, 0.04)
    s = kucuk_oda(s, islak=0.1)
    yaz(SESLER, "Kucuk_Tikirti", yumusak_sinirla(s, 2.6), TIKIRTI_DB)


# ---------- 16. AN: SÜRTÜNME (manken) ----------
# Tahta ayaklı bir manken yerde kendi etrafında döner: ayağın tahta tabanı parkenin üstünde burulur.
# Burulma bir sürtünmedir (200-1500 Hz, yavaş kabarıp inen) ve ağırlık bir noktaya bindiği için yapış-kay
# yapar: tıklar yavaştan (90 Hz) hızlanır (140 Hz) ve tahtanın modlarında (380/820/1700 Hz) gıcırtı olur.
# Başta ağırlığın kayması: hafif, alçak bir tok ses. Kısa, ürkütücü, sanki biri onu yavaşça çevirmiş.
def an_surtunme():
    random.seed(9716)
    n = int(0.75 * HIZ)
    s = [0.0] * n
    tok = modlar(darbe(int(0.2 * HIZ), HIZ, 0.004), HIZ, [(120, 2.5, 1.0), (280, 3, 0.6)])
    ekle(s, tepeyi_esitle(tok), int(0.01 * HIZ), 0.4)
    bas, bit = 0.03, 0.6
    m = int((bit - bas) * HIZ)
    surt = alcak(yuksek(gurultu(m), 200, HIZ), 1500, HIZ)
    kabar = tane_zarfi(m, 0.02, 0.06, 0.4, 1.0, yumusak=0.004)
    surt = [x * kabar[i] * math.sin(math.pi * i / m) ** 0.7 for i, x in enumerate(surt)]
    ekle(s, tepeyi_esitle(surt), int(bas * HIZ), 0.45)
    mot = yapis_kay(n, bas + 0.05, bit - 0.03, lambda t: 90 + 50 * (t - bas) / (bit - bas),
                    lambda t: math.sin(math.pi * (t - bas) / (bit - bas)) ** 1.2, darbe_suresi=0.0015, titreklik=0.18)
    gic = modlar(mot, HIZ, [(380, 6, 1.0), (820, 7, 0.8), (1700, 6, 0.45)])
    ekle(s, tepeyi_esitle(gic), 0, 0.6)
    s = yuksek(s, 70, HIZ)
    s = zarf(s, 0.003, 0.1)
    s = kucuk_oda(s, islak=0.14, boyut=0.45, oda=0.6)
    yaz(ANLAR, "An_Surtunme", yumusak_sinirla(s, 2.0), ESYA_DB)


# ---------- 17. KAPI GICIRTISI DÖNGÜSÜ ----------
# Oyuncu bir kapıyı sürükledikçe çalan eski menteşe gıcırtısı. Kod sesi kapının hızına göre açıp kısar;
# dosya yalnızca DİKİŞSİZ DÖNEN 2 saniyelik bir gıcırtı dokusudur. İki şart:
#   1. Dokunun kendisi dönmeli: tıkların sıklığı (perdesi) ve gücü zamanla değişir ama bu değişim
#      2 sn'lik bir PERİYOTLA tekrar eder (sinüslerin periyodu döngünün uzunluğu): sonda başladığı yere
#      döner. Sabit bir perde makine vızıltısı gibi duyulurdu; değişen perde gerçek bir menteşedir.
#   2. Uçlar birleşmeli: rastgele tıklar yüzünden son ile baş yine de tam aynı olmaz. Ses 2 sn'den biraz
#      uzun üretilir; fazlası (0,25 sn) dosyanın başına çapraz geçişle karıştırılır (baştan başa artan,
#      sondan gelen azalan). Böylece dosyanın son örneği, ilk örneğin "hemen öncesi" olur: dönerken tık yok.
#      Rastgele iki gürültü toplanırken güçleri toplandığı için geçiş eğrisi kosinüs/sinüs biçimlidir
#      (eşit güç): düz çizgi geçişin ortasında ses 3 dB çukura düşerdi.
# Menteşe büyük (oda kapısı): tıklar seyrek (110-190 Hz), modlar alçak-orta (260/540/1150/2300 Hz, q 8-12).
# Yankı dairesel: döngünün sonunun yankısı başına eklenir (ses_ortak.dairesel_yanki).
def kapi_gicirti_dongu():
    random.seed(9717)
    L, X, on = 2.0, 0.25, 0.1                                # döngü, çapraz geçiş, süzgeç ısınma payı
    n, c, p = int(L * HIZ), int(X * HIZ), int(on * HIZ)
    toplam = p + n + c
    T = L                                                    # bütün yavaş değişimlerin periyodu = döngü
    def zaman(t):                                            # 'on' payını çıkarıp döngü zamanına çevir
        return t - on
    def siklik(t):
        u = zaman(t)
        return 150 + 28 * math.sin(2 * math.pi * u / T) + 12 * math.sin(4 * math.pi * u / T + 1.3)
    def guc(t):
        u = zaman(t)
        return 0.78 + 0.16 * math.sin(2 * math.pi * u / T + 0.6) + 0.06 * math.sin(6 * math.pi * u / T)
    mot = yapis_kay(toplam, 0.0, toplam / HIZ, siklik, guc, darbe_suresi=0.0013, titreklik=0.1)
    gic = modlar(mot, HIZ, [(260, 8, 0.7), (540, 10, 1.0), (1150, 12, 0.75), (2300, 9, 0.35)])
    # İnce bir sürtünme hışırtısı (pimle yuvanın kuru sürtünmesi) gıcırtının altında sürekli durur.
    his = bant(gurultu(toplam), 1600, HIZ, 0.8)
    gic = tepeyi_esitle(gic)
    his = tepeyi_esitle(his)
    g = [a + 0.06 * b for a, b in zip(gic, his)]
    g = yuksek(g, 120, HIZ)[p:]                              # ısınma payını at (süzgeçler oturdu)
    sonuc = g[:n]
    for i in range(c):                                       # eşit güçlü çapraz geçiş
        a = i / c
        sonuc[i] = g[i] * math.sin(0.5 * math.pi * a) + g[n + i] * math.cos(0.5 * math.pi * a)
    sonuc = dairesel_yanki(sonuc, HIZ, 0.4, boyut=0.4, sonum=0.5, islak=0.1, oda=0.5)
    yaz(SESLER, "Kapi_Gicirti_Dongu", yumusak_sinirla(sonuc, 1.6), ESYA_DB)


# ---------- RAPOR ----------

def oku(yol):
    with wave.open(yol, "rb") as d:
        n = d.getnframes()
        return [v / 32768 for v in struct.unpack("<%dh" % n, d.readframes(n))], d.getframerate()


def db(x):
    return 20 * math.log10(x) if x > 0 else -999.0


def rapor():
    print()
    print(f"{'dosya':<24}{'süre sn':>8}{'tepe dB':>9}{'RMS dB':>9}{'en gür 100ms':>14}")
    for yol in URETILENLER:
        x, h = oku(yol)
        tepe = max(abs(v) for v in x)
        rms = math.sqrt(sum(v * v for v in x) / len(x))
        print(f"{os.path.basename(yol)[:-4]:<24}{len(x) / h:>8.2f}{db(tepe):>9.1f}{db(rms):>9.1f}{db(en_gur_rms(x)):>14.1f}")
        if os.path.basename(yol) == "Kapi_Gicirti_Dongu.wav":
            # Döngü denetimi: ilk ve son 5 ms'nin gücü yakın olmalı, dönüş noktasındaki sıçrama
            # (son örnek -> ilk örnek) sesin içindeki sıradan örnekten-örneğe farklardan büyük olmamalı.
            k = int(0.005 * h)
            r_bas = math.sqrt(sum(v * v for v in x[:k]) / k)
            r_son = math.sqrt(sum(v * v for v in x[-k:]) / k)
            sicrama = abs(x[0] - x[-1])
            farklar = sorted(abs(x[i + 1] - x[i]) for i in range(len(x) - 1))
            print(f"  DÖNGÜ: ilk 5 ms RMS {db(r_bas):.1f} dB, son 5 ms RMS {db(r_son):.1f} dB "
                  f"(fark {abs(db(r_bas) - db(r_son)):.1f} dB); dönüş sıçraması {sicrama:.4f} "
                  f"(içerideki örnek farklarının ortancası {farklar[len(farklar) // 2]:.4f}, "
                  f"%99'u {farklar[int(len(farklar) * 0.99)]:.4f})")


URETICILER = {
    "cekmece": cekmece, "kapak": kapak, "buzdolabi": buzdolabi, "metal_kapak": metal_kapak,
    "kavanoz": kavanoz, "agir_surtunme": agir_surtunme, "cekmece_kilitli": cekmece_kilitli,
    "cam_tikirti": cam_tikirti, "al_metal": al_metal, "al_kagit": al_kagit, "al_seramik": al_seramik,
    "al_kucuk": al_kucuk, "kilit_kucuk": kilit_kucuk, "dugme_bakalit": dugme_bakalit,
    "kucuk_tikirti": kucuk_tikirti, "an_surtunme": an_surtunme, "kapi_gicirti_dongu": kapi_gicirti_dongu,
}

if __name__ == "__main__":
    # Ad verilirse yalnızca o sesler üretilir; ad verilmezse hepsi. Onaylanmış bir sesi yanlışlıkla
    # yeniden üretmemek için tek tek üretmek iyi.
    for ad in (sys.argv[1:] or list(URETICILER)):
        if ad not in URETICILER:
            print("Bilinmeyen ses:", ad, "| seçenekler:", ", ".join(URETICILER))
            continue
        URETICILER[ad]()
    rapor()
