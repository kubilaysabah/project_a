# kat_kapisi.py
# Katlar arasındaki kilitli demir parmaklıklı kat kapısı ve katın anahtarı (#52).
#
# Yönetim "güvenlik için" katların arasına demir kapılar taktırmış (hikâye, #50). Kapı,
# çıkış kolunun TEPESİNDE, ara sahanlığın girişinde durur: oyuncu sekiz basamak çıkar,
# dönüşte kilitli parmaklığa dayanır; parmaklıklardan üst kolun karanlığını görür.
# (Kolun dibine koysaydık açılan kanat basamaklara çarpardı; ara sahanlık düz ve 1,4 m
# derin, 1,32 m'lik kanat oraya rahat açılır.)
#
# İki parça, ayrı pivotlar (elektrik sayacının kapağı gibi):
#   Kasa   iki dikme + üst kayıt + alt eşik; pivotu menteşe hattında (kasa hareket etmez)
#   Kanat  menteşe hattı etrafında dönen parmaklıklı kanat; pivotu menteşede
# Unity'de kanat, menteşe hattından (duvar tarafı) merdiven gözüne doğru -X yönünde uzanır.
# Blender → Unity: (x, y, z) → (-x, z, -y). Yani kanat Blender'da +x yönünde uzanır,
# kalınlık y, yükseklik z.
#
# Anahtar ayrı dosya (Anahtar.fbx): eski tip, sap halkası + gövde + dil. Pivot ortada,
# yere yatık durur (z en ince eksen).
#
# ÇALIŞTIRMA:  blender -b --factory-startup --python Art/Blender/scripts/kat_kapisi.py

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

KANAT_EN, KANAT_BOY = 1.30, 2.05         # kanat: merdiven kolu 1,4 m; kasa dikmeleri arasında 1,32
DIKME = 0.05                              # dikme ve kayıt kalınlığı
CUBUK = 0.016                             # parmaklık çubuğu çapı
ARALIK = 0.115                            # çubuk aralığı (bir kafa geçmez, bir el geçer)
DERIN = 0.04                              # kanat çerçevesinin derinliği (y)

ortak.sahneyi_temizle()
demir = ortak.malzeme("KatKapisi_Demir", (0.09, 0.09, 0.10), puruz=0.55, metal=0.8)   # koyu boyalı demir
pas = ortak.malzeme("KatKapisi_Pas", (0.20, 0.10, 0.05), puruz=0.9, metal=0.3)         # eşik ve kilit kutusu: paslı

# ---------- KASA ----------
kasa_parcalar = []
# Menteşe tarafı dikmesi tam pivotta (x = -DIKME/2 .. 0 aralığı pivotun hemen gerisinde).
kasa_parcalar.append(ortak.kutu("Dikme_Mentese", (DIKME, DIKME + 0.01, KANAT_BOY + 0.10), (-DIKME / 2 - 0.005, 0, (KANAT_BOY + 0.10) / 2), demir, 0.003))
kasa_parcalar.append(ortak.kutu("Dikme_Kilit", (DIKME, DIKME + 0.01, KANAT_BOY + 0.10), (KANAT_EN + 0.01 + DIKME / 2, 0, (KANAT_BOY + 0.10) / 2), demir, 0.003))
kasa_parcalar.append(ortak.kutu("Ust_Kayit", (KANAT_EN + 0.02 + 2 * DIKME, DIKME + 0.01, DIKME), ((KANAT_EN + 0.01) / 2 - 0.005, 0, KANAT_BOY + 0.10 - DIKME / 2), demir, 0.003))
kasa_parcalar.append(ortak.kutu("Esik", (KANAT_EN + 0.02, DIKME, 0.02), ((KANAT_EN + 0.01) / 2 - 0.005, 0, 0.01), pas))
kasa = ortak.tek_parca(kasa_parcalar, "Kasa", 35)

