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

secilenler = [int(a) for a in sys.argv[sys.argv.index("--") + 1:]] if "--" in sys.argv else sorted(HEDEF_BOY)


def gltf_bul(n):
    for ad in ("scene.gltf", "scene.glb"):
        yol = os.path.join(GIRDI, str(n), ad)
        if os.path.exists(yol): return yol
    adaylar = glob.glob(os.path.join(GIRDI, str(n), "**", "*.gl*"), recursive=True)
    return adaylar[0] if adaylar else None


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
    ortak.yumusat(f, 45)
    # Dışa aktar: kendi klasörüne, dokular yanına.
    klasor = os.path.join(CIKTI, f"Cin_{n}")
    os.makedirs(klasor, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT"); f.select_set(True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(klasor, f"Cin_{n}.fbx"), use_selection=True, apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                             mesh_smooth_type="FACE", use_mesh_modifiers=True, add_leaf_bones=False,
                             path_mode="COPY", embed_textures=False)
    ucgen2 = sum(len(p.vertices) - 2 for p in f.data.polygons)
    print(f"OLCU Cin_{n} ucgen {ucgen} -> {ucgen2} boy {boy:.2f} -> {HEDEF_BOY[n]:.2f} m, dokular {klasor}")


for n in secilenler:
    hazirla(n)
