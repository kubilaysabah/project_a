# sigorta_kutusu.py
# Antre duvarındaki sigorta kutusu: kapak, dört buşonlu sigorta ve bir ana şalter (#53).
# Bulmaca eşyası, iki kilide birden hizmet eder (Docs/hikaye.md §6):
#   - Sigorta kilidi: bir sigorta eksiktir (Unity'de gizlenir); bulunup takılınca elektrik gelir.
#   - Kol kilidi: ana şalter çekilince (aşağı indirilince) bağlı olduğu kapının kilidi açılır.
#
# DUVAR EŞYASI: ön yüz -y tarafında, pivot arka yüzün alt ortası.
# PARÇALAR
#   Sigorta_Kutusu : sac kutu, içinde koyu pano ve sigorta yuvaları.
#   Kapak          : menteşesi sol kenarda (oyuncunun solu); Unity'de y ekseni etrafında döner.
#   Sigorta_1..4   : porselen gövdeli, renkli başlıklı sigortalar; pivot kutunun pivotunda.
#   Salter         : ana şalterin kolu; pivotu kendi milinde, Unity'de x ekseni etrafında döner.
#                    Modelde kol YUKARIDA (elektrik açık) durur.

import math, os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

EN, DERIN, BOY = 0.30, 0.09, 0.38
KAPAK = 0.012

ortak.sahneyi_temizle()
sac = ortak.malzeme("Sigorta_Sac", (0.55, 0.55, 0.50), puruz=0.5, metal=0.4)
pano = ortak.malzeme("Sigorta_Pano", (0.06, 0.06, 0.06), puruz=0.8)
porselen = ortak.malzeme("Sigorta_Porselen", (0.85, 0.84, 0.80), puruz=0.3)
kirmizi = ortak.malzeme("Sigorta_Kirmizi", (0.55, 0.05, 0.04), puruz=0.4)
siyah = ortak.malzeme("Sigorta_Siyah", (0.03, 0.03, 0.03), puruz=0.5)


def yatir(n):
    """Dik duran silindiri öne doğru (y ekseni boyunca) yatırır ve dönüşü meshe işler.
    Önce döndürüp sonra uygulamak şart: uygulanmayan dönüş FBX'te ayrı bir açı olarak
    kalır ve Unity'de parça beklenmedik bir yöne bakar."""
    n.rotation_euler = (math.pi / 2, 0, 0)
    bpy.ops.object.select_all(action="DESELECT")
    n.select_set(True)
    bpy.context.view_layer.objects.active = n
    bpy.ops.object.transform_apply(rotation=True)
    return n


# ---------- KUTU ----------
kutu = ortak.kutu("Kutu", (EN, DERIN, BOY), (0, -DERIN / 2, BOY / 2), sac, 0.004)
ortak.oy(kutu, ortak.kutu("k", (EN - 0.02, DERIN, BOY - 0.02), (0, -DERIN / 2 - 0.012, BOY / 2), sac))
ic_pano = ortak.kutu("Pano", (EN - 0.02, 0.004, BOY - 0.02), (0, -0.012, BOY / 2), pano)
parcalar = [kutu, ic_pano]
# Dört sigorta yuvası (porselen halkalar) ve şalterin gövdesi.
xler = (-0.09, -0.03, 0.03, 0.09)
sig_z = 0.25
for i, x in enumerate(xler):
    parcalar.append(yatir(ortak.silindir(f"Yuva{i}", 0.02, 0.02, (x, -0.024, sig_z), porselen, kose=20)))
parcalar.append(ortak.kutu("SalterGovde", (0.10, 0.03, 0.08), (0, -0.029, 0.10), siyah, 0.004))
govde = ortak.birlestir(parcalar, "Sigorta_Kutusu")
ortak.yumusat(govde, 30)
ortak.pivotu_tasi(govde, (0, 0, 0))
nesneler = [govde]

# ---------- SİGORTALAR ----------
for i, x in enumerate(xler):
    g = yatir(ortak.silindir("G", 0.014, 0.03, (x, -0.049, sig_z), porselen, kose=16))
    b = yatir(ortak.silindir("B", 0.010, 0.008, (x, -0.068, sig_z), kirmizi, kose=16))   # renkli başlık
    s = ortak.birlestir([g, b], f"Sigorta_{i + 1}")
    ortak.yumusat(s, 40)
    ortak.pivotu_tasi(s, (0, 0, 0))
    nesneler.append(s)

# ---------- ANA ŞALTER KOLU ----------
# Mil şalter gövdesinin ortasında (y = -0.045, z = 0.10). Kol yukarı dönük: elektrik açık.
mil = (0, -0.045, 0.10)
kol = ortak.kutu("Kol", (0.03, 0.018, 0.07), (0, -0.05, 0.10 + 0.035), kirmizi, 0.004)
dugme = ortak.kutu("Dugme", (0.05, 0.03, 0.02), (0, -0.055, 0.10 + 0.07), kirmizi, 0.005)
salter = ortak.birlestir([kol, dugme], "Salter")
ortak.yumusat(salter, 30)
ortak.pivotu_tasi(salter, mil)
nesneler.append(salter)

# ---------- KAPAK ----------
kapak = ortak.kutu("Kapak", (EN, KAPAK, BOY), (0, -DERIN - KAPAK / 2, BOY / 2), sac, 0.003)
# Yıldırım işareti levhası (sarı) ve kilit dili.
sari = ortak.malzeme("Sigorta_Sari", (0.85, 0.65, 0.05), puruz=0.5)
levha = ortak.kutu("Levha", (0.07, 0.003, 0.07), (0, -DERIN - KAPAK - 0.0015, BOY * 0.65), sari)
dil = ortak.kutu("Dil", (0.015, 0.012, 0.03), (EN / 2 - 0.025, -DERIN - KAPAK - 0.006, BOY / 2), siyah, 0.002)
kapak = ortak.birlestir([kapak, levha, dil], "Kapak")
ortak.yumusat(kapak, 30)
# Menteşe: Blender -x = Unity +x = önünde duran oyuncunun solu.
ortak.pivotu_tasi(kapak, (-EN / 2, -DERIN, 0))
nesneler.append(kapak)

ortak.calistir_ve_bitir("Sigorta_Kutusu", nesneler, BOY, 0.9, onizlemede_kaldir=0.05)
