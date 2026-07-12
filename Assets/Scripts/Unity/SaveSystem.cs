using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public static class SaveSystem
    {
        const string ScoreKey = "zen.score";
        const string LinesKey = "zen.lines";
        const string BoardKey = "zen.board";

        public static void Load(GameState g)
        {
            long.TryParse(PlayerPrefs.GetString(ScoreKey, "0"), out long score);
            int lines = PlayerPrefs.GetInt(LinesKey, 0);
            g.Score.Load(score, lines);

            var board = PlayerPrefs.GetString(BoardKey, "");
            if (!string.IsNullOrEmpty(board)) g.LoadBoard(board);
        }

        public static void Save(GameState g)
        {
            PlayerPrefs.SetString(ScoreKey, g.Score.Score.ToString());
            PlayerPrefs.SetInt(LinesKey, g.Score.TotalLines);
            PlayerPrefs.SetString(BoardKey, g.Board.Serialize());
            PlayerPrefs.Save();
        }
    }
}
