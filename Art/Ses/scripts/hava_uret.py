# hava_uret.py
# Dışarıdaki havanın seslerini kodla üretir (#95): şimşekten sonra gelen gök gürültüsü.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/hava_uret.py [ses adları]
#              python Art/Ses/scripts/hava_uret.py gok_gurultusu
#
# ÜRETİLEN DOSYALAR
#   Assets/Resources/Sesler/Gok_Gurultusu_1.wav   yakın sayılır: belirgin çatırtı, uzun yuvarlanma
#   Assets/Resources/Sesler/Gok_Gurultusu_2.wav   daha uzak: sönük çatırtı, daha boğuk ve dalgalı
#
# Dosyaları oyunda Simsek.cs çalar; SimsekKurucu onları bileşene BAĞLAR (build yalnızca bağlanmış
# dosyaları paketler, #26 kuralı). Mesafe hissini (gecikme, kısıklık, boğukluk) oyun ayrıca verir.
#
# GÖK GÜRÜLTÜSÜ NEDEN GÜRÜLTÜDEN YAPILIR: gerçek gök gürültüsünde hiçbir perde yoktur. Yıldırım
# havayı bir anda ısıtıp patlatır (ÇATIRTI: bütün frekanslar, çok kısa); sonra kanalın kilometrelerce
# uzunluktaki farklı parçalarından gelen ses kulağa farklı zamanlarda varır (YUVARLANMA: alçak,
# dalgalanan, düzensiz kabaran bir gümbürtü). Sinüs dalgası bunu taklit edemez ("bip" ya da "kova"
# gibi öter, bkz. ses_ortak.py); bu yüzden her katman beyaz gürültünün süzülmesiyle yapılır.

import math, os, random, sys
from ses_ortak import alcak, yuksek, darbe, gurultu, ekle, yanki, tepeyi_esitle, yumusak_sinirla
from ses_ortak import kaydet as kaydet_hiz

# 22 kHz yeterli: gök gürültüsünün enerjisi 3 kHz'in altında, en tiz katman (çatırtı) bile 3 kHz'de kesiliyor.
HIZ = 22050

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")


