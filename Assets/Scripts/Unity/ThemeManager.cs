using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    // Seviye değiştikçe temayı yumuşak (fade) geçişle değiştirir.
    // Board/yazı/vurgu renkleri lerp; arkaplan degradesi çapraz-geçiş.
    public sealed class ThemeManager : MonoBehaviour
    {
        GameState state;
        SpriteRenderer boardPanel;
        HudUI hud;
        Camera cam;
        SpriteRenderer bgA, bgB;

        ThemePalette from, to;
        float t;
        bool animating;
        int lastLevel;

        const float Duration = 0.8f;

        public void Init(GameState s, SpriteRenderer boardPanel, HudUI hud, Camera cam)
        {
            state = s;
            this.boardPanel = boardPanel;
            this.hud = hud;
            this.cam = cam;

            bgA = MakeBg("BackgroundA", -5);
            bgB = MakeBg("BackgroundB", -4);

            lastLevel = state.Score.Level;
            from = to = Theme.ForLevel(lastLevel);
            ApplyInstant(to);
        }

        SpriteRenderer MakeBg(string name, int order)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            go.transform.position = new Vector3(5f, 10f, 0);
            go.transform.localScale = new Vector3(64f, 40f, 1f);
            return sr;
        }

        void ApplyInstant(ThemePalette p)
        {
            bgA.sprite = Gradient(p.BgCenter, p.BgEdge);
            bgA.color = Color.white;
            bgB.color = new Color(1, 1, 1, 0);
            boardPanel.color = p.BoardBg;
            cam.backgroundColor = p.BgEdge;
            hud.ApplyColors(p.TextPrimary, p.TextMuted, p.Accent);
        }

        void StartTransition(ThemePalette next)
        {
            // Geçiş ortasındaysak, sıçramayı önlemek için mevcut ara paletten başla.
            from = animating ? LerpPalette(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t))) : to;
            to = next;
            t = 0f;
            animating = true;
            // Yeni degradeyi üstteki katmana koy, şeffaftan başlat (eskinin üstüne yavaşça biner)
            bgB.sprite = Gradient(to.BgCenter, to.BgEdge);
            bgB.color = new Color(1, 1, 1, 0);
        }

        void Update()
        {
            if (state == null) return;

            int lvl = state.Score.Level;
            if (lvl != lastLevel)
            {
                lastLevel = lvl;
                StartTransition(Theme.ForLevel(lvl));
            }

            if (!animating) return;

            t += Time.deltaTime / Duration;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            boardPanel.color = Color.Lerp(from.BoardBg, to.BoardBg, k);
            cam.backgroundColor = Color.Lerp(from.BgEdge, to.BgEdge, k);
            hud.ApplyColors(
                Color.Lerp(from.TextPrimary, to.TextPrimary, k),
                Color.Lerp(from.TextMuted, to.TextMuted, k),
                Color.Lerp(from.Accent, to.Accent, k));
            bgB.color = new Color(1, 1, 1, k);

            if (t >= 1f)
            {
                animating = false;
                // Yeni degradeyi tabana taşı, üst katmanı gizle
                bgA.sprite = bgB.sprite;
                bgA.color = Color.white;
                bgB.color = new Color(1, 1, 1, 0);
            }
        }

        static ThemePalette LerpPalette(ThemePalette a, ThemePalette b, float k) => new ThemePalette
        {
            BgCenter = Color.Lerp(a.BgCenter, b.BgCenter, k),
            BgEdge = Color.Lerp(a.BgEdge, b.BgEdge, k),
            BoardBg = Color.Lerp(a.BoardBg, b.BoardBg, k),
            TextPrimary = Color.Lerp(a.TextPrimary, b.TextPrimary, k),
            TextMuted = Color.Lerp(a.TextMuted, b.TextMuted, k),
            Accent = Color.Lerp(a.Accent, b.Accent, k),
        };

        // Radial vignette: üstte sıcak, kenarlara/aşağıya koyulaşan degrade.
        static Sprite Gradient(Color center, Color edge)
        {
            const int size = 256;
            var tex = RoundedTex.NewTex(size, size, FilterMode.Bilinear);
            tex.wrapMode = TextureWrapMode.Clamp;
            float cx = 0.5f, cy = 0.85f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float ny = (float)y / (size - 1);
                    float dx = (nx - cx) * 0.85f;
                    float dy = ny - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float tt = Mathf.Clamp01(d / 0.72f);
                    tex.SetPixel(x, y, Color.Lerp(center, edge, tt));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
