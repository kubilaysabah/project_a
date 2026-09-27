# klozet.py
# Banyonun alafranga klozeti, alçak rezervuarlı (#73). 90'ların Türk apartmanı: beyaz porselen,
# üstünde plastik kapak. Her dairenin banyosunda kalıcı eşya.
#
# PARÇALAR
#   Klozet : ayak, hazne, rezervuar, rezervuar kapağı, duvara giden boru. Pivot arka yüzün
#            dibi (duvara değen yüzün alt ortası): Unity'de duvarın dibine konur.
#   Kapak  : oturağın üstündeki kapak. Pivot arka kenarındaki menteşe çizgisi: Unity'de
#            yerel X ekseninde döndürülünce yukarı kalkar (AcilirParca, GrayboxKurucu_Daire).
# Ön yüz -y tarafında; FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py).
#
# DİKKAT: silindirler kendi merkezlerinde durur; elips yapmak için ölçekleri meshe işlenir
# (transform_apply). Hiçbir parça döndürülmez (kutu ve silindirin dönüşü dünya merkezine göre
# savrulur, bkz. CLAUDE.md'deki Blender tuzağı).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

HAZNE_UST = 0.40          # oturak yüksekliği (yerden)
HAZNE_Y = -0.40           # haznenin ortası (duvardan uzaklık, -y)
ELIPS = 1.35              # haznenin öne doğru uzunluk oranı

ortak.sahneyi_temizle()
porselen = ortak.malzeme("Klozet_Porselen", (0.88, 0.88, 0.85), puruz=0.12)
plastik = ortak.malzeme("Klozet_Kapak", (0.86, 0.85, 0.80), puruz=0.35)
krom = ortak.malzeme("Klozet_Krom", (0.6, 0.6, 0.6), puruz=0.2, metal=1.0)


def elips(ad, r, boy, konum, mat, oran=ELIPS):
    """y yönünde uzatılmış dik silindir (elips taban)."""
    n = ortak.silindir(ad, r, boy, konum, mat, kose=40)
    n.scale = (1.0, oran, 1.0)
    bpy.ops.object.select_all(action="DESELECT"); n.select_set(True); bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return n


parcalar = []
# Ayak: hazneye doğru daralan porselen gövde.
parcalar.append(elips("Ayak", 0.12, HAZNE_UST - 0.08, (0, HAZNE_Y + 0.05, (HAZNE_UST - 0.08) / 2), porselen, 1.25))
# Hazne: üstü oyulmuş elips kase.
hazne = elips("Hazne", 0.19, 0.12, (0, HAZNE_Y, HAZNE_UST - 0.06), porselen)
ortak.oy(hazne, elips("h_ic", 0.15, 0.12, (0, HAZNE_Y - 0.01, HAZNE_UST - 0.02), porselen, 1.3))
parcalar.append(hazne)
# Arka bağlantı: hazneden rezervuara uzanan gövde.
parcalar.append(ortak.kutu("Boyun", (0.26, 0.16, 0.18), (0, -0.14, HAZNE_UST - 0.09), porselen, 0.02))
# Rezervuar ve kapağı.
parcalar.append(ortak.kutu("Rezervuar", (0.44, 0.17, 0.36), (0, -0.095, HAZNE_UST + 0.18), porselen, 0.02))
parcalar.append(ortak.kutu("RezervuarKapak", (0.46, 0.19, 0.03), (0, -0.095, HAZNE_UST + 0.375), porselen, 0.01))
# Sifon düğmesi ve duvara giden su borusu.
parcalar.append(ortak.silindir("Dugme", 0.02, 0.012, (0, -0.095, HAZNE_UST + 0.395), krom, kose=16))
parcalar.append(ortak.kutu("Boru", (0.02, 0.02, 0.22), (0.17, -0.01, HAZNE_UST + 0.02), krom))
klozet = ortak.birlestir(parcalar, "Klozet")
ortak.yumusat(klozet, 35)
ortak.pivotu_tasi(klozet, (0, 0, 0))

# Kapak: haznenin üstünde ince elips; menteşesi rezervuarın önünde (arka kenar).
kapak = elips("Kapak", 0.195, 0.025, (0, HAZNE_Y - 0.01, HAZNE_UST + 0.0125), plastik)
ortak.yumusat(kapak, 35)
ortak.pivotu_tasi(kapak, (0, HAZNE_Y - 0.01 + 0.195 * ELIPS, HAZNE_UST + 0.0125))

ortak.calistir_ve_bitir("Klozet", [klozet, kapak], 0.8, 1.6)
