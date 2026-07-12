namespace ZenTetris.Core
{
    public readonly struct Piece
    {
        public readonly TetrominoType Type;
        public readonly int Rotation;
        public readonly int X;
        public readonly int Y;

        public Piece(TetrominoType type, int rotation, int x, int y)
        {
            Type = type;
            Rotation = ((rotation % 4) + 4) % 4;
            X = x;
            Y = y;
        }

        public Piece Moved(int dx, int dy) => new(Type, Rotation, X + dx, Y + dy);
        public Piece Rotated(int newRotation) => new(Type, newRotation, X, Y);

        public (int x, int y)[] AbsoluteCells()
        {
            var rel = Tetromino.Cells(Type, Rotation);
            var abs = new (int x, int y)[4];
            for (int i = 0; i < 4; i++) abs[i] = (X + rel[i].x, Y + rel[i].y);
            return abs;
        }
    }
}
