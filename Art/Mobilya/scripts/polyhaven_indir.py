# polyhaven_indir.py
# Poly Haven'dan (CC0, https://polyhaven.com/license) seçilen mobilya modellerini 1k çözünürlükte
# FBX + dokularıyla indirir (#73). Dosyalar DEĞİŞTİRİLMEDEN Art/Mobilya/polyhaven/<kimlik>/ altına
# yazılır (kaynak kaydı; ses kayıtlarındaki kural gibi). Oyuna giren sürümü mobilya_donustur.py üretir.
#
# Kullanım:  python polyhaven_indir.py            -> listedeki hepsi (inmiş olanlar atlanır)
#            python polyhaven_indir.py Sofa_01    -> yalnızca verilenler
#
# Poly Haven'ın herkese açık API'si: /files/<kimlik> her biçimin ve çözünürlüğün adresini verir.

import json
import os
import sys
import urllib.request

KOK = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "polyhaven")
COZUNURLUK = "1k"   # karanlık sahnede 1k yeterli; model başına ~1,5 MB

MODELLER = [
    # Salon: oturma
    "Sofa_01", "sofa_02", "ArmChair_01", "GreenChair_01", "Rockingchair_01", "Ottoman_01",
    # Salon: sehpa, dolap, TV, avize
    "CoffeeTable_01", "side_table_01", "coffee_table_round_01",
    "ClassicConsole_01", "GothicCommode_01", "painted_wooden_cabinet", "vintage_wooden_drawer_01",
    "Television_01", "television_02", "Chandelier_01", "Chandelier_02",
    # Duvar: ayna, çerçeveler, saatler
    "ornate_mirror_01", "fancy_picture_frame_01", "hanging_picture_frame_02", "standing_picture_frame_01",
    "wall_clock", "mantel_clock_01",
    # Yatak odaları
    "GothicBed_01", "vintage_day_bed",
    # Diğer
    "vintage_suitcase", "throw_pillows_01", "ceramic_vase_01", "plastic_monobloc_chair_01", "painted_wooden_chair_01",
]

BASLIK = {"User-Agent": "huzur-apartmani-asset-script/1.0"}


def getir(adres):
    istek = urllib.request.Request(adres, headers=BASLIK)
    with urllib.request.urlopen(istek, timeout=120) as cevap:
        return cevap.read()


def indir(kimlik):
    hedef = os.path.join(KOK, kimlik)
    dosyalar = json.loads(getir(f"https://api.polyhaven.com/files/{kimlik}"))
    fbx = dosyalar["fbx"][COZUNURLUK]["fbx"]
    liste = [(os.path.basename(fbx["url"]), fbx["url"])]
    for yol, bilgi in fbx.get("include", {}).items():
        liste.append((yol, bilgi["url"]))
    toplam = 0
    for yol, adres in liste:
        dosya = os.path.join(hedef, yol)
        if os.path.exists(dosya):
            continue
        os.makedirs(os.path.dirname(dosya), exist_ok=True)
        veri = getir(adres)
        with open(dosya, "wb") as f:
            f.write(veri)
        toplam += len(veri)
    # Lisans ve kaynak bilgisi modelin yanında dursun.
    bilgi = json.loads(getir(f"https://api.polyhaven.com/info/{kimlik}"))
    with open(os.path.join(hedef, "kaynak.json"), "w", encoding="utf-8") as f:
        json.dump({"kimlik": kimlik, "ad": bilgi.get("name"), "yazarlar": bilgi.get("authors"),
                   "lisans": "CC0 1.0", "adres": f"https://polyhaven.com/a/{kimlik}",
                   "boyutlar_mm": bilgi.get("dimensions")}, f, ensure_ascii=False, indent=2)
    print(f"{kimlik}: {len(liste)} dosya, {toplam / 1e6:.1f} MB indi")


if __name__ == "__main__":
    for k in (sys.argv[1:] or MODELLER):
        indir(k)
