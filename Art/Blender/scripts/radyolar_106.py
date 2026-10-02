# radyolar_106.py
# Radyo anonsunun (#106) ses cihazı olmayan katları için üç küçük radyo. Her katta 1974'teki olaya değinen
# bir haber cümlesi radyodan ya da televizyondan gelir; 3, 5, 6 ve 7. katların kendi TV'leri var, 1, 4 ve 8.
# katlarda yoktu. Kurala göre (#83) model katlar arasında tekrarlanmaz: üç ayrı radyo.
#
#   Radyo_Kutu_K1 : 80'lerin plastik radyo-teypi (taşınma kolisinin üstünde kalmış). Koyu gri gövde, iki
#                   yuvarlak hoparlör ızgarası, ortada kaset penceresi, üstte tuş sırası, yatık anten.
#   Radyo_Cep_K4  : cep transistörlü radyo (Hasan'ın kamp yatağının üstünde, YATIK durur). Siyah plastik,
#                   delikli ızgara, küçük kadran, kulaklık yuvası.
#   Radyo_Masa_K8 : yönetimin masa radyosu. Bej plastik, büyük yuvarlak kadran, dik teleskopik anten.
#
# Pivot: tabanın ortası (eşya durduğu yüzeyin üstüne konur). Ön yüz Blender'da -y → Unity'de +Z (ortak.py).
# Çalıştırma:  blender -b --factory-startup --python radyolar_106.py -- Radyo_Kutu_K1
import math, os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

hangi = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Radyo_Kutu_K1"
ortak.sahneyi_temizle()

# Blender malzeme renkleri doğrusal değerdir; Unity açık gösterir (CLAUDE.md): koyu seçildi.
plastik_koyu = ortak.malzeme("R106_PlastikKoyu", (0.025, 0.025, 0.027), puruz=0.55)
plastik_bej = ortak.malzeme("R106_PlastikBej", (0.42, 0.36, 0.24), puruz=0.6)
izgara = ortak.malzeme("R106_Izgara", (0.30, 0.30, 0.31), puruz=0.35, metal=0.8)
krom = ortak.malzeme("R106_Krom", (0.55, 0.56, 0.58), puruz=0.2, metal=1.0)
cam = ortak.malzeme("R106_Cam", (0.08, 0.08, 0.06), puruz=0.15)
kadran_m = ortak.malzeme("R106_Kadran", (0.70, 0.62, 0.40), puruz=0.3)
tus_m = ortak.malzeme("R106_Tus", (0.05, 0.05, 0.05), puruz=0.5)

p = []   # parçalar

if hangi == "Radyo_Kutu_K1":
    EN, DERIN, BOY = 0.36, 0.10, 0.14
    govde = ortak.kutu("Govde", (EN, DERIN, BOY), (0, 0, BOY / 2), plastik_koyu, 0.004)
    p.append(govde)
    on = -DERIN / 2
    # İki yuvarlak hoparlör ızgarası: ön yüzde, 1 mm dışarı taşan ince diskler.
    for x in (-EN / 2 + 0.065, EN / 2 - 0.065):
        d = ortak.silindir("Hoparlor", 0.045, 0.004, (x, on - 0.001, BOY / 2 - 0.005), izgara, kose=40)
        d.rotation_euler = (math.radians(90), 0, 0)
        p.append(d)
    # Kaset penceresi ortada: koyu cam, çerçeve.
    p.append(ortak.kutu("KasetCerceve", (0.11, 0.004, 0.06), (0, on - 0.002, BOY / 2 - 0.01), plastik_koyu))
    p.append(ortak.kutu("KasetCam", (0.095, 0.002, 0.045), (0, on - 0.0045, BOY / 2 - 0.01), cam))
    # Üstte altı tuş (play, rec, stop...): küçük dikdörtgenler, biri basılı (daha alçak).
    for i in range(6):
        x = -0.06 + i * 0.024
        h = 0.006 if i != 2 else 0.003
        p.append(ortak.kutu(f"Tus{i}", (0.018, 0.016, h), (x, 0.02, BOY + h / 2), tus_m))
    # Sağ üstte ses ve istasyon düğmeleri (üstten, dik silindir).
    for i, x in enumerate((EN / 2 - 0.05, EN / 2 - 0.025)):
        p.append(ortak.silindir(f"Dugme{i}", 0.008, 0.006, (x, -0.01, BOY + 0.003), tus_m, kose=20))
    # Anten yatık, sol arkada: ince krom çubuk (döndürmek yerine son ölçüsüyle kutu).
    p.append(ortak.kutu("Anten", (0.30, 0.005, 0.005), (-0.02, DERIN / 2 - 0.012, BOY + 0.004), krom))
    p.append(ortak.silindir("AntenYuva", 0.007, 0.012, (-EN / 2 + 0.03, DERIN / 2 - 0.012, BOY + 0.006), tus_m, kose=16))
    # Taşıma sapı: üstte, iki küçük ayakla duran yatık çubuk.
    p.append(ortak.kutu("Sap", (0.16, 0.012, 0.01), (0.02, -0.03, BOY + 0.03), plastik_koyu, 0.003))
    for x in (-0.06, 0.10):
        p.append(ortak.kutu("SapAyak", (0.012, 0.012, 0.03), (x, -0.03, BOY + 0.015), plastik_koyu))
    boy_onizleme, uzak = 0.5, 0.9

