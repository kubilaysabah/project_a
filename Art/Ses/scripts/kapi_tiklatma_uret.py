# kapi_tiklatma_uret.py
# Bir dairenin İÇİNDEN, kapıya vurulan üç tıklatmayı kodla üretir (anomali: kapı tıklatılıyor).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/kapi_tiklatma_uret.py
#
# SES NASIL ÜRETİLİYOR
# Bir kapı vuruşu iki şeyin toplamıdır:
#   - GÜMLEME: tahta kanadın alçak frekansta (~110-150 Hz) kısa süre titreşmesi.
#   - TIKIRTI: yumruğun değdiği an; çok kısa, rastgele bir gürültü patlaması.
# Ses kapının ÖTE tarafından geldiği için tiz kısmı boğuktur: tahta yüksek
# frekansları yutar. Bunu "alçak geçiren süzgeç" ile taklit ediyoruz: her örneği
# bir öncekiyle harmanlayınca ani (tiz) değişimler yumuşar, alçak sesler kalır.
# Vuruşlar arası boşluk bilerek eşit değil: makine gibi değil, bir insan gibi.

import math, os, random, struct, wave

ORNEK_HIZI = 44100
SURE = 1.4
random.seed(3)          # gürültü rastgele ama her çalıştırmada AYNI ses çıksın

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CIKTI = os.path.join(KOK, "Assets", "Audio", "Anomali", "Kapi_Tiklatma.wav")

# Üç vuruşun zamanı (saniye) ve gücü. Sonuncusu biraz daha sert.
VURUSLAR = [(0.00, 0.85), (0.31, 0.8), (0.58, 1.0)]


def vurus(gecen, guc):
    """Tek bir vuruşun, vuruştan 'gecen' saniye sonraki değeri."""
    if gecen < 0:
        return 0.0
    gumleme = math.sin(2 * math.pi * 128 * gecen) * math.exp(-gecen * 38)   # tok, kısa gümleme
    gumleme += 0.5 * math.sin(2 * math.pi * 205 * gecen) * math.exp(-gecen * 55)
    tikirti = random.uniform(-1, 1) * math.exp(-gecen * 260)                # çok kısa gürültü
    return guc * (gumleme + 0.6 * tikirti)


def uret():
    ham = []
    for i in range(int(ORNEK_HIZI * SURE)):
        t = i / ORNEK_HIZI
        ham.append(sum(vurus(t - bas, guc) for bas, guc in VURUSLAR))

    # Alçak geçiren süzgeç: yeni = önceki + katsayı x (ham - önceki).
    # Katsayı küçüldükçe ses boğuklaşır. 0,18 "kapının arkasından" gibi.
    suzulmus, onceki = [], 0.0
    for x in ham:
        onceki += 0.18 * (x - onceki)
        suzulmus.append(onceki)

    tepe = max(abs(x) for x in suzulmus)
    olcek = 0.85 / tepe

    os.makedirs(os.path.dirname(CIKTI), exist_ok=True)
    with wave.open(CIKTI, "wb") as dosya:
        dosya.setnchannels(1)          # mono: 3B ses için
        dosya.setsampwidth(2)
        dosya.setframerate(ORNEK_HIZI)
        dosya.writeframes(b"".join(struct.pack("<h", int(x * olcek * 32767)) for x in suzulmus))

    print("TIKLATMA_OK", CIKTI, SURE, "sn")


if __name__ == "__main__":
    uret()
