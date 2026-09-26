# Huzur Apartmanı — Hikâye ve İlerleme Tasarımı (taslak 2, #50)

**Durum:** Şahan'la konuşularak yazıldı (2026-09-26); onayını bekliyor. Onaylanana kadar hiçbir
mevcut sistem silinmez. Kubilay'ı Şahan bilgilendirecek; Steam konumlandırması ve isim (#1) onun alanı.

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
  Düşük kalitede gölgesiz). **F / kolun sol omzu** ile açılıp kapanır (karanlıkta durup dinlemek, saklanmak).
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

Dört tür, her karışımı taşır; oyuncu kuralları ilk katlarda öğrenir:

| Tür | Nasıl çözülür | İlk görüldüğü kat |
|---|---|---|
| **Anahtar** | Dairede bir yerde bulunur, tek kapıyı açar | 1 |
| **Şifreli asma kilit** | 4 haneli şifre odadaki bir şeyde: takvim, boy cetveli, fatura, doğum günü | 2 |
| **Sigorta / elektrik** | Sigorta atık; sigorta teli bulunup takılınca ışık ve elektrikli şeyler çalışır | 4 |
| **Tornavida ile menteşe** | Kilidi açılmayan kapının menteşeleri sökülür; tornavida dairede | 5 |

Zincir yükselişi: **1-1-2-2-2-3-3-3** (kat başına kilitli oda sayısı).

## 7. Kat kat taslak

| Kat | Daire, kimin | Kilitli odalar ve zincir | Sahanlık işi | Olaylar | Hikâye |
|---|---|---|---|---|---|
| Zemin | Kapıcı odası | — | Notu oku, kafa lambasını al, 1. kat anahtarı çivide | — | Cemal'in eşyaları, yarım not |
| 1 | 2, taşınmış aile (kutular) | **1:** Oda A anahtarı mutfakta kavanozda ("anahtarlar kavanozda" notu) → Oda A komodininde kat anahtarı | Çöp, ampul değiştir | Yalnız ortam; ilk FISILTI | Yönetim'in tahliye yazısı |
| 2 | 3, hastanedeki yaşlı (Nuri) | **1:** Oda C asma kilitli; şifre duvar takviminde işaretli tarih → kat anahtarı kilerde | Sayaç oku, ilan bırak | Radyo kendi açılır, terlik yürür | Rıza Efendi'nin fotoğrafı; **telefon 1** |
| 3 | 1, çocuklu aile (gece yoklar) | **2:** Oda B anahtarı salonda oyuncak kutusunda → Oda B'deki dolap asma kilitli, şifre boy cetvelindeki yıllar → dolapta kat anahtarı | Ayakkabıları rafa koy | İçeriden bebek, tavandaki terlik | Çocuğun çizimi: "dokuzuncu kattaki adam" |
| 4 | 4, boş ve elektriksiz | **2:** Sigorta teli mutfak çekmecesinde → sigorta takılınca buzdolabı ışığı; Oda B anahtarı buzdolabında → Oda B'de kat anahtarı | Çöp | Karanlık kat, göz kırpan ışık, kafa lambası titrer | Hasan'ın notu |
| 5 | 2, Cemal'in son çalıştığı daire | **2:** Banyo içeriden kilitli → tornavida salondaki alet çantasında (Cemal'in) → banyoda Oda A anahtarı → Oda A'da kat anahtarı | İlan bırak | Sessizlik, düzenli damla, kendi açılan sayaç | Alet çantası; **telefon 2:** "7. katı atla" |
| 6 | 3, "kimse oturmuyor" ama sofra kurulu | **3:** Anahtar + şifre + sigorta; **kaçış anı 1** (koridordaki şey, salona sığın) | Zil çal | KÂBUS: sofra, açılan asansör | Sofrada dört tabak: dört kapıcı |
| 7 | 1, Cemal'in eşyaları; bütün kapılar 13 | **3:** Tornavida + anahtar + şifre (Cemal'in doğum günü); **kaçış anı 2** (kat kapısına koş) | Yok: "atla" | En yoğun kat: aynı numara, aralık kapılar, tersten ağlama | Cemal'in son notu: "asansör beni bekliyor" |
| 8 | 4, Yönetim dairesi (dosyalar) | **3:** Dosya dolabı anahtarı → şifre: binanın yapım yılı → sigorta: asansör panosu → **çatı anahtarı** | Çatı kapısını kilitle | Sessizlik; asansör "9" der ve gelir | Kapıcıların dosyaları, dokuzuncu katın planı; final |

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

