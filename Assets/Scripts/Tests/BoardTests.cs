using NUnit.Framework;
using ZenTetris.Core;

public class BoardTests
{
    [Test]
    public void NewBoard_IsEmpty()
    {
        var b = new Board();
        Assert.IsTrue(b.IsEmpty);
        Assert.AreEqual(0, b.Get(0, 0));
    }

    [Test]
    public void OutOfBounds_IsOccupied()
    {
        var b = new Board();
        Assert.IsTrue(b.IsOccupied(-1, 0));
        Assert.IsTrue(b.IsOccupied(10, 0));
        Assert.IsTrue(b.IsOccupied(0, -1));
        Assert.IsTrue(b.IsOccupied(0, 40));
        Assert.IsFalse(b.IsOccupied(0, 0));
    }

    [Test]
    public void CanPlace_RejectsWallOverlap()
    {
        var b = new Board();
        Assert.IsTrue(b.CanPlace(TetrominoType.T, 0, 4, 1));
        Assert.IsFalse(b.CanPlace(TetrominoType.I, 0, 8, 1)); // sağ hücre x=10 -> duvar
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, -1, 1)); // sol hücre x=-2 -> duvar
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, 4, -1)); // zemin altı
    }

    [Test]
    public void Lock_WritesColorIndex()
    {
        var b = new Board();
        b.Lock(new Piece(TetrominoType.T, 0, 4, 1));
        Assert.AreEqual(Tetromino.ColorIndex(TetrominoType.T), b.Get(4, 1));
        Assert.AreEqual(Tetromino.ColorIndex(TetrominoType.T), b.Get(4, 2));
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, 4, 1));
    }

    [Test]
    public void ClearFullLines_RemovesRowAndShiftsDown()
    {
        var b = new Board();
        // y=0 satırını tek hücrelik kilitlemelerle doldur, y=1'e bir işaret koy
        for (int x = 0; x < Board.Width; x++) b.SetCell(x, 0, 3);
        b.SetCell(5, 1, 7);
        int cleared = b.ClearFullLines();
        Assert.AreEqual(1, cleared);
        Assert.AreEqual(7, b.Get(5, 0)); // üst satır aşağı kaydı
        Assert.AreEqual(0, b.Get(5, 1));
    }

    [Test]
    public void ClearFullLines_MultipleRows()
    {
        var b = new Board();
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < Board.Width; x++) b.SetCell(x, y, 1);
        b.SetCell(0, 2, 5);
        Assert.AreEqual(2, b.ClearFullLines());
        Assert.AreEqual(5, b.Get(0, 0));
    }

    [Test]
    public void ClearAll_EmptiesBoard()
    {
        var b = new Board();
        b.SetCell(3, 3, 2);
        b.ClearAll();
        Assert.IsTrue(b.IsEmpty);
    }
}
