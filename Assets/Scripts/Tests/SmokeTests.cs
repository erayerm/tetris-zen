using NUnit.Framework;
using ZenTetris.Core;

public class SmokeTests
{
    [Test]
    public void Gravity_ClampsAtMax()
    {
        Assert.AreEqual(1f, GameConfig.GravityFor(1));
        Assert.AreEqual(12f, GameConfig.GravityFor(999));
    }
}
