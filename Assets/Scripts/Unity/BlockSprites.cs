using System.Collections.Generic;
using UnityEngine;

namespace ZenTetris.Unity
{
    public static class BlockSprites
    {
        public const int PPU = 32;
        static readonly Dictionary<int, Sprite> solid = new();
        static readonly Dictionary<int, Sprite> ghost = new();

        const int Margin = 1;   // hücreler arası ince saydam boşluk
        const int Radius = 6;   // köşe yuvarlaklığı

        public static Color32 ColorOf(int colorIndex) => Theme.Blocks[colorIndex];

        public static Sprite Solid(int colorIndex) => Get(solid, colorIndex, 1f);
        public static Sprite Ghost(int colorIndex) => Get(ghost, colorIndex, 0.28f);

        static Sprite Get(Dictionary<int, Sprite> cache, int colorIndex, float alpha)
        {
            if (cache.TryGetValue(colorIndex, out var s)) return s;

            var baseColor = (Color)Theme.Blocks[colorIndex];
            var highlight = baseColor * 1.14f; highlight.a = 1f; // üstte ince açık kenar

            var tex = new Texture2D(PPU, PPU, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            int lo = Margin, hi = PPU - 1 - Margin;
            var clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    if (x < lo || x > hi || y < lo || y > hi || !InRoundedRect(x, y, lo, hi))
                    {
                        tex.SetPixel(x, y, clear);
                        continue;
                    }

                    Color c = (y >= hi - 2) ? highlight : baseColor; // üst kenar parlaklığı
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, PPU, PPU), new Vector2(0.5f, 0.5f), PPU);
            cache[colorIndex] = sprite;
            return sprite;
        }

        // Yuvarlatılmış dikdörtgen içi mi? Köşelerde çeyrek daire testi.
        static bool InRoundedRect(int x, int y, int lo, int hi)
        {
            int r = Radius;
            int left = lo + r, right = hi - r, bottom = lo + r, top = hi - r;
            float cx = x, cy = y;
            if (x < left && y < bottom) return Dist(cx, cy, left, bottom) <= r;
            if (x > right && y < bottom) return Dist(cx, cy, right, bottom) <= r;
            if (x < left && y > top) return Dist(cx, cy, left, top) <= r;
            if (x > right && y > top) return Dist(cx, cy, right, top) <= r;
            return true;
        }

        static float Dist(float x, float y, float ax, float ay)
        {
            float dx = x - ax, dy = y - ay;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
