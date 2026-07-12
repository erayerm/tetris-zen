using UnityEngine;

namespace ZenTetris.Unity
{
    // Cozy görsel tema değerleri. İlk sürüm: "Kahve molası".
    // İleride seviye atladıkça arkaplan değiştirmek için bir tema listesi haline gelecek.
    public static class Theme
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // Arkaplan degradesi (radial vignette): merkez -> kenar
        public static readonly Color BackgroundCenter = Hex("#6B5442");
        public static readonly Color BackgroundEdge = Hex("#38291F");

        public static readonly Color BoardBackground = Hex("#241A13");

        // Yan paneller ve grid çizgileri: sıcak beyaz, düşük alfa
        public static readonly Color Panel = new Color(1f, 0.933f, 0.863f, 0.06f);
        public static readonly Color GridLine = new Color(1f, 0.910f, 0.831f, 0.05f);

        public static readonly Color TextPrimary = Hex("#F3E7D8");
        public static readonly Color TextMuted = Hex("#C9AB93");
        public static readonly Color Accent = Hex("#E0A06E");

        // Blok paleti (pastel, mat düz). colorIndex 1..7 = I,O,T,S,Z,J,L
        public static readonly Color32[] Blocks =
        {
            new(0, 0, 0, 0),            // 0: boş
            new(124, 199, 192, 255),    // 1: I  #7CC7C0
            new(231, 206, 114, 255),    // 2: O  #E7CE72
            new(190, 134, 200, 255),    // 3: T  #BE86C8
            new(166, 199, 118, 255),    // 4: S  #A6C776
            new(226, 128, 136, 255),    // 5: Z  #E28088
            new(124, 143, 221, 255),    // 6: J  #7C8FDD
            new(224, 160, 110, 255),    // 7: L  #E0A06E
        };
    }
}
