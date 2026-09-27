# buzdolabi.py
# 90'ların tek kapılı küçük buzdolabı (#53).
# Bulmaca eşyası: 4. katta (elektriksiz daire) sigorta takılınca buzdolabının ışığı yanar ve
# Oda B'nin anahtarı buzdolabında görünür (Docs/hikaye.md §7).
#
# PARÇALAR
#   Buzdolabi : dış gövde, içi oyulmuş; içeride iki raf, üstte dondurucu bölmesi ve lamba yuvası.
#               Pivot tabanın ortası.
#   Kapi      : kapı, içinde iki kapı rafı; menteşe sağ ön kenarda (oyuncunun sağı).
#               Unity'de y ekseni etrafında döner.
# Işık Unity'de eklenir: lamba yuvasının hizasına bir nokta ışığı (sigorta takılınca yanar).

import os, sys
sys.path.append(os.path.dirname(__file__))
import ortak

EN, DERIN, BOY = 0.58, 0.60, 1.45
KAPI = 0.055                      # kapı kalınlığı
ET = 0.035                        # gövde duvarının kalınlığı

ortak.sahneyi_temizle()
emaye = ortak.malzeme("Buzdolabi_Emaye", (0.80, 0.79, 0.74), puruz=0.3)       # sararmış beyaz emaye
ic = ortak.malzeme("Buzdolabi_Ic", (0.88, 0.88, 0.86), puruz=0.4)           # beyaz plastik iç
conta = ortak.malzeme("Buzdolabi_Conta", (0.10, 0.10, 0.10), puruz=0.7)     # kapı lastiği
krom = ortak.malzeme("Buzdolabi_Krom", (0.6, 0.6, 0.6), puruz=0.2, metal=1.0)
raf_mat = ortak.malzeme("Buzdolabi_Raf", (0.55, 0.60, 0.62), puruz=0.1)     # buzlu cam raf

# ---------- GÖVDE ----------
# Kapı gövdenin önüne oturur; gövde DERIN - KAPI derinliğinde, ön ağzı y = -(DERIN/2 - KAPI).
govde_derin = DERIN - KAPI
govde_y = KAPI / 2                                  # gövdenin merkezi arkaya kayık
on_y = govde_y - govde_derin / 2                    # gövdenin ön ağzı
govde = ortak.kutu("Govde", (EN, govde_derin, BOY), (0, govde_y, BOY / 2), emaye, 0.02)
ortak.oy(govde, ortak.kutu("ic", (EN - 2 * ET, govde_derin, BOY - 2 * ET - 0.06),
                           (0, govde_y - ET, BOY / 2 + 0.03), ic))
parcalar = [govde]
# İç raflar: iki buzlu cam raf.
for i, z in enumerate((0.45, 0.80)):
    parcalar.append(ortak.kutu(f"Raf{i}", (EN - 2 * ET - 0.01, govde_derin - ET - 0.04, 0.008),
                               (0, govde_y - ET / 2 + 0.01, z), raf_mat))
# Dondurucu bölmesi: tepede, içi oyulmuş küçük bir kutu.
don_z = BOY - ET - 0.03 - 0.10
don = ortak.kutu("Dondurucu", (EN - 2 * ET - 0.06, govde_derin - ET - 0.06, 0.18), (0, govde_y, don_z), ic, 0.004)
ortak.oy(don, ortak.kutu("don_ic", (EN - 2 * ET - 0.09, govde_derin, 0.15), (0, govde_y - 0.03, don_z), ic))
parcalar.append(don)
# Lamba yuvası: sağ iç duvarda, rafların üstünde küçük bir kutu (ışık Unity'de buraya konur).
parcalar.append(ortak.kutu("Lamba_Yuvasi", (0.02, 0.08, 0.05), (EN / 2 - ET - 0.01, govde_y - 0.05, 1.05), ic, 0.004))
# Arka alt havalandırma ızgarası (arkadan bakılmayacak ama gövdeyi süsler) ve ayaklar.
for x in (-EN / 2 + 0.05, EN / 2 - 0.05):
    parcalar.append(ortak.kutu(f"Ayak{x}", (0.04, 0.04, 0.02), (x, govde_y - govde_derin / 2 + 0.05, 0.01), conta))
buzdolabi = ortak.birlestir(parcalar, "Buzdolabi")
ortak.yumusat(buzdolabi, 30)
ortak.pivotu_tasi(buzdolabi, (0, 0, 0))

# ---------- KAPI ----------
kapi_y = on_y - KAPI / 2
kapi = ortak.kutu("Panel", (EN, KAPI, BOY - 0.02), (0, kapi_y, BOY / 2 + 0.005), emaye, 0.015)
# İç yüzde lastik conta ve iki kapı rafı (şişe rafı).
kapi_parca = [kapi, ortak.kutu("Conta", (EN - 0.04, 0.01, BOY - 0.10), (0, on_y + 0.004, BOY / 2), conta)]
for i, z in enumerate((0.35, 0.75)):
    raf = ortak.kutu(f"KapiRaf{i}", (EN - 0.10, 0.07, 0.08), (0, on_y + 0.045, z), ic, 0.004)
    ortak.oy(raf, ortak.kutu("r", (EN - 0.12, 0.05, 0.08), (0, on_y + 0.045, z + 0.01), ic))
    kapi_parca.append(raf)
# Kulp: sol kenarda (menteşenin karşısı) dikey krom tutamak ve üstte marka plakası.
kapi_parca.append(ortak.kutu("Kulp", (0.025, 0.035, 0.30), (-EN / 2 + 0.05, kapi_y - KAPI / 2 - 0.017, 1.00), krom, 0.006))
kapi_parca.append(ortak.kutu("Plaka", (0.12, 0.004, 0.025), (0, kapi_y - KAPI / 2 - 0.002, BOY - 0.12), krom))
kapi = ortak.birlestir(kapi_parca, "Kapi")
ortak.yumusat(kapi, 30)
# Menteşe: Blender +x = Unity -x = önünde duran oyuncunun sağı; ön yüzün kenarında.
ortak.pivotu_tasi(kapi, (EN / 2, kapi_y, 0))

ortak.calistir_ve_bitir("Buzdolabi", [buzdolabi, kapi], BOY, 2.6)
