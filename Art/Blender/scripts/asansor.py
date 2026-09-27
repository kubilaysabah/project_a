# asansor.py
# Arızalı asansör, 90'ların Türk apartmanı gibi (#77, Şahan: "çok modern kalıyor").
# Krem fırın boyalı sac kanatlar, her kanatta dar dikey bir gözetleme camı ve altta tekme sacı;
# koyu kahve, kademeli (kabartma) kasa; alüminyum plaka üstünde siyah bakalit çağrı düğmesi ve
# kırmızı "geliyor" lambası; kapının üstünde ok lambalı eski tip kat göstergesi.
# Ölçüler graybox'la aynı: boşluk 1,10 m x 2,10 m.
#
# Parçalar AYRI nesneler ve isimleri graybox'taki isimlerle aynı (Kasa, Kapi_Sol, Kapi_Sag,
# Gosterge, Cagri_Dugmesi). Asansör panosu (#68) kanatları kaydırır, 8. kat zinciri ve kartlar bu
# isimleri kullanır. "ARIZALI" notu ve göstergedeki rakam Unity tarafında kalıyor (yazılar).
#
# Yön: ön yüz -y. Bütün parçaların pivotu BOŞLUĞUN ALT ORTASI (dünya merkezi); Unity'de
# asansörü boşluğun tabanına koymak yeter, parçalar yerine oturur.
# Kasa en fazla 10 cm: sağındaki sigorta panosu kasanın 16 cm dışında başlıyor.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

GEN, YUK = 1.10, 2.10          # boşluk

ortak.sahneyi_temizle()
boya = ortak.malzeme("Asansor_Boya", (0.60, 0.53, 0.40), puruz=0.55, metal=0.25)       # krem fırın boyası
kasa_boya = ortak.malzeme("Asansor_Kasa", (0.20, 0.13, 0.08), puruz=0.5, metal=0.2)    # koyu kahve
cam = ortak.malzeme("Asansor_Cam", (0.015, 0.018, 0.02), puruz=0.05)                   # kuyunun karanlığını gösteren cam
tekme = ortak.malzeme("Asansor_Tekme", (0.45, 0.45, 0.44), puruz=0.45, metal=0.9)      # çizik sac
aluminyum = ortak.malzeme("Asansor_Aluminyum", (0.62, 0.62, 0.60), puruz=0.4, metal=0.85)
bakalit = ortak.malzeme("Asansor_Bakalit", (0.02, 0.02, 0.02), puruz=0.3)
lamba = ortak.malzeme("Asansor_Lamba", (0.22, 0.03, 0.02), puruz=0.2)                  # sönük kırmızı cam
ekran = ortak.malzeme("Asansor_Ekran", (0.03, 0.01, 0.01), puruz=0.15)


def son(n, ad):
    """Birleştirilmiş parçayı adlandır, yumuşat, pivotu boşluğun alt ortasına al."""
    n.name = ad
    n.data.name = ad
    ortak.yumusat(n, 30)
    ortak.pivotu_dunya_merkezine_al(n)
    return n


# ---------- KASA ----------
# İki kademe: dıştaki geniş, duvardan az taşan bant; içteki dar, daha çok taşan bant. Işık iki
# kademenin arasındaki basamağa vurunca kasa "kabartma" görünür.
parcalar = []
for gen, tasma, pah in ((0.10, 0.025, 0.004), (0.05, 0.045, 0.006)):
    ic = GEN / 2 + gen / 2
    parcalar += [
        ortak.kutu("Sol", (gen, tasma, YUK + gen), (-ic, -tasma / 2, (YUK + gen) / 2), kasa_boya, pah),
        ortak.kutu("Sag", (gen, tasma, YUK + gen), (ic, -tasma / 2, (YUK + gen) / 2), kasa_boya, pah),
        ortak.kutu("Ust", (GEN + 2 * gen, tasma, gen), (0, -tasma / 2, YUK + gen / 2), kasa_boya, pah),
    ]
# Eşik: yerde kabartmalı sac şerit.
parcalar.append(ortak.kutu("Esik", (GEN, 0.09, 0.01), (0, -0.025, 0.005), tekme, 0.002))
kasa = son(ortak.birlestir(parcalar, "Kasa"), "Kasa")

