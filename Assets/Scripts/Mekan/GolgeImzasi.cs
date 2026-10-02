// GolgeImzasi.cs
// Gölge'nin İMZA SESİ (#105): her belirişinden hemen önce aynı ses, bir kapıcının anahtarlığının şıkırtısı.
//
// NEDEN
// Korku oyunlarında canavarın tanınan bir sesi olur; oyuncu ikinci karşılaşmada o sesi duyunca neyin
// geldiğini bilir ve korku tam o anda, canavar görünmeden başlar. Bizim varlığımız önceki kapıcı:
// anahtarlığını hâlâ taşıyor. Ses her yerde aynı dosyadır (Sesler/Anahtarlik, kayit_isle.py anahtarlik),
// 3B çalar: oyuncu Gölge'nin NEREDEN geleceğini de duyar.
//
// Kullanan yerler (altısı da bu tek fonksiyonu çağırır; başka hiçbir şey bilmezler):
//   Gezen Gölge (üst kattan adımların başı), av (Varlik.Av), 7. kat kovalamacası (KacisAni.Baslat),
//   6. kat saklanma anı (SaklanmaAni.UyariBaslat), ayna korkusu (AynaKorkusu), duvardaki gölge (GolgeIzi).

using UnityEngine;

public static class GolgeImzasi
{
    private static AudioClip klip;
    private static bool arandi;   // dosya bir kez aranır; yoksa her seferinde yeniden denenmez

    /// <summary>
    /// İmza sesini 'yer'den çalar (3B). ses: 0-1. Dosya yoksa sessizce hiçbir şey yapmaz.
    /// Her çağrı günlüğe "[IMZA]" yazar: testler sayar.
    /// </summary>
    public static void Cal(Vector3 yer, float ses = 0.9f)
    {
        if (!arandi) { klip = Resources.Load<AudioClip>("Sesler/Anahtarlik"); arandi = true; }
        if (klip == null) return;
        GameObject g = new GameObject("Golge_Imzasi");
        g.transform.position = yer;
        AudioSource a = g.AddComponent<AudioSource>();
        a.clip = klip;
        a.spatialBlend = 1f;             // 3B: yönü duyulsun
        a.volume = Mathf.Clamp01(ses);
        a.minDistance = 1.5f;            // bu mesafeye kadar tam ses
        a.maxDistance = 18f;             // bundan uzakta duyulmaz (dairenin öbür ucu 10-12 m)
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();
        Object.Destroy(g, klip.length + 0.1f);
        Debug.Log($"[IMZA] anahtarlık {yer:F1} konumundan çaldı.");
    }
}
