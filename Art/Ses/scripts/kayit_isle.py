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


def rev_merdiven():
    """S20m: G.Lamont, "Concrete Stairs" (CC0). Merdivende ve sahanlıkta beton adımlar; kayıttaki
    dört adım tek tek (Merdiven_Adim_1-4). Adları "Adim" ile başlamıyor: AyakSesi o önekle
    başlayanların hepsini parke adımı sayıyor."""
    k = oku("118041__glamont__concrete-stairs.wav", H44)
    for i, bas in enumerate((4.47, 5.12, 5.72, 6.37), 1):
        _kaydet(SESLER, f"Merdiven_Adim_{i}.wav", kes(k, bas, bas + 0.4, giris=0.002, cikis=0.12, hiz=H44))


URETICILER_74 = {"damla": rev_damla, "anahtar": rev_anahtar, "cam": rev_cam, "devrilme": rev_devrilme,
                 "vida": rev_vida, "demir_sarsma": rev_demir_sarsma, "kol": rev_kol, "sigorta": rev_sigorta,
                 "sifre": rev_sifre, "zil": rev_zil, "ic_kilitli": rev_ic_kilitli, "asansor_zili": rev_asansor_zili,
                 "gicirti": rev_gicirti, "merdiven": rev_merdiven}


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

if __name__ == "__main__":
    for ad in (sys.argv[1:] or list(URETICILER)):
        URETICILER[ad]()
