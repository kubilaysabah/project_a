# lavabo.py
# Banyonun duvara asılı lavabosu (#65, 5. kat: mekanizma).
# Bulmaca eşyası: 5. katta lavabo damlatıyor; ana vana kapatılınca sifon sökülür, katın
# anahtarı sifonun içinden çıkar (Docs/hikaye.md §7).
#
# PARÇALAR
#   Lavabo : porselen tekne, musluk, duvara giden gider borusu. Pivot duvarın dibi (arka yüz,
#            zemin hizası, orta): Unity'de duvarın dibine konur, yükseklikler gerçek.
#   Sifon  : teknenin altındaki şişe sifon (üstü açık bir kap). Pivotu kendi üst ortası:
#            Unity'de aşağı kaydırılınca "sökülmüş" olur; içindeki anahtar görünür.
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

EN, DERIN = 0.55, 0.44          # tekne
UST = 0.85                      # teknenin üst kenarı (yerden)
TEKNE_BOY = 0.16
SIFON_X, SIFON_Y = 0.0, -0.22   # sifonun yeri (gider deliğinin altı)
SIFON_UST = UST - TEKNE_BOY - 0.02
SIFON_BOY = 0.17

ortak.sahneyi_temizle()
porselen = ortak.malzeme("Lavabo_Porselen", (0.90, 0.90, 0.87), puruz=0.15)
krom = ortak.malzeme("Lavabo_Krom", (0.65, 0.65, 0.65), puruz=0.15, metal=1.0)
plastik = ortak.malzeme("Lavabo_Sifon", (0.82, 0.82, 0.80), puruz=0.4)

# ---------- TEKNE ----------
tekne = ortak.kutu("Tekne", (EN, DERIN, TEKNE_BOY), (0, -DERIN / 2, UST - TEKNE_BOY / 2), porselen, 0.03)
ortak.oy(tekne, ortak.kutu("ic", (EN - 0.08, DERIN - 0.10, TEKNE_BOY), (0, -DERIN / 2 - 0.01, UST - TEKNE_BOY / 2 + 0.03), porselen))
parcalar = [tekne]
# Musluk: arka kenarda dik gövde ve öne uzanan ağız.
parcalar.append(ortak.silindir("MuslukGovde", 0.018, 0.14, (0, -0.05, UST + 0.07), krom, kose=16))
parcalar.append(ortak.kutu("MuslukAgiz", (0.03, 0.12, 0.025), (0, -0.10, UST + 0.13), krom, 0.008))
for x in (-0.07, 0.07):
    parcalar.append(ortak.silindir("Vana", 0.02, 0.04, (x, -0.05, UST + 0.02), krom, kose=12))
# Gider borusu: sifonun üstünden duvara giden yatay boru (sifon sökülünce yerinde kalır).
boru = ortak.silindir("Boru", 0.016, 0.22, (0, SIFON_Y / 2, SIFON_UST - 0.03), krom, kose=12)
boru.rotation_euler = (math.radians(90), 0, 0)
bpy.ops.object.select_all(action="DESELECT"); boru.select_set(True); bpy.context.view_layer.objects.active = boru
bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)   # dönüşü meshe işle
parcalar.append(boru)
parcalar.append(ortak.silindir("Gider", 0.018, 0.05, (SIFON_X, SIFON_Y, SIFON_UST + 0.02), krom, kose=12))
lavabo = ortak.birlestir(parcalar, "Lavabo")
ortak.yumusat(lavabo, 35)
ortak.pivotu_tasi(lavabo, (0, 0, 0))

# ---------- SİFON ----------
# Üstü açık bir kap: dış silindirden iç silindir oyulur (tabanı kalır).
sifon = ortak.silindir("Sifon", 0.045, SIFON_BOY, (SIFON_X, SIFON_Y, SIFON_UST - SIFON_BOY / 2), plastik, kose=24)
ortak.oy(sifon, ortak.silindir("s_ic", 0.037, SIFON_BOY, (SIFON_X, SIFON_Y, SIFON_UST - SIFON_BOY / 2 + 0.012), plastik, kose=24))
halka = ortak.silindir("Halka", 0.05, 0.02, (SIFON_X, SIFON_Y, SIFON_UST - 0.01), krom, kose=24)
ortak.oy(halka, ortak.silindir("h_ic", 0.037, 0.03, (SIFON_X, SIFON_Y, SIFON_UST - 0.01), krom, kose=24))
sifon = ortak.birlestir([sifon, halka], "Sifon")
ortak.yumusat(sifon, 35)
ortak.pivotu_tasi(sifon, (SIFON_X, SIFON_Y, SIFON_UST))

print("SIFON_UST", SIFON_UST, "SIFON_Y", SIFON_Y)
ortak.calistir_ve_bitir("Lavabo", [lavabo, sifon], 1.0, 1.4)
