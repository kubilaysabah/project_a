# Ses kataloğu (#28)

Oyundaki her ses, numarasıyla. Revizyon isterken numarayla söylemek yeterli:
"S03 daha seyrek ağlasın", "S06 çok yüksek", "S15 daha tok olsun".

Her satırın sonundaki **Revizyon notu** sütunu, Şahan'ın isteğinin yazılacağı yer.
İstek yapılınca `✓` ve commit hash'i eklenir.

**Genel bilgi**
- **Bütün sesler kodla üretiliyor**, hazır kayıt yok. Üreten betikler `Art/Ses/scripts/`
  içinde. Bir sesin **karakteri** (perde, boğukluk, ritim, uzunluk) betikte değişir,
  betik yeniden çalıştırılır. Bir sesin **seviyesi, duyulma mesafesi ve yeri** ise
  sahnede ya da oyun kodunda değişir, ses dosyasına dokunulmaz.
- Herhangi bir ses, istenirse gerçek bir kayıtla değiştirilebilir (kod değişmez, yalnızca
  dosya). O zaman kaynak, lisans ve boyut önce sorulur.
- **3B** ses, dünyada bir noktadan gelir: uzaklaştıkça kısılır, sağdan-soldan duyulur.
  **2B** ses "kafanın içinden" gelir, yeri yoktur.
- **Seviye** 0-1 arası (1 = dosyanın tam gücü). #27'deki ses ayarı hepsini birlikte ölçekler.
- **Duyulma mesafesi** (min–maks, metre): min'e kadar tam güçte, maks'ta sessiz.

---

## A. Ortam katmanları — her katta, sürekli (#14)

Normal bir katın ses dokusu. Hepsi döngüde çalar, 3B'dir ve **saate kilitlidir**: iki
sahanlıktaki kopyalar aynı anı çalar, ışınlanmada ses atlamaz. Açılıştaki zemin katta
da aynı dört katman (S01-S04) vardır. Anomali kartları bu katmanları susturarak,
taşıyarak ya da yerine bozuk bir ses koyarak "bozar" (D bölümü).

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Bozan kartlar | Revizyon notu |
|---|---|---|---|---|---|---|
| **S01** | **Bina uğultusu.** Sürekli, alçak bir vızıltı: elektrik şebekesi ve hava. Fark edilmeyecek kadar sabit; kesilince fark edilir. | Sahanlığın ortası (`Ses_Ugultu`) | `Assets/Audio/Ortam/Ortam_Ugultu.wav` · `ortam_uret.py` → `ugultu()` | 0,25 · 4–9 m · 8 sn | `karanlik_kat`, `sessizlik` (susturur) | |
| **S02** | **Duvar arkasından TV.** Boğuk konuşma mırıltısı (kelimeler seçilmez), arada kısık bir müzik akoru. | 2 numaralı dairenin içi (`Kapi_2/Ses_TV`) | `Assets/Audio/Ortam/Ortam_TV.wav` · `ortam_uret.py` → `tv()` | 0,50 · 1,5–9 m · 24 sn | `susan_tv`, `sessizlik` (susturur); `yanlis_tv` (9 m sağa, karşı daireye taşır); `calan_zil` (2,5 sn sonra susar); `besinci_kapi` (var olmayan 5. kapıdan da TV) | |
| **S03** | **Ağlayan bebek.** Başka bir dairede arada bir ağlama nöbetleri ("vaaa", titreyen perde, nefes alışlar); çoğu zaman sessiz. | 4 numaralı dairenin içi (`Kapi_4/Ses_Bebek`) | `Assets/Audio/Ortam/Ortam_Bebek.wav` · `ortam_uret.py` → `bebek()` | 0,55 · 1,5–10 m · 30 sn | `sessizlik` (susturur); `ters_aglama` (S12 ile değiştirir); `asansorde_aglama` (S13 ile değiştirir) | |
| **S04** | **Boru tıkırtıları.** Duvarın içinde düzensiz aralıklarla metalik çın + tok vuruş. | Sahanlık duvarının içi (`Ses_Boru`) | `Assets/Audio/Ortam/Ortam_Boru.wav` · `ortam_uret.py` → `boru()` | 0,50 · 1–8 m · 16 sn | `sessizlik` (susturur); `duzenli_boru` (S14 ile değiştirir) | |
| **S05** | **Rüzgâr.** Merdiven boşluğunun penceresinden gelen hafif rüzgâr. | Merdiven çekirdeği (`Cekirdek_A/Ses_Ruzgar`) | `Assets/Audio/Ortam/Ortam_Ruzgar.wav` · `ortam_uret.py` → `ruzgar()` | 0,30 · 1–6 m · 12 sn | — | |

