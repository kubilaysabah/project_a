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


def sessizligi_kirp(sol, sag, hiz, esik_db=-40.0):
    """Baştaki ve sondaki sessizliği atar: tepenin 40 dB altındaki örnekler "sessiz" sayılır."""
    tepe = max(max(abs(x) for x in sol), max(abs(x) for x in sag), 1e-9)
    esik = tepe * 10 ** (esik_db / 20)
    n = len(sol)
    ilk = next(i for i in range(n) if abs(sol[i]) > esik or abs(sag[i]) > esik)
    son = next(i for i in range(n - 1, -1, -1) if abs(sol[i]) > esik or abs(sag[i]) > esik)
    return sol[ilk:son + 1], sag[ilk:son + 1]


# Arka plan müziği (#74, Şahan'ın ikinci tur seçimi): Scott Elliott, "Death is Looming"
# (OpenGameArt, CC-BY 4.0; jenerikte isim yazılı). Bestecisine göre "yoğun korku ambiyansı".
# İlk hâli Joth'un beş sakin parçasıydı; Şahan "90'ların korku temasına uygun, arkada tam
# gerilim ve korku katacak bir müzik" istedi. Döngü için işlem:
#   1. baştaki ve sondaki sessizlik kırpılır,
#   2. gürlük eski müzikle aynı düzeye getirilir (%95'lik dilim 0,1),
#   3. son 4 saniye başın üstüne karışır ve sondan kesilir: döngü dikişsiz,
#   4. yavaş dengeleyici gür yerleri biraz kısar (oyunun arkasında aniden bağırmasın).
def muzik():
    sol, sag = oku("scottelliott_death_is_looming.mp3", HIZ, kanal=2)
    sol, sag = sessizligi_kirp(sol, sag, HIZ)
    g = 0.1 / seviye([(a + b) / 2 for a, b in zip(sol, sag)], HIZ, 0.95)
    sol, sag = [x * g for x in sol], [x * g for x in sag]
    gecis = int(4.0 * HIZ)
    n = len(sol)
    for i in range(gecis):
        a = i / gecis
        sol[i] = sol[n - gecis + i] * (1 - a) + sol[i] * a
        sag[i] = sag[n - gecis + i] * (1 - a) + sag[i] * a
    sol, sag = sol[:n - gecis], sag[:n - gecis]
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
# ovadaki yankısı. İki çeşit: tek atış ve 1,1 sn arayla iki atış.
# #74 (Şahan, 2026-09-27): ilk hâli (1400 Hz'de iki kez süzgeç + yoğun yankı) "silah sesine
# hiç benzemiyor" dendi. Üç hafiflikte aday dinlendi; Şahan NEREDEYSE HAMI seçti: yalnızca
# en tizler (8 kHz) törpülenir, çok hafif bir oda yankısı eklenir.
def dis_silah():
    H = 44100
    kayit = oku("bigsoundbank_2854_rifle_shot_2.wav", H)
    tek = kes(kayit, 0.0, len(kayit) / H, giris=0.001, cikis=0.2, hiz=H)

    def neredeyse_ham(ses):
        s = ortak.alcak(ses, 8000, H) + sessizlik(1.5, H)
        return ortak.yanki(s, H, boyut=0.9, sonum=0.45, islak=0.15, kuru=1.0, oda=1.5)

    ortak.kaydet(os.path.join(DIS, "Dis_Silah_1.wav"), neredeyse_ham(tek), H, 0.8)
    iki = list(tek) + sessizlik(1.1, H)
    ortak.ekle(iki, tek, int(1.1 * H), 0.8)
    ortak.kaydet(os.path.join(DIS, "Dis_Silah_2.wav"), neredeyse_ham(iki), H, 0.8)


# Dış rüzgâr (Dis_Ruzgar_1-3) #74'te kalktı (Şahan'ın ses kararları); üreticisi de silindi,
# yoksa betik adsız çalıştırılınca silinen dosyaları geri getirirdi.


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

