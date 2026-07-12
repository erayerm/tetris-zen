using TMPro;
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class HudUI : MonoBehaviour
    {
        GameState state;
        TextMeshPro scoreText;
        TextMeshPro levelText;
        TextMeshPro pausedText;

        public void Init(GameState s)
        {
            state = s;

            // Panel etiketleri (mockup'taki gibi)
            MakeLabel("HOLD", new Vector3(-3.5f, 20.2f, 0), Theme.TextMuted);
            MakeLabel("NEXT", new Vector3(12.5f, 20.2f, 0), Theme.TextMuted);

            // Skor tasarımı: skor küçük/soluk üstte, seviye büyük altta, elmas sırası en altta
            scoreText = MakeText("Score", new Vector3(5f, -0.6f, 0), 2.4f, Theme.TextMuted);
            levelText = MakeText("Level", new Vector3(5f, -1.75f, 0), 6f, Theme.TextPrimary);
            MakePips(new Vector3(5f, -2.7f, 0));

            pausedText = MakeText("Paused", new Vector3(5f, 10f, 0), 8f, Theme.Accent);
            pausedText.text = "PAUSED";

            state.Changed += Redraw;
            Redraw();
        }

        TextMeshPro MakeText(string name, Vector3 pos, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.sortingOrder = 10;
            return t;
        }

        void MakeLabel(string text, Vector3 pos, Color color)
        {
            var t = MakeText("Label_" + text, pos, 2.2f, color);
            t.text = text;
            t.characterSpacing = 12f;
        }

        // 4 küçük elmas (◆ ◇ ◇ □): ilki dolu, diğerleri çerçeve.
        void MakePips(Vector3 center)
        {
            var filled = PipSprite(true);
            var outline = PipSprite(false);
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Pip" + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = center + new Vector3((i - 1.5f) * 0.75f, 0, 0);
                go.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                go.transform.localScale = Vector3.one * 0.42f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = i == 0 ? filled : outline;
                sr.color = Theme.Accent;
                sr.sortingOrder = 10;
            }
        }

        static Sprite PipSprite(bool filled)
        {
            const int n = 16;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var on = Color.white;
            var off = new Color(0, 0, 0, 0);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    bool border = x < 2 || y < 2 || x >= n - 2 || y >= n - 2;
                    tex.SetPixel(x, y, (filled || border) ? on : off);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        void Redraw()
        {
            scoreText.text = state.Score.Score.ToString("N0");
            levelText.text = state.Score.Level.ToString();
            pausedText.gameObject.SetActive(state.Paused);
        }

        void Update() // Paused, Changed tetiklemeden değişebilir
        {
            if (state != null && pausedText.gameObject.activeSelf != state.Paused)
                pausedText.gameObject.SetActive(state.Paused);
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
