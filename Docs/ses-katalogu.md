# Ses kataloğu (#28, #74)

Oyundaki bütün sesler, numarasıyla (2026-09-27, #74 için baştan sayıldı; eski S01-S27 numaraları geçersiz).
Şahan her sesi dinleme sayfasında işaretledi (2026-09-27): 13 kalsın, 14 kaldırıldı, 22 değişecek. Kaldırılanlar oyundan ve projeden silindi (#74). Değişecekler için iki turda aday dinlendi; seçilen kayıtlar Karar sütununda "Yeni" olarak yazılı (`3716d48`), kaynakları `Art/Ses/kayitlar/LISANSLAR.md`'de.
İşaretleme yerel dinleme sayfasında yapılır (sesler orada oyundaki seviyede çalar); sonuçlar buraya işlenir.

**Genel bilgi**
- **3B** ses dünyada bir noktadan gelir (uzaklaştıkça kısılır, yönü duyulur); **2B** ses "kafanın içinden" gelir.
- **Seviye** 0-1 arası (1 = dosyanın tam gücü); ayarlardaki ses düzeyi hepsini birlikte ölçekler.
- **Sentez** sesler `Art/Ses/scripts/` içindeki betiklerle üretilir; **kayıtlar** `Art/Ses/kayitlar/` içinde değiştirilmeden durur, lisansları `LISANSLAR.md`'de.
- Eski kuralın (geri dön / ceza) ve anomali kartlarının sesleri yeni ilerlemede çalmıyor (`KatYoneticisi.eskiKural` kapalı).
- Müzik: `ArkaMuzik` tabanı 0,28 (#143; sahnedeki kaynak değeri okunmaz), binanın gerilimiyle en çok ×1,6; kaçış ve Gölge anlarında 0,65'e kabarır; çatıda susar.
- Dış sesler: `DisSesleri` ilk sesi 20-40 sn sonra, sonra 40-90 sn'de bir çalar; aynı tür art arda gelmez; kat olayıyla çakışmaz.

---

## 1. Müzik

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S01 | `Audio/Muzik/Muzik_Gerilim.wav` · 268,5 sn | Arka plan gerilim müziği. Başlık ekranı dahil bütün oyun boyunca döner (2B, taban 0,28, gerilimle ×1-1,6; #143). Kaçış başlayınca ve Gölge belirince 0,65'e kabarır, 3 sn tutar, ~5 sn'de iner. Çatı kapısı açılınca 4 sn'de susar. | Kayıt: Joth, "Ambience Pack 1 – Sci Fi Horror" (OpenGameArt, CC0), beş parça 3 sn geçişle birleştirildi | **Değişsin** → **Yeni:** Scott Elliott, "Death is Looming" (OpenGameArt, CC-BY 4.0; jenerikte), dikişsiz döngü |

---

## 2. Sürekli ortam katmanları (her katta, döngü)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S02 | `Audio/Ortam/Ortam_Ugultu.wav` · 30 sn | Binanın çok kısık hava uğultusu, sahanlığın ortasından (3B, 0,25, 4-9 m). Kat olayları bunun çocuğu. | Sentez: ortam_uret.py → ugultu() | Kalsın |
| S03 | `Audio/Ortam/Ortam_TV.wav` · 85,5 sn | 2 numaranın duvarının ardında boğuk televizyon (3B, 0,5, 1,5-9 m). Her katta kaydın başka bir yeri çalar. | Kayıt: Pixabay, arunangshubanerjee "tv playing in the next room" | **Kaldırıldı (#143):** Şahan 13 Ekim testinde her kattaki sürekli TV sesini gereksiz gürültü buldu; dosya depodan silindi, kayıt `Art/Ses/kayitlar/`da duruyor. |
| S04 | `Audio/Ortam/Ortam_Boru.wav` · 16 sn | Sahanlık duvarının içinde düzensiz boru tıkırtıları (3B, 0,5, 1-8 m). | Sentez: ortam_uret.py → boru() | **Kaldırıldı** (#74) |
| S05 | `Audio/Ortam/Ortam_Ruzgar.wav` · 12 sn | Merdiven penceresinde hafif rüzgâr (3B, 0,3, 1-6 m). | Sentez: ortam_uret.py → ruzgar() | **Kaldırıldı** (#74) |

---

## 3. Kat olayları ve katın kendi sesleri

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S06 | `Audio/Ortam/Ortam_Bebek.wav` · 10,4 sn | Kat olayı "bebek": 4 numaranın duvarının ardında bir nöbet bebek ağlaması. Kata varıştan 4-9 sn sonra ve 16-24 sn sonra birer kez (3B, 0,55). | Kayıt: Wikimedia "Crying_newborn_baby.ogg" (the_yura, CC0), boğuklaştırıldı | Kalsın |
| S07 | `Audio/Olay/Olay_UzakKapi.wav` · 3,4 sn | Kat olayı: bir üst katta uzakta bir kapı kapanır (3B, 0,2). | Sentez: ortam_uret.py → kat_olaylari() | **Kaldırıldı** (#74) |
| S08 | `Audio/Olay/Olay_UstKatAdimlari.wav` · 5,4 sn | Kat olayı: tavanın üstünde dört ağır adım (3B, 0,2). | Sentez: ortam_uret.py → kat_olaylari() | **Yeni görev (#74):** Gezen Gölge'nin ve 7. kattaki kaçışın habercisi; Gölge sesin ortasında belirir. Kat olayı olmaktan çıktı. |
| S09 | `Audio/Olay/Olay_Kalorifer.wav` · 3,3 sn | Kat olayı: sol duvarın dibinde kalorifer borusu tıkırtısı (3B, 0,12). | Sentez: ortam_uret.py → kat_olaylari() | **Kaldırıldı** (#74) |
| S10 | `Audio/Olay/Olay_Araba.wav` · 7,0 sn | Kat olayı: sokaktan bir araba geçer (3B, 0,15). | Sentez: ortam_uret.py → kat_olaylari() | **Kaldırıldı** (#74) |
| S11 | `Audio/Daire/Radyo_Istasyon.wav` · 23 sn | 2. kat: Nuri'nin radyosu, daireye girince 4 sn sonra kendiliğinden açılır; kapalı kapının ardından boğuk duyulur, sesle bulunur (3B, 0,8, döngü). | Sentez + kayıt: Joth "The Surreal Truth" bant geçirildi, hışırtı eklendi (kayit_isle.py → radyo()) | **Değişsin**: daha anlaşılır bir haber radyosu 90lı yıllara ait → **Yeni:** Carlos Gardel, "Mi noche triste" (1917 tango, kamu malı), eski radyo sesi; haber yok (Şahan: yalnızca tango) |
| S12 | `Audio/Anomali/Boru_Damla.wav` · 16 sn | 5. kat: banyodaki lavabonun düzenli damlası (3B, 0,7, döngü); sifon sökülünce kesilir. | Sentez: ortam_uret.py → duzenli_damla() | **Değişsin**: ses çok dijital biraz eski tiplemelerde bulabilirmisin → **Yeni:** Funkelfang, "Water Drip – Leaky Faucet in Ceramic Sink" (Freesound, CC-BY 4.0; jenerikte) |

---

## 4. Dış sesler (pencereden, 40-90 sn'de bir)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S13 | `Audio/Dis/Dis_Kopek_1.wav` · 11,0 sn | Uzakta köpek, seyrek havlamalar. | Kayıt: Freesound 849570, spirit.zip "dog barking at night" (CC0) | Kalsın |
| S14 | `Audio/Dis/Dis_Kopek_2.wav` · 9,5 sn | Uzakta köpek, sık bir havlama nöbeti. | Kayıt: aynı | Kalsın |
| S15 | `Audio/Dis/Dis_Silah_1.wav` · 4,1 sn | Uzakta tek el silah, şehir yankısıyla. | Kayıt: BigSoundBank #2854 "Rifle shot 2" (Joseph Sardin, CC0) | **Değişsin**: silah sesine hiç benzemiyor daha gerçekci bir silah sesi olabilir. → **Yeni:** aynı BigSoundBank kaydı, neredeyse ham işlem (`162a7c4`) |
| S16 | `Audio/Dis/Dis_Silah_2.wav` · 5,2 sn | Uzakta iki el silah, 1,1 sn arayla. | Kayıt: aynı | **Değişsin**: silah sesine hiç benzemiyor daha gerçekci bir silah sesi olabilir. → **Yeni:** aynı BigSoundBank kaydı, neredeyse ham işlem (`162a7c4`) |
| S17 | `Audio/Dis/Dis_Ruzgar_1.wav` · 14 sn | Pencereden sızan ince rüzgâr, esinti 1. | Kayıt: BigSoundBank #0147 "Whistling of the wind 1" (Joseph Sardin, CC0) | **Kaldırıldı** (#74) |
| S18 | `Audio/Dis/Dis_Ruzgar_2.wav` · 13 sn | Esinti 2. | Kayıt: aynı | **Kaldırıldı** (#74) |
| S19 | `Audio/Dis/Dis_Ruzgar_3.wav` · 14 sn | Esinti 3. | Kayıt: aynı | **Kaldırıldı** (#74) |

---

## 5. Oyuncu ve etkileşim sesleri

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S20 | `Resources/Sesler/Adim_1.wav` … `Adim_8.wav` · 0,41 sn | Oyuncunun PARKE adımı (2B, 8 gerçek adım, art arda aynısı çalmaz, perde ±%4). Aynı dosyalar kalın perdeyle (0,62) Gölge'nin ve kaçış siluetinin parke adımı; Adim_1 ayrıca ayakkabı rafa konunca. En gür 100 ms -19,5 dB. | Kayıt (#101): sturmankin wood_15a_darkshoes_walk (CC0) → kayit_isle.py adimlar | **Değişsin**: oyun eski bir apartmanda geçtiği için ara ara adım attığımızda parke gıcırtısı gelebilir. → sentez adım + parke gıcırtısı (qubodup, CC0). → **#101:** Şahan adımların korku oyunlarındaki gibi gerçek olmasını istedi; dört zemin aynı kişinin aynı ayakkabısıyla kaydedilmiş bir bankadan, gıcırtı kaldı. Şahan dinleyecek |
| S20m | `Resources/Sesler/Merdiven_Adim_1.wav` … `_8.wav` · 0,41 sn | Oyuncunun BETON adımı: merdiven, sahanlık, daire dışı her yer; Gölge'nin de. En gür 100 ms -19 dB. | Kayıt (#101): sturmankin concrete_15a_darkshoes_walk (CC0) → kayit_isle.py adimlar | #74'te G.Lamont'un merdiven kaydından 4 adımdı; #101'de parkeyle aynı ayakkabı oldu. Şahan dinleyecek |
| S20f | `Resources/Sesler/Adim_Fayans_1.wav` … `_8.wav` · 0,41 sn | Oyuncunun FAYANS adımı (banyo, mutfak): sert, parlak topuk; küçük banyonun kısa yankısı kayıtta. Ayna korkusunun adımı da Adim_Fayans_1. En gür 100 ms -19 dB. | Kayıt (#101): sturmankin ceramic_14a_lightshoes_walk (CC0) → kayit_isle.py adimlar | #87'de sentezdi; #101'de gerçek kayıt. Şahan dinleyecek |
| S20h | `Resources/Sesler/Adim_Hali_1.wav` … `_8.wav` · 0,41 sn | Oyuncunun HALI adımı (halı, kilim, yolluk, paçavra): boğuk, yumuşak; 5 kHz üstü kısık (kaydın hışırtısı da azalır). En gür 100 ms -23 dB (öbürlerinden ~4 dB kısık). | Kayıt (#101): sturmankin carpet_15a_darkshoes_walk (CC0) → kayit_isle.py adimlar | #87'de sentezdi; #101'de gerçek kayıt. Şahan dinleyecek |
| S21 | `Resources/Sesler/Adim_2.wav` · 0,42 sn | Ayak sesi, çeşit 2 (bkz. S20). | Sentez: aynı | **Kaldırıldı** (#74) |
| S22 | `Resources/Sesler/Adim_3.wav` · 0,42 sn | Ayak sesi, çeşit 3. | Sentez: aynı | **Kaldırıldı** (#74) |
| S23 | `Resources/Sesler/Adim_4.wav` · 0,42 sn | Ayak sesi, çeşit 4. | Sentez: aynı | **Kaldırıldı** (#74) |
| S24 | `Resources/Sesler/Anahtar_Al.wav` · 0,5 sn | Anahtar ya da herhangi bir eşya alınınca pirinç şıngırtısı; sofraya tabak konunca da (3B). | Sentez: kapi_sesleri_uret.py → anahtar_al() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** sophiehall3535, "Pick Up Keys 1" (Freesound, CC0) |
| S25 | `Resources/Sesler/Cam_Kirilma.wav` · 1,3 sn | Kavanoz ya da cam kırılınca (kırma kilidi, 3B). | Sentez: kapi_sesleri_uret.py → cam_kirilma() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** loganzsound, "Glass break hammer" (Freesound, CC0) |
| S26 | `Resources/Sesler/IcKapi_Acilma.wav` · 1,1 sn | Daire içi oda kapısı, çekmece, dolap kapağı, sandık ve buzdolabı açılırken (3B). | Sentez: kapi_sesleri_uret.py → ic_acilma() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** BigSoundBank #1159 "Door squeaks and rubs #1" (CC0), ilk 2,8 sn |
| S27 | `Resources/Sesler/IcKapi_Kapanma.wav` · 0,9 sn | Aynı şeyler kapanırken (3B). | Sentez: kapi_sesleri_uret.py → ic_kapanma() | **Kaldırıldı** (#74) |
| S28 | `Resources/Sesler/IcKapi_Kilitli.wav` · 1,0 sn | Kilitli oda kapısını ya da kilitli çekmeceyi zorlayınca sarsılma (3B). | Sentez: kapi_sesleri_uret.py → ic_kilitli() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** Kodack, "Trying to open a locked door" (Pixabay) |
| S29 | `Resources/Sesler/Kapi_Devrilme.wav` · 1,6 sn | Menteşeleri sökülen kapı yere devrilince (3B). | Sentez: kapi_sesleri_uret.py → kapi_devrilme() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** qubodup "Door Smash 1" (CC0) + Alex_Jauk "Large Wood Plank Falls Down" (Pixabay), üst üste |
| S30 | `Resources/Sesler/Vida_Sokme.wav` · 0,55 sn | Her menteşe vidası sökülürken (3B). | Sentez: kapi_sesleri_uret.py → vida_sokme() | **Değişsin**: torna vida ile sökülüyor kapı ona uygun bir ses bulmamız lazım. → **Yeni:** Fwughox_Official, "Screwing into wood" (Freesound, CC0) |
| S31 | `Resources/Sesler/KatKapisi_Gicirti.wav` · 1,8 sn | Demir kat kapısı açılırken gıcırtı; 7. katta gerçek kapı çalınınca açılırken de (3B). | Sentez: kapi_sesleri_uret.py → gicirti() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** BigSoundBank #0683 "Grinding metal gate" (CC0), 6,4-8,4 sn |
| S32 | `Resources/Sesler/KatKapisi_Kilit.wav` · 1,0 sn | Kilit açılma sesi: kat kapısına anahtar takılınca, çatı kapısında ve her çözülen kilitte (3B). | Sentez: kapi_sesleri_uret.py → kilit_acilma() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** BigSoundBank #0378 "Lock and big key" (CC0), ilk çekim |
| S33 | ~~`Resources/Sesler/KatKapisi_Kilitli.wav`~~ → `IcKapi_Kilitli.wav` (S27'nin kaydı; #143) | Anahtarsız kat kapısını ya da çatı kapısını sarsınca (3B). | Sentez: kapi_sesleri_uret.py → sarsma() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** launemax, "Rattling at an iron gate" (Freesound, CC0) |
| S34 | `Resources/Sesler/Kol_Cekme.wav` · 0,9 sn | Kol çekilince (kilerdeki kol, sigorta kutusunun şalteri, 3B). | Sentez: kapi_sesleri_uret.py → kol_cekme() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** kyles, "switch big breaker metal click on, off" (Freesound, CC0) |
| S35 | `Resources/Sesler/Sigorta_Takma.wav` · 1,4 sn | Sigorta kutusuna ya da asansör panosuna sigorta takılınca (3B). | Sentez: kapi_sesleri_uret.py → sigorta_takma() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** C-V "Screwing in light bulb" (CC0) + kyles şalter tıkı |
| S36 | `Resources/Sesler/Sifre_Cark.wav` · 0,12 sn | Şifreli asma kilidin yakın ekranında her çark tıkırtısı. | Sentez: kapi_sesleri_uret.py → sifre_cark() | **Değişsin**: daha gerçekçi seslere bakalım. → **Yeni:** Group39, "SFX-Int.-Combination Lock" (Freesound, CC0) |
| S37 | `Resources/Sesler/Radyo_Dugme.wav` · 0,12 sn | Plastik tık: radyonun düğmesi (2. kat) ve asansörün çağrı düğmesi (8. kat). | Sentez: kayit_isle.py → radyo() | Kalsın |
| S38 | `Resources/Sesler/Lamba_Sonme.wav` · 0,4 sn | Kafa lambası açılıp kapanırken ve titrerken tık (2B). (Eski kuralda cezadaki lamba sönmesi.) | Sentez: oyuncu_sesleri_uret.py → lamba_sonme() | Kalsın |
| S39 | `Resources/Sesler/Poset_Hisirti.wav` · 0,7 sn | Kâğıt/poşet hışırtısı: çöp poşeti alınınca, ayarlarda ses kaydırıcısının örnek sesi, defter simgesi. | Sentez: oyuncu_sesleri_uret.py → poset() | Kalsın |

---

## 6. Senaryolu anlar (kaçış, asansör, 7. kat kapıları, Gölge, final)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S40 | `Audio/Anomali/Asansor_Ding.wav` · 2,4 sn | Asansör "ding"i: 8. katta asansör gelince; 7. katta anahtar alınınca kaçış başlarken; finalde dışarıdan, dokuzuncu katın penceresi yanmadan önce (kısık, 2B). | Sentez: asansor_ding_uret.py | **Değişsin**: 90 yıllardaki asansör sesine uygun bir ses lazım. → **Yeni:** zil: cognito perceptu "elevator bell doors" (CC0); asansörün kendisi BigSoundBank #1712 (CC0): yolculuk uğultusu (döngü), varış, kalkış |
| S41 | `Audio/Anomali/Asansor_Ugultu.wav` · 8 sn | Derin kuyu uğultusu (döngü, 3B): 8. katta asansör gelirken; 7. kattaki kovalayan siluetin ve her kattaki Gezen Gölge'nin çevresinde. | Sentez: ortam_uret.py → asansor_ugultu() | Değişsin: kaçışta ve Gölge'de yerini S08 aldı (uğultu kalktı). Asansördeki kullanımı S40'ın 90'lar asansör sesiyle değişecek. → **Yeni:** asansörde BigSoundBank #1712'nin yolculuk uğultusu; kaçışta ve Gölge'de uğultu yok (S08 habercisi) |
| S42 | `Audio/Anomali/Kapi_Tiklatma.wav` · 1,8 sn | 7. kat: "KAPIYI ÇAL" ile dört 13 kapısından birini tıklatma (3B). | Kayıt: Pixabay "wooden door knock" (asıl Freesound 348371, FrostyFrost, CC0) | Kalsın |
| S43 | `Audio/Anomali/Kapi_Carpma.wav` · 2,5 sn | 7. kat, yanlış kapı: kapının ardından çarpan kapı (üç olaydan biri, 3B). | Kayıt: Freesound 637482, kyles "door wood apartment slam" (CC0) | Kalsın |
| S44 | `Audio/Anomali/Bebek_Ters.wav` · 14 sn | 7. kat, yanlış kapı: tersten çalınan bebek ağlaması (üç olaydan biri, 3B). | Kayıt: S06'nın kaydı ters çevrildi | Kalsın |
| S45 | `Audio/Anomali/Zil_DingDong.wav` · 2,6 sn | 7. kat, yanlış kapı: kapı zili ding-dong (üç olaydan biri, 3B). | Sentez: zil_uret.py | **Değişsin**: 90 kar dairelere uygun bir zil sesi ile değişmesi gerekiyor. → **Yeni:** craigsmith, "R09-13-Apartment Buzzer" (Freesound, CC0) |
| S46 | `Resources/Sesler/Lamba_Cizirti.wav` · 0,9 sn | Gezen Gölge belirirken elektrik cızırtısı (3B). (Eski kuralda cezadaki son lambanın titremesi.) | Sentez: oyuncu_sesleri_uret.py → lamba_cizirti() | **Kaldırıldı** (#74) |
| S47 | `Resources/Sesler/Guvercin_Kanat.wav` · 3,8 sn | Çatı kapısı açıldığı an güvercinlerin havalanması (3B). | Kayıt: Freesound 701283, 5ro4 "pigeons flying wings noise" (CC0) | Kalsın |
| S48 | `Audio/Ortam/Ortam_Sabah.wav` · 20 sn | Sabah: uzak şehir, hafif rüzgâr, kuşlar. Çatı kapısı açılınca çatıdan içeri dolar; finaldeki dış görüntüde de çalar. | Sentez: oyuncu_sesleri_uret.py → sabah() | Kalsın |

---

## 7. Yalnızca eski anomali kartında (şu an hiç çalmıyor)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S49 | `Audio/Anomali/Bebek_Yakin.wav` · 14 sn | Asansör kuyusundan yakın, boğuksuz bebek ağlaması. Yalnızca kapalı bir anomali kartı kullanıyor; oyunda hiç çalmıyor. | Kayıt: S06'nın kaydı, kuyu yankısıyla | **Kaldırıldı** (#74) |

---

## 8. Yeni sesler (2026-10-01, #91 #95 #96; Şahan dinleyecek)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S50 | `Resources/Sesler/Kalp_Atisi.wav` · 0,45 sn | Gerilim yükselince oyuncunun kalp atışı (2B, oda yankısı yok); gerilim 1. kademede yavaş, avda hızlı (#91). | Sentez: oyuncu_sesleri_uret.py → kalp_atisi() | yeni, Şahan dinleyecek |
| S51 | `Resources/Sesler/Gok_Gurultusu_1.wav` · 8,9 sn | Şimşekten 1-3 sn sonra yakın gök gürültüsü (2B, uzaklığa göre kısılıp boğulur) (#95). | Sentez: hava_uret.py → gok_gurultusu() | yeni, Şahan dinleyecek |
| S52 | `Resources/Sesler/Gok_Gurultusu_2.wav` · 7,2 sn | Uzak gök gürültüsü (#95). | Sentez: hava_uret.py → gok_gurultusu() | yeni, Şahan dinleyecek |
| S53 | `Audio/Anlar/An_Gicirti.wav` · 1,1 sn | 2. kat sallanan sandalye, 3. kat beşik: her salınımın ucunda ahşap gıcırtısı (3B) (#96). | Sentez: an_sesleri_uret.py | yeni, Şahan dinleyecek |
| S54 | `Audio/Anlar/An_Devrilme.wav` · 1,5 sn | 1. kat: koli devrilip yere düşer (3B) (#96). | Sentez: an_sesleri_uret.py | yeni, Şahan dinleyecek |
| S55 | `Audio/Anlar/An_Vizilti.wav` · 1,05 sn | 4. kat: hol lambası titreyerek yanarken elektrik vızıltısı (3B) (#96). | Sentez: an_sesleri_uret.py | yeni, Şahan dinleyecek |
| S56 | `Audio/Anlar/An_Damla.wav` · 5,2 sn | 5. kat: su kesikken kovaya hızlanarak damlayan su (3B) (#96). | Sentez: an_sesleri_uret.py | yeni, Şahan dinleyecek |
| S57 | `Audio/Anlar/An_Parazit.wav` · 5,8 sn | 8. kat: tepegöz kendiliğinden açılır: şalter, uğultu, fan, çatırtı (3B) (#96). | Sentez: an_sesleri_uret.py | yeni, Şahan dinleyecek |

---

## 9. Nesneye uyan sesler (2026-10-01, #99 #100; Şahan dinleyecek)

Hepsi `esya_sesleri_uret.py` ile gürültüden üretildi (sinüs yok, indirme yok). Hangi nesnenin hangisini çaldığına `SesMalzemesi` (saf C#, testli) karar verir.

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S58 | `Resources/Sesler/Cekmece_Acilma.wav` · 0,6 sn | Ahşap çekmece açılınca (komodin, tezgâh, portmanto) | Sentez: esya_sesleri_uret.py → cekmece | yeni, Şahan dinleyecek |
| S59 | `Resources/Sesler/Kapak_Acilma.wav` · 0,6 sn | Küçük ahşap dolap kapağı, sandık kapağı, ahşap gardırop kapısı | Sentez → kapak | yeni |
| S60 | `Resources/Sesler/Buzdolabi_Acilma.wav` · 0,75 sn | Buzdolabı kapısı: conta ayrılır, raftaki şişeler tınlar | Sentez → buzdolabi | yeni |
| S61 | `Resources/Sesler/Metal_Kapak_Acilma.wav` · 0,8 sn | Sac/çelik dolap, dosya dolabı, alet kutusu, sigorta kutusu kapağı; metal gardıroplar (saklanma) | Sentez → metal_kapak | yeni |
| S62 | `Resources/Sesler/Kavanoz_Acilma.wav` · 0,5 sn | Kavanozun vidalı kapağı, sökülen lavabo sifonu | Sentez → kavanoz | yeni |
| S63 | `Resources/Sesler/Agir_Surtunme.wav` · 1,3 sn | İtilen ağır gardırop; ekipman olmadan zorlanan ağır mobilya | Sentez → agir_surtunme | yeni |
| S64 | `Resources/Sesler/Cekmece_Kilitli.wav` · 0,36 sn | Kilitli çekmece/kapak çekiştirilince | Sentez → cekmece_kilitli | yeni |
| S65 | `Resources/Sesler/Cam_Tikirti.wav` · 0,3 sn | Alet olmadan kavanoz zorlanınca | Sentez → cam_tikirti | yeni |
| S66 | `Resources/Sesler/Al_Metal.wav` · 0,42 sn | Tornavida, levye, kaydırıcı alınınca | Sentez → al_metal | yeni |
| S67 | `Resources/Sesler/Al_Kagit.wav` · 0,42 sn | Not, çizim parçası alınınca; not ve fotoğraf açılınca | Sentez → al_kagit | yeni |
| S68 | `Resources/Sesler/Al_Seramik.wav` · 0,42 sn | Tabak alınınca ve sofraya konunca | Sentez → al_seramik | yeni |
| S69 | `Resources/Sesler/Al_Kucuk.wav` · 0,24 sn | Sigorta alınınca | Sentez → al_kucuk | yeni |
| S70 | `Resources/Sesler/Kilit_Kucuk.wav` · 0,45 sn | Oda kapısı kilidi, asma kilit açılınca | Sentez → kilit_kucuk | yeni |
| S71 | `Resources/Sesler/Dugme_Bakalit.wav` · 0,16 sn | Elektrik düğmesi (#100), asansör çağır düğmesi | Sentez → dugme_bakalit | yeni |
| S72 | `Resources/Sesler/Kucuk_Tikirti.wav` · 0,16 sn | Takılma ipucu: aranan eşyanın yerinden küçük tıkırtı (#98) | Sentez → kucuk_tikirti | yeni |
| S73 | `Audio/Anlar/An_Surtunme.wav` · 0,75 sn | 6. kat: manken ahşap ayağında döner (#96) | Sentez → an_surtunme | yeni |
| S74 | `Resources/Sesler/Kapi_Gicirti_Dongu.wav` · 2,0 sn (döngü) | Oda kapısı sürüklenirken menteşe gıcırtısı; sesi dönüş hızına göre (#100) | Sentez → kapi_gicirti_dongu | yeni |
| S75 | `Resources/Sesler/Vana_Cark.wav` · 1,85 sn | 5. kat: salondaki vananın çarkı döndürülünce (paslı mil gıcırtısı, diş sürtünmesi, iki yeniden kavrama, kapanırken borudaki su) | Sentez → vana_cark | yeni, Şahan dinleyecek |
| S76 | `Resources/Sesler/Anahtarlik.wav` · 2,6 sn | Gölge'nin İMZASI (#105): her belirişinden önce aynı ses, önceki kapıcının anahtarlığının üç şıkırtısı, gittikçe pes, koridor yankılı. Gezen Gölge, av, 7. kat kovalamacası, 6. kat saklanma anı, ayna, duvardaki gölge (`GolgeImzasi.Cal`, 3B, 1,5-18 m). | Kayıt: S24 anahtar kaydından (sophiehall3535, CC0) → kayit_isle.py anahtarlik | yeni (#105), Şahan dinleyecek |
| S77 | `Resources/Sesler/Lamba_Vinlama.wav` · 3,0 sn (döngü) | **#143: kafa lambasında artık çalmıyor** (Şahan: "fener sesi gereksiz"); yalnız arızalı asansör lambası (`ArizaliLamba`, #111) kullanır. Eski kullanım — kafa lambasının elektrik vınlaması (#105): gerilim 0,25'te başlar, 0,85'te tam (en çok 0,22 ses), perdesi %8 tizleşir; lamba kapalıyken susar. 2B, oda yankısından muaf. | Sentez: oyuncu_sesleri_uret.py lamba_vinlama (100 Hz harmonikleri + 7,8 kHz ıslık + bant gürültüsü; döngü için 3 sn'ye tam periyot) | yeni (#105), Şahan dinleyecek |
| S78 | `Resources/Sesler/Radyo_Parazit.wav` · 2,3 sn (döngü) | Radyo anonsunun (#106) önü ve arkası: bant gürültüsü ve seyrek çıtırtı. `RadyoAnonsu`: düğme tıkı (S37) → parazit 0,8 sn kabarır → seslendirme (`Konusmalar/radyo_katN`, varsa) → parazit 0,6 sn söner → tık. Her dairede bir kez, oyuncu içeri girdikten 18-35 sn sonra. | Sentez: kayit_isle.py radyo_parazit | yeni (#106); seslendirmeler Şahan'ın kaydını bekliyor |
| S79 | `Resources/Sesler/Kirilma_Tabak.wav` · 0,95 sn | Cin olayı (#131): oyuncunun 2-4 m arkasında seramik tabak kırılır (3B); yerde `Cam_Kiriklari` parçası kalır. Üç kırılma sesinden rastgele biri. | Sentez: esya_sesleri_uret.py → kirilma_tabak | yeni (#131); Şahan'ın dinlemesini bekliyor |
| S80 | `Resources/Sesler/Kirilma_Bardak.wav` · 0,75 sn | Cin olayı (#131): cam bardak kırılması; tiz, ince, uzun çınlayan parçalar. | Sentez: esya_sesleri_uret.py → kirilma_bardak | yeni (#131) |
| S81 | `Resources/Sesler/Kirilma_Vazo.wav` · 1,25 sn | Cin olayı (#131): ağır, boş seramik vazo; alçak darbe, iri parçalar, uzun yerleşme. | Sentez: esya_sesleri_uret.py → kirilma_vazo | yeni (#131) |
| S82 | `Resources/Sesler/Cin_Stinger_1.wav` · 1,9 sn | Üç harflinin belirişi (#143, madde 10): figürün yerinden (3B, 2-25 m) tırmanan uyumsuz bir küme + alt-bas darbe; iki dosyadan rastgele biri (`CinSesi.Belirdi`). Belirme, geçiş, kaçış, gezen gölge, av, 7. kat kovalamacası, 6. kat saklanma anı, ayna korkusu, dokuzuncu katta kalkış. | Sentez: cin_sesleri_uret.py → stinger (dar bant süzgeçli gürültü, kayan frekans; sinüs yok) | yeni (#143); Şahan'ın dinlemesini bekliyor |
| S83 | `Resources/Sesler/Cin_Stinger_2.wav` · 1,9 sn | S82'nin ikinci tohumu. | Sentez: cin_sesleri_uret.py → stinger | yeni (#143) |
| S84 | `Resources/Sesler/Cin_Ugultu.wav` · 3,5 sn | Her belirişte oyuncunun 1,2 m ARKASINDAN (3B, 0,8-6 m) alçak, titreyen uğultu; karartmalı yaklaşmada ve duvardaki gölgede vuruşsuz tek başına. Yanında varsa `Nefes_Cin_1..3` (Şahan'ın kaydı; yoksa sessiz). | Sentez: cin_sesleri_uret.py → ugultu | yeni (#143) |

Değişen bağlantılar: ayna korkusu artık fayansta tek adım (`Adim_Fayans_1`), 7. kattaki ahşap daire kapısı oda kapısı gıcırtısı (`IcKapi_Acilma`), Gölge'nin adımları bastığı zemine göre (`Merdiven_Adim`, `Adim_1`, `Adim_Fayans`, `Adim_Hali`).
