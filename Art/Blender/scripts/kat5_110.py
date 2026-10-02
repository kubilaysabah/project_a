# kat5_110.py
# 5. katın dönem düzeltmesi (#110). Tam testte iki eşya 1994 Türkiye'sine uymadı:
#   - mutfakta ocağın üstündeki ELEKTRİKLİ SU ISITICISI (Poly Haven "vintage_electric_kettle"): 2000'lerin eşyası.
#     Yerine 90'ların her evindeki ÇİFT KATLI ÇAYDANLIK: altta su kazanı, üstte demlik.
#   - yatak odasındaki "GOTİK" KARYOLA (Poly Haven "GothicBed_01"): oymalı, koyu, Türk evine yabancı. Yerine sade
#     ceviz karyola, açık renk (krem) yatak örtüsü ve iki beyaz yastık: kapıdan bakınca odanın ne olduğu okunur
#     (tam testin "odak eşyası" bulgusu).
#
#   Caydanlik_K5 : paslanmaz çelik çaydanlık, demliği üstünde. Pivot tabanın ortası (ocağın üstüne konur).
#   Karyola_K5   : 1,45 x 2,02 m, gotik karyolanın tabanıyla aynı (1,49 x 2,04): levyenin saklandığı yer (karyolanın
#                  güney kenarının dibi) bozulmasın. ALTI AÇIK (22 cm), örtü yanlardan SARKMAZ: eğilen oyuncu altını
#                  görebilsin (#102 bulunabilirlik). Baş tarafı Blender +y (Unity -Z), ayak ucu -y (Unity +Z, "ön").
#                  Pivot tabanın ortası.
# Çalıştırma:  blender -b --factory-startup --python kat5_110.py -- Caydanlik_K5
import math, os, sys
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Caydanlik_K5"
ortak.sahneyi_temizle()
# Blender malzeme renkleri doğrusal değerdir; Unity açık gösterir (CLAUDE.md): koyular koyu seçildi. Krem örtü
# açık seçildi ama içe aktarmadaki eskitme (EsyaEskitici) onu biraz sarartıp koyulaştırır.
# Tam metal (1,0) karanlık sahnede yansıtacak bir şey bulamayıp simsiyah görünür; yarı metal, biraz pürüzlü.
celik = ortak.malzeme("K5_Celik", (0.60, 0.61, 0.62), puruz=0.38, metal=0.5)
bakalit = ortak.malzeme("K5_Bakalit", (0.02, 0.018, 0.016), puruz=0.5)
ceviz = ortak.malzeme("K5_Ceviz", (0.05, 0.022, 0.008), puruz=0.35)
ortu = ortak.malzeme("K5_Ortu", (0.62, 0.55, 0.42), puruz=0.9)       # krem pike yatak örtüsü
yastik = ortak.malzeme("K5_Yastik", (0.72, 0.70, 0.64), puruz=0.9)   # beyaz yastık kılıfı
battaniye = ortak.malzeme("K5_Battaniye", (0.30, 0.06, 0.05), puruz=0.95)   # ayak ucunda katlı bordo battaniye
dantel = ortak.malzeme("K5_Dantel", (0.80, 0.76, 0.66), puruz=0.9)


def yatik_silindir(ad, r, boy, konum, mat, eksen, kose=16):
    """Yatay silindir; dönüş ekleme anında verilip meshe işlenir (ortak.kutu tuzağı yok)."""
    rot = (0, math.pi / 2, 0) if eksen == "x" else (math.pi / 2, 0, 0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=rot)
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return n


