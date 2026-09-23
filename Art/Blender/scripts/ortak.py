# ortak.py
# Bütün eşya betiklerinin paylaştığı yardımcılar.
#
# NEDEN BETİKLE MODELLİYORUZ
# Graybox'ı nasıl kodla kurduysak eşyaları da öyle kuruyoruz: ölçüler kodda durur,
# bir eşyayı değiştirmek için tek sayıyı değiştirip betiği yeniden çalıştırmak yeter.
# Elle modellenen bir şeyi "biraz daha kısa yap" demek saatler alabilir; burada saniyeler.
#
# NASIL ÇALIŞTIRILIR
# Blender arayüzü açılmadan, arka planda:
#   blender -b --factory-startup --python Art/Blender/scripts/tup.py
# "-b" arayüzsüz (background) demek; "--factory-startup" kişisel Blender
# ayarlarını yok sayar, böylece betik her bilgisayarda aynı sonucu verir.
#
# PYTHON HAKKINDA KISA NOT
# Blender'ın betik dili Python. C#'tan farkları: süslü parantez yerine GİRİNTİ
# kullanılır (bir bloğun içindeki satırlar içeri itilir), satır sonunda ";" yok,
# değişken tipi yazılmaz. "def" bir fonksiyon tanımlar, "#" yorum satırıdır.

import os      # dosya yolları için
import bpy     # Blender'ın kendisi: sahne, nesneler, malzemeler hep buradan
import math    # pi, radyan dönüşümleri

# Deponun kök klasörü. Bu dosya Art/Blender/scripts/ içinde, yani üç klasör yukarısı.
KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))

# Kaynak .blend dosyaları burada durur (Unity bunları görmez).
BLEND_KLASORU = os.path.join(KOK, "Art", "Blender")

# Unity'nin okuyacağı .fbx dosyaları burada durur.
FBX_KLASORU = os.path.join(KOK, "Assets", "Models", "Props")


def sahneyi_temizle():
    """Boş bir sahneyle başla: varsayılan küp, ışık ve kamerayı sil."""
    # Nesne moduna geç; düzenleme modunda kalınırsa silme çalışmaz.
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")   # hepsini seç
    bpy.ops.object.delete()                       # sil
    # Sahneden silinen ama bellekte kalan malzeme ve meshleri de temizle;
    # yoksa betik iki kez çalışınca "Govde.001" gibi kopyalar oluşur.
    for veri in (bpy.data.meshes, bpy.data.materials):
        for oge in list(veri):
            if oge.users == 0:
                veri.remove(oge)
    # Ölçü birimi metre: Unity'de 1 birim = 1 metre, ikisi aynı olmalı.
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0


def malzeme(ad, renk, puruz=0.5, metal=0.0):
    """
    Basit bir PBR malzemesi üretir.
    renk: (kırmızı, yeşil, mavi) 0-1 arası.
    puruz (roughness): 0 ayna gibi parlak, 1 tamamen mat.
    metal (metallic): 0 boya/plastik, 1 çıplak metal.
    Unity bu değerleri FBX'ten okuyup URP malzemesine çevirir.
    """
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")   # Blender'ın standart PBR düğümü
    bsdf.inputs["Base Color"].default_value = (renk[0], renk[1], renk[2], 1.0)
    bsdf.inputs["Roughness"].default_value = puruz
    bsdf.inputs["Metallic"].default_value = metal
    return m


def ata(nesne, mat):
    """Nesneye tek bir malzeme verir (öncekileri siler)."""
    nesne.data.materials.clear()
    nesne.data.materials.append(mat)


def yumusat(nesne, aci=35):
    """
    Yüzeyleri yumuşak gölgelendirir ama keskin köşeleri korur.
    Silindir gibi yuvarlak şekiller "düz yüzeyli" (köşeli) görünmesin diye.
    aci: bu açıdan keskin kenarlar keskin kalır.
    """
    bpy.context.view_layer.objects.active = nesne
    nesne.select_set(True)
    bpy.ops.object.shade_auto_smooth(angle=math.radians(aci))
    nesne.select_set(False)


def pah(nesne, genislik=0.004, bolum=2):
    """
    Kenarlara küçük bir pah (bevel) verir. Gerçek dünyada hiçbir kenar
    bıçak gibi keskin değildir; bu ince yuvarlatma ışığı yakalar ve
    modeli "bilgisayar kutusu" olmaktan çıkarır.
    """
    mod = nesne.modifiers.new("Pah", "BEVEL")
    mod.width = genislik
    mod.segments = bolum
    mod.limit_method = "ANGLE"   # yalnızca belirli açıdan keskin kenarlara uygula


