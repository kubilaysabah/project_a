# Kayıt dosyaları ve Steam Cloud (#122)

Oyun kaydını ve ayarlarını **dosyaya** yazar (eskiden PlayerPrefs = Windows kayıt defteri; Steam Cloud onu eşlemez,
Steam Deck'te yoktur). Kod: `KayitDeposu` (PlayerPrefs ile aynı yüz: GetInt/SetString/HasKey/DeleteKey/Save),
biçim: `KayitDosyasiKurali` (saf C#, testli), satır = `anahtar<TAB>değer`.

| Dosya | İçerik | Steam Cloud |
|---|---|---|
| `kayit.txt` | kat, çözülen kilitler, parçalar, aletler (kat başı anları dâhil), okunan notlar, bakılan fotoğraflar, en yüksek kat | **evet** |
| `ayarlar.txt` | fare, ses/müzik/konuşma, FOV, ters fare, kafa sallanması, kalite, parlaklık, pencere, V-Sync, çözünürlük, dil | hayır (makineye özel) |
| `kayit_test.txt` | Claude'un oynatma testlerinin yuvası (`KayitYuvasi.Ek` = `_test`); Şahan'ın `kayit.txt`'sine test dokunmaz | hayır |

Klasör: `Application.persistentDataPath` = `%USERPROFILE%\AppData\LocalLow\<Şirket>\<Ürün>\`
(bugün `DefaultCompany\project_a`; #1 ile ad değişince klasör de değişir — o gün eski klasördeki `kayit.txt`
yeni klasöre kopyalanır). Editor ve build **aynı klasörü** kullanır (PlayerPrefs'te ayrıydı): Editor'deki kayıt
build'de de görünür.

**İlk açılış:** dosya yoksa aynı adlı PlayerPrefs anahtarları okunup dosyaya yazılır (`[KAYIT] PlayerPrefs'ten N
değer … taşındı`); PlayerPrefs silinmez. Yazım önce `.yeni` dosyasına, sonra yerine: çökme yarım kayıt bırakmaz.

## Steamworks ayarı (sayfa açılınca, #12)

Steamworks → App Admin → **Cloud**: "Enable cloud synchronization" + Auto-Cloud:
- Root: `WinAppDataLocalLow`, Subdirectory: `<Şirket>/<Ürün>`, Pattern: `kayit.txt`, OS: Windows.
- Linux/Proton (Steam Deck) için aynı dosya Proton'un prefix'inde `pfx/drive_c/users/steamuser/AppData/LocalLow/…`
  altındadır; Steam Play için Root `WinAppDataLocalLow` otomatik eşlenir.
- Byte quota: 1 MB / 1 dosya fazlasıyla yeter (`kayit.txt` birkaç KB).
- `ayarlar.txt` listeye **girmez**.
