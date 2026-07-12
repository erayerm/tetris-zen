using UnityEngine;

namespace ZenTetris.Unity
{
    // Yuvarlatılmış dikdörtgen doku üretimi. Kenarlar SDF ile yumuşatılır (anti-alias).
    public static class RoundedTex
    {
        // (px,py) pikselinin, [lo..hi] kare bölgesinin r yarıçaplı yuvarlatılmış
        // hâlinin içinde kapladığı oran (0..1). Kenarda ~1px yumuşak geçiş.
        public static float Coverage(float px, float py, int lo, int hi, float r)
        {
            float c = (lo + hi) * 0.5f;
            float half = (hi - lo) * 0.5f;
            float qx = Mathf.Abs(px - c) - half + r;
            float qy = Mathf.Abs(py - c) - half + r;
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                                       Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            float sd = outside + inside - r;   // <0 içeride
            return Mathf.Clamp01(0.5f - sd);
        }

        public static Texture2D NewTex(int w, int h, FilterMode fm, bool mip = false)
            => new Texture2D(w, h, TextureFormat.RGBA32, mip) { filterMode = fm };

        // 9-slice ile ölçeklenebilen yuvarlak köşeli panel sprite'ı (beyaz basılır;
        // gerçek renk SpriteRenderer.color ile verilir -> tema geçişinde lerp edilebilir).
        // radiusPx dokunun yarısını aşmamalı; aşarsa 9-slice'ın orta dilimi kalmaz.
        public static Sprite RoundedPanel(int radiusPx)
        {
            const int s = 256;
            radiusPx = Mathf.Clamp(radiusPx, 2, s / 2 - 2);
            var tex = NewTex(s, s, FilterMode.Bilinear);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float cov = Coverage(x + 0.5f, y + 0.5f, 0, s - 1, radiusPx);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, cov));
                }
            tex.Apply();
            var border = new Vector4(radiusPx, radiusPx, radiusPx, radiusPx);
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s, 0,
                                 SpriteMeshType.FullRect, border);
        }
    }
}
