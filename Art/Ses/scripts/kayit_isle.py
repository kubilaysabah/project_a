# kayit_isle.py
# Gerçek ses kayıtlarından oyunun ses dosyalarını üretir (#14, #25 revizyonu, 2026-09-26).
#
# ÇALIŞTIRMA:  python Art/Ses/scripts/kayit_isle.py [ses adları]   (ör. bebek)
#
# NEDEN GERÇEK KAYIT: Şahan'ın dinleme testinde canlı ve organik sesler (bebek, TV,
# güvercin, tahta kapı) formülle taklit edilince "uzaylı gibi", "alakası yok" diye
# geri döndü. Bunlar için lisansı temiz (CC0 / kamu malı / Pixabay) gerçek kayıtlar
# seçildi. Kayıtların asılları Art/Ses/kayitlar/ klasöründe, lisansları LISANSLAR.md'de.
#
# Bu betik kaydı DEĞİŞTİRMEZ; onu okur, kullanılacak parçayı keser, oyundaki yerine göre
# işler (duvar arkası boğukluğu, ters çevirme, yankı) ve Assets altına WAV yazar.
# Kayıtlar ogg/mp3 olabildiği için önce Blender ile WAV'a çevrilir (kayit_donustur.py).

import os, struct, subprocess, sys, tempfile, wave
import ses_ortak as ortak

HIZ = 22050
BLENDER = r"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe"

KOK = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
KAYITLAR = os.path.join(KOK, "Art", "Ses", "kayitlar")
ORTAM = os.path.join(KOK, "Assets", "Audio", "Ortam")
ANOMALI = os.path.join(KOK, "Assets", "Audio", "Anomali")


# ---------- YARDIMCILAR ----------

def oku(dosya_adi, hiz=HIZ):
    """Art/Ses/kayitlar/ altındaki bir kaydı mono örnek listesi olarak okur (-1..1)."""
    girdi = os.path.join(KAYITLAR, dosya_adi)
    gecici = os.path.join(tempfile.gettempdir(), "huzur_kayit.wav")
    # Blender'ı penceresiz çalıştırıp kaydı WAV'a çevir; çıktısını gösterme.
    subprocess.run([BLENDER, "-b", "--factory-startup", "--python",
                    os.path.join(os.path.dirname(__file__), "kayit_donustur.py"),
                    "--", girdi, gecici, str(hiz)], check=True, capture_output=True)
    with wave.open(gecici) as w:
        n = w.getnframes()
        return [x / 32768 for x in struct.unpack("<%dh" % n, w.readframes(n))]


def kes(ornekler, bas, son, giris=0.05, cikis=0.4, hiz=HIZ):
    """
    Kaydın 'bas' ile 'son' saniyeleri arasını keser. Başına ve sonuna yumuşak geçiş
    (fade) koyar: kesilen yerde ses aniden başlayıp bitseydi bir "çıt" duyulurdu.
    """
    parca = ornekler[int(bas * hiz):int(son * hiz)]
    n, g, c = len(parca), int(giris * hiz), int(cikis * hiz)
    for i in range(n):
        e = 1.0
        if i < g:
            e = i / g
        if i > n - c:
            e = min(e, (n - i) / c)
        parca[i] *= e
    return parca


def duvar_arkasi(ornekler, hiz=HIZ):
    """
    Başka bir dairenin içinden, duvarın arkasından duyulan ses: duvar tizleri yutar
    (800 Hz üstü iki kez törpülenir), çok alçak gümbürtü de atılır; üstüne o odanın
    küçük yankısı eklenir.
    """
    s = ortak.alcak(ortak.alcak(ortak.yuksek(ornekler, 120, hiz), 800, hiz), 800, hiz)
    s += sessizlik(1.0, hiz)                       # yankının sönmesine yer kalsın
    return ortak.yanki(s, hiz, boyut=0.5, sonum=0.5, islak=0.2, oda=0.6)


def sessizlik(saniye, hiz=HIZ):
    return [0.0] * int(saniye * hiz)


# ---------- BEBEK (S03, S12, S13) ----------
# Kayıt: "Crying newborn baby" (Wikimedia Commons, CC0), 23 sn. İçinde iki nöbet var:
# 0-3,3 sn kısa bir ağlama, 8-23 sn uzun ve yoğun bir ağlama. Oyunda kullanılan parça
# uzun nöbetin ilk 9,4 saniyesi (8,0-17,4 sn): güçlü bir "vaaa" ile başlıyor, arada
# nefes alışlar var.
#   S03 Ortam_Bebek: 30 sn'lik döngü; ağlama 5. saniyede başlar, geri kalanı sessizdir
#       (bebek arada bir ağlar). Duvar arkasından: boğuk.
#   S12 Bebek_Ters:  aynı parça TERSTEN, yine boğuk (anomali). Kata varınca hemen duyulsun
#       diye 1. saniyede başlar.
#   S13 Bebek_Yakin: aynı parça boğuk DEĞİL: duvar arkasından değil, asansör kuyusundan,
#       çok yakından (anomali). Kuyunun sert, uzun yankısı eklenir.
def bebek():
    kayit = oku("Crying_newborn_baby.ogg")
    parca = kes(kayit, 8.0, 17.4)
    boguk = duvar_arkasi(parca)

    ortam = sessizlik(5.0) + boguk
    ortam += sessizlik(30.0 - len(ortam) / HIZ)
    ortak.kaydet(os.path.join(ORTAM, "Ortam_Bebek.wav"), ortam[:int(30 * HIZ)], HIZ, 0.8)

    ters = sessizlik(1.0) + duvar_arkasi(parca[::-1])
    ters += sessizlik(14.0 - len(ters) / HIZ)
    ortak.kaydet(os.path.join(ANOMALI, "Bebek_Ters.wav"), ters[:int(14 * HIZ)], HIZ, 0.8)

    yakin = sessizlik(1.0) + ortak.yanki(ortak.alcak(parca, 7000, HIZ) + sessizlik(2.0), HIZ,
                                         boyut=0.75, sonum=0.3, islak=0.3, oda=0.8)
    yakin += sessizlik(14.0 - len(yakin) / HIZ)
    ortak.kaydet(os.path.join(ANOMALI, "Bebek_Yakin.wav"), yakin[:int(14 * HIZ)], HIZ, 0.8)


