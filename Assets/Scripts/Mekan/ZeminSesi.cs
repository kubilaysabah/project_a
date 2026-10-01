// ZeminSesi.cs
// Bir noktadaki zeminin türü ve o zeminin adım sesi (#99).
//
// Oyuncunun ayak sesi (#87, AyakSesi) zemine göre değişiyordu ama Gölge'nin adımları her yerde
// parke sesiydi: beton merdivende de, fayanslı mutfakta da. Gölge de aynı binada yürüyor; onun
// adımı da bastığı yere uymalı. Zemini bulma işi buraya taşındı ki oyuncu da Gölge de aynı kuralı
// kullansın:
//   - aşağı bir ışın atılır, en yakın çarpma zemindir;
//   - bir dairenin parçası değilse BETON (merdiven, sahanlık);
//   - adında halı/kilim/yolluk/paçavra/paspas geçen bir süs eşyasıysa HALI;
//   - "Daire_Fayans" kutusuysa FAYANS; dairenin başka her zemini PARKE.

using UnityEngine;

public static class ZeminSesi
{
    public enum Zemin { Beton, Parke, Fayans, Hali }

    private static AudioClip[] beton, parke, fayans, hali;

    /// <summary>Noktanın altındaki zemin. haric: ışının görmezden geleceği nesne (oyuncunun kendisi).</summary>
    public static Zemin Bul(Vector3 nokta, Transform haric)
    {
        RaycastHit[] vuruslar = Physics.RaycastAll(nokta + Vector3.up * 0.3f, Vector3.down, 0.8f, ~0, QueryTriggerInteraction.Ignore);
        float enYakin = float.MaxValue;
        Transform zemin = null;
        foreach (RaycastHit v in vuruslar)
        {
            if (haric != null && v.collider.transform.IsChildOf(haric)) continue;
            if (v.distance < enYakin) { enYakin = v.distance; zemin = v.collider.transform; }
        }
        if (zemin == null || zemin.GetComponentInParent<DaireEksenleri>() == null) return Zemin.Beton;
        for (Transform t = zemin; t != null && t.GetComponent<DaireEksenleri>() == null; t = t.parent)
        {
            string ad = t.name;
            if (ad.Contains("Hali") || ad.Contains("Kilim") || ad.Contains("Yolluk") || ad.Contains("Pacavra") || ad.Contains("Paspas"))
                return Zemin.Hali;
        }
        if (zemin.name.StartsWith("Daire_Fayans")) return Zemin.Fayans;
        return Zemin.Parke;
    }

    /// <summary>Noktadaki zeminin adım seslerinden biri (rastgele). Gölge'nin adımları çalar.</summary>
    public static AudioClip AdimKlibi(Vector3 nokta, Transform haric)
    {
        Yukle();
        AudioClip[] cesit;
        switch (Bul(nokta, haric))
        {
            case Zemin.Fayans: cesit = fayans; break;
            case Zemin.Hali: cesit = hali; break;
            case Zemin.Parke: cesit = parke; break;
            default: cesit = beton; break;
        }
        if (cesit == null || cesit.Length == 0) cesit = parke;
        return cesit != null && cesit.Length > 0 ? cesit[Random.Range(0, cesit.Length)] : null;
    }

    // Sesler Resources/Sesler'den bir kez yüklenir; ad önekleri AyakSesi'ndekiyle aynı.
    private static void Yukle()
    {
        if (parke != null) return;
        var b = new System.Collections.Generic.List<AudioClip>();
        var p = new System.Collections.Generic.List<AudioClip>();
        var f = new System.Collections.Generic.List<AudioClip>();
        var h = new System.Collections.Generic.List<AudioClip>();
        foreach (AudioClip k in Resources.LoadAll<AudioClip>("Sesler"))
        {
            if (k.name.StartsWith("Adim_Fayans")) f.Add(k);
            else if (k.name.StartsWith("Adim_Hali")) h.Add(k);
            else if (k.name.StartsWith("Adim")) p.Add(k);
            else if (k.name.StartsWith("Merdiven_Adim")) b.Add(k);
        }
        beton = b.ToArray(); parke = p.ToArray(); fayans = f.ToArray(); hali = h.ToArray();
    }
}
