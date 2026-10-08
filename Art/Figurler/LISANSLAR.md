# Üç harfli figürleri — kaynaklar ve lisanslar (#134)

Şahan'ın yerel seçim sayfasından (2026-10-08) seçtiği altı figür. Hepsi Sketchfab'dan, **CC BY 4.0**
(Creative Commons Attribution): ticari kullanım serbest, yazar adı ve kaynak belirtilir (jenerik `jenerik`
satırı + bu dosya). Modeller `Art/Figurler/indirilen/<N>/` altına **glTF** olarak indirilir (Sketchfab hesabı
gerekir; Şahan indirir), `Art/Blender/scripts/figur_hazirla.py` ile `Assets/Models/Figurler/Cin_N/` altına
FBX + dokular olarak hazırlanır. İndirilen ham dosyalar depoya girmez (`.gitignore`).

| N | Ad | Yazar | Kaynak | Lisans | Üçgen (ham) |
|---|---|---|---|---|---|
| 1 | Horror Ghost Character - Blood Stained Spirit | adhamAsalah | https://sketchfab.com/3d-models/horror-ghost-character-blood-stained-spirit-ed1a90be19404450935720f7abaae471 | CC BY 4.0 | 197 867 |
| 2 | Drowned Hag - Waterlogged Horror Witch | Pigcraft (s8819296) | https://sketchfab.com/3d-models/drowned-hag-waterlogged-horror-witch-9a4cc20c82b84c3989f5b63576eddb19 | CC BY 4.0 | 1 981 828 |
| 3 | Ghost Woman A-Pose | fluxium3d | https://sketchfab.com/3d-models/ghost-woman-a-pose-f4f4f2f59f124f54a787438bb3ed9ab0 | CC BY 4.0 | 49 970 |
| 4 | A terrifying granny | Kratoz_001 | https://sketchfab.com/3d-models/a-terrifying-granny-251a32e10977495a90860bdbd94d1484 | CC BY 4.0 | 373 552 |
| 5 | Zombie woman | maicollgdalpiaz | https://sketchfab.com/3d-models/zombie-woman-b69df5d905d546a4b79cb78e0d014e71 | CC BY 4.0 | 9 978 |
| 6 | Zombie Female 3D | Pachedore3D | https://sketchfab.com/3d-models/zombie-female-3d-24aa8315c0ef4fbeb1c0dd183322d11a | CC BY 4.0 | 19 110 |

Yazar adları indirilen zip'lerdeki `license.txt` dosyalarından alındı (2026-10-08); jenerik satırı aynı adları taşır.

**Yapay zekâ notu (Steam beyanı, `Docs/steam-yapay-zeka-beyani.md`):** 1, 2 ve 3 numaralı modellerin doku dosya adları
(`tripo_mat_*`, `mvadapter_*`) Tripo / MV-Adapter üretimi olduğunu gösteriyor; yani bu üç figür üçüncü kişilerce yapay zekâyla
üretilmiş CC BY varlıklardır. 4, 5 ve 6 el yapımı görünüyor (emin olunamıyor). Beyanda "üçüncü taraf CC BY 3B figürlerin bir
kısmı yapay zekâ ile üretilmiştir" diye geçer. Hazırlanmış FBX'lerin üçgen sayısı 35-45 bin arasıdır (`figur_hazirla.py` indirger).

## İndirme (Şahan)

1. Sketchfab'da oturum aç; her bağlantıda **Download 3D Model → glTF** (Autoconverted format) seç.
2. İnen zip'i `Art/Figurler/indirilen/<N>/` içine aç (`scene.gltf` + `scene.bin` + `textures/`). N yukarıdaki numara.
3. Sonra: `blender -b --factory-startup --python Art/Blender/scripts/figur_hazirla.py` (hepsi) ya da `-- 3` (yalnızca 3).
4. Unity'de `Huzur → Cin Figürlerini Kur` prefab'ları üretir ve `CinOlaylari`'na bağlar.
