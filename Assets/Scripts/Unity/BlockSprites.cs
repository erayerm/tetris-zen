using System.Collections.Generic;
using UnityEngine;

namespace ZenTetris.Unity
{
    public static class BlockSprites
    {
        public const int PPU = 32;
        static readonly Dictionary<int, Sprite> solid = new();
        static readonly Dictionary<int, Sprite> ghost = new();

        static readonly Color32[] Colors =
        {
            new(0, 0, 0, 0),          // 0: boş
            new(50, 213, 200, 255),   // 1: I
            new(230, 196, 64, 255),   // 2: O
            new(184, 74, 200, 255),   // 3: T
            new(150, 200, 60, 255),   // 4: S
            new(230, 75, 85, 255),    // 5: Z
            new(75, 100, 230, 255),   // 6: J
            new(230, 138, 50, 255),   // 7: L
        };

        public static Color32 ColorOf(int colorIndex) => Colors[colorIndex];

        public static Sprite Solid(int colorIndex) => Get(solid, colorIndex, 1f);
        public static Sprite Ghost(int colorIndex) => Get(ghost, colorIndex, 0.3f);

        static Sprite Get(Dictionary<int, Sprite> cache, int colorIndex, float alpha)
        {
            if (cache.TryGetValue(colorIndex, out var s)) return s;

            var baseColor = (Color)Colors[colorIndex];
            var tex = new Texture2D(PPU, PPU, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            var inner = baseColor * 1.15f; inner.a = 1f;   // iç parlak çerçeve
            var edge = baseColor * 0.7f; edge.a = 1f;      // dış koyu kenar
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    Color c = baseColor;
                    bool outerRim = x < 2 || y < 2 || x >= PPU - 2 || y >= PPU - 2;
                    bool innerRim = !outerRim && (x < 5 || y < 5 || x >= PPU - 5 || y >= PPU - 5);
                    if (outerRim) c = edge;
                    else if (innerRim) c = inner;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, PPU, PPU), new Vector2(0.5f, 0.5f), PPU);
            cache[colorIndex] = sprite;
            return sprite;
        }
    }
}
