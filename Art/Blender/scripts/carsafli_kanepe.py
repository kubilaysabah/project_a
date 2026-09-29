# carsafli_kanepe.py
# Üstü beyaz çarşafla örtülmüş kanepe (#83, 1. katın salonu; Şahan'ın seçimi 2026-09-29).
# Taşınan evde eşyanın tozlanmasın diye örtüldüğü çarşaf: kanepenin biçimi (oturak, sırt, iki kol)
# yumuşak hatlarla seçilir, çarşafın etekleri yere kadar iner ve kıvrım yapar. Karanlıkta kafa lambası
# vurunca bir an oturan biri gibi görünsün diye sırtın bir ucunda yuvarlak bir tümsek var (altında
# unutulmuş bir yastık).
#
# PARÇALAR
#   Carsafli_Kanepe : tek parça. Pivot tabanın ortası. Ön yüz -y tarafında; Unity'de +Z.
# Ölçü yaklaşık 1,90 (en) x 0,88 (derinlik) x 0,86 (boy) m.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
carsaf = ortak.malzeme("Carsaf", (0.62, 0.60, 0.55), puruz=0.95)     # kirli beyaz pamuklu çarşaf

EN, DER = 1.90, 0.88
p = []
# Çarşafın altındaki biçim: oturak, sırt, iki kol ve sırtta bir yastık tümseği (kalın pahlı kutular).
p.append(ortak.kutu("Oturak", (EN - 0.30, DER - 0.10, 0.44), (0, 0.02, 0.22), carsaf, 0.08))
p.append(ortak.kutu("Sirt", (EN - 0.10, 0.24, 0.84), (0, DER / 2 - 0.14, 0.42), carsaf, 0.10))
for yon in (-1, 1):
    p.append(ortak.kutu("Kol", (0.20, DER - 0.04, 0.62), (yon * (EN / 2 - 0.12), 0.0, 0.31), carsaf, 0.09))
p.append(ortak.kutu("Yastik", (0.46, 0.20, 0.36), (-0.45, DER / 2 - 0.30, 0.56), carsaf, 0.12))
# Etekler: önde ve yanlarda yere kadar inen çarşaf; kıvrımlar dar kutularla (öne çıkık ve geride sırayla).
KIVRIM = 17
for i in range(KIVRIM):
    x = -EN / 2 + 0.08 + (EN - 0.16) * (i + 0.5) / KIVRIM
    derin = 0.018 if i % 2 == 0 else 0.0
    p.append(ortak.kutu("OnEtek", ((EN - 0.16) / KIVRIM + 0.01, 0.03, 0.30), (x, -DER / 2 + 0.07 - derin, 0.15), carsaf, 0.012))
for yon in (-1, 1):
    for i in range(7):
        yy = -DER / 2 + 0.1 + (DER - 0.2) * (i + 0.5) / 7
        derin = 0.016 if i % 2 == 0 else 0.0
        p.append(ortak.kutu("YanEtek", (0.03, (DER - 0.2) / 7 + 0.01, 0.28), (yon * (EN / 2 - 0.02 + derin), yy, 0.14), carsaf, 0.012))
# Yerde toplanmış çarşaf ucu: önde bir köşede yere yayılmış kısım.
p.append(ortak.kutu("YerdekiUc", (0.40, 0.22, 0.03), (0.62, -DER / 2 - 0.02, 0.015), carsaf, 0.012))

kanepe = ortak.birlestir(p, "Carsafli_Kanepe")
# Alt bölme (subdivision): bütün köşeleri yuvarlatır, çarşafın yumuşaklığını verir.
mod = kanepe.modifiers.new("Yumusat", "SUBSURF")
mod.levels = 2
bpy.context.view_layer.objects.active = kanepe
bpy.ops.object.modifier_apply(modifier=mod.name)
ortak.yumusat(kanepe, 60)
ortak.pivotu_tasi(kanepe, (0, 0, 0))
ortak.calistir_ve_bitir("Carsafli_Kanepe", [kanepe], 0.86, 3.0)
