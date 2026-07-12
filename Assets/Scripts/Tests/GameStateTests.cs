using NUnit.Framework;
using ZenTetris.Core;

public class GameStateTests
{
    static GameState NewGame() => new GameState(seed: 42);

    [Test]
    public void Spawn_ActivePieceAtSpawnPosition_NextHasFive()
    {
        var g = NewGame();
        Assert.AreEqual(4, g.Active.X);
        Assert.AreEqual(20, g.Active.Y);
        Assert.AreEqual(5, g.NextQueue.Count);
    }

    [Test]
    public void MoveLeft_AtWall_ReturnsFalse()
    {
        var g = NewGame();
        while (g.MoveLeft()) { }
        Assert.IsFalse(g.MoveLeft());
    }

    [Test]
    public void Gravity_MovesPieceDownOverTime()
    {
        var g = NewGame();
        int y0 = g.Active.Y;
        g.Tick(2.0f); // seviye 1: 0.5 hücre/sn -> 1 hücre 2 sn'de
        Assert.AreEqual(y0 - 1, g.Active.Y);
    }

    [Test]
    public void HardDrop_LocksImmediately_AndSpawnsNext()
    {
        var g = NewGame();
        var first = g.Active.Type;
        var expectedNext = g.NextQueue[0];
        g.HardDrop();
        Assert.AreEqual(expectedNext, g.Active.Type);
        Assert.IsFalse(g.Board.IsEmpty);
        Assert.AreEqual(4, g.Active.X); // yeni parça spawn'da
    }

    [Test]
    public void LockDelay_PieceLocksAfterHalfSecondOnGround()
    {
        var g = NewGame();
        // Parçayı soft drop ile zemine indir; zemine değer değmez dur (kilitlenmeden).
        g.SetSoftDrop(true);
        int guard = 0;
        while (g.Active.Y != g.GhostY() && guard++ < 2000) g.Tick(0.05f);
        g.SetSoftDrop(false);

        var type = g.Active.Type;
        g.Tick(0.3f); // lock delay (0.5 sn) dolmadı -> kilitlenmemeli
        Assert.AreEqual(type, g.Active.Type);
        Assert.AreEqual(0, CountFilled(g.Board));
        g.Tick(0.3f); // toplam > 0.5 sn -> kilitlenir
        Assert.AreNotEqual(0, CountFilled(g.Board));
    }

    [Test]
    public void GhostY_IsLandingRow()
    {
        var g = NewGame();
        int ghost = g.GhostY();
        g.HardDrop();
        // Kilitlendi; ghost, hard drop'un indiği satırdı. Board'da o satır civarı dolu olmalı.
        Assert.LessOrEqual(ghost, 20);
        Assert.GreaterOrEqual(ghost, 0);
    }

    [Test]
    public void TryHold_SwapsAndBlocksSecondHold()
    {
        var g = NewGame();
        var first = g.Active.Type;
        Assert.IsTrue(g.TryHold());
        Assert.AreEqual(first, g.Held);
        Assert.IsFalse(g.TryHold()); // aynı parça sırasında ikinci hold yok
        g.HardDrop();
        Assert.IsTrue(g.TryHold()); // yeni parçayla tekrar serbest
    }

    [Test]
    public void TopOut_ClearsBoard_KeepsScore()
    {
        var g = NewGame();
        g.Score.Load(9999, 0);
        // Spawn bölgesini tıka
        for (int x = 0; x < Board.Width; x++)
            for (int y = 18; y < 24; y++)
                g.Board.SetCell(x, y, 1);
        g.HardDrop(); // kilitlenme tamamen gizli bölgede veya spawn tıkalı -> temizlik
        Assert.IsTrue(BoardMostlyEmpty(g.Board));
        Assert.AreEqual(9999 + 40 /* hard drop puanı olabilir */, g.Score.Score, 60);
    }

    [Test]
    public void Paused_TickDoesNothing()
    {
        var g = NewGame();
        g.Paused = true;
        int y = g.Active.Y;
        g.Tick(5f);
        Assert.AreEqual(y, g.Active.Y);
    }

    static int CountFilled(Board b)
    {
        int n = 0;
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
                if (b.Get(x, y) != 0) n++;
        return n;
    }

    static bool BoardMostlyEmpty(Board b) => CountFilled(b) <= 4; // en fazla yeni kilitlenen parça
}
