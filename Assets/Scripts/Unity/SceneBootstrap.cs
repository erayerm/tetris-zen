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
            cam.orthographicSize = 14f;                     // altta skor bloğuna yer aç
            cam.transform.position = new Vector3(5f, 9f, -10f);
            cam.backgroundColor = Theme.BackgroundEdge;

            // Arkaplan degradesi (sıcak radial vignette)
            var bg = new GameObject("Background");
            var bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = MakeGradientSprite(Theme.BackgroundCenter, Theme.BackgroundEdge);
            bgsr.sortingOrder = -3;
            bg.transform.position = new Vector3(5f, 10f, 0);
            bg.transform.localScale = new Vector3(64f, 40f, 1f);

            // Tüm görünür UI tek bir kök altında -> juice bounce hepsini senkron sektirir
            // (arkaplan hariç). Board üst kenarı y=20.3; yan paneller de bu hizada.
            var uiRoot = new GameObject("UIRoot");

            // Board arkaplanı (yuvarlak köşeli sıcak koyu panel, hücreleri çerçeveler)
            var boardPanel = MakeRoundedPanel("BoardPanel", new Vector3(5f, 10f, 0),
                             new Vector2(10.6f, 20.6f), Theme.BoardBackground, 0.6f, -2);

            // Boş hücreler (yuvarlak, boşluklu, soluk)
            var cells = new GameObject("Cells");
            var csr = cells.AddComponent<SpriteRenderer>();
            csr.sprite = MakeCellsSprite();
            csr.sortingOrder = -1;
            cells.transform.position = new Vector3(5f, 10f, 0);

            // Yan paneller: üst kenarları board üst kenarıyla (20.3) hizalı;
            // board'a uzaklıkları eşit (0.4 birim = eski Next boşluğunun 2 katı).
            var holdPanel = MakeRoundedPanel("HoldPanel", new Vector3(-2.7f, 18.3f, 0),
                             new Vector2(4f, 4f), Theme.Panel, 0.4f, -1);
            var nextPanel = MakeRoundedPanel("NextPanel", new Vector3(12.7f, 12.3f, 0),
                             new Vector2(4f, 16f), Theme.Panel, 0.4f, -1);

            // Bileşenler
            var renderer = new GameObject("BoardRenderer").AddComponent<BoardRenderer>();
            renderer.Init(state);

            var preview = new GameObject("Previews").AddComponent<PiecePreviewUI>();
            preview.Init(state, new Vector3(-2.7f, 17.6f, 0), new Vector3(12.7f, 17.9f, 0));

            var hud = new GameObject("Hud").AddComponent<HudUI>();
            hud.Init(state);

            // Görünür her şeyi uiRoot altına al (arkaplan hariç)
            foreach (var t in new[] { boardPanel.transform, cells.transform, holdPanel.transform,
                                      nextPanel.transform, renderer.transform,
                                      preview.transform, hud.transform })
                t.SetParent(uiRoot.transform, true);

            // Juice: bounce + partiküller (tüm UI kökünü sektirir)
            var juice = new GameObject("Juice").AddComponent<Juice>();
            juice.Init(state, uiRoot.transform);

            var controller = gameObject.AddComponent<GameController>();
            controller.Init(state);
        }

        static GameObject MakeRoundedPanel(string name, Vector3 pos, Vector2 size, Color color,
                                           float radiusUnits, int order)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            int radiusPx = Mathf.RoundToInt(radiusUnits * 256f); // sprite 1 birim = 256px
            sr.sprite = RoundedTex.RoundedPanel(color, radiusPx);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = order;
            go.transform.position = pos;
            return go;
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
            const int ppu = 64;              // bloklarla aynı oran (margin/radius), makul doku boyutu
            const int margin = 4, radius = 14;
            int w = Board.Width * ppu, h = Board.VisibleHeight * ppu;
            var tex = RoundedTex.NewTex(w, h, FilterMode.Trilinear, mip: true);
            var cell = Theme.EmptyCell;
            for (int py = 0; py < h; py++)
                for (int px = 0; px < w; px++)
                {
                    int lx = px % ppu, ly = py % ppu;
                    float cov = RoundedTex.Coverage(lx + 0.5f, ly + 0.5f, margin, ppu - 1 - margin, radius);
                    tex.SetPixel(px, py, new Color(cell.r, cell.g, cell.b, cell.a * cov));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
