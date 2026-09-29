# metin_belgesi.py
# Oyundaki hikâye metinlerini (notlar, telefon konuşmaları, fotoğrafların arkası) metin tablosundan
# okuyup Şahan'ın okuması için tek bir belgeye döker (#60): Docs/hikaye-metinleri.md.
# Tablo değişince yeniden çalıştırılır:  python Docs/scripts/metin_belgesi.py

import os

KOK = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
TABLO = os.path.join(KOK, "Assets", "Resources", "Metinler.txt")
CIKTI = os.path.join(KOK, "Docs", "hikaye-metinleri.md")

GRUPLAR = [
    ("Açılış ve kapıcı odası", lambda k: k in ("not.yonetici",) or k == "foto.kapici" or k.startswith("not.telefon")),
    ("1. kat", lambda k: k.startswith("not.kat1") or k == "foto.kat1"),
    ("2. kat", lambda k: k.startswith("not.kat2") or k == "foto.kat2"),
    ("3. kat", lambda k: k.startswith("not.kat3") or k == "foto.kat3"),
    ("4. kat", lambda k: k.startswith("not.kat4") or k == "foto.kat4"),
    ("5. kat", lambda k: k.startswith("not.kat5") or k == "foto.kat5"),
    ("6. kat", lambda k: k.startswith("not.kat6") or k == "foto.kat6"),
    ("7. kat", lambda k: k.startswith("not.kat7") or k == "foto.kat7"),
    ("8. kat", lambda k: k.startswith("not.kat8") or k == "foto.kat8"),
]

# Oyundan çıkarılan notlar (#83, Şahan 2026-09-29: ipucu notu yok; oyuncu neyi nerede arayacağını bir
# nottan okumamalı). Tabloda artık yoklar; belgede katlarının sonunda gerekçeleriyle anılırlar.
IPUCU = "Şahan 2026-09-29'da ipucu notlarını kaldırdı (#83): "
KALDIRILAN = {
    "not.kat1_kavanoz": IPUCU + "oyuncu neyi nerede arayacağını bir nottan okumamalı.",
    "not.kat2_kiler": IPUCU + "kilerin şifresini radyoya bağlayan not oyuncuya ne yapacağını söylüyordu.",
    "not.kat6_kol": IPUCU + "kolun öbür dairede bir kapıyı açtığını söylüyordu.",
    "not.kat6_takvim": IPUCU + "kilerin şifresinin yan dairedeki takvimde olduğunu söylüyordu.",
    "not.kat7_kol": IPUCU + "kolun mutfağı açtığını ve kırık anahtarın parçalarının yerini söylüyordu.",
    "not.kat7_sifre": IPUCU + "Oda B'nin şifresinin yatak odasında, karanlıkta olduğunu söylüyordu.",
    "not.kat8_kiler": IPUCU + "kolun ve mutfak anahtarının yerini söylüyordu.",
    "not.kat8_salon": IPUCU + "kırık anahtarın parçalarının yerini söylüyordu.",
    "not.kat8_arsiv": IPUCU + "arşivin şifresinin binanın yapım yılı olduğunu söylüyordu.",
}


def oku():
    satirlar = []
    for satir in open(TABLO, encoding="utf-8").read().splitlines():
        if not satir.strip() or satir.startswith("#"):
            continue
        parca = satir.split("\t")
        if len(parca) >= 3 and (parca[0].startswith("not.") or parca[0].startswith("foto.kat") or parca[0] == "foto.kapici"):
            satirlar.append((parca[0], parca[1], parca[2]))
    return satirlar


def metin(s):
    # Tablodaki \n gerçek satır sonu; <size=..> gibi biçim etiketleri okuma için atılır.
    import re
    s = re.sub(r"<[^>]+>", "", s)
    return s.replace("\\n", "\n")


def main():
    tum = oku()
    kullanilan = set()
    out = ["# Hikâye metinleri: okuma belgesi (#60)", "",
           "Oyundaki bütün notlar, telefon konuşmaları ve fotoğrafların arkasındaki yazılar, katlara göre. "
           "Kaynak `Assets/Resources/Metinler.txt`; bu belge `Docs/scripts/metin_belgesi.py` ile üretilir, "
           "elle düzenlenmez. Değişiklik isteğini metnin anahtarıyla (örneğin `not.kat3_anne`) söylemek yeter.", ""]
    for baslik, uyar in GRUPLAR:
        grup = [t for t in tum if uyar(t[0])]
        if not grup:
            continue
        out.append(f"## {baslik}")
        out.append("")
        for k, tr, en in grup:
            kullanilan.add(k)
            out.append(f"### `{k}`")
            out.append("")
            out.append("**TR**")
            out.append("")
            out.extend("> " + x if x else ">" for x in metin(tr).split("\n"))
            out.append("")
            out.append("**EN**")
            out.append("")
            out.extend("> " + x if x else ">" for x in metin(en).split("\n"))
            out.append("")
        for k, neden in KALDIRILAN.items():
            if uyar(k):
                out.append(f"### `{k}` (kaldırıldı)")
                out.append("")
                out.append(neden)
                out.append("")
    kalan = [t for t in tum if t[0] not in kullanilan]
    if kalan:
        out.append("## Diğer")
        out.append("")
        for k, tr, en in kalan:
            out.append(f"- `{k}`: {metin(tr).splitlines()[0]}")
        out.append("")
    open(CIKTI, "w", encoding="utf-8").write("\n".join(out))
    print(len(tum), "metin ->", CIKTI)


if __name__ == "__main__":
    main()
