# an_sesleri_uret.py
# Katların imza korku anlarının seslerini kodla üretir (#96).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/an_sesleri_uret.py [ses adları]
#              (ad verilmezse hepsi; ör. "python an_sesleri_uret.py gicirti damla")
#
# ÜRETİLEN DOSYALAR (Assets/Audio/Anlar/, kat dosyalarına BAĞLANIR, Resources'ta değil)
#   An_Gicirti   sallanan sandalyenin / beşiğin tahtası: her salınımın ucunda bir gıcırtı (2. ve 3. kat)
#   An_Devrilme  üstteki karton koli kayıp yere düşer: sürtünme, boğuk gümbürtü, küçük bir sekme (1. kat)
#   An_Vizilti   elektriksiz dairede lambanın bir an yanması: ark vızıltısı ve çıtırtı (4. kat)
#   An_Damla     su kesikken holdeki kovaya yeniden damlayan su: seyrek, sıklaşan, birden kesilen (5. kat)
#   An_Parazit   tepegözün kendiliğinden açılması: balast uğultusu, fan, çıtırtılar (8. kat)
#
# KURAL (ses_ortak.py'nin başı, Şahan'ın dinleme testi): saf sinüs YOK. Her ses gürültüden doğar,
# cismin kendi frekanslarından (modlar) geçer, sonra odanın yankısını alır. Uğultu bile bir sinüs değil:
# çok dar bantlardan geçirilmiş gürültüdür (gerçek bir balastın titreşen sacı gibi biraz "kirli").
# Her ses kendi tohumuyla (random.seed) üretilir: betik yeniden çalışınca aynı dosya çıkar.

import math, os, random, sys
from ses_ortak import alcak, yuksek, bant, modlar, darbe, gurultu, ekle, yanki, tepeyi_esitle, yumusak_sinirla, kaydet

HIZ = 44100   # kısa, keskin olaylar (gıcırtı, çıtırtı, damla) tizlerini kaybetmesin

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
ANLAR = os.path.join(KOK, "Assets", "Audio", "Anlar")


def zarf(ornekler, giris, cikis):
    """Başta 'giris', sonda 'cikis' saniyelik yumuşak açılış/kapanış (tık sesi olmasın)."""
    n = len(ornekler)
    g, c = max(1, int(giris * HIZ)), max(1, int(cikis * HIZ))
    s = list(ornekler)
    for i in range(min(g, n)):
        s[i] *= i / g
    for i in range(min(c, n)):
        s[n - 1 - i] *= i / c
    return s


def tepe_raporu(ad, ornekler):
    """Kaydetmeden önceki tepe ve ortalama güç (RMS) oranı: sesin ne kadar 'sivri' olduğunu gösterir."""
    tepe = max(abs(x) for x in ornekler) or 1.0
    rms = math.sqrt(sum(x * x for x in ornekler) / len(ornekler)) or 1e-9
    print(f"TEPE {ad}: tepe/rms = {20 * math.log10(tepe / rms):.1f} dB")


