#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessSetup
{
    [MenuItem("EscapeGame/Setup/Post Process")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[PostFX] Edit mode で実行してください"); return; }

        SetupVolume();
        EnableCameraPostProcess();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[PostFX] ポストプロセス設定完了");
    }

    private static void SetupVolume()
    {
        var existing = GameObject.Find("GlobalPostProcessVolume");
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject("GlobalPostProcessVolume");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;

        const string dir = "Assets/_Project/VolumeProfiles/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        string path = dir + "MansionProfile.asset";
        // 既存プロファイルを削除して再作成
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(profile, path);

        // Bloom：炎・燭台の光が滲む
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.7f);
        bloom.intensity.Override(0.8f);
        bloom.scatter.Override(0.75f);
        bloom.tint.Override(new Color(1f, 0.82f, 0.55f));

        // Vignette：画面端が暗くなり没入感を演出
        var vignette = profile.Add<Vignette>(true);
        vignette.color.Override(Color.black);
        vignette.intensity.Override(0.52f);
        vignette.smoothness.Override(0.4f);
        vignette.rounded.Override(true);

        // Color Adjustments：暖色アンバー調整
        var ca = profile.Add<ColorAdjustments>(true);
        ca.postExposure.Override(-0.25f);
        ca.contrast.Override(22f);
        ca.colorFilter.Override(new Color(1.0f, 0.87f, 0.70f));
        ca.saturation.Override(-12f);

        // Tonemapping：ACES で映画的な階調
        var tm = profile.Add<Tonemapping>(true);
        tm.mode.Override(TonemappingMode.ACES);

        // Lift Gamma Gain：シャドウを暗く、ハイライトを暖色に
        var lgg = profile.Add<LiftGammaGain>(true);
        lgg.lift.Override(new Vector4(0.98f, 0.95f, 0.90f, -0.03f));   // シャドウ冷色
        lgg.gamma.Override(new Vector4(1.0f, 0.97f, 0.94f, 0f));
        lgg.gain.Override(new Vector4(1.02f, 0.98f, 0.90f, 0.05f));    // ハイライト暖色

        volume.profile = profile;
        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(go);
        Debug.Log("[PostFX] ボリューム設定完了");
    }

    private static void EnableCameraPostProcess()
    {
        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[PostFX] Main Camera が見つかりません"); return; }

        var camData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null) camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[PostFX] カメラ PostProcessing 有効化");
    }
}
#endif
