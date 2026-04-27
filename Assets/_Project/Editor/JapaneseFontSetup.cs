#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using System.IO;

public class JapaneseFontSetup
{
    private const string FontAssetPath = "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset";
    private const string SystemFontPath1 = "/System/Library/Fonts/ヒラギノ角ゴシック W3.ttc";
    private const string SystemFontPath2 = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf";
    private const string FontCopyPath   = "Assets/_Project/Fonts/JapaneseFont.ttf";

    [MenuItem("EscapeGame/Setup/Create Japanese Font Asset")]
    public static void CreateJapaneseFontAsset()
    {
        Directory.CreateDirectory("Assets/_Project/Fonts");

        // システムフォントをプロジェクトへコピー
        string srcPath = File.Exists(SystemFontPath1) ? SystemFontPath1 : SystemFontPath2;
        if (!File.Exists(srcPath))
        {
            Debug.LogError("[FontSetup] 日本語フォントが見つかりません: " + srcPath);
            return;
        }

        // .ttcはTMPが直接読めないため .ttf として扱う（ヒラギノは実質TTF互換）
        string ext = Path.GetExtension(srcPath).ToLower() == ".ttc" ? ".ttc" : ".ttf";
        string destPath = "Assets/_Project/Fonts/JapaneseFont" + ext;
        File.Copy(srcPath, destPath, true);
        AssetDatabase.Refresh();

        var font = AssetDatabase.LoadAssetAtPath<Font>(destPath);
        if (font == null) { Debug.LogError("[FontSetup] フォントロード失敗: " + destPath); return; }

        // Dynamic TMP Font Asset を作成
        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        fontAsset.name = "JapaneseFont_Dynamic";
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FontSetup] Dynamic Font Asset 作成完了: " + FontAssetPath);
        ApplyFontToAllTMP(fontAsset);
    }

    [MenuItem("EscapeGame/Setup/Apply Japanese Font to All TMP")]
    public static void ApplyFontToAllTMP(TMP_FontAsset fontAsset = null)
    {
        if (fontAsset == null)
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null) { Debug.LogError("[FontSetup] Font Asset が見つかりません。先に Create Japanese Font Asset を実行してください"); return; }

        int count = 0;
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            tmp.font = fontAsset;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FontSetup] {count} 個の TextMeshProUGUI にフォント適用完了");
    }
}
#endif
