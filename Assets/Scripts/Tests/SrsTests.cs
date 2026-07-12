using NUnit.Framework;
using ZenTetris.Core;

public class SrsTests
{
    [Test]
    public void OpenField_RotatesWithoutKick()
    {
        var b = new Board();
        var t = new Piece(TetrominoType.T, 0, 4, 5);
        Assert.IsTrue(Srs.TryRotate(b, t, clockwise: true, out var r, out var kick));
        Assert.AreEqual(1, r.Rotation);
        Assert.AreEqual(0, kick);
        Assert.AreEqual((4, 5), (r.X, r.Y));
    }

    [Test]
    public void BlockedRotation_ReturnsFalse()
    {
        var b = new Board();
        // T'nin etrafını tamamen doldur (pivot 4,1) — hiçbir kick çalışmasın
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < 5; y++)
                if (!(x >= 3 && x <= 5 && y <= 2)) b.SetCell(x, y, 1);
        for (int x = 3; x <= 5; x++) b.SetCell(x, 2, 1); // tavan
        b.SetCell(3, 0, 1); b.SetCell(5, 0, 1);          // alt köşeler
        var t = new Piece(TetrominoType.T, 2, 4, 1); // aşağı bakan T
        Assert.IsFalse(Srs.TryRotate(b, t, true, out _, out _));
    }

    [Test]
    public void WallKick_LeftWall_JPiece()
    {
        var b = new Board();
        // Sol duvara dayalı dikey J (rotasyon 1/R), CCW dönünce duvar kick gerekir
        var j = new Piece(TetrominoType.J, 1, 0, 5);
        Assert.IsTrue(b.CanPlace(j));
        Assert.IsTrue(Srs.TryRotate(b, j, clockwise: false, out var r, out var kick));
        Assert.AreEqual(0, r.Rotation);
        Assert.Greater(kick, 0); // kick'siz sığmaz (sol hücre x=-1 olurdu)
    }

    [Test]
    public void I_UsesOwnKickTable()
    {
        var b = new Board();
        var i = new Piece(TetrominoType.I, 0, 4, 0); // zeminde yatay I
        Assert.IsTrue(Srs.TryRotate(b, i, true, out var r, out _));
        Assert.IsTrue(b.CanPlace(r));
    }
}
