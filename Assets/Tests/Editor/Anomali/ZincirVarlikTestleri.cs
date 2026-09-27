// ZincirVarlikTestleri.cs
// Projedeki bütün kat zinciri dosyalarının (Assets/Zincirler/*.asset) denetimi (#54, #55).
//
// Kural testleri (KilitZinciriTestleri) kuralın doğru çalıştığını kanıtlar; bu test ise
// GERÇEK dosyalara bakar: her zincir çözülebilir mi, içindeki her yol dairede gerçekten var
// mı? Yolu yanlış yazılmış bir yer oyunda yalnızca bir uyarı basar ve eşya konmaz; oyuncu
// katı bitiremez. Bu test onu oyun oynanmadan yakalar.
//
// Zincir dosyasının sınıfı (KatZinciriVarligi) oyunun varsayılan kodunda; test derlemesi onu
// göremediği için dosyayı ScriptableObject olarak yükleyip "zincir" alanını yansımayla okuyoruz.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[TestFixture]
public class ZincirVarlikTestleri
{
    const string SahneYolu = "Assets/Scenes/Landing.unity";

    private static List<KeyValuePair<string, KilitZinciri>> Zincirler()
    {
        var sonuc = new List<KeyValuePair<string, KilitZinciri>>();
        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Zincirler" }))
        {
            string yol = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject v = AssetDatabase.LoadAssetAtPath<ScriptableObject>(yol);
            var alan = v != null ? v.GetType().GetField("zincir") : null;
            if (alan != null) sonuc.Add(new KeyValuePair<string, KilitZinciri>(yol, (KilitZinciri)alan.GetValue(v)));
        }
        return sonuc;
    }

    // Oyundaki ZincirUygulayici.YolBul ile aynı kural: "Sahanlik:" yolları sahanlıktan, {n} daire numarası.
    // İkinci dairenin yolları ("B:...", #70) öneksiz olarak aynı dairede aranır: dört daire aynı planla kurulu.
    private static Transform Bul(Transform sahanlik, Transform daire, int n, string yol)
    {
        const string On = "Sahanlik:";
        if (yol.StartsWith(KilitZinciri.IkinciOnek)) yol = yol.Substring(KilitZinciri.IkinciOnek.Length);
        return yol.StartsWith(On) ? sahanlik.Find(yol.Substring(On.Length).Replace("{n}", n.ToString())) : daire.Find(yol);
    }

    [Test]
    public void En_az_bir_zincir_dosyasi_var()
    {
        Assert.Greater(Zincirler().Count, 0, "Assets/Zincirler boş; Huzur → Zincir menüsüyle kurulmalı.");
    }

