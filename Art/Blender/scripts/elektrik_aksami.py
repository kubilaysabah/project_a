# elektrik_aksami.py
# 1990'ların krem plastik elektrik aksamı (#87): duvar anahtarı ve topraklı priz.
# İkisi de Viko/Makel tipi, sararmış krem plastik; kat dairelerinin duvarlarına asılır.
#
#   blender -b --factory-startup --python Art/Blender/scripts/elektrik_aksami.py -- Dugme
#   blender -b --factory-startup --python Art/Blender/scripts/elektrik_aksami.py -- Priz
#
# PARÇALAR (her çağrı tek eşya üretir, "--" sonrası adı seçer)
#   Dugme : 8,2 x 8,2 cm, 1 cm kalın kare plaka; ortasında bir yarısı dışarıda duran eğik bir
#           basmalı (rocker) tuş; üstte ve altta iki küçük vida çukuru.
#   Priz  : aynı plaka ama 1,2 cm kalın; ortasında Ø 5,8 cm, 1,2 cm derin yuvarlak çukur (Schuko),
#           çukurun dibinde iki pim deliği, çukurun içinde üstte ve altta iki ince toprak klipsi.
#
# EKSENLER: Ön yüz -y tarafında (Unity'de +Z). Pivot ARKA düzlemin (duvar yüzünün) alt ortası:
# y = 0 duvar, z = 0 eşyanın alt kenarı. Unity'de duvar yüzüne 0 ofsetle, istenen yükseklikte konur.
#
# TUZAK HATIRLATMASI: ortak.kutu'yu kurduktan SONRA döndürme (konum meshe işlenir, pivot dünya
# merkezinde kalır, dönüş parçayı savurur). Eğik tuş için küp eklenirken döndürülür (aşağıdaki
# egik_kutu), ölçek de aynı anda meshe işlenir.

import os, sys, math
sys.path.append(os.path.dirname(__file__))   # ortak.py bu klasörde; Python onu bulabilsin
import ortak
import bpy

# "--" işaretinden sonraki kelime hangi eşyanın kurulacağını söyler (varsayılan Dugme).
hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Dugme"
ortak.sahneyi_temizle()
p = []                                       # birleştirilecek parçalar bu listede toplanır

# ---------- MALZEMELER ----------
# Blender renkleri DOĞRUSAL (linear) değerdir; Unity ekranda çok daha açık gösterir.
# Bu yüzden "krem" için 0,55 gibi düşük sayılar yazılır, ekranda açık krem çıkar.
krem = ortak.malzeme("Elektrik_Krem", (0.55, 0.50, 0.40), puruz=0.45)        # sararmış krem plastik
kirli = ortak.malzeme("Elektrik_Kir", (0.40, 0.355, 0.27), puruz=0.6)        # kenarlardaki kir, daha koyu krem
tus_krem = ortak.malzeme("Elektrik_Tus", (0.58, 0.52, 0.39), puruz=0.4)      # tuş biraz daha parlak ve sarı
bakalit = ortak.malzeme("Elektrik_Bakalit", (0.05, 0.045, 0.04), puruz=0.5)  # vida ve delik dibi: koyu bakalit
metal = ortak.malzeme("Elektrik_Metal", (0.45, 0.45, 0.43), puruz=0.55, metal=0.9)  # toprak klipsi: mat çelik


# ---------- YARDIMCILAR ----------
def degistiricileri_uygula(n):
    """
    Nesnenin bekleyen değiştiricilerini (pah vb.) hemen meshe işler.
    NEDEN: boolean oyma (ortak.oy) yığındaki İLK değiştirici değilse Blender önceki
    değiştiricileri yok sayarak uygular ve uyarı verir. Pahı önce işleyip sonra oyuyoruz;
    böylece plaka kenarı pahlı kalır, oyuğun kenarı ise keskin ve temiz olur.
    """
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)                            # origin/modifier işlemleri yalnızca seçili nesneye uygulanır
    bpy.context.view_layer.objects.active = n
    for mod in list(n.modifiers):
        bpy.ops.object.modifier_apply(modifier=mod.name)


