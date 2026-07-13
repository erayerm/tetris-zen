using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Batchmode build giriş noktaları:
//   -executeMethod BuildScript.BuildWindows
//   -executeMethod BuildScript.BuildWebGL
public static class BuildScript
{
    static readonly string[] Scenes = { "Assets/Scenes/SampleScene.unity" };

    // Assets/Icon.png -> uygulama ikonu (exe ikonu + WebGL favicon).
    static void ApplyIcon()
    {
        AssetDatabase.Refresh();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icon.png");
        if (tex == null) { Debug.LogWarning("Icon.png bulunamadı, varsayılan ikon kullanılacak."); return; }
        ApplyIconFor(NamedBuildTarget.Standalone, IconKind.Application, tex);
        ApplyIconFor(NamedBuildTarget.WebGL, IconKind.Any, tex);
    }

    // İlgili platformun BÜTÜN ikon boyutu slotlarını aynı görselle doldurur
    // (tek boyut vermek exe'ye ikonu gömmüyordu).
    static void ApplyIconFor(NamedBuildTarget target, IconKind kind, Texture2D tex)
    {
        var sizes = PlayerSettings.GetIconSizes(target, kind);
        if (sizes == null || sizes.Length == 0)
        {
            PlayerSettings.SetIcons(target, new[] { tex }, kind);
            return;
        }
        var icons = new Texture2D[sizes.Length];
        for (int i = 0; i < icons.Length; i++) icons[i] = tex;
        PlayerSettings.SetIcons(target, icons, kind);
    }

    public static void BuildWindows()
    {
        ApplyIcon();
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
        ApplyIcon();
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