def yumusak_rastgele(n, hiz, degisim_hz, tohum_rng):
    """
    Yavaş değişen rastgele bir eğri (0-1 arası): her 1/degisim_hz saniyede yeni bir rastgele hedef
    seçilir, aradaki noktalar yumuşak (kosinüs) geçişle doldurulur. Gümbürtünün şiddetini bununla
    dalgalandırıyoruz: düzenli bir titreşim değil, öngörülemeyen kabarıp sönmeler.
    """
    adim = max(1, int(hiz / degisim_hz))                      # iki hedef arası örnek sayısı
    hedefler = [tohum_rng.random() for _ in range(n // adim + 2)]
    sonuc = []
    for i in range(n):
        k, kalan = divmod(i, adim)
        t = kalan / adim
        a = 0.5 - 0.5 * math.cos(math.pi * t)                 # 0'dan 1'e yumuşak geçiş
        sonuc.append(hedefler[k] * (1 - a) + hedefler[k + 1] * a)
    return sonuc


def kabarmalar(n, hiz, sayi, rng):
    """
    Yuvarlanmanın zarfı (envelope): rastgele anlarda, rastgele genişlik ve yükseklikte 'sayi' kadar
    kabarma (çan eğrisi). İlk kabarma hep en başta ve en güçlüsü: yıldırımın en yakın parçası.
    Toplamın üstüne yavaş bir genel sönüş biner; ses sonunda sessizliğe iner.
    """
    zarf = [0.0] * n
    sure = n / hiz
    for k in range(sayi):
        merkez = 0.25 if k == 0 else rng.uniform(0.7, sure * 0.85)   # saniye
        genislik = rng.uniform(0.25, 0.8)                             # saniye
        yukseklik = 1.0 if k == 0 else rng.uniform(0.45, 0.95)
        bas = max(0, int((merkez - 3 * genislik) * hiz))
        son = min(n, int((merkez + 3 * genislik) * hiz))
        for i in range(bas, son):
            x = (i / hiz - merkez) / genislik
            zarf[i] += yukseklik * math.exp(-x * x)                  # çan eğrisi
    for i in range(n):
        t = i / n
        saldiri = min(1.0, (i / hiz) / 0.12)                          # ilk 0,12 sn'de açılır
        sonus = (1 - t) ** 0.8                                        # sona doğru söner (yavaş: kabarmalar seçilsin)
        zarf[i] *= saldiri * sonus
    return zarf


def gok(ad, tohum, uzak):
    """
    Tek bir gök gürültüsü. uzak = 0 (yakın) ... 1 (uzak): uzak olanın çatırtısı sönük, gümbürtüsü
    daha boğuk; ses uzaktan gelirken tizini havada kaybeder.
    """
    rng = random.Random(tohum)
    random.seed(tohum)                       # ses_ortak'ın gurultu/darbe'si 'random' modülünü kullanır
    uzunluk = rng.uniform(4.6, 6.6)          # yuvarlanmanın süresi (saniye)
    kuyruk = 2.5                             # yankı için sonda bırakılan pay
    n = int((uzunluk + kuyruk) * HIZ)
    ses = [0.0] * n

    # 1) ÇATIRTI: kısa, geniş bantlı gürültü patlamaları (3-6 tane, ilk 0,4 saniyede), 800-3000 Hz
    #    arası bırakılır. Tek patlama "tok" bir vuruş olurdu; art arda birkaç tanesi yırtılma hissi verir.
    catirti = [0.0] * n
    for k in range(rng.randint(3, 6)):
        bas = int(rng.uniform(0.0, 0.4) * HIZ)
        p = darbe(int(0.3 * HIZ), HIZ, rng.uniform(0.04, 0.16))        # hızla sönen gürültü
        ekle(catirti, p, bas, rng.uniform(0.5, 1.0) if k else 1.0)
    catirti = alcak(yuksek(catirti, 800, HIZ), 3000, HIZ)           # yalnızca 800-3000 Hz
    catirti = tepeyi_esitle(catirti)

    # 2) YUVARLANMA: beyaz gürültü iki kez 200 Hz'in altına süzülür (iki kat süzgeç: tizler daha
    #    sert kesilir, geriye yalnızca gümbürtü kalır), 30 Hz'in altı atılır (hoparlörü zorlayan,
    #    duyulmayan basınç). Uzak gök gürültüsü daha boğuk: kesim 200 yerine 140 Hz.
    kesim = 200 - 60 * uzak
    yuvarlanma = alcak(alcak(gurultu(n), kesim, HIZ), kesim, HIZ)
    yuvarlanma = yuksek(yuvarlanma, 30, HIZ)
    yuvarlanma = tepeyi_esitle(yuvarlanma)

    # Gümbürtünün içinde biraz daha "dokulu" bir orta katman (200-500 Hz): yuvarlanırken çıtırdayan
    # parçalar. Tek başına alçak gürültü fazla düz, bir motor uğultusu gibi duyulurdu.
    orta = alcak(yuksek(gurultu(n), 200, HIZ), 500, HIZ)
    orta = tepeyi_esitle(orta)

    # Zarf: düzensiz kabarmalar x yavaş rastgele dalgalanma (saniyede ~3 değişim).
    zarf = kabarmalar(n - int(kuyruk * HIZ), HIZ, rng.randint(4, 7), rng) + [0.0] * int(kuyruk * HIZ)
    dalga = yumusak_rastgele(n, HIZ, 3.0, rng)
    titrek = yumusak_rastgele(n, HIZ, 11.0, rng)              # daha hızlı, küçük oynamalar (orta katman için)

    catirti_gucu = 0.85 - 0.55 * uzak       # uzak gök gürültüsünün çatırtısı zor seçilir
    for i in range(n):
        g = zarf[i] * (0.55 + 0.45 * dalga[i])
        ses[i] = g * yuvarlanma[i] + 0.22 * g * titrek[i] * orta[i] + catirti_gucu * catirti[i]

    # 3) YANKI: gökyüzü ve binalar arasında çok büyük bir alan. boyut yüksek (uzun kuyruk), oda 2,4
    #    (yansımalar seyrek: koca bir boşluk), sönüm yüksek (yansıyan ses tizini kaybeder).
    ses = yanki(ses, HIZ, boyut=0.93, sonum=0.6, islak=0.55, kuru=1.0, oda=2.4)

    # Son 0,4 saniye yumuşakça sıfıra iner: dosya sonunda "tık" olmasın.
    son = int(0.4 * HIZ)
    for i in range(son):
        ses[n - son + i] *= 1 - i / son

    # Sivri tepeleri bastır: gümbürtünün gövdesi daha gür duyulur (darbe sesleri gibi, ses_ortak.py).
    ses = yumusak_sinirla(ses, 2.2)
    kaydet_hiz(os.path.join(SESLER, ad + ".wav"), ses, HIZ, tepe=0.85)
    rapor(ad, ses)


def rapor(ad, ses):
    """Süre, tepe ve ortalama güç (RMS) dBFS olarak; dosya 0,85 tepeye ölçeklendiği için tepe sabittir."""
    tepe = max(abs(x) for x in ses) or 1.0
    olcek = 0.85 / tepe
    rms = math.sqrt(sum((x * olcek) ** 2 for x in ses) / len(ses))
    print(f"   {ad}: {len(ses) / HIZ:.2f} sn, tepe {20 * math.log10(0.85):.1f} dBFS, RMS {20 * math.log10(rms):.1f} dBFS")


def gok_gurultusu():
    # İki çeşit, sabit tohumla: tekrar çalıştırınca aynı dosyalar çıkar.
    gok("Gok_Gurultusu_1", 9501, uzak=0.0)
    gok("Gok_Gurultusu_2", 9502, uzak=0.6)


URETICILER = {"gok_gurultusu": gok_gurultusu}

if __name__ == "__main__":
    # Ad verilirse yalnızca o sesler üretilir; verilmezse hepsi.
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
