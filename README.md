# Project A (çalışma adı)

1994 model bir Türk apartmanında geçen **birinci şahıs anomali-korku** oyunu. Oyuncu gece kapıcısıdır: kat kat çıkıp çöpleri toplar, her sahanlıkta tek bir karar verir — **normalse devam et, bir anomali varsa geri dön.**

> **İsim kararı bekliyor.** Steam'de "KAPICI" adlı başka bir oyun bulunduğu için nihai isim henüz seçilmedi; repo bu yüzden `project_a` adını taşıyor. Ayrıntı ve adaylar: [issue #1](https://github.com/kubilaysabah/project_a/issues/1).

## Oyun

- **Çekirdek döngü:** üst kata çık → kapı önündeki çöp poşetini al → sahanlığı gözle ve kulakla tara → karar ver.
- **Ceza anında gelmez:** yanlış karardan sonra yarım kat çıkarsın, ışıklar teker teker söner, sonra 1. kata dönersin. O sessizlik oyunun en korkunç anıdır.
- **Kazanma:** 8. kata hatasız ulaşan çatıya çıkar, sabah olur — ve son planda binanın 9 katlı olduğu görülür.
- **İçerik:** 60+ anomali, üç katmanda — FISILTI (ters kapı numarası, tek terlik), TEKİNSİZ (yeşil ampul, aralık kapı), KÂBUS (nadir, klip değerinde anlar).
- **Ses:** normal kat dokusu (uzak TV, bebek ağlaması, boru tıkırtısı) anomali katında bozulur. Kulak gözden önce fark eder.

Hedef: 2-3 ayda Steam çıkışı, $4.99, TR + EN.

## Teknik

- **Unity 6.5 (6000.5.5f1) + URP**, yeni Input System.
- Oyun mantığı (kat sayacı, karar değerlendirme) MonoBehaviour **dışında** düz C# sınıflarında — test edilebilirlik için.
- **İçerik veridir:** her anomali bir `AnomalyCard` ScriptableObject'i. Yeni anomali eklemek kod değil, yeni bir veri dosyası gerektirir.
- Tek modellenen mekân: bir sahanlık + bir kat merdiveni; oyuncu kat geçişinde sezdirmeden başa taşınır.

## Projeyi açmak

1. Unity Hub'da **6000.5.5f1** sürümünü kur (başka sürüm projeyi yükseltmeye çalışır — açma).
2. Depoyu klonlamadan önce `git lfs install` çalıştır. `.gitattributes` binary dosyaları Git LFS'te tutar; LFS kurulu değilse asset'ler 3 satırlık işaretçi dosyası olarak iner.
3. Unity Hub → Add → klasörü seç → aç.

## Çalışma düzeni

- **`CLAUDE.md` bu projenin bağlayıcı hafızasıdır** — her oturum onu okur, kurallar oradadır (mentor modu, git kuralları, mimari kararlar, durum).
- Her iş bir GitHub issue'sudur: İngilizce emir kipi başlık, Türkçe gövde (Amaç / Kapsam / Kabul kriterleri).
- Commit mesajları %100 İngilizce, Conventional Commits. **AI imzası / Co-Authored-By eklenmez.**
- **Derleme kontrolü pazarlıksız:** her kod adımından sonra Unity Console'da 0 hata doğrulanmadan ilerlenmez.
- Ders sonunda önce sahne kaydedilir (Ctrl+S), sonra commit atılır.

## Bağlantılar

| Ne | Nerede |
| --- | --- |
| Görev panosu | [Projects #4](https://github.com/users/kubilaysabah/projects/4) |
| Açık işler | [Issues](https://github.com/kubilaysabah/project_a/issues) |
| Proje hafızası (oturum günlüğü, kararlar) | [Canlı doküman](https://claude.ai/code/artifact/00a0d343-6a47-48dc-807e-b671a03224c1) |
| Oyun tasarım belgesi | [Canlı doküman](https://claude.ai/code/artifact/78f0fa6f-6087-497b-8492-4576f63bd6ee) |

## Klasör yapısı

```
Assets/           Oyun içeriği: sahneler, scriptler, materyaller, anomali kartları
Packages/         Unity paket bildirimi (manifest.json)
ProjectSettings/  Unity proje ayarları
CLAUDE.md         Bağlayıcı proje hafızası ve çalışma kuralları
```
