# Anomali görünürlük denetimi (#33)

`Huzur → Anomali → Görünürlük Denetimi` ile üretildi (2026-09-26 18:28). Elle düzenlemeyin; aracı yeniden çalıştırın.

**Ne ölçüldü:** her kart için sahanlık, oyuncunun göz hizasından iki noktadan (kata varış noktası: 4 yön, sahanlığın ortası: 8 yön) normal ve anomalili hâlde çizildi. Zamanlı kartlarda her etkinin gerçekleştiği an ayrı ölçüldü. Kartın değeri en görünür olduğu yön ve andaki değerdir.

- **Alan:** belirgin biçimde (bir renk kanalında 0,03'ten fazla) değişen piksellerin ekrana oranı. Renk değişimleri de sayılır (yeşil ampul gibi).
- **Güç:** değişen piksellerin ortalama farkı (0-1). Küçük ama net bir değişiklik (kapı numarası) alanı küçük, gücü büyüktür.
- **Sınıf** (sınırlar sezgisel başlangıç değerleri; Şahan'ın gözüyle karşılaştırılıp ayarlanacak): görünmez (alan < %0,05) · silik (güç < 0,12) · küçük ama net (alan < %0,3) · belirgin.
- **Gürültü tabanı:** %0,00 (aynı sahne iki kez çizilince ölçülen fark; 0'a yakınsa ölçüm güvenilir).
- ⚠ = tasarım zorluğu 1-2 ("kolay") olduğu hâlde görünmez ya da silik çıkan kart. ↓ = gece efektleri (#31) kartı daha düşük bir sınıfa indirmiş.

## Görsel kartlar (en az görünenden en çok görünene)

| # | Kart | Katman | Zorluk | Efektli alan · güç | Efektsiz alan · güç | Sınıf (efektli) | Not |
|---|---|---|---|---|---|---|---|
| 1 | `eksik_kapi_gozu` | Fisilti | 5 | %0,01 · 0,11 | %0,01 · 0,17 | görünmez |  |
| 2 | `ters_numara` | Fisilti | 4 | %0,02 · 0,09 | %0,02 · 0,14 | görünmez |  |
| 3 | `eksik_kol` | Fisilti | 3 | %0,03 · 0,22 | %0,03 · 0,25 | görünmez |  |
| 4 | `tek_terlik` | Fisilti | 3 | %0,05 · 0,05 | %0,14 · 0,13 | silik | ↓ efektsiz: küçük ama net |
| 5 | `tavandaki_terlik` | Tekinsiz | 4 | %0,05 · 0,05 | %0,14 · 0,13 | silik | ↓ efektsiz: küçük ama net |
| 6 | `donuk_terlik` | Fisilti | 4 | %0,06 · 0,06 | %0,09 · 0,12 | silik | ↓ efektsiz: küçük ama net |
| 7 | `tek_ayakkabi` | Tekinsiz | 3 | %0,07 · 0,05 | %0,09 · 0,16 | silik | ↓ efektsiz: küçük ama net |
| 8 | `ters_terlikler` | Fisilti | 4 | %0,10 · 0,07 | %0,14 · 0,16 | silik | ↓ efektsiz: küçük ama net |
| 9 | `yuruyen_terlik` | Tekinsiz | 3 | %0,11 · 0,07 | %0,15 · 0,14 | silik | ↓ efektsiz: küçük ama net, zamanlı |
| 10 | `iceri_yuruyen_terlikler` | Tekinsiz | 3 | %0,17 · 0,05 | %0,26 · 0,12 | silik | ↓ efektsiz: küçük ama net |
| 11 | `eksik_plaka` | Fisilti | 4 | %0,20 · 0,04 | %0,20 · 0,11 | silik |  |
| 12 | `eksik_saksi` | Fisilti | 3 | %0,22 · 0,07 | %0,93 · 0,07 | silik |  |
| 13 | `devrik_saksi` | Fisilti | 3 | %0,25 · 0,08 | %0,74 · 0,07 | silik |  |
| 14 | `eksik_sayac` | Fisilti | 3 | %0,33 · 0,04 | %0,64 · 0,11 | silik |  |
| 15 | `eksik_paspas` | Fisilti | 3 | %0,38 · 0,05 | %0,69 · 0,08 | silik |  |
| 16 | `yan_sayac` | Fisilti | 3 | %0,60 · 0,05 | %0,09 · 0,12 | silik | ↓ efektsiz: küçük ama net |
| 17 | `kaymis_paspas` | Fisilti | 3 | %0,64 · 0,05 | %1,18 · 0,09 | silik |  |
| 18 | `acilan_sayac` | Tekinsiz | 2 | %0,66 · 0,04 | %0,92 · 0,17 | silik | ⚠ kolay sanılıyor, ↓ efektsiz: belirgin, zamanlı |
| 19 | `buyuk_paspas` | Fisilti | 3 | %0,82 · 0,05 | %1,56 · 0,10 | silik |  |
| 20 | `yamuk_ilan` | Fisilti | 5 | %0,87 · 0,11 | %0,82 · 0,11 | silik |  |
| 21 | `donmus_ayakkabilik` | Fisilti | 3 | %1,37 · 0,05 | %1,85 · 0,10 | silik |  |
| 22 | `kapi_onunde_tup` | Fisilti | 3 | %1,39 · 0,10 | %1,80 · 0,16 | silik | ↓ efektsiz: belirgin |
| 23 | `devrik_ayakkabilik` | Tekinsiz | 2 | %2,28 · 0,07 | %3,72 · 0,11 | silik | ⚠ kolay sanılıyor |
| 24 | `yer_degistiren_ayakkabilik` | Fisilti | 3 | %2,55 · 0,06 | %7,04 · 0,11 | silik |  |
| 25 | `aralik_kapi` | Tekinsiz | 2 | %4,10 · 0,06 | %4,47 · 0,09 | silik | ⚠ kolay sanılıyor |
| 26 | `yuksek_kol` | Fisilti | 4 | %0,05 · 0,15 | %0,06 · 0,21 | küçük ama net |  |
| 27 | `yer_degistiren_zil` | Fisilti | 4 | %0,06 · 0,15 | %0,06 · 0,16 | küçük ama net |  |
| 28 | `kaymis_zil` | Fisilti | 4 | %0,07 · 0,20 | %0,08 · 0,22 | küçük ama net |  |
| 29 | `ters_not` | Fisilti | 4 | %0,09 · 0,20 | %0,09 · 0,29 | küçük ama net |  |
| 30 | `paspasta_gazete` | Fisilti | 4 | %0,10 · 0,14 | %0,10 · 0,24 | küçük ama net |  |
| 31 | `cekilen_gazete` | Tekinsiz | 2 | %0,10 · 0,14 | %0,10 · 0,24 | küçük ama net | zamanlı |
| 32 | `eksik_dugme` | Fisilti | 3 | %0,19 · 0,29 | %0,15 · 0,27 | küçük ama net |  |
| 33 | `acik_sayac` | Tekinsiz | 3 | %0,29 · 0,12 | %0,91 · 0,18 | küçük ama net | ↓ efektsiz: belirgin |
| 34 | `kisa_kanat` | Fisilti | 5 | %0,32 · 0,15 | %0,48 · 0,28 | belirgin |  |
| 35 | `dik_paspas` | Tekinsiz | 2 | %0,48 · 0,12 | %1,83 · 0,12 | belirgin |  |
| 36 | `eksik_not` | Fisilti | 2 | %0,76 · 0,32 | %0,76 · 0,29 | belirgin |  |
| 37 | `eksik_ilan` | Fisilti | 3 | %2,46 · 0,15 | %2,78 · 0,12 | belirgin |  |
| 38 | `dusmus_ilan` | Fisilti | 3 | %2,78 · 0,14 | %4,19 · 0,11 | belirgin |  |
| 39 | `carpan_kapi` | Tekinsiz | 2 | %5,82 · 0,13 | %7,91 · 0,14 | belirgin | zamanlı |
| 40 | `besinci_kapi` | Kabus | 3 | %6,07 · 0,18 | %11,29 · 0,20 | belirgin |  |
| 41 | `aralik_kapilar` | Tekinsiz | 1 | %8,49 · 0,12 | %7,98 · 0,18 | belirgin |  |
| 42 | `aralik_asansor` | Tekinsiz | 2 | %10,55 · 0,19 | %10,95 · 0,43 | belirgin |  |
| 43 | `acik_kapi` | Tekinsiz | 1 | %10,98 · 0,23 | %11,02 · 0,42 | belirgin |  |
| 44 | `acilan_asansor` | Kabus | 1 | %13,68 · 0,24 | %14,87 · 0,47 | belirgin | zamanlı |
| 45 | `yesil_ampul` | Tekinsiz | 1 | %55,53 · 0,13 | %99,74 · 0,16 | belirgin |  |
| 46 | `kirmizi_ampul` | Tekinsiz | 2 | %56,96 · 0,16 | %99,72 · 0,15 | belirgin |  |
| 47 | `sonuk_ampul` | Tekinsiz | 2 | %59,70 · 0,15 | %99,73 · 0,13 | belirgin |  |
| 48 | `karanlik_kat` | Kabus | 1 | %64,08 · 0,20 | %99,74 · 0,20 | belirgin |  |
| 49 | `titreyen_ampul` | Tekinsiz | 2 | %64,08 · 0,20 | %99,74 · 0,20 | belirgin | zamanlı |
| 50 | `goz_kirpan_isik` | Kabus | 2 | %64,08 · 0,20 | %99,74 · 0,20 | belirgin | zamanlı |
| 51 | `kaymis_ampul` | Fisilti | 4 | %77,18 · 0,25 | %93,26 · 0,14 | belirgin |  |

**Özet (efektli):** 3 görünmez, 22 silik, 8 küçük ama net, 18 belirgin.

## Yazı kartları (yakından okunur)

Kapı numarası gibi yazıyı değiştiren kartlar. Oyuncu yazıyı kapıya yaklaşıp okur; iki bakış noktasından ölçülen değer onları olduğundan az görünür gösterir, bu yüzden sınıflandırılmadılar. Değerler yalnızca bilgi için.

- `degisen_numara` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,08
- `bos_plaka` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,11
- `geri_sayim` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,51
- `dokuzuncu_kat` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,45
- `calisan_gosterge` (Fisilti, zorluk 3) — efektli alan %0,02 · güç 0,45
- `on_uc_numara` (Tekinsiz, zorluk 3) — efektli alan %0,03 · güç 0,09
- `ayni_numara` (Kabus, zorluk 1) — efektli alan %0,05 · güç 0,12

Her görsel kartın 'normal | anomali' resmi (en görünür yön ve an, efektler açık): `Builds/AnomaliGorunurluk/<kart>.png` (git dışında; aracı çalıştıran makinede).

## Yalnızca sesle anlaşılan kartlar

Görsel farkları yok; görünürlükleri ses revizyonlarıyla (#14, #25) değerlendirilir.

- `susan_tv` (Fisilti, zorluk 4)
- `ters_aglama` (Tekinsiz, zorluk 2)
- `yanlis_tv` (Fisilti, zorluk 5)
- `calan_zil` (Kabus, zorluk 1)
- `asansorde_aglama` (Tekinsiz, zorluk 2)
- `duzenli_boru` (Tekinsiz, zorluk 3)
- `sessizlik` (Tekinsiz, zorluk 3)
- `kapi_tiklatiliyor` (Tekinsiz, zorluk 2)
