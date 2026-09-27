# foto_uret.py
# Dairelerdeki aile fotoğraflarının adaylarını yerel ComfyUI ile üretir (#69).
#
# Model: Stable Diffusion XL 1.0 base (stabilityai, CreativeML Open RAIL++-M; üretilen görüntüler
# ticari kullanıma uygun). ComfyUI bu makinede taşınabilir AMD sürümüyle çalışır
# (C:\Users\sahan\AI\ComfyUI_windows_portable, yalnızca 127.0.0.1:8188). Bkz. ../LISANSLAR.md.
#
# Kullanım (ComfyUI açıkken):
#   python foto_uret.py            -> bütün fotoğraflar, her birinden 3 aday
#   python foto_uret.py kat4 kat7  -> yalnızca verilenler
# Adaylar Art/Fotograflar/aday/<ad>_<tohum>.png olarak yazılır; seçilen aday foto_isle.py ile
# eskitilip oyuna girer.
#
# Korku katmanı (#69): ilk üç kat normal; 4. kattan sonra arka planda uzun, yüzü seçilmeyen bir adam
# ("dokuzuncu kattaki adam") her katta biraz daha önde.

import json
import os
import sys
import time
import urllib.request

SUNUCU = "http://127.0.0.1:8188"
BURASI = os.path.dirname(os.path.abspath(__file__))
ADAY = os.path.normpath(os.path.join(BURASI, "..", "aday"))
TOHUMLAR = (1994, 1974, 9)

ORTAK = ("1990s Turkish amateur family snapshot, faded color film photograph, 35mm Kodak film grain, "
         "on-camera flash, slightly soft focus, warm color cast, authentic candid photo, ")
OLUMSUZ = ("text, watermark, logo, signature, cartoon, painting, illustration, 3d render, anime, "
           "modern clothing, smartphone, deformed face, extra fingers, extra limbs, blurry eyes, oversaturated")

# ad: (en, boy, açıklama)
FOTOGRAFLAR = {
    "kat1": (1152, 896, "a Turkish family of four sitting together on a patterned sofa in a modest apartment living room "
                        "during a holiday, father with a mustache, mother, a teenage girl and a small boy, lace doilies, "
                        "wall carpet, smiling at the camera"),
    "kat2": (896, 1152, "an elderly Turkish man with a white mustache in a cardigan standing beside a middle-aged "
                        "apartment janitor in a grey work coat in front of an old apartment building entrance door, "
                        "daytime, both looking at the camera"),
    "kat3": (1152, 896, "a young Turkish mother and father with their seven year old son behind a birthday cake with candles "
                        "on a kitchen table, the boy holding a crayon drawing, balloons, evening, flash"),
    "kat4": (1152, 896, "a Turkish middle-aged couple standing in a dim, half-furnished apartment living room, a dark open "
                        "doorway behind them, in the darkness of the doorway a very tall thin man stands barely visible, "
                        "his face in shadow, unsettling"),
    "kat5": (896, 1152, "a Turkish apartment janitor in his forties with a thick black mustache, wearing a dark work jacket, "
                        "holding a metal toolbox, standing on the landing of a concrete stairwell, on the stairs above him "
                        "a tall man in a dark coat stands half hidden in shadow, face not visible"),
    "kat6": (1152, 896, "three Turkish men of different ages sitting at a dinner table set with four plates in a dark "
                        "dining room, the fourth chair is empty, behind the empty chair stands a tall man in a dark suit "
                        "whose face is blurred and out of frame, eerie, flash photo"),
    "kat7": (896, 1152, "a Turkish apartment janitor with a black mustache standing in front of closed cream-colored elevator "
                        "doors in a dark stairwell, directly behind him a tall man in a dark coat stands very close, "
                        "the tall man's face is hidden in darkness, creepy"),
    "kat8": (1152, 896, "a group of five Turkish men in 1970s suits and ties posing in front of a new apartment building "
                        "entrance at an opening ceremony with a red ribbon, black and white faded photo, in the back row "
                        "behind them stands a much taller man whose face is blurred"),
    "kapici": (1152, 896, "three small passport photos of three different Turkish men side by side on a plain background, "
                          "an older man with a grey mustache, a man in his thirties, a man in his forties with a black "
                          "mustache, 1990s vesikalik ID photos, faded"),
}


def istek(yol, veri=None):
    r = urllib.request.Request(SUNUCU + yol, data=json.dumps(veri).encode() if veri is not None else None,
                               headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(r, timeout=600) as c:
        return json.loads(c.read())


def akis(aciklama, en, boy, tohum, onek):
    """ComfyUI'nin API biçiminde en basit metinden görüntü akışı."""
    return {
        "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": "sd_xl_base_1.0.safetensors"}},
        "2": {"class_type": "CLIPTextEncode", "inputs": {"text": ORTAK + aciklama, "clip": ["1", 1]}},
        "3": {"class_type": "CLIPTextEncode", "inputs": {"text": OLUMSUZ, "clip": ["1", 1]}},
        "4": {"class_type": "EmptyLatentImage", "inputs": {"width": en, "height": boy, "batch_size": 1}},
        "5": {"class_type": "KSampler", "inputs": {"model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0],
                                                   "latent_image": ["4", 0], "seed": tohum, "steps": 30, "cfg": 6.5,
                                                   "sampler_name": "dpmpp_2m", "scheduler": "karras", "denoise": 1.0}},
        "6": {"class_type": "VAEDecode", "inputs": {"samples": ["5", 0], "vae": ["1", 2]}},
        "7": {"class_type": "SaveImage", "inputs": {"images": ["6", 0], "filename_prefix": onek}},
    }


def uret(ad):
    en, boy, aciklama = FOTOGRAFLAR[ad]
    os.makedirs(ADAY, exist_ok=True)
    for tohum in TOHUMLAR:
        kimlik = istek("/prompt", {"prompt": akis(aciklama, en, boy, tohum, f"huzur_{ad}_{tohum}")})["prompt_id"]
        while True:
            gecmis = istek(f"/history/{kimlik}")
            if kimlik in gecmis and gecmis[kimlik].get("outputs"):
                break
            time.sleep(1.5)
        for cikti in gecmis[kimlik]["outputs"].values():
            for g in cikti.get("images", []):
                adres = f"{SUNUCU}/view?filename={g['filename']}&subfolder={g['subfolder']}&type={g['type']}"
                with urllib.request.urlopen(adres) as c:
                    open(os.path.join(ADAY, f"{ad}_{tohum}.png"), "wb").write(c.read())
        print(f"[FOTO] {ad} tohum {tohum} hazır", flush=True)


if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(FOTOGRAFLAR)):
        uret(ad)
