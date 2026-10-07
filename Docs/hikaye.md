# Huzur Apartmanı — Hikâye ve İlerleme Tasarımı (taslak 2, #50)

**Durum:** Şahan onayladı (2026-09-26, "böyle kalsın"). Uygulama #51-#60 issue'larında; eski kural
yalnızca #59 ile, yeni ilerleme çalışır hâle gelince kaldırılır. Kubilay'ı Şahan bilgilendirecek; Steam konumlandırması ve isim (#1) onun alanı.

## 1. Karar ve gerekçe

Şahan No70: Eye of Basir ve Phasmophobia'yı inceledi: bizde hikâye ve akış yoktu; oyuncu katları
temizliyor ama bir şey bulup ilerleyemiyordu. **"Yanlış kata girme, geri dön" kuralı ve ceza kalkar.**
Oyun kat kat yükselen bir anlatı olur: her katta bir daire açıktır, dairenin içindeki kilit zinciri
çözülünce katın anahtarı bulunur, kilitli kat kapısı açılır, üst kata çıkılır. Anomaliler kalır ama
karar aracı değil **olay** olur. Bina karanlıktır; ışığı kapıcının kafa lambası taşır.

## 2. Öncül

1994, Huzur Apartmanı, sekiz kat. Binada yıllardır "olaylar" oluyor; son üç kapıcı işi bıraktı:

| Kapıcı | Yıllar | Ne oldu | Oyunda izi |
|---|---|---|---|
| Rıza Efendi | 1987-1991 | "Gürültüye dayanamadı", ayrıldı | Odadaki eski defter, 2. katta fotoğrafı |
| Hasan | 1991-1993 | Hastalandı, köyüne döndü | 4. katta notu, odadaki radyo |
| Cemal | 1993-Ağustos 1994 | Bir gece işi bıraktı; eşyaları odada kaldı, kendisi görülmedi | Kafa lambası, alet çantası, notları, 7. kattaki eşyaları |

Oyuncu yeni kapıcıdır (adsız). İlk dakikada odadaki çividen **Cemal'in kafa lambasını** alır; kayışında
adı yazar. Yönetim'in notu kuralı koyar: katlar arasına "güvenlik için" demir kapılar taktırıldı; her
katın işini bitir, katın anahtarını bul, üst kata öyle çık; **sabah çatı kapısını kilitle.** Oyunun
sorusu: kapılar bir şey girmesin diye mi takıldı, çıkmasın diye mi? Cevap dokuzuncu kattır: binada
sekiz kat vardır ama asansör dokuza kadar sayar; önceki kapıcılar oraya çıkmıştır.

## 3. Işık: karanlık bina ve kafa lambası

