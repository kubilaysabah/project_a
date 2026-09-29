# Aile fotoğrafları: kaynak ve lisans (#69)

Fotoğraflar **yapay zekâyla, Şahan'ın makinesinde yerel olarak** üretildi (Şahan'ın kararı). Hiçbir
gerçek kişinin fotoğrafı kullanılmadı; yüzler modelin ürettiği hayalî kişiler.

| Araç / model | Sürüm | Lisans | Kaynak |
|---|---|---|---|
| Stable Diffusion XL 1.0 base (`sd_xl_base_1.0.safetensors`) | 1.0 | CreativeML Open RAIL++-M: üretilen görüntüler ticari kullanıma uygun; lisansın kullanım kısıtlarına (zarar verici içerik vb.) uyulur | https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0 |
| ComfyUI, taşınabilir AMD sürümü | 0.37.0 | GPL-3.0 (yalnızca araç; çıktılar üzerinde hak iddia etmez) | https://github.com/comfyanonymous/ComfyUI |

İndirmeler Şahan'ın gece izniyle yapıldı (2026-09-28). Kurulum depo dışında:
`C:\Users\sahan\AI\ComfyUI_windows_portable` (yalnızca 127.0.0.1:8188 dinler).

**Yol:**
1. `scripts/foto_uret.py`: ComfyUI API'siyle her fotoğraf için üç aday üretir (tohumlar 1994, 1974, 9; 30 adım, dpmpp_2m karras, cfg 6,5).
2. Seçilen aday `aday/` klasöründe kalır. Öbürleri aynı tohumlarla yeniden üretilebilir, o yüzden silindi.
3. `scripts/foto_isle.py` (başsız Blender) seçileni eskitir: soldurma, sararma, gren, köşe kararması. Bazılarına bozulma ekler:
   - 4. ve 6. katta arkadaki adamın yüzü karanlıkta kalır;
   - 5. ve 8. katta yüzsüz bir siluet eklenir;
   - 7. katta Cemal'in yüzü tükenmez kalemle karalanır.

   Çıktı: `Assets/Textures/Fotograflar/`.

| Fotoğraf | Aday | Bozulma |
|---|---|---|
| kat1 | kat1_1974 | yok |
| kat2 | kat2_9 | yok |
| kat3 | kat3_1994 | yok |
| kat4 | kat4_1974 | arkadaki uzun adamın yüzü karanlıkta |
| kat5 | kat5_1974 | merdivenin tepesinde yüzsüz siluet (eklendi) |
| kat6 | kat6_1974 | ayaktaki adamın yüzü karanlıkta |
| kat7 | kat7_9 | Cemal'in yüzü karalanmış |
| kat8 | kat8_1994 | arka planda beşinci, yüzsüz adam (eklendi) |
| kapici | kapici_1994 | yok |

## Duvar portreleri (#83)

- `Assets/Textures/Portreler/kat2_es.png`: 2. kattaki Nuri Bey'in rahmetli eşinin portresi. Aynı yerel SDXL 1.0 base ile üretildi (`foto_uret.py` PORTRELER, tohum 1994), `foto_isle.py` ile eskitildi. Lisans yukarıdaki gibi (CreativeML Open RAIL++-M; üretilen görüntü ticari kullanıma uygun). Adaylar: `aday/kat2_es_*.png`.
