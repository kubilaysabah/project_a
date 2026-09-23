# Asset alışveriş listesi (#9)

**Kural:** ücretsiz araçlar + toplam asset harcaması **≤ 100 $**. Onaysız hiçbir satın alma yok;
satın almayı geliştirici yapar (Claude ödeme yapmaz, hesaba girmez).

**Araştırma tarihi:** 2026-09-23. Fiyatlar mağaza sayfasından okundu; **vergi/KDV hariç** (Asset Store
"Taxes/VAT calculated at checkout" diyor). Satın almadan önce fiyat ve indirim yeniden kontrol edilmeli.

---

## 1. İhtiyaç listesi: oyunda ne görünecek?

| Parça | Nereden | Not |
|---|---|---|
| Duvar, döşeme, tavan dokuları (sıva, mozaik, karo, beton) | **Satın alınan paket** veya CC0 doku | Sahanlık + çekirdek |
| Merdiven, basamak, korkuluk/tırabzan | **Satın alınan paket** | Yarım dönüşlü çekirdeğe oturmalı |
| Daire kapıları (4), kasa, kol, numaralık | Paket + Blender düzeltme | Menteşe/kanat yapısı korunmalı (anomali yolları) |
| Asansör kapısı, kasa, gösterge, düğme | **Blender** | Paketlerde 70'ler asansörü yok |
| Pencere (ara sahanlık) | Paket | |
| Lamba/ampul duyu | Paket | |
| Elektrik sayacı / pano | Paket | İki adayda da var |
| Posta kutuları (bina girişi / ileride) | Paket | Soviet kitte var |
| Çöp poşeti | Paket veya Blender | Abandoned Stairwell'de var |
| **Türk kimliği:** tüp, terlik, ayakkabılık, paspas, saksı, 1994 gazetesi, isimlik, yönetim ilanı | **Blender + doku** | SATIN ALINMAZ, üretilir (CLAUDE.md) |
| #8'den gelen eşyalar: tek erkek ayakkabısı, sayaç kapağı, ayak izi çıkartması, gizli 5. kapı | Blender / decal | |
| İnsan figürü (K03, K08, T22) | **Mixamo** (ücretsiz) | Sırtı dönük duruş için animasyonsuz poz yeter |
| Ambiyans ve efekt sesleri | Ücretsiz / CC kaynaklar | Asıl iş #14 |

---

## 2. Sanat yönü kararı (önce bu seçilmeli)

Araştırma iki ayrı yol gösterdi. Hangisi seçilirse paketler ona göre alınır; ikisini karıştırmak kötü görünür.

| | **A — Gerçekçi (PBR)** | **B — PSX / düşük çözünürlük** |
|---|---|---|
| Görünüm | 1994 apartmanını gerçeğe yakın gösterir | PS1 dönemi: düşük poligon, pikselli doku, titreyen görüntü |
| Kaynak | Unity Asset Store | itch.io |
| Maliyet | ~50 $ | ~20 $ |
| Artısı | Konseptteki "gerçek bir apartman" hissi; anomaliler ince detaylara dayanabilir | Çok ucuz, indie korkuda tutan bir tarz, eksik detayları "stil" affeder |
| Eksisi | Kendi Blender eşyalarımız paketin kalitesine yetişmeli | FISILTI (çok ince) anomaliler düşük çözünürlükte zor okunur; konseptten sapma |

**Öneri: A.** Oyunun çekirdeği "her kat aynı, farkı bul"; 35 FISILTI fikrinin çoğu küçük detaya dayanıyor
(yamuk ilan, eksik kapı gözü, kısa kapı kanadı). Düşük çözünürlük bu ayrımı öldürür.

---

## 3. Adaylar — Unity Asset Store (Yol A)

Hepsi **Standard Unity Asset Store EULA** (oyunda ticari kullanım serbest, varlığın kendisi ayrıca satılamaz).

