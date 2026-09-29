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
DUVAR = {"ornate_mirror_01", "fancy_picture_frame_01", "hanging_picture_frame_02", "vintage_telephone_wall_clock"}
TAVAN = {"Chandelier_01", "Chandelier_02"}
# Tek parça kalacaklar (#81): açılır parçası olsa da oyunda açılmaz, gövdeye birleşir. Ocağın fırın kapağı
# alttan menteşelidir; yan menteşeli dolap kapağı gibi açılsaydı tuhaf dururdu.
TEK_PARCA = {"electric_stove"}

# BULMACA EŞYALARININ YENİ GÖRÜNÜMÜ (#81, Şahan'ın seçimi). Bu iki model normal mobilya olarak da çevrilir, ama
# ayrıca eski bulmaca eşyasının yerini alacak bir kopya Assets/Models/Props/'a yazılır: aynı dosya adı, aynı
# parça adları, eskisine yakın ölçü. Böylece kilitler, kapaklar ve içine konan anahtarlar kod değişmeden çalışır.
#   olcu: hedef (en, derinlik, boy) metre; olcek: tek sayı (oranı bozmadan küçült).
BULMACA = {
    # Oymalı dolap -> büfe: en ve derinlik eski büfenin (1,40 x 0,45), boy 1,90. Alt iki kapak kilitli kapılar
    # (Kapi_1 sol, Kapi_2 sağ); üst kapaklar gövdeye birleşir. Alt kapakların arkası oyulur: anahtar orada durur.
    "GothicCabinet_01": {"ad": "Bufe", "olcu": (1.40, 0.45, 1.90)},
    # Ahşap sandık -> oyuncak kutusu: eni eski kutunun (0,60), oranı korunur. Kapak arka üst kenardan açılır.
    "treasure_chest": {"ad": "Oyuncak_Kutusu", "en": 0.60},
    # (Vitrinli dolap -> gardırop kopyası 2026-09-29'da kaldırıldı: Şahan Blender'da iki kapılı aynalı ceviz
    # gardırop seçti, Art/Blender/scripts/gardirop.py. Burada kalsaydı yeniden çalışınca onu ezerdi.)
}


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
        elif ("drawer" in ad or "door" in ad or ad.endswith("_lid")) and "body" not in ad and kimlik not in TEK_PARCA:
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
            # 16 bitlik PNG (Blender onu "float" resim olarak açar) pikselleri doğrusal (linear) verir; 8 bitlik
            # sRGB'ye olduğu gibi yazılınca renk iki kez koyulaşıyordu: vitrinli dolap simsiyah çıktı (#81).
            # Doğrusal değeri sRGB'ye çeviririz (standart sRGB eğrisi).
            if renk.is_float:
                c = np.clip(p[..., :3], 0.0, 1.0)
                p[..., :3] = np.where(c <= 0.0031308, c * 12.92, 1.055 * np.power(c, 1 / 2.4) - 0.055)
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
    if kimlik in BULMACA:
        bulmaca_esyasi(kimlik)


