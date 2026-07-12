namespace ZenTetris.Core
{
    // Board üzerinde tek bir hücre: konum + renk indeksi (juice/olaylar için).
    public readonly struct Cell
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Color;

        public Cell(int x, int y, int color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}
