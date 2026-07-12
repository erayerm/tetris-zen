using System.Collections.Generic;
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class PiecePreviewUI : MonoBehaviour
    {
        GameState state;
        Vector3 holdOrigin, nextOrigin;
        readonly List<SpriteRenderer> pool = new();
        int used;

        public void Init(GameState s, Vector3 hold, Vector3 next)
        {
            state = s;
            holdOrigin = hold;
            nextOrigin = next;
            state.Changed += Redraw;
            Redraw();
        }

        SpriteRenderer Rent()
        {
            if (used < pool.Count) { pool[used].gameObject.SetActive(true); return pool[used++]; }
            var go = new GameObject("preview");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            pool.Add(sr);
            used++;
            return sr;
        }

        void DrawPiece(TetrominoType type, Vector3 origin, float scale, Color tint)
        {
            int color = Tetromino.ColorIndex(type);
            foreach (var (x, y) in Tetromino.Cells(type, 0))
            {
                var sr = Rent();
                sr.sprite = BlockSprites.Solid(color);
                sr.color = tint;
                sr.transform.localPosition = origin + new Vector3(x * scale, y * scale, 0);
                sr.transform.localScale = Vector3.one * scale;
            }
        }

        void Redraw()
        {
            used = 0;
            if (state.Held.HasValue)
                DrawPiece(state.Held.Value, holdOrigin, 0.8f, Color.white);
            for (int i = 0; i < state.NextQueue.Count; i++)
                DrawPiece(state.NextQueue[i], nextOrigin + new Vector3(0, -i * 3f, 0), 0.8f, Color.white);
            for (int i = used; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
