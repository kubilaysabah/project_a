// FotografAlbumu.cs
// Oyundaki bütün aile fotoğraflarının listesi (#69): her fotoğrafın kimliği, resmi ve arkasında
// yazan hikâyenin metin anahtarı. Tek bir dosya (Assets/Fotograflar/Album.asset); GrayboxKurucu
// kurar ve fotoğraf çerçevelerine ve menüye BAĞLAR. Oyun build'de resimleri adıyla aramaz,
// bu listeden alır (build'e yalnızca bağlı olan varlıklar girer).

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Album", menuName = "Huzur/Fotoğraf Albümü")]
public class FotografAlbumu : ScriptableObject
{
    [System.Serializable]
    public class Fotograf
    {
        public string Kimlik;       // "kat4", "kapici"
        public Texture2D Doku;      // eskitilmiş fotoğraf
        public string Arka;         // arkasındaki el yazısının metin anahtarı ("foto.kat4")
    }

    public List<Fotograf> fotograflar = new List<Fotograf>();

    /// <summary>Kimliğe göre fotoğraf; yoksa null.</summary>
    public Fotograf Bul(string kimlik)
    {
        foreach (Fotograf f in fotograflar)
            if (f != null && f.Kimlik == kimlik) return f;
        return null;
    }
}
