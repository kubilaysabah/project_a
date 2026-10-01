# Seslendirme rehberi — telefon konuşmaları (#90)

Kapıcı odasındaki telefona iki arama gelir (2. katta Nuri, 5. katta Hasan). Bugün konuşma not ekranında
okunuyor; #90 ile ahizeden **gerçek bir sesle** de duyulacak. Proje kuralı: insan sesi asla sentezlenmez,
kayıtları Şahan kendisi okur. Bu belge neyi, nasıl kaydedeceğini ve dosyanın oyuna nasıl gireceğini anlatır.

## 1. Okunacak metinler

Metinler `Assets/Resources/Metinler.txt` tablosundan, `not.telefon` ile başlayan satırlardan alındı
(tablodaki `\n` satır başıdır). Dört anahtar var; **ikisi seslendirilir**, ikisi kâğıda yazılı not:

| Anahtar | Ne | Seslendirme |
|---|---|---|
| `not.telefon_kat2` | Nuri'nin aramasının kendisi | **Evet** → dosya `telefon_kat2` |
| `not.telefon_kat5` | Hasan'ın aramasının kendisi | **Evet** → dosya `telefon_kat5` |
| `not.telefon_kat2_kacti` | Arama kaçırılınca yönetimin bıraktığı yazılı not | Hayır (kâğıt) |
| `not.telefon_kat5_kacti` | Arama kaçırılınca yönetimin bıraktığı yazılı not | Hayır (kâğıt) |

`not.` ile başlayan öbür anahtarlar (`not.yonetici` kapıcı odasındaki yönetici notu, `not.katN_*` katlardaki
notlar) hep yazılı kâğıtlardır; seslendirilecek başka satır yok.

### `telefon_kat2` — Nuri (3 numara; yaşlı, hastanede, yorgun ama kibar; sonda sesi alçalır)

> — Alo? Kapıcı mısın? Ben Nuri, 3 numaradan.
> Hastanedeyim, merak etme.
>
> Bir şey rica edeceğim evladım:
> radyomu kapatma. Açık kalsın.
> Kapatırsan duyarlar.
>
> — Kim mi? Dokuzuncu kattakiler.

Parantezli son satır *(Hat kesiliyor.)* okunmaz; hattın kesilmesi sestir, kelime değil. "Kim mi?" sorusunun
önünde kapıcının (oyuncunun) duyulmayan bir sorusu var: oradan önce 1-1,5 saniye bekle, sonra devam et.

### `telefon_kat5` — Hasan (eski kapıcı; köyden arıyor, tedirgin, hızlı ve alçak sesle, uyarır gibi)

> — Alo... Cemal? Cemal sen misin?
> — Değilsin. Yenisin demek.
>
> Ben Hasan. Eski kapıcı. Köydeyim.
> Dinle beni: yedinci katta dört kapının dördü de 13.
> Gerçeğinin önünde paspas yoktur, terlik yoktur.
> Gözetleme deliğinden ışık gelir.
>
> Bir de... asansör dokuzu gösterirse binme.

Çizgili iki satır ayrı iki replik: "Cemal sen misin?"den sonra bir nefes payı bırak, sonra "Değilsin." gelsin.

## 2. Nasıl kaydedilir

- **Sessiz oda**, kapı pencere kapalı, bilgisayar fanından uzak. Gece sessizliği iyidir.
- **Telefonun sesli not uygulaması yeter**; mikrofon ağızdan **~30 cm** uzakta, hafif yana (nefes patlamaları
  "p"/"b" harflerinde mikrofona vurmasın).
- Her metni **iki kez** oku (aynı dosyaya olabilir); temizi işlerken seçeriz.
- Başta ve sonda **1 saniye sessizlik** bırak (kesme ve geçişler için gerekir).
- Kısık değil, normal konuşma sesi; işlemci telefon kısıklığını kendi verir. Ses "telefonda konuşur gibi"
  yapılmaya çalışılmaz, sadece karakterin ruh hâli oynanır.
- Biçim: **WAV** en iyisi; telefonun verdiği **M4A** da olur. Blender'ın `aud` modülü her ikisini de açar:
  `kayit_isle.py` içindeki `oku()` dosyayı Blender'ı penceresiz çalıştırarak
  (`blender -b --factory-startup --python Art/Ses/scripts/kayit_donustur.py -- <girdi> <çıktı.wav> 44100 1`)
  44,1 kHz mono WAV'a çevirir; elle çalıştırmaya gerek yok. Stereo kayıt mono'ya indirilir.

## 3. Dosya adları ve yerleri

- Ad = anahtarın `not.` öneki atılmış hâli: `not.telefon_kat2` → **`telefon_kat2`**, `not.telefon_kat5` → **`telefon_kat5`**.
  Her arama için bir dosya.
- Ham kayıt **değiştirilmeden** `Art/Ses/kayitlar/` klasörüne konur: `telefon_kat2.wav` (ya da `.m4a`), `telefon_kat5.wav`.
  Klasör LFS'tedir; büyük dosya sorun değil.
- `Art/Ses/kayitlar/LISANSLAR.md` tablosuna her dosya için bir satır eklenir (tablonun sütunları: Dosya | Oyundaki ses | Kaynak | Yükleyen | Lisans):

  ```
  | `telefon_kat2.wav` | 2. kat telefon konuşması, Nuri (#90) | Kendi kaydı | Şahan Boz | Şahan Boz, kendi kaydı, tüm hakları saklı |
  | `telefon_kat5.wav` | 5. kat telefon konuşması, Hasan (#90) | Kendi kaydı | Şahan Boz | Şahan Boz, kendi kaydı, tüm hakları saklı |
  ```

## 4. İşleme komutu

Depo kökünden:

```
python Art/Ses/scripts/kayit_isle.py telefon telefon_kat2.wav telefon_kat2
python Art/Ses/scripts/kayit_isle.py telefon telefon_kat5.wav telefon_kat5
```

İlk argüman `Art/Ses/kayitlar/` içindeki dosya adı (ya da tam bir yol), ikincisi oyundaki ad (uzantısız).
Çıktı: `Assets/Resources/Sesler/Konusmalar/<ad>.wav` (44,1 kHz, mono, 16 bit, tepe 0,8). İşlemci kaydı 90'ların
ahizesine çevirir: 300-3400 Hz hat bandı (iki geçişli biquad) + 3,6 kHz'de sert tiz kesimi, karbon mikrofon
hırıltısı için yumuşak kırpma, −45 dB'de 50 Hz şebeke vınlaması ve hat hışırtısı. Betik adsız çalıştırılınca
(`python kayit_isle.py`) konuşmalara dokunmaz; yalnızca `telefon` komutuyla üretilirler.

Bir kaydı yeniden okursan aynı komut dosyanın üstüne yazar; Unity `.meta` dosyası değişmez.

## 5. Oyunda

Oyun, arama açılınca `Resources/Sesler/Konusmalar/telefon_katN` dosyasını arar ve **varsa** ahizeden çalar;
yoksa bugünkü gibi yalnızca not ekranı görünür. Dosyayı koymak yeter, kod ya da sahne değişikliği gerekmez.
