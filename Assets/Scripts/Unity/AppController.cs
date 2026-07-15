using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    // Uygulama akışı: Menü -> Geri sayım -> Oynanış -> Pause.
    public sealed class AppController : MonoBehaviour
    {
        enum AppState { Menu, Countdown, Playing, Paused }

        GameState state;
        GameController controller;
        AppState st;
        float countdown;

        // Menü teması (sabit "Sıcak alacakaranlık")
        static readonly ThemePalette Dusk = Theme.Palettes[0];
        static readonly Color Cream = Dusk.TextPrimary;
        static readonly Color Muted = Dusk.TextMuted;
        static readonly Color Accent = Dusk.Accent;
        static readonly Color OnAccent = new Color(0.16f, 0.12f, 0.14f);
        static readonly Color PanelBtn = new Color(0.96f, 0.92f, 1f, 0.07f);

        // World overlay (board merkezinde geri sayım / PAUSED)
        TextMeshPro centerText;
        SpriteRenderer dim;

        static readonly Color CardBg = new Color(0.20f, 0.15f, 0.19f, 1f);   // opak panel
        static readonly Color IconBtnBg = new Color(0.10f, 0.07f, 0.10f, 0.5f);

        // uGUI
        GameObject menuPanel, howToPanel, gameUiPanel;
        RectTransform headerRect;
        TextMeshProUGUI statText;
        Image pauseIcon;
        Sprite roundSprite, solidSprite, spPause, spPlay, spHome;

        public void Init(GameState s, GameController c)
        {
            state = s;
            controller = c;

            roundSprite = RoundedTex.RoundedPanel(48);
            solidSprite = SolidSprite();
            spPause = IconPause();
            spPlay = IconPlay();
            spHome = IconHome();

            BuildWorldOverlay();
            BuildCanvas();
            ShowMenu();
        }

        // ---------- Durum geçişleri ----------

        void ShowMenu()
        {
            st = AppState.Menu;
            confirming = false;
            newGameArmed = false;
            controller.enabled = false;
            state.Paused = true;
            SaveSystem.Save(state);
            statText.text = $"Last score {state.Score.Score:N0}  ·  Level {state.Score.Level}";
            menuPanel.SetActive(true);
            if (headerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(headerRect);
            howToPanel.SetActive(false);
            gameUiPanel.SetActive(false);
            centerText.gameObject.SetActive(false);
            dim.gameObject.SetActive(false);
        }

        void StartGame(bool newGame)
        {
            confirming = false;
            newGameArmed = false;
            if (newGame) state.Reset();
            menuPanel.SetActive(false);
            howToPanel.SetActive(false);
            gameUiPanel.SetActive(true);
            dim.gameObject.SetActive(false);
            BeginCountdown();
        }

        void BeginCountdown()
        {
            st = AppState.Countdown;
            controller.enabled = false;
            state.Paused = true;
            countdown = 3f;
            centerText.gameObject.SetActive(true);
            centerText.text = "3";
        }

        void StartPlaying()
        {
            st = AppState.Playing;
            centerText.gameObject.SetActive(false);
            dim.gameObject.SetActive(false);
            state.Paused = false;
            controller.enabled = true;
            SetPauseIcon(false);
        }

        void Pause()
        {
            st = AppState.Paused;
            controller.enabled = false;
            state.Paused = true;
            dim.gameObject.SetActive(true);
            centerText.gameObject.SetActive(true);
            centerText.text = "PAUSED";
            SetPauseIcon(true);
        }

        void Resume()
        {
            st = AppState.Playing;
            controller.enabled = true;
            state.Paused = false;
            dim.gameObject.SetActive(false);
            centerText.gameObject.SetActive(false);
            SetPauseIcon(false);
        }

        void TogglePause()
        {
            if (st == AppState.Playing) Pause();
            else if (st == AppState.Paused) Resume();
        }

        // Pause'dayken "devam" (play) ikonu, oynarken "duraklat" ikonu.
        void SetPauseIcon(bool paused)
        {
            if (pauseIcon != null) pauseIcon.sprite = paused ? spPlay : spPause;
        }

        void Update()
        {
            if (st == AppState.Countdown)
            {
                countdown -= Time.deltaTime;
                int n = Mathf.CeilToInt(countdown);
                if (n <= 0) { StartPlaying(); return; }
                centerText.text = n.ToString();
                return;
            }

            if (st == AppState.Playing || st == AppState.Paused)
            {
                var kb = Keyboard.current;
                var gp = Gamepad.current;
                bool pausePressed = (kb?.escapeKey.wasPressedThisFrame ?? false)
                                 || (gp?.startButton.wasPressedThisFrame ?? false);
                if (pausePressed) TogglePause();
            }
        }

        // ---------- World overlay ----------

        void BuildWorldOverlay()
        {
            // Karartma (pause'da board üstünde)
            var dGo = new GameObject("PauseDim");
            dim = dGo.AddComponent<SpriteRenderer>();
            dim.sprite = solidSprite;
            dim.color = new Color(0.10f, 0.07f, 0.10f, 0.55f);
            dim.sortingOrder = 15;
            dGo.transform.position = new Vector3(5f, 10f, 0);
            dGo.transform.localScale = new Vector3(10.6f, 20.6f, 1f);
            dGo.SetActive(false);

            var cGo = new GameObject("CenterText");
            centerText = cGo.AddComponent<TextMeshPro>();
            FontProvider.Apply(centerText);
            centerText.fontSize = 16f;
            centerText.color = Accent;
            centerText.alignment = TextAlignmentOptions.Center;
            centerText.sortingOrder = 16;
            cGo.transform.position = new Vector3(5f, 10f, 0);
            cGo.SetActive(false);
        }

        // ---------- uGUI ----------

        void BuildCanvas()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            BuildMenuPanel(canvas.transform);
            BuildHowToPanel(canvas.transform);
            BuildGameUiPanel(canvas.transform);
        }

        void BuildMenuPanel(Transform parent)
        {
            menuPanel = FullPanel("MenuPanel", parent);
            var bg = menuPanel.AddComponent<Image>();
            bg.sprite = solidSprite;
            bg.color = Dusk.BgEdge;

            var grad = MakeImage("Grad", menuPanel.transform, GradientSprite(Dusk.BgCenter, Dusk.BgEdge), Color.white);
            Stretch(grad.rectTransform);

            BuildHeader(menuPanel.transform);

            MenuButton("Continue", 70, true, () => StartGame(false));
            MenuButton("New Game", 10, false, ConfirmNewGame);
            MenuButton("How to Play", -50, false, () => howToPanel.SetActive(true));
            var quit = MenuButton("Quit", -110, false, DoQuit);
            if (Application.platform == RuntimePlatform.WebGLPlayer) quit.SetActive(false);

            statText = Label("Stat", menuPanel.transform, "", 26, Muted, new Vector2(0, -210), new Vector2(800, 40));
        }

        // Başlık: T-parçası logosu + kalın "ZEN TETRIS" (onaylanan tasarım).
        void BuildHeader(Transform parent)
        {
            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(parent, false);
            var hrt = (RectTransform)header.transform;
            headerRect = hrt;
            hrt.anchoredPosition = new Vector2(0, 205);
            hrt.sizeDelta = new Vector2(760, 96);
            var h = header.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.spacing = 20;
            h.childControlWidth = false; h.childControlHeight = false;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            // T logosu (mor bloklar)
            var logo = new GameObject("Logo", typeof(RectTransform));
            logo.transform.SetParent(header.transform, false);
            var lrt = (RectTransform)logo.transform;
            const float bs = 26f, gap = 2f;
            lrt.sizeDelta = new Vector2(3 * bs + 2 * gap, 2 * bs + gap);
            logo.AddComponent<LayoutElement>().preferredWidth = lrt.sizeDelta.x;
            logo.GetComponent<LayoutElement>().preferredHeight = lrt.sizeDelta.y;
            var tSprite = BlockSprites.Solid(3); // T = mor
            foreach (var (cx, cy) in new[] { (0, 0), (1, 0), (2, 0), (1, 1) })
            {
                var b = new GameObject("blk", typeof(RectTransform));
                b.transform.SetParent(logo.transform, false);
                var img = b.AddComponent<Image>();
                img.sprite = tSprite; img.color = Color.white;
                var brt = img.rectTransform;
                brt.sizeDelta = new Vector2(bs, bs);
                brt.anchorMin = brt.anchorMax = brt.pivot = Vector2.zero;
                brt.anchoredPosition = new Vector2(cx * (bs + gap), cy * (bs + gap));
            }

            // Başlık (kalın)
            var title = new GameObject("Title", typeof(RectTransform));
            title.transform.SetParent(header.transform, false);
            var t = title.AddComponent<TextMeshProUGUI>();
            FontProvider.Apply(t);
            t.text = "ZEN TETRIS";
            t.fontSize = 62;
            t.color = Cream;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Left;
            var csf = title.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // İlk karede yazı genişliği hesaplanmadan yerleşme bug'ını önle.
            t.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(hrt);
        }

        // "Yeni Oyun" onayı: statText'i geçici olarak onay satırına çevir.
        bool confirming;
        void ConfirmNewGame()
        {
            if (confirming) return;
            confirming = true;
            statText.text = "Saved progress will be erased — press New Game again.";
            // ikinci basışı yakalamak için buton davranışını değiştir
            newGameArmed = true;
        }
        bool newGameArmed;

        void BuildHowToPanel(Transform parent)
        {
            howToPanel = FullPanel("HowToPanel", parent);
            var shade = howToPanel.AddComponent<Image>();
            shade.sprite = solidSprite;
            shade.color = new Color(0.06f, 0.04f, 0.06f, 0.85f);

            var panel = MakeImage("Card", howToPanel.transform, roundSprite, CardBg); // opak
            panel.type = Image.Type.Sliced;
            var prt = panel.rectTransform;
            prt.sizeDelta = new Vector2(720, 420);
            prt.anchoredPosition = Vector2.zero;

            Label("HowTitle", panel.transform, "How to Play", 34, Cream, new Vector2(0, 160), new Vector2(600, 50));

            string kb = "Move    ← →\nSoft drop    ↓\nHard drop    Space\nRotate    ↑ · X · Z\nHold    C · Shift\nPause    Esc";
            string gp = "Move    D-pad / stick\nSoft drop    down\nHard drop    Y\nRotate    A · B · X\nHold    LB · RB\nPause    Start";
            Label("KbCol", panel.transform, "Keyboard\n\n" + kb, 22, Cream, new Vector2(-170, -10), new Vector2(320, 320));
            Label("GpCol", panel.transform, "Gamepad\n\n" + gp, 22, Cream, new Vector2(170, -10), new Vector2(320, 320));

            var back = Button("Back", panel.transform, "← Back", false, new Vector2(0, -175), new Vector2(160, 44),
                              () => howToPanel.SetActive(false), out _);
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -175);

            howToPanel.SetActive(false);
        }

        void BuildGameUiPanel(Transform parent)
        {
            gameUiPanel = FullPanel("GameUiPanel", parent);
            // Sağ üst köşe: ikon-only butonlar (koddan çizilmiş ikonlar)
            IconButton("PauseBtn", gameUiPanel.transform, spPause, new Vector2(-68, -16), TogglePause, out pauseIcon);
            IconButton("HomeBtn", gameUiPanel.transform, spHome, new Vector2(-14, -16), ShowMenu, out _);
            gameUiPanel.SetActive(false);
        }

        void IconButton(string name, Transform parent, Sprite icon, Vector2 pos, System.Action onClick, out Image iconImg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.sprite = roundSprite; bg.type = Image.Type.Sliced; bg.color = IconBtnBg;
            var rt = bg.rectTransform;
            rt.sizeDelta = new Vector2(46, 46);
            AnchorTopRight(rt, pos);
            var btn = go.AddComponent<Button>();
            var act = onClick; btn.onClick.AddListener(() => act());

            var ic = new GameObject("Icon", typeof(RectTransform));
            ic.transform.SetParent(go.transform, false);
            iconImg = ic.AddComponent<Image>();
            iconImg.sprite = icon; iconImg.color = Cream;
            iconImg.raycastTarget = false;
            var irt = iconImg.rectTransform;
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(24, 24);
            irt.anchoredPosition = Vector2.zero;
        }

        void DoQuit()
        {
            Application.Quit(); // Editörde no-op; Windows build'de kapatır (WebGL'de buton gizli)
        }

        // ---------- uGUI yardımcıları ----------

        GameObject FullPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            return go;
        }

        Image MakeImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color,
                              Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            FontProvider.Apply(t);
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            var rt = t.rectTransform;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = pos;
            return t;
        }

        GameObject MenuButton(string text, float y, bool primary, System.Action onClick)
        {
            var go = Button(text + "Btn", menuPanel.transform, text, primary,
                            new Vector2(0, y), new Vector2(280, 48), () => OnMenuButton(text, onClick), out _);
            return go;
        }

        // Yeni Oyun onay akışı: silme uyarısından sonra ikinci basış sıfırlar.
        void OnMenuButton(string text, System.Action onClick)
        {
            if (text == "New Game" && newGameArmed)
            {
                newGameArmed = false; confirming = false;
                StartGame(true);
                return;
            }
            if (text != "New Game") { newGameArmed = false; confirming = false; }
            onClick();
        }

        GameObject Button(string name, Transform parent, string label, bool primary, Vector2 pos, Vector2 size,
                          System.Action onClick, out TextMeshProUGUI lbl)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = roundSprite;
            img.type = Image.Type.Sliced;
            img.color = primary ? Accent : PanelBtn;
            var rt = img.rectTransform;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var btn = go.AddComponent<Button>();
            var act = onClick;
            btn.onClick.AddListener(() => act());

            lbl = Label("Label", go.transform, label, 22, primary ? OnAccent : Cream, Vector2.zero, size);
            return go;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static void AnchorTopRight(RectTransform rt, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = pos;
        }

        static Sprite SolidSprite()
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        // ---------- İkonlar (koddan çizilir, beyaz; renk Image.color ile) ----------

        const int IcN = 64;

        static Texture2D NewIcon()
        {
            var t = new Texture2D(IcN, IcN, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < IcN; y++)
                for (int x = 0; x < IcN; x++) t.SetPixel(x, y, clear);
            return t;
        }
        static Sprite IconSprite(Texture2D t) => Sprite.Create(t, new Rect(0, 0, IcN, IcN), new Vector2(0.5f, 0.5f), IcN);

        static Sprite IconPause()
        {
            var t = NewIcon();
            for (int y = 14; y <= 50; y++)
                for (int x = 0; x < IcN; x++)
                    if ((x >= 18 && x <= 28) || (x >= 36 && x <= 46)) t.SetPixel(x, y, Color.white);
            t.Apply(); return IconSprite(t);
        }

        static Sprite IconPlay()
        {
            var t = NewIcon();
            var a = new Vector2(22, 12); var b = new Vector2(22, 52); var c = new Vector2(50, 32);
            for (int y = 0; y < IcN; y++)
                for (int x = 0; x < IcN; x++)
                    if (InTriangle(x + 0.5f, y + 0.5f, a, b, c)) t.SetPixel(x, y, Color.white);
            t.Apply(); return IconSprite(t);
        }

        static Sprite IconHome()
        {
            var t = NewIcon();
            var apex = new Vector2(32, 54); var bl = new Vector2(10, 32); var br = new Vector2(54, 32);
            for (int y = 0; y < IcN; y++)
                for (int x = 0; x < IcN; x++)
                {
                    bool roof = InTriangle(x + 0.5f, y + 0.5f, apex, bl, br);
                    bool body = x >= 18 && x <= 46 && y >= 12 && y <= 32;
                    bool door = x >= 28 && x <= 36 && y >= 12 && y <= 26; // kapı boşluğu
                    if ((roof || body) && !door) t.SetPixel(x, y, Color.white);
                }
            t.Apply(); return IconSprite(t);
        }

        static bool InTriangle(float px, float py, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(px, py, a, b), d2 = Cross(px, py, b, c), d3 = Cross(px, py, c, a);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
        static float Cross(float px, float py, Vector2 a, Vector2 b)
            => (px - b.x) * (a.y - b.y) - (a.x - b.x) * (py - b.y);

        static Sprite GradientSprite(Color center, Color edge)
        {
            const int s = 256;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float nx = (float)x / (s - 1) - 0.5f, ny = (float)y / (s - 1) - 0.18f;
                    float d = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny) / 0.85f);
                    tex.SetPixel(x, y, Color.Lerp(center, edge, d));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        }
    }
}
