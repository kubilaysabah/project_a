// KagitDokusu.cs
// Not ve pano kâğıtlarının dokusu (#128). Kâğıt düz bej bir dikdörtgendi; muadil oyunlarda kâğıt lifli,
// sararmış, kenarları koyu. Doku Art/Dokular/scripts/kagit_uret.py ile üretilir (saf Python), Resources/Dokular
// altında durur ki OnGUI çizimleri adıyla yükleyebilsin. Doku yoksa eski düz renk kalır (Al null döner).
//
// KULLANIM: GUI.color = KagitDokusu.Renk; GUI.DrawTexture(kagit, KagitDokusu.Al() ?? Texture2D.whiteTexture);

using UnityEngine;

public static class KagitDokusu
{
    private static Texture2D doku;
    private static bool arandi;

    /// <summary>Kâğıt dokusu; Resources/Dokular/Kagit yoksa null (çağıran düz renge düşer).</summary>
    public static Texture2D Al()
    {
        if (!arandi) { arandi = true; doku = Resources.Load<Texture2D>("Dokular/Kagit"); }
        return doku;
    }

    /// <summary>Dokuyla çarpılan renk: doku zaten sararmış, hafif çarpan; doku yoksa eski bej.</summary>
    public static Color Renk { get { return Al() != null ? new Color(0.98f, 0.97f, 0.94f, 1f) : new Color(0.86f, 0.82f, 0.70f, 1f); } }
}
