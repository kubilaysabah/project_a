# kat_isiklari_116.py
# Kat imzası ışıklarının (#116) elektriksiz kaynakları: bina karanlık (Şahan, #111), ama her katın dairesine
# girince o katı tanıtan zayıf bir ışık var. 1994'te sık elektrik kesintileri olurdu; evlerde gaz lambası,
# mum ve el feneri hazır dururdu. Kurala göre (#83) model katlar arasında tekrarlanmaz: üç ayrı eşya.
#
#   Gaz_Lambasi_K5 : pirinç gazyağı lambası (5. kat holünde yerde). Yuvarlak depo, fitil ağzı, camın
#                    çevresinde tel kafes, üstte kapak ve taşıma halkası. CAM YOK: camın yerine Unity'de
#                    parlayan bir yumurta konur (ZincirUygulayici, KatIsigi.AlevBoyu), ışık da onun içinde.
#                    Cam bölgesi: z 0,115-0,235 (merkez 0,175), yarıçap ~0,036.
#   Samdan_K6      : pirinç tek kollu şamdan ve beyaz mum (6. kat, 1974 sofrasının ortasında). Mumun fitili
#                    z 0,300'de biter; alev Unity'de onun üstüne konur (merkez 0,318).
#   El_Feneri_K7   : 90'ların metal el feneri, yerde yatık (7. kat holünde). Ön yüzü (cam) Blender'da -y
#                    yani Unity'de +Z; camın merkezi Unity'de (0, 0,026, 0,117). Spot ışık oradan çıkar.
#
# Pivot: tabanın ortası. Ön yüz Blender'da -y → Unity'de +Z (ortak.py).
# Çalıştırma:  blender -b --factory-startup --python kat_isiklari_116.py -- Gaz_Lambasi_K5
import math, os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Gaz_Lambasi_K5"
ortak.sahneyi_temizle()

# Blender malzeme renkleri doğrusal değerdir; Unity açık gösterir (CLAUDE.md): koyu seçildi.
pirinc = ortak.malzeme("I116_Pirinc", (0.30, 0.19, 0.06), puruz=0.38, metal=0.9)
pirinc_koyu = ortak.malzeme("I116_PirincKoyu", (0.12, 0.07, 0.02), puruz=0.5, metal=0.8)
tel = ortak.malzeme("I116_Tel", (0.05, 0.05, 0.05), puruz=0.45, metal=0.9)
mum = ortak.malzeme("I116_Mum", (0.62, 0.58, 0.48), puruz=0.6)
fitil = ortak.malzeme("I116_Fitil", (0.02, 0.018, 0.015), puruz=0.9)
krom = ortak.malzeme("I116_Krom", (0.40, 0.40, 0.42), puruz=0.25, metal=1.0)
siyah = ortak.malzeme("I116_Siyah", (0.02, 0.02, 0.02), puruz=0.5)
lastik = ortak.malzeme("I116_Lastik", (0.03, 0.03, 0.03), puruz=0.85)

p = []

def dik_silindir(ad, r, boy, z_alt, mat, kose=32, x=0.0, y=0.0):
    """Tabanı z_alt'ta duran dik silindir."""
    return ortak.silindir(ad, r, boy, (x, y, z_alt + boy / 2), mat, kose=kose)

if hangi == "Gaz_Lambasi_K5":
    # Ayak: geniş, alçak; üstünde basık yuvarlak depo (gazyağı haznesi).
    p.append(dik_silindir("Ayak", 0.060, 0.012, 0.0, pirinc_koyu, 40))
    p.append(dik_silindir("AyakBoyun", 0.040, 0.010, 0.012, pirinc, 40))
    depo = dik_silindir("Depo", 0.055, 0.060, 0.022, pirinc, 40)
    depo.scale = (1.0, 1.0, 1.0)
    p.append(depo)
    ortak.pah(depo, 0.012, 3)                                    # yuvarlak omuzlar
    p.append(dik_silindir("DepoKapak", 0.010, 0.010, 0.082, pirinc_koyu, 16, x=0.040))   # doldurma ağzı
    # Fitil ağzı (brülör) ve fitil düğmesi.
    p.append(dik_silindir("Brulor", 0.030, 0.020, 0.082, pirinc, 32))
    p.append(dik_silindir("BrulorTac", 0.038, 0.013, 0.102, pirinc_koyu, 32))
    d = ortak.silindir("FitilDugme", 0.008, 0.016, (0.0, -0.045, 0.093), pirinc, kose=16)
    d.rotation_euler = (math.radians(90), 0, 0)
    p.append(d)
    # Tel kafes: camın çevresinde altı dik tel, iki halka (cam bölgesi z 0,115-0,235).
    for i in range(6):
        a = i * math.pi / 3
        p.append(ortak.kutu(f"Tel{i}", (0.003, 0.003, 0.125), (0.044 * math.cos(a), 0.044 * math.sin(a), 0.175), tel))
    for z in (0.140, 0.210):
        halka = ortak.silindir("Halka", 0.045, 0.003, (0, 0, z), tel, kose=36)
        p.append(halka)
        ic = ortak.silindir("HalkaIc", 0.042, 0.006, (0, 0, z), tel, kose=36)
        ortak.oy(halka, ic)
    # Üst kapak (baca) ve taşıma halkası.
    p.append(dik_silindir("UstKapak", 0.040, 0.012, 0.235, pirinc_koyu, 32))
    p.append(dik_silindir("Baca", 0.020, 0.018, 0.247, pirinc, 24))
    sap = ortak.silindir("Sap", 0.040, 0.004, (0, 0, 0.300), tel, kose=36)
    sap.rotation_euler = (math.radians(90), 0, 0)
    p.append(sap)
    sap_ic = ortak.silindir("SapIc", 0.037, 0.010, (0, 0, 0.300), tel, kose=36)
    sap_ic.rotation_euler = (math.radians(90), 0, 0)
    ortak.oy(sap, sap_ic)
    boy_onizleme, uzak = 0.4, 0.8

