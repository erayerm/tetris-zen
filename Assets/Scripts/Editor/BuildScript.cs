using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Batchmode build giriş noktaları:
//   -executeMethod BuildScript.BuildWindows
//   -executeMethod BuildScript.BuildWebGL
public static class BuildScript
{
    static readonly string[] Scenes = { "Assets/Scenes/SampleScene.unity" };

    public static void BuildWindows()
    {
        Run(new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = "Build/Windows/ZenTetris.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        });
    }

    public static void BuildWebGL()
    {
        // GitHub Pages / statik hosting Brotli'yi doğru header'la sunmaz;
        // decompression fallback ile loader kendi çözer.
        PlayerSettings.WebGL.decompressionFallback = true;

        Run(new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = "Build/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
    }

    static void Run(BuildPlayerOptions opts)
    {
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        var s = report.summary;
        Debug.Log($"Build {s.result}: {s.totalSize} bytes, {s.totalErrors} errors, output: {s.outputPath}");
        if (s.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
