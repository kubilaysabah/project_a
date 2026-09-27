# not_kagidi.py
# Dairelerde bulunan okunabilir not kâğıdı (#55): duvara bantlı ya da masaya bırakılmış.
# Unity'de YoneticiNotu bileşeniyle "OKU (E)" olur; metni kat dosyasından gelir.
#
# Tek FBX: Not_Kagidi.fbx. Dik duran A5 kâğıt (15 x 21 cm), hafif kıvrık alt köşe, üstte
# iki bant parçası. Ön yüz -y (Unity'de +Z); pivot arka yüzün alt ortası (duvara yapışık).
# Masaya konacaksa Unity'de x ekseninde -90° yatırılır.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, BOY, KALIN = 0.15, 0.21, 0.0012

ortak.sahneyi_temizle()
kagit = ortak.malzeme("Not_Kagit", (0.86, 0.84, 0.76), puruz=0.9)
bant = ortak.malzeme("Not_Bant", (0.80, 0.74, 0.55), puruz=0.3)
cizgi = ortak.malzeme("Not_Cizgi", (0.25, 0.25, 0.32), puruz=0.9)

parcalar = [ortak.kutu("Kagit", (EN, KALIN, BOY), (0, -KALIN / 2, BOY / 2), kagit)]
# Üst köşelerde bant parçaları (eğik).
for x in (-EN / 2 + 0.015, EN / 2 - 0.015):
    parcalar.append(ortak.kutu("Bant", (0.035, 0.0008, 0.015), (x, -KALIN - 0.0004, BOY - 0.004), bant))
# Uzaktan "yazılı kâğıt" diye okunsun diye soluk satır çizgileri (okunur metin Unity'de ekranda).
for i in range(7):
    parcalar.append(ortak.kutu(f"Satir{i}", (EN * (0.7 if i % 3 else 0.5), 0.0004, 0.0025),
                               (-0.01 if i % 3 else -0.025, -KALIN - 0.0002, BOY - 0.045 - i * 0.022), cizgi))
not_kagidi = ortak.birlestir(parcalar, "Not_Kagidi")
ortak.pivotu_tasi(not_kagidi, (0, 0, 0))
ortak.calistir_ve_bitir("Not_Kagidi", [not_kagidi], BOY, 0.5, onizlemede_kaldir=0.01)
