// KatKaplamalari.cs
// Her katın dairesinin kaplama takımı (#53): oda duvarı, parke, ıslak zemin, ıslak duvar.
//
// ---------- NEDEN KATA BAĞLI ----------
// Şahan'ın isteği (2026-09-27): oyuncu her katta "yine aynı dairedeyim" hissine kapılmasın.
// Sahnede dört daire nesnesi var ve sekiz katta sırayla açılıyorlar (1. ve 5. katta aynı
// "2 numara" açılır). Kaplama daireye bağlı olsaydı 1. ve 5. kat aynı görünürdü. Bu yüzden
// kaplama KATA bağlı: daire her açıldığında o katın takımını giyer (KatYoneticisi).
//
// Her satır tasarım belgesindeki (Docs/hikaye.md §7) kiracıya göre seçildi: taşınmış aile
// balıksırtı parke ve bej rulo boya, yaşlı Nuri soyulan eski kâğıt, boş daire çatlak sıva...
//
// Dokular Assets/Textures/<ad>_renk.jpg ve <ad>_normal.jpg (Poly Haven ve ambientCG, CC0;
// kaynaklar Art/Dokular/LISANSLAR.md). "Boy" dokunun kapladığı gerçek alanın kenarı (metre):
// dairenin yüzeylerinin UV'si metre cinsinden, malzeme dokuyu 1/boy kez tekrar eder; böylece
// parke tahtası da fayans da gerçek boyunda görünür.

public static class KatKaplamalari
{
    /// <summary>Dairedeki kaplanan dört yüzey türü. Sıra malzeme dizisindeki sırayla aynı.</summary>
    public enum Yuzey { OdaDuvari = 0, Parke = 1, IslakZemin = 2, IslakDuvar = 3 }
    public const int YuzeySayisi = 4;

    /// <summary>Bir yüzeyin kaplaması: doku, gerçek boyu, renk tonu (0-1) ve parlaklığı.</summary>
    public struct Kaplama
    {
        public string Doku;
        public float Boy;
        public float R, G, B;
        public float Parlaklik;
        public Kaplama(string doku, float boy, float parlaklik, float r = 1f, float g = 1f, float b = 1f)
        { Doku = doku; Boy = boy; Parlaklik = parlaklik; R = r; G = g; B = b; }
    }

    // Kısaltmalar: sık kullanılan tonlar ve parlaklıklar.
    private static Kaplama Duvar(string d, float boy, float r = 1f, float g = 1f, float b = 1f) { return new Kaplama(d, boy, 0.08f, r, g, b); }
    private static Kaplama Tahta(string d, float boy) { return new Kaplama(d, boy, 0.35f); }
    private static Kaplama Karo(string d, float boy, float r = 1f, float g = 1f, float b = 1f) { return new Kaplama(d, boy, 0.50f, r, g, b); }

    // [kat - 1, yüzey]. Sekiz kat, dört yüzey. İki boyutlu dizi: satır kat, sütun yüzey.
    private static readonly Kaplama[,] Tablo =
    {
        // 1. kat: taşınmış aile (koliler). Bej rulo boya, balıksırtı parke, karo mozaik.
        { Duvar("beige_wall_002", 3.0f), Tahta("herringbone_parquet", 3.4f), Karo("terrazzo_tiles", 2.0f), Karo("long_white_tiles", 1.27f) },
        // 2. kat: hastanedeki yaşlı Nuri. Soyulan eski duvar kâğıdı, cilası gitmiş tahta, damalı mermer.
        { Duvar("decrepit_wallpaper", 2.5f), Tahta("old_wooden_floor_03", 1.2f), Karo("floor_tiles_06", 3.0f), Karo("interior_tiles", 1.9f) },
        // 3. kat: çocuklu aile. Açık maviye boyanmış kabartmalı kâğıt, hasır parke.
        { Duvar("wallpaper001a", 1.0f, 0.74f, 0.84f, 0.95f), Tahta("rectangular_parquet", 2.25f), Karo("floor_tiles_08", 1.5f), Karo("tiled_floor_001", 1.5f) },
        // 4. kat: boş ve elektriksiz. Çatlak sıva, aşınmış tahta, kırık karo.
        { Duvar("worn_cracked_plaster", 1.8f), Tahta("wood_floor_worn", 2.0f), Karo("worn_tile_floor", 2.0f), Karo("dirty_tiles", 2.26f) },
        // 5. kat: Cemal'in son çalıştığı daire. Temiz beyaz sıva, laminat.
        { Duvar("white_plaster_02", 1.0f), Tahta("laminate_floor_02", 1.7f), Karo("dirty_tiles", 2.26f), Karo("floor_tiles_08", 1.5f) },
        // 6. kat: kimse oturmuyor ama sofra kurulu. Mint yeşili kabartmalı kâğıt, çapraz parke.
        { Duvar("wallpaper002a", 1.0f, 0.76f, 0.92f, 0.82f), Tahta("diagonal_parquet", 2.36f), Karo("marble_tiles", 2.0f), Karo("long_white_tiles", 1.27f, 0.82f, 0.95f, 0.87f) },
        // 7. kat: Cemal'in eşyaları, bütün kapılar 13. Küflü kâğıt, hasarlı tahta.
        { Duvar("wallpaper002c", 1.0f), Tahta("plank_flooring", 1.34f), Karo("interior_tiles", 1.9f), Karo("dirty_tiles", 2.26f) },
        // 8. kat: Yönetim dairesi (dosyalar). Düz bej boya, düz şerit tahta.
        { Duvar("beige_wall_001", 3.0f), Tahta("wooden_floor_02", 1.94f), Karo("tiled_floor_001", 1.5f), Karo("marble_tiles", 2.0f) },
    };

    /// <summary>Tabloda kaç kat var (1'den başlar).</summary>
    public static int KatSayisi { get { return Tablo.GetLength(0); } }

    /// <summary>Bu katın bu yüzeyinin kaplaması. Tablonun dışındaki katlar en yakın kata yuvarlanır.</summary>
    public static Kaplama Getir(int kat, Yuzey yuzey)
    {
        int i = kat - 1;
        if (i < 0) i = 0;
        if (i >= KatSayisi) i = KatSayisi - 1;
        return Tablo[i, (int)yuzey];
    }
}