## B. Oyuncu ve olay sesleri (#25)

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S06** | **Ayak sesleri.** Beton üstünde topuk vuruşu + taban sürtünmesi; dört çeşit, her adımda perde biraz değişir (makine gibi tekrar etmesin). | Her 0,6 m yürüyüşte bir; 2B ("kendi ayağın") | `Assets/Resources/Sesler/Adim_1..4.wav` · `oyuncu_sesleri_uret.py` → `adimlar()` · çalan kod `AyakSesi.cs` | 0,35 × (0,85–1,0), perde 0,92–1,08 · — · 0,3 sn | |
| **S07** | **Lamba sönme "tık"ı.** Röle tıkı + ampul telinin kısa çınlaması. | Ceza anında her lamba sönerken, lambanın yerinden (3B) | `Assets/Resources/Sesler/Lamba_Sonme.wav` · `oyuncu_sesleri_uret.py` → `lamba_sonme()` · çalan kod `KatYoneticisi.cs` | 0,9 · — · 0,6 sn | |
| **S08** | **Son lambanın cızırtısı.** 50 Hz elektrik vızıltısı + düzensiz kıvılcım çıtırtıları. | Ceza anında son lamba titrerken (3B) | `Assets/Resources/Sesler/Lamba_Cizirti.wav` · `oyuncu_sesleri_uret.py` → `lamba_cizirti()` · çalan kod `KatYoneticisi.cs` | 0,8 · — · 0,9 sn | |
| **S09** | **Poşet hışırtısı.** Naylonun buruşması: çok sayıda minik, tiz çıtırtı. | Çöp poşeti toplanınca, poşetin yerinden (3B) | `Assets/Resources/Sesler/Poset_Hisirti.wav` · `oyuncu_sesleri_uret.py` → `poset()` · çalan kod `CopPoseti.cs` | 0,8 · — · 0,5 sn | |

## C. Final sesleri (#16, #25)

