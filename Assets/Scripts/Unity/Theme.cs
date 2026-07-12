using UnityEngine;

namespace ZenTetris.Unity
{
    // Temaya göre değişen renkler.
    public struct ThemePalette
    {
        public Color BgCenter, BgEdge, BoardBg, TextPrimary, TextMuted, Accent;
    }

    // Cozy görsel tema verileri. Seviye değiştikçe Palettes arasında geçilir.
    // Blok paleti ve panel/hücre tonları sabit; sadece aşağıdaki 6 renk değişir.
    public static class Theme
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // Temadan bağımsız sabitler
        public static readonly Color Panel = new Color(1f, 0.933f, 0.863f, 0.06f);
        public static readonly Color GridLine = new Color(1f, 0.910f, 0.831f, 0.05f);
        public static readonly Color EmptyCell = new Color(1f, 0.910f, 0.831f, 0.05f);

        // Blok paleti (pastel, mat düz). colorIndex 1..7 = I,O,T,S,Z,J,L
        public static readonly Color32[] Blocks =
        {
            new(0, 0, 0, 0),
            new(124, 199, 192, 255),    // I
            new(231, 206, 114, 255),    // O
            new(190, 134, 200, 255),    // T
            new(166, 199, 118, 255),    // S
            new(226, 128, 136, 255),    // Z
            new(124, 143, 221, 255),    // J
            new(224, 160, 110, 255),    // L
        };

        static ThemePalette P(string center, string edge, string board,
                              string text, string muted, string accent) => new ThemePalette
        {
            BgCenter = Hex(center), BgEdge = Hex(edge), BoardBg = Hex(board),
            TextPrimary = Hex(text), TextMuted = Hex(muted), Accent = Hex(accent)
        };

        // Kullanıcının seçtiği 7 tema (seviye ile döngüsel)
        public static readonly ThemePalette[] Palettes =
        {
            P("#6B4A63", "#3A2B3D", "#241A26", "#F3E4D8", "#C8A9B8", "#E0A06E"), // Sıcak alacakaranlık
            P("#3A3350", "#211D2E", "#191527", "#EFE8FF", "#B3A6CF", "#E7CE72"), // Gece lambası
            P("#6B5442", "#38291F", "#241A13", "#F3E7D8", "#C9AB93", "#E0A06E"), // Kahve molası
            P("#A8708A", "#4D2E40", "#2A1A22", "#F6E4EC", "#CD9FB2", "#E28088"), // Gül bahçesi
            P("#4F6B52", "#243528", "#16221A", "#EAF5E8", "#A6C4A8", "#A6C776"), // Orman zemini
            P("#A85F43", "#47281C", "#281611", "#F5E2D6", "#CF9C86", "#E28088"), // Baharat pazarı
            P("#9A6A3C", "#46311D", "#271A10", "#F4E6D4", "#CCA47C", "#E0A06E"), // Sonbahar
        };

        // level 1 -> index 0, döngüsel
        public static ThemePalette ForLevel(int level)
            => Palettes[((level - 1) % Palettes.Length + Palettes.Length) % Palettes.Length];
    }
}
