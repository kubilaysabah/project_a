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

def oku(dosya_adi, hiz=HIZ, kanal=1):
    """
    Art/Ses/kayitlar/ altındaki bir kaydı örnek listesi olarak okur (-1..1).
    kanal=1: tek liste (mono). kanal=2: (sol, sağ) iki liste (stereo; müzik için).
    """
    girdi = os.path.join(KAYITLAR, dosya_adi)
    gecici = os.path.join(tempfile.gettempdir(), "huzur_kayit.wav")
    # Blender'ı penceresiz çalıştırıp kaydı WAV'a çevir; çıktısını gösterme.
    subprocess.run([BLENDER, "-b", "--factory-startup", "--python",
                    os.path.join(os.path.dirname(__file__), "kayit_donustur.py"),
                    "--", girdi, gecici, str(hiz), str(kanal)], check=True, capture_output=True)
    with wave.open(gecici) as w:
        n = w.getnframes() * kanal
        ornekler = [x / 32768 for x in struct.unpack("<%dh" % n, w.readframes(w.getnframes()))]
    if kanal == 1:
        return ornekler
    # Stereo dosyada örnekler sırayla dizilir: sol, sağ, sol, sağ... İkiye ayırıyoruz.
    return ornekler[0::2], ornekler[1::2]


def stereo_kaydet(yol, sol, sag, hiz, tepe=0.8):
    """İki kanalı aynı ölçekle (en yüksek noktaya göre) 16 bit stereo WAV olarak yazar."""
    enbuyuk = max(max(abs(x) for x in sol), max(abs(x) for x in sag)) or 1.0
    olcek = tepe / enbuyuk
    os.makedirs(os.path.dirname(yol), exist_ok=True)
    with wave.open(yol, "wb") as dosya:
        dosya.setnchannels(2)
        dosya.setsampwidth(2)
        dosya.setframerate(hiz)
        veri = bytearray()
        for a, b in zip(sol, sag):
            veri += struct.pack("<hh", int(a * olcek * 32767), int(b * olcek * 32767))
        dosya.writeframes(bytes(veri))
    print("OK", os.path.basename(yol), round(len(sol) / hiz, 1), "sn (stereo)")


def seviye(ornekler, hiz=HIZ, yuzdelik=0.8):
    """
    Bir kaydın "tipik" gürlüğü: 1 saniyelik parçaların RMS değerleri sıralanır, %80'lik
    dilimdeki değer alınır. Ortalama yerine bunu kullanıyoruz: kısa ve çok gür bir an
    (ya da uzun bir sessizlik) ortalamayı bozar, bu ölçüyü bozmaz.
    """
    p = hiz
    degerler = sorted((sum(x * x for x in ornekler[i:i + p]) / p) ** 0.5
                      for i in range(0, len(ornekler) - p, p)) or [1e-9]
    return degerler[int(yuzdelik * (len(degerler) - 1))] or 1e-9


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
#   S03 Ortam_Bebek: tek seferlik bir "kat olayı" (#35): yalnızca planın bebeği seçtiği
#       katlarda, kata varıştan birkaç saniye sonra bir iki kez çalar. Duvar arkasından:
#       boğuk. (Önce 30 sn'lik bir döngüydü ve her katta çalıyordu; Şahan build'de
#       "sürekli olması rahatsız ediyor" dedi.)
#   S12 Bebek_Ters:  aynı parça TERSTEN, yine boğuk (anomali). Kata varınca hemen duyulsun
#       diye 1. saniyede başlar.
#   S13 Bebek_Yakin: aynı parça boğuk DEĞİL: duvar arkasından değil, asansör kuyusundan,
#       çok yakından (anomali). Kuyunun sert, uzun yankısı eklenir.
def bebek():
    kayit = oku("Crying_newborn_baby.ogg")
    parca = kes(kayit, 8.0, 17.4)
    boguk = duvar_arkasi(parca)

    ortak.kaydet(os.path.join(ORTAM, "Ortam_Bebek.wav"), boguk, HIZ, 0.8)

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


