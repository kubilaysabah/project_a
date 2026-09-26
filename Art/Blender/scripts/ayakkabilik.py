# ayakkabilik.py
# Her dairenin kapısı yanındaki kendi ayakkabılığı: dar, açık önlü, 5 raflı ahşap raf (#44).
# Her rafa bir çift ayakkabı sığar. (Eskiden bütün dairelerin ortak kullandığı iki kapaklı
# tek bir ayakkabılık vardı; Şahan'ın isteğiyle her daireye kendi rafı geldi.)
#
# Ölçü: 32 cm en, 32 cm derinlik, 102 cm yükseklik. Raf üst yüzleri 5, 24, 43, 62, 81 cm'de;
# bir çift ayakkabı (2 × 10 cm en, 28 cm boy) her rafa rahat sığar.
# BU YÜKSEKLİKLER GrayboxKurucu.RafYukseklikleri ile AYNI olmalı: Unity ayakkabıları bu
# raflara oturtuyor.
#
# Yön: ön yüz (açık taraf) -y tarafında. Pivot: tabanın alt ortası.
#
# ÇALIŞTIRMA:  blender -b --factory-startup --python Art/Blender/scripts/ayakkabilik.py

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, DERIN, BOY = 0.32, 0.32, 1.02
KALIN = 0.018                                  # tahta kalınlığı
RAFLAR = [0.05, 0.24, 0.43, 0.62, 0.81]        # raf ÜST yüzlerinin yüksekliği (m)

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Ayakkabilik_Ahsap", (0.30, 0.17, 0.08), puruz=0.6)     # ceviz kaplama
koyu = ortak.malzeme("Ayakkabilik_Kaide", (0.12, 0.07, 0.04), puruz=0.7)

parcalar = []
# İki yan panel: tam boy.
for yon in (-1, 1):
    parcalar.append(ortak.kutu("Yan", (KALIN, DERIN, BOY), (yon * (EN - KALIN) / 2, 0, BOY / 2), ahsap, 0.002))
# Üst tabla: yanlardan 1 cm taşar.
parcalar.append(ortak.kutu("Tabla", (EN + 0.02, DERIN + 0.01, KALIN), (0, 0, BOY - KALIN / 2), ahsap, 0.003))
# Raflar: iki yan panelin arasında; önü 2 cm geride (ayakkabının burnu değil topuğu görünür).
ic_en = EN - 2 * KALIN
for i, ust in enumerate(RAFLAR):
    parcalar.append(ortak.kutu(f"Raf_{i + 1}", (ic_en, DERIN - 0.03, KALIN), (0, 0.015, ust - KALIN / 2), ahsap, 0.002))
# Arkada iki yatay çıta: raf arkadan açık değil, ama tam kapalı bir kutu da değil.
for z in (0.36, 0.74):
    parcalar.append(ortak.kutu("Cita", (ic_en, 0.012, 0.05), (0, DERIN / 2 - 0.006, z), ahsap))
# Alt kaide: geri çekik, koyu.
parcalar.append(ortak.kutu("Kaide", (EN - 0.03, DERIN - 0.04, 0.03), (0, 0.01, 0.015), koyu))

raf = ortak.tek_parca(parcalar, "Ayakkabilik", 35)
ortak.calistir_ve_bitir("Ayakkabilik", [raf], 1.0, 2.2)
