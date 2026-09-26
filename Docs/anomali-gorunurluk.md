# Anomali görünürlük denetimi (#33)

`Huzur → Anomali → Görünürlük Denetimi` ile üretildi (2026-09-26 13:49). Elle düzenlemeyin; aracı yeniden çalıştırın.

**Ne ölçüldü:** her kart için sahanlık, oyuncunun göz hizasından iki noktadan (kata varış noktası: 4 yön, sahanlığın ortası: 8 yön) normal ve anomalili hâlde çizildi. Zamanlı kartlarda her etkinin gerçekleştiği an ayrı ölçüldü. Kartın değeri en görünür olduğu yön ve andaki değerdir.

- **Alan:** belirgin biçimde (bir renk kanalında 0,03'ten fazla) değişen piksellerin ekrana oranı. Renk değişimleri de sayılır (yeşil ampul gibi).
- **Güç:** değişen piksellerin ortalama farkı (0-1). Küçük ama net bir değişiklik (kapı numarası) alanı küçük, gücü büyüktür.
- **Sınıf** (sınırlar sezgisel başlangıç değerleri; Şahan'ın gözüyle karşılaştırılıp ayarlanacak): görünmez (alan < %0,05) · silik (güç < 0,12) · küçük ama net (alan < %0,3) · belirgin.
- **Gürültü tabanı:** %0,00 (aynı sahne iki kez çizilince ölçülen fark; 0'a yakınsa ölçüm güvenilir).
- ⚠ = tasarım zorluğu 1-2 ("kolay") olduğu hâlde görünmez ya da silik çıkan kart. ↓ = gece efektleri (#31) kartı daha düşük bir sınıfa indirmiş.

## Görsel kartlar (en az görünenden en çok görünene)

| # | Kart | Katman | Zorluk | Efektli alan · güç | Efektsiz alan · güç | Sınıf (efektli) | Not |
|---|---|---|---|---|---|---|---|
| 1 | `eksik_kapi_gozu` | Fisilti | 5 | %0,00 · 0,08 | %0,00 · 0,16 | görünmez |  |
| 2 | `eksik_kol` | Fisilti | 3 | %0,01 · 0,09 | %0,01 · 0,14 | görünmez |  |
| 3 | `yer_degistiren_zil` | Fisilti | 4 | %0,01 · 0,07 | %0,01 · 0,17 | görünmez |  |
| 4 | `kaymis_zil` | Fisilti | 4 | %0,01 · 0,10 | %0,01 · 0,24 | görünmez |  |
| 5 | `eksik_plaka` | Fisilti | 2 | %0,01 · 0,06 | %0,03 · 0,12 | görünmez | ⚠ kolay sanılıyor |
| 6 | `ters_numara` | Fisilti | 4 | %0,01 · 0,05 | %0,01 · 0,07 | görünmez |  |
| 7 | `yuksek_kol` | Fisilti | 4 | %0,03 · 0,07 | %0,06 · 0,10 | görünmez | ↓ efektsiz: silik |
| 8 | `tavandaki_terlik` | Tekinsiz | 2 | %0,05 · 0,05 | %0,14 · 0,13 | silik | ⚠ kolay sanılıyor, ↓ efektsiz: küçük ama net |
| 9 | `tek_terlik` | Fisilti | 3 | %0,05 · 0,05 | %0,14 · 0,13 | silik | ↓ efektsiz: küçük ama net |
| 10 | `donuk_terlik` | Fisilti | 4 | %0,06 · 0,06 | %0,09 · 0,12 | silik | ↓ efektsiz: küçük ama net |
| 11 | `tek_ayakkabi` | Tekinsiz | 3 | %0,07 · 0,05 | %0,09 · 0,16 | silik | ↓ efektsiz: küçük ama net |
| 12 | `ters_terlikler` | Fisilti | 4 | %0,10 · 0,07 | %0,14 · 0,16 | silik | ↓ efektsiz: küçük ama net |
| 13 | `yuruyen_terlik` | Tekinsiz | 3 | %0,13 · 0,05 | %0,16 · 0,14 | silik | ↓ efektsiz: küçük ama net, zamanlı |
| 14 | `iceri_yuruyen_terlikler` | Tekinsiz | 3 | %0,17 · 0,05 | %0,12 · 0,13 | silik | ↓ efektsiz: küçük ama net |
| 15 | `eksik_sayac` | Fisilti | 3 | %0,23 · 0,08 | %0,67 · 0,09 | silik |  |
| 16 | `devrik_saksi` | Fisilti | 3 | %0,28 · 0,09 | %0,80 · 0,08 | silik |  |
| 17 | `kisa_kanat` | Fisilti | 5 | %0,28 · 0,05 | %0,47 · 0,14 | silik | ↓ efektsiz: belirgin |
| 18 | `eksik_paspas` | Fisilti | 2 | %0,29 · 0,04 | %0,58 · 0,09 | silik | ⚠ kolay sanılıyor |
| 19 | `eksik_saksi` | Fisilti | 3 | %0,34 · 0,06 | %0,96 · 0,08 | silik |  |
| 20 | `besinci_kapi` | Kabus | 1 | %0,40 · 0,06 | %0,70 · 0,07 | silik | ⚠ kolay sanılıyor |
| 21 | `yan_sayac` | Fisilti | 3 | %0,47 · 0,04 | %1,19 · 0,09 | silik |  |
| 22 | `acilan_sayac` | Tekinsiz | 2 | %0,58 · 0,04 | %0,92 · 0,15 | silik | ⚠ kolay sanılıyor, ↓ efektsiz: belirgin, zamanlı |
| 23 | `kaymis_paspas` | Fisilti | 3 | %0,61 · 0,04 | %1,14 · 0,09 | silik |  |
| 24 | `acik_sayac` | Tekinsiz | 3 | %0,70 · 0,04 | %0,91 · 0,16 | silik | ↓ efektsiz: belirgin |
| 25 | `buyuk_paspas` | Fisilti | 3 | %0,92 · 0,04 | %1,57 · 0,10 | silik |  |
| 26 | `yamuk_ilan` | Fisilti | 5 | %0,93 · 0,10 | %0,84 · 0,11 | silik |  |
| 27 | `dik_paspas` | Tekinsiz | 2 | %1,17 · 0,05 | %1,79 · 0,08 | silik | ⚠ kolay sanılıyor |
| 28 | `kapi_onunde_tup` | Fisilti | 3 | %1,19 · 0,07 | %1,64 · 0,14 | silik | ↓ efektsiz: belirgin |
| 29 | `donmus_ayakkabilik` | Fisilti | 3 | %1,41 · 0,06 | %1,89 · 0,10 | silik |  |
| 30 | `devrik_ayakkabilik` | Tekinsiz | 2 | %2,48 · 0,06 | %3,81 · 0,11 | silik | ⚠ kolay sanılıyor |
| 31 | `yer_degistiren_ayakkabilik` | Fisilti | 3 | %2,52 · 0,05 | %7,40 · 0,10 | silik |  |
| 32 | `aralik_kapi` | Tekinsiz | 2 | %2,89 · 0,05 | %4,33 · 0,06 | silik | ⚠ kolay sanılıyor |
| 33 | `carpan_kapi` | Tekinsiz | 2 | %6,02 · 0,06 | %6,85 · 0,14 | silik | ⚠ kolay sanılıyor, ↓ efektsiz: belirgin, zamanlı |
| 34 | `aralik_kapilar` | Tekinsiz | 1 | %9,21 · 0,07 | %10,12 · 0,10 | silik | ⚠ kolay sanılıyor |
| 35 | `ters_not` | Fisilti | 4 | %0,09 · 0,20 | %0,09 · 0,29 | küçük ama net |  |
| 36 | `paspasta_gazete` | Fisilti | 4 | %0,10 · 0,14 | %0,10 · 0,24 | küçük ama net |  |
| 37 | `cekilen_gazete` | Tekinsiz | 2 | %0,10 · 0,14 | %0,10 · 0,24 | küçük ama net | zamanlı |
| 38 | `eksik_dugme` | Fisilti | 3 | %0,23 · 0,21 | %0,15 · 0,24 | küçük ama net |  |
| 39 | `eksik_not` | Fisilti | 2 | %0,76 · 0,32 | %0,76 · 0,29 | belirgin |  |
| 40 | `eksik_ilan` | Fisilti | 3 | %2,68 · 0,12 | %2,86 · 0,11 | belirgin |  |
| 41 | `dusmus_ilan` | Fisilti | 3 | %2,76 · 0,12 | %4,41 · 0,10 | belirgin |  |
| 42 | `aralik_asansor` | Tekinsiz | 2 | %10,55 · 0,19 | %10,95 · 0,43 | belirgin |  |
| 43 | `acik_kapi` | Tekinsiz | 1 | %10,62 · 0,13 | %11,00 · 0,30 | belirgin |  |
| 44 | `acilan_asansor` | Kabus | 1 | %13,70 · 0,24 | %14,88 · 0,47 | belirgin | zamanlı |
| 45 | `yesil_ampul` | Tekinsiz | 1 | %61,59 · 0,13 | %99,75 · 0,17 | belirgin |  |
| 46 | `sonuk_ampul` | Tekinsiz | 2 | %67,13 · 0,15 | %99,75 · 0,14 | belirgin |  |
| 47 | `kirmizi_ampul` | Tekinsiz | 2 | %69,11 · 0,15 | %99,75 · 0,17 | belirgin |  |
| 48 | `goz_kirpan_isik` | Kabus | 2 | %70,91 · 0,20 | %99,75 · 0,21 | belirgin | zamanlı |
| 49 | `titreyen_ampul` | Tekinsiz | 2 | %70,91 · 0,20 | %99,75 · 0,21 | belirgin | zamanlı |
| 50 | `karanlik_kat` | Kabus | 1 | %70,91 · 0,20 | %99,75 · 0,21 | belirgin |  |
| 51 | `kaymis_ampul` | Fisilti | 4 | %79,50 · 0,26 | %96,22 · 0,14 | belirgin |  |

**Özet (efektli):** 7 görünmez, 27 silik, 4 küçük ama net, 13 belirgin.

## Yazı kartları (yakından okunur)

Kapı numarası gibi yazıyı değiştiren kartlar. Oyuncu yazıyı kapıya yaklaşıp okur; iki bakış noktasından ölçülen değer onları olduğundan az görünür gösterir, bu yüzden sınıflandırılmadılar. Değerler yalnızca bilgi için.

- `degisen_numara` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,04
- `bos_plaka` (Fisilti, zorluk 3) — efektli alan %0,01 · güç 0,05
- `geri_sayim` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,51
- `dokuzuncu_kat` (Tekinsiz, zorluk 2) — efektli alan %0,02 · güç 0,45
- `on_uc_numara` (Tekinsiz, zorluk 3) — efektli alan %0,02 · güç 0,05
- `calisan_gosterge` (Fisilti, zorluk 3) — efektli alan %0,02 · güç 0,45
- `ayni_numara` (Kabus, zorluk 1) — efektli alan %0,04 · güç 0,05

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
