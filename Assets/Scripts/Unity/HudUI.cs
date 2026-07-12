using System.Collections.Generic;
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
        readonly List<TextMeshPro> labels = new();      // HOLD / NEXT (muted)
        readonly List<SpriteRenderer> pips = new();      // accent

        public void Init(GameState s)
        {
            state = s;

            MakeLabel("HOLD", new Vector3(-2.7f, 19.5f, 0));
            MakeLabel("NEXT", new Vector3(12.7f, 19.5f, 0));

            scoreText = MakeText("Score", new Vector3(5f, -1.1f, 0), 4.5f);
            levelText = MakeText("Level", new Vector3(5f, -2.9f, 0), 13f);
            MakePips(new Vector3(5f, -4.2f, 0));

            pausedText = MakeText("Paused", new Vector3(5f, 10f, 0), 12f);
            pausedText.text = "PAUSED";

            state.Changed += Redraw;
            Redraw();
        }

        // Tema geçişinde ThemeManager çağırır.
        public void ApplyColors(Color primary, Color muted, Color accent)
        {
            if (scoreText != null) scoreText.color = muted;
            if (levelText != null) levelText.color = primary;
            if (pausedText != null) pausedText.color = accent;
            foreach (var l in labels) l.color = muted;
            foreach (var p in pips) p.color = accent;
        }

        TextMeshPro MakeText(string name, Vector3 pos, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            FontProvider.Apply(t);
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.sortingOrder = 10;
            return t;
        }

        void MakeLabel(string text, Vector3 pos)
        {
            var t = MakeText("Label_" + text, pos, 5.32f);
            t.text = text;
            t.characterSpacing = 10f;
            labels.Add(t);
        }

        void MakePips(Vector3 center)
        {
            var filled = PipSprite(true);
            var outline = PipSprite(false);
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Pip" + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = center + new Vector3((i - 1.5f) * 0.95f, 0, 0);
                go.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                go.transform.localScale = Vector3.one * 0.6f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = i == 0 ? filled : outline;
                sr.sortingOrder = 10;
                pips.Add(sr);
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

        void Update()
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