# ---------- KANAT ----------
# Çerçeve: dört kenar, içinde dikey çubuklar ve iki yatay kuşak. Menteşe tarafı x = 0'da.
kanat_parcalar = []
alt_bosluk = 0.06                                     # kanat eşiğin 6 cm üstünden başlar
kanat_parcalar.append(ortak.kutu("Kanat_Sol", (0.04, DERIN, KANAT_BOY), (0.02, 0, alt_bosluk + KANAT_BOY / 2), demir, 0.003))
kanat_parcalar.append(ortak.kutu("Kanat_Sag", (0.04, DERIN, KANAT_BOY), (KANAT_EN - 0.02, 0, alt_bosluk + KANAT_BOY / 2), demir, 0.003))
kanat_parcalar.append(ortak.kutu("Kanat_Alt", (KANAT_EN, DERIN, 0.06), (KANAT_EN / 2, 0, alt_bosluk + 0.03), demir, 0.003))
kanat_parcalar.append(ortak.kutu("Kanat_Ust", (KANAT_EN, DERIN, 0.04), (KANAT_EN / 2, 0, alt_bosluk + KANAT_BOY - 0.02), demir, 0.003))
kanat_parcalar.append(ortak.kutu("Kusak", (KANAT_EN, DERIN * 0.8, 0.035), (KANAT_EN / 2, 0, alt_bosluk + 1.05), demir))
# Dikey çubuklar.
adet = int((KANAT_EN - 0.08) / ARALIK)
for i in range(adet):
    x = 0.04 + ARALIK * (i + 0.5) + (KANAT_EN - 0.08 - adet * ARALIK) / 2
    kanat_parcalar.append(ortak.silindir(f"Cubuk_{i:02d}", CUBUK / 2, KANAT_BOY - 0.10, (x, 0, alt_bosluk + KANAT_BOY / 2), demir, kose=10))
# Kilit kutusu: kilit tarafında, el hizasında; anahtar deliği önde.
kanat_parcalar.append(ortak.kutu("Kilit_Kutusu", (0.12, DERIN + 0.03, 0.16), (KANAT_EN - 0.07, 0, alt_bosluk + 1.0), pas, 0.003))
kanat_parcalar.append(ortak.silindir("Kilit_Deligi", 0.009, DERIN + 0.04, (KANAT_EN - 0.07, 0, alt_bosluk + 0.98), demir, kose=10))
kanat = ortak.birlestir(kanat_parcalar, "Kanat")
ortak.yumusat(kanat, 40)
# Pivot: menteşe hattı (x = 0, y = 0, z = 0), tabanda. Kanat Unity'de bu nokta etrafında döner.
ortak.pivotu_tasi(kanat, (0, 0, 0))

ortak.kaydet_ve_disari_aktar("Kat_Kapisi", [kasa, kanat])
for n in (kasa, kanat):
    print("OLCU", n.name, round(n.dimensions.x, 3), round(n.dimensions.y, 3), round(n.dimensions.z, 3))

# ---------- ANAHTAR ----------
ortak.sahneyi_temizle()
pirinc = ortak.malzeme("Anahtar_Pirinc", (0.62, 0.48, 0.20), puruz=0.35, metal=1.0)
a = []
# Yere yatık: uzunluk y ekseni, kalınlık z. Halka arkada (-y), dil önde (+y).
bpy.ops.mesh.primitive_torus_add(major_radius=0.016, minor_radius=0.004, location=(0, -0.03, 0.004), major_segments=24, minor_segments=8)
halka = bpy.context.active_object
ortak.ata(halka, pirinc)
a.append(halka)
a.append(ortak.kutu("Govde", (0.006, 0.055, 0.006), (0, 0.012, 0.004), pirinc, 0.001))
a.append(ortak.kutu("Dil", (0.014, 0.012, 0.006), (0.006, 0.033, 0.004), pirinc, 0.001))
a.append(ortak.kutu("Dis", (0.006, 0.006, 0.006), (0.012, 0.024, 0.004), pirinc, 0.001))
anahtar = ortak.tek_parca(a, "Anahtar", 40)
ortak.kaydet_ve_disari_aktar("Anahtar", [anahtar])
print("OLCU", anahtar.name, round(anahtar.dimensions.x, 3), round(anahtar.dimensions.y, 3), round(anahtar.dimensions.z, 3))
