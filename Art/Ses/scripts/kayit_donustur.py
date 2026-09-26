# kayit_donustur.py
# İndirilmiş gerçek bir ses kaydını (ogg, mp3, flac, wav...) 16 bit mono WAV'a çevirir (#14, #25).
#
# ÇALIŞTIRMA (Blender içinden, penceresiz):
#   blender -b --factory-startup --python Art/Ses/scripts/kayit_donustur.py -- <girdi> <çıktı.wav> [örnek_hızı]
#
# NEDEN BLENDER: Python'un kendi kütüphanesi yalnızca WAV okuyabiliyor; ogg ya da mp3
# çözmek için ayrı bir program (ffmpeg) indirmek gerekirdi. Blender zaten kurulu ve
# içinde "aud" adlı bir ses kütüphanesi var: neredeyse her ses biçimini okuyup yazabiliyor.
# Bu betik yalnızca biçim değiştirir. Boğuklaştırma, ters çevirme, kesme gibi işler
# sonra düz Python betiklerinde (kayit_isle.py) WAV üzerinde yapılır.

import sys
import aud

# Blender'ın kendi argümanları "--" işaretine kadar; bizimkiler ondan sonra geliyor.
argumanlar = sys.argv[sys.argv.index("--") + 1:]
girdi, cikti = argumanlar[0], argumanlar[1]
hiz = int(argumanlar[2]) if len(argumanlar) > 2 else 44100

ses = aud.Sound(girdi)                 # dosyayı aç (biçimi kendisi tanır)
ses = ses.resample(hiz)                # istenen örnek hızına getir
ses = ses.rechannel(1)                 # stereo ise mono'ya indir: Unity 3B konumu kendisi hesaplıyor
ses.write(cikti, rate=hiz, channels=aud.CHANNELS_MONO,
          format=aud.FORMAT_S16, container=aud.CONTAINER_WAV, codec=aud.CODEC_PCM)
print("DONUSTU", girdi, "->", cikti, hiz, "Hz")