# ---------- RADYO (#62, 2. kat) ----------
# Yaşlı Nuri'nin radyosu, oyuncu daireye girdikten birkaç saniye sonra kendiliğinden açılır.
# İstasyon tam tutmuyor: parazitin (hışırtı ve çıtırtı) içinde uzak bir yayın bir belirir
# bir kaybolur. Yayın olarak gerilim müziğinin bir parçası kullanılıyor (Joth, CC0; zaten
# oyunda ve lisansı kayıtlı): radyonun küçük hoparlörü gibi yalnızca orta frekanslar bırakılır
# (350-2800 Hz), böylece "odada çalan müzik" değil "eski bir radyodan gelen ses" duyulur.
# Çıktı 24 sn'lik dikişsiz bir döngü (Radyo_Istasyon.wav) ve radyonun düğme tıkı (Radyo_Dugme.wav).
def radyo():
    import math, random
    random.seed(62)                                  # her çalıştırmada aynı ses (onaylanınca değişmesin)

    # 1. Yayın (#74, Şahan'ın seçimi): Carlos Gardel, "Mi noche triste" (1917 tango, Wikimedia
    # Commons, kamu malı; kayıt da beste de). İlk hâli parazitli bir müzik parçasıydı; Şahan
    # "90'lar haber radyosu, haber ve slow bir şarkı" istedi, ikinci turda yalnızca tangoyu seçti.
    # Şarkının tamamı çalar (~3 dk). Küçük hoparlör süzgeci: 350-2800 Hz, iki kez, dik kenarlı.
    yayin = oku("CarlosGardel-MiNocheTriste(1917).ogg", HIZ)
    for _ in range(2):
        yayin = ortak.alcak(ortak.yuksek(yayin, 350, HIZ), 2800, HIZ)
    yayin = ortak.tepeyi_esitle(yayin)
    n = len(yayin)
    sure = n / HIZ

    # 2. Parazit: bant geçiren gürültü (radyonun "şşşş"si) ve seyrek çıtırtılar.
    parazit = ortak.bant(ortak.gurultu(n), 1800, HIZ, 0.6)
    parazit = ortak.tepeyi_esitle(parazit)
    for _ in range(int(sure * 5)):                   # saniyede ortalama beş çıtırtı
        cit = ortak.darbe(int(0.02 * HIZ), HIZ, 0.004)
        ortak.ekle(parazit, cit, random.randrange(n), random.uniform(0.3, 1.0))

    # 3. Sinyal gücü: yayın ile parazit birbirinin tersine kabarıp söner (istasyon kayıyor).
    # İki yavaş sinüs; periyotları kaydın süresine tam bölünür (13 ve 19 tur), baş ve son aynı
    # değerde buluşur. Başta ve sonda sinyal en zayıf (parazit en güçlü): şarkının başa
    # dönüşü parazitin içinde kaybolur.
    sonuc = []
    for i in range(n):
        t = i / n
        g = 0.62 + 0.23 * math.cos(2 * math.pi * 13 * t) + 0.15 * math.cos(2 * math.pi * 19 * t)
        g = 1.0 - g                                  # t = 0'da g en küçük: sinyal zayıf
        g = min(1.0, max(0.0, 0.25 + 1.1 * g))       # 0: yalnız parazit, 1: temiz yayın
        sonuc.append(yayin[i] * g * 0.9 + parazit[i] * (0.12 + 0.5 * (1 - g)))

    # 4. Dikiş: son 1,5 saniye başın üstüne karışır.
    gecis = int(1.5 * HIZ)
    for i in range(gecis):
        a = i / gecis
        sonuc[i] = sonuc[n - gecis + i] * (1 - a) + sonuc[i] * a
    sonuc = sonuc[:n - gecis]
    ortak.kaydet(os.path.join(KOK, "Assets", "Audio", "Daire", "Radyo_Istasyon.wav"), sonuc, HIZ, 0.8)

    # Radyo_Dugme.wav (düğmenin tıkı) eskiden burada üretiliyordu. Şahan onu onayladı (S37 kalsın);
    # rastgele sayı dizisini bu fonksiyondaki parazitle paylaştığı için tango gelince değişiyordu.
    # Artık üretilmiyor: onaylı dosya olduğu gibi kalır (#74).



# ---------- #74 SES REVİZYONU: ŞAHAN'IN SEÇTİĞİ KAYITLAR ----------
# Şahan 49 sesi dinleyip işaretledi, değişecekler için adaylar arasından seçti (2026-09-27).
# Freesound dosyalarını Şahan kendi hesabıyla indirdi; Pixabay dosyaları herkese açık dosya
# sunucusundan indirildi. Kesme yerleri kayıtların ses zarfından (50 ms'lik tepe) bulundu.
# Hepsi 44,1 kHz mono; oyundaki dosya adları değişmedi, kod aynı dosyaları okur.
SESLER = os.path.join(KOK, "Assets", "Resources", "Sesler")
H44 = 44100


def _kaydet(klasor, ad, ornekler, tepe=0.8):
    ortak.kaydet(os.path.join(klasor, ad), ornekler, H44, tepe)


def rev_damla():
    """S12: Funkelfang, "Water Drip – Leaky Faucet in Ceramic Sink" (CC-BY 4.0, jenerikte isim).
    Seramik lavabo, fayanslı banyo. 1,2-21,15 sn arası: iki ucu da iki damla arasındaki sessizlikte,
    bu yüzden döngü dikişsiz."""
    k = oku("847083__funkelfang__water-drip-leaky-faucet-in-ceramic-sink.wav", H44)
    _kaydet(ANOMALI, "Boru_Damla.wav", kes(k, 1.2, 21.15, giris=0.05, cikis=0.05, hiz=H44))


def rev_anahtar():
    """S24: sophiehall3535, "Pick Up Keys 1" (CC0). Anahtar destesi alınıyor, tamamı."""
    k = oku("248037__sophiehall3535__pick-up-keys-1.wav", H44)
    _kaydet(SESLER, "Anahtar_Al.wav", kes(k, 0.0, 1.35, giris=0.005, cikis=0.15, hiz=H44))


def rev_cam():
    """S25: loganzsound, "Glass break hammer" (CC0). Çekiçle cam ve saçılan parçalar, tamamı."""
    k = oku("784151__loganzsound__glass-break-hammer.wav", H44)
    _kaydet(SESLER, "Cam_Kirilma.wav", kes(k, 0.0, 1.29, giris=0.002, cikis=0.2, hiz=H44))


