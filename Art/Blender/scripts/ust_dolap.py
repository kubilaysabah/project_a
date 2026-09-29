# ust_dolap.py
# Mutfağın duvara asılı üst dolabı, kapakları açık ve içi boş (#83, 1. katın mutfağı; Şahan'ın seçimi 2026-09-29).
# Tezgâhla aynı ceviz desenli laminat ve koyu çıta; iki kapak menteşelerinden 90° açılmış duruyor (aile
# dolabı boşaltırken açık bırakmış). Kapaklar açık hâlde modellendi: 90° açık bir kapak, dolabın yan
# kenarında duvara dik duran bir levhadır, döndürme gerekmez (ortak.kutu tuzağı).
#
# PARÇALAR
#   Ust_Dolap : tek parça (gövde, iki raf, açık kapaklar). Pivot ARKA yüzün (duvarın) alt ortası.
# Ölçü 1,20 (en) x 0,33 (derinlik) x 0,65 (boy). Ön yüz -y; Unity'de +Z.

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

ortak.sahneyi_temizle()
lamine = ortak.malzeme("Tezgah_Lamine", (0.13, 0.06, 0.025), puruz=0.4)     # tezgâhla aynı malzeme adları
cita = ortak.malzeme("Tezgah_Cita", (0.06, 0.028, 0.012), puruz=0.35)
govde_mat = ortak.malzeme("Tezgah_Govde", (0.55, 0.48, 0.38), puruz=0.6)    # sunta iç yüzeyler
pirinc = ortak.malzeme("Tezgah_Pirinc", (0.62, 0.46, 0.22), puruz=0.3, metal=0.85)

EN, DER, BOY = 1.20, 0.33, 0.65
ET = 0.018
yc = -DER / 2
p = []
# Kasa: arka, iki yan, alt ve üst.
p.append(ortak.kutu("Arka", (EN, ET, BOY), (0, -ET / 2, BOY / 2), govde_mat))
for yon in (-1, 1):
    p.append(ortak.kutu("Yan", (ET, DER, BOY), (yon * (EN / 2 - ET / 2), yc, BOY / 2), lamine, 0.002))
p.append(ortak.kutu("Alt", (EN, DER, ET), (0, yc, ET / 2), lamine, 0.002))
p.append(ortak.kutu("Ust", (EN, DER, ET), (0, yc, BOY - ET / 2), lamine, 0.002))
p.append(ortak.kutu("Orta", (ET, DER - ET, BOY - 2 * ET), (0, yc - ET / 2, BOY / 2), govde_mat))
# İki raf (boş).
for z in (BOY * 0.36, BOY * 0.68):
    p.append(ortak.kutu("Raf", (EN - 2 * ET, DER - 0.03, 0.014), (0, yc + 0.005, z), govde_mat))
# Açık kapaklar: her biri kendi yan kenarında, duvara dik, dolabın önünden dışarı uzanır.
kapak_en = EN / 2 - 0.01
for yon in (-1, 1):
    x = yon * (EN / 2 + 0.01)
    p.append(ortak.kutu("Kapak", (0.018, kapak_en, BOY - 0.02), (x, -DER - kapak_en / 2, BOY / 2), lamine, 0.003))
    # Kapak çerçevesi (çıta) ve kulp, kapağın dış yüzünde.
    p.append(ortak.kutu("KapakCita", (0.004, kapak_en - 0.06, 0.012), (x + yon * 0.011, -DER - kapak_en / 2, BOY - 0.06), cita))
    p.append(ortak.kutu("KapakCita", (0.004, kapak_en - 0.06, 0.012), (x + yon * 0.011, -DER - kapak_en / 2, 0.06), cita))
    p.append(ortak.kutu("Kulp", (0.012, 0.012, 0.09), (x + yon * 0.016, -DER - kapak_en + 0.05, BOY / 2), pirinc, 0.003))

dolap = ortak.birlestir(p, "Ust_Dolap")
ortak.yumusat(dolap, 30)
ortak.pivotu_tasi(dolap, (0, 0, 0))
ortak.calistir_ve_bitir("Ust_Dolap", [dolap], 0.65, 2.2)
