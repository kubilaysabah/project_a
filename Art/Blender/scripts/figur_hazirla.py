# figur_hazirla.py — Sketchfab'dan indirilen üç harfli figürlerini (#134) oyuna hazırlar.
#
# Girdi:  Art/Figurler/indirilen/<N>/scene.gltf (+ scene.bin, textures/)   — Şahan indirir, depoya girmez.
# Çıktı:  Assets/Models/Figurler/Cin_<N>/Cin_<N>.fbx + yanına kopyalanan dokular.
#
# Her figürde aynı dört iş:
#   1. İÇE AL: Blender'ın glTF okuyucusu (glTF +Z ön → Blender -Y ön; bizim FBX kuralıyla Unity'de +Z ön olur).
#   2. BİRLEŞTİR + İNDİRGE: bütün mesh parçaları tek nesnede toplanır; Decimate değiştiricisi üçgen sayısını
#      HEDEF_UCGEN'e indirir (2 milyonluk cadı da 40 bine iner; oyun ortamında 5-8 m'den bakılan bir figür
#      için yeter, dokular ayrıntıyı taşır).
#   3. ÖLÇÜ + PİVOT: boy HEDEF_BOY'a ölçeklenir (Sketchfab modelleri cm, inç ya da rastgele ölçekte gelir),
#      pivot ayakların ortasına (x, y merkez; z en alt) konur — Gölge şablonunun kuralı: kök yerde durur.
#   3b. KOLLARI İNDİR (#142, yalnız POZ tablosundakiler): A/T pozunda gelen figürün kolları omuz çevresinde
#      aşağı döndürülür. İskelet yok; her köşe kendi ağırlığı kadar (0-1) döner: omuzda 0, kolun geri kalanında
#      1, arada yumuşak geçiş. Böylece omuz dikişi yırtılmaz, kol bükülür gibi iner.
#   4. DIŞA AKTAR: ortak.kaydet_ve_disari_aktar ile aynı eksen kuralı (-Z ileri, Y yukarı, dönüşüm meshe),
#      dokular FBX'in yanına kopyalanır (path_mode COPY, gömme yok: Unity yanındaki dosyaları okur).
#
# Çalıştır:  blender -b --factory-startup --python Art/Blender/scripts/figur_hazirla.py            (hepsi)
#            blender -b --factory-startup --python Art/Blender/scripts/figur_hazirla.py -- 3 5     (yalnızca 3 ve 5)
import os, sys, glob, shutil, math
KOK = os.path.dirname(os.path.abspath(__file__))
sys.path.append(KOK)
import bpy
import ortak

PROJE = os.path.normpath(os.path.join(KOK, "..", "..", ".."))
GIRDI = os.path.join(PROJE, "Art", "Figurler", "indirilen")
CIKTI = os.path.join(PROJE, "Assets", "Models", "Figurler")
HEDEF_UCGEN = 40000
HEDEF_BOY = {1: 1.85, 2: 1.95, 3: 1.80, 4: 1.70, 5: 1.78, 6: 1.78}   # metre; cadı ve ruh biraz uzun, nine kısa

# #142 (B4): kolları indirilecek figürler. omuz: omuz ekleminin (x, yükseklik oranı); aci: indirme açısı (derece).
#   "kapsul": kol, omuzdan ele uzanan bir çubuğun çevresindeki köşeler (r metre); A pozunda kol gövdeden ayrık
#             dururken eteğe dokunmadan yalnız kolu seçer. el: elin (x, yükseklik oranı).
#   "bant":   omuz yüksekliğinin üstünde (zalt oranından yukarı) ve omuzdan dışarıdaki her şey (geniş yenler dahil);
#             T pozunda kollar yatay olduğu için bu bantta gövdeden başka yalnız kol ve yen vardır.
POZ = {
    3: dict(yontem="kapsul", omuz=(0.17, 0.82), el=(0.62, 0.43), r=0.11, aci=24.0),   # el: parmak uçlarının biraz ötesi
    6: dict(yontem="bant", omuz=(0.17, 0.82), zalt=0.70, aci=62.0),
}

# #143 (madde 11): OTURAN kopya. Dokuzuncu katın sofrasında oturan figür (Şahan: Cin_4). Aynı ağdan ikinci bir
# FBX (Cin_N_Oturan.fbx, aynı klasöre: dokular paylaşılır): kalça ekleminin altındaki her şey 90° öne, dizin
# altındaki her şey oradan 90° aşağı döndürülür (ağırlıklı, eklem çevresinde yumuşak bant). Etek de kucağa
# yatar ve dizden sarkar: oturan bir elbise gibi okunur. Oranlar boyun yüzdesi: kalça eklemi, diz eklemi.
OTURAN = {4: dict(kalca=0.47, diz=0.27)}

secilenler = [int(a) for a in sys.argv[sys.argv.index("--") + 1:]] if "--" in sys.argv else sorted(HEDEF_BOY)


