# Anomali görünürlük denetimi (#33)

`Huzur → Anomali → Görünürlük Denetimi` ile üretildi (2026-09-25 11:40). Elle düzenlemeyin; aracı yeniden çalıştırın.

**Ne ölçüldü:** her kart için sahanlık, oyuncunun göz hizasından iki noktadan (kata varış noktası: 4 yön, sahanlığın ortası: 8 yön) normal ve anomalili hâlde çizildi. Zamanlı kartlarda her etkinin gerçekleştiği an ayrı ölçüldü. Kartın değeri en görünür olduğu yön ve andaki değerdir.

- **Alan:** belirgin biçimde (bir renk kanalında 0,03'ten fazla) değişen piksellerin ekrana oranı. Renk değişimleri de sayılır (yeşil ampul gibi).
- **Güç:** değişen piksellerin ortalama farkı (0-1). Küçük ama net bir değişiklik (kapı numarası) alanı küçük, gücü büyüktür.
- **Sınıf** (sınırlar sezgisel başlangıç değerleri; Şahan'ın gözüyle karşılaştırılıp ayarlanacak): görünmez (alan < %0,05) · silik (güç < 0,12) · küçük ama net (alan < %0,3) · belirgin.
- **Gürültü tabanı:** %0,00 (aynı sahne iki kez çizilince ölçülen fark; 0'a yakınsa ölçüm güvenilir).
- ⚠ = tasarım zorluğu 1-2 ("kolay") olduğu hâlde görünmez ya da silik çıkan kart. ↓ = gece efektleri (#31) kartı daha düşük bir sınıfa indirmiş.

## Görsel kartlar (en az görünenden en çok görünene)

| # | Kart | Katman | Zorluk | Efektli alan · güç | Efektsiz alan · güç | Sınıf (efektli) | Not |
|---|---|---|---|---|---|---|---|
| 1 | `eksik_kapi_gozu` | Fisilti | 5 | %0,00 · 0,13 | %0,00 · 0,18 | görünmez |  |
| 2 | `yer_degistiren_zil` | Fisilti | 4 | %0,01 · 0,13 | %0,01 · 0,20 | görünmez |  |
| 3 | `kaymis_zil` | Fisilti | 4 | %0,01 · 0,20 | %0,01 · 0,28 | görünmez |  |
| 4 | `ters_numara` | Fisilti | 4 | %0,01 · 0,04 | %0,01 · 0,08 | görünmez |  |
| 5 | `eksik_kol` | Fisilti | 3 | %0,01 · 0,12 | %0,01 · 0,16 | görünmez |  |
| 6 | `eksik_plaka` | Fisilti | 2 | %0,02 · 0,04 | %0,03 · 0,13 | görünmez | ⚠ kolay sanılıyor |
| 7 | `yuksek_kol` | Fisilti | 4 | %0,05 · 0,07 | %0,06 · 0,11 | silik |  |
| 8 | `donuk_terlik` | Fisilti | 4 | %0,08 · 0,08 | %0,09 · 0,14 | silik | ↓ efektsiz: küçük ama net |
| 9 | `tavandaki_terlik` | Tekinsiz | 2 | %0,12 · 0,07 | %0,14 · 0,15 | silik | ⚠ kolay sanılıyor, ↓ efektsiz: küçük ama net |
| 10 | `tek_terlik` | Fisilti | 3 | %0,12 · 0,07 | %0,14 · 0,15 | silik | ↓ efektsiz: küçük ama net |
| 11 | `yuruyen_terlik` | Tekinsiz | 3 | %0,15 · 0,09 | %0,16 · 0,16 | silik | ↓ efektsiz: küçük ama net, zamanlı |
| 12 | `kisa_kanat` | Fisilti | 5 | %0,47 · 0,08 | %0,47 · 0,16 | silik | ↓ efektsiz: belirgin |
| 13 | `devrik_saksi` | Fisilti | 3 | %0,50 · 0,09 | %0,88 · 0,08 | silik |  |
| 14 | `eksik_sayac` | Fisilti | 3 | %0,51 · 0,07 | %0,76 · 0,09 | silik |  |
| 15 | `eksik_paspas` | Fisilti | 2 | %0,54 · 0,06 | %0,67 · 0,09 | silik | ⚠ kolay sanılıyor |
| 16 | `eksik_saksi` | Fisilti | 3 | %0,56 · 0,07 | %1,03 · 0,09 | silik |  |
| 17 | `besinci_kapi` | Kabus | 1 | %0,70 · 0,06 | %0,81 · 0,07 | silik | ⚠ kolay sanılıyor |
| 18 | `yan_sayac` | Fisilti | 3 | %0,88 · 0,08 | %1,36 · 0,09 | silik |  |
| 19 | `kaymis_paspas` | Fisilti | 3 | %0,96 · 0,06 | %1,19 · 0,10 | silik |  |
| 20 | `buyuk_paspas` | Fisilti | 3 | %1,39 · 0,07 | %1,61 · 0,11 | silik |  |
| 21 | `kapi_onunde_tup` | Fisilti | 3 | %1,55 · 0,11 | %1,82 · 0,15 | silik | ↓ efektsiz: belirgin |
| 22 | `dik_paspas` | Tekinsiz | 2 | %1,68 · 0,07 | %1,80 · 0,10 | silik | ⚠ kolay sanılıyor |
| 23 | `donmus_ayakkabilik` | Fisilti | 3 | %1,83 · 0,09 | %1,98 · 0,11 | silik |  |
| 24 | `aralik_kapi` | Tekinsiz | 2 | %4,89 · 0,06 | %4,82 · 0,07 | silik | ⚠ kolay sanılıyor |
| 25 | `yer_degistiren_ayakkabilik` | Fisilti | 3 | %5,51 · 0,07 | %7,69 · 0,11 | silik |  |
| 26 | `aralik_kapilar` | Tekinsiz | 1 | %11,33 · 0,09 | %10,84 · 0,12 | silik | ⚠ kolay sanılıyor |
| 27 | `tek_ayakkabi` | Tekinsiz | 3 | %0,05 · 0,15 | %0,09 · 0,18 | küçük ama net |  |
| 28 | `ters_not` | Fisilti | 4 | %0,09 · 0,36 | %0,10 · 0,36 | küçük ama net |  |
| 29 | `iceri_yuruyen_terlikler` | Tekinsiz | 3 | %0,10 · 0,12 | %0,28 · 0,13 | küçük ama net |  |
| 30 | `paspasta_gazete` | Fisilti | 4 | %0,10 · 0,26 | %0,10 · 0,28 | küçük ama net |  |
| 31 | `cekilen_gazete` | Tekinsiz | 2 | %0,10 · 0,26 | %0,10 · 0,28 | küçük ama net | zamanlı |
| 32 | `ters_terlikler` | Fisilti | 4 | %0,12 · 0,14 | %0,14 · 0,18 | küçük ama net |  |
| 33 | `acilan_sayac` | Tekinsiz | 2 | %0,29 · 0,16 | %0,92 · 0,17 | küçük ama net | ↓ efektsiz: belirgin, zamanlı |
| 34 | `acik_sayac` | Tekinsiz | 3 | %0,31 · 0,17 | %0,91 · 0,19 | belirgin |  |
| 35 | `eksik_dugme` | Fisilti | 3 | %0,32 · 0,16 | %0,16 · 0,25 | belirgin |  |
| 36 | `eksik_not` | Fisilti | 2 | %0,76 · 0,35 | %0,76 · 0,34 | belirgin |  |
| 37 | `yamuk_ilan` | Fisilti | 5 | %1,09 · 0,12 | %0,92 · 0,12 | belirgin |  |
| 38 | `devrik_ayakkabilik` | Tekinsiz | 2 | %2,63 · 0,12 | %3,97 · 0,12 | belirgin |  |
| 39 | `eksik_ilan` | Fisilti | 3 | %3,16 · 0,14 | %3,00 · 0,12 | belirgin |  |
| 40 | `carpan_kapi` | Tekinsiz | 2 | %3,33 · 0,13 | %6,86 · 0,17 | belirgin | zamanlı |
| 41 | `dusmus_ilan` | Fisilti | 3 | %4,05 · 0,13 | %4,71 · 0,11 | belirgin |  |
| 42 | `acik_kapi` | Tekinsiz | 1 | %11,01 · 0,20 | %11,03 · 0,35 | belirgin |  |
| 43 | `aralik_asansor` | Tekinsiz | 2 | %11,41 · 0,33 | %11,53 · 0,47 | belirgin |  |
| 44 | `acilan_asansor` | Kabus | 1 | %15,30 · 0,39 | %15,23 · 0,53 | belirgin | zamanlı |
| 45 | `yesil_ampul` | Tekinsiz | 1 | %82,99 · 0,13 | %99,75 · 0,20 | belirgin |  |
| 46 | `sonuk_ampul` | Tekinsiz | 2 | %86,77 · 0,16 | %99,75 · 0,16 | belirgin |  |
| 47 | `kirmizi_ampul` | Tekinsiz | 2 | %88,07 · 0,17 | %99,75 · 0,20 | belirgin |  |
| 48 | `karanlik_kat` | Kabus | 1 | %89,09 · 0,23 | %99,75 · 0,24 | belirgin |  |
| 49 | `titreyen_ampul` | Tekinsiz | 2 | %89,09 · 0,23 | %99,75 · 0,24 | belirgin | zamanlı |
| 50 | `goz_kirpan_isik` | Kabus | 2 | %89,09 · 0,23 | %99,75 · 0,24 | belirgin | zamanlı |
| 51 | `kaymis_ampul` | Fisilti | 4 | %89,15 · 0,32 | %96,51 · 0,16 | belirgin |  |

**Özet (efektli):** 6 görünmez, 20 silik, 7 küçük ama net, 18 belirgin.

## Yazı kartları (yakından okunur)

Kapı numarası gibi yazıyı değiştiren kartlar. Oyuncu yazıyı kapıya yaklaşıp okur; iki bakış noktasından ölçülen değer onları olduğundan az görünür gösterir, bu yüzden sınıflandırılmadılar. Değerler yalnızca bilgi için.

- `degisen_numara` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,04
- `bos_plaka` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,10
- `dokuzuncu_kat` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,47
- `geri_sayim` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,53
- `on_uc_numara` (Tekinsiz, zorluk 3) — efektli alan %0,02 · güç 0,04
- `calisan_gosterge` (Fisilti, zorluk 3) — efektli alan %0,02 · güç 0,48
- `ayni_numara` (Kabus, zorluk 1) — efektli alan %0,05 · güç 0,09

Her görsel kartın 'normal | anomali' resmi (en görünür yön ve an, efektler açık): `Builds/AnomaliGorunurluk/<kart>.png` (git dışında; aracı çalıştıran makinede).

## Yalnızca sesle anlaşılan kartlar

Görsel farkları yok; görünürlükleri ses revizyonlarıyla (#14, #25) değerlendirilir.

- `ters_aglama` (Tekinsiz, zorluk 2)
- `sessizlik` (Tekinsiz, zorluk 3)
- `kapi_tiklatiliyor` (Tekinsiz, zorluk 2)
- `duzenli_boru` (Tekinsiz, zorluk 3)
- `asansorde_aglama` (Tekinsiz, zorluk 2)
- `calan_zil` (Kabus, zorluk 1)
- `yanlis_tv` (Fisilti, zorluk 5)
- `susan_tv` (Fisilti, zorluk 4)
