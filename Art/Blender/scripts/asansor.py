# asansor.py
# Arızalı asansör: paslanmaz çelik kasa, iki sürgülü kanat, kat göstergesi ve çağrı düğmesi.
# Ölçüler graybox'la aynı: boşluk 1,10 m x 2,10 m.
#
# Parçalar AYRI nesneler ve isimleri graybox'taki isimlerle aynı (Kasa, Kapi_Sol,
# Kapi_Sag, Gosterge, Cagri_Dugmesi). Anomali kartları bu isimleri hedef alıyor:
# "aralık asansör" (T05) kanatları kaydırıyor. "ARIZALI" notu ve göstergedeki yazı
# Unity tarafında kalıyor (yazılar kartların değiştirebileceği metinler).
#
# Yön: ön yüz -y. Bütün parçaların pivotu BOŞLUĞUN ALT ORTASI (dünya merkezi);
# Unity'de asansörü boşluğun tabanına koymak yeter, parçalar yerine oturur.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

GEN, YUK = 1.10, 2.10          # boşluk
KASA = 0.06                    # kasa genişliği
TASMA = 0.04                   # kasanın duvardan taşması

ortak.sahneyi_temizle()
celik = ortak.malzeme("Asansor_Celik", (0.55, 0.56, 0.58), puruz=0.38, metal=0.9)  # fırçalanmış paslanmaz
koyu = ortak.malzeme("Asansor_Koyu", (0.04, 0.04, 0.045), puruz=0.5)
ekran = ortak.malzeme("Asansor_Ekran", (0.03, 0.01, 0.01), puruz=0.15)           # sönük kırmızı ekran camı
dugme = ortak.malzeme("Asansor_Dugme", (0.75, 0.70, 0.55), puruz=0.3, metal=0.6)

def son(n, ad):
    """Birleştirilmiş parçayı adlandır, yumuşat, pivotu boşluğun alt ortasına al."""
    n.name = ad
    n.data.name = ad
    ortak.yumusat(n, 30)
    ortak.pivotu_dunya_merkezine_al(n)
    return n

# KASA: boşluğu üç yandan saran çerçeve (iki dikme + üst başlık).
kasa = ortak.birlestir([
    ortak.kutu("Sol", (KASA, TASMA, YUK), (-(GEN + KASA) / 2, -TASMA / 2, YUK / 2), celik, 0.004),
    ortak.kutu("Sag", (KASA, TASMA, YUK), ((GEN + KASA) / 2, -TASMA / 2, YUK / 2), celik, 0.004),
    ortak.kutu("Ust", (GEN + 2 * KASA, TASMA, KASA), (0, -TASMA / 2, YUK + KASA / 2), celik, 0.004),
    # Eşik: yerde ince bir çelik şerit.
    ortak.kutu("Esik", (GEN, 0.08, 0.008), (0, -0.02, 0.004), celik, 0.002)], "Kasa")
son(kasa, "Kasa")

# KANATLAR: boşluğun 5 cm içinde; ortada birleşiyor. Her biri ayrı nesne ki kayabilsin.
kanat_gen = GEN / 2 - 0.005
for ad, yon in (("Kapi_Sol", -1), ("Kapi_Sag", 1)):
    k = ortak.kutu(ad, (kanat_gen, 0.04, YUK - 0.01), (yon * (kanat_gen / 2 + 0.0025), 0.03, (YUK - 0.01) / 2), celik, 0.003)
    son(k, ad)

# GÖSTERGE: kapının üstünde, koyu çerçeve içinde ekran camı.
gosterge = ortak.birlestir([
    ortak.kutu("Cerceve", (0.30, 0.03, 0.12), (0, -0.015, YUK + 0.22), celik, 0.003),
    ortak.kutu("Ekran", (0.24, 0.004, 0.07), (0, -0.031, YUK + 0.22), ekran)], "Gosterge")
son(gosterge, "Gosterge")

# ÇAĞRI DÜĞMESİ: oyuncuya göre SAĞDA; plaka ve üstünde yuvarlak düğme.
# DİKKAT: FBX aktarımı modeli Unity'de dikey eksen etrafında 180° çeviriyor
# (Blender'ın -y'si Unity'de +z, -x'i +x oluyor). Oyuncu asansöre bakarken
# sağı Unity'de -x; bu da Blender'da +x demek. İlk sürümde -x'e konmuştu ve
# düğme oyuncunun soluna düşüyordu.
bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.018, depth=0.012,
                                    location=((GEN / 2 + 0.3), -0.036, 1.1), rotation=(1.5708, 0, 0))
d = bpy.context.active_object
ortak.ata(d, dugme)
cagri = ortak.birlestir([
    ortak.kutu("Plaka", (0.10, 0.03, 0.18), ((GEN / 2 + 0.3), -0.015, 1.1), celik, 0.003), d], "Cagri_Dugmesi")
son(cagri, "Cagri_Dugmesi")

parcalar = [kasa] + [bpy.data.objects[a] for a in ("Kapi_Sol", "Kapi_Sag")] + [gosterge, cagri]
ortak.calistir_ve_bitir("Asansor", parcalar, 2.4, 3.2)