# ---------- KANATLAR ----------
# Boşluğun 3 cm içinde; ortada birleşiyor. Her biri ayrı nesne ki kayabilsin. Kanat: sac levha,
# dar dikey cam (içi kuyunun karanlığı), çevresinde ince kasnak, altta çizik tekme sacı.
kanat_gen = GEN / 2 - 0.005
for ad, yon in (("Kapi_Sol", -1), ("Kapi_Sag", 1)):
    x = yon * (kanat_gen / 2 + 0.0025)
    levha = ortak.kutu(ad, (kanat_gen, 0.035, YUK - 0.01), (x, 0.03, (YUK - 0.01) / 2), boya, 0.003)
    # Cam yuvası: levhadan oyulur, içine koyu cam konur (camın kuyuya bakan yüzü de karanlık).
    cam_x = x - yon * 0.12                           # cam, kanadın birleşme çizgisine yakın
    ortak.oy(levha, ortak.kutu("oyuk", (0.09, 0.05, 0.62), (cam_x, 0.03, 1.45), boya))
    parca = [levha,
             ortak.kutu("Cam", (0.09, 0.006, 0.62), (cam_x, 0.03, 1.45), cam),
             ortak.kutu("Kasnak", (0.13, 0.012, 0.66), (cam_x, 0.009, 1.45), kasa_boya, 0.002),
             ortak.kutu("Tekme", (kanat_gen - 0.04, 0.004, 0.22), (x, 0.0105, 0.14), tekme, 0.001)]
    # Kasnağın ortası oyulur: camın önü açık kalsın.
    ortak.oy(parca[2], ortak.kutu("k_oyuk", (0.09, 0.03, 0.62), (cam_x, 0.009, 1.45), kasa_boya))
    son(ortak.birlestir(parca, ad), ad)

# ---------- GÖSTERGE ----------
# Kapının üstünde koyu kutu; ortada rakam ekranı (rakam Unity'de yazılır), iki yanda yukarı ve
# aşağı ok lambaları. Ölçü eski göstergeyle aynı yerde: Unity'deki yazı ekranın önüne düşer.
gosterge = ortak.birlestir([
    ortak.kutu("Kutu", (0.40, 0.04, 0.13), (0, -0.02, YUK + 0.22), kasa_boya, 0.006),
    ortak.kutu("Ekran", (0.14, 0.004, 0.08), (0, -0.041, YUK + 0.22), ekran),
    ortak.kutu("Ok_Yukari", (0.035, 0.006, 0.035), (-0.13, -0.041, YUK + 0.235), lamba, 0.004),
    ortak.kutu("Ok_Asagi", (0.035, 0.006, 0.035), (0.13, -0.041, YUK + 0.205), lamba, 0.004)], "Gosterge")
son(gosterge, "Gosterge")

# ---------- ÇAĞRI DÜĞMESİ ----------
# Oyuncuya göre SAĞDA: alüminyum plaka, üstte küçük kırmızı "geliyor" lambası, altta büyük
# siyah bakalit düğme. DİKKAT: FBX aktarımı modeli Unity'de dikey eksen etrafında 180° çeviriyor;
# oyuncunun sağı Unity'de -x, Blender'da +x (ilk sürümde -x'e konmuştu, sola düşmüştü).
DX = GEN / 2 + 0.3
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.028, depth=0.02, location=(DX, -0.04, 1.06), rotation=(1.5708, 0, 0))
d = bpy.context.active_object; ortak.ata(d, bakalit)
bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.012, depth=0.01, location=(DX, -0.035, 1.16), rotation=(1.5708, 0, 0))
l = bpy.context.active_object; ortak.ata(l, lamba)
cagri = ortak.birlestir([
    ortak.kutu("Plaka", (0.11, 0.03, 0.22), (DX, -0.015, 1.1), aluminyum, 0.004), d, l], "Cagri_Dugmesi")
son(cagri, "Cagri_Dugmesi")

parcalar = [kasa] + [bpy.data.objects[a] for a in ("Kapi_Sol", "Kapi_Sag")] + [gosterge, cagri]
ortak.calistir_ve_bitir("Asansor", parcalar, 2.4, 3.2)