def birlestir(nesneler, ad):
    """Birden fazla parçayı tek bir nesnede birleştirir (Unity'de tek parça görünsün)."""
    bpy.ops.object.select_all(action="DESELECT")
    for n in nesneler:
        n.select_set(True)
    bpy.context.view_layer.objects.active = nesneler[0]
    # Değiştiricileri (pah vb.) önce uygula; birleştirme sırasında kaybolmasınlar.
    for n in nesneler:
        bpy.context.view_layer.objects.active = n
        for mod in list(n.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.context.view_layer.objects.active = nesneler[0]
    bpy.ops.object.join()
    sonuc = bpy.context.view_layer.objects.active
    sonuc.name = ad
    sonuc.data.name = ad
    return sonuc


def pivotu_dunya_merkezine_al(nesne):
    """
    Nesnenin pivotunu (kendi merkez noktasını) dünya merkezine, yani tabanın
    ortasına taşır. Böylece Unity'de y = 0 yazınca eşya tam yere oturur.

    DİKKAT: Blender'ın "origin_set" komutu yalnızca SEÇİLİ nesnelere uygulanır.
    İlk tüpte nesne seçili değildi, komut sessizce hiçbir şey yapmadı ve pivot
    taban halkasının ortasında kaldı; tüp Unity'de 2,7 cm yere gömüldü.
    Bu yüzden önce nesneyi seçip etkin yapıyoruz.
    """
    bpy.ops.object.select_all(action="DESELECT")
    nesne.select_set(True)
    bpy.context.view_layer.objects.active = nesne
    bpy.context.scene.cursor.location = (0, 0, 0)   # 3B imleci dünya merkezine koy
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")  # pivotu imlecin olduğu yere taşı


def kaydet_ve_disari_aktar(ad, nesneler):
    """
    Sahneyi Art/Blender/<ad>.blend olarak kaydeder, verilen nesneleri
    Assets/Models/Props/<ad>.fbx olarak Unity'ye aktarır.
    """
    os.makedirs(BLEND_KLASORU, exist_ok=True)
    os.makedirs(FBX_KLASORU, exist_ok=True)

    # Önce FBX: yalnızca eşyanın kendisi gitsin, önizleme kamerası ve ışığı değil.
    bpy.ops.object.select_all(action="DESELECT")
    for n in nesneler:
        n.select_set(True)

    bpy.ops.export_scene.fbx(
        filepath=os.path.join(FBX_KLASORU, ad + ".fbx"),
        use_selection=True,              # sadece seçili nesneler
        apply_unit_scale=True,           # metre ölçüsünü koru
        apply_scale_options="FBX_SCALE_ALL",
        # EKSEN DÖNÜŞÜMÜ: Blender'da yukarı Z, Unity'de yukarı Y.
        # Bu iki satır olmadan eşya Unity'de yan yatmış gelir.
        # SONUÇ (Unity'de ölçülerek doğrulandı): Blender (x, y, z) -> Unity (-x, z, -y).
        # Yani model dikey eksende 180° döner: Blender'da -y'ye bakan ön yüz,
        # Unity'de +Z'ye bakar. Bütün eşyalar bu kurala göre yerleştirilir.
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,       # dönüşümü meshe işle; Unity'de dönüş 0,0,0 kalsın
        mesh_smooth_type="FACE",         # yumuşatma bilgisini de gönder
        use_mesh_modifiers=True,         # pah gibi değiştiricileri uygulanmış gönder
        add_leaf_bones=False,            # iskelet yok, gereksiz kemik ekleme
    )

    # Sonra kaynak .blend dosyası (ileride elle düzeltmek istersek açılacak dosya).
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BLEND_KLASORU, ad + ".blend"))


def onizleme_al(yol, hedef_yukseklik, uzaklik=1.6):
    """
    Kontrol için tek kare görüntü alır: eşyaya çapraz yukarıdan bakan kamera,
    bir ana ışık, gri bir zemin. Bu görüntü depoya girmez, sadece gözle kontrol içindir.
    """
    sahne = bpy.context.scene

    # Zemin: eşyanın oturduğu gri düzlem (gölgesi görünsün diye).
    bpy.ops.mesh.primitive_plane_add(size=4, location=(0, 0, 0))
    zemin = bpy.context.active_object
    ata(zemin, malzeme("Onizleme_Zemin", (0.35, 0.35, 0.35), 0.9))

    # Kamera: eşyanın ortasına bakar.
    orta = hedef_yukseklik / 2
    bpy.ops.object.camera_add(location=(uzaklik * 0.8, -uzaklik, orta + uzaklik * 0.55))
    kamera = bpy.context.active_object
    iz = kamera.constraints.new("TRACK_TO")        # kamerayı bir noktaya kilitler
    bpy.ops.object.empty_add(location=(0, 0, orta))
    iz.target = bpy.context.active_object
    iz.track_axis = "TRACK_NEGATIVE_Z"
    iz.up_axis = "UP_Y"
    sahne.camera = kamera

    # Işık: sıcak bir ana ışık + dünyadan hafif ortam ışığı.
    bpy.ops.object.light_add(type="AREA", location=(1.5, -1.0, 2.5))
    isik = bpy.context.active_object
    isik.data.energy = 250
    isik.data.size = 1.5
    if sahne.world is None:
        sahne.world = bpy.data.worlds.new("Dunya")
    sahne.world.use_nodes = True
    sahne.world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.6

    # Hızlı render: EEVEE, küçük çözünürlük.
    sahne.render.engine = "BLENDER_EEVEE"
    sahne.render.resolution_x = 640
    sahne.render.resolution_y = 640
    sahne.render.filepath = yol
    bpy.ops.render.render(write_still=True)