| No | Ses | Nerede / ne zaman | Dosya · betik | Seviye · mesafe · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S10** | **Sabah.** Uzak şehir uğultusu, hafif rüzgâr ve kuş cıvıltıları. | Çatı kapısı açılınca çatıdan içeri dolar (3B); dış planda aynı ses 2B | `Assets/Audio/Ortam/Ortam_Sabah.wav` · `oyuncu_sesleri_uret.py` → `sabah()` | çatıda 0,60 · 3–20 m; dış planda 0,70 · 20 sn | |
| **S11** | **Güvercinler.** Ürken bir sürünün kanat çırpışları; önce hep birlikte, sonra uzaklaşarak. | Çatı kapısı açıldığı an, çatının ortasından (3B) | `Assets/Resources/Sesler/Guvercin_Kanat.wav` · `oyuncu_sesleri_uret.py` → `guvercinler()` · çalan kod `FinalSahnesi.cs` | 1,0 · — · 3,5 sn | |
| — | *Sabah ezanı — henüz yok.* Gerçek, lisansı temiz bir kayıt bekleniyor (#16). | Çatıda, uzaktan, kısık | — | — | |

## D. Anomali sesleri — yalnızca anomalili katlarda (#13, #14)

Kart hangi katta gelirse o katta çalar. "Gecikme", oyuncunun kata varışından sonra geçen
süre.

| No | Ses | Hangi kart · nerede · ne zaman | Dosya · betik | Seviye · uzunluk | Revizyon notu |
|---|---|---|---|---|---|
| **S12** | **Tersten ağlama.** S03'teki nöbetin tersten çalınmışı (boğuk). | `ters_aglama` (TEKİNSİZ, zorluk 2): normal bebek susar, bu 4 numaranın içinden çalar (döngü) | `Assets/Audio/Anomali/Bebek_Ters.wav` · `ortam_uret.py` → `bebek()` | 0,55 · 14 sn | |
| **S13** | **Yakından ağlama.** Aynı nöbet ama boğuk değil: duvar arkasından değil, çok yakından. | `asansorde_aglama` (TEKİNSİZ, 2): normal bebek susar, bu asansör kuyusundan çalar (döngü) | `Assets/Audio/Anomali/Bebek_Yakin.wav` · `ortam_uret.py` → `bebek()` | 0,60 · 14 sn | |
| **S14** | **Düzenli boru.** Boru tıkırtısı makine gibi: hep aynı güçte, tam 0,8 sn arayla. | `duzenli_boru` (TEKİNSİZ, 3): normal boru susar, bu sol arka duvardan çalar (döngü) | `Assets/Audio/Anomali/Boru_Ritim.wav` · `ortam_uret.py` → `boru()` | 0,50 · 16 sn | |
| **S15** | **Kapı çarpması.** | `carpan_kapi` (TEKİNSİZ, 2): 2 numaranın kapısı, 4 sn sonra | `Assets/Audio/Anomali/Kapi_Carpma.wav` · `kapi_carpma_uret.py` | 1,0 · 1,2 sn | |
| **S16** | **Kapı tıklatma.** | `kapi_tiklatiliyor` (TEKİNSİZ, 2): 1 numaranın içinden, 3 sn ve 8 sn sonra | `Assets/Audio/Anomali/Kapi_Tiklatma.wav` · `kapi_tiklatma_uret.py` | 1,0 · 1,4 sn | |
| **S17** | **Kapı zili (ding-dong).** | `calan_zil` (KÂBUS): 3 numaranın zili kendi kendine, 2,5 sn ve 8 sn sonra; ilk çalışta TV (S02) susar | `Assets/Audio/Anomali/Zil_DingDong.wav` · `zil_uret.py` | 1,0 · 2,6 sn | |
| **S18** | **Asansör "ding"i.** Arızalı asansör varmış gibi. | `acilan_asansor` (KÂBUS): asansör göstergesinden, 3 sn sonra | `Assets/Audio/Anomali/Asansor_Ding.wav` · `asansor_ding_uret.py` | 1,0 · 2,4 sn | |
| **S19** | **Asansör kuyusu uğultusu.** Kuyudan yükselen derin uğultu. | `acilan_asansor` (KÂBUS): kuyudan, 3,6 sn sonra başlar (döngü) | `Assets/Audio/Anomali/Asansor_Ugultu.wav` · `ortam_uret.py` → `asansor_ugultu()` | 0,70 · 8 sn | |

**Yalnızca sesi susturan / taşıyan kartlar** (yeni ses çalmaz, ortamı bozar):
`susan_tv` (S02 susar) · `yanlis_tv` (S02 karşı daireden gelir) · `sessizlik` (S01-S04
hepsi susar) · `karanlik_kat` (S01 susar, ışıklar da söner) · `besinci_kapi` (S02
var olmayan 5. kapının arkasından da çalar, seviye 0,45).

---

*Liste, 2026-09-25'te sahnedeki ses kaynakları ve bütün anomali kartları taranarak
çıkarıldı: 22 ses dosyası (S06 dört dosya), 11 sahne ses kaynağı, sesle ilgili 12 anomali kartı.*
