# golge.py
# Kovalayan siluet (#58, 7. katın kaçış anı): uzun boylu, ince, kapkara bir insan biçimi; iki
# küçük kırmızı göz (Kerem'in çizimindeki "dokuzuncu kattaki adam" gibi). Kat anahtarı alınınca
# anahtarın alındığı yerden çıkar ve oyuncunun yürüdüğü yolu izler (KacisAni).
#
# #90: TEK PARÇA YERİNE ALTI PARÇA
# Eskiden bütün figür tek bir mesh'ti; tek mesh'in kolu bacağı oynatılamaz. Şimdi her uzuv
# AYRI bir nesne ve her nesnenin pivotu (dönme noktası) kendi EKLEMİNDE duruyor: kolun pivotu
# omuzda, bacağın pivotu kalçada, başın pivotu boyunda. Unity'de bir parçayı döndürünce
# eklemden döner, tıpkı sayaç kapağının menteşesinden açılması gibi. Siluet, boy, renk ve
# gözler eskisiyle aynı; yalnızca eklem yerlerine küçük küreler eklendi ki uzuv ±30° dönünce
# arada boşluk görünmesin (gerçek bir omuz ve kalça da yuvarlaktır).
#
# PARÇALAR (hepsi FBX'te en üst düzeyde, dünya konumlarıyla; buzdolabının Govde + Kapi'si gibi)
#   Govde     : kalça + gövde + boyun kökü. Pivot ayakların arası (0, 0, 0): eski yerleştirme
#               kodu değişmeden çalışsın.
#   Bas       : boyun üstü + baş küresi + iki kırmızı göz. Pivot boyun eklemi (gövdenin üstü).
#   Kol_Sol, Kol_Sag     : dizin altına sarkan uzun kollar. Pivot omuz.
#   Bacak_Sol, Bacak_Sag : ince uzun bacaklar. Pivot kalça.
# SOL/SAĞ figürün KENDİ sol ve sağıdır: figür Blender'da -y'ye bakar, bu yüzden kendi sağı
# Blender'ın -x tarafı, kendi solu +x tarafıdır. FBX aktarımı x'i aynalar (Blender x -> Unity -x),
# yani Unity'de Kol_Sag +x'te, Kol_Sol -x'te durur; +Z'ye bakan bir figürün sağı Unity'de +x'tir,
# adlar Unity'de de doğru çıkar.
# Ön yüz -y tarafında; Unity'de +Z. (Kutular döndürülmez: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

ortak.sahneyi_temizle()
kara = ortak.malzeme("Golge_Kara", (0.035, 0.032, 0.034), puruz=0.9)   # 0,012 kafa lambasında bile görünmüyordu
goz = ortak.malzeme("Golge_Goz", (0.9, 0.04, 0.03), puruz=0.3)


def kure(ad, yaricap, konum, mat, dilim=12, halka=8):
    """
    Eklem küresi: verilen noktada küçük bir küre. Blender'ın küre komutu pivotu kürenin
    merkezine koyar; parçaya katıldıktan sonra pivot zaten ekleme taşınıyor, sorun değil.
    dilim/halka: kürenin kaç yüzden örüleceği (azı yeter, küreler kapkara ve küçük).
    """
    bpy.ops.mesh.primitive_uv_sphere_add(radius=yaricap, location=konum, segments=dilim, ring_count=halka)
    n = bpy.context.active_object
    n.name = ad
    ortak.ata(n, mat)
    return n


def parca_bitir(parcalar, ad, pivot):
    """
    Bir uzvun parçalarını tek nesnede birleştirir, yumuşatır ve pivotunu EKLEME taşır.
    Bütün uzuvlar aynı üç adımdan geçtiği için tek yerde yazıldı (eşya betiklerindeki
    tek_parca'nın pivotu seçilebilen hâli).
    """
    n = ortak.birlestir(parcalar, ad)
    ortak.yumusat(n, 30)
    ortak.pivotu_tasi(n, pivot)
    return n


# ---------- EKLEM YÜKSEKLİKLERİ ----------
# Bütün ölçüler eski tek parçalı modelle aynı; yalnızca eklem noktaları isim aldı.
KALCA_Z = 1.00     # bacağın gövdeye bağlandığı yükseklik (kalça kutusunun içinde, 0,925-1,175)
OMUZ_Z = 1.64      # kolun gövdeye bağlandığı yükseklik (gövde kutusunun üstü 1,695)
BOYUN_Z = 1.73     # başın gövdeye bağlandığı yükseklik (boyun kutusunun ortası)
BOYUN_Y = -0.05    # boyun, gövde gibi hafifçe önde (figür öne eğik dursun)

# ---------- GÖVDE ----------
# Kalça kutusu, dar omuzlu ve öne kayık gövde kutusu, boynun alt yarısı. Pivot yerde, ortada.
govde_parcalar = [
    ortak.kutu("Kalca", (0.30, 0.16, 0.25), (0, 0, 1.05), kara, 0.03),
    ortak.kutu("Govde", (0.34, 0.18, 0.55), (0, -0.03, 1.42), kara, 0.04),
    # Boynun gövdede kalan kökü: 1,68-1,74 arası (baş parçasının boynuyla 2 cm üst üste biner).
    ortak.kutu("Boyun_Koku", (0.08, 0.08, 0.06), (0, BOYUN_Y, 1.71), kara),
]
govde = parca_bitir(govde_parcalar, "Govde", (0, 0, 0))

