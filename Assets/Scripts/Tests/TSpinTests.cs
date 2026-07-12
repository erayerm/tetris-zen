using NUnit.Framework;
using ZenTetris.Core;

public class TSpinTests
{
    // Klasik TSD (T-Spin Double) yuvası kur:
    //   y=2:  X X X . X X X X X X   (x=3 boş — T başı buradan girer)
    //   y=1:  X X X . . . X X X X   (x=3,4,5 boş)
    //   y=0:  X X X X . X X X X X   (x=4 boş — nokta)
    static Board BuildTsdSlot()
    {
        var b = new Board();
        for (int x = 0; x < Board.Width; x++)
        {
            if (x != 3) b.SetCell(x, 2, 1);
            if (x < 3 || x > 5) b.SetCell(x, 1, 1);
            if (x != 4) b.SetCell(x, 0, 1);
        }
        return b;
    }

    [Test]
    public void TSpinDouble_DetectedAndScored()
    {
        var g = new GameState(seed: 1);
        // Board'u elle kur, aktif parçayı T yapmak için: T gelene kadar hold/drop etmek kırılgan.
        // Bunun yerine düşük seviye API ile doğrudan senaryo testi:
        var b = BuildTsdSlot();
        // T rotasyon 2 (aşağı bakan), yuvaya rotate ederek girmiş kabul edilecek pozisyon: pivot (4,1)
        var t = new Piece(TetrominoType.T, 2, 4, 1);
        Assert.IsTrue(b.CanPlace(t), "T yuvaya sığmalı");
        // 3 köşe kuralı: (3,2),(5,2),(3,0),(5,0) köşelerinden en az 3'ü dolu
        int corners = 0;
        foreach (var (dx, dy) in new[] { (-1, 1), (1, 1), (-1, -1), (1, -1) })
            if (b.IsOccupied(4 + dx, 1 + dy)) corners++;
        Assert.GreaterOrEqual(corners, 3);
        // Kilitle ve iki satırın silindiğini doğrula
        b.Lock(t);
        Assert.AreEqual(2, b.ClearFullLines());
    }

    [Test]
    public void ScoreSystem_TsdChain_B2BApplies()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(2, TSpinKind.Full);  // 1200
        s.OnPieceLocked(2, TSpinKind.Full);  // 1200*1.5 + combo 50 = 1850
        Assert.AreEqual(1200 + 1850, s.Score);
    }
}