# ---------- GICIRTI ----------
# Tahta gıcırtısı bir "yapış-kay" olayıdır: iki tahta birbirine sürtünürken takılır, kurtulur, yine
# takılır. Her kurtuluş küçük bir tıktır; tıklar saniyede 60-120 kez gelir ve tahtanın modlarını
# titreştirir. Tıkların sıklığı sürtünmenin hızıyla değişir (yavaş başlar, hızlanır, yavaşlar): kulak
# bunu "iii-ı" diye perdesi kayan bir inilti olarak duyar. Perdeyi tıkların ARALIĞI yapar, sinüs değil.
def gicirti():
    random.seed(9601)
    sure = 0.75
    n = int(sure * HIZ)
    uyari = [0.0] * n
    t = 0.02
    while t < sure - 0.05:
        e = t / sure
        # Sıklık: 55 Hz'den 115'e çıkar, sona doğru 70'e iner (sürtünmenin hızı).
        siklik = 55 + 60 * math.sin(math.pi * min(1.0, e * 1.2)) + 15 * e
        # Güç: ortada en büyük; her tıkın gücü biraz rastgele (tahta pürüzlü).
        guc = math.sin(math.pi * e) ** 0.7 * random.uniform(0.6, 1.0)
        ekle(uyari, darbe(int(0.004 * HIZ), HIZ, 0.0012), int(t * HIZ), guc)
        t += (1.0 / siklik) * random.uniform(0.85, 1.15)   # aralıkta küçük titreklik: makine gibi düzgün olmasın
    # Tahtanın modları: alçak ve orta frekanslar, orta darlıkta (tahta çabuk söner, metal gibi çınlamaz).
    s = modlar(uyari, HIZ, [(310, 6, 1.0), (690, 7, 0.85), (1280, 8, 0.6), (2350, 6, 0.35), (3900, 4, 0.15)])
    s = yuksek(s, 120, HIZ)                       # gümbürtüyü at: gıcırtının gövdesi orta frekanslarda
    s = zarf(s, 0.03, 0.12)
    s = yanki(s + [0.0] * int(0.35 * HIZ), HIZ, boyut=0.45, sonum=0.5, islak=0.18, oda=0.55)   # küçük oda
    tepe_raporu("An_Gicirti", s)
    kaydet(os.path.join(ANLAR, "An_Gicirti.wav"), yumusak_sinirla(s, 1.6), HIZ)


# ---------- DEVRİLME ----------
# Üç olay: (1) koli kenarı alttaki kolinin üstünde sürtünür (karton hışırtısı, 0,2 sn), (2) yere düşer:
# içi boş bir kutunun boğuk "güm"ü (alçak modlar, karton çabuk söner), (3) bir kez hafifçe seker.
def devrilme():
    random.seed(9602)
    n = int(1.5 * HIZ)
    s = [0.0] * n
    # 1. Sürtünme: bant geçiren gürültü, gücü yavaşça artıp kesilir.
    m = int(0.22 * HIZ)
    surt = bant(gurultu(m), 1800, HIZ, 0.9)
    surt = [x * (i / m) ** 1.5 for i, x in enumerate(surt)]
    ekle(s, surt, int(0.02 * HIZ), 0.25)
    # 2. Düşüş: geniş bir darbe, içi boş kartonun modlarından.
    gum = modlar(darbe(n, HIZ, 0.006), HIZ, [(88, 2.2, 1.0), (170, 2.8, 0.85), (380, 3.0, 0.55), (820, 2.2, 0.35), (2100, 1.4, 0.18)])
    ekle(s, gum, int(0.26 * HIZ), 1.0)
    # 3. Sekme: aynı modlar, daha kısa ve kısık.
    sek = modlar(darbe(n, HIZ, 0.003), HIZ, [(120, 2.5, 1.0), (430, 3.0, 0.6), (1100, 2.0, 0.3)])
    ekle(s, sek, int(0.43 * HIZ), 0.3)
    # Kartonun son hışırtısı: kolinin kapakları oturur.
    m2 = int(0.18 * HIZ)
    his = bant(gurultu(m2), 2600, HIZ, 1.2)
    his = [x * (1 - i / m2) ** 2 for i, x in enumerate(his)]
    ekle(s, his, int(0.30 * HIZ), 0.12)
    s = zarf(s, 0.005, 0.2)
    s = yanki(s, HIZ, boyut=0.55, sonum=0.45, islak=0.22, oda=0.7)   # boş, eşyası götürülmüş oda: biraz yankılı
    tepe_raporu("An_Devrilme", s)
    kaydet(os.path.join(ANLAR, "An_Devrilme.wav"), yumusak_sinirla(s, 2.2), HIZ)


