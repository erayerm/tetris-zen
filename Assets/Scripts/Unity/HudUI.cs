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
            scoreText = MakeText("Score", new Vector3(5f, -1.2f, 0), 4f);
            levelText = MakeText("Level", new Vector3(5f, -2.6f, 0), 6f);
            pausedText = MakeText("Paused", new Vector3(5f, 10f, 0), 8f);
            pausedText.text = "PAUSED";
            state.Changed += Redraw;
            Redraw();
        }

        TextMeshPro MakeText(string name, Vector3 pos, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.sortingOrder = 10;
            return t;
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