- Ölüm yok. **İki senaryolu kaçış anı** (6. ve 7. kat): yetişemeyen oyuncu karanlıkta o katın başında uyanır
  (kaybı 1-2 dakika).
- **Kayıt:** her kat kapısı geçilince ve dairede her kilit çözülünce otomatik. Oyun kapanıp açılınca kaldığı
  yerden. (Yeni ihtiyaç: eski turlar kısaydı, artık iki saat.)

## 10. Kapıcı odası: merkez

Açılış burada (var). Yeni: **pano** (bulunan notlar asılır), **çivi** (1. kat anahtarı, kafa lambası),
**telefon** (2. ve 5. katta çalar; oyuncu odaya inmeden de sahanlıkta duyar, açmak için iner: isteğe bağlı,
inmezse mesaj panoda not olarak belirir), **radyo**, **Cemal'in eşyaları**. Aşağı inmek serbest.

## 11. Ses ve müzik

- Müzik hiç susmaz; yavaş dalgalanır (#49), olaylarda kabarır (#40), katla yükselen taban.
- Yeni sesler: demir kapı kilidi ve gıcırtısı, asma kilit şifre tıkları, sigorta şalteri, menteşe sökme,
  telefon zili ve ahize, kapı altından kayan kâğıt, kapı arkasından konuşan kiracı, daire içi ortamı.
- Kiracı ve Yönetim sesleri: şimdilik yazı (kâğıt gibi); seslendirme sonra (#12 bütçesi).

## 12. Ne kalır, ne değişir, ne gider

| Sistem | Durum |
|---|---|
| Sahanlık, döngü ışınlanması, katlar üst üste | **Kalır**; kat kapısı ve açık daire eklenir |
| Anomali kartları, uygulayıcı, zamanlı etkiler, sesler | **Kalır** (olay kütüphanesi) |
| Anomali seçici (rastgele, zorluk) | **Değişir:** kat senaryosu + dolgu |
| Karar kuralı (KatSayaci.KararVer), ceza, görev-eksik cezası | **Gider** |
| Çöp, ayakkabı, kat olayları, TV kaydırması, defter | **Kalır** (defterin koşulu değişir) |
| Gece aydınlatması | **Değişir:** karanlık bina + kafa lambası |
| Görünürlük denetimi | Araç kalır, ölçüt olmaktan çıkar |
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

1. **Final:** 8. katta asansör "9" der ve gelir. Oyuncu (a) çatıya çıkar, mevcut final oynar (şafak, dokuz katlı
   cephe); (b) asansöre biner, kapılar kapanır, jenerik; (c) ikisi de: çatıya çıkar, dış cephe planında
   dokuzuncu katın ışığı yanar ve asansörün geldiği duyulur. Öneri: **c**.
2. Kaçış anlarında yakalanınca "o katın başında uyanma" yeterli mi, yoksa küçük bir bedel (kat anahtarı
   yeniden bulunur) olsun mu?
3. Telefon: açmak için odaya inmek isteğe bağlı mı (öneri: evet), zorunlu mu?
4. 8 katın kimliklerini (taşınmış aile, yaşlı, çocuklu aile…) böyle mi kuralım? Değiştirmek istediğin kat var mı?