# ---------- VIZILTI ----------
# Elektriksiz dairede lamba bir an yanıyor: kötü bir temas. Akım şebekenin her yarım dalgasında
# (saniyede 100 kez) ark yapar; her ark bir gürültü patlamasıdır. Patlamalar duyda ve ampulün camında
# çınlar (orta-tiz modlar). Üstüne düzensiz çıtırtılar. Lambanın titremesine uyar: yan, sön, yan, 0,6 sn yanık.
def vizilti():
    random.seed(9603)
    n = int(1.05 * HIZ)
    uyari = [0.0] * n
    # Lamba açık mı? (KatAni.IsikSiluet'in adımlarıyla aynı: 0,06 yan, 0,07 sön, 0,05 yan, sonra 0,6 yanık)
    def acik(t):
        return t < 0.06 or 0.13 <= t < 0.78
    yarim = 1.0 / 100
    t = 0.0
    while t < 0.8:
        if acik(t):
            ekle(uyari, darbe(int(0.003 * HIZ), HIZ, 0.0015), int(t * HIZ), random.uniform(0.5, 1.0))
        t += yarim * random.uniform(0.97, 1.03)
    # Çıtırtılar: seyrek, güçlü, kısa.
    for _ in range(14):
        t = random.uniform(0.0, 0.78)
        if acik(t):
            ekle(uyari, darbe(int(0.006 * HIZ), HIZ, 0.0008), int(t * HIZ), random.uniform(1.2, 2.2))
    s = modlar(uyari, HIZ, [(200, 12, 0.7), (400, 14, 0.6), (1150, 8, 0.5), (3100, 6, 0.4), (5200, 4, 0.25)])
    s = yuksek(s, 90, HIZ)
    s = zarf(s, 0.002, 0.08)
    s = yanki(s, HIZ, boyut=0.4, sonum=0.5, islak=0.12, oda=0.6)
    tepe_raporu("An_Vizilti", s)
    kaydet(os.path.join(ANLAR, "An_Vizilti.wav"), yumusak_sinirla(s, 1.8), HIZ)


# ---------- DAMLA ----------
# Kovadaki suya düşen damla iki ses çıkarır: suya çarpışın kısa "pıt"ı (geniş bant, kısık) ve suyun
# altında kalan küçük hava kabarcığının "plink"i (dar bir band: kabarcık kendi frekansında çınlar,
# 900-1800 Hz arası, kabarcığın boyuna göre her damlada başka). Damlalar seyrek başlar (0,9 sn arayla),
# sıklaşır (0,15 sn), sonra birden kesilir: kovanın kenarında tek bir son damla.
def damla():
    random.seed(9604)
    n = int(5.2 * HIZ)
    s = [0.0] * n
    t, aralik = 0.15, 0.9
    zamanlar = []
    while t < 3.9:
        zamanlar.append(t)
        t += aralik * random.uniform(0.85, 1.15)
        aralik = max(0.14, aralik * 0.84)       # her damlada aralık kısalır: hızlanan damlama
    zamanlar.append(t + 0.6)                     # bir süre sessizlik, son bir damla
    for i, z in enumerate(zamanlar):
        m = int(0.35 * HIZ)
        dar = random.uniform(950, 1750)
        tek = modlar(darbe(m, HIZ, 0.0015), HIZ, [(dar, 28, 1.0), (dar * 1.9, 18, 0.25), (320, 3, 0.35), (2600, 2, 0.2)])
        guc = random.uniform(0.6, 1.0) * (0.75 if i == len(zamanlar) - 1 else 1.0)
        ekle(s, tek, int(z * HIZ), guc)
    s = yuksek(s, 200, HIZ)
    s = zarf(s, 0.01, 0.3)
    s = yanki(s, HIZ, boyut=0.62, sonum=0.35, islak=0.3, oda=0.9)   # karo döşeli, eşyasız hol: belirgin yankı
    tepe_raporu("An_Damla", s)
    kaydet(os.path.join(ANLAR, "An_Damla.wav"), yumusak_sinirla(s, 1.5), HIZ)


