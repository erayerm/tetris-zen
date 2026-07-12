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
            var cells = Tetromino.Cells(type, 0);

            // Parçayı sınırlayıcı kutusunun merkezine göre ortala.
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var (x, y) in cells)
            {
                if (x < minX) minX = x; if (x > maxX) maxX = x;
                if (y < minY) minY = y; if (y > maxY) maxY = y;
            }
            float cx = (minX + maxX) / 2f, cy = (minY + maxY) / 2f;

            foreach (var (x, y) in cells)
            {
                var sr = Rent();
                sr.sprite = BlockSprites.Solid(color);
                sr.color = tint;
                sr.transform.localPosition = origin + new Vector3((x - cx) * scale, (y - cy) * scale, 0);
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