# ---------- GERİLİM MÜZİĞİ (#36) ----------
# Kayıt: Joth, "Ambience Pack 1 -- Sci Fi Horror" (OpenGameArt, CC0): her biri yaklaşık bir
# dakikalık, döngüye uygun 5 karanlık ambiyans parçası. Şahan'ın seçimi: beşi sırayla.
# Parçalar arka arkaya, 3 sn'lik yumuşak geçişle tek bir döngüye birleştiriliyor; son
# parça da ilkine aynı biçimde bağlanıyor. Toplam ~4,5 dk: tekrar fark edilmez.
# Parçaların gürlükleri çok farklı (-20 ile -40 dB arası); her biri aynı tipik gürlüğe
# getiriliyor, yoksa müzik bir parçada kaybolur, ötekinde bağırırdı.
# Müzik 2B (kafanın içinden) çaldığı için stereo kalıyor. Oyunda çok kısık çalar.
MUZIK_PARCALARI = ["joth_The_Surreal_Truth.mp3", "joth_Infestation_in_the_Control_Room.mp3",
                   "joth_Final_Captains_Log.mp3", "joth_The_Depths_of_Hell.mp3", "joth_Cage_of_the_Cryptid.mp3"]


def dengele(sol, sag, hedef, hiz=HIZ):
    """
    Yavaş bir seviye dengeleyici: müziğin gürlüğünü birkaç saniyelik ortalamasına göre
    izler, gür yerleri kısar, kısık yerleri biraz açar. Bir parçanın gür başlangıcı
    (Cage of the Cryptid) öbürlerinden ~10 dB yüksekti; oyunun arkasında aniden
    bağırmasın. Yavaş (0,7 sn) çalışır, yoksa müziğin kendi nefesini düzleştirirdi.
    """
    k = 1.0 / (0.7 * hiz)                       # 0,7 saniyelik yumuşatma
    guc, cikti_sol, cikti_sag = hedef * hedef, [], []
    for a, b in zip(sol, sag):
        m = (a + b) / 2
        guc += k * (m * m - guc)                # kaydırmalı ortalama güç
        g = hedef / max(guc ** 0.5, 1e-6)
        g = min(1.6, max(0.25, g))              # en fazla 4 kat kıs, 1,6 kat aç
        cikti_sol.append(a * g)
        cikti_sag.append(b * g)
    return cikti_sol, cikti_sag


def muzik():
    gecis = int(3.0 * HIZ)
    parcalar = []
    for ad in MUZIK_PARCALARI:
        sol, sag = oku(ad, HIZ, kanal=2)
        # %95'lik dilim: parçanın gür kısımları eşitlenir. (%80 ile, çoğu sessiz ama gür
        # başlayan Cage of the Cryptid aşırı açılıyor ve başı bağırıyordu.)
        g = 0.1 / seviye([(a + b) / 2 for a, b in zip(sol, sag)], HIZ, 0.95)
        parcalar.append(([x * g for x in sol], [x * g for x in sag]))

    # Zincir: her parçanın son 3 saniyesi bir sonrakinin ilk 3 saniyesiyle karışır.
    sol, sag = list(parcalar[0][0]), list(parcalar[0][1])
    for ps, pg in parcalar[1:]:
        for i in range(gecis):
            a = i / gecis
            sol[len(sol) - gecis + i] = sol[len(sol) - gecis + i] * (1 - a) + ps[i] * a
            sag[len(sag) - gecis + i] = sag[len(sag) - gecis + i] * (1 - a) + pg[i] * a
        sol += ps[gecis:]
        sag += pg[gecis:]
    # Döngü: son 3 saniye, başın üstüne karışır ve sondan kesilir.
    for i in range(gecis):
        a = i / gecis
        sol[i] = sol[len(sol) - gecis + i] * (1 - a) + sol[i] * a
        sag[i] = sag[len(sag) - gecis + i] * (1 - a) + sag[i] * a
    sol, sag = sol[:-gecis], sag[:-gecis]
    sol, sag = dengele(sol, sag, hedef=0.02)
    stereo_kaydet(os.path.join(KOK, "Assets", "Audio", "Muzik", "Muzik_Gerilim.wav"), sol, sag, HIZ, 0.8)