- Bina karanlık: her katta en fazla bir çalışan sahanlık ampulü, çoğu sönük ya da titrek.
- Kafa lambası kameraya bağlı sıcak bir spot (~40°, yumuşak kenar, 8-10 m); gölge atar (tek ışık, ucuz;
  Düşük kalitede gölgesiz). **L / kolun sol omzu** ile açılıp kapanır (#76; ilk hâli F idi) (karanlıkta durup dinlemek, saklanmak).
- Pil yönetimi **yok**. Lamba yalnızca senaryolu anlarda titrer ya da bir anlığına söner.
- Korku bakışa bağlanır: oyuncu yalnızca huzmedekini görür; olaylar huzmenin kenarında olur.
- "Ampul değiştir" görevi bazı katlarda sahanlık ışığını geri getirir: oyuncu karanlığa karşı bir şey yapabilir.

## 4. Kat = bölüm

1. **Varış.** Kat kapısından geçilir; kapı arkada açık kalır (aşağı inmek serbest, zorunlu iniş yok).
2. **Görev listesi.** Kattaki ilan panosunda Yönetim'in el yazısı: hangi daire, ne iş. Sahanlıkta 1-2 küçük
   iş (çöp, ayakkabı, ampul, ilan) ve dairenin sebebi ("2 numara taşındı; çöpünü içeriden al").
3. **Açık daire.** Dört kapıdan yalnızca biri açılır; sırası katlara göre 2, 3, 1, 4, 2, 3, 1, 4. Diğer üç
   kapı kapalıdır; arkalarından ses ve kapı altından kâğıt gelir. Kiracılar hiç görülmez.
4. **Kilit zinciri.** Dairenin içinde katla artan sayıda kilitli oda; son kilidin ardında katın anahtarı.
5. **Kat kapısı.** Anahtarla açılır, anahtar tükenir, kapı açık kalır; **otomatik kayıt** burada.

Envanter yok. Elde tek bir eşya taşınır (anahtar, sigorta, tornavida, not); köşede küçük simgesi görünür.
Bulunan notlar otomatik olarak kapıcı odasındaki panoya asılır.

## 5. Daire: 3+1, her katta aynı plan

Gerçek apartmanlarda katların planı aynıdır; 1 ile 4, 2 ile 3 birbirinin aynası. Daire bir kez kurulur
(Blender + kurucu), her katta farklı döşenir ve farklı odaları kilitlenir. Odalar:

| Oda | Not |
|---|---|
| Antre | Giriş; sigorta kutusu, anahtarlık, ayakkabılık |
| Salon | En büyük oda; TV, koltuk, vitrin, takvim, telefon |
| Mutfak | Buzdolabı, çekmeceler, kavanozlar (küçük eşyaların yeri) |
| Banyo | Dar; ayna (olay yeri) |
| Oda A | Ebeveyn: dolap, komodin, valiz |
| Oda B | Çocuk / misafir: yatak, oyuncak kutusu, boy cetveli |
| Oda C | Küçük oda / kiler: raflar, kutular |
| Balkon | Her zaman kilitli; camdan gece görünür |

Dairenin "kimin" olduğu döşemeden okunur: taşınan aile, hastanedeki yaşlı, çocuklu aile, boş daire,
Cemal'in son çalıştığı daire, sofrası kurulu boş daire, Yönetim dairesi.

## 6. Kilit türleri

**Temel kural (Şahan, 2026-09-27, ilerleme bunun üzerine kurulur):** dairenin içinde de odalar kilitlidir.
Oyuncu bir odadan ötekine ancak bir **anahtar**, bir **şifre** ya da bir **kol çekerek** geçer. Dairenin
bütün odaları açılmaz: zincirin dışında kalan odalar o katta hep kapalı kalır (arkalarından ses gelebilir).
Açık olan yalnızca antre, koridor ve zincirin başladığı oda; gerisini oyuncu açarak kazanır.

Beş tür, her karışımı taşır; oyuncu kuralları ilk katlarda öğrenir:

| Tür | Nasıl çözülür | İlk görüldüğü kat |
|---|---|---|
| **Anahtar** | Dairede bir yerde bulunur, tek kapıyı açar | 1 |
| **Şifreli asma kilit** | 4 haneli şifre odadaki bir şeyde: takvim, boy cetveli, fatura, doğum günü | 2 |
| **Kol** | Bir yerde bir kol/manivela çekilir, başka yerdeki bir kapının kilidi "tak" diye açılır: kilerde duvar kolu, sigorta kutusunun ana şalteri, sifon zinciri, tablonun arkasındaki gizli kol | #55'te belirlenecek (öneri 3) |
| **Sigorta / elektrik** | Sigorta atık; sigorta teli bulunup takılınca ışık ve elektrikli şeyler çalışır | 4 |
| **Tornavida ile menteşe** | Kilidi açılmayan kapının menteşeleri sökülür; tornavida dairede | 5 |
| **Kırma** (#61) | Bir nesne (cam kavanoz, cam dolap) bulunan bir aletle kırılır, içindeki ortaya çıkar | 1 |

**Aletler** (tornavida gibi) bitmez ve eli meşgul etmez: bir kez bulunan alet kafa lambası gibi
kapıcıda kalır, sonraki katlarda da işe yarar (Şahan, 2026-09-27). Elde tek eşya kuralı yalnızca
anahtar, sigorta ve not için geçerlidir.

Zincir yükselişi: **1-1-2-2-2-3-3-3** (kat başına geçilmesi gereken kilitli oda sayısı; zincirin
dışındaki kapalı odalar bu sayıya girmez).

## 7. Kat kat taslak

**Kat çıktıkça bulmak zorlaşır (Şahan, 2026-09-27).** Parçalar ve anahtarlar yapboz gibi birbirini
tamamlar: bir kapının anahtarı kırık olup parçaları farklı odalarda olabilir, bir odanın kilidi başka
bir odadaki kolla açılabilir, önemli oda (5. katta gardıroplu salon) hemen açılmaz. Her kat bir öncekinden
daha uzun ve dolambaçlı olur: 5. kat beş kilit; 6-8. katlar en az bu kadar, daha çok saklı yer ve boş kapla.

**Her katın geçişi farklıdır (Şahan, 2026-09-27).** Kilit türleri her katta karışık kullanılır ama her
katın bir **imza mekaniği** vardır; oyuncu hiçbir katı "yine aynısı" diye geçmez:

| Kat | İmza mekaniği | İş |
|---|---|---|
| 1 | Anahtar + kırma (öğretici): ayakkabılıktaki tornavidayla kavanozu kır, anahtarı al | #54, #61 |
| 2 | Sesle bulma: kendiliğinden açılan radyonun sesini takip et | #62 |
| 3 | Parça birleştirme: çocuğun yırtılmış çizimini odalardan topla, şifre birleşince okunur | #63 |
| 4 | Işık ve karanlık: sigorta, ancak ışıkta görünen not ve yol | #64 |
| 5 | Mekanizma: vana, su saati, itilen dolap; suyu kesince lavabodan anahtar | #65 |
| 6 | Sıralama/yerleştirme + zamanlı: sofraya dört tabak; kaçış anı 1 | #66, #58 |
| 7 | Gözlem (anomali) + kovalamaca: dört "13" kapısından gerçeği; kaçış anı 2 | #67, #58 |
| 8 | Asansör: panoya sigorta, kabini çağır, çatı anahtarı kabinde | #68 |

Aşağıdaki tablonun zincir sütunu bu mekaniklere göre #55'te yeniden yazılır.

| Kat | Daire, kimin | Kilitli odalar ve zincir | Sahanlık işi | Olaylar | Hikâye |
|---|---|---|---|---|---|
| Zemin | Kapıcı odası | — | Notu oku, kafa lambasını al, 1. kat anahtarı çivide | — | Cemal'in eşyaları, yarım not |
| 1 | 2, taşınmış aile (kutular) | **1:** Oda A anahtarı mutfakta kavanozda ("anahtarlar kavanozda" notu) → Oda A komodininde kat anahtarı | Çöp, ampul değiştir | Yalnız ortam; ilk FISILTI | Yönetim'in tahliye yazısı |
| 2 | 3, hastanedeki yaşlı (Nuri) | **1 (kuruldu, #62):** Oda A'daki radyo oyuncu daireye girince kendiliğinden çalar, kapalı kapının ardından boğuk duyulur; kadranındaki istasyon (102.7) kilerin (Oda C) asma kilidi; Nuri'nin notu kilerin yanında; kat anahtarı kilerde alet çantasında. Banyo ve Oda B kapalı | Sayaç oku, ilan bırak | Radyo kendi açılır, terlik yürür | Rıza Efendi'nin fotoğrafı; **telefon 1** |
| 3 | 1, çocuklu aile (gece yoklar) | **2 (kuruldu, #63):** Kerem'in yırtık çiziminin dört parçası: sahanlıktaki ayakkabılıkta, salonda kanepenin altında (eğilerek, #79), buzdolabında, Oda B'deki oyuncak sandığında (Oda B'nin anahtarı mutfak çekmecesinde); birleşen resmin köşelerinde 0 3 0 9 → Oda A'nın (yatak odası) asma kilidi; komodinde kat anahtarı. Annenin notu koridorda. Banyo ve kiler kapalı | Ayakkabıları rafa koy | İçeriden bebek, tavandaki terlik | Çocuğun çizimi: "dokuzuncu kattaki adam" |
| 4 | 4, boş ve elektriksiz | **2 (kuruldu, #64):** mutfak dolabının şifresi salonla Oda B arasındaki duvarda fosforlu boyayla, yalnızca kafa lambası kapalıyken görünür → dolapta sigorta → takılınca antre, koridor, salon ve mutfağın lambaları titreyerek yanar, buzdolabı açılır → iç ışığında Hasan'ın ikinci notu ve kat anahtarı. Yalnızca salon ve mutfak açık | Çöp | Karanlık kat, göz kırpan ışık, kafa lambası titrer | Hasan'ın notu |
| 5 | 2, Cemal'in son çalıştığı daire | **5 (kuruldu ve zorlaştırıldı, #65):** banyo içeriden kilitli (menteşe, tornavida) → kilerdeki kol mutfağın sürgüsünü açar → salonun anahtarı kırık: başı buzdolabında, ucu Cemal'in alet çantasında, ikisi birleşince salon açılır (köşede "1/2") → gardırop ağır: levye (kilerde) + kaydırıcı (banyoda) → arkasında ana vana, çark çevrilince damla susar → lavabonun sifonu sökülür, ağzında kat anahtarı. Oda A ve Oda B kapalı | İlan bırak | Sessizlik, düzenli damla, kendi açılan sayaç | Alet çantası; **telefon 2:** "7. katı atla" |
| 6 | 3 ve 4 birlikte (çift daire, #70): 3 "kimse oturmuyor", sofra kurulmayı bekliyor | **6 (kuruldu, #66, #70):** sofrada dört isim kartı (Rıza Efendi, Hasan, Cemal, Yeni Kapıcı), üstlerinde isim yazan dört tabak iki daireye dağılmış, bulundukça kapıcının kucağında üst üste birikir (köşede "2/4" ve en üstteki tabağın ismi); sofrada en üstteki tabak konur, yanlışı geri alınınca yığının altına gider; doğru yerlere konunca büfe açılır, içinde kat anahtarı. İki daire birbirinin kilidini açar: 4'ün kileri şifreli (3'ün takviminde 1994), 3'ün mutfak anahtarı 4'ün buzdolabında, 3'ün yatak odası kırık anahtarlı (başı 4'ün kilerinde, ucu 3'ün mutfak çekmecesinde), 4'ün banyosu menteşeli, 4'ün yatak odası 3'ün kilerindeki kolla. Kaçış anı (#58) ayrı iş | Zil çal | KÂBUS: sofra, açılan asansör | Sofrada dört tabak: dört kapıcı |
| 7 | 1, Cemal'in eşyaları; bütün kapılar 13 | **7 (kuruldu, #67, #58):** sahanlıkta dört '13' kapısı, dördü kapalı; Cemal'in notu gerçeğini tarif eder (paspas yok, terlik yok, gözetleme deliğinden ışık); yanlış kapıyı çalmak bir olay (ses + kafa lambası titrer). Elektriksiz daire, yedi kilit: banyo (menteşe) → salon (şifre Cemal'in doğum günü 1107, nüfus cüzdanı banyoda) → büfede kiler anahtarı ve kırık anahtarın başı → kiler → kol → mutfak (kırık anahtarın ucu) → Oda A (fosforlu 0913) → Oda B → sandık (levye) → kat anahtarı. **Kaçış anı:** anahtar alınınca asansör çalar, 3 sn sonra kırmızı gözlü siluet oyuncunun yolunu izler; kat kapısına duraksamadan giden kaçar; yakalanınca **kat baştan başlar** (Şahan, 2026-09-27: kilitler ve parçalar sıfırlanır, oyun değil yalnızca kat; siluet 2,0 m/sn) | Yok: "atla" | En yoğun kat: aynı numara, aralık kapılar, tersten ağlama | Cemal'in son notu: "asansör beni bekliyor" |
| 8 | 4, Yönetim dairesi (dosyalar) | **8 (kuruldu, #68):** banyo (menteşe; yerde kiler anahtarı) → kiler (alet çantasında mutfak anahtarı, duvarda kol) → Oda B (kilerin kolu; sandıkta salon anahtarının başı) → mutfak (buzdolabında ucu) → salon (kırık anahtar; duvarda iskân ruhsatı: "Zemin + 8 kat, yapım yılı 1974") → Oda A, arşiv (şifre 1974; komodinde dosya dolabının anahtarı) → dosya dolabının üst çekmecesi (asansörün sigortası, Cemal'in notu; öbür çekmecelerde kapıcıların dosyaları ve dokuzuncu katın planı) → **asansör:** sigorta sahanlıktaki panoya takılır, düğmeyle çağrılır, gösterge "9" der, sonra "8"; kapılar açılınca kabinde **çatı anahtarı**. Anahtar alınınca asansör kapanıp "9"a döner. Çatı kapısı yalnızca bu anahtarla açılır | Çatı kapısını kilitle | Sessizlik; asansör "9" der ve gelir | Kapıcıların dosyaları, dokuzuncu katın planı; **final:** oyuncu çatıya çıkar, şafak; dış cephede dokuzuncu katın penceresi baştan yanıktır (çiviye asılan lamba, #108); asansörün "ding"i duyulunca pencerede bir siluet belirir (#117) |

Süre: kat başına 10-15 dk (zincir uzadıkça artar), açılış ve final 10 dk: ilk oynayış **~2 saat**.

## 8. Anomalilerin yeni rolü

- 66 kart olay kütüphanesidir; kart sistemi, zamanlı etkiler ve ses etkileri aynen kalır. Kartlar artık
  daire nesnelerini de hedefleyebilir (kod aynı, yol farklı).
- Seçim rastgele değil **senaryolu**: her katın sıralı olay listesi (kart + an) ve yoğunluğuna göre rastgele
  FISILTI dolgusu. İki oynayış aynı omurgayı, farklı ayrıntıları gösterir.
- Ceza yok. Bazı olaylar işi etkiler (karanlık kat sigortaya gönderir, kapı çarpması ayakkabıları dağıtır).
- **Defter (#45):** "Gördüklerin". Olay huzmeye girince ya da sesi çalınca işlenir. Tekrar oynama nedeni.
- Görünürlük denetimi, zorluk tavanı, "1. ve 8. kat hep normal" kuralları kalkar.

## 9. Tehdit ve kayıt

- Ölüm yok. **İki senaryolu kaçış anı** (6. ve 7. kat): yetişemeyen oyuncu karanlıkta o katın kapısında
  uyanır; çözülmüş kilitler çözülmüş kalır, elindeki eşya düşmez (kaybı 1-2 dakika). Şahan'ın seçimi.
- **Kayıt:** her kat kapısı geçilince ve dairede her kilit çözülünce otomatik. Oyun kapanıp açılınca kaldığı
  yerden. (Yeni ihtiyaç: eski turlar kısaydı, artık iki saat.)

## 10. Kapıcı odası: merkez

Açılış burada (var). Yeni: **pano** (bulunan notlar asılır), **çivi** (1. kat anahtarı, kafa lambası),
**telefon** (2. ve 5. katta çalar; zil sahanlıktan duyulur; açmak için inmek **isteğe bağlı** (Şahan'ın seçimi):
inen konuşmayı dinler, inmeyen mesajı sonra panoda not olarak bulur), **radyo**, **Cemal'in eşyaları**. Aşağı inmek serbest.

## 11. Ses ve müzik

- Müzik hiç susmaz; yavaş dalgalanır (#49), olaylarda kabarır (#40), katla yükselen taban.
- Yeni sesler: demir kapı kilidi ve gıcırtısı, asma kilit şifre tıkları, sigorta şalteri, menteşe sökme,
  telefon zili ve ahize, kapı altından kayan kâğıt, kapı arkasından konuşan kiracı, daire içi ortamı.
- Kiracı ve Yönetim sesleri: şimdilik yazı (kâğıt gibi); seslendirme sonra (#12 bütçesi).

## 12. Ne kalır, ne değişir, ne gider

| Sistem | Durum |
|---|---|
| Sahanlık, döngü ışınlanması, katlar üst üste | **Kalır**; kat kapısı ve açık daire eklenir |
| Anomali kartları, uygulayıcı, zamanlı etkiler, sesler | Uygulayıcı **kalır** (kat dosyasının senaryolu sahanlık kartı, 7. kat); 65 rastgele kart **gitti** (#59) |
| Anomali seçici (rastgele, zorluk) | **Gitti** (#59) |
| Karar kuralı (KatSayaci.KararVer), ceza, görev-eksik cezası | **Gitti** (#59, 2026-09-27) |
| Çöp, ayakkabı, kat olayları, TV kaydırması | **Kalır**; anomali defteri (#45) Şahan'ın kararıyla **rafa kalktı** (#59) |
| Gece aydınlatması | **Değişir:** karanlık bina + kafa lambası |
| Görünürlük denetimi | **Gitti** (#59): ölçeceği rastgele kart kalmadı |
| Kapıcı odası, final, dış cephe | **Kalır**, oda merkez olur |
| Test göstergesi | Kat, daire, zincir durumu ve olay listesi |
| **Yeni** | Kafa lambası, kat kapısı ve anahtar, daire üretici (3+1), kilit/eşya sistemi (4 tür), kat içerik verisi (8 kat), yeni görev türleri (sayaç, ilan, zil, ampul), kaçış anı, pano/telefon/sigorta, kayıt |

## 13. Uygulama sırası (onaydan sonra açılacak issue'lar)

1. Karanlık bina + kafa lambası (1 gün) — hemen hissedilir, görsel için de gerekli
2. Kilitli kat kapısı + anahtar + kayıt (3 gün)
3. Daire üretici: 3+1 plan, kapılar, temel mobilya (1 hafta)
4. Kilit/eşya sistemi: anahtar, şifre, sigorta, tornavida; tek eşya taşıma (3 gün)
5. Kat içerik verisi: kat başına daire döşemesi, zincir, görevler, olay listesi (kat başına 1 gün × 8)
6. Yeni görev türleri (2 gün), kapıcı odası merkezi (2 gün), kaçış anları (2 gün)
7. Karar kuralı ve cezanın kaldırılması, testlerin güncellenmesi (1 gün)
8. Yazı: notlar, telefon, Yönetim listeleri TR + EN (paralel, ~1 hafta)

Toplam **5-6 hafta.** Steam sayfası (18 Ekim) merdiven ve ilk daireyle çıkabilir; metin yeni öncüle göre (#12).
H5-7 içerik planı buna göre yeniden kurulur; #46 (geceler) **kalkar**, #47 (kiracı notları) ve #48 (gece
saati) bu belgenin parçası olur. Demo (H8-9) tarihini Kubilay'la yeniden konuşmak gerekir.

## 14. KAPICI çakışması

Steam'deki KAPICI de *Türk apartmanında kapıcıyla anlatı korkusu*; artık türde de yakınız. Ayrışma:
görev = ilerleme (oyuncu bir kapıcı gibi çalışır), kat kat büyüyen kilit zincirleri, 66 olaylık kütüphane ve
defter, karanlık + kafa lambası, 1994 dokusu. **İsim (#1) artık acil.**

## 15. Kalan açık kararlar

Final (c), kaçış bedeli (katın kapısında uyanma) ve telefon (isteğe bağlı) Şahan'ın seçimiyle kapandı;
yukarıdaki bölümlere işlendi.

1. **Kat kimlikleri (§7):** Şahan değişiklik istiyor; hangi katların ne olacağı bekleniyor.

## 16. Dokuzuncu kat (final bölümü) — Şahan'ın kararı, 2026-10-02

**Neden:** oyunun her parçası dokuzuncu kata işaret ediyor (ruhsatta olmayan kat, gizli plandaki 33-36 numaralı
daireler, personel dosyaları, Nuri'nin "dokuzuncu kattakiler"i, Cemal'in "asansör beni bekliyor" notu, 6. katın
dört tabaklı sofra daveti, "9"dan inen asansör, finaldeki yanan pencere) ama oyuncu orayı hiç görmüyordu. Bu bölüm o
borcu öder. Kararlar: **zorunlu** (seçim yok; bina çağırır), **bulmacasız** (4-6 dakikalık yürüyüş ve tek kaçış),
**lamba çiviye asılır**.

**Akış**
1. **Asansör gelir, anahtar kabinde değildir.** 8. katın zinciri sigorta → çağır ile biter. Oyuncu kabine girince
   kapılar kendiliğinden kapanır, gösterge 8'den 9'a geçer, kabin sarsılır; kapılar 1974'te açılır.
2. **Sahanlık, 1974.** Aynı sahanlık, ama yeni: boyası taze, mozaiği parlak, kapı numaraları 33-36. Merdiven iki yöne de
   **tuğlayla örülü** ("merdiven çıkmaz, asansör çıkar"). Elektrik yok; tek açık kapı, ışığı dışarıdan görülen
   pencerenin dairesi; yalnızca o dairenin salonu yanıyor. Takvim: Ekim 1974.
3. **Daire: binanın sakladıkları.** Aynı 3+1 plan. Holde üç çivi: Rıza Efendi'nin tespihi, Hasan'ın kasketi (çıkıp
   gidenlerin bıraktıkları) ve BOŞ üçüncü çivi. Salon kapısı kapalı. Oyuncu Cemal'in lambasını boş çiviye asınca
   (etkileşim "AS"; çividen alındığı gibi) salon kapısı kendiliğinden açılır. Bu bir kapı anıdır, bulmaca değil: holde
   etkileşilecek tek şey çividir ve parıldar. Odalarda öteki katlardan tanıdık eşyalar (Nuri'nin radyosunun ikizi
   çalıyor, Hasan'ın kamp yatağı, Cemal'in sandığı): burada model tekrarı bilerek vardır, tekrar hikâyenin kendisidir.
4. **Sofra.** Salonda 6. katın sofrası kurulu: dört tabak. Rıza'nın ve Hasan'ın tabakları boş, sandalyeleri geri
   itilmiş (kalkıp gitmişler). Cemal'in sandalyesinde Gölge OTURUYOR, sırtı dönük, önünde anahtarlığı. Dördüncü tabak
   oyuncunun: üstünde çatı anahtarı, sandalye çekilmiş. Kural yazısızdır: oturan kalmaz. Anahtar alınır, oturulmaz.
5. **Kaçış.** Anahtar alındığı an anahtarlık şıkırdar, salonun ışığı söner, Gölge kalkar. Oyuncunun kendi lambası
   çivide; yol yalnızca kabinin ışığıyla görülür. 7. katın kovalamacası (iz sürme), hedef asansör. Kabine girince
   kapılar kapanır, Gölge kapıya çarpar, kabin 8'e iner. Yakalanırsa bölüm kabinin açılışından yeniden başlar (anahtar
   tabağa, Gölge sandalyesine döner; kayıt değişmez).
6. **Çatı.** 8. katta kapılar açılır; merdivenle çatı kapısına, bugünkü final. Dış görüntü değişmez: dokuz kat ve yanan
   pencere. Oyuncu artık o pencerenin hangi oda olduğunu bilir ve ışığın kendi lambası olduğunu anlar: sofra hâlâ
   kurulu, bir sandalye hâlâ boş.

**Kurallar**
- Varlık tektir (Cemal); Rıza ve Hasan kurtulmuştur (dosyalar ve Hasan'ın telefonu). Gölge'nin anahtarlığı (#105)
  burada masada görünür.
- Dokuzuncu kat kayda GİRMEZ: oyun burada kapatılırsa 8. katın başından açılır (#84 kuralı). Gerilim kuralı ve
  takılma ipucu bu bölümde kapalıdır; tek tehdit sofradaki Gölge.
- Açık karar: final fotoğrafındaki "Yeni kapıcı, 1994" satırı olduğu gibi kalır (Şahan dokunmadı).

**Kurulan hâli (2026-10-02, #108)** — tasarımdan ayrılan ya da netleşen noktalar:
- Dokuzuncu kat sahnede ayrı bir settir (`Dokuzuncu_Kat`, x = -400; dış cephe gibi). Oyuncu oraya asansörle
  gider: 8. katın kabinine binince kapılar kapanır, kapılar kapalıyken dokuzuncu katın birebir aynı kabinine
  ışınlanır (`DokuzuncuKat.Yolculuk`), kapılar 1974'te açılır. Dönüş aynı yolla.
- Daire 34 (sol-uzak daire). Açık odalar: antre, hol, salon (lamba asılınca), Oda A (Nuri'nin radyosunun ikizi
  komodinin üstünde çalar), Oda B (Hasan'ın kamp yatağı, Cemal'in sandığı). Banyo, mutfak, Oda C, balkon kilitli.
  Takvim "EKİM 1974" salonda. Kaplamalar 6. katın (1974 evi) duvar kâğıdı ve parkesi; leke yok.
- **Çivideki lamba yanık kalır.** Anahtar alınınca salonun ışığı söner, çividekini bina söndürmez (titrer ve
  kalır): kaçışın yolu holdeki o ışıkla ve kabinin ışığıyla görülür, ve finaldeki pencere ışığı budur. Asılan
  lamba geri alınamaz; oyuncu çatıya lambasız çıkar (8. katın sahanlık ampulleri yanar).
- Sofra: 1 Cemal (Gölge oturur, sırtı kapıya), 2 oyuncu (sandalye çekilmiş, anahtar tabakta), 3 Rıza ve 4 Hasan
  (tabaklar boş, sandalyeler geri itilmiş); isim kartları 6. kattaki gibi. Gölge'nin anahtarlığı tabağının önünde.
- Kovalamaca 7. katın `KacisAni`'sidir (oturan Gölge kalkar, 2,5 sn sonra izi sürer, 2,0 m/sn). Kaçış: kabine
  girince kapılar kapanır, Gölge kapıya iki kez vurur. Yakalanma: kararma, bölüm kabinin açılışından yeniden
  (anahtar tabağa, Gölge sandalyeye, lamba oyuncuya); kayıt değişmez, 8. katın zinciri de bozulmaz.
- 8. katın zincirinde artık kat anahtarı YOK: kural (`KilitZinciri`) asansör kilidi çözülünce zinciri bitmiş
  sayar. T listesinde çatı anahtarı görünmez.
- #109'da yapıldı: sahanlık 1974'te yeni (temiz beyaz sıva, cilalı mozaik, yeni vernikli kapılar, parlak pirinç;
  duvarların alt 1,15 m'si çağla yeşili parlak yağlı boya ve koyu bordür — `GrayboxKurucu.DokuzuncuKatiTazele`,
  Dokula'dan sonra ad eşlemesiyle `Taze_*` malzemeler); tespih ve kasket yeniden modellendi; kabin yolculuğunda
  gösterge bir an boş kalır ve kabin lambası titrer. Boş çivi fenerin ışığında eşyalar gibi parıldar
  (`FenerParlamasi`, eşyasız; sabit 3,5 m), lamba asılınca söner. Gölge'nin kolu dirsekten iki parça
  (`Kol_*/OnKol_*`); otururken ön kol masaya yatar, yürürken dirsekten de salınır.
