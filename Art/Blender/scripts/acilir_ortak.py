# acilir_ortak.py
# Görev kabı olabilecek açılır mobilyaların ortak parçaları (#83): çekmece, kapı, kapak.
#
# NEDEN AYRI PARÇALAR
# Unity'de açılabilen şey ayrı bir nesne olmalı: çekmece gövdeden bağımsız kayar, kapı kendi
# menteşesinde döner. Bu yüzden FBX'e birden fazla nesne gider ve adları sözleşmedir; Unity'deki
# GrayboxKurucu.AcilirSablonKur onları adlarından tanır:
#   Govde      : gövde (içi boş, önü açık); içine eşya konabilsin diye oyuk.
#   Cekmece_N  : öne kayan çekmece. PİVOTU çekmecenin İÇ TABANININ ORTASI: zincir eşyayı (0, 0,005, 0)'a koyar.
#   Kapi_N     : dik menteşeli kapı. PİVOTU menteşe çizgisinin alt ucu. Unity dönüş yönünü kapının
#                pivotun hangi yanında durduğundan bulur; iki kapılı dolapta ikisi de odaya doğru açılır.
#   Kapak      : arka üst kenarından menteşeli kapak (sandık, kutu). PİVOTU arka üst kenarın ortası.
#   Surgu_N    : yana kayan kapak (Unity'de yerel +X yönünde; Blender'da -x).
# Ön yüz Blender'da -y (Unity'de +Z). Gövdenin pivotu tabanın ortası.
#
# EKSEN HATIRLATMASI: Blender (x, y, z) Unity'de (-x, z, -y) olur. Kabın içine (gövdenin köküne)
# konacak bir eşyanın Unity'deki yerel konumu, Blender'daki (x, y, z) noktasının (-x, z, -y)'sidir.

import bpy
import ortak

KAL = 0.018      # levha kalınlığı (18 mm sunta/masif)


def levha(ad, x0, x1, y0, y1, z0, z1, mat, pah=0.002):
    """Köşe noktalarıyla verilen dikdörtgen levha (kutu)."""
    return ortak.kutu(ad, (x1 - x0, y1 - y0, z1 - z0), ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), mat, pah)


def govde_levhalari(en, derin, boy, mat, taban=0.0, raflar=(), dikmeler=(), arka=True, ust=True):
    """
    Önü açık bir gövdenin levhaları: iki yan, üst, alt (taban yüksekliğinde), arka, raflar ve ara dikmeler.
    Önü y = -derin/2 düzleminde; çekmece ve kapılar o düzlemi kapatır. Levha listesi döner (birleştirilmek üzere).
    raflar: rafların ÜST yüzlerinin z'leri. dikmeler: ara dikmelerin x'leri.
    """
    x0, x1, y0, y1 = -en / 2, en / 2, -derin / 2, derin / 2
    p = [levha("Yan", x0, x0 + KAL, y0, y1, taban, boy, mat),
         levha("Yan", x1 - KAL, x1, y0, y1, taban, boy, mat),
         levha("Alt", x0 + KAL, x1 - KAL, y0, y1, taban, taban + KAL, mat)]
    if ust:
        p.append(levha("Ust", x0, x1, y0 - 0.005, y1, boy - KAL, boy, mat))
    if arka:
        p.append(levha("Arka", x0 + KAL, x1 - KAL, y1 - 0.008, y1, taban + KAL, boy - KAL, mat))
    for z in raflar:
        p.append(levha("Raf", x0 + KAL, x1 - KAL, y0 + 0.02, y1 - 0.008, z - KAL, z, mat))
    for x in dikmeler:
        p.append(levha("Dikme", x - KAL / 2, x + KAL / 2, y0, y1 - 0.008, taban + KAL, boy - KAL, mat))
    return p


def kulp(x, y, z, mat, en=0.10, tip="cubuk"):
    """Ön yüzde (y düzleminde, -y'ye taşan) kulp: 'cubuk' yatay çubuk, 'topuz' yuvarlak düğme."""
    if tip == "topuz":
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.014, location=(x, y - 0.014, z), segments=12, ring_count=8)
        n = bpy.context.active_object
        ortak.ata(n, mat)
        return [n]
    return [ortak.kutu("KulpC", (en, 0.012, 0.012), (x, y - 0.025, z), mat, 0.003),
            ortak.kutu("KulpA", (0.012, 0.025, 0.012), (x - en / 2 + 0.006, y - 0.0125, z), mat),
            ortak.kutu("KulpA", (0.012, 0.025, 0.012), (x + en / 2 - 0.006, y - 0.0125, z), mat)]


