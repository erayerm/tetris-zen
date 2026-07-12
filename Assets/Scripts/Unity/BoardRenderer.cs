using UnityEngine;
using UnityEngine.Tilemaps;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class BoardRenderer : MonoBehaviour
    {
        GameState state;
        Tilemap blocks;
        Tilemap ghost;
        readonly Tile[] solidTiles = new Tile[8];
        readonly Tile[] ghostTiles = new Tile[8];

        public void Init(GameState s)
        {
            state = s;
            blocks = CreateLayer("Blocks", 1);
            ghost = CreateLayer("Ghost", 0);
            for (int i = 1; i <= 7; i++)
            {
                solidTiles[i] = ScriptableObject.CreateInstance<Tile>();
                solidTiles[i].sprite = BlockSprites.Solid(i);
                ghostTiles[i] = ScriptableObject.CreateInstance<Tile>();
                ghostTiles[i].sprite = BlockSprites.Ghost(i);
            }
            state.Changed += Redraw;
            Redraw();
        }

        Tilemap CreateLayer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var tm = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
            if (GetComponent<Grid>() == null) gameObject.AddComponent<Grid>();
            return tm;
        }

        public void Redraw()
        {
            blocks.ClearAllTiles();
            ghost.ClearAllTiles();

            for (int x = 0; x < Board.Width; x++)
                for (int y = 0; y < Board.VisibleHeight + 2; y++) // taşma görünürlüğü için +2
                {
                    int c = state.Board.Get(x, y);
                    if (c != 0) blocks.SetTile(new Vector3Int(x, y, 0), solidTiles[c]);
                }

            int color = Tetromino.ColorIndex(state.Active.Type);
            int gy = state.GhostY();
            foreach (var (cx, cy) in Tetromino.Cells(state.Active.Type, state.Active.Rotation))
            {
                var gpos = new Vector3Int(state.Active.X + cx, gy + cy, 0);
                if (gpos.y < Board.VisibleHeight + 2) ghost.SetTile(gpos, ghostTiles[color]);
                var apos = new Vector3Int(state.Active.X + cx, state.Active.Y + cy, 0);
                if (apos.y < Board.VisibleHeight + 2) blocks.SetTile(apos, solidTiles[color]);
            }
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
