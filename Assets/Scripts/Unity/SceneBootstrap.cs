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
            cam.backgroundColor = new Color(0.16f, 0.35f, 0.20f); // placeholder arkaplan

            // Board arkaplan paneli (yarı saydam siyah)
            var panel = new GameObject("BoardPanel");
            var psr = panel.AddComponent<SpriteRenderer>();
            psr.sprite = MakeSolidSprite(new Color(0f, 0f, 0f, 0.82f));
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
            sr.sprite = MakeSolidSprite(new Color(0f, 0f, 0f, 0.7f));
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

        static Sprite MakeGridSprite()
        {
            const int ppu = 32;
            int w = Board.Width * ppu, h = Board.VisibleHeight * ppu;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color(0, 0, 0, 0);
            var line = new Color(1f, 1f, 1f, 0.06f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, (x % ppu == 0 || y % ppu == 0) ? line : clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
