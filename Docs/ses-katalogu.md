# Ses kataloğu (#28)

Oyundaki her ses, numarasıyla. Revizyon isterken numarayla söylemek yeterli:
"S03 daha seyrek ağlasın", "S06 çok yüksek", "S15 daha tok olsun".

Her satırın sonundaki **Revizyon notu** sütunu, Şahan'ın isteğinin yazılacağı yer.
İstek yapılınca `✓` ve commit hash'i eklenir.

**Genel bilgi**
- **Sesler iki yoldan üretiliyor** (2026-09-26 revizyonundan beri):
  - **Formülle:** uğultu, boru, rüzgâr, adım, lamba, poşet, sabah, damla, zil, asansör.
    Betikler `Art/Ses/scripts/` içinde (`ortam_uret.py`, `oyuncu_sesleri_uret.py`,
    `zil_uret.py`, `asansor_ding_uret.py`); ortak aletler `ses_ortak.py` (gürültü darbesi →
    cismin modları → beton merdiven yankısı).
  - **Gerçek kayıttan:** TV, bebek (üç hâli), güvercin, kapı çarpması, kapı tıklatma.
    Kayıtların asılları `Art/Ses/kayitlar/`, lisansları `Art/Ses/kayitlar/LISANSLAR.md`;
    oyundaki hâlleri `kayit_isle.py` üretir. Canlı/organik sesler formülle "uzaylı gibi"
    duyuldu, bu yüzden kayda geçildi.
- Bir sesin **karakteri** (perde, boğukluk, ritim, uzunluk, kesilen parça) betikte değişir,
  betik o sesin adıyla yeniden çalıştırılır (`python ortam_uret.py ugultu`). Bir sesin
  **seviyesi, duyulma mesafesi ve yeri** ise sahnede ya da oyun kodunda değişir.
- Yeni bir gerçek kayıt eklenmeden önce kaynak, lisans ve boyut Şahan'a sorulur.
- **3B** ses, dünyada bir noktadan gelir: uzaklaştıkça kısılır, sağdan-soldan duyulur.
  **2B** ses "kafanın içinden" gelir, yeri yoktur.
- **Seviye** 0-1 arası (1 = dosyanın tam gücü). #27'deki ses ayarı hepsini birlikte ölçekler.
- **Duyulma mesafesi** (min–maks, metre): min'e kadar tam güçte, maks'ta sessiz.

---

## A. Ortam katmanları — her katta, sürekli (#14; bebek #35 ile E'ye geçti)

