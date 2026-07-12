using System.Linq;
using NUnit.Framework;
using ZenTetris.Core;

public class BagRandomizerTests
{
    [Test]
    public void EverySevenPieces_ContainAllTypes()
    {
        var bag = new BagRandomizer(seed: 42);
        for (int round = 0; round < 10; round++)
        {
            var seven = Enumerable.Range(0, 7).Select(_ => bag.Next()).ToArray();
            CollectionAssert.AreEquivalent(
                System.Enum.GetValues(typeof(TetrominoType)).Cast<TetrominoType>(), seven);
        }
    }

    [Test]
    public void SameSeed_SameSequence()
    {
        var a = new BagRandomizer(7);
        var b = new BagRandomizer(7);
        for (int i = 0; i < 21; i++) Assert.AreEqual(a.Next(), b.Next());
    }

    [Test]
    public void DifferentSeeds_DifferAtLeastOnce()
    {
        var a = new BagRandomizer(1);
        var b = new BagRandomizer(2);
        bool differs = Enumerable.Range(0, 21).Any(_ => a.Next() != b.Next());
        Assert.IsTrue(differs);
    }
}