def egik_silindir(ad, r, boy, konum, aci_x, mat, kose=12):
    """x ekseni çevresinde eğik silindir (emzik): dönüş meshe işlenir."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=r, depth=boy, location=konum, rotation=(aci_x, 0, 0))
    n = bpy.context.active_object
    n.name = ad
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, mat)
    return n


def kure(ad, r, konum, mat, olcek=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=r, location=konum)
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcek
    bpy.ops.object.transform_apply(scale=True)
    ortak.ata(n, mat)
    return n


p = []
if hangi == "Caydanlik_K5":
    # ALT KAZAN: geniş, alçak silindir; üstü kubbe; önde (Blender -y = Unity +Z) eğik emzik, üstte bakalit sap.
    p.append(ortak.silindir("Kazan", 0.088, 0.13, (0, 0, 0.065), celik, kose=40))
    p.append(kure("Kazan_Kubbe", 0.088, (0, 0, 0.13), celik, olcek=(1, 1, 0.35)))
    p.append(ortak.silindir("Kazan_Taban", 0.090, 0.006, (0, 0, 0.003), celik, kose=40))
    p.append(egik_silindir("Kazan_Emzik", 0.012, 0.09, (0, -0.105, 0.115), math.radians(-50), celik))
    # Kazanın sapı: iki dik ayak ve üstte yatay bakalit tutamak (yandan yandan, x boyunca).
    for x in (-0.06, 0.06):
        p.append(ortak.kutu("Sap_Ayak", (0.008, 0.008, 0.07), (x, 0.03, 0.18), celik))
    p.append(yatik_silindir("Sap", 0.011, 0.13, (0, 0.03, 0.215), bakalit, "x"))
    # ÜST DEMLİK: kazanın ağzına oturan dar boyun, gövde, kapak ve topuz; emzik öne, sap arkaya.
    p.append(ortak.silindir("Demlik_Boyun", 0.048, 0.02, (0, -0.005, 0.165), celik, kose=32))
    p.append(ortak.silindir("Demlik", 0.058, 0.085, (0, -0.005, 0.217), celik, kose=36))
    p.append(kure("Demlik_Kapak", 0.056, (0, -0.005, 0.26), celik, olcek=(1, 1, 0.3)))
    p.append(kure("Topuz", 0.012, (0, -0.005, 0.282), bakalit))
    p.append(egik_silindir("Demlik_Emzik", 0.008, 0.07, (0, -0.075, 0.225), math.radians(-55), celik))
    p.append(ortak.kutu("Demlik_Sap", (0.012, 0.03, 0.06), (0, 0.072, 0.22), bakalit))
    nesne = ortak.birlestir(p, "Caydanlik_K5")
    ortak.yumusat(nesne, 40)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    boy_onizleme, uzak = 0.35, 0.8

else:   # Karyola_K5
    EN, BOY = 1.45, 2.02          # x genişlik, y uzunluk
    YAN = 0.22                    # yan kayıtların alt kenarı: altı açık (eğilince görülür)
    KAYIT = 0.12                  # yan kayıt yüksekliği
    yatak_alt = YAN + KAYIT       # 0,34
    # Dört ayak (köşelerde 6 x 6 cm), başta yüksek dikmeler başlığı taşır.
    for x in (-EN / 2 + 0.03, EN / 2 - 0.03):
        p.append(ortak.kutu("Ayak_Ayak", (0.06, 0.06, 0.55), (x, -BOY / 2 + 0.03, 0.275), ceviz, 0.004))
        p.append(ortak.kutu("Ayak_Bas", (0.06, 0.06, 1.05), (x, BOY / 2 - 0.03, 0.525), ceviz, 0.004))
    # Yan kayıtlar ve uçlardaki kayıtlar: altı açık bir çerçeve.
    for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
        p.append(ortak.kutu("Yan_Kayit", (0.035, BOY - 0.12, KAYIT), (x, 0, YAN + KAYIT / 2), ceviz, 0.003))
    # Başlık: iki dikme arasında tablalı pano ve üstte hafif kemerli taç (iki basamak); ayak ucunda alçak pano.
    p.append(ortak.kutu("Baslik", (EN - 0.10, 0.03, 0.55), (0, BOY / 2 - 0.03, 0.62), ceviz, 0.004))
    p.append(ortak.kutu("Baslik_Tac", (EN - 0.02, 0.04, 0.06), (0, BOY / 2 - 0.03, 0.93), ceviz, 0.006))
    p.append(ortak.kutu("Baslik_Tac2", (0.70, 0.04, 0.05), (0, BOY / 2 - 0.03, 0.985), ceviz, 0.006))
    for x in (-0.32, 0.32):   # başlıkta iki dikdörtgen tabla çıtası
        p.append(ortak.kutu("Tabla", (0.50, 0.008, 0.36), (x, BOY / 2 - 0.05, 0.62), ceviz, 0.002))
    p.append(ortak.kutu("Ayaklik", (EN - 0.10, 0.03, 0.28), (0, -BOY / 2 + 0.03, 0.40), ceviz, 0.004))
    # Döşek ve örtü: döşek kayıtların üstünde; krem örtü döşeği 4 cm taşar, yanlardan SARKMAZ (altı görünsün).
    p.append(ortak.kutu("Dosek", (EN - 0.08, BOY - 0.10, 0.16), (0, 0, yatak_alt + 0.08), yastik, 0.02))
    p.append(ortak.kutu("Ortu", (EN - 0.02, BOY - 0.20, 0.025), (0, -0.05, yatak_alt + 0.17), ortu, 0.01))
    # Örtünün kenarında dantel şerit (90'ların çeyiz örtüsü).
    for x in (-(EN - 0.02) / 2, (EN - 0.02) / 2):
        p.append(ortak.kutu("Dantel", (0.012, BOY - 0.20, 0.035), (x, -0.05, yatak_alt + 0.165), dantel))
    # İki yastık başta, katlı bordo battaniye ayak ucunda.
    for x in (-0.33, 0.33):
        p.append(kure("Yastik", 0.5, (x, BOY / 2 - 0.30, yatak_alt + 0.24), yastik, olcek=(0.60, 0.36, 0.13)))
    p.append(ortak.kutu("Battaniye", (EN - 0.16, 0.42, 0.06), (0, -BOY / 2 + 0.35, yatak_alt + 0.21), battaniye, 0.02))
    nesne = ortak.birlestir(p, "Karyola_K5")
    ortak.yumusat(nesne, 35)
    ortak.pivotu_tasi(nesne, (0, 0, 0))
    boy_onizleme, uzak = 1.0, 3.0

ortak.calistir_ve_bitir(hangi, [nesne], boy_onizleme, uzak)