elif hangi == "Radyo_Cep_K4":
    # Yatık durur: uzun kenar x, kalınlık z. Ön yüz (ızgara) yukarı bakar (yatay yatınca üstte).
    EN, DERIN, KALIN = 0.12, 0.07, 0.025
    govde = ortak.kutu("Govde", (EN, DERIN, KALIN), (0, 0, KALIN / 2), plastik_koyu, 0.003)
    p.append(govde)
    ust = KALIN
    # Izgara: üst yüzün sol yarısında ince çizgiler (yatay oluklar yerine 1 mm kabartma çubuklar).
    for i in range(7):
        y = -DERIN / 2 + 0.012 + i * 0.0075
        p.append(ortak.kutu(f"Oluk{i}", (0.05, 0.003, 0.001), (-EN / 2 + 0.035, y, ust + 0.0005), izgara))
    # Kadran: sağda küçük dikdörtgen cam ve kırmızı ibre.
    p.append(ortak.kutu("Kadran", (0.035, 0.03, 0.0015), (EN / 2 - 0.03, 0.005, ust + 0.0008), kadran_m))
    p.append(ortak.kutu("Ibre", (0.001, 0.028, 0.002), (EN / 2 - 0.038, 0.005, ust + 0.001), ortak.malzeme("R106_Ibre", (0.5, 0.02, 0.02), puruz=0.5)))
    # Ayar tekerleği: sağ kenardan dışarı taşan ince dik disk.
    t = ortak.silindir("Teker", 0.012, 0.004, (EN / 2 - 0.002, -0.02, KALIN / 2), tus_m, kose=28)
    t.rotation_euler = (0, math.radians(90), 0)
    p.append(t)
    # Kulaklık yuvası: ön kenarda küçük silindir deliği yerine krom bir halka.
    h = ortak.silindir("Yuva", 0.003, 0.003, (-EN / 2 + 0.015, -DERIN / 2 - 0.0005, KALIN / 2), krom, kose=16)
    h.rotation_euler = (math.radians(90), 0, 0)
    p.append(h)
    # Katlı kısa anten: üst yüzde, arka kenara yakın, yatık.
    p.append(ortak.kutu("Anten", (0.09, 0.003, 0.003), (0.0, DERIN / 2 - 0.006, ust + 0.0015), krom))
    boy_onizleme, uzak = 0.3, 0.45

else:   # Radyo_Masa_K8
    EN, DERIN, BOY = 0.26, 0.09, 0.12
    govde = ortak.kutu("Govde", (EN, DERIN, BOY), (0, 0, BOY / 2), plastik_bej, 0.005)
    p.append(govde)
    on = -DERIN / 2
    # Büyük yuvarlak kadran solda (cam + içinde pirinç ibre), hoparlör ızgarası sağda (dikey çubuklar).
    k = ortak.silindir("KadranCam", 0.042, 0.004, (-EN / 2 + 0.065, on - 0.001, BOY / 2), kadran_m, kose=40)
    k.rotation_euler = (math.radians(90), 0, 0)
    p.append(k)
    p.append(ortak.kutu("Ibre", (0.002, 0.003, 0.07), (-EN / 2 + 0.07, on - 0.0035, BOY / 2), ortak.malzeme("R106_Ibre2", (0.5, 0.02, 0.02), puruz=0.5)))
    for i in range(9):
        x = 0.01 + i * 0.0125
        p.append(ortak.kutu(f"Cubuk{i}", (0.004, 0.004, BOY - 0.04), (x, on - 0.002, BOY / 2), izgara))
    # Alt kenarda iki düğme.
    for i, x in enumerate((-EN / 2 + 0.03, -EN / 2 + 0.1)):
        d = ortak.silindir(f"Dugme{i}", 0.009, 0.012, (x, on - 0.004, 0.018), tus_m, kose=20)
        d.rotation_euler = (math.radians(90), 0, 0)
        p.append(d)
    # Dik teleskopik anten: sağ arkada, üç kademe incelen silindirler.
    x_a, y_a = EN / 2 - 0.02, DERIN / 2 - 0.015
    z = BOY
    for i, (r, h) in enumerate(((0.004, 0.09), (0.003, 0.09), (0.002, 0.08))):
        p.append(ortak.silindir(f"Anten{i}", r, h, (x_a, y_a, z + h / 2), krom, kose=12))
        z += h
    p.append(ortak.silindir("AntenUc", 0.004, 0.006, (x_a, y_a, z + 0.003), krom, kose=12))
    # Dört küçük lastik ayak.
    for x in (-EN / 2 + 0.02, EN / 2 - 0.02):
        for y in (-DERIN / 2 + 0.015, DERIN / 2 - 0.015):
            p.append(ortak.kutu("Ayak", (0.012, 0.012, 0.004), (x, y, 0.002), tus_m))
    # Gövde ayakların üstüne biner: gövdeyi 4 mm kaldır.
    govde.location.z += 0.004
    boy_onizleme, uzak = 0.5, 0.8

radyo = ortak.birlestir(p, hangi)
ortak.yumusat(radyo, 35)
ortak.pivotu_tasi(radyo, (0, 0, 0))
ortak.calistir_ve_bitir(hangi, [radyo], boy_onizleme, uzak)
