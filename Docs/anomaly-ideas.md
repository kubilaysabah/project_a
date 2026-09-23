# Anomali fikirleri (#8)

Oyunun içerik hammaddesi. Her satır bir anomali fikri; buradan seçilenler
`Assets/Resources/Anomaliler/` altına kart olarak dökülür (#13).

**Oturum:** 2026-09-23 · **Toplam:** 67 fikir (35 FISILTI · 22 TEKİNSİZ · 10 KÂBUS) · 10'u oyunda.

## Sütunlar

- **Hedef / değişiklik:** kartın dokunduğu nesne yolu (sahanlığa göre) ve ne yaptığı.
- **Algı:** 👁 göz · 👂 kulak.
- **Zorluk:** 1 (hemen görülür) – 5 (çok ince). Kat ilerledikçe tavan yükselir (`AnomaliSecici.ZorlukTavani`).
- **Yapım:**
  - **K** = sadece kart. Bugün, sıfır kodla yapılabilir.
  - **E** = önce sahneye yeni bir eşya eklenmeli (sonrası yine sadece kart).
  - **Y** = yeni kod gerekir (ses sistemi, animasyon, çekirdeğe dokunan kart, senaryolu olay).
- **Durum:** ✅ oyunda · 🎨 graybox beyazken okunmaz, art pass (#10) sonrası · ⭐ sıradaki 10'luk (#13), onaylı.

## Kurallar (bu listeyi yazarken uyulanlar)

- **Çöp poşetleri hedef olamaz**: sayıları rastgele, oyuncu farkı bilemez (bütünlük testi yasaklıyor).
- **8. kat hep normal**, 1. kat varsayılan olarak normal. Ters durumlar için fikir yazılmadı.
- Graybox tamamen beyaz: **şekil, konum, varlık** değişiklikleri okunur; **yüzey rengi** okunmaz. Işığın rengi okunur (beyaz kutuları boyar).
- Simetrik bir kutuyu 180° çevirmek görünmez; bu tür fikirler 🎨 işaretli.
- Kartlar şu an yalnızca **sahanlığı** hedefleyebilir. Merdiven çekirdeğindeki pencere ve merdiven ampulü için uygulayıcının genişlemesi gerekir (Y).

---

## FISILTI — ince farklar (35)

| # | Ad | Hedef / değişiklik | Algı | Zorluk | Yapım | Durum |
|---|---|---|---|---|---|---|
| F01 | Ters kapı numarası | `Kapi_4/Mentese/Numara` Z 180° | 👁 | 4 | K | ✅ |
| F02 | Tek terlik | `Kapi_1/Terlikler/Terlik_Sag` gizli | 👁 | 3 | K | ✅ |
| F03 | Kaymış paspas | `Kapi_2/Paspas` duvardan ayrılmış, 17° dönük | 👁 | 3 | K | ✅ |
| F04 | Dönmüş ayakkabılık | `Ayakkabilik` 34° dönük | 👁 | 3 | K | ✅ |
| F05 | Eksik ilan | `Ilan` gizli | 👁 | 3 | K | ✅ |
| F06 | Kaymış zil | `Kapi_3/Zil` 45 cm aşağıda | 👁 | 4 | K | ✅ |
| F07 | Eksik ARIZALI notu | `Asansor/Not` + `Not_Yazi` gizli | 👁 | 2 | K | ✅ |
| F08 | Yer değiştirmiş numaralar | `Kapi_1` ile `Kapi_2` numaraları takas | 👁 | 4 | Y (kata göre metin) | |
| F09 | Tekrarlanan numara | `Kapi_3` numarası `Kapi_4` ile aynı | 👁 | 3 | Y (kata göre metin) | |
| F10 | Eksik numara plakası | `Kapi_2/Mentese/Numaralik` + `Numara` gizli | 👁 | 2 | K | ⭐ |
| F11 | Boş plaka | `Kapi_1/Mentese/Numara` metni boş | 👁 | 3 | K | |
| F12 | Asansör göstergesi çalışıyor | `Asansor/Yazi` "-" yerine sabit bir sayı ("5") | 👁 | 3 | K | ⭐ |
| F13 | Çağrı düğmesi yok | `Asansor/Cagri_Dugmesi` gizli | 👁 | 3 | K | |
| F14 | Eksik sayaç | `Elektrik_Sayaci` gizli | 👁 | 3 | K | ⭐ |
| F15 | Yan yatmış sayaç | `Elektrik_Sayaci` Z 90° | 👁 | 3 | K | |
| F16 | Zil öbür yanda | `Kapi_4/Zil` kapının menteşe tarafında | 👁 | 4 | K | |
| F17 | Eksik paspas | `Kapi_3/Paspas` gizli | 👁 | 2 | K | |
| F18 | Büyümüş paspas | `Kapi_1/Paspas` ölçek ×1,6 | 👁 | 3 | K | |
| F19 | Yukarıdaki kapı kolu | `Kapi_2/Mentese/Kol` 50 cm yukarıda | 👁 | 4 | K | ⭐ |
| F20 | Eksik kapı kolu | `Kapi_4/Mentese/Kol` gizli | 👁 | 3 | K | |
| F21 | Eksik kapı gözü | `Kapi_3/Mentese/Kapi_Gozu` gizli | 👁 | 5 | K | |
| F22 | Yamuk ilan | `Ilan` Z 12° | 👁 | 5 | K | |
| F23 | Yere düşmüş ilan | `Ilan` yerde, yatık | 👁 | 3 | K | |
| F24 | Yer değiştirmiş ayakkabılık | `Ayakkabilik` sol duvara geçmiş | 👁 | 3 | K | |
| F25 | Kaymış ampul | `Ampul` köşeye kaymış; gölgeler ve aydınlık kayar | 👁 | 4 | K | |
| F26 | Kısa kapı kanadı | `Kapi_1/Mentese/Kanat` boyu %95, altta aralık | 👁 | 5 | K | |
| F27 | Ters terlikler | `Kapi_2/Terlikler` 180° (burunlar kapıya) | 👁 | 4 | K | 🎨 |
| F28 | Sağa dönük tek terlik | `Kapi_4/Terlikler/Terlik_Sol` 90° | 👁 | 4 | K | |
| F29 | Kapının önünde tüp | Normalde olmayan bir tüp `Kapi_2` önünde | 👁 | 3 | E (tüp) | |
| F30 | Eksik saksı | `Kapi_1` yanındaki saksı yok | 👁 | 3 | E (saksı) | |
| F31 | Kapı önünde gazete | `Kapi_3` paspasında 1994 tarihli gazete | 👁 | 4 | E (gazete) | |
| F32 | Sallanan ampul | `Ampul` hafifçe sallanıyor | 👁 | 4 | Y (animasyon) | |
| F33 | Pencerede kayıt yok | `Pencere/Kayit` gizli | 👁 | 4 | Y (çekirdek hedefi) | |
| F34 | Uzak TV susmuş | Normal ambiyanstaki uzak TV sesi yok | 👂 | 3 | Y (ses sistemi) | |
| F35 | Boru tıkırtısı ritimli | Rastgele boru tıkırtısı düzenli bir ritimde | 👂 | 5 | Y (ses sistemi) | |

## TEKİNSİZ — fark edilince ürperten (22)

| # | Ad | Hedef / değişiklik | Algı | Zorluk | Yapım | Durum |
|---|---|---|---|---|---|---|
| T01 | Yeşil ampul | `Ampul` ışığı hastane yeşili | 👁 | 1 | K | ✅ |
| T02 | Aralık kapı | `Kapi_3/Mentese` +22° (içeri) | 👁 | 2 | K | ✅ |
| T03 | Sönük ampul | `Ampul` şiddet düşük | 👁 | 2 | K | ✅ |
| T04 | Göstergede 9 | `Asansor/Yazi` = "9"; bina 8 katlı (finaldeki 9. kat ipucu) | 👁 | 2 | K | ⭐ |
| T05 | Aralık asansör | `Asansor/Kapi_Sol` + `Kapi_Sag` 15'er cm açık; içi karanlık kuyu | 👁 | 2 | K | ⭐ |
| T06 | Bütün kapılar aralık | 4 kapının menteşesi +15° | 👁 | 1 | K | |
| T07 | Ardına kadar açık kapı | `Kapi_1/Mentese` +80°; karanlık hol görünür | 👁 | 1 | K | ⭐ |
| T08 | İçeri yürüyen terlikler | `Kapi_4/Terlikler` kapının eşiğinde, yarısı içeride | 👁 | 3 | K | |
| T09 | Kırmızı ampul | `Ampul` ışığı koyu kırmızı | 👁 | 2 | K | |
| T10 | Duvara dikilmiş paspas | `Kapi_3/Paspas` dik, duvara yaslı | 👁 | 2 | K | ⭐ |
| T11 | Devrilmiş ayakkabılık | `Ayakkabilik` yan yatmış | 👁 | 2 | K | ⭐ |
| T12 | 13 numara | Bir kapının numarası "13" (sabit metin) | 👁 | 3 | K | |
| T13 | Tavana yapışık terlik | `Kapi_2/Terlikler/Terlik_Sol` tavanda | 👁 | 2 | K | |
| T14 | İlan "ÇIKMAYIN" | İlanda normalde duyuru; bu katta tek kelime | 👁 | 2 | E (ilana yazı) | |
| T15 | Kapı önünde tek ayakkabı | Hiç kimsenin olmayan tek bir erkek ayakkabısı | 👁 | 3 | E (ayakkabı) | |
| T16 | Ayak izleri | Islak ayak izleri asansörden bir kapıya | 👁 | 2 | E (decal) | 🎨 |
| T17 | Sayaç kapağı açık | Elektrik sayacının kapağı açık, içi karanlık | 👁 | 3 | E (kapak) | |
| T18 | Mırıltı susmuş | Kapıların ardındaki uğultu bu katta yok | 👂 | 3 | Y (ses sistemi) | |
| T19 | Kapı gözünde ışık | Bir kapının gözünden ışık sızıyor, sonra kararıyor | 👁 | 3 | Y (ışık + zamanlama) | |
| T20 | Merdiven ampulü sönük | `Ampul_Merdiven` sönük | 👁 | 3 | Y (çekirdek hedefi) | |
| T21 | Yanıp sönen ampul | `Ampul` titriyor | 👁 | 2 | Y (animasyon) | |
| T22 | Pencerede silüet | Ara sahanlık penceresinde dışarıda biri | 👁 | 2 | Y + E (figür) | |

## KÂBUS — nadir, senaryolu (10)

| # | Ad | Hedef / değişiklik | Algı | Zorluk | Yapım | Durum |
|---|---|---|---|---|---|---|
| K01 | Bütün kapılar aynı numara | 4 kapının numarası aynı | 👁 | 1 | K | ⭐ |
| K02 | Beşinci kapı | Duvarda olmaması gereken 5. bir kapı | 👁 | 1 | E (gizli `Kapi_5`) | |
| K03 | Sırtı dönük adam | Asansörün önünde sırtı dönük biri duruyor | 👁 | 1 | Y + E (figür) | |
| K04 | Zil kendiliğinden çalıyor | Oyuncu sahanlığa varınca bir zil çalar | 👂 | 1 | Y (ses) | |
| K05 | "Geldim" | Bir kapının ardından "geldim" diye bir ses | 👂 | 1 | Y (ses) | |
| K06 | Ters ağlayan bebek | Normal ambiyanstaki bebek ağlaması tersten | 👂 | 3 | Y (ses) | |
| K07 | Açılan asansör | Asansör kapısı açılır, içi ışıklı ve boş | 👁👂 | 1 | Y (animasyon + ses) | |
| K08 | Gözden bakan yüz | Merdiven gözünden aşağıda yukarı bakan bir yüz | 👁 | 2 | Y + E (figür) | |
| K09 | Karanlık kat | Sahanlık tamamen karanlık, yalnız asansör göstergesi yanıyor | 👁 | 1 | E (gösterge ışığı) | |
| K10 | Göz kırpan ışık, açık kapı | Işık bir an söner; yandığında bir kapı ardına kadar açık | 👁 | 1 | Y (zamanlama) | |

---

## Özet: ne gerekiyor

**Sadece kartla yapılabilecek (K):** 40 fikir (10'u yapıldı). Bu havuz #13'ün büyük kısmını kod yazmadan doldurur.

**Sahneye eklenecek eşyalar (E) → #9 alışveriş / Blender listesi:**
tüp, saksı, 1994 gazetesi, tek erkek ayakkabısı, ayak izi çıkartması (decal), sayaç kapağı,
ilan üstüne yazı, gizli beşinci kapı, asansör göstergesi ışığı, insan figürü (K03, K08, T22).

**Yazılacak kod (Y):**
1. **Ses/ambiyans sistemi** (#14): F34, F35, T18, K04, K05, K06. Konseptin en güçlü korku aracı.
2. **Çekirdeği hedefleyen kartlar**: uygulayıcı şu an yalnızca sahanlığa dokunuyor (F33, T20, T22).
3. **Zamanlı/animasyonlu etkiler**: sallanan ve titreyen ampul, kapı gözü ışığı (F32, T19, T21).
4. **Senaryolu KÂBUS olayları**: tetikleyiciyle oynayan kısa sahneler (K03, K07, K10).

## Sıradaki 10'luk üretim sırası (#13) — Şahan onayladı, 2026-09-23

Hepsi **sadece kart** (sıfır kod), katman ve zorluk dağılımı dengeli, yeni sahanlığın asansörünü ve dört kapısını kullanıyor:

| Sıra | # | Ad | Katman | Zorluk |
|---|---|---|---|---|
| 1 | F10 | Eksik numara plakası | FISILTI | 2 |
| 2 | F14 | Eksik sayaç | FISILTI | 3 |
| 3 | F12 | Asansör göstergesi çalışıyor | FISILTI | 3 |
| 4 | F19 | Yukarıdaki kapı kolu | FISILTI | 4 |
| 5 | T07 | Ardına kadar açık kapı | TEKİNSİZ | 1 |
| 6 | T11 | Devrilmiş ayakkabılık | TEKİNSİZ | 2 |
| 7 | T10 | Duvara dikilmiş paspas | TEKİNSİZ | 2 |
| 8 | T05 | Aralık asansör | TEKİNSİZ | 2 |
| 9 | T04 | Göstergede 9 | TEKİNSİZ | 2 |
| 10 | K01 | Bütün kapılar aynı numara | KÂBUS | 1 |

Not: kart metni **sabittir**. Kapı numaraları her katta yeniden yazıldığı için bir kart numarayı yalnızca
sabit bir metne çevirebilir (F12 "5", T04 "9", T12 "13", K01 dört kapıda aynı sayı); uygulayıcının sırası
(geri al → numara yaz → kart) bunu destekliyor. Numarayı **kata göre** değiştiren fikirler (F08 takas, F09
kopya) için kartın "o katın numarası" gibi bir şablon okuyabilmesi gerekir → Y.
