# tup.py
# 12 kg'lık ev tüpü: 1994 Türk apartmanının kapı önünde görülebilecek en tanıdık eşya.
# Marka, logo veya yazı YOK (başka bir şirketin kimliğini kullanmamak için).
#
# Çalıştırma:
#   blender -b --factory-startup --python Art/Blender/scripts/tup.py
#
# Blender'da yukarı ekseni Z'dir (Unity'de Y). Aşağıdaki bütün konumlar metre;
# tüp dünya merkezinde, tabanı z = 0'da durur. Unity'de de pivotu tabanının
# ortası olur: yere koymak için y = 0 yazmak yeter.

import os
import sys
import math

sys.path.append(os.path.dirname(__file__))   # ortak.py'yi bulabilmek için
import bpy
import ortak

# ---------- ÖLÇÜLER (gerçek 12 kg tüp) ----------
GOVDE_YARICAP = 0.150    # 30 cm çap
TABAN_YUKSEK  = 0.055    # alttaki ayak halkası
GOVDE_YUKSEK  = 0.330    # düz silindir kısmı (toplam boy halkayla ~61 cm)
KUBBE_YUKSEK  = 0.065    # üst ve alttaki yuvarlak omuz
HALKA_YARICAP = 0.100    # üstteki koruma halkası
HALKA_KALIN   = 0.008
HALKA_YUKSEK  = 0.120

ortak.sahneyi_temizle()

# ---------- MALZEMELER ----------
# Renk ilk denemede fazla parlak ve oyuncak gibiydi; kullanılmış boyalı sac daha koyu ve mat.
mavi = ortak.malzeme("Tup_Boya", (0.025, 0.10, 0.36), puruz=0.5)          # boyalı sac
pirinc = ortak.malzeme("Tup_Vana", (0.78, 0.60, 0.25), puruz=0.35, metal=1.0)  # pirinç vana
lastik = ortak.malzeme("Tup_Conta", (0.03, 0.03, 0.03), puruz=0.8)       # siyah conta

parcalar = []   # sonunda birleştirilecek parçaların listesi

# ---------- TABAN HALKASI ----------
# Tüpü yerden kaldıran ayak. Gövdeden biraz dar.
bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=GOVDE_YARICAP * 0.88,
                                    depth=TABAN_YUKSEK, location=(0, 0, TABAN_YUKSEK / 2))
taban = bpy.context.active_object
ortak.ata(taban, mavi)
ortak.pah(taban, 0.004)
parcalar.append(taban)

# ---------- GÖVDE ----------
alt = TABAN_YUKSEK + KUBBE_YUKSEK            # düz kısmın başladığı kot
bpy.ops.mesh.primitive_cylinder_add(vertices=64, radius=GOVDE_YARICAP,
                                    depth=GOVDE_YUKSEK, location=(0, 0, alt + GOVDE_YUKSEK / 2))
govde = bpy.context.active_object
ortak.ata(govde, mavi)
parcalar.append(govde)

# Üst ve alt omuz: bir kürenin basıklaştırılmış hali. Küreyi z ekseninde
# ezince (scale) tüpün yuvarlak omzu oluşur. Kürenin yarısı gövdenin içinde kalır, görünmez.
for z in (alt, alt + GOVDE_YUKSEK):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=64, ring_count=24,
                                         radius=GOVDE_YARICAP, location=(0, 0, z))
    kubbe = bpy.context.active_object
    kubbe.scale = (1, 1, KUBBE_YUKSEK / GOVDE_YARICAP)
    bpy.ops.object.transform_apply(scale=True)   # ölçeği meshe işle (FBX'e temiz gitsin)
    ortak.ata(kubbe, mavi)
    parcalar.append(kubbe)

