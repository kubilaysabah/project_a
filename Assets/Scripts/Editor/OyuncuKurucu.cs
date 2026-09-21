// OyuncuKurucu.cs
// Oyuncu nesnesini (gövde + kamera + kontrolcü) sahneye kuran editör aracı.
// GrayboxKurucu ile aynı mantık: sahne menüden baştan kurulabilsin,
// hiçbir şey "bir zamanlar elle yapılmıştı" durumunda kalmasın.

using UnityEngine;
using UnityEditor;

public static class OyuncuKurucu
{
    // Kapıcının ölçüleri. Göz hizası, gerçek bir yetişkinin ~1.65 m'sidir.
    const float BoyUzunlugu  = 1.75f;
    const float GozHizasi    = 1.65f;
    const float GovdeYaricap = 0.3f;

    // Basamak yüksekliği 0.1875 m. Tırmanma eşiği bunun üstünde olmalı,
    // yoksa karakter merdivene takılır.
    const float BasamakEsigi = 0.35f;

    [MenuItem("Huzur/Oyuncuyu Yeniden Kur")]
    public static void YenidenKur()
    {
        // Eski oyuncu varsa sil; iki kopya olmasın.
        GameObject eski = GameObject.Find("Oyuncu");
        if (eski != null)
        {
            // Kamerayı kaybetmeyelim: silmeden önce dışarı çıkarırız.
            Camera ickamera = eski.GetComponentInChildren<Camera>();
            if (ickamera != null) ickamera.transform.SetParent(null, true);

            Object.DestroyImmediate(eski);
        }

        // ---- GÖVDE ----
        GameObject oyuncu = new GameObject("Oyuncu");

        // Sahanlık A'nın üstünde, merdivene bakacak şekilde başlat.
        oyuncu.transform.position = new Vector3(0f, 0.2f, -0.5f);

        // CharacterController: Unity'nin hazır yürüyüş bileşeni.
        // Fizik motorunun tam simülasyonunu kullanmaz; bunun yerine
        // "kapsül şeklinde bir hacmi duvarlara çarpmadan taşı" işini yapar.
        // Yürüyen karakterler için rigidbody'den çok daha öngörülebilirdir.
        CharacterController cc = oyuncu.AddComponent<CharacterController>();
        cc.height = BoyUzunlugu;
        cc.radius = GovdeYaricap;

        // center: kapsülün merkezi. Ayaklar y=0'da olsun diye boyun yarısı kadar yukarı.
        cc.center = new Vector3(0f, BoyUzunlugu / 2f, 0f);

        cc.stepOffset = BasamakEsigi;   // bu yükseklikteki engele kendiliğinden tırmanır
        cc.slopeLimit = 50f;            // bu açıdan dik yamaçlara çıkamaz
        cc.skinWidth = 0.02f;           // çarpışma toleransı; çok küçük olursa duvara girer

        // ---- KAMERA ----
        // Sahnedeki mevcut kamerayı kullan; yoksa yenisini kur.
        Camera kamera = Camera.main;
        if (kamera == null)
        {
            GameObject kamNesne = new GameObject("Kamera");
            kamera = kamNesne.AddComponent<Camera>();
            kamNesne.AddComponent<AudioListener>();
            kamNesne.tag = "MainCamera";
        }

        kamera.gameObject.name = "Kamera";

        // Kamerayı oyuncunun çocuğu yap. false = yerel konumu koru,
        // böylece aşağıdaki değerler doğrudan geçerli olur.
        kamera.transform.SetParent(oyuncu.transform, false);
        kamera.transform.localPosition = new Vector3(0f, GozHizasi, 0f);
        kamera.transform.localRotation = Quaternion.identity;   // identity = hiç dönmemiş

        // Dar bir merdiven boşluğunda geniş açı mide bulandırır; 60 derece rahat.
        kamera.fieldOfView = 60f;
        kamera.nearClipPlane = 0.05f;   // buruna çok yakın nesneler kesilmesin

        // ---- KONTROLCÜ ----
        BirinciSahisKontrolcu kontrolcu = oyuncu.AddComponent<BirinciSahisKontrolcu>();

        // Scriptteki "kamera" alanı private olduğu için doğrudan atayamayız.
        // SerializedObject, Unity'nin Inspector'da kullandığı kapıyı bize açar:
        // private ama [SerializeField] işaretli alanlara kodla erişmeyi sağlar.
        SerializedObject seri = new SerializedObject(kontrolcu);
        seri.FindProperty("kamera").objectReferenceValue = kamera.transform;
        seri.ApplyModifiedProperties();   // değişikliği gerçekten yaz

        // ---- SONSUZ MERDİVEN ----
        // Oyuncuyu üst sahanlıktan alt sahanlığa sessizce taşıyan döngü.
        oyuncu.AddComponent<SonsuzMerdiven>();

        Selection.activeGameObject = oyuncu;   // Hierarchy'de seçili gelsin

        Debug.Log($"Oyuncu kuruldu. Boy: {BoyUzunlugu:0.00} m, göz hizası: {GozHizasi:0.00} m, " +
                  $"basamak eşiği: {BasamakEsigi:0.00} m");
    }
}
