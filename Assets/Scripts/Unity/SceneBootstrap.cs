using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class SceneBootstrap : MonoBehaviour
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoBoot()
        {
            var go = new UnityEngine.GameObject("Bootstrap");
            go.AddComponent<SceneBootstrap>();
        }

        void Start()
        {
            var state = new GameState();
            SaveSystem.Load(state.Score);

            // Kamera
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 13f;
            cam.transform.position = new Vector3(5f, 10f, -10f);
            cam.backgroundColor = Theme.BackgroundEdge; // degrade kadraj dışında kalırsa

            // Arkaplan degradesi (sıcak radial vignette)
            var bg = new GameObject("Background");
            var bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = MakeGradientSprite(Theme.BackgroundCenter, Theme.BackgroundEdge);
            bgsr.sortingOrder = -3;
            bg.transform.position = new Vector3(5f, 10f, 0);
            bg.transform.localScale = new Vector3(64f, 40f, 1f);

            // Board arkaplan paneli (sıcak koyu zemin)
            var panel = new GameObject("BoardPanel");
            var psr = panel.AddComponent<SpriteRenderer>();
            psr.sprite = MakeSolidSprite(Theme.BoardBackground);
            psr.sortingOrder = -1;
            panel.transform.position = new Vector3(5f, 10f, 0);
            panel.transform.localScale = new Vector3(10f, 20f, 1f);

            // Grid çizgileri
            var grid = new GameObject("GridLines");
            var gsr = grid.AddComponent<SpriteRenderer>();
            gsr.sprite = MakeGridSprite();
            gsr.sortingOrder = 0;
            grid.transform.position = new Vector3(5f, 10f, 0);

            // Yan paneller (Hold / Next arkaplanı)
            MakePanel("HoldPanel", new Vector3(-3.5f, 17.5f, 0), new Vector3(4f, 4f, 1f));
            MakePanel("NextPanel", new Vector3(12.5f, 11.5f, 0), new Vector3(4f, 16f, 1f));

            // Bileşenler
            var renderer = new GameObject("BoardRenderer").AddComponent<BoardRenderer>();
            renderer.Init(state);

            var preview = new GameObject("Previews").AddComponent<PiecePreviewUI>();
            preview.Init(state, new Vector3(-3.5f, 17.5f, 0), new Vector3(12.5f, 17.5f, 0));

            var hud = new GameObject("Hud").AddComponent<HudUI>();
            hud.Init(state);

            var controller = gameObject.AddComponent<GameController>();
            controller.Init(state);
        }

        static void MakePanel(string name, Vector3 pos, Vector3 scale)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite(Theme.Panel);
            sr.sortingOrder = -1;
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        static Sprite MakeSolidSprite(Color c)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        // Radial vignette: merkez sıcak, kenarlara doğru koyulaşan degrade.
        static Sprite MakeGradientSprite(Color center, Color edge)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            // Merkez üst-ortada, yatayda hafif geniş (mockup: 120% 90% at 50% ~15%)
            float cx = 0.5f, cy = 0.85f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float ny = (float)y / (size - 1);
                    float dx = (nx - cx) * 0.85f; // yatayda daha geniş yayılım
                    float dy = (ny - cy);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(d / 0.72f);
                    tex.SetPixel(x, y, Color.Lerp(center, edge, t));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeGridSprite()
        {
            const int ppu = 32;
            int w = Board.Width * ppu, h = Board.VisibleHeight * ppu;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color(0, 0, 0, 0);
            var line = Theme.GridLine;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, (x % ppu == 0 || y % ppu == 0) ? line : clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