# ---------- DIŞ SESLER (#37) ----------
# Apartmanın dışından, ara sıra gelen sesler (kat olaylarından ayrı, rastgele bir katman).
# Hepsi merdiven penceresinin ardından, uzaktan duyulur: tizler yolda kalır, gecenin
# sokaklarında yankılanır. Oyunda 2B, kısık çalar.
DIS = os.path.join(KOK, "Assets", "Audio", "Dis")


def uzaklastir(ornekler, hiz, kesim, yanki_payi, kuyruk):
    """Sesi uzaktan geliyormuş gibi yapar: tizleri kes, uzun yankı ekle, yankıya yer bırak."""
    s = ortak.alcak(ortak.alcak(ornekler, kesim, hiz), kesim, hiz) + sessizlik(kuyruk, hiz)
    return ortak.yanki(s, hiz, boyut=0.9, sonum=0.45, islak=yanki_payi, kuru=0.6, oda=1.5)


# Kayıt: "Rifle: Shot #2" (BigSoundBank #2854, CC0): açık arazide bir tüfek atışı ve
# ovadaki yankısı. Uzak bir şehir atışı gibi duyulsun diye boğuklaştırılıp yankısı
# uzatılıyor. İki çeşit: tek atış ve 1,1 sn arayla iki atış.
def dis_silah():
    H = 44100
    kayit = oku("bigsoundbank_2854_rifle_shot_2.wav", H)
    tek = kes(kayit, 0.0, len(kayit) / H, giris=0.001, cikis=0.2, hiz=H)
    ortak.kaydet(os.path.join(DIS, "Dis_Silah_1.wav"), uzaklastir(tek, H, 1400, 0.7, 3.0), H, 0.8)
    iki = list(tek) + sessizlik(1.1, H)
    ortak.ekle(iki, tek, int(1.1 * H), 0.8)
    ortak.kaydet(os.path.join(DIS, "Dis_Silah_2.wav"), uzaklastir(iki, H, 1400, 0.7, 3.0), H, 0.8)


# Kayıt: "Whistling of the Wind #1" (BigSoundBank #0147, CC0): açık bir pencereden içeri
# süzülen hafif esinti. Şahan'ın isteği: "bina içine girecek cılız rüzgâr sesi". Kaydın
# içinden üç esinti kesiliyor (esintinin kabarıp söndüğü yerler), her biri ~13 sn,
# başı ve sonu yumuşak. Kayıt zaten içeriden alınmış; yalnızca en alttaki uğultu atılıyor.
def dis_ruzgar():
    kayit = ortak.yuksek(oku("bigsoundbank_0147_whistling_of_the_wind_1.wav"), 80, HIZ)
    for i, (bas, son) in enumerate(((22.0, 36.0), (45.0, 58.0), (86.0, 100.0)), 1):
        parca = kes(kayit, bas, son, giris=2.5, cikis=3.0)
        ortak.kaydet(os.path.join(DIS, f"Dis_Ruzgar_{i}.wav"), parca, HIZ, 0.8)


# Kayıt: "Dog Barking at Night" (spirit.zip, Freesound, CC0; Şahan indirdi): sessiz bir
# mahallede gece uzaktan havlayan köpek, 22,7 sn. İki parça kesiliyor: 3,0-12,5 sn arası
# seyrek ve uzak havlamalar, 12,6-20,6 sn arası daha sık ve yakın bir nöbet. Kayıt zaten
# uzaktan alınmış; pencerenin ardından duyulsun diye tizleri hafifçe törpülenip kısa bir
# sokak yankısı ekleniyor.
def dis_kopek():
    kayit = oku("849570__spiritzip__dog-barking-at-night.wav")
    for i, (bas, son) in enumerate(((3.0, 12.5), (12.6, 20.6)), 1):
        parca = ortak.yuksek(kes(kayit, bas, son, giris=0.3, cikis=0.6), 120, HIZ)
        ortak.kaydet(os.path.join(DIS, f"Dis_Kopek_{i}.wav"), uzaklastir(parca, HIZ, 2600, 0.3, 1.5), HIZ, 0.8)


URETICILER = {"bebek": bebek, "tv": tv, "guvercin": guvercin, "kapi_carpma": kapi_carpma, "kapi_tiklatma": kapi_tiklatma,
              "muzik": muzik, "dis_silah": dis_silah, "dis_ruzgar": dis_ruzgar, "dis_kopek": dis_kopek}

if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