# ---------- TV (S02) ----------
# Kayıt: "TV Playing in the Next Room" (Pixabay, Pixabay Content License), 87 sn.
# Kayıt zaten yan odadan, duvar arkasından alınmış: konuşmalar boğuk, kelimeler seçilmiyor,
# arada sessiz anlar var. Neredeyse tamamı kullanılıyor (0-85,5 sn): döngü ne kadar
# uzunsa oyuncunun tekrarı fark etmesi o kadar zor. Sonun son 0,8 saniyesi başa
# karıştırılıyor (ortam_uret.py'deki dongule ile aynı fikir): döngü dikişsiz.
# Kelimeler hiç seçilmesin diye 3 kHz üstü biraz daha törpüleniyor.
def tv():
    kayit = oku("arunangshubanerjee-tv-playing-in-the-next-room-distant-and-indistinct-television-sound-360697.mp3")
    parca = ortak.alcak(ortak.yuksek(kayit[:int(86.3 * HIZ)], 80, HIZ), 3000, HIZ)
    n, gecis = int(85.5 * HIZ), int(0.8 * HIZ)
    dongu = parca[:n]
    for i in range(gecis):
        a = i / gecis
        dongu[i] = parca[i] * a + parca[n + i] * (1 - a)
    ortak.kaydet(os.path.join(ORTAM, "Ortam_TV.wav"), dongu, HIZ, 0.8)


# ---------- GÜVERCİNLER (S11) ----------
# Kayıt: "pigeons flying wings noise" (5ro4, Freesound, CC0), 3,8 sn: birkaç güvercinin
# kanat çırpışı; başta bir havalanış, ortada çırpınma, sonda ikinci bir havalanış.
# Tamamı kullanılıyor. Telefonla kaydedildiği için en alttaki gümbürtü (100 Hz altı)
# atılıyor. Çatı açık hava: yankı eklenmiyor.
def guvercin():
    kayit = oku("701283__5ro4__pigeons-flying-wings-noise.wav", 44100)
    parca = kes(ortak.yuksek(kayit, 100, 44100), 0.0, len(kayit) / 44100, giris=0.01, cikis=0.3, hiz=44100)
    ortak.kaydet(os.path.join(KOK, "Assets", "Resources", "Sesler", "Guvercin_Kanat.wav"), parca, 44100, 0.8)


# ---------- KAPI ÇARPMASI (S15) ----------
# Kayıt: "door wood apartment interior open close fast slam" (kyles, Freesound, CC0):
# gerçek bir tahta apartman kapısı. Kayıtta önce açılış (0-0,4 sn), sonra çarpma
# (1,8 sn'den sonra) var; yalnızca çarpma kullanılıyor. Kayıt kuru bir odada alınmış;
# oyunda kapı beton merdiven boşluğunda çarptığı için boşluğun yankısı ekleniyor.
def kapi_carpma():
    H = 44100
    kayit = oku("637482__kyles__door-wood-apartment-interior-open-close-fast-slam.flac", H)
    parca = kes(kayit, 1.80, len(kayit) / H, giris=0.005, cikis=0.2, hiz=H)
    s = ortak.yanki(parca + sessizlik(1.5, H), H, boyut=0.85, sonum=0.35, islak=0.35, oda=1.3)
    ortak.kaydet(os.path.join(ANOMALI, "Kapi_Carpma.wav"), s, H, 0.9)


# ---------- KAPI TIKLATMA (S16) ----------
# Kayıt: "Wooden Door Knock" (FrostyFrost, Freesound CC0; Pixabay kopyası), kalın bir
# tahta kapıya üç vuruş (2,05 / 2,30 / 2,55 sn). Üç vuruş kesiliyor. Oyunda vuruşlar
# 1 numaralı dairenin İÇİNDEN geliyor: kapı tizleri yutar (1,8 kHz üstü törpülenir),
# ardından antrenin ve merdivenin kısa yankısı.
def kapi_tiklatma():
    H = 44100
    kayit = oku("freesound_community-wooden-door-knock-102902.mp3", H)
    parca = kes(kayit, 2.00, 3.00, giris=0.005, cikis=0.1, hiz=H)
    s = ortak.alcak(ortak.alcak(parca, 1800, H), 1800, H)
    s = ortak.yanki(s + sessizlik(0.8, H), H, boyut=0.6, sonum=0.45, islak=0.2, oda=0.8)
    ortak.kaydet(os.path.join(ANOMALI, "Kapi_Tiklatma.wav"), s, H, 0.85)


URETICILER = {"bebek": bebek, "tv": tv, "guvercin": guvercin, "kapi_carpma": kapi_carpma, "kapi_tiklatma": kapi_tiklatma}

if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
