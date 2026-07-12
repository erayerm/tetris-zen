using NUnit.Framework;
using ZenTetris.Core;

public class ScoreSystemTests
{
    [Test]
    public void Single_ScoresBaseTimesLevel()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(1, TSpinKind.None);
        Assert.AreEqual(100, s.Score);
        Assert.AreEqual(1, s.TotalLines);
    }

    [Test]
    public void Tetris_Then_Tetris_GetsBackToBackBonus()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(4, TSpinKind.None);          // 800
        s.OnPieceLocked(0, TSpinKind.None);          // combo kırılır ama B2B durur
        s.OnPieceLocked(4, TSpinKind.None);          // 800*1.5 = 1200
        Assert.AreEqual(800 + 1200, s.Score);
        Assert.IsTrue(s.BackToBack);
    }

    [Test]
    public void NormalClear_BreaksBackToBack()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(4, TSpinKind.None);
        s.OnPieceLocked(1, TSpinKind.None);
        Assert.IsFalse(s.BackToBack);
    }

    [Test]
    public void Combo_AddsFiftyPerStep()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(1, TSpinKind.None);          // combo 0: 100
        s.OnPieceLocked(1, TSpinKind.None);          // combo 1: 100 + 50
        s.OnPieceLocked(1, TSpinKind.None);          // combo 2: 100 + 100
        Assert.AreEqual(100 + 150 + 200, s.Score);
    }

    [Test]
    public void TSpinFull_UsesTSpinTable()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(2, TSpinKind.Full);          // 1200
        Assert.AreEqual(1200, s.Score);
        Assert.IsTrue(s.BackToBack);                 // T-spin clear B2B başlatır
    }

    [Test]
    public void TSpinMini_NoLines_Scores100()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(0, TSpinKind.Mini);
        Assert.AreEqual(100, s.Score);
    }

    [Test]
    public void Level_IncreasesEveryTenLines_AndMultiplies()
    {
        var s = new ScoreSystem();
        for (int i = 0; i < 3; i++) s.OnPieceLocked(4, TSpinKind.None); // 12 satır
        Assert.AreEqual(2, s.Level); // 12/10+1
        long before = s.Score;
        s.OnPieceLocked(1, TSpinKind.None); // Single, seviye 2 => 200 + combo yok (önceki hepsi clear, combo sürüyor!)
        // combo 3. adım: (100 + 50*3) * 2 = 500
        Assert.AreEqual(before + 500, s.Score);
    }

    [Test]
    public void DropPoints_AreFlat()
    {
        var s = new ScoreSystem();
        s.AddDropPoints(5, hard: false); // +5
        s.AddDropPoints(10, hard: true); // +20
        Assert.AreEqual(25, s.Score);
    }

    [Test]
    public void Load_RestoresProgress()
    {
        var s = new ScoreSystem();
        s.Load(5000, 25);
        Assert.AreEqual(5000, s.Score);
        Assert.AreEqual(3, s.Level);
    }
}
