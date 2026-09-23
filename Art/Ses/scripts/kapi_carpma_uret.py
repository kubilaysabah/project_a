# kapi_carpma_uret.py
# Bir daire kapısının kendi kendine ÇARPARAK kapanma sesini kodla üretir (anomali: çarpan kapı).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/kapi_carpma_uret.py
#
# SES NASIL ÜRETİLİYOR
# Bir kapı çarpması üç katmandır:
#   - GÜM: ağır tahta kanadın kasaya çarptığı an; çok alçak (~60-90 Hz), güçlü, kısa.
#   - TAK: kanadın kenarının kasaya değdiği sert tıkırtı; kısa bir gürültü patlaması.
#   - TİTREŞİM: kasanın, kilidin ve duvarın ardından gelen kısa, düzensiz sarsıntısı.
# Kapıya vurmaktan (kapi_tiklatma_uret.py) farkı: tek ve çok daha güçlü bir darbe,
# ve ses kapının bu tarafında, sahanlıkta duyulduğu için boğuk değil.

import math, os, random, struct, wave

ORNEK_HIZI = 44100
SURE = 1.2
random.seed(11)          # gürültü rastgele ama her çalıştırmada aynı ses çıksın

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Audio", "Anomali", "Kapi_Carpma.wav")


def uret():
    ham = []
    for i in range(int(ORNEK_HIZI * SURE)):
        t = i / ORNEK_HIZI
        gum = math.sin(2 * math.pi * 68 * t) * math.exp(-t * 14)                # derin gümleme
        gum += 0.6 * math.sin(2 * math.pi * 97 * t) * math.exp(-t * 20)
        tak = random.uniform(-1, 1) * math.exp(-t * 90)                         # sert tıkırtı
        # Titreşim: 0,06 sn sonra başlayan, hızla sönen düzensiz sarsıntı.
        titresim = 0.0
        if t > 0.06:
            g = t - 0.06
            titresim = 0.35 * random.uniform(-1, 1) * math.exp(-g * 30) * (0.5 + 0.5 * math.sin(2 * math.pi * 31 * g))
        ham.append(1.0 * gum + 0.7 * tak + titresim)

    # Hafif süzgeç: tıkırtının en tiz kısmını törpüler, ama boğuklaştırmaz (katsayı yüksek).
    suzulmus, onceki = [], 0.0
    for x in ham:
        onceki += 0.45 * (x - onceki)
        suzulmus.append(onceki)

    tepe = max(abs(x) for x in suzulmus)
    olcek = 0.9 / tepe

    os.makedirs(os.path.dirname(CIKTI), exist_ok=True)
    with wave.open(CIKTI, "wb") as dosya:
        dosya.setnchannels(1)          # mono: 3B ses için
        dosya.setsampwidth(2)
        dosya.setframerate(ORNEK_HIZI)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in suzulmus))

    print("CARPMA_OK", CIKTI, SURE, "sn")


if __name__ == "__main__":
    uret()