def rev_devrilme():
    """S29: qubodup "Door Smash 1" (CC0) + Alex_Jauk "Large Wood Plank Falls Down" (Pixabay).
    Şahan'ın seçimi: ağır kapı çarpması ile düz tahta şaplamasının karışımı. Kapının asıl vuruşu
    kayıtta 1,95 sn'de; 1,7 sn'den kesilince 0,25 sn'ye, tahtanın vuruşuna (0,2 sn) denk gelir."""
    kapi = kes(oku("218890__qubodup__door-smash-1.flac", H44), 1.7, 4.4, giris=0.01, cikis=0.6, hiz=H44)
    tahta = kes(oku("pixabay_alex_jauk_large-wood-plank-falls-down-230892.mp3", H44), 0.0, 1.42, giris=0.002, cikis=0.3, hiz=H44)
    karisim = [x * 0.9 for x in kapi]
    ortak.ekle(karisim, tahta, int(0.03 * H44), 0.75)
    _kaydet(SESLER, "Kapi_Devrilme.wav", karisim)


def rev_vida():
    """S30: Fwughox_Official, "Screwing into wood" (CC0). El tornavidasının tur tur dönüşü;
    her basışta bir vida, 0,95-2,2 sn arası (iki dönüş)."""
    k = oku("747175__fwughox_official__screwing-into-wood.wav", H44)
    _kaydet(SESLER, "Vida_Sokme.wav", kes(k, 0.95, 2.2, giris=0.01, cikis=0.15, hiz=H44))


def rev_demir_sarsma():
    """S33: launemax, "Rattling at an iron gate" (CC0). Kilitli demir kat kapısını sarsmak;
    5,4-6,9 sn arası yoğun şangırtı."""
    k = oku("274766__launemax__rattling-at-an-iron-gate.wav", H44)
    _kaydet(SESLER, "KatKapisi_Kilitli.wav", kes(k, 5.4, 6.9, giris=0.01, cikis=0.3, hiz=H44))


def rev_kol():
    """S34: kyles, "switch big breaker metal click on, off" (CC0). İlk (açma) tıkı, 0-1,2 sn."""
    k = oku("451933__kyles__switch-big-breaker-metal-click-on-off.flac", H44)
    _kaydet(SESLER, "Kol_Cekme.wav", kes(k, 0.0, 1.2, giris=0.002, cikis=0.3, hiz=H44))


def rev_sigorta():
    """S35: C-V, "Screwing in light bulb" (CC0) + S34'ün şalter tıkı. Buşonun kaydı hiçbir izinli
    kaynakta yok; ampulün duya vidalanması aynı mekanik. 0,9-3,2 sn arası vidalama, sonunda
    elektrik gelirken şalterin tok tıkı."""
    vida = kes(oku("521127__c-v__screwing-in-light-bulb.wav", H44), 0.9, 3.2, giris=0.02, cikis=0.15, hiz=H44)
    tik = kes(oku("451933__kyles__switch-big-breaker-metal-click-on-off.flac", H44), 0.1, 0.9, giris=0.002, cikis=0.2, hiz=H44)
    karisim = list(vida) + sessizlik(0.9, H44)
    ortak.ekle(karisim, tik, int(2.35 * H44), 0.8)
    _kaydet(SESLER, "Sigorta_Takma.wav", karisim)


def rev_sifre():
    """S36: Group39, "SFX-Int.-Combination Lock" (CC0). Çarkın tek bir rakam tıkı: 1,28-1,45 sn."""
    k = oku("708579__group39__sfx-int.wav", H44)
    _kaydet(SESLER, "Sifre_Cark.wav", kes(k, 1.28, 1.45, giris=0.001, cikis=0.04, hiz=H44))


def rev_zil():
    """S45: craigsmith, "R09-13-Apartment Buzzer" (CC0). Küçük bir dairenin tiz vızıltılı zili;
    1,6-4,3 sn arası iki çalış. Oyundaki dosya adı eski ding-dong'dan kaldı (Zil_DingDong)."""
    k = oku("479992__craigsmith__r09-13-apartment-buzzer.wav", H44)
    _kaydet(ANOMALI, "Zil_DingDong.wav", kes(k, 1.6, 4.3, giris=0.005, cikis=0.3, hiz=H44))


def rev_ic_kilitli():
    """S28: Kodack, "Trying to open a locked door" (Pixabay). Kilitli oda kapısının kolunu zorlamak;
    0,25-1,55 sn arası."""
    k = oku("pixabay_kodack_trying-to-open-a-locked-door-104302.mp3", H44)
    _kaydet(SESLER, "IcKapi_Kilitli.wav", kes(k, 0.25, 1.55, giris=0.005, cikis=0.2, hiz=H44))


def rev_asansor_zili():
    """S40z: cognito perceptu, "elevator bell doors" (CC0). Eski bir asansörün zili; 0,1-2,4 sn."""
    k = oku("18768__cognito-perceptu__elevator-bell-doors.wav", H44)
    _kaydet(ANOMALI, "Asansor_Ding.wav", kes(k, 0.1, 2.4, giris=0.005, cikis=0.4, hiz=H44))


def rev_gicirti():
    """S20+: qubodup, "Creaking Floorboards 13 Long Slow" (CC0). Dairelerde adımların arasına ara sıra
    karışan parke gıcırtısı; uzun gıcırtıdan üç çeşit (Gicirti_1-3)."""
    k = oku("802595__qubodup__creaking-floorboards-13-long-slow.wav", H44)
    for i, (bas, son) in enumerate(((0.0, 1.4), (1.4, 2.9), (2.9, 4.0)), 1):
        _kaydet(SESLER, f"Gicirti_{i}.wav", kes(k, bas, son, giris=0.03, cikis=0.25, hiz=H44), tepe=0.6)