def gltf_bul(n):
    for ad in ("scene.gltf", "scene.glb"):
        yol = os.path.join(GIRDI, str(n), ad)
        if os.path.exists(yol): return yol
    adaylar = glob.glob(os.path.join(GIRDI, str(n), "**", "*.gl*"), recursive=True)
    return adaylar[0] if adaylar else None


def yumusak(a, b, x):
    # smoothstep: x, a'dan b'ye giderken 0'dan 1'e S biçiminde yükselir (uçlarda yavaş, ortada hızlı).
    if b == a: return 1.0 if x >= b else 0.0
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


def kollari_indir(f, n, boy):
    # #142 (B4): kolları omuz çevresinde döndürür. Ayak ortası (0, 0, 0), ön -Y, yukarı +Z (pivot işlendi).
    p = POZ[n]
    ox, oz = p["omuz"][0], p["omuz"][1] * boy
    aci = math.radians(p["aci"])
    for v in f.data.vertices:
        x, y, z = v.co.x, v.co.y, v.co.z
        taraf = 1.0 if x >= 0 else -1.0          # sağ kol +x, sol kol -x (ayna)
        ax = abs(x)
        if p["yontem"] == "kapsul":
            # Omuzdan ele çubuk: köşenin çubuğa uzaklığı r'den küçükse kolun parçası. t: çubuk boyunca 0 (omuz) - 1 (el).
            ex, ez = p["el"][0], p["el"][1] * boy
            dx, dz = ex - ox, ez - oz
            uz2 = dx * dx + dz * dz
            t = ((ax - ox) * dx + (z - oz) * dz) / uz2
            tk = max(0.0, min(1.0, t))
            px, pz = ox + dx * tk, oz + dz * tk
            uzak = math.hypot(ax - px, z - pz)
            if uzak > p["r"] or t < -0.05: continue
            w = yumusak(0.0, 0.22, t) * (1.0 - yumusak(p["r"] * 0.7, p["r"], uzak))
        else:
            w = yumusak(ox - 0.02, ox + 0.12, ax) * yumusak(p["zalt"] * boy, p["zalt"] * boy + 0.06, z)
        if w <= 0.0: continue
        # Omuz eklemi çevresinde, ön-arka ekseni (Y) etrafında döndürme. Sağda +açı kolu aşağı indirir
        # (x ekseni -z'ye döner), solda ayna: -açı. Ağırlık açıyı ölçekler (açı karışımı: omuzda kıvrım yırtılmaz).
        a = aci * w * taraf
        rx, rz = x - ox * taraf, z - oz
        v.co.x = ox * taraf + rx * math.cos(a) + rz * math.sin(a)
        v.co.z = oz - rx * math.sin(a) + rz * math.cos(a)
    print(f"POZ Cin_{n}: kollar {p['aci']:.0f} derece indirildi ({p['yontem']})")


def oturt(f, boy, kalca_oran, diz_oran):
    # Pivot ayaklarda (0, 0, 0), ön -Y, yukarı +Z. Önce kalça, sonra diz; sınıflandırma ORİJİNAL yüksekliğe göre.
    zk, zd = kalca_oran * boy, diz_oran * boy
    def ort_y(z0, z1):
        ys = [v.co.y for v in f.data.vertices if z0 <= v.co.z <= z1]
        return sum(ys) / len(ys) if ys else 0.0
    yk = ort_y(zk - 0.03, zk + 0.03)           # kalça ekseninin ön-arka yeri (bandın ortalaması)
    yd = ort_y(zd - 0.03, zd + 0.03)           # diz ekseni
    # Diz pivotunun kalça dönüşünden SONRAKİ yeri (ağırlık 1 ile): (dy, dz) -> (dy + dz, -dy), dz = zd - zk < 0.
    dy_d, dz_d = yd - yk, zd - zk
    yd2, zd2 = yk + dy_d + dz_d, zk - dy_d
    for v in f.data.vertices:
        z0 = v.co.z
        w1 = 1.0 - yumusak(zk - 0.04, zk + 0.04, z0)    # kalçanın altında 1, üstünde 0
        w2 = 1.0 - yumusak(zd - 0.04, zd + 0.04, z0)    # dizin altında 1
        if w1 <= 0.0: continue
        a = math.radians(90.0) * w1
        dy, dz = v.co.y - yk, v.co.z - zk
        y1 = yk + dy * math.cos(a) + dz * math.sin(a)
        z1 = zk - dy * math.sin(a) + dz * math.cos(a)
        if w2 > 0.0:
            b = math.radians(-90.0) * w2
            dy, dz = y1 - yd2, z1 - zd2
            y1 = yd2 + dy * math.cos(b) + dz * math.sin(b)
            z1 = zd2 - dy * math.sin(b) + dz * math.cos(b)
        v.co.y, v.co.z = y1, z1
    # Ayaklar yere, pivot kalçanın altına: otururken kök sandalyenin önündeki zeminde, kalçanın tam altında durur.
    zmin = min(v.co.z for v in f.data.vertices)
    for v in f.data.vertices:
        v.co.z -= zmin
        v.co.y -= yk
    print(f"POZ oturan: kalça {zk:.2f} m -> {zk - zmin:.2f} m, diz {zd:.2f}, en alt {zmin:.3f} m kaydırıldı")


