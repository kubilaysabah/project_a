# mobilya_donustur.py
# Poly Haven'dan inen modelleri (polyhaven_indir.py) oyunun düzenine çevirir (#73).
#
# Çalıştırma (başsız Blender):
#   blender -b --factory-startup --python Art/Mobilya/scripts/mobilya_donustur.py            -> hepsi
#   blender -b --factory-startup --python Art/Mobilya/scripts/mobilya_donustur.py -- Sofa_01 -> yalnızca verilen
#
# Her model için:
#   1) Parçalar adlandırılır: gövde "Govde" (tek nesne), çekmeceler "Cekmece_1.." (yukarıdan aşağı,
#      soldan sağa), kapaklar "Kapak_1..", çerçevenin tuvali "Resim" (aile fotoğrafı buraya, #69).
#   2) Başlangıç noktası (origin) yerleştirmeye göre konur: yerde duranlarda tabanın ortası,
#      duvara asılanlarda arka yüzün ortası, avizede tepesi. Kapakların noktası menteşe kenarında
#      (Unity'de AcilirParca o noktanın etrafında döndürür), çekmecelerinki kendi ortasında.
#   3) Her malzemenin dokuları üç standart dosyaya çevrilir: _renk (albedo), _normal, _ms
#      (Unity URP'nin beklediği "metallik + pürüzsüzlük": RGB metal, A = 1 - pürüzlülük).
#      Poly Haven'da dosya adları modelden modele değişiyor (rough/roughness, metal/metallic);
#      adlara değil, Blender'ın FBX'ten kurduğu malzeme düğümlerine bakıyoruz.
#   4) FBX, projenin öbür eşyalarıyla aynı ayarlarla yazılır: Blender (x, y, z) -> Unity (-x, z, -y);
#      Poly Haven modellerinin önü Blender'da -Y, yani Unity'de +Z (projenin kuralı).

import bpy
import os
import sys
import numpy as np
from mathutils import Vector, Matrix

BURASI = os.path.dirname(os.path.abspath(__file__))
KAYNAK = os.path.normpath(os.path.join(BURASI, "..", "polyhaven"))
PROJE = os.path.normpath(os.path.join(BURASI, "..", "..", ".."))
FBX_KLASORU = os.path.join(PROJE, "Assets", "Models", "Mobilya")
DOKU_KLASORU = os.path.join(FBX_KLASORU, "Dokular")

# Yerleştirme türü: "zemin" (varsayılan), "duvar" (arka yüzü duvara), "tavan" (tepesinden asılır).
DUVAR = {"ornate_mirror_01", "fancy_picture_frame_01", "hanging_picture_frame_02", "wall_clock"}
TAVAN = {"Chandelier_01", "Chandelier_02"}


def yerlesim(kimlik):
    return "duvar" if kimlik in DUVAR else "tavan" if kimlik in TAVAN else "zemin"


# ---------- YARDIMCILAR ----------

def sinir(nesneler):
    """Nesnelerin dünya uzayındaki birleşik sınır kutusu (en küçük, en büyük köşe)."""
    mn = Vector((1e9, 1e9, 1e9)); mx = Vector((-1e9, -1e9, -1e9))
    for o in nesneler:
        for v in o.data.vertices:
            w = o.matrix_world @ v.co
            mn = Vector((min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)))
            mx = Vector((max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)))
    return mn, mx


def sec(nesneler, etkin=None):
    bpy.ops.object.select_all(action='DESELECT')
    for o in nesneler:
        o.select_set(True)
    bpy.context.view_layer.objects.active = etkin or (nesneler[0] if nesneler else None)


def noktayi_koy(o, nokta):
    """Nesnenin başlangıç noktasını dünya uzayındaki 'nokta'ya taşır (mesh yerinde kalır)."""
    kay = o.matrix_world.inverted() @ nokta
    o.data.transform(Matrix.Translation(-kay))
    o.matrix_world = o.matrix_world @ Matrix.Translation(kay)


def yukari_resim(soket):
    """Bir düğüm girişine (Base Color, Roughness...) yukarı akıştan bağlanan ilk resmi bulur."""
    gorulen = set()
    kuyruk = [soket]
    while kuyruk:
        s = kuyruk.pop(0)
        for bag in s.links:
            n = bag.from_node
            if n.name in gorulen:
                continue
            gorulen.add(n.name)
            if n.type == 'TEX_IMAGE' and n.image is not None:
                return n.image
            kuyruk.extend(n.inputs)
    return None


