namespace ZenTetris.Core
{
    public sealed class Board
    {
        public const int Width = 10;
        public const int VisibleHeight = 20;
        public const int Height = 40;

        readonly int[] cells = new int[Width * Height]; // 0 = boş

        public int Get(int x, int y) => cells[y * Width + x];
        public void SetCell(int x, int y, int value) => cells[y * Width + x] = value;

        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return true;
            return cells[y * Width + x] != 0;
        }

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < cells.Length; i++)
                    if (cells[i] != 0) return false;
                return true;
            }
        }

        public bool CanPlace(TetrominoType type, int rotation, int px, int py)
        {
            foreach (var (cx, cy) in Tetromino.Cells(type, rotation))
                if (IsOccupied(px + cx, py + cy)) return false;
            return true;
        }

        public bool CanPlace(in Piece p) => CanPlace(p.Type, p.Rotation, p.X, p.Y);

        public void Lock(in Piece p)
        {
            int color = Tetromino.ColorIndex(p.Type);
            foreach (var (x, y) in p.AbsoluteCells())
                if (x >= 0 && x < Width && y >= 0 && y < Height)
                    cells[y * Width + x] = color;
        }

        public int ClearFullLines()
        {
            int cleared = 0;
            for (int y = 0; y < Height; y++)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                    if (cells[y * Width + x] == 0) { full = false; break; }

                if (full) { cleared++; continue; }
                if (cleared > 0)
                    for (int x = 0; x < Width; x++)
                        cells[(y - cleared) * Width + x] = cells[y * Width + x];
            }
            for (int y = Height - cleared; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    cells[y * Width + x] = 0;
            return cleared;
        }

        public void ClearAll() => System.Array.Clear(cells, 0, cells.Length);
    }
}
