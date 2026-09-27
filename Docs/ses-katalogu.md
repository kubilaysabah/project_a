# Ses kataloğu (#28, #74)

Oyundaki bütün sesler, numarasıyla (2026-09-27, #74 için baştan sayıldı; eski S01-S27 numaraları geçersiz).
Şahan her sesi **Karar** sütununda *Kalsın* ya da *Değişsin* diye işaretler; değişecek seslere sonra birlikte bakılır.
İşaretleme yerel dinleme sayfasında yapılır (sesler orada oyundaki seviyede çalar); sonuçlar buraya işlenir.

**Genel bilgi**
- **3B** ses dünyada bir noktadan gelir (uzaklaştıkça kısılır, yönü duyulur); **2B** ses "kafanın içinden" gelir.
- **Seviye** 0-1 arası (1 = dosyanın tam gücü); ayarlardaki ses düzeyi hepsini birlikte ölçekler.
- **Sentez** sesler `Art/Ses/scripts/` içindeki betiklerle üretilir; **kayıtlar** `Art/Ses/kayitlar/` içinde değiştirilmeden durur, lisansları `LISANSLAR.md`'de.
- Eski kuralın (geri dön / ceza) ve anomali kartlarının sesleri yeni ilerlemede çalmıyor (`KatYoneticisi.eskiKural` kapalı).
- Müzik: `ArkaMuzik` 0,15'te sürekli döner; kaçış ve Gölge anlarında 0,42'ye kabarır; çatıda susar.
- Dış sesler: `DisSesleri` ilk sesi 20-40 sn sonra, sonra 40-90 sn'de bir çalar; aynı tür art arda gelmez; kat olayıyla çakışmaz.

---

## 1. Müzik

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S01 | `Audio/Muzik/Muzik_Gerilim.wav` · 268,5 sn | Arka plan gerilim müziği. Başlık ekranı dahil bütün oyun boyunca döner (2B, 0,15). Kaçış başlayınca ve Gölge belirince 0,42'ye kabarır, 3 sn tutar, ~5 sn'de iner. Çatı kapısı açılınca 4 sn'de susar. | Kayıt: Joth, "Ambience Pack 1 – Sci Fi Horror" (OpenGameArt, CC0), beş parça 3 sn geçişle birleştirildi | |

---

## 2. Sürekli ortam katmanları (her katta, döngü)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S02 | `Audio/Ortam/Ortam_Ugultu.wav` · 30 sn | Binanın çok kısık hava uğultusu, sahanlığın ortasından (3B, 0,25, 4-9 m). Kat olayları bunun çocuğu. | Sentez: ortam_uret.py → ugultu() | |
| S03 | `Audio/Ortam/Ortam_TV.wav` · 85,5 sn | 2 numaranın duvarının ardında boğuk televizyon (3B, 0,5, 1,5-9 m). Her katta kaydın başka bir yeri çalar. | Kayıt: Pixabay, arunangshubanerjee "tv playing in the next room" | |
| S04 | `Audio/Ortam/Ortam_Boru.wav` · 16 sn | Sahanlık duvarının içinde düzensiz boru tıkırtıları (3B, 0,5, 1-8 m). | Sentez: ortam_uret.py → boru() | |
| S05 | `Audio/Ortam/Ortam_Ruzgar.wav` · 12 sn | Merdiven penceresinde hafif rüzgâr (3B, 0,3, 1-6 m). | Sentez: ortam_uret.py → ruzgar() | |

---

## 3. Kat olayları ve katın kendi sesleri

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S06 | `Audio/Ortam/Ortam_Bebek.wav` · 10,4 sn | Kat olayı "bebek": 4 numaranın duvarının ardında bir nöbet bebek ağlaması. Kata varıştan 4-9 sn sonra ve 16-24 sn sonra birer kez (3B, 0,55). | Kayıt: Wikimedia "Crying_newborn_baby.ogg" (the_yura, CC0), boğuklaştırıldı | |
| S07 | `Audio/Olay/Olay_UzakKapi.wav` · 3,4 sn | Kat olayı: bir üst katta uzakta bir kapı kapanır (3B, 0,2). | Sentez: ortam_uret.py → kat_olaylari() | |
| S08 | `Audio/Olay/Olay_UstKatAdimlari.wav` · 5,4 sn | Kat olayı: tavanın üstünde dört ağır adım (3B, 0,2). | Sentez: ortam_uret.py → kat_olaylari() | |
| S09 | `Audio/Olay/Olay_Kalorifer.wav` · 3,3 sn | Kat olayı: sol duvarın dibinde kalorifer borusu tıkırtısı (3B, 0,12). | Sentez: ortam_uret.py → kat_olaylari() | |
| S10 | `Audio/Olay/Olay_Araba.wav` · 7,0 sn | Kat olayı: sokaktan bir araba geçer (3B, 0,15). | Sentez: ortam_uret.py → kat_olaylari() | |
| S11 | `Audio/Daire/Radyo_Istasyon.wav` · 23 sn | 2. kat: Nuri'nin radyosu, daireye girince 4 sn sonra kendiliğinden açılır; kapalı kapının ardından boğuk duyulur, sesle bulunur (3B, 0,8, döngü). | Sentez + kayıt: Joth "The Surreal Truth" bant geçirildi, hışırtı eklendi (kayit_isle.py → radyo()) | |
| S12 | `Audio/Anomali/Boru_Damla.wav` · 16 sn | 5. kat: banyodaki lavabonun düzenli damlası (3B, 0,7, döngü); sifon sökülünce kesilir. | Sentez: ortam_uret.py → duzenli_damla() | |

---

## 4. Dış sesler (pencereden, 40-90 sn'de bir)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S13 | `Audio/Dis/Dis_Kopek_1.wav` · 11,0 sn | Uzakta köpek, seyrek havlamalar. | Kayıt: Freesound 849570, spirit.zip "dog barking at night" (CC0) | |
| S14 | `Audio/Dis/Dis_Kopek_2.wav` · 9,5 sn | Uzakta köpek, sık bir havlama nöbeti. | Kayıt: aynı | |
| S15 | `Audio/Dis/Dis_Silah_1.wav` · 4,1 sn | Uzakta tek el silah, şehir yankısıyla. | Kayıt: BigSoundBank #2854 "Rifle shot 2" (Joseph Sardin, CC0) | |
| S16 | `Audio/Dis/Dis_Silah_2.wav` · 5,2 sn | Uzakta iki el silah, 1,1 sn arayla. | Kayıt: aynı | |
| S17 | `Audio/Dis/Dis_Ruzgar_1.wav` · 14 sn | Pencereden sızan ince rüzgâr, esinti 1. | Kayıt: BigSoundBank #0147 "Whistling of the wind 1" (Joseph Sardin, CC0) | |
| S18 | `Audio/Dis/Dis_Ruzgar_2.wav` · 13 sn | Esinti 2. | Kayıt: aynı | |
| S19 | `Audio/Dis/Dis_Ruzgar_3.wav` · 14 sn | Esinti 3. | Kayıt: aynı | |

---

## 5. Oyuncu ve etkileşim sesleri

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S20 | `Resources/Sesler/Adim_1.wav` · 0,42 sn | Oyuncunun ayak sesi (2B, dört çeşit sırayla). Aynı dosyalar kalın perdeyle (0,62) kaçış siluetinin ve Gezen Gölge'nin ağır adımları; Adim_2 ayrıca ayakkabı rafa konunca. | Sentez: oyuncu_sesleri_uret.py → adimlar() | |
| S21 | `Resources/Sesler/Adim_2.wav` · 0,42 sn | Ayak sesi, çeşit 2 (bkz. S20). | Sentez: aynı | |
| S22 | `Resources/Sesler/Adim_3.wav` · 0,42 sn | Ayak sesi, çeşit 3. | Sentez: aynı | |
| S23 | `Resources/Sesler/Adim_4.wav` · 0,42 sn | Ayak sesi, çeşit 4. | Sentez: aynı | |
| S24 | `Resources/Sesler/Anahtar_Al.wav` · 0,5 sn | Anahtar ya da herhangi bir eşya alınınca pirinç şıngırtısı; sofraya tabak konunca da (3B). | Sentez: kapi_sesleri_uret.py → anahtar_al() | |
| S25 | `Resources/Sesler/Cam_Kirilma.wav` · 1,3 sn | Kavanoz ya da cam kırılınca (kırma kilidi, 3B). | Sentez: kapi_sesleri_uret.py → cam_kirilma() | |
| S26 | `Resources/Sesler/IcKapi_Acilma.wav` · 1,1 sn | Daire içi oda kapısı, çekmece, dolap kapağı, sandık ve buzdolabı açılırken (3B). | Sentez: kapi_sesleri_uret.py → ic_acilma() | |
| S27 | `Resources/Sesler/IcKapi_Kapanma.wav` · 0,9 sn | Aynı şeyler kapanırken (3B). | Sentez: kapi_sesleri_uret.py → ic_kapanma() | |
| S28 | `Resources/Sesler/IcKapi_Kilitli.wav` · 1,0 sn | Kilitli oda kapısını ya da kilitli çekmeceyi zorlayınca sarsılma (3B). | Sentez: kapi_sesleri_uret.py → ic_kilitli() | |
| S29 | `Resources/Sesler/Kapi_Devrilme.wav` · 1,6 sn | Menteşeleri sökülen kapı yere devrilince (3B). | Sentez: kapi_sesleri_uret.py → kapi_devrilme() | |
| S30 | `Resources/Sesler/Vida_Sokme.wav` · 0,55 sn | Her menteşe vidası sökülürken (3B). | Sentez: kapi_sesleri_uret.py → vida_sokme() | |
| S31 | `Resources/Sesler/KatKapisi_Gicirti.wav` · 1,8 sn | Demir kat kapısı açılırken gıcırtı; 7. katta gerçek kapı çalınınca açılırken de (3B). | Sentez: kapi_sesleri_uret.py → gicirti() | |
| S32 | `Resources/Sesler/KatKapisi_Kilit.wav` · 1,0 sn | Kilit açılma sesi: kat kapısına anahtar takılınca, çatı kapısında ve her çözülen kilitte (3B). | Sentez: kapi_sesleri_uret.py → kilit_acilma() | |
| S33 | `Resources/Sesler/KatKapisi_Kilitli.wav` · 1,2 sn | Anahtarsız kat kapısını ya da çatı kapısını sarsınca (3B). | Sentez: kapi_sesleri_uret.py → sarsma() | |
| S34 | `Resources/Sesler/Kol_Cekme.wav` · 0,9 sn | Kol çekilince (kilerdeki kol, sigorta kutusunun şalteri, 3B). | Sentez: kapi_sesleri_uret.py → kol_cekme() | |
| S35 | `Resources/Sesler/Sigorta_Takma.wav` · 1,4 sn | Sigorta kutusuna ya da asansör panosuna sigorta takılınca (3B). | Sentez: kapi_sesleri_uret.py → sigorta_takma() | |
| S36 | `Resources/Sesler/Sifre_Cark.wav` · 0,12 sn | Şifreli asma kilidin yakın ekranında her çark tıkırtısı. | Sentez: kapi_sesleri_uret.py → sifre_cark() | |
| S37 | `Resources/Sesler/Radyo_Dugme.wav` · 0,12 sn | Plastik tık: radyonun düğmesi (2. kat) ve asansörün çağrı düğmesi (8. kat). | Sentez: kayit_isle.py → radyo() | |
| S38 | `Resources/Sesler/Lamba_Sonme.wav` · 0,4 sn | Kafa lambası açılıp kapanırken ve titrerken tık (2B). (Eski kuralda cezadaki lamba sönmesi.) | Sentez: oyuncu_sesleri_uret.py → lamba_sonme() | |
| S39 | `Resources/Sesler/Poset_Hisirti.wav` · 0,7 sn | Kâğıt/poşet hışırtısı: çöp poşeti alınınca, ayarlarda ses kaydırıcısının örnek sesi, defter simgesi. | Sentez: oyuncu_sesleri_uret.py → poset() | |

---

## 6. Senaryolu anlar (kaçış, asansör, 7. kat kapıları, Gölge, final)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S40 | `Audio/Anomali/Asansor_Ding.wav` · 2,4 sn | Asansör "ding"i: 8. katta asansör gelince; 7. katta anahtar alınınca kaçış başlarken; finalde dışarıdan, dokuzuncu katın penceresi yanmadan önce (kısık, 2B). | Sentez: asansor_ding_uret.py | |
| S41 | `Audio/Anomali/Asansor_Ugultu.wav` · 8 sn | Derin kuyu uğultusu (döngü, 3B): 8. katta asansör gelirken; 7. kattaki kovalayan siluetin ve her kattaki Gezen Gölge'nin çevresinde. | Sentez: ortam_uret.py → asansor_ugultu() | |
| S42 | `Audio/Anomali/Kapi_Tiklatma.wav` · 1,8 sn | 7. kat: "KAPIYI ÇAL" ile dört 13 kapısından birini tıklatma (3B). | Kayıt: Pixabay "wooden door knock" (asıl Freesound 348371, FrostyFrost, CC0) | |
| S43 | `Audio/Anomali/Kapi_Carpma.wav` · 2,5 sn | 7. kat, yanlış kapı: kapının ardından çarpan kapı (üç olaydan biri, 3B). | Kayıt: Freesound 637482, kyles "door wood apartment slam" (CC0) | |
| S44 | `Audio/Anomali/Bebek_Ters.wav` · 14 sn | 7. kat, yanlış kapı: tersten çalınan bebek ağlaması (üç olaydan biri, 3B). | Kayıt: S06'nın kaydı ters çevrildi | |
| S45 | `Audio/Anomali/Zil_DingDong.wav` · 2,6 sn | 7. kat, yanlış kapı: kapı zili ding-dong (üç olaydan biri, 3B). | Sentez: zil_uret.py | |
| S46 | `Resources/Sesler/Lamba_Cizirti.wav` · 0,9 sn | Gezen Gölge belirirken elektrik cızırtısı (3B). (Eski kuralda cezadaki son lambanın titremesi.) | Sentez: oyuncu_sesleri_uret.py → lamba_cizirti() | |
| S47 | `Resources/Sesler/Guvercin_Kanat.wav` · 3,8 sn | Çatı kapısı açıldığı an güvercinlerin havalanması (3B). | Kayıt: Freesound 701283, 5ro4 "pigeons flying wings noise" (CC0) | |
| S48 | `Audio/Ortam/Ortam_Sabah.wav` · 20 sn | Sabah: uzak şehir, hafif rüzgâr, kuşlar. Çatı kapısı açılınca çatıdan içeri dolar; finaldeki dış görüntüde de çalar. | Sentez: oyuncu_sesleri_uret.py → sabah() | |

---

## 7. Yalnızca eski anomali kartında (şu an hiç çalmıyor)

| No | Dosya · süre | Ne zaman, nerede | Kaynak | Karar |
|---|---|---|---|---|
| S49 | `Audio/Anomali/Bebek_Yakin.wav` · 14 sn | Asansör kuyusundan yakın, boğuksuz bebek ağlaması. Yalnızca kapalı bir anomali kartı kullanıyor; oyunda hiç çalmıyor. | Kayıt: S06'nın kaydı, kuyu yankısıyla | |
