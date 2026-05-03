#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using System.IO;

public class JapaneseFontSetup
{
    private const string FontAssetPath = "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset";
    // Arial Unicode は純粋なTTFで日本語全文字対応 → TTC互換性問題を回避
    private const string SystemFontPath1 = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf";
    private const string SystemFontPath2 = "/System/Library/Fonts/ヒラギノ角ゴシック W3.ttc";

    // ゲーム内で使用する全文字（ノート・ヒント・UI含む）
    private const string AllGameChars =
        // ひらがな全文字
        "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
        "ぁぃぅぇぉっゃゅょ" +
        "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ" +
        // カタカナ全文字（長音符ーを含む）
        "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
        "ァィゥェォッャュョ" +
        "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヴ" +
        "ー" +
        // 数字・英字
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
        // 記号
        "（）：・！─「」、。　" +
        // 漢字（全ノート・ヒント・UI文字を網羅）
        "嵐洋館脱出成功見回消去続特探壁竣工繋閉決解" +
        "一二三四五六七八九十百千" +
        "以下上中並位何人仕代付作使入内出切前列刻印受古台号" +
        "味冊写冠剣叡儀奇契威守家序座建式引形必押掟揃描換" +
        "明星智炎灯点燭物王現理生真確示祖系約紙置菱薇薔謎" +
        "護走輪認説読金間電順食開隣封壇変図庫章端管" +
        "部屋本棚机暖炉近移動画手掛数字暗証番号力調必要品救助呼" +
        "電話機内基板修理時計指青赤緑色付桁観察錠選択肖像" +
        "年先生鍵様意味写真左走書壊器" +
        "紋様個記録薔薇十字星菱形祭壇封印三冊" +
        "正替絵権注消火" +
        "ローズクロススターダイヤ";

    [MenuItem("EscapeGame/Setup/Create Japanese Font Asset")]
    public static void CreateJapaneseFontAsset()
    {
        Directory.CreateDirectory("Assets/_Project/Fonts");

        // Arial Unicode.ttf を優先（純粋TTF、TTC互換性問題なし）
        string srcPath = File.Exists(SystemFontPath1) ? SystemFontPath1 : SystemFontPath2;
        if (!File.Exists(srcPath))
        {
            Debug.LogError("[FontSetup] 日本語フォントが見つかりません: " + srcPath);
            return;
        }

        string ext = Path.GetExtension(srcPath).ToLower();
        string destPath = "Assets/_Project/Fonts/JapaneseFont" + ext;
        File.Copy(srcPath, destPath, true);
        AssetDatabase.Refresh();

        var font = AssetDatabase.LoadAssetAtPath<Font>(destPath);
        if (font == null) { Debug.LogError("[FontSetup] フォントロード失敗: " + destPath); return; }

        if (File.Exists(FontAssetPath))
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
            AssetDatabase.Refresh();
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic);
        fontAsset.name = "NotoSansJP_Dynamic";
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // ビルド時にアトラスデータを消去しない（事前焼き込みを保持）
        var so = new SerializedObject(fontAsset);
        var clearProp = so.FindProperty("m_ClearDynamicDataOnBuild");
        if (clearProp != null) { clearProp.boolValue = false; so.ApplyModifiedPropertiesWithoutUndo(); }

        // 全文字を事前にアトラスへ焼き込む（ランタイムでの動的生成に依存しない）
        if (fontAsset.TryAddCharacters(AllGameChars, out string missing))
        {
            Debug.Log("[FontSetup] 全文字アトラス焼き込み完了");
        }
        else
        {
            Debug.LogWarning("[FontSetup] 一部文字が未対応: " + (missing ?? "不明"));
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FontSetup] Font Asset 作成完了: " + FontAssetPath);
        ApplyFontToAllTMP(fontAsset);
    }

    [MenuItem("EscapeGame/Setup/Apply Japanese Font to All TMP")]
    public static void ApplyFontToAllTMPMenu()
    {
        ApplyFontToAllTMP(null);
    }

    public static void ApplyFontToAllTMP(TMP_FontAsset fontAsset = null)
    {
        if (fontAsset == null)
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null) { Debug.LogError("[FontSetup] Font Asset が見つかりません。先に Create Japanese Font Asset を実行してください"); return; }

        int count = 0;
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
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
