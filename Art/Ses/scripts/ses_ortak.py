# ses_ortak.py
# Ses üreten betiklerin ortak aletleri (#14, #25 revizyonu, 2026-09-26).
#
# İlk turdaki seslerin çoğu "yapay" duyuldu. Sebep hep aynıydı: vuruşlar saf sinüs
# dalgalarından yapılmıştı. Saf sinüs tek bir perdede ötüp söner; bu, kulağa bir
# kovanın ya da zilin çınlaması gibi gelir (Şahan: "ayağına demir kova giymiş gibi").
# Gerçek dünyada bir darbe tek bir perde değildir:
#   1. Darbe ANLIK bir gürültüdür: bütün frekanslar aynı anda, çok kısa süre.
#   2. Cisim (kapı, beton, boru) bu gürültünün yalnızca KENDİ frekanslarını bir süre
#      daha titreştirir; bunlara cismin "doğal frekansları" (modları) denir.
#      Tahta kapının modları alçak ve çabuk söner, metal borununki tiz ve uzun sürer.
#   3. Sesin geçtiği yer (beton merdiven boşluğu) onu yankılarla uzatır.
# Bu dosyadaki aletler tam bu üç adımı yapar: gürültü üret, onu cismin modlarından
# geçir (süzgeç), sonra yankı ekle. Bütün betikler bunları kullanır.

import math, os, random, struct, wave


# ---------- SÜZGEÇLER ----------
# "Biquad" süzgeç: her örneği, kendinden önceki iki girdi ve iki çıktıyla harmanlayan
# küçük bir formül. Katsayılarını değiştirerek üç iş yaptırıyoruz:
#   alcak  (low-pass):  kesim frekansının üstünü törpüler (duvar arkası, boğukluk)
#   yuksek (high-pass): kesim frekansının altını çıkarır (gümbürtüyü atar)
#   bant   (band-pass): yalnızca bir frekans çevresini bırakır (bir cismin modu)
# "q" bandın darlığıdır: q büyüdükçe bant daralır ve o frekans DAHA UZUN çınlar.
# (Katsayılar Robert Bristow-Johnson'ın yaygın "Audio EQ Cookbook" formülleri.)

def _biquad(ornekler, b0, b1, b2, a0, a1, a2):
    b0, b1, b2, a1, a2 = b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0   # a0'a göre ölçekle
    x1 = x2 = y1 = y2 = 0.0                                             # önceki iki girdi/çıktı
    sonuc = []
    for x in ornekler:
        y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2
        x2, x1, y2, y1 = x1, x, y1, y
        sonuc.append(y)
    return sonuc


def alcak(ornekler, frekans, hiz, q=0.707):
    w = 2 * math.pi * frekans / hiz
    c, a = math.cos(w), math.sin(w) / (2 * q)
    return _biquad(ornekler, (1 - c) / 2, 1 - c, (1 - c) / 2, 1 + a, -2 * c, 1 - a)


def yuksek(ornekler, frekans, hiz, q=0.707):
    w = 2 * math.pi * frekans / hiz
    c, a = math.cos(w), math.sin(w) / (2 * q)
    return _biquad(ornekler, (1 + c) / 2, -(1 + c), (1 + c) / 2, 1 + a, -2 * c, 1 - a)


def bant(ornekler, frekans, hiz, q=1.0):
    w = 2 * math.pi * frekans / hiz
    c, a = math.cos(w), math.sin(w) / (2 * q)
    return _biquad(ornekler, a, 0.0, -a, 1 + a, -2 * c, 1 - a)


def modlar(uyari, hiz, liste):
    """
    Bir cismin sesi: aynı uyarıyı (darbe gürültüsünü) cismin her modundan ayrı ayrı
    geçirip toplar. liste = [(frekans, q, guc), ...]. q büyük olan mod uzun çınlar.
    Dar bir bant, geniş bir banttan çok daha az enerji geçirir; 'guc' değerleri
    anlamlı olsun diye her modun tepesi önce 1'e eşitlenir, sonra 'guc' ile çarpılır.
    """
    toplam = [0.0] * len(uyari)
    for frekans, q, guc in liste:
        m = bant(uyari, frekans, hiz, q)
        tepe = max(abs(x) for x in m) or 1.0
        for i, x in enumerate(m):
            toplam[i] += guc * x / tepe
    return toplam


def yumusak_sinirla(ornekler, surus):
    """
    Darbe sesleri çok "sivri"dir: en yüksek nokta bir anlıktır, sesin gövdesi ondan çok
    kısıktır. Dosya en yüksek noktaya göre ölçeklendiği için böyle bir ses kulağa
    kısık gelir. Bu fonksiyon sivri tepeleri yumuşakça bastırır (tanh eğrisi: küçük
    değerleri olduğu gibi bırakır, büyükleri 1'e yaklaştırıp keser); gövde görece
    büyür, ses daha dolgun ve gür duyulur. 'surus' büyüdükçe bastırma artar (1-4 arası).
    """
    esit = tepeyi_esitle(ornekler)
    b = math.tanh(surus)
    return [math.tanh(surus * x) / b for x in esit]


