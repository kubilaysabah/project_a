# asansor_ding_uret.py
# Asansörün kata vardığını haber veren tek "ding" sesini kodla üretir (KÂBUS: açılan asansör).
# Asansör arızalı; bu sesi hiç duymaması gereken bir binada duymak anomalinin kendisi.
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/asansor_ding_uret.py
#
# SES NASIL ÜRETİLİYOR
# Kapı zilinden (zil_uret.py) farkı: tek vuruş, daha tiz (La, A5 = 880 Hz) ve
# daha "temiz" bir çan. Asansör çanları elektronik değil küçük metal bir çana
# vurulan tokmaktır; bu yüzden birkaç üst ton ve uzun bir sönüm var.

import math, os, struct, wave

ORNEK_HIZI = 44100
SURE = 2.4

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Audio", "Anomali", "Asansor_Ding.wav")

TEMEL = 880.0                                             # La (A5)
UST_TONLAR = [(1.0, 1.0), (2.0, 0.28), (3.0, 0.08), (4.2, 0.05)]


def uret():
    ornekler = []
    for i in range(int(ORNEK_HIZI * SURE)):
        t = i / ORNEK_HIZI
        giris = min(1.0, t / 0.003)                       # "tık" sesi çıkmasın diye yumuşak giriş
        deger = 0.0
        for carpan, genlik in UST_TONLAR:
            deger += genlik * math.sin(2 * math.pi * TEMEL * carpan * t) * math.exp(-t * 1.6 * carpan ** 0.6)
        ornekler.append(giris * deger)

    tepe = max(abs(x) for x in ornekler)
    olcek = 0.75 / tepe

    os.makedirs(os.path.dirname(CIKTI), exist_ok=True)
    with wave.open(CIKTI, "wb") as dosya:
        dosya.setnchannels(1)
        dosya.setsampwidth(2)
        dosya.setframerate(ORNEK_HIZI)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in ornekler))

    print("DING_OK", CIKTI, SURE, "sn")


if __name__ == "__main__":
    uret()
