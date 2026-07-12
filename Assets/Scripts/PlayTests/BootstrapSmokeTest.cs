using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ZenTetris.Unity;

public class BootstrapSmokeTest
{
    // Drives the real player loop: SceneBootstrap builds the scene, GameController
    // ticks each frame. Any runtime exception (null ref, missing Tilemap/TMP, etc.)
    // fails the test via LogAssert's implicit "no unexpected error logs" check.
    [UnityTest]
    public IEnumerator Bootstrap_RunsManyFrames_WithoutExceptions()
    {
        var boot = new GameObject("Bootstrap").AddComponent<SceneBootstrap>();

        // Let Start() run and then advance ~120 frames of gameplay.
        for (int i = 0; i < 120; i++)
            yield return null;

        // Board renderer and HUD should have been created as child/other objects.
        Assert.IsNotNull(Object.FindObjectOfType<BoardRenderer>(), "BoardRenderer not created");
        Assert.IsNotNull(Object.FindObjectOfType<HudUI>(), "HudUI not created");
        Assert.IsNotNull(Object.FindObjectOfType<GameController>(), "GameController not created");

        Object.Destroy(boot.gameObject);
        yield return null;
    }
}
