namespace ZenTetris.Core
{
    public static class Srs
    {
        // Kick offsetleri (dx, dy), y yukarı pozitif. Satır sırası KickRow ile eşleşir.
        static readonly (int x, int y)[][] JlstzKicks =
        {
            /* 0->R */ new[] { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },
            /* R->0 */ new[] { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },
            /* R->2 */ new[] { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },
            /* 2->R */ new[] { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },
            /* 2->L */ new[] { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },
            /* L->2 */ new[] { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },
            /* L->0 */ new[] { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },
            /* 0->L */ new[] { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },
        };

        static readonly (int x, int y)[][] IKicks =
        {
            /* 0->R */ new[] { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },
            /* R->0 */ new[] { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },
            /* R->2 */ new[] { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) },
            /* 2->R */ new[] { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },
            /* 2->L */ new[] { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },
            /* L->2 */ new[] { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },
            /* L->0 */ new[] { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },
            /* 0->L */ new[] { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) },
        };

        // (from, to) çiftini yukarıdaki tablo indeksine çevirir.
        static int KickRow(int from, bool clockwise) => (from, clockwise) switch
        {
            (0, true) => 0,  // 0->R
            (1, false) => 1, // R->0
            (1, true) => 2,  // R->2
            (2, false) => 3, // 2->R
            (2, true) => 4,  // 2->L
            (3, false) => 5, // L->2
            (3, true) => 6,  // L->0
            (0, false) => 7, // 0->L
            _ => 0
        };

        public static bool TryRotate(Board board, in Piece piece, bool clockwise,
                                     out Piece result, out int kickIndex)
        {
            int to = ((piece.Rotation + (clockwise ? 1 : -1)) % 4 + 4) % 4;

            if (piece.Type == TetrominoType.O)
            {
                result = piece.Rotated(to);
                kickIndex = 0;
                return true;
            }

            var table = piece.Type == TetrominoType.I ? IKicks : JlstzKicks;
            var kicks = table[KickRow(piece.Rotation, clockwise)];

            for (int i = 0; i < kicks.Length; i++)
            {
                var candidate = new Piece(piece.Type, to, piece.X + kicks[i].x, piece.Y + kicks[i].y);
                if (board.CanPlace(candidate))
                {
                    result = candidate;
                    kickIndex = i;
                    return true;
                }
            }
            result = piece;
            kickIndex = -1;
            return false;
        }
    }
}
