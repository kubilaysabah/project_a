# sofra.py
# Dört kişilik yemek masası ve sandalyeleri (#66, 6. kat: sıralama / yerleştirme).
# 6. katta "kimsenin oturmadığı" dairede sofra kurulmayı bekliyor: dört tabak, dört isim kartı
# (Rıza Efendi, Hasan, Cemal ve "Yeni Kapıcı"). Tabaklar doğru yerlere konunca büfe açılır.
# Tabak yuvaları (Yer_1..4) Unity'de kurucu tarafından masanın üstüne eklenir.
#
# #81 (Şahan'ın seçimi, 2026-09-28): "dantel örtülü ceviz sofra". İlk hâli düz kutulardı; açık renkli tahta
# ve pembe minderlerle oyuncak gibi duruyordu. Artık koyu cilalı ceviz, tornalanmış ayaklar, kenarları masadan
# sarkan ve ucu fistolu (dalgalı) dantel örtü, üstünde bir runner; sandalyeler çubuklu sırtlı, bordo kadife
# minderli. Ölçüler ve sandalyelerin yeri aynı: tabak yuvaları kaymasın (masa üstü 75 cm, örtü 0,4 cm).
#
# PARÇALAR
#   Sofra : masa (140 x 90 cm, 75 cm yükseklik) ve uzun kenarlarda ikişer sandalye. Tek parça.
#           Pivot tabanın ortası. Masanın uzun ekseni x; sandalyeler y = +-0,62'de.
# Ön yüz -y tarafında; Unity'de +Z. (Döndürme yok: ortak.kutu tuzağı, bkz. vana.py.)

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
# Renkler doğrusal değer (Unity sRGB'ye çevirip açık gösterir): 0,05 ekranda koyu ceviz (~0,25) olur.
ceviz = ortak.malzeme("Sofra_Ceviz", (0.05, 0.022, 0.008), puruz=0.3)      # koyu cilalı ceviz
dantel = ortak.malzeme("Sofra_Dantel", (0.86, 0.82, 0.72), puruz=0.9)      # sararmış beyaz dantel
runner = ortak.malzeme("Sofra_Runner", (0.55, 0.12, 0.10), puruz=0.8)      # bordo işlemeli örtü
kumas = ortak.malzeme("Sofra_Kumas", (0.30, 0.05, 0.06), puruz=0.95)       # bordo kadife minder

EN, DERIN, BOY = 1.40, 0.90, 0.75
SARKMA = 0.12                     # örtünün masadan sarkan kısmı

p = [ortak.kutu("Tabla", (EN, DERIN, 0.035), (0, 0, BOY - 0.0175), ceviz, 0.006),
     ortak.kutu("Etek", (EN - 0.10, DERIN - 0.10, 0.08), (0, 0, BOY - 0.035 - 0.04), ceviz, 0.004)]

# Tornalanmış ayaklar: üst üste silindirler (kalın-ince-kalın), ucunda top.
for x in (-EN / 2 + 0.07, EN / 2 - 0.07):
    for y in (-DERIN / 2 + 0.07, DERIN / 2 - 0.07):
        h = BOY - 0.035
        p.append(ortak.kutu("AyakUst", (0.06, 0.06, 0.12), (x, y, h - 0.06), ceviz, 0.004))
        p.append(ortak.silindir("AyakGovde", 0.022, h - 0.20, (x, y, 0.06 + (h - 0.20) / 2), ceviz, kose=16))
        for z, r in ((0.10, 0.032), (h * 0.55, 0.030), (h - 0.14, 0.028)):
            p.append(ortak.silindir("Bilezik", r, 0.035, (x, y, z), ceviz, kose=16))
        p.append(ortak.silindir("AyakUc", 0.026, 0.06, (x, y, 0.03), ceviz, kose=16))