# ---------- BULMACA EŞYASI KOPYASI (#81) ----------
def kutu_oy(hedef, mn, mx):
    """hedef nesneden (mn, mx) köşeli kutunun hacmini oyar (boolean fark)."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(mn + mx) / 2)
    k = bpy.context.active_object
    k.scale = mx - mn
    sec([k]); bpy.ops.object.transform_apply(scale=True)
    mod = hedef.modifiers.new("Oy", "BOOLEAN"); mod.operation = "DIFFERENCE"; mod.object = k
    sec([hedef]); bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(k, do_unlink=True)


def bulmaca_esyasi(kimlik):
    ayar = BULMACA[kimlik]
    nesneler = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    # Her nesnenin dönüşümünü meshe işle: hepsi dünya koordinatında, birim dönüşümle kalsın.
    for o in nesneler:
        o.data.transform(o.matrix_world); o.matrix_world = Matrix.Identity(4)
    mn, mx = sinir(nesneler)
    boy = mx - mn
    if "olcu" in ayar:
        o3 = ayar["olcu"]; sx, sy, sz = o3[0] / boy.x, o3[1] / boy.y, o3[2] / boy.z
    else:
        k = ayar.get("olcek") or ayar["en"] / boy.x; sx = sy = sz = k
    for o in nesneler:
        o.data.transform(Matrix.Diagonal((sx, sy, sz, 1)))
    bpy.context.view_layer.update()
    ad = ayar["ad"]
    govde = bpy.data.objects["Govde"]
    ihracat = []
    if ad == "Bufe":
        # Dört kapaktan (dönüştürücü bazılarını derinliğinden "Cekmece" sanabilir) alçakta olan ikisi kilitli
        # kapılar; soldan sağa sıralanır. Üsttekiler gövdeye birleşir.
        kapaklar4 = sorted([o for o in nesneler if o.name.startswith("Kapak_") or o.name.startswith("Cekmece_")],
                           key=lambda o: (sinir([o])[0].z + sinir([o])[1].z))
        ust = kapaklar4[2:]
        alt = sorted(kapaklar4[:2], key=lambda o: (sinir([o])[0].x + sinir([o])[1].x))
        sec([govde] + ust, govde); bpy.ops.object.join()
        govde = bpy.context.view_layer.objects.active
        a0, a1 = sinir(alt)
        g0, g1 = sinir([govde])
        taban = a0.z
        # Alt kapakların arkası oyulur: önü açık, arka ve yanlarda 4-5 cm duvar kalır.
        kutu_oy(govde, Vector((g0.x + 0.05, g0.y - 0.05, taban)), Vector((g1.x - 0.05, g1.y - 0.04, a1.z - 0.02)))
        print("IC_TABAN", ad, round(taban, 3))
        for i, o in enumerate(alt):
            b0, b1 = sinir([o])
            o.name = f"Kapi_{i + 1}"; o.data.name = o.name
            # Menteşe dış kenarda: sol kapının solu, sağ kapının sağı; kapağın ön yüzünde.
            noktayi_koy(o, Vector((b0.x if i == 0 else b1.x, b0.y, 0)))
        ihracat = alt
    elif ad == "Oyuncak_Kutusu":
        # Kapak derin olduğu için dönüştürücü onu "Cekmece" saymış olabilir: gövde dışındaki tek parça kapaktır.
        kapak = next(o for o in nesneler if o.name.startswith("Kapak_") or o.name.startswith("Cekmece_"))
        g0, g1 = sinir([govde])
        k0, k1 = sinir([kapak])
        kutu_oy(govde, Vector((g0.x + 0.05, g0.y + 0.03, 0.03)), Vector((g1.x - 0.05, g1.y - 0.03, k0.z + 0.02)))
        kapak.name = "Kapak"; kapak.data.name = "Kapak"
        noktayi_koy(kapak, Vector(((k0.x + k1.x) / 2, k1.y, k0.z)))   # menteşe: arka üst kenar
        ihracat = [kapak]
    else:
        sec(nesneler, govde); bpy.ops.object.join()
        govde = bpy.context.view_layer.objects.active
    govde.name = ad; govde.data.name = ad
    noktayi_koy(govde, Vector((0, 0, 0)))
    sec([govde] + ihracat)
    props = os.path.join(PROJE, "Assets", "Models", "Props")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(props, ad + ".fbx"),
        use_selection=True, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True,
        object_types={'MESH'}, use_mesh_modifiers=True, mesh_smooth_type='FACE',
        path_mode='STRIP', embed_textures=False)
    g0, g1 = sinir([govde] + ihracat)
    print(f"[BULMACA] {kimlik} -> Props/{ad}.fbx, boyut {tuple(round(v, 2) for v in (g1 - g0))}, "
          f"parçalar {[o.name for o in [govde] + ihracat]}")


if __name__ == "__main__":
    istenen = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sorted(os.listdir(KAYNAK))
    for k in istenen:
        if os.path.isdir(os.path.join(KAYNAK, k)):
            donustur(k)