# ---------- OYUNCUNUN ADIMLARI (#101) ----------
# Şahan'ın isteği (2026-10-01): adım sesleri korku oyunlarını (Amnesia, Outlast, Visage) baz alsın.
# O oyunların adımları gerçek foley kaydıdır ve her zeminde AYNI ayakkabı kaydedilir: oyuncu zemin
# değişince "aynı adamın ayağı başka yere bastı" diye duyar. Bizde parke, fayans ve halı sentezdi,
# merdiven ise başka birinin kaydı; ayakkabılar birbirini tutmuyordu.
#
# Kaynak: Freesound kullanıcısı sturmankin'in ayak sesi bankası (CC0). Aynı kişi, aynı mikrofon
# (ZOOM H2), aynı ev. Koyu deri ayakkabı: parke, halı, beton. Fayansta koyu ayakkabıyla yürüyüş
# kaydı yok; aynı bankanın hafif ayakkabı kaydı kullanıldı (aynı banyo, aynı mikrofon).
#
# Her kayıttan adımlar TEK TEK bulunur ve kesilir:
#   1. Zarf: 5 ms'lik pencerelerde sesin gücü (RMS).
#   2. Başlangıç: güç bir eşiği aşağıdan yukarı geçtiği an bir adım vardır (iki adım arası en az
#      0,3 sn); kesim, topuğun ilk değişini kaçırmamak için oradan geriye, gücün düştüğü yere kayar.
#      Eşik kaydın kendi gürültüsüne göre: sessiz kayıtta düşük, halıda (kayıt gürültüsü
#      yüksek, adım yumuşak) daha yüksek.
#   3. Temiz adım: öncesinde 0,3 sn, sonrasında 0,45 sn başka adım olmayan adımlar (üst üste binen
#      iki adım tek dosyaya girmesin). En gür ve en kısık %20 atılır (tökezleme, sürtme), kalanlardan
#      kayıt boyunca eşit aralıkla 8 tane seçilir: çeşit, ayağın sağ/sol farkını ve yorgunluğu taşır.
#   4. İşleme: 60 Hz altı atılır (bodrum gürültüsü), başa 2 ms, sona 120 ms yumuşak geçiş; seviye
#      her adımın en gür 100 ms'sine göre eşitlenir (adımlar arası tutarlı). Korku oyunlarındaki gibi
#      oyuncunun kendi adımı KISIK: eski sentez adımdan 1 dB kısık; halı 3,5 dB daha kısık.
# Çıktı: Adim_1-8 (parke), Adim_Fayans_1-8, Adim_Hali_1-8, Merdiven_Adim_1-8 (beton).

ADIM_KAYITLARI = [
    # (kayıt, çıktı öneki, en gür 100 ms hedefi dB, alçak geçiren Hz ya da None)
    # Halı kaydının hışırtısı (kayıt gürültüsü) yüksek; halıda tiz bir şey duyulmaz, o yüzden
    # 5 kHz üstü kısılır: hem hışırtı azalır hem adım daha boğuk, halıya yakışır.
    ("273372__sturmankin__wood_15a_darkshoes_walk.wav", "Adim", -19.5, None),
    ("272150__sturmankin__ceramic_14a_lightshoes_walk.wav", "Adim_Fayans", -19.0, None),
    ("271911__sturmankin__carpet_15a_darkshoes_walk.wav", "Adim_Hali", -23.0, 5000),
    ("272255__sturmankin__concrete_15a_darkshoes_walk.wav", "Merdiven_Adim", -19.0, None),
]
ADIM_SAYISI = 8


def _zarf(x, hiz, pencere=0.005):
    """5 ms'lik pencerelerin RMS gücü (her 2. örnek: hız için; zarf için yeterli)."""
    adim = int(pencere * hiz)
    z = []
    for i in range(0, len(x) - adim, adim):
        parca = x[i:i + adim:2]
        z.append((sum(v * v for v in parca) / len(parca)) ** 0.5)
    return z


