# Steam yapay zekâ içerik beyanı (#118)

Steam, yükleme formunda ("AI Generated Content Disclosure") oyunda yapay zekâ ile üretilmiş ya da
yapay zekâ yardımıyla yapılmış içeriğin beyan edilmesini ister. İki başlık vardır:

- **Pre-generated (önceden üretilmiş):** geliştirme sırasında yapay zekâ araçlarıyla üretilen her tür içerik
  (görsel, ses, metin, kod). Oyunun içinde sabit durur.
- **Live-generated (oyun sırasında üretilen):** oyun çalışırken yapay zekânın ürettiği içerik. **Bu oyunda yok.**

Beyan edilmezse inceleme reddedilebilir; yanlış beyan Steam hesabını tehlikeye atar. Beyan metni mağaza
sayfasında herkese görünür, o yüzden açık ve kısa yazılır.

## Bu oyunda yapay zekâ nerede kullanıldı

| İçerik | Araç | Nasıl | Belge |
|---|---|---|---|
| Dairelerdeki aile fotoğrafları (9 görsel) | Stable Diffusion XL 1.0 (ComfyUI, Şahan'ın makinesinde, çevrimdışı) | Üretildi, sonra Blender betiğiyle eskitildi ve bozuldu; gerçek kişi yok | `Art/Fotograflar/LISANSLAR.md` |
| Oyun metinleri (notlar, telefon, radyo, menü; TR/EN) | Claude (Anthropic) ile birlikte yazıldı | Şahan karar verdi ve onayladı; Claude taslakları yazdı | `Docs/hikaye-metinleri.md` |
| Çeviriler (DE, ES, PT-BR, RU, ZH, JA — #123) | Claude ile çevrildi | Türkçe/İngilizce asıl metinlerden; insan düzeltmesi yok (jenerikte "Çeviriler: Claude ile" satırı var) | `Assets/Resources/Metinler.txt` |
| Kod ve Blender prop betikleri | Claude ile birlikte yazıldı | Şahan'ın tasarım kararlarıyla, Claude tarafından yazılıp Şahan'ın Editöründe doğrulandı | `CLAUDE.md` |
| Sesler | **Yapay zekâ yok**: gürültüden sentez (Python betikleri) ve CC0/CC BY kayıtlar; sesler hiç sentezlenmedi (Şahan okur) | | `Docs/ses-katalogu.md`, `Art/Ses/kayitlar/LISANSLAR.md` |
| Dokular | **Yapay zekâ yok**: Poly Haven / ambientCG CC0 + betikle üretilen kir/leke haritaları | | `Art/Dokular/LISANSLAR.md` |
| Müzik | **Yapay zekâ yok**: CC0 / CC BY parçalar | | jenerik |

## Form metni (Şahan form doldururken buradan kopyalar)

**İngilizce (Steam formu İngilizce ister):**

> The nine family photographs seen in the apartments were generated with Stable Diffusion XL running
> locally and then aged and distorted by our own scripts; they depict no real person. The game's text
> (notes, phone calls, radio lines, menus) and source code were written with the assistance of Claude,
> an AI coding assistant, under the developer's direction and review. The German, Spanish, Portuguese,
> Russian, Chinese and Japanese translations were produced with Claude from the Turkish and English
> originals. No AI-generated voices, music or
> sound effects are used; voices are recorded by the developer. The game does not generate any content
> while running.

**Türkçe (kayıt için):**

> Dairelerde görülen dokuz aile fotoğrafı, yerel olarak çalıştırılan Stable Diffusion XL ile üretildi,
> sonra kendi betiklerimizle eskitildi ve bozuldu; hiçbir gerçek kişiyi göstermez. Oyunun metinleri
> (notlar, telefon konuşmaları, radyo cümleleri, menüler) ve kaynak kodu, geliştiricinin yönetiminde ve
> denetiminde, yapay zekâ kodlama yardımcısı Claude ile yazıldı. Almanca, İspanyolca, Portekizce, Rusça,
> Çince ve Japonca çeviriler Türkçe ve İngilizce asıllardan Claude ile yapıldı. Yapay zekâ ile üretilmiş ses, müzik ya
> da ses efekti yoktur; sesler geliştirici tarafından kaydedilir. Oyun çalışırken hiçbir içerik üretmez.

## Jenerik

Jenerikte ("jenerik" metni) fotoğraflar için bir satır var: "Aile fotoğrafları: Stable Diffusion XL ile
üretildi (gerçek kişi yok)". Metin ve kod için jenerik satırı Şahan'ın kararına bırakıldı (standing kural:
commit ve PR'larda yapay zekâ imzası yok; jenerik ayrı bir karar).

## Ne zaman güncellenir

Yeni bir yapay zekâ kullanımı eklenirse (örneğin kapsül görseli, fragman sesi) bu tablo ve form metni
aynı commit'te güncellenir. Steam sayfası (#12) açılırken bu metin forma yapıştırılır.
