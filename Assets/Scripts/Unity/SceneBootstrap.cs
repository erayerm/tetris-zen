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
            cam.backgroundColor = Theme.BackgroundEdge;

            // Arkaplan degradesi (sıcak radial vignette)
            var bg = new GameObject("Background");
            var bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = MakeGradientSprite(Theme.BackgroundCenter, Theme.BackgroundEdge);
            bgsr.sortingOrder = -3;
            bg.transform.position = new Vector3(5f, 10f, 0);
            bg.transform.localScale = new Vector3(64f, 40f, 1f);

            // Board arkaplanı (yuvarlak köşeli sıcak koyu panel, hücreleri çerçeveler)
            MakeRoundedPanel("BoardPanel", new Vector3(5f, 10f, 0), new Vector2(10.6f, 20.6f),
                             Theme.BoardBackground, 0.5f, -2);

            // Boş hücreler (yuvarlak, boşluklu, soluk)
            var cells = new GameObject("Cells");
            var csr = cells.AddComponent<SpriteRenderer>();
            csr.sprite = MakeCellsSprite();
            csr.sortingOrder = -1;
            cells.transform.position = new Vector3(5f, 10f, 0);

            // Yan paneller (yuvarlak köşeli)
            MakeRoundedPanel("HoldPanel", new Vector3(-3.5f, 17.5f, 0), new Vector2(4f, 4f),
                             Theme.Panel, 0.35f, -1);
            MakeRoundedPanel("NextPanel", new Vector3(12.5f, 11.5f, 0), new Vector2(4f, 16f),
                             Theme.Panel, 0.35f, -1);

            // Bileşenler
            var renderer = new GameObject("BoardRenderer").AddComponent<BoardRenderer>();
            renderer.Init(state);

            var preview = new GameObject("Previews").AddComponent<PiecePreviewUI>();
            preview.Init(state, new Vector3(-3.5f, 16.6f, 0), new Vector3(12.5f, 16.3f, 0));

            var hud = new GameObject("Hud").AddComponent<HudUI>();
            hud.Init(state);

            var controller = gameObject.AddComponent<GameController>();
            controller.Init(state);
        }

        static void MakeRoundedPanel(string name, Vector3 pos, Vector2 size, Color color,
                                     float radiusUnits, int order)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            int radiusPx = Mathf.RoundToInt(radiusUnits * 64f); // sprite 1 birim = 64px
            sr.sprite = RoundedTex.RoundedPanel(color, radiusPx);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = order;
            go.transform.position = pos;
        }

        // Radial vignette: üstte sıcak, kenarlara/aşağıya doğru koyulaşan degrade.
        static Sprite MakeGradientSprite(Color center, Color edge)
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
                    float dy = (ny - cy);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(d / 0.72f);
                    tex.SetPixel(x, y, Color.Lerp(center, edge, t));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // Görünür board hücrelerini yuvarlak köşeli, boşluklu, soluk kareler olarak çizer.
        static Sprite MakeCellsSprite()
        {
            const int ppu = BlockSprites.PPU;
            const int margin = 2, radius = 7;
            int w = Board.Width * ppu, h = Board.VisibleHeight * ppu;
            var tex = RoundedTex.NewTex(w, h, FilterMode.Bilinear);
            var clear = new Color(0, 0, 0, 0);
            var cell = Theme.EmptyCell;
            for (int py = 0; py < h; py++)
                for (int px = 0; px < w; px++)
                {
                    int lx = px % ppu, ly = py % ppu;
                    tex.SetPixel(px, py,
                        RoundedTex.Inside(lx, ly, margin, ppu - 1 - margin, radius) ? cell : clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
