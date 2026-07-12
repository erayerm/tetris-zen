using System.Collections.Generic;
using UnityEngine;

namespace ZenTetris.Unity
{
    public static class BlockSprites
    {
        public const int PPU = 32;
        static readonly Dictionary<int, Sprite> solid = new();
        static readonly Dictionary<int, Sprite> ghost = new();

        public static Color32 ColorOf(int colorIndex) => Theme.Blocks[colorIndex];

        public static Sprite Solid(int colorIndex) => GetSolid(colorIndex);
        public static Sprite Ghost(int colorIndex) => GetGhost(colorIndex);

        // Tam kare, mat düz dolgu: hafif üst parlaklık + ince koyu kenar (hücre ayrımı).
        static Sprite GetSolid(int colorIndex)
        {
            if (solid.TryGetValue(colorIndex, out var s)) return s;

            var baseColor = (Color)Theme.Blocks[colorIndex];
            var highlight = baseColor * 1.12f; highlight.a = 1f;
            var edge = baseColor * 0.82f; edge.a = 1f;

            var tex = NewTex();
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    Color c = baseColor;
                    if (y >= PPU - 2) c = highlight;                               // üst kenar
                    else if (x == 0 || y == 0 || x == PPU - 1 || y == PPU - 1) c = edge; // 1px çerçeve
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Cache(solid, colorIndex, tex);
        }

        // Hayalet (düşüş önizlemesi): dış kenara yakın renkli çerçeve, içi şeffaf.
        static Sprite GetGhost(int colorIndex)
        {
            if (ghost.TryGetValue(colorIndex, out var s)) return s;

            var col = (Color)Theme.Blocks[colorIndex];
            var clear = new Color(0, 0, 0, 0);
            const int border = 3;

            var tex = NewTex();
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    bool onBorder = x < border || y < border || x >= PPU - border || y >= PPU - border;
                    tex.SetPixel(x, y, onBorder ? new Color(col.r, col.g, col.b, 0.55f) : clear);
                }
            tex.Apply();
            return Cache(ghost, colorIndex, tex);
        }

        static Texture2D NewTex() =>
            new Texture2D(PPU, PPU, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

        static Sprite Cache(Dictionary<int, Sprite> cache, int colorIndex, Texture2D tex)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, PPU, PPU), new Vector2(0.5f, 0.5f), PPU);
            cache[colorIndex] = sprite;
            return sprite;
        }
    }
}
