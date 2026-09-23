# gazete.py
# Paspasın üstüne bırakılmış katlanmış gazete (F31). Üstündeki 1994 tarihli
# manşet ve yazılar sonradan DOKU olarak eklenecek; burada sadece katlı kâğıt biçimi var.
# Pivot: alt ortası.

import os, sys
sys.path.append(os.path.dirname(__file__))
import bpy
import ortak

ortak.sahneyi_temizle()
kagit = ortak.malzeme("Gazete_Kagit", (0.74, 0.72, 0.66), puruz=0.95)   # sararmış gazete kâğıdı

# İkiye katlanmış bir gazete üst üste iki kalın deste gibi durur;
# üstteki biraz kaymış ve dönük olunca "elle bırakılmış" hissi verir.
alt = ortak.kutu("Alt", (0.30, 0.21, 0.007), (0, 0, 0.0035), kagit, 0.002)
ust = ortak.kutu("Ust", (0.295, 0.205, 0.005), (0.004, 0.003, 0.0095), kagit, 0.002)
ust.rotation_euler = (0, 0, 0.05)       # 3 derece dönük
bpy.ops.object.transform_apply(rotation=True)

gazete = ortak.tek_parca([alt, ust], "Gazete", 30)
ortak.calistir_ve_bitir("Gazete", [gazete], 0.02, 0.5)