def hazirla(n):
    yol = gltf_bul(n)
    if yol is None:
        print(f"ATLANDI {n}: Art/Figurler/indirilen/{n}/ altında glTF yok"); return
    ortak.sahneyi_temizle()
    bpy.ops.import_scene.gltf(filepath=yol)
    meshler = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshler:
        print(f"ATLANDI {n}: mesh yok"); return
    # Ebeveyn dönüşümleri meshe işlensin (glTF sahneleri iç içe düğümlerle gelir), sonra tek parça.
    bpy.ops.object.select_all(action="DESELECT")
    for o in meshler: o.select_set(True)
    bpy.context.view_layer.objects.active = meshler[0]
    bpy.ops.object.parent_clear(type="CLEAR_KEEP_TRANSFORM")
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.join()
    f = bpy.context.active_object
    f.name = f"Cin_{n}"
    # Diğer düğümler (boş nesneler, kameralar, ışıklar) gereksiz.
    for o in list(bpy.context.scene.objects):
        if o != f: bpy.data.objects.remove(o)
    # İndirgeme: üçgen sayısı hedefin üstündeyse Decimate (collapse) oranı hedef/şimdiki.
    bpy.ops.object.select_all(action="DESELECT"); f.select_set(True); bpy.context.view_layer.objects.active = f
    ucgen = sum(len(p.vertices) - 2 for p in f.data.polygons)
    if ucgen > HEDEF_UCGEN:
        d = f.modifiers.new("Indirge", "DECIMATE")
        d.ratio = HEDEF_UCGEN / ucgen
        d.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=d.name)
    # Boy ve pivot: sınır kutusu dünya koordinatında.
    koseler = [f.matrix_world @ v.co for v in f.data.vertices]
    zmin = min(v.z for v in koseler); zmax = max(v.z for v in koseler)
    boy = zmax - zmin
    olcek = HEDEF_BOY[n] / boy if boy > 1e-6 else 1.0
    f.scale = (olcek, olcek, olcek)
    bpy.ops.object.transform_apply(scale=True)
    koseler = [f.matrix_world @ v.co for v in f.data.vertices]
    xo = (min(v.x for v in koseler) + max(v.x for v in koseler)) / 2
    yo = (min(v.y for v in koseler) + max(v.y for v in koseler)) / 2
    zmin = min(v.z for v in koseler)
    # Pivotu ayak ortasına: mesh'i kaydır, nesneyi orijine koy.
    for v in f.data.vertices:
        v.co = f.matrix_world @ v.co
        v.co.x -= xo; v.co.y -= yo; v.co.z -= zmin
    f.matrix_world.identity()
    if n in POZ: kollari_indir(f, n, HEDEF_BOY[n])   # #142 (B4)
    ortak.yumusat(f, 45)
    # Dışa aktar: kendi klasörüne, dokular yanına.
    klasor = os.path.join(CIKTI, f"Cin_{n}")
    os.makedirs(klasor, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT"); f.select_set(True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(klasor, f"Cin_{n}.fbx"), use_selection=True, apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                             mesh_smooth_type="FACE", use_mesh_modifiers=True, add_leaf_bones=False,
                             path_mode="COPY", embed_textures=False)
    # #143: oturan kopya (yalnız OTURAN tablosundakiler). Ayakta duran dosya yazıldıktan sonra aynı ağ bükülür ve
    # ikinci bir FBX olarak yanına kaydedilir; dokular aynı klasöre bir daha kopyalanır (aynı dosyalar).
    if n in OTURAN:
        oturt(f, HEDEF_BOY[n], OTURAN[n]["kalca"], OTURAN[n]["diz"])
        f.name = f"Cin_{n}_Oturan"
        bpy.ops.export_scene.fbx(filepath=os.path.join(klasor, f"Cin_{n}_Oturan.fbx"), use_selection=True, apply_unit_scale=True,
                                 apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                                 mesh_smooth_type="FACE", use_mesh_modifiers=True, add_leaf_bones=False,
                                 path_mode="COPY", embed_textures=False)
    ucgen2 = sum(len(p.vertices) - 2 for p in f.data.polygons)
    print(f"OLCU Cin_{n} ucgen {ucgen} -> {ucgen2} boy {boy:.2f} -> {HEDEF_BOY[n]:.2f} m, dokular {klasor}")


for n in secilenler:
    hazirla(n)
