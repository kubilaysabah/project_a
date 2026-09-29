# telefon_takimi.py
# Antredeki telefon sehpasının üstü (#83, 1. katın antresi): dantel örtü, telefon rehberi ve
# fişi çekilmiş telefon kablosu. Telefonun kendisi kapıcı odasındaki model (telefon.py); Unity'deki
# şablon onu dantelin üstüne koyar (Blender'da x = -0,05, y = 0,11 noktasına: telefonun pivotu arka
# kenarının ortası).
# Hikâye: 1. kattaki ailenin borcu yüzünden telefon hattı kesilmiş; kablo duvardan sökülmüş, ucu
# rehberin üstünde duruyor.
#
# PARÇALAR
#   Telefon_Takimi : tek parça (dantel, rehber, kablo, fiş). Pivot sehpanın üst yüzünün ortası.
# Ön yüz -y tarafında; Unity'de +Z.

import os, sys, math
sys.path.append(os.path.dirname(__file__))
import ortak
import bpy

ortak.sahneyi_temizle()
dantel = ortak.malzeme("Takim_Dantel", (0.86, 0.82, 0.72), puruz=0.9)        # sararmış beyaz dantel
kapak = ortak.malzeme("Takim_Rehber_Kapak", (0.55, 0.40, 0.05), puruz=0.7)   # sarı karton kapak
sayfa = ortak.malzeme("Takim_Rehber_Sayfa", (0.75, 0.72, 0.62), puruz=0.9)   # kenardan görünen sayfalar
kablo = ortak.malzeme("Takim_Kablo", (0.70, 0.65, 0.55), puruz=0.5)          # telefonla aynı krem
fis = ortak.malzeme("Takim_Fis", (0.85, 0.84, 0.80), puruz=0.4)

p = []
# Dantel: yuvarlak ince örtü, kenarında fisto (küçük yarım daireler).
DX = -0.05                         # dantelin ve telefonun merkezi (x)
p.append(ortak.silindir("Dantel", 0.15, 0.002, (DX, 0, 0.001), dantel, kose=48))
for i in range(28):
    a = 2 * math.pi * i / 28
    p.append(ortak.silindir("Fisto", 0.018, 0.002, (DX + math.cos(a) * 0.152, math.sin(a) * 0.152, 0.001), dantel, kose=12))

# Telefon rehberi: sağda, dantelin kenarına binmiş kalın kitap; üstü sarı kapak, kenarları sayfa.
RX, RY, REN, RBOY, RKAL = 0.14, -0.02, 0.14, 0.20, 0.028
p.append(ortak.kutu("Sayfalar", (REN - 0.006, RBOY - 0.006, RKAL - 0.004), (RX, RY, 0.002 + RKAL / 2), sayfa, 0.002))
p.append(ortak.kutu("KapakUst", (REN, RBOY, 0.002), (RX, RY, 0.002 + RKAL - 0.001), kapak))
p.append(ortak.kutu("KapakAlt", (REN, RBOY, 0.002), (RX, RY, 0.003), kapak))
p.append(ortak.kutu("Sirt", (0.003, RBOY, RKAL), (RX - REN / 2, RY, 0.002 + RKAL / 2), kapak))

# Kablo: telefonun arkasından çıkar, sağa gider, rehberin üstüne tırmanır ve fişli ucu orada kalır.
K = 0.006
z0, z1 = 0.005, 0.002 + RKAL + K / 2
p.append(ortak.kutu("Kablo1", (0.03, K, K), (0.055, 0.10, z0), kablo))                  # telefonun arkasından
p.append(ortak.kutu("Kablo2", (K, 0.04, K), (0.07, 0.08, z0), kablo))                  # öne kıvrılır
p.append(ortak.kutu("Kablo3", (K, K, z1 - z0 + K), (0.07, 0.06, (z0 + z1) / 2), kablo))  # rehberin kenarına tırmanır
p.append(ortak.kutu("Kablo4", (0.09, K, K), (0.115, 0.06, z1), kablo))                  # rehberin üstünde sağa
p.append(ortak.kutu("Kablo5", (K, 0.10, K), (0.16, 0.013, z1), kablo))                  # öne doğru
p.append(ortak.kutu("Fis", (0.022, 0.03, 0.014), (0.16, -0.05, z1 + 0.004), fis, 0.003))  # duvardan sökülmüş fiş
p.append(ortak.kutu("FisUc", (0.004, 0.008, 0.004), (0.155, -0.068, z1 + 0.004), kablo))
p.append(ortak.kutu("FisUc", (0.004, 0.008, 0.004), (0.165, -0.068, z1 + 0.004), kablo))

takim = ortak.birlestir(p, "Telefon_Takimi")
ortak.yumusat(takim, 30)
ortak.pivotu_tasi(takim, (0, 0, 0))
ortak.calistir_ve_bitir("Telefon_Takimi", [takim], 0.15, 0.8)
