# boy_cetveli.py
# Çocuk odasında kapı kasasına / duvara yapıştırılmış boy cetveli (#53).
# Bulmaca eşyası: 3. katta dolabın şifresi boy cetvelindeki yıllar (Docs/hikaye.md §7). Çocuğun
# her yıl çizilen boy çizgileri ve yanlarındaki yıllar Unity'de YAZI ve çizgi olarak eklenir.
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot arka yüzün alt ortası. Cetvel 60 cm'den başlar;
# Unity'de pivot yerden 0,6 m yükseğe konur, böylece çizgiler gerçek boyları gösterir.
# PARÇALAR
#   Boy_Cetveli : kâğıt şerit, her 10 cm'de uzun, her 5 cm'de kısa çentik, tepede zürafa başı.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, BOY = 0.09, 1.20          # 0,60 m'den 1,80 m'ye
KALIN = 0.002

ortak.sahneyi_temizle()
kagit = ortak.malzeme("Cetvel_Kagit", (0.90, 0.80, 0.45), puruz=0.9)      # sararmış sarı kâğıt
cizgi = ortak.malzeme("Cetvel_Cizgi", (0.08, 0.20, 0.45), puruz=0.8)      # lacivert baskı
zurafa = ortak.malzeme("Cetvel_Zurafa", (0.80, 0.50, 0.15), puruz=0.9)

parcalar = [ortak.kutu("Serit", (EN, KALIN, BOY), (0, -KALIN / 2, BOY / 2), kagit)]
# Çentikler: her 5 cm'de bir, 10 cm'nin katları uzun. Sol kenardan içeri uzanırlar.
for i in range(1, int(BOY / 0.05)):
    z = i * 0.05
    uzun = i % 2 == 0
    boy = 0.05 if uzun else 0.025
    parcalar.append(ortak.kutu(f"Centik{i}", (boy, 0.001, 0.003), (-EN / 2 + boy / 2, -KALIN - 0.0005, z), cizgi))
# Tepede basit bir zürafa başı: çocuk odası cetvellerinin klasik süsü.
parcalar.append(ortak.kutu("Bas", (0.07, 0.002, 0.05), (0.005, -KALIN - 0.001, BOY + 0.02), zurafa, 0.004))
parcalar.append(ortak.kutu("Boyun", (0.03, 0.002, 0.06), (0.015, -KALIN - 0.001, BOY - 0.005), zurafa))
cetvel = ortak.birlestir(parcalar, "Boy_Cetveli")
ortak.pivotu_tasi(cetvel, (0, 0, 0))

ortak.calistir_ve_bitir("Boy_Cetveli", [cetvel], BOY, 1.6, onizlemede_kaldir=0.02)
