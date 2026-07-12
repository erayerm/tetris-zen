namespace ZenTetris.Core
{
    public enum TetrominoType { I, O, T, S, Z, J, L }

    public static class Tetromino
    {
        // Spawn (rotasyon 0) hücreleri, pivot-göreceli, y yukarı.
        static readonly (int x, int y)[][] Spawn =
        {
            /* I */ new[] { (-1, 0), (0, 0), (1, 0), (2, 0) },
            /* O */ new[] { (0, 0), (1, 0), (0, 1), (1, 1) },
            /* T */ new[] { (-1, 0), (0, 0), (1, 0), (0, 1) },
            /* S */ new[] { (-1, 0), (0, 0), (0, 1), (1, 1) },
            /* Z */ new[] { (-1, 1), (0, 1), (0, 0), (1, 0) },
            /* J */ new[] { (-1, 1), (-1, 0), (0, 0), (1, 0) },
            /* L */ new[] { (1, 1), (-1, 0), (0, 0), (1, 0) },
        };

        // [type][rotation][cell] — statik olarak önceden hesaplanır.
        static readonly (int x, int y)[][][] All = Build();

        static (int x, int y)[][][] Build()
        {
            var all = new (int x, int y)[Spawn.Length][][];
            for (int t = 0; t < Spawn.Length; t++)
            {
                all[t] = new (int x, int y)[4][];
                all[t][0] = Spawn[t];
                for (int r = 1; r < 4; r++)
                {
                    var prev = all[t][r - 1];
                    var next = new (int x, int y)[4];
                    for (int i = 0; i < 4; i++)
                        next[i] = RotateCW((TetrominoType)t, prev[i]);
                    all[t][r] = next;
                }
            }
            return all;
        }

        static (int x, int y) RotateCW(TetrominoType type, (int x, int y) c) => type switch
        {
            TetrominoType.O => c,                      // O dönmez
            TetrominoType.I => (c.y, 1 - c.x),         // (0.5, 0.5) merkezli SRS I dönüşü
            _ => (c.y, -c.x),                          // pivot merkezli
        };

        public static (int x, int y)[] Cells(TetrominoType type, int rotation) =>
            ((int x, int y)[])All[(int)type][((rotation % 4) + 4) % 4].Clone();

        public static int ColorIndex(TetrominoType type) => (int)type + 1;
    }
}