| Paket | Fiyat | URP | Unity | İçerik (özet) | Uygunluk |
|---|---|---|---|---|---|
| **Soviet Modular Apartment Kit** (Public Void Interactive) | **30 $** | ✅ (yalnız URP) | 6000.3 | 70'ler Kruşçevka apartmanı: duvar/pencere/kapı modülleri, **merdiven boşluğu parçaları**, boru/kalorifer, dönem elektrik panosu, 17 gerçek apartmandan alınmış doku, 34 decal (**daire numaraları**, sıva/boya hasarı), lamba + ampul duyu, posta kutuları. 326 MB, sürüm 1.0 (Ağu 2026) | ⭐ En iyi eşleşme: aynı dönem, aynı sosyal konut dili |
| **Abandoned Stairwell** (AK Studio Art) | **19,99 $** | ✅ | 2022.3 | Çok katlı merdiven boşluğu modülleri, beton/karo malzemeler, **metal korkuluk**, floresan lamba, elektrik panosu, **çöp poşeti** varyasyonları, depo kapısı. 178 MB (Haz 2026) | ⭐ Korkuluk + poşet + kirli dokular; ilk paketin eksiğini kapatır |
| Dirty Apartments (Triplebrick) | 35 $ | ✅ | 6000.0 | 5 katlı bina demo sahnesi, low-poly (200-1200 üçgen), 6 duvar kâğıdı, karo stilleri, 20 mobilya, 133 prefab. 55 değerlendirme | Yedek: mobilya ağırlıklı, bizim daireler kapalı |
| Horror - modular interior and props (Mixall) | 29,99 $ | ✅ | 2021.3 | Modern iç mekân, açılan kapı/çekmeceler | ✗ Modern daire içi, bize lazım değil |
| Apartment Kit (Brick Project Studio) | Ücretsiz | ✗ yalnız Built-in | 2020.3 | 200+ prefab | ✗ URP değil; dönüştürme uğraşı |

## 4. Adaylar — itch.io (Yol B, karşılaştırma için)

| Paket | Fiyat | İçerik |
|---|---|---|
| PSX Mega Pack II (pizzadoggy) | 11,97 € (indirimli) | Half-Life 2 esintili modüler iç/dış mekân + eşya |
| PSX Textures / II (pizzadoggy) | 4,97 € / 5,95 € | Retro dokular |
| Final Exam - PSX Survival Horror Pack | 7,99 £ | 90'lar survival-horror eşyaları |
| PSX Industrial Environment | Ücretsiz | Endüstriyel modüller |

itch.io lisansları paket paket değişir; B seçilirse her birinin sayfasındaki lisans ayrıca okunmalı.

---

## 5. Ücretsiz kaynaklar

| Kaynak | Ne için | Lisans |
|---|---|---|
| **ambientCG** | Mozaik, karo, sıva, beton dokuları | CC0 (atıf gerekmez, ticari serbest) |
| **Poly Haven** | Doku + HDRI | CC0 |
| **Mixamo** (Adobe) | İnsan figürü, poz/animasyon | Oyunlarda ticari kullanım telifsiz; Adobe hesabı gerekir |
| **Blender** (kurulu, 5.2.1) | Türk eşyaları, asansör | Ürettiğimiz her şey bizim |
| **Sonniss GDC Game Audio Bundle** | Ambiyans + efekt | Telifsiz, ticari serbest, atıf gerekmez |
| **Freesound.org** | Tek tek sesler (zil, boru, bebek, TV mırıltısı) | **Dosya dosya değişir** (CC0 / CC-BY / CC-BY-NC). **NC olanlar kullanılamaz** (ticari oyun) |

---

## 6. Önerilen sepet (Yol A)

| Kalem | Fiyat |
|---|---|
| Soviet Modular Apartment Kit | 30,00 $ |
| Abandoned Stairwell | 19,99 $ |
| **Toplam** | **49,99 $** (+ vergi) |
| **Kalan bütçe** | **~50 $** — art pass sırasında çıkan eksik için yedek (örneğin bir ses paketi veya figür). Şimdiden harcanmıyor. |

## 7. Satın almadan önce netleşmesi gerekenler

1. **Sanat yönü:** A mı, B mi? (Öneri A.)
2. **Kubilay'ın onayı:** bütçe ortak karar (CLAUDE.md).
3. **Lisans türü "Single Entity":** tek bir kişi veya kuruluş için. İki geliştirici aynı çatı altında (aynı şirket / aynı ekip hesabı) sayılıyorsa yeter; değilse paketin kimin hesabından alınacağı ve ikinci kişinin nasıl erişeceği Kubilay'la netleşmeli.
4. **#21:** Kubilay'ın makinesinde LFS doğrulanmadan paketler depoya eklenmemeli (ikili dosyalar ham girer).
5. **Depo boyutu:** iki paket ~500 MB. Paketin tamamı değil, yalnızca kullanılan parçalar depoya girmeli; kullanılmayan kısım silinir veya `Assets/_Paketler` gibi bir klasörde tutulup ayrıca karar verilir.