Normal bir katın ses dokusu. Hepsi döngüde çalar, 3B'dir ve **saate kilitlidir**: iki
sahanlıktaki kopyalar aynı anı çalar, ışınlanmada ses atlamaz. Açılıştaki zemin katta
da aynı dört katman (S01-S04) vardır. Anomali kartları bu katmanları susturarak,
taşıyarak ya da yerine bozuk bir ses koyarak "bozar" (D bölümü).

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Bozan kartlar | Revizyon notu |
|---|---|---|---|---|---|---|
| **S01** | **Bina uğultusu.** Gece binasının "sessizliği": çok kısık, alçak bir hava sesi (şebeke vızıltısının yalnızca izi). Fark edilmez; kesilince fark edilir. | Sahanlığın ortası (`Ses_Ugultu`) | `Assets/Audio/Ortam/Ortam_Ugultu.wav` · `ortam_uret.py` → `ugultu()` | 0,25 · 4–9 m · 30 sn | `karanlik_kat`, `sessizlik` (susturur; E bölümündeki uzak olaylar da onunla susar) | "Daha düşük, ufak yankılar olsun" → vızıltı çıktı, uzak yankılı olaylar eklendi. ✓ 2. turda onay. Build testi: "sürekli olması rahatsız ediyor" → uzak olaylar ayrı kat olaylarına çıktı (E, #35) |
| **S02** | **Duvar arkasından TV.** Yan odadan kaydedilmiş gerçek bir TV: boğuk konuşmalar (kelimeler seçilmez), arada sessiz anlar. | 2 numaralı dairenin içi (`Kapi_2/Ses_TV`) | `Assets/Audio/Ortam/Ortam_TV.wav` · kayıt (Pixabay) · `kayit_isle.py` → `tv()` | 0,50 · 1,5–9 m · 85,5 sn | `susan_tv`, `sessizlik` (susturur); `yanlis_tv` (9 m sağa, karşı daireye taşır); `calan_zil` (2,5 sn sonra susar); `besinci_kapi` (var olmayan 5. kapıdan da TV) | "TV ile alakası yok" → gerçek kayıt. ✓ 4. turda onay |
| **S03** | **Ağlayan bebek.** Gerçek bir bebeğin 9,4 sn'lik ağlama nöbeti, duvar arkasından (boğuk). **Artık döngü değil, bir kat olayı** (E bölümü): yalnızca planın bebeği seçtiği katlarda bir iki kez çalar. | 4 numaralı dairenin içi (`Kapi_4/Ses_Bebek`) | `Assets/Audio/Ortam/Ortam_Bebek.wav` · kayıt (Wikimedia) · `kayit_isle.py` → `bebek()` | 0,55 · 1,5–10 m · 10,4 sn, tek seferlik | `sessizlik` (susturur); `ters_aglama` (S12 ile değiştirir); `asansorde_aglama` (S13 ile değiştirir) | "Ağlayan uzaylı gibi" → gerçek kayıt. ✓ 4. turda onay. Build testi: her katta ağlaması rahatsız etti → kat olayı (#35) |
| **S04** | **Boru tıkırtıları.** Duvarın içinde düzensiz aralıklarla boğuk metalik vuruş ve uzun boru-beton yankısı. | Sahanlık duvarının içi (`Ses_Boru`) | `Assets/Audio/Ortam/Ortam_Boru.wav` · `ortam_uret.py` → `boru()` | 0,50 · 1–8 m · 16 sn | `sessizlik` (susturur); `duzenli_boru` (S14 ile değiştirir) | "Daha yankılı, net gelmesin" → boğuk + uzun yankı. ✓ 2. turda onay |
| **S05** | **Rüzgâr.** Merdiven boşluğunun penceresinden gelen hafif rüzgâr. | Merdiven çekirdeği (`Cekirdek_A/Ses_Ruzgar`) | `Assets/Audio/Ortam/Ortam_Ruzgar.wav` · `ortam_uret.py` → `ruzgar()` | 0,30 · 1–6 m · 12 sn | — | ✓ 1. turda onay (2026-09-26) |

## B. Oyuncu ve olay sesleri (#25)

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S06** | **Ayak sesleri.** Beton üstünde topuğun tok değişi + ayağın önü + taban sürtünmesi + kum-toz cızırtısı, kısa merdiven yankısı; dört çeşit, her adımda perde biraz değişir (makine gibi tekrar etmesin). | Her 0,6 m yürüyüşte bir; 2B ("kendi ayağın") | `Assets/Resources/Sesler/Adim_1..4.wav` · `oyuncu_sesleri_uret.py` → `adimlar()` · çalan kod `AyakSesi.cs` | 0,35 × (0,85–1,0), perde 0,92–1,08 · — · 0,4 sn | "Demir kova giymiş gibi" → sinüs çınlaması kaldırıldı (gürültü darbeleri); 2. tur "biraz daha gelişebilir" → kum-toz eklendi. ✓ 3. turda onay (gerçek kayıt adayları beğenilmedi) |
| **S07** | **Lamba sönme "tık"ı.** Eski bir duvar anahtarının küçük, kuru çift tıkı. | Ceza anında her lamba sönerken, lambanın yerinden (3B) | `Assets/Resources/Sesler/Lamba_Sonme.wav` · `oyuncu_sesleri_uret.py` → `lamba_sonme()` · çalan kod `KatYoneticisi.cs` | 0,9 · — · 0,4 sn | 1. tur "daha tok, lamba anahtarı gibi"; 2. tur "çok yüksek, bir yere vuruyormuş gibi" → alçak gövde çıktı, kısıldı. ✓ 3. turda onay |
| **S08** | **Son lambanın cızırtısı.** Düzensiz kıvılcım çıtırtıları. | Ceza anında son lamba titrerken (3B) | `Assets/Resources/Sesler/Lamba_Cizirti.wav` · `oyuncu_sesleri_uret.py` → `lamba_cizirti()` · çalan kod `KatYoneticisi.cs` | 0,8 · — · 0,9 sn | "Arkadaki fon olmasın" → 50 Hz vızıltı çıkarıldı. ✓ 2. turda onay |
| **S09** | **Poşet hışırtısı.** Kâğıt poşetin kırışması ve elde kayan kâğıdın hışırtısı. | Çöp poşeti toplanınca, poşetin yerinden (3B) | `Assets/Resources/Sesler/Poset_Hisirti.wav` · `oyuncu_sesleri_uret.py` → `poset()` · çalan kod `CopPoseti.cs` | 0,8 · — · 0,7 sn | "Elektrik hışırtısı gibi, kâğıt poşet olsun" → kâğıt. ✓ 2. turda onay |

## C. Final sesleri (#16, #25)

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S10** | **Sabah.** Uzak şehir uğultusu, hafif rüzgâr ve kuş cıvıltıları. | Çatı kapısı açılınca çatıdan içeri dolar (3B); dış planda aynı ses 2B | `Assets/Audio/Ortam/Ortam_Sabah.wav` · `oyuncu_sesleri_uret.py` → `sabah()` | çatıda 0,60 · 3–20 m; dış planda 0,70 · 20 sn | ✓ 1. turda onay (2026-09-26) |
| **S11** | **Güvercinler.** Gerçek güvercinlerin kanat çırpışı: havalanış, çırpınma, ikinci bir havalanış. | Çatı kapısı açıldığı an, çatının ortasından (3B) | `Assets/Resources/Sesler/Guvercin_Kanat.wav` · kayıt (Freesound) · `kayit_isle.py` → `guvercin()` · çalan kod `FinalSahnesi.cs` | 1,0 · — · 3,8 sn | "Güvercinle alakası yok" → gerçek kayıt. ✓ 4. turda onay |
| — | *Sabah ezanı — henüz yok.* Gerçek, lisansı temiz bir kayıt bekleniyor (#16). | Çatıda, uzaktan, kısık | — | — | |

## D. Anomali sesleri — yalnızca anomalili katlarda (#13, #14)

Kart hangi katta gelirse o katta çalar. "Gecikme", oyuncunun kata varışından sonra geçen
süre.

| No | Ses | Hangi kart · nerede · ne zaman | Dosya · betik | Seviye · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S12** | **Tersten ağlama.** S03'teki gerçek nöbetin tersten çalınmışı (boğuk). | `ters_aglama` (TEKİNSİZ, zorluk 2): normal bebek susar, bu 4 numaranın içinden çalar (döngü) | `Assets/Audio/Anomali/Bebek_Ters.wav` · `kayit_isle.py` → `bebek()` | 0,55 · 14 sn | "Bebekle alakası yok" → gerçek kayıt. ✓ 4. turda onay |
| **S13** | **Yakından ağlama.** Aynı nöbet ama boğuk değil: duvar arkasından değil, çok yakından. | `asansorde_aglama` (TEKİNSİZ, 2): normal bebek susar, bu asansör kuyusundan çalar (döngü) | `Assets/Audio/Anomali/Bebek_Yakin.wav` · `kayit_isle.py` → `bebek()` (kuyu yankısı) | 0,60 · 14 sn | "Bebekle alakası yok" → gerçek kayıt. ✓ 4. turda onay |
| **S14** | **Düzenli damla.** Sızdıran borudan makine gibi düzenli su damlası: hep aynı damla, tam 1,6 sn arayla. | `duzenli_boru` (TEKİNSİZ, 3): normal boru susar, bu sol arka duvardan çalar (döngü) | `Assets/Audio/Anomali/Boru_Damla.wav` · `ortam_uret.py` → `duzenli_damla()` | 0,50 · 16 sn | 1. tur "su damlası olsun" (dosya Boru_Ritim → Boru_Damla, GUID korundu); 2. tur "az yankı, daha seyrek" → 0,8 → 1,6 sn. ✓ 3. turda onay |
| **S15** | **Kapı çarpması.** Gerçek bir tahta apartman kapısının çarpması + merdiven boşluğu yankısı. | `carpan_kapi` (TEKİNSİZ, 2): 2 numaranın kapısı, 4 sn sonra | `Assets/Audio/Anomali/Kapi_Carpma.wav` · kayıt (Freesound) · `kayit_isle.py` → `kapi_carpma()` | 1,0 · 2,5 sn | 1. tur "kapı çarpmasına benzemiyor", 2. tur (formül) "tahta kapı sesi eklersen düzelir" → gerçek kayıt. ✓ 4. turda onay |
| **S16** | **Kapı tıklatma.** Kalın tahta kapıya gerçek üç vuruş, dairenin içinden (boğuk) + kısa yankı. | `kapi_tiklatiliyor` (TEKİNSİZ, 2): 1 numaranın içinden, 3 sn ve 8 sn sonra | `Assets/Audio/Anomali/Kapi_Tiklatma.wav` · kayıt (Pixabay/FrostyFrost) · `kayit_isle.py` → `kapi_tiklatma()` | 1,0 · 1,8 sn | 1. tur "daha tok", 2. tur (formül) "tahta kapıya tıklama sesi eklersen düzelir" → gerçek kayıt. ✓ 4. turda onay |
| **S17** | **Kapı zili (ding-dong).** | `calan_zil` (KÂBUS): 3 numaranın zili kendi kendine, 2,5 sn ve 8 sn sonra; ilk çalışta TV (S02) susar | `Assets/Audio/Anomali/Zil_DingDong.wav` · `zil_uret.py` | 1,0 · 2,6 sn | ✓ 1. turda onay (2026-09-26) |
| **S18** | **Asansör "ding"i.** Arızalı asansör varmış gibi. | `acilan_asansor` (KÂBUS): asansör göstergesinden, 3 sn sonra | `Assets/Audio/Anomali/Asansor_Ding.wav` · `asansor_ding_uret.py` | 1,0 · 2,4 sn | ✓ 1. turda onay (2026-09-26) |
| **S19** | **Asansör kuyusu uğultusu.** Kuyudan yükselen derin uğultu. | `acilan_asansor` (KÂBUS): kuyudan, 3,6 sn sonra başlar (döngü) | `Assets/Audio/Anomali/Asansor_Ugultu.wav` · `ortam_uret.py` → `asansor_ugultu()` | 0,70 · 8 sn | ✓ 1. turda onay (2026-09-26) |

**Yalnızca sesi susturan / taşıyan kartlar** (yeni ses çalmaz, ortamı bozar):
`susan_tv` (S02 susar) · `yanlis_tv` (S02 karşı daireden gelir) · `sessizlik` (S01-S04
hepsi susar) · `karanlik_kat` (S01 susar, ışıklar da söner) · `besinci_kapi` (S02
var olmayan 5. kapının arkasından da çalar, seviye 0,45).

## E. Kat olayları — her katta yalnızca biri (#35)

Şahan build'de dinledikten sonra (2026-09-26): "bazı seslerin sürekli olması rahatsızlık
veriyor; her katta damlayan su sesi, bebek ağlaması, kapı çarpması olmasın ama her katta
bunlardan biri olsun". Her kata bu beş olaydan **yalnızca biri** düşer (`KatOlayPlani.cs`):
tur başında çekilir, tur boyunca sabit, cezadan sonra yeniden çekilir; komşu iki kata aynı
olay düşmez. Olay, kata varıştan 4-9 sn sonra bir kez, ondan 16-24 sn sonra bir kez daha
çalar (`KatOlayCalici.cs`, saatini `KatYoneticisi` ilerletir; yalnızca oyuncunun bulunduğu
katta). **Kendi sesi olan bir anomali kartı gelen katta olay çalmaz**: o katın sesi
anomalinin sesidir. Uzak olaylar uğultunun çocuğu olduğu için `sessizlik` ve `karanlik_kat`
onları da susturur. Test göstergesi o katın olayını yazar ("olay: Bebek").

| No | Olay | Nerede | Dosya · betik | Seviye · mesafe · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S03** | Bebek ağlaması | 4 numaranın içi (`Kapi_4/Ses_Bebek`) | yukarıda, A bölümü | 0,55 · 1,5–10 m · 10,4 sn | |
| **S20** | Uzakta kapanan kapı: alçak, boğuk bir güm ve uzun yankısı | Bir kat yukarıda (`Ses_Ugultu/Olay_UzakKapi`) | `Assets/Audio/Olay/Olay_UzakKapi.wav` · `ortam_uret.py` → `kat_olaylari()` | 0,20 · 2–14 m · 3,4 sn | eskiden uğultunun içindeydi (S01) |
| **S21** | Üst katta dört yavaş adım | Tavanın üstü (`Ses_Ugultu/Olay_UstKatAdimlari`) | `Assets/Audio/Olay/Olay_UstKatAdimlari.wav` · `kat_olaylari()` | 0,20 · 1,5–10 m · 5,4 sn | eskiden uğultunun içindeydi |
| **S22** | Kalorifer borusunun metalik tıkırtısı | Sol duvarın dibi (`Ses_Ugultu/Olay_Kalorifer`) | `Assets/Audio/Olay/Olay_Kalorifer.wav` · `kat_olaylari()` | 0,12 · 1–8 m · 3,3 sn | eskiden uğultunun içindeydi; "su damlası gibi" duyulan buydu |
| **S23** | Dışarıdan geçen araba | Sokak (`Ses_Ugultu/Olay_Araba`) | `Assets/Audio/Olay/Olay_Araba.wav` · `kat_olaylari()` | 0,15 · 4–18 m · 7,0 sn | eskiden uğultunun içindeydi |

## F. Müzik ve dış sesler (#36, #37)

| No | Ses | Nasıl çalar | Dosya · betik | Seviye · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S24** | **Gerilim müziği.** Joth'un 5 karanlık ambiyans parçası (OpenGameArt, CC0) 3 sn'lik geçişlerle tek döngü; seviyesi yavaş bir dengeleyiciyle düzleştirildi | 2B, oyun boyunca döngüde (başlık ekranı dahil); ceza karanlığında durur, çatı kapısı açılınca 4 sn'de susar (`ArkaMuzik.cs`, sahnede `Arka_Muzik`) | `Assets/Audio/Muzik/Muzik_Gerilim.wav` · `kayit_isle.py` → `muzik()` | 0,15 · 268,5 sn, stereo | Şahan adaylar arasından seçti (Joth), "beşi sırayla" |
| **S25** | **Uzak silah sesi.** Tek atış ve 1,1 sn arayla iki atış; boğuk, şehir yankılı | Dış katman (`DisSesleri.cs`, `Dis_Sesler`): 2B, 40-90 sn'de bir, türler art arda tekrar etmez; kat olayı, anomali sesi ya da ceza sürerken araya girmez, kat olayları da onu bekler | `Assets/Audio/Dis/Dis_Silah_1/2.wav` · `kayit_isle.py` → `dis_silah()` (BigSoundBank #2854) | 0,5 · 4,1 / 5,2 sn | Şahan seçti |
| **S26** | **Pencereden içeri süzülen cılız rüzgâr.** Üç esinti | Dış katman, aynı kurallar | `Assets/Audio/Dis/Dis_Ruzgar_1..3.wav` · `dis_ruzgar()` (BigSoundBank #0147) | 0,5 · 13-14 sn | Şahan: "dışarıda esen değil, bina içine girecek cılız rüzgâr" |
| **S27** | **Uzaktan köpek havlaması** | Dış katman, aynı kurallar | *Kayıt bekleniyor* (spirit.zip, Freesound; Şahan indirecek) | — | |

---

*Liste, 2026-09-25'te sahnedeki ses kaynakları ve bütün anomali kartları taranarak
çıkarıldı: 22 ses dosyası (S06 dört dosya), 11 sahne ses kaynağı, sesle ilgili 12 anomali kartı.
2026-09-26: Şahan 19 sesin hepsini dört turluk dinleme testinde onayladı (#14, #25).*