# Dantel örtü: masanın üstünü örter, dört kenardan sarkar; sarkan kısmın ucunda fisto (küçük yarım daireler).
p.append(ortak.kutu("Ortu", (EN + 0.02, DERIN + 0.02, 0.004), (0, 0, BOY + 0.002), dantel))
for yon in (-1, 1):
    p.append(ortak.kutu("OrtuUzun", (EN + 0.02, 0.004, SARKMA), (0, yon * (DERIN / 2 + 0.012), BOY - SARKMA / 2), dantel))
    p.append(ortak.kutu("OrtuKisa", (0.004, DERIN + 0.02, SARKMA), (yon * (EN / 2 + 0.012), 0, BOY - SARKMA / 2), dantel))
fisto_z = BOY - SARKMA


def dik_disk(x, y, eksen_y):
    """Sarkan örtünün alt kenarında dik duran yarım daire (fisto): ekseni örtüye dik ince silindir.
    Ekleme anında kendi yerinde döndürülüp dönüş meshe işlenir (ortak.kutu tuzağı yok)."""
    rot = (math.pi / 2, 0, 0) if eksen_y else (0, math.pi / 2, 0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.03, depth=0.004, location=(x, y, fisto_z), rotation=rot)
    n = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    ortak.ata(n, dantel)
    return n


for i in range(22):
    x = -EN / 2 + (i + 0.5) * EN / 22
    for yon in (-1, 1):
        p.append(dik_disk(x, yon * (DERIN / 2 + 0.012), True))
for i in range(14):
    y = -DERIN / 2 + (i + 0.5) * DERIN / 14
    for yon in (-1, 1):
        p.append(dik_disk(yon * (EN / 2 + 0.012), y, False))

# Runner: masanın ortasında boydan boya bordo işlemeli şerit ve kenar sırması.
p.append(ortak.kutu("Runner", (EN - 0.30, 0.30, 0.003), (0, 0, BOY + 0.0055), runner))
for yon in (-1, 1):
    p.append(ortak.kutu("Sirma", (EN - 0.30, 0.012, 0.0035), (0, yon * 0.14, BOY + 0.006), dantel))

# Sandalyeler: ön kenarda (-y) iki, arka kenarda (+y) iki; sırtları masadan uzakta.
for x in (-0.35, 0.35):
    for yon in (-1, 1):
        y = yon * 0.62
        p.append(ortak.kutu("Oturak", (0.44, 0.42, 0.045), (x, y, 0.45), ceviz, 0.008))
        p.append(ortak.kutu("Minder", (0.40, 0.38, 0.05), (x, y, 0.495), kumas, 0.015))
        # Arka ayaklar sırta kadar yükselir; ön ayaklar tornalı.
        sy = y + yon * 0.19
        for dx in (-0.19, 0.19):
            p.append(ortak.kutu("ArkaAyak", (0.035, 0.035, 0.95), (x + dx, sy, 0.475), ceviz, 0.004))
            p.append(ortak.silindir("OnAyak", 0.017, 0.43, (x + dx, y - yon * 0.18, 0.215), ceviz, kose=12))
            p.append(ortak.silindir("OnBilezik", 0.024, 0.03, (x + dx, y - yon * 0.18, 0.30), ceviz, kose=12))
        # Sırt: üstte kavisli görünen tepe kuşağı (iki basamak), altında beş ince çubuk.
        p.append(ortak.kutu("SirtTepe", (0.42, 0.035, 0.07), (x, sy, 0.90), ceviz, 0.01))
        p.append(ortak.kutu("SirtTepe2", (0.22, 0.035, 0.03), (x, sy, 0.945), ceviz, 0.008))
        p.append(ortak.kutu("SirtAlt", (0.40, 0.03, 0.035), (x, sy, 0.56), ceviz, 0.004))
        for i in range(5):
            p.append(ortak.silindir("Cubuk", 0.009, 0.30, (x - 0.14 + i * 0.07, sy, 0.715), ceviz, kose=10))
        # Ayaklar arası bağlantı çıtası.
        p.append(ortak.kutu("Kusak", (0.38, 0.02, 0.02), (x, y, 0.14), ceviz))

sofra = ortak.birlestir(p, "Sofra")
ortak.yumusat(sofra, 30)
ortak.pivotu_tasi(sofra, (0, 0, 0))
ortak.calistir_ve_bitir("Sofra", [sofra], 1.2, 3.2)
