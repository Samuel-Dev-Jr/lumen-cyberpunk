#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Script de build (apenas para o Editor). Permite gerar o executável Windows
/// pela linha de comando, sem abrir as janelas do editor. Também pode ser
/// chamado pelo menu: "Lumen > Build Windows".
/// </summary>
public static class BuildScript
{
    const string Output = "Build/Lumen.exe";

    [MenuItem("Lumen/Build Windows")]
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log("BUILD_OK :: " + summary.totalSize + " bytes :: " + summary.outputPath);
        else
            Debug.LogError("BUILD_FAILED :: " + summary.result + " :: errors=" + summary.totalErrors);

        // só encerra o processo quando rodando em batch (linha de comando)
        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
#endif