def _en_gur_100ms(x, hiz):
    """Sesin en gür 100 ms'sinin RMS'i (dB). Bütün dosyanın RMS'i uzunluğa bağlı olduğu için
    farklı uzunluktaki sesler bununla karşılaştırılır (esya_sesleri_uret.py ile aynı ölçü)."""
    n = int(0.1 * hiz)
    en = 1e-9
    for i in range(0, max(1, len(x) - n), n // 4):
        p = x[i:i + n]
        r = (sum(v * v for v in p) / max(1, len(p))) ** 0.5
        en = max(en, r)
    import math
    return 20 * math.log10(en)


def _adim_baslangiclari(z):
    """Zarftan adım başlangıçlarının pencere numaraları."""
    s = sorted(z)
    medyan = s[len(s) // 2]
    esik = max(medyan * 6, s[int(len(s) * 0.9)] * 0.8)
    baslar, son = [], -10 ** 9
    for i in range(1, len(z)):
        if z[i] > esik and z[i - 1] <= esik and (i - son) * 0.005 > 0.3:
            # Eşik, adımın yükselen kenarının ORTASINDA aşılır; topuğun ilk değişi daha öncedir.
            # Oradan kesilseydi adımın vuruşu kesik başlardı (ilk denemede ilk 12 ms'de -12 dB
            # ses vardı). Bu yüzden geriye, gücün adımın tepesinin %10'unun ya da kayıt
            # gürültüsünün 2 katının altına indiği yere kadar yürünür (en çok 150 ms).
            tepe = max(z[i:i + 20])
            alt = max(medyan * 2, tepe * 0.1)
            j = i
            while j > 0 and z[j - 1] > alt and i - j < 30:
                j -= 1
            baslar.append(j)
            son = i
    return baslar


def adimlar():
    """#101: dört zeminin adımları gerçek kayıttan, zemin başına 8 çeşit."""
    for kayit, onek, hedef_db, alcak_hz in ADIM_KAYITLARI:
        x = oku(kayit, H44)
        z = _zarf(x, H44)
        baslar = _adim_baslangiclari(z)
        # Pencere 220 örnek = 4,989 ms (tam 5 ms değil); 0,005 ile çarpılsaydı 20. saniyede
        # kesim 44 ms geç kalır, adımın vuruşu kesilirdi (ilk denemede tam olarak bu oldu).
        pencere = int(0.005 * H44) / H44
        zamanlar = [b * pencere for b in baslar]
        adaylar = []
        for k, t in enumerate(zamanlar):
            if t < 0.5 or t > len(x) / H44 - 1.0:
                continue                                   # kaydın başı ve sonu (kayıt tuşu, nefes)
            onceki = zamanlar[k - 1] if k > 0 else -10.0
            sonraki = zamanlar[k + 1] if k + 1 < len(zamanlar) else 10 ** 9
            if t - onceki < 0.3 or sonraki - t < 0.45:
                continue                                   # üst üste binen adım
            parca = x[int((t - 0.01) * H44):int((t + 0.40) * H44)]
            adaylar.append((t, max(abs(v) for v in parca), parca))
        if len(adaylar) < ADIM_SAYISI:
            raise RuntimeError(f"{kayit}: yalnızca {len(adaylar)} temiz adım bulundu")
        # En gür ve en kısık %20 atılır; kalanlardan zamana yayılmış 8 tane.
        sirali = sorted(adaylar, key=lambda a: a[1])
        kes_ = len(sirali) // 5
        orta = sorted(sirali[kes_:len(sirali) - kes_], key=lambda a: a[0])
        if len(orta) < ADIM_SAYISI:
            orta = sorted(adaylar, key=lambda a: a[0])
        secilen = [orta[int(i * (len(orta) - 1) / (ADIM_SAYISI - 1))] for i in range(ADIM_SAYISI)]
        for i, (t, _, parca) in enumerate(secilen, 1):
            p = ortak.yuksek(list(parca), 60, H44)
            if alcak_hz:
                p = ortak.alcak(p, alcak_hz, H44)
            p = kes(p, 0.0, len(p) / H44, giris=0.002, cikis=0.12, hiz=H44)
            # Seviye: en gür 100 ms hedefe; tepe 0,8'i geçmesin (kaydet tepeye ölçekler, o yüzden
            # tepe değerini kendimiz hesaplayıp veriyoruz).
            simdi = _en_gur_100ms(p, H44)
            kazanc = 10 ** ((hedef_db - simdi) / 20)
            tepe = min(0.8, max(abs(v) for v in p) * kazanc)
            _kaydet(SESLER, f"{onek}_{i}.wav", p, tepe)
        print(f"  {onek}: {len(zamanlar)} adım bulundu, {len(adaylar)} temiz, 8 seçildi "
              f"({', '.join(f'{a[0]:.2f}' for a in secilen)} sn)")


# ---------- GÖLGE'NİN İMZASI: ANAHTARLIK (#105) ----------
# Şahan'ın kıyaslama turu (2026-10-02): varlığın yüzü ve sesi yok, her belirişi rastgele hissediliyor.
# Korku oyunlarında canavarın bir İMZA SESİ olur (Amnesia'nın hırıltısı, Phasmophobia'nın kalp atışı);
# oyuncu ikinci karşılaşmada o sesi duyunca donar. Bizim varlığımız eski bir kapıcı: imzası bir kapıcının
# anahtarlığıdır. Üç kısa şıkırtı, her biri bir öncekinden biraz daha pes (anahtarlık yavaşça sallanıyor),
# uzak bir koridordan gelir gibi boğuk ve yankılı. Kaynak mevcut anahtar kaydı (S24, CC0); indirme yok.
def _yeniden_ornekle(ornekler, oran):
    """Sesi 'oran' kat yavaş çalar (0,9 = daha pes ve uzun). En yakın örneği alır; şıkırtı için yeter."""
    n = int(len(ornekler) / oran)
    return [ornekler[min(len(ornekler) - 1, int(i * oran))] for i in range(n)]


def rev_anahtarlik():
    """S76: anahtarlığın üç şıkırtısı, koridor yankısıyla (Gölge'nin imzası, GolgeImzasi.Cal)."""
    k = oku("248037__sophiehall3535__pick-up-keys-1.wav", H44)
    toplam = sessizlik(2.6, H44)
    # Kayıttan üç farklı kesit; her biri biraz daha pes, aralar 0,33 ve 0,30 sn (düzensiz: elle sallanıyor).
    for bas, oran, zaman, guc in ((0.05, 1.00, 0.00, 1.0), (0.40, 0.92, 0.33, 0.85), (0.75, 0.85, 0.63, 0.7)):
        parca = kes(k, bas, bas + 0.34, giris=0.003, cikis=0.12, hiz=H44)
        parca = _yeniden_ornekle(parca, oran)
        i0 = int(zaman * H44)
        for i, v in enumerate(parca):
            if i0 + i < len(toplam): toplam[i0 + i] += v * guc
    s = ortak.alcak(toplam, 3200, H44)                                        # uzaktan: tizler duvarlarda kaybolur
    s = ortak.yanki(s, H44, boyut=0.9, sonum=0.5, islak=0.45, kuru=0.8, oda=1.6)   # uzun koridor
    _kaydet(SESLER, "Anahtarlik.wav", s, 0.8)


URETICILER_74 = {"damla": rev_damla, "anahtar": rev_anahtar, "cam": rev_cam, "devrilme": rev_devrilme,
                 "vida": rev_vida, "demir_sarsma": rev_demir_sarsma, "kol": rev_kol, "sigorta": rev_sigorta,
                 "sifre": rev_sifre, "zil": rev_zil, "ic_kilitli": rev_ic_kilitli, "asansor_zili": rev_asansor_zili,
                 "gicirti": rev_gicirti, "adimlar": adimlar, "anahtarlik": rev_anahtarlik}


def rev_ic_acilma():
    """S26: BigSoundBank #1159, "Door squeaks and rubs #1" (CC0). Kolun gıcırtısı ve kanadın yere
    sürtünmesi: sarkmış bir 90'lar kapısı. Kaydın ilk 2,8 sn'si açılış (sonrası kapanış, kullanılmıyor).
    Çekmece, dolap kapağı ve sandık da bu sesi kullanır (eskisi gibi)."""
    k = oku("bigsoundbank_1159_door_squeaks_and_rubs_1.wav", H44)
    _kaydet(SESLER, "IcKapi_Acilma.wav", kes(k, 0.0, 2.8, giris=0.005, cikis=0.4, hiz=H44))


def rev_demir_gicirti():
    """S31: BigSoundBank #0683, "Grinding metal gate" (CC0). Demir kat kapısının açılış gıcırtısı;
    6,4-8,4 sn arası (kaydın en uzun, tek parça gıcırtısı; kanat 1,6 sn'de açılıyor)."""
    k = oku("bigsoundbank_0683_grinding_metal_gate.wav", H44)
    _kaydet(SESLER, "KatKapisi_Gicirti.wav", kes(k, 6.4, 8.4, giris=0.02, cikis=0.4, hiz=H44))


def rev_kilit():
    """S32: BigSoundBank #0378, "Lock and big key" (CC0). Büyük eski anahtar deliğe girer ve döner;
    ilk çekim, 0-1,8 sn. Kat kapısı, çatı kapısı ve her çözülen kilit bu sesi kullanır."""
    k = oku("bigsoundbank_0378_lock_and_big_key.wav", H44)
    _kaydet(SESLER, "KatKapisi_Kilit.wav", kes(k, 0.0, 1.8, giris=0.005, cikis=0.3, hiz=H44))


def rev_asansor():
    """S40: BigSoundBank #1712, "Elevator, 6 floors" (CC0). Eski bir Paris binasının küçük asansörü;
    kayıt: kapılar kapanır ve motor kalkar (1,8-6,5 sn), yolculuk (7-22 sn düzenli uğultu),
    duruş ve kapıların açılması (35,8 sn - son). Üç parça:
      Asansor_Ugultu.wav   yolculuk uğultusu, 9-21 sn, son 1 sn başın üstüne karışır: dikişsiz döngü
                           (asansör gelirken ve dokuzuncu kata dönerken döner)
      Asansor_Varis.wav    duruş ve kapıların açılması (varışta, zilden hemen önce)
      Asansor_Kalkis.wav   kapıların kapanması ve motorun kalkışı (asansör geri dönerken)"""
    k = oku("bigsoundbank_1712_elevator_6_floors.wav", H44)
    dongu = kes(k, 9.0, 21.0, giris=0.0, cikis=0.0, hiz=H44)
    gecis = int(1.0 * H44)
    n = len(dongu)
    for i in range(gecis):
        a = i / gecis
        dongu[i] = dongu[n - gecis + i] * (1 - a) + dongu[i] * a
    _kaydet(ANOMALI, "Asansor_Ugultu.wav", dongu[:n - gecis])
    _kaydet(SESLER, "Asansor_Varis.wav", kes(k, 35.8, len(k) / H44, giris=0.05, cikis=0.4, hiz=H44))
    _kaydet(SESLER, "Asansor_Kalkis.wav", kes(k, 1.8, 6.5, giris=0.01, cikis=0.6, hiz=H44))


URETICILER_74.update({"ic_acilma": rev_ic_acilma, "demir_gicirti": rev_demir_gicirti, "kilit": rev_kilit,
                      "asansor": rev_asansor})

URETICILER = {"bebek": bebek, "tv": tv, "guvercin": guvercin, "kapi_carpma": kapi_carpma, "kapi_tiklatma": kapi_tiklatma,
              "muzik": muzik, "dis_silah": dis_silah, "dis_kopek": dis_kopek,
              "radyo": radyo}
URETICILER.update(URETICILER_74)


# ---------- TELEFON KONUŞMALARI (#90) ----------
# Kapıcı odasındaki telefona gelen aramalar (2. kat Nuri, 5. kat Hasan) seslendirilecek.
# Proje kuralı: insan sesi asla sentezlenmez; konuşmaları Şahan kendisi okuyup kaydeder
# (nasıl kaydedileceği Docs/seslendirme.md'de). Bu işlemci o temiz kaydı alıp 90'ların
# Türk şehir hattından, bakalit bir ahizeden duyulan sese çevirir.
#
#   python Art/Ses/scripts/kayit_isle.py telefon <girdi> <cikti_adi>
#     girdi:     Art/Ses/kayitlar/ içindeki dosya adı (wav, m4a, mp3...) ya da tam bir yol
#     cikti_adi: oyundaki ad, uzantısız (ör. telefon_kat2)
#     çıktı:     Assets/Resources/Sesler/Konusmalar/<cikti_adi>.wav  (44,1 kHz, mono, 16 bit)
#
# Öbür üreticilerden farkı: argüman ister. O yüzden URETICILER sözlüğünde DEĞİLDİR; betik
# adsız çalıştırılınca bütün sesleri yeniden üretirken konuşmalara dokunmaz (dokunamaz da:
# hangi kaydı işleyeceğini bilmez). Aşağıdaki __main__ onu ayrı bir dalda ele alır.
#
# NEDEN BÖYLE DUYULUR: 90'ların analog telefon hattı sesi 300-3400 Hz arasına sıkıştırır
# (hattın "ses bandı"). Bunun altındaki gövde (göğüs tınısı) ve üstündeki parlaklık ("s",
# "ş" harflerinin tizliği) yolda kalır; kulak tam da bu eksikliği "telefondan geliyor" diye
# tanır. Ahizedeki karbon mikrofon ise gür heceleri ezer (hafif bir hırıltı), hat boşken
# de şebekenin 50 Hz vınlaması ve cılız bir hışırtı duyulur. Dört adım bunları taklit eder.
KONUSMALAR = os.path.join(SESLER, "Konusmalar")


def telefon(dosya_adi, cikti_adi, klasor=None):
    """Temiz bir ses kaydını 90'lar ahize sesine çevirip Konusmalar/<cikti_adi>.wav yazar.
    'klasor' yalnızca deneme içindir (çıktıyı Assets dışına yazmak için)."""
    import math
    H = H44

    # Girdi ya kayitlar/ klasöründe bir dosya adıdır ya da var olan bir yol (deneme için).
    girdi = os.path.abspath(dosya_adi) if os.path.isfile(dosya_adi) else dosya_adi
    ses = oku(girdi, H)                                        # Blender çevirir, -1..1 örnekler

    # 1. HAT BANDI: 300 Hz altı ve 3400 Hz üstü atılır. Biquad süzgeçler tek geçişte yumuşak
    #    bir yamaç çizer; iki kez geçince yamaç dikleşir ve bant "telefon gibi" sertleşir.
    for _ in range(2):
        ses = ortak.alcak(ortak.yuksek(ses, 300, H), 3400, H)
    # Üstüne 3600 Hz'de iki geçiş daha: eski hatlar tizleri gerçekten keskin keser. Örnek hızı
    # 44,1 kHz kalır (Unity'ye dokunulmaz); "8 kHz'lik hat" hissini bu sert kesim verir.
    for _ in range(2):
        ses = ortak.alcak(ses, 3600, H)

    # 2. KARBON MİKROFON: gür heceler yumuşakça ezilir (tanh). ses_ortak'taki darbeler için
    #    1-4 arası "surus" kullanılır; burada 3,2 ile hafif ama duyulur bir hırıltı oluşur.
    #    (Fonksiyon önce tepeyi 1'e eşitler; o yüzden sonuç -1..1 arasındadır.)
    ses = ortak.yumusak_sinirla(ses, 3.2)

    # 3. HAT YATAĞI: 50 Hz şebeke vınlaması (üstüne cılız 100 Hz harmoniği; trafo vınlaması
    #    hep ikinci harmoniğini de taşır) ve hattın hışırtısı (bant sınırlı gürültü).
    #    Seviye -45 dB: 10^(-45/20) ≈ 0,0056. Konuşmanın tepesi 1 iken bu, ancak sessiz
    #    anlarda fark edilen bir taban: hattın "açık" olduğunu söyler, konuşmayı bozmaz.
    n = len(ses)
    taban = 10 ** (-45 / 20)
    hisirti = ortak.tepeyi_esitle(ortak.bant(ortak.gurultu(n), 1800, H, 0.5))
    for i in range(n):
        t = i / H
        vinlama = 0.8 * math.sin(2 * math.pi * 50 * t) + 0.3 * math.sin(2 * math.pi * 100 * t)
        ses[i] += taban * (vinlama + 0.7 * hisirti[i])

    # 4. YAZ: tepe 0,8'e ölçeklenir (öbür oyun sesleriyle aynı), 44,1 kHz mono 16 bit.
    hedef = os.path.join(klasor or KONUSMALAR, cikti_adi + ".wav")
    ortak.kaydet(hedef, ses, H, 0.8)
    return hedef


# ---------- RADYO ANONSU (#106) ----------
# Her katta radyodan ya da televizyondan 1974'teki olaya dolaylı değinen bir haber cümlesi gelir
# (RadyoAnonsu bileşeni): önce parazit, sonra seslendirilmiş cümle, sonra yine parazit.
DAIRE = os.path.join(KOK, "Assets", "Audio", "Daire")


def radyo_parazit():
    """Resources/Sesler/Radyo_Parazit.wav: 2,5 sn bant gürültüsü ve seyrek çıtırtı; anonsun önü ve arkası. Döngü için kenarlar
    birbirine karışır (son 0,2 sn başın üstüne)."""
    import random
    random.seed(106)
    H = H44
    n = int(2.5 * H)
    parazit = ortak.tepeyi_esitle(ortak.bant(ortak.gurultu(n), 1800, H, 0.6))
    for _ in range(12):
        cit = ortak.darbe(int(0.02 * H), H, 0.004)
        ortak.ekle(parazit, cit, random.randrange(n - int(0.02 * H)), random.uniform(0.3, 1.0))
    gecis = int(0.2 * H)
    for i in range(gecis):
        a = i / gecis
        parazit[i] = parazit[n - gecis + i] * (1 - a) + parazit[i] * a
    parazit = parazit[:n - gecis]
    ortak.kaydet(os.path.join(SESLER, "Radyo_Parazit.wav"), parazit, H, 0.6)


def fisilti(dosya_adi, cikti_adi, klasor=None):
    """Fısıltı kaydını (#132) oyuna hazırlar: Sesler/Fisilti_N.wav. Kafa lambası sönünce 2B çalar.
    Bant 300-5000 Hz (hava sesi kalsın, oda uğultusu gitsin), hafif sıkıştırma (tanh 1,8: hecelerin
    tepesi düzleşir, fısıltı "yakın" gelir), baş ve sonda 60 ms iniş-çıkış (tık olmasın), tepe 0,5."""
    H = H44
    girdi = os.path.abspath(dosya_adi) if os.path.isfile(dosya_adi) else dosya_adi
    ses = oku(girdi, H)
    for _ in range(2):
        ses = ortak.alcak(ortak.yuksek(ses, 300, H), 5000, H)
    ses = ortak.yumusak_sinirla(ses, 1.8)
    k = int(0.06 * H)
    n = len(ses)
    for i in range(min(k, n)):
        ses[i] *= i / k
        ses[n - 1 - i] *= i / k
    ad = cikti_adi[:1].upper() + cikti_adi[1:]           # fisilti_1 -> Fisilti_1 (Resources/Sesler adlandırması)
    hedef = os.path.join(klasor or SESLER, ad + ".wav")
    ortak.kaydet(hedef, ses, H, 0.5)
    return hedef


def radyo_anons(dosya_adi, cikti_adi, klasor=None):
    """Temiz bir okuma kaydını 90'lar radyo yayınına çevirir: AM bandı (250-4200 Hz, iki geçiş), hafif
    sıkıştırma, -32 dB parazit yatağı. Çıktı Konusmalar/<cikti_adi>.wav (RadyoAnonsu adıyla yükler).
    Kullanım: python kayit_isle.py radyo_anons <girdi> <cikti_adi>   (ör. radyo_anons radyo_kat3.wav radyo_kat3)"""
    import math, random
    H = H44
    girdi = os.path.abspath(dosya_adi) if os.path.isfile(dosya_adi) else dosya_adi
    ses = oku(girdi, H)
    for _ in range(2):
        ses = ortak.alcak(ortak.yuksek(ses, 250, H), 4200, H)
    ses = ortak.yumusak_sinirla(ses, 2.5)
    n = len(ses)
    random.seed(7)
    taban = 10 ** (-32 / 20)
    hisirti = ortak.tepeyi_esitle(ortak.bant(ortak.gurultu(n), 1800, H, 0.6))
    for i in range(n):
        ses[i] += taban * hisirti[i]
    hedef = os.path.join(klasor or KONUSMALAR, cikti_adi + ".wav")
    os.makedirs(os.path.dirname(hedef), exist_ok=True)
    ortak.kaydet(hedef, ses, H, 0.8)
    return hedef


URETICILER["radyo_parazit"] = radyo_parazit   # (#106) üretici tablosu yukarıda kuruldu; fonksiyon burada tanımlı


if __name__ == "__main__":
    argumanlar = sys.argv[1:]
    if argumanlar and argumanlar[0] == "radyo_anons":
        if len(argumanlar) != 3:
            print("Kullanım: python kayit_isle.py radyo_anons <girdi> <cikti_adi>")
            sys.exit(1)
        radyo_anons(argumanlar[1], argumanlar[2])
    elif argumanlar and argumanlar[0] == "fisilti":
        if len(argumanlar) != 3:
            print("Kullanım: python kayit_isle.py fisilti <girdi> fisilti_N")
            sys.exit(1)
        fisilti(argumanlar[1], argumanlar[2])
    elif argumanlar and argumanlar[0] == "telefon":
        # Konuşma işlemcisi argüman ister: python kayit_isle.py telefon <girdi> <cikti_adi>
        if len(argumanlar) != 3:
            print("Kullanım: python kayit_isle.py telefon <girdi> <cikti_adi>   (ör. telefon telefon_kat2.wav telefon_kat2)")
            sys.exit(1)
        telefon(argumanlar[1], argumanlar[2])
    else:
        for ad in (argumanlar or list(URETICILER)):
            URETICILER[ad]()