def piksel(img):
    """Resmin piksellerini (yükseklik, genişlik, 4) float dizisi olarak verir."""
    w, h = img.size
    a = np.empty(w * h * 4, dtype=np.float32)
    img.pixels.foreach_get(a)
    return a.reshape(h, w, 4), w, h


def png_yaz(dizi, w, h, yol, renkli):
    """(h, w, 4) diziyi PNG olarak yazar. renkli=False: renk dönüşümü yapılmasın (normal, ms)."""
    img = bpy.data.images.new(os.path.basename(yol), w, h, alpha=True, float_buffer=False)
    img.colorspace_settings.name = 'sRGB' if renkli else 'Non-Color'
    img.pixels.foreach_set(np.clip(dizi, 0, 1).astype(np.float32).ravel())
    img.filepath_raw = yol
    img.file_format = 'PNG'
    img.save()
    bpy.data.images.remove(img)


def rol(malzeme_adi):
    m = malzeme_adi.lower()
    if "glass" in m: return "cam"
    if "canvas" in m or "artwork" in m: return "resim"
    if "bulb" in m or "lamps" in m: return "ampul"
    return ""


# ---------- DÖNÜŞTÜR ----------

def donustur(kimlik):
    klasor = os.path.join(KAYNAK, kimlik)
    fbxler = [f for f in os.listdir(klasor) if f.lower().endswith(".fbx")]
    if not fbxler:
        print(f"[MOBILYA] {kimlik}: FBX yok, atlandı"); return
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.join(klasor, fbxler[0]))

    # Kemik (rig) ve boş nesnelerden kurtul: meshler dünyadaki yerlerini korusun.
    for o in list(bpy.context.scene.objects):
        if o.type == 'MESH' and o.parent is not None:
            m = o.matrix_world.copy(); o.parent = None; o.matrix_world = m
    for o in list(bpy.context.scene.objects):
        if o.type != 'MESH':
            bpy.data.objects.remove(o, do_unlink=True)
    meshler = [o for o in bpy.context.scene.objects if o.type == 'MESH']

    # Dönüş ve ölçeği meshe işle (FBX 0,01 ölçek ve 90° dönüşle gelebilir).
    sec(meshler)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # Çerçevelerde tuval malzemesi ayrı bir nesne olsun ("Resim").
    for o in list(meshler):
        if len(o.material_slots) > 1 and any(rol(s.material.name) == "resim" for s in o.material_slots if s.material):
            sec([o]); bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.separate(type='MATERIAL'); bpy.ops.object.mode_set(mode='OBJECT')
    meshler = [o for o in bpy.context.scene.objects if o.type == 'MESH']

    # Referans nokta: bütün modelin sınırından.
    mn, mx = sinir(meshler)
    tur = yerlesim(kimlik)
    orta = (mn + mx) / 2
    ref = Vector((orta.x, orta.y, mn.z)) if tur == "zemin" else \
          Vector((orta.x, mx.y, orta.z)) if tur == "duvar" else Vector((orta.x, orta.y, mx.z))
    for o in meshler:
        o.data.transform(Matrix.Translation(-ref))
    bpy.context.view_layer.update()

    # Parçaları ayır: açılır parçalar ve resim; geri kalan her şey gövdeye birleşir.
    cekmeceler, kapaklar, resimler, govde = [], [], [], []
    for o in meshler:
        ad = o.name.lower()
        if o.material_slots and o.material_slots[0].material and rol(o.material_slots[0].material.name) == "resim" and len(o.material_slots) == 1:
            resimler.append(o)
        elif ("drawer" in ad or "door" in ad) and "body" not in ad:
            # "vintage_wooden_drawer_01_body": modelin adında "drawer" geçiyor ama bu gövde.
            d = o.dimensions
            # Derin parça (>15 cm) çekmecedir; ince olan kapaktır (şifonyerin "door"ları aslında çekmece).
            (cekmeceler if d.y > 0.15 else kapaklar).append(o)
        else:
            govde.append(o)

    if len(govde) > 1:
        sec(govde); bpy.ops.object.join()
    g = bpy.context.view_layer.objects.active if len(govde) > 1 else govde[0]
    g.name = "Govde"; g.data.name = "Govde"
    noktayi_koy(g, Vector((0, 0, 0)))

    def sira(o):
        c = sinir([o]); m = (c[0] + c[1]) / 2
        return (-round(m.z, 2), round(m.x, 2))

    for i, o in enumerate(sorted(cekmeceler, key=sira)):
        o.name = f"Cekmece_{i + 1}"; o.data.name = o.name
        a, b = sinir([o]); noktayi_koy(o, (a + b) / 2)
    for i, o in enumerate(sorted(kapaklar, key=sira)):
        o.name = f"Kapak_{i + 1}"; o.data.name = o.name
        a, b = sinir([o]); m = (a + b) / 2
        # Menteşe dış kenarda: gövdenin ortasından uzak olan dikey kenar.
        x = a.x if m.x < 0 else b.x
        noktayi_koy(o, Vector((x, b.y, m.z)))
    for i, o in enumerate(resimler):
        o.name = "Resim" if i == 0 else f"Resim_{i + 1}"; o.data.name = o.name
        a, b = sinir([o]); noktayi_koy(o, (a + b) / 2)

    # ---------- MALZEMELER VE DOKULAR ----------
    os.makedirs(DOKU_KLASORU, exist_ok=True)
    malzemeler = []
    for o in bpy.context.scene.objects:
        for s in o.material_slots:
            if s.material and s.material not in malzemeler:
                malzemeler.append(s.material)
    for i, m in enumerate(malzemeler):
        r = rol(m.name)
        yeni = f"{kimlik}__{i}" + (f"_{r}" if r else "")
        bsdf = next((n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'), None) if m.use_nodes else None
        m.name = yeni
        if bsdf is None or r == "cam":
            continue   # cam Unity'de ortak koyu cam malzemesini alır
        renk = yukari_resim(bsdf.inputs["Base Color"])
        if renk is not None:
            p, w, h = piksel(renk)
            png_yaz(p, w, h, os.path.join(DOKU_KLASORU, yeni + "_renk.png"), True)
        nrm = yukari_resim(bsdf.inputs["Normal"])
        if nrm is not None:
            nrm.colorspace_settings.name = 'Non-Color'
            p, w, h = piksel(nrm)
            p[..., 3] = 1.0
            png_yaz(p, w, h, os.path.join(DOKU_KLASORU, yeni + "_normal.png"), False)
        pur = yukari_resim(bsdf.inputs["Roughness"])
        met = yukari_resim(bsdf.inputs["Metallic"])
        if pur is not None or met is not None:
            kaynak = pur or met
            w, h = kaynak.size
            ms = np.zeros((h, w, 4), dtype=np.float32)
            if met is not None and tuple(met.size) == (w, h):
                met.colorspace_settings.name = 'Non-Color'
                mp, _, _ = piksel(met); ms[..., 0:3] = mp[..., 0:1]
            if pur is not None:
                pur.colorspace_settings.name = 'Non-Color'
                pp, _, _ = piksel(pur); ms[..., 3] = 1.0 - pp[..., 0]
            else:
                ms[..., 3] = 1.0 - float(bsdf.inputs["Roughness"].default_value)
            png_yaz(ms, w, h, os.path.join(DOKU_KLASORU, yeni + "_ms.png"), False)

    # ---------- FBX ----------
    os.makedirs(FBX_KLASORU, exist_ok=True)
    sec([o for o in bpy.context.scene.objects if o.type == 'MESH'])
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(FBX_KLASORU, kimlik + ".fbx"),
        use_selection=True, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True,
        object_types={'MESH'}, use_mesh_modifiers=True, mesh_smooth_type='FACE',
        path_mode='STRIP', embed_textures=False)
    parcalar = sorted(o.name for o in bpy.context.scene.objects if o.type == 'MESH')
    print(f"[MOBILYA] {kimlik}: {tur}, boyut {tuple(round(v, 2) for v in (mx - mn))}, parçalar {parcalar}, "
          f"malzemeler {[m.name for m in malzemeler]}")


if __name__ == "__main__":
    istenen = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sorted(os.listdir(KAYNAK))
    for k in istenen:
        if os.path.isdir(os.path.join(KAYNAK, k)):
            donustur(k)
