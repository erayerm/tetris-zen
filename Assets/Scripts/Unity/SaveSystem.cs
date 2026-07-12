using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public static class SaveSystem
    {
        const string ScoreKey = "zen.score";
        const string LinesKey = "zen.lines";

        public static void Load(ScoreSystem s)
        {
            long score = 0;
            long.TryParse(PlayerPrefs.GetString(ScoreKey, "0"), out score);
            int lines = PlayerPrefs.GetInt(LinesKey, 0);
            s.Load(score, lines);
        }

        public static void Save(ScoreSystem s)
        {
            PlayerPrefs.SetString(ScoreKey, s.Score.ToString());
            PlayerPrefs.SetInt(LinesKey, s.TotalLines);
            PlayerPrefs.Save();
        }
    }
}
