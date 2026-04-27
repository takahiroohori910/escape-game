#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WebGLBuildHelper
{
    [MenuItem("EscapeGame/Setup/Configure WebGL Settings")]
    public static void ConfigureWebGL()
    {
        PlayerSettings.companyName   = "EscapeGame";
        PlayerSettings.productName   = "嵐の洋館";
        PlayerSettings.bundleVersion = "0.2.0";

        // カスタムテンプレートを設定（Mobile フォルダが存在する場合）
        if (AssetDatabase.IsValidFolder("Assets/WebGLTemplates/Mobile"))
        {
            PlayerSettings.WebGL.template = "PROJECT:Mobile";
            Debug.Log("[WebGL] テンプレート: Mobile");
        }

        // シーン登録
        var guids  = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" });
        var scenes = guids
            .Select(g => new EditorBuildSettingsScene(AssetDatabase.GUIDToAssetPath(g), true))
            .ToArray();
        EditorBuildSettings.scenes = scenes;

        AssetDatabase.SaveAssets();
        Debug.Log($"[WebGL] 設定完了: {scenes.Length} シーン登録");

        EditorUtility.DisplayDialog("WebGL 設定完了",
            $"設定を適用しました（{scenes.Length} シーン）。\n\n" +
            "次のステップ:\n" +
            "1. File > Build Settings を開く\n" +
            "2. Platform を WebGL に切り替え → Switch Platform\n" +
            "3. EscapeGame > Build > Build WebGL を実行",
            "OK");
    }

    [MenuItem("EscapeGame/Build/Build WebGL")]
    public static void BuildWebGL()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            EditorUtility.DisplayDialog("プラットフォーム未切り替え",
                "File > Build Settings で WebGL に切り替えてから実行してください。",
                "OK");
            return;
        }

        string buildPath = Path.Combine(
            Path.GetDirectoryName(Application.dataPath),
            "Builds", "WebGL");
        Directory.CreateDirectory(buildPath);

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[WebGL] シーン未登録。Configure WebGL Settings を先に実行してください。");
            return;
        }

        Debug.Log($"[WebGL] ビルド開始 → {buildPath}");
        var report = BuildPipeline.BuildPlayer(
            scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[WebGL] ビルド成功 → {buildPath}");
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"[WebGL] ビルド失敗: {report.summary.result}");
        }
    }
}
#endif
