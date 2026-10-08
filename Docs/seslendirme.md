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

### `telefon_kat2` — Nuri (7 numara; yaşlı, hastanede, yorgun ama kibar; sonda sesi alçalır)

> — Alo? Kapıcı mısın? Ben Nuri, 7 numaradan.
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
> Üç harfli oradan iner.

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

## 6. Radyo haber cümleleri (#106)

Her katta daireye girdikten bir süre sonra o katın ses cihazı (radyo ya da televizyon) cızırtıyla açılır,
**tek bir cümle** söyler ve cızırtıya döner. Sekiz cümle aynı olaya (1974, Huzur Apartmanı'nın yapımı ve
sayılmayan dokuzuncu kat) başka başka açılardan değinir; hiçbiri oyuncuya ne arayacağını söylemez (#83).
Hepsi 90'ların TRT haber / gece kuşağı tonunda okunur: düz, resmî, hafif yorgun bir spiker sesi; 2. ve 4. kat
cümleleri "dinleyici notu" olarak biraz daha samimi okunabilir. Her cümle 8-14 saniye.

Dosya adları `radyo_kat1` … `radyo_kat8`; işleme: `python Art/Ses/scripts/kayit_isle.py radyo_anons <girdi> radyo_katN`
(AM bandı 250-4200 Hz, hafif sıkıştırma, parazit yatağı; çıktı `Assets/Resources/Sesler/Konusmalar/`).
2. katın cümlesi şimdilik oyunda ÇALMAZ: o katın radyosu bulmacanın parçası (tango yayını); cümle ileride
tango istasyonunun şarkı arasına girecek.

| Kat | Cihaz | TR | EN |
|---|---|---|---|
| 1 | koli üstündeki radyo-teyp | Saatlerimiz gece on ikiyi gösteriyor. Belediye Fen İşleri, 1974'teki arşiv yangınında kaybolan yapı ruhsatlarının yenilenmesi için son başvuru tarihini uzattı. Ayrıntılar sabah bülteninde. | It is now midnight. The Municipal Works Department has extended the deadline to replace the building permits lost in the archive fire of 1974. Details in the morning bulletin. |
| 2 | Nuri'nin radyosu (tango arası, ileride) | Gece Kuşağı'na bir dinleyici notu: "Bizim apartmanda geceleri üst kattan ayak sesi geliyor, ama üst kat yok." Şakacı dinleyicimize iyi geceler. | A listener's note for the night programme: "In our building you hear footsteps from the floor above at night, but there is no floor above." Good night to our joker. |
| 3 | tüplü televizyon | Şehir haberleri: Eski Fen İşleri binasındaki tadilatta, 1974 tarihli bir projede bir katın sonradan silindiği görüldü. Yetkililer "kayıt hatası" dedi. | City news: during repairs at the old Works Department, a 1974 project was found to have one floor erased after the fact. Officials called it a clerical error. |
| 4 | kamp yatağındaki cep radyosu | Dinleyicimiz soruyor: "Elektrikler kesilince komşunun penceresi nasıl yanıyor?" Elektrik İdaresi, yirmi yıldır tespit edilemeyen kaçak hatlar olduğunu kabul etti. | A listener asks: "How does the neighbour's window stay lit during a power cut?" The electricity board has admitted there are illegal lines it has failed to trace for twenty years. |
| 5 | TV ünitesi | Kayıp ilanı: Çarşamba gecesinden beri haber alınamayan bir apartman görevlisi için ailesi yardım istiyor. Son görüldüğünde iş önlüğü ve el feneriyle merdivenlere çıkmış. | Missing person: the family of a building caretaker unheard of since Wednesday night asks for help. He was last seen going up the stairs in his work apron, carrying a torch. |
| 6 | eski televizyon | Arşivden: 1974 Ekim'inde bu mahallede teslim edilen bir apartmanın açılışına yüklenici gelmedi. Gazete "dokuzuncu kattan inmedi" yazmış, düzeltmeyi ertesi gün basmıştı. | From the archive: in October 1974 the contractor failed to attend the opening of a building delivered in this district. The paper wrote "he never came down from the ninth floor" and printed a correction the next day. |
| 7 | taburedeki portatif TV | Gece yarısı bülteni: İtfaiye, bir apartmanın çatısından gelen ışık ihbarına gitti; çatıda kimse yoktu. İhbarı yapan, "her gece aynı pencere" dedi. | Midnight bulletin: the fire brigade answered a report of a light on an apartment building's roof; no one was there. The caller said, "the same window every night." |
| 8 | yönetimin masa radyosu | Son dakika: Belediye, iskân ruhsatı olmayan katlar için af yasasının bu gece yarısı yürürlüğe girdiğini duyurdu. Yirmi yıldır "olmayan" katlar artık kayıtlara geçecek. | Breaking: the municipality announced that the amnesty for floors without an occupancy permit takes effect at midnight tonight. Floors that "did not exist" for twenty years will now enter the records. |

## 7. Fısıltılar (#132)

Kafa lambası kendiliğinden sönünce (gerilim karartması, 2. katın dersi, 4. kattaki kodun parlaması) karanlıkta
1-2 saniyelik, çok kısık bir **fısıltı** duyulur: cin (kiracıların diliyle "üç harfli") kulağın dibindedir.
Kayıt yoksa sessizdir; dosyayı koymak yeter.

Ne söylenir: anlaşılmayan, nefesli, Türkçe hecelerle 1-2 saniye. Örnekler (her biri ayrı dosya):
`"...kapıcı..."`, `"...çık buradan..."`, `"...geldim..."`, anlamsız bir mırıltı. Kelime anlaşılmamalı; oyuncu
"bir şey mi dedi?" demeli. Ağız mikrofona 5-10 cm, hava sesi var, oda yankısı yok.

Dosya adları `fisilti_1`, `fisilti_2`, `fisilti_3` (en çok 3; lamba her sönüşte rastgele birini seçer).
İşleme: `python Art/Ses/scripts/kayit_isle.py fisilti <girdi> fisilti_N` → `Assets/Resources/Sesler/Fisilti_N.wav`
(300 Hz altı ve 5 kHz üstü atılır, hafif sıkıştırma, baş-son 60 ms kısılır; tepe 0,5: fısıltı asla yüksek
değil). Oyunda 2B çalar (kafanın içinde), oda yankısından muaf.
