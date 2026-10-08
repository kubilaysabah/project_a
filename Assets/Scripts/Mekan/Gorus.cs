// Gorus.cs
// "Oyuncu bu noktayı görüyor mu?" — ortak yardımcı (#131). KatAni'nin kendi özel kopyası var (#96); cin
// olayları, saklanma anı gibi başka yerler de aynı soruyu soruyor; kural tek yerde dursun.
//
// Görmek iki şeydir: nokta ekranın İÇİNDE (kenarlardan %5 pay bırakılarak; kenarda kalan şey fark edilmez)
// ve kameradan noktaya AÇIK bir çizgi var (duvar, dolap araya girmiyor). Oyuncunun kendi kapsülü engel
// sayılmaz (kamera kapsülün içinde), "haric" verilen nesne de sayılmaz (bir kapının kanadına bakarken
// kanadın kendi çarpıştırıcısı onu örtmesin).

using UnityEngine;

public static class Gorus
{
    public static bool Gorunur(Camera kamera, Transform oyuncu, Vector3 nokta, Transform haric)
    {
        if (kamera == null) return false;
        Vector3 e = kamera.WorldToViewportPoint(nokta);
        if (e.z <= 0f || e.x < 0.05f || e.x > 0.95f || e.y < 0.05f || e.y > 0.95f) return false;
        Vector3 goz = kamera.transform.position;
        Vector3 fark = nokta - goz;
        foreach (RaycastHit h in Physics.RaycastAll(goz, fark, fark.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform t = h.collider.transform;
            if (oyuncu != null && t.IsChildOf(oyuncu)) continue;
            if (haric != null && t.IsChildOf(haric)) continue;
            return false;
        }
        return true;
    }

    /// <summary>Nokta oyuncunun ARKASINDA mı (kameranın ileri yönüne göre 90°'den geniş açıda)?</summary>
    public static bool Arkada(Camera kamera, Vector3 nokta)
    {
        if (kamera == null) return true;
        Vector3 ileri = kamera.transform.forward; ileri.y = 0f;
        Vector3 yon = nokta - kamera.transform.position; yon.y = 0f;
        return Vector3.Dot(ileri.normalized, yon.normalized) < 0.2f;
    }
}