def tepeyi_esitle(ornekler, tepe=1.0):
    """Sesin en yüksek noktasını 'tepe' yapar (katmanları karıştırmadan önce)."""
    enbuyuk = max(abs(x) for x in ornekler) or 1.0
    return [x * tepe / enbuyuk for x in ornekler]


# ---------- GÜRÜLTÜ VE DARBE ----------

def gurultu(n):
    """n örneklik beyaz gürültü (her örnek -1 ile 1 arası rastgele)."""
    return [random.uniform(-1, 1) for _ in range(n)]


def darbe(n, hiz, sure):
    """
    Bir çarpmanın kendisi: 'sure' saniye süren, hızla sönen bir gürültü patlaması.
    Kalan kısmı sıfırdır; cismin çınlaması bu patlamayı süzgeçten geçirince ortaya çıkar.
    """
    s = [0.0] * n
    m = min(n, max(1, int(sure * hiz)))
    for i in range(m):
        s[i] = random.uniform(-1, 1) * (1 - i / m) ** 2
    return s


def ekle(hedef, kaynak, bas, guc=1.0):
    """'kaynak' sesini 'hedef'e, 'bas' örnekten başlayarak 'guc' şiddetinde ekler."""
    for i, x in enumerate(kaynak):
        j = bas + i
        if 0 <= j < len(hedef):
            hedef[j] += guc * x


# ---------- YANKI ----------
# Beton bir merdiven boşluğunda ses duvarlardan defalarca yansır ve yavaşça söner.
# Bunu taklit eden klasik yöntem ("Freeverb" düzeni): ses, farklı uzunlukta sekiz
# "geri besleme hattından" (comb) geçer. Her hat sesi biraz geciktirip azaltarak
# kendine geri ekler: bir yansımalar dizisi. Uzunluklar birbirinin katı olmadığı
# için yansımalar üst üste binip düzgün bir kuyruk oluşturur. Ardından dört
# "tüm geçiren" (allpass) hat, yansımaları dağıtıp pürüzsüzleştirir.
#   boyut:  0-1, yankının ne kadar uzun sürdüğü (büyük = büyük, sert oda)
#   sonum:  0-1, tizlerin ne çabuk söndüğü (büyük = daha boğuk yankı; beton+sıva ~0,4)
#   islak:  yankının payı; kuru: sesin kendisinin payı

_COMB = (1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617)   # 44,1 kHz'de örnek sayıları
_ALLPASS = (556, 441, 341, 225)


def yanki(ornekler, hiz, boyut=0.7, sonum=0.4, islak=0.3, kuru=1.0, oda=1.0):
    """'oda' gecikmeleri ölçekler: küçük sayı = küçük oda (yansımalar sık)."""
    olcek = hiz / 44100 * oda
    geri = 0.7 + 0.28 * boyut                         # geri besleme: 0,70-0,98
    girdi = [x * 0.03 for x in ornekler]              # hatlar toplanınca taşmasın
    cikis = [0.0] * len(ornekler)
    for d in _COMB:
        uz = max(1, int(d * olcek))
        tampon, k, sakli = [0.0] * uz, 0, 0.0
        for i, x in enumerate(girdi):
            y = tampon[k]
            sakli = y * (1 - sonum) + sakli * sonum   # her yansımada tizler biraz söner
            tampon[k] = x + sakli * geri
            k = k + 1 if k + 1 < uz else 0
            cikis[i] += y
    for d in _ALLPASS:
        uz = max(1, int(d * olcek))
        tampon, k = [0.0] * uz, 0
        for i, x in enumerate(cikis):
            y = tampon[k]
            cikis[i] = y - x
            tampon[k] = x + y * 0.5
            k = k + 1 if k + 1 < uz else 0
    return [kuru * a + islak * b for a, b in zip(ornekler, cikis)]


def dairesel_yanki(ornekler, hiz, kuyruk, **ayar):
    """
    Döngüde çalan bir sese yankı: sonuna 'kuyruk' saniye sessizlik eklenip yankı
    uygulanır, taşan kuyruk kaydın BAŞINA eklenir. Böylece son olayın yankısı bir
    sonraki turun başında sürer ve döngü dikişsiz kalır.
    """
    n = len(ornekler)
    t = yanki(ornekler + [0.0] * int(kuyruk * hiz), hiz, **ayar)
    sonuc = t[:n]
    for i, x in enumerate(t[n:]):
        sonuc[i % n] += x
    return sonuc


# ---------- KAYDETME ----------

def kaydet(yol, ornekler, hiz, tepe=0.8):
    """En yüksek noktayı 'tepe'ye ölçekleyip 16 bit mono WAV yazar."""
    enbuyuk = max(abs(x) for x in ornekler) or 1.0
    olcek = tepe / enbuyuk
    os.makedirs(os.path.dirname(yol), exist_ok=True)
    with wave.open(yol, "wb") as dosya:
        dosya.setnchannels(1)          # mono: Unity 3B konumu kendisi hesaplıyor
        dosya.setsampwidth(2)          # 16 bit
        dosya.setframerate(hiz)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in ornekler))
    print("OK", os.path.basename(yol), round(len(ornekler) / hiz, 2), "sn")
