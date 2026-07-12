namespace ZenTetris.Core
{
    public enum TSpinKind { None, Mini, Full }

    public sealed class ScoreSystem
    {
        public long Score { get; private set; }
        public int TotalLines { get; private set; }
        public int Combo { get; private set; } = -1;
        public bool BackToBack { get; private set; }
        public int Level => TotalLines / 10 + 1;

        static readonly int[] LineBase = { 0, 100, 300, 500, 800 };
        static readonly int[] TSpinBase = { 400, 800, 1200, 1600 };
        static readonly int[] TSpinMiniBase = { 100, 200 };

        public void OnPieceLocked(int linesCleared, TSpinKind tspin)
        {
            int level = Level; // silmeden ÖNCEKİ seviye ile puanla

            long points = tspin switch
            {
                TSpinKind.Full => TSpinBase[linesCleared],
                TSpinKind.Mini => TSpinMiniBase[System.Math.Min(linesCleared, 1)],
                _ => LineBase[linesCleared],
            };

            if (linesCleared > 0)
            {
                Combo++;
                bool difficult = linesCleared == 4 || tspin != TSpinKind.None;
                if (difficult && BackToBack) points = points * 3 / 2;
                if (Combo > 0) points += 50L * Combo;
                BackToBack = difficult;
                Score += points * level;
                TotalLines += linesCleared;
            }
            else
            {
                Combo = -1;
                if (tspin != TSpinKind.None) Score += points * level; // satırsız T-spin puanı
            }
        }

        public void AddDropPoints(int cells, bool hard) => Score += cells * (hard ? 2 : 1);

        public void Load(long score, int totalLines)
        {
            Score = score;
            TotalLines = totalLines;
            Combo = -1;
            BackToBack = false;
        }
    }
}