def cekmece(ad, x, z_alt, en, boy, derin, on_y, mat, on_mat, kulp_mat, kulp_tipi="cubuk"):
    """
    Çekmece: x ortası, z_alt alt kenarı, en x boy ön panosu (ön yüzü on_y'de), derin kadar içeri (+y).
    Kutu ön panodan 5 mm dar ve alçak (gövdeye sürtmesin). Pivot iç tabanın ortası.
    """
    p = [levha("On", x - en / 2, x + en / 2, on_y, on_y + KAL, z_alt, z_alt + boy, on_mat)]
    ic0, ic1 = x - en / 2 + 0.012, x + en / 2 - 0.012
    y0, y1 = on_y + KAL, on_y + derin
    zt = z_alt + 0.012
    p.append(levha("Taban", ic0, ic1, y0, y1, zt, zt + 0.008, mat, 0.0))
    yan_ust = z_alt + boy * 0.78
    p.append(levha("CYan", ic0, ic0 + 0.012, y0, y1, zt, yan_ust, mat, 0.0))
    p.append(levha("CYan", ic1 - 0.012, ic1, y0, y1, zt, yan_ust, mat, 0.0))
    p.append(levha("CArka", ic0, ic1, y1 - 0.012, y1, zt, yan_ust, mat, 0.0))
    p += kulp(x, on_y, z_alt + boy / 2, kulp_mat, en=min(0.12, en * 0.4), tip=kulp_tipi)
    n = ortak.birlestir(p, ad)
    ortak.pivotu_tasi(n, (x, (y0 + y1) / 2, zt + 0.008))
    return n


def kapi(ad, x_mentese, x_serbest, z_alt, z_ust, on_y, mat, kulp_mat, kulp_tipi="topuz", cita=True):
    """
    Dik menteşeli kapı: x_mentese kenarından x_serbest kenarına uzanan pano, ön yüzü on_y'de (kalınlığı
    +y yönünde). Kulp serbest kenara yakın. cita: ön yüzde ince çerçeve (tablalı kapı görünümü).
    Pivot menteşe çizgisinin alt ucu.
    """
    a, b = min(x_mentese, x_serbest), max(x_mentese, x_serbest)
    p = [levha("Pano", a, b, on_y, on_y + KAL, z_alt, z_ust, mat)]
    if cita:
        k = 0.035
        for (xa, xb, za, zb) in ((a + 0.02, b - 0.02, z_ust - 0.02 - 0.012, z_ust - 0.02),
                                 (a + 0.02, b - 0.02, z_alt + 0.02, z_alt + 0.02 + 0.012),
                                 (a + 0.02, a + 0.032, z_alt + 0.02, z_ust - 0.02),
                                 (b - 0.032, b - 0.02, z_alt + 0.02, z_ust - 0.02)):
            p.append(levha("Cita", xa, xb, on_y - 0.006, on_y, za, zb, mat, 0.001))
    ks = x_serbest - (0.045 if x_serbest > x_mentese else -0.045)
    p += kulp(ks, on_y, (z_alt + z_ust) / 2, kulp_mat, en=0.08, tip=kulp_tipi)
    n = ortak.birlestir(p, ad)
    ortak.pivotu_tasi(n, (x_mentese, on_y + KAL / 2, z_alt))
    return n


def kapak(ad, en, derin, kal, z_ust, mat, kulp_mat=None, y_arka=None, x=0.0):
    """
    Üstten menteşeli kapak: en x derin x kal levha, üst yüzü z_ust'te. Pivot arka üst kenar (y_arka, varsayılan
    derin/2). kulp_mat verilirse ön kenarın ortasında küçük bir tutamak.
    """
    ya = derin / 2 if y_arka is None else y_arka
    p = [levha("Kapak", x - en / 2, x + en / 2, ya - derin, ya, z_ust - kal, z_ust, mat)]
    if kulp_mat is not None:
        p.append(ortak.kutu("Tutamak", (0.07, 0.015, 0.02), (x, ya - derin - 0.007, z_ust - kal / 2), kulp_mat, 0.003))
    n = ortak.birlestir(p, ad)
    ortak.pivotu_tasi(n, (x, ya, z_ust))
    return n