    [Test]
    public void Her_zincir_cozulebilir()
    {
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) { odalar.Add(o.Ad); odalar.Add(KilitZinciri.IkinciOnek + o.Ad); }
        foreach (var z in Zincirler())
        {
            KilitZinciri.Sonuc s = z.Value.Cozumle(odalar);
            Assert.IsTrue(s.Cozulebilir, $"{z.Key}: " + string.Join("; ", s.Hatalar));
        }
    }

    [Test]
    public void Zincirdeki_her_yol_dairede_var()
    {
        Scene sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive);
        try
        {
            Transform a = null;
            foreach (GameObject g in sahne.GetRootGameObjects()) if (g.name == "Sahanlik_A") a = g.transform;
            Assert.IsNotNull(a);
            foreach (var z in Zincirler())
                for (int n = 1; n <= 4; n++)
                {
                    Transform d = a.Find($"Daire_{n}");
                    foreach (ZincirYeri y in z.Value.Yerler)
                    {
                        // Planda nokta olan yer (#63) bir nesne değil; odasını kural denetliyor.
                        if (y.PlandaNokta) continue;
                        Assert.IsNotNull(Bul(a, d, n, y.Yol), $"{z.Key}: Daire_{n} için '{y.Yol}' yok ('{y.Kimlik}')");
                        if (!string.IsNullOrEmpty(y.KilitYolu))
                            Assert.IsNotNull(Bul(a, d, n, y.KilitYolu), $"{z.Key}: Daire_{n} için '{y.KilitYolu}' yok ('{y.Kimlik}' kilit yolu)");
                    }
                    foreach (ZincirKilidi k in z.Value.Kilitler)
                        if (k.OdaKapisi)
                            Assert.IsNotNull(d.Find("Kapilar/Kapi_" + KilitZinciri.OdaAdi(k.Hedef)), $"{z.Key}: Daire_{n}/Kapilar/Kapi_{k.Hedef} yok");
                    foreach (string oda in z.Value.KapaliOdalar)
                        Assert.IsNotNull(d.Find("Kapilar/Kapi_" + KilitZinciri.OdaAdi(oda)), $"{z.Key}: kapalı oda '{oda}' için kapı yok");
                }
        }
        finally { EditorSceneManager.CloseScene(sahne, removeScene: true); }
    }

    // #67: katın sahanlık kartının hedefleri sahanlıkta var mı (gerçek kapının farkları).
    [Test]
    public void Sahanlik_kartlarinin_hedefleri_sahanlikta_var()
    {
        Scene sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive);
        try
        {
            Transform a = null;
            foreach (GameObject g in sahne.GetRootGameObjects()) if (g.name == "Sahanlik_A") a = g.transform;
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Zincirler" }))
            {
                string yol = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject v = AssetDatabase.LoadAssetAtPath<ScriptableObject>(yol);
                var alan = v.GetType().GetField("sahanlikKarti");
                AnomaliKarti kart = alan != null ? alan.GetValue(v) as AnomaliKarti : null;
                if (kart == null) continue;
                foreach (AnomaliEtkisi e in kart.Etkiler)
                    Assert.IsNotNull(a.Find(e.hedefYol), $"{yol}: sahanlık kartının hedefi '{e.hedefYol}' yok");
            }
        }
        finally { EditorSceneManager.CloseScene(sahne, removeScene: true); }
    }

    // #62: katın kendiliğinden çalan eşyaları. Kat dosyasının sınıfı oyun derlemesinde
    // (Assembly-CSharp) olduğu için test onu alan adlarıyla, yansımayla (reflection) okur.
    [Test]
    public void Her_calan_esyanin_yolu_odasi_ve_sesi_var()
    {
        var odalar = new List<string>();
        foreach (DairePlani.Oda o in DairePlani.Odalar) { odalar.Add(o.Ad); odalar.Add(KilitZinciri.IkinciOnek + o.Ad); }
        Scene sahne = EditorSceneManager.OpenScene(SahneYolu, OpenSceneMode.Additive);
        try
        {
            Transform a = null;
            foreach (GameObject g in sahne.GetRootGameObjects()) if (g.name == "Sahanlik_A") a = g.transform;
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Zincirler" }))
            {
                string yol = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject v = AssetDatabase.LoadAssetAtPath<ScriptableObject>(yol);
                var alan = v.GetType().GetField("sesler");
                if (alan == null) continue;
                foreach (object ses in (System.Collections.IEnumerable)alan.GetValue(v))
                {
                    var tur = ses.GetType();
                    string sesYolu = (string)tur.GetField("Yol").GetValue(ses);
                    string oda = (string)tur.GetField("Oda").GetValue(ses);
                    Object klip = (Object)tur.GetField("Klip").GetValue(ses);
                    Assert.IsTrue(klip != null, $"{yol}: '{sesYolu}' için ses dosyası bağlı değil");
                    Assert.Contains(oda, odalar, $"{yol}: '{sesYolu}' için oda '{oda}' planda yok");
                    for (int n = 1; n <= 4; n++)
                        Assert.IsNotNull(Bul(a, a.Find($"Daire_{n}"), n, sesYolu), $"{yol}: Daire_{n} için '{sesYolu}' yok");
                }
            }
        }
        finally { EditorSceneManager.CloseScene(sahne, removeScene: true); }
    }
}