elif hangi == "Samdan_K6":
    # Pirinç taban (iki kademe), ince gövde, ortada bir boğum, üstte mum tablası ve kovanı.
    p.append(dik_silindir("Taban", 0.055, 0.010, 0.0, pirinc_koyu, 40))
    p.append(dik_silindir("Taban2", 0.042, 0.008, 0.010, pirinc, 40))
    p.append(dik_silindir("Govde", 0.008, 0.140, 0.018, pirinc, 20))
    p.append(ortak.silindir("Bogum", 0.016, 0.014, (0, 0, 0.085), pirinc_koyu, kose=24))
    p.append(dik_silindir("Tabla", 0.030, 0.006, 0.156, pirinc, 32))
    p.append(dik_silindir("Kovan", 0.015, 0.020, 0.160, pirinc_koyu, 24))
    # Mum: yarı erimiş, kenarında akmış bir damla; fitil siyah.
    p.append(dik_silindir("Mum", 0.011, 0.115, 0.180, mum, 24))
    p.append(ortak.kutu("Damla", (0.006, 0.004, 0.030), (0.010, 0.0, 0.250), mum))
    p.append(dik_silindir("Fitil", 0.0015, 0.007, 0.293, fitil, 8))
    boy_onizleme, uzak = 0.4, 0.7

elif hangi == "El_Feneri_K7":
    # Yerde yatan metal el feneri: gövde Blender'da y ekseni boyunca, ön (cam) -y'de. Yerde durması için
    # gövdenin merkezi z = 0,026 (kafanın yarıçapı); ince gövde yere değmez, kafa ve kuyruk değer.
    R_GOVDE, R_KAFA, Z = 0.018, 0.026, 0.026
    g = ortak.silindir("Govde", R_GOVDE, 0.150, (0, 0.020, Z), krom, kose=28)
    g.rotation_euler = (math.radians(90), 0, 0)
    p.append(g)
    # Tırtıllı tutma bandı: gövdenin ortasında dört ince halka.
    for i in range(4):
        h = ortak.silindir("Tirtil", R_GOVDE + 0.0015, 0.004, (0, 0.040 + i * 0.010, Z), siyah, kose=28)
        h.rotation_euler = (math.radians(90), 0, 0)
        p.append(h)
    # Kafa: gövdeden kafaya genişleyen huni (kesik koni yerine iki kademe) ve cam çerçevesi.
    k1 = ortak.silindir("KafaBoyun", 0.022, 0.020, (0, -0.065, Z), krom, kose=32)
    k1.rotation_euler = (math.radians(90), 0, 0)
    p.append(k1)
    k2 = ortak.silindir("Kafa", R_KAFA, 0.035, (0, -0.0925, Z), krom, kose=32)
    k2.rotation_euler = (math.radians(90), 0, 0)
    p.append(k2)
    cer = ortak.silindir("CamCerceve", R_KAFA + 0.002, 0.006, (0, -0.111, Z), siyah, kose=32)
    cer.rotation_euler = (math.radians(90), 0, 0)
    oy_ic = ortak.silindir("CamOyuk", R_KAFA - 0.004, 0.010, (0, -0.113, Z), siyah, kose=32)
    oy_ic.rotation_euler = (math.radians(90), 0, 0)
    ortak.oy(cer, oy_ic)
    p.append(cer)
    # Düğme (üstte, siyah) ve kuyruk kapağı (arka uç, lastik), asma halkası.
    p.append(ortak.kutu("Dugme", (0.010, 0.016, 0.006), (0, -0.030, Z + R_GOVDE + 0.002), siyah, 0.002))
    ku = ortak.silindir("Kuyruk", R_GOVDE + 0.003, 0.014, (0, 0.100, Z), lastik, kose=28)
    ku.rotation_euler = (math.radians(90), 0, 0)
    p.append(ku)
    boy_onizleme, uzak = 0.15, 0.5

else:
    raise SystemExit("bilinmeyen model: " + hangi)

esya = ortak.birlestir(p, hangi)
# birlestir, ilk parçanın dönüşünü nesnede bırakır (fenerde ilk parça 90° yatırılmış silindir): dönüş meshe
# işlenir, yoksa Unity'ye giden nesne kökü dönük kalır ve şablon meshi dik durur.
import bpy
bpy.ops.object.select_all(action="DESELECT")
esya.select_set(True)
bpy.context.view_layer.objects.active = esya
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
ortak.yumusat(esya, 35)
ortak.pivotu_tasi(esya, (0, 0, 0))
ortak.calistir_ve_bitir(hangi, [esya], boy_onizleme, uzak)
