# kayit_donustur.py
# İndirilmiş gerçek bir ses kaydını (ogg, mp3, flac, wav...) 16 bit mono WAV'a çevirir (#14, #25).
#
# ÇALIŞTIRMA (Blender içinden, penceresiz):
#   blender -b --factory-startup --python Art/Ses/scripts/kayit_donustur.py -- <girdi> <çıktı.wav> [örnek_hızı] [kanal]
#   kanal: 1 = mono (varsayılan, 3B sesler için), 2 = stereo (müzik gibi 2B sesler için)
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
kanal = int(argumanlar[3]) if len(argumanlar) > 3 else 1

ses = aud.Sound(girdi)                 # dosyayı aç (biçimi kendisi tanır)
ses = ses.resample(hiz)                # istenen örnek hızına getir
ses = ses.rechannel(kanal)             # mono istenirse stereo kayıt mono'ya iner: Unity 3B konumu kendisi hesaplıyor
ses.write(cikti, rate=hiz, channels=aud.CHANNELS_STEREO if kanal == 2 else aud.CHANNELS_MONO,
          format=aud.FORMAT_S16, container=aud.CONTAINER_WAV, codec=aud.CODEC_PCM)
print("DONUSTU", girdi, "->", cikti, hiz, "Hz")
