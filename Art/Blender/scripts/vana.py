# vana.py
# Dairenin ana su vanası (#65, 5. kat: mekanizma): duvar dibinde dik bir boru, üstünde
# kırmızı el çarklı bir vana. Salonda gardırobun arkasında saklı; çark çevrilince su kesilir.
#
# PARÇALAR
#   Vana : duvara kelepçeli boru ve vana gövdesi. Pivot duvarın dibi (arka yüz, zemin, orta).
#   Cark : kırmızı el çarkı; pivotu çarkın merkezi. Unity'de kendi ön ekseni (+Z) etrafında
#          döndürülür (KolAyari).
# Ön yüz -y tarafında; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy


def cevir(n, rx=0.0, ry=0.0, rz=0.0):
    """Nesneyi döndürür ve dönüşü meshe İŞLER (uygulanmamış dönüş boyutları ve boolean'ı bozar)."""
    n.rotation_euler = (math.radians(rx), math.radians(ry), math.radians(rz))
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    return n

BORU_Y = -0.045          # borunun duvardan uzaklığı (merkez)
VANA_Z = 0.62            # vananın yüksekliği

ortak.sahneyi_temizle()
demir = ortak.malzeme("Vana_Demir", (0.35, 0.33, 0.30), puruz=0.6, metal=0.8)
pirinc = ortak.malzeme("Vana_Pirinc", (0.60, 0.45, 0.20), puruz=0.35, metal=1.0)
kirmizi = ortak.malzeme("Vana_Kirmizi", (0.55, 0.05, 0.04), puruz=0.4, metal=0.3)

parcalar = [ortak.silindir("Boru", 0.022, 1.00, (0, BORU_Y, 0.05 + 0.50), demir, kose=16)]   # yerden 5 cm, 1 m boy
for z in (0.25, 0.95):
    parcalar.append(ortak.kutu("Kelepce", (0.06, 0.05, 0.02), (0, BORU_Y + 0.02, z), demir))
parcalar.append(ortak.kutu("Govde", (0.07, 0.07, 0.10), (0, BORU_Y, VANA_Z), pirinc, 0.01))
mil = ortak.silindir("Mil", 0.008, 0.07, (0, BORU_Y - 0.06, VANA_Z), pirinc, kose=10)
cevir(mil, 90)
parcalar.append(mil)
vana = ortak.birlestir(parcalar, "Vana")
ortak.yumusat(vana, 35)
ortak.pivotu_tasi(vana, (0, 0, 0))

# Çark: halka + dört kol + göbek, ön yüzü -y'ye bakar (silindir x ekseninde 90° çevrilir).
cy = BORU_Y - 0.10
halka = ortak.silindir("Halka", 0.06, 0.012, (0, cy, VANA_Z), kirmizi, kose=28)
cevir(halka, 90)
ic = ortak.silindir("h_ic", 0.048, 0.03, (0, cy, VANA_Z), kirmizi, kose=28)
cevir(ic, 90)
ortak.oy(halka, ic)
cark = [halka]
# İki kol: biri yatay, biri dik. Dik olan döndürülerek değil, ölçüleri dik verilerek yapılır:
# ortak.kutu konumu meshe işler ve nesnenin pivotunu dünya merkezinde bırakır; döndürülseydi
# dünya merkezi etrafında döner, çarktan 60 cm uzağa savrulurdu (ilk denemede oldu).
for olcu in ((0.10, 0.010, 0.012), (0.012, 0.010, 0.10)):
    cark.append(ortak.kutu("Kol", olcu, (0, cy, VANA_Z), kirmizi))
gobek = ortak.silindir("Gobek", 0.014, 0.02, (0, cy, VANA_Z), pirinc, kose=12)
cevir(gobek, 90)
cark.append(gobek)
cark = ortak.birlestir(cark, "Cark")
ortak.yumusat(cark, 35)
ortak.pivotu_tasi(cark, (0, cy, VANA_Z))

print("CARK_MERKEZ", cy, VANA_Z)
ortak.calistir_ve_bitir("Vana", [vana, cark], 1.0, 1.2)
