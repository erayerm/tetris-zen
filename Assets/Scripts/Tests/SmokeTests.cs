using NUnit.Framework;
using ZenTetris.Core;

public class SmokeTests
{
    [Test]
    public void Gravity_ClampsAtMax()
    {
        Assert.AreEqual(0.5f, GameConfig.GravityFor(1));
        Assert.AreEqual(3f, GameConfig.GravityFor(999));
    }
}