def egik_kutu(ad, olcu, konum, aci_derece, mat, pah=0.0):
    """
    X ekseni etrafında eğik kutu (tuş için). Küp EKLENİRKEN döndürülür: ortak.kutu'yu
    sonradan döndürme tuzağına düşmeyiz. Dönüş ve ölçek birlikte meshe işlenir.
    Pozitif açı: üst kenar öne (-y) gelir, alt kenar plakaya gömülür = üstü basılmamış tuş.
    """
    bpy.ops.mesh.primitive_cube_add(size=1, location=konum, rotation=(math.radians(aci_derece), 0, 0))
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu                                # ölçek nesnenin KENDİ (dönmüş) eksenlerinde uygulanır
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    ortak.ata(n, mat)
    if pah > 0:
        ortak.pah(n, pah)
    p.append(n)
    return n


def y_silindir(ad, yaricap, boy, konum, mat=None, kose=32):
    """
    Ekseni y boyunca (duvara dik) yatan silindir: oyuk kesicisi, disk, vida başı.
    Dik silindir eklenirken 90° x etrafında döndürülür, dönüş hemen meshe işlenir.
    mat None ise parça listeye EKLENMEZ (kesici olarak kullanılıp atılacak demektir).
    """
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=yaricap, depth=boy, location=konum,
                                        rotation=(math.pi / 2, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    if mat is not None:
        ortak.ata(n, mat)
        p.append(n)
    return n


# ---------- ORTAK ÖLÇÜLER ----------
EN = 0.082                 # plakanın eni ve boyu (kare)
ORTA_Z = EN / 2            # eşyanın dikey ortası (pivot altta olduğu için 4,1 cm)

if hangi == "Dugme":
    # ---------- ANAHTAR ----------
    KAL = 0.010            # plaka kalınlığı
    # Dış plaka: kirli-koyu krem, pahlı kenar. Merkez y = -KAL/2 → arka yüz tam y = 0 (duvar).
    plaka = ortak.kutu("Plaka", (EN, KAL, EN), (0, -KAL / 2, ORTA_Z), kirli, 0.002)
    p.append(plaka)
    # Ön yüz kaplaması: 5 mm içeriden, 1,5 mm kabarık, temiz krem. Dış plakanın kenarı böylece
    # koyu bir "kir çerçevesi" gibi görünür. Pah yok: vida çukurları buraya oyulacak.
    YUZ_KAL = 0.0015
    yuz = ortak.kutu("Yuz", (EN - 0.010, YUZ_KAL, EN - 0.010), (0, -KAL - YUZ_KAL / 2, ORTA_Z), krem)
    ON = -KAL - YUZ_KAL    # ön yüzün y değeri (-0,0115)
    # Vida çukurları: ön yüzden 2 mm oyulur, dibine 0,5 mm çukurda koyu bir vida başı konur.
    for z in (0.0065, EN - 0.0065):
        kesici = y_silindir("Kesici", 0.0027, 0.004, (0, ON + 0.002, z), None, kose=16)
        ortak.oy(yuz, kesici)
        p.append(y_silindir("Vida", 0.0024, 0.002, (0, ON + 0.002, z), bakalit, kose=16))
    p.append(yuz)
    # Tuş (rocker): 4,5 x 6 cm, 6 mm kalın, x etrafında 5° eğik. Merkezi ön yüzden 3 mm önde;
    # eğim üst kenarı ~2,6 mm daha öne çıkarır, alt kenarı plakaya gömer.
    egik_kutu("Tus", (0.045, 0.006, 0.060), (0, ON - 0.003, ORTA_Z), 5.0, tus_krem, 0.0015)
    ad, yukseklik, uzaklik = "Dugme", EN, 0.35

elif hangi == "Priz":
    # ---------- PRİZ ----------
    KAL = 0.012            # plaka kalınlığı
    plaka = ortak.kutu("Plaka", (EN, KAL, EN), (0, -KAL / 2, ORTA_Z), krem, 0.002)
    degistiricileri_uygula(plaka)                 # pah önce işlensin, oyuk sonra açılsın (yukarıdaki açıklama)
    # Yuvarlak bilezik: çukurun çevresinde 3 mm kabarık halka (90'ların prizlerinde çukur ağzı plakadan
    # hafif çıkıntılıdır). Böylece 1,2 cm derin çukur, 1,2 cm kalın plakanın arkasını delmez:
    # çukur dibi bileziğin tepesinden 1,2 cm aşağıda, duvara 3 mm kalır.
    BILEZIK = 0.003
    bilezik = y_silindir("Bilezik", 0.034, BILEZIK, (0, -KAL - BILEZIK / 2, ORTA_Z), kirli, kose=48)
    ON = -KAL - BILEZIK    # bileziğin ön yüzü (-0,015)
    DERIN = 0.012          # çukur derinliği
    DIP = ON + DERIN       # çukur dibinin y değeri (-0,003)
    # Çukur: Ø 5,8 cm silindir hem bilezikten hem plakadan oyulur. Kesici biraz uzun tutulur ki
    # ön yüzle tam çakışan yüzeyler boolean'ı şaşırtmasın.
    for hedef in (bilezik, plaka):
        kesici = y_silindir("Kesici", 0.029, DERIN + 0.01, (0, ON + DERIN / 2 - 0.005, ORTA_Z), None, kose=48)
        ortak.oy(hedef, kesici)
    p.append(plaka)
    # Pim delikleri: Ø 5 mm, Schuko ölçüsü 19 mm aralık; dipten 2,5 mm oyulur, 0,5 mm arka kalır.
    for x in (-0.0095, 0.0095):
        kesici = y_silindir("Kesici", 0.0025, 0.0025, (x, DIP + 0.00125, ORTA_Z), None, kose=16)
        ortak.oy(plaka, kesici)
        # Deliğin dibine koyu bakalit disk: delik içi karanlık okunsun.
        p.append(y_silindir("DelikDibi", 0.0023, 0.001, (x, DIP + 0.002, ORTA_Z), bakalit, kose=16))
    # Toprak klipsleri: çukurun üst ve alt duvarından içeri uzanan ince çelik şeritler, ucunda
    # küçük bir dil. Hafifçe çukur duvarına gömülür (yarıçap 2,9 cm; şerit merkezi 2,75 cm).
    for yon in (+1, -1):
        zc = ORTA_Z + yon * 0.0275
        p.append(ortak.kutu("Klips", (0.008, 0.010, 0.0012), (0, ON + 0.006, zc), metal))
        p.append(ortak.kutu("KlipsDil", (0.008, 0.0015, 0.004), (0, ON + 0.0035, zc - yon * 0.0015), metal))
    ad, yukseklik, uzaklik = "Priz", EN, 0.35

else:
    raise SystemExit("Bilinmeyen eşya: " + hangi + " (Dugme ya da Priz yazın)")

# ---------- BİTİR: tek parça, yumuşat, pivot duvar yüzünün alt ortası, kaydet, FBX ----------
nesne = ortak.birlestir(p, ad)                   # parçalar tek mesh olur; adı tam olarak eşyanın adı
# Boolean oyma, malzemesiz kesiciden hedefe BOŞ bir malzeme yuvası bırakır (hiçbir yüz kullanmaz).
# Unity'ye boş yuva gitmesin: kullanılmayan yuvaları at. Operatör yalnızca seçili+etkin nesneye çalışır.
bpy.ops.object.select_all(action="DESELECT")
nesne.select_set(True)
bpy.context.view_layer.objects.active = nesne
bpy.ops.object.material_slot_remove_unused()
ortak.yumusat(nesne, 35)                         # yuvarlak çukur ve silindirler köşeli görünmesin
ortak.pivotu_tasi(nesne, (0, 0, 0))              # y = 0 duvar, z = 0 alt kenar, x = 0 orta
ortak.calistir_ve_bitir(ad, [nesne], yukseklik, uzaklik)   # OLCU satırı ölçüleri yazdırır
