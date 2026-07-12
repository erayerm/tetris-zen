using System.Collections.Generic;
using UnityEngine;

namespace ZenTetris.Unity
{
    public static class BlockSprites
    {
        public const int PPU = 128;  // yüksek çözünürlük -> pürüzsüz kenarlar
        const int Margin = 8;   // hücreler arası boşluk
        const int Radius = 28;  // köşe yuvarlaklığı

        static readonly Dictionary<int, Sprite> solid = new();
        static readonly Dictionary<int, Sprite> ghost = new();

        public static Color32 ColorOf(int colorIndex) => Theme.Blocks[colorIndex];

        public static Sprite Solid(int colorIndex) => GetSolid(colorIndex);
        public static Sprite Ghost(int colorIndex) => GetGhost(colorIndex);

        // Yuvarlak köşeli, boşluklu, mat düz dolgu + hafif üst parlaklık.
        static Sprite GetSolid(int colorIndex)
        {
            if (solid.TryGetValue(colorIndex, out var s)) return s;

            var baseColor = (Color)Theme.Blocks[colorIndex];
            var highlight = baseColor * 1.12f; highlight.a = 1f;

            int lo = Margin, hi = PPU - 1 - Margin;
            var tex = RoundedTex.NewTex(PPU, PPU, FilterMode.Trilinear, mip: true);
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    float cov = RoundedTex.Coverage(x + 0.5f, y + 0.5f, lo, hi, Radius);
                    Color c = (y >= hi - 8) ? highlight : baseColor;
                    c.a = cov;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Cache(solid, colorIndex, tex);
        }

        // Hayalet: yuvarlak köşeli renkli dış çerçeve, içi şeffaf.
        static Sprite GetGhost(int colorIndex)
        {
            if (ghost.TryGetValue(colorIndex, out var s)) return s;

            var col = (Color)Theme.Blocks[colorIndex];
            const float ghostAlpha = 0.35f;

            int lo = Margin, hi = PPU - 1 - Margin, t = 12;
            var tex = RoundedTex.NewTex(PPU, PPU, FilterMode.Trilinear, mip: true);
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    float outer = RoundedTex.Coverage(x + 0.5f, y + 0.5f, lo, hi, Radius);
                    float inner = RoundedTex.Coverage(x + 0.5f, y + 0.5f, lo + t, hi - t, Radius - 8);
                    float ring = Mathf.Clamp01(outer - inner);   // yumuşak çerçeve
                    tex.SetPixel(x, y, new Color(col.r, col.g, col.b, ring * ghostAlpha));
                }
            tex.Apply();
            return Cache(ghost, colorIndex, tex);
        }

        static Sprite Cache(Dictionary<int, Sprite> cache, int colorIndex, Texture2D tex)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, PPU, PPU), new Vector2(0.5f, 0.5f), PPU);
            cache[colorIndex] = sprite;
            return sprite;
        }
    }
}
