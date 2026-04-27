// ターミナル（Claude Code）からUnityをバッチモードで操作するEditor拡張
// 実行例: unity -batchmode -projectPath /path/to/project -executeMethod BuildAutomator.SwitchToiOS -quit
#if UNITY_EDITOR
using EscapeGame.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BuildAutomator
{
    // iOS/WebGL 共通のPlayerSettings（重複排除）
    private static void ApplyCommonPlayerSettings()
    {
        PlayerSettings.applicationIdentifier = "co.cleaz.escapegame";
        PlayerSettings.companyName = "Cleaz";
        PlayerSettings.productName = "脱出ゲーム";
    }

    [MenuItem("EscapeGame/Build/Switch to iOS")]
    public static void SwitchToiOS()
    {
        Debug.Log("[BuildAutomator] iOSビルドターゲットに切り替え開始");

        ApplyCommonPlayerSettings();
        PlayerSettings.iOS.targetOSVersionString = "15.0";

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        Debug.Log("[BuildAutomator] iOSビルドターゲット切り替え完了");
    }

    [MenuItem("EscapeGame/Build/Switch to WebGL")]
    public static void SwitchToWebGL()
    {
        Debug.Log("[BuildAutomator] WebGLビルドターゲットに切り替え開始");

        ApplyCommonPlayerSettings();
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        Debug.Log("[BuildAutomator] WebGLビルドターゲット切り替え完了");
    }

    // シーン内の全Canvasに SafeAreaHandler と CanvasScaler を自動アタッチする
    [MenuItem("EscapeGame/Build/Setup Scene for iOS")]
    public static void SetupSceneForIOS()
    {
        Debug.Log("[BuildAutomator] iOS向けシーンセットアップ開始");

        var canvases = Object.FindObjectsByType<Canvas>();
        if (canvases.Length == 0)
        {
            Debug.LogWarning("[BuildAutomator] シーン内にCanvasが見つかりません");
            return;
        }

        foreach (var canvas in canvases)
        {
            var scaler = canvas.GetOrAddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170, 2532); // iPhone 14 Pro基準
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvas.GetComponent<SafeAreaHandler>() == null)
            {
                canvas.gameObject.AddComponent<SafeAreaHandler>();
                Debug.Log($"[BuildAutomator] SafeAreaHandler を追加: {canvas.name}");
            }

            EditorUtility.SetDirty(canvas.gameObject);
        }

        Debug.Log($"[BuildAutomator] iOS向けセットアップ完了。対象Canvas数: {canvases.Length}");
    }
}

// GetOrAddComponent 拡張（BuildAutomator 内で使うユーティリティ）
internal static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this Component c) where T : Component
        => c.GetComponent<T>() ?? c.gameObject.AddComponent<T>();
}
#endif
