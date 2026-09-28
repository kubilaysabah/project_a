# doku_indir.py
# Poly Haven'dan (CC0, https://polyhaven.com/license) bir duvar ya da zemin dokusunu projenin düzeninde indirir:
# 1K renk (diffuse) ve 1K normal (GL) haritası, JPG, Assets/Textures/<kimlik>_renk.jpg ve _normal.jpg.
# DokuKurucu bu adlarla arar (KatKaplamalari'nda kimlik yazmak yeter). Kaynak ve yazar LISANSLAR.md'ye elle eklenir.
#
# Kullanım:  python doku_indir.py peeling_painted_wall yellow_plaster
# Poly Haven'ın herkese açık API'si: /files/<kimlik> her haritanın her çözünürlükteki adresini verir.

import json
import os
import sys
import urllib.request

HEDEF = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "Assets", "Textures"))
BASLIK = {"User-Agent": "huzur-apartmani-asset-script/1.0"}


def getir(adres):
    with urllib.request.urlopen(urllib.request.Request(adres, headers=BASLIK), timeout=120) as cevap:
        return cevap.read()


def indir(kimlik):
    dosyalar = json.loads(getir(f"https://api.polyhaven.com/files/{kimlik}"))
    # Harita adları: "Diffuse" (renk) ve "nor_gl" (OpenGL normal; Unity de bu yönü bekler).
    for harita, ek in (("Diffuse", "renk"), ("nor_gl", "normal")):
        adres = dosyalar[harita]["1k"]["jpg"]["url"]
        veri = getir(adres)
        with open(os.path.join(HEDEF, f"{kimlik}_{ek}.jpg"), "wb") as f:
            f.write(veri)
        print(f"{kimlik}_{ek}.jpg: {len(veri) / 1e6:.2f} MB")


if __name__ == "__main__":
    for k in sys.argv[1:]:
        indir(k)
