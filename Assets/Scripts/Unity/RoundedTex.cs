using UnityEngine;

namespace ZenTetris.Unity
{
    // Yuvarlatılmış dikdörtgen doku üretimi için ortak yardımcılar.
    public static class RoundedTex
    {
        // (x,y) pikseli, [lo..hi] karesinin r yarıçaplı yuvarlatılmış hâlinin içinde mi?
        public static bool Inside(int x, int y, int lo, int hi, int r)
        {
            if (x < lo || x > hi || y < lo || y > hi) return false;
            int left = lo + r, right = hi - r, bottom = lo + r, top = hi - r;
            if (x < left && y < bottom) return Dist(x, y, left, bottom) <= r;
            if (x > right && y < bottom) return Dist(x, y, right, bottom) <= r;
            if (x < left && y > top) return Dist(x, y, left, top) <= r;
            if (x > right && y > top) return Dist(x, y, right, top) <= r;
            return true;
        }

        static float Dist(int x, int y, int ax, int ay)
        {
            float dx = x - ax, dy = y - ay;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static Texture2D NewTex(int w, int h, FilterMode fm)
            => new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = fm };

        // 9-slice ile ölçeklenebilen yuvarlak köşeli panel sprite'ı (kenarlar bozulmaz).
        // radiusPx dokunun yarısını aşmamalı; aşarsa 9-slice'ın orta dilimi kalmaz.
        public static Sprite RoundedPanel(Color color, int radiusPx)
        {
            const int s = 256; // yüksek çözünürlük -> pürüzsüz köşe
            radiusPx = Mathf.Clamp(radiusPx, 2, s / 2 - 2);
            var tex = NewTex(s, s, FilterMode.Bilinear);
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                    tex.SetPixel(x, y, Inside(x, y, 0, s - 1, radiusPx) ? color : clear);
            tex.Apply();
            var border = new Vector4(radiusPx, radiusPx, radiusPx, radiusPx);
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s, 0,
                                 SpriteMeshType.FullRect, border);
        }
    }
}
