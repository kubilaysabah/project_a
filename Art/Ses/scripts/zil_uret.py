# zil_uret.py
# 90'ların apartman kapı zilini ("ding-dong") kodla üretir ve WAV dosyası olarak kaydeder.
#
# NEDEN KODLA
# Eşyaları Blender betikleriyle ürettiğimiz gibi sesleri de mümkün olduğunca kodla
# üretiyoruz: indirme yok, lisans sorusu yok, bir sesi değiştirmek için tek sayıyı
# değiştirip betiği yeniden çalıştırmak yetiyor. Gerçek kayıtlar ses tasarımında (#14)
# gelebilir; o zaman bu dosyanın yerine konur, anomali kartı değişmez.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/zil_uret.py
#
# SES NASIL ÜRETİLİYOR
# Bilgisayarda ses, saniyede 44.100 kez ölçülmüş bir sayı dizisidir (örnekleme).
# Bir çan tonu, belli bir frekansta titreşen ve zamanla sönen bir dalgadır:
#   genlik x sin(2 x pi x frekans x zaman) x sönüm(zaman)
# Gerçek bir çan tek frekansta değil, birkaç "üst ton"la birlikte titreşir; o
# üst tonlar sesi düdük gibi değil çan gibi yapar. "Ding-dong" iki çan tonunun
# arka arkaya çalınmasıdır: önce yüksek (Mi), sonra alçak (Do).

import math, os, struct, wave

ORNEK_HIZI = 44100                     # saniyedeki örnek sayısı (CD kalitesi)
SURE = 2.6                             # toplam süre (saniye)

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Audio", "Anomali", "Zil_DingDong.wav")

# Çanın üst tonları: (frekans çarpanı, göreli genlik). 2,76 gibi tam sayı olmayan
# çarpanlar metal çanlara özgüdür; sesi "elektronik bip" olmaktan çıkarır.
UST_TONLAR = [(1.0, 1.0), (2.0, 0.35), (2.76, 0.22), (5.4, 0.06)]


def can(t, baslangic, frekans, sonum):
    """Verilen anda (t) tek bir çan vuruşunun değeri. Vuruştan önce sessizdir."""
    if t < baslangic:
        return 0.0
    gecen = t - baslangic
    # Vuruşun ilk 4 milisaniyesinde yumuşak giriş: ani başlangıç "tık" sesi çıkarır.
    giris = min(1.0, gecen / 0.004)
    toplam = 0.0
    for carpan, genlik in UST_TONLAR:
        # Yüksek üst tonlar daha hızlı söner (gerçek çanlarda da böyle).
        toplam += genlik * math.sin(2 * math.pi * frekans * carpan * gecen) * math.exp(-gecen * sonum * carpan ** 0.5)
    return giris * toplam


def uret():
    ornekler = []
    for i in range(int(ORNEK_HIZI * SURE)):
        t = i / ORNEK_HIZI
        deger = can(t, 0.00, 659.25, 2.2)       # "ding": Mi (E5)
        deger += can(t, 0.55, 523.25, 1.8)      # "dong": Do (C5), yarım saniye sonra
        ornekler.append(deger)

    # Normalleştirme: en yüksek değeri 0,8'e ölçekle (1'i aşarsa ses cızırdar).
    tepe = max(abs(x) for x in ornekler)
    olcek = 0.8 / tepe

    os.makedirs(os.path.dirname(CIKTI), exist_ok=True)
    with wave.open(CIKTI, "wb") as dosya:
        dosya.setnchannels(1)          # tek kanal: 3B ses için mono gerekir (Unity yönü kendisi hesaplar)
        dosya.setsampwidth(2)          # 16 bit
        dosya.setframerate(ORNEK_HIZI)
        # Her örnek -32768..32767 arası bir tam sayıya çevrilip 2 bayt olarak yazılır.
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in ornekler))

    print("ZIL_OK", CIKTI, round(SURE, 2), "sn")


if __name__ == "__main__":
    uret()
