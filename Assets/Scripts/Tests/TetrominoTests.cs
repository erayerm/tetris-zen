using System.Linq;
using NUnit.Framework;
using ZenTetris.Core;

public class TetrominoTests
{
    [Test]
    public void EveryPieceEveryRotation_HasFourCells()
    {
        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            for (int r = 0; r < 4; r++)
                Assert.AreEqual(4, Tetromino.Cells(t, r).Length, $"{t} rot {r}");
    }

    [Test]
    public void O_DoesNotChangeWithRotation()
    {
        var r0 = Tetromino.Cells(TetrominoType.O, 0).OrderBy(c => (c.x, c.y)).ToArray();
        for (int r = 1; r < 4; r++)
            CollectionAssert.AreEqual(r0, Tetromino.Cells(TetrominoType.O, r).OrderBy(c => (c.x, c.y)).ToArray());
    }

    [Test]
    public void T_SpawnCells_AreCorrect()
    {
        CollectionAssert.AreEquivalent(
            new[] { (-1, 0), (0, 0), (1, 0), (0, 1) },
            Tetromino.Cells(TetrominoType.T, 0));
    }

    [Test]
    public void I_RotatedCW_IsVerticalColumn()
    {
        CollectionAssert.AreEquivalent(
            new[] { (0, 2), (0, 1), (0, 0), (0, -1) },
            Tetromino.Cells(TetrominoType.I, 1));
    }

    [Test]
    public void ColorIndex_IsTypePlusOne()
    {
        Assert.AreEqual(1, Tetromino.ColorIndex(TetrominoType.I));
        Assert.AreEqual(7, Tetromino.ColorIndex(TetrominoType.L));
    }
}