# ---------- PARAZİT ----------
# Tepegöz açılıyor: (1) şalterin "tak"ı, (2) balastın uğultusu: 100 Hz ve katlarında titreşen sac; saf
# ton değil, çok dar bantlardan geçirilmiş gürültü (biraz kirli, dalgalı), (3) soğutma fanı: alçak
# geçiren gürültü, yavaşça kalkan devirle, (4) lambanın düzensiz çıtırtıları. Sonunda bir anda kesilir
# (KatAni ışığı ve sesi aynı anda keser; dosyanın sonu da kesik, kısa bir kuyrukla).
def parazit():
    random.seed(9605)
    sure = 5.4
    n = int(sure * HIZ)
    s = [0.0] * n
    # 1. Şalter.
    ekle(s, modlar(darbe(int(0.4 * HIZ), HIZ, 0.002), HIZ, [(160, 3, 1.0), (620, 4, 0.7), (2400, 3, 0.4)]), int(0.01 * HIZ), 0.9)
    # 2. Balast uğultusu: gürültü -> çok dar bantlar. Dalgalanma: güç 0,4 Hz'de hafifçe iner kalkar.
    g = gurultu(n)
    ugul = modlar(g, HIZ, [(100, 60, 1.0), (200, 50, 0.7), (300, 40, 0.45), (500, 30, 0.2)])
    ugul = [x * (0.8 + 0.2 * math.sin(2 * math.pi * 0.4 * i / HIZ)) * min(1.0, i / (0.35 * HIZ)) for i, x in enumerate(ugul)]
    ekle(s, tepeyi_esitle(ugul), 0, 0.35)
    # 3. Fan: alçak geçiren gürültü, devri ilk 1,5 sn'de kalkar (kesim frekansı yükselir gibi: güç artar).
    fan = alcak(alcak(gurultu(n), 700, HIZ), 700, HIZ)
    fan = [x * min(1.0, i / (1.5 * HIZ)) * (0.9 + 0.1 * math.sin(2 * math.pi * 23 * i / HIZ)) for i, x in enumerate(fan)]
    ekle(s, tepeyi_esitle(fan), 0, 0.28)
    # 4. Çıtırtılar: ışığın titrediği anlarda (KatAni her 50 ms'de bir parlaklık seçer) düzensiz tıklar.
    cit = [0.0] * n
    t = 0.3
    while t < sure - 0.3:
        ekle(cit, darbe(int(0.008 * HIZ), HIZ, 0.0007), int(t * HIZ), random.uniform(0.4, 1.0))
        t += random.expovariate(9.0)             # ortalama saniyede 9 çıtırtı, aralıkları düzensiz
    cit = modlar(cit, HIZ, [(2300, 5, 1.0), (4100, 4, 0.7), (6200, 3, 0.4)])
    ekle(s, tepeyi_esitle(cit), 0, 0.45)
    # Kesik son: son 60 ms'de söner (tık olmasın), sonra kısa oda kuyruğu.
    s = zarf(s, 0.002, 0.06)
    s = yanki(s + [0.0] * int(0.4 * HIZ), HIZ, boyut=0.45, sonum=0.5, islak=0.15, oda=0.7)
    tepe_raporu("An_Parazit", s)
    kaydet(os.path.join(ANLAR, "An_Parazit.wav"), yumusak_sinirla(s, 1.3), HIZ)


SESLER = {"gicirti": gicirti, "devrilme": devrilme, "vizilti": vizilti, "damla": damla, "parazit": parazit}

if __name__ == "__main__":
    adlar = sys.argv[1:] or list(SESLER)
    for ad in adlar:
        if ad not in SESLER:
            print("Bilinmeyen ses:", ad, "| seçenekler:", ", ".join(SESLER))
            continue
        SESLER[ad]()