# ---------- KISA YOLLAR ----------
# Eşya betiklerinde en çok kullanılan iki şekil. Her biri tek satırda
# "şu ölçüde, şurada, şu malzemeyle bir parça" demeyi sağlıyor.

def kutu(ad, olcu, konum, mat, pah_genislik=0.0):
    """olcu: (en x, derinlik y, yükseklik z) metre. konum: kutunun MERKEZİ."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=konum)
    n = bpy.context.active_object
    n.name = ad
    n.scale = olcu                                  # 1 m'lik küpü istenen ölçüye getir
    bpy.ops.object.transform_apply(scale=True)      # ölçeği meshe işle; pah doğru hesaplansın
    ata(n, mat)
    if pah_genislik > 0:
        pah(n, pah_genislik)
    return n


def silindir(ad, yaricap, boy, konum, mat, kose=32, pah_genislik=0.0):
    """Dik (z ekseninde) bir silindir. konum: MERKEZİ. kose: çevredeki köşe sayısı."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=kose, radius=yaricap, depth=boy, location=konum)
    n = bpy.context.active_object
    n.name = ad
    ata(n, mat)
    if pah_genislik > 0:
        pah(n, pah_genislik)
    return n


def oy(hedef, kesici):
    """
    BOOLEAN: kesicinin kapladığı hacmi hedeften siler (oyma), sonra kesiciyi atar.
    Delik, oyuk, çentik açmanın en kısa yolu.
    """
    mod = hedef.modifiers.new("Oy", "BOOLEAN")
    mod.operation = "DIFFERENCE"
    mod.object = kesici
    bpy.context.view_layer.objects.active = hedef
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(kesici, do_unlink=True)


def tek_parca(parcalar, ad, aci=35):
    """Parçaları birleştir, yumuşat, pivotu tabana al. Çoğu eşyanın son adımı."""
    n = birlestir(parcalar, ad)
    yumusat(n, aci)
    pivotu_dunya_merkezine_al(n)
    return n


def pivotu_tasi(nesne, nokta):
    """
    Pivotu verilen noktaya taşır. Menteşeli parçalar için gerekli: sayaç
    kapağının pivotu menteşe çizgisinde olmalı ki Unity'de döndürünce
    kenarından açılsın (daire kapılarındaki "Mentese" mantığının aynısı).
    """
    bpy.ops.object.select_all(action="DESELECT")
    nesne.select_set(True)
    bpy.context.view_layer.objects.active = nesne
    bpy.context.scene.cursor.location = nokta
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.context.scene.cursor.location = (0, 0, 0)


def calistir_ve_bitir(ad, nesneler, onizleme_yuksekligi, onizleme_uzakligi, onizlemede_kaldir=0.0):
    """
    Kaydet, FBX'e aktar, istenirse önizleme al ve ölçüyü yazdır. Her betiğin son satırı.
    onizlemede_kaldir: duvar eşyalarının pivotu ortada olduğu için önizlemede yarısı
    zeminin altında kalır; eşyayı yalnızca önizleme için bu kadar yukarı kaldırır.
    Dışa aktarım ÖNCE yapıldığı için Unity'ye giden dosya bundan etkilenmez.
    """
    import sys
    kaydet_ve_disari_aktar(ad, nesneler)
    for n in nesneler:
        print("OLCU", n.name, round(n.dimensions.x, 3), round(n.dimensions.y, 3), round(n.dimensions.z, 3))
    if "--onizleme" in sys.argv:
        for n in nesneler:
            n.location.z += onizlemede_kaldir
        onizleme_al(sys.argv[sys.argv.index("--onizleme") + 1], onizleme_yuksekligi, onizleme_uzakligi)
