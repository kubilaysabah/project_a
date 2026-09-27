// Cep.cs
// Kapıcının cebi: sahnedeki tek Envanter ve değişince haber veren olay (#52).
//
// Dil, Ayarlar ve Defter gibi "static": her yerden Cep.Al(...), Cep.Kullan(...) denir.
// Kural Cekirdek/Envanter.cs'de (test ediliyor); burası yalnızca köprü ve bildirim.
// Köşedeki eşya simgesi (EsyaGostergesi) Degisti olayına abone olur.

public static class Cep
{
    private static readonly Envanter envanter = new Envanter();

    /// <summary>Eldeki eşya değişti (alındı, kullanıldı, boşaltıldı).</summary>
    public static event System.Action Degisti;

    public static EsyaTuru Tasinan { get { return envanter.Tasinan; } }
    public static int Kat { get { return envanter.Kat; } }
    public static string Kimlik { get { return envanter.Kimlik; } }
    public static bool ElBos { get { return envanter.ElBos; } }

    public static bool Tasiyor(EsyaTuru tur, int kat = 0) { return envanter.Tasiyor(tur, kat); }
    public static bool KimligiTasiyor(string kimlik) { return envanter.KimligiTasiyor(kimlik); }

    public static bool Al(EsyaTuru tur, int kat = 0, string kimlik = "")
    {
        if (!envanter.Al(tur, kat, kimlik)) return false;
        Degisti?.Invoke();
        return true;
    }

    public static bool Kullan(EsyaTuru tur, int kat = 0)
    {
        if (!envanter.Kullan(tur, kat)) return false;
        Degisti?.Invoke();
        return true;
    }

    public static bool KimlikleKullan(string kimlik)
    {
        if (!envanter.KimlikleKullan(kimlik)) return false;
        Degisti?.Invoke();
        return true;
    }

    public static void Bosalt()
    {
        envanter.Bosalt();
        Degisti?.Invoke();
    }
}