# Kaynak dikişi: tüp iki yarım kabuğun ortadan kaynağıyla yapılır; gövdenin
# ortasında ince, hafif taşan bir halka olarak görünür. Küçük bir ayrıntı ama
# eşyayı "düz bir silindir" olmaktan çıkarıp gerçek bir tüp gibi okutuyor.
# Torus: simit şeklinde bir halka. major = halkanın yarıçapı, minor = kalınlığı.
bpy.ops.mesh.primitive_torus_add(major_radius=GOVDE_YARICAP, minor_radius=0.0035,
                                 major_segments=64, minor_segments=8,
                                 location=(0, 0, alt + GOVDE_YUKSEK / 2))
dikis = bpy.context.active_object
ortak.ata(dikis, mavi)
parcalar.append(dikis)

ust_kot = alt + GOVDE_YUKSEK + KUBBE_YUKSEK   # omzun en tepesi

# ---------- KORUMA HALKASI ----------
# Vanayı korur, aynı zamanda tüpün taşındığı tutamaktır. İçi boş bir boru;
# iki yanında tutmak için birer oval delik var.
halka_z = ust_kot - 0.02 + HALKA_YUKSEK / 2
bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=HALKA_YARICAP,
                                    depth=HALKA_YUKSEK, location=(0, 0, halka_z))
halka = bpy.context.active_object
ortak.ata(halka, mavi)

# BOOLEAN: bir şekli başka bir şekille "oymak". İçi boşaltmak için daha ince
# bir silindiri, delikler için bir kutuyu halkadan çıkarıyoruz.
def oy(hedef, kesici):
    mod = hedef.modifiers.new("Oy", "BOOLEAN")
    mod.operation = "DIFFERENCE"   # kesicinin kapladığı yeri hedeften sil
    mod.object = kesici
    bpy.context.view_layer.objects.active = hedef
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(kesici, do_unlink=True)   # kesici artık gereksiz

bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=HALKA_YARICAP - HALKA_KALIN,
                                    depth=HALKA_YUKSEK * 2, location=(0, 0, halka_z))
oy(halka, bpy.context.active_object)

# Tutamak delikleri: x ekseni boyunca uzanan bir kutu, halkanın iki yanını birden deler.
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, halka_z + 0.02))
delik = bpy.context.active_object
delik.scale = (HALKA_YARICAP * 3, 0.085, 0.035)
bpy.ops.object.transform_apply(scale=True)
oy(halka, delik)

ortak.pah(halka, 0.002)
parcalar.append(halka)

# ---------- VANA ----------
# Omzun tepesinde, halkanın ortasında pirinç bir gövde ve altıgen başlık.
bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.020, depth=0.045,
                                    location=(0, 0, ust_kot + 0.02))
vana = bpy.context.active_object
ortak.ata(vana, pirinc)
parcalar.append(vana)

bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.024, depth=0.018,   # 6 köşe = altıgen
                                    location=(0, 0, ust_kot + 0.05))
baslik = bpy.context.active_object
ortak.ata(baslik, pirinc)
ortak.pah(baslik, 0.0015)
parcalar.append(baslik)

# Vana ağzındaki siyah conta.
bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.012, depth=0.006,
                                    location=(0, 0, ust_kot + 0.062))
conta = bpy.context.active_object
ortak.ata(conta, lastik)
parcalar.append(conta)

# ---------- BİRLEŞTİR ----------
tup = ortak.birlestir(parcalar, "Tup")
ortak.yumusat(tup, 40)

# Pivot (nesnenin kendi merkezi) tabanın ortası olsun.
ortak.pivotu_dunya_merkezine_al(tup)

ortak.kaydet_ve_disari_aktar("Tup", [tup])

# Kontrol görüntüsü (depoya girmez). Yol komut satırından verilirse oraya yazılır.
if "--onizleme" in sys.argv:
    ortak.onizleme_al(sys.argv[sys.argv.index("--onizleme") + 1], ust_kot + 0.07, 1.1)

print("TUP_OK", round(tup.dimensions.x, 3), round(tup.dimensions.y, 3), round(tup.dimensions.z, 3))
