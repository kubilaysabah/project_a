# kafa_lambasi.py
# Kapıcı odasının duvarındaki çivi ve ona asılı Cemal'in kafa lambası (#56).
#
# HİKÂYE: Yönetimin notu "kafa lambası çivide asılı" diyor. Oyuncu odada uyandığında lamba
# çivide asılı ve hâlâ yanıyor (Cemal onu açık bırakıp gitmiş); karanlık odayı o aydınlatıyor.
# Alınınca oyuncunun kafasına geçer. Lambanın gövdesindeki beyaz bantta sahibinin adı yazar.
#
# PARÇALAR (ikisinin de pivotu çivinin duvara girdiği nokta, yani ikisi Unity'de aynı yere konur)
#   Civi  : duvardan çıkan çivi. Lamba alınınca duvarda kalır.
#   Lamba : lastik kayış halkası (çiviye asılı) + altta sarkan lamba gövdesi, camı, ad bandı.
# Duvar y = 0 düzlemi; eşya -y tarafına çıkar. FBX aktarımında Unity'de +Z'ye bakar (bkz. ortak.py),
# yani Unity'de yüzü duvardan odaya dönük.
# DİKKAT: ortak.kutu/silindir döndürülmez (konumu meshe işlenir, dönüş dünya merkezine göre savrulur).
# Yatık silindirler burada kendi konumlarında, ekleme anında döndürülerek (rotation=) kuruluyor ve
# dönüş hemen meshe işleniyor; bu güvenli.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
demir = ortak.malzeme("Civi_Demir", (0.30, 0.29, 0.27), puruz=0.55, metal=0.9)
lastik = ortak.malzeme("Lamba_Kayis", (0.07, 0.08, 0.10), puruz=0.8)            # eskimiş siyah lastik
plastik = ortak.malzeme("Lamba_Govde", (0.55, 0.12, 0.08), puruz=0.45)          # 90'lar kırmızısı
koyu = ortak.malzeme("Lamba_Cerceve", (0.05, 0.05, 0.05), puruz=0.4)
cam = ortak.malzeme("Lamba_Cam", (0.95, 0.88, 0.70), puruz=0.1)                 # sıcak sarı cam
bant = ortak.malzeme("Lamba_Bant", (0.85, 0.83, 0.76), puruz=0.7)               # kâğıt bant
murekkep = ortak.malzeme("Lamba_Yazi", (0.03, 0.03, 0.04), puruz=0.6)


def yatik_silindir(ad, yaricap, boy, konum, mat, kose=24):
    """Ekseni y boyunca (duvara dik) uzanan silindir: çivi, halka, cam."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=yaricap, depth=boy,
                                        location=konum, rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)   # dönüşü meshe işle; birleştirmede kaybolmasın
    ortak.ata(n, mat)
    return n


# ---- ÇİVİ: 4,5 cm dışarı çıkan gövde ve yassı başı ----
civi = ortak.birlestir([
    yatik_silindir("Govde", 0.0025, 0.045, (0, -0.0225, 0), demir, 12),
    yatik_silindir("Bas", 0.0065, 0.003, (0, -0.0465, 0), demir, 16),
], "Civi")
ortak.pivotu_tasi(civi, (0, 0, 0))

# ---- KAYIŞ: çiviye asılı, ağırlıkla biraz uzamış (dikey oval) lastik halka ----
ic, dis, uzama = 0.078, 0.0815, 1.15
# Halkanın iç üst kenarı çivinin üstüne otursun: merkez = çivi üstü - iç yarıçap × uzama.
merkez_z = 0.0025 - ic * uzama
halka = yatik_silindir("Kayis", dis, 0.024, (0, -0.030, merkez_z), lastik, 48)
oyuk = yatik_silindir("Oyuk", ic, 0.04, (0, -0.030, merkez_z), lastik, 48)
ortak.oy(halka, oyuk)                       # dolu diskin ortası oyulur: geriye halka kalır
halka.scale = (1, 1, uzama)                 # kendi merkezinde dikeyde uzat
bpy.ops.object.select_all(action="DESELECT")
halka.select_set(True)
bpy.context.view_layer.objects.active = halka
bpy.ops.object.transform_apply(scale=True)

# ---- LAMBA GÖVDESİ: halkanın dibinde sarkıyor, camı odaya dönük ----
alt = merkez_z - dis * uzama                # halkanın en alt noktası
govde_z = alt - 0.028
parcalar = [halka,
            ortak.kutu("Govde", (0.08, 0.04, 0.06), (0, -0.030, govde_z), plastik, 0.008),
            ortak.kutu("Tutucu", (0.03, 0.02, 0.012), (0, -0.030, alt - 0.002), koyu, 0.002),
            yatik_silindir("Cerceve", 0.019, 0.006, (0, -0.0525, govde_z + 0.007), koyu, 32),
            yatik_silindir("Cam", 0.015, 0.004, (0, -0.0565, govde_z + 0.007), cam, 32),
            ortak.kutu("Bant", (0.052, 0.002, 0.011), (0, -0.0508, govde_z - 0.021), bant)]

# Bandın üstünde sahibinin adı: Blender yazısı mesh'e çevrilir (Unity'de yazı bileşeni gerekmez).
bpy.ops.object.text_add(location=(0, -0.0521, govde_z - 0.0245), rotation=(math.pi / 2, 0, 0))
yazi = bpy.context.active_object
yazi.data.body = "CEMAL"
yazi.data.size = 0.0085
yazi.data.align_x = "CENTER"
yazi.data.extrude = 0.0003
bpy.ops.object.convert(target="MESH")
bpy.ops.object.transform_apply(rotation=True)
ortak.ata(yazi, murekkep)
parcalar.append(yazi)

lamba = ortak.birlestir(parcalar, "Lamba")
ortak.yumusat(lamba, 35)
ortak.pivotu_tasi(lamba, (0, 0, 0))

ortak.calistir_ve_bitir("Kafa_Lambasi", [civi, lamba], 0.3, 0.55, onizlemede_kaldir=0.3)