# ---------- BAŞ ----------
# Boynun üst yarısı, boyun ekleminde küçük bir küre (baş eğilince boşluk kalmasın), baş küresi
# ve iki göz. Pivot boyun ekleminde: Unity'de Bas'ı x ekseninde döndürmek başı öne/arkaya eğer.
bas_parcalar = [
    ortak.kutu("Boyun_Ustu", (0.08, 0.08, 0.07), (0, BOYUN_Y, 1.755), kara),
    kure("Boyun_Eklemi", 0.05, (0, BOYUN_Y, BOYUN_Z), kara),
    kure("Kafa", 0.12, (0, -0.07, 1.88), kara, 16, 10),
]
# Gözler: başın önünde iki küçük kırmızı nokta. Kendi malzemeleriyle başa katılırlar; Unity'de
# Bas parçası iki malzeme taşır (eskiden bütün figür taşıyordu), gözler aynı yerde ve aynı renkte.
for x in (-0.04, 0.04):
    bas_parcalar.append(kure("Goz", 0.014, (x, -0.18, 1.90), goz, 8, 6))
bas = parca_bitir(bas_parcalar, "Bas", (0, BOYUN_Y, BOYUN_Z))

# ---------- KOLLAR ----------
# Dizlerin altına kadar sarkan, orantısız uzun kollar (eskisiyle aynı kutu: 0,705-1,655).
# Omuzda bir küre: gövdenin kenarına 5 mm girer ve kol sallanınca eklemi örter.
# Figürün sağı Blender -x (bkz. baş yorum).
# #109: kol DİRSEKTEN iki parça. Üst kol (Kol_*) omuzdan dirseğe (1,655-1,18), ön kol (OnKol_*) dirsekten
# ele (1,18-0,705); ön kolun pivotu dirsekte, dirsekte küçük bir küre eklemi örter. Unity'de OnKol, Kol'un
# çocuğudur (GolgeSablonuKur): üst kol dönünce ön kol onunla gider, ön kol ayrıca kendi başına kıvrılır
# (otururken masaya yatar). Dışarıdan görünen toplam boy ve kalınlık eskisiyle aynı.
DIRSEK_Z = 1.18
kollar = {}
on_kollar = {}
for ad, x in (("Kol_Sag", -0.22), ("Kol_Sol", 0.22)):
    parcalar = [
        ortak.kutu("Kol", (0.07, 0.08, OMUZ_Z - DIRSEK_Z + 0.015), (x, -0.02, (OMUZ_Z + DIRSEK_Z) / 2 + 0.0075), kara, 0.02),
        kure("Omuz", 0.055, (x, -0.02, OMUZ_Z), kara),
    ]
    kollar[ad] = parca_bitir(parcalar, ad, (x, -0.02, OMUZ_Z))
    on_parcalar = [
        ortak.kutu("OnKol", (0.065, 0.075, DIRSEK_Z - 0.705), (x, -0.02, (DIRSEK_Z + 0.705) / 2), kara, 0.02),
        kure("Dirsek", 0.045, (x, -0.02, DIRSEK_Z), kara),
    ]
    on_kollar["On" + ad] = parca_bitir(on_parcalar, "On" + ad, (x, -0.02, DIRSEK_Z))

# ---------- BACAKLAR ----------
# İnce ve uzun bacaklar. Eskiden 0,95 m'ydi; şimdi 1,00 m: üst 7,5 cm'i kalça kutusunun İÇİNDE
# kalır (kalça 0,925'ten başlar), dışarıdan görünen boy aynı. Kalça ekleminde bir küre, o da
# kalça kutusunun içinde saklı; yalnızca bacak öne/arkaya savrulunca ortaya çıkar ve boşluğu örter.
bacaklar = {}
for ad, x in (("Bacak_Sag", -0.09), ("Bacak_Sol", 0.09)):
    parcalar = [
        ortak.kutu("Bacak", (0.09, 0.10, KALCA_Z), (x, 0, KALCA_Z / 2), kara, 0.02),
        kure("Kalca_Eklemi", 0.06, (x, 0, KALCA_Z), kara),
    ]
    bacaklar[ad] = parca_bitir(parcalar, ad, (x, 0, KALCA_Z))

# Sekiz parça tek FBX'e (#109: ön kollar eklendi): Unity'de GolgeSablonuKur her birini adıyla alır (GrayboxKurucu.Model
# parçanın FBX içindeki pivot konumunu kendisi ekler, buzdolabı kapısında olduğu gibi).
ortak.calistir_ve_bitir("Golge",
                        [govde, bas, kollar["Kol_Sol"], kollar["Kol_Sag"],
                         on_kollar["OnKol_Sol"], on_kollar["OnKol_Sag"],
                         bacaklar["Bacak_Sol"], bacaklar["Bacak_Sag"]],
                        2.0, 4.0)
