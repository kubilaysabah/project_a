// DuraklatmaMenusu.cs
// Esc ile açılan duraklatma menüsü (#15): Devam, Dil, Oyundan Çık.
// Dil seçimi burada yapılır; ileride fare hassasiyeti ve ses ayarı da buraya gelir.
//
// Menü açılınca:
//   - oyunun saati durur (Time.timeScale = 0),
//   - sesler duraklar (AudioListener.pause),
//   - oyuncu yürüyemez, bakamaz, etkileşemez,
//   - fare serbest kalır ve görünür olur.
// Kapanınca hepsi TAM OLARAK ESKİ HÂLİNE döner; "açık/kapalı"ya değil, menü
// açılmadan önceki değere. Ceza karanlığında ses zaten kapalıysa (KatYoneticisi
// kapatmıştı) menüden dönünce ses açılmamalı; aksi hâlde karanlığın sessizliği bozulur.
//
// Menü NE ZAMAN açılamaz: oyuncunun kontrolü zaten kapalıyken. Bu iki durumu kapsar:
// yönetici notu açıkken (Esc orada notu kapatır) ve final sırasında (kamera dışarıda).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuraklatmaMenusu : MonoBehaviour
{
    private bool acik;

    // Bir önceki karenin sonunda oyuncu oynayabiliyor muydu? Esc'yi bununla
    // karşılaştırıyoruz. Neden bir önceki kare: notu kapatan Esc basışı, aynı karede
    // oyuncunun kontrolünü geri açıyor. Bu karenin durumuna bakarsak aynı basış
    // menüyü de açardı; not kapanır kapanmaz menü açılırdı.
    private bool oncekiKareOynanabilir;

    private BirinciSahisKontrolcu kontrolcu;

    // Menünün kendisinin kapattığı bileşenler. Geri açarken yalnızca bunları açarız;
    // başka biri (not, final) kapattıysa ona dokunmayız.
    private readonly List<Behaviour> kapattiklarim = new List<Behaviour>();

    // Menü açılmadan önceki değerler; kapanınca bunlara dönülür.
    private float eskiZamanOlcegi = 1f;
    private bool eskiSesDuraklamasi;
    private CursorLockMode eskiFareKilidi;
    private bool eskiFareGorunur;

    private void Start()
    {
        kontrolcu = FindAnyObjectByType<BirinciSahisKontrolcu>();
    }

    private void Update()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (!esc) return;

        if (acik) Devam();
        else if (oncekiKareOynanabilir) Ac();
    }

    // LateUpdate: bütün Update'lerden sonra çalışır. Karenin SON durumunu kaydeder.
    private void LateUpdate()
    {
        oncekiKareOynanabilir = !acik && kontrolcu != null && kontrolcu.enabled;
    }

    // ---------- AÇ / KAPAT ----------
    private void Ac()
    {
        acik = true;

        // Time.timeScale: oyun saatinin hızı. 1 = normal, 0 = durmuş. Sıfırken
        // Time.deltaTime 0 olur: hareket, fizik, WaitForSeconds bekleyen ceza ve final
        // sayaçları durur. Update yine çalışır; menü bu yüzden tepki vermeye devam eder.
        eskiZamanOlcegi = Time.timeScale;
        Time.timeScale = 0f;

        eskiSesDuraklamasi = AudioListener.pause;
        AudioListener.pause = true;

        // Oyuncunun girişi okuyan bileşenlerini kapat (açık olanları).
        kapattiklarim.Clear();
        if (kontrolcu != null)
        {
            KapatVeHatirla(kontrolcu);
            KapatVeHatirla(kontrolcu.GetComponent<Etkilesimci>());
        }

        eskiFareKilidi = Cursor.lockState;
        eskiFareGorunur = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Devam()
    {
        acik = false;

        Time.timeScale = eskiZamanOlcegi;
        AudioListener.pause = eskiSesDuraklamasi;

        foreach (Behaviour b in kapattiklarim) if (b != null) b.enabled = true;
        kapattiklarim.Clear();

        Cursor.lockState = eskiFareKilidi;
        Cursor.visible = eskiFareGorunur;
    }

    private void KapatVeHatirla(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        kapattiklarim.Add(b);
    }

    // Menü açıkken sahne kapanırsa (final sonrası yeniden yükleme, Play'den çıkış)
    // saat durmuş kalmasın: Time.timeScale sahneden sahneye taşınan bir ayar.
    private void OnDisable()
    {
        if (acik) Devam();
    }

    private void OyundanCik()
    {
        // #if: derleme anında seçim. UNITY_EDITOR yalnızca Editor'de tanımlı; bu satır
        // oyunun kendisine (build) hiç girmez. Editor'de Application.Quit bir şey
        // yapmadığı için Play modundan çıkıyoruz.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- ÇİZİM ----------
    // Not ve jenerik gibi OnGUI ile çiziliyor. GUI.Button bir düğme çizer ve o karede
    // tıklandıysa true döner; düğmenin ne yapacağı hemen altındaki if'te.
    private GUIStyle baslikStili, dugmeStili;

    private void OnGUI()
    {
        if (!acik) return;
        GUI.depth = -200;   // not ve jeneriğin de üstünde

        if (baslikStili == null)
        {
            baslikStili = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            baslikStili.normal.textColor = new Color(0.92f, 0.90f, 0.84f);
            dugmeStili = new GUIStyle(GUI.skin.button) { fontSize = 24 };
        }

        // Ekranı karart.
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float en = 360f, boy = 56f, ara = 16f;
        float x = (Screen.width - en) / 2f;
        float y = Screen.height / 2f - 150f;

        GUI.Label(new Rect(0, y, Screen.width, 60f), Dil.Getir("menu.baslik"), baslikStili);
        y += 60f + 2f * ara;

        if (GUI.Button(new Rect(x, y, en, boy), Dil.Getir("menu.devam"), dugmeStili)) Devam();
        y += boy + ara;

        // "Dil: Türkçe" / "Language: English". Dilin adı her zaman kendi dilinde
        // yazılır; İngilizce bir oyuncu da "Türkçe"yi, Türk oyuncu da "English"i tanır.
        string dilYazisi = string.Format(Dil.Getir("menu.dil"), Dil.Getir("dil.ad"));
        if (GUI.Button(new Rect(x, y, en, boy), dilYazisi, dugmeStili)) Dil.Sonraki();
        y += boy + ara;

        if (GUI.Button(new Rect(x, y, en, boy), Dil.Getir("menu.cik"), dugmeStili)) OyundanCik();
    }
}
