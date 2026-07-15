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

        // uGUI
        GameObject menuPanel, howToPanel, gameUiPanel;
        TextMeshProUGUI statText, pauseLabel;
        Sprite roundSprite, solidSprite;

        public void Init(GameState s, GameController c)
        {
            state = s;
            controller = c;

            roundSprite = RoundedTex.RoundedPanel(48);
            solidSprite = SolidSprite();

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
            statText.text = $"Son skor {state.Score.Score:N0}  ·  Seviye {state.Score.Level}";
            menuPanel.SetActive(true);
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
            SetPauseLabel(false);
        }

        void Pause()
        {
            st = AppState.Paused;
            controller.enabled = false;
            state.Paused = true;
            dim.gameObject.SetActive(true);
            centerText.gameObject.SetActive(true);
            centerText.text = "PAUSED";
            SetPauseLabel(true);
        }

        void Resume()
        {
            st = AppState.Playing;
            controller.enabled = true;
            state.Paused = false;
            dim.gameObject.SetActive(false);
            centerText.gameObject.SetActive(false);
            SetPauseLabel(false);
        }

        void TogglePause()
        {
            if (st == AppState.Playing) Pause();
            else if (st == AppState.Paused) Resume();
        }

        void SetPauseLabel(bool paused) => pauseLabel.text = paused ? "Devam" : "Duraklat";

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

            Label("Title", menuPanel.transform, "ZEN TETRIS", 64, Cream, new Vector2(0, 210), new Vector2(700, 90));

            MenuButton("Devam Et", 70, true, () => StartGame(false));
            MenuButton("Yeni Oyun", 10, false, ConfirmNewGame);
            MenuButton("Nasıl Oynanır", -50, false, () => howToPanel.SetActive(true));
            var quit = MenuButton("Çıkış", -110, false, DoQuit);
            if (Application.platform == RuntimePlatform.WebGLPlayer) quit.SetActive(false);

            statText = Label("Stat", menuPanel.transform, "", 26, Muted, new Vector2(0, -210), new Vector2(800, 40));
        }

        // "Yeni Oyun" onayı: statText'i geçici olarak onay satırına çevir.
        bool confirming;
        void ConfirmNewGame()
        {
            if (confirming) return;
            confirming = true;
            statText.text = "Kayıtlı ilerleme silinecek — tekrar Yeni Oyun'a bas.";
            // ikinci basışı yakalamak için buton davranışını değiştir
            newGameArmed = true;
        }
        bool newGameArmed;

        void BuildHowToPanel(Transform parent)
        {
            howToPanel = FullPanel("HowToPanel", parent);
            var shade = howToPanel.AddComponent<Image>();
            shade.sprite = solidSprite;
            shade.color = new Color(0.10f, 0.07f, 0.10f, 0.72f);

            var panel = MakeImage("Card", howToPanel.transform, roundSprite, PanelBtn);
            panel.type = Image.Type.Sliced;
            var prt = panel.rectTransform;
            prt.sizeDelta = new Vector2(720, 420);
            prt.anchoredPosition = Vector2.zero;

            Label("HowTitle", panel.transform, "Nasıl Oynanır", 34, Cream, new Vector2(0, 160), new Vector2(600, 50));

            string kb = "Hareket    ← →\nYumuşak düşüş    ↓\nSert düşüş    Space\nDöndür    ↑ · X · Z\nHold    C · Shift\nDuraklat    Esc";
            string gp = "Hareket    D-pad / analog\nYumuşak düşüş    aşağı\nSert düşüş    Y\nDöndür    A · B · X\nHold    LB · RB\nDuraklat    Start";
            Label("KbCol", panel.transform, "Klavye\n\n" + kb, 22, Cream, new Vector2(-170, -10), new Vector2(320, 320));
            Label("GpCol", panel.transform, "Gamepad\n\n" + gp, 22, Cream, new Vector2(170, -10), new Vector2(320, 320));

            var back = Button("Back", panel.transform, "← Geri", false, new Vector2(0, -175), new Vector2(160, 44),
                              () => howToPanel.SetActive(false), out _);
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -175);

            howToPanel.SetActive(false);
        }

        void BuildGameUiPanel(Transform parent)
        {
            gameUiPanel = FullPanel("GameUiPanel", parent);
            // Sağ üst köşe butonları
            var pause = Button("PauseBtn", gameUiPanel.transform, "Duraklat", false,
                               Vector2.zero, new Vector2(120, 40), TogglePause, out pauseLabel);
            AnchorTopRight(pause.GetComponent<RectTransform>(), new Vector2(-150, -18));

            var home = Button("HomeBtn", gameUiPanel.transform, "Menü", false,
                              Vector2.zero, new Vector2(90, 40), ShowMenu, out _);
            AnchorTopRight(home.GetComponent<RectTransform>(), new Vector2(-52, -18));

            gameUiPanel.SetActive(false);
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
            if (text == "Yeni Oyun" && newGameArmed)
            {
                newGameArmed = false; confirming = false;
                StartGame(true);
                return;
            }
            if (text != "Yeni Oyun") { newGameArmed = false; confirming = false; }
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
