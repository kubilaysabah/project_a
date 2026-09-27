# radyo.py
# Yaşlı Nuri'nin masa radyosu (#62, 2. kat): 70'lerin ahşap kasalı lambalı radyosu.
# Bulmaca eşyası: 2. katta kendiliğinden açılır; oyuncu sesini takip edip bulur, kilerin
# şifresi kadranındaki istasyondur (Docs/hikaye.md §7). İstasyon yazısı Unity'de eklenir
# (zincirin ipucu metni), modelde yazı yok.
#
# PARÇALAR
#   Radyo  : ahşap kasa, hoparlör bezi, iki düğme, ayaklar. Pivot tabanın ortası.
#   Kadran : istasyon camı (ayrı parça: radyo açılınca Unity'de ışıklı malzemeye geçer).
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.34, 0.14, 0.20      # metre
AYAK = 0.012                           # ayak yüksekliği

ortak.sahneyi_temizle()
ahsap = ortak.malzeme("Radyo_Ahsap", (0.30, 0.16, 0.07), puruz=0.35)       # cilalı ceviz
bez = ortak.malzeme("Radyo_Bez", (0.55, 0.47, 0.34), puruz=0.95)           # bej hoparlör bezi
cita = ortak.malzeme("Radyo_Cita", (0.62, 0.55, 0.40), puruz=0.3, metal=0.7)
dugme_m = ortak.malzeme("Radyo_Dugme", (0.08, 0.07, 0.06), puruz=0.4)
kadran_m = ortak.malzeme("Radyo_Kadran", (0.80, 0.72, 0.52), puruz=0.25)   # sarımsı cam

# ---------- KASA ----------
kasa = ortak.kutu("Kasa", (EN, DERIN, BOY - AYAK), (0, 0, AYAK + (BOY - AYAK) / 2), ahsap, 0.008)
# Ön yüz 1 cm içeri oyuk: bez ve kadran bu çerçevenin içine oturur (çerçeve ahşap kalır).
on_y = -DERIN / 2
ortak.oy(kasa, ortak.kutu("oyuk", (EN - 0.03, 0.02, BOY - AYAK - 0.03), (0, on_y, AYAK + (BOY - AYAK) / 2), ahsap))
parcalar = [kasa]
# Hoparlör bezi: sol yarı (Blender'da -x; Unity'de x işareti döner, önden bakınca yine solda).
bez_en = 0.17
parcalar.append(ortak.kutu("Bez", (bez_en, 0.004, BOY - AYAK - 0.03), (-EN / 2 + 0.015 + bez_en / 2, on_y + 0.008, AYAK + (BOY - AYAK) / 2), bez))
# Bezin önünde üç ince yatay pirinç çıta.
for i in range(3):
    z = AYAK + 0.05 + i * 0.045
    parcalar.append(ortak.kutu(f"Cita{i}", (bez_en, 0.003, 0.004), (-EN / 2 + 0.015 + bez_en / 2, on_y + 0.005, z), cita))
# Sağ yarının arka paneli (kadranın ve düğmelerin arkasındaki ahşap yüz).
sag_en = EN - 0.03 - bez_en
sag_x = EN / 2 - 0.015 - sag_en / 2
parcalar.append(ortak.kutu("Panel", (sag_en, 0.004, BOY - AYAK - 0.03), (sag_x, on_y + 0.008, AYAK + (BOY - AYAK) / 2), ahsap))
# İki düğme: kadranın altında (ses ve istasyon). Silindir z'de dik durur; önden görünsün diye
# x ekseninde 90° çevrilir.
import math
for i, x in enumerate((sag_x - 0.035, sag_x + 0.035)):
    d = ortak.silindir(f"Dugme{i}", 0.014, 0.018, (x, on_y - 0.003, AYAK + 0.045), dugme_m, kose=24)
    d.rotation_euler = (math.radians(90), 0, 0)
    parcalar.append(d)
# Ayaklar: dört küçük kare ayak.
for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
    for y in (-DERIN / 2 + 0.025, DERIN / 2 - 0.025):
        parcalar.append(ortak.kutu("Ayak", (0.02, 0.02, AYAK), (x, y, AYAK / 2), dugme_m))
radyo = ortak.birlestir(parcalar, "Radyo")
ortak.yumusat(radyo, 30)
ortak.pivotu_tasi(radyo, (0, 0, 0))

# ---------- KADRAN ----------
# İstasyon camı: sağ yarının üstünde, panelin 2 mm önünde. Pivotu radyonunkiyle aynı nokta
# (tabanın ortası): Unity'de radyonun çocuğu olarak yerinde durur.
kadran = ortak.kutu("Kadran", (sag_en - 0.02, 0.003, 0.055), (sag_x, on_y + 0.004, AYAK + 0.125), kadran_m)
ortak.pivotu_tasi(kadran, (0, 0, 0))

print("KADRAN_MERKEZ", round(sag_x, 4), round(on_y + 0.0025, 4), round(AYAK + 0.125, 4))
ortak.calistir_ve_bitir("Radyo", [radyo, kadran], BOY, 0.7)
